# Bilbo — History Archive

> Archived learnings from before 2026-03-27 (30+ days old as of 2026-04-26).

---

## Archived Entries

### 2026-03-23 — Spreadsheet analysis & data model

Analyzed all four source spreadsheets in `data/input/`:
- `Categories.xlsx` (51 rows) — competition category definitions with flags and relationships
- `Competitor.xlsx` (37 rows) — carver registration records; PII/payment columns excluded from output
- `Judging.xlsx` (266 rows) — full judging results; winner encoded as "ID Name" string; ~122 empty rows
- `Prizes.xlsx` (33 rows) — 33 named awards; Prize column is mixed integer/string

Key findings: carver identity is denormalized across sheets as "ID FirstName LastName" strings; category names have trailing whitespace; the existing schema doesn't match the actual data structure (no ribbons — actual data uses 1st/2nd/3rd place).

Deliverables:
- Updated `data/processed/README.md` with corrected JSON schema for Frodo
- Updated `data/raw/README.md` with actual column documentation
- Wrote `.squad/decisions/inbox/bilbo-data-model.md` — recommends revised `results.json` structure; flagged for Gandalf approval
- Updated `.squad/identity/now.md`

Next: pending Gandalf schema approval → build parser in `src/parse/`

### 2026-03-23 — Parser built and verified

Built `src/parse/index.js` (plain JS, runs with `node src/parse/index.js`) and `src/parse/index.ts` (TypeScript equivalent for future compile step). Created `package.json` with `xlsx` and `ajv` dependencies.

Parser produces `data/output/results.json` that validates against `schema/results.schema.json`:
- 33 special prizes (sorted by order)
- 2 overall result categories (Best of Show + Overall Best of Show Theme)
- 3 divisions (Intermediate, Novice, Open) with full category/style/places breakdown
- 37 competitors

Key implementation notes:
- `xlsx` library renames duplicate Judging.xlsx columns: `#` (2nd) → `#_1`, `#` (3rd) → `#_2` (same for Prize)
- Row 1 (0-indexed) in all sheets is the merged title header; `{range: 1}` skips it correctly
- `{defval: null}` required so missing cells come through as null, not undefined
- AJV v8 exposes constructor as `Ajv.default` in CommonJS require context

Data quality issue discovered: 10 rows in Judging.xlsx have entry_number = 0. Of these, 9 have null carver names (phantom zeros — never emitted). 1 row (Novice "21 Busts N", carver 16 Erik Mitchell) has a real winner with entry_number = 0 — that place entry is dropped with a warning. See `.squad/decisions/inbox/bilbo-entry-number-zeros.md`.

### Issue #7 — Feature Complete (2026-03-25T13:57:52Z)

**Milestone:** Issue #7 now feature-complete across all sub-issues. CLI JSON export contribution validated.

- **Summary:** Delivered PR #14 (CLI JSON export with `--format json` and results-{year}.json convention).
- **Validation:** Test plan (28 cases) and test data spec (results-2024.json, results-2023.json) created by Aragorn. Code review PASSED.
- **Architecture verified:** results-{year}.json path convention correct, data structure validated, integration with Joomla data layer confirmed.
- **Awaiting:** PR merge sequence. Feature ready for production deployment once all 6 PRs merged in order.

### Issue #24 — Year Changed from Integer to String (2026-03-25)

**Task:** Support text in year parameter (e.g., `2026T` for test events). Changed year from integer to string throughout the codebase.

**Files modified:**
1. `schema/results.schema.json` — Changed year type from `integer` with `minimum: 2000` to `string` with `pattern: "^[a-zA-Z0-9]+$"` (prevents path traversal, allows alphanumeric values)
2. `src/ShowcaseResults.Cli/Models/Results.cs` — Changed `EventInfo` record from `int Year` to `string Year`
3. `src/ShowcaseResults.Cli/Program.cs` — Changed `yearOption` from `Option<int>` to `Option<string>`, updated default value from `DateTime.Now.Year` to `DateTime.Now.Year.ToString()`, added null-forgiving operators for year variables

**Key findings:**
- Filename construction `$"results-{data.Event.Year}.json"` works naturally with string interpolation — no changes needed
- No arithmetic or comparison operations on year found in the codebase
- Build succeeded with no compilation errors after nullable reference warnings fixed

**Validation:** Built project successfully with `dotnet build src\ShowcaseResults.Cli\ShowcaseResults.Cli.csproj`

