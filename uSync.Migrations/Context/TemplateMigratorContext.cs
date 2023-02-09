using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Context;
public class TemplateMigratorContext
{
	private Dictionary<string, Guid> _templateKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);


	/// <summary>
	///  Add a template key to the context.
	/// </summary>
	public void AddAlias(string templateAlias, Guid templateKey)
		 => _ = _templateKeys.TryAdd(templateAlias, templateKey);

	/// <summary>
	///  get a template key (Guid) from the context 
	/// </summary>
	public Guid GetKeyByAlias(string templateAlias)
		 => _templateKeys?.TryGetValue(templateAlias, out var key) == true ? key : Guid.Empty;
}
