# Layer 0-B — Router Pairs (040)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-040-001 — ProgressBar: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-040-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a progress bar component that shows task completion percentage."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-040-002 — ProgressBar: UI only routes to 1.1

```
source_id: xiigen-b01-0b-040-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design a progress indicator with visual states for loading, partial, and complete."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-040-003 — ProgressBar: UI+IMPLEMENT+TEST routes to 1.1 + 4.1 + TEST.1

```
source_id: xiigen-b01-0b-040-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]
   User request: "Write a test suite for the progress bar component covering all state transitions."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "TEST.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.

---

## B01-0B-040-004 — ProgressBar: knowledge-edge (track gap)

```
source_id: xiigen-b01-0b-040-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Track progress across wizard steps and show percentage completion."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
