using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SalesforceMarketing.Models.Identifiers;

public class AssetIdentifier
{
    [Display("Asset ID"), DataSource(typeof(AssetDataHandler))]
    public string AssetId { get; set; }
}
