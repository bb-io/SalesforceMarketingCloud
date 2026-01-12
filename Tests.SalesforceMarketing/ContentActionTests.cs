using Tests.SalesforceMarketing.Base;
using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Request.Content;

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
        var assetId = new EmailIdentifier { EmailId = "670941" };

        // Act
        var result = await actions.GetEmailDetails(assetId);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task CreateContentBlock_ReturnsCreatedContentBlock()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext);
        var request = new CreateContentBlockRequest
        {
            Content = "test content freeform",
            AssetTypeId = "195",
            Name = "Test freeform content block from the tests"
        };

        // Act
        var result = await actions.CreateContentBlock(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
