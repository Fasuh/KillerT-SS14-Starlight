using Content.Server.Access.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared.Forensics.Components;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Content.Shared._Starlight.Records;
using Content.Shared._Starlight.Records.Systems;
using Robust.Shared.Prototypes;
using CharacterRecordsComponent = Content.Shared._Starlight.Records.Components.CharacterRecordsComponent;

namespace Content.Server._Starlight.Records.Systems;

public sealed class CharacterRecordsSystem : SharedCharacterRecordsSystem
{
    private static readonly ISawmill Sawmill = Logger.GetSawmill("characterrecords");

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;

    private const string UnknownSpeciesDisplay = "Unknown";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn, after: new[] { typeof(StationRecordsSystem) });
    }

    /// <summary>
    /// Seeds the runtime record cache whenever a player joins or respawns.
    /// </summary>
    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        if (!HasComp<StationRecordsComponent>(args.Station))
        {
            Sawmill.Error(
                $"Tried to add character records on station {ToPrettyString(args.Station)} which is missing {nameof(StationRecordsComponent)}.");
            return;
        }

        if (!HasComp<CharacterRecordsComponent>(args.Station))
        {
            AddComp<CharacterRecordsComponent>(args.Station);
            Sawmill.Debug($"Attached {nameof(CharacterRecordsComponent)} to station {ToPrettyString(args.Station)}.");
        }

        if (args.Profile is null)
        {
            Sawmill.Error(
                $"Null profile in {nameof(CharacterRecordsSystem)}.{nameof(OnPlayerSpawn)} for player {args.Player?.Name ?? "<unknown>"}.");
            return;
        }

        if (string.IsNullOrEmpty(args.JobId))
        {
            Sawmill.Error(
                $"Null or empty JobId in {nameof(OnPlayerSpawn)} for character {args.Profile.Name} played by {args.Player.Name}.");
            return;
        }

        var profile = args.Profile;

        // Use the player's saved records when available; otherwise seed with the default template.
        // TODO: Update to use Starlight profile property when available
        var profileRecords = profile.CDCharacterRecords ?? PlayerProvidedCharacterRecords.DefaultRecords();

        // Calculate height/weight from appearance scales
        profileRecords = CharacterRecordConstants.WithCalculatedMetrics(profileRecords, profile, _prototype);

        if (!_prototype.TryIndex(args.JobId, out JobPrototype? jobPrototype))
        {
            Sawmill.Error($"Invalid job prototype ID '{args.JobId}' while creating records for {profile.Name}.");
            return;
        }

        var player = args.Mob;
        TryComp<FingerprintComponent>(player, out var fingerprintComponent);
        TryComp<DnaComponent>(player, out var dnaComponent);

        var jobTitle = jobPrototype.LocalizedName;

        // Get station record key from player's ID card
        var stationRecordsKey = GetStationRecordKey(player);
        if (stationRecordsKey == null)
        {
            Sawmill.Debug(
                $"No station record key found for {profile.Name} ({ToPrettyString(player)}). Skipping character record creation.");
            return;
        }

        // Prefer the live station record title in case the job changed after spawning
        if (_stationRecords.TryGetRecord<GeneralStationRecord>(stationRecordsKey.Value, out var stationRecord))
        {
            jobTitle = stationRecord.JobTitle;
        }

        var speciesName = GetReadableSpeciesName(profile);

        // Build the composite record
        var records = new FullCharacterRecords(
            pRecords: new PlayerProvidedCharacterRecords(profileRecords),
            stationRecordsKey: stationRecordsKey?.Id,
            name: profile.Name,
            age: profile.Age,
            species: speciesName,
            jobTitle: jobTitle,
            jobIcon: jobPrototype.Icon,
            gender: profile.Gender,
            sex: profile.Sex,
            fingerprint: fingerprintComponent?.Fingerprint,
            dna: dnaComponent?.DNA,
            owner: player);

        AddRecord(args.Station, player, records);
    }

    /// <summary>
    /// Gets station record key from a player entity by checking their ID card.
    /// </summary>
    private StationRecordKey? GetStationRecordKey(EntityUid uid)
    {
        if (!_idCard.TryFindIdCard(uid, out var idCard))
            return null;

        if (!TryComp<StationRecordKeyStorageComponent>(idCard, out var storage) || storage.Key == null)
            return null;

        return storage.Key;
    }

    /// <summary>
    /// Resolves a localized, human-readable base species display name.
    /// </summary>
    private string ResolveBaseSpeciesDisplayName(string? speciesId)
    {
        if (string.IsNullOrWhiteSpace(speciesId))
            return UnknownSpeciesDisplay;

        if (_prototype.TryIndex<SpeciesPrototype>(speciesId, out var proto))
        {
            if (Loc.TryGetString(proto.Name, out var localized) && !string.IsNullOrWhiteSpace(localized))
                return localized;

            if (!string.IsNullOrWhiteSpace(proto.Name))
                return proto.Name;
        }

        return !string.IsNullOrWhiteSpace(speciesId) ? speciesId : UnknownSpeciesDisplay;
    }

    /// <summary>
    /// Returns "Custom (Base)" when a differing custom name exists; otherwise only the base display.
    /// </summary>
    private string GetReadableSpeciesName(HumanoidCharacterProfile profile)
    {
        var baseDisplay = ResolveBaseSpeciesDisplayName(profile?.Species);
        var custom = profile?.CustomSpecieName;

        if (!string.IsNullOrWhiteSpace(custom))
        {
            var customTrimmed = custom.Trim();
            if (!customTrimmed.Equals(baseDisplay, StringComparison.OrdinalIgnoreCase))
                return $"{customTrimmed} ({baseDisplay})";
        }

        return baseDisplay;
    }

    /// <summary>
    /// Persists a newly constructed record using station record key ID as the dictionary key.
    /// Requires a valid station record key - will not create records without one.
    /// </summary>
    private void AddRecord(EntityUid station, EntityUid player, FullCharacterRecords records,
        CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return;

        if (!records.StationRecordsKey.HasValue)
        {
            Sawmill.Warning(
                $"Attempted to add character record for {ToPrettyString(player)} on {ToPrettyString(station)} without a station record key. Skipping.");
            return;
        }

        // Use station record key ID as the dictionary key
        var key = records.StationRecordsKey.Value;

        // Store the record
        if (!recordsDb.Records.TryAdd(key, records))
        {
            Sawmill.Warning(
                $"Duplicate character record key {key} encountered for {ToPrettyString(player)} on {ToPrettyString(station)}. Overwriting existing entry.");
            recordsDb.Records[key] = records;
        }

        Sawmill.Debug(
            $"Stored character record {key} for {ToPrettyString(player)} on {ToPrettyString(station)} (station record id: {records.StationRecordsKey.Value}).");
        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }

    /// <summary>
    /// Resets all player-authored information back to the default template.
    /// </summary>
    public void ResetRecord(
        EntityUid station,
        EntityUid player,
        StationRecordKey? stationKey = null,
        CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return;

        // Get station record key if not provided
        if (stationKey == null)
        {
            stationKey = GetStationRecordKey(player);
            if (stationKey == null)
            {
                Sawmill.Warning(
                    $"Attempted to reset records for {ToPrettyString(player)} but no station record key found.");
                return;
            }
        }

        if (!recordsDb.Records.TryGetValue(stationKey.Value.Id, out var value))
        {
            Sawmill.Warning(
                $"Attempted to reset records for {ToPrettyString(player)} but no entry exists on station {ToPrettyString(station)}.");
            return;
        }

        // Replace the player-authored information with a clean template
        var records = PlayerProvidedCharacterRecords.DefaultRecords();

        if (TryComp(player, out MetaDataComponent? meta))
            value.Name = meta.EntityName;

        value.PRecords = records;
        Sawmill.Debug($"Reset character records for {ToPrettyString(player)} on {ToPrettyString(station)}.");
        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }

    /// <summary>
    /// Updates a selected record.
    /// </summary>
    public void UpdateRecord(
        EntityUid station,
        StationRecordKey key,
        Action<FullCharacterRecords> update,
        CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return;

        if (!recordsDb.Records.TryGetValue(key.Id, out var record))
            return;

        update(record);
        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }
    
    /// <summary>
    /// Fetches all character records for a given station.
    /// </summary>
    /// <param name="station">The station entity to query records from.</param>
    /// <param name="recordsDb">Optional component to fetch from.</param>
    /// <returns>
    /// A dictionary of FullCharacterRecords for a given station.
    /// </returns>
    public IDictionary<uint, FullCharacterRecords> QueryRecords(EntityUid station, CharacterRecordsComponent? recordsDb = null) =>
        // Provide a safe empty map when the station lacks runtime record state.
        !Resolve(station, ref recordsDb)
            ? new Dictionary<uint, FullCharacterRecords>()
            : recordsDb.Records;
    
    /// <summary>
    /// Gets a character record by station record key.
    /// </summary>
    /// <param name="station">The station entity.</param>
    /// <param name="key">The station record key to look for.</param>
    /// <param name="recordsDb">Optional component to fetch from.</param>
    /// <returns>The character record if found, null otherwise.</returns>
    public FullCharacterRecords? GetRecord(EntityUid station, StationRecordKey key, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return null;
    
        return recordsDb.Records.TryGetValue(key.Id, out var record) ? record : null;
    }
}

public sealed class CharacterRecordsModifiedEvent : EntityEventArgs;