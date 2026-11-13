# Layer 0-B — Router Pairs (044)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-044-001 — InfiniteScroll: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-044-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement infinite scroll for the product feed."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "scroll" is gap term — UI fires via screen context; IMPLEMENT fires on "implement".

---

## B01-0B-044-002 — InfiniteScroll: UI+IMPLEMENT via "component" routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-044-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a reusable infinite scroll component with loading indicator and end-of-list detection."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-044-003 — InfiniteScroll: knowledge-edge (optimize + scroll gaps)

```
source_id: xiigen-b01-0b-044-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Optimize infinite scroll performance by using virtual rendering and throttling scroll events."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

---

## B01-0B-044-004 — InfiniteScroll: IMPLEMENT+TEST routes to 4.1 + TEST.1

```
source_id: xiigen-b01-0b-044-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
   User request: "Write integration tests for the infinite scroll behavior."
B: DecisionCount: 2 | SubPhaseIds: ["4.1", "TEST.1"] | KnowledgeEdge: false (both)

Note: No UI lexicon terms in this request — UI does NOT fire.
