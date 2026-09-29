# Open questions for the partners

Questions we need the partners (e.g. LBTU) to answer. Each links to the finding and decision it came from.

| ID | Question | Context | Source | Status |
|---|---|---|---|---|
| PQ-01 | **Raw honey uniqueness.** A raw honey production is currently unique per apiary + production period + honey type. If two different wholesalers buy honey from the same beekeeper (same apiary, period and honey type), how should this be recorded? Should the rule include the wholesaler, or should one production be split between wholesalers? Relatedly, should a production period be restricted to certain honey types? | The duplicate check is in `RawHoneyProductionsController.Create`. There is no period↔honey-type relation in the schema. No change and no DB change in the current release (D-06). | FB-16, FB-17 | open, waiting for partner |
