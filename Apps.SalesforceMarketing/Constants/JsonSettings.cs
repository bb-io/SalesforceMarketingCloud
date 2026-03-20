using Newtonsoft.Json;

namespace Apps.SalesforceMarketing.Constants;

public static class JsonSettings
{
    public static JsonSerializerSettings Settings => new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
}
