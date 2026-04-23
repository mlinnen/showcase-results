# Session Log — Issue #30: Checked-In Carvers Filtering

**Date:** 2026-04-23  
**Timestamp:** 2026-04-23T12:47:42Z  
**Team:** Frodo (builder), Aragorn (tester)  

## Objective

Filter the public carvers list to show only checked-in carvers (those appearing in prizes or ranked results).

## Outcome

✅ **COMPLETE**

- **PR #33:** Merged changes for checked-in carvers filtering
- **Branch:** `squad/30-only-show-checked-in-carvers-in-list`
- **Code:** Service layer filtering + template logic
- **Tests:** 4 new test cases added
- **Status:** Approved by tester, ready for merge

## Key Decision

Checked-in = carver_id appears in special_prizes or division_results. Registrations with zero results are hidden from the public list.

## Artifacts

- Orchestration logs: `.squad/orchestration-log/2026-04-23T12-47-42Z-{frodo,aragorn}.md`
- Decision inbox merged to decisions.md
- Agent histories updated
