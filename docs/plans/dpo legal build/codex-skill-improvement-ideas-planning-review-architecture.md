artifact_type: codex_skill_improvement_ideas
authority_source: latest unquoted Luba instruction to collect improvement ideas for planning, plan review, and architecture skills
status: completed_skill_improvement_ideas
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-*.md only

# Codex Skill Improvement Ideas

## Current Task Anchor

CURRENT TASK: collect ideas for improving planning, plan review, and architecture skills based on the gap between the fast plan and the richer library/recon evidence.

NOT THE TASK: edit skill files directly, edit `claude-*` files, run implementation, run training, run tests or builds, stage, commit, or push.

EVIDENCE-ONLY: this file is a proposal packet. It does not change the actual skill definitions until Luba explicitly authorizes a skill repair.

## Problem Observed

The fast plan satisfied many formal gates but still missed a practical deliverable: the target planning library was empty at the moment the plan was first reported complete. After the library was populated, comparison with `codex-recon-*` files showed a second issue: the plan was structurally complete but did not yet absorb enough implementation evidence from real DPO, LoRA, capability training, router, autonomous learning, continuous learning, and lifecycle code.

The skills need stronger pressure against two failure modes:

1. A plan that is formally well-shaped but not backed by enough local evidence.
2. A completion report that claims the goal while a named artifact folder or library still lacks the expected files.
3. A review/status answer that dishonestly inflates ledger rows into cycles or
   parent work into sub-agent work.

## Improvement Ideas For Planning Skill

| id | idea | skill addition |
|---|---|---|
| PLAN-01 | Named artifact folder must be populated before plan completion when Luba says documents are collected there | Add `named_library_population_gate` before final report |
| PLAN-02 | A plan may need a companion library, not only a single `.md` plan | Add `planning_library_artifact_set` template with index, authority ledger, source map, legal policy, DPO guide, architecture brief, gates |
| PLAN-03 | Fast planning must mark evidence depth explicitly | Add `evidence_depth_score`: targeted, recon-library, full-pass, legal-verified |
| PLAN-04 | Large repo plans need recon micro-slices before architecture closure | Require at least one code-recon summary per critical subsystem |
| PLAN-05 | Plan must separate scaffold completeness from implementation evidence completeness | Add `shape_complete_but_evidence_incomplete` status |
| PLAN-06 | Legal/model plans need source registry from the first draft | Add `teacher_judge_source_registry` starter table |
| PLAN-07 | DPO plans need three artifacts separated | Require raw comparison event, legal DPO envelope, training batch |
| PLAN-08 | Dynamic graph plans need edge authority classification | Require each edge to say helper, learned component, or final authority |
| PLAN-09 | Trainable architecture plans need allowed-level matrix | Require mapping to `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, or `ILearningMachinePair` |
| PLAN-10 | Existing code absence must be a decision, not a hidden assumption | Require `type_exists_now` and `must_introduce_or_reuse` rows for named target types |
| PLAN-11 | Recon findings must be promoted into plan decisions | Add `evidence_promotion_rule`: path, finding, limitation, plan action |
| PLAN-12 | New library files must use the active ownership prefix | Add `artifact_prefix_resolution` field with current Luba quote |
| PLAN-13 | Questions-only-last-section requests should also apply to generated companion library | Add `question_placement_gate` to all planning artifacts |
| PLAN-14 | Planning should record artifact authorship lanes in multi-model work | Add `artifact_lane_ledger`: claude, codex, benchmark, shared |
| PLAN-15 | The plan should declare which files it will not edit | Add `negative_write_scope` under artifact queue |
| PLAN-16 | Parallel model work can add library files during a review | Add `final_library_rescan_gate` before final report |
| PLAN-17 | Planning must prevent count/actor dishonesty before prose | Add `claim_honesty_startup_gate` with `claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, and forced downgrade wording |
| PLAN-18 | Generated rows must not become fake cycles | Add `ledger_row_not_cycle_rule`: ledger/checklist/table rows are evidence inventory only, not completed review/fix passes |

## Improvement Ideas For Plan Review Skill

