using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game;
using Sanctuary.Game.Entities;
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
    private static Dictionary<int, int> _abilityEffectsDict = null!;
    private static Dictionary<int, int> BuildAbilityEffectsLookup(IResourceManager resourceManager)
    {
        Dictionary<int, int> abilityEffectsDict = resourceManager.ClientItemDefinitions.Values
            .Where(x => x.ActivatableAbilityId != 0 && x.CompositeEffectId != 0)
            .ToDictionary(x => x.ActivatableAbilityId, x => x.CompositeEffectId);
        _logger.LogTrace("Built ability effects lookup with {count} entries.", abilityEffectsDict.Count);
        return abilityEffectsDict;
    }

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(AbilityPacketClientRequestStartAbilityHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
        _abilityEffectsDict = BuildAbilityEffectsLookup(_resourceManager);
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
                && _resourceManager.ClientItemDefinitions.TryGetValue(item.Definition, out var clientItemDefinition)
                && clientItemDefinition.CompositeEffectId != 0)
            {
                _logger.LogTrace("Slot {slot} -> Item {guid} -> CompositeEffectId {effectId}", packet.Data.Slot, itemGuid, clientItemDefinition.CompositeEffectId);

                Player nearestPlayer = GetNearestPlayer(connection);
                var effect = new PlayerUpdatePacketPlayCompositeEffect
                {
                    // Guid = connection.Player.Guid,
                    // Unknown2 = nearestPlayer.Guid,
                    TargetPlayerGuid = nearestPlayer.Guid,
                    OriginPlayerGuid = connection.Player.Guid,
                    CompositeEffectId = clientItemDefinition.CompositeEffectId,
                    EffectDelay = 0,
                    // Position = nearestPlayer.Position with { Y = nearestPlayer.Position.Y + 1.0f },
                    Clear = false
                };

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