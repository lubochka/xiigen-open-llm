# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-015-001 — SearchIndex: seed orchestration

**Scenario:** SearchIndexRebuild — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Execute seed: insert all active products into search_index
   → N08 Gate B: seeded_count == product_count AND idempotent
   → N09 output
```

---

## B18-L1-015-002 — SearchIndex: IMPLEMENT+TEST dependency

**Orchestration sequence:**
```
1. DataMigrationFlow44 — seed search_index with product data
2. UnitTestingFlow51 — run seed verification tests
3. N08: seeded_count correct AND all tests pass
4. N09 output
```

**Hard dependency:** UnitTestingFlow51 cannot run before DataMigrationFlow44 completes.
TEST always waits on IMPLEMENT (this is a B18 absolute rule).

---

## B18-L1-015-003 — SearchIndex: IMPLEMENT+ARCHITECTURE orchestration

**Orchestration sequence:**
```
1. ArchitectureFlow31 — define index structure (schema for full-text search)
2. DataMigrationFlow44 — rebuild index per schema specification
3. N08: index_schema_approved AND seeded_count correct
4. N09 output
```

---

## B18-L1-015-004 — SearchIndex: refactoring orchestration (BackendServiceFlow41)

**Scenario:** SearchIndexServiceRefactoring — IMPLEMENT, refactoring, 4.1

**Orchestration sequence:**
```
1. [BackendServiceFlow41]
   → Refactor SearchIndexService; extract ReindexingJob
   → N07 regression check: all existing search tests pass
   → N08 Gate B: all_pre_refactor_tests_pass
   → N09 output
```

---

## B18-L1-015-005 — SearchIndex: orchestration does NOT add UnitTestingFlow51 for "fixture"

**Key orchestration rule:**
- "fixture" keyword → DataMigrationFlow44 (not UnitTestingFlow51)
- "write tests" keyword → UnitTestingFlow51 added to orchestration
- "fixture for integration testing" (noun form) → only DataMigrationFlow44 runs

**Orchestrator rule:** flow list is determined by predicate list. TEST predicate required for UnitTestingFlow51 to be added. Fixture alone does not add TEST to predicate list.
