# Рекон: core 11.DPO — как реально строится и используется DPO

## Что это и где (точные ключевые пути)

Проект `C:/Projects/llm_mvp_core/11.DPO` — это низкоуровневая **тренировочная инфраструктура** Direct Preference Optimization (DPO, прямая оптимизация предпочтений). НЕ продуктовая модель, НЕ `ILearningMachine`, НЕ DPO-authoring oracle, НЕ построитель пар. Это «движок одного шага обучения», которому пары и токены подаёт внешний код.

Исходники (всё остальное в Glob — это `obj/` и `bin/` артефакты сборки):
- `11.DPO/Models/DpoPair.cs` — DTO пары предпочтений.
- `11.DPO/Models/DpoBatch.cs` — DTO батча (тензоры ids + masks).
- `11.DPO/Models/DpoMetrics.cs` — DTO метрик одного шага.
- `11.DPO/DpoDataset.cs` — загрузчик JSONL + Fisher-Yates shuffle.
- `11.DPO/DpoTrainer.cs` — лосс/градиент/optimizer step + сохранение метрик; здесь же `record DpoVectorBatch`.
- `11.DPO/DPO.csproj` — зависит только от `00.TensorCore`, `04.TrainingLoop`, `10.LoRA` (LoRA подключён, но в `Step` не используется).

Потребители (через Grep вне 11.DPO):
- `Tests/DpoTrainerTests.cs`, `Tests/DpoDatasetTests.cs` — контрактные/поведенческие тесты.
- `Tests/CapabilityResultTranslatorAndDeeperModelTests.cs` — интеграционный one-step DPO smoke через токенайзер/модель/батч.
- Документация: `docs/projects/11-dpo.md` (подробная, совпадает с кодом), `docs/SUBPROJECT-MAP.md`, `docs/ARCHITECTURE.md`, `docs/COMPONENT-INTERFACES.md`.
- Активный план графа: `docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` (ссылается на DPO/DpoPair).
- Прямого вызова `DpoTrainer`/`new DpoBatch` из боевого рантайма (15.Orchestrator/42.ProjectWiki/43.SmartSkills и т.п.) НЕ найдено — только тесты строят `DpoBatch` и гоняют `Step`. Пайплайна «текст → токены → DpoBatch» в самом 11.DPO нет; токенизация/батчинг — ответственность внешнего кода.

## Как устроено/работает (кратко)

Модель пары — `DpoPair { Prompt, Chosen, Rejected }` (три строки, по умолчанию пустые; **нет** source/metadata/near-miss/domain/quality-label полей).

Построение пары (chosen/rejected): **в 11.DPO её не строят**. `DpoDataset.Load(path)` читает готовый JSONL (по строке на `DpoPair`, пустые строки игнорируются, null → `InvalidOperationException`), `Shuffle(seed)` отдаёт перемешанную копию (Fisher-Yates, исходник не мутируется). Кто и как формирует «хорошо/плохо» — вне этого проекта (DPO-construction живёт в GUIDE/планах DCM, не в core 11).

Потребление пары: внешний код токенизирует строки и собирает `DpoBatch { ChosenIds, RejectedIds, ChosenMask, RejectedMask }` (тензоры `[1,T]`). Этот батч идёт в `DpoTrainer`.

Лосс/тренировка (`DpoTrainer.Step`, `11.DPO/DpoTrainer.cs:31-70`):
1. `LogProbs` (строки 227-261): средний log-softmax по next-token с маской и сдвигом на 1 (padding пропускается) — отдельно для **policy** (обучаемая) и **reference** (frozen).
2. `chosenReward = Beta*(polChosen-refChosen)`, `rejectedReward` аналогично; `Beta = 0.1f` (константа).
3. `margin = chosenReward - rejectedReward`; `loss = -log(σ(margin)+1e-8)`.
4. Ручной backward: `gradMargin = σ(margin)-1`; `BackwardLogProbs` пишет градиент в logits (one-hot − softmax) только в policy; chosen со знаком `+`, rejected со знаком `−`.
5. `AdamW.Step(policy.Parameters(), lr)` (lr по умолчанию `1e-5f`), затем `policy.ZeroGrad()`. Reference НЕ обновляется.
6. `DpoMetrics { Step, Loss, ChosenReward, RejectedReward, RewardMargin, Accuracy=(margin>0?1:0) }`.
7. `SaveMetrics`: атомарная запись JSON `dpo_step_NNNNNN.json` через `.tmp` + `File.Move(overwrite)` (DNA: метрики до инкремента шага).

Варианты: `StepMany` (усреднение по списку батчей, gradient-scale `1/N`, один optimizer step), `EvaluateMany` (те же метрики БЕЗ backward/update/save), `StepVector` (взвешивание через `DpoVectorBatch{Batch,TeachingVector,Weight}`; `VectorSignal` = clamp(1 + active_slots/60, 1..2) — усиливает градиент по числу активных слотов вектора).

Что доказано тестами: формула шага, positive/non-NaN loss, бинарная accuracy, `margin = chosen-rejected`, атомарная запись метрик, и что при копировании весов reference→policy reward margin **растёт** за 40 шагов (`StepMany_WithCopiedReferenceImprovesRewardMarginOnRepeatedPreference`). Чего НЕТ: сохранения весов модели как checkpoint/manifest/locator, export/import, fresh-load, continue-training, held-out качества, CPU/RAM/dataset-hash в метриках.

