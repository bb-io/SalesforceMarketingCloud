using Apps.SalesforceMarketing.Api;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using RestSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace Apps.SalesforceMarketing.Helpers;

public static class ContentBlockHelper
{
    private static readonly Regex IdBlockRegex = new Regex(
        @"(?i)%%=ContentBlockByID\(\s*[""']?(\d+)[""']?\s*\)=%%",
        RegexOptions.Compiled
    );

    private static readonly Regex NameBlockRegex = new Regex(
        @"(?i)%%=ContentBlockByName\(\s*(['""])(.*?)\1\s*\)=%%",
        RegexOptions.Compiled
    );

    public static async Task<string> ExpandContentBlocks(
        string html, 
        SalesforceClient client, 
        IEnumerable<string>? blockIdsToIgnore)
    {
        var sb = new StringBuilder(html);
        bool foundNewBlocks = true;
        int depth = 0;
        const int MaxDepth = 5; 
        var skippedIds = new HashSet<string>(blockIdsToIgnore ?? []);

        while (foundNewBlocks && depth < MaxDepth)
        {
            foundNewBlocks = false;
            string currentHtml = sb.ToString();

            var idMatches = IdBlockRegex.Matches(currentHtml);
            for (int i = idMatches.Count - 1; i >= 0; i--)
            {
                var match = idMatches[i];
                string blockId = match.Groups[1].Value;
                if (skippedIds.Contains(blockId)) 
                    continue;

                var blockEntity = await GetAssetById(client, blockId);

                string blockContent = GetBlockContent(blockEntity);
                string replacement = $@"<div id=""{BlackbirdMetadataIds.ContentBlockId}-{blockId}"">{blockContent}</div>";

                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
                foundNewBlocks = true;
            }

            currentHtml = sb.ToString();
            var nameMatches = NameBlockRegex.Matches(currentHtml);

            for (int i = nameMatches.Count - 1; i >= 0; i--)
            {
                var match = nameMatches[i];
                string fullPath = match.Groups[2].Value;

                var pathParts = fullPath.Split('\\');
                string blockName = pathParts.Last();
                string? parentFolderName = pathParts.Length > 1
                    ? pathParts[^2]
                    : null;

                var blockEntity = await GetAssetByName(client, blockName, parentFolderName);
                if (blockEntity != null && skippedIds.Contains(blockEntity.Id))
                    continue;

                string blockContent = GetBlockContent(blockEntity);
                string blockId = blockEntity?.Id ?? "0";

                string replacement = $@"<div id=""{BlackbirdMetadataIds.ContentBlockId}-{blockId}"">{blockContent}</div>";

                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
                foundNewBlocks = true;
            }

            depth++;
        }

        return sb.ToString();
    }

    private static string GetBlockContent(AssetEntity? entity)
    {
        if (entity == null) 
            return "";

        if (!string.IsNullOrEmpty(entity.Content)) 
            return entity.Content;
        if (!string.IsNullOrEmpty(entity.Views?.Html?.Content)) 
            return entity.Views.Html.Content;
        if (!string.IsNullOrEmpty(entity.Views?.Text?.Content)) 
            return entity.Views.Text.Content;

        return "";
    }

    private static async Task<AssetEntity?> GetAssetById(SalesforceClient client, string id)
    {
        var request = new RestRequest($"asset/v1/content/assets/{id}", Method.Get);
        return await client.ExecuteWithErrorHandling<AssetEntity>(request);
    }

    private static async Task<AssetEntity?> GetAssetByName(SalesforceClient client, string name, string? parentFolderName)
    {
        string safeName = name.Replace("'", "''");
        var request = new RestRequest("asset/v1/content/assets", Method.Get);
        request.AddQueryParameter("$filter", $"name eq '{safeName}'");

        var response = await client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);

        if (response.Items == null || response.Items.Count == 0) 
            return null;

        if (!string.IsNullOrEmpty(parentFolderName))
        {
            var match = response.Items.FirstOrDefault(a =>
                a.Category != null &&
                a.Category.Name.Equals(parentFolderName, StringComparison.OrdinalIgnoreCase));

            return match ?? response.Items.First();
        }

        return response.Items.First();
    }
}
