# Document Examination to Training Sets - Orchestration Edition
# Version: 6.0 - 2026-05-05
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v5.md

---

## Canonical Pattern Dependency

This document uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the
authority for `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, recursive
composition, and array output.

This document does not define new component kinds. It defines how external and
first-party sources become training rows for those canonical contracts.

---

## Purpose

External learning sets must teach the whole orchestration path:

```text
human prompt
-> transition orchestrator
-> selected A/B/X transition
-> expert domain split
-> nine-node execution flow
-> teaching and retraining flows
```

Version 5 covered only the transition orchestrator. That was incomplete. The
transition orchestrator precedes the nine-node flow; it does not replace it.

Version 6 restores the three required pair layers:

1. Transition orchestrator pairs.
2. Nine-node flow pairs.
3. Teaching unit and teaching flow pairs.

Every pair must state its runtime contract before A and B are written.

---

## Universal Pair Rule

Each pair has this shape:

```text
Contract:
  <interface signature>

A:
  the exact runtime input object

B:
  the exact runtime output object
```

B must be the output itself, written in full. B must not say what the output
should contain.

Correct:

```text
B:
  candidates:
    - transition_type: feature_addition
      confidence: 0.86
      reason: "The prompt asks to add a new user-visible authentication capability."
```

Incorrect:

```text
B:
  The model should produce a feature addition candidate with a good reason.
```

---

## Layer 1 - Transition Orchestrator Pairs

### Orchestrator Contract

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

Runtime input:

```text
LogicalOrchestratorRequest
```

Runtime output:

```text
LogicalOrchestratorResult
```

The A side is the real user prompt exactly as the system receives it. The B side
is the selected transition flow result, including the candidate set and rejected
candidates as evidence.

### Transition-Orchestrator Pair Shape

```text
Contract:
  LogicalTransitionOrchestrator : ILogicalOrchestrator

A:
  run_id: "O1-training-001"
  scenario_id: "O1"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  flags:
    UseCompiledPredicateRepresentation: true
    UseDomainScopedRetrieval: true
    UseConfidenceFloor: true
    UseLearnedUnitWhenAvailable: true
    UseTraceEvidence: true
    UseContextCarryForward: true
    UseIdleLearningScheduling: true
    UseArbiterAuthority: true
    UseKnowledgeEdgeDetection: true
    UseInternalFirstExecution: true
    RequireExternalAiApproval: true
    RejectArbiterContradictions: true
  metadata:
    source: "XIIGen first-party prompt"
    license_state: "first_party"

B:
  selected_flow_id: "transition.feature_addition"
  overall_verdict: "accepted_for_expert_split"
  final_human_result: "Feature-addition transition selected. Expert split required before execution."
  candidate_set:
    - transition_type: "feature_addition"
      flow_id: "transition.feature_addition"
      confidence: 0.88
      normalized_A: "current API has no tenant-aware JWT authentication guarantee"
      normalized_B: "API has tenant-aware JWT authentication and tenant-isolation tests"
      transformation_X: "add authentication model, API contract constraints, implementation branches, integration tests, and release review"
      evidence:
        - "add multi-tenant JWT authentication"
        - "make sure tenant isolation is tested"
      selected: true
    - transition_type: "repair"
      flow_id: "transition.repair"
      confidence: 0.41
      rejection_reason: "The prompt asks to add capability, not fix an identified broken existing behavior."
      selected: false
    - transition_type: "external_source_integration"
      flow_id: "transition.external_source_integration"
      confidence: 0.34
      rejection_reason: "The prompt needs security authority evidence, but does not ask to ingest a new external source."
      selected: false
  flow_result:
    FlowId: "transition.feature_addition"
    ScenarioId: "O1"
    OverallVerdict: "accepted_for_expert_split"
    FinalHumanResult: "Feature-addition transition selected."
```

### Internal Transition Units

The transition orchestrator may contain these units. Each is an ordinary
`ILogicalUnit`.

| Unit | Contract | Pair purpose |
|---|---|---|
| Transition intent classifier | `ILogicalUnit<HumanPromptTransitionInput, CandidateTransitionTypeScore[]>` | Produces scored candidate transition types. |
| Candidate transition generator | `ILogicalUnit<CandidateTransitionTypeScore, LogicalTransitionCandidate>` | Expands one candidate type into A/B/X. |
| Candidate compressor | `ILogicalUnit<LogicalTransitionCandidate, LogicalTransitionCandidate>` | Shortens the candidate without losing A, B, X, evidence, or risk. |
| Transition arbiter selector | `ILogicalUnit<LogicalTransitionCandidate[], TransitionArbiterSelection[]>` | Selects arbiters for the candidate set. |
| Transition candidate selector | `ILogicalUnit<ReviewedTransitionCandidate[], LogicalTransitionSelection>` | Chooses selected candidate and rejected candidates. |

Pair writers may produce rows for any of these units, but the A and B must always
match the listed contract.

---

## Layer 2 - Nine-Node Flow Pairs

The selected transition enters the expert execution flow. The nine-node flow is
still required.

### Node 01 - Domain Splitter

```text
DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

