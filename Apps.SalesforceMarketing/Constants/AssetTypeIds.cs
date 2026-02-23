namespace Apps.SalesforceMarketing.Constants;

public static class AssetTypeIds
{
    public const string FreeformBlock = "195";
    public const string TextBlock = "196";
    public const string HtmlBlock = "197";
    public const string HtmlEmail = "208";

    public static readonly List<string> SupportedAssetTypes = [FreeformBlock, TextBlock, HtmlBlock, HtmlEmail];
}
