// Builds the TOP4HoneyChains user manual: docs/manual/<lang>/*.md (in file-name order) -> one branded PDF and one HTML
// file per language in docs/manual/dist/. Usage: npm install, then `node build.mjs [lang ...]` (default: all five).
// - Headings carry explicit anchors (`## Sign-in {#sign-in}`) so every language has the same ids.
// - `<!-- SHOT: <id> -->` lines become figures when img/<lang>/<id>.png exists (captured by capture.mjs), and nothing otherwise.
// - Brand: honey-foam palette and fonts from the app (wwwroot/css/brand.css, wwwroot/fonts), W3 logo from docs/brand.
// - PDF through Playwright with the installed Chrome (or Chromium); if that fails the HTML is still written.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import MarkdownIt from 'markdown-it';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const fonts = path.join(repo, 'TOP4HONEYCHAINS', 'TOP4HoneyChains.Web', 'wwwroot', 'fonts');
const logo = path.join(repo, 'docs', 'brand', 'top4-logo-w3.svg');
const dist = path.join(here, 'dist');
const LANGS = {
    en: { name: 'English', title: 'User manual', draft: null, toc: 'Contents', version: 'Version', page: 'Page', month: 'October 2026',
          figure: 'Figure', table: 'Table', figures: 'List of figures', tables: 'List of tables',
          consortium: 'Consortium', funding: 'Funding', acknowledgement: 'Carried out within the ICT-AGRI-FOOD ERA-NET Cofund (2022 call).' },
    tr: { name: 'Türkçe', title: 'Kullanım kılavuzu', draft: null, toc: 'İçindekiler', version: 'Sürüm', page: 'Sayfa', month: 'Ekim 2026',
          figure: 'Şekil', table: 'Tablo', figures: 'Şekiller listesi', tables: 'Tablolar listesi',
          consortium: 'Konsorsiyum', funding: 'Finansman', acknowledgement: 'ICT-AGRI-FOOD ERA-NET Cofund (2022 çağrısı) kapsamında yürütülmektedir.' },
    es: { name: 'Español', title: 'Manual de usuario', draft: 'Borrador, para revisión de los socios', toc: 'Índice', version: 'Versión', page: 'Página', month: 'Octubre de 2026',
          figure: 'Figura', table: 'Tabla', figures: 'Índice de figuras', tables: 'Índice de tablas',
          consortium: 'Consorcio', funding: 'Financiación', acknowledgement: 'Realizado en el marco del ERA-NET Cofund ICT-AGRI-FOOD (convocatoria 2022).' },
    pl: { name: 'Polski', title: 'Podręcznik użytkownika', draft: 'Wersja robocza do przeglądu przez partnerów', toc: 'Spis treści', version: 'Wersja', page: 'Strona', month: 'Październik 2026',
          figure: 'Rysunek', table: 'Tabela', figures: 'Spis rysunków', tables: 'Spis tabel',
          consortium: 'Konsorcjum', funding: 'Finansowanie', acknowledgement: 'Realizowany w ramach ERA-NET Cofund ICT-AGRI-FOOD (nabór 2022).' },
    lv: { name: 'Latviešu', title: 'Lietotāja rokasgrāmata', draft: 'Melnraksts partneru pārskatīšanai', toc: 'Saturs', version: 'Versija', page: 'Lapa', month: '2026. gada oktobris',
          figure: 'Attēls', table: 'Tabula', figures: 'Attēlu saraksts', tables: 'Tabulu saraksts',
          consortium: 'Konsorcijs', funding: 'Finansējums', acknowledgement: 'Īstenots ICT-AGRI-FOOD ERA-NET Cofund (2022. gada konkursa) ietvaros.' },
};
// the edition shown on the cover and in the footer: the month of the release in each language (MANUAL_MONTH overrides it)
const edition = lang => process.env.MANUAL_MONTH || LANGS[lang].month;

// ---- markdown: explicit heading anchors {#id}, tables, plain links
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

