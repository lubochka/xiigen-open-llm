# Trainable Architecture Decision Skill

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include explicit skill-load guidance and the complete schema below. A generic prompt that only names a target file and broad instruction is invalid. If a sub-agent receives a generic prompt, the parent must interrupt and replace it before accepting work.

Required skill load guidance for DPO/DynamicGraph plan repair:

```text
SK-SESSION: planning-session-startup-SKILL.md + SESSION-START-PROMPT.md + AGENTS.md
SK-INTENT: architect-behavior-classifier-SKILL.md
SK-PLAN: authority-requirement-binding-SKILL.md + reference-inventory-and-source-map-SKILL.md + phase-planning-SKILL.md
SK-REVIEW: plan-review-SKILL.md + goal-delivery-completeness-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md + specificity-calibration-SKILL.md + output-skepticism-SKILL.md + reconnaissance-gate-SKILL.md
SK-ARCH: trainable-architecture-decision-SKILL.md + no-product-decisions-SKILL.md + output-readability-gate-SKILL.md
SK-DOC/PARTA when plan touches existing code: implementation-doc-uml-SKILL.md + implementation-doc-algorithms-SKILL.md + implementation-doc-code-refs-SKILL.md + implementation-doc-status-SKILL.md + implementation-doc-hierarchy-SKILL.md
```

If a work order lacks this explicit skill block or an equivalent current-task-specific skill block, it is invalid and must be interrupted/replaced.

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
original_task_requirements_to_cover:
mental_simulation_required:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

Good sub-agent prompt: names exact skills, exact attachments, exact active files, exact requirements, good/bad examples, pass count, nonparallel rule, verification commands, return schema, and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix active-codex plan readiness` or `Search NOT_READY and fix wording` without skills, task attachments, examples, coverage matrix, and simulation gates.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.
Later stricter full-task coverage evidence dominates earlier narrow ready verdicts. If any later sequential simulation, reviewer packet, or attachment reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers for the same plan, the agent must immediately downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim. The downgrade must be propagated to active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim; a plan-only wording patch is incomplete.

NEWER_PLAN_PARTIAL_RESIDUE_DOWNGRADE_GATE: Newer Claude, other-model, attachment, or reviewer evidence that reports PLAN_PARTIAL, NOT_READY, or remaining plan-level residues for the same active plan supersedes older READY or PLAN_READY_FOR_LUBA_REVIEW wording. Labels such as `trivial`, `accepted stop`, `v1-compatible residue`, `only polish`, or `non-defect` cannot preserve current PLAN_READY when Luba asks to continue to full coverage. Downgrade active readiness to PLAN_NOT_READY, keep implementation authorization NO, and convert each residue into a bounded repair queue until it is reconciled and re-reviewed.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

Pass06 architecture authority guard: Architecture decisions must distinguish deterministic baseline from trainable authority. A deterministic helper, traversal, adapter, or fixture may be a safety/reference baseline, but it cannot silently become runtime/trainable authority; trainable authority must name the owning unit, learned signal, state/checkpoint boundary, provenance, evaluation, fallback, and DPO pair handoff.

Readiness-repair work orders must name the skills, source attachments, active files, original-task requirements, good/bad examples, pass-count and evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Claude Baseline Plan Authority Gate

Execution cannot start until active-codex contains all Claude baseline details, references, and explanations plus only validated Codex deltas.
Claude baseline defines repo scope unless Luba explicitly expands it.
Discovered/copied workdirs are not repo authority.
50-pass plan authority review requires 50 independently inspectable evidence records before claiming completion.
Missed reference propagation is a planning-gate failure, not an execution question.

Application: for Claude/Codex/active-codex planning repair, Claude is the baseline authority; Codex may only add validated non-contradictory deltas/source-map details. Any active-codex open question answered by Claude or Codex evidence must be downgraded from open to resolved/partial with exact source evidence before execution work can be issued.

Use this skill whenever a plan, review, code change, dataset, report, or user
question mentions trainable, learned, semantic, adaptive, DPO, selector, router,
classifier, scorer, retrieval, KNN, dictionary, counter, pattern matching,
`TryDecide*`, `ChooseBestPattern`, `SelectBestPattern`, or
`CountMatchingSignals`.

## Purpose

Prevent deterministic or counter-based implementation details from being
accepted as new trainable architecture patterns.

## Luba Plan-Quality Criteria / Architecture Comparison Gate

For trainable, DPO, learned, adaptive, semantic, DynamicGraph, or architecture
selection work, a "better plan" is the plan that best satisfies Luba's
criteria: human clarity, readable architecture, architectural reliability,
evidence honesty, requirement fidelity, execution-control quality, and low
overclaim risk. A newer ledger, status table, file count, or claimed pass count
is delta evidence only and cannot outrank architecture quality.

The architecture stack must be loaded before comparison or execution:
SK-INTENT, SK-PLAN, SK-REVIEW, and SK-ARCH. If not loaded, say so and load it
before plan choice, executor work order, status, or implementation.

This skill must be used with
`human-readable-architecture-audit-SKILL.md` when the output is a plan, review,
code review, documentation section, or audit report for Luba/architect review.
The architecture decision must be readable in the operator's language (plain
language; Russian for the original author) unless Luba asks otherwise; machine labels are secondary.

For every deep architecture/trainable block, require a solved architecture block
instead of only audit findings or a replacement matrix:

```text
Solved Architecture Block:
  problem:
  selected_design:
  rejected_alternatives:
  UML:
  data_flow:
  current_code_refs:
  future_files_classes_methods_signatures_dependencies:
  deterministic-vs-learned boundary:
  gates_and_tests:
  failure_modes:
  docs:
