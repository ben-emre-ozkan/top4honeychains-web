// Renders the partners and supporters board (board.html) to board_<lang>.jpg for every language.
// Only needed when institutions.json or a logo changes: the JPG files in this folder are already rendered.
// usage: node assets/board/render_board.mjs [lang ...]        (uses the Playwright that is installed in capture/)
import { readFileSync } from 'node:fs';
import { fileURLToPath, pathToFileURL } from 'node:url';
import path from 'node:path';
import { createRequire } from 'node:module';

const here = path.dirname(fileURLToPath(import.meta.url));
let chromium;
for (const from of [path.join(here, '..', '..', 'capture', 'package.json'), import.meta.url]) {      // Playwright is installed for the screenshots, in capture/
  try { ({ chromium } = createRequire(from)('playwright')); break; } catch { /* try the next place */ }
}
if (!chromium) { console.error('The "playwright" package was not found. Run in the capture folder: npm install, then npx playwright install chromium'); process.exit(1); }

const data = JSON.parse(readFileSync(path.join(here, '..', 'institutions.json'), 'utf8'));
const langs = process.argv.slice(2).length ? process.argv.slice(2) : ['en', 'tr', 'es', 'pl', 'lv'];
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1000, height: 900 }, deviceScaleFactor: 3 });
await page.goto(pathToFileURL(path.join(here, 'board.html')).href);
for (const lang of langs) {
  const size = await page.evaluate(([d, l]) => build(d, l), [data, lang]);
  const out = path.join(here, `board_${lang}.jpg`);
  await page.locator('#board').screenshot({ path: out, type: 'jpeg', quality: 93 });
  console.log(`${path.basename(out)}  ${size.width} x ${size.height} css px`);
}
await browser.close();
