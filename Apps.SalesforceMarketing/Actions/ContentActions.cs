using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Request.Content;
using Apps.SalesforceMarketing.Models.Response.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : SalesforceInvocable(invocationContext)
{
    [Action("Search content", Description = "Search content (emails and content blocks) with specific criteria")]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest input)
    {
        input.Validate();

        var assetTypes = new[] {
            "htmlemail", "templatebasedemail", "textblock", 
            "freeformblock", "htmlblock",
        };

        var query = new AssetFilterBuilder()
            .WhereIn("assetType.name", assetTypes)
            .WhereGreaterOrEqual("createdDate", input.CreatedFromDate)
            .WhereLessOrEqual("createdDate", input.CreatedToDate)
            .WhereGreaterOrEqual("modifiedDate", input.UpdatedFromDate)
            .WhereLessOrEqual("modifiedDate", input.UpdatedToDate)
            .WhereEquals("category.id", input.CategoryId)
            .Build();

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post);

        var body = new JObject();
        if (query != null)
            body["query"] = query;

        request.AddStringBody(body.ToString(), DataFormat.Json);
        var entities = await Client.PaginatePost<AssetEntity>(request);

        var wrappedItems = entities.Select(x => new GetContentResponse(x)).ToArray();
        return new SearchContentResponse(wrappedItems);
    }

    [Action("Get email details", Description = "Get details of a specific email")]
    public async Task<GetEmailDetailsResponse> GetEmailDetails([ActionParameter] EmailIdentifier emailId)
    {
        var request = new RestRequest($"asset/v1/content/assets/{emailId.EmailId}", Method.Get);
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(entity);
    }

    [Action("Create content block", Description = "Create a new content block")]
    public async Task<GetContentResponse> CreateContentBlock([ActionParameter] CreateContentBlockRequest input)
    {
        input.Validate();

        string finalContent;
        if (input.FileContent != null)
            finalContent = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.FileContent);
        else
            finalContent = input.TextContent!;

        var request = new RestRequest("asset/v1/content/assets", Method.Post);
        var body = new
        {
            name = input.Name,
            assetType = new
            {
                id = int.Parse(input.AssetTypeId),
            },
            content = finalContent,
            category = !string.IsNullOrEmpty(input.CategoryId) ? new { id = int.Parse(input.CategoryId) } : null
        };

        request.AddJsonBody(body);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(createdEntity);
    }

    [Action("Download email", Description = "Download email content")]
    public async Task<DownloadEmailResponse> DownloadEmail([ActionParameter] EmailIdentifier emailId)
    {
        var request = new RestRequest($"asset/v1/content/assets/{emailId.EmailId}", Method.Get);
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);

        string htmlContent = entity.Views.Html.Content;
        string? subjectLine = entity.Views.SubjectLine?.Content;

        if (!string.IsNullOrEmpty(subjectLine))
            htmlContent = HtmlHelper.InjectDivMetadata(htmlContent, subjectLine, BlackbirdMetadataIds.SubjectLine);

        htmlContent = HtmlHelper.InjectHeadMetadata(htmlContent, entity.Id, BlackbirdMetadataIds.EmailId);
        htmlContent = ScriptHelper.ExtractVariables(htmlContent, "@subjectLine", BlackbirdMetadataIds.SubjectLine);
        htmlContent = ScriptHelper.WrapAmpScriptBlocks(htmlContent);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        var file = await fileManagementClient.UploadAsync(stream, "application/html", $"{entity.Name}.html");

        return new DownloadEmailResponse(file);
    }

    [Action("Create new email", Description = "Create new email from file")]
    public async Task<GetContentResponse> CreateEmail([ActionParameter] UploadEmailRequest input)
    {
        string html = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        var (UpdatedHtml, ExtractedSubject) = HtmlHelper.ExtractAndDeleteDivMetadata(html, BlackbirdMetadataIds.SubjectLine);
        string cleanHtml = UpdatedHtml;

        if (input.ScriptVariableNames != null && input.ScriptVariableValues != null)
        {
            cleanHtml = ScriptHelper.UpsertScriptVariables(
                cleanHtml,
                input.ScriptVariableNames,
                input.ScriptVariableValues
            );
        }
        cleanHtml = ScriptHelper.UnwrapAmpScriptBlocks(cleanHtml);
        cleanHtml = ScriptHelper.RestoreVariables(cleanHtml, BlackbirdMetadataIds.SubjectLine);

        string subject = 
            input.SubjectLine ?? 
            ExtractedSubject ?? 
            throw new PluginMisconfigurationException(
                "Email subject is not found in the input file. Provide it in the input or include it in the file"
            );

        string emailName = string.IsNullOrEmpty(input.EmailName) ? input.Content.Name : input.EmailName;

        var request = new RestRequest("asset/v1/content/assets", Method.Post);
        var body = new
        {
            name = emailName,
            assetType = new
            {
                id = 208,   // htmlemail
            },
            views = new
            {
                html = new { content = cleanHtml },
                subjectline = new { content = subject }
            },
            category = !string.IsNullOrEmpty(input.CategoryId) ? new { id = int.Parse(input.CategoryId) } : null
        };
        request.AddJsonBody(body);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new GetContentResponse(createdEntity);
    }
}