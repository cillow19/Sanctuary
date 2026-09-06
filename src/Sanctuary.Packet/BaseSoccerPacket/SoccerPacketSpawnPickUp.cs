using System;
using System.Numerics;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSpawnPickUp@@"
// Spawns one of the field pickups (see SoccerPickupType - Speed/Toughness/Charge/Knockdown/
// MultiBall/Spring, names confirmed from the client's asset manifest and audio files).
public class SoccerPacketSpawnPickUp : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSpawnPickUp>
{
    public new const short OpCode = 20;

    public int PickupGuid;
    public SoccerPickupType Type;

    public Vector4 Position;

    public SoccerPacketSpawnPickUp() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(PickupGuid);
        writer.Write((int)Type);

        writer.Write(Position, true);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSpawnPickUp value)
    {
        value = new SoccerPacketSpawnPickUp();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.PickupGuid))
            return false;

        if (!reader.TryRead(out int type))
            return false;

        value.Type = (SoccerPickupType)type;

        if (!reader.TryRead(out value.Position, true))
            return false;

        return reader.RemainingLength == 0;
    }
}
