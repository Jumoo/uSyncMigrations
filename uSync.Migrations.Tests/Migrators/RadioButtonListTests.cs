using Newtonsoft.Json.Linq;
using NUnit.Framework;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Tests.Migrators;

[TestFixture]
public class RadioButtonListTests : MigratiorTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _migrator = new RadioButtonListMigrator();
    }

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty() =>
        new SyncMigrationDataTypeProperty("RadioButtonList", UmbConstants.PropertyEditors.Aliases.RadioButtonList,
            "Nvarchar",
            new List<PreValue>
            {
                new PreValue { SortOrder = 0, Alias = "0", Value = "One" },
                new PreValue { SortOrder = 1, Alias = "1", Value = "Two" },
                new PreValue { SortOrder = 2, Alias = "2", Value = "Three" },
            });
    
    private SyncMigrationDataTypeProperty GetMigrationDataTypePropertyWithInvalidSortOrder() =>
        new SyncMigrationDataTypeProperty("RadioButtonList", UmbConstants.PropertyEditors.Aliases.RadioButtonList,
            "Nvarchar",
            new List<PreValue>
            {
                new PreValue { SortOrder = 0, Alias = "0", Value = "One" },
                new PreValue { SortOrder = 0, Alias = "1", Value = "Two" },
                new PreValue { SortOrder = 0, Alias = "2", Value = "Three" },
            });

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value) =>
        new SyncMigrationContentProperty("Test", "RadioButton list", UmbConstants.PropertyEditors.Aliases.RadioButtonList,
            value);

    [Test]
    public override void DatabaseTypeAsExpected() => DatabaseTypeAsExpectedBase("Nvarchar");

    [Test]
    public override void EditorAliasAsExpected() =>
        EditorAliasAsExpectedbase(UmbConstants.PropertyEditors.Aliases.RadioButtonList);

    [Test]
    public override void ConfigValueAsExpected()
    {
        var configFile = _migrator!.GetConfigValues(GetMigrationDataTypeProperty(), _context!);
        // Parse the JSON to a JObject so we don't have to mess around inserting newlines which are enforced
        // by the base class serializer config
        var obj = JObject.Parse("{\"Items\":[{\"id\":0,\"value\":\"One\"},{\"id\":1,\"value\":\"Two\"},{\"id\":2,\"value\":\"Three\"}]}");
        var expected = obj.ToString();

        Assert.AreEqual(expected, ConvertResultToJsonTestResult(configFile));
    }
    
    [Test]
    public void ConfigValue_As_Expected_When_Source_Has_Invalid_SortOrder()
    {
        // In this test we will get the config values for a data type property that has invalid sort order values
        // that are all the same
        var configFile = _migrator!.GetConfigValues(GetMigrationDataTypePropertyWithInvalidSortOrder(), _context!);
        
        // Parse the JSON to a JObject so we don't have to mess around inserting newlines which are enforced
        // by the base class serializer config
        var obj = JObject.Parse(
            "{\"Items\":[{\"id\":0,\"value\":\"One\"},{\"id\":1,\"value\":\"Two\"},{\"id\":2,\"value\":\"Three\"}]}");
        var expected = obj.ToString();

        Assert.AreEqual(expected, ConvertResultToJsonTestResult(configFile));
    }

    public override void ContentValueAsExpeceted(string value, string expected)
    {
        ContentValueAsExpecetedBase(value, expected);
    }
}