using System.Numerics;
using System.Text.Json.Serialization;

using Sanctuary.Core.IO;

namespace Sanctuary.Game.Resources.Definitions.Zones;

public sealed class SoccerZoneDefinition : BaseZoneDefinition
{
    // Independent from SpawnPosition/SpawnRotation (which place the player) so the test ball
    // can sit somewhere else entirely, e.g. center field while players spawn on the sideline.
    [JsonConverter(typeof(Vector4JsonConverter))]
    public Vector4 BallSpawnPosition { get; set; }

    [JsonConverter(typeof(QuaternionJsonConverter))]
    public Quaternion BallSpawnRotation { get; set; }
}
