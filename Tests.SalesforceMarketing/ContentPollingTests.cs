using Apps.SalesforceMarketing.Polling;
using Apps.SalesforceMarketing.Polling.Memory;
using Apps.SalesforceMarketing.Polling.Request;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.SalesforceMarketing.Base;

namespace Tests.SalesforceMarketing;

[TestClass]
public class ContentPollingTests : TestBase
{
    [TestMethod]
    public async Task OnContentCreatedOrUpdated_ReturnsContent()
    {
		// Arrange
		var polling = new ContentPollingList(InvocationContext);
		var memory = new DateTimeMemory(DateTime.UtcNow - TimeSpan.FromHours(1));
		var pollingRequest = new PollingEventRequest<DateTimeMemory> { Memory = memory };
        var input = new OnContentCreatedOrUpdatedRequest
        {
            
        };

        // Act
        var result = await polling.OnContentCreatedOrUpdated(pollingRequest, input);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
