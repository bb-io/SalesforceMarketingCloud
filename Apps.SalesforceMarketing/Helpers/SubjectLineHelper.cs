using Apps.SalesforceMarketing.Constants;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Apps.SalesforceMarketing.Helpers;

public static class SubjectLineHelper
{
    private const string SubjectLineVarName = "@subjectLine";

    public static string ExtractSubjectLinesFromAmpScript(string html)
    {
        var assignmentRegex = new Regex(
            $@"(?i)(SET\s+{Regex.Escape(SubjectLineVarName)}\s*=\s*)(['""])(.*?)\2",
            RegexOptions.Compiled);

        var matches = assignmentRegex.Matches(html);

        if (matches.Count == 0) 
            return html;

        var sbHtml = new StringBuilder(html);
        var translationDivs = new StringBuilder();

        translationDivs.AppendLine();
        translationDivs.AppendLine("");

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var m = matches[i];

            var prefix = m.Groups[1].Value;
            var quote = m.Groups[2].Value;
            var content = m.Groups[3].Value;

            if (string.IsNullOrWhiteSpace(content) || content.StartsWith($"[[{BlackbirdMetadataIds.SubjectLine}"))
                continue;

            string id = $"{BlackbirdMetadataIds.SubjectLine}-{matches.Count - i}";
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

    public static string RestoreSubjectLinesInAmpScript(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var translationDivs = doc.DocumentNode.SelectNodes($"//div[starts-with(@id, '{BlackbirdMetadataIds.SubjectLine}-')]");

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
            $@"<div id=""{BlackbirdMetadataIds.SubjectLine}-\d+"".*?</div>\s*", 
            "", 
            RegexOptions.Singleline
        );

        return result.Trim();
    }
}
