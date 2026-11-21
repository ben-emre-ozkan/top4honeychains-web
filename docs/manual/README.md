# TOP4HoneyChains user manuals

This folder holds everything that makes the user manuals of the web platform: the text (Markdown, one folder per language), the screenshots, the template that turns them into Word and PDF files, and the institution block (partners and supporters).

- Start with `BRIEF.md`: goal, rules, phases, writing format.
- Build: `cd build`, `npm install`, `node build.mjs en` (or `tr`, `es`, `pl`, `lv`, `all`). Results are written to `dist/`.
- Layout sample: `sample/`.

## Screenshots

The pictures come from the platform's own run on a database with fictional data only (`TOP4HoneyChainsDb_Manual`, the Şile cast of `tools/live-like`). Never take pictures from the local copy of the live database.

1. Once: `powershell -ExecutionPolicy Bypass -File capture\setup-db.ps1` (stop `run-demo.ps1` first). It builds the database and keeps a backup for resets; the practice password goes to `capture/.env.local` (untracked).
2. `cd capture`, `npm install`, `npx playwright install chromium`, then `node capture.mjs en`. Every run restores the backup, starts the app on port 5256 and carries one honey through the whole journey, taking each picture where the step happens (about 3 minutes). `--only=03,04` takes only those chapters' pictures.
3. `capturepp.ps1 stop` stops the app afterwards.

## Changing the institutions

Names, order and logos are in `assets/institutions.json` and `assets/logos/`. After a change run `node assets/board/render_board.mjs` (it uses the Playwright installed in `capture/`) to redraw the board on the cover, then build again. Logos are the institutions' own files: do not redraw or recolour them.

## Credits

The board uses the open fonts Instrument Serif, Instrument Sans and Bricolage Grotesque (SIL Open Font License 1.1). The pages use Calibri, Cambria and Consolas, which come with Microsoft Office; LibreOffice replaces them with Carlito, Caladea and its own fixed-width font. Sources of the logo files are listed in `assets/README.md`.

## Glossary

`glossary-data.json` holds the terms of the manuals in five languages (from the interface where it has the term, otherwise chosen by the translators; unsure terms are marked). `python build/glossary.py` writes `glossary.md` and `dist/TOP4HoneyChains_Glossary.docx` (one table, unsure terms marked with (?)), which goes out with the PDFs for the partners to check.
