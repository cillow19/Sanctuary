using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class BaseSoccerPacket
{
    // TODO: CONFIRM. Ghidra confirms FreeRealms.exe has a distinct "BaseSoccerPacket" RTTI type
    // (separate from the generic BaseMiniGamePacket used for turn-based games like Chess/TCG),
    // but the real top-level opcode value could not be recovered from static analysis alone.
    // Placed outside the 1-211 range already used/verified elsewhere in this project so it can't
    // collide with a real opcode. Replace with the confirmed value (e.g. from a packet capture)
    // before wiring this into the tunnel handlers for real traffic.
    public const short OpCode = 220;

    // The 24 concrete SoccerPacket* SubOpCode values below (see each file) ARE confirmed for
    // real: traced from the client's SoccerProcessor message-dispatch switch (FUN_00b80ac0),
    // matched 1:1 against each concrete class's own constructor via its RTTI vtable - no
    // guessing involved. That switch runs 1-25 with 20 skipped entirely, i.e. there is no
    // 25th message type; the client's real protocol has exactly these 24.

    private short SubOpCode;

    public BaseSoccerPacket(short subOpCode)
    {
        SubOpCode = subOpCode;
    }

    public virtual void Write(PacketWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(SubOpCode);
    }

    public bool TryRead(ref PacketReader reader)
    {
        if (!reader.TryRead(out short opCode) && opCode != OpCode)
            return false;

        if (!reader.TryRead(out short subOpCode) && subOpCode != SubOpCode)
            return false;

        return true;
    }
}
