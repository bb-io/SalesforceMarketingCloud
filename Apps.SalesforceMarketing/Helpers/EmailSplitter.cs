using Apps.SalesforceMarketing.Constants;
using HtmlAgilityPack;

namespace Apps.SalesforceMarketing.Helpers;

public static class EmailSplitter
{
    public static string MergeViews(string? htmlContent, string? plaintextContent)
    {
        var doc = new HtmlDocument();
        var htmlNode = doc.CreateElement("html");
        var headNode = doc.CreateElement("head");
        var bodyNode = doc.CreateElement("body");

        doc.DocumentNode.AppendChild(htmlNode);
        htmlNode.AppendChild(headNode);
        htmlNode.AppendChild(bodyNode);

        if (!string.IsNullOrEmpty(htmlContent))
        {
            var sourceDoc = new HtmlDocument();
            sourceDoc.LoadHtml(htmlContent);

            var sourceHead = sourceDoc.DocumentNode.SelectSingleNode("//head");
            if (sourceHead != null)
                headNode.InnerHtml = sourceHead.InnerHtml;

            var htmlViewDiv = doc.CreateElement("div");
            htmlViewDiv.SetAttributeValue("id", BlackbirdMetadataIds.HtmlEmailView);

            var sourceBody = sourceDoc.DocumentNode.SelectSingleNode("//body");
            htmlViewDiv.InnerHtml = sourceBody != null ? sourceBody.InnerHtml : sourceDoc.DocumentNode.InnerHtml;

            bodyNode.AppendChild(htmlViewDiv);
        }

        if (!string.IsNullOrEmpty(plaintextContent))
        {
            var textViewDiv = doc.CreateElement("div");

            textViewDiv.SetAttributeValue("id", BlackbirdMetadataIds.PlainTextView);
            textViewDiv.InnerHtml = plaintextContent;

            bodyNode.AppendChild(textViewDiv);
        }

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

        string? finalHtmlContent = null;
        string? finalPlaintextContent = null;

        if (htmlViewNode != null)
        {
            var newDoc = new HtmlDocument();
            var htmlNode = newDoc.CreateElement("html");
            var headNode = newDoc.CreateElement("head");
            var bodyNode = newDoc.CreateElement("body");

            newDoc.DocumentNode.AppendChild(htmlNode);
            htmlNode.AppendChild(headNode);
            htmlNode.AppendChild(bodyNode);

            var originalHead = doc.DocumentNode.SelectSingleNode("//head");
            if (originalHead != null)
                headNode.InnerHtml = originalHead.InnerHtml;

            bodyNode.InnerHtml = htmlViewNode.InnerHtml;
            finalHtmlContent = newDoc.DocumentNode.OuterHtml;
        }

        if (plaintextViewNode != null)
            finalPlaintextContent = plaintextViewNode.InnerHtml;

        return (finalHtmlContent, finalPlaintextContent);
    }
}
