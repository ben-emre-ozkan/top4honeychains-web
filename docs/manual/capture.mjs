// Captures the user-manual screenshots listed in shots.yml from the LOCAL demo application, in every language:
//   node capture.mjs [lang ...] [--only id,id] [--probe id]
// - Base URL http://localhost:5255 (MANUAL_BASE overrides it; never point it at the live site).
// - Credentials are read at run time from the untracked demo_users.txt at the repository root and kept in memory only.
// - Only [DEMO] records are shown: account names and e-mail addresses are replaced on the page before capture, list rows
//   that are not [DEMO] are blurred, and a privacy check refuses to save an image that still shows an account name.
// - Stable images: reduced motion, network idle, loader gone, no caret; highlights are a honey-coloured outline added as CSS.
// Output: img/<lang>/<id>.png (desktop 1440 x 900 at scale 1, phone 390 x 844 at scale 2).
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
export const BASE = (process.env.MANUAL_BASE || 'http://localhost:5255').replace(/\/$/, '');
if (!/^http:\/\/localhost(:\d+)?$/.test(BASE)) throw new Error('capture.mjs runs against the local demo application only');
export const LANGS = ['en', 'tr', 'es', 'pl', 'lv'];
const TRACE = !!process.env.MANUAL_TRACE;
const t0 = Date.now();
export const trace = (...a) => { if (TRACE) console.log(`  [${((Date.now() - t0) / 1000).toFixed(1)}s]`, ...a); };
// page.evaluate has no timeout of its own: in-page waits go through this
const within = (promise, ms, what) => Promise.race([promise, new Promise((_, reject) => setTimeout(() => reject(new Error(`${what} took longer than ${ms / 1000} s`)), ms))]);

// ---- credentials: the box-drawn table in demo_users.txt (user name | organisation | roles | password)
let accounts;
export function credentials() {
    if (accounts) return accounts;
    const file = path.join(repo, 'demo_users.txt');
    if (!fs.existsSync(file)) throw new Error('demo_users.txt not found at the repository root');
    accounts = [];
    for (const line of fs.readFileSync(file, 'utf8').split(/\r?\n/)) {
        if (!line.startsWith('│')) continue;
        const cells = line.split('│').slice(1, -1).map(c => c.trim());
        if (cells.length < 4 || !cells[3] || /^user name$/i.test(cells[0])) continue;
        accounts.push({ user: cells[0], org: cells[1], roles: cells[2], password: cells[3] });
    }
    if (!accounts.length) throw new Error('no accounts found in demo_users.txt');
    return accounts;
}
// who: { org: 'Kovan', role: 'Staff' } or 'admin'
export function account(who) {
    const list = credentials();
    const found = who === 'admin'
        ? list.find(a => /administrator/i.test(a.roles))
        : list.find(a => a.org.includes(who.org) && a.roles.split(/[+,]/).map(r => r.trim()).includes(who.role));
    if (!found) throw new Error(`no demo account for ${JSON.stringify(who)}`);
    return found;
}
export function accountNames() { return credentials().map(a => a.user); }

// ---- lookups in the local database ([DEMO] records only)
export function sql(query) {
    const out = execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', 'TOP4HoneyChainsDb', '-I', '-W', '-h', '-1', '-s', '|', '-f', '65001',
        '-Q', 'SET NOCOUNT ON; ' + query], { encoding: 'utf8' });
    return out.split(/\r?\n/).filter(l => l.trim() && !/^\(\d+ rows? affected\)$/.test(l)).map(l => l.split('|'));
}
export function one(query) { const r = sql(query); return r.length && r[0][0] !== 'NULL' ? r[0][0] : null; }
const demo = col => `${col} LIKE N'\\[DEMO\\]%' ESCAPE '\\'`;
export const like = (col, pattern) => `${col} LIKE N'${pattern.replace(/\[DEMO\]/g, '\\[DEMO\\]')}' ESCAPE '\\'`;

