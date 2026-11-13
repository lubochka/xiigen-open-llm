# Layer 0-B — Router Pairs (045)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-045-001 — VirtualList: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-045-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a virtual list component that renders only visible rows."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-045-002 — VirtualList: UI only routes to 1.1

```
source_id: xiigen-b01-0b-045-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design the virtual list layout including row height and visible window."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-045-003 — VirtualList: IMPLEMENT only routes to 4.1

```
source_id: xiigen-b01-0b-045-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT"]
   User request: "Add virtual scrolling to the data table to handle 10,000-row datasets."
B: DecisionCount: 1 | SubPhaseId: "4.1" | KnowledgeEdge: false

---

## B01-0B-045-004 — VirtualList: UI+IMPLEMENT+ARCHITECTURE+DOCUMENT routes to 1.1 + 4.1 + 3.1 + 2.1

```
source_id: xiigen-b01-0b-045-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
   User request: "Build virtual list component, define data pipeline API, document performance requirements, write benchmarks."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "3.1", "2.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.
