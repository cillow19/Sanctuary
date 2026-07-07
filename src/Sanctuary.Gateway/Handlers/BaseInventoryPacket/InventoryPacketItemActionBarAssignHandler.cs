using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class InventoryPacketItemActionBarAssignHandler
{
    private static ILogger _logger = null!;
    private static IResourceManager _resourceManager = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(InventoryPacketItemActionBarAssignHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
    }

    private static void ClearDuplicateAssignments(GatewayConnection connection, int packetGuid, int targetSlot)
    {
        Dictionary<int, int> actionBarSlots = connection.Player.ActionBarSlots;
        List<int> duplicateSlots = actionBarSlots
            .Where(x => x.Value == packetGuid && x.Key != targetSlot)
            .Select(x => x.Key)
            .ToList();

        foreach (int duplicateSlot in duplicateSlots)
        {
            actionBarSlots.Remove(duplicateSlot);

            connection.SendTunneled(new ClientUpdatePacketUpdateActionBarSlot
            {
                Data =
                {
                    Id = 2,
                    Slot = duplicateSlot
                },
                Slot =
                {
                    IsEmpty = true
                }
            });
        }
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!InventoryPacketItemActionBarAssign.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(InventoryPacketItemActionBarAssign));
            return false;
        }

        _logger.LogTrace("Received {name} packet. ( {packet} )", nameof(InventoryPacketItemActionBarAssign), packet);

        var clientUpdatePacketUpdateActionBarSlot = new ClientUpdatePacketUpdateActionBarSlot
        {
            Data =
            {
                Id = 2,
                Slot = packet.Slot
            }
        };

        if (packet.Guid == 0)
        {
            connection.Player.ActionBarSlots.Remove(packet.Slot);
            clientUpdatePacketUpdateActionBarSlot.Slot.IsEmpty = true;

            connection.SendTunneled(clientUpdatePacketUpdateActionBarSlot);

            return true;
        }

        var clientItem = connection.Player.Items.SingleOrDefault(x => x.Id == packet.Guid);

        if (clientItem is null)
        {
            _logger.LogWarning("User tried to equip unknown item. {guid}", packet.Guid);
            return true;
        }

        if (!_resourceManager.ClientItemDefinitions.TryGetValue(clientItem.Definition, out var clientItemDefinition))
        {
            _logger.LogWarning("User tried to equip unknown item definition. {guid} {definition}", packet.Guid, clientItem.Definition);
            return true;
        }

        ClearDuplicateAssignments(connection, packet.Guid, packet.Slot);

        connection.Player.ActionBarSlots[packet.Slot] = packet.Guid;

        clientUpdatePacketUpdateActionBarSlot.Slot.IsEmpty = false;
        clientUpdatePacketUpdateActionBarSlot.Slot.IconId = clientItemDefinition.Icon.Id;
        clientUpdatePacketUpdateActionBarSlot.Slot.NameId = clientItemDefinition.NameId;
        clientUpdatePacketUpdateActionBarSlot.Slot.Unknown5 = 1;
        clientUpdatePacketUpdateActionBarSlot.Slot.Unknown6 = 4;
        clientUpdatePacketUpdateActionBarSlot.Slot.Unknown7 = 15;

        clientUpdatePacketUpdateActionBarSlot.Slot.Enabled = true;

        clientUpdatePacketUpdateActionBarSlot.Slot.Unknown10 = 1000;
        clientUpdatePacketUpdateActionBarSlot.Slot.TotalRefreshTime = 1000;
        clientUpdatePacketUpdateActionBarSlot.Slot.Quantity = clientItem.Count;
        clientUpdatePacketUpdateActionBarSlot.Slot.ForceDismount = true;
        clientUpdatePacketUpdateActionBarSlot.Slot.Unknown15 = 1000;

        connection.SendTunneled(clientUpdatePacketUpdateActionBarSlot);

        return true;
    }
}