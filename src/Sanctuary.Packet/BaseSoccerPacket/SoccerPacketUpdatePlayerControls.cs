using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdatePlayerControls@@"
// Raw client input state, sent so other clients can predict/animate this player between
// SoccerPacketUpdatePlayerPos updates (movement stick + kick charge/sprint buttons).
public class SoccerPacketUpdatePlayerControls : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdatePlayerControls>
{
    public new const short OpCode = 8;

    public ulong Guid;

    public float MoveX;
    public float MoveY;

    public bool Sprint;
    public bool KickHeld;

    public float KickChargeSeconds;

    public SoccerPacketUpdatePlayerControls() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        writer.Write(MoveX);
        writer.Write(MoveY);

        writer.Write(Sprint);
        writer.Write(KickHeld);

        writer.Write(KickChargeSeconds);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdatePlayerControls value)
    {
        value = new SoccerPacketUpdatePlayerControls();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.MoveX))
            return false;

        if (!reader.TryRead(out value.MoveY))
            return false;

        if (!reader.TryRead(out value.Sprint))
            return false;

        if (!reader.TryRead(out value.KickHeld))
            return false;

        if (!reader.TryRead(out value.KickChargeSeconds))
            return false;

        return reader.RemainingLength == 0;
    }
}
