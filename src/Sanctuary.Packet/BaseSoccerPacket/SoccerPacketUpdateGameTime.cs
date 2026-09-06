using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdateGameTime@@"
public class SoccerPacketUpdateGameTime : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdateGameTime>
{
    public new const short OpCode = 6;

    public int Period;
    public float TimeRemainingSeconds;

    public SoccerPacketUpdateGameTime() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Period);
        writer.Write(TimeRemainingSeconds);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdateGameTime value)
    {
        value = new SoccerPacketUpdateGameTime();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Period))
            return false;

        if (!reader.TryRead(out value.TimeRemainingSeconds))
            return false;

        return reader.RemainingLength == 0;
    }
}
