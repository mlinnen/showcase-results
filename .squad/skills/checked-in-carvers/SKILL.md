---
name: "checked-in-carvers"
description: "Keep public carver lists aligned with checked-in participants, preferably at the JSON generation layer"
domain: "joomla"
confidence: "high"
source: "earned"
---

## Context
Use this when building or updating the spreadsheet-to-JSON pipeline or Joomla views that expose a public list of carvers for an event. Keep the JSON contract coherent for both public list filtering and winner/detail lookups.

## Patterns
- Prefer emitting a competitor directory that keeps every checked-in carver plus any result-bearing competitor needed for lookup integrity.
- Use the competitor spreadsheet's own checked-in signal when it exists.
- Treat the source sheet's explicit checked-in column as authoritative even when a checked-in carver has no prizes or placements.
- In the current workbook, the explicit column name is `Checked In` and values like `Yes` should be treated as checked in.
- Validate that no prize/result-bearing `carver_id` is accidentally excluded by the explicit checked-in filter; otherwise downstream lookups can lose access to a real winner.
- If a result-bearing competitor is marked unchecked, keep that row in `competitors` with `checked_in: false` and warn during generation.
- If the competitor sheet truly lacks a checked-in field, fall back to the union of result-bearing `carver_id` values.
- Build the checked-in set from the union of:
  - `special_prizes[].carver_id`
  - `overall_results[].places[].carver_id`
  - `division_results[].categories[].places[].carver_id`
- After collecting the result-bearing IDs, keep rows where `checked_in` is true or the ID appears in results, then let public list views filter on `checked_in`.
- Do not drop checked-in competitors just because they have zero results; that was only a fallback heuristic when no explicit check-in field existed.
- Keep the output sorted exactly as the consumer expects after filtering; filtering should not change the display sort contract.
- Validate the rule against a sample results file by comparing the registration count with the unique checked-in ID count and naming at least one excluded registration.
- When testing, create one deliberate mismatch case (result-bearing competitor marked unchecked) to confirm the pipeline warns or fails instead of silently emitting orphaned result rows.

## Examples
- `src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs` — source check-in detection with fallback when the sheet lacks that field
- `src/ShowcaseResults.Cli/Program.cs` — explicit checked-in flag merged with result-bearing IDs before JSON serialization
- `joomla/com_showcaseresults/site/src/Service/ResultsService.php` — `getCarversList()` filters on `checked_in` while name/ID lookups still use the full directory
- `docs/test-plan-carvers-list.md` — hidden no-result registrations are part of the expected behavior

## Anti-Patterns
- Rendering every entry in `competitors` when the JSON contract includes integrity-only rows
- Assuming registration implies check-in
- Re-implementing the same checked-in filter separately in every downstream consumer
- Silently emitting result rows for carver IDs that were removed from `competitors` by the explicit checked-in filter
