using System.Text;
using System.Text.RegularExpressions;
using Apps.SalesforceMarketing.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SalesforceMarketing.Helpers;

public static class ScriptHelper
{
    private static readonly Regex AmpScriptBlockRegex = new Regex(@"(?s)%%\[(.*?)\]%%", RegexOptions.Compiled);

    public static string WrapAmpScriptBlocks(string html)
    {
        var matches = AmpScriptBlockRegex.Matches(html);
        if (matches.Count == 0) 
            return html;

        var sb = new StringBuilder(html);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];

            string innerContent = match.Groups[1].Value;
            innerContent = innerContent.Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);

            string id = $"{BlackbirdMetadataIds.AmpScript}-{i + 1}";
            string scriptTag = $"<script id=\"{id}\" type=\"text/ampscript\">{innerContent}</script>";

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, scriptTag);
        }

        return sb.ToString();
    }

    public static string UnwrapAmpScriptBlocks(string html)
    {
        var scriptRegex = new Regex(
            $@"(?s)<script\s+[^>]*?id=""({BlackbirdMetadataIds.AmpScript}-\d+)""[^>]*?>(.*?)</script>",
            RegexOptions.IgnoreCase
        );

        var matches = scriptRegex.Matches(html);
        if (matches.Count == 0) return html;

        var sb = new StringBuilder(html);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            string innerContent = match.Groups[2].Value;

            innerContent = innerContent.Replace("<\\/script>", "</script>", StringComparison.OrdinalIgnoreCase);

            string originalBlock = $"%%[{innerContent}]%%";

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, originalBlock);
        }

        return sb.ToString();
    }

    public static string UpsertScriptVariables(string html, IEnumerable<string> varNames, IEnumerable<string> varValues)
    {
        if (string.IsNullOrWhiteSpace(html) || varNames == null || !varNames.Any())
            return html;

        var namesList = NormalizeVariableNames(varNames);
        var valuesList = varValues?.ToList() ?? [];

        if (namesList.Count != valuesList.Count)
        {
            throw new PluginMisconfigurationException(
                $"Mismatch between variables and values. You provided {namesList.Count} names but {valuesList.Count} values"
            );
        }

        for (int i = 0; i < namesList.Count; i++)
            html = UpdateFirstOccurrence(html, namesList[i], valuesList[i]);

        return html;
    }

    private static string UpdateFirstOccurrence(string html, string varName, string newValue)
    {
        var safeVar = Regex.Escape(varName);
        var pattern = $@"(?i)(SET\s+{safeVar}\s*=\s*)(['""])(.*?)\2";

        var regex = new Regex(pattern);
        return regex.Replace(html, m => $"{m.Groups[1].Value}{m.Groups[2].Value}{newValue}{m.Groups[2].Value}", 1);
    }

    private static List<string> NormalizeVariableNames(IEnumerable<string> varNames)
    {
        return varNames
            .Select(n =>
            {
                string cleanName = n.Trim();
                return cleanName.StartsWith('@') ? cleanName : "@" + cleanName;
            })
            .ToList();
    }
}