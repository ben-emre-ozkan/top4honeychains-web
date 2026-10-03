# CCPs in the current system (as-is)

Status: read-only investigation on 2026-10-03 against `main` (`b41127b`) and the local DB copy of production.
No code or data was changed. Usage numbers are aggregates only. Paths are relative to `TOP4HONEYCHAINS/TOP4HoneyChains.Web/` unless noted.

---

## 1. CCP definitions

### 1.1 Data model

| Table | Purpose | Columns that matter |
|---|---|---|
| `CriticalControlPoints` | One row per CCP | `Id`, `Code` (text, e.g. `CCP 1`), `Title`, `ProcessingStep`, `CriticalityJustification` |
| `CriticalControlPointParameterCategories` | Groups parameters on the entry form | `CategoryName`, `CategoryDesc` |
| `CriticalControlPointParameters` | Parameters of a CCP | `CCPId`, `ParameterName`, `ParameterCategory`, `ParameterUnit` (free text), `CriticalLimit` (free text, `nvarchar(100)`) |
| `CriticalControlPointParameterContents` | Public "what is it / why important" text and an optional video per parameter | `WhatIsIt`, `WhyImportant`, `ExtraInfo`, `VideoPath`, `IsActive` |
| `RawHoneyCCPAssignments` | CCP assigned to a raw honey lot | `RawHoneyCCPAssignmentCCP`, `RawHoneyProductionId`, `…Status`, start/end date. Unique on (lot, CCP) |
| `FinalHoneyCCPAssignments` | CCP assigned to a final batch | same shape, unique on (batch, CCP) |
| `CriticalControlPointMonitoringLogs` | One measured value per parameter per lot/batch | `CCPId`, `ParameterId`, `RawHoneyProductionId` **or** `FinalHoneyProductionId`, `MeasuredNumericValue decimal(10,4)`, `MeasuredTextValue nvarchar(100)`, `MeasurementDate`, `Notes` |
| `AssignmentStatuses` | Lookup | 1 Assigned, 2 In Progress, 3 Completed, 4 Rejected |

What the model does **not** have:

- No data type per parameter. Every parameter shows both a numeric box and a text box.
- No min/max or acceptable range. `CriticalLimit` is a free-text column, and in the DB all 69 rows hold the literal string `None`. 35 of 69 units are also the string `None`.
- No required/optional flag. Every parameter of a CCP is required for completion (see §2.3).
- No "not applicable" / "skipped" value or status.
- No owner, role or organization per CCP or per parameter.
- No user/author on a log (`MeasurementDate` only; who entered it is not recorded).

### 1.2 CCPs in the DB

`Id` = CCP number + 1 (`CCP 0` is `Id 1`). There is a **CCP 0**.

| Code | Title | Processing step | Category | # params |
|---|---|---|---|---|
| CCP 0 | Primary Audit | Pre-Production | 5 categories (below) | 21 |
| CCP 1 | Sampling and Preliminary Acceptance Tests | Raw Honey Reception | CCP 1 Monitoring Parameters | 17 |
| CCP 2 | Temperature Monitoring | Heating | CCP 2 Monitoring Parameters | 5 |
| CCP 3 | Filter Condition Control | Filtration | CCP 3 Monitoring Parameters | 3 |
| CCP 4 | Homogeneity Control | Homogenization & Mixing | CCP 4 Monitoring Parameters | 4 |
| CCP 5 | Time and Temperature Control | Pasteurization | CCP 5 Monitoring Parameters | 6 |
| CCP 6 | Foam and Sediment Removal | Settling (Clarification) | CCP 6 Monitoring Parameters | 3 |
| CCP 7 | Quantity Control | Filling | CCP 7 Monitoring Parameters | 2 |
| CCP 8 | Label Accuracy Verification | Labeling | CCP 8 Monitoring Parameters | 3 |
| CCP 9 | Packaging Integrity Check | Packaging | CCP 9 Monitoring Parameters | 3 |
| CCP 10 | Temperature and Humidity Monitoring | Storage | CCP 10 Monitoring Parameters | 2 |

