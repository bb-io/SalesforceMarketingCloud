using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Response.Block;

public class DownloadContentBlockResponse(FileReference content)
{
    public FileReference Content { get; set; } = content;
}
