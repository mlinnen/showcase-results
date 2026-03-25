# Gandalf — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Lead
- **Joined:** 2026-03-23T01:57:14.726Z

## Learnings

<!-- Append learnings below -->

- **2026-03-24 — Release pipeline created (ADR-003).** Built `.github/workflows/release.yml`: matrix strategy builds `win-x64`, `win-x86`, `win-arm64` in parallel on `windows-latest` using `actions/setup-dotnet@v4` with .NET 10. Each matrix job publishes a self-contained single-file exe, renames it to `showcase-results-{runtime}.exe`, and uploads as an artifact. A final `release` job downloads all three artifacts and creates a GitHub Release via `softprops/action-gh-release@v2` with a date+run-number tag (`vYYYY.MM.DD-{run_number}`). Triggers on push to `main`; workflow-level `contents: write` permission enables release creation.

- **2026-03-23 — Schema rewrite approved (ADR-002).** The bootstrap schema assumed ribbons and flat categories; actual data uses 1st/2nd/3rd place across divisions with named prizes. Rewrote `schema/results.schema.json` to match Bilbo's real data model: `special_prizes`, `overall_results`, `division_results`, `competitors` with `carver_id`. No code depended on the old schema yet, so zero migration cost. Lesson: always validate schema assumptions against actual data before anyone builds against them.
