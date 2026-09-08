using System;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdateGameState@@"
// Drives the match state machine (see SoccerGameState - values confirmed from the client's
// state-id -> name lookup function, e.g. InitializeGame, KickOff, PlayingSoccer, Halftime,
// GoalCelebration, SuperShot, Winner, ...).
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 5).
// Size/shape CONFIRMED (disassembly of the real deserializer, client FUN_00b79a60): 14 bare
// 4-byte fields, then one 8-byte value (read as a single unit - likely a Guid), then 2 more
// 4-byte fields - 72 bytes total, not the 8-byte State+StateTimeMs originally guessed. This is
// clearly a full scoreboard/state snapshot, not a bare state change - individual field meanings
// beyond State (the first one) are NOT confirmed.
public class SoccerPacketUpdateGameState : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdateGameState>
{
    public new const short OpCode = 5;

    public SoccerGameState State;

    public int Unknown2;
    public int Unknown3;
    public int Unknown4;
    public int Unknown5;
    public int Unknown6;
    public int Unknown7;
    public int Unknown8;
    public int Unknown9;
    public int Unknown10;
    public int Unknown11;
    public int Unknown12;
    public int Unknown13;
    public int Unknown14;

    // The one 8-byte field read as a single unit in the real deserializer - a Guid is the most
    // plausible read (e.g. "player who last touched the ball"), but that's not confirmed either.
    public ulong Unknown15;

    public int Unknown16;
    public int Unknown17;

    public SoccerPacketUpdateGameState() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write((int)State);
        writer.Write(Unknown2);
        writer.Write(Unknown3);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
        writer.Write(Unknown6);
        writer.Write(Unknown7);
        writer.Write(Unknown8);
        writer.Write(Unknown9);
        writer.Write(Unknown10);
        writer.Write(Unknown11);
        writer.Write(Unknown12);
        writer.Write(Unknown13);
        writer.Write(Unknown14);
        writer.Write(Unknown15);
        writer.Write(Unknown16);
        writer.Write(Unknown17);

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

        if (!reader.TryRead(out value.Unknown2))
            return false;

        if (!reader.TryRead(out value.Unknown3))
            return false;

        if (!reader.TryRead(out value.Unknown4))
            return false;

        if (!reader.TryRead(out value.Unknown5))
            return false;

        if (!reader.TryRead(out value.Unknown6))
            return false;

        if (!reader.TryRead(out value.Unknown7))
            return false;

        if (!reader.TryRead(out value.Unknown8))
            return false;

        if (!reader.TryRead(out value.Unknown9))
            return false;

        if (!reader.TryRead(out value.Unknown10))
            return false;

        if (!reader.TryRead(out value.Unknown11))
            return false;

        if (!reader.TryRead(out value.Unknown12))
            return false;

        if (!reader.TryRead(out value.Unknown13))
            return false;

        if (!reader.TryRead(out value.Unknown14))
            return false;

        if (!reader.TryRead(out value.Unknown15))
            return false;

        if (!reader.TryRead(out value.Unknown16))
            return false;

        if (!reader.TryRead(out value.Unknown17))
            return false;

        return reader.RemainingLength == 0;
    }
}
