# 12. Platform administrator {#admin}

## Your daily work {#admin-daily}

Platform administrators run the platform. They verify organizations, manage members and permissions, keep the reference data and control point definitions, register beekeepers, create batches (for now), decide corrections and watch the audit log.

Administrators can read every record but **do not enter or approve business data**. They land on **My work**, which shows **Organizations to verify**, **Registry numbers to verify** and **Corrections to decide**. The **Dashboard** gives an overview (figures, the chain, a map of beekeepers and apiaries by province, recent activity and approvals waiting).

<!-- SHOT: admin-dashboard -->

The administration menu:

| Menu | Entries |
|---|---|
| **Dashboard** | Overview |
| **Beekeepers** | Beekeepers list, Apiaries, education levels, beekeeping purposes, beekeeping types |
| **Raw Honey Production** | Raw honey lots, honey types, production periods |
| **Wholesalers** | Wholesalers list, add a wholesaler |
| **Final Honey Productions** | Batches |
| **Critical Control Points** | CCPs, parameter categories, parameters, translations of reference data |
| **User Management** | Users, roles, **Audit log** |
| **Organizations** | My work, organizations, capabilities, registry, roles and permissions, CCP responsibilities, corrections, shipments, samples, beekeeper pages |

## Organizations {#admin-organizations}

### Create an organization {#admin-org-create}

1. **Organizations** → **New organization**.
2. Enter the name, legal name, tax number, province and district.
3. If the organization is a wholesaler already in the platform, choose it under the existing wholesaler field. **A processing plant must be linked to its wholesaler record**: the lots it receives are filed under that wholesaler, and a batch can only use lots of its own wholesaler. The link can also be set later on the organization's **Edit** page. Lots that a plant received before it was linked are taken by the batches of the wholesaler it is linked to.
4. Tick at least one capability and click **Create**.

The organization starts as **Pending**: "Organization created. It stays Pending until it is verified."

<!-- SHOT: admin-org-create -->

### Verify, request documents, suspend {#admin-org-verify}

On the organization page:

- **Verify** (from Pending) needs at least one uploaded document.
- **Request documents** (while Pending) needs a reason, for example which document is missing.
- **Suspend** (while Verified) needs a reason. **Reinstate** verifies a suspended organization again.

Every change appears in the verification history and in the audit log.

### Capabilities {#admin-org-capabilities}

**Add capability** adds a capability. For **Laboratory**, tick accredited and enter the accreditation number and its validity date; an accredited laboratory needs a number. **Remove** removes one; an organization keeps at least one capability. The **Capabilities** page shows, per capability, which CCPs need it and which organizations have it. The tab **Organization capabilities** under **Roles and permissions** lets you edit all organizations at once.

### Members {#admin-members}

1. On the organization page, under members, choose the role.
2. Enter an existing user name, or a new user name, e-mail and password to create the user at the same time.
3. Click **Add member**: "… added as …".

A user can have several memberships. **Revoke** ends a membership and signs the user out everywhere. The tab **Members** under **Roles and permissions** lists all memberships of all organizations.

<!-- SHOT: admin-org-members -->

### Four-eyes exemption {#admin-four-eyes}

Small organizations with one quality person can get the **Four-eyes exemption**: members may then approve their own CCP records and confirm their own finalizing requests. Every such approval is flagged in the audit log. The exemption never applies to lab results.

## Users {#admin-users}

**User Management** → **Users** lists the accounts.

- **Add New User** creates an account. With the organization model, leave **Global role** empty for organization members and give them a membership on the organization page; choose **Platform administrator** only for administrators.
- **Reset Password** sets a new password for a user.
- **Unlock** appears next to a **Locked** account and ends the lock at once ("Account unlocked."). Without it, the lock ends by itself after 15 minutes.
- **Delete** removes an account (use with care; it cannot be undone). You cannot delete your own account. Every deletion and every unlock is written to the audit log.

Passwords need at least 6 characters. After 5 failed sign-ins an account is locked for 15 minutes.

**User Management** → **Roles** lists the global roles. The roles the platform relies on (**Administrator** and **Beekeeper**) are marked **Protected** and cannot be renamed or deleted.

## Roles and permissions {#admin-permissions}

**Roles and permissions** has three tabs:

- **Members**: every active membership; add and revoke.
- **Role permissions**: choose a role and tick, for each function, **Enter**, **Approve** and **Read**. The functions are the CCPs (one row each) and the areas intake, samples, lab results, shipments, beekeeper pages, recall and finalize. Every right must keep at least one role. Changes apply at once.
- **Organization capabilities**: capabilities per organization.

