namespace Apps.SalesforceMarketing.Extensions;

public static class StringExtensions
{
    public static string ToHtmlFileName(this string input)
    {
        input = input.Replace(' ', '_');
        return $"{input}.html";
    }

    public static string EnsureStartsWith(this string input, string prefix)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.StartsWith(prefix) ? input : prefix + input;
    }
}