export function ids() {
    const batch = pattern => one(`SELECT FinalHoneyProductionId FROM FinalHoneyProductions WHERE ${like('FinalHoneyProductionTitle', pattern)}`);
    const qr = pattern => one(`SELECT LOWER(CAST(FinalHoneyProductionQR AS varchar(36))) FROM FinalHoneyProductions WHERE ${like('FinalHoneyProductionTitle', pattern)}`);
    const org = pattern => one(`SELECT OrgId FROM Organizations WHERE ${like('DisplayName', pattern)}`);
    const lot = amount => one(`SELECT TOP 1 r.RawHoneyProductionId FROM RawHoneyProductions r JOIN Apiaries a ON a.ApiaryId = r.RawHoneyProductionApiaryId
        WHERE ${demo('a.ApiaryTitle')} AND r.RawHoneyProductionAmount = ${amount} ORDER BY r.RawHoneyProductionId DESC`);
    const ccp = code => one(`SELECT Id FROM CriticalControlPoints WHERE Code = '${code}'`);
    const r = {
        kekik: batch('[DEMO] Kekik Bal%'), kekikQr: qr('[DEMO] Kekik Bal%'), yayla: batch('[DEMO] Yayla Bal%'),
        open: batch('[DEMO] Orman Bal%'), requested: batch('[DEMO] Yayla _i_ek%'),
        processor: org('[DEMO] Kovan%'), lab: org('[DEMO] Analiz%'), association: org('[DEMO] Ar_c_lar%'), market: org('[DEMO] %Marketi'),
        lotSubmitted: lot(61.25), lotNew: lot(62.5), lotLab: lot(63.75),
        ccp0: ccp('CCP 0'), ccp4: ccp('CCP 4'), ccp10: ccp('CCP 10'),
        zeynep: one(`SELECT BeekeeperId FROM Beekeepers WHERE ${like('BeekeeperFirstName', '[DEMO] Zeynep%')}`),
        zeynepNumber: one(`SELECT BeekeeperBusinessNumber FROM Beekeepers WHERE ${like('BeekeeperFirstName', '[DEMO] Zeynep%')}`),
        noConsent: one(`SELECT TOP 1 b.BeekeeperId FROM Beekeepers b WHERE ${demo('b.BeekeeperFirstName')}
            AND NOT EXISTS (SELECT 1 FROM Consents c WHERE c.BeekeeperId = b.BeekeeperId AND c.WithdrawnAt IS NULL)`),
        moisture: one(`SELECT TOP 1 p.ParameterId FROM CriticalControlPointParameters p JOIN CriticalControlPoints c ON c.Id = p.CCPId
            WHERE c.Code = 'CCP 1' AND p.ParameterName = 'Moisture'`),
    };
    const sampleOf = (lotId, status) => lotId && one(`SELECT TOP 1 SampleId FROM Samples WHERE RawHoneyProductionId = ${lotId}${status ? ` AND Status = '${status}'` : ''} ORDER BY SampleId DESC`);
    const demoLots = `SELECT r.RawHoneyProductionId FROM RawHoneyProductions r JOIN Apiaries a ON a.ApiaryId = r.RawHoneyProductionApiaryId WHERE ${demo('a.ApiaryTitle')}`;
    r.sampleSent = sampleOf(r.lotSubmitted, 'Sent');
    r.sampleSubmitted = one(`SELECT TOP 1 l.SampleId FROM LabReports l JOIN Samples s ON s.SampleId = l.SampleId WHERE l.Status = 'Submitted' AND s.RawHoneyProductionId IN (${demoLots})`);
    r.sampleDraft = one(`SELECT TOP 1 l.SampleId FROM LabReports l JOIN Samples s ON s.SampleId = l.SampleId WHERE l.Status = 'Draft' AND s.RawHoneyProductionId IN (${demoLots})`);
    r.reportCode = one(`SELECT TOP 1 l.ReportCode FROM LabReports l JOIN Samples s ON s.SampleId = l.SampleId WHERE l.Status = 'Approved' AND l.Sha256 NOT LIKE 'dddd%' AND s.RawHoneyProductionId IN (${demoLots}) ORDER BY l.ReportId DESC`);
    r.demoBatchIds = sql(`SELECT FinalHoneyProductionId FROM FinalHoneyProductions WHERE ${demo('FinalHoneyProductionTitle')}`).map(x => x[0]);
    return r;
}

// ---- browser
export async function launch(lang = 'en') {
    const { chromium } = await import('playwright-core');
    for (const options of [{ channel: 'chrome' }, { channel: 'msedge' }, {}]) {
        try { return await chromium.launch({ headless: !HEADED, args: [`--lang=${lang}`, ...(HEADED ? ['--start-maximized'] : [])], env: { ...process.env, LANGUAGE: lang }, ...options }); } catch { /* next */ }
    }
    throw new Error('no Chrome, Edge or Chromium found for Playwright');
}

const HEADED = process.argv.includes('--headed') || !!process.env.MANUAL_HEADED;
const sessions = new Map();   // signed-in cookies per user and language, in memory only
export async function session(browser, { who, lang = 'en', viewport = 'desktop' }) {
    const phone = viewport === 'phone';
    const context = await browser.newContext({
        viewport: phone ? { width: 390, height: 844 } : { width: 1440, height: 900 },
        deviceScaleFactor: phone ? 2 : 1, isMobile: phone, hasTouch: phone,
        reducedMotion: 'reduce', locale: lang, timezoneId: 'Europe/Istanbul', acceptDownloads: false,
    });
    const key = JSON.stringify([who, lang]);
    if (sessions.has(key)) await context.addCookies(sessions.get(key));
    const page = await context.newPage();
    page.setDefaultTimeout(10000);
    if (!sessions.has(key)) {
        await page.goto(`${BASE}/Home/SetLanguage?culture=${lang}&returnUrl=${encodeURIComponent('/Account/Login')}`);
        if (who && who !== 'anonymous') {
            const a = account(who);
            trace('sign in', typeof who === 'string' ? who : `${who.role}@${who.org}`, lang);
            await page.goto(`${BASE}/Account/Login`);
            await page.fill('#UserName', a.user);
            await page.fill('#Password', a.password);
            await Promise.all([page.waitForURL(u => !/\/Account\/Login/i.test(u.pathname)), page.click('.au-submit')]);
        }
        sessions.set(key, await context.cookies());
    }
    return { context, page };
}

