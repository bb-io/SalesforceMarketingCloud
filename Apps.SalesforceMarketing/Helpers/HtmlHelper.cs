using HtmlAgilityPack;

namespace Apps.SalesforceMarketing.Helpers;

public static class HtmlHelper
{
    public static string InjectDivMetadata(string html, string content, string divId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
        var div = doc.CreateElement("div");
        div.Id = divId;
        div.InnerHtml = content;

        rootNode.PrependChild(div);
        return doc.DocumentNode.OuterHtml;
    }

    public static (string UpdatedHtml, string? Content) ExtractAndDeleteDivMetadata(string html, string divId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var div = doc.DocumentNode.SelectSingleNode($"//div[@id='{divId}']");
        if (div != null)
        {
            var subject = div.InnerText.Trim();
            div.Remove();
            return (doc.DocumentNode.OuterHtml, subject);
        }

        return (html, null);
    }
}
