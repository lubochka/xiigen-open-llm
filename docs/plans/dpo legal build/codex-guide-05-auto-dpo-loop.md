# Гайд 05 — Замкнутая авто-DPO петля (сердце проекта ILearningDynamicGraph)

Автор гайда: Codex (модель Opus 4.8 MAX), роль — автор гайда. Это часть ПЛАНА
(документация-гайд), а не разрешение на реализацию продукта. Все цитаты кода —
из реально существующих файлов с `file:line`. Иерархия и уровни проверены по
`docs/skills/trainable-architecture-decision-SKILL.md` и
`00.Core/LearningMachineContracts.cs`.

Связанные гайды этого плана: гайд 01 (легальный teacher/provenance гейт),
гайд 03 (измерение обучения), гайд 04 (async-прогон одного входа через РАЗНЫЕ
комбинации юнитов и выбор лучшего). Этот гайд 05 — про петлю, которая их
связывает в один цикл само-улучшения.

---

## 1. Назначение гайда

Гайд описывает **замкнутую авто-DPO петлю** — главный механизм проекта, который
делает модель само-обучающейся без ручного написания каждой DPO-пары. Петля
выглядит так:

1. **прогнать** наш проект (ILearningDynamicGraph / его узлы и рёбра) на реальной
   задаче;
2. **обнаружить** низкокачественный выход узла/ребра/маршрута;
3. **взять выход РАЗРЕШЁННОГО teacher как goal** (правильный ответ-цель);
4. **обучить ILearningDynamicGraph СТРОИТЬ DPO** — то есть автоматически собрать
   пару `chosen` (как у teacher/лучшей комбинации) / `rejected` (наш слабый
   выход) с аннотацией `[WRONG: ...]`;
5. **измерить** (по гайду 03), что обучение реально произошло (reward margin,
   loss, held-out accuracy, ablation);
6. **повторять** до достаточного качества (промоушен только при улучшении).

ОБЯЗАТЕЛЬНОЕ ядро петли (требование задачи): на шаге 1 один и тот же вход
прогоняется **async через РАЗНЫЕ комбинации юнитов** (см. гайд 04), и лучший
выход становится обучающим сигналом `chosen`. DPO строит **либо сам
ILearningDynamicGraph, либо разрешённый teacher** (МОДЕЛЬ-учитель из открытых
весов/permissive). **Никогда не используются Claude/OpenAI/Gemini** как
teacher/distillation-цель — только как judge/eval-материал, если Люба явно
разрешит (см. §6 и гайд 01).

Гайд отвечает на 4 практических вопроса для плана:
- что из этого УЖЕ есть в коде (и работает), а что — ЗАГЛУШКА;
- какой именно алгоритм петли построить;
- как выглядит правильная DPO-пара на реальных значениях;
- какие гейты (анти-leakage, легальный teacher, запрет скрытого детерминизма,
  измерение) защищают петлю от само-обмана.

---

## 2. Концепт простыми словами (аналогия, без кода)

Представьте мастерскую с **учеником** (наша маленькая обучаемая модель —
ILearningDynamicGraph и его узлы) и **наставником** (разрешённый teacher —
открытая модель с дружественной лицензией).

Цикл одного «урока»:

1. **Ученику дают задачу.** Например: «по запросу пользователя выбрать домен и
   под-фазу». Ученик решает её **несколькими способами одновременно** — разными
   комбинациями своих инструментов (узлов). Это как если бы он написал
   несколько вариантов ответа разными методами (гайд 04: async-прогон РАЗНЫХ
   комбинаций).
2. **Сравниваем варианты ученика между собой и с эталоном наставника.** Если
   лучший вариант ученика всё ещё хуже, чем у наставника, — фиксируем разрыв.
   Эталон (`goal`) берём ТОЛЬКО у легального наставника, не у запрещённых
   закрытых моделей.
3. **Делаем «карточку урока» (DPO-пару).** На карточке: слева тот же вопрос
   (`A-side`), в графе «правильно» (`chosen`) — лучший/эталонный ответ, в графе
   «неправильно» (`rejected`) — слабый ответ ученика, и короткая пометка
   `[WRONG: ...]` — ПОЧЕМУ слабый ответ хуже (одно функциональное последствие,
   а не «потому что плохо»).
4. **Ученик учится на карточке** — но не зубрит дословно, а сдвигает
   предпочтение в сторону `chosen`. В терминах DPO: повышает `log P(chosen)`
   относительно `log P(rejected)` так, чтобы **reward margin стал > 0**.
5. **Проверяем, что урок усвоен** (гайд 03): на отложенных задачах (held-out),
   а не на тех же карточках. И проверяем, что ученик не сжульничал — не выучил
   позицию варианта в списке, имя файла или дословный текст вопроса (ablation /
   permutation).
6. **Повторяем**, пока качество не станет достаточным; «диплом» (промоушен
   чекпойнта) выдаём ТОЛЬКО если измеренное качество выросло.

