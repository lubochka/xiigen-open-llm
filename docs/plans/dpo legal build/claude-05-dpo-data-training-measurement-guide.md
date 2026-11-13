---
artifact_type: dpo_data_training_measurement_guide
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_no_training_run
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# DPO Data Training Measurement Guide

## Goal

Build DPO preference data for `ILearningDynamicGraph` from low-quality local or project outputs versus teacher, goal, or human-approved outputs, then measure whether the learned graph policy improves routing, branch composition, and final graph answers on held-out tasks.

## Pair Shape

Minimal shape follows existing project precedent:

```text
prompt
chosen
rejected
```

For dynamic graph training, attach domain metadata:

```text
graph_task_id
initial_graph_state_ref
candidate_branch_set_ref
chosen_path
rejected_path
chosen_final_answer
rejected_final_answer
evidence_ids
source_ids
provenance_ref
train_or_heldout
source_disjoint_group
```

## Pair Generation Strategy

1. Start with a project-local task prompt and source ids.
2. Generate one or more local candidate graph routes or outputs.
3. Mark plausible but lower-quality local outputs as `rejected`.
4. Use human-approved, project-goal, or legally approved teacher output as `chosen`.
5. Reject rows where chosen and rejected are near-identical.
6. Reject rows where the rejected side is random nonsense instead of plausible lower-quality behavior.
7. Split rows into source-disjoint train and held-out sets before training.
8. Store provenance before any row can enter training.

## Judge Strategy

| judge route | status now | allowed role |
|---|---|---|
| Human or Luba review | planning allowed | Gold labels and promotion decisions |
| Self-hosted verified open model | future conditional | Teacher or judge after license gate |
| Graph self-judge | future conditional | Secondary judge, never sole legal authority |
| Closed hosted API | blocked now | No train signal until provider terms gate passes |
| Deterministic metric checker | allowed helper | Structural quality, evidence coverage, leakage checks |

## Measurement Families

| family | metric examples | pass meaning |
|---|---|---|
| DPO metrics | loss, chosen reward, rejected reward, reward margin, accuracy | Policy prefers chosen over rejected on held-out rows |
| Graph routing | correct flow path, dependency order, bridge subphase inclusion | Graph route matches goal and evidence |
| Branch quality | branch answer score, evidence grounding, conflict resolution | Async branches improve result quality |
| Lifecycle | save, load, continue training, fresh-load evaluation | Learned state persists outside current process |
| Leakage | source overlap, candidate-order leakage, prompt memorization | Held-out metric is credible |
| Resource | CPU, RAM, elapsed time | Training and routing stay within product budget |

## Anti Deterministic Training Warning

A deterministic graph helper that always chooses the same route is not learning. A static rule improvement is useful, but it must be labeled deterministic. A learned claim requires at least:

```text
train rows
held-out rows
positive reward margin
held-out accuracy or route-quality lift
fresh-load checkpoint pass
ablation against no learned state
source-disjoint split
```

## Async Unit Combination Comparison

Dynamic graph training must compare combinations, not only single units. Example comparison set:

```text
candidate A: SourceEncoderUnit -> DirectAnswerFlow
candidate B: SourceEncoderUnit -> CriticUnit -> RepairFlow
candidate C: SourceEncoderUnit -> ParallelEvidenceFlow + ParallelRiskFlow -> MergeUnit
candidate D: BridgeSubPhaseFlow -> CriticUnit -> FinalAnswerFlow
```

The DPO pair can prefer a whole path, a branch merge, or a final answer. The measurement must record which unit combination won, which lost, and whether the win depends on task type.

## Quality Gates

| gate | pass condition |
|---|---|
| row_validity_gate | Prompt, chosen, rejected, provenance, source ids, and use rights are present |
| plausibility_gate | Rejected output is plausible lower-quality behavior |
| legal_gate | Provider and source terms allow the row use |
| split_gate | Train and held-out source groups are disjoint |
| judge_gate | Judge route is approved for the use type |
| metric_gate | DPO and graph metrics are numeric and not placeholder |
| lifecycle_gate | Fresh machine can load checkpoint and pass held-out evaluation |
| ablation_gate | Learned state beats no-state or deterministic baseline |
| async_gate | Independent branches can run and merge without lost context |
| promotion_gate | All required gates pass before marking graph trusted |
