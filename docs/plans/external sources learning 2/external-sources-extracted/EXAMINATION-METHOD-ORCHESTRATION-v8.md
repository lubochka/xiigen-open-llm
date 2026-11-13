# Document Examination to Training Sets — Orchestration Edition
# Version: 8.0 — 2026-05-08
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v7.md

---

## What changed from v7

**DPO pair quality rules added as a first-class section.** Data from calibration
run 2026-05-06 showed that 75% of batches had negative reward margin — the model
assigned a higher score to rejected than to chosen. The root cause was not missing
sources. It was the structure of rejected responses. Every rejected template that
says "Do not generate a capability for sub-phase X" is now explicitly forbidden.
The rules for constructing valid rejected responses are part of this document.

**ARCH and PLAN domains are the priority target.** Code sub-phases (4.x) are
performing adequately. ARCH had zero training batches in the calibration run.
PLAN had only two of six batches with positive reward margin. All pair writing
priorities and the Stage 9 corruption case requirements reflect this.

---

## Canonical Pattern Dependency

This document uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the authority
for `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, recursive composition,
and array output.

This document does not define new component kinds. It defines how sources become
training rows for those canonical contracts and how rejected responses must be
constructed for each layer.

---

## Full Ordered Path

```
human prompt
-> PromptRoutingInputCompilerUnit     [Layer 0-A]
-> MultiSubPhaseRouterUnit            [Layer 0-B]
-> knowledge-edge detection           [Layer 0-C]
-> flow factory selection             [Layer 0-D]
-> LogicalTransitionOrchestrator      [Layer 1]
-> selected A/B/X transition
-> DomainSplitterUnit                 [Layer 2, Node 01]
-> DomainTranslatorUnit               [Layer 2, Node 02]
-> SchemePlannerUnit                  [Layer 2, Node 03]
-> DomainProcessorUnit                [Layer 2, Node 04]
-> ApplierUnit                        [Layer 2, Node 05]
-> ResultTranslatorUnit               [Layer 2, Node 06]
-> ArbiterSelectorUnit                [Layer 2, Node 07]
-> ArbiterUnit                        [Layer 2, Node 08]
-> FormatterUnit                      [Layer 2, Node 09]
-> teaching and retraining flows      [Layer 3]
```

Layer 0 must be produced before Layer 1 for any batch targeting the adaptive
orchestrator. For batches targeting only the nine-node flow or teaching flow,
Layer 0 is not required — but if a scenario has been used in any adaptive routing
batch, Layer 0 pairs must exist for all subsequent batches using that scenario.

---

## Universal Pair Rule

Each pair has this shape:

```
Contract:
  <interface signature>

A:
  the exact runtime input object

B:
  the exact runtime output object
