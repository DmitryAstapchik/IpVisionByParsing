using IpVisionByParsing;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

var configuration = builder.Build();

var homePage = configuration["HomePage"];
var credentialsPath = configuration["CredentialsPath"];
var spreadsheetId = configuration["SpreadsheetId"];
var range = configuration["Range"];

var siteParserService = new SiteParserService(homePage!);
await siteParserService.ParseCatalog();

var googleSheetsService = new GoogleSheetsService(credentialsPath!, spreadsheetId!, range!);
await googleSheetsService.WriteDataToSheet(siteParserService.HtmlDocumentParser.ParsedProducts);

Console.Read();