- JSON year fields must be strings (alphanumeric: ^[a-zA-Z0-9]+$) per schema update, not integers

### Session: Issue #24 — Year as String (2026-04-01)

**Role:** Data Engineer  
**Task 1:** JSON schema + C# model changes  
**Task 2:** JSON data file updates + README

Implemented year-as-string at schema and data layers:
- schema/results.schema.json: year type → string with pattern validation
- EventInfo.Year: int → string
- CLI --year: Option<int> → Option<string>
- output/*.json: updated all year values to strings
- README.md: added alphanumeric year example

Status: ✅ COMPLETED - Both tasks merged and validated.

### Session: year→event Parameter Rename (current)

**Task:** Rename the `year` CLI parameter/property to `event` throughout C# source and JSON schema.

**Rename pattern:** `year` (parameter/property) → `event` / `EventId` / `eventId`

**Files changed:**
1. `src/ShowcaseResults.Cli/Models/Results.cs` — `EventInfo` record: `string Year` → `string EventId`
2. `src/ShowcaseResults.Cli/Program.cs` — `yearOption` → `eventOption`; `--year` → `--event`; `var year` → `var eventId` (x2 handlers); `new EventInfo(eventName, year)` → `new EventInfo(eventName, eventId)` (x2); `data.Event.Year` → `data.Event.EventId` in filename construction
3. `src/ShowcaseResults.Cli/Rendering/ArticleRenderer.cs` — `data.Event.Year` → `data.Event.EventId` (x3 string interpolations)
4. `schema/results.schema.json` — `"required": ["name", "year"]` → `["name", "event"]`; `"year"` property key → `"event"`

**No `data/` JSON files needed changes** — grep confirmed no `"year"` keys present in any data files.

**Build result:** `dotnet build src\showcase-results.sln` — succeeded with 0 errors (2 pre-existing NuGet vulnerability warnings, unrelated).

**Key constraint respected:** `DateTime.Now.Year.ToString()` left intact — that is a .NET system call, not the parameter name.

### Session: --format Help Text Improvement (2026-04-01)

**Task:** Review and improve `--format` help text in `create results`.

**Findings:**
- `create results` has `--format` as `Option<string[]>` with `AllowMultipleArgumentsPerToken = true`. Default `["html"]`. Values: `html` (writes article to `--output` path) and `json` (writes `results-{event}.json` to the same directory for the Joomla data layer).
- `create carver-article` has NO `--format` option — correct by design; carver-article is HTML-only and produces no JSON variant.
- Original help text (`"Output format(s): html, json (can be repeated: --format html --format json)"`) said nothing about *what* each format writes or where.

**Change:** Improved `--format` description to: `"Output format(s): html writes the results article to the --output path; json writes results-{event}.json to the same directory (used by the Joomla data layer). Repeatable: --format html --format json"`

**Build:** `dotnet build src\showcase-results.sln` — 0 errors, 2 pre-existing NuGet warnings (unchanged).

### Session: --data-root Parameter (2026-04-01)

**Task:** Add optional `--data-root` parameter to CLI for setting default input/output root paths.

**Implementation:**
- Added `--data-root` option (nullable string) to both `create results` and `create carver-article` commands
- Changed `--competitors`, `--prizes`, `--judging`, `--output` options from `Option<string>` with hardcoded defaults to `Option<string?>` with null defaults
- Handler logic computes effective paths: explicit option > data-root-based path > original relative default
- Updated help text for input/output options to document the data-root behavior

**Path resolution pattern:**
```csharp
var competitors = Path.GetFullPath(competitorsExplicit 
    ?? (dataRoot != null ? Path.Join(dataRoot, "input", "Competitor.xlsx") 
    : Path.Join("data", "input", "Competitor.xlsx")));
```

**Default paths with data-root:**
- `{data-root}/input/Competitor.xlsx`, `Prizes.xlsx`, `Judging.xlsx`
- `{data-root}/output/article.html` (results command)
- `{data-root}/output/carver-{id}.html` (carver-article command)

**Key finding:** System.CommandLine doesn't provide "was this option explicitly set" metadata, so nullable defaults with null-coalescing logic is the cleanest approach to distinguish user input from defaults.

**Build:** `dotnet build src\ShowcaseResults.Cli\ShowcaseResults.Cli.csproj` — 0 errors (2 pre-existing NuGet warnings).

**Deliverable:** PR #25 created on branch `feature/data-root-parameter`.
