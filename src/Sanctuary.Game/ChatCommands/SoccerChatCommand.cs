using System;

using Sanctuary.Core.IO;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.ChatCommands;

// Debug-only trigger for the soccer test zone (see SoccerZone). Not the real entry point -
// production soccer would go through LobbyGameDefinition/matchmaking - this exists purely to
// get a character into "bw_soccer" so the client's SoccerProcessor gets constructed and its
// real network opcode can be observed. See BaseSoccerPacket.cs.
public class SoccerChatCommand : IChatCommand
{
    private readonly IZoneManager _zoneManager;
    private readonly IChatCommandManager _chatCommandManager;

    public string KeyWord => "soccer";
    public string Usage => "[testopcode <n>]";
    public string Description => "[Debug] Teleports you into the soccer test zone, or probes a candidate BaseSoccerPacket opcode.";

    public ChatCommandRole RequiredRole => ChatCommandRole.Admin;

    public SoccerChatCommand(IZoneManager zoneManager, IChatCommandManager chatCommandManager)
    {
        _zoneManager = zoneManager;
        _chatCommandManager = chatCommandManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        if (args.Length == 0)
            return TeleportToZone(invoker);

        if (args[0].Equals("testopcode", StringComparison.OrdinalIgnoreCase))
            return TestOpcode(invoker, args[1..]);

        return false;
    }

    private bool TeleportToZone(Player invoker)
    {
        var zone = _zoneManager.SoccerZone;

        invoker.TeleportToZone(zone, zone.SpawnPosition, zone.SpawnRotation);

        _chatCommandManager.LogAction(this, invoker, "Teleport to soccer test zone", null, null);
        ChatHelper.SendSystemMessage(invoker, "Teleporting to the soccer test zone...");
        return true;
    }

    // Sends the same match-bootstrap sequence as SoccerZone.SendMatchBootstrap, but with the
    // top-level BaseSoccerPacket opcode overridden to a candidate value at runtime instead of
    // the compiled placeholder (220) - so we can try real candidates without a rebuild/restart
    // per attempt. If the candidate is correct, the client's SoccerProcessor should react
    // visibly: camera switching to Camera_ChangeToSoccerSettings, SoccerScores/SoccerTimer UI
    // appearing, etc. (all confirmed via Ghidra). Nothing here touches the client process
    // itself - this is only ever normal game traffic our own server already sends.
    private bool TestOpcode(Player invoker, string[] args)
    {
        if (args.Length != 1 || !short.TryParse(args[0], out var opcode))
            return false;

        invoker.SendTunneled(BuildSetClientConfig(opcode));
        invoker.SendTunneled(BuildRegisterPlayer(opcode, invoker));
        invoker.SendTunneled(BuildSetPlayerTeam(opcode, invoker));
        invoker.SendTunneled(BuildTeamColours(opcode));
        invoker.SendTunneled(BuildUpdateGameState(opcode));

        _chatCommandManager.LogAction(this, invoker, "Test soccer opcode", null, $"opcode={opcode}");
        ChatHelper.SendSystemMessage(invoker, $"Sent soccer bootstrap using candidate opcode {opcode}. Watch for any reaction.");
        return true;
    }

    private static byte[] BuildSetClientConfig(short opcode)
    {
        using var writer = new PacketWriter();

        writer.Write(opcode);
        writer.Write((short)7); // SoccerPacketSetClientConfig sub-opcode

        writer.Write(1);   // GameId
        writer.Write(0);   // MapId (Briarwood)
        writer.Write(1);   // PlayersPerTeam
        writer.Write(300); // PeriodLengthSeconds
        writer.Write(5);   // MaxScore
        writer.Write(false); // PickupsEnabled
        writer.Write(false); // SuddenDeathEnabled

        return writer.Buffer;
    }

    private static byte[] BuildRegisterPlayer(short opcode, Player player)
    {
        using var writer = new PacketWriter();

        writer.Write(opcode);
        writer.Write((short)3); // SoccerPacketRegisterPlayer sub-opcode

        writer.Write(player.Guid);
        writer.Write(player.Name.FullName);
        writer.Write(0);     // TeamId
        writer.Write(false); // IsGoalie
        writer.Write(true);  // IsLocalPlayer

        return writer.Buffer;
    }

    private static byte[] BuildSetPlayerTeam(short opcode, Player player)
    {
        using var writer = new PacketWriter();

        writer.Write(opcode);
        writer.Write((short)6); // SoccerPacketSetPlayerTeam sub-opcode

        writer.Write(player.Guid);
        writer.Write(0); // TeamId

        return writer.Buffer;
    }

    private static byte[] BuildTeamColours(short opcode)
    {
        using var writer = new PacketWriter();

        writer.Write(opcode);
        writer.Write((short)8); // SoccerPacketTeamColours sub-opcode

        writer.Write(0);          // TeamId
        writer.Write(0x1E90FFu);  // PrimaryColor
        writer.Write(0xFFFFFFu);  // SecondaryColor

        return writer.Buffer;
    }

    private static byte[] BuildUpdateGameState(short opcode)
    {
        using var writer = new PacketWriter();

        writer.Write(opcode);
        writer.Write((short)5); // SoccerPacketUpdateGameState sub-opcode

        writer.Write((int)SoccerGameState.PlayingSoccer);
        writer.Write(0); // StateTimeMs

        return writer.Buffer;
    }
}
