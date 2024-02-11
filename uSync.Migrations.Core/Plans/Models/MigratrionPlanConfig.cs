﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Core.Plans.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigratrionPlanConfig
{
    public string[]? Remove { get; set; }

    public List<MigrationPlan> Plans { get; set; }
        = new List<MigrationPlan>();
}
