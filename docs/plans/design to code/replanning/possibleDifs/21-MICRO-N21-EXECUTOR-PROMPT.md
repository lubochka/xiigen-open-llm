# Kiro/CODEX — один исполнительский запуск MICRO-N21, без эскалации в Windows CI

**Дата:** 2026-10-16. **Статус:** WORK_ORDER_NOT_EXECUTED. **Владелец:** bounded executor; отдельная независимая проверка по точному candidate SHA. **Не считать этот prompt доказательством сделанного запуска.**

## Цель, результат и границы

Доказать или опровергнуть **на одной малой слепой панели**, что правдивый corrective lesson, отфильтрованный как похожий исходный текст, способен улучшить **конечный ответ пользователя** после реального обучения Translation Unit. Сначала доказать сам filter на настоящем C#, затем сравнить actual trained Unit и ordinary consumer с отрицательным контролем.

**Источники (читать перед изменениями):** [N21 source audit](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [эксперимент](21-MICRO-CAUSAL-LEARNING-QUALITY-PILOT.md), [fixture](21-MICRO-CAUSAL-LAW-DELTA-FIXTURE.json); текущий `llm_mvp_core` `codex/dpo-implementation`, pinned `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`, но прежде всего **получи фактический новый HEAD**. Прочти `TranslationStructuredBatchBuilder.cs` полностью (включая filter, BuildGeneralBatch, negative-candidate), `TranslationMachine.cs`, `TranslationTrainedUnits.cs`, `TranslationRoutingFlow.cs` и их callers/consumers. Сверь действующие `docs/skills`: `conversation-intent-and-self-reflection`, `phase-planning`, `plan-review`, `human-readable-architecture-audit`, `code-examination`, `root-cause-tracing`, `experiment-design`, `claim-verification` и `prompt-patch-authoring`.

### Строгий исполнительский prompt

> Ты исследователь причин потери обучения, а не исполнитель расширения тестового покрытия. Цель — `MICRO-N21-CORRECTIVE-ADMISSION-01`. Не принимай заранее, что исходник ошибочен или что KNN способен решить neural RP01.  
>  
> **L0.** Отслежи реальный вызов `FilterByTrainingHistoryUniqueness` из `TranslationBatch029Runner`. Запусти 3 существующим native .NET host: `Марии→Ольге`, регистра/пунктуация, английское отрицание (последнее только для проверки математики метода, не как фактический RU_EN случай). Сохрани входы, Jaccard, фильтр decision, repo SHA. Javascript proxy 5/7=0.714286 в документе является гипотезой для проверки, НЕ результатом C#.  
>  
> **L1.** Возьми только approved lawful synthetic DEV examples (проверить предметным независимым оценщиком). Создай A: current filter; B: точно та же история, но одна approved recipient-correction допущена через временный test-only law-aware gate; C: то же число и тексты, но противоречащая метка correction; D: frozen baseline/no-new-training. Каждый arm создай заново из одного baseline/seed/Unit state, ничего не наследуй от предшествующего arm. Никакого LoRA/DPO/500-pair обучения.  
>  
> Выполни все 8 proposed heldout источников через **реально вызванный** `TranslationRoutingFlow` и separately direct Unit. Проверяй, что exact training source отсутствует среди heldout; не передавай gold, `required/forbidden`, правильный recipient в `Metadata`/index или candidate selector. Сохрани `AdapterId`, key/source/evidence ID, returned translation, judge response (correct/incorrect/abstain), wrong recipient, origin of answer, duration и memory. Правильность выбери независимым rubric **до** результатов.  
>  
> **Решение:** B>C на качественных ответах при корректных traces и отсутствие critical regressions — exploratory support; C≈B — не подтверждено на данном наборе; улучшение только на train exact — memorization; trained Unit меняется, а consumer не вызывает — adoption gap. Если нет native runner, прав/approved labels/способа вызвать consumer — честный `BLOCKED_WITH_EXACT_REASON`, не придумывай PASS.  
>  
> Публикуй **только** реально полученный `21-MICRO-N21-RESULTS.json` (8 строк + команды, SHA, ошибка/ограничения) в `possibleDifs`, с independent review binding к reviewed commit/diff. Не трогай сохранённые frozen RP01 science outcomes, не меняй acceptance post hoc, не навязывай Windows Actions, общее покрытие тестами или долгий релизный pipeline на этап выяснения причин. Не модифицируй production learner до отделения baseline и результата; однофакторные изменения держи в test-only shadow harness.  
>  
> В отчёте человеку сначала объясни на русском: *какой именно урок мог исчезнуть, действительно ли исчез, изменилось ли качество на новых запросах, дошёл ли результат до пользователя, что осталось неопределённым*. Только затем таблица SHA/IDs/logs.

## Reviewer-acceptance, а не автоматический «зелёный» verdict

| Review | PASS только если | Иначе |
|---|---|---|
| Архитектор | L0 and L1 actual caller→Unit→Flow→user route shown, no false RP01 LoRA attribution | NEEDS_FIX |
| Планировщик | A/B/C/D share baseline; labels/heldout not leaked; metric frozen; all 8 cases logged | NEEDS_FIX |
| Code QA | Source hashes, no production path mutation, actual native C# evidence, no external-LLM authorship spoof | NEEDS_FIX |
| Промпт/данные | CODEX/Astra independent DEV-label judge; no hidden answer in runtime input; future final DPO roles distinct from DEV | DATA / NEEDS_REVIEW |
| Human | One-paragraph effect explanation with exact numbers, no causal overclaim | DRAFT_ONLY |

**Stop:** завершить один ограниченный пилот после полного outcome или точного внешнего blocker. Не заменять отрицательный итог другим «более зелёным» набором. Следующая задача после `EXPLORATORY_POSITIVE` — 20+ source-family-disjoint доменных экзаменов и проверка cold serving, не автоматический ремонт всей архитектуры.

**Ограничение текущей публикации:** этот prompt создан документарно и НЕ означает запуск CODEX/Astra, Chinese teacher, native tests или доказанный gain.
