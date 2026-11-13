## Current Code-First Navigation Overlay (2026-07-14)

Use this order for the live execution state:

1. `active-codex-execution-EXECUTION-CONTROL-BOARD.md` — current code-first status, accepted/pushed tips, publication rule and active slice.
2. `active-codex-execution-code-first-implementation-audit.md` — reconciled implementation evidence through accepted/pushed core tip `e2a6bce7448a1672a90bdec95106da279b077c5b` and the current post-slice-13 production/manual boundary.
3. `active-codex-execution-two-phase-local-remote-overlay.md` — sole owner of TP1/TP2 semantics and final DPO/training activation; consult only when approaching teacher-stage entry or checking the DPO-last boundary.
4. plan-07 and later DPO/training owners — actionable only after the owner overlay's final gate passes.

Current sequence pointer: `CODE_FIRST -> TP1_LOCAL eval-only -> TP2_HOSTED eval-only -> G-DPO-FINAL -> DPO/training`. Do not derive stage gates from this index. Deterministic solutions remain outside learning.

`active-codex-execution-file-rag.md` preserves the completed 20-cycle/55-source snapshot and two explicitly post-cycle planning profiles (`INV-056`, `INV-057`). Its separate current-core code index now describes the contract/production/test files used by accepted slices 07-13 and how/when to use them. This is a navigation refresh, not a claimed new cycle. Older no-commit/no-push, absent-contract, missing-tool or design-only current-status text must be read through the new top overlays. Existing phase/subphase/substep counters are unchanged.

Current capability distinction: slice 07 runner executes bounded candidate combinations; slice 08 builder validates/proposes topology; slice 09 runtime owner applies revisioned topology and exposes the read-only source; slice 10 projector converts strict logical-registry structure into a build signal without applying it; slice 11 consumes the read-only topology in production L4 orchestration; slice 12 bootstraps projection, runtime ownership and the assistant behind a production facade; slice 13 is the first production caller outside tests and converts one explicit caller-owned profile into that facade. The remaining manual-only chain starts with selecting a real deployment host/reference seam, then host-side profile deserialization/selection and runtime lifetime ownership, invocation through `Runtime.Assistant`, persistence/restart of profile plus revision/CAS state, and only after an explicit design decision the currently insufficient authority schema/canon/resolver. No bounded host slice is declared selected by this index. `ACCEPTED_PUSHED_SLICE` does not close composite Gate B or authorize TP1, TP2, DPO or training; both teacher stages and DPO remain not started.

## Post-Library-Build R-B Reconciliation Overlay (2026-07-06)

plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE (unchanged; R-B closure does not change plan readiness)
implementation_authorization: NO until fresh Luba approval after review
r_b_reconciliation_basis: The 2026-07-05 Fable Coverage Repair Overlay below stamped the family with `R-B 20/40 knowledge-library artifacts are not accepted built artifacts`. That wording is now historical/pre-reconciliation. Both R-B knowledge-library target artifacts are built and parent-verified: `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` is `accepted_focused_pass_records: 20/20` (parent readback verified, header `R_B_LLM_MVP_20_PASS_TARGET_COMPLETE`), and `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` is `accepted_focused_pass_records: 40/40` (parent readback verified, header + Honesty Packet reconciled to 40/40, `R_B_CORE_40_PASS_TARGET_COMPLETE__PLAN_FAMILY_STILL_PLAN_NOT_READY`). Therefore R-B blockers 01-03 (the library-not-built / library-records-incomplete / not-accepted-built-artifact class tracked in `active-codex-execution-42-fable-gap-repair-additions-20260705.md`) are repaired at knowledge-library-closure level, evidence-only.
r_b_reconciliation_scope: This overlay reconciles ONLY the R-B knowledge-library status carrier. It does NOT close R-G source addressability, does NOT resolve the shared-contract/readability/lifecycle partial repairs, does NOT claim plan readiness, and is NOT implementation authorization. R-B closure is evidence-only corpus synthesis, not product/test/build/runtime/provider/training/checkpoint/git proof.
historical_classification_rule: Any line in the 2026-07-05 Fable overlay below, or elsewhere in this file, saying the R-B 20/40 or `llm_mvp_core` 40-pass knowledge-library artifacts are `not accepted built artifacts`, `incomplete`, `still future`, or `open` for the built-artifact question is historical/pre-2026-07-06-reconciliation evidence only; the current R-B built-artifact verdict is 20/20 and 40/40 parent-accepted per the two library artifacts named above. The rest of each older overlay (R-G, partial contracts, plan-not-ready, implementation authorization NO) remains current.

## Fable Coverage Repair Overlay (2026-07-05)

plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
implementation_authorization: NO until fresh Luba approval after review
current_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` supersede the previous final-readiness overlay. Fable reports 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers. R-B knowledge-library status is reconciled by the 2026-07-06 Post-Library-Build R-B Reconciliation Overlay above: the R-B `llm mvp` 20-pass target is complete (`20/20` parent-accepted) and the R-B `llm_mvp_core` 40-pass target is complete (`40/40` parent-accepted) as evidence-only knowledge-library closure, so the older `not accepted built artifacts` reading of R-B is historical/pre-2026-07-06 evidence only. Remaining current blockers: R-G source addressability is incomplete; several shared contracts/readability/lifecycle carriers need plan-family repair. This is plan repair evidence only, not product implementation, not tests/builds/runtime/provider/training/checkpoint/git proof.
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
## Current Readiness Verdict Reconciliation Overlay (2026-07-02)

historical_pre_final_review_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
implementation_authorization: NO until fresh Luba approval after review
historical_pre_final_current_verdict_basis: Newer evidence attachment 0aa5730c, the Erdos bounded worker packet, and the 2026-07-05 Pass37-Pass50/Singer correction supersede stale READY and stale acceptance-count overlays. Historical/pre-Fable evidence supported PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE: current-ledger 50-pass acceptance is repaired as bounded planning-section evidence (50 headings exist and strict acceptance-field/readability evidence is 50/50 after the 2026-07-05 Pass37-Pass50/Singer repair). This queue-classification repair finds no current plan/design blocker in the stale CURRENT_REPAIR_QUEUE labels: F0 locks are recorded with implementation proof pending; legal/provenance is propagated in active plan, policy, and source-map surfaces; hosted-provider proof is a route-specific external pre-use gate before hosted Qwen3/Together train/common use, not a local self-authored v1 planning blocker; future execution proof remains after approval; fresh Luba post-review approval is implementation authorization, not a plan-design blocker. This is not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization. The current B owner table now gives a single B-6/B-8/B-9/B-12/B-22 status; implementation cannot start until parent readiness review accepts the candidate state and Luba gives fresh post-review approval.
historical_pre_final_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not cycles/readiness/product/implementation proof; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; F0_LOCKS_RECORDED_IMPLEMENTATION_PROOF_PENDING=Q3/Q6/Q8/Q11/Q14/Q30/Q38 recorded, Q37/Q-host implementation checks only; LEGAL_PROVENANCE_PROPAGATED_IN_PLAN_EXECUTION_PROOF_PENDING=20-field TeacherProvenance, ProviderProof, LicenseProof, LegalVerdict, C16/C17 quarantine/block/rebuild behavior present in active plan/policy/source-map surfaces; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use, not local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=fresh Luba post-review approval required before execution, not plan-design blocker; EXECUTION_PROOF_PENDING_AFTER_APPROVAL=code implementation, seed file creation, tests/builds/runtime, mock execution/replacement, training/checkpoint/fresh-load/ablation, same-scenario dual-path proof, branch/mergeability/git; readiness_candidate_for_parent_review=ZERO_CURRENT_DESIGN_BLOCKERS_FOUND_BY_THIS_QUEUE_CLASSIFICATION; parent must run final readiness review before any PLAN_READY wording; HISTORICAL_PRE_0AA_EVIDENCE=older PLAN_READY_FOR_LUBA_REVIEW / READY and older NOT_READY records unless a later evidence-bound repair proves current status.
resolved_residue_evidence:
- plan-09 B-15 / IGraphWeaknessDetectorUnit edge/graph wording is now v1 diagnostic/deferred scope only; v1 train-ready DPO rows remain unit-level and edge/graph acceptance/training/promotion stays deferred unless Luba later expands scope.
- plan-06 fan-in merger canon is IEdgeFanInMerger.Merge(GraphFanInMergeInput input) -> OperationResult<GraphFanInMergeResult>; bare LogicalFlowResult merger wording is historical/pre-0aa only.
historical_pre_current_43c2_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, READY, PLAN_NOT_READY, or PLAN_PARTIAL_REPAIR_QUEUE is historical/pre-0aa evidence unless it appears in the current top overlay or cites a later evidence-bound repair after attachment 0aa5730c.
## Historical/pre-43c2 Pass50 Readiness Verdict Overlay (2026-07-02)

historical_pre_43c2_pass50_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
implementation_authorization: NO until fresh Luba approval after review
current_pass_chain: 50 canonical pass headings exist and strict current-ledger acceptance-field/readability evidence is now 50/50 after the 2026-07-05 Pass37-Pass50 bounded acceptance repair recorded in active-codex-execution-41-current-ledger-50-pass-acceptance-index.md. This is 50 accepted planning pass sections, not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.

Historical/pre-43c2 Pass50 basis:
- Original-task R1-R10 are FULLY_COVERED_PLAN_LEVEL by Pass46 and this Pass50 adversarial review.
- B-1..B-22 have no remaining implementable active-codex design/dependency blocker under the Pass39/Pass43/Pass44 current overlays. Hosted provider proof remains required only before hosted train/common use.
- Source-to-Plan inventory is 64/64 by exact canonical row-id audit, with 0 noncanonical row-like offenders.
- Claude/latest-baseline details are carried in active-codex: five participating repos, DPOLegalLearning work name, C:\Projects workdir boundary, latest-updated source-branch authority as active/Codex delta, xiigen-platform facade plus xiigen-flows seam, FigmaToCode/CanvaToCode as clients/callers, unit-level 14-field GraphDpoPair, 20-field TeacherProvenance, exactly one active language-translate pilot, and provider/terms boundary.
- QA, tests, builds, screenshots, same-scenario dual-path proof, mock replacement, provider/API proof, runtime/training/checkpoint proof, branch/mergeability, git, PR, and product review remain future execution-continuation gates after fresh Luba approval. They are not design decisions and were not run in Pass50.
- This Pass50 overlay is historical/pre-43c2 evidence only. Newer attachment 43c2 supersedes it; READY wording in this block is not current readiness, not B-22 closure, and not implementation authorization.

Readback: This Pass50 paragraph is historical/pre-43c2 evidence only. Attachment 43c2 triggered a downgrade, the residues were repaired in active plan text, and historical/pre-0aa bounded reconciliation restored the evidence-supported plan-level verdict to PLAN_READY_FOR_LUBA_REVIEW at that time; historical/pre-Fable verdict was PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE and implementation_authorization remains NO until fresh Luba approval after review.
---
artifact_type: planning_library_index
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: populated_now_with_future_evidence_tasks
last_updated: 2026-06-30
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
---

# ILearningDynamicGraph DPO Planning Library Index

## Pass 01 R-OT-01 Current Authority Anchor

The planning library for this active plan is exactly `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build`.
Claude `claude-*` artifacts in this library are read-only baseline evidence authority. `active-codex-execution-*`
artifacts are the execution-facing copy and may contain only validated non-contradictory deltas. The current
verdict remains `PLAN_NOT_READY`; implementation authorization is `NO`, so no product implementation, code/test
edits, git work, host probes, training, or provider/API calls may start until the plan is fully ready and Luba
approves after review.

## Pass31 Active Index Addendum - Current Readiness/Reconciliation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. The current controlling readiness overlay is now in `active-codex-execution-16-full-task-coverage-simulation-gate.md` under "Pass31 Current Readiness And Claude-Baseline Reconciliation Overlay".

Pass31 accepts Source-to-Plan `64/64` only as source-map inventory evidence and updates the Pass39 B-1..B-22 wording boundary. Several older open rows are no longer honest as current design-open wording because the read-only Claude baseline and attachment evidence resolved them at plan level. Pass32 resolves B-16 typed CON-merge in plan only. Pass33 resolves the B-19 SR-16 identifier overload in plan only by splitting `SR-16-CoreBacking`, `SR-16-RouteFacade`, and `SeedCorpus-LanguageTranslate`. Pass34 classifies B-10 AD-detect / graph-order no-hidden-determinism as `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`: the design fields are present, while AD-detect/permutation/ablation/fresh-load/continue-training evidence remains future execution proof. Pass35 classifies `SeedCorpus-LanguageTranslate` as `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING` by defining `SeedCorpus-LanguageTranslate-v1`, a self-authored synthetic local fixture contract planned for `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl`; the file is not created and no tests/training ran. Historical/pre-Pass39 B-22 propagation or closure wording is superseded by the Pass39 table; this does not close readiness, provider terms proof, future implementation/test/runtime/host/provider/git proof, or the 50-pass evidence chain.

## Purpose

This library collects planning evidence for `ILearningDynamicGraph` and DPO over `ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit`. It is for planning, not implementation: separate current Luba requirements from evidence-only materials, lock legal gates, describe the learning hierarchy, and prepare the stage queue without running code, tests, builds, training, or git.

## Document Map

| file | use in planning | status |
|---|---|---|
| `active-codex-execution-16-full-task-coverage-simulation-gate.md` | Historical/pre-post-repair controlling downgrade/backlog gate at that time after attachments 0ce0 and 912; records evidence-only coverage counts, historical/pre-post-repair `PLAN_NOT_READY`, and Pass06 B-1..B-22 repair backlog with `MISSING_NUMBER_NEEDS_TRACE_REPAIR` for missing/ambiguous blocker numbering such as B-5. | current_control_packet_plan_not_ready_after_pass06_attachment_912 |
| `active-codex-execution-plan-readiness-repair-20260629.md` | Historical/pre-0ce0 narrower repair for Luba instruction `Plan is not ready for execution - needs repair`; attachment 429f defect extraction, source-map rows, resolved/partial decisions, plan-readiness gates, implementation authorization NO | historical_pre_attachment_0ce0_narrower_repair |
| `claude-00-library-index.md` | Library navigation, population status, future evidence tasks | populated_now |
| `claude-01-authority-requirements-ledger.md` | Binding requirements ledger from the current work order and mapping to gates | populated_now |
| `claude-02-source-inventory-and-reference-map.md` | Source categories, exact evidence paths, future 20-pass and 40-pass collection phases | populated_now |
| `claude-03-ilearningmachine-pattern-notes.md` | Brief pattern notes for hierarchy `ILearningMachine -> ILogicalOrchestrator -> ILogicalFlow -> ILogicalUnit` | populated_now |
| `claude-04-dpo-legal-model-policy.md` | Legal model policy, closed API prohibitions, provenance and provider-terms gate | populated_now |
| `claude-05-dpo-data-training-measurement-guide.md` | DPO pair generation, judging, measurement, anti-deterministic-training warnings | populated_now |
| `claude-06-ilearningdynamicgraph-architecture-brief.md` | Architecture brief, data flow, graph state, async routing, Mermaid diagrams, solved subtasks | populated_now |
| `claude-07-execution-gates-open-questions.md` | Gates, stage queue, acceptance criteria, and final Luba questions section only | populated_now |
| `active-codex-execution-two-phase-local-remote-overlay.md` | Единый владелец двухфазной структуры teacher-маршрута: Этап 1 TP1_LOCAL (локальный Ollama-учитель, профиль qwen35-local-v1), Этап 2 TP2_HOSTED (hosted qwen3-together-v1 и др. допущенные); UML фаз, матрица этапов, критерии перехода | single_owner_two_phase_structure_active |

## Library Status

| area | collected now | future or not done |
|---|---|---|
| Current Luba authority | Current work order captured in `claude-01` | No implementation approval invented |
| Existing plan evidence | Existing plan path recorded as evidence-only | Old plan commands not reactivated |
| Core contracts | Exact contract paths recorded from `00.Core` and related project files | Historical/pre-closure wording "Full 40-pass core-doc pass still future" is superseded: the 40-pass core-doc library pass is complete at `40/40` (see row below, `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md`) as evidence-only knowledge-library closure; full cross-repo class audit remains future |
| ILearning pattern | Main hierarchy and lifecycle summarized | Full cross-repo class audit still future |
| DPO mechanics | Existing `DpoPair`, `DpoTrainer`, routing DPO precedent summarized | No training run performed |
| Legal model policy | Closed API prohibition and provider-terms gate defined | No internet or provider terms check performed |
| R-B `llm mvp` docs/background library | Active target artifact complete | `active-codex-execution-knowledge-library-llm-mvp-20-pass.md`; current accepted focused-pass count is `20/20` |
| R-B `llm_mvp_core` docs+code library | Active target artifact materialized | `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md`; current accepted focused-pass count is `40/40` |
| Execution gates | Stage queue and acceptance criteria written; attachments 0ce0 and 912 supersede the narrower blocker-1..4 ready verdict and historical/pre-post-repair plan readiness at that pass was `PLAN_NOT_READY`; historical/pre-43c2 post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable readiness after 0aa/Laplace/Kepler evidence was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains NO | Fresh Luba implementation authorization required only after full-task coverage and zero blockers are proven, then Luba approves the reviewed plan |

## PASS17 B-22 Active-vs-Claude Reconciliation Snapshot

PASS30 extends the B-22 map with design-conversion evidence-boundary precedent. PASS31 adds source/provenance/legal helper precedent for TeacherProvenance, provider/license verdicts, common/adaptive scope, and contamination quarantine boundaries. PASS32 adds unit-level graph DPO pair-shape precedent and rejects arbitrary whole-graph DPO pair semantics without a new explicit Luba decision. PASS33 adds checkpoint manifest, fresh-load, promotion, rollback, and contamination lifecycle precedent so model lifecycle cannot be hidden as a future gate. PASS34 adds async combination scheduling and bounded-concurrency precedent so N CombinationPlan execution semantics cannot remain ambiguous. PASS35 adds graph scheduler contract and Gate B test-expectation precedent for ordering, caps, cancellation, timeout, partial failure, and queue/backpressure. PASS36 adds external API seam/mock/unit-test/replacement-plan precedent and preserves the `codex-external-calls-mock.md` debt rule as the only allowed technical debt. PASS37 adds source-addressability and `SourceHandleId` precedent so active source maps cannot rely on vague docs/code references. PASS38 adds repo authority, separate workdir, `DPOLegalLearning`, and no-write mergeability-before-commit/push precedent without performing git actions. PASS39 adds internal consistency and stale-status QA sweep precedent. PASS40 closes only the llm_mvp_core 40-pass knowledge-library target as evidence-only corpus synthesis; active plan consumers, readiness review, implementation, tests/builds/runtime/provider/training/checkpoint/git proof, and Luba implementation approval remain outside this target. File count is inventory only: the active family includes deltas, ledgers, gates, repair artifacts, and now two R-B target knowledge-library artifacts. Extra active files do not outrank Claude baseline by recency, size, or count. Current Codex recon files and `codex-knowledge-library-*` files remain evidence-only unless an active map accepts a specific non-contradictory delta. The only active R-B count authorities are `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` (`20/20`) and `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` (`40/40`).

| category | Claude baseline source | active counterpart | PASS17 classification | B-22 boundary |
|---|---|---|---|---|
| master plan | `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | `ACTIVE_HAS_DELTA` | Active has pass overlays and B-row repairs; next reconciliation must prove every Claude decision/explanation is preserved or mark a Luba-approved delta. |
| execution-control board | `claude-EXECUTION-CONTROL-BOARD.md` | `active-codex-execution-EXECUTION-CONTROL-BOARD.md` | `ACTIVE_HAS_DELTA` | Active board carries newer repair text; it must remain subordinate to Claude baseline plus accepted deltas. |
| requirements coverage matrix | `claude-REQUIREMENTS-COVERAGE-MATRIX.md` | `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md` | `ACTIVE_HAS_DELTA` | R21/R22/R23 additions are active deltas, not replacement of Claude R1-R20. |
| plan blocks 00-14 | `claude-plan-00-*` through `claude-plan-14-*` | matching `active-codex-execution-plan-00-*` through `active-codex-execution-plan-14-*` | `ACTIVE_HAS_DELTA` | Historical/pre-Pass39 snapshot: B-22 remained open at that time until block-by-block baseline preservation was checked. Current B-22 blocker-inventory classification is the Pass39 table, not this old row. |
| guide set 01-05 | `claude-guide-01-*` through `claude-guide-05-*` | matching `active-codex-execution-guide-01-*` through `active-codex-execution-guide-05-*` | `ACTIVE_HAS_DELTA` | Counterparts exist; guide-specific Claude details still need direct reconciliation. |
| library files 00-07 | `claude-00-*` through `claude-07-*` | matching `active-codex-execution-00-*` through `active-codex-execution-07-*` | `ACTIVE_HAS_DELTA` | Active 00/02 now carry B-22 authority wording; remaining 01/03/04/05/06/07 must still be checked for drift. |
| Codex recon/library evidence | no Claude recon-file namespace; Claude matrix treats Codex recon as evidence-only | Codex has recon files + 2 evidence-only knowledge libraries; active-codex now has 2 target library artifacts; current counts are llm mvp `20/20` and llm_mvp_core `40/40` | `LLM_MVP_AND_CORE_TARGETS_COMPLETE_EVIDENCE_ONLY` | Codex recon cannot close readiness or replace Claude. B-21/R-B closes only through exact accepted focused-pass rows in the active target artifacts, or a later explicit Luba narrowing decision. |

