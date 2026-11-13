# Held-Out and Corruption Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-007-001 — HO: migration + ARCHITECTURE + TEST → three predicates, feature_addition

**Request:** "Write the UserAccountMigration, design the target schema using the microservice architecture pattern, and write integration tests to confirm all user records transferred correctly."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1 + 5.x
- Flows: DataMigrationFlow44 + ArchitectureFlow31 + UnitTestingFlow51

**Reasoning:**
- "write migration" → IMPLEMENT, feature_addition, 4.4
- "architecture" + "pattern" → ARCHITECTURE fires (both triggers present)
- "microservice architecture pattern" → 3.1
- "write integration tests" → TEST, 5.x
- No refactoring verbs → TransitionType: feature_addition

---

## B18-HO-007-002 — COR: ARCHITECTURE suppressed in migration + test context

**Request:** "Write the UserAccountMigration, design the target schema using the microservice architecture pattern, and write integration tests."

**Corrupted classification:**
- predicates: ["IMPLEMENT", "TEST"] ← WRONG (ARCHITECTURE missing)
- TransitionType: feature_addition
- Sub-phases: 4.4 + 5.x
- Flows: DataMigrationFlow44 + UnitTestingFlow51 (ArchitectureFlow31 missing)

**Corruption type:** v6-01 type — ARCHITECTURE suppressed in migration context
**Corrupted reasoning:** "design the target schema using microservice architecture pattern is just describing the migration approach, not requiring an architecture design step"

**Why wrong:** "architecture" fires ARCHITECTURE (Rule 7, absolute). "pattern" fires ARCHITECTURE (Rule 7, absolute). Both are present → ARCHITECTURE fires twice.

---

## B18-HO-007-003 — HO: TEST ordering constraint

**Request:** "Write the ProductCatalogMigration, design the schema structure, and add integration tests to verify the migrated product counts."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- Flows: ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

**Orchestration ordering:**
1. ArchitectureFlow31 first (schema design)
2. DataMigrationFlow44 second (migration uses schema spec)
3. UnitTestingFlow51 third (tests verify migration — cannot precede migration)

---

## B18-HO-007-004 — COR: TEST before IMPLEMENT (ordering violation)

**Corrupted classification:**
- predicates correct: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType correct: feature_addition
- Sub-phases correct: 4.4 + 3.1 + 5.x
- **WRONG ORDERING:** UnitTestingFlow51 scheduled before DataMigrationFlow44

**Why wrong:** TEST always depends on IMPLEMENT in migration scenarios. Integration tests that verify product counts require the migration to have been run first. UnitTestingFlow51 cannot precede DataMigrationFlow44.

---

## B18-HO-007-005 — HO: ARCHITECTURE fires on "design" in test request

**Request:** "Design the user migration schema and write tests to validate the migration process."

**Expected classification:**
- predicates: ["IMPLEMENT" (implicit migration), "ARCHITECTURE" ("design"), "TEST" ("write tests")]
- TransitionType: feature_addition

**Note:** "design" fires ARCHITECTURE even when the primary task context is about testing. All three predicates fire independently.

---

## B18-HO-007-006 — Summary: three-predicate held-out rules

**Three-predicate combinations in B18:**

| Predicates | Correct routing | Notes |
|---|---|---|
| [IMPLEMENT, ARCHITECTURE, TEST] | 4.4 + 3.1 + 5.x | Sequential: ARCH → IMPL → TEST |
| [IMPLEMENT, ARCHITECTURE, DOCUMENT] | 4.4 + 3.1 + 2.1 | ARCH → IMPL; DOC parallel |
| [IMPLEMENT, ARCHITECTURE, SECURITY] | 4.4 + 3.1 + security | ARCH → IMPL → SEC |
| [IMPLEMENT, TEST, DOCUMENT] | 4.4 + 5.x + 2.1 | IMPL → TEST; DOC parallel |
| [IMPLEMENT, SECURITY, TEST] | 4.4 + security + 5.x | IMPL → SEC + TEST parallel |
