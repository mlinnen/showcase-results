# Frodo — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Content Builder
- **Joined:** 2026-03-23T01:57:14.727Z

## Learnings

### Schema Approved, Parser Complete (2026-03-23T02:33:36Z)
- **Status:** Unblocked. Parser complete, data/output/results.json ready.
- **Schema:** ADR-002 approved. 33 special prizes, 2 overall results, 3 divisions, 37 competitors. No ribbon concept — uses 1st/2nd/3rd place rankings.
- **Data Quality:** One note: Carver 16 (Erik Mitchell) has entry_number = 0 for Novice "21 Busts N"; win omitted from output.
- **Next:** Build output/article.html from results.json per ADR-002 constraints (sections: special prizes, overall results, division results).

### Article Built (2026-03-23)
- **Status:** Complete. `output/article.html` generated (1190 lines). `src/render/index.js` written as reproducible Node.js renderer.
- **Joomla compliance verified:** No `<html>/<head>/<body>` wrappers, no inline `style=`, no `<script>` tags. Zero violations.
- **Structure:** Three sections — Special Prizes table (33 rows, sorted by `order`), Overall Results table (2 categories), Division Results (3 divisions × awards + category sub-tables).
- **Style handling:** `null` → empty cell (no "null" text); `N` → "Natural"; `P` → "Painted".
- **Place cells:** Render as "Name (#entry)" using a `<span class="cca-entry">` for the entry number — keeps name and entry scannable without extra columns.
- **Division layout:** Categories with `style: null` (Best Of awards) split into a compact 2-column "Division Awards" table; N/P categories go into a 5-column "Category Results" table. Prevents visual noise from empty Style cells on award rows.
- **Missing places:** Empty `<td>` for 2nd/3rd when not present — no placeholder text, clean output.
- **Pre-existing `article.ts`:** Placeholder TypeScript file left untouched; `index.js` is the new canonical renderer per the task spec.

### Fix 2 — Title and Special Prizes Entry Format (2026-03-23)
- **Status:** Fixed two renderer issues in `src/render/index.js`. Article regenerated.
- **Title fix:** Removed duplicated year from title. Now uses `event.name` directly without appending `event.year`, allowing `event.name` to control the full event title (e.g., "CCA Showcase 2026").
- **Special prizes entry format fix:** Changed entry numbers from bare text `#N` to `<span class="cca-entry">(#N)</span>`, matching the format used in category results tables. Entry number 0 or null/undefined renders as empty `<td></td>`.
- **Spot-checks passed:** Title reads "Showcase of Woodcarvings — Showcase Results" (will read "CCA Showcase 2026 — Showcase Results" once JSON updated); special prizes table uses correct `<span class="cca-entry">(#N)</span>` format; no literal "null" text; no entry #0 displayed.

### Division Results Table in Carver Article (2026-03-25)
- **Status:** Complete. `RenderCarverArticle()` division results block changed from `<ul>/<li>` to a `<table>` per division.
- **Table columns:** Category (name + style suffix when present), Place (e.g., "1st Place"), Entry # (entry number, empty when 0).
- **CSS class:** `cca-carver-division-results` applied to each division table.
- **Scope:** Only the carver article's division results block was modified — `RenderDivisionResults()` (main article), overall results, and special prizes were untouched.
- **Build:** Passed with 0 errors (2 pre-existing NU1903 warnings only).

### Special Prizes Prize Column Enhancement (2026-03-23)
- **Status:** Enhanced special prizes table with new Prize column. `renderSpecialPrizes()` updated, article regenerated.
- **Table structure:** Column order now: Name | Prize | Winner | Entry (was: Prize | Winner | Entry). First column header changed from "Prize" to "Name" to reflect that it displays the award name (e.g., "Best of Show").
- **Prize value formatting:** New Prize column (renders `p.prize` field) implements smart formatting: numeric strings (e.g., "450") render as dollar amounts ("$450"), non-numeric strings render as-is (e.g., "Lee S. Dukes Memorial Award Commemorative Plaque"), missing/null/empty values render as empty `<td></td>`.
- **Verification passed:** Checked output/article.html — `<th>Name</th>` and `<th>Prize</th>` present in correct order, dollar amounts render correctly (e.g., $450, $250), non-numeric prizes display as plain text, regex pattern `/^\d+(\.\d{1,2})?$/` correctly identifies numeric values for currency formatting.

### Joomla Component Scaffold (2026-03-25)
- **Status:** Complete. Created `com_showcaseresults` Joomla component scaffold. PR #15 opened to dev.
- **Files created:** 12 files — manifest XML, controller, view, template (placeholder), menu item XML, service providers (site/admin), language files (en-GB), media folder, build script.
- **Structure:** `joomla/com_showcaseresults/` with site/, admin/, and media/ folders. Namespace `Mlinnen\Component\ShowcaseResults` registered.
- **Query params:** View accepts `name`, `carver_id`, `year` parameters. Placeholder template displays them for testing.
- **Build script:** `joomla/build.ps1` creates `com_showcaseresults.zip` (7.25 KB) — tested successfully.
- **Placeholders:** Controller and view are minimal skeletons. Business logic (data loading) comes in #10, rendering enhancement in #11, error handling (carver_id without year) in #12.
- **Joomla compliance:** Manifest compatible with Joomla 4.x and 5.x, PHP 8.1+ requirement, follows modern component structure with service providers and DI container registration.

