# Layer 0-B — Router Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-015-001 — SearchIndex: seed → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (seed, search_index, product data vocabulary)
- ConnectorName: SearchIndexRebuild

**Routing decision:**
- IMPLEMENT + feature_addition + seed/4.4 vocabulary → DataMigrationFlow44
- No secondary routing

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-015-002 — SearchIndex: fixture → DataMigrationFlow44 (not UnitTestingFlow51)

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (fixture + search_index)

**Routing decision:**
- "fixture" routes to 4.4 (DataMigrationFlow44), NOT to 5.x (UnitTestingFlow51)
- Fixture is data vocabulary, not test vocabulary
- No TEST predicate fired → no UnitTestingFlow51

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

**Teaching point:** The router does not activate UnitTestingFlow51 for "fixture". Fixture is a 4.4 data artifact. TEST predicate is required to activate UnitTestingFlow51.

---

## B18-0B-015-003 — SearchIndex + ARCHITECTURE → ArchitectureFlow31 + DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (rebuild, search index) + 3.1 (index structure)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 first (define index structure before populating it)

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44], sub-phases: [3.1, 4.4]

---

## B18-0B-015-004 — SearchIndex: refactoring → BackendServiceFlow41

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.1 (SearchIndexService, class vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.1 vocabulary → BackendServiceFlow41

**Output:** flow: BackendServiceFlow41, sub-phase: 4.1

---

## B18-0B-015-005 — SearchIndex: seed + write tests → DataMigrationFlow44 + UnitTestingFlow51

**Input:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (seed, search_index) + 5.x (write tests)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- TEST → UnitTestingFlow51, sub-phase: 5.x
- Dependency: UnitTestingFlow51 waits on DataMigrationFlow44 (tests verify seed results)

**Output:** flows: [DataMigrationFlow44, UnitTestingFlow51], sub-phases: [4.4, 5.x]
