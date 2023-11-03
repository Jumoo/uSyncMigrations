using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Core.Serialization;

public class SyncMigrationsContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return base
            .CreateProperties(type, memberSerialization)
            .OrderBy(p => p.PropertyName)
            .ToList();
    }
}