# 8. Analyst {#role-analyst}

## Your daily work {#analyst-daily}

You work in a food-analysis laboratory and are the first of its two people: you enter, the lab manager approves. You receive the sealed sample jars that processing plants send you (one from every container of a delivery), check seal, label and cap, make the composite sample, analyse the honey, enter the results with the report PDF, and submit the report to your lab manager for approval.

Your tasks appear in **My work** under **Samples to receive** and **Lab results to enter**. The menu shows **Samples and lab**.

## Receive a sample {#analyst-receive}

1. In **My work** open **Samples to receive**, or go to **Samples and lab** → **Samples sent to your laboratory**. Each jar shows its code (`S-{lot}-C{n}`), its container and its seal number.
2. Answer the three checks for the jar: **The seal number matches the record**, **The label is intact** and **The cap is intact (button down)**.
3. If all three are **Yes**, click **Record the reception**: the jar is received.
4. If any check is **No**, write the **Reason if a check fails** and click **Record the reception**: the jar is rejected. The plant takes a new jar from the same container and sends it to you.

<!-- SHOT: sample-receive -->
Figure: Receiving sample jars at the laboratory: seal, label and cap.

A sample of a legacy delivery (one sample per lot) has the single seal check: **Received, seal intact**, or the **reason (if the seal is broken)** and **Seal broken: reject**.

## Make the composite sample {#analyst-composite}

With composite analysis (the default), the lab analyses one composite sample per lot (see [Composite or per container](#flow-analysis)). When every container's jar of a lot has arrived in good order, the lot appears under **Lots ready for the composite sample**: "Every container's jar of these lots arrived in good order. Mix equal parts of the jars into one composite sample and analyse it; the jars stay retained for single analyses."

1. Mix equal parts of the jars.
2. Click **Make the composite**. The composite sample `S-{lot}-COMP` is created and received; its report is entered like any other.

When the composite fails, or on request, you can also enter a report for a single jar; it stays with that jar and does not change the lot's CCP 1 values. With per-container analysis there is no composite: every jar gets its own report, and the lot's CCP 1 takes the worst value of each parameter (see [The worst-value rule](#flow-worst)).

## Enter lab results {#analyst-results}

1. In **My work** open **Lab results to enter**. The **Lab report** page opens for the sample.
2. Check that the page does not say "CCP 0 (primary audit) of this lot is not completed yet; …". While CCP 0 of the lot is not approved, the form is read-only. Wait until the plant's Quality Assurance Officer approves CCP 0.
3. Enter the **Report number** and the **Issue date** (not in the future).
4. Choose the **Report PDF** (a PDF up to 10 MB).
5. Enter the value of each parameter. Required parameters are marked with *. Numbers accept a comma or a dot.
6. Click **Save**: "Lab results saved."

<!-- SHOT: lab-results-form -->
Figure: Entering lab results with the report PDF.

After saving, the platform:

- stores the PDF with its SHA-256 fingerprint (shown with a copy button);
- generates the 12-character **Report code**, which anyone can check on the public verification page;
- compares each value with its limit (**Within limit** / **Out of limit**).

You can save as often as you like while the report is a draft.

## Submit the report {#analyst-submit}

1. The page shows "Required values entered: … of …"; the count follows your typing, and "Missing:" names the values still empty. **Submit for approval** is enabled only when all required values, the PDF, the report number and the date are **saved**: when everything is typed but not saved yet, the page says "Save the values before submitting."
2. Click **Submit for approval**: "Report submitted; a lab manager (another person) must approve it."

<!-- SHOT: lab-submit -->
Figure: All required data entered: the report can be submitted.

After submitting, the report can no longer be edited unless the lab manager returns it. A returned report shows the reason; correct it and submit again.

**Common errors**

Table: Common errors when entering lab results {#tbl-lab-errors}

| Message | What to do |
|---|---|
| "Results are entered only for a received sample with an intact seal." | Receive the jar first; a rejected jar gets no results. |
| "Upload the report PDF before submitting." | Choose the PDF and save. |
| "Enter all required values before submitting: … of … entered." | Complete the missing values listed under "Missing:". |
| "The lab report must be a PDF." | Only PDF files are accepted for the report. |
| "The report is submitted; the lab manager approves it or returns it for changes." | Wait for the lab manager. |
| "An approved lab report cannot be changed; request a correction instead." | Ask a platform administrator for a correction. |
| "The lot is locked in a finalized batch; lab values can only be changed through a correction." | The lot was sealed in a batch; a draft can no longer be saved or submitted. Ask a platform administrator. |
