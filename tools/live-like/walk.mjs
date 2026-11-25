// D-88 (Şile cast since D-90): the end-to-end check in a browser, as the people of the live-like cast (never the platform administrator): a new beekeeper is
// registered, delivers chestnut honey in sealed containers (code read on the beekeeper's phone page), CCP 0, the laboratory, the lot's
// control sheet, New batch, the batch's control sheet, finalizing, the shipment to the shop with its storage record, the public Honey
// Story, and a consumer who creates an account from the story, saves the honey and sends feedback that the association answers.
//   node tools/live-like/walk.mjs            (run-demo.ps1 running; English pages)
//   WALK_LANG=tr node tools/live-like/walk.mjs   (the same walk on Turkish pages; D-97)
// - D-97: the delivery takes its seal labels and the association's container from the seal pool, never the codes reserved for the
//   owner's guide (TS-000001 to TS-000030, TK-0001 to TK-0003).
// - Runs only against localhost. The password and the phone page key are read from the logins file in Downloads, never printed.
// - Saves no screenshots. Writes the steps it did (labels, URLs, messages, timings) as JSON to WALK_OUT when set.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const { chromium } = await import(new URL('../../docs/manual/node_modules/playwright-core/index.mjs', import.meta.url));
const BASE = (process.env.WALK_BASE || 'http://localhost:5255').replace(/\/$/, '');
if (!/^http:\/\/localhost(:\d+)?$/.test(BASE)) throw new Error('walk.mjs runs against the local app only');
const cast = JSON.parse(fs.readFileSync(path.join(here, 'cast.json'), 'utf8'));
const values = JSON.parse(fs.readFileSync(path.join(repo, 'docs', 'partner-test', 'values.json'), 'utf8'));
const loginsText = fs.readFileSync(path.join(os.homedir(), 'Downloads', 'TOP4-local-logins.txt'), 'utf8');
const PASSWORD = loginsText.match(/^Password:\s+(\S+)/m)[1];
const KEY = loginsText.match(/\/phone\/([A-Za-z0-9_-]{16,64})\//)[1];
const bee = cast.beekeepers.find(b => b.key === 'new');
const beeDigits = bee.phone.replace(/\D/g, '');
const user = (role, org) => cast.people.find(p => p.role === role && p.org === org).user;
const orgName = key => cast.orgs.find(o => o.key === key).name;
const BATCH_TITLE = process.env.WALK_BATCH || 'Şile Kestane Balı 2026/2';
const CONSUMER = process.env.WALK_CONSUMER || 'ece.kaplan';
const LANG = process.env.WALK_LANG || 'en';
if (!['en', 'tr', 'es', 'pl', 'lv'].includes(LANG)) throw new Error('WALK_LANG: en, tr, es, pl or lv');
const RESERVED_LABELS = 'TS-000030', RESERVED_BOXES = 'TK-0003';
// reference ids (no personal data): the honey type and the production period of the cast, chosen by id so any language works
const refId = sql => execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', 'TOP4HoneyChainsDb', '-E', '-b', '-W', '-h', '-1', '-f', '65001', '-Q', 'SET NOCOUNT ON; ' + sql],
    { encoding: 'utf8' }).split(/\r?\n/).map(l => l.trim()).find(l => /^\d+$/.test(l));

