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
