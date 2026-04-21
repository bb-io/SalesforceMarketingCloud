using Apps.SalesforceMarketing.Constants;
using HtmlAgilityPack;

namespace Apps.SalesforceMarketing.Helpers;

public static class EmailSplitter
{
    public static string MergeViews(string? htmlContent, string? plaintextContent)
    {
        var doc = new HtmlDocument();

        if (!string.IsNullOrWhiteSpace(htmlContent))
        {
            doc.LoadHtml(htmlContent);
            
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                string originalBodyContent = bodyNode.InnerHtml;
            
                bodyNode.RemoveAllChildren();
                bodyNode.AppendChild(CreateWrapperNode(doc, BlackbirdMetadataIds.HtmlEmailView, originalBodyContent));

                if (!string.IsNullOrWhiteSpace(plaintextContent))
                    bodyNode.AppendChild(CreateWrapperNode(doc, BlackbirdMetadataIds.PlainTextView, plaintextContent));

                return doc.DocumentNode.OuterHtml;
            }
        }

        doc = new HtmlDocument();
        var htmlNode = doc.CreateElement("html");
        var headNode = doc.CreateElement("head");
        var newBodyNode = doc.CreateElement("body");

        doc.DocumentNode.AppendChild(htmlNode);
        htmlNode.AppendChild(headNode);
        htmlNode.AppendChild(newBodyNode);

        if (!string.IsNullOrWhiteSpace(htmlContent))
            newBodyNode.AppendChild(CreateWrapperNode(doc, BlackbirdMetadataIds.HtmlEmailView, htmlContent));

        if (!string.IsNullOrWhiteSpace(plaintextContent))
            newBodyNode.AppendChild(CreateWrapperNode(doc, BlackbirdMetadataIds.PlainTextView, plaintextContent));

        return doc.DocumentNode.OuterHtml;
    }
    
    public static (string? HtmlView, string? PlaintextView) ExtractViews(string masterHtml)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(masterHtml);

        var htmlViewNode = doc.DocumentNode.SelectSingleNode($"//div[@id='{BlackbirdMetadataIds.HtmlEmailView}']");
        var plaintextViewNode = doc.DocumentNode.SelectSingleNode($"//div[@id='{BlackbirdMetadataIds.PlainTextView}']");

        if (htmlViewNode == null && plaintextViewNode == null)
            return (masterHtml, null);

        string? finalPlaintextContent = null;
        string? finalHtmlContent = null;

        if (plaintextViewNode != null)
        {
            finalPlaintextContent = plaintextViewNode.InnerText;
            plaintextViewNode.Remove();
        }

        if (htmlViewNode != null)
        {
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                var innerNodes = htmlViewNode.ChildNodes.ToList();
                bodyNode.RemoveAllChildren();

                foreach (var node in innerNodes)
                    bodyNode.AppendChild(node);
            }
            
            finalHtmlContent = doc.DocumentNode.OuterHtml;
        }

        return (finalHtmlContent, finalPlaintextContent);
    }
    
    private static HtmlNode CreateWrapperNode(HtmlDocument doc, string metadataId, string content)
    {
        var wrapperDiv = doc.CreateElement("div");
        wrapperDiv.SetAttributeValue("id", metadataId);
        wrapperDiv.InnerHtml = content;
    
        return wrapperDiv;
    }
}
