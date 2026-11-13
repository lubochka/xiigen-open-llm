## Current Published Implementation State Overlay (2026-07-14)
> Пометка 14.08.26: упоминания ветки `DpoImplementation` в этом файле — historical; ветка-канон = `codex/dpo-implementation` (см. 14.08.26WorkLeft-05-branch-reconciliation-codex-dpo-implementation.md). Overlay 2026-07-09 ниже (branch `DpoImplementation` per repo) — historical для `llm_mvp_core`; для 4 не-core репо имя локальной ветки этим проходом не переадъюдицировано (upstream = `origin/codex/dpo-implementation`). Пути `C:\Projects\<repo>-DpoImplementation` — каталоги, не ветки.

This evidence-bound overlay supersedes the 2026-07-09 “no commit/no push yet” status and any use of `origin/DpoImplementation` as the current publication target. Local core worktree `llm_mvp_core-DpoImplementation` is checked out on `codex/dpo-implementation` (ветка-канон исправлена 14.08.26; прежде ошибочно DpoImplementation); the other worktrees' local branch label is not re-adjudicated by the 14.08 sweep; their active upstream/publication target is `origin/codex/dpo-implementation`. The core repository's older `origin/DpoImplementation` is divergent legacy history and must not be merged, reset, force-pushed or treated as the current target without a separate explicit decision.

| Repository worktree | Current accepted/pushed tip | Current observation |
|---|---|---|
| `C:\Projects\llm_mvp_core-DpoImplementation` | `e2a6bce7448a1672a90bdec95106da279b077c5b` | clean; local HEAD equals `origin/codex/dpo-implementation` |
| `C:\Projects\xiigen-platform-dot-net-angular-DpoImplementation` | `2763345f377` | clean; upstream `origin/codex/dpo-implementation` |
| `C:\Projects\FigmaToCode-design-gen-DpoImplementation` | `44dd7ec59e` | clean; upstream `origin/codex/dpo-implementation` |
| `C:\Projects\CanvaToCode-DpoImplementation` | `82b1debf0d` | clean; upstream `origin/codex/dpo-implementation` |
| `C:\Projects\xiigen-flows-dot-net-angular-DpoImplementation` | no new accepted tip in this packet | clean; no new accepted-slice claim |

These are source/publication pointers, not composite readiness claims. Each subsequent independently accepted slice requires its own commit, successful push, remote-tip equality check and clean worktree before the next slice starts. Core slices 07-13 are separate accepted/pushed commits: `408044656911d0b5766d71d9723d661e310156ce` multi-combination runner; `62c1e996670d8a7786dcf0b4545b5f82e8937254` deterministic topology builder; `382544dd7cfc2a6ff473acdb1b5a6abf82a18064` revisioned runtime owner; `002bd7f1523f8c3802a341438cf914bc1a37d12d` logical-registry projection; `d489507e1e9fb0350c541fc1ac3dd5786e01500d` production L4 read-only consumer; `16e92b46cc4f57c3a73341902a853d86a5c17267` production bootstrap facade; and `e2a6bce7448a1672a90bdec95106da279b077c5b` explicit production composition profile/caller. Evidence for slices 07-11 remains recorded in the code-first audit. Slice 12 passed focused `14/14`, the required six-class packet `132/132`, seven classes including Adaptive `156/156`, and full build `0` warnings / `0` errors. Slice 13 passed focused `20/20`, runtime `14/14`, DynamicGraph `271/271`, Assistant+Adaptive `44/44`, and full build `0` warnings / `0` errors. Every slice received independent acceptance before publication.

Slice 13 is the first production caller outside `Tests`: it accepts an explicit profile, snapshots it, creates descriptor-only nodes and delegates once to the slice-12 runtime facade. This does not supply a deployment host. The current manual-only chain starts with identifying a real deployment host/reference seam; that host must deserialize/select the profile, own the runtime lifetime, invoke `Runtime.Assistant`, and later supply persistence/restart for the selected profile and revision/CAS state. Authority remains metadata-only: the present edge fields are insufficient to implement authority execution, so authority schema/canon/resolver design remains a later explicit prerequisite, not an inferred capability. The exact next bounded implementation slice is not selected by this source map. Whole `Ф1.1.3`, `Ф1.3.2`, `Ф6.1.1`, Gate B, full machine and DPO remain open at their composite boundaries. Teacher/DPO/training sequencing is not owned here; use `active-codex-execution-two-phase-local-remote-overlay.md` and preserve `CODE_FIRST -> TP1_LOCAL eval-only -> TP2_HOSTED eval-only -> G-DPO-FINAL -> DPO/training`. TP1, TP2 and DPO/training are still not started, and closed-proprietary outputs remain prohibited as DPO authors/training targets.

## Current Implementation Branch/Workdir Convention Overlay (2026-07-09, Luba live authority)

authority: Luba current unquoted instruction 2026-07-09 — create per-repo branch `DpoImplementation` from each repo's latest-updated branch, place its worktree at `C:\Projects\<repo-name>-DpoImplementation`, and make that branch the folder's main (HEAD).

current_convention (supersedes the older `DPOLegalLearning` work-branch and `C:\Projects\DPOLegalLearning\<repo>` workdir convention wherever it appears in this plan family):
- implementation branch name per repo: `DpoImplementation` (NOT a branch named `DPOLegalLearning`). — historical (2026-07-09); с 14.08.26 ветка-канон для `llm_mvp_core` = `codex/dpo-implementation` (ветка-канон исправлена 14.08.26; прежде ошибочно DpoImplementation).
- implementation workdir per repo: git worktree at `C:\Projects\<repo-name>-DpoImplementation`, with `DpoImplementation` as HEAD (main/active branch of that folder). (HEAD `DpoImplementation` — historical; канон 14.08.26 для core = `codex/dpo-implementation`; путь каталога не меняется).
- mechanism: `git worktree add -b DpoImplementation "C:\Projects\<repo>-DpoImplementation" <latest-updated-source-branch>`; source branch stays read-only authority.
- participating repos + source startpoint (latest updated branch per repo, verified 2026-07-09): llm_mvp_core -> DesignToCode-clean; xiigen-platform-dot-net-angular -> DesignToCode-pass75; xiigen-flows-dot-net-angular -> DesignToCode-pass70; FigmaToCode-design-gen -> DPOLegalLearning (that repo's latest-updated branch); CanvaToCode -> DPOLegalLearning (that repo's latest-updated branch).
- planning repo `dpo legal build` gets no branch (no-branch policy unchanged).
- status 2026-07-09: all 5 worktrees created and populated, clean; no commit, no push yet; pre-commit/pre-push mergeability check into the original source branch AND `main` remains required before the first commit (unchanged OP-67 / Pass 14 gate).

name_scope_rule (avoid dual-live-canon): the DPO effort's TASK/PROJECT/work name remains "DPO Legal Learning" / `DPOLegalLearning` — this overlay does NOT rename the task. ONLY the implementation BRANCH name and the implementation WORKDIR PATH change.

historical_classification_rule: any older line in this plan family stating the implementation workdir as `C:\Projects\DPOLegalLearning\<repo>` or the implementation work BRANCH as `DPOLegalLearning` is historical/pre-2026-07-09 evidence only, superseded by this overlay, unless a later unquoted Luba instruction changes it again. Older `DPOLegalLearning` mentions that name the TASK/PROJECT remain current.

## Fable Coverage Repair Overlay (2026-07-05)

plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
implementation_authorization: NO until fresh Luba approval after review
current_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` supersede the previous final-readiness overlay. Fable reports 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers: R-B `llm mvp` 20-pass and `llm_mvp_core` 40-pass knowledge-library targets are accepted only as evidence-library closure, not readiness/implementation proof; R-G source addressability is incomplete; several shared contracts/readability/lifecycle carriers need plan-family repair. This is plan repair evidence only, not product implementation, not tests/builds/runtime/provider/training/checkpoint/git proof.
current_verdict_classes: DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS; CURRENT_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=previous 50/50 planning-section acceptance remains historical/process evidence only and cannot override later Fable MISSING/PARTIAL findings; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, Final Readiness Review Overlay, no unresolved design/dependency blocker, CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW, or parent final-readiness acceptance before this overlay is historical/pre-Fable evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md is the current repair map for R-B/R-G and the partial contract/readability/lifecycle additions.

## Pass31-Pass40 Consumer Sync (40-pass llm_mvp_core evidence library)

sync_scope: evidence-only propagation from `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` Pass31/40 through Pass40/40. Current verdict remains `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`; `implementation_authorization: NO`.

| SourceHandleId | source path | role | consumer boundary |
|---|---|---|---|
| RB40-P31-PROVENANCE | `C:\Projects\llm_mvp_core` targeted docs/code evidence in Pass31/40 | provenance/legal precedent | Carries `GraphDpoSourceRow -> GraphDpoPair -> TeacherProvenance -> legal verdict -> training batch -> checkpoint manifest -> promotion gate`; no provider terms proof was run. |
| RB40-P32-GRAPH-DPO-PAIR | `C:\Projects\llm_mvp_core` targeted DPO and learning-contract evidence in Pass32/40 | unit-level pair shape | `GraphDpoPair` remains a graph-context envelope around a unit-level row, not a whole-graph pair. |
| RB40-P33-CHECKPOINT | `C:\Projects\llm_mvp_core` targeted lifecycle evidence in Pass33/40 | checkpoint/export lifecycle | Requires `GraphCheckpointManifest` / `GraphLearningExport` linkage to held-out, ablation, promotion, rollback, and contamination rebuild evidence. |
| RB40-P34-P35-SCHEDULER | `C:\Projects\llm_mvp_core` targeted async/scheduler evidence in Pass34/40-Pass35/40 | combination scheduling | Distinguishes `MaxCombinations`, `MaxParallelCombinationRuns`, and `MaxParallelBranches`; tests remain Gate B execution evidence. |
| RB40-P36-MOCK-SEAM | `C:\Projects\llm_mvp_core` targeted fake/mock evidence in Pass36/40 | external-call seam | Future implementation must use deterministic mock/fake expected responses plus replacement-plan row at `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`; this file was not created here. |
| RB40-P37-SOURCE-HANDLE | `C:\Projects\llm_mvp_core` targeted source-addressability evidence in Pass37/40 | source-map schema | Each source row needs `SourceHandleId`, repo root, relative path, optional line range, role, owner plan section, status, evidence boundary, and last verified date. |
| RB40-P38-REPO-AUTHORITY | `C:\Projects\llm_mvp_core` planning guidance evidence in Pass38/40 | future repo/workdir gate | Later implementation must rediscover latest-updated source branch per repo, work under `C:\Projects\DPOLegalLearning\<repo>`, and run no-write mergeability checks before commit/push; no PR/merge is authorized here. (superseded — see Current Implementation Branch/Workdir Convention Overlay 2026-07-09) |
## Current R-G Source Addressability Repair (2026-07-05)

This section supersedes older short-name or wrong-path rows for the named original-task source materials. It is source-addressability evidence only: it does not claim recon completion, implementation, tests, builds, provider proof, git work, or plan readiness.

| source group | exact current path | role in the plan | target plan consumer (navigation row) | current evidence |
|---|---|---|---|---|
| Planning library | `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` | active planning documents, `claude-*` baseline, `active-codex-execution-*` active plan surface | `active-codex-execution-*` active plan surface + plan-14 §5 navigation | `PATH_EXISTS_VERIFIED` |
| `llm mvp` background root | `C:\Users\lubao\Documents\xiigen\llm mvp` | required >=20-pass background/documentation knowledge-library corpus | `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` (R-B evidence library) | `PATH_EXISTS_VERIFIED`; R-B closed as evidence-only: 20-pass library accepted `20/20` after parent verification (`active-codex-execution-knowledge-library-llm-mvp-20-pass.md`), whole-root folder inventory present. Prior "library records still incomplete" is historical/pre-2026-07-05-closure evidence only; current verdict is evidence-library-closed, not readiness. |
| `Small model racional` | `C:\Users\lubao\Documents\xiigen\llm mvp\Small model racional` | DPO / learning-machine background reference | plan-14 R15 §6.3 navigation/reference-only domain `small-model`; 20-pass library MVP-SMALL | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 01, Pass 13 (20-pass library). Prior "needs library pass row" is historical/pre-closure evidence only. |
| `language translate learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning` | active v1 pilot background and seed/scorer context | plan-14 R15 §6.3 active v1 pilot `language-translate` (Ф10.3.1..Ф10.3.5); 20-pass library MVP-LANG | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 02, Pass 03, Pass 10 (20-pass library). Prior "needs library pass row" is historical/pre-closure evidence only. |
| `local wiki learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\local wiki learning` | deferred/reference domain background | plan-14 R15 §6.3 navigation/reference-only domain `local-wiki`; 20-pass library MVP-WIKI | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 04, Pass 11 (20-pass library). Prior "needs navigation/library row" is historical/pre-closure evidence only. |
| `extensions` | `C:\Users\lubao\Documents\xiigen\llm mvp\extensions` | local/offline extension/product boundary reference | plan-14 R15 §6.3 navigation/reference-only domain `extensions`; 20-pass library MVP-EXT | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 06, Pass 14 (20-pass library). Prior "needs library pass row" is historical/pre-closure evidence only. |
| `documentation learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning` | documentation-learning reference domain | plan-14 R15 §6.3 navigation/reference-only domain `documentation-learning`; 20-pass library MVP-DOC | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 05, Pass 12 (20-pass library). Prior "needs library pass row" is historical/pre-closure evidence only. |
| `Planning sessions/session-start-extracted` | `C:\Users\lubao\Documents\xiigen\Planning sessions\session-start-extracted` | original-task session-start material; not under `llm mvp` | plan-14 R15 §6.3 navigation/reference-only domain `session-start`; 20-pass library MVP-SESSION (outside `llm mvp` root, task-named) | `PATH_EXISTS_VERIFIED`; navigation/library rows present: Pass 07, Pass 15 (20-pass library, MVP-SESSION). Prior "needs recon/navigation row" is historical/pre-closure evidence only; previous `C:\Users\lubao\Documents\xiigen\llm mvp\Planning sessions\session-start-extracted` is `PATH_NOT_FOUND_VERIFIED`. |
| core project | `C:\Projects\llm_mvp_core` | implementation authority for DynamicGraph/DPO/learning-machine core | `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` (R-B evidence library) | `PATH_EXISTS_VERIFIED`; 40-pass docs+code library accepted `40/40` as R-B evidence-only closure, not readiness/implementation proof |
| platform facade repo | `C:\Projects\xiigen-platform-dot-net-angular` | public API facade for figma/html-css and better-output->DPO routes | plan-14 R16/R17 seam §6.2.1 `FIGMA_CANVA_DESIGN_TO_HTMLCSS` / `BETTER_OUTPUT_FEEDBACK_TO_DPO` (platform facade) | `PATH_EXISTS_VERIFIED`; previous `C:\Projects\xiigen-platform` is `PATH_NOT_FOUND_VERIFIED` |
| flow seam repo | `C:\Projects\xiigen-flows-dot-net-angular` | FLOW-31/FLOW-38 internal seam for conversion/feedback route | plan-14 R16/R17 seam §6.2.1 FLOW-31 design-analysis / FLOW-38 feedback event | `PATH_EXISTS_VERIFIED`; previous `C:\Projects\xiigen-flows` is `PATH_NOT_FOUND_VERIFIED` |
| Figma client repo | `C:\Projects\FigmaToCode-design-gen` | client/caller surface only, not host authority | plan-14 R16 §6.3 client/caller surface; SCN-R16 Path B host UI | `PATH_EXISTS_VERIFIED` |
| Canva client repo | `C:\Projects\CanvaToCode` | client/caller surface only, not host authority | plan-14 R16 §6.3 client/caller surface; SCN-R16 Path B host UI | `PATH_EXISTS_VERIFIED` |

Readiness impact: R-G source addressability is now CLOSED at navigation level. Every named source folder above carries an exact path, a target plan consumer (navigation row), and an inventory/navigation row (accepted pass rows in the 20-pass library, or the named R-B evidence-library artifact for the two >=N-pass corpora), which satisfies the FABLE-BLOCKER-04 acceptance rule (`active-codex-execution-42-fable-gap-repair-additions-20260705.md` §FABLE-BLOCKER-04) across both required plan targets (this file and `active-codex-execution-plan-14-stage-references.md`). The prior wording "R-G path addressability is partially repaired ... until R-G/source addressability ... are completed" is historical/pre-closure evidence only and is superseded by the navigation rows present in the table above. The R-B evidence-library targets are accepted only as evidence-only closures: `llm mvp` is `20/20` and `llm_mvp_core` is `40/40`. The plan still remains `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE` until active-plan carrier review, remaining Fable MISSING/PARTIAL reconciliation, and a separate readiness review are completed. These navigation and evidence-library closures do not prove PLAN_READY, product readiness, B-22 closure, implementation authorization, tests/builds/runtime/provider/git/training/checkpoint proof, or mergeability.

## Two-Phase Teacher Source Addressability Addendum (2026-07-13, Проход 5)

Русское человеческое резюме: двухфазный teacher-маршрут назвал несколько источников/владельцев. По FC-44 source-addressability каждый названный источник получает shortname -> путь + роль + потребитель + evidence. Это НАВИГАЦИЯ: SR-NN id НЕ присваиваются, Source-to-Plan остаётся `64/64` (map-счёт не меняется). readiness — `PLAN_NOT_READY` (см. TP5/overlay), impl-auth `NO`.

| shortname | путь (физический / секция-владелец) | роль | потребитель | evidence |
|---|---|---|---|---|
| two-phase-overlay | `active-codex-execution-two-phase-local-remote-overlay.md` | единый владелец структуры двухфазного teacher-маршрута (этапы, UML, матрица, критерии G-TP-1..G-TP-7) | matrix/07/16/02/F0/plan-14 (pointer) | PATH_EXISTS_VERIFIED (in-repo) |
| teacher-profile-registry | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 | владелец профиля учителей `qwen35-local-v1` / `qwen3-together-v1` (`TeacherProvenanceProfile`) | plan-09/plan-10/plan-11 | PATH_EXISTS_VERIFIED (in-repo секция) |
| qwen35-9b-weights-source | external: HF `Qwen/Qwen3.5-9B` (planned ref) + `OllamaModelRef` (Ollama-тег, Q4_K_M) | веса локального учителя Этапа 1 | профиль `qwen35-local-v1` (plan-08 §3.5.2) | EXTERNAL_PLANNED; лицензия весов = PROVISIONAL, физическая проверка HF-repo/лицензии — execution-гейт `G-TP-2` (НЕ verified здесь) |
| teacher-stage-gap-carrier | `active-codex-execution-plan-11-measurement.md` §7 | носитель разрыва учителя между этапами (`TeacherStageGapReport`, `StageGap9bVs235b`) | guide-03 (зеркало), overlay §4 | PATH_EXISTS_VERIFIED (in-repo секция) |
| two-phase-negative-tests | `active-codex-execution-plan-13-test-strategy.md` §3.1a | негатив/честность/trained тесты двухфазности (13 строк a..m) | plan-07/08/09/10 (клетки) | PATH_EXISTS_VERIFIED (in-repo секция) |
| stage-scope-lifecycle | `active-codex-execution-plan-10-training-lifecycle.md` §8.1 | `stage_scope_gate` + terms-гейт по составу строк (TP1-only не false-block) | Pass16 контракт, overlay §8 | PATH_EXISTS_VERIFIED (in-repo секция) |
| tie-break-owner | `active-codex-execution-plan-12-no-hidden-determinism.md` §3.B.2 | детерминированный tie-break (SHA-256 тройное зеркало), helpers вне счёта 9 | plan-09 §3 Шаг 3a (зеркало) | PATH_EXISTS_VERIFIED (in-repo секция) |

Термины-указатели: `TP1_LOCAL`/`TP2_HOSTED` = имена ЭТАПОВ (не фазы доски); `qwen35-local-v1` = Qwen3.5-9B/Ollama; `qwen3-together-v1` = Qwen3-235B-A22B (Together). Ни одна строка НЕ создаёт SR-NN id и НЕ добавляет Source-to-Plan row; счёт `64/64` неизменен.

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
current_b_inventory_owner: `active-codex-execution-16-full-task-coverage-simulation-gate.md` section `Current B-1..B-22 Status Reconciliation post-0aa5730c`; current B-row status is B-2 EXTERNAL_PRE_USE_PROOF_GATE, B-8 RESOLVED_SCOPE_EXECUTION_PENDING, B-18/B-21 HISTORICAL_OR_TRACE_ONLY, B-22 RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING, and all other B rows RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING. Current verdict remains PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE; implementation authorization remains NO.
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
artifact_type: source_inventory_reference_map
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: populated_now_future_collection_marked
last_updated: 2026-06-30
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
---

# Source Inventory And Reference Map

## R22.3 Pass 15 Test/Build/Mock Verification Source Map

Pass 15 maps future verification obligations only. It does not authorize implementation, tests/builds, runtime probes, real external calls, network access, provider terms retrieval, or creation of `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`.

| verification route | source/owner surfaces | future required evidence |
|---|---|---|
| Build | `llm_mvp_core`, platform/flows/client repos when touched by approved implementation | Exact build command/output after code exists. |
| Unit-level DPO | block 07 builder plus plan-08 provenance/legal gate | Unit tests for `GraphDpoPair(level=unit)` semantics, no whole-graph train-ready shortcut, and rejection/quarantine paths. |
| External seam mocks | R22 seams: `FIGMA_CANVA_DESIGN_TO_HTMLCSS`, `BETTER_OUTPUT_FEEDBACK_TO_DPO`, `LEGAL_TEACHER_JUDGE_PROVIDER`, `PROVIDER_TERMS_LICENSE_METADATA` | Request fixture, exact expected mock response/result, unit test name, and exact assertion for every seam. |
| Legal/provenance | plan-08, matrix R14/R17/R22, board legal gates | Provider policy, 20-field `TeacherProvenance`, ProviderProof, LicenseProof, terms, service-mode/grounding/search, and sidecar/readback tests. |
| Measurement/scorer | plan-11/plan-13 measurement and scorer routes | Speed/accuracy/quality, `RuntimeProductAccuracy`, `delta`, `RewardMargin`, ablation, fresh-load, continue-training, and baseline-stability tests. |
| Domain E2E activation scope | master Q41/R15, matrix R15/R16, plan-14 section 6.3, board F10.3, current 50-pass ledger | Exactly one active executable pilot: `language-translate`. `local-wiki`, `small-model`, `extensions`, `figma-html-css`, `design-to-code`, and FigmaToCode/CanvaToCode client surfaces remain navigation/reference/deferred until their own seed/data, held-out scorer/gold, legal/provenance, activation substeps, lifecycle, and test bundle exists. Recon files, generic templates, Source-to-Plan rows, and future gates are not E2E proof. |
| Integration/runtime | plan-13 same-scenario dual path, R16/R17 host/API seams | Same-scenario Path A plus Path B evidence with equality tuple and no-secrets redaction. |
| Replacement audit | Future DPO LEARNING mock replacement ledger | After product readiness only, record replacement rows in `codex-external-calls-mock.md`; this is the only allowed technical debt. |

All rows above are continuation checkpoints. If one fails, future bounded repair workers fix the failing slice or record a true external blocker. Missing source-map authority, provider/provenance policy, thresholds, input/output contracts, mock response contracts, or replacement-ledger contracts remain design blockers and cannot be hidden in future tests/builds.

## R22.4 Pass 16 Model/Checkpoint Lifecycle Source Map

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This row is plan/governance repair only and does not claim any model/checkpoint implementation, training success, fresh-load proof, promotion, rollback, or continue-training execution.

| lifecycle surface | controlling active-codex plan evidence | Pass 16 mapped decision |
|---|---|---|
| deterministic baseline vs learned policy | master Pass 16 gate; plan-04 DynamicGraph boundary; plan-10 lifecycle contract | Deterministic traversal/code/config remains baseline/evaluation authority. Learned policy/checkpoint state is persisted separately with manifest and lineage. |
| checkpoint manifest | master Pass 16 gate; plan-10 lifecycle contract; plan-11 measurement; plan-13 tests | Required fields: `model_id`, `checkpoint_id`, `manifest_schema_version`, `graph_schema_version`, `code_source_ref`, `scorer_version`, `threshold_set`, `training_data_refs`, `source_split_refs`, `metrics_snapshot`, `provenance_refs`, `teacher_provenance_coverage`, `legal_verdict`, `scope`, `created_at`, `parent_checkpoint_id`, `rollback_ref`, `compatibility_status`. |
| persistence/export/import | plan-10 lifecycle contract; plan-13 test gate | Export/import is checkpoint plus manifest plus lineage. Import rejects missing fields, incompatible schema/scorer/thresholds, missing source ref, provider-terms-pending, or failed legal/provenance/source split. |
| fresh-load evaluation | plan-11 measurement; plan-13 test gate | A clean-process fresh-load run must record `RuntimeProductAccuracy`, reward margin, delta, ablation, baseline comparison, scorer version, threshold set, and compatibility before product-ready or promotion claims. |
| common/global promotion | master Pass 16 gate; board; matrix; plan-07/09/10/11/13 evidence | Promotion requires legal/provenance/source split pass, held-out thresholds, ablation, no regression versus deterministic baseline and previous checkpoint, no provider terms pending, and recorded source branch/ref. |
| rollback and continue-training | plan-09 promotion decision evidence; plan-10 lifecycle contract; plan-13 tests | Previous accepted checkpoint remains `rollback_ref`; continue-training must load parent checkpoint, record `parent_checkpoint_id`, use approved source-disjoint new rows/refs, and write a child manifest. Retraining from zero is forbidden except explicit cold-start scaffold, which remains untrained/eval/quarantine until gates pass. |

No row above closes readiness or B-22. Missing lifecycle evidence remains repair queue; it cannot be deferred to future tests as an unresolved design decision.

## Collection Boundary

Collected now means targeted evidence read or referenced for library planning. Future means planned collection still required. This file does not claim internet research, provider terms review, model license review, product readiness, implementation authorization, or runtime/provider/training/checkpoint proof from the accepted 20-pass/40-pass evidence-library closures.

## Pass 13 Code Reference Inventory Contract

historical_pre_post_repair_verdict_at_that_pass: `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review: `PLAN_READY_FOR_LUBA_REVIEW`; historical_pre_hubble_plan_readiness_after_latest_plan_partial_reconciliation: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_0aa_plan_readiness: `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation_authorization: `NO until fresh Luba approval after review`
pass_scope: `R-OT-13 code-reference inventory policy and non-fake source anchor map`
subagent_id_or_NONE: `019f1d1c-6b36-7c12-8fae-4d947f2656e9`

This current Code Reference Inventory is plan/governance evidence only. It does not implement product code, run tests/builds/runtime probes, inspect browser routes, call providers or network APIs, create screenshots, use git, or prove implementation exists.

Evidence classes for every code reference in active-codex:

- `PATH_EXISTS_ONLY`: the file path exists, but no source line or quote was verified in the current pass.
- `LINE_REF_VERIFIED`: the exact source line or range was opened and read in the current pass, or a cited owner document keeps its own line-read evidence boundary.
- `QUOTE_VERIFIED`: the exact quoted source text was opened and read in the current pass, or a cited owner document keeps its own quote-read evidence boundary.
- `NEEDS_CODE_REF_INVENTORY`: future implementation needs exact anchors before code work, but the current pass did not verify them.
- `HISTORICAL_UNVERIFIED`: old, benchmark, attachment, ledger, or owner-doc anchors retained as evidence only; useful for search, not current line/quote proof.

No fake code reference rule: do not invent `file:line` anchors, quotes, method signatures, or "implementation exists" claims from plan prose. A docs-only reference can support planning intent, not source-line proof or implementation proof. `implementation proof pending` remains the default until product code and verification are actually run in a later authorized pass.

| code source path | current Pass 13 status | future inventory action |
|---|---|---|
| `C:\Projects\llm_mvp_core\00.Core\LogicalContracts.cs` | `PATH_EXISTS_ONLY` | Refresh exact lines/quotes after copying the latest updated branch to the `DPOLegalLearning` workdir; otherwise mark needed anchors `NEEDS_CODE_REF_INVENTORY`. |
| `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs` | `PATH_EXISTS_ONLY` | Refresh exact lines/quotes after copying the latest updated branch to the `DPOLegalLearning` workdir; otherwise mark needed anchors `NEEDS_CODE_REF_INVENTORY`. |
| `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs` | `PATH_EXISTS_ONLY` | Refresh exact lines/quotes after copying the latest updated branch to the `DPOLegalLearning` workdir; otherwise mark needed anchors `NEEDS_CODE_REF_INVENTORY`. |

Older active/Claude rows that already carry `file:line` references are owner-doc or historical evidence unless a current inventory row explicitly classifies the exact anchor as `LINE_REF_VERIFIED` or `QUOTE_VERIFIED`. This pass performed bounded path existence checks only for the three paths above and did not reopen source files at their lines.

## Pass 14 Repo/Branch/Workdir Authority Gate

historical_pre_post_repair_verdict_at_that_pass: `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review: `PLAN_READY_FOR_LUBA_REVIEW`; historical_pre_hubble_plan_readiness_after_latest_plan_partial_reconciliation: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_0aa_plan_readiness: `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation_authorization: `NO until fresh Luba approval after review`
pass_scope: `R-OT-14 implementation repo authority, branch source, workdir, and mergeability boundary`
subagent_id_or_NONE: `019f1d2e-b883-7de2-a85a-0f36961a0061`

This Pass 14 gate is plan/governance evidence only. It does not implement product code, create workdirs, run git, prove branch freshness, prove mergeability, create PRs, merge, push, run tests/builds/runtime probes, call providers/APIs/network, or authorize implementation.

For every future implementation repo, authority is the selected latest updated branch or most advanced branch in that same repo as identified by the plan/repo inventory at implementation start. A copied directory, discovered workdir name, old branch memory, `planning-dpo-learning`, or `C:\Users\lubao\Documents\xiigen` is not implementation branch authority.

The implementation working name is exactly `DPOLegalLearning`. Future implementation, after plan readiness and fresh Luba approval only, must copy or clone the selected source authority branch into a separate directory under `C:\Projects` using the active plan working-branch/workdir convention, normally `C:\Projects\DPOLegalLearning\<repo-name>`. (superseded — see Current Implementation Branch/Workdir Convention Overlay 2026-07-09)

Before every future commit and before every future push, the executor must refresh/check the original source authority branch and run a no-write mergeability/no-conflict check back into that original branch, and into `main` where project rules require main-branch mergeability. The result must be recorded as execution evidence in that future pass. PR creation and merge are forbidden by the current plan unless Luba later explicitly asks for them.

Full access means no permission prompts for authorized plan/governance repair. It does not authorize product implementation before plan readiness and Luba's later implementation approval.

## Sequential Pass 05 R-OT-05 Reference Propagation Contract

historical_pre_post_repair_verdict_at_that_pass: `PLAN_NOT_READY`; historical_pre_43c2_plan_readiness_after_post_repair_review: `PLAN_READY_FOR_LUBA_REVIEW`; historical_pre_hubble_plan_readiness_after_latest_plan_partial_reconciliation: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical_pre_0aa_plan_readiness: `PLAN_READY_FOR_LUBA_REVIEW`; current_plan_readiness: `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation_authorization: `NO until fresh Luba approval after review`
pass_scope: `R-OT-05 source inventory, recon coverage, and reference propagation`
subagent_id_or_NONE: `019f1ca1-c52e-7c03-a3c7-43f70b83aae9`

This Pass 05 contract is a plan/source-map repair only. It does not implement product code, run tests/builds/runtime probes, inspect browser/host routes, call providers, or perform git work.

| item | current authority contract | active propagation target | status |
|---|---|---|---|
| Planning library | All active planning docs for this chain live in `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build`; `claude-*` is read-only Claude baseline authority, `codex-*` is evidence-only/delta input, and `active-codex-execution-*` is the active plan surface. | master plan, library index, this source map, board, matrix, full-task gate, plan-14 | `MAPPED_CURRENT_CONTRACT` |
| Original task attachment | Pass 05 used `C:\Users\lubao\.codex\attachments\e358f905-b68d-4ba5-8b3c-70fc6844ce2b\pasted-text.txt`. Stored/output text is mojibake in the terminal, so this map uses the inspectable path literals and clear English/path fragments only. | source inventory and Pass 05 ledger | `MAPPED_ATTACHMENT_USED` |
| Source roots | Original task requires library/reference work over `C:\Users\lubao\Documents\xiigen\llm mvp` and `C:\Projects\llm_mvp_core`; both paths existed in the Pass 05 inventory check. | source map and later implementation work orders | `ROOTS_EXISTENCE_CONFIRMED_ONLY` |
| Core project authority | `C:\Projects\llm_mvp_core` / `llm_mvp_core` is the core project/source-root for trainable/DPO/DynamicGraph logic. It must not be replaced by CanvaToCode or a copied planning workdir. | R21.1, board, matrix, plan-14, implementation work orders | `MAPPED_CORE_AUTHORITY` |
| Participating repos | Claude baseline/source-map contract remains exactly: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. `xiigen-platform-dot-net-angular` is the public facade, `xiigen-flows-dot-net-angular` owns FLOW-31/FLOW-38 seams, and FigmaToCode/CanvaToCode are client/caller surfaces only. Short paths `C:\Projects\xiigen-platform` and `C:\Projects\xiigen-flows` are historical/wrong for this task unless a later exact source reintroduces them. | R21.1, R22, R23, board, matrix, plan-14 | `MAPPED_NO_SCOPE_EXPANSION_PATHS_REPAIRED_2026_07_05` |
| Implementation repo authority | Future implementation must copy/use the latest updated branch from each participating repo into a separate `C:\Projects\DPOLegalLearning\<repo>` working directory or `DPOLegalLearning` work branch. The source branch is read-only authority. Before any later commit/push, refresh source branch evidence and run a no-write mergeability/no-conflict check against the original source authority branch, and against `main` where project rules require main-branch mergeability. No PR and no merge without separate future Luba instruction. (superseded — see Current Implementation Branch/Workdir Convention Overlay 2026-07-09) | pre-implementation, pre-commit, pre-push gates | `MAPPED_EXECUTION_GATE_LATER` |
| Recon library floor | Historical/pre-Fable Pass43 downgraded the explicit original-task `>=20`/`>=40` floor to first-substantive-recon sufficiency. Current Fable/Claude evidence rejects that downgrade. Current contract after parent acceptance: the 20-pass `llm mvp` docs library and 40-pass `llm_mvp_core` docs+code library are accepted only as R-B evidence-library closures; they do not close readiness, R-G addressability, source-map consumer sync, tests/builds/runtime/provider/training/checkpoint proof, or implementation authorization. | source map, full-task gate, board, matrix, plan-14, Claude `claude-02`/`claude-03` RECON v1, Fable addendum, 40-pass library | `CURRENT_RB_EVIDENCE_LIBRARY_CLOSED_NOT_READY` |
| Reference propagation | If a question can be answered from Claude baseline, active-codex source files, Codex evidence-only files, or source roots, the active plan must downgrade the question from open to resolved or partial with exact evidence before asking Luba or stopping. Missing reference propagation is a planning blocker, not a future implementation stop permission. | open-question tables, coverage matrix, board, work orders | `MAPPED_GATE` |
| Wrong-repo guard | CanvaToCode and FigmaToCode may be implementation repos only for their client/caller surfaces. They are not platform-host authority, not `llm_mvp_core` authority, and not a replacement for xiigen-platform/xiigen-flows unless a later exact source proves a narrower surface. | R21.1, R22/R23 scenario rows, board, matrix, plan-14 | `MAPPED_GUARD` |

