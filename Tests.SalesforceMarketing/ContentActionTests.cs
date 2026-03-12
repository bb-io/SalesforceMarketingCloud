using Tests.SalesforceMarketing.Base;
using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Models.Request.Content;
using Blackbird.Applications.Sdk.Common.Files;

namespace Tests.SalesforceMarketing;

[TestClass]
public class ContentActionTests : TestBase
{
    private readonly ContentActions _actions;

    public ContentActionTests() => _actions = new ContentActions(InvocationContext, FileManager);

    [TestMethod]
    public async Task SearchContent_ReturnsAssets()
    {
        // Arrange
        var input = new SearchContentRequest
        {
            CreatedFromDate = DateTime.UtcNow - TimeSpan.FromHours(1),
            IncludeSubfolders = true,
            CategoryId = "1325630",
        };

        // Act
        var result = await _actions.SearchContent(input);

        // Assert
        Console.WriteLine($"Count: {result.Items.Length}");
        PrintResult(result);
        Assert.IsNotEmpty(result.Items);
    }
        
    [TestMethod]
    public async Task GetContentIdFromFile_ReturnsContentId()
    {
        // Arrange
        var input = new GetContentIdFromFileRequest
        {
            Content = new FileReference { Name = "test.html" }
        };

        // Act
        var result = await _actions.GetContentIdFromFile(input);

        // Assert
        Console.WriteLine(result.ContentId);
        Assert.IsNotNull(result.ContentId);
    }
}
