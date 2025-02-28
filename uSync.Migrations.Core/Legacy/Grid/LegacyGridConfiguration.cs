using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.PropertyEditors;

using Umbraco.Cms.Core;

namespace uSync.Migrations.Core.Legacy.Grid;

public class LegacyGridConfiguration : IIgnoreUserStartNodesConfig
{
	[ConfigurationField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
	public JObject? Items { get; set; }

	[ConfigurationField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration", HideLabel = true)]
	public JObject? Rte { get; set; }

	[ConfigurationField("mediaParentId", "Image Upload Folder", "mediafolderpicker", Description = "Choose the upload location of pasted images")]
	public GuidUdi? MediaParentId { get; set; }

	[ConfigurationField("ignoreUserStartNodes", "Ignore User Start Nodes", "boolean", Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
	public bool IgnoreUserStartNodes { get; set; }
}