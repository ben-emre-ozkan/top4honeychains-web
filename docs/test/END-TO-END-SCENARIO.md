# End-to-end test scenario (team role play)

Everyone logs in as a user. Each user is a member of one organisation with one or more org roles. The organisation's capabilities decide which work it may do; the role decides who does it inside the organisation. The beekeeper and the consumer do not log in: the beekeeper is a record, the consumer uses the public pages.

Local LocalDB only, `Features:OrgRoleModel` on, demo organisations from `db/demo/create_demo.sql`. User names start with `demo_` and e-mails end with `@example.invalid`, so `cleanup_demo.sql` removes them.

## Cast

| # | User name | Real-life person | Organisation | Capabilities | Org role(s) | Does in the system |
|---|---|---|---|---|---|---|
| 1 | existing admin account | Platform team (project) | Platform | PlatformAdmin | Platform administrator | Verifies organisations, adds members, sets CCP responsibilities, verifies the beekeeper's AKS number, reads the recall report and audit log |
| 2 | – (no login) | Beekeeper with an AKS enterprise number | – (beekeeper record, linked to the association) | – | – | Delivers honey, signs the delivery form and the storefront consent form |
| 3 | demo_birlik_denetci | Association field technician (optional) | [DEMO] Arıcılar Birliği | Association | FieldInspector | No task by default; does CCP0 only if an administrator assigns CCP0 to the field inspector (D-63) |
| 4 | demo_birlik_yonetici | Association chair or secretary | [DEMO] Arıcılar Birliği | Association | QualityApprover + OrgAdmin | Records the beekeeper's storefront consent |
| 5 | demo_isleme_kabul | Honey reception officer at the plant | [DEMO] Kovan Bal İşleme | Processing (CCP1–6), Packing (CCP7–9), Wholesale | SampleAcceptanceOfficer | Records the delivery, does CCP0 (primary audit) for the new lot, takes sealed sample and counter-sample, sends it to the lab |
| 6 | demo_isleme_personel | Production and filling staff | [DEMO] Kovan Bal İşleme | same | Staff | Enters CCP2–9, creates the batch, requests finalisation, ships |
| 7 | demo_isleme_kalite | Quality manager (food engineer) | [DEMO] Kovan Bal İşleme | same | QualityApprover | Approves CCP0, the other CCP values and finalisation |
| 8 | demo_lab_analist | Laboratory analyst | [DEMO] Analiz Laboratuvarı | Laboratory (accredited) | Analyst | Receives the sample (seal check), enters report data, PDF and values |
| 9 | demo_lab_mudur | Laboratory manager | [DEMO] Analiz Laboratuvarı | Laboratory (accredited) | LabManager | Approves or returns the report |
| 10 | demo_market | Shop or warehouse manager | [DEMO] Şehir Marketi | Retail (CCP10) | Staff | Confirms or rejects the shipment, enters CCP10 storage |
| 10b | demo_market_onay | Shop manager's deputy | [DEMO] Şehir Marketi | Retail (CCP10) | QualityApprover | Approves CCP10 storage (four-eyes) |
| 11 | – (no login) | Consumer | – | – | – | Opens the Honey Story from the QR code, the beekeeper page, report verification |

The market has a second user (10b) as QualityApprover, so CCP10 can be approved without the four-eyes exemption.

CCP0 comes before the lab test (D-61). By default the reception officer who records the delivery does CCP0 and the plant's quality approver approves it (D-63). An administrator can give CCP0 to the association's field inspector instead under CCP responsibilities (CCP 0 → "the beekeeper's association", Association, FieldInspector).

## Preparation (admin)

1. Flag on in `appsettings.Development.json`; app restarted.
2. The four demo organisations are Verified with the capabilities above.
3. Users added through Organizations → organisation → Add member; person 4 added twice (QualityApprover, OrgAdmin).
4. Market four-eyes exemption or second market user.
5. A new beekeeper and apiary (names starting with `[DEMO]`), so the test starts from a new delivery. The existing demo batch is already finalised.

## Steps

Expected results marked **must block** are negative checks: the system has to refuse the action.

| Step | Who | Action | Screen | Expected |
|---|---|---|---|---|
| 1 | 1 Admin | Link the beekeeper to the association, enter and verify the AKS number | Registry | Beekeeper verified |
| 2 | 2 Beekeeper + 4 OrgAdmin | Beekeeper signs consent; OrgAdmin uploads it and records consent | Storefronts → Consent | Beekeeper page published |
| 3 | 2 Beekeeper + 5 Reception | Find beekeeper by AKS number, record delivery and photo of the signed form | Intake | Raw lot created, owned by the processor |
| 4 | 5 Reception | Seal sample and counter-sample, seal numbers, storage place, choose lab, send | Samples | Sample sent |
| 5 | 8 Analyst | Receive sample (seal intact); try to enter results | My work → Lab results | **must block** until CCP0 is approved ("waiting for CCP 0") |
| 6 | 5 Reception | Enter CCP0 for the new lot (the task is in My work right after the delivery; "If yes, which methods…" only when its question is Yes) | My work → CCP | CCP0 submitted |
| 7 | 5 Reception | Try to approve own CCP0 | CCP | **must block** (four-eyes) |
| 8 | 7 Quality approver | Approve CCP0 | My work → CCP | CCP0 approved; the lab's task is open |
| 8b | 8 Analyst | Enter report data, PDF and values | My work → Lab results | Out-of-limit values flagged; cannot submit with required values missing |
| 9 | 8 Analyst | Try to approve own report | Lab results | **must block** |
| 10 | 9 Lab manager | Approve the report; with out-of-limit values a confirmation lists them (Cancel keeps it unapproved, D-64) | My work → Lab results | Values written to the lot's CCP1 and locked; out-of-limit values stay flagged |
| 11 | 6 Staff | Enter CCP2–6; mark a step not applicable with a reason | CCP | Values flagged against limits |
| 12 | 7 Quality approver | Approve CCP2–6 | My work → CCP | All approved |
| 13 | 6 Staff → 7 | Create a batch from the lot, enter CCP7–9; approver approves | Final productions → CCP | Batch CCPs approved |
| 14 | 6 Staff | Try to finalise while a CCP is open | Final productions | **must block** |
| 15 | 6 Staff → 7 | Request finalisation; approver approves | Final productions | Batch and lots locked; Honey Story link and QR created |
| 16 | 6 Staff | Ship part of the batch to the market | Shipments | Shipment pending; custody still with the processor |
| 17 | 10 Market | Confirm the shipment (optionally try a rejection with a reason first) | Shipments | Custody moves only after confirmation |
| 18 | 10 Market | Enter CCP10 storage conditions | CCP | Needs a second person or the exemption to approve |
| 19 | 10 Market | Try to change one of the processor's CCPs | CCP | **must block** (read only) |
| 20 | 11 Consumer | Open the Honey Story from the QR, then the beekeeper page; verify the report code | Public pages | Beekeeper share, origin %, lab badge visible; no phone number or exact location |
| 21 | 1 Admin | Open the recall report and the audit log | Recall, Audit | Every step visible with user and time; audit chain valid |
