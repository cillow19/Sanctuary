using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class SoccerPacketAcquireBallHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(SoccerPacketAcquireBallHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!SoccerPacketAcquireBall.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(SoccerPacketAcquireBall));
            return false;
        }

        // TODO: validate the claim server-side (proximity/possession rules) and, if accepted,
        // broadcast SoccerPacketUpdateSoccerBall with OwnerGuid = packet.PlayerGuid.

        return true;
    }
}
