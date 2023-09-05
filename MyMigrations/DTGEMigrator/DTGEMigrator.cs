using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;

namespace MyMigrations.DTGEMigrator;

[SyncMigrator(Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid)]
[SyncMigratorVersion(7,8)]
[SyncDefaultMigrator]
public class DTGEMigrator : SyncPropertyMigratorBase
{
	
	private readonly ILoggerFactory _loggerFactory;
	private readonly ILogger<DTGEMigrator> _logger;


    public DTGEMigrator(
	    ILoggerFactory loggerFactory)
    {

        _loggerFactory = loggerFactory;
		_logger = loggerFactory.CreateLogger<DTGEMigrator>();	
		
		
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);
	
	public override object? GetConfigValues (SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
		var ignoreUserStartNodes = dataTypeProperty.PreValues.SingleOrDefault(x => x.Alias == "ignoreUserStartNodes");
		if (ignoreUserStartNodes != null)
		{
			bool newValue = bool.TryParse(ignoreUserStartNodes.Value, out var parsedValue) 
				? parsedValue 
				: false;

			ignoreUserStartNodes.Value = newValue.ToString();
		}

		return base.GetConfigValues(dataTypeProperty, context);
	}
	
	public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
	    if (contentProperty.Value == null) return string.Empty;

	    var grid = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value);
	    if (grid == null) return contentProperty.Value;
	    
	    foreach (var section in grid.Sections)
	    {
		    foreach (var row in section.Rows)
		    {
			    foreach (var area in row.Areas)
			    {
				    foreach (var control in area.Controls)
				    {
					    if (!control.Value?.HasValues == true) continue;
					    
					    var isDTGEvalue = !String.IsNullOrEmpty(control.Value?.Value<string>("dtgeContentTypeAlias"));
					    if (isDTGEvalue)
					    {
						    var updatedValues = GetPropertyValues(control, context);
						    var updatedValuesSerialized = JsonConvert.SerializeObject(updatedValues);
						    
						    var controlValue = control.Value;
						    
						    controlValue["value"] = (JToken)JsonConvert.DeserializeObject(updatedValuesSerialized);
					    }
				    }
			    }
		    }
	    }

	    var propValue = JsonConvert.SerializeObject(grid);
	    return propValue;
	}
	
	private string GetContentTypeAlias(GridValue.GridControl control)
		=> control.Value?.Value<string>("dtgeContentTypeAlias") ?? string.Empty;



	private Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
	{
		var propertyValues = new Dictionary<string, object>();

		var contentTypeAlias = GetContentTypeAlias(control);
		if (string.IsNullOrWhiteSpace(contentTypeAlias)) return propertyValues;

		var elementValue = control.Value?.Value<JObject>("value")?
			.ToObject<IDictionary<string, object>>();

		if (elementValue == null) return propertyValues;

		foreach (var (propertyAlias, value) in elementValue)
		{
			var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias);

			if (editorAlias == null) continue;

			var migrator = context.Migrators.TryGetMigrator("DTGE." + editorAlias.OriginalEditorAlias);
			if (migrator == null)
				migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
			
			var propertyValue = value;
			
			if (migrator != null)
			{
				var valueToConvert = (value?.ToString() ?? "").Trim();
				
				var property = new SyncMigrationContentProperty(
					$"DTGE.{editorAlias.OriginalEditorAlias}",
					propertyAlias,
					editorAlias.OriginalEditorAlias,
					valueToConvert);

				var convertedValue = migrator.GetContentValue(property, context);
				if (convertedValue?.Trim().DetectIsJson() == true)
					propertyValue = JsonConvert.DeserializeObject(convertedValue ?? "");
				else
					propertyValue = convertedValue;

			}

			propertyValues[propertyAlias] = propertyValue;
		}

		return propertyValues;
	}
	
}