### Joomla Data Layer — ResultsService (2026-03-25)
- **Status:** Complete. Implemented PHP ResultsService class. PR #16 opened to squad/9-joomla-scaffold.
- **Files created:** `site/src/Service/ResultsService.php` (504 lines, 9 methods)
- **Files updated:** `HtmlView.php` (instantiates service, sets page title), `default.php` (displays carverData with var_dump)
- **Three lookup modes implemented:**
  1. **Name cross-event:** `?name=John+Doe` — scans all results-*.json files, groups by event, sorts by year descending
  2. **Name single-event:** `?name=John+Doe&year=2024` — scans one file
  3. **ID single-event:** `?carver_id=16&year=2024` — scans one file
- **Critical design constraint:** carver_id is per-event only (privacy feature) — CANNOT be used to correlate across events. Only name works cross-event.
- **Edge cases handled:** carver_id without year (error), name not found (found=false), no JSON files (error=no_data), malformed JSON (skip with warning)
- **Data path:** `JPATH_ROOT . '/media/com_showcaseresults/data'` (expects results-{year}.json from CLI JSON export)
- **Return structure:** Array with carver_name, found flag, results array (event_name, event_year, special_prizes, overall_results, division_results)
- **Helper methods:** loadResultsFile(), getResultsFiles(), extractCarverResults(), findCarverIdByName()
- **Template rendering:** Raw var_dump for now — real HTML rendering comes in #11
- **Next:** Issue #11 will replace var_dump with proper HTML table rendering matching the Node.js article renderer's style

