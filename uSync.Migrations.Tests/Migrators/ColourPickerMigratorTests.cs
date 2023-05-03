using NUnit.Framework;

using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Tests.Migrators;

[TestFixture]
public class ColourPickerMigratorTests : MigratiorTestBase
{
    private IList<PreValue> _preValues = new List<PreValue>();

    string _migratedValue = @"{
  ""Items"": [
    {
      ""id"": 1,
      ""value"": ""{\""value\"":\""000000\"",\""label\"":\""Black\""}""
    },
    {
      ""id"": 2,
      ""value"": ""{\""value\"":\""ff0000\"",\""label\"":\""Red\""}""
    },
    {
      ""id"": 3,
      ""value"": ""{\""value\"":\""00ff00\"",\""label\"":\""Blue\""}""
    },
    {
      ""id"": 4,
      ""value"": ""{\""value\"":\""0000ff\"",\""label\"":\""Green\""}""
    }
  ],
  ""UseLabel"": true
}";

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _preValues = new List<PreValue>()
        {
            new PreValue { SortOrder = 1, Alias = "useLabel", Value = "1" },
            new PreValue { SortOrder = 2, Alias = "0", Value = "{\"value\":\"000000\",\"label\":\"Black\",\"sortOrder\":0}" },
            new PreValue { SortOrder = 3, Alias = "1", Value = "{\"value\":\"ff0000\",\"label\":\"Red\",\"sortOrder\":1}" },
            new PreValue { SortOrder = 4, Alias = "2", Value = "{\"value\":\"00ff00\",\"label\":\"Blue\",\"sortOrder\":2}" },
            new PreValue { SortOrder = 5, Alias = "3", Value = "{\"value\":\"0000ff\",\"label\":\"Green\",\"sortOrder\":3}" },
        };

        _migrator = new ColorPickerMigrator();
    }

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty()
        => new SyncMigrationDataTypeProperty("Colour Picker", "Umbraco.ColorPickerAlias", "Nvarchar", _preValues);

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value)
        => new SyncMigrationContentProperty("Test", "Colour picker", "Umbraco.ColorPickerAlias", value);

    [Test]
    public override void ConfigValueAsExpected()
    {
        var value = _migrator!.GetConfigValues(GetMigrationDataTypeProperty(), _context!);

        Assert.AreEqual(_migratedValue, base.ConvertResultToJsonTestResult(value));
    }

    [Test]
    public override void DatabaseTypeAsExpected()
        => DatabaseTypeAsExpectedBase("Nvarchar");

    [Test]
    public override void EditorAliasAsExpected()
        => EditorAliasAsExpectedbase(UmbConstants.PropertyEditors.Aliases.ColorPicker);

    [TestCase(
@"{""label"":""Blue"",""value"":""0000FF""}",
@"{
  ""value"": ""0000FF"",
  ""label"": ""Blue"",
  ""sortOrder"": 1,
  ""id"": ""1""
}")]
    public override void ContentValueAsExpeceted(string value, string expected)
        => ContentValueAsExpecetedBase(value, expected);

}
