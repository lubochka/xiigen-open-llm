# KU-01. Два корректных сегмента одного target отклоняют весь learned маршрут

## Вывод и границы

**SOURCE-CONFIRMED CONTRACT COLLISION; ISOLATED_FORMULA_REPRODUCED; NATIVE / LEARNING QUALITY NOT MEASURED.** В текущем `MultiSubPhaseRouterUnit` два сегмента с одним правильным `PredictedSubPhaseId` могут не дойти до исполнения: объединитель требует одинаковый `DocumentId`, хотя штатный компилятор специально создаёт уникальный ID каждому сегменту. Условие воспроизводится даже при полностью одинаковых confidence, predicates, phase и остальных смысловых полях. Это конкретная несовместимость producer → aggregate, а не утверждение, что вся детерминированная валидация ошибочна. [Producer L136–155][producer] [Aggregate L125–145][merge] [Equality L738–752][equality]

**Условие достижимости:** вызван именно текущий multi-router, ему явно передан корректный resolved learned authority, включён `UseLearnedUnitWhenAvailable`, оба сегмента имеют непустой baseline shortlist и оба learned результата прошли индивидуальную проверку. Конструктор не подключает learned authority автоматически. Поэтому finding **не доказывает причину первого RP01 и не распространяется на default baseline без этого подключения**. [Constructor L39–55][constructor] [Cutover L202–220][cutover]

**Приоритет:** P1 conditional для совместного выполнения нескольких задач по документации, коду и оркестрации. В общем списке идёт после подтверждённых экспериментов восстановления обучения из `external_domains`. Это не доказанный дефект LoRA/градиента.

## Пошаговое доказательство

| Граница | Что делает точный исходник | Почему это существенно |
|---|---|---|
| Канонический producer | `PromptRoutingInputCompilerUnit.BuildSegmentDocumentsAsync` строит ID `{documentId}-segment-{index + 1}`; каждый сегмент сохраняет свой текст и predicates. [L136–155][producer] | Два самостоятельных сегмента по конструкции имеют разные ID. |
| Вызов consumer | `AdaptiveLogicalOrchestrator.RouteSubPhasesAsync` передаёт snapshot source в настроенный multi-router; ошибка зависимости немедленно возвращается вызывающему. [L1253–1275][caller] | Ошибка агрегации влияет на обычный исполнительный путь при разрешённом подключении learned seam. |
| Индивидуальный learned result | Router строит каждое `SubPhaseRoutingDecision.DocumentId` из `request.Input.DocumentId.Trim()`. [L484–506][output] | Learned unit не подделывает ID: он возвращает target/confidence, ID добавляет доверенная обвязка. |
| Группировка | Ключ `learnedDecisionByTarget` — только target ID; второй результат того же target вызывает equivalence check. [L125–145][merge] | Разные сегменты закономерно встречаются в одной группе. |
| Эквивалентность | Первое сравнение — точное равенство source `DocumentId`. [L738–752][equality] | При сегментных ID оно ложно независимо от качества learned решения. |
| Наблюдаемый выход | Ошибка `MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID`; вызывающий не исполняет ветки. [L884–887][error] [L1268–1275][caller] | Правильное обучение каждого юнита может выглядеть как ухудшение общей системы. |

**Нормальный случай:** «Design an OpenAPI request contract and then design an OpenAPI response contract». Это две задачи одному API-contract unit `3.3`; golden target set может быть `{3.3}`. Гипотеза **не требует двух отдельных запусков** этого target. Достаточно, чтобы единственный агрегированный маршрут сохранил оба источника и дошёл до исполнителя. Натуральный prompt ещё надо прогнать через native compiler и записать фактические сегменты; в выполненной ниже пробе ID построены по подтверждённой формуле producer, а результаты сконструированы вручную.

