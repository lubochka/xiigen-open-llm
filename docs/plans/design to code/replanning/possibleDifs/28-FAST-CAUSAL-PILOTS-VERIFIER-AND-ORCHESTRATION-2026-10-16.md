# N28 — три быстрых причинных опыта вместо очередного полного Windows CI

**16.10.2026 | EXECUTOR-READY DESIGN; .NET NATIVE NOT_RUN; ML quality NOT_MEASURED.** Весь кодный эксперимент делать в временном console harness с `ProjectReference` на существующие `27.TranslationDomain` и `42.ProjectWikiDomain`; не переписывать production и не вызывать внешнего учителя. Repository source remains read-only in this auditing round. Отчёт и гипотезы публиковать в `xiigen-planning/.../possibleDifs`. Эксперимент не заменяет DPO RAG policy для настоящих новых учебных пар.

## Общий контракт V28 / O28 / C28

- **Один фактор за раз:** один и тот же immutable fixture; один cold new instance на arm; всё, включая tokenization/signatures/split, фиксировано до первого запуска.
- **Не копировать исходные пары в heldout:** группы по `SourceId/SourceBundleId` и source family, а не row index. У каждого source heldout отличающийся текст, без копии в train. Для проверки механизма допускается специально построенный counterfactual с тем же prompt при разных Chosen; он **не считается генерализационной метрикой**.
- **Матрица отчёта:** route, exact model/branch SHA, source fixture IDs, previous→new answer, source concept map, chosen-consumption trace, cold-restore comparison; `correct / incorrect / abstain / fallback`, отдельно для каждого домена/route.
- **Метрики:** `blind_semantic_correct_rate`, `wrong_accept_rate`, `false_reject_rate`, `source_disjoint_accuracy`, `new_knowledge_gain`, `old_knowledge_retention`, `cold_state_parity`; reporting requires denominators (не «100%» без N).
- **Стоп:** 1 local project build + до 4 маленьких проходов; без полного Windows CI и PR-конвейера. Если compilation/environment FAIL — `NOT_RUN`, не подменять JS-эмуляцией. Исторические цифры E38V2/E40V2 не перезаписывать.

## V28 (первый приоритет, дешёвый) — проверка роли concept/context

Source:
[Train/Parser](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1094-L1147),
[TryParseIssue](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1216-L1247),
[Verify](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1051-L1092),
[TranslationLearningExample schema](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L126-L178).

Fixture: один TRAIN typed `TranslationLearningExample` UnitId=`quality-verifier`, `HeldOut=false`, `SemanticSignature="TRAIN|LAUNCH_OP|ADD_OP"`, `InputJson` содержит `VerifierInput("source train", "missing ready")`, `ChosenOutputJson` — `VerifierOutput(0.42, ["expected 'ready' for concept LAUNCH_OP in context ADD_OP"], false, null)`; `RejectedOutputJson` — независимый opposite `VerifierOutput(0.91,[],true,null)`. Explicit `SourceId="train/source-1"`. Здесь исходная сторона текста намеренно концептуальная fixture, не авторизованная teacher DPO. Замер semantic QA на настоящих DPO требует законного учителя и независимого judge отдельно.

Construct `SemanticTokenSequence` прямо из public record with distinct `Signature` strings (не train signature), `Pattern=SVO`, `Tokens=[new SemanticToken("add","add",NOUN,"OBJ",ADD_OP)]` или токены `LAUNCH_OP` и `ADD_OP`; `RuleTags=[]`; PosBigrams empty. Use `QualityVerifierUnit.TrainOnExample` once. Query candidate text `"valid output without keyword"` vs `"ready"`.

| Case | Tokens of heldout source | Gold when rule means `LAUNCH_OP in ADD_OP context → ready` | Expected current `Verify(..., missing ready)` |
|---|---|---|---|
| V1 (irrelevant) | ADD_OP only, distinct signature | PASS | **FAIL** due enum-order swapped source |
| V2 (context absent) | LAUNCH_OP only, distinct signature | PASS | PASS (scope absent; parsed source=ADD_OP) |
| V3 (both) | LAUNCH_OP + ADD_OP, distinct signature | FAIL | FAIL |
| V4 positive | LAUNCH_OP + ADD_OP, candidate includes ready | PASS | PASS |

