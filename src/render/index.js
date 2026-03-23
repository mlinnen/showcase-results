/**
 * src/render/index.js — Frodo (Content Builder)
 * Reads data/output/results.json and writes output/article.html.
 * Usage: node src/render/index.js
 */

'use strict';

const fs   = require('fs');
const path = require('path');

const INPUT_FILE  = path.resolve(__dirname, '../../data/output/results.json');
const OUTPUT_FILE = path.resolve(__dirname, '../../output/article.html');

const data = JSON.parse(fs.readFileSync(INPUT_FILE, 'utf8'));

// ── Helpers ───────────────────────────────────────────────────────────────────

function esc(str) {
  if (str == null) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

/** Format a style code: null → '', 'N' → 'Natural', 'P' → 'Painted' */
function styleLabel(style) {
  if (!style) return '';
  if (style === 'N') return 'Natural';
  if (style === 'P') return 'Painted';
  return esc(style);
}

/** Format a place entry as "Full Name (#entry)" or "Full Name" if no entry */
function placeCell(placeObj) {
  if (!placeObj) return '';
  const name  = esc(placeObj.winner);
  const entry = placeObj.entry_number;
  return entry ? `${name} <span class="cca-entry">(#${entry})</span>` : name;
}

/** Get place object by rank from a places array */
function getPlace(places, rank) {
  return (places || []).find(p => p.place === rank) || null;
}

// ── Section renderers ─────────────────────────────────────────────────────────

function renderSpecialPrizes(prizes) {
  const sorted = [...prizes].sort((a, b) => a.order - b.order);

  const rows = sorted.map(p => {
    const entry = p.entry_number ? `<span class="cca-entry">(#${p.entry_number})</span>` : '';
    
    let prizeValue = '';
    if (p.prize) {
      const trimmed = String(p.prize).trim();
      if (/^\d+(\.\d{1,2})?$/.test(trimmed)) {
        const amount = parseFloat(trimmed);
        prizeValue = `$${amount}`;
      } else {
        prizeValue = esc(trimmed);
      }
    }
    
    return `    <tr>
      <td>${esc(p.name)}</td>
      <td>${esc(p.winner)}</td>
      <td>${entry}</td>
      <td>${prizeValue}</td>
    </tr>`;
  }).join('\n');

  return `<h2>Special Prizes</h2>
<table class="cca-special-prizes">
  <thead>
    <tr>
      <th style="text-align:left">Name</th>
      <th style="text-align:left">Winner</th>
      <th style="text-align:left">Entry</th>
      <th style="text-align:left">Prize</th>
    </tr>
  </thead>
  <tbody>
${rows}
  </tbody>
</table>`;
}

function renderOverallResults(overallResults) {
  const rows = overallResults.map(cat => {
    const first  = getPlace(cat.places, 1);
    const second = getPlace(cat.places, 2);
    const third  = getPlace(cat.places, 3);
    return `    <tr>
      <td>${esc(cat.category)}</td>
      <td>${placeCell(first)}</td>
      <td>${placeCell(second)}</td>
      <td>${placeCell(third)}</td>
    </tr>`;
  }).join('\n');

  return `<h2>Overall Results</h2>
<table class="cca-overall-results">
  <thead>
    <tr>
      <th style="text-align:left">Category</th>
      <th style="text-align:left">1st Place</th>
      <th style="text-align:left">2nd Place</th>
      <th style="text-align:left">3rd Place</th>
    </tr>
  </thead>
  <tbody>
${rows}
  </tbody>
</table>`;
}

function renderDivisionAwardsTable(awardCats) {
  const rows = awardCats.map(cat => {
    const first = getPlace(cat.places, 1);
    return `    <tr>
      <td>${esc(cat.name)}</td>
      <td>${placeCell(first)}</td>
    </tr>`;
  }).join('\n');

  return `<table class="cca-division-awards">
  <thead>
    <tr>
      <th style="text-align:left">Award</th>
      <th style="text-align:left">Winner</th>
    </tr>
  </thead>
  <tbody>
${rows}
  </tbody>
</table>`;
}

function renderDivisionCategoryTable(categoryCats) {
  const rows = categoryCats.map(cat => {
    const style  = styleLabel(cat.style);
    const first  = getPlace(cat.places, 1);
    const second = getPlace(cat.places, 2);
    const third  = getPlace(cat.places, 3);
    return `    <tr>
      <td>${esc(cat.name)}</td>
      <td>${style}</td>
      <td>${placeCell(first)}</td>
      <td>${placeCell(second)}</td>
      <td>${placeCell(third)}</td>
    </tr>`;
  }).join('\n');

  return `<table class="cca-category-results">
  <thead>
    <tr>
      <th style="text-align:left">Category</th>
      <th style="text-align:left">Style</th>
      <th style="text-align:left">1st Place</th>
      <th style="text-align:left">2nd Place</th>
      <th style="text-align:left">3rd Place</th>
    </tr>
  </thead>
  <tbody>
${rows}
  </tbody>
</table>`;
}

function renderDivisionResults(divisionResults) {
  const sections = divisionResults.map(div => {
    const awardCats    = div.categories.filter(c => c.style === null);
    const categoryCats = div.categories.filter(c => c.style !== null);

    const awardsHtml   = awardCats.length    ? `\n<h4>Division Awards</h4>\n${renderDivisionAwardsTable(awardCats)}\n`    : '';
    const categoryHtml = categoryCats.length ? `\n<h4>Category Results</h4>\n${renderDivisionCategoryTable(categoryCats)}\n` : '';

    return `<h3>${esc(div.division)} Division</h3>${awardsHtml}${categoryHtml}`;
  }).join('\n');

  return `<h2>Division Results</h2>\n${sections}`;
}

// ── Assemble article ──────────────────────────────────────────────────────────

function renderArticle(data) {
  const { event, special_prizes, overall_results, division_results } = data;

  const header  = `<!-- ${esc(event.name)} ${event.year} showcase results -- generated by src/render/index.js -->`;
  const title   = `<h2>${event.year} ${esc(event.name)} Results</h2>`;
  const special = renderSpecialPrizes(special_prizes);
  const overall = renderOverallResults(overall_results);
  const divs    = renderDivisionResults(division_results);

  return [header, title, special, overall, divs].join('\n\n') + '\n';
}

// ── Run ───────────────────────────────────────────────────────────────────────

const html = renderArticle(data);
fs.writeFileSync(OUTPUT_FILE, html, 'utf8');
console.log(`Wrote ${OUTPUT_FILE}`);
