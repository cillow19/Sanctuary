using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;

namespace Sanctuary.Game.ChatCommands;

// Debug-only trigger for the soccer test zone (see SoccerZone). Not the real entry point -
// production soccer would go through LobbyGameDefinition/matchmaking - this exists purely to
// get a character into "bw_soccer" so the client's SoccerProcessor gets constructed and its
// real network opcode can be observed (live capture/debugger). See BaseSoccerPacket.cs.
public class SoccerChatCommand : IChatCommand
{
    private readonly IZoneManager _zoneManager;
    private readonly IChatCommandManager _chatCommandManager;

    public string KeyWord => "soccer";
    public string Usage => "";
    public string Description => "[Debug] Teleports you into the soccer test zone.";

    public ChatCommandRole RequiredRole => ChatCommandRole.Admin;

    public SoccerChatCommand(IZoneManager zoneManager, IChatCommandManager chatCommandManager)
    {
        _zoneManager = zoneManager;
        _chatCommandManager = chatCommandManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        var zone = _zoneManager.SoccerZone;

        invoker.TeleportToZone(zone, zone.SpawnPosition, zone.SpawnRotation);

        _chatCommandManager.LogAction(this, invoker, "Teleport to soccer test zone", null, null);
        ChatHelper.SendSystemMessage(invoker, "Teleporting to the soccer test zone...");
        return true;
    }
}
