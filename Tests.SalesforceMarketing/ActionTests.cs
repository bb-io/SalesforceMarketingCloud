using Apps.SalesforceMarketing.Actions;
using Tests.SalesforceMarketing.Base;

namespace Tests.AppSalesforceMarketingname;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Dynamic_handler_works()
    {
        var actions = new Actions(InvocationContext);

        await actions.Action();
    }
}
