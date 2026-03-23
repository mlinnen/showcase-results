# Processed Data — JSON Output

This folder contains Bilbo's normalized output from the source spreadsheets in `data/input/`. These files are the **input for Frodo** when building the Joomla article.

> **Note:** Source spreadsheets live in `data/input/`, not `data/raw/`. The `data/raw/` directory was planned but superseded by `data/input/`. See `.squad/decisions/inbox/bilbo-data-model.md`.

---

## Files

| File | Description | Owner |
|------|-------------|-------|
| `results.json` | Single normalized output (ADR-001 contract with Frodo) | Bilbo |
| `NOTES.md` | Data-cleaning log and column mapping decisions | Bilbo |

> `results.json` must validate against `schema/results.schema.json`.

---

## Schema: `results.json`

The full structure Frodo consumes. All field names are exact. Required fields marked *.

```json
{
  "event": {
    "name": "...",     // * string — event name (supplied at parse time; not in spreadsheets)
    "year": 2025       // * integer — event year (supplied at parse time; not in spreadsheets)
  },

  "special_prizes": [
    {
      "order":        1,              // * integer — display sort order (Prizes.Order)
      "name":         "Best of Show", // * string  — award name (Prizes.Name)
      "carver_id":    4,              // * integer — leading integer parsed from Prizes.Carver
      "winner":       "Neil McGuire", // * string  — name portion parsed from Prizes.Carver
      "entry_number": 37,             // * integer — Prizes."Entry #"
      "prize":        "450"           // * string  — dollar amount or description (Prizes.Prize)
    }
  ],

  "overall_results": [
    {
      "category": "Best of Show",     // * string — Judging.Category where Division is null
      "places": [
        {
          "place":        1,              // * integer — 1, 2, or 3
          "carver_id":    4,              // * integer — leading integer parsed from Judging.1st
          "winner":       "Neil McGuire", // * string  — name parsed from Judging.1st
          "entry_number": 37              // * integer — Judging.#
        }
      ]
    }
  ],

  "division_results": [
    {
      "division": "Intermediate",     // * string — "Intermediate" | "Novice" | "Open"
      "categories": [
        {
          "name":  "01 Show Theme",   // * string — Judging.Category (trimmed)
          "style": null,              // string|null — "N" | "P" | null (Judging.Style)
          "places": [
            {
              "place":        1,
              "carver_id":    31,
              "winner":       "Lou Glass",
              "entry_number": 1
            }
          ]
        }
      ]
    }
  ],

  "competitors": [
    {
      "carver_id":  1,           // * integer — Competitor."Carver ID"
      "first_name": "Mike",      // * string  — Competitor."First Name"
      "last_name":  "Linnen"     // * string  — Competitor."Last Name"
    }
  ]
}
```

---

## Source → Output Field Map

### `special_prizes` ← `Prizes.xlsx`

| `results.json` field | Source column | Notes |
|----------------------|--------------|-------|
| `order` | `Order` | Integer display sort order |
| `name` | `Name` | Award name string |
| `carver_id` | `Carver` | Parse leading integer from "ID FirstName LastName" |
| `winner` | `Carver` | Parse name portion from "ID FirstName LastName" |
| `entry_number` | `Entry #` | Integer |
| `prize` | `Prize` | Dollar amount (as string) or descriptive text |

### `overall_results` + `division_results` ← `Judging.xlsx`

| `results.json` field | Source column | Notes |
|----------------------|--------------|-------|
| `division` | `Division` | null rows → `overall_results`; non-null → `division_results` |
| `category` | `Category` | Trim trailing whitespace |
| `style` | `Style` | "N" or "P"; null for special/best-of categories |
| `place` | _(column position)_ | `1st`→1, `2nd`→2, `3rd`→3 |
| `carver_id` | `1st` / `2nd` / `3rd` | Parse leading integer from "ID Name" string |
| `winner` | `1st` / `2nd` / `3rd` | Parse name portion from "ID Name" string |
| `entry_number` | `#` / `#.1` / `#.2` | Integer |

### `competitors` ← `Competitor.xlsx`

| `results.json` field | Source column | Notes |
|----------------------|--------------|-------|
| `carver_id` | `Carver ID` | Integer PK |
| `first_name` | `First Name` | String |
| `last_name` | `Last Name` | String |

> Columns `Email`, `Phone`, `Pay By`, `Entry Fee`, `Club Fee`, `Donation`, `Total Due`, `Total Paid`, `Notes` are **omitted** from output (PII / payment data).

### `Categories.xlsx` — reference only

Not emitted directly into `results.json`. Used to validate category names during parsing. See `.squad/decisions/inbox/bilbo-data-model.md` for full column reference.

---

## Parsing Notes

- Carver identity is encoded in Judging and Prizes as `"ID FirstName LastName"` (e.g. `"4 Neil McGuire"`). Split on first space to extract `carver_id`.
- Category names in Judging/Categories often have **trailing whitespace** — always trim.
- `Prize` in Prizes.xlsx can be a dollar integer (`"450"`) or a description (`"Lee S. Dukes Memorial Award Commemorative Plaque"`). Emit as string.
- Prizes `Order` jumps from 29 → 96–99. Non-monetary special awards appear at the end.
- Judging rows with all-null place columns (no entries submitted) should be **omitted** from output.
- `Style` is null for "Best Of" and overall categories; only "N"/"P" for numbered entry categories.
- Schema changes to `results.json` require Gandalf approval (ADR-001).
