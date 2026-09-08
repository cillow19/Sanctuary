using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSetClientConfig@@"
// Sent once, right after the client's SoccerProcessor is created. This is what
// WaitForWorldReady is blocked on client-side ("WaitForWorldReady: waiting for soccer
// processor") before it will drop the loading screen for a soccer zone. Successfully parsing
// this packet is also what triggers the client to construct its own SoccerBall actor
// (confirmed: the dispatch case for sub-opcode 7 calls FUN_00cde1c0(0) right after a successful
// deserialize - the same address independently identified as SoccerBall's real constructor).
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 7).
//
// Field layout CONFIRMED for size/order/primitive-type (disassembly of the real deserializer,
// client FUN_00b78600, and its four field-reading helpers FUN_00b77fc0/FUN_008d53b0/
// FUN_008e2410/FUN_008e2470). NOT confirmed: what any individual field actually means - this is
// clearly arena/match config (field geometry, physics tuning, a config/map name string, plus a
// couple of trailing integers) but semantic mapping is a total guess. Read order, exactly as it
// appears client-side:
//   1. two bare 16-bit shorts (FUN_00b77fc0 - the only helper that reads 2-byte, not 4-byte,
//      values)
//   2. one Vector4 (FUN_008e2410 loop of 4 NaN-checked floats)
//   3. sixteen bare NaN-checked floats (offsets 0x20-0x5c in the client struct) - grouped here
//      as four Vector4s since 16 floats in a row this early is very likely 4 points/transforms
//      (e.g. field corners or goal locations), though the grouping boundaries are a guess
//   4. eleven bare NaN-checked floats (FUN_008d53b0 x11)
//   5. one length-prefixed string (FUN_008e2470 - same string-reading helper pattern used
//      elsewhere, e.g. SoccerPacketTeamColours's team names)
//   6. two bare NaN-checked floats (FUN_008d53b0 x2)
//   7. one Vector4 (FUN_008e2410 loop of 4 NaN-checked floats)
//   8. nine bare NaN-checked floats (FUN_008d53b0 x9)
//   9. two bare 32-bit ints with NO NaN check (i.e. genuinely ints, not floats stored as ints)
//  10. one final bare NaN-checked float (FUN_008d53b0)
public class SoccerPacketSetClientConfig : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSetClientConfig>
{
    public new const short OpCode = 7;

    public short Unknown1;
    public short Unknown2;

    public Vector4 UnknownVector1;

    public Vector4 UnknownVector2;
    public Vector4 UnknownVector3;
    public Vector4 UnknownVector4;
    public Vector4 UnknownVector5;

    public float Unknown19;
    public float Unknown20;
    public float Unknown21;
    public float Unknown22;
    public float Unknown23;
    public float Unknown24;
    public float Unknown25;
    public float Unknown26;
    public float Unknown27;
    public float Unknown28;
    public float Unknown29;

    public string ConfigName = string.Empty;

    public float Unknown30;
    public float Unknown31;

    public Vector4 UnknownVector6;

    public float Unknown32;
    public float Unknown33;
    public float Unknown34;
    public float Unknown35;
    public float Unknown36;
    public float Unknown37;
    public float Unknown38;
    public float Unknown39;
    public float Unknown40;

    public int Unknown41;
    public int Unknown42;

    public float Unknown43;

    public SoccerPacketSetClientConfig() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Unknown1);
        writer.Write(Unknown2);

        writer.Write(UnknownVector1);

        writer.Write(UnknownVector2);
        writer.Write(UnknownVector3);
        writer.Write(UnknownVector4);
        writer.Write(UnknownVector5);

        writer.Write(Unknown19);
        writer.Write(Unknown20);
        writer.Write(Unknown21);
        writer.Write(Unknown22);
        writer.Write(Unknown23);
        writer.Write(Unknown24);
        writer.Write(Unknown25);
        writer.Write(Unknown26);
        writer.Write(Unknown27);
        writer.Write(Unknown28);
        writer.Write(Unknown29);

        writer.Write(ConfigName);

        writer.Write(Unknown30);
        writer.Write(Unknown31);

        writer.Write(UnknownVector6);

        writer.Write(Unknown32);
        writer.Write(Unknown33);
        writer.Write(Unknown34);
        writer.Write(Unknown35);
        writer.Write(Unknown36);
        writer.Write(Unknown37);
        writer.Write(Unknown38);
        writer.Write(Unknown39);
        writer.Write(Unknown40);

        writer.Write(Unknown41);
        writer.Write(Unknown42);

        writer.Write(Unknown43);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSetClientConfig value)
    {
        value = new SoccerPacketSetClientConfig();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Unknown1))
            return false;

        if (!reader.TryRead(out value.Unknown2))
            return false;

        if (!reader.TryRead(out value.UnknownVector1))
            return false;

        if (!reader.TryRead(out value.UnknownVector2))
            return false;

        if (!reader.TryRead(out value.UnknownVector3))
            return false;

        if (!reader.TryRead(out value.UnknownVector4))
            return false;

        if (!reader.TryRead(out value.UnknownVector5))
            return false;

        if (!reader.TryRead(out value.Unknown19))
            return false;

        if (!reader.TryRead(out value.Unknown20))
            return false;

        if (!reader.TryRead(out value.Unknown21))
            return false;

        if (!reader.TryRead(out value.Unknown22))
            return false;

        if (!reader.TryRead(out value.Unknown23))
            return false;

        if (!reader.TryRead(out value.Unknown24))
            return false;

        if (!reader.TryRead(out value.Unknown25))
            return false;

        if (!reader.TryRead(out value.Unknown26))
            return false;

        if (!reader.TryRead(out value.Unknown27))
            return false;

        if (!reader.TryRead(out value.Unknown28))
            return false;

        if (!reader.TryRead(out value.Unknown29))
            return false;

        if (!reader.TryRead(out value.ConfigName))
            return false;

        if (!reader.TryRead(out value.Unknown30))
            return false;

        if (!reader.TryRead(out value.Unknown31))
            return false;

        if (!reader.TryRead(out value.UnknownVector6))
            return false;

        if (!reader.TryRead(out value.Unknown32))
            return false;

        if (!reader.TryRead(out value.Unknown33))
            return false;

        if (!reader.TryRead(out value.Unknown34))
            return false;

        if (!reader.TryRead(out value.Unknown35))
            return false;

        if (!reader.TryRead(out value.Unknown36))
            return false;

        if (!reader.TryRead(out value.Unknown37))
            return false;

        if (!reader.TryRead(out value.Unknown38))
            return false;

        if (!reader.TryRead(out value.Unknown39))
            return false;

        if (!reader.TryRead(out value.Unknown40))
            return false;

        if (!reader.TryRead(out value.Unknown41))
            return false;

        if (!reader.TryRead(out value.Unknown42))
            return false;

        if (!reader.TryRead(out value.Unknown43))
            return false;

        return reader.RemainingLength == 0;
    }
}
