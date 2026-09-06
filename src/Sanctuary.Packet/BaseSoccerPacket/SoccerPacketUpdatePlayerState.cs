using System;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdatePlayerState@@"
// Syncs which action/animation state a player (or goalie) is in - see SoccerPlayerAnimState,
// whose names are confirmed from the client's cSoccerPlayerState* / cSoccerGoalieState*
// string table (e.g. RunWithBall, SlideTackleStart, SuperKick, GoalieCatchHigh, ...).
public class SoccerPacketUpdatePlayerState : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdatePlayerState>
{
    public new const short OpCode = 9;

    public ulong Guid;
    public SoccerPlayerAnimState State;

    public SoccerPacketUpdatePlayerState() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);
        writer.Write((int)State);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdatePlayerState value)
    {
        value = new SoccerPacketUpdatePlayerState();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out int state))
            return false;

        value.State = (SoccerPlayerAnimState)state;

        return reader.RemainingLength == 0;
    }
}
