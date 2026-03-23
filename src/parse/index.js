/**
 * index.js — Bilbo
 * Reads all four spreadsheets from data/input/, normalises the data per ADR-002,
 * validates against schema/results.schema.json, and writes data/output/results.json.
 *
 * Usage: node src/parse/index.js
 */

'use strict';

const fs   = require('fs');
const path = require('path');
const XLSX = require('xlsx');
const Ajv  = require('ajv');

const INPUT_DIR   = path.resolve(__dirname, '../../data/input');
const OUTPUT_FILE = path.resolve(__dirname, '../../data/output/results.json');
const SCHEMA_FILE = path.resolve(__dirname, '../../schema/results.schema.json');

// Event metadata — not present in the spreadsheets; update per event
const EVENT_NAME = 'CCA Showcase';
const EVENT_YEAR = 2026;

// ─── helpers ────────────────────────────────────────────────────────────────

/** Parse "ID FirstName LastName" → { carver_id, winner } or null (ADR-002 constraint 2) */
function parseCarver(raw) {
  if (!raw) return null;
  const s = String(raw).trim();
  const spaceIdx = s.indexOf(' ');
  if (spaceIdx === -1) return null;
  const carver_id = parseInt(s.substring(0, spaceIdx), 10);
  const winner = s.substring(spaceIdx + 1).trim();
  if (isNaN(carver_id) || !winner) return null;
  return { carver_id, winner };
}

/** Normalize Style: "N", "P", or null — never empty string (ADR-002 constraint 6) */
function normalizeStyle(val) {
  if (!val || typeof val !== 'string' || val.trim() === '') return null;
  const s = val.trim();
  if (s === 'N' || s === 'P') return s;
  return null;
}

/** Normalize Division: exactly "Intermediate", "Novice", "Open", or null (ADR-002 constraint 7) */
function normalizeDivision(val) {
  if (!val || typeof val !== 'string') return null;
  const s = val.trim();
  if (s === 'Intermediate' || s === 'Novice' || s === 'Open') return s;
  return null;
}

/**
 * Read an xlsx file, skipping the merged title row (row 0).
 * Row 1 is used as the header row; rows 2+ are data.
 */
function readSheet(filename) {
  const wb = XLSX.readFile(path.join(INPUT_DIR, filename));
  const ws = wb.Sheets[wb.SheetNames[0]];
  return XLSX.utils.sheet_to_json(ws, { range: 1, defval: null });
}

// ─── parsers ─────────────────────────────────────────────────────────────────

function parseCompetitors() {
  const rows = readSheet('Competitor.xlsx');
  return rows
    .filter(r => r['Carver ID'] != null)
    .map(r => ({
      carver_id:  parseInt(String(r['Carver ID']), 10),
      first_name: String(r['First Name']).trim(),
      last_name:  String(r['Last Name']).trim(),
    }));
}

function parseSpecialPrizes() {
  const rows = readSheet('Prizes.xlsx');
  const prizes = rows
    .filter(r => r['Name'] && r['Carver'] && r['Order'] != null)
    .map(r => {
      const carver = parseCarver(r['Carver']);
      if (!carver) return null;
      return {
        order:        parseInt(String(r['Order']), 10),
        name:         String(r['Name']).trim(),
        carver_id:    carver.carver_id,
        winner:       carver.winner,
        entry_number: parseInt(String(r['Entry #']), 10),
        prize:        String(r['Prize']).trim(),  // always a string (ADR-002 constraint 5)
      };
    })
    .filter(x => x !== null);

  prizes.sort((a, b) => a.order - b.order);
  return prizes;
}

