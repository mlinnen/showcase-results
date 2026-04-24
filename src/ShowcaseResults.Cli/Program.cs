using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using ShowcaseResults.Models;
using ShowcaseResults.Parsing;
using ShowcaseResults.Rendering;

static HashSet<int> CollectFallbackCheckedInCarverIds(
    IEnumerable<SpecialPrize> specialPrizes,
    IEnumerable<OverallCategory> overallResults,
    IEnumerable<DivisionResult> divisionResults)
{
    var checkedInCarverIds = new HashSet<int>(specialPrizes.Select(prize => prize.CarverId));

    foreach (var place in overallResults.SelectMany(category => category.Places))
        checkedInCarverIds.Add(place.CarverId);

    foreach (var place in divisionResults.SelectMany(division => division.Categories).SelectMany(category => category.Places))
        checkedInCarverIds.Add(place.CarverId);

    return checkedInCarverIds;
}

var rootCommand = new RootCommand("showcase-results CLI");

var createCommand = new Command("create", "Create showcase results output");
var resultsCommand = new Command("results", "Generate results article HTML");

var eventNameOption = new Option<string>(
    "--event-name",
    () => "Showcase of Woodcarvings",
    "Name of the event");
var eventOption = new Option<string>(
    "--event",
    () => DateTime.Now.Year.ToString(),
    "Event identifier (e.g. 2024, 2026T)");
var dataRootOption = new Option<string?>(
    "--data-root",
    () => null,
    "Parent path for default input/output locations (e.g. C:\\tools\\cli\\data)");
var competitorsOption = new Option<string?>(
    "--competitors",
    () => null,
    "Path to Competitor.xlsx (default: {data-root}/input/Competitor.xlsx or data/input/Competitor.xlsx)");
var prizesOption = new Option<string?>(
    "--prizes",
    () => null,
    "Path to Prizes.xlsx (default: {data-root}/input/Prizes.xlsx or data/input/Prizes.xlsx)");
var judgingOption = new Option<string?>(
    "--judging",
    () => null,
    "Path to Judging.xlsx (default: {data-root}/input/Judging.xlsx or data/input/Judging.xlsx)");
var outputOption = new Option<string?>(
    "--output",
    () => null,
    "Path for the output article.html (default: {data-root}/output/article.html or output/article.html)");

var formatOption = new Option<string[]>(
    "--format",
    () => new[] { "html" },
    "Output format(s): html writes the results article to the --output path; json writes results-{event}.json to the same directory (used by the Joomla data layer). Repeatable: --format html --format json");
formatOption.AllowMultipleArgumentsPerToken = true;

resultsCommand.AddOption(eventNameOption);
resultsCommand.AddOption(eventOption);
resultsCommand.AddOption(dataRootOption);
resultsCommand.AddOption(competitorsOption);
resultsCommand.AddOption(prizesOption);
resultsCommand.AddOption(judgingOption);
resultsCommand.AddOption(outputOption);
resultsCommand.AddOption(formatOption);

