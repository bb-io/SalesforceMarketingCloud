using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Response.Content;

public class DownloadEmailResponse(FileReference content)
{
    public FileReference Content { get; set; } = content;
}
