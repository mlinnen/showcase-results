/**
 * index.ts — Bilbo
 * Entry point: read both spreadsheets, parse, cross-validate, write results.json.
 *
 * Usage: npx ts-node src/parse/index.ts
 */

import * as fs from 'fs';
import * as path from 'path';
import { parsePrizesSheet } from './prizes';
import { parseCompetitorsSheet } from './competitors';

const INPUT_DIR  = path.resolve(__dirname, '../../data/input');
const OUTPUT_FILE = path.resolve(__dirname, '../../data/output/results.json');

async function main() {
  // TODO: load prizes.xlsx and competitors.xlsx using a spreadsheet library (e.g. xlsx / exceljs)
  // const prizesRows       = loadSheet(path.join(INPUT_DIR, 'prizes.xlsx'));
  // const competitorsRows  = loadSheet(path.join(INPUT_DIR, 'competitors.xlsx'));

  // const categories   = parsePrizesSheet(prizesRows);
  // const competitors  = parseCompetitorsSheet(competitorsRows);

  // TODO: cross-validate — every winner in prizes must appear in competitors
  // validateCrossRef(categories, competitors);

  const results = {
    event: {
      name: 'TODO: read from spreadsheet or config',
      year: new Date().getFullYear(),
    },
    categories: [],  // replace with: categories
    competitors: [], // replace with: competitors
  };

  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(results, null, 2), 'utf8');
  console.log(`Wrote ${OUTPUT_FILE}`);
}

main().catch(err => { console.error(err); process.exit(1); });
