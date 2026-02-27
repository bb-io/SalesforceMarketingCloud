namespace Apps.SalesforceMarketing.Helpers;

public static class ClientSideFilterHelper
{
    public static IEnumerable<T> FilterExcludedNames<T>(
        this IEnumerable<T> entities,
        IEnumerable<string>? namesToExclude,
        Func<T, string?> nameSelector)
    {
        var excludeTokens = (namesToExclude ?? [])
           .Select(x => x?.Trim())
           .Where(x => !string.IsNullOrWhiteSpace(x))
           .Distinct(StringComparer.OrdinalIgnoreCase)
           .ToArray();

        if (excludeTokens.Length == 0)
            return entities;

        return entities
            .Where(e => !excludeTokens.Any(token =>
                (nameSelector(e) ?? string.Empty).Contains(token!, StringComparison.OrdinalIgnoreCase))
            );
    }
}
