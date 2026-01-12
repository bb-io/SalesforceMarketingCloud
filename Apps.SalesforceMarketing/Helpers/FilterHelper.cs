using Newtonsoft.Json.Linq;

namespace Apps.SalesforceMarketing.Helpers;

public static class FilterHelper
{
    public static JObject? BuildQueryTree(List<JObject> conditions)
    {
        if (conditions.Count == 0) return null;
        if (conditions.Count == 1) return conditions[0];

        var compoundQuery = new JObject
        {
            ["leftOperand"] = conditions[0],
            ["logicalOperator"] = "AND",
            ["rightOperand"] = conditions[1]
        };

        for (int i = 2; i < conditions.Count; i++)
        {
            compoundQuery = new JObject
            {
                ["leftOperand"] = compoundQuery,
                ["logicalOperator"] = "AND",
                ["rightOperand"] = conditions[i]
            };
        }

        return compoundQuery;
    }
}
