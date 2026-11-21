# User-manual inventory (TOP4HoneyChains.Web)

Analysis for the next edition of the user manuals, read from the code on `main` (after D-93, 2026-11-21). No code was changed.
Paths are relative to `TOP4HONEYCHAINS/TOP4HoneyChains.Web/` unless they start with `docs/` or `tools/`.

## 0. Feature flags decide what a user sees

`FeatureFlags` (`Services/OrgModel/CurrentOrg.cs:12-22`), bound from `Features`:

| Flag | `appsettings.json` | `appsettings.Development.json` | Effect |
|---|---|---|---|
| `OrgRoleModel` | false | true | Organizations, memberships, My work, the member and Set-up/Oversight menus. Org-model pages answer 404 when off (`[RequireOrgModel]`). |
| `CustodyIntake` | false | true | Sealed-container delivery form (`Intake/CreateCustody`) instead of the plain one. |
| `BeekeeperAccounts` | false | true | Beekeeper sign-in and the `/Me` portal. |
| `ConsumerAccounts` | false | true | `/Consumer` pages, sign-up, Save/feedback on the Honey Story, "Consumer feedback" menu. |

Other switches that change screens: `Sms:Provider=Fake` (adds "SMS outbox", enables the `/phone/...` test page), `Demo:Enabled` (demo banner, `/Demo/Inbox`), `Notices:Enforce`, `Account:PasswordResetChannel` (SMS default, or e-mail).

This inventory describes the app **with all four flags on** (the local run-demo set-up), and notes the flags-off differences briefly. See open question 1.

## 1. Roles, sign-in and the first page

### 1.1 Roles in the code

- **Identity roles**:
  - `Administrator` is the platform administrator (`CurrentOrg.AdminRole`; policy `PlatformAdmin`).
  - `Beekeeper` is added to beekeeper users, but nothing is authorized by it; portal access comes from the `BeekeeperAccounts` table.
  - `SampleAcceptanceOfficer` is a retired global role, no longer offered.
  - With the org model on, the only role offered on Users is Administrator (`Controllers/UserManagementController.cs:23,44-46`).
- **Organization roles** (`OrgRoles`, `Models/Entities/Organization.cs:350-370`). Names come from `RefText.RoleNames` and `ReferenceTranslations`, not from resx:

| Code | English name | Signing role* |
|---|---|---|
| OrgAdmin | Organization admin | yes |
| Staff | Staff | |
| FieldInspector | Field inspector | |
| SampleAcceptanceOfficer | Sample acceptance officer | |
| QualityApprover | Quality Assurance Officer | yes |
| Analyst | Analyst | |
| LabManager | Lab manager | yes |
| RegistrationOfficer | Beekeeper registration officer | yes |
| ComplianceOfficer | Compliance Officer | yes |

  \*Signing roles confirm the notice with an SMS code.

- **Organization capabilities** (`Capabilities`, `Organization.cs:333-347`; names from migration 026). They decide which menu items and pages an organization's members get:
  - Beekeeping (enterprise)
  - Association
  - CentralAssociation
  - Processing
  - Packing
  - Laboratory
  - Wholesale
  - Retail
  - PublicAuthority (reads everything)
  - PlatformAdmin
  - BeekeeperRegistration
- **Rights per role** are an editable matrix: Set-up → Roles and permissions. The defaults are in `Services/OrgModel/Permissions.cs:21-53`. CCP enter and approve rights per CCP come from Set-up → CCP responsibilities. A member works only after their organization is **Verified**.
- **Other actors**:
  - **Beekeeper**: an active `BeekeeperAccounts` row.
  - **Consumer**: a `ConsumerAccounts` row; no Identity role.
  - **Anonymous visitor**.

### 1.2 Sign-in and landing page

All users sign in on `/Account/Login` (`AccountController.cs:121-195`). The account locks for 15 minutes after 5 failed sign-ins.

