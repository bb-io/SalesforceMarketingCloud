using Tests.SalesforceMarketing.Base;
using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Models.Request;
using Apps.SalesforceMarketing.Models.Identifiers;

namespace Tests.SalesforceMarketing;

[TestClass]
public class ContentActionTests : TestBase
{
    [TestMethod]
    public async Task SearchContent_ReturnsAssets()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext);
        var input = new SearchContentRequest
        {
            CreatedFromDate = new DateTime(2025, 10, 06, 11, 12, 00),
            CreatedToDate = new DateTime(2025, 10, 06, 11, 14, 00),
        };

        // Act
        var result = await actions.SearchContent(input);

        // Assert
        Console.WriteLine($"Count: {result.Items.Length}");
        PrintResult(result);
        Assert.IsNotEmpty(result.Items);
    }

    [TestMethod]
    public async Task GetEmailDetails_ReturnsEmailMetadata()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext);
        var assetId = new AssetIdentifier { AssetId = "692794" };

        // Act
        var result = await actions.GetEmailDetails(assetId);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
