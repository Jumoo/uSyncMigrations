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
	private Dictionary<string, ISyncPropertyMigrator> _propertyMigrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	private Dictionary<string, ISyncPropertyMergingMigrator> _mergingMigrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	public ISyncPropertyMigrator? TryGetMigrator(string? editorAlias)
	=> string.IsNullOrEmpty(editorAlias)
		? null
		: _migrators.TryGetValue(editorAlias, out var migrator) == true ? migrator : null;

	public ISyncPropertyMigrator? TryGetPropertyAliasMigrator(string? propertyAlias)
	{
		if (string.IsNullOrEmpty(propertyAlias)) return null;

		// search for the full pattern
		if (_propertyMigrators.TryGetValue(propertyAlias, out var migrator))
			return migrator;

		// if we haven't found - but its a split (contentType_alias) value split the value and look just for the
		// propertyAlias

		if (propertyAlias.IndexOf('_') > 0)
		{
			var propertyEditorAlias = propertyAlias.Substring(propertyAlias.IndexOf('_') + 1);
			return _propertyMigrators.TryGetValue(propertyEditorAlias, out var propertyAliasMigrator) == true
				? propertyAliasMigrator : null;
		}
		return null;
	}

	public ISyncPropertyMigrator? TryGetMigrator(string? propertyAlias, string? editorAlias)
		=> TryGetPropertyAliasMigrator(propertyAlias) ?? TryGetMigrator(editorAlias);

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

	/// <summary>
	/// gets the property splitting version of a migrator (if there is one)
	/// </summary>
	public ISyncPropertySplittingMigrator? TryGetPropertySplittingMigrator(string editorAlias)
	{
		if (_migrators.TryGetValue(editorAlias, out var migrator)
			&& migrator is ISyncPropertySplittingMigrator propertySplittingMigrator)
		{
			return propertySplittingMigrator;
		}

		return null;
	}

	/// <summary>
	/// A cache of dictionary items that can be used if you need to store/retrieve custom data between 
	/// config and content mappings
	/// </summary>
	private Dictionary<string, Dictionary<string, object>> _migratorCache = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	///  add a dictionary of custom values for this datatype alias.
	/// </summary>
	/// <remarks>
	///  It is the migrators responsibility to make sure this custom set of values is uniqe and 
	///  does not clash. recommendation is to use the datatype's alias. 
	/// </remarks>
	public void AddCustomValues(string alias, Dictionary<string, object> values)
		=> _ = _migratorCache.TryAdd(alias, values);

	/// <summary>
	///  retreive a dictionary of custom values.
	/// </summary>
	/// <param name="alias"></param>
	/// <returns></returns>
	public Dictionary<string, object> GetCustomValues(string alias)
		=> _migratorCache.TryGetValue(alias, out Dictionary<string, object>? values)
			? values : new Dictionary<string, object>();


	public void AddMergingMigrator(string contentType, ISyncPropertyMergingMigrator mergingMigrator)
		=> _ = _mergingMigrators.TryAdd(contentType, mergingMigrator);

	public ISyncPropertyMergingMigrator? GetMergingMigrator(string contentType)
		=> _mergingMigrators.TryGetValue(contentType, out var migrator) == true ? migrator : null;

}
