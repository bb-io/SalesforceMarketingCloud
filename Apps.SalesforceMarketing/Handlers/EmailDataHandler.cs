using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.SalesforceMarketing.Handlers;

public class EmailDataHandler(InvocationContext invocationContext)
    : SalesforceInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var assetTypes = new[] { "htmlemail", "templatebasedemail" };
        var query = new AssetFilterBuilder()
            .WhereIn("assetType.name", assetTypes)
            .WhereLike("name", context.SearchString)
            .Build();

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post);

        var body = new JObject();
        if (query != null)
            body["query"] = query;

        request.AddStringBody(body.ToString(), DataFormat.Json);
        var entities = await Client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        return entities.Items.Select(x => new DataSourceItem(x.Id, x.ToString()));
    }
}