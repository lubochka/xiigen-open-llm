# Layer 0-C — Knowledge Edge Detection Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: AdaptiveLogicalOrchestrator — knowledge-edge result path
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: adaptive_routing_knowledge_edge

---

## Method note

Layer 0-C pairs teach the model to distinguish the two orchestrator-level
outcomes that hinge on sub-phase and flow registration status:

  Pair 001: routing decision contains KnowledgeEdge: true →
    orchestrator produces the synthetic knowledge-edge flow result
    (FlowId "knowledge-edge-missing-flow", OverallVerdict "KNOWLEDGE_EDGE").

  Pair 002: routing decision resolves to a registered flow →
    orchestrator produces a normal flow result
    (FlowId matching the registered flow, OverallVerdict "PASS_WITH_GAPS"
    for a generic nine-node flow).

The six required Metrics keys for the knowledge-edge path are:
  knowledge_edge, missing_sub_phase, missing_flow,
  routed_phase_id, routing_confidence, knowledge_edge_reason.

---

## Coverage map

| Pair ID | Routing input | Orchestrator path | OverallVerdict |
|---|---|---|---|
| O1B1-WDA-kedge-001 | KnowledgeEdge: true (no predicate match) | synthetic knowledge-edge flow | KNOWLEDGE_EDGE |
| O1B1-WDA-kedge-002 | KnowledgeEdge: false, sub-phase 3.4 registered | normal flow execution | PASS_WITH_GAPS |

---

## Pair O1B1-WDA-kedge-001 — no-predicate routes to knowledge-edge synthetic result

```
source_id: xiigen-o1-kedge-001
source_title: XIIGen O1 scenario knowledge edge — no predicate match triggers synthetic result
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
transition_type: unknown
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_knowledge_edge
node_role: knowledge-edge-detector
coverage_status: covered
pair_kind: adaptive_routing_knowledge_edge
```

A:
  run_id: "O1-kedge-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "The thing on the login page is broken again."
  routing_decisions:
    - DocumentId: "O1-router-002-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.0
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

B:
  ScenarioId: "adaptive-logical-orchestration"
  SelectedFlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: "No executable sub-phase could be selected. The request contains no high-signal development predicate. Capability learning is required before this request can be routed."
  Metrics:
    knowledge_edge: "true"
    missing_sub_phase: "unknown"
    missing_flow: "unknown"
    routed_phase_id: "unknown"
    routing_confidence: "0.0"
    knowledge_edge_reason: "no_predicate_match"
  BranchResults: []
  Summary:
    NextAction: "SCHEDULE_CAPABILITY_LEARNING"
    Detail: "The routing predicate compiler found no matching development predicate for 'The thing on the login page is broken again.' The orchestrator cannot select a sub-phase or flow without a predicate signal."

Lexicon evidence:
  "login": not in any predicate lexicon. Colloquial term, not a routable signal.
  "broken": not in any predicate lexicon.
  No predicate fires → KnowledgeEdge path triggers.

---

## Pair O1B1-WDA-kedge-002 — registered flow resolves to normal execution result

```
source_id: xiigen-o1-kedge-002
source_title: XIIGen O1 scenario knowledge edge — registered flow resolves normally
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — normal flow result path
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_knowledge_edge
node_role: knowledge-edge-detector
coverage_status: covered
pair_kind: adaptive_routing_knowledge_edge
```

A:
  run_id: "O1-kedge-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  routing_decisions:
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.65
      EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.68
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.70
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: null

B:
  ScenarioId: "adaptive-logical-orchestration"
  SelectedFlowId: "security-verification-flow-3.4"
  OverallVerdict: "PASS_WITH_GAPS"
  FinalHumanResult: "Security and authorization sub-phase 3.4 selected as primary branch. Specific SecurityVerification34Flow registered and executed. API contract (3.3) and integration test (5.1) branches executed via generic nine-node flows."
  Metrics:
    knowledge_edge: "false"
    missing_sub_phase: ""
    missing_flow: ""
    routed_phase_id: "3"
    routing_confidence: "0.92"
    knowledge_edge_reason: ""
  BranchResults:
    - BranchSubPhaseId: "3.4"
      FlowId: "security-verification-flow-3.4"
      OverallVerdict: "PASS_WITH_GAPS"
    - BranchSubPhaseId: "3.3"
      FlowId: "universal-nine-node-development-flow-3.3"
      OverallVerdict: "PASS_WITH_GAPS"
    - BranchSubPhaseId: "5.1"
      FlowId: "universal-nine-node-development-flow-5.1"
      OverallVerdict: "PASS_WITH_GAPS"
  Summary:
    NextAction: "REVIEW_BRANCH_RESULTS"
    Detail: "All three routing branches resolved to registered flows. Sub-phase 3.4 used the specific SecurityVerification34Flow. Sub-phases 3.3 and 5.1 used generic nine-node flows pending domain-specific flow registration."

Registration evidence:
  3.4: SecurityVerification34FlowAdapter registered in CreateDefault() before
    DevelopmentLifecycleTopologySeeder.Seed(); seeder skips 3.4 (hasSpecificFlow = true).
  3.3: DevelopmentLifecycleTopologySeeder registers UniversalNineNodeDevelopmentFlow
    for 3.3 as "universal-nine-node-development-flow-3.3".
  5.1: DevelopmentLifecycleTopologySeeder registers UniversalNineNodeDevelopmentFlow
    for 5.1 as "universal-nine-node-development-flow-5.1".
  KnowledgeEdge path does not fire because all three decisions have KnowledgeEdge: false
  and all three FlowIds resolve in the registry.
