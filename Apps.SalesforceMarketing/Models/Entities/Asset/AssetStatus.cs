using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetStatus
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
