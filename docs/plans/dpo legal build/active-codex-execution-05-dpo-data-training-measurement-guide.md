## Fable Coverage Repair Overlay (2026-07-05)

plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
implementation_authorization: NO until fresh Luba approval after review
current_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` supersede the previous final-readiness overlay. Fable reports 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers: R-B 20/40 knowledge-library artifacts are not accepted built artifacts; R-G source addressability is incomplete; several shared contracts/readability/lifecycle carriers need plan-family repair. This is plan repair evidence only, not product implementation, not tests/builds/runtime/provider/training/checkpoint/git proof.
current_verdict_classes: DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS; CURRENT_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=previous 50/50 planning-section acceptance remains historical/process evidence only and cannot override later Fable MISSING/PARTIAL findings; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, Final Readiness Review Overlay, no unresolved design/dependency blocker, CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW, or parent final-readiness acceptance before this overlay is historical/pre-Fable evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md is the current repair map for R-B/R-G and the partial contract/readability/lifecycle additions.
## Historical Pre-Fable Final Readiness Review Overlay (2026-07-05)
historical_pre_fable_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
implementation_authorization: NO until fresh Luba approval after review
final_readiness_review_scope: one bounded planning/governance readiness review only; no product implementation, tests, builds, runtime, browser, provider/API, model/training/checkpoint, git, branch, mergeability, PR, or product-readiness proof was run.
historical_pre_fable_current_verdict_basis: This final review read the required governance/skill gate markers, the two unique original-task/reference attachments, the minimum Claude baseline surfaces, the active-codex plan/board/matrix/source-map/legal/gate/ledger surfaces, and current status/search evidence. End-to-end mental simulation from the active plan found no unresolved design/dependency decision that an executor would have to choose before coding/testing. The prior PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE state after 0aa/Laplace/Kepler/Singer was retained for parent final-review acceptance; this overlay supplies that final bounded review. Remaining items are execution or approval gates only: fresh Luba implementation approval, local self-authored seed file creation, code implementation, tests/builds/runtime/training/checkpoint/fresh-load/ablation proof, same-scenario Path A/Path B host proof, mock/replacement-ledger proof, branch/mergeability/git proof if later authorized, final product review, and route-specific hosted TogetherAI/Qwen3 terms/output-use proof before hosted train/common use.
historical_pre_fable_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not 50 cycles, not 50 product reviews, not product proof, not test/build/runtime/provider/training/checkpoint/git proof; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use only, not a local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE, current verdict remains PLAN_NOT_READY, CURRENT_REPAIR_QUEUE, parent must run final readiness review, or no-go boundaries before this overlay is historical/pre-final-readiness-review evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
---
artifact_type: dpo_data_training_measurement_guide
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: populated_now_no_training_run
last_updated: 2026-06-29
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
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

owner_pointer (2026-07-06, Fable v2 exec-05 dual-live-canon repair): the single measurement canon is block 11 `MeasurementReport` with typed axes `SpeedAxis` (`CpuMs`/`PeakRamMb`/`ElapsedMs`/`TrainRows`), `AccuracyAxis` (live vs held-out), `QualityAxis` (novelty/ablation/permutation), plus `SplitProof` and `TeacherProvenance` (see `active-codex-execution-plan-11-measurement.md` §5 / `codex-plan-11-measurement.md` MeasurementReport Canon, and mirror `active-codex-execution-guide-03-*`). The table below is an early superset sketch, NOT a second canon; it is abridged/evidence-only and does not redefine the block-11 axes. Families map to the canon axes as follows so no dual-live measurement taxonomy remains:

| family (this sketch) | maps to block-11 canon carrier |
|---|---|
| DPO metrics (loss, chosen/rejected reward, reward margin, accuracy) | `AccuracyAxis` (held-out acceptance) + DPO-loop metrics of block 09 |
| Graph routing (flow path, dependency order, bridge subphase) | `QualityAxis` route-quality signals (graph-routing correctness) |
| Branch quality (branch answer score, evidence grounding, conflict resolution) | `QualityAxis` (branch/answer quality) |
| Lifecycle (save, load, continue training, fresh-load evaluation) | lifecycle proof gated by `AccuracyAxis` fresh-load + block-11 lifecycle gate (not a separate axis) |
| Leakage (source overlap, candidate-order leakage, prompt memorization) | `SplitProof` + `QualityAxis` permutation/leakage checks |
| Resource (CPU, RAM, elapsed time) | `SpeedAxis` (`CpuMs`/`PeakRamMb`/`ElapsedMs`/`TrainRows`) |

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
---
---

# Active Codex Execution Addendum - DPO Legal Learning

## Current Live Authority And Evidence Boundary

This file is an active execution copy of the Claude baseline artifact. The original `claude-*` file remains unchanged. Claude remains the baseline because it is stronger by Luba criteria: human clarity, readable architecture, architectural reliability, evidence honesty, requirement fidelity, execution-control quality, and low overclaim risk. `codex-*` files are delta/source-map evidence only, not the baseline and not product-readiness proof.

Current active task source is Luba's latest live unquoted instruction for the DPO Legal Learning active plan chain. Prior sub-agent packets are evidence to verify, not live commands. The Heisenberg/no-branches/git-realignment branch of work is explicitly NOT THE TASK and must not be revived as a blocker, cleanup scope, branch operation, commit/push reason, or planning-repo git task.

Codex delta inputs retained for this addendum:

- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-EXECUTION-CONTROL-BOARD.md`: matching branch/source-map notes for `DPOLegalLearning`; not repo authority and not a source for expanding scope.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-REQUIREMENTS-COVERAGE-MATRIX.md`: matching notes for Claude latest-updated branch authority and `DPOLegalLearning` working branch/work name; not independent authority.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`: crosswalk and plan-only branch authority wording.