Historical/pre-Pass39 B-22 gate snapshot: at Pass17, active-codex remained `PLAN_NOT_READY` until every Claude baseline category was either `ACTIVE_MATCHES_CLAUDE_BASELINE` or had an explicit Luba-approved/accepted active delta with source evidence and no lost Claude detail. Current B-22 blocker-inventory status is owned by the Pass39 table as `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`; this is not readiness, implementation authorization, or execution proof.

## Pass25 Source-to-Plan 64/B-22 Continuation Index

Open `active-codex-execution-02-source-inventory-and-reference-map.md` first for the current Pass26 Source-to-Plan F5 measurement/quality/lifecycle mirror and Pass 17 Source-to-Plan 64/B-22 Reconciliation Contract. Pass18 enumerates the denominator roster in plan-14 section 3.2; Pass26 accepts 46 direct/planned-contract delta rows at planning/source-map level. No pass claims `64/64`, B-22 closure, plan readiness, Claude Repair2 acceptance, or implementation authorization.

Continuation order for target 64/B-22 repair:
1. Use `active-codex-execution-plan-14-stage-references.md` section 3.2 Pass26 row-evidence repair for the canonical 64 row ids/categories and accepted-delta boundary.
2. Use the source map Pass26 mirror to keep the current boundary and downstream mirrors aligned.
3. Mirror accepted rows into the master plan, board, requirements matrix, and full-task gate.
4. Keep rows open unless the state is `MAPPED_VERIFIED` or `MAPPED_ACCEPTED_DELTA`.
5. Route unresolved rows as `PARTIAL_NEEDS_OWNER_REVIEW`, `MISSING_ACTIVE_COUNTERPART`, `STALE_HISTORICAL_ONLY`, `CLAUDE_REPAIR2_UNVERIFIED`, `NEEDS_CODE_REF_INVENTORY`, or `ROSTER_ONLY_NEEDS_SOURCE_REF`.

