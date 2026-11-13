## Current Code-First Execution Pointer (2026-07-14)

Current execution is authorized only for the deterministic/code-first lane. Read live status and the next bounded slice from `active-codex-execution-EXECUTION-CONTROL-BOARD.md`; read reconciled code evidence from `active-codex-execution-code-first-implementation-audit.md`. The ordered navigation is:

`CODE_FIRST -> TP1_LOCAL eval-only -> TP2_HOSTED eval-only -> G-DPO-FINAL -> DPO/training`.

The sole owner of TP1/TP2 meaning and all stage/legal/human/activation gates is `active-codex-execution-two-phase-local-remote-overlay.md`. This pointer does not restate those gates. Older “implementation authorization NO” wording is historical only for the now-authorized code-first lane; it remains effective for teacher, DPO and training work until the owner overlay's final gate passes. Deterministic validation, enumeration, routing, merge and tie-break decisions stay outside learning. Existing phase/subphase/substep counters are unchanged.

Current accepted/pushed core tip: `e2a6bce7448a1672a90bdec95106da279b077c5b`. Separate accepted slices 07-13 physically add bounded multi-combination execution, deterministic topology building, revisioned runtime ownership/read-only source, logical-registry projection, production L4 consumption, a production bootstrap facade, and the first production composition caller outside tests. The code remains partial: no deployment host/reference seam selects and deserializes a real profile or owns the runtime lifetime; persistence/restart and authority schema/canon/resolver execution are not implemented. Therefore whole Ф1.1.3, Ф1.3.2, Ф6.1.1, Gate B and the full machine are not complete.

Current manual-only chain: select a real deployment host/reference seam -> deserialize/select the explicit profile and own the runtime lifetime -> invoke `Runtime.Assistant` -> persist/restart the selected profile plus revision/CAS state -> explicitly design the authority schema/canon/resolver because current metadata is insufficient -> finish remaining deterministic `CODE_FIRST`. This is a dependency/navigation order, not a claim that the next bounded host slice has already been architecturally selected. No DPO work belongs in this chain. Exact accepted-slice commits and test/build evidence live in the top reconciliation of `active-codex-execution-code-first-implementation-audit.md`; TP1, TP2 and DPO/training remain not started.

## Two-Phase Teacher Rollout Pointer (2026-07-13, Luba live authority)

owner_pointer: единственный владелец структуры двухфазного teacher-маршрута —
`active-codex-execution-two-phase-local-remote-overlay.md`. Этот блок — только
pointer/навигация; он не переопределяет канон и не является вторым каноном.
stages: Этап 1 `TP1_LOCAL` (локальный Ollama-учитель Qwen3.5-9B Q4_K_M, профиль
`qwen35-local-v1`); Этап 2 `TP2_HOSTED` (hosted `qwen3-together-v1` и др.
допущенные нейтральные провайдеры).
name_scope_rule: `TP1_LOCAL` / `TP2_HOSTED` — имена ЭТАПОВ развёртывания учителя,
не фазы доски. Счётчики плана 12 фаз / 27 подфаз / 64 подшага НЕ меняются.
supersession_scope: этот pointer секвенирует уже сохранённое бизнес-одобрение
hosted `qwen3-together-v1` (APPROVED_BY_LUBA_2026-07-07) в Этап 2 и уточняет
Pass44 route-specific расширение; технический гейт `ProviderTermsProof`
(TogetherAI ToS/ZDR/output-use) остаётся PENDING как execution-гейт. Более старое
Pass37/Pass44 wording «Luba hosted-use approval pending» superseded ТОЛЬКО по
вопросу бизнес-одобрения.
readiness_unchanged: план остаётся `PLAN_NOT_READY`; implementation_authorization
`NO until fresh Luba approval after review`. Этот верхний блок не меняет readiness
позицией: контракты `TeacherProvenance` (20 полей, владелец plan-08 §3.1) и
`GraphDpoPair` (14 полей, владелец plan-07) не тронуты.

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
# Мастер-план ILearningDynamicGraph — v1 (claude namespace, dpo legal build)

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

> **Артефакт:** `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`
> **Автор-роль:** автор claude-плана (Opus 4.8 MAX). Это **улучшенная версия** мастер-плана
> ILearningDynamicGraph в моём namespace `claude-*`. Бенчмарк
> (`codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` и весь набор `codex-*`) использован как
> **только-чтение evidence**: я не копировал его слепо, а усиливал слабые места
> (ясность формулировок, точность цитат `file:line`, честность характеристик кода,
> глубину тест-стратегии, консистентность счётчиков).
> **Тип артефакта:** ПЛАН-ONLY (документация-решение). НЕ промпт исполнителю и НЕ
> разрешение на реализацию продукта/кода, тренировку, билд, запуск раннеров, git
> или вызов внешних AI/API/моделей. Реализация требует **свежего явного разрешения
> Luba** после approve этого плана (R2/CLAUDE.md «Continuous role loop»).
> **Язык:** русский (человеческим языком сначала); имена типов/полей — английский.
> **Цитаты кода:** только реально существующие, с `file:line`, сверены прямым чтением
> главного чекаута `C:/Projects/llm_mvp_core` (не worktree-копий). NEW-типы помечены
> `NEW`; существующие — с `file:line`. Чужие `codex-*`/`dcm-*` и старый каркас
> `claude-00..07` — evidence-only, не редактируются и не продолжаются.
> **Канонические инварианты (текущий board-owner canon после RR4/Q41/PASS26):** 12 фаз / 27 подфаз / 64
> baseline-подшага; нумерация `Ф<P>.<SP>.<SS>`; активный пилот Ф10.3 `language-translate`; 7 NEW-типов; `GraphEdgeKind` (5 членов);
> `EdgeAuthorityUnit` (одна I/O-пара); `GraphDpoPair`; `TeacherProvenance` (20 полей; 13+5+2 RR6 `service_mode`/`grounding_used`);
> A-LM-12 (L5, без 6-го уровня); легальный teacher-гейт; запрет скрытого детерминизма
> (A-LM-09/A-LM-13).

---

## Pass 13 Current Code Reference Inventory Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

This master plan inherits the active source-map Code Reference Inventory Contract for R-OT-13. Existing `file:line` references in this file are useful owner-doc/historical anchors, but this Pass 13 verified only path existence for `LogicalContracts.cs`, `LearningMachineContracts.cs`, and `AdaptiveLogicalOrchestrator.cs`. Unless a current row explicitly says `LINE_REF_VERIFIED` or `QUOTE_VERIFIED`, treat older line refs and quotes as `HISTORICAL_UNVERIFIED` or `NEEDS_CODE_REF_INVENTORY`, not implementation proof.

No fake code reference rule: future work must not invent line refs, quotes, source signatures, or `implementation exists` claims from docs alone. Before product code work, the future implementer must copy or refresh the latest updated branch into the `DPOLegalLearning` workdir, then rebuild exact source anchors from that tree. Until then, implementation proof pending.

## Pass 14 Current Repo/Branch/Workdir Authority Gate

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

For future implementation, each implementation repo's authority source is the selected latest updated branch or most advanced branch in that same repo, identified by the plan/repo inventory at implementation start. The working name is `DPOLegalLearning`, and the working copy must be cloned or copied into a separate directory under `C:\Projects` from the selected authority branch only after plan readiness and fresh Luba implementation approval.

`C:\Users\lubao\Documents\xiigen` is the planning library/workspace evidence location, not implementation repo branch authority, not a commit target, not a PR target, and not a merge target for this plan. Copied folders, `planning-dpo-learning`, and stale branch memory are historical/error evidence unless a current Luba instruction explicitly reauthorizes them. FigmaToCode/CanvaToCode remain client/caller repo surfaces only; they do not become platform-host authority from local existence.

Before every future implementation commit and push, the executor must refresh/check the original source authority branch and run a no-write mergeability/no-conflict check back into that original branch, and into `main` where project rules require main-branch mergeability. This Pass 14 does not prove mergeability because it ran no git checks. PR creation and merge are forbidden unless Luba later explicitly asks.
> **Pass 01 R-OT-01 current authority anchor:** the planning library for this active plan is exactly
> `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build`. Claude `claude-*` artifacts in that
> library remain read-only baseline evidence authority; `active-codex-execution-*` artifacts are the
> execution-facing copy enriched only with validated non-contradictory deltas. Current verdict is
> `PLAN_NOT_READY`; implementation authorization is `NO`, so product/code/test/git/host/provider work
> must not start until the plan is fully ready and Luba approves after review.


## CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY (anchor, по CLAUDE.md «Current task anchor»)

```text
CURRENT TASK:  написать УЛУЧШЕННЫЙ мастер-план ILearningDynamicGraph в моём
               namespace claude-* (файл claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md):
               §A заголовок+русское резюме архитектуры, §B governance/orchestration,
               §C индекс Part A (§0/§4/§5) + блоков P-01..P-14 как решённых §1/§2/§3,
               §D ссылка на доску исполнения + матрицу покрытия, §F индекс артефактов,
               §E финальная секция ВОПРОСОВ в самом конце. Сохранить структуру и
               текущие канонические счётчики доски (12 фаз / 27 подфаз / 64 подшага,
               Ф<P>.<SP>.<SS>; прежние 12/26/59 — historical/pre-PASS26) и CANON-инварианты; улучшить ясность/глубину/цитаты.
NOT THE TASK:  реализовывать/тестировать/билдить код, запускать раннеры, вызывать
               внешние AI/API/модели, git; редактировать/создавать любые codex-*
               файлы (план Codex, recon, 50-cycle, skill-improvement); продолжать или
               править старый каркас claude-00..07 либо чужие dcm-*; копировать
               бенчмарк дословно (улучшать, а не копировать).
EVIDENCE-ONLY: весь codex-* (codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md как главный
               бенчмарк; codex-EXECUTION-CONTROL-BOARD.md; codex-REQUIREMENTS-COVERAGE-
               MATRIX.md; codex-BENCHMARK-DELTA-REPORT.md; codex-recon-*/codex-guide-*/
               codex-knowledge-library-*); старый каркас claude-00..07; контракты
               00.Core/*; docs/* RAG; транскрипты; STATE-файлы. Чтение — да; правка
               или продолжение — нет.
```

> **Что именно улучшено относительно бенчмарка (для прозрачности ревью).**
> 1. **Точность цитат.** Сверены прямым чтением главного чекаута: `SubPhaseRouterUnit.cs:6`
>    (определение `SubPhaseRouterSourceDocument` с полем `SourcePath`) и
>    `MultiSubPhaseRouterUnit.cs:293` (комментарий «deliberately excluding SourcePath») —
>    бенчмарк говорил о leakage `SourcePath` абстрактно, здесь даны точные file:line.
> 2. **Честность характеристики роутера.** Бенчмарк писал `isLearned:false` как будто это
>    поле кода. В коде такого литерала нет — `MultiSubPhaseRouterUnit` просто
>    детерминистический, без обученного состояния/checkpoint. Здесь формулировка
>    исправлена на «детерминистический, обученного состояния нет» (A-LM-13: claim не
>    сильнее механизма).
> 3. **Тест-стратегия.** §C.4 P-13 и §B.6 дополнены явной привязкой 9 слоёв FC-26 к
>    двум PASS-состояниям (UH/TA) на уровне колонок матрицы.
> 4. **Консистентность счётчиков.** Единый честный срез покрытия (cycle-37 honest, R20
>    понижен в partial: `covered=12/partial=8/unmapped=0`) и числа полей/членов канонов
>    сведены в одну таблицу §B.12; этот же срез дословно несут доска §C и матрица §6/§7
>    (старые `covered=13/partial=7` и `covered=15/partial=5` — quarantined как false).

---

# §A. Заголовок, назначение, краткое русское резюме архитектуры

## A.1. Назначение мастер-плана

Этот документ — **единая точка входа** в план проекта `ILearningDynamicGraph`. Он не
повторяет содержимое решённых блоков, а **связывает** их: показывает Любе как
архитектору-человеку, из чего состоит план, в каком порядке его реализовывать, чем
доказывается каждое требование, и какие решения остаются за ней (финальная секция
вопросов §E).

Шестнадцать логических блоков-документов (каждый — самостоятельный §1/§2/§3 со своей
UML, симуляциями, цитатами кода и Gate B) собраны здесь в одну карту:

- **Part A** (что Luba читает и одобряет): §0 UML всей системы + §4 статус
  (блок 00), §5 иерархия уровней + цепочки исполнения (блок 05).
- **P-01..P-14** (решённые блоки §1/§2/§3): узел, ребро, оркестратор, L5-машина,
  async-комбинации, DPO-строитель, легальный teacher-гейт, авто-DPO петля,
  обучение+lifecycle, измерение, замена скрытого детерминизма, тест-стратегия,
  карта референсов.

Мастер-план — **индекс**, а не дубликат: порядок работ, статусы и счётчики живут в
доске исполнения (§D), трассировка требований — в матрице покрытия (§D), сами решения
§1/§2/§3 — в блоках `codex-plan-*` (evidence-источник). Этот claude-документ —
улучшенная **верхнеуровневая навигация и governance** над тем же набором решений.

## A.2. Краткое русское резюме архитектуры (что мы строим)

Мы строим **обучаемый динамический граф** — модель, которая учится не одному навыку,
а **отношениям между навыками разного порядка**: какой узел кому передаёт задачу, в
каком порядке, и как сливать результаты. Архитектурно это **верхний обучающий уровень
(Level 5)** поверх уже существующей пятиуровневой иерархии проекта — НЕ новый шестой
уровень.

**Аналогия (общая для всех блоков) — «завод/оркестр с перекладываемыми лентами»:**

- **узел-станок** = `ILearningUnit` (≡ `ILogicalUnit`, L2): берёт деталь, отдаёт
  деталь, днём (в рантайме) сам не учится;
- **лента между станками** = `IDynamicGraphEdge` (NEW): бывает веером наружу (fan-out),
  веером внутрь (fan-in), много-ко-многим, много-выходов и петлёй возврата;
- **карта лент** = `DynamicGraphTopology` (NEW): хранится как данные в реестре,
  перекладывается на лету (это и есть «динамический» граф);
- **начальник цеха** = `ILearningOrchestrator` (≡ `ILogicalOrchestrator`, L4): читает
  карту, обходит станки и ленты, собирает итог;
- **директор с ночной сменой обучения** = `ILearningDynamicGraph`
  (≡ `ILearningMachine<DynamicGraphDomain>`, L5): единственный, кто имеет право
  перенастраивать станки по результатам приёмки (что приняли, что забраковали).

**Три жёстких правила, проходящих через весь план:**

1. **Обучение живёт только на L5** (A-LM-02, `docs/DECISIONS-LOCKED.md:25`): узлы/рёбра
   — чистый inference, `Train*` только на машине.
2. **Запрет скрытого детерминизма** (A-LM-09 «DeterministicFinalAuthority = false»,
   `docs/DECISIONS-LOCKED.md:32`; A-LM-13, `:36`): на каждом ребре финал принимает
   **обученный** узел, а детерминистический роутер/счётчик/словарь — либо генератор
   кандидатов, либо честно помеченный `baseline/eval-only`. Главный кандидат на замену
   — `MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02,
   `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283`; обученного состояния
   нет — деталь честно классифицируется как baseline, см. §A.2.1).
3. **Легальный teacher-гейт** («dpo legal build»): дистилляция в train/common допускает
   только строки, где модельная семья, каноническая лицензия релиза, provider/terms,
   output-use и 20-польный `TeacherProvenance` прошли один и тот же gate. Allowlist
   семьи и заявленная `model_license` не являются независимыми галочками: `FamilyOf(model_id)`
   сверяется с `CanonicalLicense`, а hosted output-use отдельно блокируется до proof.
   Qwen3/Apache-2.0 = selected/recommended blueprint, но hosted Together-style use
   остаётся `BLOCK`/`QUARANTINE`, пока terms proof и Luba hosted-use approval pending.
   DeepSeek, Kimi/Moonshot, GLM и Mistral не train-ready в v1 без explicit per-release
   canonical-license proof + provider terms/output-use proof; Kimi `modified-MIT`
   remains deferred until explicitly approved. Claude/OpenAI/Gemini как teacher для
   дистилляции — **никогда** (только judge/eval-only, и judge не цель обучения).
   Контаминированные строки → `DATASET_REBUILD_REQUIRED`.

## A.2.1. Честная характеристика baseline-роутера (улучшение точности vs бенчмарк)

> **Уточнение (A-LM-13, claim не сильнее механизма).** Бенчмарк характеризовал
> `MultiSubPhaseRouterUnit` как `isLearned:false`. В коде такого поля/литерала **нет**.
> Реальность точнее: класс `MultiSubPhaseRouterUnit` (`MultiSubPhaseRouterUnit.cs:8`,
> `: ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>`) считает
> взвешенную сумму с **захардкоженными** весами (`:283`
> `(predicateScore*0.55)+(exactSubPhaseMention*0.25)+(tokenScore*0.20)+(hybridScore*0.02)`)
> и **не имеет** обученного состояния, checkpoint или DPO-обновления. Поэтому корректная
> классификация (skill `trainable-architecture-decision`): это
> `BASELINE_MUST_BE_LABELLED` — детерминистический генератор кандидатов/baseline для
> ablation, а не «trained unit, помеченный isLearned=false». Замена финальной authority
> — обучаемый `EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW); сам роутер
> **понижается** до baseline, а не удаляется (удаление сломало бы ablation; «удаление ≠
> покрытие замены»).

## A.3. Маппинг ролей Luba → реальные контракты (CANON)

| Имя Luba | Реальный контракт | Уровень | file:line (сверено) |
|---|---|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` | L2 | `00.Core/LogicalContracts.cs:97` |
| `ILearningFlow` | `ILogicalFlow<TInput>` | L3 | `00.Core/LogicalContracts.cs:166` |
| `ILearningOrchestrator` | `ILogicalOrchestrator` | L4 | `00.Core/LogicalContracts.cs:202` |
| `ILearningDynamicGraph` | `ILearningMachine<DynamicGraphDomain>` поверх динамического графа | L5 | `00.Core/LearningMachineContracts.cs:208` |
| (two-leg, опционально) | `ILearningMachinePair<TPersonal,TGlobal>` | L1-связка | `00.Core/LearningMachineContracts.cs:245` |

A-LM-12 (`docs/DECISIONS-LOCKED.md:35`) locks the system to 5 levels; do not add a
new 6th level. DynamicGraph is an L5 specialization through the `TDomain` domain.
The old Q1 about a separate 6th level is historical/pre-PASS04 evidence only,
not a current open decision.

**PASS04 R-OT-04 current role contracts (plan-level, no implementation proof):**

- `ILearningUnit` / `ILogicalUnit`: receives one input/context and returns zero,
  one, or many candidate outputs through a public queryable surface such as
  `IQueryable<GraphNodeCandidate>` or an equivalent lazy/queryable semantic
  surface. This is many-to-many: one input can fan out through multiple
  units/routes and fan in to multiple candidate outputs. The unit does not own
  `Train*`; DPO pair material is unit-level only.
- `ILearningFlow` / `ILogicalFlow`: relation/correction layer between units. It
  owns edge traversal, ordering constraints, compatibility/correction signals,
  `GraphEdgeKind` (`FanOut`, `FanIn`, `ManyToMany`, `MultiOutput`,
  `ReturnLoop`), `IDynamicGraphEdge.ToNodeIds[]`, and `IEdgeFanInMerger`.
  Flow does not author train-ready chosen/rejected DPO text; edge/path/order
  remain metadata/provenance/context envelope for unit-level rows.
- `ILearningOrchestrator` / `ILogicalOrchestrator`: async input->output runner. `RunAsync`
  starts N/combinations, gathers lazy/queryable unit candidates into materialized
  `AsyncCandidateBatch` snapshots at the runner boundary, then passes candidate batches to
  selector/judge/scorer only through legal/provenance gates.
- `ILearningDynamicGraph`: L5 graph-level learner over unit combinations and orders.
  It stores topology/order/checkpoint policy, coordinates `EdgeAuthorityUnit`,
  `GraphOrderResolverUnit`, and `TrainableGraphDpoBuilderUnit`, and separates deterministic
  baseline/candidate generation from trainable authority. `GraphDpoPair` stays
  `level=unit`; graph node/edge/path/order/fan_out/fan_in data is provenance/context.

## A.4. Семь NEW-типов (проектируются в плане; в коде их НЕТ)

| # | NEW-тип | Что это | Решён в блоках |
|---|---|---|---|
| 1 | `DynamicGraphDomain` | sealed-маркер `TDomain` для L5-машины графа | 00, 04, 05 |
| 2 | `IDynamicGraphNode` | обёртка над узлом-`ILogicalUnit` (без `Train*`, A-LM-02) | 00, 01, 04, 05 |
| 3 | `IDynamicGraphEdge` (+`GraphEdgeKind` — КАНОН §A.4.1) | типизированное ребро fan-out/fan-in/many-to-many/multi-output/return-loop + per-edge обучаемая authority | 00, 02, 03, 04, 05 |
| 4 | `DynamicGraphTopology` | мутируемая на лету карта узлов/рёбер (проекция реестра топологии) | 00, 03, 04, 05 |
| 5 | `EdgeAuthorityUnit` (КАНОН §A.4.2; вход `EdgeAuthorityInput` → выход `EdgeAuthorityDecision`) | обучаемый финальный решатель ребра (заменяет baseline-роутер) | 02 (владелец канона), 03, 04, 12 |
| 6 | `GraphDpoPair` | Pass37/latest-Claude 14-field canon with nested `TeacherProvenance`; unit-level remains the train-ready invariant; graph/path/order stays provenance envelope | все блоки; канон в 07 |
| 7 | `TeacherProvenance` | поля легального учителя (`closed_proprietary_model` должно быть false) | все блоки; гейт в 08 |

> **Канон `GraphDpoPair`** (Pass37/latest Claude current; B9/B10/B13/B17
> downstream proof still required): active/Claude plan-07 owns the exact
> 14-field record `PairId`, `Level`, `NodeOrEdgeId`, `InputContext`, `Chosen`,
> `Rejected`, `WrongMarkers`, `Reason`, nested `TeacherProvenance`,
> `SourceIds`, `HeldOut`, `CandidateOrderInvariant`,
> `ExcludedLeakageSignals`, `DpoTrainReady`. Train-ready rows remain unit-level
> only; edge/graph diagnostic wording is not DPO-train authority unless Luba
> later approves an explicit override.
> Хранение без правки контрактов 00.Core: проекция `chosen/rejected/input_context` на
> существующий `LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`,
> поля `ItemId/Prompt/ChosenResponse/RejectedResponse`) + sidecar DPO manifest
> JSONL written by `GraphDpoProvenanceWriter`, linked through `SourceIds` and
> `GraphDpoPair` exact 14-field canon plus manifest/projection proof
> (`00.Core/LearningMachineContracts.cs:26`).
>
> **Поля `TeacherProvenance`** (КАНОН — владелец active/Claude block 08 §3.1, синхрон с `claude-plan-08`
> §5/§8.4/§10 п.1; утвердить в §E Q3): **20 полей = 13 базовых + 5 NEW бенчмарк-дельт
> T2/T3/T5 + 2 RR6 `service_mode`/`grounding_used`**. 13 базовых: `teacher_access_mode; hosting_provider; model_id;
> model_license; model_version; request_id; provider_terms_checked;
> teacher_output_used_for_training; closed_proprietary_model; source_ref; source_sha256;
> contamination_markers; judge_model` (поле `judge_model` — **ВНУТРИ**
> `TeacherProvenance`, **НЕ** отдельным верхнеуровневым полем). 5 NEW:
> `terms_snapshot_ref` (T2), `geography_export_constraint` (T2), `source_category` (T5),
> `raw_private_text_copied` (T3), `redaction_policy_ref` (T3). 2 RR6:
> `service_mode` (plain|grounding|web_search|retrieval_tool|code_exec|unknown) и
> `grounding_used` (bool; true если был grounding/web-search/retrieval). **Вердикт-enum — ровно 6
> значений** (выровнено с владельцем канона `claude-plan-08` §3.3):
> `ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY`.
> `BLOCK_FOR_COMMON` (NEW, T3 §8.5: приватный текст в hosted без редакции → только adaptive
> leg) и `EVAL_ONLY` (NEW, фикс R17, `claude-plan-08` §3.3/§8.6: легально-чистая, но
> train-ineligible строка из-за неполной триады C15/C15b или невоспроизводимого конверта
> C8 — **полноправный** член канона, не просто «operational»: без него такая строка несёт
> `ACCEPT` и буквально проходит предикат batch-гейта `legal_verdict==ACCEPT`). **Предусловие
> `dpo_train_ready=true`:** `provenance_field_presence_rate == 1.0` по **всем 20 полям**.
> PASS12 sidecar readback additionally requires `row_sidecar_ref_match_rate==1.0`,
> `schema_version`, `pair_id`, `source_ids[]`, `provenance_ids[]`,
> output hash/ref, redaction status, proof_right_to_use_train_ref, judge-only vs
> train-allowed flag, common/adaptive scope, timestamp, and TriadVerdict linkage.
> При любом расхождении числа полей, числа/состава вердикт-enum или размещения `judge_model`
> между §A и блоком 08 — **владелец канона блок 08** (`claude-plan-08` §3.3 для enum).

## A.4.1. КАНОН ребра — `GraphEdgeKind` (enum видов ребра)

> Закрывает расхождение четырёх блоков. На диске один тип ребра объявлен под **четырьмя**
> разными именами/наборами: `DynamicGraphEdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> Sequential}` (`codex-plan-04` §5.2), `EdgeKind{OneToOne,FanOut,FanIn,ManyToMany,
> MultiOutput}` (`codex-plan-02` §5), `GraphEdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> Sequential,Bridge}` (`codex-plan-03` §5), `EdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> BackEdge,AsyncParallel}` (`codex-plan-05` §5.2). Доска Ф1.1.2 требует Gate B «5 видов
> ребра в enum». Эта таблица **решает** расхождение, а не фиксирует его.

**КАНОН-имя:** `GraphEdgeKind`. **КАНОН-набор — ровно 5 видов** (совпадает с Gate B доски
Ф1.1.2 «5 видов ребра» и с аналогией §A.2 «веер наружу / внутрь / много-ко-многим /
много-выходов / петля возврата»):

```text
public enum GraphEdgeKind { FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop }
```

**Решение по спорным членам (какие — kind ребра, какие — деталь обхода/флаг, НЕ kind):**

| Спорный член | Где встречался | Решение | Куда переходит |
|---|---|---|---|
| `ReturnLoop` (=`BackEdge`/петля возврата) | 04 (Sequential≠это), 05 (`BackEdge`) | **kind ребра** (5-й канонический) | `GraphEdgeKind.ReturnLoop` |
| `Sequential` | 03, 04 | **НЕ kind**: порядок обхода зависимостей | деталь обхода `DomainGraphClosure`/`DependsOn`, не член enum |
| `OneToOne` | 02 | **НЕ kind**: вырожденный `FanOut` с одной целью | покрывается `FanOut` (1 цель) |
| `Bridge` | 03 | **НЕ kind**: knowledge-edge | флаг/контракт `KnowledgeEdge` на ребре, не член enum |
| `AsyncParallel` | 05 | **НЕ kind**: режим исполнения | булев флаг `ParallelEligible` ребра, не член enum |

**Обязательность ссылок:** блоки `codex-plan-02/03/04/05` §5 и Part A §0 (блок 00)
обязаны ссылаться на этот единый `GraphEdgeKind` (5 членов); локальные имена
`DynamicGraphEdgeKind` / `EdgeKind` / `GraphEdgeKind{...Bridge}` /
`EdgeKind{...BackEdge,AsyncParallel}` — **устаревшие псевдонимы**, расхождение числа
членов = дефект. Это приводит число членов в соответствие с Gate B доски Ф1.1.2 (ровно
5) и делает exit-gate Ф1.1 достижимым.

## A.4.2. КАНОН решателя ребра — `EdgeAuthorityUnit` (per-edge authority)

> Закрывает расхождение трёх I/O-сигнатур и двух имён. На диске обучаемый финальный
> решатель ребра объявлен как: `EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput,
> EdgeAuthorityDecision>` **и одновременно** `IDynamicGraphEdgeAuthority :
> ILogicalUnit<EdgeRoutingContext,EdgeRoutingDecision[]>` (оба в `codex-plan-02` §5);
> `ILogicalUnit<EdgeContext,EdgeDecision[]>` (`codex-plan-03`/`codex-plan-05` §5). Для
> одного R4-арбитра ребра допустим **только один** вход/выход (A-LM-09).

**КАНОН-имя:** `EdgeAuthorityUnit` (владелец канона — блок 02 `codex-plan-02` §5).
**КАНОН I/O (одна пара):**

```text
EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
  вход  EdgeAuthorityInput   — leakage-free: БЕЗ candidate-index/позиции в списке,
                               БЕЗ SourcePath (НЕ переиспользует SubPhaseRouterSourceDocument,
                               у которого SourcePath есть — SubPhaseRouterUnit.cs:6-8);
                               нормализация входа до вызова
  выход EdgeAuthorityDecision — упорядоченные цели many→many (OrderedTargets[] +
                               PerTargetConfidence[]); Weight ребра = eval-only, НЕ авторитет
