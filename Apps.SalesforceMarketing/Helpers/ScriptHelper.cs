using Apps.SalesforceMarketing.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Apps.SalesforceMarketing.Helpers;

public static class ScriptHelper
{
    private static readonly Regex AmpScriptBlockRegex = new Regex(@"(?s)%%\[.*?\]%%", RegexOptions.Compiled);

    public static string ExtractVariables(string html, string variableName, string metadataId)
    {
        var assignmentRegex = new Regex(
            $@"(?i)(SET\s+{Regex.Escape(variableName)}\s*=\s*)(['""])(.*?)\2",
            RegexOptions.Compiled
        );

        var matches = assignmentRegex.Matches(html);

        if (matches.Count == 0)
            return html;

        var sbHtml = new StringBuilder(html);
        var translationDivs = new StringBuilder();

        translationDivs.AppendLine();
        translationDivs.AppendLine($"");

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var m = matches[i];

            var prefix = m.Groups[1].Value;
            var quote = m.Groups[2].Value;
            var content = m.Groups[3].Value;

            if (string.IsNullOrWhiteSpace(content) || content.StartsWith($"[[{metadataId}"))
                continue;

            string id = $"{metadataId}-{matches.Count - i}";
            string token = $"[[{id}]]";

            sbHtml.Remove(m.Index, m.Length);
            sbHtml.Insert(m.Index, $"{prefix}{quote}{token}{quote}");

            translationDivs.AppendLine(
                $"<div id=\"{id}\">{content}</div>"
            );
        }

        string result = sbHtml.ToString();
        var bodyMatch = Regex.Match(result, "<body[^>]*>", RegexOptions.IgnoreCase);

        if (bodyMatch.Success)
            result = result.Insert(bodyMatch.Index + bodyMatch.Length, translationDivs.ToString());

        return result;
    }

    public static string RestoreVariables(string html, string metadataId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var translationDivs = doc.DocumentNode.SelectNodes($"//div[starts-with(@id, '{metadataId}-')]");

        if (translationDivs == null)
            return html;

        string result = html;

        foreach (var div in translationDivs)
        {
            string token = $"[[{div.Id}]]";
            string translatedText = div.InnerHtml;

            translatedText = WebUtility.HtmlDecode(translatedText);
            result = result.Replace(token, translatedText);
        }

        result = Regex.Replace(
            result,
            $@"<div id=""{Regex.Escape(metadataId)}-\d+"".*?</div>\s*",
            "",
            RegexOptions.Singleline
        );

        return result.Trim();
    }

    public static string WrapAmpScriptBlocks(string html)
    {
        var matches = AmpScriptBlockRegex.Matches(html);
        if (matches.Count == 0) 
            return html;

        var sb = new StringBuilder(html);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];

            string content = match.Value;
            content = content.Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);

            string id = $"{BlackbirdMetadataIds.AmpScript}-{i + 1}";
            string scriptTag = $"<script id=\"{id}\" type=\"text/ampscript\">{content}\n</script>";

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, scriptTag);
        }

        return sb.ToString();
    }

    public static string RestoreScriptBlocks(string html)
    {
        var scriptRegex = new Regex(
            $@"(?s)<script\s+[^>]*?id=""({BlackbirdMetadataIds.AmpScript}-\d+)""[^>]*?>(.*?)</script>",
            RegexOptions.IgnoreCase
        );

        var matches = scriptRegex.Matches(html);
        var sb = new StringBuilder(html);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            string innerContent = match.Groups[2].Value;

            innerContent = innerContent.Replace("<\\/script>", "</script>", StringComparison.OrdinalIgnoreCase);

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, innerContent);
        }

        return sb.ToString();
    }

    public static string UpdateScriptVariables(string html, IEnumerable<string> varNames, IEnumerable<string> varValues)
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