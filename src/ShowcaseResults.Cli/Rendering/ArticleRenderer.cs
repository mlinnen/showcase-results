using System.Text.RegularExpressions;
using ShowcaseResults.Models;

namespace ShowcaseResults.Rendering;

public class ArticleRenderer
{
    private static string Esc(string? str)
    {
        if (str == null) return "";
        return str
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string StyleLabel(string? style)
    {
        if (style == null) return "";
        if (style == "N") return "Natural";
        if (style == "P") return "Painted";
        return Esc(style);
    }

    private static string FormatPrize(string prize)
    {
        var trimmed = prize.Trim();
        if (Regex.IsMatch(trimmed, @"^\d+(\.\d{1,2})?$"))
        {
            var amount = double.Parse(trimmed);
            return $"${amount}";
        }
        return Esc(trimmed);
    }

    private static PlaceEntry? GetPlace(List<PlaceEntry> places, int rank)
        => places.FirstOrDefault(p => p.Place == rank);

    private static string PlaceCell(PlaceEntry? place)
    {
        if (place == null) return "";
        var name = Esc(place.Winner);
        if (place.EntryNumber > 0)
            return $"{name} <span class=\"cca-entry\">(#{place.EntryNumber})</span>";
        return name;
    }

    private static string RenderSpecialPrizes(List<SpecialPrize> prizes)
    {
        var sorted = prizes.OrderBy(p => p.Order).ToList();
        var rows = string.Join("\n", sorted.Select(p =>
        {
            var entry = p.EntryNumber > 0 ? $"<span class=\"cca-entry\">(#{p.EntryNumber})</span>" : "";
            var prizeValue = FormatPrize(p.Prize);
            return "    <tr>\n" +
                   $"      <td>{Esc(p.Name)}</td>\n" +
                   $"      <td>{Esc(p.Winner)}</td>\n" +
                   $"      <td>{entry}</td>\n" +
                   $"      <td>{prizeValue}</td>\n" +
                   "    </tr>";
        }));

        return "<h2>Special Prizes</h2>\n" +
               "<table class=\"cca-special-prizes\">\n" +
               "  <thead>\n" +
               "    <tr>\n" +
               "      <th style=\"text-align:left\">Name</th>\n" +
               "      <th style=\"text-align:left\">Winner</th>\n" +
               "      <th style=\"text-align:left\">Entry</th>\n" +
               "      <th style=\"text-align:left\">Prize</th>\n" +
               "    </tr>\n" +
               "  </thead>\n" +
               "  <tbody>\n" +
               rows + "\n" +
               "  </tbody>\n" +
               "</table>";
    }

    private static string RenderOverallResults(List<OverallCategory> overallResults)
    {
        var rows = string.Join("\n", overallResults.Select(cat =>
        {
            var first  = GetPlace(cat.Places, 1);
            var second = GetPlace(cat.Places, 2);
            var third  = GetPlace(cat.Places, 3);
            return "    <tr>\n" +
                   $"      <td>{Esc(cat.Category)}</td>\n" +
                   $"      <td>{PlaceCell(first)}</td>\n" +
                   $"      <td>{PlaceCell(second)}</td>\n" +
                   $"      <td>{PlaceCell(third)}</td>\n" +
                   "    </tr>";
        }));

        return "<h2>Overall Results</h2>\n" +
               "<table class=\"cca-overall-results\">\n" +
               "  <thead>\n" +
               "    <tr>\n" +
               "      <th style=\"text-align:left\">Category</th>\n" +
               "      <th style=\"text-align:left\">1st Place</th>\n" +
               "      <th style=\"text-align:left\">2nd Place</th>\n" +
               "      <th style=\"text-align:left\">3rd Place</th>\n" +
               "    </tr>\n" +
               "  </thead>\n" +
               "  <tbody>\n" +
               rows + "\n" +
               "  </tbody>\n" +
               "</table>";
    }

    private static string RenderDivisionAwardsTable(List<DivisionCategory> awardCats)
    {
        var rows = string.Join("\n", awardCats.Select(cat =>
        {
            var first = GetPlace(cat.Places, 1);
            return "    <tr>\n" +
                   $"      <td>{Esc(cat.Name)}</td>\n" +
                   $"      <td>{PlaceCell(first)}</td>\n" +
                   "    </tr>";
        }));

        return "<table class=\"cca-division-awards\">\n" +
               "  <thead>\n" +
               "    <tr>\n" +
               "      <th style=\"text-align:left\">Award</th>\n" +
               "      <th style=\"text-align:left\">Winner</th>\n" +
               "    </tr>\n" +
               "  </thead>\n" +
               "  <tbody>\n" +
               rows + "\n" +
               "  </tbody>\n" +
               "</table>";
    }

    private static string RenderDivisionCategoryTable(List<DivisionCategory> categoryCats)
    {
        var rows = string.Join("\n", categoryCats.Select(cat =>
        {
            var style  = StyleLabel(cat.Style);
            var first  = GetPlace(cat.Places, 1);
            var second = GetPlace(cat.Places, 2);
            var third  = GetPlace(cat.Places, 3);
            return "    <tr>\n" +
                   $"      <td>{Esc(cat.Name)}</td>\n" +
                   $"      <td>{style}</td>\n" +
                   $"      <td>{PlaceCell(first)}</td>\n" +
                   $"      <td>{PlaceCell(second)}</td>\n" +
                   $"      <td>{PlaceCell(third)}</td>\n" +
                   "    </tr>";
        }));

        return "<table class=\"cca-category-results\">\n" +
               "  <thead>\n" +
               "    <tr>\n" +
               "      <th style=\"text-align:left\">Category</th>\n" +
               "      <th style=\"text-align:left\">Style</th>\n" +
               "      <th style=\"text-align:left\">1st Place</th>\n" +
               "      <th style=\"text-align:left\">2nd Place</th>\n" +
               "      <th style=\"text-align:left\">3rd Place</th>\n" +
               "    </tr>\n" +
               "  </thead>\n" +
               "  <tbody>\n" +
               rows + "\n" +
               "  </tbody>\n" +
               "</table>";
    }

    private static string RenderDivisionResults(List<DivisionResult> divisionResults)
    {
        var sections = string.Join("\n", divisionResults.Select(div =>
        {
            var awardCats    = div.Categories.Where(c => c.Style == null).ToList();
            var categoryCats = div.Categories.Where(c => c.Style != null).ToList();

            var awardsHtml   = awardCats.Count    > 0
                ? "\n<h4>Division Awards</h4>\n" + RenderDivisionAwardsTable(awardCats)    + "\n"
                : "";
            var categoryHtml = categoryCats.Count > 0
                ? "\n<h4>Category Results</h4>\n" + RenderDivisionCategoryTable(categoryCats) + "\n"
                : "";

            return $"<h3>{Esc(div.Division)} Division</h3>{awardsHtml}{categoryHtml}";
        }));

        return "<h2>Division Results</h2>\n" + sections;
    }

    public string RenderArticle(ShowcaseResultsData data)
    {
        var header  = $"<!-- {Esc(data.Event.Name)} {data.Event.EventId} showcase results -- generated by ShowcaseResults.Cli -->";
        var title   = $"<h2>{data.Event.EventId} {Esc(data.Event.Name)} Results</h2>";
        var special = RenderSpecialPrizes(data.SpecialPrizes);
        var overall = RenderOverallResults(data.OverallResults);
        var divs    = RenderDivisionResults(data.DivisionResults);

        return string.Join("\n\n", new[] { header, title, special, overall, divs }) + "\n";
    }

    private static string PlaceName(int place) => place switch
    {
        1 => "1st Place",
        2 => "2nd Place",
        3 => "3rd Place",
        _ => $"{place}th Place"
    };

    /// <summary>
    /// Render a per-carver article showing their ribbons, overall wins, and special prizes.
    /// Returns null if the carver is not found in competitors (caller should handle error).
    /// Returns HTML with a "no results" message if the carver exists but has no results.
    /// </summary>
    public CarverArticleResult? RenderCarverArticle(ShowcaseResultsData data, int? carverId, string? carverName)
    {
        // Resolve carver
        Competitor? carver = null;
        if (carverId.HasValue)
            carver = data.Competitors.FirstOrDefault(c => c.CarverId == carverId.Value);
        else if (!string.IsNullOrWhiteSpace(carverName))
            carver = data.Competitors.FirstOrDefault(c =>
                string.Equals($"{c.FirstName} {c.LastName}", carverName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (carver == null)
            return null;

        var id = carver.CarverId;
        var fullName = $"{carver.FirstName} {carver.LastName}";

        // Filter division results — keep only places where carver_id matches
        var carverDivisions = new List<(string Division, List<(string Category, string? Style, PlaceEntry Place)> Entries)>();
        foreach (var div in data.DivisionResults)
        {
            var entries = new List<(string Category, string? Style, PlaceEntry Place)>();
            foreach (var cat in div.Categories)
            {
                foreach (var place in cat.Places.Where(p => p.CarverId == id))
                    entries.Add((cat.Name, cat.Style, place));
            }
            if (entries.Count > 0)
                carverDivisions.Add((div.Division, entries));
        }

        // Filter overall results
        var carverOverall = new List<(string Category, PlaceEntry Place)>();
        foreach (var cat in data.OverallResults)
        {
            foreach (var place in cat.Places.Where(p => p.CarverId == id))
                carverOverall.Add((cat.Category, place));
        }

        // Filter special prizes
        var carverPrizes = data.SpecialPrizes
            .Where(p => p.CarverId == id)
            .OrderBy(p => p.Order)
            .ToList();

        var hasResults = carverDivisions.Count > 0 || carverOverall.Count > 0 || carverPrizes.Count > 0;

        var header = $"<!-- Carver article for {Esc(fullName)} -- generated by ShowcaseResults.Cli -->";
        var title = $"<h1>{Esc(fullName)} &mdash; {data.Event.EventId} Results</h1>";

        if (!hasResults)
        {
            var html = $"{header}\n\n{title}\n\n<p>No results found for {Esc(fullName)}.</p>\n";
            return new CarverArticleResult(id, fullName, html, HasResults: false);
        }

        var sections = new List<string> { header, title };

        // Division results
        if (carverDivisions.Count > 0)
        {
            var divHtml = "<h2>Division Results</h2>";
            foreach (var (division, entries) in carverDivisions)
            {
                var rows = string.Join("\n", entries.Select(e =>
                {
                    var styleStr = e.Style != null ? $" ({StyleLabel(e.Style)})" : "";
                    var entryNum = e.Place.EntryNumber > 0 ? e.Place.EntryNumber.ToString() : "";
                    return "  <tr>\n" +
                           $"    <td>{Esc(e.Category)}{styleStr}</td>\n" +
                           $"    <td>{PlaceName(e.Place.Place)}</td>\n" +
                           $"    <td>{entryNum}</td>\n" +
                           "  </tr>";
                }));

                divHtml += $"\n<h3>{Esc(division)} Division</h3>\n" +
                           "<table class=\"cca-carver-division-results\">\n" +
                           "  <thead>\n" +
                           "    <tr><th style=\"text-align:left\">Category</th><th style=\"text-align:left\">Place</th><th style=\"text-align:left\">Entry #</th></tr>\n" +
                           "  </thead>\n" +
                           "  <tbody>\n" +
                           rows + "\n" +
                           "  </tbody>\n" +
                           "</table>";
            }
            sections.Add(divHtml);
        }

        // Overall results
        if (carverOverall.Count > 0)
        {
            var overHtml = "<h2>Overall Results</h2>\n<ul>";
            foreach (var (category, place) in carverOverall)
                overHtml += $"\n  <li>{Esc(category)}: {PlaceName(place.Place)}</li>";
            overHtml += "\n</ul>";
            sections.Add(overHtml);
        }

        // Special prizes
        if (carverPrizes.Count > 0)
        {
            var rows = string.Join("\n", carverPrizes.Select(p =>
            {
                var entry = p.EntryNumber > 0 ? $"#{p.EntryNumber}" : "";
                return $"  <tr>\n    <td>{Esc(p.Name)}</td>\n    <td>{FormatPrize(p.Prize)}</td>\n    <td>{entry}</td>\n  </tr>";
            }));

            var prizeHtml = "<h2>Special Prizes</h2>\n" +
                "<table class=\"cca-carver-prizes\">\n" +
                "  <thead>\n" +
                "  <tr>\n" +
                "    <th style=\"text-align:left\">Prize</th>\n" +
                "    <th style=\"text-align:left\">Value</th>\n" +
                "    <th style=\"text-align:left\">Entry</th>\n" +
                "  </tr>\n" +
                "  </thead>\n" +
                "  <tbody>\n" +
                rows + "\n" +
                "  </tbody>\n" +
                "</table>";
            sections.Add(prizeHtml);
        }

        return new CarverArticleResult(id, fullName, string.Join("\n\n", sections) + "\n", HasResults: true);
    }
}

public record CarverArticleResult(int CarverId, string FullName, string Html, bool HasResults);
