using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Handlers;
using Apps.SalesforceMarketing.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Polling.Request;

public class OnContentCreatedOrUpdatedRequest
{
    [Display("Category ID"), FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Content types to search", Description = "Searches all available content types by default")]
    [StaticDataSource(typeof(ContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    [Display("Include subfolders",
        Description = "Searches for content in subfolders of the specified category ID. False by default")]
    public bool? IncludeSubfolders { get; set; }

    [Display("Name contains",
        Description = "Return only assets whose name contains this value (case-insensitive)")]
    public string? NameContains { get; set; }

    [Display("Name doesn't contain",
        Description = "Exclude assets whose name contains any of these values (case-insensitive)")]
    public IEnumerable<string>? NameDoesntContain { get; set; }

    public void ApplyDefaultValues()
    {
        IncludeSubfolders ??= false;
        ContentTypes ??= AssetTypeIds.SupportedAssetTypes.ToList();
    }
}
