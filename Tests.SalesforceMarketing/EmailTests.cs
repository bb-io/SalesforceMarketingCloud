using Apps.SalesforceMarketing.Actions;
using Apps.SalesforceMarketing.Models.Identifiers;
using Apps.SalesforceMarketing.Models.Identifiers.Optional;
using Apps.SalesforceMarketing.Models.Request.Email;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.SalesforceMarketing.Base;

namespace Tests.SalesforceMarketing;

[TestClass]
public class EmailTests : TestBase
{
    private readonly EmailActions _actions;

    public EmailTests() => _actions = new EmailActions(InvocationContext, FileManager);

    [TestMethod]
    public async Task FindEmailByName_ExistingEmail_ReturnsEmailId()
    {
        // Arrange
        var input = new FindEmailByNameRequest 
        { 
            EmailName = "test",
            CategoryId = "1326002"
        };

        // Act
        var result = await _actions.FindEmailByName(input);

        // Assert
        Console.WriteLine(result.EmailId);
        Assert.IsNotNull(result.EmailId);
    }

    [TestMethod]
    public async Task GetEmailDetails_ReturnsEmailMetadata()
    {
        // Arrange
        var emailId = new EmailIdentifier { EmailId = "705627" };

        // Act
        var result = await _actions.GetEmailDetails(emailId);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DownloadEmail_IsSuccess()
    {
        // Arrange
        var emailId = new EmailIdentifier { EmailId = "940892" };
        var request = new DownloadEmailRequest
        {
            DownloadHtmlEmailContent = true,
            DownloadPlaintextEmailContent = true,
            ExtractAllScriptVariables = true,
        };

        // Act
        var result = await _actions.DownloadEmail(emailId, request);

        // Assert
        Assert.IsNotNull(result.Content);
        Console.WriteLine(result.Content.Name);
    }

    [TestMethod]
    public async Task CreateEmail_ReturnsCreatedAsset()
    {
        // Arrange
        var request = new UploadEmailRequest
        {
            Content = new FileReference { Name = "test.html" },
            EmailName = "test context suffixes",
            CategoryId = "1326002",
            CreateContentBlocksInOriginalFolder = true,
            ContentSuffix = "(de-AT)"
            //ScriptVariableNames =   [   "chkey",           "@jobtype",         "test"                  ],
            //ScriptVariableValues =  [   "updatedChKey",    "updatedJobType",   "this will not update"  ]
        };

        // Act
        var result = await _actions.CreateEmail(request);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateEmail_ReturnsUpdatedEmail()
    {
        // Arrange
        var emailId = new OptionalEmailIdentifier { /*EmailId = "940892"*/ };
        var input = new UpdateEmailRequest
        {
            Content = new FileReference { Name = "test.html" },
            //SubjectLine = "overwritten"
        };

        // Act
        var result = await _actions.UpdateEmail(emailId, input);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
