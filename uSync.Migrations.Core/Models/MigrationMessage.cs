using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Core.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationMessage
{
    public MigrationMessage(string itemType, string itemName, MigrationMessageType messageType)
        : this(itemType, itemName, string.Empty, messageType) { }

	public MigrationMessage(string itemType, string itemName, string message, MigrationMessageType messageType)
	{
		ItemType = itemType;
        ItemName = itemName;
        Message = message;
        MessageType = messageType;
    }

    public string ItemType { get; set; }

    public string ItemName { get; set; }

    public string Message { get; set; }

    public MigrationMessageType MessageType { get; set; }
}
