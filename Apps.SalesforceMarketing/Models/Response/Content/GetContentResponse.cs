using Blackbird.Applications.Sdk.Common;
using Apps.SalesforceMarketing.Models.Entities.Asset;

namespace Apps.SalesforceMarketing.Models.Response.Content;

public class GetContentResponse(AssetEntity assetEntity)
{
    [Display("Asset ID")]
    public string Id { get; set; } = assetEntity.Id;

    [Display("Customer key")]
    public string? CustomerKey { get; set; } = assetEntity.CustomerKey;

    [Display("Asset type")]
    public string? AssetType { get; set; } = assetEntity.AssetType.DisplayName;

    [Display("Asset name")]
    public string Name { get; set; } = assetEntity.Name;

    [Display("Description")]
    public string? Description { get; set; } = string.IsNullOrEmpty(assetEntity.Description) ? null : assetEntity.Description;

    [Display("Owner")]
    public string? Owner { get; set; } = assetEntity.Owner?.Name;

    [Display("Created date")]
    public DateTime CreatedDate { get; set; } = assetEntity.CreatedDate;

    [Display("Created by")]
    public string? CreatedBy { get; set; } = assetEntity.CreatedBy?.Name;

    [Display("Modified date")]
    public DateTime? ModifiedDate { get; set; } = assetEntity.ModifiedDate;

    [Display("Modified by")]
    public string? ModifiedBy { get; set; } = assetEntity.ModifiedBy?.Name;

    [Display("Status")]
    public string? Status { get; set; } = assetEntity.Status?.Name;
}