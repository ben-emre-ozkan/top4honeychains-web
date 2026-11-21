#!/usr/bin/env node
// TOP4HoneyChains user manual builder.
// content/<lang>/manual.md (Markdown) -> dist/TOP4HoneyChains_User_Manual_<LANG>.docx -> .pdf
// The script adds the cover with the partners and supporters board, the contents table with page numbers,
// chapter numbers, numbered captions of figures and tables, cross-references, page header and footer,
// and the closing "Partners and supporters" page.
//
// usage:  node build.mjs <lang ...|all> [--no-pdf] [--engine=word|libreoffice]
// The Markdown conventions are described in ../BRIEF.md (section "Writing format").

import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { spawnSync } from 'node:child_process';
import { marked } from 'marked';
import JSZip from 'jszip';
import {
  AlignmentType, Bookmark, BorderStyle, Document, ExternalHyperlink, Footer, Header, HeadingLevel, HorizontalPositionRelativeFrom,
  ImageRun, InternalHyperlink, LevelFormat, LineRuleType, Packer, PageBreak, PageNumber, Paragraph, SectionType, ShadingType, Tab, Table, TableCell, TableLayoutType,
  TableOfContents, TableRow, TabStopType, TextRun, TextWrappingType, VerticalAlign, VerticalPositionRelativeFrom, WidthType,
} from 'docx';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(HERE, '..');
const LANGS = ['en', 'tr', 'es', 'pl', 'lv'];
const BRAND = 'TOP4HoneyChains';

/* ---------- page geometry (A4) ---------- */
const tw = cm => Math.round(cm / 2.54 * 1440);          // centimetres -> twips
const px = cm => Math.round(cm / 2.54 * 96);            // centimetres -> pixels at 96 dpi (unit of image sizes in docx)
const emu = cm => Math.round(cm * 360000);              // centimetres -> EMU (unit of floating positions)
const PAGE = { w: 11906, h: 16838 };
const BODY = { top: tw(2.7), bottom: tw(2.3), side: 1440, header: tw(1.1), footer: tw(1.0) };
const BODY_W = PAGE.w - 2 * BODY.side;                  // 9026 twips = 15.92 cm (the contents table of the docx library assumes this width)
const BODY_W_CM = BODY_W / 1440 * 2.54;
const COVER = { side: 1.8, top: 1.9, bottom: 1.6 };     // cm
const MAX_FIGURE_H_CM = 17;
const LIST_STEP = 425;                                  // indent of one list level, twips (0.75 cm)

/* ---------- look ---------- */
const FONT = 'Calibri', SERIF = 'Cambria', MONO = 'Consolas';
const C = { text: '2B2B2B', brown: '4F3F1B', gold: 'C98F1F', line: 'D6A02C', label: '9A6A0E', grey: '6F6A5E', rule: 'E3D9BF', zebra: 'FBF6E9', code: 'F4EFE1' };
const CALLOUT = {
  note: { bar: 'D6A02C', fill: 'FFF7E3', label: '9A6A0E' },
  tip: { bar: '2E9E5B', fill: 'EEF8F1', label: '1F7A43' },
  important: { bar: '1B2748', fill: 'EEF1F7', label: '1B2748' },
  warning: { bar: 'C4491D', fill: 'FDEFE8', label: 'A63A14' },
  quote: { bar: 'BDB6A3', fill: 'F7F5F0', label: C.grey },
};
const NONE = { style: BorderStyle.NONE, size: 0, color: 'FFFFFF' };

/* ---------- command line ---------- */
const args = process.argv.slice(2);
const flags = args.filter(a => a.startsWith('--'));
let langs = args.filter(a => !a.startsWith('--'));
if (!langs.length) { console.log('usage: node build.mjs <lang ...|all> [--no-pdf] [--engine=word|libreoffice]'); process.exit(1); }
if (langs.includes('all')) langs = LANGS.filter(l => fs.existsSync(path.join(ROOT, 'content', l, 'manual.md')));
const WANT_PDF = !flags.includes('--no-pdf');
const ENGINE = (flags.find(f => f.startsWith('--engine=')) || '').split('=')[1] || '';

const STRINGS = JSON.parse(fs.readFileSync(path.join(HERE, 'strings.json'), 'utf8'));
const DATA = JSON.parse(fs.readFileSync(path.join(ROOT, 'assets', 'institutions.json'), 'utf8'));
const asset = (...p) => fs.readFileSync(path.join(ROOT, 'assets', ...p));
const LOGO = asset('logos', 'top4honeychains_logo.png'), MARK = asset('logos', 'top4honeychains_mark.png');