export async function settle(page) {
    await page.waitForLoadState('networkidle', { timeout: 8000 }).catch(() => trace('network not idle after 8 s'));
    await within(page.evaluate(() => document.fonts.ready), 5000, 'fonts').catch(e => trace(e.message));
    await page.waitForFunction(() => {
        const l = document.getElementById('app-loader');
        return !l || getComputedStyle(l).display === 'none' || getComputedStyle(l).visibility === 'hidden' || getComputedStyle(l).opacity === '0' || l.hidden;
    }).catch(() => {});
    await page.waitForTimeout(250);
}

export async function go(page, url) {
    await page.goto(BASE + url);
    await settle(page);
}

// ---- privacy: account names and e-mail addresses never reach an image
const EMAIL = /[\w.+-]+@[\w-]+(\.[\w-]+)+/;
export async function scrub(page) {
    await page.evaluate(names => {
        const email = /[\w.+-]+@[\w-]+(\.[\w-]+)+/g;
        const esc = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const nameRe = new RegExp(`(^|[^\\w])(${names.map(esc).join('|')})(?![\\w])`, 'gi');
        const fix = t => t.replace(email, '[DEMO] e-mail').replace(nameRe, (m, pre) => `${pre}[DEMO] user`);
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        for (let n = walker.nextNode(); n; n = walker.nextNode()) { const v = fix(n.nodeValue); if (v !== n.nodeValue) n.nodeValue = v; }
        for (const el of document.querySelectorAll('input, textarea')) {
            if (['password', 'file', 'hidden', 'checkbox', 'radio'].includes(el.type)) continue;
            const v = fix(el.value); if (v !== el.value) el.value = v;
        }
        for (const el of document.querySelectorAll('[title], [aria-label], [placeholder]'))
            for (const a of ['title', 'aria-label', 'placeholder']) if (el.hasAttribute(a)) el.setAttribute(a, fix(el.getAttribute(a)));
    }, accountNames());
}
// list rows that are not [DEMO] records are blurred; keep = substrings that mark an allowed row
export async function blurRows(page, rows, keep = ['[DEMO]']) {
    return page.evaluate(({ rows, keep }) => {
        let blurred = 0;
        for (const r of document.querySelectorAll(rows)) {
            if (keep.some(k => r.innerText.includes(k))) continue;
            r.style.filter = 'blur(7px)'; blurred++;
        }
        return blurred;
    }, { rows, keep });
}
async function privacyCheck(page, clip) {
    const leaks = await page.evaluate(({ names, clip }) => {
        const inClip = el => { if (!clip) return true; const b = el.getBoundingClientRect(); return b.bottom > clip.y && b.top < clip.y + clip.height && b.right > clip.x && b.left < clip.x + clip.width; };
        const esc = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const nameRe = new RegExp(`(^|[^\\w])(${names.map(esc).join('|')})(?![\\w])`, 'i');
        const out = [];
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        for (let n = walker.nextNode(); n; n = walker.nextNode()) {
            const el = n.parentElement;
            if (!el || !el.getClientRects().length || !inClip(el) || el.closest('[style*="blur"]')) continue;
            if (/[\w.+-]+@[\w-]+\.[\w.-]+/.test(n.nodeValue)) out.push('e-mail: ' + n.nodeValue.trim().slice(0, 3) + '…');
            if (nameRe.test(n.nodeValue)) out.push('account name');
        }
        for (const el of document.querySelectorAll('input, textarea'))
            if (!['password', 'hidden', 'file'].includes(el.type) && el.getClientRects().length && inClip(el) && nameRe.test(el.value)) out.push('account name in a field');
        return [...new Set(out)];
    }, { names: accountNames(), clip });
    if (leaks.length) throw new Error('privacy check failed: ' + leaks.join(', '));
}

