# Гайд 02 — Как обучать юнит (LoRA + DPO, reward margin, continue-training, lifecycle)

Документ-гайд для ПЛАНА проекта ILearningDynamicGraph. Это часть планирования
(документация), НЕ разрешение реализовывать продукт. Все цитаты кода — реально
существующие, с `file:line`. Машинные ярлыки вторичны; объясняю человеческим
языком сначала, код потом.

Автор-роль: автор гайда (Opus 4.8 MAX). Фокус гайда: механика обучения одного
обучаемого юнита/узла — LoRA-адаптеры + DPO-лосс (reference-vs-policy, AdamW),
reward margin, continue-training с чекпойнта, и полный lifecycle
export/import/locator/manifest со split общий/адаптивный (правило R5).

---

## 1. Назначение гайда

Этот гайд отвечает на один вопрос: **как в нашем коде реально обучается один
юнит** (он же будущий узел графа ILearningUnit), и что план ILearningDynamicGraph
должен переиспользовать, а что — достроить.

Гайд нужен, потому что в репозитории есть ДВА разных «обучения», и их легко
перепутать:

- **Настоящий тренировочный путь** (работает): `18.CapabilityTraining` →
  `LogicalDpoTrainingUnit` → `LoraApplicator.Apply` + `DpoTrainer.StepVector`.
  Реальный градиент, реальный AdamW-шаг, реальные метрики `dpo_step_*.json`.
- **Заглушка-обёртка** (НЕ обучает): `14.ContinuousLearning` → `IncrementalTrainer`.
  Возвращает путь к файлу адаптера и больше ничего не делает.

Если план перепутает эти два, он построит «авто-DPO» поверх заглушки и получит
ноль обучения. Поэтому гайд явно отделяет рабочую механику от пробелов.

Границы гайда:
- В фокусе — механика обучения **одного** юнита/узла: LoRA, DPO-лосс, reward
  margin, continue-training, lifecycle (export/import/manifest/locator), split
  общий/адаптивный (R5).
- НЕ в фокусе — как авто-строить chosen/rejected пары (это отдельный гайд
  построения пар) и графовая топология рёбер (отдельный архитектурный раздел
  плана). Здесь они упоминаются только как точки стыковки и пробелы.

---

## 2. Концепт простыми словами (аналогия, без кода)

### 2.1. Что такое «обучить юнит» в нашем смысле

Представь модель как опытного переводчика, который уже неплохо говорит на языке
(это **базовая модель**, frozen, замороженная). Мы НЕ учим его языку заново.
Мы учим его **предпочтению**: «вот этот ответ лучше, чем вот тот».

Это и есть DPO (Direct Preference Optimization, прямая оптимизация
предпочтений). Мы показываем пары:
- **chosen** — правильный ответ;
- **rejected** — правдоподобно-неправильный ответ с пометкой, ЧТО именно в нём
  плохо (маркер `[WRONG: ...]`).

Модель сдвигает свои веса так, чтобы chosen стал для неё чуть вероятнее, чем
rejected. Не «зазубрить chosen», а «предпочитать chosen над похожим плохим».

### 2.2. Аналогия с двумя экзаменаторами (reference vs policy)

DPO работает не «в абсолюте», а **относительно эталона**. Есть два экземпляра
одной модели:
- **reference** (эталон) — заморожен, не меняется. Это «как модель думала
  ДО обучения».
- **policy** (политика) — обучаемая копия, её веса двигаются.

Сравниваем: «насколько policy стала предпочитать chosen над rejected СИЛЬНЕЕ, чем
это делал reference». Если policy просто повторяет reference — обучения нет
(margin ≈ 0). Если policy реально сдвинулась в сторону chosen — margin растёт.

Это защищает от деградации: модель не уезжает от исходных знаний слишком далеко,
она лишь донастраивает предпочтение.

### 2.3. Аналогия с LoRA: стикеры поверх книги

Базовая модель — это толстая книга, которую дорого перепечатывать. LoRA
(Low-Rank Adaptation, низкоранговая адаптация) — это тонкие **стикеры-поправки**
поверх некоторых страниц (слоёв внимания). Книгу не трогаем (frozen), а учим
только стикеры. Стикеров мало (низкий ранг), поэтому обучение дешёвое и его
легко сохранить/переслать отдельно от книги.

Важная деталь нашей реализации (это пробел, см. §6–7): стикеры **создаются и
возвращаются «сбоку»**, но в текущем коде они НЕ вклеиваются автоматически
обратно в книгу при чтении (LoRA-путь не встроен в `LlmModel.Forward`).

### 2.4. Reward margin: «насколько уверенно лучше»

Reward margin — это число «насколько модель предпочла chosen над rejected».
- `margin > 0` → модель права (chosen лучше) → бинарная accuracy = 1.
- `margin` растёт от шага к шагу → модель действительно учится.

ОСТОРОЖНО (главная ловушка проекта): в DPO-движке margin **вычисляется** из
log-вероятностей. Но в `TranslationMachine` поле `RewardMargin` сейчас
**захардкожено** (0.03, если у всех пар есть маркер `[WRONG:`, иначе 0.0). Это
«метка наличия отрицательного примера», а НЕ обучающий сигнал. Для узла графа
такой margin нельзя предъявлять как доказательство обучения.

