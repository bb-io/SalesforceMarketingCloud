using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SalesforceMarketing.Models.Identifiers;

public class EmailIdentifier
{
    [Display("Email ID"), DataSource(typeof(EmailDataHandler))]
    public string EmailId { get; set; }
}
