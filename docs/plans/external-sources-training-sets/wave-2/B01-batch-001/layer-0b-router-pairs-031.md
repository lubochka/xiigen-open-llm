# Layer 0-B — Router Pairs (031)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-031-001 — TabNavigation with design-system: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-031-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement a tab component using the design system's typography tokens and spacing scale."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-031-002 — "write UI spec for hero" → UI+IMPLEMENT+DOCUMENT routes to 1.1 + 4.1 + 2.1

```
source_id: xiigen-b01-0b-031-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
   User request: "Write the UI spec for the hero section covering visual states and interaction patterns."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "2.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.

---

## B01-0B-031-003 — "redesign review widget brand identity" → knowledge-edge

```
source_id: xiigen-b01-0b-031-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Redesign the review widget look and feel to match the new brand identity."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Gap vocabulary: "redesign", "brand", "identity", "look and feel" — none in any predicate lexicon.

---

## B01-0B-031-004 — ReviewWidget + ARCHITECTURE: UI+IMPLEMENT+ARCHITECTURE routes to 1.1 + 4.1 + 3.1

```
source_id: xiigen-b01-0b-031-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"]
   User request: "Build the review widget and define the database schema for storing review data."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "3.1"] | KnowledgeEdge: false (all)
