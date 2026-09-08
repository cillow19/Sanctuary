using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketTeamColours@@"
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 8).
// Field layout CONFIRMED (disassembly of the real deserializer, client FUN_00b78490): two
// length-prefixed strings (almost certainly the two team names, not colors as originally
// guessed), then four bare 4-byte ints, then one bare bool. The "colors" this class is named
// for are apparently carried as ints among the four unknowns, not as a color pair - exact
// mapping isn't confirmed.
public class SoccerPacketTeamColours : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketTeamColours>
{
    public new const short OpCode = 8;

    public string TeamAName = string.Empty;
    public string TeamBName = string.Empty;

    public int Unknown3;
    public int Unknown4;
    public int Unknown5;
    public int Unknown6;

    public bool Unknown7;

    public SoccerPacketTeamColours() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(TeamAName);
        writer.Write(TeamBName);

        writer.Write(Unknown3);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
        writer.Write(Unknown6);

        writer.Write(Unknown7);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketTeamColours value)
    {
        value = new SoccerPacketTeamColours();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.TeamAName))
            return false;

        if (!reader.TryRead(out value.TeamBName))
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

        return reader.RemainingLength == 0;
    }
}
