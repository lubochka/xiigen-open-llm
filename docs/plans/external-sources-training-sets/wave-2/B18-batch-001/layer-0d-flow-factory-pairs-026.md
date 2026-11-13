# Layer 0-D — Flow Factory Pairs (026)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-026-001 — ForeignKeyMigration factory entry

**Scenario:** "Write a migration to add a foreign key from orders.customer_id to customers.id."

```
FlowFactoryEntry {
  ScenarioId: "ForeignKeyAddMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "fk_constraint_exists(orders_customer_id_fkey) AND count(orphan_rows) == 0"}
  ]
}
```

---

## B18-0D-026-002 — ForeignKeyMigration with ARCHITECTURE factory entry

**Scenario:** "Design the FK architecture between orders, customers, products and write the migration."

```
FlowFactoryEntry {
  ScenarioId: "ForeignKeyArchitectureMigration",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "fk_schema_design_approved == true"},
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "all_fk_constraints_applied AND count(orphan_rows) == 0"}
  ]
}
```

---

## B18-0D-026-003 — ForeignKeyConsolidation factory entry (refactoring)

**Scenario:** "Consolidate FK constraints into a consistent pattern across billing tables."

```
FlowFactoryEntry {
  ScenarioId: "ForeignKeyConsolidation",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "refactoring",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "consolidation_pattern_spec_approved == true"},
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "all_fk_naming_consistent AND data_integrity_preserved == true"}
  ]
}
```

---

## B18-0D-026-004 — ForeignKeyMigration with DOCUMENT factory entry

**Scenario:** "Add FK constraints between all related tables and document the referential integrity rules."

```
FlowFactoryEntry {
  ScenarioId: "ForeignKeyDocumentedMigration",
  Flows: ["DataMigrationFlow44", "DocumentationFlow21"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  execution_mode: "parallel",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "all_fk_constraints_applied == true"},
    {flow: "DocumentationFlow21", type: "doc_complete",
     condition: "referential_integrity_docs_published == true"}
  ]
}
```

---

## B18-0D-026-005 — ForeignKeyMigration: probationary_rejected scenario

**Scenario:** ForeignKeyMigration ran and Gate B FAILED (orphan rows found).

```
FlowFactoryEntry {
  ScenarioId: "ForeignKeyAddMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_rejected",
  RejectionReason: "count(orphan_rows) > 0 (1,247 orphan orders found)",
  RecommendedFix: "Delete or reassign orphan orders before re-running FK migration",
  RollbackStatus: "COMPLETE"
}
```
