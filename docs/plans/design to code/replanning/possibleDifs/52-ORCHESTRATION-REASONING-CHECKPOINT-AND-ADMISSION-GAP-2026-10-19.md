# NEW-GOLD-OR-01..04 — OrchestrationReasoning: v1→v2 trained-state reachability, restoration fidelity, registry and fallbacks

Дата 2026-10-19; аудит READ-ONLY; implementation NOT_CHANGED; .NET runtime NOT_RUN; качество heldout NOT_MEASURED. Пользовательский золотой донор: [orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc). Recipient: [codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76).

## Проверенные наблюдения (не inference)

| ID | Gold | Current | Проверенная причинная связь и граница |
| --- | --- | --- | --- |
| OR-01A | [Machine L111](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L109-L115): CheckpointSchemaVersion=1 | [Machine L132](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L128-L138): Version=2 | SOURCE_DIFF_VERIFIED; миграция умышленно ввела provenance proof |
| OR-01B | [Gold v1 loader L404-L434](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L404-L434) использует ImportLearnedState | [Current ValidateCheckpoint L971-L980](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L971-L980): v1 fail | CURRENT_STATIC_REJECTION_VERIFIED |
| OR-01C | v1 manifest checkpointRef=orchestration-reasoning-v1-checkpoint.json | [Current ReadManifestAsync L1063-L1069](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L1063-L1069): any schema v1 fail | Второй независимый блокер; простой upgrade checkpoint без manifest не спасёт |
| OR-01D | Legacy v1 load checks manifest/hash | [Current LoadCheckpointAsync L699-L755](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L699-L755) дополнительно требует proof, co-location и admission | **Отмена** safety gate запрещена: нужно построить допустимый новый bundle из исходных материалов |
| OR-01E | На обоих heads оставлен v1 [manifest](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json) и [named checkpoint](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json) | Код recipient не может легитимно импортировать прежнюю поставку через тот же load path | Product regression hypothesis: фактическое влияние на запросы НЕ замерено |
| OR-01F | Gold [XiigenExtensionApiService L1586](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1586-L1613) грузит v1 | [Current API L1427-L1450](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1427-L1450) при fail записывает error/warning, оставляет new OrchestrationReasoningMachine | SOURCE_CHAIN_CONFIRMED; expected status machine not-trained, не вымышленный successful import |
| OR-02 | [Donor index](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/models/index.json) содержит orchestration-reasoning-v1 | [Current index](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/index.json) его не содержит | REGISTRY_DIFF_VERIFIED, consumer impact UNKNOWN; **API использует literal path, не index** |
| OR-04 | — | [Current API L1470+](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1464-L1479): all_common_assets_loaded = pttLoaded && orLoaded && skillLoaded | Aggregate false при одной отказавшей единице. End-user severity HYPOTHESIS, а не прямой KPI |

Проверка реальных JSON в обеих ветках (read-back 19.10.2026):
- **manifest.json:** schemaVersion=1, learnedPatternCount=15, dpoPairCount=8, checkpointRef=orchestration-reasoning-v1-checkpoint.json.
- **orchestration-reasoning-v1-checkpoint.json:** schemaVersion=1, learnedState.learnedPatterns.length=15, acceptedRefIds.length=11.
- **checkpoint.json:** schemaVersion=1, learnedState.learnedPatterns.length=4, acceptedRefIds.length=4.
- SHA blob этих трёх файлов при сравнении старой и текущей ветки одинаковы (manifest=14328d4549, named checkpoint=0c401fabe2, checkpoint.json=d6de73985b; укороченные Git blob SHA, не SHA256 содержимого). Следовательно, **артефакты остались старыми, а loader заменён**. Ведущая гипотеза — compatibility/rollout gap, а не повреждение весов.
- В старом index metadata checkpointSha256 начинается dcd7d02b, а в позднем manifest checkpointSha256 начинается 9ec930d5: gold index уже устарел относительно последнего file bundle. Нельзя лечить runtime простым копированием старого index.

## Схема вычислений: что действительно ломается

    Gold branch:
      v1 checkpoint(15 patterns) + v1 manifest
          -> v1 loader + ImportLearnedState
          -> OrchestrationReasoningScenarioUnit.Decide
          -> XiigenExtensionApiService response

    Current branch, checked-in common bundle:
      v1 checkpoint + v1 manifest
          -> v2 LoadCheckpointAsync
          -> v1 rejected (provenance rebuild required)
          -> XiigenExtensionApiService warning and new untrained machine
          -> response cannot use those 15 trained patterns

    Target repair:
      original lawful rows + original 15 pattern semantics/identity
          -> independent provenance reconstruction/admission receipt
          -> v2 checkpoint + v2 manifest + v2 proof bundle
          -> cold restore -> learned consumer -> independent heldout quality

