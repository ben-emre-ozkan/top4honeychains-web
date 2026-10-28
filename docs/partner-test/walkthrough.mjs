// D-86: walks the partner test scenario "Follow one honey" through a LOCAL demo site, as the testers do it, with the demo accounts of
// one country. It checks that the guide's steps work and saves one screenshot per step outside the repository.
//   WALK_BASE=http://localhost:5256 WALK_COUNTRY=AR PARTNER_PACK=<pack folder> node walkthrough.mjs
// - Runs only against localhost (a local demo site on a demo database, never the live site).
// - Passwords are read at run time from <pack>\<country folder>\credentials.txt and never printed.
// - SMS codes and sign-in details are read in the demo SMS inbox, like a tester reads them.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const { chromium } = await import(new URL('../manual/node_modules/playwright-core/index.mjs', import.meta.url));
const BASE = (process.env.WALK_BASE || 'http://localhost:5256').replace(/\/$/, '');
if (!/^http:\/\/localhost(:\d+)?$/.test(BASE)) throw new Error('walkthrough.mjs runs against a local demo site only');
const COUNTRY = (process.env.WALK_COUNTRY || 'AR').toUpperCase();
const LANG = process.env.WALK_LANG || 'en';
const PACK = process.env.PARTNER_PACK;
const FOLDERS = { AR: 'AR-Argentina', PL: 'PL-Poland', LV: 'LV-Latvia', TR: 'TR-Turkiye' };
const OUT = path.join(os.tmpdir(), `top4-walkthrough-${COUNTRY.toLowerCase()}`);
fs.rmSync(OUT, { recursive: true, force: true });
fs.mkdirSync(OUT, { recursive: true });

const manifest = JSON.parse(fs.readFileSync(path.join(here, 'demo-manifest.json'), 'utf8'));
const values = JSON.parse(fs.readFileSync(path.join(here, 'values.json'), 'utf8'));
const country = manifest.Countries[COUNTRY];
const prefix = COUNTRY.toLowerCase();
const PLAN = {
    AR: { registry: 'AR-DEMO-0002', phone: '+54 9 343 455-03', district: 'Paraná', locality: 'Oro Verde', lat: '-31.8250', lng: '-60.5170', origin: 'AR', botanical: 'Multifloral', countryName: 'Argentina', price: '5400' },
    PL: { registry: 'PL-DEMO-0002', phone: '+48 512 000 3', district: 'Puławy', locality: 'Żyrzyn', lat: '51.4990', lng: '22.0910', origin: 'PL', botanical: 'Multifloral', countryName: 'Polska', price: '39' },
    LV: { registry: 'LV-DEMO-0002', phone: '+371 2000 03', district: 'Jelgava', locality: 'Ozolnieki', lat: '56.6890', lng: '23.7790', origin: 'LV', botanical: 'Multifloral', countryName: 'Latvija', price: '9.8' },
    TR: { registry: 'TR4800000002', phone: '+90 555 000 93 ', district: 'Milas', locality: 'Kırcağız', lat: '37.3160', lng: '27.7840', origin: 'TR', botanical: 'Thyme', countryName: 'Türkiye', price: '660' },
}[COUNTRY];

// ---- credentials (kept in memory only)
const passwords = {};
for (const line of fs.readFileSync(path.join(PACK, FOLDERS[COUNTRY], 'credentials.txt'), 'utf8').split(/\r?\n/)) {
    const m = line.match(/(\S+)\s+([A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4})\s*$/);
    if (m) passwords[m[1]] = m[2];
}

const browser = await (async () => {
    for (const o of [{ channel: 'chrome' }, { channel: 'msedge' }, {}]) { try { return await chromium.launch({ headless: true, ...o }); } catch { /* next */ } }
    throw new Error('no browser');
})();
const log = (...a) => console.log(...a);
let stepNo = 0;
const results = [];

