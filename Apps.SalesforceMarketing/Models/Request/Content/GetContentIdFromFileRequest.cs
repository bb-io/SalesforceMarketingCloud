using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class GetContentIdFromFileRequest
{
    [Display("Content")]
    public FileReference Content { get; set; }
}
