# External Sources Orchestration Pipeline Plan
# Version: 5.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v4.md

---

## Canonical Pattern Dependency

This plan uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the authority.
Every pipeline stage produces rows for a named `ILogicalOrchestrator`,
`ILogicalFlow`, or `ILogicalUnit` contract.

---

## Purpose

External and first-party sources are not collected to teach isolated facts. They
are collected to teach orchestration:

```text
prompt transition
expert split
phase and sub-phase routing
task sequencing
branch processing
arbiter selection
arbiter verdicts
human result formatting
model teaching and retraining
```

The code implementation is secondary at this stage. The first goal is that the
orchestrator can split a real development request into the correct functional
territories, route those territories through the correct flows, preserve
dependencies and conflicts, and produce a human-readable result.

---

## Pipeline Overview

```text
source registry
-> license and authority check
-> prompt and evidence extraction
-> transition-orchestrator row production
-> selected transition oracle
-> Node 01 expert split rows
-> Node 02 domain translation rows
-> Node 03 scheme plan rows
-> Node 04 processor rows
-> Node 05 applier rows
-> Node 06 result translation rows
-> Node 07 arbiter selector rows
-> Node 08 arbiter verdict rows
-> Node 09 formatter rows
-> teaching-unit rows
-> held-out and corruption rows
-> measured learning-cycle evidence
-> batch zip
```

No downstream row is written from imagination. Each downstream A side comes from
the accepted B side of the previous station.

---

## Stage 0 - Source Registration

Each source receives a source record:

```text
source_id
source_title
source_family
license_state
authority_type
supported_transition_types
supported_phase_sub_phase_task_types
supported_contract_signatures
excluded_uses
attribution_requirement
```

`supported_contract_signatures` is mandatory. A source that cannot be mapped to
a contract signature cannot enter the learning set.

---

## Stage 1 - Prompt and Evidence Extraction

Extract two things separately:

1. Developer-language prompts.
2. Authority evidence that constrains the correct output.

Developer-language prompts become A sides for transition-orchestrator rows.
Authority evidence shapes B sides and arbiter verdicts.

Do not use specification text as the user prompt unless the source is explicitly
a requirements document produced by a user or team.

---

## Stage 2 - Transition-Orchestrator Rows

Target contract:

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

Rows must show:

```text
candidate transition array
selected candidate
rejected candidates
candidate confidence
candidate A/B/X
evidence for each candidate
reason for each rejection
selected flow id
```

The transition type registry and sub-phase registry are connected here:

```text
transition type selected first
sub-phase split produced second
```

The same sub-phase can appear under different transition types. The row must
preserve that distinction.

---

## Stage 3 - Node 01 Expert Split Rows

Target contract:

```text
DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

Rows must split the selected transition into exact domain packets:

```text
phase
sub_phase
task_type
prompt_fragment
coverage_status
reason
handoff_marker
```

The split is minimal and grounded. Do not add territories because they often
appear in similar work.

---

## Stage 4 - Node 02 Domain Translation Rows

Target contract:

```text
DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

Rows are one packet at a time. This is an array-node contract.

The output is the formal domain statement, including boundary, conflict,
knowledge-edge, and handoff markers when present.

---

## Stage 5 - Node 03 Scheme Planner Rows

Target contract:

```text
SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

Rows receive all translated statements together and produce ordered work
branches.

Node 03 rows must prove orchestration relationships:

```text
dependencies
parallel groups
wait states
recovery paths
context carry-forward requirements
blocked and rejected branches
knowledge-edge branches
```

---

## Stage 6 - Nodes 04 Through 06 Branch Execution Rows

Targets:

```text
DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
ApplierUnit         : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
ResultTranslatorUnit: ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
```

Node 04 and Node 05 rows are one branch at a time. Node 06 receives the full set
and may fold results into fewer human fragments if every status class remains
visible.

Implementation code can remain abstract. The important artifact is the logical
transformation:

```text
A current state
B target state
X transformation
accepted
blocked
rejected
knowledge edge
next action
```

---

## Stage 7 - Nodes 07 Through 09 Review and Human Result Rows

Targets:

```text
ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
ArbiterUnit         : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
FormatterUnit       : ILogicalUnit<ReviewedResultSet, HumanResult>
```

Rows must show how the system protects orchestration correctness:

```text
which arbiters fire
which arbiters do not fire
why each arbiter was selected or rejected
pass, concern, reprocess, or reject verdict
human-visible preservation of every hard edge
```

---

## Stage 8 - Teaching Unit Rows

Targets:

```text
PairParserUnit             : ILogicalUnit<PairFileSet, ParsedPairDataset>
FeatureExtractionUnit      : ILogicalUnit<ParsedPairDataset, FeatureVectorDataset>
ModelTrainingUnit          : ILogicalUnit<FeatureVectorDataset, TrainingResult>
HeldOutEvaluationUnit      : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
CorrectionLearningUnit     : ILogicalUnit<CorrectionTrainingInput, TrainingResult>
```

Teaching rows are not optional. The goal is measured learning, not only pair
authoring.

Training result rows must include:

```text
teaching time
process time
CPU
RAM
drive read/write
model artifact size
training count
held-out count
held-out accuracy
oracle stability
```

---

## Stage 9 - Held-Out and Corruption Rows

Each batch must include:

```text
held-out prompts
rephrased prompts
wrong-transition prompts
dropped-constraint prompts
wrong-sub-phase prompts
missing-evidence prompts
arbiter contradiction prompts
```

These rows test whether the learned unit detects incorrect orchestration rather
than only reproducing positive cases.

---

## Wave Priorities

### Wave 2

Security and compliance review, sub-phase 6.3.

Purpose: convert Wave 1 security authority into post-implementation review rows.

Primary contracts:

```text
ArbiterSelectorUnit
ArbiterUnit
FormatterUnit
HeldOutEvaluationUnit
```

### Wave 3

Architecture and component topology, sub-phase 3.1.

Purpose: teach the prerequisites that every security and implementation branch
depends on.

Primary contracts:

```text
DomainSplitterUnit
DomainTranslatorUnit
SchemePlannerUnit
TransitionCandidateSelector
```

### Wave 4

Integration testing, sub-phase 5.2.

Purpose: teach cross-component and cross-tenant validation as orchestration
evidence, not as code explanation.

Primary contracts:

```text
SchemePlannerUnit
DomainProcessorUnit
ApplierUnit
ArbiterUnit
```

### Wave 5

Cross-phase transition rows.

Purpose: teach carry-forward from architecture to security, security to
implementation, implementation to testing, and testing to QA.

Primary contracts:

```text
LogicalTransitionOrchestrator
DomainSplitterUnit
SchemePlannerUnit
TransitionVerificationUnit
```

---

## Batch Acceptance

A batch is accepted only if it contains all applicable layers:

```text
transition orchestrator rows
Node 01 split rows
Node 02 translation rows
Node 03 plan rows
Node 04-06 branch rows when upstream plan branches exist
Node 07-09 review rows when branch results exist
teaching unit rows when enough pair files exist to train
held-out and corruption rows
manifest
zip
```

---

## Last Updated

2026-05-05.

