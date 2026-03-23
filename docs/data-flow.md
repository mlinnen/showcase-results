# Data Flow — showcase-results

## Pipeline Overview

```
data/input/prizes.xlsx       ──┐
                                ├─► src/parse/index.ts  ─►  data/output/results.json
data/input/competitors.xlsx  ──┘           │                          │
            (Bilbo)                         │                          ▼
                                            │               src/render/article.ts
                                            │                          │
                                            │                          ▼
                                            │                  output/article.html
                                            │                   (Frodo)
                                            │
                                     tests/*.test.ts
                                       (Aragorn)
```

## Stages

### Stage 1 — Parse (Bilbo)
- **Input:** `data/input/prizes.xlsx`, `data/input/competitors.xlsx`
- **Output:** `data/output/results.json`
- **Validation:** output must conform to `schema/results.schema.json`
- **Cross-check:** every winner name in the prizes sheet must appear in the competitors sheet

### Stage 2 — Render (Frodo)
- **Input:** `data/output/results.json`
- **Output:** `output/article.html`
- **Constraints:**
  - Joomla article body fragment only (no `<html>`, `<head>`, `<body>`)
  - Semantic HTML: `<h2>` for category headings, `<table>` for results
  - No inline `style=""` attributes
  - No `<script>` tags

### Stage 3 — Validate (Aragorn)
- Schema validation of `results.json` against `schema/results.schema.json`
- Cross-sheet integrity: prize winners ↔ competitor names
- HTML structural checks: well-formed, no disallowed tags
- Joomla-safe attribute checks

## Interface Contract

The **schema** (`schema/results.schema.json`) is the hard contract between Bilbo and Frodo.
- Bilbo must not change the schema without Gandalf approval
- Frodo must not read spreadsheets directly
- Any schema change triggers a re-run of all tests

## File Ownership

| Path                         | Owner   |
|------------------------------|---------|
| `data/input/*.xlsx`          | User    |
| `src/parse/`                 | Bilbo   |
| `schema/results.schema.json` | Gandalf |
| `data/output/results.json`   | Bilbo   |
| `src/render/`                | Frodo   |
| `output/article.html`        | Frodo   |
| `tests/`                     | Aragorn |
| `docs/`                      | Gandalf |
