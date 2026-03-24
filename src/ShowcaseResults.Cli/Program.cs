using System.CommandLine;
using System.CommandLine.Invocation;
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
var yearOption = new Option<int>(
    "--year",
    () => DateTime.Now.Year,
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

resultsCommand.AddOption(eventNameOption);
resultsCommand.AddOption(yearOption);
resultsCommand.AddOption(competitorsOption);
resultsCommand.AddOption(prizesOption);
resultsCommand.AddOption(judgingOption);
resultsCommand.AddOption(outputOption);

resultsCommand.SetHandler((InvocationContext ctx) =>
{
    var eventName   = ctx.ParseResult.GetValueForOption(eventNameOption)!;
    var year        = ctx.ParseResult.GetValueForOption(yearOption);
    var competitors = Path.GetFullPath(ctx.ParseResult.GetValueForOption(competitorsOption)!);
    var prizes      = Path.GetFullPath(ctx.ParseResult.GetValueForOption(prizesOption)!);
    var judging     = Path.GetFullPath(ctx.ParseResult.GetValueForOption(judgingOption)!);
    var output      = Path.GetFullPath(ctx.ParseResult.GetValueForOption(outputOption)!);

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

    var renderer = new ArticleRenderer();
    var html = renderer.RenderArticle(data);

    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    File.WriteAllText(output, html, System.Text.Encoding.UTF8);

    Console.WriteLine($"\u2713 Wrote {output}");
});

createCommand.AddCommand(resultsCommand);
rootCommand.AddCommand(createCommand);

return await rootCommand.InvokeAsync(args);