## Релевантность к ILearningDynamicGraph

- **Готовый кирпич лосса/обновления.** Если узлы графа (Unit/Flow/Orchestrator) обучаются предпочтениями, `DpoTrainer.Step/StepMany/StepVector` — это и есть точка применения сигнала. Авто-построение DPO для графа должно производить именно `DpoPair`/`DpoBatch` и кормить их сюда.
- **Пробел №1 для плана — построение пары.** В core 11 НЕТ логики «как из узла/ребра графа получить chosen/rejected». Авто-DPO-граф обязан это добавить (отдельный слой), а не искать в 11.DPO.
- **Пробел №2 — provenance/teacher.** `DpoPair` не хранит источник/teacher/лицензию. Для графа критично (см. ниже Legal Teacher Gate скила): кто сгенерировал chosen — должно фиксироваться, иначе нельзя обучать.
- **Пробел №3 — per-edge/per-node lifecycle.** Нет checkpoint/manifest/locator/export-import. Для `ILearningDynamicGraph` (много узлов/рёбер, fan-out/fan-in) нужен полный asset lifecycle на каждый обучаемый узел — 11.DPO его не закрывает.
- **`StepVector`/`TeachingVector`** — потенциальный канал «teaching signal по графу» (вес обучающего сигнала на ребре), но семантическую корректность вектора он НЕ доказывает; это лишь весовка градиента.
- **Измерение.** `DpoMetrics`/`EvaluateMany` дают loss/margin/accuracy на наборе — годятся как метрика обучения узла, но не как доказательство пользовательского качества или held-out.

## Классификация по правилам скила

Применяю `trainable-architecture-decision-SKILL.md` (раздел «Decision Procedure», список allowed-levels, KNN/retrieval note) и `generated-code-review-SKILL.md` Layer 5/5A.

- `DpoTrainer`, `DpoDataset`, `DpoPair`, `DpoBatch`, `DpoMetrics`, `DpoVectorBatch` — **внутренние детерминистические/инфраструктурные детали, НЕ allowed architecture level.** Это training engine (лосс, градиент, optimizer step, IO), а не `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator/ILearningMachine/ILearningMachinePair`. По скилу «scorers/selectors/.../dictionary/counter… are not architecture patterns; они допустимы только как implementation detail внутри allowed level». DPO-движок тут — именно такой honest implementation detail тренировочного слоя.
- **Это НЕ скрытый детерминистический shortcut, маскирующийся под trainable** (то, что ловит Layer 5A / `FORBIDDEN_RUNTIME_SHORTCUT`). Реальное обучение присутствует: настоящий градиент по policy и AdamW-апдейт; lifecycle-пробелы (нет checkpoint/export/manifest) задокументированы честно в `docs/projects/11-dpo.md` и НЕ выдаются за готовую модель. Накопления счётчиков/словарей как «финальной authority» здесь нет.
- Verdict по скилу: **`OK_INTERNAL_DETAIL`** для всего набора (training infrastructure, честно помеченная как неполный продуктовый lifecycle). Для `DpoTrainer` отдельно — `OK_MODEL_COMPONENT` в смысле «настоящий тренировочный компонент», но БЕЗ права называться `ILearningMachine`/обученной моделью без asset-lifecycle.
- **Graph-Style / DynamicGraph Reconstruction Gate**, **Legal Teacher / Provenance Gate** скила к *самому* 11.DPO неприменимы (нет рёбер графа, нет teacher-полей). Но они становятся обязательными для проекта-обёртки ILearningDynamicGraph: каждое ребро должно назвать владельца authority (детерминистический хелпер vs обучаемый узел), а `DpoPair` для графа придётся расширить teacher_provenance (teacher_access_mode/hosting_provider/model_id/model_license/...) — иначе `DATASET_REBUILD_REQUIRED`.

## Пробелы / что доисследовать

- Кто в боевом рантайме (вне Tests) реально вызывает `DpoTrainer`/строит `DpoBatch`. Сейчас прямых вызовов из 14/15/16/18/25/42/43 не найдено — похоже, боевого пайплайна обучения через 11.DPO ещё нет; подтвердить отдельно при планировании графа.
- Как и где строятся пары chosen/rejected на уровне проекта (GUIDE: `docs/skills/dcm-dpo-construction-GUIDE.md`, `design-conversion-dpo-authoring-SKILL.md`, артефакты `artifacts/*dpo-pair*`). Это вне core 11, но это и есть «как строится пара» для будущего ILearningDynamicGraph — требует отдельного рекона.
- Активный план `docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` — чужой/claude-* артефакт (по правилам — read-only evidence); сверить, как он предполагает соединять граф с 11.DPO, но не редактировать.
- Где сохраняются веса policy после `Step` (checkpoint-asset) — в самом 11.DPO этого нет; найти, делает ли это вызывающий код (`04.TrainingLoop`/`25.ModelServer`).
- `StepVector`/`TeachingVector` происхождение вектора и его роль в графе (teaching signal на ребре) — требует уточнения семантики на уровне обёртки.
