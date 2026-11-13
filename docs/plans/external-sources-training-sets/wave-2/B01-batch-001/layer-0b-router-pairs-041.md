# Layer 0-B — Router Pairs (041)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-041-001 — LoadingSkeleton: UI+IMPLEMENT via "component" routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-041-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a loading skeleton component to show placeholder content."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-041-002 — LoadingSkeleton: UI+IMPLEMENT via "screen" routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-041-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create skeleton screen variants for the product list, card grid, and table views."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "screen" in "skeleton screen" fires UI — "skeleton" alone does not.

---

## B01-0B-041-003 — LoadingSkeleton: knowledge-edge (optimize gap)

```
source_id: xiigen-b01-0b-041-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Optimize skeleton screen rendering performance and reduce layout shift."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

---

## B01-0B-041-004 — LoadingSkeleton: UI+IMPLEMENT+DOCUMENT routes to 1.1 + 4.1 + 2.1

```
source_id: xiigen-b01-0b-041-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
   User request: "Build the loading skeleton component and document its usage guidelines and screen variants."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "2.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.
