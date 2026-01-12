using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Models.Response;

public class AuthResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}
