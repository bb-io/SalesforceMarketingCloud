using HtmlAgilityPack;
using Apps.SalesforceMarketing.Constants;
using Apps.SalesforceMarketing.Models.Html;

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
        head.PrependChild(newMeta);

        return doc.DocumentNode.OuterHtml;
    }

    public static string? ExtractHeadMetadata(string html, string metadataId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var metaNode = doc.DocumentNode.SelectSingleNode($"//meta[@name='{metadataId}']");
        return metaNode?.GetAttributeValue("content", string.Empty);
    }

    public static string? ExtractContentId(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var knownContentId in BlackbirdMetadataIds.ContentTypeIds)
        {
            var metaNode = doc.DocumentNode.SelectSingleNode($"//meta[@name='{knownContentId}']");
            if (metaNode != null)
            {
                string contentValue = metaNode.GetAttributeValue("content", string.Empty);
                if (!string.IsNullOrEmpty(contentValue))
                    return contentValue;
            }
        }

        var rootBlockNode = doc.DocumentNode.SelectSingleNode("//blackbird-content-block[@data-root='true']");
        if (rootBlockNode != null)
        {
            string idAttribute = rootBlockNode.GetAttributeValue("id", string.Empty);
            if (!string.IsNullOrEmpty(idAttribute))
                return idAttribute;
        }

        return null;
    }

    public static string InjectDiv(string html, string? content, string divId)
    {
        if (string.IsNullOrEmpty(content))
            return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
        var div = doc.CreateElement("div");
        div.Id = divId;
        div.InnerHtml = content;
        rootNode.PrependChild(div);

        return doc.DocumentNode.OuterHtml;
    }

    public static ExtractedMetadataContent ExtractAndDeleteDiv(string html, string divId)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var div = doc.DocumentNode.SelectSingleNode($"//div[@id='{divId}']");
        if (div != null)
        {
            var subject = div.InnerText.Trim();
            div.Remove();
            return new(doc.DocumentNode.OuterHtml, subject);
        }

        return new(html, null);
    }
}
