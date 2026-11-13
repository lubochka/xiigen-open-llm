# N38-T35 — Детерминированный Unit принимает контекст, но его содержимое не влияет на ответ

**16.10.2026 | DIRECT_SOURCE_VERIFIED | NEW_CAUSAL_HYPOTHESIS | NATIVE_RUN_NOT_EXECUTED | RP01_CAUSAL_LINK_NOT_ESTABLISHED.**  
Research target: `llm_mvp_core/codex/dpo-implementation`, закреплённый просмотренный commit `d1862939bfe24c3bbeb12a49af917c7ca569a10f`. Публикация: исключительно `xiigen-planning/llm_plan3/.../possibleDifs`. `llm_mvp_core/docs/plans/replanning-possible-diffs` — READ ONLY.

## 1. Что действительно найдено

**У Unit есть правильный интерфейс передачи контекста, но отсутствует фактическая зависимость содержимого выходного пакета от контекста.** Это структурный пробел переноса типа `deterministic station → typed Unit`, а не заключение, что все Units в проекте детерминистические.

| Источник | Конкретное наблюдение | Степень |
| --- | --- | --- |
| [T34TransitionDomainSplitterUnit.cs#L11-L26](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs#L11-L26) | `ILogicalUnit<PhaseContextPacket,DomainPacket[]>`; `Confidence = 0.92` — константа | **CODE** |
| [тот же #L26-L64](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs#L26-L64) | `ExecuteAsync` проверяет cancellation и flags, затем вызывает `BuildPackets(request.Input)` и возвращает указанную константу confidence | **CODE** |
| [тот же #L67-L86](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs#L67-L86) | `BuildPackets(phaseContext)` выполняет `_ = phaseContext;` и возвращает **десять заранее заданных DomainPacket**, включая `decision:dynamic_runtime_topology` и `decision:internal_first` | **CODE — существенная улика** |
| [T35LogicalUnitReplacementProof.cs#L54-L108](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/T35LogicalUnitReplacementProof.cs#L54-L108) | И тестовый Unit, и эталон получают один сценарий T34; сравниваются выходные packet IDs/status/source IDs и confidence. Мутация входных решений не создаётся | **CODE** |
| [T34LogicalFlowAdapter.cs#L19-L40](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/LogicalFlow/T34LogicalFlowAdapter.cs#L19-L40) | Flow-adapter пересобирает неизменный T34 oracle `BuildEvidence()`, а не использует текстовый `request.Input` для новых сценариев | **CODE; отдельная граница** |

**Вывод только по данному классу:** для двух валидных `PhaseContextPacket` с различающимся содержанием и равными flags результат `BuildPackets` одинаков по логике тела метода. Он не изменяется ни от удаления решения, ни от замены знания, ни от появления нового домена. `0.92` — заявленный уровень уверенности, **не оценка, полученная из изменяющегося состояния или отложенной выборки**.

## 2. Где возможно ложное прочтение «успешного обучения»

- **Что T35 действительно доказывает:** соответствие *предопределённому* T34 oracle в одной конфигурации, что полезно как контракт переноса конкретной deterministic station и интерфейса `ILogicalUnit`.
- **Что T35 НЕ доказывает:** обучение Unit на DPO/feedback, semantic generalization, реакцию на другой контекст, recall отсутствующих/противоречивых решений, калиброванную уверенность.
- **Нельзя объявить ошибкой:** фиксированные пакеты являются сознательно скопированным каноническим T34 oracle для *этого узкого scenario*, если продукт не позиционирует их как general trainable Unit.
- **Риск возникает**, если эта замена используется для новой фазы/проекта или её `confidence` и паритет с эталоном принимаются за «обученную структуру». Тогда отсутствующее upstream решение может быть **выдумано на выходе** как `covered`.

## 3. Связь с конкретными запросами RP01 и историей

Контекст RP01: [сводка причин и опасных переносов](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [44 различия](01-PRIORITIZED-DIFFERENCES.md), [история веток и успешных обучений](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Историческая translation память [TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/27.TranslationDomain/TranslationTrainedUnits.cs) изменяет maps/knn/learned verification, тогда как **этот** T35 Unit ничего не обучает в собственном `ExecuteAsync`.

**Не установлено:** что neural RP01 `DpoTrainer → TextGenerator` вообще вызывает T35; что именно T35 ухудшил 0/3 C# и 0/4 ENRU; что документированный T35 PASS ложен для своей original parity-цели. Категория — `CONFIRMED_CODE_SEAM / NEW_UNIT_GENERALIZATION_HYPOTHESIS / RP01_CAUSE_UNPROVEN`.

## 4. Быстрый различающий эксперимент

[38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md](38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md) и [неизменяемая контрольная панель JSON](38-MICRO-T35-COUNTERFACTUAL-CASES-2026-10-16.json).

Три настоящих захода:
1. **A:** исходный корректный handoff Phase3→4 с решением dynamic runtime.
2. **B:** такой же `PhaseContextPacket`, но dynamic runtime и internal-first удалены или заменены явно противоречащими решениями. Другие поля, unit ID, flags и время оценки неизменны.
3. **C — отрицательный контроль:** тот же A, но `UseContextCarryForward=false`: настоящий gate обязан отказать.

Считать не count пакетов, а: `unsupported_source_ids` для B, `changed_outbound_packets` A↔B, неизменность confidence, и успешность C. Настоящий counterfactual *не должен* сохранять утверждение `decision:dynamic_runtime_topology` как `covered`, если такого решения нет во входе. Не нужно учить большую модель и запускать Windows CI.

## 5. Reviewer / архитектура / человек

- **Архитектура:** разделить (а) read-only oracle parity и (б) learnable Unit с `state_version`, `training_example_id`, explicit input-to-evidence binding; менять production алгоритм только после измерения дефекта.
- **План/QA:** единый источник каждой `SourceId`, counterfactual must fail safely, strict labels `RULE_PASS` vs `LEARNING_GAIN`.
- **Code review:** проверить реальный вызывающий `T34LogicalFlowAdapter` и все consumers `Confidence`, прежде чем делать вывод о пользовательском качестве.
- **Prompt/human:** плохое утверждение — «T35 Unit обучился и уверенно выдаёт правильные знания». Правильное — «T35 воспроизводит ten-packet oracle при фиксированной исходной конфигурации; перенос на отличные входы не подтверждён».
- **Ожидаемый verdict:** исследовательский DRAFT, не `PRODUCT_DEFECT_PROVEN`; независимая проверка кода/выходного клиента требуется до promotion.

**Сделано сейчас:** адресно сверены файлы и логика; **НЕ сделано:** native .NET run, реальный изменённый checkpoint, RAG-полный обход, independent CODEX/Astra review, изменения кода или защищённого каталога.