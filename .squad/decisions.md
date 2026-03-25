# Squad Decisions

## Active Decisions

### ADR-001 — Three-stage pipeline (2026-03-23, Gandalf)
Spreadsheets → results.json (Bilbo) → article.html (Frodo). JSON Schema in schema/results.schema.json is the hard contract between stages.

### ADR-002 — Schema Approval: results.schema.json (2026-03-23, Gandalf)
Full rewrite approved. Schema now models special prizes (33), overall results (2), division results (3), competitors (37), and 1st/2nd/3rd place rankings.

### Real Data Model (2026-03-23, Bilbo)
Four sheets: Categories (51), Competitor (37), Judging (266), Prizes (33). Carver ID embedded as 'ID FirstName LastName'.

### Data Quality: Carver 16 Entry Number (2026-03-23, Bilbo)
Novice '21 Busts' N: carver 16 has entry_number = 0; skipped with WARN. Source data correction needed.

### ADR-004 — Per-Carver Article Generator (2026-03-25, Gandalf)
New `create carver-article` command supports `--carver-id N` and `--carver-name "First Last"` lookup. RenderCarverArticle() returns CarverArticleResult record (CarverId, FullName, Html, HasResults) for clean separation of concerns. Filtering happens at render time. Reuses SpreadsheetParser. Output defaults to `output/carver-{id}.html`. Carver not found = non-zero exit; no results = exit 0 with warning. Future: batch generation can iterate competitors.

### Article Rendering (2026-03-23, Frodo)
Section order: Special Prizes → Overall Results → Division Results. Special prizes sorted by order field, 3-col table. Overall results: Category | 1st | 2nd | 3rd (empty cells for missing places). Division layout: h3 per division, then two h4-headed sub-tables per division: Division Awards (2-col: Award | Winner, 1st place only) and Category Results (5-col: Category | Style | 1st | 2nd | 3rd). Place format: "Name <span class='cca-entry'>(#entry)</span>". Entry 0/null handled as empty td. CSS classes: cca-special-prizes, cca-overall-results, cca-division-awards, cca-category-results, cca-entry. No Joomla wrappers, no inline styles, no scripts.

### Entry Number Format Fix (2026-03-25, Frodo)
Special prizes entry numbers: `<span class="cca-entry">(#N)</span>` wrapped in parentheses. When entry_number is 0/null/undefined, render empty `<td></td>`.

### Release Pipeline (2026-03-24, Gandalf)
`.github/workflows/release.yml`: Trigger on push to main. Matrix build on windows-latest for win-x64, win-x86, win-arm64. Build: `dotnet publish -r {runtime} --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false`. Artifacts renamed to `showcase-results-{runtime}.exe`. Release job downloads all three and publishes via softprops/action-gh-release@v2. Tag format: `vYYYY.MM.DD-{run_number}`. Rationale: windows-latest avoids cross-compile complexity; matrix maximizes parallelism; softprops is community standard; PublishTrimmed=false avoids ClosedXML reflection issues.

### Article Validation APPROVED (2026-03-25, Aragorn)
article.html passed all 6 validation checks: Fix #2 confirmed, no regression on category results, event title correct, null/zero entry handling clean, Joomla compliance verified, 33 special prizes count matches. APPROVED for production.

### Carver Article — Division Results as Table (2026-03-25, Frodo)
Changed the Division Results section in the carver article from a <ul>/<li> list to a <table> per division, with three columns: Category, Place, Entry #. Table structure: `<h3>{Division} Division</h3><table class="cca-carver-division-results"><thead><tr><th>Category</th><th>Place</th><th>Entry #</th></tr></thead><tbody>...`. Rationale: list format obscured entry numbers; table aligns with main article's tabular rendering style; CSS class follows naming convention; Entry # renders empty when EntryNumber is 0 — consistent with other entry handling.

### Joomla Data Layer — ResultsService (2026-03-25, Frodo)
Implemented PHP ResultsService class as the data layer for the Joomla component. Service loads and aggregates results-*.json files from media/com_showcaseresults/data/. Three lookup modes: (1) Name cross-event (scans all results-*.json, searches competitors, collects results grouped by event, sorted year descending); (2) Name single-event (?name=John+Doe&year=2024); (3) ID single-event (?carver_id=16&year=2024). Critical constraint: carver_id is per-event only (privacy feature) — CANNOT correlate across events; only name works cross-event. Enforced: ?carver_id=X without ?year=Y returns error='carver_id_requires_year'. Return structure: array with carver_name, found flag, results array (event_name, event_year, special_prizes, overall_results, division_results) sorted by year descending. Edge cases: name/carver_id not found (found=false), no JSON files (error='no_data'), malformed JSON (skip with warning). Data path: JPATH_ROOT/media/com_showcaseresults/data. 504 lines, 9 methods in Mlinnen\Component\ShowcaseResults\Site\Service. Rationale: Separation of concerns; reusable service; privacy-first enforcement; robust error handling. HtmlView instantiates service, sets page title. Template rendering (var_dump for now) comes in #11.

