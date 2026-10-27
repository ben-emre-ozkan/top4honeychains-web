# 4. The end-to-end flow {#flow}

This chapter follows one batch of honey through the platform: who does each step and what must be finished before the next step can start ([Figure](#fig-flow-overview)). The role chapters (5–14) explain each screen in detail.

<!-- SHOT: flow-overview -->
Figure: The **Process flow** of a finalized batch: every stage with its status and responsible organization.

## The control points (CCPs) {#ccp-list}

A **critical control point (CCP)** is a step where something is measured or checked. Each CCP has parameters (for example temperature or moisture), limits, and a status. CCP 0–3 belong to the **raw honey lot**, CCP 4–10 to the **batch** ([Table](#tbl-ccps)).

Table: The critical control points (CCPs) and their default roles {#tbl-ccps}

| CCP | Belongs to | What is checked | Entered by (default) | Approved by (default) |
|---|---|---|---|---|
| CCP 0 | Lot | Primary audit at reception, **before** the lab test | Sample acceptance officer of the plant | Quality Assurance Officer of the plant |
| CCP 1 | Lot | Sampling and laboratory results | Written from the approved lab report | Lab manager (or the plant's Quality Assurance Officer, see below) |
| CCP 2 | Lot | Temperature monitoring (heating) | Staff of the plant | Quality Assurance Officer |
| CCP 3 | Lot | Filter condition | Staff | Quality Assurance Officer |
| CCP 4 | Batch | Homogeneity (mixing) | Staff of the batch owner | Quality Assurance Officer |
| CCP 5 | Batch | Time and temperature (pasteurization) | Staff | Quality Assurance Officer |
| CCP 6 | Batch | Foam and sediment removal | Staff | Quality Assurance Officer |
| CCP 7 | Batch | Quantity (filling) | Staff (Packing) | Quality Assurance Officer |
| CCP 8 | Batch | Label accuracy | Staff (Packing) | Quality Assurance Officer |
| CCP 9 | Batch | Packaging integrity | Staff (Packing) | Quality Assurance Officer |
| CCP 10 | Batch | Temperature and humidity in storage | Staff (Wholesale, Retail or Packing) | Quality Assurance Officer |

CCP 10 appears twice in the life of a batch: first **on the batch, at the plant** (it must be approved before the batch can be sealed), and later as a **storage record at each receiver** after a shipment.

A CCP record moves through these statuses: **Assigned** (no data yet) → **In Progress** (some data) → **Submitted** (all required data entered; waiting for approval) → **Completed** with an approval. A CCP that does not apply can be marked **Not applicable** with a reason; it must still be approved. Changing any data removes an existing approval.

## Step by step {#flow-steps}

[Table](#tbl-flow-steps) lists every step, who does it and what it blocks.

Table: The end-to-end flow, step by step {#tbl-flow-steps}

| # | Step | Who | Where | Blocks the next step until… |
|---|---|---|---|---|
| 1 | Beekeeper and first apiary are registered: registry number, identity, notice and consents, confirmed by an SMS code (or a signed photo) | Beekeeper registration officer (district directorate) or platform administrator | **Beekeeper registration** | A beekeeper confirmed by a signed photo is found at intake only after the Compliance Officer accepts the photo |
| 2 | Registry number verified, association linked | Platform administrator | **Beekeeper registry and associations** | (Needed for the public beekeeper page, not for deliveries) |
| 3 | Delivery recorded: every container with its weight and seal, a sample jar and a counter-sample jar from every container; the beekeeper confirms with the SMS code; a raw honey lot is created | Sample acceptance officer (plant) | **Deliveries (intake)** → **Record a delivery** | – (CCP 0–3 are assigned to the lot automatically; the jars go to the chosen laboratory) |
| 4 | Labels printed and stuck on the containers and jars | Sample acceptance officer | Delivery page → **Labels** | – |
| 5 | CCP 0 entered | Sample acceptance officer | **My work** → CCP 0 | It is submitted |
| 6 | CCP 0 approved | Quality Assurance Officer (another person) | **My work** → CCP 0 | **Lab results can be entered only after CCP 0 is approved** |
| 7 | Every jar received: seal number, label and cap checked | Analyst or Lab manager | **My work** → **Samples to receive** | A failed check rejects the jar; the plant takes a new jar from the same container |
| 8 | Composite sample made from the received jars (composite analysis) | Analyst or Lab manager | **Samples and lab** → **Make the composite** | Every container's jar must be received in good order |
| 9 | Lab results entered, PDF uploaded, report submitted | Analyst | **My work** → **Lab results to enter** | All required values, the PDF, the report number and date are present |
| 10 | Lab report approved (or returned) | Lab manager (another person) | **My work** → **Lab reports to approve** | Approval writes the results into the lot's CCP 1 |
| 11 | CCP 2–3 entered and approved | Staff, then Quality Assurance Officer | **My work** | A lot can go into a batch only when **all** its CCPs are completed or not applicable, **and approved** |
| 12 | Batch created, lots added, owner set, CCP 4–10 assigned | **Platform administrator** (for now) | **Final Honey Productions** | The batch has lots and its CCPs |
| 13 | CCP 4–10 entered and approved, including CCP 10 storage at the plant | Staff, then Quality Assurance Officer | **My work** | Every batch CCP must be approved |
| 14 | Finalizing requested | Staff or Organization admin of the owner | **My organization** → batch → **Request finalizing** | The button is disabled while anything is missing (also a delivery photo that the Compliance Officer has not accepted) |
| 15 | Finalizing confirmed: the batch is sealed | Quality Assurance Officer (another person) | **My work** → **Finalize requests** | – The Honey Story and QR code go live; the batch and its lots are locked; the beekeeper's price is locked |
| 16 | Shipment created | Staff or Organization admin of the holder | **Shipments and stock** → **Ship** | The receiver confirms |
| 17 | Receipt confirmed (or rejected) | Staff or Organization admin of the receiver | **My work** → **Shipments to confirm** | Custody moves only on confirmation |
| 18 | Storage record (CCP 10) at the receiver, approved | Receiver's Staff, then its Quality Assurance Officer | **Shipments and stock** → **Storage** | – |
| 19 | Receiver reads the producer's records | Receiver (any role with CCP read right) | CCP pages, read-only | – |
| 20 | Consumer scans the QR code | Any public visitor (no account) | Honey Story | – |

Alongside the flow, the beekeeper follows their deliveries, samples and batches in the beekeeper portal and enters their asking price (see [chapter 14](#portal)), and the Compliance Officer of each organization reviews signed photos, rejected samples and broken seals (see [chapter 13](#role-co)).

### Registration with an SMS code or a signed photo {#flow-registration}

A beekeeper is registered once, by the beekeeper registration officer of the district directorate of agriculture (or by a platform administrator). The officer checks the enterprise registry number (`TR`, the province plate code and 10 digits) against the directorate's province, records the beekeeper and the first apiary, lets the beekeeper read the information notice and records the beekeeper's choices: the optional consent to show the name on public pages and the optional consent to SMS notifications. The beekeeper confirms all of it with a **6-digit code** that arrives by SMS and that they read out; the code is bound to exactly this content.

A beekeeper without a mobile phone signs the printed form instead; the officer uploads a photo of it with the reason. Such a registration waits for the directorate's **Compliance Officer**; until the photo is accepted, the beekeeper cannot deliver ("No active beekeeper with this number." at intake), and a rejected photo withdraws the consents. A beekeeper without a mobile phone gets no portal account. See [chapter 12](#role-ro).

### Containers, seals and sample jars {#flow-containers}

Honey arrives in **containers** (tins, drums, barrels). At reception the sample acceptance officer records one row per container:

1. **The seal on arrival**: a sealed container is checked (seal number and **seal intact**); an unsealed one gets a plant seal in front of the beekeeper (**Plant seal applied now**).
2. **The weight**: net kilograms (and optionally gross); the lot's amount is the sum of the containers.
3. **The sample jars**: the container is opened and a **sample jar** and a **counter-sample jar** are filled from it, each with its own seal number and a **tamper-evident cap**. The cap must be intact (the button down) when the jar is sealed; a popped cap means the jar was opened, so it is discarded and a new jar is filled.
4. **A new seal** closes the container after sampling.

Every step is a **seal event** that can only be added to, never changed. The laboratory receives the sample jars; the counter-samples stay at the plant for the time set under **Keep until**.

**The beekeeper's confirmation.** The beekeeper gets an SMS with the organization, the date, the containers, the kilograms, the honey type, the number of samples and a code. When the code is entered, the delivery is confirmed and its **content fingerprint** is stored. Changing anything after the code was sent voids it, and a new code is needed. If the beekeeper has left, the officer calls them and they read the code from their phone. Without an SMS (no phone, no signal) the officer uploads the **signed delivery form** with a reason: the lot can be worked on, but it can be finalized only after the plant's Compliance Officer accepts the photo.

<!-- SHOT: intake-containers -->
Figure: Recording a delivery: one row per container with its seals and sample jars.

**Labels.** The delivery page prints the labels with QR codes: one per container (`L{lot}-C{n}`), one per sample jar (`S-{lot}-C{n}`) and one per counter-sample (`…-K`), on an A4 sheet (2 × 5) or one 100 × 50 mm label per page.

### Reception at the laboratory {#flow-reception}

For every jar the laboratory records three checks: **The seal number matches the record**, **The label is intact** and **The cap is intact (button down)**. If any answer is **No**, the jar is rejected with a reason. The plant then takes a **new jar from the same container** (**Open, sample and seal again**), which breaks the container seal again and applies a new one; both are visible in the seal history. The confirmed delivery content keeps the first jar.

### Composite or per container {#flow-analysis}

The platform supports two ways of analysing a lot; the platform's setting decides which one is used:

- **Composite (default).** When every container's jar has been received in good order, the laboratory makes **one composite sample** from equal parts of the jars (**Make the composite**, code `S-{lot}-COMP`) and analyses it. Its approved report fills the lot's CCP 1. The individual jars stay retained; when the composite fails, or on request, a single jar is analysed on its own. Such a single-jar report stays with its jar and does not replace the lot's CCP 1 values.
- **Per container.** Every jar is analysed on its own, and every approved jar report counts for the lot.

### The worst-value rule {#flow-worst}

When the jars are analysed one by one, the lot's CCP 1 value of each parameter is the **worst** container value:

- a value outside its limits comes first, the one furthest outside;
- otherwise the value closest to the limit, in the direction that matters (the highest where there is a maximum, the lowest where there is a minimum);
- without a limit, the highest value;
- text answers are all kept, each with its container.

The lab report and the recall report show every container's values ("Lab values per container"), with the lot value in bold.

### Corrections of delivery facts {#flow-corrections}

Once a delivery is confirmed, its facts (date, kind and weights of the containers, seal numbers) are locked, also for administrators. A mistake is corrected by **two platform administrators**: one requests the correction with a reason, a second one confirms it (the requester never decides). The original and the corrected content stay readable on the delivery page, with their fingerprints, and the beekeeper gets an information SMS. Containers are never added or removed by a correction.

### Why CCP 0 comes before the lab test {#why-ccp0-first}

CCP 0 is the primary audit at reception: is this honey acceptable at all? The laboratory therefore cannot save, submit or approve results until CCP 0 of the lot is completed **and approved**. Until then the lab task shows "CCP 0 (primary audit) of this lot is not completed yet; lab results are entered and approved only after it." A report can still be returned while waiting.

### Why batch CCP 10 is needed before sealing {#why-ccp10}

Finalizing seals the batch only when every CCP assigned to it is approved. CCP 10 on the batch records the storage conditions at the plant itself, so the plant's own storage record must be in place before the seal. Each later holder keeps its own CCP 10 storage record for its stock.

### Batch creation is admin-only for now {#batch-admin-only}

Creating a batch, choosing its lots and amounts, setting the owning organization and assigning the batch CCPs are done by a platform administrator. Plant staff then enter the batch CCPs. Whether plant staff may create batches themselves will be decided by the project.

### Deliveries recorded before containers {#flow-legacy}

Deliveries recorded before the container procedure (or while a plant still works without it) are marked **legacy (containers not recorded)**: they have one signed delivery form and a single sample with one seal check. They stay valid and are finalized as before.

## What blocks what: a summary {#flow-blocks}

- **No delivery without** a verified Processing organization, a role with the intake right and a registered beekeeper (a registration by photo only after the Compliance Officer accepted it).
- **No confirmed delivery without** the beekeeper's SMS code for exactly this content, or a signed photo with a reason.
- **No lab results without** a received jar whose seal, label and cap checks passed, and an approved CCP 0.
- **No report approval by** the person who entered the results.
- **No lot in a batch** until all its CCPs are completed or not applicable and approved, and the lot belongs to the batch's wholesaler.
- **No finalizing** while the batch has no honey, a batch CCP is missing or unapproved, a lot CCP is open, or a delivery photo waits for the Compliance Officer; the list "Missing before finalizing:" names each problem.
- **No confirmation of finalizing** by the person who requested it.
- **No shipment** of a batch that is not finalized, and no more than the quantity you hold.
- **No change** to a finalized batch or its lots, or to the facts of a confirmed delivery, except through a correction by two administrators.
