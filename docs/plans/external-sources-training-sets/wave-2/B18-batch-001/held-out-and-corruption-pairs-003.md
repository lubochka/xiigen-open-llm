# Held-Out and Corruption Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-003-001 — HO: "seed staging data" → IMPLEMENT, feature_addition, 4.4

**Request:** "Add seed data for the staging environment with 500 test customer accounts and their associated orders."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Reasoning:**
- "seed data" → IMPLEMENT (Rule 4), feature_addition, 4.4
- "staging environment" → 4.4 reinforcement (staging data context)
- "customer accounts", "orders" → 4.4 data vocabulary
- No refactoring verbs → NOT refactoring

---

## B18-HO-003-002 — COR: seed → 4.1 (wrong routing)

**Request:** "Add seed data for the staging environment with 500 test customer accounts."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: **4.1** ← WRONG
- Flow: **BackendServiceFlow41** ← WRONG

**Corruption type:** v6-03 type
**Corrupted reasoning:** "Seeding is a code implementation task; BackendServiceFlow41 handles all implementation"

**Why wrong:** "seed" → 4.4 (Rule 4, absolute). Seeding is a data operation, not a code operation. BackendServiceFlow41 handles code refactoring (4.1). DataMigrationFlow44 handles data seeding (4.4).

---

## B18-HO-003-003 — HO: "populate staging" → same result as "seed staging"

**Request:** "Populate the staging database with sample data for the QA team."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Note on "QA":** "QA team" is mentioned but as the DATA RECIPIENT, not as a task instruction. No QAFlow fires. QAFlow fires only on "QA" as a process verb ("perform QA on"), not as a noun ("for the QA team").

---

## B18-HO-003-004 — HO: staging fixture → 4.4

**Request:** "Create staging fixtures for the payment flow tests — include 10 payment scenarios with various card types."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Teaching:** "test" and "payment flow tests" are context nouns describing what the fixtures are for. "fixtures" is the operative noun → 4.4. TEST does NOT fire unless "write tests" is explicitly present.

---

## B18-HO-003-005 — COR: "fixture" → 5.x (wrong routing)

**Request:** "Create staging fixtures for the payment flow tests."

**Corrupted classification:**
- predicates: ["IMPLEMENT", "TEST"] ← WRONG (TEST fired on "tests" but "tests" is a reference noun, not a task verb)
- TransitionType: feature_addition
- Sub-phase: **5.x** ← WRONG for fixture part
- Flow: UnitTestingFlow51 ← WRONG

**Corruption type:** "tests" in "payment flow tests" incorrectly fires TEST predicate
**Corrupted reasoning:** "fixture for tests" → TEST fires; "tests" present → 5.x routing

**Why wrong:** TEST fires on the VERB "write tests", "add tests", "create tests" — not on the NOUN "tests". "For the [noun] tests" does not trigger TEST predicate. Fixtures are still 4.4.

---

## B18-HO-003-006 — HO: seed + write tests → both correctly

**Request:** "Add seed data for the staging database and write unit tests to verify the seeded records are correct."

**Expected classification:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 5.x
- Flows: DataMigrationFlow44 + UnitTestingFlow51
- Orchestration: DataMigrationFlow44 first; UnitTestingFlow51 after

**Teaching:** When "seed data" AND "write tests" BOTH appear explicitly → both predicates fire correctly. The distinction is whether "write tests" is explicitly stated (TEST fires) or if "tests" is just a reference noun (TEST does NOT fire).