// ---- highlight and capture
const STYLE = `
html, body { scroll-behavior: auto !important; }
*, *::before, *::after { caret-color: transparent !important; cursor: none !important; transition: none !important; animation: none !important; }
#app-loader, .hm-loader, .app-toast { display: none !important; }
.manual-hl { outline: 3px solid #FFB547 !important; outline-offset: 3px !important; border-radius: 6px; box-shadow: 0 0 0 7px rgba(255, 181, 71, 0.30) !important; }
`;
async function highlight(page, selectors = []) {
    const missing = [];
    for (const s of selectors) {
        const { sel, all, parent, closest } = typeof s === 'string' ? { sel: s } : s;
        const n = await page.evaluate(({ sel, all, parent, closest }) => {
            const els = [...document.querySelectorAll(sel)].filter(e => e.getClientRects().length)
                .map(e => (closest ? e.closest(closest) || e : parent ? e.parentElement : e));
            (all ? els : els.slice(0, 1)).forEach(e => e.classList.add('manual-hl'));
            return els.length;
        }, { sel, all: !!all, parent: !!parent, closest: closest || null });
        if (!n) missing.push(sel);
    }
    return missing;
}
// clip: one selector or several joined with "||"; the union of their boxes plus padding.
// Returns { box, viewport }: viewport = true when the box fits below the fixed header and is given in viewport coordinates.
const UNION = ({ sel, pad }) => {
    const els = sel.split('||').map(s => [...document.querySelectorAll(s.trim())].find(e => e.getClientRects().length)).filter(Boolean);
    if (!els.length) return null;
    const rs = els.map(e => e.getBoundingClientRect());
    const x = Math.max(0, Math.min(...rs.map(r => r.left)) - pad), y = Math.min(...rs.map(r => r.top)) - pad;
    const right = Math.min(innerWidth, Math.max(...rs.map(r => r.right)) + pad), bottom = Math.max(...rs.map(r => r.bottom)) + pad;
    return { x, y, width: right - x, height: bottom - y };
};
// height of the fixed or sticky bars at the top of the page (the app header and menu)
const HEADER = () => {
    let bottom = 0;
    for (const el of document.querySelectorAll('body *')) {
        const pos = getComputedStyle(el).position;
        if (pos !== 'fixed' && pos !== 'sticky') continue;
        const r = el.getBoundingClientRect();
        if (r.top <= 1 && r.height > 20 && r.height < 260 && r.width > innerWidth / 2) bottom = Math.max(bottom, r.bottom);
    }
    return bottom;
};
// the top bar becomes fixed only once the page is scrolled: measure it scrolled, then restore the position
async function headerHeight(page) {
    const y = await page.evaluate(() => scrollY);
    await page.evaluate(() => window.scrollTo(0, Math.max(scrollY, 400)));
    await page.waitForTimeout(200);
    const h = await page.evaluate(HEADER);
    await page.evaluate(y => window.scrollTo(0, y), y);
    await page.waitForTimeout(100);
    return h;
}
async function regionOf(page, shot) {
    if (!shot.clip) return null;
    const vp = page.viewportSize();
    const args = { sel: shot.clip, pad: shot.pad ?? 14 };
    let r = await page.evaluate(UNION, args);
    if (!r) throw new Error('clip element not found: ' + shot.clip);
    const header = await headerHeight(page);
    const height = Math.min(r.height, shot.maxHeight ?? Infinity);
    if (height <= vp.height - header) {
        if (!shot.fixedClip) {
            await page.evaluate(({ y, header }) => window.scrollBy(0, y - header - 4), { y: r.y, header });
            await page.waitForTimeout(150);
            r = await page.evaluate(UNION, args);
        }
        return { viewport: true, box: { x: r.x, y: Math.max(0, r.y), width: r.width, height } };
    }
    const sy = await page.evaluate(() => scrollY);
    return { viewport: false, box: { x: r.x, y: r.y + sy, width: r.width, height } };
}
async function pinFixed(page) {
    await page.evaluate(() => {
        for (const el of document.querySelectorAll('body *')) {
            const pos = getComputedStyle(el).position;
            if ((pos === 'fixed' || pos === 'sticky') && el.tagName !== 'DIALOG') { el.style.position = 'absolute'; el.style.top = el.style.top || '0'; }
        }
    });
}

// lazy images: load them all before the capture
async function loadImages(page) {
    await within(page.evaluate(async () => {
        for (const img of document.querySelectorAll('img[loading="lazy"]')) img.loading = 'eager';
        await Promise.all([...document.images].filter(i => !i.complete).map(i => new Promise(r => { i.onload = i.onerror = r; setTimeout(r, 4000); })));
    }), 8000, 'images').catch(e => trace(e.message));
}
const shotFile = (lang, id) => path.join(here, 'img', lang, `${id}.png`);

export const SHOT_LIMIT_MS = Number(process.env.MANUAL_SHOT_LIMIT || 60000);
// one shot with a hard time limit: on timeout the context is closed, which aborts whatever the page was waiting for
export async function captureShot(browser, shot, lang, I, options = {}) {
    let context;
    const work = (async () => {
        const s = await session(browser, { who: shot.who, lang, viewport: shot.viewport });
        context = s.context;
        return captureIn(s.page, shot, lang, I, options);
    })();
    try {
        return await within(work, SHOT_LIMIT_MS, 'the shot');
    } finally {
        work.catch(() => {});
        await context?.close().catch(() => {});
    }
}
async function captureIn(page, shot, lang, I, { probe = false } = {}) {
    {
        trace('open page');
        await go(page, typeof shot.url === 'function' ? shot.url(I) : shot.url);
        await page.addStyleTag({ content: STYLE });
        await loadImages(page);
        trace('prepare');
        if (shot.prepare) await shot.prepare(page, I, lang);
        await settle(page);
        await page.addStyleTag({ content: STYLE });
        await page.evaluate(() => document.activeElement?.blur?.());
        trace('scrub');
        await scrub(page);
        for (const b of [].concat(shot.blur || [])) await blurRows(page, b.rows, typeof b.keep === 'function' ? b.keep(I) : b.keep);
        const htmlLang = await page.evaluate(() => document.documentElement.lang || '');
        if (!htmlLang.toLowerCase().startsWith(lang)) throw new Error(`page language is "${htmlLang}", expected ${lang}`);
        if (probe) return await outline(page);
        trace('highlight');
        const missing = await highlight(page, shot.hl);
        if (shot.view) await page.evaluate(({ sel, header }) => { const e = document.querySelector(sel); if (e) window.scrollBy(0, e.getBoundingClientRect().top - header - 16); },
            { sel: shot.view, header: await headerHeight(page) });
        else if (!shot.clip && !shot.keepScroll) await page.evaluate(() => window.scrollTo(0, 0));
        await page.waitForTimeout(200);
        const region = await regionOf(page, shot);
        const full = !!shot.fullPage && !region;
        const viewClip = region?.viewport ? region.box : region ? null : full ? null : { x: 0, y: 0, ...page.viewportSize() };
        trace('privacy check');
        await privacyCheck(page, viewClip);
        trace('screenshot');
        fs.mkdirSync(path.dirname(shotFile(lang, shot.id)), { recursive: true });
        if (full || (region && !region.viewport)) await pinFixed(page);
        await page.screenshot({ path: shotFile(lang, shot.id), clip: region?.box, fullPage: !!(full || (region && !region.viewport)), animations: 'disabled', caret: 'hide' });
        return missing;
    }
}
// --probe: ids, classes and UI labels of the visible elements (no table-cell text), to choose selectors
async function outline(page) {
    return page.evaluate(() => {
        const out = [];
        const pick = 'h1,h2,h3,h4,h5,label,button,a.btn,th,[id],.card,.card-title,.st-pill,.badge,.nav-link,section,form,select,input,textarea,li[class],table';
        for (const el of document.querySelectorAll(pick)) {
            if (!el.getClientRects().length || el.closest('#page-topbar, .topnav')) continue;
            const r = el.getBoundingClientRect();
            const label = /^(H\d|LABEL|BUTTON|A|TH)$/.test(el.tagName) || el.matches('.card-title,.badge,.st-pill') ? el.innerText.trim().replace(/\s+/g, ' ').slice(0, 40) : '';
            const cls = typeof el.className === 'string' && el.className.trim() ? '.' + el.className.trim().split(/\s+/).slice(0, 4).join('.') : '';
            out.push(`${el.tagName.toLowerCase()}${el.id ? '#' + el.id : ''}${cls}${el.name ? `[name=${el.name}]` : ''} @${Math.round(r.left)},${Math.round(r.top + scrollY)} ${Math.round(r.width)}x${Math.round(r.height)}${label ? ' "' + label + '"' : ''}`);
        }
        return out.slice(0, 300).join('\n');
    });
}