### Joomla Template Rendering — Issue #11 (2026-03-25)
- **Status:** Complete. Full HTML template rendering implemented in default.php. PR #17 opened to squad/10-data-layer.
- **File modified:** `site/tmpl/carver/default.php` (157 lines added, 13 removed)
- **ordinal() helper:** Inline function converts place numbers to ordinal text (1st, 2nd, 3rd, 4th...). Handles edge cases: 11th, 12th, 13th (not 11st, 12nd, 13rd).
- **Data structure from HtmlView:** Template receives `$this->carverData` with keys: carver_name (string), found (bool), error (optional), results (array of event records)
- **Event record shape:** event_name, event_year, special_prizes (array), overall_results (array with category + places), division_results (array with division + categories array, each category has name, style, places)
- **Template structure:**
  - **Page header (cca-carver-header):** Carver name + subtitle (cross-event vs single-event)
  - **Event sections (cca-event-section):** One per event, already sorted by year descending via ResultsService
  - **Special Prizes (cca-special-prizes):** 3-column table (Award, Prize, Entry #)
  - **Overall Results (cca-overall-results):** 3-column table (Category, Place, Entry #)
  - **Division Results (cca-division-results):** One table per division, 4-column (Category, Style, Place, Entry #)
- **Empty handling:** Sections with no data are completely skipped (no empty tables rendered)
- **Entry numbers:** 0 or null render as empty `<td></td>` (no text)
- **Style values:** N → "Natural", P → "Painted", null → empty cell
- **Semantic HTML:** Uses `<section>`, `<table>`, `<thead>`, `<tbody>`, `<h1>`-`<h4>` tags. No inline styles, no CSS frameworks, no Joomla article wrappers.
- **CSS classes:** All use cca-* prefix per team conventions
- **Subtitle logic:** Single-event (year param present) shows "Results for {Event Name} {Year}", cross-event shows "Results across all events"
- **Next:** Issue #12 will add error handling and edge cases; issue #13 will add testing/verification

### Joomla Error Handling — Issue #12 (2026-03-25)
- **Status:** Complete. Comprehensive error handling and parameter validation. PR #18 opened to squad/11-template-rendering.
- **Files modified:** `HtmlView.php` (added validateParameters() method, 122 lines), `ResultsService.php` (enhanced error messages, added getAvailableYears()), `default.php` (error display logic, esc() helper)
- **Parameter validation (HtmlView):** No parameters → usage instructions; non-numeric carver_id/year → friendly errors; carver_id without year → "A year is required..." (explains privacy constraint); name + carver_id → name takes precedence
- **Not-found states (ResultsService):** Name not found cross-event, name not found in year, carver_id not found, registered but zero results — all with contextual, helpful messages
- **Data file errors:** No files, year missing (shows available years via getAvailableYears()), malformed JSON (graceful handling)
- **Security:** HTML escaping via esc() helper using ENT_QUOTES|ENT_HTML5; year validated numeric BEFORE path construction (path traversal prevention)
- **Error display:** `<div class="cca-usage">` for no-params (instructional), `<div class="cca-error">` for all others
- **Next:** Issue #13 (testing and verification)

### Carvers List View — Issue #22 (2026-03-29)
- **Status:** Complete. New `carvers` (plural) view delivering a year-filtered list of all competitors.
- **JSON updated:** Added `"division"` field to every competitor in `results-2024.json` and `results-2023.json`. Division derived from first appearance in `division_results`. Carvers with no results receive `""`.
- **ResultsService changes:** New public method `getCarversList(int $year): array` returns `event_name`, `event_year`, `carvers[]` (each with `carver_id`, `first_name`, `last_name`, `full_name`, `division`), sorted by last_name then first_name (case-insensitive). `getAvailableYears()` promoted from `private` to `public` (needed by HtmlView year-selector).
- **New files:** `site/src/View/Carvers/HtmlView.php`, `site/tmpl/carvers/default.php`, `site/tmpl/carvers/default.xml`.
- **Template:** Year-selector (`cca-year-selector`) when no year param; table (`cca-carvers-list`) with Carver ID, Name (linked to carver detail view via `Route::_()`), Division when year provided; error block (`cca-error`) on data failure.
- **Build:** ZIP rebuilt successfully — 31 entries (was 28). All new files confirmed present.
- **Naming note:** Template uses `escCarvers()` helper instead of `esc()` to avoid PHP global function collision if both carver templates ever end up in the same request scope.

## Issue #7 — Feature Complete (2026-03-25T13:57:52Z)

**Milestone:** All 5 PRs (#15–#19) delivered and validated by Aragorn. Feature is production-ready.

- **Summary:** Joomla `com_showcaseresults` component now fully implemented with data layer, view rendering, error handling, and comprehensive test coverage.
- **Total contribution:** 5 open PRs, ~800 lines of production PHP code (scaffold, service layer, template, error handling).
- **Validation status:**
  - ✅ Code review PASSED (Aragorn audit: security, error handling, edge cases verified)
  - ✅ Test plan delivered (28 cases, 6 sections)
  - ✅ Test data specification created (results-2024.json, results-2023.json)
- **Minor findings:** 2 low-priority maintainability issues identified (year validation, subtitle escaping) — noted in decisions.md for future refactoring.
- **Awaiting:** PR merge sequence (#14 → #15 → #16 → #17 → #18 → #19).

## Issue #22 — Carvers List View (2026-04-01T02:05:44Z)

**Status:** ✅ COMPLETE and committed to dev.

- **Commit:** a354e6a — feat: add carvers list view (issue #22)
- **Deliverables:**
  - Extended results.json schema with `division` field for each competitor
  - `ResultsService::getCarversList(int $year): array` — returns all carvers for a given year, sorted by last_name then first_name
  - New `CarversView` controller and `Carvers/HtmlView.php` (site component)
  - New template `site/tmpl/carvers/default.php` with year-selector and carvers table
  - Updated component ZIP with all new files
- **Template features:** Year-selector dropdown (when no year param), carvers table (ID, Name, Division), error handling for missing data, XSS protection via escCarvers() helper
- **Data model:** Each carver row includes carver_id, first_name, last_name, full_name, division. Division derived from competitor's first appearance in division_results array, or "" if no results.
- **Navigation:** Carver names linked via Route::_() to carver detail view (?view=carver&name=...)
- **Build:** ZIP successfully regenerated (31 entries, all new files verified present)
- **Next:** Ready for Aragorn's QA testing per test plan

### Joomla Extension User Guide (2026-03-29)

**Status:** ✅ COMPLETE. Markdown user guide written for non-technical Joomla admins.

- **File:** `docs/joomla-extension.md` (12.0 KB)
- **Sections covered:**
  1. **Overview** — What the extension does (carvers list + carver detail views, JSON-based data loading)
  2. **Installation** — 3-step process: download, upload via Joomla admin, verify
  3. **Adding result data** — File location `media/com_showcaseresults/data/`, naming convention `results-{year}.json`, uploading, caching behavior
  4. **Setting up menu items** — Step-by-step for Carvers List (with year param) and Carver Detail (blank params for search); year-selector fallback explained
  5. **URL parameter reference** — Complete parameter tables for both views (cross-event search by name, single-year, carver_id+year), examples, privacy note on per-event carver_id
  6. **JSON data file format** — Sample JSON structure with field annotations; link to schema/results.schema.json for full spec
  7. **Generating JSON from spreadsheets** — References CLI tool, quick command example, feature list
  8. **Troubleshooting** — 6 common issues (file path, cache, name mismatch, division blank, carver_id without year, malformed JSON) with symptoms, causes, fixes
- **Tone & style:** Plain English, step-by-step where appropriate, headers/tables/code blocks, assumes Joomla basics but not extension knowledge
- **Security coverage:** Parameter validation, path handling, caching (Joomla cache plugin warning), XSS context (HTML escaping)
- **Cross-references:** CLI docs, schema, README.md, Joomla menu/extensions UI

