// Builds the TOP4HoneyChains user manual: docs/manual/<lang>/*.md (in file-name order) -> one branded PDF and one HTML
// file per language in docs/manual/dist/. Usage: npm install, then `node build.mjs [lang ...]` (default: all five).
// - Headings carry explicit anchors (`## Sign-in {#sign-in}`) so every language has the same ids.
// - `<!-- SHOT: <id> -->` lines become figures when img/<lang>/<id>.png exists (pass 2), and nothing otherwise.
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
    en: { name: 'English', title: 'User manual', draft: null, toc: 'Contents', version: 'Version', page: 'Page' },
    tr: { name: 'Türkçe', title: 'Kullanım kılavuzu', draft: null, toc: 'İçindekiler', version: 'Sürüm', page: 'Sayfa' },
    es: { name: 'Español', title: 'Manual de usuario', draft: 'Borrador, para revisión de los socios', toc: 'Índice', version: 'Versión', page: 'Página' },
    pl: { name: 'Polski', title: 'Podręcznik użytkownika', draft: 'Wersja robocza do przeglądu przez partnerów', toc: 'Spis treści', version: 'Wersja', page: 'Strona' },
    lv: { name: 'Latviešu', title: 'Lietotāja rokasgrāmata', draft: 'Melnraksts partneru pārskatīšanai', toc: 'Saturs', version: 'Versija', page: 'Lapa' },
};
const version = process.env.MANUAL_DATE || new Date().toISOString().slice(0, 10);

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
    let source = files.map(f => fs.readFileSync(path.join(dir, f), 'utf8')).join('\n\n');
    // screenshots: a figure when the image exists, otherwise nothing
    let shots = 0, present = 0;
    source = source.replace(/^<!--\s*SHOT:\s*([\w.-]+)\s*-->\s*$/gm, (_, id) => {
        shots++;
        const img = path.join(here, 'img', lang, `${id}.png`);
        if (!fs.existsSync(img)) return '';
        present++;
        return `<figure class="shot"><img src="${pathToFileURL(img).href}" alt=""></figure>`;
    });
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
    return { body, toc, files, shots, present };
}

function font(file) { return pathToFileURL(path.join(fonts, file)).href; }

function html(lang, { body, toc }) {
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
figure.shot img { max-width: 100%; max-height: 150mm; border: 1px solid var(--line); border-radius: 8px; }
.cover { height: 297mm; padding: 34mm 24mm 24mm; display: flex; flex-direction: column; background: radial-gradient(70% 55% at 100% 0%, rgba(255, 210, 90, 0.45), transparent 70%), radial-gradient(60% 50% at 0% 100%, rgba(255, 231, 174, 0.7), transparent 70%), linear-gradient(180deg, #FFFFFF, var(--foam)); break-after: page; }
.cover .logo svg { width: 120mm; height: auto; }
.cover .project { margin-top: 26mm; font: 700 13pt 'Bricolage Grotesque', sans-serif; letter-spacing: 0.08em; text-transform: uppercase; color: var(--heading); }
.cover .title { margin: 6mm 0 0; font: 800 34pt/1.05 'Bricolage Grotesque', sans-serif; color: #4A3F36; border: 0; padding: 0; }
.cover .lang { margin-top: 6mm; font-size: 14pt; color: var(--ink); }
.cover .draft { display: inline-block; margin-top: 8mm; padding: 3mm 6mm; border-radius: 999px; background: #FFF0E0; color: #A2450B; font-weight: 600; border: 1px solid #F3C9A0; }
.cover .meta { margin-top: auto; font-size: 10pt; color: var(--muted); border-top: 1px solid var(--line); padding-top: 5mm; }
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
  <div class="project">TÜBİTAK TOP4HoneyChains</div>
  <h1 class="title">TOP4HoneyChains<br>${L.title}</h1>
  <div class="lang">${L.name}</div>
  ${L.draft ? `<div><span class="draft">${L.draft}</span></div>` : ''}
  <div class="meta">${L.version} ${version}</div>
</section>
<nav class="toc"><h1 id="contents">${L.toc}</h1><ul>
${toc}
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
                <span>TOP4HoneyChains · ${L.title} · ${L.name} · ${version}</span><span>${L.page} <span class="pageNumber"></span> / <span class="totalPages"></span></span></div>`,
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
        console.log(`${lang}: ${result.files.length} files, ${result.shots} shot placeholders (${result.present} images), ${pageCount(`${base}.pdf`)} pages -> ${path.relative(repo, base)}.pdf`);
    } catch (e) {
        console.log(`${lang}: PDF failed (${e.message.split('\n')[0]}); HTML written -> ${path.relative(repo, base)}.html`);
        process.exitCode = 1;
    }
}
