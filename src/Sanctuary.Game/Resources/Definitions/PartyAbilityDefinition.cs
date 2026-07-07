using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Resources.Definitions;

public class PartyAbilityDefinition : AbilityDefinition
{
    public int AbilityId { get; set; }
    public bool HasTarget { get; set; }

}