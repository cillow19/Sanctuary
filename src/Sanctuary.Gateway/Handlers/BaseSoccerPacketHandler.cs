using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.IO;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class BaseSoccerPacketHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(BaseSoccerPacketHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, PacketReader reader)
    {
        if (!reader.TryRead(out short opCode))
        {
            _logger.LogError("Failed to read opcode from packet. ( Data: {data} )", Convert.ToHexString(reader.Span));
            return false;
        }

        return opCode switch
        {
            SoccerPacketUpdatePlayerPos.OpCode => SoccerPacketUpdatePlayerPosHandler.HandlePacket(connection, reader.Span),
            SoccerPacketUpdatePlayerControls.OpCode => SoccerPacketUpdatePlayerControlsHandler.HandlePacket(connection, reader.Span),
            SoccerPacketAcquireBall.OpCode => SoccerPacketAcquireBallHandler.HandlePacket(connection, reader.Span),
            SoccerPacketAcquirePickUp.OpCode => SoccerPacketAcquirePickUpHandler.HandlePacket(connection, reader.Span),

            // The remaining SoccerPacket* types (RegisterPlayer, SetClientConfig, SetPlayerTeam,
            // TeamColours, UpdateGameState, UpdateGameTime, UpdatePlayerState, UpdatePlayerStats,
            // PlayerVariableStats, SetSoccerPlayerOneTimeStats, StatRollInfo, UpdateSoccerBall,
            // FreeBall, ImpulseBall, HighArcBall, BallBounce, SpawnPickUp, GoalMade,
            // SpecialAnimTime, SendAIInfo) are server -> client only today, so there's nothing
            // for the gateway to receive for them yet.
            _ => true // soccer not implemented
        };
    }
}
