using Blackbird.Applications.Sdk.Common;

namespace Apps.SalesforceMarketing.Models.Response.Content;

public record GetContentIdFromFileResponse(string ContentId)
{
    [Display("Content ID")]
    public string ContentId { get; set; } = ContentId;
}
