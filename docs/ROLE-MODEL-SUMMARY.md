# TOP4HoneyChains – role and organization model (summary)

*Meeting note, 3 October 2026. Detailed design: `ROLE-MODEL.md`.*

## Goal
Only **licensed or platform-verified** organizations enter data in the honey chain. Every record is stored with who entered it, on behalf of which organization and when. Important steps need the approval of a second person. Consumers see only verified information.

## Who does what
| Organization type | Task | Critical control points (CCP) |
|---|---|---|
| Beekeeper | Does not sign in and enters no data. The association keeps the beekeeper's record. | — |
| Beekeepers' association | Keeps the records of member beekeepers and does the field inspection. | CCP 0 (field inspection) |
| Processing organization (association or packer) | Receives the honey, takes samples, processes it. | CCP 1–6 (acceptance, heating, filtering, blending, pasteurization, settling) |
| Laboratory (accredited or in-house) | Runs the analyses; the lab manager approves the results. | CCP 1 lab results |
| Packer | Filling, labelling and packaging. | CCP 7–9 |
| Wholesaler / retailer | Receives and stores the product. | CCP 10 (storage) |
| Central association | May verify associations later. | — |
| Public authority | Can read every record and change nothing. | — |
| Platform management | Verifies organizations and configures the system. For now the project team. | — |

- An organization can have several roles. For example an association can pack its honey itself.
- Inside organizations there are personal roles: administrator, staff, field inspector, sample acceptance officer, quality approver, analyst and lab manager.
- **Rules:**
  - A CCP is entered by the organization that holds the product at that moment.
  - An organization later in the chain can read the records of the earlier ones.
  - Important steps need the approval of a second person: CCP values, lab results and finalizing a batch.
  - Platform management can grant an exemption to one-person organizations. The exemption does not apply to lab results, and every approval given under the exemption is recorded.
  - A finalized batch is never reopened; a correction is made only with a correction record that states the reason.
  - *Update 18 November 2026 (D-88):* the processing or packing organization creates its own batches from its approved lots, and its staff enter the control points of a lot or batch on one control sheet that the quality approver approves in one action. Platform management sets the platform up and oversees it; it takes no part in the honey's journey.

## What the beekeeper gains
- **Own storefront page.** Built only from verified data and published after the beekeeper's recorded consent (KVKK).
- **Location privacy.** Province and district are shown, the exact location is not.
- **Visibility.** Consumers also see which batches the beekeeper's honey went into and the analysis results.
- **Notification.** Optional SMS information at delivery.

## What can be configured (without programming)
- Which organization type and which role enters each CCP, and who approves it.
- Analysis limits, also per honey type. Starting values come from the Turkish Food Codex and are waiting to be verified.
- Whether associations are verified by platform management or by the central association.
- Four-eyes exemption for one-person organizations.
- Verification of the beekeeper registry: for now by document upload and manual approval. Automatic verification is possible once AKS access exists.

## Phased plan (status at the time of the meeting)
| Phase | Content | Status |
|---|---|---|
| P1 | Organizations, roles and memberships; organization verification; append-only audit log; access rules | Ready and tested. **Off** in the system: takes effect when switched on. |
| P2 | CCP responsibility matrix, approval flow, organization-specific screens | Planned |
| P3 | Intake, shipments between organizations, storage (CCP 10), recall report | Planned |
| P4 | Samples, seals, counter samples, lab report and lab badges | Planned |
| P5 | Beekeeper storefront, AKS link, origin percentages in blends (EU 2024/1438) | Planned |

## Open questions
1. Is the "wholesaler" the same as the organization that packs and sells the honey?
2. Who will run the platform after the project ends? Who will verify associations?
3. Who can see whose records? For example, can an association see where a member's honey was sold?
4. Is access to AKS possible? Is the enterprise number format correct?
5. What is the platform's role under KVKK? How long are records kept?
6. How will origin percentages be applied in Türkiye?
7. How often is the field inspection (CCP 0) done?
8. Analysis limits are to be verified by an accredited laboratory (`LIMITS-TO-VERIFY-TR.md`).
9. Will beekeepers get an SMS at delivery? If so, who pays for it?