/* ---------- small helpers ---------- */
function imageSize(buf) {
  if (buf.length > 24 && buf.readUInt32BE(0) === 0x89504e47) return { type: 'png', w: buf.readUInt32BE(16), h: buf.readUInt32BE(20) };
  if (buf[0] === 0xff && buf[1] === 0xd8) {
    let o = 2;
    while (o + 9 < buf.length) {
      if (buf[o] !== 0xff) { o++; continue; }
      const m = buf[o + 1];
      if (m === 0xff) { o++; continue; }
      if (m >= 0xc0 && m <= 0xcf && m !== 0xc4 && m !== 0xc8 && m !== 0xcc) return { type: 'jpg', h: buf.readUInt16BE(o + 5), w: buf.readUInt16BE(o + 7) };
      o += 2 + buf.readUInt16BE(o + 2);
    }
  }
  throw new Error('unsupported image format (use PNG or JPEG)');
}
const ENT = { '&amp;': '&', '&lt;': '<', '&gt;': '>', '&quot;': '"', '&#39;': "'", '&nbsp;': '\u00a0' };
const clean = s => String(s ?? '').replace(/&(amp|lt|gt|quot|#39|nbsp);/g, m => ENT[m]).replace(/[ \t]*\n[ \t]*/g, ' ');
const plain = tokens => (tokens || []).map(t => (t.tokens ? plain(t.tokens) : t.type === 'br' ? ' ' : clean(t.text ?? ''))).join('');
const norm = s => s.normalize('NFKC').replace(/\s+/g, '').toLowerCase();

/* Trailing attributes of a heading, a caption or a picture's text: {#id}, {-} or {.unnumbered}, in any order. */
function attrs(s) {
  const str = String(s ?? ''), m = str.match(/\s*\{([^{}]*)\}\s*$/);
  if (!m || !/^\s*(?:(?:#[\w:.-]+|-|\.unnumbered)\s*)+$/.test(m[1])) return { text: str, id: null, unnumbered: false };
  return { text: str.slice(0, m.index), id: (m[1].match(/#([\w:.-]+)/) || [])[1] || null, unnumbered: /(^|\s)(-|\.unnumbered)(?=\s|$)/.test(m[1]) };
}
/* Word bookmark names: a letter first, then letters, digits and underscores, at most 40 characters. */
const bm = id => { let b = String(id).replace(/[^A-Za-z0-9_]/g, '_'); if (!/^[A-Za-z]/.test(b)) b = `a_${b}`; return b.slice(0, 40); };
const isCaptionPara = (t, word) => !!t && (t.type === 'paragraph' || t.type === 'text') && (t.text || '').startsWith(`${word}: `);
const onlyFigure = b => !!b && (b.type === 'paragraph' || b.type === 'text') && (b.tokens || []).some(k => k.type === 'image') && !plain((b.tokens || []).filter(k => k.type !== 'image')).trim();

/* Captions written on their own line: "Table: ..." just above a table (or just below it), "Figure: ..." just below a picture.
   They are attached to the table or picture, and every captioned table and figure is numbered in reading order,
   so that a link such as [Table](#tbl-roles) can print "Table 3". Heading ids are collected for links such as [Ground rules](#ground-rules). */
function prepare(tokens, st) {
  const real = tokens.filter(t => t.type !== 'space');
  real.forEach((t, i) => {
    if (t.type === 'list') t.items.forEach(it => prepare(it.tokens, st));
    if (t.type === 'blockquote') prepare(t.tokens, st);
    for (const word of ['Table', 'Figure']) {
      if (!isCaptionPara(t, word)) continue;
      const a = attrs(t.text.slice(word.length + 1).trim()), cap = { text: a.text.trim(), id: a.id }, prev = real[i - 1], next = real[i + 1];
      if (word === 'Table') {
        if (next && next.type === 'table' && !next.caption) next.caption = cap;
        else if (prev && prev.type === 'table' && !prev.caption) prev.caption = cap;
        else { st.warn(`"Table: ${cap.text}" is not right above or below a table`); continue; }
      } else {
        const imgs = onlyFigure(prev) ? prev.tokens.filter(k => k.type === 'image') : [];
        if (imgs.length === 1) imgs[0].caption = cap;
        else { st.warn(`"Figure: ${cap.text}" does not follow a single picture on its own line`); continue; }
      }
      t.type = 'space';                                                   // the caption line itself is not printed again
    }
  });
}
function number(tokens, st) {
  for (const t of tokens) {
    if (t.type === 'heading') { const a = attrs(t.text); if (a.id) st.labels.set(a.id, { kind: 'section' }); }
    else if (t.type === 'table') { if (t.caption) { t.n = ++st.tables; if (t.caption.id) st.labels.set(t.caption.id, { kind: 'table', n: t.n }); } }
    else if (t.type === 'list') t.items.forEach(it => number(it.tokens, st));
    else if (t.type === 'blockquote') number(t.tokens, st);
    else if (t.type === 'paragraph' || t.type === 'text') {
      for (const k of t.tokens || []) {
        if (k.type !== 'image') continue;
        if (!k.caption) { const a = attrs(clean(k.text || '').trim()); k.caption = { text: a.text.trim(), id: a.id }; }
        if (k.caption.text) { k.n = ++st.figures; if (k.caption.id) st.labels.set(k.caption.id, { kind: 'figure', n: k.n }); }
      }
    }
  }
}

function readContent(lang) {
  const file = path.join(ROOT, 'content', lang, 'manual.md');
  if (!fs.existsSync(file)) throw new Error(`no content for "${lang}": ${path.relative(ROOT, file)} is missing`);
  const full = fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '').replace(/\r\n/g, '\n');
  const dashes = []; full.split('\n').forEach((l, i) => { if (l.includes('\u2014')) dashes.push(i + 1); });
  if (dashes.length) throw new Error(`${path.relative(ROOT, file)}: em-dash on line ${dashes.join(', ')}. The manuals do not use em-dashes: rewrite the sentence (comma, colon or two sentences).`);
  const meta = {}; let src = full;
  const m = full.match(/^---\n([\s\S]*?)\n---\n/);
  if (m) {
    for (const line of m[1].split('\n')) { const k = line.match(/^([A-Za-z_]+)\s*:\s*(.*)$/); if (k) meta[k[1]] = k[2].trim().replace(/^["']|["']$/g, ''); }
    src = full.slice(m[0].length);
  }
  return { file, dir: path.dirname(file), meta, src };
}

/* ---------- Markdown -> document blocks ---------- */
function makeRenderer(S, st) {
  const T = (s, o = {}) => new TextRun({
    text: clean(s), bold: o.bold, italics: o.italics, strike: o.strike, size: o.code ? Math.max(16, (o.size ?? 22) - 2) : o.size,
    color: o.link || o.xref ? C.label : o.color, underline: o.link ? {} : undefined, font: o.code ? MONO : undefined,
    shading: o.code ? { type: ShadingType.CLEAR, fill: C.code } : undefined,
  });

  function runs(tokens, o = {}) {
    const out = [];
    for (const t of tokens || []) {
      switch (t.type) {
        case 'strong': out.push(...runs(t.tokens, { ...o, bold: true })); break;
        case 'em': out.push(...runs(t.tokens, { ...o, italics: true })); break;
        case 'del': out.push(...runs(t.tokens, { ...o, strike: true })); break;
        case 'codespan': out.push(T(t.text, { ...o, code: true })); break;
        case 'br': out.push(new TextRun({ break: 1 })); break;
        case 'link': {
          if (!t.href.startsWith('#')) { out.push(new ExternalHyperlink({ link: t.href, children: runs(t.tokens, { ...o, link: true }) })); break; }
          const id = decodeURI(t.href.slice(1)), target = st.labels.get(id);
          if (!target) { st.warn(`link to "#${id}": no heading, table or figure has this id`); out.push(...runs(t.tokens, o)); break; }
          if (target.kind === 'section') { out.push(new InternalHyperlink({ anchor: bm(id), children: runs(t.tokens, { ...o, xref: true }) })); break; }
          const text = plain(t.tokens).trim(), n = String(target.n);           // [Table](#tbl-x) -> "Table 3"; [{n}. tabula](#tbl-x) -> "3. tabula"
          out.push(new InternalHyperlink({ anchor: bm(id), children: [T(text.includes('{n}') ? text.replace('{n}', n) : text ? `${text} ${n}` : n, { ...o, xref: true })] }));
          break;
        }
        case 'html': break;                                             // comments and raw HTML stay out of the document
        case 'image': st.warn(`image inside a line of text is not supported: ${t.href}`); break;
        default: if (t.tokens && t.tokens.length) out.push(...runs(t.tokens, o)); else if (t.text) out.push(T(t.text, o));
      }
    }
    return out;
  }

  /* A paragraph of text. Images in it become figures of their own, in place. Two or three images alone on one line stand side by side. */
  function paragraph(t, ctx) {
    const imgs = (t.tokens || []).filter(k => k.type === 'image');
    if (imgs.length > 1 && !plain((t.tokens || []).filter(k => k.type !== 'image')).trim()) return figureRow(imgs, ctx);
    const segs = []; let cur = [];
    for (const k of t.tokens || []) { if (k.type === 'image') { if (cur.length) segs.push(cur); segs.push(k); cur = []; } else cur.push(k); }
    if (cur.length) segs.push(cur);
    const out = []; let numbering = ctx.numbering;
    for (const [i, s] of segs.entries()) {
      if (!Array.isArray(s)) { out.push(...figure(s, ctx)); continue; }
      if (!plain(s).trim()) continue;
      const first = s[0], last = s[s.length - 1];
      if (first.type === 'text' && !first.tokens) first.text = first.text.replace(/^\s+/, '');
      if (last.type === 'text' && !last.tokens) last.text = last.text.replace(/\s+$/, '');
      out.push(new Paragraph({
        numbering, indent: !numbering && ctx.left ? { left: ctx.left } : undefined, alignment: ctx.align,
        keepNext: (i === segs.length - 1 ? ctx.beforeFigure : !Array.isArray(segs[i + 1])) || undefined,
        spacing: ctx.tight ? { after: 0 } : ctx.level ? { after: 80 } : undefined, children: runs(s, ctx.run),
      }));
      numbering = undefined;
    }
    return out;
  }

  function figure(img, ctx) {
    const rel = decodeURI(img.href), file = path.resolve(st.dir, rel), indent = ctx.left ? { left: ctx.left } : undefined;
    if (!fs.existsSync(file)) { st.missing.push(rel); return [new Paragraph({ indent, children: [new TextRun({ text: `[missing image: ${rel}]`, bold: true, color: 'C00000' })] })]; }
    const buf = fs.readFileSync(file), { type, w, h } = imageSize(buf), avail = ctx.width / 1440 * 2.54;
    const hint = (img.title || '').match(/width\s*=\s*([\d.]+)\s*(%|cm)/i);
    let wc = hint ? (hint[2] === '%' ? avail * parseFloat(hint[1]) / 100 : parseFloat(hint[1])) : (w / st.shotScale) * (BODY_W_CM / st.viewport);
    wc = Math.min(wc, avail); let hc = wc * h / w;
    if (hc > MAX_FIGURE_H_CM) { hc = MAX_FIGURE_H_CM; wc = hc * w / h; }
    if (wc < 4 && !hint) st.warn(`figure ${rel} comes out only ${wc.toFixed(1)} cm wide: check screenshot_scale and viewport_width in the front matter`);
    const caption = img.caption?.text || '', n = img.n;
    if (!caption) st.warn(`picture ${rel} has no caption: every figure needs one (its text in ![...], or a "Figure: ..." line below it)`);
    const out = [new Paragraph({
      alignment: AlignmentType.CENTER, indent, keepNext: !!caption, spacing: { before: 140, after: caption ? 70 : 220 },
      children: [new ImageRun({
        type, data: buf, transformation: { width: px(wc), height: px(hc) }, altText: { name: `figure-${st.images++}`, description: caption || rel, title: caption || rel },
        outline: { type: 'solidFill', solidFillType: 'rgb', value: 'CFC8B4', width: 9525 },
      })],
    })];
    if (caption) {
      const [pre, post] = S.figure.replace('{n}', n).split('{text}'), label = new TextRun({ text: pre, bold: true, italics: false });
      out.push(new Paragraph({ style: 'Caption', indent, children: [img.caption.id ? new Bookmark({ id: bm(img.caption.id), children: [label] }) : label, ...runs(marked.Lexer.lexInline(caption)), ...(post ? [new TextRun({ text: post })] : [])] }));
    }
    return out;
  }

  function figureRow(imgs, ctx) {
    const n = imgs.length, col = Math.floor(ctx.width / n), none = { top: NONE, bottom: NONE, left: NONE, right: NONE };
    return [
      new Table({
        width: { size: col * n, type: WidthType.DXA }, columnWidths: Array(n).fill(col), layout: TableLayoutType.FIXED,
        indent: ctx.left ? { size: ctx.left, type: WidthType.DXA } : undefined, borders: { ...none, insideHorizontal: NONE, insideVertical: NONE },
        rows: [new TableRow({ cantSplit: true, children: imgs.map(img => new TableCell({ width: { size: col, type: WidthType.DXA }, borders: none, margins: { top: 0, bottom: 0, left: 80, right: 80 }, verticalAlign: VerticalAlign.BOTTOM, children: figure(img, { left: 0, width: col - 160 }) })) })],
      }),
      gap(40),
    ];
  }

  function list(t, ctx) {
    const level = Math.min(ctx.level ?? 0, 2), kind = t.ordered ? 'ordered' : 'bullet', depth = ctx[kind] ?? 0, out = [];
    const reference = t.ordered ? (depth ? 'substeps' : 'steps') : (depth ? 'dashes' : 'bullets'), instance = t.ordered ? ++st.lists : undefined;
    const base = ctx.base ?? ctx.left ?? 0;
    const inner = { ...ctx, [kind]: depth + 1, base, level: level + 1, left: base + LIST_STEP * (level + 1), width: (ctx.baseWidth ?? ctx.width) - LIST_STEP * (level + 1), baseWidth: ctx.baseWidth ?? ctx.width, numbering: undefined };
    for (const item of t.items) {
      const parts = item.tokens.filter(b => b.type !== 'space');
      parts.forEach((b, i) => {
        const beforeFigure = onlyFigure(parts[i + 1]);                    // a step stays on the page of its picture
        if (i === 0 && (b.type === 'text' || b.type === 'paragraph')) out.push(...paragraph(b, { ...inner, beforeFigure, numbering: { reference, level, instance } }));
        else if (b.type === 'text' || b.type === 'paragraph') out.push(...paragraph(b, { ...inner, beforeFigure }));
        else out.push(...blocks([b], inner));
      });
    }
    return out;
  }

  /* A thin empty line after a table or a box. Remembered, so that one standing right before a new chapter can be dropped (it could spill onto an empty page). */
  const gap = (h = 120) => { const p = new Paragraph({ spacing: { before: 0, after: 0, line: h, lineRule: LineRuleType.EXACT }, children: [] }); st.gaps.add(p); return p; };

  function callout(t, ctx) {
    const inner = t.tokens.filter(k => k.type !== 'space'), first = inner[0];
    let kind = 'quote';
    if (first && first.type === 'paragraph' && first.tokens.length && first.tokens[0].type === 'text') {
      const m = first.tokens[0].text.match(/^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*/i);
      if (m) {
        kind = m[1].toLowerCase() === 'caution' ? 'warning' : m[1].toLowerCase();
        first.tokens[0].text = first.tokens[0].text.slice(m[0].length);
        if (!first.tokens[0].text) first.tokens.shift();
        if (first.tokens[0] && first.tokens[0].type === 'br') first.tokens.shift();
      }
    }
    const k = CALLOUT[kind], width = ctx.width, cellCtx = { left: 0, width: width - 360, level: 0, baseWidth: width - 360 };
    const children = [];
    inner.forEach((b, i) => {
      if (i === 0 && b.type === 'paragraph' && kind !== 'quote') {
        children.push(new Paragraph({ spacing: { after: 80 }, children: [new TextRun({ text: S[kind] + '  ', bold: true, color: k.label }), ...runs(b.tokens)] }));
      } else children.push(...blocks([b], cellCtx));
    });
    if (!children.length) children.push(new Paragraph({ children: [] }));
    return [
      new Table({
        width: { size: width, type: WidthType.DXA }, columnWidths: [width], layout: TableLayoutType.FIXED,
        indent: ctx.left ? { size: ctx.left, type: WidthType.DXA } : undefined,
        borders: { top: NONE, bottom: NONE, left: NONE, right: NONE, insideHorizontal: NONE, insideVertical: NONE },
        rows: [new TableRow({
          cantSplit: true,
          children: [new TableCell({
            width: { size: width, type: WidthType.DXA }, shading: { type: ShadingType.CLEAR, fill: k.fill },
            margins: { top: 110, bottom: 40, left: 200, right: 160 },
            borders: { top: NONE, bottom: NONE, right: NONE, left: { style: BorderStyle.SINGLE, size: 24, color: k.bar } },
            children,
          })],
        })],
      }),
      gap(),
    ];
  }

  function table(t, ctx) {
    const n = t.header.length, len = c => Math.min(42, Math.max(6, plain(c.tokens).length));
    const size = n >= 8 ? 15 : n >= 6 ? 18 : 20, pad = n >= 8 ? 45 : 110;              // wide tables (for example a rights matrix) use smaller type
    const weight = t.header.map((hd, i) => Math.max(len(hd), ...t.rows.map(r => (r[i] ? len(r[i]) : 0))));
    const sum = weight.reduce((a, b) => a + b, 0);
    let widths = weight.map(wt => Math.max(Math.round(ctx.width * Math.min(0.12, 0.6 / n)), Math.round(ctx.width * wt / sum)));
    const scale = ctx.width / widths.reduce((a, b) => a + b, 0); widths = widths.map(x => Math.floor(x * scale));
    widths[n - 1] += ctx.width - widths.reduce((a, b) => a + b, 0);
    const edge = { style: BorderStyle.SINGLE, size: 4, color: C.rule };
    const align = i => ({ center: AlignmentType.CENTER, right: AlignmentType.RIGHT }[t.align?.[i]] ?? AlignmentType.LEFT);
    const cell = (c, i, o) => new TableCell({
      width: { size: widths[i], type: WidthType.DXA }, verticalAlign: VerticalAlign.CENTER, margins: { top: n >= 8 ? 40 : 70, bottom: n >= 8 ? 40 : 70, left: pad, right: pad },
      shading: o.fill ? { type: ShadingType.CLEAR, fill: o.fill } : undefined, borders: { top: edge, bottom: edge, left: edge, right: edge },
      children: [new Paragraph({ alignment: align(i), spacing: { after: 0, line: n >= 8 ? 220 : 260, lineRule: LineRuleType.AUTO }, children: runs(c.tokens, { size, bold: o.head, color: o.head ? 'FFFFFF' : undefined }) })],
    });
    const head = [];
    if (t.caption && t.caption.text) {
      const [pre, post] = S.table.replace('{n}', t.n).split('{text}'), label = new TextRun({ text: pre, bold: true, italics: false });
      head.push(new Paragraph({ style: 'TableCaption', keepNext: true, indent: ctx.left ? { left: ctx.left } : undefined, children: [t.caption.id ? new Bookmark({ id: bm(t.caption.id), children: [label] }) : label, ...runs(marked.Lexer.lexInline(t.caption.text)), ...(post ? [new TextRun({ text: post })] : [])] }));
    } else st.warn(`a table has no caption: write "Table: ..." on the line above it (table starting "${t.header.slice(0, 3).map(c => plain(c.tokens)).join(' | ')}")`);
    return [
      ...head,
      new Table({
        width: { size: ctx.width, type: WidthType.DXA }, columnWidths: widths, layout: TableLayoutType.FIXED,
        indent: ctx.left ? { size: ctx.left, type: WidthType.DXA } : undefined,
        rows: [
          new TableRow({ tableHeader: true, cantSplit: true, children: t.header.map((c, i) => cell(c, i, { head: true, fill: C.brown })) }),
          ...t.rows.map((r, j) => new TableRow({ cantSplit: true, children: t.header.map((_, i) => cell(r[i] || { tokens: [] }, i, { fill: j % 2 ? C.zebra : undefined })) })),
        ],
      }),
      gap(260),
    ];
  }

  function heading(t) {
    const a = attrs(t.text), unnumbered = a.unnumbered, tokens = marked.Lexer.lexInline(a.text.trim()), title = plain(tokens).trim();
    if (t.depth === 1) { st.chapterNumbered = !unnumbered; if (!unnumbered) { st.h1++; st.h2 = 0; } }
    const num = t.depth === 1 ? (unnumbered ? '' : String(st.h1)) : t.depth === 2 && st.chapterNumbered && !unnumbered ? `${st.h1}.${++st.h2}` : '';
    if (t.depth > 2) return [new Paragraph({ heading: t.depth === 3 ? HeadingLevel.HEADING_3 : HeadingLevel.HEADING_4, children: a.id ? [new Bookmark({ id: bm(a.id), children: runs(tokens) })] : runs(tokens) })];
    const id = a.id ? bm(a.id) : `_Toc${String(st.entries.length + 1).padStart(6, '0')}`;
    st.entries.push({ level: t.depth, title: num ? `${num}  ${title}` : title, id });
    const p = new Paragraph({
      heading: t.depth === 1 ? HeadingLevel.HEADING_1 : HeadingLevel.HEADING_2, pageBreakBefore: t.depth === 1,
      children: [new Bookmark({ id, children: [...(num ? [new TextRun({ text: `${num}  `, color: C.gold })] : []), ...runs(tokens)] })],
    });
    if (t.depth === 1) st.chapters.add(p);
    return [p];
  }

  function blocks(tokens, ctx) {
    const out = [], real = tokens.filter(t => t.type !== 'space');
    for (const [i, t] of real.entries()) {
      switch (t.type) {
        case 'hr': case 'def': break;
        case 'heading': out.push(...heading(t)); break;
        case 'paragraph': case 'text': out.push(...paragraph(t, { ...ctx, beforeFigure: onlyFigure(real[i + 1]) })); break;   // a sentence stays with the picture it introduces
        case 'list': out.push(...list(t, ctx)); break;
        case 'blockquote': out.push(...callout(t, ctx)); break;
        case 'table': out.push(...table(t, ctx)); break;
        case 'code': for (const line of t.text.split('\n')) out.push(new Paragraph({ indent: ctx.left ? { left: ctx.left } : undefined, spacing: { after: 0 }, shading: { type: ShadingType.CLEAR, fill: C.code }, children: [new TextRun({ text: line || ' ', font: MONO, size: 19 })] })); out.push(gap()); break;
        case 'html': if (/<!--\s*pagebreak\s*-->/i.test(t.text)) out.push(new Paragraph({ children: [new PageBreak()] })); break;
        default: st.warn(`Markdown element "${t.type}" is not supported and was skipped`);
      }
    }
    return out;
  }
  return { blocks, heading };
}

/* ---------- fixed pages ---------- */
function coverPage(S, meta, lang) {
  const title = meta.title || S.manual_title, subtitle = meta.subtitle || S.subtitle;
  const given = new Date(meta.date), date = meta.date && !Number.isNaN(given.getTime()) ? given : new Date();
  const when = new Intl.DateTimeFormat(S.locale, { month: 'long', year: 'numeric' }).format(date);
  const titleSize = 2 * Math.min(44, Math.floor(482 / (0.6 * title.length)));        // long titles step down so they stay on one line
  const board = asset('board', `board_${lang}.jpg`), bs = imageSize(board);
  const bw = 21 - 2 * COVER.side, bh = bw * bs.h / bs.w;
  const names = [...DATA.partners.map(p => p.name[lang]), DATA.text.eu_name[lang], ...DATA.funders.filter(f => f.full_name).map(f => f.full_name)].join('; ');
  return [
    new Paragraph({ spacing: { after: 0 }, children: [new ImageRun({ type: 'png', data: LOGO, transformation: { width: px(8.8), height: px(8.8 * 544 / 2000) }, altText: { name: 'logo', description: `${BRAND} logo`, title: BRAND } })] }),
    new Paragraph({ spacing: { before: 1300, after: 0, line: 240, lineRule: LineRuleType.AUTO }, children: [new TextRun({ text: title, font: SERIF, bold: true, size: titleSize, color: C.brown })] }),
    new Paragraph({
      spacing: { before: 120, after: 260 }, border: { bottom: { style: BorderStyle.SINGLE, size: 12, color: C.line, space: 14 } },
      children: [new TextRun({ text: subtitle, size: 40, color: C.label })],
    }),
    new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: `${S.version} ${meta.version || '1.0'}  ·  ${when}`, size: 24, color: C.grey })] }),
    new Paragraph({
      spacing: { after: 0 },
      children: [
        new ExternalHyperlink({ link: DATA.project.platform_url, children: [new TextRun({ text: DATA.project.platform_url.replace(/^https?:\/\//, ''), size: 24, bold: true, color: C.label })] }),
        new ImageRun({
          type: 'jpg', data: board, transformation: { width: px(bw), height: px(bh) },
          altText: { name: 'partners-board', description: S.cover_alt.replace('{list}', names), title: S.partners_title },
          floating: {
            horizontalPosition: { relative: HorizontalPositionRelativeFrom.PAGE, offset: emu(COVER.side) },
            verticalPosition: { relative: VerticalPositionRelativeFrom.PAGE, offset: emu(29.7 - COVER.bottom - bh) },
            wrap: { type: TextWrappingType.NONE }, allowOverlap: true,
          },
        }),
      ],
    }),
  ];
}

function partnersPage(S, lang, R) {
  const edge = { style: BorderStyle.SINGLE, size: 4, color: C.rule }, all = { top: edge, bottom: edge, left: edge, right: edge }, none = { top: NONE, bottom: NONE, left: NONE, right: NONE };
  const flag = c => new ImageRun({ type: 'png', data: asset('flags', `${c}.png`), transformation: { width: px(0.44 * ({ lv: 2, pl: 1.6, ar: 1.6 }[c] ?? 1.5)), height: px(0.44) }, altText: { name: `flag-${c}`, description: '', title: '' }, outline: { type: 'solidFill', solidFillType: 'rgb', value: 'BDB6A3', width: 6350 } });
  const w1 = 2700, w2 = BODY_W - w1, m = { top: 80, bottom: 80, left: 120, right: 120 };
  const countryCell = (c, borders) => new TableCell({ width: { size: w1, type: WidthType.DXA }, margins: m, borders, verticalAlign: VerticalAlign.CENTER, children: [new Paragraph({ spacing: { after: 0 }, children: [flag(c), new TextRun({ text: `  ${DATA.countries[c].name[lang]}`, bold: true })] })] });
  const head = (text, w) => new TableCell({ width: { size: w, type: WidthType.DXA }, margins: m, borders: all, shading: { type: ShadingType.CLEAR, fill: C.brown }, children: [new Paragraph({ spacing: { after: 0 }, children: [new TextRun({ text, bold: true, color: 'FFFFFF', size: 20 })] })] });
  const partnerRows = DATA.partner_order_by_country.map((c, j) => new TableRow({
    cantSplit: true,
    children: [countryCell(c, all), new TableCell({ width: { size: w2, type: WidthType.DXA }, margins: m, borders: all, children: DATA.partners.filter(p => p.country === c).map(p => new Paragraph({ spacing: { after: 20 }, children: [new TextRun({ text: p.name[lang] }), ...(p.full_name && p.full_name !== p.name[lang] ? [new TextRun({ text: ` (${p.full_name})`, color: C.grey })] : [])] })) })],
  }));
  const euW = 2100;
  return [
    ...R.heading({ depth: 1, text: `${S.partners_title} {-}` }),
    new Paragraph({ children: [new TextRun({ text: S.partners_intro })] }),
    new Table({ width: { size: BODY_W, type: WidthType.DXA }, columnWidths: [w1, w2], layout: TableLayoutType.FIXED, rows: [new TableRow({ tableHeader: true, children: [head(S.country, w1), head(S.partners, w2)] }), ...partnerRows] }),
    new Paragraph({ heading: HeadingLevel.HEADING_3, spacing: { before: 360, after: 140 }, children: [new TextRun({ text: S.supported_by })] }),
    new Table({
      width: { size: BODY_W, type: WidthType.DXA }, columnWidths: [euW, BODY_W - euW], layout: TableLayoutType.FIXED,
      borders: { ...none, insideHorizontal: NONE, insideVertical: NONE },
      rows: [new TableRow({
        cantSplit: true,
        children: [
          new TableCell({ width: { size: euW, type: WidthType.DXA }, borders: none, margins: { top: 40, bottom: 40, left: 0, right: 160 }, verticalAlign: VerticalAlign.CENTER, children: [new Paragraph({ spacing: { after: 0 }, children: [new ImageRun({ type: 'png', data: asset('flags', 'eu.png'), transformation: { width: px(3.0), height: px(2.0) }, altText: { name: 'eu-emblem', description: DATA.text.eu_name[lang], title: DATA.text.eu_name[lang] } })] })] }),
          new TableCell({
            width: { size: BODY_W - euW, type: WidthType.DXA }, borders: none, margins: { top: 40, bottom: 40, left: 120, right: 0 }, verticalAlign: VerticalAlign.CENTER,
            children: [
              new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: DATA.text.eu_name[lang], bold: true, size: 26, color: C.brown })] }),
              new Paragraph({ spacing: { after: 0 }, children: [new TextRun({ text: DATA.text.funding_statement[lang] })] }),
            ],
          }),
        ],
      })],
    }),
    new Paragraph({ spacing: { before: 280, after: 120 }, children: [new TextRun({ text: S.national_funders, bold: true, color: C.brown })] }),
    new Table({
      width: { size: BODY_W, type: WidthType.DXA }, columnWidths: [w1, w2], layout: TableLayoutType.FIXED,
      rows: DATA.funders.filter(f => f.full_name).map(f => new TableRow({
        cantSplit: true,
        children: [countryCell(f.country, all), new TableCell({ width: { size: w2, type: WidthType.DXA }, margins: m, borders: all, verticalAlign: VerticalAlign.CENTER, children: [new Paragraph({ spacing: { after: 0 }, children: [new TextRun({ text: f.name, bold: true }), new TextRun({ text: `  ·  ${f.full_name}` })] })] })],
      })),
    }),
  ];
}

/* ---------- the document ---------- */
function buildDocument(lang, content, pages) {
  const S = STRINGS[lang]; if (!S) throw new Error(`no strings for language "${lang}" in build/strings.json`);
  const st = {
    dir: content.dir, entries: [], missing: [], warnings: [], gaps: new WeakSet(), chapters: new WeakSet(), labels: new Map(), h1: 0, h2: 0, chapterNumbered: false, lists: 0, figures: 0, tables: 0, images: 0,
    shotScale: parseFloat(content.meta.screenshot_scale) || 2, viewport: parseFloat(content.meta.viewport_width) || 1100,
    warn(msg) { if (!this.warnings.includes(msg)) this.warnings.push(msg); },
  };
  const R = makeRenderer(S, st), tokens = marked.lexer(content.src);
  prepare(tokens, st); number(tokens, st);
  const body = R.blocks(tokens, { left: 0, width: BODY_W, level: 0 });
  const closing = partnersPage(S, lang, R);
  const flow = [...body, ...closing].filter((b, i, all) => !(st.gaps.has(b) && (i === all.length - 1 || st.chapters.has(all[i + 1]))));
  const title = content.meta.title || S.manual_title, subtitle = content.meta.subtitle || S.subtitle;

  const cached = st.entries.map((e, i) => ({ title: e.title, level: e.level, href: e.id, page: pages === 'placeholder' ? 888 : pages ? pages[i] : undefined }));
  const contents = [
    new Paragraph({ style: 'ContentsTitle', children: [new TextRun({ text: S.contents })] }),
    new TableOfContents(S.contents, { hyperlink: true, headingStyleRange: '1-2', cachedEntries: cached, beginDirty: !pages }),
  ];

  const markSize = imageSize(MARK);
  const header = new Header({
    children: [new Paragraph({
      tabStops: [{ type: TabStopType.RIGHT, position: BODY_W }], spacing: { after: 0 }, border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: C.line, space: 6 } },
      children: [
        new ImageRun({ type: 'png', data: MARK, transformation: { width: px(0.82 * markSize.w / markSize.h), height: px(0.82) }, altText: { name: 'logo-mark', description: `${BRAND} logo`, title: BRAND } }),
        new TextRun({ text: `  ${BRAND}`, bold: true, size: 21, color: C.brown }),
        new TextRun({ children: [new Tab(), `${title}  ·  ${subtitle}`], size: 18, color: C.grey }),
      ],
    })],
  });
  const footer = new Footer({
    children: [new Paragraph({
      tabStops: [{ type: TabStopType.RIGHT, position: BODY_W }], spacing: { after: 0 },
      children: [new TextRun({ text: DATA.project.platform_url.replace(/^https?:\/\//, ''), size: 17, color: C.grey }), new TextRun({ children: [new Tab(), PageNumber.CURRENT], size: 19, color: C.brown, bold: true })],
    })],
  });

  const headingStyle = (font, size, color, before, after, extra = {}) => ({ run: { font, size, bold: true, color }, paragraph: { spacing: { before, after }, keepNext: true, keepLines: true, ...extra } });
  const doc = new Document({
    creator: BRAND, title: `${BRAND} ${title}`, subject: subtitle, description: `${BRAND} ${title}, ${subtitle}`, lastModifiedBy: BRAND,
    styles: {
      default: {
        document: { run: { font: FONT, size: 22, color: C.text, language: { value: S.locale } }, paragraph: { spacing: { after: 130, line: 280, lineRule: LineRuleType.AUTO } } },
        heading1: headingStyle(SERIF, 46, C.brown, 0, 300, { border: { bottom: { style: BorderStyle.SINGLE, size: 12, color: C.line, space: 8 } } }),
        heading2: headingStyle(FONT, 30, C.brown, 380, 140),
        heading3: headingStyle(FONT, 24, C.label, 260, 90),
        heading4: headingStyle(FONT, 22, C.text, 200, 80),
      },
      paragraphStyles: [
        { id: 'Caption', name: 'caption', basedOn: 'Normal', next: 'Normal', quickFormat: true, run: { size: 19, italics: true, color: C.grey }, paragraph: { alignment: AlignmentType.CENTER, spacing: { after: 260 } } },
        { id: 'TableCaption', name: 'Table Caption', basedOn: 'Normal', next: 'Normal', run: { size: 19, italics: true, color: C.grey }, paragraph: { spacing: { before: 200, after: 90 }, keepNext: true } },
        { id: 'ContentsTitle', name: 'Contents Title', basedOn: 'Normal', next: 'Normal', run: { font: SERIF, size: 46, bold: true, color: C.brown }, paragraph: { spacing: { after: 300 }, border: { bottom: { style: BorderStyle.SINGLE, size: 12, color: C.line, space: 8 } } } },
        { id: 'TOC1', name: 'toc 1', basedOn: 'Normal', next: 'Normal', run: { bold: true, color: C.brown, size: 23 }, paragraph: { spacing: { before: 200, after: 50 } } },
        { id: 'TOC2', name: 'toc 2', basedOn: 'Normal', next: 'Normal', run: { size: 22 }, paragraph: { indent: { left: 380 }, spacing: { after: 30 } } },
      ],
      characterStyles: [{ id: 'IndexLink', name: 'Index Link', run: {} }],
    },
    numbering: {
      config: [
        ...[['steps', LevelFormat.DECIMAL], ['substeps', LevelFormat.LOWER_LETTER]].map(([reference, format]) => ({ reference, levels: [0, 1, 2].map(level => ({ level, format, text: `%${level + 1}.`, alignment: AlignmentType.LEFT, style: { run: { bold: true, color: C.gold }, paragraph: { indent: { left: LIST_STEP * (level + 1), hanging: LIST_STEP } } } })) })),
        ...[['bullets', '\u2022'], ['dashes', '\u2013']].map(([reference, text]) => ({ reference, levels: [0, 1, 2].map(level => ({ level, format: LevelFormat.BULLET, text, alignment: AlignmentType.LEFT, style: { run: { color: C.gold }, paragraph: { indent: { left: LIST_STEP * (level + 1), hanging: LIST_STEP } } } })) })),
      ],
    },
    sections: [
      { properties: { page: { size: { width: PAGE.w, height: PAGE.h }, margin: { top: tw(COVER.top), bottom: tw(COVER.bottom), left: tw(COVER.side), right: tw(COVER.side), header: 0, footer: 0 } } }, children: coverPage(S, content.meta, lang) },
      {
        properties: { type: SectionType.NEXT_PAGE, page: { size: { width: PAGE.w, height: PAGE.h }, margin: { top: BODY.top, bottom: BODY.bottom, left: BODY.side, right: BODY.side, header: BODY.header, footer: BODY.footer } } },
        headers: { default: header }, footers: { default: footer }, children: [...contents, ...flow],
      },
    ],
  });
  return { doc, st };
}

async function writeDocx(doc, file) {
  const zip = await JSZip.loadAsync(await Packer.toBuffer(doc)), clean = new JSZip();
  for (const [name, f] of Object.entries(zip.files)) if (!f.dir) clean.file(name, await f.async('nodebuffer'), { createFolders: false });   // no folder entries in the archive
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, await clean.generateAsync({ type: 'nodebuffer', compression: 'DEFLATE', compressionOptions: { level: 9 } }));
}

/* ---------- PDF ---------- */
function findSoffice() {
  const list = [process.env.SOFFICE, 'soffice', 'C:\\Program Files\\LibreOffice\\program\\soffice.exe', 'C:\\Program Files (x86)\\LibreOffice\\program\\soffice.exe', '/Applications/LibreOffice.app/Contents/MacOS/soffice', '/usr/bin/soffice'];
  for (const c of list.filter(Boolean)) { if (c.includes(path.sep) ? fs.existsSync(c) : spawnSync(c, ['--version'], { stdio: 'ignore' }).status === 0) return c; }
  return null;
}
function pdfEngine() {
  if (!WANT_PDF) return null;
  if (process.platform === 'win32' && ENGINE !== 'libreoffice' && spawnSync('reg', ['query', 'HKCR\\Word.Application'], { stdio: 'ignore' }).status === 0) return { name: 'Microsoft Word', run: wordPdf };
  const so = ENGINE !== 'word' ? findSoffice() : null;
  return so ? { name: 'LibreOffice', run: (i, o) => sofficePdf(so, i, o) } : null;
}
function wordPdf(docx, pdf) {
  const ps = path.join(os.tmpdir(), `top4_manual_pdf_${process.pid}.ps1`);
  fs.writeFileSync(ps, [
    'param([string]$In, [string]$Out)', "$ErrorActionPreference = 'Stop'", '$word = New-Object -ComObject Word.Application', '$word.Visible = $false', '$word.DisplayAlerts = 0',
    'try {', '  $doc = $word.Documents.Open($In, $false, $true)',
    '  $doc.ExportAsFixedFormat($Out, 17, $false, 0, 0, 1, 1, 0, $true, $true, 1, $true, $true, $false)',     // 17 = PDF, bookmarks from headings
    '  $doc.Close(0)', '} finally {', '  $word.Quit()', '}',
  ].join('\r\n'));
  const r = spawnSync('powershell', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ps, '-In', docx, '-Out', pdf], { encoding: 'utf8' });
  fs.rmSync(ps, { force: true });
  if (r.status !== 0 || !fs.existsSync(pdf)) throw new Error(`Microsoft Word could not write the PDF:\n${r.stderr || r.stdout}`);
}
function sofficePdf(soffice, docx, pdf) {
  const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'top4_manual_lo_')), outDir = path.dirname(pdf);
  const r = spawnSync(soffice, [`-env:UserInstallation=${pathToFileURL(profile).href}`, '--headless', '--convert-to', 'pdf', '--outdir', outDir, docx], { encoding: 'utf8' });
  fs.rmSync(profile, { recursive: true, force: true });
  const made = path.join(outDir, path.basename(docx).replace(/\.docx$/i, '.pdf'));
  if (!fs.existsSync(made)) throw new Error(`LibreOffice could not write the PDF:\n${r.stderr || r.stdout}`);
  if (made !== pdf) fs.renameSync(made, pdf);
}

/* The text of every page of a PDF, without spaces and in lower case; text.pictures[i] counts the pictures drawn on page i + 1. */
async function pageTexts(pdf) {
  const quiet = { log: console.log, warn: console.warn }; console.log = console.warn = () => {};       // the PDF reader prints notes about drawing; only text is read here
  let pdfjs; try { pdfjs = await import('pdfjs-dist/legacy/build/pdf.mjs'); } finally { Object.assign(console, quiet); }
  const task = pdfjs.getDocument({ data: new Uint8Array(fs.readFileSync(pdf)), verbosity: 0 }), doc = await task.promise;
  const text = [], pictures = [], paint = new Set([pdfjs.OPS.paintImageXObject, pdfjs.OPS.paintInlineImageXObject, pdfjs.OPS.paintImageMaskXObject].filter(x => x !== undefined));
  for (let p = 1; p <= doc.numPages; p++) {
    const page = await doc.getPage(p), tc = await page.getTextContent();
    text.push(norm(tc.items.map(i => i.str).join('')));
    try { pictures.push((await page.getOperatorList()).fnArray.filter(f => paint.has(f)).length); } catch { pictures.push(0); }
  }
  await task.destroy?.();
  text.pictures = pictures;
  return text;
}

/* Page of every contents entry. The contents table lists each heading once; the chapters repeat them in the same order after it. */
function headingPages(text, entries) {
  const keys = entries.map(e => norm(e.title)), start = text.findIndex(t => t.includes(keys[keys.length - 1])) + 1;      // first page after the contents table
  const pages = [], notFound = [];
  let p = start, chapter = start - 1;
  for (let i = 0; i < keys.length; i++) {
    let q = entries[i].level === 1 ? Math.max(p, chapter + 1) : p;          // a chapter always starts on a new page
    while (q < text.length && !text[q].includes(keys[i])) q++;
    if (q >= text.length) { notFound.push(entries[i].title); pages.push(pages.length ? pages[pages.length - 1] : start + 1); continue; }
    pages.push(q + 1); p = q; if (entries[i].level === 1) chapter = q;
  }
  return { pages, notFound };
}

/* ---------- main ---------- */
const engine = pdfEngine();
if (WANT_PDF && !engine) console.log('! No PDF engine found (Microsoft Word on Windows, or LibreOffice). Word files only; their contents table gets its page numbers when Word updates it on opening.');
let failed = false;
for (const lang of langs) {
  try {
    const content = readContent(lang), name = `${BRAND}_User_Manual_${lang.toUpperCase()}`, out = path.join(ROOT, 'dist', `${name}.docx`), pdf = out.replace(/\.docx$/, '.pdf');
    const notes = [];
    let pages = null, total = 0, built;
    if (engine) {
      // first pass: lay the document out once to learn on which page every heading falls
      const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'top4_manual_')), d1 = path.join(tmp, `${name}.docx`), p1 = path.join(tmp, `${name}.pdf`);
      const draft = buildDocument(lang, content, 'placeholder');
      await writeDocx(draft.doc, d1);
      engine.run(d1, p1);
      try { pages = headingPages(await pageTexts(p1), draft.st.entries).pages; }
      catch (e) { notes.push(`page numbers of the contents table could not be read (${e.message.split('\n')[0]}); Word fills them in when it updates the table on opening`); }
      fs.rmSync(tmp, { recursive: true, force: true });
    }
    for (let round = 0; round < 2; round++) {
      built = buildDocument(lang, content, pages);
      await writeDocx(built.doc, out);
      if (!engine) break;
      engine.run(out, pdf);
      if (!pages) break;
      const text = await pageTexts(pdf), found = headingPages(text, built.st.entries);
      total = text.length;
      if (round === 0 && found.pages.join() !== pages.join()) { pages = found.pages; continue; }          // the numbers moved a heading: write once more with the final pages
      found.notFound.forEach(t => notes.push(`contents: no page found for "${t}"`));
      const fixed = [BRAND, content.meta.title || STRINGS[lang].manual_title, content.meta.subtitle || STRINGS[lang].subtitle, DATA.project.platform_url.replace(/^https?:\/\//, '')].map(norm);
      const body = t => fixed.reduce((s, f) => s.replace(f, ''), t).replace(/[\d·]/g, '');
      text.forEach((t, i) => { if (i > 0 && body(t).length < 3) notes.push(`page ${i + 1} is empty: look for a forced page break or a picture that leaves a gap`); });
      // the last page of a chapter that holds only a few lines (the header logo is the one picture on a page of text)
      let chapterStart = -1;
      built.st.entries.forEach((e, k) => {
        if (e.level !== 1) return;
        const first = found.pages[k] - 1, last = first - 1;                // page indexes: this chapter's first page, the page before it
        if (chapterStart >= 0 && last > chapterStart && body(text[last]).length < 320 && (text.pictures[last] ?? 0) <= 1)
          notes.push(`page ${last + 1} holds only a few lines before "${e.title}" starts: shorten or rearrange the end of the chapter before it`);
        chapterStart = first;
      });
      break;
    }
    const st = built.st;
    console.log(`${lang}: ${path.relative(ROOT, out)}${engine ? ` + .pdf (${total ? `${total} pages, ` : ''}${engine.name})` : ''}, ${st.entries.length} headings, ${st.figures} figures, ${st.tables} tables`);
    [...notes, ...st.warnings].forEach(w => console.log(`  ! ${w}`));
    if (st.missing.length) { failed = true; console.log(`  ! ${st.missing.length} image(s) missing:`); [...new Set(st.missing)].forEach(m => console.log(`    - ${m}`)); }
  } catch (e) { failed = true; console.log(`${lang}: FAILED\n  ${e.message}`); }
}
process.exit(failed ? 1 : 0);