```

If deterministic-vs-learned boundary is not relevant, write explicit `N/A` with
the reason. Do not leave the boundary implicit.

When a trainable architecture decision supports a host extension, runtime path,
or product-readiness claim, include the Dual-Path Host Verification Rule in
`gates_and_tests`. The Automated
boundary path must cover model logic, DPO/training, token measurement, quality
scoring, backup/export/import/share/continue-training/rollback, checkpoint and
locator gates, plus adapter/API/core request-response. The Local desktop path
must run the same scenario with an agent in the real installed host, using a
visible marker, screenshot/video/log evidence, and exact blocker if UI control
is impossible. JSON-only, adapter/core-only, test-only hook,
process/window-list, or synthetic helper UI cannot replace Local desktop proof;
screenshot-only cannot replace Automated boundary proof. A blocked Local
desktop path downgrades the claim and continues other proof paths; it is not a
stop gate.

## Graph-Style / DynamicGraph Reconstruction Gate

When the system is graph-like, DynamicGraph-based, multi-node, or routes
trainable decisions across edges, the architecture decision is incomplete until
it reconstructs the graph:

```text
roles_and_hierarchy:
DynamicGraph nodes:
DynamicGraph edges:
input_contracts:
output_contracts:
many_to_many_outputs:
fan_out:
fan_in:
multi_output_edge_contracts:
async_graph_execution:
deterministic_vs_trainable_authority_on_edges:
hidden_deterministic_shortcut_check_tied_to_each_edge:
```

Every edge must name who owns final authority: deterministic helper,
trainable unit, flow, orchestrator, machine, or pair. Hidden deterministic
shortcuts on an edge are `FORBIDDEN_RUNTIME_SHORTCUT` unless explicitly bounded
as eval-only/helper/internal baseline with ablation or no-shortcut tests.

## Graph-DPO Artifact Triad Gate

For Graph-DPO, graph-policy, DynamicGraph, or graph-path training, no DPO row is
train-ready until the artifact triad exists:

```text
raw_comparison_event:
  graph_path_ids:
  prompt_or_request_id_if_allowed:
  source_license_authority:
  candidate_order_hash:
  near_miss_reason:
legal_provenance_dpo_envelope:
  source:
  license:
  authority:
  allowed_use_decision:
  split_id:
  terms_status:
