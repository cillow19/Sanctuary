using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketPlayerVariableStats@@"
// Per-match running totals shown on the in-match scoreboard/HUD (SoccerScores/SoccerResults),
// as opposed to the persistent CharacterStat pool carried by SoccerPacketUpdatePlayerStats.
public class SoccerPacketPlayerVariableStats : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketPlayerVariableStats>
{
    public new const short OpCode = 11;

    public ulong Guid;

    public int Goals;
    public int Assists;
    public int Tackles;
    public int Saves;
    public int SuperKicksUsed;

    public SoccerPacketPlayerVariableStats() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        writer.Write(Goals);
        writer.Write(Assists);
        writer.Write(Tackles);
        writer.Write(Saves);
        writer.Write(SuperKicksUsed);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketPlayerVariableStats value)
    {
        value = new SoccerPacketPlayerVariableStats();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.Goals))
            return false;

        if (!reader.TryRead(out value.Assists))
            return false;

        if (!reader.TryRead(out value.Tackles))
            return false;

        if (!reader.TryRead(out value.Saves))
            return false;

        if (!reader.TryRead(out value.SuperKicksUsed))
            return false;

        return reader.RemainingLength == 0;
    }
}
