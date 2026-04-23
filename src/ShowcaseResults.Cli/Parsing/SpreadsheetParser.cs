using ClosedXML.Excel;
using ShowcaseResults.Models;
using System.Text.RegularExpressions;

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

    public List<Competitor> ParseCompetitorsForJson(out bool usedSourceCheckInSignal)
    {
        var rows = ReadSheet(_competitorsPath);
        var competitors = rows
            .Where(r => r.GetValueOrDefault("Carver ID") != null)
            .Select(r => new Competitor(
                int.Parse(r["Carver ID"]!),
                r.GetValueOrDefault("First Name")?.Trim() ?? "",
                r.GetValueOrDefault("Last Name")?.Trim() ?? "",
                NormalizeDivision(r.GetValueOrDefault("Division"))))
            .ToList();

        var checkInColumn = FindCheckedInColumn(rows);

        if (checkInColumn == null)
        {
            usedSourceCheckInSignal = false;
            return competitors;
        }

        usedSourceCheckInSignal = true;

        return rows
            .Where(r =>
                r.GetValueOrDefault("Carver ID") != null &&
                TryParseCheckInValue(r.GetValueOrDefault(checkInColumn), out var isCheckedIn) &&
                isCheckedIn)
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
            var category = row.GetValueOrDefault("Category")?.Trim() ?? "";
            var divisionRaw = row.GetValueOrDefault("Division");

            if (int.TryParse(row.GetValueOrDefault("1st Carver Id"), out int c1Id))
            {
                var c1Name = row.GetValueOrDefault("1st Carver Name")?.Trim();
                if (!string.IsNullOrEmpty(c1Name))
                {
                    if (int.TryParse(row.GetValueOrDefault("#"), out int en1) && en1 >= 1)
                        places.Add(new PlaceEntry(1, c1Id, c1Name, en1, row.GetValueOrDefault("Prize")?.Trim()));
                    else
                        Console.Error.WriteLine($"  WARN: skipping 1st place for \"{category}\" ({divisionRaw}) — entry# is {row.GetValueOrDefault("#")}");
                }
            }

            if (int.TryParse(row.GetValueOrDefault("2nd Carver Id"), out int c2Id))
            {
                var c2Name = row.GetValueOrDefault("2nd Carver Name")?.Trim();
                if (!string.IsNullOrEmpty(c2Name))
                {
                    if (int.TryParse(row.GetValueOrDefault("#_1"), out int en2) && en2 >= 1)
                        places.Add(new PlaceEntry(2, c2Id, c2Name, en2, row.GetValueOrDefault("Prize_1")?.Trim()));
                    else
                        Console.Error.WriteLine($"  WARN: skipping 2nd place for \"{category}\" ({divisionRaw}) — entry# is {row.GetValueOrDefault("#_1")}");
                }
            }

            if (int.TryParse(row.GetValueOrDefault("3rd Carver Id"), out int c3Id))
            {
                var c3Name = row.GetValueOrDefault("3rd Carver Name")?.Trim();
                if (!string.IsNullOrEmpty(c3Name))
                {
                    if (int.TryParse(row.GetValueOrDefault("#_2"), out int en3) && en3 >= 1)
                        places.Add(new PlaceEntry(3, c3Id, c3Name, en3, row.GetValueOrDefault("Prize_2")?.Trim()));
                    else
                        Console.Error.WriteLine($"  WARN: skipping 3rd place for \"{category}\" ({divisionRaw}) — entry# is {row.GetValueOrDefault("#_2")}");
                }
            }

            if (places.Count == 0) continue;

            var division = NormalizeDivision(divisionRaw);
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

    private static string? FindCheckedInColumn(List<Dictionary<string, string?>> rows)
    {
        if (rows.Count == 0)
        {
            return null;
        }

        string? bestColumn = null;
        int bestScore = 0;

        foreach (var header in rows[0].Keys)
        {
            if (!IsPotentialCheckInHeader(header))
            {
                continue;
            }

            int parsedValues = 0;

            foreach (var row in rows)
            {
                if (TryParseCheckInValue(row.GetValueOrDefault(header), out _))
                {
                    parsedValues++;
                }
            }

            if (parsedValues > bestScore)
            {
                bestColumn = header;
                bestScore = parsedValues;
            }
        }

        return bestScore > 0 ? bestColumn : null;
    }

    private static bool IsPotentialCheckInHeader(string header)
    {
        var normalizedHeader = Regex.Replace(header, @"[^A-Za-z0-9]+", " ")
            .Trim()
            .ToLowerInvariant();

        return normalizedHeader is "checked in"
            or "check in"
            or "checkedin"
            or "checkin"
            or "is checked in"
            or "checked in status"
            or "check in status"
            or "present"
            or "attendance";
    }

    private static bool TryParseCheckInValue(string? value, out bool isCheckedIn)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();

        switch (normalizedValue)
        {
            case "true":
            case "yes":
            case "y":
            case "1":
            case "x":
            case "checked in":
            case "checked-in":
            case "present":
                isCheckedIn = true;
                return true;
            case "false":
            case "no":
            case "n":
            case "0":
            case "checked out":
            case "not checked in":
            case "not checked-in":
            case "absent":
            case "":
            case null:
                isCheckedIn = false;
                return normalizedValue != null;
            default:
                isCheckedIn = false;
                return false;
        }
    }
}
