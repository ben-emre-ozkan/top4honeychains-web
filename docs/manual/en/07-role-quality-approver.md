# 7. Quality approver {#role-qa}

## Your daily work {#qa-daily}

You are the second pair of eyes of your organization. You:

- approve CCP records that colleagues have submitted (CCP 0, CCP 2–10 and storage records);
- confirm finalizing requests, which seals a batch;
- at a wholesaler or shop, approve the CCP 10 storage records.

Your tasks appear in **My work** under **Values to approve** and **Finalize requests**. You never approve a record in which you recorded values yourself.

## Approve a CCP record {#qa-approve}

1. In **My work** open an item under **Values to approve**, for example "CCP 0 · #20070".
2. Check the values. Each value shows **Within limit** or **Out of limit** against its limit.
3. In the card **Approval (four-eyes)**, read the note: approving confirms that the values are recorded correctly, not that they are within their limits.
4. Click **Approve**.

<!-- SHOT: ccp-approve -->

The message "CCP record approved." confirms it; the status pill shows **Approved** with your name and the time.

**Values outside their limits.** If any value is out of limit, a dialog **Values outside their limits** lists them, each with its **Limit:** (and "limit not verified yet" for limits that are still to be verified). Click **Approve anyway** to approve, or **Cancel**. The values stay flagged as out of limit; approval only confirms that they were recorded correctly.

<!-- SHOT: out-of-limit-dialog -->

**Not applicable records.** A CCP marked "Not applicable" must be approved as well. Read the reason before you approve.

**Common errors**

| Message | Meaning |
|---|---|
| "Four-eyes: you recorded values of this CCP, so another person must approve it." | You entered values in this record. Another approver must approve it. |
| "Only a submitted (or "Not applicable") CCP record can be approved." | Required values are still missing. |
| "This CCP record is already approved." | Nothing to do. |
| "Your role (…) cannot approve this control point; approving roles: …" | Another role approves this CCP. |

If your organization has the **Four-eyes exemption**, you may approve your own records; the message then says "Approved under the four-eyes exemption (flagged in the audit log)." The exemption never applies to lab results.

## Confirm finalizing (seal a batch) {#qa-finalize}

1. In **My work** open the item under **Finalize requests**, or open the batch on **My organization**.
2. Check the batch: the **Control points of the batch** and the **Source lots** are all approved, and "Missing before finalizing:" is empty.
3. Click **Confirm and finalize**.

<!-- SHOT: finalize-confirm -->

The batch is sealed: "Batch finalized." A seal appears once with the links **Open the Honey Story** and **Download QR**. From now on:

- the batch and all its lots are locked;
- the public Honey Story is live at its QR code;
- the batch can be shipped.

There is no "un-finalize". Mistakes found later are corrected through the administrators' correction process.

Errors: "Four-eyes: the person who requested finalizing cannot confirm it.", "Finalizing must be requested first.", "The batch cannot be finalized yet: …".

<!-- SHOT: batch-sealed -->

## Approve storage records (wholesaler or shop) {#qa-storage}

1. In **My work** open the item marked **storage** ("CCP 10 · [batch]"), or go to **Shipments and stock** → **Storage**.
2. Check the values and click **Approve** (confirm out-of-limit values in the dialog if asked).

The same four-eyes rule applies: the colleague who recorded the values cannot approve them.
