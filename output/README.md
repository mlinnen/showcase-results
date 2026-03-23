# Output — Joomla Article HTML

This folder contains the final HTML article produced by Frodo.

---

## Files

| File | Description |
|------|-------------|
| `article.html` | Ready-to-paste Joomla article HTML fragment |

---

## How to Use in Joomla

1. Open the target Joomla article in the backend editor
2. Switch to **Source** / **HTML** view (button in the toolbar, usually `<>` or `Source`)
3. Paste the contents of `article.html` into the editor
4. Switch back to visual mode and review the layout
5. Save the article

> **Note:** `article.html` is an HTML **fragment** — it does not include `<html>`, `<head>`, or `<body>` tags. Joomla's editor expects fragments.

---

## Article Structure

The generated article contains:

1. **Event header** — event name and date
2. **Prizes section** — grouped by category; prize name + winner for each award
3. **Ribbon Winners section** — grouped by category; carver name, entry title, and ribbon for each entry

---

## Regenerating the Article

If source data changes:
1. Bilbo re-runs parsing → updates `../data/processed/prizes.json` and `../data/processed/competitors.json`
2. Frodo re-runs article build → overwrites `output/article.html`
3. Aragorn validates before the updated file is considered ready
