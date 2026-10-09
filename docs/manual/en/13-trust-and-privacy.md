# 13. Trust and privacy {#trust}

## The audit log {#trust-audit}

Every change in the platform (every save, approval, finalizing, shipment, sign-in, permission change) is written to the **Audit log**. Platform administrators open it under **User Management** → **Audit log**.

Each row shows:

- **When**: local time; hover to see UTC;
- **Who**: the user and the organization;
- **Action**: for example Create, Update, Approve, Finalize, Verify, Login, with flags such as "SelfApproved";
- **Record**: which record, with a link where an administration page exists;
- **Changes**: each changed field as **old → new**, with the field name in your language; user and organization ids are shown by name;
- **Hash**: the beginning of the row's fingerprint.

**Show raw** opens the stored data exactly as written (old and new values).

Filter by record type, action, user name or flag (for example `SelfApproved`) and click **Filter**.

<!-- SHOT: audit-log -->

### Append-only and the hash chain {#trust-hash-chain}

The audit log can only be added to. The database rejects any attempt to change or delete a row. Each row contains a SHA-256 fingerprint of its own content and of the previous row, which forms a chain.

**Verify hash chain** recalculates the whole chain. "Hash chain: All … rows verify." means nothing has been changed. Otherwise the message names the first row that was changed or that does not point to the previous row.

## Records that do not change {#trust-locked}

- An approved CCP record keeps its approval only while its values are unchanged.
- A finalized batch and its lots are locked.
- Approved lab results become read-only CCP 1 values.
- Corrections need two administrators and are logged with the reason.

## Files and fingerprints {#trust-files}

Delivery forms, lab report PDFs, organization documents, registry proofs and consent documents are stored outside the public web folder with a random name and their **SHA-256 fingerprint**. Uploads are checked by their content, not only by the file name: PDF up to 10 MB, or JPG, PNG or WEBP up to 8 MB. Before an approved lab report is used, the platform checks that the stored PDF still matches its fingerprint. Anyone can compare a report PDF with its fingerprint on the public verification page, without uploading it.

## What the public sees {#trust-public}

The public pages (Honey Story, beekeeper page, report verification) show only approved, verified information:

- **Place**: the **province** only (the beekeeper page also names the district). The province is calculated on the server; apiary coordinates never leave the server. The map of Türkiye colours provinces, never points.
- **People**: a beekeeper's name only with written consent (otherwise a neutral code); never a phone number, birth date or photo of a person. No user names of the platform.
- **Values**: laboratory and control point values; a within/out-of-limit verdict only for **verified limits**. Free-text answers and files are not published unless an administrator has explicitly allowed it for a CCP whose answers contain no personal data.
- **Reports**: the report code, laboratory, accreditation, date and fingerprint, never the lot or the beekeeper.

A Honey Story exists only for finalized batches; a beekeeper page only while the consent is active.

<!-- SHOT: public-map -->