Do not claim Codex 50/50 cycles as proven product readiness. Ledger rows, checklist rows, generated tables, and summaries are not completed cycles unless independently inspectable review/fix evidence records exist for the exact claim.

## Active Requirement Count Strategy

The active execution requirement count strategy is:

- original Claude `R1-R20` / master `R-01..R-20` baseline remains the quality and honesty baseline;
- Claude baseline, with matching non-contradictory Codex delta evidence, defines `R21`: latest-updated implementation source-branch authority plus the `DPOLegalLearning` work name rule for implementation repo copies/workdirs under `C:\Projects`;
- current Luba execution requirements are live addendum requirements and must be checked by pre-implementation, pre-commit, pre-push, and before-stop gates;
- the prior no-branch planning-repo policy packet is quarantined evidence only and is not an active task, stop gate, branch-cleanup requirement, or permission to touch planning-repo git state.

This strategy is an execution-control strategy, not product-readiness proof.

## R21 - Source Authority, Working Directory, And DPOLegalLearning

For each participating repository/source role before implementation:

1. Identify the source authority as the most advanced/latest-updated branch in that repository at the time work starts, using read-only git commands.
2. Treat the source authority branch as read-only authority, not as the implementation work branch.
3. Because `C:\Project` does not exist and `C:\Projects` exists in this environment, use `C:\Projects` as the implementation root. Create or use a separate working directory for the work name `DPOLegalLearning`, with repo copies under `C:\Projects\DPOLegalLearning\<repo-name>` unless an existing active plan row names a more specific safe `C:\Projects` path.
4. The controlling repo-by-repo authority/source-to-plan table is `active-codex-execution-02-source-inventory-and-reference-map.md` R21.1. During future implementation, record the concrete repo path or URL, branch-list source, selected latest-updated branch, selected branch timestamp/ref evidence, divergence note, separate `C:\Projects` working directory, and work name `DPOLegalLearning`.
5. Participating Claude-baseline route/source identifiers are `llm_mvp_core` (core implementation), `xiigen-platform-dot-net-angular` (public API facade for R16/R17), `xiigen-flows-dot-net-angular` (FLOW-31/FLOW-38 internal seam), and client/caller labels `FigmaToCode-design-gen` + `CanvaToCode` (R16 html/css clients). FigmaToCode/CanvaToCode are not implementation-repository authority for the platform host unless a later exact source proves that. Codex may be used only as matching delta/source-map evidence. This list must not be expanded from discovered/copied workdirs unless Luba explicitly expands repo scope.
6. Do not switch, create, delete, rebase, merge, cherry-pick, push, or open PRs in `C:\Users\lubao\Documents\xiigen`. That planning location may be read only as an evidence source when needed; no git realignment or branch cleanup is part of this chain.

