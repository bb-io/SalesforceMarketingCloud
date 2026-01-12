using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class ModifiedBy
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
