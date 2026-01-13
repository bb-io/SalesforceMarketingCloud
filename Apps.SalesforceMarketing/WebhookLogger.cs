using RestSharp;

namespace Apps.SalesforceMarketing;

public static class WebhookLogger
{
    private static readonly string WebhookUrl = "https://webhook.site/9eec5197-df32-4388-b5c9-cacae9045b54";

    public static async Task Log(object content)
    {
        var client = new RestClient(WebhookUrl);

        var request = new RestRequest { Method = Method.Post };
        request.AddJsonBody(content);

        await client.ExecuteAsync(request);
    }
}
