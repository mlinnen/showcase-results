# Showcase Results — Joomla Component User Guide

## Overview

**Showcase Results** (`com_showcaseresults`) is a Joomla component that displays woodcarving competition results dynamically from JSON data files. Site visitors can:

- **Browse carvers** — View the checked-in carvers for a specific event year
- **View carver details** — See individual carver results across one event or search across all past events by name

The component loads data from JSON files stored on your server and requires no database configuration. All views are read-only for site visitors; data management happens through file uploads.

## Installation

### Step 1: Download the Extension
Locate the file **`joomla/com_showcaseresults.zip`** in the project repository.

### Step 2: Install in Joomla Admin
1. Log in to your Joomla administrator panel
2. Navigate to **Extensions → Manage → Install**
3. Click the **Upload Package File** button
4. Select `com_showcaseresults.zip` from your computer and upload
5. After successful installation, you should see confirmation: "Showcase Results component has been installed successfully"

### Step 3: Verify Installation
Once installed, the component appears in the admin menu:
- Go to **Components → Showcase Results**

The component is now ready. Next, add result data files to activate it.

## Adding Result Data

Competition results live as **JSON files** on your server at:

```
[joomla-root]/media/com_showcaseresults/data/
```

Where `[joomla-root]` is your Joomla installation directory (e.g., `/var/www/html/joomla`).

### File Naming Convention

Files must be named: **`results-{event}.json`**

Examples:
- `results-2024.json` — 2024 competition data
- `results-2023.json` — 2023 competition data
- `results-2022.json` — 2022 competition data
- `results-2026T.json` — 2026 test event data (alphanumeric event identifiers are supported)

### Shipping and Updating Data

The component **ships with sample JSON files** for years that have data. To add a new year or update existing data:

1. **Generate or prepare** a `results-{event}.json` file (see "Generating JSON from Spreadsheets" below)
2. **Upload** the file to `media/com_showcaseresults/data/` on your server (via SFTP or your hosting file manager)
3. **No restart needed** — the component reads files on each page load

### Caching Note

If visitors see old data after you upload a new file:
- Clear Joomla's page cache: **System → Clear Cache**
- If a **page cache plugin** is enabled, results appear immediately after cache is cleared

## Setting Up Menu Items

Menu items allow visitors to access the two views. Create menu items in your site's main navigation.

### View 1: Carvers List

The **Carvers List** displays checked-in carvers for a specific event year in a table.

**To create a Carvers List menu item:**

1. Go to **Menus → [Your Menu Name] → Add New Menu Item**
2. Enter a menu title (e.g., "2024 Carvers")
3. Click **Select** next to Menu Item Type
4. Choose **Showcase Results → Carvers List**
5. Under **Menu Item** tab, set the **Event** parameter:
   - Enter `2024` (or your event year)
   - This filters the list to that year only
6. Click **Save & Close**

**If you leave Event blank:**
- Visitors see a **event-selector dropdown** instead
- They choose the event, then view the carvers list
- Useful if your site covers multiple events

### View 2: Carver Detail (Per-Carver Results)

The **Carver Detail** view shows results for a single carver.

**Typical usage:** Carvers List creates links to this view automatically—you don't usually need a direct menu item.

**If you do create a menu item for it:**

1. Go to **Menus → [Your Menu Name] → Add New Menu Item**
2. Enter a menu title (e.g., "Search Carver")
3. Click **Select** next to Menu Item Type
4. Choose **Showcase Results → Carver Results**
5. Leave the **name**, **carver_id**, and **event** parameters blank
   - This lets the page work as a general search/lookup
   - Visitors can enter a carver's name or use URL parameters (see reference below)
6. Click **Save & Close**

## URL Parameter Reference

Visitors and developers can use URL parameters to navigate directly to results. Both views accept parameters via the query string.

### Carvers List View (`?view=carvers`)

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `event` | No | Event identifier to display | `?view=carvers&event=2024` |

**If event is omitted:** Shows a event-selector dropdown listing all available years.

**URL example:** `https://yoursite.com/carvers?event=2024`