training_batch:
  batch_id:
  graph_manifest_id:
  checkpoint_hash:
  model_id:
  sampling_params:
  seed:
  access_mode:
```

The graph manifest lifecycle must name `manifest_id`, source cutoff, late-file
queue, schema version, graph terminology, substrate, path-id strategy,
export/import, backup, fresh-load, continue-training, and rollback. A graph term
such as node, edge, path, event, policy, or substrate must not be reused with two
meanings in the same plan.

Anti-forgetting, replay, or continue-training claims are allowed only after the
first source-disjoint held-out graph-policy lift is measured against a frozen
baseline. Before that lift, replay data is evidence/eval-only, not training
authority.

## Legal Teacher / Provenance Gate

DPO or distillation teacher outputs can train or rank a model only when the
teacher is an allowed hosted open-weight/permissive model and provider terms
plus model license were checked. Claude, OpenAI, Gemini, and Gemini Grounding
outputs are prohibited as distillation teachers.

Required provenance fields:

```text
teacher_access_mode:
hosting_provider:
model_id:
model_license:
provider_terms_checked:
teacher_output_used_for_training:
closed_proprietary_model:
request_id/model_version when available:
```

DPO, judge, or candidate generation that uses stochastic sampling must record
prompt/request id where allowed, sampling params, seed, checkpoint hash, access
mode, model id, and license/terms status. Missing reproducibility fields make
the row eval-only until repaired.

If `closed_proprietary_model=true` and `teacher_output_used_for_training=true`,
the verdict is always `DATASET_REBUILD_REQUIRED` for DPO/distillation teacher
data. No Luba legal exception may permit closed proprietary teacher outputs as
training targets for DPO or distillation. When Luba explicitly allows them,
closed proprietary outputs may be used only as non-training judge/eval evidence,
critique, or discussion material; they must not become chosen/rejected
distillation targets, DPO labels, ranking targets, or training rows.

The only allowed trainable architecture levels are:

- `ILearningMachine`;
- `ILogicalOrchestrator`;
- `ILogicalFlow`;
- `ILogicalUnit`;
- `ILearningMachinePair` for a common plus user-adaptive
  two-leg machine.

Generic interface signatures such as `ILearningMachine<TDomain>`,
`ILogicalFlow<TInput>`, `ILogicalUnit<TInput,TOutput>`, and
`ILearningMachinePair<TPersonal,TGlobal>` may appear only as implementation
contract signatures. They do not create additional allowed trainable levels.

Everything else is an implementation detail unless it is wrapped by one of
those levels and has the required lifecycle evidence.

KNN/retrieval note: KNN is audit-triggered, not forbidden. It may be a normal
model architecture component, memory layer, nearest-neighbor component,
candidate retriever, or feature source inside an allowed level. The violation is
not "KNN exists"; the violation is accepting standalone KNN/retrieval as the
whole trainable architecture, or using it as unlabelled final authority where a
trained unit was required.

## Decision Procedure

For each capability:

1. Name the user-facing capability in plain language.
2. Name the claimed architecture pattern.
3. Read the actual code path and identify the mechanism:
   - trained state;
   - dictionary/counter state;
   - scorer;
   - selector;
   - classifier;
   - router;
   - KNN/BM25/Jaccard retrieval;
   - regex/keyword/pattern matching;
   - private `TryDecide*` or `ChooseBestPattern` method.
4. Map architecture ownership to exactly one allowed level:
   - `ILearningMachine`;
   - `ILogicalOrchestrator`;
   - `ILogicalFlow`;
   - `ILogicalUnit`;
   - `ILearningMachinePair`.
5. If the mechanism is not an allowed level, classify it as `internal detail
   only` and name the enclosing allowed level that owns it.
6. Decide whether it is runtime authority or only candidate generation, audit,
   fixture, corpus, or evaluation helper.
   If it is deterministic and kept, label it explicitly as eval-only, helper,
   internal detail, or baseline. It must not be runtime/trainable authority for
   a semantic or learned claim.
   The same boundary must appear in the solved architecture block, or be marked
   `N/A` with a reason when no learned component is relevant.
7. Decide whether the report or plan claims semantic quality.
8. Check lifecycle evidence:
   - training data;
   - learning signal;
   - evaluation;
   - export/import;
   - manifest;
   - checkpoint;
   - locator;
   - fresh-load;
   - continue-training;
   - ablation or no-shortcut gate when deterministic helpers exist.
9. Produce a verdict.

## Minimum Breadth Challenge

After focused findings in an architecture/trainable audit, run a second pass
over adjacent trainable/runtime authority zones before any sufficiency or
completion wording.

Check at minimum:
- confidence claims and confidence floors;
- export accuracy claims;
- target-label leakage;
- runtime routing confidence floors;
- source/path/provenance leakage;
- fallback-to-first-entry and default-candidate shortcuts.

Parent review must ask and answer:

```text
What would a broader audit likely find that this artifact missed?
```

If the answer names a likely missed BLOCKING/HIGH area that should be
discoverable from the current repository evidence, the artifact cannot claim
"Architecture audit sufficient/complete". It must either audit that zone now or
bound the wording to "v1 scope completed: <exact scope>".

Delta-Severity Feedback Gate: if a later same-domain audit finds BLOCKING/HIGH
issues in these zones, record why the previous trainable review missed them and
patch the relevant skill/guide/index/agent rule before accepting the next
architecture decision.

## Verdicts

- `OK_ALLOWED_LEVEL`: implemented through an allowed level with evidence.
- `OK_MODEL_COMPONENT`: KNN/retrieval/scoring component is part of an allowed
  trainable model architecture and its role is named honestly.
- `OK_INTERNAL_DETAIL`: deterministic helper is internal and honestly labelled.
- `BASELINE_MUST_BE_LABELLED`: useful baseline, but not semantic quality.
- `NEEDS_ILOGICALUNIT_WRAPPER`: claimed unit is not an actual unit.
- `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER`: routing or sequence logic is not
  at an allowed orchestration level.
- `FORBIDDEN_RUNTIME_SHORTCUT`: deterministic or retrieval helper is unlabelled
  final runtime authority where a trained unit was required.
- `REPORT_CLAIM_TOO_STRONG`: evidence is weaker than the claim.
- `DATASET_REBUILD_REQUIRED`: rows teach shortcut or leakage instead of
  learning.

## Required Review Table

```text
capability | file/class/method | claimed pattern | actual mechanism |
allowed architecture level | non-level classification | runtime authority |
semantic claim | lifecycle evidence | verdict | required fix
```

A finding that names only `required fix` in generic language is incomplete.
For every suspicious code quote, the plan/review must immediately add a
concrete repair row:

```text
current code quote | what it computes today | why that signal is wrong or weak |
replacement allowed level | replacement class/unit name | replacement algorithm in plain language |
code-shape change | affected training sets | set rebuild rule |
affected tests | test repair/new tests | documentation/report wording change
```

The replacement algorithm must be specific enough that an executor can implement
the correct shape without inventing the architecture. For example, for a
candidate-position counter the repair cannot say only "make it provenance-only".
It must say whether the counter is removed from final authority, moved to a
diagnostic section, or replaced by a concrete `ILogicalUnit` such as a
candidate-order leakage guard.

Human-readable format is mandatory. For every problem quote, write:
`now`, `why bad/ok`, `what it should become`, exact files/classes/records/
methods/data rows, how the unit trains/works, sets/checkpoints/manifests,
tests/e2e/qa, and lifecycle. A later appendix does not satisfy this if the
quoted problem has no adjacent solution.

## Dictionary-to-ILogicalUnit Replacement Matrix

When a finding quotes a learned dictionary, counter, map, or private decision
method, produce one row per field/method. Do not group several dictionaries into
one vague "make trainable" action.

Required row:

```text
current field/method:
what it computes today:
why it is weak/leaky:
replacement ILogicalUnit:
input contract:
output contract:
typed trained state:
training update algorithm:
KNN/nearest methods:
KNN/nearest role: model component | support evidence | diagnostic only | forbidden
DPO/source roots:
external-source license gate:
pair-building rule:
forbidden signals:
load order:
backup/export/import/share:
continue-training:
affected model/checkpoint assets:
affected training sets:
set rebuild/quarantine:
tests:
dashboard/API QA scenario:
docs/report wording:
old method replaced:
```

This row is a checklist only until it is filled with concrete values. A plan or
review must not accept a row that still reads like a prompt to the executor.
For every quoted mechanism, the filled row must be placed directly beside the
quote and must name the exact replacement files, classes, records, method calls,
state fields, DPO examples, KNN method signatures, tests, and lifecycle paths.
The adjacent solution must also connect to a solved architecture block that
states the selected design, rejected alternatives, data flow, deterministic vs
learned boundary, gates/tests, failure modes, and docs.

Minimum filled blueprint:

```text
old code quote:
current runtime calculation:
replacement files to create/change:
new ILogicalUnit class signature:
new input/output records:
new typed trained-state record and fields:
old field/method removed, migrated, or diagnostics-only:
new runtime call chain:
training update algorithm:
  input example:
  state mutation:
  checkpoint save:
KNN/nearest methods:
  signature:
  role: model component | support evidence | diagnostic only | forbidden
positive DPO row:
  source:
  input:
  chosen:
  rejected:
  teaches:
rejected DPO row:
  wrong signal:
  why chosen is better:
quarantine row:
  leakage reason:
  rebuild action:
affected sets:
tests:
  exact test name:
  assertion:
load/cache/repo lifecycle:
backup/export/import/share lifecycle:
continue-training lifecycle:
source/license examples:
  candidate source:
  license file/ref to verify:
  common vs user-adaptive leg:
```

Hard fail phrases in a supposed solution:
- "row schema" without filled row examples;
- "chosen/rejected/quarantine" without actual values and leakage reason;
- "add KNN" without method signatures and role;
- "wrap in ILogicalUnit" without class signature, input/output records, and
  typed state fields;
- "continue training" without loading the existing checkpoint first;
- "use MIT/Apache sources" without source-family examples and license-file
  verification.
- any schema-only or prompt/spec row that tells a future executor to invent the
  replacement instead of naming concrete files, classes, records, methods,
  tests, data rows, assets, and lifecycle paths. Such a row is not a
  solution.

The replacement state must be a named typed record, not another raw nested
dictionary. A nested dictionary may remain only as a private storage detail
inside that typed state when the public architecture names the state fields,
normalization rules, provenance fields, and lifecycle.

`ILogicalUnit` lifecycle checklist:

- load user/cache checkpoint first;
- load repo/common checkpoint second as fallback;
- return honest untrained/scaffold status if neither exists;
- backup/export/import/share user state without losing provenance;
- continue training by loading the existing checkpoint, not retraining from
  zero;
- preserve common and user-adaptive legs separately when the sphere requires
  `ILearningMachinePair`.

External learning material checklist:

- state whether the material trains common or user-adaptive leg;
- name source, license, and allowed fields;
- prefer MIT or Apache-2.0 sources for common learning;
- reject unknown-license, GPL, or private material for common learning unless
  explicitly approved;
- keep project-specific code-execution recipes out of universal orchestration
  DPO unless the target machine is explicitly project-specific.
- give concrete candidate source examples and verification steps. Acceptable
  examples are not just "MIT/Apache material"; they must name an upstream URL or
  local archive, a license file/reference, allowed fields, and the universal
  skill taught. Candidate families include Kubernetes docs repositories, Apache
  Airflow docs/tutorials, VS Code extension samples, and similar verified
  MIT/Apache-2.0 repositories. If the license file cannot be verified, the row
  is evidence-only or user-adaptive/private, not common training.

Concrete SkillSelection replacement example:

| Current state | Replacement unit | Typed state | Training source | KNN/nearest role | Tests |
|---|---|---|---|---|---|
| `_candidatePositionWeights` | `CandidateOrderLeakageGuardUnit` | `CandidateOrderLeakageGuardState` with permutation counts, blocked features, training row ids | candidate-list permutation DPO rows | diagnostic only; no skill selection authority | permutation invariance, no-position-authority fresh load |
| `_promptUnderstandingSignals` | `PromptUnderstandingSkillUnit` | `PromptUnderstandingSkillUnitState` with prompt feature vocabulary, skill vocabulary, weights, calibration, KNN memory, provenance | prompt paraphrase DPO pairs from user prompts, skill docs, approved references | nearest prompt examples support the decision; not exact prompt replay | paraphrase stability, no exact prompt/source id leakage, ablation |
| `_taskUnderstandingSignals` | `TaskUnderstandingSkillUnit` | `TaskUnderstandingSkillUnitState` with task feature vocabulary, task kind vocabulary, skill weights, KNN memory, provenance | structured task rows from PromptToTask and approved orchestration plans | nearest task examples support task boundary | near-miss task boundary, task paraphrase, ablation |
| `_promptContextSignals` | `PromptContextSkillSelectionUnit` | `PromptContextSkillSelectionUnitState` with prompt weights, context weights, skill vocabulary, KNN memory, calibration | context-switch DPO pairs where same prompt needs different skills under different context | nearest prompt-context examples explain context support | context switch selects different skill, prompt-only insufficient, no candidate position |
| `_intermediateChecksSignals` | `IntermediateChecksSkillUnit` | `IntermediateChecksUnitState` with check feature vocabulary, skill weights, KNN memory, provenance | review/check findings that require extra skills | nearest check examples support the missing-skill decision | intermediate finding selects required skill, no source id leakage |

## Required Dataset Table

```text
set or row | intended machine | chosen signal | rejected signal |
leakage risk | project-specific contamination risk | verdict | rebuild action
```

Use these dataset verdicts:

- `KEEP_FOR_TRAINING`;
- `KEEP_FOR_BASELINE_ONLY`;
- `KEEP_FOR_EVAL_ONLY`;
- `REWRITE_TO_DPO`;
- `QUARANTINE_DO_NOT_TRAIN`;
- `WRONG_DOMAIN_REMOVE_FROM_THIS_MACHINE`.

## Human-Language Rule

Every finding must first explain in plain language what the code actually does.
Then give the exact short code quote and the architectural consequence.
Then give the concrete replacement algorithm, affected sets, affected tests,
and the "current -> target" repair.

Example:

```text
The code learns integer weights from DPO rows and later ranks skills by those
weights. That is a learned counter baseline inside SkillSelectionMachine. It is
not by itself a new semantic unit.