```

B must be the output itself, written in full. B must not say what the output
should contain.

---

## DPO Pair Quality Rules — Universal

These rules apply to every pair produced from every layer. They override any
layer-specific guidance where there is a conflict.

### The forbidden rejected template

This template is forbidden in all batches:

```
"Do not generate a capability for sub-phase X."
```

This template shares tokens with the chosen response and gives the model no
signal. It was the cause of negative reward margin in 29 of 40 batches in
calibration run 2026-05-06.

### Three valid rejected types

**Type A — Plausible adjacent sub-phase.**
The rejected response addresses a different sub-phase that is semantically close
to the correct one. It contains real technical content and a coherent argument
but it addresses the wrong responsibility.

Use Type A when two sub-phases share vocabulary but own different decisions. The
most common confusion pairs for ARCH are listed in the reference table below.

**Type B — Correct sub-phases, wrong dependency order.**
The rejected response names the same sub-phases as the chosen response but
sequences them in the wrong order or omits a dependency constraint.

Use Type B for all PLAN pairs where the prompt contains multiple tasks. The
dependency ordering table at the end of this document lists the correct sequences
and the natural wrong orders that make plausible rejected responses.

**Type C — Wrong domain for the prompt's verb.**
The rejected response addresses the wrong domain based on a misreading of the
prompt's intent signal. The intent verb determines the domain:

```
"explain / how does X work"  → CODE_Q
"X throws / X fails"         → BUG
"implement / add / build"    → PLAN or IMPLEMENT
"design / define boundaries" → ARCH
"review / audit"             → QA or ARCH review
```

### Required annotation

Every rejected response must end with a [WRONG: ...] block naming the specific
confusion:

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow,
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

A generic "this is incorrect" is not an acceptable annotation.

### One prompt per pair per batch

A prompt must not appear as chosen in one pair and as rejected in another within
the same batch.

### Batch composition minimums

| Pair type | Minimum fraction |
|---|---|
| Type A (adjacent sub-phase confusion) | 40% of pairs in the batch |
| Type B (wrong dependency order) | 20% — required when prompt contains multiple tasks |
| Type C (wrong domain) | 30% — required for every domain in the batch |
| Stage 9 corruption rows | At least one of each corruption kind per batch |

---

## ARCH sub-phase confusion reference table

Use this table to select a plausible rejected for every ARCH pair.

| Chosen sub-phase | What it owns | Most plausible rejected | Why it is wrong |
|---|---|---|---|
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.1 Backend Services | Writes the class instead of the boundary rule |
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.4 Refactoring | Moves code instead of deciding ownership |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.1 Backend Services | Writes the repository instead of the model |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.5 Integration | Writes the adapter instead of the boundary |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 4.1 Backend Services | Wires the logger instead of defining the strategy |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 7.3 Monitoring and Feedback Loop | Configures the dashboard before signals are designed |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.1 Unit Test Design | Automates instead of defining human acceptance |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.3 End-to-End Scenario Testing | Automates the user journey instead of a human checklist |

---

## PLAN dependency ordering reference table

Use this table to construct Type B rejected responses for PLAN pairs.

| Correct sequence | Natural wrong order for rejected |
|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 as afterthought documentation |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model derived from contract |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mock tokens, 3.4 added after |
| 3.1 + 3.4 in parallel → 4.1 after both | All three in parallel, no dependency stated |
| 2.2 Acceptance Criteria → 5.1 Unit Tests | 5.1 first, criteria inferred from passing tests |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 configuration first, design retroactively |
| 3.3 API Contract → 4.5 Integration Connectors | 4.5 first, contract written to match implementation |

---

## Layer 0 — Adaptive Orchestrator Infrastructure Pairs

Layer 0 covers four components unique to the `AdaptiveLogicalOrchestrator`. None
appear in the nine-node flow or transition orchestrator.

---

### Layer 0-A — Prompt Routing Input Compiler

```
PromptRoutingInputCompilerUnit
    : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

**What this unit does:** Converts the raw orchestrator request into a structured
`SubPhaseRouterSourceDocument` that the router can score. It extracts predicate
IDs from the nine-predicate lexicon. It does not resolve ambiguity — it faithfully
extracts what is present.

**B field names** (use these exactly — no invented fields):

```
DocumentId             — "{run_id}-prompt"
SourcePath             — "logical-orchestrator://user-prompt"
SourceTier             — "runtime_prompt"
LicenseOrAuthority     — "user_request"
DocumentType           — "user_prompt"
DocumentTitle          — "Runtime user prompt"
CompiledText           — user_request string trimmed
CompiledPredicateIds   — matched predicate IDs from the lexicon
SourceStatus           — "ready" when predicates found; "knowledge_edge: ..." when none
KnownFailureMode       — "runtime_prompt_compiled"
```

**Predicate lexicon:**

```
SECURITY      — jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION — authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  — api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  — architecture, component, topology, boundary, dependency, service
IMPLEMENT     — "implement the", "implementation plan", connector, provider, "integration layer", "code change"
TEST          — test, testing, "integration test", "unit test", coverage, fixture
QA            — qa, quality, release, acceptance, verification
UI            — ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      — document, documentation, spec, specification, requirements
```

