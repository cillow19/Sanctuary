using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI in FreeRealms.exe: ".?AUSoccerPacketRegisterPlayer@@"
// Registers a participant's actor with the client's SoccerProcessor so it can be spawned on
// the field. Sent for every player (and AI-filled slot) taking part in the match.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 3).
// Field layout CONFIRMED (disassembly of the real deserializer, client FUN_00b78020): just a
// Guid (read as two back-to-back 4-byte halves) followed by two bare bools - 10 bytes total.
// No name string, no team id - team assignment lives entirely in SoccerPacketSetPlayerTeam.
public class SoccerPacketRegisterPlayer : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketRegisterPlayer>
{
    public new const short OpCode = 3;

    public ulong Guid;

    // Exact meaning of these two flags is not confirmed, only that there are exactly two of
    // them here - IsGoalie/IsLocalPlayer is a plausible guess given the class's other fields
    // originally assumed them, but could be swapped or mean something else entirely.
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

        if (!reader.TryRead(out value.IsGoalie))
            return false;

        if (!reader.TryRead(out value.IsLocalPlayer))
            return false;

        return reader.RemainingLength == 0;
    }
}
