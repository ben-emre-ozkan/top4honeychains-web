// D-86, updated for D-88 and the Şile cast of D-90: walks the partner test scenario "Follow one honey" on the LOCAL app as the people of the live-like cast
// (tools/live-like/cast.json), never the platform administrator: a new beekeeper is registered (code read on the beekeeper's phone
// page), signs in to the portal and sets the asking price, delivers two containers (D-97 cards: seal labels from the association's pool,
// one association container), CCP 0, the laboratory, "ready for packing" (D-98 association path, CCP 2–6 do not apply), New batch in the
// association, both containers emptied into it (closing seals scanned), the batch's control sheet (CCP 7–10), finalizing by two people, the shipment to the shop
// with its storage record, the public Honey Story, and a consumer who creates an account from the story, saves the honey and sends
// feedback that the association answers. It checks that the steps work and saves one screenshot per step outside the repository.
//   node docs/partner-test/walkthrough.mjs        (run-demo.ps1 running on http://localhost:5255; WALK_LANG=en|tr|es|pl|lv)
// - Runs only against localhost (the local app on the local database, never the live site).
// - The practice password and the phone page key are read at run time from Downloads\TOP4-local-logins.txt and never printed.
// - Each run registers its own beekeeper (registry number TR3445901300NN, phone +90 555 000 11 NN) and consumer (zz_walk_NN), with
//   NN unused so far; the cast's cleanup script removes them together with the cast.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const { chromium } = await import(new URL('../manual/node_modules/playwright-core/index.mjs', import.meta.url));
const BASE = (process.env.WALK_BASE || 'http://localhost:5255').replace(/\/$/, '');
if (!/^http:\/\/localhost(:\d+)?$/.test(BASE)) throw new Error('walkthrough.mjs runs against the local app only');
const LANG = process.env.WALK_LANG || 'en';
const OUT = path.join(os.tmpdir(), 'top4-walkthrough');
fs.rmSync(OUT, { recursive: true, force: true });
fs.mkdirSync(OUT, { recursive: true });

const cast = JSON.parse(fs.readFileSync(path.join(repo, 'tools', 'live-like', 'cast.json'), 'utf8'));
const values = JSON.parse(fs.readFileSync(path.join(here, 'values.json'), 'utf8'));
const logins = fs.readFileSync(path.join(os.homedir(), 'Downloads', 'TOP4-local-logins.txt'), 'utf8');
const PASSWORD = logins.match(/^Password:\s+(\S+)/m)[1];   // kept in memory only
const KEY = logins.match(/\/phone\/([A-Za-z0-9_-]{16,64})\//)[1];
const user = (role, org) => cast.people.find(p => p.role === role && p.org === org).user;
const orgName = key => cast.orgs.find(o => o.key === key).name;

// reference data and counts only (never rows with personal data), on the local database
function sql(query) {
    return execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', 'TOP4HoneyChainsDb', '-E', '-b', '-W', '-h', '-1', '-s', '|', '-f', '65001', '-Q', 'SET NOCOUNT ON; ' + query],
        { encoding: 'utf8' }).split(/\r?\n/).map(l => l.trim()).filter(Boolean);
}
const params = Object.fromEntries(sql('SELECT p.ParameterId, c.Code, LTRIM(RTRIM(p.ParameterName)), p.DataType FROM dbo.CriticalControlPointParameters p JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId')
    .filter(l => /^\d+\|/.test(l)).map(l => { const [id, code, name, type] = l.split('|'); return [id, { code, name, type }]; }));
const idOf = query => sql(query).find(l => /^\d+$/.test(l));
const honeyTypeId = idOf(`SELECT HoneyTypeId FROM dbo.HoneyTypes WHERE HoneyTypeTitle = N'${cast.history.honeyType.replace(/'/g, "''")}'`);
const periodId = idOf(`SELECT RawHoneyProductionPeriodId FROM dbo.RawHoneyProductionPeriods WHERE RawHoneyProductionPeriodTitle = N'${cast.history.period.title.replace(/'/g, "''")}'`);
const orgId = key => idOf(`SELECT OrgId FROM dbo.Organizations WHERE IsPlatform = 0 AND DisplayName = N'${orgName(key).replace(/'/g, "''")}'`);
if (!honeyTypeId || !periodId || !orgId('association')) throw new Error('the live-like cast is not in the local database: run tools/live-like first');

