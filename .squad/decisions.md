# Squad Decisions

## Active Decisions

### ADR-001 — Three-stage pipeline (2026-03-23, Gandalf)
Spreadsheets → `results.json` (Bilbo) → `article.html` (Frodo). JSON Schema in `schema/results.schema.json` is the hard contract between stages. See `.squad/decisions/inbox/gandalf-architecture.md` for full rationale.

**Key rules:**
- Frodo reads only `results.json` — never spreadsheets directly
- Schema changes require Gandalf approval
- Joomla HTML rules (no wrappers, no inline styles, no scripts) enforced by Aragorn's tests
- `data/input/*.xlsx` are gitignored (potential personal data)

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
