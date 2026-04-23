# Bilbo — Year Schema & C# Model Changes

**Agent:** bilbo-year-schema-csharp  
**Role:** Data Engineer  
**Task:** JSON schema + C# model/CLI changes  
**Status:** ✅ COMPLETED  
**Timestamp:** 2026-04-01T11:40:05Z

## Summary

Bilbo implemented ADR-007 at the schema and data model layers:

### JSON Schema (schema/results.schema.json)
- year field type changed from integer to string
- Added pattern constraint: ^[a-zA-Z0-9]+$ (alphanumeric validation)
- Updated schema version and documentation

### C# EventInfo Model (src/Showcase.Results.Core/Models/EventInfo.cs)
- public int Year → public string Year
- Removed integer constraints
- Type now aligns with schema definition

### CLI Interface (src/Showcase.Results/Program.cs)
- --year option: Option<int> → Option<string>
- No longer enforces numeric-only input
- Validation delegated to runtime layer and Joomla validation

## Files Modified
- schema/results.schema.json
- src/Showcase.Results.Core/Models/EventInfo.cs
- src/Showcase.Results/Program.cs

## Validation
- Schema validates against updated EventInfo model
- CLI accepts alphanumeric years
- Existing numeric years continue to work