### Joomla Template Rendering — Issue #11 (2026-03-25, Frodo)
Full HTML template rendering implemented in default.php (157 lines added, 13 removed). ordinal() helper function converts place numbers to ordinal text (1st, 2nd, 3rd, 4th..., handles 11th/12th/13th edge cases). Page header (cca-carver-header): h1 with carver name + subtitle (single-event: "Results for {Event Name} {Year}", cross-event: "Results across all events"). Per-event sections (cca-event-section): one per event, h2 with event name+year, sorted year descending via ResultsService. Three table types: (1) Special Prizes (cca-special-prizes): 3 columns (Award, Prize, Entry #); (2) Overall Results (cca-overall-results): 3 columns (Category, Place, Entry #) with ordinal() place; (3) Division Results (cca-division-results): one table per division with h4 division name, 4 columns (Category, Style, Place, Entry #) with ordinal() place. Empty section handling: sections with no data completely skipped (no empty tables). Entry numbers: 0 or null render as empty `<td></td>`. Style values: N→"Natural", P→"Painted", null→empty cell. Semantic HTML: section, table, thead, tbody, h1–h4, no inline styles, no CSS framework, all cca-* prefix classes, no Joomla article wrappers. Rationale: Follows decisions.md conventions. Next: #12 (error handling), #13 (testing).

### Joomla Error Handling — Issue #12 (2026-03-25, Frodo)
Comprehensive error handling and edge case validation for the Joomla carver results component. Parameter validation in HtmlView (122 lines): no params → usage instructions; non-numeric carver_id/year → friendly errors; carver_id without year → required error explaining privacy constraint; name + carver_id → name takes precedence. Not-found states from ResultsService: name not found cross-event ("No results found for '{name}'. Please check the spelling..."), name not found in year ("No results found for '{name}' in {year}. They may have competed in a different year"), carver_id not found in year, registered competitor but zero results. Data file errors: no results-*.json files available, specified year missing (shows available years via getAvailableYears()), malformed JSON (silently skipped cross-event, error message on single-event). Security measures: HTML escaping via esc() helper using htmlspecialchars($str, ENT_QUOTES | ENT_HTML5, 'UTF-8'); year validated numeric BEFORE file path construction (prevents path traversal); layered validation (HtmlView syntax, ResultsService semantics). Error display: `<div class="cca-usage">` for no-params instructional with examples, `<div class="cca-error">` for all other errors. Helper method getAvailableYears() extracts years from filenames using regex `/results-(\d{4})\.json$/`. Rationale: User-friendly messages guide toward successful searches; security prevents XSS and path traversal; graceful degradation for malformed data; layered validation enforces constraints early.

### Issue #7 Decomposition into Sub-Issues (2026-03-25, Gandalf)
Decomposed issue #7 ("Feature: Joomla component for dynamic per-carver results across all events") into 6 independently implementable sub-issues (#8–#13): CLI JSON export (#8), Joomla component scaffolding (#9), data layer (#10), carver view rendering (#11), error handling & edge cases (#12), testing & verification (#13). Key architectural decision: carver_id is per-event only (privacy constraint); name is the only cross-event lookup key. Dependency graph allows #8 (C# CLI) and #9 (PHP boilerplate) to proceed in parallel. Data layer (#10) separated from view (#11) following MVC; error handling (#12) isolated to keep happy-path code clean. #13 depends on all others. Rationale: parallel development across stacks, independent review of concerns, verification not an afterthought.

### JSON Export Output Path Convention (2026-03-25, Bilbo)
`--format json` writes `results-{year}.json` to the same directory as HTML output (derived from `Path.GetDirectoryName(--output)`). Default: `output/results-{year}.json`. Year comes from `data.Event.Year` (parsed data), not `--year` CLI flag. Rationale: co-located outputs, filename always matches actual data. Multiple years coexist: `results-2024.json`, `results-2025.json`, etc. Joomla component (#10) expects this path structure.

### Joomla Component Structure (2026-03-25, Frodo)
Issue #9 — com_showcaseresults scaffold created using modern Joomla 4.x/5.x structure. Namespace: `Mlinnen\Component\ShowcaseResults`. Service providers for site/admin with DI container registration. Query parameters: `name` (cross-event), `carver_id` (per-event, requires year), `year` (event year). Data path: media/com_showcaseresults/data (configurable, receives results-{year}.json from CLI). PHP 8.1+, no legacy compatibility. 12 files delivered: controllers, views, service providers, language files, menu config, build script, installable .zip. MVC separation enables issues #10 (data layer), #11 (view rendering), #12 (error handling) to proceed independently. PR #15: squad/9-joomla-scaffold → dev.

## Governance
- All meaningful changes require team consensus
- Document architectural decisions here
