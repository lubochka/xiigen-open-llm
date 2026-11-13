# Layer 0-A — Compiler Pairs (046)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-046-001 — "RICE scoring" + "review" fires QA

A:
  run_id: "B36-0A-046-001"
  user_request: "Review the RICE scoring (Reach, Impact, Confidence, Effort) results with the product team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
  Teaching: "review" fires QA unconditionally. RICE scoring is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-046-002 — "now-next-later" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-046-002"
  user_request: "Build the now-next-later roadmap view in the project dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"
  Teaching: "build" fires IMPLEMENT. "dashboard" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-046-003 — "t-shirt sizing" + "story points" — "story" fires DOCUMENT

A:
  run_id: "B36-0A-046-003"
  user_request: "Convert t-shirt sizing estimates to story points for the backlog items."
  Task: "Identify predicates. 'story' in 'story points'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-story-point-001"
    - "gap-project-planning-backlog-001"
  Teaching: "story" fires DOCUMENT (false positive in "story points"). "convert" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-046-004 — "hypothesis" + "test" fires TEST

A:
  run_id: "B36-0A-046-004"
  user_request: "Test the hypothesis about user engagement with the new onboarding flow."
  Task: "Identify predicates. Distinguish 'test' from 'experiment'."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
  Teaching: "test" fires TEST unconditionally. "hypothesis" is gap vocabulary.
    Contrast: "experiment" without "test" → TEST does NOT fire (gap only).
    "test" + "hypothesis" → TEST fires. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-046-005 — "experiment" + "test" fires TEST (not double gap)

A:
  run_id: "B36-0A-046-005"
  user_request: "Test the experiment results to see if the feature flag improved conversion."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-feature-flag-001"
  Teaching: "test" fires TEST. "experiment" is gap vocabulary (not TEST alone).
    "feature flag" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