## Execution Actor And Stop Gate

Future implementation, if later authorized, must run through bounded sub-agent work packets, requested model/mode `gpt-5.5` / `xhigh` (user wording: gpt 5.5 extra high). Parent orchestrates and no unbounded executor is allowed. Each packet must name exact scope, allowed files/repos, max time, required report packet, accepted evidence, and forbidden actions. A spawned bounded sub-agent is already the delegated execution actor for its allowed scope and must not invoke nested Codex/Claude/CLI agents unless the work order explicitly asks for nested delegation. Ledger rows, tables, summaries, and self-review are not fake cycles and cannot be reported as sub-agent passes.

Stop is allowed only when the product is fully ready, or when a true external blocker remains after mitigation/discovery and decision-prep. Before any stop, the worker must check whether the active plan is complete. If incomplete, the worker must identify the loophole that would cause premature stop, repair the governing plan/gate/skill text inside the allowed scope, and continue the nearest safe authorized work. NOT READY, NEEDS_FIX, review findings, partial evidence, and missing verification are bounded work queues, not completion claims.

## Pre-Commit And Pre-Push Merge/PR Conflict Gate

Before every commit and before every push during implementation:

1. Re-read the current source authority branch for each participating implementation repo because the source branch may advance in parallel.
2. Validate that merge/PR into the original source authority branch copied from has no conflicts.
3. Record the exact command or tool evidence used for the no-conflict result.
4. If conflicts exist, stop the commit/push path, repair or prepare a decision packet, then re-check against the current source authority.
5. Do not create PRs and do not merge unless Luba gives a separate future instruction.

This gate does not authorize any branch operation, commit, push, merge, PR, or cleanup in `C:\Users\lubao\Documents\xiigen`.

## External APIs And Mock-Only Technical Debt Gate

Future implementation, if later authorized, must handle every external API/resource path through an explicit seam/interface/adapter boundary, mock the expected external response instead of making the real external call, and add a unit test that asserts exactly the expected mock response/result. Record replacement of the mock with the real external resource call only in:

`C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`

This mock replacement record is the only allowed technical debt. No other technical debt may be introduced under this active plan. Mock-only evidence must not be reported as real external integration readiness.

## Current Worker Execution Boundary

Current 2026-06-29 planning-repair boundary: Luba's latest unquoted instruction says `План не готов к выполнению - нужно починить`. This worker is authorized only to repair active plan files in this folder. Product implementation, implementation workdir setup/copying, repo inventory, tests/builds, git actions, commits, pushes, PRs, merges, and mock-debt file edits are NOT AUTHORIZED in this task. Any earlier wording that implied starting implementation after coherence is superseded until a later plan-readiness gate truly passes and Luba gives fresh implementation authority.

Source `claude-*` and `codex-*` files remain read-only. In this planning-repair task, PRs, merges, commits, pushes, branch switches, workdir copying, and implementation repo edits are forbidden. Future implementation commits/pushes may be considered only in separate implementation workdirs after plan readiness, a bounded sub-agent work order, and the mergeability/no-conflict gate pass; PRs and merges still require separate future Luba instruction.