A is the selected A/B/X transition plus the original prompt as evidence. B is
the actual domain packet array.

B must contain one packet per development territory actually present in the
selected transition. Do not add usual-but-unstated territories.

Required fields per packet:

```text
packet_id
domain_label
phase
sub_phase
task_type
prompt_fragment
coverage_status
reason
handoff_marker
source_evidence
```

Clean routing B example:

```text
B:
  packets:
    - packet_id: "O1-D01"
      domain_label: "architecture-and-component-topology"
      phase: "3 technical design"
      sub_phase: "3.1"
      task_type: "tenant isolation topology"
      prompt_fragment: "multi-tenant JWT authentication"
      coverage_status: "covered"
      reason: "The authentication model needs an architecture decision for tenant boundary placement."
      handoff_marker: "ready_for_scheme_planner"
      source_evidence: ["multi-tenant JWT authentication"]
    - packet_id: "O1-D02"
      domain_label: "security-and-authorization-model"
      phase: "3 technical design"
      sub_phase: "3.4"
      task_type: "JWT validation and tenant authorization"
      prompt_fragment: "JWT authentication"
      coverage_status: "covered"
      reason: "The prompt asks for JWT authentication and tenant isolation."
      handoff_marker: "ready_for_scheme_planner"
      source_evidence: ["JWT authentication", "tenant isolation"]
    - packet_id: "O1-D03"
      domain_label: "integration-tests"
      phase: "5 testing"
      sub_phase: "5.2"
      task_type: "cross-tenant isolation integration tests"
      prompt_fragment: "make sure tenant isolation is tested"
      coverage_status: "covered"
      reason: "The prompt explicitly asks for tenant isolation testing."
      handoff_marker: "ready_for_scheme_planner"
      source_evidence: ["tenant isolation is tested"]
```

### Node 02 - Domain Translator

```text
DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

A is exactly one domain packet. Node 02 is an array-node position. It does not
receive the whole packet array.

B is the actual formal statement for that packet. It includes the handoff marker
from the packet, not a description of the marker.

For security packets, B should include control type, authority source, required
evidence, conflict status, and knowledge-edge status when present.

### Node 03 - Scheme Planner

```text
SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

A is the complete set of translated domain statements from one selected
transition. Node 03 is not an array node.

B is the actual ordered work branch array. B must include:

```text
branch_id
domain_packet_ids
action
produces
depends_on
parallel_group
wait_state
recovery_path
boundary_or_knowledge_edge_status
```

The plan must express relationships, not just a step list. Dependencies must be
enforced in the branch data.

### Node 04 - Domain Processor

```text
DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
```

A is one work branch. Node 04 is an array-node position.

B is the actual processed branch result. It must state what was derived from the
branch, what constraints were preserved, what cannot be completed locally, and
which evidence ids support the result.

### Node 05 - Applier

```text
ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
```

A is one processed branch result. Node 05 is an array-node position.

B is the actual applied result. At this stage implementation code can remain
abstract; the important requirement is that B preserves the functional principle:

```text
current state A
target state B
transformation X
accepted items
blocked items
rejected items
knowledge edges
```

### Node 06 - Result Translator

```text
ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
```

A is the full set of applied branch results. Node 06 may fold many applied
results into fewer translated fragments, but every status class must remain
visible.

B is the actual translated result fragments, not a summary of them.

### Node 07 - Arbiter Selector

```text
ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
```

A is the full translated result set.

B names each selected arbiter request and each considered-but-not-selected
arbiter with the reason. This teaches arbiter routing, not arbiter judgment.

### Node 08 - Arbiter Unit

```text
ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
```

A is one arbiter review request. Node 08 is an array-node position.

B is the actual verdict:

```text
verdict: pass | concern | reprocess | reject
reason
violated_constraint
reprocess_instruction
learning_gap_packet
```

For pass cases, B may be exactly:

```text
B:
  verdict: "pass"
```

Do not write explanations for clean pass cases unless the target arbiter output
at runtime includes them.

### Node 09 - Formatter

