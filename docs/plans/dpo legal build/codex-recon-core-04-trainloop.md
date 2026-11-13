# Codex Recon — core / 04.TrainingLoop (механика тренировочного цикла)

## Что это и где (точные ключевые пути)

`04.TrainingLoop` — цикл **pre-training** трансформера (next-token, cross-entropy)
на чистом C# поверх TensorCore. Это НЕ DPO и НЕ обучение ILearningMachine-юнитов.

Исходники (всего 4 .cs + csproj; остальное в области — это `bin/`/`obj/` артефакты):
- `C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs` — главный цикл обучения.
- `C:/Projects/llm_mvp_core/04.TrainingLoop/TrainingConfig.cs` — все гиперпараметры (record).
- `C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs` — оптимизатор AdamW.
- `C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs` — LR-планировщик (warmup + cosine).
- `C:/Projects/llm_mvp_core/04.TrainingLoop/TrainingLoop.csproj` — зависит только от
  `00.TensorCore`, `02.DatasetPipeline`, `03.TransformerModel`.

## Как устроено / работает (кратко)

Шаги цикла (`Trainer.TrainAsync`, Trainer.cs:31-77), пока `_globalStep < cfg.TotalSteps`:
1. **Накопление градиентов** (gradient accumulation, имитация большого батча):
   `cfg.GradAccumSteps` мини-батчей из `DataLoader.NextBatch()` (Trainer.cs:42-57).
2. **Forward**: `model.Forward(inputTensor)` → логиты `[B, T, V]` (Trainer.cs:51).
3. **Loss**: cross-entropy для мониторинга, делённый на GradAccumSteps
   (`CrossEntropyLoss`, Trainer.cs:52, 80-89; через `TensorOps.CrossEntropy`).
4. **Backward вручную**: градиент `dL/dlogits = softmax(logits) - one_hot`, без autograd-обёртки,
   затем `logits.Backward(...)` (`BackwardLoss`, Trainer.cs:56, 92-125).
5. **Clip**: `TensorOps.ClipGradNorm(params, cfg.GradClipNorm)` (Trainer.cs:60) — защита от взрыва градиентов.
6. **LR от планировщика**: `_scheduler.GetLr(_globalStep)` (Trainer.cs:62).
7. **Шаг оптимизатора**: `optimizer.Step(model.Parameters(), lr)` (Trainer.cs:63).
8. **Обнуление градиентов**: `model.ZeroGrad()` (Trainer.cs:64).

**Метрики**: единственная метрика — `loss` (и `lr`), печатается в Console каждые
`cfg.LogEvery` шагов (Trainer.cs:66-67). Числовых файлов метрик/JSON нет; нет accuracy,
reward margin, eval-петли, held-out. Метрика только логируется, не персистится.

**Чекпойнты**: `model.SaveCheckpoint(.../ckpt_{step:D6}.bin)` каждые `cfg.CheckpointEvery`
шагов (Trainer.cs:70-71). DNA rule 8: сохранение ДО инкремента `_globalStep` (Trainer.cs:69-73).
Каталог создаётся в начале (Trainer.cs:34). Нет загрузки/resume чекпойнта (only Save),
нет манифеста, нет export/import, нет fresh-load probe — цикл всегда стартует с `_globalStep=0`.

**AdamW** (AdamW.cs): m/v скользящие средние + bias-correction + decoupled weight decay;
состояние момента в `Dictionary<Parameter,(m,v)>` с `ReferenceEqualityComparer` (AdamW.cs:21-23) —
это in-memory состояние оптимизатора, не персистится. Замечание: ctor принимает `lr`
(AdamW.cs:25), но реально используется `lr` из аргумента `Step(params, lr)` (AdamW.cs:35);
LR полностью задаётся планировщиком, поле lr из ctor не применяется.

**CosineWithWarmup** (CosineWithWarmup.cs:23-31): линейный warmup до `warmupSteps`, затем
cosine-спад от `maxLr` к `minLr`. Чистая детерминированная функция шага.

**Конфиг** (TrainingConfig.cs): BatchSize=8, SeqLen=2048, MaxLr=3e-4, MinLr=3e-5,
WarmupSteps=100, TotalSteps=10000, GradAccumSteps=4, GradClipNorm=1.0, WeightDecay=0.1,
CheckpointEvery=500, CheckpointDir="checkpoints", LogEvery=10.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **Прямой связи нет.** Grep по `ILearningMachine|ILogicalUnit|ILogicalFlow|ILogicalOrchestrator|ILearningMachinePair|DPO` в `04.TrainingLoop/*.cs` → **0 совпадений**. Это слой обучения базовой LLM (next-token), а не авто-построение DPO для юнитов/флоу/оркестратора/графа.
- **Граф / оркестрация / edges**: отсутствуют. Один линейный while-цикл по шагам, без узлов, рёбер, fan-out/fan-in, async-графа. Под Graph-Style Reconstruction Gate скила — N/A (не граф).
- **DPO / preference**: отсутствует. Loss — это supervised cross-entropy по target-токенам, а не preference/chosen-rejected. Нет reward margin, нет pair-building. Для DPO ILearningDynamicGraph этот цикл — только **референс/прецедент шаблона обучения** (steps→loss→clip→optimizer→checkpoint), а не источник DPO-логики.
- **Измерение (metrics)**: текущий цикл логирует только loss/lr в Console. Для требований проекта к trainable-юнитам (accuracy %, exact %, reward margin, train_rows, promotionDecision, CPU/RAM/elapsed, персист в runner-report) здесь **ничего нет** — это пробел, который план ILearningDynamicGraph не сможет закрыть переиспользованием этого кода как есть.
- **Teacher / provenance (Legal Teacher Gate)**: N/A — нет teacher-выхода, нет distillation, нет внешних моделей. Обучение идёт по локальному датасету через `DatasetPipeline.DataLoader`, источник/лицензия данных здесь не фиксируются (определяются в `02.DatasetPipeline`, вне области).
- **Потенциальная польза для плана**: как образец механики «один шаг обучения» (gradient accumulation, clip, scheduler step, save-before-increment по DNA rule 8) при проектировании training-runner-а для DPO-юнитов ILearningDynamicGraph. Любое такое переиспользование должно жить ВНУТРИ allowed-level раннера/машины, а сам `Trainer` остаётся generic-LLM-pretrain (R5: общие модели/логика — в `llm_mvp_core`).