Confidence is 0.92 when at least one predicate matches. 0.70 when none match.

**Example — multi-predicate prompt:**

```
Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-001"
  scenario_id: "O1"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "O1-router-001-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  CompiledPredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    Confidence: 0.92
    PredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
```

**Example — no-predicate prompt:**

```
Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-002"
  user_request: "The 403 is wrong on the admin endpoint."

B:
  DocumentId: "O1-router-002-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "The 403 is wrong on the admin endpoint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    Confidence: 0.70
    PredicateIds: []
    Warnings: ["no_high_signal_predicate"]
```

**DPO pairs for this layer:**

Chosen: correct predicate extraction for the prompt.
Rejected (Type A): plausible wrong predicate set — for example, assigning
SECURITY to a prompt that mentions "authentication" only as an adjective while
the primary verb is "test coverage" (correct: TEST + SECURITY; rejected: SECURITY
only). Use the wrong-predicate-assignment corruption pattern.

---

### Layer 0-B — Multi-Sub-Phase Router

```
MultiSubPhaseRouterUnit
    : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

**What this unit does:** Receives the compiled source document and returns an
ordered array of routing decisions — one per identified sub-phase. The A for
every router row comes directly from the B of the corresponding compiler row.
Do not invent router A shapes.

**B field names per decision** (use these exactly):

```
DocumentId                — carried from the source document
SourceStatus              — "predicted" or "knowledge_edge"
PredictedPhaseId          — phase identifier e.g. "3"
PredictedSubPhaseId       — sub-phase identifier e.g. "3.4"
Confidence                — routing confidence 0.0 to 1.0
NearestWrongSubPhaseScore — score of closest non-selected sub-phase
EvidencePredicates        — predicate IDs that grounded this decision
KnowledgeEdge             — true when confidence below floor
FailureClassification     — "none" for clean routes
```

Dependency ordering between branches is **not a field on SubPhaseRoutingDecision**.
The orchestrator reads `ILogicalTopologyRegistry.ListTransitions()` at runtime
to determine dependency order. Do not add `dependency_order`, `parallel_group`,
`domain_label`, or `is_primary` to router B sides.

**Example — clean multi-route:**

```
Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>

A:
  DocumentId: "O1-router-001-prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  CompiledPredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
  SourceStatus: "ready"

B:
  routing_decisions:
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.91
      NearestWrongSubPhaseScore: 0.52
      EvidencePredicates: ["AUTHORIZATION", "SECURITY"]
      KnowledgeEdge: false
      FailureClassification: "none"
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.78
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "none"
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.48
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "none"
```

**DPO pairs for this layer:**

Chosen: all matching sub-phases routed correctly.
Rejected (Type A): routes only one sub-phase when the predicates support two —
the ambiguous-predicate-case corruption pattern. [WRONG: API_CONTRACT predicate
from "API endpoint" is present. When multiple predicates match, all sub-phases
must be routed.]

---

### Layer 0-C — Knowledge Edge Detection

This is not a separate unit. It is the pair shape for the output the
`AdaptiveLogicalOrchestrator` produces when a routed sub-phase has no registered
flow.

**The gap branch shape** (all six metrics required):

```
FlowId:          "knowledge-edge-missing-flow"
ScenarioId:      <the request ScenarioId>
OverallVerdict:  "KNOWLEDGE_EDGE"
FinalHumanResult: "Knowledge edge: routed to sub-phase '<id>', but <reason>."
Metrics:
  knowledge_edge:        "True"
  missing_sub_phase:     <PredictedSubPhaseId>
  missing_flow:          "True"
  routed_phase_id:       <PredictedPhaseId>
  routing_confidence:    <Confidence as 2-decimal string e.g. "0.83">
  knowledge_edge_reason: "no logical flow registered for routed sub-phase"
