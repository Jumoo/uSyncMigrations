using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("Crumpled.CharLimitEditor")]
    public class CrumpledCharLimitEditorToTextboxMigrator : SyncPropertyMigratorBase
    {
        public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
      => UmbConstants.PropertyEditors.Aliases.TextBox;
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var config = new TextboxConfiguration();

            var mappings = new Dictionary<string, string>
            {
                { "limit", nameof(config.MaxChars) }
            };

            return config.MapPreValues(dataTypeProperty.PreValues, mappings);
        }
    }
}