Future implementation agents must begin from this contract before selecting repositories or asking Luba a source question. If any referenced repo/source/guide cannot be located from this map, the result is `PLAN_NOT_READY_REFERENCE_GAP`, not implementation authorization.

## Direct Evidence Read Now

| source_id | path | planning use |
|---|---|---|
| ATT-0CE0 | `C:\Users\lubao\.codex\attachments\0ce0d4e1-64cd-4a35-b8ed-56993844e412\pasted-text.txt` | Historical/current downgrade evidence before Pass06. Attachment 0ce0 reports a stricter full-task simulation: 33 valid sequential passes, FULLY=0, PARTIAL=25, NOT_COVERED=8, ERROR=1, verdict `NOT_READY`, blockers B1-B21 with B5 absent. Evidence-only; not implementation authority and not a pass run by this worker. Pass06 treats B5 absence as `MISSING_NUMBER_NEEDS_TRACE_REPAIR`, not permission to drop B-5. |
| ATT-912 | `C:\Users\lubao\.codex\attachments\912d5c83-1da1-45a7-8221-c01b59d0c6be\pasted-text.txt` | Pass06 evidence input. Decoded text reports a single-agent/no-parallel governance loophole, post-repair still `NOT_READY`, 0 FULLY coverage, and 22 deduplicated blockers. Explicit blocker details include B-1 input X/entrypoint loop, B-2/B-3 provider/mock-only boundary, B-4 FLOW-38 path missing, B-9 output-shape contradiction, B-10 graph-order contract gap, B-13 canonical-contract mismatch, B-18 local-client-model gap, B-21 recon absent, and B-22 Pass01/02 active-vs-baseline lag. Evidence-only; not implementation authority and not a pass run by this worker. |
| ATT-429F | `C:\Users\lubao\.codex\attachments\429f288a-ad3d-4c0f-872e-1228c0199914\pasted-text.txt` | Historical/pre-attachment-0ce0 defect source. Attachment 429f reported the pre-repair plan as `NOT_READY`; it also flagged F0 contradiction, stale open-question propagation, missing Gate B threshold propagation, dual-path gap, review-floor overclaim risk, and plan-only status. Evidence-only; not implementation authority. |
| ATT-03D85215 | `C:\Users\lubao\.codex\attachments\03d85215-af75-4b4c-a2d5-a7e4b4f65063\pasted-text.txt` | Current Pass21 attachment. UTF-8 read says: "ок, исправь пожалуйста в claude-* - через агентов - не больше одного паралельно"; it authorizes plan-only `claude-*` baseline repair, says no git, reports prior active-codex lag, and lists Repair2 plan-level residuals including `Kind` vs `EdgeKind`, `candidate_order_invariant` vs `candidate_order_hash`, and `EdgeAuthorityDecision` field drift. Current pass used it only for bounded plan/governance repair scope; no nested agents, implementation, tests, providers, browser, or git. |
| LUBA-CLAUDE-REPAIR2-QUOTE | Latest in-chat Luba context during Pass21, quoting Claude workflow `claude-lock-defaults-then-coverage-resim-sequential-wf_fd08f65f-4b3` / task `wpvf7hwib` | Evidence says Claude replaced Lock with Repair2, planned 9 sequential `claude-*` plan-level fixes, then Simulate(33) + Synthesize, no git, no parallel. Pass21 safe inspection did not find that workflow script/task output in this planning library, and did not inspect a completed Claude packet. Status: `CLAUDE_REPAIR2_RUNNING_OR_UNVERIFIED`. This is not accepted Claude baseline and cannot close B-22/readiness. After Claude returns inspectable output, active-codex must refresh this source map and reconcile against the fresh Claude baseline before readiness or implementation authorization. |
| PASS22-CLAUDE-ORDERED-PATTERNIDS-LOCAL | Local files `claude-plan-07-dpo-builder.md:228-242` and `claude-plan-04-dynamicgraph-machine.md:547-584`; active counterpart `active-codex-execution-plan-07-dpo-builder.md` | Pass22 found no standalone `wpvf7hwib` workflow packet in the planning library, so full Claude Repair2 completion is still not accepted from workflow evidence. However, exact current Claude plan files are locally inspectable for the `OrderedPatternIds` carrier decision. Classification for this slice only: `CLAUDE_REPAIR2_INSPECTABLE_AND_PARTIALLY_MAPPED`. Active plan-07 was updated with the carrier rule; before Pass23, active plan-04 still had 0 `OrderedPatternIds` hits and remained a downstream review target. Not implementation, not B-22 closure, not readiness. |
| PASS23-PLAN04-ORDERED-PATTERNIDS-RESOLVER | Local files `claude-plan-04-dynamicgraph-machine.md:547-584` and `:943-951`; active counterpart `active-codex-execution-plan-04-dynamicgraph-machine.md` | Pass23 mapped the resolver-side order-channel rule into active plan-04 only. `GraphOrderResolverUnit` / block 04 owns resolver decision/context `OrderedPatternIds`; handoff to block-07 `GraphDpoBuilder` uses existing `input_context`, `chosen`, `rejected`, `path_order_ref`, and refs. `graph_path_ids` remains evidence-only provenance, and `candidate_order_hash` remains permutation proof, not confidence input. This is not whole Claude Repair2 completion, not implementation, not B-7/B-10/B-22 closure, not readiness. |
| PASS06-R-OT-06-DPO-LIFECYCLE | Active files `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`, `active-codex-execution-plan-09-auto-dpo-loop.md`, `active-codex-execution-16-full-task-coverage-simulation-gate.md`, `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md`, `active-codex-execution-EXECUTION-CONTROL-BOARD.md`, and this Pass 06 ledger record | Current sequential Pass 06 maps R-OT-06 only: `low-result`/`weakness` detection through `IGraphWeaknessDetectorUnit`, better target via `self_best` or `legal teacher`, closed proprietary `judge-only` boundary, `GraphDpoPair level=unit`, `TeacherProvenance`/`JudgeProvenance`, held-out + ablation + speed/accuracy/quality measurement, `ShouldStop` with `qualityThreshold=0.95` and `teacherGapEpsilon=0.02`, and mock/unit-test-only external-call future boundary. This is plan-contract repair evidence only, not implementation, tests, training, provider calls, or readiness. |
| PASS07-R-OT-07-ASYNC-CANDIDATE-ORDER-DPO | Active files `active-codex-execution-plan-06-async-combinations.md`, `active-codex-execution-plan-09-auto-dpo-loop.md`, `active-codex-execution-plan-04-dynamicgraph-machine.md`, `active-codex-execution-plan-07-dpo-builder.md`, `active-codex-execution-guide-04-ilearningmachine-pattern-and-graph.md`, master/gate/board/matrix, and this Pass 07 ledger record | Current sequential Pass 07 maps R-OT-07 only: `GraphCandidateRunRequest` -> async route execution -> `AsyncCandidateBatch` -> typed `GraphNodeCandidate`/`CandidateSelectionInput`; `N == combinationCount == request.CombinationCount`, default `4`, ceiling `8`; fan-out/fan-in/many-to-many merge preserves conflicts/provenance/trace and does not write DPO; `GraphOrderResolverUnit` emits `GraphOrderDecision` with `path_order_ref`, `ordered_unit_ids`, `ordered_path_refs`, `skipped_edges`, `rejected_edges`, `trace_ref`; DPO feed remains `GraphDpoPair(level=unit)`. This is plan-contract repair evidence only, not implementation, tests, runtime, training, provider calls, or readiness. |
| ACT-0CE0-GATE | `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-16-full-task-coverage-simulation-gate.md` | Current controlling active-codex gate after Pass06: `PLAN_NOT_READY`, implementation authorization NO, attachment 0ce0/912 coverage evidence, uncovered set, B-1..B-22 repair backlog, and `MISSING_NUMBER_NEEDS_TRACE_REPAIR` handling for missing/ambiguous blocker numbering. |
| ACT-REPAIR-20260629 | `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-plan-readiness-repair-20260629.md` | Historical/pre-attachment-0ce0 narrower repair packet for `План не готов к выполнению - нужно починить`: CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY rows, attachment 429f source map, resolved/partial decisions, and implementation authorization NO. |
| P-001 | `C:\Projects\llm_mvp_core\docs\plans\claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` | Existing plan evidence, source map, future phase framing |
| C-001 | `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs` | `LearningBatch`, `LearningTrainingItem`, `LearningEvaluationSet`, `LearningBatchResult`, `ILearningMachine`, `ILearningMachinePair` |
| C-002 | `C:\Projects\llm_mvp_core\00.Core\LogicalContracts.cs` | `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, transition signals, context packets |
| C-003 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs` | Adaptive routing, branch execution, transition follow-up, capability trainer boundary |
| C-004 | `C:\Projects\llm_mvp_core\17.DynamicDomains\DynamicDomainGraph.cs` | Graph closure, bridge subphase expansion |
| C-005 | `C:\Projects\llm_mvp_core\22.AgentAlgorithms\MultiAgent\DependencyGraph.cs` | Topological levels, sequential dependency detection, parallel level execution model |
| C-006 | `C:\Projects\llm_mvp_core\11.DPO\Models\DpoPair.cs` | Minimal DPO pair shape: prompt, chosen, rejected |
| C-007 | `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs` | DPO loss, reward margin, accuracy, metrics persistence |
| C-008 | `C:\Projects\llm_mvp_core\26.TrainableRouter\Training\RoutingDpoPairBuilder.cs` | Routing DPO pair precedent |
| C-009 | `C:\Projects\llm_mvp_core\26.TrainableRouter\Training\RoutingTrainingRunner.cs` | Train and evaluate runner precedent |
| C-010 | `C:\Projects\llm_mvp_core\27.TranslationDomain\TranslationMachine.cs` | `ILearningMachine` lifecycle precedent with training history, evaluation, export, import, checkpoint boundary |
| C-011 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\UniversalNineNodeDevelopmentFlow.cs` | `ILogicalFlow` and `ILogicalUnit` execution sequence with transition signals |

## Repository Reference Categories

| category | current status | future collection work |
|---|---|---|
| Core contracts | Targeted paths read now | 40-pass core docs+code library accepted as evidence-only; exact consumer source handles still need plan-family use |
| Logical orchestration | Main orchestrator and nine-node flow read now | More flow variants and tests remain future |
| Dynamic graph helpers | `DynamicDomainGraph` read now | Graph engine, PageRank, serialization, project wiki graph domain remain future |
| DPO components | DPO pair and trainer read now | All DPO generators, runners, metrics, and source registry tests remain future |
| Translation machine precedent | One machine implementation read now | Other `ILearningMachine` implementations remain future |
| Planning and review skills | Existing plan references recorded | Skill and guide audit remains future unless reauthorized |

## Xiigen Documentation Roots

| root | current status | planning relevance |
|---|---|---|
| `C:\Users\lubao\Documents\xiigen\llm mvp` | Target folder lives here | Library storage and local xiigen planning context |
| `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` | Populated now by this work | Current active library |
| `C:\Users\lubao\Documents\xiigen\llm mvp\extensions` | Evidence root only if later read | Product boundary, local/offline policy, extension targets |
| `C:\Users\lubao\Documents\xiigen\llm mvp\translation_guidance_fixed` | Evidence root only if later read | Translation DPO provenance and source policy precedent |
| `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2` | Evidence root only if later read | External source registry and legal evidence precedent |
| `C:\Users\lubao\Documents\xiigen\llm mvp\Small model racional` | Path exists; library pass rows present: Pass 01, Pass 13 (20-pass library). Prior "library pass row still required" is historical/pre-closure. | Small-model / learning-machine background reference |
| `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning` | Path exists; library pass rows present: Pass 02, Pass 03, Pass 10 (20-pass library). Prior "library pass row still required" is historical/pre-closure. | Active v1 language-translate pilot background |
| `C:\Users\lubao\Documents\xiigen\llm mvp\local wiki learning` | Path exists; navigation/library rows present: Pass 04, Pass 11 (20-pass library). Prior "navigation/library row still required" is historical/pre-closure. | Deferred/reference wiki domain |
| `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning` | Path exists; library pass rows present: Pass 05, Pass 12 (20-pass library). Prior "library pass row still required" is historical/pre-closure. | Documentation-learning domain |
| `C:\Users\lubao\Documents\xiigen\Planning sessions\session-start-extracted` | Path exists; not under `llm mvp`; navigation/library rows present: Pass 07, Pass 15 (20-pass library, MVP-SESSION). Prior "navigation/library row still required" is historical/pre-closure. | Original session-start material from task attachment |

## Legal And Provider Evidence

| item | current status | required future gate |
|---|---|---|
| Provider terms for closed APIs | not collected now | terms_snapshot_gate before any training or judging use |
| Model licenses for open-weight candidates | not collected now | license_snapshot_gate before candidate acceptance |
| Hosted API output-use rights | not collected now | closed_api_distillation_block unless explicit terms allow |
| Data retention and privacy terms | not collected now | privacy_retention_gate before hosted use |
| Geography and export constraints | not collected now | legal_review_gate before provider activation |

## Future Collection Phases

| phase | status | output expected |
|---|---|---|
| 20-pass `llm mvp` background/docs knowledge library | required_not_done | Exactly 20 focused-pass records over `C:\Users\lubao\Documents\xiigen\llm mvp` and the original-task reference roots; target artifact `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` |
| 40-pass `llm_mvp_core` docs+code knowledge library | accepted_evidence_only_not_readiness | Exactly 40 focused-pass records over `C:\Projects\llm_mvp_core` docs+code accepted by parent as R-B evidence-library closure; target artifact `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md`; remaining consumer sync and review do not authorize implementation |
| Provider terms verification | future_not_done | Terms snapshot refs and allowed-use matrix |
| Model candidate verification | future_not_done | Accepted, rejected, or conditional candidate table |
| Implementation source audit | future_not_done | Exact class and interface change plan after Luba approval |

## Current Task / Not The Task / Evidence-Only Source Map - 2026-06-29

| bucket | source | row | plan effect |
|---|---|---|---|
| CURRENT TASK | Luba latest unquoted instruction | `План не готов к выполнению - нужно починить` | Repair active plan; do not authorize implementation. |
| CURRENT TASK | Current bounded work order | Target only `active-codex-execution-*` files in this folder | Allowed edits are restricted to this active artifact family. |
| CURRENT TASK | Attachment 429f | Defect inventory in human-readable Russian | Extract every concrete readiness defect and patch the active plan accordingly. |
| NOT THE TASK | Current bounded work order | No product implementation, no tests/builds, no git, no commit/push/PR/merge | These actions remain forbidden in this task and cannot be implied by old active wording. |
| NOT THE TASK | Current bounded work order | Do not edit `claude-*`, `codex-*`, `dcm-*`, source plans, or repo code | Those files are read/evidence only. |
| EVIDENCE-ONLY | Attachment 429f previous assistant/status text | Review output and critic notes | Evidence to diagnose contradictions; not proof of real cycles or readiness. |
| EVIDENCE-ONLY | Previous sub-agent packets and old summaries | Prior context | Cannot create current task, implementation approval, or git scope. |
| EVIDENCE-ONLY | `codex-*` artifacts | Delta/source-map evidence | Cannot override Claude baseline or expand repo scope. |
---

## PASS17 B-22 Active-vs-Claude Reconciliation Map

Historical/pre-Pass17/50 PASS17 inventory command output counted 31 `claude-*.md` planning-library files and 41 `active-codex-execution-*.md` active files. Current Pass17/50 refresh counted 31 `claude-*` files, 50 `active-codex-execution-*` files, 0 missing active counterparts for Claude top-level files, and 19 active-only delta/control files. These counts are only file inventory. They do not prove 31 matched categories, 50 reviews, B-22 closure, or readiness. Claude baseline remains authority; Codex material and extra active files are deltas/source-map inputs only.

### PASS17 Inventory Names By Group

| inventory group | count | names or concise name range |
|---|---:|---|
| Claude library 00-07 | 8 | `claude-00-library-index.md`; `claude-01-authority-requirements-ledger.md`; `claude-02-source-inventory-and-reference-map.md`; `claude-03-ilearningmachine-pattern-notes.md`; `claude-04-dpo-legal-model-policy.md`; `claude-05-dpo-data-training-measurement-guide.md`; `claude-06-ilearningdynamicgraph-architecture-brief.md`; `claude-07-execution-gates-open-questions.md` |
| Claude master/board/matrix | 3 | `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`; `claude-EXECUTION-CONTROL-BOARD.md`; `claude-REQUIREMENTS-COVERAGE-MATRIX.md` |
| Claude guides | 5 | `claude-guide-01-how-to-build-dpo.md` through `claude-guide-05-auto-dpo-loop.md` |
| Claude plan blocks | 15 | `claude-plan-00-partA-uml-status.md` through `claude-plan-14-stage-references.md` |
| Active baseline counterparts | 31 | Matching `active-codex-execution-00-*` through `active-codex-execution-07-*`; active master/board/matrix; active guides 01-05; active plan blocks 00-14 |
| Active extra delta/control files | 19 | `active-codex-execution-16-full-task-coverage-simulation-gate.md`; `active-codex-execution-17-50-pass-readiness-repair-ledger.md`; `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md`; `active-codex-execution-19-30-pass-sequential-original-task-coverage-ledger.md`; `active-codex-execution-31-readiness-reconciliation-parent-acceptance.md`; `active-codex-execution-32-b16-parent-acceptance.md`; `active-codex-execution-33-b19-parent-acceptance.md`; `active-codex-execution-34-b10-parent-acceptance.md`; `active-codex-execution-35-seed-corpus-parent-acceptance.md`; `active-codex-execution-37-pass37-claude-baseline-sync.md`; `active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`; `active-codex-execution-50-pass-authority-ledger-20260629.md`; `active-codex-execution-F0-decision-unblocker.md`; `active-codex-execution-NO-SAFE-WORK-EVIDENCE-20260629.md`; `active-codex-execution-opus-f0-reconciliation-20260629.md`; `active-codex-execution-original-task-authority-audit-20260629.md`; `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md`; `active-codex-execution-plan-readiness-repair-20260629.md`; `active-codex-execution-XIIGEN-PLANNING-REPO-NO-BRANCH-POLICY.md` |
| Codex recon evidence | 63 | `codex-recon-*.md` files; evidence-only inventory family, not active baseline authority |
| Codex knowledge-library evidence | 2 | `codex-knowledge-library-llm-mvp.md`; `codex-knowledge-library-llm-mvp-core.md` |
| Active execution recon counterparts | 0 | no `active-codex-execution-recon-*.md` files found |
| Claude recon namespace | 0 | no `claude-recon-*.md` files found |

Pass18/50 current baseline SR-id boundary: latest inspected Claude plan-14 assigns `SR-17` to the `language-translate` seed input-prompt corpus, and active-codex searched consumers now use current `SR-17` only for that seed-corpus meaning. Older active LogicalContracts/source-inventory anchors formerly labelled `SR-17` are preserved as `SR-17-legacy-active-logicalcontracts` (historical/local alias; not current `SR-17`). The Pass17 `ACTIVE_LAGS_CLAUDE_FOR_SR17_ID_SPLIT` marker is historical for this repaired slice after recheck, but the 64 Source-to-Plan rows remain inventory/source-map evidence only and cannot be used as B-22 closure, readiness evidence, seed-file proof, or implementation authorization.

### PASS17 Category Coverage

| major category | Claude files/categories inventoried | active counterpart inventoried | classification | concrete repair or queue |
|---|---|---|---|---|
| master plan | `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | `ACTIVE_HAS_DELTA` | Active master contains appended repair addenda and should be reconciled line/section-wise against Claude §E/Q decisions before any ready claim. |
| execution-control board | `claude-EXECUTION-CONTROL-BOARD.md` | `active-codex-execution-EXECUTION-CONTROL-BOARD.md` | `ACTIVE_HAS_DELTA` | Active board carries Pass02/0ce0/R21/R22/R23 deltas; it may refine but not replace Claude phase ordering and branch authority. |
| requirements coverage matrix | `claude-REQUIREMENTS-COVERAGE-MATRIX.md` | `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md` | `ACTIVE_HAS_DELTA` | Active matrix adds R21/R22/R23 and pass repairs; R1-R20 remain Claude baseline unless an accepted delta says otherwise. |
| plan blocks 00-14 | `claude-plan-00-partA-uml-status.md` through `claude-plan-14-stage-references.md` | matching `active-codex-execution-plan-00-partA-uml-status.md` through `active-codex-execution-plan-14-stage-references.md` | `ACTIVE_HAS_DELTA` | All named counterparts exist; PASS17 did not perform detailed block-by-block semantic diff, so B-22 queues a future pass over 00-14. |
| guide set 01-05 | `claude-guide-01-how-to-build-dpo.md` through `claude-guide-05-auto-dpo-loop.md` | matching `active-codex-execution-guide-01-how-to-build-dpo.md` through `active-codex-execution-guide-05-auto-dpo-loop.md` | `ACTIVE_HAS_DELTA` | PASS18 reconciled guides 03-05 at semantic-section level below; guides 01-02 and non-guide categories still need direct B-22 review. |
| library/control files 00-07 | `claude-00-library-index.md` through `claude-07-execution-gates-open-questions.md` | matching `active-codex-execution-00-library-index.md` through `active-codex-execution-07-execution-gates-open-questions.md` | `ACTIVE_HAS_DELTA` | 00 and 02 now carry PASS17 authority text; 01/03/04/05/06/07 still need category readback. |
| recon/knowledge library | Claude matrix references recon-library gate; no `claude-recon-*.md` files found | 63 `codex-recon-*.md`, 2 `codex-knowledge-library-*.md`, 0 `active-codex-execution-recon-*.md` found | `NO_ACTIVE_COUNTERPART_FOUND` | Codex recon/knowledge files are evidence-only; B-21/B-22 must map exact accepted rows before any recon-library coverage claim. |
| branch/repo authority | Claude board lines 20-24 and active board/source map R21 name read-only latest-updated authority and `DPOLegalLearning` work name | active source map R21.1 and active board already carry the five-repo rule | `ACTIVE_HAS_DELTA` | Keep the five Claude-baseline repos only: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`; planning repo and copied folders are not authority. |
| legal/provider/provenance | Claude plan/matrix/board and `claude-plan-08` define legal teacher/judge and 20-field provenance rules | active legal policy, active plan-08, PASS12/PASS16 source-map rows | `ACTIVE_HAS_DELTA` | Active deltas preserve quarantine/personal/adaptive boundary; future reconciliation must verify provider defaults and sidecar paths against Claude before closure. |
| DPO unit-level pair model | Claude matrix rows for `GraphDpoPair`/`TeacherProvenance`; active PASS04/PASS12 rows | active master, plan-07, plan-08, plan-09, matrix | `ACTIVE_HAS_DELTA` | Current active contract is unit-level only; edge/graph wording is evidence-only unless Luba approves. Future pass must prove no Claude train-ready field was dropped. |
| host-route/Q-host | Claude master Q-host/B-19 and plan-14; active PASS14/PASS15/PASS16 | active source map R21/R22/R23, plan-14, gate sections | `ACTIVE_HAS_DELTA` | Host route is specified but physical path remains unproven; B-19 and B-4/R17 cannot close from plan text. |
| B-1..B-22 blocker inventory | attachment 0ce0/912 evidence plus active gate backlog | `active-codex-execution-16-full-task-coverage-simulation-gate.md` and this ledger | `ACTIVE_HAS_DELTA` | B-22 is now explicit; all B-1..B-22 remain a repair queue, not readiness proof. |

### PASS17 B-22 Readiness Rule

Active-codex is not ready until every row above is either:

1. `ACTIVE_MATCHES_CLAUDE_BASELINE` with direct readback evidence for the exact category; or
2. `ACTIVE_HAS_DELTA` with an explicit accepted/Luba-approved delta, exact source references, and proof that no Claude baseline detail/reference/explanation was lost.

`ACTIVE_LAGS_CLAUDE` and `NO_ACTIVE_COUNTERPART_FOUND` rows are blockers. File count, newer ledger text, status tables, and PASS rows cannot close B-22.

### Pass 17 Source-to-Plan 64/B-22 Reconciliation Contract

Current count boundary: `64/64 not claimed by Pass 17`. Pass 17 did not enumerate all 64 Source-to-Plan rows and did not close B-22. It creates the active reconciliation contract that future reviewers can continue without asking Luba or dropping rows.

Status vocabulary for target-64 and B-22 rows:

| status | acceptance meaning | closure rule |
|---|---|---|
| `MAPPED_VERIFIED` | One exact baseline substep has an active row with row id, Claude/source owner, active owner, SR/source reference, downstream consumers, and readback evidence. | Counts toward a future 64/64 claim only as one row. |
| `MAPPED_ACCEPTED_DELTA` | Active row intentionally differs from Claude because a current Luba-approved or accepted Codex delta is cited, and no Claude detail/reference/explanation is lost. | Counts only for that exact row/category. |
| `PARTIAL_NEEDS_OWNER_REVIEW` | A row group, category, or support anchor exists, but per-row ownership, downstream consumers, or direct evidence is incomplete. | Cannot count as complete. Route to the named owner. |
| `MISSING_ACTIVE_COUNTERPART` | Claude/latest baseline or board baseline row exists, but active-codex has no concrete Source-to-Plan row yet. | Blocks 64/64 and B-22 closure. |
| `STALE_HISTORICAL_ONLY` | Old 12/26/59, 18-field, 8/59, older ready, or older partial-count text remains only as labelled evidence. | Cannot support current readiness. |
| `CLAUDE_REPAIR2_UNVERIFIED` | A Claude-side Repair2 or cleanup is quoted/running/pending or not directly inspected. | Blocks B-22 until exact changed `claude-*` output is inspected. |
| `NEEDS_CODE_REF_INVENTORY` | The plan row needs later approved implementation/source inventory for physical repo/path/branch/file proof. | Does not authorize implementation now. |

Historical/pre-Pass30/pre-Pass39 Pass 17 row-category ledger:

| category | owner | current status | Pass 17 count boundary | next repair routing |
|---|---|---|---|---|
| Source-to-Plan denominator | `active-codex-execution-plan-14-stage-references.md` block 14 | `PARTIAL_NEEDS_OWNER_REVIEW` | target is 64 baseline substeps; complete 64-row list not verified here | Derive the full row roster from the baseline-64 board/order, then create one row per baseline substep. |
| Existing support anchors | plan-14 section 3.2 | `PARTIAL_NEEDS_OWNER_REVIEW` | active plan-14 directly lists 8 support row groups; Pass 17 does not convert groups into 64 verified rows | Split support groups into exact row ids only when each row has owner, source, consumers, and readback. |
| Active pilot `language-translate` rows | plan-14 plus Claude board/matrix/plan-14 | `PARTIAL_NEEDS_OWNER_REVIEW` | F10.3.1 through F10.3.5 are in baseline-64 but are not accepted as five complete Source-to-Plan rows by Pass 17 | Owner review must add or cite the five individual pilot rows before 64/64. |
| Non-F10 baseline rows | execution board plus plan-14 owner map | `MISSING_ACTIVE_COUNTERPART` until enumerated | no complete per-row active map enumerated by Pass 17 | Continue row-range fill by phase/subphase; unresolved rows stay blockers. |
| Recon/knowledge-library support | source map plus Codex recon files | `NO_ACTIVE_COUNTERPART_FOUND` and `NEEDS_CODE_REF_INVENTORY` | Codex recon counts are evidence-only and cannot count as accepted active rows without exact mapping | Map exact accepted recon rows into B-21/B-22 or keep evidence-only. |
| Historical counter/status text | active plan surfaces with older counts/status | `STALE_HISTORICAL_ONLY` | older text supports drift diagnosis only | Keep current verdict nearby; do not use old text for readiness. |
| Claude Repair2/latest-baseline deltas | source map rows `LUBA-CLAUDE-REPAIR2-QUOTE`, `PASS22`, `PASS23`, and later inspectable Claude outputs | `CLAUDE_REPAIR2_UNVERIFIED` unless exact local output was inspected for that slice | no whole-Repair2 acceptance by Pass 17 | Inspect exact changed `claude-*` files and reconcile active copies before B-22 closure. |

Historical/pre-Pass39 Pass 17 status: B-22 remained open at that time. Current B-22 classification is owned by the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`: `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`, with no readiness, implementation authorization, or execution proof claimed.

