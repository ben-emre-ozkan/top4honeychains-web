# Open questions for the partners

Questions we need the partners (e.g. LBTU) to answer. Each links to the finding and decision it came from.

| ID | Question | Context | Source | Status |
|---|---|---|---|---|
| PQ-01 | **Raw honey uniqueness.** A raw honey production is currently unique per apiary + production period + honey type. If two different wholesalers buy honey from the same beekeeper (same apiary, period and honey type), how should this be recorded? Should the rule include the wholesaler, or should one production be split between wholesalers? Relatedly, should a production period be restricted to certain honey types? | The duplicate check is in `RawHoneyProductionsController.Create`. There is no period↔honey-type relation in the schema. No change and no DB change in the current release (D-06). | FB-16, FB-17 | open, waiting for partner |
| PQ-02 | **CCP measurement data.** In the current data most CCP parameters hold the same value ten times, and parameters 38–42 hold exactly the values of parameters 22–26 shifted by one position (e.g. honey moisture 5.90 %, proline 3.75 mg/kg, both implausible). Are these records test data, or were values entered under the wrong parameter? Should they be corrected or removed before go-live? | Found by a read-only scan on 2026-09-29 (see the note on decimal input in `FEEDBACK.md`). Not a code issue; no data was changed. | ENG-07 scan | open, waiting for partner |
