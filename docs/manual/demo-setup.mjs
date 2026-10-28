// Creates the [DEMO] records of the D-78 screens in the LOCAL demo application, through the application itself (D-85):
//   node demo-setup.mjs
// - a [DEMO] district directorate (Beekeeper registration) with a registration officer and a Compliance Officer, a Compliance
//   Officer at the [DEMO] processing plant;
// - the re-verification of [DEMO] Zeynep Kovancı by SMS code with her portal sign-in, and a [DEMO] beekeeper registered by
//   signed photo (waiting for the Compliance Officer);
// - a [DEMO] delivery in two containers confirmed by SMS code, one jar rejected at the laboratory and sampled again, the
//   beekeeper's asking price, and a delivery correction waiting for the second administrator.
// Runs against http://localhost:5255 with the test SMS provider (codes are read from the page's demo panel and the SMS outbox).
// New accounts get random passwords, appended to the untracked demo_users.txt at the repository root; nothing is printed.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { BASE, launch, account, credentials, one, like, settle } from './capture.mjs';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const usersFile = path.join(repo, 'demo_users.txt');
const pdf = path.join(os.tmpdir(), 'top4-demo-setup.pdf');
const log = (...a) => console.log('  ', ...a);

const DIRECTORATE = '[DEMO] Merkez İlçe Tarım Müdürlüğü';
const PLANT = '[DEMO] Kovan Bal İşleme';
const NEW_USERS = [
    { user: 'demo_ilce_kayit', org: DIRECTORATE, role: 'RegistrationOfficer', phone: '+905550000101' },
    { user: 'demo_ilce_uyum', org: DIRECTORATE, role: 'ComplianceOfficer', phone: '+905550000102' },
    { user: 'demo_isleme_uyum', org: PLANT, role: 'ComplianceOfficer', phone: '+905550000103' },
];
const ZEYNEP_NUMBER = 'TR-BEE-2026-000064', ZEYNEP_PHONE = '+905550000201';
const HASAN_REGISTRY = 'TR340000000003';

