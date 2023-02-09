using Umbraco.Cms.Core.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Models;

namespace uSync.Migrations.Handlers;

public interface ISyncMigrationHandler : IDiscoverable
{
    public int SourceVersion { get; } 

    public string Group { get; }

    public string ItemType { get; }

    public int Priority { get; }

    public void PrepareMigrations(SyncMigrationContext context);

    public IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context);
}

