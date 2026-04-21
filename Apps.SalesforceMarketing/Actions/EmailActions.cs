using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Extensions;
using Apps.SalesforceMarketing.Helpers;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Identifiers.Optional;
using Apps.SalesforceMarketing.Models.Request.Email;
using Apps.SalesforceMarketing.Models.Response.Content;
using Apps.SalesforceMarketing.Models.Response.Email;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Text;

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Emails")]
public class EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : SalesforceInvocable(invocationContext)
{
    [Action("Find email by name", Description = "Find email ID using its name")]
    public async Task<FindEmailByNameResponse> FindEmailByName([ActionParameter] FindEmailByNameRequest input)
    {
        input.ApplyDefaultValues();

        var categoryIds = await CategoryHelper.GetCategoryIds(
            Client,
            input.CategoryId,
            input.IncludeSubfolders
        );

        var query = new AssetFilterBuilder()
            .WhereEquals("assetType.id", AssetTypeIds.HtmlEmail)
            .WhereIn("category.id", categoryIds)
            .WhereEquals("name", input.EmailName)
            .BuildPayload();

        var request = new RestRequest("asset/v1/content/assets/query", Method.Post)
            .AddStringBody(query.ToString(), DataFormat.Json);

        var response = await Client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        var entity = response.Items.FirstOrDefault();
        return new(entity?.Id);
    }

    [Action("Get email details", Description = "Get details of a specific email")]
    public async Task<GetEmailDetailsResponse> GetEmailDetails([ActionParameter] EmailIdentifier emailId)
    {
        var request = new RestRequest($"asset/v1/content/assets/{emailId.EmailId}", Method.Get);
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(entity);
    }

    [Action("Download email", Description = "Download email content")]
    public async Task<DownloadEmailResponse> DownloadEmail(
        [ActionParameter] EmailIdentifier emailId,
        [ActionParameter] DownloadEmailRequest input)
    {
        input.ApplyDefaultValues().Validate();

        var request = new RestRequest($"asset/v1/content/assets/{emailId.EmailId}", Method.Get);
        var entity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);

        if (entity.Views == null)
        {
            throw new PluginMisconfigurationException(
                $"The asset '{entity.Name}' does not contain any views. Please ensure your email has content");
        }
        
        string? htmlContent = input.DownloadHtmlEmailContent == true ? entity.Views.Html?.Content : null;
        string? plaintextContent = input.DownloadPlaintextEmailContent == true ? entity.Views.Text?.Content : null;

        string resultContent = EmailSplitter.MergeViews(htmlContent, plaintextContent);
        resultContent = await ContentBlockHelper.ExpandContentBlocks(
            resultContent,
            Client,
            input.ContentBlockIdsToIgnore,
            input.IgnoreBlocksInFolderIds);

        string? subjectLine = entity.Views.SubjectLine?.Content;
        string? preheader = entity.Views.Preheader?.Content;

        resultContent = HtmlHelper.InjectDiv(resultContent, subjectLine, BlackbirdMetadataIds.SubjectLine);
        resultContent = HtmlHelper.InjectDiv(resultContent, preheader, BlackbirdMetadataIds.Preheader);
        resultContent = HtmlHelper.InjectHeadMetadata(resultContent, entity.Id, BlackbirdMetadataIds.EmailId);

        resultContent = ScriptHelper.ExtractVariables(resultContent, ["@subjectLine"], BlackbirdMetadataIds.SubjectLine);

        if (input.ExtractAllScriptVariables == true)
        {
            var allVariables = ScriptHelper.FindVariablesWithStringValues(resultContent);
            var variablesWithAt = (input.ScriptVariablesToIgnore ?? []).Select(v => v.EnsureStartsWith("@"));

            var varsToIgnore = new HashSet<string>(variablesWithAt, StringComparer.OrdinalIgnoreCase)
            {
                "@subjectLine"
            };

            var varsToExtract = allVariables.Where(v => !varsToIgnore.Contains(v)).ToList();
            if (varsToExtract.Count != 0)
                resultContent = ScriptHelper.ExtractVariables(resultContent, varsToExtract);
        }
        else if (input.ScriptVariablesToExtract != null && input.ScriptVariablesToExtract.Any())
            resultContent = ScriptHelper.ExtractVariables(resultContent, input.ScriptVariablesToExtract);