| Who | How they get an account | After sign-in | What the first page shows |
|---|---|---|---|
| Platform administrator | Created on Users | `/MyWork` (flags off: `/Dashboard`) | "Hello, {name}" and waiting items: organizations to verify, registry, corrections, evidence. When empty: links to Organizations, Registry and Corrections. |
| Organization member | The admin adds a member and issues sign-in details by SMS (10-character temporary password, valid 72 h). | `/MyWork` | Items from `Services/OrgModel/MyWork.cs`: CCP sheets to enter or approve, new batch, finalize, receive samples, enter or approve lab results, confirm shipments, evidence, compliance, feedback. Items show only once the organization is verified. When empty: links to My organization and the allowed areas. |
| Beekeeper | The registration officer ticks "Also send the sign-in details" or uses "Send sign-in details" on the registration page. User name = business number (TR-BEE-…). | `/Me` (returnUrl ignored) | Hello, deliveries, deliveries without a price (to-do), last lab result, batch count, notices. Language = the SMS language unless the browser chose one (D-92). |
| Consumer | Self sign-up at `/Consumer/SignUp`, linked from Login ("New here? Create a free consumer account"). | `/Consumer` (or back to the Honey Story they came from) | Saved honeys and My feedback. |
| Anyone with issued credentials | - | `/Account/FirstPassword` first | Set your own password, then the landing page above. |

- **Already signed in:** opening Login shows "Continue as {name}" (D-91).
- **Notice gate:** pending notices are accepted at `/Notices/Accept`; signing roles confirm with an SMS code.
- **Password reset:** by SMS code to a verified phone (`SendResetCode`, `ResetWithCode`).
- **Boundary:** beekeepers outside `/Me` and consumers outside `/Consumer` get 403 (`BeekeeperBoundary.cs:17-55`).

With `OrgRoleModel` off, only Administrators can use the panel (v1.0 menu, `/Dashboard`); members are refused with `Login.NoPanelAccess`.

## 2. Menus and pages per role, in honey-flow order

Menus are built in `Views/Shared/_horizontal.cshtml`; member visibility rules are in `MemberMenu` (`Services/OrgModel/MyWork.cs:264-294`). Every panel user also has a user dropdown: Profile, Show the tour, Show animations, Logout. Members have a role chip to switch organization or role.

### 2.1 Organization member menu (shown by capability and right)

| Menu item (resx key) | Route | Shown when |
|---|---|---|
| My work (`Work.Title`) | /MyWork | always |
| My organization (`Org.MyOrganization`) | /OrgWorkspace | always |
| Deliveries (intake) (`Cust.Intakes`) | /Intake | Processing and Intake.Read |
| Samples and lab (`Lab.Samples`) | /Samples | Processing or Laboratory, and Samples.Read |
| Shipments and stock (`Cust.Shipments`) | /Shipments | Processing, Packing, Wholesale or Retail, and Shipments.Read |
| Beekeeper registration (`Reg78.Title`) | /Registration | BeekeeperRegistration and the Registration right |
| Compliance (`Co.Title`) | /Compliance | role ComplianceOfficer |
| Consumer feedback (`Feedback.Title`) | /ConsumerFeedback | ConsumerFeedback.Read and the ConsumerAccounts flag |
| Beekeeper pages (`P5.Storefronts`) | /Storefronts | Association and Storefront.Read |

### 2.2 Pages in flow order

Access abbreviations:
- **PU**: panel user, i.e. Administrator or an active member.
- **PA**: platform admin.
- **Verified**: the organization's status is Verified.

Views are `Views/<Controller>/<Action>.cshtml` unless noted.

**A. Beekeeper registration** (RegistrationController; RegistrationOfficer in a verified org with BeekeeperRegistration, or PA)

| Route | Action / view | Tasks |
|---|---|---|
| /Registration | Index | List the organization's registrations; "Find or register" by registry (AKS) or business number. |
| /Registration/New, /Registration/Reverify/{id} | New, Reverify → `Registration/Form` | Enter identity, phone, SMS language, education (optional), main purpose, type and association. Enter apiaries: the first on the map, more with "Add apiary" (D-93). Consents, attestation. "Send code" → the beekeeper reads the SMS code → "Confirm the registration". Fallback: a signed photo. |
| /Registration/Details/{id} | Details | Read the record and all apiaries. Add an apiary. "Send sign-in details" (and "Send again" with a tick, D-91). Download the evidence. The Compliance Officer accepts or rejects photo evidence (four-eyes). |

