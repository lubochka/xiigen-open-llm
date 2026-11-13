# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-011-001 — Canonical B18 orchestration pattern 1: simple migration

**Pattern: [IMPLEMENT] feature_addition → DataMigrationFlow44 (4.4)**

```
Orchestration:
  DataMigrationFlow44 → N08 → N09
  
  Gate B: count(target) == count(source)
  N09: migration complete; [3-5] files
  
Examples: OrderHistoryMigration, UserAccountMigration, ProductCatalogMigration (without structure)
```

---

## B18-L1-011-002 — Canonical B18 orchestration pattern 2: migration + schema

**Pattern: [IMPLEMENT, ARCHITECTURE] feature_addition → DataMigrationFlow44 + ArchitectureFlow31**

```
Orchestration:
  ArchitectureFlow31 (3.1) → [schema spec]
  DataMigrationFlow44 (4.4) → N08 → N09
  
  Sequential: ArchitectureFlow31 must complete before DataMigrationFlow44 starts
  Gate B A: schema specification complete
  Gate B B: count(target) == count(source)
  N09: migration complete; [5-7] files
  
Examples: ProductCatalogMigration, GdprUserDataMigration (partial)
```

---

## B18-L1-011-003 — Canonical B18 orchestration pattern 3: simple refactoring

**Pattern: [IMPLEMENT] refactoring → BackendServiceFlow41 (4.1)**

```
Orchestration:
  BackendServiceFlow41 → N07 (test run) → N08 → N09
  
  Gate B: count(passing_tests_after) >= count(passing_tests_before)
  N09: refactoring complete; [2-4] files
  
Examples: OrderServiceRefactoring, PaymentServiceRefactoring
```

---

## B18-L1-011-004 — Canonical B18 orchestration pattern 4: refactoring + architecture

**Pattern: [IMPLEMENT, ARCHITECTURE] refactoring → BackendServiceFlow41 + ArchitectureFlow31**

```
Orchestration:
  ArchitectureFlow31 (3.1) → [pattern spec]
  BackendServiceFlow41 (4.1) → N07 → N08 → N09
  
  Sequential: ArchitectureFlow31 before BackendServiceFlow41
  Gate B A: pattern specification complete
  Gate B B: tests pass after refactoring
  N09: refactoring complete; [3-5] files
  
Examples: OrderServicePatternRefactor (repository pattern)
```

---

## B18-L1-011-005 — Canonical B18 orchestration pattern 5: migration + tests

**Pattern: [IMPLEMENT, TEST] feature_addition → DataMigrationFlow44 + UnitTestingFlow51**

```
Orchestration:
  DataMigrationFlow44 (4.4) → N08 Gate B (row count)
  UnitTestingFlow51 (5.x) → waits for DataMigrationFlow44 → N08 Gate B (tests pass)
  
  Sequential: TEST after IMPLEMENT
  N09: migration complete with test coverage; [5-7] files
  
Examples: UserAccountMigration with tests, MultiStepMigration (tests component)
```

---

## B18-L1-011-006 — Canonical B18 orchestration pattern 6: full five-predicate

**Pattern: [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] → Five flows**

```
Orchestration:
  Phase 1: ArchitectureFlow31 (3.1) — schema design
  Phase 2: DataMigrationFlow44 (4.4) — migration
    Parallel: DocumentationFlow21 (2.1) — documentation
  Phase 3: SecurityReviewFlow — security review
    Parallel: UnitTestingFlow51 (5.x) — tests
  
  Gate B: all five flows pass their respective Gate B conditions
  N09: full GDPR-compliant migration; [10] files
  
Example: GdprUserDataMigration
```

---

## B18-L1-011-007 — Canonical pattern summary table

| Pattern | Predicates | TransitionType | Flows | Orchestration style | Files |
|---|---|---|---|---|---|
| 1 | [IMPLEMENT] | feature_addition | DataMigrationFlow44 | Sequential, single flow | 3-5 |
| 2 | [IMPLEMENT, ARCH] | feature_addition | DataMigrationFlow44 + ArchitectureFlow31 | Sequential | 5-7 |
| 3 | [IMPLEMENT] | refactoring | BackendServiceFlow41 | Sequential, single flow | 2-4 |
| 4 | [IMPLEMENT, ARCH] | refactoring | BackendServiceFlow41 + ArchitectureFlow31 | Sequential | 3-5 |
| 5 | [IMPLEMENT, TEST] | feature_addition | DataMigrationFlow44 + UnitTestingFlow51 | Sequential (TEST after) | 5-7 |
| 6 | [IMPLEMENT, ARCH, SEC, TEST, DOC] | feature_addition | Five flows | Mixed parallel/sequential | 10 |
