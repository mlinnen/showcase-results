# Test Data Specification

This document specifies the structure and content requirements for sample JSON test data files used to validate the Carver Results feature (Issue #7).

## Purpose

These test files enable comprehensive manual testing of:
- Cross-event name lookups (same person across multiple years)
- Single-event lookups (by name or carver_id)
- Error handling (carver not found, no results, etc.)
- Edge cases (zero entry numbers, checked-in competitors with no wins)
- Rendering of all three result types (special prizes, overall, division)

## Schema Compliance

All JSON files MUST validate against `schema/results.schema.json` (ADR-002).

Required top-level properties:
- `event`: Object with `name` (string) and `event_id` (alphanumeric string)
- `special_prizes`: Array of special prize objects
- `overall_results`: Array of overall category results
- `division_results`: Array of division results (Novice, Intermediate, Open)
- `competitors`: Array of checked-in competitors plus any result-bearing integrity rows; each entry includes `checked_in`

Refer to `schema/results.schema.json` for complete field definitions and validation rules.

---

## File 1: `results-2024.json`

### Event Metadata
```json
{
  "event": {
    "name": "CCA Showcase 2024",
    "event_id": "2024"
  }
}
```

### Required Competitors

| carver_id | first_name | last_name | Purpose |
|-----------|------------|-----------|---------|
| 16 | John | Smith | Has ALL three result types (special prizes, overall, division) |
| 23 | Jane | Doe | Appears in both 2024 and 2023 (different carver_id: 19 in 2023) |
| 8 | Bob | Wilson | Checked-in competitor with ZERO results; stays in `competitors` with `checked_in: true` because the source checked-in column says yes |
| 31 | Irene | Winner | Deliberate mismatch: has a prize/result row but source `Checked In` says no; stays in `competitors` with `checked_in: false` so Joomla lookups still work |
| 42 | Alice | Brown | Only appears in 2024 (single-year carver) |
| 5 | Charlie | Green | Only in 2024, has division results only |
| 11 | Diana | Lee | Has special prizes only |
| 27 | Eve | Martinez | Has overall results only |
| 30 | Frank | Taylor | Division results with both Natural (N) and Painted (P) styles |
| 14 | Grace | Anderson | Has entry_number = 0 in one result (tests null handling) |
| 19 | Henry | Thomas | Minimum 10 competitors for realistic volume |

**Total competitors**: At least 10 (can include more for realism)

### Special Prizes (sample)

Include at least 3 special prizes with varied data:

```json
{
  "special_prizes": [
    {
      "order": 1,
      "name": "Best of Show",
      "carver_id": 16,
      "winner": "John Smith",
      "entry_number": 42,
      "prize": "$500"
    },
    {
      "order": 2,
      "name": "People's Choice Award",
      "carver_id": 11,
      "winner": "Diana Lee",
      "entry_number": 17,
      "prize": "$100"
    },
    {
      "order": 3,
      "name": "Best Novice",
      "carver_id": 23,
      "winner": "Jane Doe",
      "entry_number": 29,
      "prize": "Ribbon + $50"
    }
  ]
}
```

**Note**: Carver 16 (John Smith) MUST have at least one special prize.

### Overall Results (sample)

Include at least 1 overall category with carver 16 placing:

```json
{
  "overall_results": [
    {
      "category": "Best of Show - Theme",
      "places": [
        {
          "place": 1,
          "carver_id": 16,
          "winner": "John Smith",
          "entry_number": 42
        },
        {
          "place": 2,
          "carver_id": 27,
          "winner": "Eve Martinez",
          "entry_number": 33
        },
        {
          "place": 3,
          "carver_id": 42,
          "winner": "Alice Brown",
          "entry_number": 8
        }
      ]
    }
  ]
}
```

**Note**: Carver 16 must appear in at least one overall result.

### Division Results (sample)

Include all three divisions (Novice, Intermediate, Open) with varied categories:

#### Novice Division
```json
{
  "division": "Novice",
  "categories": [
    {
      "name": "Birds N",
      "style": "N",
      "places": [
        {
          "place": 1,
          "carver_id": 23,
          "winner": "Jane Doe",
          "entry_number": 29
        },
        {
          "place": 2,
          "carver_id": 5,
          "winner": "Charlie Green",
          "entry_number": 15
        }
      ]
    },
    {
      "name": "Wildlife P",
      "style": "P",
      "places": [
        {
          "place": 1,
          "carver_id": 30,
          "winner": "Frank Taylor",
          "entry_number": 51
        }
      ]
    }
  ]
}
```

#### Intermediate Division
```json
{
  "division": "Intermediate",
  "categories": [
    {
      "name": "Caricature N",
      "style": "N",
      "places": [
        {
          "place": 1,
          "carver_id": 16,
          "winner": "John Smith",
          "entry_number": 42
        },
        {
          "place": 2,
          "carver_id": 42,
          "winner": "Alice Brown",
          "entry_number": 7
        },
        {
          "place": 3,
          "carver_id": 5,
          "winner": "Charlie Green",
          "entry_number": 18
        }
      ]
    }
  ]
}
```

#### Open Division
```json
{
  "division": "Open",
  "categories": [
    {
      "name": "Abstract P",
      "style": "P",
      "places": [
        {
          "place": 1,
          "carver_id": 30,
          "winner": "Frank Taylor",
          "entry_number": 52
        }
      ]
    }
  ]
}
```

**Notes**:
- Carver 16 (John Smith) MUST appear in at least one division result
- Include at least one category with style "N", one with "P", and one with null (for Best Of categories)
- Carver 30 should appear in both N and P categories to test style rendering

### Edge Case: Entry Number Zero

Include at least one result with `entry_number: 0` for carver 14 (Grace Anderson):

```json
{
  "place": 2,
  "carver_id": 14,
  "winner": "Grace Anderson",
  "entry_number": 0
}
```

This tests the template's handling of zero entry numbers (should render as empty cell).

### Validation Checklist for results-2024.json

- [ ] Event year is 2024
- [ ] Contains at least 10 competitors
- [ ] Carver 16 (John Smith) appears in special_prizes, overall_results, AND division_results
- [ ] Carver 23 (Jane Doe) has at least one result of any type
- [ ] Carver 8 (Bob Wilson) is in competitors array and has ZERO results
- [ ] Carver 42 (Alice Brown) has results in 2024 only
- [ ] At least one entry has entry_number = 0
- [ ] All three divisions (Novice, Intermediate, Open) are present
- [ ] Division results include both style "N" and style "P" categories
- [ ] File validates against schema/results.schema.json

---

## File 2: `results-2023.json`

### Event Metadata
```json
{
  "event": {
    "name": "CCA Showcase 2023",
    "event_id": "2023"
  }
}
```

### Required Competitors

| carver_id | first_name | last_name | Purpose |
|-----------|------------|-----------|---------|
| 19 | Jane | Doe | **CRITICAL**: Same person as carver 23 in 2024 (different ID, tests cross-event name lookup) |
| 7 | Charlie | Green | Only appears in 2023 (not in 2024) |
| 3 | Sam | White | 2023-only carver with division results |
| 12 | Laura | King | 2023-only carver with special prizes |
| 25 | Mike | Scott | 2023 competitor with multiple results |
| 31 | Nina | Clark | Minimum competitor count |
| 18 | Oscar | Hall | Additional volume |
| 9 | Paula | Adams | Additional volume |
| 21 | Quinn | Baker | Additional volume |
| 28 | Rachel | Wright | Minimum 10 competitors |

**Total competitors**: At least 10

**CRITICAL**: Carver 19 in 2023 has the SAME `first_name` and `last_name` as carver 23 in 2024 ("Jane Doe"). This is the key test case for cross-event name lookups where the same person receives different carver_id values in different years.

### Special Prizes (sample)

```json
{
  "special_prizes": [
    {
      "order": 1,
      "name": "Best of Show",
      "carver_id": 12,
      "winner": "Laura King",
      "entry_number": 38,
      "prize": "$500"
    },
    {
      "order": 2,
      "name": "Best Intermediate",
      "carver_id": 19,
      "winner": "Jane Doe",
      "entry_number": 22,
      "prize": "$150"
    }
  ]
}
```

**Note**: Carver 19 (Jane Doe) must have at least one special prize to ensure cross-event lookup returns results from 2023.

### Overall Results (sample)

```json
{
  "overall_results": [
    {
      "category": "Best of Show - Wildlife",
      "places": [
        {
          "place": 1,
          "carver_id": 25,
          "winner": "Mike Scott",
          "entry_number": 41
        },
        {
          "place": 2,
          "carver_id": 7,
          "winner": "Charlie Green",
          "entry_number": 13
        }
      ]
    }
  ]
}
```

### Division Results (sample)

Include all three divisions with Jane Doe (carver 19) appearing in at least one:

```json
{
  "division_results": [
    {
      "division": "Novice",
      "categories": [
        {
          "name": "Animals N",
          "style": "N",
          "places": [
            {
              "place": 1,
              "carver_id": 19,
              "winner": "Jane Doe",
              "entry_number": 22
            },
            {
              "place": 2,
              "carver_id": 3,
              "winner": "Sam White",
              "entry_number": 11
            }
          ]
        }
      ]
    },
    {
      "division": "Intermediate",
      "categories": [
        {
          "name": "Figures P",
          "style": "P",
          "places": [
            {
              "place": 1,
              "carver_id": 7,
              "winner": "Charlie Green",
              "entry_number": 14
            }
          ]
        }
      ]
    },
    {
      "division": "Open",
      "categories": [
        {
          "name": "Sculpture N",
          "style": "N",
          "places": [
            {
              "place": 1,
              "carver_id": 25,
              "winner": "Mike Scott",
              "entry_number": 40
            }
          ]
        }
      ]
    }
  ]
}
```

### Validation Checklist for results-2023.json

- [ ] Event year is 2023
- [ ] Contains at least 10 competitors
- [ ] Carver 19 is named "Jane Doe" (first_name: "Jane", last_name: "Doe")
- [ ] Carver 19 has results in at least two of three categories (special_prizes, overall_results, division_results)
- [ ] Carver 7 (Charlie Green) has results in 2023 but is NOT in 2024 (single-year test)
- [ ] All three divisions (Novice, Intermediate, Open) are present
- [ ] File validates against schema/results.schema.json

---

## Cross-File Requirements

### Critical Test Case: Jane Doe (Cross-Event Name Lookup)

**Setup**:
- `results-2024.json`: Carver 23 named "Jane Doe" with results
- `results-2023.json`: Carver 19 named "Jane Doe" with results

**Purpose**: Test that `?name=Jane+Doe` returns results from BOTH events, demonstrating that name-based lookup correctly aggregates across events despite different carver_id values.

**Expected Behavior**:
- Navigate to `?name=Jane+Doe`
- Page displays two `<section class="cca-event-section">` elements
- First section: "CCA Showcase 2024" with carver 23's results
- Second section: "CCA Showcase 2023" with carver 19's results
- Sections sorted year descending (2024 before 2023)

### Contrast Test Case: Single-Year Carvers

**Setup**:
- `results-2024.json`: Carver 42 (Alice Brown) — only in 2024
- `results-2023.json`: Carver 7 (Charlie Green) — only in 2023

**Purpose**: Verify that single-event carvers return only one section in cross-event lookups.

**Expected Behavior**:
- `?name=Alice+Brown` returns only 2024 section
- `?name=Charlie+Green` returns only 2023 section

---

## Generation Commands

### Using the CLI Tool

Assuming you have real spreadsheet data for 2023 and 2024:

```bash
# Generate 2024 JSON
showcase-results create results --input data/2024.xlsx --format json --output output/results-2024.html

# Generate 2023 JSON
showcase-results create results --input data/2023.xlsx --format json --output output/results-2023.html

# Validate schemas
ajv validate -s schema/results.schema.json -d output/results-2024.json
ajv validate -s schema/results.schema.json -d output/results-2023.json

# Upload to Joomla
# Copy output/results-2024.json to media/com_showcaseresults/data/results-2024.json
# Copy output/results-2023.json to media/com_showcaseresults/data/results-2023.json
```

### Manual JSON Creation

If creating test data manually, use the schema definitions and samples above as templates. Key points:

- Ensure all `carver_id` values in results arrays exist in the `competitors` array
- Use `checked_in` to decide public list membership; do not drop a result-bearing competitor from `competitors` just because their source check-in value is false
- Use realistic but fake names (or real names if you have permission)
- Include variety: some carvers with 1 result, some with 10+ results
- Test edge cases: entry_number 0, style null, single-place categories
- Keep event names consistent with your organization's branding

---

## Data Upload and Maintenance

### Installation Path

On Joomla server:
```
<joomla-root>/media/com_showcaseresults/data/
```

Example full path:
```
/var/www/html/joomla/media/com_showcaseresults/data/results-2024.json
/var/www/html/joomla/media/com_showcaseresults/data/results-2023.json
```

### File Permissions

Ensure files are readable by the web server:
```bash
chmod 644 media/com_showcaseresults/data/results-*.json
chown www-data:www-data media/com_showcaseresults/data/results-*.json
```

### Adding New Years

To test dynamic year addition (Test Case 6.1):
1. Generate `results-2025.json` following the same schema
2. Upload to `media/com_showcaseresults/data/`
3. No code changes or component reinstallation required
4. Verify `?name=Jane+Doe` immediately includes 2025 results (if Jane Doe exists in that year's data)

---

## Appendix: Quick Reference Schema Summary

**Place Entry Object** (used in all result types):
```json
{
  "place": 1,           // 1-3
  "carver_id": 16,      // Integer, matches competitors array
  "winner": "John Doe", // First + Last name
  "entry_number": 42    // Integer ≥ 1 (or 0 for edge case testing)
}
```

**Style Codes**:
- `"N"`: Natural
- `"P"`: Painted
- `null`: Best Of categories (no style distinction)

**Division Names** (enum):
- `"Novice"`
- `"Intermediate"`
- `"Open"`

**Required Field Presence**:
- All top-level arrays (special_prizes, overall_results, division_results, competitors) must exist, even if empty
- `event.name` and `event.event_id` are required
- All place objects must have `place`, `carver_id`, `winner`, `entry_number` (even if 0)

**Recommended Data Volume**:
- Minimum 10 competitors per file (realistic context)
- 3-5 special prizes per event
- 1-2 overall categories
- 5-10 division categories across all divisions
- Total result count: 20-30 place entries per file