Другие доменные панели: «добавить тест для X, затем тест для Y» → один testing target; «описать первый раздел API, затем второй» → один documentation target. Перевод релевантен только если несколько фрагментов действительно обслуживаются через эту orchestration seam. Самостоятельный `TranslationMachine` не затронут этим доказательством.

## Сравнение с external_domains

В `external_domains@3036a01…` multi-router объединяет сегменты через `GroupBy(PredictedSubPhaseId)`, выбирает один результат по confidence и сохраняет все segment IDs в evidence carrier. Разные source IDs сами по себе не вызывают отказ. В текущем коде этот алгоритм остался для baseline, но для learned outputs добавилось строгoе сравнение source IDs. [Donor L103–141][donor] [Current L147–174][baseline]

Это **прямое различие агрегации** между snapshots, но исторический donor path в данном месте — deterministic baseline. Мы не выдаём его за доказательство успешно обученного edge-authority checkpoint и не предлагаем переносить произвольный max-confidence победитель как окончательную learned политику. Восстанавливаемый закон уже: «два разрешённых источника с одинаковым смысловым маршрутом не являются конфликтом только из-за разных provenance IDs».

## Контрдоказательства и почему текущие тесты недостаточны для этого случая

- Тест `TwoSegmentsInvokeLearnedAuthorityExactlyTwiceAndNeverInvokeParent` использует разные targets `3.3` и `4.1`; collision group не возникает. [L365–399][test_different]
- Тест `LearnedSegmentsRetainFirstExactDuplicateInStableOrder` дважды передаёт сегмент с **одинаковым** искусственным ID `segment-shared`. Это доказывает intended exact-repeat поведение, но отличается от реального producer. [L403–433][test_same]
- Тест `LearnedSegmentsRejectConflictingDuplicateTarget` намеренно отвергает различный confidence. Это конфликт по текущему carrier-контракту, а не само по себе доказательство смыслового противоречия двух задач. Узкая B-проба сохраняет этот отказ; политику объединения разных confidence она не меняет. [L436–466][test_conflict]
- Строгая provenance-проверка нужна: чужой ID, несовпадающий phase, неизвестный target, нечисловой confidence и конфликт learned решений не должны проходить. Исправление границы не означает удаление этих проверок.
- При единственном сегменте, разных targets или отключённом learned seam именно этот дефект не активен.

## Что выполнено сейчас

[probe_learning_delivery.py](evidence/routing/probe_learning_delivery.py) и [probe_results.json](evidence/routing/probe_results.json) воспроизводят только операции carrier equality / grouping из cited source. C#, модель, DPO training, checkpoint restore и внешняя модель не запускались; `dotnet` в текущей среде отсутствует. Это **арифметическая/структурная проба, не native тест качества**.

| Сконструированный контроль | Current formula | Donor baseline formula | Узкий B-контроль |
|---|---|---|---|
| Разные разрешённые segment IDs, один target, всё остальное совпадает | Ошибка | Один target | Один target + оба source IDs |
| Полностью одинаковый повтор, включая ID | Один target | Один target | Один target |
| Два разных targets | Два targets | Два targets | Два targets |
| Один target, различный confidence | Ошибка | Один winner | Ошибка |

B сравнивает смысловые поля после проверки каждого source ID и отдельно хранит provenance. Он не вводит новую learned политику выбора между конфликтующими ответами.

Различный confidence остаётся отрицательным контролем действующего контракта; даже одинаковые targets с разной уверенностью не объединяются этим B автоматически. Другие возможные правила агрегации потребуют отдельного обоснования.

## Малый native / доменный эксперимент

**L1 — до обучения, одна граница.** В изолированной исследовательской копии использовать реальный `PromptRoutingInputCompilerUnit`, текущий router и один counting learned authority с фиксированным корректным typed decision. Взять два normal prompts на API documentation и testing, плюс те же задачи по одной. Снять source IDs, shortlist каждого сегмента, learned call count, индивидуальные ответы, aggregate error/target set, вызов реального capture-executor. Контроль B меняет только критерий согласованности provenance при одинаковом смысловом решении; сохраняет validation и current target-level cardinality. Не подменять эту проверку arbitrary string fixture с повторным `segment-shared`.

