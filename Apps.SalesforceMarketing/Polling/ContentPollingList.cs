using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Response.Content;
using Apps.SalesforceMarketing.Polling.Memory;
using Apps.SalesforceMarketing.Polling.Request;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.SalesforceMarketing.Polling;

[PollingEventList("Content")]
public class ContentPollingList(InvocationContext invocationContext) : SalesforceInvocable(invocationContext)
{
    [PollingEvent("On content created or updated", "On content created or updated")]
    public async Task<PollingEventResponse<DateTimeMemory, SearchContentResponse>> OnContentCreatedOrUpdated(
        PollingEventRequest<DateTimeMemory> pollingRequest,
        [PollingEventParameter] OnContentCreatedOrUpdatedRequest input)
    {
        var currentExecutionTime = DateTime.UtcNow;

        if (pollingRequest.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new(currentExecutionTime),
            };
        }

        input.ApplyDefaultValues();

        var categoryIds = await CategoryHelper.GetCategoryIds(
            Client,
            input.CategoryId,
            input.IncludeSubfolders
        );

        var query = new AssetFilterBuilder()
            .WhereIn("assetType.id", input.ContentTypes)
            .WhereGreaterOrEqual("modifiedDate", pollingRequest.Memory.LastInteractionDate)
            .WhereIn("category.id", categoryIds)
            .WhereMustContains("name", input.NameContains)
            .Build();

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post);

        var body = new JObject();
        if (query != null)
            body["query"] = query;

        request.AddStringBody(body.ToString(), DataFormat.Json);

        var entities = await Client.PaginatePost<AssetEntity>(request);
        entities = entities.FilterExcludedNames(input.NameDoesntContain, e => e.Name);

        if (!entities.Any())
        {
            return new()
            {
                FlyBird = false,
                Memory = new(currentExecutionTime),
            };
        }

        var wrappedItems = entities.Select(x => new GetContentResponse(x)).ToArray();
        return new()
        {
            FlyBird = true,
            Memory = new(currentExecutionTime),
            Result = new SearchContentResponse(wrappedItems),
        };
    }
}
