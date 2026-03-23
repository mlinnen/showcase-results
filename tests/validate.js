/**
 * tests/validate.js — Aragorn (Tester)
 * Comprehensive validation of pipeline outputs:
 *   - data/output/results.json  (Bilbo's parser output)
 *   - output/article.html       (Frodo's rendered article)
 *
 * Usage: node tests/validate.js
 * Exit 0 = all checks pass. Exit 1 = one or more failures.
 */

'use strict';

const fs   = require('fs');
const path = require('path');
const Ajv  = require('ajv');

// ── File paths ─────────────────────────────────────────────────────────────────
const ROOT        = path.resolve(__dirname, '..');
const RESULTS_JSON = path.join(ROOT, 'data', 'output', 'results.json');
const SCHEMA_JSON  = path.join(ROOT, 'schema', 'results.schema.json');
const ARTICLE_HTML = path.join(ROOT, 'output', 'article.html');

// ── Test runner ────────────────────────────────────────────────────────────────
let passed = 0;
let failed = 0;
const failures = [];

function check(label, fn) {
  try {
    const result = fn();
    if (result === false) {
      console.log(`  FAIL  ${label}`);
      failed++;
      failures.push(label);
    } else {
      console.log(`  PASS  ${label}`);
      passed++;
    }
  } catch (err) {
    console.log(`  FAIL  ${label}`);
    console.log(`        Error: ${err.message}`);
    failed++;
    failures.push(`${label}: ${err.message}`);
  }
}

// ── Load files ─────────────────────────────────────────────────────────────────
let results, schema, html;

try {
  results = JSON.parse(fs.readFileSync(RESULTS_JSON, 'utf8'));
} catch (e) {
  console.error(`FATAL: Could not read ${RESULTS_JSON}: ${e.message}`);
  process.exit(1);
}

try {
  schema = JSON.parse(fs.readFileSync(SCHEMA_JSON, 'utf8'));
} catch (e) {
  console.error(`FATAL: Could not read ${SCHEMA_JSON}: ${e.message}`);
  process.exit(1);
}

try {
  html = fs.readFileSync(ARTICLE_HTML, 'utf8');
} catch (e) {
  console.error(`FATAL: Could not read ${ARTICLE_HTML}: ${e.message}`);
  process.exit(1);
}

// ── Build carver lookup ────────────────────────────────────────────────────────
const carverById = {};
for (const c of results.competitors) {
  carverById[c.carver_id] = `${c.first_name} ${c.last_name}`;
}

// ══════════════════════════════════════════════════════════════════════════════
// SECTION 1: JSON Validation — results.json
// ══════════════════════════════════════════════════════════════════════════════

console.log('\n── JSON Validation: data/output/results.json ──\n');

// 1.1 Schema conformance
check('JSON conforms to results.schema.json (AJV)', () => {
  const ajv = new Ajv({ strict: false });
  const validate = ajv.compile(schema);
  const valid = validate(results);
  if (!valid) {
    const errs = validate.errors.map(e => `${e.instancePath} ${e.message}`).join('; ');
    throw new Error(`Schema errors: ${errs}`);
  }
});

// 1.2 Count: exactly 33 special prizes
check('special_prizes count = 33', () => results.special_prizes.length === 33);

// 1.3 Count: exactly 37 competitors
check('competitors count = 37', () => results.competitors.length === 37);

// 1.4 Count: exactly 3 divisions
check('division_results count = 3', () => results.division_results.length === 3);

// 1.5 Division names are exactly the allowed set
check('division names are Intermediate/Novice/Open', () => {
  const allowed = new Set(['Intermediate', 'Novice', 'Open']);
  const divNames = results.division_results.map(d => d.division);
  const invalid = divNames.filter(n => !allowed.has(n));
  if (invalid.length) throw new Error(`Invalid divisions: ${invalid.join(', ')}`);
});

// 1.6 All 3 expected divisions present
check('all three divisions present (Intermediate, Novice, Open)', () => {
  const divNames = new Set(results.division_results.map(d => d.division));
  return divNames.has('Intermediate') && divNames.has('Novice') && divNames.has('Open');
});

// 1.7 style values: only "N", "P", or null — never empty string
check('style values are only "N", "P", or null (never empty string)', () => {
  const bad = [];
  for (const div of results.division_results) {
    for (const cat of div.categories) {
      if (cat.style !== null && cat.style !== 'N' && cat.style !== 'P') {
        bad.push(`${div.division}/${cat.name}: style="${cat.style}"`);
      }
    }
  }
  if (bad.length) throw new Error(`Bad styles: ${bad.join('; ')}`);
});

