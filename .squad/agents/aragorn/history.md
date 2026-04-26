# Aragorn — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Tester
- **Joined:** 2026-03-23T01:57:14.727Z

## Core Context (Summarized)

**Initial validation phase (2026-03-23):** Validated article.html rendering and identified 2 failures (event title branding, special prizes entry format). Conducted full pipeline validation (results.json schema, article.html HTML compliance, cross-checks). Found data quality issue (duplicate carver names — Franklin Beck IDs 14/21, Lucille Reid IDs 15/23). Revalidated after Frodo's fixes — all 6 checks passed, article approved for production.

**Issue #7 comprehensive audit (2026-03-25):** Performed full code review of Joomla component (ResultsService.php, HtmlView.php, template). Security audit PASSED: XSS protection via esc() helper, path traversal prevented, array access safe. Found 2 low-priority issues: year validation gap (negative years), subtitle escaping consistency. Created 28-case manual test plan and test data spec (results-2024.json, results-2023.json) for reproducible testing. APPROVED for production.

**Issue #22 (2026-03-26):** Created 19-case test plan for carvers list view (10 primary + 9 edge cases) covering year filtering, sorting, XSS escaping, cross-year duplication.

**Issue #24 (2026-04-01):** Validated year-as-string changes across full stack — found 4 critical failures (template getInt(), 3 JSON files with integer years), coordinated fixes with Frodo/Bilbo, re-validated all resolved. APPROVED for production.

**Issue #25 (2026-04-09):** Code review of --data-root parameter implementation. Found 2 bugs: empty/whitespace validation and help text inconsistency. REJECTED, returned to developer for fixes.

**Issue #30 (2026-04-23):** Validated PR #33 after data-layer change; 35 checked-in competitors confirmed from 37 registrations. APPROVED for merge.

## Learnings

### PR #33 Revised Review — JSON-Layer Checked-In Filtering (2026-04-23)

### PR #33 Revised Review — JSON-Layer Checked-In Filtering (2026-04-23)

**Reviewed:** Revised PR #33 after user feedback moved the filter from Joomla rendering into CLI JSON generation.

**Validation results:**
- `Competitor.xlsx` currently has no explicit checked-in column; row-2 headers stop at `Notes` and then blanks, so the CLI correctly used fallback detection.
- Regenerating `results-2026T.json` from `data/input/*.xlsx` produced 35 competitors from 37 source registrations, excluding only carver IDs **22** and **34**.
- The generated competitor IDs matched the union of assigned special-prize IDs plus ranked overall/division result IDs exactly; no result-bearing carver was dropped.
- Re-running the CLI with event name `Showcase of Woodcarvings Test` produced JSON identical to `joomla/com_showcaseresults/media/data/results-2026T.json`.

**Key paths:**
- Source logic: `src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs`, `src/ShowcaseResults.Cli/Program.cs`
- Consumer trust point: `joomla/com_showcaseresults/site/src/Service/ResultsService.php`
- Golden sample: `joomla/com_showcaseresults/media/data/results-2026T.json`

### Session: Issue #30 — Checked-In Carvers Filtering (2026-04-23)

**Role:** Tester  
**Task:** Review PR #33 for correctness and edge cases

**Review scope:**
- Verified checked-in logic: carver_id appears in special_prizes or division_results
- Tested edge cases: empty results, duplicate IDs, multi-year carvers
- Regression testing: confirmed no impact on existing carvers list
- Documentation alignment: test plans match implementation

**Findings:**
- ✅ No critical issues
- ✅ Filtering logic correct
- ✅ Edge cases handled properly (empty sets, deduplication, per-year isolation)
- ✅ XSS escaping maintained
- ✅ Documentation aligned with code

**Verdict: APPROVED FOR MERGE**

The checked-in filtering meets all requirements. Privacy-preserving and semantically correct (participation roster vs. raw registration dump).

### PR #25 — --data-root Parameter Review (2026-04-01)

**Reviewed:** `--data-root` parameter implementation in `src/ShowcaseResults.Cli/Program.cs`

**Findings:**
- **BUG 1:** Empty string or whitespace-only `--data-root` values are not validated. Empty string (`""`) resolves to `F:\cwc\showcase-results\input\...` instead of falling back to default `data\input\...`. Whitespace-only (`"   "`) resolves to invalid path `F:\cwc\showcase-results\   \input\...`.
- **BUG 2:** Help text for `carverOutputOption` (line 139) does not mention `{data-root}` prefix. Help text says `"Path for the output HTML file (default: output/carver-{id}.html)"` but should say `"Path for the output HTML file (default: {data-root}/output/carver-{id}.html or output/carver-{id}.html)"` to match the pattern used for all other options.
- **APPROVED:** Three-tier path resolution logic is correct for non-empty data-root values. Explicit paths always win. Trailing slashes handled correctly by `Path.Join()`. Relative data-root paths accepted. Option registration is correct (separate `carverOutputOption` vs `outputOption`, both using `--output` alias but on different commands).

