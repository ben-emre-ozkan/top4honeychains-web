# D-90 – Şile Arıcılar Birliği: field visit and design options

Design note only: no workflow code changes come with D-90. The owner visited Şile Arıcılar Birliği (İstanbul/Şile) in November 2026. Since D-90 the local live-like cast uses the association's real name. With the capabilities Association, Processing and Packing, the association receives, samples, processes, packs and labels the honey. A fictional laboratory tests it, and a separate shop sells it.

## 1. What we saw, and what the app covers today

| # | Field fact | Today | Where |
|---|---|---|---|
| 1 | Beekeepers bring their honey to the association, which takes samples from it. | **Supported.** The association records the delivery in sealed containers. The beekeeper confirms it with an SMS code, and each container gets a sample jar, a counter-sample and a new seal. | Intake → New delivery (`/Intake/CreateCustody`); `LotIntakes`, `Containers`, `Samples` (Kind = Container) |
| 2 | Several beekeepers' samples are mixed in a set ratio and sent to a university laboratory for a pre-test, because testing is expensive. A full test follows later. | **Partly.** The laboratory makes a composite sample, but only from the jars of **one lot**, with no ratio. There is one kind of report, and no pre-test. | `/Samples/Composite` (`LabCustody.CompositeSourcesAsync`); `SampleSources`; `LabReports` (no kind column) |
| 3 | If the mix passes, every sample in it passes. If it fails, they look back at which samples went into it. | **Partly.** An approved report fills CCP 1 of its own lot only. `SampleSources` already records which jars went into a composite, so the trace exists within one lot. | CCP 1 (`CriticalControlPointMonitoringLogs.LabReportId`); `SampleSources` |
| 4 | Counter-samples are kept in a cabinet in the management office. | **Supported.** Each container has its counter-sample seal, a storage place ("Yönetim ofisi, şahit numune dolabı") and a keep-until date. | Delivery form: Counter-sample seal, Counter-sample location, Keep until; `Samples.CounterSealNumber`, `CounterLocation`, `RetainUntil` |
| 5 | Packing and label printing happen in the association's own facility, and the honey is sold under the cooperative's identity. | **Mostly.** Processing and Packing let the association run the batch sheet (CCP 4–9) and finalize, and the batch's QR code can be downloaded for the label. The Association capability gives the association a public storefront. There is no label designer: the label itself is printed outside the app. | My organization → New batch, batch control sheet, Finalize, **Download QR**; Storefronts |
| 6 | Outside beekeepers may bring honey too. In general, only registered beekeepers may bring honey for "Şile Kestane Balı" (geographical indication). | **Partly.** Any registered beekeeper can deliver, member or not, and membership is recorded (`BeekeeperAssociations`). Nothing marks a lot or batch as eligible for the geographical indication, and nothing stops a non-member's lot from going into such a batch. | Registration (Association field); `BeekeeperAssociations`; New batch |

## 2. Minimal design options

**A. A pooled sample across several beekeepers' lots (made by the association, with a recorded ratio).**
- *Option A1 (recommended):* a new sample kind `Pool`, made by the **receiving association** from the counter-samples or extra jars of several lots of the same honey type and season.
  - Each source row in `SampleSources` gets a `Share` (percent or grams). A new column is added by an additive migration.
  - The pool form lists the lots ready for pooling, with a share per lot, and the shares must add up to 100 %.
  - The pool travels to the laboratory with its own seal, like a jar today.
- *Option A2:* the laboratory pools the jars it received. This is simpler, but it does not match what we saw: the association mixes, and the lab only tests.

**B. Pre-test, then full test.**
- Add `LabReports.Kind` with the values `PreTest` and `Full`.
  - A **pre-test** checks a short parameter set chosen by the association, for example moisture, HMF, diastase, electrical conductivity and pollen/sensory: the parameters that fail chestnut honey most often.
  - A **full test** checks every CCP 1 parameter.
- **CCP 1:** an approved full test fills CCP 1 as today. A passed pre-test only marks the pooled lots "pre-test passed", which allows them into a batch only if the association's rule says so (a setting, default off). The lot sheet stays incomplete until a full test covers it, either of the pool or of a later batch sample.
- **Honey Story:** it shows the full test with its verification code, as today. A pre-test is shown only as the line "Pre-tested in a pool of N deliveries on date", and its numbers are never shown as the honey's results.

**C. The pool passes → every lot in it passes.**
- Approving a pool's full report fills CCP 1 of **each source lot** with the same values, and each record links to the report (`LabReportId`).
- Each lot's Honey Story says "tested in a pool (report …)".

**D. The pool fails → trace back.**
- The failed report lists the source lots and their shares, and opens a task "Test lot by lot" in the association's My work, one item per lot.
- For each lot, the counter-sample is taken from the cabinet, its seal is checked, and it is sent as an ordinary single-lot sample.
- Until its own result arrives, each lot is blocked from New batch. A lot that fails on its own takes the existing non-conformity path.

**E. "Şile Kestane Balı" eligibility.**
- A per-association setting names the protected product, for example "Şile Kestane Balı": honey type Chestnut, the district list and the member-only rule.
- New batch then offers a "protected name" box. It is available only when every lot comes from an active member with an apiary in the listed districts, and the batch title and Honey Story carry the name only then.
- The check runs again at finalizing.

**F. Outside beekeepers.**
- Deliveries from non-members stay allowed, as today.
- The delivery form shows "Member" or "Not a member" next to the beekeeper's name, and the lot carries that flag.
- Such lots can go into ordinary batches, never into a protected-name batch (E).
- Optionally, an association can turn off deliveries from non-members.

## 3. Open questions for the association president

1. How is the ratio set: by weight delivered, the same amount per beekeeper, or another rule? Who decides it, and is it written down?
2. How many beekeepers go into one pool, and must they share the same honey type, harvest week or village?
3. Which parameters does the pre-test cover, which does the full test cover, and does the full test run on the pool, on each lot, or on the final batch?
4. When a pool fails, do you test every lot in it, or only the suspect ones? What happens to the honey while you wait?
5. Is the pre-test result ever shown to buyers, or only the full test?
6. Who may use the name "Şile Kestane Balı": members only, members with apiaries inside Şile, or anyone the association accepts? Who checks this (the association, the Patent Office registration holder, the directorate)?
7. What does the association do with outside beekeepers' honey: a separate brand, bulk sale, or the same batches without the protected name?
8. How long do counter-samples stay in the cabinet, and who may open it?
9. Which laboratory do you use for each kind of test, and do they send a PDF report with an accreditation number?
10. Do you print the labels yourselves? Which printer and label size, and should the QR code be on every jar?

## 4. Rough effort (development and tests; manuals and translations extra)

| Part | Effort |
|---|---|
| A. Pool sample with shares (migration, form, custody, tests) | 3–4 days |
| B. Report kinds and the pre-test parameter set | 2–3 days |
| C. Pool result fills CCP 1 of each source lot, Honey Story line | 2 days |
| D. Trace-back tasks, lot-by-lot counter-sample tests, batch block | 2–3 days |
| E. Protected-name setting and checks at New batch and finalizing | 2–3 days |
| F. Member flag on deliveries and the optional "members only" switch | 1 day |
| **Total** | **about 12–16 working days**, after the president's answers. A and C alone (pool plus pass-for-all) are about 5–6 days. |
