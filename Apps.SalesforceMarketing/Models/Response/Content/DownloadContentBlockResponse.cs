using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Response.Content;

public class DownloadContentBlockResponse(FileReference content)
{
    public FileReference Content { get; set; } = content;
}
