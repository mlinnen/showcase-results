# Squad Decisions

## Active Decisions

### ADR-001 — Three-stage pipeline (2026-03-23, Gandalf)
Spreadsheets → results.json (Bilbo) → article.html (Frodo). JSON Schema in schema/results.schema.json is the hard contract between stages.

### ADR-002 — Schema Approval: results.schema.json (2026-03-23, Gandalf)
Full rewrite approved. Schema now models special prizes (33), overall results (2), division results (3), competitors (37), and 1st/2nd/3rd place rankings.

### Real Data Model (2026-03-23, Bilbo)
Four sheets: Categories (51), Competitor (37), Judging (266), Prizes (33). Carver ID embedded as 'ID FirstName LastName'.

### Data Quality: Carver 16 Entry Number (2026-03-23, Bilbo)
Novice '21 Busts' N: carver 16 has entry_number = 0; skipped with WARN. Source data correction needed.

## Governance
- All meaningful changes require team consensus
- Document architectural decisions here
