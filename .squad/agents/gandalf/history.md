# Gandalf — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Lead
- **Joined:** 2026-03-23T01:57:14.726Z

## Learnings

<!-- Append learnings below -->

- **2026-03-23 — Schema rewrite approved (ADR-002).** The bootstrap schema assumed ribbons and flat categories; actual data uses 1st/2nd/3rd place across divisions with named prizes. Rewrote `schema/results.schema.json` to match Bilbo's real data model: `special_prizes`, `overall_results`, `division_results`, `competitors` with `carver_id`. No code depended on the old schema yet, so zero migration cost. Lesson: always validate schema assumptions against actual data before anyone builds against them.
