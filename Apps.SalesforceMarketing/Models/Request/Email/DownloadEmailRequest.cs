using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Email;

public class DownloadEmailRequest
{
    [Display("Download HTML email content", Description = "Default is true")]
    public bool? DownloadHtmlEmailContent { get; set; }

    [Display("Download plain text email content", Description = "Default is false")]
    public bool? DownloadPlaintextEmailContent { get; set; }

    [Display("Content block IDs to ignore"), DataSource(typeof(ContentBlockDataHandler))]
    public IEnumerable<string>? ContentBlockIdsToIgnore { get; set; }

    [Display("Ignore content blocks in categories", 
        Description = "Category (folder) IDs containing content blocks that should be ignored.")]
    [FileDataSource(typeof(CategoryDataHandler))]
    public IEnumerable<string>? IgnoreBlocksInFolderIds { get; set; }

    [Display("Specific AMPscript variables to extract",
        Description = "List of variable names (e.g. @VarName or VarName) to extract into translatable <div> tags.")]
    public IEnumerable<string>? ScriptVariablesToExtract { get; set; }

    [Display("Extract all AMPscript variables", 
        Description = "Extract all AMPscript variables into translatable <div> tags. Default is false.")]
    public bool? ExtractAllScriptVariables { get; set; }

    [Display("Specific AMPscript variables to ignore", 
        Description = "When extracting all variables, specify which ones should not be placed in translatable <div> tags.")]
    public IEnumerable<string>? ScriptVariablesToIgnore { get; set; }

    public DownloadEmailRequest ApplyDefaultValues()
    {
        ExtractAllScriptVariables ??= false;
        DownloadHtmlEmailContent ??= true;
        DownloadPlaintextEmailContent ??= false;
        return this;
    }

    public DownloadEmailRequest Validate()
    {
        if (DownloadHtmlEmailContent == false && DownloadPlaintextEmailContent == false)
        {
            throw new PluginMisconfigurationException(
                "At least one email content type must be specified for download - either HTML or plain text. " +
                "Please set the 'Download HTML email content' or 'Download plain text email content' input to true.");
        }

        return this;
    }
}
