# Layer 0-D — Flow Factory Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-017-001 — InvoiceHistory: IMPLEMENT+DOCUMENT factory output

**Factory input:**
```
FlowFactoryInput {
  predicates: ["IMPLEMENT", "DOCUMENT"],
  sub_phases: ["4.4", "2.1"],
  transition_type: "feature_addition",
  connector_name: "InvoiceHistoryMigration"
}
```

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "DocumentationFlow21"],
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "migrated_invoice_count == source_invoice_count"},
    {flow: "DocumentationFlow21", type: "doc_complete", condition: "migration_runbook_published"}
  ],
  parallel_eligible: ["DocumentationFlow21"],
  factory_mode: "probationary_accepted"
}
```

---

## B18-0D-017-002 — InvoiceHistory: DocumentationFlow21 Gate B type is doc_complete

**doc_complete Gate B:**
- Applied to DocumentationFlow21 always
- Condition: specified documentation artifacts are published/complete
- "migration_runbook_published" = runbook file exists and is marked complete
- Gate B PASS: document exists, has expected sections, and is marked complete
- Gate B FAIL: document missing, incomplete, or not published

---

## B18-0D-017-003 — InvoiceHistory: refactoring InvoiceService + migration — dual factory output

**Factory input:**
- transition_type: "refactoring"
- sub_phases: ["4.1", "4.4"]

**Dual factory output:**
```
FlowFactoryEntry {
  Flows: ["BackendServiceFlow41", "DataMigrationFlow44"],
  GateBSet: [
    {flow: "BackendServiceFlow41", type: "test_regression_check", condition: "all_invoice_service_tests_pass"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "migrated_count == source_count"}
  ]
}
```

---

## B18-0D-017-004 — InvoiceHistory: trusted_learned status

**InvoiceHistoryMigration (IMPLEMENT+DOCUMENT):**
- This combination was seen in GdprUserDataMigration and OrderArchiveMigration
- After repeated Gate B passes: trusted_learned
- Factory selects DataMigrationFlow44 + DocumentationFlow21 without probation

---

## B18-0D-017-005 — InvoiceHistory: factory schema for all IMPLEMENT+DOCUMENT combinations

**IMPLEMENT+DOCUMENT factory schema (generic):**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "DocumentationFlow21"],
  TransitionType: "feature_addition",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "migrated == source"},
    {flow: "DocumentationFlow21", type: "doc_complete", condition: "all_docs_published"}
  ],
  parallel_eligible: ["DocumentationFlow21"],
  factory_mode: "deterministic_generic" (for known IMPLEMENT+DOCUMENT pattern)
}
```
