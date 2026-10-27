# Planned application changes assumed by the user manual

The user manual of October 2026 (decision D-76, review by the project coordinator) is written as if the changes below have shipped. Items marked **Done** are built (D-84, release with D-78; not yet on the live site, which goes live with D-78 as a whole); A7 was done with D-83; A8 is still open. The manual of the D-78 release (D-85) describes the shipped labels; [Table "Labels renamed in this release"](en/17-glossary-and-faq.md#glossary) lists old → new names for readers.

Each item names where the change belongs in the code, so it can be planned as its own work item (one branch per item, per `CLAUDE.md`). Reference-text changes go into a new numbered migration in `db/migrations/` (idempotent, five languages), like `017_capability_texts.sql`.

## A. Changes the manual assumes (review D-76)

| # | Change | Where | Size |
|---|---|---|---|
| A1 | **Done (D-84, 2026-10-28).** Rename the role **Quality approver** to **Quality Assurance Officer** in all five languages: TR "Kalite Güvence Sorumlusu", ES "Responsable de aseguramiento de la calidad", PL "Specjalista ds. zapewnienia jakości", LV "Kvalitātes nodrošināšanas speciālists". The role code `QualityApprover` stays. | Role names in the reference translations (`ReferenceTranslations`, entity Role) and the resource files; new migration for the reference texts | small |
| A2 | **Done (D-84, 2026-10-28).** Messages that name a role by its code show the role name instead: "a QualityApprover (another person) must confirm it", "a LabManager (another person) must approve it", "the LabManager approves it or returns it". | Finalize request message (OrgWorkspace / finalize service), lab report submit messages (`LabReportsController`), resource files | small |
| A3 | **Done (D-84, 2026-10-28).** English spelling **organization** everywhere on screen: "Organisation admin" → "Organization admin", "Verified organisations", "Only verified organisations take part" and similar. | `SharedResource.en.resx`, role names in the reference translations | small |
| A4 | **Done (D-84, 2026-10-28).** "Fixed rules (cannot be switched off):" on **Roles and permissions** becomes "Ground rules (cannot be switched off):" in five languages. | `Views/Permissions/Index.cshtml` resource key, five `.resx` files | small |
| A5 | **Done (D-84, 2026-10-28).** Capability descriptions written as abilities ("Able to …") in five languages, as listed in the manual ([Table "Capabilities"](en/02-capabilities-organizations-and-roles.md#capabilities)). | `Capabilities.Description` and `ReferenceTranslations` (Field = Description); new migration after 017 | small |
| A6 | **Done (D-84, 2026-10-28).** The special capabilities **CentralAssociation**, **PublicAuthority** and **Beekeeping** show their codes on screen; give them names in five languages ("Central association", "Public authority", "Beekeeping enterprise"). | Capability names in `ReferenceTranslations` (and the English base text) | small |
| A7 | **Done (D-83, 2026-10-27).** Funder wording: the application itself does not name TÜBİTAK anywhere (checked: views, resources, scripts). If the project wants the ICT-AGRI-FOOD 2022 acknowledgement and the funding agencies' logos on the landing page or in the footer, use the programme logo in `docs/brand/programme/`, the consortium logos in `docs/brand/partners/` and, once they arrive, the funding agencies' logos in `docs/brand/funders/`. | Landing page, footer | small, optional |
| A8 | **Origin map for other partner countries.** The Honey Story, the beekeeper page and the admin dashboard draw origins on a province map of Türkiye only. Supporting other partner countries needs: a country on apiaries/lots (the lot already has a country of origin), region data and map shapes per country, the province calculation per country, and the public map component per country. | Honey Story, beekeeper page, dashboard, `tmap` component, map data | **large**, separate project item |

## B. Open items from the screenshot pass (pass 2)

| # | Observation | Where | Size |
|---|---|---|---|
| B1 | **Done (D-84, 2026-10-28).** Capability codes shown raw (see A6). | reference data | small |
| B2 | **Done (D-84, 2026-10-28).** The Latvian **Roles and permissions** matrix labels its rows "CCP 0 …" instead of "KKP 0 …" (the language's abbreviation is not applied on that page; check the other languages too). | `Views/Permissions/Index.cshtml` (use the CCP code helper used elsewhere) | small |
| B3 | **Done (D-84, 2026-10-28).** The landing page says "CCP 2–9" for step 4, but the lot carries CCP 2–3 and the batch CCP 4–10. | landing page texts in five languages | small |
| B4 | **Done (D-84, 2026-10-28).** On **Manage Batch**, the "Manage batch CCPs" button is almost invisible (very low contrast). | `Views/FinalHoneyProductions/ManageBatch.cshtml` / CSS | small |
| B5 | **Done (D-84, 2026-10-28).** The public **Verify a lab report** page is not branded (plain Bootstrap: purple button, default font). | `Views/Verify/Index.cshtml` | small |
| B6 | **Done (D-84, 2026-10-28).** On a phone, the incoming-shipments table on **Shipments and stock** scrolls sideways; **Confirm receipt** and **Reject** are off-screen and the reason field is a few pixels wide. | `Views/Shipments/Index.cshtml` (card layout on narrow screens) | small–medium |
| B7 | **Done (D-84, 2026-10-28).** The lab report's "Required values entered: … of …" counts only saved data; it does not update while the analyst types. | `Views/LabReports/Edit.cshtml` (live count in the page script) | small |
| B8 | **Done (D-84, 2026-10-28).** Demo data: the [DEMO] Kekik Balı 2026 batch shows "Origin: Argentina 100%" and its lot is "Citrus Honey" from the Kekik (thyme) apiary in Giresun. Make the demo records consistent before the next demonstration. | local demo data only (not the live site) | small |
| B9 | **Done (D-84, 2026-10-28).** The recall report of a member organization lists only its own holding under "Current holders" (by design); a label such as "Your current holding" would avoid the impression that nobody else holds the batch. | `Views/Recall/Report.cshtml`, resources | small, optional |

## C. Points to confirm, found while deriving the data rights from the code

These are not assumed by the manual; they are decisions for the project. The manual describes the intended behaviour.

| # | Finding | Where |
|---|---|---|
| C1 | **Done (D-84, 2026-10-28).** The read rights **Shipments → Read** and **Beekeeper pages → Read** only hide the menu entry; the pages themselves do not check them, so any role of the organization can open them by address (an association's list shows its member beekeepers' names to every role). | `ShipmentsController.Index`, `StorefrontsController.Index` |
| C2 | **Done (D-84, 2026-10-28).** Signed delivery forms, organization documents and consent documents can be downloaded by any member of the organization, without a role check. | `IntakeController.Form`, organization and consent document downloads |
| C3 | **Done (D-84, 2026-10-28).** Taking a sample, shipping and finalizing check status, role and scope, but not the organization's capability. | `SamplesController`, `ShipmentsController`, `OrgWorkspaceController` |
| C4 | **Done (D-84, 2026-10-28).** The public **Verify a lab report** page also answers for reports that are submitted but not yet approved (only drafts are hidden). | `VerifyController` |
| C5 | **Done (D-84, 2026-10-28).** The public Honey Story names the beekeepers of the batch without checking a consent (decision D-12 says the name is shown; confirm that this is still wanted). | `HoneyStoryController` |
| C6 | **Done (D-84, 2026-10-28).** With the central-association setting, platform administrators can still verify associations as well (not exclusive). | `OrganizationAuthorizationHandler` |
