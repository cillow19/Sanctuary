using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdateSoccerBall@@"
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 4).
public class SoccerPacketUpdateSoccerBall : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdateSoccerBall>
{
    public new const short OpCode = 4;

    public Vector4 Position;
    public Vector4 Velocity;

    // 0 when the ball is free (not currently held by a player).
    public ulong OwnerGuid;

    public SoccerPacketUpdateSoccerBall() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Position, true);
        writer.Write(Velocity, true);

        writer.Write(OwnerGuid);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdateSoccerBall value)
    {
        value = new SoccerPacketUpdateSoccerBall();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Position, true))
            return false;

        if (!reader.TryRead(out value.Velocity, true))
            return false;

        if (!reader.TryRead(out value.OwnerGuid))
            return false;

        return reader.RemainingLength == 0;
    }
}
