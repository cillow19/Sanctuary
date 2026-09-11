using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Database;
using Sanctuary.Game;
using Sanctuary.Game.Entities;
using Sanctuary.Gateway.Helpers.Abilities;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;
using Sanctuary.Packet.Common.Attributes;

using SQLitePCL;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class AbilityPacketClientRequestStartAbilityHandler
{
    private static ILogger _logger = null!;
    private static IResourceManager _resourceManager = null!;

    // Tried in order; first match handles it. The default matches anything, so it goes last.
    private static ConsumableAbility[] _consumableAbilities = [];

    // Built at startup from ClientItemDefinitions: ActivatableAbilityId -> CompositeEffectId
    private static IResourceManager _resourceManager = null!;
    private static IDbContextFactory<DatabaseContext> _dbContextFactory = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        _logger = loggerFactory.CreateLogger(nameof(AbilityPacketClientRequestStartAbilityHandler));
        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();

        var abilityServices = new AbilityServices(
            _logger,
            _resourceManager,
            serviceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>());

        _consumableAbilities =
        [
            new BoomboxAbility(abilityServices),
            new CakeAbility(abilityServices),
            new SillyStringAbility(abilityServices),
            new TransformFoodAbility(abilityServices),
            new FoodEffectAbility(abilityServices),
            new DefaultConsumableAbility(abilityServices),
        ];
    }

    private static void HandleActionBarUpdate(GatewayConnection connection, ClientItem item, bool isDeleted)
    {
        Dictionary<int, int> actionBarSlots = connection.Player.ActionBarSlots;
        int? foundSlot = actionBarSlots
            .Where(slot => slot.Value == item.Id)
            .Select(slot => (int?)slot.Key)
            .SingleOrDefault();

        if (foundSlot == null)
        {
            _logger.LogWarning("Failed to find action bar slot for item {id}.", item.Id);
            return;
        }

        int slotWithItem = foundSlot.Value;
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
            --dbItem.Count;
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

            
        } else
        {
            item.Count = dbItem.Count;

            connection.SendTunneled(new ClientUpdatePacketItemUpdate
            {
                ItemGuid = item.Id,
                Count = item.Count
            });
        }

        // then update action bar
        HandleActionBarUpdate(connection, item, itemRemoved);
    }

    private static Player GetNearestPlayer(GatewayConnection connection) {
        const float MaxNearestPlayerDistanceMeters = 30f * 0.3048f; 
        Vector4 playerPosition = connection.Player.Position;
        var nearestPlayer = connection.Player.VisiblePlayers.Values
            .Where(p => Vector4.Distance(p.Position, playerPosition) <= MaxNearestPlayerDistanceMeters)
            .MinBy(p => Vector4.Distance(p.Position, playerPosition));
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

        if (packet.Data.Id == ConsumableAbility.ActionBarId)
            return HandleItemAbility(connection.Player, packet);

        return ConsumableAbility.SendFailure(connection.Player);
    }

    private static bool HandleItemAbility(Player player, AbilityPacketClientRequestStartAbility packet)
    {
        player.ActionBars.TryGetValue(ConsumableAbility.ActionBarId, out var actionBar);

        if (actionBar is null || !actionBar.Slots.TryGetValue(packet.Data.Slot, out var slot) || slot.IsEmpty)
            return ConsumableAbility.SendFailure(player);

        if (!player.ActionBarItemGuids.TryGetValue(ConsumableAbility.ActionBarId, out var slotItemGuids) ||
            !slotItemGuids.TryGetValue(packet.Data.Slot, out var itemGuid))
            return ConsumableAbility.SendFailure(player);

        var clientItem = player.Items.FirstOrDefault(x => x.Id == itemGuid);

        if (clientItem is null)
            return ConsumableAbility.SendFailure(player);

        if (!_resourceManager.ClientItemDefinitions.TryGetValue(clientItem.Definition, out var itemDefinition) ||
            itemDefinition.ActivatableAbilityId == 0)
            return ConsumableAbility.SendFailure(player);

        foreach (var ability in _consumableAbilities)
        {
            if (ability.Matches(itemDefinition))
                return ability.HandleAbility(player, packet, packet.Data.Slot, clientItem, itemDefinition);
        }

        return ConsumableAbility.SendFailure(player);
    }
}
