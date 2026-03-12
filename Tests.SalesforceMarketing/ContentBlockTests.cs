using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Request.Block;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.SalesforceMarketing.Base;

namespace Tests.SalesforceMarketing;

[TestClass]
public class ContentBlockTests : TestBase
{
    private readonly ContentBlockActions _actions;

    public ContentBlockTests() => _actions = new ContentBlockActions(InvocationContext, FileManager);    

    [TestMethod]
    public async Task CreateContentBlock_ReturnsCreatedContentBlock()
    {
        // Arrange
        var request = new CreateContentBlockRequest
        {
            BlockTypeId = AssetTypeIds.FreeformBlock,
            BlockName = "Test freeform content block from the tests",
            CreateContentBlocksInOriginalFolder = true,
            ContentSuffix = "testsuffx",
            CategoryId = "1326002",
            Content = new FileReference { Name = "test.html" }
        };

        // Act
        var result = await _actions.CreateContentBlock(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DownloadContentBlock_IsSuccess()
    {
        // Arrange
        var blockInput = new ContentBlockIdentifier { ContentBlockId = "945692" };
        var input = new DownloadContentBlockRequest
        {

        };

        // Act
        var result = await _actions.DownloadContentBlock(blockInput, input);

        // Assert
        Console.WriteLine(result.Content.Name);
        Assert.IsNotNull(result.Content);
    }
}
