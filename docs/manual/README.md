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

The manual describes the application as it runs with `Features:OrgRoleModel = true`. It was written from the code (controllers, views, permission checks, `db/migrations`, in particular `013_role_permissions.sql` and `015_ccp0_reception_officer.sql`). UI labels and messages are taken from `TOP4HONEYCHAINS/TOP4HoneyChains.Web/Resources/SharedResource.<lang>.resx`; role, capability, status and CCP names from the reference translations in the database. When the application changes, update the English chapter first, then the same section in the other languages.

## Structure

Each language folder has the same files, the same headings and the same anchors (`{#anchor}` after a heading), so links and screenshots line up across languages:

1. About TOP4HoneyChains
2. Capabilities, organizations and roles (with the permission matrix)
3. Getting started
4. The end-to-end flow
5. – 11. One chapter per role
12. Platform administrator
13. Trust and privacy
14. Glossary and FAQ

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
