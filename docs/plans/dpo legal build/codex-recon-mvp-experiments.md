# РЕКОН: Эксперименты v2 — метрики обучения (для плана ILearningDynamicGraph)

Автор: рекон-аналитик (Opus 4.8 MAX). Только evidence для плана, не разрешение на реализацию.
Вопрос: «Эксперименты v2: метрики обучения».

---

## 1. ВАЖНО: целевая папка пуста — реальный корпус в другом месте

Заданная цель **пуста**:
- `C:\Users\lubao\Documents\xiigen\llm mvp\experiments-v2\` — содержит ровно одну
  подпапку `TEACHING-20-ROWS-CURRENT`, и та **пустая** (0 файлов, 0 элементов рекурсивно).
  Имя намекает на teaching/v2, но контента нет.

Реальные «эксперименты v2 с метриками обучения» лежат **в репозитории кода**, не в xiigen:
- `C:\Projects\llm_mvp_core\experiments-v2\` — полноценная папка экспериментов
  (E01…E13, E51…E53, `INTERNAL-TRAIN-READY-V191`, `results`, `progress`,
  `PROJECT-WIKI-BASE-HELDOUT-EVAL`, `T34/T35`-gate, teaching-батчи и т.д.).

Связующее звено (как я это нашёл): соседняя папка
`C:\Users\lubao\Documents\xiigen\llm mvp\train-ready-v2\dpo-pairs-supplement-dpo-quality-v197.jsonl`
(1 файл, 14 963 байт) содержит DPO-пары, которые **прямо документируют смысл метрики**
и ссылаются `source` на `...\experiments-v2\INTERNAL-TRAIN-READY-V191\...\DpoTrainer.cs`
и на аудит `...\internal learnin 2\model-led-run-20260508-v1\evidence\run362-v196-live-dpo-quality-audit.json` (существует, 90 378 байт).

> Дисклеймер по путям: точный путь из v197 `...\runner\InternalTrainReadyDpoRunner\11.DPO\DpoTrainer.cs`
> НЕ резолвится; реальный раннер — `...\runner\InternalTrainReadyDpoRunner\Program.cs`,
> а сам `DpoTrainer` собран в сборку `DPO.dll`/`DPO.pdb` (исходник `DpoTrainer.cs` в этом
> поддереве как .cs не лежит — только бинарь). Это пробел (см. §6).

---

## 2. Что это и где (точные ключевые пути)

Целевая (пустая):
- `C:\Users\lubao\Documents\xiigen\llm mvp\experiments-v2\TEACHING-20-ROWS-CURRENT\` — пусто.

Реальный корпус метрик обучения (llm_mvp_core):
- `C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\` — основной прогон DPO.
- Раннер: `...\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\Program.cs` (13 333 байт)
  + `InternalTrainReadyDpoRunner.csproj`; сборки `DPO.dll`, `TransformerModel.dll`,
  `TensorCore.dll`, `LoRA.dll`, `TrainingLoop.dll`, `Tokenizer.dll`.
- Результаты прогонов (десятки версий v191…v244+, по дате):
  `...\INTERNAL-TRAIN-READY-V191\results\train-ready-vNNN-...-<timestamp>\`.
  Пример: `...\results\train-ready-v196-full-20260510-232330\`.

Состав одной папки прогона (это и есть «метрики обучения»):
- `internal-train-ready-dpo-summary.json` (1 311 байт) — **итоговые агрегаты прогона**.
- `internal-train-ready-dpo-summary.md` (30 423 байт) — человекочитаемый отчёт.
- `batch-summary.jsonl` (104 069 байт) — **по-батчевые метрики** (1 строка = 1 батч).
- `progress.jsonl` (75 336 байт) — поток событий (start/batch_complete/…).
- `checkpoints\dpo_step_NNNNNN.json` (245 шт, ~130 байт) — **по-шаговые метрики**.
- `checkpoints\policy_step_NNNNNN.bin` (по `--save-every`, ~1.19 МБ) — чекпойнты политики.
- `launch.json`, `process-output.log`, `process-error.log`, `dpo-pairs-normalized.jsonl` (вход).

Документация смысла метрики (DPO-пары):
- `C:\Users\lubao\Documents\xiigen\llm mvp\train-ready-v2\dpo-pairs-supplement-dpo-quality-v197.jsonl`.
- Аудит слабых батчей: `...\internal learnin 2\model-led-run-20260508-v1\evidence\run362-v196-live-dpo-quality-audit.json`.

---

## 3. Как устроено / работает (кратко)

Раннер (`Program.cs`) грузит DPO-пары (`Prompt/Chosen/Rejected`), нормализует их в
`dpo-pairs-normalized.jsonl`, строит модель `LlmModel` (ModelConfig: VocabSize=2048,
HiddenDim=64, NumLayers=1, NumHeads=4, FfnHiddenDim=128, ContextLen из аргумента),
создаёт `policy` и `reference`, и обучает через `DpoTrainer.StepMany(dpoBatches, step)`.
Батчи фиксированного размера (`--batch-size`, дефолт 16); чекпойнт каждые `--save-every` (25).

Набор метрик на каждый батч/шаг (из `trainer.StepMany(...).metrics` → `BatchSummary`):
- `Loss` — функция потерь DPO;
- `ChosenReward`, `RejectedReward` — награды политики за chosen/rejected;
- `RewardMargin` = `ChosenReward − RejectedReward` (главный сигнал предпочтения);
- `Accuracy` — **доля пар в батче, где `RewardMargin > 0`** (chosen выше rejected);
- ресурсные: `CpuPercent`, `WorkingSetDeltaMb`, `CheckpointBytesDelta/Total`, `ElapsedMs`.

Итоговый `internal-train-ready-dpo-summary.json` агрегирует: `PairCount`, `BatchSize`,
`BatchCount`, `ContextLen`, `VocabSize`, `TotalElapsedMs`, `AverageLoss`, `FinalLoss`,
`AverageAccuracy`, `FinalAccuracy`, `AverageRewardMargin`, `FinalRewardMargin`,
`CheckpointBytesTotal`, + `SourceSha256` входного датасета (провенанс).

Конкретные значения v196 (живые данные, не выдумка):
- PairCount=3916, BatchSize=16, BatchCount=245, ContextLen=96, VocabSize=2048;
- AverageLoss≈0.6873, FinalLoss≈0.6882;
- AverageAccuracy≈0.6228, FinalAccuracy≈0.5833;
- AverageRewardMargin≈0.01217, FinalRewardMargin≈0.01013;
- шаг 1: `{Loss 0.6837, ChosenReward 0.01533, RejectedReward −0.00401, RewardMargin 0.01934, Accuracy 0.75}`.

Семантика метрики (подтверждена парами v197): weighted/batch accuracy — это **живой сигнал
reward-margin**, а НЕ held-out семантическая точность и НЕ вердикт о доверии к корпусу.
Низкий батч = трудные/слабо контрастные/слабо заякоренные пары, а не «датасет отравлен».

---

## 4. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

Высокая. Это готовый рабочий шаблон того, что плану ILearningDynamicGraph нужно
для авто-построения DPO и измерения обучения узлов/рёбер:

- **DPO-механика**: формат `Prompt/Chosen/Rejected`, reward-margin loss, per-step/per-batch
  метрики, чекпойнты политики vs reference — прямой прототип «обучаемого узла»
  (ILearningUnit) и сигнала обучения для графа.
- **Метрики обучения как контракт измерения**: связка step→batch→summary (Loss, RewardMargin,
  Accuracy, ресурсы) — кандидат на стандартный «evaluation/metrics»-контракт для
  ILearningUnit/ILearningFlow/ILearningOrchestrator (что мерить до/после обучения).
- **Провенанс источника**: `SourceSha256` + `SourcePath` в summary, и поля провенанса в
  DPO-парах (`origin`, `source`, `source_status`, `tier`, `normalized_family`, `bucket`).
  Это заготовка для требуемого teacher/source-provenance плана.
- **Версионирование экспериментов**: десятки `train-ready-vNNN` с ablation-именами
  (`-skill-governance-core-ablation`, `-strict-minus-negative-batches`, `-domain-only`)
  — образец того, как граф может вести ablation/leakage-эксперименты по рёбрам.
- **Аудит качества**: `run362-v196-live-dpo-quality-audit.json` + пары v197 показывают
  цикл «метрика упала → диагностика композиции батча/якоря → переписать строки», а не
  «удалить источник». Полезно как gate-логика для DynamicGraph.

Предостережение для плана: метрики здесь — **live reward-margin**, их нельзя выдавать за
held-out семантическую точность узла графа. Для «trained-feature»-доказательства плану всё
равно нужны held-out метрики + ablation + fresh-load + continue-training (отдельно от этих).

---

## 5. Классификация (по правилам скила trainable-architecture-decision)

Объект: раннер `InternalTrainReadyDpoRunner` (`Program.cs`) + `DpoTrainer` (сборка `DPO.dll`)
+ артефакты метрик (summary/batch/step JSON).

- `DpoTrainer.StepMany(...)` — **обучающая петля (training loop) обучаемого юнита**.
  Это НЕ хелпер: она вычисляет reward-margin/loss и мутирует веса `policy` (чекпойнты
  `policy_step_*.bin`). По уровням ILearningMachine это «движок обучения» для
  капабилити уровня **ILogicalUnit/ILearningMachine** (обучаемая политика). Сами интерфейсы
  ILearning* в этом раннере НЕ реализованы напрямую — это автономный экспериментальный
  раннер поверх `TransformerModel`/`TensorCore`; в план его надо заворачивать в
  ILearningMachine (TrainBatch/Evaluate/Export/Import), а не подключать как есть.
- Раннер `Program.cs` (загрузка пар, батчинг, запись JSON/JSONL/чекпойнтов, замер CPU/RAM,
  SHA256) — **внутренний детерминистический хелпер уровня eval/runner-harness**.
  Причина (правило скила: «scorer/selector/router/… не являются паттернами архитектуры;
  это деталь реализации внутри разрешённого уровня», и правило completion-evidence о
  runner/metrics): он измеряет и оркеструет прогон, но сам не является обучаемым
  semantic-юнитом и не должен заявляться как ILogical*-уровень.
- Метрика `Accuracy = доля(RewardMargin>0)` — **детерминистический счётчик/измеритель**
  (eval-only). По правилам скила её нельзя подавать как «семантическую точность модели»
  или финальную авторитетную оценку; это live preference-margin сигнал. Пары v197 это
  явно фиксируют (rejected-варианты «считать batch accuracy финальной классификацией» помечены [WRONG]).
- DPO-пары `*.jsonl` — **обучающие данные/ассеты** (training assets), не код-уровень.
  Имеют провенанс-поля, годятся как материал для DPO графа после source/license-гейта.

Вывод классификации: trainable-ядро (обучаемая политика + DPO-loss) = кандидат на
ILearningMachine/ILogicalUnit; раннер и метрики = eval/runner-хелперы (помечать
eval-only/internal baseline); метрики обучения = измерительный контракт, не авторитет.

---

## 6. Пробелы / что доисследовать

1. **Целевая папка пуста.** `xiigen\...\experiments-v2` не содержит данных; вероятно
   контент был перемещён в `C:\Projects\llm_mvp_core\experiments-v2`. Подтвердить у Luba,
   какая папка каноническая для плана (xiigen vs llm_mvp_core).
2. **Исходник `DpoTrainer.cs` не найден как .cs** в поддереве раннера (только `DPO.dll`/`.pdb`).
   Точная формула loss/Accuracy/RewardMargin прочитана из `Program.cs` (потребитель) и из
   живых JSON; сам класс `DpoTrainer` и `metrics`-структуру надо найти в исходниках сборки
   `DPO` (искать `DpoTrainer.cs`, `StepMany`, `DpoMetrics`/`BatchMetrics` в `11.DPO`/проекте DPO).
3. **Held-out vs live.** Прочитан только `INTERNAL-TRAIN-READY-V191`. Папки
   `PROJECT-WIKI-BASE-HELDOUT-EVAL`, `results`, `progress`, `T35-LOGICAL-UNIT-REPLACEMENT-PROOF`
   не открывались — там может быть held-out/семантическая оценка и proof замены статического
   юнита обучаемым (прямо релевантно ILearningDynamicGraph). Доисследовать отдельным реконом.
4. **Teacher-provenance.** В summary есть `SourceSha256`/`SourcePath`; полей teacher (model_id,
   license, teacher_access_mode) в summary НЕТ — провенанс учителя живёт в DPO-парах/аудите.
   Для плана нужно проверить, проходит ли источник DPO source/license-гейт.
5. **Десятки версий v191…v244+** не сравнивались по метрикам (тренд accuracy/margin между
   ablation-прогонами). Если плану нужен «улучшение доказано» — нужен отдельный delta-разбор
   `internal-train-ready-dpo-summary.json` по версиям.
6. **Связь с интерфейсами ILearning*.** В этом раннере прямых реализаций ILearningUnit/Flow/
   Orchestrator/DynamicGraph нет; где они объявлены и как метрики туда привязываются —
   доисследовать в `00.Core`/`15.OrchestratorModel` (вне этой узкой под-задачи).
