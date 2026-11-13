# 49/02. Прямые branch differences, причинные гипотезы N49 и границы их применимости

**Сравнение:** [local_wiki_planning](https://github.com/lubochka/llm_mvp_core/tree/local_wiki_planning) (прочитанный HEAD 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a) ↔ [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation). **SOURCE-DIFF CONFIRMED; native C# и causal quality NOT_RUN.** Здесь слова «новое» означают новую *проверенную узкую цепь/контраст относительно просмотренных prior документов*, не отсутствие любого похожего замечания среди 400+ исследований.

## D49-01 — потеря source, chosen и rejected при DPO-scoring CodeTrace

- **Донор:** [CodeTraceBuilderTrainingPipeline.cs:119–120][ct_old] посылает ProjectWikiDpoMetricPair(pair.PairId, pair.A, pair.Chosen, pair.Rejected). В архиве 24 мая 100 разных этих троек и heldout accuracy=0.92.
- **Цель:** [тот же метод:124–129][ct_new] посылает BuildMetricPrompt(pair.RejectedType), BuildChosenSignal(pair.RejectedType), BuildRejectedSignal(pair.RejectedType); actual pair.A, pair.Chosen и pair.Rejected **не читаются этим преобразованием**.
- **Механизм:** две разные трассы с одним rejectedType превращаются в одинаковые prompt/chosen/rejected для metric learner; имя pair.Id само по себе не добавляет исходный C# код в model input. Обучать content-conditioned восстановление callers/values по такому DPO нельзя; можно научиться распознавать общие defect markers.
- **Статус:** VERIFIED_SOURCE_DIFF + VERIFIED_DONOR_DIVERSITY. **Это не единственная установленная причина RP01:** RP01 научная цепь вызывает другой pair materializer. См. известный [Core28][core28] и [Core76][core76] для сходных классов проблем; *новое здесь* — конкретная прямая ветвевая замена и поздний положительный архив 24.05. Сохранённые цифры не являются matched before/after.
- **Профиль:** C# CodeTrace/call graph/value flow. Не применять syntax-tree fix к переводу без отдельного основания.
- **Быстрый способ различить:** EXP49-CT-PAIR (49/03). Required source traces: число видимых уникальных model triplets до и после, semantic trace correctness и настоящий downstream caller.

## D49-02 — RequestRouter DPO обучается на CODE_TRACE_DPO шаблоне без route identity

- **Донор:** [ProjectWikiRequestRoutingTrainingPipeline.cs:110–116][rr_old] делает Task: route... + Executor prompt: pair.Prompt, затем pair.Chosen, pair.Rejected.
- **Цель:** [тот же pipeline:131–167][rr_new] формирует CODE_TRACE_DPO, DEFECT_TYPE_x, PREFERRED_CODE_TRACE_OUTPUT ×12, TRUE/FALSE и RouteCorrectionTokens(pair.RejectedType). **Не использует pair.Prompt, pair.Chosen, pair.Rejected и ExpectedRouteKind** в стороне DPO model material. Строка CODE_TRACE — не название действительной предметной задачи router.
- **Сильное проверяемое следствие:** при равном RejectedType два исполнителя с *противоположным требуемым route* получают один и тот же model-visible DPO triplet (за исключением идентификатора записи, который не входит в текст). Модель не сможет установить соответствие вход→маршрут по этим двум одинаковым triples; она может различать только тип дефекта и общие маркеры.
- **Статус:** VERIFIED_SOURCE_DIFF, будущая semantic-loss hypothesis. Важно: это **только дополнительный DPO scorer**. Отдельный ProjectWikiRequestRoutingTrainableModel.Train обучает lexical perceptron на Prompt/ExpectedRouteKind и не теряет их из-за этого ToDpoMetricPair. Не объявлять весь router нетренируемым.
- **Исторический контрпример ложной «готовности»:** batch092, primary accuracy=1.00, DPO heldout=.66, publication skipped. Данные не доказывают, что именно эту неудачу вызвала текущая версия функции: старый архив создан другим source.
- **Профиль:** оркестрация / выбор Units. EXP49-ROUTE-IDENTITY (49/03).

## D49-03 — нынешний fail-closed semantic gate делает предметный PASS недостаточным

**Нынешний pipeline** [ProjectWikiRequestRoutingTrainingRunner.cs, блок gate][rr_runner] вычисляет routeAccuracy и heldOutGatePassed, но явно присваивает semanticQualityClaimed=false, затем формирует snapshot.PublishAllowed=heldOutGatePassed && semanticQualityClaimed. [ProjectWikiRequestRoutingTrainingPipeline.cs:80–115][rr_new] также требует snapshot.PublishAllowed и поэтому отказывает, даже если достигается высокий предметный score. [TrainableModel.cs:95–100][route_model] использует Route(... requireEvaluationGate:true), а RouteForEvaluation(...false) — **другой режим потребления**.

Это **подтверждённая охранная граница**, которую нельзя автоматически назвать ошибкой: отдельно проверенная semantic quality не предъявлена. Возможное ухудшение *доступности способности*, не доказанное ухудшение обучения: новая модель обучилась, но не попала к потребителю. Ремонт требует реального независимого semantic/heldout evidence и предусмотренной authority-цепи, а не установки false→true или обхода отказа. Старый donor не требовал того же snapshot publish gate; это может объяснить отличие по готовности route, не по нейронному качеству. EXP49-GATE-ADOPTION.

## D49-04 — исторический и нынешний orchestration flow по умолчанию читают статический RouterUnit

- **Донор:** [ProjectWikiTrainingOrchestrator.cs:129][orch_old] вызывает _router.Route(prompt), конструктор создаёт ProjectWikiRequestRouterUnit.
- **Цель:** [ProjectWikiTrainingOrchestrator.cs:205][orch_new] по-прежнему вызывает _router.Route(prompt), а не ProjectWikiRequestRoutingTrainableModel.Route. [Current ProjectWikiRequestRouterUnit.cs][router_unit] содержит множественные ContainsAny/keyword ветви.
- **Отдельный trainable consumer существует:** [ProjectWikiRequestRoutingTrainableModel.cs:410–430][route_model] имеет отдельный logical execution путь. Поэтому нельзя писать «модель вообще нигде не вызывается».
- **Статус:** CONFIRMED_EXISTING_CONSUMER_SEPARATION; **это преимущественно сохранённый по веткам интерфейсный разрыв**, не новая сломавшаяся формула. Очень важно для интерпретации исторической primary accuracy, DPO gate и ordinary behavior.
- **Профиль:** обучаемый выбор маршрута в orchestration Unit. Успех отдельного классификатора ≠ изменение ответа обычного orchestrator.

## N49-05 — положительные 100% DPO QueryPlanning не удостоверяют зависимость ответа от вопроса

[Исходный QP-архив][qp_pairs] имеет ровно 1 chosen и 1 rejected для 100 записей. TRAIN prompt unique=8/50; heldout prompt unique=8/50, exact overlap между split=0. Поэтому **условная текстовая генерация не наблюдается в target labels**: при любом вопросе ожидаемое выбранное тело одинаковое. Это не утечка exact prompts и не знак отрицательного качества фиксированной инструкции. Но это делает 1.00 DPO preference несопоставимой с содержательным C# CodeTrace .92.

**Новизна N49-05:** пересчёт полного позднего положительного архива 134 и конкретная постановка counterfactual condition test; общая проблема marker shortcut изучена ранее. EXP49-QP-CONDITIONAL должен фиксировать два противоположных вопроса, ожидать отличающихся required steps/owners, а не только маркера GOOD.

## N49-06 — перенос «успеха» мог заменить recipient, не алгоритм

Старый ProjectWikiRequestRoutingTrainableModel.Train: лексические unigram/bigram признаки + обновляемые веса и biases, 18 эпох, lr0.14. [Донор](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L38-L65). Текущий аналог всё ещё имеет этот trainable механизм, но добавил gate. RP01 [CodeDocumentationSimulationDpoExperiment.cs][rp01] обучает сохранённую модель отдельными exact DPO материалами и q/v-LoRA. Поэтому потеря навыка могла возникнуть из-за смены **получателя/consumer**, а не из-за «сломали математику LoRA». Даже успешные привычные Wiki-механизмы не гарантируют, что автономный Student генерирует тот же ответ.

**Тест переноса:** четыре контрольных плеча Old donor, Current specialized Unit, Student-only, Student+Unit с запрещённой произвольной подменой примеров. Проверить identity state и provenance. Не включать эксперимент в P0-1 пока не восстановлен raw teaching material.

## Что уже есть в prior каталогах и НЕ выдаётся за новые открытия

| Направление из запроса | Наличие prior evidence | Задача при восстановлении / доменная граница |
|---|---|---|
| DPO objective, chosen/rejected, exact mask и teacher lineage | prior 00-MASTER, 03-DPO, Core41, Core76 | Отличать нынешний exact material от старого Compact(480); проверять финальный candidate→ordinary response, а не только loss |
| LoRA q/v против обучаемого output head; RMSNorm backward | [Core28][core28], Core47 | Проверка отдельно Student; не откатывать реальный работающий backward на старый |
| Vector tokenization, нестабильные ID, AST feature labels | prior 05-SEMANTIC-VECTOR, 48/N48, Core73 | Только C# AST/source semantics, единая схема признаков для всех файлов |
| kNN learned vs static first-hit; POS и контекст sentence | prior 04-KNN, 12-MATRIX-KNN, 47/48 | Domain-aware k, rank/gold/consumer, не универсальный для SQL+перевода+Wiki |
| DPO→Unit/Flow/Graph mapping и deterministic shadow | prior 06-UNITS, Core58 | Сопоставить фактических recipients, preserve many-to-many, correct real ordinary consumer |
| Checkpoint restore, optimizer/RNG и metadata-only import | prior 10-TRANSLATION, Core76/78 | Cold fresh process и равенство ответов/route; не путать neural weights с JSON |
| Replay/retention и скрытые old memory | prior E43, Core68, 00-LOCAL-WIKI | Разные old/new families, one-factor replay, не переносить оптимальный процент между нейронными/kNN доменами |
| EOS, vector scales, LR resumption | prior 47/48, PREPARETOGPL | Исследовать лишь если показан active caller в конкретном RP01, без фиктивного регрессионного статуса |

**Итог:** подтверждены реальные source transformations, но лишь короткий native semantic A/B способен установить, ухудшили ли они качество конкретного обученного получателя. Никакие исходные методы RP01 не были переписаны в данном исследовании.

## Source anchors
[ct_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120
[ct_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L129
[rr_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L110-L116
[rr_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L75-L168
[rr_runner]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L48-L100
[route_model]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs
[orch_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L129
[orch_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L205
[router_unit]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRouterUnit.cs
[qp_pairs]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pairs.jsonl
[rp01]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs
[core28]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/28-code-trace-learning-donor.md
[core76]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md
