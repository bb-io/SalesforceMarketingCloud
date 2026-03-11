namespace Apps.SalesforceMarketing.Constants;

public static class AssetTypeIds
{
    public const string FreeformBlock = "195";
    public const string TextBlock = "196";
    public const string HtmlBlock = "197";
    public const string HtmlEmail = "208";

    public static readonly string[] SupportedAssetTypes = [FreeformBlock, TextBlock, HtmlBlock, HtmlEmail];
    public static readonly string[] ContentBlockTypes = [FreeformBlock, TextBlock, HtmlBlock];
}
