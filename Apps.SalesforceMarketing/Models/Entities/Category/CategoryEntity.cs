using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Entities.Category;

public class CategoryEntity
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("parentId")]
    public int ParentId { get; set; }
}
