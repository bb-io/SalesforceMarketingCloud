using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Request;
using Apps.SalesforceMarketing.Models.Response.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext) : SalesforceInvocable(invocationContext)
{
    [Action("Search content", Description = "Search content with specific criteria")]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest input)
    {
        input.Validate();

        var conditions = new List<JObject>();

        if (input.CreatedFromDate.HasValue)
        {
            conditions.Add(new JObject
            {
                ["property"] = "createdDate",
                ["simpleOperator"] = "greaterThanOrEqual",
                ["value"] = input.CreatedFromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

        if (input.CreatedToDate.HasValue)
        {
            conditions.Add(new JObject
            {
                ["property"] = "createdDate",
                ["simpleOperator"] = "lessThanOrEqual",
                ["value"] = input.CreatedToDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

        if (input.UpdatedFromDate.HasValue)
        {
            conditions.Add(new JObject
            {
                ["property"] = "modifiedDate",
                ["simpleOperator"] = "greaterThanOrEqual",
                ["value"] = input.UpdatedFromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

        if (input.UpdatedToDate.HasValue)
        {
            conditions.Add(new JObject
            {
                ["property"] = "modifiedDate",
                ["simpleOperator"] = "lessThanOrEqual",
                ["value"] = input.UpdatedToDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post);

        var finalQuery = FilterHelper.BuildQueryTree(conditions);
        var body = new JObject();
        if (finalQuery != null)
            body["query"] = finalQuery;

        request.AddStringBody(body.ToString(), DataFormat.Json);
        var entities = await Client.PaginatePost<AssetEntity>(request);

        var wrappedItems = entities.Select(x => new GetContentResponse(x)).ToArray();
        return new SearchContentResponse(wrappedItems);
    }
}