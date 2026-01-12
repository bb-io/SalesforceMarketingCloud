using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Error;

public class ErrorResponse
{
    [JsonProperty("message")]
    public string? Message { get; set; }
}
