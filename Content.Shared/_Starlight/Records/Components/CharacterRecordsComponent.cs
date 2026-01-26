using Content.Shared._Starlight.Records.Systems;

namespace Content.Shared._Starlight.Records.Components;

/// <summary>
/// Stores every player's record for a given station during the round.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedCharacterRecordsSystem))]
public sealed partial class CharacterRecordsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public Dictionary<uint, FullCharacterRecords> Records = new();
}
