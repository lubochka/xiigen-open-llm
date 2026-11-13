# Layer 0-B — Router Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-001-001 — B18 router overview: two primary flows

**B18 has two primary flow types:**
1. **DataMigrationFlow44** — routes predicates with 4.4 sub-phase signal (data migration, seeding, ETL)
2. **BackendServiceFlow41** — routes predicates with 4.1 sub-phase signal (code refactoring, service restructuring)

**Router input:** compiled predicate list + TransitionType + sub-phase signals
**Router output:** flow name(s) + sub-phase assignment(s)

---

## B18-0B-001-002 — Routing decision: single IMPLEMENT (4.1) → BackendServiceFlow41

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.1 (service vocabulary present)

**Routing decision:**
- IMPLEMENT (refactoring, 4.1) → BackendServiceFlow41
- No secondary routing (single predicate, single sub-phase)

**Output:** flow: BackendServiceFlow41, sub-phase: 4.1

---

## B18-0B-001-003 — Routing decision: single IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (migration/data vocabulary present)

**Routing decision:**
- IMPLEMENT (feature_addition, 4.4) → DataMigrationFlow44
- No secondary routing

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-001-004 — Routing decision: IMPLEMENT+ARCHITECTURE → two flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (IMPLEMENT) + 3.1 (ARCHITECTURE)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1

**Output:** flows: [DataMigrationFlow44, ArchitectureFlow31], sub-phases: [4.4, 3.1]

---

## B18-0B-001-005 — B18 router flow table (all combinations)

| Predicates | TransitionType | Flows | Sub-phases |
|---|---|---|---|
| [IMPLEMENT] refactoring | refactoring | BackendServiceFlow41 | 4.1 |
| [IMPLEMENT] migration | feature_addition | DataMigrationFlow44 | 4.4 |
| [IMPLEMENT, ARCHITECTURE] | feature_addition | DataMigrationFlow44 + ArchitectureFlow31 | 4.4 + 3.1 |
| [IMPLEMENT, ARCHITECTURE] refactoring | refactoring | BackendServiceFlow41 + ArchitectureFlow31 | 4.1 + 3.1 |
| [IMPLEMENT, TEST] | feature_addition | DataMigrationFlow44 + UnitTestingFlow51 | 4.4 + 5.x |
| [IMPLEMENT, DOCUMENT] | feature_addition | DataMigrationFlow44 + DocumentationFlow21 | 4.4 + 2.1 |
| [IMPLEMENT, ARCHITECTURE, TEST] | feature_addition | DataMigrationFlow44 + ArchitectureFlow31 + UnitTestingFlow51 | 4.4 + 3.1 + 5.x |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | feature_addition | 5 flows | 5 sub-phases |

---

## B18-0B-001-006 — No GapHandler in B18 router

**B18 router does NOT include GapHandler routing.**

In B18, there is no gap vocabulary that triggers a gap-handling flow. All IMPLEMENT predicates route to either DataMigrationFlow44 (4.4) or BackendServiceFlow41 (4.1) based on sub-phase vocabulary.

Gap vocabulary in other scenarios would route to a GapHandler. In B18, this routing path is absent by design. The router will never output a GapHandler flow for B18 predicates.
