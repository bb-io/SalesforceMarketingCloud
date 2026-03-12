using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Request.Email;

public class UpdateEmailRequest
{
    [Display("Content")]
    public FileReference Content { get; set; }

    [Display("Overwrite subject line")]
    public string? SubjectLine { get; set; }

    [Display("Script variable names to update",
        Description = "List of AMPScript variable names (e.g. @Language or Language). Must match the order of values")]
    public IEnumerable<string>? ScriptVariableNames { get; set; }

    [Display("Script variable values to update", Description = "List of values corresponding to the variable names")]
    public IEnumerable<string>? ScriptVariableValues { get; set; }
}