The box "Fixed rules (cannot be switched off):" lists the rules that no setting changes (see [Fixed rules](#fixed-rules)).

<!-- SHOT: admin-role-permissions -->

## CCP responsibilities {#admin-ccp-responsibilities}

**CCP responsibilities** decides, per CCP, which organization is responsible (the owner of the lot or batch, or, for lots, the beekeeper's association), which capability it needs, which roles enter and which roles approve, and whether approval is required. Example: give CCP 0 to the association by choosing **The association of the lot's beekeeper**, **Association** and **Field inspector**.

## Control points, parameters and limits {#admin-ccps}

- **Critical Control Points**: code, title, processing step, criticality justification and **Applies To** (raw honey lot or final batch). The option to show free-text values on the public Honey Story should stay off when answers may contain personal data.
- **Parameters**: name, unit, category, data type (**Number**, **Yes / No**, **Text**, **File**), minimum and maximum, required or optional, and "required only when" another yes/no question has a given answer.
- **Limits**: per parameter, default limits for every honey type and specific limits by honey class (floral, honeydew, pine) or honey type, optionally with a condition, a sum or a ratio. Stored results are not recalculated when limits change.

**Verified limits.** Each parameter has the box **Limits verified (shown publicly)**. Only verified limits produce a public within/out-of-limit verdict on the Honey Story and the beekeeper page. Unverified limits are marked "to verify" in the panel, and the public pages say that limits are still being checked.

<!-- SHOT: admin-parameter-limits -->

## Reference data and translations {#admin-reference}

Honey types (with their honey class for the limits), production periods, education levels, beekeeping types and purposes, and wholesalers are kept under their menus. A record that is in use cannot be deleted.

**Translations of reference data** holds the names shown in lists, tables and public pages in the five languages: honey types, periods, statuses, capabilities, roles, CCPs, parameters, parameter categories, limit notes, parameter explanations and countries. An empty field falls back to English, then to the base text.

## Beekeepers, apiaries and the registry {#admin-registry}

- **Beekeepers**: add a beekeeper (name, birth date, experience, phone, education, beekeeping type and purpose, optional photo). The business number is generated. Beekeepers do not sign in. They are never deleted, only deactivated.
- **Apiaries**: title, number of hives and location (latitude and longitude). The location is never shown publicly; public pages show the province only.
- **Beekeeper registry and associations**: per beekeeper, link the association, and enter the enterprise registry number (format `TR` + province code + 10 digits, for example `TR061234567890`) with a proof document. Then **Verify** or **Reject** (a reason is required to reject).

<!-- SHOT: admin-registry -->

## Batches (admin-only for now) {#admin-batches}

1. **Wholesalers** → the wholesaler → **Final Honey Productions (Batches)** → **New Batch**.
2. Enter the **Batch Title** and **Production Date** and click **Create Batch**. The QR code is generated.
3. On **Manage Batch**, enter the amount to use from each eligible lot (lots of this wholesaler whose CCPs are all approved) and click **Save Batch Composition**: "Batch composition saved: … kg in total." If an amount is too large or a lot cannot be used, the page lists each problem (for example "Lot #…: at most … kg is available.") and saves nothing. If a plant has lots that no batch can take because it is not linked to a wholesaler record, the page names it with a link to its **Edit** page.
4. **Edit** the batch to set the owning organization (the plant).
5. **Manage CCP**: tick CCP 4 to CCP 10 and click **Save Assignments**: "CCP assignments saved: … added, … removed."

The plant then enters the batch CCPs and finalizes the batch. Administrators cannot finalize: "With the organization model, the owning organization finalizes the batch (request and approval by two people)."

<!-- SHOT: admin-manage-batch -->

## Corrections {#admin-corrections}

Approved or locked data is changed only through a correction:

1. Open the CCP page of the record. The **Corrections** box lets you choose the parameter, enter the new value and a **reason**, and click **Request**: "Correction requested; another platform administrator must approve it."
2. A **second** administrator opens **Corrections**, reads the old and new value and the reason, and clicks **Approve and apply** or **Reject** (a note is required to reject).

The person who requested a correction cannot decide it. Corrections also apply to locked records, and both steps are in the audit log.

## Beekeeper pages, shipments and samples {#admin-overviews}

Administrators can open **Beekeeper pages**, **Shipments and stock** and **Samples and lab** read-only, to follow the work of all organizations.

## Recall {#admin-recall}

The **Recall report** of a batch or lot lists:

- **upstream**: the lots, apiaries, beekeepers and delivery forms;
- the batches;
- **downstream**: the shipments and the current holders.

Members see the part their organization may see (owner, holder or association); hidden rows are counted. **Print** and **CSV** export the report.

<!-- SHOT: recall-report -->

## Audit log {#admin-audit}

See [The audit log](#trust-audit).