Criticality justifications (as stored): CCP 0 "Ensures GHP (Good Hygiene Practices) at the source"; CCP 1 "To ensure incoming honey complies with safety and quality standards before further processing"; CCP 2 "Prevents excessive heating which may degrade quality or violate regulatory limits"; CCP 3 "Ensures effective removal of impurities and foreign matter"; CCP 4 "Maintains product uniformity and consumer acceptability"; CCP 5 "Prevents under/over-processing which can affect safety and quality"; CCP 6 "Enhances product clarity and appearance"; CCP 7 "Ensures accurate labeling and regulatory compliance" (reads like a labeling text, but it is on Filling); CCP 8 "Prevents mislabeling and consumer misinformation"; CCP 9 "Protects product from contamination and supports shelf life"; CCP 10 "Preserves product stability and inhibits microbial activity".

### 1.3 Parameters

For every parameter below: data type = none (numeric or text), range = none (`CriticalLimit` = `None`), required = yes (implicitly). "–" = unit stored as the string `None`.

**CCP 0 – Primary Audit** (21 yes/no style audit questions, unit –)

| Category | Questions (shortened) |
|---|---|
| CCP 0 Hive Hygiene and Maintenance (9) | unused tools cleaned and stored; hive boxes/frames/foundation managed; smokers used; colony/brood/pest assessed; samples of debris/honey/brood sent to a lab; temperature/humidity monitored (and how); hive components cleaned/disinfected; which disinfection methods; hygiene records kept |
| CCP 0 Pesticide Usage and Residue Monitoring (3) | risk assessment of pesticide use; timing/dosage/frequency of permitted pesticides (varroa); non-chemical methods used |
| CCP 0 Water Quality and Sanitation (3) | water verified by accredited lab tests and how often; cleaning water suitable; water for sugar feeding suitable |
| CCP 0 Honey Harvesting & Extraction Practices (4) | harvest timing by ripeness/nectar source; physical hazards monitored; biological hazards monitored; chemical hazards tracked |
| CCP 0 Transportation and Storage (2) | transport/storage designed against adverse conditions; temperature/humidity monitored during transport/storage |

**CCP 1 – Sampling and Preliminary Acceptance Tests**

| Parameter | Unit |
|---|---|
| Moisture | % (Percentage) |
| HMF | mg/kg |
| Diastase | DN (Schade) |
| C4 Sugar | pct |
| Sucrose | % |
| Glikoz (glucose) | % |
| Fruktoz (fructose) | % |
| Maltoz (maltose) | % |
| Pesticides | ppb |
| Antibiotics | ppb |
| Electrical Conductivity | mscm |
| Proline | mg/kg |
| pH | pH |
| Free Acidity | meq/kg |
| Insoluble Matter | pct |
| Sensory Analysis | – |
| Naphthalene (added later, `ParameterId 69`) | ppb |

**CCP 2 – Heating:** Temperature (°C), HMF (mg/kg), Diastase (DN), Moisture (%), Proline (mg/kg).
**CCP 3 – Filtration:** Filter integrity (Micron µm), Sediment presence (–), Pore size (–). The µm unit is on "Filter integrity", not on "Pore size".
**CCP 4 – Homogenization & Mixing:** Visual appearance (–), Viscosity (–), Color consistency (–), Moisture (%).
**CCP 5 – Pasteurization:** Temperature (°C), Holding Time (Minutes/Seconds), HMF, Diastase, Moisture, Proline.
**CCP 6 – Settling:** Visual Inspection (–), Holding Time (Minutes/Seconds), Temperature (°C).
**CCP 7 – Filling:** Net weight (g), Visual Inspection (–).
**CCP 8 – Labeling:** Origin, Botanical source, Analytical consistency (all –).
**CCP 9 – Packaging:** Material type, Cleanliness, Physical durability (all –).
**CCP 10 – Storage:** Temperature (°C), Relative Humidity (%). The design's "≤ 25 °C" is not stored anywhere.

Parameter contents (public info pages): 1 row in total, with 1 video. Not part of the CCP flow.

---

## 2. Attachment and flow

### 2.1 What a CCP attaches to

