using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Email;

public class UpdateEmailRequest
{
    [Display("Content")] 
    public FileReference Content { get; set; } = null!;

    [Display("Overwrite subject line")]
    public string? SubjectLine { get; set; }
    
    [Display("Content suffix")]
    public string? ContentSuffix { get; set; }

    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Keep original folders", Description = "False by default")]
    public bool? KeepOriginalFolders { get; set; }

    [Display("Script variable names to update",
        Description = "List of AMPScript variable names (e.g. @Language or Language). Must match the order of values")]
    public IEnumerable<string>? ScriptVariableNames { get; set; }

    [Display("Script variable values to update", Description = "List of values corresponding to the variable names")]
    public IEnumerable<string>? ScriptVariableValues { get; set; }
}