// the parameters (reference data only): id -> CCP code, name, data type
const params = Object.fromEntries(execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', 'TOP4HoneyChainsDb', '-E', '-b', '-W', '-h', '-1', '-s', '|', '-f', '65001', '-Q',
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

const browser = await (async () => {
    for (const o of [{ channel: 'chrome' }, { channel: 'msedge' }, {}]) { try { return await chromium.launch({ headless: true, ...o }); } catch { /* next */ } }
    throw new Error('no browser');
})();
const started = Date.now();
const facts = { base: BASE, startedAt: new Date().toISOString(), steps: [], records: {} };
const log = (...a) => console.log(...a);

async function settle(page) {
    await page.waitForLoadState('networkidle', { timeout: 8000 }).catch(() => {});
}
async function flash(page) {
    return page.evaluate(() => [...document.querySelectorAll('#flash-success, #flash-error')].map(e => e.innerText.replace(/\s*×\s*$/, '').trim()).filter(Boolean).join(' / '));
}
async function errorsOf(page) {
    return page.evaluate(() => [...document.querySelectorAll('.alert-danger, .validation-summary-errors, [id$="-errors"]')].map(e => e.innerText.trim()).filter(Boolean).join(' / ').slice(0, 400));
}

const actors = {};
async function actor(name) {
    if (actors[name]) return actors[name];
    const context = await browser.newContext({ viewport: { width: 1366, height: 900 }, locale: LANG, reducedMotion: 'reduce' });
    const page = await context.newPage();
    page.setDefaultTimeout(20000);
    await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/Account/Login')}`);
    await page.fill('#UserName', name);
    await page.fill('#Password', PASSWORD);
    await Promise.all([page.waitForURL(u => !/\/Account\/Login/i.test(u.pathname)), page.click('.au-submit')]);
    await settle(page);
    actors[name] = page;
    return page;
}

// a phone: the phone page of one number, open like a messaging app; returns the newest message that arrives after the action
const phones = {};
async function phonePage(digits) {
    if (phones[digits]) return phones[digits];
    const context = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, locale: LANG });
    const page = await context.newPage();
    await page.goto(`${BASE}/phone/${KEY}/${digits}`);
    await page.waitForTimeout(1500);
    phones[digits] = { page, seen: Math.max(0, ...await page.locator('.msg').evaluateAll(els => els.map(e => +e.dataset.id))) };
    return phones[digits];
}
// the newest message that arrived on the phone page after the last one read and matches re
async function nextMessage(digits, re = /\b\d{6}\b/) {
    const phone = await phonePage(digits);
    const deadline = Date.now() + 15000;
    while (Date.now() < deadline) {
        const fresh = await phone.page.locator('.msg').evaluateAll(els => els.map(e => ({ id: +e.dataset.id, text: e.innerText })));
        const hit = fresh.filter(m => m.id > phone.seen && re.test(m.text)).sort((a, b) => b.id - a.id)[0];
        if (hit) { phone.seen = Math.max(phone.seen, hit.id); return hit.text.replace(/\n[^\n]*$/, '').trim(); }
        await phone.page.waitForTimeout(500);
    }
    throw new Error(`no new message matching ${re} on the phone page of ${digits}`);
}
const codeIn = t => t.match(/\b\d{6}\b/)[0];

