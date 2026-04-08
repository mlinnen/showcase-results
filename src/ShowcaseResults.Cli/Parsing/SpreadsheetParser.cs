using ClosedXML.Excel;
using ShowcaseResults.Models;

namespace ShowcaseResults.Parsing;

public class SpreadsheetParser
{
    private readonly string _competitorsPath;
    private readonly string _prizesPath;
    private readonly string _judgingPath;

    public SpreadsheetParser(string competitorsPath, string prizesPath, string judgingPath)
    {
        _competitorsPath = competitorsPath;
        _prizesPath = prizesPath;
        _judgingPath = judgingPath;
    }

    /// <summary>
    /// Read an xlsx file, skipping row 1 (merged title). Row 2 = headers, rows 3+ = data.
    /// Duplicate column names are renamed with _1, _2 suffixes (matching xlsx JS library behavior).
    /// </summary>
    private static List<Dictionary<string, string?>> ReadSheet(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheets.First();

        var lastUsedCol = ws.LastCellUsed()?.Address.ColumnNumber ?? 0;

        // Build header list from row 2, handling duplicate names
        var headers = new List<string>();
        var headerCounts = new Dictionary<string, int>();

        for (int col = 1; col <= lastUsedCol; col++)
        {
            var cell = ws.Cell(2, col);
            var name = cell.IsEmpty() ? "" : GetCellString(cell)?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                headers.Add($"__col{col}");
                continue;
            }

            if (headerCounts.ContainsKey(name))
            {
                headerCounts[name]++;
                headers.Add($"{name}_{headerCounts[name]}");
            }
            else
            {
                headerCounts[name] = 0;
                headers.Add(name);
            }
        }

