# Layer 0-B — Router Pairs (043)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-043-001 — ErrorBoundary: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-043-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement a React error boundary component that catches runtime errors and displays a fallback UI."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-043-002 — ErrorBoundary: UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-043-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Document the error boundary component behavior, fallback screen requirements, and recovery flow."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-043-003 — ErrorBoundary: IMPLEMENT only routes to 4.1

```
source_id: xiigen-b01-0b-043-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT"]
   User request: "Add error boundary wrappers to all production routes and service modules."
B: DecisionCount: 1 | SubPhaseId: "4.1" | KnowledgeEdge: false

Note: "add" fires IMPLEMENT; no UI lexicon terms present → IMPLEMENT only.

---

## B01-0B-043-004 — ErrorBoundary: UI+IMPLEMENT+DOCUMENT+ARCHITECTURE routes to 1.1 + 4.1 + 2.1 + 3.1

```
source_id: xiigen-b01-0b-043-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
   User request: "Build error boundary components, write tests, document error recovery requirements, define logging API schema."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "2.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.
