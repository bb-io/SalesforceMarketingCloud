using Apps.SalesforceMarketing.Extensions;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Request.Block;
using Apps.SalesforceMarketing.Models.Response.Block;
using Apps.SalesforceMarketing.Models.Response.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Text;

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Content blocks")]
public class ContentBlockActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : SalesforceInvocable(invocationContext)
{
    [Action("Download content block", Description = "Download content block content")]
    public async Task<DownloadContentBlockResponse> DownloadContentBlock(
        [ActionParameter] ContentBlockIdentifier contentBlockId,
        [ActionParameter] DownloadContentBlockRequest input)
    {
        input.ApplyDefaultValues();

        var request = new RestRequest($"asset/v1/content/assets/{contentBlockId.ContentBlockId}");
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);

        string content = entity.Content ??
            throw new PluginMisconfigurationException("This content block does not have any content");

        if (input.IgnoreAllNestedBlocks == false)
        {
            content = await ContentBlockHelper.ExpandContentBlocks(
                content,
                Client,
                input.ContentBlockIdsToIgnore,
                input.IgnoreBlocksInFolderIds);
        }

        content = ContentBlockHelper.WrapBlockInTag(entity.Id, content);
        content = ScriptHelper.WrapAmpScriptBlocks(content);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var file = await fileManagementClient.UploadAsync(stream, "application/html", entity.Name.ToHtmlFileName());

        return new(file);
    }

    [Action("Create content block", Description = "Create a new content block")]
    public async Task<GetContentResponse> CreateContentBlock([ActionParameter] CreateContentBlockRequest input)
    {
        string content = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        content = ContentBlockHelper.UnwrapBlockFromTag(content);
        content = await ContentBlockHelper.RestoreContentBlocks(
            content,
            Client,
            input.BlockName,
            input.CategoryId,
            input.CreateContentBlocksInOriginalFolder ?? false,
            input.ContentSuffix);

        string finalBlockName = string.IsNullOrWhiteSpace(input.ContentSuffix)
            ? (input.BlockName ?? input.Content.Name)
            : $"{input.BlockName ?? input.Content.Name} {input.ContentSuffix}".Trim();

        var request = new RestRequest("asset/v1/content/assets", Method.Post);
        var body = new
        {
            name = finalBlockName,
            assetType = new { id = int.Parse(input.BlockTypeId) },
            views = new { html = new { content } },
            category = !string.IsNullOrEmpty(input.CategoryId) ? new { id = int.Parse(input.CategoryId) } : null
        };

        request.AddJsonBody(body);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(createdEntity);
    }
}
