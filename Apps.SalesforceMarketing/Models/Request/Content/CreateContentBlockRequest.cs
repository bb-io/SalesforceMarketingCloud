using Apps.SalesforceMarketing.Handlers;
using Apps.SalesforceMarketing.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class CreateContentBlockRequest
{
    [Display("Content block name", Description = "Overrides the default file name")]
    public string BlockName { get; set; }

    [Display("Content block type ID"), StaticDataSource(typeof(ContentBlockTypeDataHandler))]
    public string BlockTypeId { get; set; }

    [Display("Content")]
    public FileReference Content { get; set; }

    [Display("Content suffix", 
        Description = "A custom ending for the name of the newly created content block and its nested blocks")]
    public string? ContentSuffix { get; set; }

    [Display("Create nested content blocks in their original folder", Description = "False by default")]
    public bool? CreateContentBlocksInOriginalFolder { get; set; }

    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }
}
