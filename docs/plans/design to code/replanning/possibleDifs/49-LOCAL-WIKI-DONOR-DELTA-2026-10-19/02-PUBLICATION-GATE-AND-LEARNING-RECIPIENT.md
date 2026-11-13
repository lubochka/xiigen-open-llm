# N49-02 — обученный Wiki Router может остаться не опубликованным; рядом работает другой router

**Статус:** source-level конъюнкция VERIFIED, невыполненный native publication/restart test. Не считать safeguard дефектом автоматически.

## Проверенная цепочка текущей версии

1. `ProjectWikiRequestRoutingDpoGenerator.BuildBatchAsync` строит осмысленные `Prompt`, `ExpectedRouteKind`, `Chosen`, `Rejected`: [generator:27-42](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingDpoGenerator.cs#L27-L42).
2. `ProjectWikiRequestRoutingTrainingRunner.TrainAndEvaluateAsync` создаёт `ProjectWikiRequestRoutingTrainableModel.Train(trainingRows)`; на heldout вызывает `RouteForEvaluation(row.Prompt)`, затем вычисляет `routeAccuracy` и confidence: [runner:42-65](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L42-L65).
3. В **этом** runner жёстко стоит `var semanticQualityClaimed = false`, а `gateEvidence` получает `PublishAllowed = heldOutGatePassed && semanticQualityClaimed`: [runner:63-84](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L63-L84). Для всех путей через данный метод значение publish gate математически false.
4. Пайплайн читает `var snapshotAllowsPublication = snapshot is not null && snapshot.PublishAllowed`, и если false, возвращает пакет с `publication=null` и предупреждением, **не вызывая publisher**: [pipeline:78-113](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L78-L113).
5. Одновременно `ProjectWikiRequestRoutingTrainableModel.Route(prompt)` проверяет existence/trained state, top-two margin, attached heldout gate, calibrated confidence, а `RouteForEvaluation` минует именно требование attached evaluation gate: [model:95-102](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L95-L102), [model:155-202](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L155-L202).
6. В проверенном обычном **training orchestrator** `RunCoreAsync` вызывается `_router.Route(prompt)`, где `_router` по умолчанию — **`ProjectWikiRequestRouterUnit`**, а не `ProjectWikiRequestRoutingTrainableModel`: [orchestrator:12,28-46](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L12-L46), [orchestrator:205-265](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L205-L265). Последний в [`ProjectWikiRequestRouterUnit.cs:20-56`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRouterUnit.cs#L20-L56) использует статические keyword/ContainsAny правила.

## Почему это важно: three different concepts

- **Raw example:** `ProjectWikiRequestRoutingDpoPair`, который может содержать правильную и неправильную целевую маршрутизацию.
- **Обучаемый получатель №1:** `ProjectWikiRequestRoutingTrainableModel`, token-perceptron по `ExpectedRouteKind`; его веса и snapshot — не q/v LoRA и не neural DPO checkpoint.
- **Обучаемый получатель №2:** отдельный `ProjectWikiDpoMetricRunner` и его policy/reference; при нынешней проекции они видят defect types и synthetic tokens, не исходные запросы.
- **Ordinary training orchestrator consumer:** `ProjectWikiRequestRouterUnit` с детерминированными правилами, а не очевидным импортером одного из двух обученных состояний. Это не доказывает, что **нигде во всём репозитории** нет learned router path; надо прочитать всех consumers/injections/locator на следующем шаге.

В старом `local_wiki_planning` default training orchestrator **тоже** имел статический `ProjectWikiRequestRouterUnit`, поэтому отсутствие adoption нельзя объявлять вновь внесённой регрессией от одного сравнительного diff. Текущая невозможность publication из данного runner и дополнительный heldout gate — **source-level изменение контрактов**, его последствие требует отдельного causal proof.

## Риск: встроенный false-negative publication

Даже если `routeAccuracy=1`, `wrongAnnotationRate=1`, `averageConfidence > 0.85`, и отдельный DPO получил хороший score, `semanticQualityClaimed=false` исключает публикацию именно из данного pipeline. Это может быть **намеренной защитой от плохого heldout**, а не ошибкой. Она корректна как safety gate, но без реализации независимого semantic assessor превращается в вечное состояние «обучаем, но не используем». Недопустимые решения: `semanticQualityClaimed=true` без смысла, убрать guard, подкрутить метрику после результата, тайно применять sidecar snapshot.

## Новая проверяемая гипотеза и независимые контрольные случаи

**N49-02:** снижение полезности Wiki routing может быть следствием того, что новое состояние не принято/не восстановлено/не вызывается, а не только плохого gradient. Минимальные шаги:
- SOURCE/C: на 1 небольшом `ProjectWikiRequestRoutingTrainingRunner` пакете измерить ровно `HeldOutGatePassed`, `SemanticQualityClaimed`, `PublishAllowed`; зафиксировать, откуда появились snapshots.
- L: две близкие фразы о `CodeTraceTraining` и `AnswerComposerTraining`, независимый human/AST gold и 8–16 учебных контрастов; проверить что маршрут до→после меняется **через learned model** и не только статический `ContainsAny`.
- U: новый процесс, точный snapshot hash/token features/gate, locator, `Route(prompt)`, затем **обычный wiki orchestrator**, assert `chosen route == trained route`; сравнить без learner/с learner. Если отсутствует настоящее подключение к user flow, НЕ заявлять восстановленную обучаемую способность даже при improved metric.
- Без участия `ProjectWikiRequestRoutingTrainableModel` обычным consumer proof ограничен trainer-only. Если же другой route consumer найден, записать его как альтернативу и изменить статус гипотезы.

**Falsifiers:** gate оказался истинным на иной live verified branch/runner; обычный consumer получает тот же checkpoint и приобрёл правильный новый маршрут без дефектных shortcut; разница `fresh-process restored` отсутствует при точном hash/same fixtures.

## Историческая оговорка

В [архиве run043](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json) structural route accuracy=1.0 при DPO heldout=.88 и `publication=null`. **Он не доказывает**, что новый current gate вызвал отсутствие публикации *в прошлом*: этот исторический JSON был создан на иной временной версии code/runtime; branching HEAD и дата сохранения данных различаются. 

**Не путать:** read-only [`43.LearningLocalWikiData/reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.md`](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.md) содержит `training_started:false`, `new_batches_created:0`, `TargetUnitName:NONE`; а migration report `accepted_count=35`, `registry_validated=True` — это **данные реестра, не измеренный новый learning gain**.
