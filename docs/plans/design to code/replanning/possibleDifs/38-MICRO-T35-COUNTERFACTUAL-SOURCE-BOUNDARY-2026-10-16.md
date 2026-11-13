# MICRO-N38-T35 — проверка, что typed Unit читает новое решение, а не возвращает готовый oracle

**Статус: DESIGNED, NOT_EXECUTED (16.10.2026).** Это быстрый A/B контрфактический опыт **по домену: Phase3→Phase4 технический дизайн / передача решений**, НЕ обобщённый neural DPO experiment. Для его выполнения требуется лишь малая native .NET сборка и один тестовый вызов, без тренировки Qwen, 500 rows, Windows runner или фиксации CI-GREEN.

**ExperimentId:** `MICRO-N38-T35-CONTEXT-DEPENDENCY`. **Tier:** 1 (три входа, один Unit, три проверяемых пары утверждений). **Baseline A:** исходный T34 пакет; **Counterfactual B:** тот же ID и формат, изменено одно/два архитектурных решения; **Negative control C:** прежний A с выключенным carry forward.  
**Primary metric:** `unsupported_source_count_B` = число положительно заявленных `SourceId`, отсутствующих/противоречащих upstream решению B. Дополнительно `outbound_change_count_AB`, `confidence_delta_AB`, `control_C_rejected`, `semantic_gate_response`. `TestPassed` не подменяет качество модели.

## Что уже доказано исходником до запуска

- `BuildPackets(phaseContext)` игнорирует parameter через `_ = phaseContext;`, затем выдаёт константные 10 пакетов: [строки 67–86](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs#L67-L86).
- `ExecuteAsync` проверяет flags и возвращает `Confidence=0.92`: [строки 23–64](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs#L23-L64).
- Старый `T35LogicalUnitReplacementProof.BuildEvidenceAsync` сравнивает с одним тем же oracle, не строя изменённый input: [строки 54–108](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/T35LogicalUnitReplacementProof.cs#L54-L108).

## Fixture, обязательная фиксация ДО результата

Содержимое [38-MICRO-T35-COUNTERFACTUAL-CASES-2026-10-16.json](38-MICRO-T35-COUNTERFACTUAL-CASES-2026-10-16.json): три входа с восьмью полями record `PhaseContextPacket`. `A` содержит `decision:dynamic_runtime_topology` и `decision:internal_first`; `B` заменяет `dynamic_runtime_topology` на `decision:static_runtime_topology` и удаляет `internal_first`; `C` наследует A, но `UseContextCarryForward=false`. Нужен семантический comparator: если B утверждает статический runtime, вывод `td01_runtime_orchestration_decision` как `covered` с `dynamic_runtime_topology` не разрешён. То же для internal first и `td02`.

Не допускается подмена `B` одинаковыми default/empty null arrays, которая создала бы вне-контрактный объект: допустимо противоречивое, но корректно типизированное решение.

## Native micro implementation — изолированно, без изменения production

1. Создать временный тест с ссылкой на `ExperimentsV2` assembly; прочитать 3 объекта из JSON (либо построить теми же полями inline); вызвать `new T34TransitionDomainSplitterUnit().ExecuteAsync(new LogicalUnitRequest<PhaseContextPacket>(runId, Id, Role, DomainTag, packet, new LogicalExecutionFlags { UseContextCarryForward=... }, new Dictionary<string,object>()))`. Если `LogicalExecutionFlags` read-only по конструкции — использовать соответствующий публичный ctor/инициализатор из [контракта](https://github.com/lubochka/llm_mvp_core/tree/d1862939bfe24c3bbeb12a49af917c7ca569a10f/21.AgentInfrastructure/Logical), не менять сервис ради теста.
2. Для A и B составить canonical digest из `PacketId|DomainLabel|CoverageStatus|ordered SourceIds|PromptFragment`; сравнить 10 output по индексам, не полагаться на порядок словаря.
3. Посчитать неподкреплённые B `decision:dynamic_runtime_topology` и `decision:internal_first`, сверяясь с `DecisionsMade`; отчёт должен включать **actual** input `DecisionsMade`, output `SourceIds`, flags, `Confidence`, `IsSuccess`, версии кода, SHA и тип caller.
4. Запустить C и убедиться в `IsSuccess=false` при выключенном `UseContextCarryForward`; этот положительный для guardrail отрицательный контроль не доказывает, что смысл input был прочитан.
5. Протестировать сравнительную **проектируемую** версию B' только в shadow: перед добавлением пакета требовать реальный `SourceId` из input и корректный статус, иначе возвращать `unsubstantiated/unknown`. **Не править production** до доказательства источника качества и выбора владельца интерфейса. В A' оставить все исходные решения и требовать, чтобы правильный oracle A продолжал работать.
6. Не объявлять «Unit научился» по успеху A' или одной смене статуса. Для этого нужно отдельно добавить настоящий урок, изменённый `state_version`, сохранение и повторный обычный consumer call на скрытом контексте.

**Predicted from source, NOT measured:** A и B возвращают одинаковые десять body-пакетов, `confidence_A=confidence_B=0.92`; B включает минимум два unsupported source claims; C прекращается fail. Если измерение иное, сначала пересмотреть pinned commit, actual request path и сборку — **не подгонять ожидаемые данные**.

## Decision / falsification

| Исход native опыта | Интерпретация | Дальнейшее действие |
| --- | --- | --- |
| A=B, B содержит неподкреплённые source claims, C отказал | Source-based гипотеза получила runtime подтверждение в **этом Unit** | P1: проверка product callers и решение о learnable replacement |
| A≠B, входы отличаются только decision | Исходная гипотеза о таком runtime не подтверждена | Снять `CONFIRMED_RUNTIME`; проверить версию и скрытый preprocessing |
| A/B одинаковы, но consumer фиксирует `unsupported` и не допускает downstream | Unit static, но downstream защита работает | Не предъявлять user-visible defect |
| Изолированный класс не вызывается RP01 | Нет доказанного пути к neural regression | Не переносить findings на 0/3 и 0/4 |

**Acceptance научного опыта:** сохранить *все* A/B/C результаты и ложные срабатывания; оригинальные E/T35 JSON не трогать; сравнение semantic oracle B задано **до** run; не выбирать только удачные counterfactuals. Достаточно 3 вызовов и 1 readback; никаких сотен тестов.

## Доменная применимость

| Домен | Применимость | Почему |
| --- | --- | --- |
| Архитектурный handoff / DesignToCode | **Высокая при фактическом использовании T35** | Источник решений может исчезнуть, а фиксированное правило сохранится |
| Проектирование workflow / code checks | Условная | Только для consumer, зависящего от этого Unit |
| EN↔RU | **Не применять** | Этот T35 не выполняет translation KNN или LoRA |
| C# neural DPO RP01 | **Не применять без call trace** | Этот Unit не участвует в показанном `DpoTrainer→TextGenerator` |

Проверка независимым CODEX/Astra: после source readback рассмотреть input decision contradiction, не требовать успешного native run прежде, чем инженер оценит fixtures. Статус `PROPOSED` не выдавать за `PASSED`.