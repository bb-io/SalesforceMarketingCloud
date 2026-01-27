using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetCategory
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}
