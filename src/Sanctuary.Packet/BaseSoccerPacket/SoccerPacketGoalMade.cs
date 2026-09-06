using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketGoalMade@@"
// Drives SoccerGoal / SoccerOpponentGoal / SoccerScores UI and the AMB_LP_Soccer_Crowd_Goal cue.
// SubOpCode confirmed from SoccerProcessor's message-dispatch switch (client FUN_00b80ac0, case 2).
public class SoccerPacketGoalMade : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketGoalMade>
{
    public new const short OpCode = 2;

    public int ScoringTeamId;
    public ulong ScorerGuid;

    public int TeamScore;
    public int OpponentScore;

    public bool IsSuperShot;

    public SoccerPacketGoalMade() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(ScoringTeamId);
        writer.Write(ScorerGuid);

        writer.Write(TeamScore);
        writer.Write(OpponentScore);

        writer.Write(IsSuperShot);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketGoalMade value)
    {
        value = new SoccerPacketGoalMade();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.ScoringTeamId))
            return false;

        if (!reader.TryRead(out value.ScorerGuid))
            return false;

        if (!reader.TryRead(out value.TeamScore))
            return false;

        if (!reader.TryRead(out value.OpponentScore))
            return false;

        if (!reader.TryRead(out value.IsSuperShot))
            return false;

        return reader.RemainingLength == 0;
    }
}
