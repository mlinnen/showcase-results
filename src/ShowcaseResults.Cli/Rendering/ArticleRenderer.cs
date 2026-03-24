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
        var header  = $"<!-- {Esc(data.Event.Name)} {data.Event.Year} showcase results -- generated by ShowcaseResults.Cli -->";
        var title   = $"<h2>{data.Event.Year} {Esc(data.Event.Name)} Results</h2>";
        var special = RenderSpecialPrizes(data.SpecialPrizes);
        var overall = RenderOverallResults(data.OverallResults);
        var divs    = RenderDivisionResults(data.DivisionResults);

        return string.Join("\n\n", new[] { header, title, special, overall, divs }) + "\n";
    }
}
