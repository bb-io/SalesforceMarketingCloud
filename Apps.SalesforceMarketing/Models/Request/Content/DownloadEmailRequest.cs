using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class DownloadEmailRequest
{
    [Display("Content block IDs to ignore"), DataSource(typeof(ContentBlockDataHandler))]
    public IEnumerable<string>? ContentBlockIdsToIgnore { get; set; }

    [Display("Ignore content blocks in categories", 
        Description = "Category (folder) IDs containing content blocks that should be ignored.")]
    [FileDataSource(typeof(CategoryDataHandler))]
    public IEnumerable<string>? IgnoreBlocksInFolderIds { get; set; }

    [Display("AMPscript variables to extract",
        Description = "List of variable names (e.g. @VarName or VarName) to extract into translatable <div> tags.")]
    public IEnumerable<string>? ScriptVariablesToExtract { get; set; }
}
