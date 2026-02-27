using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Identifiers.Optional;
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
            assetType = new { id = int.Parse(input.AssetTypeId) },
            content = finalContent,
            category = !string.IsNullOrEmpty(input.CategoryId) ? new { id = int.Parse(input.CategoryId) } : null
        };

        request.AddJsonBody(body);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(createdEntity);
    }

    [Action("Download email", Description = "Download email content")]
    public async Task<DownloadEmailResponse> DownloadEmail(
        [ActionParameter] EmailIdentifier emailId,
        [ActionParameter] DownloadEmailRequest input)
    {
        var request = new RestRequest($"asset/v1/content/assets/{emailId.EmailId}", Method.Get);
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);

        if (entity.Views?.Html == null)
        {
            throw new PluginMisconfigurationException(
                $"The asset '{entity.Name}' does not contain valid HTML content. " +
                "Please ensure you have selected a valid 'Content Builder Email' and not a Template, Content Block or Legacy Email"
            );
        }

        string htmlContent = entity.Views.Html.Content;
        htmlContent = await ContentBlockHelper.ExpandContentBlocks(htmlContent, Client, input.ContentBlockIdsToIgnore);

        string? subjectLine = entity.Views.SubjectLine?.Content;
        string? preheader = entity.Views.Preheader?.Content;
        htmlContent = HtmlHelper.InjectDiv(htmlContent, subjectLine, BlackbirdMetadataIds.SubjectLine);
        htmlContent = HtmlHelper.InjectDiv(htmlContent, preheader, BlackbirdMetadataIds.Preheader);
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

        var (htmlWithoutSubject, ExtractedSubject) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.SubjectLine);
        html = htmlWithoutSubject;

        var (htmlWithoutPreheader, ExtractedPreheader) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.Preheader);
        html = htmlWithoutPreheader;

        if (input.ScriptVariableNames != null && input.ScriptVariableValues != null)
            html = ScriptHelper.UpdateScriptVariables(html, input.ScriptVariableNames, input.ScriptVariableValues);

        html = ScriptHelper.RestoreScriptBlocks(html);
        html = ScriptHelper.RestoreVariables(html, BlackbirdMetadataIds.SubjectLine);

        html = await ContentBlockHelper.RestoreContentBlocks(html, Client, input.EmailName, input.CategoryId);

        string subject = 
            input.SubjectLine ?? 
            ExtractedSubject ?? 
            throw new PluginMisconfigurationException(
                "Email subject line is not found in the input file. Provide it in the input or include it in the file"
            );
        string? preheader = ExtractedPreheader;
        string emailName = string.IsNullOrEmpty(input.EmailName) ? input.Content.Name : input.EmailName;

        var request = new RestRequest("asset/v1/content/assets", Method.Post);
        var body = new
        {
            name = emailName,
            assetType = new { id = AssetTypeIds.HtmlEmail },
            views = new
            {
                html = new { content = html },
                subjectline = new { content = subject },
                preheader = new { content = preheader },
            },
            category = !string.IsNullOrEmpty(input.CategoryId) ? new { id = int.Parse(input.CategoryId) } : null
        };
        request.AddJsonBody(body);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new GetContentResponse(createdEntity);
    }

    [Action("Update email", Description = "Update existing email from file")]
    public async Task<GetContentResponse> UpdateEmail(
        [ActionParameter] OptionalEmailIdentifier emailInput,
        [ActionParameter] UpdateEmailRequest input)
    {
        string html = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        string emailId =
            HtmlHelper.ExtractHeadMetadata(html, BlackbirdMetadataIds.EmailId) ??
            emailInput.EmailId ??
            throw new PluginMisconfigurationException(
                "Email ID is not found in the input file. Provide it in the input or include it in the file"
            );

        var (htmlWithoutSubject, ExtractedSubject) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.SubjectLine);
        html = htmlWithoutSubject;

        var (htmlWithoutPreheader, ExtractedPreheader) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.Preheader);
        html = htmlWithoutPreheader;

        html = ScriptHelper.RestoreScriptBlocks(html);
        html = ScriptHelper.RestoreVariables(html, BlackbirdMetadataIds.SubjectLine);

        html = await ContentBlockHelper.UpdateContentBlocks(html, Client);

        string? subjectLine = string.IsNullOrEmpty(input.SubjectLine) ? ExtractedSubject : input.SubjectLine;

        var request = new RestRequest($"asset/v1/content/assets/{emailId}", Method.Patch);
        
        var views = new Dictionary<string, object> { { "html", new { content = html } } };

        if (!string.IsNullOrEmpty(subjectLine))
            views.Add("subjectline", new { content = subjectLine });

        if (!string.IsNullOrEmpty(ExtractedPreheader))
            views.Add("preheader", new { content = ExtractedPreheader });

        var body = new { views };
        request.AddJsonBody(body);

        var updatedEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new GetContentResponse(updatedEntity);
    }   
}