Ключевая идея «сердца проекта»: **ученик сам себе готовит карточки-уроки** из
своих же ошибок и эталона наставника. Человек не пишет каждую DPO-пару руками —
петля строит их автоматически и непрерывно.

Важная честная оговорка (анти-masquerade): «решать несколькими способами» и
«сравнивать» — это НЕ обязательно «модель умная». Если выбор лучшего варианта
делает фиксированная формула подсчёта ключевых слов, то это **детерминированный
baseline**, а не обученный интеллект (см. §3 про `26.TrainableRouter` и §6 про
запрет скрытого детерминизма). Петля имеет право называться обучающей только
когда обучаемый уровень реально сдвигает веса и это доказано измерением.

---

## 3. Как это сейчас реализовано в нашем коде

Здесь — точные ссылки `file:line`, что РАБОТАЕТ, а что ЗАГЛУШКА. Классификация
по `trainable-architecture-decision-SKILL.md`.

### 3.1. Сам DPO-движок — РАБОТАЕТ (реальная математика)

`11.DPO/DpoTrainer.cs` — настоящий DPO-тренер, не заглушка.

- Формула потерь и reward margin (комментарий-инвариант):
  `11.DPO/DpoTrainer.cs:8-9`
  ```
  // Loss = -E[log σ(β · (log π(y_w|x) - log π(y_l|x) - log π_ref(y_w|x) + log π_ref(y_l|x)))]
  ```
- Реальный расчёт margin/loss/accuracy на шаге: `11.DPO/DpoTrainer.cs:42-65`
  ```csharp
  float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
  float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
  float margin         = chosenReward - rejectedReward;
  float loss           = -MathF.Log(Sigmoid(margin) + 1e-8f);
  ...
  Accuracy = margin > 0 ? 1f : 0f
  ```
  `Beta = 0.1f` (`11.DPO/DpoTrainer.cs:13`). Есть ручной backward через logits
  (`BackwardLogProbs`, `:264-309`) и AdamW (`:54`). Метрики сохраняются атомарно
  до возврата (`SaveMetrics`, `:322-328`, tmp→Move). Это реальный обучающий шаг.
- `StepVector(...)` (`:169-224`) — батч с teaching-вектором и весами; именно его
  зовёт обучающий юнит из проекта 18.

**Вывод по §3.1:** ядро «как именно учить на паре chosen/rejected» в проекте
есть и работает. Петле НЕ нужно изобретать DPO-математику — нужно её **кормить
правильными авто-построенными парами и измерять результат**.

Контракт пары — намеренно простой: `11.DPO/Models/DpoPair.cs:3-8`
```csharp
public record DpoPair { public string Prompt; public string Chosen; public string Rejected; }
```

### 3.2. Авто-построение DPO «из наблюдённой ветки» — РАБОТАЕТ ЧАСТИЧНО (есть пайплайн, но teacher-цели нет)

`18.CapabilityTraining` — это ближайший готовый референс авто-DPO для
узлов/флоу. Пайплайн `DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync`:

- Авто-сборка обучающего ряда из наблюдённой ветки:
  `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:167-175`
  ```csharp
  var trainingText = BuildTrainingTextCore(compiledPrompt, compiledText, request.OriginalRequest.UserRequest, branch, subPhase.Value!.SubPhase);
  branchPlans.Add(new TrainingBranchPlan(
      branch, subPhaseId, subPhase.Value.SubPhase,
      trainingText.Text,
      BuildChosenCapabilityText(subPhase.Value.SubPhase),                                  // chosen
      $"Do not generate a capability for sub-phase {subPhase.Value.SubPhase.SubPhaseId}.", // rejected
      trainingText.TeachingVector));
  ```
