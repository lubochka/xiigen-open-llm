# Layer 3 — Teaching Unit Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-023-001 — Teaching: billing API vocabulary fires 4.3 alongside 4.4

**Rule:** When both 4.4 (billing records) and 4.3 (billing API contract) vocabulary appear, both sub-phases fire.

**Test:** "Migrate billing data and update the billing API contract" → sub-phases?
**Answer:** 4.4 + 4.3. Both DataMigrationFlow44 and ApiContractFlow43 activate.

**Anti-example:** Router routes to 4.4 only (ignores API contract vocabulary). Wrong: 4.3 vocabulary fires independently.

---

## B18-TU-023-002 — Teaching: "write integration tests" (verb form) fires TEST

**Rule reinforcement (critical for billing scenario):**

**Test:** "Write the SubscriptionBillingMigration and write integration tests to verify all records transferred."
**Predicates?:** ["IMPLEMENT", "TEST"], feature_addition, 4.4 + 5.x.

- "write migration" → IMPLEMENT + feature_addition (Rule 2)
- "write integration tests" → TEST fires (verb form "write tests", Rule 12)
- UnitTestingFlow51 waits on DataMigrationFlow44 (hard dependency)

---

## B18-TU-023-003 — Teaching: "restructure billing tables" → refactoring, 4.4

**Rule:** "restructure" is refactoring (Rule 1). Applied to billing tables → sub-phase 4.4 (database vocabulary).

**Test:** "Restructure billing tables to merge monthly and annual subscriptions" → TransitionType?
**Answer:** refactoring. "restructure" verb present (Rule 9-equivalent via Rule 1).

**Sub-phase:** 4.4 (tables = database vocabulary).
**Flow:** DataMigrationFlow44 (not BackendServiceFlow41 — no 4.1 code vocabulary present).

---

## B18-TU-023-004 — Teaching: TEST always waits on IMPLEMENT (absolute hard dependency)

**Rule:** UnitTestingFlow51 (TEST) always runs after DataMigrationFlow44 (IMPLEMENT) completes. This is a hard dependency — no exceptions.

**Why:** Tests verify migration results. The migration must exist before it can be verified.

**Test:** "Migrate billing records and write tests — can they run in parallel?"
**Answer:** No. DataMigrationFlow44 must complete first. UnitTestingFlow51 waits.

---

## B18-TU-023-005 — Teaching: five-predicate billing scenario

**Full five-predicate trace for subscription billing:**

**Request:** "Write the SubscriptionBillingMigration, design the billing schema structure, ensure PII encrypted, write integration tests, document the migration runbook."

**Compiler analysis:**
- "write migration" → IMPLEMENT, feature_addition
- "billing schema structure" → "structure" → ARCHITECTURE (absolute)
- "PII encrypted" → PII → SECURITY (absolute) + "encrypted" → SECURITY (absolute; confirmed)
- "write integration tests" → TEST (verb form)
- "document the migration runbook" → DOCUMENT

**Result:** ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], feature_addition, 4.4 + 3.1 + security + 5.x + 2.1

**This is the maximum predicate scenario.** Five predicates → five flows → five Gate B evaluations.
