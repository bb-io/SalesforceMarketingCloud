using Apps.SalesforceMarketing.Api;
using Apps.SalesforceMarketing.Models.Entities.Category;
using RestSharp;

namespace Apps.SalesforceMarketing.Helpers;

public static class CategoryHelper
{
    public static async Task<List<string>> GetCategoryIds(
        SalesforceClient client,
        string? categoryId,
        bool? includeSubfolders)
    {
        var folderIds = new List<string>();
        if (string.IsNullOrEmpty(categoryId))
            return folderIds;

        folderIds.Add(categoryId);

        if (includeSubfolders != true)
            return folderIds;

        var foldersToProcess = new Queue<string>();
        foldersToProcess.Enqueue(categoryId);

        while (foldersToProcess.Count > 0)
        {
            var currentParentId = foldersToProcess.Dequeue();

            var categoryRequest = new RestRequest("asset/v1/content/categories", Method.Get);
            categoryRequest.AddQueryParameter("$filter", $"parentId eq {currentParentId}");

            var children = await client.PaginateGet<CategoryEntity>(categoryRequest);

            if (children != null)
            {
                foreach (var child in children)
                {
                    folderIds.Add(child.Id);
                    foldersToProcess.Enqueue(child.Id);
                }
            }
        }

        return folderIds;
    }
}
