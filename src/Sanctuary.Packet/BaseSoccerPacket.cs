using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class BaseSoccerPacket
{
    // CONFIRMED (disassembly, not a guess): every concrete SoccerPacket* constructor calls a
    // single shared base constructor (client FUN_00b730a0) passing its own confirmed SubOpCode
    // as a literal - cross-checked against SoccerPacketUpdatePlayerPos (passes 1) and
    // SoccerPacketGoalMade (passes 2), each then setting its own already-known vtable address.
    // That shared constructor itself hardcodes "MOV [this+4], 0x4C" before either call sets its
    // SubOpCode - i.e. the real client literally writes 76 as BaseSoccerPacket's own opcode.
    // Doesn't collide with any opcode already confirmed elsewhere in this project.
    public const short OpCode = 76;

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