Historical/pre-Pass39 boundary after Pass26: source-to-plan roster rows = 64; MAPPED_VERIFIED = 0; MAPPED_ACCEPTED_DELTA = 46; PARTIAL_NEEDS_OWNER_REVIEW = 0; NEEDS_CODE_REF_INVENTORY = 18; ROSTER_ONLY_NEEDS_SOURCE_REF = 0; 64/64 completion was not claimed; B-22 remained open at that time; verdict was PLAN_NOT_READY; implementation authorization was NO. Current status is different: Pass30 accepts Source-to-Plan as 64/64 inventory evidence only, and Pass39 classifies B-22 as RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING, not readiness, not implementation authorization, and not execution proof. The earlier phase-row details on that old line are historical/pre-Pass39 inventory evidence only; DynamicGraph/DPO/F4/F5 planned contracts and all tests/builds/runtime/training/checkpoint/provider/git proof remain future evidence after approval.

## Usage Order

1. Read `active-codex-execution-16-full-task-coverage-simulation-gate.md` first for the historical/pre-post-repair `PLAN_NOT_READY` verdict after attachments 0ce0 and 912, full-task coverage counts, B-1..B-22 backlog, missing-number trace defects, and implementation authorization NO.
2. Read `active-codex-execution-plan-readiness-repair-20260629.md` only as historical/pre-0ce0 narrower repair evidence.
3. Read `claude-01` to determine what is binding in the Claude baseline.
4. Read `claude-02` to separate collected evidence from future collection.
5. Read `claude-03` and `claude-06` together for architecture shape.
6. Read `claude-04` before accepting any teacher, judge, provider, or training source.
7. Read `claude-05` before creating DPO pairs or metrics.
8. Read `claude-07` before issuing any execution work order.

