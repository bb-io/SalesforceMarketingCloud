using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Apps.SalesforceMarketing.Constants;

namespace Apps.SalesforceMarketing.Handlers.Static;

public class ContentBlockTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return 
        [
            new DataSourceItem(AssetTypeIds.FreeformBlock, "Freeform block"),
            new DataSourceItem(AssetTypeIds.TextBlock, "Text block"),
            new DataSourceItem(AssetTypeIds.HtmlBlock, "HTML block")
        ];
    }
}
