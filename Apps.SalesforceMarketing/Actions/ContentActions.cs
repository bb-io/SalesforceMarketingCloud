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

namespace Apps.SalesforceMarketing.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : SalesforceInvocable(invocationContext)
{
    [Action("Search content", Description = "Search content (emails and content blocks) with specific criteria")]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest input)
    {
        input.Validate();

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

        if (!string.IsNullOrEmpty(input.CategoryId))
        {
            conditions.Add(new JObject
            {
                ["property"] = "category.id",
                ["simpleOperator"] = "equals",
                ["value"] = input.CategoryId
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

        string subjectLine = entity.Views.SubjectLine.Content ?? "";
        if (!string.IsNullOrEmpty(subjectLine))
            htmlContent = HtmlHelper.InjectDivMetadata(htmlContent, subjectLine, BlackbirdMetadataIds.SubjectLine);

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlContent));
        var file = await fileManagementClient.UploadAsync(stream, "application/html", $"{entity.Name}.html");

        return new DownloadEmailResponse(file);
    }

    [Action("Upload email", Description = "Create new email from file")]
    public async Task<GetContentResponse> UploadEmail([ActionParameter] UploadEmailRequest input)
    {
        string html = await FileContentHelper.GetHtmlFromFile(fileManagementClient, input.Content);

        var (UpdatedHtml, ExtractedSubject) = HtmlHelper.ExtractAndDeleteDivMetadata(html, BlackbirdMetadataIds.SubjectLine);
        var cleanHtml = UpdatedHtml;
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