Quote: "profile[signal] = profile.GetValueOrDefault(signal) + 1;"
Consequence: acceptable only as internal state unless wrapped/evaluated as an
allowed ILogicalUnit or honestly reported as baseline.
```

Concrete repair example:

```text
Current quote: "private readonly Dictionary<int, int> _candidatePositionWeights = new();"
Current behavior: learns that the skill at candidate position N was often chosen.
Problem: candidate order is not task meaning; using it for final selection is
order leakage.
Replacement level: ILogicalUnit.
Replacement unit: CandidateOrderLeakageGuardUnit.
Replacement algorithm: run the same request through two or more candidate-order
permutations; if the selected skill changes only because order changed, return
order_sensitive=true and block position features from final authority.
Code-shape change: remove candidate-position weights from final selection;
store old weights only under diagnostics/provenance, or delete after migration.
Affected sets: quarantine or rewrite rows where chosen can be predicted from
candidate index; train-ready rows must include permutation pairs where chosen
skill is stable across candidate order.
Affected tests: add candidate permutation invariance, source/pair id leakage,
fresh-load no-position-authority, and report-wording tests.
```

## Subagent-Only Firewall Compatibility

Trainable architecture repair and review must respect the Parent Architect
Subagent-Only Firewall:

```text
Parent Architect Subagent-Only Firewall
parent local sidecar work ban
read-only verification after a sub-agent packet returns
sub-agent waiting is not a stop
no local non-overlapping work escape
```

When Luba's current instruction requires sub-agent-only, architect-only, or
governance-only operation, the parent/current session must not directly author
trainable architecture repairs, skill patches, plans, reports, code, data, or
protocol text. The parent may review and integrate only after the sub-agent
packet returns, and waiting for that packet is not a stop gate while a safe
bounded next action exists.

## Question Handling

If Luba asks "is this hiding deterministic logic?", answer as classification,
not as defense and not as repair.

Use this order:

1. classify the mechanism;
2. say whether it is allowed as architecture or only as internal detail;
3. say what evidence would make it acceptable;
4. list affected code, sets, tests, and documentation to audit.

Do not start code repair from this question unless Luba explicitly asks for the
repair after the classification.

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: For trainable/DPO/DynamicGraph plans, latest inspectable Claude cleanup/resimulation plus original-task coverage simulation dominates older active-codex plan-polish/status. Active-codex cannot claim readiness, B-22 closure, or implementation authorization until every original-task requirement is mapped and every latest inspectable baseline detail is reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, repo-host roles, and deterministic-vs-trainable authority decisions must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

## Structural-Invariant Decision Gates (additive)

These gates extend the classification catalogue above with architecture-level
structural invariants for auto-DPO / ILearningDynamicGraph plans. They are the
skill-side enforcement of CLAUDE.md "Trainable architecture structural-invariant
pattern" and AGENTS.md OP-66. Apply each as a classification/decision check
before a threshold, teacher mode, or pilot is locked.

SI_GATE_1_SHARED_CONTRACT_SINGLE_OWNER_SWEEP: any change to the field count,
names, or types of a shared typed contract (consumed by >1 block/carrier) must
sweep EVERY carrier in the same decision — owner §5.2 record, every consumer §0
render, every text-canon mention, and every measurable predicate whose
denominator is that count (`provenance_field_presence_rate=20`,
`triad_field_presence_rate=3`). A stale count anywhere is a FAIL. A §0 subset
render needs an abridgement note (owner full count + owner location + literal
"read-only subset, full canon §X").

SI_GATE_2_CROSS_FILE_OWNER_RESOLUTION: a consumer naming a type/unit and
attributing it to an owner block must resolve to that exact type at the owner
with matching NAME + architectural LEVEL (`ILogicalUnit` vs helper) + I/O TYPE.
Mismatch (e.g. `IGraphDpoPairBuilderUnit(GraphDpoBuildInput)` vs
`GraphDpoBuilder(GraphDpoSourceRow)`) or an unresolved delegate/owner pointer is
a FAIL.

SI_GATE_3_REWARD_SCALARIZATION_FOR_NON_SCALAR_SIDES: a trainable unit whose
chosen/rejected side is a sequence/ranking/path/plan/structure must carry a
reward-scalarization rule (multi-element output -> one scalar reward for the DPO
margin) beside the training-update algorithm. A margin primitive without a
defined `reward(side)` extractor for the actual side type is admission-blocking.

SI_GATE_4_CARDINALITY_FAN_OUT: an N>1 candidate producer feeding a single
`rejected` field must name the projection rule (winner-vs-each-loser /
winner-vs-single-worst / other) + single owner file + line-by-line
`producer-output -> SourceRow[]`. Plural-losers prose over a single-valued field,
or a 1-vs-1-only simulation when N>2 is locked, is a FAIL.

SI_GATE_5_ORDER_FEATURES_CARRIER: for a trainable order/sequence/ranking
capability, the record carrying the order/sequence features into and out of
training must exist and be named in the same decision; absence is admission-
blocking (no ordering signal to read). A candidate-position/order counter must
never be final selection authority.

SI_GATE_6_YBEST_DETERMINISM_CLASSIFICATION: the `Y_best` selection (source of
`chosen`) must be classified as deterministic eval-only baseline OR trainable
authority, with edge-authority strictness. An unclassified winner-selector (fixed
confidence formula) silently becoming final authority is a back-door R4/A-LM-09
bypass FAIL.

SI_GATE_7_ACCEPTANCE_SIGNAL_INDEPENDENCE: check
`acceptance_signal_source != trained_unit_self_confidence`. The arbiter number
deciding ACCEPTED/REJECTED must not originate from the self-reported confidence
of the same trained unit whose choice becomes chosen/rejected (circular
self-labelling FAIL). If both a deterministic winner-selector and a judge-quality
axis could author the same `chosen`, exactly one is authority and the other is
eval/critic-only; both unreconciled is hidden-authority ambiguity.

SI_GATE_8_THRESHOLD_SCORER_INPUT_COMPLETENESS: before a threshold/floor is
LOCKED, the scorer input(s) feeding every gated branch must be chosen and
instantiated (metric + gold + comparison + expected-label origin). For a single
threshold over a signal whose carrier may be trained AND deterministic, require a
calibration-comparability invariant OR an explicit per-carrier/tunable note.

SI_GATE_9_PROVIDER_NAME_ROLE_DISAMBIGUATION: when one name is both a
teacher-family and a neutral hosting provider (Moonshot/Kimi, DeepSeek, Z.ai),
separate first-party vendor hosting from neutral-reseller hosting and record the
effect on `terms_snapshot`/output-use.

SI_GATE_10_FAMILY_LICENSE_COHERENCE: C4 (identity/allowlist-match) and C5
(declared-license-permissive) must be joined by a registry-carried
`family -> canonical_license` invariant, and the declared `model_license` must be
cross-checked against it (extend R2 distrust to the license field). The C5
permissive set enumerates exactly the license of every allowlisted family (no
ellipsis); a non-trivial license (`modified-MIT` ≠ `MIT`) needs an explicit
set-membership + attribution-notice decision; heterogeneous families need a
heterogeneity flag. Each allowlist family must be RESOLVED (ProviderConfig +
canonical license + terms fate) or explicitly deferred with that literal at every
mention; named-but-not-instantiated is a false-ACCEPT laundering FAIL.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.

### Model/Checkpoint Lifecycle Decision Gate

For any trainable, DPO, DynamicGraph, adaptive, learned, or semantic architecture plan, model/checkpoint lifecycle is a design decision, not a future test placeholder. Before readiness or implementation authorization, the plan must name:

- the `deterministic baseline` code/config/traversal surface and the separate `learned policy`/checkpoint state surface;
- the checkpoint manifest contract, including at minimum `model_id`, `checkpoint_id`, `graph_schema_version`, `code_source_ref`, `scorer_version`, `threshold_set`, `training_data_refs`, `provenance_refs`, `legal verdict`, `TeacherProvenance`, `scope`, `created_at`, `parent_checkpoint_id`, and `compatibility_status`;
- the persistence/export/import path for common/global and personal/adaptive artifacts, with checkpoint state, manifest, and parent lineage carried together;
- the fresh-load evaluation gate: clean-process import/load, held-out evaluation, RuntimeProductAccuracy, reward margin, delta, ablation, threshold/scorer set, compatibility, baseline comparison, and no-regression check before product-ready or promotion claims;
- the promotion gate: legal/provenance/source split pass, held-out thresholds, ablation, no regression, no provider terms pending, recorded source branch/ref, and manifest compatibility PASS before common/global promotion;
- rollback and continue-training rules: previous accepted checkpoint remains rollback target; continue-training must load parent checkpoint, record `parent_checkpoint_id`, use approved source-disjoint new rows/refs, and write a child manifest.

If any lifecycle item is absent, ambiguous, hidden in a future test/build gate, or treated as in-memory training success, verdict is PLAN_NOT_READY. Route failures to bounded repair or a true external blocker after mitigation; do not claim product readiness, promotion, or implementation authorization.
