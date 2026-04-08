# Frodo — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Content Builder
- **Joined:** 2026-03-23T01:57:14.727Z

## Core Context (Summarized)

**Early project phase (2026-03-23):** Rendered carving competition results into HTML article format per ADR-002 spec. Created Node.js renderer (`src/render/index.js`) producing semantic HTML with three sections (special prizes, overall results, division results). Entry numbers formatted as `<span class="cca-entry">(#N)</span>`, with proper null/zero handling. Joomla compliance verified: no HTML wrappers, inline styles, or scripts.

**Joomla component build phase (2026-03-25):** Scaffolded `com_showcaseresults` component with service providers, controllers, views, templates, and build script. Implemented three-mode data layer (ResultsService): (1) cross-event name lookup, (2) single-year name+year, (3) per-event carver_id (privacy-constrained). Full template rendering completed with ordinal() helper, error handling, security (esc() helper, path traversal prevention). Minor findings: year validation gap, subtitle escaping consistency (deferred).

**Feature expansion (2026-03-29):** Added carvers list view (year-filtered competitor table with division field, sorted by name). Extended ResultsService with getCarversList() method and promoted getAvailableYears() to public. Updated JSON schema with division field for competitors.

## Issues Worked (Summarized)

- **Issue #7 (Joomla component):** Scaffolded component, implemented data layer (ResultsService with 3 lookup modes), full HTML template rendering with ordinal() helper, error handling, security auditing. All 6 sub-issues (#8–#13) delivered across C#/PHP/JSON layers.
- **Issue #22 (Carvers list view):** Extended schema with division field, implemented getCarversList() method, new CarversView and year-selector template, component ZIP rebuilt.
- **Issue #24 (Year as string):** Updated all Joomla year parameters from integer to string, validation via alphanumeric regex `/^[a-zA-Z0-9]+$/`, XML fields changed from type="number" to type="text", Joomla regex updated to match alphanumeric filenames.

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

## Joomla Extension User Guide (2026-03-29)

**Status:** ✅ COMPLETE. Markdown user guide written for non-technical Joomla admins.

- **File:** `docs/joomla-extension.md` (12.0 KB)
- **Sections covered:** Overview, Installation, Adding result data, Setting up menu items, URL parameter reference, JSON data file format, Generating JSON from spreadsheets, Troubleshooting
- **Security coverage:** Parameter validation, path handling, caching, XSS context (HTML escaping)


## Session: Issue #24 — Year as String (2026-04-01)

**Role:** Content Builder  
**Task 1:** Joomla PHP/XML year string changes  
**Task 2:** Fix missed getInt() in default.php

Implemented year-as-string throughout Joomla component:
- ResultsService: year parameter int → string|null
- HtmlView: validation uses preg_match() for alphanumeric
- default.php: all year inputs getInt() → getString()
- XML: carver/carvers menu fields type="number" → type="text"
- Regex: updated getAvailableYears() to match alphanumeric filenames

Status: ✅ COMPLETED - Both tasks merged and validated.

## Learnings

### Session: `year` → `event` parameter rename (2026-04-01)

**Task:** Renamed the `year` URL query parameter to `event` across all Joomla extension files and documentation.

**Files containing `year` parameter references (now updated to `event`):**

**Joomla PHP/XML/INI:**
- `site/src/View/Carver/HtmlView.php` — `getString('year')` → `getString('event')`, `$yearRaw` → `$eventRaw`, `$year` → `$event`, all return-array keys `'year'` → `'event'`, error codes `invalid_year`/`carver_id_requires_year` → `invalid_event`/`carver_id_requires_event`, function signature
- `site/src/View/Carvers/HtmlView.php` — same getString/variable pattern; `'no_year'` → `'no_event'`, `'available_years'` → `'available_events'`, `getAvailableYears()` → `getAvailableEvents()`
- `site/src/Service/ResultsService.php` — all method signatures (`$year` → `$event`), `lookupByNameAndYear` → `lookupByNameAndEvent`, `lookupByCarverIdAndYear` → `lookupByCarverIdAndEvent`, `getCarversList`, `getAvailableYears` → `getAvailableEvents`, error keys `year_not_found` → `event_not_found`, `search_year` → `search_event`, error messages
- `site/tmpl/carver/default.php` — `getString('year')` → `getString('event')`, `$year` → `$event`, code examples `&amp;year=` → `&amp;event=`
- `site/tmpl/carvers/default.php` — `no_year` → `no_event`, `available_years` → `available_events`, URL construction `&year=` → `&event=`, cast `(int)` → `escCarvers()` for string event values
- `site/tmpl/carver/default.xml` — `name="year"` → `name="event"`, language key `FIELD_YEAR_*` → `FIELD_EVENT_*`
- `site/tmpl/carvers/default.xml` — same XML field rename
- `site/language/en-GB/com_showcaseresults.ini` — `FIELD_YEAR_LABEL/DESC` → `FIELD_EVENT_LABEL/DESC`

**Documentation:**
- `docs/joomla-extension.md` — all `?year=`/`&year=` URL examples, param table row, menu item instructions, error messages
- `docs/test-plan-issue-7.md` — URL param examples in test steps, error messages, carver_id/year relationship tests
- `docs/test-plan-carvers-list.md` — `?year=` URL params, "year selector" → "event selector", test case titles/criteria

**README.md:** `--year` CLI flag → `--event`

**NOT renamed (intentional):** `data['event']['year']` JSON field access, `event_year` data key in return arrays, `results-{year}.json` filename pattern references (filenames still use year values), prose descriptions of year as a concept, `cca-year-selector` CSS class.
