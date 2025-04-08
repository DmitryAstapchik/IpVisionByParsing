using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.XPath;

namespace IpVisionByParsing;

internal class SiteParserService(string homePage)
{
    private readonly IBrowsingContext _context =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    internal readonly HtmlDocumentParser HtmlDocumentParser = new(homePage);

    internal async Task ParseCatalog()
    {
        var categories = await GetLinksFromPage("catalog");
        foreach (var category in categories)
        {
            var subcategories = await GetLinksFromPage(category.Key);
            if (subcategories.Count > 0)
            {
                foreach (var subcategory in subcategories)
                {
                    var document =
                        await _context.OpenAsync(new Uri(new Uri(homePage), subcategory.Value)
                            .AbsoluteUri);
                    await ParseAllCardsInCategoryOrSubcategory(document, category, subcategory);
                }
            }
            else
            {
                var document =
                    await _context.OpenAsync(new Uri(new Uri(homePage), category.Value).AbsoluteUri);
                await ParseAllCardsInCategoryOrSubcategory(document, category);
            }
        }
    }

    private async Task ParseAllCardsInCategoryOrSubcategory(IDocument startDocument,
        KeyValuePair<string, string> category,
        KeyValuePair<string, string> subcategory = default)
    {
        HtmlDocumentParser.ParseCardsOnPage(startDocument, category, subcategory);
        var nextLink = startDocument.Body.SelectSingleNode("//a[contains(@class, 'next')]");
        while (nextLink != null)
        {
            var href = ((IElement)nextLink).GetAttribute("href");
            var document = await _context.OpenAsync(new Uri(new Uri(homePage), href).AbsoluteUri);
            HtmlDocumentParser.ParseCardsOnPage(document, category, subcategory);
            nextLink = document.Body.SelectSingleNode("//a[contains(@class, 'next')]");
        }
    }

    private async Task<Dictionary<string, string>> GetLinksFromPage(string relativeUrl)
    {
        var document = await _context.OpenAsync(new Uri(new Uri(homePage), relativeUrl).AbsoluteUri);
        return HtmlDocumentParser.ExtractLinks(document);
    }
}