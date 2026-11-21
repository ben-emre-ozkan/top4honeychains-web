# TOP4HoneyChains user manuals: brief

## Goal

Five user manuals for the web platform (`TOP4HoneyChains.Web`), one per language: English (en), Turkish (tr), Spanish for Argentina (es), Polish (pl), Latvian (lv). Each one is delivered as a Word file and a PDF, and each one is complete on its own.

The readers are the people who will use the platform in each partner country: staff of beekeepers' associations, laboratories, packagers, wholesalers and retailers, administrators, and consumers. They are not technical. The manuals describe the system as it is now.

## What is in this folder

| Path | What it is |
| --- | --- |
| `build/` | The template. `build.mjs` turns `content/<lang>/manual.md` into the Word file and the PDF: cover, contents table, chapter and figure numbers, header, footer, last page. |
| `assets/` | Institution data (`institutions.json`), logos, flags and the partners and supporters board for the cover (`board/`). Approved by the project coordinator; see "Institutions". |
| `capture/helpers.mjs` | Screenshot helpers (window size, marks with step numbers, cropping). Write the capture flows on top of them. |
| `content/en/` | A sample that shows every building block. Replace it with the real manual and delete the sample pictures. |
| `sample/` | The sample as PDF and Word, to see the layout. |

You create: `outline.md`, `glossary.md`, `capture/capture.mjs`, `content/<lang>/manual.md`, `content/<lang>/screens/`, `reports/`. The inventory from the first step is `inventory.md`. If `notes/` exists, it holds a reviewer's notes on an earlier manual: apply them.

## Rules

1. The code and the running application are the source of truth. Describe only what exists and works. Do not invent features, fields or messages. Where something is broken or unfinished, leave the task out or describe what really happens, and list it in `reports/ui-issues.md`.
2. Do not change application code, migrations or committed configuration in this task. Run the app for screenshots through environment variables and a separate database. A problem you would fix in code goes into `reports/ui-issues.md` instead.
3. Privacy. The local database `TOP4HoneyChainsDb` is a copy of the live system with real personal data. Nothing from it may appear in a picture. Use a separate database (for example `TOP4HoneyChainsDb_Manual`) that holds only fictional data; reference lists without personal data may be copied.
4. Sample data looks real: believable names of people, organisations and places, sensible quantities and dates. No "[DEMO]" tags, no accounts or records named "test", "demo" or "admin", no placeholder text. If the repository already has a presentation cast, use it. No photos of real people. E-mails and text messages must not reach real addresses or phones.
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

Adapt this to the inventory. Order the role chapters along the journey of the honey.

1. About this manual `{-}`: who it is for, how it is organised, the conventions (bold for labels, numbered steps, marks in pictures).
2. The platform at a glance: what it is for, the journey of the honey from the beekeeper to the jar, a table of roles (who does what), the control points in one table, what the consumer sees.
3. Getting started: open the platform, choose the language, sign in, forgotten password, the parts of the screen, sign out.
4. One chapter per role. It opens with what this role does on the platform and a short table "task, where to find it". Then one section per task. It ends with what happens next and who continues.
5. The Honey Story page: how a consumer reaches it and how to read each part of it.
6. Administration, if the role exists.
7. Problems and questions: only real cases (locked account, missing permission, a value the form rejects, file too large).
8. Glossary `{-}`.

Aim for 40 to 70 pages. Do not describe every field of every form; for a long form use a table: field, what to enter, required or not.

## Writing rules

- One section is one task, named as the user would say it, starting with a verb: "Record a honey delivery".
- A task has: one sentence on when you do it, numbered steps with one action each, and the result the user should see at the end.
- Labels of buttons, menus and fields are in bold and exactly as on the screen in that language. A menu path is **Menu > Item**.
- Speak to the reader directly. Short sentences. Active voice. Present tense.
- Name the roles as the interface names them.
- No internal words: controller, endpoint, database, table, ID, seed.
- Notes are rare. `[!NOTE]` for useful extra information, `[!TIP]` for a shortcut, `[!IMPORTANT]` for a rule that must not be missed, `[!WARNING]` when a mistake loses data or blocks someone.
- Every picture has a caption that says what the reader sees.

## Writing format

`content/en/manual.md` shows all of it. In short:

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

1. A step with a **Label**.
2. A step with its own picture (indent the picture by three spaces):

   ![Caption of the picture](screens/03-02-save.png)

![Caption](screens/03-01-list.png)                      a picture on its own line
![Caption](screens/06-01-story.png "width=42%")         a narrow picture with a set width
![Caption a](screens/a.png) ![Caption b](screens/b.png) two or three pictures side by side

> [!NOTE]
> Text of the note.

| Field | What to enter | Required |      tables as usual
`LOT-2026-0142`                              text the reader types
<!-- pagebreak -->                           a forced new page (rarely needed)
```

Do not write a cover, a contents table, chapter numbers, figure numbers ("Figure 3") or the partners page: the build adds them. Title and subtitle of the cover come from `build/strings.json` in the right language.

## Pictures

- Every picture is taken by `capture/capture.mjs` with the helpers, from the local app with the fictional database, in the language of the manual. No picture is taken or edited by hand.
- Window 1100 x 720 CSS pixels, scale 2. If the normal desktop layout needs more width, use the smallest width that works without a horizontal scroll bar and set the same number in `VIEWPORT` (helpers) and `viewport_width` (front matter of every `manual.md`). The build sizes every picture from these numbers, so text has the same size in all of them.
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

The board on the cover and the last page ("Partners and supporters") show the same institutions as the launch film, approved by the project coordinator. Do not change names, order or logos, and do not redraw a logo. If the user asks for a change: edit `assets/institutions.json`, run `node assets/board/render_board.mjs` (it uses the Playwright installed in `capture/`), rebuild.

## Checks before you report a language

1. `node build.mjs <lang>` ends without warnings.
2. Turn the PDF pages into images and look at every page: no cut or blurred picture, no picture in the wrong language, no private or placeholder data, no label that differs between text and picture, no heading alone at the bottom of a page, no almost empty page that a small edit would avoid.
3. Carry out five tasks in the app exactly as written, one per role. Fix what does not match.
4. Search the text for em-dashes, for leftovers of another language and for names from the live data.

## Report

A few lines in English: files written, pages per language, what was left out and why, `reports/ui-issues.md` and `reports/missing-translations.md` in one line each, and questions for the user.