В L1/L2 записать **фактический вход downstream executor**: оба исходных текста, source IDs и связь с выбранным target. Outer EvidenceIds сам по себе этого не доказывает: multi-router сохраняет их во внешнем LogicalUnitResult, а `RouteSubPhasesAsync` возвращает ownedDecisions. Если полный исходный request уже сохраняет обе задачи, зафиксировать именно этот путь. IDs только в отчёте/outer evidence означают промежуточный результат, а не восстановленную доставку двух задач. [Multi-router:157–175][source-carrier] [Caller:1275–1332][returned-carrier]

**L2 — влияние на доставку результата обучения.** Для каждого из доменов документация и код подготовить по 8 человечески различимых учебных DPO-пар и 6 новых heldout пар: две операции одному target; операции разным targets; одна операция. CODEX / Astra 6 ultracode выбирает и проверяет пары. Открытая китайская модель, если доступна исполнителю, только предлагает кандидаты. Её вызов, имя и revision записываются в receipt; автоматический evaluator из генератора не делается. Если реальный обученный authority недоступен, остановиться на L1 и записать `TRAINED_AUTHORITY_UNAVAILABLE`, а не называть mock обучением.

Один frozen trained authority / один checkpoint используется в обеих arms. Данные, compiler, tokenizer, LoRA, IDs, prompts, порядок вызовов и разрешённый targetset одинаковы. Измерять **отдельно**: точность индивидуального learned target; долю успешных агрегаций правильных индивидуальных ответов; полное покрытие обеих частей пользовательской задачи конечным результатом; чужие/конфликтные источники. Успех: выигрыш B в same-target multi-segment группе при неизменной индивидуальной точности и нуле принятия настоящих конфликтов. Это улучшение интеграции обученного решения; отдельно утверждать «улучшились градиенты» нельзя.

**Критерии опровержения:** native compiler в заявленном профиле не создаёт несколько сегментов; configured host не подключает этот learned seam; каждый разрешённый пример обязан иметь разные targets по явному контракту; либо реальные корректные ответы проходят current aggregate без ошибки. Если улучшится только synthetic counting-authority fixture, статус остаётся mechanism-confirmed / quality-not-established.

## Новизна и архитектурная классификация

Precise symbols `AreEquivalentLearnedSegmentDecisions`, `TryBuildLearnedAuthorityInput` и `MultiSubPhaseRouterUnit` проверены по корпусу 661 pinned markdown (389 planning + 272 protected core). Встречен общий исторический router/hybrid mapping и темы learned graph order, но не этот producer-ID × target-key конфликт. Это новая конкретизация **контракта агрегации**, не переименование общих проблем many-to-many / graph-order. Свежие дополнения сверены интегратором; итоговый охват указан в общей методологии.

По `architect-behavior-classifier-SKILL.md`: конечный выбор target — learned decision; membership/identity/integrity checks — fixed checks; комбинация нескольких законных source identities — ответственность assembly contract. Нужна проверка одной границы, а не новый Unit, новый resolver или превращение guard в ML.

[current]: https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76
[producer]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs#L136-L155
[merge]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L125-L145
[equality]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L738-L752
[constructor]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L39-L55
[cutover]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L202-L220
[caller]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L1253-L1275
[output]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L484-L506
[error]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L884-L887
[donor]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L103-L141
[baseline]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L147-L174
[test_different]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/MultiSubPhaseRouterLearnedAuthorityTests.cs#L365-L399
[test_same]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/MultiSubPhaseRouterLearnedAuthorityTests.cs#L403-L433
[test_conflict]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/MultiSubPhaseRouterLearnedAuthorityTests.cs#L436-L466
[source-carrier]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L157-L175
[returned-carrier]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L1275-L1332