// 1.8 carver_id is always an integer in special_prizes
check('special_prizes: carver_id is always integer', () => {
  const bad = results.special_prizes.filter(p => !Number.isInteger(p.carver_id));
  if (bad.length) throw new Error(`Non-integer carver_ids: ${bad.map(p => p.name).join(', ')}`);
});

// 1.9 carver_id is always an integer in division_results
check('division_results: carver_id is always integer', () => {
  const bad = [];
  for (const div of results.division_results) {
    for (const cat of div.categories) {
      for (const place of cat.places) {
        if (!Number.isInteger(place.carver_id)) {
          bad.push(`${div.division}/${cat.name}/place ${place.place}: carver_id=${place.carver_id}`);
        }
      }
    }
  }
  if (bad.length) throw new Error(bad.join('; '));
});

// 1.10 prize in special_prizes is always a string
check('special_prizes: prize is always a string', () => {
  const bad = results.special_prizes.filter(p => typeof p.prize !== 'string');
  if (bad.length) throw new Error(`Non-string prizes: ${bad.map(p => p.name).join(', ')}`);
});

// 1.11 Carver 16 (Erik Mitchell) data quality check
check('carver 16 (Erik Mitchell) absent from special_prizes (entry_number=0 known issue, correctly omitted)', () => {
  const inSpecial = results.special_prizes.some(p => p.carver_id === 16);
  // carver 16 should NOT be in special_prizes per data quality note
  if (inSpecial) throw new Error('Carver 16 unexpectedly appears in special_prizes');
});

check('carver 16 (Erik Mitchell) is in competitors list', () => {
  const inCompetitors = results.competitors.some(c => c.carver_id === 16);
  if (!inCompetitors) throw new Error('Carver 16 missing from competitors list entirely');
});

// 1.12 special_prizes order values are unique
check('special_prizes: order values are unique', () => {
  const orders = results.special_prizes.map(p => p.order);
  const unique = new Set(orders);
  if (unique.size !== orders.length) {
    const dups = orders.filter((o, i) => orders.indexOf(o) !== i);
    throw new Error(`Duplicate order values: ${[...new Set(dups)].join(', ')}`);
  }
});

// 1.13 All winner names in special_prizes are non-empty strings (not raw IDs)
check('special_prizes: winner is always a non-empty string', () => {
  const bad = results.special_prizes.filter(p => typeof p.winner !== 'string' || p.winner.trim() === '');
  if (bad.length) throw new Error(`Bad winners: ${bad.map(p => p.name).join(', ')}`);
});

// 1.14 No duplicate place numbers within a single category
check('division_results: no duplicate place numbers within a category', () => {
  const bad = [];
  for (const div of results.division_results) {
    for (const cat of div.categories) {
      const places = cat.places.map(p => p.place);
      const unique = new Set(places);
      if (unique.size !== places.length) {
        bad.push(`${div.division}/${cat.name}: duplicate places [${places.join(',')}]`);
      }
    }
  }
  if (bad.length) throw new Error(bad.join('; '));
});

// 1.15 Duplicate competitor names (data quality — same name, different IDs)
check('competitors: no duplicate full names (data quality warning)', () => {
  const nameCount = {};
  for (const c of results.competitors) {
    const fullName = `${c.first_name} ${c.last_name}`;
    nameCount[fullName] = (nameCount[fullName] || 0) + 1;
  }
  const dupes = Object.entries(nameCount).filter(([, cnt]) => cnt > 1);
  if (dupes.length) {
    throw new Error(`Duplicate names: ${dupes.map(([n, cnt]) => `"${n}" x${cnt}`).join(', ')}`);
  }
});

// ══════════════════════════════════════════════════════════════════════════════
// SECTION 2: HTML Validation — output/article.html
// ══════════════════════════════════════════════════════════════════════════════

console.log('\n── HTML Validation: output/article.html ──\n');

// 2.1 ADR-002: No <html> tag
check('ADR-002: no <html> tag (fragment only)', () => !/<html[\s>]/i.test(html));

// 2.2 ADR-002: No <head> tag
check('ADR-002: no <head> tag', () => !/<head[\s>]/i.test(html));

// 2.3 ADR-002: No <body> tag
check('ADR-002: no <body> tag', () => !/<body[\s>]/i.test(html));

