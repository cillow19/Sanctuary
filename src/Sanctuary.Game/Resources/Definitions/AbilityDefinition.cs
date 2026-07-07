using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Resources.Definitions;

public class AbilityDefinition
{
    public int Id { get; set; }

    public int NameId { get; set; }
    public int AbilityId { get; set; }
    public int CompositeEffectId { get; set; }
    public bool HasTarget { get; set; }
}