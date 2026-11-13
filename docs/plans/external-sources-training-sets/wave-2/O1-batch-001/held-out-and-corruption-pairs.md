# Held-Out and Corruption Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs

Held-out pairs are excluded from training. They test generalization:
the model must produce correct outputs for these inputs without having
seen them in the training set.

---

### Held-Out Pair O1B1-WDA-HO-001 — rephrased JWT auth still routes to 3.4

```
source_id: xiigen-o1-held-out-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "O1-ho-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Implement OAuth 2.0 with per-tenant token scoping and RBAC on the backend API."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "3.4", EvidencePredicates: ["SECURITY", "AUTHORIZATION"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.3", EvidencePredicates: ["API_CONTRACT"], KnowledgeEdge: false

Generalization test:
  "OAuth 2.0" matches SECURITY lexicon (oauth). "RBAC" matches AUTHORIZATION lexicon.
  "API" matches API_CONTRACT. The model must recognize these terms without having
  seen this exact phrasing in the training set.

---

### Held-Out Pair O1B1-WDA-HO-002 — boundary case: "improve security" without JWT signal

```
source_id: xiigen-o1-held-out-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "O1-ho-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We should improve security across the platform."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["SECURITY"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "3.4", EvidencePredicates: ["SECURITY"], KnowledgeEdge: false

Generalization test:
  "security" is in the SECURITY lexicon. A single weak predicate still routes to 3.4.
  "improve" does not add IMPLEMENT because IMPLEMENT lexicon requires implementation-
  specific terms (implement, code, build, develop, write, backend, service, module).
  "improve security" is an ambiguous directive — the model should not over-fire predicates.

---

### Held-Out Pair O1B1-WDA-HO-003 — unambiguous test-only request routes to 5.1 only

```
source_id: xiigen-o1-held-out-003
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "O1-ho-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need unit tests for the authentication service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["TEST"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "5.1", EvidencePredicates: ["TEST"], KnowledgeEdge: false

Generalization test:
  "unit tests" matches TEST lexicon. "authentication service" names the subject under
  test but does not fire SECURITY — "authentication" by itself fires SECURITY only when
  paired with implementation vocabulary (jwt, auth, oauth). In a test-request context
  the SECURITY predicate does not fire; the request is about adding test coverage, not
  adding a security model. The model must not over-fire SECURITY from context alone.

---

## Section 2 — Corruption Pairs (v6 types)

Corruption pairs show a correct A input alongside both the correct B output and
a corrupted B output. The corruption type and harm are named explicitly.
These pairs train the model to recognize and reject corruption patterns.

Three corruption types introduced in v6 of the pipeline plan:

  Corruption-v6-01: Knowledge edge suppression
  Corruption-v6-02: Flow factory fallback suppression
  Corruption-v6-03: Multi-branch result collapse

---

### Corruption Pair O1B1-WDA-COR-001 — Corruption-v6-01: knowledge edge suppression

```
source_id: xiigen-o1-corruption-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  DocumentId: "O1-router-002-prompt"
  CompiledText: "The thing on the login page is broken again."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

B-correct:
  RoutingDecisions:
    - DocumentId: "O1-router-002-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.0
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

B-corrupt:
  RoutingDecisions:
    - DocumentId: "O1-router-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.70
      KnowledgeEdge: false
      FailureClassification: null

Corruption type: Corruption-v6-01 — knowledge edge suppression.
  The corrupted output invents a sub-phase routing decision (3.4) for an input with no
  matched predicates. It suppresses the KnowledgeEdge: true signal and fabricates a
  ready SourceStatus.

Why this is harmful:
  The orchestrator reads KnowledgeEdge: false and proceeds to execute the SecurityVerification34Flow
  for a request that contains no actionable development signal. The result is a phantom
  sub-phase execution that produces garbage design decisions, wastes compute, and — critically
  — suppresses the capability-learning signal that would trigger learning-loop resolution.
  The correct behavior is KnowledgeEdge: true → synthetic flow → SCHEDULE_CAPABILITY_LEARNING.

---

### Corruption Pair O1B1-WDA-COR-002 — Corruption-v6-02: flow factory fallback suppression

```
source_id: xiigen-o1-corruption-002
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "5.1"
  flow_registry_state:
    registered_flow_id: "unit-test-design-flow-5.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 2 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-5.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "unit-test-design-flow-5.1-candidate"
  Warnings: ["probationary_flow_rejected:unit-test-design-flow-5.1-candidate", "fallback_used:universal-nine-node-development-flow-5.1"]
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt:
  SelectedFlowId: "unit-test-design-flow-5.1-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Corruption type: Corruption-v6-02 — flow factory fallback suppression.
  The corrupted output accepts a probationary flow that has NOT passed Gate B,
  incorrectly records GateBPassed as true, suppresses the rejection warnings,
  and suppresses the fallback flag.

Why this is harmful:
  An insufficiently trained flow (2 of 9 nodes covered) is executed for production-
  adjacent routing. The missing node coverage means 7 of the 9 domain nodes will
  produce generic or incorrect outputs. The suppressed warnings prevent the learning
  pipeline from detecting the premature promotion and scheduling the missing node
  coverage work. The correct behavior is to reject the candidate and execute the
  safe deterministic generic flow with explicit warnings.

---

### Corruption Pair O1B1-WDA-COR-003 — Corruption-v6-03: multi-branch result collapse

```
source_id: xiigen-o1-corruption-003
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  DocumentId: "O1-router-001-prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "3.4", EvidencePredicates: ["SECURITY", "AUTHORIZATION"], KnowledgeEdge: false
    - PredictedSubPhaseId: "3.3", EvidencePredicates: ["API_CONTRACT"], KnowledgeEdge: false
    - PredictedSubPhaseId: "5.1", EvidencePredicates: ["TEST"], KnowledgeEdge: false
  DecisionCount: 3

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "3.4", EvidencePredicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — multi-branch result collapse.
  The corrupted output collapses all four predicates into a single routing decision
  for sub-phase 3.4, instead of producing three separate branch decisions.

Why this is harmful:
  The collapsed output assigns API_CONTRACT and TEST to sub-phase 3.4, which does not
  own those predicates. Sub-phases 3.3 (api-contract) and 5.1 (unit-tests) are never
  selected; their nine-node flows never execute; their domain learning signals are lost.
  The orchestrator's BranchResults[] will contain only one entry instead of three,
  causing multi-sub-phase coverage gaps for the JWT feature. The correct behavior is
  three independent RoutingDecision entries, each carrying only its evidence predicates.
