using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using NUnit.Framework;

using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Tests.Migrators;

[TestFixture]
internal class TrueFalseMigratorTests : MigratorTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _migrator = new TrueFalseMigrator();
    }

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value)
        => new SyncMigrationContentProperty("Test", "True/False", UmbConstants.PropertyEditors.Aliases.Boolean, value);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty()
        => new SyncMigrationDataTypeProperty(
            "True/False",
            UmbConstants.PropertyEditors.Aliases.Boolean,
            "Integer",
            new List<PreValue>());

    [Test]
    public override void ConfigValueAsExpected()
    {
        var configFile = _migrator!.GetConfigValues(GetMigrationDataTypeProperty(), _context!);

        var expected = @"{
  ""Default"": false,
  ""LabelOff"": null,
  ""LabelOn"": null,
  ""ShowLabels"": false
}";

        Assert.AreEqual(expected, ConvertResultToJsonTestResult(configFile));

    }

    [TestCase("1", "1")]
    [TestCase("0", "0")]
    public override void ContentValueAsExpected(string value, string expected)
        => ContentValueAsExpectedBase(value, expected);

    [Test]
    public override void DatabaseTypeAsExpected()
        => DatabaseTypeAsExpectedBase("Integer");

    [Test]
    public override void EditorAliasAsExpected()
        => EditorAliasAsExpectedBase("Umbraco.TrueFalse");
}
