# Squad Decisions

## Active Decisions

### Issue #34 Research — Architecture Sound, Phase 1 Ready (2026-04-26, Gandalf)
Completed comprehensive research on AppSheet API integration. Existing five-phase plan is architecturally sound. Phase 1 (IDataProvider abstraction) is unblocked and ready for design review. Phases 2–5 depend on user input: AppSheet app configuration, table names, column mappings, carver ID format, checked-in column semantics, judging table structure, API rate limits. Gandalf research summary: https://github.com/mlinnen/showcase-results/issues/34#issuecomment-4323581264. Bilbo feasibility report confirms HIGH feasibility; design maintains zero-change contract to downstream layers; AppSheet parser will replicate three-layer checked-in logic. Next: Phase 1 design review (Bilbo), Aragorn validation within 2 days. Blockers resolved by 48-hour user input window.

### Issue #34 Feasibility Confirmed — Seven Unknowns Identified (2026-04-26, Bilbo)
Detailed AppSheet API integration feasibility study completed. Key findings: (1) Current SpreadsheetParser produces ShowcaseResultsData—an AppSheetParser implementing IDataProvider would be a drop-in replacement with zero downstream changes. (2) AppSheet's row-based, column-oriented REST API aligns with XLSX structure. (3) Normalization logic (trim, enum validation, carver ID parsing, checked-in detection) reusable from SpreadsheetParser. (4) Feasibility HIGH; risk LOW for high-confidence areas (data model alignment, abstraction design, normalization, JSON/HTML output). (5) Seven user-provided unknowns block Phase 2: app ID, table names, column mappings, carver ID format, checked-in column details, multi-place storage (rows vs. columns), API rate limits. Implementation strategy: CLI accepts table/column mappings via options (Phase 4) or optional config file (Phase 3). Bilbo report: https://github.com/mlinnen/showcase-results/issues/34 comment. Recommendation: Proceed with Phase 1 design, schedule 48-hour window for user input on seven unknowns.

---

## Decision Templates

_None currently in draft._