function render(lang) {
    const dir = path.join(here, lang);
    const files = fs.readdirSync(dir).filter(f => /^\d\d-.*\.md$/.test(f)).sort();
    const L0 = LANGS[lang];
    let source = files.map(f => fs.readFileSync(path.join(dir, f), 'utf8')).join('\n\n');
    // Figures and tables are numbered in order of appearance:
    // - a screenshot is a `<!-- SHOT: <id> -->` line followed by a `Figure: <caption>` line; its anchor is #fig-<id>;
    // - a table is preceded by a `Table: <caption> {#tbl-<id>}` line;
    // - a link [Figure](#fig-<id>) or [Table](#tbl-<id>) gets the number as its text ("Figure 3", "Tabla 5").
    const figures = [], tables = [], problems = [];
    let shots = 0, present = 0;
    source = source.replace(/^<!--\s*SHOT:\s*([\w.-]+)\s*-->[ \t]*\r?\n(?:[ \t]*\r?\n)*(?:Figure:[ \t]*(.+))?$/gm, (_, id, caption) => {
        shots++;
        if (!caption) problems.push(`figure ${id} has no caption`);
        const n = figures.push({ id: `fig-${id}`, caption: caption || id });
        const img = path.join(here, 'img', lang, `${id}.png`);
        if (!fs.existsSync(img)) return '';
        present++;
        // phone captures (390 px at scale 2 = 780 px wide) print narrow and tall; desktop captures use the page width
        const width = fs.readFileSync(img).readUInt32BE(16);
        // a blank line after the figure ends the HTML block, so a heading or table right below it is still Markdown
        return `<figure class="shot${width <= 800 ? ' phone' : ''}" id="fig-${id}"><img src="${pathToFileURL(img).href}" alt="">` +
            `<figcaption><b>${L0.figure} ${n}.</b> ${md.renderInline(caption || '')}</figcaption></figure>\n`;
    });
    source = source.replace(/^Table:[ \t]*(.+?)\s*\{#(tbl-[\w-]+)\}[ \t]*$/gm, (_, caption, id) => {
        const n = tables.push({ id, caption });
        return `<p class="tbl-caption" id="${id}"><b>${L0.table} ${n}.</b> ${md.renderInline(caption)}</p>\n`;
    });
    // every Markdown table needs a caption line right above it (blank lines in between are allowed)
    let before = '';
    for (const line of source.split(/\r?\n/)) {
        if (/^\|/.test(line) && !/^\|/.test(before) && !/class="tbl-caption"/.test(before)) problems.push(`table without caption: ${line.slice(0, 60)}`);
        if (line.trim() !== '') before = line;
    }
    const number = Object.fromEntries([...figures.map((f, i) => [f.id, `${L0.figure} ${i + 1}`]), ...tables.map((t, i) => [t.id, `${L0.table} ${i + 1}`])]);
    source = source.replace(/\[[^\]]*\]\(#((?:fig|tbl)-[\w-]+)\)/g, (all, id) => {
        if (!number[id]) { problems.push(`reference to unknown ${id}`); return all; }
        return `[${number[id]}](#${id})`;
    });
    for (const p of problems) console.log(`${lang}: ${p}`);
    // wide tables (eight columns or more, such as the permission matrix) get a compact style
    // the Markdown table of contents of the language folder (<lang>/README.md), from the H1 and H2 headings
    const L = LANGS[lang];
    const lines = [`# TOP4HoneyChains · ${L.title} (${L.name})`, '', `## ${L.toc}`, ''];
    for (const f of files)
        for (const m of fs.readFileSync(path.join(dir, f), 'utf8').matchAll(/^(#{1,2}) (.+?)\s*\{#([\w-]+)\}\s*$/gm))
            lines.push(`${m[1] === '#' ? '' : '  '}- [${m[2]}](${f}#${m[3]})`);
    fs.writeFileSync(path.join(dir, 'README.md'), lines.join('\n') + '\n');
    const body = md.render(source).replace(/<table>([\s\S]*?)<\/table>/g, (t, inner) =>
        ((inner.split('</thead>')[0].match(/<th/g) || []).length >= 8 ? '<table class="wide">' + inner + '</table>' : t));
    // contents: the H1 and H2 headings with their anchors
    const toc = [...body.matchAll(/<h([12]) id="([\w-]+)">(.*?)<\/h\1>/g)]
        .map(m => `<li class="toc-${m[1]}"><a href="#${m[2]}">${m[3].replace(/<[^>]+>/g, '')}</a></li>`).join('\n');
    const list = items => items.map((x, i) => `<li><a href="#${x.id}">${i + 1}. ${md.renderInline(x.caption).replace(/<\/?a[^>]*>/g, '')}</a></li>`).join('\n');
    return { body, toc, files, shots, present, figureList: list(figures), tableList: list(tables), figures: figures.length, tables: tables.length, problems: problems.length };
}

function font(file) { return pathToFileURL(path.join(fonts, file)).href; }

// Cover logos (docs/brand/):
// - programme/  the programme logo (ICT-AGRI-FOOD ERA-NET Cofund), shown prominently with the acknowledgement line;
// - partners/   the consortium partners' logos, in file-name order, in a strip along the bottom captioned "Consortium";
// - funders/    optional, for the funding agencies once their logos arrive (a second strip captioned "Funding"), and an
//               acknowledgement.<lang>.md or acknowledgement.md that replaces the default acknowledgement line.
// Missing folders simply leave their part out.
const brand = path.join(repo, 'docs', 'brand');
const isImage = f => /\.(svg|png|jpe?g)$/i.test(f);
const images = dir => (fs.existsSync(path.join(brand, dir)) ? fs.readdirSync(path.join(brand, dir)).filter(isImage).sort() : [])
    .map(f => `<img src="${pathToFileURL(path.join(brand, dir, f)).href}" alt="">`);
function coverLogos(lang) {
    const funders = path.join(brand, 'funders');
    const text = fs.existsSync(funders) && [`acknowledgement.${lang}.md`, 'acknowledgement.md'].find(f => fs.existsSync(path.join(funders, f)));
    return {
        programme: images('programme')[0] || '',
        partners: images('partners').join(''),
        agencies: images('funders').join(''),
        acknowledgement: text ? md.renderInline(fs.readFileSync(path.join(funders, text), 'utf8').trim()) : LANGS[lang].acknowledgement,
    };
}

function html(lang, { body, toc, figureList, tableList }) {
    const F = coverLogos(lang);
    const L = LANGS[lang];
    const svg = fs.readFileSync(logo, 'utf8').replace(/<\?xml[^>]*>/, '');
    return `<!doctype html>
<html lang="${lang}">
<head>
<meta charset="utf-8">
<title>TOP4HoneyChains · ${L.title} (${L.name})</title>
<style>
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 700; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-ext-700-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 700; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-700-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 800; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-ext-800-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Bricolage Grotesque'; font-weight: 800; src: url(${font('bricolage-grotesque/bricolage-grotesque-latin-800-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Instrument Sans'; font-weight: 400; src: url(${font('instrument-sans/instrument-sans-latin-ext-400-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Instrument Sans'; font-weight: 400; src: url(${font('instrument-sans/instrument-sans-latin-400-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
@font-face { font-family: 'Instrument Sans'; font-weight: 600; src: url(${font('instrument-sans/instrument-sans-latin-ext-600-normal.woff2')}) format('woff2'); unicode-range: U+0100-024F, U+1E00-1EFF; }
@font-face { font-family: 'Instrument Sans'; font-weight: 600; src: url(${font('instrument-sans/instrument-sans-latin-600-normal.woff2')}) format('woff2'); unicode-range: U+0000-00FF, U+2000-206F, U+20AC, U+2122; }
:root { --foam: #FFFBF0; --cream: #FFF4D6; --honey: #FFB547; --honey-light: #FFE7AE; --heading: #8C5E2E; --ink: #574A40; --muted: #7B6A5C; --line: #F2E3C6; --trust: #0B7A75; --trust-soft: #E8F8F7; }
@page { size: A4; margin: 22mm 18mm 20mm; }
@page :first { margin: 0; }
* { box-sizing: border-box; }
html { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
body { margin: 0; font: 400 10.5pt/1.55 'Instrument Sans', 'Segoe UI', system-ui, sans-serif; color: var(--ink); background: #FFFFFF; }
h1, h2, h3, h4 { font-family: 'Bricolage Grotesque', 'Segoe UI', system-ui, sans-serif; color: var(--heading); line-height: 1.2; break-after: avoid; }
h1 { font-size: 22pt; font-weight: 800; margin: 0 0 14pt; padding-bottom: 8pt; border-bottom: 3px solid var(--honey); break-before: page; }
h2 { font-size: 15pt; font-weight: 700; margin: 20pt 0 8pt; }
h3 { font-size: 12pt; font-weight: 700; margin: 14pt 0 6pt; }
h4 { font-size: 10.5pt; font-weight: 700; margin: 10pt 0 4pt; color: var(--ink); }
p, ul, ol { margin: 0 0 8pt; }
li { margin: 0 0 3pt; }
a { color: var(--trust); text-decoration: none; }
code { font-family: Consolas, 'Cascadia Mono', monospace; font-size: 9pt; background: var(--foam); border: 1px solid var(--line); border-radius: 4px; padding: 0 3px; }
strong { color: #4A3F36; }
table { width: 100%; border-collapse: collapse; margin: 6pt 0 12pt; font-size: 9pt; break-inside: auto; }
th { background: var(--cream); color: var(--heading); font-family: 'Bricolage Grotesque', sans-serif; font-weight: 700; text-align: left; }
th, td { border: 1px solid var(--line); padding: 4pt 6pt; vertical-align: top; }
tr { break-inside: avoid; }
table.wide { font-size: 7.4pt; table-layout: fixed; }
table.wide th, table.wide td { padding: 3pt 3pt; overflow-wrap: anywhere; hyphens: auto; }
table.wide th:first-child, table.wide td:first-child { width: 17%; }
table.wide th:nth-child(2), table.wide td:nth-child(2) { width: 13%; }
tbody tr:nth-child(even) td { background: #FFFDF7; }
blockquote { margin: 8pt 0 10pt; padding: 8pt 12pt; background: var(--foam); border-left: 4px solid var(--honey); border-radius: 0 8px 8px 0; }
blockquote p:last-child { margin: 0; }
figure.shot { margin: 8pt 0 12pt; break-inside: avoid; }
figure.shot img { max-width: 100%; max-height: 235mm; border: 1px solid var(--line); border-radius: 8px; }
figure.shot.phone { text-align: center; }
figure.shot.phone img { max-width: 78mm; }
figure.shot figcaption, p.tbl-caption { font-size: 9pt; color: var(--muted); margin: 3pt 0 0; text-align: left; }
figure.shot.phone figcaption { text-align: center; }
p.tbl-caption { margin: 10pt 0 2pt; break-after: avoid; }
figure.shot figcaption b, p.tbl-caption b { color: var(--heading); }
nav.lists { break-after: page; }
nav.lists h1 { break-before: auto; font-size: 16pt; }
nav.lists ul { list-style: none; padding: 0; font-size: 9pt; }
nav.lists li { margin: 0 0 2pt; }
nav.lists a { color: inherit; }
.cover { height: 297mm; padding: 34mm 24mm 24mm; display: flex; flex-direction: column; background: radial-gradient(70% 55% at 100% 0%, rgba(255, 210, 90, 0.45), transparent 70%), radial-gradient(60% 50% at 0% 100%, rgba(255, 231, 174, 0.7), transparent 70%), linear-gradient(180deg, #FFFFFF, var(--foam)); break-after: page; }
.cover .logo svg { width: 120mm; height: auto; }
.cover .project { margin-top: 26mm; font: 700 13pt 'Bricolage Grotesque', sans-serif; letter-spacing: 0.08em; text-transform: uppercase; color: var(--heading); }
.cover .title { break-before: avoid; margin: 6mm 0 0; font: 800 34pt/1.05 'Bricolage Grotesque', sans-serif; color: #4A3F36; border: 0; padding: 0; }
.cover .lang { margin-top: 6mm; font-size: 14pt; color: var(--ink); }
.cover .draft { display: inline-block; margin-top: 8mm; padding: 3mm 6mm; border-radius: 999px; background: #FFF0E0; color: #A2450B; font-weight: 600; border: 1px solid #F3C9A0; }
.cover .meta { margin-top: 5mm; font-size: 10pt; color: var(--muted); border-top: 1px solid var(--line); padding-top: 4mm; }
.cover .programme { margin-top: 14mm; display: flex; align-items: center; gap: 7mm; }
.cover .programme img { height: 24mm; width: auto; }
.cover .programme p { margin: 0; max-width: 70mm; font-size: 10pt; line-height: 1.4; color: var(--ink); }
.cover .logos { margin-top: auto; }
.cover .logos h2 { margin: 0 0 3mm; font: 700 8.5pt 'Instrument Sans', sans-serif; letter-spacing: 0.12em; text-transform: uppercase; color: var(--muted); text-align: center; }
.cover .strip { display: flex; flex-wrap: wrap; justify-content: center; align-items: center; gap: 4mm 6mm; padding: 4mm 3mm; background: #FFFFFF; border-radius: 4mm; }
.cover .strip img { height: 10mm; width: auto; }
.cover .strip + h2 { margin-top: 4mm; }
nav.toc { break-after: page; }
nav.toc h1 { break-before: auto; }
nav.toc ul { list-style: none; padding: 0; columns: 1; }
nav.toc li.toc-1 { margin-top: 7pt; font-weight: 600; color: var(--heading); }
nav.toc li.toc-2 { margin-left: 14pt; font-size: 9.5pt; }
nav.toc a { color: inherit; }
</style>
</head>
<body>
<section class="cover">
  <div class="logo">${svg}</div>
  <div class="project" lang="en">TOP4HoneyChains · ICT-AGRI-FOOD 2022</div>
  <h1 class="title">TOP4HoneyChains<br>${L.title}</h1>
  <div class="lang">${L.name}</div>
  ${L.draft ? `<div><span class="draft">${L.draft}</span></div>` : ''}
  <div class="programme">${F.programme}<p>${F.acknowledgement}</p></div>
  <div class="logos">
    ${F.partners ? `<h2>${L.consortium}</h2><div class="strip">${F.partners}</div>` : ''}
    ${F.agencies ? `<h2>${L.funding}</h2><div class="strip">${F.agencies}</div>` : ''}
  </div>
  <div class="meta">${edition(lang)}</div>
</section>
<nav class="toc"><h1 id="contents">${L.toc}</h1><ul>
${toc}
</ul></nav>
<nav class="lists"><h1 id="list-of-figures">${L.figures}</h1><ul>
${figureList}
</ul>
<h1 id="list-of-tables">${L.tables}</h1><ul>
${tableList}
</ul></nav>
<main>
${body}
</main>
</body>
</html>`;
}

async function pdf(htmlFile, pdfFile, lang) {
    const { chromium } = await import('playwright-core');
    let browser;
    for (const options of [{ channel: 'chrome' }, { channel: 'msedge' }, {}]) {
        try { browser = await chromium.launch({ headless: true, ...options }); break; } catch { /* try the next browser */ }
    }
    if (!browser) throw new Error('no Chrome, Edge or Chromium found for Playwright');
    try {
        const page = await browser.newPage();
        await page.goto(pathToFileURL(htmlFile).href, { waitUntil: 'load' });
        await page.evaluate(() => document.fonts.ready);
        const L = LANGS[lang];
        await page.pdf({
            path: pdfFile, format: 'A4', printBackground: true, preferCSSPageSize: true, displayHeaderFooter: true,
            headerTemplate: '<span></span>',
            footerTemplate: `<div style="width:100%;font:8px 'Segoe UI',sans-serif;color:#7B6A5C;padding:0 18mm;display:flex;justify-content:space-between">
                <span>TOP4HoneyChains · ${L.title} · ${L.name} · ${edition(lang)}</span><span>${L.page} <span class="pageNumber"></span> / <span class="totalPages"></span></span></div>`,
        });
    } finally {
        await browser.close();
    }
}

function pageCount(file) {
    const buf = fs.readFileSync(file).toString('latin1');
    return (buf.match(/\/Type\s*\/Page[^s]/g) || []).length;
}

const wanted = process.argv.slice(2).length ? process.argv.slice(2) : Object.keys(LANGS);
fs.mkdirSync(dist, { recursive: true });
for (const lang of wanted) {
    const result = render(lang);
    const base = path.join(dist, `TOP4HoneyChains-User-Manual-${lang.toUpperCase()}`);
    fs.writeFileSync(`${base}.html`, html(lang, result));
    try {
        await pdf(`${base}.html`, `${base}.pdf`, lang);
        console.log(`${lang}: ${result.files.length} files, ${result.figures} figures (${result.present} images), ${result.tables} tables, ${result.problems} problems, ${pageCount(`${base}.pdf`)} pages -> ${path.relative(repo, base)}.pdf`);
    } catch (e) {
        console.log(`${lang}: PDF failed (${e.message.split('\n')[0]}); HTML written -> ${path.relative(repo, base)}.html`);
        process.exitCode = 1;
    }
}
