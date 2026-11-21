# TOP4HoneyChains user manuals: brief

**Update of 21 November 2026 (after Phase 1).** New: numbered captions for tables and figures, ids and cross-references (see "Writing format"); the coordinator's review notes in `notes/`; the October manual's depth is required again (see "Structure of a manual"); the cover board now carries the ICT-AGRI-FOOD logo and the partners' own logos; decisions and the work list are in "Phase 1b". Do Phase 1b before Phase 2.

## Goal

Five user manuals for the web platform (`TOP4HoneyChains.Web`), one per language: English (en), Turkish (tr), Spanish for Argentina (es), Polish (pl), Latvian (lv). Each one is delivered as a Word file and a PDF, and each one is complete on its own.

The readers are the people who will use the platform in each partner country: staff of beekeepers' associations, laboratories, packagers, wholesalers and retailers, administrators, and consumers. They are not technical. The manuals describe the system as it is now.

## What is in this folder

| Path | What it is |
| --- | --- |
| `build/` | The template. `build.mjs` turns `content/<lang>/manual.md` into the Word file and the PDF: cover, contents table, chapter and figure numbers, header, footer, last page. |
| `assets/` | Institution data (`institutions.json`), logos, flags and the partners and supporters board for the cover (`board/`). Approved by the project coordinator; see "Institutions". |
| `capture/helpers.mjs` | Screenshot helpers (window size, marks with step numbers, cropping). Write the capture flows on top of them. |
| `sample/` | `sample.md` shows every building block of the writing format; the PDF and Word files show the result. |
| `notes/` | Review notes on earlier manuals. Apply every note and fill in its status. |

You create: `outline.md`, `glossary.md`, `capture/capture.mjs`, `content/<lang>/manual.md`, `content/<lang>/screens/`, `reports/`. The inventory from the first step is `inventory.md`.

## Rules

1. The code and the running application are the source of truth. Describe only what exists and works. Do not invent features, fields or messages. Where something is broken or unfinished, leave the task out or describe what really happens, and list it in `reports/ui-issues.md`.
2. Do not change application code, migrations or committed configuration in this task, except the fixes that Phase 1b lists, on their own branch. Run the app for screenshots through environment variables and a separate database. Any other problem you would fix in code goes into `reports/ui-issues.md`.
3. Privacy. The local database `TOP4HoneyChainsDb` is a copy of the live system with real personal data. Nothing from it may appear in a picture. Use a separate database (for example `TOP4HoneyChainsDb_Manual`) that holds only fictional data; reference lists without personal data may be copied.
4. Sample data looks real but is invented: believable names of people and organisations, sensible quantities and dates. No real organisation appears in the sample data, in pictures or in text (the project partners and funders on the cover and the last page are the exception; places such as villages and districts may be real). No "[DEMO]" tags, no accounts or records named "test", "demo" or "admin", no placeholder text. No photos of real people. E-mails and text messages must not reach real addresses or phones.
5. No em-dashes anywhere, in any language. The build stops on them.
6. Work on the branch `docs/user-manuals`. Commit text, scripts and reports. Do not commit `dist/` or `node_modules/`. Do not merge or deploy.
7. Reports to the user are in English and short.

## Phases

### Phase 1: the English manual, then stop

1. Prove the toolchain first: `npm install` and `node build.mjs en` in `build/` with the sample as it is, and compare the PDF with `sample/`. Fix any failure before writing anything.
2. Read `inventory.md` (update it if the code has changed) and any existing manual it lists. Keep what is still true. If there is no `inventory.md`, write it first: roles and how each one signs in, pages and tasks per role in the order of the honey journey, the state of the five interface languages, how to run the app locally with fictional data only, existing manuals in the repository.
3. Write `outline.md`: chapters, tasks and the pictures each task needs.
4. Prepare the screenshot run: the separate database with the fictional cast, one account per role, and `capture/capture.mjs`, which signs in per role and takes every picture for a language (`node capture.mjs en`). Passwords stay in an untracked local file. The run must be repeatable: the same pictures every time (reset the data before a run, or do not save changes during it).
5. Write `content/en/manual.md`, take the pictures, build, and check the result (see "Checks").
6. Stop and report: page count, what is covered, open questions, the two reports. The user will have the English PDF reviewed before the other languages start.

