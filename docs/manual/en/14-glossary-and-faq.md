# 14. Glossary and FAQ {#glossary-faq}

## Glossary {#glossary}

[Table](#tbl-glossary) defines the terms used in this manual; [Table](#tbl-renamed-labels) lists the labels that change in the next release.

Table: Glossary {#tbl-glossary}

| Term | Meaning |
|---|---|
| **HACCP** | Hazard Analysis and Critical Control Points: the food-safety method the control points come from. |
| **CCP (critical control point)** | A step where a hazard is controlled by measuring or checking. KKN in Turkish, PCC in Spanish, KPK in Polish, KKP in Latvian. |
| **Data** | What people record in the platform: measured values (for example moisture in %) and answers (for example Yes / No). |
| **Parameter** | One item of data recorded at a CCP, for example moisture or temperature. |
| **Critical limit** | The minimum and/or maximum allowed for a parameter. |
| **Verified limit** | A limit confirmed by the project; only verified limits give a public within/out-of-limit verdict. |
| **Within limit / Out of limit** | The comparison of a value with its limit. Out-of-limit values stay flagged after approval. |
| **Not applicable** | A CCP that does not apply to this honey, recorded with a reason; it still needs approval. |
| **Raw honey lot** | The honey of one delivery from one apiary (one period, one honey type). Carries CCP 0–3. |
| **Batch** | Honey made from one or more lots, processed and packed together. Carries CCP 4–10 and the QR code. |
| **Finalize / seal** | The two-person step that locks a batch and its lots and publishes the Honey Story. |
| **Four-eyes principle** | Two different people are involved in every record: one enters the data, a second person checks and approves it. Nobody approves what they entered themselves. See [Ground rules](#fixed-rules). |
| **Four-eyes exemption** | A setting for small organizations that allows self-approval of CCP records, always flagged. Never for lab results. |
| **Sample / counter-sample** | The sealed sample sent to the laboratory, and its twin kept at the plant. |
| **Seal number** | The number on the seal of a sample; the laboratory checks it on receipt. |
| **Lab report / report code** | The laboratory's report (PDF and values) and its public 12-character code. |
| **Food-analysis laboratory** | The laboratory that analyses the honey samples, typically an accredited one. Its analyst enters the results; its lab manager approves them. |
| **Accredited laboratory** | A laboratory with a valid accreditation, recorded when the report is approved. |
| **In-house laboratory** | A laboratory that belongs to the lot's owner. |
| **Holder** | An organization that holds a quantity of a batch after a confirmed shipment. |
| **Custody** | Who holds the goods; it moves when the receiver confirms a shipment. |
| **Recall report** | The list of everything upstream and downstream of a batch or lot. |
| **Honey Story** | The public page of a finalized batch, opened from its QR code. |
| **Beekeeper page** | The public page of a beekeeper, published with consent. |
| **Registry number (AKS)** | The beekeeper's enterprise registry number, `TR` + province code + 10 digits. |
| **Membership** | One user account × one organization × one role. One person can have several memberships, also several in the same organization. |
| **Beekeeper (record)** | Beekeepers have no accounts. Each one is represented in the platform by a Beekeeper record. |
| **Read** | The right to open and read records, without changing them. |
| **Capability** | An ability of an organization: what it is able to do in the honey chain (for example "Able to receive and process honey"). See [Table](#tbl-capabilities). |
| **Ground rules** | The rules built into the platform that no setting can switch off. See [Ground rules](#fixed-rules). |
| **Audit log** | The append-only record of every change, protected by a hash chain. |
| **SHA-256 fingerprint** | A code calculated from a file's content; any change to the file changes it. |

Table: Labels renamed in the next release (the screenshots show the old labels) {#tbl-renamed-labels}

| Today on screen | New name (used in this manual) |
|---|---|
| Quality approver | Quality Assurance Officer |
| Organisation admin | Organization admin |
| Fixed rules (cannot be switched off) | Ground rules (cannot be switched off) |
| Capability descriptions (for example "Reception and processing: …") | Ability texts (for example "Able to receive and process honey: …"), see [Table](#tbl-capabilities) |
| CentralAssociation, PublicAuthority, Beekeeping (codes) | Central association, Public authority, Beekeeping enterprise |
| Messages that name a role by its code ("a QualityApprover", "a LabManager") | The role name ("a Quality Assurance Officer", "a lab manager") |

## FAQ and troubleshooting {#faq}

**I cannot sign in: "Invalid user name or password. …"**
Check the user name (not the e-mail) and the password. After 5 failed attempts the account is locked for 15 minutes; wait, or ask a platform administrator to **Unlock** it, then use **Forgot your password?** if needed.

**"This account has no access to the panel yet: it is not a member of any organization."**
Your account has no active membership. Ask a platform administrator to add you to your organization in the right role.

**I see "You can't open this page" (403).**
Your role or organization may not open this page or this record. For a CCP of another organization this is expected, unless your organization holds that batch. Ask your organization admin if you need access.

**I see "This page isn't here" (404).**
The address is wrong, the record does not exist, or (for the Honey Story) the batch is not finalized yet.

**I cannot enter anything; the page says "Read only".**
Read the reason under it. Typical reasons: your organization is not verified yet; another organization or role is responsible for this CCP; the record is locked by finalizing.

**My organization is not verified.**
The organization admin uploads a document (licence, accreditation, registry proof) on the organization page; a platform administrator then verifies it.

**The lab cannot enter results: "CCP 0 (primary audit) of this lot is not completed yet; …"**
The plant must enter CCP 0 and its Quality Assurance Officer must approve it first.

**I cannot approve: "Four-eyes: you recorded values of this CCP, so another person must approve it."**
Another approver must do it. If your organization has only one quality person, ask a platform administrator about the four-eyes exemption (not available for lab results).

**"Request finalizing" is disabled.**
Read the list "Missing before finalizing:". Every batch CCP, including CCP 10 at the plant, and every lot CCP must be approved.

**I changed a value and the approval disappeared.**
That is intended: any change needs a new approval.

**The lot does not appear on "Manage Batch".**
A lot appears only when all its CCPs are completed or not applicable and approved, and only for the batch's wholesaler. A processing plant must be linked to its wholesaler record (organization → **Edit**); the page names plants that are not.

**A value was approved but is wrong.**
Ask a platform administrator for a correction; a second administrator approves it.

**The beekeeper page is not public.**
Check that the consent is recorded, the registry number is verified, the association is verified and the beekeeper is active.

**I work for two organizations.**
Switch with the organization · role chip in the top bar. The choice lasts until you close the browser.

**I don't receive the password reset e-mail.**
Check the spam folder. A platform administrator can set a new password for you.
