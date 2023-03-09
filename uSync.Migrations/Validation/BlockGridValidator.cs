using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.Configuration.Grid;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Migrators.BlockGrid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Models;

namespace uSync.Migrations.Validation;
internal class BlockGridValidator : ISyncMigrationValidator
{
    private readonly IGridConfig _gridConfig;
    private readonly SyncBlockMigratorCollection _blockMigrators;

    public BlockGridValidator(IGridConfig gridConfig, SyncBlockMigratorCollection blockMigrators)
    {
        this._gridConfig = gridConfig;
        _blockMigrators = blockMigrators;
    }

    public IEnumerable<MigrationMessage> Validate(MigrationOptions options)
    {
        if (options.SourceVersion == 7) return Enumerable.Empty<MigrationMessage>();
        if (options.PreferredMigrators == null) return Enumerable.Empty<MigrationMessage>();

        if (!options.PreferredMigrators.ContainsKey(UmbConstants.PropertyEditors.Aliases.Grid)
            || options.PreferredMigrators[UmbConstants.PropertyEditors.Aliases.Grid] != nameof(GridToBlockGridMigrator)) 
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        // validates that we have a block migrator for all the elements in the grid. 
        var results = new List<MigrationMessage>();

        foreach(var editor in _gridConfig.EditorsConfig.Editors)
        {
            var migrator = _blockMigrators.GetMigrator(editor);

            var message = new MigrationMessage("BlockGrid", "Editor",
                migrator is GridDefaultBlockMigrator ? MigrationMessageType.Warning : MigrationMessageType.Success);

            if (migrator == null)
            {
                message.Message = $"No migrator found";
                message.MessageType = MigrationMessageType.Error;
            }
            else if (migrator is GridDefaultBlockMigrator)
            {
                message.Message = $"No migrator found for '{editor.Alias}' or '{editor.View}' default will be used";
            }
            else
            {
                message.Message = $"Found '{migrator.GetType().Name}' for '{editor.Alias}'";
            }

            results.Add(message);
        }

        return results;

    }
}
