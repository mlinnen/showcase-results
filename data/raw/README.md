# Raw Input Spreadsheets

> **Note:** Source spreadsheets are in `data/input/`, not `data/raw/`. This directory was planned during project bootstrap but superseded. The documentation below reflects the **actual** files found in `data/input/`.

---

## Actual Files in `data/input/`

| File | Sheet | Rows | Description |
|------|-------|------|-------------|
| `Categories.xlsx` | `Categories` | 51 | Competition category definitions |
| `Competitor.xlsx` | `Competitors` | 37 | Competitor registration records |
| `Judging.xlsx` | `Judging` | 266 | Judging results — places by division/category/style |
| `Prizes.xlsx` | `Prizes` | 33 | Named awards (Best of Show, Best of Division, etc.) |

---

## Columns: `Categories.xlsx` (sheet: `Categories`)

> First row is a merged header "Categories"; row 2 is the column header row.

| Column | Type | Notes |
|--------|------|-------|
| `Name` | string | Category name. Numbered entries prefixed e.g. "01 Show Theme". May have trailing whitespace. |
| `Description` | string\|null | Human-readable description of the category |
| `N` | boolean string | "True"/"False" — whether Novice division uses this category |
| `P` | boolean string | "True"/"False" — whether the Professional/Open style applies |
| `Div Specific` | boolean string | "True"/"False" — whether the category is division-specific |
| `Positions` | integer string | "1", "2", or "3" — how many places are awarded |
| `Related` | string\|null | For "Best Of" categories: name of the linked entry category |

Top rows are "Best Of" aggregate categories; remaining rows (starting "01 …") are numbered entry categories.

---

## Columns: `Competitor.xlsx` (sheet: `Competitors`)

> First row is a merged header "Competitors"; row 2 is the column header row.

| Column | Type | Notes |
|--------|------|-------|
| `Carver ID` | integer | Primary key — referenced in Judging and Prizes as leading integer of "ID Name" strings |
| `First Name` | string | |
| `Last Name` | string | |
| `Email` | string | PII — omit from output |
| `Phone` | string | PII — omit from output |
| `Pay By` | string | "Online Payment" \| "Check" \| "Cash" — omit from output |
| `Entry Fee` | decimal | Payment data — omit from output |
| `Club Fee` | decimal | Payment data — omit from output |
| `Donation` | decimal | Payment data — omit from output |
| `Total Due` | decimal | Payment data — omit from output |
| `Total Paid` | decimal | Payment data — omit from output |
| `Notes` | string\|null | Admin notes — omit from output |

---

## Columns: `Judging.xlsx` (sheet: `Judging`)

> First row is a merged header "Judging"; row 2 is the column header row.

| Column | Type | Notes |
|--------|------|-------|
| `Division` | string\|null | "Intermediate" \| "Novice" \| "Open"; null for overall categories |
| `Category` | string | References Categories.Name. Trim trailing whitespace. |
| `Style` | string\|null | "N" \| "P"; null for special/best-of categories |
| `Team` | string | Judging team ("Team A"–"Team D") |
| `Positions` | integer string | How many places awarded for this row |
| `1st` | string\|null | Winner: "ID FirstName LastName" format |
| `#` | integer string\|null | Entry number for 1st place |
| `Prize` | string\|null | Prize value or description for 1st place |
| `2nd` | string\|null | Runner-up: "ID FirstName LastName" format |
| `#.1` | integer string\|null | Entry number for 2nd place |
| `Prize.1` | string\|null | Prize for 2nd place |
| `3rd` | string\|null | Third place: "ID FirstName LastName" format |
| `#.2` | integer string\|null | Entry number for 3rd place |
| `Prize.2` | string\|null | Prize for 3rd place |

Rows where 1st/2nd/3rd are all null represent categories with no entries — omit from output.

---

## Columns: `Prizes.xlsx` (sheet: `Prizes`)

> First row is a merged header "Prizes"; row 2 is the column header row.

| Column | Type | Notes |
|--------|------|-------|
| `Name` | string | Award name (e.g. "Best of Show", "Best of Open - Runner up") |
| `Assigned` | boolean string | Always "True" in observed data |
| `Order` | integer string | Display sort order; jumps from 29 → 96 for special/non-monetary awards |
| `Carver` | string | Winner: "ID FirstName LastName" format |
| `Entry #` | integer string | Entry number |
| `Prize` | string | Dollar amount (e.g. "450") or descriptive text (e.g. "Lee S. Dukes Memorial Award…") |

---

## Relationships Between Sheets

```
Competitor.xlsx  Carver ID  ──┐
                               │  (embedded as leading integer in "ID Name" strings)
Judging.xlsx     1st/2nd/3rd ──┤
Prizes.xlsx      Carver     ──┘

Judging.xlsx     Category   ──►  Categories.xlsx  Name
Prizes.xlsx      Name       ──►  (derived from Categories.xlsx  Name — not a hard FK)
```

- **Carver identity** is denormalized into Judging and Prizes as `"ID FirstName LastName"`. Parser must split to recover `carver_id`.
- **Category names** in Judging reference Categories.Name — trailing whitespace must be trimmed for matching.
