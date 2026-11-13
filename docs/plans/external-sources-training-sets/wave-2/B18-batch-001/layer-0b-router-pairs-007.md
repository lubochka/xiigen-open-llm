# Layer 0-B — Router Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-007-001 — [IMPLEMENT, ARCHITECTURE, TEST] → 4.4 + 3.1 + 5.x

**Scenario:** MultiStepMigration (migration + design + tests)

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- Vocabulary: "write migration" → 4.4; "design schema structure" → 3.1; "write integration tests" → 5.x
- Request: "Write the user account migration and design the target schema structure, then write integration tests to verify row counts."

**Routing logic:**
1. IMPLEMENT (feature_addition, 4.4) → DataMigrationFlow44
2. ARCHITECTURE ("design", "structure") → ArchitectureFlow31
3. TEST ("write integration tests") → UnitTestingFlow51

**Router output:**
- Flows: [DataMigrationFlow44, ArchitectureFlow31, UnitTestingFlow51]
- Sub-phases: [4.4, 3.1, 5.x]

**Ordering constraint:** TEST (UnitTestingFlow51) waits on IMPLEMENT (DataMigrationFlow44) completion. ARCHITECTURE (ArchitectureFlow31) can run parallel to or before IMPLEMENT.

---

## B18-0B-007-002 — TEST routing sub-phase: 5.x

**TEST predicate routing in B18:**
- "write unit tests" → UnitTestingFlow51, sub-phase 5.1
- "write integration tests" → UnitTestingFlow51, sub-phase 5.2 (or 5.x generic)
- "add tests" → UnitTestingFlow51, sub-phase 5.x

**TEST does NOT route to:**
- 4.4 (DataMigrationFlow44) — even if tests verify data migrations
- 4.1 (BackendServiceFlow41) — even if tests verify refactored services
- 2.1 (DocumentationFlow21) — test code is not documentation

---

## B18-0B-007-003 — MultiStepMigration: full router trace

**ConnectorName:** MultiStepMigration

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4, 3.1, 5.x

**Router sequence:**
1. IMPLEMENT → DataMigrationFlow44 (4.4) — primary flow
2. ARCHITECTURE → ArchitectureFlow31 (3.1) — concurrent flow
3. TEST → UnitTestingFlow51 (5.x) — dependent flow (waits on IMPLEMENT)

**Final routing output:**
```
{
  "primary_flow": "DataMigrationFlow44",
  "secondary_flows": ["ArchitectureFlow31", "UnitTestingFlow51"],
  "sub_phases": ["4.4", "3.1", "5.x"],
  "execution_order": {
    "parallel": ["DataMigrationFlow44", "ArchitectureFlow31"],
    "sequential_after_primary": ["UnitTestingFlow51"]
  },
  "connector_name": "MultiStepMigration",
  "transition_type": "feature_addition"
}
```

---

## B18-0B-007-004 — TEST dependency on IMPLEMENT in migration scenarios

**Rule:** TEST always waits on IMPLEMENT in migration scenarios.

**Reason:** Integration tests that verify migration row counts, data integrity, or transformation correctness can only run after the migration has been written and can be executed. You cannot test a migration before it exists.

**Contrast with ARCHITECTURE:** ARCHITECTURE (schema design) can happen before IMPLEMENT (writing the migration). Design precedes implementation.

**Execution order for [IMPLEMENT, ARCHITECTURE, TEST]:**
1. ARCHITECTURE (parallel or before IMPLEMENT — design first)
2. IMPLEMENT (write migration)
3. TEST (test the migration — must come after)

---

## B18-0B-007-005 — IMPLEMENT+TEST without ARCHITECTURE

**Request:** "Write the UserAccountMigration and create integration tests to confirm all 50,000 user records transferred correctly."

**Router input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Vocabulary: "migration" → 4.4; "create integration tests" → 5.x
- No "structure", "pattern", "architecture", "design" → ARCHITECTURE not present

**Router output:**
- IMPLEMENT → DataMigrationFlow44, 4.4
- TEST → UnitTestingFlow51, 5.x
- Flows: [DataMigrationFlow44, UnitTestingFlow51]

---

## B18-0B-007-006 — Routing with "then" ordering signal

**Request:** "Write the data migration for order history, then add tests to verify the migrated row counts match the source."

**Router input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- "then" → temporal dependency signal (TEST after IMPLEMENT)

**Router output:**
- IMPLEMENT → DataMigrationFlow44, 4.4
- TEST → UnitTestingFlow51, 5.x (explicit sequential ordering from "then")
- Execution: DataMigrationFlow44 first; UnitTestingFlow51 second

**Flows:** [DataMigrationFlow44, UnitTestingFlow51]
**Ordering:** sequential (not parallel)
