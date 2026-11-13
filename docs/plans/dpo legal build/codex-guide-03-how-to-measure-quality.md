# Гайд 03 — Как мерить скорость, точность и качество обучения

Часть ПЛАНА проекта `ILearningDynamicGraph` (авто-построение DPO для
`ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator`/`ILearningDynamicGraph`,
маппинг на реальную иерархию `ILearningMachine`/`ILogical*`). Это документ-гайд,
не разрешение на реализацию. Все цитаты кода — реально существующие, с `file:line`.

Автор: Opus 4.8 MAX (роль — автор гайда измерения). Язык — русский, человеческим
языком сначала, код потом.

---

## 1. Назначение гайда

Этот гайд отвечает ровно на один вопрос: **как доказать числом, что узел графа
(`ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator`) реально обучился, а не
притворяется.** Он даёт три оси измерения и две «честности», чтобы план
`ILearningDynamicGraph` не принял заглушку за обученную фичу.

Три оси:

- **СКОРОСТЬ** — сколько стоило обучение: `CpuMs`, `RamMb`/`PeakRamMb`, `elapsed_ms`,
  `train_rows`. Это «дорого/дёшево», а не «правильно».
- **ТОЧНОСТЬ** — насколько модель права. Здесь главный подвох: **live-accuracy
  (доля пар с `margin>0` на обучающем потоке) — это НЕ точность.** Точность узла
  графа доказывается только на **held-out** (отложенных, не виденных при
  обучении) строках.
- **КАЧЕСТВО** — это ли обучение, а не утечка/память: **novelty-gate** (учим новое,
  а не зубрим), **ablation** (без обученного состояния узел обязан быть
  `not_ready`), **permutation-invariance** (решение не должно зависеть от порядка
  кандидатов или id источника).

Две честности (две разные легитимные «зелёные» оценки, которые НЕЛЬЗЯ путать):

- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — скелет честно говорит «я не обучен»;
- `PASS_AS_TRAINED_FEATURE` — обученная фича с полным комплектом доказательств
  (3 гейта: held-out числа + ablation + lifecycle export/import/fresh-load/
  continue-training через реальную точку входа).

Гайд опирается на уже работающие в репозитории образцы измерения
(`27.TranslationDomain`, `11.DPO`, тесты обучаемых машин в `core/Tests`) и на
правила скилов (`trainable-architecture-decision-SKILL.md`,
`phase-completion-packager-SKILL.md`, `capability-measurement-SKILL.md`).

---

## 2. Концепт простыми словами (аналогия, без кода)

Представьте, что вы нанимаете и проверяете **переводчика**.

- **СКОРОСТЬ** — это сколько времени и сил он потратил на курс обучения: часы,
  усталость, сколько учебных карточек прошёл. Важно для бюджета, но быстро
  выученный курс не значит, что человек переводит хорошо.

- **ТОЧНОСТЬ на тренировке vs точность на экзамене.** Пока вы учите переводчика
  на конкретных карточках, легко добиться, чтобы на ЭТИХ карточках он отвечал
  лучше «себя вчерашнего». Это и есть live-accuracy: «на учебных парах
  правильный вариант теперь нравится модели больше неправильного». Но это ничего
  не говорит о незнакомом тексте. Настоящий экзамен — **новые** фразы, которых не
  было в учёбе (held-out). Только провал/успех на новом тексте — это «точность».

- **КАЧЕСТВО = доказать, что он именно научился, а не схитрил.** Три ловушки:
  1. **Зубрёжка вместо понимания** (novelty-gate): если он просто заучил ответы и
     не видит новых слов, его нельзя «повышать». Учить надо новое.
  2. **Тайная шпаргалка** (ablation): отнимите у него выученное состояние — он
     обязан честно сказать «я ещё не готов». Если без обучения он всё равно
     «отвечает» — значит, отвечал не он, а спрятанная детерминированная шпаргалка
     (regex/словарь/точный lookup).
  3. **Подсматривание разметки** (permutation/leakage): если он угадывает по
     номеру варианта в списке, по имени файла или по id источника — это не
     перевод, это списывание. Поменяйте порядок вариантов: правильный ответ не
     должен «переехать» только из-за перестановки.

- **Две честности.** Есть разница между «честный стажёр, который прямо говорит: я
  ещё не учился» (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — это нормальная веха) и
  «дипломированный специалист, сдавший экзамен на новом тексте, чьи навыки можно
  сохранить, перенести и продолжить обучать» (`PASS_AS_TRAINED_FEATURE`).
  Выдавать стажёра за специалиста — самый опасный обман
  (`two_pass_conflation_untrained_reported_as_trained`,  `-1000`).

