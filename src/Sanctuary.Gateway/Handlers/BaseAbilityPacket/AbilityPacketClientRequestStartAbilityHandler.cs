using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

using SQLitePCL;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class AbilityPacketClientRequestStartAbilityHandler
{
    private static ILogger _logger = null!;

    // Built at startup from ClientItemDefinitions: ActivatableAbilityId -> CompositeEffectId
    private static IResourceManager _resourceManager = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(AbilityPacketClientRequestStartAbilityHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
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


    private static PlayerUpdatePacketPlayCompositeEffect BuildPacket(GatewayConnection connection, AbilityDefinition abilityDefinition)
    {
        ulong originPlayerGuid = connection.Player.Guid;
        int compositeEffectId = abilityDefinition.CompositeEffectId;
        if (abilityDefinition.HasTarget)
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
        else
        {
            return new PlayerUpdatePacketPlayCompositeEffect
            {
                TargetPlayerGuid = originPlayerGuid,
                OriginPlayerGuid = originPlayerGuid,
                CompositeEffectId = compositeEffectId,
                EffectDelay = 0,
                Clear = false
            };
        }   
        
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
            var item = connection.Player.Items.SingleOrDefault(x => x.Id == itemGuid);
            if (item != null
                && _resourceManager.Abilities.TryGetValue(item.Definition, out var abilityDefinition)
                && abilityDefinition.CompositeEffectId != 0)
            {
                _logger.LogTrace("Slot {slot} -> Item {guid} -> CompositeEffectId {effectId}", packet.Data.Slot, itemGuid, abilityDefinition.CompositeEffectId);


                Player nearestPlayer = GetNearestPlayer(connection);
                var effect = BuildPacket(connection, abilityDefinition);

                connection.Player.SendTunneledToVisible(effect, sendToSelf: true);
                return true;
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