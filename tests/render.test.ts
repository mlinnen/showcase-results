/**
 * render.test.ts — Aragorn
 * Validates output/article.html:
 *   1. File exists and is non-empty
 *   2. No disallowed Joomla tags (<html>, <head>, <body>, <script>)
 *   3. Tables have thead + tbody
 *   4. No inline style attributes
 */

import * as fs   from 'fs';
import * as path from 'path';

const ARTICLE_FILE = path.resolve(__dirname, '../output/article.html');

// TODO: choose an HTML parser (e.g. node-html-parser or cheerio) and import here

describe('article.html Joomla safety', () => {
  let html: string;

  beforeAll(() => {
    html = fs.readFileSync(ARTICLE_FILE, 'utf8');
  });

  test('file exists and is non-empty', () => {
    expect(html.trim().length).toBeGreaterThan(0);
  });

  test('no <html> wrapper', () => {
    expect(html).not.toMatch(/<html[\s>]/i);
  });

  test('no <head> section', () => {
    expect(html).not.toMatch(/<head[\s>]/i);
  });

  test('no <body> wrapper', () => {
    expect(html).not.toMatch(/<body[\s>]/i);
  });

  test('no <script> tags', () => {
    expect(html).not.toMatch(/<script[\s>]/i);
  });

  test('no inline style attributes', () => {
    expect(html).not.toMatch(/\sstyle=/i);
  });
});

describe('article.html structure', () => {
  let html: string;

  beforeAll(() => {
    html = fs.readFileSync(ARTICLE_FILE, 'utf8');
  });

  test('contains at least one table', () => {
    expect(html).toMatch(/<table/i);
  });

  test('every table has a thead', () => {
    // TODO: parse HTML and assert each <table> contains <thead>
  });

  test('every table has a tbody', () => {
    // TODO: parse HTML and assert each <table> contains <tbody>
  });

  test('prize winners appear in the article', () => {
    // TODO: load results.json, check each winner.name appears in the HTML
  });
});
