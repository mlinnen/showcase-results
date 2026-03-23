/**
 * competitors.ts — Bilbo
 * Parse the competitors spreadsheet into an array of Competitor objects.
 *
 * Expected sheet columns (row 1 = header):
 *   Competitor | Category | Place | Ribbon
 */

export interface CompetitorRibbon {
  category: string;
  place: number;
  ribbon: string;
}

export interface Competitor {
  name: string;
  ribbons: CompetitorRibbon[];
}

/**
 * Parse raw sheet rows into Competitor[].
 * @param rows - 2D array from the spreadsheet parser (header row excluded)
 */
export function parseCompetitorsSheet(rows: string[][]): Competitor[] {
  // TODO: implement
  // Group rows by Competitor column, map Category/Place/Ribbon into CompetitorRibbon objects
  throw new Error('Not implemented');
}
