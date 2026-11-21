// Takes every picture of the user manual in one language from the manual's own run of the platform (fictional data only).
//   node capture.mjs en            (tr, es, pl, lv later; needs setup-db.ps1 once, then works on its own)
//   node capture.mjs en --keep     (do not reset: reuse the running app as it is; for trying a change, not for the real pictures)
//   node capture.mjs en --only=03,04   (the whole journey runs, but only these chapters' pictures are taken)
// Every run starts from the same data: the app is stopped, the database and the app's files are restored from the backup made by
// setup-db.ps1, and the app is started again on http://localhost:<MANUAL_PORT>. The run then carries one new honey through the whole
// journey as the people of the cast (outline.md), taking each picture where the step happens. Pictures go to content/<lang>/screens/.
// The practice password and the phone page key are read at run time and never printed.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execFileSync, spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { VIEWPORT, SCALE, LOCALE, settle, mark, clearMarks, shot } from './helpers.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const REPO = path.resolve(HERE, '..', '..', '..');
const WEB = path.join(REPO, 'TOP4HONEYCHAINS', 'TOP4HoneyChains.Web');
const LANG = process.argv[2] || 'en';
const KEEP = process.argv.includes('--keep');
const ONLY = (process.argv.find(a => a.startsWith('--only=')) || '').slice(7);
if (!LOCALE[LANG]) throw new Error('usage: node capture.mjs <en|tr|es|pl|lv> [--keep] [--only=NN,NN]');
const SCREENS = path.join(HERE, '..', 'content', LANG, 'screens');

const env = Object.fromEntries(fs.readFileSync(path.join(HERE, '.env.local'), 'utf8').split(/\r?\n/)
    .filter(l => /^[A-Z_]+=/.test(l)).map(l => [l.slice(0, l.indexOf('=')), l.slice(l.indexOf('=') + 1).trim()]));
const PASSWORD = env.MANUAL_PASSWORD;
const BASE = `http://localhost:${env.MANUAL_PORT || 5256}`;
// the manual's copy of the cast, with fictional organisation names (written by setup-db.ps1)
const CAST_FILE = path.join(process.env.LOCALAPPDATA || '', 'TOP4-manual', 'cast.json');
if (!fs.existsSync(CAST_FILE)) throw new Error('Run setup-db.ps1 first: ' + CAST_FILE + ' is missing.');
const cast = JSON.parse(fs.readFileSync(CAST_FILE, 'utf8'));
const values = JSON.parse(fs.readFileSync(path.join(REPO, 'docs', 'partner-test', 'values.json'), 'utf8'));
const person = (role, org) => cast.people.find(p => p.role === role && p.org === org);
const digitsOf = phone => phone.replace(/\D/g, '');
const orgName = key => cast.orgs.find(o => o.key === key).name;
const bee = cast.beekeepers.find(b => b.key === 'new');            // Hatice Turan: registered, delivers and uses the portal during the run
const CONSUMER = 'petekci42';
const BATCH_TITLE = 'Şile Kestane Balı 2026/2';
const log = (...a) => console.log(...a);
const want = chapter => !ONLY || ONLY.split(',').includes(chapter);

// ------------------------------------------------------------------ the app: reset and start
// output goes straight to this console; app.ps1 starts the app without handing it any handle of this process
function ps(...args) {
    const r = spawnSync('powershell', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', path.join(HERE, 'app.ps1'), ...args], { stdio: 'inherit' });
    if (r.status !== 0) throw new Error(`app.ps1 ${args.join(' ')} failed`);
}
if (!KEEP) { ps('reset'); ps('start'); }
if (!ONLY && fs.existsSync(SCREENS)) for (const f of fs.readdirSync(SCREENS)) if (f.endsWith('.png')) fs.rmSync(path.join(SCREENS, f));
const phoneKey = execFileSync('dotnet', ['user-secrets', 'list', '--project', WEB], { encoding: 'utf8' }).match(/^Sms:PhonePageKey = (.+)$/m)?.[1].trim();
if (!phoneKey) throw new Error('no phone page key in the user secrets of the web project');

// the parameters (reference data, no personal data): id -> CCP code, name, data type
const params = Object.fromEntries(execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', 'TOP4HoneyChainsDb_Manual', '-E', '-b', '-W', '-h', '-1', '-s', '|', '-f', '65001', '-Q',
    'SET NOCOUNT ON; SELECT p.ParameterId, c.Code, LTRIM(RTRIM(p.ParameterName)), p.DataType FROM dbo.CriticalControlPointParameters p JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId'],
    { encoding: 'utf8' }).split(/\r?\n/).filter(l => /^\d+\|/.test(l)).map(l => { const [id, code, name, type] = l.split('|'); return [id, { code, name, type }]; }));
function value(id) {
    const p = params[id];
    const key = `${p.code}|${p.name}`;
    const own = cast.history.values?.[key];
    if (own) return own;
    if (p.type === 'Number') return values.numbers[key];
    if (values.texts[key]) return values.texts[key].replace('{country}', 'Türkiye').replace('{botanical}', cast.history.botanical);
    if (p.code === 'CCP 0') return p.type === 'YesNo' ? values.ccp0.yesNo
        : Object.entries(values.ccp0).find(([k]) => k !== 'yesNo' && k !== 'text' && p.name.startsWith(k))?.[1] ?? values.ccp0.text;
    return p.type === 'YesNo' ? values.otherYesNo : '';
}
const today = () => new Date().toISOString().slice(0, 10);