**B. Honey delivery (intake)** (IntakeController; SampleAcceptanceOfficer in a verified Processing org)

| Route | Action / view | Tasks |
|---|---|---|
| /Intake | Index | List deliveries with the beekeepers' asking prices. |
| /Intake/Create?number= | Create → `Intake/CreateCustody` | Find the beekeeper. Choose the apiary (every apiary with its hives; no pre-selection when there are several, D-93), honey type, period, date, origin, laboratory and counter-sample place. One row per container: type, net and gross kg, arrived sealed or unsealed, seals, sample jar and counter-sample jar seals, cap intact, reseal. Confirm with the beekeeper's SMS code (fallback: signed photo). |
| /Intake/Details/{id} | Details | Read the delivery. Resample after a rejected jar. Corrections partial. Compliance review of photo evidence. |
| /Intake/Labels/{id}?size=a4\|label | Labels | Print container and jar labels with QR. |
| /Intake/Form/{id} | Form | Download the signed delivery form. |

**C. Lots** (no lot page of its own)
- **My organization (`/OrgWorkspace`):** the lots table, with CCP chips, the "Control sheet" link, Flow and Recall.
- **`/Flow/Lot/{id}`:** read-only chain of a lot.
- **`/Recall/Lot/{id}`:** recall report, exportable.

**D. Laboratory**

| Route | Action / view | Who | Tasks |
|---|---|---|---|
| /Samples | Index | SampleAcceptanceOfficer, OrgAdmin, Analyst, LabManager | Sent and Received lists. **Receive** a jar (seal match, label, cap): Analyst or LabManager of a verified Laboratory. **Composite** builds the composite sample from received jars. |
| /LabReports/Edit/{id} | Edit | Analyst, then LabManager | Analyst: enter values, report number and PDF; Save; Submit. LabManager: approve or return (four-eyes). Blocked until CCP 0 is approved. Download the PDF. |
| /Samples/Create?lotId= | Create | - | Legacy lots only; custody lots redirect to the delivery. |

The analyst's daily entry point is **My work** (Results and LabApprove items).

**E. Processing: CCP monitoring** (CCPMonitoringController; rights per CCP from CCP responsibilities)

| Route | Tasks |
|---|---|
| /CCPMonitoring/Sheet?kind=raw&id= | Lot control sheet (CCP 2–3 on one page): enter by Staff, FieldInspector or SampleAcceptanceOfficer; **Approve sheet** by the QualityApprover in one action (D-88). |
| /CCPMonitoring/Sheet?kind=batch&id= | Batch control sheet (CCP 4–10). |
| /CCPMonitoring/RawHoneyProductionManageCCP?…, /FinalHoneyProductionManageCCP?… | A single CCP (CCP 0 and 1 only have this page): values, Not applicable, Approve. |
| /CCPMonitoring/Corrections | PA decides correction requests. |

**F. Batches** (OrgWorkspaceController; Processing or Packing org)

| Route | Tasks |
|---|---|
| /OrgWorkspace/NewBatch | Staff or OrgAdmin pick eligible lots and kg → the batch is created with its CCPs (D-88; the admin no longer creates batches). |
| /OrgWorkspace/Batch/{id} | See blockers. **Request finalize** (Staff or OrgAdmin), then **Finalize** (QualityApprover, a different person). After finalizing: open the Honey Story, download the QR (`/honeystory/qrcode?code=`). There is no unfinalize (D-14). |

**G. Packing, shipments, retail.** No jar, packing or retail page exists.