function parseJudging() {
  const rows = readSheet('Judging.xlsx');

  // The xlsx library renames duplicate column headers:
  //   "#" (1st) → "#",  "#" (2nd) → "#_1",  "#" (3rd) → "#_2"
  //   "Prize" (1st) → "Prize", (2nd) → "Prize_1", (3rd) → "Prize_2"
  const overallResults  = [];
  const divisionMap     = {};

  for (const row of rows) {
    const places = [];

    const c1 = parseCarver(row['1st']);
    if (c1) {
      const en = parseInt(String(row['#']), 10);
      if (en >= 1) {
        places.push({ place: 1, carver_id: c1.carver_id, winner: c1.winner, entry_number: en });
      } else {
        console.warn(`  WARN: skipping 1st place for "${String(row['Category']).trim()}" (${row['Division']}) — entry# is ${row['#']} (carver ${c1.carver_id} ${c1.winner})`);
      }
    }

    const c2 = parseCarver(row['2nd']);
    if (c2) {
      const en = parseInt(String(row['#_1']), 10);
      if (en >= 1) {
        places.push({ place: 2, carver_id: c2.carver_id, winner: c2.winner, entry_number: en });
      } else {
        console.warn(`  WARN: skipping 2nd place for "${String(row['Category']).trim()}" (${row['Division']}) — entry# is ${row['#_1']} (carver ${c2.carver_id} ${c2.winner})`);
      }
    }

    const c3 = parseCarver(row['3rd']);
    if (c3) {
      const en = parseInt(String(row['#_2']), 10);
      if (en >= 1) {
        places.push({ place: 3, carver_id: c3.carver_id, winner: c3.winner, entry_number: en });
      } else {
        console.warn(`  WARN: skipping 3rd place for "${String(row['Category']).trim()}" (${row['Division']}) — entry# is ${row['#_2']} (carver ${c3.carver_id} ${c3.winner})`);
      }
    }

    // Omit rows with all-null place columns (ADR-002 constraint 4)
    if (places.length === 0) continue;

    const category = String(row['Category']).trim();  // trim trailing whitespace (ADR-002 constraint 3)
    const division = normalizeDivision(row['Division']);
    const style    = normalizeStyle(row['Style']);

    if (division === null) {
      overallResults.push({ category, places });
    } else {
      if (!divisionMap[division]) divisionMap[division] = [];
      divisionMap[division].push({ name: category, style, places });
    }
  }

  const DIVISION_ORDER = ['Intermediate', 'Novice', 'Open'];
  const divisionResults = DIVISION_ORDER
    .filter(d => divisionMap[d])
    .map(d => ({ division: d, categories: divisionMap[d] }));

  return { overallResults, divisionResults };
}

// ─── main ────────────────────────────────────────────────────────────────────

async function main() {
  const competitors    = parseCompetitors();
  const special_prizes = parseSpecialPrizes();
  const { overallResults, divisionResults } = parseJudging();

  const results = {
    event:            { name: EVENT_NAME, year: EVENT_YEAR },
    special_prizes,
    overall_results:  overallResults,
    division_results: divisionResults,
    competitors,
  };

  // Validate against approved schema before writing (ADR-002 constraint 1)
  const AjvClass = typeof Ajv === 'function' ? Ajv : Ajv.default;
  const ajv = new AjvClass({ strict: false });
  const schema = JSON.parse(fs.readFileSync(SCHEMA_FILE, 'utf8'));
  const validate = ajv.compile(schema);
  const valid = validate(results);
  if (!valid) {
    console.error('Schema validation FAILED:');
    console.error(JSON.stringify(validate.errors, null, 2));
    process.exit(1);
  }

  fs.mkdirSync(path.dirname(OUTPUT_FILE), { recursive: true });
  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(results, null, 2), 'utf8');

  console.log(`✓ Wrote ${OUTPUT_FILE}`);
  console.log(`  ${special_prizes.length} special prizes`);
  console.log(`  ${overallResults.length} overall result categories`);
  console.log(`  ${divisionResults.length} divisions (${divisionResults.map(d => d.division).join(', ')})`);
  console.log(`  ${competitors.length} competitors`);
}

main().catch(err => { console.error(err); process.exit(1); });
