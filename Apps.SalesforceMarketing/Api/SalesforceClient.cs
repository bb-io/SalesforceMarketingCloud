using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models.Error;
using Apps.SalesforceMarketing.Models.Response;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

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

    public async Task<List<T>> PaginatePost<T>(RestRequest request)
    {
        var allItems = new List<T>();

        var bodyParam = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
        JObject jsonBody;

        if (bodyParam != null && bodyParam.Value != null)
        {
            jsonBody = bodyParam.Value is string s
                ? JObject.Parse(s)
                : JObject.FromObject(bodyParam.Value);
        }
        else
            jsonBody = new JObject();

        int page = 1;
        int pageSize = 50;

        if (jsonBody["page"] == null)
            jsonBody["page"] = new JObject{ ["pageSize"] = pageSize };

        while (true)
        {
            jsonBody["page"]!["page"] = page;

            var currentParam = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (currentParam != null)
                request.RemoveParameter(currentParam);

            request.AddStringBody(jsonBody.ToString(), DataFormat.Json);

            var response = await ExecuteWithErrorHandling<JObject>(request);
            var items = response["items"]?.ToObject<List<T>>();

            if (items == null || items.Count == 0) 
                break;

            allItems.AddRange(items);

            if (items.Count < pageSize) 
                break;

            page++;
        }

        return allItems;
    }

    public async Task<List<T>> PaginateGet<T>(RestRequest request)
    {
        var allItems = new List<T>();
        int page = 1;
        int pageSize = 50;

        while (true)
        {
            var pageParam = request.Parameters.FirstOrDefault(p => p.Name == "$page");
            if (pageParam != null) request.RemoveParameter(pageParam);

            var sizeParam = request.Parameters.FirstOrDefault(p => p.Name == "$pageSize");
            if (sizeParam != null) request.RemoveParameter(sizeParam);

            request.AddQueryParameter("$page", page.ToString());
            request.AddQueryParameter("$pageSize", pageSize.ToString());

            var response = await ExecuteWithErrorHandling<JObject>(request);

            var items = response["items"]?.ToObject<List<T>>();
            if (items == null || items.Count == 0)
                break;

            allItems.AddRange(items);

            if (items.Count < pageSize)
                break;

            page++;
        }

        return allItems;
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
        if (response.Content == null)
            throw new PluginApplicationException("Error - no content received from the server");

        var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
        if (error == null || error.Message == null)
            throw new PluginApplicationException("Error - could not deserialize an error message");

        throw new PluginApplicationException(error.Message);
    }
}