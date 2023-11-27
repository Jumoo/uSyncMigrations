using uSync.BackOffice;

namespace uSync.Migrations.Core.Context;

/// <summary>
///  A uSync migration context, lets us keep a whole list of things in memory while we do the migration.
/// </summary>
public class SyncMigrationContext : IDisposable
{
    public SyncMigrationContext(Guid migrationId, string sourceFolder, string siteFolder, bool siteIsSite, int version)
    {
        Metadata = new MigrationContextMetadata(
            migrationId: migrationId,
            sourceFolder: sourceFolder,
            siteFolder: siteFolder,
            siteIsSite: siteIsSite,
            sourceVersion: version);
    }

    /// <summary>
    ///  meta information about the Migration (id, source folder etc)
    /// </summary>
    public MigrationContextMetadata Metadata { get; private set; }

    /// <summary>
    ///  the suite of Migrators being used for this migration
    /// </summary>
    public MigratorsContext Migrators { get; } = new MigratorsContext();

    /// <summary>
    ///  Datatype information
    /// </summary>
    public DataTypeMigrationContext DataTypes { get; } = new DataTypeMigrationContext();


    /// <summary>
    ///  ContentType information
    /// </summary>
    public ContentTypeMigrationContext ContentTypes { get; } = new ContentTypeMigrationContext();

    /// <summary>
    ///  Content Information
    /// </summary>
    public ContentMigrationContext Content { get; } = new ContentMigrationContext();

    /// <summary>
    ///  Template Information
    /// </summary>
    public TemplateMigratorContext Templates { get; } = new TemplateMigratorContext();

    // generic stuff (applies to all types).

    private HashSet<string> _blockedTypes = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<int, Guid> _idKeyMap { get; set; } = new();
    private Dictionary<Guid, string> _keyToUdiEntityTypeMap { get; set; } = new();

    /// <summary>
    ///  is this item blocked based on alias and type. 
    /// </summary>
    public bool IsBlocked(string itemType, string alias)
        => _blockedTypes.Contains($"{itemType}_{alias}") == true;

    /// <summary>
    ///  add a blocked item to the context.
    /// </summary>
    public void AddBlocked(string itemType, string alias)
        => _ = _blockedTypes.Add($"{itemType}_{alias}");

    /// <summary>
    /// Adds the `int` ID (from the v7 CMS) with the corresponding `Guid` key.
    /// </summary>
    public void AddKey(int id, Guid key)
        => _idKeyMap.TryAdd(id, key);

    /// <summary>
    /// Retrieves the `Guid` key from the `int` ID reference (from the v7 CMS).
    /// </summary>
    public Guid GetKey(int id)
        => _idKeyMap?.TryGetValue(id, out var key) == true ? key : Guid.Empty;

    /// <summary>
    /// Retrieves the `int` ID reference (from the v7 CMS) from the `Guid` key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int GetId(Guid key)
            => _idKeyMap?.FirstOrDefault(x => x.Value == key).Key ?? 0;

    /// <summary>
    /// Adds the reference from a guid key to find the entity type
    /// </summary>
    public void AddEntityMap(Guid key, string entityType)
        => _keyToUdiEntityTypeMap.TryAdd(key, entityType);

    /// <summary>
    /// Get the entity type for the guid
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetEntityType(Guid key)
    {
        return _keyToUdiEntityTypeMap.TryGetValue(key, out var entityType) == true ? entityType : UmbConstants.UdiEntityType.Document;
    }

    public void Dispose()
    { }


    /// <summary>
    ///  the callback functions for sending information to the client.
    /// </summary>
    public uSyncCallbacks? Callbacks { get; set; }

    public void SendUpdate(string message, int count, int total)
    {
        Callbacks?.Update(message, count, total);   
    }


}
