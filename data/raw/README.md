# Raw Input Spreadsheets

Place the source spreadsheets here before running the pipeline. Accepted formats: `.xlsx` or `.csv`.

## Expected Files

| File | Description |
|------|-------------|
| `prizes.xlsx` | Lists every prize and the carver who won it |
| `competitors.xlsx` | Lists every competitor and the ribbon they received |

---

## Expected Columns: `prizes.xlsx`

> **Bilbo:** Update this table with actual column names after parsing.

| Expected field | Likely column name(s) | Notes |
|---------------|----------------------|-------|
| Prize name | `Prize`, `Award`, `Prize Name` | Required |
| Category | `Category`, `Class`, `Division` | Optional — leave blank if not present |
| Carver name | `Carver`, `Winner`, `Name` | Required |

---

## Expected Columns: `competitors.xlsx`

> **Bilbo:** Update this table with actual column names after parsing.

| Expected field | Likely column name(s) | Notes |
|---------------|----------------------|-------|
| Carver name | `Carver`, `Name`, `Entrant` | Required |
| Entry title | `Entry`, `Title`, `Piece Name` | Optional |
| Category | `Category`, `Class`, `Division` | Required |
| Ribbon | `Ribbon`, `Award`, `Place` | Required — expected values: Blue, Red, White, Participant (or equivalent) |

---

## Notes

- Bilbo will normalize whitespace and capitalization during parsing
- Any data anomalies will be logged in `../processed/NOTES.md`
