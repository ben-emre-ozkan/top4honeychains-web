// D-86: builds the partner test pack of every country from <lang>/guide.md and <lang>/content.json, the demo data in
// demo-manifest.json (no passwords) and the suggested values in values.json:
//   node build.mjs [lang-CC ...]        default: es-AR pl-PL lv-LV tr-TR en-XX (XX = the coordinator's edition)
// Per edition: the guide PDF (branded like the user manual, with the manual's screenshots in the same language), a self-contained
// interactive checklist (HTML, ticks and notes kept in the browser, CSV export) and the feedback template (CSV).
// Output: PARTNER_OUT (default dist/) / <country folder>/. Parameter names per language come from the reference texts of
// the local database (PARTNER_DB, default TOP4HoneyChainsDb; reference table only).
import fs from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const manualDir = path.join(repo, 'docs', 'manual');
const { default: MarkdownIt } = await import(pathToFileURL(path.join(manualDir, 'node_modules', 'markdown-it', 'index.mjs')).href);
const fonts = path.join(repo, 'TOP4HONEYCHAINS', 'TOP4HoneyChains.Web', 'wwwroot', 'fonts');
const logo = path.join(repo, 'docs', 'brand', 'top4-logo-w3.svg');
const OUT = process.env.PARTNER_OUT || path.join(here, 'dist');
const DB = process.env.PARTNER_DB || 'TOP4HoneyChainsDb';
const DEMO_URL = process.env.PARTNER_DEMO_URL || 'https://demo.top4honeychains.intalalab.net';

const manifest = JSON.parse(fs.readFileSync(path.join(here, 'demo-manifest.json'), 'utf8'));
const values = JSON.parse(fs.readFileSync(path.join(here, 'values.json'), 'utf8'));
export const FOLDERS = { AR: 'AR-Argentina', PL: 'PL-Poland', LV: 'LV-Latvia', TR: 'TR-Turkiye', XX: 'EN-Coordinator' };
const COUNTRY_NAMES = {
    AR: { en: 'Argentina', es: 'Argentina', pl: 'Argentyna', lv: 'Argentīna', tr: 'Arjantin' },
    PL: { en: 'Poland', es: 'Polonia', pl: 'Polska', lv: 'Polija', tr: 'Polonya' },
    LV: { en: 'Latvia', es: 'Letonia', pl: 'Łotwa', lv: 'Latvija', tr: 'Letonya' },
    TR: { en: 'Türkiye', es: 'Türkiye', pl: 'Turcja', lv: 'Turcija', tr: 'Türkiye' },
};
// what a tester types in the scenario, per country
const PLAN = {
    AR: { registry: 'AR-DEMO-0002**nn**', phone: '+54 9 343 455-03**nn**', district: 'Paraná', honey: 'Flower Honey' },
    PL: { registry: 'PL-DEMO-0002**nn**', phone: '+48 512 000 3**nn**', district: 'Puławy', honey: 'Flower Honey' },
    LV: { registry: 'LV-DEMO-0002**nn**', phone: '+371 2000 03**nn**', district: 'Jelgava', honey: 'Flower Honey' },
    TR: { registry: 'TR4800000002**nn**', phone: '+90 555 000 93 **nn**', district: 'Milas', honey: 'Thyme Honey' },
};

