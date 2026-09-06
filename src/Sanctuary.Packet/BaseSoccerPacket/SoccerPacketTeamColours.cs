using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketTeamColours@@"
public class SoccerPacketTeamColours : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketTeamColours>
{
    public new const short OpCode = 4;

    public int TeamId;

    public uint PrimaryColor;
    public uint SecondaryColor;

    public SoccerPacketTeamColours() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(TeamId);

        writer.Write(PrimaryColor);
        writer.Write(SecondaryColor);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketTeamColours value)
    {
        value = new SoccerPacketTeamColours();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.TeamId))
            return false;

        if (!reader.TryRead(out value.PrimaryColor))
            return false;

        if (!reader.TryRead(out value.SecondaryColor))
            return false;

        return reader.RemainingLength == 0;
    }
}
