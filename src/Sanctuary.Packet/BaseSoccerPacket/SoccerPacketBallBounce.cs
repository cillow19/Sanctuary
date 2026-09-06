using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketBallBounce@@"
// Fires the MG_Soccer_Ball_Bounce effect/sound at the given spot for everyone in the match.
public class SoccerPacketBallBounce : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketBallBounce>
{
    public new const short OpCode = 19;

    public Vector4 Position;
    public float Intensity;

    public SoccerPacketBallBounce() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Position, true);
        writer.Write(Intensity);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketBallBounce value)
    {
        value = new SoccerPacketBallBounce();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Position, true))
            return false;

        if (!reader.TryRead(out value.Intensity))
            return false;

        return reader.RemainingLength == 0;
    }
}
