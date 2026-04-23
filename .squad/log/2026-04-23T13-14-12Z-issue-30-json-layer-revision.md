# Session Log — Issue #30 JSON-Layer Revision (2026-04-23)

**Timestamp:** 2026-04-23T13:14:12Z  
**Milestone:** Moved checked-in carver filtering from Joomla rendering to CLI JSON generation  
**Status:** ✅ COMPLETE

## Summary

User feedback redirected the checked-in carver filtering from presentation-layer (Joomla template) to data-layer (CLI JSON export). Two-agent session:

1. **Bilbo (Data Engineer):** Updated CLI parser and schema; regenerated sample JSON with 35 checked-in competitors (37 source registrations, filtered via result-bearing carver IDs)
2. **Aragorn (Tester):** Validated filtering logic, golden-sample alignment, and Joomla component's trust-model compliance

## Key Deliverables

- ✅ PR #33: Data-layer checked-in filtering (`src/ShowcaseResults.Cli/`)
- ✅ Updated JSON schema and docs
- ✅ Regenerated sample output: `results-2026T.json` (35 competitors)
- ✅ Joomla component validated and approved

## Decision Document

See `.squad/decisions.md` — **Issue #30 — Checked-In Carvers Filtering** (2026-04-23, Frodo).

Moved checked-in filtering to JSON generation layer. Joomla component now trusts pre-filtered competitors data. Workbook lacks explicit checked-in column; fallback uses result-bearing carver IDs. Privacy-preserving by design.

## Next

Merge PR #33 to dev; deploy to staging for final acceptance.
