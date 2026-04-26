# Gandalf — Architecture Decision ADR-007

**Agent:** gandalf-year-arch  
**Role:** Lead  
**Task:** Architecture decision ADR-007 for year-as-string  
**Status:** ✅ COMPLETED  
**Timestamp:** 2026-04-01T11:40:00Z

## Summary

Gandalf led architectural decision ADR-007 establishing that the year field should support alphanumeric values (not just integers). This decision cascaded across the entire stack:

- **JSON Schema:** Updated to support string type with pattern ^[a-zA-Z0-9]+$
- **C# Model:** EventInfo.Year changed from int to string
- **CLI Interface:** --year option updated to accept Option<string>
- **Joomla Layer:** All year handling switched from getInt() to getString() and validation updated

## Impact

- Enables experimental year notation (e.g., "2026T" for trial runs)
- Maintains backward compatibility with existing numeric years
- Consistent type propagation across all layers

## Decisions Made

1. Pattern validation: alphanumeric only ([a-zA-Z0-9]+)
2. Default year handling updated where applicable
3. Cascading coordination with Bilbo (schema), Frodo (Joomla), and Aragorn (validation)
