# Held-Out and Corruption Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-011-001 — HO: v6-01 detection example (ARCHITECTURE suppressed)

**Request:** "Implement the normalized data structure for the legacy order migration."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Flows: DataMigrationFlow44 + ArchitectureFlow31

**v6-01 corrupted classification:**
- predicates: ["IMPLEMENT"] ← WRONG
- TransitionType: feature_addition
- Sub-phase: 4.4 only

**v6-01 corrupted reasoning:** "data structure here means a database table shape, not an architectural design"

**v6-01 detection checklist:**
1. Is "structure", "architecture", "pattern", or "design" present in the request? → YES ("structure")
2. Is ARCHITECTURE in the output predicates? → NO
3. Is there any exception in the B18 rules for the context? → NO (Rule 3: absolute)
4. **Verdict:** v6-01 corruption confirmed. ARCHITECTURE must be added.

---

## B18-HO-011-002 — HO: v6-02 detection example ("refactor" → feature_addition)

**Request:** "Refactor the OrderRepository to extract the query builder logic into a separate class."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**v6-02 corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phase: 4.1 (accidentally correct)
- Flow: BackendServiceFlow41 (accidentally correct)

**v6-02 corrupted reasoning:** "extract the query builder logic into a separate class creates a new class — this is creating a new artifact, not refactoring"

**v6-02 detection checklist:**
1. Is "refactor" present in the request? → YES
2. Does TransitionType equal "refactoring"? → NO (it's "feature_addition")
3. Is there a refactoring verb (refactor, extract, rename, consolidate, restructure, reorganize, move)? → YES
4. **Verdict:** v6-02 corruption confirmed. TransitionType must be "refactoring".

**Rule 9:** "refactor" → TransitionType: refactoring, ALWAYS. Even if a new class is created as the result, the intent is restructuring existing code.

---

## B18-HO-011-003 — HO: v6-03 detection example (4.4 routed to 4.1)

**Request:** "Migrate all product records from the legacy flat files to the new normalized database."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**v6-03 corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition (correct)
- Sub-phase: **4.1** ← WRONG
- Flow: **BackendServiceFlow41** ← WRONG

**v6-03 corrupted reasoning:** "migrate product records involves writing a migration service class — a backend service task (4.1)"

**v6-03 detection checklist:**
1. Is "migrate data/records" or "data migration" vocabulary present? → YES
2. Is the sub-phase 4.4? → NO (it's 4.1)
3. Is "data/records/table/rows/legacy flat files" present (4.4 vocabulary)? → YES
4. **Verdict:** v6-03 corruption confirmed. Sub-phase must be 4.4 / DataMigrationFlow44.

**Data vocabulary → 4.4, always:** migrate records, product data, flat files, legacy table, rows, ETL, seed, fixture.

---

## B18-HO-011-004 — HO: v6 composite corruption (v6-02 + v6-03 combined)

**Request:** "Migrate all order data from the legacy flat table to the normalized orders schema."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**v6 composite corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **refactoring** ← WRONG (v6-02 variant)
- Sub-phase: **4.1** ← WRONG (v6-03)
- Flow: **BackendServiceFlow41** ← WRONG

**v6 composite corrupted reasoning:** "migrate implies moving/reorganizing data from one location to another — reorganization = refactoring; migration service = backend service class = 4.1"

**Why wrong (both errors):**
1. v6-02 component: "migrate" is NOT in the refactoring vocabulary. Refactoring vocabulary: refactor, extract, rename, consolidate, restructure, reorganize, move (code). "migrate data" ≠ "reorganize code".
2. v6-03 component: "order data" + "legacy flat table" + "schema" = 4.4 data vocabulary. Must route to DataMigrationFlow44.
3. Two independent errors, both must be corrected.

---

## B18-HO-011-005 — HO: Clean classification (all three v6 errors absent)

**Request:** "Refactor the PaymentService to extract payment processing logic, write the OrderHistoryMigration to move 1.2 million records to the new schema structure, and add integration tests."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: refactoring (Rule 9: "refactor" wins, even with migration and tests present)
- Sub-phases: 4.1 + 4.4 + 3.1 + 5.x
- Flows: BackendServiceFlow41 + DataMigrationFlow44 + ArchitectureFlow31 + UnitTestingFlow51

**v6 check for this request:**
- v6-01 check: "structure" present → ARCHITECTURE in output? YES → v6-01 absent ✓
- v6-02 check: "refactor" present → TransitionType is "refactoring"? YES → v6-02 absent ✓
- v6-03 check: "migration" + "records" + "schema" → 4.4 in sub-phases? YES → v6-03 absent ✓

**All three v6 corruption types absent. Clean classification.**

---

## B18-HO-011-006 — Summary: v6 corruption detection table

| Corruption | Trigger | Error | Detection | Fix |
|---|---|---|---|---|
| v6-01 | "structure"/"architecture"/"pattern"/"design" present | ARCHITECTURE missing from predicates | Check absolute triggers vs output | Add ARCHITECTURE |
| v6-02 | "refactor" or refactoring verb present | TransitionType = "feature_addition" | Check refactor vocab vs TransitionType | Set TransitionType = "refactoring" |
| v6-03 | Data vocabulary present (migrate records, ETL, seed, table) | Sub-phase = 4.1 instead of 4.4 | Check data vocab vs sub-phase | Set sub-phase = 4.4 / DataMigrationFlow44 |
| v6 composite | Multiple triggers misrouted | Multiple fields wrong | Apply all three checks | Fix all errors independently |
