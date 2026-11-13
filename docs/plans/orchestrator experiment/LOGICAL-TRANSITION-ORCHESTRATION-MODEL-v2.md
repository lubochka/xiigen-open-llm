# Logical Transition Orchestration Model
# Version: 2.0 - 2026-05-05
# Supersedes: LOGICAL-TRANSITION-ORCHESTRATION-MODEL-v1.md

---

## Purpose

Version 1 correctly added the missing transition step:

```text
human prompt -> A/B/X transition object -> expert split
```

Version 2 corrects the execution shape of that step.

Transition normalization is not a deterministic unit.

Transition normalization is itself an orchestrator.

It follows the same pattern as the rest of the dynamic flow:

```text
ILogicalOrchestrator -> ILogicalFlow -> ILogicalUnit cells -> arbiters -> result
```

The transition object is generated, compared, reviewed, and selected. It is not
hardcoded or produced by a single fixed rule.

---

## Corrected High-Level Flow

```text
human prompt
-> logical transition orchestrator
   -> transition intent flow selection
   -> candidate A/B/X generation
   -> candidate compression
   -> transition arbiter review
   -> selected transition result
-> expertise and task splitter
-> prompt simplifier
-> domain-language translator
-> scheme planner
-> branch logical units
-> appliers
-> result translator
-> arbiter selector
-> arbiter units
-> final human formatter
```

The logical transition orchestrator owns the first adaptive decision: what kind
of transition the human prompt is asking for.

---

## Why It Must Be An Orchestrator

A user prompt can imply different transition types:

```text
state repair transition;
feature addition transition;
architecture redesign transition;
testing expansion transition;
QA rejection transition;
external-source learning transition;
methodology/topology mutation transition;
performance correction transition;
```

A single deterministic normalizer would need to choose one shape up front. That
would hardcode methodology and lose useful ambiguity.

An orchestrator can:

```text
produce multiple candidate A/B/X interpretations;
score each candidate;
run arbiters over contradictions and source gaps;
select one transition flow;
preserve rejected candidates as evidence;
ask for clarification when candidates cannot be separated safely.
```

That is the dynamic-flow pattern.

---

## Interface Shape

The transition layer is represented as:

```text
ILogicalOrchestrator
```

Suggested concrete role:

```text
LogicalTransitionOrchestrator
```

It receives:

```text
LogicalOrchestratorRequest
```

It returns:

```text
LogicalOrchestratorResult
```

The selected flow result contains:

```text
LogicalTransitionResult
```

---

## LogicalTransitionResult

The transition orchestrator result should contain:

```yaml
selected_transition:
  transition_id:
  transition_type:
  source_state_A:
  target_state_B:
  transformation_intent_X:
  constraints:
  unknowns:
  rejected_intents:
  allowed_scope:
candidate_transitions:
  - transition_id:
    transition_type:
    source_state_A:
    target_state_B:
    transformation_intent_X:
    confidence:
    status:
    rejection_reason:
arbiters:
  selected:
  verdicts:
clarification:
  needed:
  questions:
confidence:
warnings:
evidence_ids:
```

The splitter receives `selected_transition`, but the inspector and evidence
files keep the candidates visible.

---

## Internal Transition Flows

The transition orchestrator can select among flows.

| Flow | Used when |
|---|---|
| `feature-addition-transition-flow` | The prompt asks to add a new capability. |
| `repair-transition-flow` | The prompt describes broken or insufficient behavior. |
| `performance-correction-transition-flow` | The prompt describes CPU, RAM, latency, throughput, or allocation problems. |
| `learning-cycle-transition-flow` | The prompt asks to teach, retrain, expand corpus, or improve unit behavior. |
| `topology-mutation-transition-flow` | The prompt asks to add/edit domains, phases, sub-phases, transitions, or methodology. |
| `external-source-integration-transition-flow` | The prompt asks to use external sources while preserving license/source authority. |
| `qa-release-transition-flow` | The prompt asks to evaluate release readiness, QA findings, or automation gates. |

The list is registry data. It is not a fixed `switch` inside the orchestrator.

---

## Candidate Generation

The transition orchestrator does not immediately select one A/B/X.

It first generates candidates:

```text
C1 = A/B/X as feature-addition transition
C2 = A/B/X as repair transition
C3 = A/B/X as learning-cycle transition
...
```

Each candidate carries:

```text
confidence;
source evidence;
missing evidence;
conflicts;
why it may be wrong.
```

Then arbiters judge candidate validity.

---

## Selection Rules

The selected transition is the highest-confidence candidate that:

```text
does not contradict a hard constraint;
does not invent missing source authority;
preserves rejected intents;
does not hide a knowledge edge;
stays inside allowed scope;
passes transition arbiter review.
```

If two candidates remain too close in confidence and imply different downstream
work, the result is clarification, not guessing.

---

## Relationship To A/B/X

A/B/X remains the working representation.

The correction is that A/B/X is generated by an orchestrator.

```text
human prompt
-> transition orchestrator
-> candidate A/B/X set
-> selected A/B/X
-> expert split
```

Each branch still becomes:

```text
A1 -> B1 + X1
A2 -> B2 + X2
...
An -> Bn + Xn
```

---

## Teaching Implication

The first learning set must teach:

```text
transition-flow selection;
candidate A/B/X generation;
candidate comparison;
transition arbiter review;
clarification when candidate confidence is unresolved.
```

The first learned component is not a deterministic normalizer.

The first trained orchestration component is:

```text
LogicalTransitionOrchestrator
```

with learned units inside it.

---

## O1 Example

The O1 prompt can produce multiple candidate transitions:

### Candidate 1 - Feature addition

```yaml
transition_type: feature_addition
A:
  - API lacks multi-tenant authentication lifecycle.
B:
  - API has tenant-aware authentication through design, implementation, tests,
    review, and release automation.
X:
  - add authentication capability and evidence chain.
confidence: high
status: accepted_candidate
```

### Candidate 2 - Security repair

```yaml
transition_type: security_repair
A:
  - Existing authentication is unsafe.
B:
  - Existing authentication is repaired.
X:
  - fix unsafe behavior.
confidence: medium
status: rejected_candidate
rejection_reason: prompt asks to add capability, not repair an existing failed implementation.
```

### Candidate 3 - Learning-cycle request

```yaml
transition_type: learning_cycle
A:
  - local machine lacks training for tenant auth.
B:
  - local machine learns tenant auth.
X:
  - teach corpus and retrain units.
confidence: low
status: rejected_candidate
rejection_reason: prompt asks to orchestrate product work, not teach the machine.
```

The selected transition is Candidate 1.

The rejected candidates remain visible in evidence because they explain why the
orchestrator did not route the prompt as repair or learning.

---

## Last Updated

2026-05-05. Version 2 makes transition normalization an adaptive
`ILogicalOrchestrator`, not a deterministic `ILogicalUnit`.