EvidenceIds: <EvidencePredicates from routing decision>
Warnings:    ["knowledge_edge"]
```

Every knowledge-edge pair must be accompanied in the same batch by at least one
comparison pair where the same prompt routes to a registered sub-phase (3.4) and
produces a PASS result. The model must see both cases to learn the distinction.

**Example — gap pair:**

```
Contract:
  AdaptiveLogicalOrchestrator : ILogicalOrchestrator
  pair_kind: knowledge_edge_detection

A:
  run_id: "O1-gap-001"
  user_request: "We need to document the API for external partners."
  flags:
    UseKnowledgeEdgeDetection: true

B:
  ScenarioId: "adaptive-logical-orchestration"
  SelectedFlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  BranchResults:
    - FlowId: "knowledge-edge-missing-flow"
      OverallVerdict: "KNOWLEDGE_EDGE"
      FinalHumanResult: "Knowledge edge: routed to sub-phase '3.3', but no logical flow registered for routed sub-phase."
      Metrics:
        knowledge_edge: "True"
        missing_sub_phase: "3.3"
        missing_flow: "True"
        routed_phase_id: "3"
        routing_confidence: "0.83"
        knowledge_edge_reason: "no logical flow registered for routed sub-phase"
      EvidenceIds: ["API_CONTRACT"]
      Warnings: ["knowledge_edge"]
```

**DPO pairs for this layer:**

Chosen: KNOWLEDGE_EDGE result with all six metrics present.
Rejected (knowledge-edge-suppressed corruption): PASS result returned for the
same prompt, suppressing the knowledge edge. [WRONG: when no flow is registered
for a routed sub-phase, the result must be KNOWLEDGE_EDGE. Returning PASS
fabricates a result the system cannot actually produce.]

---

### Layer 0-D — Flow Factory Selection

```
ILogicalFlowFactory<TInput>.Create(LogicalFlowFactoryRequest) -> ILogicalFlow<TInput>
```

Every batch must include all four trust-state cases:

1. `UseLearnedUnitWhenAvailable: true` + Trusted learned unit → learned unit selected
2. `UseLearnedUnitWhenAvailable: false` + Trusted learned unit → deterministic unit selected
3. `UseLearnedUnitWhenAvailable: true` + only Rejected learned unit → deterministic fallback
4. `UseLearnedUnitWhenAvailable: true` + Probationary learned unit + minimum trust Probationary → Probationary selected

**DPO pairs for this layer:**

Chosen: correct unit selection for the trust state and flag combination.
Rejected (Type A): wrong unit selected — for example, selecting a Rejected unit
when a deterministic unit is available. [WRONG: Rejected units are never selected
regardless of the UseLearnedUnitWhenAvailable flag. Deterministic fallback must
be used when the only learned unit is Rejected.]

---

## Layer 1 — Transition Orchestrator Pairs

```
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

The A side is the real user prompt as the system receives it. The B side is the
selected transition result including the full candidate set with rejected candidates.

**Example:**

```
Contract:
  LogicalTransitionOrchestrator : ILogicalOrchestrator

A:
  run_id: "O1-training-001"
  scenario_id: "O1"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  selected_flow_id: "transition.feature_addition"
  overall_verdict: "accepted_for_expert_split"
  candidate_set:
    - transition_type: "feature_addition"
      confidence: 0.88
      normalized_A: "current API has no tenant-aware JWT authentication"
      normalized_B: "API has tenant-aware JWT authentication and tenant-isolation tests"
      transformation_X: "add authentication model, API contract constraints, implementation branches, integration tests, release review"
      selected: true
    - transition_type: "repair"
      confidence: 0.41
      rejection_reason: "The prompt adds capability, not fixes broken existing behavior."
      selected: false
    - transition_type: "external_source_integration"
      confidence: 0.34
      rejection_reason: "The prompt does not ask to ingest a new external source."
      selected: false
```

**DPO pairs for this layer:**

