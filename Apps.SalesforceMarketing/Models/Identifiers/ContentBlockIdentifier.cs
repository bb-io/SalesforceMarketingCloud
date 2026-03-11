using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SalesforceMarketing.Models.Identifiers;

public class ContentBlockIdentifier
{
    [Display("Content block ID"), DataSource(typeof(ContentBlockDataHandler))]
    public string ContentBlockId { get; set; }
}