### Carver Detail View (`?view=carver`)

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `name` | No | Carver's full name; searches across all events | `?view=carver&name=John+Doe` |
| `name` + `event` | No | Carver's name for a specific event only | `?view=carver&name=John+Doe&event=2024` |
| `carver_id` | No | Carver's per-event ID; **must pair with event** | `?view=carver&carver_id=16&event=2024` |

**Important:** `carver_id` is **per-event only** (privacy-respecting design):
- A carver with ID 16 in 2024 might be ID 22 in 2023
- Always include the event when using `carver_id`
- Without an event, the page returns a friendly error

**Cross-event search (by name):**
- `?view=carver&name=Jane+Doe` — Shows Jane's results across all events, sorted by year (newest first)

**Single-event search:**
- `?view=carver&name=Jane+Doe&event=2024` — Shows Jane's 2024 results only

**URL examples:**
- `https://yoursite.com/search-carver?name=Jane+Doe` — Cross-event search
- `https://yoursite.com/search-carver?name=Jane+Doe&event=2024` — Single year
- `https://yoursite.com/search-carver?carver_id=16&event=2024` — By carver ID

## JSON Data File Format

If you're manually creating or editing a results file, here's the structure:

**Standard event (numeric year):** `results-2024.json`

```json
{
  "event": {
    "name": "41st Showcase of Woodcarvings",
    "event_id": "2024"
  },
  "competitors": [
    {
      "carver_id": 1,
      "first_name": "Jane",
      "last_name": "Doe",
      "division": "Novice"
    },
    {
      "carver_id": 16,
      "first_name": "John",
      "last_name": "Smith",
      "division": ""
    }
  ],
  "special_prizes": [
    {
      "name": "Best of Show",
      "prize": "$500",
      "winner": { "carver_id": 1, "first_name": "Jane", "last_name": "Doe" },
      "entry_number": 42,
      "order": 1
    }
  ],
  "overall_results": [
    {
      "category": "Realistic Figures",
      "places": [
        { "place": 1, "carver_id": 1, "first_name": "Jane", "last_name": "Doe", "entry_number": 42 },
        { "place": 2, "carver_id": 16, "first_name": "John", "last_name": "Smith", "entry_number": 7 }
      ]
    }
  ],
  "division_results": [
    {
      "division": "Novice",
      "categories": [
        {
          "name": "Best of Division",
          "style": null,
          "places": [
            { "place": 1, "carver_id": 1, "first_name": "Jane", "last_name": "Doe", "entry_number": 42 }
          ]
        },
        {
          "name": "Realistic Figures",
          "style": "N",
          "places": [
            { "place": 1, "carver_id": 1, "first_name": "Jane", "last_name": "Doe", "entry_number": 42 },
            { "place": 2, "carver_id": 16, "first_name": "John", "last_name": "Smith", "entry_number": 7 }
          ]
        }
      ]
    }
  ]
}
```

**Test/alphanumeric event:** `results-2026T.json`

```json
{
  "event": {
    "name": "2026 Test Event",
    "event_id": "2026T"
  },
  "competitors": [
    {
      "carver_id": 1,
      "first_name": "Jane",
      "last_name": "Doe",
      "division": "Novice"
    }
  ],
  "special_prizes": [],
  "overall_results": [],
  "division_results": []
}
```

> **Tip:** Use `2026T` (or any alphanumeric suffix) as a test event identifier. The component treats it identically to numeric years — the `T` suffix keeps it out of real production data when browsing the event list.

### Key Fields

- **event.name:** The event title (e.g., "41st Showcase of Woodcarvings")
- **event.event_id:** Event identifier string (e.g., `"2024"` or `"2026T"`); must match the filename suffix
- **competitors:** Registration list of carvers; includes `carver_id` (unique per event), name, and optional `division` field
- **special_prizes:** Awards and winners; ordered by `order` field (lower = higher priority)
- **overall_results:** Competition categories with 1st/2nd/3rd places
- **division_results:** Results grouped by division (Novice, Intermediate, Master, etc.); each division has categories with ranked places
- **entry_number:** Carving's entry ID; if 0 or null, renders as empty in the UI

