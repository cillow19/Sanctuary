using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSendAIInfo@@"
// Tells the client which registered slot is bot-controlled (used to fill out a match that
// doesn't have enough human players) and how strong that bot should appear to play.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 24).
public class SoccerPacketSendAIInfo : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSendAIInfo>
{
    public new const short OpCode = 24;

    public ulong Guid;

    public bool IsAI;
    public int Difficulty;

    public SoccerPacketSendAIInfo() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        writer.Write(IsAI);
        writer.Write(Difficulty);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSendAIInfo value)
    {
        value = new SoccerPacketSendAIInfo();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Guid))
            return false;

        if (!reader.TryRead(out value.IsAI))
            return false;

        if (!reader.TryRead(out value.Difficulty))
            return false;

        return reader.RemainingLength == 0;
    }
}
