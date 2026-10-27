# 6. Staff {#role-staff}

## Your daily work {#staff-daily}

**At a processing or packing plant** you enter the data of the production control points: CCP 2–3 on the raw honey lots and CCP 4–10 on the batches. When a batch is complete you request finalizing, and after it is sealed you ship it.

**At a wholesaler or retailer** you confirm or reject incoming shipments and record the storage conditions (CCP 10) of the batches you hold.

Your tasks appear in **My work** under **Values to enter** and **Shipments to confirm**. The menu shows **My organization** and **Shipments and stock**.

## My organization {#staff-my-organization}

**My organization** lists your organization's raw honey lots and batches. Each lot and batch shows its CCPs as small chips. The colour tells the state:

- grey: open (no values or not complete);
- yellow: submitted, waiting for approval;
- outlined green: completed but not approved yet;
- green with ✓: approved;
- outlined dark: not applicable.

Click a chip to open the CCP page. The **Process flow** link shows the timeline of a lot or batch.

<!-- SHOT: my-organization -->
Figure: **My organization**: lots and batches with their CCP chips.

## Enter lot CCP 2–3 {#staff-lot-ccps}

1. In **My work** open "CCP 2 · #[lot]" (or click the chip on **My organization**).
2. Enter the values as described in [Working on a CCP page](#ccp-page) and click **Save Monitoring Logs**.
3. If the step does not apply to this honey, mark it **Not applicable** with a reason.
4. Repeat for CCP 3.

The Quality Assurance Officer then approves them. A lot can be put into a batch only when all its CCPs are completed or not applicable and approved.

## Enter batch CCP 4–10 {#staff-batch-ccps}

A platform administrator creates the batch, adds the lots and assigns CCP 4–10. The batch then appears on **My organization** and its CCPs in your **My work**.

1. Open each batch CCP (CCP 4 to CCP 10) and enter its values.
2. CCP 10 on the batch is the **storage at your plant**. It must be entered and approved before the batch can be sealed.
3. Click **Save Monitoring Logs** each time.

<!-- SHOT: ccp-page-batch -->
Figure: A batch CCP page: source composition and the CCP 10 parameters.

## Request finalizing {#staff-request-finalize}

1. On **My organization** open the batch.
2. The **Finalize** card lists everything still missing under "Missing before finalizing:", for example "Batch: CCP 10 (Temperature and Humidity Monitoring) is Submitted, not approved yet". While anything is missing, **Request finalizing** is disabled.
3. When the list is empty, click **Request finalizing**.

<!-- SHOT: finalize-request -->
Figure: The **Finalize** card listing what is still missing.

The message "Finalizing requested; a Quality Assurance Officer (another person) must confirm it." confirms the request. A Quality Assurance Officer of your organization finds it under **Finalize requests**. You cannot confirm your own request.

## Ship a batch {#staff-ship}

Only a finalized batch can be shipped, and only by an organization that processes, packs or trades honey (capability **Processing**, **Packing**, **Wholesale** or **Retail**).

1. Open **Shipments and stock**. **Batches you hold** shows, per batch, the quantity held, reserved by pending shipments and available.
2. Click **Ship** next to the batch.
3. Choose the receiving organization in **To** (any other verified organization), enter the **Amount (kg)** and an optional **Note**.
4. Confirm. The quantity is reserved at once: "Shipment created; custody moves when the receiver confirms it."

<!-- SHOT: shipment-create -->
Figure: Shipping a batch to another organization.

Errors: "Only a finalized batch can be shipped.", "You can ship at most … kg of this batch.", "Choose another verified organization as receiver.", "Only an organization that processes, packs or trades honey ships or receives it."

## Confirm or reject a shipment (receiver) {#staff-receive}

1. In **My work** open **Shipments to confirm**, or go to **Shipments and stock** → **Incoming shipments**.
2. Check the goods. Click **Confirm receipt**: "Receipt confirmed; you now hold this quantity."
3. Or write a **reason (to reject)** and click **Reject**: "Shipment rejected; the quantity stays with the sender." A reason is required.

On a phone, every incoming shipment is shown as a card with the reason field and the two buttons below it.

<!-- SHOT: shipment-confirm -->
Figure: An incoming shipment on a phone: **Confirm receipt** or **Reject**.

## Record storage (CCP 10 at the receiver) {#staff-storage}

After you have received a batch:

1. In **Shipments and stock**, in the **Storage** column of the batch, click **CCP 10** (page "CCP 10 – Storage").
2. Enter all required storage values in one go (for example temperature and humidity).
3. Click **Save**: "Storage values saved." The record is **Submitted** and your Quality Assurance Officer finds it under **Values to approve**.

<!-- SHOT: storage-record -->
Figure: Recording CCP 10 storage at the receiver.

## Read the producer's records {#staff-read-only}

When your organization holds or has received a batch, you can open the producer's control points of that batch and of its lots **read-only**: you can read the data and who approved it, but no save or approve buttons. The beekeeper and apiary names are shown as "not shown", as in the recall report. The page shows **Read only** with "This control point is the responsibility of another organization; you can read it only." For a batch your organization never received, the platform shows "You can't open this page".

To open them, go to **Shipments and stock**, click **Process flow** next to the batch you hold, and click a stage of the flow.

## Recall report {#staff-recall}

From a batch or a lot you can open the **Recall report**. It lists the lots upstream and the shipments downstream that your organization may read, with **Print** and **CSV** buttons. Under **Your current holding** it shows the quantity your organization holds; other organizations may hold parts of the batch too, and only platform administrators see every holder ("Only your organization's holding is shown; other organizations may hold parts of this batch."). See [Recall](#admin-recall).
