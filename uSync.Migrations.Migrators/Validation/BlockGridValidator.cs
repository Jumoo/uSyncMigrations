﻿using uSync.Migrations.Core;
using uSync.Migrations.Core.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

namespace uSync.Migrations.Migrators.Validation;
internal class BlockGridValidator : ISyncMigrationValidator
{
    private readonly ILegacyGridConfig _gridConfig;
    private readonly SyncBlockMigratorCollection _blockMigrators;

    public BlockGridValidator(ILegacyGridConfig gridConfig, SyncBlockMigratorCollection blockMigrators)
    {
        _gridConfig = gridConfig;
        _blockMigrators = blockMigrators;
    }

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        if (validationContext.Metadata.SourceVersion == 7) return Enumerable.Empty<MigrationMessage>();
        if (validationContext.Options.PreferredMigrators == null) return Enumerable.Empty<MigrationMessage>();

        if (!validationContext.Options.PreferredMigrators.ContainsKey(uSyncMigrations.EditorAliases.Grid)
            || validationContext.Options.PreferredMigrators[uSyncMigrations.EditorAliases.Grid] != nameof(GridToBlockGridMigrator))
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        // TODO: Ideally we should have worked out if the site folder is the root, before here
        //       but at the moment validate doesn't create a context. 
        var legacyGridEditorsConfig = validationContext.Metadata.SiteFolderIsSite
            ? _gridConfig.EditorsConfig : _gridConfig.EditorsFromFolder(validationContext.Metadata.SiteFolder);

        // validates that we have a block migrator for all the elements in the grid. 
        var results = new List<MigrationMessage>
        {
            new MigrationMessage("BlockGird", "Config", MigrationMessageType.Success)
            {
                Message = $"Loaded {legacyGridEditorsConfig.Editors.Count} editors from grid config"
            }
        };

        foreach (var editor in legacyGridEditorsConfig.Editors)
        {
            var migrator = _blockMigrators.GetMigrator(editor);

            var message = new MigrationMessage("BlockGrid", "Editor",
                migrator is GridDefaultBlockMigrator ? MigrationMessageType.Warning : MigrationMessageType.Success);

            var thing = !string.IsNullOrEmpty(editor.Alias) ? editor.Alias : editor.View;

            if (migrator == null)
            {
                message.Message = $"No migrator found";
                message.MessageType = MigrationMessageType.Error;
            }
            else if (migrator is GridDefaultBlockMigrator)
            {
                message.Message = $"No migrator found for '{thing}' default will be used";
            }
            else
            {
                message.Message = $"Found '{migrator.GetType().Name}' for '{thing}'";
            }

            results.Add(message);
        }

        return results;

    }
}