// ---- the shots (ids and order as in shots.yml)
const STAFF = { org: 'Kovan', role: 'Staff' }, SAO = { org: 'Kovan', role: 'SampleAcceptanceOfficer' }, QA = { org: 'Kovan', role: 'QualityApprover' };
const ASSOC = { org: 'Arıcılar', role: 'OrgAdmin' }, ANALYST = { org: 'Analiz', role: 'Analyst' }, LABMGR = { org: 'Analiz', role: 'LabManager' };
const MARKET = { org: 'Marketi', role: 'Staff' }, ADMIN = 'admin', ANON = 'anonymous';
const desktop = 'desktop', phone = 'phone';
const lotCcp = (lot, ccp) => `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=${ccp}`;
const batchCcp = (b, ccp) => `/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId=${b}&ccpId=${ccp}`;
const demoPdf = path.join(os.tmpdir(), 'top4-manual-demo-document.pdf');
function writeDemoPdf() {
    const stream = 'BT /F1 24 Tf 72 760 Td ([DEMO] document for the user manual) Tj ET';
    const objs = ['<< /Type /Catalog /Pages 2 0 R >>', '<< /Type /Pages /Kids [3 0 R] /Count 1 >>',
        '<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>',
        `<< /Length ${stream.length} >>\nstream\n${stream}\nendstream`, '<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>'];
    let out = '%PDF-1.4\n'; const offs = [];
    objs.forEach((o, i) => { offs.push(out.length); out += `${i + 1} 0 obj\n${o}\nendobj\n`; });
    const xref = out.length;
    out += `xref\n0 ${objs.length + 1}\n0000000000 65535 f \n` + offs.map(o => String(o).padStart(10, '0') + ' 00000 n \n').join('');
    out += `trailer\n<< /Size ${objs.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;
    fs.writeFileSync(demoPdf, out, 'latin1');
}
async function fillParams(page, values, count = Infinity) {
    let n = 0;
    for (const b of await page.locator('.ccp-param[data-parameter-id]').all()) {
        if (n >= count) break;
        const pid = Number(await b.getAttribute('data-parameter-id'));
        const num = b.locator('input[name$=".MeasuredNumericValue"]'), sel = b.locator('select[name$=".MeasuredTextValue"]'), txt = b.locator('input[type=text][name$=".MeasuredTextValue"]');
        if (await num.count()) await num.first().fill(String(values[pid] ?? 10));
        else if (await sel.count()) await sel.first().selectOption(values[pid] ?? 'Yes');
        else if (await txt.count() && await txt.first().isVisible()) await txt.first().fill(values[pid] ?? '[DEMO] As specified');
        n++;
    }
}
async function chooseFile(page, selector) {
    const input = page.locator(selector).first();
    await input.setInputFiles(demoPdf);
}

export const SHOTS = [
    { id: 'intro-chain', who: ANON, viewport: desktop, url: '/', clip: '#chain', pad: 0, hl: [{ sel: '#chain li', all: true }] },
    { id: 'public-honey-story', who: ANON, viewport: phone, url: I => `/honeystory/${I.kekikQr}`, hl: ['#honeystory h1', '#honeystory-badges', '#hs-journey'] },
    { id: 'sign-in', who: ANON, viewport: desktop, url: '/Account/Login', hl: ['#UserName', '#Password', '#RememberMe', '.au-submit', { sel: '[href*="SetLanguage"]', parent: true }] },
    { id: 'forgot-password', who: ANON, viewport: desktop, url: '/Account/ForgotPassword',
        prepare: async page => { await page.fill('input[type=email], #Email, [name=Email]', 'zz_test_manual@example.invalid'); await Promise.all([page.waitForLoadState('load'), page.click('.au-submit')]); },
        hl: ['.au-alert-ok, .au-alert'] },
    { id: 'profile', who: STAFF, viewport: desktop, url: '/Account/Profile',
        prepare: async page => { await page.locator('[data-bs-target="#changePasswordModal"]').first().click(); await page.waitForSelector('#changePasswordModal.show'); await page.waitForTimeout(400); },
        hl: ['#changePasswordModal .modal-content'] },
    { id: 'top-bar-role-chip', who: { org: 'Arıcılar', role: 'QualityApprover' }, viewport: desktop, url: '/MyWork/Index', clip: '#page-topbar', pad: 0, hl: ['.role-chip'] },
    { id: 'role-switch', who: { org: 'Arıcılar', role: 'QualityApprover' }, viewport: desktop, url: '/MyWork/Index',
        prepare: async page => { await page.locator('.role-chip').first().click(); await page.waitForTimeout(400); },
        clip: '#page-topbar||.dropdown-menu.show', maxHeight: 560, hl: ['.dropdown-menu.show'] },
    { id: 'my-work', who: QA, viewport: desktop, url: '/MyWork/Index', hl: ['.work-tiles', '#group-Approve .work-item', 'a.nav-link[href="/MyWork/Index"]'] },
    { id: 'admin-permissions-matrix', who: ADMIN, viewport: desktop, url: '/Permissions/Index?tab=roles&role=Staff',
        hl: ['#role-matrix', '#fixed-rules'] },
    { id: 'flow-overview', who: STAFF, viewport: desktop, url: I => `/Flow/Batch/${I.kekik}`, hl: ['#flow-stages', '#flow-lots'] },
    { id: 'intake-form', who: SAO, viewport: phone, url: I => `/Intake/Create?number=${encodeURIComponent(I.zeynepNumber)}`,
        prepare: async page => {
            await page.selectOption('#apiaryId', { index: 1 });
            await page.selectOption('#honeyTypeId', '3');
            await page.fill('#quantity', '120,5');
            const prov = page.locator('select[data-province-select]');
            await prov.selectOption({ label: 'Muğla' });
            const did = await prov.getAttribute('data-province-select');
            await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
            await page.locator('#' + did).selectOption({ index: 1 });
        },
        hl: ['#find-beekeeper', '#intake-form', '#deliveryForm', '#intake-form button[type=submit]'], fullPage: true },
    { id: 'sample-form', who: SAO, viewport: desktop, url: I => `/Samples/Create?lotId=${I.lotNew}`,
        prepare: async (page, I) => { await page.selectOption('#labOrgId', String(I.lab)); await page.fill('#sealNumber', 'DEMO-S-0042'); await page.fill('#counterSealNumber', 'DEMO-C-0042'); await page.fill('#counterLocation', '[DEMO] Retain shelf 3'); },
        hl: ['#labOrgId', '#sealNumber', '#counterSealNumber', '#sample-form button[type=submit]'] },
    { id: 'ccp-page-lot', who: SAO, viewport: desktop, url: I => lotCcp(I.lotNew, I.ccp0),
        prepare: async page => { await fillParams(page, { 7: 'Yes', 6: '[DEMO] Weekly, logged' }, 7); await page.waitForTimeout(300); },
        view: '.ccp-param[data-parameter-id="7"]', hl: ['.ccp-param[data-parameter-id="7"] select', '.ccp-param[data-parameter-id="8"]'] },
    { id: 'ccp0-submitted', who: SAO, viewport: desktop, url: I => lotCcp(I.lotSubmitted, I.ccp0), clip: '#ccp-status||#record-strip||#ccp-approval', pad: 20, hl: ['#ccp-status', '#record-strip'] },
    { id: 'my-organization', who: STAFF, viewport: desktop, url: '/OrgWorkspace/Index', hl: ['#lots'] },
    { id: 'ccp-page-batch', who: STAFF, viewport: desktop, url: I => batchCcp(I.open, I.ccp10),
        prepare: async page => { await fillParams(page, { 67: 18, 68: 55 }); },
        view: '.card:has(.mdi-database-outline)', hl: ['.card:has(.mdi-database-outline)', { sel: '.ccp-param', all: true }] },
    { id: 'finalize-request', who: STAFF, viewport: desktop, url: I => `/OrgWorkspace/Batch/${I.open}`, clip: '#finalize-card', maxHeight: 700, hl: ['#finalize-blockers', '#request-finalize'] },
    { id: 'shipment-create', who: STAFF, viewport: desktop, url: I => `/Shipments/Create?batchId=${I.kekik}`,
        prepare: async (page, I) => { await page.selectOption('#toOrgId', String(I.market)); await page.fill('#quantity', '10'); await page.fill('#note', '[DEMO] Weekly delivery'); },
        hl: ['#available', '#toOrgId', '#quantity'] },
    { id: 'shipment-confirm', who: MARKET, viewport: phone, url: '/Shipments/Index',
        prepare: async page => { await page.evaluate(() => { const t = document.querySelector('#incoming'); const box = t?.closest('.table-responsive') || t?.parentElement; if (box) box.scrollLeft = box.scrollWidth; }); },
        clip: '#incoming', pad: 10, maxHeight: 560,
        hl: ['#incoming tbody tr:first-child button.btn-success', '#incoming tbody tr:first-child input[name=reason]'] },
    { id: 'storage-record', who: MARKET, viewport: desktop, url: I => `/Shipments/Storage?batchId=${I.yayla}`,
        prepare: async page => { await page.locator('#storage-form input[name^="values"]').first().fill('18'); const s = page.locator('#storage-form input[name^="values"]'); if (await s.count() > 1) await s.nth(1).fill('55'); },
        hl: ['#storage-form table, #storage-form'] },
    { id: 'ccp-approve', who: QA, viewport: desktop, url: I => lotCcp(I.lotSubmitted, I.ccp0), clip: '#ccp-status||#record-strip||#ccp-approval', pad: 20, hl: ['#approve-button', '#approve-note'] },
    { id: 'out-of-limit-dialog', who: QA, viewport: desktop, url: I => batchCcp(I.open, I.ccp4),
        prepare: async page => { await page.click('#approve-button'); await page.waitForSelector('dialog.oolc[open]'); await page.waitForTimeout(300); },
        clip: 'dialog.oolc[open]', pad: 60, fixedClip: true, hl: ['.oolc-list', '.oolc-confirm'] },
    { id: 'finalize-confirm', who: QA, viewport: desktop, url: I => `/OrgWorkspace/Batch/${I.requested}`, clip: '#finalize-card', maxHeight: 700, hl: ['#finalize-requested', '#confirm-finalize'] },
    { id: 'batch-sealed', who: QA, viewport: desktop, url: I => `/OrgWorkspace/Batch/${I.kekik}`, clip: '#batch-seal', hl: ['.seal-title', '#seal-story', '#seal-download'] },
    { id: 'sample-receive', who: ANALYST, viewport: desktop, url: '/Samples/Index', clip: '#lab-samples', maxHeight: 800,
        blur: { rows: '#lab-samples tbody tr', keep: I => [`DEMO-S-${I.lotSubmitted}`, 'DEMO'] }, hl: ['#lab-samples form'] },
    { id: 'lab-results-form', who: ANALYST, viewport: desktop, url: I => `/LabReports/Edit/${I.sampleDraft}`,
        prepare: async page => {
            await page.fill('#reportNumber', '[DEMO] LR-2026-030');
            await chooseFile(page, '#reportFile');
            for (const pid of [24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 69]) { const el = page.locator(`[name="values[${pid}]"]`); if (await el.count()) await el.fill(''); }
            for (const [pid, v] of Object.entries({ 22: 17.2, 23: 12.4 })) { const el = page.locator(`[name="values[${pid}]"]`); if (await el.count()) await el.fill(String(v)); }
        },
        hl: ['.app-file', '#value-22', '#report-form td .st-pill'] },
    { id: 'lab-submit', who: ANALYST, viewport: desktop, url: I => `/LabReports/Edit/${I.sampleDraft}`,
        clip: '#report-form tbody tr:nth-last-child(2)||#required-progress||#submit-report', pad: 24, hl: ['#required-progress', '#submit-report'] },
    { id: 'lab-approve', who: LABMGR, viewport: desktop, url: I => `/LabReports/Edit/${I.sampleSubmitted}`, clip: '#approval', maxHeight: 700, hl: ['#approve-report', '#return-reason', '#approve-note'] },
    { id: 'verify-report', who: ANON, viewport: phone, url: I => `/verify-report?code=${I.reportCode}`, hl: ['#verify-status', '#verify-sha'] },
    { id: 'org-details', who: ASSOC, viewport: desktop, url: I => `/Organizations/Details/${I.association}`, clip: '#documents||#upload-form', pad: 40,
        hl: ['#upload-form select', '#upload-form button[type=submit]', '#documents th:nth-child(3)'] },
    { id: 'consent-form', who: ASSOC, viewport: phone, url: I => `/Storefronts/Consent/${I.noConsent}`,
        prepare: async page => {
            for (const c of await page.locator('#consent-form input[type=checkbox]').all()) await c.check().catch(() => {});
            if (await page.locator('#givenOn').count()) await page.fill('#givenOn', '2026-10-08');
            if (await page.locator('#document').count()) await chooseFile(page, '#document');
        },
        clip: '#consent-form', pad: 16, maxHeight: 1700, hl: [{ sel: '#consent-form .form-check', all: true }, '#givenOn', '#document'] },
    { id: 'admin-dashboard', who: ADMIN, viewport: desktop, url: '/Dashboard/Index',
        hl: [{ sel: '#kpi-lots', parent: true }, { sel: '#chain-apiary', parent: true }] },
    { id: 'admin-org-create', who: ADMIN, viewport: desktop, url: '/Organizations/Create',
        prepare: async page => {
            await page.fill('input[name=DisplayName], #DisplayName', '[DEMO] Örnek Kurum');
            const legal = page.locator('input[name=LegalName], #LegalName'); if (await legal.count()) await legal.fill('[DEMO] Örnek Kurum A.Ş.');
            const prov = page.locator('select[data-province-select]');
            if (await prov.count()) {
                await prov.selectOption({ label: 'Muğla' });
                const did = await prov.getAttribute('data-province-select');
                await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
                await page.locator('#' + did).selectOption({ index: 1 });
            }
            for (const c of await page.locator('input[type=checkbox][name*=apab]').all()) { const v = await c.getAttribute('value'); if (['Processing', 'Packing'].includes(v)) await c.check(); }
        },
        hl: ['select[name=LegacyWholesalerId], #LegacyWholesalerId', { sel: 'input[type=checkbox][name*=apab]', closest: '.mb-3' }] },
    { id: 'admin-org-members', who: ADMIN, viewport: desktop, url: I => `/Organizations/Details/${I.processor}`, clip: '#members||#add-member', pad: 40,
        hl: ['#add-member select', '#add-member input[type=text], #add-member input:not([type=hidden])', '#add-member button[type=submit]'] },
    { id: 'admin-role-permissions', who: ADMIN, viewport: desktop, url: '/Permissions/Index?tab=roles&role=QualityApprover',
        hl: ['#role-matrix tbody tr:first-child', '#role-matrix'] },
    { id: 'admin-parameter-limits', who: ADMIN, viewport: desktop, url: I => `/CriticalControlPointParameters/Limits/${I.moisture}`, hl: ['table', 'form'] },
    { id: 'admin-registry', who: ADMIN, viewport: desktop, url: I => `/Registry/Edit/${I.zeynep}`, hl: ['#registry-number', '#registry-requests', '#registry-status'] },
    { id: 'admin-manage-batch', who: ADMIN, viewport: desktop, url: I => `/FinalHoneyProductions/ManageBatch/${I.open}`,
        prepare: async page => { const ins = page.locator('#manageBatchForm input[name^="usedAmounts"]'); const n = await ins.count(); for (let i = 0; i < n; i++) if ((await ins.nth(i).inputValue()) === '0') { await ins.nth(i).fill('5'); break; } },
        clip: '#manageBatchForm||[onclick*=confirmSaveBatch]', pad: 16, blur: { rows: '#manageBatchForm tbody tr', keep: ['[DEMO]'] },
        hl: [{ sel: '#manageBatchForm input[name^="usedAmounts"]', all: true }, '[onclick*=confirmSaveBatch]'] },
    { id: 'recall-report', who: STAFF, viewport: desktop, url: I => `/Recall/Batch/${I.kekik}`, fullPage: true,
        hl: ['#recall-lots', '#recall-shipments', '#recall-holders', '.btn:has(.mdi-printer), [onclick*=print]', '#csv-link'] },
    { id: 'audit-log', who: ADMIN, viewport: desktop, url: '/Audit/Index?entity=FinalHoneyProductions',
        prepare: async page => { const raw = page.locator('#audit-table details summary').first(); if (await raw.count()) await raw.click(); },
        blur: { rows: '#audit-table tbody tr', keep: I => I.demoBatchIds.map(id => `#${id}`).concat(['[DEMO]']) },
        hl: ['#audit-table tbody tr:first-child td:nth-child(6)', '#audit-table details[open] summary', '#verify-chain'] },
    { id: 'public-map', who: ANON, viewport: phone, url: I => `/honeystory/${I.kekikQr}`,
        prepare: async page => { await page.evaluate(() => document.getElementById('hs-map')?.scrollIntoView({ block: 'center' })); await page.waitForTimeout(600); const p = page.locator('#hs-map path.is-on').first(); if (await p.count()) await p.click().catch(() => {}); await page.waitForTimeout(300); },
        clip: '#hs-map', pad: 12, hl: ['#hs-map figure, #hs-map .tmap'] },
];

