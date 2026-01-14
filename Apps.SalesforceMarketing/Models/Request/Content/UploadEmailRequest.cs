using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Content;

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

    [Display("Script variable names", Description = "List of AMPscript variable names (e.g. @Language or Language). Must match the order of values")]
    public IEnumerable<string>? ScriptVariableNames { get; set; }

    [Display("Script variable values", Description = "List of values corresponding to the variable names")]
    public IEnumerable<string>? ScriptVariableValues { get; set; }
}
