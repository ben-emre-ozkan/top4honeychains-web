# 9. Lab manager {#role-lab-manager}

## Your daily work {#labmanager-daily}

You approve or return the lab reports your analysts submit. You can also receive samples. You never approve a report whose results you entered yourself; for lab results there is no exemption from this rule.

Your tasks appear in **My work** under **Lab reports to approve** (and **Samples to receive**).

## Approve or return a report {#labmanager-decide}

1. In **My work** open **Lab reports to approve**.
2. Check the report: the PDF, the report number, the issue date and every value. Out-of-limit values are flagged.
3. To approve, click **Approve**. If values are out of limit, the dialog **Values outside their limits** asks you to confirm; click **Approve anyway**.
4. To return the report, write the **reason (to return)** and click **Return**: "Report returned to the analyst."

<!-- SHOT: lab-approve -->

Approving confirms that the report is recorded correctly, not that the honey complies; values outside their limits stay flagged on the lot.

**What approval does** ("Lab report approved; the results are now the lot's CCP 1 values."):

- The report becomes **Approved** and visible on the public verification page.
- The platform records whether the laboratory was **accredited** at that moment (with its accreditation number) and whether it is an **in-house** laboratory of the lot's owner.
- The results become the lot's **CCP 1** values. They can no longer be edited, only corrected by the administrators. If the laboratory covered every CCP 1 parameter, CCP 1 is completed and approved at once; otherwise it goes to the plant's quality approver.

**Common errors**

| Message | Meaning |
|---|---|
| "Four-eyes: the person who entered the results cannot approve them (no exemption for lab results)." | Another lab manager must approve. |
| "A reason is required to return a report." | Write the reason. |
| "The stored report file does not match its SHA-256 hash; upload it again." | The stored PDF changed; the analyst must upload it again. |
| "CCP 0 (primary audit) of this lot is not completed yet; …" | Approval waits for CCP 0. You can still return the report. |

## Verify a report publicly {#labmanager-verify}

Anyone can check a report at **Verify a lab report** (`/verify-report`): enter the **Report code** and click **Check**. The page shows whether the report is approved, the laboratory, its accreditation, the report number, the date and the SHA-256 fingerprint. **Compare a PDF with this fingerprint (the file stays on your device)** checks a PDF on the visitor's own device (the file is not uploaded). No lot, beekeeper or user data is shown.

<!-- SHOT: verify-report -->
