using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketImpulseBall@@"
// A direct kick/pass impulse applied to the ball (as opposed to the arced shot handled by
// SoccerPacketHighArcBall). Covers MG_Soccer_Kick_Light/Heavy and MG_Soccer_SuperKick.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 13).
public class SoccerPacketImpulseBall : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketImpulseBall>
{
    public new const short OpCode = 13;

    public ulong KickerGuid;

    public Vector4 Impulse;

    public bool IsSuperKick;

    public SoccerPacketImpulseBall() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(KickerGuid);

        writer.Write(Impulse, true);

        writer.Write(IsSuperKick);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketImpulseBall value)
    {
        value = new SoccerPacketImpulseBall();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.KickerGuid))
            return false;

        if (!reader.TryRead(out value.Impulse, true))
            return false;

        if (!reader.TryRead(out value.IsSuperKick))
            return false;

        return reader.RemainingLength == 0;
    }
}
