using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.SalesforceMarketing.Base;

namespace Tests.SalesforceMarketing;

[TestClass]
public class HandlerTests : TestBase
{
    private readonly DataSourceContext _emptyDataSourceContext = new() { };

    [TestMethod]
    public async Task AssetDataHandler_ReturnsAssets()
    {
        // Arrange
        var handler = new AssetDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(_emptyDataSourceContext, CancellationToken.None);

        // Assert
        Console.WriteLine($"Total: {result.Count()}");
        foreach (var item in result)
            Console.WriteLine($"{item.Value}: {item.DisplayName}");
        Assert.IsNotEmpty(result);
    }
}
