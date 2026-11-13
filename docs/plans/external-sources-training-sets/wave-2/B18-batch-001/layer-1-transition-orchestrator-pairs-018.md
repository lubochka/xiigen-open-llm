# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-018-001 — TaxRecord: single migration orchestration

**Scenario:** TaxRecordMigration — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Migrate tax records from legacy_tax_history to tax_records
   → N08 Gate B: count(tax_records) == count(legacy_tax_history)
   → N09 output
```

---

## B18-L1-018-002 — TaxRecord + SECURITY: compliance orchestration

**Orchestration sequence:**
```
1. DataMigrationFlow44 — migrate tax records
2. SecurityReviewFlow — verify compliance requirements; check audit trail
3. N08: migrated_count == source AND compliance_requirements_verified
4. N09 output
```

---

## B18-L1-018-003 — TaxRecord: ETL orchestration (same as migration)

**ETL uses the same orchestration pattern as data migration:**
```
1. [DataMigrationFlow44]
   → Extract from legacy system, transform currency fields, load into tax_records
   → N08 Gate B: extracted_count == loaded_count AND transformation_errors == 0
   → N09 output
```

DataMigrationFlow44 handles both standard migration and ETL.

---

## B18-L1-018-004 — TaxRecord + ARCHITECTURE + SECURITY: three-flow ordering

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design multi-jurisdictional tax schema
2. DataMigrationFlow44 — migrate per schema spec
3. SecurityReviewFlow — verify compliance and audit trail
4. N08: all three Gate B conditions
5. N09 output
```

---

## B18-L1-018-005 — TaxRecord: rollback orchestration

**Orchestrator rollback plan:**
```
RollbackPlan {
  trigger: "N08 Gate B FAIL",
  step_1: "pause DataMigrationFlow44",
  step_2: "execute TaxRecordMigration_down.sql",
  step_3: "verify source table count restored",
  step_4: "log rollback event",
  step_5: "notify operator; do not auto-retry"
}
```

Rollback is part of every orchestration plan for migration scenarios. Gate B FAIL triggers rollback automatically.
