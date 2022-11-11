using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Tests.Migrators;

[TestFixture]
internal class TextBoxMigratorTests : MigratiorTestBase
{
    [SetUp]
    public override void Setup()
    {
        _migrator = new TextBoxMigrator();
    }

    [Test]
    public override void ConfigValueAsExpected()
    {
        var expected = @"{
  ""MaxChars"": 128
}";

        var result = _migrator.GetConfigValues(GetMigrationDataTypeProperty(), _context);

        Assert.AreEqual(expected, ConvertResultToJsonTestResult(result));
    }

    [TestCase("Some Text in a box", "Some Text in a box")]
    public override void ContentValueAsExpeceted(string value, string expected)
        => ContentValueAsExpecetedBase(value, expected);

    [Test]
    public override void DatabaseTypeAsExpected()
        => DatabaseTypeAsExpectedBase("Nvarchar");

    [Test]
    public override void EditorAliasAsExpected()
        => EditorAliasAsExpectedbase(UmbEditors.Aliases.TextBox);

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value)
        => new SyncMigrationContentProperty(UmbEditors.Aliases.TextBox, value);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty()
        => new SyncMigrationDataTypeProperty(UmbEditors.Aliases.TextBox, "Nvarchar",
            new List<PreValue> 
            { 
                new PreValue { SortOrder = 1, Alias = "maxChars", Value = "128" }
            });
}
