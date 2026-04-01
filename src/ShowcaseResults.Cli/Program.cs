using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using ShowcaseResults.Models;
using ShowcaseResults.Parsing;
using ShowcaseResults.Rendering;

var rootCommand = new RootCommand("showcase-results CLI");

var createCommand = new Command("create", "Create showcase results output");
var resultsCommand = new Command("results", "Generate results article HTML");

var eventNameOption = new Option<string>(
    "--event-name",
    () => "Showcase of Woodcarvings",
    "Name of the event");
var yearOption = new Option<string>(
    "--year",
    () => DateTime.Now.Year.ToString(),
    "Year of the event");
var competitorsOption = new Option<string>(
    "--competitors",
    () => Path.Join("data", "input", "Competitor.xlsx"),
    "Path to Competitor.xlsx");
var prizesOption = new Option<string>(
    "--prizes",
    () => Path.Join("data", "input", "Prizes.xlsx"),
    "Path to Prizes.xlsx");
var judgingOption = new Option<string>(
    "--judging",
    () => Path.Join("data", "input", "Judging.xlsx"),
    "Path to Judging.xlsx");
var outputOption = new Option<string>(
    "--output",
    () => Path.Join("output", "article.html"),
    "Path for the output article.html");

var formatOption = new Option<string[]>(
    "--format",
    () => new[] { "html" },
    "Output format(s): html, json (can be repeated: --format html --format json)");
formatOption.AllowMultipleArgumentsPerToken = true;

resultsCommand.AddOption(eventNameOption);
resultsCommand.AddOption(yearOption);
resultsCommand.AddOption(competitorsOption);
resultsCommand.AddOption(prizesOption);
resultsCommand.AddOption(judgingOption);
resultsCommand.AddOption(outputOption);
resultsCommand.AddOption(formatOption);

resultsCommand.SetHandler((InvocationContext ctx) =>
{
    var eventName   = ctx.ParseResult.GetValueForOption(eventNameOption)!;
    var year        = ctx.ParseResult.GetValueForOption(yearOption)!;
    var competitors = Path.GetFullPath(ctx.ParseResult.GetValueForOption(competitorsOption)!);
    var prizes      = Path.GetFullPath(ctx.ParseResult.GetValueForOption(prizesOption)!);
    var judging     = Path.GetFullPath(ctx.ParseResult.GetValueForOption(judgingOption)!);
    var output      = Path.GetFullPath(ctx.ParseResult.GetValueForOption(outputOption)!);
    var formats     = new HashSet<string>(
        ctx.ParseResult.GetValueForOption(formatOption) ?? new[] { "html" },
        StringComparer.OrdinalIgnoreCase);

    Console.WriteLine($"Parsing spreadsheets for {eventName} {year}...");

    var parser = new SpreadsheetParser(competitors, prizes, judging);

    var competitorList = parser.ParseCompetitors();
    var specialPrizes  = parser.ParseSpecialPrizes();
    var (overallResults, divisionResults) = parser.ParseJudging();

    Console.WriteLine($"  {specialPrizes.Count} special prizes");
    Console.WriteLine($"  {overallResults.Count} overall result categories");
    Console.WriteLine($"  {divisionResults.Count} divisions ({string.Join(", ", divisionResults.Select(d => d.Division))})");
    Console.WriteLine($"  {competitorList.Count} competitors");

    var data = new ShowcaseResultsData(
        new EventInfo(eventName, year),
        specialPrizes,
        overallResults,
        divisionResults,
        competitorList);

    var outputDir = Path.GetDirectoryName(output)!;
    Directory.CreateDirectory(outputDir);

    if (formats.Contains("html"))
    {
        var renderer = new ArticleRenderer();
        var html = renderer.RenderArticle(data);
        File.WriteAllText(output, html, System.Text.Encoding.UTF8);
        Console.WriteLine($"\u2713 Wrote {output}");
    }

    if (formats.Contains("json"))
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var jsonPath = Path.Join(outputDir, $"results-{data.Event.Year}.json");
        var json = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(jsonPath, json, System.Text.Encoding.UTF8);
        Console.WriteLine($"\u2713 Wrote {jsonPath}");
    }
});

// --- create carver-article ---
var carverArticleCommand = new Command("carver-article", "Generate a per-carver results article HTML");

var carverIdOption = new Option<int?>(
    "--carver-id",
    () => null,
    "Carver ID to generate article for");
var carverNameOption = new Option<string?>(
    "--carver-name",
    () => null,
    "Carver full name to generate article for (e.g. \"John Doe\")");
var carverOutputOption = new Option<string?>(
    "--output",
    () => null,
    "Path for the output HTML file (default: output/carver-{id}.html)");

carverArticleCommand.AddOption(eventNameOption);
carverArticleCommand.AddOption(yearOption);
carverArticleCommand.AddOption(competitorsOption);
carverArticleCommand.AddOption(prizesOption);
carverArticleCommand.AddOption(judgingOption);
carverArticleCommand.AddOption(carverIdOption);
carverArticleCommand.AddOption(carverNameOption);
carverArticleCommand.AddOption(carverOutputOption);

carverArticleCommand.SetHandler((InvocationContext ctx) =>
{
    var eventName   = ctx.ParseResult.GetValueForOption(eventNameOption)!;
    var year        = ctx.ParseResult.GetValueForOption(yearOption)!;
    var competitors = Path.GetFullPath(ctx.ParseResult.GetValueForOption(competitorsOption)!);
    var prizes      = Path.GetFullPath(ctx.ParseResult.GetValueForOption(prizesOption)!);
    var judging     = Path.GetFullPath(ctx.ParseResult.GetValueForOption(judgingOption)!);
    var carverId    = ctx.ParseResult.GetValueForOption(carverIdOption);
    var carverName  = ctx.ParseResult.GetValueForOption(carverNameOption);
    var outputPath  = ctx.ParseResult.GetValueForOption(carverOutputOption);

    if (!carverId.HasValue && string.IsNullOrWhiteSpace(carverName))
    {
        Console.Error.WriteLine("Error: must specify --carver-id or --carver-name");
        ctx.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Parsing spreadsheets for {eventName} {year}...");

    var parser = new SpreadsheetParser(competitors, prizes, judging);

    var competitorList = parser.ParseCompetitors();
    var specialPrizes  = parser.ParseSpecialPrizes();
    var (overallResults, divisionResults) = parser.ParseJudging();

    var data = new ShowcaseResultsData(
        new EventInfo(eventName, year),
        specialPrizes,
        overallResults,
        divisionResults,
        competitorList);

    var renderer = new ArticleRenderer();
    var result = renderer.RenderCarverArticle(data, carverId, carverName);

    if (result == null)
    {
        var lookup = carverId.HasValue ? $"ID {carverId.Value}" : $"\"{carverName}\"";
        Console.Error.WriteLine($"Error: carver {lookup} not found in competitors list.");
        ctx.ExitCode = 1;
        return;
    }

    if (!result.HasResults)
        Console.WriteLine($"Warning: No results found for {result.FullName}.");

    var outFile = !string.IsNullOrWhiteSpace(outputPath)
        ? Path.GetFullPath(outputPath)
        : Path.GetFullPath(Path.Join("output", $"carver-{result.CarverId}.html"));

    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
    File.WriteAllText(outFile, result.Html, System.Text.Encoding.UTF8);

    Console.WriteLine($"\u2713 Wrote {outFile}");
});

createCommand.AddCommand(resultsCommand);
createCommand.AddCommand(carverArticleCommand);
rootCommand.AddCommand(createCommand);

return await rootCommand.InvokeAsync(args);
