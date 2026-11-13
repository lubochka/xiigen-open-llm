# Layer 0-A — Compiler Pairs (050)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-050-001 — MapComponent: "build an interactive map component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-050-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build an interactive map component with pin markers, clustering, and location search input."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component", "input" (via "search input")
  IMPLEMENT fires on: "build"
  "map" NOT in UI lexicon; fires via co-occurring "component" and "input".
  TransitionType: feature_addition.

---

## B01-0A-050-002 — MapComponent: "design map view screens" fires UI only

```
source_id: xiigen-b01-0a-050-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the map view screens including location detail panel, filter sidebar, and search overlay."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87

Predicate evidence:
  UI fires on: "screen"
  "design" NOT IMPLEMENT.
  "sidebar" NOT in UI lexicon (gap — confirmed from round-2).
  "overlay" NOT in UI lexicon.
  Single predicate via "screen".
  TransitionType: documentation.

---

## B01-0A-050-003 — MapComponent: "integrate mapping API service" fires ARCHITECTURE+IMPLEMENT

```
source_id: xiigen-b01-0a-050-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Integrate the mapping API service and implement the geolocation data pipeline."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  ARCHITECTURE fires on: "API", "service", "pipeline"
  IMPLEMENT fires on: "implement"
  "integrate" is NOT in IMPLEMENT lexicon (gap verb — confirmed from round-2); ARCHITECTURE fires via "service".
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-050-004 — MapComponent: "map clustering algorithm" fires nothing

```
source_id: xiigen-b01-0a-050-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Evaluate map clustering algorithms to determine the best approach for 50,000 location pins."
  CompiledPredicateIds: []
  Confidence: 0.39

Predicate evidence:
  "evaluate" NOT in any predicate lexicon.
  "clustering" NOT in any predicate lexicon.
  "algorithm" NOT in any predicate lexicon.
  "location pins" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
