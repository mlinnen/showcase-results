# Issue #30 — Move checked-in filtering to JSON generation

## Context

Issue #30 originally filtered the public carvers list in Joomla by deriving a checked-in set from prizes and results. Mike changed direction: the JSON output should carry only checked-in competitors so downstream consumers do not have to repeat that filter.

## Decision

The CLI now owns checked-in filtering for the `competitors` array.

- First choice: use an explicit checked-in signal from `Competitor.xlsx` when a recognizable checked-in column exists.
- Fallback: if the competitor sheet truly has no checked-in field, limit `competitors` to carvers referenced by assigned prizes or ranked judging results.
- Joomla `ResultsService::getCarversList()` now trusts the filtered JSON instead of re-deriving checked-in IDs.

## Notes

- The current sample `data/input/Competitor.xlsx` has no explicit checked-in column, so generated sample JSON uses the fallback path today.
- This keeps the data contract aligned with the user's preference while still supporting future spreadsheets that add a true checked-in field.
