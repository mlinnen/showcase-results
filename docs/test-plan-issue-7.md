# Manual Test Plan: Carver Results Feature (Issue #7)

## Prerequisites

- Joomla 4.x or 5.x test instance running and accessible
- CLI tool built with .NET 8.0+ (`dotnet build`)
- Component package built (`joomla/build-component.sh` or equivalent)
- At least two `results-{year}.json` files uploaded to `media/com_showcaseresults/data/` (see Test Data Requirements below)
- Access to Joomla admin panel for component installation
- Browser with developer tools for inspecting HTML output

## Test Data Requirements

Two JSON files are required for comprehensive testing:

### `results-2024.json`
- **Carver 16 (John Smith)**: Has special prizes, overall results, and division results (all three types)
- **Carver 23 (Jane Doe)**: Appears in both 2024 and 2023 (same name, different carver_id)
- **Carver 8 (Bob Wilson)**: Registered competitor with zero results (in competitors array but no wins)
- **Carver 42 (Alice Brown)**: Only appears in 2024 (single-year carver)
- Multiple entries with varying entry numbers including some with entry_number = 0 (to test null handling)

### `results-2023.json`
- **Carver 19 (Jane Doe)**: Same person as carver 23 in 2024, different ID (tests cross-event name lookup)
- **Carver 7 (Charlie Green)**: Only appears in 2023
- At least 10 total competitors to ensure realistic data volume

**Schema Compliance**: Both files must validate against `schema/results.schema.json` with required properties: event, special_prizes, overall_results, division_results, competitors.

See `docs/test-data-spec.md` for detailed specifications.

---

## Section 1: CLI JSON Export

**Dependencies**: Issue #8 (CLI JSON export implementation)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.1 | JSON output creation | 1. Run `showcase-results create results --input data/2024.xlsx --format json --output output/test.html`<br>2. Check output directory | File `output/results-2024.json` is created alongside HTML output | [ ] |
| 1.2 | Schema validation | 1. Use JSON schema validator (e.g., `ajv-cli`)<br>2. Run `ajv validate -s schema/results.schema.json -d output/results-2024.json` | Validation passes with no errors; confirms all required fields present | [ ] |
| 1.3 | Default format unchanged | 1. Run `showcase-results create results --input data/2024.xlsx --output output/test2.html` (no `--format` flag)<br>2. Check output directory | Only `output/test2.html` created; no JSON file generated (backward compatibility maintained) | [ ] |
| 1.4 | Year in filename | 1. Generate JSON for events from different years<br>2. Compare filename year to `event.year` field in JSON | Filename is `results-{year}.json` where year matches the `event.year` property value | [ ] |

---

## Section 2: Component Installation

**Dependencies**: Issue #9 (Joomla component scaffolding)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.1 | Install on Joomla 4.x | 1. Navigate to Extensions > Manage > Install<br>2. Upload `com_showcaseresults.zip`<br>3. Click Install | Extension installs successfully with "Installation successful" message; no PHP errors or warnings in browser console or Joomla error log | [ ] |
| 2.2 | Install on Joomla 5.x | 1. Navigate to System > Install > Extensions<br>2. Upload `com_showcaseresults.zip`<br>3. Click Install | Extension installs successfully with "Installation successful" message; no PHP errors or warnings; component appears in Extensions list | [ ] |
| 2.3 | Menu item creation | 1. Navigate to Menus > Main Menu > Add New Menu Item<br>2. Click "Select" button for Menu Item Type<br>3. Search for "Showcase" or browse to ShowcaseResults component | "Carver Results" menu item type is available and selectable under ShowcaseResults component category | [ ] |
| 2.4 | Data folder exists | 1. Use FTP/SSH or Joomla file manager<br>2. Navigate to `media/com_showcaseresults/`<br>3. Check for `data/` subdirectory | Folder `media/com_showcaseresults/data/` exists and is writable (permissions 755 or 775) | [ ] |

---

## Section 3: Lookup Modes

**Dependencies**: Issues #10 (data layer), #11 (view rendering)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.1 | Name cross-event lookup | 1. Upload `results-2023.json` and `results-2024.json` to media/data folder<br>2. Navigate to `?name=Jane+Doe` | Page displays results from BOTH 2023 and 2024, sorted with 2024 first, then 2023; separate `<section class="cca-event-section">` for each year with event name and year as heading | [ ] |
| 3.2 | Name single-event lookup | 1. Navigate to `?name=Jane+Doe&event=2024` | Page displays ONLY 2024 results for Jane Doe; subtitle reads "Results for [Event Name] 2024"; no 2023 section appears | [ ] |
| 3.3 | ID single-event lookup | 1. Navigate to `?carver_id=16&event=2024` | Page displays results for carver #16 from 2024; page title shows carver's actual name (not the ID number) | [ ] |
| 3.4 | Case insensitivity | 1. Navigate to `?name=jane+doe` (lowercase)<br>2. Navigate to `?name=JANE+DOE` (uppercase)<br>3. Navigate to `?name=Jane+Doe` (mixed case) | All three URLs return identical results; name comparison is case-insensitive | [ ] |
| 3.5 | Multi-year sections | 1. Navigate to `?name=Jane+Doe` (appears in 2+ years)<br>2. Inspect HTML structure | Each year has separate `<section class="cca-event-section">` with `<h2>` showing event name + year; sections ordered by year descending (2024, 2023, 2022, etc.) | [ ] |

