# N28-B/C — ProjectWiki: inert Chosen and blocked revised lessons

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N28:START -->
## Текущий consumer и целостность редакции — уточнение 11 ноября 2026

Сверка core 55cfd9d00bbebdeae04da7c505b84d0b1cb9b747 уточняет исторический SOURCE-CONFIRMED ниже. **Общее утверждение N28-B «Mode2 не читает chosen body» устарело:** FIX-P118 добавляет body при нормализованном точном совпадении prompt. Для переставленных/новых запросов exact guard сохраняется; точное воспроизведение не доказывает парафразное обобщение.

B12 остаётся отдельным: store даёт Jaccard=1 для пустых наборов и может выбрать нулевого соседа по PairId. Mode1 имеет положительные score guards; Mode2/Mode3 имеют собственные условия. Не приписывать всякому zero-overlap выдачу чужого body: current Mode2 проверяет exact prompt. Измерять output, style attribution/flow scope и причину fallback для empty/punctuation, no-overlap, reordered и exact inputs.

F08/B11 требует active revision одного FlowRef: A→B1→B2 сохраняет совместимые B1/B2 и body B; историческая A не выбирает исполняемую цель. Проектируемый checkpoint переносит revision вместе с weights/pairs. PairId immutable: identical retry идемпотентен, changed duplicate отвергается до мутации. Import проверяет payload каждого member, а не только существование ID; ambiguous/invalid snapshot не стирает действующее состояние. Порядок PairId/JSON не назначает latest.

N28-C сохраняет условность: повтор одинаковой пары и новая редакция — разные события. Не снимать dedup guard; назначить new lesson/revision identity. Приёмка ordinary Generate/wrapper проверяет полный body, независимый маршрут C, restore и мутацию active filter. [Core план](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/repair-plan.md).

Это исправление временной границы описания и проект будущей приёмки; новых native A/B и доказательства влияния на neural RP01 нет.
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N28:END -->


Date 2026-10-16. SOURCE-CONFIRMED CODE PATHS. Native quality NOT_RUN. Core docs/plans/replanning-possible-diffs READ ONLY. This is a possible cause in ProjectWiki learning, not a proven causal regression in RP01's LoRA student.

## N28-B — KNN lesson can influence route metadata, not learned content

Source-to-consumer chain:

1. [OrchestrationTrainingPair](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanContracts.cs#L109-L127) stores PairId, Prompt, Chosen.PlanText, Chosen.Title, mode, source bundle.
2. [TrainOrchestrationBatchAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L189-L215) passes accepted Mode2 lessons to `_mode2Knn.Add` and Mode3 to `_humanExplanationKnn.Add`.
3. [OrchestrationKnnStore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L30-L64) returns a nearest pair by token Jaccard, even when all similarities are zero.
4. [GeneratePlanAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L60-L132) passes that pair into the ordinary Mode2/Mode3 consumer.
5. [RenderMode2Plan](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L38-L62) only consumes the nearest PairId as `style_source`. It does not consume `Chosen.PlanText`, `Chosen.Title` or rejected response. [RenderHumanExplanation](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L64-L81) uses FlowRef and PairId but not the learned explanation text. **Counterexample boundary:** Mode1 [RenderLearnedFlowPlan](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L10-L35) DOES include chosen content; do not generalize this finding to Mode1.
6. [Pilot EvaluateAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPilotLoopRunner.cs#L256-L324) evaluates Mode2 mainly by mode/classifier/MlGenerated flags and a FlowRef that can already be echoed in `prompt=`; it does not require fidelity to newly learned Chosen.PlanText. [Machine EvaluateAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L252-L295) uses PlanText.Contains(ExpectedResponse); a scaffold string could satisfy some evaluations without learning. Historical baseline rows need independent inspection before calling a specific historical PASS invalid.

H28-B1: replacing Chosen.PlanText with contradictory held-out instruction but keeping PairId/Prompt/Mode constant will not change Mode2 generated subject text. H28-B2: deleting training memory may leave Mode2 routing accuracy unaffected. Domains: ProjectWiki orchestration Mode2/Mode3 only; no statement about translation KNN or RP01 B0 neural generation.

## N28-C — correction of existing PairId may never become a learned revision

[TrainOrchestrationBatchAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L189-L214) immediately skips an existing `_trainedPairIds` ID, without checking content hash. [OrchestrationKnnStore.Add](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L11-L26) independently drops a duplicate PairId. [Export/ImportCheckpoint](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L338-L379) persists the set so the skip survives cold restore. [Pilot SelectTrainPairs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPilotLoopRunner.cs#L246-L253) also filters already-trained IDs.

H28-C1 is **conditional**: if corrected DPO lessons intentionally reuse PairId, their newer Chosen never reaches memory. If PairId is guaranteed immutable and every revision gets a new ID, this is correct idempotency, not a bug. Fix must distinguish retry of byte-identical material from a real revision by version/content identity; do not simply delete the guard.

## Relevant prior work / novel delta

Existing [core historical transfer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md) already examines E38V2 and E40V2; do not count their findings as new. Existing [planning historical branch evidence](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) distinguishes routes and successes. This delta concerns **actual ProjectWiki Mode2/Mode3 learned-content consumption and correction identity**, not a second label for those findings. It also exists on the older [DesignToCode store](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/42.ProjectWikiDomain/OrchestrationKnnStore.cs), so not introduced by RP01.

Evidence type: direct source review and cross-branch code read; new native A/B runs and independent quality review NOT_RUN. See [N28 causal pilots](28-FAST-CAUSAL-PILOTS-VERIFIER-AND-ORCHESTRATION-2026-10-16.md).
