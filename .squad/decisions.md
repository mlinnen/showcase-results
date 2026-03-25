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

### Issue #7 Decomposition into Sub-Issues (2026-03-25, Gandalf)
Decomposed issue #7 ("Feature: Joomla component for dynamic per-carver results across all events") into 6 independently implementable sub-issues (#8–#13): CLI JSON export (#8), Joomla component scaffolding (#9), data layer (#10), carver view rendering (#11), error handling & edge cases (#12), testing & verification (#13). Key architectural decision: carver_id is per-event only (privacy constraint); name is the only cross-event lookup key. Dependency graph allows #8 (C# CLI) and #9 (PHP boilerplate) to proceed in parallel. Data layer (#10) separated from view (#11) following MVC; error handling (#12) isolated to keep happy-path code clean. #13 depends on all others. Rationale: parallel development across stacks, independent review of concerns, verification not an afterthought.

## Governance
- All meaningful changes require team consensus
- Document architectural decisions here
