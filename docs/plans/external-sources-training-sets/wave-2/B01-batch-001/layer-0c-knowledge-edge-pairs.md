# Layer 0-C — Knowledge-Edge Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: AdaptiveLogicalOrchestrator — knowledge-edge result path
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Method note

Layer 0-C pairs teach the orchestrator's two result paths after routing:

  Pair 001 (knowledge-edge path): CompiledPredicateIds is empty or all
    routing decisions have KnowledgeEdge: true. The orchestrator must
    produce a synthetic result with OverallVerdict: "KNOWLEDGE_EDGE",
    FlowId: "knowledge-edge-missing-flow", and 6 required Metrics keys.
    No real flow executes. A capability-learning signal is emitted.

  Pair 002 (normal registered-flow path): CompiledPredicateIds contains
    a matched predicate, and the routing decision has KnowledgeEdge: false.
    The orchestrator executes the registered flow (or its fallback) and
    produces a normal result.

The model must learn to distinguish these two paths without conflating them.

---

## Pair B01B1-WDA-kedge-001 — knowledge-edge: no UI lexicon signal

```
source_id: xiigen-b01-kedge-001
source_title: XIIGen B01 knowledge-edge — no predicate match, vague UX redesign
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
transition_type: unknown
phase: null
sub_phase: null
task_type: orchestrator-knowledge-edge
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  DocumentId: "B01-kedge-001-prompt"
  CompiledText: "We should redesign the onboarding experience to feel more modern and engaging."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match"

Predicate analysis:
  "redesign" — not in IMPLEMENT lexicon (which requires: implement, code, build,
    develop, write, create, function, class, logic, library, handler, middleware,
    controller, backend, feature, method, refactor)
  "onboarding" — not in any predicate lexicon
  "experience" — not in UI lexicon (which requires: ui, interface, ux, screen, page, ...)
  "modern", "engaging" — not in any predicate lexicon
  Result: no predicate fires → knowledge-edge

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-ux-redesign-vague-001"
    capability_gap_description: "Request uses general UX intent ('redesign', 'modern', 'engaging') without any UI lexicon terms. Cannot route to a registered sub-phase."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-ux-redesign-vague-001"]
  Warnings:
    - "knowledge_edge: no predicate matched — capability learning scheduled"
    - "capability_gap_id: gap-ux-redesign-vague-001"

Knowledge-edge rationale:
  The user intends to work on the UX but uses no terms from the UI lexicon.
  "redesign", "modern", and "engaging" are subjective quality signals with no
  registered predicate. The correct response is KNOWLEDGE_EDGE — not a guess
  at a sub-phase. Guessing 1.1 or 1.2 would be a Corruption-v6-01 error.

---

## Pair B01B1-WDA-kedge-002 — normal path: UI predicate matched, flow registered

```
source_id: xiigen-b01-kedge-002
source_title: XIIGen B01 knowledge-edge — normal registered-flow execution path
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — normal registered-flow result path
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: knowledge_edge
```

A:
  DocumentId: "B01-compiler-001-prompt"
  CompiledText: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

  RoutingDecision (from Layer 0-B):
    PredictedSubPhaseId: "1.1"
    KnowledgeEdge: false
    EvidencePredicates: ["UI"]

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "FLOW_EXECUTING"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    capability_gap_id: "null"
    capability_gap_description: "null"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-1.1"
  EvidenceIds: ["B01-compiler-001-prompt"]
  Warnings: []

Normal-path rationale:
  UI predicate fires → sub-phase 1.1 registered → orchestrator selects the
  universal nine-node development flow for 1.1 (no trusted_learned flow exists
  for 1.1 yet; the generic deterministic flow is the correct fallback).
  OverallVerdict is FLOW_EXECUTING, not KNOWLEDGE_EDGE.
  The model must learn this is the distinct opposite of the knowledge-edge path.
