using System.Collections.Generic;

namespace Sanctuary.Game.Resources.Definitions;

public class AbilityDefinitionFile
{
    public List<PartyAbilityDefinition> PartyAbilities { get; set; } = new List<PartyAbilityDefinition>();
}