| Route | Tasks |
|---|---|
| /Shipments | Holdings; confirm or reject incoming shipments (Shipments.Approve). |
| /Shipments/Create?batchId= | Send a quantity to another organization (Staff or OrgAdmin). |
| /Shipments/Storage?batchId= | The receiving holder enters storage values (CCP 10), then approves them. |
| /Flow/Batch/{id}, /Recall/Batch/{id} | Batch chain, recall report. |
| /Storefronts, /Storefronts/Consent/{id}, /Card/{id}, /Prices/{id} | Association (OrgAdmin): record a beekeeper's consent for a public beekeeper page, withdraw it, print a QR card, see asking prices. |

**H. Public pages** (anonymous)

| Route | View | Notes |
|---|---|---|
| / | Home/Index | Landing page: How it works, Trust, For whom, Honey Story demo, Sign in, Verify a lab report; partner and funder logos in the footer. Language switch. |
| /honeystory/{qr} | HoneyStory/Index | The **Honey Story**, opened by scanning the batch QR. Finalized batches only (404 otherwise). Provinces only, never apiary pins (D-93 keeps this). Save and Feedback buttons when ConsumerAccounts is on. |
| /honeystory/parameter/{id}?qr=&lot= | ParameterDetails | Opened from a value tile. |
| /storefront/{code} | Storefront/Index | Public beekeeper page (with consent). |
| /verify-report?code= | Verify/Index | Confirms an approved lab report. |
| /privacy?for=&org= | Notices/Public | Privacy notices. |

`/honeystory/details/{qr}` has no inbound link (probably dead).

**I. Consumer** (`_ConsumerLayout`; menu: My honeys, Account; no language switch in its header)

| Route | Tasks |
|---|---|
| /Consumer/SignUp | Create an account (rate-limited, honeypot). |
| /Consumer | Saved honeys (remove), own feedback with answers. |
| /Consumer/Save?qr=, /Consumer/Feedback?qr= | Save a honey; send feedback with a category (one per batch, five per day). |
| /Consumer/Account | Phone and verify, language, password, download my data, delete the account. |

**J. Beekeeper portal** (`/Me`, `_MeLayout`; header has a language switch, D-92)

| Menu item | Route | Tasks |
|---|---|---|
| Home | /Me | Summary; the price to-do links to My deliveries. |
| My deliveries | /Me/Deliveries | Delivered lots, containers, seals; set the asking price per kg (editable until the lot is in a finalized batch). |
| Samples and lab | /Me/Samples | Lab results. |
| Batches with my honey, My Honey Story pages | /Me/Batches, /Me/Stories | Read batches made from their honey and open their Honey Story pages. |
| My apiaries | /Me/Apiaries | All apiaries: title, hives, pin. |
| Privacy and consents | /Me/Privacy | Public name, notifications, price to association, storefront. |
| Account | /Me/Account | Phone, language, password. |

**K. Organization pages for members**

| Route | Tasks |
|---|---|
| /OrgWorkspace | Organization card, warnings, lots and batches tables, New batch. |
| /Organizations/Details/{id} | Read their own organization; OrgAdmin or ComplianceOfficer upload documents. |
| /Compliance | ComplianceOfficer: review items, data-subject requests, pending evidence. |
| /ConsumerFeedback, /Details/{id} | OrgAdmin, QualityApprover, ComplianceOfficer: read, answer, close feedback. |

### 2.3 Platform administrator

Menu: My work, Dashboard, **Set-up**, **Oversight**. The administrator reads business data but does not enter it (intake redirects to My organization).

