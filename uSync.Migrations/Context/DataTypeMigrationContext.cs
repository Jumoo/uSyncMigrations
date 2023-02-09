using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Context;

/// <summary>
///  migrations for data types. 
/// </summary>
public class DataTypeMigrationContext
{
	/// <summary>
	///  list of keys to editor aliases used to lookup datatypes in content types !
	/// </summary>
	private Dictionary<Guid, string> _dataTypeDefinitions { get; set; } = new();

	/// <summary>
	///  when we replace an datatype with something else .
	/// </summary>
	private Dictionary<Guid, Guid> _dataTypeReplacements { get; set; } = new();

	/// <summary>
	///  datatypes that vary by something (e.g culture)
	/// </summary>
	private Dictionary<Guid, string> _dataTypeVariations { get; set; } = new();


	/// <summary>
	///  add a datatypedefinion (aka datatype key) to the context.
	/// </summary>
	public void AddDefinition(Guid dtd, string editorAlias)
		=> _ = _dataTypeDefinitions.TryAdd(dtd, editorAlias);

	/// <summary>
	///  get a datatype definiton from the context.
	/// </summary>
	public string GetByDefinition(Guid guid)
		=> _dataTypeDefinitions?.TryGetValue(guid, out var editorAlias) == true
			? editorAlias
			: string.Empty;

	/// <summary>
	///  add the key that replaces a datatype to the context.
	/// </summary>
	public void AddReplacement(Guid orginal, Guid replacement)
		=> _ = _dataTypeReplacements.TryAdd(orginal, replacement);

	/// <summary>
	///  get any replacement key values for a given datatype key
	/// </summary>
	public Guid GetReplacement(Guid orginal)
		=> _dataTypeReplacements?.TryGetValue(orginal, out var replacement) == true
			? replacement
			: orginal;

	/// <summary>
	///  add a variation (e.g culture, segment or nothing) value for a datatype to the context.
	/// </summary>
	public void AddVariation(Guid guid, string variation)
		=> _ = _dataTypeVariations?.TryAdd(guid, variation);

	/// <summary>
	///  retrieve the variation that a datatype will ask a doctype property to perform.
	/// </summary>
	public string GetVariation(Guid guid)
		=> _dataTypeVariations?.TryGetValue(guid, out var variation) == true
			? variation : "Nothing";

}
