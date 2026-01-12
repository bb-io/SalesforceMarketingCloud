using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.SalesforceMarketing.Handlers.Static;

public class ContentBlockTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return 
        [
            new DataSourceItem("196", "Text block"),
            new DataSourceItem("197", "HTML block"),
            new DataSourceItem("195", "Freeform block")
        ];
    }
}
