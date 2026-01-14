using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Error;

public class ValidationError
{
    [JsonProperty("message")]
    public string? Message { get; set; }
}
