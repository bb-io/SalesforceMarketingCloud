using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SalesforceMarketing.Helpers;

public static class ScriptHelper
{
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