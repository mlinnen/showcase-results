# Gandalf — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Lead
- **Joined:** 2026-03-23T01:57:14.726Z

## Learnings

<!-- Append learnings below -->

- **2026-04-08 — year → event parameter rename (full stack).** Renamed the `year`/`--year` parameter to `event`/`--event` across all components. Motivation: ADR-007 established alphanumeric event IDs (e.g., `2026T`), making "year" a misleading name. Changes: C# `EventInfo.Year` → `EventId`, CLI `--year` → `--event`, `ArticleRenderer` 3 interpolations, JSON schema `event.year` → `event.event`, 8 Joomla PHP/XML/INI files (URL params, vars, array keys, XML fields), 3 doc files + README, 29 URL param occurrences in test plans. Build: 0 errors. 19 files changed. Lesson: parameter naming should reflect semantics (event identity) not format assumption (calendar year).

- **2026-03-26 — Solution file moved to src\ (sln-move).** Moved `showcase-results.sln` from repo root to `src\showcase-results.sln`. Updated the single project reference path inside the .sln from `src\ShowcaseResults.Cli\ShowcaseResults.Cli.csproj` → `ShowcaseResults.Cli\ShowcaseResults.Cli.csproj`. No other files (README, workflows, scripts) referenced the .sln path. Build verified clean: `dotnet build src\showcase-results.sln` — 0 errors. Lesson: always grep all file types before assuming a path is only referenced in one place.

- **2026-03-24 — Release pipeline created (ADR-003).** Built `.github/workflows/release.yml`: matrix strategy builds `win-x64`, `win-x86`, `win-arm64` in parallel on `windows-latest` using `actions/setup-dotnet@v4` with .NET 10. Each matrix job publishes a self-contained single-file exe, renames it to `showcase-results-{runtime}.exe`, and uploads as an artifact. A final `release` job downloads all three artifacts and creates a GitHub Release via `softprops/action-gh-release@v2` with a date+run-number tag (`vYYYY.MM.DD-{run_number}`). Triggers on push to `main`; workflow-level `contents: write` permission enables release creation.

- **2026-03-23 — Schema rewrite approved (ADR-002).** The bootstrap schema assumed ribbons and flat categories; actual data uses 1st/2nd/3rd place across divisions with named prizes. Rewrote `schema/results.schema.json` to match Bilbo's real data model: `special_prizes`, `overall_results`, `division_results`, `competitors` with `carver_id`. No code depended on the old schema yet, so zero migration cost. Lesson: always validate schema assumptions against actual data before anyone builds against them.

- **2026-03-25 — Per-carver article generator (issue #1).** Added `create carver-article` sub-command to `Program.cs` with `--carver-id` / `--carver-name` / `--output` options. Added `RenderCarverArticle()` to `ArticleRenderer.cs` that filters division_results, overall_results, and special_prizes by carver_id, then renders a focused HTML fragment. Reuses existing SpreadsheetParser (ADR-001 pipeline intact). Created 4 sub-issues (#2–#5) for CLI scaffolding, filtering logic, rendering, and tests. Key pattern: the renderer returns a `CarverArticleResult` record with both the HTML and metadata (CarverId, FullName, HasResults) so the CLI handler can decide exit behavior without parsing HTML.

- **2026-03-25 — Issue #7 decomposition (Joomla dynamic component).** Decomposed issue #7 into 6 sub-issues (#8–#13): CLI JSON export (#8), Joomla scaffolding (#9), data layer (#10), view rendering (#11), error handling (#12), and test plan (#13). Key architectural decision: `carver_id` is per-event only (privacy), so `?name=` is the only cross-event lookup key. Dependency graph allows #8 (C#) and #9 (PHP scaffolding) to proceed in parallel. Separated data layer from view following MVC; isolated error handling to keep happy-path clean. Decision written to inbox.

- **2026-03-26 — Year as String type (ADR-007, Issue #24).** Test events require alphanumeric year suffixes (e.g., `2026T`), so year must be a string everywhere: JSON schema, C# EventInfo, Joomla parameters. Security enforced via three-layer validation: HtmlView input check with `^[a-zA-Z0-9]+$` regex, JSON schema pattern constraint, and safe file path construction (`results-{year}.json`). Prevents path traversal and null-byte injection. Backward compatible: old numeric-only years still work (`2026`), new files output string format. Migration: update C# type `int Year` → `string Year`, add schema pattern, update Joomla is_numeric() to regex, update filename extraction regex from `\d{4}` to `[a-zA-Z0-9]+`. Decision in inbox/gandalf-year-as-string.md.

## Session: Issue #24 — Year as String (2026-04-01)

**Role:** Lead - Architecture Decision  
**Task:** ADR-007 establishing alphanumeric year support  
**Status:** ✅ COMPLETED - APPROVED FOR PRODUCTION

Led architectural decision enabling year values to contain text (e.g., 2026T for test events). Coordinated full stack implementation:
- JSON schema: year type string with pattern constraint
- C# model: EventInfo.Year int → string
- Joomla: all year parameters use getString()
- Data files: updated to string format

Decision documented in decisions.md ADR-007 entry.
