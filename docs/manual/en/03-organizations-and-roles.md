# 3. Organizations, roles and permissions {#roles}

In TOP4HoneyChains a person never acts alone: every action is done **for an organization, in a role**. Three things decide what you may do:

1. the **capabilities** of your organization (what kind of work it does);
2. your **role** in the organization (who you are inside it);
3. the **status** of your organization: only a **Verified** organization can enter data.

On top of these, some rules are fixed in the code and cannot be switched off (see [Fixed rules](#fixed-rules)).

## Organization types (capabilities) {#capabilities}

An organization has one or more capabilities. A platform administrator sets them.

| Capability | Typical organization | What it allows |
|---|---|---|
| **Association** | Beekeepers' association | Links member beekeepers; records beekeepers' consent for a public beekeeper page |
| **CentralAssociation** | National association | Optional: may verify associations, if the platform is configured for it |
| **Processing** | Honey processing plant | Records deliveries, takes samples, records lot CCP 0–3 and batch CCP 4–6 |
| **Packing** | Filling and packing plant | Records batch CCP 7–9 (filling, labelling, packing) and CCP 10 |
| **Laboratory** | Analysis laboratory (accredited or in-house) | Receives samples, enters and approves lab reports |
| **Wholesale** | Wholesaler | Holds batches; records CCP 10 storage |
| **Retail** | Shop or market | Holds batches; records CCP 10 storage |
| **PublicAuthority** | Ministry, inspection body | Reads records (read-only) |
| **Beekeeping** | Beekeeping enterprise | Reserved for beekeeping enterprises; beekeepers do not sign in |
| **PlatformAdmin** | The platform operator | Reserved for the platform organization |

A processing plant that also fills and stores its own honey needs **Processing**, **Packing** and **Wholesale** (or **Retail**), because batch CCP 7–9 need Packing and CCP 10 needs Wholesale, Retail or Packing.

### Organization status {#org-status}

| Status | Meaning |
|---|---|
| **Pending** | New organization, waiting for verification. Members can read and the organisation admin can upload documents. Nobody can enter data. |
| **Verified** | Checked by a platform administrator. Members can work according to their roles. |
| **Suspended** | Verification withdrawn (with a reason). Works like Pending until reinstated. |

## Roles {#role-list}

| Role (on screen) | Code | Typical person | Main work |
|---|---|---|---|
| **Organisation admin** | OrgAdmin | Manager of the organization | Uploads documents, requests finalizing, ships and confirms shipments, records beekeeper consents (associations), reads intake and samples |
| **Staff** | Staff | Production, filling or shop staff | Enters CCP 2–10 values, requests finalizing, ships and confirms shipments, records storage |
| **Field inspector** | FieldInspector | Association field technician | Reads records; enters CCP 0 only if an administrator gives this CCP to the association |
| **Sample acceptance officer** | SampleAcceptanceOfficer | Reception officer at the plant | Records deliveries, enters CCP 0, takes and sends samples |
| **Quality approver** | QualityApprover | Quality manager, food engineer | Approves CCP records, confirms finalizing, approves storage records |
| **Analyst** | Analyst | Laboratory analyst | Receives samples, enters lab results, submits reports |
| **Lab manager** | LabManager | Laboratory manager | Receives samples, approves or returns lab reports |
| **Platform administrator** | (global role) | Platform operator | Administration (chapter 12); read-only on business data |

A platform administrator is not a member of a business organization's workflow: they can read every record but **cannot enter or approve business data** (deliveries, CCP values, lab results, shipments, consents). When data is wrong after approval, an administrator requests a *correction*, which a second administrator approves (see [Corrections](#admin-corrections)).

## Permission matrix {#permission-matrix}

The matrix below shows the **default** rights as they are configured today. A platform administrator can change most of them under **Roles and permissions** and **CCP responsibilities**; the fixed rules below cannot be changed.

Legend: **V** view, **C** create or enter, **A** approve (or receive), **F** finalize (request or confirm), **–** no right. The organization column says which capability the organization needs.

| Action | Capability needed | Org admin | Staff | Field inspector | Sample acceptance officer | Quality approver | Analyst | Lab manager | Platform admin |
|---|---|---|---|---|---|---|---|---|---|
| Record a delivery (intake) | Processing | V | – | – | C V | – | – | – | V |
| Take and send a sample | Processing (lot owner) | V | – | – | C V | – | V | V | V |
| Receive a sample (seal check) | Laboratory | – | – | – | – | – | A | A | V |
| Enter lab results, submit report | Laboratory | V | V | V | V | V | C V | V | V |
| Approve or return a lab report | Laboratory | V | V | V | V | V | V | A V | V |
| CCP 0 reception check (lot) | Processing (lot owner) | V | V | V | C V | A V | V | V | V |
| CCP 1 laboratory values (lot) | written from the approved lab report | V | V | V | V | V | V | V | V |
| CCP 2–3 (lot) | Processing (lot owner) | V | C V | V | V | A V | V | V | V |
| Create a batch, compose it, assign CCPs | – | – | – | – | – | – | – | – | C |
| CCP 4–6 (batch) | Processing (batch owner) | V | C V | V | V | A V | V | V | V |
| CCP 7–9 (batch) | Packing (batch owner) | V | C V | V | V | A V | V | V | V |
| CCP 10 storage at the plant (batch) | Wholesale, Retail or Packing (batch owner) | V | C V | V | V | A V | V | V | V |
| Request finalizing | owner, verified | F | F | – | – | – | – | – | – |
| Confirm finalizing (seal) | owner, verified | – | – | – | – | F | – | – | – |
| Create a shipment | holder, verified | C V | C V | – | – | V | – | – | V |
| Confirm or reject a received shipment | receiver, verified | A V | A V | – | – | V | – | – | V |
| CCP 10 storage record at the receiver | Wholesale, Retail or Packing | V | C V | V | V | A V | V | V | V |
| Recall report (own scope) | – | V | V | V | V | V | V | V | V (all) |
| Record or withdraw a beekeeper's consent | Association | C V | – | – | – | – | – | – | V |
| Upload organization documents | own organization | C | – | – | – | – | – | – | C |
| Verify organizations, manage members and capabilities | – | – | – | – | – | – | – | – | C A |
| Verify beekeeper registry numbers | – | – | – | – | – | – | – | – | A |
| Change permissions, CCPs, limits, translations | – | – | – | – | – | – | – | – | C |
| Request or decide a correction | – | – | – | – | – | – | – | – | C A (two different admins) |
| Audit log | – | – | – | – | – | – | – | – | V |

Notes:

- **View of CCPs (V)**: every role may open the CCPs of the lots and batches its organization takes part in: as owner or responsible organization, as the organization that took a sample (for lots), as a later holder of the batch, or as a verified public authority. A holder sees the batch's CCPs and those of its lots **read-only**. Any other organization gets the "You can't open this page" (403) page.
- **CCP 1** is filled from the approved laboratory report. Its configured entering roles (Sample acceptance officer, Staff) are only used for CCP 1 parameters that the laboratory report does not cover.
- **Shipments and finalizing** check the organization's role and status, not a capability: any verified owner of a finalized batch can ship it.
- The administrator can give **CCP 0** to the beekeeper's association instead of the plant (**CCP responsibilities** → CCP 0 → **The association of the lot's beekeeper**, capability Association, role Field inspector). This is not the default.

<!-- SHOT: admin-permissions-matrix -->

## Fixed rules {#fixed-rules}

These rules are built into the platform:

- **Four-eyes on CCP records**: nobody approves a CCP record in which they recorded values, or that they marked "Not applicable". Exception: a platform administrator can grant an organization the **Four-eyes exemption**; members may then approve their own CCP records, and every such approval is flagged in the audit log.
- **Four-eyes on lab results, always**: the person who entered the results cannot approve them. There is no exemption.
- **Four-eyes on finalizing**: the person who requested finalizing cannot confirm it (unless the organization has the exemption).
- **Only verified organizations enter data.**
- **Platform administrators are read-only** on business data and correct it only through the two-administrator correction process.
- **Finalized means locked**: a finalized batch and all its lots can no longer be changed, except through a correction.
- **The audit log can only be added to**; nothing in it is changed or deleted, and every change on the permission pages is itself logged.