### 2.5. Continue-training: дообучение, а не обучение с нуля

«Завести новый узел/домен с нуля» по продукту = **скопировать веса базовой
модели + дообучить на доменном DPO-шарде**, а не учить случайную модель на пустом
месте. Технически это «загрузить родительский чекпойнт, затем продолжить шаги
обучения». В коде такой путь есть (загрузка `ParentPolicyCheckpointPath`), но с
нюансом порядка операций (см. §3.4).

### 2.6. Lifecycle: где модель живёт после обучения

Обучили — мало. Нужно: сохранить чекпойнт на диск, описать манифестом
(гиперпараметры, пути, провенанс), опубликовать через ModelServer, сделать
находимым через locator, уметь импортировать и **продолжить** обучение. И
разделить (R5): **общая** модель — в `llm_mvp_core` (saved/import/export),
**адаптивная** пользовательская — на стороне пользователя.

---

## 3. Как это сейчас реализовано в нашем коде (file:line, что РАБОТАЕТ / что ЗАГЛУШКА)

### 3.1. DPO-движок одного шага — РАБОТАЕТ (`11.DPO`)

`DpoTrainer` — это «движок одного шага обучения»: лосс, ручной backward, шаг
оптимизатора, сохранение метрик. Пары/токены ему подаёт внешний код.

