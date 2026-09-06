using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketAcquirePickUp@@"
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 10).
public class SoccerPacketAcquirePickUp : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketAcquirePickUp>
{
    public new const short OpCode = 10;

    public int PickupGuid;
    public ulong PlayerGuid;

    public SoccerPacketAcquirePickUp() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(PickupGuid);
        writer.Write(PlayerGuid);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketAcquirePickUp value)
    {
        value = new SoccerPacketAcquirePickUp();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.PickupGuid))
            return false;

        if (!reader.TryRead(out value.PlayerGuid))
            return false;

        return reader.RemainingLength == 0;
    }
}
