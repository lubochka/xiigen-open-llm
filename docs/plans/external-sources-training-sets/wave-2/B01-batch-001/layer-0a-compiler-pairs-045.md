# Layer 0-A — Compiler Pairs (045)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-045-001 — VirtualList: "build a virtual list component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-045-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a virtual list component that renders only visible rows for large dataset performance."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build"
  "virtual list" NOT in UI lexicon; fires only via "component".
  TransitionType: feature_addition.

---

## B01-0A-045-002 — VirtualList: "design virtual list layout" fires UI only

```
source_id: xiigen-b01-0a-045-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the virtual list layout including row height calculation, visible window, and overflow handling."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.86

Predicate evidence:
  UI fires on: "layout"
  "design" NOT IMPLEMENT.
  "virtual" NOT in any lexicon.
  TransitionType: documentation.

---

## B01-0A-045-003 — VirtualList: "add virtual scrolling to data table" fires IMPLEMENT only

```
source_id: xiigen-b01-0a-045-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Add virtual scrolling to the data table to handle 10,000-row datasets without performance degradation."
  CompiledPredicateIds: ["IMPLEMENT"]
  Confidence: 0.87

Predicate evidence:
  IMPLEMENT fires on: "add" (code artifact context)
  "scrolling" NOT in any predicate lexicon.
  No UI lexicon terms in this specific request.
  Single predicate → single routing decision.
  TransitionType: feature_addition.

---

## B01-0A-045-004 — VirtualList: full predicate sweep including ARCHITECTURE

```
source_id: xiigen-b01-0a-045-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the virtual list component, define the data pipeline API, document performance requirements, and write benchmarks."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build", "write"
  ARCHITECTURE fires on: "API", "pipeline"
  DOCUMENT fires on: "document", "requirements"
  Four predicates → four independent routing decisions.