| Menu | Item | Route | Tasks |
|---|---|---|---|
| Set-up | Organizations | /Organizations, /Create, /Edit, /Details/{id} | Create an organization with its capabilities and lab accreditation; add or remove members; change status (verify; needs a document and a Compliance Officer); four-eyes exemption. |
| Set-up | Capabilities | /Capabilities | Overview. |
| Set-up | Roles and permissions | /Permissions?tab=members\|roles\|capabilities | Members, the rights matrix, capability rights. |
| Set-up | CCP responsibilities | /CcpResponsibilities | Per CCP: owner, approval, capabilities, enter and approve roles. |
| Set-up | Beekeeper registry and associations | /Registry | Link an association; enterprise number and proof; decide. |
| Set-up | Reference data | /ReferenceData | Honey types, periods, education levels, main purposes, beekeeping types, CCPs, parameter categories, parameters and limits. |
| Set-up | Translations of reference data | /ReferenceTranslations | Edit the translations. |
| Oversight | Records | /Records?tab=batches\|lots | All lots and batches; links to sheets, Flow and Honey Story. |
| Oversight | Audit log | /Audit | Filter; verify the hash chain. |
| Oversight | Users | /UserManagement | Create; issue credentials; unlock; lock a consumer; change password; delete. |
| Oversight | Notices and consent texts | /Notices | Publish notice versions. |
| Oversight | Data retention | /Retention | Report, then run. |
| Oversight | Corrections | /CCPMonitoring/Corrections | Decide CCP and delivery corrections. |
| Oversight | SMS outbox (test provider) | /Sms/Outbox | Only with the fake SMS provider. |

Dev and test only (not for the manual): `/phone/{key}/{digits}` (fake phone inbox; needs the fake SMS provider, not Production, and the key), `/Demo/Inbox`, `/Dev/Scenes`.

Legacy pages reachable only with `OrgRoleModel` off (v1.0 admin data-entry menu): Beekeepers, Apiaries, Raw Honey Productions, Wholesalers, Final Honey Productions (admin batch creation and finalize), Roles.

## 3. Localization

- **Cultures:** tr (default), en, es, pl, lv. The culture comes from the query string, then the cookie `.AspNetCore.Culture`, then Accept-Language (`Program.cs:48-72`). The beekeeper portal sets the SMS language at sign-in when no cookie exists (D-92).
- **UI strings:**
  - `Resources/SharedResource.{en,tr,es,pl,lv}.resx`. The key is the English text or a dotted key (`Reg93.AddApiary`); views use `IStringLocalizer L`.
  - Flash and validation messages are translated sentence by sentence by `Services/MessageTranslator.cs`.
  - Field names come from `Services/LocalizedMetadata.cs`.
  - DataTables and Parsley texts are in `Views/Shared/_I18nScripts.cshtml`.
- **Data-driven texts:**
  - Reference data: table `ReferenceTranslations` via `Services/RefText.cs`. It looks up the language, then English, then the base column.
  - Org role and capability names also come from the database.
  - SMS texts: `Services/Messaging/SmsText.cs`.
  - Notices: versioned per language in the database. If a language has no version, the English one shows, then the Turkish one.
- **Completeness:**
  - All five resx files have 3215 keys, none missing or empty. `ResourceTests` enforces this.
  - Values equal to English are mostly legitimate (Hash, SHA-256, Video). To check: es `Roles` / `Error` / `Floral`, pl `Administrator` / `Minimum`, lv `Licence` / `Me.Story` / `Province`.
  - **Review status:** Turkish is reviewed. es, pl and lv UI texts, reference translations and SMS texts are our drafts for partner review (GO-LIVE L62, D-51, `SmsText.cs`, `docs/manual/README.md`).
- **Where English can still appear:**
  - Reference rows added after migration 010 without a translation.
  - Notices with no version in that language.
  - Any future message without a resx key.
  - Hard-coded texts: the honeypot label "Website" on Consumer sign-up, and placeholders (`+90 5xx…`, `platform-2026-12-v1`). Brand names stay as they are.
  - One Turkish reference text still says "kuruluş" (D-91 open item; needs a migration).

## 4. Running locally with fictional data only

- **The local database `TOP4HoneyChainsDb`** is a copy of live with real personal data. It must not appear in screenshots.
  - The existing manual screenshots (`docs/manual/capture.mjs`, `shots.yml`) were shot against [DEMO] records in this database.
  - D-88 deleted those records and demo accounts, so `capture.mjs` and `demo-setup.mjs` no longer run.
