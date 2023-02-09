using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Migrators;

namespace uSync.Migrations.Context;
public class MigratorsContext
{
	private Dictionary<string, ISyncPropertyMigrator> _migrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	public ISyncPropertyMigrator? TryGetMigrator(string? editorAlias)
	=> string.IsNullOrEmpty(editorAlias)
		? null
		: _migrators.TryGetValue(editorAlias, out var migrator) == true ? migrator : null;

	/// <summary>
	///  Add a migrator for a given editorAlias
	/// </summary>
	public void AddPropertyMigration(string editorAlias, ISyncPropertyMigrator migrator)
	{
		_migrators.TryAdd(editorAlias, migrator);
	}

	/// <summary>
	///  get the variant version of a migrator (if there is one)
	/// </summary>
	public ISyncVariationPropertyMigrator? TryGetVariantMigrator(string editorAlias)
	{
		if (_migrators.TryGetValue(editorAlias, out var migrator)
			&& migrator is ISyncVariationPropertyMigrator variationPropertyMigrator)
		{
			return variationPropertyMigrator;
		}

		return null;
	}

}