// this run's beekeeper and consumer: the first NN whose registry number, phone and user name are all unused
const tag = (() => {
    const start = Date.now() % 90;
    for (let i = 0; i < 90; i++) {
        const nn = String((start + i) % 90 + 10);
        const used = sql(`SELECT (SELECT COUNT(*) FROM dbo.BeekeeperRegistry WHERE RegistryNumber = N'TR34459013${nn.padStart(4, '0')}') + (SELECT COUNT(*) FROM dbo.Beekeepers WHERE BeekeeperPhoneNumber IN (N'+9055500011${nn}', N'+90 555 000 11 ${nn}')) + (SELECT COUNT(*) FROM dbo.AspNetUsers WHERE UserName = N'zz_walk_${nn}')`)[0];
        if (used === '0') return nn;
    }
    throw new Error('no unused walkthrough number left');
})();
const bee = { registry: `TR34459013${tag.padStart(4, '0')}`, first: 'Arif', last: 'Deneme', birth: '1979-04-12', phone: `+90 555 000 11 ${tag}`, province: 'İstanbul', district: 'Şile', locality: 'Hacıllı', hives: '30', lat: '41.0950', lng: '29.7380' };
const beeDigits = bee.phone.replace(/\D/g, '');
const CONSUMER = `zz_walk_${tag}`;
const BATCH_TITLE = `Şile Kestane Balı 2026/W${tag}`;
const REPORT = `${cast.history.lab.reportPrefix}W${tag}`;

