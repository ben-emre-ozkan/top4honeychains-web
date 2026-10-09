# 4. The end-to-end flow {#flow}

This chapter follows one batch of honey through the platform: who does each step and what must be finished before the next step can start. The role chapters (5–11) explain each screen in detail.

<!-- SHOT: flow-overview -->

## The control points (CCPs) {#ccp-list}

A **critical control point (CCP)** is a step where something is measured or checked. Each CCP has parameters (for example temperature or moisture), limits, and a status. CCP 0–3 belong to the **raw honey lot**, CCP 4–10 to the **batch**.

| CCP | Belongs to | What is checked | Entered by (default) | Approved by (default) |
|---|---|---|---|---|
| CCP 0 | Lot | Primary audit at reception, **before** the lab test | Sample acceptance officer of the plant | Quality approver of the plant |
| CCP 1 | Lot | Sampling and laboratory results | Written from the approved lab report | Lab manager (or the plant's Quality approver, see below) |
| CCP 2 | Lot | Temperature monitoring (heating) | Staff of the plant | Quality approver |
| CCP 3 | Lot | Filter condition | Staff | Quality approver |
| CCP 4 | Batch | Homogeneity (mixing) | Staff of the batch owner | Quality approver |
| CCP 5 | Batch | Time and temperature (pasteurization) | Staff | Quality approver |
| CCP 6 | Batch | Foam and sediment removal | Staff | Quality approver |
| CCP 7 | Batch | Quantity (filling) | Staff (Packing) | Quality approver |
| CCP 8 | Batch | Label accuracy | Staff (Packing) | Quality approver |
| CCP 9 | Batch | Packaging integrity | Staff (Packing) | Quality approver |
| CCP 10 | Batch | Temperature and humidity in storage | Staff (Wholesale, Retail or Packing) | Quality approver |

CCP 10 appears twice in the life of a batch: first **on the batch, at the plant** (it must be approved before the batch can be sealed), and later as a **storage record at each receiver** after a shipment.

A CCP record moves through these statuses: **Assigned** (no values yet) → **In Progress** (some values) → **Submitted** (all required values entered; waiting for approval) → **Completed** with an approval. A CCP that does not apply can be marked **Not applicable** with a reason; it must still be approved. Changing a value removes an existing approval.

## Step by step {#flow-steps}

| # | Step | Who | Where | Blocks the next step until… |
|---|---|---|---|---|
| 1 | Beekeeper and apiary are registered | Platform administrator | **Beekeepers**, **Apiaries** | The beekeeper is active and has an apiary |
| 2 | Registry number verified, association linked | Platform administrator | **Beekeeper registry and associations** | (Needed for the public beekeeper page, not for deliveries) |
| 3 | Delivery recorded: a raw honey lot is created | Sample acceptance officer (plant) | **Deliveries (intake)** → **Record a delivery** | – (CCP 0–3 are assigned to the lot automatically) |
| 4 | Sample sealed and sent to a laboratory | Sample acceptance officer | **Samples and lab** → **Take a sample** | The laboratory receives it |
| 5 | CCP 0 entered | Sample acceptance officer | **My work** → CCP 0 | It is submitted |
| 6 | CCP 0 approved | Quality approver (another person) | **My work** → CCP 0 | **Lab results can be entered only after CCP 0 is approved** |
| 7 | Sample received, seal checked | Analyst or Lab manager | **My work** → **Samples to receive** | A broken seal rejects the sample; take a new one |
| 8 | Lab results entered, PDF uploaded, report submitted | Analyst | **My work** → **Lab results to enter** | All required values, the PDF, the report number and date are present |
| 9 | Lab report approved (or returned) | Lab manager (another person) | **My work** → **Lab reports to approve** | Approval writes the results into the lot's CCP 1 |
| 10 | CCP 2–3 entered and approved | Staff, then Quality approver | **My work** | A lot can go into a batch only when **all** its CCPs are completed or not applicable, **and approved** |
| 11 | Batch created, lots added, owner set, CCP 4–10 assigned | **Platform administrator** (for now) | **Final Honey Productions** | The batch has lots and its CCPs |
| 12 | CCP 4–10 entered and approved, including CCP 10 storage at the plant | Staff, then Quality approver | **My work** | Every batch CCP must be approved |
| 13 | Finalizing requested | Staff or Organisation admin of the owner | **My organization** → batch → **Request finalizing** | The button is disabled while anything is missing |
| 14 | Finalizing confirmed: the batch is sealed | Quality approver (another person) | **My work** → **Finalize requests** | – The Honey Story and QR code go live; the batch and its lots are locked |
| 15 | Shipment created | Staff or Organisation admin of the holder | **Shipments and stock** → **Ship** | The receiver confirms |
| 16 | Receipt confirmed (or rejected) | Staff or Organisation admin of the receiver | **My work** → **Shipments to confirm** | Custody moves only on confirmation |
| 17 | Storage record (CCP 10) at the receiver, approved | Receiver's Staff, then its Quality approver | **Shipments and stock** → **Storage** | – |
| 18 | Receiver reads the producer's records | Receiver (any role with CCP read right) | CCP pages, read-only | – |
| 19 | Consumer scans the QR code | Anyone | Honey Story | – |

### Why CCP 0 comes before the lab test {#why-ccp0-first}

CCP 0 is the primary audit at reception: is this honey acceptable at all? The laboratory therefore cannot save, submit or approve results until CCP 0 of the lot is completed **and approved**. Until then the lab task shows "CCP 0 (primary audit) of this lot is not completed yet; lab results are entered and approved only after it." A report can still be returned while waiting.

### Why batch CCP 10 is needed before sealing {#why-ccp10}

Finalizing seals the batch only when every CCP assigned to it is approved. CCP 10 on the batch records the storage conditions at the plant itself, so the plant's own storage record must be in place before the seal. Each later holder keeps its own CCP 10 storage record for its stock.

### Batch creation is admin-only for now {#batch-admin-only}

Creating a batch, choosing its lots and amounts, setting the owning organization and assigning the batch CCPs are done by a platform administrator. Plant staff then enter the batch CCPs. Whether plant staff may create batches themselves will be decided by the project.

## What blocks what: a summary {#flow-blocks}

- **No delivery without** a verified Processing organization and a role with the intake right.
- **No lab results without** an intact-seal received sample and an approved CCP 0.
- **No report approval by** the person who entered the results.
- **No lot in a batch** until all its CCPs are completed or not applicable and approved, and the lot belongs to the batch's wholesaler.
- **No finalizing** while the batch has no honey, a batch CCP is missing or unapproved, or a lot CCP is open; the list "Missing before finalizing:" names each problem.
- **No confirmation of finalizing** by the person who requested it.
- **No shipment** of a batch that is not finalized, and no more than the quantity you hold.
- **No change** to a finalized batch or its lots, except through a correction by two administrators.
