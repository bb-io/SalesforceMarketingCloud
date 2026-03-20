using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetCategory
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}
