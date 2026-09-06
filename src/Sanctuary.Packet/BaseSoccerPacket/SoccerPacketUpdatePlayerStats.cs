using System.Collections.Generic;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketUpdatePlayerStats@@"
// Pushes a player's derived soccer stats (SoccerKickPower/Footwork/Speed/Toughness/TacklePower -
// these already exist in CharacterStatId) after items/abilities have modified the base values.
// Server -> client only, same as ClientUpdatePacketUpdateStat which this mirrors: CharacterStat
// has no reader, only a writer.
public class SoccerPacketUpdatePlayerStats : BaseSoccerPacket, ISerializablePacket
{
    public new const short OpCode = 10;

    public ulong Guid;

    public List<CharacterStat> Stats = new();

    public SoccerPacketUpdatePlayerStats() : base(OpCode)
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