Сохранить отрицательный контроль с untrained current machine: результат не должен оказываться тем же «успехом» только благодаря детерминистическому fallback. Exact path, selected pattern IDs, matched signal counts, accepted Ref IDs, admission receipt, warnings и actual generated response логировать отдельно.

## Новые независимые гипотезы относительно этого факта

**OR-03: partial re-materialization.** Если «repair» возьмёт только dpoPairCount=8 и удалит историю продолжения состояния либо выберет checkpoint.json, может получиться валидный v2 artifact с неполной памятью (4 вместо 15). 8 DPO pairs, 11 acceptedRefs, 15 patterns — **не три значения одной и той же величины**. Нужна семантическая таблица patternId→sourceId→acceptedRef→decision. Это не обвинение в существующем баге мигратора; пока допущение, которое следует опровергнуть.

**OR-05: restorable-but-unreachable.** Даже после корректного v2 bundle потребитель может остаться на другой model root (user adaptive cache before repo common fallback) либо ложном scope. [Current user loader L1515-L1535](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1515-L1535). Измерять одновременно asset_source, loaded_first, actual pattern, not just file existence/manifest hash.

**OR-06: status/evidence cross-poisoning.** Aggregate AllLoaded=false может маскировать независимый хороший PromptToTask и SkillSelection в общей оценке. Проверить per-component и global statuses; не объявлять «все три модели не работали» по одному false boolean. [API L1371-L1381](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1371-L1381) делает global semantic quality gating по AllLoaded плюс другим критериям. Это **возможное** скрытие локального эффекта, не доказанный источник 0/4 RP01.

**OR-07: gold attribution control.** [Donor E53](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-2026-05-04-131603.md) — deterministic graph + governance/abstention with 0 live external calls; он не измеряет повторно DPO/LoRA. Должно быть два отдельных baseline: historical graph path и historical learned guide path. Не смешивать их качества и не приписывать обе функции neural training.

## DOMAIN/WIN + falsifier + владельцы

| ID | Домен | Положительная интерпретация | Условие опровержения |
| --- | --- | --- | --- |
| OR-01 | OrchestrationReasoning/PromptToTask/SkillSelection | current v1 common cannot load; legal v2 rebuild возвращает learned decisions | Old v1 на актуальном current build всё-таки успешно и admission-valid грузится; либо v2 восстановлен, но outcomes те же/хуже gold |
| OR-03 | Orchestration learned scenarios, risk gates | semantic replay loss due to wrong source selection | v2 preserves all 15 IDs, 11 refs and blind decisions, effect 0 |
| OR-04/06 | API host integration | one invalid asset depresses aggregate status | quality gate did not use aggregate for selected ordinary requests |
| OR-05 | user adaptive cache, common bundle, cold start | priority/fallback chooses different checkpoint | asset_source and selected state match pinned wanted bundle |
| OR-07 | graph planning vs learned decisions | structural correctness is mistakenly reported as training success | positive neural/inference heldout for same exact target is independently measured |

**Рекомендуемая диспетчеризация:** архитектор/skill plan reviewer подтверждают typed unit, loader safety и ownership; code examiner фиксирует точные caller→consumer spans; prompt engineer создаёт source-disjoint paired cases; human communication оформляет результат как native/analytic/unknown. Никаких скрытых разрешений менять core branch и никаких Windows CI как substitute for learning score.

## Быстрые проверочные эксперименты

Полный [52-GOLD-CAUSAL MICRO](52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md): GOLD-OR-A0 в 4 arms, 12 запросов и 3 исхода (load, selected pattern, heldout correctness) + guardrail; GOLD-OR-A1 graph/deterministic E53; GOLD-OR-A2 replay E43; GOLD-OR-A3 domain-aware kNN. **Сначала A0**. Если v1 fail подтвердился, не повторять 500 пар; первым исполнительным patch должен быть законный v2 state admission, затем cold readback на прежнем и новых вопросах.

**Запрещённая оптимизация:** подпилить LoadCheckpointAsync чтобы пропускать v1 с фиктивным proof; изменить оценки «not-trained» на «ready»; поправить gold oracles под новый output; выдавать source-only скрипт за улучшившееся обучение.

