# Held-Out and Corruption Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-018-001 — "capacity" + "design" (ARCHITECTURE fires)

```
held_out_reason: "Tests ARCHITECTURE fires on 'design' even in capacity-planning context"
expected_outcome: ARCHITECTURE fires
```

A:
  user_request: "Based on our team capacity for Q3, design the service architecture for the new billing module."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-capacity-001"
  Teaching: "design" + "architecture" + "service" fire ARCHITECTURE. "capacity" is gap. ARCHITECTURE fires unconditionally.

---

### Held-Out B36B1-HO-018-002 — "iteration" + "debug" (IMPLEMENT fires via debug co-occurrence)

```
held_out_reason: "Tests IMPLEMENT fires in iteration context via 'debug' + code entity"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "In this iteration, debug the payment service and fix the authentication module."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-iteration-001"
  Teaching: "debug" + "service" + "module" fire IMPLEMENT. "fix" co-occurring with code entity also fires IMPLEMENT. Iteration is gap.

---

### Corruption B36B1-COR-018-001 — v6-02: ARCHITECTURE suppressed in capacity-planning context

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Review team capacity and structure the system components for Q3."
  (Expected: QA fires on "review"; ARCHITECTURE fires on "structure" + "system")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed QA and ARCHITECTURE because the sentence 'is about capacity planning'"

Why this is corrupt:
  "'review' fires QA unconditionally. 'structure' + 'system' fires ARCHITECTURE unconditionally.
   Capacity-planning context does not suppress predicates. This is v6-02. Correct output:
   CompiledPredicateIds: ['QA', 'ARCHITECTURE']."

Correct B:
  CompiledPredicateIds: ["QA", "ARCHITECTURE"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-018-002 — v6-01: CAPACITY_PLAN predicate invented

```
corruption_type: "Corruption-v6-01: non-existent predicate invented"
```

A:
  CompiledText: "Assess team capacity and plan the sprint iterations."
  (Expected: knowledge_edge — no predicate fires)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["CAPACITY_PLAN"]
  SourceStatus: "ready"

Why this is corrupt:
  "CAPACITY_PLAN is not a registered predicate. 'assess' without a code entity does not fire.
   'plan' is not a predicate. 'capacity', 'sprint', 'iterations' are gap vocabulary.
   Zero predicates → knowledge_edge. The corrupt model invented a predicate to avoid knowledge-edge."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"
