# Layer 0-B — Router Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-023-001 — SubscriptionBilling: IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (subscription billing records, billing_history table)
- ConnectorName: SubscriptionBillingMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-023-002 — SubscriptionBilling: 4.4 + 4.3 vocabulary → both flows route

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (billing data, migrate) + 4.3 (billing API contract)

**Routing decision:**
- 4.4 vocabulary → DataMigrationFlow44
- 4.3 vocabulary (API contract) → API contract flow (4.3)
- Both sub-phases present → both flows activate

**Output:** flows: [DataMigrationFlow44, ApiContractFlow43], sub-phases: [4.4, 4.3]

---

## B18-0B-023-003 — SubscriptionBilling + TEST → DataMigrationFlow44 + UnitTestingFlow51

**Input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (billing records) + 5.x (write integration tests)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- TEST → UnitTestingFlow51, sub-phase: 5.x
- Dependency: UnitTestingFlow51 waits on DataMigrationFlow44 (tests verify migration)

**Output:** flows: [DataMigrationFlow44, UnitTestingFlow51], sub-phases: [4.4, 5.x]

---

## B18-0B-023-004 — SubscriptionBilling: restructure + billing tables → refactoring → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.4 (billing tables — database vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 database vocabulary → DataMigrationFlow44
- Not BackendServiceFlow41 (no 4.1 service vocabulary present)

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4, TransitionType: refactoring

---

## B18-0B-023-005 — SubscriptionBilling: all B18 router rules summary for 4.4 scenarios

**Summary: all sub-phase 4.4 routing combinations in B18:**

| Predicates | TransitionType | Flows |
|---|---|---|
| [IMPLEMENT] | feature_addition | DataMigrationFlow44 |
| [IMPLEMENT] | refactoring (data tables) | DataMigrationFlow44 |
| [IMPLEMENT, ARCHITECTURE] | feature_addition | ArchitectureFlow31 + DataMigrationFlow44 |
| [IMPLEMENT, SECURITY] | feature_addition | DataMigrationFlow44 + SecurityReviewFlow |
| [IMPLEMENT, TEST] | feature_addition | DataMigrationFlow44 + UnitTestingFlow51 |
| [IMPLEMENT, DOCUMENT] | feature_addition | DataMigrationFlow44 + DocumentationFlow21 |
| [IMPLEMENT, ARCHITECTURE, SECURITY] | feature_addition | ArchitectureFlow31 + DataMigrationFlow44 + SecurityReviewFlow |
| [IMPLEMENT, ARCHITECTURE, TEST] | feature_addition | ArchitectureFlow31 + DataMigrationFlow44 + UnitTestingFlow51 |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | feature_addition | All five flows |
