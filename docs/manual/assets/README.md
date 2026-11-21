# Institution block: partners and supporters

The same institutions, order and wording as the opening board of the launch film, approved by the project coordinator on 20 November 2026, plus the ICT-AGRI-FOOD programme logo that he asked for on the manual cover (review of 11 October 2026).

## Contents

- `institutions.json`: partners by country, supporters, the programme and the fixed texts in five languages (en, tr, es, pl, lv). Use the strings as they are; do not retranslate them.
- `logos/`: partner, funder and programme logos as PNG (SVG originals where they exist), the project logo and its emblem.
- `flags/`: the EU emblem and the flags of Türkiye, Poland, Latvia and Argentina (PNG and SVG).
- `board/`: the board for the cover, drawn from the data above. `board_<lang>.jpg` are ready; `board.html` and `render_board.mjs` redraw them after a change.

## Rules

- Logos are the institutions' own files. Do not redraw, recolour, crop or stretch them.
- Apiculture Research Institute, INTA, Coop. Agrop. y Apícola Norte Grande, Alimentos Naturales (its ALELUYA mark) and ICT-AGRI-FOOD use the files the project supplied (`docs/brand/partners`, `docs/brand/programme`, see the README there). INTA's supplied full logo has white texts for dark backgrounds, so only the INTA mark is used.
- Institution names: English names in the English, Spanish, Polish and Latvian manuals, except where `institutions.json` gives the official name in that language; Turkish names in the Turkish manual.
- Supporters shown: European Union (Horizon 2020, ICT-AGRI-FOOD ERA-NET Cofund, Grant Agreement No 862665), TÜBİTAK, NCBR, LZP, INTA.

## To be confirmed by the partners

- The Spanish, Polish and Latvian wording of the support line and of the funding statement.
- Sharper files would help: the institute's logo is 90 pixels high; a positive (dark on light) version of the full INTA logo.

## Sources of the logo files

Supplied by the project (`docs/brand`): Apiculture Research Institute, INTA, Norte Grande, Alimentos Naturales (ALELUYA), ICT-AGRI-FOOD. From the institutions' websites: tubitak.gov.tr, bogazici.edu.tr (media store), isikun.edu.tr (site logo, shown without its anniversary badge), nexco-sa.com.ar, ue.katowice.pl, lbtu.lv, gov.pl (NCBR), lzp.gov.lv. Flags and the EU emblem are drawn to their official specifications; the sun of the Argentine flag comes from flag-icons 7.6.0 (MIT licence).
