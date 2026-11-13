# Layer 3 — Teaching Unit Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-010-001 — Teaching: v6 corruption patterns for B18

**What the model must learn:** Three specific v6 corruption patterns affect B18 classification. Learn to identify and correct each.

---

## B18-TU-010-002 — Teaching: v6-01 corruption — ARCHITECTURE suppressed for "structure"

**v6-01 pattern:**
- Input contains: "structure" (or compound: "table structure", "data structure")
- v6-01 output: ARCHITECTURE NOT in predicates
- v6-01 reasoning: "structure is descriptive, not architectural"

**Correct behavior:**
- "structure" always fires ARCHITECTURE (Rule 3, absolute)
- No reasoning or context check needed
- Correction: add ARCHITECTURE to predicates; add ArchitectureFlow31 to routing

**Identification:** scan request for "structure" token. If found AND ARCHITECTURE absent → v6-01 corruption present.

**Frequency in B18:** HIGH. Many B18 migration requests include "schema structure" or "table structure".

---

## B18-TU-010-003 — Teaching: v6-02 corruption — refactor → feature_addition

**v6-02 pattern:**
- Input contains: "refactor" (or "consolidate", "rename", "restructure")
- v6-02 output: TransitionType: feature_addition
- v6-02 reasoning: "new files are created by the refactoring"

**Correct behavior:**
- "refactor" verb → TransitionType: refactoring (Rule 9, absolute)
- The fact that new files emerge does NOT change TransitionType
- Correction: set TransitionType: refactoring

**Identification:** scan request for refactoring verbs. If found AND TransitionType is feature_addition → v6-02 corruption present.

**Downstream impact:** v6-02 causes wrong flow selection (DataMigrationFlow44 instead of BackendServiceFlow41) and wrong Gate B type (row count instead of test regression).

---

## B18-TU-010-004 — Teaching: v6-03 corruption — 4.4 routed to 4.1

**v6-03 pattern:**
- Input contains: migration/data vocabulary (migrate, seed, ETL, records, database)
- v6-03 output: sub-phase 4.1, flow: BackendServiceFlow41
- v6-03 reasoning: "implementation tasks route to BackendServiceFlow41 by default"

**Correct behavior:**
- Migration/data vocabulary → sub-phase 4.4, flow: DataMigrationFlow44
- Correction: route to DataMigrationFlow44 (4.4)

**Identification:** scan for data vocabulary (migrate, seed, ETL, records). If found AND routing is BackendServiceFlow41 (4.1) → v6-03 corruption.

**Downstream impact:** v6-03 applies test_regression Gate B instead of row_count Gate B. Data migration never executed. False PASS reported (no tests fail because no data was moved).

---

## B18-TU-010-005 — Teaching: combined v6 corruption

**Worst case: all three corruptions together**

Request: "Refactor the customer data and write the migration for the normalized table structure."

**v6 combined corruption:**
1. v6-01: "structure" suppressed → ARCHITECTURE missing
2. v6-02: "refactor" → feature_addition (wrong TransitionType)
3. v6-03: "migration" + "data" → BackendServiceFlow41 (4.1) wrong

**v6 corrupt output:**
- predicates: ["IMPLEMENT"] (ARCHITECTURE missing — v6-01)
- TransitionType: feature_addition (wrong — v6-02)
- Flow: BackendServiceFlow41 (wrong — v6-03)

**Correct output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"] (structure fires ARCHITECTURE — Rule 3)
- TransitionType: refactoring ("refactor" wins — Rule 9)
- Flows: BackendServiceFlow41 (4.1, refactoring) + DataMigrationFlow44 (4.4, migration) + ArchitectureFlow31 (3.1)

---

## B18-TU-010-006 — Teaching: v6 corruption detection checklist

**Five-question corruption check for B18:**

1. Does the request contain "structure"?
   - YES and ARCHITECTURE absent → **v6-01 detected**

2. Does the request contain "refactor", "rename", "consolidate", "restructure", "reorganize", "extract" (code)?
   - YES and TransitionType is feature_addition → **v6-02 detected**

3. Does the request contain "migrate" (data), "seed", "fixture", "ETL", "transform" (data), "records"?
   - YES and flow is BackendServiceFlow41 (4.1) → **v6-03 detected**

4. Does the request contain "write/create/build migration"?
   - YES and routing is NOT DataMigrationFlow44 → **v6-03 variant detected**

5. Does the request contain "pattern", "architecture", "design" (structural)?
   - YES and ARCHITECTURE absent → **v6-01 variant detected**

Run all five checks on every B18 classification to catch corruptions.
