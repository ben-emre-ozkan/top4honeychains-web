# 15. Platform administrator {#admin}

## Your daily work {#admin-daily}

Platform administrators run the platform. They verify organizations, manage memberships and permissions, send sign-in details, publish the notice texts, keep the reference data and control point definitions, register beekeepers, create batches (for now), decide corrections and watch the audit log.

Administrators can read every record but **do not enter or approve business data**. They land on **My work**, which shows **Organizations to verify**, **Registry numbers to verify** and **Corrections to decide**. The **Dashboard** gives an overview (figures, the chain, a map of beekeepers and apiaries by province, recent activity and approvals waiting).

<!-- SHOT: admin-dashboard -->
Figure: The platform administrator's dashboard.

The administration menu ([Table](#tbl-admin-menu)):

Table: The administration menu {#tbl-admin-menu}

| Menu | Entries |
|---|---|
| **Dashboard** | Overview |
| **Beekeepers** | Beekeepers list, Apiaries, education levels, beekeeping purposes, beekeeping types |
| **Raw Honey Production** | Raw honey lots, honey types, production periods |
| **Wholesalers** | Wholesalers list, add a wholesaler |
| **Final Honey Productions** | Batches |
| **Critical Control Points** | CCPs, parameter categories, parameters, translations of reference data |
| **User Management** | Users, roles, **Audit log**, **Data retention**, **Notices and consent texts**, **SMS outbox (test provider)** (only with the test SMS provider) |
| **Organizations** | My work, organizations, capabilities, registry, beekeeper registration, roles and permissions, CCP responsibilities, corrections, shipments, samples, beekeeper pages |

## Organizations {#admin-organizations}

### Create an organization {#admin-org-create}

1. **Organizations** → **New organization**.
2. Enter the name, legal name, tax number, province and district.
3. If the organization is a wholesaler already in the platform, choose it under the existing wholesaler field. **A processing plant must be linked to its wholesaler record**: the lots it receives are filed under that wholesaler, and a batch can only use lots of its own wholesaler. The link can also be set later on the organization's **Edit** page. Lots that a plant received before it was linked are taken by the batches of the wholesaler it is linked to.
4. Tick at least one capability and click **Create**.

The organization starts as **Pending**: "Organization created. It stays Pending until it is verified."

<!-- SHOT: admin-org-create -->
Figure: Creating an organization: wholesaler link and capabilities.

### Verify, request documents, suspend {#admin-org-verify}

On the organization page (a platform administrator; for beekeepers' associations the organization admin of a verified central association instead, when the platform is configured for it: administrators then get "access denied" for associations):

- **Verify** (from Pending) needs at least one uploaded document, and, for an organization verified for the first time, an active Compliance Officer.
- **Request documents** (while Pending) needs a reason, for example which document is missing.
- **Suspend** (while Verified) needs a reason. **Reinstate** verifies a suspended organization again.

Every change appears in the verification history and in the audit log.

### Capabilities {#admin-org-capabilities}

**Add capability** adds a capability. For **Laboratory**, tick accredited and enter the accreditation number and its validity date; an accredited laboratory needs a number. **Remove** removes one; an organization keeps at least one capability. The **Capabilities** page shows, per capability, which CCPs need it and which organizations have it. The tab **Organization capabilities** under **Roles and permissions** lets you edit all organizations at once.

### Members {#admin-members}

1. On the organization page, under members, choose the role. The role **Beekeeper registration officer** is available only in an organization with the capability Beekeeper registration.
2. Enter an existing user name, or a new user name, e-mail and **Mobile phone** to create the user at the same time. Choose how the user gets the sign-in details: **Send the user name and a temporary password by SMS (recommended)**, or **Set an initial password here (the user changes it at the first sign-in)**.
3. Click **Add member**: "… added as …".

A membership is one user account × one organization × one role; a user can have several. **Revoke** ends a membership and signs the user out everywhere. The tab **Members** under **Roles and permissions** lists all memberships of all organizations.

<!-- SHOT: admin-org-members -->
Figure: Memberships of an organization and the **Add member** form.

### Four-eyes exemption {#admin-four-eyes}

Small organizations with one quality person can get the **Four-eyes exemption**: members may then approve their own CCP records and confirm their own finalizing requests. Every such approval is flagged in the audit log, and the organization's Compliance Officer sees it under **Approval mechanism**. The exemption never applies to lab results or to the review of signed photos.

### Data controller {#admin-data-controller}

Each organization is the data controller for the personal data it records. On the organization page, **Data controller** holds its **Legal name**, **Contact (e-mail or phone)** and **Address**; they are named in the organization's privacy notice. The organization's Compliance Officer or a platform administrator keeps them up to date.

## Users {#admin-users}

**User Management** → **Users** lists the accounts.

- **Add New User** creates an account. With the organization model, leave **Global role** empty for organization members and give them a membership on the organization page; choose **Platform administrator** only for administrators. Enter the **Mobile phone** and choose **Send the user name and a temporary password by SMS (recommended)**, or **Set an initial password here (the user changes it at the first sign-in)**.
- **Send sign-in by SMS** sends new sign-in details (a new temporary password, valid 72 hours) to a user who lost them. Nobody, also not the administrator, sees the temporary password.
- **Reset Password** sets a new password for a user; it also counts as an initial password that the user changes at the next sign-in.
- **Unlock** appears next to a **Locked** account and ends the lock at once ("Account unlocked."). Without it, the lock ends by itself after 15 minutes.
- **Delete** removes an account (use with care; it cannot be undone). You cannot delete your own account. Every deletion and every unlock is written to the audit log.

New passwords need at least 10 characters, not the user name and not a common password; existing passwords stay valid. After 5 failed sign-ins an account is locked for 15 minutes.

**User Management** → **Roles** lists the global roles. The roles the platform relies on (**Administrator** and **Beekeeper**) are marked **Protected** and cannot be renamed or deleted.

## Roles and permissions {#admin-permissions}

**Roles and permissions** has three tabs:

- **Members**: every active membership; add and revoke.
- **Role permissions**: choose a role and tick, for each function, **Enter**, **Approve** and **Read**. The functions are the CCPs (one row each, with the CCP abbreviation of the page's language) and the areas intake, samples, lab results, shipments, beekeeper pages, recall, finalize, beekeeper registration, evidence, price and the organization's audit entries. Every right must keep at least one role. Changes apply at once.
- **Organization capabilities**: capabilities per organization.

The box "Ground rules (cannot be switched off):" lists the rules that no setting changes (see [Ground rules](#fixed-rules)). [Table](#tbl-admin-changeable) lists exactly what an administrator can change.

<!-- SHOT: admin-role-permissions -->
Figure: Role permissions of the Quality Assurance Officer.

## CCP responsibilities {#admin-ccp-responsibilities}

**CCP responsibilities** decides, per CCP, which organization is responsible (the owner of the lot or batch, or, for lots, the beekeeper's association), which capability it needs, which roles enter and which roles approve, and whether approval is required. Example: give CCP 0 to the association by choosing **The association of the lot's beekeeper**, **Association** and **Field inspector**.

## Control points, parameters and limits {#admin-ccps}

- **Critical Control Points**: code, title, processing step, criticality justification and **Applies To** (raw honey lot or final batch). The option to show free-text values on the public Honey Story should stay off when answers may contain personal data.
- **Parameters**: name, unit, category, data type (**Number**, **Yes / No**, **Text**, **File**), minimum and maximum, required or optional, and "required only when" another yes/no question has a given answer.
- **Limits**: per parameter, default limits for every honey type and specific limits by honey class (floral, honeydew, pine) or honey type, optionally with a condition, a sum or a ratio. Stored results are not recalculated when limits change.

**Verified limits.** Each parameter has the box **Limits verified (shown publicly)**. Only verified limits produce a public within/out-of-limit verdict on the Honey Story and the beekeeper page. Unverified limits are marked "to verify" in the panel, and the public pages say that limits are still being checked.

<!-- SHOT: admin-parameter-limits -->
Figure: Limits of a parameter (CCP 1, moisture).

## Reference data and translations {#admin-reference}

Honey types (with their honey class for the limits), production periods, education levels, beekeeping types and purposes, and wholesalers are kept under their menus. A record that is in use cannot be deleted.

**Translations of reference data** holds the names shown in lists, tables and public pages in the five languages: honey types, periods, statuses, capabilities, roles, CCPs, parameters, parameter categories, limit notes, parameter explanations and countries. An empty field falls back to English, then to the base text.

## Beekeepers, apiaries and the registry {#admin-registry}

- **Beekeeper registration**: beekeepers are registered by the beekeeper registration officers of the district directorates, with an SMS code or a signed photo (see [chapter 12](#role-ro)). A platform administrator can register beekeepers in the same way; such registrations are recorded for the platform's own organization, and their signed photos are reviewed by another platform administrator.
- **Beekeepers**: the beekeeper list and page (name, birth date, experience, phone, education, beekeeping type and purpose, optional photo). The business number is generated. Facts attested at registration (identity, registry number, phone) are read-only here; they change only with a new registration. Beekeepers are never deleted, only deactivated. Older beekeepers registered before the registration procedure are re-verified with the same form; their old logins stay locked.
- **Apiaries**: title, number of hives and location (latitude and longitude). The location is never shown publicly; public pages show the province only.
- **Beekeeper registry and associations**: per beekeeper, link the association, and enter the enterprise registry number (format `TR` + province code + 10 digits, for example `TR061234567890`) with a proof document. Then **Verify** or **Reject** (a reason is required to reject).

<!-- SHOT: admin-registry -->
Figure: Beekeeper registry and association.

## Batches (admin-only for now) {#admin-batches}

1. **Wholesalers** → the wholesaler → **Final Honey Productions (Batches)** → **New Batch**.
2. Enter the **Batch Title** and **Production Date** and click **Create Batch**. The QR code is generated.
3. On **Manage Batch**, enter the amount to use from each eligible lot (lots of this wholesaler whose CCPs are all approved) and click **Save Batch Composition**: "Batch composition saved: … kg in total." If an amount is too large or a lot cannot be used, the page lists each problem (for example "Lot #…: at most … kg is available.") and saves nothing. If a plant has lots that no batch can take because it is not linked to a wholesaler record, the page names it with a link to its **Edit** page.
4. **Edit** the batch to set the owning organization (the plant).
5. **Manage CCP**: tick CCP 4 to CCP 10 and click **Save Assignments**: "CCP assignments saved: … added, … removed."

The plant then enters the batch CCPs and finalizes the batch. Administrators cannot finalize: "With the organization model, the owning organization finalizes the batch (request and approval by two people)."

<!-- SHOT: admin-manage-batch -->
Figure: Composing a batch from eligible lots.

## Corrections {#admin-corrections}

Approved or locked data is changed only through a correction:

1. Open the CCP page of the record. The **Corrections** box lets you choose the parameter, enter the new value and a **reason**, and click **Request**: "Correction requested; another platform administrator must approve it."
2. A **second** administrator opens **Corrections**, reads the old and new value and the reason, and clicks **Approve and apply** or **Reject** (a note is required to reject).

The person who requested a correction cannot decide it. Corrections also apply to locked records, and both steps are in the audit log.

**Delivery facts.** The facts of a confirmed delivery (date, kind and weights of the containers, seal numbers) are corrected on the delivery page, under **Corrections of this delivery** ("Only for a mistake in the record. Another platform administrator must confirm the correction; the original stays readable here and the beekeeper is informed by SMS."):

1. Click **Request a correction**, change the fields, write the **Reason** and click **Send the request**: "Correction requested; another platform administrator must confirm it."
2. A second administrator compares **Original** and **Corrected** and clicks **Confirm and apply**, or writes a **Note (required to reject)** and clicks **Reject**. "Correction applied; the beekeeper was informed by SMS."

Containers are never added or removed by a correction. The delivery keeps both contents with their fingerprints; the audit log records every changed row.

<!-- SHOT: admin-delivery-correction -->
Figure: A delivery correction waiting for the second administrator.

## Beekeeper pages, shipments and samples {#admin-overviews}

Administrators can open **Beekeeper pages**, **Shipments and stock** and **Samples and lab** read-only, to follow the work of all organizations.

## Recall {#admin-recall}

The **Recall report** of a batch or lot lists:

- **upstream**: the lots, apiaries, beekeepers and delivery forms, and for deliveries with containers every container with its seal history, its sample jars, the lab values per container and whether the confirmed content is unchanged;
- the batches;
- **downstream**: the shipments and the current holders.

Members read the part their organization may read (owner, holder or association); hidden rows are counted. A member sees only its own holding, under **Your current holding**; administrators see every holder under **Current holders**. Prices are never in the recall report. **Print** and **CSV** export the report.

<!-- SHOT: recall-report -->
Figure: The recall report of a batch.

## Notices and consent texts {#admin-notices}

**Notices and consent texts** lists the privacy notices (for users and for beekeepers, of the platform and of each organization), the consent texts and the registration attestation, per language, with their **Version**, the date **Published** and how many people **Accepted** them. "Texts are versioned and never changed after publishing: publish a new version instead."

1. Click **Publish a new version** (or **New version** next to a text).
2. Choose the **Kind**, who it is **For**, the **Organization** (empty for the platform), the **Consent purpose** for a consent text, the **Version** and the **Language**, and write the **Text**.
3. Tick **Draft (not yet approved by the data-protection office)** while the text is not approved; it is then shown with the mark **DRAFT**.
4. Click **Publish**. "Publishing retires the current text of the same kind, audience, organization, purpose and language. Users who accepted an earlier version are asked again."

The public page **Privacy and consent** (`/privacy`) shows the current notices for beekeepers and for users of the platform.

## SMS {#admin-sms}

The platform sends SMS for registration codes, sign-in details, delivery confirmations, notice confirmations, password resets, phone checks and information messages. Which provider sends them is a server setting (see [Settings in the server configuration](#admin-settings)). The platform keeps a log of every message with a masked number; codes and passwords are never stored in clear. A daily limit protects the SMS budget; password resets and delivery confirmations are still sent after it is reached.

With the **test provider** (development and demonstrations only), nothing is sent: **SMS outbox (test provider)** shows every message with its text and code instead, with "Segments today / daily limit".

<!-- SHOT: admin-sms-outbox -->
Figure: The SMS outbox of the test provider.

## Settings in the server configuration {#admin-settings}

Some switches are set in the server configuration, not in the panel, and changing them needs the server's operator. Each one is switched on separately, when what it needs is ready:

Table: Switches in the server configuration {#tbl-settings}

| Setting | Off (today) | On | Switch on when |
|---|---|---|---|
| SMS provider | Messages are only written to the log | Messages are sent by the contracted provider | The SMS package is bought and the sender name is registered |
| Deliveries with containers | The plants record a delivery with one signed form and one sample (legacy) | The container procedure of [chapter 5](#sao-intake) | The plants have the tamper-evident sample jars, seals and labels |
| Notice confirmation required | A banner asks users to confirm the notice | The panel stays closed until the notice is confirmed | The notice texts are approved by the data-protection office |
| Beekeeper portal | Beekeepers get no sign-in | Registration officers send the sign-in; the portal of [chapter 14](#portal) opens | The project gives the go for the portal |
| Public names of older beekeepers | – | The Honey Story shows the neutral code until a name consent exists (default) | – (the alternative keeps the names until a beekeeper is registered again) |
| Data retention run | The **Data retention** page only counts | The run is possible (see below) | The owner approves the first run and the pseudonym format |

## Data retention {#admin-retention}

**Data retention** shows how long the platform keeps each kind of data and what happens afterwards ([Table](#tbl-retention)). The page is a **dry run**: "it counts the rows past their period and changes nothing". **Dry-run report (CSV)** downloads the counts.

Table: Retention periods {#tbl-retention}

| Data | Kept | Then |
|---|---|---|
| One-time codes | 30 days | deleted (only hashes were stored); a code that proves a record stays with it |
| SMS messages | 1 year | number and body removed, counts kept |
| Prices | 5 years | deleted |
| Lots, intakes, containers, samples, lab reports, batches | 5 years after the latest best-before of a batch (batch date + 2 years), or after the delivery | the beekeeper link pseudonymised; product data kept |
| Beekeeper identity (inactive beekeepers) | while active, then the period of their lots | pseudonymised |
| Evidence photos (signed delivery forms) | as long as the record they prove | deleted, the hash kept |
| Consents and notice acceptances | as long as the related data + 3 years (proof) | deleted |
| User accounts of former members | 2 years after the last membership ends | anonymised; audit entries keep the user id only |
| Audit log | 10 years | not changed (append-only) |

The retention run is **off** ("Off: the first run is the owner's decision"). When the owner has approved it and the platform's operator has switched it on, **Run retention now** appears; it needs the confirmation "The owner approved this run (it deletes and clears data and cannot be undone)." The run deletes old one-time codes, clears the number and text of old SMS messages and deletes old prices; it is written to the audit log. The other rules are only counted ("counts only (decision O-14)") until the format of the pseudonyms is decided. A monthly run can be switched on only after a first run by hand.

**Best-before date.** Batches carry no best-before date; the batch date plus 2 years stands in for it (a setting). The producer confirms the real shelf life of its honey; the period is then set accordingly.

## Audit log {#admin-audit}

See [The audit log](#trust-audit).