// the record of one step: who, where, what was clicked and typed (with the labels on the page), the result
let current;
async function step(title, who, fn) {
    const page = who ? await actor(who) : null;
    const t0 = Date.now();
    current = { n: facts.steps.length + 1, title, who: who || 'anonymous', urls: [], did: [], result: '', next: [], seconds: 0 };
    try {
        await fn(page);
        current.seconds = +((Date.now() - t0) / 1000).toFixed(1);
        facts.steps.push(current);
        log(`${String(current.n).padStart(2)} ok   ${current.seconds.toFixed(1).padStart(5)} s  ${title} (${current.who}) ${current.result}`);
    } catch (e) {
        current.seconds = +((Date.now() - t0) / 1000).toFixed(1);
        current.error = e.message.split('\n')[0];
        facts.steps.push(current);
        log(`${String(current.n).padStart(2)} FAIL ${title} (${current.who}): ${current.error}`);
        throw e;
    }
}
async function go(page, url) { await page.goto(BASE + url); await settle(page); current.urls.push(new URL(page.url()).pathname + new URL(page.url()).search); }
async function labelOf(page, selector) {
    return page.locator(selector).first().evaluate(el => (el.labels?.[0]?.innerText || el.getAttribute('aria-label') || el.closest('label')?.innerText || el.name || '').replace(/\s+/g, ' ').trim());
}
async function fill(page, selector, v, show = true) { await page.locator(selector).first().fill(v); current.did.push({ type: 'type', label: await labelOf(page, selector), value: show ? v : '(practice password)' }); }
async function choose(page, selector, label) { await page.locator(selector).first().selectOption({ label }); current.did.push({ type: 'choose', label: await labelOf(page, selector), value: label }); }
async function tick(page, selector) { await page.locator(selector).first().check(); current.did.push({ type: 'tick', label: await labelOf(page, selector) }); }
async function press(page, selector) {
    const button = page.locator(selector).first();
    const text = (await button.innerText().catch(() => '')).replace(/\s+/g, ' ').trim() || await button.getAttribute('value');
    current.did.push({ type: 'click', label: text });
    await Promise.all([page.waitForLoadState('load'), button.click()]);
    await settle(page);
    current.urls.push(new URL(page.url()).pathname + new URL(page.url()).search);
}
async function result(page, expect) {
    const f = await flash(page);
    current.result = f || current.result;
    // D-97: on other languages a success message (and no error) is the check; the English texts are checked on English pages
    if (expect && LANG !== 'en') {
        const ok = await page.locator('#flash-success').count() && !await page.locator('#flash-error').count();
        if (!ok) throw new Error(`expected a success message, got "${f}" ${await errorsOf(page)}`);
        return f;
    }
    if (expect && !expect.test(f)) throw new Error(`expected ${expect}, got "${f}" ${await errorsOf(page)}`);
    return f;
}
// a CCP code on My work: "CCP 0" in English, the language's own abbreviation elsewhere (KKN 0, PCC 0, ...)
const ccp = n => `\\S+ ${n}`;
// what appears in someone's My work next
async function workOf(who, re) {
    const page = await actor(who);
    await page.goto(`${BASE}/MyWork/Index`); await settle(page);
    const items = await page.locator('.work-item').evaluateAll(els => els.map(e => ({ text: e.innerText.replace(/\s+/g, ' ').trim(), href: e.getAttribute('href'), group: e.closest('.work-group')?.querySelector('.work-group-title span:nth-child(2)')?.innerText })));
    const hit = items.find(i => re.test(i.text) || re.test(i.href || ''));
    if (!hit) throw new Error(`nothing matching ${re} in the My work of ${who}`);
    current.next.push({ who, group: hit.group, item: hit.text, href: hit.href });
    return hit;
}
// fills the CCP inputs of a page (single CCP page or control sheet) with the suggested values
async function fillCcpBoxes(page, scope = page) {
    const boxes = await scope.locator('.ccp-param[data-parameter-id]').evaluateAll(els => els.map(e => e.dataset.parameterId));
    for (const id of boxes) {
        const v = value(id);
        if (!v) continue;
        const box = scope.locator(`.ccp-param[data-parameter-id="${id}"]`);
        if (await box.locator('select').count()) await box.locator('select').first().selectOption(v);
        else await box.locator('input[type=text]').first().fill(v);
    }
    current.did.push({ type: 'type', label: `${boxes.length} values (docs/partner-test/values.json)` });
}
// the approve button; when the page asks to confirm values outside their limits, the dialog's confirm button too
async function approveWithDialog(page, buttonSelector, dialogSelector) {
    current.did.push({ type: 'click', label: (await page.locator(buttonSelector).innerText()).trim() });
    const navigation = page.waitForNavigation({ timeout: 4000 }).then(() => true).catch(() => false);
    await page.locator(buttonSelector).click();
    if (!await navigation) {
        const confirm = page.locator(`${dialogSelector} .oolc-confirm`);
        current.did.push({ type: 'click', label: (await confirm.innerText()).trim() + ' (values outside their limits)' });
        await Promise.all([page.waitForNavigation(), confirm.click()]);
    }
    await settle(page);
    current.urls.push(new URL(page.url()).pathname + new URL(page.url()).search);
}

// D-97: free seal labels and the association's last permanent container, read from the Seals page (never the guide's reserved codes)
async function freeCodes(who) {
    const page = await actor(who);
    await page.goto(`${BASE}/Seals/Index?status=Issued&kind=Label`); await settle(page);
    const labels = (await page.locator('#seal-codes tr[data-code]').evaluateAll(els => els.map(e => e.dataset.code)))
        .filter(c => /^TS-\d{6}$/.test(c) && c > RESERVED_LABELS).sort();
    const boxes = (await page.locator('#org-containers tr[data-box]').evaluateAll(els => els.map(e => e.dataset.box))).filter(c => c > RESERVED_BOXES).sort();
    return { labels, box: boxes[boxes.length - 1] };
}

const reg = user('RegistrationOfficer', 'directorate'), sao = user('SampleAcceptanceOfficer', 'association'), staff = user('Staff', 'association'), qa = user('QualityApprover', 'association');
const analyst = user('Analyst', 'lab'), labManager = user('LabManager', 'lab'), shop = user('Staff', 'shop'), shopQa = user('QualityApprover', 'shop');
let business, portalUser, lot, intake, batch, qr, composite;

