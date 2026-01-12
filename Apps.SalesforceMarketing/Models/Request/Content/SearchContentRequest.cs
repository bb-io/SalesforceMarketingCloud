using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class SearchContentRequest
{
    [Display("Created from")]
    public DateTime? CreatedFromDate { get; set; }

    [Display("Created to")]
    public DateTime? CreatedToDate { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFromDate { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedToDate { get; set; }

    public void Validate()
    {
        if (CreatedFromDate.HasValue && CreatedToDate.HasValue && CreatedFromDate > CreatedToDate)
            throw new PluginMisconfigurationException("'Created from' can't be after the 'Created to' date");

        if (UpdatedFromDate.HasValue && UpdatedToDate.HasValue && UpdatedFromDate > UpdatedToDate)
            throw new PluginMisconfigurationException("'Updated from' can't be after the 'Updated to' date");
    }
}
