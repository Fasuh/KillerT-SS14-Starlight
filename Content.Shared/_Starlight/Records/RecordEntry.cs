using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.Records;

/// <summary>
/// A single record entry used in the system.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class RecordEntry
{
    /// <summary>
    /// The title of a record entry.
    /// </summary>
    [DataField]
    public string Title { get; private set; }

    /// <summary>
    /// The person who authored or created the entry.
    /// </summary>
    [DataField]
    public string Involved { get; private set; }

    /// <summary>
    /// The description of the entry.
    /// </summary>
    [DataField]
    public string Description { get; private set; }

    public RecordEntry(string title, string involved, string desc)
    {
        Title = title;
        Involved = involved;
        Description = desc;
    }

    public RecordEntry(RecordEntry other)
        : this(other.Title, other.Involved, other.Description)
    {
    }

    /// <summary>
    /// Compares two RecordEntry instances, checking if they are equal. Returns True if all values are equal, otherwise False.
    /// </summary>
    /// <param name="other">The value we are comparing against.</param>
    public bool MemberwiseEquals(RecordEntry other) => Title == other.Title && Involved == other.Involved && Description == other.Description;

    /// <summary>
    /// Checks if the RecordEntry is valid, otherwise it will truncate the values.
    /// </summary>
    public void EnsureValid()
    {
        Title = CharacterRecordConstants.ClampString(Title, CharacterRecordConstants.TextMedLen);
        Involved = CharacterRecordConstants.ClampString(Involved, CharacterRecordConstants.TextMedLen);
        Description = CharacterRecordConstants.ClampString(Description, CharacterRecordConstants.TextVeryLargeLen);
    }
}