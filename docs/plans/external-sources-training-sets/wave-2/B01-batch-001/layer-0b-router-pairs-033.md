# Layer 0-B — Router Pairs (033)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-033-001 — Tooltip with spacing/color: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-033-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a tooltip component using spacing tokens and primary color from the design system."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-033-002 — Gallery document: UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-033-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Document the image gallery visual layout requirements and responsive breakpoints."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-033-003 — Pagination accessible: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-033-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement the pagination component with full keyboard accessibility support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-033-004 — Router count verification: DecisionCount always equals predicate count

```
source_id: xiigen-b01-0b-033-count-rule
source_license_state: first_party
pair_kind: router
```

Core v6-03 guard rule:
  DecisionCount == len(CompiledPredicateIds)  ← always true
  DecisionCount != 1 when 2 predicates present  ← v6-03 violation if count is collapsed
  Each RoutingDecision entry: one SubPhaseId, one PredicateId, one KnowledgeEdge value

Examples:
  1 predicate → DecisionCount: 1
  2 predicates → DecisionCount: 2
  3 predicates → DecisionCount: 3
  4 predicates → DecisionCount: 4
