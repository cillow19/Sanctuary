using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdatePlayerPos@@"
// Mirrors PlayerUpdatePacketUpdatePosition, but scoped to the soccer opcode family so it can be
// exchanged while the player's SoccerProcessor session is active on the field.
public class SoccerPacketUpdatePlayerPos : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketUpdatePlayerPos>
{
    public new const short OpCode = 7;

    public ulong Guid;

    public Vector4 Position;
    public Quaternion Rotation;

    public SoccerPacketUpdatePlayerPos() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        writer.Write(Position, true);
        writer.Write(Rotation, true);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketUpdatePlayerPos value)
    {
        value = new SoccerPacketUpdatePlayerPos();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.Position, true))
            return false;

        if (!reader.TryRead(out value.Rotation, true))
            return false;

        return reader.RemainingLength == 0;
    }
}