// ------------------------------------------------------------------ browser and pages
const { chromium } = await import('playwright');
const browser = await chromium.launch({ args: [`--lang=${LOCALE[LANG]}`] });     // dates in the language's own order
const DESK = { viewport: VIEWPORT, deviceScaleFactor: SCALE, locale: LOCALE[LANG], colorScheme: 'light', reducedMotion: 'reduce' };
const PHONE = { viewport: { width: 390, height: 844 }, deviceScaleFactor: SCALE, isMobile: true, hasTouch: true, locale: LOCALE[LANG], colorScheme: 'light', reducedMotion: 'reduce' };

async function newPage(options = DESK) {
    const context = await browser.newContext(options);
    const page = await context.newPage();
    page.setDefaultTimeout(20000);
    await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=/`);
    return page;
}
// closes the tour (shown once per user and role) and the notice banner, and waits until the page is at rest
async function quiet(page) {
    await settle(page);
    const skip = page.locator('[data-tour-skip]');
    if (await skip.isVisible().catch(() => false)) { await skip.click(); await settle(page); }
}
async function go(page, url) { await page.goto(BASE + url); await quiet(page); }
async function press(page, target) {
    await Promise.all([page.waitForLoadState('load'), (typeof target === 'string' ? page.locator(target).first() : target).click()]);
    await quiet(page);
}
async function flash(page) {
    return page.evaluate(() => [...document.querySelectorAll('#flash-success, #flash-error')].map(e => `${e.id}: ${e.innerText.replace(/\s*×\s*$/, '').trim()}`).join(' / '));
}
async function errorsOf(page) {
    return page.evaluate(() => [...document.querySelectorAll('.alert-danger, .validation-summary-errors, [id$="-errors"]')].map(e => e.innerText.trim()).filter(Boolean).join(' / ').slice(0, 400));
}
async function expectSuccess(page, what) {
    const f = await flash(page);
    if (!/flash-success/.test(f)) throw new Error(`${what}: ${f} ${await errorsOf(page)}`);
    return f.replace(/^flash-success: /, '');
}
// the bottom of the fixed header (desktop) so that a part scrolled to the top is not hidden under it
const headerBottom = page => page.evaluate(() => Math.max(0, ...[...document.querySelectorAll('header, nav, #page-topbar, .topnav, .app-top')]
    .filter(e => ['fixed', 'sticky'].includes(getComputedStyle(e).position)).map(e => e.getBoundingClientRect().bottom)));

// One picture. Without options: the whole window. crop: that element, scrolled to just below the header. part: from the top of `from`
// to the top of `until` (or the bottom of `through`), as wide as `within`. marks: [locator, number, options] drawn after scrolling.
async function snap(page, name, { crop, part, marks = [], margin = 16, scroll, keepFlash = false } = {}) {
    if (!keepFlash) await page.evaluate(() => document.querySelectorAll('#flash-success, #flash-error').forEach(e => e.closest('.toast, .alert, [role=alert], [role=status]')?.querySelector('.btn-close, button[aria-label]')?.click()));
    if (!scroll && !crop && !part && marks.length) { await marks[0][0].evaluate(e => e.scrollIntoView({ block: 'center' })); await page.waitForTimeout(150); }
    if (scroll === 'top') await page.evaluate(() => window.scrollTo(0, 0));
    if (scroll === 'bottom') await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
    if (scroll) await page.waitForTimeout(150);
    const top = part ? part.from : crop;
    // a part of a form is cut out below the fixed header: the header is hidden for that picture (it is not in the cut-out)
    if (part) await page.addStyleTag({ content: '#page-topbar, .app-top, .topnav { visibility: hidden !important; }' });
    if (top) {
        await top.evaluate(e => e.scrollIntoView({ block: 'start' }));
        const h = part ? 0 : await headerBottom(page);
        await page.evaluate(d => window.scrollBy(0, -d), h + margin + 4);
        await page.waitForTimeout(120);
    }
    for (const [loc, n, o] of marks) await mark(page, loc, n, o);
    if (part) {
        const a = await part.from.boundingBox(), w = await part.within.boundingBox();
        const end = part.until ? (await part.until.boundingBox()).y : (b => b.y + b.height + margin)(await part.through.boundingBox());
        const view = page.viewportSize();
        const x = Math.max(0, w.x - margin), y = Math.max(0, a.y - margin);
        const clip = { x, y, width: Math.min(view.width, w.x + w.width + margin) - x, height: Math.min(view.height, end) - y };
        if (end > view.height) log(`  note: ${name} is cut at the window bottom (${Math.round(end - y)} px tall)`);
        fs.mkdirSync(SCREENS, { recursive: true });
        await page.screenshot({ path: path.join(SCREENS, `${name}.png`), clip });
        await page.evaluate(() => document.querySelectorAll('style').forEach(st => { if (st.textContent.includes('visibility: hidden !important')) st.remove(); }));
    } else await shot(page, LANG, name, crop ? { crop, margin } : {});
    await clearMarks(page);
}
// an open menu of the top bar: the top bar and the menu, from the menu's left edge minus a margin to the right edge of the window
async function shotMenu(page, name) {
    await page.evaluate(() => window.scrollTo(0, 0));
    const m = await page.locator('.dropdown-menu.show').first().boundingBox();
    const view = page.viewportSize();
    const x = Math.max(0, m.x - 360);
    fs.mkdirSync(SCREENS, { recursive: true });
    await page.screenshot({ path: path.join(SCREENS, `${name}.png`), clip: { x, y: 0, width: view.width - x, height: Math.min(view.height, m.y + m.height + 16) } });
}
// the phone pictures: scroll a part to the top of the phone and take the screen
async function snapPhone(page, name, part) {
    if (part) {
        await part.evaluate(e => e.scrollIntoView({ block: 'start' }));
        const h = await headerBottom(page);
        await page.evaluate(d => window.scrollBy(0, -d), h + 8);
        await settle(page, 500);
    }
    await shot(page, LANG, name);
}

// the phone page of a number (fake SMS provider): the newest message after the last one read
const phones = {};
async function watchPhone(digits) {
    if (phones[digits]) return;
    const page = await (await browser.newContext()).newPage();
    await page.goto(`${BASE}/phone/${phoneKey}/${digits}`);
    await page.waitForTimeout(500);
    phones[digits] = { page, seen: Math.max(0, ...await page.locator('.msg').evaluateAll(els => els.map(e => +e.dataset.id))) };
}
async function nextSms(digits, re = /\b\d{6}\b/) {
    await watchPhone(digits);
    const phone = phones[digits];
    const deadline = Date.now() + 15000;
    while (Date.now() < deadline) {
        await phone.page.reload();
        const fresh = await phone.page.locator('.msg').evaluateAll(els => els.map(e => ({ id: +e.dataset.id, text: e.innerText })));
        const hit = fresh.filter(m => m.id > phone.seen && re.test(m.text)).sort((a, b) => b.id - a.id)[0];
        if (hit) { phone.seen = Math.max(phone.seen, hit.id); return hit.text; }
        await phone.page.waitForTimeout(500);
    }
    throw new Error(`no new SMS matching ${re} for ${digits}`);
}
const codeIn = t => t.match(/\b\d{6}\b/)[0];

// a signed-in person; the privacy notice is confirmed on the way unless keepNotice
const people = {};
async function signIn(user, { keepNotice = false } = {}) {
    if (people[user]) return people[user];
    const page = await newPage();
    await go(page, '/Account/Login');
    await page.fill('#UserName', user);
    await page.fill('#Password', PASSWORD);
    await press(page, '.au-submit');
    if (/\/Account\/Login/i.test(page.url())) throw new Error(`${user} could not sign in`);
    if (!keepNotice) await confirmNotice(page, user);
    people[user] = page;
    return page;
}
async function confirmNotice(page, user, { pictures = false } = {}) {
    await go(page, '/Notices/Accept');
    if (await page.locator('#notice-none').count()) return;
    if (await page.locator('#notice-send-code').count()) {
        const p = cast.people.find(x => x.user === user) || {};
        const digits = digitsOf(p.phone || '');
        await watchPhone(digits);
        if (!(await page.locator('#phone').inputValue())) await page.fill('#phone', p.phone);
        await press(page, '#notice-send-code button[type=submit]');
        await page.fill('#code', codeIn(await nextSms(digits)));
    }
    await page.locator('#notice-accept input[name=read]').check();
    if (pictures) await snap(page, '02-05-notice-confirm', { crop: page.locator('#notice-accept'),
        marks: [[page.locator('#notice-accept input[name=read]').locator('xpath=ancestor::label[1]'), 2], [page.locator('#notice-accept button[type=submit]'), 3]] });
    await press(page, '#notice-accept button[type=submit]');
}

// fills the CCP inputs of a page (single control point or control sheet) with the values of docs/partner-test/values.json
async function fillCcpBoxes(page) {
    const boxes = await page.locator('.ccp-param[data-parameter-id]').evaluateAll(els => els.map(e => e.dataset.parameterId));
    for (const id of boxes) {
        const v = value(id);
        if (!v) continue;
        const box = page.locator(`.ccp-param[data-parameter-id="${id}"]`);
        if (await box.locator('select').count()) await box.locator('select').first().selectOption(v);
        else await box.locator('input[type=text]').first().fill(v);
    }
}
// the approve button; when values are outside their limits the page asks to confirm in a dialog
async function approve(page, button, dialog) {
    const navigation = page.waitForNavigation({ timeout: 4000 }).then(() => true).catch(() => false);
    await page.locator(button).click();
    if (!await navigation) await Promise.all([page.waitForNavigation(), page.locator(`${dialog} .oolc-confirm`).click()]);
    await quiet(page);
}

// ================================================================== the journey
const started = Date.now();
const reg = person('RegistrationOfficer', 'directorate').user, sao = person('SampleAcceptanceOfficer', 'association').user;
const staff = person('Staff', 'association').user, qa = person('QualityApprover', 'association').user;
const analyst = person('Analyst', 'lab').user, labManager = person('LabManager', 'lab').user;
const shop = person('Staff', 'shop').user, shopQa = person('QualityApprover', 'shop').user;
const orgAdmin = person('OrgAdmin', 'association').user, compliance = person('ComplianceOfficer', 'association').user;
const admin = cast.operator.user;
const beeDigits = digitsOf(bee.phone);
let business, lot, intake, batch, qr, composite, reportCode;

try {
    // ---------------------------------------------------------- 1 and 2: the start page, signing in, the first steps
    log('· 1-2 start page, sign-in');
    {
        const page = await newPage();
        await go(page, '/');
        if (want('01')) await snap(page, '01-01-home');
        await go(page, '/Account/Login');
        if (want('02')) {
            await snap(page, '02-01-language', { marks: [[page.locator('a[href*="SetLanguage?culture=tr"]').locator('xpath=..'), 1]] });
            await page.fill('#UserName', sao);
            await page.fill('#Password', 'password-hidden');
            await snap(page, '02-02-sign-in', { crop: page.locator('form').first(),
                marks: [[page.locator('#UserName'), 2], [page.locator('#Password'), 3], [page.locator('.au-submit'), 4]] });
            await go(page, '/Account/ForgotPassword');
            await page.fill('input[type=text], input:not([type])', sao).catch(() => {});
            await snap(page, '02-11-forgot-password', { crop: page.locator('form').first() });
        }
        await page.context().close();
    }
    {
        const page = await signIn(sao, { keepNotice: true });
        await go(page, '/MyWork/Index');
        if (want('02')) await snap(page, '02-04-notice-banner', { crop: page.locator('#notice-banner'), marks: [[page.locator('#notice-banner a, #notice-banner .btn').first(), 1]] });
        await confirmNotice(page, sao, { pictures: want('02') });
        await go(page, '/MyWork/Index');
        if (want('02')) {
            await snap(page, '02-06-my-work', { marks: [[page.locator('#member-menu'), 1], [page.locator('#page-topbar button.role-chip'), 2], [page.locator('#page-header-user-dropdown'), 3, { at: 'left' }]] });
            await page.locator('#page-topbar button.role-chip').click();
            await page.waitForTimeout(300);
            await shotMenu(page, '02-07-role-switch');
            await page.keyboard.press('Escape');
            await page.locator('#page-header-user-dropdown').click();
            await page.waitForTimeout(300);
            await shotMenu(page, '02-08-user-menu');
            await page.keyboard.press('Escape');
            await page.locator('#menu-tour').evaluate(e => e.click());
            await page.waitForTimeout(400);
            await snap(page, '02-09-tour');
            await page.keyboard.press('Escape');
            await go(page, '/Account/Profile');
            await snap(page, '02-10-profile');
        }
    }

    // ---------------------------------------------------------- 3: Hatice Turan is registered
    log('· 3 registration');
    await watchPhone(beeDigits);
    {
        const page = await signIn(reg);
        await go(page, '/Registration/Index');
        await page.fill('#number', bee.registry);
        if (want('03')) await snap(page, '03-01-registration-list', { marks: [[page.locator('#number'), 2], [page.locator('#registration-find button[type=submit]'), 3]] });
        await press(page, '#registration-find button[type=submit]');
        const h5 = page.locator('#registration-form h5'), form = page.locator('#registration-form');
        await page.fill('#FirstName', bee.first);
        await page.fill('#LastName', bee.last);
        await page.fill('#BirthDate', bee.birth);
        await page.fill('#Phone', bee.phone);
        await page.selectOption('#Language', 'tr').catch(() => {});
        await page.selectOption('#AssociationOrgId', { label: orgName('association') });
        if (want('03')) await snap(page, '03-02-form-person', { part: { from: h5.nth(0), until: h5.nth(2), within: form } });
        await page.selectOption('#ap-province', { label: bee.province });
        await page.waitForFunction(() => document.getElementById('ap-district').options.length > 1);
        await page.selectOption('#ap-district', { label: bee.district });
        await page.fill('#ap-locality', bee.locality);
        await page.fill('#ApiaryHives', bee.hives);
        await page.fill('#ApiaryLocationLatitude', bee.lat);
        await page.fill('#ApiaryLocationLongitude', bee.lng);
        await page.locator('#ApiaryLocationLongitude').dispatchEvent('change');
        await settle(page, 1500);
        if (want('03')) {
            await snap(page, '03-03-form-apiary', { part: { from: h5.nth(2), through: page.locator('#ap-locality'), within: form } });
            await page.click('#add-apiary');
            await snap(page, '03-06-add-apiary', { part: { from: page.locator('#more-apiaries .more-apiary').first(), through: page.locator('#add-apiary'), within: form },
                marks: [[page.locator('#more-apiaries .remove-apiary').first(), 2], [page.locator('#add-apiary'), 1]] });
            await page.locator('#more-apiaries .remove-apiary').first().click();
        }
        await page.check('#PublicName');
        await page.check('#PortalNotifications');
        await page.check('#Attest');
        if (await page.locator('#IssueSignIn').count()) await page.check('#IssueSignIn');
        if (want('03')) await snap(page, '03-04-form-consents', { part: { from: page.locator('#PublicName').locator('xpath=ancestor::*[contains(@class,"form-check")][1]'), through: page.locator('#registration-send'), within: form },
            marks: [[page.locator('#registration-send'), 1]] });
        await press(page, '#registration-send');
        if (!await page.locator('#Code').count()) throw new Error('registration: no code field ' + await errorsOf(page));
        await page.fill('#Code', codeIn(await nextSms(beeDigits)));
        if (want('03')) await snap(page, '03-05-code', { part: { from: page.locator('#registration-form h5').last(), through: page.locator('#registration-confirm'), within: form },
            marks: [[page.locator('#Code'), 2], [page.locator('#registration-confirm'), 3]] });
        await press(page, '#registration-confirm');
        await expectSuccess(page, 'registration');
        business = (await page.evaluate(() => document.body.innerText)).match(/TR-BEE-\d{4}-\d{6}/)?.[0];
        if (want('03')) await snap(page, '03-07-details', { keepFlash: true });
        log('  registered', business);
    }

    // the beekeeper's first sign-in with the details from the SMS (phone)
    let beePage;
    {
        const sms = await nextSms(beeDigits, /TR-BEE-\d{4}-\d{6}/);
        const user = sms.match(/TR-BEE-\d{4}-\d{6}/)[0];
        beePage = await newPage(PHONE);
        let ok = false;
        for (const candidate of sms.match(/(?<![A-Za-z0-9-])[A-HJ-NP-Za-km-z2-9]{10}(?![A-Za-z0-9-])/g) || []) {
            await go(beePage, '/Account/Login');
            await beePage.fill('#UserName', user);
            await beePage.fill('#Password', candidate);
            await press(beePage, '.au-submit');
            if (!/\/Account\/Login/i.test(beePage.url())) { ok = true; break; }
        }
        if (!ok) throw new Error('the temporary password did not work');
        if (/FirstPassword/i.test(beePage.url())) {
            const fields = beePage.locator('input[type=password]');
            await fields.nth(0).fill(PASSWORD); await fields.nth(1).fill(PASSWORD);
            if (want('02')) await snap(beePage, '02-03-first-password');
            await press(beePage, beePage.locator('form button[type=submit]').first());
        }
        await confirmNotice(beePage, user);
        await go(beePage, `/Home/SetLanguage?culture=${LANG}&returnUrl=/Me`);
    }

    // ---------------------------------------------------------- 4: the delivery and CCP 0
    log('· 4 delivery');
    {
        const page = await signIn(sao);
        await go(page, '/Intake/Index');
        if (want('04')) await snap(page, '04-01-deliveries', { marks: [[page.locator('#new-intake'), 1]] });
        await press(page, '#new-intake');
        await page.fill('input[name=number]', business);
        if (want('04')) await snap(page, '04-02-find', { crop: page.locator('#find-beekeeper'), marks: [[page.locator('input[name=number]'), 2], [page.locator('#find-beekeeper button[type=submit]'), 3]] });
        await press(page, '#find-beekeeper button[type=submit]');
        const h5 = page.locator('#custody-form h5'), form = page.locator('#custody-form');
        if (!(await page.locator('#ApiaryId').inputValue())) await page.locator('#ApiaryId').selectOption({ index: 1 });
        await page.selectOption('#HoneyTypeId', { label: cast.history.honeyType });
        await page.selectOption('#PeriodId', { label: cast.history.period.title });
        const prov = page.locator('select[data-province-select]').first();
        await prov.selectOption({ label: bee.province });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ label: bee.district });
        if (!(await page.locator('#DeliveryDate').inputValue())) await page.fill('#DeliveryDate', today());
        await page.selectOption('#OriginCountry', 'TR').catch(() => {});
        await page.selectOption('#LabOrgId', { label: orgName('lab') });
        if (!(await page.locator('#RetainUntil').inputValue())) { const d = new Date(); d.setFullYear(d.getFullYear() + 2); await page.fill('#RetainUntil', d.toISOString().slice(0, 10)); }
        await page.fill('#CounterLocation', cast.history.counterLocation);
        if (want('04')) await snap(page, '04-03-delivery-form', { part: { from: h5.nth(0), until: h5.nth(1), within: form } });
        if (await page.locator('#containers tbody tr').count() < 2) await page.click('#add-container');
        const rows = page.locator('#containers tbody tr');
        const plan = [
            { net: '23.8', gross: '25.2', origin: 'SAB-2026-0517', jar: 'SAB-J-34201', counter: 'SAB-K-34201', reseal: 'SAB-P-34201' },
            { net: '21.6', gross: '23.0', origin: 'SAB-2026-0518', jar: 'SAB-J-34202', counter: 'SAB-K-34202', reseal: 'SAB-P-34202' }
        ];
        for (let i = 0; i < plan.length; i++) {
            const r = rows.nth(i), p = plan[i];
            const f = n => r.locator(`[data-field="${n}"]`);
            await f('Kind').selectOption({ index: 0 });
            await f('NetKg').fill(p.net); await f('GrossKg').fill(p.gross);
            await f('ArrivedSealed').check(); await f('OriginSeal').fill(p.origin); await f('OriginSealIntact').check();
            await f('JarSeal').fill(p.jar); await f('CounterSeal').fill(p.counter); await f('CapIntact').check(); await f('Reseal').fill(p.reseal);
        }
        if (want('04')) {
            await snap(page, '04-04-containers', { part: { from: h5.nth(1), through: page.locator('#add-container'), within: form }, marks: [[page.locator('#add-container'), 1]] });
            // the table of containers is wider than the window: its right-hand part as a second picture
            await page.locator('#containers').evaluate(t => { const w = t.closest('.table-responsive') || t.parentElement; w.scrollLeft = w.scrollWidth; });
            await snap(page, '04-04-containers-right', { part: { from: h5.nth(1), through: page.locator('#containers'), within: form } });
            await page.locator('#containers').evaluate(t => { const w = t.closest('.table-responsive') || t.parentElement; w.scrollLeft = 0; });
            await snap(page, '04-05-samples', { part: { from: h5.nth(2), through: page.locator('#intake-send'), within: form }, marks: [[page.locator('#intake-send'), 1]] });
        }
        await press(page, '#intake-send');
        if (!await page.locator('#Code').count()) throw new Error('delivery: code not sent ' + await errorsOf(page));
        await page.fill('#Code', codeIn(await nextSms(beeDigits)));
        if (want('04')) await snap(page, '04-06-code', { part: { from: page.locator('#custody-form h5').last(), through: page.locator('#intake-confirm'), within: page.locator('#custody-form') },
            marks: [[page.locator('#Code'), 2], [page.locator('#intake-confirm'), 3]] });
        await press(page, '#intake-confirm');
        const f = await expectSuccess(page, 'delivery');
        intake = page.url().match(/\/(\d+)(\?|$)/)?.[1];
        lot = f.match(/#(\d+)/)?.[1] ?? (await page.content()).match(/(?:lotId|rawHoneyProductionId)=(\d+)/)?.[1];
        if (want('04')) {
            await snap(page, '04-07-delivery', { keepFlash: true, marks: [[page.locator('a[href*="size=a4"]').first(), 1]] });
            await go(page, `/Intake/Labels/${intake}?size=a4`);
            await snap(page, '04-08-labels');
        }
        log('  delivery', intake, 'lot', lot);
        await go(page, '/MyWork/Index');
        if (want('04')) await snap(page, '04-09-my-work', { marks: [[page.locator('.work-item').first(), 1]] });
        await go(page, `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`);
        await fillCcpBoxes(page);
        if (want('04')) await snap(page, '04-10-ccp0', { scroll: 'top' });
        if (want('04')) await snap(page, '04-11-ccp0-save', { scroll: 'bottom', marks: [[page.locator('form.field-form button[type=submit]'), 3]] });
        await press(page, 'form.field-form button[type=submit]');
        await expectSuccess(page, 'CCP 0');
    }
    {
        const page = await signIn(qa);
        await go(page, '/MyWork/Index');
        if (want('07')) await snap(page, '07-01-my-work', { marks: [[page.locator('.work-item').first(), 1]] });
        await go(page, `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`);
        if (want('07')) await snap(page, '07-02-approve-ccp0', { crop: page.locator('#approve-button').locator('xpath=ancestor::*[contains(@class,"card")][1]'), marks: [[page.locator('#approve-button'), 2]] });
        await approve(page, '#approve-button', '#oolc-approve-form');
        await expectSuccess(page, 'CCP 0 approval');
    }

    // ---------------------------------------------------------- 5: the laboratory
    log('· 5 laboratory');
    {
        const page = await signIn(analyst);
        await go(page, '/MyWork/Index');
        if (want('05')) await snap(page, '05-01-my-work');
        await go(page, '/Samples/Index');
        let first = true;
        for (let i = 0; i < 4; i++) {
            const form = page.locator('form.receive-jar-form').first();
            if (!await form.count()) break;
            for (const name of ['sealMatches', 'labelIntact', 'capIntact']) await form.locator(`input[name=${name}][value=true]`).check();
            if (first && want('05')) await snap(page, '05-02-receive', { marks: [[form.locator('button[type=submit]'), 3]] });
            first = false;
            await press(page, 'form.receive-jar-form button[type=submit]');
        }
        const compositeForm = page.locator('#composite-ready, form[action*="Composite"]').first();
        if (want('05')) await snap(page, '05-03-composite', { crop: compositeForm, marks: [[compositeForm.locator('button[type=submit]'), 1]] });
        await press(page, compositeForm.locator('button[type=submit]'));
        if (!/LabReports\/Edit/.test(page.url())) throw new Error('composite: ' + await flash(page));
        composite = page.url().split('/').pop();
        const reportNumber = cast.history.lab.reportPrefix + '1207';
        await page.fill('#reportNumber', reportNumber);
        await page.fill('#issuedOn', today());
        const ids = await page.locator('[name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) {
            const input = page.locator(`[name="values[${id}]"]`);
            if ((await input.evaluate(e => e.tagName)) === 'SELECT') await input.selectOption(value(id)); else await input.fill(value(id));
        }
        const pdf = path.join(os.tmpdir(), `${reportNumber}.pdf`);
        fs.writeFileSync(pdf, '%PDF-1.4\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n');
        await page.setInputFiles('#reportFile', pdf);
        if (want('05')) await snap(page, '05-04-report', { scroll: 'top' });
        await press(page, '#required-save, #report-form button[type=submit]');
        fs.rmSync(pdf, { force: true });
        if (want('05')) await snap(page, '05-05-submit', { marks: [[page.locator('#submit-report'), 1]] });
        await press(page, '#submit-report');
        await expectSuccess(page, 'lab submit');
    }
    {
        const page = await signIn(labManager);
        await go(page, `/LabReports/Edit/${composite}`);
        if (want('05')) await snap(page, '05-06-approve', { crop: page.locator('#approve-report').locator('xpath=ancestor::form[1]/..'), marks: [[page.locator('#approve-report'), 1]] });
        await press(page, '#approve-report');
        await expectSuccess(page, 'lab approve');
    }

    // ---------------------------------------------------------- 10: the beekeeper portal, before the honey is in a batch (phone)
    log('· 10 portal');
    if (want('10')) {
        const p = beePage;
        await go(p, '/Me');
        await snapPhone(p, '10-01-home');
        await go(p, '/Me/Deliveries');
        await snapPhone(p, '10-02-deliveries');
        const form = p.locator('form.price-form').first();
        await form.locator('[name=pricePerKg]').fill('650');
        await form.scrollIntoViewIfNeeded();
        await snapPhone(p, '10-03-price', form.locator('xpath=..'));
        p.once('dialog', d => d.accept());
        await press(p, form.locator('button[type=submit]'));
        await go(p, '/Me/Samples');
        await snapPhone(p, '10-04-samples');
        await go(p, '/Me/Privacy');
        await snapPhone(p, '10-05-privacy');
        await go(p, '/Me/Account');
        await snapPhone(p, '10-06-account');
    }

    // ---------------------------------------------------------- 6 and 7: control sheets, the batch, finalizing, the shipment
    log('· 6-7 processing');
    {
        const page = await signIn(staff);
        await go(page, '/OrgWorkspace/Index');
        if (want('06')) await snap(page, '06-01-my-organization', { marks: [[page.locator(`a[href*="Sheet?kind=raw&id=${lot}"]`).first(), 1]] });
        await go(page, `/CCPMonitoring/Sheet?kind=raw&id=${lot}`);
        await fillCcpBoxes(page);
        if (want('06')) await snap(page, '06-02-lot-sheet', { scroll: 'top' });
        if (want('06')) await snap(page, '06-03-lot-sheet-save', { scroll: 'bottom', marks: [[page.locator('#sheet-save'), 3]] });
        await press(page, '#sheet-save');
        await expectSuccess(page, 'lot sheet');
    }
    {
        const page = await signIn(qa);
        await go(page, `/CCPMonitoring/Sheet?kind=raw&id=${lot}`);
        if (want('07')) await snap(page, '07-03-approve-sheet', { marks: [[page.locator('#sheet-approve-button'), 2]] });
        await approve(page, '#sheet-approve-button', '#oolc-sheet-approve-form');
        await expectSuccess(page, 'lot sheet approval');
    }
    {
        const page = await signIn(staff);
        await go(page, '/OrgWorkspace/Index');
        await press(page, '#link-new-batch');
        const rows = await page.locator('#new-batch-lots tbody tr').evaluateAll(els => els.map(e => e.dataset.lotId));
        for (const id of rows) if (id !== lot) await page.locator(`#use-${id}`).uncheck();
        await page.fill('#batch-title', BATCH_TITLE);
        if (want('06')) await snap(page, '06-04-new-batch', { marks: [[page.locator('#batch-title'), 2], [page.locator(`#use-${lot}`), 3], [page.locator('#new-batch-submit'), 4]] });
        await press(page, '#new-batch-submit');
        batch = page.url().match(/\/OrgWorkspace\/Batch\/(\d+)/)?.[1];
        if (!batch) throw new Error('new batch: ' + await errorsOf(page));
        await go(page, `/CCPMonitoring/Sheet?kind=batch&id=${batch}`);
        await fillCcpBoxes(page);
        if (want('06')) await snap(page, '06-05-batch-sheet', { scroll: 'top' });
        await press(page, '#sheet-save');
        await expectSuccess(page, 'batch sheet');
    }
    {
        const page = await signIn(qa);
        await go(page, `/CCPMonitoring/Sheet?kind=batch&id=${batch}`);
        await approve(page, '#sheet-approve-button', '#oolc-sheet-approve-form');
        await expectSuccess(page, 'batch sheet approval');
    }
    {
        const page = await signIn(staff);
        await go(page, `/OrgWorkspace/Batch/${batch}`);
        if (want('06')) await snap(page, '06-06-request', { marks: [[page.locator('#request-finalize'), 1]] });
        await press(page, '#request-finalize');
        await expectSuccess(page, 'request finalize');
    }
    {
        const page = await signIn(qa);
        await go(page, `/OrgWorkspace/Batch/${batch}`);
        if (want('07')) await snap(page, '07-04-finalize', { marks: [[page.locator('#confirm-finalize'), 2]] });
        await press(page, '#confirm-finalize');
        await expectSuccess(page, 'finalize');
        qr = (await page.content()).match(/honeystory\/([0-9A-Fa-f-]{36})/)?.[1];
        if (want('07')) await snap(page, '07-05-finalized', { keepFlash: true });
    }
    {
        const page = await signIn(staff);
        await go(page, '/Shipments/Index');
        if (want('06')) await snap(page, '06-07-shipments', { marks: [[page.locator(`a[href*="Shipments/Create?batchId=${batch}"]`).first(), 1]] });
        await go(page, `/Shipments/Create?batchId=${batch}`);
        await page.selectOption('#toOrgId', { label: orgName('shop') });
        await page.fill('#quantity', '20');
        if (want('06')) await snap(page, '06-08-ship', { crop: page.locator('#ship-form'), marks: [[page.locator('#ship-form button[type=submit]'), 4]] });
        await press(page, '#ship-form button[type=submit]');
        await expectSuccess(page, 'shipment');
    }

    // ---------------------------------------------------------- 8: the shop
    log('· 8 shop');
    {
        const page = await signIn(shop);
        await go(page, '/Shipments/Index');
        if (want('08')) await snap(page, '08-01-incoming', { crop: page.locator('#incoming'), marks: [[page.locator('#incoming form.decide-form button[name=confirm][value=true]').first(), 2]] });
        await press(page, '#incoming form.decide-form button[name=confirm][value=true]');
        await expectSuccess(page, 'receipt');
        await go(page, `/Shipments/Storage?batchId=${batch}`);
        const ids = await page.locator('#storage-form [name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) await page.locator(`#storage-form [name="values[${id}]"]`).fill(value(id));
        if (want('08')) await snap(page, '08-02-storage', { marks: [[page.locator('#storage-form button[type=submit]'), 3]] });
        await press(page, '#storage-form button[type=submit]');
        await expectSuccess(page, 'storage');
    }
    {
        const page = await signIn(shopQa);
        await go(page, `/Shipments/Storage?batchId=${batch}`);
        if (want('08')) await snap(page, '08-03-approve-storage', { marks: [[page.locator('#storage-approve-form button[type=submit]'), 2]] });
        await press(page, '#storage-approve-form button[type=submit]');
        await expectSuccess(page, 'storage approval');
    }

    // ---------------------------------------------------------- 11: the Honey Story (phone); 12: the consumer; 7: the answer
    log('· 11-12 Honey Story, consumer');
    {
        const story = await newPage(PHONE);
        await go(story, `/honeystory/${qr}`);
        reportCode = (await story.content()).match(/verify-report\?code=([A-Z0-9]+)/)?.[1];
        if (want('11')) {
            await snapPhone(story, '11-01-top');
            await snapPhone(story, '11-02-journey', story.locator('.hs-journey, #journey, section:has(.hs-step)').first());
            await snapPhone(story, '11-03-origins', story.locator('#hs-sources'));
            await snapPhone(story, '11-04-lab', story.locator('#lab-report'));
            await snapPhone(story, '11-05-process', story.locator('#process'));
            await snapPhone(story, '11-06-account', story.locator('#consumer-anon'));
        }
        if (want('05') && reportCode) {
            const verify = await newPage();
            await go(verify, `/verify-report?code=${reportCode}`);
            await snap(verify, '05-07-verify');
            await verify.context().close();
        }
        await go(story, `/Consumer/SignUp?returnUrl=${encodeURIComponent('/honeystory/' + qr)}`);
        await story.fill('#UserName', CONSUMER);
        await story.fill('#Password', PASSWORD); await story.fill('#ConfirmPassword', PASSWORD);
        await story.selectOption('#Language', LANG).catch(() => {});
        await story.locator('input[name=IsAdult][type=checkbox]').check();
        await story.locator('input[name=AcceptNotice][type=checkbox]').check();
        if (want('12')) await snapPhone(story, '12-01-sign-up', story.locator('#UserName').locator('xpath=ancestor::form[1]'));
        await story.waitForTimeout(3500);
        await press(story, '#consumer-signup-form button[type=submit]');
        await go(story, `/honeystory/${qr}`);
        await story.locator('#consumer-save').click(); await quiet(story);
        if (want('12')) await snapPhone(story, '12-02-save');
        await press(story, '#save-form button[type=submit]');
        await go(story, `/Consumer/Feedback?qr=${qr}`);
        await story.locator('input[name=category]').first().check();
        await story.fill('#message', 'Very nice chestnut honey. Is it raw, and how should I store it?');
        if (want('12')) await snapPhone(story, '12-03-feedback');
        await press(story, '#feedback-form button[type=submit]');

        const page = await signIn(qa);
        await go(page, '/ConsumerFeedback');
        await go(page, await page.locator('a[href*="/ConsumerFeedback/Details/"]').first().getAttribute('href'));
        await page.fill('#answer', 'Thank you! It is raw chestnut honey from Şile, only filtered. Keep it closed at room temperature, away from sunlight.');
        if (want('07')) await snap(page, '07-06-feedback', { marks: [[page.locator('#answer'), 2], [page.locator('#answer-form button[type=submit]'), 3]] });
        await press(page, '#answer-form button[type=submit]');

        await go(story, '/Consumer');
        if (want('12')) await snapPhone(story, '12-04-my-honeys');
        await go(story, '/Consumer/Account');
        if (want('12')) await snapPhone(story, '12-05-account');
    }

    // ---------------------------------------------------------- 9: organization admin and Compliance Officer
    log('· 9 organization admin, compliance');
    if (want('09')) {
        const page = await signIn(orgAdmin);
        await go(page, '/OrgWorkspace/Index');
        await go(page, await page.locator('a[href*="/Organizations/Details/"]').first().getAttribute('href'));
        await snap(page, '09-01-organization');
        await go(page, '/Storefronts/Index');
        await snap(page, '09-02-beekeeper-pages');
        const co = await signIn(compliance);
        await go(co, '/Compliance');
        await snap(co, '09-03-compliance');
    }

    // ---------------------------------------------------------- 13: platform administration
    log('· 13 administration');
    if (want('13')) {
        const page = await signIn(admin);
        await go(page, '/Dashboard/Index');
        await snap(page, '13-01-dashboard');
        await go(page, '/Organizations/Index');
        await snap(page, '13-02-organizations', { marks: [[page.locator('a[href*="/Organizations/Create"]').first(), 1]] });
        await go(page, '/Organizations/Create');
        await snap(page, '13-03-new-organization');
        await go(page, '/Organizations/Index');
        const shopRow = page.locator('tr', { hasText: orgName('shop') }).locator('a[href*="/Organizations/Details/"]').first();
        await go(page, await shopRow.getAttribute('href'));
        await snap(page, '13-04-organization-details');
        await go(page, '/Permissions/Index');
        await snap(page, '13-05-permissions');
        await go(page, '/CcpResponsibilities/Index');
        await snap(page, '13-06-ccp-responsibilities');
        await go(page, '/UserManagement/Index');
        await snap(page, '13-07-users');
        await go(page, '/Records/Index');
        await snap(page, '13-08-records');
        await go(page, '/Audit/Index');
        await snap(page, '13-09-audit');
        await go(page, '/Notices/Index');
        await snap(page, '13-10-notices');
        await go(page, '/ReferenceTranslations/Index');
        await snap(page, '13-11-translations');
        // Phase 1b: the limits of a parameter, the registry, the recall report, data retention, the process flow of the batch
        // the parameter list is paged: open the limits of Electrical Conductivity by its number in the reference data
        const ecId = Object.keys(params).find(id => params[id].name === 'Electrical Conductivity');
        if (!ecId) throw new Error('no parameter Electrical Conductivity');
        await go(page, `/CriticalControlPointParameters/Limits/${ecId}`);
        await snap(page, '13-12-limits', { scroll: 'top' });
        if (business) {
            await go(page, '/Registry/Index');
            const regHref = await page.locator('a[href*="/Registry/Edit/"]').evaluateAll((as, b) => as.map(a => [a.getAttribute('href'), a.closest('tr')?.innerText || ''])
                .find(([, row]) => row.includes(b))?.[0], business);
            if (!regHref) throw new Error('no registry row for ' + business);
            await go(page, regHref);
            await snap(page, '13-13-registry', { scroll: 'top' });
        }
        if (batch) {
            await go(page, `/Recall/Batch/${batch}`);
            await snap(page, '13-14-recall', { scroll: 'top' });
            await go(page, `/Flow/Batch/${batch}`);
            await snap(page, '01-02-flow', { scroll: 'top' });
        }
        await go(page, '/Retention/Index');
        await snap(page, '13-15-retention', { scroll: 'top' });
    }
    // pictures over about 400 KB are stored with a palette (BRIEF.md: keep a picture under about 400 KB)
    const shrink = spawnSync('python', [path.join(HERE, 'shrink.py'), SCREENS], { encoding: 'utf8' });
    if (shrink.status !== 0) log('  shrink.py did not run (Python with Pillow is needed): ' + (shrink.stderr || '').split(/\r?\n/)[0]); else if (shrink.stdout.trim()) log(shrink.stdout.trimEnd());
    log(`done in ${Math.round((Date.now() - started) / 1000)} s: business ${business}, lot ${lot}, batch ${batch}`);
} finally {
    await browser.close();
}
