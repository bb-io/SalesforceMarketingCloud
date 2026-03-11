namespace Apps.SalesforceMarketing.Extensions;

public static class StringExtensions
{
    public static string ToHtmlFileName(this string input)
    {
        input = input.Replace(' ', '_');
        return $"{input}.html";
    }
}