async function settle(page) {
    await page.waitForLoadState('networkidle', { timeout: 8000 }).catch(() => {});
    await page.waitForTimeout(150);
}
async function flash(page) {
    return page.evaluate(() => [...document.querySelectorAll('#flash-success, #flash-error, .alert-success, .alert-danger, .au-alert, .text-danger.validation-summary-errors')]
        .map(e => e.innerText.trim()).filter(Boolean).join(' / ').slice(0, 300));
}
async function submit(page, locator) { await Promise.all([page.waitForLoadState('load'), locator.click()]); await settle(page); }

const actors = {};
async function actor(user, password) {
    if (actors[user]) return actors[user];
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, locale: LANG, reducedMotion: 'reduce' });
    const page = await context.newPage();
    page.setDefaultTimeout(15000);
    page.on('dialog', d => d.accept());
    await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/Account/Login')}`);
    await page.fill('#UserName', user);
    await page.fill('#Password', password ?? passwords[user]);
    await Promise.all([page.waitForURL(u => !/\/Account\/Login/i.test(u.pathname)), page.click('.au-submit')]);
    await settle(page);
    actors[user] = page;
    await notices(page);
    return page;
}

// the newest message in the demo SMS inbox that matches
async function inbox(page, re) {
    // like a tester: the inbox opens in a new tab, the form stays as it is
    const tab = await page.context().newPage();
    await tab.goto(`${BASE}/Demo/Inbox`);
    const texts = await tab.locator('#demo-inbox tbody td.sms-text').allInnerTexts();
    await tab.close();
    const found = texts.find(t => re.test(t));
    if (!found) throw new Error(`no message matching ${re} in the demo SMS inbox`);
    return found;
}
const codeIn = t => t.match(/\b\d{6}\b/)[0];

// the notice gate at the first sign-in (Notices:Enforce on the demo site)
async function notices(page) {
    if (!/\/Notices\/Accept/i.test(page.url())) return;
    if (await page.locator('#notice-send-code').count()) {
        await submit(page, page.locator('#notice-send-code button[type=submit]'));
        await page.fill('#code', codeIn(await inbox(page, /\b\d{6}\b/)));
    }
    await page.locator('#notice-accept input[name=read]').check();
    await submit(page, page.locator('#notice-accept button[type=submit]').last());
    if (/\/Notices\/Accept/i.test(page.url())) throw new Error('notice not accepted: ' + await flash(page));
}

async function step(title, who, fn) {
    stepNo++;
    const page = await actor(who);
    const started = Date.now();
    try {
        const note = await fn(page);
        await page.screenshot({ path: path.join(OUT, `${String(stepNo).padStart(2, '0')}-${title.replace(/[^\w]+/g, '-').toLowerCase()}.png`) });
        results.push({ n: stepNo, title, who, ok: true, note: note || '' });
        log(`${String(stepNo).padStart(2)} ok   ${title} (${who}, ${((Date.now() - started) / 1000).toFixed(1)} s) ${note || ''}`);
    } catch (e) {
        await page.screenshot({ path: path.join(OUT, `${String(stepNo).padStart(2, '0')}-FAILED.png`), fullPage: true }).catch(() => {});
        results.push({ n: stepNo, title, who, ok: false, note: e.message.split(String.fromCharCode(10))[0] });
        log(`${String(stepNo).padStart(2)} FAIL ${title} (${who}): ${e.message.split('\n')[0]}`);
        throw e;
    }
}

