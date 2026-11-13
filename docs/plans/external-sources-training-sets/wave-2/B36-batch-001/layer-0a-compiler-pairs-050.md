# Layer 0-A — Compiler Pairs (050)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-050-001 — "t-shirt sizing" + "review" fires QA

A:
  run_id: "B36-0A-050-001"
  user_request: "Review the t-shirt sizing estimates with the engineering team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
  Teaching: "review" fires QA unconditionally. "t-shirt sizing" is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-050-002 — "hypothesis" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-050-002"
  user_request: "Define the success criteria for the hypothesis before running the experiment."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
  Teaching: "define" fires DOCUMENT. "criteria" fires DOCUMENT (additive).
    "experiment" here is gap (no "test" token). DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-050-003 — "north-star metric" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-050-003"
  user_request: "Implement the north-star metric tracking in the analytics service."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
  Teaching: "implement" fires IMPLEMENT. "analytics service" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-050-004 — v6-01 guard: "PLANNING_STRATEGY" invented predicate

A:
  run_id: "B36-0A-050-004"
  user_request: "Plan the OKR strategy for next quarter."

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["PLANNING_STRATEGY"]
  Note: "request is strategic planning; PLANNING_STRATEGY predicate fires"

Why corrupt:
  "PLANNING_STRATEGY is an invented predicate that does not exist in any lexicon.
   v6-01: model invents a planning-domain predicate to avoid empty output.
   Correct output: CompiledPredicateIds: [] (zero predicates). 'plan' is neutral."

Correct B:
  CompiledPredicateIds: []
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]
  DomainId: "project-management"

---

## B36-0A-050-005 — "impact mapping" + "design" fires ARCHITECTURE

A:
  run_id: "B36-0A-050-005"
  user_request: "Design the system architecture to support the outcomes from the impact mapping."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
  Teaching: "design" fires ARCHITECTURE. "system architecture" reinforces ARCHITECTURE (additive).
    "impact mapping" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
