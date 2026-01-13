using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class UploadEmailRequest
{
    [Display("Content")]
    public FileReference Content { get; set; }

    [Display("Subject line")]
    public string SubjectLine { get; set; }

    [Display("Email name", Description = "Overrides the default file name")]
    public string? EmailName { get; set; }
}
