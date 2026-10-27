# 5. Sample acceptance officer {#role-sao}

## Your daily work {#sao-daily}

You work at the reception of a processing plant. When a beekeeper delivers honey you:

1. record the delivery container by container, with the seals, and fill a sample jar and a counter-sample jar from every container;
2. let the beekeeper confirm the delivery with the code they receive by SMS;
3. print the labels and send the sample jars to the laboratory;
4. enter CCP 0, the primary audit of the lot, so that a Quality Assurance Officer can approve it.

Your tasks appear in **My work** under **Values to enter**. The menu shows **Deliveries (intake)** and **Samples and lab**.

## Record a delivery {#sao-intake}

"Record each container of the delivery with its seal, take a sample from every container, then the beekeeper confirms everything with an SMS code."

1. Open **Deliveries (intake)** and click **Record a delivery**.
2. In **Beekeeper business or registry number**, type the beekeeper's business number or verified enterprise registry number exactly, and click **Find**. If nothing is found: "No active beekeeper with this number." Check the number. Beekeepers are registered by the beekeeper registration officer; a beekeeper registered with a signed photo can deliver only after the Compliance Officer has accepted the photo. "no mobile phone: signed photo only" means that the beekeeper cannot get an SMS code.
3. **The delivery**: choose the **Apiary**, the **Honey type** and the **Production period** (the current one is preselected), the **Country of origin** and the **Delivery date** (today by default; it cannot be in the future).
4. **Containers and samples**: one row per container ([Figure](#fig-intake-form)).
   - **Type** of container, **Net kg** and, optionally, **Gross kg** (a comma or a dot works as decimal separator).
   - **On arrival**: tick **arrived sealed** and enter its **Seal number**, ticking **seal intact** when it is; for an unsealed container enter the number of the **Plant seal applied now**, in front of the beekeeper.
   - Open the container and fill the sample jar and the counter-sample jar. Enter the **Jar seal** and the **Counter-sample seal**.
   - Tick **Cap intact** only when the cap's button is down. "Tamper-evident cap: tick only when the button is down. A popped cap means the jar was opened: discard it and fill a new jar."
   - Enter the **New seal after sampling** that closes the container again.
   - **Add a container** adds a row; **Remove the row** removes one. **Total net** adds up the containers.
5. **Laboratory and counter-samples**: choose the **Laboratory** (only verified laboratories are listed; an accredited one shows its accreditation number), **Where the counter-samples are kept** and **Keep until** (12 months ahead by default).
6. **The beekeeper confirms**: click **Send code**. The beekeeper gets an SMS with your organization, the date, the containers, the kilograms, the honey type, the number of samples and a code. Enter the **Code from the SMS (6 digits)** that the beekeeper reads out, and click **Confirm the delivery**. "If the beekeeper is not present, call them: they read the code from their phone."

<!-- SHOT: intake-form -->
Figure: Recording a delivery on a phone: containers, seals and sample jars.

The message "Delivery confirmed by the beekeeper and recorded as lot #… ." confirms the new lot. The platform assigns CCP 0–3 to the lot, records every seal as a seal event and sends one sample jar per container to the chosen laboratory.

**The code.** The code is valid for 5 minutes and can be tried 3 times; **Send a new code** sends another one after a minute. If you change anything after sending the code, it is void ("The details changed after the code was sent. Send a new code."). After 10 wrong codes in an hour you must wait 15 minutes, and one officer sends at most 30 codes an hour.

**No SMS possible: signed photo.** If the beekeeper has no mobile phone, or the SMS cannot be sent, open **No SMS possible: signed photo**, print the delivery form, let the beekeeper sign it, choose the **Photo of the signed delivery form** (camera or file: PDF up to 10 MB, or JPG, PNG or WEBP up to 8 MB), write **Why no SMS code** and click **Record with the signed photo**: "Delivery recorded as lot #… with the signed photo; it waits for the Compliance Officer." The lot can be worked on, but it cannot be finalized until the plant's Compliance Officer accepts the photo.

**Common errors**

Table: Common errors when recording a delivery {#tbl-intake-errors}

| Message | What to do |
|---|---|
| "Record at least one container." | Add a row for every container. |
| "Container …: enter the net weight (more than 0 kg)." / "…: the gross weight cannot be lower than the net weight." | Correct the weights of that container. |
| "Container …: enter the number of the seal it arrived with." / "…: apply a plant seal in front of the beekeeper and enter its number." | Every container needs its arrival seal or a plant seal. |
| "Container …: enter the seal numbers of the sample jar and of the counter-sample jar." | Both jars of every container are sealed. |
| "Container …: the jar's cap must be intact (button down) when sealed. A popped cap: discard the jar and fill a new one." | Fill a new jar with an intact cap. |
| "Container …: enter the new plant seal applied after sampling." | Close the container with a new seal. |
| "Every seal number of the delivery must be different." | A seal number is used twice; check the numbers. |
| "Choose one of the beekeeper's apiaries." / "Enter the delivery date (not in the future)." | Correct the delivery fields. |
| "This apiary already has a production record for the selected period and honey type." | One apiary delivers once per period and honey type. Check the period and honey type. |
| "Wrong code. … tries left." / "The code has expired. Send a new code." | Ask the beekeeper to read the code again, or send a new one. |
| "Give the reason why the SMS code cannot be used." / "Attach the photo of the signed delivery form." | The signed-photo path needs both. |
| "Only a Processing organization records deliveries." / "Your role (…) cannot record deliveries; roles: …" | Your organization or role cannot record deliveries. Ask your organization admin. |

The **Deliveries (intake)** list shows every delivery with its lot number, date, apiary, honey type, amount, origin and how it was confirmed. From each row you can open the delivery page, **Sample** and **Recall**.

## The delivery page and the labels {#sao-labels}

The delivery page shows the lot, the containers with their **Seal history**, the **Sample jars** and their status at the laboratory, how the delivery was **Confirmed by** (SMS code or signed photo) and the **Confirmed content** check: "unchanged since confirmation" or, after a correction, the corrected content.

**Labels (A4 sheet)** prints all labels of the delivery on A4 sheets (2 × 5); **Labels (100 × 50 mm)** prints one label per page for a label printer. There is one label per container (`L{lot}-C{n}`), per sample jar (`S-{lot}-C{n}`) and per counter-sample (`S-{lot}-C{n}-K`), each with a QR code and the seal number; the counter-sample label also shows **keep until**.

<!-- SHOT: intake-labels -->
Figure: Labels of a delivery: container, sample jar and counter-sample.

## Take a new jar when the laboratory rejects one {#sao-resample}

If the laboratory rejects a jar (seal number, label or cap not in order), the delivery page shows "The laboratory rejected the jar: take a new one" on that jar. Open the container again, fill a new sample jar and counter-sample jar, enter their seals, tick **Cap intact** and the **New seal after sampling**, and click **Open, sample and seal again**. The new jar (`S-{lot}-C{n}-2`, …) goes to the same laboratory; the seal history shows that the container was opened and sealed again.

## Take a sample of a legacy delivery {#sao-sample}

Deliveries recorded before the container procedure ("legacy (containers not recorded)") have a single sample, taken with the page **Take a sample**:

1. Open **Take a sample** from **Samples and lab** or from the delivery row.
2. Choose the **Laboratory**: only verified laboratories are listed.
3. Enter the **Seal number** of the sample and the **Counter-sample seal number**. They must be different.
4. Enter **Where the counter-sample is kept** and **Keep until** (12 months ahead by default).
5. Click **Send to the laboratory**, or **Later** to send it another time.

<!-- SHOT: sample-form -->
Figure: Taking a sample of a legacy delivery: laboratory and seal numbers.

The sample gets a code such as `S-20070-1` and the status **Sent**. If the laboratory finds the seal broken, the sample is rejected; take a new sample for the same lot.

Errors: "Choose a verified laboratory.", "The sample and the counter-sample need different seals.", "Enter where the counter-sample is kept (at most 200 characters).", "Enter until when the counter-sample is kept (a future date).", "Only the organization that holds the lot takes its samples.", "Only a processing organization takes samples."

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