**Verdict:** REJECTED until bugs fixed.

**Required fixes:**
1. Add validation: `dataRoot = string.IsNullOrWhiteSpace(dataRoot) ? null : dataRoot;` after reading the option value (lines 61, 155) to normalize empty/whitespace to null.
2. Update help text for `carverOutputOption` (line 139) to include `{data-root}` prefix pattern.

### Test File Locations and Year→Event Rename Pattern (2026-04-01)

**Test files in this repo** (no C# xUnit/NUnit test projects exist; all tests are manual test plans in `docs/`):
- `docs/test-plan-issue-7.md` — 28-case manual test plan for the Joomla carver results component
- `docs/test-plan-carvers-list.md` — 19-case manual test plan for the carvers list view
- `docs/test-data-spec.md` — test data specification (JSON structure, not executable tests)

**Rename pattern applied: `year` parameter → `event` parameter**
- URL query params: `?year=2024` → `?event=2024` and `&year=2024` → `&event=2024`
- Applied across all test step "Navigate to" instructions and link format examples
- Updated error message expectations: `"Year must be a valid number."` → `"Event must be a valid number."`
- Updated test case section headers: e.g., "No Year Parameter — Year Selector Shown" → "No Event Parameter — Event Selector Shown"
- NOT changed: data model field references (`"year": 2024` in JSON schema), data concept uses ("single-year carver", "multiple years coexist"), or PHP method names
- No C# test files or `--year` CLI flag references found in the test plan docs
- No `dotnet test` run needed — no executable test projects exist in the solution

## Session: PR #25 — --data-root Parameter (Code Review) (2026-04-09)

**Role:** Tester (Code Review)  
**Task:** Review --data-root parameter implementation and identify issues

**Review findings:**
- 🐛 **BUG 1 — Empty/Whitespace Not Validated:** `--data-root ""` or `--data-root "   "` produces malformed paths; should normalize to null for fallback to defaults
- 🐛 **BUG 2 — Incomplete Help Text:** carverOutputOption help text omits {data-root} prefix pattern; inconsistent with other options
- ✅ **APPROVED:** Three-tier path resolution logic correct for non-empty values

**Verdict:** REJECTED — Return to developer for fixes

**Next:** Gandalf assigned to apply fixes

### Issue #30 — Checked-In Carvers Validation (2026-04-23)

**Reviewed:** PR #33 for `joomla/com_showcaseresults/site/src/Service/ResultsService.php` and related carvers-list docs.

**Learnings:**
- Public carvers lists must be derived from result-bearing `carver_id` values, not directly from the `competitors` registration array.
- The checked-in set comes from the union of `special_prizes[].carver_id`, `overall_results[].places[].carver_id`, and `division_results[].categories[].places[].carver_id`.
- Validation anchor: `joomla/com_showcaseresults/media/data/results-2026T.json` has 37 registrations but only 35 checked-in carvers, so IDs 22 and 34 should stay hidden.
- Related paths for this behavior: `joomla/com_showcaseresults/site/src/Service/ResultsService.php`, `joomla/com_showcaseresults/site/tmpl/carvers/default.php`, `docs/test-plan-carvers-list.md`, and `docs/joomla-extension.md`.

## Session: Issue #30 — Checked-In Filtering Moved to JSON Layer (2026-04-23)

**Role:** Tester  
**Task:** Review PR #33 after data-layer change

**Validation performed:**
- Verified `Competitor.xlsx` has no explicit checked-in column; fallback detection correctly applied by Bilbo
- Regenerated `results-2026T.json` from source data: 35 competitors from 37 registrations, excluding IDs 22 and 34
- Confirmed generated JSON matches golden sample in `joomla/com_showcaseresults/media/data/results-2026T.json`
- Validated filtering logic: union of special_prizes[].carver_id + overall_results[].places[].carver_id + division_results[].categories[].places[].carver_id
- Confirmed Joomla component now trusts pre-filtered competitors; no redundant filtering needed
- All test cases pass; no regressions detected

**Key findings:**
- ✅ Checked-in filtering logic correct and complete
- ✅ Fallback behavior working as designed
- ✅ Edge cases handled: empty sets, deduplication, per-year isolation
- ✅ XSS escaping and security posture maintained
- ✅ Documentation updated and aligned

**Verdict: APPROVED FOR MERGE** — PR #33 meets all requirements and is production-ready.
