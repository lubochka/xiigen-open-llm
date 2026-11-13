# Layer 0-C — Knowledge-Edge Pairs (Extended)
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: AdaptiveLogicalOrchestrator — knowledge-edge result path
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Method note

B37 is a capability-gap batch. Layer 0-C is extended with additional
knowledge-edge pairs covering the full vocabulary range of project managing
terms. Layer 2 is replaced by extended capability-gap pairs because
no registered flow exists for pure project managing vocabulary.

Key distinctions B37 teaches vs B36:
  - "track" ≠ TEST (TEST requires: test, verify, assert, validate, coverage, spec, case, suite)
  - "coordinate" has no predicate match at all
  - "dependencies" fires API_CONTRACT ONLY with API co-occurrence
  - "stakeholder" (without document/requirements co-occurrence) still fires DOCUMENT
    because DOCUMENT owns the "stakeholder" term

---

## Pair B37B1-WDA-kedge-001 — pure project coordination: zero predicates

```
source_id: xiigen-b37-kedge-001
source_title: XIIGen B37 knowledge-edge — pure project coordination, no predicate
pair_kind: knowledge_edge
```

A:
  DocumentId: "B37-kedge-001-prompt"
  CompiledText: "Coordinate the delivery across three teams and track the blockers in the RAID log."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "coordinate" — not in any predicate lexicon
  "delivery" — not in any predicate lexicon
  "teams" — not in any predicate lexicon
  "track" — not in any predicate lexicon (NOT TEST)
  "blockers" — not in any predicate lexicon
  "RAID log" — not in any predicate lexicon (RAID = Risks, Assumptions, Issues,
    Dependencies; project management artifact)
  Result: CompiledPredicateIds: []

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-managing-coordinate-blockers-001"
    capability_gap_description: "Request uses project managing vocabulary (coordinate,
      track, blockers, RAID log, delivery) that has no registered predicate. The
      orchestrator cannot route to any development sub-phase. Capability learning
      is scheduled to potentially extend the predicate lexicon."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-managing-coordinate-blockers-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: coordinate, track, blockers, RAID-log"
    - "capability_gap_id: gap-project-managing-coordinate-blockers-001"
    - "no_registered_sub_phase_for_project_coordination"

---

## Pair B37B1-WDA-kedge-002 — "dependencies" boundary: API_CONTRACT fires only with API co-occurrence

```
source_id: xiigen-b37-kedge-002
source_title: XIIGen B37 knowledge-edge — dependencies boundary case
pair_kind: knowledge_edge
```

A (Part 1 — no API co-occurrence):
  DocumentId: "B37-kedge-002a-prompt"
  CompiledText: "Map the project dependencies and identify which teams are blocking the Q3 release."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis (Part 1):
  "map" — not in any predicate lexicon
  "project dependencies" — "dependencies" is NOT in API_CONTRACT here; no API
    co-occurrence present (no: api, endpoint, contract, interface, swagger,
    openapi, rest, graphql). "Project dependencies" = team relationships, not
    software contracts.
  "teams" — not in any predicate lexicon
  "blocking" — not in any predicate lexicon
  "release" — not in any predicate lexicon
  Result: CompiledPredicateIds: []

A (Part 2 — with API co-occurrence):
  DocumentId: "B37-kedge-002b-prompt"
  CompiledText: "Map the API dependencies between the payment service and the checkout module."

Predicate analysis (Part 2):
  "API" → API_CONTRACT fires on "api"
  "dependencies" → API_CONTRACT fires on "dependency" when "api" is present
  "service" → IMPLEMENT fires on "service"
  "module" → IMPLEMENT fires on "module"
  Result: CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT"]

Teaching point: "dependencies" is NOT always API_CONTRACT. "Project dependencies"
(team/timeline relationships) is gap vocabulary. "API dependencies" fires
API_CONTRACT because "api" co-occurs. The compiler is context-sensitive for
this term: "dependencies" alone = gap; "API dependencies" = API_CONTRACT.

B (Part 1):
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-managing-dependencies-team-001"
    capability_gap_description: "Request uses 'project dependencies' in a team
      coordination context. This is gap vocabulary — API_CONTRACT does not fire
      without an API co-occurrence term."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

B (Part 2):
  [Not a knowledge-edge result — normal routing proceeds]
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT"]
  SourceStatus: "ready"
  Router routes to: 3.3 (API_CONTRACT) and 4.1 (IMPLEMENT)

---

## Pair B37B1-WDA-kedge-003 — escalation and accountability: pure gap

```
source_id: xiigen-b37-kedge-003
source_title: XIIGen B37 knowledge-edge — escalation and accountability vocabulary
pair_kind: knowledge_edge
```

A:
  DocumentId: "B37-kedge-003-prompt"
  CompiledText: "Escalate the unresolved blockers to senior management and establish accountability for each action item."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "escalate" — not in any predicate lexicon
  "unresolved" — not in any predicate lexicon
  "blockers" — not in any predicate lexicon
  "senior management" — not in any predicate lexicon
  "establish" — not in IMPLEMENT lexicon (establish ≠ implement/build/code/write)
  "accountability" — not in any predicate lexicon
  "action item" — not in any predicate lexicon
  Result: CompiledPredicateIds: []

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-managing-escalation-accountability-001"
    capability_gap_description: "Request uses escalation and accountability vocabulary
      that has no registered predicate. These are organizational management terms
      that the orchestrator does not currently route."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-managing-escalation-accountability-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: escalate, blockers, accountability, action-item"
    - "capability_gap_id: gap-project-managing-escalation-accountability-001"
    - "no_registered_sub_phase_for_organizational_management"
