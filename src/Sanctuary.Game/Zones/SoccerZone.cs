using System;

using Sanctuary.Game.Resources.Definitions.Zones;

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
}
