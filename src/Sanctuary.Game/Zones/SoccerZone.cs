using System;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions.Zones;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Zones;

// Test-harness zone for "bw_soccer". Gets a character into the real zone/ball assets and now
// runs a real server-simulated ball, bootstrapped through the actual BaseSoccerPacket protocol
// - see SoccerBall.cs and BaseSoccerPacket.cs for what's still unconfirmed (the top-level
// opcode) versus what's already solid (the 24 real sub-opcodes, the packet shapes).
// Still no real gameplay: no teams beyond a single placeholder, no matchmaking, no scoring.
public sealed class SoccerZone : BaseZone
{
    // "soccerball_m" - the real ball model, already registered in Resources/Models.txt. The
    // field itself (turf, goals, lines) is baked into the bw_soccer.gzne zone scene and needs
    // no separate prop.
    private const int SoccerBallModelId = 3027;

    private readonly SoccerZoneDefinition _zoneDefinition;

    public SoccerBall? Ball { get; private set; }

    public SoccerZone(SoccerZoneDefinition zoneDefinition, IServiceProvider serviceProvider)
        : base(zoneDefinition, serviceProvider)
    {
        _zoneDefinition = zoneDefinition;
    }

    public override void OnStart()
    {
        base.OnStart();

        if (TryCreateNpc(null, guid => new SoccerBall(this, _zoneDefinition.BallSpawnPosition.Y)
        {
            Guid = guid,
            ModelId = SoccerBallModelId,
            Scale = 1f,
            Visible = true
        }, out var ball))
        {
            ball.UpdatePosition(_zoneDefinition.BallSpawnPosition, _zoneDefinition.BallSpawnRotation);

            Ball = ball;
        }
    }

    // Without these, the client sits in an infinite loading screen: WaitForWorldReady loops
    // forever waiting on "InitialZoneDataComplete"/"ReceivedPreloadDonePacket" (confirmed via
    // Ghidra), which StartingZone.OnClientIsReady satisfies but the base BaseZone no-op does
    // not - and that gate has nothing to do with the soccer-processor branch further down.
    public override void OnClientIsReady(Player player)
    {
        player.SendTunneled(new PacketZoneDoneSendingInitialData());
        player.SendTunneled(new ClientUpdatePacketDoneSendingPreloadCharacters());

        SendMatchBootstrap(player);
    }

    // Minimal single-player test bootstrap through the real Soccer protocol - not real
    // matchmaking/teams. The client can only act on this once BaseSoccerPacket.OpCode is
    // confirmed; until then these are sent but never routed to anything client-side.
    private void SendMatchBootstrap(Player player)
    {
        player.SendTunneled(new SoccerPacketSetClientConfig
        {
            GameId = 1,
            MapId = 0, // Briarwood
            PlayersPerTeam = 1,
            PeriodLengthSeconds = 300,
            MaxScore = 5,
            PickupsEnabled = false,
            SuddenDeathEnabled = false
        });

        player.SendTunneled(new SoccerPacketRegisterPlayer
        {
            Guid = player.Guid,
            Name = player.Name.FullName,
            TeamId = 0,
            IsGoalie = false,
            IsLocalPlayer = true
        });

        player.SendTunneled(new SoccerPacketSetPlayerTeam
        {
            PlayerGuid = player.Guid,
            TeamId = 0
        });

        player.SendTunneled(new SoccerPacketTeamColours
        {
            TeamId = 0,
            PrimaryColor = 0x1E90FFu,
            SecondaryColor = 0xFFFFFFu
        });

        player.SendTunneled(new SoccerPacketUpdateGameState
        {
            State = SoccerGameState.PlayingSoccer,
            StateTimeMs = 0
        });
    }
}