---

## Section 4: Error Handling

**Dependencies**: Issue #12 (error handling & edge cases)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 4.1 | ID without year (privacy constraint) | 1. Navigate to `?carver_id=16` (no event parameter) | Page displays error message: "A year is required when looking up by carver ID, because carver IDs differ between events. Try adding &event=2024 or search by name instead."; no PHP error or stack trace | [ ] |
| 4.2 | No parameters (usage guide) | 1. Navigate to component URL with no query string | Page displays usage instructions with examples: `?name=John+Doe`, `?name=John+Doe&event=2024`, `?carver_id=16&event=2024`; wrapped in `<div class="cca-usage">` | [ ] |
| 4.3 | Name not found (typo/nonexistent) | 1. Navigate to `?name=Nobody+Here` (name that doesn't exist in any results file) | Error message: "No results found for 'Nobody Here'. Please check the spelling and try again."; wrapped in `<div class="cca-error">` | [ ] |
| 4.4 | ID not found (nonexistent ID) | 1. Navigate to `?carver_id=9999&event=2024` | Error message: "Carver #9999 was not found in the 2024 results."; no PHP warning about undefined array keys | [ ] |
| 4.5 | No results carver (registered, zero wins) | 1. Navigate to `?name=Bob+Wilson` (registered in competitors array but has no results)<br>2. Or use `?carver_id=8&event=2024` | Error message: "Bob Wilson is a registered competitor in 2024 but has no recorded results."; carver name displayed (not "Competitor #8") | [ ] |
| 4.6 | No data files (empty data folder) | 1. Temporarily rename or remove all JSON files from `media/com_showcaseresults/data/`<br>2. Navigate to `?name=John+Doe` | Error message: "No competition data is currently available. Please check back later."; no PHP file_get_contents warnings | [ ] |
| 4.7 | Invalid event (not found) | 1. Navigate to `?name=John+Doe&event=1999` (year with no data file) | Error message: "No data available for 1999. Available events: 2024, 2023." (or similar, listing actually available years) | [ ] |
| 4.8 | Non-numeric ID (input validation) | 1. Navigate to `?carver_id=abc&event=2024` | Validation error: "Carver ID must be a number."; input rejected before database query | [ ] |
| 4.9 | Non-numeric event (input validation) | 1. Navigate to `?name=John+Doe&event=abcd` | Validation error: "Event must be a valid number."; input rejected before file access | [ ] |

---

## Section 5: Rendering Quality

**Dependencies**: Issue #11 (template rendering)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 5.1 | Semantic HTML structure | 1. Navigate to `?name=John+Smith&event=2024`<br>2. View page source | Uses semantic HTML5 elements: `<section>`, `<table>`, `<thead>`, `<tbody>`, `<h1>`–`<h4>`; no `<div>` soup; tables have proper headers | [ ] |
| 5.2 | No CSS framework dependencies | 1. View page source<br>2. Search for common framework class names | No Bootstrap classes (e.g., `col-`, `btn-`), no Tailwind classes (e.g., `flex`, `grid`), no Joomla template wrapper classes | [ ] |
| 5.3 | CSS class naming convention | 1. View page source<br>2. Inspect all class attributes | All classes use `cca-*` prefix: `cca-carver-header`, `cca-event-section`, `cca-special-prizes`, `cca-overall-results`, `cca-division-results`, `cca-error`, `cca-usage` | [ ] |
| 5.4 | XSS prevention (malicious input) | 1. Navigate to `?name=<script>alert(1)</script>`<br>2. Inspect page source and rendered output | Script tag is HTML-escaped in source as `&lt;script&gt;alert(1)&lt;/script&gt;` and displayed as literal text; script does NOT execute; no JavaScript alert appears | [ ] |
| 5.5 | Entry number zero/null handling | 1. View results for a carver with entry_number = 0 or null in JSON<br>2. Inspect table cells for entry numbers | Entry # column renders as empty `<td></td>` (or `<td> </td>`); no literal "0", "null", or "undefined" text displayed | [ ] |
| 5.6 | Style code display (N/P conversion) | 1. View division results table with style codes<br>2. Check Style column | "N" displays as "Natural", "P" displays as "Painted", null displays as empty cell; no raw "N" or "P" shown to users | [ ] |
| 5.7 | Ordinal place formatting | 1. View overall or division results<br>2. Check Place column | Places display as "1st", "2nd", "3rd", "4th", etc.; handles edge cases like "11th", "12th", "13th" (not "11st"); ordinal function works correctly | [ ] |

---

## Section 6: Data File Management

**Dependencies**: All previous issues

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 6.1 | Add new year dynamically | 1. Generate and upload `results-2025.json` to media/data folder (no code change)<br>2. Navigate to `?name=Jane+Doe` (cross-event lookup) | 2025 results appear immediately in the output without component reinstall or code modification; sorted as newest year first | [ ] |
| 6.2 | Multiple years coexist | 1. Ensure `results-2023.json` and `results-2024.json` both exist in media/data<br>2. Navigate to single-event URLs for each year | Both `?event=2023` and `?event=2024` work independently; files do not conflict or overwrite each other | [ ] |
| 6.3 | JSON file path structure | 1. Check media/data folder contents | Files named exactly as `results-2024.json`, `results-2023.json` (lowercase "results-", four-digit year, ".json" extension); no spaces or other naming variations | [ ] |

---

## Known Gaps / Bugs Found During Code Review

### Minor Issues

1. **Potential event validation gap (ResultsService.php:182, 281)**
   - **Location**: `ResultsService::lookupByNameAndYear()` line 182, `lookupByCarverIdAndYear()` line 281
   - **Issue**: File path constructed as `$this->dataPath . '/results-' . $event . '.json'` without validating that `$event` is positive. If a negative year somehow passes validation (e.g., `-2024`), it creates path `results--2024.json` which is valid but unintended.
   - **Impact**: Low — HtmlView validates event is alphanumeric and casts to int, but doesn't check for negative values. Unlikely in production but possible via direct service instantiation.
   - **Recommendation**: Add validation in `ResultsService::lookup()` to ensure event data file exists (or similar reasonable lower bound) before dispatching to year-specific methods. Alternatively, validate in HtmlView that event is alphanumeric.
   - **Owner**: Frodo (data layer)

2. **Subtitle construction mixing escaped/unescaped content (default.php:77)**
   - **Location**: Template file `default.php` line 77
   - **Issue**: Variable `$subtitle` is constructed by concatenating `esc($event['event_name'])` (escaped) with unescaped plain text and integer `$event['event_year']`. While safe (integers don't need escaping), the inconsistent pattern could cause issues if modified.
   - **Impact**: None currently — integers are safe and the escaped name is correctly escaped. However, maintainability concern.
   - **Recommendation**: For consistency, construct subtitle entirely with escaping: `'Results for ' . esc($event['event_name']) . ' ' . esc((string)$event['event_year'])` or build it in the echo statement directly.
   - **Owner**: Frodo (template)

### No Critical Security Issues Found

- ✅ **XSS Protection**: All user input and database content properly escaped via `esc()` helper using `htmlspecialchars()` with `ENT_QUOTES | ENT_HTML5`
- ✅ **Path Traversal**: Year used in file paths is validated as numeric before use; no arbitrary user input in file paths
- ✅ **SQL Injection**: N/A — component uses file-based data, no database queries
- ✅ **Array Access Safety**: Consistent use of `??` operator and `isset()` checks prevent undefined key warnings
- ✅ **Empty Data Handling**: All array iterations check `!empty()` before rendering to avoid empty tables

### Validation Passed

- ✅ **carver_id requires event**: Enforced in HtmlView line 152-161 with clear error message (privacy-by-design requirement met)
- ✅ **Case-insensitive name search**: Implemented via `strtolower()` comparison in `findCarverIdByName()` line 570-581
- ✅ **No empty tables**: All sections wrapped in `if (!empty(...))` checks in template lines 94, 122, 152
- ✅ **Entry number zero/null handling**: Template lines 111-113, 140-142, 182-184 check `> 0` before rendering

---

## Test Execution Notes

- **PHP Error Logging**: Before testing, enable PHP error logging in Joomla Global Configuration > System > Debug System to catch warnings/notices
- **Browser Console**: Keep developer tools console open during testing to catch JavaScript errors (though none should occur)
- **Test Order**: Sections should be tested in order 1→6 due to dependencies between issues
- **Data Cleanup**: After testing error cases (4.6), remember to restore JSON files for subsequent tests
- **Regression Testing**: After any bug fixes from this plan, re-run ALL tests to ensure no regressions

---

## Acceptance Criteria

- [ ] All 28 test cases in sections 1-6 pass
- [ ] No PHP errors, warnings, or notices logged during any test
- [ ] Component works identically on both Joomla 4.x and 5.x
- [ ] All error messages are user-friendly (no stack traces or technical jargon)
- [ ] HTML output passes W3C validation (semantic, accessible markup)
- [ ] Security review shows no XSS, path traversal, or injection vulnerabilities
