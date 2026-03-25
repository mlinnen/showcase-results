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

## Governance
- All meaningful changes require team consensus
- Document architectural decisions here
