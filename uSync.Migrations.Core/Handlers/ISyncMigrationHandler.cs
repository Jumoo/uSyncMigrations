using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Handlers;

public interface ISyncMigrationHandler : IDiscoverable
{
    public int SourceVersion { get; }

    public string Group { get; }

    public string ItemType { get; }

    public int Priority { get; }

    public void PrepareMigrations(SyncMigrationContext context);

    public IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context);
}

