using Apps.SalesforceMarketing.Api;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.SalesforceMarketing.Handlers;

public class ContentBlockDataHandler(InvocationContext invocationContext) 
    : SalesforceInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var client = new SalesforceClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new RestRequest("asset/v1/content/assets", Method.Get);

        var filters = new List<string>
        {
            $"assetType.id in ({AssetTypeIds.HtmlBlock}, {AssetTypeIds.TextBlock}, {AssetTypeIds.FreeformBlock})"
        };

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            filters.Add($"name like '%{context.SearchString}%'");

        if (filters.Count != 0)
        {
            string filterQuery = string.Join(" AND ", filters);
            request.AddQueryParameter("$filter", filterQuery);
        }

        request.AddQueryParameter("$pageSize", "50");
        request.AddQueryParameter("$orderBy", "name asc");

        var response = await client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        return response.Items.Select(x => new DataSourceItem(x.Id, x.ToString()));
    }
}
