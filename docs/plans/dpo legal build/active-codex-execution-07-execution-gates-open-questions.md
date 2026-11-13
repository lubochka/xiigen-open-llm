## Current Code-First Reconciliation Overlay (2026-07-14)

current_implementation_authorization: YES_CODE_FIRST_ONLY
teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL
dpo_status: NOT_STARTED
dpo_priority: LAST
current_counters: 12 phases / 27 subphases / 64 substeps
canonical_sequence: CODE_FIRST -> TP1_LOCAL_EVAL_ONLY -> TP2_HOSTED_EVAL_ONLY (only after G-TP-4) -> G_DPO_FINAL -> DPO_TRAINING
current_readiness_boundary: code-first execution is authorized, but no DPO execution readiness or whole-plan readiness is claimed. TP1 and TP2 are evaluation-only; neither starts DPO.
provider_gate_separation: `APPROVED_BY_LUBA_2026-07-07` is the hosted-Qwen business approval and is not `ProviderTermsProof`; `G-TP-4` is the separate technical precondition for TP2.
current_future_phase_residuals: `Q-TP-NOGOLD`, the numeric `Q-TP-235B-EVAL` criterion, reference-checkpoint, reward-margin, held-out/source-disjoint evaluation, lifecycle/fresh-load/continue-training, and quantization remain future teacher/DPO-phase owners. They do not block the current code-first lane and they do not authorize DPO.
historical_reconciliation_rule_20260714: all TP5, S17-B04, Fable, pre-Fable, 43c2, and Pass50 readiness/authorization labels below are preserved as historical evidence and cannot override this overlay.

## Historical TP5 Two-Phase Gates & Open-Questions Overlay (2026-07-13)

historical_tp5_plan_readiness: PLAN_NOT_READY / TWO_PHASE_ADAPTATION_QUEUE
historical_tp5_implementation_authorization: NO until Luba says `реализуй`

Русское человеческое резюме: 2026-07-13 Luba потребовала двухфазный teacher-маршрут (Этап 1 локальный `qwen35-local-v1`, Этап 2 hosted `qwen3-together-v1`). Это новое требование появилось ПОСЛЕ S17-B04 (2026-07-07 «design complete»), поэтому readiness этого файла синхронизируется вниз к общему `PLAN_NOT_READY` (устранение stale dual-live: overlay/16/02/F0 уже несут PLAN_NOT_READY). Владелец двухфазной структуры — `active-codex-execution-two-phase-local-remote-overlay.md`.

single_value_readiness_reconciliation (CURRENT_READINESS_SINGLE_VALUE_GATE): текущий единственный readiness-вердикт семьи плана = `PLAN_NOT_READY`. Метка `TWO_PHASE_ADAPTATION_QUEUE` — живая причина очереди; старая `FABLE_GAP_REPAIR_QUEUE` закрыта по составу и несёт то же readiness-значение. Второго канона readiness нет.

execution_gates_TP: G-TP-1..G-TP-4 (двухфазные гейты) — это execution-continuation checkpoints (выполняются через bounded sub-agents ПОСЛЕ одобрения Luba `реализуй`), НЕ дизайн-блокеры и НЕ stop-разрешения. G-TP-2 (лицензия весов Qwen3.5-9B) и G-TP-4 (`ProviderTermsProof` Together ToS/ZDR/output-use) — открытые execution-гейты, не Luba-вопросы.

luba_questions_pointer: открытые Luba-решения двухфазности (Q-TP-*) находятся ТОЛЬКО в финальной секции этого файла «Two-Phase (TP1/TP2) Luba Product-Decision Residuals» и в `F0-decision-unblocker.md`. Здесь, наверху, они не дублируются как второй канон.

historical_classification_rule_TP5: вердикт S17-B04 (2026-07-07) `PLAN_READY_FOR_LUBA_REVIEW (design complete)` становится historical/pre-two-phase ТОЛЬКО в части READINESS. Пять решений S17-B04, RESOLVED_FROM_DOCUMENTED_AUTHORITY ((a) Q-P09-XSRC; (b) Q-P12-M4; (c) legal-teacher canonical-license Kimi-K2/GLM/Mistral; (d) Q-B21-FLOOR 20/40 floor MET/binding; (e) qwen3-together-v1 Together/Qwen3 RESOLVED_BY_LUBA_DECISION 2026-07-07), ОСТАЮТСЯ ДЕЙСТВУЮЩИМИ и НЕ переоткрываются. Двухфазность лишь секвенирует hosted-этап во вторую фазу и добавляет открытые Q-TP-* поверх уже принятого.

## Historical S17-B04 Documented-Authority Reconciliation & Readiness Verdict Overlay (2026-07-07)