try {
    await step('Register the new beekeeper (SMS code from the beekeeper\'s phone)', reg, async page => {
        await phonePage(beeDigits);
        await go(page, '/Registration/Index');
        await fill(page, '#number', bee.registry);
        await press(page, '#registration-find button[type=submit]');
        await fill(page, '#FirstName', bee.first);
        await fill(page, '#LastName', bee.last);
        await fill(page, '#BirthDate', bee.birth);
        await fill(page, '#Phone', bee.phone);
        await choose(page, '#Language', 'Türkçe').catch(() => {});
        await choose(page, '#AssociationOrgId', orgName('association'));
        await choose(page, '#ap-province', bee.province);
        await page.waitForFunction(() => document.getElementById('ap-district').options.length > 1);
        await choose(page, '#ap-district', bee.district);
        await fill(page, '#ap-locality', bee.locality);
        await fill(page, '#ApiaryHives', bee.hives);
        await fill(page, '#ApiaryLocationLatitude', bee.lat);
        await fill(page, '#ApiaryLocationLongitude', bee.lng);
        await tick(page, '#PublicName');
        await tick(page, '#PortalNotifications');
        await tick(page, '#Attest');
        if (await page.locator('#IssueSignIn').count()) await tick(page, '#IssueSignIn');
        await press(page, '#registration-send');
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + await errorsOf(page));
        const sms = await nextMessage(beeDigits);
        current.did.push({ type: 'phone', label: 'SMS on the beekeeper\'s phone page', value: sms.replace(/\b\d{6}\b/, '######') });
        await fill(page, '#Code', codeIn(sms));
        await press(page, '#registration-confirm');
        await result(page, /registered/i);
        business = (await page.evaluate(() => document.body.innerText)).match(/TR-BEE-\d{4}-\d{6}/)?.[0];
        facts.records.registration = page.url().split('/').pop();
        facts.records.business = business;
    });

    await step('The beekeeper signs in to the portal with the SMS details', null, async () => {
        const sms = await nextMessage(beeDigits, /TR-BEE-\d{4}-\d{6}/);
        current.did.push({ type: 'phone', label: 'SMS with the sign-in details', value: sms.replace(/(?<![A-Za-z0-9-])[A-HJ-NP-Za-km-z2-9]{10}(?![A-Za-z0-9-])/g, '(temporary password)') });
        portalUser = sms.match(/TR-BEE-\d{4}-\d{6}/)?.[0] || business;
        const context = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, locale: LANG });
        const page = await context.newPage();
        let ok = false;
        for (const candidate of sms.match(/(?<![A-Za-z0-9-])[A-HJ-NP-Za-km-z2-9]{10}(?![A-Za-z0-9-])/g) || []) {
            await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/Account/Login')}`);
            await page.fill('#UserName', portalUser);
            await page.fill('#Password', candidate);
            await Promise.all([page.waitForLoadState('load'), page.click('.au-submit')]);
            if (!/\/Account\/Login/i.test(new URL(page.url()).pathname)) { ok = true; break; }
        }
        if (!ok) throw new Error('the temporary password did not work');
        current.urls.push(new URL(page.url()).pathname);
        if (/FirstPassword/i.test(page.url())) {
            const fields = page.locator('input[type=password]');
            await fields.nth(0).fill(PASSWORD); await fields.nth(1).fill(PASSWORD);
            current.did.push({ type: 'type', label: 'New password, twice', value: '(practice password)' });
            await Promise.all([page.waitForLoadState('load'), page.locator('form button[type=submit]').first().click()]);
        }
        await settle(page);
        current.urls.push(new URL(page.url()).pathname);
        current.result = `portal account ${portalUser}`;
        facts.records.portalUser = portalUser;
        await context.close();
    });

    await step('Record the delivery in sealed containers (code from the beekeeper\'s phone)', sao, async page => {
        await go(page, '/Intake/Index');
        await press(page, '#new-intake');
        if (await page.locator('input[name=number]').count() && !await page.locator('#ApiaryId').count()) {
            await fill(page, 'input[name=number]', business);
            await press(page, 'form:has(input[name=number]) button[type=submit]');
        }
        if (!(await page.locator('#ApiaryId').inputValue())) await page.locator('#ApiaryId').selectOption({ index: 1 });
        current.did.push({ type: 'choose', label: await labelOf(page, '#ApiaryId'), value: (await page.locator('#ApiaryId option:checked').innerText()).trim() });
        const honeyId = refId(`SELECT HoneyTypeId FROM dbo.HoneyTypes WHERE HoneyTypeTitle = N'${cast.history.honeyType.replace(/'/g, "''")}'`);
        const periodId = refId(`SELECT MAX(RawHoneyProductionPeriodId) FROM dbo.RawHoneyProductionPeriods WHERE RawHoneyProductionPeriodTitle = N'${cast.history.period.title.replace(/'/g, "''")}'`);
        await page.selectOption('#HoneyTypeId', honeyId);
        current.did.push({ type: 'choose', label: await labelOf(page, '#HoneyTypeId'), value: (await page.locator('#HoneyTypeId option:checked').innerText()).trim() });
        await page.selectOption('#PeriodId', periodId);
        current.did.push({ type: 'choose', label: await labelOf(page, '#PeriodId'), value: (await page.locator('#PeriodId option:checked').innerText()).trim() });
        const prov = page.locator('select[data-province-select]').first();
        await prov.selectOption({ label: bee.province });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ label: bee.district });
        current.did.push({ type: 'choose', label: 'Province / District', value: `${bee.province} / ${bee.district}` });
        if (!(await page.locator('#DeliveryDate').inputValue())) await fill(page, '#DeliveryDate', new Date().toISOString().slice(0, 10));
        await page.selectOption('#OriginCountry', 'TR').catch(() => {});
        await choose(page, '#LabOrgId', orgName('lab'));
        if (!(await page.locator('#RetainUntil').inputValue())) { const d = new Date(); d.setFullYear(d.getFullYear() + 2); await fill(page, '#RetainUntil', d.toISOString().slice(0, 10)); }
        await fill(page, '#CounterLocation', cast.history.counterLocation);
        // D-97: one card per container; the codes come from the seal pool (free codes after the ones reserved for the guide)
        const free = await freeCodes(sao);
        if (free.labels.length < 6 || !free.box) throw new Error('the seal pool has too few free codes: issue seal labels on the Seals page');
        current.did.push({ type: 'choose', label: await page.locator('input[name=CapType]:checked').evaluate(e => e.closest('.ci-tile').querySelector('.ci-tile-title').innerText.trim()), value: 'SafetyButton' });
        if (await page.locator('[data-card]').count() < 2) { await page.click('#add-container'); current.did.push({ type: 'click', label: 'Add container' }); }
        const cards = page.locator('#ci-cards [data-card]');
        const plan = [
            { closure: 'SealLabel', net: '23.8', gross: '25.2', jar: free.labels[0], counter: free.labels[1], closing: free.labels[2] },
            { closure: 'OrgContainer', box: free.box, net: '21.6', gross: '23.0', jar: free.labels[3], counter: free.labels[4], closing: free.labels[5] }
        ];
        for (let i = 0; i < plan.length; i++) {
            const card = cards.nth(i), p = plan[i];
            const f = n => card.locator(`[data-field="${n}"]`);
            await card.locator(`[data-field="Closure"][value="${p.closure}"]`).check();
            const closureText = (await card.locator(`[data-field="Closure"][value="${p.closure}"]`).evaluate(e => e.closest('.ci-tile').querySelector('.ci-tile-title').innerText)).trim();
            if (p.box) {
                await f('OrgContainerCode').fill(p.box);
                await card.locator('.ci-box.is-ok').waitFor({ timeout: 8000 });
            } else await f('Kind').selectOption({ index: 0 });
            await f('NetKg').fill(p.net);
            await f('GrossKg').fill(p.gross);
            await card.locator('[data-field="ArrivedSealed"][value="false"]').check();
            for (const [field, code] of [['JarSeal', p.jar], ['CounterSeal', p.counter], ['ClosingSeal', p.closing]]) {
                await f(field).fill(code);
                await card.locator(`[data-field="${field}"].is-ok`).waitFor({ timeout: 8000 });   // the live check: in the pool, unused
            }
            await f('CapIntact').check();
            if (await card.locator('.ci-step.is-done').count() !== 3) throw new Error(`container ${i + 1}: not every step is marked done`);
            current.did.push({ type: 'type', label: `Container ${i + 1}`, value: `${closureText}${p.box ? ' ' + p.box : ''}, net ${p.net} kg, gross ${p.gross} kg, arrived unsealed, sample jar ${p.jar}, counter-sample ${p.counter}, button down, closing seal ${p.closing}` });
        }
        const confirms = (await page.locator('#ci-confirm-list').innerText()).replace(/\s+/g, ' ');
        for (const code of [free.box, ...free.labels.slice(0, 6)]) if (!confirms.includes(code)) throw new Error(`${code} is missing from what the beekeeper confirms`);
        await press(page, '#intake-send');
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + await errorsOf(page));
        const sms = await nextMessage(beeDigits);
        current.did.push({ type: 'phone', label: 'SMS on the beekeeper\'s phone page', value: sms.replace(/\b\d{6}\b/, '######') });
        await fill(page, '#Code', codeIn(sms));
        await press(page, '#intake-confirm');
        intake = page.url().match(/\/(\d+)(\?|$)/)?.[1];
        current.result = await flash(page) || page.url();
        lot = current.result.match(/lot #(\d+)/)?.[1] ?? (await page.content()).match(/(?:lotId|rawHoneyProductionId)=(\d+)/)?.[1];
        if (!lot) throw new Error('no lot number after the delivery: ' + current.result);
        facts.records.intake = intake; facts.records.lot = lot;
        await workOf(sao, new RegExp(`${ccp(0)} · #${lot}`));
    });

    await step('CCP 0 (primary audit) entered', sao, async page => {
        await go(page, `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`);
        await fillCcpBoxes(page);
        await press(page, 'form.field-form button[type=submit]');
        await result(page, /saved/i);
        await workOf(qa, new RegExp(`${ccp(0)} · #${lot}`));
    });
    await step('CCP 0 approved', qa, async page => {
        await go(page, `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`);
        await approveWithDialog(page, '#approve-button', '#oolc-approve-form');
        await result(page, /approved/i);
    });

    await step('Laboratory receives the jars and makes the composite sample', analyst, async page => {
        await go(page, '/Samples/Index');
        for (let i = 0; i < 4; i++) {
            const form = page.locator('form.receive-jar-form').first();
            if (!await form.count()) break;
            for (const name of ['sealMatches', 'labelIntact', 'capIntact']) await form.locator(`input[name=${name}][value=true]`).check();
            current.did.push({ type: 'tick', label: 'Seal matches · Label intact · Cap intact: Yes' });
            await press(page, 'form.receive-jar-form button[type=submit]');
        }
        await press(page, '#composite-ready button[type=submit], form[action*="Composite"] button[type=submit]');
        if (!/LabReports\/Edit/.test(page.url())) throw new Error('composite: ' + await flash(page));
        composite = page.url().split('/').pop();
        current.result = 'composite sample, report form open';
    });
    await step('Laboratory results entered and submitted', analyst, async page => {
        const reportNumber = cast.history.lab.reportPrefix + '1203';
        await fill(page, '#reportNumber', reportNumber);
        await fill(page, '#issuedOn', new Date().toISOString().slice(0, 10));
        const ids = await page.locator('[name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) {
            const input = page.locator(`[name="values[${id}]"]`);
            if ((await input.evaluate(e => e.tagName)) === 'SELECT') await input.selectOption(value(id)); else await input.fill(value(id));
        }
        current.did.push({ type: 'type', label: `${ids.length} results (docs/partner-test/values.json, chestnut: conductivity 1.24)` });
        const pdf = path.join(os.tmpdir(), `walk-${reportNumber}.pdf`);
        fs.writeFileSync(pdf, '%PDF-1.4\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n');
        await page.setInputFiles('#reportFile', pdf);
        current.did.push({ type: 'file', label: 'Report (PDF)', value: `${reportNumber}.pdf` });
        await press(page, '#required-save, #report-form button[type=submit]');
        fs.rmSync(pdf, { force: true });
        await press(page, '#submit-report');
        await result(page, /submitted/i);
        facts.records.report = reportNumber;
        await workOf(labManager, new RegExp(reportNumber.replace(/-/g, '.') + '|LabReports/Edit'));
    });
    await step('Laboratory report approved', labManager, async page => {
        await go(page, `/LabReports/Edit/${composite}`);
        await press(page, '#approve-report');
        await result(page, /approved/i);
        // the approved report writes CCP 1 of the lot, completed and approved (four-eyes: analyst and lab manager)
        await workOf(staff, new RegExp(`kind=raw&id=${lot}`));
    });

    await step('Lot control sheet (CCP 2–3) entered', staff, async page => {
        await go(page, `/CCPMonitoring/Sheet?kind=raw&id=${lot}`);
        await fillCcpBoxes(page);
        await press(page, '#sheet-save');
        await result(page, /Control sheet saved/i);
        await workOf(qa, new RegExp(`kind=raw&id=${lot}`));
    });
    await step('Lot control sheet approved in one action', qa, async page => {
        await go(page, `/CCPMonitoring/Sheet?kind=raw&id=${lot}`);
        await approveWithDialog(page, '#sheet-approve-button', '#oolc-sheet-approve-form');
        await result(page, /Approved:/);
        await workOf(staff, /OrgWorkspace\/NewBatch/);
    });

    await step('New batch from the approved lot', staff, async page => {
        await go(page, '/OrgWorkspace/Index');
        await press(page, '#link-new-batch');
        const rows = await page.locator('#new-batch-lots tbody tr').evaluateAll(els => els.map(e => e.dataset.lotId));
        for (const id of rows) if (id !== lot) await page.locator(`#use-${id}`).uncheck();
        await fill(page, '#batch-title', BATCH_TITLE);
        current.did.push({ type: 'tick', label: `Use lot #${lot}`, value: `kilograms prefilled: ${await page.locator(`input[name="kg[${lot}]"]`).inputValue()}` });
        await press(page, '#new-batch-submit');
        batch = page.url().match(/\/OrgWorkspace\/Batch\/(\d+)/)?.[1];
        if (!batch) throw new Error('batch: ' + await errorsOf(page));
        await result(page, /Batch created/i);
        facts.records.batch = batch;
        await workOf(staff, new RegExp(`kind=batch&id=${batch}`));
    });
    await step('Batch control sheet (CCP 4–10) entered', staff, async page => {
        await go(page, `/CCPMonitoring/Sheet?kind=batch&id=${batch}`);
        await fillCcpBoxes(page);
        await press(page, '#sheet-save');
        await result(page, /Control sheet saved/i);
        await workOf(qa, new RegExp(`kind=batch&id=${batch}`));
    });
    await step('Batch control sheet approved in one action', qa, async page => {
        await go(page, `/CCPMonitoring/Sheet?kind=batch&id=${batch}`);
        await approveWithDialog(page, '#sheet-approve-button', '#oolc-sheet-approve-form');
        await result(page, /Approved:/);
    });
    await step('Finalizing requested', staff, async page => {
        await go(page, `/OrgWorkspace/Batch/${batch}`);
        await press(page, '#request-finalize');
        await result(page, /requested/i);
        await workOf(qa, new RegExp(`OrgWorkspace/Batch/${batch}`));
    });
    await step('Batch finalized (sealed, Honey Story published)', qa, async page => {
        await go(page, `/OrgWorkspace/Batch/${batch}`);
        await press(page, '#confirm-finalize');
        await result(page, /finalized/i);
        qr = (await page.content()).match(/honeystory\/([0-9A-Fa-f-]{36})/)?.[1];
        facts.records.qr = qr;
    });

    await step('Shipment to the shop', staff, async page => {
        await go(page, `/Shipments/Create?batchId=${batch}`);
        await choose(page, '#toOrgId', orgName('shop'));
        await fill(page, '#quantity', '20');
        await press(page, '#ship-form button[type=submit]');
        await result(page, /Shipment created/i);
        await workOf(shop, new RegExp(BATCH_TITLE.replace(/[/]/g, '.')));
    });
    await step('The shop confirms receipt', shop, async page => {
        await go(page, '/Shipments/Index');
        await press(page, '#incoming form.decide-form button[name=confirm][value=true]');
        await result(page, /Receipt confirmed/i);
    });
    await step('The shop records storage (CCP 10)', shop, async page => {
        await go(page, `/Shipments/Storage?batchId=${batch}`);
        const ids = await page.locator('#storage-form [name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) await page.locator(`#storage-form [name="values[${id}]"]`).fill(value(id));
        current.did.push({ type: 'type', label: `${ids.length} values (temperature 18, relative humidity 55)` });
        await press(page, '#storage-form button[type=submit]');
        await result(page, /saved/i);
        await workOf(shopQa, /CCP 10/);
    });
    await step('The shop\'s QA officer approves storage', shopQa, async page => {
        await go(page, `/Shipments/Storage?batchId=${batch}`);
        await press(page, '#storage-approve-form button[type=submit]');
        await result(page, /approved/i);
    });

    let story;
    await step('Public Honey Story (phone, no sign-in)', null, async () => {
        const context = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, locale: LANG });
        story = await context.newPage();
        await story.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/honeystory/' + qr)}`);
        await settle(story);
        current.urls.push(new URL(story.url()).pathname);
        const text = await story.evaluate(() => document.body.innerText);
        if (!text.includes(bee.first)) throw new Error('the Honey Story does not name the beekeeper');
        current.result = 'beekeeper, apiary place, lab report and every CCP shown';
    });

    await step('A consumer creates an account from the Honey Story and saves the honey', null, async () => {
        await story.locator('#consumer-anon a[href*="/Consumer/SignUp"]').click(); await settle(story);
        current.did.push({ type: 'click', label: (await story.title()) });
        current.urls.push(new URL(story.url()).pathname);
        await story.fill('#UserName', CONSUMER); current.did.push({ type: 'type', label: await story.locator('#UserName').evaluate(e => e.labels?.[0]?.innerText.trim()), value: CONSUMER });
        await story.fill('#Password', PASSWORD); await story.fill('#ConfirmPassword', PASSWORD);
        current.did.push({ type: 'type', label: 'Password, Confirm password', value: '(practice password)' });
        await story.selectOption('#Language', 'en').catch(() => {});
        await story.locator('input[name=IsAdult][type=checkbox]').check();
        await story.locator('input[name=AcceptNotice][type=checkbox]').check();
        current.did.push({ type: 'tick', label: 'I am 18 or older · I have read the privacy notice' });
        await story.waitForTimeout(3500);
        await Promise.all([story.waitForLoadState('load'), story.locator('#consumer-signup-form button[type=submit]').click()]);
        await settle(story);
        current.urls.push(new URL(story.url()).pathname);
        await story.locator('#consumer-save').click(); await settle(story);
        current.urls.push(new URL(story.url()).pathname + new URL(story.url()).search);
        current.did.push({ type: 'click', label: 'Save this honey' });
        await Promise.all([story.waitForLoadState('load'), story.locator('#save-form button[type=submit]').click()]);
        await settle(story);
        current.urls.push(new URL(story.url()).pathname);
        current.result = await flash(story);
        facts.records.consumer = CONSUMER;
    });
    await step('The consumer sends feedback to the association', null, async () => {
        await story.goto(`${BASE}/Consumer/Feedback?qr=${qr}`); await settle(story);
        current.urls.push(new URL(story.url()).pathname + new URL(story.url()).search);
        const category = story.locator('select[name=category]');
        if (await category.count()) { await category.selectOption({ index: 1 }); current.did.push({ type: 'choose', label: 'Category', value: (await category.locator('option:checked').innerText()).trim() }); }
        else { await story.locator('input[name=category]').first().check(); current.did.push({ type: 'choose', label: 'Category', value: (await story.locator('input[name=category]').first().evaluate(e => e.closest('label')?.innerText || e.value)).trim() }); }
        const message = 'Very nice chestnut honey. Is it raw, and how should I store it?';
        await story.fill('#message', message);
        current.did.push({ type: 'type', label: 'Message', value: message });
        await Promise.all([story.waitForLoadState('load'), story.locator('#feedback-form button[type=submit]').click()]);
        await settle(story);
        current.urls.push(new URL(story.url()).pathname);
        current.result = await flash(story);
        await workOf(qa, /ConsumerFeedback\/Details/);
    });
    await step('The association answers the feedback', qa, async page => {
        const item = current.next[0] ?? await workOf(qa, /ConsumerFeedback\/Details/);
        await go(page, '/ConsumerFeedback');
        await go(page, (await page.locator('a[href*="/ConsumerFeedback/Details/"]').first().getAttribute('href')));
        await fill(page, '#answer', 'Thank you! It is raw chestnut honey from Şile, only filtered. Keep it closed at room temperature, away from sunlight.');
        await press(page, '#answer-form button[type=submit]');
        await result(page, /answer/i);
    });
    await step('The consumer reads the answer', null, async () => {
        await story.goto(`${BASE}/Consumer`); await settle(story);
        current.urls.push('/Consumer');
        const text = await story.locator('#my-feedback').innerText();
        if (!/raw chestnut honey/i.test(text)) throw new Error('the answer is not shown to the consumer');
        current.result = 'answer shown under My feedback; the honey under My honeys';
    });
} finally {
    facts.totalSeconds = +((Date.now() - started) / 1000).toFixed(1);
    if (process.env.WALK_OUT) fs.writeFileSync(process.env.WALK_OUT, JSON.stringify(facts, null, 2));
    log(`total ${facts.totalSeconds} s`);
    await browser.close();
}
