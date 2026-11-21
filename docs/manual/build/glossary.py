"""Writes docs/manual/glossary.md and dist/TOP4HoneyChains_Glossary.docx from the glossary data (glossary-data.json):
one row per term, one column per language; a term the translators were not sure of is marked (?) for the partners to confirm.
usage: python glossary.py"""
import io, json, os
from docx import Document
from docx.enum.section import WD_ORIENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
LANGS = [('tr', 'Türkçe'), ('es', 'Español (Argentina)'), ('pl', 'Polski'), ('lv', 'Latviešu')]
data = json.load(io.open(os.path.join(ROOT, 'glossary-data.json'), encoding='utf-8'))
terms = sorted(data['terms'], key=lambda t: t['en'].lower())
unsure = {l: sum(1 for t in terms if t.get(l, {}).get('unsure')) for l, _ in LANGS}
INTRO = data['intro']
COUNTS = 'Unsure terms: ' + ', '.join(f'{l} {unsure[l]}' for l, _ in LANGS) + '.'

# ---- Markdown
esc = lambda x: x.replace('|', '\\|')
md = ['# Glossary of the user manuals', '', INTRO, '', COUNTS, '',
      '| English | ' + ' | '.join(n for _, n in LANGS) + ' |', '| --- |' + ' --- |' * len(LANGS)]
for t in terms:
    cells = []
    for l, _ in LANGS:
        c = t.get(l)
        cells.append((esc(c['text']) + (' **(?)**' if c.get('unsure') else '')) if c and c.get('text') else '·')
    md.append('| ' + esc(t['en']) + ' | ' + ' | '.join(cells) + ' |')
io.open(os.path.join(ROOT, 'glossary.md'), 'w', encoding='utf-8', newline='\n').write('\n'.join(md) + '\n')

# ---- Word: landscape A4, one table, header row repeated on every page
doc = Document()
sec = doc.sections[0]
sec.orientation = WD_ORIENT.LANDSCAPE
sec.page_width, sec.page_height = Cm(29.7), Cm(21.0)
for m in ('left_margin', 'right_margin', 'top_margin', 'bottom_margin'):
    setattr(sec, m, Cm(1.6))
st = doc.styles['Normal']
st.font.name = 'Calibri'
st.font.size = Pt(9.5)
doc.add_heading('TOP4HoneyChains user manuals: glossary', level=1)
doc.add_paragraph(INTRO.replace('**', ''))
doc.add_paragraph(COUNTS)
table = doc.add_table(rows=1, cols=1 + len(LANGS))
table.style = 'Table Grid'
head = ['English'] + [n for _, n in LANGS]
for i, h in enumerate(head):
    cell = table.rows[0].cells[i]
    cell.text = ''
    r = cell.paragraphs[0].add_run(h)
    r.bold = True
    r.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
    shd = OxmlElement('w:shd'); shd.set(qn('w:val'), 'clear'); shd.set(qn('w:fill'), '5A3E1B')
    cell._tc.get_or_add_tcPr().append(shd)
trPr = table.rows[0]._tr.get_or_add_trPr()
rep = OxmlElement('w:tblHeader'); rep.set(qn('w:val'), 'true'); trPr.append(rep)
for t in terms:
    row = table.add_row().cells
    row[0].text = t['en']
    for i, (l, _) in enumerate(LANGS, start=1):
        c = t.get(l) or {}
        row[i].text = ''
        p = row[i].paragraphs[0]
        p.add_run(c.get('text') or '·')
        if c.get('unsure'):
            q = p.add_run('  (?)')
            q.bold = True
            q.font.color.rgb = RGBColor(0xB0, 0x3A, 0x2E)
            shd = OxmlElement('w:shd'); shd.set(qn('w:val'), 'clear'); shd.set(qn('w:fill'), 'FCEBD2')
            row[i]._tc.get_or_add_tcPr().append(shd)
widths = [Cm(6.0)] + [Cm(5.1)] * len(LANGS)
for r in table.rows:
    for w, cell in zip(widths, r.cells):
        cell.width = w
out = os.path.join(ROOT, 'dist', 'TOP4HoneyChains_Glossary.docx')
os.makedirs(os.path.dirname(out), exist_ok=True)
doc.save(out)
print(f'{len(terms)} terms; unsure {unsure}; {os.path.relpath(out, ROOT)} and glossary.md written')
