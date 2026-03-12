using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Email;

public class UploadEmailRequest
{
    [Display("Content")]
    public FileReference Content { get; set; }

    [Display("Subject line")]
    public string? SubjectLine { get; set; }

    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Email name", Description = "Overrides the default file name")]
    public string? EmailName { get; set; }

    [Display("Content suffix", Description = "A custom ending for the name of the newly created email and its content blocks")]
    public string? ContentSuffix { get; set; }

    [Display("Create content blocks in their original folder", Description = "False by default")]
    public bool? CreateContentBlocksInOriginalFolder { get; set; }

    [Display("Script variable names to update",
        Description = "List of AMPScript variable names (e.g. @Language or Language). Must match the order of values")]
    public IEnumerable<string>? ScriptVariableNames { get; set; }

    [Display("Script variable values to update", Description = "List of values corresponding to the variable names")]
    public IEnumerable<string>? ScriptVariableValues { get; set; }
}