Both. A CCP can be assigned to a **raw honey lot** (`RawHoneyCCPAssignments`) and/or to a **final batch** (`FinalHoneyCCPAssignments`). Logs point to one of the two (never both, never neither in the data).

There is **no rule** that says which CCPs belong to which side. Both "Manage CCP" pages list all 11 CCPs (`RawHoneyProductionsController.ManageCCP`, `FinalHoneyProductionsController.ManageCCP`). The split exists only:

- by convention in the data: all 59 raw lots with assignments have exactly {CCP 0, 1, 2, 3}; all 5 final batches have exactly {CCP 4 … 10};
- hard-coded in the public QR page (`HoneyStoryController`): `Details` reads raw-lot logs for `CCPId 2–4` (= CCP 1–3; the code comment says "CCP0-CCP3", so CCP 0 is silently left out) and batch logs for `CCPId 5–11` (= CCP 4–10). `Index` reads only `CCPId == 2` (CCP 1).

### 2.2 Assigning (Manage CCP)

- Raw lot (`RawHoneyProductions/ManageCCP`): a switch per CCP. New ones are inserted with status 1 (Assigned). Unchecking a CCP that already has logs is refused ("cannot be removed because monitoring data exists").
- Final batch (`FinalHoneyProductions/ManageCCP`): same switches, status 1. Unchecking **deletes the assignment even if logs exist** (the logs stay, orphaned). Neither the assignment page nor the log entry page checks whether the batch is finalized, so CCPs and values of a finalized (public) batch can still be changed.

### 2.3 Entering values and "complete"

`CCPMonitoringController.RawHoneyProductionManageCCP` / `FinalHoneyProductionManageCCP` (POST):

1. For each parameter of the CCP: a numeric or non-blank text value creates/updates one log; an empty field deletes the existing log.
2. Completion: count the CCP's parameters (`totalParameters`) and the distinct parameters with a value for this lot/batch (`loggedParameters`).
   - Raw lot: if equal and > 0 → status 3 Completed + end date; otherwise → status 2 In Progress, end date cleared.
   - Final batch: if equal and > 0 → status 3 Completed. Otherwise **nothing** (stays 1, never set to 2; clearing values of a completed CCP does not revert it).
3. Values are never compared with any limit. Any text (e.g. "-") counts as a value. Status 4 Rejected is never set by code.
4. The page does not check that the CCP is actually assigned to that lot/batch (the IDs come from the query string).

A CCP is therefore "complete" when **every** parameter has some value. There is no N/A option, no optional parameter, and no pass/fail.

### 2.4 Finalize rule

Two separate gates:

- **Raw lot → batch eligibility** (`ManageBatch` GET): a lot of the same wholesaler is offered only if it has **at least one** raw CCP assignment **and all** its raw assignments have status 3. `ManageBatchPost` re-checks only "all assignments are status 3", which is also true for a lot with **no** assignments (not reachable from the UI, only via a crafted POST). A DB trigger (`TR_Check_WholesalerConsistency`) enforces same-wholesaler.
- **`FinalizeBatch`**: checks only that the batch is not already finalized and that the sum of used amounts > 0. It does **not** look at the batch's own CCPs (CCP 4–10). `UnfinalizeBatch` has a commented-out role check.

So a batch can be finalized, and gets a public QR page, with all final-side CCPs still unstarted. In the data, 4 of the 5 finalized batches are exactly that.

---

## 3. Who can enter what

