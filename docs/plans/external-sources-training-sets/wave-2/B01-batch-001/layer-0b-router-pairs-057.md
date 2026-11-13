# Layer 0-B — Router Pairs (057)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-057-001 — RatingStars: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-057-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a star rating component with half-star support and accessible keyboard input."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-057-002 — RatingStars: UI only routes to 1.1

```
source_id: xiigen-b01-0b-057-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design a rating display widget that shows aggregate score and distribution bars."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-057-003 — RatingStars: IMPLEMENT+ARCHITECTURE routes to 4.1 + 3.1

```
source_id: xiigen-b01-0b-057-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
   User request: "Implement the rating submission API endpoint and database schema."
B: DecisionCount: 2 | SubPhaseIds: ["4.1", "3.1"] | KnowledgeEdge: false (both)

---

## B01-0B-057-004 — RatingStars: knowledge-edge (gaming gap)

```
source_id: xiigen-b01-0b-057-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Users are gaming the rating system — investigate fake review submissions and propose mitigation."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