// 2.4 ADR-002: No inline styles
check('ADR-002: no inline style= attributes', () => !/\sstyle=/i.test(html));

// 2.5 ADR-002: No <script> tags
check('ADR-002: no <script> tags', () => !/<script[\s>]/i.test(html));

// 2.6 ADR-002: No <link> tags
check('ADR-002: no <link> tags', () => !/<link[\s>]/i.test(html));

// 2.7 All 33 special prize names appear (with HTML entity encoding)
check('all 33 special prize names appear in article', () => {
  const missing = results.special_prizes
    .filter(p => {
      const encoded = p.name.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
      return !html.includes(encoded) && !html.includes(p.name);
    })
    .map(p => p.name);
  if (missing.length) throw new Error(`Missing prizes: ${missing.join('; ')}`);
});

// 2.8 All 3 divisions appear
check('all 3 divisions appear in article (Intermediate, Novice, Open)', () => {
  const missing = ['Intermediate', 'Novice', 'Open'].filter(d => !html.includes(d));
  if (missing.length) throw new Error(`Missing divisions: ${missing.join(', ')}`);
});

// 2.9 Place labels present
check('place labels 1st, 2nd, 3rd appear in article', () => {
  const missing = ['1st', '2nd', '3rd'].filter(p => !html.includes(p));
  if (missing.length) throw new Error(`Missing place labels: ${missing.join(', ')}`);
});

// 2.10 No raw carver_id field names in HTML
check('no raw "carver_id" text in HTML', () => !html.includes('carver_id'));

// 2.11 Winner names are resolved full names (not bare numeric IDs in winner positions)
check('winner cells contain no bare numeric IDs (e.g. <td>4</td>)', () => {
  // Look for table cells that contain only a raw number — would indicate unresolved carver_id
  const bareNumCell = /<td>\s*\d+\s*<\/td>/g;
  // Entry cells like "#37" are expected, but bare integers like "4" are not
  const matches = html.match(/<td>\s*\d+\s*<\/td>/g) || [];
  // Filter out entry-number-only cells (these wouldn't appear because entries use "#N" format)
  if (matches.length) throw new Error(`Bare numeric cells found: ${matches.slice(0,3).join(', ')}`);
});

// 2.12 Special prize winner names from JSON appear in HTML
check('special prize winners are resolved full names in HTML', () => {
  const missing = [];
  for (const p of results.special_prizes) {
    const encodedName = p.winner.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    if (!html.includes(encodedName) && !html.includes(p.winner)) {
      missing.push(`${p.name} → "${p.winner}"`);
    }
  }
  if (missing.length) throw new Error(`Missing winners: ${missing.join('; ')}`);
});

// 2.13 Prize amounts/descriptions are rendered in the special prizes table
check('special prize amounts/descriptions appear in article (prize field rendered)', () => {
  const missing = [];
  for (const p of results.special_prizes) {
    const encodedPrize = p.prize.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    if (!html.includes(encodedPrize) && !html.includes(p.prize)) {
      missing.push(`"${p.name}": prize="${p.prize}"`);
    }
  }
  if (missing.length > 0) {
    throw new Error(`Prize values not rendered in HTML (${missing.length} missing). Examples: ${missing.slice(0,3).join('; ')}`);
  }
});

// 2.14 Division category tables have 1st/2nd/3rd place columns
check('division category tables include 1st/2nd/3rd place column headers', () => {
  const hasFirst  = html.includes('1st Place');
  const hasSecond = html.includes('2nd Place');
  const hasThird  = html.includes('3rd Place');
  if (!hasFirst || !hasSecond || !hasThird) {
    const missing = ['1st Place', '2nd Place', '3rd Place'].filter(h => !html.includes(h));
    throw new Error(`Missing column headers: ${missing.join(', ')}`);
  }
});

// 2.15 HTML is non-empty
check('article.html is non-empty', () => html.trim().length > 0);

// ══════════════════════════════════════════════════════════════════════════════
// RESULTS
// ══════════════════════════════════════════════════════════════════════════════

const total = passed + failed;
console.log(`\n${'─'.repeat(60)}`);
console.log(`Results: ${passed}/${total} checks passed, ${failed} failed`);
console.log(`${'─'.repeat(60)}`);

if (failed > 0) {
  console.log('\nFailed checks:');
  failures.forEach((f, i) => console.log(`  ${i + 1}. ${f}`));
  console.log('');
  process.exit(1);
} else {
  console.log('\nAll checks passed.\n');
  process.exit(0);
}
