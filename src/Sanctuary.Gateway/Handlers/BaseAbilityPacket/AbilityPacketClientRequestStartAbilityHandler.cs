using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.Helpers;
using Sanctuary.Database;
using Sanctuary.Game;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;
using Sanctuary.Packet.Common.Attributes;

using SQLitePCL;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class AbilityPacketClientRequestStartAbilityHandler
{
    private static ILogger _logger = null!;

    // Built at startup from ClientItemDefinitions: ActivatableAbilityId -> CompositeEffectId
    private static IResourceManager _resourceManager = null!;
    private static IDbContextFactory<DatabaseContext> _dbContextFactory = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(AbilityPacketClientRequestStartAbilityHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
    }

    private static void HandleActionBarUpdate(GatewayConnection connection, ClientItem item, bool isDeleted)
    {
        Dictionary<int, int> actionBarSlots = connection.Player.ActionBarSlots;
        var slotWithItem = actionBarSlots
            .Where(slot => slot.Value == item.Id)
            .Select(slot => slot.Key)
            .SingleOrDefault();

        if (slotWithItem == default)
        {
            _logger.LogWarning("Failed to find action bar slot for item {id}.", item.Id);
            return;
        }

        int clientActionBarId = 2;
        ClientUpdatePacketUpdateActionBarSlot packet = new ClientUpdatePacketUpdateActionBarSlot
        {
            Data = 
            {
                Id = clientActionBarId,
                Slot = slotWithItem,
            },
        };

        if (isDeleted)
        {
            actionBarSlots.Remove(slotWithItem);
            packet.Slot.IsEmpty = true;
            connection.SendTunneled(packet);
            return;
        }

        if (!_resourceManager.ClientItemDefinitions.TryGetValue(item.Definition, out var clientItemDefinition))
        {
            _logger.LogWarning("Failed to find client item definition for item {id}.", item.Id);
            return;
        }

        packet.Slot.IsEmpty = false;
        packet.Slot.IconId = clientItemDefinition.Icon.Id;
        packet.Slot.NameId = clientItemDefinition.NameId;
        packet.Slot.Unknown5 = 1;
        packet.Slot.Unknown6 = 4;
        packet.Slot.Unknown7 = 15;
        packet.Slot.Enabled = true;
        packet.Slot.Unknown10 = 1000;
        packet.Slot.TotalRefreshTime = 1000;
        packet.Slot.Quantity = item.Count;
        packet.Slot.ForceDismount = true;
        packet.Slot.Unknown15 = 1000;

        connection.SendTunneled(packet);
    }

    private static void DecrementItem(GatewayConnection connection, ClientItem item)
    {
        using DatabaseContext dbContext = _dbContextFactory.CreateDbContext();

        ulong playerId = GuidHelper.GetPlayerId(connection.Player.Guid);
        var dbItem = dbContext.Characters
            .Where(x => x.Id == playerId)
            .SelectMany(x => x.Items)
            .SingleOrDefault(x => x.Id == item.Id);

        if (dbItem == null)
        {
            _logger.LogWarning("Failed to find database item {id} to consume.", item.Id);
            return;
        }

        // if item count is 0, remove from player items db
        bool itemRemoved = false;
        if (item.Count <= 1)
        {
            dbContext.Items.Remove(dbItem);
            itemRemoved = true;
        } else
        {
            dbItem.Count -= 1;
        }

        int saveStatus = dbContext.SaveChanges();
        if (saveStatus <= 0)
        {
            _logger.LogWarning("Failed to save item consumption for item {id}.", item.Id);
            return;
        }

        if (itemRemoved)
        {
            connection.Player.Items.Remove(item);
            connection.SendTunneled(new ClientUpdatePacketItemDelete
            {
                ItemGuid = item.Id
            });

            // then update action bar
            HandleActionBarUpdate(connection, item, itemRemoved);
        } else
        {
            item.Count = dbItem.Count;

            connection.SendTunneled(new ClientUpdatePacketItemUpdate
            {
                ItemGuid = item.Id,
                Count = item.Count
            });
        }
    }

    private static Player GetNearestPlayer(GatewayConnection connection) {
        Vector4 playerPosition = connection.Player.Position;
        var nearestPlayer = connection.Player.VisiblePlayers.Values.MinBy(p => Vector4.Distance(p.Position, playerPosition));
        if (nearestPlayer is not null)
        {
            _logger.LogTrace("Found another player");
            return nearestPlayer;
        }
        _logger.LogTrace("No visible players found for player {guid}, using self as nearest.", connection.Player.Guid);
        return connection.Player; 
    }


    private static PlayerUpdatePacketPlayCompositeEffect HandlePartyAbility(GatewayConnection connection, PartyAbilityDefinition partyAbilityDefinition)
    {
        ulong originPlayerGuid = connection.Player.Guid;

        int compositeEffectId = partyAbilityDefinition.CompositeEffectId;
        if (partyAbilityDefinition.HasTarget)
        {
            Player nearestPlayer = GetNearestPlayer(connection);
            return new PlayerUpdatePacketPlayCompositeEffect
            {
                TargetPlayerGuid = nearestPlayer.Guid,
                OriginPlayerGuid = originPlayerGuid,
                CompositeEffectId = compositeEffectId,
                EffectDelay = 0,
                Clear = false
            };
        }
        return new PlayerUpdatePacketPlayCompositeEffect
        {
            TargetPlayerGuid = originPlayerGuid,
            OriginPlayerGuid = originPlayerGuid,
            CompositeEffectId = compositeEffectId,
            EffectDelay = 0,
            Clear = false
        };   
    }


    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {

        if (!AbilityPacketClientRequestStartAbility.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(AbilityPacketClientRequestStartAbility));
            return false;
        }

        _logger.LogTrace("Received {name} packet. ( {packet} )", nameof(AbilityPacketClientRequestStartAbility), packet);
         
        if (connection.Player.ActionBarSlots.TryGetValue(packet.Data.Slot, out var itemGuid))
        {
            _logger.LogTrace("Slot {slot} -> Item {guid}", packet.Data.Slot, itemGuid);
            var item = connection.Player.Items.SingleOrDefault(x => x.Id == itemGuid);
            if (item != null && _resourceManager.Abilities.TryGetValue(item.Definition, out var abilityDefinition))
            {
                PlayerUpdatePacketPlayCompositeEffect? effect = null;

                switch (abilityDefinition)
                {
                    case PartyAbilityDefinition partyAbility:
                        _logger.LogTrace("Slot {slot} -> Item {guid} -> PartyAbility {abilityId}", packet.Data.Slot, itemGuid, partyAbility.AbilityId);
                        effect = HandlePartyAbility(connection, partyAbility);
                        break;

                    //case OtherAbility miscAbility:
                    //    _logger.LogTrace("Slot {slot} -> Item {guid} -> Misc
                    // ...

                    default:
                        _logger.LogWarning("Slot {slot} -> Item {guid} -> Unknown ability type {type}", packet.Data.Slot, itemGuid, abilityDefinition.GetType().Name);
                        break;
                }

                if (effect != null)
                {
                    DecrementItem(connection, item);
                    connection.Player.SendTunneledToVisible(effect, sendToSelf: true);
                    return true;
                }
            } 
        }
        else
        {
            _logger.LogTrace("No action bar slot mapping for slot {slot}", packet.Data.Slot);
        }

        var abilityPacketFailed = new AbilityPacketFailed
        {
            // You can't use that ability right now.
            StringId = 3079
        };

        connection.SendTunneled(abilityPacketFailed);

        return true;
    }
}