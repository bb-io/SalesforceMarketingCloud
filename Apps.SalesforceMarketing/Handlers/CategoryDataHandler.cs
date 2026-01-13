using RestSharp;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Category;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SalesforceMarketing.Handlers;

public class CategoryDataHandler(InvocationContext invocationContext)
    : SalesforceInvocable(invocationContext), IAsyncFileDataSourceItemHandler
{
    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context, CancellationToken ct)
    {
        await WebhookLogger.Log(context);
        var request = new RestRequest("asset/v1/content/categories", Method.Get);
        var filters = new List<string>();

        if (!string.IsNullOrEmpty(context.FolderId))
            filters.Add($"parentId eq {context.FolderId}");
        else if (string.IsNullOrEmpty(context.FolderId))
            filters.Add("parentId eq 0");   // root = 0

        if (filters.Count != 0)
            request.AddQueryParameter("$filter", string.Join(" AND ", filters));

        request.AddQueryParameter("$page", "1");
        request.AddQueryParameter("$pageSize", "50");
        await WebhookLogger.Log(filters);
        await WebhookLogger.Log(request);

        var response = await Client.ExecuteWithErrorHandling<ItemsWrapper<CategoryEntity>>(request);
        await WebhookLogger.Log(response);
        var folders = new List<Folder>();
        int i = 0;

        foreach (var category in response.Items)
        {
            i++;
            await WebhookLogger.Log($"run {i}");
            await WebhookLogger.Log(filters);
            var folder = new Folder
            {
                Id = category.Id.ToString(),
                DisplayName = category.Name,
                IsSelectable = true
            };
            folders.Add(folder);
            await WebhookLogger.Log(folders);
        }
        await WebhookLogger.Log(folders);
        return folders;
    }

    public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(context.FileDataItemId))
            return [];

        if (!int.TryParse(context.FileDataItemId, out int currentId))
            return [];

        var path = new List<FolderPathItem>();
        while (currentId != 0)
        {
            var request = new RestRequest($"asset/v1/content/categories/{currentId}", Method.Get);

            var category = await Client.ExecuteWithErrorHandling<CategoryEntity>(request);
            path.Add(new FolderPathItem
            {
                Id = category.Id.ToString(),
                DisplayName = category.Name
            });

            currentId = category.ParentId;
        }

        path.Reverse();
        return path;
    }
}
