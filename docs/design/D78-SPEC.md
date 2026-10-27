# D-78 design: changes from the coordinator meeting of 24 October 2026

Status: **design only**, written 2026-10-27. No code, database or reference data was changed for this document. The application is live at `https://top4honeychains.intalalab.net` with real data; every migration below is designed so that existing records stay valid.

Naming: the owner calls this work item "D-78". In `docs/DECISIONS.md` the number D-78 was already used (local test environment, 2026-10-19), so the decisions of this meeting are recorded there as **D-79**. The file names keep "D78".

Sources: the meeting notes (owner, 24 October 2026), `docs/manual/PLANNED-APP-CHANGES.md` (A1–A8, B1–B9, C1–C6), the role model ([ROLE-MODEL.md](../ROLE-MODEL.md)), the manual chapter 2 (roles and data rights) and the code on `main` at `5e1ed8a`.

Contents

1. [Overview and mapping](#1-overview-and-mapping)
2. [Organizations and roles](#2-organizations-and-roles)
3. [Data model](#3-data-model)
4. [Flows](#4-flows)
5. [SMS design](#5-sms-design)
6. [Privacy](#6-privacy)
7. [Explanatory animation (F16)](#7-explanatory-animation-f16)
8. [Promo videos (F15)](#8-promo-videos-f15)
9. [Phased plan](#9-phased-plan)
10. [Open decisions for the coordinator](#10-open-decisions-for-the-coordinator)

---

## 1. Overview and mapping

The meeting moves the platform from "the plant records everything about a beekeeper" to "the beekeeper is a verified person who confirms what happens to their honey". Four ideas carry the changes:

- **A verified start.** A district directorate of agriculture registers the beekeeper: registry number check, an SMS code to the beekeeper's phone, and the officer's attestation. The privacy notice and the beekeeper's consents are captured in the same step.
- **Proof at every hand-over.** Honey arrives in sealed containers (tins). The beekeeper confirms each delivery with an SMS code. A sample is taken from every container into a jar with a tamper-evident cap; the jar, its cap and its seal are checked again at the laboratory. Once confirmed, delivery data cannot be changed.
- **The beekeeper as a reader.** Beekeepers sign in to a portal with read-only access to their own data. The only thing they write is the price of their honey (plus their own consent choices and password, which are account actions, not business data).
- **Accountability inside every organization.** Each organization names its data controller and a **Compliance Officer**, and everyone who works in the platform accepts an information notice before their first use.

Decisions already made (final, recorded as D-79): real SMS through a Turkish provider behind an abstraction, with a fake provider and outbox for development and demos, e-mail stays disconnected; beekeepers can enter the price of their honey and nothing else; the role is called **Compliance Officer**.

Table 1. Feedback items → section → phase

| # | Item | Section | Phase |
|---|---|---|---|
| F1 | Landing page: logos in two groups, "Project partners" and "Funding organisations" | [4.11](#411-landing-page-logos-f1) | P4 |
| F2 | District directorate of agriculture; registration role (TOP officer) | [2.1](#21-new-capability-beekeeper-registration), [2.2](#22-roles-table), [4.1](#41-beekeeper-registration-f2-f3-f4) | P1 |
| F3 | How the beekeeper is verified: registry number, SMS code, officer attestation, signed photo as fallback | [4.1](#41-beekeeper-registration-f2-f3-f4) | P1 |
| F4 | Consent moves into registration: notice and explicit consent, confirmed by SMS (or signed photo) | [4.1](#41-beekeeper-registration-f2-f3-f4), [6.2](#62-notices-and-consents) | P1 |
| F5 | Apiary titles generated from province – district – apiary name | [3.6](#36-apiary-title-f5), [4.2](#42-apiary-at-registration-f5) | P1 |
| F6 | Intake: SMS code to the beekeeper, officer enters it; photo as fallback; officer enters the delivery date; WhatsApp later | [4.4](#44-intake-with-sms-confirmation-and-sealed-containers-f6-f10), [5](#5-sms-design) | P2 (WhatsApp later) |
| F7 | Data residency (EU / Türkiye, transfers) | [6.6](#66-data-residency-f7-for-decision) | decision, no phase |
| F8 | Everyone registered by an administrator accepts a notice; each organization names a data controller | [4.9](#49-notice-acceptance-at-first-sign-in-f8), [6.1](#61-roles-in-data-protection-data-controller-per-organization) | P1 |
| F9 | Compliance Officer in every organization | [2.1](#21-new-capability-beekeeper-registration), [4.8](#48-compliance-officer-duties-f9) | role in P1, duties in P3 |
| F10 | Chain of custody: delivered honey → containers → samples, delivery immutable, sealed containers | [3.2](#32-containers-and-seals-f10-f14), [3.10](#310-immutability-rules), [4.4](#44-intake-with-sms-confirmation-and-sealed-containers-f6-f10) | P2 |
| F11 | Beekeepers sign in, read-only, only the price is written | [2.4](#24-beekeeper-as-a-user-type-f11), [4.7](#47-beekeeper-portal-and-price-entry-f11-f17) | P3 (accounts issued in P1) |
| F12 | Credentials issued by whoever registers the user, sent by SMS, forced change at first sign-in | [4.3](#43-credentials-by-sms-f12) | P1 |
| F13 | Tamper-evident caps on sample jars, checked at sealing and at lab reception | [4.5](#45-sampling-per-container-with-tamper-evident-caps-f13-f14), [4.6](#46-lab-reception-checks-f13) | P2 |
| F14 | A sample from every container | [4.5](#45-sampling-per-container-with-tamper-evident-caps-f13-f14) | P2 |
| F15 | Two promo videos (consumer, beekeeper) | [8](#8-promo-videos-f15) | content work, alongside P4 |
| F16 | In-app explanatory animations | [7](#7-explanatory-animation-f16) | P4 |
| F17 | Beekeeper portal | [4.7](#47-beekeeper-portal-and-price-entry-f11-f17) | P3 |

PLANNED-APP-CHANGES (A1–A8, B1–B9, C1–C6) are folded into P4 and P5; [P5 in section 9](#p5-privacy-fixes-and-the-planned-app-changes-leftovers) lists each item, and [6.4](#64-how-f4-f8-and-f11-resolve-c2-c4-and-c5) says how F4, F8 and F11 resolve the privacy items C2, C4 and C5.

What this spec does not change: the CCP model, lab report approval (four-eyes, no exemption), batch finalizing, shipments and the recall report keep their current rules. Where D-78 touches them (sample per container, cap checks, lots that cannot be changed), the change is stated explicitly.

---

## 2. Organizations and roles

### 2.1 New capability: beekeeper registration

**District directorate of agriculture** (TR *İlçe Tarım ve Orman Müdürlüğü*) becomes a new kind of organization. Following the rule of the manual ("an organization's type is the set of capabilities it has"), it is modelled as a new capability, not as a new organization type:

| Capability code | Name (EN) | TR | ES | PL | LV | Ability |
|---|---|---|---|---|---|---|
| `BeekeeperRegistration` | Beekeeper registration | Arıcı kaydı | Registro de apicultores | Rejestracja pszczelarzy | Biškopju reģistrācija | Able to register beekeepers and their apiaries in its area, attest their identity and registry number, record their consent and issue their sign-in. |

- Scope: the organization's province and district (already on `Organizations`). An officer can register any beekeeper, but **lists and reads only the beekeepers that its own organization registered** (no browsing of other directorates' beekeepers). The registry number's province code (`TR` + 2 digits) is compared with the organization's province; a mismatch needs a reason (a beekeeper may keep bees in a neighbouring province).
- The capability is generic on purpose: in another country an agency or an association can be given the same capability.
- Typical organization: District directorate of agriculture → **Beekeeper registration** (optionally **Public authority** if it should also read the records of the chain, which is not the default).

**Compliance Officer** is a role in **every** organization, not tied to a capability (2.2). It is the organization's accountable person for approvals, sampling quality, agreements and data protection (4.8).

### 2.2 Roles table

The table follows the manual's [Table "Roles"](../manual/en/02-capabilities-organizations-and-roles.md#role-list). **[new]** marks a new row, **[changed]** a row whose work changes; changed text is in bold.

Table 2. Roles (after D-78)

| Role | Code | Typical person | Main work | Change |
|---|---|---|---|---|
| Organization admin | OrgAdmin | Manager of the organization | Uploads documents, requests finalizing, ships and confirms shipments, records beekeeper consents (associations), reads deliveries and samples, **reads the price of the lots its organization owns** | [changed] |
| Staff | Staff | Production, filling or retail staff | Enters CCP 1–10 data, requests finalizing, ships and confirms shipments, records storage | |
| Field inspector | FieldInspector | Association field technician | Reads records; enters CCP 0 only if an administrator gives this CCP to the association | |
| Sample acceptance officer | SampleAcceptanceOfficer | Reception officer at the plant | Records deliveries **with their sealed containers, sends the beekeeper's SMS code and enters it**, enters CCP 0, takes **a sample from every container** and sends samples | [changed] |
| Quality Assurance Officer | QualityApprover | Quality manager, food engineer | Approves CCP records and storage records, confirms finalizing | (renamed by A1, P5) |
| Analyst | Analyst | Laboratory analyst | Receives samples (**seal, cap and label check**), enters lab results, submits reports | [changed] |
| Lab manager | LabManager | Laboratory manager | Receives samples (**seal, cap and label check**), approves or returns lab reports | [changed] |
| **Registration officer** (working name; "TOP officer") | RegistrationOfficer | Officer of a district directorate of agriculture | **Registers beekeepers and apiaries, checks the registry number, sends the SMS code, attests the identity, records the notice and consents, issues the beekeeper's sign-in** | [new] |
| **Compliance Officer** | ComplianceOfficer | Quality or data-protection lead of any organization | **Oversees approvals and sampling quality, reviews fallback evidence (signed photos), keeps the organization's data controller details and notices, handles data-subject requests and consent withdrawals, reads the organization's audit entries** | [new] |
| **Beekeeper** (user type, not a membership role) | – (account linked to a Beekeeper record) | The beekeeper | **Reads their own apiaries, deliveries, samples, lab status, batches and Honey Story pages; enters the price of their honey; manages their own consents** | [new] |
| Platform administrator | (global role) | Platform operator | Administration; reads all workflow data but enters and approves none; **can also register beekeepers (same flow as the officer)** | [changed] |

Role names in five languages (to be reviewed by the partners):

| Code | EN | TR | ES | PL | LV |
|---|---|---|---|---|---|
| ComplianceOfficer | Compliance Officer | Uyum Sorumlusu | Responsable de cumplimiento | Specjalista ds. zgodności | Atbilstības speciālists |
| RegistrationOfficer | Registration officer | Kayıt Görevlisi (alternatives in [10](#10-open-decisions-for-the-coordinator)) | Responsable de registro | Specjalista ds. rejestracji | Reģistrācijas speciālists |
| Beekeeper (user) | Beekeeper | Arıcı | Apicultor | Pszczelarz | Biškopis |

The PL and LV forms follow the pattern of A1 ("Specjalista ds. …", "… speciālists").

Typical organizations (manual Table "Typical organizations", additions):

| Organization | Capabilities | Typical roles | Change |
|---|---|---|---|
| District directorate of agriculture | **Beekeeper registration** | Organization admin, Registration officer, Compliance Officer | [new] |
| Every organization | (any) | **+ Compliance Officer** (one person at least; can be the organization admin in a small organization, see 2.5) | [changed] |

### 2.3 Data rights (CRUD + Approve)

Same format and legend as the manual's [Table "Default data rights per role"](../manual/en/02-capabilities-organizations-and-roles.md#crud): **C** create, **R** read, **U** update, **D** delete, **A** approve. "–" no right. New columns and rows are marked **[new]**; a changed cell is in **bold**. Column abbreviations: OA org admin, St staff, FI field inspector, SAO sample acceptance officer, QAO Quality Assurance Officer, An analyst, LM lab manager, **RO** registration officer [new], **CO** Compliance Officer [new], **Bk** beekeeper user [new], PA platform administrator, Pub public visitor.

Table 3. Data rights per role (after D-78)

| Record | OA | St | FI | SAO | QAO | An | LM | **RO** [new] | **CO** [new] | **Bk** [new] | PA | Pub |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Beekeeper | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | **C R U** ¹⁸ | R ¹ | **R** (self) | C R U ² | R ³ |
| Apiary | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | **C R U** ¹⁸ | R ¹ | **R** (own) | C R U ² | R ³ |
| **Beekeeper registration (attestation, method, evidence)** [new] | – | – | – | – | – | – | – | **C R** ¹⁸ | **R A** ¹⁹ | **R** (own) | **C R** | – |
| **User credentials (issue, re-issue)** [new] | – | – | – | – | – | – | – | **C** ²⁰ | – | – | **C R** ²⁰ | – |
| Raw honey lot | R | R | R | C R ⁴ | R | – | – | – | R | **R** (own) | C R U D ⁵ **(no U of delivery facts once confirmed)** | R ³ |
| **Delivery confirmation (SMS code or signed photo)** [new] | R | – | – | **C R** | R | – | – | – | **R A** ¹⁹ | **A** ²¹ | R | – |
| **Container and its seals** [new] | R | R | R | **C R** | R | R | R | – | R | **R** (own) | R | – |
| Sample | R | – | – | C R **(per container)** | **R** | R A ⁶ | R A ⁶ | – | **R** | **R** (status, own) | R | – |
| Lab report and results | R | R | R | R | R | C R U D ⁷ | R A ⁷ | – | R | **R** (approved, own) | R | R ⁸ |
| CCP record | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | – | R | **R** (own lots, approved) | R ¹⁰ | R ³ |
| Batch | R, request finalizing | R, request finalizing | R | R | R, confirm finalizing (A) | R | R | – | R | **R** ²² | C R U D ¹¹ | R ³ |
| Shipment | C R A ¹² | C R A ¹² | R | R | R | R | R | – | R | – | R | – |
| Storage record (CCP 10) | R | C R U D | R | R | R A | R | R | – | R | – | R | – |
| **Honey price** [new] | **R** ²³ | – | – | – | – | – | – | – | **R** ²³ | **C R U** ²³ | R | – ²⁴ |
| Beekeeper consent | C R U ¹³ | R | R | R | R | R | R | **C R** ²⁵ | **R U** ²⁵ | **R U** ²⁵ (own) | R | R ¹⁴ |
| **Notice (text, version)** [new] | R | R | R | R | R | R | R | R | **C R U** ²⁶ | R | **C R U** ²⁶ | R |
| **Notice acceptance** [new] | R (own) | R (own) | R (own) | R (own) | R (own) | R (own) | R (own) | R (own) | **R** (organization) | R (own) | R | – |
| **Data controller details (organization)** [new] | R | R | R | R | R | R | R | R | **C R U** | R | **C R U** | R (in the notice) |
| **SMS message log** [new] | – | – | – | **R** (sent by self) | – | – | – | **R** (sent by self) | **R** (organization, masked) | – | **R** (masked) ²⁷ | – |
| Organization (data, documents) | R; documents C | R | R | R | R | R | R | R | R; **documents C** | – | C R U, status A ¹⁵ | – |
| Membership | R | R | R | R | R | R | R | R | R | – | C R U ¹⁶ | – |
| Audit entry | – | – | – | – | – | – | – | – | **R** (organization) ²⁸ | – | R ¹⁷ | – |

Notes 1–17 are the manual's notes and keep their meaning. New notes:

18. A registration officer creates and updates beekeepers and apiaries **registered by their own organization**. Identity facts that were attested (name, birth date, registry number, phone) can be changed only with a new attestation (SMS code to the beekeeper, 4.1).
19. **A** = accept or reject evidence recorded by the fallback path (a signed photo instead of an SMS code), by a second person. The Compliance Officer of the recording organization does it; a platform administrator can do it for the platform's own registrations.
20. Credentials are issued by whoever registers the user: registration officers for beekeepers they registered; platform administrators for members (and, as today, for any account). Nobody ever sees a password after it was sent.
21. The beekeeper's **A** is the SMS code they read out to the officer. It is not a sign-in action.
22. Batches that contain honey from the beekeeper's own lots: name, quantity share of their honey, status, Honey Story link. Never other beekeepers' names, the batch owner's internal data or prices.
23. The beekeeper enters a price per lot (3.8); the organization that owns the lot (its OA and CO) and platform administrators read it. Staff, laboratories, later holders and associations do not (see decision O-6).
24. Public pages show no price (decision O-7).
25. The registration officer records the consents given at registration; the beekeeper gives or withdraws an optional consent in the portal; the Compliance Officer records a withdrawal received on paper or by phone. The association keeps its current right to record storefront consent until the storefront consent moves into registration (P5).
26. Notice texts are versioned and never edited in place: "U" publishes a new version. The platform administrator keeps the platform notice; each organization's CO keeps the organization's notice.
27. Message bodies are stored with codes and passwords masked when a real provider is used; with the fake provider, the outbox shows the full text (5.3).
28. Audit entries whose `OrgId` is the CO's organization (read-only).

What can be deleted (additions): nothing new can be deleted. Containers, seal events, SMS messages, one-time codes, notices, acceptances, registrations and price entries are append-only or versioned.

### 2.4 Beekeeper as a user type (F11)

- A beekeeper user is an ASP.NET Identity account linked to exactly one `Beekeepers` row through the new table `BeekeeperAccounts` (3.7). It holds **no membership**: it is not a member of any organization and never appears in organization member lists.
- Sign-in opens the **beekeeper portal** (`/me`, 4.7), a separate area with its own layout. Every other panel area answers 403 to a beekeeper account.
- Authorization: a new policy `BeekeeperSelf`: the account is active, linked to an active beekeeper record, has accepted the current notice and has changed its first password. Every query in the portal is filtered by that beekeeper's id at the query level, never by a parameter from the page.
- Writes allowed: the price of their own lots (until locked, 3.8), their own optional consents (give or withdraw), their password and phone verification. Nothing else.
- The 63 locked legacy beekeeper logins on the local copy (`Beekeepers.BeekeeperId` = `AspNetUsers.Id`, role `Beekeeper`, locked by migration 001, D-09/D-23) are **reused**, not duplicated: when a registration officer re-verifies a legacy beekeeper, the existing account is linked, unlocked and given new credentials. Accounts that are never re-verified stay locked.
- D-23 ("beekeepers never log in") is superseded by D-79 once P3 ships.

### 2.5 Compliance Officer: placement rules

- At least one Compliance Officer per verified organization. The organization page shows a warning while it has none; P3 makes the role required for verification of new organizations (existing ones get a grace period, decision O-9).
- The CO can be the same person as the organization admin in a small organization (a second membership, as today). **Four-eyes still applies**: a CO never accepts fallback evidence they recorded themselves, and never accepts a delivery photo for a lot they received.
- The CO does not enter workflow data (deliveries, CCPs, lab results). It supervises.

### 2.6 Permission matrix additions

New activities for the manual's [Table "Default rights per activity"](../manual/en/02-capabilities-organizations-and-roles.md#permission-matrix) (R read, C create, A approve, –):

| Activity | Capability needed | OA | St | FI | SAO | QAO | An | LM | RO | CO | PA |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Register a beekeeper and apiaries | Beekeeper registration | – | – | – | – | – | – | – | C R | R | C R |
| Issue or re-issue a beekeeper's sign-in | Beekeeper registration | – | – | – | – | – | – | – | C | – | C |
| Accept fallback evidence (signed photo) | own organization | – | – | – | – | – | – | – | – | A | A (platform registrations) |
| Record a delivery with containers, send SMS code | Processing | R | – | – | C R | – | – | – | – | R | R |
| Take samples per container | Processing (lot owner) | R | – | – | C R | R | R | R | – | R | R |
| Receive a sample (seal, cap and label check) | Laboratory | – | – | – | – | – | A | A | – | R | R |
| Keep the organization's notice and data controller details | own organization | R | – | – | – | – | – | – | – | C R | C R |
| Read the organization's audit entries | own organization | – | – | – | – | – | – | – | – | R | R |
| Read the price of owned lots | Processing (lot owner) | R | – | – | – | – | – | – | – | R | R |

`RolePermissions` (D-56) gets new areas so that the administrator can move these rights like the existing ones: `Registration` (Enter, Read), `Evidence` (Approve), `Price` (Read), `Notices` (Enter), `OrgAudit` (Read). The fixed rules stay in code: a beekeeper's rights are not configurable, and nobody accepts their own evidence.

---

## 3. Data model

All business tables stay database-first: one new numbered, idempotent, additive SQL script per change in `db/migrations/`, starting at **018**, compatible with SQL Server 2019. The EF model (`Data/TOP4HoneyChainsContext.cs`, `Models/Entities/`) follows each script. Identity tables are **not** changed: everything that belongs to an account (first-password flag, language, notice acceptance) lives in business tables keyed by `UserId`, so no EF migration is needed.

Conventions used below: `UserId` columns are `nvarchar(450)` without FK (as today); times are UTC `datetime2`; every new table is written to the audit log by the existing interceptor; "append-only" tables also get a DB trigger like `TR_AuditEntries_AppendOnly` that refuses `UPDATE`/`DELETE` (except the columns named as "set once").

### 3.1 Overview

```
Organizations ─┬─ DataController columns (3.5)          Notices ── NoticeAcceptances (user or beekeeper)
               │
Beekeepers ────┼─ BeekeeperRegistrations (attestation, method, evidence)
   │           ├─ BeekeeperAccounts ── AspNetUsers ── UserCredentialIssues
   │           └─ Consents (extended)
   └─ Apiaries (Province, District, Locality, generated title)
         └─ RawHoneyProductions (lot) ── LotIntakes (confirmation, content hash)
               │                            └─ Containers ── ContainerSealEvents (append-only)
               │                                   └─ Samples (ContainerId, cap checks) ── SampleSources (composite)
               └─ LotPrices (append-only)
SmsMessages ── OneTimeCodes (purpose, subject, content hash)
```

### 3.2 Containers and seals (F10, F14)

`Containers` (new): one row per physical container of a delivery.

| Column | Type | Notes |
|---|---|---|
| ContainerId | int identity PK | |
| IntakeId | int FK → LotIntakes | |
| RawHoneyProductionId | int FK → RawHoneyProductions | redundant with the intake, for fast reads |
| ContainerCode | nvarchar(30), unique | `L{lot}-C{n}`, printed on the tin label with a QR |
| Kind | varchar(20) | `Tin` (default, teneke), `Drum`, `Bucket`, `Ibc`, `Other` |
| NetWeightKg | decimal(10,2) | > 0; the lot amount is the sum of the containers |
| GrossWeightKg | decimal(10,2) null | optional |
| OriginSealNumber | nvarchar(50) null | seal applied before delivery (as in Argentina); null if the tin arrived unsealed |
| OriginSealIntact | bit null | checked at intake |
| CreatedByUserId, CreatedAt | | set once |

`ContainerSealEvents` (new, **append-only**): the seal history of a container.

| Column | Type | Notes |
|---|---|---|
| EventId | int identity PK | |
| ContainerId | int FK | |
| Event | varchar(20) | `Checked` (origin seal checked at intake), `Applied` (plant seal applied), `Broken` (opened for sampling or processing), `Replaced` |
| SealNumber | nvarchar(50) | unique per organization among active seals |
| Intact | bit null | for `Checked` |
| UserId, At, Note | | |
| PhotoStoredFileName, PhotoSha256 | null | optional photo |

Lifecycle of one container: arrives (origin seal `Checked`, or a plant seal `Applied` in front of the beekeeper) → `Broken` for sampling → new seal `Applied` → stays sealed until it is emptied into a batch (`Broken`, reason "used in batch {code}").

### 3.3 Samples per container, caps and composites (F10, F13, F14)

`Samples` (existing) gets nullable columns, so all 11 existing samples stay valid:

| New column | Type | Notes |
|---|---|---|
| ContainerId | int null FK → Containers | null = legacy sample (lot-level) |
| Kind | varchar(20) null | `Container`, `Composite`; null = legacy (read as lot sample) |
| JarCapIntactAtSealing | bit null | the tamper-evident cap was down (not popped) when the jar was sealed |
| JarCapIntactAtReception | bit null | checked by the laboratory |
| LabelIntactAtReception | bit null | |
| SealNumberMatches | bit null | the seal number read at the laboratory matched the record |

`SampleSources` (new): composite sample → its container samples (many-to-many), for a lab that analyses a mix of the container jars (decision O-4). Insert-only; set when the composite is created and never changed.

Existing columns keep their meaning: `SealNumber` (the jar's seal label), `CounterSealNumber` and `CounterLocation` (the counter-sample jar kept at the plant), `SealIntact` (the seal label at reception).

### 3.4 SMS messages and one-time codes (F3, F4, F6, F12)

`SmsMessages` (new, append-only except the delivery status columns):

| Column | Type | Notes |
|---|---|---|
| SmsId | bigint identity PK | |
| Purpose | varchar(30) | `Registration`, `Credentials`, `IntakeConfirm`, `PasswordReset`, `NoticeConfirm`, `PhoneVerify`, `Info` |
| SubjectType, SubjectKey | varchar(30), nvarchar(64) | e.g. `LotIntake` / `123`, `Beekeeper` / id |
| ToPhoneMasked | varchar(20) | `+90******4567`; the full number is never stored here |
| ToPhoneHash | char(64) | HMAC-SHA256 of the E.164 number, for rate limits |
| Language | varchar(5) | tr, en, es, pl, lv |
| BodyStored | nvarchar(800) | with codes and passwords replaced by `******` for a real provider; full text for the fake provider (5.3) |
| Segments | tinyint | billed segments, as computed before sending |
| Provider | varchar(30) | `Fake`, `Logging`, `Netgsm`, `IletiMerkezi`, … |
| ProviderMessageId | varchar(64) null | |
| Status | varchar(20) | `Queued`, `Sent`, `Delivered`, `Failed`, `Expired`, `Fake` (set by delivery reports; the only updatable columns) |
| StatusAt, Error | | |
| SentByUserId, OrgId, CreatedAt | | |

`OneTimeCodes` (new):

| Column | Type | Notes |
|---|---|---|
| CodeId | bigint identity PK | |
| SmsId | bigint FK | the message that carried it |
| Purpose, SubjectType, SubjectKey | | the code is valid only for this purpose and subject |
| ContentSha256 | char(64) | hash of exactly what the message asked the person to confirm (4.4) |
| CodeHash | char(64) | HMAC-SHA256(server key, salt + code) |
| Salt | binary(16) | |
| ExpiresAt | datetime2 | created + 5 minutes |
| Attempts | tinyint | wrong entries, max 3 |
| LastAttemptAt | datetime2 null | time of the last wrong entry (P1 addition: pause of an officer after 10 wrong entries per hour, D-80) |
| UsedAt, UsedByUserId | null | set once |
| VoidedAt, VoidReason | null | replaced by a resend, too many attempts, or content changed |

`LotIntakes.SmsStatus` and `SmsAt` (from D-41) stay for the legacy rows and are no longer written; new rows use `SmsMessages`.

### 3.5 Notices, acceptances and data controller (F4, F8)

`Notices` (new, versioned, never updated after publishing):

| Column | Type | Notes |
|---|---|---|
| NoticeId | int identity PK | |
| Kind | varchar(30) | `PrivacyNotice` (aydınlatma metni), `ConsentText` (açık rıza metni, one per consent purpose), `TermsOfUse` |
| Audience | varchar(20) | `Beekeeper`, `Member`, `All` |
| OrgId | int null | null = the platform's notice; otherwise the organization as data controller |
| Purpose | varchar(30) null | for `ConsentText`: `PublicName`, `PublicPhoto`, `PortalNotifications`, `TransferAbroad`, `WhatsApp` |
| Version | varchar(30) | e.g. `platform-2026-11-v1` |
| Language | varchar(5) | one row per language of the same version |
| Body | nvarchar(max) | |
| BodySha256 | char(64) | |
| PublishedAt, PublishedByUserId, RetiredAt | | a new version retires the previous one |

`NoticeAcceptances` (new, append-only):

| Column | Type | Notes |
|---|---|---|
| AcceptanceId | bigint identity PK | |
| NoticeId | int FK | the exact version and language shown |
| UserId | nvarchar(450) null | a signed-in user |
| BeekeeperId | nvarchar(450) null | a beekeeper at registration (no account yet); exactly one of UserId / BeekeeperId |
| OrgId | int null | membership context, if the notice belongs to an organization |
| Method | varchar(20) | `Click` (signed-in user), `SmsCode`, `SignedPhoto`, `Paper` |
| CodeId | bigint null FK | for `SmsCode` |
| EvidenceStoredFileName, EvidenceSha256 | null | for `SignedPhoto` / `Paper` |
| RecordedByUserId | nvarchar(450) | the user, or the officer who recorded it |
| At, ClientIp | | |

`Consents` (existing, D-43): **extended**, not replaced. New nullable columns: `Method` (`SignedDocument` for the 4 existing rows when read as null, `SmsCode`, `SignedPhoto`, `Portal`), `CodeId`, `NoticeId` (the consent text version), `RegisteringOrgId` (the directorate when given at registration). `Purpose` gains the values listed under `Notices.Purpose`; `AssociationOrgId` becomes nullable (a consent given at registration has no association). Making a `NOT NULL` column nullable is a widening, allowed by the migration rules. The document columns become nullable too (an SMS consent has no document).

`Organizations` (existing) gets nullable columns for the **data controller**: `DataControllerName` (legal entity), `DataControllerAddress`, `DataControllerContact` (e-mail or phone for data-subject requests), `VerbisNumber` (Türkiye's data controller registry, optional), `PrivacyNoticeId` (current notice). Null on existing organizations until their CO fills them; P1 shows a "missing" badge.

### 3.6 Apiary title (F5)

`Apiaries` (existing) gets nullable `Province` (plate code, FK-like to `Provinces`), `DistrictId` (to `Districts`), `Locality` (nvarchar(60): village, plateau or field name, typed) and `TitleSource` (`Generated` / `Typed`; null = typed, legacy).

- The title is generated and stored in the existing `ApiaryTitle` (nvarchar(50)): **"{Province} – {District} – {Locality}"**, e.g. "Giresun – Dereli – Kümbet Yaylası". If it is longer than 50 characters, the locality is shortened with "…"; the full parts stay in their columns. A second apiary with the same three parts gets " 2", " 3".
- The form has no title field any more. The province is proposed from the map pin (the server already computes provinces from coordinates for the public map, D-72); district and locality are chosen and typed.
- Legacy apiaries (71) keep their typed titles (`TitleSource` null). Editing a legacy apiary proposes the generated title; it is applied when the user saves with province, district and locality filled in. Nothing is rewritten by the migration.
- The meeting's "apiary name" is read as the locality name. Batch titles are not covered by F5 (decision O-11).

### 3.7 Beekeeper ↔ user link, registration and credentials (F2, F3, F11, F12)

`BeekeeperAccounts` (new): `BeekeeperId` (PK, unique), `UserId` (unique), `LinkedAt`, `LinkedByUserId`, `Status` (`Active`, `Suspended`). One beekeeper, one account. The legacy implicit link (`BeekeeperId` = `AspNetUsers.Id`) is **not** backfilled by the migration; a link row is written when the beekeeper is re-verified (4.1, step 9), so no locked account becomes usable without a new attestation.

`BeekeeperRegistrations` (new, append-only): one row per attestation.

| Column | Notes |
|---|---|
| RegistrationId, BeekeeperId | |
| RegisteringOrgId, OfficerUserId, At | the directorate and the officer |
| Kind | `New`, `Reverification` (legacy beekeeper), `IdentityChange` (name, birth date, registry number or phone changed) |
| RegistryNumber, RegistryFormatOk, RegistryProvinceMatches | snapshot of the check |
| PhoneE164Hash | the number the code went to (hash; the number itself stays on `Beekeepers`) |
| Method | `SmsCode` or `SignedPhoto` |
| CodeId | for `SmsCode` |
| EvidenceStoredFileName, EvidenceSha256 | the signed form photo (fallback) |
| OfficerAttestation | bit, always 1; the text attested ("I have seen the beekeeper's ID document and the registry record") is versioned in `Notices` (Kind `Attestation`) |
| EvidenceStatus | `Accepted` (SMS), `PendingReview` / `Accepted` / `Rejected` (photo) |
| ReviewedByUserId, ReviewedAt, ReviewReason | second person (CO), photo path only |

A registration with `Accepted` evidence sets `BeekeeperRegistry.Status` = `Verified` (method "Officer attestation"), which replaces the manual document verification of D-29 for beekeepers registered by a directorate. The existing manual path stays for platform administrators.

`Beekeepers` (existing) gets nullable `PreferredLanguage` (varchar(5), null = tr) for SMS texts and the portal.

`UserCredentialIssues` (new, append-only except `PasswordChangedAt`): `IssueId`, `UserId`, `IssuedByUserId`, `IssuedAt`, `Channel` (`Sms`), `SmsId`, `ExpiresAt` (+72 h), `PasswordChangedAt` (set once). A user whose latest issue has no `PasswordChangedAt` must change the password before anything else; after `ExpiresAt` the temporary password is refused. This replaces a column on `AspNetUsers`, so Identity stays untouched.

`UserSettings` (new, small): `UserId` PK, `Language`, `PhoneVerifiedAt` (the Identity `PhoneNumberConfirmed` flag is also set; this keeps the date and the code id).

### 3.8 Honey price (F11, F17, decisions)

`LotPrices` (new, **append-only**; the current price is the latest row of a lot):

| Column | Type | Notes |
|---|---|---|
| PriceId | int identity PK | |
| RawHoneyProductionId | int FK | **per lot** (one delivery = one lot today), see O-5 for "per delivery" vs a price list |
| PricePerKg | decimal(10,2) | > 0, < 100 000 |
| Currency | char(3) | ISO 4217; default from the lot's origin country (TRY for TR, ARS, PLN, EUR); the beekeeper can choose another |
| Basis | varchar(20) | `Asking` (the beekeeper's price) — only value in P3; `Agreed` reserved for a later confirmation by the buyer (O-5) |
| Note | nvarchar(200) null | |
| EnteredByUserId, EnteredAt | | the beekeeper's account |
| ClientIp | | |

Rules: entered only by the beekeeper linked to the lot's apiary; allowed from the moment the delivery is confirmed until the lot is in a finalized batch (then locked). Each change is a new row; history is shown to the beekeeper and the lot owner. Read rights in 2.3 note 23. Not shown on the Honey Story (O-7). Audit: the interceptor logs each row; the price is commercial data, so it is excluded from the recall report and from the public API surface.

### 3.9 Delivery confirmation on the intake

`LotIntakes` (existing) gets nullable columns:

| Column | Notes |
|---|---|
| ConfirmationMethod | `SmsCode`, `SignedPhoto`; null = legacy (before D-78: signed form photo only) |
| ContentSha256 | hash of the canonical delivery content (4.4, step 6) |
| ConfirmedAt, CodeId | set once |
| FallbackReason | why the photo path was used |
| EvidenceStatus, ReviewedByUserId, ReviewedAt | photo path: CO review (4.4) |
| ContainersRecorded | bit; 1 for intakes with containers; null = legacy |

`FormStoredFileName`, `FormOriginalFileName` and `FormSha256` become **nullable** (the photo is now the fallback, not mandatory). This widens the columns; existing values stay.

### 3.10 Immutability rules

| What | Locked when | How a mistake is corrected |
|---|---|---|
| Delivery facts: beekeeper, apiary, honey type, period, delivery date, origin, quantity, containers (code, kind, net weight, origin seal) | the intake is **confirmed** (SMS code, or photo accepted) | Two-administrator correction (existing process) that **appends** a correction record; the beekeeper gets an `Info` SMS with the change. Never an in-place edit. |
| Seal events | always (append-only, DB trigger) | a new event (`Replaced`) with a note |
| Samples: container, seal and counter-seal numbers, cap at sealing, taken by/at | on creation | a sample recorded wrongly is **rejected** (existing status) and a new sample is taken; never edited |
| Reception checks (cap, label, seal match) | on reception | none; a rejected sample stays rejected |
| SMS messages, one-time codes, acceptances, registrations, credential issues, price rows | always (append-only) | a new row |
| Notices | on publishing | a new version |
| Lot (`RawHoneyProductions`): apiary, honey type, period, amount, date | confirmed intake (new) or finalized batch (existing) | as above; the admin Edit page shows these fields read-only for confirmed lots |

Enforcement: in the application (services refuse changes, pages show read-only fields), in the database for append-only tables (triggers), and by the content hash: `ContentSha256` of a confirmed intake is recomputed by the recall report and by a nightly check; a mismatch is shown as "changed after confirmation" and logged.

### 3.11 Existing live records

| Records (local copy counts) | After the migrations | Shown as |
|---|---|---|
| 70 beekeepers, 63 with a locked legacy login | unchanged; no `BeekeeperRegistrations` row | "Registered before D-78" badge; a registration officer can re-verify (4.1) |
| 66 beekeepers with a phone number | unchanged | the number is used only after an SMS code confirms it |
| 71 apiaries | typed titles kept, no province/district/locality | title as today; generated on the next edit with the parts filled in |
| 73 lots, 12 intakes, 11 samples | no containers; `ContainersRecorded` null; samples without `ContainerId` | "Legacy lot: recorded before sealed containers"; finalizing does **not** require containers for lots whose intake is legacy or that have no intake (admin-created lots) |
| Intakes with a signed form photo | `ConfirmationMethod` null | "Signed delivery form (before D-78)" |
| 4 storefront consents | `Method` null = signed document, purpose `Storefront` | unchanged; still valid until withdrawn |
| 74 users, 12 memberships | no acceptances | must accept the current notice at their next sign-in (4.9); nothing else changes |
| 12 organizations | no data controller details | "Data controller missing" badge for the CO and the platform administrator |
| 11 batches, finalized ones public | unchanged | Honey Story names: see C5 and decision O-8 |

The new rules apply only to records created after the release: an intake created after P2 must have at least one container, an SMS confirmation or an accepted photo, and one sample per container before CCP 1 can be filled.

### 3.12 Migrations

| Script | Phase | Content | Live data |
|---|---|---|---|
| `018_d78_roles_capability.sql` | P1 | capability `BeekeeperRegistration`; org roles `RegistrationOfficer`, `ComplianceOfficer`; their names and descriptions in five languages (`ReferenceTranslations`); `RolePermissions` seeds for the new areas | insert-only |
| `019_d78_sms_otp.sql` | P1 | `SmsMessages`, `OneTimeCodes`, append-only triggers | new tables |
| `020_d78_notices.sql` | P1 | `Notices`, `NoticeAcceptances`, organization data controller columns, `Consents` new columns and widened columns | additive; no row changed |
| `021_d78_beekeeper_accounts.sql` | P1 | `BeekeeperAccounts`, `BeekeeperRegistrations`, `UserCredentialIssues`, `UserSettings`, `Beekeepers.PreferredLanguage` | additive |
| `022_d78_apiary_title.sql` | P1 | `Apiaries.Province`, `DistrictId`, `Locality`, `TitleSource` | additive; titles not touched |
| `023_d78_containers_samples.sql` | P2 | `Containers`, `ContainerSealEvents`, `SampleSources`, new `Samples` and `LotIntakes` columns, nullable form columns, triggers | additive; widening only |
| `024_d78_lot_prices.sql` | P3 | `LotPrices`, trigger | new table |
| `025_d78_compliance.sql` | P3 | `DataSubjectRequests` (4.8), `ComplianceReviews` (sampling and evidence reviews) | new tables |
| `026_d76_reference_texts.sql` | P5 | A1, A3, A5, A6 texts (role and capability names and descriptions, five languages) | reference texts only |

Each script aborts if its predecessor is missing (as 005–008 do), prints what it created, and changes nothing on a second run. `docs/GO-LIVE.md` gets one line per script when it is written.

---

## 4. Flows

Each flow names the role, the screen and what is stored. "Code" is the 6-digit SMS one-time code of section 5.

### 4.1 Beekeeper registration (F2, F3, F4)

Actor: registration officer (or platform administrator), with the beekeeper present at the directorate or in the field.

1. **Find first.** The officer enters the registry number (`TR` + province code + 10 digits, D-29). The format is checked at once; the province code is compared with the directorate's province (a mismatch asks for a reason). If a beekeeper with this number exists, the officer is offered **Re-verify** (legacy beekeeper, step 9) instead of a duplicate.
2. **Identity.** First and last name, birth date, mobile phone (one field, stored as E.164 with the phone rules of 2026-10-19), education, beekeeping type and purpose, preferred language (default from the phone's country). Profile photo is optional and never public.
3. **Apiaries.** At least one apiary (4.2).
4. **Notice and consents.** The screen shows the information notice of the directorate (data controller) and of the platform in the beekeeper's language, and the optional consents with explicit yes/no choices, nothing pre-ticked:
   - name on public pages (Honey Story, beekeeper page): yes/no (resolves C5);
   - apiary photos on public pages: yes/no (only if photos are ever enabled, D-58);
   - portal notifications by SMS: yes/no (deliveries and lab results; codes are always sent, they are part of the service).
   The officer can print the same text (PDF with a form number) for the fallback path.
5. **Officer attestation.** A check box with the versioned text: "I have seen the beekeeper's identity document and the registry record, and the beekeeper has read or heard the notice."
6. **SMS code.** **Send code** sends message M1 (5.5) to the phone: who registers them, the notice link, their public-name choice, and the code. The content hash covers name, birth date, registry number, phone, apiaries and the consent choices; changing any of them voids the code.
7. **Confirm.** The beekeeper reads the code aloud; the officer types it. On success, in one transaction: the beekeeper and apiaries are saved, `BeekeeperRegistrations` (method `SmsCode`, evidence `Accepted`), `NoticeAcceptances` (method `SmsCode`), one `Consents` row per **yes**, `BeekeeperRegistry.Status` = `Verified`, phone marked verified.
8. **Fallback: signed photo.** Allowed when the beekeeper has no mobile phone, the SMS is not delivered within 2 minutes after one resend, or the beekeeper refuses SMS; a reason is required. The officer prints the form (identity, apiaries, notice, choices, form number), the beekeeper signs, the officer uploads a photo (PDF/JPG/PNG/WEBP, checked like today's uploads). The registration is saved with evidence `PendingReview`; the beekeeper's registry status stays `Pending` and the beekeeper cannot be used for an intake until the directorate's **Compliance Officer** accepts the photo (My work task; four-eyes). No portal account is issued without a verified phone.
9. **Re-verify (legacy beekeepers).** Same screen prefilled from the existing record; steps 4–7 are done again; on success the legacy login (if any) is linked (`BeekeeperAccounts`) and handled in 4.3.
10. **Credentials.** If the beekeeper wants portal access (default yes when the phone is verified), the officer clicks **Issue sign-in** (4.3).

Changing an attested fact later (name, birth date, registry number, phone) is an `IdentityChange` registration: a new code to the (new) phone, and to the old phone as an `Info` message.

### 4.2 Apiary at registration (F5)

1. The officer places the pin on the map (the map picker of 2026-10-19) or types coordinates; hive count.
2. Province is proposed from the pin, district is chosen from the province's districts, locality is typed (village, plateau, field).
3. The title is shown read-only as it will be saved: "Giresun – Dereli – Kümbet Yaylası". There is no title field.
4. Apiaries are saved with the registration (step 7 above). Later changes by the officer or an administrator regenerate the title; the lots keep pointing to the same apiary id.

### 4.3 Credentials by SMS (F12)

Applies to every account: beekeepers (issued by the registration officer) and members (issued by the platform administrator who creates the user; decision O-10 for organization admins).

1. The issuer creates the account (or links a legacy one). User name: for beekeepers the business number (`TR-BEE-2026-000068`, already their login on legacy accounts); for members chosen by the administrator. A verified mobile number is required.
2. The platform generates a **temporary password**: 10 characters from an unambiguous alphabet (no 0/O, 1/l/I), ≥ 50 bits. It is never shown to the issuer.
3. Message M2 (5.5) sends user name and temporary password. The stored body masks the password. `UserCredentialIssues` records the issue, valid 72 hours.
4. **First sign-in:** the user signs in with the temporary password and must at once (a) set a new password, (b) accept the notice(s) (4.9). Nothing else opens before both are done.
5. Expired or lost: the issuer clicks **Re-issue** (a new temporary password; the old one stops working). There is no "show password".
6. Password policy for all accounts (raised from today's 6 characters): at least 10 characters, not equal to the user name, not in a list of common passwords; lockout stays 5 tries / 15 minutes.
7. **Forgot password** (e-mail stays disconnected): the user enters the user name; if the account has a verified phone, message M4 sends a code; the code plus a new password resets it. The response is the same whether or not the user exists. Without a verified phone: ask the issuer for a re-issue.

### 4.4 Intake with SMS confirmation and sealed containers (F6, F10)

Actor: sample acceptance officer of a verified Processing organization; the beekeeper is present (or reachable by phone).

1. **Find the beekeeper** by business or verified registry number (as today). Beekeepers with registry status `Pending` or `Rejected` cannot deliver.
2. **Delivery** (today's fields): apiary, honey type, period, origin country, province/district of origin, and the **delivery date entered by the officer** (not in the future; the time of recording is kept separately, as today).
3. **Containers.** One row per container: kind (tin by default), net weight, origin seal number and "seal intact" if it arrived sealed, otherwise a plant seal number applied now in front of the beekeeper. A scanner or typed code; the platform prints the container labels (`L{lot}-C{n}` + QR). The lot quantity is the sum of the net weights (no separate quantity field).
4. **Samples** (4.5) are taken now, one per container (D-42: sampling at intake).
5. **Summary** screen: beekeeper, apiary, date, containers with seal numbers, samples with jar seal numbers.
6. **Send code.** The platform computes `ContentSha256` over a canonical text of the summary (ids, numbers, weights, seal numbers, date) and sends M3 (5.5): organization, date, number of containers, kilograms, honey type, number of samples sealed, and the code. Any change to the summary after sending voids the code.
7. **Confirm.** The beekeeper reads the code aloud, the officer types it. In one transaction: lot, intake (`ConfirmationMethod` = `SmsCode`, `ConfirmedAt`, `ContentSha256`), containers, seal events, samples; CCP assignments as today. From here the delivery is locked (3.10). The content hash is also written into the audit entry, so the hash chain protects it.
8. **Fallback: signed photo** of the printed delivery form (today's path) when the SMS cannot be used (same reasons as 4.1 step 8, reason required). The lot is created and can be worked on, but it is marked "Confirmation: photo, pending review" and cannot be **finalized** in a batch until the plant's Compliance Officer accepts the photo (four-eyes: not the officer who recorded the intake).
9. **Beekeeper not present** (delivery by a driver): the code goes to the beekeeper's phone; the driver or the officer calls the beekeeper, who reads the code back by phone. The code stays valid 5 minutes and can be resent twice.
10. WhatsApp (later): the same message as a WhatsApp template where the beekeeper chose WhatsApp (5.4); the code rules are the same.

Before P2, the intake form keeps working as today; the switch is per release, not per organization.

### 4.5 Sampling per container with tamper-evident caps (F13, F14)

1. For each container: break the seal (event `Broken`), draw the sample into a jar with a **tamper-evident cap** (a pop-up button that rises once the vacuum seal is broken), close it, check that the button is **down**, and put a numbered seal label over cap and jar. Fill the counter-sample jar the same way (kept at the plant, `CounterSealNumber`, `CounterLocation`).
2. Record per container: jar seal number, counter seal number, **"cap intact at sealing"** (must be yes; a jar whose cap is not down is discarded and a new one filled), and the new seal of the container (`Applied`).
3. The platform refuses a second sample record for the same container unless the first was rejected, and refuses confirmation (4.4 step 7) while a container has no sample.
4. **Sending:** all container jars of a lot go to one laboratory (existing "send" with the lab chosen); custody event `Sent` per jar.
5. Proof that a sample came from the delivery: jar → container (`ContainerId`) → intake (`IntakeId`) → confirmed content hash, which includes the jar and container seal numbers and which the beekeeper confirmed with the code. A substituted jar has a seal number that is not in the confirmed content; a substituted tin has no matching seal history.

### 4.6 Lab reception checks (F13)

Actor: analyst or lab manager of the receiving laboratory.

1. Scan or type the jar's sample code.
2. Check and record: **seal number matches** (the label reads the recorded number), **label intact**, **cap intact** (button still down). All three are required answers.
3. All yes → `Received`; any no → `Rejected` with a reason (existing). A rejected jar ends its chain: the plant must take a new sample from the same container, which means breaking the container's seal again (a visible seal event).
4. If the laboratory analyses a **composite** (decision O-4), it creates the composite sample from the received jars of the lot (`SampleSources`); the report is written on the composite; the container jars are retained.
5. Everything else stays: four-eyes on reports, approval by the lab manager, CCP 1 from the approved report, results only after CCP 0 is approved.

### 4.7 Beekeeper portal and price entry (F11, F17)

Area `/me`, mobile-first, the beekeeper's language. Read-only except where marked.

| Screen | Content |
|---|---|
| **Home** | Open items (price not entered, notice to accept), last delivery, last lab result, a "your honey in batches" count |
| **My apiaries** | Title, province, district, hive count, map pin (only the beekeeper sees the exact pin) |
| **My deliveries** | Per delivery: date, organization, containers with seal numbers, quantity, confirmation method and time; **price** (entry) |
| **Samples and lab status** | Per container jar: sealed, sent, received (cap check), report status (shown only when approved), and the approved values with their verdict where the limit is verified (D-18) |
| **Batches with my honey** | Batch title, status, my share in kg and %, Honey Story link once finalized; no other beekeepers' names |
| **My Honey Story pages** | Links to the public pages that contain their honey, and how they appear (name or neutral code) |
| **Privacy and consents** | The notices they accepted (with version and date), each optional consent with **Give** / **Withdraw** (withdrawal effective at once, as D-30), and the data controller contacts of each organization holding their data |
| **Account** | Change password, phone (changing it needs a code to the new number), language |

**Price entry** (the only business write):

1. On a delivery in "My deliveries", **Enter price**: price per kg, currency (default by origin), optional note. A confirmation dialog shows the total (price × confirmed quantity).
2. Saved as a new `LotPrices` row; the lot owner's OA and CO see it on the lot page and in a price column of their intake list.
3. Changes are allowed until the lot is in a finalized batch; every change is kept and shown as history.

### 4.8 Compliance Officer duties (F9)

A **Compliance** section in My organization, and tasks in My work:

| Duty | What the CO does in the platform |
|---|---|
| Approval mechanism | Sees which roles enter and approve which records in their organization (read-only view of the CCP responsibility matrix and role permissions); requests changes from the platform administrator (the configuration stays with the administrator); sees approvals waiting longer than N days and self-approvals under the four-eyes exemption (flagged entries) |
| Sampling quality | Reviews every intake confirmed by photo (accept/reject), every rejected sample, every container with a broken seal outside sampling, and cap or seal mismatches reported by labs; writes a `ComplianceReviews` row with finding and action |
| Evidence | Accepts or rejects fallback photos of registrations (directorates) and deliveries (plants), never their own |
| Governance and agreements | Keeps the organization's documents (licences, agreements with the platform and with partners; new document kinds `DataProcessingAgreement`, `SupplyAgreement`) |
| Data management | Keeps the data controller details and the organization's notice (new versions); records data-subject requests (access, correction, deletion, objection) with their 30-day KVKK deadline in `DataSubjectRequests`; records consent withdrawals received outside the portal; reads the organization's audit entries |

A platform-level Compliance Officer (in the platform organization) does the same for platform registrations and platform notices.

### 4.9 Notice acceptance at first sign-in (F8)

1. After a successful sign-in, a gate checks: does the user have an acceptance of the **current** platform notice and, for each active membership, of the current notice of that organization (and, for beekeepers, of each organization holding their data that published one)?
2. If not, the notice page opens before anything else: the text in the user's language, the data controller's name and contact, and **I have read and accept** (method `Click`, with the password just entered as the signature, time and IP stored).
3. For roles that sign on behalf of the organization (OA, CO, RO, QAO, LM), the acceptance is confirmed with an SMS code (M5), so the record shows the person, not just the session (decision O-12 can extend this to all).
4. When a new version is published, everyone affected sees the page again at their next sign-in. A membership switch into an organization whose notice is not accepted shows the page first.
5. Users created before the release accept at their next sign-in; nothing is locked by the migration.

### 4.10 Password reset

As 4.3 step 7: by SMS code to a verified phone, or by a re-issue by the issuer. The e-mail reset stays in the code but is hidden while SMTP is not configured (`SmtpSettings:Host` empty), so nobody waits for a mail that never comes.

### 4.11 Landing page logos (F1)

The landing footer (today: a text list of partners, and logos from `wwwroot/img/partners/` when present) becomes two labelled groups:

- **Project partners** (TR "Proje paydaşları"): the nine consortium logos from `docs/brand/partners/` copied to `wwwroot/img/partners/`, in the README order (coordinator first).
- **Funding organisations** (TR "Fonlayan kuruluşlar"): ICT-AGRI-FOOD ERA-NET Cofund (`docs/brand/programme/ict-agri-food.png`) with the acknowledgement line used on the manual cover (D-77); the funding agencies' logos from `docs/brand/funders/` → `wwwroot/img/funders/` when supplied (the group shows whatever files are there, like today's partner folder).

Rules: monochrome-friendly display (logos at full colour, same height, 40 px desktop / 32 px phone, wrapping grid), each logo with its partner name as `alt`, no links unless the partner gives a URL. Known asset issues from the partner README: INTA's file is the dark-background version (ask for the positive one), Katowice's file is low-resolution. Group names in five languages: Project partners / Proje paydaşları / Socios del proyecto / Partnerzy projektu / Projekta partneri; Funding organisations / Fonlayan kuruluşlar / Entidades financiadoras / Instytucje finansujące / Finansētāji. Spelling: the UI uses "organization" (A3); the group title follows that: "Funding organizations".

---

## 5. SMS design

### 5.1 The abstraction

Today: `ISmsSender.SendAsync(phone, text)` with `LoggingSmsSender` (logs a masked number, status `Logged`). D-78 replaces it with two layers:

- **`IMessageService`** (application layer): `SendCodeAsync(purpose, subject, recipient, language, contentHash, placeholders)` and `VerifyCodeAsync(purpose, subject, code, contentHash)`, `SendTextAsync(purpose, subject, recipient, language, placeholders)`. It renders the template (5.5), applies the code rules (5.2) and rate limits, writes `SmsMessages` and `OneTimeCodes`, picks the channel and calls a gateway.
- **`ISmsGateway`** (provider layer): `SendAsync(e164, text, options) → (providerMessageId, status)` and `GetStatusAsync(providerMessageId)` or a delivery-report callback. One class per provider:

| Provider | Use | Notes |
|---|---|---|
| `FakeSmsGateway` | development, tests, demos | stores the full text, status `Fake`; the **Outbox** screen shows it (5.3) |
| `LoggingSmsGateway` | fallback | today's behaviour, masked log line |
| `NetgsmSmsGateway` | production target (option 1) | Netgsm REST API |
| `IletiMerkeziSmsGateway` | production target (option 2) | İleti Merkezi REST API |
| `WhatsAppGateway` (later) | second channel | WhatsApp Business Platform templates; same `IMessageService`, channel chosen per recipient (5.4) |

Configuration (server-side only, never in the repo; secrets in `dotnet user-secrets` locally and in server config in production, as GO-LIVE describes):

```
Sms:Provider            Fake | Logging | Netgsm | IletiMerkezi
Sms:Sender              the registered sender name (originator), e.g. TOP4HONEY (max 11 characters)
Sms:<Provider>:*        credentials and endpoint of the chosen provider
Sms:CodeKey             secret key for hashing codes and phone numbers (32 random bytes, base64)
Sms:DailyLimit          hard cap on segments per day for the whole platform (protects the package), e.g. 100
Sms:AllowedCountries    e.g. TR (numbers of other countries are refused until decided, O-13)
Sms:AllowFake           true only on development and demo servers; production refuses to start with Provider=Fake unless set
```

Provider choice: both Netgsm and İleti Merkezi are established Turkish operators with HTTP APIs, Turkish-character encoding, delivery reports and a registered-sender process. The gateway is about 100 lines per provider, so the choice can follow price and paperwork. Before buying, confirm with the provider: (a) the sender-name registration documents (the legal entity that owns the sender name: the university or the project), (b) that the messages are sent as **transactional (informational)** messages, which do not need İYS (the national commercial-message consent system) consent, (c) an OTP or priority route for code messages, (d) delivery reports by callback or polling, (e) international delivery if partners outside Türkiye will use it.

### 5.2 One-time code rules

| Rule | Value |
|---|---|
| Format | 6 digits from a cryptographic random generator, leading zeros allowed |
| Storage | never in clear: HMAC-SHA256 with `Sms:CodeKey` over a per-code salt + code; the message body stored with `******` (real providers) |
| Binding | a code is valid only for its purpose, subject (beekeeper, intake, user) and content hash; changing the content voids it |
| Expiry | 5 minutes from sending |
| Tries | 3 wrong entries void the code; the screen says how many are left |
| Resend | allowed after 60 seconds; a resend voids the previous code; at most 3 codes per purpose and subject per hour |
| Per phone | at most 5 codes per hour and 10 per day to one number (any purpose) |
| Per user (sender) | at most 30 codes per hour per officer; at most 10 failed verifications per hour per officer, then a 15-minute pause |
| Password reset | at most 3 codes per account per hour; the page answers identically for unknown accounts |
| Platform | `Sms:DailyLimit` segments per day; beyond it only password-reset and code messages for an intake in progress are sent, and the platform administrator is warned |
| Comparison | constant-time |
| Audit | each send and each verification (success, failure, void) is an audit entry with the masked number |

### 5.3 Fake provider and outbox

- **Outbox** at `/Sms/Outbox`: platform administrators, available only when the provider is `Fake` (404 otherwise). It lists messages newest first with purpose, masked and full number, language, full text with the code, segments and the subject link. A **demo mode** setting can also show a small "message received" panel to the officer on the intake and registration pages, so a demo runs without a phone.
- E2E tests read codes from the fake provider's store (not from the UI), so tests cover the whole flow.
- A real provider never stores the code or a password in clear; the outbox does not exist there.

### 5.4 WhatsApp later

WhatsApp is added as a channel, not as a replacement: the recipient chooses it (consent purpose `WhatsApp`, since messages go through Meta, outside Türkiye, see 6.6), the message must be a pre-approved template, and SMS remains the fallback when WhatsApp is not delivered within a set time. Codes over WhatsApp follow the same rules (5.2).

### 5.5 Message texts in five languages

Placeholders: `{org}` organization name, `{code}`, `{link}` short link to the public site, `{user}`, `{password}`, `{date}`, `{containers}` (e.g. "3 teneke", "3 tins"), `{kg}`, `{honey}`, `{samples}`, `{name}` (public-name choice: yes/no word), `{version}`. "TOP4HoneyChains" is the sender label in the text; the network sender name comes from `Sms:Sender`. ES, PL and LV are our translations; partners review them like the earlier `Error.*` texts.

**M1 Registration and consent** (to the beekeeper, 4.1)

| Lang | Text |
|---|---|
| TR | TOP4HoneyChains: {org} sizi arıcı olarak kaydediyor. Aydınlatma metni: {link} Adınız herkese açık sayfalarda: {name}. Onaylıyorsanız bu kodu görevliye söyleyin: {code} (5 dk). |
| EN | TOP4HoneyChains: {org} is registering you as a beekeeper. Privacy notice: {link} Your name on public pages: {name}. To agree, tell the officer this code: {code} (5 min). |
| ES | TOP4HoneyChains: {org} le registra como apicultor. Aviso de privacidad: {link} Su nombre en páginas públicas: {name}. Si está de acuerdo, diga este código al responsable: {code} (5 min). |
| PL | TOP4HoneyChains: {org} rejestruje Cię jako pszczelarza. Informacja o danych: {link} Imię i nazwisko na stronach publicznych: {name}. Jeśli się zgadzasz, podaj kod: {code} (5 min). |
| LV | TOP4HoneyChains: {org} jūs reģistrē kā biškopi. Privātuma paziņojums: {link} Jūsu vārds publiskajās lapās: {name}. Ja piekrītat, pasakiet šo kodu darbiniekam: {code} (5 min). |

**M2 Credentials** (4.3)

| Lang | Text |
|---|---|
| TR | TOP4HoneyChains: kullanıcı adınız {user}, geçici şifreniz {password}. 72 saat içinde {link} adresinden giriş yapıp yeni şifrenizi belirleyin. |
| EN | TOP4HoneyChains: user name {user}, temporary password {password}. Sign in at {link} within 72 h and choose a new password. |
| ES | TOP4HoneyChains: usuario {user}, contraseña temporal {password}. Entre en {link} antes de 72 h y elija una contraseña nueva. |
| PL | TOP4HoneyChains: login {user}, hasło tymczasowe {password}. Zaloguj się na {link} w ciągu 72 h i ustaw nowe hasło. |
| LV | TOP4HoneyChains: lietotājvārds {user}, pagaidu parole {password}. 72 h laikā pieslēdzieties {link} un izvēlieties jaunu paroli. |

**M3 Delivery confirmation** (4.4)

| Lang | Text |
|---|---|
| TR | TOP4HoneyChains: {org} {date} tarihinde {containers}, {kg} kg {honey} teslim aldı; {samples} numune mühürlendi. Doğruysa kodu görevliye söyleyin: {code} (5 dk). |
| EN | TOP4HoneyChains: {org} received on {date}: {containers}, {kg} kg {honey}; {samples} samples sealed. If correct, tell the officer: {code} (5 min). |
| ES | TOP4HoneyChains: {org} recibió el {date}: {containers}, {kg} kg de {honey}; {samples} muestras precintadas. Si es correcto, diga al responsable: {code} (5 min). |
| PL | TOP4HoneyChains: {org} przyjął {date}: {containers}, {kg} kg {honey}; zaplombowane próbki: {samples}. Jeśli się zgadza, podaj kod: {code} (5 min). |
| LV | TOP4HoneyChains: {org} {date} saņēma: {containers}, {kg} kg {honey}; noplombēti paraugi: {samples}. Ja viss pareizi, pasakiet kodu: {code} (5 min). |

**M4 Password reset** (4.3, 4.10)

| Lang | Text |
|---|---|
| TR | TOP4HoneyChains: şifre sıfırlama kodunuz {code} (5 dk geçerli). Bu isteği siz yapmadıysanız mesajı dikkate almayın. |
| EN | TOP4HoneyChains: your password reset code is {code} (valid 5 min). If you did not ask for it, ignore this message. |
| ES | TOP4HoneyChains: su código para restablecer la contraseña es {code} (válido 5 min). Si no lo pidió, ignore este mensaje. |
| PL | TOP4HoneyChains: kod do zmiany hasła: {code} (ważny 5 min). Jeśli to nie Ty, zignoruj tę wiadomość. |
| LV | TOP4HoneyChains: paroles atjaunošanas kods: {code} (derīgs 5 min). Ja to neprasījāt, ignorējiet šo ziņu. |

**M5 Notice confirmation and phone check** (4.9, phone change)

| Lang | Text |
|---|---|
| TR | TOP4HoneyChains: {code} kodu, {org} aydınlatma metnini (sürüm {version}) okuduğunuzu onaylar. 5 dk geçerli. |
| EN | TOP4HoneyChains: code {code} confirms that you read the privacy notice of {org} (version {version}). Valid 5 min. |
| ES | TOP4HoneyChains: el código {code} confirma que leyó el aviso de privacidad de {org} (versión {version}). Válido 5 min. |
| PL | TOP4HoneyChains: kod {code} potwierdza zapoznanie się z informacją o danych {org} (wersja {version}). Ważny 5 min. |
| LV | TOP4HoneyChains: kods {code} apstiprina, ka izlasījāt {org} privātuma paziņojumu (versija {version}). Derīgs 5 min. |

(Phone check uses the same message without the notice part: "TOP4HoneyChains: telefon doğrulama kodunuz {code} (5 dk)." and equivalents.)

**M6 Information (optional, P3)**: delivery corrected, consent withdrawn, honey in a finalized batch with the Honey Story link. Sent only with the "portal notifications by SMS" consent, except the correction notice, which is always sent.

Length: with the provider's Turkish encoding a segment holds about 155 characters; ES may fit the GSM alphabet if accents are avoided, PL and LV need Unicode (70 characters, 67 per part of a long message). M1, M2 and M3 are about 150–190 characters in Turkish, so they are **2 segments** (M2 measured at 178 characters with a short link; the spec first assumed 1); M4 and M5 are 1 segment in Turkish. The template engine counts segments before sending and stores the count.

### 5.6 Expected volume (to size the package)

Live counts (local copy of production, October 2026): 70 beekeepers, 73 lots, 12 intakes recorded through the new intake form, 74 user accounts, 12 organizations.

| Flow | Messages per event | Segments (TR) | Event count, year 1 (estimate) | Segments per year |
|---|---|---|---|---|
| Registration and consent (M1), incl. ~20 % resends | 1.2 | 2 | 70 re-verifications + 100 new beekeepers = 170 | ≈ 410 |
| Credentials, beekeepers (M2), incl. re-issues | 1.2 | 2 | 150 | ≈ 360 |
| Credentials, members (M2, 2 segments) and their notice confirmations (M5, 1 segment) | 1.2 + 1 | 2 / 1 | 60 | ≈ 200 |
| Delivery confirmation (M3), incl. resends | 1.2 | 2 | 400 deliveries | ≈ 960 |
| Password resets (M4) | 1 | 1 | 25 % of 210 accounts | ≈ 55 |
| Notice re-acceptance after a new version (M5, signing roles) | 1 | 1 | 1 version × 30 | ≈ 30 |
| Information (M6, optional) | 1 | 1–2 | 400 | ≈ 600 |
| Tests on the production line before go-live | | | | ≈ 100 |
| **Total** | | | | **≈ 2 100 without M6, ≈ 2 700 with M6** |

Recommendation: a **5 000-SMS package** covers the first year with room for growth; a 2 000 package is too small. Formula for other scenarios: segments ≈ 2.4 × registrations + 2.4 × beekeeper accounts + 3.4 × member accounts + 2.4 × deliveries (+ 1.5 × deliveries with M6) + 200 for resets, notices and tests. Set `Sms:DailyLimit` to about 10 × the expected daily average (e.g. 100) so that a bug or abuse cannot empty the package in a day.

---

## 6. Privacy

Legal frame: KVKK (Law 6698, with the 2024 amendments on transfers abroad) for data in Türkiye, GDPR for data of partners in the EU. This section is a design, not legal advice; the texts of the notices need review by the university's data-protection office and the partners (O-14).

### 6.1 Roles in data protection; data controller per organization

| Party | Role (proposed) | For which data |
|---|---|---|
| Each organization (directorate, plant, laboratory, association, holder) | data controller | the personal data it records: beekeepers it registers, deliveries it receives, its staff's accounts in its membership |
| The platform operator (the project / Işık University, INTALA Lab) | processor for the organizations; controller for platform accounts, the audit log, SMS logs and the public pages | hosting, security, public pages |

Each organization records its data controller details (3.5) and publishes a notice; the platform publishes its own. A data-processing agreement between the operator and each organization is kept as an organization document (4.8). VERBİS registration is each controller's own duty (the column only records the number).

### 6.2 Notices and consents

- **Information notice** (aydınlatma, KVKK art. 10 / GDPR art. 13): required for everyone, acknowledged, not consented. Shown at registration (beekeepers) and at first sign-in (users); versioned; accepted per version.
- **Legal bases** (proposed): registration and traceability data → legal obligation (food traceability, Food Law 5996 and Regulation (EC) 178/2002 art. 18) and the contract with the plant; phone number for codes → necessary for the service; audit log → legitimate interest and legal obligation. No consent is asked for these, so withdrawing a consent never breaks traceability.
- **Explicit consent** (açık rıza) only for what is optional: name on public pages, apiary photos on public pages, SMS notifications beyond codes, WhatsApp, and any transfer abroad that has no other basis (6.6). Each is a separate yes/no, never pre-ticked, given by SMS code or signed photo at registration, or in the portal later; withdrawable in the portal at any time.
- Every acceptance and consent stores the exact text version and hash (3.5).

### 6.3 Retention (proposal for decision O-14)

| Data | Kept | Then |
|---|---|---|
| Lots, intakes, containers, samples, lab reports, batches | 5 years after the latest best-before date of a batch that contains them, or 5 years after the delivery if never in a batch | personal links pseudonymised (beekeeper replaced by a code); product data kept |
| Beekeeper identity | while active + the period above for their lots | pseudonymised |
| Evidence photos (signed forms) | as the record they prove | deleted with a hash kept |
| SMS messages | 1 year | number hash and body removed, counts kept |
| One-time codes | 30 days | deleted (only hashes were stored) |
| Notice acceptances, consents and withdrawals | as long as the related data + 3 years (proof) | deleted |
| User accounts of former members | 2 years after the last membership ends | anonymised; audit entries keep the user id only |
| Audit log | 10 years (append-only) | not changed; personal values inside entries are not rewritten (decision) |
| Prices | 5 years (commercial records) | deleted |

Retention is enforced by a monthly job in a later phase (P5 designs the job, its first run is a decision with the owner because it changes data).

### 6.4 How F4, F8 and F11 resolve C2, C4 and C5

| Item | Finding | Resolution |
|---|---|---|
| **C2** | Signed delivery forms, organization documents and consent documents can be downloaded by any member of the organization without a role check | **F4 and F8** make evidence documents legal records with a named controller, so access follows the notice: downloads of intake forms, registration and consent evidence are limited to the recording role (SAO or RO), the Compliance Officer, the organization admin and platform administrators; other roles get 403; every download is an audit entry. With F4 most new consents are SMS-confirmed and have **no document at all**, which shrinks the problem. **F11** lets the beekeeper see their own evidence in the portal (not download others'). Done in P5 for the existing downloads and built that way in P1/P2 for the new ones. |
| **C4** | The public **Verify a lab report** page answers for submitted, not yet approved reports | Not resolved by F4, F8 or F11; fixed directly in P5: the page answers only for **approved** reports (others: "not found"). **F11** follows the same rule: the beekeeper portal shows a report only once approved. |
| **C5** | The public Honey Story names the beekeepers without checking a consent | **F4** captures the public-name consent at registration (yes/no in M1); the Honey Story and the beekeeper page show the name only with an active `PublicName` (or the existing `Storefront`) consent, otherwise the neutral code already used by the storefront. **F11** lets the beekeeper give or withdraw it in the portal, effective at once. **F8** gives the public page a named controller. For legacy beekeepers without a consent: decision O-8. |

### 6.5 Security measures that support privacy

Codes and phone numbers hashed in logs; temporary passwords never stored; evidence files outside `wwwroot` with SHA-256 (as today); the beekeeper portal filters by the signed-in beekeeper only; prices visible only to the lot owner; downloads audited; the raised password policy (4.3).

### 6.6 Data residency (F7, for decision)

Today everything runs on one server in Türkiye (INTALA Lab). Personal data in the platform is almost all Turkish (Turkish beekeepers, Turkish organizations); partners in Poland, Latvia and Argentina mostly see product data. Options:

| Option | What it means | KVKK (data from Türkiye) | GDPR (data from the EU) | Effort | Fit |
|---|---|---|---|---|---|
| **A. Türkiye only** (as today) | one server in Türkiye | no transfer abroad | EU partners' personal data (their staff accounts, any EU beekeepers) is transferred to Türkiye: needs EU standard contractual clauses with the operator (Türkiye has no EU adequacy decision) | none | good while personal data is Turkish |
| **B. EU only** | move the server to an EU data centre | every Turkish beekeeper's data is transferred abroad: needs a KVKK art. 9 mechanism (standard contract notified to the Authority, or another listed safeguard); explicit consent is only for occasional transfers | no transfer | migration + contracts | poor: the bulk of personal data would leave Türkiye |
| **C. Two regions** | a Türkiye node and an EU node; personal data stays in its region; the shared chain (lots, CCPs, batches, lab results) uses pseudonymous ids and is replicated | transfer only of pseudonymised data; public pages read names from the region that holds them | same | large: two databases, replication, routing, two operations teams | best for a multi-country roll-out with real EU beekeepers |
| **D. EU primary with a Türkiye "branch"** (the meeting's idea) | an EU data centre with a Turkish location as a branch | legally still a transfer if the data is accessible from the EU operator; brings the B obligations unless the Turkish branch alone holds the Turkish data, which then is option C | as C | as B or C | not a separate legal option; it becomes B or C |

Recommendation: **stay with A now**, and prepare for C without building it: (1) add a `DataRegion` value (`TR`, `EU`, `AR`) on organizations, beekeepers and users when they are created (a small, additive column in a later migration), (2) keep public and cross-organization views on pseudonymous codes where they do not need names (already the case for holders, D-75), (3) sign standard contractual clauses with EU partners whose staff get accounts, (4) Argentina has an EU adequacy decision, which does not cover Türkiye, so an Argentine partner's personal data in Türkiye also needs a contract under Argentine law (check with INTA). Revisit when the first non-Turkish beekeepers are registered. WhatsApp (5.4) is a transfer to Meta and needs consent or a contract either way.

---

## 7. Explanatory animation (F16)

### 7.1 Where motion explains

| Place | Trigger | Scene (≤ 1.5 s) |
|---|---|---|
| Intake (**Record delivery**) | first press of the button, or the ⓘ next to the page title | a truck backs in, three tins slide down, their seals tick green |
| Sealing containers | first container row added | a seal tag loops through the tin handle and clicks shut |
| **Take sample** (button drawn as a jar) | each press, until the user turns it off | tin lid lifts, a honey ribbon fills a jar, the cap screws on, its button presses down |
| Lab handover (**Send to lab**, **Receive sample**) | first use per user | jar slides into a bag, travels a short dotted path to a bench; a magnifier checks the cap, a tick appears |
| Shipment | first shipment of the organization | a crate moves along the chain line to the next organization's dot; the custody ribbon passes with it |
| Empty **My work** | when the list is empty | the bee rests on a comb cell (uses the existing `slow` mark), "Nothing waiting for you" |
| First-use tour per role | first sign-in of a user in a role (after the notice gate) | 3–4 steps: a spotlight on the menu items of that role with a one-line caption each; a still illustration, not a scene |
| Beekeeper portal | first visit | "Your honey's journey": apiary → tins → jar → lab → jar on a shelf, as the portal's header strip |

### 7.2 Rules

1. **At most 1.5 s** per scene; most are 0.9–1.2 s.
2. **Skippable**: a click, a key press or focus anywhere ends it at its final frame; a "Show animations" switch in the user menu turns explanatory scenes off for good (stored per user).
3. **Never blocks input**: the form is usable from the first frame. Scenes play in a reserved strip (the page header or beside the button), never as a modal overlay; focus goes to the first field as today; typing ends the scene (the existing `html.is-typing` rule).
4. **Reduced motion** (`prefers-reduced-motion: reduce`): the final frame is shown as a static illustration with its caption. Same for the "off" setting.
5. **Plays once per trigger**: a scene repeats only when the user asks (ⓘ), except the jar button, whose short press feedback (cap down, 300 ms) is part of the control.
6. **Honey-foam style**: cream background (`--b-cream`), honey amber gradient for honey, soft foam bubbles (small circles that rise and fade), round line ends, 2 px strokes in the brand ink, no text inside the drawing (captions are HTML, so they are translated and read by screen readers).
7. Captions sit next to the scene; the scene itself is `aria-hidden="true"`; nothing important is told only by motion.

### 7.3 How this fits MOTION.md

MOTION.md has three levels: brand moments (expressive), feedback (subtle) and data entry (still). Explanatory scenes are a **fourth level, "Explanation"**, between brand and feedback:

| Level | Where | Motion | Timing |
|---|---|---|---|
| **Explanation**: teaches a physical step | the triggers in 7.1 | a short scene of what happens to the honey outside the computer | ≤ 1.5 s, once per trigger |

It differs from **decorative** motion in purpose: each scene shows a real-world step that the screen records (tins, seals, jars, caps), so it is information, not decoration. It respects the data-entry rule because it never moves **while** someone types and never animates fields, counters or values. The general rules stay: only `transform` and `opacity` (a seal's stroke drawing is allowed once, like the map outline), reduced motion respected, loops none, one brand moment at a time (a scene never plays together with the mark's success moment). MOTION.md gets this level and a checklist line when P4 is implemented.

### 7.4 Assets

- **Inline SVG + CSS keyframes**, one partial per scene (`Views/Shared/Explain/_Intake.cshtml`, …) rendered by a tag helper `<honey-scene name="sample" trigger="click" />` like `<honey-mark>` (prefixed ids, so two scenes can share a page). No Lottie, no video, no new library; each scene ≤ 8 KB.
- Shared parts as `<symbol>`s in one sprite: tin, jar, cap, seal tag, truck, bee, comb cell, bench, magnifier, crate.
- A small script (`js/honey-scene.js`) starts a scene by adding a class, ends it on input, honours the per-user switch and reduced motion; the CSS holds all timing.
- The static fallback is the scene's last frame (the same SVG without the animation class), so there is one source per scene.

### 7.5 Storyboards

**S1 Take sample (jar button, 1.2 s)**

| t | Frame |
|---|---|
| 0.0 | A closed tin with a seal tag on its handle; an empty jar beside it. |
| 0.2 | The seal tag snaps open and falls away (broken), the lid lifts. |
| 0.4–0.8 | A honey ribbon flows from the tin into the jar; the jar fills; two foam bubbles rise and fade. |
| 0.9 | The cap drops onto the jar and turns a quarter turn. |
| 1.05 | The cap's button presses **down** (scale 1 → 0.85 → 1) and a small tick appears. |
| 1.2 | A numbered seal label wraps over cap and jar. Caption: "One sample from every tin. Check the cap button is down." |

**S2 Record delivery (1.3 s)**

| t | Frame |
|---|---|
| 0.0 | A small truck enters from the left and stops at a loading line. |
| 0.3–0.8 | Three tins slide down a ramp one after another and line up. |
| 0.9 | Each tin's seal tag gets a tick in turn. |
| 1.1 | A phone outline beside the tins shows a message bubble with six dots. Caption: "Record each tin and its seal. The beekeeper confirms with the SMS code." |

**S3 Receive sample at the lab (1.2 s)**

| t | Frame |
|---|---|
| 0.0 | A sealed jar in a bag on a bench. |
| 0.3 | The bag opens downwards; the jar stands. |
| 0.5–0.9 | A magnifier passes over the seal label (number matches, tick), then the cap (button down, tick). |
| 1.0–1.2 | The jar moves onto a rack. Caption: "Check the seal number, the label and the cap button. If any fails, reject the sample." |

**S4 Empty My work (still + 1 s)**

| t | Frame |
|---|---|
| 0.0–1.0 | A comb of seven cells fades in; the bee mark lands on the middle cell and folds its wings (the existing mark in `slow` mode). Caption: "Nothing waiting for you." The mark keeps its calm loop under the existing rules. |

---

## 8. Promo videos (F15)

Two films, 60–90 s each, for the website, partner events and social media; produced as animation in the same honey-foam style as 7 so that the app and the films look alike. Narration in TR and EN first, then ES, PL, LV as subtitles; music without lyrics; every on-screen text also in subtitles. Real names, real faces and real data are not used; the beekeeper is an illustrated character.

### 8.1 Consumer film: "What is in this jar?" (≈ 75 s)

| # | Time | Picture | Narration (EN) |
|---|---|---|---|
| 1 | 0–8 s | A supermarket shelf; a hand stops at a jar of thyme honey and hesitates. | "Every jar of honey tells a story. But can you read it?" |
| 2 | 8–16 s | The phone scans the QR on the label; the Honey Story opens. | "Scan the code on the lid, and the jar answers." |
| 3 | 16–28 s | The map of Türkiye fills in a province; zoom to hills with hives. | "It comes from these hills, from beekeepers who are registered and verified." |
| 4 | 28–40 s | Sealed tins arrive at a plant; a sample from every tin goes into a jar with a sealed cap. | "Every tin is sealed. A sample is taken from each one, and nobody can open it unseen." |
| 5 | 40–52 s | The lab: the cap is checked, the analysis runs, values appear with ticks. | "An accredited laboratory checks it: moisture, sugars, residues, against the official limits." |
| 6 | 52–64 s | The journey line on the Honey Story lights up step by step: apiary, plant, lab, packing, shelf. | "Every step is recorded, and once the batch is sealed, nothing can be changed." |
| 7 | 64–75 s | Back at the shelf; the jar goes into the basket; logo and URL. | "Real honey, with its whole story. TOP4HoneyChains." |

### 8.2 Beekeeper film: "My honey, my name" (≈ 85 s)

| # | Time | Picture | Narration (EN) |
|---|---|---|---|
| 1 | 0–10 s | Dawn at an apiary; the beekeeper lifts a frame full of honey. | "I know my honey is good. The problem is proving it." |
| 2 | 10–22 s | At the district directorate: the officer types, the beekeeper's phone buzzes, she reads a code aloud. | "The district directorate registers me. A code on my phone, and it's done: my registry number is verified, and I decide whether my name is shown." |
| 3 | 22–36 s | Delivery at the plant: tins with seals, the officer records each one; another code on the phone. | "When I deliver, every tin is sealed and counted. I confirm the delivery with a code. Nobody can change it afterwards." |
| 4 | 36–48 s | A jar is filled from each tin; the cap button clicks down; the jars travel to the lab. | "A sample from every tin, in jars nobody can open unseen." |
| 5 | 48–62 s | The beekeeper at home, opening the portal on the phone: deliveries, lab results with ticks, a price field where she types her price. | "In my portal I see my deliveries and my lab results, and I set the price of my honey." |
| 6 | 62–76 s | In a city shop, a customer scans a jar; the Honey Story shows the beekeeper's name and province. | "And when someone far away buys a jar, they see where it comes from, and who made it." |
| 7 | 76–85 s | Back at the apiary, sunset; logo, "Ask your district directorate", URL. | "My honey, my name. TOP4HoneyChains." |

Production notes: a storyboard and an animatic first (reviewed by the coordinator), then animation; reuse the in-app scene drawings (7.4) as the visual base; the films show the planned D-78 screens, so they are produced after P3, or they show stylised screens that do not age.

---

## 9. Phased plan

Each phase is one or more work items (one branch per item, per CLAUDE.md), merged to `main` when its checks pass. Nothing goes live piecemeal: the phases are released together through `docs/GO-LIVE.md`, or in two releases if the coordinator decides so (O-15). Effort is rough, in developer days, including tests and the browser check.

### P1: roles, beekeeper registration, consent and notices, credentials, SMS infrastructure

- **Scope:** capability `BeekeeperRegistration`, roles RO and CO (2.1–2.2), RO permissions; `IMessageService`, fake and logging gateways, outbox, one real gateway (Netgsm or İleti Merkezi, whichever is bought), code rules and rate limits (5); notices, acceptances, data controller details, notice gate at sign-in (4.9); beekeeper registration with SMS code, photo fallback and CO review, re-verification of legacy beekeepers (4.1); apiary title (4.2); credentials by SMS, forced first change, password policy, SMS password reset (4.3, 4.10); `BeekeeperAccounts` link (the portal itself comes in P3; beekeeper sign-in in P1 shows only "Change password", "Notice" and a "Your portal is coming" page, or credentials are issued only from P3, O-15).
- **Migrations:** 018, 019, 020, 021, 022.
- **Tests:** unit: code generation, hashing, expiry, 3 tries, resend voiding, rate limits, content-hash binding, segment counting, template rendering in five languages, title generation (length, duplicates), registry-province check, temporary-password expiry; E2E (fake provider): register a beekeeper by SMS, by photo + CO acceptance (and the refusal of self-acceptance), re-verify a legacy-like test beekeeper, issue credentials and force the change, the notice gate for an existing user, password reset by SMS; negative: wrong code ×3, expired code, code for another subject, officer of another directorate cannot read the beekeeper, beekeeper account gets 403 on panel pages.
- **Risk to live data:** low. Scripts are additive; no row is changed. Visible effects: every existing user meets the notice page at the next sign-in (needs the notice texts ready, O-14); the password policy applies at the next password change only. Legacy beekeeper logins stay locked until re-verified.
- **Effort:** 14–18 days (SMS 4, registration and evidence 5, notices 3, credentials and reset 3, apiary title 1, tests 2–3).

### P2: intake SMS, sealed containers, sampling chain of custody, lab checks

- **Scope:** containers and seal events, container labels (print), intake with summary, content hash, M3 code and photo fallback with CO review (4.4); one sample per container with cap check, composite samples if O-4 says so (4.5); lab reception with seal, label and cap checks (4.6); immutability of confirmed deliveries, read-only admin fields, content-hash check in the recall report (3.10); recall report shows containers and seals.
- **Migrations:** 023.
- **Tests:** unit: canonical content and hash, lot amount = container sum, one sample per container, lock rules; E2E: intake with two tins, two samples, SMS confirm; intake by photo, finalize blocked until CO accepts; lab rejects a jar with a popped cap, the plant re-samples (new seal events); negative: edit a confirmed intake (refused), change the summary after sending the code (code void), confirm with a code from another intake, legacy lot still finalizable without containers.
- **Risk to live data:** medium-low. Widening of the intake form columns and new nullable columns; legacy lots, intakes and samples stay valid and are shown as legacy. Risk is in the rules: a rule that wrongly treats a legacy lot as "missing containers" would block finalizing of live batches, which the E2E test with legacy-shaped data covers.
- **Effort:** 12–15 days.

### P3: beekeeper portal with price entry, and the Compliance Officer

- **Scope:** `/me` portal (4.7) with all screens, price entry and history (3.8), consent give/withdraw, phone and language; Compliance section and My work tasks (4.8), `DataSubjectRequests`, `ComplianceReviews`, organization audit view; M6 information messages if chosen; the requirement of a CO for new organizations (2.5).
- **Migrations:** 024, 025.
- **Tests:** unit: portal queries filtered by the signed-in beekeeper (a test per screen with two beekeepers), price rules (owner only, lock at finalize, history); E2E: beekeeper signs in, sees only their lots, enters and changes a price, the plant's OA sees it, Staff and a later holder do not; withdraw the public-name consent and the Honey Story shows the neutral code; CO accepts a photo, cannot accept their own; negative: a beekeeper id in a URL that is not theirs (404), POST to any panel action (403).
- **Risk to live data:** low. New tables; the change visible to the public is the Honey Story name rule (O-8), which is decided before P3 ships.
- **Effort:** 10–13 days.

### P4: landing logos and in-app animations

- **Scope:** landing logo groups (4.11, A7); explanatory scenes S1–S4 and the remaining triggers of 7.1, the "Show animations" switch, MOTION.md updated with the Explanation level; first-use tour per role.
- **Migrations:** none (logos are files; the per-user switch uses `UserSettings` from 021).
- **Tests:** unit: tag helper ids are unique per page; E2E: the scene does not block typing (type during the scene), reduced-motion shows the static frame, the switch persists; manual: 4× CPU throttling, no task over 50 ms (MOTION.md canvas rule).
- **Risk to live data:** none.
- **Effort:** 6–8 days (scenes 4–5, logos 0.5, tour 1–1.5, MOTION.md and checks 0.5). The promo films (8) are separate content production, outside the developer effort.

### P5: privacy fixes and the PLANNED-APP-CHANGES leftovers

- **Scope:**

| Item | What | Note |
|---|---|---|
| C2 | role-checked, audited downloads of intake forms, consent and registration evidence, organization documents | 6.4 |
| C4 | Verify a lab report answers only for approved reports | 6.4 |
| C5 | public name only with consent; legacy rule per O-8 | built in P1/P3, closed here |
| C1 | Shipments → Read and Beekeeper pages → Read checked on the pages, not only in the menu | |
| C3 | sample, ship, finalize also check the organization's capability | |
| C6 | central-association setting exclusive or not: decision O-16 | |
| A1 | Quality approver → Quality Assurance Officer in five languages | migration 026 |
| A2 | messages name the role, not the code | |
| A3 | "organization" spelling in English | migration 026 and `.resx` |
| A4 | "Ground rules" label | `.resx` |
| A5, A6 / B1 | capability descriptions as abilities; names for CentralAssociation, PublicAuthority, Beekeeping (+ BeekeeperRegistration from 018) | migration 026 |
| A7 | funder logos | done in P4 with F1 |
| A8 | origin map for other countries | **not in D-78**: a large separate project item; stays in PLANNED-APP-CHANGES |
| B2 | "KKP/CCP" abbreviation on the Roles and permissions matrix in every language | |
| B3 | landing "CCP 2–9" text corrected (lot CCP 2–3, batch CCP 4–10) | |
| B4 | contrast of "Manage batch CCPs" | |
| B5 | branded Verify page | |
| B6 | Shipments as cards on phones | |
| B7 | live count of required lab values | |
| B8 | demo data consistent (local demo data only) | |
| B9 | "Your current holding" label on the recall report | optional |
| Retention | design of the retention job; first run only after the owner's approval (it changes data) | 6.3 |

- **Migrations:** 026.
- **Tests:** E2E for each C item (positive and the 403/404 negative), a resource-key test for A1–A4 in five languages.
- **Risk to live data:** low; 026 changes reference texts only. The visible change is C4 (submitted reports stop verifying publicly) and the A1 rename.
- **Effort:** 6–8 days.

### P6: manual in five languages, screenshots, live update package

- **Scope:** manual chapters updated in five languages: chapter 2 (tables 2–3 of this spec), new chapters for the registration officer, the Compliance Officer and the beekeeper portal, chapter 4 flow (containers, codes, caps), chapter 13 (notices, consents, data controller); the old → new labels table updated; screenshots recaptured with the fake SMS provider and test data only (CLAUDE.md data rules); `docs/manual/PLANNED-APP-CHANGES.md` closed; GO-LIVE entries for 018–026, SMS configuration and sender registration, notice texts; release notes; the live update package (publish build, scripts 018–026 in order, configuration list) prepared and checked against a LocalDB copy; deployment only after the owner's go (CLAUDE.md).
- **Migrations:** none new.
- **Tests:** the full test suite; a dry run of the update package on a fresh copy of the local database: scripts in order, second run changes nothing, the legacy records render; smoke test list extended (registration by SMS with the real provider on a test number, intake by SMS, portal sign-in).
- **Risk to live data:** the deployment itself; mitigated by the GO-LIVE backup and rollback steps (scripts are additive, the old build runs on the new schema).
- **Effort:** 8–10 days (manual ×5 languages is the bulk).

Total: about 56–72 developer days, P1 → P3 in order (P2 needs P1's SMS; P3 needs P1's accounts and P2's confirmed deliveries); P4 and P5 can run in parallel with P2–P3.

---

## 10. Open decisions for the coordinator

| # | Decision | Options | Recommendation |
|---|---|---|---|
| O-1 | **Final name of the registration role** | (a) "Registration officer" / TR "Kayıt Görevlisi"; (b) "TOP officer" / TR "TOP Görevlisi" (the meeting's name; short, but "TOP" means nothing to a new reader and is the project name); (c) "Beekeeper registration officer" / TR "Arıcı Kayıt Görevlisi"; (d) TR "Kayıt Sorumlusu" to match "Uyum Sorumlusu" | **(c)** on screens where space allows, code `RegistrationOfficer`: it says what the person does and keeps the TR pattern; the code stays the same whatever the label |
| O-2 | **Data residency** (6.6) | A Türkiye only; B EU only; C two regions; D EU with a Türkiye branch | **A now**, with a `DataRegion` tag and contracts with EU partners; **C** when the first non-Turkish beekeepers are registered |
| O-3 | **SMS provider** | Netgsm; İleti Merkezi; another Turkish provider | Either; pick on price for 5 000 SMS, sender-name paperwork, an OTP route and delivery reports. Ask both for a quote with these four points |
| O-4 | **Lab analysis: per container or composite** | (a) every container jar analysed; (b) the lab mixes the container jars of a lot into a composite and analyses that, keeps the jars; (c) composite, plus individual analysis of a container when the composite fails or on request | **(c)**: proof per container is kept (every jar sealed and checked), lab cost stays per lot |
| O-5 | **Price: per lot (delivery) or a price list** | (a) asking price per delivered lot; (b) a price list per honey type and season before delivery; (c) (a) plus an "agreed" price confirmed by the buyer | **(a)** in P3; (c) later if plants want to record the price paid |
| O-6 | **Who reads the price** | lot owner (OA, CO) and platform admins; + the beekeeper's association; + later holders | lot owner and platform admins only; the association only with the beekeeper's consent |
| O-7 | **Price on the public Honey Story** | never; optional "fair price" badge with both beekeeper's and owner's consent | **never** in this release |
| O-8 | **Names of legacy beekeepers on existing Honey Story pages** (C5) | (a) keep showing names until the beekeeper is re-verified; (b) switch to the neutral code at release until consent is given; (c) ask the associations to collect consent before release | **(b)** with (c) running before go-live: it is the safe default under KVKK; names come back as consents arrive |
| O-9 | **Compliance Officer required** | required for verification of new organizations; required for all within N days; recommended only | required for new organizations from P3; existing ones within 60 days (warning, no blocking) |
| O-10 | **Who issues member accounts** | platform administrators only (today); also organization admins for their own organization | platform administrators in this release; organization admins later, with the CO's review |
| O-11 | **Batch titles** | typed (today); generated from honey type, province and date | keep typed; F5 covers apiaries |
| O-12 | **SMS confirmation of the notice** | for signing roles only; for everyone; for nobody (click only) | signing roles only (OA, CO, RO, QAO, LM) |
| O-13 | **SMS outside Türkiye** | TR numbers only; all partner countries | TR only in the first release; others when a partner organization joins with users |
| O-14 | **Notice texts and retention periods** | drafted by us and reviewed by the university's data-protection office; written by the office | we draft from 6.2–6.3; the office reviews before P1 ships (the notice gate cannot go live without approved texts) |
| O-15 | **Release cut** | one release after P6; two releases (P1+P2+P5, then P3+P4) | **two releases**: the verification and custody chain first; the portal second; beekeeper credentials are issued only with the portal |
| O-16 | **C6, central association** | exclusive verifier when configured; both (today) | exclusive when configured, as the manual describes |
| O-17 | **Beekeepers without a mobile phone** | registration by signed photo, no portal; a relative's phone as the contact | signed photo, no portal; a second number only with the beekeeper's written consent |

Decisions recorded as final in D-79 (not open): real SMS with a provider abstraction and a fake provider with an outbox; e-mail stays disconnected; beekeepers write only their price; the role name **Compliance Officer**.
