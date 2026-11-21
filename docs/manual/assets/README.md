# Institution block: partners and supporters

The same institutions, order and wording as the opening board of the launch film, approved by the project coordinator on 20 November 2026.

## Contents

- `institutions.json`: partners by country, supporters and the fixed texts in five languages (en, tr, es, pl, lv). Use the strings as they are; do not retranslate them.
- `logos/`: partner and funder logos as PNG (SVG originals where they exist), the project logo and its emblem.
- `flags/`: the EU emblem and the flags of Türkiye, Poland, Latvia and Argentina (PNG and SVG).
- `board/`: the board for the cover, drawn from the data above. `board_<lang>.jpg` are ready; `board.html` and `render_board.mjs` redraw them after a change.

## Rules

- Logos are the institutions' own files. Do not redraw, recolour, crop or stretch them.
- INTA: only a white logo exists, so the board and the documents use `logos/inta_on_navy.png` (white logo on a dark plate). Never put `inta_white.png` on a light background.
- CNORTEGRANDE Cooperative and Alimentos Naturales Natural Foods S.A. have no logo file: their names are set as plain text. Do not invent a logo.
- Apiculture Research Institute uses the emblem of the Ministry of Agriculture and Forestry. The file is 100 x 100 pixels: keep it small.
- Institution names: English names in the English, Spanish, Polish and Latvian manuals, except where `institutions.json` gives the official name in that language; Turkish names in the Turkish manual.
- Supporters shown: European Union (Horizon 2020, ICT-AGRI-FOOD ERA-NET Cofund, Grant Agreement No 862665), TÜBİTAK, NCBR, LZP, INTA.

## To be confirmed by the partners

- The Spanish, Polish and Latvian wording of the support line and of the funding statement.
- Missing files: logos of the two Argentine partners named above, a colour version of the INTA logo, the institute's own logo if it has one, the ICT-AGRI-FOOD logo.

## Sources of the logo files

tubitak.gov.tr, bogazici.edu.tr (media store), isikun.edu.tr (site logo, shown without its anniversary badge), tarimorman.gov.tr, argentina.gob.ar (INTA), nexco-sa.com.ar, ue.katowice.pl, lbtu.lv, gov.pl (NCBR), lzp.gov.lv. Flags and the EU emblem are drawn to their official specifications; the sun of the Argentine flag comes from flag-icons 7.6.0 (MIT licence).
