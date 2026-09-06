using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI in FreeRealms.exe: ".?AUSoccerPacketRegisterPlayer@@"
// Registers a participant's actor with the client's SoccerProcessor so it can be spawned on
// the field. Sent for every player (and AI-filled slot) taking part in the match.
public class SoccerPacketRegisterPlayer : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketRegisterPlayer>
{
    public new const short OpCode = 1;

    public ulong Guid;
    public string Name = null!;

    public int TeamId;

    public bool IsGoalie;
    public bool IsLocalPlayer;

    public SoccerPacketRegisterPlayer() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);
        writer.Write(Name);

        writer.Write(TeamId);

        writer.Write(IsGoalie);
        writer.Write(IsLocalPlayer);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketRegisterPlayer value)
    {
        value = new SoccerPacketRegisterPlayer();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.Name))
            return false;

        if (!reader.TryRead(out value.TeamId))
            return false;

        if (!reader.TryRead(out value.IsGoalie))
            return false;

        if (!reader.TryRead(out value.IsLocalPlayer))
            return false;

        return reader.RemainingLength == 0;
    }
}
