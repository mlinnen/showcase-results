---
updated_at: 2025-01-01T00:00:00.000Z
focus_area: Carving event results — Joomla article pipeline
active_issues: []
---

# What We're Focused On

Building a pipeline from two carving-event spreadsheets to a Joomla article HTML fragment.

## Current Status

- ✅ Architecture designed by Gandalf (`decisions/inbox/gandalf-project-kickoff.md`)
- ✅ Folder structure created (`data/raw/`, `data/processed/`, `output/`)
- ⏳ Waiting on user to drop spreadsheets into `data/raw/`
- ⏳ Bilbo — parse spreadsheets → `data/processed/prizes.json` + `competitors.json`
- ⏳ Frodo — build `output/article.html` from processed JSON
- ⏳ Aragorn — validate JSON and HTML

## Pipeline

```
data/raw/prizes.xlsx        ─┐
data/raw/competitors.xlsx    ├─► Bilbo ─► data/processed/*.json ─► Frodo ─► output/article.html
                             └─────────────────────────────────────────────► Aragorn (validates)
```
