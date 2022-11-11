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

    [TestCase("Two,Three", @"Two,
Three")]
    public override void ContentValueAsExpeceted(string value, string expected)
        => ContentValueAsExpecetedBase(value, expected);

    [Test]
    public override void DatabaseTypeAsExpected()
        => DatabaseTypeAsExpectedBase("Nvarchar");

    [Test]
    public override void EditorAliasAsExpected()
        => EditorAliasAsExpectedbase(UmbConstants.PropertyEditors.Aliases.CheckBoxList);

    protected override SyncMigrationContentProperty GetMigrationContentProperty(string value)
        => new SyncMigrationContentProperty(UmbConstants.PropertyEditors.Aliases.CheckBoxList, value);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty()
        => new SyncMigrationDataTypeProperty(UmbConstants.PropertyEditors.Aliases.CheckBoxList, 
            "Nvarchar", 
            new List<PreValue>
            {
                new PreValue { SortOrder = 1, Alias = "0", Value = "One"},
                new PreValue { SortOrder = 1, Alias = "1", Value = "Twe"},
                new PreValue { SortOrder = 1, Alias = "2", Value = "Three"},
            });
}