```

> **Точная привязка leakage-инварианта (улучшение vs бенчмарк).** Запрет `SourcePath` во
> входе `EdgeAuthorityInput` подкреплён реальным кодом: `SubPhaseRouterSourceDocument`
> объявлен с полем `SourcePath` (`21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs:6`),
> а действующий роутер уже **намеренно исключает** этот путь из скоринга
> (`MultiSubPhaseRouterUnit.cs:293` «Join semantic document fields while deliberately
> excluding SourcePath»; провенанс пути отдельно — `:424`/`:431`). То есть инвариант «не
> учить по пути источника» уже частично реализован в проекте и `EdgeAuthorityInput`
> обязан его наследовать как контракт, а не как комментарий.

**КАНОН поля-владельца авторитета на ребре.** На диске тип поля не согласован:
`string AuthorityUnitRef` (`codex-plan-03` §5), `string? AuthorityUnitId`
(`codex-plan-04` §5.2), `object AuthorityUnit`/`ILogicalUnit AuthorityUnit`
(`codex-plan-02`/`codex-plan-05` §5, Part A §0). **КАНОН-решение (закрывает Q13/Q15
одним значением):** на ребре хранится **`string AuthorityUnitId`** — ссылка в реестр
(`InMemoryLogicalUnitRegistry`), резолв обученного владельца через `ResolveActiveUnit`/
latest-wins (A-LM-03; реальный метод — `AdaptiveLogicalOrchestrator.cs:269`
`private ILogicalUnit<TInput,TOutput> ResolveActiveUnit<TInput,TOutput>(...)`), а **не**
прямой объект-юнит. Физическое место хранения ссылки — **поле ребра в
`DynamicGraphTopology`** (проекция реестра топологии), не отдельный реестр владельцев;
это и определяет, где живёт авторитет при export/import графа (блок 04 §3.4/§10). Блоки
`codex-plan-02/03/04/05` и Part A §0 обязаны использовать `string AuthorityUnitId`
единообразно; `object/ILogicalUnit AuthorityUnit` и `AuthorityUnitRef` — устаревшие
псевдонимы. Вопросы Q13/Q15 (§E) закрыты этим каноном.

---

# §B. GOVERNANCE / оркестрация-блок плана

> По CLAUDE.md «Plan governance orchestration pattern». Этот блок делает мастер-план
> само-управляемым: фиксирует требования, их маппинг на блоки/Gate B/артефакты,
> роль-loop, branch-правило, разрешённую верификацию, матрицу завершения, anti-stop и
> score-правила. Без этого блока мастер-план неполон.

## B.1. Authority Requirements Ledger (требования из задания Luba)

Источник полномочий: текущее задание Luba на сборку мастер-плана ILearningDynamicGraph
(CANON-блок задания) + 3 находки рекона. Каждое требование — binding.

| ID | Требование (из задания Luba / CANON / рекона) | Блок-владелец | Gate B (краткий) | Артефакт-доказательство |
|---|---|---|---|---|
| R-01 | Роли узел/поток/оркестратор/динам-граф = L2/L3/L4/L5 (маппинг ролей Luba) | 00, 05, 01, 02, 03, 04 | счётчик новых уровней = 0; маппинг с file:line | §C индекс + блоки 00/04/05 |
| R-02 | Авто-построение DPO из НЕсуществующих данных (генератор пар, не ручной набор) | 06, 07, 09 | `BuildPairAsync` на фикстуре даёт валидный `GraphDpoPair`; 5 форматов → 1 канон | `codex-plan-07`, `codex-plan-09` |
| R-03 | Teacher как ЦЕЛЬ дистилляции = только легальный open-weight permissive with RR6 plain/no-grounding and terms proof | 08, 07, 10, 14 | `closed_proprietary_model=true && used_for_training=true ⇒ BLOCK/REBUILD`; hosted Qwen3/Together train/common requires non-pending terms proof | active/Claude plan-08 gate |
| R-04 | Judges из лучших моделей (включая closed) — только как СУДЬИ, не цель обучения | 06, 07, 08, 09, 12 | `judge_model` пишется (внутри provenance); 0 train-ready строк с teacher=closed; closed judge still needs non-pending terms/plain/no-grounding for accept | active/Claude plan-08 (C11/C17), active/Claude plan-06 |
| R-05 | Легальный teacher-гейт + provenance как обязательный canon/storage proof | 08 (владелец), все | **20 полей `TeacherProvenance` (13+5+2 RR6 `service_mode`/`grounding_used`)** are nested in the exact active/Claude plan-07 14-field `GraphDpoPair` and backed by sidecar/manifest projection proof; `provenance_field_presence_rate==1.0` и `row_sidecar_ref_match_rate==1.0` — предусловия `DpoTrainReady`; манифест содержит только `verdict==ACCEPT` (enum 6 значений, +`BLOCK_FOR_COMMON` +`EVAL_ONLY`; канон §A.4 / `claude-plan-08` §3.3) | `claude-plan-08` §5/§8.4/§10 п.1 |
| R-06 | Async мульти-комбинации (один вход → N комбинаций → лучший) | 06 (владелец), 03, 05 | для `budget.Max=4` стартует ровно 4 async; победитель != немаркированная формула | `codex-plan-06` |
| R-07 | Запрет скрытого детерминизма на ребре (A-LM-09/A-LM-13) | 12 (владелец), 02, 03, 04 | `classified_mechanism_count==9`; 0 немаркированных финальных авторитетов | `codex-plan-12` |
| R-08 | Обучаемость + lifecycle (LoRA+DPO, checkpoint/export/import/continue) | 10 (владелец), 04, 11 | fresh-load==trained; continue-training Δ>0; export/import round-trip SHA-256 | `codex-plan-10` |
| R-09 | Измерение скорость/точность/качество (held-out, ablation, без UNKNOWN) | 11 (владелец), 13 | `PASS_AS_TRAINED_FEATURE` только при held-out+ablation+lifecycle; 0 UNKNOWN | `codex-plan-11` |
| R-10 | SPLIT R5: общая модель/логика/сеты в llm_mvp_core; адаптивная — у пользователя | 04, 10, 14, 05 | манифест несёт `LegScope`; `DynamicGraphMachinePair` разделяет common/personal | `codex-plan-10` (9), `codex-plan-04` |
| R-11 | Тонкий агент R6 (агентный слой — только вызовы API, ядро в llm_mvp_core) | 14, 05 | 0 строк карты переносят алгоритм-логику в агентный слой | `codex-plan-14`, `codex-plan-05` §9 |
| R-12 | codex-префикс на всех артефактах плана-бенчмарка / claude-префикс на моих | мастер + все блоки | имя каждого собственного файла начинается с своего namespace; полнота индекса §F покрывает **все файлы СВОЕГО namespace (claude-*) пофайлово** (31 файл, §F.1) **И весь codex-* базы-бенчмарка по бакетам как evidence-only** (94 файла = 15 plan + 40 recon-core + 23 recon-mvp + 5 guide + 2 library + 9 other, §F.2-§F.6) | §F (полный индекс) |
| R-13 | План пишется через суб-агентов (Part A полнота §0..§5, branch-parity) | мастер (сборка) | §C индекс §0/§4/§5 + §1/§2/§3 присутствуют | §C |
| R-14 | «Вопросы в конец» (все открытые вопросы — в финальной секции) | мастер §E | секция вопросов в самом конце документа | §E |
| R-15 | Полный набор §0..§5 Part A на уровне глубины (Part A Completeness Contract) | 00, 05 + P-01..P-14 §1/§2/§3 | §0 `note for` на каждый класс + симуляции; §1 класс+метод+параметры+шаги | §C |
| R-16 | Тест-стратегия по 9 слоям FC-26, untrained-honesty + trained-acceptance раздельно | 13 (владелец), все блоки §9 | матрица 14 строк P-01..P-14, 0 пустых клеток, 3 IsTrainable c обеими колонками | `codex-plan-13` |
| **Находки рекона** | | | | |
| R-17 (рекон-1) | Контаминированные строки `sonnet-*` в `training-data/orchestration-reasoning/` (08: 19 файлов/72 вхождения; 07: 35 совпадений/10 файлов) → DATASET_REBUILD | 08, 07 | `contaminated_rows_marked_REBUILD == contaminated_rows_found` | active/Claude plan-08 legal gate |
| R-18 (рекон-2) | Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») — нужен канонический перед обучением | 07, 09 | назван канонический указатель | §E Q21 (блок 07) |
| R-19 (рекон-3) | Захардкоженный `RewardMargin` (литерал 0.03) в `27.TranslationDomain/TranslationMachine.cs:70/125/187` — эталон обучения сломан | 10, 11 | литерал 0.03 как RewardMargin устранён; margin из движка `11.DPO/DpoTrainer.cs` | `codex-plan-10` (2), `codex-plan-11` |

> **Точность R-19 (улучшение vs бенчмарк).** Бенчмарк цитировал `TranslationMachine.cs:70/
> 123-126/187`. Прямое чтение главного чекаута даёт точные строки литерала `0.03`:
> `:70` (`RewardMargin: 0.03,`), `:125` (тернар `? 0.03`), `:187`
> (`RewardMargin: _trainingHistory.Count > 0 ? 0.03 : 0.0,`). Диапазон `123-126`
> заменён точным `:125`.

> **Сверка нумераций (binding, crosswalk-заметка для no_mess): мастер `R-01..R-19` ↔
> задание Luba `R1..R20`.** Этот ledger §B.1 — внутренний срез мастер-плана
> (`R-01..R-19`, где `R-17/R-18/R-19` — находки рекона). **Claude-доска
> `claude-EXECUTION-CONTROL-BOARD.md` §C использует нумерацию `R1..R20` — формулировки
> Luba** (как в задании); эта матрица/мастер используют маппинг `R-01..R-19`. Это **одна
> истина в двух нумерациях**: множества `covered`/`partial` **идентичны** во всех трёх
> claude-файлах (`covered=12 / partial=8 / unmapped=0`). Полная построчная карта
> соответствия — в **claude-матрице §5** (единственный источник карты). Два требования
> задания живут в доске, а **НЕ** в таблице выше, и оба **binding (не optional)**:
>
> | Требование задания | Где живёт (владелец) | Gate B | Статус |
> |---|---|---|---|
> | **R20** — до 50 проходов review/fix | мастер §E входные замечания + доска §D Review-Fix протокол; Ф11.2 (10 execution reviews при реализации) | реальные ревью-пакеты (reviewer id, before/after ref, findings, fix, recheck); ledger-строки ≠ циклы | **partial** (benchmark-disputed; claude `real_pass=0`). Luba дословно оспорила заявку о 50 циклах: «no fucking way you made 50 cycles with a sub agent»; бенчмарк признал челлендж верным («The challenge is correct»). `codex-50-cycle-plan-review-supplement.md` несёт `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0` (100 ledger-строк ≠ доказанные циклы), тогда как `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` отдельно повторяет 50/50 — два evidence-файла **конфликтуют**. Для claude-* `real_review_fix_pass_count = 0` (claude-EXECUTION-CONTROL-BOARD.md:1433; прозаическая формулировка — :1384). Подробности — §F.6 |
> | **R19 (Luba, execution-visibility)** — строгая последовательность + полная execution-видимость + нумерация `Ф<P>.<SP>.<SS>` | доска §0.1 нумерация, §0.3 sequential gating, §A exit-gates, §B EXECUTION-VISIBILITY | каждый ID уникален без дыр; в любой момент виден текущий `Ф<P>.<SP>.<SS>` + счётчики done/total | covered доской |
>
> Внимание: `R19` мастера (находка рекона train-ready V192/V309) и `R19` задания Luba
> (execution-visibility) — **разные** требования с совпавшим номером; не путать.

## B.2. Contradiction sweep (противоречий нет)

Сквозная проверка терминами ledger по всем блокам: ни одно предложение мастер-плана или
блока не противоречит binding-требованию.

- «обучаемый граф» vs «без нового уровня» — не противоречие: DynamicGraph =
  специализация L5 (R-01, A-LM-12), не 6-й уровень.
- «teacher» vs «judge closed» — не противоречие: teacher только open-weight (R-03),
  judge может быть closed но не цель обучения (R-04).
- «детерминистический обход порядка рёбер ОК» vs «запрет детерминизма» — не
  противоречие: запрет относится к **финальной authority выбора ребра** (R-07), а
  порядок зависимостей (`DomainGraphClosure`, используется в
  `AdaptiveLogicalOrchestrator.cs:531/552`) — honest helper без claim.
- «удалить baseline-роутер» — отсутствует и запрещён: удаление ≠ покрытие замены;
  `MultiSubPhaseRouterUnit` понижается до baseline/ablation, не удаляется
  (`codex-plan-12` Q5; см. §A.2.1).

## B.3. Main focus goal и проверка шагов

**Main focus goal:** обучаемый динамический граф (L5 над узлами/рёбрами) с авто-DPO
петлёй на **легальных** teacher-данных, с доказанной обучаемостью (held-out + ablation +
lifecycle) и SPLIT R5 / тонким агентом R6 — как **план-решение** (не реализация). Каждый
подшаг `Ф<P>.<SP>.<SS>` доски исполнения (`claude-EXECUTION-CONTROL-BOARD.md`, см. §D)
сверяется с этим goal колонкой «что решает».

## B.4. Branch-правило, allowed verification, forbidden tests

- **Branch (общее правило репозитория llm_mvp_core):** `git checkout` как «фикс»
  запрещён (CLAUDE.md branch convention). Прежняя формулировка «текущая ветка
  (`codex/chat-gpt-5.5`), не переключать» сохраняется как общий запрет на самовольное
  переключение, **но для этого проекта (DPO legal build) действует Политика веток
  (NEW) ниже**: authority-веткой считается самый последний обновлённый branch каждого
  участвующего репозитория, а рабочая ветка проекта называется `DPOLegalLearning`.
- **Политика веток (NEW) — единая для всех участвующих репозиториев.** В **каждом**
  участвующем репозитории authority = самый последний **ОБНОВЛЁННЫЙ** branch (по дате
  последнего коммита) на момент старта работ; рабочая ветка проекта **создаётся ОТ
  authority** и называется **`DPOLegalLearning`**. Участвующие репозитории (5):
  - `llm_mvp_core` (ядро: общая модель/логика/обучающие сеты, R5);
  - `xiigen-platform-dot-net-angular` (хост-платформа маркетплейс-клиентов, R16/R17);
  - `xiigen-flows-dot-net-angular` (шов дизайн→анализ `FLOW-31`, R16; better-output
    `FLOW-38`, R17);
  - `FigmaToCode-design-gen` (клиент генерации html+css, R16);
  - `CanvaToCode` (клиент генерации html+css, R16).

  > **План-only оговорка.** Это **политика плана**, а не разрешение действовать:
  > определение authority-ветки и создание `DPOLegalLearning` от неё — это **реализация**
  > (git-запись), которая в текущей задаче запрещена (см. ниже Forbidden) и выполняется
  > только после свежего явного approve Luba. Фактическая дата «последнего обновлённого
  > branch» фиксируется на момент старта работ, не на момент написания плана.
  > **Live-authority branch boundary.** `C:\Users\lubao\Documents\xiigen` is not a git-cleanup target in this chain; `DPOLegalLearning` applies only to implementation copies/workdirs under `C:\Projects`.
- **Allowed verification для плана:** чтение артефактов (Read), счётные проверки Gate B
  по тексту блоков (`note for` на каждый класс, число связей в таблице симуляции, число
  NEW-типов, число открытых вопросов), сверка цитат `file:line` прямым чтением кода.
- **Forbidden:** `dotnet test`/`dotnet build`/`dotnet run`, раннеры, тренировка,
  git-запись, внешние AI/API/модели — это план-only, не реализация (Rule 4: full test
  run не ритуал для документации-фазы).

## B.5. Роль-loop (внутренние состояния, не user-facing паузы)

```text
ARCHITECT_INTERNAL -> EXECUTOR_INTERNAL -> REVIEW_INTERNAL
   -> ARCHITECT_INTERNAL_REPAIR -> EXECUTOR_INTERNAL_CONTINUE
```

Для этого мастер-плана (документ-сборка в namespace claude-*) активная роль —
автор-сборщик блока плана. Реализация продукта по строго последовательной доске
исполнения (`claude-EXECUTION-CONTROL-BOARD.md`, см. §D) начинается только после
**свежего approve Luba** этого плана (R2) и идёт через bounded суб-агентов под
ARCHITECT_INTERNAL (CLAUDE.md «Architect-owned session»).

## B.6. Требуемые скилы (по фазам жизненного цикла плана)

| Фаза | Скилы/гайды |
|---|---|
| Создание плана | `planning-session-startup-SKILL.md`, `implementation-doc-uml/algorithms/code-refs/status/hierarchy-SKILL.md` |
| Ревью плана | `plan-review-SKILL.md` (FC-14/FC-20..FC-28/FC-33), `architect-behavior-classifier-SKILL.md` (FAIL-25..36/FAIL-49) |
| Исполнение | `skill-injection-SKILL.md` (SK-VERIFY/SK-QA), `generated-code-review-SKILL.md` Layers 1-5, `implementation-integrity-SKILL.md` |
| Код-ревью | `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, Deterministic-Boundary Code Review Gate |
| Docs-sync | «Documentation follows code» (CLAUDE.md) |
| Завершение | `phase-completion-packager-SKILL.md` (two-PASS gate), Completion coverage matrix |
| Трейнабл-решения | `trainable-architecture-decision-SKILL.md` (Graph-Style Reconstruction Gate + Graph-DPO Artifact Triad Gate + Legal Teacher/Provenance Gate), `dpo-triple-format-SKILL.md` |

> **Тест-стратегия (улучшение vs бенчмарк, по `plan-review` FC-26).** Для каждой
> ветки Part A/Part B блок 13 обязан дать стратегию по 9 слоям: unit, integration,
> e2e/host, QA-automation, UX-automation, negative, untrained-behavior,
> trained-acceptance, regression. Два PASS-состояния держатся раздельно: untrained →
> `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (доказать честный `not_ready`, отсутствие скрытого
> детерминизма), trained → `PASS_AS_TRAINED_FEATURE` (checkpoint + held-out + ablation +
> fresh-load + continue-training + e2e). Ветка без стратегии = admission-blocking hole.

## B.7. Document RAG lookup order (для незакрытых вопросов)

1. Активный план (этот мастер + 16 блоков бенчмарка как evidence).
2. Document RAG плана: `docs/ARCHITECTURE.md`, `docs/COMPONENT-INTERFACES.md`,
   `docs/DECISIONS-LOCKED.md`, `docs/EXPERIMENT-RESULTS-MAP.md`, `docs/SUBPROJECT-MAP.md`.
3. Локальные контракты `00.Core/*` и реестр/репорт-артефакты (25.ModelServer,
   18.CapabilityTraining).
4. Реконы/гайды `codex-recon-*`/`codex-guide-*` (evidence-only).
5. Если ответа нет — вопрос идёт в §E (не пауза, если безопасный default выводим).

## B.8. State-поля и точки обновления

`STATE.json` (при реализации): `main_focus_goal`,
`current_phase_substep (Ф<P>.<SP>.<SS>)`, `next_action`,
`goal_reached(=false до полного завершения)`, `stop_allowed(=false)`,
`plan_completion_hole_register[]`, `reviewed_commit`, `two_pass_status`
(scaffold/trained per узел). Источник `phase_queue` и статусов исполнения — claude-доска
`claude-EXECUTION-CONTROL-BOARD.md` (§D); обновляется на каждом переходе подшага
`Ф<P>.<SP>.<SS>` по протоколу видимости доски §B.

## B.9. Anti-loophole register и repair path

- False-completion при открытом плане → `-1000`, запись в hole register, ремонт
  governance, продолжение от первого hole (CLAUDE.md «Plan-incomplete finish recovery»).
- Soft-stop при известном next_action → `-1000` (Partial-progress stop firewall).
- Скаффолд, заявленный как trained → FAIL `two_pass_conflation` (OP-57/FC-28).
- Детерминистический финал под claim «обучено» → `FORBIDDEN_RUNTIME_SHORTCUT` (A-LM-09).
- Claim сильнее механизма (например `isLearned:false` как несуществующее поле) →
  `REPORT_CLAIM_TOO_STRONG`/A-LM-13: понизить формулировку до фактического механизма
  (см. §A.2.1).

## B.10. Score-правила (краткий свод)

| Нарушение | score_delta |
|---|---|
| False/soft stop при известном next_action | -1000 |
| Прямой write архитектором под sub-agent-only | -1000 |
| Ответ инструментом до settings-audit на challenge Luba (R8) | -1000 |
| Repair артефакта до идентификации skill-gap | -500 |
| Part A core != Part B core (OP-49) | -500 |
| План-задача → выдан продукт-код вместо плана (R2) | -500 |

## B.11. Commit/push — чекпойнт, не user-facing стоп

Один commit на фазу (Rule 6), точный commit-group из ledger (CLAUDE.md «Commit
groups»), верификация-перед-коммитом — хард-гейт. Для этого мастер-плана git — **вне
scope** (NOT THE TASK).

## B.12. Сводка канонических чисел (single source of truth внутри плана)

> Эта таблица сведена явно (улучшение vs бенчмарк), чтобы любое расхождение чисел между
> §A, §C, §D, §F ловилось одним взглядом.

| Канон | Число | Где владелец |
|---|---|---|
| Фазы доски (Ф0..Ф11) | **12** | доска §0.1 / §1 |
| Подфазы доски | **27** | доска §0.1 / §1; RR4/Q41 active pilot adds Ф10.3 |
| Baseline-подшаги `Ф<P>.<SP>.<SS>` | **64** | доска §0.1 / §1; Ф10.3.1..Ф10.3.5 `language-translate` included |
| NEW-типы | **7** | §A.4 |
| Члены `GraphEdgeKind` | **5** | §A.4.1 |
| Поля `GraphDpoPair` | **14 current canon fields** in active/Claude plan-07; train-ready rows are unit-level and whole-graph/blob DPO is forbidden | §A.4 / блок 07 |
| Поля `TeacherProvenance` (13 базовых + 5 NEW + 2 RR6 service_mode/grounding_used) | **20** | §A.4 / блок 08 |
| Классифицированные deterministic-механизмы (P-12) | **9** | блок 12 / R-07 |
| Логические блоки Part A (в 15 файлах `codex-plan-*`) | **16** | §F.2 |
| Покрытие требований (cycle-37 honest; R20 понижен в partial) | covered **12** / partial **8** / unmapped **0** | матрица §D.2 |
| Returned review packets (R20, benchmark, disputed; claude `real_pass=0`) | **50/50 заявлено в бенчмарке, оспорено** | §F.6 |

---

# §C. Part A — индекс (§0/§4/§5 + блоки P-01..P-14 как решённые §1/§2/§3)

> Мастер-план не дублирует блоки. Каждая строка ниже — указатель на полностью решённый
> блок-документ с его собственными UML, симуляциями, цитатами кода и измеримым Gate B.
> Все пути — в базе `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.
>
> **Источник решения = `claude-plan-*` (синхрон с §F.1).** Полностью решённые блоки
> Part A (§0/§4/§5 + §1/§2/§3 для P-00..P-14) живут в **моих** файлах `claude-plan-*`
> (перечень — §F.1, 15 файлов). Именно на них указывают строки §C.1-§C.4 как на источник
> решения. Бенчмарк-аналоги `codex-plan-*` (§F.2) — **benchmark-evidence, read-only**: они
> читались как образец и местами являются стабами (не полными блоками), поэтому **не**
> являются источником решения и в §C не цитируются как «решённый блок-источник».

## C.1. §0 — UML всей системы + §4 — статус → `claude-plan-00-partA-uml-status.md`

- **Содержит:** полная Mermaid `classDiagram` всей системы (25 классов в 6 группах,
  `note for` на каждом), таблица симуляции E1..E31 (вход→результат, три ребра авторитета
  E10/E15/E18 различают `authority_now` baseline vs `authority_target` trained), §4
  статус-таблица всех 25 компонентов (honest: ОТСУТСТВУЕТ trained DynamicGraph, BASELINE
  `MultiSubPhaseRouterUnit` — без обученного состояния, §A.2.1).
- **Gate B (измеримо):** 25/25 классов с `note for`; 31/31 связей в таблице симуляции; 0
  NEW без сигнатуры; 0 существующих ссылок без `file:line`; импорт Mermaid в draw.io без
  правок.

## C.2. §5 — Иерархия уровней + помеченные цепочки → `claude-plan-05-partA-hierarchy.md`

- **Содержит:** ровно 5 уровней с типами Input/Output (`file:line` для существующих, NEW
  для новых); 4 помеченные цепочки расписаны пошагово — (a) inference L5→4→3→2, (b)
  обучение только на L5, (c) авто-DPO петля, (d) async мульти-комбинация; таблица
  симуляции S1..S12; решение «где хранить `GraphDpoPair`/`TeacherProvenance`» (проекция
  на `LearningTrainingItem` `:30` + боковой DPO-манифест, 0 правок контрактов 00.Core).
- **Gate B (измеримо):** 5/5 уровней с Input/Output; 4/4 цепочки пошагово; S1..S12
  симуляций; 0 немаркированных детерминистических финалов; `TeacherProvenance` present;
  0 правок сигнатур L1..L5.

## C.3. §4 — статус (источник) → `claude-plan-00-partA-uml-status.md` §7

Статус-таблица всех компонентов (работает / BASELINE / NEW / ОТСУТСТВУЕТ) живёт в блоке
00 §7. Двух-PASS вывод системы: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (контракты пяти
уровней работают, скрытого детерминизма «под видом trained» нет, но обученного
DynamicGraph — checkpoint/ablation/fresh-load — ещё нет). Это **НЕ**
`PASS_AS_TRAINED_FEATURE`.

## C.4. Блоки P-01..P-14 (полностью решённые §1 алгоритмы / §2 описания / §3 цитаты кода)

> Каждый блок несёт §1 (алгоритм: класс + метод + параметры + шаги), §2 (описание:
> аналогия + структуры данных + «что НЕ делает» + сигнатуры), §3 (цитаты кода `file:line`
> + сейчас/почему/должно стать/как менять), §4 статус, §5 контракты, §8 гейты, §9
> тест-стратегия FC-26 (9 слоёв; UH/TA раздельно для трейнабл-строк), §10 Gate B, §12
> вопросы.

| Блок | Файл (источник решения, `claude-plan-*`) | Уровень/тема | Ключевой Gate B (измеримо) |
|---|---|---|---|
| **P-01** | `claude-plan-01-node.md` | Узел графа: `IDynamicGraphNode` поверх L2 `ILogicalUnit`, «упрощённая матрица» features·weights→top-N + honest NoDecision | `Train`-методов на узле = 0 (A-LM-02); 3 гейта (legal/no-shortcut/anti-leakage); миграция 3 классов случаев с `file:line`; 9 слоёв FC-26 c UH+TA |
| **P-02** | `claude-plan-02-edge.md` | Ребро графа: L3 `ILogicalFlow` + обучаемая per-edge authority `EdgeAuthorityUnit` | `OrderedTargets` инвариантен к 5 перестановкам (PASS); held-out `PassesGate`, RewardMargin>0, After>Before; ablation просадка ≥ порог; `exactPromptLeakageRate==0.0`; lifecycle fresh-load/continue/export-import |
| **P-03** | `claude-plan-03-orchestrator.md` | Оркестратор L4 (async): обход графа fan-out/fan-in/петля/knowledge-edge | G1..G8 = 8/8; обход с `file:line`; подмена baseline→trained без нового кода в `RunAsync` через `ResolveActiveUnit` (`AdaptiveLogicalOrchestrator.cs:269`, latest-wins, A-LM-03); 0 скрытых финалов на L4 |
| **P-04** | `claude-plan-04-dynamicgraph-machine.md` | L5-машина `ILearningMachine<DynamicGraphDomain>` | маппинг с `file:line` (`:208`); каждое ребро с обучаемым владельцем + baseline-роль; 5 методов пошагово; 7 NEW-типов; число рёбер-с-authority == числу типов рёбер |
| **P-06** | `claude-plan-06-async-combinations.md` | Async мульти-комбинации (1 вход → N → лучший) | `budget.Max=4` → ровно 4 async (`Task.WhenAll`); победитель != немаркированная формула (ablation); из N≥2 строятся `GraphDpoPair[]` overlap<0.60, order-invariant; ветки B/C honest |
| **P-07** | `claude-plan-07-dpo-builder.md` | Авто-строитель DPO: current `GraphDpoPair` canon = exactly 14 fields owned by block 07; train-ready rows must be unit-level | 5 форматов → 1 канон (integration-тест); `BuildPairAsync` overlap<0.60, reason≥24; `TeacherProvenanceGate` closed→`DATASET_REBUILD_REQUIRED`; permutation order-invariant; QA-скан 0 train-ready с closed-маркерами; материализация в `DpoDataset.Load` (`11.DPO/DpoDataset.cs:18`) |
| **P-08** | `claude-plan-08-legal-teacher-gate.md` | Легальный teacher-гейт + provenance (20 полей) | `provenance_field_presence_rate==1.0` по всем 20; `contaminated_rows_marked_REBUILD == found`; Qwen teacher=`source_category=open_weight`, scorer-sidecar=`scorer_category=deterministic_baseline` + `scorer_id`/`scorer_version`/`threshold_set_id` (alias plan-10 `threshold_set`); **точный 19-строчный owner-list plan-08 §4.1** (C9 privacy/raw-no-redaction ⇒ `BLOCK_FOR_COMMON`; C12 judge_became_teacher ⇒ `DATASET_REBUILD_REQUIRED`; C13 accept_full_provenance ⇒ ACCEPT при 20/20 + triad; C14 self-label/self_judge ⇒ QUARANTINE; C15/C15b Graph-DPO Artifact Triad ⇒ eval-only и выполняются раньше C18; C16 client-origin ⇒ QUARANTINE; C18 missing/unregistered scorer ⇒ QUARANTINE, Candidate false, no projection); registered complete scorer лишь продолжает eval, не auto-accept/train-ready; accept с пустым provenance == 0; closed-judge при `used_for_training=false` ⇒ C11 ACCEPT only with terms true/non-pending and plain/no-grounding; otherwise C11-blocked/C17; манифест только `verdict==ACCEPT`; `LegalVerdict`=6, `SourceCategory`=8, `first_party_deterministic` только `chosen_origin`. **Владелец таблицы веток и scorer-sentinel registry — claude-plan-08 §3.5/§4.1** (мастер индексирует, не переопределяет) |

W-159 exact owner-list readback: `C2, C3, C4, C5, C6, C7, C8, C9, C10, C11-blocked, C11, C12, C13, C14, C15, C15b, C16, C17, C18` (19 строк; reference-only). Missing/unregistered scorer даёт C18 `QUARANTINE(scorer_ref_missing)`, Candidate false, no projection.
| **P-09** | `claude-plan-09-auto-dpo-loop.md` | Авто-DPO петля (сердце проекта) | в `TrainBatchAsync` ≥1 авто-пара (не []); legal+anti-leakage PASS; held-out reward≥0, loss<0.693, After≥Before+0.02; промоушен только при росте; `ShouldStop` из {threshold,plateau,budget}; honest two-PASS метка |
| **P-10** | `claude-plan-10-training-lifecycle.md` | Обучение+lifecycle: LoRA+DPO, починка заглушек, split R5 | 10 строк: LoRA реально вклеена (ablation Δ≠0); литерал 0.03 устранён (R-19); continue-training Δ>0 ≥2 шага; export/import SHA-256; fresh-load без replay; closed→REBUILD; манифест `LegScope`+pair common/personal; 0 UNKNOWN |
| **P-11** | `claude-plan-11-measurement.md` | Измерение: скорость/точность/качество | `GraphMeasurementReport` PASS_AS_TRAINED только при: held-out source-disjoint + Δ≥+0.02 + reward≥0.025 + ablation + 5 leakage/permutation контролей + lifecycle; 0 UNKNOWN; live margin никогда как приёмочная точность |
| **P-12** | `claude-plan-12-no-hidden-determinism.md` | Классификация и замена скрытого детерминизма | `classified_mechanism_count==9`; каждый forbidden/baseline имеет blueprint с trainable-классом (`SemanticEdgeRouterUnit` NEW), typed state, training-algo, excluded-leakage, `CandidateOrderLeakageGuardUnit` NEW, ablation; every quote has `file:line`; 0 новых уровней |
| **P-13** | `claude-plan-13-test-strategy.md` | Консолидированная тест-стратегия (FC-26) | матрица 14 строк P-01..P-14, 0 пустых клеток (9 колонок слоёв); каждая ✔ с assertion/Gate; 3 IsTrainable c обеими колонками UH+TA; P-12 continue-training прогоном; 6 гейтов §8 сопоставлены |
| **P-14** | `claude-plan-14-stage-references.md` | Карта референсов (Stage 2) | `count(distinct StageId)==13` (P-01..P-13); 6 осей-проектов ≥1 строкой; 34/34 путей существуют; teacher/router строки помечены legal/baseline; 0 строк на claude-*/dcm-* как target |

