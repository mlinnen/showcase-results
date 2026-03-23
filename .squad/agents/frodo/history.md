# Frodo — History

## Core Context

- **Project:** A Joomla article generator that transforms carving competition spreadsheets (prizes, winners, competitors, ribbons) into formatted web content.
- **Role:** Content Builder
- **Joined:** 2026-03-23T01:57:14.727Z

## Learnings

### Schema Approved, Parser Complete (2026-03-23T02:33:36Z)
- **Status:** Unblocked. Parser complete, data/output/results.json ready.
- **Schema:** ADR-002 approved. 33 special prizes, 2 overall results, 3 divisions, 37 competitors. No ribbon concept — uses 1st/2nd/3rd place rankings.
- **Data Quality:** One note: Carver 16 (Erik Mitchell) has entry_number = 0 for Novice "21 Busts N"; win omitted from output.
- **Next:** Build output/article.html from results.json per ADR-002 constraints (sections: special prizes, overall results, division results).

### Article Built (2026-03-23)
- **Status:** Complete. `output/article.html` generated (1190 lines). `src/render/index.js` written as reproducible Node.js renderer.
- **Joomla compliance verified:** No `<html>/<head>/<body>` wrappers, no inline `style=`, no `<script>` tags. Zero violations.
- **Structure:** Three sections — Special Prizes table (33 rows, sorted by `order`), Overall Results table (2 categories), Division Results (3 divisions × awards + category sub-tables).
- **Style handling:** `null` → empty cell (no "null" text); `N` → "Natural"; `P` → "Painted".
- **Place cells:** Render as "Name (#entry)" using a `<span class="cca-entry">` for the entry number — keeps name and entry scannable without extra columns.
- **Division layout:** Categories with `style: null` (Best Of awards) split into a compact 2-column "Division Awards" table; N/P categories go into a 5-column "Category Results" table. Prevents visual noise from empty Style cells on award rows.
- **Missing places:** Empty `<td>` for 2nd/3rd when not present — no placeholder text, clean output.
- **Pre-existing `article.ts`:** Placeholder TypeScript file left untouched; `index.js` is the new canonical renderer per the task spec.

### Fix 2 — Title and Special Prizes Entry Format (2026-03-23)
- **Status:** Fixed two renderer issues in `src/render/index.js`. Article regenerated.
- **Title fix:** Removed duplicated year from title. Now uses `event.name` directly without appending `event.year`, allowing `event.name` to control the full event title (e.g., "CCA Showcase 2026").
- **Special prizes entry format fix:** Changed entry numbers from bare text `#N` to `<span class="cca-entry">(#N)</span>`, matching the format used in category results tables. Entry number 0 or null/undefined renders as empty `<td></td>`.
- **Spot-checks passed:** Title reads "Showcase of Woodcarvings — Showcase Results" (will read "CCA Showcase 2026 — Showcase Results" once JSON updated); special prizes table uses correct `<span class="cca-entry">(#N)</span>` format; no literal "null" text; no entry #0 displayed.

### Special Prizes Prize Column Enhancement (2026-03-23)
- **Status:** Enhanced special prizes table with new Prize column. `renderSpecialPrizes()` updated, article regenerated.
- **Table structure:** Column order now: Name | Prize | Winner | Entry (was: Prize | Winner | Entry). First column header changed from "Prize" to "Name" to reflect that it displays the award name (e.g., "Best of Show").
- **Prize value formatting:** New Prize column (renders `p.prize` field) implements smart formatting: numeric strings (e.g., "450") render as dollar amounts ("$450"), non-numeric strings render as-is (e.g., "Lee S. Dukes Memorial Award Commemorative Plaque"), missing/null/empty values render as empty `<td></td>`.
- **Verification passed:** Checked output/article.html — `<th>Name</th>` and `<th>Prize</th>` present in correct order, dollar amounts render correctly (e.g., $450, $250), non-numeric prizes display as plain text, regex pattern `/^\d+(\.\d{1,2})?$/` correctly identifies numeric values for currency formatting.

