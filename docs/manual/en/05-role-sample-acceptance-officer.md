# 5. Sample acceptance officer {#role-sao}

## Your daily work {#sao-daily}

You work at the reception of a processing plant. When a beekeeper delivers honey you:

1. record the delivery (this creates the raw honey lot);
2. seal a sample and a counter-sample and send the sample to a laboratory;
3. enter CCP 0, the primary audit of the lot, so that a Quality Assurance Officer can approve it.

Your tasks appear in **My work** under **Values to enter**. The menu shows **Deliveries (intake)** and **Samples and lab**.

## Record a delivery {#sao-intake}

1. Open **Deliveries (intake)** and click **Record a delivery**.
2. In **Beekeeper business or registry number**, type the beekeeper's business number or verified enterprise registry number exactly, and click **Find**. If nothing is found: "No active beekeeper with this number." Check the number; beekeepers are registered by a platform administrator.
3. Fill in the form:
   - **Apiary**: one of the beekeeper's apiaries.
   - **Honey type**.
   - **Production period**: the current period is preselected.
   - **Country of origin**.
   - **Delivery date**: today by default; it cannot be in the future.
   - **Amount (kg)**: a comma or a dot works as decimal separator.
   - **Province** and **District** of origin: the province is pre-filled from the registry number when possible.
   - **Photo of the signed delivery form**: take a photo with the camera button or choose a file (PDF up to 10 MB, or JPG, PNG or WEBP up to 8 MB).
   - **Send an SMS to the beekeeper** (optional; in the current version the message is logged, not sent).
4. Click **Record delivery**.

<!-- SHOT: intake-form -->
Figure: Recording a delivery on a phone.

The message "Delivery recorded as lot #… ." confirms the new lot. The platform assigns CCP 0–3 to the lot and opens **Take a sample** for it.

**Common errors**

Table: Common errors when recording a delivery {#tbl-intake-errors}

| Message | What to do |
|---|---|
| "Choose one of the beekeeper's apiaries." | Select an apiary from the list. |
| "Enter the delivery date (not in the future)." | Correct the date. |
| "The amount must be greater than 0." | Enter the quantity in kilograms. |
| "Choose a district of the selected province." | Choose the province first, then a district from its list. |
| "Attach the photo of the signed delivery form." | The signed form is required. |
| "This apiary already has a production record for the selected period and honey type." | One apiary delivers once per period and honey type. Check the period and honey type. |
| "Only a Processing organization records deliveries." / "Your role (…) cannot record deliveries; roles: …" | Your organization or role cannot record deliveries. Ask your organization admin. |

The **Deliveries (intake)** list shows every delivery with its lot number, date, apiary, honey type, amount, origin and the signed form (with the first characters of its SHA-256 fingerprint). From each row you can open **Sample** and **Recall**.

## Take and send a sample {#sao-sample}

1. After recording the delivery the page **Take a sample** opens. You can also open it from **Samples and lab** or from the delivery row.
2. Choose the **Laboratory**: only verified laboratories are listed; an accredited laboratory shows its accreditation number.
3. Enter the **Seal number** of the sample and the **Counter-sample seal number**. They must be different.
4. Enter **Where the counter-sample is kept** and **Keep until** (12 months ahead by default).
5. Click **Send to the laboratory**, or **Later** to send it another time.

<!-- SHOT: sample-form -->
Figure: Taking a sample: laboratory and seal numbers.

The sample gets a code such as `S-20070-1` and the status **Sent**. The laboratory finds it under **Samples to receive**. If the laboratory finds the seal broken, the sample is rejected; take a new sample for the same lot.

Errors: "Choose a verified laboratory.", "The sample and the counter-sample need different seals.", "Enter where the counter-sample is kept (at most 200 characters).", "Enter until when the counter-sample is kept (a future date).", "Only the organization that holds the lot takes its samples."

## Working on a CCP page {#ccp-page}

All roles that enter or approve CCP data use the same page ([Figure](#fig-ccp-page-lot)). Open it from **My work**, from the CCP chips on **My organization**, or from the **Process flow** page of a lot or batch.

<!-- SHOT: ccp-page-lot -->
Figure: A lot CCP page: a **Yes** answer opens a required follow-up question.

The page shows:

- the lot (beekeeper, apiary, honey type, production date) or the batch (batch, total amount, wholesaler, source composition);
- the **Status** pill and who entered and approved the record;
- the honey class used for the limits ("Limits for honey class: …");
- the parameters, grouped, each with its input, its **Critical limit:** and, after saving, **Within limit** or **Out of limit**;
- a **Read only** banner with the reason when you may not enter data.

Input types:

- **Number**: comma or dot as decimal separator.
- **Yes / No**: choose from the list.
- **Text**: at most 100 characters.
- **File**: upload a document (PDF up to 10 MB, or an image up to 8 MB). Tick **Remove** to delete it.
- **Notes**: an optional observation, at most 500 characters.

Some parameters are required only when another question is answered in a certain way, for example "Required only when "…" is answered Yes."

**To enter data**

1. Fill in the parameters.
2. Click **Save Monitoring Logs**. The message "CCP … values saved." confirms the save and you return to **My organization**.

There is no separate submit button. When every required parameter has a value, the record becomes **Submitted** and appears in the approvers' **My work**. With only some values it stays **In Progress**.

**If the CCP does not apply** (for example the honey is not heated):

1. While the record has no values, open "This CCP does not apply here (mark as "Not applicable")".
2. Write the **Reason**, for example "Raw honey is not heated or pasteurized".
3. Click **Mark as "Not applicable"**. The reason, your user name and the time are recorded and shown as "Not applicable" on the public Honey Story.

To undo it, click **Remove "Not applicable"**. A not-applicable CCP still needs approval, by another person than the one who marked it.

**Common errors**

Table: Common errors on a CCP page {#tbl-ccp-errors}

| Message | Meaning |
|---|---|
| "…: enter a number." / "…: choose Yes or No." | A value has the wrong type. |
| "This CCP is marked "Not applicable". Remove that mark first to record values." | Remove the mark before entering values. |
| "This control point is the responsibility of another organization; you can read it only." | You can read this CCP but not change it. |
| "Your role (…) cannot enter this control point; entering roles: …" | Another role enters this CCP. |
| "Your organization is Pending; only verified organizations can enter data." | Wait for verification. |
| "This raw honey lot is used in the finalized batch … and is locked. …" | The lot is sealed; only a correction can change it. |

## Enter CCP 0 (primary audit) {#sao-ccp0}

1. In **My work**, under **Values to enter**, open the item "CCP 0 · #[lot]".
2. Enter the reception checks (the parameters of CCP 0). Some questions open further required questions when answered **Yes**.
3. Click **Save Monitoring Logs**.

When all required data is present, CCP 0 is **Submitted** ([Figure](#fig-ccp0-submitted)) and the Quality Assurance Officer of your organization finds it under **Values to approve**. You cannot approve your own CCP 0 ("Four-eyes: you recorded values of this CCP, so another person must approve it."). The laboratory can enter results only after CCP 0 is approved.

<!-- SHOT: ccp0-submitted -->
Figure: CCP 0 submitted and waiting for approval by another person.
