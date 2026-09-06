using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions.Zones;
using Sanctuary.Game.Zones;

namespace Sanctuary.Game;

public class ZoneManager : IZoneManager
{
    private readonly ILogger _logger;
    private readonly IResourceManager _resourceManager;
    private readonly IServiceProvider _serviceProvider;

    private static int _uniqueId = 1;

    private readonly ConcurrentDictionary<int, IZone> _zones = new();

    private const int StartingZoneDefinitionId = 1;
    private const int SoccerZoneDefinitionId = 2;
    public StartingZone StartingZone { get; private set; } = null!;
    public SoccerZone SoccerZone { get; private set; } = null!;

    public ZoneManager(
        ILoggerFactory loggerFactory,
        IResourceManager resourceManager,
        IServiceProvider serviceProvider)
    {
        _logger = loggerFactory.CreateLogger<ZoneManager>();

        _resourceManager = resourceManager;
        _serviceProvider = serviceProvider;
    }

    public bool Load()
    {
        if (!TryCreateStartingZone(StartingZoneDefinitionId, out var startingZone))
            return false;

        StartingZone = startingZone;

        if (!TryCreateSoccerZone(SoccerZoneDefinitionId, out var soccerZone))
            return false;

        SoccerZone = soccerZone;

        return true;
    }

    public bool TryGetPlayer(ulong guid, [MaybeNullWhen(false)] out Player player)
    {
        player = default;

        foreach (var zone in _zones)
        {
            if (zone.Value.TryGetPlayer(guid, out player))
                return true;
        }

        return false;
    }

    public bool TryGetPlayer(string name, [MaybeNullWhen(false)] out Player player)
    {
        player = default;

        foreach (var zone in _zones)
        {
            foreach (var zonePlayer in zone.Value.Players)
            {
                if (zonePlayer.Name.FullName == name)
                {
                    player = zonePlayer;
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryCreateStartingZone(int definitionId, [MaybeNullWhen(false)] out StartingZone zone)
    {
        zone = default;

        if (!_resourceManager.Zones.TryGetValue(definitionId, out var zoneDefinition))
            return false;

        if (zoneDefinition is not StartingZoneDefinition startingZoneDefinition)
            return false;

        zone = new StartingZone(startingZoneDefinition, _serviceProvider)
        {
            Id = _uniqueId++
        };

        zone.OnStart();

        return _zones.TryAdd(zone.Id, zone);
    }

    private bool TryCreateSoccerZone(int definitionId, [MaybeNullWhen(false)] out SoccerZone zone)
    {
        zone = default;

        if (!_resourceManager.Zones.TryGetValue(definitionId, out var zoneDefinition))
            return false;

        if (zoneDefinition is not SoccerZoneDefinition soccerZoneDefinition)
            return false;

        zone = new SoccerZone(soccerZoneDefinition, _serviceProvider)
        {
            Id = _uniqueId++
        };

        zone.OnStart();

        return _zones.TryAdd(zone.Id, zone);
    }
}