### PASS18 Guide 03-05 Section-Level Reconciliation

PASS18 checked the Claude guide 03-05 files against their active-codex counterparts. Claude baseline remains authority; Codex delta text is accepted only when it preserves the Claude requirement/decision/gate/prohibition and does not replace it by freshness or file length.

| guide | Claude source | active source | PASS18 classification | concrete copied or queued detail |
|---|---|---|---|---|
| guide 03 measurement | `claude-guide-03-how-to-measure-quality.md` | `active-codex-execution-guide-03-how-to-measure-quality.md` | `ACTIVE_HAS_ACCEPTED_DELTA` | PASS18 correction repaired the earlier internal-consistency overclaim: all active guide 03 owner references now use `GraphDpoPair` -> block 07 (`claude-plan-07`) and `TeacherProvenance` -> block 08 (`claude-plan-08` §3.1/legal-gate). Active addendum remains a Codex delta/source-map layer, not baseline authority. |
| guide 04 ILearningMachine pattern and graph | `claude-guide-04-ilearningmachine-pattern-and-graph.md` | `active-codex-execution-guide-04-ilearningmachine-pattern-and-graph.md` | `ACTIVE_HAS_ACCEPTED_DELTA_WITH_PASS37_REPAIR` | Pass37 repaired active guide 04 / owner consumers: current `GraphDpoPair` is the active/Claude plan-07 14-field canon and remains unit-level for train-ready rows; `TeacherProvenance` is the active/Claude plan-08 20-field canon; `candidate_order_hash` is raw comparison/permutation-proof metadata rather than a separate `GraphDpoPair` field. |
| guide 05 auto-DPO loop | `claude-guide-05-auto-dpo-loop.md` | `active-codex-execution-guide-05-auto-dpo-loop.md` | `ACTIVE_HAS_ACCEPTED_DELTA` | PASS18 correction repaired active guide 05 owner contradictions. Later PASS09/PASS24 overlays supersede any historical `RunCandidatesAsync(string input, int combinationCount)` shorthand with the current request/batch contract: `IGraphCandidateRunner.RunCandidatesAsync(GraphCandidateRunRequest request, CancellationToken)` -> `AsyncCandidateBatch`; `N==request.CombinationCount`, default `N=4`, ceiling `8`. Active addendum remains subordinate Codex delta/source-map evidence. |

PASS18 does not close B-22. The next B-22 repair target is plan-block reconciliation, especially `active-codex-execution-plan-08-legal-teacher-gate.md` and `active-codex-execution-plan-14-stage-references.md`, plus guide 01-02 if a later pass needs full guide-set closure.

### PASS24 Locked N / CombinationCount Reconciliation

PASS24 checked the Claude Q16/block-06 and block-09 evidence for the async candidate cardinality decision and
propagated it into active plan artifacts. Classification: `ACTIVE_HAS_ACCEPTED_DELTA_FOR_Q16_N_LOCKED_DECISION`.
This does not close B-7, B-11, B-22, readiness, implementation, tests, runtime proof, browser/provider calls, or git.
Correction after parent readback: the first Pass24 verification overclaimed that exact stale unlocked wording was
gone; `active-codex-execution-plan-06-async-combinations.md` still had stale unlocked cap wording in the
simulation table. This correction changes that residual to `LOCKED_BY_LUBA_2026_06_30` without creating a new pass.

| item | Claude baseline detail preserved in active | PASS24 classification |
|---|---|---|
| Q16 / async N | Claude master and block 06 record `RESOLVED(Luba 2026-06-30)`: `N == combinationCount == request.CombinationCount`, default `4`, ceiling `8`. | `LOCKED_BY_LUBA_2026_06_30` |
| Cap source | Ceiling `8` comes from `MaxAdaptiveBranchCount=8` / `AdaptiveLogicalOrchestrator.cs:19`; budget ceiling above `8` requires later explicit Luba override. | preserved |
| Owner/consumer boundary | Block 06 owns the candidate runner and budget validation; block 09 consumes the block-06 result and must not redeclare a no-N runner. | preserved |
| Boundary not closed | Legal/provenance can lower actual candidates; requested vs actual must be recorded. N is async cardinality only, not `qualityThreshold`, Q17, Q22, Q24, readiness, or blocker closure. | downstream proof still open |

### PASS25 RR6 Legal/Provenance Owner-Canon Reconciliation

PASS25 checked the local Claude Repair2/RR6 evidence in `claude-plan-08-legal-teacher-gate.md` and `claude-04-dpo-legal-model-policy.md` for the legal/provenance owner-canon update and propagated the active-codex consumer wording. Classification: `PASS25_RR6_OWNER_CANON_RECONCILED_NEEDS_DOWNSTREAM_PROOF`.
This does not close B-13, B-17, B-22, readiness, implementation, tests, runtime proof, browser/provider calls, provider terms review, or git.

| item | Claude RR6 detail preserved in active | PASS25 classification |
|---|---|---|
| `TeacherProvenance` owner canon | Block 08 / active plan-08 is the owner; `TeacherProvenance` is 20 fields: 13 base + 5 NEW + 2 RR6 (`service_mode`, `grounding_used`). `provenance_field_presence_rate==1.0` means 20/20. | repaired in active owner/consumers; downstream proof open |
| C17 service-mode/grounding/search ban | `used_for_training && (service_mode != "plain" || grounding_used==true) => BLOCK`, reason `service_mode_grounding_for_training`; Gemini Grounding / Google Search / web retrieval are blocked for training even when the model family is allowlisted. | repaired in plan/policy/board/matrix/gate wording |
| Provider terms snapshot behavior | Model license is not provider terms approval. Hosted Qwen3/Together remains `ProviderTermsChecked=false` with `TermsSnapshotRef=PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)`; `SelectProvider` must not choose it for train/common use while pending. | repaired as pending plan lock, not resolved terms proof |
| C11 closed judge | Closed judge-only accept requires provider terms checked true, non-pending/non-UNKNOWN terms snapshot, `service_mode=="plain"`, and `grounding_used=false`; missing terms is C11-blocked QUARANTINE and grounding/search is C17 BLOCK. | repaired in active scenario wording |
| Historical 18-field evidence | Old Pass12/Pass19/18-field text is evidence-only when explicitly labelled historical/pre-Pass25; unqualified current `TeacherProvenance (18 fields)` is not allowed. | governance/source-map rule added; historical/pre-Pass39 B-22 remained open at that time |

