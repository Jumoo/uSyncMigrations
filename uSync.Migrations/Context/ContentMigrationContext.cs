using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Context;
public class ContentMigrationContext
{

	private Dictionary<Guid, string> _contentKeys { get; set; } = new();
	private Dictionary<Guid, string> _contentPaths { get; set; } = new();

	/// <summary>
	///  add the path for a content item to context. 
	/// </summary>
	public void AddContentPath(Guid key, string path)
		 => _ = _contentPaths.TryAdd(key, path);

	/// <summary>
	///  get the content path for a parent item from the context.
	/// </summary>
	public string GetContentPath(Guid parentKey)
		=> _contentPaths?.TryGetValue(parentKey, out var path) == true ? path : string.Empty;

	/// <summary>
	///  add a content key to the context.
	/// </summary>
	public void AddKey(Guid key, string alias)
		=> _ = _contentKeys.TryAdd(key, alias);

	/// <summary>
	///  get a context alias from the context
	/// </summary>
	public string GetAliasByKey(Guid key)
		=> _contentKeys?.TryGetValue(key, out var alias) == true ? alias : string.Empty;

}