function value(id) {
    const p = params[id];
    const key = `${p.code}|${p.name}`;
    if (cast.history.values?.[key]) return cast.history.values[key];
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
const log = (...a) => console.log(...a);
const results = [];
let stepNo = 0;

async function settle(page) { await page.waitForLoadState('networkidle', { timeout: 8000 }).catch(() => {}); }
async function flash(page) {
    return page.evaluate(() => [...document.querySelectorAll('#flash-success, #flash-error, .alert-danger')]
        .map(e => e.innerText.replace(/\s*×\s*$/, '').trim()).filter(Boolean).join(' / ').slice(0, 300));
}
// a step succeeded when the page shows a success message and no error
async function success(page) {
    const ok = await page.locator('#flash-success').count() > 0 && await page.locator('#flash-error, #db-update-error').count() === 0;
    const text = await flash(page);
    if (!ok) throw new Error(text || 'no success message');
    return text;
}
async function submit(page, locator) { await Promise.all([page.waitForLoadState('load'), locator.first().click()]); await settle(page); }
async function newPage(mobile = false) {
    const context = await browser.newContext(mobile ? { viewport: { width: 390, height: 844 }, isMobile: true, locale: LANG }
        : { viewport: { width: 1440, height: 900 }, locale: LANG, reducedMotion: 'reduce' });
    const page = await context.newPage();
    page.setDefaultTimeout(20000);
    page.on('dialog', d => d.accept());
    await page.goto(`${BASE}/Home/SetLanguage?culture=${LANG}&returnUrl=${encodeURIComponent('/')}`);
    return page;
}
async function signIn(page, name, password) {
    await page.goto(`${BASE}/Account/Login`);
    await page.fill('#UserName', name);
    await page.fill('#Password', password);
    await Promise.all([page.waitForLoadState('load'), page.click('.au-submit')]);
    await settle(page);
    return !/\/Account\/Login/i.test(new URL(page.url()).pathname);
}

const actors = {};
async function actor(name) {
    if (actors[name]) return actors[name];
    const page = await newPage();
    if (!await signIn(page, name, PASSWORD)) throw new Error(`${name} could not sign in`);
    if (/\/Notices\/Accept/i.test(page.url())) {   // the notice gate, when it is switched on
        await page.locator('#notice-accept input[name=read]').check();
        await submit(page, page.locator('#notice-accept button[type=submit]').last());
    }
    actors[name] = page;
    return page;
}

// the phone page of the beekeeper's number, open like a messaging app: the newest message after the last one read
const phone = { page: null, seen: 0 };
async function nextMessage(re = /\b\d{6}\b/) {
    if (!phone.page) {
        phone.page = await newPage(true);
        await phone.page.goto(`${BASE}/phone/${KEY}/${beeDigits}`);
        await phone.page.waitForTimeout(1000);
        phone.seen = Math.max(0, ...await phone.page.locator('.msg').evaluateAll(els => els.map(e => +e.dataset.id)));
        return null;
    }
    const deadline = Date.now() + 15000;
    while (Date.now() < deadline) {
        const fresh = await phone.page.locator('.msg').evaluateAll(els => els.map(e => ({ id: +e.dataset.id, text: e.innerText })));
        const hit = fresh.filter(m => m.id > phone.seen && re.test(m.text)).sort((a, b) => b.id - a.id)[0];
        if (hit) { phone.seen = hit.id; return hit.text; }
        await phone.page.waitForTimeout(500);
    }
    throw new Error(`no new message matching ${re} on the beekeeper's phone page`);
}
const codeIn = t => t.match(/\b\d{6}\b/)[0];

// what appears in someone's My work next (matched by the item's link)
async function workOf(who, re) {
    const page = await actor(who);
    await page.goto(`${BASE}/MyWork/Index`); await settle(page);
    const items = await page.locator('.work-item').evaluateAll(els => els.map(e => ({ text: e.innerText.replace(/\s+/g, ' ').trim(), href: e.getAttribute('href') || '' })));
    const hit = items.find(i => re.test(i.href) || re.test(i.text));
    if (!hit) throw new Error(`nothing for ${re} in the My work of ${who}`);
    return `next: ${who} → ${hit.text}`;
}

// who: a cast user name (signed in in its own browser), or { label, page } for the beekeeper and the consumer
async function step(title, who, fn) {
    stepNo++;
    const page = typeof who === 'string' ? await actor(who) : who?.page ?? null;
    const label = typeof who === 'string' ? who : who?.label ?? '-';
    const started = Date.now();
    const shot = `${String(stepNo).padStart(2, '0')}-${title.replace(/[^\w]+/g, '-').replace(/-+$/, '').toLowerCase()}.png`;
    try {
        const note = await fn(page);
        if (page) await page.screenshot({ path: path.join(OUT, shot) });
        const seconds = +((Date.now() - started) / 1000).toFixed(1);
        results.push({ n: stepNo, title, who: label, ok: true, seconds, note: note || '' });
        log(`${String(stepNo).padStart(2)} ok   ${seconds.toFixed(1).padStart(5)} s  ${title} (${label}) ${note || ''}`);
    } catch (e) {
        if (page) await page.screenshot({ path: path.join(OUT, `${String(stepNo).padStart(2, '0')}-FAILED.png`), fullPage: true }).catch(() => {});
        results.push({ n: stepNo, title, who: label, ok: false, note: e.message.split('\n')[0] });
        log(`${String(stepNo).padStart(2)} FAIL ${title}: ${e.message.split('\n')[0]}`);
        throw e;
    }
}

// fills the CCP inputs of a page (one CCP or a control sheet) with the suggested values
async function fillCcpBoxes(page) {
    const ids = await page.locator('.ccp-param[data-parameter-id]').evaluateAll(els => els.map(e => e.dataset.parameterId));
    for (const id of ids) {
        const v = value(id);
        if (!v) continue;
        const box = page.locator(`.ccp-param[data-parameter-id="${id}"]`);
        if (await box.locator('select').count()) await box.locator('select').first().selectOption(v);
        else await box.locator('input[type=text]').first().fill(v);
    }
    return ids.length;
}
// the approve button; when values are outside their limits the page asks to confirm in a dialog
async function approveWithDialog(page, button, dialog) {
    const navigation = page.waitForNavigation({ timeout: 4000 }).then(() => true).catch(() => false);
    await page.locator(button).click();
    if (!await navigation) await Promise.all([page.waitForNavigation(), page.locator(`${dialog} .oolc-confirm`).click()]);
    await settle(page);
}

const reg = user('RegistrationOfficer', 'directorate'), sao = user('SampleAcceptanceOfficer', 'association'), staff = user('Staff', 'association'), qa = user('QualityApprover', 'association');
const analyst = user('Analyst', 'lab'), labManager = user('LabManager', 'lab'), shop = user('Staff', 'shop'), shopQa = user('QualityApprover', 'shop');
let business, lot, batch, composite, qr, portal, story, closings = [];

// D-97: free seal labels of the association's pool after the ones reserved for the owner's guide (TS-000001..030) and a free association
// container after TK-0004 (TK-0001..0003: the guide; the last one: tools/live-like/walk.mjs). Seal codes are not personal data.
function freeCodes() {
    const org = orgId('association');
    const labels = sql(`SELECT Code FROM dbo.SealCodes WHERE OrgId = ${org} AND Kind = 'Label' AND Status = 'Issued' AND Code > 'TS-000030' ORDER BY Code`).filter(c => /^TS-\d{6}$/.test(c));
    const boxes = sql(`SELECT b.Code FROM dbo.OrgContainers b WHERE b.OrgId = ${org} AND b.IsActive = 1 AND b.Code > 'TK-0003' ORDER BY b.Code`).filter(c => /^TK-\d{4}$/.test(c));
    return { labels, box: boxes.length > 1 ? boxes[boxes.length - 2] : boxes[0] };
}

try {
    await nextMessage();   // the beekeeper's phone page is open before anything is sent

    // 1. registration by the directorate's registration officer, confirmed with the code from the beekeeper's phone
    await step('Register a beekeeper', reg, async page => {
        await page.goto(`${BASE}/Registration/Index`); await settle(page);
        await page.fill('#number', bee.registry);
        await submit(page, page.locator('#registration-find button[type=submit]'));
        if (!/Registration\/New/.test(page.url())) throw new Error('find: ' + await flash(page));
        await page.fill('#FirstName', bee.first);
        await page.fill('#LastName', bee.last);
        await page.fill('#BirthDate', bee.birth);
        await page.fill('#Phone', bee.phone);
        await page.selectOption('#Language', 'tr').catch(() => {});
        await page.selectOption('#AssociationOrgId', orgId('association'));
        await page.selectOption('#ap-province', { label: bee.province });
        await page.waitForFunction(() => document.getElementById('ap-district').options.length > 1);
        await page.selectOption('#ap-district', { label: bee.district });
        await page.fill('#ap-locality', bee.locality);
        await page.fill('#ApiaryHives', bee.hives);
        await page.fill('#ApiaryLocationLatitude', bee.lat);
        await page.fill('#ApiaryLocationLongitude', bee.lng);
        for (const box of ['#PublicName', '#PortalNotifications', '#Attest', '#IssueSignIn']) if (await page.locator(box).count()) await page.check(box);
        await submit(page, page.locator('#registration-send'));
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + (await page.locator('#registration-errors').innerText().catch(() => '')));
        await page.fill('#Code', codeIn(await nextMessage()));
        await submit(page, page.locator('#registration-confirm'));
        const f = await success(page);
        business = (await page.evaluate(() => document.body.innerText)).match(/TR-BEE-\d{4}-\d{6}/)?.[0];
        return `${bee.registry}, ${bee.phone}; ${f}`;
    });

    // 2. the beekeeper reads the sign-in details on the phone and chooses a password
    await step('The beekeeper signs in to the portal', { label: 'the beekeeper' }, async () => {
        const sms = await nextMessage(/TR-BEE-\d{4}-\d{6}/);
        business = sms.match(/TR-BEE-\d{4}-\d{6}/)[0];
        portal = await newPage(true);
        let ok = false;
        for (const candidate of sms.match(/(?<![A-Za-z0-9-])[A-HJ-NP-Za-km-z2-9]{10}(?![A-Za-z0-9-])/g) || [])
            if (await signIn(portal, business, candidate)) { ok = true; break; }
        if (!ok) throw new Error('the temporary password did not work');
        if (/FirstPassword/i.test(portal.url())) {
            const fields = portal.locator('input[type=password]');
            await fields.nth(0).fill(PASSWORD); await fields.nth(1).fill(PASSWORD);
            await submit(portal, portal.locator('form button[type=submit]'));
        }
        await portal.screenshot({ path: path.join(OUT, `${String(stepNo).padStart(2, '0')}-portal.png`) });
        return `portal account ${business}`;
    });

    // 3. the delivery in sealed containers, confirmed by the beekeeper's code
    await step('Record a delivery with sealed containers', sao, async page => {
        await page.goto(`${BASE}/Intake/Create?number=${encodeURIComponent(business)}`); await settle(page);
        if (!(await page.locator('#ApiaryId').inputValue())) await page.locator('#ApiaryId').selectOption({ index: 1 });
        await page.selectOption('#HoneyTypeId', honeyTypeId);
        await page.selectOption('#PeriodId', periodId);
        const prov = page.locator('select[data-province-select]').first();
        await prov.selectOption({ label: bee.province });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ label: bee.district });
        if (!(await page.locator('#DeliveryDate').inputValue())) await page.fill('#DeliveryDate', new Date().toISOString().slice(0, 10));
        await page.selectOption('#OriginCountry', 'TR').catch(() => {});
        await page.selectOption('#LabOrgId', orgId('lab'));
        if (!(await page.locator('#RetainUntil').inputValue())) { const d = new Date(); d.setFullYear(d.getFullYear() + 2); await page.fill('#RetainUntil', d.toISOString().slice(0, 10)); }
        await page.fill('#CounterLocation', cast.history.counterLocation);
        // D-97: one card per container, every code from the association's seal pool (checked live); jar caps with a safety button
        const free = freeCodes();
        if (free.labels.length < 6 || !free.box) throw new Error('the seal pool has too few free codes: issue seal labels on the Seals page');
        if (await page.locator('#ci-cards [data-card]').count() < 2) await page.click('#add-container');
        const cards = page.locator('#ci-cards [data-card]');
        const plan = [
            { closure: 'SealLabel', net: '24.2', gross: '25.6', jar: free.labels[0], counter: free.labels[1], closing: free.labels[2] },
            { closure: 'OrgContainer', box: free.box, net: '20.9', gross: '22.3', jar: free.labels[3], counter: free.labels[4], closing: free.labels[5] }
        ];
        closings = plan.map(p => p.closing);
        for (let i = 0; i < plan.length; i++) {
            const card = cards.nth(i), p = plan[i];
            const f = n => card.locator(`[data-field="${n}"]`);
            await card.locator(`[data-field="Closure"][value="${p.closure}"]`).check();
            if (p.box) {
                await f('OrgContainerCode').fill(p.box);
                await card.locator('.ci-box.is-ok').waitFor({ timeout: 8000 });
            } else await f('Kind').selectOption({ index: 0 });
            await f('NetKg').fill(p.net);
            await f('GrossKg').fill(p.gross);
            await card.locator('[data-field="ArrivedSealed"][value="false"]').check();
            for (const [field, code] of [['JarSeal', p.jar], ['CounterSeal', p.counter], ['ClosingSeal', p.closing]]) {
                await f(field).fill(code);
                await card.locator(`[data-field="${field}"].is-ok`).waitFor({ timeout: 8000 });
            }
            await f('CapIntact').check();
        }
        await submit(page, page.locator('#intake-send'));
        if (!await page.locator('#Code').count()) throw new Error('code not sent: ' + (await page.locator('#intake-errors').innerText().catch(() => '')));
        await page.fill('#Code', codeIn(await nextMessage()));
        await submit(page, page.locator('#intake-confirm'));
        const f = await success(page);
        lot = f.match(/#(\d+)/)?.[1];
        if (!lot) throw new Error('no lot number: ' + f);
        return `${f} ${await workOf(sao, new RegExp(`rawHoneyProductionId=${lot}&ccpId=1`))}`;
    });

    // 4. the beekeeper enters the asking price for this delivery in the portal
    await step('The beekeeper sets the asking price', { label: 'the beekeeper' }, async () => {
        await portal.goto(`${BASE}/Me/Deliveries`); await settle(portal);
        const form = portal.locator(`form.price-form:has(input[name=lotId][value="${lot}"])`);
        await form.locator('input[name=pricePerKg]').fill('650');
        await submit(portal, form.locator('button[type=submit]'));
        await portal.screenshot({ path: path.join(OUT, `${String(stepNo).padStart(2, '0')}-asking-price.png`) });
        return await success(portal);
    });

    // 5. CCP 0
    await step('CCP 0 entered', sao, async page => {
        await page.goto(`${BASE}/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`); await settle(page);
        const n = await fillCcpBoxes(page);
        await submit(page, page.locator('form.field-form button[type=submit]'));
        return `${n} values; ${await success(page)} ${await workOf(qa, new RegExp(`rawHoneyProductionId=${lot}&ccpId=1`))}`;
    });
    await step('CCP 0 approved', qa, async page => {
        await page.goto(`${BASE}/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=1`); await settle(page);
        await approveWithDialog(page, '#approve-button', '#oolc-approve-form');
        return await success(page);
    });

    // 6. the laboratory: reception of the jars, composite, results, approval by the lab manager (completes CCP 1)
    await step('Lab receives the jars and makes the composite', analyst, async page => {
        for (let i = 0; i < 4; i++) {
            await page.goto(`${BASE}/Samples/Index`); await settle(page);
            const form = page.locator('form.receive-jar-form').first();
            if (!await form.count()) break;
            for (const name of ['sealMatches', 'labelIntact', 'capIntact']) await form.locator(`input[name=${name}][value=true]`).check();
            await submit(page, form.locator('button[type=submit]'));
        }
        await page.goto(`${BASE}/Samples/Index`); await settle(page);
        await submit(page, page.locator('#composite-ready button[type=submit], form[action*="Composite"] button[type=submit]'));
        if (!/LabReports\/Edit/.test(page.url())) throw new Error('composite: ' + await flash(page));
        composite = page.url().split('/').pop();
        return 'composite sample, report form open';
    });
    await step('Lab results entered and submitted', analyst, async page => {
        await page.fill('#reportNumber', REPORT);
        await page.fill('#issuedOn', new Date().toISOString().slice(0, 10));
        const ids = await page.locator('[name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) {
            const input = page.locator(`[name="values[${id}]"]`);
            if ((await input.evaluate(e => e.tagName)) === 'SELECT') await input.selectOption(value(id)); else await input.fill(value(id));
        }
        const pdf = path.join(os.tmpdir(), `walkthrough-${REPORT}.pdf`);
        fs.writeFileSync(pdf, '%PDF-1.4\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n');
        await page.setInputFiles('#reportFile', pdf);
        await submit(page, page.locator('#required-save, #report-form button[type=submit]'));
        fs.rmSync(pdf, { force: true });
        await submit(page, page.locator('#submit-report'));
        return `${ids.length} results; ${await success(page)} ${await workOf(labManager, new RegExp(`LabReports/Edit/${composite}`))}`;
    });
    await step('Lab report approved', labManager, async page => {
        await page.goto(`${BASE}/LabReports/Edit/${composite}`); await settle(page);
        await submit(page, page.locator('#approve-report'));
        return `${await success(page)} ${await workOf(qa, new RegExp(`rawHoneyProductionId=${lot}&ccpId=`))}`;
    });

    // 7. D-98, association path: the QA officer marks the honey ready for packing (CCP 2–6 do not apply)
    await step('Honey ready for packing', qa, async page => {
        await page.goto(`${BASE}/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId=${lot}&ccpId=2`); await settle(page);
        await submit(page, page.locator('#packing-ready-button'));
        return `${await success(page)} ${await workOf(staff, /OrgWorkspace\/NewBatch/)}`;
    });

    // 8. the association creates the batch from its approved lot
    await step('New batch in the association', staff, async page => {
        await page.goto(`${BASE}/OrgWorkspace/Index`); await settle(page);
        await submit(page, page.locator('#link-new-batch'));
        for (const id of await page.locator('#new-batch-lots tbody tr').evaluateAll(els => els.map(e => e.dataset.lotId)))
            if (id !== lot) await page.locator(`#use-${id}`).uncheck();
        await page.fill('#batch-title', BATCH_TITLE);
        await submit(page, page.locator('#new-batch-submit'));
        batch = page.url().match(/\/OrgWorkspace\/Batch\/(\d+)/)?.[1];
        if (!batch) throw new Error('batch: ' + await flash(page));
        return `${await success(page)} ${await workOf(staff, new RegExp(`kind=batch&id=${batch}`))}`;
    });

    // 8b. D-98: each container is emptied into the batch once its closing seal is scanned and found intact
    await step('Containers emptied into the batch', staff, async page => {
        await page.goto(`${BASE}/OrgWorkspace/Batch/${batch}`); await settle(page);
        for (let i = 0; i < closings.length; i++) {
            const row = page.locator(`#emptying-list tr[data-container="L${lot}-C${i + 1}"]`);
            await row.locator('input.em-seal').fill(closings[i]);
            await row.locator('input[type=checkbox][name=intact]').check();
            await submit(page, row.locator('.em-empty'));
            if (await page.locator(`#emptying-list tr[data-container="L${lot}-C${i + 1}"][data-state="Emptied"]`).count() !== 1)
                throw new Error(`container ${i + 1} not emptied: ${await flash(page)}`);
        }
        return `${closings.length} containers emptied`;
    });

    // 9. the batch's control sheet (CCP 7–10; CCP 4–6 do not apply on the association path)
    await step('Batch control sheet (CCP 7-10) entered', staff, async page => {
        await page.goto(`${BASE}/CCPMonitoring/Sheet?kind=batch&id=${batch}`); await settle(page);
        const n = await fillCcpBoxes(page);
        await submit(page, page.locator('#sheet-save'));
        return `${n} values; ${await success(page)} ${await workOf(qa, new RegExp(`kind=batch&id=${batch}`))}`;
    });
    await step('Batch control sheet approved', qa, async page => {
        await page.goto(`${BASE}/CCPMonitoring/Sheet?kind=batch&id=${batch}`); await settle(page);
        await approveWithDialog(page, '#sheet-approve-button', '#oolc-sheet-approve-form');
        return await success(page);
    });

    // 10. finalizing by two people
    await step('Finalizing requested', staff, async page => {
        await page.goto(`${BASE}/OrgWorkspace/Batch/${batch}`); await settle(page);
        await submit(page, page.locator('#request-finalize'));
        return `${await success(page)} ${await workOf(qa, new RegExp(`OrgWorkspace/Batch/${batch}`))}`;
    });
    await step('Batch finalized (sealed)', qa, async page => {
        await page.goto(`${BASE}/OrgWorkspace/Batch/${batch}`); await settle(page);
        await submit(page, page.locator('#confirm-finalize'));
        const f = await success(page);
        qr = (await page.content()).match(/honeystory\/([0-9A-Fa-f-]{36})/)?.[1];
        if (!qr) throw new Error('no Honey Story link after finalizing');
        return f;
    });

    // 11. ship, receive, store
    await step('Shipment to the shop', staff, async page => {
        await page.goto(`${BASE}/Shipments/Create?batchId=${batch}`); await settle(page);
        await page.selectOption('#toOrgId', orgId('shop'));
        await page.fill('#quantity', '15');
        await submit(page, page.locator('#ship-form button[type=submit]'));
        return `${await success(page)} ${await workOf(shop, new RegExp(BATCH_TITLE.replace(/[/]/g, '.')))}`;
    });
    await step('Shop confirms receipt', shop, async page => {
        await page.goto(`${BASE}/Shipments/Index`); await settle(page);
        await submit(page, page.locator('#incoming form.decide-form button[name=confirm][value=true]'));
        return await success(page);
    });
    await step('Shop records storage (CCP 10)', shop, async page => {
        await page.goto(`${BASE}/Shipments/Storage?batchId=${batch}`); await settle(page);
        const ids = await page.locator('#storage-form [name^="values["]').evaluateAll(els => els.map(e => e.name.match(/\d+/)[0]));
        for (const id of ids) {
            const box = page.locator(`#storage-form [name="values[${id}]"]`);
            if (await box.evaluate(e => e.tagName === 'SELECT')) await box.selectOption(value(id)); else await box.fill(value(id));
        }
        await submit(page, page.locator('#storage-form button[type=submit]'));
        return `${await success(page)} ${await workOf(shopQa, new RegExp(`Storage\\?batchId=${batch}|CCP 10 · ${BATCH_TITLE.replace(/[/]/g, '.')}`))}`;
    });
    await step('Shop approves storage', shopQa, async page => {
        await page.goto(`${BASE}/Shipments/Storage?batchId=${batch}`); await settle(page);
        await submit(page, page.locator('#storage-approve-form button[type=submit]'));
        return await success(page);
    });

    // 12. the consumer: the public Honey Story on a phone, an account from the story, save, feedback, the association's answer
    story = await newPage(true);
    await step('Consumer opens the Honey Story', { label: 'anyone, without signing in', page: story }, async page => {
        await page.goto(`${BASE}/honeystory/${qr}`); await settle(page);
        if (!(await page.evaluate(() => document.body.innerText)).includes(`${bee.first} ${bee.last}`)) throw new Error('the Honey Story does not name the beekeeper');
        return 'beekeeper named';
    });
    await step('Consumer creates an account and saves the honey', { label: CONSUMER, page: story }, async page => {
        await submit(page, page.locator('#consumer-anon a[href*="/Consumer/SignUp"]'));
        await page.fill('#UserName', CONSUMER);
        await page.fill('#Password', PASSWORD);
        await page.fill('#ConfirmPassword', PASSWORD);
        await page.selectOption('#Language', LANG).catch(() => {});
        await page.locator('input[name=IsAdult][type=checkbox]').check();
        await page.locator('input[name=AcceptNotice][type=checkbox]').check();
        await page.waitForTimeout(3500);   // the form refuses a sending faster than a person types
        await submit(page, page.locator('#consumer-signup-form button[type=submit]'));
        await submit(page, page.locator('#consumer-save'));
        await submit(page, page.locator('#save-form button[type=submit]'));
        return `${CONSUMER}: ${await success(page)}`;
    });
    await step('Consumer sends feedback', { label: CONSUMER, page: story }, async page => {
        await page.goto(`${BASE}/Consumer/Feedback?qr=${qr}`); await settle(page);
        const category = page.locator('select[name=category]');
        if (await category.count()) await category.selectOption({ index: 1 }); else await page.locator('input[name=category]').first().check();
        await page.fill('#message', 'Lovely chestnut honey. Does it crystallize, and how should I store it?');
        await submit(page, page.locator('#feedback-form button[type=submit]'));
        return `${await success(page)} ${await workOf(qa, /ConsumerFeedback\/Details/)}`;
    });
    await step('The association answers the feedback', qa, async page => {
        await page.goto(`${BASE}/ConsumerFeedback`); await settle(page);
        const href = await page.locator('a[href*="/ConsumerFeedback/Details/"]').filter({ hasText: BATCH_TITLE }).first().getAttribute('href');
        await page.goto(BASE + href); await settle(page);
        await page.fill('#answer', 'Thank you! Chestnut honey crystallizes slowly. Keep it closed at room temperature, away from sunlight.');
        await submit(page, page.locator('#answer-form button[type=submit]'));
        return await success(page);
    });
    await step('The consumer reads the answer', { label: CONSUMER, page: story }, async page => {
        await page.goto(`${BASE}/Consumer`); await settle(page);
        if (!/crystallizes slowly/.test(await page.locator('#my-feedback').innerText())) throw new Error('the answer is not shown to the consumer');
        return 'answer shown under My feedback';
    });
} finally {
    fs.writeFileSync(path.join(OUT, 'results.json'), JSON.stringify({ lang: LANG, registry: bee.registry, phone: bee.phone, consumer: CONSUMER, lot, batch, results }, null, 2));
    log(`screenshots and results.json in ${OUT}`);
    await browser.close();
}
