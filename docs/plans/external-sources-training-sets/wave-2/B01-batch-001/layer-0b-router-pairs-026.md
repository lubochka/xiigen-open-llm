# Layer 0-B — Router Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-026-001 — FilterPanel: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-026-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-026-002 — TooltipComponent: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-026-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a tooltip component with accessible ARIA labels."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

EM-9 guard: no dependency_order, parallel_group, domain_label, or is_primary fields in output.

---

## B01-0B-026-003 — ModalDialog: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-026-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement a modal dialog with header, body, and close button."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-026-004 — "refine filter options + improve sidebar" knowledge-edge routing

```
source_id: xiigen-b01-0b-026-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Refine the filter options and improve the sidebar search flow."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Routing rule: gap vocabulary ("refine", "improve", "sidebar", "search", "flow") → no predicate fires.
Knowledge-edge path: no sub-phase assigned.