В графе `ILearningDynamicGraph` каждый узел/ребро — это такой «переводчик». Гайд
говорит, какими числами и проверками сдавать экзамен на каждом узле.

---

## 3. Как это сейчас реализовано в нашем коде (что РАБОТАЕТ, что ЗАГЛУШКА)

Ниже — реальные образцы измерения, уже присутствующие в репозитории. Это фундамент,
на который план графа должен опереться (а не изобретать заново).

### 3.1 СКОРОСТЬ — РАБОТАЕТ (ресурсные метрики уже считаются)

Раннер продуктового held-out для перевода пишет ресурсные числа прямо в отчёт:

- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:239-241`
  — `CpuMs: trainResult.Value.CpuMs`, `PeakRamMb: trainResult.Value.PeakRamMb`,
  `ElapsedMs: Environment.TickCount64 - startMs`.
- Там же `TrainingRows: trainRows.Length` (строка ~226) — это `train_rows`.

DPO-раннер `InternalTrainReadyDpoRunner` (по рекону
`codex-recon-mvp-experiments.md:70-80`) на каждый батч пишет `CpuPercent`,
`WorkingSetDeltaMb`, `ElapsedMs`, `CheckpointBytesTotal`, а в summary —
`TotalElapsedMs`. Teaching-раннер (по рекону `codex-recon-mvp-teaching-ledger.md:37-39`)
сравнивает дешёвый baseline (`elapsed_ms` ~40-76 мс) против обучения
(`teaching_elapsed_ms` ~2.5-14.7 тыс. мс), плюс `process_cpu_percent`,
`working_set_delta_mb`, `disk_delta_mb`, `checkpoint_bytes`.

**Вывод:** ось «скорость» уже имеет рабочий контракт полей. Заглушки тут нет;
есть фрагментация (разные раннеры пишут поля чуть по-разному) — план должен
свести их в единый контракт измерения узла.

### 3.2 ТОЧНОСТЬ — ДВА РАЗНЫХ ЧИСЛА, оба РАБОТАЮТ, но значат разное

**(а) Live reward-margin accuracy (НЕ held-out, осторожно).** Низкоуровневый
DPO-тренер определяет accuracy так:

- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:64` —
  `Accuracy = margin > 0 ? 1f : 0f;` (для одного батча);
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:120` —
  `Accuracy = accuracySum / count` (усреднение по батчам).

То есть «accuracy» раннера = **доля пар, где chosen ранжируется выше rejected** на
ТЕКУЩЕМ потоке обучения. Живые значения v196 (рекон
`codex-recon-mvp-experiments.md:82-87`): `AverageAccuracy≈0.6228`,
`FinalAccuracy≈0.5833`, `AverageRewardMargin≈0.01217`. Это рабочий сигнал «учится
ли вообще», но **это не семантическая точность узла графа**.

**(б) Held-out продуктовая точность (это и есть «точность» для приёмки).** Раннер
перевода считает метрики на отложенных кейсах и блокирует контаминацию:

- `TranslationProductHeldOutRunner.cs:285-287` — проверка пересечения
  train↔held-out: `BuildTrainHeldOutOverlapError(...)` → ошибка
  `TRAIN_HELDOUT_OVERLAP` ДО объявления accuracy. Это ключ: точность объявляется
  только если обучение и экзамен не пересеклись.
- `TranslationProductHeldOutRunner.cs:369-387` — реальные числа на held-out:
  `RuntimeExactAccuracy`, `RuntimeProductAccuracy` (составной: не bootstrap, все
  required-маркеры, нет forbidden, верный скрипт, не копия источника, эмодзи и
  именованные сущности сохранены), `RuntimeBootstrapRate`, `TargetScriptAccuracy`,
  `Bleu1`, `ChrF1`.

**Вывод:** оба числа РАБОТАЮТ, но план обязан их разделять. См. §4 шаг 2 и §6
«Гейт измерения 1».

### 3.3 КАЧЕСТВО — РАБОТАЕТ (gate, novelty, ablation, leakage уже есть)

**Промоушен-гейт перевода (числовые пороги — реальные).**

- `TranslationProductHeldOutRunner.cs:211-220` и зеркально `:389-398`:
  ```
  AccuracyAfter >= AccuracyBefore + 0.02   // обучение должно поднять минимум на 2 п.п.
  && RewardMargin >= 0.025
  && RuntimeProductAccuracy >= 0.95
  && RuntimeBootstrapRate <= 0.15
  && TargetScriptAccuracy >= 1.0
  && CopiedSourceRate <= 0.0
  && WrongDirectionRate <= 0.0
  && EmojiPreservationAccuracy >= 1.0
  && NamedEntityPreservationAccuracy >= 1.0
  ```
- `:244-247` — `PassesGate` + `GateNote` («...gate failed; do not promote this
  adapter.»). То есть «не прошёл гейт = не повышать».

**Novelty-gate (строгий, из CLAUDE.md) — РАБОТАЕТ.** По рекону
`codex-recon-core-27-translation.md:27`, `TatoebaPilotLoopRunner.cs:777`
вычисляет `promotionDecision` с порогами от `maxAccuracy`:
`MinimumNewPatternRate = 1 - maxAccuracy`, `MaximumSeenTokenRate = maxAccuracy`.
Решения вида `PROMOTE_NEW_PATTERN_GT_{X}_AND_SEEN_TOKENS_LT_{Y}` или
`SKIP_SHORTFALL`. Формула совпадает с правилом CLAUDE.md: novelty-gate строгий
`newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy`.

**Ablation / анти-leakage — РАБОТАЕТ (на уровне машины).**

- `C:/Projects/llm_mvp_core/Tests/Learn06HeldOutAblationRunnerTests.cs:42` —
  трек OrchestrationReasoning должен дать вердикт
  `PASS_NAMED_CONTROLS_NOT_LEAKAGE`. По рекону `codex-recon-core-tests.md:25`
  пять именованных контролей: `exact_prompt_replay`,
  `path_title_source_id_leakage`, `nearest_neighbor_memorization`,
  `no_trained_state_baseline`, `shuffled_chosen_rejected`. Контроль
  `no_trained_state_baseline` сравнивает загруженную модель с
  `new OrchestrationReasoningMachine()` — прямая проверка, что результат от
  обученного state, а не от детерминированного fallback.
- Skeleton-тест честного НЕобученного (ablation «нет состояния → not_ready»):
  `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:18-20` —
  пустая машина возвращает `ContractStatus == NotTrainedStatus`,
  `TaskType == NotReadyTaskType`, evidence содержит `NO_TRAINED_STATE_LOADED`.
- Запрет скрытого детерминизма (рефлексией):
  `PromptToTaskMachineSkeletonTests.cs:79-85` — у типа НЕТ полей `Regex`/
  `Dictionary` и членов `Classifier`/`Trainer`/`Lookup`/`Bm25`/`ExactPrompt`.

**Permutation-invariance — РАБОТАЕТ (образец).** По рекону
`codex-recon-core-tests.md:23`, `SkillSelectionPhase06TrainedSelectorEvidenceTests`
содержит `EvaluateCandidateOrderLeakage` (реверс `AvailableSkillIds`), запрет
`CandidatePositionScaffoldEvidence` и негативный контроль `SelectedFromSourceIdOnly`
обязан быть `false`.

**Lifecycle обученной машины — РАБОТАЕТ (эталон приёмки).** Тот же
`SkillSelectionPhase06...` (рекон `codex-recon-core-tests.md:23`): строит
`LearningBatch<SkillSelectionDomain>` + source-disjoint `LearningEvaluationSet`,
гоняет `EvaluateAsync` на ПУСТОЙ (baseline) и обученной машине, требует
`PassesGate`/`Accuracy`/`RewardMargin`, затем `SaveCheckpointAsync` →
`ImportAsync` → `LoadCheckpointAsync` (fresh-load) и проверяет
`freshEval.Accuracy == evalResult.Accuracy` (загрузка без переобучения).

**ЗАГЛУШКИ / СЛАБЫЕ МЕСТА (план обязан закрыть):**

- **Авто-построение DPO (главная цель плана) — ЗАГЛУШКА/отсутствует.** Прочитанные
  тесты ПОТРЕБЛЯЮТ готовые DPO-строки; теста-ГЕНЕРАТОРА DPO-троек не найдено
  (рекон `codex-recon-core-tests.md:50`). Это ядро нового плана.
- **DynamicGraph как граф — отсутствует среди измеренного.** Нет теста multi-node/
  рёбер/fan-in/fan-out/many-to-many/async-исполнения (рекон
  `codex-recon-core-tests.md:49`). Все ablation-контроли реализованы на уровне
  машины, не на ребре графа — их надо тиражировать **per-edge**.
- **`RewardMargin` в переводе фиксированный (0.03/0.0)** — это метка наличия
  `[WRONG:`-примера, а не вычисленный обучающий сигнал (рекон
  `codex-recon-core-27-translation.md:22,49`). Для узла графа, заявленного как
  «обученный», нужен реальный margin (как в `11.DPO/DpoTrainer.cs`).
- **Точное запоминание источника (`_exactSourceMap`, conf 0.93; `RuntimeExactAccuracy`)**
  — кандидат на leakage/replay (рекон `codex-recon-core-27-translation.md:43,50`).
  Перед заявлением «обученный» по узлу — обязательна ablation на replay.
- **Continue-training поверх чекпойнта — флаг есть, реальное исполнение не
  подтверждено** (рекон `codex-recon-core-tests.md:52`:
  `ContinueTrainingFromCheckpointReady=true`, но повторный `TrainBatchAsync`
  поверх загруженного чекпойнта в прочитанном тесте не гоняется).
- **Legal-teacher provenance в измерении — отсутствует** (рекон
  `codex-recon-core-tests.md:51`, `codex-recon-mvp-train-ready.md:84-88`). См. §6.

---

## 4. Пошаговый рецепт/алгоритм измерения узла/ребра графа

Для каждого `ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator` в графе:

**Шаг 0 — Классифицировать уровень (до любых чисел).** По
`trainable-architecture-decision-SKILL.md` подтвердить, что измеряемое — один из
allowed-уровней (`ILearningMachine`/`ILogicalOrchestrator`/`ILogicalFlow`/
`ILogicalUnit`/`ILearningMachinePair`). KNN/scorer/counter/regex — НЕ уровень;
если такой механизм заявлен как «обученный авторитет» → `FORBIDDEN_RUNTIME_SHORTCUT`,
мерить нечего, сначала чинить архитектуру. Для графа на КАЖДОМ ребре назвать
владельца финального авторитета (Graph-Style Reconstruction Gate).

**Шаг 1 — Измерить честность НЕобученного (ablation-«нет состояния»).** На ПУСТОМ
узле вызвать инференс. Ожидать честный `not_ready`-ответ (status `NotTrained`,
evidence `NO_TRAINED_STATE_LOADED`/`NO_SEMANTIC_CLASSIFICATION_PERFORMED`) и
рефлексией доказать отсутствие `Regex`/`Dictionary`/`Classifier`/`Lookup`/`Bm25`/
`ExactPrompt`. Образец — `PromptToTaskMachineSkeletonTests.cs:18-20, 79-85`. Это
кандидат на `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

**Шаг 2 — Построить train/held-out СПЛИТ без пересечения.** Зарезервировать
held-out строки, пометить их `UsedForTraining=false`, обучить ТОЛЬКО на train.
Перед объявлением любой точности проверить контаминацию (образец:
`TranslationProductHeldOutRunner.cs:285-287`, ошибка `TRAIN_HELDOUT_OVERLAP`).
Источники train и held-out должны быть **source-disjoint** (разные
`SourcePromptEntryId`).

**Шаг 3 — Измерить ТОЧНОСТЬ двумя числами и не путать их.**
- Live: `Accuracy = доля(margin>0)` на обучении (`DpoTrainer.cs:64,120`) — только
  как «учится ли вообще».
- Held-out: реальная точность на отложенных строках
  (`TranslationProductHeldOutRunner.cs:369-387`). Это число идёт в приёмку.
- Зафиксировать дельту `AccuracyAfter - AccuracyBefore` (обученная минус пустая):
  без положительной дельты обучения не было.

**Шаг 4 — Измерить КАЧЕСТВО (это обучение, а не утечка).**
- **novelty-gate:** `newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy`
  (образец `TatoebaPilotLoopRunner.cs:777`). Иначе `SKIP_SHORTFALL` — не повышать.
- **ablation `no_trained_state_baseline`:** сравнить с `new ...Machine()`; обученный
  результат обязан отличаться от пустого (`Learn06...Tests.cs:42`).
- **leakage-контроли (per-edge):** `exact_prompt_replay`, `source_id`/`pair_id`/
  `file_path`/`candidate_position`. `SelectedFromSourceIdOnly` обязан быть `false`.
- **permutation-invariance:** реверс порядка кандидатов не меняет выбор
  (`EvaluateCandidateOrderLeakage`).

**Шаг 5 — Измерить СКОРОСТЬ.** Записать `CpuMs`, `PeakRamMb`, `ElapsedMs`,
`train_rows` (образец `TranslationProductHeldOutRunner.cs:239-241`). UNKNOWN в этих
полях запрещён для done-заявления (Runtime Evidence Firewall, см. §6).

**Шаг 6 — Доказать LIFECYCLE (для `PASS_AS_TRAINED_FEATURE`).** `SaveCheckpointAsync`
→ `ImportAsync` → `LoadCheckpointAsync` (fresh-load) → проверить
`freshEval == trainedEval` (загрузка без переобучения), затем `continue-training`:
повторный `TrainBatchAsync` поверх загруженного чекпойнта, без обучения с нуля.

**Шаг 7 — Выбрать честность и применить промоушен-гейт.** Если все три гейта (§6)
зелёные → `PASS_AS_TRAINED_FEATURE`. Если только шаг 1 → только
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. Любая попытка записать trained на скаффолд-баре
= масскарад (`-1000`).

---

## 5. Конкретные примеры с реальными значениями

### 5.1 DPO-пара chosen/rejected/[WRONG:] (формат перевода)

Реальная строка train-ready (рекон `codex-recon-core-27-translation.md:21`,
файл-семейство `27.TranslationDomain/TrainingData/*.jsonl`,
`ru-en-product-b001-001`):

```json
{
  "id": "ru-en-product-b001-001",
  "source": "...",
  "direction": "ru-en",
  "pair_family": "product_phrase_map",
  "prompt": "...",
  "a_side": "Фразовая карта: Привет; Я новый тут; чем занимаешься.",
  "chosen": "Phrase map: Hey!; I'm new here; what are you up to? 😊",
  "rejected": "... [WRONG: product_phrase_boundary_lost - drops casual chat tone or emoji - consequence: ответ звучит формально и теряет смысл]",
  "validation_outcome": "train_ready"
}
```

Чтение маркера: `[WRONG: <слабость> - <в чём дефект> - <последствие>]`. Это явный
контраст «правильно vs неправильно», на котором учится DPO-узел. Запрещённые
основания контраста (Set Quarantine, см. §6): разница chosen/rejected = id
источника, pair id, имя файла/путь, порядок кандидатов, точная память промпта,
keyword→skill lookup.

### 5.2 Живые числа DPO-обучения (v196, реальные)

Рекон `codex-recon-mvp-experiments.md:82-87`:

```
PairCount=3916, BatchSize=16, BatchCount=245, ContextLen=96, VocabSize=2048
AverageLoss≈0.6873   FinalLoss≈0.6882
AverageAccuracy≈0.6228   FinalAccuracy≈0.5833      // live margin>0, НЕ held-out
AverageRewardMargin≈0.01217   FinalRewardMargin≈0.01013
Шаг 1: Loss 0.6837, ChosenReward 0.01533, RejectedReward -0.00401, RewardMargin 0.01934, Accuracy 0.75
```

Как это читать в гайде: `AverageAccuracy 0.62` — это «на 62% обучающих пар chosen
уже выше rejected». Это НЕ «модель переводит на 62% правильно». Для приёмки нужно
держать held-out отчёт отдельно.

### 5.3 Реальный промоушен-гейт (перевод, held-out) — пример прохода/провала

Пороги `TranslationProductHeldOutRunner.cs:211-220`. Пример:

```
AccuracyBefore = 0.40, AccuracyAfter = 0.44   -> +0.04 >= +0.02  OK
RewardMargin = 0.03                            -> >= 0.025        OK
RuntimeProductAccuracy = 0.96                  -> >= 0.95         OK
RuntimeBootstrapRate = 0.10                    -> <= 0.15         OK
TargetScriptAccuracy = 1.0, Copied = 0, WrongDirection = 0, Emoji = 1.0, NamedEntity = 1.0
=> PassesGate = true,  GateNote = "RU_EN product held-out gate passed."

Контр-пример: RuntimeProductAccuracy = 0.93  -> 0.93 < 0.95  =>  PassesGate = false
GateNote = "RU_EN product held-out gate failed; do not promote this adapter."
```

### 5.4 Novelty-gate (пример решения)

`promotionDecision` (`TatoebaPilotLoopRunner.cs:777`), при `maxAccuracy = 0.85`:
`MinimumNewPatternRate = 1 - 0.85 = 0.15`, `MaximumSeenTokenRate = 0.85`.

```
newPattern = 0.20 (> 0.15)  AND  seenTokens = 0.70 (< 0.85)
=> PROMOTE_NEW_PATTERN_GT_0.15_AND_SEEN_TOKENS_LT_0.85   (учим новое — повышаем)

newPattern = 0.05 (< 0.15)  =>  SKIP_SHORTFALL           (зубрёжка — не повышать)
```

### 5.5 Ablation-контроли узла (вердикты, реальные)

`Learn06HeldOutAblationRunnerTests.cs:42-44`: трек машины должен дать
`PASS_NAMED_CONTROLS_NOT_LEAKAGE`; overlay-треки честно помечены
`PARTIAL_NOT_FULL_MACHINE_EVAL` (не выдают себя за полный held-out машины). Это
образец «честного ограничения объёма», который план должен повторять на узлах
графа.

---

## 6. Гейты и правила (что применимо к измерению)

### Гейт измерения 1 — НЕ ПУТАТЬ live-accuracy и held-out

`Accuracy = доля(margin>0)` (`DpoTrainer.cs:64,120`) — это live reward-margin на
обучающем потоке. Запрещено выдавать её за «семантическую точность узла». Точность
для приёмки = held-out, source-disjoint, после проверки контаминации
(`TRAIN_HELDOUT_OVERLAP`, `TranslationProductHeldOutRunner.cs:285-287`). Это прямо
зафиксировано в DPO-парах качества (рекон `codex-recon-mvp-train-ready.md:62-67`:
вариант «считать batch accuracy финальной классификацией» помечен `[WRONG]`).

### Гейт измерения 2 — Runtime Evidence Firewall (анти-стаб)

Из `phase-completion-packager-SKILL.md:82-85`. Отвергать как доказательство
обучения: `validation_only: true`; `metric_type` со значением `static`/`contract`/
`mixed`; `cpu_ms`/`ram_mb`/`accuracy` = `UNKNOWN`. Build/contract/DNA проходят и на
json-заглушке — поэтому их НЕДОСТАТОЧНО для trained-узла. Нужны воспроизводимые
числовые runtime-метрики, и узел обязан **менять решение после обучения на новых
парах**, а не реплеить точную память.

### Гейт измерения 3 — Two-PASS (OP-57), 3 гейта для trained

Из `phase-completion-packager-SKILL.md:96-114`:

- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — разрешён, когда скелет/контракт честно
  `not_ready`, нет семантик-claim, нет скрытого детерминизма, нет фейковых
  DPO/checkpoint/metrics (тесты-честности `PromptToTaskMachineSkeletonTests.cs`).
- `PASS_AS_TRAINED_FEATURE` — разрешён ТОЛЬКО при полном комплекте: **(1)** held-out
  числовые метрики + **(2)** ablation (без обученного состояния → not_ready;
  `no_trained_state_baseline`) + **(3)** lifecycle: fresh-load probe +
  continue-training probe + e2e через реальную точку входа.

Смешение (trained на скаффолд-баре) = `two_pass_conflation_untrained_reported_as_trained`,
`-1000`.

### Гейт измерения 4 — Анти-leakage / permutation-invariance (per-edge)

На каждом ребре графа запретить решающие сигналы: `candidate_position`,
`source_id`, `pair_id`, `file_path`, `exact_prompt_replay`. Контроль
`SelectedFromSourceIdOnly` обязан быть `false`; реверс порядка кандидатов не меняет
выбор (`EvaluateCandidateOrderLeakage`, рекон `codex-recon-core-tests.md:23`).
Набор именованных контролей-эталон: `exact_prompt_replay`,
`path_title_source_id_leakage`, `nearest_neighbor_memorization`,
`no_trained_state_baseline`, `shuffled_chosen_rejected`
(`Learn06HeldOutAblationRunnerTests.cs:42` + рекон `codex-recon-core-tests.md:25`).

### Гейт измерения 5 — Запрет скрытого детерминизма (что вообще можно мерить)

`trainable-architecture-decision-SKILL.md`: KNN/BM25/scorer/selector/router/
counter/regex — НЕ архитектурный уровень; допустимы только как помеченная
internal-деталь/baseline/eval-only внутри allowed-уровня. Если такой механизм —
немаркированный финальный авторитет ребра → `FORBIDDEN_RUNTIME_SHORTCUT` (мерить
«обученность» нельзя, сначала чинить). Прямой code-эталон отсутствия скрытого
детерминизма — рефлексия в `PromptToTaskMachineSkeletonTests.cs:79-85`. Принцип
E51B: «KNN can choose memory. Arbiters decide acceptance» (рекон
`codex-recon-core-docs-exp.md:104-107`) — память не есть авторитет.

### Гейт измерения 6 — Легальный teacher / provenance (источник chosen/rejected)

`trainable-architecture-decision-SKILL.md` (Legal Teacher Gate) + триггер
HOW-TO-USE-SKILLS.md:330. Если `chosen`/`rejected` сгенерированы внешней моделью,
обязательны поля: `teacher_access_mode`, `hosting_provider`, `model_id`,
`model_license`, `provider_terms_checked`, `teacher_output_used_for_training`,
`closed_proprietary_model`. **Claude/OpenAI/Gemini как distillation-учители
запрещены.** Если `closed_proprietary_model=true` и
`teacher_output_used_for_training=true` → всегда `DATASET_REBUILD_REQUIRED`.
Текущее состояние данных — пробел: в teaching-ledger и supplement этих полей нет
(рекон `codex-recon-mvp-teaching-ledger.md:63`,
`codex-recon-mvp-train-ready.md:84-88`); строки помечены `accepted_model_led_*` —
надо установить, какая модель-учитель, до общего обучения.

### Гейт измерения 7 — Контракт результата эксперимента (delta-порог 5%)

`docs/EXPERIMENT-RESULTS-MAP.md` + схема
`docs/plans/test hypothesys/small experiment/experiment-result-schema-v1.json`
(рекон `codex-recon-core-docs-exp.md:22-25,43-48`): один эксперимент = baseline vs
improved, 5 групп метрик (`prediction_error`/`time_ms`/`memory_peak_mb`/
`cpu_avg_pct`/`disk_write_bytes`), `verdict.passed=true` только при улучшении
**>=5%**; `next_action` enum: `PROMOTE_TO_NEXT_TIER`/`TUNE_AND_RETRY`/
`ABANDON_APPROACH`/`NEEDS_HUMAN_REVIEW`.

### Гейт измерения 8 — SCALE_UP, а не «провал» (анти-ложный-вывод)

`experiment-design-SKILL.md` + MAP: `SCALE_UP` = «нужно больше данных, алгоритм
корректен» (E14/E20: данные слишком тривиальны/разделимы на Tier 3;
`EXPERIMENT-RESULTS-MAP.md:44-45`). FAILED на Tier 2 ≠ гипотеза неверна → эскалация
на Tier 3 до финального вердикта. Критические находки измерения:
`StratifiedSplit` обязателен при фиксированном seed (`E40`,
`EXPERIMENT-RESULTS-MAP.md:72`); забывание при неверной `max_weight`-константе
(`E44`, `:71`).

### Гейт измерения 9 — Capability score только с runtime-командой

`capability-measurement-SKILL.md`: шкала 0/25/50/75/100%; «Do not report a score
without the runtime command that proves it». Любой score >=75% обязан быть спарен с
командой раннера/теста, которая его воспроизводит.

### Правило читаемости (Human gate)

Любой статус измерения для Luba — на человеческом русском: что меряли, какое число
вышло, прошёл ли порог, что это доказывает и что НЕТ
(`phase-completion-packager-SKILL.md`, секция Human-Readable). Технические метки
(`PASS_NAMED_CONTROLS_NOT_LEAKAGE`, `SKIP_SHORTFALL`) переводить тут же.

---

## 7. Пробелы / что нужно построить в плане

1. **Единый контракт измерения узла графа.** Свести разрозненные поля
   (`DpoTrainer` метрики; `TranslationProductHeldOutRunner` held-out числа;
   teaching-ledger ресурсы) в один типизированный record «MeasurementReport узла»
   с тремя осями (скорость/точность/качество) и обязательным разделением
   live-accuracy vs held-out.
2. **Per-edge ablation/leakage.** Сейчас контроли (`no_trained_state_baseline`,
   permutation, source-id) живут на уровне машины. План обязан тиражировать их на
   КАЖДОЕ ребро графа (Graph-Style Reconstruction Gate:
   `deterministic_vs_trainable_authority_on_edges`,
   `hidden_deterministic_shortcut_check_tied_to_each_edge`).
3. **Реальный held-out + ablation раннер для каждого уровня** (Unit/Flow/
   Orchestrator/Graph), а не только для машины перевода/skill-selection.
   `Learn06...` сам признаёт `PARTIAL_NOT_FULL_MACHINE_EVAL` для PromptToTask и
   SkillSelection (рекон `codex-recon-core-tests.md:24`).
4. **Вычисленный RewardMargin для не-трансформерных узлов.** Фиксированный
   `RewardMargin=0.03/0.0` в переводе — метка, не сигнал. Узел, заявленный
   «обученным», должен давать реальный margin (как `11.DPO/DpoTrainer.cs`).
5. **Continue-training probe реально исполнить** (повторный `TrainBatchAsync`
   поверх загруженного чекпойнта), а не только флаг
   `ContinueTrainingFromCheckpointReady=true`.
6. **Анти-replay для exact-lookup узлов** (`_exactSourceMap`, `RuntimeExactAccuracy`):
   ablation, доказывающая, что точное совпадение — не запоминание источника.
7. **Teacher/source-provenance измерять и хранить** (поля Legal Teacher Gate) до
   любого общего обучения; до этого `chosen/rejected` модельного происхождения —
   evidence/user-adaptive, не common-training.
8. **Авто-генератор DPO-троек с встроенными гейтами измерения** — главного
   артефакта (генератора) сейчас нет; его выход обязан сразу проходить Set
   Quarantine и leakage-контроли.
9. **Воспроизводимость:** часть раннеров пишет в локальный пользовательский путь
   (`C:\Users\lubao\.xiigen\...`, см. `Learn06HeldOutAblationRunnerTests.cs:30-33`).
   План измерения должен задать переносимые пути/фикстуры.

---

## 8. Ссылки

### Recon-сводки (evidence)

- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-tests.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-docs-exp.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-27-translation.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-teaching-ledger.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-experiments.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-train-ready.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-docs-skills-catalog.md`

### Реальные файлы кода (с опорными строками)

- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:64` — `Accuracy = margin > 0 ? 1f : 0f` (live, один батч).
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:120` — `Accuracy = accuracySum / count` (live, усреднение).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:211-220` — промоушен-гейт (числовые пороги, RunDirection).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:389-398` — тот же гейт (RunAllDirections).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:285-287` — контаминация `TRAIN_HELDOUT_OVERLAP`.
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:369-387` — held-out метрики (Exact/Product/Bootstrap/Script/Bleu1/ChrF1).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:239-241,244-247` — CpuMs/PeakRamMb/ElapsedMs, PassesGate/GateNote.
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TatoebaPilotLoopRunner.cs:777` — promotionDecision + novelty-gate (рекон `codex-recon-core-27-translation.md:27`).
- `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:18-20` — честный not_ready (NotTrainedStatus/NO_TRAINED_STATE_LOADED).
- `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:79-85` — запрет скрытого детерминизма (рефлексия: нет Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt).
- `C:/Projects/llm_mvp_core/Tests/Learn06HeldOutAblationRunnerTests.cs:42-44` — вердикты `PASS_NAMED_CONTROLS_NOT_LEAKAGE` / `PARTIAL_NOT_FULL_MACHINE_EVAL`.
- `C:/Projects/llm_mvp_core/Tests/SkillSelectionPhase06TrainedSelectorEvidenceTests.cs` — эталон lifecycle обученной машины (train→eval baseline vs trained→export/import→fresh-load→leakage-ablation) (рекон `codex-recon-core-tests.md:23`).

### Скилы / реестры (правила)

- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md` — allowed-уровни, Graph-Style Reconstruction Gate, Legal Teacher / Provenance Gate, вердикты.
- `C:/Projects/llm_mvp_core/docs/skills/phase-completion-packager-SKILL.md:82-85` — Runtime Evidence Firewall (анти-стаб).
- `C:/Projects/llm_mvp_core/docs/skills/phase-completion-packager-SKILL.md:96-114` — Two-PASS gate (scaffold vs trained, 3 гейта).
- `C:/Projects/llm_mvp_core/docs/skills/capability-measurement-SKILL.md` — 6 классов, шкала 0-100%, runtime-команда обязательна.
- `C:/Projects/llm_mvp_core/docs/EXPERIMENT-RESULTS-MAP.md` — результаты E01-E50, SCALE_UP, key findings (E40 StratifiedSplit, E44 max_weight).
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md:330` — триггер DPO/distillation teacher provenance.
- `C:/Projects/llm_mvp_core/docs/plans/test hypothesys/small experiment/experiment-result-schema-v1.json` — схема результата эксперимента, delta>=5%, next_action enum (рекон `codex-recon-core-docs-exp.md:22-25`).
