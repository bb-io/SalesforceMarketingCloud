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
            CreatedFromDate = new DateTime(2026, 1, 12),
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
        var emailId = new EmailIdentifier { EmailId = "932683" };

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
        var emailId = new EmailIdentifier { EmailId = "932683" };
        var request = new DownloadEmailRequest { ContentBlockIdsToIgnore = ["933760"] };

        // Act
        var result = await actions.DownloadEmail(emailId, request);

        // Assert
        Assert.IsNotNull(result.Content);
        Console.WriteLine(result.Content.Name);
    }

    [TestMethod]
    public async Task CreateEmail_ReturnsCreatedAsset()
    {
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
        var request = new UploadEmailRequest
        {
            Content = new FileReference { Name = "test.html" },
            EmailName = "test ampscript update vars",
            //ScriptVariableNames =   [   "chkey",           "@jobtype",         "test"                  ],
            //ScriptVariableValues =  [   "updatedChKey",    "updatedJobType",   "this will not update"  ]
        };

        // Act
        var result = await actions.CreateEmail(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
