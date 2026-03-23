# Processed Data — JSON Output

This folder contains Bilbo's normalized output from the raw spreadsheets. These files are the **input for Frodo** when building the Joomla article.

---

## Files

| File | Description | Owner |
|------|-------------|-------|
| `prizes.json` | Normalized list of prizes and winners | Bilbo |
| `competitors.json` | Normalized list of competitors and ribbons | Bilbo |
| `NOTES.md` | Data-cleaning log and column mapping decisions | Bilbo |

---

## Schema: `prizes.json`

Array of prize objects. Required fields marked *.

```json
[
  {
    "prize_name": "Best in Show",   // * string
    "category": "Open",             // string, may be ""
    "carver_name": "Jane Doe"       // * string
  }
]
```

Sorted: by `category` (ascending), then `prize_name` (ascending).

---

## Schema: `competitors.json`

Array of entry objects. Required fields marked *.

```json
[
  {
    "carver_name": "Jane Doe",      // * string
    "entry_title": "Mountain Eagle",// string, may be ""
    "category": "Open",             // * string
    "ribbon": "Blue"                // * one of: Blue | Red | White | Participant
  }
]
```

Sorted: by `category` (ascending), then ribbon rank (Blue → Red → White → Participant), then `carver_name`.

---

## Ribbon Rank Reference

| Ribbon | Rank | Typical meaning |
|--------|------|----------------|
| Blue | 1st | First place |
| Red | 2nd | Second place |
| White | 3rd | Third place |
| Participant | 4th | Participation / honorable mention |

> Bilbo: if the source data uses different values (e.g. "1st", "Gold"), map them to these canonical names and document in `NOTES.md`.