### Phase 1b: depth, captions and decisions (21 November 2026), then stop

The Phase 1 manual (70 pages, about 8,100 words) reads well task by task, but it lost the conceptual and reference parts of the October manual (about 28,500 words) that the coordinator reviewed and asked for. Do these steps in order.

1. **Interface fixes first**, on a branch `D-94` from `main`, merged like the other D- items; then bring `docs/user-manuals` up to date with `main`. Only these items of `reports/ui-issues.md`:
   - 2: the beekeeper portal's language switch on phones (use the panel's compact language menu or one select);
   - 3: the radio buttons of the consumer's feedback form next to their labels;
   - 6: the Electrical Conductivity limit shows only the limit of the lot's honey type;
   - 8: phone numbers shown formatted (`+90 555 000 08 02`) wherever they are displayed;
   - 10: English uses the UK flag and British dates (21 November 2026) everywhere.
   Mark them fixed in the report. Leave the other items as they are.
2. **Decisions of the user:**
   - The manuals describe the platform with all four feature switches on. Update `docs/GO-LIVE.md` to say so. If committed production settings say otherwise, list that in the report; do not change them.
   - Fictional organisations only. In the manual database (`capture/setup-db.ps1`), rename "Şile Arıcılar Birliği" and "Şile İlçe Tarım ve Orman Müdürlüğü" to fictional names, for example "Kıyıboyu Arıcılar Birliği" and "Kıyıboyu İlçe Tarım ve Orman Müdürlüğü" (check on the web that no real organisation or district has the name you choose). Apiary places may stay real. Fix the sentence about real names in "About this manual". The demo cast of `run-demo.ps1` stays as it is.
   - Pictures that show the DRAFT badge of a privacy notice stay until the notices are approved; they are retaken then.
3. **Bring back the depth of the October manual.** Read it with `git show 83ab5a6:docs/manual/en/<file>` (17 chapters). Keep the Phase 1 task chapters and their pictures, and add, updated to the current code (D-77 to D-94):
   - a chapter "Capabilities, organisations and roles" after "The platform at a glance": capabilities written as abilities, value-chain activities (what people do) apart from data rights (what the platform records), organisation types and status, the numbered role table, data rights in a CRUD + Approve table per role and record, the permission matrix, what an administrator can change, ground rules, memberships;
   - the Compliance Officer as a chapter of its own: signed photos, sampling quality, data-subject requests, the approval mechanism, agreements, data controller and audit entries;
   - the missing parts of platform administration that exist today (roles and permissions, control points, parameters and limits, reference data and translations, the registry, corrections, recall, notices and consent texts, data retention, the audit log); leave out development-only pages;
   - a chapter "Trust and privacy" (audit log and hash chain, records that do not change, codes and file fingerprints, notices, consents and the data controller, what the public can read, how long data is kept);
   - the questions of the October FAQ that still apply.
   Check every statement against the code; where the October text is wrong today, follow the code.
4. **Captions and references.** Every table and every figure gets a caption, so a number. Give an id to the tables and sections that other text refers to (the role table above all), and refer to them with links (`[Table](#tbl-roles)`).
5. **The coordinator's notes:** work through `notes/2026-10-11-coordinator-review.md` and fill in the status of every row.
6. Retake the English pictures, build, run the checks, commit on `docs/user-manuals`, and stop. Report in English: pages, what was added, the status of the 26 notes in one line, the new organisation names, the D-94 fixes, open questions.

### Phase 2: tr, es, pl, lv (only after the English manual is approved)