- Реальный обучающий юнит DPO: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:308-415`
  (`LogicalDpoTrainingUnit : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>`):
  применяет LoRA (`LoraApplicator.Apply`, `:339`), грузит родительский чекпойнт
  для continue-training (`:340-350`), кодирует пары в тензоры (`BuildBatch`,
  `:418-438`) и крутит `DpoTrainer.StepVector` с ранней остановкой
  (`:367-376`). Метрики — реальные (`LastLoss/LastAccuracy`, `:385-391`).
- Inference learned-юнита: `TransformerEmbeddingKnnLogicalUnit<TOutput>`
  (`:31-171`) — embedding + HNSW поиск + fusion-скоринг (`:151-156`), confidence
  `0.50 + fused*0.49` (`:156`).

**ЧТО ЗАГЛУШКА / СЛАБО здесь (важно для плана):**

1. **`chosen`/`rejected` строятся ДЕТЕРМИНИРОВАННЫМ шаблоном, без teacher.**
   - `chosen`: `BuildChosenCapabilityText` — это шаблон-строка, не выход
     наставника: `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:680-683`
     ```csharp
     return $"Generated capability for sub-phase {subPhase.SubPhaseId}: {subPhase.Name}. Execute the requested work through task type {BuildTaskType(subPhase)}.";
     ```
   - `rejected`: дословный шаблон-отрицание
     `"Do not generate a capability for sub-phase {Id}."`
     (`:174`). По рекону `mvp-internal-learn` именно этот шаблон-отрицание дал
     overlap ~0.70 и провал калибровки 2026-05-06 (29/40 батчей с отрицательным
     margin, loss=ln2≈0.693). То есть это **анти-пример качества пары**.
   - Для авто-DPO петли (наша задача) `chosen` ОБЯЗАН приходить от лучшей
     async-комбинации (гайд 04) или легального teacher (гайд 01), а не от
     шаблона; `rejected` — реальный слабый выход с осмысленным `[WRONG: ...]`.
2. **Нет полей teacher-provenance** в манифесте (нет `model_id`,
   `model_license`, `teacher_access_mode` — см. рекон `core-18-capability` §5).
3. **Held-out/ablation отсутствуют** — выводятся `LastLoss/LastAccuracy` на
   обучающих парах, нет отдельного held-out и no-shortcut доказательства
   (рекон `core-18-capability` §5). Для `PASS_AS_TRAINED_FEATURE` этого мало.
4. **Checkpoint-root по умолчанию ВНЕ репозитория**
   (`Path.GetTempPath()/llm-mvp-capability-training`, рекон §2) — нарушает
   R5/Trainable Unit pattern, нет публикации в `25.ModelServer`.

Классификация по скилу: `LogicalDpoTrainingUnit` = `OK_ALLOWED_LEVEL`
(реальное обучение); `TransformerEmbeddingKnnLogicalUnit` = `OK_MODEL_COMPONENT`
(KNN/HNSW как memory-слой); fusion-re-ranker `ScoreCandidates` (`:177-204`) =
`OK_INTERNAL_DETAIL` пока он re-ranker внутри обученного юнита, НО требует
ablation, иначе риск, что выбор сводится к детерминированной формуле.

### 3.3. Контур «обучить → измерить → повысить только при улучшении» — РАБОТАЕТ как образец (но DPO-канал ПУСТОЙ)

`16.AutonomousLearning` — готовый прецедент само-улучшения с гейтом дельты, НО
именно «авто-DPO» здесь **не подключён**:

- Гейт «повышать только при улучшении»:
  `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs:137-140`
  ```csharp
  if (enhanced <= baseline)
      return OperationResult<CycleResult>.Fail(
          $"Candidate adapter was not promoted because enhanced accuracy {enhanced:0.###} did not exceed baseline {baseline:0.###}.",
          "ACCURACY_NOT_IMPROVED");
  ```
  Это прямой образец шага 6 петли (промоушен по дельте) + кандидат на
  ablation/no-shortcut гейт.
- **DPO-канал ПРОЛОЖЕН, но вызывается ПУСТЫМ** — «розетка без вилки»:
  `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs:120`
  ```csharp
  var trainResult = await _trainer.TrainAsync(domain, weightedExamples, []); // <- [] = пустой список DPO-пар
  ```
  Сигнатура реально принимает DPO-пары:
  `16.AutonomousLearning/Training/AutonomousTrainer.cs:44-47,61`
  ```csharp
  public async Task<OperationResult<string>> TrainAsync(
      string domain, IReadOnlyList<TrainingExample> newExamples, IReadOnlyList<DpoPair> dpoPairs)
  ...
  var trainResult = await _trainer.TrainAsync(domain, mixedBatch, dpoPairs); // проброс в 14.ContinuousLearning
  ```
  То есть место для авто-DPO определено, но «модель сама находит слабые места →
  строит пары» здесь НЕ реализовано: нет анализа ошибок → нет chosen/rejected.

**Вывод по §3.3:** у нас есть и гейт дельты, и проброс DPO-канала, и
anti-forgetting (`ReplayBuffer` 70/30). Не хватает именно **генератора пар из
ошибок + легального teacher-goal** — это и есть дельта до «сердца проекта».

### 3.4. «Trainable» маршрутизация — СЕЙЧАС ДЕТЕРМИНИРОВАННАЯ (masquerade-риск)

`26.TrainableRouter` называет себя обучаемым, но рантайм-authority —
детерминированный подсчёт ключевых слов (рекон `core-26-router` §«Классификация»):

- `KeywordUnitScorer.Score` — `text.Contains(keyword)` подсчёт + boosts →
  вердикт скила `FORBIDDEN_RUNTIME_SHORTCUT` (детерминизм как финальная
  authority там, где заявлен trainable router).
- `RoutingProfileTrainer.Train` — сбор словаря ключевых слов, без весов/loss/
  чекпойнта → `REPORT_CLAIM_TOO_STRONG` (это keyword-harvest, не обучение).
- `RoutingTrainingResult.RewardMargin/DpoAccuracy/Promoted` — контрактные
  плейсхолдеры, нигде реально не считаются (рекон §«Классификация»).
- Lifecycle отсутствует целиком: grep по проекту НЕ нашёл `checkpoint`,
  `ExportAsync`, `ImportAsync`, реального DPO-цикла (рекон §«Lifecycle»).

**Вывод по §3.4:** `26` даёт ГОТОВЫЙ ФОРМАТ DPO-пар (`RoutingDpoPairBuilder`,
`OrchestrationPromptDpoPairBuilder`) и метрики acceptance — переиспользуем как
заготовку. Но его рантайм-выбор юнита нельзя использовать как «обученный» —
он `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` только при честной пометке baseline.
В авто-DPO петле его роль — **источник `rejected` (слабый детерминированный
выход) и формат пары**, а `chosen` приходит от обученного уровня/teacher.

### 3.5. Куда петля «приземляется» по иерархии (контракт уже есть)

Иерархия обучаемых уровней объявлена в
`00.Core/LearningMachineContracts.cs` (namespace `AgentInfrastructure.Logical`):

- `ILearningMachine<TDomain>` — Level 5, методы
  `TrainBatchAsync / EvaluateAsync / ExportAsync / ImportAsync /
  GetOrchestratorAsync`: `00.Core/LearningMachineContracts.cs:208-234`.
- DPO-триплет уже типизирован как `LearningTrainingItem(ItemId, Prompt,
  ChosenResponse, RejectedResponse)`: `00.Core/LearningMachineContracts.cs:30-39`
  и батч `LearningBatch<TDomain>`: `:18-27`.
- Метрики батча `LearningBatchResult` несут `AccuracyBefore/AccuracyAfter/
  RewardMargin/CpuMs/PeakRamMb/ElapsedMs/PromotedUnitIds`:
  `:43-62` — это готовая рамка измерения для петли (гайд 03).
- Правило A-LM-02 (в шапке файла, `:8`): `Train()` только на `ILearningMachine`;
  `ILogicalUnit`/`ILogicalFlow` — чистый инференс. Петля обязана это уважать:
  обучает через машину/тренер, а не дописывает Train* в юниты.

**Где НЕТ кода (это новая надстройка плана):** типов
`ILearningDynamicGraph / ILearningUnit / ILearningFlow / ILearningOrchestrator`
в репозитории как отдельных интерфейсов НЕТ (в задаче они — новые имена; в репо
канон — `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator/ILearningMachine`).
Их граф-контракты (узлы/рёбра/fan-out/fan-in/many-to-many/async) надо
спроектировать в плане по Graph-Style Reconstruction Gate скила.

---

## 4. Пошаговый рецепт / алгоритм петли

Ниже — алгоритм без кода: что происходит, где, в каком порядке, какие
классы/контракты задействованы. Это рецепт для ПЛАНА, не реализация.

### Шаг 0 — Подготовка (один раз на петлю)
- Зафиксировать **обучаемый объект** петли: какой узел/ребро/маршрут
  ILearningDynamicGraph учим (например, выбор домена+под-фазы на ребре
  `orchestrator→unit`).
- Зафиксировать **legal teacher** (гайд 01): `teacher_access_mode`,
  `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`,
  `closed_proprietary_model=false`. Без зелёного гейта teacher-выходы как
  `chosen` запрещены.
- Загрузить чекпойнт обучаемого уровня: сначала user/cache, затем repo/common;
  если нет — честный `untrained scaffold` (lifecycle из скила).

### Шаг 1 — Прогон одного входа через РАЗНЫЕ комбинации (ОБЯЗАТЕЛЬНО, async; гайд 04)
- Взять реальный вход `X` (запрос пользователя / задача узла).
- Async прогнать `X` через N **разных комбинаций юнитов/маршрутов** графа
  (разные узлы, разный порядок, разные параметры) — детали в гайде 04.
- Собрать множество кандидатов-выходов `{Y_1..Y_N}` с их confidence и трассой
  (какая комбинация дала результат). Источник идей формата кандидата —
  `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync`
  (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs:129-171`).

