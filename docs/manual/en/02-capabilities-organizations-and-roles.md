# 2. Capabilities, organizations and roles {#roles}

In TOP4HoneyChains a person never acts alone: every action is done **for an organization, in a role**. Three things decide what you may do:

1. the **capabilities** of your organization: what it is able to do in the honey chain;
2. your **role** in the organization: who you are inside it;
3. the **status** of your organization: only a **Verified** organization can enter data. Organizations are verified by a platform administrator (see [Organization status](#org-status)).

A role is held through a **membership**: one user account × one organization × one role (see [Multiple memberships](#memberships)). On top of this, some rules are built into the platform and cannot be switched off (see [Ground rules](#fixed-rules)).

This chapter keeps two things apart:

- the **value-chain activities**: the work people do with the honey outside the computer, such as delivering, sampling, analysing, processing, storing and shipping ([Table](#tbl-activities));
- the **data rights**: what each role may do with the records in the platform: create, read, update, delete and approve ([Table](#tbl-crud)).

## Capabilities {#capabilities}

A capability is an ability of an organization: it says what the organization is **able to do** in the honey chain. Which member does the work is set by the roles (see [Roles](#role-list)). An organization has one or more capabilities; a platform administrator gives and removes them (see [Capabilities](#admin-org-capabilities)). The list of capabilities itself is fixed. [Table](#tbl-capabilities) lists them with their name on screen.

Table: Capabilities: what an organization is able to do {#tbl-capabilities}

| Capability (on screen) | Ability | CCPs (default) |
|---|---|---|
| **Association** | Able to link its member beekeepers and record their consent for a public beekeeper page. | none; CCP 0 only if an administrator assigns it to the association |
| **Processing** | Able to receive and process honey: deliveries, samples, lot CCP 0–3 and batch CCP 4–6. | lot CCP 0–3, batch CCP 4–6 |
| **Packing** | Able to fill, label and pack honey: batch CCP 7–9 and the CCP 10 storage at the plant. | batch CCP 7–9, CCP 10 |
| **Laboratory** | Able to analyse honey samples and issue lab reports, as an accredited or an in-house food-analysis laboratory. The accreditation number and its validity date are kept on this capability. | none; its approved results fill lot CCP 1 |
| **Wholesale** | Able to hold batches in wholesale, record their storage (CCP 10) and ship them on. | CCP 10 |
| **Retail** | Able to hold batches for sale to consumers, record their storage (CCP 10) and ship them on. | CCP 10 |
| **Central association** | Able to verify beekeepers' associations, when the platform is configured for it. | none |
| **Public authority** | Able to read all records, as a ministry or inspection body; enters nothing. | none (reads all) |
| **Beekeeping enterprise** | Able to produce honey. Beekeepers have no accounts; their enterprise is represented by a Beekeeper record. | none |

A processing plant that also fills and stores its own honey needs **Processing**, **Packing** and **Wholesale** (or **Retail**), because batch CCP 7–9 need Packing and CCP 10 needs Wholesale, Retail or Packing.

**The platform.** *Platform administrator* is not a capability that an organization can be given: it is a global role of a user account, for the people who run the platform. Platform administrators manage organizations, memberships, reference data and settings and read every record, but enter no workflow data (see [chapter 12](#admin)). The platform's own organization carries the reserved capability **PlatformAdmin**; it cannot be given to any other organization or removed.

## Value-chain activities {#activities}

The value-chain activities are done by people, with the honey, outside the computer. The platform records the result of each activity as data. [Table](#tbl-activities) shows, per capability, which activities its people do and what is recorded in the platform.

Table: Value-chain activities (people) and what the platform records (computer) {#tbl-activities}

| Capability | Value-chain activities (people, outside the computer) | Recorded in the platform (computer-based) |
|---|---|---|
| Beekeeping enterprise | Keep bees, harvest honey, deliver it to a plant | Beekeeper and apiary records, kept by the administrators and the association |
| Association | Link and inform member beekeepers, read the consent text to them, collect signatures | Consent records; public beekeeper pages |
| Processing | Receive deliveries, check honey at reception, seal samples, heat, filter, mix and settle honey | Delivery (raw honey lot), sample, CCP 0 and CCP 2–6 data |
| Packing | Fill, label and pack honey, store it at the plant | CCP 7–10 data |
| Laboratory | Receive and check sealed samples, analyse honey, sign the report | Sample receipt, lab report (PDF) and results (CCP 1) |
| Wholesale, Retail | Receive goods, store them, sell or ship them on | Shipment confirmation, storage record (CCP 10), new shipments |
| Central association | Check beekeepers' associations | Verification of associations (when configured) |
| Public authority | Inspect | Reads records |

## Organization types {#org-types}

The platform has no separate list of organization types: an organization's type is the set of capabilities it has. Typical organizations are listed in [Table](#tbl-org-types).

Table: Typical organizations and their capabilities {#tbl-org-types}

| Organization | Capabilities | Typical roles of its members |
|---|---|---|
| Beekeepers' association | **Association** | Organization admin, Field inspector |
| Processing plant or packer | **Processing**, **Packing**, and **Wholesale** or **Retail** for storage at the plant | Organization admin, Staff, Sample acceptance officer, Quality Assurance Officer |
| Food-analysis laboratory (accredited or in-house) | **Laboratory** | Organization admin, Analyst, Lab manager |
| Wholesaler | **Wholesale** | Organization admin, Staff, Quality Assurance Officer |
| Retailer | **Retail** | Organization admin, Staff, Quality Assurance Officer |
| Ministry or inspection body | **Public authority** | Organization admin (read-only) |

An association that runs its own processing plant also has **Processing** (and **Packing**), and then also the roles of a plant.

### Organization status {#org-status}

A platform administrator verifies, suspends and reinstates organizations. If the platform is configured for it, the organization admin of a verified **Central association** can also verify, suspend and reinstate beekeepers' associations. Verifying needs at least one uploaded document; suspending needs a reason. [Table](#tbl-org-status) lists the statuses.

Table: Organization status {#tbl-org-status}

| Status | Meaning |
|---|---|
| **Pending** | New organization, waiting for verification. Members can read and the organization admin can upload documents. Nobody can enter data. |
| **Verified** | Checked by a platform administrator (or, for associations, by the central association when configured). Members can work according to their roles. |
| **Suspended** | Verification withdrawn (with a reason). Works like Pending until reinstated. |

## Roles {#role-list}

[Table](#tbl-roles) is the reference list of roles. Every other table in this manual uses the same role names in the same order.

Table: Roles {#tbl-roles}

| Role (in this manual) | Code | Typical person | Main work |
|---|---|---|---|
| **Organization admin** | OrgAdmin | Manager of the organization | Uploads documents, requests finalizing, ships and confirms shipments, records beekeeper consents (associations), reads deliveries and samples |
| **Staff** | Staff | Production, filling or retail staff | Enters CCP 1–10 data, requests finalizing, ships and confirms shipments, records storage |
| **Field inspector** | FieldInspector | Association field technician | Reads records; enters CCP 0 only if an administrator gives this CCP to the association |
| **Sample acceptance officer** | SampleAcceptanceOfficer | Reception officer at the plant | Records deliveries, enters CCP 0, takes and sends samples |
| **Quality Assurance Officer** | QualityApprover | Quality manager, food engineer | Approves CCP records and storage records, confirms finalizing |
| **Analyst** | Analyst | Laboratory analyst | Receives samples, enters lab results, submits reports (the first person at the laboratory) |
| **Lab manager** | LabManager | Laboratory manager | Receives samples, approves or returns lab reports (the second person at the laboratory) |
| **Platform administrator** | (global role) | Platform operator | Administration (chapter 12); reads all workflow data but enters and approves none |

A platform administrator is not a member of a business organization's workflow: they can read every record but **cannot enter or approve workflow data** (deliveries, CCP data, lab results, shipments, consents). When data is wrong after approval, an administrator requests a *correction*, which a second administrator approves (see [Corrections](#admin-corrections)). Administrators do keep the master data: beekeepers, apiaries, lots, batches and their composition (see [chapter 12](#admin)).

## Data rights (CRUD) {#crud}

[Table](#tbl-crud) shows the default data rights of each role on each kind of record, in the CRUD model: **C** create, **R** read, **U** update, **D** delete, and **A** approve (a separate right). "–" means no right. A member's rights apply only to the records of their own organization's work (its own lots and batches, batches it holds, its association's beekeepers) and, except for reading, only while the organization is verified. The rights come from the code and the default settings; [Table](#tbl-admin-changeable) lists which of them an administrator can change.

Table: Default data rights per role (CRUD + Approve) {#tbl-crud}

| Record | Org admin | Staff | Field inspector | Sample acceptance officer | Quality Assurance Officer | Analyst | Lab manager | Platform administrator | Public visitor |
|---|---|---|---|---|---|---|---|---|---|
| Beekeeper | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Apiary | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Raw honey lot | R | R | R | C R ⁴ | R | – | – | C R U D ⁵ | R ³ |
| Sample | R | – | – | C R | – | R A ⁶ | R A ⁶ | R | – |
| Lab report and results | R | R | R | R | R | C R U D ⁷ | R A ⁷ | R | R ⁸ |
| CCP record (data, "Not applicable") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | R ¹⁰ | R ³ |
| Batch | R, request finalizing | R, request finalizing | R | R | R, confirm finalizing (A) | R | R | C R U D ¹¹ | R ³ |
| Shipment | C R A ¹² | C R A ¹² | R | R | R | R | R | R | – |
| Storage record at the receiver (CCP 10) | R | C R U D | R | R | R A | R | R | R | – |
| Beekeeper consent | C R U ¹³ | R | R | R | R | R | R | R | R ¹⁴ |
| Organization (data, documents) | R; documents C | R | R | R | R | R | R | C R U, status A ¹⁵ | – |
| Membership | R | R | R | R | R | R | R | C R U ¹⁶ | – |
| Audit entry | – | – | – | – | – | – | – | R ¹⁷ | – |

Notes:

1. Read only through the records the organization works on (CCP pages, recall report). A later holder of a batch reads the data without beekeeper and apiary names. Members of an association read the list of their member beekeepers.
2. Beekeepers and apiaries are never deleted, only deactivated. Apiary photos can be added, changed and deleted.
3. On the public pages of finalized batches (Honey Story) and on published beekeeper pages: names, province and approved data; never coordinates or contact data (see [What the public can read](#trust-public)).
4. A lot is created by recording a delivery, at a verified organization with the **Processing** capability.
5. Administrators keep lot master data (for example the owning organization and the CCP assignment). Nothing can be changed once the lot is in a finalized batch; a lot that has deliveries, samples or CCP data in use cannot be deleted.
6. **A** = receive the sample with an intact seal, or reject it.
7. The analyst creates, saves, changes and submits a report while it is a draft; the lab manager approves or returns it. An approved report cannot be changed.
8. The **Verify a lab report** page, by report code.
9. Enter data (C), change or clear it (U, D) and mark "Not applicable", until the record is approved or locked. Default entering roles: Sample acceptance officer for CCP 0–1, Staff for CCP 1–10. The field inspector enters CCP 0 only if an administrator gives CCP 0 to the association.
10. Administrators change approved or locked data only through a correction requested by one administrator and approved by a second.
11. Create a batch, compose it from lots, assign its CCPs and set its owner. A batch can be deleted only before it is finalized. Administrators cannot finalize.
12. **C** = ship a finalized batch that your organization holds; **A** = confirm or reject a shipment that your organization receives.
13. Record (C) and withdraw (U) a consent; associations only.
14. The published beekeeper page.
15. Verify, request documents, suspend and reinstate (see [Organization status](#org-status)). Organizations are never deleted.
16. Add a membership and revoke it (U); memberships are never deleted.
17. The audit log can only be added to; nobody can change or delete an entry.

**What can be deleted.** Members only clear their own data before it is approved (CCP data, draft lab results, storage data). Administrators can delete lots and batches before finalizing, apiary photos, reference data and user accounts (never their own). Beekeepers, apiaries, organizations, documents, memberships, consents, samples, lab reports, shipments and audit entries are never deleted.

## Permission matrix {#permission-matrix}

[Table](#tbl-permissions) shows the default rights per **activity in the platform** (a function such as "record a delivery"), as they are configured today. It complements the data rights of [Table](#tbl-crud) and uses the same roles in the same order. Legend: **R** read, **C** create or enter, **A** approve (or receive), **F** finalize (request or confirm), **–** no right. The column "Capability needed" says which capability the organization needs.

Table: Default rights per activity in the platform and role {#tbl-permissions}

| Activity in the platform | Capability needed | Org admin | Staff | Field inspector | Sample acceptance officer | Quality Assurance Officer | Analyst | Lab manager | Platform administrator |
|---|---|---|---|---|---|---|---|---|---|
| Record a delivery (intake) | Processing | R | – | – | C R | – | – | – | R |
| Take and send a sample | Processing (lot owner) | R | – | – | C R | – | R | R | R |
| Receive a sample (seal check) | Laboratory | – | – | – | – | – | A | A | R |
| Enter lab results, submit report | Laboratory | R | R | R | R | R | C R | R | R |
| Approve or return a lab report | Laboratory | R | R | R | R | R | R | A R | R |
| CCP 0 reception check (lot) | Processing (lot owner) | R | R | R | C R | A R | R | R | R |
| CCP 1 laboratory data (lot) | written from the approved lab report | R | R | R | R | R | R | R | R |
| CCP 2–3 (lot) | Processing (lot owner) | R | C R | R | R | A R | R | R | R |
| Create a batch, compose it, assign CCPs | – | – | – | – | – | – | – | – | C |
| CCP 4–6 (batch) | Processing (batch owner) | R | C R | R | R | A R | R | R | R |
| CCP 7–9 (batch) | Packing (batch owner) | R | C R | R | R | A R | R | R | R |
| CCP 10 storage at the plant (batch) | Wholesale, Retail or Packing (batch owner) | R | C R | R | R | A R | R | R | R |
| Request finalizing | owner, verified | F | F | – | – | – | – | – | – |
| Confirm finalizing (seal) | owner, verified | – | – | – | – | F | – | – | – |
| Create a shipment | holder, verified | C R | C R | – | – | R | – | – | R |
| Confirm or reject a received shipment | receiver, verified | A R | A R | – | – | R | – | – | R |
| CCP 10 storage record at the receiver | Wholesale, Retail or Packing | R | C R | R | R | A R | R | R | R |
| Recall report (own scope) | – | R | R | R | R | R | R | R | R (all) |
| Record or withdraw a beekeeper's consent | Association | C R | – | – | – | – | – | – | R |
| Upload organization documents | own organization | C | – | – | – | – | – | – | C |
| Verify organizations, manage memberships and capabilities | – | – | – | – | – | – | – | – | C A |
| Verify beekeeper registry numbers | – | – | – | – | – | – | – | – | A |
| Change permissions, CCPs, limits, translations | – | – | – | – | – | – | – | – | C |
| Request or decide a correction | – | – | – | – | – | – | – | – | C A (two different administrators) |
| Audit log | – | – | – | – | – | – | – | – | R |

Notes:

- **Read of CCPs (R)**: every role may open the CCPs of the lots and batches its organization takes part in: as owner or responsible organization, as the organization that took a sample (for lots), as a later holder of the batch, or as a verified public authority. A holder reads the batch's CCPs and those of its lots **read-only**. Any other organization gets the "You can't open this page" (403) page.
- **CCP 1** is filled from the approved laboratory report. Its configured entering roles (Sample acceptance officer, Staff) are only used for CCP 1 parameters that the laboratory report does not cover.
- **Shipments and finalizing** check the organization's role and status, not a capability: any verified owner of a finalized batch can ship it.
- The administrator can give **CCP 0** to the beekeeper's association instead of the plant (**CCP responsibilities** → CCP 0 → **The association of the lot's beekeeper**, capability Association, role Field inspector). This is not the default.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Roles and permissions**: the default rights of the role Staff.

### What an administrator can change {#admin-changeable}

[Table](#tbl-admin-changeable) lists every setting a platform administrator can change. Everything else is fixed: the list of roles, the list of capabilities and the [Ground rules](#fixed-rules).

Table: What a platform administrator can change {#tbl-admin-changeable}

| Setting | Where | What exactly can be changed |
|---|---|---|
| Role permissions | **Roles and permissions** → **Role permissions** | Per role: **Enter**, **Approve** and **Read** for each CCP; **Enter** and **Read** for deliveries (intake); **Enter**, **Approve** (receive) and **Read** for samples; **Enter**, **Approve** and **Read** for lab results; **Enter** (ship), **Approve** (confirm receipt) and **Read** for shipments; **Enter** and **Read** for beekeeper pages; **Read** for the recall report; **Enter** (request) and **Approve** (confirm) for finalizing. Every right keeps at least one role. |
| CCP responsibilities | **CCP responsibilities** | Per CCP: the responsible organization (the owner of the lot or batch, or, for lots, the beekeeper's association), the capabilities it needs, the entering roles, the approving roles, and whether approval is required. |
| Capabilities | Organization page; **Organization capabilities** | Add or remove capabilities of an organization (at least one remains); laboratory accreditation number and validity. |
| Four-eyes exemption | Organization page | Per organization: allows its members to approve their own CCP records and storage records and to confirm their own finalizing request; always flagged in the audit log. Never for lab results. |
| Organization status | Organization page | Verify, request documents, suspend, reinstate. |
| Memberships and users | Organization page; **Members**; **User Management** | Add and revoke memberships; create users, set passwords, unlock, delete accounts. |
| Control points, parameters, limits | **Critical Control Points** | Parameters, data types, required questions, limits, "Limits verified". |
| Master and reference data | Administration menus | Beekeepers, apiaries, registry numbers, lots, batches, honey types, periods, translations. |

## Ground rules {#fixed-rules}

These rules are built into the platform; no setting switches them off:

- **Only verified organizations enter data.**
- **Four-eyes on CCP records**: nobody approves a CCP record in which they recorded data, or that they marked "Not applicable". Exception: a platform administrator can grant an organization the **Four-eyes exemption**; members may then approve their own CCP records, and every such approval is flagged in the audit log.
- **Four-eyes on lab results, always**: the person who entered the results cannot approve them. There is no exemption.
- **Four-eyes on finalizing and on storage records**: the person who requested finalizing cannot confirm it, and the person who recorded a storage record cannot approve it (unless the organization has the exemption).
- **Any change removes the approval**: changing data clears an existing approval; data outside its limits must be confirmed explicitly when it is approved.
- **Lab results follow the sample**: results are entered only for a received sample with an intact seal and only after the lot's CCP 0 is approved; an approved report cannot be changed.
- **Platform administrators are read-only** on workflow data and correct it only through the two-administrator correction process.
- **Finalized means locked**: a batch is finalized only when every CCP of the batch and of its lots is done; a finalized batch and all its lots can no longer be changed, except through a correction. There is no "un-finalize".
- **Shipments**: only finalized batches, only to another verified organization, at most the quantity held.
- **Organizations are configured by administrators only**: capabilities, memberships and the exemption.
- **Every right keeps at least one role.**
- **The audit log can only be added to**; nothing in it is changed or deleted, and every change on the permission pages is itself logged.
- **Beekeeper pages** are public only with an active consent, a verified registry number and a verified association.

## Multiple memberships {#memberships}

A **membership** is one user account × one organization × one role. One person can hold several memberships: in different organizations, or several roles in the same organization (for example **Quality Assurance Officer** and **Organization admin** at an association). You always work in one membership at a time, and rights are not combined: holding two roles never lets you approve data you entered yourself.

To switch:

1. Click the **organization · role chip** in the top bar (see [The top bar and your role](#top-bar)). A list headed **Your roles** shows every active membership; the current one is ticked.
2. Click another membership. The platform opens **My organization** in the new membership.

The menu, the tasks in **My work**, the records you can read and the actions you may take all follow the active membership. The choice lasts until you close the browser; after that you start again in your first membership (alphabetically by organization name). **My organization** also has an **Act as** list with a **Switch** button when you have two or more memberships.
