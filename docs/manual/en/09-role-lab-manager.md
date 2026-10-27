# 9. Lab manager {#role-lab-manager}

## Your daily work {#labmanager-daily}

You are the second of the laboratory's two people: you approve or return the lab reports your analysts submit. You can also receive sample jars (seal, label and cap checks, see [Receive a sample](#analyst-receive)) and make the composite sample. You never approve a report whose results you entered yourself; for lab results there is no exemption from this rule.

Your tasks appear in **My work** under **Lab reports to approve** (and **Samples to receive**).

## Approve or return a report {#labmanager-decide}

1. In **My work** open **Lab reports to approve**.
2. Check the report: the PDF, the report number, the issue date and every value. Out-of-limit values are flagged.
3. To approve, click **Approve**. If values are out of limit, the dialog **Values outside their limits** asks you to confirm; click **Approve anyway**.
4. To return the report, write the **reason (to return)** and click **Return**: "Report returned to the analyst."

<!-- SHOT: lab-approve -->
Figure: The lab manager approves or returns a report.

Approving confirms that the report is recorded correctly, not that the honey complies; values outside their limits stay flagged on the lot.

**What approval does** ("Lab report approved; the results are now the lot's CCP 1 values."):

- The report becomes **Approved** and visible on the public verification page.
- The platform records whether the laboratory was **accredited** at that moment (with its accreditation number) and whether it is an **in-house** laboratory of the lot's owner.
- The results become the lot's **CCP 1** values: the composite's values, or, when every jar is analysed on its own, the worst value of each parameter over the containers (see [The worst-value rule](#flow-worst)). A single-jar report in composite analysis stays with its jar. They can no longer be edited, only corrected by the administrators. If the laboratory covered every CCP 1 parameter, CCP 1 is completed and approved at once; otherwise it goes to the plant's Quality Assurance Officer.

**Common errors**

Table: Common errors when approving a lab report {#tbl-labmgr-errors}

| Message | Meaning |
|---|---|
| "Four-eyes: the person who entered the results cannot approve them (no exemption for lab results)." | Another lab manager must approve. |
| "A reason is required to return a report." | Write the reason. |
| "The stored report file does not match its SHA-256 hash; upload it again." | The stored PDF changed; the analyst must upload it again. |
| "CCP 0 (primary audit) of this lot is not completed yet; …" | Approval waits for CCP 0. You can still return the report. |

## Verify a report publicly {#labmanager-verify}

Anyone can check a report at **Verify a lab report** (`/verify-report`): enter the **Report code** and click **Check**. Only **approved** reports are found; a draft or a submitted report answers like an unknown code. The page shows the approval date, the laboratory, its accreditation, the report number, the issue date and the SHA-256 fingerprint. Under **Compare a PDF with this fingerprint (the file stays on your device)**, **Choose PDF** checks a PDF on the visitor's own device (the file is not uploaded) and answers whether it matches. No lot, beekeeper or user data is shown.

<!-- SHOT: verify-report -->
Figure: Public verification of a lab report on a phone.
