# Corpus Expansion Plan
# Transition-orchestrator teaching corpus plan
# Version: 7.0 - 2026-05-05
# Supersedes: CORPUS-EXPANSION-PLAN-v6.md

---

## Core Correction From v6

Version 6 correctly moved the corpus target from isolated node behavior to full
orchestration learning.

Version 7 corrects the first learning boundary.

The first learning boundary is not:

```text
prompt -> prompt splitter
```

The first learning boundary is:

```text
prompt -> LogicalTransitionOrchestrator -> selected A/B/X transition
```

The transition layer is non-deterministic and orchestrator-based. It produces
candidate transitions, reviews them, and selects one or asks for clarification.

---

## Active Completion Target

The active target is a transition-orchestrator teaching corpus.

The corpus is complete for this stage when:

```text
O1 through O5 have transition-orchestrator training rows;
each scenario has candidate transition examples;
each scenario has selected and rejected candidate examples;
each scenario has clarification examples when candidates are unresolved;
each scenario has expert split rows derived from the selected transition;
each scenario has compact Ai/Bi/Xi branch rows;
each scenario has arbiter and reprocess examples;
each scenario has measured teaching-cycle templates;
```

---

## First Teaching Order

| Order | Component | Interface | What it teaches |
|---:|---|---|---|
| 1 | `LogicalTransitionOrchestrator` | `ILogicalOrchestrator` | Selects transition flow and candidate A/B/X transition. |
| 2 | `TransitionIntentClassifier` | `ILogicalUnit` | Scores possible transition types. |
| 3 | `CandidateTransitionGenerator` | `ILogicalUnit` | Produces candidate A/B/X objects. |
| 4 | `TransitionCandidateSelector` | `ILogicalUnit` | Selects, rejects, or clarifies between candidates. |
| 5 | `ExpertiseTaskSplitter` | `ILogicalUnit` or nested flow | Splits selected transition into expertise/task branches. |
| 6 | `PromptSimplifier` | `ILogicalUnit` | Produces compact Ai/Bi/Xi branch sources. |
| 7 | downstream branch units | `ILogicalUnit` | Solve branch principles. |
| 8 | arbiters and formatter | `ILogicalUnit` | Review and translate reviewed logical results. |

---

## Required Row Families

Every orchestration scenario now produces:

```text
transition intent rows;
candidate A/B/X generation rows;
candidate comparison rows;
transition arbiter rows;
selected transition rows;
expertise split rows;
prompt simplification rows;
branch logical-unit rows;
arbiter review rows;
human result translation rows;
held-out rows;
corruption rows;
resource metric rows.
```

---

## Scenario Order

Start with O1.

O1 is strong because it naturally produces multiple transition candidates:

```text
feature_addition candidate - selected;
repair candidate - rejected;
learning_cycle candidate - rejected;
topology_mutation candidate - rejected;
external_source_integration candidate - partial support but not selected.
```

O2 through O5 continue after O1 artifacts exist.

---

## Wave 1 Placement

Wave 1 remains a deep security authority corpus.

In the transition orchestrator, Wave 1 supports candidates and branches that
include security authority. It does not decide the whole transition by itself.

Example:

```text
O1 selected transition = feature_addition;
Wave 1 supports the security branch inside that transition;
Wave 1 does not convert O1 into an external-source-learning transition.
```

---

## Measurement Requirement

Every transition-orchestrator teaching cycle records:

```text
candidate_count;
selected_transition_type;
candidate_margin;
false_candidate_selected;
clarification_accuracy;
source_gap_preserved;
teaching_elapsed_ms;
prompt_process_elapsed_ms;
CPU;
RAM;
managed allocation;
disk allocation;
model size.
```

---

## Last Updated

2026-05-05. Version 7 makes `LogicalTransitionOrchestrator` the first active
teaching target.