### Notes on Fields

- **carver_id:** Unique only within an event—do not assume it correlates across years
- **division:** Optional; if missing, the carver is registered but may not appear in division results
- **style:** In categories, `"N"` = Natural, `"P"` = Painted, `null` = award-only (no style)
- **entry_number:** 0 or null means no entry number; the UI renders an empty cell

For the **complete schema**, see: `schema/results.schema.json`

## Generating JSON from Spreadsheets

Manually creating JSON files is error-prone. Use the **ShowcaseResults CLI tool** to generate them from competition spreadsheets.

The CLI accepts `.xlsx` (Excel) files and outputs `results-{event}.json`.

### CLI Usage

See the main **README.md** in the project repository for full CLI documentation and examples.

Quick example — generate JSON only (required for the Joomla component):
```bash
ShowcaseResults.Cli.exe create results --format json --output output/
```

Generate both HTML article and JSON in one run:
```bash
ShowcaseResults.Cli.exe create results --format html --format json --output output/article.html
```

The `--format` option accepts `html` and `json`. It can be repeated to produce both formats in a single run. The default is `html`.

This generates `output/results-{event}.json`. Upload it to `media/com_showcaseresults/data/` on your Joomla server.

### CLI Features

- Parses spreadsheet sheets: Competitors, Categories, Judging, Prizes
- Validates data against the schema
- Outputs `html` (Joomla article fragment), `json` (component data), or both — controlled by `--format`
- Supports multiple runs without overwriting previous years (each year is its own file)

## Troubleshooting

### Symptom: "No data available for 2024. Available events: none."

**Likely Cause:** JSON file is not at the correct location.

**Fix:**
1. Verify the file exists at: `media/com_showcaseresults/data/results-2024.json`
2. Check the filename spelling — must be exactly `results-{event}.json` (e.g., `results-2024.json` or `results-2026T.json`)
3. Ensure file permissions allow the web server to read it

### Symptom: Old data showing after I uploaded a new file

**Likely Cause:** Joomla's page cache is enabled.

**Fix:**
1. Go to **System → Clear Cache**
2. Confirm cache cleared; reload the page in your browser

### Symptom: Carver name not found or no results for a carver I know is in the data

**Likely Cause:** Name spelling or case mismatch; search is case-insensitive but must match the full name exactly.

**Fix:**
1. Double-check the carver's first and last name in the JSON file
2. Verify spelling, spaces, and punctuation match exactly
3. Try searching via Carvers List view to see the exact name format in your data
4. If the carver exists but has zero results, they may be registered but didn't place in any categories—check the `division_results` array in the JSON

### Symptom: Division column is blank for a carver in the Carvers List

**Likely Cause:** Carver is registered in `competitors` but does not appear in `division_results`.

**Fix:**
1. Add the carver's entry to the relevant division's categories in `division_results`, or
2. Update the `competitors` entry for that carver to include the `"division"` field with the correct division name (e.g., `"Novice"`, `"Master"`)

### Symptom: URL with `?carver_id=16` returns an error

**Likely Cause:** Missing the `event` parameter.

**Fix:**
- Always include `event` when using `carver_id`: `?view=carver&carver_id=16&event=2024`
- The `carver_id` is unique only within a single event (privacy-respecting design)

### Symptom: Malformed JSON file (syntax error) silently ignored

**Likely Cause:** JSON file has invalid formatting (missing comma, unmatched bracket, etc.).

**Fix:**
1. Validate the JSON file syntax using an online JSON validator (e.g., jsonlint.com)
2. Re-generate the file using the CLI tool (recommended)
3. The component will skip malformed files and show available years instead

---

## Support & Documentation

- **Component files:** `joomla/com_showcaseresults/`
- **Data schema:** `schema/results.schema.json`
- **CLI tool:** See main **README.md** for generator usage
- **Code:** PHP 8.1+, Joomla 4.x/5.x compatible

For issues or questions, contact your development team.
