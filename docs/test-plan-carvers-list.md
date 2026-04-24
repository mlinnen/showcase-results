# Test Plan: Carvers List View (Issue #22)

## Overview

This test plan validates the **Carvers List View** for the com_showcaseresults Joomla component. The view displays checked-in carvers from a given event year in a filterable list showing Carver ID, Carver Name (linked to detail view), and Division. The data is provided by `ResultsService::getCarversList(string $event)` and supports filtering by event query parameter (`?event=2024`).

## Prerequisites

- Joomla 4.x or 5.x test instance running
- Carvers List view component installed
- Test data JSON files: `results-2024.json`, `results-2023.json`, `results-2025.json` in `media/com_showcaseresults/data/`
- Test data should include multiple carvers, divisions, and edge cases (per [Test Data Requirements](#test-data-requirements))
- Access to Joomla component configuration
- Browser with developer tools

## Test Data Requirements

### `results-2024.json`
- **Minimum 15 competitors** across multiple divisions (Novice, Intermediate, Advanced, Professional)
- **Test carvers:**
  - Carver 1: "Alice Brown" (Novice)
  - Carver 2: "Bob Smith" (Intermediate) — to test ordering (last name "Smith" comes after "Brown")
  - Carver 3: "Charlie Adams" (Advanced) — to test ordering (first name "Adams" comes before "Brown")
  - Carver 4: "D. Brown" (Professional) — carver with initial only
  - Carver 5: "Edward <b>Tagged</b>" (Novice) — test XSS escaping with HTML tags
  - Carver 6: "Frank O'Brien" (Intermediate) — test special characters in name
  - Multiple checked-in source registrations with no results (no assigned prize or ranked placement) to verify they still appear in the generated JSON
- **Note:** The generated `competitors` array should already contain only rows marked checked in by the competitor spreadsheet

### `results-2023.json`
- **Minimum 10 competitors** with at least 3 from different divisions
- **Carver 1:** "Alice Brown" (Novice) — same name as 2024, different carver_id (cross-year testing)
- **Carver 7:** "Grace Jackson" (Intermediate) — only in 2023

### `results-2025.json`
- **Minimum 8 competitors**
- For testing multiple years available selector

---

## Test Cases

### 1. Happy Path — Event with Data, Checked-In Carvers Shown

**Input:**
- Navigate to carvers list view with query parameter: `?event=2024`

**Expected Output:**
- Page displays a list/table of checked-in carvers from 2024
- Each row shows:
  - Carver ID (e.g., "1")
  - Carver Name as a clickable link (e.g., "Alice Brown")
  - Division (e.g., "Novice")
- Minimum 15 carvers visible (from test data)
- No error messages shown

**Pass Criteria:**
- [ ] All test carvers (1–6, plus others) appear in the list
- [ ] List contains exactly the count of carvers in `results-2024.json`'s `competitors` array
- [ ] All links are present and functional

---

### 2. No Event Parameter — Event Selector Shown

**Input:**
- Navigate to carvers list view without event parameter (no `?event=...`)

**Expected Output:**
- Page displays a event selector/dropdown or clickable list of available years
- Shows all years with available data: 2023, 2024, 2025
- Each year is a clickable link or selectable option

**Pass Criteria:**
- [ ] Selector displays exactly 3 years (2023, 2024, 2025)
- [ ] Years are in descending order (newest first: 2025, 2024, 2023)
- [ ] Clicking/selecting a year navigates to carvers list for that year
- [ ] No error message shown

---

### 3. Invalid Event (Non-Numeric)

**Input:**
- Navigate to carvers list view with invalid event: `?event=abc`

**Expected Output:**
- Error message displayed: "Event must be a valid number." (or similar user-friendly message)
- event selector shown as fallback
- No PHP errors or warnings in Joomla error log

**Pass Criteria:**
- [ ] Non-numeric input is rejected
- [ ] Friendly error message shown
- [ ] No stack traces or technical jargon
- [ ] No PHP notices/warnings logged

---

### 4. Valid Event but No JSON File Exists

**Input:**
- Navigate to carvers list view with a valid event that has no data: `?event=2022`

**Expected Output:**
- Error message: "No data available for 2022. Available events: 2025, 2024, 2023." (lists actual available years)
- event selector shown with available years
- Users can click to select a year with data

**Pass Criteria:**
- [ ] Available years list is correct and in descending order
- [ ] Message is user-friendly and helpful
- [ ] event selector links work

---

### 5. Carvers Sorted by Last Name Then First Name

**Input:**
- Navigate to `?event=2024`
- Examine the list order

**Expected Output:**
- List is sorted alphabetically:
  1. Charlie Adams (first_name: Charlie, last_name: Adams)
  2. Alice Brown (first_name: Alice, last_name: Brown)
  3. D. Brown (first_name: D., last_name: Brown)
  4. Frank O'Brien (first_name: Frank, last_name: O'Brien)
  5. Bob Smith (first_name: Bob, last_name: Smith)
  6. Edward <b>Tagged</b> (first_name: Edward, last_name: Tagged)
- Sorting is by last_name ASC, then first_name ASC

**Pass Criteria:**
- [ ] All carvers appear in correct alphabetical order by last_name
- [ ] Within same last_name, ordered by first_name (e.g., Alice before D. in "Brown")
- [ ] Sorting is case-insensitive

---

### 6. Checked-In Carver with No Results Still Appears

**Input:**
- Navigate to `?event=2024`
- Compare the generated JSON against the source spreadsheet and confirm rows marked checked in are preserved upstream even when they have no prize/result rows

**Expected Output:**
- Checked-in carvers with no results still appear in the list
- Only carvers present in the JSON `competitors` array appear
- No placeholder row or warning is shown for checked-in competitors who simply have no placements

**Pass Criteria:**
- [ ] Checked-in carvers with no results are included in the list
- [ ] Visible carver count matches the `competitors` array length in the JSON file
- [ ] No placeholder or warning row appears for checked-in competitors with no placements

---

### 7. Division Shown Correctly for Each Carver

**Input:**
- Navigate to `?event=2024`
- Inspect division column for each test carver

**Expected Output:**
- Carver 1 (Alice Brown) shows Division: "Novice"
- Carver 2 (Bob Smith) shows Division: "Intermediate"
- Carver 3 (Charlie Adams) shows Division: "Advanced"
- Carver 4 (D. Brown) shows Division: "Professional"
- Division text matches the carver's division in JSON

**Pass Criteria:**
- [ ] Division column displays correct division for each carver
- [ ] All division values are populated (no blanks where division exists)
- [ ] Division text is exact match to JSON value

---

### 8. Carver Name Link Points to Correct Detail View URL

**Input:**
- Navigate to `?event=2024`
- Inspect href of Carver Name links, e.g., "Alice Brown"

**Expected Output:**
- Link format: `?view=carver&carver_id=1&event=2024` (for Carver 1)
- Link format: `?view=carver&carver_id=2&event=2024` (for Carver 2)
- Each carver's link includes correct carver_id and year

**Pass Criteria:**
- [ ] All carver name links follow format: `?view=carver&carver_id=X&event=2024`
- [ ] carver_id in URL matches the Carver ID shown in list
- [ ] event in URL matches the current event being viewed
- [ ] Clicking a link navigates to carver detail view

---

### 9. HTML Escaping of Carver Names (XSS Prevention)

**Input:**
- Navigate to `?event=2024`
- Look for Carver 5: "Edward <b>Tagged</b>"
- View page source (Ctrl+U or View > Source)

**Expected Output:**
- In page source, carver name appears as: `Edward &lt;b&gt;Tagged&lt;/b&gt;`
- In rendered view, displays literally as: `Edward <b>Tagged</b>` (not bold, tags shown as text)
- No `<script>alert(1)</script>` or any HTML tags execute
- No JavaScript errors in browser console

**Pass Criteria:**
- [ ] HTML tags in carver names are escaped (`<` becomes `&lt;`, `>` becomes `&gt;`)
- [ ] Malicious script tags do NOT execute
- [ ] Page displays safely without errors
- [ ] All special characters in names (apostrophes, brackets) are properly escaped

---

### 10. Multiple Years Available — Links Work for Each Year

**Input:**
- Navigate to carvers list with no event parameter
- Click event selector for 2023
- Then click event selector for 2024
- Then click event selector for 2025

**Expected Output:**
- Navigating to `?event=2023` shows all carvers from 2023 (including Grace Jackson)
- Navigating to `?event=2024` shows all carvers from 2024 (including Alice Brown, Bob Smith, etc.)
- Navigating to `?event=2025` shows all carvers from 2025
- Each year's list is independent and shows only that year's carvers

**Pass Criteria:**
- [ ] All three year links work correctly
- [ ] Each year displays correct set of carvers
- [ ] event selector is always available/accessible
- [ ] URL updates to reflect selected year
- [ ] No carvers from other years mixed into current year's list

---

## Edge Cases and Boundary Tests

### 11. Empty JSON File (No Competitors)

**Setup:** Create `results-2026.json` with valid structure but empty competitors array.

**Input:**
- Navigate to `?event=2026`

**Expected Output:**
- Message: "No competitors found for 2026." (or similar)
- event selector shown as fallback
- No errors or crashes

**Pass Criteria:**
- [ ] Graceful handling of no data
- [ ] User-friendly message
- [ ] Application does not crash

---

### 12. Negative Event (Input Validation)

**Input:**
- Navigate to `?event=-2024`

**Expected Output:**
- Error: "Event must be a valid number." or "Invalid event."
- event selector shown

**Pass Criteria:**
- [ ] Negative event values rejected
- [ ] Error message shown

---

### 13. Event as Text (Input Validation)

**Input:**
- Navigate to `?event=twenty-twenty-four`

**Expected Output:**
- Error: "Event must be a valid number."
- event selector shown

**Pass Criteria:**
- [ ] Non-alphanumeric event values rejected
- [ ] User-friendly error message

---

### 14. Very Large Event (Boundary Test)

**Input:**
- Navigate to `?event=999999`

**Expected Output:**
- Error: "No data available for 999999. Available events: 2025, 2024, 2023." (since file doesn't exist)
- OR accepted if component allows arbitrary large years (year as integer is valid)

**Pass Criteria:**
- [ ] Large numbers don't cause errors or crashes
- [ ] Appropriate error shown if no data exists

---

### 15. Duplicate Carver Names Across Years

**Input:**
- Navigate to `?event=2024` and find Alice Brown (Carver 1)
- Navigate to `?event=2023` and find Alice Brown (different Carver ID)

**Expected Output:**
- 2024 Alice Brown links to `?view=carver&carver_id=1&event=2024`
- 2023 Alice Brown links to `?view=carver&carver_id=1&event=2023` (same ID in both, or different if test data differs)
- Each year's Alice Brown displays only in that year's list
- No mixing or confusion

**Pass Criteria:**
- [ ] Same names in different years are treated as separate entries (carver_id is per-year)
- [ ] Links go to correct year
- [ ] No data collision or cross-year corruption

---

### 16. Special Characters in Division Name

**Input:**
- Navigate to `?event=2024`
- Check division column for special characters if test data includes them (e.g., "Youth & Beginner", "Advanced/Master")

**Expected Output:**
- Special characters are properly escaped and displayed
- Ampersands appear as `&amp;` in source, display as `&` in rendered view
- Slashes and other chars display correctly

**Pass Criteria:**
- [ ] Division names with special characters display correctly
- [ ] No HTML escaping issues

---

### 17. Very Long Carver Name

**Input:**
- Navigate to `?event=2024`
- If test data includes a long name, verify layout

**Expected Output:**
- Long name does not break page layout
- Text either wraps or is truncated gracefully
- Link still functional
- Division column visible and readable

**Pass Criteria:**
- [ ] Long names don't cause layout breaking
- [ ] Page remains usable and readable

---

### 18. No Parameters, No Data Files Exist

**Setup:** Temporarily remove all JSON files from `media/com_showcaseresults/data/`.

**Input:**
- Navigate to carvers list view with no parameters

**Expected Output:**
- Error message: "No competition data is currently available. Please check back later." (or similar)
- No event selector shown (no years to select)
- No technical errors or stack traces

**Pass Criteria:**
- [ ] Graceful error handling when no data exists
- [ ] No PHP warnings or errors logged
- [ ] User-friendly message

---

### 19. Filtering/Pagination (if implemented)

**Input:**
- If view supports filters (e.g., division filter), test filtering by division
- If view supports pagination, test multiple pages

**Expected Output:**
- Filters work correctly and show only filtered results
- Pagination shows correct page count and navigation
- Sorting remains correct after filtering

**Pass Criteria:**
- [ ] Filters (if implemented) work correctly
- [ ] Pagination (if implemented) works correctly
- [ ] All results still sorted correctly

---

## Acceptance Criteria

- [ ] All 10 primary test cases (1–10) pass
- [ ] All 9 edge case tests (11–19) pass or are marked N/A if feature not in scope
- [ ] No PHP errors, warnings, or notices logged
- [ ] No JavaScript errors in browser console
- [ ] XSS protection verified (HTML escaping works)
- [ ] Carvers sorted correctly by last name, then first name
- [ ] Event selector appears when no event parameter provided
- [ ] Error handling is user-friendly and informative
- [ ] Only checked-in carvers appear in list
- [ ] Links to carver detail view are correct

---

## Testing Notes

### Test Execution Order
1. Execute test 2 first (event selector) to verify data setup
2. Execute tests 1, 5–10 for main functionality (with `?event=2024`)
3. Execute test 3–4 for error handling
4. Execute test 10 for multi-event navigation
5. Execute edge cases (11–19) as applicable

### Browser Compatibility
- Test on Chrome, Firefox, Safari (if available)
- Verify page renders correctly on desktop and mobile views
- Check responsive behavior if layout includes tables/lists

### Data Cleanup
- After test 18, restore JSON files for subsequent testing
- Do not modify test JSON files during test runs
- Backup test data before each test session

### Regression Testing
- After any bug fixes related to carvers list view, re-run ALL tests
- Verify no regressions in carver detail view (test a detail link from list)
- Verify no regressions in multi-carver lookups

---

## Known Limitations / Out of Scope

- Performance testing (large datasets > 1000 carvers) — not in initial test scope
- Internationalization (multiple languages) — test only English names
- Print layout — basic screen rendering only
- Accessibility (WCAG compliance) — documented separately if required
- API/programmatic access (if view doesn't expose API) — UI testing only