1. Write `glossary.md` first: one row per term, one column per language. Take every interface term from the application's own localization resources for that language, not from your own translation.
2. Translate `manual.md` into `content/<lang>/manual.md`. Same structure, same order of headings and pictures, same file names of pictures.
3. Take the pictures again in that language with the same script. A page that the interface shows in English in another language is captured as it is, written with the labels as they appear, and listed in `reports/missing-translations.md` (page, label, language).
4. Build and check each language.
5. No native reader checks these texts here. In `glossary.md`, mark every term you are not sure of, so that the partners can confirm it.

Language notes:
- en: British spelling (programme, organisation).
- tr: polite imperative (seçin, girin, kaydedin).
- es: Spanish as used in Argentina, formal "usted" imperative (seleccione, ingrese), Latin American vocabulary (computadora, celular, contraseña).
- pl: second person singular imperative, as in Polish software documentation (kliknij, wybierz).
- lv: polite plural imperative (izvēlieties, ievadiet).
- Not translated: TOP4HoneyChains, QR, and any product term the interface itself keeps in English.
- Institution names, the cover and the last page are not yours to translate: the build takes them from `assets/`.

## Structure of a manual

Order the role chapters along the journey of the honey.

1. About this manual `{-}`: who it is for, how it is organised, the conventions (bold for labels, numbered steps, marks in pictures).
2. The platform at a glance: what it is for, the journey of the honey from the beekeeper to the jar, who the platform is for (a numbered table that includes the consumer and the visitor), the control points in one table, the four-eyes principle explained, what the consumer sees.
3. Capabilities, organisations and roles: the conceptual chapter (see Phase 1b, step 3).
4. Getting started: open the platform, choose the language, sign in, forgotten password, the parts of the screen, sign out.
5. One chapter per role. It opens with what this role does on the platform and a short table "task, where to find it". Then one section per task. It ends with what happens next and who continues. The Compliance Officer has a chapter of its own.
6. The beekeeper portal, the Honey Story page and the consumer account.
7. Platform administration.
8. Trust and privacy.
9. Problems and questions: real cases only.
10. Glossary `{-}`.

The October 2026 manual set the depth the coordinator reviewed and expects. Length follows the content; expect about 100 pages. Do not describe every field of every form; for a long form use a table: field, what to enter, required or not.

## Writing rules

- One section is one task, named as the user would say it, starting with a verb: "Record a honey delivery".
- A task has: one sentence on when you do it, numbered steps with one action each, and the result the user should see at the end.
- Labels of buttons, menus and fields are in bold and exactly as on the screen in that language. A menu path is **Menu > Item**.
- Speak to the reader directly. Short sentences. Active voice. Present tense.
- Name the roles as the interface names them.
- No internal words: controller, endpoint, database, table, ID, seed.
- Notes are rare. `[!NOTE]` for useful extra information, `[!TIP]` for a shortcut, `[!IMPORTANT]` for a rule that must not be missed, `[!WARNING]` when a mistake loses data or blocks someone.
- Every picture and every table has a caption, and so a number. The caption of a picture says what the reader sees; the caption of a table says what it lists.
- Refer to a table, a figure or a section by a link to its id, never by "the table above".
- Capabilities are abilities ("Able to receive and process honey"). Data rights use CRUD words (create, read, update, delete, approve); "reads", not "sees".
- Keep what people do in the value chain apart from what the platform records and allows.
- The funding is the EU (ICT-AGRI-FOOD ERA-NET Cofund) and four national agencies: never name one agency alone as the funder.

## Writing format

`sample/sample.md` shows all of it. In short:

