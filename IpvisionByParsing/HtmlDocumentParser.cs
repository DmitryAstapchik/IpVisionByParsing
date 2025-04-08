using AngleSharp.Dom;
using AngleSharp.XPath;

namespace IpVisionByParsing;

internal class HtmlDocumentParser(string homePage)
{
    private int _productId = 1;

    internal List<ProductInfo> ParsedProducts = [];

    internal void ParseCardsOnPage(IDocument document, KeyValuePair<string, string> category,
        KeyValuePair<string, string> subcategory)
    {
        var cards = document.Body?.SelectNodes("//div[contains(@class, 'uk-card ')]");
        if (cards == null) return;
        foreach (var card in cards.Cast<IElement>())
        {
            var productLink = new Uri(new Uri(homePage),
                card.QuerySelector("div.uk-card-body a")?.GetAttribute("href")).AbsoluteUri;
            var productName = card.QuerySelector("h4")?.TextContent.Trim();
            var price = card.QuerySelector("ul.uk-list > li")?.TextContent.Trim() ??
                        card.QuerySelector("ul.uk-list > div#not_available")?.TextContent.Trim();
            var description = card.QuerySelector("div.uk-text-small")?.TextContent.Trim();
            var imageSrc = card.QuerySelector("img.el-image")?.GetAttribute("src");

            ParsedProducts.Add(
                new ProductInfo
                {
                    Id = _productId,
                    Category = category.Key,
                    CategoryLink = new Uri(new Uri(homePage), category.Value).AbsoluteUri,
                    Subcategory = subcategory.Key,
                    SubcategoryLink = new Uri(new Uri(homePage), subcategory.Value).AbsoluteUri,
                    ProductName = productName,
                    ProductLink = productLink,
                    ImageSrc = imageSrc,
                    ImageFormula = $"=IMAGE(H{_productId + 3})",
                    Description = description,
                    Price = price
                }
            );
            _productId++;
        }
    }

    internal static Dictionary<string, string> ExtractLinks(IDocument document)
    {
        var links = document.Body.SelectNodes("//a[contains(@class, 'product_link')]");
        var result = new Dictionary<string, string>();
        foreach (var link in links)
        {
            var text = link.TextContent.Trim();
            var href = ((IElement)link).GetAttribute("href");
            result[text] = href!;
        }

        return result;
    }
}