function password() {
    const abc = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789';
    return Array.from(crypto.randomBytes(14), b => abc[b % abc.length]).join('') + '!7';
}
function remember(user, org, role, pw) {
    const text = fs.readFileSync(usersFile, 'utf8');
    fs.appendFileSync(usersFile, `${text.endsWith('\n') ? '' : '\n'}│ ${user} │ ${org} │ ${role} │ ${pw} │\n`, 'utf8');
}
function known(user) { return fs.readFileSync(usersFile, 'utf8').split(/\r?\n/).some(l => l.startsWith('│') && l.split('│')[1]?.trim() === user); }
function writePdf() {
    const stream = 'BT /F1 22 Tf 72 760 Td ([DEMO] signed form for the user manual) Tj ET';
    const objs = ['<< /Type /Catalog /Pages 2 0 R >>', '<< /Type /Pages /Kids [3 0 R] /Count 1 >>',
        '<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>',
        `<< /Length ${stream.length} >>\nstream\n${stream}\nendstream`, '<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>'];
    let out = '%PDF-1.4\n'; const offs = [];
    objs.forEach((o, i) => { offs.push(out.length); out += `${i + 1} 0 obj\n${o}\nendobj\n`; });
    const xref = out.length;
    out += `xref\n0 ${objs.length + 1}\n0000000000 65535 f \n` + offs.map(o => String(o).padStart(10, '0') + ' 00000 n \n').join('');
    out += `trailer\n<< /Size ${objs.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;
    fs.mkdirSync(path.dirname(pdf), { recursive: true });
    fs.writeFileSync(pdf, out, 'latin1');
}

async function open(browser) {
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, locale: 'en', reducedMotion: 'reduce' });
    const page = await context.newPage();
    page.setDefaultTimeout(15000);
    await page.goto(`${BASE}/Home/SetLanguage?culture=en&returnUrl=${encodeURIComponent('/Account/Login')}`);
    return { context, page };
}
// signs in; a first sign-in with an initial password chooses the final one (returned)
async function signIn(page, user, pw) {
    await page.goto(`${BASE}/Account/Login`);
    await page.fill('#UserName', user);
    await page.fill('#Password', pw);
    await Promise.all([page.waitForURL(u => !/\/Account\/Login/i.test(u.pathname)), page.click('.au-submit')]);
    if (/FirstPassword/i.test(page.url())) {
        const final = password();
        const fields = page.locator('input[type=password]');
        await fields.nth(0).fill(final);
        await fields.nth(1).fill(final);
        await Promise.all([page.waitForURL(u => !/FirstPassword/i.test(u.pathname)), page.locator('form button[type=submit]').first().click()]);
        return final;
    }
    return pw;
}
async function go(page, url) { await page.goto(BASE + url); await settle(page); }
async function submit(page, locator) { await Promise.all([page.waitForLoadState('load'), locator.click()]); await settle(page); }
const flash = page => page.evaluate(() => [...document.querySelectorAll('.alert, .app-toast, .toast')].map(e => e.innerText.trim()).filter(Boolean).join(' / ').slice(0, 300));

async function admin(browser) {
    const { context, page } = await open(browser);
    const a = account('admin');
    await signIn(page, a.user, a.password);
    return { context, page };
}

// ---- 1. organizations, members, first sign-ins
async function organizations(browser) {
    const { context, page } = await admin(browser);
    let orgId = one(`SELECT OrgId FROM Organizations WHERE ${like('DisplayName', DIRECTORATE)}`);
    if (!orgId) {
        await go(page, '/Organizations/Create');
        await page.fill('#DisplayName', DIRECTORATE);
        await page.fill('#LegalName', DIRECTORATE + ' (T.C. Tarım ve Orman Bakanlığı)');
        const prov = page.locator('select[data-province-select]');
        await prov.selectOption({ label: 'İstanbul' });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ index: 1 });
        await page.check('#cap_BeekeeperRegistration');
        await submit(page, page.locator('form[action*="Create"] button[type=submit]').first());
        orgId = one(`SELECT OrgId FROM Organizations WHERE ${like('DisplayName', DIRECTORATE)}`);
        log('directorate created', !!orgId);
    }
    const plantId = one(`SELECT OrgId FROM Organizations WHERE ${like('DisplayName', PLANT)}`);
    for (const [id, name] of [[orgId, DIRECTORATE], [plantId, PLANT]]) {
        await go(page, `/Organizations/Details/${id}`);
        if (await page.locator('#data-controller-missing').count()) {
            await page.fill('#dcName', name.replace('[DEMO] ', '[DEMO] ') + ' (veri sorumlusu)');
            await page.fill('#dcContact', 'kvkk@example.invalid');
            await page.fill('#dcAddress', '[DEMO] Örnek Mah. 1. Sok. No:1, İstanbul');
            await submit(page, page.locator('#data-controller-form button[type=submit]'));
            log('data controller', name, await flash(page));
        }
    }
    for (const u of NEW_USERS) {
        if (known(u.user)) continue;
        const id = u.org === DIRECTORATE ? orgId : plantId;
        await go(page, `/Organizations/Details/${id}`);
        const initial = password();
        await page.selectOption('#add-member select[name=orgRole]', u.role);
        await page.fill('#add-member input[name=newUserName]', u.user);
        await page.fill('#add-member input[name=newPhone]', u.phone);
        await page.fill('#add-member input[name=newEmail]', `${u.user}@example.invalid`);
        await page.fill('#add-member input[name=newPassword]', initial);
        await submit(page, page.locator('#add-member button[type=submit]'));
        log('member', u.user, await flash(page));
        const { context: c2, page: p2 } = await open(browser);
        const final = await signIn(p2, u.user, initial);
        await c2.close();
        remember(u.user, u.org, u.role, final);
    }
    if (one(`SELECT Status FROM Organizations WHERE OrgId = ${orgId}`) !== 'Verified') {
        await go(page, `/Organizations/Details/${orgId}`);
        if (!one(`SELECT TOP 1 DocumentId FROM OrgDocuments WHERE OrgId = ${orgId}`)) {
            await page.selectOption('#upload-form select[name=kind]', { index: 0 });
            await page.setInputFiles('#upload-form input[type=file]', pdf);
            await submit(page, page.locator('#upload-form button[type=submit]'));
            log('document', await flash(page));
        }
        await submit(page, page.locator('#verify-form button[name=toStatus][value=Verified]'));
        log('verified', one(`SELECT Status FROM Organizations WHERE OrgId = ${orgId}`));
    }
    await context.close();
}

function user(name) { const a = credentials(true).find(x => x.user === name); if (!a) throw new Error('no account ' + name); return a; }

// ---- 2. registrations
async function demoCode(page) {
    if (!await page.locator('#demo-sms').count()) {
        const problems = await page.evaluate(() => [...document.querySelectorAll('.alert-danger, .text-danger, .field-validation-error, .validation-summary-errors, [id$=-errors]')]
            .map(e => e.innerText.trim()).filter(Boolean).join(' / ').slice(0, 500));
        await page.screenshot({ path: process.env.DEMO_DEBUG_SHOT || path.join(os.tmpdir(), 'top4-demo-setup-debug.png'), fullPage: true }).catch(() => {});
        const main = await page.evaluate(() => (document.querySelector('.page-content') || document.body).innerText.replace(/\s+/g, ' ').slice(0, 700));
        throw new Error(`no demo code panel at ${page.url()}: ${problems} || ${main}`);
    }
    const text = await page.locator('#demo-sms').innerText();
    const m = text.match(/\b(\d{6})\b/);
    if (!m) throw new Error('no code in the demo panel');
    return m[1];
}
async function registrations(browser) {
    const { context, page } = await open(browser);
    const ro = user('demo_ilce_kayit');
    await signIn(page, ro.user, ro.password);
    const zeynep = one(`SELECT BeekeeperId FROM Beekeepers WHERE BeekeeperBusinessNumber = '${ZEYNEP_NUMBER}'`);
    if (!one(`SELECT TOP 1 RegistrationId FROM BeekeeperRegistrations WHERE BeekeeperId = '${zeynep}'`)) {
        await go(page, '/Registration/Index');
        await page.fill('#number', ZEYNEP_NUMBER);
        await submit(page, page.locator('#registration-find button[type=submit]'));
        await page.fill('#Phone', ZEYNEP_PHONE);
        await page.check('#PublicName');
        await page.check('#PortalNotifications');
        await page.check('#Attest');
        if (await page.locator('#IssueSignIn').count()) await page.check('#IssueSignIn');
        await submit(page, page.locator('#registration-send'));
        const errors = await page.locator('#registration-errors').count() ? await page.locator('#registration-errors').innerText() : '';
        if (errors) log('registration send:', errors.slice(0, 300));
        await page.fill('#Code', await demoCode(page));
        await submit(page, page.locator('#registration-confirm'));
        log('zeynep registration', await flash(page));
    }
    if (!one(`SELECT TOP 1 BeekeeperId FROM BeekeeperRegistry WHERE RegistryNumber = '${HASAN_REGISTRY}'`)) {
        await go(page, '/Registration/Index');
        await page.fill('#number', HASAN_REGISTRY);
        await submit(page, page.locator('#registration-find button[type=submit]'));
        await page.fill('#FirstName', '[DEMO] Hasan');
        await page.fill('#LastName', 'Peteklik');
        await page.fill('#BirthDate', '1971-04-12');
        await page.check('#NoPhone');
        await page.selectOption('#ap-province', { label: 'İstanbul' });
        await page.waitForFunction(() => document.getElementById('ap-district').options.length > 1);
        await page.selectOption('#ap-district', { index: 1 });
        await page.fill('#ap-locality', '[DEMO] Ormanköy');
        await page.fill('#ApiaryHives', '24');
        for (const [sel, v] of [['#ApiaryLocationLatitude', '41.1'], ['#ApiaryLocationLongitude', '29.1']]) if (await page.locator(sel).count()) await page.fill(sel, v);
        await page.check('#Attest');
        await page.evaluate(() => { const d = document.getElementById('registration-fallback'); if (d && d.tagName === 'DETAILS') d.open = true; else d?.classList.add('show'); });
        await page.fill('#FallbackReason', '[DEMO] The beekeeper has no mobile phone.');
        await page.setInputFiles('#evidence', pdf);
        await submit(page, page.locator('#registration-photo'));
        log('hasan registration', await flash(page));
    }
    await context.close();
}

// ---- 3. the beekeeper's portal account
async function portal(browser) {
    if (known(ZEYNEP_NUMBER)) return;
    const { context, page } = await admin(browser);
    await go(page, '/Sms/Outbox');
    const texts = await page.locator('table tbody tr').allInnerTexts();
    const credentialsSms = texts.find(t => /user name\s+\S+,/i.test(t) && /password/i.test(t));
    await context.close();
    if (!credentialsSms) { log('no credentials SMS in the outbox'); return; }
    const userName = (credentialsSms.match(/user name\s+([^,\s]+)/i) || [])[1];
    const temp = (credentialsSms.match(/password\s+(\S+?)\.(?:\s|$)/i) || [])[1];
    if (!userName || !temp) { log('credentials SMS not understood'); return; }
    const { context: c2, page: p2 } = await open(browser);
    const final = await signIn(p2, userName.replace(/[.,;]$/, ''), temp.replace(/[.,;]$/, ''));
    await c2.close();
    remember(userName.replace(/[.,;]$/, ''), '[DEMO] Zeynep Kovancı (beekeeper portal)', 'Beekeeper', final);
    log('portal account ready');
}

// ---- 4. a delivery in containers, the laboratory, the price and a correction
async function delivery(browser) {
    const zeynep = one(`SELECT BeekeeperId FROM Beekeepers WHERE BeekeeperBusinessNumber = '${ZEYNEP_NUMBER}'`);
    let lot = one(`SELECT TOP 1 i.RawHoneyProductionId FROM LotIntakes i JOIN RawHoneyProductions r ON r.RawHoneyProductionId = i.RawHoneyProductionId
        JOIN Apiaries a ON a.ApiaryId = r.RawHoneyProductionApiaryId WHERE i.ContainersRecorded = 1 AND a.ApiaryBeekeeperId = '${zeynep}' ORDER BY i.IntakeId DESC`);
    if (!lot) {
        const { context, page } = await open(browser);
        const sao = account({ org: 'Kovan', role: 'SampleAcceptanceOfficer' });
        await signIn(page, sao.user, sao.password);
        await go(page, `/Intake/Create?number=${encodeURIComponent(ZEYNEP_NUMBER)}`);
        await page.selectOption('#HoneyTypeId', '3');
        await page.selectOption('#LabOrgId', { index: 0 });
        const prov = page.locator('#deliveryForm select[data-province-select], select[data-province-select]').first();
        if (!(await prov.inputValue())) await prov.selectOption({ label: 'İstanbul' });
        const did = await prov.getAttribute('data-province-select');
        await page.waitForFunction(id => document.getElementById(id).options.length > 1, did);
        await page.locator('#' + did).selectOption({ index: 1 });
        if (await page.locator('#CounterLocation').count()) await page.fill('#CounterLocation', '[DEMO] Retain shelf 3');
        if (await page.locator('#containers tbody tr').count() < 2) await page.click('#add-container');
        const rows = page.locator('#containers tbody tr');
        const plan = [
            { kind: 0, net: '24,5', gross: '26', sealed: true, origin: 'DEMO-B-1001', jar: 'DEMO-J-2001', counter: 'DEMO-K-2001', reseal: 'DEMO-P-3001' },
            { kind: 0, net: '23,75', gross: '25,2', sealed: false, plant: 'DEMO-P-3002', jar: 'DEMO-J-2002', counter: 'DEMO-K-2002', reseal: 'DEMO-P-3003' },
        ];
        for (let i = 0; i < plan.length; i++) {
            const r = rows.nth(i), p = plan[i];
            const f = n => r.locator(`[data-field="${n}"]`);
            await f('Kind').selectOption({ index: p.kind });
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
        const errors = await page.locator('#intake-errors').count() ? await page.locator('#intake-errors').innerText() : '';
        if (errors) log('intake send:', errors.slice(0, 300));
        await page.fill('#Code', await demoCode(page));
        await submit(page, page.locator('#intake-confirm'));
        log('delivery', await flash(page));
        await context.close();
        lot = one(`SELECT TOP 1 i.RawHoneyProductionId FROM LotIntakes i JOIN RawHoneyProductions r ON r.RawHoneyProductionId = i.RawHoneyProductionId
            JOIN Apiaries a ON a.ApiaryId = r.RawHoneyProductionApiaryId WHERE i.ContainersRecorded = 1 AND a.ApiaryBeekeeperId = '${zeynep}' ORDER BY i.IntakeId DESC`);
    }
    if (!lot) throw new Error('the delivery was not recorded');

    // the laboratory: the first jar in order, the second with a popped cap (rejected), then the new jar
    const jars = () => one(`SELECT COUNT(*) FROM Samples WHERE RawHoneyProductionId = ${lot} AND Status = 'Sent'`);
    if (Number(jars()) > 0) {
        const { context, page } = await open(browser);
        const an = account({ org: 'Analiz', role: 'Analyst' });
        await signIn(page, an.user, an.password);
        for (let round = 0; round < 3 && Number(jars()) > 0; round++) {
            await go(page, '/Samples/Index');
            const forms = page.locator('form.receive-jar-form');
            const n = await forms.count();
            if (!n) break;
            const form = forms.first();
            const rejected = one(`SELECT COUNT(*) FROM Samples WHERE RawHoneyProductionId = ${lot} AND Status = 'Rejected'`);
            const fail = rejected === '0' && round === 1;
            for (const name of ['sealMatches', 'labelIntact', 'capIntact'])
                await form.locator(`input[name=${name}][value=${fail && name === 'capIntact' ? 'false' : 'true'}]`).check();
            if (fail) await form.locator('input[name=note]').fill('[DEMO] The cap button was up.');
            await submit(page, form.locator('button[type=submit]').first());
            log('reception', fail ? 'rejected' : 'received', await flash(page));
        }
        await context.close();
    }
    const rejected = one(`SELECT TOP 1 SampleId FROM Samples WHERE RawHoneyProductionId = ${lot} AND Status = 'Rejected'`);
    if (rejected && one(`SELECT COUNT(*) FROM Samples WHERE RawHoneyProductionId = ${lot}`) === '2') {
        const { context, page } = await open(browser);
        const sao = account({ org: 'Kovan', role: 'SampleAcceptanceOfficer' });
        await signIn(page, sao.user, sao.password);
        const intake = one(`SELECT IntakeId FROM LotIntakes WHERE RawHoneyProductionId = ${lot}`);
        await go(page, `/Intake/Details/${intake}`);
        const form = page.locator('form:has(button:has-text("Open, sample and seal again"))').first();
        await form.locator('input[name=jarSeal]').fill('DEMO-J-2004');
        await form.locator('input[name=counterSeal]').fill('DEMO-K-2004');
        await form.locator('input[name=capIntact]').check();
        await form.locator('input[name=reseal]').fill('DEMO-P-3004');
        await submit(page, form.locator('button[type=submit]'));
        log('new jar', await flash(page));
        await context.close();
    }

    // the beekeeper's asking price
    if (known(ZEYNEP_NUMBER) && !one(`SELECT TOP 1 PriceId FROM LotPrices WHERE RawHoneyProductionId = ${lot}`)) {
        const { context, page } = await open(browser);
        const bk = user(ZEYNEP_NUMBER);
        await signIn(page, bk.user, bk.password);
        await go(page, '/Me/Deliveries');
        page.once('dialog', d => d.accept());
        const form = page.locator(`form:has(input[name=pricePerKg])`).first();
        await form.locator('input[name=pricePerKg]').fill('285');
        await form.locator('select[name=currency]').selectOption('TRY').catch(() => {});
        await submit(page, form.locator('button[type=submit]'));
        log('price', await flash(page));
        await context.close();
    }

    // a delivery correction waiting for the second administrator
    const intake = one(`SELECT IntakeId FROM LotIntakes WHERE RawHoneyProductionId = ${lot}`);
    if (!one(`SELECT TOP 1 CorrectionId FROM DeliveryCorrections WHERE IntakeId = ${intake}`)) {
        const { context, page } = await admin(browser);
        await go(page, `/Intake/Details/${intake}`);
        const opener = page.locator('summary:has-text("Request a correction"), button:has-text("Request a correction")').first();
        if (await opener.count()) await opener.click();
        const form = page.locator('form:has([name=Reason])').filter({ has: page.locator('button:has-text("Send the request")') }).first();
        const net = form.locator('input[name*="NetKg"]').first();
        await net.fill('24.25');
        await form.locator('[name=Reason]').fill('[DEMO] The scale was not set to zero; the weighing slip shows 24.25 kg.');
        await submit(page, form.locator('button:has-text("Send the request")'));
        log('correction', await flash(page));
        await context.close();
    }
    return lot;
}

writePdf();
credentials();
const browser = await launch('en');
try {
    console.log('organizations'); await organizations(browser);
    console.log('registrations'); await registrations(browser);
    console.log('portal'); await portal(browser);
    console.log('delivery'); const lot = await delivery(browser);
    console.log('done, lot', lot);
} finally {
    await browser.close();
}
