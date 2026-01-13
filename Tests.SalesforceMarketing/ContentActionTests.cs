using Tests.SalesforceMarketing.Base;
using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Request.Content;
using Blackbird.Applications.Sdk.Common.Files;

namespace Tests.SalesforceMarketing;

[TestClass]
public class ContentActionTests : TestBase
{
    [TestMethod]
    public async Task SearchContent_ReturnsAssets()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
        var input = new SearchContentRequest
        {
            CreatedFromDate = new DateTime(2026, 1, 13),
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
        var actions = new ContentActions(InvocationContext, FileManager);
        var emailId = new EmailIdentifier { EmailId = "670941" };

        // Act
        var result = await actions.GetEmailDetails(emailId);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task CreateContentBlock_ReturnsCreatedContentBlock()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
        var request = new CreateContentBlockRequest
        {
            TextContent = "test content freeform",
            AssetTypeId = "195",
            Name = "Test freeform content block from the tests"
        };

        // Act
        var result = await actions.CreateContentBlock(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DownloadEmail_IsSuccess()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
        var emailId = new EmailIdentifier { EmailId = "930504" };

        // Act
        var result = await actions.DownloadEmail(emailId);

        // Assert
        Assert.IsNotNull(result.Content);
    }

    [TestMethod]
    public async Task UploadEmail_ReturnsCreatedAsset()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
        var request = new UploadEmailRequest
        {
            Content = new FileReference { Name = "test.html" },
            //SubjectLine = "test subject from tests",
            EmailName = "test name 456"
        };

        // Act
        var result = await actions.UploadEmail(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
