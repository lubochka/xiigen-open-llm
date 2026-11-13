# Layer 0-B — Router Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-017-001 — InvoiceHistory: IMPLEMENT+DOCUMENT → DataMigrationFlow44 + DocumentationFlow21 (parallel)

**Input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (invoice history records) + 2.1 (document runbook)
- ConnectorName: InvoiceHistoryMigration

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- DOCUMENT → DocumentationFlow21, sub-phase: 2.1
- DocumentationFlow21 is parallel-eligible (runbook can be drafted while migration runs)

**Output:** flows: [DataMigrationFlow44, DocumentationFlow21], parallel: DocumentationFlow21, sub-phases: [4.4, 2.1]

---

## B18-0B-017-002 — InvoiceHistory: when is DOCUMENT parallel vs sequential?

**DOCUMENT parallelism rules:**

| Documentation type | Parallel with IMPLEMENT? |
|---|---|
| Runbook (process documentation) | YES — can be drafted from spec |
| Schema change document | YES — known before migration runs |
| As-built documentation | NO — must wait for IMPLEMENT to complete |
| Migration results report | NO — must wait for Gate B to pass |

---

## B18-0B-017-003 — InvoiceHistory + ARCHITECTURE → three flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 + 3.1 + 2.1

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- DOCUMENT → DocumentationFlow21, sub-phase: 2.1
- Ordering: ArchitectureFlow31 → DataMigrationFlow44 (parallel: DocumentationFlow21)

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44, DocumentationFlow21], sub-phases: [3.1, 4.4, 2.1]

---

## B18-0B-017-004 — InvoiceHistory: refactoring code vs migrating data — both fire

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signals: 4.1 (InvoiceService) + 4.4 (invoice records)

**Routing decision:**
- Both 4.1 and 4.4 vocabulary present → both flows activate
- BackendServiceFlow41 (4.1) + DataMigrationFlow44 (4.4)
- TransitionType: refactoring applies to BackendServiceFlow41 instance

**Output:** flows: [BackendServiceFlow41, DataMigrationFlow44], sub-phases: [4.1, 4.4]

---

## B18-0B-017-005 — InvoiceHistory: IMPLEMENT (4.4 only, feature_addition) summary

**Standard single-flow InvoiceHistory routing:**

- Input: ["IMPLEMENT"], feature_addition, 4.4 vocabulary only
- Output: DataMigrationFlow44, sub-phase: 4.4
- Gate B type: row_count_assertion
- Factory mode: probationary_accepted (first run) → trusted_learned (subsequent)
