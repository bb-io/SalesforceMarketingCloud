using RestSharp;
using Apps.SalesforceMarketing.Api;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.SalesforceMarketing.Connections;

public class ConnectionValidator: IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new SalesforceClient(authenticationCredentialsProviders);
            var request = new RestRequest("platform/v1/tokenContext", Method.Get);

            await client.ExecuteWithErrorHandling(request);

            return new() { IsValid = true };
        } catch(Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }

    }
}