historical_s17_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW (design complete)
historical_s17_implementation_authorization: NO until Luba says `реализуй`
historical_demoted_current_verdict_basis: This was the historical S17 TOP verdict for the execution-gates open-questions inventory. All plan-level DESIGN decisions in this chain are now RESOLVED FROM DOCUMENTED AUTHORITY (Luba's answers were already present in the task-process / legal / master documents and were EXTRACTED into the owner plan files, not chosen here): (a) `Q-P09-XSRC` auto-DPO input source = `RESOLVED_FROM_TASK_PROCESS` (original-task line 20; audit `-original-task-authority-audit-20260629.md:53`; owner `-plan-09:268-337`); (b) `Q-P12-M4` scenario phase-plan mechanism #4 = `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (master §E.0 Q-mech4, Luba 2026-06-30; owner `-plan-12:1321-1384`); (c) legal-teacher canonical-license rows Kimi-K2-`modified-MIT` (verify-before-use, NOT pre-admitted to C5) and GLM/Mistral (per-model canonical-license verification) = `RESOLVED_FROM_LEGAL_PART` (legal G1 list `-plan-09:1178-1180`; audit `-original-task-authority-audit-20260629.md:57` `RESOLVED_BY_ORIGINAL_TASK`; owner `-plan-08:148-159`); (d) `Q-B21-FLOOR` 20/40 knowledge-library floor = `RESOLVED_FROM_TASK_MINIMUM` (task «минимум 20/40 проходов», master §F.5 / Recon Library Gate; owner `-F0-decision-unblocker.md:805-857`; MET at 20/20+40/40, stays binding, no open «narrow the floor» Luba decision). No plan-level design/dependency decision remains open in this file. Bounded planning/governance reconciliation only; no product implementation, tests, builds, runtime, provider/API, training/checkpoint, git, or mergeability proof was run. This is NOT full `FULLY_READY`.
residual_to_full_readiness:
- EXECUTION_PROOF: F0 locks (implementation proof pending); R16/R17 host-route / better-output seams (route/code-location proof pending); seed-corpus creation + training/eval/checkpoint/fresh-load/ablation; R16/R17 same-scenario Path A/Path B host proof; branch/mergeability/git proof — all pending after `реализуй`.
- EXECUTION_TUNING (set during implementation, NOT open plan decisions): auto-DPO reward weights `w_lift/w_legal/w_contrast/w_leak` numeric VALUES (legal is a HARD GATE, not a soft weighted term; owner `-plan-07:1163-1211`) and the runtime confidence-floor / `enqueue_threshold` numeric value(s) per carrier (owner `-plan-09:333-337`, `-plan-02/04/06/11` confidence-floor). These numbers are genuinely not literals in the task-process/legal docs and are tuned in implementation; they are NOT invented here and do NOT hold plan-review readiness.
- EXTERNAL: Qwen3 / TogetherAI hosted provider terms + output-use proof before hosted train/common use (route-specific external pre-use gate).
historical_demoted_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_SURVIVING_ALL_DESIGN_DECISIONS_RESOLVED_FROM_DOCUMENTED_AUTHORITY; CURRENT_REPAIR_QUEUE=EXECUTION_PROOF_PLUS_EXECUTION_TUNING_PLUS_EXTERNAL; IMPLEMENTATION_AUTHORIZATION_GATE=NO until Luba says `реализуй`.
historical_classification_rule_S17_B04: Every overlay/verdict below this block — including the `Fable Coverage Repair Overlay (2026-07-05)` with `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`, the Historical Pre-Fable Final Readiness Review Overlay, the 43c2 downgrade overlay, the Pass50 overlay, and any `Q-P09-XSRC`/`Q-P12-M4` `UNDECIDED` residual line further down — is historical/pre-S17-B04 evidence only; its verdict and any `UNDECIDED` marker for the five decisions above are NOT current. The historical S17-B04 verdict for this file was: PLAN_READY_FOR_LUBA_REVIEW (design complete), implementation_authorization NO until `реализуй`.

## Fable Coverage Repair Overlay (2026-07-05) [HISTORICAL/pre-S17-B04 — superseded by the S17-B04 overlay above; see historical_classification_rule_S17_B04]

historical_demoted_plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE (HISTORICAL/pre-S17-B04 — superseded above)
historical_demoted_implementation_authorization: NO until fresh Luba approval after review
historical_demoted_current_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` supersede the previous final-readiness overlay. Fable reports 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers: R-B `llm mvp` 20-pass and `llm_mvp_core` 40-pass knowledge-library targets are accepted only as evidence-library closure, not readiness/implementation proof; R-G source addressability is incomplete; several shared contracts/readability/lifecycle carriers need plan-family repair. This is plan repair evidence only, not product implementation, not tests/builds/runtime/provider/training/checkpoint/git proof. [HISTORICAL/pre-S17-B04: the R-B/R-G/partial items named here are now CLOSED at plan level and the five design decisions are RESOLVED_FROM_DOCUMENTED_AUTHORITY per the S17-B04 overlay above.]
historical_demoted_current_verdict_classes: DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS; CURRENT_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=previous 50/50 planning-section acceptance remains historical/process evidence only and cannot override later Fable MISSING/PARTIAL findings; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, Final Readiness Review Overlay, no unresolved design/dependency blocker, CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW, or parent final-readiness acceptance before this overlay is historical/pre-Fable evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
historical_fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md is the current repair map for R-B/R-G and the partial contract/readability/lifecycle additions.

## Pass31-Pass40 Consumer Sync (40-pass llm_mvp_core evidence library)

historical_sync_scope: evidence-only propagation from `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` Pass31/40 through Pass40/40. Current verdict remains `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`; `implementation_authorization: NO`.

| gate | required later evidence | boundary |
|---|---|---|
| RB40-CONSUMED | Carry 40/40 library as closed R-B evidence target in active plan carriers. | Not product readiness, not B-22 closure, not implementation authorization. |
| EXT-MOCK-PLAN | In implementation phase, create/update `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md` for every external/provider/host/API/Figma/Canva/html-css mock with endpoint, expected response, test, replacement criterion, legal/provider gate, and debt owner. | This worker did not create the file and did not call any external provider. |
| SCHEDULER-GATE-B | Tests must verify `MaxCombinations`, `MaxParallelCombinationRuns`, `MaxParallelBranches`, deterministic ordering, timeout, cancellation, partial failure, and backpressure. | Tests are future execution gates only after approval. |
| REPO-AUTH-MERGEABILITY | Later implementation must rediscover latest-updated source branch per repo, use `C:\Projects\DPOLegalLearning\<repo>`, and perform no-write mergeability checks before commit/push. | No git/branch/copy/mergeability action was run here. |
## Historical Pre-Fable Final Readiness Review Overlay (2026-07-05)
historical_pre_fable_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
historical_demoted_implementation_authorization: NO until fresh Luba approval after review
historical_final_readiness_review_scope: one bounded planning/governance readiness review only; no product implementation, tests, builds, runtime, browser, provider/API, model/training/checkpoint, git, branch, mergeability, PR, or product-readiness proof was run.
historical_pre_fable_current_verdict_basis: This final review read the required governance/skill gate markers, the two unique original-task/reference attachments, the minimum Claude baseline surfaces, the active-codex plan/board/matrix/source-map/legal/gate/ledger surfaces, and current status/search evidence. End-to-end mental simulation from the active plan found no unresolved design/dependency decision that an executor would have to choose before coding/testing. The prior PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE state after 0aa/Laplace/Kepler/Singer was retained for parent final-review acceptance; this overlay supplies that final bounded review. Remaining items are execution or approval gates only: fresh Luba implementation approval, local self-authored seed file creation, code implementation, tests/builds/runtime/training/checkpoint/fresh-load/ablation proof, same-scenario Path A/Path B host proof, mock/replacement-ledger proof, branch/mergeability/git proof if later authorized, final product review, and route-specific hosted TogetherAI/Qwen3 terms/output-use proof before hosted train/common use.
historical_pre_fable_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not 50 cycles, not 50 product reviews, not product proof, not test/build/runtime/provider/training/checkpoint/git proof; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use only, not a local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE, current verdict remains PLAN_NOT_READY, CURRENT_REPAIR_QUEUE, parent must run final readiness review, or no-go boundaries before this overlay is historical/pre-final-readiness-review evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
## Historical 43c2 Readiness Downgrade Overlay (2026-07-02; superseded by 2026-07-14 overlay)

Historical boundary only: this section cannot override the current 2026-07-14 code-first overlay.

historical_pre_final_review_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_demoted_implementation_authorization: NO
latest_attachment: C:\Users\lubao\.codex\attachments\43c2bc75-9134-4cb2-8ad3-8985d2689bb6\pasted-text.txt
historical_evidence_scope: Attachment 43c2 reports newer Claude PLAN_PARTIAL / NOT READY evidence with two plan-level residues. It is evidence-only, not this worker's pass count, not a sub-agent packet, and not implementation proof.
historical_current_residue_queue:
- plan-09 B-15 / IGraphWeaknessDetectorUnit edge/graph wording: v1 train-ready DPO rows remain unit-level only; edge/graph training, scoring, acceptance, and promotion are deferred unless a later explicit Luba-approved plan expands scope.
- plan-06 fan-in merger wording: current canon is IEdgeFanInMerger.Merge(GraphFanInMergeInput input) -> OperationResult<GraphFanInMergeResult>, not a bare LogicalFlowResult return.
historical_pre_final_current_verdict_basis: Newer evidence attachment 0aa5730c, the Erdos bounded worker packet, and the 2026-07-05 Pass37-Pass50/Singer correction supersede stale READY and stale acceptance-count overlays. Historical/pre-Fable evidence supported PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE: current-ledger 50-pass acceptance is repaired as bounded planning-section evidence (50 headings exist and strict acceptance-field/readability evidence is 50/50 after the 2026-07-05 Pass37-Pass50/Singer repair). This queue-classification repair finds no current plan/design blocker in the stale CURRENT_REPAIR_QUEUE labels: F0 locks are recorded with implementation proof pending; legal/provenance is propagated in active plan, policy, and source-map surfaces; hosted-provider proof is a route-specific external pre-use gate before hosted Qwen3/Together train/common use, not a local self-authored v1 planning blocker; future execution proof remains after approval; fresh Luba post-review approval is implementation authorization, not a plan-design blocker. This is not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.
historical_pre_final_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not cycles/readiness/product/implementation proof; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; F0_LOCKS_RECORDED_IMPLEMENTATION_PROOF_PENDING=Q3/Q6/Q8/Q11/Q14/Q30/Q38 recorded, Q37/Q-host implementation checks only; LEGAL_PROVENANCE_PROPAGATED_IN_PLAN_EXECUTION_PROOF_PENDING=20-field TeacherProvenance, ProviderProof, LicenseProof, LegalVerdict, C16/C17 quarantine/block/rebuild behavior present in active plan/policy/source-map surfaces; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use, not local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=fresh Luba post-review approval required before execution, not plan-design blocker; EXECUTION_PROOF_PENDING_AFTER_APPROVAL=code implementation, seed file creation, tests/builds/runtime, mock execution/replacement, training/checkpoint/fresh-load/ablation, same-scenario dual-path proof, branch/mergeability/git; readiness_candidate_for_parent_review=ZERO_CURRENT_DESIGN_BLOCKERS_FOUND_BY_THIS_QUEUE_CLASSIFICATION; parent must run final readiness review before any PLAN_READY wording; HISTORICAL_PRE_0AA_EVIDENCE=older PLAN_READY_FOR_LUBA_REVIEW / READY and older NOT_READY records unless a later evidence-bound repair proves current status.
## Historical/pre-43c2 Pass50 Readiness Verdict Overlay (2026-07-02)

historical_pre_43c2_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_demoted_implementation_authorization: NO until fresh Luba approval after review
historical_current_pass_chain: 50 canonical pass headings exist and strict current-ledger acceptance-field/readability evidence is now 50/50 after the 2026-07-05 Pass37-Pass50 bounded acceptance repair recorded in active-codex-execution-41-current-ledger-50-pass-acceptance-index.md. This is 50 accepted planning pass sections, not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.

Historical/pre-43c2 Pass50 basis:
- Original-task R1-R10 are FULLY_COVERED_PLAN_LEVEL by Pass46 and this Pass50 adversarial review.
- B-1..B-22 have no remaining implementable active-codex design/dependency blocker under the Pass39/Pass43/Pass44 current overlays. Hosted provider proof remains required only before hosted train/common use.
- Source-to-Plan inventory is 64/64 by exact canonical row-id audit, with 0 noncanonical row-like offenders.
- Claude/latest-baseline details are carried in active-codex: five participating repos, DPOLegalLearning work name, C:\Projects workdir boundary, latest-updated source-branch authority as active/Codex delta, xiigen-platform facade plus xiigen-flows seam, FigmaToCode/CanvaToCode as clients/callers, unit-level 14-field GraphDpoPair, 20-field TeacherProvenance, exactly one active language-translate pilot, and provider/terms boundary.
- QA, tests, builds, screenshots, same-scenario dual-path proof, mock replacement, provider/API proof, runtime/training/checkpoint proof, branch/mergeability, git, PR, and product review remain future execution-continuation gates after fresh Luba approval. They are not design decisions and were not run in Pass50.
- This Pass50 overlay is historical/pre-43c2 evidence only. Newer attachment 43c2 supersedes it; READY wording in this block is not current readiness, not B-22 closure, and not implementation authorization.

Readback: This Pass50 paragraph is historical/pre-43c2 evidence only. Attachment 43c2 triggered a downgrade, the residues were repaired, and the historical/pre-0aa post-repair-review active-codex verdict was PLAN_READY_FOR_LUBA_REVIEW; historical/pre-Fable 2026-07-02 bounded-repair verdict was PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE; implementation_authorization remains NO.
---
artifact_type: execution_gates_open_questions
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: populated_now_questions_final_section
last_updated: 2026-06-29
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
---

# Execution Gates And Historical Question Evidence

## Pass 15 Future Gate Clarification

Future tests/builds/runtime/mock checks are executable proof gates after code exists, not current evidence and not stop permissions. Before any product-ready claim, a future worker must record exact build output, unit-level DPO test output, exact mock-response unit tests for every external API/resource seam, legal/provenance test output, measurement/scorer test output, same-scenario runtime proof, and the later `codex-external-calls-mock.md` replacement audit.

If one of those checks fails, the next route is a bounded repair worker or a true external blocker after mitigation and decision-prep. The worker must not ask Luba merely because a repairable build/test/mock/runtime check failed. Conditional rule: if provider policy, provenance policy, source-map authority, thresholds, input/output contracts, mock response contracts, replacement-ledger obligations, or model/checkpoint lifecycle decisions are missing in the active plan, that missing item is a design blocker and cannot be deferred into future gates. Pass47 did not identify those items as remaining design blockers after the Pass46 repaired baseline; the historical/pre-post-repair PLAN_NOT_READY basis is the incomplete 50-pass evidence chain and parent synthesis.

## Current Boundary

This library is a planning artifact. It does not authorize code implementation, training, tests, builds, git, external browsing, or provider terms checks.

Live-authority addendum for 2026-06-29 bounded planning repair: Luba's latest
unquoted instruction says `План не готов к выполнению - нужно починить`.
This authorizes active-plan repair only. It does not authorize product
implementation, proof-producing product work, workdir copying, repo inventory,
tests, builds, training, real external resource calls, git actions, commits,
pushes, PRs, merges, planning-repo git, or any implementation row.

F0 decision-continuation rule:

```text
F0 decision blocker is a bounded continuation queue, not a final stop
true Luba choices stay in the last section
safe non-decision workstreams/reversible scaffolding must be exhausted through subagents before final hold
final hold requires independent no-safe-work evidence
```

Open F0 choices block only dependent rows that would select the answer. In the
current task, safe bounded work means active-plan repair only. Decision-lock
sync validation, contract inventory, disabled/not-configured test harnesses,
mock-only external-call scaffolding, provenance/quarantine skeletons, product
documentation, repo inventory, and implementation validation are future work
unless a later work order explicitly authorizes them.

## Stage Queue

| stage | action | status | exit evidence |
|---|---|---|---|
| S0 | Populate planning library in target folder | done | Eight `claude-*.md` files exist |
| S1 | Parent review of exact library files | remaining | Exact file list, headings, line counts |
| S2 | Luba acceptance of library and plan direction | blocked_on_luba | Current explicit acceptance quote |
| S3 | 20-pass external-doc and provider evidence collection | remaining | Terms, license, model-card, source registry packet |
| S4 | 40-pass `llm_mvp_core` core-doc library collection | accepted_evidence_only_not_readiness | Full source map consumer sync and gap review still required; accepted 40/40 target is not readiness/implementation proof |
| S5 | Architecture plan repair or final plan write | remaining | Approved architecture plan artifact |
| S6 | Implementation work order through authorized executor or sub-agent | not_authorized_now | Fresh implementation approval quote |
| S7 | DPO pair builder and legal gate implementation | not_authorized_now | Code, unit tests, source gates |
| S8 | Training runner and measurement implementation | not_authorized_now | Held-out metrics and lifecycle proof |
| S9 | Promotion review | not_authorized_now | Legal, metric, ablation, fresh-load gates pass |

## Hard Gates Before Implementation

| gate | pass condition |
|---|---|
| library_review_gate | Exact target folder contains only accepted library docs for this task |
| authority_gate | Current Luba instruction explicitly approves implementation |
| legal_gate | Provider terms and model license checks completed before model use |
| source_gate | DPO rows have source ids, provenance, and allowed-use status |
| hierarchy_gate | Training remains on `ILearningMachine`, not on flow or unit |
| graph_helper_gate | Deterministic helpers are not presented as learned behavior |
| async_gate | Branch dependencies and parallel levels are explicit |
| metric_gate | DPO and graph metrics are numeric and source-disjoint |
| lifecycle_gate | Checkpoint save, fresh-load, import, export, and continue-training are verified |
| privacy_gate | Raw project code or private data does not leave local boundary without approval |

## Acceptance Criteria For This Library

| criterion | expected result |
|---|---|
| Required files | Files 00 through 07 exist in target folder |
| Prefix | Every created basename starts with `claude-` |
| Non-empty | Every file has substantive Markdown |
| Header block | Every file includes `artifact_type`, `authority_source`, `status`, `last_updated`, `write_scope` |
| Future honesty | Future evidence phases are marked future or not done |
| Question placement | All questions for Luba appear only in final section of this file |
| No git | No git commands run |
| No tests or builds | No tests or builds run |
| No code edits | Repository code is unchanged |

## Current Luba Decisions From Attachment 483d

Authority repair 2026-06-29: earlier broad library-bootstrap questions are
historical and superseded by the Claude-baseline 50-pass repair plus the
original-task authority audit and Opus F0 reconciliation. Claude already
answers the L5/no-sixth-level model, GraphDpoPair row/envelope semantics,
TeacherProvenance field list, teacher/judge allowlist/quarantine baseline, and
MultiSubPhaseRouterUnit baseline/ablation fate. The original task answers Q8's
container as queryable/IQueryable many-to-many, Q37's origin principle as
legal open-weight hosted teacher/self-DynamicGraph only, and Q-host scope as
both `xiigen-platform` and `xiigen-flows` with FigmaToCode/CanvaToCode clients.
The later attachment `C:\Users\lubao\.codex\attachments\483d666f-ff58-45e2-be05-121ed616cb6a\pasted-text.txt` explicitly records that Luba chose the recommended defaults. Its lines 33, 35, and 47-58 supersede the earlier proposal-only wording in this file.

No current Luba decision remains in this section for Q3/Q6/Q8/Q11/Q14/Q30/Q38 unless Luba explicitly reopens them. The active locks are:

1. Q3-storage: sidecar manifest through `LearningBatch.SourceIds`, JSONL + `schema_version`; common under `llm_mvp_core/models|training-data`, adaptive under `<project>\.xiigen`.
2. Q6-assets / B15 current Pass35 policy: v1 is common-only. Common remains in `llm_mvp_core` (`25.ModelServer` + `IRemoteUnitLocator`) and may train only self-authored/project-internal or legally proven common-eligible material. `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` is a later activation blueprint; adaptive/personal lives in `<project>\.xiigen` only if Luba later explicitly expands scope. Client/private/third-party/generated outputs are local/adaptive/eval-only/quarantine by default and may not promote to common without positive provenance/output-license/consent-or-ownership/redaction/split/leakage/held-out/ablation and explicit Luba approval proof. Older "pair is v1-active" and "v1 or later?" wording is historical/pre-Pass35 only.
3. Q8-candidate-fields: `IQueryable<GraphNodeCandidate>` with `CandidateId`, `Confidence`, `EvidenceIds[]`, `SemanticQualityClaimed`; `candidate_order_invariant=true`; leakage excluded.
4. Q11-ablation-threshold: `0.02`.
5. Q14-traversal-order / B7 Pass 05: deterministic traversal/topological sorting is correctness/safety baseline and candidate enumeration only; v1 defines `GraphOrderResolverUnit` as trainable/adaptive resolver over `order-0-direct-unit` through `order-4-async-many-to-many-aggregation` patterns. `GraphOrderDecision` must carry `path_order_ref`, legal/provenance status, skipped/rejected edges, trace, and reason codes. Hidden deterministic final authority and B1/B6/provenance bypass are forbidden after cutover.
6. Q30-heldout-threshold: held-out `>=0.95` plus `delta>=+0.02` and `reward>=0.025`.
7. Q38-output-license: generated client outputs stay adaptive/quarantine by default; common use only after output-license clearance, redaction, per-client terms snapshot, and Luba approval.

Q37 and Q-host are not current Luba decisions in this plan section. Q37 is policy resolved with C16/provenance/terms/license checks still required. Q-host is route-principle resolved; blocker 2 closes the seam contract in-plan and blocker 3 closes the same-scenario dual-path design in-plan. Future implementation, source/file evidence, tests, host/UI runs, screenshots, and same-scenario join files are still required before any implementation or host-readiness claim. This is not an implementation readiness claim.

User-reported context: Luba reports "я замержила в main все что было в xiigen mapping" and "я замержила в main все что было в xiigen planning". This file records both as user-reported only; no git verification was performed in `C:\Users\lubao\Documents\xiigen`.

## Historical/pre-Pass50 Attachment 0ce0 Supersession Evidence - 2026-06-30

Attachment `C:\Users\lubao\.codex\attachments\0ce0d4e1-64cd-4a35-b8ed-56993844e412\pasted-text.txt` is readable Russian evidence for the historical/pre-Pass50 downgrade. It reports a stricter full-task coverage simulation: 33 valid sequential passes, FULLY=0, PARTIAL=25, NOT_COVERED=8, ERROR=1, verdict `NOT_READY`, and blockers B1-B21 with B5 absent from the numbering. The earlier attachment 429f/blocker-1..4 repair is historical/pre-attachment-0ce0/narrower-repair evidence only.

Historical/pre-Pass50 gate result, superseded by Pass50 current verdict:

```text
historical_pre_pass50_plan_readiness: PLAN_NOT_READY
historical_pre_pass50_verdict_after_attachment_0ce0: PLAN_NOT_READY
historical_pre_43c2_pass50_verdict: PLAN_READY_FOR_LUBA_REVIEW
historical_demoted_implementation_authorization: NO until fresh Luba approval after review
historical_tests_builds_git_authorized_now: NO
```

Readiness split after Pass37 work-order repair: strict `PLAN_READY` now requires both zero unresolved design/dependency blockers and full original-task coverage proven by minimum 50 sequential mental implementation/review evidence records, one at a time, not parallel, reconciled against active-codex and latest Claude baseline. Historical/pre-Pass39 attachment 0ce0 reported zero fully covered requirements and B1-B21 design/dependency blockers; the Pass39 B-1..B-22 classification is owned by the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`. The earlier blocker-1..4 closure remains useful historical/narrow repair evidence, but it cannot authorize implementation or outrank full-task NOT_READY evidence. Missing tests/builds, screenshots, concrete branch-ref evidence, mergeability checks, real external calls, provider terms retrieval, same-scenario join files, and runtime proof remain future execution-continuation checkpoints after approval; they cannot hide active design/dependency decisions. Historical/pre-Pass50 readiness was `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`, implementation_authorization remains `NO until fresh Luba approval after review`, and the 50 sequential evidence-record chain is now verified in the canonical Pass50 ledger.

Будущие gates после approve Luba:

1. Tests/builds gate — разрешённые тесты и builds должны пройти; failures возвращают работу в bounded repair.
2. Repo inventory and source-branch authority evidence gate — repo inventory и concrete source branch refs проверяются read-only перед claims; copied/discovered workdirs не становятся authority.
3. Mergeability-before-commit/push gate - commit/push is allowed only after source refresh and a no-write mergeability check against the selected read-only authority branch, and against `main` where project rules require main-branch mergeability.
4. Implementation proof gate — реализация, runtime probes и evidence появляются только после свежего approve, не из планового verdict.
5. Legal/provider/provenance proof gate - provider terms, C16 quarantine, TeacherProvenance, ProviderProof, LicenseProof, LegalVerdict transitions, and legal policy mapping are resolved in-plan. Future evidence is required before training/common-use claims; missing proof routes to quarantine/eval-only/block/rebuild-required.
6. External API seam+mock+unit-test+replacement-plan gate — CLOSED_IN_PLAN for blocker 2 by the canonical seam table/R22. Execution later implements adapters, runs the exact mock unit tests, and records mock replacement debt only in the later approved ledger.
7. Dual-path same-scenario host/extension gate — design is specified in-plan by plan-15/R23. Future execution later captures Path A evidence, Path B evidence, screenshots, no-secrets redaction logs, and same-scenario join files, or records an exact blocker with mitigation.
8. Final review floor/claim-verification gate — финальный review проверяет counts, cycles, sub-agent packets, tests/git claims and stale historical statuses.

Pass29 clean readback of the future-gate block above: future gates after Luba approval are execution-continuation checkpoints, not stop permission and not hidden design/dependency decisions. Tests/builds, repo inventory, mergeability, implementation proof, legal/provider/provenance proof, external API seam/mock replacement, dual-path host proof, and final review may run only after readiness and explicit approval; failures route to bounded repair or a true external blocker after mitigation.

Before any future implementation work order, these gates must be explicitly loaded and passed:

| gate | required before execution |
|---|---|
| architecture/planning stack | SK-INTENT, SK-PLAN, SK-REVIEW, and SK-ARCH for trainable/DPO/DynamicGraph work are loaded and named. |
| claim verification | No claimed cycles/passes/subagents/tests/git actions without independently inspectable evidence. |
| source map | CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY rows are current and human-readable. |
| Claude baseline | Active-codex preserves Claude as baseline authority and Codex as validated delta only. |
| DPO provenance | TeacherProvenance, C16 client-origin quarantine, Graph-DPO artifact triad, and legal/provider provenance gates are explicit. |
| DynamicGraph graph-DPO | No hidden deterministic final authority; held-out, ablation, fresh-load, continue-training, and lifecycle proof gates are explicit. |
| dual-path same-scenario | Extension/host readiness requires Path A adapter/API/core proof plus Path B local host UI proof for the same scenario, or an exact blocker packet with mitigation. |
| Russian readability | Changed Russian sections must read back as human-readable Cyrillic; marker-clean alone is not enough. |
| implementation authorization | Fresh Luba authorization after plan readiness; this current repair does not provide it. |
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

Current blocker-2 seam authority is the canonical table in `active-codex-execution-plan-readiness-repair-20260629.md` plus R22 in `active-codex-execution-02-source-inventory-and-reference-map.md`. The later replacement ledger is not created by this planning worker and cannot be used as a reason to leave request/response fields, mock response shape, unit-test intent, or legal/provenance rules undefined.

## Historical 2026-06-29 Planning-Repair Boundary (superseded by 2026-07-14 overlay)

Historical boundary only: this section cannot override the current 2026-07-14 code-first overlay.

At that historical time (2026-06-29), Luba's then-current instruction was `План не готов к выполнению - нужно починить`, and that planning-repair worker was authorized only to repair plan files. Product implementation, workdir setup, tests/builds, and git operations were not authorized for that historical task. This record is superseded and cannot override the 2026-07-14 code-first overlay.

At that historical time, source `claude-*` and `codex-*` files were treated as read-only and PRs, merges, commits, pushes, branch switches, workdir copying, and implementation edits were forbidden for that old planning-repair task. Those old prohibitions do not govern current execution; only the 2026-07-14 overlay and current Luba instructions do.

## Pass31 Historical Open-Question Snapshot And Gate Boundary

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Historical/pre-Pass32 open questions/blockers before Pass32:

1. B-16: materialize `MergeRole` and `merge_order_is_authority` as typed plan fields for CON-merge.
2. B-19: disambiguate SR-16 so seed-corpus evidence and DesignConversion/R16 backing do not share one overloaded identifier.
3. B-10 historical/pre-Pass34: AD-detect evidence was still failed/unmeasured. Pass34 supersedes this as a design classification only: B-10 is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`; AD-detect remains future execution measurement.
4. Seed corpus: `language-translate` requires authored/located `learning-sets` seed inputs before trained-feature proof.
5. Provider terms: Qwen3/Together v1 teacher profile is selected in plan, but terms review remains pending and blocks teacher-leg train use.
6. B-22: propagate Pass31/current Claude reconciliation to all active consumers before any closure claim.

Resolved-as-design, not execution proof: teacher/judge real-call path, FLOW-38 intake, v1 one-pilot scope, common-only v1 with deferred personal/adaptive leg, 20-field `TeacherProvenance`, GraphDpoPair/unit-level semantics, graph-order baseline/trainable boundary, no-hidden-determinism policy, source-to-plan inventory `64/64`, and legal/provenance routing. These cannot be re-opened as Luba questions without newer contradictory evidence, but they also do not authorize product work.

## Pass32 Historical Open-Question Snapshot And Gate Boundary

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Pass32 resolves B-16 in plan only: `IEdgeFanInMerger.Merge` returns typed `GraphFanInMergeResult` with `MergeRole = FanInMergeRole.DeterministicFanInBaseline`, `MergeOrderIsAuthority = false`, `Merged`, `Context`, `MergedBranchCount`, and conflict/rejected/provenance/trace/failure refs. Future proof must still implement/run tests; the current plan repair does not prove product behavior.

Historical/pre-Pass33 open questions/blockers after Pass32:

1. B-19 before Pass33: disambiguate SR-16 so seed-corpus evidence and DesignConversion/R16 backing do not share one overloaded identifier.
2. B-10 historical/pre-Pass34: AD-detect evidence was still failed/unmeasured. Pass34 supersedes this as a design classification only: B-10 is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`; AD-detect remains future execution measurement.
3. Seed corpus: `language-translate` requires authored/located `learning-sets` seed inputs before trained-feature proof.
4. Provider terms: Qwen3/Together v1 teacher profile is selected in plan, but terms review remains pending and blocks teacher-leg train use.
5. B-22: propagate Pass32/current Claude reconciliation to all active consumers before any closure claim.

B-16 is resolved-as-design, not execution proof, and stays separate from better-output/FLOW-38/five-format adapter work under B-4/R17 and B-19.

## Pass33 Historical Open-Question Snapshot And Gate Boundary

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Pass33 resolves the B-19 SR-16 overload in plan only:

| name | current meaning |
|---|---|
| `SR-16-CoreBacking` | `llm_mvp_core` DesignConversion/R16 backing only; not platform host proof and not seed corpus. |
| `SR-16-RouteFacade` | R16 route facade/seam/client authority surface: xiigen-platform public facade, xiigen-flows FLOW-31 internal seam, FigmaToCode-design-gen and CanvaToCode as client/caller surfaces only. |
| `SeedCorpus-LanguageTranslate` | Separate active-pilot seed-corpus dependency for `language-translate`; not SR-16 host-route proof. |

Historical/pre-Pass35 open questions/blockers after Pass33:

1. B-10 after Pass34: design is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`; AD-detect/permutation/ablation/fresh-load/continue-training execution evidence is still pending and must not be claimed as run.
2. Seed corpus: `SeedCorpus-LanguageTranslate` still requires authored/located `learning-sets` seed inputs before trained-feature proof.
3. Provider terms: Qwen3/Together v1 teacher profile is selected in plan, but terms review remains pending and blocks teacher-leg train use.
4. B-22: propagate Pass34/current Claude reconciliation to all active consumers before any closure claim.
5. B-19 execution proof: physical host/API route files, branch/path inventory, and same-scenario Path A/Path B proof remain future execution evidence after readiness and Luba approval.

## Pass35 Historical Open-Question Snapshot And Gate Boundary

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Pass35 resolves `SeedCorpus-LanguageTranslate` sourcing/shape in plan only as `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`. The seed corpus contract is `SeedCorpus-LanguageTranslate-v1`, a self-authored synthetic local fixture for `language-translate`, planned after approval at `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl`. The fixture uses no third-party text, no external API, no web scrape, and no closed-model output; future rows must carry `source_category=synthetic_self_authored`, `license=project_internal_self_authored`, provenance refs or `TeacherProvenance`, `split=train|held_out`, and `semantic_quality_claimed=false` until measured.

Historical/pre-Pass36 open questions/blockers after Pass35:

1. Provider terms: Qwen3/Together v1 teacher profile is selected in plan, but terms review remains pending and blocks teacher-leg train use.
2. B-22: propagate Pass35/current Claude reconciliation to all active consumers before any closure claim.
3. Future execution evidence: create the seed JSONL, run legal/provenance/split gates, train/evaluate, prove held-out exact-match + char-accuracy + `RuntimeProductAccuracy`, run tests/builds/runtime/host/provider/git gates only after readiness and Luba approval.

## Pass36 Historical Open-Question Snapshot And Gate Boundary

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Pass38 supersedes the Pass36 optional-disabled route line for current status. Hosted Qwen3 provider status is now `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`: Qwen3 Apache-2.0 is selected/recommended as the teacher blueprint and `qwen3-together-v1` / TogetherAI is the selected v1 hosted-provider recommendation, while TogetherAI terms/output-use proof and Luba hosted-use approval remain unresolved gates before hosted train/common use. Fireworks/OpenRouter/DashScope are deferred alternates only if a later plan explicitly selects and proves them.

## Pass37 Historical Open-Question Latest-Claude Sync

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Historical Pass37 kept `qwen3-together-v1` selected but technically pending. **Current overlay:** business `APPROVED_BY_LUBA_2026-07-07` is separate from technical proof; `ProviderTermsChecked=false` until positive `HostedTermsProof` passes `G-TP-4`; before then hosted use is BLOCK/QUARANTINE, after it TP2 is eval-only, and train/common remains false until the full predicate plus `G_DPO_FINAL`.

This is not a request to choose a provider silently. It records the current residual honestly and keeps implementation authorization `NO`.

Historical/pre-Pass39 current-looking blocker line:

1. Historical/pre-Pass39: B-22 propagation/reconciliation was still listed as the current remaining blocker after Pass36/Pass37. Current status is owned by the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`: B-22 is `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`, not readiness, not implementation authorization, and not execution proof.

Future execution evidence after readiness and Luba approval: create the seed JSONL, run legal/provenance/split gates, train/evaluate, prove held-out exact-match + char-accuracy + `RuntimeProductAccuracy`, and run tests/builds/runtime/host/provider/git gates only after readiness and Luba approval.

Hosted Qwen3/Together proof boundary: Qwen3 Apache-2.0 model-license evidence is not hosted-provider output-use approval. Before train/common use, future execution must provide `ProviderTermsProof.Provider=TogetherAI`, `ModelFamily=Qwen3`, exact `ModelId`, Qwen Apache-2.0 source ref, Together ToS snapshot, Together privacy/security/ZDR organization-setting snapshot, `passthrough` disabled or upstream policy captured, `service_mode=plain`, `grounding_used=false`, `provider_terms_checked=true`, non-pending `terms_snapshot_ref`, `data_retention_mode=ZDR_or_no_store`, `output_ownership_checked=true`, `third_party_model_terms_checked=true`, and Luba hosted-use approval. Missing proof means `BLOCK` or `QUARANTINE`, not train-ready/common-ready; judge/eval-only requires `used_for_training=false`, non-pending terms, org privacy/ZDR proof, no passthrough gap, no grounding/search, and no sensitive/private raw data. **[HISTORICAL/pre-approval-2026-07-07 в части «Luba hosted-use approval» — SUPERSEDED; текущий канон: business `APPROVED_BY_LUBA_2026-07-07` дана, но техническая `ProviderTermsProof` (execution-гейт `G-TP-4`, overlay §6/§7) остаётся открытой; сбор proof-полей ниже — это `G-TP-4`, а не повторное бизнес-одобрение.]**

Provider/license coherence gate: future execution may collect proof, but it may not choose the policy later. Current plan requires `FamilyOf(model_id)` -> per-release `CanonicalLicense` cross-check, exact C5 set `{Apache-2.0, MIT, ProjectInternal, Synthetic}`, no ellipsis, and separate `ModelFamily/Vendor` vs `HostingProvider/Transport` roles. DeepSeek, Kimi/Moonshot, GLM, and Mistral remain deferred/not train-ready unless an explicit plan repair supplies canonical license, provider terms/output-use proof, and role-disambiguated provider profile.

## Pass39 Open-Question Routing

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

The Pass39 B-1..B-22 inventory is now the Pass39 table in
`active-codex-execution-16-full-task-coverage-simulation-gate.md`. Older current-looking
open-question rows that say B-1..B-21 are generic plan blockers, B-21 is simply open,
or B-22 still needs propagation are historical/evidence-only for present status when
they conflict with Pass39.

Current residual gates after Pass47 classification:
1. Hosted-only external gate: Luba/external hosted-provider proof for B-2 is required before hosted Qwen3/Together train/common use. It is not a local self-authored/self-best v1 planning blocker and is not the historical/pre-post-repair global PLAN_NOT_READY basis.
2. Current global PLAN_NOT_READY basis: minimum 50 sequential simulation/review evidence records plus parent synthesis before any readiness or implementation authorization claim.
3. Historical/pre-R-B-closure expansion note: B-18 defaulted to v1 common-only with adaptive activation deferred unless Luba later expands scope. Older B-21 wording that treated first substantive recon as enough or deeper recon as optional/future is superseded by the accepted R-B evidence-library closure: `llm mvp` is `20/20` evidence-only and `llm_mvp_core` is `40/40` evidence-only. This does not prove readiness, B-22 closure, implementation authorization, tests/builds/runtime/provider/git/training/checkpoint proof, or mergeability.

## Pass42 Residual Routing After Pass41

Historical/pre-Pass50 verdict then remained `PLAN_NOT_READY`; historical_pre_post_repair_review_verdict_after_43c2 was `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review was `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness_after_latest_claude_plan_partial_reconciliation is `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_43c2_pass50_verdict is `PLAN_READY_FOR_LUBA_REVIEW`; implementation_authorization remains `NO until fresh Luba approval after review`.

Open-gate consumers must treat the Pass42/Pass43 B-18/B-21 default-defer wording as historical/pre-R-B-closure evidence when it conflicts with the accepted `20/20` plus `40/40` evidence-library state. Pass47 clarification still applies: external/provider/legal proof for hosted Qwen3/Together is a hosted train/common gate only, not a local self-authored/self-best v1 planning blocker; future execution proof waits until readiness and approval; implementation authorization remains `NO`. Current remaining work is R-G/source addressability, Fable MISSING/PARTIAL reconciliation, active-plan carrier review, and separate readiness review.

Older rows that still look like B-22 propagation, seed-corpus shape, external seams, repo/workdir authority, lifecycle, or no-hidden-determinism are current design blockers are historical/pre-Pass42 for residual routing unless a later pass reopens them with direct evidence.

## 2026-07-07 Reopened Luba Product-Decision Residuals (Q-P09-XSRC, Q-P12-M4) — NOW RESOLVED_FROM_DOCUMENTED_AUTHORITY (slice B04-reconcile-file07-matrix-verdict)

historical_demoted_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW (design complete) — see the S17-B04 top overlay
historical_demoted_implementation_authorization: NO until Luba says `реализуй`
scope: This section previously registered `Q-P09-XSRC` and `Q-P12-M4` as reopened `LUBA_PRODUCT_DECISION_RESIDUAL` rows (`UNDECIDED`). Both have since been ANSWERED FROM DOCUMENTED AUTHORITY in their owner files (Luba's answers were already in the task-process / documented-authority text; they were EXTRACTED, not chosen here). This slice reconciles the central open-questions ledger to those owner resolutions. It does not invent any value; the ONLY remaining residual is execution-time TUNING (see the tuning-residual note below), which is not an open plan decision.

| id | owner file | exact question | documented answer (extracted, not chosen) | status | documented citation |
|---|---|---|---|---|---|
| `Q-P09-XSRC` | `active-codex-execution-plan-09-auto-dpo-loop.md` (Шаг 1, `AutoDpoLoopOrchestrator._inputQueue`) | Каким должен быть источник входа `X` для такта авто-DPO — чем наполняется `_inputQueue`? | Первичный источник = **runtime-observations**: наблюдать реальные низко-результатные прогоны НАШИХ проектов и enqueue именно их; эталон (`goal`) берёт разрешённая внешняя модель. `seed-corpus` = cold-start-only, `FLOW-38` = вспомогательный поток (узкий смысл `both`, но приоритет закреплён: runtime primary, seed cold-start-only). | `RESOLVED_FROM_TASK_PROCESS` (2026-07-07) | Процесс задачи, дословно «мы запускаем любой из наших проектов и видим, где модель дала низкий результат … берём то что выдала внешняя модель как goal … обучаем нашу модель строить DPO … до тех пор пока не начнём получать … достаточно качественный результат» = строка 20 оригинальной задачи (attachment `pasted-text.txt`), зафиксировано в `active-codex-execution-original-task-authority-audit-20260629.md:53` (+ строки 45/44); owner-разрешение `active-codex-execution-plan-09-auto-dpo-loop.md:268-337`. |
| `Q-P12-M4` | `active-codex-execution-plan-12-no-hidden-determinism.md` (§13 item `Q-P12-M4`; ранее локально «§13 Q1», переименован 2026-07-07) | Разрешено ли финальному плану фаз сценария механизма #4 (`OrchestrationScenarioPhasePlanner.PlanPhases` — ВЫБОР + ПОРЯДОК фаз `PhaseOrder/PhaseIndex/BuildPhases`) оставаться детерминированным baseline, или обязан стать обучаемым L4/L5-юнитом? | v1 = детерминированный **помеченный** `BASELINE_MUST_BE_LABELLED` baseline (`isLearned=false`, `phase_plan_semantic_quality_claimed=false`, НЕ финальная семантическая власть; baseline НЕ удаляется — нужен для ablation). Обучаемый `SemanticPhasePlanUnit` (L4/L5-cutover, выбор И порядок) — именованная **будущая** опция, не v1. | `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (2026-07-07) | Мастер `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §E.0 «Блок 12 … Q-mech4», решение Luba 2026-06-30 (прецедент §E.0 Q14); owner-разрешение `active-codex-execution-plan-12-no-hidden-determinism.md:1321-1384` (+ §3.0/§3.B/§13). |

Note: `Q-P12-M4` is a plan-12-local id, deliberately family-unique to avoid collision with the family-level `Ф0 Q1`. `Q-P09-XSRC` is a plan-09-local id. Neither is an `Ф0`/F0 question. Both are now RESOLVED at plan-design level from documented authority; `implementation_authorization` remains `NO` until Luba says `реализуй`.

**Tuning-residual note (execution-time, NOT an open plan decision).** After the two design answers above, the ONLY residual attached to these mechanisms is a set of execution-time TUNING values that are genuinely NOT given as literal numbers in the task-process / legal documents. These are implementation-time settings (реализуй), not open plan decisions:
- `Q-P09-XSRC` tuning residual: the numeric `enqueue_threshold` (the «low result» boundary on `RuntimeProductAccuracy` that decides which runtime observations are enqueued). Not a literal in the docs → set during implementation on the same scorer/split/threshold-set as §3 Шаг 6. Owner: `active-codex-execution-plan-09-auto-dpo-loop.md:333-337`.
- Auto-DPO reward weights `w_lift/w_legal/w_contrast/w_leak` (owner `active-codex-execution-plan-07-dpo-builder.md:1163-1211`): the algorithm shape, trainer path, and per-field mutation are DECIDED; the concrete numeric weight VALUES are execution-time TUNING. **legal is a HARD GATE** (a pass/fail provenance/legal gate, not a soft weighted term that can trade off against `w_lift`/`w_contrast`) — the «legal as hard gate» disposition is the documented answer; only the remaining weight NUMBERS are tuning. (Owner plan-07 still carries the older `DEFERRED_LUBA_DECISION` framing for the numbers; that is a cross-file propagation item outside this slice's edit scope and is recorded here as a residual note, not silently overridden.)

These EXECUTION_TUNING values do NOT hold plan-review readiness; they are set during implementation and do not create an open plan decision.

## Two-Phase (TP1/TP2) Luba Product-Decision Residuals — Final Section (2026-07-13, Проход 5)

Русское человеческое резюме: двухфазный teacher-маршрут добавил несколько РЕШЕНИЙ, которые по правилу «no product decisions» не может выбрать агент — они остаются за Luba. Ниже — единственный владелец списка этих открытых двухфазных Luba-вопросов (`Q-TP-*`). Это НЕ execution-гейты (execution-гейты G-TP-* — в overlay §6 и в TP5-overlay наверху этого файла) и НЕ занижение floor. Пока они открыты, readiness = `PLAN_NOT_READY`; impl-auth = `NO`.

| Q-TP id | Вопрос (продуктовое решение Luba) | Текущий канон / дефолт | Статус |
|---|---|---|---|
| Q-TP-NUMS | Числовой критерий перехода TP1→TP2: какое плато held-out / `TeacherBeatRate` / окно объявляет «локальный учитель исчерпан» и разрешает переход на hosted-этап. | Окно=3 наследует `LOCKED_FROM_CLAUDE_RR9_Q24` (`plateauWindow`/`regressionWindow`/`noSignalWindow=3`); плато `< +0.02` наследует locked `minImprovement=+0.02`/`teacherGapEpsilon=0.02` (Q24/Q22/Q30); `TeacherBeatRate(TP1) < 0.10` = `EXECUTION_TUNING` (safe eval-only default, plan-09:1484, гейтит 0 train-ready); акт перехода подтверждает Luba — уже канон Гейт 6b (plan-11 §8). Источники: мастер-план:1081, plan-09:1690, plan-11:848. | RESOLVED_FROM_DOCUMENTED_AUTHORITY (числа сигнала: окно=3, плато `< +0.02` из LOCKED Q24/Q22/Q30; `TeacherBeatRate<0.10` = `EXECUTION_TUNING`; акт перехода — Luba, канон Гейт 6b). Прежняя `LUBA_PRODUCT_DECISION_RESIDUAL` — historical/pre-resolution (прецедент Q-B21-FLOOR). |
| Q-TP-KGEN | Сколько генераций `K` берёт локальный учитель на кандидата (best-of-K) на Этапе 1. | PROPOSED_DEFAULT `k_generations=4` (plan-09:478 «PROPOSED_DEFAULT empirically-tunable»; 07:434-438 tuning-класс, implementation-time); границы `K` уже канон (SI_GATE_7, бюджет 8192); best-of-K по self-confidence запрещён как метка (NEG-2P-a). | EXECUTION_TUNING (PROPOSED_DEFAULT `k_generations=4`; implementation-time per 07 tuning-класс; границы `SI_GATE_7`/бюджет 8192). Прежняя `LUBA_PRODUCT_DECISION_RESIDUAL` — historical/pre-resolution. |
| Q-TP-NOGOLD | Поведение при отсутствии gold на Этапе 1: `quarantine/eval-only` vs `judge verdict-metadata-only`. | Нет тихого default-to-teacher (NEG-2P-f); готового выбора режима в документах НЕТ (F0:897 «выбор за Luba»). PROPOSED_DEFAULT (вывод-из-канона): v1 = `quarantine/eval-only` (паттерн LOW_CONTRAST→quarantine plan-09:1516; приёмка только по held-out self-authored gold guide-03); judge-режим = opt-in Luba, verdict-metadata-only, C11/C17 «judge-only does NOT waive terms» (04-policy:104). | LUBA_PRODUCT_DECISION_RESIDUAL (narrowed: выбор режима v1 остаётся за Luba) |
| Q-TP-TVT | Политика разбиения train/val/test (source-disjoint) для двухфазного набора: пропорции и held-out per stage. | Структура: source-disjoint + `SplitProof` (plan-11:800-803) + один scorer/split через границу Гейт 6a/`SI_GATE_8` + `StageGap` на тех же строках; наследует replay `70/30` + source-disjoint held-out (guide-03, plan-13). Числовые пропорции per stage — `EXECUTION_TUNING` (прецедент Q-P09-XSRC 07:435). Справка: teacher-vs-teacher пара запрещена как train-ready v1 (plan-07:424), не открытый вопрос. | RESOLVED_FROM_DOCUMENTED_AUTHORITY (структура source-disjoint+`SplitProof`+один scorer/split через границу+`StageGap` на тех же строках) + EXECUTION_TUNING (пропорции per stage, прецедент Q-P09-XSRC). Прежняя `LUBA_PRODUCT_DECISION_RESIDUAL` — historical/pre-resolution. |
| Q-TP-235B-EVAL | Как оценивается hosted `qwen3-together-v1` (Qwen3-235B-A22B) на Этапе 2: held-out/бюджет/критерий принятия перехода. | Легальность/методика: hosted-вызов (включая eval) ТОЛЬКО после `G-TP-4` (plan-09:484, plan-11:812/848, 04-policy:104, NEG-2P-m plan-13:451, ZDR-first §3.5.1b); `StageGap9bVs235b` — eval-only. Числовой критерий приёмки hosted-этапа = `RESOLVED_BY_LUBA_LIVE` 2026-07-14: `HeldOutAccuracy(Y_goal_TP2) >= 0.90` (переиспользует порог `generic_documentation_qa_accuracy`); нейтральная нота-риск абсолютного пола (на слабых доменах может не достигаться → возможен вечный BLOCK; митигация per-domain пол ИЛИ budget-bounded fallback = open EXECUTION-наблюдение, НЕ переоткрытие решения). | RESOLVED_FROM_DOCUMENTED_AUTHORITY (легальность/методика: hosted incl. eval только после `G-TP-4` — plan-09/plan-11/04-policy/NEG-2P-m/ZDR-first) + RESOLVED_BY_LUBA_LIVE 2026-07-14 (числовой критерий приёмки: `HeldOutAccuracy >= 0.90`, зеркало каталога §4 п.6 / FIX-RES-1 B1). Прежняя полная `LUBA_PRODUCT_DECISION_RESIDUAL` И прежний narrowed `LUBA_PRODUCT_DECISION_RESIDUAL` (числовой критерий) — historical/pre-resolution; текущий вердикт = RESOLVED (документ-авторитет + Luba-live), строка НЕ открыта. |

Уже принятые двухфазные решения (НЕ открыты, НЕ переоткрываются):
- Двухфазность как таковая + локальный профиль `qwen35-local-v1` (Qwen3.5-9B/Ollama): `RESOLVED_BY_LUBA_LIVE` (живое указание 2026-07-13).
- Hosted `qwen3-together-v1` бизнес-одобрение hosted-use: `APPROVED_BY_LUBA_2026-07-07`, секвенировано в Этап 2 через overlay; технический гейт `G-TP-4` (`ProviderTermsProof`: Together ToS/ZDR/output-use snapshot) остаётся ОТКРЫТЫМ execution-гейтом, НЕ Luba-вопросом.
- `G-TP-2` (лицензия весов Qwen3.5-9B для локального этапа): execution-гейт, НЕ Luba-вопрос.

Замечание о single-owner (Проход 6, 2026-07-13; обновлено FIX-RES-1 2026-07-14): эти пять `Q-TP-*` строк — владелец-список двухфазных Luba-вопросов вместе с зеркалом в `F0-decision-unblocker.md` (Fable-v2-таблица). Фраза «закрываются только явным ответом Luba» остаётся верной ТОЛЬКО для ОДНОГО оставшегося narrowed-остатка: `Q-TP-NOGOLD` (выбор режима v1). Narrowed-остаток `Q-TP-235B-EVAL` (числовой критерий приёмки hosted-этапа) закрыт живым решением Luba 2026-07-14 (`HeldOutAccuracy(Y_goal_TP2) >= 0.90`) → `RESOLVED_BY_LUBA_LIVE`, больше НЕ открыт (зеркало каталога §4 п.6 / FIX-RES-1 B1 + нота-риск абсолютного пола). Для строк `RESOLVED_FROM_DOCUMENTED_AUTHORITY`/`EXECUTION_TUNING` (`Q-TP-NUMS`, `Q-TP-KGEN`, `Q-TP-TVT`, легальность/методика + теперь числовой критерий `Q-TP-235B-EVAL`) применён прецедент `Q-B21-FLOOR` (residual был mis-scoped; прежняя `LUBA_PRODUCT_DECISION_RESIDUAL`-формулировка — historical/pre-resolution) — эти документ-/Luba-обоснованные решения НЕ переоткрываются. **Propagation-нота (вне scope правки FIX-RES-1 — разрешено править только 2 файла: этот 07 owner + каталог):** зеркало Fable-v2-таблицы в `F0-decision-unblocker.md` требует того же обновления `Q-TP-235B-EVAL` narrowed → `RESOLVED_BY_LUBA_LIVE`; до этой синхронизации F0-зеркало = cross-file propagation-остаток, НЕ второй живой канон (owner-строка ЗДЕСЬ = источник истины). Секция «Last Section — Current Luba Decisions Recorded» в F0 (о НЕ-двухфазных решениях) не относится к этим строкам.
