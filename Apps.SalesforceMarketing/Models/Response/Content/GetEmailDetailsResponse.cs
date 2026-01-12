using Apps.SalesforceMarketing.Models.Entities.Asset;
using Blackbird.Applications.Sdk.Common;

namespace Apps.SalesforceMarketing.Models.Response.Content;

public class GetEmailDetailsResponse(AssetEntity entity) : GetContentResponse(entity)
{
    [Display("Subject line")]
    public string SubjectLine { get; set; } = entity.Views.SubjectLine.Content;

    [Display("Preheader")]
    public string Preheader { get; set; } = entity.Views.Preheader.Content;
}