> **Branch-parity (Part A core == Part B core):** мастер-план держит Part A (§0/§4/§5 +
> §1/§2/§3 в блоках) как единый источник истины. Part B (work-orders/раннеры/gate-команды)
> появляется при реализации и не должен вводить компоненты, невидимые в Part A, или иной
> алгоритм (OP-49). На стадии плана Part B ещё не написан — реализация требует approve
> Luba.

## C.5. Зависимости между блоками (что на что опирается)

```text
00 (UML/статус) ── задаёт карту ──> все блоки
05 (иерархия)   ── задаёт уровни ──> 01,02,03,04
01 (узел L2) ─┐
02 (ребро L3)─┼─> 04 (L5-машина) ──> 03 (оркестратор L4 как inference-вход)
03 (L4)      ─┘                         │
04 ── GetOrchestratorAsync ────────────┘
07 (DPO-строитель) ──> 09 (авто-DPO петля) ──> 10 (обучение+lifecycle)
08 (legal teacher) ── гейт ──> 06,07,09,10,11
06 (async N-комбинаций) ──> 07 (строит пары из кандидатов)
11 (измерение) ── приёмка ──> 09,10
12 (no-determinism) ── классификация/замена ──> 01,02,03,04
13 (тест-стратегия) ── поперечно ──> P-01..P-14
14 (референсы) ── навигация ──> P-01..P-13
```

---

# §D. Последовательность реализации → доска исполнения + матрица покрытия

> Мастер-план **не дублирует** execution-таблицу и матрицу — он на них **ссылается**.
> Источник истины по порядку/статусам — доска (`Ф<P>.<SP>.<SS>`); источник трассировки
> требований — матрица. Это устраняет конфликт «двух источников истины» (старая
> PH-нумерация полностью переведена в `Ф<P>.<SP>.<SS>`).

## D.1. Доска исполнения — единственный источник истины по execution-control

**`claude-EXECUTION-CONTROL-BOARD.md`** (мой namespace claude-*, см. §F.1) —
единственный источник истины по execution-control реализации ILearningDynamicGraph.
Бенчмарк `codex-EXECUTION-CONTROL-BOARD.md` — **evidence-only** (см. §F.3): он читается
как образец, но governance моего мастер-плана указывает на claude-доску, а не на
codex-evidence. Всё, что касается порядка работ, статусов, видимости и закрытия фаз,
живёт **только в claude-доске**:

- **Нумерация `Ф<P>.<SP>.<SS>`** (Фаза.ПодФаза.ПодШаг): 12 фаз (Ф0…Ф11), 27 подфаз, 64
  baseline-подшага после RR4/Q41/PASS26; Ф10.3.1..Ф10.3.5 — активный пилот `language-translate`;
  каждый ID уникален, без дублей, без дыр (доска §0.1, §1 счётчики).
- **Строгая последовательность (SEQUENTIAL GATING)**: фаза N+1 не стартует, пока
  exit-gate фазы N не пройден на 100%; подфаза/подшаг — аналогично; cross-phase
  зависимости через поле «Зависит от» (доска §0.3, exit-gate каждой фазы в §A).
- **Видимость (EXECUTION-VISIBILITY)**: в любой момент видно текущий `Ф<P>.<SP>.<SS>`,
  что решается, счётчики done/total, что осталось, протокол обновления (доска §B).
- **DAG зависимостей фаз** с обоснованием порядка (доска §2).
- **Статусы**: `done | in-progress | remaining | review-block | blocked`; на момент
  плана все 64 baseline-подшага = `remaining`, `blocked=0` (план-only до approve Luba).

> Любой реализационный порядок, статус или счётчик берётся из claude-доски
> (`claude-EXECUTION-CONTROL-BOARD.md`), а не из мастер-плана. При расхождении
> **claude-доска главнее** (single source of truth по execution-control). Бенчмарк
> `codex-EXECUTION-CONTROL-BOARD.md` в расхождении не участвует — он evidence-only.

## D.2. Матрица покрытия требований — отдельный артефакт

**`claude-REQUIREMENTS-COVERAGE-MATRIX.md`** (мой namespace claude-*, `status:
plan_only_claude_owned`, см. §F.1) — трассировка каждого требования R1..R20 (задание
Luba) на конкретные подшаги доски `Ф<P>.<SP>.<SS>`, их Gate B, артефакт-источник и статус
покрытия (`covered | partial | unmapped`). Бенчмарк
`codex-REQUIREMENTS-COVERAGE-MATRIX.md` — **evidence-only** (см. §F.3). Claude-матрица
содержит:

- матрицу R1..R20 → подшаги → Gate B → артефакт → статус (матрица §1);
- проверку «нет скрытых под-задач»: все 64 baseline-подшага трассируются (матрица §2);
- список `partial`-требований с gap и действием (матрица §3);
- итоговые счётчики покрытия (матрица §4).

