# 8. Analyst {#role-analyst}

## Your daily work {#analyst-daily}

You work in a food-analysis laboratory and are the first of its two people: you enter, the lab manager approves. You receive the sealed samples that processing plants send you, check the seal, analyse the honey, enter the results with the report PDF, and submit the report to your lab manager for approval.

Your tasks appear in **My work** under **Samples to receive** and **Lab results to enter**. The menu shows **Samples and lab**.

## Receive a sample {#analyst-receive}

1. In **My work** open **Samples to receive**, or go to **Samples and lab** → **Samples sent to your laboratory**.
2. Compare the seal number on the sample with the one shown.
3. If the seal is intact, click **Received, seal intact**: "Sample received; the seal is intact."
4. If the seal is broken, write the **reason (if the seal is broken)** and click **Seal broken: reject**: "Sample rejected: the seal is broken." The plant must take a new sample.

<!-- SHOT: sample-receive -->
Figure: Receiving samples at the laboratory: the seal check.

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

1. The page shows "Required values entered: … of …". **Submit for approval** is enabled only when all required values, the PDF, the report number and the date are present.
2. Click **Submit for approval**: "Report submitted; a lab manager (another person) must approve it."

<!-- SHOT: lab-submit -->
Figure: All required data entered: the report can be submitted.

After submitting, the report can no longer be edited unless the lab manager returns it. A returned report shows the reason; correct it and submit again.

**Common errors**

Table: Common errors when entering lab results {#tbl-lab-errors}

| Message | What to do |
|---|---|
| "Results are entered only for a received sample with an intact seal." | Receive the sample first. |
| "Upload the report PDF before submitting." | Choose the PDF and save. |
| "Enter all required values before submitting: … of … entered." | Complete the missing values listed under "Missing:". |
| "The lab report must be a PDF." | Only PDF files are accepted for the report. |
| "The report is submitted; the lab manager approves it or returns it for changes." | Wait for the lab manager. |
| "An approved lab report cannot be changed; request a correction instead." | Ask a platform administrator for a correction. |
| "The lot is locked in a finalized batch; lab values can only be changed through a correction." | The lot was sealed in a batch; a draft can no longer be saved or submitted. Ask a platform administrator. |
