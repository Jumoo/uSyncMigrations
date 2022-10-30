using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationMessage
{
    public MigrationMessage(string type, string name, MigrationMessageType messageType)
    {
        ItemType = type;
        ItemName = name;
        Message = string.Empty;
        MessageType = messageType;
    }

    public string ItemType { get; set; }

    public string ItemName { get; set; }

    public string Message { get; set; }

    public MigrationMessageType MessageType { get; set; }
}
