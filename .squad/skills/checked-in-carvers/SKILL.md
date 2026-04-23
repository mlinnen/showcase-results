---
name: "checked-in-carvers"
description: "Keep public carver lists aligned with checked-in participants, preferably at the JSON generation layer"
domain: "joomla"
confidence: "high"
source: "earned"
---

## Context
Use this when building or updating the spreadsheet-to-JSON pipeline or Joomla views that expose a public list of carvers for an event. Prefer filtering the `competitors` array during JSON generation so downstream views can trust the data contract.

## Patterns
- Prefer emitting only checked-in carvers in the JSON `competitors` array so downstream consumers do not need duplicate filtering logic.
- Use the competitor spreadsheet's own checked-in signal when it exists.
- If the competitor sheet truly lacks a checked-in field, fall back to the union of result-bearing `carver_id` values.
- Build the checked-in set from the union of:
  - `special_prizes[].carver_id`
  - `overall_results[].places[].carver_id`
  - `division_results[].categories[].places[].carver_id`
- After collecting the checked-in IDs, filter `competitors` down to matching rows so the list can still use competitor metadata like full name and division.
- Keep the output sorted exactly as the consumer expects after filtering; filtering should not change the display sort contract.
- Validate the rule against a sample results file by comparing the registration count with the unique checked-in ID count and naming at least one excluded registration.

## Examples
- `src/ShowcaseResults.Cli/Parsing/SpreadsheetParser.cs` — source check-in detection with fallback when the sheet lacks that field
- `src/ShowcaseResults.Cli/Program.cs` — fallback result-based checked-in set applied before JSON serialization
- `joomla/com_showcaseresults/site/src/Service/ResultsService.php` — simplified `getCarversList()` that trusts the filtered JSON
- `docs/test-plan-carvers-list.md` — hidden no-result registrations are part of the expected behavior

## Anti-Patterns
- Rendering every entry in `competitors` when the JSON contract already says it is checked-in only
- Assuming registration implies check-in
- Re-implementing the same checked-in filter separately in every downstream consumer