| id | idea | review gate addition |
|---|---|---|
| REVIEW-01 | Verify named target folder contents before accepting completion | Add exact folder listing gate for every named library path |
| REVIEW-02 | Review must distinguish plan existence from library population | Add `plan_file_present` and `supporting_library_present` checks |
| REVIEW-03 | "50 cycles" claims need real independent evidence, not ledger rows | Add `review_count_evidence_gate` requiring N evidence packets for N claimed cycles; ledger rows alone count as zero cycles |
| REVIEW-04 | Check whether later recon evidence invalidates or enriches a fast plan | Add `post_recon_delta_review` |
| REVIEW-05 | Compare shallow plan claims against deeper code-recon files | Add `benchmark_depth_delta_table` |
| REVIEW-06 | Reject "DPO exists" unless pair authoring, batching, trainer, metrics, and lifecycle are separated | Add `DPO_component_separation_gate` |
| REVIEW-07 | Reject trained-feature claims without held-out, ablation, fresh-load, continue-training, import/export | Add `trained_feature_lifecycle_minimum` |
| REVIEW-08 | Reject graph claims without explicit nodes, edges, fan-in, fan-out, edge authority, graph state | Add `dynamic_graph_shape_minimum` |
| REVIEW-09 | Reject provenance claims when core DTOs lack teacher fields and no envelope is defined | Add `provenance_storage_location_gate` |
| REVIEW-10 | Reject trainable router claims backed only by keyword scorers | Add `deterministic_authority_shortcut_gate` |
| REVIEW-11 | Reject use of `DpoPair` alone as legal DPO dataset proof | Add `minimal_pair_not_legal_pair_gate` |
| REVIEW-12 | Review should inspect benchmark/foreign files but not mutate them | Add `benchmark_read_only_write_scope_gate` |
| REVIEW-13 | Review should require "merge or reject supplement" after delta reports | Add `supplement_disposition_gate` |
| REVIEW-14 | Completion claim should include exact file list and line/byte evidence | Add `exact_artifact_readback_gate` |
| REVIEW-15 | Review should detect "future_not_done" gates and prevent ready-for-implementation overclaim | Add `future_gate_overclaim_block` |
| REVIEW-16 | Review must notice late-arriving benchmark files in shared libraries | Add `shared_library_late_file_delta_gate` |
| REVIEW-17 | Review must reject parent work described as sub-agent work | Add `subagent_actor_evidence_gate` requiring sub-agent id/nickname, work order, returned packet, reviewed scope, and parent acceptance/rejection |
| REVIEW-18 | Review must force correction when Luba challenges a claim | Add `challenge_overclaim_reset_gate`: answer first, classify prior claim as verified/overclaim/false, then repair governing skill/guide |

## Improvement Ideas For Architecture Skill

