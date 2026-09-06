using System.Collections.Generic;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSetSoccerPlayerOneTimeStats@@"
// Applies an end-of-match stat/XP rollup exactly once (e.g. after SoccerGameState.Winner).
// Server -> client only, same reasoning as SoccerPacketUpdatePlayerStats.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 21).
public class SoccerPacketSetSoccerPlayerOneTimeStats : BaseSoccerPacket, ISerializablePacket
{
    public new const short OpCode = 21;

    public ulong Guid;

    public List<CharacterStat> Stats = new();

    public SoccerPacketSetSoccerPlayerOneTimeStats() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        writer.Write(Stats);

        return writer.Buffer;
    }
}
