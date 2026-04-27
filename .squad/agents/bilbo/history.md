# Bilbo — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Data Engineer
- **Joined:** 2026-03-23T01:57:14.727Z

## Learnings

<!-- Append learnings below -->

- **2026-04-26 — Issue #34 Feasibility Study (AppSheet API Integration).** Completed detailed feasibility analysis for AppSheet as optional data source. Key findings: (1) Current SpreadsheetParser produces ShowcaseResultsData; an AppSheetParser implementing IDataProvider would be a drop-in replacement with zero downstream changes. (2) AppSheet's row-based, column-oriented REST API aligns directly with XLSX structure. (3) Normalization logic (carver ID parsing, enum validation, checked-in detection) reusable from SpreadsheetParser. (4) Feasibility HIGH; core design areas (data model alignment, abstraction, normalization) LOW risk; moderate risk in multi-place storage format and API rate limits. (5) Seven user-provided unknowns block Phase 2: app ID, table names, column mappings, carver ID format, checked-in column details, multi-place storage, API limits. Implementation strategy: CLI accepts table/column mappings via options (Phase 4) or config file (Phase 3). Three-layer checked-in logic (column flag → integrity fallback → lookup directory) applies to AppSheet same as XLSX. Recommendation: Proceed with Phase 1 design (no user input needed), schedule 48-hour window for seven unknowns. Decision inbox: .squad/decisions/inbox/bilbo-issue-34-research.md (merged to active decisions.md by Scribe 2026-04-26).

- **2026-04-23 — Issue #30 JSON-layer checked-in filtering.** User preference: enforce checked-in filtering in the generated JSON, not only in Joomla rendering. Key parser path: src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs now looks for an explicit checked-in signal in Competitor.xlsx; if none exists, src/ShowcaseResults.Cli/Program.cs falls back to competitors referenced by assigned prizes or ranked judging results before serializing JSON. Downstream contract: schema/results.schema.json, README.md, and docs/joomla-extension.md now describe competitors as checked-in-only data for consumers like joomla/com_showcaseresults/site/src/Service/ResultsService.php. Current source-data constraint: the checked-in sample workbook in data/input/Competitor.xlsx has no explicit checked-in column, so today's generated sample JSON falls back to result-bearing carver IDs (35 competitors in joomla/com_showcaseresults/media/data/results-2026T.json).

> For archived learnings from before 2026-03-27, see history-archive.md