const md = new MarkdownIt({ html: true, linkify: false, typographer: false });
md.core.ruler.push('heading_anchors', state => {
    const tokens = state.tokens;
    for (let i = 0; i < tokens.length; i++) {
        if (tokens[i].type !== 'heading_open') continue;
        const inline = tokens[i + 1];
        const m = inline.content.match(/\s*\{#([\w-]+)\}\s*$/);
        if (!m) continue;
        tokens[i].attrSet('id', m[1]);
        inline.content = inline.content.slice(0, m.index);
        const last = inline.children[inline.children.length - 1];
        if (last && last.type === 'text') last.content = last.content.replace(/\s*\{#[\w-]+\}\s*$/, '');
    }
});
const esc = s => String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const inline = s => md.renderInline(s ?? '');

// ---- reference texts (parameter names and units, CCP titles) per language, from the local database
function referenceTexts() {
    try {
        const out = execFileSync('sqlcmd', ['-S', '(localdb)\\MSSQLLocalDB', '-d', DB, '-I', '-W', '-h', '-1', '-s', '\t', '-f', '65001', '-Q',
            "SET NOCOUNT ON; SELECT Entity, Field, EntityKey, Culture, Value FROM ReferenceTranslations WHERE Entity IN ('Parameter','Ccp') AND Field IN ('Name','Unit','Title')"], { encoding: 'utf8' });
        const map = new Map();
        for (const line of out.split(/\r?\n/)) {
            const [entity, field, key, culture, ...rest] = line.split('\t');
            if (rest.length) map.set(`${entity}|${field}|${key}|${culture}`, rest.join('\t').trim());
        }
        return map;
    } catch (e) {
        console.log('reference texts not available (' + e.message.split('\n')[0] + '); English names are used');
        return new Map();
    }
}
const refs = referenceTexts();
const ref = (entity, field, key, lang, fallback) => (lang !== 'en' && refs.get(`${entity}|${field}|${key}|${lang}`)) || fallback;

// ---- one edition: language + country
function edition(lang, cc) {
    const content = JSON.parse(fs.readFileSync(path.join(here, lang, 'content.json'), 'utf8'));
    const L = content.labels;
    const generic = cc === 'XX';
    const data = generic ? null : manifest.Countries[cc];
    const vars = {};
    const prefix = generic ? 'xx' : cc.toLowerCase();
    const accountKeys = ['admin', 'registration', 'association', 'inspector', 'reception', 'staff', 'quality', 'plantadmin', 'analyst', 'labmanager', 'shop', 'shopquality', 'compliance'];
    for (const k of accountKeys) vars[`u.${k}`] = `\`${prefix}.${k}\``;
    if (generic) {
        const g = L.generic;
        for (const k of ['authority', 'association', 'plant', 'lab', 'shop']) vars[`org.${k}`] = g[k];
        Object.assign(vars, {
            country: g.country, countryName: g.country, registry: g.registry, phone: g.phone, region: g.region, district: g.district,
            currency: 'ARS / PLN / EUR / TRY', bn: 'XX-BEE-DEMO-0000**nn**', 'b1.name': g.b1, 'b3.name': g.b3,
            'example.batch': g.exampleBatch, 'example.story': g.exampleStory
        });
    } else {
        for (const k of ['authority', 'association', 'plant', 'lab', 'shop']) vars[`org.${k}`] = data.Orgs[k].Name;
        Object.assign(vars, {
            country: COUNTRY_NAMES[cc][lang], countryName: COUNTRY_NAMES[cc][lang], registry: `\`${PLAN[cc].registry.replace(/\*\*/g, '')}\``,
            phone: PLAN[cc].phone.replace(/\*\*/g, ''), region: data.Province, district: PLAN[cc].district, currency: data.Currency,
            bn: `${cc}-BEE-DEMO-0000nn`, 'b1.name': data.Beekeepers.b1.Name, 'b3.name': data.Beekeepers.b3.Name,
            'example.batch': data.Example.BatchTitle, 'example.story': `${DEMO_URL}/honeystory/${data.Example.Qr}`
        });
    }
    vars['demo.url'] = DEMO_URL;
    const fill = s => String(s ?? '').replace(/\{\{([\w.]+)\}\}/g, (all, k) => (k in vars ? vars[k] : all));
    return { lang, cc, content, L, generic, data, vars, fill, prefix, folder: FOLDERS[cc] };
}

// ---- generated blocks
function roleCards(e) {
    const { content, L, fill } = e;
    const cards = content.roles.map(r => {
        const accounts = r.accounts.length === 0 ? `<em>${esc(L.anyone)}</em>`
            : r.accounts.map(a => a === 'beekeeper'
                ? (e.generic ? `<code>XX-BEE-DEMO-0000nn</code>` : `<code>${esc(e.data.Beekeepers.b1.PortalUserName)}</code>`) + ` ${esc(L.beekeeperAccountNote)}`
                : `<code>${e.prefix}.${a}</code>`).join(', ');
        return `<section class="card" id="role-${r.id}">
<h2 id="role-${r.id}-h">${esc(r.name)}</h2>
<p class="who">${inline(fill(r.who))}</p>
<h4>${esc(L.does)}</h4>
<ul>${r.does.map(d => `<li>${inline(fill(d))}</li>`).join('')}</ul>
<table class="facts"><tr><th>${esc(L.screens)}</th><td>${inline(fill(r.screens))}</td></tr>
<tr><th>${esc(L.account)}</th><td>${accounts}</td></tr>
<tr><th>${esc(L.manual)}</th><td>${esc(L.chapter)} ${r.manual}</td></tr></table>
</section>`;
    });
    // three cards per page
    const pages = [];
    for (let i = 0; i < cards.length; i += 3) pages.push(`<div class="card-page">${cards.slice(i, i + 3).join('\n')}</div>`);
    return pages.join('\n');
}

function accountsBlock(e) {
    const { L, content } = e;
    const roleName = key => (content.roles.find(r => r.accounts.includes(key)) || {}).name || key;
    const orgOf = key => ({ admin: L.platformOrg, registration: e.vars['org.authority'], association: e.vars['org.association'], inspector: e.vars['org.association'],
        reception: e.vars['org.plant'], staff: e.vars['org.plant'], quality: e.vars['org.plant'], plantadmin: e.vars['org.plant'], analyst: e.vars['org.lab'],
        labmanager: e.vars['org.lab'], shop: e.vars['org.shop'], shopquality: e.vars['org.shop'], compliance: L.allOrgs })[key];
    const rows = ['admin', 'registration', 'association', 'inspector', 'reception', 'staff', 'quality', 'plantadmin', 'analyst', 'labmanager', 'shop', 'shopquality', 'compliance']
        .map(k => `| ${roleName(k)} | ${orgOf(k)} | \`${e.prefix}.${k}\` |`);
    let out = `Table: ${L.accountsTable} {#tbl-accounts}\n\n| ${L.role} | ${L.organization} | ${L.username} |\n|---|---|---|\n${rows.join('\n')}\n`;
    if (!e.generic) {
        const b = e.data.Beekeepers;
        out += `\n${L.beekeepersIntro}\n\nTable: ${L.beekeepersTable} {#tbl-beekeepers}\n\n| ${L.beekeeper} | ${L.businessNumber} | ${L.note} |\n|---|---|---|\n`
            + `| ${b.b1.Name} | \`${b.b1.BusinessNumber}\` | ${L.b1Note} |\n| ${b.b2.Name} | \`${b.b2.BusinessNumber}\` | ${L.b2Note} |\n| ${b.b3.Name} | \`${b.b3.BusinessNumber}\` | ${L.b3Note} |\n`;
    } else {
        out += `\nTable: ${L.demoDataTable} {#tbl-demodata}\n\n| ${L.country} | ${L.organizations} | ${L.beekeepers} |\n|---|---|---|\n`;
        for (const cc of ['AR', 'PL', 'LV', 'TR']) {
            const d = manifest.Countries[cc];
            out += `| ${COUNTRY_NAMES[cc].en} (\`${cc.toLowerCase()}.\`) | ${['authority', 'association', 'plant', 'lab', 'shop'].map(k => d.Orgs[k].Name).join('<br>')} | `
                + `${['b1', 'b2', 'b3'].map(k => `${d.Beekeepers[k].Name} (\`${d.Beekeepers[k].BusinessNumber}\`)`).join('<br>')}<br>${L.example}: ${d.Example.BatchTitle} |\n`;
        }
    }
    return out;
}

function scenarioBlock(e) {
    const { content, L, fill } = e;
    let out = '';
    for (const ph of content.phases) {
        out += `\n## ${L.part} ${ph.id}. ${ph.title} {#part-${ph.id.toLowerCase()}}\n\n`;
        for (const s of content.steps.filter(x => x.phase === ph.id)) {
            const account = s.account === null ? L.anyone : s.account === 'beekeeper' ? L.beekeeperAccount : `\`${e.prefix}.${s.account}\``;
            const role = s.account === null ? content.roles.find(r => r.id === 'public').name : (content.roles.find(r => r.accounts.includes(s.account)) || {}).name;
            out += `### ${L.step} ${s.id}. ${s.title} {#step-${s.id.toLowerCase()}}\n\n`;
            out += `<p class="who-line"><span class="pill">${esc(role)}</span> ${md.renderInline(account)} · ${esc(L.manual)}, ${esc(L.chapter)} ${s.manual}</p>\n\n`;
            out += s.do.map((d, i) => `${i + 1}. ${fill(d)}`).join('\n') + '\n\n';
            out += `> **${L.expect}:** ${fill(s.expect)}\n\n`;
            if (s.shot) out += `<!-- SHOT: ${s.shot} -->\nFigure: ${s.title}\n\n`;
        }
    }
    return out;
}

function feedbackBlock(e) {
    const { content, L } = e;
    let out = `Table: ${L.feedbackTable} {#tbl-feedback}\n\n<table class="fb"><colgroup><col style="width:38%"><col style="width:9%"><col style="width:11%"><col style="width:42%"></colgroup>`
        + `<thead><tr><th>${esc(L.step)}</th><th>${esc(L.works)}</th><th>${esc(L.doesNotWork)}</th><th>${esc(L.comment)}</th></tr></thead><tbody>`
        + content.steps.map(s => `<tr><td>${esc(s.id)}. ${esc(s.title)}</td><td>☐</td><td>☐</td><td></td></tr>`).join('') + '</tbody></table>\n\n';
    out += `## ${L.general} {#questions}\n\n`;
    out += content.questions.map((q, i) => `${i + 1}. ${q}\n\n   <div class="answer"></div>\n`).join('\n');
    return out;
}

function valuesBlock(e) {
    const { L, lang } = e;
    const rows = [];
    for (const p of manifest.Parameters.filter(p => p.Type !== 'File' && p.Ccp !== 'CCP 0')) {
        const key = `${p.Ccp}|${p.Name.trim()}`;
        let v;
        if (p.Type === 'Number') v = values.numbers[key];
        else if (values.texts[key]) v = values.texts[key].replace('{country}', e.vars.countryName).replace('{botanical}', e.generic ? '…' : (e.cc === 'TR' ? 'Thyme' : 'Multifloral'));
        else if (p.Ccp === 'CCP 0') v = p.Type === 'YesNo' ? L.yes : Object.entries(values.ccp0).find(([k]) => k !== 'yesNo' && k !== 'text' && p.Name.startsWith(k))?.[1] ?? values.ccp0.text;
        else v = L.yes;
        if (p.Type === 'YesNo' && v === 'Yes') v = L.yes;
        if (p.Type === 'YesNo' && v === 'No') v = L.no;
        const name = ref('Parameter', 'Name', String(p.Id), lang, p.Name);
        const unit = p.Unit && p.Unit !== 'None' ? ref('Parameter', 'Unit', String(p.Id), lang, p.Unit) : '';
        const limit = p.Min != null && p.Max != null ? `${p.Min} – ${p.Max}` : p.Min != null ? `≥ ${p.Min}` : p.Max != null ? `≤ ${p.Max}` : '';
        rows.push(`| ${p.Ccp.replace('CCP', L.ccp)} | ${name.replace(/\|/g, '/')} | ${unit} | ${limit} | ${v} |`);
    }
    const ccp0 = e.fill(L.ccp0Values).replace('{if}', values.ccp0['If yes']).replace('{extent}', values.ccp0['To what extent']).replace('{text}', values.ccp0.text);
    return `${ccp0}\n\nTable: ${L.valuesTable} {#tbl-values}\n\n| CCP | ${L.parameter} | ${L.unit} | ${L.limit} | ${L.value} |\n|---|---|---|---|---|\n${rows.join('\n')}\n`;
}

// ---- the guide (markdown -> HTML -> PDF)
function guideHtml(e) {
    const { L, lang, fill } = e;
    let source = fs.readFileSync(path.join(here, lang, 'guide.md'), 'utf8');
    source = source.replace('{{ROLECARDS}}', roleCards(e)).replace('{{ACCOUNTS}}', accountsBlock(e)).replace('{{SCENARIO}}', scenarioBlock(e))
        .replace('{{FEEDBACK}}', feedbackBlock(e)).replace('{{VALUES}}', valuesBlock(e))
        .replace('{{COORDINATORNOTE}}', e.generic ? `> ${L.coordinatorNote}` : '');
    source = fill(source);
    const figures = [], tables = [], problems = [];
    source = source.replace(/^<!--\s*SHOT:\s*([\w.-]+)\s*-->[ \t]*\r?\n(?:Figure:[ \t]*(.+))?$/gm, (_, id, caption) => {
        const img = path.join(manualDir, 'img', lang, `${id}.png`);
        if (!fs.existsSync(img)) { problems.push(`no image ${lang}/${id}`); return ''; }
        const n = figures.push({ id: `fig-${id}-${figures.length}`, caption });
        const width = fs.readFileSync(img).readUInt32BE(16);
        return `<figure class="shot${width <= 800 ? ' phone' : ''}"><img src="${pathToFileURL(img).href}" alt=""><figcaption><b>${L.figure} ${n}.</b> ${inline(caption)}</figcaption></figure>\n`;
    });
    source = source.replace(/^Table:[ \t]*(.+?)\s*\{#(tbl-[\w-]+)\}[ \t]*$/gm, (_, caption, id) => {
        const n = tables.push({ id, caption });
        return `<p class="tbl-caption" id="${id}"><b>${L.table} ${n}.</b> ${inline(caption)}</p>\n`;
    });
    const number = Object.fromEntries(tables.map((t, i) => [t.id, `${L.table} ${i + 1}`]));
    source = source.replace(/\[[^\]]*\]\(#(tbl-[\w-]+)\)/g, (all, id) => (number[id] ? `[${number[id]}](#${id})` : (problems.push('unknown ' + id), all)));
    // "Table \"Suggested values\"" in the step texts points to the values table
    source = source.replace(new RegExp(`${L.table} "${L.valuesTable.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"`, 'g'), `[${number['tbl-values']}](#tbl-values)`);
    const leftover = source.match(/\{\{[\w.]+\}\}/g);
    if (leftover) problems.push('placeholders left: ' + [...new Set(leftover)].join(' '));
    const body = md.render(source);
    const toc = [...body.matchAll(/<h([12]) id="([\w-]+)">(.*?)<\/h\1>/g)]
        .map(m => `<li class="toc-${m[1]}"><a href="#${m[2]}">${m[3].replace(/<[^>]+>/g, '')}</a></li>`).join('\n');
    const svg = fs.readFileSync(logo, 'utf8').replace(/<\?xml[^>]*>/, '');
    const font = f => pathToFileURL(path.join(fonts, f)).href;
    const scope = e.generic ? L.coordinatorScope : e.vars.country;
    const html = `<!doctype html><html lang="${lang}"><head><meta charset="utf-8"><title>TOP4HoneyChains · ${esc(L.title)} · ${esc(scope)}</title><style>
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 700; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-ext-700-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 700; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-700-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 800; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-ext-800-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 800; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-800-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Instrument Sans'; font-weight: 400; src: url(${font('instrument-sans/instrument-sans-latin-ext-400-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Instrument Sans'; font-weight: 400; src: url(${font('instrument-sans/instrument-sans-latin-400-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Instrument Sans'; font-weight: 600; src: url(${font('instrument-sans/instrument-sans-latin-ext-600-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Instrument Sans'; font-weight: 600; src: url(${font('instrument-sans/instrument-sans-latin-600-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
:root { --foam: #FFFBF0; --cream: #FFF4D6; --honey: #FFB547; --heading: #8C5E2E; --ink: #574A40; --muted: #7B6A5C; --line: #F2E3C6; --trust: #0B7A75; --trust-soft: #E8F8F7; }
@page { size: A4; margin: 20mm 17mm 19mm; }
@page :first { margin: 0; }
* { box-sizing: border-box; }
html { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
body { margin: 0; font: 400 10.3pt/1.5 'Instrument Sans', 'Segoe UI', system-ui, sans-serif; color: var(--ink); }
h1, h2, h3, h4 { font-family: 'Bricolage Grotesque', 'Segoe UI', sans-serif; color: var(--heading); line-height: 1.2; break-after: avoid; }
h1 { font-size: 21pt; font-weight: 800; margin: 0 0 12pt; padding-bottom: 7pt; border-bottom: 3px solid var(--honey); break-before: page; }
h2 { font-size: 14.5pt; font-weight: 700; margin: 16pt 0 7pt; }
h3 { font-size: 11.5pt; font-weight: 700; margin: 14pt 0 5pt; padding-top: 6pt; border-top: 1px solid var(--line); }
h4 { font-size: 10pt; margin: 8pt 0 3pt; color: var(--ink); }
p, ul, ol { margin: 0 0 7pt; }
li { margin: 0 0 3pt; }
a { color: var(--trust); text-decoration: none; }
code { font-family: Consolas, 'Cascadia Mono', monospace; font-size: 9pt; background: var(--foam); border: 1px solid var(--line); border-radius: 4px; padding: 0 3px; }
strong { color: #4A3F36; }
td:first-child { white-space: nowrap; } table { width: 100%; border-collapse: collapse; margin: 5pt 0 11pt; font-size: 8.8pt; }
th { background: var(--cream); color: var(--heading); font-family: 'Bricolage Grotesque', sans-serif; font-weight: 700; text-align: left; }
th, td { border: 1px solid var(--line); padding: 4pt 6pt; vertical-align: top; }
tr { break-inside: avoid; }
tbody tr:nth-child(even) td { background: #FFFDF7; }
blockquote { margin: 6pt 0 10pt; padding: 7pt 11pt; background: var(--trust-soft); border-left: 4px solid var(--trust); border-radius: 0 8px 8px 0; break-inside: avoid; }
blockquote p:last-child { margin: 0; }
figure.shot { margin: 6pt 0 12pt; break-inside: avoid; }
figure.shot img { max-width: 100%; max-height: 82mm; border: 1px solid var(--line); border-radius: 8px; }
figure.shot.phone { text-align: center; } figure.shot.phone img { max-width: 48mm; max-height: 95mm; }
figure.shot figcaption, p.tbl-caption { font-size: 8.8pt; color: var(--muted); margin: 3pt 0 0; }
p.tbl-caption { margin: 9pt 0 2pt; break-after: avoid; }
figure.shot figcaption b, p.tbl-caption b { color: var(--heading); }
div.card-page { break-before: page; } section.card { break-inside: avoid; border: 1px solid var(--line); border-radius: 12px; padding: 0 12pt 4pt; margin: 0 0 9pt; font-size: 9.6pt; } section.card li { margin: 0 0 1pt; }
section.card h2 { font-size: 13.5pt; margin: 0 -12pt 6pt; padding: 6pt 12pt; background: var(--cream); border-radius: 12px 12px 0 0; border-left: 6px solid var(--honey); }
section.card p.who { font-size: 9.8pt; margin-bottom: 4pt; } section.card h4 { margin: 4pt 0 2pt; } section.card ul { margin-bottom: 5pt; } section.card table.facts { margin-bottom: 4pt; }
section.card table.facts th { width: 30%; }
p.who-line { margin: 0 0 6pt; color: var(--muted); }
span.pill { display: inline-block; padding: 1pt 7pt; border-radius: 999px; background: var(--cream); color: var(--heading); font-weight: 600; border: 1px solid var(--line); }
div.answer { height: 16mm; border: 1px dashed var(--line); border-radius: 6px; margin: 3pt 0 6pt; }
.cover { height: 297mm; padding: 34mm 24mm 24mm; display: flex; flex-direction: column; background: radial-gradient(70% 55% at 100% 0%, rgba(255, 210, 90, 0.45), transparent 70%), radial-gradient(60% 50% at 0% 100%, rgba(255, 231, 174, 0.7), transparent 70%), linear-gradient(180deg, #FFFFFF, var(--foam)); break-after: page; }
.cover .logo svg { width: 115mm; height: auto; }
.cover .project { margin-top: 24mm; font: 700 12pt 'Bricolage Grotesque', sans-serif; letter-spacing: 0.08em; text-transform: uppercase; color: var(--heading); }
.cover .title { margin: 6mm 0 0; font: 800 32pt/1.05 'Bricolage Grotesque', sans-serif; color: #4A3F36; border: 0; padding: 0; break-before: avoid; }
.cover .subtitle { margin-top: 4mm; font-size: 15pt; color: var(--heading); }
.cover .scope { margin-top: 10mm; display: inline-block; padding: 3mm 7mm; border-radius: 999px; background: #FFFFFF; border: 1px solid var(--line); font: 700 14pt 'Bricolage Grotesque', sans-serif; color: var(--heading); }
.cover .draft { margin-top: 6mm; display: inline-block; padding: 2mm 5mm; border-radius: 999px; background: #FFF0E0; color: #A2450B; font-weight: 600; border: 1px solid #F3C9A0; }
.cover .meta { margin-top: auto; font-size: 10pt; color: var(--muted); border-top: 1px solid var(--line); padding-top: 4mm; }
nav.toc { break-after: page; } nav.toc h1 { break-before: auto; } nav.toc ul { list-style: none; padding: 0; }
nav.toc li.toc-1 { margin-top: 6pt; font-weight: 600; color: var(--heading); } nav.toc li.toc-2 { margin-left: 14pt; font-size: 9.3pt; } nav.toc a { color: inherit; }
</style></head><body>
<section class="cover"><div class="logo">${svg}</div><div class="project">TOP4HoneyChains · ICT-AGRI-FOOD 2022</div>
<h1 class="title">${esc(L.title)}</h1><div class="subtitle">${esc(L.subtitle)}</div>
<div><span class="scope">${esc(scope)} · ${esc(L.language)}</span></div>
${L.draft ? `<div><span class="draft">${esc(L.draft)}</span></div>` : ''}
<div class="meta">${esc(L.month)} · ${esc(DEMO_URL)}</div></section>
<nav class="toc"><h1 id="contents">${esc(L.toc)}</h1><ul>${toc}</ul></nav>
<main>${body}</main></body></html>`;
    return { html, problems, figures: figures.length, tables: tables.length };
}

// ---- the interactive checklist (self-contained HTML)
function checklistHtml(e) {
    const { content, L, fill, lang } = e;
    const scope = e.generic ? L.coordinatorScope : e.vars.country;
    const plain = s => esc(fill(s)).replace(/\*\*(.+?)\*\*/g, '<b>$1</b>').replace(/`(.+?)`/g, '<code>$1</code>').replace(/_(nn)_/g, '$1');
    const steps = content.steps.map(s => ({
        id: s.id, phase: s.phase, title: s.title,
        account: s.account === null ? L.anyone : s.account === 'beekeeper' ? L.beekeeperAccountPlain : `${e.prefix}.${s.account}`,
        role: s.account === null ? content.roles.find(r => r.id === 'public').name : (content.roles.find(r => r.accounts.includes(s.account)) || {}).name,
        doHtml: s.do.map(d => `<li>${plain(d)}</li>`).join(''), expect: plain(s.expect)
    }));
    const key = `t4h-partner-checklist-${e.cc}-${lang}`;
    const phases = content.phases.map(ph => `<section class="phase"><h2>${esc(L.part)} ${ph.id}. ${esc(ph.title)}</h2>${steps.filter(s => s.phase === ph.id).map(s => `
<article class="step" data-step="${s.id}">
  <header><label class="done"><input type="checkbox" data-k="done"> <span class="sid">${s.id}</span> <b>${esc(s.title)}</b></label>
  <span class="who"><span class="pill">${esc(s.role)}</span> <code>${esc(s.account)}</code></span></header>
  <details><summary>${esc(L.todo)}</summary><ol>${s.doHtml}</ol></details>
  <p class="expect"><b>${esc(L.expect)}:</b> ${s.expect}</p>
  <div class="result" role="radiogroup" aria-label="${esc(L.result)}">
    <label><input type="radio" name="r-${s.id}" value="works" data-k="result"> ${esc(L.works)}</label>
    <label><input type="radio" name="r-${s.id}" value="fails" data-k="result"> ${esc(L.doesNotWork)}</label>
    <label><input type="radio" name="r-${s.id}" value="skipped" data-k="result"> ${esc(L.notTested)}</label>
  </div>
  <textarea data-k="note" rows="2" placeholder="${esc(L.comment)}" aria-label="${esc(L.comment)} ${s.id}"></textarea>
</article>`).join('')}</section>`).join('');
    const questions = content.questions.map((q, i) => `<div class="q"><label for="q${i + 1}"><b>${i + 1}.</b> ${esc(q)}</label><textarea id="q${i + 1}" data-q="${i + 1}" rows="3"></textarea></div>`).join('');
    const csvMeta = { country: e.generic ? 'XX' : e.cc, lang, labels: { works: L.works, fails: L.doesNotWork, skipped: L.notTested },
        header: [L.country, 'Language', L.tester, L.date, L.step, L.step + ' / ' + L.question, L.role, L.account, L.result, L.comment + ' / ' + L.answer],
        steps: steps.map(s => [s.id, s.title, s.role, s.account]), questions: content.questions };
    return `<!doctype html>
<html lang="${lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>TOP4HoneyChains · ${esc(L.checklistTitle)} · ${esc(scope)}</title>
<style>
:root{--bg:#FFFBF0;--surface:#FFFFFF;--ink:#3F352D;--muted:#6E5F52;--line:#F0DFC0;--honey:#8C5E2E;--accent:#FFB547;--chip:#FFF4D6;--ok:#1F6B4A;--ok-tint:#E3F3EA;--bad:#A2450B;--bad-tint:#FDEBDD;color-scheme:light}
@media (prefers-color-scheme: dark){:root:not([data-theme="light"]){--bg:#17150F;--surface:#211E17;--ink:#F2ECE0;--muted:#B5AC9C;--line:#3A3428;--honey:#F0B44A;--accent:#F0B44A;--chip:#2C271E;--ok:#7FC8A4;--ok-tint:#17332A;--bad:#F2A07B;--bad-tint:#3A2118;color-scheme:dark}}
:root[data-theme="dark"]{--bg:#17150F;--surface:#211E17;--ink:#F2ECE0;--muted:#B5AC9C;--line:#3A3428;--honey:#F0B44A;--accent:#F0B44A;--chip:#2C271E;--ok:#7FC8A4;--ok-tint:#17332A;--bad:#F2A07B;--bad-tint:#3A2118;color-scheme:dark}
*{box-sizing:border-box}
body{margin:0;background:var(--bg);color:var(--ink);font:15px/1.55 'Segoe UI',system-ui,-apple-system,sans-serif;padding:24px 16px 64px}
.wrap{max-width:980px;margin:0 auto;display:flex;flex-direction:column;gap:22px}
h1,h2{color:var(--honey);margin:0;line-height:1.2} h1{font-size:clamp(24px,4vw,34px)} h2{font-size:20px;margin-bottom:10px}
.lead{color:var(--muted);max-width:75ch}
.bar{display:flex;flex-wrap:wrap;gap:10px;align-items:center;position:sticky;top:0;background:var(--bg);padding:10px 0;z-index:2;border-bottom:1px solid var(--line)}
.bar button{min-height:40px;padding:8px 14px;border-radius:10px;border:1px solid var(--line);background:var(--surface);color:var(--ink);font:inherit;cursor:pointer}
.bar button.primary{background:var(--accent);border-color:var(--accent);color:#2B1D05;font-weight:700}
.bar .progress{margin-left:auto;color:var(--muted);font-weight:600}
.bar input{min-height:40px;padding:6px 10px;border-radius:10px;border:1px solid var(--line);background:var(--surface);color:var(--ink);font:inherit;min-width:200px}
.step{background:var(--surface);border:1px solid var(--line);border-radius:14px;padding:14px 16px;margin-bottom:12px;display:flex;flex-direction:column;gap:8px}
.step.is-done{border-color:var(--ok)}
.step header{display:flex;flex-wrap:wrap;gap:8px 14px;align-items:center;justify-content:space-between}
.done{display:flex;gap:8px;align-items:center;cursor:pointer} .done input{width:20px;height:20px}
.sid{display:inline-block;min-width:34px;text-align:center;font-weight:800;color:var(--honey)}
.who{color:var(--muted);font-size:14px}
.pill{display:inline-block;padding:2px 9px;border-radius:999px;background:var(--chip);color:var(--honey);font-weight:600;font-size:13px}
code{font-family:Consolas,ui-monospace,monospace;font-size:13px;background:var(--chip);padding:1px 5px;border-radius:5px}
details summary{cursor:pointer;color:var(--honey);font-weight:600} details ol{margin:6px 0 0;padding-left:22px}
.expect{margin:0;padding:8px 10px;border-left:4px solid var(--ok);background:var(--ok-tint);border-radius:0 8px 8px 0;font-size:14px}
.result{display:flex;flex-wrap:wrap;gap:6px 16px} .result label{display:flex;gap:6px;align-items:center;cursor:pointer;min-height:32px}
textarea{width:100%;border:1px solid var(--line);border-radius:10px;padding:8px 10px;background:var(--bg);color:var(--ink);font:inherit;resize:vertical}
.step.fails{border-color:var(--bad);background:linear-gradient(0deg,var(--bad-tint),var(--surface) 60%)}
.q{background:var(--surface);border:1px solid var(--line);border-radius:14px;padding:12px 14px;margin-bottom:10px;display:flex;flex-direction:column;gap:6px}
.accounts{width:100%;border-collapse:collapse;font-size:14px;background:var(--surface);border-radius:12px;overflow:hidden}
.accounts th,.accounts td{text-align:left;padding:7px 10px;border-bottom:1px solid var(--line)} .accounts th{background:var(--chip);color:var(--honey)}
.table-box{overflow-x:auto;border:1px solid var(--line);border-radius:12px}
@media print{.bar button,.bar .progress{display:none}.bar{position:static}details{display:block}details>*{display:block}.step{break-inside:avoid}}
</style></head>
<body><div class="wrap">
<header><div class="lead" style="font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--honey)">TOP4HoneyChains · ${esc(L.title)}</div>
<h1>${esc(L.checklistTitle)} · ${esc(scope)}</h1>
<p class="lead">${esc(L.checklistIntro)}</p>
<p class="lead">${esc(L.demoSite)}: <a href="${esc(DEMO_URL)}">${esc(DEMO_URL)}</a> · ${esc(L.inboxHint)}</p></header>
<div class="bar"><input id="tester" placeholder="${esc(L.testerName)}" aria-label="${esc(L.testerName)}">
<button class="primary" id="export">${esc(L.export)}</button><button id="print">${esc(L.print)}</button><button id="reset">${esc(L.reset)}</button>
<span class="progress" id="progress"></span></div>
<section><h2>${esc(L.accounts)}</h2><div class="table-box"><table class="accounts"><thead><tr><th>${esc(L.role)}</th><th>${esc(L.username)}</th></tr></thead><tbody>
${['admin', 'registration', 'association', 'inspector', 'reception', 'staff', 'quality', 'plantadmin', 'analyst', 'labmanager', 'shop', 'shopquality', 'compliance'].map(k => {
        const id = { plantadmin: 'association', shopquality: 'quality' }[k] || k;
        return `<tr><td>${esc((content.roles.find(r => r.id === id) || {}).name || k)}</td><td><code>${e.prefix}.${k}</code></td></tr>`;
    }).join('')}
<tr><td>${esc(content.roles.find(r => r.id === 'beekeeper').name)}</td><td>${esc(L.beekeeperAccountPlain)}</td></tr></tbody></table></div>
<p class="lead">${esc(L.passwordsHint)}</p></section>
${phases}
<section><h2>${esc(L.general)}</h2>${questions}</section>
<p class="lead">${esc(L.csvHint)}</p>
</div>
<script>
(function () {
  var KEY = ${JSON.stringify(key)}, META = ${JSON.stringify(csvMeta)};
  var state = {};
  try { state = JSON.parse(localStorage.getItem(KEY) || '{}') || {}; } catch (e) { state = {}; }
  function save() { try { localStorage.setItem(KEY, JSON.stringify(state)); } catch (e) { /* private mode: nothing kept */ } }
  var tester = document.getElementById('tester');
  tester.value = state.tester || '';
  tester.addEventListener('input', function () { state.tester = tester.value; save(); });
  document.querySelectorAll('.step').forEach(function (el) {
    var id = el.getAttribute('data-step'), s = state[id] || (state[id] = {});
    var done = el.querySelector('[data-k=done]'), note = el.querySelector('[data-k=note]');
    done.checked = !!s.done; note.value = s.note || '';
    el.querySelectorAll('[data-k=result]').forEach(function (r) { r.checked = r.value === s.result; });
    function paint() { el.classList.toggle('is-done', !!s.done); el.classList.toggle('fails', s.result === 'fails'); progress(); }
    done.addEventListener('change', function () { s.done = done.checked; save(); paint(); });
    note.addEventListener('input', function () { s.note = note.value; save(); });
    el.querySelectorAll('[data-k=result]').forEach(function (r) { r.addEventListener('change', function () { s.result = r.value; if (r.value !== 'skipped') s.done = true; done.checked = !!s.done; save(); paint(); }); });
    paint();
  });
  document.querySelectorAll('[data-q]').forEach(function (t) {
    var k = 'q' + t.getAttribute('data-q');
    t.value = state[k] || '';
    t.addEventListener('input', function () { state[k] = t.value; save(); });
  });
  function progress() {
    var all = document.querySelectorAll('.step').length, n = document.querySelectorAll('.step.is-done').length;
    document.getElementById('progress').textContent = n + ' / ' + all + ' ${esc(L.progress)}';
  }
  function cell(v) { v = String(v == null ? '' : v); return /[;"\\n\\r]/.test(v) ? '"' + v.replace(/"/g, '""') + '"' : v; }
  document.getElementById('export').addEventListener('click', function () {
    var date = new Date().toISOString().slice(0, 10), rows = [META.header];
    META.steps.forEach(function (s) { var st = state[s[0]] || {}; rows.push([META.country, META.lang, state.tester || '', date, s[0], s[1], s[2], s[3], st.result ? META.labels[st.result] : '', st.note || '']); });
    META.questions.forEach(function (q, i) { rows.push([META.country, META.lang, state.tester || '', date, 'Q' + (i + 1), q, '', '', '', state['q' + (i + 1)] || '']); });
    var csv = '\\ufeff' + rows.map(function (r) { return r.map(cell).join(';'); }).join('\\r\\n') + '\\r\\n';
    var a = document.createElement('a');
    a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    a.download = 'TOP4HoneyChains-feedback-' + META.country + '-' + (state.tester || 'tester').replace(/[^\\w-]+/g, '_') + '-' + date + '.csv';
    document.body.appendChild(a); a.click(); a.remove();
  });
  document.getElementById('print').addEventListener('click', function () { document.querySelectorAll('details').forEach(function (d) { d.open = true; }); window.print(); });
  document.getElementById('reset').addEventListener('click', function () { if (confirm(${JSON.stringify(L.resetConfirm)})) { state = {}; save(); location.reload(); } });
})();
</script></body></html>`;
}

// ---- the feedback template (CSV, semicolon, UTF-8 with BOM for Excel)
function feedbackCsv(e) {
    const { content, L } = e;
    const cell = v => { v = String(v ?? ''); return /[;"\n\r]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v; };
    const rows = [[L.country, 'Language', L.tester, L.date, L.step, `${L.step} / ${L.question}`, L.role, L.account, `${L.result} (${L.works} / ${L.doesNotWork} / ${L.notTested})`, `${L.comment} / ${L.answer}`]];
    for (const s of content.steps) {
        const role = s.account === null ? content.roles.find(r => r.id === 'public').name : (content.roles.find(r => r.accounts.includes(s.account)) || {}).name;
        const account = s.account === null ? L.anyone : s.account === 'beekeeper' ? L.beekeeperAccountPlain : `${e.prefix}.${s.account}`;
        rows.push([e.generic ? 'XX' : e.cc, e.lang, '', '', s.id, s.title, role, account, '', '']);
    }
    content.questions.forEach((q, i) => rows.push([e.generic ? 'XX' : e.cc, e.lang, '', '', `Q${i + 1}`, q, '', '', '', '']));
    return '﻿' + rows.map(r => r.map(cell).join(';')).join('\r\n') + '\r\n';
}

async function toPdf(htmlFile, pdfFile, footer) {
    const { chromium } = await import(pathToFileURL(path.join(manualDir, 'node_modules', 'playwright-core', 'index.mjs')).href);
    let browser;
    for (const o of [{ channel: 'chrome' }, { channel: 'msedge' }, {}]) { try { browser = await chromium.launch({ headless: true, ...o }); break; } catch { /* next */ } }
    if (!browser) throw new Error('no Chrome, Edge or Chromium found');
    try {
        const page = await browser.newPage();
        await page.goto(pathToFileURL(htmlFile).href, { waitUntil: 'load' });
        await page.evaluate(() => document.fonts.ready);
        await page.pdf({ path: pdfFile, format: 'A4', printBackground: true, preferCSSPageSize: true, displayHeaderFooter: true, headerTemplate: '<span></span>',
            footerTemplate: `<div style="width:100%;font:8px 'Segoe UI',sans-serif;color:#7B6A5C;padding:0 17mm;display:flex;justify-content:space-between"><span>${footer}</span><span><span class="pageNumber"></span> / <span class="totalPages"></span></span></div>` });
    } finally { await browser.close(); }
}
const pages = f => (fs.readFileSync(f).toString('latin1').match(/\/Type\s*\/Page[^s]/g) || []).length;

const wanted = process.argv.slice(2).length ? process.argv.slice(2) : ['es-AR', 'pl-PL', 'lv-LV', 'tr-TR', 'en-XX'];
for (const w of wanted) {
    const [lang, cc] = w.split('-');
    const e = edition(lang, cc);
    const dir = path.join(OUT, e.folder);
    fs.mkdirSync(dir, { recursive: true });
    const tag = `${lang.toUpperCase()}-${cc}`;
    const g = guideHtml(e);
    const htmlFile = path.join(dir, `.guide-${tag}.html`);
    fs.writeFileSync(htmlFile, g.html);
    const pdfFile = path.join(dir, `TOP4HoneyChains-Partner-Test-Guide-${tag}.pdf`);
    await toPdf(htmlFile, pdfFile, `TOP4HoneyChains · ${esc(e.L.title)} · ${esc(e.generic ? e.L.coordinatorScope : e.vars.country)} · ${esc(e.L.month)}`);
    fs.rmSync(htmlFile);
    fs.writeFileSync(path.join(dir, `TOP4HoneyChains-Test-Checklist-${tag}.html`), checklistHtml(e));
    fs.writeFileSync(path.join(dir, `TOP4HoneyChains-Feedback-Template-${tag}.csv`), feedbackCsv(e));
    for (const p of g.problems) console.log(`${tag}: ${p}`);
    console.log(`${tag}: ${pages(pdfFile)} pages, ${g.figures} figures, ${g.tables} tables, ${g.problems.length} problems -> ${path.relative(repo, dir)}`);
}
