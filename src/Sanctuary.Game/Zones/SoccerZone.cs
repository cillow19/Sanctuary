using System;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions.Zones;
using Sanctuary.Packet;

namespace Sanctuary.Game.Zones;

// Minimal test-harness zone: just enough to let a character actually zone into "bw_soccer" so
// the client's SoccerProcessor gets constructed and its real network opcode can be observed
// (via live capture/debugger) - see BaseSoccerPacket.cs. No gameplay (teams/ball/scoring) yet.
public sealed class SoccerZone : BaseZone
{
    public SoccerZone(SoccerZoneDefinition zoneDefinition, IServiceProvider serviceProvider)
        : base(zoneDefinition, serviceProvider)
    {
    }

    // Without these, the client sits in an infinite loading screen: WaitForWorldReady loops
    // forever waiting on "InitialZoneDataComplete"/"ReceivedPreloadDonePacket" (confirmed via
    // Ghidra), which StartingZone.OnClientIsReady satisfies but the base BaseZone no-op does
    // not - and that gate has nothing to do with the soccer-processor branch further down.
    public override void OnClientIsReady(Player player)
    {
        player.SendTunneled(new PacketZoneDoneSendingInitialData());
        player.SendTunneled(new ClientUpdatePacketDoneSendingPreloadCharacters());
    }
}
