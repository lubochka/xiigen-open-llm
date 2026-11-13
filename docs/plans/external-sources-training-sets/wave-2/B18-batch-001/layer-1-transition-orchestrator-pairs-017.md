# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-017-001 — InvoiceHistory: IMPLEMENT+DOCUMENT parallel orchestration

**Scenario:** InvoiceHistoryMigration — IMPLEMENT+DOCUMENT, feature_addition

**Orchestration sequence:**
```
Parallel tracks (start simultaneously):
  Track A: DataMigrationFlow44 — migrate 3M invoice records
  Track B: DocumentationFlow21 — write migration runbook

Both complete → N08 evaluates both Gate B conditions → N09 output
```

**DocumentationFlow21 is parallel-eligible here** because the runbook documents the migration process (known from spec) rather than the migration results (which require completion).

---

## B18-L1-017-002 — InvoiceHistory: when DocumentationFlow21 must wait

**DocumentationFlow21 sequential (not parallel) when:**
- Documentation is "as-built" (describes final migration output)
- Documentation includes actual row counts from N09
- Documentation is a migration results report

**DocumentationFlow21 parallel when:**
- Documentation is a runbook (process documentation, can be drafted from spec)
- Documentation is a schema change document
- Documentation is archival/retention policy

**InvoiceHistory runbook → parallel eligible.**

---

## B18-L1-017-003 — InvoiceHistory + ARCHITECTURE: three-flow orchestration

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design invoices/line_items schema
2. DataMigrationFlow44 ‖ DocumentationFlow21 — migrate + document in parallel
3. N08: schema_approved AND migrated_count == source AND runbook_published
4. N09 output
```

---

## B18-L1-017-004 — InvoiceHistory: extract code + migrate data — two separate flow chains

**Scenario:** Both BackendServiceFlow41 (4.1) and DataMigrationFlow44 (4.4) activate

**Orchestration sequence:**
```
Sequential:
1. BackendServiceFlow41 — extract InvoiceService from OrderService
   (must complete before migration if migration uses new InvoiceService)
2. DataMigrationFlow44 — migrate 3M invoice records
3. N08: all service tests pass AND migrated_count == source
4. N09 output
```

---

## B18-L1-017-005 — InvoiceHistory: TransitionType through orchestration

**TransitionType carries through orchestration:**
- TransitionType: feature_addition (set at Layer 0-A)
- Persists through orchestrator
- N09 output includes TransitionType: feature_addition

**If TransitionType were refactoring** (e.g., rename columns + migrate):
- Gate B type changes from row_count_assertion to schema_change_verification (for the rename portion)
- TransitionType: refactoring is passed to N09 output
