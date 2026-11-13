# Layer 0-B — Router Pairs (037)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-037-001 — AccordionPanel: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-037-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build an accordion panel component with expand and collapse animation."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-037-002 — AccordionPanel: UI only routes to 1.1

```
source_id: xiigen-b01-0b-037-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design the accordion interaction model."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-037-003 — AccordionPanel: knowledge-edge (animation gap)

```
source_id: xiigen-b01-0b-037-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Improve the accordion animation speed and transition timing."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

---

## B01-0B-037-004 — AccordionPanel: UI+IMPLEMENT+DOCUMENT+TEST routes to 1.1 + 4.1 + 2.1 + TEST.1

```
source_id: xiigen-b01-0b-037-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "TEST"]
   User request: "Build accordion component, document accessibility requirements, define API schema, write tests."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "2.1", "TEST.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.
