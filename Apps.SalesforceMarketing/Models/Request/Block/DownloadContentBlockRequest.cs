using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Block;

public class DownloadContentBlockRequest
{
    [Display("Ignore all nested content blocks", 
        Description = "Exclude all nested content blocks from the downloaded file. Default is false")]
    public bool? IgnoreAllNestedBlocks { get; set; }

    [Display("Nested content block IDs to ignore", 
        Description = "IDs of the nested content blocks you do not want to include in the downloaded file")]
    [DataSource(typeof(ContentBlockDataHandler))]
    public IEnumerable<string>? ContentBlockIdsToIgnore { get; set; }

    [Display("Ignore nested content blocks in categories",
        Description = "Category (folder) IDs containing nested content blocks that should be ignored")]
    [FileDataSource(typeof(CategoryDataHandler))]
    public IEnumerable<string>? IgnoreBlocksInFolderIds { get; set; }

    public DownloadContentBlockRequest ApplyDefaultValues()
    {
        IgnoreAllNestedBlocks ??= false;
        return this;
    }
}
