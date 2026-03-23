# Bilbo — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Data Engineer
- **Joined:** 2026-03-23T01:57:14.727Z

## Learnings

<!-- Append learnings below -->

## 2026-03-23 — Spreadsheet analysis & data model

Analyzed all four source spreadsheets in `data/input/`:
- `Categories.xlsx` (51 rows) — competition category definitions with flags and relationships
- `Competitor.xlsx` (37 rows) — carver registration records; PII/payment columns excluded from output
- `Judging.xlsx` (266 rows) — full judging results; winner encoded as "ID Name" string; ~122 empty rows
- `Prizes.xlsx` (33 rows) — 33 named awards; Prize column is mixed integer/string

Key findings: carver identity is denormalized across sheets as "ID FirstName LastName" strings; category names have trailing whitespace; the existing schema doesn't match the actual data structure (no ribbons — actual data uses 1st/2nd/3rd place).

Deliverables:
- Updated `data/processed/README.md` with corrected JSON schema for Frodo
- Updated `data/raw/README.md` with actual column documentation
- Wrote `.squad/decisions/inbox/bilbo-data-model.md` — recommends revised `results.json` structure; flagged for Gandalf approval
- Updated `.squad/identity/now.md`

Next: pending Gandalf schema approval → build parser in `src/parse/`

## 2026-03-23 — Parser built and verified

Built `src/parse/index.js` (plain JS, runs with `node src/parse/index.js`) and `src/parse/index.ts` (TypeScript equivalent for future compile step). Created `package.json` with `xlsx` and `ajv` dependencies.

Parser produces `data/output/results.json` that validates against `schema/results.schema.json`:
- 33 special prizes (sorted by order)
- 2 overall result categories (Best of Show + Overall Best of Show Theme)
- 3 divisions (Intermediate, Novice, Open) with full category/style/places breakdown
- 37 competitors

Key implementation notes:
- `xlsx` library renames duplicate Judging.xlsx columns: `#` (2nd) → `#_1`, `#` (3rd) → `#_2` (same for Prize)
- Row 1 (0-indexed) in all sheets is the merged title header; `{range: 1}` skips it correctly
- `{defval: null}` required so missing cells come through as null, not undefined
- AJV v8 exposes constructor as `Ajv.default` in CommonJS require context

Data quality issue discovered: 10 rows in Judging.xlsx have entry_number = 0. Of these, 9 have null carver names (phantom zeros — never emitted). 1 row (Novice "21 Busts N", carver 16 Erik Mitchell) has a real winner with entry_number = 0 — that place entry is dropped with a warning. See `.squad/decisions/inbox/bilbo-entry-number-zeros.md`.

