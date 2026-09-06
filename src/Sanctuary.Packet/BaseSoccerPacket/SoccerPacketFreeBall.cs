using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketFreeBall@@"
// Sent when the ball leaves a player's possession (tackled off, passed and not yet caught,
// knocked loose, ...) so it becomes independently simulated again.
public class SoccerPacketFreeBall : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketFreeBall>
{
    public new const short OpCode = 16;

    public Vector4 Position;
    public Vector4 Velocity;

    public SoccerPacketFreeBall() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Position, true);
        writer.Write(Velocity, true);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketFreeBall value)
    {
        value = new SoccerPacketFreeBall();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Position, true))
            return false;

        if (!reader.TryRead(out value.Velocity, true))
            return false;

        return reader.RemainingLength == 0;
    }
}