### PASS19 Plan-08 Legal/Provenance Reconciliation

PASS19 compared `claude-plan-08-legal-teacher-gate.md` with `active-codex-execution-plan-08-legal-teacher-gate.md` by semantic section for decisions, gates, prohibitions, and field contracts. Classification: `ACTIVE_HAS_ACCEPTED_DELTA` for the plan-08 legal/provenance slice only. This does not close B-22, does not close B-1..B-21, and does not authorize implementation.

| plan-08 item | Claude baseline detail preserved in active | PASS19 classification |
|---|---|---|
| `TeacherProvenance` ownership | Active plan-08 keeps block-08 as owner of the 20-field `TeacherProvenance` (13+5+2 RR6 `service_mode`/`grounding_used`) record and the legal verdict gate. | `ACTIVE_HAS_ACCEPTED_DELTA` |
| Verdict outcomes and legal scenarios | Active preserves exactly six `LegalVerdict` values (`ACCEPT`, `QUARANTINE`, `BLOCK`, `DATASET_REBUILD_REQUIRED`, `BLOCK_FOR_COMMON`, `EVAL_ONLY`), eight `SourceCategory` values, and the exact 19-row plan-08 §4.1 owner-list: `C2, C3, C4, C5, C6, C7, C8, C9, C10, C11-blocked, C11, C12, C13, C14, C15, C15b, C16, C17, C18`. W-159 axis contract: Qwen teacher=`source_category=open_weight`; scorer-sidecar=`scorer_category=deterministic_baseline` + `scorer_id`/`scorer_version`/`threshold_set_id` (alias plan-10 `threshold_set`). C15/C15b precede C18; complete registered scorer only continues eval and never auto-accepts or becomes train-ready; missing/unregistered scorer → C18 `QUARANTINE(scorer_ref_missing)`/Candidate false/no projection; disguised teacher self-score → C14. The single registry owner is plan-08 §3.5; this map only indexes it. `first_party_deterministic` remains only `chosen_origin`. | `ACTIVE_HAS_ACCEPTED_DELTA` |
| Closed proprietary teacher/judge policy | Active preserves that closed proprietary providers may judge/evaluate only when not used for training; a closed-provider chosen/rejected/train-target attempt is `DATASET_REBUILD_REQUIRED`, not mere quarantine. | `ACTIVE_HAS_ACCEPTED_DELTA` |
| Q37/Q38 client and third-party output policy | Active preserves default `PersonalAdaptive`/`QUARANTINE` until positive license, provenance, redaction, and per-client terms proof exists; common/global promotion is blocked without proof. | `ACTIVE_HAS_ACCEPTED_DELTA` |
| Sidecar/manifest/provenance path | Active PASS12 storage/readback delta preserves the Claude contract by keeping heavy `TeacherProvenance` in sidecar/manifest JSONL, linking refs from `GraphDpoPair`/batch ids, and requiring row-ref/readback parity before train-ready/common promotion. | accepted delta; still execution-evidence pending |
| `GraphDpoPair` / `TeacherProvenance` owner split | PASS19 repaired the remaining ambiguous owner wording: `GraphDpoPair` shape/builder remain block-07 owned; plan-08 owns `TeacherProvenance`, legal gate/verdict, sidecar/provenance proof, and the decision whether a pair/source is train-ready. | repaired in active plan-08; not B-22 closure |

PASS19 does not claim B-22 closure. Remaining B-22 work must still reconcile other plan blocks, especially `active-codex-execution-plan-14-stage-references.md`, plus guide 01-02 and downstream master/board/matrix references if later passes find drift.

### Ordered B-22 Work Queue After PASS17

1. PASS18 completed guide files 03-05 as `ACTIVE_HAS_ACCEPTED_DELTA`; do not treat this as guide-set or B-22 closure.
2. PASS19 completed plan-08 legal/provenance as `ACTIVE_HAS_ACCEPTED_DELTA`; do not treat this as plan-block-set or B-22 closure.
3. PASS20 completed plan-14 route/repo-authority as `ACTIVE_HAS_ACCEPTED_DELTA_FOR_PLAN14_ROUTE_REPO_AUTHORITY`; do not treat this as remaining plan-block closure.
4. PASS22 partially mapped the local Claude Repair2 `OrderedPatternIds` carrier decision into active plan-07 only; PASS23 then mapped the resolver-side `GraphOrderResolverUnit`/RR2 order-channel wording into active plan-04 only.
5. PASS25 reconciled the RR6 legal/provenance owner-canon drift from 18-field stale consumers to the 20-field active owner plus C17/service-mode/grounding/pending-terms behavior. This does not close B-13/B-17/B-22.
6. Reconcile remaining plan blocks 00-14 block-by-block; next non-conflicting target is guide 01-02, downstream master/board/matrix references, or another still-unmapped plan block chosen by the next bounded pass.
7. Reconcile master/board/matrix together for R1-R20 vs active R21/R22/R23 deltas.
8. Map Codex recon/knowledge rows into B-21/B-22 only where exact accepted source rows exist; otherwise keep them evidence-only.
9. Only after the map has no `ACTIVE_LAGS_CLAUDE` or `NO_ACTIVE_COUNTERPART_FOUND` blockers may a later pass consider a B-22 closure review.

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
4. The repo-by-repo authority and source-to-plan map below is the in-plan design decision. During future implementation, record the concrete branch-list source, selected latest-updated branch, selected branch timestamp/ref evidence, divergence note, separate `C:\Projects` working directory, and work name `DPOLegalLearning`.
5. Participating Claude-baseline implementation repos are exactly five: `llm_mvp_core` (core implementation), `xiigen-platform-dot-net-angular` (public API facade for R16/R17), `xiigen-flows-dot-net-angular` (FLOW-31/FLOW-38 internal seam), `FigmaToCode-design-gen` (R16 html/css client/caller), and `CanvaToCode` (R16 html/css client/caller). Codex may be used only as matching delta/source-map evidence. This list must not be expanded from discovered/copied workdirs unless Luba explicitly expands repo scope.
6. Client/caller rows in R21.1 are implementation repos only for their client/caller surfaces and route participants for provenance and future explicitly scoped client-host proof; they do not prove platform-host implementation ownership. PASS20 preserves both facts: exactly five implementation repos and no FigmaToCode/CanvaToCode platform-host authority.
7. Do not switch, create, delete, rebase, merge, cherry-pick, push, or open PRs in `C:\Users\lubao\Documents\xiigen`. That planning location may be read only as an evidence source when needed; no git realignment or branch cleanup is part of this chain.

### R21.1 Concrete Source-To-Plan Map

| repo_id | canonical_repo_name | role_from_claude_baseline | source_authority_branch_rule | working_directory_under_C_projects | working_branch_name_or_workdir_name | commit_push_rule | mergeability_target | PR_merge_policy | source_to_plan_requirements | active_plan_sections | status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| IMPL-CORE | `llm_mvp_core` | Core trainable/DPO/DynamicGraph logic; common leg; `ILearningMachine`/`ILearningMachinePair` contracts; provenance/legal gates; Pass20 SR-16 in-repo R16 design-conversion backing. | Source authority is the most advanced/latest-updated branch in this repo at implementation start, proven by read-only branch/ref timestamp evidence; authority branch is read-only and receives no direct commits from this plan. | `C:\Projects\DPOLegalLearning\llm_mvp_core` | `DPOLegalLearning` workdir/branch created or copied from selected authority only after fresh implementation approval. | No commit/push in this planning repair. Future commit/push only after source refresh and mergeability/no-conflict evidence against the selected authority. | Selected read-only latest-updated source authority branch for `llm_mvp_core`, refreshed immediately before commit/push. | No PR and no merge unless Luba gives separate future instruction. | OTR-01, OTR-03, OTR-04, OTR-06, OTR-07, OTR-08, OTR-12, OTR-13 core design-conversion backing; common/adaptive split; no hidden deterministic final authority. | Master §E.0; plans 01-13; plan-14 SR-00..SR-10/SR-14/SR-16; board Ф1-Ф11; matrix R5/R6/R10/R11/R16/R17/R21. | DESIGN_DECISION_CLOSED_EXECUTION_EVIDENCE_PENDING |
| IMPL-PLATFORM | `xiigen-platform-dot-net-angular` | Public/platform API facade for figma->html+css and better-output->learning-DPO routes; marketplace/client host boundary; no core algorithm ownership. | Same latest-updated read-only authority rule, determined from this repo only; copied/discovered folders cannot add scope. | `C:\Projects\DPOLegalLearning\xiigen-platform-dot-net-angular` | `DPOLegalLearning` workdir/branch created or copied from selected authority only after fresh implementation approval. | Same no-current-commit rule; future commit/push only after source refresh and mergeability/no-conflict evidence. | Selected read-only latest-updated authority branch for `xiigen-platform-dot-net-angular`. | No PR/merge without separate future Luba instruction. | OTR-13 figma API facade; OTR-14 better-output-to-DPO facade; thin host/API boundary. | Master §E.0 Q-host; board Ф10.1.1/Ф10.2.1; plan-14 §6.1; matrix R16/R17/R21. | DESIGN_DECISION_CLOSED_EXECUTION_EVIDENCE_PENDING |
| IMPL-FLOWS | `xiigen-flows-dot-net-angular` | FLOW-31 design-analysis seam and FLOW-38/better-output feedback/DPO seam; connects platform/client routes to `llm_mvp_core` without owning core training logic. | Same latest-updated read-only authority rule, determined from this repo only. | `C:\Projects\DPOLegalLearning\xiigen-flows-dot-net-angular` | `DPOLegalLearning` workdir/branch created or copied from selected authority only after fresh implementation approval. | Same no-current-commit rule; future commit/push only after source refresh and mergeability/no-conflict evidence. | Selected read-only latest-updated authority branch for `xiigen-flows-dot-net-angular`. | No PR/merge without separate future Luba instruction. | OTR-13 FLOW-31 binding map; OTR-14 FLOW-38/better-output DPO contract; blocker-2 exact seam contract is now closed in-plan by R22; implementation proof remains future evidence. | Board Ф9.3.2/Ф10.1.1/Ф10.2.1; plan-14 SR-11/SR-14 and §6.1; matrix R16/R17/R21/R22. | DESIGN_DECISION_CLOSED_EXECUTION_EVIDENCE_PENDING |
| IMPL-FIGMA | `FigmaToCode-design-gen` | Caller/client for figma->html+css route; thin client that must preserve generated-output provenance and must not own core DPO/training logic. | Same latest-updated read-only authority rule, determined from this repo only. | `C:\Projects\DPOLegalLearning\FigmaToCode-design-gen` | `DPOLegalLearning` workdir/branch created or copied from selected authority only after fresh implementation approval. | Same no-current-commit rule; future commit/push only after source refresh and mergeability/no-conflict evidence. | Selected read-only latest-updated authority branch for `FigmaToCode-design-gen`. | No PR/merge without separate future Luba instruction. | OTR-13 caller role for figma->html+css; C16/client-origin provenance quarantine for outputs used in DPO. | Board Ф10.1.2; plan-14 SR-12/§6.1; matrix R16/R21. | DESIGN_DECISION_CLOSED_EXECUTION_EVIDENCE_PENDING |
| IMPL-CANVA | `CanvaToCode` | Caller/client for Canva/design->html+css route named by Claude baseline/original task; thin client only. | Same latest-updated read-only authority rule, determined from this repo only; its inclusion is baseline-driven, not folder-discovery-driven. | `C:\Projects\DPOLegalLearning\CanvaToCode` | `DPOLegalLearning` workdir/branch created or copied from selected authority only after fresh implementation approval. | Same no-current-commit rule; future commit/push only after source refresh and mergeability/no-conflict evidence. | Selected read-only latest-updated authority branch for `CanvaToCode`. | No PR/merge without separate future Luba instruction. | OTR-13 caller role for figma/canva design->html+css; C16/client-origin provenance quarantine for outputs used in DPO. | Board Ф10.1.2; plan-14 SR-13/§6.1; matrix R16/R21. | BASELINE_NAMED_ONLY_EXECUTION_EVIDENCE_PENDING |
| PLAN-XIIGEN | `C:\Users\lubao\Documents\xiigen` | Planning/evidence repository and document root only; not an implementation repo, not a branch target, and not source authority for product work. | No source-authority branch for implementation. `planning-dpo-learning` and any planning-repo branch state are evidence-only/no-branch cleanup context, not implementation authority. | Not applicable for implementation. | No `DPOLegalLearning` implementation branch/workdir is created here. | No git action, commit, push, branch switch, cleanup, PR, or merge in this planning repo unless Luba separately asks for that exact planning-repo git task. | None. | No PR/merge scope in this active chain. | Houses active plan artifacts and original planning docs only; cannot satisfy product repo ownership or branch authority. | All `active-codex-execution-*` planning artifacts; R21 boundary; current worker execution boundary. | NOT_IMPLEMENTATION_REPO_EVIDENCE_ONLY |

R21.1 mergeability target overlay for all `IMPL-*` rows: before every future commit and before every future push, the implementation worker must record no-write mergeability/no-conflict evidence against the selected original source authority branch and against `main` where project rules require main-branch mergeability. This overlay does not create a PR, perform a merge, run git now, or authorize implementation.

### R22 - External API Seam Contracts, Mocks, Unit Tests, And Replacement Ledger

`R22` closes blocker 2 in-plan. It does not authorize implementation, tests/builds, real external calls, network access, provider terms retrieval, or creation of `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`.

| seam_id | source evidence used | owner repos/components | contract location | current status |
|---|---|---|---|---|
| `FIGMA_CANVA_DESIGN_TO_HTMLCSS` | Original task attachments c6a/eb1 name `xiigen-platform` figma API plus FigmaToCode/CanvaToCode callers; attachment 483d and plan-14 record FLOW-31 as design analysis and clients as html/css generators. | `xiigen-platform-dot-net-angular` facade; `xiigen-flows-dot-net-angular` FLOW-31 analysis; `FigmaToCode-design-gen` and `CanvaToCode` callers/client generators; `llm_mvp_core` only for future core orchestration seam if approved. | `active-codex-execution-plan-readiness-repair-20260629.md` Blocker 2 table; test addendum in plan-13. | CLOSED_IN_PLAN_EXECUTION_EVIDENCE_PENDING |
| `BETTER_OUTPUT_FEEDBACK_TO_DPO` | Original task c6a/eb1 better-output learning-DPO route; attachment 483d Q37/Q38 policy; plan-14 R17/FLOW-38 notes; plan-08 C16 policy; PASS15 maps the better-output/five-format alias to B-4/R17, not B-16; PASS16 specifies exact mock input/response/normalized output/unit-test/replacement path. | `xiigen-platform-dot-net-angular` feedback facade; `xiigen-flows-dot-net-angular` FLOW-38 event/resource seam; `llm_mvp_core` legal gate and GraphDPO builder; clients/Smart* as callers. Adapter contract: `GeneratedUiOutputFeedbackEvent` -> `IFlow38FeedbackResourceClient` / `IBetterOutputDpoIngestApi` -> `Flow38FeedbackAdapter` -> `GraphDpoSourceRow` -> legal/provenance/split gate -> quarantined unit-level `GraphDpoPairCandidate` only unless positive proof exists. | Blocker 2 table; matrix R22; plan-07 five-format adapter matrix; plan-14 R17; PASS15 and PASS16 sections in full-task gate/master/source map. | PASS16_B4_R17_MOCK_SEAM_SPECIFIED_NEEDS_IMPLEMENTATION_AND_REAL_REPLACEMENT_PROOF |
| `LEGAL_TEACHER_JUDGE_PROVIDER` | Original task legal model policy and provenance fields; active plans 08/09/10 legal teacher/judge restrictions. | `llm_mvp_core` legal teacher/judge gate and adapter; platform/flows only through approved adapter. | Blocker 2 table; blocker-4 provider/provenance proof contract; DPO legal policy files retain real provider proof as future evidence before training/common-use claims. | CLOSED_IN_PLAN_EVIDENCE_GATE |
| `PROVIDER_TERMS_LICENSE_METADATA` | Original task provider metadata fields; active legal policy requiring terms/license/provenance proof. | `llm_mvp_core` legal/provider evidence registry or legal gate input. | Blocker 2 table; ProviderProof/LicenseProof fields; future evidence gates. Pending/missing proof routes to quarantine/eval-only/block/rebuild-required. | CLOSED_IN_PLAN_EVIDENCE_GATE |
| `MOCK_REPLACEMENT_LEDGER` | Current work order and active external mock gate. | Future product-readiness evidence file only, outside current write scope. | Later `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`; not created now. | RULE_DEFINED_FILE_CREATION_NOT_AUTHORIZED_NOW |

### R22.1 PASS15 Better-Output / Five-Format Alias Boundary

PASS15 source-map decision:

- B-16 remains only N-to-1 fan-in merge. Better-output/FLOW-38/five-format wording cannot close B-16.
- The five named DPO input formats are the plan-07 adapter matrix: `DpoPair`, `RoutingDpoPair`, `OrchestrationPromptDpoPair`, `CodeTraceBuilderDpoPair`, and `CrossStackBusinessFlowGraphDpoPair`.
- Better-output/FLOW-38 is a separate B-4/R17 output-feedback adapter path, not one of those five existing format proofs unless a later source explicitly says so.

