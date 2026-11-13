# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-009-001 — v6 corruption types in B18: overview

**Three v6 corruption types affect B18 classification:**

**v6-01:** "structure" ARCHITECTURE suppressed
- Error: ARCHITECTURE predicate missing when "structure" token is present
- Root cause: contextual reasoning applied to literal rule
- Affected scenarios: any request with "table structure", "data structure", "normalized structure", etc.

**v6-02:** "refactor" classified as feature_addition
- Error: TransitionType: feature_addition when "refactor" (or other refactoring verbs) are the primary verb
- Root cause: focusing on file creation outcome rather than action verb classification
- Affected scenarios: OrderServiceRefactoring, PaymentServiceRefactoring, SchemaConsolidation

**v6-03:** 4.4 migrations wrongly routed to 4.1
- Error: DataMigrationFlow44 replaced by BackendServiceFlow41 for data migration tasks
- Root cause: treating all IMPLEMENT predicates as equivalent; using 4.1 as default
- Affected scenarios: all data migration scenarios (OrderHistoryMigration, ProductCatalogMigration, etc.)

---

## B18-0C-009-002 — v6-01 corruption: detection and correction

**v6-01 detection:**
- Check: does the request contain the token "structure"?
- If YES and ARCHITECTURE is missing from predicates → v6-01 corruption

**v6-01 correction algorithm:**
1. Scan request for token "structure" (exact match, not substring)
2. If found → add ARCHITECTURE to predicates (unconditionally)
3. Assign ARCHITECTURE to sub-phase 3.1 (default for B18)
4. Add ArchitectureFlow31 to routing output

**v6-01 example correction:**
- Input: predicates: ["IMPLEMENT"], request contains "table structure"
- Correction: predicates: ["IMPLEMENT", "ARCHITECTURE"]
- Corrected routing: [DataMigrationFlow44, ArchitectureFlow31]

---

## B18-0C-009-003 — v6-02 corruption: detection and correction

**v6-02 detection:**
- Check: does the request contain "refactor", "rename", "consolidate", "restructure", "reorganize", "clean up", "extract" (code context)?
- If YES and TransitionType is feature_addition → v6-02 corruption

**v6-02 correction algorithm:**
1. Scan request for refactoring verbs
2. If found → set TransitionType: refactoring (Rule 9)
3. Verify routing: IMPLEMENT with refactoring → BackendServiceFlow41 (not DataMigrationFlow44)
4. Exception: if "write migration" is ALSO present AS PRIMARY TASK → apply Rule 2 analysis

**v6-02 example correction:**
- Input: TransitionType: feature_addition, request contains "refactor"
- Correction: TransitionType: refactoring
- Corrected flow: BackendServiceFlow41 (not DataMigrationFlow44) if only 4.1 vocabulary present

---

## B18-0C-009-004 — v6-03 corruption: detection and correction

**v6-03 detection:**
- Check: does the request contain migration/data vocabulary (migrate, migration, seed, fixture, ETL, transform (data), load (data), records)?
- If YES and routing is BackendServiceFlow41 (4.1) → v6-03 corruption

**v6-03 correction algorithm:**
1. Scan request for 4.4 vocabulary signals
2. If found → route IMPLEMENT to DataMigrationFlow44 (4.4)
3. Remove BackendServiceFlow41 from routing IF TransitionType is feature_addition
4. Exception: if BOTH 4.1 vocabulary (service/class) AND 4.4 vocabulary (migration/seed) present → both flows fire

**v6-03 example correction:**
- Input: routing: BackendServiceFlow41 (4.1), request contains "migrate records"
- Correction: routing: DataMigrationFlow44 (4.4)

---

## B18-0C-009-005 — Compound corruption: v6-01 + v6-02 + v6-03 together

**Request:** "Refactor the customer data structure and write the consolidation migration for the records."

**v6 (all three corruptions applied):**
- v6-01: "structure" suppressed → ARCHITECTURE missing
- v6-02: "refactor" → feature_addition (wrong)
- v6-03: "write migration" + "records" → BackendServiceFlow41 (4.1) (wrong)
- v6 result: predicates: ["IMPLEMENT"], feature_addition, BackendServiceFlow41 (4.1)

**Correct result:**
- v6-01 fix: "structure" → ARCHITECTURE fires → predicates: ["IMPLEMENT", "ARCHITECTURE"]
- v6-02 fix: "refactor" → TransitionType: refactoring
- v6-03 fix: "write migration" + "records" → DataMigrationFlow44 (4.4) fires ALSO
- Correct: predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, flows: [BackendServiceFlow41, DataMigrationFlow44, ArchitectureFlow31]

**Note:** SchemaConsolidation has refactoring verbs AND migration vocabulary → both flows fire; refactoring TransitionType wins (Rule 9); ARCHITECTURE fires for "structure".

---

## B18-0C-009-006 — v6 corruption impact table for B18 scenarios

| Scenario | v6-01 risk | v6-02 risk | v6-03 risk |
|---|---|---|---|
| CustomerDataMigration | HIGH (structure likely present) | LOW (no refactoring verb) | HIGH (data migration) |
| OrderHistoryMigration | LOW (structure rarely present) | LOW | HIGH |
| ProductCatalogMigration | HIGH ("schema structure" present) | LOW | HIGH |
| OrderServiceRefactoring | LOW | HIGH (refactor verb) | LOW (code vocab) |
| PaymentServiceRefactoring | LOW | HIGH (rename/consolidate) | LOW |
| SchemaConsolidation | HIGH ("structure") | HIGH (refactor) | HIGH (migration) |
| ETLPipelineBuild | MEDIUM ("structure" sometimes) | LOW | HIGH (ETL→4.4) |
| GdprUserDataMigration | HIGH ("encrypted structure") | LOW | HIGH (data migration) |
| MultiStepMigration | HIGH ("schema structure") | LOW | HIGH |
