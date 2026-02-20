using Apps.SalesforceMarketing.Api;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models;
using Apps.SalesforceMarketing.Models.Entities.Asset;
using Blackbird.Applications.Sdk.Common.Exceptions;
using HtmlAgilityPack;
using RestSharp;
using System.Net;
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

                var blockEntity = await GetAssetById(client, blockId)
                    ?? throw new PluginMisconfigurationException($"The block with ID {blockId} was not found");
                string blockContent = GetBlockContent(blockEntity);
                string replacement = 
                    $@"<{CustomHtmlTagNames.ContentBlock} id=""{BlackbirdMetadataIds.ContentBlockId}-{blockId}"">
                    {blockContent}
                    </{CustomHtmlTagNames.ContentBlock}>";

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
                string? parentFolderName = pathParts.Length > 1 ? pathParts[^2] : null;

                var blockEntity = await GetAssetByName(client, blockName, parentFolderName) 
                    ?? throw new PluginMisconfigurationException($"The block with name '{blockName}' was not found");
                if (skippedIds.Contains(blockEntity.Id)) 
                    continue;

                string blockContent = GetBlockContent(blockEntity);
                string blockId = blockEntity?.Id ?? "0";
                string replacement = 
                    $@"<{CustomHtmlTagNames.ContentBlock} id=""{BlackbirdMetadataIds.ContentBlockId}-{blockId}"">
                    {blockContent}
                    </{CustomHtmlTagNames.ContentBlock}>";

                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
                foundNewBlocks = true;
            }

            depth++;
        }

        return sb.ToString();
    }

    public static async Task<string> RestoreContentBlocks(
        string html,
        SalesforceClient client,
        string? newEmailName,
        string? categoryId)
    {
        var doc = new HtmlDocument();
        doc.OptionFixNestedTags = true;
        doc.LoadHtml(html);

        var blockNodes = doc.DocumentNode.SelectNodes(
            $"//{CustomHtmlTagNames.ContentBlock}[starts-with(@id, '{BlackbirdMetadataIds.ContentBlockId}-')]"
        );

        if (blockNodes == null) 
            return html;

        var sortedNodes = blockNodes
            .OrderByDescending(node => node.Ancestors(CustomHtmlTagNames.ContentBlock).Count())
            .ToList();

        var uploadedBlocksCache = new Dictionary<string, string>();

        foreach (var node in sortedNodes)
        {
            string originalId = node.Id.Replace($"{BlackbirdMetadataIds.ContentBlockId}-", "");
            if (uploadedBlocksCache.TryGetValue(originalId, out string? existingNewAssetId))
            {
                ReplaceNodeWithReference(doc, node, existingNewAssetId);
                continue;
            }

            string translatedContent = WebUtility.HtmlDecode(node.InnerHtml.Trim());
            string prefix = !string.IsNullOrEmpty(newEmailName) ? $"{newEmailName} - " : string.Empty;
            string newBlockName = $"({prefix}Block {originalId} - {DateTime.UtcNow.Ticks})";
            string newAssetId = await CreateNewAsset(client, newBlockName, translatedContent, categoryId);

            uploadedBlocksCache[originalId] = newAssetId;
            ReplaceNodeWithReference(doc, node, newAssetId);
        }

        return doc.DocumentNode.OuterHtml;
    }

    private static void ReplaceNodeWithReference(HtmlDocument doc, HtmlNode node, string assetId)
    {
        string newReference = $"%%=ContentBlockByID({assetId})=%%";
        var textNode = doc.CreateTextNode(newReference);
        node.ParentNode.ReplaceChild(textNode, node);
    }

    private static async Task<string> CreateNewAsset(
        SalesforceClient client,
        string name,
        string content,
        string? categoryId)
    {
        var request = new RestRequest("asset/v1/content/assets", Method.Post);
        var body = new Dictionary<string, object>
        {
            { "name", name },
            { "assetType", new { id = AssetTypeIds.HtmlBlock } },
            { "views", new { html = new { content } } }
        };

        if (!string.IsNullOrEmpty(categoryId) && int.TryParse(categoryId, out int catId))
            body.Add("category", new { id = catId });

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<AssetEntity>(request);
        return response.Id;
    }

    private static string GetBlockContent(AssetEntity? entity)
    {
        if (entity == null) 
            return string.Empty;

        if (!string.IsNullOrEmpty(entity.Content)) 
            return entity.Content;
        if (!string.IsNullOrEmpty(entity.Views?.Html?.Content)) 
            return entity.Views.Html.Content;
        if (!string.IsNullOrEmpty(entity.Views?.Text?.Content)) 
            return entity.Views.Text.Content;

        return string.Empty;
    }

    private static async Task<AssetEntity?> GetAssetById(SalesforceClient client, string id)
    {
        var request = new RestRequest("asset/v1/content/assets", Method.Get);
        request.AddQueryParameter("$filter", $"id eq {id}");

        var response = await client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        return response.Items.FirstOrDefault();
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
                a.Category.Name.Equals(parentFolderName, StringComparison.OrdinalIgnoreCase)
            );

            return match ?? response.Items.First();
        }

        return response.Items.First();
    }
}
