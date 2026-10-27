# 16. Trust and privacy {#trust}

## The audit log {#trust-audit}

Every change in the platform (every save, approval, finalizing, shipment, sign-in, permission change, evidence download and retention run) is written to the **Audit log**. Platform administrators open it under **User Management** → **Audit log**; a Compliance Officer reads the entries of their own organization on the **Compliance** page.

Each row shows:

- **When**: local time; hover to see UTC;
- **Who**: the user and the organization;
- **Action**: for example Create, Update, Approve, Finalize, Verify, Login, Download, with flags such as "SelfApproved";
- **Record**: which record, with a link where an administration page exists;
- **Changes**: each changed field as **old → new**, with the field name in your language; user and organization ids are shown by name;
- **Hash**: the beginning of the row's fingerprint.

**Show raw** opens the stored data exactly as written (old and new values).

Filter by record type, action, user name or flag (for example `SelfApproved`) and click **Filter**.

<!-- SHOT: audit-log -->
Figure: The audit log with old → new changes and the raw data.

### Append-only and the hash chain {#trust-hash-chain}

The audit log can only be added to. The database rejects any attempt to change or delete a row. Each row contains a SHA-256 fingerprint of its own content and of the previous row, which forms a chain.

**Verify hash chain** recalculates the whole chain. "Hash chain: All … rows verify." means nothing has been changed. Otherwise the message names the first row that was changed or that does not point to the previous row.

## Records that do not change {#trust-locked}

- An approved CCP record keeps its approval only while its values are unchanged.
- A finalized batch and its lots are locked.
- Approved lab results become read-only CCP 1 values.
- The facts of a confirmed delivery are locked; containers, seal events, registrations, price entries, SMS messages, notices and their acceptances can only be added to.
- Corrections need two administrators and are logged with the reason.

## Codes that confirm content {#trust-codes}

The beekeeper confirms a registration and a delivery with a 6-digit code received by SMS, and the signing roles confirm the privacy notice in the same way. Each code is bound to a fingerprint of exactly the content it was sent for: changing anything afterwards voids it. Codes are valid for 5 minutes and can be tried 3 times; only a hash of the code is stored. Where no SMS is possible, a signed photo replaces the code, and a second person (the Compliance Officer) accepts or rejects it.

## Files and fingerprints {#trust-files}

Delivery forms, registration photos, lab report PDFs, organization documents, registry proofs and consent documents are stored outside the public web folder with a random name and their **SHA-256 fingerprint**. Uploads are checked by their content, not only by the file name: PDF up to 10 MB, or JPG, PNG or WEBP up to 8 MB. Before an approved lab report is used, the platform checks that the stored PDF still matches its fingerprint. Anyone can compare a report PDF with its fingerprint on the public verification page, without uploading it.

**Who downloads evidence.** Evidence files are downloaded only by the role that records them, the organization's Compliance Officer and organization admin, and platform administrators; another role of the organization gets 403, another organization 404, and every download is written to the audit log (see [Data rights](#crud)).

## Notices, consents and the data controller {#trust-notices}

**Privacy notices.** The platform and every organization publish a privacy notice: how personal data is processed, for which purpose, how long it is kept, and who the **data controller** is. Users confirm the notices of the platform and of each organization they work for (see [The privacy notice](#notice-accept)); beekeepers read their notice at registration and confirm it with their SMS code. Notices are versioned: a new version asks everyone again, and the old acceptances stay as proof. Texts not yet approved by the data-protection office carry the mark **DRAFT**. The public page **Privacy and consent** (`/privacy`) shows the current notices.

**The data controller.** Each organization is the data controller for the data it records; its legal name, contact and address are named in its notice and kept on the organization page by its Compliance Officer or a platform administrator. A beekeeper sees in the portal which organizations hold their data (**Who holds your data**).

**Consents.** Everything beyond what the work needs is optional and asked separately:

- the beekeeper's name on public pages;
- information SMS about deliveries and results;
- the association's access to the beekeeper's asking prices;
- a public beekeeper page, recorded by the association.

A consent is given at registration (confirmed by the SMS code), in the portal or, for the beekeeper page, by the association with a signed document. A withdrawal takes effect at once: in the portal, or recorded by the Compliance Officer when it arrives by letter or phone.

**Requests about personal data.** Access, correction, deletion and objection requests are answered by the organization's Compliance Officer within 30 days (see [Data-subject requests](#co-requests)).

## What the public can read {#trust-public}

The public pages (Honey Story, beekeeper page, report verification) show only approved, verified information:

- **Place**: the **province** only (the beekeeper page also names the district). The province is calculated on the server; apiary coordinates never leave the server. The map colours provinces, never points; it currently covers Türkiye, and maps of the other partner countries are planned.
- **People**: the Honey Story names a beekeeper **only with an active name consent**; otherwise it shows the neutral code of the beekeeper's page. Beekeepers registered before the consent procedure are shown by their code until they give the consent (or, if the platform is configured so, keep their name until they are registered again). Never a phone number, birth date or photo of a person, and no user names of the platform.
- **Data**: laboratory and control point data; a within/out-of-limit verdict only for **verified limits**. Free-text answers and files are not published unless an administrator has explicitly allowed it for a CCP whose answers contain no personal data.
- **Reports**: only **approved** reports can be verified; the page shows the report code, laboratory, accreditation, date and fingerprint, never the lot or the beekeeper.
- **Prices**: never.
- **Photos**: only photos of the apiaries that supplied the batch, served without their metadata (no GPS position or camera data).

A Honey Story exists only for finalized batches; a beekeeper page only while the consent is active.

<!-- SHOT: public-map -->
Figure: Origins by province on the public Honey Story.

## How long data is kept {#trust-retention}

The retention periods are listed in [Table](#tbl-retention): for example one-time codes 30 days, SMS messages 1 year, prices 5 years, lots and their records 5 years after the best-before date of the batch, the audit log 10 years. The retention run is **off**: the platform administrators see a dry run that only counts, and the first real run needs the owner's approval and the decision on the format of the pseudonyms (see [Data retention](#admin-retention)).