// fills a CCP page with the suggested values; returns the number of parameters
async function fillCcp(page, ccpCode) {
    const params = manifest.Parameters.filter(p => p.Ccp === ccpCode && p.Type !== 'File');
    for (const p of params) {
        const box = page.locator(`.ccp-param[data-parameter-id="${p.Id}"]`);
        if (!await box.count()) continue;
        const key = `${p.Ccp}|${p.Name.trim()}`;
        let v;
        if (p.Type === 'Number') v = values.numbers[key];
        else if (values.texts[key]) v = values.texts[key].replace('{country}', PLAN.countryName).replace('{botanical}', PLAN.botanical);
        else if (p.Ccp === 'CCP 0') v = p.Type === 'YesNo' ? values.ccp0.yesNo
            : Object.entries(values.ccp0).find(([k]) => k !== 'yesNo' && k !== 'text' && p.Name.startsWith(k))?.[1] ?? values.ccp0.text;
        else v = values.otherYesNo;
        const select = box.locator('select');
        if (await select.count()) await select.first().selectOption(v);
        else await box.locator('input[type=text]').first().fill(v);
    }
    return params.length;
}
const ccpId = code => ({ 'CCP 0': 1, 'CCP 1': 2, 'CCP 2': 3, 'CCP 3': 4, 'CCP 4': 5, 'CCP 5': 6, 'CCP 6': 7, 'CCP 7': 8, 'CCP 8': 9, 'CCP 9': 10, 'CCP 10': 11 })[code];
async function saveCcp(page, kind, owner, code) {
    const url = kind === 'raw' ? `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${owner}&ccpId=${ccpId(code)}`
        : `/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId=${owner}&ccpId=${ccpId(code)}`;
    await page.goto(BASE + url); await settle(page);
    const n = await fillCcp(page, code);
    await submit(page, page.locator('form.field-form button[type=submit], form[action*="ManageCCP"] button[type=submit]').first());
    const f = await flash(page);
    if (!/saved/i.test(f)) throw new Error(`${code}: ${f}`);
    return `${n} values`;
}
async function approveCcp(page, kind, owner, code) {
    const url = kind === 'raw' ? `/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${owner}&ccpId=${ccpId(code)}`
        : `/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId=${owner}&ccpId=${ccpId(code)}`;
    await page.goto(BASE + url); await settle(page);
    await submit(page, page.locator('#approve-form button[type=submit]').first());
    const f = await flash(page);
    if (!/approved/i.test(f)) throw new Error(`${code} approval: ${f}`);
}

const U = k => `${prefix}.${k}`;
const tag = String(Date.now() % 90 + 10);
const registry = PLAN.registry + tag;
const phone = PLAN.phone + tag;
let regId, business, lot, intake, batch, qr, beePassword;

