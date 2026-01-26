using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json.Serialization;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Starlight.Records;

/// <summary>
/// Contains character record information that can be set by a player through their character profile.
/// Stored on the character profile and replicated to the server to seed station records.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class PlayerProvidedCharacterRecords
{
    /* Basic info */

    // Additional data is fetched from the profile itself (name, age, etc.)

    [DataField]
    public int Height { get; private set; }

    [DataField]
    public int Weight { get; private set; }

    [DataField]
    public string EmergencyContactName { get; private set; }

    // Employment
    [DataField]
    public bool HasWorkAuthorization { get; private set; }

    // Security
    [DataField]
    public string IdentifyingFeatures { get; private set; }

    // Medical
    [DataField]
    public string Allergies { get; private set; }
    [DataField]
    public string DrugAllergies { get; private set; }
    [DataField]
    public string PostmortemInstructions { get; private set; }

    // Incidents / free-form entries
    [DataField, JsonIgnore]
    public List<RecordEntry> MedicalEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> SecurityEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> EmploymentEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> AdminEntries { get; private set; }

    public PlayerProvidedCharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string emergencyContactName,
        string identifyingFeatures,
        string allergies, string drugAllergies,
        string postmortemInstructions,
        List<RecordEntry> medicalEntries,
        List<RecordEntry> securityEntries,
        List<RecordEntry> employmentEntries,
        List<RecordEntry> adminEntries)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        EmergencyContactName = emergencyContactName;
        IdentifyingFeatures = identifyingFeatures;
        Allergies = allergies;
        DrugAllergies = drugAllergies;
        PostmortemInstructions = postmortemInstructions;
        MedicalEntries = medicalEntries;
        SecurityEntries = securityEntries;
        EmploymentEntries = employmentEntries;
        AdminEntries = adminEntries;
    }

    public PlayerProvidedCharacterRecords(PlayerProvidedCharacterRecords other)
    {
        Height = other.Height;
        Weight = other.Weight;
        EmergencyContactName = other.EmergencyContactName;
        HasWorkAuthorization = other.HasWorkAuthorization;
        IdentifyingFeatures = other.IdentifyingFeatures;
        Allergies = other.Allergies;
        DrugAllergies = other.DrugAllergies;
        PostmortemInstructions = other.PostmortemInstructions;
        MedicalEntries = other.MedicalEntries.Select(x => new RecordEntry(x)).ToList();
        SecurityEntries = other.SecurityEntries.Select(x => new RecordEntry(x)).ToList();
        EmploymentEntries = other.EmploymentEntries.Select(x => new RecordEntry(x)).ToList();
        AdminEntries = other.AdminEntries.Select(x => new RecordEntry(x)).ToList();
    }

    /// <summary>
    /// Template with sensible defaults used when a profile has no saved character records.
    /// </summary>
    /// <returns>Returns default PlayerProvidedCharacterRecords</returns>
    public static PlayerProvidedCharacterRecords DefaultRecords() =>
        new(
            hasWorkAuthorization: true,
            height: 164, weight: 74,
            emergencyContactName: string.Empty,
            identifyingFeatures: string.Empty,
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>(),
            adminEntries: new List<RecordEntry>()
        );

    /// <summary>
    /// Template with sensible defaults used when a profile has no saved character records.
    /// </summary>
    /// <param name="species">Species for which the records are being made</param>
    /// <returns>Returns default PlayerProvidedCharacterRecords</returns>
    public static PlayerProvidedCharacterRecords DefaultRecords(SpeciesPrototype species) =>
        new(
            hasWorkAuthorization: true,
            height: (int)(species.StandardSize * (species.DefaultHeight - 1f) * 2f + species.StandardSize),
            weight: (int)(species.StandardWeight + species.StandardDensity * (species.DefaultWidth * species.DefaultHeight * species.DefaultHeight - 1)),
            emergencyContactName: string.Empty,
            identifyingFeatures: string.Empty,
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>(),
            adminEntries: new List<RecordEntry>()
        );

    /// <summary>
    /// Verifies whether this instance of PlayerProvidedCharacterRecords is deep equal with the other.
    /// </summary>
    /// <param name="other">Another instance of PlayerProvidedCharacterRecords that we are comparing against.</param>
    public bool MemberwiseEquals(PlayerProvidedCharacterRecords other)
    {
        var matches = Height == other.Height
                   && Weight == other.Weight
                   && EmergencyContactName == other.EmergencyContactName
                   && HasWorkAuthorization == other.HasWorkAuthorization
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && Allergies == other.Allergies
                   && DrugAllergies == other.DrugAllergies
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!matches)
            return false;
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (AdminEntries.Count != other.AdminEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
            return false;
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
            return false;
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
            return false;
        if (AdminEntries.Where((t, i) => !t.MemberwiseEquals(other.AdminEntries[i])).Any())
            return false;
        return true;
    }

    /// <summary>
    /// Ensure that this instance of PlayerProvidedCharacterRecords is equal with other, throwing DebugAssertException if they are not.
    /// </summary>
    /// <param name="other">Another instance of PlayerProvidedCharacterRecords that we are comparing against.</param>
    /// <exception cref="DebugAssertException">Thrown when instances of PlayerProvidedCharacterRecords are not equal.</exception>
    public bool AssertEquals(PlayerProvidedCharacterRecords other)
    {
        if (Height != other.Height) throw new DebugAssertException($"Height does not match expected got '{Height}' expected '{other.Height}'");
        if (Weight != other.Weight) throw new DebugAssertException($"Weight does not match expected got '{Weight}' expected '{other.Weight}'");
        if (EmergencyContactName != other.EmergencyContactName) throw new DebugAssertException($"EmergencyContactName does not match expected got '{EmergencyContactName}' expected '{other.EmergencyContactName}'");
        if (HasWorkAuthorization != other.HasWorkAuthorization) throw new DebugAssertException($"HasWorkAuthorization does not match expected got '{HasWorkAuthorization}' expected '{other.HasWorkAuthorization}'");
        if (IdentifyingFeatures != other.IdentifyingFeatures) throw new DebugAssertException($"IdentifyingFeatures does not match expected got '{IdentifyingFeatures}' expected '{other.IdentifyingFeatures}'");
        if (Allergies != other.Allergies) throw new DebugAssertException($"Allergies does not match expected got '{Allergies}' expected '{other.Allergies}'");
        if (DrugAllergies != other.DrugAllergies) throw new DebugAssertException($"DrugAllergies does not match expected got '{DrugAllergies}' expected '{other.DrugAllergies}'");
        if (PostmortemInstructions != other.PostmortemInstructions) throw new DebugAssertException($"PostmortemInstructions does not match expected got '{PostmortemInstructions}' expected '{other.PostmortemInstructions}'");
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (AdminEntries.Count != other.AdminEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
            return false;
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
            return false;
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
            return false;
        if (AdminEntries.Where((t, i) => !t.MemberwiseEquals(other.AdminEntries[i])).Any())
            return false;
        return true;
    }

    /// <summary>
    /// Verifies that the station records are in a valid state, such as clamping the strings and ensuring lists are not null.
    /// </summary>
    [Pure]
    public PlayerProvidedCharacterRecords EnsureValid()
    {
        // Clamp fields before serialization so database rows cannot exceed UI expectations.
        EmergencyContactName = CharacterRecordConstants.ClampString(EmergencyContactName, CharacterRecordConstants.TextMedLen);
        IdentifyingFeatures = CharacterRecordConstants.ClampString(IdentifyingFeatures, CharacterRecordConstants.TextMedLen);
        Allergies = CharacterRecordConstants.ClampString(Allergies, CharacterRecordConstants.TextMedLen);
        DrugAllergies = CharacterRecordConstants.ClampString(DrugAllergies, CharacterRecordConstants.TextMedLen);
        PostmortemInstructions = CharacterRecordConstants.ClampString(PostmortemInstructions, CharacterRecordConstants.TextMedLen);

        MedicalEntries ??= [];
        SecurityEntries ??= [];
        EmploymentEntries ??= [];
        AdminEntries ??= [];

        EnsureValidEntries(EmploymentEntries);
        EnsureValidEntries(MedicalEntries);
        EnsureValidEntries(SecurityEntries);
        EnsureValidEntries(AdminEntries);
        return this;
    }

    private static void EnsureValidEntries(List<RecordEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.EnsureValid();
        }
    }

    public PlayerProvidedCharacterRecords WithHeight(int height) => new(this) { Height = height };

    public PlayerProvidedCharacterRecords WithWeight(int weight) => new(this) { Weight = weight };

    public PlayerProvidedCharacterRecords WithWorkAuth(bool auth) => new(this) { HasWorkAuthorization = auth };

    public PlayerProvidedCharacterRecords WithContactName(string name) => new(this) { EmergencyContactName = name };

    public PlayerProvidedCharacterRecords WithIdentifyingFeatures(string features) => new(this) { IdentifyingFeatures = features };

    public PlayerProvidedCharacterRecords WithAllergies(string allergies) => new(this) { Allergies = allergies };

    public PlayerProvidedCharacterRecords WithDrugAllergies(string allergies) => new(this) { DrugAllergies = allergies };

    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string instructions) => new(this) { PostmortemInstructions = instructions };

    public PlayerProvidedCharacterRecords WithEmploymentEntries(List<RecordEntry> entries) => new(this) { EmploymentEntries = entries };

    public PlayerProvidedCharacterRecords WithMedicalEntries(List<RecordEntry> entries) => new(this) { MedicalEntries = entries };

    public PlayerProvidedCharacterRecords WithSecurityEntries(List<RecordEntry> entries) => new(this) { SecurityEntries = entries };

    public PlayerProvidedCharacterRecords WithAdminEntries(List<RecordEntry> entries) => new(this) { AdminEntries = entries };
}
