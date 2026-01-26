using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Records;

/// <summary>
/// Constants used in character records.
/// </summary>
public sealed class CharacterRecordConstants
{
    /// <summary>
    /// Maximum length of a medium text, used in text fields like Title, Emergency contact etc.
    /// </summary>
    public const int TextMedLen = 64;
    
    /// <summary>
    /// Maximum length of a very long text, used in text fields like descriptions.
    /// </summary>
    public const int TextVeryLargeLen = 4096;
    
    /// <summary>
    /// Clamps the string, ensuring that it fits defined length.
    /// </summary>
    /// <param name="value">The value that we are clamping.</param>
    /// <param name="length">The length that we will truncate towards.</param>
    /// <returns>Returns truncated string that fits the defined length</returns>
    public static string ClampString(string value, int length)
    {
        // Avoid printing garbage characters by trimming to the console text limits.
        if (value.Length <= length)
            return value;
        return value[..length];
    }
    
    /// <summary>
    /// Calculates height and weight in centimeters and kilograms from appearance scales.
    /// </summary>
    /// <param name="profile">Character profile from which we take the scales.</param>
    /// <param name="prototypes">Prototype manager to get species data.</param>
    /// <param name="heightCentimeters">Calculated height in centimeters.</param>
    /// <param name="weightKilograms">Calculated weight in kilograms.</param>
    public static void CalculateMetrics(
        HumanoidCharacterProfile profile,
        IPrototypeManager prototypes,
        out int heightCentimeters,
        out int weightKilograms)
    {
        var species = prototypes.Index(profile.Species);
        var heightScale = profile.Appearance.Height;
        var widthScale = profile.Appearance.Width;

        // Convert proportions back into concrete centimeter/kilogram figures using the species defaults.
        var height = (species.StandardSize * (heightScale - 1f) * 2f) + species.StandardSize;
        var weight = species.StandardWeight +
                     (species.StandardDensity * ((widthScale * heightScale * heightScale) - 1f));

        heightCentimeters = (int) Math.Round(height);
        weightKilograms = (int) Math.Round(weight);
    }

    /// <summary>
    /// Creates a new PlayerProvidedCharacterRecords with height and weight calculated from appearance scales.
    /// </summary>
    /// <param name="records">Base records to copy.</param>
    /// <param name="profile">Character profile from which we take the scales.</param>
    /// <param name="prototypes">Prototype manager to get species data.</param>
    /// <returns>New records with recalculated weight and height.</returns>
    public static PlayerProvidedCharacterRecords WithCalculatedMetrics(
        PlayerProvidedCharacterRecords records,
        HumanoidCharacterProfile profile,
        IPrototypeManager prototypes)
    {
        CalculateMetrics(profile, prototypes, out var heightCm, out var weightKg);
        return records.WithHeight(heightCm).WithWeight(weightKg);
    }
}