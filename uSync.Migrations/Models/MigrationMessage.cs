using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationMessage
{
    public MigrationMessage()
    {
        Message = string.Empty;
    }

    public MigrationMessage(string type, string name, MigrationMessageType messageType)
        : this()
    {
        ItemType = type;
        ItemName = name;

        MessageType = messageType;
    }

    public string ItemType { get; set; }
    public string ItemName { get; set; }

    public MigrationMessageType MessageType { get; set; }
    public string Message { get; set; }
}
