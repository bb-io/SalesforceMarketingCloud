using Apps.SalesforceMarketing.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class CreateContentBlockRequest
{
    [Display("Name")]
    public string Name { get; set; }

    [Display("Content")]
    public string Content { get; set; }

    [Display("Block type")]
    [StaticDataSource(typeof(ContentBlockTypeDataHandler))]
    public string AssetTypeId { get; set; }
}
