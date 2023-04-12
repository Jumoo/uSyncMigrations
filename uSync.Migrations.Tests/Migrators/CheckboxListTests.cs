using NUnit.Framework;

using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Tests.Migrators;

[TestFixture]
public class CheckboxListTests : MigratiorTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _migrator = new CheckboxListMigrator();
    }

    [Test]
    public override void ConfigValueAsExpected() { }

    [Test]
    public void ContentValueIsJsonAsExpeceted()
    {
        var value = "Two, Three";
        var expected = $"[{Environment.NewLine}  \"Two\",{Environment.NewLine}  \"Three\"{Environment.NewLine}]";
        ContentValueAsExpecetedBase(value, expected);
    }

    [Test]
    public override void DatabaseTypeAsExpected()
        => DatabaseTypeAsExpectedBase("Nvarchar");

    [Test]
    public override void EditorAliasAsExpected()
        => EditorAliasAsExpectedbase(UmbConstants.PropertyEditors.Aliases.CheckBoxList);

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value)
        => new SyncMigrationContentProperty("Test", "Checkbox list", UmbConstants.PropertyEditors.Aliases.CheckBoxList, value);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty()
        => new SyncMigrationDataTypeProperty("Checkbox", UmbConstants.PropertyEditors.Aliases.CheckBoxList,
            "Nvarchar",
            new List<PreValue>
            {
                new PreValue { SortOrder = 1, Alias = "0", Value = "One"},
                new PreValue { SortOrder = 1, Alias = "1", Value = "Twe"},
                new PreValue { SortOrder = 1, Alias = "2", Value = "Three"},
            });

    public override void ContentValueAsExpeceted(string value, string expected)
    {
        // 
    }
}
