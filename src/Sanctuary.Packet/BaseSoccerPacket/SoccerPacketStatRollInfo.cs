using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketStatRollInfo@@"
// End-of-match reward roll shown to the player, reusing the existing RewardBundleBase type
// (same one MiniGameInfo uses for its reward fields). Server -> client only: RewardBundleBase
// has no reader, only a writer.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 17).
public class SoccerPacketStatRollInfo : BaseSoccerPacket, ISerializablePacket
{
    public new const short OpCode = 17;

    public ulong Guid;

    public RewardBundleBase RewardBundle = new();

    public SoccerPacketStatRollInfo() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(Guid);

        RewardBundle.Serialize(writer);

        return writer.Buffer;
    }
}
