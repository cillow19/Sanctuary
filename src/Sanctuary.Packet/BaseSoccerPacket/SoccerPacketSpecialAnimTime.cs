using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSpecialAnimTime@@"
// Synchronizes the duration of a one-off scripted animation across clients (goal celebration,
// disappointed reaction, goalie throw, ...) so everyone's timeline lines up.
public class SoccerPacketSpecialAnimTime : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSpecialAnimTime>
{
    public new const short OpCode = 23;

    public ulong Guid;
    public float DurationSeconds;

    public SoccerPacketSpecialAnimTime() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);
        writer.Write(DurationSeconds);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSpecialAnimTime value)
    {
        value = new SoccerPacketSpecialAnimTime();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.DurationSeconds))
            return false;

        return reader.RemainingLength == 0;
    }
}
