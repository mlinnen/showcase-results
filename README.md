# Showcase Results

A .NET 10 CLI application that parses woodcarving competition spreadsheets and generates an HTML article fragment ready to publish in Joomla.

## Overview

The tool reads three Excel spreadsheets — competitors, prizes, and judging results — and produces a single `article.html` file containing the formatted event results. The output is a semantic HTML fragment (no `<html>`, `<head>`, or `<body>` tags) safe to paste directly into the Joomla editor.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Build

```bash
dotnet build src/ShowcaseResults.Cli
```

## Usage

```bash
showcase-results create results [options]
```

All options are optional. If the spreadsheet paths are omitted the tool looks for files in `data/input/`. If `--event-name` or `--year` are omitted the defaults below are used.

### Options

| Option | Default | Description |
|--------|---------|-------------|
| `--event-name` | `Showcase of Woodcarvings` | Name of the event |
| `--year` | Current year | Year of the event |
| `--competitors` | `data/input/Competitor.xlsx` | Path to the competitors spreadsheet |
| `--prizes` | `data/input/Prizes.xlsx` | Path to the prizes spreadsheet |
| `--judging` | `data/input/Judging.xlsx` | Path to the judging results spreadsheet |
| `--output` | `output/article.html` | Path for the generated HTML file |

### Examples

Run with all defaults (spreadsheets in `data/input/`):

```bash
showcase-results create results
```

Override the event name and year:

```bash
showcase-results create results --event-name "Spring Invitational" --year 2025
```

Use alphanumeric year values for test events:

```bash
showcase-results create results --event-name "Test Event" --year "2026T"
```

Provide explicit file paths:

```bash
showcase-results create results \
  --competitors "C:\data\Competitor.xlsx" \
  --prizes "C:\data\Prizes.xlsx" \
  --judging "C:\data\Judging.xlsx" \
  --output "C:\output\article.html"
```

### Console output

```
Parsing spreadsheets for Showcase of Woodcarvings 2026...
  5 special prizes
  3 overall result categories
  3 divisions (Intermediate, Novice, Open)
  47 competitors
✓ Wrote output/article.html
```

## Input Spreadsheets

Each spreadsheet has a merged title in row 1 (skipped), column headers in row 2, and data from row 3 onward.

| File | Required columns |
|------|-----------------|
| `Competitor.xlsx` | `Carver ID`, `First Name`, `Last Name` |
| `Prizes.xlsx` | `Name`, `Carver`, `Order`, `Entry #`, `Prize` |
| `Judging.xlsx` | `Category`, `Division`, `Style`, `1st`, `2nd`, `3rd`, `#`, `#_1`, `#_2` |

## Output

The generated `article.html` contains:

1. **Event header** — event name and year
2. **Special Prizes** section — grouped by category; prize name and winner for each award
3. **Ribbon Winners** section — grouped by category, division, and style; carver name, entry title, and ribbon for each entry

The HTML uses only semantic elements (`<h2>`, `<table>`, etc.) with no inline styles or scripts, making it compatible with Joomla's content editor.

### Publishing to Joomla

1. Open the target article in the Joomla backend editor
2. Switch to **Source / HTML** view (the `<>` or **Source** button in the toolbar)
3. Paste the contents of `article.html` into the editor
4. Switch back to visual mode and review the layout
5. Save the article

> **Note:** `article.html` is an HTML **fragment** — it does not include `<html>`, `<head>`, or `<body>` tags. Joomla's editor expects fragments.

## Project structure

```
src/
  ShowcaseResults.Cli/     # C# CLI application (.NET 10)
data/
  input/                   # Default location for input spreadsheets (git-ignored)
  output/                  # Intermediate data (git-ignored)
output/                    # Generated article.html (git-ignored)
schema/                    # JSON schemas for intermediate data
```