- **`tools/live-like`** (Şile cast, D-88/D-90; `cast.json`, `Program.cs`, `walk.mjs`):
  - Realistic fictional people, 4 organizations, 2 beekeepers, a consumer, and a full history up to a finalized batch with a Honey Story.
  - "Şile Arıcılar Birliği" is a real association name, used with the owner's consent. The manual's database renames it and the directorate to "Akçakovan Arıcılar Birliği" and "Akçakovan İlçe Tarım ve Orman Müdürlüğü" (`capture/fictional-names.json`, decision of 2026-11-21).
  - Built through the app (`dotnet run --project tools/live-like`, about 17 s); logins are written to `~/Downloads/TOP4-local-logins.txt`.
  - **It writes into `TOP4HoneyChainsDb`**: the name is hard-coded and any other database is refused. So the cast sits next to real data. `run-demo.ps1` and `walk.mjs` also hard-code that database.
- **The D-86 demo database: the only fictional-only path today** (GO-LIVE L121-125):
  1. `Web.Tests/Demo/DemoBuildTests.Export_the_demo_package` with `DEMO_EXPORT_DIR` set writes `demo-database.sql`. That is the schema (`db/demo/schema.sql`), reference tables copied from the local database with user ids removed and privacy-scanned, plus a [DEMO] cast of four partner countries, with a demo marker.
  2. Create an empty LocalDB database and run that script.
  3. Start the app with that connection string, `Demo:Enabled=true`, `Sms:Provider=Fake` and `Sms:AllowFake=true`.
  - Limits:
    - `DemoSeed` predates D-88: batches are made by the admin, with no consumer and no control sheets.
    - Demo mode adds a banner and AR/PL/LV-DEMO registry numbers, which would show in screenshots.
- **Missing for "Şile cast on a fictional-only database":**
  - a database-name parameter in `tools/live-like/Program.cs`, `walk.mjs` and `run-demo.ps1`;
  - a step that creates schema plus reference data without the [DEMO] cast and marker (e.g. reuse `DemoApp` up to the seed);
  - the cleanup script lives outside the repo.

- **Done for the manuals (Phase 1, 2026-11-21):** `capture/setup-db.ps1` builds `TOP4HoneyChainsDb_Manual` on LocalDB: the schema of `db/demo/schema.sql`, the reference tables of the local database without any person id (the D-86 list), the platform organisation, then the Şile cast through the app with `tools/live-like` (new environment overrides: database, content root, logins file, password, public address). The app's files go to `%LOCALAPPDATA%\TOP4-manualpp`, not to the web project's `App_Data`. `capture/app.ps1` resets and starts that run on port 5256; `capture/capture.mjs` takes the pictures. No row of the local database other than reference data is read.

## 5. Existing manuals and guides

Dates are the last commit.