Chosen: correct transition selected with full candidate evidence.
Rejected (Type A): wrong transition type selected — e.g. "repair" selected for
a prompt that adds new capability. [WRONG: repair applies when existing behavior
is broken. This prompt adds a new authentication capability that does not exist
yet. Feature addition is the correct transition.]

---

## Layer 2 — Nine-Node Flow Pairs

The selected transition enters the expert execution flow. Downstream node pairs
must be written from real upstream outputs — never from invented shapes.

### Node 01 — Domain Splitter

```
DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

B contains one packet per development territory actually present in the selected
transition. Do not add territories because they usually appear in similar work.

Required fields per packet:

```
packet_id, domain_label, phase, sub_phase, task_type,
prompt_fragment, coverage_status, reason, handoff_marker, source_evidence
```

**DPO pairs for this layer:**

Chosen: minimal correct territory set grounded in the transition.
Rejected (Type A): adds a plausible but unstated territory — e.g. adds a 5.2
integration testing packet when the prompt only mentions implementation. [WRONG:
the domain splitter must not add territories that are not present in the selected
transition. Testing was not mentioned in the prompt or in the A/B/X statement.]

### Node 02 — Domain Translator

```
DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

A is exactly one domain packet. Node 02 is an array-node — it does not receive
the whole packet array.

B is the formal statement for that packet, including the handoff marker from the
packet.

### Node 03 — Scheme Planner

```
SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

A is the complete set of translated statements from one selected transition.
Node 03 is not an array node.

B is the ordered work branch array. B must express relationships, not just a
step list.

Required fields per branch:

```
branch_id, domain_packet_ids, action, produces, depends_on,
parallel_group, wait_state, recovery_path, boundary_or_knowledge_edge_status
```

**DPO pairs for this layer — highest priority for PLAN training:**

Chosen: correct dependency order with explicit `depends_on` values.
Rejected (Type B): same branches in wrong order, or branches listed with empty
`depends_on` when a real dependency exists. This is the primary training signal
for PLAN accuracy. Use the dependency ordering reference table to construct the
rejected.

Example:

```
CHOSEN — branches in correct order:
  branch_1: sub_phase=3.4, action="Define JWT model and bypass paths", depends_on=[]
  branch_2: sub_phase=5.2, action="Write tenant isolation integration tests", depends_on=["branch_1"]

REJECTED (Type B) — reversed dependency:
  branch_1: sub_phase=5.2, action="Write tenant isolation tests with mock tokens", depends_on=[]
  branch_2: sub_phase=3.4, action="Define JWT model after tests are written", depends_on=["branch_1"]
