using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Models.Request.Email;

public class FindEmailByNameRequest
{
    [Display("Email name")]
    public string EmailName { get; set; }

    [Display("Category ID", Description = "Recommended to specify as emails with the same name may exist in different categories")]
    [FileDataSource(typeof(CategoryDataHandler))]
    public string? CategoryId { get; set; }

    [Display("Include subfolders",
        Description = "Searches for emails in subfolders of the specified category ID. False by default")]
    public bool? IncludeSubfolders { get; set; }

    public FindEmailByNameRequest ApplyDefaultValues()
    {
        IncludeSubfolders ??= false;
        return this;
    }
}
