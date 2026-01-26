using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class DownloadEmailRequest
{
    [Display("Content block IDs to ignore"), DataSource(typeof(ContentBlockDataHandler))]
    public IEnumerable<string>? ContentBlockIdsToIgnore { get; set; }
}