## Классификация (по trainable-architecture-decision-SKILL.md)

| Класс/файл | Заявленный паттерн | Фактический механизм | Allowed level? | Классификация | Verdict |
|---|---|---|---|---|---|
| `Trainer` (Trainer.cs) | — (паттерн не заявлен) | Императивный pre-training loop: forward/CE-loss/manual-backward/clip/optimizer-step/checkpoint | НЕТ | Внутренний детерминистический хелпер обучения (training driver) | `OK_INTERNAL_DETAIL` |
| `AdamW` (AdamW.cs) | — | Численный оптимизатор (m/v + bias-correction + weight decay) | НЕТ | Внутренняя деталь обучения (optimizer) | `OK_INTERNAL_DETAIL` |
| `CosineWithWarmup` (CosineWithWarmup.cs) | — | Детерминированная функция LR от шага | НЕТ | Внутренняя деталь обучения (scheduler) | `OK_INTERNAL_DETAIL` |
| `TrainingConfig` (TrainingConfig.cs) | — | record гиперпараметров | НЕТ | Внутренняя деталь (config) | `OK_INTERNAL_DETAIL` |

**Почему не allowed-level (со ссылкой на правило скила):**
Скил (раздел «The only allowed trainable architecture levels are…», стрк. 95-110)
перечисляет ровно 5 уровней: `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`,
`ILogicalUnit`, `ILearningMachinePair`; «Everything else is an implementation detail unless
it is wrapped by one of those levels and has the required lifecycle evidence» (стрк. 109-110).
Эти классы НЕ реализуют ни один из контрактов (Grep подтвердил 0 совпадений), не обёрнуты
в allowed-level и не несут lifecycle-evidence (нет export/import/manifest/locator/fresh-load/
continue-training). Это инфраструктура обучения базовой модели — корректный
`OK_INTERNAL_DETAIL`. Это **не** скрытый детерминистический шорткат, маскирующийся под
trainable-юнит (нет претензии на семантику/выбор/маршрутизацию; нет
scorer/selector/router/KNN/dictionary-counter под видом обучаемого решения — раздел
Decision Procedure §3, стрк. 125-134), поэтому `FORBIDDEN_RUNTIME_SHORTCUT` неприменим.
Solved-architecture deterministic-vs-learned boundary: **N/A** — это обучающий драйвер LLM,
а не runtime-решение с семантической претензией.

## Пробелы / что доисследовать

- **Checkpoint lifecycle**: реализован только `SaveCheckpoint`; формат `.bin`, загрузка/resume,
  манифест, export/import находятся в `03.TransformerModel` (`LlmModel.SaveCheckpoint`) — вне
  области; доисследовать там, если плану нужен continue-training / fresh-load для DPO-юнитов.
- **DataLoader / источник данных и лицензия**: `DataLoader.NextBatch()` и provenance датасета —
  в `02.DatasetPipeline` (вне области). Нужен для Legal Teacher/Source-license анализа в плане.
- **TensorOps**: `CrossEntropy`, `ClipGradNorm`, `Reshape`, `ZerosLike`, `Backward` —
  в `00.TensorCore` (вне области). Точная семантика autograd/backward там.
- **Метрики/eval**: текущий цикл не имеет eval-петли, held-out, accuracy/reward margin,
  персиста метрик. Для ILearningDynamicGraph training-runner-а это придётся проектировать
  заново — не доступно для переиспользования из 04.
- **Где живёт реальный DPO/обучение юнитов**: в 04 его нет. Кандидаты на рекон:
  `15.OrchestratorModel/Logical/`, `25.ModelServer`, `42.ProjectWikiDomain` (Bootstrap-юниты,
  стартовая точка DPO), `43.LearningLocalWikiData` — по CLAUDE.md. Не входит в эту под-задачу.
- **Вызывающая сторона**: кто конструирует `Trainer`/`AdamW`/`DataLoader` и запускает
  `TrainAsync` (runner/Program/тесты) — не найдено в области 04; искать в раннерах/тестах.
