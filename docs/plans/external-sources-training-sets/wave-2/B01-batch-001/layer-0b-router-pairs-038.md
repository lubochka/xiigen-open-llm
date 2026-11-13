# Layer 0-B — Router Pairs (038)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-038-001 — DropdownMenu: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-038-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a dropdown menu component with keyboard navigation and multi-select support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-038-002 — DropdownMenu: UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-038-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Document the dropdown menu accessibility requirements and ARIA specification."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-038-003 — DropdownMenu: knowledge-edge (refactor gap)

```
source_id: xiigen-b01-0b-038-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Refactor the dropdown selection logic to improve reusability."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

---

## B01-0B-038-004 — DropdownMenu: UI+IMPLEMENT+TEST routes to 1.1 + 4.1 + TEST.1

```
source_id: xiigen-b01-0b-038-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]
   User request: "Implement the dropdown component and write unit tests for keyboard interaction."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "TEST.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.
