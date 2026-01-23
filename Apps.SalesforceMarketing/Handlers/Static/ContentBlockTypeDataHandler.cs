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
            new DataSourceItem(AssetTypeIds.FreeformBlock.ToString(), "Freeform block"),
            new DataSourceItem(AssetTypeIds.TextBlock.ToString(), "Text block"),
            new DataSourceItem(AssetTypeIds.HtmlBlock.ToString(), "HTML block")
        ];
    }
}