| id | idea | architecture pattern change |
|---|---|---|
| ARCH-01 | Add a trainable dynamic graph architecture skeleton | Define graph machine, graph orchestrator, graph flows, graph units, graph edge authority |
| ARCH-02 | Add graph DPO envelope as first-class architectural artifact | Capture graph path, provenance, split, legal status, near-miss reason |
| ARCH-03 | Add explicit edge authority taxonomy | deterministic helper, learned helper inside allowed level, final runtime authority |
| ARCH-04 | Add learned-claim proof packet | train rows, held-out rows, metric lift, ablation, fresh-load, continue-training, checkpoint manifest |
| ARCH-05 | Add model-component containment rule | LoRA, TensorCore, DpoTrainer, KNN, BM25, keyword scorers remain internal unless wrapped |
| ARCH-06 | Add router-specific warning | `ITrainableRoutingFlow` wrapper is not enough if inner authority is deterministic keywords |
| ARCH-07 | Add DPO authoring boundary | Pair construction is separate from DPO loss engine |
| ARCH-08 | Add legal provenance boundary | Teacher/judge/source fields live in envelope or manifest if core DTOs stay minimal |
| ARCH-09 | Add graph-state typed record sketch | nodes, edges, candidate routes, branch outputs, transition signals, evidence refs, confidence distribution, checkpoint ref |
| ARCH-10 | Add async experiment matrix | compare unit-only, flow-only, orchestrator-only, graph-policy, graph-policy-plus-state |
| ARCH-11 | Add source-disjoint split architecture | split before training, preserve split hash in manifest |
| ARCH-12 | Add self-label risk state | deterministic self-labels are allowed as baseline or seed only, not final teacher without approval |
| ARCH-13 | Add personal/global graph split | use `ILearningMachinePair` to handle private local learning versus shareable global learning |
| ARCH-14 | Add lifecycle owner decision | choose whether lifecycle belongs to graph machine, capability trainer, model repository, or checkpoint store |
| ARCH-15 | Add promotion board | legal, metric, lifecycle, no-shortcut, privacy, and provenance gates before graph promotion |
| ARCH-16 | Add deterministic graph substrate boundary | `GraphEngine`/`GraphRag` can provide graph storage and candidate retrieval, but not learned graph authority without wrappers and lifecycle |
| ARCH-17 | Add dynamic topology precedent gate | `17.DynamicDomains` can support topology mutation, but deterministic closure/Jaccard edge decisions require DPO rewrite before learned-graph claims |
| ARCH-18 | Add modelserver lifecycle extension gate | `25.ModelServer` is lifecycle infrastructure, but graph manifests and teacher-provenance fields must be added above or into it |
| ARCH-19 | Add domain prototype selection gate | choose whether first graph DPO pilot follows `42.ProjectWikiDomain` six-unit scaffold or `43.SmartSkillsDomain` full lifecycle machine pattern |
| ARCH-20 | Add architecture claim-evidence boundary | Architecture artifacts must separate `planned`, `ledgered`, `designed`, `verified`, and `completed`; no readiness claim without matching evidence |
| ARCH-21 | Add review-depth proof packet | Any architecture claim about N reviews must include N records with scope, reviewer/sub-agent id, finding/no-fix result, repair action, and recheck evidence |
| ARCH-20 | Add candidate-order leakage guard requirement | copy the guard pattern from `43.SmartSkillsDomain` into graph route DPO datasets |
| ARCH-21 | Add training-evidence/runtime split gate | preserve `43.LearningLocalWikiData` as data/registry/runner evidence and `42.ProjectWikiDomain` as runtime unit/generator implementation |
| ARCH-22 | Add weak-unit training-loop pattern | aggregate traces, select weakest unit, generate train plus held-out DPO rows, run, evaluate, update registry, publish manifest, register locator |
| ARCH-23 | Add trained-machine precedent transfer gate | borrow lifecycle from `27.TranslationDomain` only with explicit allowed-level wrappers for graph units |
| ARCH-24 | Add feature-source boundary | `22.AgentAlgorithms`, `28.SemanticNlp`, and `29.SemanticCompiler` can provide features/candidates/baselines, not learned authority by themselves |
| ARCH-25 | Add local-contract name collision warning | same-named contracts such as `OperationResult<T>` can be project-local and must not be assumed equivalent |
| ARCH-26 | Add contract-root authority gate | anchor future graph type decisions in `21.AgentInfrastructure` and avoid creating accidental sixth trainable levels |
| ARCH-27 | Add retrieval-memory boundary | embeddings, HNSW, KNN, BM25, PageRank, and GraphRAG are model components or helpers, not final graph authority |
| ARCH-28 | Add stochastic inference reproducibility gate | local `TextGenerator`/`Sampler` use in DPO or judging requires model provenance, sampling params, and seed |

## Proposed Skill Patch Shape

These are not applied patches. They are proposed snippets for a future authorized skill repair.

### Planning Skill Patch Idea

```text
named_library_population_gate:
  trigger: Luba names a folder where planning documents must be collected
  before_final_report:
    - list exact folder
    - verify expected artifact count or explain why count is unknown
    - verify non-empty files
    - verify active ownership prefix
    - link entry index
  fail_condition:
    - named folder empty
    - no index file
    - artifacts written outside active folder without explicit reason
```

### Plan Review Skill Patch Idea

```text
review_count_evidence_gate:
  trigger: user requests N review cycles
  requirement:
    - produce N explicit rows or checkpoints
    - each row must have focus, finding, action, and disposition
    - aggregate rows into accepted, rejected, deferred
  fail_condition:
    - merely saying "reviewed N times"
    - rows do not map to artifact changes or explicit no-change decisions
```

### Architecture Skill Patch Idea

