using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Response.Email;

public class DownloadEmailResponse(FileReference content)
{
    public FileReference Content { get; set; } = content;
}
