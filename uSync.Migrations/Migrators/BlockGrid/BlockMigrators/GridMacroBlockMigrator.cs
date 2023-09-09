using Newtonsoft.Json;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators
{
    public class GridMacroBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
    {
        private readonly IContentTypeService _contentTypeService;

        public GridMacroBlockMigrator(IShortStringHelper shortStringHelper, IContentTypeService contentTypeService) : base(shortStringHelper)
        {
            _contentTypeService = contentTypeService;
        }

        public string[] Aliases => new[] { "macro" };

        public override string GetEditorAlias(ILegacyGridEditorConfig editor) => "Macro Picker";


        public override Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
        {
            var properties = new Dictionary<string, object>();
            if (control.Value == null) return properties;

            var macroObject = JsonConvert.DeserializeObject<MacroObject>(control.Value.ToString());

            if (macroObject == null) return properties;

            if (macroObject.MacroParams == null 
                || macroObject.MacroParams.RawPropertyValues == null 
                || !macroObject.MacroParams.RawPropertyValues.Any()) return properties;

 
            var contentType = _contentTypeService.GetAllElementTypes().Where(x => x.Alias.ToLower() == macroObject.MacroEditorAlias.ToLower()).FirstOrDefault();

            foreach (var item in macroObject.MacroParams.RawPropertyValues)
            {
                if(item.Value != null)
                    properties.Add(item.Key, item.Value);
            }

            return properties;
        }
    }

    public class MacroObject
    {
        [JsonProperty("macroAlias")]
        public string MacroEditorAlias { get; set; } = null!;

        [JsonProperty("macroParamsDictionary")]
        public MacroParams? MacroParams { get; set; } = null!;
    }

    public class MacroParams {
        [JsonExtensionData]
        public IDictionary<string, object?> RawPropertyValues { get; set; } = null!;
    }
}
