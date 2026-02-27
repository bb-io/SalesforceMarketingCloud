using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class UpdateEmailRequest
{
    [Display("Content")]
    public FileReference Content { get; set; }

    [Display("Overwrite subject line")]
    public string? SubjectLine { get; set; }
}
