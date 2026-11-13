# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-010-001 — TransitionType summary table for B18 (all action words → TransitionType)

**Complete TransitionType determination table for B18:**

| Action word/phrase | TransitionType | Notes |
|---|---|---|
| refactor | refactoring | Rule 1; Rule 9 (always wins) |
| extract (code) | refactoring | Rule 1; code target |
| rename | refactoring | Rule 19 |
| consolidate | refactoring | Rule 20 |
| restructure | refactoring | Rule 1 |
| reorganize | refactoring | Rule 1 |
| move (code) | refactoring | Rule 1; code target |
| clean up | refactoring | Rule 1 |
| write migration | feature_addition | Rule 2; new artifact |
| create migration | feature_addition | Rule 2; new artifact |
| build migration | feature_addition | Rule 2; new artifact |
| seed | feature_addition | Rule 4; new data |
| fixture | feature_addition | Rule 4; new data |
| migrate (data) | feature_addition | Rule 5; data move |
| transform (data) | feature_addition | Rule 5; data transformation |
| ETL | feature_addition | Rule 5; data pipeline |
| load (data) | feature_addition | Rule 5; data loading |
| document | documentation | Rule 11; DOCUMENT predicate |
| annotate | documentation | Rule 11; DOCUMENT predicate |
| write tests | (TEST predicate; no TransitionType effect) | Rule 12 |

---

## B18-L1-010-002 — TransitionType conflict: refactoring verb + feature_addition signal

**Conflict resolution rules:**

**Rule 9 wins when "refactor" is the PRIMARY verb:**
- "Refactor the OrderService" → refactoring
- "Refactor and write migration" → refactoring (Rule 9 wins)
- "Refactor the service to match the new data structure" → refactoring

**Rule 2 wins when "write migration" is the PRIMARY artifact:**
- "Write migration to consolidate tables" → feature_addition (Rule 2 — migration is the artifact)
- "Create migration for the refactored schema" → feature_addition (Rule 2)

**SchemaConsolidation case:**
- "Refactor tables AND write the consolidation migration"
- Both Rule 9 (refactor) and Rule 2 (write migration) apply
- Rule 9 wins: TransitionType: refactoring
- But BOTH sub-phases fire (4.1 + 4.4)

---

## B18-L1-010-003 — TransitionType persistence through orchestration

**TransitionType is set once by the compiler and persists through all layers:**
- Compiler: sets TransitionType
- Router: uses TransitionType to select flow
- Factory: uses TransitionType to configure flow
- Orchestrator: records TransitionType in execution plan
- N08/N09: TransitionType appears in Gate B evaluation and output

**The TransitionType does NOT change during orchestration.** If compiler sets refactoring, all downstream layers see refactoring.

---

## B18-L1-010-004 — TransitionType in nine-node flow output

**How TransitionType appears in nine-node output:**

N09 output always includes TransitionType:
```
{
  "connector_name": "...",
  "transition_type": "refactoring" | "feature_addition" | "documentation",
  ...
}
```

**TransitionType in N08 Gate B:**
- refactoring Gate B: test_regression_check
- feature_addition Gate B: row_count_assertion (for 4.4) or creation_assertion (new artifact created)
- documentation Gate B: doc_completeness

The Gate B TYPE is derived from TransitionType. This is why correct TransitionType classification (avoiding v6-02) is critical — wrong TransitionType = wrong Gate B type = wrong verification.

---

## B18-L1-010-005 — Orchestrator TransitionType validation

**Orchestrator performs TransitionType consistency check:**

Before executing flows, orchestrator validates:
1. TransitionType is consistent with flow type:
   - TransitionType: refactoring + DataMigrationFlow44 → WARNING (unusual; check if correct)
   - TransitionType: feature_addition + BackendServiceFlow41 → WARNING (unusual; check if correct)
   - TransitionType: refactoring + BackendServiceFlow41 → OK
   - TransitionType: feature_addition + DataMigrationFlow44 → OK

2. If inconsistency detected → log warning; proceed with factory selection as-is

**Note:** These inconsistencies can occur in SchemaConsolidation (refactoring TransitionType + DataMigrationFlow44 for 4.4 sub-component). The warning is informational; orchestrator does not block execution.

---

## B18-L1-010-006 — TransitionType summary: B18 scenarios

| Scenario | TransitionType | Reasoning |
|---|---|---|
| CustomerDataMigration | feature_addition | Data migration creates new artifact |
| OrderHistoryMigration | feature_addition | Migration creates new data state |
| ProductCatalogMigration | feature_addition | Migration creates new data state |
| UserAccountMigration | feature_addition | Migration creates new data state |
| LegacyPaymentMigration | feature_addition | Migration creates new data state |
| ProductInventorySeeding | feature_addition | Seed creates new data records |
| CategoryFixtureSeeding | feature_addition | Fixture creates new data records |
| ETLPipelineBuild | feature_addition | Pipeline creates new artifact |
| OrderServiceRefactoring | refactoring | Code restructuring (extract) |
| PaymentServiceRefactoring | refactoring | Code restructuring (rename/consolidate) |
| SchemaConsolidation | refactoring | Rule 9 wins (refactor verb); both sub-phases fire |
| MultiStepMigration | feature_addition | Migration creates new artifacts |
| OrderArchiveMigration | feature_addition | Migration creates new data state |
| GdprUserDataMigration | feature_addition | Migration creates new data state |
