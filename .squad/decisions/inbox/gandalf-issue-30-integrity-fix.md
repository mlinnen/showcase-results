# Issue #30 — checked-in list integrity fix

Date: 2026-04-24
Owner: Gandalf

## Decision

Treat `competitors` as the shared competitor directory for downstream consumers, not as the public carvers list itself.

## Rule

- If `Competitor.xlsx` has a checked-in column, set `checked_in` on every competitor row from that source field.
- Keep every `checked_in=true` row.
- Also keep any competitor whose `carver_id` appears in `special_prizes`, `overall_results[].places`, or `division_results[].categories[].places`, even when `checked_in=false`.
- Public list views must filter on `checked_in`.
- Winner/detail/name lookup code may use the full `competitors` directory.

## Rationale

Aragorn's finding exposed a contract bug: filtering `competitors` down to checked-in rows alone can orphan real winners whose results still exist elsewhere in the JSON. The fix preserves the spreadsheet's checked-in column as the primary public-list signal while guaranteeing Joomla can still resolve names/details for result-bearing competitors.

## Implementation Notes

- C# pipeline: `Competitor.CheckedIn` added to the model; JSON generation now merges checked-in rows with result-bearing integrity rows and emits a warning for mismatches.
- Joomla: `ResultsService::getCarversList()` filters on `checked_in`; other lookup paths still read the full competitor directory.
- Schema/docs/sample data updated to document `checked_in` and the integrity exception.