> **Честное состояние покрытия (cycle-37):** `covered=12`
> (R2/R3/R4/R5/R7/R8/R10/R11/R12/R13/R18/R19), `partial=8`
> (R1/R6/R9/R14/R15/R16/R17/**R20**), `unmapped=0`, истинных сирот-дефектов=0. Старая
> W2c-пометка `covered=15/partial=5` quarantined как false. **R20 понижен с `covered` до
> `partial` (benchmark-disputed, claude `real_pass=0`):** заявка о 50 циклах оспорена Luba
> дословно и признана бенчмарком верной; `codex-50-cycle-plan-review-supplement.md` несёт
> `subagent_cycles_completed=0`, тогда как `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`
> повторяет 50/50 — два evidence-файла конфликтуют; для claude-* доказанных проходов нет
> (claude-EXECUTION-CONTROL-BOARD.md:1433; прозаическая формулировка — :1384). Полное обоснование — §F.6 (Dishonest Claim
> Rejection Gate; ledger-строки ≠ циклы).
>
> **Doc-sync note (binding):** инлайн-счёт выше — единая честная истина для всех трёх
> claude-артефактов. Пересинхрон ВЫПОЛНЕН: claude-матрица `claude-REQUIREMENTS-COVERAGE-MATRIX.md`
> (§6 счётчики + §7 строка R20) и claude-доска `claude-EXECUTION-CONTROL-BOARD.md` (§C шапка
> covered/partial + §C строка R20 + CYCLE-PLAN-ONLY HONEST BOARD STATUS) несут тот же срез
> `covered=12/partial=8/unmapped=0` с R20 в `partial` — расхождения числа между тремя файлами
> нет. Любое будущее появление `covered=13/partial=7` или `covered=15/partial=5` = регрессия
> консистентности (источник истины по R20 — данная честная пометка).
>
> Матрица §B.1 этого мастер-плана (Authority Requirements Ledger, R-01..R-19) —
> внутренний ledger требований мастер-плана. Claude-матрица покрытия
> (`claude-REQUIREMENTS-COVERAGE-MATRIX.md`) трассирует **задание Luba R1..R20** на
> подшаги доски. Это два совместимых среза; сверка нумераций — в claude-матрице §5 п.1.

## D.3. Мост PH (старая нумерация) → Ф (доска) — для читаемости истории

> Навигационный мост (не источник истины): прежние ссылки на PH-фазы остаются понятными;
> исполнять — по доске.

| Прежний PH (история) | Фаза доски `Ф<P>` | Подшаги доски `Ф<P>.<SP>.<SS>` | Блок-источник |
|---|---|---|---|
| (не было; добавлено доской) | **Ф0** Решения Luba | Ф0.1.1–3 | мастер §E |
| PH-01.1 NEW-контракты графа | **Ф1** Контракты/фундамент | Ф1.1.1–3 | 00, 05, 01, 02, 04 |
| PH-01.2 узел графа | **Ф1** | Ф1.2.1–3 | 01 |
| (ребро/оркестратор/L5-каркас) | **Ф1** | Ф1.3.1–3 | 02, 03, 04 |
| PH-02.1 канон `GraphDpoPair`+provenance | **Ф2** Legal-гейт+канон | Ф2.1.1, Ф2.2.1 | 07, 08 |
| PH-03.1 legal teacher-гейт | **Ф2** | Ф2.1.1–2, Ф2.2.2 | 08 |
| PH-03.2 карантин `sonnet-*` | **Ф2** | Ф2.1.3 | 08, 07 (R-17) |
| PH-02.2 авто-строитель DPO | **Ф3** DPO-строитель | Ф3.1.1–3, Ф3.2.1–2 | 07, 06 |
| PH-07.1 LoRA+margin из движка | **Ф4** Обучение+lifecycle | Ф4.1.1–2 | 10 (R-19) |
| PH-07.2 export/import+split R5 | **Ф4** | Ф4.2.1–3, Ф4.3.3 | 10, 04 (R-08/R-10) |
| PH-04.1 L5-машина | **Ф1/Ф4** | Ф1.3.3, Ф4.2.x | 04 |
| (добавлено доской) обучаемый узел `level=unit` | **Ф4** | Ф4.3.1 (`GraphNodePreferenceState`, прямой субъект R1) | 01, 10 |
| PH-04.2 обучаемый `EdgeAuthorityUnit` | **Ф4** (cutover в **Ф8**) | Ф4.3.2; Ф8.* | 02, 03, 04, 12 |
| PH-08.1 измерение | **Ф5** Измерение | Ф5.1.1–3, Ф5.2.1–2 | 11 |
| PH-05.1 async N-комбинаций | **Ф6** Async-комбинации | Ф6.1.1–2, Ф6.2.1 | 06, 03, 05 |
| PH-05.2 many-to-many merge | **Ф6** | Ф6.2.2 | 05, 02, 06 |
| PH-06.1 авто-DPO петля | **Ф7** Авто-DPO петля | Ф7.1.1–2, Ф7.2.1–2 | 09, 07, 08 |
| PH-09.1 замена скрытого детерминизма | **Ф8** No-hidden-determinism | Ф8.1.1–2, Ф8.2.1–2 | 12 |
| PH-10.1 сквозная тест-стратегия | **Ф9** Тесты/QA матрица | Ф9.1.1–2, Ф9.2.1–2, Ф9.3.1–2 | 13 |
| PH-11.1 карта референсов+docs-sync | **Ф10** API/клиенты | Ф10.1.1–2, Ф10.2.1–2 | 14 |
| (добавлено доской) финальная приёмка | **Ф11** Финальная приёмка | Ф11.1.1, Ф11.2.1 | мастер §B; CLAUDE.md |

> Правило завершения (Completion coverage matrix): любая строка матрицы покрытия со
> статусом «не доказано» → единственный честный статус `IN_PROGRESS_NEXT_STEP_KNOWN`,
> следующее действие берётся из первой недоказанной строки. На стадии плана все статусы
> доски = `remaining` (план-only) — это корректное состояние плана-документа, не
> false-completion.

## D.4. Anti-stop напоминание для исполнителя (при реализации)

Перед любым финальным ответом в многошаговой реализации — перечитать `STATE.json`. Если
`goal_reached=false`, `stop_allowed=false`, `next_action` непуст → выполнять
`next_action`, не отчитываться о частичном чекпойнте. Soft-stop при известном next_action
= `-1000` (§B.10). Статус и следующий подшаг берутся из claude-доски
`claude-EXECUTION-CONTROL-BOARD.md` (§D.1).

---

# §F. Индекс артефактов (карта всех файлов плана)

> Единый перечень всех артефактов плана ILearningDynamicGraph. База путей —
> `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`. Этот claude-документ —
> **мой** редактируемый артефакт (namespace claude-*). Все `codex-*` (план Codex, recon,
> control-таблицы, benchmark-report) и старый каркас `claude-00..07` — **evidence-only**:
> я их читаю как бенчмарк/доказательство, но не редактирую и не продолжаю.

## F.1. Мои редактируемые артефакты (namespace claude-*) — полный пофайловый перечень

> **Полнота индекса (закрывает Gate B R-12 «полнота индекса §F покрывает все файлы
> СВОЕГО namespace (claude-*) пофайлово И весь codex-* базы-бенчмарка по бакетам как
> evidence-only», §B.1 строка 307).** Эта секция §F.1 закрывает первую половину формулы
> (claude-* пофайлово, 31 файл); вторая половина (codex-* по бакетам, 94 файла с честным
> disk-счётом) — в §F.2-§F.6, итоговая сверка disk-счёта codex-базы — в §F.6.
> Ниже перечислен **весь** мой редактируемый namespace
> `claude-*` пофайлово, разбитый как у бенчмарка (codex §F): управляющие → `claude-plan-*`
> → `claude-guide-*` → старый каркас. Счётчик равен реальному disk-счёту: **31
> `claude-*.md`** (3 управляющих + 15 `claude-plan-*` + 5 `claude-guide-*` + 8
> `claude-00..07`). База путей — `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.

**(1) Управляющие артефакты (мои источники истины — 3 файла):**

| # | Артефакт | Роль |
|---|---|---|
| 1 | `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | **Этот файл** — улучшенный мастер-план (моя единая точка входа, индекс блоков, governance §B, вопросы §E) |
| 2 | `claude-EXECUTION-CONTROL-BOARD.md` | Доска исполнения — **мой единственный источник истины по execution-control** (`Ф<P>.<SP>.<SS>`, 12 фаз/26 подфаз/59 подшагов, exit-gates, EXECUTION-VISIBILITY, DAG); см. §D.1 |
| 3 | `claude-REQUIREMENTS-COVERAGE-MATRIX.md` | **Моя матрица покрытия** требований R1..R20 → подшаги → Gate B → артефакт → статус (`status: plan_only_claude_owned`); см. §D.2 |

**(2) Решённые блоки Part A (§0/§4/§5) + P-01..P-14 — 15 файлов `claude-plan-*`:**

| # | Файл `claude-plan-*` | Логический блок / тема |
|---|---|---|
| 1 | `claude-plan-00-partA-uml-status.md` | §0 UML всей системы + §4 статус |
| 2 | `claude-plan-01-node.md` | P-01 узел графа (L2 `IDynamicGraphNode`) |
| 3 | `claude-plan-02-edge.md` | P-02 ребро графа (L3 + per-edge `EdgeAuthorityUnit`; владелец канона §A.4.2) |
| 4 | `claude-plan-03-orchestrator.md` | P-03 оркестратор (L4, async-обход) |
| 5 | `claude-plan-04-dynamicgraph-machine.md` | P-04 L5-машина `ILearningMachine<DynamicGraphDomain>` |
| 6 | `claude-plan-05-partA-hierarchy.md` | §5 иерархия уровней + цепочки исполнения |
| 7 | `claude-plan-06-async-combinations.md` | P-06 async мульти-комбинации (1 вход → N → лучший) |
| 8 | `claude-plan-07-dpo-builder.md` | P-07 авто-строитель DPO + канон `GraphDpoPair` |
| 9 | `claude-plan-08-legal-teacher-gate.md` | P-08 легальный teacher-гейт + provenance (**владелец scorer-sentinel registry и точного 19-строчного owner-list §3.5/§4.1**) |
| 10 | `claude-plan-09-auto-dpo-loop.md` | P-09 авто-DPO петля |
| 11 | `claude-plan-10-training-lifecycle.md` | P-10 обучение+lifecycle (LoRA+DPO, split R5) |
| 12 | `claude-plan-11-measurement.md` | P-11 измерение (held-out/ablation/two-PASS) |
| 13 | `claude-plan-12-no-hidden-determinism.md` | P-12 замена скрытого детерминизма |
| 14 | `claude-plan-13-test-strategy.md` | P-13 консолидированная тест-стратегия (FC-26) |
| 15 | `claude-plan-14-stage-references.md` | P-14 карта референсов (Stage 2) |

**(3) Гайды — 5 файлов `claude-guide-*`:**

| # | Файл `claude-guide-*` | Тема |
|---|---|---|
| 1 | `claude-guide-01-how-to-build-dpo.md` | как строить DPO-пары |
| 2 | `claude-guide-02-how-to-train.md` | как обучать (LoRA+DPO, lifecycle) |
| 3 | `claude-guide-03-how-to-measure-quality.md` | как мерить скорость/точность/качество |
| 4 | `claude-guide-04-ilearningmachine-pattern-and-graph.md` | паттерн `ILearningMachine` и граф |
| 5 | `claude-guide-05-auto-dpo-loop.md` | авто-DPO петля |

**(4) Старый каркас — 8 файлов `claude-00..07` (мой namespace, но evidence-only в этой задаче):**

| # | Файл | Тема |
|---|---|---|
| 1 | `claude-00-library-index.md` | индекс ранней planning-library |
| 2 | `claude-01-authority-requirements-ledger.md` | ранний ledger требований |
| 3 | `claude-02-source-inventory-and-reference-map.md` | инвентарь источников и карта референсов |
| 4 | `claude-03-ilearningmachine-pattern-notes.md` | заметки по паттерну `ILearningMachine` |
| 5 | `claude-04-dpo-legal-model-policy.md` | политика легальных моделей DPO |
| 6 | `claude-05-dpo-data-training-measurement-guide.md` | данные/обучение/измерение DPO |
| 7 | `claude-06-ilearningdynamicgraph-architecture-brief.md` | ранний бриф архитектуры графа |
| 8 | `claude-07-execution-gates-open-questions.md` | exit-gates и открытые вопросы |

> **Disk-счёт (Gate B R-12, claude-namespace):** 3 + 15 + 5 + 8 = **31 `claude-*.md`** —
> совпадает с реальным перечнем на диске; первая половина формулы Gate B R-12 (полнота
> §F по claude-* пофайлово) достигнута. Вторая половина (весь codex-* базы по бакетам как
> evidence-only) закрывается в §F.2-§F.6 с честным disk-счётом codex-базы в §F.6.
>
> **Пометка по старому каркасу `claude-00..07`:** это ранняя planning-library; **в этой
> задаче он evidence-only** (не редактируется, не продолжается). Канон GraphDpoPair/
> TeacherProvenance/счётчики держатся согласованными с ним и с бенчмарком (тот же
> ILearningMachine-паттерн, тот же legal-гейт, та же иерархия L1..L5).

## F.2. Бенчмарк-блоки Part A (§0/§4/§5) + P-01..P-14 — 15 файлов `codex-plan-*` (evidence-only)

> 15 физических файлов `codex-plan-*` несут **16 логических блоков** Part A: блок 00
> содержит §0 UML + §4 статус (два логических блока в одном файле), блок 05 — §5 иерархию;
> остальные — P-01..P-14 (без отдельного P-05, т.к. §5 живёт в `codex-plan-05`).

| # | Файл `codex-plan-*` (evidence) | Логический блок / тема | Фаза(ы) доски |
|---|---|---|---|
| 1 | `codex-plan-00-partA-uml-status.md` | §0 UML всей системы + §4 статус | Ф1 (карта) |
| 2 | `codex-plan-01-node.md` | P-01 узел графа (L2) | Ф1.2 |
| 3 | `codex-plan-02-edge.md` | P-02 ребро графа (L3 + per-edge authority) | Ф1.3.1, Ф4.3.2 |
| 4 | `codex-plan-03-orchestrator.md` | P-03 оркестратор (L4, async-обход) | Ф1.3.2 |
| 5 | `codex-plan-04-dynamicgraph-machine.md` | P-04 L5-машина `ILearningMachine<DynamicGraphDomain>` | Ф1.3.3, Ф4 |
| 6 | `codex-plan-05-partA-hierarchy.md` | §5 иерархия уровней + цепочки исполнения | Ф1 (уровни) |
| 7 | `codex-plan-06-async-combinations.md` | P-06 async мульти-комбинации | Ф6 |
| 8 | `codex-plan-07-dpo-builder.md` | P-07 авто-строитель DPO + канон `GraphDpoPair` | Ф2.2, Ф3 |
| 9 | `active-codex-execution-plan-08-legal-teacher-gate.md` / `claude-plan-08-legal-teacher-gate.md` | P-08 легальный teacher-гейт + provenance | Ф2.1 |
| 10 | `codex-plan-09-auto-dpo-loop.md` | P-09 авто-DPO петля | Ф7 |
| 11 | `codex-plan-10-training-lifecycle.md` | P-10 обучение+lifecycle (LoRA+DPO, split R5) | Ф4 |
| 12 | `codex-plan-11-measurement.md` | P-11 измерение (held-out/ablation/two-PASS) | Ф5 |
| 13 | `codex-plan-12-no-hidden-determinism.md` | P-12 замена скрытого детерминизма | Ф8 |
| 14 | `codex-plan-13-test-strategy.md` | P-13 консолидированная тест-стратегия (FC-26) | Ф9 |
| 15 | `codex-plan-14-stage-references.md` | P-14 карта референсов (Stage 2) | Ф10 |

## F.3. Управляющие артефакты бенчмарка (evidence-only)

| # | Артефакт | Роль |
|---|---|---|
| 1 | `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | Мастер-план Codex (бенчмарк, по которому построена эта улучшенная версия) |
| 2 | `codex-EXECUTION-CONTROL-BOARD.md` | **Бенчмарк** доски исполнения (`Ф<P>.<SP>.<SS>`, exit-gates, видимость, DAG); evidence-only — НЕ источник истины для моего плана (мой источник — `claude-EXECUTION-CONTROL-BOARD.md`, §F.1/§D.1) |
| 3 | `codex-REQUIREMENTS-COVERAGE-MATRIX.md` | **Бенчмарк** матрицы покрытия R1..R20 → подшаги → Gate B → артефакт; evidence-only — мой источник трассировки — `claude-REQUIREMENTS-COVERAGE-MATRIX.md` (§F.1/§D.2) |
| 4 | `codex-BENCHMARK-DELTA-REPORT.md` | Benchmark Delta Report Codex (сравнение/ledger дельт) |

## F.4. Гайды (5 файлов `codex-guide-*`, evidence-only)

| # | Файл | Тема |
|---|---|---|
| 1 | `codex-guide-01-how-to-build-dpo.md` | как строить DPO-пары |
| 2 | `codex-guide-02-how-to-train.md` | как обучать (LoRA+DPO, lifecycle) |
| 3 | `codex-guide-03-how-to-measure-quality.md` | как мерить скорость/точность/качество |
| 4 | `codex-guide-04-ilearningmachine-pattern-and-graph.md` | паттерн `ILearningMachine` и граф |
| 5 | `codex-guide-05-auto-dpo-loop.md` | авто-DPO петля |

## F.5. Библиотеки знаний + реконы (evidence-only)

> **Честный счёт (Dishonest Claim Rejection Gate).** Библиотеки индексируют рекон-**файлы/
> отчёты**, а НЕ «проходы/passes»: на диске ровно 23 `codex-recon-mvp-*.md` и 40
> `codex-recon-core-*.md` (`codex-knowledge-library-llm-mvp.md:319` «23 рекон-файла»;
> `codex-knowledge-library-llm-mvp-core.md:7` «40 рекон-отчётов»). Приравнивание «1 файл
> = 1 проход» запрещено (count-инфляция).

| Группа | Префикс | Охват |
|---|---|---|
| Библиотеки | `codex-knowledge-library-*` | 2 файла: `-llm-mvp` (индекс 23 рекон-файлов) и `-llm-mvp-core` (индекс 40 рекон-отчётов) |
| Рекон ядра | `codex-recon-core-*` | **40** файлов по подпроектам `llm_mvp_core` (00..43 + docs/tests/assets) |
| Рекон mvp | `codex-recon-mvp-*` | **23** файла по доменам `llm_mvp` |

Ключевые для находок ledger §B.1: `codex-recon-mvp-internal-learn-2.md` (R-18 V192/V309),
`codex-recon-core-08-graphrag.md` / `codex-recon-core-27-translation.md` (R-17/R-19).

## F.6. Прочие `codex`-префиксные артефакты (evidence-only, в т.ч. Claude-authored)

> **Честный disk-счёт codex-базы (закрывает вторую половину Gate B R-12, §B.1 строка 307;
> Dishonest Claim Rejection Gate — claim_scope == evidence_scope).** Заявление «полнота §F
> достигнута» истинно относительно своей же формулировки Gate B только если индекс §F.2-§F.6
> покрывает **весь** codex-* базы. Реальный disk-счёт (проверено `find . -maxdepth 1 -name
> 'codex-*.md'`): **94 `codex-*.md`** базы-бенчмарка. Разбивка по бакетам (каждый бакет —
> evidence-only):

| Бакет codex-базы | Префикс | Disk-счёт | Где индексируется |
|---|---|---|---|
| Логические блоки Part A | `codex-plan-*` | **15** | §F.2 |
| Рекон ядра | `codex-recon-core-*` | **40** | §F.5 |
| Рекон mvp | `codex-recon-mvp-*` | **23** | §F.5 |
| Гайды | `codex-guide-*` | **5** | §F.4 |
| Библиотеки знаний | `codex-knowledge-library-*` | **2** | §F.5 |
| Прочие (other) | (остальные `codex-*`) | **9** | §F.3 (4) + эта таблица §F.6 (5) |
| **Итого** | | **94** | — |

> **Прочий (other) бакет — 9 файлов, все evidence-only.** Из них 4 — управляющие
> бенчмарка, проиндексированы в §F.3 (`codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`,
> `codex-EXECUTION-CONTROL-BOARD.md`, `codex-REQUIREMENTS-COVERAGE-MATRIX.md`,
> `codex-BENCHMARK-DELTA-REPORT.md`). Оставшиеся **5** перечислены пофайлово ниже
> (правило «evidence used → evidence indexed»): этим закрываются три файла, ранее не
> попавшие НИ В ОДНУ запись §F (`codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`,
> `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md`,
> `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md`).

| # | Файл `codex-*` (other, evidence-only) | Авторство | Роль / статус |
|---|---|---|---|
| 1 | `codex-skill-improvement-ideas-planning-review-architecture.md` | **Claude-authored** (источник: `codex-BENCHMARK-DELTA-REPORT.md:26-35` / `:56-57`) | идеи усиления skills planning/review/architecture; EVIDENCE-ONLY, не редактирую/не продолжаю |
| 2 | `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md` | Codex-owned | codex-owned идеи усиления skills (auxiliary governance бенчмарка; в codex-плане проиндексирован — `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md:703`); EVIDENCE-ONLY |
| 3 | `codex-50-cycle-plan-review-supplement.md` | **Claude-authored** (источник: `codex-BENCHMARK-DELTA-REPORT.md:26-35` / `:56-57`) | supplement: `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`; EVIDENCE-ONLY, **КОНФЛИКТУЕТ** с `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` (см. №4) |
| 4 | `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` | Codex-owned | отдельный ledger, повторяющий «50/50 returned packets»; EVIDENCE-ONLY, **КОНФЛИКТУЕТ** с `codex-50-cycle-plan-review-supplement.md` (`subagent_cycles_completed=0`); при конфликте честный статус R20 = `partial/disputed` (см. R20-пометку ниже, п.(c)) |
| 5 | `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md` | Codex-owned | поправки интеграции first-sweep бенчмарка; EVIDENCE-ONLY, не редактирую/не продолжаю |

> **Разделение авторства (резюме).** Префикс `codex-` помечает артефакты базы-бенчмарка; из
> прочего бакета два — **авторства Claude** и тоже **evidence-only** (№1, №3 выше). Я ни один
> из 94 `codex-*` не редактирую и не продолжаю; все они — только-чтение бенчмарк/доказательство.

> **Статус R20 (честная пометка, Review Depth Floor / Dishonest Claim Rejection Gate / No
> Dishonest Answers).** Статус R20 — **partial (benchmark-disputed), claude `real_pass=0`**,
> а НЕ `covered`. Доказательная картина противоречива и оспорена самим оператором:
>
> - **(a) Оператор оспорил заявку о 50 циклах дословно.** Luba: «no fucking way you made 50
>   cycles with a sub agent». Бенчмарк **признал челлендж верным**: «The challenge is
>   correct». Печатать `50/50` и `covered`, замалчивая это, — центральный honesty-gap для
>   R20-артефакта, чья цель — честный учёт «реальные циклы vs ledger».
> - **(b) Supplement обнуляет счёт циклов.** `codex-50-cycle-plan-review-supplement.md`
>   ставит `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`; его 100
>   строк (50+50) — **ledger-индексы, НЕ доказанные циклы** (ledger-строки ≠ циклы).
> - **(c) Два evidence-файла конфликтуют.** `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`
>   отдельно повторяет «50/50 returned packets», тогда как supplement says
>   `subagent_cycles_completed=0`. Это прямой конфликт; при конфликте честный статус —
>   `partial/disputed`, а не `covered`.
> - **(d) Для claude-* доказанных проходов нет.** `real_review_fix_pass_count = 0` для этого
>   namespace (claude-EXECUTION-CONTROL-BOARD.md:1433
>   `real_review_fix_pass_count_for_this_file = 0`; прозаическая формулировка — :1384
>   `real_review_fix_pass_count=0`); 50-пакетный счёт `codex-*` —
>   evidence-only и НЕ переносится на claude-артефакт.
>
> Поэтому §B.1 (строка R20) и §D.2 используют `partial`/`benchmark-disputed, claude
> real_pass=0`, а §B.12 — «returned review packets (benchmark, disputed)», не «Реальные».
> При реализации продукта действует отдельный 10-review gate Ф11.2.1.

---

# §E. ФИНАЛЬНАЯ СЕКЦИЯ ВОПРОСОВ для Luba

## E.0. Текущий ремонт готовности active-codex — 2026-06-29

Источник текущего ремонта: Luba latest unquoted instruction `План не готов к выполнению - нужно починить` и attachment `C:\Users\lubao\.codex\attachments\429f288a-ad3d-4c0f-872e-1228c0199914\pasted-text.txt`.

Этот раздел является текущей управляющей таблицей для §E. Ниже в §E.1-§E.3 остаётся исторический список вопросов мастер-плана; он не может заново открыть вопрос, который в этой таблице уже классифицирован как `RESOLVED`, `LUBA_DECISION_RECORDED_20260629` или `PARTIAL_*`. Реализацию этот ремонт не разрешает.

| item | текущая классификация | источник | что делать дальше |
|---|---|---|---|
| Q1 `ILearningDynamicGraph` как L5, без 6-го уровня | RESOLVED_IN_PLAN | Claude baseline / A-LM-12 in active plan | Не спрашивать заново, если Luba явно не reopen. |
| Q2 `GraphDpoPair` | RESOLVED_IN_PLAN_WITH_PASS37_LATEST_CLAUDE_CANON_DOWNSTREAM_PROOF | Current owner canon: exactly 14 fields in active/Claude plan-07 (`PairId`, `Level`, `NodeOrEdgeId`, `InputContext`, `Chosen`, `Rejected`, `WrongMarkers[]`, `Reason`, nested `TeacherProvenance`, `SourceIds[]`, `HeldOut`, `CandidateOrderInvariant`, `ExcludedLeakageSignals[]`, `DpoTrainReady`). | Train-ready use remains unit-level (`Level="unit"` and unit identity in `NodeOrEdgeId`); do not ask vague whole-graph pair. Implementation/data-manifest/readback proof remains future evidence. |
| Q3 `TeacherProvenance` storage | LUBA_DECISION_RECORDED_20260629_WITH_PASS37_WRITER_ALIGNMENT | attachment 483d as already recorded in active F0/board + latest Claude/Pass37 writer alignment | `GraphDpoProvenanceWriter` writes sidecar/manifest projection proof around the exact 14-field `GraphDpoPair` with nested `TeacherProvenance`, including `schema_version`, `pair_id`, provenance identifiers, `TriadVerdict`, and row-backref; it does not add fields to `GraphDpoPair`. Implementation proof pending. |
| Q4 teacher allowlist / `sonnet-*` fate | RESOLVED_IN_PLAN_WITH_EXTERNAL_TERMS_GATE | Claude legal baseline / block 08 | Open-weight permissive/self only for training teacher; closed proprietary teacher rows rebuild/quarantine; provider terms proof pending. |
| Q5 closed-model judge policy | RESOLVED_IN_PLAN_WITH_EXTERNAL_TERMS_GATE | Claude legal baseline / block 08 | Closed proprietary models are judge/eval only when `teacher_output_used_for_training=false` and terms/provenance allow. |
| Q6 common/adaptive placement / B15 pair | PASS02_CONSISTENCY_REPAIRED_NEEDS_REVIEW | attachment 483d + attachment 0ce0 B15 repair requirement + Pass01/Pass02 repair | V1 must define `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` now. Common leg remains under `llm_mvp_core` + `25.ModelServer`/`IRemoteUnitLocator`; personal/adaptive leg lives under `<project>\.xiigen`, is local/quarantine by default, and promotes only after personal held-out + provenance gates. Older "adaptive later" / "ask whether pair is v1" text is historical/pre-Pass01 only, not a current Luba question. |
| Q7 `MultiSubPhaseRouterUnit` fate | RESOLVED_IN_PLAN | Claude no-hidden-determinism baseline | Keep as deterministic baseline/candidate generator for ablation; do not delete; not final trainable authority after cutover. |
| Q8 many-to-many output | LUBA_DECISION_RECORDED_20260629_WITH_PASS10_RECONCILIATION | original task + attachment 483d as already recorded in active F0/board; Claude output-shape baseline; Pass10 active-codex repair | Public semantic output is `IQueryable<GraphNodeCandidate>` or an equivalent queryable semantic surface. `GraphNodeCandidate` must carry `candidate_id`, `unit_id`/`unit_ids`, `logical_unit_ref`, `output_ref`/`output_refs`, `output_type`, `confidence`/`score`, `provenance_refs`, `path_order_ref`, `legal_status`, `status`/`error_codes`, `source_ids`, `trace_ref`/`trace_refs`, and B1/B7/B11/B12/B6 handoff refs. Async batches materialize `IReadOnlyList<GraphNodeCandidate>` snapshots only; canonical raw `object Output` and untyped arrays are forbidden (`NO_OBJECT_OUTPUT`). Implementation proof pending. |
| Q11 ablation threshold | LUBA_DECISION_RECORDED_20260629 | attachment 483d; attachment 429f defect says missing propagation | `ablation_threshold_locked = 0.02`; Gate B must not ask Luba to assign it again. |
| Q17 `goalConfidenceFloor` | PROPOSED_NOT_LOCKED | active/Claude §E.2 block 06 | Proposed `0.80` only; it is a real `TeacherCallRequest.GoalConfidenceFloor` field in the planned teacher-call seam, but no Luba lock was found in this pass. |
| Q14 traversal order | PASS05_GRAPH_ORDER_RESOLVER_DESIGNED_NEEDS_DOWNSTREAM_REVIEW | attachment 483d + attachment 0ce0 B7 repair requirement + Pass05 B7 repair | Deterministic traversal/topological sort is a correctness/safety baseline and candidate enumerator only, not final learned authority. V1 defines `GraphOrderResolverUnit` as the trainable/adaptive logical unit that ranks legal traversal/order patterns and emits `GraphOrderDecision(pattern_id, ordered_unit_ids, skipped_edges, legal_status, provenance_status, path_order_ref, trace)`. Hidden deterministic final authority and legal/provenance bypass are forbidden. Downstream B12/B13/B18/B9/B10 consistency review remains before closure. |
| Q30 held-out threshold | LUBA_DECISION_RECORDED_20260629 | attachment 483d; attachment 429f defect says missing propagation | Held-out `>=0.95`, delta `>=+0.02`, reward `>=0.025`; runtime proof pending. |
| Q22 `qualityThreshold` / stop threshold | LOCKED_FROM_CLAUDE_RR9_Q22_Q30 | Claude RR9/Q22/Q30 locks `qualityThreshold=0.95` and `teacherGapEpsilon=0.02`; active plan-09 has `ShouldStop` branch semantics | `qualityThreshold = 0.95` aliases the Q30 held-out threshold. `teacher_on` also requires measured teacher-gap closure within `teacherGapEpsilon=0.02`; `self_best` uses only `AccuracyAfter >= qualityThreshold`. Do not treat stop threshold as a separate unresolved B-11 number. |
| Q23 `self_best` default | LOCKED_FROM_LUBA_OR_PLAN | Claude §E.2 and active plan-09 mode semantics | `self_best` is the legal default when no configured legal teacher profile exists or provider is unavailable; teacher turns on only through the legal provider/provenance path. This is a mode policy, not a numeric threshold. |
| Q24 loop stop/plateau defaults | LOCKED_FROM_CLAUDE_RR9_Q24 | Claude §E.2 RR9-lock-numbers + active plan-09 owner | `maxIterations=20`, `plateauWindow=3`, `regressionWindow=3`, `minImprovement=+0.02`, `rollbackEpsilon=0.02`, `noSignalWindow=3`, and replay `70/30` are loop-control defaults locked by the latest Claude/owner baseline. `minImprovement=+0.02` is shared with the Q30 held-out delta value but remains a loop plateau parameter, not a separate acceptance threshold. |
| Q37 better-output origin | PARTIAL_POLICY_RESOLVED_IMPLEMENTATION_GATE_REMAINS | original task + Claude legal baseline | Not a current Luba decision; real outputs must pass C16/provenance/terms/license gates. |
| Q38 output-license policy | LUBA_DECISION_RECORDED_20260629_WITH_IMPLEMENTATION_GATE | attachment 483d | Generated client outputs stay adaptive/quarantine by default; common use only after output-license clearance, redaction, per-client terms snapshot, and Luba approval. |
| Q-host route | PASS14_ROUTE_AUTHORITY_REPAIRED_CODE_LOCATION_UNPROVEN | original task route evidence already recorded in active F0/ledger + Claude baseline Q-host Variant B + Pass14 source-map repair | `xiigen-platform-dot-net-angular` is the public/API-facing facade where R16/R17 name the platform route; `xiigen-flows-dot-net-angular` is the internal `FLOW-31`/`FLOW-38` seam where directly referenced; FigmaToCode/CanvaToCode are client/caller labels, not implementation-repository authority for the platform host unless exact later source proves otherwise. `PLAN_ROUTE_SPECIFIED_BUT_CODE_LOCATION_UNPROVEN`: physical repo/path/file proof remains future implementation inventory evidence. |
| 50 cycles / review floor | NOT_PROVEN_AS_CYCLES | attachment 429f + honesty guard | Ledger rows are not cycles. Real review floor needs returned packets with reviewer id, findings/fixes/recheck. |
| dual-path extensions/host | CLOSED_IN_PLAN_EXECUTION_EVIDENCE_PENDING | attachment 429f + `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md` + source inventory R23 | Same-scenario design is now specified for SCN-R16 and SCN-R17. No host/extension readiness claim until future approved Path A + Path B evidence or exact blocker packet exists. |

## PASS08 Canonical Threshold And Gate B Overlay

Current verdict after this numeric repair remains `PLAN_NOT_READY`. This table is the canonical active-codex threshold map for Q11/Q17/Q22/Q23/Q24/Q30 and the B-5/B-11/B-12 numeric family until a later pass supersedes it with direct evidence.

| threshold key/name | value | unit/metric meaning | owner blocker/Q reference | Gate B placement | source/evidence status | current readiness effect |
|---|---|---|---|---|---|---|
| `ablation_delta_min` | `0.02` | Absolute held-out accuracy drop required when learned authority is disabled against deterministic baseline. | Q11; B-5 numeric family; dependent B7/B10/B12 reviews. | Ф0.1.3 planning lock; Ф4.3.2 edge authority; Ф5.2.1 quality meter; P-02/P-12 ablation cells. | LOCKED_FROM_LUBA_OR_PLAN: attachment 483d records Q11=0.02; active/Claude §E.0 propagate it. | Numeric value is no longer a Luba ask; implementation/config/ablation proof remains future evidence, so readiness is not closed. |
| `heldout_accuracy_min` | `>=0.95` | Minimum source-disjoint held-out `RuntimeProductAccuracy` / `AccuracyAfter` for trained acceptance. | Q30; Q34; Q22 alias; B-5 numeric family. | Ф0.1.3 planning lock; Ф5.1.x/Ф5.2.1 measurement; Ф7.2.2 `ShouldStop`; Ф9 TA cells. | LOCKED_FROM_LUBA_OR_PLAN: attachment 483d records Q30=0.95; Claude §E.2 aligns Q34 and Q22 to Q30. | Threshold ask is repaired; runtime held-out proof and split proof remain future Gate B evidence. |
| `heldout_delta_min` | `>=+0.02` | `AccuracyAfter - AccuracyBefore` minimum lift on source-disjoint held-out. | Q30/Q34; P-09/P-11/P-13 trained-acceptance. | Ф5 promotion gate; Ф7.2.1/Ф7.2.2 loop promotion; plan-11 Gate B; plan-13 TA cells. | LOCKED_FROM_LUBA_OR_PLAN: attachment 483d records Q30 delta `>=+0.02`. | Value locked; no training/proof was run in this pass. |
| `reward_margin_min` | `>=0.025` | Minimum computed reward margin from the DPO engine, not the stale `0.03` literal. | Q30/Q34; Q35 margin policy remains for wrapper classification. | Ф5.2.1 quality meter; plan-11 promotion gate; plan-13 TA cells. | LOCKED_FROM_LUBA_OR_PLAN for numeric value; Q35 wrapper policy still needs downstream review. | Numeric threshold is repaired; wrapper-vs-trained classification can still block readiness. |
| `qualityThreshold` | `0.95`, alias of `heldout_accuracy_min` | Auto-DPO stop threshold on held-out `AccuracyAfter`. | Q22; B-5 numeric family; not B-11 after Pass08 separation. | Ф7.2.2 `AutoDpoLoopOrchestrator.ShouldStop`; plan-09 Gate B. | LOCKED_FROM_LUBA_OR_PLAN for numeric value via Q30 alignment. | Stop threshold value is no longer unclear; `teacher_mode` branch and proof remain implementation/test work. |
| `teacherGapEpsilon` | `0.02` locked RR9/Q22/Q30 default for teacher-gap closure | Allowed gap for `teacher_on`: `HeldOutAccuracy(Y_best) >= HeldOutAccuracy(Y_goal) - teacherGapEpsilon`. | Q22 teacher-on subcondition. | F7.2.2 `ShouldStop` teacher_on branch. | LOCKED_FROM_CLAUDE_RR9_Q22_Q30: latest Claude baseline records teacher-gap closure as within `teacherGapEpsilon=0.02`; active plan-09 now matches. | Value no longer remains a current Luba ask; runtime teacher-gap proof remains future implementation evidence. |
| `goalConfidenceFloor` | proposed `0.80` | Confidence floor for calling legal teacher for `Y_goal`; request/config field, not acceptance accuracy. | Q17; B-2/B-3 teacher call path. | Teacher-call seam `TeacherCallRequest.GoalConfidenceFloor`; plan-08/plan-09 preparation gates. | PROPOSED_NOT_LOCKED: active/Claude §E.2 says proposed and still "за Luba". | Remains a real open/proposed value; cannot be reported as locked. |
| `self_best_default` | enabled as legal fallback/default mode | Mode when no legal teacher profile exists or provider is unavailable; chosen must be `Y_best` from real async candidate, not judge-authored text. | Q23; B-12 mode/default candidate. | Ф7.1.1 preparation; Ф7.2.2 `ShouldStop` self_best branch; plan-09 judge-origin gates. | LOCKED_FROM_LUBA_OR_PLAN: Claude §E.2 resolves Q23 as legal default; active plan-09 already uses it. | Mode ambiguity reduced; B-12 winner-selection/downstream arbiter review remains open. |
| `loop_stop_defaults` / Q24 | `maxIterations=20`; `plateauWindow=3`; `regressionWindow=3`; `minImprovement=+0.02`; `rollbackEpsilon=0.02`; `noSignalWindow=3`; replay `70/30` | Measurable stop, plateau, rollback, no-signal, and replay defaults for the auto-DPO loop. | Q24; auto-DPO loop parameters. | Ф7.2.2 plateau/budget/no-signal branches; plan-09 config. | LOCKED_FROM_CLAUDE_RR9_Q24: Claude §E.0/§E.2 and plan-09 owner mark these as Luba-default/RR9 locked. | Future gates verify behavior with these values; they must not choose loop defaults during implementation. |
| `N` / `combinationCount` / `budget.MaxCombinations` | default `4`, ceiling `8` are `LOCKED_BY_LUBA_2026_06_30`; PASS09 signature uses `GraphCandidateRunRequest.CombinationCount` | Async fan-out cardinality and producer/consumer contract, not a quality threshold. | Q16; B-11 signature repair, not Q22. | Ф6.1.2 async combinations; B-11 producer/consumer signature Gate B. | LOCKED_BY_LUBA_2026_06_30: Claude master Q16/block-06 baseline records `RESOLVED(Luba 2026-06-30)`, `N==combinationCount==request.CombinationCount`, default `4`, ceiling `8` from `MaxAdaptiveBranchCount=8` / `AdaptiveLogicalOrchestrator.cs:19`. | Q16/N drift is repaired; B-11 still depends on B-8/B-9 output-shape reconciliation, B12 winner-selection review, B6 handoff proof, and future specs/tests. |

## PASS26 Threshold/Scorer Input Calibration Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This overlay does not add product evidence. It only binds every locked numeric gate to a scorer/input/gold/comparison/scope/calibration contract so no future implementation or test gate can defer metric choice.

Calibration invariant: a deterministic baseline, ablated variant, trained checkpoint, fresh-load checkpoint, and teacher-goal comparison are comparable only when they use the same held-out row set, scorer interface, scorer version, threshold-set id, normalization rules, domain tag, source-disjoint split proof, and legal/provenance-clean gold. A mismatch is `THRESHOLD_SCORER_INPUT_MISMATCH` and blocks promotion/common readiness.

| locked value | metric/scorer | input carrier | gold or expected-label origin | comparison semantics | branch/domain scope | calibration/comparability rule | failure behavior |
|---|---|---|---|---|---|---|---|
| `RuntimeProductAccuracy >= 0.95` / `qualityThreshold=0.95` | `IHeldOutScorer.Score(HeldOutScoringRequest)` -> `ProductAccuracyScore.RuntimeProductAccuracy` | Source-disjoint `HeldOutScoringRequest` over held-out rows and candidate output refs | `expected_label` from held-out seed rows; for `language-translate`, it is self-authored and equals `chosen_translation` | Pass when the held-out product accuracy is at least `0.95`; `qualityThreshold` is an alias, not a second metric | Active v1 executable pilot is `language-translate`; future unit/edge/graph domains must register their own scorer/gold before using the profile | Same held-out set/scorer/version/normalization/domain for deterministic baseline, trained checkpoint, fresh-load, and teacher comparison | Block with `SCORER_INPUT_UNDEFINED` or `THRESHOLD_SCORER_INPUT_MISMATCH`; no Future Gate B metric choice |
| held-out lift `delta >= +0.02` | Same `IHeldOutScorer` / `RuntimeProductAccuracy` | `AccuracyBefore` from frozen deterministic baseline or parent checkpoint; `AccuracyAfter` from trained checkpoint on the same held-out request | Same `expected_label` rows as the held-out accuracy gate | `AccuracyAfter - AccuracyBefore >= +0.02` | Applies only to the trained authority being promoted; graph/edge uses require their own registered scorer/gold | Before/after must be same split, same scorer, same domain, same threshold-set id, and recorded baseline/checkpoint refs | Block promotion if the before/after rows, scorer, or baseline ref differ |
| ablation drop `>= 0.02` / Q11 | Same `IHeldOutScorer` / `RuntimeProductAccuracy` | Trained checkpoint versus ablated/no-trained-authority deterministic baseline | Same held-out `expected_label` rows | `AccuracyTrained - AccuracyAblated >= 0.02`; disabling trained authority must reduce held-out accuracy by at least two points | Unit/edge/graph threshold profile is reusable only when the branch has an ablatable trained authority and registered scorer/gold | Deterministic baseline is labelled and retained for comparison; removing the baseline is not ablation evidence | Block as `ABLATION_BASELINE_MISSING`, `SCORER_INPUT_UNDEFINED`, or `DIFFERENT_DATASET_COMPARISON` |
| `RewardMargin >= 0.025` | Block-07/11 DPO reward extractor: `reward(side,input_context)` and `DpoTrainer` margin | Accepted unit-level `GraphDpoSourceRow`/`GraphDpoPair` with serialized `Chosen`, `Rejected`, and `InputContext` | Pair semantics, not held-out gold: `Chosen`/`Rejected` must come from legal runtime candidate, self-best, legal teacher, or approved feedback route, with provenance | `chosenReward - rejectedReward >= 0.025`; judge scalars and stale `0.03` literals are not the margin | Unit-level DPO pair training/promotion signal; graph-wide/blob pairs remain forbidden | Reward margin is comparable only across the same reward extractor, serializer, pair set, checkpoint lineage, and legal/provenance gate | Block train-ready/promotion with `REWARD_EXTRACTOR_MISSING`, `STRUCTURED_SIDE_SERIALIZER_MISSING`, or stale literal margin |
| `teacherGapEpsilon = 0.02` | Same `IHeldOutScorer` applied to `Y_best` and legal `Y_goal` | Two held-out scoring requests over identical rows: learner output and legal teacher goal output | Same held-out `expected_label`; teacher output is not gold unless legal teacher route is active and provenance permits | `HeldOutAccuracy(Y_best) >= HeldOutAccuracy(Y_goal) - teacherGapEpsilon` | `teacher_on` branch only; `self_best` branch has no teacher-gap conjunct | Same scorer, split, domain, threshold set, and legal teacher provenance; teacher-gap cannot use live training-stream metrics | Block `teacher_on` threshold stop if teacher output or scorer/gold is unavailable |
| `plateauWindow=3`, `minImprovement=+0.02` | Held-out `RuntimeProductAccuracy` history from the same scorer | Last three loop iterations for the same target/checkpoint lineage | Same held-out gold rows and split policy | Plateau when the last three iterations do not improve by at least `+0.02` on held-out accuracy | Auto-DPO loop for the current target subject | Do not substitute live reward margin or training-stream accuracy for held-out plateau acceptance | Stop reason may be `plateau`; do not promote without held-out/ablation/fresh-load gates |
| `regressionWindow=3`, rollback epsilon `0.02` | Same held-out scorer plus regression-row retest | Last three accepted rows/checkpoints and prior accepted checkpoint refs | Same held-out/regression gold rows used for those rows | Regression if current checkpoint degrades prior accepted rows beyond epsilon; rollback keeps prior locator | Auto-DPO loop and lifecycle promotion | Same scorer, threshold set, split refs, and checkpoint lineage are required for fresh-load and rollback comparison | Roll back or quarantine; no common/global advancement |
| `noSignalWindow=3` | Weakness/legal/provenance signal ledger, not a product-accuracy scorer | Three consecutive candidate-discovery or pair-building attempts for the same target | Not applicable; absence-of-signal must still record legal/provenance rejection reasons | Stop with `no_signal` after three consecutive no legal/accepted train-ready signals | Auto-DPO discovery loop | No-signal cannot be used as quality success; it is an honest stop state only | Stop without promotion and keep `PLAN_NOT_READY`/future repair path |
| replay split `70/30` | Replay buffer selection policy, not held-out scorer | Training-batch composition: 70 percent new rows, 30 percent replay rows | Training rows only; held-out `expected_label` rows must not leak into replay training | Maintains anti-forgetting while preserving source-disjoint held-out evaluation | Auto-DPO training loop | Replay split must not change held-out scorer input or make replay/cache look like trained proof | Block or quarantine if held-out rows enter train/replay or replay substitutes for fresh held-out proof |

Плановая готовность после attachment 0ce0 full-task coverage simulation: `PLAN_NOT_READY`. Implementation authorization: `NO until fresh Luba approval after review`.

## PASS09 B-11 Async Producer/Consumer Contract Overlay

Current verdict remains `PLAN_NOT_READY`. This overlay repairs B-11 contract readability only; it does not
close B-8/B-9 output-shape reconciliation, B12 winner-selection, B6 unit-level DPO pair proof, implementation,
tests, builds, runtime probes, provider calls, git, or readiness.

- Canon producer owner: block 06 `IGraphCandidateRunner`.
- Canon request/config: `GraphCandidateRunRequest(ScenarioId, Query, InputGraphRefs, InputUnitRefs,
  AsyncCombinationBudget, CombinationCount, Cancellation, LegalScope, ProvenanceRefs, TraceRef)`.
- Canon producer output: `AsyncCandidateBatch(BatchId, Request, Candidates, CandidateStream,
  RequestedCombinationCount, ActualCombinationCount, Status, ErrorCodes)`.
- Canon candidate item: `GraphCandidateOutput(CandidateId, OutputRefs, UnitId, UnitIds, Confidence, Score,
  CombinationId, ProvenanceRefs, PathTraceRef, TraceRef, ErrorCodes)`. `OutputRefs` is a typed materialized
  `IReadOnlyList<GraphNodeCandidate>` snapshot of the B-8/B-9 queryable node-output shape; raw `object Output`
  is not canonical (`NO_OBJECT_OUTPUT`).
- Canon consumer handoff: `CandidateSelectionInput(AsyncCandidateBatch, GraphOrderDecision, ConsumerUnitId,
  LegalProvenanceRefs, TraceRefs)` feeds B12 winner selection and `TrainableGraphDpoBuilderUnit`; it is not
  direct training material unless B6/B12/legal/provenance gates pass.
- `N` / `combinationCount` policy: `CombinationCount` is async cardinality only, not `qualityThreshold`.
  PASS24 supersedes the historical/pre-PASS24 proposal wording: current default `4` and ceiling `8` are
  `LOCKED_BY_LUBA_2026_06_30` from Claude Q16/block-06 baseline (`RESOLVED(Luba 2026-06-30)`,
  `MaxAdaptiveBranchCount=8` / `AdaptiveLogicalOrchestrator.cs:19`).
  Requests reject `<2`, reject `> Budget.MaxCombinations`, reject/forbid a budget ceiling above `8` unless a
  later explicit Luba override exists, and respect legal/provenance constraints that can
  lower actual candidates. The batch records requested vs actual without duplicate fill.
- Failure/cancellation statuses: `PARTIAL_FAILURE`, `CANCELLED`, `TIMEOUT`, `EMPTY_CANDIDATES`,
  `DUPLICATE_CANDIDATES`, `LOW_CONTRAST`, and `INVALID_PROVENANCE` produce no train-ready pair by themselves.

## PASS24 Locked N / CombinationCount Baseline Reconciliation

Current verdict remains `PLAN_NOT_READY`. Pass24 repairs only the active-vs-Claude drift where Q16/N was still
historical/pre-Pass24 described as proposed. It does not close B-7, B-11, B-22, readiness, implementation, tests, builds, runtime probes,
provider calls, browser checks, git, or downstream proof.

- Canon decision: `N == combinationCount == request.CombinationCount`; default `4`; ceiling `8`.
- Authority: Claude baseline records `RESOLVED(Luba 2026-06-30)` and block 09 records `LOCKED N=4, потолок 8`;
  the ceiling comes from `MaxAdaptiveBranchCount=8` / `AdaptiveLogicalOrchestrator.cs:19`.
- Ownership: block 06 owns `IGraphCandidateRunner` / candidate runner and budget validation; block 09 consumes the
  block-06 result and must not redeclare a no-N runner.
- Validation: reject `CombinationCount < 2`; reject `CombinationCount > Budget.MaxCombinations`; reject/forbid
  `Budget.MaxCombinations > 8` unless a later explicit Luba override is cited.
- Boundary: legal/provenance constraints may reduce the actual usable candidate count; `AsyncCandidateBatch` records
  requested vs actual. N is async cardinality only, not `qualityThreshold`, stop threshold, Q17, Q22, or Q24.

## PASS10 B-8/B-9 Output Shape Contract Overlay

Current verdict remains `PLAN_NOT_READY`. Pass10 reconciles the plan contract for many-to-many output shape only; it does not implement code, run tests/builds/runtime probes, call providers, close B12/B6/B11, or authorize execution.

- Active numbering: B-8 is the public many-to-many/node output-shape contract. B-9 is the container/materialization reconciliation. Q41/R15 domain activation is a historical/trace alias and cannot reopen Q8.
- Public semantic shape: node and many-to-many output is `IQueryable<GraphNodeCandidate>` or an equivalent queryable semantic surface so consumers can compose threshold/order/top-N filters before materialization.
- Materialized async shape: B11 `IGraphCandidateRunner` materializes that queryable surface exactly at the node-to-runner boundary into `IReadOnlyList<GraphNodeCandidate>` snapshots carried by `GraphCandidateOutput.OutputRefs` and `AsyncCandidateBatch`.
- Candidate fields: `GraphNodeCandidate` must carry candidate/unit/logical-unit/output refs, `output_type`, confidence/score, provenance refs, `path_order_ref`, legal/status/error fields, source ids, trace refs, plus handoff refs for B1 teacher/judge provenance, B7 graph order, B11 async batch, B12 winner selection, and B6 unit-level DPO builder.
- Forbidden current contract: `NO_OBJECT_OUTPUT`; raw `object Output`, untyped arrays, and final semantic `GraphNodeCandidate[]` are historical/pre-PASS10 only. Lists/arrays are allowed only as materialized snapshots inside async batch comparison and tests.
- Handoff compatibility: B12 winner-selection consumes typed candidates; B6 receives unit-level candidate refs and output refs, not whole-graph blobs.
- Future acceptance rows: queryable-to-batch materialization preserves all metadata; no object-output canonical shape exists; typed `GraphNodeCandidate` fields are present; B12/B6 handoff metadata is present; array order is not authority.

## PASS11 B-12 Winner Selection / Chosen-Rejected Provenance Overlay

Current verdict remains `PLAN_NOT_READY`. Pass11 repairs B-12 at plan-contract level only; it does not implement code, run tests/builds/runtime probes, call providers, close B6/B10/B13/B15, or authorize execution.

- Canon owner: B12 uses the existing `IGraphWeaknessDetectorUnit` as the winner-selection unit. `CandidateWinnerSelectionUnit` / `IWinnerSelectionUnit` are not separate active names in this plan.
- Canon input: `CandidateSelectionInput(AsyncCandidateBatch, GraphOrderDecision?, ConsumerUnitId, LegalProvenanceRefs[], TraceRefs[])` from Pass09/Pass10.
- Canon output: `WinnerSelectionDecision(decision_id, chosen_candidate_ref, chosen_source_kind, rejected_candidate_refs[], weak_markers[], reason_codes[], contrast_reason, confidence, legal_status, judge_origin_status, judge_did_not_author_chosen, chosen_equals_runtime_candidate_or_legal_goal, path_order_ref, source_ids[], provenance_refs[], trace_refs[], failure_code?)`.
- `self_best` means no external teacher. It does not prove winner selection. In `self_best`, chosen must equal one real runtime candidate from `AsyncCandidateBatch.Candidates` / `GraphCandidateOutput.OutputRefs`.
- `teacher_on` may use a legal teacher goal only after B1/P-08 legal proof says the material is train-allowed for the exact scope.
- Closed proprietary judges may score/rank/flag only and must never author trainable chosen/rejected text. API-transport judge output is also eval-only unless it is separately routed through the hosted open-weight/permissive teacher path with provider terms plus model license proof; closed Claude/OpenAI/Gemini/Gemini Grounding/Search output is never that path. Required gates: `judge_did_not_author_chosen == true` and `chosen_equals_runtime_candidate_or_legal_goal == true`.
- Rejected must be a weaker/incorrect real candidate or accepted legal contrast with weak marker and reason. Empty, arbitrary, or filler rejected sides are invalid.
- Failure modes `EMPTY_CANDIDATES`, `NO_WINNER`, `DUPLICATE_CANDIDATES`, `TIE`, `LOW_CONTRAST`, `INVALID_PROVENANCE`, `JUDGE_ORIGIN_VIOLATION`, `CLOSED_PROVIDER_CONTAMINATION`, `CANCELLED`, and `TIMEOUT` produce no train-ready pair by themselves.
- Handoff: B12 feeds `TrainableGraphDpoBuilderUnit` and B10 provenance writer with chosen/rejected candidate refs, `path_order_ref`, source ids, legal/judge-origin status, provenance refs, and trace refs. B6 may create only a candidate for the exact 14-field `GraphDpoPair` with `Level="unit"` until B6/B9/B10/legal/provenance gates pass.
- Future acceptance rows: self_best winner equals runtime candidate; closed judge cannot author chosen; rejected has contrast/reason; tie/low contrast quarantines or returns no train-ready pair; decision metadata feeds the B6 candidate and B10 provenance writer.

## PASS12 B-9/B-10/B-13/B-17 GraphDpoPair / TeacherProvenance / Writer Alignment

Current verdict remains `PLAN_NOT_READY`. Pass12 repairs the canonical/provenance
contract at plan-text level only; it does not implement code, run tests/builds,
call providers, close B6/B12/B15, or authorize execution.

- `GraphDpoPair` current train-ready authority is the active/Claude plan-07
  exact 14-field canon: `PairId`, `Level`, `NodeOrEdgeId`, `InputContext`,
  `Chosen`, `Rejected`, `WrongMarkers`, `Reason`, nested `TeacherProvenance`,
  `SourceIds`, `HeldOut`, `CandidateOrderInvariant`,
  `ExcludedLeakageSignals`, `DpoTrainReady`. Train-ready rows remain unit-level;
  edge/graph diagnostic evidence is not DPO-train authority unless Luba later
  approves an explicit override.
- `TeacherProvenance` remains the block-08 20-field owner. Heavy legal proof is
  stored in a sidecar manifest JSONL with `schema_version`, timestamp,
  source teacher/provider, model id/version, license, terms snapshot,
  proof_right_to_use_train_ref, source category, output hash/ref, redaction
  status, provenance ids, judge-only/train-allowed flags, common/adaptive scope,
  and TriadVerdict linkage.
- `GraphDpoProvenanceWriter` is the planned writer/normalizer. It consumes B1
  teacher/judge metadata, B12 winner-selection refs, B6 unit-level candidate
  refs, and B7/B10 path-order refs; it writes sidecar rows plus
  the exact 14-field `GraphDpoPair` plus manifest/projection proof and `SourceIds`.
- `dpo_train_ready=true` requires `legal_accept && split_accept &&
  authority_provenance_accept`, `provenance_field_presence_rate==1.0`, and
  `row_sidecar_ref_match_rate==1.0`. Common promotion is blocked without
  output-use, license/terms, redaction, split/source-disjoint, and authority
  provenance proof.
- Closed provider train attempt returns `DATASET_REBUILD_REQUIRED`; closed
  judge scores can be `judge_provenance` only and cannot become train-allowed
  `TeacherProvenance` or chosen/rejected training targets.
- Remaining boundary: B9/B10/B13/B17 are repaired for canonical alignment only.
  Implementation tests, data-manifest readback, export/import proof, and
  source-disjoint held-out evidence remain future proof.

Historical narrower blocker-1 (not attachment 0ce0 B1 teacher/judge) was closed in plan: source-branch/repo authority и source-to-plan mapping заданы в `active-codex-execution-02-source-inventory-and-reference-map.md` R21.1/R21.2. Implementation repos = пять repo из Claude baseline/original task; source authority = latest-updated branch per repo, read-only; work = separate `C:\Projects\DPOLegalLearning\<repo>` / `DPOLegalLearning`; planning repo `C:\Users\lubao\Documents\xiigen` не implementation authority.

Historical/pre-attachment-0ce0 note: the earlier narrower blocker-1..4 repair said `PLAN_READY_FOR_LUBA_REVIEW` after source authority, external seam, same-scenario dual-path design, and provider/provenance proof policy were closed in-plan. Attachment 0ce0 supersedes that narrower verdict because it reports full original-task coverage simulation with FULLY=0, PARTIAL=25, NOT_COVERED=8, ERROR=1 and design/dependency blockers B1-B21. Historical/pre-post-repair verdict after attachment 0ce0 was `PLAN_NOT_READY`. Provider terms/license/output-use proof collection, tests/builds/screenshots/branch-ref evidence/mergeability, same-scenario join files, real external calls, provider terms retrieval, and implementation proof remain future evidence/execution checkpoints only after plan repair and approval; they do not authorize implementation and cannot hide current design blockers B1-B21.

## Central Architecture Repair Overlay - B1/B6/B7/B15

Status: `PLAN_NOT_READY`. This overlay combines Pass 01 central design material, Pass 02 B15/Q6 consistency repair, Pass 03 B1 seam refinement, and Pass 04 B6 unit-level pair-policy repair. It does not close readiness because remaining blockers B2-B4/B6-B21 and the 50-pass chain still remain.

| blocker | current design decision | current status |
|---|---|---|
| B1 | Pass 03 canonical seam: `ITeacherGoalClient` generates legal teacher goal/output material; `IJudgeScoreClient` scores/evaluates only. Both are real-call seams even when v1 starts with `ExternalTeacherMock`/`ExternalJudgeMock`. Requests carry `provider_id`, `model_id`, `model_version`, `endpoint_profile`, `prompt_template_id`, `input_graph_id`, `input_unit_ids[]`, `source_ids[]`, `provenance_ids[]`, `license_policy_snapshot`, `terms_snapshot_id`, `request_hash`, `created_at`, and no secrets. Teacher responses carry `response_hash`, `raw_response_ref`, `redacted_response_ref`, `safety_status`, `legal_status`, complete `TeacherProvenance`, and generated `GraphDpoPair`/unit-level preference material refs. Judge responses carry `score`, `score_breakdown`, `reasoning_summary_ref`, `rubric_id`, `rubric_version`, `thresholds_snapshot`, `may_train`, `judge_only`, and `judge_did_not_author_chosen`. Closed proprietary providers are judge/eval/critique only and never training targets; Claude/OpenAI/Gemini/Gemini Grounding/Search output reaching training triggers `DATASET_REBUILD_REQUIRED`. API transport may teach only for hosted open-weight/permissive models with recorded provider terms plus model license proof. Unknown/missing/negative proof routes to `QUARANTINE` / not train-ready or `DATASET_REBUILD_REQUIRED`, never common train-ready rows. Unit tests must assert exact mock response shapes; only external-call mock replacement may later be tracked in `codex-external-calls-mock.md` after product readiness and approval. | `PASS03_SEAM_DESIGNED_NEEDS_REVIEW` |
| B6 | Split deterministic schema normalization from learned unit-level DPO pair policy. `GraphDpoBuilder.BuildPairAsync` remains deterministic fallback/validator with `semantic_quality_claimed=false`. `TrainableGraphDpoBuilderUnit` is an allowed `ILogicalUnit` owned, contained, and coordinated by `ILearningDynamicGraph` through `DynamicGraphMachine`; it learns which `UnitId` / `ILogicalUnit` output comparison should become DPO training material. `GraphDpoPair` is train-ready only at unit level but keeps the exact active/Claude plan-07 14-field schema: `Level="unit"`, unit id in `NodeOrEdgeId`, `InputContext`, `Chosen`, `Rejected`, `WrongMarkers`, `Reason`, nested `TeacherProvenance`, `SourceIds`, `HeldOut`, `CandidateOrderInvariant`, `ExcludedLeakageSignals`, and `DpoTrainReady`. Graph path/order/legal/split diagnostics live in triad/provenance/trace records, not as extra pair fields. Whole graph A vs whole graph B and graph-wide blob pairs are forbidden as train-ready DPO. State is `GraphDpoBuilderUnitState` with feature weights, policy version, unit-context vocabulary, rejection-reason classifier state, weakness-detector state, advisory provenance/legal calibration, observed reward/outcome stats, and checkpoint refs. Inputs name B1 teacher/judge refs, B7 graph-order output, B10 provenance envelope, B12 winner-selection trace, B15 legal_scope, and B18 cold-start seed without claiming those blockers are closed. Output is `GraphDpoPairCandidate(Level=unit, ...)` with `builder_score`, rejection reason, hard legal/provenance gate result, and common/adaptive/quarantine/eval-only routing. Deterministic fallback cannot be claimed as learned builder. | `PASS04_UNIT_LEVEL_POLICY_DESIGNED_NEEDS_DOWNSTREAM_REVIEW` |
| B7 | Pass 05 canonical design: deterministic/topological traversal is the stable correctness and safety baseline, while `GraphOrderResolverUnit` is the trainable/adaptive `ILogicalUnit` that ranks only legal traversal/order patterns under hard constraints. Pattern taxonomy is inspectable: `order-0-direct-unit`, `order-1-single-predecessor`, `order-2-fan-out-fan-in`, `order-3-cross-domain-reference`, and `order-4-async-many-to-many-aggregation`. Inputs include graph nodes/edges, `AuthorityUnitId`, dependency/predecessor refs, candidate units, scenario/query, provenance refs, budget/concurrency limits, legal scope, prior outcomes, and B12 winner-selection trace. Output is `GraphOrderDecision(pattern_id, ordered_unit_ids[], ordered_path_refs[], skipped_edges[], rejected_edges[], confidence, legal_status, provenance_status, path_order_ref, trace_ref, reason_codes[])`. Hard constraints: acyclic/termination guard, source/provenance/legal gate before train-ready output, no hidden sixth graph level beyond L0-L5, and no learned order may bypass `TeacherProvenance`, B1 legal seam, or B6 unit-level DPO invariant. Training/evaluation use held-out success, reward delta, resolver-off ablation, legal rejection reasons, runtime outcome, and user/client feedback when policy allows; deterministic baseline remains stable for comparison. `path_order_ref` feeds B6 provenance/triad context and must not be added as a `GraphDpoPair` field. | `PASS05_GRAPH_ORDER_RESOLVER_DESIGNED_NEEDS_DOWNSTREAM_REVIEW` |
| B15 | Current v1 storage route is common-only. `DynamicGraphGlobal` trains only common-eligible legal material under `llm_mvp_core`: self-authored/project-internal seed rows, legal first-party/self-best rows, or future hosted/open-weight teacher rows only after provider/license/output-use/terms/provenance/split proof. `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` remains the later activation blueprint, not a v1-active personal leg. Client outputs, UI feedback, raw/private text, customer/project material, FigmaToCode/CanvaToCode/FLOW-38 outputs, and unverified third-party material route to `PersonalAdaptive`, `EvalOnly`, or `Quarantine` by default and cannot become common/global train-ready. Cross-boundary promotion to common requires positive output-license/provenance, consent/ownership where needed, redaction, per-client terms snapshot, source-disjoint split/leakage proof, held-out/ablation/no-regression/fresh-load proof, manifest compatibility, and explicit Luba approval. Proof gates: separate future personal/common checkpoints/manifests, local held-out before/after, fresh-load local, rollback, export/import/backup, no secret capture, and common-contamination zero. Historical/pre-Pass35 wording that made the pair v1-active is superseded. | `PASS35_COMMON_ONLY_V1_STORAGE_POLICY_REPAIRED_EXECUTION_PROOF_PENDING` |

Pass 02 consistency repair status is historical/pre-Pass35 for B15/Q6 wording where it made `DynamicGraphMachinePair` v1-active. Current Pass35/Pass43 policy supersedes it: v1 is common-only, while the personal/adaptive pair is a later activation blueprint that requires explicit Luba scope expansion. This does not close readiness, does not set `PLAN_READY`, and does not authorize implementation, tests, builds, runtime probes, browser checks, git actions, real provider calls, or training.

Pass 05 B7 repair status: `PASS05_GRAPH_ORDER_RESOLVER_DESIGNED_NEEDS_DOWNSTREAM_REVIEW`. This pass defines the graph-order resolver contract in the plan only. It does not implement `GraphOrderResolverUnit`, run tests/builds/training/runtime probes, close B7, set `PLAN_READY`, or authorize execution. B7 still needs downstream review against B12 winner selection, B13 fan-in/fan-out ownership, B18 cold-start seed, and B9/B10 provenance writer contracts before closure.

## Central Architecture Repair Overlay - PASS13 B13/B14/B16/B20 Ownership

Status: `PLAN_NOT_READY`. Pass13 repairs ownership and merge seams at plan-contract level only. It does not implement code, run tests/builds/runtime probes, call providers, use git, close readiness, or claim any additional pass beyond 13/50 minimum.

| blocker | current design decision | current status |
|---|---|---|
| B13 | Ownership map is now explicit. Block 06 enumerates async fan-out candidates through `IGraphCandidateRunner`, `GraphCandidateRunRequest`, `AsyncCandidateBatch`, and `IGraphCombinationStrategy`. Block 04/B7 owns graph order/path selection through `GraphOrderResolverUnit` and `path_order_ref`. Block 02 owns `EdgeAuthorityUnit`, `EdgeAuthorityInput`, `EdgeAuthorityDecision`, and the fan-in interface. Block 06 owns async branch/candidate payloads consumed by merge. Block 04/master topology owns `DynamicGraphTopology.AuthorityUnitId` and latest-wins registry refs. | `PASS13_OWNERSHIP_MERGE_SEAMS_REPAIRED_NEEDS_DOWNSTREAM_PROOF`; not closed. |
| B14 | `EdgeAuthorityInput` fields are explicit: `edge_id`, `from_unit_id`, `to_unit_ids[]`, `candidate_transition_ref`, `candidate_path_ref`, `path_order_ref`, `source_ids[]`, `provenance_refs[]`, `legal_scope`, `graph_context_ref`, `authority_state_ref`, `checkpoint_ref`, `candidate_refs[]`, `excluded_leakage_signals[]`, `trace_ref`. `EdgeAuthorityDecision` fields are explicit: `decision`, `confidence`, `score`, `chosen_edge_refs[]`, `chosen_path_refs[]`, `rejected_edge_refs[]`, `skipped_edge_refs[]`, `reason_codes[]`, `trace_refs[]`, `provenance_refs[]`, `authority_source`, `dpo_train_ready=false until B6/B10/legal/provenance gates pass`. | `PASS13_EDGEAUTHORITY_IO_REPAIRED_NEEDS_DOWNSTREAM_PROOF`; not closed; `EdgeAuthorityDecision` is metadata, not a train-ready row. |
| B16 | N-to-1 merge is explicit and typed. `IEdgeFanInMerger.Merge(branchResults, runId)` returns `GraphFanInMergeResult` with `Merged`, `Context`, `MergeRole = FanInMergeRole.DeterministicFanInBaseline`, `MergeOrderIsAuthority = false`, `MergedBranchCount`, conflicts/rejected alternatives/provenance/trace/failure reason refs. B7 supplies path/order metadata; B12/B6 receive context/provenance only and still own unit-level chosen/rejected DPO semantics. | `PASS32_B16_TYPED_CON_MERGE_CARRIER_RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`; not implementation/test proof; merge is deterministic baseline in v1, not trainable merge and not a train-ready DPO writer. |
| B20 | Namespace/block ownership is separated from common/adaptive policy. The active master/block owner map is the logical owner registry for NEW planned types/units. `DynamicGraphTopology.AuthorityUnitId` carries the edge owner ref; latest-wins resolution is future implementation behavior; copied workdirs and benchmark files remain evidence only. B15 remains the owner of common/adaptive `DynamicGraphMachinePair` policy. | `PASS13_NAMESPACE_OWNER_REPAIRED_NEEDS_TRACE_AND_PROOF`; not closed. |

Historical Pass13 boundary: B14 host-route/API alias, B16 better-output adapter/five-format alias, and B20 common/adaptive alias remained downstream trace-repair items at Pass13 time. Current Pass15 supersedes only the B16 alias by mapping better-output/five-format to B-4/R17 output-feedback adapter plus B-19 host-route proof; B14/B20 downstream proof remains separate.

## Central Architecture Repair Overlay - PASS14 B14/B19/Q-host Route Authority

Status: `PLAN_NOT_READY`. Pass14 repairs host-route/source authority wording at plan level only. It does not implement code, inspect physical host repositories, run tests/builds/runtime probes, call providers, use browser, use git, or claim readiness.

| blocker / route | current design decision | current status |
|---|---|---|
| B14 | B-14 is EdgeAuthority I/O only. Host-route/API evidence must not be hidden under `EdgeAuthorityInput` or `EdgeAuthorityDecision`. | `PASS14_EDGEAUTHORITY_SEPARATED_FROM_HOST_ROUTE_NEEDS_DOWNSTREAM_PROOF`; not closed. |
| B19 / Q-host | Canon route authority is Claude baseline / Luba Q-host Variant B: `xiigen-platform-dot-net-angular` public facade -> `xiigen-flows/FLOW-31` internal design-analysis seam; `FigmaToCode-design-gen` + `CanvaToCode` are client/caller labels for html/css generation. FLOW-38 belongs to better-output-to-DPO route evidence only where directly referenced. | `PASS14_Q_HOST_ROUTE_AUTHORITY_REPAIRED_CODE_LOCATION_UNPROVEN`; not closed; `PLAN_ROUTE_SPECIFIED_BUT_CODE_LOCATION_UNPROVEN`. |
| B16 downstream alias | B-16 remains the N-to-1 merge contract from Pass13. PASS15 maps better-output/five-format wording away from B-16 and into B-4/R17 plus B-19. | `PASS15_B16_N_TO_1_ONLY_NO_BETTER_OUTPUT_CLOSURE`; not closed. |
| Repo/source authority wording | Active-codex now separates core/platform/flow repositories from client/caller labels. Discovered/copied workdirs remain non-authoritative unless Luba explicitly expands scope. | Source-map wording repaired; physical branch/path/file inventory remains future proof. |

## Central Architecture Repair Overlay - PASS15 B4/R17 Better-Output/Five-Format Alias

Status: `PLAN_NOT_READY`. Pass15 repairs the better-output/five-format alias at plan/source-map level only. It does not implement code, inspect physical host repositories, run tests/builds/runtime probes, call providers, use browser, use git, close readiness, or claim more than one sequential pass.

| blocker / route | current design decision | current status |
|---|---|---|
| B4 / R17 | Better-output is the R17 output-feedback adapter contract. Input is `GeneratedUiOutputFeedbackEvent` from platform/flows host route with event/correlation/tenant refs, chosen/rejected output refs or hashes, score/outcome/reason, source category, terms snapshot refs, and provenance refs. `Flow38FeedbackAdapter` normalizes to `GraphDpoSourceRow`; legal/provenance/split gates decide quarantine/personal/common routing; only positive proof may emit a unit-level `GraphDpoPairCandidate`, otherwise `dpo_train_ready=false`. | `PASS15_B4_R17_OUTPUT_ADAPTER_CONTRACT_REPAIRED_NEEDS_DOWNSTREAM_PROOF`; not closed; no physical code proof. |
| Five input formats | The plan-07 five-format adapter matrix remains the five named existing formats: `DpoPair`, `RoutingDpoPair`, `OrchestrationPromptDpoPair`, `CodeTraceBuilderDpoPair`, and `CrossStackBusinessFlowGraphDpoPair`. | Five-format proof cannot be invented from better-output wording; FLOW-38 is a separate/new host feedback input path. |
| B16 | B-16 is strictly N-to-1 fan-in merge. | `PASS15_B16_N_TO_1_ONLY_NO_BETTER_OUTPUT_CLOSURE`; better-output/FLOW-38/five-format wording cannot silently close B-16. |
| B19 / Q-host | B-19 owns R17 host/API physical-route proof: platform facade plus flows FLOW-38 route. | `PASS15_B19_R17_HOST_ROUTE_BOUNDARY_CODE_UNPROVEN`; future inventory must name exact repo/path/branch/file evidence before readiness. |
| Mock/test/replacement obligation | Future approved execution must mock the host/external feedback response and unit-test quarantine/personal routing for unverified client/third-party origin; common/train-ready requires positive legal/provenance/split proof. | Replacement with real route/provider evidence belongs only in the approved mock replacement ledger during future product execution. |

## Central Architecture Repair Overlay - PASS16 B4/R17 Mock Seam Specification

Status: `PLAN_NOT_READY`. Pass16 repairs only the plan-level mock seam for B-4/R17. It does not implement code, run tests/builds/runtime probes, inspect host repos, call providers, use browser, use git, create `codex-external-calls-mock.md`, close readiness, or claim more than one sequential pass.

| blocker / route | current design decision | current status |
|---|---|---|
| B4 / R17 seam class | Better-output/FLOW-38 is classified as `internal_host_api_or_resource_seam` under Luba's mock policy because it crosses platform/flows/core process/repo/resource boundaries. | `PASS16_B4_R17_MOCK_SEAM_SPECIFIED_NEEDS_IMPLEMENTATION_AND_REAL_REPLACEMENT_PROOF`; not closed. |
| Planned interfaces | Host facade `IBetterOutputFeedbackFacade`, resource seam/client `IFlow38FeedbackResourceClient.SubmitFeedbackAsync(GeneratedUiOutputFeedbackEvent request)`, core ingress `IBetterOutputDpoIngestApi.IngestAsync(...)`, and normalizer `Flow38FeedbackAdapter.Normalize(...) -> GraphDpoSourceRow`. | Plan contract only; future implementation inventory must prove exact physical owner files. |
| Mock input event | Synthetic `GeneratedUiOutputFeedbackEvent` fixture uses `event_id=evt-flow38-pass16-001`, `correlation_id=corr-flow38-pass16-001`, `tenant_scope=tenant:test-local`, `prompt_hash=sha256:prompt-r17-pass16`, chosen/rejected refs `mock://flow38/output/chosen/v1` and `mock://flow38/output/rejected/v1`, hashes `sha256:chosen-r17-pass16` and `sha256:rejected-r17-pass16`, `source_category=client_unverified`, pending terms/output-license refs, one synthetic provenance ref, `redaction_policy_ref=synthetic_fixture_no_private_text`, and `mock_source=SYNTHETIC_FIXTURE`. | Self-owned synthetic fixture only; not provider/legal proof and not train-ready evidence. |
| Mock resource response | Mock FLOW-38 response is `resource_status=mock_feedback_recorded`, `feedback_ref=mock://flow38/feedback/pass16`, chosen/rejected refs preserved, `score_delta=0.42`, `source_category=client_unverified`, `terms_snapshot_status=missing_or_pending`, `output_license_status=pending`, `provenance_status=client_supplied_unverified`, `route=platform_facade_to_FLOW38_to_core_adapter`, `mock_source=SYNTHETIC_FIXTURE`. | Mock host/resource response only; no real FLOW-38 integration claim. |
| Normalized output | `Flow38FeedbackAdapter` must output one `GraphDpoSourceRow` with source/event/correlation refs preserved, chosen/rejected refs and hashes preserved, `chosen_origin=client_supplied_unverified`, `rejected_origin=client_supplied_unverified`, `source_category=client_unverified`, `legal_verdict=QUARANTINE`, `allowed_use=PersonalAdaptive`, `route=adaptive_or_quarantine`, `semantic_quality_claimed=false`, `mock_source=SYNTHETIC_FIXTURE`, and `dpo_train_ready=false`. Any `GraphDpoPairCandidate` preview is unit-level, carries the same refs/hashes, and stays quarantined/non-common/non-train-ready. | Legal/provenance/split gates must pass later before any train-ready pair. |
| Unit test expectation | Future approved test `Flow38FeedbackAdapterTests.MockFeedbackEvent_NormalizesToQuarantinedGraphDpoSourceRow` asserts exactly one normalized row, exact event id and chosen/rejected hashes, `mock_source=="SYNTHETIC_FIXTURE"`, `legal_verdict=="QUARANTINE"`, `allowed_use=="PersonalAdaptive"`, `route=="adaptive_or_quarantine"`, `semantic_quality_claimed==false`, `dpo_train_ready==false`, and zero common/train-ready output. | Test name/assertion is specified now; execution is future approved implementation evidence. |
| Replacement ledger | Real host/resource replacement after product readiness must be recorded only in `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md` with seam id `BETTER_OUTPUT_FEEDBACK_TO_DPO`, mock name/hash, unit test, real endpoint/resource, terms/license/provenance refs, approval ref, and replacement status. | File is not created now; no other technical debt is allowed. |

> По прямому требованию Luba «вопросы в конец» (R-14): все открытые вопросы и ключевые
> решения собраны здесь, в самом конце мастер-плана. Сначала — **7 решающих вопросов**
> (они повторяются почти во всех блоках и определяют канон), затем — **остаток per-block**
> (специфичные для отдельных блоков). Нумерация сквозная.

## E.1. Семь решающих вопросов (CANON — определяют весь план)

Текущий статус Q1-Q7 задаёт §E.0: Q1/Q2/Q4/Q5/Q7 resolved in plan, Q3/Q6 recorded by attachment 483d, and none of Q1-Q7 is a current Luba-only question unless Luba explicitly reopens it. Paragraphs below are retained as historical trace of the original master-plan decision surface.

**Q1. Подтверждение уровня A-LM-12 (главный).** Подтвердить, что `ILearningDynamicGraph`
= `ILearningMachine<DynamicGraphDomain>` (специализация Level 5 поверх динамического
графа), а **НЕ** новый 6-й архитектурный уровень. A-LM-12 (`docs/DECISIONS-LOCKED.md:35`)
— закрытый список из 5 уровней; введение DynamicGraph как уровня = decision-reopening и
правка `DECISIONS-LOCKED.md`. Ведущий вариант (CANON) — специализация поверх L5.
*(Источник: блоки 00, 04, 05, 01, 02, 03, 06, 08, 09, 10, 11, 12, 13, 14 — вопрос задан
в каждом.)*

**Q2. Утверждение канона `GraphDpoPair` (historical/pre-Pass37 wording repaired).** Старый набор полей
the old multi-level `level` selector plus `node_or_edge_id` wording is retained here only as trace of the original
формулировки. Pass37/latest-Claude current authority requires the exact active/Claude
plan-07 14-field canon with nested `TeacherProvenance`: `PairId`, `Level`,
`NodeOrEdgeId`, `InputContext`, `Chosen`, `Rejected`, `WrongMarkers`, `Reason`,
`TeacherProvenance`, `SourceIds`, `HeldOut`, `CandidateOrderInvariant`,
`ExcludedLeakageSignals`, `DpoTrainReady`. Train-ready rows require
`Level="unit"`; path/order/legal/split diagnostics live in provenance/triad/trace
records, not as extra pair fields. Проекция `chosen/rejected/input_context` на
существующий `LearningTrainingItem` `00.Core/LearningMachineContracts.cs:30` не
меняет контракты 00.Core. *(Источник: все блоки.)*

**Q2 authority repair (Pass04 update).** This active plan now treats the old
`old broad GraphDpoPair question` wording as resolved, not as a remaining
whole-graph-pair question. `GraphDpoPair` train-ready material is unit-level DPO
material with the exact 14-field active/Claude canon above; unit-level is enforced
by `Level="unit"` and unit identity in `NodeOrEdgeId`, while path/order/legal/split
metadata remains in provenance/triad/trace records.
Remaining questions must not
ask Luba to approve a vague whole-graph `old broad GraphDpoPair question`; they must also
not re-ask Q3/Q6/Q8/Q11/Q14/Q30/Q38 values already locked by §E.0/attachment 483d or
Q37/Q-host principles already classified as resolved/partial implementation gates.

**Q3. Где хранить `TeacherProvenance` (20 полей; 13+5+2 RR6 `service_mode`/`grounding_used`).** Исторический вопрос закрыт текущей таблицей §E.0 and Pass37/latest-Claude: `TeacherProvenance` is nested in the exact 14-field `GraphDpoPair`, and `GraphDpoProvenanceWriter` writes sidecar/manifest projection proof with `schema_version`; common под `llm_mvp_core/models|training-data`, adaptive под `<project>\.xiigen`. Ниже оставлена исходная формулировка как trace, не как текущий вопрос к Luba: на строке `GraphDpoPair`, на
`LearningBatch` (рядом с `SourceIds`, `00.Core/LearningMachineContracts.cs:26`), или
расширить манифест `25.ModelServer`/`18.CapabilityTraining`? Сегодня teacher-полей нет
нигде (только в скилах). Current answer: the pair has the nested 20-field
`TeacherProvenance`, and heavy legal/storage proof is backed by sidecar/manifest
readback; future proof must verify export/import/readback parity.
*(Источник: 00, 02, 04, 08, 12 и др.)*

**Q4. Список legal teacher-семейств и нейтральных provider roles (audit contamination).**
Current answer: это не flat permissive set. `FamilyOf(model_id)` identifies model-family/vendor;
`hosting_provider` identifies host/reseller/API transport; names that can be both vendor and host
(DeepSeek, Moonshot/Kimi, Z.ai/GLM, Mistral platform) must be recorded in both roles separately.
Family fate for v1: Qwen3 has the selected/recommended `qwen3-together-v1` blueprint but hosted
train/common use is blocked until non-pending provider terms/output-use proof; DeepSeek,
Kimi/Moonshot, GLM and Mistral are deferred/not train-ready unless exact per-release
`CanonicalLicense`, provider terms, output-use, model/version, and provenance proof exist. C5
permissive set is exact: `Apache-2.0`, `MIT`, `ProjectInternal`, `Synthetic`; no ellipsis and no
acceptance from declared license alone. Contaminated `sonnet-*`/closed-provider training rows
remain `DATASET_REBUILD_REQUIRED`. *(Источник/current owner: active/Claude block 08 ADD-08 +
R-17; this row is current policy, not an open request to Luba.)*

**Q5. Политика judge-моделей.** Подтвердить, что закрытые модели (Claude/OpenAI/Gemini)
допустимы **только** как `judge_model` при `teacher_output_used_for_training=false`, и
нигде как chosen/rejected-цель дистилляции. Нужен ли отдельный явный allow-list
judge-провайдеров. *(Источник: 06, 07, 08, 09, 12.)*

**Q6. Common vs adaptive assets (R5/A-LM-10).** Historical/pre-Pass35 wording below is superseded for current planning: attachment 483d keeps common under `llm_mvp_core` + `25.ModelServer`/`IRemoteUnitLocator`, and current Pass35/Pass43 B15/B18 boundary requires v1 common-only storage. `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` is the later activation blueprint with personal/adaptive under `<project>\.xiigen` only if Luba later explicitly expands scope. The original lines below are retained as trace, not a current Luba question and not a v1-active pair:
директорию/манифест/локатор: common-нога графа — `llm_mvp_core/models/` +
`training-data/` через `25.ModelServer` (`IModelRepository`, `IRemoteUnitLocator`);
adaptive-нога — `<project>\.xiigen\...` (A-LM-10, `docs/DECISIONS-LOCKED.md:33`) через
`ILearningMachinePair` (`00.Core/LearningMachineContracts.cs:245`). Historical question superseded by Pass35: v1 is common-only now; personal/adaptive pair activation is later and cannot train common without the promotion proof bundle and explicit Luba approval. Implementation proof remains future. *(Источник: 00, 02, 04, 05, 10, 11; Pass35 B15 repair.)*

**Q7. Судьба `MultiSubPhaseRouterUnit` / baseline-роутеров.** Подтвердить путь: обучаемый
`EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW) = финал ребра;
`MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02,
`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283`; обученного состояния нет
— §A.2.1) и `26.TrainableRouter`/`KeywordUnitScorer` **понижаются** до генераторов
кандидатов/baseline для ablation, а **не удаляются** (удаление сломало бы ablation и
нарушило «удаление ≠ покрытие замены»). *(Источник: 00, 04, 12; A-LM-09/A-LM-13.)*

## E.2. Остаток вопросов per-block (специфичные)

**Блок 01 (узел):**
- Q8. Historical/pre-PASS10 trace alias only: the old question "`IQueryable<GraphNodeCandidate>` or
  `GraphNodeCandidate[]`?" is resolved by §E.0 and PASS10. Current contract: public node/many-to-many
  output is `IQueryable<GraphNodeCandidate>` (queryable semantic surface); `IReadOnlyList<GraphNodeCandidate>`
  is only the materialized async snapshot inside `AsyncCandidateBatch`; raw `object Output` and untyped arrays
  are not canonical (`NO_OBJECT_OUTPUT`). Implementation proof remains future evidence.
- Q9. Веса узла — counter-baseline (`Dictionary<string,int>+1`, как в 43) или сразу
  типизированный preference-weight update с реальным DPO reward-margin?
- Q10. Узлы доменного trainable-интерфейса
  (`27.TranslationDomain/ITranslationUnitTrainer`) оборачивать в узел графа или оставлять
  внутренними компонентами их машины?

**Блок 02 (ребро):**
- Q11. RESOLVED by §E.0/attachment 483d: `ablation_threshold_locked = 0.02`; не спрашивать Luba заново. Gate B п.3 должен требовать это число как плановый lock, а implementation proof остаётся будущим.
- Q12. **RESOLVED_IN_PLAN (B-16/F1-merge-carrier):** v1 uses «N веток → 1 результат +
  context-packet» through deterministic baseline `IEdgeFanInMerger.Merge(branchResults, runId)`,
  returning typed `GraphFanInMergeResult` with `Merged`, `Context`,
  `MergeRole = FanInMergeRole.DeterministicFanInBaseline`, `MergeOrderIsAuthority = false`,
  `MergedBranchCount`, conflicts/rejected alternatives/provenance/trace/failure reason refs.
  Multi-output stays `IDynamicGraphEdge.ToNodeIds[]` + `GraphEdgeKind.MultiOutput`; a trainable
  fan-in arbiter is deferred and not introduced in v1. Implementation/test/runtime proof remains future evidence.

**Блок 03 (оркестратор):**
- Q13. **Закрыт каноном §A.4.1/§A.4.2** (`GraphEdgeKind` 5 членов / `string
  AuthorityUnitId` / `ParallelEligible`). Требуется лишь финальное подтверждение Luba:
  утвердить NEW-сигнатуры `IDynamicGraphEdge` и `DynamicGraphTopology` как адаптер поверх
  `BuildDynamicDomainTransitions` (`AdaptiveLogicalOrchestrator.cs:825`) и как результат
  `BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)` с budget-proof /
  `budget_exceeded` / mutation-evidence Gate B из блока 04.
- Q14. RESOLVED by §E.0/attachment 483d: v1 keeps deterministic honest traversal/topological baseline; learnability remains in `EdgeAuthorityUnit`; Jaccard predictor deferred as a real trainable unit only. Historical wording: порядок рёбер (параллель/очередь) — оставить детерминистическим baseline
  (`DomainGraphClosure` + E35; вызовы `AdaptiveLogicalOrchestrator.cs:531/552`) или сделать
  обучаемый предиктор зависимостей (Jaccard E49/E50 → baseline для ablation)? По умолчанию
  — детерминистический.
- Q15. **Закрыт каноном §A.4.2:** per-edge авторитет хранится как `string
  AuthorityUnitId` (ссылка в реестр, резолв `ResolveActiveUnit`/latest-wins,
  `AdaptiveLogicalOrchestrator.cs:269`) в **поле ребра `DynamicGraphTopology`** (не
  отдельный реестр владельцев); это определяет, где живёт авторитет при export/import
  графа (блок 04 §3.4/§10). Требуется лишь финальное подтверждение Luba.

**Блок 06 (async-комбинации):**
- Q16. RESOLVED(Luba 2026-06-30) / `LOCKED_BY_LUBA_2026_06_30`: `N == combinationCount ==
  request.CombinationCount`; default `4`; потолок `8` от `MaxAdaptiveBranchCount` /
  `AdaptiveLogicalOrchestrator.cs:19`. Это async cardinality, не `qualityThreshold` и не stop threshold.
  Потолок выше `8` запрещён без later explicit Luba override.
- Q17. `goalConfidenceFloor` = PROPOSED_NOT_LOCKED: предложено `0.80` для зова легального
  teacher за `Y_goal`; Claude baseline связывает это с `TeacherCallRequest.GoalConfidenceFloor`,
  но число остаётся за Luba / later explicit lock. Не смешивать с Q30/Q22 quality threshold.
- Q18. Куда приземлять обучение пар: новая L5-машина графа или временный мост через
  `AutonomousTrainer.TrainAsync(...dpoPairs)`?

**Блок 07 (DPO-строитель):**
- Q19. `chosen_origin=first_party_deterministic` — оставить как baseline-режим (honest
  scaffold, `semantic_quality_claimed=false`) или запретить и требовать teacher для каждой
  train-ready пары?
- Q20. Пороги `overlap_ratio < 0.60` и `loss < 0.685` — оставить как канонические или
  задать иные для графа?
- Q21. Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») — какой
  канонический перед обучением графа? *(R-18.)*

**Блок 09 (авто-DPO петля):**
- Q22. RESOLVED for numeric value by §E.0/Q30: `qualityThreshold = 0.95`, единый для
  unit/edge/graph. В `teacher_on` дополнительно проверяется измеримый teacher-gap
  (`HeldOutAccuracy(Y_best) >= HeldOutAccuracy(Y_goal) - teacherGapEpsilon`); в `self_best`
  threshold = только `AccuracyAfter >= qualityThreshold`. `teacherGapEpsilon` остаётся
  PROPOSED_NOT_LOCKED/default-pending-pilot, не отдельный Q30 threshold.
- Q23. RESOLVED_IN_PLAN: `self_best` является легальным дефолтом/фолбэком, когда legal teacher
  не настроен или provider unavailable; teacher включается только при настроенном legal-профиле.
  Это mode policy, not a numeric threshold; B-12 winner-selection trace still needs review.
- Q24. RESOLVED/LOCKED_FROM_CLAUDE_RR9_Q24: loop defaults are `maxIterations=20`,
  `plateauWindow=3`, `regressionWindow=3`, `minImprovement=+0.02`,
  `rollbackEpsilon=0.02`, `noSignalWindow=3`, and replay `70/30`.
  `minImprovement=+0.02` is the plateau/default parameter sharing the Q30 delta value,
  not a separate unresolved held-out acceptance threshold.

**Блок 10 (обучение+lifecycle):**
- Q25. `IncrementalTrainer` (14): ведущий вариант B2 — вывести из пути обучения графа
  (канон = `LogicalDpoTrainingUnit` из 18) с пометкой `NOT_ON_GRAPH_TRAINING_PATH`; или B1
  — реальное делегирование в LoRA+DpoTrainer?
- Q26. `IModelServer` scope: достаточно ли `LocalFileModelServer` (локальный файловый
  транспорт весов), а сетевой HTTP — отдельный work-order вне блока 10?
- Q27. Чинить захардкоженный `RewardMargin` (литерал 0.03,
  `27.TranslationDomain/TranslationMachine.cs:70/125/187`) прямо в эталоне в этом блоке,
  или вынести в отдельный блок «эталон обучения»? *(R-19.)*
- Q28. Tatoeba (CC-BY) допустим как источник common-training (не MIT/Apache), или только
  evidence/user-adaptive?
- Q29. Унифицировать ранг LoRA (сейчас 2 vs 8: `DefaultLogicalCapabilityTrainer.cs:858` vs
  `LoraConfig.cs:5`) для узлов графа, или per-tier с обоснованием?

**Блок 11 (измерение):**
- Q30. RESOLVED by §E.0/attachment 483d: held-out `>=0.95`, delta `>=+0.02`, reward `>=0.025`; не спрашивать Luba заново. Implementation/config proof remains Gate B.
- Q31. Строгость e2e-порога графа: E51B требует 6/6 (`e2e_trace_success_rate`); для
  DynamicGraph принять 100% на малом held-out или допустить N-1?
- Q32. Существующий `CrossStackBusinessFlowGraphDpoPair`
  (`ProjectWikiDomainModels.cs:918`) — адаптируется под канон `GraphDpoPair` или канон
  строится поверх доменных пар?
- Q33. Воспроизводимые пути артефактов измерения: часть раннеров пишет в
  `C:/Users/lubao/.xiigen/...` (`Learn06HeldOutAblationRunnerTests.cs:30-33`) — задать
  переносимые фикстуры/repo-пути по R5?

**Блок 13 (тест-стратегия):**
- Q34. Числовые пороги Gate B для узлов/рёбер: переиспользовать пороги перевода
  (`RuntimeProductAccuracy>=0.95`, `RewardMargin>=0.025`,
  `TranslationProductHeldOutRunner.cs:211-220`) или задать отдельный профиль для
  `DynamicGraphDomain`?
  > PASS08 repair: resolved by §E.0/Q30 for the numeric profile. Reuse one profile for
  > unit/edge/graph: `heldout_accuracy_min >=0.95`, `delta >=+0.02`,
  > `RewardMargin >=0.025`. Q35 still governs whether non-transformer wrapper edges may be
  > honest baseline/eval-only instead of full trained TA.
- Q35. `RewardMargin` для не-трансформерных рёбер: требовать вычисленный margin (как
  `11.DPO/DpoTrainer.cs`) для каждого «обученного» ребра, или допускать рёбра-обёртки с
  честной пометкой baseline/eval-only (влияет на разделение колонок UH/TA для P-03)?

**Блок 14 (карта референсов):**
- Q36. PARTIAL_RESOLVED_SOURCE_MAP_GUARD: this is no longer a live priority question and does not
  authorize either route. Current v1 executable pilot remains `language-translate`; R16 design-to-html/css
  and R17 better-output-to-DPO are mapped seam/client surfaces for future approved implementation proof only.
  Future workers must use the source map R21/R22/R23 plus current board/matrix rows, not this old question
  block, before selecting any platform/client route.
- Q37. PARTIAL_POLICY_RESOLVED_IMPLEMENTATION_GATE_REMAINS by section E.0: better-output training
  teacher may be self `ILearningDynamicGraph` or legal hosted open-weight/permissive with provenance/terms;
  closed proprietary/unchecked chatbot outputs are not training targets. Real outputs still need
  C16/provenance/terms/license proof.
- Q38. RESOLVED by section E.0/attachment 483d: generated client outputs stay adaptive/quarantine by
  default; common use only after output-license clearance, redaction, per-client terms snapshot, and Luba
  approval. Do not ask Luba for this value again unless she reopens it.
- Q39. UNRESOLVED_REFERENCE_DEPTH_QUESTION_NOT_IMPLEMENTATION_AUTHORITY: decide whether to unpack
  `extensions/plugin-plans-*.zip` for Claude/Cursor/Codex hook details or whether
  `codex-recon-mvp-extensions.md` is enough. This is reference-depth/navigation evidence only; it cannot
  authorize product implementation, host selection, readiness, or route execution.
- Q40. PARTIAL_RESOLVED_CLIENT_SURFACE_GUARD: `CanvaToCode` remains a client/caller surface like
  `FigmaToCode` for the R16 client-host evidence path only. It is not platform-host/facade authority, not
  the `xiigen-platform` or `xiigen-flows` repo, and not active E2E pilot proof. Optional/full activation
  remains future source-map/implementation evidence after fresh approval; no current worker may infer
  implementation authorization from this legacy question block.

## E.3. Сквозной вопрос охвата референсов (R15-scope)

**Q41 (R15-scope). RESOLVED 2026-06-30: один активный пилот `language-translate`.**

Claude board-owner фиксирует Q41=Вариант B не как открытый выбор, а как закрытую
активацию ровно одного пилота: `language-translate`. Поэтому текущий baseline доски:
12 фаз / 27 подфаз / 64 подшага; Ф10.3.1..Ф10.3.5 входят в baseline-64 со
статусом `remaining`; `blocked=0`. Старый вариант «6 blocked-слотов вне baseline-59»
сохраняется только как historical/pre-PASS26 evidence-only и не является текущим scope.

```text
R15 current execution scope = active pilot language-translate
pilot_rows = Ф10.3.1..Ф10.3.5
other_R15_domains = navigation/reference-only: small-model, local-wiki, extensions, documentation-learning, session-start
Q41_open = false
```

| Домен R15 | Текущий статус после RR4/Q41/PASS26 | Что остаётся |
|---|---|---|
| `language-translate` | **active pilot**; Ф10.3.1..Ф10.3.5 baseline-64; эталон margin/lifecycle | реализация/тесты после approval, не вопрос Q41 |
| `small-model` | navigation/reference-only | source-map/backing clarity, не строка текущей доски |
| `local-wiki` | navigation/reference-only | source-map/backing clarity, не строка текущей доски |
| `extensions` | navigation/reference-only | source-map/backing clarity, не строка текущей доски |
| `documentation-learning` | navigation/reference-only | source-map/backing clarity, не строка текущей доски |
| `session-start` | navigation/reference-only; backing gap честно отмечен | найти/указать backing или оставить как gap, не reopen Q41 |

**Domain E2E scope-honesty guard:** current v1 has exactly one active executable pilot:
`language-translate`. All other R15/reference/client surfaces are navigation,
reference, or deferred until their own activation bundle exists. A recon file,
generic per-domain template, Source-to-Plan inventory row, or future gate is not
data/scorer/legal/lifecycle proof and must not be read as E2E readiness.

| Domain/surface | v1 execution status | Required bundle before any E2E-ready claim |
|---|---|---|
| `language-translate` | active executable pilot after Luba approval; bundle is designed, but seed file/tests/training/runtime proof are not created here | Use the active pilot bundle: `SeedCorpus-LanguageTranslate-v1`, train/held_out split, held-out `expected_label` gold, exact-match + char-accuracy + `RuntimeProductAccuracy`, legal/provenance gates, unit-level DPO, checkpoint/export/import/fresh-load/continue-training lifecycle. |
| `local-wiki` | navigation/reference/deferred; not E2E executable in v1 until activation bundle exists | Domain seed/query corpus, source snapshots, held-out question/gold-answer scorer, legal/provenance manifest, activation substeps, lifecycle/checkpoint/fresh-load gates, and tests. Recon files alone are evidence-only. |
| `small-model` | navigation/reference/deferred; not E2E executable in v1 until activation bundle exists | Model/source/license proof, seed/data rows, held-out scorer/gold, provenance/legal verdict, activation substeps, lifecycle/checkpoint/rollback gates, and tests. Generic model templates are not E2E proof. |
| `extensions` / host clients | navigation/reference/deferred; not active pilot proof | Explicit extension/client activation plan, source/handler proof, same-scenario Path A + Path B evidence, redaction/provenance rules, held-out scorer or domain acceptance metric, lifecycle/promotion policy, and tests. |
| `figma-html-css` / `design-to-code` | R16 seam/reference surface only; not active pilot proof | `FIGMA_CANVA_DESIGN_TO_HTMLCSS` contract plus concrete platform/FLOW-31/client-host proof, same-scenario join evidence, legal/client-output quarantine, scorer/acceptance metric, activation substeps, lifecycle/promotion gates, and tests. |
| `FigmaToCode` / `CanvaToCode` | client/caller surfaces, not platform-host authority and not active E2E pilot evidence | Future proof must show exact client adapter/host entrypoint, source snapshot, Path B screenshots/logs, provenance/quarantine handling, and join to platform/FLOW-31 evidence; local folder existence is not authority. |

**Эталон:** `language-translate` остаётся эталоном margin/lifecycle для всего графа:
реальный `RewardMargin` берётся из движка (`11.DPO/DpoTrainer.cs`), а не из литерала;
lifecycle (checkpoint/export/import/continue-training, held-out+ablation+fresh-load)
калибруется по доказанному пути перевода. Остальные R15-домены не становятся
исполняемыми подшагами без нового явного решения Luba.

---

*Конец улучшенного мастер-плана ILearningDynamicGraph v1 (namespace claude-*). PLAN-ONLY:
документация-решение, индексирующая 16 логических блоков (15 файлов `codex-plan-*` как
evidence-источник; §F индекс артефактов). Источник истины по execution-control —
claude-доска `claude-EXECUTION-CONTROL-BOARD.md` (`Ф<P>.<SP>.<SS>`, §D); трассировка
требований — claude-матрица `claude-REQUIREMENTS-COVERAGE-MATRIX.md` (§D.2). Бенчмарк
`codex-*` — evidence-only. Не разрешает реализацию
продукта/кода, тренировку, билд, запуск раннеров, git или вызов внешних моделей.
Реализация по доске исполнения требует свежего явного approve Luba после ревью этого
плана (R2). Цитаты кода — только реально существующие, с `file:line`, сверены прямым
чтением `C:/Projects/llm_mvp_core`. NEW-типы помечены; существующие — с `file:line`. Чужие
`codex-*`/`dcm-*` и старый каркас `claude-00..07` — evidence-only.*
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

## PASS17 B-22 Claude-Baseline Reconciliation Gate

Historical/pre-Pass39 PASS17 records B-22 as an active plan gate: active-codex must contain at least Claude's details, references, explanations, and authority decisions before readiness. Codex material can only be a validated delta/source-map input. Newer ledgers, larger active files, extra status tables, or copied workdir discoveries cannot make active-codex better than Claude by themselves. Current B-22 blocker-inventory classification is the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`, not this old snapshot.

Current PASS17 classifications:

| category | active-vs-Claude status | plan consequence |
|---|---|---|
| master plan, execution-control board, requirements matrix | `ACTIVE_HAS_DELTA` | Active copies exist and include repair deltas; each delta must preserve Claude baseline details or cite an accepted/Luba-approved reason. |
| plan blocks 00-14 and guides 01-05 | `ACTIVE_HAS_DELTA` | Historical/pre-Pass39: counterpart files existed, but PASS17 had not proven semantic equality, so B-22 remained open then. Current B-22 blocker-inventory status is Pass39 `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`, not readiness, not implementation authorization, and not execution proof. |
| library files 00-07 | `ACTIVE_HAS_DELTA` | Active 00/02 now carry B-22 map text; remaining library files still need direct drift review. |
| Codex recon/knowledge library | `NO_ACTIVE_COUNTERPART_FOUND` for active execution recon files | Codex recon files stay evidence-only until exact accepted rows are mapped by B-21/B-22. |
| branch/repo authority | `ACTIVE_HAS_DELTA` | Claude baseline limits implementation scope to five named repos and `DPOLegalLearning`; planning repo and copied folders are evidence-only. |
| legal/provider/provenance | `ACTIVE_HAS_DELTA` | Claude legal teacher/provenance decisions remain baseline; active PASS12/PASS16 deltas refine but cannot loosen quarantine/train-ready gates. |
| DPO unit-level pair model | `ACTIVE_HAS_DELTA` | Active unit-level train-ready policy is a current repair delta; no graph-level train-ready shortcut is allowed. |
| host-route/Q-host | `ACTIVE_HAS_DELTA` | Route design is specified, physical implementation proof remains unproven and belongs to later B-19/B-4 evidence. |
| B-1..B-22 blocker inventory | `ACTIVE_HAS_DELTA` | The inventory is explicit but still open; B-22 is not closed by this pass. |

Readiness rule: B-22 can close only when every category is `ACTIVE_MATCHES_CLAUDE_BASELINE` or has an explicit accepted/Luba-approved delta with source evidence and no lost Claude detail. Until then the plan verdict remains `PLAN_NOT_READY` and implementation authorization remains NO.

Pass18/50 latest-Claude SR-id addendum: current inspected Claude plan-14 assigns `SR-17` to the `language-translate` seed input-prompt corpus. Active plan-14 and searched consumers now reserve current `SR-17` for that seed-corpus meaning; older active LogicalContracts anchors and row references use `SR-17-legacy-active-logicalcontracts` as a historical/local alias. The Pass17 `ACTIVE_LAGS_CLAUDE_FOR_SR17_ID_SPLIT` marker is historical for this repaired slice after recheck. This is not readiness and not implementation authorization; the 64 Source-to-Plan rows remain inventory/source-map evidence only.

### Pass 17 Source-to-Plan 64/B-22 Reconciliation Contract

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass 17 does not claim `64/64`, B-22 closure, plan readiness, Claude Repair2 acceptance, or implementation authorization. Source-to-Plan target 64 remains a required denominator, not a completed count.

Future repair must enumerate Source-to-Plan rows with these acceptance states:

| state | row meaning | owner/routing |
|---|---|---|
| `MAPPED_VERIFIED` | Active row, Claude baseline row, accepted delta status, and downstream consumers agree. | Source map owner verifies, then plan-14/board/matrix/coverage consumers mirror. |
| `MAPPED_ACCEPTED_DELTA` | Active row intentionally differs from Claude and cites Luba-approved or accepted Codex delta evidence. | Source map owner records delta reason; B-22 reviewer confirms no lost Claude detail. |
| `PARTIAL_NEEDS_OWNER_REVIEW` | Category, support group, or pilot row exists but is not an enumerated 1-of-64 row closure. | Route to owner file named in the row; do not count as completed. |
| `MISSING_ACTIVE_COUNTERPART` | Claude/latest-baseline row has no active-codex counterpart. | Route to active owner plan before B-22 closure. |
| `STALE_HISTORICAL_ONLY` | Older counter/status remains as evidence only and is not current authority. | Label historical/pre-repair near the stale text or replace with current verdict. |
| `CLAUDE_REPAIR2_UNVERIFIED` | Claude-side/latest-baseline workflow is quoted as pending/running/uninspected. | Keep evidence blocked until exact output and changed Claude files are read. |
| `NEEDS_CODE_REF_INVENTORY` | Row needs implementation code anchors before later execution proof. | Route to plan-14/code-reference inventory; this is not implementation authorization. |

Current Pass 17 routing boundary: existing active-codex contains category support rows and pilot/source-map anchors, but Pass 17 did not enumerate and verify all 64 Source-to-Plan rows. Therefore `64/64 not claimed by Pass 17`; B-22 remains open until every latest Claude baseline category and accepted Luba/Codex delta is reconciled with these statuses and every downstream consumer agrees.

### Pass 19 Source-to-Plan Row-Evidence Boundary

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass18 enumerated the Source-to-Plan baseline-64 denominator roster in `active-codex-execution-plan-14-stage-references.md` section 3.2 so future implementers can inspect one row per baseline substep. Pass22 upgraded Ф1.1.1 through Ф1.3.3 at planning/source-map level; Pass23-F2 upgraded only Ф2.1.3, Ф2.2.1, and Ф2.2.2; Pass24 upgraded only Ф3.1.3, Ф3.2.1, and Ф3.2.2; Pass25 upgraded only Ф4.1.1, Ф4.1.2, Ф4.2.1, Ф4.2.2, Ф4.2.3, Ф4.3.1, and Ф4.3.3; Pass26 upgraded only Ф5.1.1, Ф5.1.2, Ф5.1.3, Ф5.2.1, and Ф5.2.2; Pass27 upgraded only Ф6.1.1, Ф6.1.2, Ф6.2.1, and Ф6.2.2; Pass28 upgraded only Ф7.1.1, Ф7.1.2, Ф7.2.1, and Ф7.2.2; Pass29 upgraded only Ф8.1.1, Ф8.1.2, Ф8.2.1, and Ф8.2.2; Pass30 upgrades only Ф9.1.1, Ф9.1.2, Ф9.2.1, Ф9.2.2, Ф9.3.1, and Ф9.3.2 at planning/source-map level and mirrors the exact current inventory state: `MAPPED_VERIFIED = 0`, `MAPPED_ACCEPTED_DELTA = 64`, `PARTIAL_NEEDS_OWNER_REVIEW = 0`, `NEEDS_CODE_REF_INVENTORY = 0`, and `ROSTER_ONLY_NEEDS_SOURCE_REF = 0`. This is source-map inventory evidence only, not readiness.

Pass20 continuation rule: keep B-22 open and keep implementation authorization `NO` until each of the 64 rows has direct source evidence, active owner/source-map acceptance, downstream consumer mirrors, and changed/current readback evidence. The four seam rows are accepted only as plan/source-map contracts: R16 has SR-16 in-repo core backing with future platform/FLOW-31/client-host proof, and R17 has SR-14 `NEW_PLANNED_CONTRACT` with mock/unit/replacement obligations but no physical implementation proof.

Pass21 continuation rule: Ф11.1.1 and Ф11.2.1 are accepted only as final plan-control rows. Ф11.1.1 still requires a future filled completion matrix with execution evidence and 0 `NOT_PROVEN`; Ф11.2.1 still requires ten real independent execution-review PASS packets plus exact `ARCHITECT_VERDICT_ALLOW_FINAL`. No review count, final verdict, B-22 closure, readiness, `64/64`, product proof, provider/API/network proof, tests/builds/runtime/browser proof, or implementation authorization is claimed by this mapping.

Pass25 continuation rule: Ф4 trained-state/lifecycle rows are accepted only as source-map planning deltas. Existing source anchors prove LoRA/DPO/model-server/lifecycle precedents; `LoraApplyResult`, `LocalFileModelServer`, `TeacherProvenance`, `GraphNodePreferenceState`, and `DynamicGraphMachinePair` remain `NEW_PLANNED_CONTRACT`/planned bridge names until physical code is implemented and verified in a later authorized phase.

Pass26 continuation rule: Ф5 measurement/quality/lifecycle-probe rows are accepted only as source-map planning deltas. Existing source anchors prove metrics/evaluation/export/import contracts, live DPO metric boundaries, quality/leakage/quarantine precedents, and scoped fresh-load/held-out probe precedents; `GraphMeasurementReport`, `IGraphMeasurementRunner`, `IHeldOutScorer`, `GraphQualityMeter`, and `GraphLifecycleProbe` remain `NEW_PLANNED_CONTRACT`/planned bridge names until physical code and execution evidence are implemented and verified in a later authorized phase.

Pass27 continuation rule: Ф6 async-combination rows are accepted only as source-map planning deltas. Existing source anchors prove bounded branch/dependency parallelism, branch-result collection, arbiter-constraint precedent, and unit-level learning contracts; Claude baseline owns the planned `IGraphCandidateRunner`/`IGraphCombinationStrategy`/`CombinationPlan`/`GraphCandidateOutput`/judge/legal/pair-assembler contracts. `GraphAsyncBranchResult`, `GraphManyToManyRoute`, `IEdgeFanInMerger`, `ICombinationJudgeUnit`, `TeacherGoalGate`, and `GraphDpoPairAssembler` remain `NEW_PLANNED_CONTRACT`/planned bridge names until physical code and execution evidence are implemented and verified in a later authorized phase.

Pass28 continuation rule: Ф7 auto-DPO-loop rows are accepted only as source-map planning deltas. Existing source anchors prove ResultGap/low-result precedent, unit-level `LearningTrainingItem`/`TrainBatchAsync`/`EvaluateAsync` contracts, held-out/source-disjoint and RewardMargin metric anchors, replay-buffer precedent, and promotion-threshold precedent; Claude baseline owns the planned weakness detector, legal teacher goal, `GraphDpoPair`, `TeacherProvenance`, `AutoDpoLoopOrchestrator`, `ShouldStop`, no-signal, replay, and promotion contracts. `GraphDpoPair`, `TeacherProvenance`, `AutoDpoLoopOrchestrator`, `IGraphWeaknessDetectorUnit`, `ITeacherGoalProviderUnit`, `IGraphDpoPairBuilderUnit`, graph `ShouldStop`, no-signal marker, and accepted-auto-pair counter remain `NEW_PLANNED_CONTRACT`/planned bridge names until physical code and execution evidence are implemented and verified in a later authorized phase.

Pass29 continuation rule: Ф8 no-hidden-determinism rows are accepted only as source-map planning deltas. Existing source anchors prove deterministic/baseline mechanisms, SmartSkills candidate-order guard precedent, latest-wins registry precedent, metric/lifecycle/promotion anchors, and leakage/ablation precedents; Claude baseline owns the planned edge-authority replacement, `SemanticEdgeRouterUnit`, `SemanticEdgeRoutingState`, `EdgeAuthorityInput`, edge `CandidateOrderLeakageGuardUnit`, `IEdgeCandidateProvider`, excluded-leakage rules, latest-wins cutover gate, and ablation threshold. Mechanism #4 `OrchestrationScenarioPhasePlanner` is explicitly classified, not hidden as a future gate: v1 keeps it labelled baseline/eval with no trained claim. Physical DynamicGraph cutover, hidden-determinism elimination, training, held-out proof, ablation run, fresh-load, tests/builds/runtime/provider/API/network/git, product readiness, and implementation authorization remain future evidence.

Pass30 continuation rule: Ф9 consolidated test-strategy rows are accepted only as Source-to-Plan planning/test-design deltas. Existing source/test anchors prove topology mutation, path-free scoring/leakage warnings, DPO primitives, provenance/quarantine precedents, lifecycle metadata probes, and governance gates as anchors only. Future tests must still implement and run topology mutation fan-out/fan-in/many-to-many cases, edge determinism/leakage/ablation cases, DPO generator plus 20-field `TeacherProvenance`/closed proprietary `DATASET_REBUILD_REQUIRED` cases, continue-training merge/duplicate/empty-state cases, component-to-layer Gate matrix assertions, and regression/anti-loophole checks. No source code was edited and no tests/builds/runtime/provider/API/network/git/training ran.

Pass22 continuation rule: Ф1.1.1 through Ф1.3.3 are accepted only as DynamicGraph planned-contract/source-inventory rows. Existing L2/L3/L4/L5 and graph/topology/machine precedents are line-read support, while `DynamicGraphDomain`, `IDynamicGraphNode`, `IDynamicGraphEdge`, `GraphEdgeKind`, `DynamicGraphTopology`, `DynamicGraphEdgeFlow`, `IEdgeCandidateProvider`, `IEdgeFanInMerger`, and `DynamicGraphMachine` remain `NEW_PLANNED_CONTRACT`; no physical DynamicGraph implementation, test execution, runtime proof, B-22 closure, readiness, `64/64`, or implementation authorization is claimed.

Pass23-F2 continuation rule: Ф2.1.3, Ф2.2.1, and Ф2.2.2 are accepted only as DPO/legal source-inventory rows. `GraphDpoPair`, `TeacherProvenance`, and `TeacherProvenanceGate` remain `NEW_PLANNED_CONTRACT`; existing DPO primitives, five source formats, projection/materialization, feedback-DPO, and quarantine/license anchors are line-read support. `sonnet-*` contamination still requires implementation-time live scan/rebuild evidence; no physical class proof, test execution, runtime proof, B-22 closure, readiness, `64/64`, or implementation authorization is claimed.

Pass24 continuation rule: Ф3.1.3, Ф3.2.1, and Ф3.2.2 are accepted only as DPO-builder source-inventory rows. `GraphDpoBuilder`, `GraphDpoSchemaValidator`, `GraphDpoPair`, and `GraphDpoSourceRow` remain `NEW_PLANNED_CONTRACT`; existing anti-leakage/schema/quarantine, near-miss/`[WRONG:]`, source-id/provenance, and `LearningTrainingItem`/`DpoPair`/`DpoDataset` anchors are line-read support. No `BuildRejected` execution, materialization execution, test execution, runtime proof, B-22 closure, readiness, `64/64`, or implementation authorization is claimed.

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
## Sequential Pass 05 R-OT-05 Source Inventory Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass 05 repairs source-inventory/reference-propagation visibility only. Active-codex now carries the R-OT-05 contract that planning docs live in `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build`, original reference roots are `C:\Users\lubao\Documents\xiigen\llm mvp` and `C:\Projects\llm_mvp_core`, and `C:\Projects\llm_mvp_core` is the core project/source-root for DynamicGraph/DPO work.

Future implementation repo authority remains the Claude baseline list: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. FigmaToCode/CanvaToCode are client/caller surfaces only; they do not replace xiigen-platform, xiigen-flows, or `llm_mvp_core` authority. Future work copies/uses the latest updated branch from each repo under `C:\Projects\DPOLegalLearning\<repo>` / `DPOLegalLearning`, runs no-write mergeability/no-conflict checks before later commit/push against the original authority branch and `main` where project rules require it, and creates no PR/merge without separate future Luba instruction.

Recon/library coverage remains not proven done by this pass. The required `20-pass` over `llm mvp` and `40-pass` over `C:\Projects\llm_mvp_core` stay as pre-implementation/reference gates unless exact focused-pass records exist. Reference propagation rule: do not ask Luba or stop on a source question that Claude baseline, active-codex, Codex evidence-only files, or source files already answer; downgrade it to resolved/partial with exact evidence first.

## PASS27 Original-Task Coverage And Latest Baseline Reconciliation

## Sequential Pass 06 R-OT-06 DPO Lifecycle Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This overlay covers only R-OT-06: low-result discovery, better-target generation, teacher/judge boundary, unit-level DPO, train/evaluate loop, measurement gates, and stop/continue criteria.

Implementation-facing contract now required by the active plan:

- `low-result` / `weakness`: detect weak unit outputs from async candidate runs plus held-out scorer feedback. `IGraphWeaknessDetectorUnit` selects `Y_best` from actual runtime candidates and flags rejected material from low confidence, failed verdict, held-out miss, or judge/scorer weakness. It does not synthesize trainable chosen text.
- Better target: `self_best` uses a real `Y_best` candidate; `legal teacher` uses hosted open-weight/permissive output only after `TeacherProvenance` and provider/license terms pass. Closed proprietary outputs are `judge-only` / eval-only and never train targets.
- Boundary: `JudgeProvenance` may support ranking/scoring/flagging; `TeacherProvenance` proves a legal trainable goal. Required row gates are `judge_did_not_author_chosen == true` and `chosen_equals_runtime_candidate_or_legal_goal == true`.
- `GraphDpoPair`: train-ready pairs remain unit-level but use the exact active/Claude plan-07 14-field schema: `PairId`, `Level`, `NodeOrEdgeId`, `InputContext`, `Chosen`, `Rejected`, `WrongMarkers`, `Reason`, nested `TeacherProvenance`, `SourceIds`, `HeldOut`, `CandidateOrderInvariant`, `ExcludedLeakageSignals`, `DpoTrainReady`. Path/order/split/legal/judge diagnostics stay in provenance/triad/trace records, not as extra pair fields. Graph-level/blob DPO remains forbidden as train-ready.
- Training and evaluation: accepted rows feed the block-10 training lifecycle, then block-11 measurement on source-disjoint `held-out` split with `ablation`, reward/accuracy/speed/quality metrics, lifecycle/fresh-load proof, and promotion only on held-out improvement.
- `ShouldStop`: `qualityThreshold=0.95`; `teacherGapEpsilon=0.02` per Claude RR9/Q22/Q30. In `teacher_on`, threshold requires both held-out quality and teacher-gap closure. In `self_best`, threshold is held-out quality only. Other stop reasons are `plateau`, `budget`, and `no_signal`; otherwise continue. Open-ended training wording is not an active contract.
- External calls: future implementation must use mock expected response plus unit test first; later real-call replacement may be tracked only in `codex-external-calls-mock.md`. This planning pass made no provider/API call and ran no tests.

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. See `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md` for the controlling latest-baseline reconciliation context. Current active-codex canon after latest-baseline reconciliation plus Pass30 Ф9 consolidated test-strategy source-map repair: 12 phases / 27 subphases / 64 substeps; `TeacherProvenance` 20 fields (13 base + 5 NEW + 2 RR6 `service_mode`/`grounding_used`); exactly one active pilot `language-translate`; Source-to-Plan inventory rows are 64/64 accepted as source-map inventory evidence only; hosted Qwen3/Together train use remains blocked until provider terms proof; held-out scorer defaults to `RuntimeProductAccuracy` plus per-domain scorer (`language-translate`: exact-match + char-accuracy). Any older unqualified 12/26/59, 18-field, 8/64 (partial; historical/pre-PASS27 corrected from 8/59), 28/36, 31/33, 34/30, 41/23, 46/18, 50/14, 54/10, 58/6, incomplete-source-inventory, or narrower ready wording in this file is historical/pre-0aa evidence only.

## Sequential Pass 07 R-OT-07 Async Candidate / Order Handoff Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This overlay covers only R-OT-07: async combinations, `N`/cardinality, candidate batch shape, graph-order/traversal handoff, fan-out/fan-in many-to-many merge, and unit-level DPO feed.

Implementation-facing contract now required by the active plan:

- Request/cardinality: `GraphCandidateRunRequest` is the single async candidate-run request. `N == combinationCount == request.CombinationCount`; default `4` and ceiling `8` are `LOCKED_BY_LUBA_2026_06_30`. The runner rejects out-of-bounds cardinality, records requested-vs-actual, and never pads with duplicate candidates.
- Async execution and candidate batch: the runner enumerates `CombinationPlan[]` with variation axes for unit set, legal graph/traversal order, and execution flags; each unique plan runs asynchronously through the L4/L3 route engine and returns `AsyncCandidateBatch`.
- Candidate shape: public many-to-many output remains `IQueryable<GraphNodeCandidate>` or equivalent queryable semantic surface until the runner materializes `IReadOnlyList<GraphNodeCandidate>` snapshots in `GraphCandidateOutput.OutputRefs`. `GraphNodeCandidate`/`GraphCandidateOutput` carry candidate/unit/logical-unit refs, typed output refs, confidence/score, `CombinationId`, provenance refs, legal/status/error fields, `path_order_ref`, source ids, and trace refs. `NO_OBJECT_OUTPUT`: raw `object Output`, untyped arrays, and whole-graph blobs are not canonical.
- Fan-out/fan-in/merge: fan-out branches may be parallel or dependency-ordered; fan-in merge preserves candidate/branch inputs, merge key, conflicts, rejected alternatives, failure modes, `LOW_CONTRAST`, `no_signal`, legal/provenance refs, `path_order_ref`, and trace refs. Merge output is candidate/decision metadata only; it is not a hidden winner picker and not a DPO row writer.
- Graph order handoff: deterministic/topological traversal is a correctness/safety baseline and candidate enumerator. `GraphOrderResolverUnit` is the trainable/adaptive unit that emits `GraphOrderDecision(pattern_id, ordered_unit_ids[], ordered_path_refs[], skipped_edges[], rejected_edges[], legal_status, provenance_status, path_order_ref, trace_ref, reason_codes[])`.
- DPO feed: `CandidateSelectionInput` passes `AsyncCandidateBatch` plus graph-order/provenance/trace metadata into B12 winner selection and B6/B7/B10/B12/B13 gates. Train-ready output can only become the exact 14-field `GraphDpoPair` with `Level="unit"` and unit id in `NodeOrEdgeId`; graph/order/path/fan-out/fan-in data stays provenance/triad/trace context through `path_order_ref` and refs. Whole graph A vs whole graph B is not train-ready DPO.

This Pass 07 overlay made no product implementation, C# edit, test/build/runtime probe, browser check, external provider/API call, screenshot, git action, or readiness claim.

## Sequential Pass 08 R-OT-08 External API Mock Seam Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This overlay covers only R-OT-08: external API/resource seams, mock expected responses, future unit-test obligations, and later mock-replacement debt tracking.

Implementation-facing contract now required by the active plan:

- Seam inventory: every external API/resource path named by the plan must use a named seam/interface/adapter before implementation. Current seam ids are `FIGMA_CANVA_DESIGN_TO_HTMLCSS`, `BETTER_OUTPUT_FEEDBACK_TO_DPO`, `LEGAL_TEACHER_JUDGE_PROVIDER`, and `PROVIDER_TERMS_LICENSE_METADATA`; `MOCK_REPLACEMENT_LEDGER` is the later record target, not an implementation seam.
- Per-seam obligation: each seam must have request fields, response fields, a mock expected response, a unit test asserting the exact mock response/result, legal/provenance gates, failure/quarantine behavior, and a later replacement plan entry target.
- R16 design-to-html/css: platform facade, FLOW-31 design analysis, and FigmaToCode/CanvaToCode client generation stay separate. The deterministic html/css mock remains quarantined/client-unverified unless positive output-license, redaction, terms, provenance, and approval evidence exists.
- R17 better-output-to-DPO: `GeneratedUiOutputFeedbackEvent` through FLOW-38/core adapter must normalize to a quarantined/personal `GraphDpoSourceRow` when provenance/terms/license proof is missing. It cannot create a common/train-ready `GraphDpoPair` from unverified client or third-party output.
- Legal teacher/judge provider: future `ITeacherGoalClient` and `IJudgeScoreClient` implementations start with mocks. Hosted open-weight teacher output is train-eligible only with provider terms plus model license proof for the exact scope and complete `TeacherProvenance`; closed proprietary providers are judge/eval/critique only. Closed output promoted to training returns `DATASET_REBUILD_REQUIRED`.
- Provider terms/license metadata: `ProviderProof` and `LicenseProof` are proof-resource seams. Pending, missing, negative, contradictory, or mock-only proof routes to `QUARANTINE`, eval-only, block, or `DATASET_REBUILD_REQUIRED`; it is not a Luba runtime stop and not train-ready evidence.
- Mock debt boundary: replacement of mocks with real external/resource calls may be recorded only after product readiness and future approval in `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`. This is the only allowed technical debt; this Pass 08 does not create or edit that file.

This Pass 08 overlay made no product implementation, C# edit, test/build/runtime probe, browser check, external provider/API/resource call, screenshot, git action, mock-debt file creation, or readiness claim.

## Sequential Pass 10 R-OT-10 Human-Readable Implementation Documentation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This overlay covers only R-OT-10: the active-codex implementation documentation packet must be readable by a future implementer and must expose UML, status, algorithm, code refs, hierarchy, stage/subphase/substep references, Gate B criteria, guides, no hidden subtasks, no hidden deterministic decisions, no L6, and implementation proof pending boundaries.

Important disambiguation: older `PASS10` text in this library is the B-8/B-9 many-to-many output-shape repair. This section is the current sequential Pass 10 R-OT-10 documentation-completeness overlay. It does not rename or upgrade the older output-shape pass.

Implementation-facing packet map:

| R-OT-10 slice | Active owner files | What the future implementer must be able to find | Evidence boundary |
|---|---|---|---|
| UML + status | `active-codex-execution-plan-00-partA-uml-status.md`, plus local UML/status sections inside plans 03-11 and guides 02-03 | Component map, owners, existing vs NEW status, implementation gaps, and no fake completed code | Existing active docs are plan evidence only; implementation proof pending. |
| algorithm | Plans 03, 04, 06, 07, 08, 09, 10, 11 | Orchestrator async route, DynamicGraph L5 training, async combinations, DPO pair building, legal gate, auto-DPO loop, training lifecycle, measurement, promotion/checkpoint | Algorithm text is a plan contract, not runtime/test proof. |
| code refs / line / quote | Plan-00, plan-07, guides 01-04, and any owner file that already carries `file:line` evidence | Exact repo/path/line anchors and short quote only when already proven in the owner doc; otherwise mark `NEEDS_CODE_REF_INVENTORY` | This Pass 10 did not inspect product source lines and must not invent line refs or quotes. |
| hierarchy | `active-codex-execution-plan-05-partA-hierarchy.md`, guide-04 | `ILearningMachine` / `ILearningDynamicGraph` L5 -> `ILogicalOrchestrator` L4 -> `ILogicalFlow` L3 -> `ILogicalUnit` L2; no L6; graph/path/order metadata stays context for unit-level DPO | No hidden graph-wide DPO row and no hidden deterministic final authority. |
| stage / subphase / substep / Gate B | `active-codex-execution-plan-14-stage-references.md`, `active-codex-execution-EXECUTION-CONTROL-BOARD.md`, `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md` | Every implementation-facing subtask points to a phase, subphase, substep, dependencies, owner files, and Gate B proof criteria | Future Gate B can require tests/builds/runtime proof; it cannot hide unresolved architecture decisions. |
| guide | `active-codex-execution-guide-01-how-to-build-dpo.md`, guide-02 train, guide-03 measure, guide-04 pattern/graph | How to build DPO, train, measure speed/accuracy/quality, and apply the ILearningMachine/DynamicGraph pattern | Guides are instructions, not evidence that product code already exists. |

Mental simulation result for this overlay: a future implementer can start at this master overlay, route to the UML/status owner for component state, route to the algorithm owners for step-by-step behavior, route to the hierarchy owner for L5/L4/L3/L2 boundaries, route to plan-14/board/matrix for stage/subphase/substep and Gate B proof, and route to guides 01-04 for build/train/measure/pattern instructions. If a needed file/path/line quote is not already proven in the active owner documents, the implementer must record `NEEDS_CODE_REF_INVENTORY` before coding instead of inventing a line or treating prose as source proof.

No hidden-subtask rule: no R-OT-10 implementation subtask may be left only in prose such as "do later", "wire later", "choose later", or "figure out during implementation". It must appear in an owner file plus board/matrix/stage reference, or it remains a plan blocker. No future gate may hide provider choice, input/output contract, hierarchy level, code-ref inventory, provenance policy, measurement threshold, model/checkpoint lifecycle, or deterministic-vs-trainable authority.

This Pass 10 overlay made no product implementation, C# edit, test/build/runtime probe, browser check, external provider/API/resource call, screenshot, git action, code-line verification, or readiness claim.

## Sequential Pass 11 R-OT-11 Source-to-Plan And Blocker Inventory Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

This Pass 11 overlay controls source-to-plan and B-1..B-22 traceability for active-codex consumers. It does not close readiness, does not authorize implementation, and does not claim any product proof.

Source-to-Plan current status:

| item | current active-codex status | boundary |
|---|---|---|
| target | `target 64` baseline substeps after RR4/Q41/PASS26 | The target is a plan coverage requirement, not implementation proof. |
| actual | Pass18 enumerates `source_to_plan_roster_rows = 64`; Pass30 records `MAPPED_VERIFIED = 0`, `MAPPED_ACCEPTED_DELTA = 64`, `PARTIAL_NEEDS_OWNER_REVIEW = 0`, `ROSTER_ONLY_NEEDS_SOURCE_REF = 0`, `NEEDS_CODE_REF_INVENTORY = 0`, and `actual_64_of_64 = accepted_as_source_map_inventory_only` | The roster is fully accepted as Source-to-Plan inventory evidence only; this does not prove implementation, tests, runtime, B-22 closure, readiness, or authorization. |
| status | `source-map inventory complete; PLAN_NOT_READY was the historical/pre-post-repair status at that pass` | The next readiness workflow must still reconcile remaining design/dependency blockers and receive Luba approval before implementation. |
| stale-count guard | older `59`, `51 remaining`, `8/59`, or unqualified `8/64` wording is historical/pre-PASS11 trace evidence only | A later count may replace this only with direct row evidence. |

B-1..B-22 current inventory owner: `active-codex-execution-16-full-task-coverage-simulation-gate.md` Pass39 table plus Pass43 B-18/B-21 default-defer supersession. Older Pass11/Pass31 rows are historical/evidence-only where they conflict with the current table. Every blocker number B-1 through B-22 is present there with current classification: most rows are resolved in active plan with execution proof pending; B-2 remains the hosted-provider/Luba approval proof residual before hosted train/common use; B-18 is not a current v1 blocker because v1 is common-only and adaptive is deferred unless Luba later expands scope; B-21 is not a current v1 blocker because first substantive recon plus source-map inventory is sufficient for current v1 planning unless Luba later reopens required depth. This does not make the plan ready, does not close the required 50-pass chain, and does not authorize implementation.

Future-gate rule for this overlay: future gate can require tests, builds, runtime probes, screenshots, mergeability checks, or proof collection after approval. A future gate cannot hide a design decision, missing Source-to-Plan row, missing blocker number, unresolved owner contract, provider choice, provenance policy, threshold, model/checkpoint lifecycle, or active-vs-Claude reconciliation. `PLAN_NOT_READY` is a repair queue, not a stop.

## Pass 15 Test/Build/Mock Verification Continuation Gate

Status: `PLAN_NOT_READY`; implementation authorization remains `NO`. This Pass 15 section is plan/governance repair only. It did not implement code, create test files, run tests, run builds, run runtime probes, call providers/APIs/network, inspect browser/host routes, create `codex-external-calls-mock.md`, use git, or prove product readiness.

Future implementation may claim product-ready only after recording exact evidence for all of these categories:

1. Build evidence: the selected implementation solution/project builds after code exists, with command, exit code, and output summary recorded.
2. Unit-level GraphDpoPair semantics: unit tests prove train-ready `GraphDpoPair` material uses the exact 14-field canon with `Level="unit"` and unit id in `NodeOrEdgeId`, nested 20-field `TeacherProvenance`, and no whole-graph train-ready shortcut; path/order/legal/split diagnostics remain provenance/triad/trace evidence, not extra pair fields.
3. External seam mock responses: every external API/resource seam, including `BETTER_OUTPUT_FEEDBACK_TO_DPO`, `LEGAL_TEACHER_JUDGE_PROVIDER`, `PROVIDER_TERMS_LICENSE_METADATA`, and R16/R17 host/resource routes, has a mock request, exact expected mock response/result, unit test name, and assertion against that exact result. `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md` is the only future replacement ledger and the only allowed technical debt.
4. Legal/provenance tests: provider policy, C16/C17 routing, 20-field `TeacherProvenance`, ProviderProof, LicenseProof, terms snapshot, service-mode, grounding/search ban, quarantine/eval-only/block/rebuild verdicts, and sidecar/readback parity are tested before train/common claims.
5. Measurement/scorer tests: speed, accuracy, quality, `RuntimeProductAccuracy >= 0.95`, `delta >= +0.02`, `RewardMargin >= 0.025`, ablation `>= 0.02`, fresh-load, continue-training, baseline stability, and domain scorer defaults are tested before promotion.
6. Integration/runtime proof: after code exists, same-scenario dual-path proof must join Path A adapter/API/core evidence with Path B host/UI evidence for the same scenario id and equality tuple. The tuple must include input hash/ref, source unit/path, expected output artifact ids, provenance sidecar refs, legal/quarantine verdict, run id, and UTC timestamp convention as specified in plan-15/R23. Path A-only is `api_only`, Path B-only is `ui_only`, and neither can claim runtime readiness, host readiness, product readiness, or accepted scenario coverage; mock-only or metadata-only evidence cannot claim runtime readiness.
7. Replacement-plan audit: after product readiness and only then, every mock external/resource seam replacement is audited in `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md` with seam id, mock hash/name, test evidence, real endpoint/resource, terms/license/provenance refs, approval ref, and replacement status.

Failure handling: build/test/runtime/mock failures are continuation checkpoints, not stop permissions. A bounded repair worker fixes the failing slice or records a true external blocker after mitigation and decision-prep. Design/dependency decisions cannot hide behind these gates: unresolved provider policy, provenance policy, source-map authority, thresholds, input/output contracts, mock response contracts, or replacement-ledger obligations keep the plan at `PLAN_NOT_READY` rather than becoming future test work.

## Pass 16 Model/Checkpoint Lifecycle Gate

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This Pass 16 gate is plan/governance repair only: it does not implement product code, create model/checkpoint files, run tests/builds/runtime/training, call providers/APIs/network/browser, or use git.

Future implementation must treat the trained ILearningDynamicGraph/DPO artifact as two separated surfaces:

1. `deterministic baseline` code/config: source code, schema/config, deterministic traversal, frozen baseline scorer fixtures, evaluation-only helpers, and repository wiring. This surface is executable baseline authority only; it is not learned policy evidence.
2. `learned policy` / checkpoint state: DynamicGraph/DPO learned state, GraphOrderResolver/DPO policy weights or equivalent trainable state, sidecar manifests, checkpoint hashes, parent lineage, and train/adaptive/common scope.

Every persisted checkpoint must have a checkpoint manifest with at least: `model_id`, `checkpoint_id`, `manifest_schema_version`, `graph_schema_version`, `code_source_ref`, `scorer_version`, `threshold_set`, `training_data_refs`, `source_split_refs`, `metrics_snapshot`, `provenance_refs`, `teacher_provenance_coverage`, `legal_verdict`, `scope` (`CommonGlobal`, `PersonalAdaptive`, `EvalOnly`, or `Quarantine`), `created_at`, `parent_checkpoint_id`, `rollback_ref`, and `compatibility_status`. The manifest is the promotion/import/export contract; an in-memory training result is not a product-ready artifact.

Persistence/export/import design decision: common/global checkpoints are stored through the approved repository/workdir model repository/locator path named by the implementation plan; personal/adaptive checkpoints remain in the user/project adaptive storage path. Export/import moves a checkpoint plus its manifest and lineage together. Import must reject missing manifest fields, incompatible `graph_schema_version`, incompatible scorer/threshold set, missing source branch/ref, legal/provenance/source-split failure, or provider-terms-pending status.

Fresh-load evaluation is mandatory before product-ready or promotion claims: load the manifest and checkpoint into a clean process, run the held-out set, record `RuntimeProductAccuracy`, reward margin, delta, ablation, baseline comparison, scorer version, threshold set, and compatibility status, then compare fresh-load results to the trained/evaluated checkpoint result. In-memory training success alone cannot satisfy this gate.

Promotion gates: no checkpoint/model artifact may be promoted to common/global unless legal/provenance/source split pass, held-out thresholds pass, ablation passes, no regression versus the deterministic baseline and previous accepted checkpoint, provider terms are not pending, source branch/ref is recorded, and manifest compatibility is PASS. Common/global promotion is blocked when `TeacherProvenance`, legal verdict, source authority, or split proof is incomplete even if quality improves.

Rollback and continue-training rules: the previous accepted checkpoint remains addressable as `rollback_ref`; promotion failure, fresh-load failure, compatibility failure, or regression beyond the rollback threshold restores the previous locator without advancing common/global. Continue-training must load an existing checkpoint, record `parent_checkpoint_id`, train only from approved new/source-disjoint rows or source refs, and write a child manifest; it must not replay or overwrite lineage without explicit proof. Retraining from zero is forbidden except for an explicit cold-start scaffold that carries `parent_checkpoint_id=null`, stays `EvalOnly` or `Quarantine` until all gates pass, and cannot be reported as trained readiness.

Failure route: lifecycle gate failures route to bounded repair or a true external blocker after mitigation and decision-prep. They are not stop permissions and cannot be hidden behind future tests.

## Pass31 Current Readiness/Reconciliation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass31 does not run or claim product work. It reconciles current readiness wording against the newer local Claude baseline and the original-task attachments after Source-to-Plan inventory reached `64/64 accepted` at inventory level only.

Current plan-level status:

- Claude remains the read-only baseline authority; active-codex is the execution-facing copy with validated non-contradictory deltas.
- The old active B-table that kept many rows broadly open is historical/pre-Pass31 where the local Claude baseline now resolves the design question.
- This is not PLAN_READY. After Pass34 plus the Pass18 SR-id repair, B-10 AD-detect / graph-order no-hidden-determinism, B-16 typed CON-merge carrier, B-19 SR-16 route/source disambiguation, and the active `SR-17` id split are resolved in plan only and still need implementation/test/runtime/host/seed-file proof later. Historical/pre-Pass35/Pass39 remaining blockers/dependencies included seed-corpus authoring, Qwen3/Together provider terms review, an incomplete 50-pass evidence chain, and future execution proof. Current status after the Pass37-Pass50/Singer acceptance repair: current-ledger acceptance is repaired only as bounded planning-section evidence (50/50 accepted planning sections; not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, not implementation authorization); `SeedCorpus-LanguageTranslate-v1` is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING`; hosted Qwen3/Together remains `BLOCK`/`QUARANTINE` until proof; B-22 is governed by the Pass39 table; future execution proof remains required after approval.
- Execution-only gates remain after readiness and Luba approval: code implementation, tests/builds/runtime/training, provider/API calls, host Path A/Path B proof, branch/mergeability/commit/push gates, and final product review.

## Pass32 B-16 Typed CON-Merge Carrier Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass32 repairs only B-16 typed CON-merge carrier materialization in active-codex plan text. It does not implement code, run tests/builds/runtime probes, call providers/APIs/network/browser, use git, close B-22, or authorize execution.

Implementation-facing B-16 contract:

- `public enum FanInMergeRole { DeterministicFanInBaseline }` is the v1 plan-level carrier role.
- `IEdgeFanInMerger.Merge(IReadOnlyList<LogicalFlowResult> branchResults, string runId)` returns `OperationResult<GraphFanInMergeResult>`.
- `GraphFanInMergeResult` carries `Merged`, `Context`, `FanInMergeRole MergeRole`, `bool MergeOrderIsAuthority`, `MergedBranchCount`, and conflicts/rejected alternatives/provenance refs/trace refs/failure reason codes already required by active plan seams.
- Required v1 values are `MergeRole == FanInMergeRole.DeterministicFanInBaseline` and `MergeOrderIsAuthority == false`.
- Permutation-invariance proof must read these fields from the type and verify identical `Merged`/`Context` across branch-order permutations.
- B-16 stays separate from better-output/FLOW-38/five-format adapter work, which remains B-4/R17 plus B-19 host-route proof.

## Pass33 B-19 SR-16 Route/Source Disambiguation Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass33 repairs only the B-19 SR-16 identifier overload in active-codex plan text. It does not implement code, run tests/builds/runtime probes, call providers/APIs/network/browser, inspect physical host repos, use git, close B-22, or authorize execution.

Implementation-facing B-19 naming contract:

- `SR-16-CoreBacking` means in-core `llm_mvp_core` DesignConversion/R16 backing evidence only.
- `SR-16-RouteFacade` means R16 route authority surfaces: `xiigen-platform-dot-net-angular` public facade, `xiigen-flows-dot-net-angular` FLOW-31 internal seam, and `FigmaToCode-design-gen` / `CanvaToCode` as client/caller surfaces only.
- `SR-17` means Claude's current `language-translate` seed input-prompt corpus; `SeedCorpus-LanguageTranslate-v1` is the active local fixture contract for that seed corpus.
- `SR-17-legacy-active-logicalcontracts` means the historical/local active alias for older `LogicalContracts.cs` L2/L3/L4 anchors formerly labelled `SR-17`.
- FigmaToCode/CanvaToCode are not platform-host implementation authority.
- Physical facade/FLOW-31/client-host route proof remains future implementation inventory and same-scenario Path A/Path B evidence.
- B-4/R17/FLOW-38 legal policy remains separate from B-19 except where B-19 owns host/API physical-route proof boundaries.

## Pass35 SeedCorpus-LanguageTranslate Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Current `SR-17` is `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING` for plan sourcing/shape as the `language-translate` seed input-prompt corpus. The active seed corpus contract is `SeedCorpus-LanguageTranslate-v1`: a self-authored synthetic local fixture for the active `language-translate` pilot. Future approved implementation stores it at `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl` or an equivalent active path under `27.TranslationDomain/SeedData/`; this plan pass does not create the file.

Fixture contract:

- No third-party text, no external API, no web scrape, and no closed-model output.
- Minimum row fields: `seed_id`, `source_language`, `target_language`, `input_text`, `chosen_translation`, `rejected_translation`, `wrong_marker` or `rejected_reason`, `split=train|held_out`, `source_category=synthetic_self_authored`, `license=project_internal_self_authored`, `TeacherProvenance` or provenance refs with synthetic/self-authored status, `semantic_quality_claimed=false` until measured, and `expected_label` required only for `split=held_out` rows.
- Held-out gold rule: for `split=held_out`, `expected_label` is self-authored gold and must equal `chosen_translation`; it is not copied from a provider, recon file, model output, web scrape, or `rejected_translation`. For `split=train`, `expected_label` is omitted or null and may not be used as train signal.
- Plan default coverage: at least 12 train rows plus 4 held_out rows across at least 2 language directions; this is not executed data.
- Future train rows build only unit-level `GraphDpoPair` records for active pilot row F10.3.2; whole-graph/blob pairs remain forbidden.
- Legal gate for active pilot row F10.3.3: project-internal self-authored rows are common-eligible only after `TeacherProvenance`, split/source-disjoint, and held-out gates pass. A later third-party corpus must pass provider/license/provenance proof and cannot silently replace this fixture.
- Seed examples are self-authored project-internal examples, not executed data: positive example `input_text="good morning"`, `chosen_translation="доброе утро"`, `expected_label="доброе утро"`, `split=held_out`; rejected example uses the same source with `rejected_translation="спокойной ночи"`, `wrong_marker="wrong_time_of_day"`; quarantine example is any row with missing `TeacherProvenance`, missing project-internal license, provider/chatbot text, third-party text, web-scraped text, or `expected_label != chosen_translation`.
- Pass29 clean readback for the seed example line above: positive held-out example uses `chosen_translation="доброе утро"` and `expected_label="доброе утро"`; rejected example uses `rejected_translation="спокойной ночи"`. The examples are self-authored project-internal examples, not executed data.
- Measurement for active pilot rows F10.3.4/F10.3.5: held-out exact-match, char-accuracy, and `RuntimeProductAccuracy` use only the `held_out` split. Comparison first applies `NormalizeTranslationForScoring` to candidate output and `expected_label`: Unicode NFC, trim outer whitespace, normalize line endings, collapse repeated ASCII whitespace, preserve case, punctuation, script, diacritics, emoji, and named entities. Exact-match passes only when the normalized strings are identical; char-accuracy is normalized edit-distance similarity against the same gold. The fixture proves scaffold/test path only, not product quality or training success.

Historical/pre-Pass39 current-looking blockers after Pass35: Qwen3/Together provider terms review and B-22 full active-vs-Claude propagation/reconciliation. Current status keeps hosted Qwen3/Together `BLOCK`/`QUARANTINE` until proof and treats B-22 by the Pass39 table, not as a fresh current blocker line. Future execution evidence still includes seed file creation, tests/builds/runtime/training, provider/API gates where authorized, host proof, git gates, and final product review after readiness and Luba approval.

## Pass36 Qwen3/Together Provider-Terms Plan Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass38 supersedes older current-looking lines that list Qwen3/Together terms review as optional-disabled. Hosted Qwen3 is now `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`: Qwen3 Apache-2.0 is selected/recommended as the teacher blueprint and `qwen3-together-v1` / TogetherAI is the selected v1 hosted-provider recommendation, while TogetherAI terms/output-use proof and Luba hosted-use approval remain blocking gates before hosted train/common use. Fireworks/OpenRouter/DashScope are deferred alternates only if a later plan explicitly selects and proves them.

Qwen3 Apache-2.0 evidence is model-license evidence only, not hosted TogetherAI output-use approval or legal approval. Future hosted train/common use requires `ProviderTermsProof.Provider=TogetherAI`, `ModelFamily=Qwen3`, exact `ModelId`, Apache-2.0 Qwen source, Together ToS snapshot, Together privacy/security/ZDR org-setting snapshot, `passthrough` disabled or upstream policy captured, `service_mode=plain`, `grounding_used=false`, `provider_terms_checked=true`, non-pending `terms_snapshot_ref`, `data_retention_mode=ZDR_or_no_store`, `output_ownership_checked=true`, `third_party_model_terms_checked=true`, and Luba hosted-use approval. Missing proof means `BLOCK` or `QUARANTINE`, not train-ready/common-ready.

Historical/pre-Pass39 current-looking blocker after Pass36: B-22 full active-vs-Claude propagation/reconciliation. Current B-22 classification is `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING` in the Pass39 table, not readiness, not implementation authorization, and not execution proof. Future execution evidence still includes seed file creation, tests/builds/runtime/training, provider/API gates where authorized, host proof, git gates, and final product review after readiness and Luba approval.

## Pass37 Latest-Claude Baseline Sync Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass37 supersedes Pass36 only where Pass36 said hosted Qwen3/Together is optional disabled-by-default or no longer a readiness/design blocker. Latest inspected Claude baseline keeps Qwen3/Together as the selected v1 provider config/recommendation (`qwen3-together-v1`) but with `provider_terms_checked=false` and `terms_snapshot_ref=PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)`; hosted train/common use remains `BLOCK` or `QUARANTINE` until terms proof plus Luba hosted-use approval. This is external/Luba/provider proof, not implementation authorization and not legal advice.

Current canon owners after Pass37:

- `GraphDpoPair`: active owner `active-codex-execution-plan-07-dpo-builder.md`; Claude authority owner `claude-plan-07-dpo-builder.md`; exactly 14 fields locked.
- `TeacherProvenance`: active owner `active-codex-execution-plan-08-legal-teacher-gate.md`; Claude authority owner `claude-plan-08-legal-teacher-gate.md` section 3.1; exactly 20 fields = 13 base + 5 NEW + 2 RR6 (`service_mode`, `grounding_used`).
- B-16 fan-in merge: `IEdgeFanInMerger.Merge(...) -> OperationResult<GraphFanInMergeResult>` with typed `MergeRole=FanInMergeRole.DeterministicFanInBaseline` and `MergeOrderIsAuthority=false`; bare `LogicalFlowResult` return text is historical/stale if encountered.

Pass37 one-pass simulation record is `active-codex-execution-37-pass37-claude-baseline-sync.md`. It is not the required 50-pass readiness simulation. No `PLAN_READY`, `FULLY_READY`, B-22 closure, or implementation authorization may be claimed until 50 sequential simulation/review evidence records exist and are reconciled against current active-codex plus latest Claude baseline.

## Pass39 Current B-Inventory Consumer Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

The active master consumes the Pass39 Pass39 B-1..B-22 table in
`active-codex-execution-16-full-task-coverage-simulation-gate.md`. Current B status:
resolved-in-plan/execution-proof-pending for B-1, B-3 through B-17, B-19, B-20, and
B-22; B-2 remains a hosted-provider terms/approval proof residual before hosted
train/common use; B-18 is a nonblocking v1 common-only default with adaptive
activation deferred unless Luba later expands scope; B-21 is a nonblocking
first-substantive-recon-accepted default for current v1 planning unless Luba later
reopens required recon depth. No B row is a current implementable active-codex
design blocker after Pass43 default-defer repair.

This is still not readiness: the minimum 50 sequential simulation/review evidence
records are not present, implementation is not authorized, and all
runtime/test/build/training/host/git proof remains future. Provider terms are not
approved for hosted train/common use only; missing hosted proof is route-specific
and does not block the local self-authored/self-best v1 path after Luba approval.

## Pass44 Hosted-Proof Boundary Consumer Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass44 consumer rule: local v1 execution after future Luba approval must start from self-authored/project-internal seed rows, legal self-best rows, and legally proven common-eligible examples. Hosted Qwen3/Together remains selected/recommended as a hosted teacher blueprint, but hosted output cannot train, mutate common, or satisfy DPO pair training material until `ProviderTermsProof`, output-use/training-rights proof, and Luba hosted-use approval exist. Missing hosted proof is therefore a route-specific external blocker for hosted train/common only, not a whole-plan blocker, not a local-v1 blocker, and not implementation authorization.

## Pass42 Residual Classification Consumer Overlay

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Hubble 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; historical/pre-0aa post-reconciliation verdict was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

The active master consumes the Pass42 residual-classification table in
`active-codex-execution-16-full-task-coverage-simulation-gate.md`. Current residual routing after Pass41:

| class | master-plan routing |
|---|---|
| Implementable plan gaps | None found in the inspected residual set after Pass41. Future findings still require plan repair before readiness. |
| Nonblocking v1 defaults / future expansion options | B-18: v1 common-only default; adaptive activation deferred unless Luba later expands scope. B-21: first substantive recon accepted for current v1 planning; deeper recon/pass-depth optional/future unless Luba later reopens required depth. |
| External/provider/legal proof | Hosted Qwen3/Together remains `BLOCK`/`QUARANTINE`/`EVAL_ONLY` until provider terms/output-use proof and Luba hosted-use approval exist. |
| Future execution proof | Seed JSONL, product code, tests/builds/runtime, training/checkpoints, host proof/screenshots, branch/mergeability, mock replacement ledger, and git are future approved execution only. |
| Count/evidence residuals | 50 accepted sequential pass records plus parent synthesis/reconciliation are still required; Source-to-Plan 64/64 is inventory only. |
| No-go boundaries | `PLAN_NOT_READY` and implementation authorization `NO` remain current. |

No row in this Pass42 overlay authorizes implementation or marks execution-pending work as done.

## Two-Phase Teacher Rollout Consumer Overlay (2026-07-13)

Навигационное резюме (НЕ второй канон). Единственный владелец структуры
двухфазного teacher-маршрута — `active-codex-execution-two-phase-local-remote-overlay.md`.
Там: имена этапов `TP1_LOCAL` (локальный Ollama Qwen3.5-9B Q4_K_M, профиль
`qwen35-local-v1`) и `TP2_HOSTED` (hosted `qwen3-together-v1`), UML фаз, матрица
этапов §5, критерии перехода G-TP-1..G-TP-7, легальный инвариант обеих фаз.
Этот раздел лишь ссылается на владельца: счётчики 12/27/64, `TeacherProvenance`
(20 полей, plan-08 §3.1), `GraphDpoPair` (14 полей, plan-07) и readiness
(`PLAN_NOT_READY`, implementation authorization `NO`) не меняются. Closed
proprietary teachers запрещены в обеих фазах.