try {
    // 1. registration
    await step('Register a beekeeper', U('registration'), async page => {
        await page.goto(`${BASE}/Registration/Index`); await settle(page);
        await page.fill('#number', registry);
        await submit(page, page.locator('#registration-find button[type=submit]'));
        if (!/Registration\/New/.test(page.url())) throw new Error('find: ' + await flash(page));
        await page.fill('#FirstName', '[DEMO] Walk');
        await page.fill('#LastName', 'Through ' + tag);
        await page.fill('#BirthDate', '1980-05-17');
        await page.fill('#Phone', phone);
        await page.selectOption('#Language', LANG === 'en' ? ({ AR: 'es', PL: 'pl', LV: 'lv', TR: 'tr' })[COUNTRY] : LANG).catch(() => {});
        await page.selectOption('#ap-province', { label: country.Province });
        await page.waitForFunction(() => document.getElementById('ap-district').options.length > 1);
        await page.selectOption('#ap-district', { label: PLAN.district });
        await page.fill('#ap-locality', PLAN.locality);
        await page.fill('#ApiaryHives', '24');
        await page.fill('#ApiaryLocationLatitude', PLAN.lat);
        await page.fill('#ApiaryLocationLongitude', PLAN.lng);
        await page.check('#PublicName');
        await page.check('#PortalNotifications');
        await page.check('#Attest');
        if (await page.locator('#IssueSignIn').count()) await page.check('#IssueSignIn');
        await submit(page, page.locator('#registration-send'));
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + (await page.locator('#registration-errors').innerText().catch(() => '')));
        await page.fill('#Code', codeIn(await inbox(page, /\b\d{6}\b/)));
        await submit(page, page.locator('#registration-confirm'));
        const f = await flash(page);
        if (!/registered/i.test(f)) throw new Error(f);
        regId = page.url().split('/').pop();
        business = (await page.evaluate(() => document.body.innerText)).match(/[A-Z]{2}-BEE-DEMO-\d{6}/)[0];
        return `${business}; ${f}`;
    });

    // 2. sign-in details for the portal (if not sent with the registration)
    await step('Sign-in details for the beekeeper portal', U('registration'), async page => {
        let sms = await inbox(page, new RegExp(business)).catch(() => null);
        if (!sms) {
            await page.goto(`${BASE}/Registration/Details/${regId}`); await settle(page);
            await submit(page, page.locator('#issue-sign-in button[type=submit]'));
            sms = await inbox(page, new RegExp(business));
        }
        const candidates = sms.match(/(?<![A-Za-z0-9-])[ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789]{10}(?![A-Za-z0-9-])/g) || [];
        beePassword = candidates;
        return `${candidates.length} candidate(s) for the temporary password`;
    });

    // 3. delivery with two containers, confirmed by the beekeeper's SMS code
    await step('Record a delivery with sealed containers', U('reception'), async page => {
        await page.goto(`${BASE}/Intake/Create?number=${encodeURIComponent(business)}`); await settle(page);
        await page.selectOption('#HoneyTypeId', { index: 1 }).catch(() => {});
        await page.selectOption('#OriginCountry', PLAN.origin).catch(() => {});
        const prov = page.locator('select[data-province-select]').first();
        await prov.selectOption({ label: country.Province });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ label: PLAN.district });
        await page.selectOption('#LabOrgId', { label: (await page.locator('#LabOrgId option').allInnerTexts()).find(o => o.includes(country.Orgs.lab.Name)) });
        await page.fill('#CounterLocation', '[DEMO] Shelf B2');
        if (await page.locator('#containers tbody tr').count() < 2) await page.click('#add-container');
        const rows = page.locator('#containers tbody tr');
        const plan = [
            { net: '22', gross: '23.4', sealed: true, origin: `${COUNTRY}-W-${tag}1`, jar: `${COUNTRY}-WJ-${tag}1`, counter: `${COUNTRY}-WK-${tag}1`, reseal: `${COUNTRY}-WP-${tag}1` },
            { net: '19.5', gross: '20.8', sealed: false, plant: `${COUNTRY}-WP-${tag}2`, jar: `${COUNTRY}-WJ-${tag}2`, counter: `${COUNTRY}-WK-${tag}2`, reseal: `${COUNTRY}-WP-${tag}3` },
        ];
        for (let i = 0; i < plan.length; i++) {
            const r = rows.nth(i), p = plan[i];
            const f = n => r.locator(`[data-field="${n}"]`);
            await f('Kind').selectOption({ index: 0 });
            await f('NetKg').fill(p.net);
            await f('GrossKg').fill(p.gross);
            if (p.sealed) { await f('ArrivedSealed').check(); await f('OriginSeal').fill(p.origin); await f('OriginSealIntact').check(); }
            else { await f('ArrivedSealed').uncheck(); await f('PlantSeal').fill(p.plant); }
            await f('JarSeal').fill(p.jar);
            await f('CounterSeal').fill(p.counter);
            await f('CapIntact').check();
            await f('Reseal').fill(p.reseal);
        }
        await submit(page, page.locator('#intake-send'));
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + (await page.locator('#intake-errors').innerText().catch(() => '')));
        await page.fill('#Code', codeIn(await inbox(page, /\b\d{6}\b/)));
        await submit(page, page.locator('#intake-confirm'));
        intake = page.url().split('/').pop();
        const f = await flash(page);
        return `intake ${intake}: ${f}`;
    });

    // 4. the beekeeper: portal, own password, asking price
    await step('Beekeeper sets the asking price', U('reception'), async () => {
        let page;
        for (const candidate of beePassword) {
            const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, locale: LANG });
            page = await context.newPage();
            page.on('dialog', d => d.accept());
            await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/Account/Login')}`);
            await page.fill('#UserName', business);
            await page.fill('#Password', candidate);
            await Promise.all([page.waitForLoadState('load'), page.click('.au-submit')]);
            if (!/\/Account\/Login/i.test(new URL(page.url()).pathname)) break;
            await context.close(); page = null;
        }
        if (!page) throw new Error('the temporary password did not work');
        if (/FirstPassword/i.test(page.url())) {
            const pw = 'Walk-' + Math.random().toString(36).slice(2, 10) + '-Demo';
            const fields = page.locator('input[type=password]');
            await fields.nth(0).fill(pw); await fields.nth(1).fill(pw);
            await submit(page, page.locator('form button[type=submit]').first());
        }
        await notices(page);
        await page.goto(`${BASE}/Me/Deliveries`); await settle(page);
        const form = page.locator('form:has(input[name=pricePerKg])').first();
        await form.locator('input[name=pricePerKg]').fill(PLAN.price);
        await submit(page, form.locator('button[type=submit]'));
        const f = await flash(page);
        lot = await form.locator('input[name=lotId]').inputValue().catch(() => null) ?? lot;
        await page.screenshot({ path: path.join(OUT, '04b-portal.png') });
        if (!/saved/i.test(f)) throw new Error(f);
        return f;
    });
    lot = lot || (await (async () => { const p = actors[U('reception')]; await p.goto(`${BASE}/Intake/Details/${intake}`); return (await p.content()).match(/rawHoneyProductionId=(\d+)/)?.[1]; })());

    // 5. CCP 0 entered and approved
    await step('CCP 0 entered', U('reception'), page => saveCcp(page, 'raw', lot, 'CCP 0'));
    await step('CCP 0 approved', U('quality'), page => approveCcp(page, 'raw', lot, 'CCP 0'));

    // 6. lab reception, composite, results, approval
    await step('Lab receives the jars', U('analyst'), async page => {
        for (let i = 0; i < 4; i++) {
            await page.goto(`${BASE}/Samples/Index`); await settle(page);
            const form = page.locator('form.receive-jar-form').first();
            if (!await form.count()) break;
            for (const name of ['sealMatches', 'labelIntact', 'capIntact']) await form.locator(`input[name=${name}][value=true]`).check();
            await submit(page, form.locator('button[type=submit]').first());
        }
        await page.goto(`${BASE}/Samples/Index`); await settle(page);
        await submit(page, page.locator('#composite-ready button[type=submit], form[action*="Composite"] button[type=submit]').first());
        if (!/LabReports\/Edit/.test(page.url())) throw new Error('composite: ' + await flash(page));
        return page.url();
    });
    await step('Lab results entered and submitted', U('analyst'), async page => {
        await page.fill('#reportNumber', `DEMO-${COUNTRY}-WALK-${tag}`);
        await page.fill('#issuedOn', new Date().toISOString().slice(0, 10));
        for (const p of manifest.Parameters.filter(x => x.Ccp === 'CCP 1' && x.Type !== 'File')) {
            const input = page.locator(`[name="values[${p.Id}]"]`);
            const v = p.Type === 'Number' ? values.numbers[`CCP 1|${p.Name.trim()}`] : values.texts[`CCP 1|${p.Name.trim()}`] ?? 'Yes';
            if ((await input.evaluate(e => e.tagName)) === 'SELECT') await input.selectOption(v); else await input.fill(v);
        }
        await page.setInputFiles('#reportFile', path.join(PACK, FOLDERS[COUNTRY], 'demo-lab-report.pdf'));
        await submit(page, page.locator('#required-save, #report-form button[type=submit]').first());
        await submit(page, page.locator('#submit-report'));
        const f = await flash(page);
        if (!/submitted/i.test(f)) throw new Error(f);
        return f;
    });
    const reportUrl = actors[U('analyst')].url();
    await step('Lab report approved', U('labmanager'), async page => {
        await page.goto(reportUrl); await settle(page);
        await submit(page, page.locator('#approve-report'));
        const f = await flash(page);
        if (!/approved/i.test(f)) throw new Error(f);
        return f;
    });

    // 7. CCP 2-3
    for (const code of ['CCP 2', 'CCP 3']) {
        await step(`${code} entered`, U('staff'), page => saveCcp(page, 'raw', lot, code));
        await step(`${code} approved`, U('quality'), page => approveCcp(page, 'raw', lot, code));
    }

    // 8. the batch (platform administrator)
    await step('Batch created by the administrator', U('admin'), async page => {
        const plant = country.Orgs.plant.Name;
        await page.goto(`${BASE}/Wholesalers/Index`); await settle(page);
        const ws = await page.evaluate(name => {
            const row = [...document.querySelectorAll('tr')].find(r => r.innerText.includes(name));
            const a = row && [...row.querySelectorAll('a')].find(x => /wholesalerId=|Details\/|Edit\//.test(x.href));
            return a ? (a.href.match(/wholesalerId=(\d+)/) || a.href.match(/\/(\d+)(\?|$)/))[1] : null;
        }, plant);
        await page.goto(`${BASE}/FinalHoneyProductions/Create?wholesalerId=${ws}`); await settle(page);
        await page.fill('#FinalHoneyProductionTitle', `[DEMO] Walkthrough ${tag}`);
        await page.fill('#FinalHoneyProductionDate', new Date().toISOString().slice(0, 10));
        await submit(page, page.locator('form[action*="/FinalHoneyProductions/Create"] button[type=submit]').first());
        batch = page.url().match(/ManageBatch\/(\d+)/)?.[1];
        if (!batch) throw new Error('batch: ' + await flash(page));
        await page.locator(`input[name="usedAmounts[${lot}]"]`).fill('41.5');
        await page.evaluate(() => document.getElementById('manageBatchForm').submit());
        await settle(page);
        await page.goto(`${BASE}/FinalHoneyProductions/ManageCCP/${batch}`); await settle(page);
        await page.evaluate(() => document.querySelectorAll('input[name=SelectedCCPIds]').forEach(i => { i.checked = true; i.dispatchEvent(new Event('change', { bubbles: true })); }));
        await submit(page, page.locator('form:has(input[name=SelectedCCPIds]) button[type=submit]').first());
        await page.goto(`${BASE}/FinalHoneyProductions/Edit/${batch}`); await settle(page);
        await page.selectOption('#OwnerOrgId', { label: plant });
        await submit(page, page.locator('form:has(#OwnerOrgId) button[type=submit]').first());
        return `batch ${batch}`;
    });

    // 9. batch CCP 4-10
    for (const code of ['CCP 4', 'CCP 5', 'CCP 6', 'CCP 7', 'CCP 8', 'CCP 9', 'CCP 10']) {
        await step(`${code} entered`, U('staff'), page => saveCcp(page, 'batch', batch, code));
        await step(`${code} approved`, U('quality'), page => approveCcp(page, 'batch', batch, code));
    }

    // 10. finalize
    await step('Finalizing requested', U('staff'), async page => {
        await page.goto(`${BASE}/OrgWorkspace/Batch/${batch}`); await settle(page);
        await submit(page, page.locator('#request-finalize button[type=submit], #request-finalize').first());
        return await flash(page);
    });
    await step('Batch finalized (sealed)', U('quality'), async page => {
        await page.goto(`${BASE}/OrgWorkspace/Batch/${batch}`); await settle(page);
        await submit(page, page.locator('#confirm-finalize button[type=submit], #confirm-finalize').first());
        const f = await flash(page);
        if (!/finalized/i.test(f)) throw new Error(f);
        qr = (await page.content()).match(/honeystory\/([0-9a-f-]{36})/i)?.[1];
        return `${f} QR ${qr ? 'found' : 'missing'}`;
    });

    // 11. ship, receive, store
    await step('Shipment to the shop', U('staff'), async page => {
        await page.goto(`${BASE}/Shipments/Create?batchId=${batch}`); await settle(page);
        await page.selectOption('#toOrgId', { label: country.Orgs.shop.Name });
        await page.fill('#quantity', '12');
        await submit(page, page.locator('#ship-form button[type=submit]'));
        return await flash(page);
    });
    await step('Shop confirms receipt', U('shop'), async page => {
        await page.goto(`${BASE}/Shipments/Index`); await settle(page);
        await submit(page, page.locator('#incoming form.decide-form button[name=confirm][value=true]').first());
        return await flash(page);
    });
    await step('Shop records storage (CCP 10)', U('shop'), async page => {
        await page.goto(`${BASE}/Shipments/Storage?batchId=${batch}`); await settle(page);
        for (const p of manifest.Parameters.filter(x => x.Ccp === 'CCP 10'))
            await page.locator(`[name="values[${p.Id}]"]`).fill(values.numbers[`CCP 10|${p.Name.trim()}`]);
        await submit(page, page.locator('#storage-form button[type=submit]'));
        return await flash(page);
    });
    await step('Shop approves storage', U('shopquality'), async page => {
        await page.goto(`${BASE}/Shipments/Storage?batchId=${batch}`); await settle(page);
        await submit(page, page.locator('#storage-approve-form button[type=submit]').first());
        return await flash(page);
    });

    // 12. Compliance Officer: the plant's sampling quality, then the directorate's signed photo
    async function switchTo(page, orgName) {
        const form = page.locator('form.role-switch-form').filter({ hasText: orgName }).first();
        if (await form.locator('button[disabled]').count()) return;
        await Promise.all([page.waitForNavigation(),
            page.evaluate(name => [...document.querySelectorAll('form.role-switch-form')].find(f => f.innerText.includes(name)).submit(), orgName)]);
        await settle(page);
    }
    await step('Compliance Officer reviews sampling quality', U('compliance'), async page => {
        await switchTo(page, country.Orgs.plant.Name);
        await page.goto(`${BASE}/Compliance/Index`); await settle(page);
        const review = page.locator('form.review-form').first();
        if (await review.count()) {
            await page.evaluate(() => document.querySelectorAll('details:has(form.review-form)').forEach(d => d.open = true));
            await review.locator('input[name=outcome][value=InOrder]').check();
            await review.locator('textarea[name=finding]').fill('[DEMO] Checked the rejected jar and its replacement.');
            await submit(page, review.locator('button[type=submit]'));
        }
        return await flash(page);
    });
    await step('Compliance Officer accepts a signed photo', U('compliance'), async page => {
        await switchTo(page, country.Orgs.authority.Name);
        await page.goto(`${BASE}/Registration/Details/${country.Beekeepers.b3.RegistrationId}`); await settle(page);
        if (!await page.locator('#evidence-review').count()) return 'already reviewed (seeded item used before)';
        await submit(page, page.locator('#evidence-review button[name=accept][value=true]').first());
        return await flash(page);
    });

    // 13. the consumer
    await step('Consumer opens the Honey Story', U('staff'), async page => {
        const ctx = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, isMobile: true, locale: LANG });
        const p = await ctx.newPage();
        await p.goto(`${BASE}/honeystory/${qr}`); await settle(p);
        const text = await p.evaluate(() => document.body.innerText);
        await p.screenshot({ path: path.join(OUT, '99-honey-story.png'), fullPage: true });
        await ctx.close();
        if (!text.includes('[DEMO] Walk')) throw new Error('the Honey Story does not name the beekeeper');
        return 'name shown';
    });
} finally {
    fs.writeFileSync(path.join(OUT, 'results.json'), JSON.stringify({ country: COUNTRY, lang: LANG, results }, null, 2));
    log(`screenshots and results.json in ${OUT}`);
    await browser.close();
}
