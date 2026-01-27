using Apps.SalesforceMarketing.Constants;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.SalesforceMarketing.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "Credentials",
            DisplayName = "Credentials",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = 
            [            
                new(CredsNames.ClientId) { DisplayName = "Client ID" },
                new(CredsNames.ClientSecret) { DisplayName = "Client secret", Sensitive = true },
                new(CredsNames.Subdomain) { DisplayName = "Subdomain" },
                new(CredsNames.AccountId) { DisplayName = "Account ID" },
            ]
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        return values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();
    }
}