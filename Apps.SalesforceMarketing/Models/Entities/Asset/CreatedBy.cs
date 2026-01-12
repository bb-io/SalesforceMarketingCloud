using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class CreatedBy
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
