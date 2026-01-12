using RestSharp;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;

namespace Apps.SalesforceMarketing.Handlers;

public class AssetDataHandler(InvocationContext invocationContext) 
    : SalesforceInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var conditions = new List<JObject>()
        {
            new() {
                ["property"] = "assetType.name",
                ["simpleOperator"] = "in",
                ["value"] = new JArray
                { 
                    // Emails
                    "htmlemail",
                    "templatebasedemail",
                    "textonlyemail",                
                    // Content blocks
                    "textblock",
                    "imageblock",
                    "freeformblock",
                    "htmlblock",
                    "buttonblock",
                    "socialfollowblock",
                    "socialshareblock"
                }
            }
        }; 
        
        if (!string.IsNullOrEmpty(context.SearchString))
        {
            conditions.Add(new JObject
            {
                ["property"] = "name",
                ["simpleOperator"] = "like",
                ["value"] = context.SearchString
            });
        }

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post);

        var finalQuery = FilterHelper.BuildQueryTree(conditions);
        var body = new JObject();
        if (finalQuery != null)
            body["query"] = finalQuery;

        request.AddStringBody(body.ToString(), DataFormat.Json);
        var entities = await Client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        return entities.Items.Select(x => new DataSourceItem(x.Id, x.Name));
    }
}
