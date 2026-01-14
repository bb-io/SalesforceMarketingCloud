using RestSharp;
using Apps.SalesforceMarketing.Models.Entities.Category;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Handlers;

public class CategoryDataHandler(InvocationContext invocationContext)
    : SalesforceInvocable(invocationContext), IAsyncFileDataSourceItemHandler
{
    private const int GlobalRootFolderId = 0;

    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest("asset/v1/content/categories", Method.Get);
        var filters = new List<string>();

        if (!string.IsNullOrEmpty(context.FolderId))
            filters.Add($"parentId eq {context.FolderId}");
        else
            filters.Add($"parentId eq {GlobalRootFolderId}");

        if (filters.Count != 0)
            request.AddQueryParameter("$filter", string.Join(" AND ", filters));

        var response = await Client.PaginateGet<CategoryEntity>(request);

        var folders = new List<FileDataItem>();
        foreach (var folder in response)
        {
            var category = new Folder()
            {
                Id = folder.Id.ToString(),
                DisplayName = folder.Name,
                IsSelectable = true
            };
            folders.Add(category);
        }

        return folders;
    }

    public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(context.FileDataItemId) || !int.TryParse(context.FileDataItemId, out int currentId))
            return [new FolderPathItem { Id = GlobalRootFolderId.ToString(), DisplayName = "Home" }];

        var path = new List<FolderPathItem>();

        int? startNodeParentId = null;
        while (currentId != 0)
        {
            var request = new RestRequest($"asset/v1/content/categories/{currentId}", Method.Get);
            var category = await Client.ExecuteWithErrorHandling<CategoryEntity>(request);

            if (category == null) 
                break;

            startNodeParentId ??= category.ParentId;

            path.Add(new FolderPathItem
            {
                Id = category.Id.ToString(),
                DisplayName = category.Name
            });

            currentId = category.ParentId;
        }

        path.Add(new FolderPathItem { Id = GlobalRootFolderId.ToString(), DisplayName = "Home" });

        if (path.Count > 0 && startNodeParentId != 0)
            path.RemoveAt(0);

        path.Reverse();
        return path;
    }
}
