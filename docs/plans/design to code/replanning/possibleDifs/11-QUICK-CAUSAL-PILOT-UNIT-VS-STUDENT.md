# 11 — Быстрый причинный микроэксперимент: потерянное обучаемое состояние переводческого Unit

**XIIGEN / 16.10.2026 · EXPERIMENT_PLAN / NOT_RUN / NOT_INDEPENDENTLY_REVIEWED**  
**Цель:** одним недорогим действием определить, **влияет ли один из механизмов, присутствовавших в раннем переводческом контуре и отсутствующих в изолированном RP01 Student path, на качество научаемого поведения**, не смешивая это с ошибками DPO или восстановлением модели. Это уточнение [00 — master synthesis](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [DIF-08/DIF-25/DIF-26](01-PRIORITIZED-DIFFERENCES.md), [08 — causal diagnostics](08-CAUSAL-DIAGNOSTICS.md), **не** новая RP01 recovery очередь.

## Что проверяется: одна подмена, а не 10 функций одновременно

**H-LU-01 (первичная гипотеза):** отдельный `ExecutorUnit` способен **из 8 корректно предъявленных примеров** сохранить action/object `PhraseMappings` и применить их к **8 ранее не предъявлявшимся комбинациям**. Это полезное *обучение дискретного Unit state*, которого не создаёт одно лишь DPO-обновление весов изолированного RP01 `TextGenerator`.

**Прямые кодовые якоря:**
- [TranslationMachine.TrainStructuredBatchAsync, строки 89–149](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L89-L149) действительно передаёт `TranslationLearningExample` соответствующему `ITranslationUnitTrainer.TrainOnExample`. **Не вызывать `TrainBatchAsync`:** [строки 51–86](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L51-L86) — legacy scaffold с псевдоприростом accuracy.
- [ExecutorUnit.TrainOnExample, строки 403–455](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) записывает exact-source, signature/prefix/structure KNN, word и phrase maps. [Execute, строки 263–294](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L294) выбирает learned path; [TryComposeFromPhraseMappings, строки 798–854](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L854) комбинирует ранее сохранённые фразы.
- [TranslationMachine.CreateRuntime, строки 714–790](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L714-L790) соединяет тот же Executor с обычным `TrainedTranslationAdapter`; [GetOrchestratorAsync, строки 653–660](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L653-L660) даёт настоящий consumer. [TranslationRoutingFlow, строки 65–115](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationRoutingFlow.cs#L65-L115) передаёт результат.
- [RP01 simulation, строки 103–139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L103-L139) обучает отдельный `LlmModel`, **не вызывает этот Unit в данном offline path**. Это CODE gap, но пока **не доказанный root cause** отсутствия neural free-generation gain.

**Одна переменная:** наличие **правильно обученного `ExecutorUnit`** с теми же исходными компонентами `TranslationMachine`. Не менять словарь, teacher, B0, LoRA, kNN-index настройки, semantic tokenizer или фильтр ответов между условиями. Нельзя сравнивать разные языки: **весь pilot EN→RU**.

## Готовый минимальный набор

Файл [11-QUICK-CAUSAL-PILOT-FIXTURE.json](11-QUICK-CAUSAL-PILOT-FIXTURE.json) содержит **8 TRAIN**, **8 HELDOUT** (только новые комбинации уже встреченных элементов) и **4 LAW CHALLENGES**. Все строки синтетические/проектные примеры **для разработки**; они не были заимствованы из frozen C#500/ENRU500, не являются внешним teacher-source, не auto-admitted в DPO и не должны подаваться в production training без явной политики.

Пример двух TRAIN (вся восьмёрка в fixture):
- `validate the file → проверь файл`, состав: `validate→проверь` + `the file→файл`;
- `delete the cache → удали кэш`, состав: `delete→удали` + `the cache→кэш`.

Ни один full input этих двух уроков не входит в scored HELDOUT:
- `delete the file → удали файл`;
- `validate the cache → проверь кэш`.

Остальные 6 — аналогичные новые action/object сочетания. Это **тест композиционного переноса внутри знакомого словаря**, а не доказательство обобщения на новый предметный закон. 4 challenges ловят потерю отрицания (`do not`), количества (`two`), второго объекта (`and`) и режима вопроса (`did you`). Они должны **не стать уверенными неверными командами**; если текущий Unit не способен выразить/верифицировать law, честно фиксировать пробел.

## Четыре условия; из них только 3 нужны для первого результата

| Условие | Изменение относительно baseline | Что измеряем |
|---|---|---|
| **A — FRESH / no learning** | Новый `TranslationMachine()`, **никаких уроков**; `GetOrchestratorAsync().RunAsync` по 12 frozen inputs | Уже встроенные правила и baseline без выученных фраз |
| **B — LEARNED CORRECT** | Другой новый `TranslationMachine()`, только **8 допущенных shadow-unit примеров** в `TrainStructuredBatchAsync` для `executor-en_ru`; затем те же 12 запросов | Причинное влияние обновившихся Unit maps |
| **C — SHUFFLED LABEL negative control** | Третий свежий `TranslationMachine()`, тот же набор, но **на отдельной изолированной копии** намеренно меняются местами `validate/delete` target phrases; никакого экспорта/промоции | Если B выигрывает из-за обученных смысловых связей, C должен терять хотя бы соответствующие ответы |
| **D — COLD REOPEN** | Новый четвёртый `TranslationMachine()`; `B.ExportCheckpoint()` → `D.ImportCheckpoint()` → 8 тех же HELDOUT | Пригодность реально сохранённого Unit state, не metadata-only `ExportAsync` |

**Не подменять A результатами `TranslationMachine.TrainBatchAsync`.** Он меняет `AccuracyAfter` без изменения Unit. **Не подавать `HeldOut` в `TrainStructuredBatchAsync`** даже с `HeldOut=true`; оценивать отдельно после обучения обычным Flow, чтобы недвусмысленно исключить feedback/label leakage. Для A/B/C один и тот же набор входных текстов, язык, нормализация и scorer, а каждый condition получает свежую память — одинаковая исходная конфигурация и единственная интервенция.

## Минимальная реализация (один автономный harness, без переписывания проекта)

Создать в **рабочем репозитории core** небольшой **исследовательский** `tools/...` (название/CLI — предлагаемое, пока **НЕ существует**), который читает фиксированный JSON, не имеет доступа к production mutation/adoption и выводит только отчёт. Конкретные уже существующие типы и вызовы:

1. `TranslationLearningExample` — [контракт и сигнатура](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L125-L160). На **каждый TRAIN**, и только на TRAIN, построить `InputJson=JsonSerializer.Serialize(new ExecutorInput(source,"en","technical"))`; `ChosenOutputJson=JsonSerializer.Serialize(new ExecutorOutput(chosen,0.95,WordMappings:null,PhraseMappings:fixture.phraseMappings))`; `UnitId=TranslationUnitIds.ExecutorEnRu`; `FlowId=TranslationRoutingFlow.FlowId`; `SemanticSignature=new TranslationSemanticTokenizer().Tokenize(source,"en").Signature`; `SourceId="local-synthetic-microprobe-v1"`; `HeldOut=false`. `RejectedOutputJson` должен оставаться honest dev shadow-неверным примером с явно указанным дефектом, **не** выдавать его за юридически допущенную DPO-пару, ведь этот `Unit` использует chosen для map updates. Проверить source/governance и не обходить действующие guardrails при интеграции.
2. Объединить в `new TranslationLearningBatch(BatchId, TranslationMachine.DomainTag, examples, SourceIds)`; вызвать `B.TrainStructuredBatchAsync(batch)` ровно один раз и сохранить фактические `PromotedUnitIds`, count и checkpoint identity. Строго проверить `executor-en_ru` среди получателей; `AccuracyAfter` от этого API **не является основным measurement**.
3. `machine.GetOrchestratorAsync()` → `orchestrator.RunAsync(new LogicalOrchestratorRequest(RunId,TranslationRoutingFlow.ScenarioId,source,new LogicalExecutionFlags(), Metadata{source_language:"en",target_language:"ru",topic_domain:"technical"}))`. Для каждого HELDOUT вывести `FinalHumanResult`, `FlowResult.Metrics["adapter_id"]` (если доступно), verdict, confidence, source/expected/response hashes. **Expected доступен только scorer после возврата модели**; никогда не передавать его Orchestrator.
4. Сравнить **выровненные 8 case IDs** и 4 challenges; экспорт B только через `ExportCheckpoint/ImportCheckpoint` как в [TranslationMachine.cs:261–359](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L261-L359). Metadata `ExportAsync/ImportAsync` не сохраняет Unit state [строки 203–251](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L203-L251) и **не** пригоден для D.
5. Один локальный `dotnet build` затронутого harness/project и **1–2 узких smoke assertions**, никаких `dotnet test` по solution, GitHub Windows, повторных 500 пар или новых внешних моделей. Не выполнять бессрочные retries. Инвентаризировать реальный `dotnet run --project <новый harness.csproj> -- --fixture ... --output ...` **только ПОСЛЕ создания такого проекта**; не писать эту команду как уже работающую.

### GOOD / BAD / EDGE
- **GOOD:** на запросе `delete the file`, который не был TRAIN whole sentence, B использует `delete` и `the file`, выдаёт `удали файл` и показывает `adapter_id=executor-en_ru-phrase-map`; A не знает сочетания, C неправильно интерпретирует `delete`. Это лишь **ожидаемый сценарий**; фактические ответы до запуска неизвестны.
- **BAD:** B запоминает точную train-строку и проходит train eval, но все 8 HELDOUT равны A: нет доказанного композиционного обучения.
- **BAD:** объявить `accuracyAfter=0.01` на `TrainBatchAsync` свидетельством улучшения.
- **EDGE:** распознаватель EN может выдать неподдерживаемый concept/word; phrase map может не пройти threshold; тогда фиксировать `fallback_reason`, а не насильно менять пороги до PASS.
- **EDGE:** отрицание/вопрос/двойные объекты и неоднозначности: ошибочный уверенный ответ хуже честного unsupported/low coverage; `abstain` не засчитывать как correct.

## Критерии исхода (объявить ДО просмотра выводов)

**Primary:** `Q_A, Q_B, Q_C` — количество законно правильных переводов среди **одних и тех же 8 не тренировочных full-sentence комбинаций** (`0…8`). Для первого скрининга **support for learned-unit effect**: `Q_B−Q_A ≥ 3` **и** `Q_B > Q_C`, B действительно использует learned `phrase-map`/word/KNN, не меняя код; среди 4 challenges нет нового критического law violation, скрытого ростом exact-match. Порог +3/8 — **предварительный practical screen**, не p-value или доказательство общего качества. Если вывод неопределённый/едва положительный, **INCONCLUSIVE**, не докручивать samples post-hoc; расширять *заранее новым* source-family panel.

**Secondary:** `coverage`, доля fallback/abstain, violations of not/number/actor/intent, `adapter_id`, time per query, peak **process** RAM (если измерено), parity `D == B` для тех же 8 outputs. `C` — intentional incorrect-target shadow model; она **никогда** не входит в обучение/промоцию. При A=B=0 или если у B нет trained path → `NO_LOCAL_EFFECT / WRONG_CONSUMER / EXPERIMENT_INVALID` по диагностике. Если B улучшает 8 и D≠B → найден persistence seam. Если B заметно лучше A, но C не хуже A, остаётся риск template/metric leakage → `INCONCLUSIVE`.

**Самое важное разграничение:** это доказывает/опровергает причинный вклад обученной **дискретной Unit memory** в **translation user response**, **НЕ доказывает**, что q/v LoRA Student умеет переводить. Чтобы привязать результат именно к RP01, нужен следующий (отдельный, условный) шаг с **одним и тем же UserRequest/consumer**: `Student alone` vs `Student + trained Unit` при фиксированных весах Student, плюс negative Unit control и novelty/retention panel. Если обычный pipeline не умеет соединить эти пути, поставить `MISSING_ADOPTION_BRIDGE`, а не выдумывать, что ранний алгоритм уже внедрён. DPO-only vs SFT→DPO — **отдельный REC03**.

## Минимальный машинно-читаемый результат

```json
{
  "experimentId": "PD-FAST-01",
  "status": "NOT_RUN",
  "coreSha": "FILL_AT_RUN",
  "planningSha": "FILL_AT_RUN",
  "fixtureSha256": "FILL_AT_RUN",
  "trainNewExamples": 8,
  "heldoutRecombinations": 8,
  "challengeCases": 4,
  "route": "TranslationMachine.TrainStructuredBatchAsync -> GetOrchestratorAsync.RunAsync",
  "arms": {
    "A_fresh": {"correct": null, "coverage": null},
    "B_learned": {"correct": null, "coverage": null},
    "C_label_corrupted": {"correct": null, "coverage": null},
    "D_cold_reopened": {"parityWithB": null}
  },
  "criticalLawRegressions": null,
  "resultClass": "NOT_MEASURED",
  "independentReview": "NOT_RUN",
  "conclusion": "Do not infer causal improvement until the execution receipt is saved."
}
```

**Executor handoff (bounded):** Owner **RP02/RP03** проверяет affected `ExecutorUnit` и Flow; **RP01** только проверяет, отсутствует ли этот consumer в isolated Student scientific route, без изменения q/v LoRA. Изучить `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md` из [docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Единственный допустимый вывод после чтения планового файла — `PLAN_CREATED`; `QUALITY_GAIN` только при наличии **нового** executed evidence и экспертизы.