[WRONG: integration tests for tenant isolation require a working JWT mechanism to
issue and validate real tokens. Mock tokens cannot test real isolation. The
security model must exist before the integration tests can run meaningfully.]
```

### Node 04 — Domain Processor

```
DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
```

A is one work branch. Node 04 is an array-node position.

B states what was derived, what constraints were preserved, what cannot be
completed locally, and which evidence ids support the result.

### Node 05 — Applier

```
ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
```

A is one processed branch result. B preserves the functional principle:

```
current state A, target state B, transformation X,
accepted items, blocked items, rejected items, knowledge edges
```

### Node 06 — Result Translator

```
ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
```

A is the full set of applied branch results. B may fold many results into fewer
translated fragments but every status class must remain visible.

### Node 07 — Arbiter Selector

```
ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
```

B names each selected arbiter request and each considered-but-not-selected
arbiter with the reason.

### Node 08 — Arbiter Unit

```
ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
```

A is one arbiter review request. B is the actual verdict:

```
verdict: pass | concern | reprocess | reject
reason, violated_constraint, reprocess_instruction, learning_gap_packet
```

For clean pass cases, B may be exactly `verdict: "pass"`.

**DPO pairs for this layer:**

Chosen: correct arbiter verdict matching the constraint.
Rejected (Type A): wrong verdict — pass when the implementation misses a
required control, or reject when all controls are satisfied. Use RFC and CWE
authority as the grounding for security arbiter pairs.

### Node 09 — Formatter

```
FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
```

B is the final human-readable result naming accepted, blocked, rejected,
knowledge-edge, and next-action sections where those statuses exist.

---

## Layer 3 — Teaching Unit and Teaching Flow Pairs

### Model Trainer

```
ModelTrainingUnit : ILogicalUnit<FeatureVectorDataset, TrainingResult>
```

B must include:

```
model_artifact_path, model_artifact_bytes,
training_started_at, training_completed_at, teaching_time_ms,
cpu_time_ms, working_set_peak_bytes, managed_allocation_delta_bytes,
disk_read_bytes, disk_write_bytes,
training_row_count, held_out_row_count,
training_accuracy, held_out_accuracy, warnings
```

### Held-Out Evaluator

```
HeldOutEvaluationUnit : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
```

B includes correct count, failed pair ids, confusion matrix, and latency statistics.

### Correction Learner

```
CorrectionLearningUnit : ILogicalUnit<CorrectionTrainingInput, TrainingResult>
```

A is one failed pair or correction pair plus the previous model artifact. B is
the retraining result and the before/after verdict for the corrected input.

---

## Stage 9 — Corruption Cases

Every batch must include at least one of each of the following.

### wrong-predicate-assignment

The compiler B assigns predicate IDs that do not match the lexicon terms in the
prompt. The correct pair shows the right predicates. The corruption shows a
plausible wrong set.

```
Prompt: "test coverage shows 42% on the authentication module"
Correct:  CompiledPredicateIds: ["TEST", "SECURITY"]
Corrupted: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST. "authentication module" is the object
being measured. TEST predicate takes priority when the primary verb is coverage
measurement, not when "authentication" appears as a noun modifier.]
```

### ambiguous-predicate-case

The router returns one routing decision when the predicates support two sub-phases.
The correct pair shows both routing decisions.

```
Prompt: "add authorization to the API endpoint"
Correct:  Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT) — two decisions.
Corrupted: Routes only to 3.4, ignores API_CONTRACT.
[WRONG: API_CONTRACT predicate is present from "API endpoint". When multiple
predicates match, all corresponding sub-phases must be routed.]
```

### knowledge-edge-suppressed

The result returns PASS for a prompt that routed to an unregistered sub-phase.

```
Prompt routes to 5.3 End-to-End Tests — no flow registered.
Correct:  OverallVerdict: "KNOWLEDGE_EDGE"
          Metrics: knowledge_edge=True, missing_sub_phase=5.3, missing_flow=True,
                   routed_phase_id=5, routing_confidence="0.84",
                   knowledge_edge_reason="no logical flow registered for routed sub-phase"