Формула лосса (документирована прямо в шапке файла):
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:9` —
  `Loss = -E[log σ(β · (log π(y_w|x) - log π(y_l|x) - log π_ref(y_w|x) + log π_ref(y_l|x)))]`

Что реально считается в `Step` (`DpoTrainer.cs:31-70`):
- `Beta = 0.1f` — коэффициент KL-штрафа, константа: `DpoTrainer.cs:13`.
- reference log-probs (frozen) и policy log-probs (обучаемая):
  `DpoTrainer.cs:34-39`.
- `chosenReward = Beta*(polChosen-refChosen)`, аналогично rejected:
  `DpoTrainer.cs:42-43`.
- `margin = chosenReward - rejectedReward`: `DpoTrainer.cs:44`.
- `loss = -log(σ(margin)+1e-8)`: `DpoTrainer.cs:47`.
- ручной backward: `gradMargin = σ(margin) - 1`: `DpoTrainer.cs:50`; запись
  градиента в logits только для policy, chosen со знаком `+`, rejected со знаком
  `-`: `DpoTrainer.cs:51-52`.
- шаг оптимизатора и обнуление градиента: `DpoTrainer.cs:54-55`.
- метрики, включая **вычисленный** `RewardMargin = margin` и бинарную
  `Accuracy = margin>0?1:0`: `DpoTrainer.cs:57-65`.

AdamW (оптимизатор) создаётся с `weightDecay: 0.01f` (`DpoTrainer.cs:25`),
`learningRate` по умолчанию `1e-5f` (`DpoTrainer.cs:21`). **Reference не
обновляется** — обновляется только `policy.Parameters()` (`DpoTrainer.cs:54`).

LogProbs (средний log-softmax по next-token, сдвиг на 1, padding пропускается):
`DpoTrainer.cs:227-261`. Ручной backward в logits (one-hot − softmax):
`DpoTrainer.cs:264-309`.

Варианты шага:
- `StepMany` — усреднение по списку батчей, `gradientScale = 1/N`, один
  optimizer step: `DpoTrainer.cs:72-125`.
- `EvaluateMany` — те же метрики БЕЗ backward/update/save (чистая оценка):
  `DpoTrainer.cs:127-167`.
- `StepVector` — взвешивание градиента через `DpoVectorBatch{Batch, TeachingVector,
  Weight}`: `DpoTrainer.cs:169-224`; запись определения record:
  `DpoTrainer.cs:331`. `VectorSignal = clamp(1 + active_slots/60, 1..2)` —
  усиливает сигнал по числу активных слотов teaching-вектора:
  `DpoTrainer.cs:313-320`.

Сохранение метрик (атомарно, tmp→move; DNA-правило 8 — метрики ДО инкремента
шага): `DpoTrainer.cs:322-328`.

Модель пары — три строки, БЕЗ source/provenance/teacher/quality полей:
- `C:/Projects/llm_mvp_core/11.DPO/Models/DpoPair.cs:3-8` (`Prompt/Chosen/Rejected`).

Метрики одного шага: `C:/Projects/llm_mvp_core/11.DPO/Models/DpoMetrics.cs:3-11`
(`Step/Loss/ChosenReward/RejectedReward/RewardMargin/Accuracy`).

ЧЕГО НЕТ в 11.DPO (честные пробелы движка): сохранения весов модели как
checkpoint, манифеста/locator, export/import, held-out-качества, и CPU/RAM/
dataset-hash в метриках. Это training engine, а не lifecycle.

### 3.2. LoRA-адаптеры — РАБОТАЮТ как математика, но НЕ вклеены в Forward (`10.LoRA`)

`LoraApplicator.Apply` (`C:/Projects/llm_mvp_core/10.LoRA/LoraApplicator.cs:12-40`):
1. замораживает ВСЕ параметры базовой модели: `LoraApplicator.cs:15-16`;
2. по `model.Blocks` берёт `block.Attention`, через reflection находит поля типа
   `Linear`: `LoraApplicator.cs:20-26`, `LoraApplicator.cs:43-55`;
3. нормализует имена (убирает `_`, lowercase), сопоставляет с `TargetModules` как
   substring: `LoraApplicator.cs:30-32`;
4. создаёт `LoraLinear` для совпавших и возвращает их списком:
   `LoraApplicator.cs:35,39`.

`LoraLinear` (адаптер поверх замороженного `Linear`):
- A — Kaiming `[Rank, inDim]`, B — нули `[outDim, Rank]` (старт = нулевая
  поправка): `C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:28-33`.
- база заморожена (`RequiresGrad=false`), `lora_a`/`lora_b` регистрируются как
  trainable: `LoraLinear.cs:36-43`.
- forward `base(x) + scaling*((dropout(x)@A^T)@B^T)`: `LoraLinear.cs:47-59`.

Конфиг по умолчанию: `Rank=8, Alpha=16, Dropout=0.05, TargetModules=[q_proj,
v_proj], Scaling=Alpha/Rank`:
`C:/Projects/llm_mvp_core/10.LoRA/LoraConfig.cs:5-12`.

**ЗАГЛУШКА/ПРОБЕЛ (подтверждено кодом):** `Apply` НЕ делает `field.SetValue` и НЕ
встраивает `LoraLinear.Forward` в `LlmModel.Forward`. Адаптеры возвращаются
«сбоку»; их должен подцепить вызывающий training-loop. Также нет собственного
checkpoint/export/import LoRA-ассета и нет inference-ветки dropout (всегда
`training:true`, `LoraLinear.cs:52`). Это означает: даже после обучения `lora_a/
lora_b` базовый `LlmModel.Forward` их по умолчанию не использует — это
архитектурный пробел для плана (см. §7).

### 3.3. Реальный обучающий юнит — РАБОТАЕТ (`18.CapabilityTraining`)

Единственный реально работающий цикл «pair → LoRA → DPO → метрики» — это
`LogicalDpoTrainingUnit : ILogicalUnit<...>`:
`C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:308`.

`ExecuteAsync` (`LlmIngredientLogicalUnits.cs:320-415`):
- валидация (модели, токенайзер, минимум одна preference): `:329-336`;
- применяет LoRA: `LoraApplicator.Apply(... LoraConfig ?? new LoraConfig())`:
  `:339`;
- (опц.) грузит родительский чекпойнт для continue-training:
  `request.Input.PolicyModel.LoadCheckpoint(ParentPolicyCheckpointPath)`:
  `:340-350`;
- собирает `DpoVectorBatch` на каждую пару (teaching-вектор из preference или
  `TrainingVectorBuilder.Build(prompt)`): `:356-361`;
- крутит цикл `trainer.StepVector(batches, step)` до `MaxStepCount` с ранней
  остановкой `ConvergenceMonitor`: `:367-376`;
- собирает метрики-файлы `dpo_step_*.json` и возвращает
  `LogicalDpoTrainingResult` (LastLoss/LastAccuracy/LoraAdapterCount/
  TrainingTimeMs/MetricFilePaths): `:378-401`.

Ingredient-список честно называет компоненты: `10.LoRA:LoraApplicator`,
`11.DPO:DpoTrainer`, `11.DPO:DpoTrainer.StepVector`: `:396-399`.

Где этот юнит вызывается: оркестратор обучения
`DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync`
(`C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs`),
LoRA-фабрика `LoraConfig{Rank=2,Alpha=4}`:
`DefaultLogicalCapabilityTrainer.cs:858` (рекон-подтверждено).

### 3.4. ВАЖНЫЙ НЮАНС ПОРЯДКА (баг-кандидат для плана)

В `LlmIngredientLogicalUnits.cs` сначала идёт `LoraApplicator.Apply` (`:339`),
которая **замораживает все параметры** базовой модели, и только ПОТОМ грузится
родительский чекпойнт `LoadCheckpoint` (`:344`). То есть continue-training
загружает веса в уже замороженную базу. Для adapter-only обучения это может быть
осознанно (учим только `lora_a/lora_b`), но это надо явно зафиксировать в плане:
загружается ли вместе с базой родительский LoRA-адаптер? Сейчас LoRA-адаптер
отдельно не сохраняется/не грузится (см. §3.2 пробел), поэтому «continue-training
адаптера» фактически не замкнут. Это пробел плана, не доказанная возможность.

### 3.5. IncrementalTrainer (14) — ЗАГЛУШКА (НЕ обучает)

`C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:17`
объявляет `IncrementalTrainer : BaseService, IIncrementalTrainer` с «контрактом
делегирования» (LoRA → cross-entropy → DpoTrainer → save adapter) в комментарии
`:6-16`. НО метод `TrainAsync` (`:38-53`):
- проверяет домен/непустые примеры: `:43-46`;
- затем просто строит путь `Path.Combine(_adapterDir, $"{domain}.lora")`,
  `await Task.CompletedTask`, и возвращает этот путь: `:50-52`;
- комментарий честно признаёт: «LoraApplicator.Apply and DpoTrainer.Step are
  wired up in 16.AutonomousLearning ... Here we record the intended adapter
  path»: `:48-49`.

ВЫВОД: `IncrementalTrainer.TrainAsync` НЕ применяет LoRA, НЕ вызывает DpoTrainer,
НЕ пишет веса. Это заглушка пути. Любой план, который строит авто-DPO поверх
`IncrementalTrainer`, обучения не получит. По Layer 5 generated-code-review это
`FAIL` как «no-op training loop» (`generated-code-review-SKILL.md:146`).

### 3.6. Lifecycle / ModelServer — ЧАСТИЧНО (`25.ModelServer`)

`LocalModelRepository` — единственная реализация `IModelRepository`
(`C:/Projects/llm_mvp_core/25.ModelServer/Local/LocalModelRepository.cs:8`):
- `LoadWeightsAsync` открывает локальный файл `localFallbackPath`
  (`File.OpenRead`), иначе `MODEL_WEIGHTS_NOT_FOUND`: `:25-29`. Параметр
  `serverBaseUrl` принимается, но для удалённой загрузки НЕ используется (нет
  HTTP): `:10-14,28`.
- `UploadAsync` проверяет существование чекпойнта (`POLICY_CHECKPOINT_NOT_FOUND`):
  `:46-47`; считает SHA-256: `:57,80-85`; конвертирует
  `GeneratedCapabilityCheckpointManifest` → `RemoteUnitManifest` с endpoint
  `/models/{unitId}`: `:49-75`. Реальной сетевой загрузки нет — просто
  заполненный `RemoteUnitManifest`.

ЗАГЛУШКА/ПРОБЕЛ: `IModelServer` (`PublishAsync(manifest, Stream weights)` /
`OpenWeightsAsync`) объявлен, но реализаций в репозитории НЕТ (рекон-25). То есть
реальной передачи весов потоком по сети нет; import весов реально только из
локального файла. `IRemoteUnitLocator` реализован только in-memory
(`InMemoryRemoteUnitLocator`, словарь по `unitId`) — рекон-25.

Манифест/чекпойнт-контракт `GeneratedCapabilityCheckpointManifest` определён в
`C:/Projects/llm_mvp_core/18.CapabilityTraining/TeachingRegistrySnapshot.cs`
(continue-training поля `ParentUnitId/ParentPolicyCheckpointPath/
TrainingStepCount`, provenance `TrainingBranch/TrainingRunId/SourceDatasetSha256`
— рекон-25/18). НЕТ полей teacher-провенанса (см. §6 Legal Teacher Gate).

### 3.7. Эталон «правильного» обученного юнита — `27.TranslationDomain`

Полный пример уровня `ILearningMachine` с настоящим disk-lifecycle:
- `TranslationMachine : ILearningMachine<TranslationDomain>` — рекон-27.
- `ExportAsync` — только МЕТАДАННЫЕ, не персист состояния (честно помечено):
  `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:196-206`.
- `SaveCheckpointAsync` — реальный персист всего состояния атомарно (tmp→move):
  `TranslationMachine.cs:384-388`.
- `LoadCheckpointAsync` — единственный валидный путь после рестарта (без replay):
  `TranslationMachine.cs:444-480`.
- `ImportCheckpoint` — валидация и восстановление всех юнитов:
  `TranslationMachine.cs:298-360`.

Базовая модель умеет персист весов: `LlmModel.SaveCheckpoint(path)`
(`C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:61`) и
`LoadCheckpoint(path) => LoadWeights(path)` (`LlmModel.cs:68`).

**Главная ловушка эталона (reward margin захардкожен):**
- `TranslationMachine.cs:70` — `RewardMargin: 0.03` (фиксировано в `TrainBatchAsync`).
- `TranslationMachine.cs:123-126` — `rewardMargin = (все rejected содержат
  "[WRONG:") ? 0.03 : 0.0`. Это presence-флаг наличия отрицательного примера, НЕ
  вычисленная награда модели.
- `TranslationMachine.cs:187` — `RewardMargin: history>0 ? 0.03 : 0.0` в
  `EvaluateAsync`.

Контраст: настоящий вычисленный margin живёт в `DpoTrainer.cs:44,63`. План графа
обязан брать reward margin из DPO-движка, а не из захардкоженной 0.03.

DPO-формат пары в TranslationDomain уже богатый (`chosen/rejected/[WRONG:]/
SourceId/HeldOut`): `TranslationUnitContracts.cs:124-144` — это шаблон для
provenance, которого нет в плоском `DpoPair` (§3.1).

### 3.8. Контракты иерархии (куда вешать обучение)

`C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs`:
- иерархия и правило A-LM-02 (`Train*` только на `ILearningMachine`):
  `:7-8,193-234`.
- `ILearningMachine<TDomain>` с `TrainBatchAsync/EvaluateAsync/ExportAsync/
  ImportAsync/GetOrchestratorAsync`: `:208-234`.
- `LearningBatchResult` с ОБЯЗАТЕЛЬНЫМИ числовыми полями `AccuracyBefore/
  AccuracyAfter/RewardMargin/CpuMs/PeakRamMb/ElapsedMs`: `:43-62` (комментарий
  «no placeholder or UNKNOWN values in production»: `:42`).
- `ILearningMachinePair<TPersonal,TGlobal>` (R5 split, anonymised sync A-LM-08):
  `:245-261`.

---

## 4. Пошаговый рецепт/алгоритм: как обучить один юнит

Ниже — порядок, который УЖЕ работает в `LogicalDpoTrainingUnit` (§3.3), плюс
точки, которые план должен достроить (помечены ПРОБЕЛ).

Шаг 0. **Решить уровень иерархии.** Юнит-узел = `ILogicalUnit`; обучение вешается
не на сам инференс-юнит, а на `ILearningMachine`/тренер (A-LM-02,
`LearningMachineContracts.cs:8`). Для split common/adaptive — `ILearningMachinePair`.

Шаг 1. **Подготовить пары.** Нужен список preference: `Prompt/Chosen/Rejected`
(`DpoPair.cs:3-8`) или богатый `TranslationLearningExample`
(`TranslationUnitContracts.cs:124`) с `[WRONG:]`, `SourceId`, `HeldOut`.
Построение пар — вне этого гайда (ПРОБЕЛ: авто-построение для графа, см. §7).

Шаг 2. **Две копии модели.** policy (обучаемая) + reference (frozen). DPO считает
сдвиг policy относительно reference (`DpoTrainer.cs:34-44`).

Шаг 3. **Применить LoRA к policy.** `LoraApplicator.Apply(policyModel, loraConfig)`
(`LlmIngredientLogicalUnits.cs:339`): замораживает базу, отдаёт `lora_a/lora_b` как
trainable. ПРОБЕЛ: адаптеры не вклеены в `Forward` (§3.2) — план должен решить
wiring (`SetValue` или адаптер-aware Forward).

Шаг 4. **Continue-training (если есть родитель).** Загрузить родительский
чекпойнт `LoadCheckpoint(ParentPolicyCheckpointPath)`
(`LlmIngredientLogicalUnits.cs:344`). ВНИМАНИЕ к порядку (§3.4): сейчас грузится
ПОСЛЕ заморозки. Это «копия базы + дообучение», а не обучение с нуля.

Шаг 5. **Токенизировать пары в батчи.** Внешний код строит `DpoBatch{ChosenIds,
RejectedIds, ChosenMask, RejectedMask}` (тензоры `[1,T]`), оборачивает в
`DpoVectorBatch{Batch, TeachingVector, Weight}` (`LlmIngredientLogicalUnits.cs:356-361`).

Шаг 6. **Крутить шаги DPO.** Цикл `trainer.StepVector(batches, step)` до
`MaxStepCount`, ранняя остановка по `ConvergenceMonitor` (порог 0.002, окно)
(`LlmIngredientLogicalUnits.cs:367-376`). На каждом шаге считается реальный
`margin`, `loss`, `Accuracy=(margin>0)` и сохраняется `dpo_step_*.json`
(`DpoTrainer.cs:212-223,322-328`).

Шаг 7. **Измерить на held-out.** Использовать `EvaluateMany` (БЕЗ
backward/update, `DpoTrainer.cs:127-167`) или
`ILearningMachine.EvaluateAsync` с настоящими числами accuracy/reward-margin.
ПРОБЕЛ: брать reward margin из движка, а не хардкодить 0.03
(`TranslationMachine.cs:70,123-126`). Контаминация train↔held-out должна
блокироваться (рекон-27: `TRAIN_HELDOUT_OVERLAP`).

Шаг 8. **Промоушен по гейту дельты.** Повышать узел, только если
`AccuracyAfter > AccuracyBefore` (прецедент `16.AutonomousLearning` шаги 6/7,
рекон-16; строгий перевод-гейт `+0.02`/reward `>=0.025`/product `>=0.95`,
рекон-27). Гейт `ACCURACY_NOT_IMPROVED` — кандидат на anti-shortcut.

Шаг 9. **Сохранить чекпойнт + манифест.** `SaveCheckpointAsync`
(`TranslationMachine.cs:384`) или `LlmModel.SaveCheckpoint`
(`LlmModel.cs:61`); собрать `GeneratedCapabilityCheckpointManifest`
(`TeachingRegistrySnapshot.cs`, рекон-25/18) с SHA-256 и provenance.

Шаг 10. **Опубликовать + сделать находимым.** `IModelRepository.UploadAsync`
(`LocalModelRepository.cs:32-77`) → `IRemoteUnitLocator.RegisterAsync` (CLAUDE.md
«Trainable Unit pattern»). ПРОБЕЛ: `IModelServer.PublishAsync(Stream weights)` не
реализован (§3.6) — сетевой export весов не замкнут.

Шаг 11. **Fresh-load + continue-training проба.** Перезапуск процесса →
`LoadCheckpointAsync` (`TranslationMachine.cs:444`) → продолжить обучение от
загруженного состояния. ПРОБЕЛ: held-out/ablation/fresh-load как отдельная
evidence в 18 не реализованы (рекон-18).

Шаг 12. **Split R5.** Общая модель + логика + общие сеты — в `llm_mvp_core`
(saved/import/export). Адаптивная пользовательская модель + пользовательские сеты
— на стороне пользователя. Контракт: `ILearningMachinePair`
(`LearningMachineContracts.cs:245-261`). ПРОБЕЛ: на уровне 25 нет common-vs-user
флага/двух ног (рекон-25).

---

## 5. Конкретные примеры с реальными значениями

### 5.1. DPO-пара chosen/rejected/[WRONG:] (реальная, из репо)

Реальная строка train-ready JSONL (рекон-27, `ru-en-product-b001-001`):

```json
{
  "id": "ru-en-product-b001-001",
  "source": "...",
  "direction": "ru-en",
  "pair_family": "...",
  "prompt": "...",
  "a_side": "Фразовая карта: Привет; Я новый тут; чем занимаешься.",
  "chosen": "Phrase map: Hey!; ... 😊",
  "rejected": "... [WRONG: product_phrase_boundary_lost - drops casual chat tone or emoji ...]",
  "validation_outcome": "train_ready"
}
```

Структура `[WRONG: <weakness - gap - consequence>]` обязательна: rejected несёт
ОДНУ проверяемую ошибку, а не случайный мусор. Это учит модель различать тонкую
разницу, а не «chosen ≠ длинная строка».

Минимальный плоский вариант под `DpoPair` (`DpoPair.cs:3-8`):

```json
{"Prompt": "Переведи: Привет, как дела?", "Chosen": "Hi, how are you?", "Rejected": "Hello, what are you doing? [WRONG: greeting_register_shift - turns casual greeting into a different question - changes meaning]"}
```

### 5.2. Как пара превращается в число (reward margin) — числовой прогон

Псевдо-значения, чтобы показать механику `DpoTrainer.Step` (`DpoTrainer.cs:31-65`):

```text
Beta = 0.1
polChosenLogP   = -2.0   refChosenLogP   = -2.3
polRejectedLogP = -3.0   refRejectedLogP = -2.5

