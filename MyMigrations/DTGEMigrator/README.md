# DTGE Migrator

This migrator is used when you are using the DocTypeGridEditor (DTGE).

DTGE stores multiple objects in its content value all at once as a large JSON string. Within the sections for each sub-control the actual value is stored as JSON directly, not as a string.

Within USync Migrations, each sub-migrator returns `GetContentValue` as a string. This has to be deserialized before adding it to the DTGE properties or it will be stored as a serialized STRING inside the DTGE config, instead of as a JSON object.

## Prevalues

DropDown and CheckboxList both use prevalues. When these are serialized by the normal uSync process the id values for the individual prevalue items are removed.

Because DTGE stores the prevalues inside the nested JSON, they don't get removed as part of the standard serialization process.

The DTGEPrevaluesMap allows for the entire list of prevalues to be serialized separately (like the grid.config file), and used as part of the conversion routine to keep prevalue entries across the migration.

To use it, run the following sql statement against your db (this SQL for v7). Store the results in `config/dtgePreValues.config.json` in the migration folder.

```sql

select distinct cmsDataTypePreValues.id, convert(nvarchar(max), cmsDataTypePreValues.value) as [value]
from cmsDataTypePreValues
    inner join cmsDataType on cmsDataType.nodeId = cmsDataTypePreValues.datatypeNodeId
    inner join cmsPropertyType on cmsDataType.nodeId = cmsPropertyType.dataTypeId
where cmsDataTypePreValues.alias <> 'multiple'
    and cmsDataType.propertyEditorAlias in ('Umbraco.CheckBoxList', 'Umbraco.DropDown.Flexible')

```