## Completion Boundary

The planning artifact set is populated; the 20-pass `llm mvp` docs/background target is complete (`20/20`), and the 40-pass `llm_mvp_core` docs+code library target is complete at `40/40` as evidence-only knowledge-library closure (`active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md`). Historical/pre-closure wording that listed the 40-pass library among not-complete items is superseded by this line. It does not claim that product implementation, model training, provider legal checks, dual-path host proof, tests/builds, git actions, or the plan-readiness review floor are complete; the 40-pass closure is evidence-only corpus synthesis, not readiness or implementation authorization.
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

Current 2026-06-29 planning-repair boundary: Luba's latest unquoted instruction says Plan is not ready for execution - needs repair. This worker is authorized only to repair active plan files in this folder. Product implementation, implementation workdir setup/copying, repo inventory, tests/builds, git actions, commits, pushes, PRs, merges, and mock-debt file edits are NOT AUTHORIZED in this task. Any earlier wording that implied starting implementation after coherence is superseded until a later plan-readiness gate truly passes and Luba gives fresh implementation authority.

Source `claude-*` and `codex-*` files remain read-only. In this planning-repair task, PRs, merges, commits, pushes, branch switches, workdir copying, and implementation repo edits are forbidden. Future implementation commits/pushes may be considered only in separate implementation workdirs after plan readiness, a bounded sub-agent work order, and the mergeability/no-conflict gate pass; PRs and merges still require separate future Luba instruction.

