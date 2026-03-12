using Blackbird.Applications.Sdk.Common;

namespace Apps.SalesforceMarketing.Models.Response.Email;

public record FindEmailByNameResponse(string? EmailId)
{
    [Display("Email ID")]
    public string? EmailId { get; set; } = EmailId;
}
