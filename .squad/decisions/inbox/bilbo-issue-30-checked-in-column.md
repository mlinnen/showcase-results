# Bilbo Decision — Issue #30 checked-in column

## Decision

Use the competitor spreadsheet's explicit `Checked In` column as the source of truth for the generated JSON `competitors` array.

## Why

The source workbook now carries an explicit attendance signal, so inferring check-in from prizes or placements would incorrectly hide checked-in carvers who did not place. That inference is now retained only as a backward-compatible fallback for older workbooks that truly lack a checked-in field.

## Impact

- `results-{event}.json` may include checked-in competitors with zero results.
- Joomla should continue trusting the filtered JSON instead of re-deriving attendance.
- Sample output and docs must describe `Checked In` / `Yes` as the primary rule.
