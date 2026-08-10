using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.IO;
using Sanctuary.Game;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;
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

        bool isReferee = connection.Player.IsMod || connection.Player.IsAdmin;

        connection.Player.ActiveProfileId = packet.Id;

        var clientUpdatePacketActivateProfile = new ClientUpdatePacketActivateProfile();

        using var packetWriter = new PacketWriter();

        var profileToSerialize = profile;

        // The client only renders the pink referee name for whichever profile Id it was told
        // is active via this exact packet. Simply swapping profile.Id to 58 (the old approach)
        // corrupted the cached Referee entry, because every OTHER field (NameId/Icon/etc.) still
        // read as the real job's — the client showed the wrong job name/icon under the Referee
        // slot, which looked like (and was) a duplicate/broken entry. Instead, build a proper
        // merged profile: identity fields (Id/Name/Icon/ItemClasses/...) come from the real
        // Referee profile so the client's Referee slot stays internally consistent, while
        // Rank/Items/Abilities come from whichever job is actually equipped, so gear/abilities
        // stay correct. This never touches the real (non-Referee) profile object at all.
        if (isReferee && profile.Id != 58)
        {
            var refereeProfile = connection.Player.Profiles.FirstOrDefault(x => x.Id == 58);

            if (refereeProfile is not null)
            {
                profileToSerialize = new ClientPcProfile
                {
                    Id = refereeProfile.Id,
                    NameId = refereeProfile.NameId,
                    DescriptionId = refereeProfile.DescriptionId,
                    Type = refereeProfile.Type,
                    Icon = refereeProfile.Icon,
                    AbilityBgImageSet = refereeProfile.AbilityBgImageSet,
                    BadgeImageSet = refereeProfile.BadgeImageSet,
                    ButtonImageSet = refereeProfile.ButtonImageSet,
                    MembersOnly = refereeProfile.MembersOnly,
                    ItemClasses = refereeProfile.ItemClasses,

                    IsCombat = profile.IsCombat,
                    Rank = profile.Rank,
                    RankPercent = profile.RankPercent,
                    StarsAvailable = profile.StarsAvailable,
                    StarsEarned = profile.StarsEarned,
                    Items = profile.Items,
                    Abilities = profile.Abilities,
                    AbilityExperiences = profile.AbilityExperiences
                };
            }
        }

        profileToSerialize.Serialize(packetWriter);

        clientUpdatePacketActivateProfile.Payload = packetWriter.Buffer;

        clientUpdatePacketActivateProfile.Attachments = connection.Player.GetAttachments();

        clientUpdatePacketActivateProfile.Animation = 3001; // emo_outfit_all
        clientUpdatePacketActivateProfile.CompositeEffect = 4005; // PFX_Job_Swirl

        connection.SendTunneled(clientUpdatePacketActivateProfile);

        var playerUpdatePacketEquippedItemsChange = new PlayerUpdatePacketEquippedItemsChange();

        playerUpdatePacketEquippedItemsChange.Guid = connection.Player.Guid;

        playerUpdatePacketEquippedItemsChange.Attachments = clientUpdatePacketActivateProfile.Attachments;

        connection.Player.SendTunneledToVisible(playerUpdatePacketEquippedItemsChange);

        if (isReferee)
        {
            connection.Player.SendTunneledToVisible(connection.Player.GetAddPcPacket());
        }

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