```
---
version: 1.0
date: 2026-11-21
screenshot_scale: 2
viewport_width: 1100
---

# Chapter title            numbered automatically; add {-} at the end for no number
## Task or section         numbered automatically, listed in the contents table
### Smaller heading        no number
## Ground rules {#ground-rules}      an id, for links: [Ground rules](#ground-rules)

1. A step with a **Label**.
2. A step with its own picture (indent the picture by three spaces):

   ![Caption of the picture](screens/03-02-save.png)

![Caption](screens/03-01-list.png)                      a picture on its own line; the caption gets the number
![Caption {#fig-list}](screens/03-01-list.png)          the same, with an id: [Figure](#fig-list) prints "Figure 7"
![](screens/06-01-story.png "width=42%")                a narrow picture with a set width,
Figure: Caption {#fig-story}                            here with its caption on the line below

![Caption a](screens/a.png) ![Caption b](screens/b.png) two or three pictures side by side

Table: Roles on the platform {#tbl-roles}               the caption line right above a table
| Role | What the role does |                          then the table; [Table](#tbl-roles) prints "Table 3"
In Latvian the number comes first: [{n}. tabulā](#tbl-roles) prints "3. tabulā".

> [!NOTE]
> Text of the note.

| Field | What to enter | Required |      tables as usual
`LOT-2026-0142`                              text the reader types
<!-- pagebreak -->                           a forced new page (rarely needed)
```

Do not write a cover, a contents table, chapter numbers, numbers of figures and tables ("Figure 3", "Table 2") or the partners page: the build adds them, and it warns about a table or picture without a caption and about a link to an unknown id. Title and subtitle of the cover come from `build/strings.json` in the right language.

## Pictures

- Every picture is taken by `capture/capture.mjs` with the helpers, from the local app with the fictional database, in the language of the manual. No picture is taken or edited by hand.
- Window 1280 x 720 CSS pixels (set in Phase 1), scale 2. The same number is in `VIEWPORT` (helpers) and in `viewport_width` (front matter of every `manual.md`). The build sizes every picture from these numbers, so text has the same size in all of them.
- One picture of the whole window where the reader needs to find their way. For a step, cut out the part that matters (`shot(..., { crop })`) instead of repeating whole windows.
- Mark the control a step talks about with `mark(page, locator, stepNumber)`. The number in the picture is the number of the step.
- The Honey Story page and other pages made for phones: a phone-sized window (390 x 844, scale 2), cut into parts, two or three side by side.
- Wait until the page is at rest (`settle`): no loading spinner, no half-finished animation, no open tooltip, maps and charts drawn.
- File names: `content/<lang>/screens/<chapter>-<number>-<short-name>.png`, the same in every language.
- Keep a picture under about 400 KB.

## Build

```
cd docs/manual/build
npm install
node build.mjs en        # or: tr, es, pl, lv, all
```

Output: `docs/manual/dist/TOP4HoneyChains_User_Manual_<LANG>.docx` and `.pdf`. Needs Node 20 or newer. The PDF is written by Microsoft Word when it is installed, otherwise by LibreOffice; install LibreOffice if neither is there. The build stops with a clear message on a missing picture or an em-dash.

Do not restyle the template or replace it with another tool. If `build.mjs` fails on this machine, fix the failure and say what you changed. If you think the design should change, put it in the report.

## Institutions

The board on the cover and the last page ("Partners and supporters") show the same institutions as the launch film, approved by the project coordinator, with the ICT-AGRI-FOOD programme logo he asked for and the partners' own logo files from `docs/brand/partners`. Do not change names, order or logos, and do not redraw a logo. If the user asks for a change: edit `assets/institutions.json`, run `node assets/board/render_board.mjs` (it uses the Playwright installed in `capture/`), rebuild.

## Checks before you report a language

1. `node build.mjs <lang>` ends without warnings (it also names a page that holds only a few lines at the end of a chapter).
2. Turn the PDF pages into images and look at every page: no cut or blurred picture, no picture in the wrong language, no private or placeholder data, no label that differs between text and picture, no heading alone at the bottom of a page, no almost empty page that a small edit would avoid.
3. Carry out five tasks in the app exactly as written, one per role. Fix what does not match.
4. Search the text for em-dashes, for leftovers of another language and for names from the live data.

## Report

A few lines in English: files written, pages per language, what was left out and why, `reports/ui-issues.md` and `reports/missing-translations.md` in one line each, and questions for the user.
