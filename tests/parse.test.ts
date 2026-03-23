/**
 * parse.test.ts — Aragorn
 * Validates:
 *   1. results.json conforms to schema/results.schema.json
 *   2. Every prize winner appears in the competitors list (cross-sheet integrity)
 *   3. No duplicate place numbers within a category
 */

import * as fs   from 'fs';
import * as path from 'path';

const RESULTS_FILE = path.resolve(__dirname, '../data/output/results.json');
const SCHEMA_FILE  = path.resolve(__dirname, '../schema/results.schema.json');

// TODO: choose a JSON Schema validator (e.g. ajv) and import here

describe('results.json schema validation', () => {
  let results: any;

  beforeAll(() => {
    results = JSON.parse(fs.readFileSync(RESULTS_FILE, 'utf8'));
  });

  test('file exists and is valid JSON', () => {
    expect(results).toBeDefined();
  });

  test('conforms to results.schema.json', () => {
    // TODO: validate with ajv
    // const schema = JSON.parse(fs.readFileSync(SCHEMA_FILE, 'utf8'));
    // const ajv = new Ajv();
    // const valid = ajv.validate(schema, results);
    // expect(valid).toBe(true);
  });

  test('event has name and year', () => {
    expect(results.event).toHaveProperty('name');
    expect(results.event).toHaveProperty('year');
  });
});

describe('cross-sheet integrity', () => {
  let results: any;

  beforeAll(() => {
    results = JSON.parse(fs.readFileSync(RESULTS_FILE, 'utf8'));
  });

  test('every prize winner exists in competitors list', () => {
    const competitorNames = new Set(results.competitors.map((c: any) => c.name));
    for (const cat of results.categories) {
      for (const prize of cat.prizes) {
        expect(competitorNames).toContain(prize.winner);
      }
    }
  });

  test('no duplicate places within a category', () => {
    for (const cat of results.categories) {
      const places = cat.prizes.map((p: any) => p.place);
      const unique  = new Set(places);
      expect(unique.size).toBe(places.length);
    }
  });
});