Pass29 status/readability override for the current worker boundary: the quoted Luba instruction above is historical 2026-06-29 planning-repair evidence. Historical/pre-post-repair active-codex status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`; the Pass30 Source-to-Plan 64/64 state is inventory evidence only; any mojibake in older Pass27/Pass28/Pass29 index headings or counters is historical/pre-0aa evidence and is superseded by the clean current overlays below.
## PASS27 Original-Task Coverage And Latest Baseline Reconciliation

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. See `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md` for the controlling latest-baseline reconciliation context. Current active-codex canon after latest-baseline reconciliation plus Pass30 F9 consolidated test-strategy source-map repair: 12 phases / 27 subphases / 64 substeps; `TeacherProvenance` 20 fields (13 base + 5 NEW + 2 RR6 `service_mode`/`grounding_used`); exactly one active pilot `language-translate`; Source-to-Plan inventory rows are 64/64 accepted as source-map inventory evidence only; hosted Qwen3/Together train use remains blocked until provider terms proof; held-out scorer defaults to `RuntimeProductAccuracy` plus per-domain scorer (`language-translate`: exact-match + char-accuracy). Any older unqualified 12/26/59, 18-field, 8/64 (partial; historical/pre-PASS27 corrected from 8/59), 28/36, 31/33, 34/30, 41/23, 46/18, 50/14, 54/10, 58/6, incomplete-source-inventory, or narrower ready wording in this file is historical/pre-0aa evidence only.

## Pass28 Active Index Addendum - F7 Auto-DPO Loop Source-to-Plan

Historical/pre-Pass29 note: after Pass28, the active index pointer for Source-to-Plan was `active-codex-execution-plan-14-stage-references.md` Pass28 F7 source inventory: total=64; `MAPPED_ACCEPTED_DELTA=54`; `NEEDS_CODE_REF_INVENTORY=10`. The historical/pre-Pass30 Pass29 pointer is recorded below.

Pass28 changed only active-codex planning artifacts for the four F7 rows. It accepted weakness/low-result detection, legal TeacherGoal/TeacherProvenance routing, unit-level `GraphDpoPair` plus replay, `AutoDpoLoopOrchestrator` prepare/run/train/evaluate shape, and `ShouldStop` threshold/plateau/budget/no-signal/promotion contracts as source-map deltas. It did not edit source code, run tests/builds/runtime/provider/API/network/git, prove training, or authorize implementation.

## Pass29 Active Index Addendum - F8 No-Hidden-Determinism Source-to-Plan

Historical/pre-Pass30 pointer: after Pass29, Source-to-Plan pointed to `active-codex-execution-plan-14-stage-references.md` Pass29 F8 source inventory: total=64; `MAPPED_ACCEPTED_DELTA=58`; `NEEDS_CODE_REF_INVENTORY=6`.

Pass29 changed only active-codex planning artifacts for the four F8 rows. It accepted the 9 deterministic/baseline mechanism inventory, replacement blueprints, `CandidateOrderLeakageGuardUnit` permutation guard, and latest-wins cutover plus ablation gate as source-map deltas. It did not edit source code, run tests/builds/runtime/provider/API/network/git, prove training, prove hidden-determinism removal, execute ablation, or authorize implementation.

## Pass30 Active Index Addendum - F9 Consolidated Test Strategy Source-to-Plan

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Active index current pointer for Source-to-Plan is now `active-codex-execution-plan-14-stage-references.md` Pass30 F9 consolidated test-strategy source inventory: total=64; `MAPPED_ACCEPTED_DELTA=64`; `NEEDS_CODE_REF_INVENTORY=0`. Source-to-Plan inventory rows are 64/64 accepted as inventory evidence only.

Pass30 changed only active-codex planning artifacts for the six F9 rows. It accepted topology mutation fan-out/fan-in/many-to-many tests, edge determinism/leakage/ablation tests, DPO generator plus `TeacherProvenance`/closed proprietary `DATASET_REBUILD_REQUIRED` gate tests, continue-training merge/duplicate/empty-state tests, component-to-layer Gate matrix, and regression/anti-loophole report as source-map/test-design deltas. It did not edit source code, run tests/builds/runtime/provider/API/network/git, prove training, prove product readiness, close B-22, claim PLAN_READY, or authorize implementation.

## Pass36 Active Index Addendum - Qwen3/Together Provider-Terms Classification

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass38 supersedes the older Pass36 current-looking Qwen3/Together route wording. Current hosted route status is `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`: latest Claude keeps Qwen3 Apache-2.0 as the selected/recommended teacher blueprint and `qwen3-together-v1` / TogetherAI as the selected v1 hosted-provider recommendation, while TogetherAI terms/output-use proof and Luba hosted-use approval remain blocking proof gates before any hosted train/common use. Fireworks/OpenRouter/DashScope are deferred alternates only if a later plan explicitly selects and proves them.

Qwen3 model-license evidence is Apache-2.0 model-weight evidence only. Hosted TogetherAI use still requires a future `ProviderTermsProof` bundle before train/common use: `ProviderTermsProof.Provider=TogetherAI`, `ModelFamily=Qwen3`, exact `ModelId`, Qwen Apache-2.0 source ref, Together ToS snapshot, Together privacy/security/ZDR organization-setting snapshot, `passthrough` disabled or upstream policy captured, `service_mode=plain`, `grounding_used=false`, `provider_terms_checked=true`, non-pending `terms_snapshot_ref`, `data_retention_mode=ZDR_or_no_store`, `output_ownership_checked=true`, `third_party_model_terms_checked=true`, and a Luba hosted-use approval ref.

Until that proof passes, hosted Qwen3/Together train/common use routes to `BLOCK` or `QUARANTINE`, not train-ready/common-ready. Judge/eval-only use is allowed only with `used_for_training=false`, non-pending terms, organization privacy/ZDR proof, no passthrough gap, no grounding/search, and no sensitive/private raw data. This addendum does not call any provider API, does not obtain legal approval, does not close B-22, and does not authorize implementation. Historical/pre-Pass39 B-22-as-current-design-blocker wording is superseded by the Pass39 table; all implementation/test/runtime/provider/git evidence remains future work after readiness and Luba approval.

## Pass37 Active Index Addendum - Latest Claude Baseline Sync And 50-Pass Guard

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass37 supersedes active Pass36 wording only where Pass36 classified hosted Qwen3/Together as optional disabled-by-default or no longer a readiness/design blocker. Latest inspected Claude baseline instead keeps `qwen3-together-v1` as the selected v1 provider config/recommendation with `ProviderTermsChecked=false` and `TermsSnapshotRef=PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)`; train/common use remains `BLOCK` or `QUARANTINE` until future terms proof and Luba hosted-use approval. Qwen3 Apache-2.0 is model-license evidence only.

Pass37 current owner map: `GraphDpoPair` canon is owned by active plan-07 / Claude plan-07 and has exactly 14 fields; `TeacherProvenance` canon is owned by active plan-08 / Claude plan-08 section 3.1 and has 20 fields including RR6 `service_mode` and `grounding_used`; B-16 fan-in merge returns `GraphFanInMergeResult` with typed `MergeRole=FanInMergeRole.DeterministicFanInBaseline` and `MergeOrderIsAuthority=false`.

Pass37 record: `active-codex-execution-37-pass37-claude-baseline-sync.md`. It is one bounded sync/simulation pass, not the required 50-pass readiness simulation. No `PLAN_READY`, `FULLY_READY`, B-22 closure, or implementation authorization may be claimed until at least 50 sequential simulation/review evidence records exist and are reconciled against active-codex plus the latest Claude baseline.

## Pass39 Active Index Addendum - Current B-Inventory

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass39 supersedes older current-looking index rows that say B-22 is still open for
blocker-inventory status. The Pass39 B-1..B-22 owner is the Pass39 table in
`active-codex-execution-16-full-task-coverage-simulation-gate.md` plus the Pass43
B-18/B-21 default-defer repair: B-2 is the hosted-provider/Luba approval proof
residual before hosted train/common use; B-18 is not a current v1 blocker because
v1 is common-only and adaptive activation is deferred unless Luba later expands
scope; B-21 is not a current v1 blocker because first substantive recon plus
source-map inventory is sufficient for current v1 planning unless Luba later
reopens required depth. The remaining B rows are resolved in active plan with
execution proof pending. This addendum does not claim readiness, 50 passes,
provider/legal approval, implementation, tests, builds, runtime, browser, or git proof.

## Pass44 Active Index Addendum - Hosted Proof Is Route-Specific

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass44 supersedes any current-looking wording that can be read as making missing hosted Qwen3/Together or TogetherAI proof a whole-plan or local-v1 blocker. The executable v1 design path after future Luba approval is local/self-authored/self-best first: `SeedCorpus-LanguageTranslate-v1`, project-internal self-authored rows, legal self-best rows, and legally proven common-eligible examples. Hosted Qwen3/Together is selected/recommended only as a hosted teacher blueprint; it remains quarantined for train/common use until `ProviderTermsProof`, output-use/training-rights proof, and Luba hosted-use approval are captured. Missing hosted proof blocks only hosted train/common use and may allow judge/eval/mock-only treatment where the legal plan permits; it does not authorize implementation and does not change the 50-pass readiness requirement.

## Two-Phase Teacher Rollout Index Addendum (2026-07-13)

Единый владелец структуры двухфазного teacher-маршрута —
`active-codex-execution-two-phase-local-remote-overlay.md` (имена этапов
`TP1_LOCAL` / `TP2_HOSTED`, UML фаз, матрица этапов, критерии перехода
G-TP-1..G-TP-7, легальный инвариант). Профиль-блок учителей `qwen35-local-v1` /
`qwen3-together-v1` = владелец `active plan-08 §3.5.2`, создан Проходом 2
(2026-07-13): qwen35-local-v1 — полный канон §3.5.2, qwen3-together-v1 — канон
§3.5.1/§3.5.1b с указателем в §3.5.2. Этот addendum — только
навигация: readiness (`PLAN_NOT_READY`, implementation authorization `NO`),
счётчики 12/27/64, `TeacherProvenance` (20 полей, plan-08 §3.1) и `GraphDpoPair`
(14 полей, plan-07) НЕ меняются. Closed proprietary teachers запрещены в обеих
фазах.

Навигация детализации Проходов 3/4/5 (только указатели, канон — в файлах-владельцах): teacher-контракт этапа = plan-09 §3 Шаг 3a; lifecycle этапов = plan-10 §8.1 `stage_scope_gate` + Pass16 контракт (terms-гейт по составу строк); измерение разрыва учителя = plan-11 §7 (`TeacherStageGapReport`/`StageGap9bVs235b`); детерминизм tie-break = plan-12 §3.B.2 (SHA-256 тройное зеркало); тесты двухфазности = plan-13 §3.1a (13 строк a..m). Проход 5 (2026-07-13) добавил TP5-overlay/навигацию/coverage в матрицу, 07, 02, plan-14, 16 и F0, а также владелец-список открытых Luba-решений `Q-TP-*` (07 финальная секция + F0). readiness остаётся `PLAN_NOT_READY / TWO_PHASE_ADAPTATION_QUEUE`, impl-auth `NO`; счётчики не меняются.
