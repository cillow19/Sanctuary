using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketAcquireBall@@"
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 11).
public class SoccerPacketAcquireBall : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketAcquireBall>
{
    public new const short OpCode = 11;

    public ulong PlayerGuid;

    public SoccerPacketAcquireBall() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(PlayerGuid);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketAcquireBall value)
    {
        value = new SoccerPacketAcquireBall();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.PlayerGuid))
            return false;

        return reader.RemainingLength == 0;
    }
}