```text
FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
```

A is the reviewed result set after arbiter verdicts.

B is the final human-readable result. It must name accepted, blocked, rejected,
knowledge-edge, and next-action sections when those statuses exist.

---

## Layer 3 - Teaching Unit and Teaching Flow Pairs

Teaching is also dynamic flow work. External learning-set documents must prepare
pairs for the units that teach other units.

### Pair Parser

```text
PairParserUnit : ILogicalUnit<PairFileSet, ParsedPairDataset>
```

A is a set of pair file paths and source metadata. B is the parsed pair dataset,
including invalid rows and reasons.

### Feature Extractor

```text
FeatureExtractionUnit : ILogicalUnit<ParsedPairDataset, FeatureVectorDataset>
```

A is parsed pairs. B is the actual feature vector dataset. B must include vector
width, label vocabulary, source pair ids, and extraction warnings.

### Model Trainer

```text
ModelTrainingUnit : ILogicalUnit<FeatureVectorDataset, TrainingResult>
```

A is the feature vector dataset and training settings.

B is the actual training result:

```text
model_artifact_path
model_artifact_bytes
training_started_at
training_completed_at
teaching_time_ms
cpu_time_ms
working_set_peak_bytes
private_memory_peak_bytes
managed_allocation_delta_bytes
disk_read_bytes
disk_write_bytes
training_row_count
held_out_row_count
training_accuracy
held_out_accuracy
warnings
```

### Held-Out Evaluator

```text
HeldOutEvaluationUnit : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
```

A is the model artifact plus held-out pair set. B is the actual evaluation
result, including correct count, failed pair ids, confusion matrix, and latency
statistics.

### Correction Learner

```text
CorrectionLearningUnit : ILogicalUnit<CorrectionTrainingInput, TrainingResult>
```

A is one failed pair or new correction pair plus the previous model artifact.
B is the retraining result and the before/after verdict for the corrected input.

---

## Source Examination Order

For every external or first-party source:

1. Confirm license state.
2. Identify which transition types it can teach.
3. Identify which phases, sub-phases, and task types it can teach.
4. Identify which runtime contracts it can produce rows for.
5. Write transition orchestrator rows first.
6. Write Node 01 rows from selected transitions.
7. Write Node 02 through Node 09 rows only from real upstream outputs.
8. Write teaching unit rows only after the pair files exist.

Do not write downstream node pairs against invented upstream shapes.

---

## Required Coverage Labels

Every pair row must carry:

```text
source_id
source_title
source_license_state
contract_signature
transition_type
phase
sub_phase
task_type
node_role
coverage_status
pair_kind
```

Allowed `pair_kind` values:

```text
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

### EM-1 - Contract Not Named

The pair has A and B but does not state the interface signature. The row cannot
be verified against runtime shape.

Correction: add the contract signature before A.

### EM-2 - B Describes Output

B explains what should happen instead of showing the actual output.

Correction: rewrite B as the runtime output object.

### EM-3 - Transition Replaces Flow Nodes

The learning set contains transition orchestrator rows but no Node 01 through
Node 09 rows.

Correction: restore the nine-node rows for the selected transition.

### EM-4 - Downstream Shape Invented

Node 04, 05, 06, 07, 08, or 09 rows are written before the upstream node output
exists.

Correction: generate rows through the chain. Node 03 B becomes Node 04 A. Node
04 B becomes Node 05 A. Node 05 B contributes to Node 06 A.

### EM-5 - Corpus Teaches Topic But Not Orchestration

The row teaches a security fact, API fact, or testing fact, but does not teach
how the orchestrator splits, routes, sequences, reviews, or learns.

Correction: map the fact to a contract signature and write the pair at the point
where that fact affects orchestration.

---

## Acceptance Checklist

Before accepting a learning-set batch:

- [ ] Every row names a contract signature.
- [ ] Every B side is an actual runtime output.
- [ ] Transition orchestrator rows include selected and rejected candidates.
- [ ] Node 01 rows include one packet per grounded territory.
- [ ] Node 02 rows are one packet at a time.
- [ ] Node 03 rows receive all translated statements together.
- [ ] Array-node rows do not group multiple items into one A.
- [ ] Node 06 fold rows preserve every status class.
- [ ] Node 07 rows separate selected and rejected arbiter candidates.
- [ ] Node 08 pass rows are silent if runtime pass is silent.
- [ ] Node 09 rows are readable to a human and preserve every hard edge.
- [ ] Teaching rows include timing, CPU, RAM, disk, model size, and accuracy.

---

## Last Updated

2026-05-05.

