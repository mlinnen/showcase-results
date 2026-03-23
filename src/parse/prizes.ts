/**
 * prizes.ts — Bilbo
 * Parse the prizes spreadsheet into an array of PrizeCategory objects.
 *
 * Expected sheet columns (row 1 = header):
 *   Category | Place | Ribbon | Winner | Entry (optional)
 */

export interface Prize {
  place: number;
  ribbon: string;
  winner: string;
  entry?: string;
}

export interface PrizeCategory {
  name: string;
  prizes: Prize[];
}

/**
 * Parse raw sheet rows into PrizeCategory[].
 * @param rows - 2D array from the spreadsheet parser (header row excluded)
 */
export function parsePrizesSheet(rows: string[][]): PrizeCategory[] {
  // TODO: implement
  // Group rows by Category column, map Place/Ribbon/Winner/Entry into Prize objects
  throw new Error('Not implemented');
}
