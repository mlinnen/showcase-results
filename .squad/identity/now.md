---
updated_at: 2026-03-23T02:33:36Z
focus_area: Carving event results — Joomla article pipeline
active_issues: []
---

# What We're Focused On

Building a pipeline from four carving-event spreadsheets to a Joomla article HTML fragment.

## Current Status

- ✅ Architecture designed by Gandalf (ADR-001)
- ✅ Folder structure created (`data/input/`, `data/output/`, `schema/`, `src/`, `output/`)
- ✅ Bilbo — analyzed all four spreadsheets; documented real data model
- ✅ Bilbo — updated data documentation with actual schemas
- ✅ **Gandalf — schema approval (ADR-002):** Full rewrite approved. Ribbon concept removed. Real data model implemented.
- ✅ **Bilbo — parser complete:** `src/parse/index.js` built and executed. `data/output/results.json` generated and validated against schema.
- ⏳ Frodo — build `output/article.html` from `results.json` (unblocked, ready to start)
- ⏳ Aragorn — validate JSON and HTML (unblocked after Frodo completes)

## Pipeline

```
data/input/Categories.xlsx  ─┐
data/input/Competitor.xlsx   ├─► Bilbo ✅ ─► data/output/results.json ✅ ─► Frodo ⏳ ─► output/article.html
data/input/Judging.xlsx      │                                                    ► Aragorn (unblocked)
data/input/Prizes.xlsx      ─┘
```

## Key Data Facts

- 37 competitors, 33 special prizes, 2 overall results, 3 divisions (Intermediate, Novice, Open)
- Judging rows: 266 total, 144 with results
- 1st/2nd/3rd place rankings — no ribbon concept
- Carver identity: ID embedded as leading integer in "ID FirstName LastName" strings
- Category names have trailing whitespace requiring trimming

## Data Quality Notes

- **Carver 16 (Erik Mitchell):** Novice division, category "21 Busts", style N, 1st place has entry_number = 0. Win omitted from output. Source data correction recommended.

