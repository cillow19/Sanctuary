using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.IO;
using Sanctuary.Game;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class CommandPacketSetProfileHandler
{
    private static ILogger _logger = null!;
    private static IZoneManager _zoneManager = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(CommandPacketSetProfileHandler));

        _zoneManager = serviceProvider.GetRequiredService<IZoneManager>();
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!CommandPacketSetProfile.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(CommandPacketSetProfile));
            return false;
        }

        _logger.LogTrace("Received {name} packet. ( {packet} )", nameof(CommandPacketSetProfile), packet);

        var profile = connection.Player.Profiles.FirstOrDefault(x => x.Id == packet.Id);

        if (profile is null)
            return true;

        connection.Player.ActiveProfileId = packet.Id;

        connection.SendActivateProfile(profile, animation: 3001, compositeEffect: 4005); // emo_outfit_all / PFX_Job_Swirl

        // Resending the full (truthful) self data repairs the client's cached menu entry for
        // whichever profile SendActivateProfile just spoofed the id of, without undoing the
        // color that was just set.
        connection.SendSelfToClient();

        var attachments = connection.Player.GetAttachments();

        var playerUpdatePacketEquippedItemsChange = new PlayerUpdatePacketEquippedItemsChange();

        playerUpdatePacketEquippedItemsChange.Guid = connection.Player.Guid;

        playerUpdatePacketEquippedItemsChange.Attachments = attachments;

        connection.Player.SendTunneledToVisible(playerUpdatePacketEquippedItemsChange);

        var friendStatusPacket = new FriendStatusPacket
        {
            Guid = connection.Player.Guid,
            Status =
            {
                ProfileId = connection.Player.ActiveProfile.Id,
                ProfileRank = connection.Player.ActiveProfile.Rank,
                ProfileIconId = connection.Player.ActiveProfile.Icon,
                ProfileNameId = connection.Player.ActiveProfile.NameId,
                ProfileBackgroundImageId = connection.Player.ActiveProfile.BadgeImageSet
            }
        };

        foreach (var friend in connection.Player.Friends)
        {
            if (!_zoneManager.TryGetPlayer(friend.Guid, out var friendPlayer))
                continue;

            friendPlayer.SendTunneled(friendStatusPacket);
        }

        return true;
    }
}