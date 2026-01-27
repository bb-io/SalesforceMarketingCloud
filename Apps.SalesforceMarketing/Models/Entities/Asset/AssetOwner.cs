using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetOwner
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
