# Held-Out and Corruption Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-023-001 — HO: "billing API contract" fires 4.3 alongside 4.4

**Request:** "Migrate billing data from legacy_billing_history and update the billing API contract for the new schema."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 4.3
- Flows: DataMigrationFlow44 + ApiContractFlow43

**COR-023-001:** Sub-phase: 4.4 only. 4.3 not activated.
- Fix: "billing API contract" is 4.3 vocabulary (API integration update). Both 4.4 (billing data migration) and 4.3 (API contract) vocabulary present → both sub-phases fire.

---

## B18-HO-023-002 — HO: "write integration tests" (verb) fires TEST

**Request:** "Write the SubscriptionBillingMigration and write integration tests to verify all subscription records transferred."

**Correct classification:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 5.x

**COR-023-002:** Only ["IMPLEMENT"]. TEST not recognized.
- Fix: "write integration tests" is the verb form that fires TEST (Rule 12). "write" + "tests" = mandatory pattern.

---

## B18-HO-023-003 — HO: "restructure billing tables" → refactoring + 4.4

**Request:** "Restructure the billing tables to merge monthly and annual subscription records into a unified subscriptions table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.4

**COR-023-003 (v6-02):** TransitionType: feature_addition ("new unified table created").
- Fix: "restructure" is refactoring verb (Rule 1). Refactoring > feature_addition in hierarchy.

---

## B18-HO-023-004 — HO: probationary_rejected → deterministic_generic fallback

**Scenario:** SubscriptionBillingMigration Gate B fails (migrated_count != source_count).

**Correct factory behavior:**
- N08 verdict: FAIL
- Factory mode: probationary_rejected
- Next run: deterministic_generic (fallback)
- N09 output includes rejection_reason

**COR-023-004:** Factory retries with probationary_accepted on next run.
- Fix: After Gate B FAIL → factory_mode is probationary_rejected, NOT probationary_accepted. Next run uses deterministic_generic fallback, not another probationary attempt.

---

## B18-HO-023-005 — HO: "billing schema structure" fires ARCHITECTURE

**Request:** "Migrate subscription billing records and design the billing schema structure to support usage-based pricing."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-023-005 (v6-01):** ARCHITECTURE absent.
- Fix: "billing schema structure" contains "structure" → ARCHITECTURE fires absolutely. Billing/financial context does not suppress the "structure" absolute edge.
