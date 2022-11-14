using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using NUglify;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Validation;

public interface ISyncMigrationValidator : IDiscoverable
{
    IEnumerable<MigrationMessage> Validate(MigrationOptions options);
}
