using Apps.SalesforceMarketing.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Tests.SalesforceMarketing.Base;

namespace Tests.SalesforceMarketing;

[TestClass]
public class HandlerTests : TestBase
{
    private readonly DataSourceContext _emptyDataSourceContext = new() { };

    [TestMethod]
    public async Task EmailDataHandler_ReturnsEmails()
    {
        // Arrange
        var handler = new EmailDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(_emptyDataSourceContext, CancellationToken.None);

        // Assert
        Console.WriteLine($"Total: {result.Count()}");
        foreach (var item in result)
            Console.WriteLine($"{item.Value}: {item.DisplayName}");
        Assert.IsNotEmpty(result);
    }

    [TestMethod]
    public async Task CategoryDataHandler_GetFolderContentAsync_ReturnsCategoriesAsFolders()
    {
        // Arrange
        var handler = new CategoryDataHandler(InvocationContext);

        // Act
        var result = await handler.GetFolderContentAsync(new FolderContentDataSourceContext { FolderId = "937292" }, default);

        // Assert
        Console.WriteLine($"Total: {result.Count()}");
        foreach (var item in result)
            Console.WriteLine($"{item.Id}: {item.DisplayName}");
        Assert.IsNotEmpty(result);
    }
}