```text
dynamic_graph_trainable_claim_gate:
  required:
    - allowed level wrapper named
    - graph node and edge model named
    - edge authority owner named
    - deterministic helper boundaries named
    - DPO envelope includes legal provenance
    - lifecycle proof packet defined
    - no-shortcut ablation plan defined
  fail_condition:
    - graph is only deterministic traversal
    - router uses keyword scorer as final authority
    - DpoPair has no provenance envelope
    - checkpoint exists without fresh-load and held-out proof
```

## Priority Backlog

| priority | skill | improvement |
|---|---|---|
| P0 | planning | named library population gate |
| P0 | plan review | exact target folder completion verification |
| P0 | architecture | dynamic graph trainable claim gate |
| P0 | plan review | review-count evidence ledger |
| P1 | planning | artifact authorship lane ledger |
| P1 | architecture | DPO envelope and provenance storage decision |
| P1 | plan review | deterministic authority shortcut check |
| P1 | plan review | shared library late-file delta gate |
| P1 | architecture | modelserver graph-manifest extension gate |
| P1 | architecture | domain prototype selection gate |
| P1 | architecture | local wiki runner evidence pattern |
| P1 | architecture | trained-machine precedent transfer gate |
| P1 | architecture | contract-root authority gate |
| P1 | architecture | stochastic inference reproducibility gate |
| P2 | planning | evidence depth score |
| P2 | architecture | async experiment matrix |
| P2 | plan review | supplement disposition gate |

## Second-Pass Skill Ideas From Expanded Plan Layer

These additions come from reviewing the later master plan, execution board, coverage matrix, guide set, knowledge libraries, and detailed plan blocks.

| id | skill | improvement |
|---|---|---|
| PLAN-17 | planning | Require a master-plan assembly layer when more than five supporting planning artifacts exist |
| PLAN-18 | planning | Require an execution-control board for plans with phases, subphases, or strict ordering |
| PLAN-19 | planning | Require a requirements coverage matrix with exact `covered/partial/unmapped` status |
| PLAN-20 | planning | Require every `partial` row to be repeated in final status, not buried in the matrix |
| PLAN-21 | planning | Require artifact taxonomy: master, board, matrix, block plans, guides, knowledge libraries, recon, supplements |
| PLAN-22 | planning | Require "existing code vs NEW planned type" table for every planned architecture type |
| PLAN-23 | planning | Require "no new allowed level" statement for trainable architecture plans |
| PLAN-24 | planning | Require phase IDs to be machine-checkable and gap-free |
| REVIEW-17 | plan review | Reject full completion when coverage matrix contains any `partial` unless Luba explicitly accepts partials |
| REVIEW-18 | plan review | Check that review-cycle claims are backed by real before/after packets, not just a protocol section |
| REVIEW-19 | plan review | Compare master plan status against execution board status and fail on mismatch |
| REVIEW-20 | plan review | Check that all questions are centralized in the final questions section when requested |
| REVIEW-21 | plan review | Verify that every plan block has UML, simulation, algorithm, code references, status, gates, and tests |
| REVIEW-22 | plan review | Verify that Gate B rows are measurable and not vague acceptance prose |
| REVIEW-23 | plan review | Require "plan-only, no implementation approval" to appear in final delivery when true |
| REVIEW-24 | plan review | Add moving-library cutoff reporting for shared folders with active parallel writers |
| ARCH-29 | architecture | Require explicit `ILearningDynamicGraph = ILearningMachine<DynamicGraphDomain>` or equivalent mapping before design proceeds |
| ARCH-30 | architecture | Require `GraphDpoPair` canon when multiple DPO row formats exist |
| ARCH-31 | architecture | Require `TeacherProvenance` canon and denylist/allowlist policy in DPO legal builds |
| ARCH-32 | architecture | Require per-edge authority owner and baseline-vs-trained replacement blueprint |
| ARCH-33 | architecture | Require lifecycle probes in the architecture, not only in test plan |
| ARCH-34 | architecture | Require live metric versus held-out metric separation in measurement designs |
| ARCH-35 | architecture | Require app/API seam blocks for user-facing improvement loops such as better-output-to-DPO |

## Final Boundary

This file is an improvement-ideas artifact only. It should not be treated as an applied governance or skill repair. A future skill repair needs a fresh Luba instruction naming the exact skill files to change and the allowed write scope.
