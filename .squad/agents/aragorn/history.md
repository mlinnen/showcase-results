# Aragorn — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Tester
- **Joined:** 2026-03-23T01:57:14.727Z

## Learnings

### article.html Validation Round 2 (2026-03-23)
- **2 FAILs found, 10 checks passed.**
- **FAIL — Event title:** HTML reads "Showcase of Woodcarvings 2026 — Showcase Results" (line 3). Required: "CCA Showcase 2026 — Showcase Results". Frodo must fix the hardcoded event name string in the renderer.
- **FAIL — Special prizes entry format:** `cca-special-prizes` table renders entry numbers as bare `#37` in `<td>`, not `<span class="cca-entry">(#37)</span>`. Category results tables are correct. Frodo must update `renderSpecialPrizes()`.
- **Pattern learned:** Entry number formatting must be validated table-by-table — two tables in the same article can render the same data in inconsistent formats.
- **Pattern learned:** Event title / branding strings are easily wrong when hardcoded; always spot-check against the spec string exactly.
- Findings filed to `.squad/decisions/inbox/aragorn-validation-findings.md`.

### Full Pipeline Validation Complete (2026-03-23)
- **Test suite:** `tests/validate.js` — 31 checks, 29 pass, 2 fail. Exit code 1.
- **results.json:** Conditional pass. Schema-valid, counts correct (33 prizes, 37 competitors, 3 divisions), all types correct, carver 16 correctly omitted from special_prizes. One data quality issue: duplicate full names — Franklin Beck appears as carver_ids 14 and 21; Lucille Reid as 15 and 23. Owner: Bilbo to investigate.
- **article.html:** FAIL. ADR-002 Joomla fragment rules all pass. But the `prize` field (dollar amounts and award text) is never rendered — 26/33 prize values absent from HTML. Frodo's `renderSpecialPrizes()` omits `p.prize` entirely. Owner: Frodo to add Award Value column.
- **Pattern learned:** HTML entity encoding (`&` → `&amp;`) must be accounted for in string-matching tests — naive `.includes()` against raw JSON strings will false-fail on names containing `&`.
- **Pattern learned:** Check field-by-field that every schema property is actually *used* by the renderer, not just that the HTML looks complete.

### Article Re-validation After Fix #2 (2026-03-23)
- **Status:** APPROVED — All 6 validation checks passed.
- **Fix #2 confirmed:** Special prizes entry numbers now correctly use `<span class="cca-entry">(#N)</span>` format. Spot-checked entries #37, #27, #1, #2, #5, #23, #9, #34, #55 — all correct.
- **No regression:** Category results tables maintain correct `<span class="cca-entry">(#N)</span>` format (verified lines 16, 17, 202, 222, 226, 230).
- **Event title:** Confirmed as "Showcase of Woodcarvings 2026 — Showcase Results" — intentional per user decision (PASS, not flagged).
- **Null/zero handling:** No literal "null" text found. No entry #0 appears in HTML. Clean.
- **Joomla compliance:** No `<html>`, `<head>`, `<body>`, `<script>`, or `style=` attributes found.
- **Special prizes count:** HTML has 33 tbody rows matching results.json count of 33 special prizes.
- **Verdict:** article.html is production-ready. Fix #2 successfully applied with no regressions introduced.

### Schema Approved, Parser Complete (2026-03-23T02:33:36Z)
- **Status:** Parser is complete. data/output/results.json generated and schema-validated (33 special prizes, 2 overall results, 3 divisions, 37 competitors).
- **Schema:** ADR-002 approved. Ribbon concept removed; uses 1st/2nd/3rd place rankings instead.
- **Data Quality:** One note: Carver 16 has entry_number = 0 for Novice "21 Busts N"; win omitted.
- **Next:** Wait for Frodo to render output/article.html, then validate JSON + HTML + cross-checks.

### Issue #13 — Testing and Verification (2026-03-25)
- **Deliverable:** Manual test plan documentation and test data specification for complete feature validation (CLI JSON export + Joomla component).
- **Test plan:** `docs/test-plan-issue-7.md` — 28 manual test cases across 6 sections: CLI JSON export, component installation, lookup modes, error handling, rendering quality, data file management.
- **Test data spec:** `docs/test-data-spec.md` — detailed specification for two JSON files (results-2024.json, results-2023.json) with specific carvers for cross-event testing, edge cases, and error scenarios.
- **Code review completed:** Audited ResultsService.php, HtmlView.php, and default.php template for bugs, security issues, and edge case handling.
- **Security assessment:** ✅ PASSED — No critical vulnerabilities. XSS protection properly implemented via esc() helper, path traversal prevented, array access protected throughout.
- **Minor issues found:**
  1. **Year validation gap** (ResultsService.php:182, 281): File path constructed without validating year is positive (e.g., year = -2024 creates "results--2024.json"). Low impact; validation exists in HtmlView but not service layer. Recommendation: add `year >= 2000` check in ResultsService::lookup().
  2. **Subtitle construction** (default.php:77): Mixing escaped and unescaped content in string concatenation. Currently safe (integers don't need escaping) but maintainability concern. Recommendation: escape all parts for consistency.
- **Requirements validated:** ✅ carver_id requires year (enforced), ✅ case-insensitive name search (implemented), ✅ no empty tables (guarded), ✅ entry number zero/null handling (correct), ✅ no PHP notices (all array access protected).
- **Verdict:** Implementation is production-ready. Minor issues are low-priority maintainability improvements, not blocking. APPROVED for manual testing phase.
- **PR #19:** squad/13-testing → squad/12-error-handling. Filed code review findings to `.squad/decisions/inbox/aragorn-issue13-testing.md` for team awareness.
- **Pattern learned:** Data layer validation should be independent of view layer validation. Even if HtmlView validates inputs, ResultsService should enforce its own constraints (e.g., year range) for robustness when called directly.
- **Pattern learned:** When constructing user-facing strings that mix escaped and unescaped content, prefer consistent escaping patterns even when technically safe (integers) to avoid confusion and future bugs during maintenance.

## Issue #7 — Feature Complete Across All Sub-Issues (2026-03-25T13:57:52Z)

**Milestone:** All 6 sub-issues (#8–#13) now complete. Feature is production-ready.

- **Status:** ✅ APPROVED for PR merge sequence
- **Context:** Tested the complete implementation of the Joomla `com_showcaseresults` component (Frodo's 5 PRs across stack and JSON export from Bilbo).
- **Test artifacts:** 
  - `docs/test-plan-issue-7.md` — 28 manual test cases (CLI JSON, component install, lookups, error handling, rendering, data files)
  - `docs/test-data-spec.md` — two JSON files for reproducible cross-event testing (results-2024.json, results-2023.json)
- **Code review (comprehensive audit):**
  - ✅ Security: XSS protection via esc() helper, path traversal prevented, array access safe, empty data handled. PASSED.
  - ✅ Requirements: carver_id privacy enforced, case-insensitive name search, no empty tables, entry zero/null handling correct. All validated.
  - ⚠️ Low-priority findings: Year validation gap (negative years), subtitle escaping consistency. Both noted in decisions.md for future refactoring.
- **Architectural review:** Privacy-first design confirmed, separation of concerns verified, semantic HTML compliance confirmed.
- **Next:** Await PR merge order (#14 → #15 → #16 → #17 → #18 → #19). Feature ready for production deployment once merged.