**Arms:** V0 fresh untrained; V1 current code; V2 only replace parser with explicit labeled capture `for concept ([A-Z_]+) in context ([A-Z_]+)` (maintain unchanged Verify); V3 parser fix + enforce that **both src AND ctx concepts** appear before checking term. Preserve cold checkpoint state and frozen examples on all arms. `V1 false reject, V2 correct` supports parser effect; `V2 vs V3` identifies separate context-scope effect. Never call V0 «learned correct» because untrained verifier passes all. Measure ordinary TranslateAsync separately on 4 non-phrase executor-generated translations, to establish that a changed Verifier decision affects user-visible answer rather than only internal result. **Falsifier:** real accepted lesson producer never includes this two-concept issue pattern, or V1 and V2 source-disjoint decisions equal on designed contrast → `LATENT/NO_GAIN`.

**Minimal implementation:** a `.NET console` test harness instantiates `QualityVerifierUnit`, creates `TranslationLearningExample` by serializing public `VerifierInput`/`VerifierOutput`, trains, then calls public `Verify` with constructed `SemanticTokenSequence`. Do not need large DPO/LoRA run. For protected static parsing helper, observable `Verify` is the actual contract, do not use reflection as substitute.

## O28 — chosen-text sensitivity and routing-metric control in ProjectWiki

Source:
[OrchestrationPlanMachine](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L60-L132),
[Route Composer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L38-L81),
[OrchestrationPilotLoopRunner](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPilotLoopRunner.cs#L256-L324).

Create **2 TRAIN Mode2 pairs** in two new machines with same `PairId`, `Prompt`, `Mode`, `SourceBundleId`, but with mutually contradictory `Chosen.PlanText` ("REQUIRE REVIEW" vs "SKIP REVIEW"), still permitted by DPO material policy only as synthetic private evaluation fixtures. Add one Mode3 pair analogous. Zero-training fresh machine is a third arm. Build two heldout prompts: (i) same domain but distinct surface/source bundle and containing a new expected action not echoed in input; (ii) entirely unrelated request with Jaccard=0. Score blind action fidelity by output text, **not** presence of `style_source`, `MlGenerated` or `pair.FlowRef`. Trace `FindNearestWithScore` and GeneratePlanAsync. **Pass for hypothesis:** training changes style_source, but changing Chosen content never changes chosen-plan semantics. **Falsifier:** learned chosen content reaches a different consumer path; record path and report discrepancy rather than patching evidence. Positive comparison: Mode1 learner might actually include Chosen text; keep a separate sample, don't count it as Mode2.

Check `Mode2Accuracy` metric on new machine vs trained and shuffled-Chosen. If all return identical routing success while `semantic_gold` differs, label *routing_accuracy_not_learning_accuracy*. Source disjoint check must not reward prompt echo. Result report include `no-memory`, `correct-chosen`, `shuffled-chosen` outputs and counts.

## C28 — corrected same-ID lesson and cold resume (second priority)

Construct two Mode2 pairs `PairId="correction-17"`, same prompt, same source bundle; initially `Chosen.PlanText="wrong"`, then re-learn `Chosen.PlanText="correct"` with **identical PairId**. Arms: (C0) two calls same instance, (C1) save/load between calls, (C2) second pair has new unique revision ID. Observe `RowsTrained`, `ExportCheckpoint().KnnStore` chosen payload before/after, `FindNearest`, `GeneratePlanAsync` and cold reload. Source pointer and content distinction both required. [Train skip](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L189-L215) and [store skip](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L11-L26).

Predicted C0/C1: corrected same-ID pair is not adopted (`RowsTrained=0`), persisted original remains; C2: new ID is adopted but old is also still present, no guarantee new wins due kNN `ThenBy(PairId)`. **Do not patch implementation until PairId versioning contract established.** If existing pipeline always generates fresh IDs for revisions, verdict is `EXPECTED_IDEMPOTENCY`, not a defect.

## Plan-review, prompt engineering and human reporting gates

Architect: trace `source → learner → state → consumer → semantic answer`; code reviewer: independently read full actual receiver before approval; planner: priority V28 then O28 then C28, stop after one discriminating slice; prompt engineer: request **one bottleneck per patch**, with WRONG/CORRECT output pair and explicit falsifier; human communication: report “текстовое правило разобрано неверно” instead of “матрица сломана”. Independent approved judge required before asserting user quality improvement.

**Statuses now:** all three design-only; no source-run on this workstation; `.NET` SDK unavailable. See [new parser finding](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md) and [ProjectWiki finding](28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md).
