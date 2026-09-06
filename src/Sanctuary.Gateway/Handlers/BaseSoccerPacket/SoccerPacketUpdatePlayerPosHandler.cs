using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class SoccerPacketUpdatePlayerPosHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(SoccerPacketUpdatePlayerPosHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!SoccerPacketUpdatePlayerPos.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(SoccerPacketUpdatePlayerPos));
            return false;
        }

        // TODO: broadcast to the other players in the same soccer match once soccer zones/
        // instances exist (see BaseZone/ZoneManager - there is currently only one, shared,
        // starting zone, so there is nowhere to route this yet).

        return true;
    }
}
