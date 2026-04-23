# Aragorn — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Tester
- **Joined:** 2026-03-23T01:57:14.727Z

## Core Context (Summarized)

**Initial validation phase (2026-03-23):** Validated article.html rendering and identified 2 failures (event title branding, special prizes entry format). Conducted full pipeline validation (results.json schema, article.html HTML compliance, cross-checks). Found data quality issue (duplicate carver names — Franklin Beck IDs 14/21, Lucille Reid IDs 15/23). Revalidated after Frodo's fixes — all 6 checks passed, article approved for production.

**Issue #7 comprehensive audit (2026-03-25):** Performed full code review of Joomla component (ResultsService.php, HtmlView.php, template). Security audit PASSED: XSS protection via esc() helper, path traversal prevented, array access safe. Found 2 low-priority issues: year validation gap (negative years), subtitle escaping consistency. Created 28-case manual test plan and test data spec (results-2024.json, results-2023.json) for reproducible testing.

**Feature expansion testing (2026-03-26, 2026-04-01):** Created 19-case test plan for carvers list view (10 primary + 9 edge cases) covering year filtering, sorting, XSS escaping, cross-year duplication. Validated Issue #24 year-as-string changes across full stack — found 4 critical failures (template getInt(), 3 JSON files with integer years), coordinated fixes with Frodo/Bilbo, re-validated all 4 failures resolved. Final verdict: APPROVED FOR PRODUCTION.

## Issues Worked (Summarized)

- **Issue #7 (Joomla component):** Code review audit (security, requirements, architectural), 28-case test plan, test data spec. Minor findings noted for future refactoring. APPROVED for production.
- **Issue #22 (Carvers list view):** 19-case test plan (10 primary + 9 edge cases) for year-filtered competitor listing, division display, sorting, XSS security.
- **Issue #24 (Year as string):** Full stack validation across C#, PHP, Joomla, JSON; identified 4 critical failures; coordinated fixes with team; re-validated all resolved. APPROVED for production.

## Issue #7 — Feature Complete Across All Sub-Issues (2026-03-25T13:57:52Z)

**Milestone:** All 6 sub-issues (#8–#13) now complete. Feature is production-ready.

- **Status:** ✅ APPROVED for PR merge sequence
- **Test artifacts:** 
  - docs/test-plan-issue-7.md — 28 manual test cases (CLI JSON, component install, lookups, error handling, rendering, data files)
  - docs/test-data-spec.md — two JSON files for reproducible cross-event testing (results-2024.json, results-2023.json)
- **Code review (comprehensive audit):**
  - ✅ Security: XSS protection via esc() helper, path traversal prevented, array access safe, empty data handled. PASSED.
  - ✅ Requirements: carver_id privacy enforced, case-insensitive name search, no empty tables, entry zero/null handling correct. All validated.
  - ⚠️ Low-priority findings: Year validation gap (negative years), subtitle escaping consistency. Both noted in decisions.md for future refactoring.
- **Architectural review:** Privacy-first design confirmed, separation of concerns verified, semantic HTML compliance confirmed.
- **Next:** Await PR merge order (#14 → #15 → #16 → #17 → #18 → #19). Feature ready for production deployment once merged.

## Issue #22 — Carvers List View Test Plan (2026-03-26)

**Deliverable:** Comprehensive test plan for new carvers list view feature.

- **Artifact:** docs/test-plan-carvers-list.md — 19 test cases (10 primary + 9 edge cases) covering all functionality
- **Test coverage breakdown:**
  - **Primary paths (1–10):** Year filtering, year selector, invalid input handling, carver sorting, division display, name linking, XSS escaping, multi-year navigation
  - **Edge cases (11–19):** Empty data, negative years, non-numeric input, very large years, duplicate names cross-year, special characters, long names, no data files, pagination
- **Key discoveries:** List views have different requirements than detail views (include all competitors regardless of results status). Per-year carver_id isolation maintains privacy. Comprehensive test data covers sorting, XSS, special characters, and cross-year duplication.

## Issue #24 — Year as String (Full Stack Validation) (2026-04-01)

**Status:** ✅ COMPLETE — All failures resolved and APPROVED FOR PRODUCTION

**Initial validation findings:**
- 4 critical failures identified: 1 PHP getInt() reference, 3 JSON files with integer years
- Full stack validation: 28+ checks performed across C#, PHP, Joomla, JSON layers
- Identified that schema changes from integer → string require data file migration

**Coordinated fixes (with Frodo and Bilbo):**
1. Frodo: Fixed getInt('year') → getString('year') in joomla/.../default.php
2. Bilbo: Updated output/*.json and joomla/.../data/*.json year values from integers to strings
3. Aragorn: Re-validated all fixes

**Re-validation results:**
- ✅ All 4 failures resolved
- ✅ Full stack validation: 28+ checks passed
- ✅ 0 regressions detected
- ✅ Security audit: PASSED
- ✅ Backward compatibility: verified (numeric years as strings work)
- ✅ Documentation: updated with alphanumeric year example

**Final verdict: APPROVED FOR PRODUCTION** — Ready for commit to main branch.

## Session: Issue #24 — Year as String (2026-04-01)

**Role:** Tester  
**Task 1:** Initial validation (found 4 failures)  
**Task 2:** Re-validation after fixes

Initial pass identified 4 issues:
1. Missing getString() in default.php (Frodo fixed)
2-4. JSON data files with integer years (Bilbo fixed)

Re-validation after fixes: ✅ ALL 4 FAILURES RESOLVED
- Full stack validation: 28+ checks performed
- 0 regressions detected
- Security audit: PASSED
- Verdict: APPROVED FOR PRODUCTION

Ready for commit to main branch.

## Learnings

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
