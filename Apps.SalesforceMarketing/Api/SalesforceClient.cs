using RestSharp;
using Newtonsoft.Json;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models.Response;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.SalesforceMarketing.Api;

public class SalesforceClient : BlackBirdRestClient
{
    public SalesforceClient(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new()
    {
        BaseUrl = new Uri($"https://{creds.Get(CredsNames.Subdomain).Value}.rest.marketingcloudapis.com/"),
    })
    {
        string token = GetAccessToken(creds).GetAwaiter().GetResult();
        this.AddDefaultHeader("Authorization", $"Bearer {token}");
    }

    private static async Task<string> GetAccessToken(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string subdomain = creds.Get(CredsNames.Subdomain).Value;
        string authUrl = $"https://{subdomain}.auth.marketingcloudapis.com/v2/token";

        using var authClient = new RestClient(authUrl);
        var request = new RestRequest { Method = Method.Post };
        request.AddJsonBody(new
        {
            grant_type = "client_credentials",
            client_id = creds.Get(CredsNames.ClientId).Value,
            client_secret = creds.Get(CredsNames.ClientSecret).Value,
            account_id = creds.Get(CredsNames.AccountId).Value
        });

        var response = await authClient.ExecuteAsync(request);
        if (!response.IsSuccessful || response.Content == null)
            throw new PluginApplicationException($"Failed to authenticate. {response.Content}");

        var data = JsonConvert.DeserializeObject<AuthResponse>(response.Content) 
            ?? throw new PluginApplicationException("Auth response received, could not deserialize access token");
        return data.AccessToken;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject(response.Content);
        var errorMessage = "";

        throw new PluginApplicationException(errorMessage);
    }
}