PASS32 source-map decision:

- B-16 typed carrier materialization is now active plan text: `IEdgeFanInMerger.Merge(branchResults, runId)` returns `GraphFanInMergeResult`.
- Required carrier fields are `Merged`, `Context`, `FanInMergeRole MergeRole`, `bool MergeOrderIsAuthority`, `MergedBranchCount`, conflicts/rejected alternatives/provenance refs/trace refs/failure reason codes.
- Required v1 values are `MergeRole == FanInMergeRole.DeterministicFanInBaseline` and `MergeOrderIsAuthority == false`.
- Permutation-invariance proof must read fields from `GraphFanInMergeResult`, not prose, and verify identical `Merged`/`Context` across branch-order permutations.
- Status is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`: no implementation/test/runtime proof, no B-22 closure, no readiness, no implementation authorization.

Retained PASS16 R17/B19 boundary (evidence-only, not Pass32 scope):

- B-19 owns only the host/API physical-route proof boundary for R17; source map text does not prove repo/path/branch/file existence.
- External/host feedback remains mock-only until an approved execution phase. The required future unit test must assert that unverified client/third-party origin remains quarantine/personal/eval-only and cannot become common/train-ready without positive legal/provenance/split proof.

### R22.2 PASS16 Better-Output Mock-Seam Source Map

| source family | exact evidence used | status in PASS16 |
|---|---|---|
| Claude baseline | `claude-plan-07-dpo-builder.md` maps FLOW-38/UI feedback to `GeneratedUiOutputFeedbackEvent`, default `PersonalAdaptive`/`Quarantine`, and DPO builder legal gate; `claude-plan-08-legal-teacher-gate.md` C16 maps client/third-party output without positive proof to `QUARANTINE`; `claude-plan-14-stage-references.md` lines 704-729 and 956-963 reclassify R17/FLOW-38 as `NEW_PLANNED_CONTRACT` with event, entrypoint, adapter, and legal route while preserving no physical implementation proof. | Baseline authority for route, legal/quarantine behavior, and non-readiness. It does not provide physical implementation proof or a real external call. |
| Active-codex current plan | `active-codex-execution-16-full-task-coverage-simulation-gate.md` PASS16 and master PASS16 now specify seam interfaces, synthetic event, mock FLOW-38 response, normalized `GraphDpoSourceRow`, quarantined `GraphDpoPairCandidate` preview, unit-test name/assertions, and replacement-ledger path. | Historical/pre-post-repair active plan repair evidence; it kept `PLAN_NOT_READY` at that pass. |
| Codex supplement / delta | `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md` requires `GeneratedUiOutputFeedbackEvent` fields and states Q37/Q38 block FLOW-38 `dpo_train_ready=true`; `codex-50-cycle-plan-review-supplement.md` requires provider terms/model-license snapshots and legality of better-output before training rows. | Validated non-contradictory delta/source-map input. It cannot authorize implementation or readiness. |
| PASS16 mock contract | `GeneratedUiOutputFeedbackEvent(event_id=evt-flow38-pass16-001, correlation_id=corr-flow38-pass16-001, tenant_scope=tenant:test-local, prompt_hash=sha256:prompt-r17-pass16, chosen_output_hash=sha256:chosen-r17-pass16, rejected_output_hash=sha256:rejected-r17-pass16, source_category=client_unverified, mock_source=SYNTHETIC_FIXTURE)` plus `Flow38FeedbackResourceResponse(resource_status=mock_feedback_recorded, feedback_ref=mock://flow38/feedback/pass16, score_delta=0.42, terms_snapshot_status=missing_or_pending, output_license_status=pending, provenance_status=client_supplied_unverified)`. | Exact assumed mock input and mock resource response for future approved test. Synthetic and not train-ready. |
| PASS16 normalized output | One `GraphDpoSourceRow` preserving event and chosen/rejected hashes with `legal_verdict=QUARANTINE`, `allowed_use=PersonalAdaptive`, `route=adaptive_or_quarantine`, `semantic_quality_claimed=false`, `mock_source=SYNTHETIC_FIXTURE`, and `dpo_train_ready=false`; any `GraphDpoPairCandidate` preview is unit-level/quarantined/non-common/non-train-ready. | Exact plan-level output expectation; no code/test execution occurred. |
| PASS16 unit test / replacement | Future approved test `Flow38FeedbackAdapterTests.MockFeedbackEvent_NormalizesToQuarantinedGraphDpoSourceRow`; real replacement after product readiness is recorded only in `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`. | Test and replacement ledger are specified but not created/executed in PASS16. |

### R22.3 PASS08 R-OT-08 External Seam Recheck

PASS08 rechecked the full R-OT-08 rule against active-codex and the read-only Claude baseline. Current result: the plan encodes the rule at plan-contract level, but implementation/tests/real calls remain future evidence and overall readiness is not claimed.

| seam_id | PASS08 coverage result | required implementation-facing contract |
|---|---|---|
| `FIGMA_CANVA_DESIGN_TO_HTMLCSS` | Covered in-plan by the Blocker 2 seam table, plan-14, plan-13, and this R22 source map. | Request/response fields, deterministic html/css mock expected response, unit test mapping to `HtmlCssCandidate`, client-origin quarantine/provenance rule, and later replacement entry in `codex-external-calls-mock.md`. |
| `BETTER_OUTPUT_FEEDBACK_TO_DPO` | Covered in-plan by PASS15/PASS16 plus the Blocker 2 seam table. | `GeneratedUiOutputFeedbackEvent` -> FLOW-38/core adapter -> `GraphDpoSourceRow`, mock resource response, unit test asserting `QUARANTINE`/`PersonalAdaptive`/`dpo_train_ready=false`, and later replacement entry. |
| `LEGAL_TEACHER_JUDGE_PROVIDER` | Covered in-plan by plan-08/09/13 and the Blocker 2 seam table. | `ITeacherGoalClient` and `IJudgeScoreClient` request/response fields, `ExternalTeacherMock`/`ExternalJudgeMock` expected responses, unit tests for permissive proof, missing terms/license, eval-only judge, and `DATASET_REBUILD_REQUIRED` contamination. |
| `PROVIDER_TERMS_LICENSE_METADATA` | Covered in-plan by legal/provenance policy, ProviderProof/LicenseProof gates, and the Blocker 2 seam table. | Proof-resource request/response fields, pending metadata mock, unit test blocking training/common use when proof is missing, and replacement entry when real terms/license proof is supplied after approval. |
| `MOCK_REPLACEMENT_LEDGER` | Covered as future file target only. | Later path exactly `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`; not created in planning repair; only allowed technical debt. |

No real external API/resource/provider call, network lookup, provider terms retrieval, product implementation, test/build/runtime probe, browser check, git action, or replacement-ledger file creation is authorized by this PASS08 source-map recheck.

### R23 - Same-Scenario Dual-Path Host/Extension Proof Design

`R23` closes blocker 3 in-plan. It does not authorize implementation, tests/builds, browser/host/UI probing, screenshots, login actions, network calls, or runtime claims.

| scenario_id | source evidence used | participating repos/components | design location | current status |
|---|---|---|---|---|
| `SCN-R16-DESIGN-TO-HTMLCSS` | Original task c6a/eb1 names `xiigen-platform` figma API plus FigmaToCode/CanvaToCode callers; attachment 483d locks Q-host variant; plan-14 records FLOW-31 as design analysis and client html/css generation. | `xiigen-platform-dot-net-angular` facade; `xiigen-flows-dot-net-angular` FLOW-31 design analysis; `FigmaToCode-design-gen` and `CanvaToCode` client/caller UI surfaces only; `llm_mvp_core` only for future approved core seam. Same-scenario proof must join `scenarioId + sourceClient + designSourceRef + designSnapshotHash + sourceUnitRef + sourcePathRef + expectedOutputArtifactIds + provenanceSidecarRef + runId + capturedAtUtcConvention + correlationId + tenantScope`. Path A-only is `api_only`; Path B-only is `ui_only`; neither is accepted coverage. | `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md` SCN-R16. | CLOSED_IN_PLAN_EXECUTION_EVIDENCE_PENDING |
| `SCN-R17-BETTER-OUTPUT-DPO` | Original task c6a/eb1 better-output route; attachment 483d Q37/Q38 policy; plan-08 C16; plan-14 FLOW-38 notes; PASS15 B-4/R17 adapter contract and B-19 route boundary. | `xiigen-platform-dot-net-angular` feedback facade; `xiigen-flows-dot-net-angular` FLOW-38 feedback event; `llm_mvp_core` GraphDPO/legal gate; client/Smart* callers only as thin callers. Same-scenario proof must join `scenarioId + eventId + flow38ResourceRef + promptHash + inputContextHash + sourceUnitRef + sourcePathRef + chosenOutputHash + rejectedOutputHash + expectedOutputArtifactIds + provenanceSidecarRef + normalizedGraphDpoSourceRowId + runId + capturedAtUtcConvention + correlationId + tenantScope` plus the same legal/quarantine outcome. Path A-only is `api_only`; Path B-only is `ui_only`; neither is accepted coverage. | `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md` SCN-R17 plus PASS15 full-task gate/source-map rows. | PASS15_ROUTE_SPECIFIED_EXECUTION_EVIDENCE_PENDING |
| `SCN-HIST-HOST-EVIDENCE-ONLY` | Attachment 429f dual-path defect and historical Claude/Cursor/Codex host/extension references. | Historical host packets only; not one of the five product implementation repos unless re-run/re-bound to SCN-R16 or SCN-R17 equality tuple. | `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md` historical boundary. | EVIDENCE_ONLY_NOT_PRODUCT_READINESS |

### R21.2 Latest-Updated Source Branch Rule

The design decision is the rule, not a concrete branch name captured during this planning repair. For each implementation repo, the future executor determines the source authority at implementation start by read-only branch/ref inspection inside that repo: collect local and remote-tracking branches, compare their latest commit timestamps, use commit identity as tie-break evidence if timestamps are equal, and record the selected branch name, timestamp, commit/ref, command/tool evidence, and whether the working copy was created from that authority. The selected authority branch is never the direct commit target for this work. If branch inventory is unavailable, stale, ambiguous, or points only to copied/discovered workdirs, the executor records a blocker/decision packet; it must not fall back to `C:\Users\lubao\Documents\xiigen`, `planning-dpo-learning`, or any copied folder as authority.

### R21.3 Design Decisions Now vs Execution Gates Later

| item | classification | plan rule |
|---|---|---|
| Participating implementation repositories | DESIGN_DECISION_CLOSED_NOW | Exactly five from Claude baseline/original task: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. |
| `CanvaToCode` scope | DESIGN_DECISION_CLOSED_NOW | Included only because Claude baseline/original task names it as caller; discovered/copied workdirs cannot introduce it. |
| Planning repo boundary | DESIGN_DECISION_CLOSED_NOW | `C:\Users\lubao\Documents\xiigen` is planning/evidence only and is not an implementation repo, branch target, source authority, commit target, PR target, or merge target. |
| Source authority branch policy | DESIGN_DECISION_CLOSED_NOW | Use most advanced/latest-updated branch per participating repo as read-only authority, determined by future read-only evidence at implementation start. |
| Working directory/branch policy | DESIGN_DECISION_CLOSED_NOW | Work under `C:\Projects\DPOLegalLearning\<repo-name>` with work name/branch `DPOLegalLearning`, created from the selected authority only after fresh implementation approval. |
| Commit/push/mergeability policy | DESIGN_DECISION_CLOSED_NOW | No current commit/push. Future commit/push requires source refresh and mergeability/no-conflict check against selected authority before commit/push. |
| PR/merge policy | DESIGN_DECISION_CLOSED_NOW | No PR and no merge unless Luba gives a separate future instruction. |
| Concrete selected branch names/refs | EXECUTION_GATE_LATER | Captured at implementation start by read-only evidence because branches can change after this plan repair. |
| Actual workdir creation/copying/branch creation | EXECUTION_GATE_LATER | Requires plan readiness, fresh Luba implementation authority, and bounded sub-agent work order. |
| Tests/builds/runtime/host probes | EXECUTION_GATE_LATER | Not authorized in this planning repair. |

### R21.4 Russian Summary For Luba

Источник правды по коду — не папка планирования и не случайно найденная копия. Для каждого product repo будущий исполнитель сначала read-only определяет самый свежий обновлённый branch в самом этом repo и использует его только как authority. Работать надо отдельно в `C:\Projects\DPOLegalLearning\<repo-name>` / `DPOLegalLearning`, а перед любым commit/push заново сверить source branch и mergeability. `C:\Users\lubao\Documents\xiigen` остаётся местом планов и evidence; в нём нельзя создавать implementation branch, делать commit/push/PR/merge или считать `planning-dpo-learning` authority для реализации.

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

## Sequential Pass 10 R-OT-10 Human-Readable Documentation Source Map

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This source map entry covers only the R-OT-10 implementation documentation surface. It is a routing map, not implementation proof.

| R-OT-10 evidence slice | Active owner / consumer | Claude read-only baseline counterpart | Code-ref policy |
|---|---|---|---|
| UML and status | `active-codex-execution-plan-00-partA-uml-status.md`; consumers: master, matrix, board | `claude-plan-00-partA-uml-status.md` | Existing owner docs may cite code. This pass did not re-verify source lines. Missing or unverified line/quote evidence is `NEEDS_CODE_REF_INVENTORY`. |
| algorithm | Active plans 03/04/06/07/08/09/10/11 | Claude plans 03/04/06/07/08/10/11 plus Claude auto-DPO baseline where present | Algorithm descriptions must stay human-readable and cannot hide architecture decisions as future gates. |
| code refs / line / quote | Plan-00, plan-07, guides 01-04, and exact owner files named by a future implementation work order | Claude plan/guides with matching code-reference sections | No new fake line refs. If a future implementer cannot inspect the source line, they must write `NEEDS_CODE_REF_INVENTORY`, not a guessed `file:line` quote. |
| hierarchy | `active-codex-execution-plan-05-partA-hierarchy.md`; guide-04 | `claude-plan-05-partA-hierarchy.md`; `claude-guide-04-ilearningmachine-pattern-and-graph.md` | Current hierarchy is `ILearningMachine` / L5 DynamicGraph -> `ILogicalOrchestrator` -> `ILogicalFlow` -> `ILogicalUnit`; no L6. |
| stage / subphase / substep / Gate B | `active-codex-execution-plan-14-stage-references.md`, board, matrix | `claude-plan-14-stage-references.md`, Claude board, Claude matrix | Stage references must point to owner files, dependencies, and Gate B proof. Hidden subtasks are blockers. |
| guide | Active guides 01-04 | Claude guides 01-04 | Guides explain build DPO, train, measure, and pattern/graph usage. They do not prove product code or training happened. |

R-OT-10 trace rule: any later implementation work order must name the exact owner file(s), phase/subphase/substep, Gate B proof, code refs status, and guide link. If those are absent, the work order is `NEEDS_FIX_R_OT_10_DOC_TRACE`, not implementation-ready.

## PASS11 R-OT-11 Source-to-Plan / B-1..B-22 Source Map

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Source-to-Plan status for this source map:

| target | actual found in active-codex | status | source owner | next repair/proof |
|---|---|---|---|---|
| `target 64` baseline substeps | Pass18 enumerates `source_to_plan_roster_rows = 64` denominator rows in plan-14; Pass27 records `MAPPED_ACCEPTED_DELTA = 50`, `MAPPED_VERIFIED = 0`, `PARTIAL_NEEDS_OWNER_REVIEW = 0`, `ROSTER_ONLY_NEEDS_SOURCE_REF = 0`, `NEEDS_CODE_REF_INVENTORY = 14`, and complete `64/64` Source-to-Plan completion remains not proven | `incomplete` | `active-codex-execution-plan-14-stage-references.md` section 3.2 Pass27 Ф6 row-evidence repair | Verify every remaining non-accepted row's source, owner, consumers, acceptance state, and readback before readiness. |

B inventory source routing:

| inventory range | current route | status |
|---|---|---|
| B-1..B-22 | Historical/pre-Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md` Pass 11 overlay | complete enumeration was present after Pass 11; current classification is superseded by Pass39 |
| B-5 | Attachment 0ce0 omitted B5; attachment 912 grouped B-5..B-20 | historical/pre-Pass39 `MISSING_NUMBER_NEEDS_TRACE_REPAIR`; Pass39 classifies B-5 as `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING` |
| B-22 | Active-vs-Claude reconciliation plus Source-to-Plan target/actual/current gaps | historical/pre-Pass39 open/not-proven; Pass39 classifies B-22 as `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING` |

Historical/pre-Pass39 Source-to-Plan, recon-library, B-1..B-22, and active-vs-Claude gaps were PLAN_NOT_READY repair queue items. Current Source-to-Plan inventory is 64/64 accepted as inventory evidence only after Pass30, and current B-row classification is the Pass39 table; neither status is readiness, implementation authorization, or execution proof.

## Historical Pass19 Source-to-Plan Row-Evidence Upgrade Mirror

Historical/pre-Pass20 boundary: Pass18 converted the Source-to-Plan denominator from prose/support groups into an inspectable roster in `active-codex-execution-plan-14-stage-references.md` section 3.2. Pass19 upgraded only the rows with direct attachment/active/Claude support, owner/source-map acceptance, downstream mirrors, and readback evidence. Current counts are superseded by the Pass20 mirror below. Neither Pass19 nor Pass20 closes B-22, claims `64/64`, accepts Claude Repair2, or authorizes implementation.

| roster fact | Pass19 evidence | boundary |
|---|---|---|
| denominator rows | `source_to_plan_roster_rows = 64` in plan-14 section 3.2 Pass18 roster | This remains a roster denominator only, not completion proof. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 13` | Historical/pre-Pass20 accepted-delta rows were Ф0.1.1, Ф0.1.2, Ф0.1.3, Ф2.1.1, Ф2.1.2, Ф3.1.1, Ф3.1.2, Ф4.3.2, and Ф10.3.1 through Ф10.3.5. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 4`; `NEEDS_CODE_REF_INVENTORY = 45`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 2` | Historical/pre-Pass20: Ф10.1.1, Ф10.1.2, Ф10.2.1, and Ф10.2.2 were still partial. Pass20 mirror upgraded those four rows; current Pass21 mirror below upgrades the final-control rows. |
| downstream mirrors | master plan, board, matrix, full-task gate, library index, plan-14 | Current mirrors point to the Pass20 boundary below; Pass19 rows remain evidence-only history. |

## Pass20 R16/R17 Seam Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass20 repairs only the four seam rows Ф10.1.1, Ф10.1.2, Ф10.2.1, and Ф10.2.2 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove implementation, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass20 evidence | boundary |
|---|---|---|
| denominator rows | `source_to_plan_roster_rows = 64` in plan-14 section 3.2 | Denominator only; no completion proof. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 17` | Historical/pre-Pass21: the four added rows are Ф10.1.1, Ф10.1.2, Ф10.2.1, and Ф10.2.2. Acceptance is planning/source-map only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 45`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 2` | Historical/pre-Pass21/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| R16 backing | `SR-16-CoreBacking` direct readback from `C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionMachine.cs`, `DesignConversionContracts.cs`, and `DesignConversionDpoGenerator.cs`; original attachments a6/dc3 lines 334/336 keep FigmaToCode/CanvaToCode as callers. | Existing in-core DesignConversion/R16 backing only; not platform host proof and not seed corpus. Platform/FLOW-31/client-host proof remains future implementation evidence under `SR-16-RouteFacade`. |
| R17 planned-new artifact | SR-14 `NEW_PLANNED_CONTRACT` with `GeneratedUiOutputFeedbackEvent`, `Flow38FeedbackResourceResponse`, `GraphDpoSourceRow`, QUARANTINE/PersonalAdaptive default, future unit test `Flow38FeedbackAdapterTests.MockFeedbackEvent_NormalizesToQuarantinedGraphDpoSourceRow`, and future replacement row in `codex-external-calls-mock.md`. | No physical `.cs` implementation proof, no external call, no train-ready/common proof. |

## Pass21 Ф11 Final-Control Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass21 repairs only rows Ф11.1.1 and Ф11.2.1 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not fill the completion matrix with execution proof, does not run or count ten execution reviews, does not write `ARCHITECT_VERDICT_ALLOW_FINAL`, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass21 evidence | boundary |
|---|---|---|
| denominator rows | `source_to_plan_roster_rows = 64` in plan-14 section 3.2 | Denominator only; no completion proof. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 19` | Historical/pre-Pass22 Pass21 count. The two added rows are Ф11.1.1 and Ф11.2.1. Acceptance is planning/source-map control only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 45`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass22/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Backing-Ф11.1.1 | Original attachments a6/dc3 lines 27-29 require stage/subtask references and full requirement coverage checks; active board Ф11.1.1 and Claude board Ф11.1.1 define the requirement-to-phase-to-evidence matrix; active/Claude requirements matrices include Ф11 as final acceptance with matrix-without-gaps. | Plan-control evidence only; actual completion rows still need execution evidence and 0 `NOT_PROVEN` before readiness. |
| Backing-Ф11.2.1 | Active board Ф11.2.1 and Claude board Ф11.2.1 define the ten execution reviews plus final architect verdict gate; active/Claude board §B.6 exposes the ten-review ledger with all rows still not run and `counts_toward_10=false`; `CLAUDE.md` Final Execution Review Gate requires at least ten real independent execution reviews and exact verdict token `ARCHITECT_VERDICT_ALLOW_FINAL`. | Planned gate evidence only; 0 reviews are counted and no final verdict is claimed. |

## Pass22 Ф1 DynamicGraph Contract/Skeleton Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass22 repairs only rows Ф1.1.1, Ф1.1.2, Ф1.1.3, Ф1.2.1, Ф1.2.2, Ф1.2.3, Ф1.3.1, Ф1.3.2, and Ф1.3.3 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical DynamicGraph implementation, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass22 evidence | boundary |
|---|---|---|
| denominator rows | `source_to_plan_roster_rows = 64` in plan-14 section 3.2 | Denominator only; no completion proof. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 28` | The nine added rows are Ф1.1.1 through Ф1.3.3. Acceptance is planning/source-map contract/skeleton evidence only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 36`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass23-F2/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Anchor-Ф1-existing-lower-level | `LINE_REF_VERIFIED` in plan-14 `SR-17-legacy-active-logicalcontracts` plus SR-18..SR-23 for `LogicalContracts.cs`, `LearningMachineContracts.cs`, `DynamicDomainGraph.cs`, `GraphEngine`, `LogicalTopologyRegistry.cs`, `AdaptiveLogicalOrchestrator.cs`, skill-selection/router precedents, and example `ILearningMachine` implementations. | These anchors support role mapping and implementation boundaries. Current `SR-17` is the `language-translate` seed corpus, not `LogicalContracts`. These anchors are not physical DynamicGraph implementation proof. |
| Anchor-Ф1-planned-new-contract-family | plan-14 SR-24 records `NEW_PLANNED_CONTRACT` for `DynamicGraphDomain`, `IDynamicGraphNode`, `DynamicGraphNodeDescriptor`, `GraphNodeCandidate`, `IDynamicGraphEdge`, `GraphEdgeKind`, `DynamicGraphTopology`, `DynamicGraphEdgeFlow`, `IEdgeCandidateProvider`, `IEdgeFanInMerger`, and `DynamicGraphMachine`; read-only `.cs` scan found no physical DynamicGraph implementation hits. | DynamicGraph-specific contracts remain future implementation work. |

## Pass23-F2 Ф2 DPO/Legal Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass23-F2 repairs only rows Ф2.1.3, Ф2.2.1, and Ф2.2.2 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical `GraphDpoPair`/`TeacherProvenance` implementation, does not run a live `sonnet-*` contamination scan, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass23-F2 evidence | boundary |
|---|---|---|
| roster denominator | 64 rows | Exact row-id count stays 64. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 31` | The three added rows are Ф2.1.3, Ф2.2.1, and Ф2.2.2. Acceptance is planning/source-map DPO/legal evidence only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 33`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass24/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Ф2.1.3 | `MAPPED_ACCEPTED_DELTA` | Quarantine/rebuild policy plus scanner route. Live contaminated-row count remains `DATA_SCAN_REQUIRED_AT_IMPLEMENTATION`. |
| Ф2.2.1 | `MAPPED_ACCEPTED_DELTA` | NEW planned `GraphDpoPair` canon plus verified projection/materialization anchors; no physical class proof. |
| Ф2.2.2 | `MAPPED_ACCEPTED_DELTA` | Five physical input-format anchors plus NEW canon/legal batch gate; adapter execution and tests remain future proof. |

## Pass24 Ф3 DPO-Builder Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass24 repairs only rows Ф3.1.3, Ф3.2.1, and Ф3.2.2 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical `GraphDpoBuilder`, `GraphDpoSchemaValidator`, `GraphDpoPair`, or `GraphDpoSourceRow` implementation, does not run `BuildRejected`, does not materialize data, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass24 evidence | boundary |
|---|---|---|
| roster denominator | 64 rows | Exact row-id count stays 64. |
| historical/pre-Pass25 mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 34` | Historical/pre-Pass25 Pass24 snapshot only: the three added rows were Ф3.1.3, Ф3.2.1, and Ф3.2.2. Current Pass26 boundary is `MAPPED_ACCEPTED_DELTA = 46`; acceptance remains planning/source-map evidence only. |
| historical/pre-Pass25 continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 30`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass25/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Ф3.1.3 | `MAPPED_ACCEPTED_DELTA` | Planned validator/invariants plus existing anti-leakage/schema/quarantine/source-provenance precedents; no physical `GraphDpoSchemaValidator` proof. |
| Ф3.2.1 | `MAPPED_ACCEPTED_DELTA` | Planned `BuildRejected` taxonomy plus existing near-miss, `[WRONG:]`, `RejectedType`, and held-out precedents; no GraphDPO execution proof. |
| Ф3.2.2 | `MAPPED_ACCEPTED_DELTA` | Planned adapter/materialization path plus existing `LearningTrainingItem`, `DpoPair`, `DpoDataset.Load`, `DpoBatch`, trainer, feedback/logical-DPO, and source-format anchors; no adapter/test/materialization execution proof. |

## Pass25 Ф4 Trained-State/Lifecycle Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass25 repairs only rows Ф4.1.1, Ф4.1.2, Ф4.2.1, Ф4.2.2, Ф4.2.3, Ф4.3.1, and Ф4.3.3 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical `LoraApplyResult`, `LocalFileModelServer`, `TeacherProvenance`, `GraphNodePreferenceState`, or `DynamicGraphMachinePair` implementation, does not run continue-training/checkpoint/lifecycle tests, and does not run builds/runtime/browser/provider/API/network/git.

| roster fact | Pass25 evidence | boundary |
|---|---|---|
| roster denominator | 64 rows | Exact row-id count stays 64. |
| historical/pre-Pass26 mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 41` | The seven added rows are Ф4.1.1, Ф4.1.2, Ф4.2.1, Ф4.2.2, Ф4.2.3, Ф4.3.1, and Ф4.3.3. Acceptance is planning/source-map trained-state/lifecycle evidence only. |
| historical/pre-Pass26 continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 23`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass26/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Ф4.1.1 | `MAPPED_ACCEPTED_DELTA` | Existing LoRA/dropout primitives plus planned bridge for `LoraApplyResult`/`ForwardWired`; no physical bridge or trained-feature proof. |
| Ф4.1.2 | `MAPPED_ACCEPTED_DELTA` | Existing `DpoMetrics.RewardMargin`/`DpoTrainer` reward-margin computation plus `TranslationMachine` literal-defect anchors; no code removal or test proof. |
| Ф4.2.1 | `MAPPED_ACCEPTED_DELTA` | Existing parent checkpoint load, DPO step, metric checkpoint, and translation continue-training probe precedents plus Claude lifecycle contract; graph `LoraCheckpointIo` remains planned. |
| Ф4.2.2 | `MAPPED_ACCEPTED_DELTA` | Existing `IModelServer`, `IModelRepository`, `IRemoteUnitLocator`, local repository, manifest, and proxy contracts plus planned `LocalFileModelServer`; no HTTP/runtime publish proof. |
| Ф4.2.3 | `MAPPED_ACCEPTED_DELTA` | Existing generated-capability manifest/persistence and Claude `TeacherProvenance` 20-field/lifecycle proof contract; physical `TeacherProvenance` and graph rollback manifest remain planned. |
| Ф4.3.1 | `MAPPED_ACCEPTED_DELTA` | Existing L5 `ILearningMachine`, `LearningTrainingItem`, logical-DPO unit, and DPO trainer support unit-level planned training; `GraphNodePreferenceState` remains planned. |
| Ф4.3.3 | `MAPPED_ACCEPTED_DELTA` | Current v1 uses the common/global checkpoint surface only; existing `ILearningMachinePair` personal/global contract is retained as later adaptive activation precedent, not v1-active proof. 25.ModelServer common-leg precedents plus Claude Q6 split support the plan; `DynamicGraphMachinePair`/`LegScope` graph wiring remains planned and deferred until Luba activates client-local/adaptive. |

## Pass26 Ф5 Measurement/Quality/Lifecycle Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass26 repairs only rows Ф5.1.1, Ф5.1.2, Ф5.1.3, Ф5.2.1, and Ф5.2.2 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical `GraphMeasurementReport`, `IGraphMeasurementRunner`, `IHeldOutScorer`, `GraphQualityMeter`, or `GraphLifecycleProbe` implementation, does not execute measurement/held-out/ablation/fresh-load/continue-training, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass26 evidence | boundary |
|---|---|---|
| roster denominator | 64 rows | Exact row-id count stays 64. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 46` | The five added rows are Ф5.1.1, Ф5.1.2, Ф5.1.3, Ф5.2.1, and Ф5.2.2. Acceptance is planning/source-map measurement/quality/lifecycle evidence only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 18`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass27/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Ф5.1.1 | `MAPPED_ACCEPTED_DELTA` | Existing learning metric contracts and live DPO metric boundaries plus Claude `MeasurementReport`/runner baseline; graph report/coordinator/speed-meter remain `NEW_PLANNED_CONTRACT`. |
| Ф5.1.2 | `MAPPED_ACCEPTED_DELTA` | Existing held-out evaluation contracts, source-disjoint/leakage precedents, and Claude `IHeldOutScorer`/split-proof baseline; no graph held-out scoring execution proof. |
| Ф5.1.3 | `MAPPED_ACCEPTED_DELTA` | Existing source/prompt ablation and semantic-quality=false precedents plus Claude untrained-honesty/no-shortcut baseline; graph reflection/no-regex probe remains planned. |
| Ф5.2.1 | `MAPPED_ACCEPTED_DELTA` | Existing reward/quality/leakage/quarantine precedents plus Claude per-unit/per-edge novelty/ablation/permutation gates; no `GraphQualityMeter` or per-edge ablation execution proof. |
| Ф5.2.2 | `MAPPED_ACCEPTED_DELTA` | Existing export/import/evaluate contracts and fresh-load/continued probe precedents plus Claude lifecycle/two-PASS gates; graph lifecycle probe execution remains future evidence. |

## Pass27 Ф6 Async-Combinations Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass27 repairs only rows Ф6.1.1, Ф6.1.2, Ф6.2.1, and Ф6.2.2 at planning/source-map level. It does not close B-22, does not claim `64/64`, does not prove physical `IGraphCombinationStrategy`, `CombinationPlan`, `IGraphCandidateRunner`, `GraphCandidateOutput`, `GraphAsyncBranchResult`, `ICombinationJudgeUnit`, `TeacherGoalGate`, `GraphDpoPairAssembler`, `GraphManyToManyRoute`, or `IEdgeFanInMerger` implementation, does not execute async combinations/candidate collection/judging/pair assembly/fan-in merge, and does not run tests/builds/runtime/browser/provider/API/network/git.

| roster fact | Pass27 evidence | boundary |
|---|---|---|
| roster denominator | 64 rows | Exact row-id count stays 64. |
| mapped completion | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 50` | The four added rows are Ф6.1.1, Ф6.1.2, Ф6.2.1, and Ф6.2.2. Acceptance is planning/source-map async-combinations evidence only. |
| continuation states | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 14`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass28/pre-Pass39 queue only; current Pass30 Source-to-Plan inventory is 64/64 accepted as inventory evidence, and Pass39 B-22 classification is Pass39 execution-proof-pending. |
| Ф6.1.1 | `MAPPED_ACCEPTED_DELTA` | Existing branch/dependency precedents plus Claude owner contract for `N=combinationCount=4`, ceiling 8, `CombinationPlan`, unique `combination_id`, and dedup. Planner/strategy/runner remain planned. |
| Ф6.1.2 | `MAPPED_ACCEPTED_DELTA` | Existing `Task.WhenAll` branch execution and arbiter-precedent code plus Claude `GraphCandidateOutput`/queryable-to-snapshot boundary. Candidate collection remains planned. |
| Ф6.2.1 | `MAPPED_ACCEPTED_DELTA` | Claude judge-origin, deterministic-baseline, legal teacher, closed-provider, service-mode, and grounding gates plus existing arbiter precedent. Judge/gate implementation remains planned. |
| Ф6.2.2 | `MAPPED_ACCEPTED_DELTA` | Existing `LearningBatch`/`LearningTrainingItem`/`TrainBatchAsync` contracts plus Claude unit-level DPO and metadata-envelope rules. Pair assembler and many-to-many merge remain planned. |

## PASS27 Original-Task Coverage And Latest Baseline Reconciliation

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. See `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md` for the controlling latest-baseline reconciliation context. Current active-codex canon after latest-baseline reconciliation plus Pass30 Ф9 consolidated test-strategy source-map repair: 12 phases / 27 subphases / 64 substeps; `TeacherProvenance` 20 fields (13 base + 5 NEW + 2 RR6 `service_mode`/`grounding_used`); exactly one active pilot `language-translate`; Source-to-Plan inventory rows are 64/64 accepted as source-map inventory evidence only; hosted Qwen3/Together train use remains blocked until provider terms proof; held-out scorer defaults to `RuntimeProductAccuracy` plus per-domain scorer (`language-translate`: exact-match + char-accuracy). Any older unqualified 12/26/59, 18-field, 8/64 (частично; historical/pre-PASS27 corrected from 8/59), 28/36, 31/33, 34/30, 41/23, 46/18, 50/14, 54/10, 58/6, incomplete-source-inventory, or narrower ready wording in this file is historical/pre-0aa evidence only.

## Pass28 Ф7 Auto-DPO Loop Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass28 mirrors only the active-codex plan-14 upgrades for rows `Ф7.1.1`, `Ф7.1.2`, `Ф7.2.1`, and `Ф7.2.2` to `MAPPED_ACCEPTED_DELTA`. The evidence scope is source-map/plan only: Claude baseline plus active plan-09 plus read-only source anchors. It is not product implementation, not a train/evaluate run, not a test/build/runtime/provider/API/network/git result, not B-22 closure, and not `64/64` Source-to-Plan completion.

| Pass28 mirror item | mapped state | source-reference evidence | boundary |
|---|---|---|---|
| Pass28 item: Ф7.1.1 weakness detector and TeacherGoal/legal teacher units | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-57/SR-58/SR-62; active plan-09 weakness/teacher-goal steps; Claude plan-09/08 legal teacher baseline. | Existing ResultGap/low-result anchors are precedents only; graph weakness detector and provider route remain planned. |
| Pass28 item: Ф7.1.2 loop pair builder and replay | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-59/SR-62; Claude plan-07 `GraphDpoPair`/TeacherProvenance owner contract; replay-buffer anchors. | Unit-level `GraphDpoPair` contract accepted; graph pair builder and anti-forgetting replay execution are not proven. |
| Pass28 item: Ф7.2.1 AutoDPO prepare/run/train/evaluate loop | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-60/SR-61/SR-62; `TrainBatchAsync`, `EvaluateAsync`, held-out/source-disjoint anchors; Claude plan-09/10 loop baseline. | Planned loop contract accepted; no `AutoDpoLoopOrchestrator` physical implementation or training proof. |
| Pass28 item: Ф7.2.2 ShouldStop threshold/plateau/budget/no-signal and promotion | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-61/SR-62; Claude plan-09/11 locked thresholds and no-signal/promotion contract. | Threshold defaults are design locks only; no graph `ShouldStop` runtime or promotion proof. |

Pass28 Source-to-Plan count at that time: total=64; `MAPPED_VERIFIED=0`; `MAPPED_ACCEPTED_DELTA=54`; `PARTIAL_NEEDS_OWNER_REVIEW=0`; `NEEDS_CODE_REF_INVENTORY=10`; `ROSTER_ONLY_NEEDS_SOURCE_REF=0`. Historical/pre-Pass30 Pass29 count is recorded below, and the current Pass30 count is recorded after that.

## Pass29 Ф8 No-Hidden-Determinism Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass29 mirrors only the active-codex plan-14 upgrades for rows `Ф8.1.1`, `Ф8.1.2`, `Ф8.2.1`, and `Ф8.2.2` to `MAPPED_ACCEPTED_DELTA`. The evidence scope is source-map/plan only: Claude baseline plan-12 plus active plan-12 plus read-only source anchors. It is not product implementation, not hidden-determinism removal in production, not a train/evaluate run, not a test/build/runtime/provider/API/network/git result, not B-22 closure, and not `64/64` Source-to-Plan completion.

| Pass29 mirror item | mapped state | source-reference evidence | boundary |
|---|---|---|---|
| Pass29 item: Ф8.1.1 inventory/classify 9 deterministic or baseline mechanisms | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-63/SR-64/SR-68; active/Claude plan-12 9-mechanism table. | Inventory/classification accepted at planning level only; formulas, keyword scorers, phase planner, KNN, latest-wins, source-path handling, and confidence-floor remain code anchors/baselines, not proof that DynamicGraph cutover has happened. |
| Pass29 item: Ф8.1.2 replacement blueprints | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-63/SR-65/SR-68; active/Claude plan-12 blueprint sections. | Replacement blueprint accepted as future trainable contract: `SemanticEdgeRouterUnit`, typed state, training update, checkpoint/ablation, and excluded-leakage signals. Deterministic mechanisms stay labelled baseline/internal/eval/model-component until future proof. |
| Pass29 item: Ф8.2.1 `CandidateOrderLeakageGuardUnit` permutation guard | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-66/SR-68; SmartSkills guard precedent plus active/Claude edge-guard plan. | Planned/adapted edge guard only; future implementation must ensure candidate-index, array-order, original/file order, source-path, source-id, exact-prompt, correlation id, and merge order are not authority/leakage features. |
| Pass29 item: Ф8.2.2 latest-wins cutover plus ablation proof | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-61/SR-67/SR-68; active/Claude plan-12 cutover and plan-11 threshold gates. | Latest-wins cutover is gated until trained proof exists: `RuntimeProductAccuracy >= 0.95`, `AccuracyAfter - AccuracyBefore >= +0.02`, `RewardMargin >= 0.025`, and ablation drop `>=0.02`; no cutover, fresh-load, ablation run, or promotion proof is claimed. |

Historical/pre-Pass30 Source-to-Plan count after Pass29: total=64; `MAPPED_VERIFIED=0`; `MAPPED_ACCEPTED_DELTA=58`; `PARTIAL_NEEDS_OWNER_REVIEW=0`; `NEEDS_CODE_REF_INVENTORY=6`; `ROSTER_ONLY_NEEDS_SOURCE_REF=0`.

## Pass30 Ф9 Consolidated Test Strategy Source-to-Plan Repair Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass30 mirrors only the active-codex plan-14 upgrades for rows `Ф9.1.1`, `Ф9.1.2`, `Ф9.2.1`, `Ф9.2.2`, `Ф9.3.1`, and `Ф9.3.2` to `MAPPED_ACCEPTED_DELTA`. The evidence scope is source-map/plan only: active/Claude plan-13, active board Ф9 rows, Claude legal/lifecycle/test-strategy baselines, and read-only source/test anchors. It is not product implementation, not test execution, not a build/runtime/provider/API/network/git result, not training proof, not B-22 closure, and not PLAN_READY.

| Pass30 mirror item | mapped state | source-reference evidence | boundary |
|---|---|---|---|
| Pass30 item: Ф9.1.1 topology mutation tests for fan-out/fan-in/many-to-many | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-69/SR-76; active board Ф9.1.1; active/Claude plan-13 matrix. | Future topology mutation tests must create/mutate topology and assert typed edge semantics. Existing transition and lifecycle topology anchors are precedents only. |
| Pass30 item: Ф9.1.2 edge determinism and leakage ablation tests | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-70/SR-76 plus SR-63/SR-67; active/Claude plan-12/13. | Future tests must permute order/source ids/prompt ids, prove no authority leakage, and run resolver-off ablation (`GraphOrderResolverOffAblation_DropsOrBlocksOrderClaim`) plus edge-authority ablation threshold checks. Deterministic topology/Kahn/closure remains baseline/evidence only. No edge guard/cutover/resolver-off test ran now. |
| Pass30 item: Ф9.2.1 DPO generator and TeacherProvenance gate tests | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-71/SR-72/SR-76; Claude plan-07/08/13; active board Ф9.2.1. | Future tests must distinguish train-ready, blocked, and quarantine datasets; require 20-field `TeacherProvenance`; closed proprietary output is judge-only and closed train target expects `DATASET_REBUILD_REQUIRED`. |
| Pass30 item: Ф9.2.2 continue-training tests with merge/duplicate/empty-state cases | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-73/SR-76; Claude plan-10/13; active board Ф9.2.2. | Future tests must import/export/continue from parent, reject duplicate and empty-state misuse, preserve held-out separation, and keep lineage. Existing metadata probes are not real graph training proof. |
| Pass30 item: Ф9.3.1 component-to-layer assertion/Gate matrix | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-74/SR-76; active/Claude plan-13; active board Ф9.3.1. | Future component-to-layer Gate matrix must map components to L1-L5/ILearning roles, FC-26 layers, and Gate B assertions. This is matrix design acceptance only. |
| Pass30 item: Ф9.3.2 regression and anti-loophole report | `MAPPED_ACCEPTED_DELTA` | plan-14 SR-75/SR-76; active/Claude plan-13; governance gates. | Future regression and anti-loophole report must catch fake ready, hidden deterministic authority, missing mock replacement plan, stale statuses, count inflation, and no-permission-prompt regressions. No regression suite ran now. |

Current Source-to-Plan count after Pass30: total=64; `MAPPED_VERIFIED=0`; `MAPPED_ACCEPTED_DELTA=64`; `PARTIAL_NEEDS_OWNER_REVIEW=0`; `NEEDS_CODE_REF_INVENTORY=0`; `ROSTER_ONLY_NEEDS_SOURCE_REF=0`. Source-to-Plan inventory rows are 64/64 accepted as inventory evidence only; this is not PLAN_READY, not B-22 closure, not implementation authorization, not test execution, not training proof, and not product readiness.

## Pass31 Source Inventory Reconciliation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass31 keeps the Pass30 Source-to-Plan count at `64/64 accepted` for inventory/source-map evidence only. The current source-map repair queue is not another row-count pass; it is reconciliation quality:

- Pass33 disambiguates SR-16: `SR-16-CoreBacking` is in-core `llm_mvp_core` DesignConversion/R16 backing, `SR-16-RouteFacade` is the xiigen-platform facade plus xiigen-flows FLOW-31 route/client-caller authority surface, and `SeedCorpus-LanguageTranslate` is the separate active-pilot seed-corpus dependency.
- Historical/pre-Pass39 status: B-22 remained open until active-codex consumers carried the current Claude-baseline decisions without stale open-question downgrades.
- Qwen3/Together terms review and `language-translate` seed corpus are dependency evidence, not source-map row-count evidence.
- No implementation, test, runtime, provider/API, host, browser, git, commit, push, PR, merge, or product-readiness evidence was produced by Pass31.

Pass39 supersession for current B inventory: the Pass31 B-22-open bullet above is
historical/pre-Pass39 for blocker-inventory status. Current B-1..B-22 classification
is owned by the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`
plus the Pass43 B-18/B-21 default-defer supersession: B-2 is the hosted-provider/Luba approval
proof residual before hosted train/common use; B-18 is a nonblocking v1 common-only default
with adaptive deferred unless Luba later expands scope; B-21 is a nonblocking first-substantive-
recon-accepted default for current v1 planning unless Luba later reopens required depth. Other B rows are
resolved in active plan with execution proof pending. This still is not readiness,
not implementation authorization, and not a 50-pass claim.

Pass42 residual-classification supersession after Pass41 plus Pass43 B-18/B-21 default-defer repair:
current residual routing is owned by `active-codex-execution-16-full-task-coverage-simulation-gate.md`.
No inspected residual is a current implementable active-codex plan gap. B-18 and B-21 are
nonblocking v1 defaults/future expansion options, not current Luba-blocking decisions; hosted Qwen3/Together proof and Luba hosted-use approval remain
external/provider/legal proof residuals before hosted train/common use; seed file creation,
product implementation, tests/builds/runtime, training/checkpoints, host proof, branch/mergeability,
mock replacement ledger, and git remain future execution proof only; 50 accepted pass records plus
parent synthesis remain count/evidence residuals; `PLAN_NOT_READY` and implementation authorization
`NO` remain the no-go boundary.

## Pass33 B-19 SR-16 Source-Id Disambiguation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| source id | exact role | evidence boundary | future proof |
|---|---|---|---|
| `SR-16-CoreBacking` | In-core `llm_mvp_core` DesignConversion/R16 backing evidence for the figma/canva design-to-html/css seam. | Source-map/design evidence only. It is not platform host proof, not Path A/Path B proof, and not seed corpus. | Later approved implementation inventory must refresh exact branch/path/file proof and run authorized checks. |
| `SR-16-RouteFacade` | R16 route authority surface: `xiigen-platform-dot-net-angular` public facade, `xiigen-flows-dot-net-angular` FLOW-31 internal seam, and `FigmaToCode-design-gen` / `CanvaToCode` as client/caller surfaces only. | Design/source-map evidence only. FigmaToCode/CanvaToCode are not platform-host implementation authority. | Physical facade-to-FLOW-31 route files, host/API inventory, and same-scenario Path A/Path B proof remain future execution evidence. |
| `SR-17` / `SeedCorpus-LanguageTranslate-v1` | Active pilot seed-corpus requirement for `language-translate` learning-set inputs. | Current Claude SR-17 seed-corpus dependency evidence. It is not SR-16 host-route proof, not DesignConversion backing, and not the old LogicalContracts alias. | Authored or located seed data source remains required before trained-feature proof. |

B-19 after Pass33 is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING` for route/source disambiguation only. B-19 still owns the host/API physical-route proof boundary. B-4/R17 and FLOW-38 remain separate from B-19 R16 physical route proof except where host/API physical evidence is explicitly routed through B-19; DPO legal policy remains B-4/R17/legal gates, not B-19.

## Pass35 SeedCorpus-LanguageTranslate Source Contract

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Current `SR-17` is now plan-resolved as the `language-translate` seed input-prompt corpus; the planned active fixture id remains `SeedCorpus-LanguageTranslate-v1` for sourcing/shape only. It is a self-authored synthetic local fixture for the active `language-translate` pilot, with no third-party text, no external API, no web scrape, and no closed-model output. Future approved implementation stores it at `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl` or the equivalent active path under `27.TranslationDomain/SeedData/`; this pass does not create that file.

Minimum future JSONL row fields: `seed_id`, `source_language`, `target_language`, `input_text`, `chosen_translation`, `rejected_translation`, `wrong_marker` or `rejected_reason`, `split` (`train` or `held_out`), `source_category=synthetic_self_authored`, `license=project_internal_self_authored`, `TeacherProvenance` or provenance refs recording synthetic/self-authored status, `semantic_quality_claimed=false` until measured, and `expected_label` required only for `held_out` rows where it must equal `chosen_translation`. Plan default coverage is at least 12 train rows plus 4 held_out rows across at least 2 language directions; this is a future data-authoring default, not executed data.

Legal/provenance boundary: rows are common-eligible only because they are self-authored and project-internal, but they still must pass `TeacherProvenance`, split/source-disjoint, and held-out gates before training. Any later third-party/open corpus must go through provider/license/provenance proof and cannot silently replace this fixture. Held-out exact-match, char-accuracy, and `RuntimeProductAccuracy` use the `held_out` split; held-out gold comes from self-authored `expected_label == chosen_translation`, then scorer comparison applies Unicode NFC, trim, line-ending normalization, repeated ASCII whitespace collapse, and no case/punctuation/script/diacritic/emoji/name-entity forgiveness. The fixture can prove scaffold/test path only, not product quality or training success.

## Pass36 Qwen3/Together Official Source Evidence And Route Map

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Official source evidence recorded for plan classification only, not legal advice or legal approval: Qwen GitHub `https://github.com/QwenLM/Qwen3` lines 525-531, Qwen blog `https://qwenlm.github.io/blog/qwen3/` lines 29-40, Together ToS `https://www.together.ai/terms-of-service` lines 174-199, and Together privacy/security `https://docs.together.ai/docs/privacy-and-security` lines 136-166. No Together API, Qwen Chat, HuggingFace inference, model endpoint, product runtime, tests/builds, browser automation, or git action was performed by this pass.

Current route classification: `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`. Qwen3 Apache-2.0 is accepted here as selected/recommended model-license evidence for the teacher blueprint, not as hosted train/common-use proof. Claude's current v1 hosted recommendation is `qwen3-together-v1` / TogetherAI. Hosted-provider use still requires a future `ProviderTermsProof` before train/common use: `ProviderTermsProof.Provider=TogetherAI`, `ModelFamily=Qwen3`, exact `ModelId`, Qwen Apache-2.0 source ref, Together ToS snapshot, Together privacy/security/ZDR or no-store snapshot, `passthrough` disabled or upstream policy captured, `service_mode=plain`, `grounding_used=false`, `provider_terms_checked=true`, non-pending `terms_snapshot_ref`, `data_retention_mode=ZDR_or_no_store`, `output_ownership_checked=true`, `third_party_model_terms_checked=true`, and Luba hosted-use approval. Fireworks/OpenRouter/DashScope are deferred alternates only if a later plan explicitly selects and proves them.

Executor implication: v1 must not stop later to ask whether Qwen/Together is the selected hosted blueprint: it is, via `qwen3-together-v1` / TogetherAI, but it is not train/common-ready while terms proof is pending. The default train-ready plan route remains `SeedCorpus-LanguageTranslate-v1` self-authored local seed plus self-best/legal local paths until TogetherAI proof and Luba hosted-use approval exist. Missing proof routes train/common use to `BLOCK` or `QUARANTINE`; judge/eval-only use requires `used_for_training=false`, non-pending terms, organization privacy/ZDR proof, no passthrough gap, no grounding/search, and no sensitive/private raw data. Historical/pre-Pass39 B-22 wording that called propagation/reconciliation a current remaining design blocker is superseded by the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`: B-22 is `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`, not readiness, not implementation authorization, and not execution proof.

## Pass37 Source Map Latest-Claude Sync

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass37 supersedes the Pass36 route classification above where it says hosted Qwen3/Together is optional disabled-by-default or not on the v1 train-ready path. Latest inspected Claude baseline records `qwen3-together-v1` as selected v1 provider config/recommendation, with `ProviderTermsChecked=false` and `TermsSnapshotRef=PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)`; hosted train/common use remains `BLOCK` or `QUARANTINE` until a future `ProviderTermsProof` plus Luba hosted-use approval exists.

Pass37 maps the current canon owners to active execution files: `GraphDpoPair` -> `active-codex-execution-plan-07-dpo-builder.md` / Claude `claude-plan-07-dpo-builder.md`; `TeacherProvenance` -> `active-codex-execution-plan-08-legal-teacher-gate.md` / Claude `claude-plan-08-legal-teacher-gate.md` section 3.1; B-16 fan-in merge -> `active-codex-execution-plan-02-edge.md` / Claude `claude-plan-02-edge.md` with `GraphFanInMergeResult`.

Pass37 source record: `active-codex-execution-37-pass37-claude-baseline-sync.md`. It is one bounded sync/simulation record only, not 50-pass readiness evidence.