Corrupted: OverallVerdict: "PASS" — knowledge edge suppressed.
[WRONG: PASS fabricates a result the system cannot produce for an unregistered
sub-phase. The required result is KNOWLEDGE_EDGE with all six metrics.]
```

---

## Source Examination Order

For batches targeting the full adaptive orchestration path:

1. Write `PromptRoutingInputCompilerUnit` pairs. No upstream dependency.
2. Write `MultiSubPhaseRouterUnit` pairs from compiler B outputs.
3. Write knowledge-edge detection pairs for every unregistered sub-phase in router output.
   Include at least one registered-flow comparison pair per batch.
4. Write flow factory selection pairs. Include all four trust-state cases per batch.
5. Write `LogicalTransitionOrchestrator` pairs.
6. Write Node 01 rows from selected transitions.
7. Write Nodes 02–09 from real upstream outputs only.
8. Write teaching unit rows after pair files exist.

Do not write downstream node pairs against invented upstream shapes.

---

## Required Coverage Labels

Every pair row must carry:

```
source_id, source_title, source_license_state, contract_signature,
transition_type, phase, sub_phase, task_type, node_role,
coverage_status, pair_kind, rejected_type (A / B / C)
```

Allowed `pair_kind` values:

```
adaptive_routing_compiler
adaptive_routing_multi_phase
knowledge_edge_detection
flow_factory_selection
transition_orchestrator
transition_internal_unit
flow_node
teaching_unit
held_out
corruption_case
rephrasing
```

---

## Failure Modes

**EM-1 — Contract Not Named.** Pair has A and B but no interface signature.
Correction: add the contract signature before A.

**EM-2 — B Describes Output.** B explains what should happen instead of showing
the actual output object.
Correction: rewrite B as the runtime output.

**EM-3 — Transition Replaces Flow Nodes.** Learning set has transition rows but
no Node 01–09 rows.
Correction: restore nine-node rows for the selected transition.

**EM-4 — Downstream Shape Invented.** Node 04–09 rows written before upstream
node output exists.
Correction: generate rows through the chain from real upstream outputs.

**EM-5 — Corpus Teaches Topic But Not Orchestration.** Row teaches a security
fact but not how the orchestrator routes, sequences, or reviews it.
Correction: map the fact to a contract signature and write the pair at the point
where it affects orchestration.

**EM-6 — Adaptive Routing Layer Missing.** Batch targets adaptive orchestrator
but has no Layer 0 pairs.
Correction: produce Layer 0-A and 0-B pairs for the same scenario.

**EM-7 — Knowledge Edge Has No Registered Comparison.** Gap pairs exist but no
comparison pair showing a registered-flow result.
Correction: add at least one registered-flow comparison per batch.

**EM-8 — Factory Selection Missing Rejected-Unit Case.** Factory selection pairs
cover learned and deterministic but not the rejected-unit fallback.
Correction: add the rejected-unit case to every batch with factory selection pairs.

**EM-9 — Router B Uses Invented Fields.** Router B uses `dependency_order`,
`parallel_group`, `domain_label`, or `is_primary`.
Correction: use only actual `SubPhaseRoutingDecision` fields.

**EM-10 — Rejected Uses Forbidden Template.** Rejected response is "Do not
generate a capability for sub-phase X" or any variant of negation phrasing.
Correction: rewrite rejected as a Type A, B, or C response with [WRONG: ...].

**EM-11 — Rejected Has No Annotation.** Rejected response contains no [WRONG: ...]
block.
Correction: add the annotation naming the specific confusion.

**EM-12 — Stage 9 Corruption Cases Missing.** Batch does not contain at least
one of each of the three corruption kinds.
Correction: add wrong-predicate-assignment, ambiguous-predicate-case, and
knowledge-edge-suppressed cases before accepting the batch.

---

## Acceptance Checklist

- [ ] Every row names a contract signature.
- [ ] Every B side is an actual runtime output object, not a description.
- [ ] Layer 0-A B sides use actual SubPhaseRouterSourceDocument field names.
- [ ] Layer 0-A B sides show Confidence 0.92 when predicates matched, 0.70 when none.
- [ ] Layer 0-B B sides use actual SubPhaseRoutingDecision field names only.
- [ ] Layer 0-B A sides come from real compiler B outputs.
- [ ] Knowledge-edge Metrics include all six keys.
- [ ] Knowledge-edge pairs include at least one registered-flow comparison.
- [ ] Flow factory selection includes all four trust-state cases.
- [ ] Transition rows include selected and rejected candidates.
- [ ] Node 02 rows are one packet at a time.
- [ ] Node 03 rows express `depends_on` relationships — not just a step list.
- [ ] Array-node rows do not group multiple items into one A.
- [ ] Node 08 pass rows are silent if runtime pass is silent.
- [ ] Teaching rows include timing, CPU, RAM, disk, accuracy.
- [ ] No rejected response uses the forbidden negation template.
- [ ] Every rejected response includes a [WRONG: ...] annotation.
- [ ] Rejected type (A, B, or C) is recorded in the coverage label.
- [ ] Batch contains at least one of each Stage 9 corruption kind.
- [ ] Batch composition meets minimum pair type fractions.

---

## Last Updated

2026-05-08.
