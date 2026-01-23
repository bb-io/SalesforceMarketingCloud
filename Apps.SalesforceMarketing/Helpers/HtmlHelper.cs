using HtmlAgilityPack;

namespace Apps.SalesforceMarketing.Helpers;

public static class HtmlHelper
{
    public static string InjectHeadMetadata(string html, string content, string metadataId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var head = doc.DocumentNode.SelectSingleNode("//head");
        if (head == null) 
            return html;

        var existingMetas = doc.DocumentNode.SelectNodes($"//meta[@name='{metadataId}']");
        if (existingMetas != null)
        {
            foreach (var meta in existingMetas)
                meta.Remove();
        }

        var newMeta = doc.CreateElement("meta");
        newMeta.SetAttributeValue("name", metadataId);
        newMeta.SetAttributeValue("content", content);

        head.PrependChild(doc.CreateTextNode("\r\n    "));
        head.PrependChild(newMeta);

        return doc.DocumentNode.OuterHtml;
    }

    public static string? ExtractHeadMetadata(string html, string metadataId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var metaNode = doc.DocumentNode.SelectSingleNode($"//meta[@name='{metadataId}']");
        return metaNode?.GetAttributeValue("content", null);
    }

    public static string InjectDivMetadata(string html, string? content, string divId)
    {
        if (string.IsNullOrEmpty(content))
            return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
        var div = doc.CreateElement("div");
        div.Id = divId;
        div.InnerHtml = content;

        rootNode.PrependChild(doc.CreateTextNode("\r\n    "));
        rootNode.PrependChild(div);
        rootNode.PrependChild(doc.CreateTextNode("\r\n    "));
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
