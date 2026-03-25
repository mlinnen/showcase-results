# ADR: Per-Carver Article Generator

**Date:** 2026-03-25
**Author:** Gandalf
**Status:** Implemented
**Issue:** #1

## Context

Carvers and website editors need individual carver result pages. The existing CLI only generates an event-wide article. We need a per-carver view without duplicating the parsing pipeline.

## Decision

### Command structure
Added `create carver-article` as a sibling to `create results` under the existing `create` command group. This keeps the CLI surface consistent (all generators live under `create`). Carver lookup supports both `--carver-id N` and `--carver-name "First Last"`.

### Renderer returns a result record, not just HTML
`RenderCarverArticle()` returns a `CarverArticleResult` record containing `CarverId`, `FullName`, `Html`, and `HasResults`. This lets the CLI handler decide exit codes and messages without inspecting HTML content. The renderer stays pure (no Console/exit concerns).

### Filtering is done inside the renderer
Rather than adding a separate filtering layer or service, filtering lives in `RenderCarverArticle()`. The data model is small enough that a single-pass filter is clear and efficient. If we later need carver filtering elsewhere (e.g., JSON export), we should extract a `CarverFilter` service.

### Reuses SpreadsheetParser unchanged
Follows ADR-001 — the three-stage pipeline is preserved. The carver-article command parses the same Excel files and builds the same `ShowcaseResultsData`, then applies carver-specific filtering at render time.

## Consequences

- No new dependencies or packages required
- Output files default to `output/carver-{id}.html`, overridable with `--output`
- Carver not found → non-zero exit; carver found but no results → exit 0 with warning
- Future batch generation (all carvers at once) can iterate over competitors and call the same renderer