        var result = new List<Dictionary<string, string?>>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 3; row <= lastRow; row++)
        {
            var dict = new Dictionary<string, string?>();
            for (int col = 0; col < headers.Count; col++)
            {
                var cell = ws.Cell(row, col + 1);
                dict[headers[col]] = cell.IsEmpty() ? null : GetCellString(cell);
            }
            result.Add(dict);
        }

        return result;
    }

    /// <summary>
    /// Convert a cell value to a string: integers as "4" (not "4.0"), text trimmed.
    /// </summary>
    private static string? GetCellString(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        var v = cell.Value;
        if (v.IsNumber)
        {
            double d = v.GetNumber();
            var s = d == Math.Floor(d) ? ((long)d).ToString() : d.ToString("G");
            return s;
        }
        var text = v.ToString().Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <summary>
    /// Parse "ID FirstName LastName" → (CarverId, Winner) or null.
    /// </summary>
    private static (int CarverId, string Winner)? ParseCarver(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        var spaceIdx = s.IndexOf(' ');
        if (spaceIdx == -1) return null;
        if (!int.TryParse(s[..spaceIdx], out int carverId)) return null;
        var winner = s[(spaceIdx + 1)..].Trim();
        if (string.IsNullOrEmpty(winner)) return null;
        return (carverId, winner);
    }

    private static string? NormalizeStyle(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var s = val.Trim();
        return s is "N" or "P" ? s : null;
    }

    private static string? NormalizeDivision(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var s = val.Trim();
        return s is "Intermediate" or "Novice" or "Open" ? s : null;
    }

    public List<Competitor> ParseCompetitors()
    {
        var rows = ReadSheet(_competitorsPath);
        return rows
            .Where(r => r.GetValueOrDefault("Carver ID") != null)
            .Select(r => new Competitor(
                int.Parse(r["Carver ID"]!),
                r.GetValueOrDefault("First Name")?.Trim() ?? "",
                r.GetValueOrDefault("Last Name")?.Trim() ?? "",
                NormalizeDivision(r.GetValueOrDefault("Division"))))
            .ToList();
    }

    public List<SpecialPrize> ParseSpecialPrizes()
    {
        var rows = ReadSheet(_prizesPath);
        return rows
            .Where(r =>
                r.GetValueOrDefault("Name") != null &&
                r.GetValueOrDefault("Order") != null &&
                string.Equals(r.GetValueOrDefault("Assigned"), "TRUE", StringComparison.OrdinalIgnoreCase))
            .Select(r =>
            {
                if (!int.TryParse(r.GetValueOrDefault("Carver #"), out int carverId)) return null;
                var winner = r.GetValueOrDefault("Carver Name")?.Trim();
                if (string.IsNullOrEmpty(winner)) return null;
                int.TryParse(r.GetValueOrDefault("Entry #"), out int entryNum);
                return new SpecialPrize(
                    int.Parse(r["Order"]!),
                    r["Name"]!.Trim(),
                    carverId,
                    winner,
                    entryNum,
                    r.GetValueOrDefault("Prize")?.Trim() ?? "");
            })
            .Where(p => p != null)
            .Select(p => p!)
            .OrderBy(p => p.Order)
            .ToList();
    }

    public (List<OverallCategory> OverallResults, List<DivisionResult> DivisionResults) ParseJudging()
    {
        var rows = ReadSheet(_judgingPath);

        var overallResults = new List<OverallCategory>();
        var divisionMap = new Dictionary<string, List<DivisionCategory>>();

        foreach (var row in rows)
        {
            var places = new List<PlaceEntry>();

            var c1 = ParseCarver(row.GetValueOrDefault("1st"));
            if (c1 != null)
            {
                if (int.TryParse(row.GetValueOrDefault("#"), out int en1) && en1 >= 1)
                    places.Add(new PlaceEntry(1, c1.Value.CarverId, c1.Value.Winner, en1));
                else
                    Console.Error.WriteLine($"  WARN: skipping 1st place for \"{row.GetValueOrDefault("Category")?.Trim()}\" ({row.GetValueOrDefault("Division")}) — entry# is {row.GetValueOrDefault("#")}");
            }

            var c2 = ParseCarver(row.GetValueOrDefault("2nd"));
            if (c2 != null)
            {
                if (int.TryParse(row.GetValueOrDefault("#_1"), out int en2) && en2 >= 1)
                    places.Add(new PlaceEntry(2, c2.Value.CarverId, c2.Value.Winner, en2));
                else
                    Console.Error.WriteLine($"  WARN: skipping 2nd place for \"{row.GetValueOrDefault("Category")?.Trim()}\" ({row.GetValueOrDefault("Division")}) — entry# is {row.GetValueOrDefault("#_1")}");
            }

            var c3 = ParseCarver(row.GetValueOrDefault("3rd"));
            if (c3 != null)
            {
                if (int.TryParse(row.GetValueOrDefault("#_2"), out int en3) && en3 >= 1)
                    places.Add(new PlaceEntry(3, c3.Value.CarverId, c3.Value.Winner, en3));
                else
                    Console.Error.WriteLine($"  WARN: skipping 3rd place for \"{row.GetValueOrDefault("Category")?.Trim()}\" ({row.GetValueOrDefault("Division")}) — entry# is {row.GetValueOrDefault("#_2")}");
            }

            if (places.Count == 0) continue;

            var category = row.GetValueOrDefault("Category")?.Trim() ?? "";
            var division = NormalizeDivision(row.GetValueOrDefault("Division"));
            var style = NormalizeStyle(row.GetValueOrDefault("Style"));

            if (division == null)
            {
                overallResults.Add(new OverallCategory(category, places));
            }
            else
            {
                if (!divisionMap.ContainsKey(division))
                    divisionMap[division] = new List<DivisionCategory>();
                divisionMap[division].Add(new DivisionCategory(category, style, places));
            }
        }

        var divisionOrder = new[] { "Intermediate", "Novice", "Open" };
        var divisionResults = divisionOrder
            .Where(d => divisionMap.ContainsKey(d))
            .Select(d => new DivisionResult(d, divisionMap[d]))
            .ToList();

        return (overallResults, divisionResults);
    }
}