        resultContent = ScriptHelper.WrapAmpScriptBlocks(resultContent);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultContent));
        var file = await fileManagementClient.UploadAsync(stream, "application/html", entity.Name.ToHtmlFileName());
        return new(file);
    }

    [Action("Create new email", Description = "Create new email from file")]
    public async Task<GetContentResponse> CreateEmail([ActionParameter] UploadEmailRequest input)
    {
        string rawHtml = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        var processedData = ProcessBaseEmailHtml(rawHtml, input.ScriptVariableNames, input.ScriptVariableValues);
        string html = processedData.ProcessedHtml;

        string baseEmailName = string.IsNullOrEmpty(input.EmailName) ? input.Content.Name : input.EmailName;
        string finalEmailName = string.IsNullOrWhiteSpace(input.ContentSuffix)
            ? baseEmailName
            : $"{baseEmailName} {input.ContentSuffix}".Trim();

        html = await ContentBlockHelper.RestoreContentBlocks(
            html,
            Client,
            input.EmailName,
            input.CategoryId,
            input.CreateContentBlocksInOriginalFolder ?? false,
            input.ContentSuffix);

        var extractedViews = EmailSplitter.ExtractViews(html);
        string subject =
            input.SubjectLine ??
            processedData.ExtractedSubject ??
            throw new PluginMisconfigurationException(
                "Email subject line is not found in the input file. Provide it in the input or include it in the file"
            );

        var body = new AssetEntity
        {
            Name = finalEmailName,
            AssetType = new AssetType { Id = AssetTypeIds.HtmlEmail },
            Category = string.IsNullOrEmpty(input.CategoryId) ? null : new AssetCategory { Id = input.CategoryId },
            Views = new AssetViews
            {
                SubjectLine = new AssetView { Content = subject },
                Html = string.IsNullOrEmpty(extractedViews.HtmlView) ? null : new AssetView { Content = extractedViews.HtmlView },
                Text = string.IsNullOrEmpty(extractedViews.PlaintextView) ? null : new AssetView { Content = extractedViews.PlaintextView },
                Preheader = string.IsNullOrEmpty(processedData.ExtractedPreheader) ? null : new AssetView { Content = processedData.ExtractedPreheader }
            }
        }; 
        var request = new RestRequest("asset/v1/content/assets", Method.Post)
            .WithJsonBody(body, JsonSettings.Settings);

        var createdEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(createdEntity);
    }

    [Action("Update email", Description = "Update existing email from file")]
    public async Task<GetContentResponse> UpdateEmail(
        [ActionParameter] OptionalEmailIdentifier emailInput,
        [ActionParameter] UpdateEmailRequest input)
    {
        string rawHtml = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        string emailId =
            emailInput.EmailId ??
            HtmlHelper.ExtractHeadMetadata(rawHtml, BlackbirdMetadataIds.EmailId) ??
            throw new PluginMisconfigurationException(
                "Email ID is not found in the input file. Provide it in the input or include it in the file");

        var processedData = ProcessBaseEmailHtml(rawHtml, input.ScriptVariableNames, input.ScriptVariableValues);
        string html = processedData.ProcessedHtml;

        html = await ContentBlockHelper.UpdateContentBlocks(html, Client);

        var extractedViews = EmailSplitter.ExtractViews(html);
        string? subjectLine = string.IsNullOrEmpty(input.SubjectLine) ? processedData.ExtractedSubject : input.SubjectLine;
        
        var body = new AssetEntity
        {
            Views = new AssetViews
            {
                SubjectLine = string.IsNullOrEmpty(subjectLine) ? null : new AssetView { Content = subjectLine },
                Html = string.IsNullOrEmpty(extractedViews.HtmlView) ? null : new AssetView { Content = extractedViews.HtmlView },
                Text = string.IsNullOrEmpty(extractedViews.PlaintextView) ? null : new AssetView { Content = extractedViews.PlaintextView },
                Preheader = string.IsNullOrEmpty(processedData.ExtractedPreheader) ? null : new AssetView { Content = processedData.ExtractedPreheader }
            }
        }; 
        var request = new RestRequest($"asset/v1/content/assets/{emailId}", Method.Patch)
            .WithJsonBody(body, JsonSettings.Settings);

        var updatedEntity = await Client.ExecuteWithErrorHandling<AssetEntity>(request);
        return new(updatedEntity);
    }

    private static (string ProcessedHtml, string? ExtractedSubject, string? ExtractedPreheader) ProcessBaseEmailHtml(
        string html,
        IEnumerable<string>? scriptNames,
        IEnumerable<string>? scriptValues)
    {
        var (htmlWithoutSubject, extractedSubject) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.SubjectLine);
        html = htmlWithoutSubject;

        var (htmlWithoutPreheader, extractedPreheader) = HtmlHelper.ExtractAndDeleteDiv(html, BlackbirdMetadataIds.Preheader);
        html = htmlWithoutPreheader;

        if (scriptNames != null && scriptValues != null)
            html = ScriptHelper.UpdateScriptVariables(html, scriptNames, scriptValues);

        html = ScriptHelper.RestoreScriptBlocks(html);
        html = ScriptHelper.RestoreVariables(html, BlackbirdMetadataIds.AmpScriptVar);
        html = ScriptHelper.RestoreVariables(html, BlackbirdMetadataIds.SubjectLine);

        return (html, extractedSubject, extractedPreheader);
    }
}
