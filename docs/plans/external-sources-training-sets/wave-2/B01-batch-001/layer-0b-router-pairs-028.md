# Layer 0-B — Router Pairs (028)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-028-001 — UI+IMPLEMENT+ARCHITECTURE routes to 1.1 + 4.1 + 3.1

```
source_id: xiigen-b01-0b-028-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"]
   User request: "Create the product card component and define the product data service architecture."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.

---

## B01-0B-028-002 — UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-028-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Specify the review widget behavior and document the star rating visual states."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-028-003 — Four-predicate routes to 1.1 + 4.1 + 2.1 + 3.1

```
source_id: xiigen-b01-0b-028-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
   User request: "Build the pagination and modal components, document their accessibility requirements, and define the API service for loading paginated data."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "2.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.

---

## B01-0B-028-004 — EM-9 forbidden fields reminder

```
source_id: xiigen-b01-0b-028-em9
source_license_state: first_party
pair_kind: router
```

EM-9 guard: the following four fields must NEVER appear in a RoutingDecision output:
  dependency_order | parallel_group | domain_label | is_primary

Corrupt example (v6-03 violation):
  RoutingDecisions: [{ SubPhaseIds: ["1.1", "4.1"], dependency_order: 1 }]  ← WRONG (merged + forbidden field)

Correct example:
  RoutingDecisions: [{ SubPhaseId: "1.1", PredicateId: "UI" }, { SubPhaseId: "4.1", PredicateId: "IMPLEMENT" }]
