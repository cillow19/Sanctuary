using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// Confirmed via RTTI: ".?AUSoccerPacketSetClientConfig@@"
// Sent once, right after the client's SoccerProcessor is created. This is what
// WaitForWorldReady is blocked on client-side ("WaitForWorldReady: waiting for soccer
// processor") before it will drop the loading screen for a soccer zone.
public class SoccerPacketSetClientConfig : BaseSoccerPacket, ISerializablePacket, IDeserializable<SoccerPacketSetClientConfig>
{
    public new const short OpCode = 2;

    public int GameId;

    // Which soccer field/zone this match is running on.
    // 0 - Briarwood ("bw_soccer")
    // 1 - Sanctuary  ("sg_soccer")
    // 2 - Snowhill   ("sh_soccer")
    public int MapId;

    public int PlayersPerTeam;
    public int PeriodLengthSeconds;
    public int MaxScore;

    public bool PickupsEnabled;
    public bool SuddenDeathEnabled;

    public SoccerPacketSetClientConfig() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(GameId);
        writer.Write(MapId);

        writer.Write(PlayersPerTeam);
        writer.Write(PeriodLengthSeconds);
        writer.Write(MaxScore);

        writer.Write(PickupsEnabled);
        writer.Write(SuddenDeathEnabled);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out SoccerPacketSetClientConfig value)
    {
        value = new SoccerPacketSetClientConfig();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.GameId))
            return false;

        if (!reader.TryRead(out value.MapId))
            return false;

        if (!reader.TryRead(out value.PlayersPerTeam))
            return false;

        if (!reader.TryRead(out value.PeriodLengthSeconds))
            return false;

        if (!reader.TryRead(out value.MaxScore))
            return false;

        if (!reader.TryRead(out value.PickupsEnabled))
            return false;

        if (!reader.TryRead(out value.SuddenDeathEnabled))
            return false;

        return reader.RemainingLength == 0;
    }
}
