// Screenshot helpers for the user manuals: one look for every picture in every language.
// Write the capture flows (sign in as a role, open a page, mark a control, take the picture) in capture.mjs on top of these.
//
//   const { browser, page } = await open('tr');                 // 1100 x 720 CSS px, scale 2, light theme
//   await page.goto(BASE + '/...'); await settle(page);
//   await mark(page, page.getByRole('button', { name: 'Kaydet' }), 3);   // outline + badge "3" = step 3 of the text
//   await shot(page, 'tr', '03-02-save');                        // whole window  -> content/tr/screens/03-02-save.png
//   await shot(page, 'tr', '03-03-form', { crop: page.locator('form') });   // only that part, with a margin
//   await clearMarks(page);
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
export const VIEWPORT = { width: 1280, height: 720 };   // 1280: the narrowest width at which the lists of deliveries, records and notices need no sideways scrolling      // keep in step with viewport_width in the front matter of manual.md
export const SCALE = 2;                                    // keep in step with screenshot_scale
export const LOCALE = { en: 'en-GB', tr: 'tr-TR', es: 'es-AR', pl: 'pl-PL', lv: 'lv-LV' };
const MARK = '#E0731A';

export async function open(lang, options = {}) {
  const { chromium } = await import('playwright');
  const browser = await chromium.launch();
  const context = await browser.newContext({ viewport: VIEWPORT, deviceScaleFactor: SCALE, locale: LOCALE[lang] || 'en-GB', colorScheme: 'light', reducedMotion: 'reduce', ...options });
  const page = await context.newPage();
  return { browser, context, page };
}

/* Wait until the page is at rest: network quiet, fonts loaded, no animation half-way, no text cursor, no scroll bars in the picture. */
export async function settle(page, extraMs = 150) {
  await page.waitForLoadState('networkidle').catch(() => {});
  await page.addStyleTag({ content: '*,*::before,*::after{animation:none!important;transition:none!important;caret-color:transparent!important;scroll-behavior:auto!important}::-webkit-scrollbar{display:none}' }).catch(() => {});
  await page.evaluate(() => document.fonts.ready).catch(() => {});
  await page.waitForTimeout(extraMs);
}

/* Outline a control and give it the number of the step that mentions it. Call once per control; clearMarks removes them all.
   The badge sits on the top right corner; use at: 'top-left', 'left' or 'right' when that corner hides something the reader needs. */
export async function mark(page, locator, number, { gap = 4, at = 'top-right' } = {}) {
  await locator.scrollIntoViewIfNeeded();
  const box = await locator.boundingBox();
  if (!box) throw new Error('mark: the control is not visible');
  await page.evaluate(({ box, number, gap, color, at }) => {
    const add = (css, text) => { const e = document.createElement('div'); e.className = 'manual-mark'; e.style.cssText = 'position:fixed;z-index:2147483647;pointer-events:none;box-sizing:border-box;' + css; if (text != null) e.textContent = text; document.body.appendChild(e); };
    const x = box.x - gap, y = box.y - gap, w = box.width + 2 * gap, h = box.height + 2 * gap;
    add(`left:${x}px;top:${y}px;width:${w}px;height:${h}px;border:3px solid ${color};border-radius:8px;box-shadow:0 0 0 2px rgba(255,255,255,.9)`);
    const pos = { 'top-right': [x + w - 12, y - 12], 'top-left': [x - 12, y - 12], left: [x - 30, y + h / 2 - 12], right: [x + w + 6, y + h / 2 - 12] }[at] || [x + w - 12, y - 12];
    if (number != null) add(`left:${pos[0]}px;top:${pos[1]}px;width:24px;height:24px;border-radius:50%;background:${color};color:#fff;font:700 14px/24px Arial,Helvetica,sans-serif;text-align:center;box-shadow:0 0 0 2px rgba(255,255,255,.9)`, String(number));
  }, { box, number, gap, color: MARK, at });
}
export const clearMarks = page => page.evaluate(() => document.querySelectorAll('.manual-mark').forEach(e => e.remove()));

/* Take the picture. Without options: the window as the user sees it. crop: a locator, cut to it with a margin (the marks stay inside). */
export async function shot(page, lang, name, { crop, margin = 20 } = {}) {
  const file = path.join(ROOT, 'content', lang, 'screens', `${name}.png`);
  fs.mkdirSync(path.dirname(file), { recursive: true });
  if (!crop) { await page.screenshot({ path: file }); return file; }
  await crop.scrollIntoViewIfNeeded();
  const b = await crop.boundingBox();
  if (!b) throw new Error(`shot ${name}: the part to cut out is not visible`);
  const x = Math.max(0, b.x - margin), y = Math.max(0, b.y - margin);
  const view = page.viewportSize() || VIEWPORT;
  const clip = { x, y, width: Math.min(view.width, b.x + b.width + margin) - x, height: Math.min(view.height, b.y + b.height + margin) - y };
  await page.screenshot({ path: file, clip });
  return file;
}
