# Layer 0-B — Router Pairs (050)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-050-001 — MapComponent: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-050-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build an interactive map component with pin markers, clustering, and location search input."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-050-002 — MapComponent: UI only (via "screen") routes to 1.1

```
source_id: xiigen-b01-0b-050-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design the map view screens including location detail panel, filter sidebar, and search overlay."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-050-003 — MapComponent: ARCHITECTURE+IMPLEMENT routes to 3.1 + 4.1

```
source_id: xiigen-b01-0b-050-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
   User request: "Integrate the mapping API service and implement the geolocation data pipeline."
B: DecisionCount: 2 | SubPhaseIds: ["3.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-050-004 — MapComponent: knowledge-edge (evaluate gap)

```
source_id: xiigen-b01-0b-050-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Evaluate map clustering algorithms to determine the best approach for 50,000 location pins."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
