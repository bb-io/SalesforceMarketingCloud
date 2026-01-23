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

    public static async Task<string> ExpandContentBlocks(string html, SalesforceClient client)
    {
        var sb = new StringBuilder(html);

        var idMatches = IdBlockRegex.Matches(html);
        for (int i = idMatches.Count - 1; i >= 0; i--)
        {
            var match = idMatches[i];
            string blockId = match.Groups[1].Value;

            var blockEntity = await GetAssetById(client, blockId);
            string blockHtml = GetBlockContent(blockEntity);

            string replacement = $"<div id=\"{BlackbirdMetadataIds.ContentBlockId}-{blockId}\">{blockHtml}</div>";

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, replacement);
        }

        string intermediateHtml = sb.ToString();
        var nameMatches = NameBlockRegex.Matches(intermediateHtml);
        var sbName = new StringBuilder(intermediateHtml);

        for (int i = nameMatches.Count - 1; i >= 0; i--)
        {
            var match = nameMatches[i]; 
            
            string fullPath = match.Groups[2].Value;
            string blockName = fullPath.Split('\\').Last();

            var blockEntity = await GetAssetByName(client, blockName);
            string blockHtml = GetBlockContent(blockEntity);

            string blockId = blockEntity?.Id ?? "0";
            string replacement = $"<div id=\"{BlackbirdMetadataIds.ContentBlockId}-{blockId}\">{blockHtml}</div>";

            sbName.Remove(match.Index, match.Length);
            sbName.Insert(match.Index, replacement);
        }

        return sbName.ToString();
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

    private static async Task<AssetEntity?> GetAssetByName(SalesforceClient client, string name)
    {
        var request = new RestRequest("asset/v1/content/assets", Method.Get);
        request.AddQueryParameter("$filter", $"name eq '{name}'");

        var response = await client.ExecuteWithErrorHandling<ItemsWrapper<AssetEntity>>(request);
        return response.Items?.FirstOrDefault();
    }
}
