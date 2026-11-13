# Layer 0-B — Router Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-011-001 — v6-03 corruption: 4.4 migration wrongly routed to 4.1

**Scenario:** OrderHistoryMigration routing error

**Request:** "Write the order history migration to move 2.5 million records from the legacy flat table to normalized orders/items/shipments."

**v6-03 CORRUPTED router output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Flow: **BackendServiceFlow41** ← WRONG
- Sub-phase: **4.1** ← WRONG
- v6-03 reasoning: "Migration is service/logic implementation; BackendServiceFlow41 handles implementation tasks"

**CORRECT router output:**
- Flow: **DataMigrationFlow44**
- Sub-phase: **4.4**
- Reason: vocabulary signals — "migration", "records", "table" → 4.4 signals. Data migration always routes to DataMigrationFlow44.

---

## B18-0B-011-002 — How to detect v6-03 router corruption

**Detection heuristics:**

v6-03 symptoms:
1. Request contains migration/data vocabulary (migrate, migration, records, data, table, seed, fixture, ETL)
2. Router output is BackendServiceFlow41 (4.1)
3. Sub-phase is 4.1

**Correction check:**
- If any of these vocabulary items present: migrate, migration, data, records, seed, fixture, ETL, transform (data), load (data)
- Then: DataMigrationFlow44 (4.4) must be in the routing output

**v6-03 root cause:** v6 treated all IMPLEMENT predicates as equivalent and applied BackendServiceFlow41 as the default. The v6 router did not distinguish between 4.1 and 4.4 sub-phase vocabulary.

---

## B18-0B-011-003 — v6-03 vs correct: LegacyPaymentMigration

**Request:** "Write the LegacyPaymentMigration to move payment records to the new encrypted storage format."

**v6-03 CORRUPTED:**
- Flow: BackendServiceFlow41, sub-phase: 4.1
- SECURITY: present (encrypted) → SecurityReviewFlow added
- v6 result: [BackendServiceFlow41, SecurityReviewFlow]

**CORRECT:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Correct result: [DataMigrationFlow44, SecurityReviewFlow]

**The difference:** v6-03 used BackendServiceFlow41 for IMPLEMENT; correct is DataMigrationFlow44.

---

## B18-0B-011-004 — v6-03 vs correct: seeding scenario

**Request:** "Seed the product inventory table with 1,000 SKUs."

**v6-03 CORRUPTED:**
- Flow: BackendServiceFlow41, sub-phase: 4.1
- Reasoning: "Seeding is a code implementation task handled by BackendServiceFlow41"

**CORRECT:**
- Flow: DataMigrationFlow44, sub-phase: 4.4
- Reason: "seed" → 4.4 routing (Rule 4); DataMigrationFlow44 handles seeding

---

## B18-0B-011-005 — v6-03 corruption pattern: three routing scenarios

**Three request types that v6-03 routes incorrectly to 4.1:**

1. **Data migration:**
   - Request: any "write migration" or "migrate data" request
   - v6-03: BackendServiceFlow41 (4.1)
   - Correct: DataMigrationFlow44 (4.4)

2. **Seeding:**
   - Request: any "seed" or "create fixture data" request
   - v6-03: BackendServiceFlow41 (4.1)
   - Correct: DataMigrationFlow44 (4.4)

3. **ETL pipeline:**
   - Request: any "build ETL" or "transform data" request
   - v6-03: BackendServiceFlow41 (4.1)
   - Correct: DataMigrationFlow44 (4.4)

---

## B18-0B-011-006 — B18 router v6 corruption summary table

| Corruption | Input | v6 wrong output | Correct output |
|---|---|---|---|
| v6-01 | "structure" present in request | ARCHITECTURE predicate absent | ARCHITECTURE present; ArchitectureFlow31 fires |
| v6-02 | "refactor" verb present | TransitionType: feature_addition | TransitionType: refactoring |
| v6-03 | Data migration vocabulary (migrate, seed, ETL) | Flow: BackendServiceFlow41, 4.1 | Flow: DataMigrationFlow44, 4.4 |

**All three corruptions represent failures to apply literal rules:**
- v6-01: failed literal ARCHITECTURE trigger on "structure"
- v6-02: failed TransitionType hierarchy application
- v6-03: failed sub-phase vocabulary disambiguation
