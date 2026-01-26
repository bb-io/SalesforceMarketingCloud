using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Asset;

public class AssetEntity
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("customerKey")]
    public string CustomerKey { get; set; }

    [JsonProperty("assetType")]
    public AssetType AssetType { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("owner")]
    public AssetOwner Owner { get; set; }

    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("createdBy")]
    public CreatedBy CreatedBy { get; set; }

    [JsonProperty("modifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [JsonProperty("modifiedBy")]
    public ModifiedBy? ModifiedBy { get; set; }

    [JsonProperty("status")]
    public AssetStatus Status { get; set; }

    [JsonProperty("views")]
    public AssetViews Views { get; set; }

    [JsonProperty("category")]
    public AssetCategory Category { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    public override string ToString()
    {
        string name = Name.Trim();
        string id = Id;
        string? category = Category.Name;

        if (!string.IsNullOrEmpty(category))
            return $"{name} (Folder: {category})";

        return $"{name} (ID: {id})";
    }
}