chosenReward   = 0.1 * (-2.0 - (-2.3)) = 0.1 * 0.3  =  0.03
rejectedReward = 0.1 * (-3.0 - (-2.5)) = 0.1 * -0.5 = -0.05
margin         = 0.03 - (-0.05) = 0.08          → RewardMargin = 0.08
loss           = -log(σ(0.08)) ≈ -log(0.52) ≈ 0.654
Accuracy       = (0.08 > 0) ? 1 : 0 = 1
gradMargin     = σ(0.08) - 1 ≈ 0.52 - 1 = -0.48  → толкает policy усиливать chosen
```

Главное: `RewardMargin` здесь — РЕЗУЛЬТАТ вычисления, и при повторных шагах он
растёт (доказано тестом `StepMany_WithCopiedReferenceImprovesRewardMarginOnRepeatedPreference`,
рекон-11).

### 5.3. Анти-пример (как НЕ надо) — захардкоженный margin

```csharp
// TranslationMachine.cs:123-126 — НЕ обучающий сигнал, а presence-флаг:
var rewardMargin = batch.Examples.All(example =>
    example.RejectedOutputJson.Contains("[WRONG:", StringComparison.Ordinal))
        ? 0.03
        : 0.0;
```

`сейчас`: margin = 0.03 просто потому, что во всех парах есть маркер `[WRONG:`.
`почему плохо`: это не измеряет, стала ли модель предпочитать chosen; число
постоянно и не растёт от обучения.
`должно стать`: брать `RewardMargin` из `DpoMetrics.RewardMargin`
(`DpoMetrics.cs:9`), вычисленного `DpoTrainer` (`DpoTrainer.cs:44/63`).

### 5.4. LoRA-конфиг (реальные значения)

```text
Default (10.LoRA):                Rank=8,  Alpha=16, Dropout=0.05, scaling=2.0  (LoraConfig.cs:5-12)
IncrementalTrainer.DefaultConfig: Rank=8,  Alpha=16                            (IncrementalTrainer.cs:20-26)  ← но Trainer заглушка
DefaultLogicalCapabilityTrainer:  Rank=2,  Alpha=4                             (DefaultLogicalCapabilityTrainer.cs:858)
LogicalDpoTrainingUnit:           LoraConfig ?? new LoraConfig() (=Rank=8)     (LlmIngredientLogicalUnits.cs:339)
```

Несогласованность ранга (2 vs 8) между тренерами — кандидат на унификацию в плане.

---

## 6. Гейты и правила (что применимо к теме обучения)

### 6.1. Запрет скрытого детерминизма (R4 / Layer 5A)

Обучение должно быть НАСТОЯЩИМ, а не словарём/счётчиком/lookup под видом
trainable. Проверка по `generated-code-review-SKILL.md:138-220` (Layer 5/5A) и
`trainable-architecture-decision-SKILL.md:119-209`.

FAIL-признаки на пути обучения (`generated-code-review-SKILL.md:142-154`):
- статус `not_ready_*`/`not_trained_*`;
- метод возвращает фикс. дескриптор вместо вычисления из обученного состояния;
- пустой/no-op training loop (TrainBatch не меняет состояние) — это ровно
  `IncrementalTrainer.TrainAsync` (§3.5);
- `NotImplementedException`/`throw`/`TODO` на пути обучения/инференса;
- инференс = exact-source/exact-prompt lookup под видом обобщения.

Числовая evidence обязательна (`generated-code-review-SKILL.md:149-154`):
accuracy/exact/loss/reward-margin на held-out (числа, не UNKNOWN); `cpu_ms`,
`ram_mb`, row counts; train vs test — разные сеты (без replay-leakage).

Разрешённые уровни (только пять, `trainable-architecture-decision-SKILL.md:95-101`):
`ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`,
`ILearningMachinePair`. LoRA/DPO-движок/KNN — это компоненты ВНУТРИ уровня
(`OK_INTERNAL_DETAIL`/`OK_MODEL_COMPONENT`), а не самостоятельная архитектура
(`trainable-architecture-decision-SKILL.md:109-117`).

### 6.2. Анти-leakage (что НЕ должно попадать в обучающий сигнал)

- НЕ учить по candidate-position/индексу кандидата (order-leakage; пример
  `_candidatePositionWeights` → `CandidateOrderLeakageGuardUnit`,
  `trainable-architecture-decision-SKILL.md:378-380,422-440`).
- НЕ учить по `SourceId`/пути/провенансу (source/path leakage — Minimum Breadth
  Challenge, `trainable-architecture-decision-SKILL.md:165-176`). `SourceId`
  (`TranslationUnitContracts.cs:142`) — для контаминации/атрибуции, НЕ фича.
- НЕ допускать train↔held-out пересечения (рекон-27: `TRAIN_HELDOUT_OVERLAP`).
- Ablation/no-shortcut: доказать, что выбор не сводится к детерминированной
  формуле/мягкому полу confidence (рекон-18: `confidence = 0.50 + fused*0.49`
  даёт пол 0.50 даже при нулевом сигнале — проверить).

### 6.3. Легальный teacher-гейт + provenance (для авто-DPO)

Если chosen/rejected генерирует teacher-модель — обязателен провенанс
(`trainable-architecture-decision-SKILL.md:68-93`):

```text
teacher_access_mode, hosting_provider, model_id, model_license,
provider_terms_checked, teacher_output_used_for_training, closed_proprietary_model
```

Жёсткое правило: Claude/OpenAI/Gemini/Gemini Grounding **запрещены** как teacher
для distillation/DPO. Если `closed_proprietary_model=true` И
`teacher_output_used_for_training=true` → вердикт всегда
`DATASET_REBUILD_REQUIRED` (`trainable-architecture-decision-SKILL.md:87-93`).
Закрытые модели можно использовать только как non-training judge/eval, не как
chosen/rejected.

Источники общего обучения: предпочесть MIT/Apache-2.0 с проверенным
license-файлом/URL (`trainable-architecture-decision-SKILL.md:359-374`). Tatoeba
= CC-BY (не MIT/Apache) — проверить пригодность для common-training (рекон-27).

ПРОБЕЛ: ни `DpoPair`, ни манифест `GeneratedCapabilityCheckpointManifest` не
несут teacher-полей (§3.1, §3.6). План обязан добавить их, если будет teacher.

### 6.4. Гейты измерения и две честные PASS-категории

- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — контракт есть, честно сообщает
  not_ready, не прячет детерминизм, доказан тестами честности.
- `PASS_AS_TRAINED_FEATURE` — checkpoint + held-out числа + ablation +
  fresh-load + continue-training + e2e через реальный хост.
(`generated-code-review-SKILL.md:138-156`, CLAUDE.md «Untrained honesty vs
trained readiness pattern».)

Выдавать scaffold за trained — самый опасный класс ошибки (masquerade). Текущая
полнота 18/27 по held-out/ablation/fresh-load НЕ достаточна для
`PASS_AS_TRAINED_FEATURE` (рекон-18/27).

### 6.5. Graph-Style Reconstruction Gate (на каждое ребро графа)

Для ILearningDynamicGraph каждое ребро обязано назвать владельца финальной
власти (детерминистический хелпер / Unit / Flow / Orchestrator / Machine);
скрытые детерминистические shortcut'ы на ребре =
`FORBIDDEN_RUNTIME_SHORTCUT` без явной пометки eval-only/baseline + ablation
(`trainable-architecture-decision-SKILL.md:41-65`).

---

## 7. Пробелы / что нужно построить в плане

Сводка пробелов обучения (каждый — work-order-кандидат для плана):

1. **IncrementalTrainer (14) — заглушка.** `TrainAsync` не обучает
   (`IncrementalTrainer.cs:38-53`). План: либо реализовать делегирование в
   LoRA+DpoTrainer, либо явно вывести из пути обучения графа и использовать
   `LogicalDpoTrainingUnit` (§3.3) как канон.

2. **LoRA Apply без `SetValue` / без adapter-aware Forward.**
   `LoraApplicator.Apply` возвращает адаптеры «сбоку»; `LlmModel.Forward` их не
   использует (`LoraApplicator.cs:12-40`, `LoraLinear.cs:47-59`). План: решить
   wiring адаптеров в forward + inference-режим dropout (сейчас всегда
   `training:true`, `LoraLinear.cs:52`).

3. **RewardMargin не вычисляется в TranslationMachine.** Захардкожен 0.03/0.0
   (`TranslationMachine.cs:70,123-126,187`). План: тянуть `RewardMargin` из
   `DpoMetrics.RewardMargin` (`DpoMetrics.cs:9`), считать на held-out, не
   хардкодить.

4. **IModelServer не реализован.** `PublishAsync(Stream weights)`/
   `OpenWeightsAsync` без реализации; `serverBaseUrl` игнорируется в
   `LoadWeightsAsync` (`LocalModelRepository.cs:10-14,28`; рекон-25). План:
   замкнуть сетевой export/import весов или явно зафиксировать local-only.

5. **Continue-training адаптера не замкнут (нюанс порядка).** Родительский
   чекпойнт грузится ПОСЛЕ заморозки (`LlmIngredientLogicalUnits.cs:339→344`), а
   LoRA-адаптер отдельно не сохраняется/не грузится (пробел №2). План: определить
   формат LoRA-чекпойнта и порядок load→apply→continue.

6. **Нет teacher-provenance полей** в `DpoPair` (`DpoPair.cs:3-8`) и манифесте
   (рекон-25/18). План: добавить поля Legal Teacher Gate (§6.3) при любом teacher.

7. **Нет held-out/ablation/fresh-load как отдельной evidence** в 18 (рекон-18).
   Метрики выводятся на обучающих парах (`LastLoss/LastAccuracy`,
   `LlmIngredientLogicalUnits.cs:389-390`). План: добавить held-out-раннер +
   ablation (no-shortcut) + fresh-load пробу.

8. **Нет split common/adaptive на уровне lifecycle (R5).** На уровне 25 нет
   common-vs-user флага/двух ног (рекон-25). Контракт `ILearningMachinePair` есть
   (`LearningMachineContracts.cs:245-261`), но lifecycle не разделён. План:
   ввести split в манифест/репозиторий.

9. **Авто-построение пар для графа отсутствует в core.** 11.DPO пар не строит
   (рекон-11); `BuildChosenCapabilityText`/Rejected в 18 — прообраз, но не
   графовый. План: отдельный слой авто-DPO «узел/ребро → chosen/rejected» (это
   тема смежного гайда построения пар).

10. **Несогласованность ранга LoRA** (2 vs 8) между тренерами (§5.4). План:
    унифицировать или явно обосновать per-tier.

11. **Дубль реестров/манифестов.** `InMemoryRemoteUnitLocator` vs
    `ModelEndpointRegistry`; snake_case vs PascalCase схемы манифеста (рекон-25/
    assets). План: один канонический реестр/схема.

---

## 8. Ссылки

### 8.1. Скилы/правила (прочитаны для гайда)
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md`
  (уровни, KNN-нота, Legal Teacher Gate, Graph Reconstruction Gate, leakage).
- `C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md`
  (Layer 5/5A — trainable vs stub, числовые метрики, deterministic-boundary).
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (триггеры загрузки скилов).

### 8.2. Рекон-сводки (codex-recon-*, evidence-only)
- `.../dpo legal build/codex-recon-core-11-dpo.md`
- `.../dpo legal build/codex-recon-core-10-lora.md`
- `.../dpo legal build/codex-recon-core-16-autonomous.md`
- `.../dpo legal build/codex-recon-core-25-modelserver.md`
- `.../dpo legal build/codex-recon-core-18-capability.md`
- `.../dpo legal build/codex-recon-core-27-translation.md`
- `.../dpo legal build/codex-recon-core-assets.md`
- `.../dpo legal build/codex-recon-mvp-novyy-domen.md`

### 8.3. Реальные файлы кода (с путями)
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs` (Step/StepMany/EvaluateMany/
  StepVector/LogProbs/BackwardLogProbs/SaveMetrics; `DpoVectorBatch`).
- `C:/Projects/llm_mvp_core/11.DPO/Models/DpoPair.cs`,
  `C:/Projects/llm_mvp_core/11.DPO/Models/DpoMetrics.cs`.
- `C:/Projects/llm_mvp_core/10.LoRA/LoraApplicator.cs`,
  `C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs`,
  `C:/Projects/llm_mvp_core/10.LoRA/LoraConfig.cs`.
- `C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs`
  (заглушка `TrainAsync`).
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs`
  (`LogicalDpoTrainingUnit`), `.../DefaultLogicalCapabilityTrainer.cs`,
  `.../TeachingRegistrySnapshot.cs` (манифест-контракт).
- `C:/Projects/llm_mvp_core/25.ModelServer/Local/LocalModelRepository.cs`
  (+ Contracts `IModelRepository`/`IModelServer`/`IRemoteUnitLocator`).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs`
  (Export/Import/Save/LoadCheckpoint, захардкоженный RewardMargin),
  `.../TranslationUnitContracts.cs` (`TranslationLearningExample`,
  `ITranslationUnitTrainer`, `TranslationKnnStore`).
- `C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs` (SaveCheckpoint/
  LoadCheckpoint).
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` (иерархия,
  `ILearningMachine`, `LearningBatchResult`, `ILearningMachinePair`).
```
