using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Handlers;
using Apps.SalesforceMarketing.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Content;

public class SearchContentRequest
{
    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Content types to search", Description = "Searches all available content types by default")]
    [StaticDataSource(typeof(ContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    [Display("Created from")]
    public DateTime? CreatedFromDate { get; set; }

    [Display("Created to")]
    public DateTime? CreatedToDate { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFromDate { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedToDate { get; set; }

    [Display("Include subfolders", 
        Description = "Searches for content in subfolders of the specified category ID. False by default")]
    public bool? IncludeSubfolders { get; set; }

    [Display("Name contains",
        Description = "Return only assets whose name contains this value (case-insensitive)")]
    public string? NameContains { get; set; }

    [Display("Name doesn't contain",
        Description = "Exclude assets whose name contains any of these values (case-insensitive)")]
    public IEnumerable<string>? NameDoesntContain { get; set; }

    public void Validate()
    {
        if (CreatedFromDate.HasValue && CreatedToDate.HasValue && CreatedFromDate > CreatedToDate)
            throw new PluginMisconfigurationException("'Created from' can't be after the 'Created to' date");

        if (UpdatedFromDate.HasValue && UpdatedToDate.HasValue && UpdatedFromDate > UpdatedToDate)
            throw new PluginMisconfigurationException("'Updated from' can't be after the 'Updated to' date");
    }

    public void ApplyDefaultValues()
    {
        IncludeSubfolders ??= false;
        ContentTypes ??= AssetTypeIds.SupportedAssetTypes.ToList();
    }
}
