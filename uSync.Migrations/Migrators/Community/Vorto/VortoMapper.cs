using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Community.Vorto;

[SyncMigrator("Our.Umbraco.Vorto")]
public class VortoMapper : SyncPropertyMigratorBase,
    ISyncReplacablePropertyMigrator,
    ISyncVariationPropertyMigrator
{
    public VortoMapper()
    { }

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => string.Empty;

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var wrappedDataType = GetWrappedDatatype(dataTypeProperty.PreValues, context);
        if (wrappedDataType.dataTypeInfo != null)
        {
            return wrappedDataType.dataTypeInfo.EditorAlias;
        }

        return string.Empty;
    }

    /// <summary>
    ///  Vorto properties don't actually need to be on the target - the properties they wrap should already be there.
    ///  so the migrator needs to actually tell the process what should be here.
    /// </summary>
    /// <param name="dataTypeProperty"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public ReplacementDataTypeInfo? GetReplacementEditorId(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var wrappedDataType = GetWrappedDatatype(dataTypeProperty.PreValues, context);
        if (wrappedDataType.dataTypeInfo != null)
        {
            return new ReplacementDataTypeInfo(wrappedDataType.key, wrappedDataType.dataTypeInfo.EditorAlias)
            {
                Variation = "Culture"
            };
        }

        return null;
    }

    private (Guid key, DataTypeInfo? dataTypeInfo) GetWrappedDatatype(IReadOnlyCollection<PreValue>? preValues, SyncMigrationContext context)
    {
        if (preValues == null) return (Guid.Empty, null);

        var dataType = preValues.FirstOrDefault(x => x.Alias.Equals("dataType"));
        if (dataType == null) return (Guid.Empty, null);

        var value = JsonConvert.DeserializeObject<JObject>(dataType.Value);
        if (value is null) return (Guid.Empty, null);

        // guid is the guid of the wrapped datatype.
        var attempt = value.Value<string>("guid").TryConvertTo<Guid>();
        if (attempt)
            return (attempt.Result, context.DataTypes.GetByDefinition(attempt.Result));

        return (Guid.Empty, null);
    }

    public Attempt<CulturedPropertyValue> GetVariedElements(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty.Value == null) return Attempt<CulturedPropertyValue>.Fail(
            new ArgumentNullException(nameof(contentProperty.Value)));

        try
        {
            var culturedValues = JsonConvert.DeserializeObject<CulturedPropertyValue>(contentProperty.Value);
            if (culturedValues is not null)
            {
                var dataType = context.DataTypes.GetByDefinition(culturedValues.DtdGuid);
                if (dataType is not null)
                {
                    var migrator = context.Migrators.TryGetMigrator(
                        $"{contentProperty.ContentTypeAlias}_{contentProperty.PropertyAlias}", dataType.OriginalEditorAlias);

                    if (migrator != null)
                    {
                        foreach (var cultureValue in culturedValues.Values)
                        {
                            var val = migrator.GetContentValue(
                                new SyncMigrationContentProperty(
                                    contentProperty.ContentTypeAlias,
                                    contentProperty.PropertyAlias,
                                    contentProperty.ContentTypeAlias,
                                    cultureValue.Value),
                                context);

                            if (val is not null)
                            {
                                culturedValues.Values[cultureValue.Key] = val;
                            }
                        }
                    }
                }

                return Attempt<CulturedPropertyValue>.Succeed(culturedValues);
            }

            return Attempt<CulturedPropertyValue>.Fail(new ArgumentNullException("Null value in Vorto", nameof(culturedValues)));
        }
        catch (Exception ex)
        {
            return Attempt<CulturedPropertyValue>.Fail(ex);
        }
    }
}
