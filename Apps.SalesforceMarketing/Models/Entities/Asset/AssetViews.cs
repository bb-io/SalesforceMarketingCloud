using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetViews
{
    [JsonProperty("html")]
    public AssetView Html { get; set; }

    [JsonProperty("subjectline")]
    public AssetView SubjectLine { get; set; }

    [JsonProperty("text")]
    public AssetView Text { get; set; }

    [JsonProperty("preheader")]
    public AssetView Preheader { get; set; }
}
