using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSetPlayerTeam@@"
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 6).
public class SoccerPacketSetPlayerTeam : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSetPlayerTeam>
{
    public new const short OpCode = 6;

    public ulong PlayerGuid;
    public int TeamId;

    public SoccerPacketSetPlayerTeam() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(PlayerGuid);
        writer.Write(TeamId);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSetPlayerTeam value)
    {
        value = new SoccerPacketSetPlayerTeam();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.PlayerGuid))
            return false;

        if (!reader.TryRead(out value.TeamId))
            return false;

        return reader.RemainingLength == 0;
    }
}
