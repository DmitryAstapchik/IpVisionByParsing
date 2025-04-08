using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace IpVisionByParsing;

internal class GoogleSheetsService(string credentialsPath, string spreadsheetId, string range)
{
    internal async Task WriteDataToSheet(IEnumerable<ProductInfo> products)
    {
        GoogleCredential credential;
        await using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(SheetsService.Scope.Spreadsheets);
        }

        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "IpVisionByParsing"
        });

        var valueRange = new ValueRange
        {
            Values = CastProductsToGoogleSheetsObjects(products)
        };

        var request = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await request.ExecuteAsync();
    }

    private static List<IList<object?>> CastProductsToGoogleSheetsObjects(IEnumerable<ProductInfo> products)
    {
        var list = products.Select(product => (IList<object?>)
        [
            product.Id, product.Category, product.CategoryLink, product.Subcategory, product.SubcategoryLink,
            product.ProductName, product.ProductLink, product.ImageSrc, product.ImageFormula, product.Description,
            product.Price
        ]).ToList();

        return list;
    }
}