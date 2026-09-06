using System;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdateGameState@@"
// Drives the match state machine (see SoccerGameState - values confirmed from the client's
// state-id -> name lookup function, e.g. InitializeGame, KickOff, PlayingSoccer, Halftime,
// GoalCelebration, SuperShot, Winner, ...).
public class SoccerPacketUpdateGameState : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdateGameState>
{
    public new const short OpCode = 5;

    public SoccerGameState State;

    // How long (ms) the client should expect to stay in this state, e.g. the KickOff countdown
    // or the GoalCelebration duration. 0 when not applicable.
    public int StateTimeMs;

    public SoccerPacketUpdateGameState() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write((int)State);
        writer.Write(StateTimeMs);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdateGameState value)
    {
        value = new SoccerPacketUpdateGameState();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out int state))
            return false;

        value.State = (SoccerGameState)state;

        if (!reader.TryRead(out value.StateTimeMs))
            return false;

        return reader.RemainingLength == 0;
    }
}