resultsCommand.SetHandler((InvocationContext ctx) =>
{
    var eventName   = ctx.ParseResult.GetValueForOption(eventNameOption)!;
    var eventId     = ctx.ParseResult.GetValueForOption(eventOption)!;
    var dataRoot    = ctx.ParseResult.GetValueForOption(dataRootOption);
    dataRoot = string.IsNullOrWhiteSpace(dataRoot) ? null : dataRoot;
    var competitorsExplicit = ctx.ParseResult.GetValueForOption(competitorsOption);
    var prizesExplicit      = ctx.ParseResult.GetValueForOption(prizesOption);
    var judgingExplicit     = ctx.ParseResult.GetValueForOption(judgingOption);
    var outputExplicit      = ctx.ParseResult.GetValueForOption(outputOption);
    
    var competitors = Path.GetFullPath(competitorsExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Competitor.xlsx") : Path.Join("data", "input", "Competitor.xlsx")));
    var prizes = Path.GetFullPath(prizesExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Prizes.xlsx") : Path.Join("data", "input", "Prizes.xlsx")));
    var judging = Path.GetFullPath(judgingExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Judging.xlsx") : Path.Join("data", "input", "Judging.xlsx")));
    var output = Path.GetFullPath(outputExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "output", "article.html") : Path.Join("output", "article.html")));
    
    var formats     = new HashSet<string>(
        ctx.ParseResult.GetValueForOption(formatOption) ?? new[] { "html" },
        StringComparer.OrdinalIgnoreCase);

    Console.WriteLine($"Parsing spreadsheets for {eventName} {eventId}...");

    var parser = new SpreadsheetParser(competitors, prizes, judging);

    var (competitorList, checkedInColumn) = parser.ParseCompetitorsForJson();
    var specialPrizes  = parser.ParseSpecialPrizes();
    var (overallResults, divisionResults) = parser.ParseJudging();

    if (checkedInColumn == null)
    {
        var checkedInCarverIds = CollectFallbackCheckedInCarverIds(specialPrizes, overallResults, divisionResults);
        competitorList = competitorList
            .Where(competitor => checkedInCarverIds.Contains(competitor.CarverId))
            .ToList();

        Console.WriteLine("  Competitor.xlsx has no checked-in column; using prize/result rows only as a backward-compatible fallback.");
    }
    else
    {
        Console.WriteLine($"  Using Competitor.xlsx \"{checkedInColumn}\" column as the JSON competitors source of truth.");
    }

    Console.WriteLine($"  {specialPrizes.Count} special prizes");
    Console.WriteLine($"  {overallResults.Count} overall result categories");
    Console.WriteLine($"  {divisionResults.Count} divisions ({string.Join(", ", divisionResults.Select(d => d.Division))})");
    Console.WriteLine($"  {competitorList.Count} competitors");

    var data = new ShowcaseResultsData(
        new EventInfo(eventName, eventId),
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
        Console.WriteLine($"\u2713 Wrote {Path.GetRelativePath(Environment.CurrentDirectory, output)}");
    }

    if (formats.Contains("json"))
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var jsonPath = Path.Join(outputDir, $"results-{data.Event.EventId}.json");
        var json = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(jsonPath, json, new System.Text.UTF8Encoding(false));
        Console.WriteLine($"\u2713 Wrote {Path.GetRelativePath(Environment.CurrentDirectory, jsonPath)}");
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
    "Path for the output HTML file (default: {data-root}/output/carver-{id}.html or output/carver-{id}.html)");

carverArticleCommand.AddOption(eventNameOption);
carverArticleCommand.AddOption(eventOption);
carverArticleCommand.AddOption(dataRootOption);
carverArticleCommand.AddOption(competitorsOption);
carverArticleCommand.AddOption(prizesOption);
carverArticleCommand.AddOption(judgingOption);
carverArticleCommand.AddOption(carverIdOption);
carverArticleCommand.AddOption(carverNameOption);
carverArticleCommand.AddOption(carverOutputOption);

carverArticleCommand.SetHandler((InvocationContext ctx) =>
{
    var eventName   = ctx.ParseResult.GetValueForOption(eventNameOption)!;
    var eventId     = ctx.ParseResult.GetValueForOption(eventOption)!;
    var dataRoot    = ctx.ParseResult.GetValueForOption(dataRootOption);
    dataRoot = string.IsNullOrWhiteSpace(dataRoot) ? null : dataRoot;
    var competitorsExplicit = ctx.ParseResult.GetValueForOption(competitorsOption);
    var prizesExplicit      = ctx.ParseResult.GetValueForOption(prizesOption);
    var judgingExplicit     = ctx.ParseResult.GetValueForOption(judgingOption);
    
    var competitors = Path.GetFullPath(competitorsExplicit
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Competitor.xlsx") : Path.Join("data", "input", "Competitor.xlsx")));
    var prizes = Path.GetFullPath(prizesExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Prizes.xlsx") : Path.Join("data", "input", "Prizes.xlsx")));
    var judging = Path.GetFullPath(judgingExplicit 
        ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Judging.xlsx") : Path.Join("data", "input", "Judging.xlsx")));
    var carverId    = ctx.ParseResult.GetValueForOption(carverIdOption);
    var carverName  = ctx.ParseResult.GetValueForOption(carverNameOption);
    var outputPath  = ctx.ParseResult.GetValueForOption(carverOutputOption);

    if (!carverId.HasValue && string.IsNullOrWhiteSpace(carverName))
    {
        Console.Error.WriteLine("Error: must specify --carver-id or --carver-name");
        ctx.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Parsing spreadsheets for {eventName} {eventId}...");

    var parser = new SpreadsheetParser(competitors, prizes, judging);

    var competitorList = parser.ParseCompetitors();
    var specialPrizes  = parser.ParseSpecialPrizes();
    var (overallResults, divisionResults) = parser.ParseJudging();

    var data = new ShowcaseResultsData(
        new EventInfo(eventName, eventId),
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
        : Path.GetFullPath(dataRoot != null 
            ? Path.Join(dataRoot, "output", $"carver-{result.CarverId}.html")
            : Path.Join("output", $"carver-{result.CarverId}.html"));

    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
    File.WriteAllText(outFile, result.Html, System.Text.Encoding.UTF8);

    Console.WriteLine($"\u2713 Wrote {Path.GetRelativePath(Environment.CurrentDirectory, outFile)}");
});

createCommand.AddCommand(resultsCommand);
createCommand.AddCommand(carverArticleCommand);
rootCommand.AddCommand(createCommand);

return await rootCommand.InvokeAsync(args);
