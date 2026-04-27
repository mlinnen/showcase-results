# Gandalf — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Lead
- **Joined:** 2026-03-23T01:57:14.726Z

## Learnings

<!-- Append learnings below -->

- **2026-04-26 — GitHub issue labels (squad assignability).** Configured `.github/workflows/sync-squad-labels.yml` to sync squad labels in the GitHub repository based on the team roster. Key constraint: synced `squad:{member}` labels must match only active assignable roster entries from `.squad/team.md`, excluding non-assignable support roles (Scribe, Ralph) so users have only working label targets for issue assignment. Also syncs `squad` umbrella label and conditional `squad:copilot` label when an active Copilot/Coding Agent entry exists. Rationale: team roster includes role diversity; label sync should target only roles that can own/drive issues. Updated decision in `.squad/decisions/inbox/gandalf-issue-labels.md`.

- **2026-04-24 — Issue #30 integrity fix for checked-in carvers.**Revised the JSON/Joomla contract so `competitors` is now a lookup directory, not a pure public roster. C# changes: `src/ShowcaseResults.Cli/Models/Results.cs` adds `Competitor.CheckedIn`; `src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs` now parses the `Checked In` column onto every competitor row; `src/ShowcaseResults.Cli/Program.cs` keeps all `checked_in=true` rows plus any result-bearing competitors and warns when a real winner is marked unchecked. Joomla change: `joomla/com_showcaseresults/site/src/Service/ResultsService.php` filters the Carvers List on `checked_in` but still uses the full competitor directory for name/ID lookup integrity. Supporting files updated: `schema/results.schema.json`, `README.md`, `docs/test-plan-carvers-list.md`, `docs/test-data-spec.md`, `docs/joomla-extension.md`, and sample JSON `joomla/com_showcaseresults/media/data/results-2026T.json`. Validation: `dotnet test src\\showcase-results.sln` passed; mismatch simulation confirmed a result-bearing unchecked competitor is retained with `checked_in=false`.

- **2026-04-24 — Issue #34 Implementation Plan Posted.** Posted comprehensive 5-phase implementation plan to issue #34 (GitHub comment https://github.com/mlinnen/showcase-results/issues/34#issuecomment-4305517069). Plan covers: (1) Current pipeline abstraction (IDataProvider interface), (2) Recommended approach with two implementations (SpreadsheetParser, AppSheetParser), (3) Proposed sequence (Phase 1-5: abstraction, HTTP layer, parser, CLI wiring, testing/docs), (4) Dependency graph, (5) Seven risk items + unknowns (AppSheet table/column names, carver_id format, check-in column, API limits, key storage), (6) Async strategy (.GetAwaiter().GetResult() in CLI handler for sync-first pattern), (7) Suggested team owners by phase, (8) Next actions. Key files analyzed: SpreadsheetParser.cs (one class, four methods), Models/Results.cs (data contracts), Program.cs (CLI handler patterns). Pattern: Interface-based abstraction maintains zero-change contract to rendering and CLI layers; HTTP client uses stock .NET HttpClient (no new dependencies); AppSheet data mapping deferred to Phase 2 (user decision required).

- **2026-04-23 — Issue #34 Architecture Plan (AppSheet as optional data source).** The CLI's data ingestion is entirely in `SpreadsheetParser.cs` (one class, 3 methods). The right abstraction is an `IDataProvider` interface with `ParseCompetitors()`, `ParseCompetitorsForJson()`, `ParseSpecialPrizes()`, and `ParseJudging()`. A new `AppSheetClient` (HttpClient-based, no new packages needed in .NET 10) and `AppSheetParser : IDataProvider` would provide the AppSheet branch. New CLI options: `--source spreadsheet|appsheet`, `--appsheet-app-id`, `--appsheet-api-key` (with `APPSHEET_API_KEY` env var fallback), table name overrides. Five-phase rollout (34-A through 34-F): Phase 1 abstraction (low risk), Phase 2 HTTP client, Phase 3 parser, Phase 4 CLI wiring, Phase 5 tests/docs. Biggest unknown: actual AppSheet table/column names — require user input before 34-C. Async vs sync: CLI context makes `.GetAwaiter().GetResult()` acceptable. Plan documented in decisions.md. Key files: `src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs`, `Program.cs`.

- **2026-04-08 — --data-root whitespace validation (PR #25 fixes).** Applied two critical fixes to PR #25 after Aragorn's review: (1) Added `dataRoot = string.IsNullOrWhiteSpace(dataRoot) ? null : dataRoot;` normalization to both command handlers (results and carver-article) immediately after reading the option value — prevents `--data-root ""` or `--data-root "   "` from producing malformed paths by ensuring whitespace values fall back to null/relative defaults. (2) Updated carverOutputOption help text from `"default: output/carver-{id}.html"` to `"default: {data-root}/output/carver-{id}.html or output/carver-{id}.html"` for consistency with other options. Build verified clean. Lesson: user-input strings need defensive normalization before path construction, and help text must document fallback behavior clearly.

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

## Session: PR #25 — --data-root Parameter (Bug Fixes) (2026-04-09)

**Role:** Lead (Defect Resolution)  
**Task:** Apply Aragorn's review fixes to PR #25

**Fixes applied:**
1. **Empty/Whitespace Normalization (Lines 61, 155):** Added `dataRoot = string.IsNullOrWhiteSpace(dataRoot) ? null : dataRoot;` in both handlers (results, carver-article) to normalize empty/whitespace values to null, ensuring fallback to original defaults
2. **Help Text Update (Line 139):** Updated carverOutputOption help text from `"default: output/carver-{id}.html"` to `"default: {data-root}/output/carver-{id}.html or output/carver-{id}.html"` for consistency

**Validation:**
- Build: `dotnet build src\showcase-results.sln` — 0 errors
- Changes: 2 code locations (normalization + help text)
- Pushed: ✅ Ready for merge

**Lesson:** User input strings need defensive normalization before path construction; help text must document fallback chain clearly
