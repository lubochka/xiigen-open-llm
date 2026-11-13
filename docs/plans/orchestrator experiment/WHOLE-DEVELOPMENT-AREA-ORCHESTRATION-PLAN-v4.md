# Whole Development Area Orchestration Plan
# Version: 4.0 - 2026-05-05
# Supersedes: WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-PLAN-v3.md

---

## Purpose

This plan defines how to teach and test the dynamic flow as a whole-development
orchestrator.

Version 4 locks the transition layer as non-deterministic and orchestrator-based.

The first adaptive component is:

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

It chooses and reviews the A/B/X transition before expert splitting begins.

---

## Correct Runtime Shape

```text
human prompt
-> LogicalTransitionOrchestrator
   -> selects transition flow
   -> generates candidate A/B/X transitions
   -> compares candidates
   -> runs transition arbiters
   -> returns selected transition or clarification
-> ExpertiseTaskSplitter
-> PromptSimplifier
-> DomainLanguageTranslator
-> SchemePlanner
-> branch ILogicalUnits
-> Applier units
-> ResultTranslator
-> ArbiterSelector
-> ArbiterUnits
-> HumanResultFormatter
```

The transition step is not a helper function and not a deterministic normalizer.
It is a nested orchestrator with its own flows, units, arbiters, traces, and
learning evidence.

---

## Functional Principle

Every branch is still expressed as:

```text
A -> B + X
```

The difference is that the top-level A/B/X is selected from candidates:

```text
C1 = A -> B + X as feature addition
C2 = A -> B + X as repair
C3 = A -> B + X as learning cycle
C4 = A -> B + X as topology mutation
...
```

The transition orchestrator selects the candidate that best matches the prompt,
source context, constraints, and arbiters.

---

## Transition Candidate Types

| Type | Meaning |
|---|---|
| `feature_addition` | Add a new capability or behavior. |
| `repair` | Correct broken, unsafe, slow, or incomplete behavior. |
| `learning_cycle` | Teach or retrain units/corpus/model behavior. |
| `topology_mutation` | Add or edit domains, phases, sub-phases, transitions, or methodology. |
| `external_source_integration` | Use external source material with license/source authority preservation. |
| `qa_release` | Review, block, promote, or automate release readiness. |
| `performance_correction` | Reduce CPU, RAM, latency, allocation, or rendering cost. |
| `documentation_alignment` | Update plans, ledgers, manifests, and project documentation. |

These are registry entries. They are not hardcoded branches.

---

## Required Orchestration Components

| Order | Component | Interface shape | Responsibility |
|---:|---|---|---|
| 1 | LogicalTransitionOrchestrator | `ILogicalOrchestrator` | Selects transition flow and returns selected A/B/X transition. |
| 2 | TransitionIntentClassifier | `ILogicalUnit` | Scores candidate transition types. |
| 3 | CandidateTransitionGenerator | `ILogicalUnit` | Produces candidate A/B/X objects. |
| 4 | TransitionCandidateCompressor | `ILogicalUnit` | Shortens candidates while preserving constraints. |
| 5 | TransitionArbiterSelector | `ILogicalUnit` | Selects arbiters that judge candidate validity. |
| 6 | TransitionArbiterUnits | `ILogicalUnit` | Reject invalid candidates and flag knowledge edges. |
| 7 | ExpertiseTaskSplitter | `ILogicalUnit` or nested flow | Splits selected transition into expertise branches. |
| 8 | PromptSimplifier | `ILogicalUnit` | Produces compact Ai/Bi/Xi branch sources. |
| 9 | Downstream flow | `ILogicalFlow` | Runs branch processing, arbiters, result translation, and formatter. |

---

## O1 Transition Selection Example

Prompt:

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

Candidate transitions:

```yaml
candidates:
  - transition_type: feature_addition
    confidence: 0.91
    status: selected
    reason: "Prompt asks to add a capability across design, implementation, test, QA, and automation."
  - transition_type: repair
    confidence: 0.55
    status: rejected
    reason: "Prompt does not state an existing implementation is broken."
  - transition_type: learning_cycle
    confidence: 0.31
    status: rejected
    reason: "Prompt asks for product orchestration, not model teaching."
selected_transition:
  A:
    - "API lacks the requested tenant-aware authentication lifecycle."
  B:
    - "API has tenant-aware authentication with security evidence and release blocking."
  X:
    - "design, implement, test, review, and automate tenant-authentication controls."
constraints:
  - "internal-first execution"
  - "duplicate identity creation rejected"
  - "missing tenant authority remains knowledge edge"
```

---

## Teaching Data Per Scenario

Each scenario now needs transition-orchestrator rows before splitter rows.

```text
transition intent rows;
candidate transition generation rows;
candidate comparison rows;
transition arbiter rows;
selected transition rows;
expert split rows;
prompt simplifier rows;
downstream branch rows;
human result rows;
held-out candidate ambiguity rows;
corruption rows.
```

---

## First Learning Path

The first learning path is:

```text
O1 transition intent classification
O1 candidate A/B/X generation
O1 transition candidate comparison
O1 transition arbiter review
O1 expert split from selected transition
```

Only after the transition orchestrator is measured should the splitter be trained
as the second layer.

---

## Measurement

Every transition-orchestrator run records:

```text
candidate_count;
selected_transition_type;
selected_candidate_confidence;
candidate_margin;
false_candidate_selected;
clarification_required;
transition_arbiter_rejection_count;
teaching_elapsed_ms;
prompt_process_elapsed_ms;
cpu_ms_delta;
working_set_mb_peak;
private_memory_mb_peak;
managed_allocated_mb_delta;
disk_read_mb_delta;
disk_write_mb_delta;
model_file_bytes;
```

The candidate margin is important. If two transition candidates are close and
lead to different downstream work, the correct result may be clarification.

---

## Last Updated

2026-05-05. Version 4 makes the transition layer an adaptive
`ILogicalOrchestrator`.