### Шаг 2 — Обнаружить низкокачественный выход
- Оценить кандидатов (гайд 03 + judges из лучших моделей разрешены как
  СУДЬИ-eval, не как teacher-цель): выбрать лучший внутренний `Y_best`.
- Признать «слабым» те выходы, что заметно хуже `Y_best`/эталона: низкий
  confidence, провал verdict, расхождение с held-out ожиданием. Это кандидаты в
  `rejected`. (Прецедент гейта дельты:
  `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs:137-140`.)

### Шаг 3 — Взять выход разрешённого teacher как goal
- Если `Y_best` всё ещё хуже наставника — получить эталон `Y_goal` у **legal
  teacher** (открытая permissive-модель), пройдя гейт §6/гайд 01.
- Если teacher недоступен/не нужен — `chosen = Y_best` (лучшая async-комбинация
  ученика). Запрещённые модели (Claude/OpenAI/Gemini) НЕ могут давать `Y_goal`
  как обучающую цель (могут только судить, если Люба явно разрешит).
- Зафиксировать provenance строки (кто сгенерировал `chosen`).

### Шаг 4 — Обучить ILearningDynamicGraph СТРОИТЬ DPO (авто-сборка пары + обучение)
- Собрать пару: `A-side = X` (вход), `chosen = Y_goal` (или `Y_best`),
  `rejected = слабый выход`, плюс `[WRONG: <одно функциональное последствие>]`.
  Формат-образец: `RoutingDpoPairBuilder`/`OrchestrationPromptDpoPairBuilder`
  (проект 26) и `LogicalDpoPreferenceExample`
  (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs:281`).
- Прогнать **анти-leakage гейты** §6 (overlap<0.60, нет утечки позиции/имени
  файла/дословного промпта, near-miss-качество).
- Обучить уровень одним батч-шагом: через `ILearningMachine.TrainBatchAsync`
  (`00.Core/LearningMachineContracts.cs:212-213`) →
  `LogicalDpoTrainingUnit.ExecuteAsync` →
  `DpoTrainer.StepVector` (`11.DPO/DpoTrainer.cs:169-224`). Если ребро уже
  обучалось — continue-training из чекпойнта
  (`LlmIngredientLogicalUnits.cs:340-350`), не с нуля.
- Anti-forgetting: подмешать прошлые принятые пары (replay), образец —
  `16.AutonomousLearning/Training/ReplayBuffer.cs` (70/30).

### Шаг 5 — Измерить (гайд 03)
- На **held-out** наборе (не на обучающих парах) замерить
  `AccuracyBefore/After`, `RewardMargin` (`LearningBatchResult`,
  `00.Core/LearningMachineContracts.cs:43-62`; `EvaluateAsync`, `:217-218`).
- Гейт принятия строки (по рекону `mvp-internal-learn` §2 + гайд 03):
  `reward_margin ≥ 0` И `loss < 0.693` (ln2 = random-guess) И overlap-scan
  пройден. Плюс ablation/permutation: chosen стабилен при перестановке
  кандидатов и без позиционного сигнала.

### Шаг 6 — Промоушен и повтор
- Повышать чекпойнт (регистрация/публикация через `25.ModelServer` —
  Trainable Unit pattern) **только если** held-out качество выросло
  (образец гейта: `AutonomousLearningPipeline.cs:137-140`).
- Сохранить ledger строки (одна строка на пару, без перезаписи) — образец
  `TeachingRowRecord` из рекона `mvp-orch-teaching` §2.
- Если качество ещё недостаточно — взять следующий вход и повторить с шага 1.
  Кумулятивность: одно состояние графа живёт через все строки; после строки N
  ретест 2-3 прошлых (regression window).

### Где петля приземляется по уровням (для §0 UML плана)
```
ILearningDynamicGraph (новый, план)            ← владеет петлёй и графом узлов/рёбер
  └── ILearningMachine<TDomain> (есть: 00.Core) ← TrainBatchAsync/EvaluateAsync/Export/Import
        └── ILogicalOrchestrator (есть)         ← маршрут + reprocessing-петля
              └── ILogicalFlow<T> (есть)         ← async-комбинации узлов (fan-out/fan-in), гайд 04
                    └── ILogicalUnit<TIn,TOut>   ← узел; learned: LogicalDpoTrainingUnit / KnnUnit (проект 18)
тренер обучения (вне инференс-иерархии, A-LM-02): DefaultLogicalCapabilityTrainer + DpoTrainer
```

---

## 5. Конкретные примеры с реальными значениями

### 5.1. Пример хорошей авто-построенной DPO-пары (routing-решение на ребре)

Формат подтверждён реальными полями проекта (`LearningTrainingItem`
`00.Core/LearningMachineContracts.cs:30-39`; trainable-строка V309 из рекона
`mvp-internal-learn-2` §2). Это пара для обучаемого ребра «выбор домена+под-фазы».

```text
A-side (Prompt):
  "Add a confidentiality check before the integration layer writes the report to disk."
  [runtime flags] UseLearnedUnitWhenAvailable: true; UseKnowledgeEdgeDetection: true

chosen (Y_goal от лучшей async-комбинации/legal teacher):
  domain: ARCHITECTURE
  sub_phase: security-verification (node arbiter-selector → security unit)
  reason: "boundary decision (где и когда проверять конфиденциальность) предшествует реализации"
  PredicateIds: ["ARCHITECTURE","SECURITY"]
  confidence: 0.92

rejected (наш слабый выход из другой комбинации):
  domain: IMPLEMENT
  sub_phase: code-generation
  PredicateIds: ["IMPLEMENT"]
  [WRONG: триггеры 'integration layer'/'writes' притянули IMPLEMENT, но запрос про
          ГРАНИЦУ контроля (когда/где проверять), а не про написание кода —
          сначала ARCHITECTURE/SECURITY-решение, потом реализация]

provenance: teacher_access_mode=hosted_open_weight; model_license=Apache-2.0;
            closed_proprietary_model=false; teacher_output_used_for_training=true
overlap_ratio: 0.18   (< 0.60 — гейт пройден)
candidate_order_invariant: true; chosen_skill_stable_across_permutations: true
```

Почему пара хорошая: `chosen` и `rejected` отличаются на ОДНОЙ границе решения
(ARCHITECTURE vs IMPLEMENT), `rejected` правдоподобен (а не отрицание/абсурд),
`[WRONG:]` называет конкретное функциональное последствие, overlap низкий,
позиция/имя-файла не используются как сигнал.

### 5.2. Пример ПЛОХОЙ пары (как сейчас в коде проекта 18 — анти-пример)

```text
A-side: <скомпилированный prompt + 60-мерный teaching-вектор>
chosen:  "Generated capability for sub-phase SP-07: ... Execute the requested work through task type X."   // шаблон, не teacher
rejected: "Do not generate a capability for sub-phase SP-07."                                              // отрицание
[WRONG: отсутствует]
```
Источник: `DefaultLogicalCapabilityTrainer.cs:173-174` и `:680-683`.
Почему плохо: `rejected` — это ОТРИЦАНИЕ `chosen`, а не правдоподобный неверный
маршрут → overlap ~0.70 → reward margin уходит в минус, loss≈0.693 (random).
Это ровно тот шаблон, что вызвал провал калибровки 2026-05-06 (рекон
`mvp-internal-learn` §2). В авто-DPO петле такой `rejected` запрещён гейтом §6.

### 5.3. Пример измерения (значения из реальных прогонов корпуса)

```text
baseline RUN-416 / V246:  weighted accuracy 0.7068; reward margin 0.016892
актуальный RUN-479 / V309: rows 1404; avg accuracy 0.838; avg reward margin 0.0289; final accuracy 1.0
домены V309: orchestration 501 / plans 829 / skills 74
ablation V308: поздние schema-shaped строки → QUARANTINE_DO_NOT_TRAIN (вредны, margin↓)
```
Источник: реконы `mvp-internal-learn` §1 и `mvp-internal-learn-2` §2. Это
показывает рабочий диапазон margin (малый, но положительный) и что ablation
реально отбраковывает вредные строки — петля должна это делать на шаге 5.

---

## 6. Гейты и правила петли

Каждый гейт защищает петлю от само-обмана. Гейт = жёсткое условие, провал =
строка/чекпойнт не принимается.

### G1 — Легальный teacher / provenance (связь с гайдом 01)
- `chosen`/`Y_goal` для обучения может прийти ТОЛЬКО от first-party данных, от
  обученного уровня (лучшая async-комбинация), или от **разрешённого hosted
  open-weight/permissive teacher** после проверки terms+license.
- **Claude / OpenAI / Gemini / Gemini Grounding — ЗАПРЕЩЕНЫ как
  distillation-teacher / chosen / rejected / ranking-target.** Если
  `closed_proprietary_model=true` И `teacher_output_used_for_training=true` →
  вердикт `DATASET_REBUILD_REQUIRED` (без исключений по скилу).
- Judges из лучших моделей РАЗРЕШЕНЫ только как **eval/судьи/критика**, не как
  обучающая цель, и только если Люба явно разрешит (CLAUDE.md: closed outputs
  допустимы лишь как non-training judge/eval).
- Обязательные поля строки: `teacher_access_mode, hosting_provider, model_id,
  model_license, provider_terms_checked, teacher_output_used_for_training,
  closed_proprietary_model, request_id/model_version`. Лицензии для common:
  MIT/Apache-2.0 предпочтительны; CC BY 4.0 только с атрибуцией; RFC/CWE/NIST —
  public domain; CC BY-SA/AGPL/GPL/unknown — запрет (рекон `mvp-external-sources`
  §2). Источники с model-output risk (ShareGPT/WizardLM/Alpaca/Dolly/FLAN-raw)
  дают только A-side форму, ассистент-ответ НЕ копируется как `chosen`.

### G2 — Анти-leakage качества пары
- `overlap_ratio(chosen, rejected) < 0.60` (иначе брак; token-overlap test).
- `rejected` правдоподобен и близок к `chosen`, отличается на ОДНОЙ границе
  решения; имеет код таксономии и осмысленный `[WRONG: ...]` (функциональное
  последствие, не «плохо»).
- Запрещено, чтобы `chosen` предсказывался по: позиции кандидата в списке, имени
  файла/пути, source/pair id, дословной памяти промпта, project-specific
  команде (для универсальной модели). Это прямые `QUARANTINE`-сигналы
  (`dpo-triple-format-SKILL.md` + `trainable-architecture-decision-SKILL.md`).
- Permutation-инвариантность: `candidate_order_invariant=true`,
  `chosen_skill_stable_across_permutations=true`, иначе
  `QUARANTINE_DO_NOT_TRAIN` или переписать пару.

### G3 — Запрет скрытого детерминизма (Graph-Style edge authority)
- Для КАЖДОГО ребра графа назвать владельца финальной authority:
  детерминированный хелпер / обучаемый unit / flow / orchestrator / machine.
- Скрытый детерминированный scorer/router/lookup/counter на ребре, выдающий
  себя за trainable → `FORBIDDEN_RUNTIME_SHORTCUT` (как `KeywordUnitScorer` в
  проекте 26). Детерминированный хелпер допустим ТОЛЬКО явно помеченным как
  eval-only/baseline/helper, и должен быть перекрыт обученным уровнем с
  ablation-доказательством, что выбор НЕ сводится к формуле.
- `RewardMargin/DpoAccuracy/Promoted` нельзя оставлять контрактными
  плейсхолдерами (как в `RoutingTrainingResult`) — это `REPORT_CLAIM_TOO_STRONG`.

### G4 — Гейты измерения (связь с гайдом 03)
- Строка принимается (`ACCEPT_ROW`) только если:
  `measurement_verdict=PASS_AS_TRAINED_FEATURE` И `legal_verdict=ACCEPT` И
  `anti_leakage_verdict=PASS` И
  `reward_margin ≥ 0` И `loss < 0.693` И overlap-scan пройден.
- Качество доказывается на **held-out**, а не на обучающих парах
  (`EvaluateAsync`, `00.Core/LearningMachineContracts.cs:217-218`). Самоотчёт
  `LastAccuracy` на train-парах ≠ доказательство (рекон `mvp-internal-learn-2`:
  «unit-test pass rate ≠ DPO accuracy»; валидный отчёт идёт от раннера: rows,
  loss, accuracy, reward margin, CPU, RAM-delta, elapsed).
- Промоушен только при росте held-out (гейт дельты,
  `AutonomousLearningPipeline.cs:137-140`).

### G5 — Lifecycle обучаемого уровня (Trainable Unit pattern, R5)
- Загрузка состояния: user/cache → repo/common → честный untrained scaffold.
- Continue-training из существующего чекпойнта (не с нуля), кроме явно
  названного cold-start.
- Export/import/share + публикация чекпойнта через `25.ModelServer`
  (`IModelRepository`), локатор `IRemoteUnitLocator`, манифест с checkpoint-ref.
- Common-модель + логика + общие сеты — в `llm_mvp_core`; адаптивная
  user-модель + user-сеты — на стороне пользователя (R5). Checkpoint-root НЕ
  должен быть `GetTempPath()` (текущий дефект проекта 18).

### G6 — Honest-untrained vs trained (две легальные PASS-метки)
- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`: контракт есть, честно сообщает
  `not_ready`, не прячет детерминизм, и эта честность доказана тестами.
- `PASS_AS_TRAINED_FEATURE`: чекпойнт + held-out метрики + ablation + fresh-load
  + continue-training + e2e через реальный entrypoint.
- Подавать scaffold как trained feature = masquerade (самый опасный
  false-positive). Петля обязана различать эти метки в отчёте.

---

## 7. Пробелы / что нужно построить в плане

Это список «дельты до сердца проекта» — что в коде отсутствует/заглушено и
должно стать work-order'ами плана (без реализации здесь).

1. **Генератор пар из ошибок + async-мульти-комбинация (шаги 1-4).** Сейчас
   `chosen/rejected` — шаблоны (`DefaultLogicalCapabilityTrainer.cs:173-174,
   680-683`), а DPO-канал в `16` пустой (`AutonomousLearningPipeline.cs:120`).
   Нужно: модуль, который async прогоняет вход через разные комбинации (гайд 04),
   выбирает лучший/teacher-goal и строит пару. Назвать класс/контракты в плане.
2. **Legal teacher интеграция (гайд 01).** Нет полей provenance и нет вызова
   разрешённого teacher как источника `Y_goal`. Добавить teacher-гейт + поля в
   строку/манифест (рекон `core-18-capability` §5, `mvp-external-sources` §1).
3. **Held-out + ablation/permutation измерение (гайд 03).** Нет held-out и
   no-shortcut доказательства в проекте 18. Построить eval-набор и ablation-гейт
   (рекон `core-18-capability` §5; пример ablation V308).
4. **Граф-контракты ILearningDynamicGraph.** Нет типов
   `ILearningDynamicGraph/ILearningUnit/ILearningFlow/ILearningOrchestrator`
   как графа узлов/рёбер с fan-out/fan-in/many-to-many/multi-output edge
   contracts/async и владельцем authority на каждом ребре (Graph-Style
   Reconstruction Gate). Спроектировать в §0/§5 плана; маппинг на канон
   `ILogicalUnit/Flow/Orchestrator/ILearningMachine`
   (`00.Core/LearningMachineContracts.cs:208-234`).
5. **Замена детерминированного роутинга проекта 26 на обучаемый уровень.**
   `KeywordUnitScorer`/`RoutingProfileTrainer` — `FORBIDDEN_RUNTIME_SHORTCUT`/
   `REPORT_CLAIM_TOO_STRONG`. План должен либо явно пометить их baseline, либо
   заменить обучаемым `IUnitScorer`/`ILogicalUnit` с lifecycle (рекон
   `core-26-router`).
6. **Lifecycle/публикация чекпойнтов (R5/Trainable Unit pattern).**
   Checkpoint-root вне репо (`GetTempPath`), нет публикации в `25.ModelServer`,
   нет export/import/locator (рекон `core-18-capability` §5). Добавить.
7. **Provenance самих chosen/rejected в существующих корпусах (блокер legal).**
   В прочитанных файлах не доказано, какой моделью сгенерированы `chosen`/`[WRONG:]`
   в `train-ready-v2` — если закрытой → `DATASET_REBUILD_REQUIRED`. План должен
   проверить per-source (реконы `mvp-internal-learn-2` §5, `mvp-local-wiki` §5,
   `mvp-orch-teaching` §5: rollout Codex/GPT-5 запрещён как teacher).
8. **Расхождение указателей версии train-ready** (V192/3884 vs V309/1404) —
   определить канонический train-ready (рекон `mvp-internal-learn-2` §5).
9. **Связка петли с измерением «одна строка за раз» + regression window**
   (кумулятивность, continue-training, ретест 2-3 прошлых) — образец в рекон
   `mvp-orch-teaching` §2; перенести в work-order петли.

---

## 8. Ссылки

### Реконы (evidence-only, эта же папка `dpo legal build`)
- `codex-recon-core-16-autonomous.md` — прецедент само-улучшения; пустой DPO-канал.
- `codex-recon-core-18-capability.md` — рабочий референс авто-DPO пайплайна.
- `codex-recon-mvp-internal-learn.md` — метод авто-построения DPO + измерение.
- `codex-recon-mvp-internal-learn-2.md` — model-led прогон, формат trainable-строк.
- `codex-recon-mvp-local-wiki.md` — per-node A→B метод; provenance-блокер.
- `codex-recon-mvp-orch-teaching.md` — «одна пара за раз», lifecycle, continue-training.
- `codex-recon-mvp-external-sources.md` — лицензии/provenance/формат пар (legal).
- `codex-recon-core-26-router.md` — детерминированный роутинг (masquerade-риск).

### Реальные файлы кода (с путями)
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs` — DPO-математика (margin/loss/accuracy), `StepVector`.
- `C:/Projects/llm_mvp_core/11.DPO/Models/DpoPair.cs` — контракт пары Prompt/Chosen/Rejected.
- `C:/Projects/llm_mvp_core/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs` — гейт дельты (`:137-140`); пустой DPO-канал (`:120`).
- `C:/Projects/llm_mvp_core/16.AutonomousLearning/Training/AutonomousTrainer.cs` — проброс `dpoPairs` (`:44-47,61`).
- `C:/Projects/llm_mvp_core/16.AutonomousLearning/Training/ReplayBuffer.cs` — anti-forgetting 70/30.
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` — авто-сборка ряда (`:167-175`); chosen/rejected шаблоны (`:173-174,680-683`).
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs` — `LogicalDpoTrainingUnit` (`:308-415`); KNN-юнит (`:31-171`); `LogicalDpoPreferenceExample` (`:281`).
- `C:/Projects/llm_mvp_core/26.TrainableRouter/Training/KeywordUnitScorer.cs` — детерминированный scorer (FORBIDDEN_RUNTIME_SHORTCUT).
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — иерархия и DPO-records (`:18-39,43-62,208-234`).

### Скилы/гайды (правила)
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md` — уровни, Legal Teacher Gate, Graph-Style Reconstruction Gate, вердикты.
- `C:/Projects/llm_mvp_core/docs/skills/dpo-triple-format-SKILL.md` — формат триплета, set-quarantine, permutation-инвариантность.
- `C:/Projects/llm_mvp_core/docs/skills/planning-dpo-authoring-SKILL.md` — GENERATED vs REINFORCEMENT, outcome attribution, source curation.
- `C:/Projects/llm_mvp_core/docs/skills/codex-planning-quality-GUIDE.md` — architecture-question-first; chosen/rejected ownership.
- Связанные гайды плана: гайд 01 (legal teacher), гайд 03 (измерение), гайд 04 (async-мульти-комбинация).