- All CCP pages (definitions, parameters, categories, contents, assignments, monitoring, finalize) are in controllers with `[Authorize(Roles = "Administrator")]`. Only `HoneyStoryController` (public QR page, parameter info) is anonymous.
- There is **no per-CCP role mapping** in code or DB. Nothing refers to an association, BALMER or TUMA in the live app.
- Roles in `AspNetRoles` (user counts): Administrator 2, Beekeeper 62, Consumer 0, SampleAcceptanceOfficer 0, Wholesaler 0. Only Administrator is enforced (see FB-33 / D-01). `SampleAcceptanceOfficer` exists only as a DB row; no code uses it.
- Organization-like data: only `Wholesalers` (7 rows, all active): Anatolia Honey Trading A.Ş. (Istanbul), Ege Flora Apiculture Ltd. (Izmir), Mediterranean Bee Products A.Ş. (Antalya), Central Anatolia ApiTrade A.Ş. (Ankara), BlackSea Honey Logistics Ltd. (Ordu), Eastern Anatolia Bee Supply A.Ş. (Erzurum), Southeastern Api Export A.Ş. (Gaziantep). There is no entity for associations, packers, labs or storage operators.
- Legacy hints (not live): `TOP4HoneyChainsWebApps` has a beekeeper "Association Membership" field and an "Apiary Barcode" label; `TOP4HONEYCHAINS/SQLQuery1.sql` (draft, never applied) defines `PackingCompanies`, `PackingCompanyTanks`, `HoneySellingCompanies`, `RawHoneyLots`, `FinalProductBatches`, `BatchCompositions`, `HoneyProducts`.

---

## 4. Related concepts

| Concept | In the live app? | Notes |
|---|---|---|
| Samples / sample IDs | No | CCP 1 is called "Sampling", but there is no sample entity, sample code or chain of custody. |
| Barcodes | No | Only a GUID QR per final batch (`FinalHoneyProductionQR`). Legacy app had an "Apiary Barcode" label. |
| Lab entities | No | No lab, accreditation, method or report number. Legacy app had `HoneyTests`, `HoneyTestStandards`, `HoneyTestStandardItems`, `HoneyTestResults` (not in the current DB). |
| File uploads per CCP (lab reports) | No | Uploads exist only for apiary photos and parameter info videos. |
| Heat-treatment flag | No | Heating (CCP 2) and Pasteurization (CCP 5) are just CCPs; nothing on the lot/batch says "heat-treated". |
| Label data | No | CCP 8 stores three free values; there is no label entity (net weight, origin, botanical source, best-before, lot code). |

---

## 5. Usage (aggregates, local copy of production)

Totals: 61 raw lots (59 with CCP assignments), 5 final batches (all finalized, all of wholesaler "Anatolia Honey Trading A.Ş."), 22 batch–lot links, 478 logs, 236 raw assignments, 35 final assignments. Logs were entered on 4 days between 2026-02-23 and 2026-03-02.

| CCP | Side | Assignments | Completed | % completed | Logs | Logs sharing their value with another log of the same parameter | Params never filled |
|---|---|---|---|---|---|---|---|
| CCP 0 | raw | 59 | 10 | 17 % | 210 | 210 (100 %) | 0 / 21 |
| CCP 1 | raw | 59 | 10 | 17 % | 165 | 165 (100 %) | 0 / 17 |
| CCP 2 | raw | 59 | 10 | 17 % | 50 | 50 (100 %) | 0 / 5 |
| CCP 3 | raw | 59 | 10 | 17 % | 30 | 30 (100 %) | 0 / 3 |
| CCP 4 | batch | 5 | 1 | 20 % | 4 | 0 | 0 / 4 |
| CCP 5 | batch | 5 | 1 | 20 % | 6 | 0 | 0 / 6 |
| CCP 6 | batch | 5 | 1 | 20 % | 3 | 0 | 0 / 3 |
| CCP 7 | batch | 5 | 1 | 20 % | 2 | 0 | 0 / 2 |
| CCP 8 | batch | 5 | 1 | 20 % | 3 | 0 | 0 / 3 |
| CCP 9 | batch | 5 | 1 | 20 % | 3 | 0 | 0 / 3 |
| CCP 10 | batch | 5 | 1 | 20 % | 2 | 0 | 0 / 2 |

- Every non-completed assignment is still status 1 (Assigned): 196 raw, 28 batch. Status 2 and 4 occur 0 times.
- The 10 completed raw lots all belong to one wholesaler; the other 6 wholesalers have 51 lots with no completed CCP.
- **Identical values:** for CCP 0, 1 and 2 every parameter has exactly **one** distinct value across all 10 lots, i.e. the 10 forms are identical (CCP 1 shows 2 distinct forms only because Naphthalene was filled for 5 lots). CCP 3 has 2 distinct forms. The batch side has one completed batch, so duplicates cannot occur there. This looks like demo/test entry, not real measurements.
- Value types: 239 logs numeric, 239 text, none both. CCP 0 is all text (mostly 3–4 characters); CCP 1 is numeric except Sensory Analysis. No text value parses as a number. No log has notes.
- Integrity: 0 logs without a matching assignment, 0 logs whose CCP differs from the parameter's CCP, 0 batch lines from ineligible lots.
- **4 of 5 finalized (public) batches have CCP 4–10 not started** (status 1, no logs).
- "Always empty" parameters: none. Every one of the 69 parameters has at least one value (most on the batch side only once).

