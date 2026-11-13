# Layer 0-A — Compiler Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-023-001 — SubscriptionBilling migration: "migrate" + "API" → IMPLEMENT, 4.4 (not 4.3)

**Request:** "Migrate all subscription billing records from the legacy billing_history table to the new subscriptions/billing_cycles schema."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "subscription billing records" → 4.4 sub-phase signal (data vocabulary)
- Token: "billing_history table" → 4.4 sub-phase signal (database vocabulary)
- Token: "schema" → borderline; no "structure" present → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-023-002 — SubscriptionBilling: "billing API" does NOT route to 4.3 when migration vocabulary dominates

**Request:** "Migrate billing data from the legacy billing_history table to the new billing system and update the billing API contract."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "billing data" → 4.4 sub-phase signal
- Token: "billing API" → 4.3 sub-phase signal (API vocabulary)
- Token: "API contract" → 4.3 sub-phase signal; also API_CONTRACT predicate signal
- Both 4.4 and 4.3 vocabulary present → both sub-phases fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 4.3

**Teaching point:** When both data migration vocabulary (4.4) and API vocabulary (4.3) are present, both sub-phases fire. The router activates both DataMigrationFlow44 and the API contract flow.

---

## B18-0A-023-003 — SubscriptionBilling: "billing schema structure" → ARCHITECTURE fires

**Request:** "Migrate subscription billing records and design the billing schema structure to support usage-based billing."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "subscription billing records" → 4.4 sub-phase signal
- Token: "design" → ARCHITECTURE (Rule 7)
- Token: "billing schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-023-004 — SubscriptionBilling: "restructure billing tables" → refactoring, 4.4

**Request:** "Restructure the billing tables to merge monthly and annual subscription records into a single unified subscriptions table."

**Compiler trace:**
- Token: "restructure" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "billing tables" → 4.4 sub-phase signal (database vocabulary)
- Token: "merge" → IMPLEMENT (consolidation vocabulary; refactoring)
- Token: "subscription records" → 4.4 sub-phase signal
- TransitionType: refactoring ("restructure" and "merge" are refactoring verbs)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

---

## B18-0A-023-005 — SubscriptionBilling: "write tests for migration" → TEST fires (verb form)

**Request:** "Write the SubscriptionBillingMigration and write integration tests to verify all subscription records transferred correctly."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT; feature_addition (Rule 2)
- Token: "subscription records" → 4.4 sub-phase signal
- Token: "write integration tests" → TEST fires (Rule 12 — verb form "write tests" fires TEST)
- TransitionType: feature_addition (no refactoring verb)

**Result:** predicates: ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phases: 4.4 + 5.x

**Teaching point:** "write integration tests" (verb form) fires TEST. "integration testing" (noun) does NOT fire TEST.
