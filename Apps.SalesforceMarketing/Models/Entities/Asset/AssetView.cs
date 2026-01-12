using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetView
{
    [JsonProperty("content")]
    public string Content { get; set; }
}
