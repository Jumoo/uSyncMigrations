﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Plans;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public interface ISyncMigrationPlan : IDiscoverable
{
    int Order { get; }

    string Name { get; }

    string Icon { get; }

    string Description { get; }

    MigrationOptions Options { get; }
}

/// <summary>
///  for legacy - but this a beta, so not really going to stay
/// </summary>
[Obsolete("Use ISyncMigrationPlan, this will not be in the release version")]
public interface ISyncMigrationProfile : ISyncMigrationPlan
{

}