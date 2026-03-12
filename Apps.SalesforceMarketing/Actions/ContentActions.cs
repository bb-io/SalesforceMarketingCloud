using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Request.Content;
using Apps.SalesforceMarketing.Models.Response.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : SalesforceInvocable(invocationContext)
{
    [Action("Search content", Description = "Search content (emails and content blocks) with specific criteria")]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest input)
    {
        input.Validate().ApplyDefaultValues();

        var categoryIds = await CategoryHelper.GetCategoryIds(
            Client,
            input.CategoryId,
            input.IncludeSubfolders
        );

        var query = new AssetFilterBuilder()
            .WhereIn("assetType.id", input.ContentTypes)
            .WhereGreaterOrEqual("createdDate", input.CreatedFromDate)
            .WhereLessOrEqual("createdDate", input.CreatedToDate)
            .WhereGreaterOrEqual("modifiedDate", input.UpdatedFromDate)
            .WhereLessOrEqual("modifiedDate", input.UpdatedToDate)
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

        var wrappedItems = entities.Select(x => new GetContentResponse(x)).ToArray();
        return new(wrappedItems);
    }

    [Action("Get content ID from a file", Description = "Get content ID from file metadata")]
    public async Task<GetContentIdFromFileResponse> GetContentIdFromFile(
        [ActionParameter] GetContentIdFromFileRequest input)
    {
        string html = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);
        string contentId = HtmlHelper.ExtractContentId(html) ?? 
            throw new PluginMisconfigurationException(
                $"No content ID was found in the input file. " +
                $"Known content metadata identifiers: {string.Join(" ;", BlackbirdMetadataIds.ContentTypeIds)}");

        return new(contentId);
    }
}