using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetType
{
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
}
