---
name: "checked-in-carvers"
description: "Filter public Joomla carver lists to checked-in participants instead of raw registrations"
domain: "joomla"
confidence: "high"
source: "earned"
---

## Context
Use this when building or updating Joomla views that expose a public list of carvers for an event. The raw `competitors` array is a registration list, so it can contain people who never actually checked in.

## Patterns
- For public list views, derive the visible carver set from result-bearing `carver_id` values, not directly from `competitors`.
- Build the checked-in set from the union of:
  - `special_prizes[].carver_id`
  - `overall_results[].places[].carver_id`
  - `division_results[].categories[].places[].carver_id`
- After collecting the checked-in IDs, filter `competitors` down to matching rows so the list can still use competitor metadata like full name and division.
- Keep the output sorted exactly as the view expects after filtering; filtering should not change the display sort contract.

## Examples
- `joomla/com_showcaseresults/site/src/Service/ResultsService.php` — `collectCheckedInCarverIds()` plus filtered `getCarversList()`
- `docs/test-plan-carvers-list.md` — hidden no-result registrations are part of the expected behavior

## Anti-Patterns
- Rendering every entry in `competitors` for a public carvers list
- Assuming registration implies check-in
- Duplicating checked-in logic in templates instead of centralizing it in the service layer
