# TOP4HoneyChains user manual

The user manual of the TOP4HoneyChains web platform in the five UI languages. English is the master; Turkish is reviewed; Spanish, Polish and Latvian are drafts for partner review.

| Language | Sources | PDF |
|---|---|---|
| English (master) | [en/](en/README.md) | `dist/TOP4HoneyChains-User-Manual-EN.pdf` |
| Türkçe | [tr/](tr/README.md) | `dist/TOP4HoneyChains-User-Manual-TR.pdf` |
| Español (draft) | [es/](es/README.md) | `dist/TOP4HoneyChains-User-Manual-ES.pdf` |
| Polski (draft) | [pl/](pl/README.md) | `dist/TOP4HoneyChains-User-Manual-PL.pdf` |
| Latviešu (draft) | [lv/](lv/README.md) | `dist/TOP4HoneyChains-User-Manual-LV.pdf` |

## Source of truth

The manual describes the application as it runs with `Features:OrgRoleModel = true` and the D-78 switches on (`Features:CustodyIntake`, `Features:BeekeeperAccounts`); chapter 15 lists the switches and what they change while off. It was written from the code (controllers, views, permission checks, `db/migrations`, in particular `013_role_permissions.sql` and `015_ccp0_reception_officer.sql`). UI labels and messages are taken from `TOP4HONEYCHAINS/TOP4HoneyChains.Web/Resources/SharedResource.<lang>.resx`; role, capability, status and CCP names from the reference translations in the database. When the application changes, update the English chapter first, then the same section in the other languages.

## Structure

Each language folder has the same files, the same headings and the same anchors (`{#anchor}` after a heading), so links and screenshots line up across languages:

1. About TOP4HoneyChains
2. Capabilities, organizations and roles (with the permission matrix)
3. Getting started
4. The end-to-end flow
5. – 13. One chapter per role (12 Beekeeper registration officer, 13 Compliance Officer)
14. The beekeeper portal
15. Platform administrator
16. Trust and privacy
17. Glossary and FAQ

## Figures, tables and references

The build numbers figures and tables in order and writes the captions in each language ("Figure 3. …", "Tabla 5. …"); the lists of figures and tables follow the contents. In the sources (the keywords stay in English in every language):

- a screenshot is a `<!-- SHOT: <id> -->` line followed by a line `Figure: <caption>`; its anchor is `#fig-<id>`;
- a table is preceded by a line `Table: <caption> {#tbl-<id>}` and a blank line;
- a reference is a link `[Figure](#fig-<id>)` or `[Table](#tbl-<id>)`; the build replaces its text with the number.

The build reports any figure or table without a caption and any reference to an unknown id ("problems" in its output must be 0).

## Cover and logos

The cover shows the platform logo, the project and the ICT-AGRI-FOOD 2022 call, the programme logo with an acknowledgement line, and the consortium partners. The logos come from `docs/brand/`:

- `programme/`: the ICT-AGRI-FOOD ERA-NET Cofund logo, shown prominently with the acknowledgement line (text per language in `build.mjs`, provisional until the wording is confirmed);
- `partners/`: the consortium partners' logos, in file-name order (`01-…` to `09-…`), in a strip along the bottom captioned "Consortium" in each language, all at the same height;
- `funders/` (optional, not present yet): the funding agencies' logos, shown as a second strip captioned "Funding", and `acknowledgement.<lang>.md` or `acknowledgement.md`, which replaces the acknowledgement line. Partner logos are never shown as funders.

`docs/brand/partners/README.md` says where each logo comes from and which ones need a better file.

## Planned application changes and review

[PLANNED-APP-CHANGES.md](PLANNED-APP-CHANGES.md) lists the application changes the review asked for; all but the multi-country origin map (A8) shipped with D-78 (D-84); [REVIEW-RESPONSE-2026-10.md](REVIEW-RESPONSE-2026-10.md) answers the review of October 2026 comment by comment.

## Screenshots

Wherever a screen helps, the text has a line `<!-- SHOT: <id> -->`. [shots.yml](shots.yml) is the capture list: for each shot the page, role, [DEMO] record, UI state, viewport, what to highlight and the file name `img/<lang>/<id>.png`. The capture script `capture.mjs` in this folder takes the shots against the local demo environment:

```
node capture.mjs            # all five languages
node capture.mjs tr en      # only some
```

It reads the demo users' credentials at run time from the untracked `demo_users.txt` at the repository root; they are never stored in this folder or in the repository. The build inserts each image where its marker is and leaves out a marker whose image is missing.

## Building the PDFs

Requires Node.js and Chrome, Edge or Chromium on the machine.

```
cd docs/manual
npm install
node build.mjs            # all five languages
node build.mjs tr en      # only some
```

Set `MANUAL_DATE=YYYY-MM-DD` to print a fixed version date on the cover (default: today). The build renders each language to `dist/TOP4HoneyChains-User-Manual-<LANG>.html` and `.pdf`: a cover (W3 logo, "TÜBİTAK TOP4HoneyChains", title, language, version date, draft note for ES/PL/LV), a table of contents, and the chapters in the honey-foam palette and the app's fonts. If the PDF step fails, the HTML is still written.
