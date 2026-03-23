/**
 * article.ts — Frodo
 * Consume data/output/results.json and render output/article.html.
 *
 * Usage: npx ts-node src/render/article.ts
 */

import * as fs   from 'fs';
import * as path from 'path';

const INPUT_FILE  = path.resolve(__dirname, '../../data/output/results.json');
const OUTPUT_FILE = path.resolve(__dirname, '../../output/article.html');
const TEMPLATE    = path.resolve(__dirname, 'templates/article.hbs');

interface Prize { place: number; ribbon: string; winner: string; entry?: string; }
interface PrizeCategory { name: string; prizes: Prize[]; }
interface CompetitorRibbon { category: string; place: number; ribbon: string; }
interface Competitor { name: string; ribbons: CompetitorRibbon[]; }
interface ShowcaseResults {
  event: { name: string; year: number };
  categories: PrizeCategory[];
  competitors: Competitor[];
}

async function main() {
  const results: ShowcaseResults = JSON.parse(fs.readFileSync(INPUT_FILE, 'utf8'));

  // TODO: load template and render with a templating engine (e.g. Handlebars)
  // const template = fs.readFileSync(TEMPLATE, 'utf8');
  // const compiled = Handlebars.compile(template);
  // const html     = compiled(results);

  const html = renderArticle(results); // placeholder — replace with template engine
  fs.writeFileSync(OUTPUT_FILE, html, 'utf8');
  console.log(`Wrote ${OUTPUT_FILE}`);
}

/**
 * Placeholder renderer — replace with Handlebars or similar.
 * Joomla rules: no <html>/<head>/<body>, no inline styles, no <script>.
 */
function renderArticle(data: ShowcaseResults): string {
  // TODO: implement full render
  return `<!-- ${data.event.name} ${data.event.year} showcase results -->\n`;
}

main().catch(err => { console.error(err); process.exit(1); });
