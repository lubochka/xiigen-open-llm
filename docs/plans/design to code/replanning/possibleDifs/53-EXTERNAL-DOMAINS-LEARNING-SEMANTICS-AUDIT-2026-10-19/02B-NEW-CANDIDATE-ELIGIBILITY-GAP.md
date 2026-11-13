# KU-02. Недопустимый для ребра target меняет shortlist до вызова learned authority

## Вывод и статус

**SOURCE-CONFIRMED PRODUCER–CONSUMER ORDERING; ISOLATED_BOUND_PROBE_COMPLETE; NATIVE / LEARNING QUALITY NOT MEASURED.** `DeterministicRegistryEdgeCandidateProvider` сначала вычисляет shortlist относительно **всего** реестра, затем пересекает его с `edge.ToNodeIds`. Порог shortlist зависит от лучшего score во всём реестре. Поэтому добавление сильного кандидата вне разрешённых targets текущего ребра может удалить все его разрешённые кандидаты. Провайдер вернёт `EDGE_CANDIDATE_NO_MATCH`, а pipeline остановится до вызова обученного юнита. [Provider L138–178][provider] [Scorer L81–95][floor] [Pipeline L150–183][pipeline]

Это новая конкретная ошибка композиции двух допустимых операций: относительного порога и ограничения topology. Не утверждается, что KNN не умеет ранжировать, что любой порог надо убрать, или что модели надо разрешить чужие targets.

**Область и приоритет:** P1 conditional для графовой оркестрации с ограниченным исходящим edge targetset и реально подключённым этим provider. Документация, язык программирования, перевод затрагиваются только как исполнители такого графа. Это не установленная причина первого RP01 и не утверждение о качестве `TransformerEmbeddingKnnLogicalUnit`.

## Точная цепочка вызовов и отказа

| Исходник на `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` | Действие |
|---|---|
| `DynamicGraphEdgeFlow` L545–565 | При learned edge path вызывает переданный authority pipeline. [Flow][flow] |
| `DynamicGraphEdgeAuthorityExecutionPipeline` L150–183 | Вызывает `candidateProvider.Propose`; неуспех немедленно превращает в `EDGE_AUTHORITY_CANDIDATE_PROPOSAL_FAILED`. [Pipeline][pipeline] |
| `DeterministicRegistryEdgeCandidateProvider` L138–157 | Проверяет регистрацию всех разрешённых targets, но в `ScoreRelevant` отправляет весь registry corpus. [Provider][provider] |
| `DeterministicSubPhaseBaselineScorer` L81–95 | Сначала положительные scores всех entries; потом floor = best<0.20 ? best : max(0.20, 0.80·best). [Scorer][floor] |
| `DeterministicRegistryEdgeCandidateProvider` L164–176 | Уже сокращённый массив фильтрует через `allowedTargets.Contains`; пустой массив → `EDGE_CANDIDATE_NO_MATCH`. [Provider][provider] |
| `DynamicGraphEdgeAuthorityExecutionPipeline` L251–322 | Resolver и `unit.ExecuteAsync` находятся ниже отказавшей границы. [Resolver/inference][inference] |

В документации класса provider это называется candidate generation; граница pipeline заявляет learned unit единственным decision authority. Но сочетание global relative floor с поздним eligibility может заранее исключить допустимый конечный ответ. Это **скрытая власть semantic preselection над поддержкой кандидатов**, а не валидация целостности. [Pipeline declaration L13–20,58–59][pipeline_role]

## Реализуемый аналитический пример

Используем обычные registered subphase IDs и форму topology fixture из текущих scorer tests. Вход: `CompiledText="bug test"`, `CompiledPredicateIds=["COMMON"]`, пустой context, нейтральные name/purpose, никаких упоминаний numeric IDs. `5.1` разрешён текущим edge, а `4.4` находится в другом участке registry и в `edge.ToNodeIds` не входит. Anchor vocabulary допускает оба по словам `bug` / `test`. Состав predicates: у `5.1` — `COMMON, OTHER`; у `4.4` — `COMMON`. [Fixture support L303–339][fixtures] [Scoring L120–138][scores]

