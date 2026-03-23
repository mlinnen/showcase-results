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