// ---- command line
const isMain = process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url);
if (isMain) {
    const args = process.argv.slice(2);
    const flag = n => { const i = args.indexOf(n); return i >= 0 ? args.splice(i, 2)[1] : null; };
    const probe = flag('--probe'), only = flag('--only');
    const langs = args.filter(a => !a.startsWith('--')).length ? args.filter(a => !a.startsWith('--')) : LANGS;
    const I = ids();
    fs.mkdirSync(path.join(here, 'img'), { recursive: true });
    writeDemoPdf();
    let browser = null;
    try {
        const wanted = SHOTS.filter(s => (!only || only.split(',').includes(s.id)) && (!probe || probe.split(',').includes(s.id)));
        const runLangs = probe ? [langs[0]] : langs;
        const total = runLangs.length * wanted.length;
        let n = 0, failed = 0;
        for (const lang of runLangs) {
            await browser?.close();
            browser = await launch(lang);
            for (const shot of wanted) {
                const start = Date.now();
                const head = `[${String(++n).padStart(3)}/${total}] ${lang} ${shot.id}`;
                try {
                    const result = await captureShot(browser, shot, lang, I, { probe: !!probe });
                    const secs = ((Date.now() - start) / 1000).toFixed(1);
                    if (probe) console.log(`==== ${shot.id}\n${result}`);
                    else console.log(`${head} ok ${secs}s${result.length ? '  (no highlight: ' + result.join(' ; ') + ')' : ''}`);
                } catch (e) {
                    failed++;
                    console.log(`${head} FAILED ${((Date.now() - start) / 1000).toFixed(1)}s: ${e.message.split('\n')[0]}`);
                    process.exitCode = 1;
                }
            }
        }
        console.log(`done: ${total - failed} ok, ${failed} failed`);
    } finally {
        await browser?.close();
    }
}
