using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("nuPickers - base migrator, not used directly")]
public abstract class NuPickersToContentmentDataListBase : SyncPropertyMigratorBase
{
    private readonly IOptions<NuPickerMigrationOptions> _options;

    protected NuPickersToContentmentDataListBase(IOptions<NuPickerMigrationOptions> options)
    {
        _options = options;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Umbraco.Community.Contentment.DataList";

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        if (!contentProperty.Value.DetectIsJson())
        {
            return contentProperty.Value;
        }

        var jtoken = JToken.Parse(contentProperty.Value);
        var values = jtoken.Select(t => t.Value<string>("key")).ToArray();
        return JsonConvert.SerializeObject(values, Formatting.Indented);
    }

    public virtual string? MapAssembly(string? assemblyName)
    {
        if (assemblyName == null)
        {
            return assemblyName;
        }
        return _options?.Value?.AssembliesMapping?.FirstOrDefault(x => x.Key.Equals(assemblyName)).Value ??
               assemblyName;
    }

    public virtual string? MapNamespace(string? nameSpace)
    {
        if (nameSpace == null)
        {
            return nameSpace;
        }
        var namespaceOverride = _options.Value.NamespacesMapping?.OrderByDescending(x => x.Key.Length)
            .Where(x => nameSpace.Contains(x.Key));


        return namespaceOverride == null && namespaceOverride?.Any() != true
            ? nameSpace
            : nameSpace.Replace(namespaceOverride.FirstOrDefault().Key, namespaceOverride.FirstOrDefault().Value);
    }
}