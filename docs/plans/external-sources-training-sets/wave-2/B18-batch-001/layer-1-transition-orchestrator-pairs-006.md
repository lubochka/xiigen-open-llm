# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-006-001 — IMPLEMENT+DOCUMENT: migration + docs, parallel eligible

**Scenario:** OrderArchiveMigration with documentation

**Orchestrator input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- Flows: DataMigrationFlow44 (4.4) + DocumentationFlow21 (2.1)
- ConnectorName: OrderArchiveMigration

**Orchestration: parallel execution**

```
Parallel Track A: DataMigrationFlow44 (4.4)
  → Write OrderArchiveMigration (move orders > 7 years to archive)
  → Generate up/down migration scripts
  → N08: count(archive) == count(source orders > 7 years)

Parallel Track B: DocumentationFlow21 (2.1)
  → Document archival policy
  → Document data retention rules (7-year threshold)
  → Document field mapping (source → archive table)

Both tracks run concurrently.
N08 evaluates both when both complete.
```

---

## B18-L1-006-002 — Why IMPLEMENT+DOCUMENT is parallel eligible

**DOCUMENT can run parallel to IMPLEMENT when:**
1. Documentation is of POLICY (archival rules — known before implementation)
2. Documentation is of SCHEMA (design specification — known from ArchitectureFlow31 output)
3. Documentation is of FIELD MAPPING (field map can be written before migration executes)

**DOCUMENT must wait on IMPLEMENT when:**
1. Documentation is of ACTUAL RESULTS ("document the 5.3M rows that were archived")
2. Documentation is of DISCOVERED EDGE CASES (discovered during migration execution)
3. Documentation is "as-built" (reflects final implementation details)

**OrderArchiveMigration:** documentation is of archival policy and retention rules → known before migration runs → parallel eligible.

---

## B18-L1-006-003 — Orchestrator parallel execution management

**Orchestrator manages parallel tracks:**

```
ParallelGroup {
  tracks: [
    {
      flow: "DataMigrationFlow44",
      priority: "primary",
      gate_b: "row_count_assertion"
    },
    {
      flow: "DocumentationFlow21",
      priority: "secondary",
      gate_b: "doc_completeness"
    }
  ],
  join_condition: "both tracks complete",
  on_primary_fail: "cancel secondary track; rollback"
}
```

---

## B18-L1-006-004 — DOCUMENT gate B in B18

**DocumentationFlow21 Gate B for B18:**

| Documentation type | Gate B condition |
|---|---|
| Schema change doc | All before/after table definitions documented |
| Field mapping | All source→target field mappings documented |
| Policy documentation | All archival/retention rules documented |
| Method signature doc | All public method signatures documented |
| Transformation rules | All ETL transformation rules documented |

**Gate B for OrderArchiveMigration documentation:**
"Archival policy documented: threshold (7 years), archive table structure, retention period, access policy"

---

## B18-L1-006-005 — Migration + documentation for refactoring scenario

**Scenario:** PaymentServiceRefactoring with documentation

**Orchestration:**
```
Parallel Track A: BackendServiceFlow41 (4.1)
  → Refactor PaymentService
  → Rename methods, consolidate validation
  → N07 regression tests
  → N08 Gate B: test regression check

Parallel Track B: DocumentationFlow21 (2.1)
  → Document new method signatures
  → Document consolidated validation API
  
(Note: DOCUMENT for method signatures can run parallel because interface is known from refactoring plan)
```

**After both complete:** N09 outputs refactored code + documentation.

---

## B18-L1-006-006 — OrderArchiveMigration N09 output

**N09 output:**

```
{
  "connector_name": "OrderArchiveMigration",
  "flows_completed": ["DataMigrationFlow44", "DocumentationFlow21"],
  "execution_mode": "parallel",
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "artifacts": [
    "OrderArchiveMigration_up.sql",
    "OrderArchiveMigration_down.sql",
    "migration_log_table.sql",
    "archival_policy.md",
    "data_retention_rules.md",
    "schema_changes.md"
  ],
  "files_generated": 6,
  "n08_verdict": "PASS"
}
```
