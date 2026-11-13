# Layer 0-D — Flow Factory Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-023-001 — SubscriptionBilling: factory for IMPLEMENT+TEST

**Factory input:**
```
FlowFactoryInput {
  predicates: ["IMPLEMENT", "TEST"],
  sub_phases: ["4.4", "5.x"],
  transition_type: "feature_addition",
  connector_name: "SubscriptionBillingMigration",
  factory_mode: "probationary_accepted"
}
```

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "UnitTestingFlow51"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "count(subscriptions) + count(billing_cycles) == count(legacy_billing)"},
    {flow: "UnitTestingFlow51", condition: "all_billing_migration_tests_pass"}
  ],
  execution_dependency: "UnitTestingFlow51 depends_on DataMigrationFlow44"
}
```

---

## B18-0D-023-002 — SubscriptionBilling: API contract Gate B

**When 4.3 (API contract) sub-phase fires alongside 4.4:**

**API contract Gate B:**
- "API_contract_version_updated == true"
- "backward_compatible_or_versioned == true"
- "API_consumers_notified == true"

**Factory adds ApiContractFlow43 Gate B when 4.3 vocabulary present.**

---

## B18-0D-023-003 — SubscriptionBilling: restructure tables → refactoring Gate B

**Factory input:**
- transition_type: "refactoring"
- sub_phase: "4.4" (billing tables)

**Gate B for data table refactoring:**
- schema_change_verification: all DDL changes applied without data loss
- row_count_assertion: count(unified_subscriptions) == sum of merged tables

---

## B18-0D-023-004 — SubscriptionBilling: trusted_learned for standard migration

**SubscriptionBillingMigration (IMPLEMENT only, feature_addition, 4.4):**
- This is a well-known B18 pattern (migrate billing records)
- After multiple Gate B passes → trusted_learned
- Factory selects DataMigrationFlow44 directly; no probation

---

## B18-0D-023-005 — SubscriptionBilling: complete factory Gate B reference

| Predicate | Gate B type | Gate B condition example |
|---|---|---|
| IMPLEMENT (4.4 migration) | row_count_assertion | migrated == source |
| IMPLEMENT (4.4 seeding) | idempotency_check | seeded_count == expected AND idempotent |
| IMPLEMENT (4.1 refactoring) | test_regression_check | all_pre_refactor_tests_pass |
| ARCHITECTURE | schema_review_complete | schema_spec_approved |
| SECURITY | security_scan_pass | relevant security conditions true |
| TEST | test_pass_assertion | all_tests_pass |
| DOCUMENT | doc_complete | all_docs_published |
