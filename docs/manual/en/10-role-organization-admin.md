# 10. Organization admin {#role-org-admin}

## Your daily work {#orgadmin-daily}

The organization admin is the manager of an organization in the platform. You:

- keep the organization's documents up to date (licence, accreditation, tax certificate, registry proof);
- at a plant: read deliveries and samples, request finalizing, ship and confirm shipments;
- at a wholesaler or retailer: confirm shipments;
- at an association: record and withdraw the beekeepers' consent for a public beekeeper page.

Memberships and capabilities of your organization are managed by the platform administrators. If a colleague needs access or a different role, ask them.

## Your organization page {#orgadmin-org-page}

1. Open **My organization** and click **Organization page**.
2. The page shows the status, capabilities, documents, verification history and members of your organization.

<!-- SHOT: org-details -->
Figure: Organization documents with their SHA-256 fingerprints.

### Upload a document {#orgadmin-documents}

1. Under the documents, choose the type: **Licence**, **Accreditation**, **Tax certificate**, **Registry proof** or **Other**.
2. Choose the file: PDF up to 10 MB, or JPG, PNG or WEBP up to 8 MB.
3. Click **Upload**: "Document uploaded."

The platform stores the file with its SHA-256 fingerprint. Documents cannot be deleted. A new organization needs at least one document before a platform administrator can verify it.

## Finalizing and shipments {#orgadmin-plant}

At a plant you can **Request finalizing** of a batch and **Ship** it, exactly like Staff: see [Request finalizing](#staff-request-finalize) and [Ship a batch](#staff-ship). At a receiver you can **Confirm receipt** or **Reject**: see [Confirm or reject a shipment](#staff-receive).

## Beekeeper pages (associations) {#orgadmin-storefronts}

An association can publish a public **Beekeeper page** for each member beekeeper who agrees in writing. The page shows only verified data: the beekeeper's name (only with consent; otherwise a neutral code such as `BK-…`), province and district, honey types, verified lots and approved lab results, and the batches the honey went into. It never shows a phone number, a photo of the person, the birth date or the exact location of an apiary. Visitors contact the beekeeper through the association.

The page is public only when **all** of these hold:

- the beekeeper is active;
- the beekeeper's enterprise registry number is verified by a platform administrator;
- your association is verified;
- the consent was recorded by your association (the beekeeper's current association) and not withdrawn.

### Record a consent {#orgadmin-consent}

1. Read the consent text to the beekeeper and have the form signed.
2. Open **Beekeeper pages** and click **Record consent** next to the beekeeper.
3. Tick **The consent text was read to the beekeeper and signed.**
4. Tick **The beekeeper agrees to show the name (otherwise a neutral code is shown)** only if the beekeeper agreed.
5. Enter the **Date of consent** (not in the future) and attach the **Signed consent document** (photo or PDF).
6. Save.

<!-- SHOT: consent-form -->
Figure: Recording a beekeeper's consent on a phone.

The message is "Consent recorded; the storefront is published." or, if the registry number is not verified yet, "Consent recorded. The storefront is published once the registry number is verified."

**Printable card.** **Printable card** prints a card with the QR code of the beekeeper page, for the beekeeper's stand.

### Withdraw a consent {#orgadmin-withdraw}

1. On **Beekeeper pages**, enter an optional **reason (optional)** and click **Withdraw consent**.
2. The page is removed at once: "Consent withdrawn; the storefront is no longer public."

Errors: "This beekeeper already has an active consent; withdraw it first to record a new one.", "Confirm that the consent text was read to the beekeeper.", "Attach the signed consent document."
