using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Resources.Definitions;

public class AbilityDefinitions
{
   public List<MiscPartySupplyDefinition> MiscPartySupplies { get; set; } = [];

}