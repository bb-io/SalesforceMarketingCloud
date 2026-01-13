using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Handlers;
using Apps.SalesforceMarketing.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class CreateContentBlockRequest
{
    [Display("Name")]
    public string Name { get; set; }

    [Display("Block type"), StaticDataSource(typeof(ContentBlockTypeDataHandler))]
    public string AssetTypeId { get; set; }

    [Display("Text content")]
    public string? TextContent { get; set; }

    [Display("File content")]
    public FileReference? FileContent { get; set; }

    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(TextContent) && FileContent == null)
            throw new PluginMisconfigurationException("At least one type of content should be specified, either Text or File");

        if (!string.IsNullOrEmpty(TextContent) && FileContent != null)
            throw new PluginMisconfigurationException("Only one type of content should be specified, either Text or File");

        if (FileContent != null && AssetTypeId != AssetTypeIds.HtmlBlock)
            throw new PluginMisconfigurationException("File content upload is supported with HTML blocks only");
    }
}