При таком входе id/token/context terms равны нулю; predicate fractions равны 1/2 и 1. Default `HybridRanker` даёт fused в [0,1], поскольку использует неотрицательные нормализованные BM25/KNN и pattern score с весами 0.45/0.35/0.20. [Hybrid L18–54][hybrid]

| Target | Допустим edge? | Raw score из исходной формулы |
|---|---:|---:|
| `5.1` | Да | 0.55·0.5 + 0.02·hybridA ∈ [0.275, 0.295] |
| `4.4` | Нет | 0.55·1.0 + 0.02·hybridB ∈ [0.55, 0.57] |

**Даже максимальный разрешённый score 0.295 меньше минимального global floor 0.80·0.55 = 0.44.** Значит разрешённый `5.1` обязательно исчезнет при любой допустимой величине hybrid обоих кандидатов. После позднего `allowedTargets` фильтра исчезнет и `4.4`. При реестре с одним `5.1` его собственный floor ниже его score, поэтому кандидат сохраняется. Это не требует NaN, ошибочных IDs, пустого topology или изменения learned weights.

На доменной панели `COMMON` следует заменить реальными predicates профиля и снять фактические native scores. Синтетическая форма здесь нужна для причинного доказательства порядка фильтров, а не для объявления `5.1` правильным ответом реальной пользовательской задачи.

## Сравнение с external_domains

В donor `external_domains@3036a01…` этот relative floor уже присутствовал внутри whole-development multi-router. Но его candidate universe в этой точке был **весь список маршрутов router**, без следующего пересечения с разрешёнными targets отдельного ребра. Перенос того же scorer в новый ограниченный edge-provider добавил взаимодействие, которого приведённый donor алгоритм не проверял. [Donor L145–166][donor]

Поэтому это **новый риск переноса функции в другой consumer**, а не доказанное ухудшение исторического neural обучения. Он релевантен пользовательскому замечанию «успешную функцию перенесли не на тот уровень алгоритма», но данным finding нельзя приписывать результаты E09/E13/E14 или обещать восстановление их качества. В приоритетном списке восстановление подтверждённых внешних экспериментов должно оставаться выше этой условной гипотезы.

## Контрдоказательства и проверка новизны

Текущий тест `RelevanceFloor_RemovesAWeakPositiveEchoBelowEightyPercentOfTheBestScore` подтверждает отсечение слабого кандидата при `edge.ToNodeIds=["5.1","4.4"]`: **оба** сравниваемых кандидата разрешены. Это корректный within-edge baseline контроль; он не проверяет влияние сильного кандидата вне edge. [Test L235–250][test]

Membership filtering сам по себе правилен и должен остаться: B не получает права направлять запрос в `4.4`. Возможная намеренная политика «абстенция, если где-то в глобальном topology есть более релевантный target» была бы отдельным cross-edge routing решением с явным сигналом и consumer. В просмотренном пути generic proposal failure такого решения не материализует.

В 661 pinned markdown не найдены `DeterministicRegistryEdgeCandidateProvider`, `EDGE_CANDIDATE_NO_MATCH`, `ScoreRelevant`, `relevanceFloor` или описание этой последовательности global floor → target eligibility. Общие старые тезисы о domain filters, lexical replays и rank normalization не являются этим механизмом: фильтр здесь существует, но выполняется после необратимого truncation. Свежие дополнения сверены интегратором; итоговый охват указан в общей методологии.

## Что уже выполнено

[probe_learning_delivery.py](evidence/routing/probe_learning_delivery.py) воспроизводит **только** source ordering / threshold arithmetic. В [probe_results.json](evidence/routing/probe_results.json):

- небольшой registry: разрешённый `5.1` сохраняется;
- расширенный registry с запретным `4.4`: current ordering даёт пустой shortlist;
- те же full-corpus raw scores, B eligibility-first: `5.1` сохраняется; `4.4` отсутствует.

