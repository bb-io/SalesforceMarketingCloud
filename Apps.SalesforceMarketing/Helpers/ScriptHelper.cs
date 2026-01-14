using System.Text;
using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SalesforceMarketing.Helpers;

public static class ScriptHelper
{
    private const string AmpScriptBlockStart = "%%[";
    private const string AmpScriptBlockEnd = "]%%";

    // We're not going to go to deep into the file to check if the AMPscript body exists
    // Because there are potentially going to be a few of them in the middle of the file and we want to modify the first one
    // This number should be enough to skip head and meta tags and check if there is a script body at the beginning of the file
    private const int HeaderBlockThreshold = 4000;

    public static string UpsertScriptVariables(string html, IEnumerable<string> varNames, IEnumerable<string> varValues)
    {
        if (varNames == null || !varNames.Any())
            return html;

        var namesList = NormalizeVariableNames(varNames);
        var valuesList = varValues?.ToList() ?? [];

        if (namesList.Count != valuesList.Count)
        {
            throw new PluginMisconfigurationException(
                $"Mismatch between variables and values. You provided {namesList.Count} names but {valuesList.Count} values"
            );
        }

        int openIdx = html.IndexOf(AmpScriptBlockStart);
        if (!ScriptBlockIsFound(openIdx) || openIdx > HeaderBlockThreshold)
            return CreateNewScriptBlock(html, namesList, valuesList);
        
        return ProcessExistingScriptBlock(html, namesList, valuesList);
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

    private static bool ScriptBlockIsFound(int index) => index != -1;

    private static string CreateNewScriptBlock(string html, List<string> names, List<string> values)
    {
        var sb = new StringBuilder();
        sb.Append(AmpScriptBlockStart + "\r\n");

        for (int i = 0; i < names.Count; i++)
        {
            sb.Append($"    var {names[i]}\r\n");
            sb.Append($"    set {names[i]} = \"{values[i]}\"\r\n");
        }

        sb.Append(AmpScriptBlockEnd + "\r\n");
        return sb.ToString() + html;
    }

    private static string ProcessExistingScriptBlock(string html, List<string> names, List<string> values)
    {
        for (int i = 0; i < names.Count; i++)
            html = ProcessVariable(html, names[i], values[i]);

        return html;
    }

    private static string ProcessVariable(string html, string name, string value)
    {
        var pattern = GetVariablePattern(name);

        if (Regex.IsMatch(html, pattern))
            return UpdateExistingVariable(html, pattern, value);

        return InjectNewVariable(html, name, value);
    }

    private static string GetVariablePattern(string varName)
    {
        var safeVar = Regex.Escape(varName);
        return $@"(?i)(SET\s+)({safeVar})(\s*=\s*)(['""])(.*?)\4";  // Returns SET @Var = "..."
    }

    private static string UpdateExistingVariable(string html, string pattern, string newValue)
    {
        return Regex.Replace(html, pattern, m =>
            $"{m.Groups[1].Value}{m.Groups[2].Value}{m.Groups[3].Value}{m.Groups[4].Value}{newValue}{m.Groups[4].Value}"
        );
    }

    private static string InjectNewVariable(string html, string name, string value)
    {
        int closeIdx = html.IndexOf(AmpScriptBlockEnd);
        if (!ScriptBlockIsFound(closeIdx)) 
            return html;

        var sb = new StringBuilder();

        if (closeIdx > 0 && html[closeIdx - 1] != '\n')
            sb.Append("\r\n");

        sb.Append($"    var {name}\r\n");
        sb.Append($"    set {name} = \"{value}\"");

        sb.Append("\r\n");
        return html.Insert(closeIdx, sb.ToString());
    }
}