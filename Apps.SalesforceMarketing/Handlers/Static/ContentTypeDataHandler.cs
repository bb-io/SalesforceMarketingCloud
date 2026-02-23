using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Apps.SalesforceMarketing.Constants;

namespace Apps.SalesforceMarketing.Handlers.Static;

public class ContentTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return
        [
            new DataSourceItem(AssetTypeIds.HtmlEmail, "HTML Email"),
            new DataSourceItem(AssetTypeIds.TextBlock, "Text Block"),
            new DataSourceItem(AssetTypeIds.FreeformBlock, "Freeform Block"),
            new DataSourceItem(AssetTypeIds.HtmlBlock, "HTML Block"),
        ];
    }
}