В пробе использована граница, наиболее благоприятная для разрешённого кандидата: `5.1=0.295`, `4.4=0.55`. Это **аналитическая fixture, не записанные native scores**. C#, модель, обучение, restore и внешняя генерация не запускались; `learning_quality_verdict=NOT_MEASURED`.

## Малый эксперимент с доменной оценкой

**L1 — native причинная проверка.** Существующий fixture builder scorer tests позволяет создать два topology variants: (A) текущий edge и его разрешённые targets; (A+) тот же edge плюс registry target, не входящий в edge. Вход, context, source IDs и unit descriptor заморожены. Записать raw component scores, full shortlist до eligibility, edge allowed IDs, proposed IDs, error code, resolver call count и learned inference call count. Считать отсутствие вызова learned unit отдельно от неверного предсказания.

**Контроль алгоритма B:** сохранить raw scores, вычисленные на **том же полном registry**; сначала ограничить кандидатов разрешённым targetset, потом применить relative floor. Так из эксперимента исключается смена BM25 IDF, max normalization, corpus order и веса fusion. Простая замена full corpus на filtered corpus прямо в `ScoreRelevant` недостаточно чиста: она одновременно меняет эти score components. Для research arm достаточно выделить raw-score snapshot и переставить только один фильтр. Проверки registered target, immutable snapshots и admissible IDs не меняются.

**L2 — проверить качество законного маршрута.** Взять по 6 heldout задач в доменах документация и код плюс 6 orchestration задач, у каждой заранее определён допустимый edge и правильный target. Пара к каждому заданию отличается только наличием чужого, но лексически сильного узла в другой части registry. Этот distractor легально существует в общей системе, но не разрешён текущему edge. Использовать один и тот же реально обученный edge authority / checkpoint во всех arms, без дообучения между registry variants.

Если такого authority пока нет, bounded counting unit проверяет только L1; качество L2 остаётся неизвестным. Для малого обучения при наличии реального learner CODEX / Astra 6 ultracode выбирает и оценивает по 8 учебных DPO-пар на домен; внешняя открытая китайская модель только предлагает кандидаты. Train/heldout разделить по пользовательской задаче и topology family, а не только по строковому ID. Данные и labels одинаковы для A/B.

**Измерения:** сохранение gold в допустимом shortlist; доля реальных inference calls; точность learned target условно на вызове; выполнение domain law конечным executor; число запрещённых targets (должно быть 0). Результат B полезен, если он устраняет зависимость законного маршрута от чужих registry nodes и повышает завершение задач при неизменной условной точности trained unit. Не объявлять это улучшением DPO loss или LoRA.

**Опровержение:** реальные raw scores не создают stated floor crossing; traced composition использует другой provider или никогда не ограничивает edge targetset; существует явный согласованный consumer, превращающий такой global mismatch в правильный cross-edge handoff; либо retained candidate всё равно неверен на слепой доменной оценке и улучшения конечного поведения нет. Последний исход не отрицает вычислительное свойство порядка фильтров, но опровергает предполагаемый выигрыш качества.

## Классификация и минимальная граница работ

По `architect-behavior-classifier-SKILL.md`: окончательное ранжирование/выбор target принадлежит learned decision; проверка topology membership — fixed constraint; relative relevance threshold — semantic candidate support. Здесь они перепутаны по порядку, а не по необходимости существования. Исследовательский контроль проверяет одну границу producer/provider; не добавляет новый графовый уровень, не разрешает чужие targets, не требует широких CI/Windows прогонов.

[provider]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DeterministicRegistryEdgeCandidateProvider.cs#L138-L178
[floor]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DeterministicSubPhaseBaselineScorer.cs#L81-L95
[pipeline]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs#L150-L183
[pipeline_role]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs#L13-L59
[flow]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs#L545-L565
[inference]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs#L251-L322
[fixtures]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/DeterministicSubPhaseBaselineScorerTests.cs#L303-L339
[scores]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/DeterministicSubPhaseBaselineScorer.cs#L120-L138
[hybrid]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Context/HybridRanker.cs#L18-L54
[donor]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L145-L166
[test]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/DeterministicSubPhaseBaselineScorerTests.cs#L235-L250