---

## 6. Comparison with the design (WP4 report, Table 1)

| Design | System | Differences |
|---|---|---|
| — | CCP 0 Primary Audit (Pre-Production), 21 audit questions | **Extra CCP** not in Table 1 (GHP checklist at the apiary). |
| CCP1 Raw honey reception / sampling & pre-acceptance tests: moisture, HMF, diastase, C4 sugar, sucrose, pesticides, antibiotics, EC, proline, pH, free acidity, insoluble matter, sensory — Association | CCP 1, raw lot | All 13 present. **Extra:** Glucose, Fructose, Maltose (Turkish names "Glikoz/Fruktoz/Maltoz"), Naphthalene. No sample entity. No owner. |
| CCP2 Heating: temperature, HMF, diastase, proline — Association | CCP 2, raw lot | **Extra:** Moisture. Temperature limit not stored. |
| CCP3 Filtration: filter integrity, sediment, pore size — Association | CCP 3, raw lot | Same parameters. µm unit sits on "Filter integrity", "Pore size" has no unit. |
| CCP4 Homogenization & mixing: appearance, viscosity, color — Association | CCP 4, final batch | **Extra:** Moisture. Attached to the batch, not to the association's processing step. |
| CCP5 Pasteurization: temperature, holding time, HMF, diastase, proline — Association | CCP 5, final batch | **Extra:** Moisture. No heat-treatment flag. |
| CCP6 Settling/clarification: visual, holding time, temperature — Association | CCP 6, final batch | Same parameters. |
| CCP7 Filling: net weight — BALMER | CCP 7, final batch | **Extra:** Visual Inspection. Justification text talks about labeling. |
| CCP8 Labeling: origin, botanical source, analytical consistency — BALMER | CCP 8, final batch | Same parameters; free values, no label data. |
| CCP9 Packaging: material, cleanliness, durability — BALMER | CCP 9, final batch | Same parameters. |
| CCP10 Storage: temperature ≤ 25 °C, relative humidity — TUMA | CCP 10, final batch | Same parameters; **≤ 25 °C not stored**, no humidity limit. |

Cross-cutting differences:

- **Owners:** the design assigns each CCP to an organization (Association for 1–6, BALMER for 7–9, TUMA for 10). The system has no owner concept; only Administrators enter anything. No Association/BALMER/TUMA entities or roles exist.
- **Side split:** design CCP 1–6 are all Association processing steps; the system puts CCP 1–3 on the raw lot and CCP 4–6 on the final batch, together with BALMER/TUMA steps.
- **Limits:** the design implies acceptance criteria (e.g. ≤ 25 °C); the system stores no limits and makes no pass/fail decision.
- **Gate:** the design implies CCPs control release; the system's finalize does not check final-side CCPs.
- **Missing CCPs:** none — all 10 design CCPs exist, plus CCP 0.

---

## 7. MS SQL + Neo4j + microservices

- **MS SQL:** yes. The live app uses SQL Server through EF Core (`Microsoft.EntityFrameworkCore.SqlServer`).
- **Neo4j:** no. No Neo4j driver, Cypher, `bolt://` URI or graph DB config anywhere in the repo (live app, API template, legacy app).
- **Microservices:** no. The live app is one ASP.NET Core MVC monolith with one DbContext. `TOP4HoneyChains.API` is an unused Web API template (`WeatherForecastController` only) and the Web app makes no HTTP calls to it. No Docker, compose, gateway or message broker files. The only mentions of "microservice" are marketing text on the landing page (`Views/Home/Index.cshtml`: meta keywords and "A microservice-based open data platform …").
