using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketHighArcBall@@"
// A lobbed pass/shot (LobKickRunning / HighKick* states) that flies to a target point along
// an arc rather than a straight impulse.
public class SoccerPacketHighArcBall : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketHighArcBall>
{
    public new const short OpCode = 18;

    public ulong KickerGuid;

    public Vector4 TargetPosition;

    public float ArcHeight;
    public float FlightTimeSeconds;

    public SoccerPacketHighArcBall() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(KickerGuid);

        writer.Write(TargetPosition, true);

        writer.Write(ArcHeight);
        writer.Write(FlightTimeSeconds);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketHighArcBall value)
    {
        value = new SoccerPacketHighArcBall();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.KickerGuid))
            return false;

        if (!reader.TryRead(out value.TargetPosition, true))
            return false;

        if (!reader.TryRead(out value.ArcHeight))
            return false;

        if (!reader.TryRead(out value.FlightTimeSeconds))
            return false;

        return reader.RemainingLength == 0;
    }
}
