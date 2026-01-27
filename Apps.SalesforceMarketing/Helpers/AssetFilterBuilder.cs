using Newtonsoft.Json.Linq;

namespace Apps.SalesforceMarketing.Helpers;

public class AssetFilterBuilder
{
    private readonly List<JObject> _filters = [];

    public AssetFilterBuilder WhereEquals(string property, string? value)
    {
        return AddCondition(property, "equals", value);
    }

    public AssetFilterBuilder WhereNotEquals(string property, string? value)
    {
        return AddCondition(property, "notEquals", value);
    }

    public AssetFilterBuilder WhereGreaterOrEqual(string property, DateTime? value)
    {
        return AddCondition(property, "greaterThanOrEqual", value);
    }

    public AssetFilterBuilder WhereLessOrEqual(string property, DateTime? value)
    {
        return AddCondition(property, "lessThanOrEqual", value);
    }

    public AssetFilterBuilder WhereIn(string property, IEnumerable<string>? values)
    {
        if (values == null || !values.Any()) 
            return this;
        return AddCondition(property, "in", values);
    }

    public AssetFilterBuilder WhereLike(string property, string? value)
    {
        return AddCondition(property, "like", value);
    }

    public JObject? Build()
    {
        if (_filters.Count == 0) 
            return null;
        if (_filters.Count == 1) 
            return _filters[0];

        var compoundQuery = new JObject
        {
            ["leftOperand"] = _filters[0],
            ["logicalOperator"] = "AND",
            ["rightOperand"] = _filters[1]
        };

        for (int i = 2; i < _filters.Count; i++)
        {
            compoundQuery = new JObject
            {
                ["leftOperand"] = compoundQuery,
                ["logicalOperator"] = "AND",
                ["rightOperand"] = _filters[i]
            };
        }

        return compoundQuery;
    }

    private AssetFilterBuilder AddCondition(string property, string simpleOperator, object? value)
    {
        if (value is null) 
            return this;
        if (value is string s && string.IsNullOrEmpty(s)) 
            return this;

        JToken safeValue;
        if (value is DateTime dt)
            safeValue = dt.ToString("yyyy-MM-ddTHH:mm:ss");
        else if (value is IEnumerable<string> stringList)
            safeValue = new JArray(stringList);
        else
            safeValue = JToken.FromObject(value);

        _filters.Add(new JObject
        {
            ["property"] = property,
            ["simpleOperator"] = simpleOperator,
            ["value"] = safeValue
        });

        return this;
    }
}