| Path | Date | Content | Still accurate |
|---|---|---|---|
| `docs/manual/{en,tr,es,pl,lv}/*.md`, built by `build.mjs` | 2026-10-28 (build.mjs 2026-10-12) | User manual, 17 chapters per language (about 28.5k words in EN): roles, beekeeper portal, platform admin, trust and privacy, glossary. `build.mjs` turns Markdown into branded HTML and a PDF per language (markdown-it, playwright-core, local Chrome); no Word. 56 screenshots per language. | **About 70%.** Outdated: batch creation by the admin (now New batch in the organization, D-88); control sheets missing (D-88); old admin menu (now Set-up/Oversight, D-88); no consumer chapter (D-87); "one delivery per apiary" refusal (D-89); wording "kuruluş", main purpose, sealed/unsealed, Continue as, info buttons, new tour (D-91); portal language switch (D-92); several apiaries (D-93). Screenshots of menus, batches and registration are stale and cannot be re-shot with `capture.mjs`. Chapters 4, 6, 7, 10, 12, 14, 15 and 17 need edits; 1–3, 8–9, 11, 13 and 16 are mostly fine. |
| `docs/manual/PLANNED-APP-CHANGES.md`, `REVIEW-RESPONSE-2026-10.md` | 2026-10-28 | Changes the manual assumed, and answers to the October review. | Historical; all shipped except A8. |
| `docs/partner-test/` (guide.md per language, `build.mjs`, `walkthrough.mjs`) | 2026-11-19 (only `walkthrough.mjs` updated) | Partner test pack D-86: "Follow one honey", 20 steps; PDF, HTML checklist and CSV. | **About 75%.** Step D2 (admin creates the batch) and D3 (CCP pages) are outdated; no consumer steps. |
| `docs/OZET-TR.md`, `docs/CCP-CURRENT.md` | 2026-10-09 | Turkish summary; CCP state. | About 60–65%. |
| `docs/RELEASE-NOTES-D78.md`, `SURUM-NOTLARI-D78.md` | 2026-10-28 | D-78 to D-85 notes. | Accurate for that release. |
| `docs/ROLE-MODEL.md` | 2026-11-18 | Role model. | About 90%. |
| `docs/test/END-TO-END-SCENARIO.md`, `demo_files/DEMO-WALKTHROUGH.md` (untracked) | 2026-10-08 / - | Old scenarios. | Obsolete: the [DEMO] records and the admin batch flow are gone. |
| `~/Downloads/TOP4-walkthrough-facts.md` (outside the repo) | 2026-11-19 | Owner's hands-on guide for the Şile cast, EN and TR labels. | Current up to D-91; D-92 and D-93 not in it. |

`docs/DECISIONS.md` has no row for D-92 (the D-92 commit exists).

## 6. Tools on this machine (checked 2026-11-21)

| Tool | Found |
|---|---|
| Node.js | v24.14.0 (npm 11.9.0) |
| Playwright | `playwright-core` 1.48.2 in `docs/manual/node_modules`. Browsers in `%LOCALAPPDATA%\ms-playwright` (chromium-1217). Chrome 157 installed (used by `build.mjs` and the scripts). The Playwright MCP server is configured, but its connection is unreliable. |
| Microsoft Word | Office 16 `WINWORD.EXE` 16.0.20625 (can be driven by COM for .docx → PDF) |
| LibreOffice | **not installed** (`soffice` not found) |
| pandoc | not installed |
| Python | 3.12.10 with `python-docx` 1.2.0 |
| .NET SDK | 8.0.419 and 10.0.103 |

## 7. Open questions

1. **Which configuration do the manuals describe?**
   - `docs/GO-LIVE.md` L86 says `OrgRoleModel` stays **off** at go-live; L43–47 keep BeekeeperAccounts, CustodyIntake and ConsumerAccounts off until later.
   - `CLAUDE.md` says the Development settings (all on) match the live server.
   - With all on, the manuals cover organizations, My work, the portal and consumers. With all off, they cover the v1.0 admin data-entry app. Which one, or both?
2. **Screenshot data.** Options:
   - (a) the D-86 fictional-only demo database: [DEMO] names and a banner, pre-D-88 seed;
   - (b) make `tools/live-like` and `run-demo.ps1` accept a fresh database built from schema plus reference data, then build the Şile cast there (small code change, separate work item);
   - (c) keep the Şile cast in the local copy and shoot only cast pages (the current practice, but not "fictional only").
   
   I recommend (b).
3. **Audience and split.** One manual per role (as today, 17 chapters), or shorter task guides per role? Do the consumer and the beekeeper get their own short guides?
4. **Languages for this edition.** All five, with es, pl and lv as drafts again, or EN and TR first?
5. **Output formats.** Word (.docx) and PDF per language. Build the .docx with python-docx or Word COM (both available), or install LibreOffice/pandoc? Keep the existing HTML/PDF `build.mjs` look?
6. **"Şile Arıcılar Birliği"** is a real association. Is it fine in published manual screenshots, or only in local demos? *Decided 2026-11-21: fictional organisation names only in the manual.*
7. **Stale material.** Retire or update `docs/test/END-TO-END-SCENARIO.md`, the old `capture.mjs` / `demo-setup.mjs`, and the partner-test steps D2/D3 as part of this work?
