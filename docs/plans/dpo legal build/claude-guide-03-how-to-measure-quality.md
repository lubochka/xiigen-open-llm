# Гайд 03 — Как мерить скорость, точность и качество обучения

Часть ПЛАНА проекта `ILearningDynamicGraph` (авто-построение DPO для
`ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator`/`ILearningDynamicGraph`,
маппинг на реальную иерархию `ILearningMachine`/`ILogical*`). Это документ-гайд,
**не разрешение на реализацию**. Все цитаты кода — реально существующие, проверены
по `file:line` в `C:/Projects/llm_mvp_core` на дату ревизии (см. §8, «как проверены
цитаты»).

Автор: Opus 4.8 MAX (роль — автор гайда измерения, namespace `claude-*`). Язык —
русский, человеческим языком сначала, код потом (Part A-формат).

> **Канон и счётчики (общие со всеми `claude-*`).** Этот гайд НЕ задаёт свой канон —
> он только **читает** канон-типы для нужд измерения. Владельцы общих типов:
> `GraphDpoPair` (14 полей) принадлежит блоку 07 (`claude-plan-07`), `TeacherProvenance`
> (20 полей) принадлежит `claude-plan-08` §3.1; единый отчёт измерения `MeasurementReport` (+ `SpeedAxis`/`AccuracyAxis`/
> `QualityAxis`/`SplitProof`) и раннер `IGraphMeasurementRunner` вводит блок
> `claude-plan-11` (measurement). `GraphEdgeKind` — 5 членов. Счётчики каркаса
> исполнения — **ровно 12 фаз / 26 подфаз / 59 подшагов**, нумерация `Ф.ПП.СС`
> (например `Ф5.2.3`). Полная карта фаз — в `claude-plan-11-measurement.md §0-pre`;
> этот гайд даёт человеческое объяснение, к каким фазам относится каждый шаг §4.

> **CANON-инварианты, которые гайд обязан сохранять.**
> 1. `ILearningDynamicGraph = ILearningMachine<DynamicGraphDomain>` — это L5, **без
>    6-го уровня** (A-LM-12). Никакой «уровень над машиной» не вводится.
> 2. Легальный teacher-гейт: closed-proprietary модель допустима **только как judge/
>    eval-критик**, НЕ как teacher для дистилляции. Контаминация train closed-выходом
>    → `DATASET_REBUILD_REQUIRED` (§6, гейт 6).
> 3. Нет скрытого детерминизма (A-LM-09/A-LM-13): прежде чем мерить «обученность»,
>    доказать, что это не replay/lookup (§6, гейт 5).

---

## 1. Назначение гайда

Этот гайд отвечает ровно на один вопрос: **как доказать числом, что узел графа
(`ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator`) реально обучился, а не
притворяется.** Он даёт три оси измерения и две «честности», чтобы план
`ILearningDynamicGraph` не принял заглушку за обученную фичу.

Три оси:

- **СКОРОСТЬ** — сколько стоило обучение: `CpuMs`, `PeakRamMb`, `ElapsedMs`,
  `train_rows`. Это «дорого/дёшево», а не «правильно». Идёт в `SpeedAxis`.
- **ТОЧНОСТЬ** — насколько модель права. Здесь главный подвох: **live-accuracy
  (доля пар с `margin>0` на обучающем потоке) — это НЕ точность.** Точность узла
  графа доказывается только на **held-out** (отложенных, не виденных при обучении)
  строках. Идёт в `AccuracyAxis` (раздельно `LiveMarginAccuracy` и `HeldOutAccuracy`).
- **КАЧЕСТВО** — это ли обучение, а не утечка/память: **novelty-gate** (учим новое,
  а не зубрим), **ablation** (без обученного состояния узел обязан быть `not_ready`),
  **permutation-invariance** (решение не зависит от порядка кандидатов или id
  источника). Идёт в `QualityAxis`.

Две честности (две разные легитимные «зелёные» оценки, которые НЕЛЬЗЯ путать):

- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — скелет честно говорит «я не обучен». Это
  **веха, не рабочая фича**.
- `PASS_AS_TRAINED_FEATURE` — обученная фича с полным комплектом доказательств
  (3 гейта: held-out числа + ablation + lifecycle export/import/fresh-load/
  continue-training через реальную точку входа).

Гайд опирается на уже работающие в репозитории образцы измерения
(`27.TranslationDomain`, `11.DPO`, тесты обучаемых машин в `Tests/`) и на правила
скилов (`trainable-architecture-decision-SKILL.md`,
`phase-completion-packager-SKILL.md`, `capability-measurement-SKILL.md`,
`plan-review-SKILL.md`).

### Карта «шаг рецепта → ось → локальный шаг блока» (быстрый ориентир)

> **Дисклеймер канон-фаз (синхрон с `claude-plan-11 §0-pre`, строки 92–98).** Глобально
> **весь** блок измерения — это **одна** фаза доски **Ф5 «Измерение»** (канон доски — ровно
> Ф0…Ф11). Колонка ниже даёт **локальный** разбор этого блока шагами `Lk` (L = local block
> step), это НЕ канон-фазы доски. ID вида `Ф12.*.*` намеренно не используется, чтобы не
> выходить за канон Ф0…Ф11 (как в `claude-plan-11` строки 96–98: `Фk ≡ Lk` локально).

| Шаг §4 | Ось/тема | Локальный шаг блока 11 (Lk; вся фаза доски = Ф5 «Измерение») | Главная цитата-эталон |
|---|---|---|---|
| Шаг 0 | классификация уровня | L2 | `trainable-architecture-decision-SKILL.md` |
| Шаг 1 | честность НЕобученного | L3 | `PromptToTaskMachineSkeletonTests.cs:18-22, 79-85` |
| Шаг 2 | сплит без пересечения | L4 | `TranslationProductHeldOutRunner.cs:285-287` |
| Шаг 3 | точность двумя числами + loss-сходимость | L5 | `DpoTrainer.cs:60,116` (loss) и `:64,120` (live) vs `:369-374` |
| Шаг 4 | качество (не утечка) | L6 | `TatoebaPilotLoopRunner.cs:777`, `Learn06...:42` |
| Шаг 5 | скорость (ресурсы) | L7 | `TranslationProductHeldOutRunner.cs:239-241` |
| Шаг 6 | lifecycle | L8 | `SkillSelectionPhase06TrainedSelectorEvidenceTests.cs` |
| Шаг 7 | честность + промоушен-гейт | L9–L12 | `:211-220`, Two-PASS |

---

## §0. Схема измерения (UML, importable to draw.io)

> Это объяснительные диаграммы гайда, не финальный код. Классы-канон (`MeasurementReport`
> и оси) вводит блок 11; провенанс (`TeacherProvenance`) — read-only канон блока 08.

### §0.1 Поток решения «scaffold / trained / rebuild / shortcut» (flowchart)

```mermaid
flowchart TD
    A["Узел/ребро графа\n(ILearningUnit/Flow/Orchestrator)"] --> B{"Это allowed-уровень?\n(L5/L4/L3/L2/pair, A-LM-12)"}
    B -->|"НЕТ\nнемаркированный KNN/scorer/regex как финал"| SHORT["🚫 FORBIDDEN_RUNTIME_SHORTCUT\nмерить нечего → чинить архитектуру"]
    B -->|"ДА"| C{"Пустой узел честно not_ready?\n(ablation «нет состояния»)"}
    C -->|"НЕТ\nотвечает без обучения"| SHORT
    C -->|"ДА, и рефлексией нет Regex/Dictionary/Lookup/Bm25/ExactPrompt"| D{"Есть обученный checkpoint\nи held-out числа?"}
    D -->|"НЕТ (только скелет)"| SCAF["✅ PASS_AS_HONEST_UNTRAINED_SCAFFOLD\nэто веха, НЕ рабочая фича"]
    D -->|"ДА"| E{"Провенанс train легален?\n(closed teacher в train?)"}
    E -->|"closed_proprietary && used_for_training"| REB["🚫 DATASET_REBUILD_REQUIRED\nстрока не считается обучением"]
    E -->|"легально"| F{"3 гейта зелёные?\nheld-out lift + ablation + lifecycle"}
    F -->|"НЕТ"| SCAF
    F -->|"ДА"| TR["✅ PASS_AS_TRAINED_FEATURE\nединственное «обучено»"]
```

**Симуляции по веткам (обязательно):**

| Ветка | Вход (симуляция) | Ожидаемый результат |
|---|---|---|
| B → SHORT | ребро решает по `_candidatePositionWeights` как финал | `FORBIDDEN_RUNTIME_SHORTCUT`, измерение не запускается |
| C → SHORT | пустая машина выдаёт «перевод» без state | shortcut: отвечал не узел, а спрятанный детерминизм |
| C → D | пустой узел → `NotTrainedStatus`, рефлексией нет запретных полей | переходим к проверке обучения |
| D → SCAF | контракт есть, checkpoint нет | `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (веха) |
| E → REB | train-строка `teacher=Claude`, `used_for_training=true` | `DATASET_REBUILD_REQUIRED`, набор в карантин |
| F → TR | held-out +0.04, ablation роняет accuracy, fresh-load==trained | `PASS_AS_TRAINED_FEATURE` |
| F → SCAF | held-out есть, но fresh-load≠trained (lifecycle не доказан) | остаёмся scaffold, не trained |

### §0.2 Единый отчёт измерения `MeasurementReport` (class diagram)

```mermaid
classDiagram
    class MeasurementReport {
        +string TargetId
        +string Level
        +SpeedAxis Speed
        +AccuracyAxis Accuracy
        +QualityAxis Quality
        +SplitProof Split
        +TeacherProvenance Provenance
        +string PassVerdict
    }
    class SpeedAxis { +long CpuMs +double PeakRamMb +long ElapsedMs +int TrainRows }
    class AccuracyAxis { +double LiveMarginAccuracy +double HeldOutAccuracy +double AccuracyDelta }
    class QualityAxis { +string NoveltyDecision +bool AblationDropsAccuracy +bool PermutationInvariant +bool NoSourceIdLeakage }
    class SplitProof { +TrainSourceRoots +EvalSourceRoots +SourceHashes +int CrossSplitNearDuplicateCount }
    class TeacherProvenance { +closed_proprietary_model +teacher_output_used_for_training +... }
    MeasurementReport o-- SpeedAxis : has_speed
    MeasurementReport o-- AccuracyAxis : has_accuracy
    MeasurementReport o-- QualityAxis : has_quality
    MeasurementReport o-- SplitProof : has_split_proof
    MeasurementReport o-- TeacherProvenance : reads_provenance
    note for MeasurementReport "NEW (блок 11), СКЕЛЕТ-план.\nЕдиный отчёт трёх осей узла/ребра + verdict.\nРешает фрагментацию полей бенчмарка"
    note for AccuracyAxis "ОБЯЗАТЕЛЬНО разделяет live и held-out.\nlive — только «учится ли»; held-out — приёмка"
    note for TeacherProvenance "Канон блока 08, тут только ЧИТАЕТСЯ.\nclosed+train → DATASET_REBUILD_REQUIRED"
```

| Соединение | Пример входа (симуляция) | Ожидаемый результат по этой ветке |
|---|---|---|
| MeasurementReport o-- AccuracyAxis | live=0.62, held-out=0.44 | `Accuracy{Live=0.62, HeldOut=0.44, Delta=+0.04}` — разделены |
| MeasurementReport o-- SplitProof | near-dup=0 | accuracy разрешена; near-dup>0 → блок до адъюдикации |
| MeasurementReport o-- TeacherProvenance | `closed=true, used_for_training=true` | `PassVerdict=DATASET_REBUILD_REQUIRED` |

---

## 2. Концепт простыми словами (аналогия, без кода)

Представьте, что вы нанимаете и проверяете **переводчика**.

- **СКОРОСТЬ** — сколько времени и сил он потратил на курс: часы, усталость,
  сколько карточек прошёл. Важно для бюджета, но быстро выученный курс не значит,
  что человек переводит хорошо.

- **ТОЧНОСТЬ на тренировке vs точность на экзамене.** Пока вы учите переводчика на
  конкретных карточках, легко добиться, чтобы на ЭТИХ карточках он отвечал лучше
  «себя вчерашнего». Это и есть live-accuracy: «на учебных парах правильный вариант
  теперь нравится модели больше неправильного». Но это ничего не говорит о незнакомом
  тексте. Настоящий экзамен — **новые** фразы, которых не было в учёбе (held-out).
  Только провал/успех на новом тексте — это «точность».

- **КАЧЕСТВО = доказать, что он именно научился, а не схитрил.** Три ловушки:
  1. **Зубрёжка вместо понимания** (novelty-gate): если он заучил ответы и не видит
     новых слов, его нельзя «повышать». Учить надо новое.
  2. **Тайная шпаргалка** (ablation): отнимите выученное состояние — он обязан честно
     сказать «я ещё не готов». Если без обучения он всё равно «отвечает», значит,
     отвечал не он, а спрятанная детерминированная шпаргалка (regex/словарь/точный
     lookup).
  3. **Подсматривание разметки** (permutation/leakage): если он угадывает по номеру
     варианта в списке, по имени файла или по id источника — это не перевод, это
     списывание. Поменяйте порядок вариантов: правильный ответ не должен «переехать»
     только из-за перестановки.

- **Две честности.** Есть разница между «честный стажёр, который прямо говорит: я ещё
  не учился» (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — нормальная веха) и «дипломированный
  специалист, сдавший экзамен на новом тексте, чьи навыки можно сохранить, перенести и
  продолжить обучать» (`PASS_AS_TRAINED_FEATURE`). Выдавать стажёра за специалиста —
  самый опасный обман (`two_pass_conflation_untrained_reported_as_trained`, `-1000`).

- **Откуда брать «правильный ответ» (легальность учителя).** Можно учиться по
  открытым лицензированным учебникам (open-weight/permissive teacher с проверенными
  условиями). А вот списать экзаменационные ответы у закрытого репетитора (Claude/
  OpenAI/Gemini) и выдать их за свою учёбу — запрещено: такой «репетитор» допустим
  только как **проверяющий/критик** (judge), но не как источник, по которому учат.
  Если в учебный набор просочились его ответы — весь набор пересобирается
  (`DATASET_REBUILD_REQUIRED`).

В графе `ILearningDynamicGraph` каждый узел/ребро — это такой «переводчик». Гайд
говорит, какими числами и проверками сдавать экзамен на каждом узле и на каждом ребре.

---

## 3. Как это сейчас реализовано в нашем коде (что РАБОТАЕТ, что ЗАГЛУШКА)

Ниже — реальные образцы измерения, уже присутствующие в репозитории. Это фундамент,
на который план графа опирается (а не изобретает заново). Каждая опорная строка
проверена чтением файла (см. §8).

### 3.1 СКОРОСТЬ — РАБОТАЕТ (ресурсные метрики уже считаются)

Раннер продуктового held-out для перевода пишет ресурсные числа прямо в отчёт:

- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:239-241`
  — `CpuMs: trainResult.Value.CpuMs`, `PeakRamMb: trainResult.Value.PeakRamMb`,
  `ElapsedMs: Environment.TickCount64 - startMs`.
- Там же `TrainingRows: trainRows.Length` (около `:226`) — это `train_rows`.

DPO-раннер `InternalTrainReadyDpoRunner` (по рекону
`codex-recon-mvp-experiments.md:70-80`) на каждый батч пишет `CpuPercent`,
`WorkingSetDeltaMb`, `ElapsedMs`, `CheckpointBytesTotal`, а в summary —
`TotalElapsedMs`. Teaching-раннер (рекон `codex-recon-mvp-teaching-ledger.md:37-39`)
сравнивает дешёвый baseline (`elapsed_ms` ~40-76 мс) против обучения
(`teaching_elapsed_ms` ~2.5-14.7 тыс. мс), плюс `process_cpu_percent`,
`working_set_delta_mb`, `disk_delta_mb`, `checkpoint_bytes`.

**Вывод:** ось «скорость» уже имеет рабочий контракт полей. Заглушки тут нет; есть
**фрагментация** (разные раннеры пишут поля чуть по-разному) — план обязан свести их
в единый `SpeedAxis` внутри `MeasurementReport`.

### 3.2 ТОЧНОСТЬ — ДВА РАЗНЫХ ЧИСЛА, оба РАБОТАЮТ, но значат разное

**(а) Live reward-margin accuracy (НЕ held-out, осторожно).** Низкоуровневый
DPO-тренер определяет accuracy так:

- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:64` —
  `Accuracy = margin > 0 ? 1f : 0f;` (для одного батча);
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:120` —
  `Accuracy = accuracySum / count` (усреднение по батчам).
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:60` — `Loss = loss` (одиночный батч) и
  `:116` — `Loss = lossSum / count` (усреднение). Это **loss-сходимость**: тоже live-сигнал
  «учится ли вообще», который ГЕЙТИТСЯ (не только описывается) — см. §6 Гейт 1a/1b.

То есть «accuracy» раннера = **доля пар, где chosen ранжируется выше rejected** на
ТЕКУЩЕМ потоке обучения. Живые значения v196 (рекон
`codex-recon-mvp-experiments.md:82-87`): `AverageAccuracy≈0.6228`,
`FinalAccuracy≈0.5833`, `AverageRewardMargin≈0.01217`; плюс `AverageLoss≈0.6873`,
`FinalLoss≈0.6882` (`FinalLoss≈0.6882 ⇒ WEAK_NEAR_RANDOM`, и `FinalLoss>AverageLoss ⇒
UNSTABLE_RUN`; §6 Гейт 1a/1b). Это рабочий сигнал «учится ли вообще», но **это не
семантическая точность узла графа**. В `MeasurementReport` идёт в
`AccuracyAxis.LiveMarginAccuracy`, помеченным как live.

**(б) Held-out продуктовая точность (это и есть «точность» для приёмки).** Раннер
перевода считает метрики на отложенных кейсах и блокирует контаминацию:

- `TranslationProductHeldOutRunner.cs:285-287` — проверка пересечения train↔held-out:
  `BuildTrainHeldOutOverlapError(...)` → ошибка `TRAIN_HELDOUT_OVERLAP` **ДО** объявления
  accuracy. Это ключ: точность объявляется только если обучение и экзамен не пересеклись.
- `TranslationProductHeldOutRunner.cs:369-374` — реальные held-out ratios:
  `RuntimeExactAccuracy`, `RuntimeProductAccuracy` (составной), `RuntimeBootstrapRate`,
  `TargetScriptAccuracy`, `CopiedSourceRate`, `WrongDirectionRate`.
- `TranslationProductHeldOutRunner.cs:375-387` — составные held-out метрики:
  `EmojiPreservationAccuracy`, `NamedEntityPreservationAccuracy`, `Bleu1`, `ChrF1`
  (с учётом наличия маркеров в кейсе).

**Вывод:** оба числа РАБОТАЮТ, но план обязан их **разделять**. См. §4 шаг 3 и §6
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
`codex-recon-core-27-translation.md:27`, `TatoebaPilotLoopRunner.cs:777` вычисляет
`promotionDecision` с порогами от `maxAccuracy`: `MinimumNewPatternRate = 1 -
maxAccuracy`, `MaximumSeenTokenRate = maxAccuracy`. Решения вида
`PROMOTE_NEW_PATTERN_GT_{X}_AND_SEEN_TOKENS_LT_{Y}` или `SKIP_SHORTFALL`. Формула
совпадает с правилом CLAUDE.md: строгий novelty-gate
`newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy`.

**Ablation / анти-leakage — РАБОТАЕТ (на уровне машины).**

- `C:/Projects/llm_mvp_core/Tests/Learn06HeldOutAblationRunnerTests.cs:42` — трек
  `OrchestrationReasoningMachine` должен дать вердикт `PASS_NAMED_CONTROLS_NOT_LEAKAGE`;
  `:43-44` — overlay-треки `PromptToTaskMachine`/`SkillSelectionMachine` честно
  помечены `PARTIAL_NOT_FULL_MACHINE_EVAL`. **(RR8, закрыто `claude-plan-11 §7.1`:** этот
  `PARTIAL` — переходное состояние; `IGraphMeasurementRunner.Measure(req.Level=Unit)` с реальным
  `HeldOut`+`ExpectedLabelSource` даёт каждому узлу полный per-node held-out прогон и поднимает
  вердикт до `PASS_AS_TRAINED_FEATURE`/`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.**) По рекону `codex-recon-core-tests.md:25`
  пять именованных контролей: `exact_prompt_replay`, `path_title_source_id_leakage`,
  `nearest_neighbor_memorization`, `no_trained_state_baseline`,
  `shuffled_chosen_rejected`. Контроль `no_trained_state_baseline` сравнивает
  загруженную модель с `new OrchestrationReasoningMachine()` — прямая проверка, что
  результат от обученного state, а не от детерминированного fallback.
- Skeleton-тест честного НЕобученного (ablation «нет состояния → not_ready»):
  `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:18-20` — пустая
  машина возвращает `ContractStatus == NotTrainedStatus`, `TaskType ==
  NotReadyTaskType`, evidence содержит `NO_TRAINED_STATE_LOADED`; `:22` —
  evidence-need «prompt-to-task inference can be claimed» (честное «пока нельзя
  заявлять инференс»).
- Запрет скрытого детерминизма (рефлексией):
  `PromptToTaskMachineSkeletonTests.cs:79-85` — у типа НЕТ поля `Regex` и членов с
  именами `Dictionary`/`Classifier`/`Trainer`/`Lookup`/`Bm25`/`ExactPrompt`.

**Permutation-invariance — РАБОТАЕТ (образец).** По рекону
`codex-recon-core-tests.md:23`, `SkillSelectionPhase06TrainedSelectorEvidenceTests`
содержит `EvaluateCandidateOrderLeakage` (реверс `AvailableSkillIds`), запрет
`CandidatePositionScaffoldEvidence` и негативный контроль `SelectedFromSourceIdOnly`
обязан быть `false`.

**Lifecycle обученной машины — РАБОТАЕТ (эталон приёмки).** Тот же
`SkillSelectionPhase06...` (рекон `codex-recon-core-tests.md:23`): строит
`LearningBatch<SkillSelectionDomain>` + source-disjoint `LearningEvaluationSet`,
гоняет `EvaluateAsync` на ПУСТОЙ (baseline) и обученной машине, требует
`PassesGate`/`Accuracy`/`RewardMargin`, затем `SaveCheckpointAsync` → `ImportAsync` →
`LoadCheckpointAsync` (fresh-load) и проверяет `freshEval.Accuracy ==
evalResult.Accuracy` (загрузка без переобучения).

**ЗАГЛУШКИ / СЛАБЫЕ МЕСТА (план обязан закрыть):**

- **Авто-построение DPO (главная цель плана) — ЗАГЛУШКА/отсутствует.** Прочитанные
  тесты ПОТРЕБЛЯЮТ готовые DPO-строки; теста-ГЕНЕРАТОРА DPO-троек не найдено (рекон
  `codex-recon-core-tests.md:50`). Это ядро нового плана (блок `claude-plan-07`).
- **DynamicGraph как граф — отсутствует среди измеренного.** Нет теста multi-node/
  рёбер/fan-in/fan-out/many-to-many/async-исполнения (рекон
  `codex-recon-core-tests.md:49`). Все ablation-контроли реализованы на уровне машины,
  не на ребре графа — их надо тиражировать **per-edge** (блок `claude-plan-02`).
- **`RewardMargin` в переводе фиксированный (0.03/0.0)** — это метка наличия
  `[WRONG:`-примера, а не вычисленный обучающий сигнал (рекон
  `codex-recon-core-27-translation.md:22,49`). Для узла графа, заявленного как
  «обученный», нужен реальный margin (как в `11.DPO/DpoTrainer.cs`).
- **Точное запоминание источника (`_exactSourceMap`, conf 0.93; `RuntimeExactAccuracy`)**
  — кандидат на leakage/replay (рекон `codex-recon-core-27-translation.md:43,50`).
  Перед заявлением «обученный» по узлу — обязательна ablation на replay.
- **Continue-training поверх чекпойнта — флаг есть, реальное исполнение не
  подтверждено** (рекон `codex-recon-core-tests.md:52`:
  `ContinueTrainingFromCheckpointReady=true`, но повторный `TrainBatchAsync` поверх
  загруженного чекпойнта в прочитанном тесте не гоняется).
- **Legal-teacher provenance в измерении — отсутствует** (рекон
  `codex-recon-core-tests.md:51`, `codex-recon-mvp-train-ready.md:84-88`). См. §6 гейт 6.

---

## 4. Пошаговый рецепт/алгоритм измерения узла/ребра графа

Для каждого `ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator` в графе. В скобках
у каждого шага — **локальный шаг блока 11** (`Lk`, по `claude-plan-11 §0-pre`), чтобы гайд
и план читались как одно целое. Глобально весь блок измерения — это одна фаза доски
**Ф5 «Измерение»** (канон доски — ровно Ф0…Ф11; ID `Ф12.*.*` не используется, `Фk ≡ Lk`
локально, как в `claude-plan-11` строки 96–98).

**Шаг 0 — Классифицировать уровень (до любых чисел) [L2].** По
`trainable-architecture-decision-SKILL.md` подтвердить, что измеряемое — один из
allowed-уровней (`ILearningMachine`/`ILogicalOrchestrator`/`ILogicalFlow`/
`ILogicalUnit`/`ILearningMachinePair`; A-LM-12 — без 6-го уровня). KNN/scorer/counter/
regex — НЕ уровень; если такой механизм заявлен как «обученный авторитет» →
`FORBIDDEN_RUNTIME_SHORTCUT`, мерить нечего, сначала чинить архитектуру. Для графа на
КАЖДОМ ребре назвать владельца финального авторитета (Graph-Style Reconstruction Gate).

**Шаг 1 — Измерить честность НЕобученного (ablation «нет состояния») [L3].** На ПУСТОМ
узле вызвать инференс. Ожидать честный `not_ready`-ответ (status `NotTrained`, evidence
`NO_TRAINED_STATE_LOADED`/`NO_SEMANTIC_CLASSIFICATION_PERFORMED`) и рефлексией доказать
отсутствие `Regex`/`Dictionary`/`Classifier`/`Lookup`/`Bm25`/`ExactPrompt`. Образец —
`PromptToTaskMachineSkeletonTests.cs:18-22, 79-85`. Это кандидат на
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (веха, не фича).

**Шаг 2 — Построить train/held-out СПЛИТ без пересечения [L4].** Зарезервировать
held-out строки, пометить их `UsedForTraining=false`, обучить ТОЛЬКО на train. Перед
объявлением любой точности (а) проверить контаминацию (образец
`TranslationProductHeldOutRunner.cs:285-287`, ошибка `TRAIN_HELDOUT_OVERLAP`); (б)
проверить, что наборы **source-disjoint** (разные `SourcePromptEntryId`); (в) посчитать
near-duplicate (simhash/minhash). Если `cross_split_near_duplicate_count > 0` —
**accuracy заблокирована** до адъюдикации (§6 гейт 8). Поля идут в `SplitProof`.

**Шаг 3 — Измерить ТОЧНОСТЬ двумя числами + loss-сходимость, и не путать их [L5].**
- Live: `Accuracy = доля(margin>0)` на обучении (`DpoTrainer.cs:64,120`) — только как
  «учится ли вообще». → `AccuracyAxis.LiveMarginAccuracy`.
- **Loss-сходимость (DPO live, обязательный сигнал «учится ли вообще»):** `FinalLoss < 0.685`
  = сигнал обучения; `FinalLoss >= 0.693 (~ln 2)` = random-guess = **брак** (no-train, не
  promotion); зона `0.685..0.693` = `WEAK_NEAR_RANDOM` (требует SCALE_UP/диагностики, **не**
  PASS). Источник: `DpoTrainer.cs:60` (одиночный `Loss`) и `:116` (усреднение `lossSum/count`).
  Это «учится ли», а не приёмка: даже при `FinalLoss<0.685` приёмочная точность всё равно =
  held-out (см. ниже и §6 Гейт 1a). Живой пример v196: `FinalLoss≈0.6882 ⇒ WEAK_NEAR_RANDOM`.
- Held-out: реальная точность на отложенных строках
  (`TranslationProductHeldOutRunner.cs:369-374`, составные `:375-387`). Это число идёт
  в приёмку. → `AccuracyAxis.HeldOutAccuracy`.
- Зафиксировать дельту `AccuracyAfter - AccuracyBefore` (обученная минус пустая): без
  положительной дельты обучения не было. → `AccuracyAxis.AccuracyDelta`.
- **Стабильность хвоста (Final vs Average) → `UNSTABLE_RUN`:** если `FinalRewardMargin <
  AverageRewardMargin` ИЛИ `FinalLoss > AverageLoss` (живой v196: margin `0.01013<0.01217`,
  loss `0.6882>0.6873`) → брать **best-checkpoint по held-out**, не последний; запрет
  `PASS_AS_TRAINED_FEATURE` на деградирующем хвосте. См. §6 Гейт 1b.

**Шаг 4 — Измерить КАЧЕСТВО (это обучение, а не утечка) [L6].**
- **novelty-gate:** `newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy`
  (образец `TatoebaPilotLoopRunner.cs:777`). Иначе `SKIP_SHORTFALL` — не повышать.
- **ablation `no_trained_state_baseline`:** сравнить с `new ...Machine()`; обученный
  результат обязан отличаться от пустого (`Learn06HeldOutAblationRunnerTests.cs:42`).
- **leakage-контроли (per-edge):** `exact_prompt_replay`, `source_id`/`pair_id`/
  `file_path`/`candidate_position`. `SelectedFromSourceIdOnly` обязан быть `false`.
- **permutation-invariance:** реверс порядка кандидатов не меняет выбор
  (`EvaluateCandidateOrderLeakage`). → `QualityAxis`.

**Шаг 5 — Измерить СКОРОСТЬ [L7].** Записать `CpuMs`, `PeakRamMb`, `ElapsedMs`,
`train_rows` (образец `TranslationProductHeldOutRunner.cs:239-241`). UNKNOWN в этих
полях запрещён для done-заявления (Runtime Evidence Firewall, §6 гейт 2). → `SpeedAxis`.

**Шаг 6 — Доказать LIFECYCLE (для `PASS_AS_TRAINED_FEATURE`) [L8].**
`SaveCheckpointAsync` → `ImportAsync` → `LoadCheckpointAsync` (fresh-load) → проверить
`freshEval == trainedEval` (загрузка без переобучения), затем `continue-training`:
повторный `TrainBatchAsync` поверх загруженного чекпойнта (новые `row_id`/`source_id`,
не replay старого батча), без обучения с нуля. Образец —
`SkillSelectionPhase06TrainedSelectorEvidenceTests.cs`.

**Шаг 7 — Выбрать честность и применить промоушен-гейт [Ф5 / L9–L12].**
- Сначала легальность: если в train есть `closed_proprietary_model &&
  teacher_output_used_for_training` → `DATASET_REBUILD_REQUIRED`, измерение этой строки
  как «обучения» недействительно (§6 гейт 6).
- Если все три гейта (held-out числа + ablation + lifecycle) зелёные →
  `PASS_AS_TRAINED_FEATURE`.
- Если только шаг 1 → только `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.
- No-signal путь: если авто-DPO дал `accepted_auto_pair_count_after_all_gates == 0` —
  нет train, нет promotion, предыдущий checkpoint сохранён (это нормальный исход, не
  провал).
- Любая попытка записать trained на скаффолд-баре = масскарад (`-1000`).

---

## 5. Конкретные примеры с реальными значениями

### 5.1 DPO-пара chosen/rejected/[WRONG:] (формат перевода)

Реальная строка train-ready (рекон `codex-recon-core-27-translation.md:21`,
файл-семейство `27.TranslationDomain/TrainingData/*.jsonl`, `ru-en-product-b001-001`):

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
контраст «правильно vs неправильно», на котором учится DPO-узел. **Запрещённые
основания контраста** (Set Quarantine, §6 гейт 4): разница chosen/rejected = id
источника, pair id, имя файла/путь, порядок кандидатов, точная память промпта,
keyword→skill lookup. В графе каждый такой контраст хранится как `GraphDpoPair`
(канон блока 08) с обязательным `TeacherProvenance`.

### 5.2 Живые числа DPO-обучения (v196, реальные)

Рекон `codex-recon-mvp-experiments.md:82-87`:

```
PairCount=3916, BatchSize=16, BatchCount=245, ContextLen=96, VocabSize=2048
AverageLoss≈0.6873   FinalLoss≈0.6882
AverageAccuracy≈0.6228   FinalAccuracy≈0.5833      // live margin>0, НЕ held-out
AverageRewardMargin≈0.01217   FinalRewardMargin≈0.01013
Шаг 1: Loss 0.6837, ChosenReward 0.01533, RejectedReward -0.00401, RewardMargin 0.01934, Accuracy 0.75
```

Как это читать: `AverageAccuracy 0.62` — это «на 62% обучающих пар chosen уже выше
rejected». Это НЕ «модель переводит на 62% правильно». Для приёмки держим held-out
отчёт отдельно (`AccuracyAxis.HeldOutAccuracy`).

А `FinalLoss≈0.6882` читается через **Гейт 1a** (§6): это зона `0.685..0.693` ⇒
`WEAK_NEAR_RANDOM` (почти random `~ln 2≈0.693`; не PASS, нужен SCALE_UP/диагностика). И
хвост деградирует: `FinalRewardMargin 0.01013 < AverageRewardMargin 0.01217` И `FinalLoss
0.6882 > AverageLoss 0.6873` ⇒ **Гейт 1b** `UNSTABLE_RUN` (брать best-checkpoint по
held-out, запрет `PASS_AS_TRAINED_FEATURE` на падающем хвосте). То есть этот прогон сам по
себе НЕ дотягивает до trained-сигнала ни по loss, ни по стабильности.

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

> **C2-measure-coherence (честное согласование числа `RewardMargin >= 0.025` с реальным ~0.01).**
> В примере выше `RewardMargin = 0.03` — это иллюстративное значение, проходящее target 0.025; на
> реальном прогоне (v196, §5.2) живой margin был **~0.01** (`AverageRewardMargin≈0.01217`,
> `FinalRewardMargin≈0.01013`), то есть **ниже** target. Разводим три числа явно:
> - **`0.025`** — это **target-приёмка `RewardMargin` на held-out промоушен-гейте**
>   (`TranslationProductHeldOutRunner.cs:211-220`), LOCKED по Q30. Это число, которое надо доказать,
>   а не факт текущего прогона.
> - **`~0.01`** — реально достигнутый **live** reward-margin на обучающем потоке (`DpoTrainer`),
>   на **другой оси** (live «учится ли»), а не held-out приёмка. Прямое сравнение live ~0.01 с target
>   0.025 как «провал» — ошибка осей (Гейт 1).
> - **`0.03` в `TranslationMachine.cs:70/125/187`** — это **не вычисленный сигнал, а
>   `[WRONG:]`-плейсхолдер-литерал** (R-19), и в измерение он НЕ принимается; приёмочный margin берётся
>   только из движка `DpoTrainer`.
>
> **Честная нота о текущем evidence:** на сегодня единственный реальный margin (~0.01 live) НИЖЕ target
> 0.025, а held-out `RewardMargin` для узлов/рёбер графа на реальном прогоне ещё НЕ измерен (генератор
> DPO и per-level held-out отсутствуют — §3 «ЗАГЛУШКИ», §7.4). Поэтому 0.025 — это **target-to-prove**:
> узел с held-out margin < 0.025 или только live ~0.01 остаётся `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
> (или `WEAK_NEAR_RANDOM`/`UNSTABLE_RUN` по Гейтам 1a/1b), а НЕ `PASS_AS_TRAINED_FEATURE`. Число
> приёмки не занижается; честно фиксируется лишь то, что target ещё не достигнут. **Синхрон с
> `claude-plan-11` §6.5a** (то же согласование) и `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1` §E Q35 (RESOLVED).

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
`PARTIAL_NOT_FULL_MACHINE_EVAL` (не выдают себя за полный held-out машины). Это образец
«честного ограничения объёма». **RR8-закрытие (владелец `claude-plan-11 §7.1`):** для узлов
графа `PARTIAL` — переходное состояние; `IGraphMeasurementRunner.Measure(req.Level=Unit)`
выполняет недостающий полный per-node held-out прогон (с владельцем эталона `ExpectedLabelSource`
и едиными числами Q30), после чего узел получает финальный вердикт scaffold/trained, а не
остаётся в `PARTIAL`.

### 5.6 Заполненный `MeasurementReport` (две честности рядом)

Чтобы было видно, как одни и те же оси дают разные вердикты:

```
// HONEST_UNTRAINED_SCAFFOLD: контракт есть, обучения нет
MeasurementReport{ TargetId="node:PromptToTask", Level="unit",
  Speed{CpuMs=0, PeakRamMb=0, ElapsedMs=2, TrainRows=0},
  Accuracy{Live=0, HeldOut=0, Delta=0},
  Quality{Novelty="N/A", AblationDropsAccuracy=true, PermutationInvariant=true, NoSourceIdLeakage=true},
  Split{CrossSplitNearDuplicateCount=0}, Provenance{closed=false,used_for_training=false},
  PassVerdict="HONEST_UNTRAINED_SCAFFOLD" }   // веха, НЕ trained

// TRAINED_FEATURE: 3 гейта зелёные
MeasurementReport{ TargetId="node:SkillSelection", Level="unit",
  Speed{CpuMs=1234, PeakRamMb=210, ElapsedMs=5600, TrainRows=50},
  Accuracy{Live=0.62, HeldOut=0.44, Delta=+0.04},
  Quality{Novelty="PROMOTE_NEW_PATTERN_GT_0.15_AND_SEEN_TOKENS_LT_0.85",
          AblationDropsAccuracy=true, PermutationInvariant=true, NoSourceIdLeakage=true},
  Split{CrossSplitNearDuplicateCount=0}, Provenance{closed=false,used_for_training=true},
  PassVerdict="TRAINED_FEATURE" }
```

---

## 6. Гейты и правила (что применимо к измерению)

### Гейт измерения 1 — НЕ ПУТАТЬ live-accuracy и held-out

`Accuracy = доля(margin>0)` (`DpoTrainer.cs:64,120`) — это live reward-margin на
обучающем потоке. Запрещено выдавать её за «семантическую точность узла». Точность для
приёмки = held-out, source-disjoint, после проверки контаминации
(`TRAIN_HELDOUT_OVERLAP`, `TranslationProductHeldOutRunner.cs:285-287`). Это прямо
зафиксировано в DPO-парах качества (рекон `codex-recon-mvp-train-ready.md:62-67`:
вариант «считать batch accuracy финальной классификацией» помечен `[WRONG]`).

### Гейт измерения 1a — loss-сходимость (DPO live)

`FinalLoss < 0.685` = сигнал обучения; `FinalLoss >= 0.693 (~ln 2)` = random-guess = **брак**
(no-train, не promotion); зона `0.685..0.693` = `WEAK_NEAR_RANDOM` (требует SCALE_UP/
диагностики, **не** PASS). Источник числа — `Loss` из `DpoMetrics`: `11.DPO/DpoTrainer.cs:60`
(одиночный батч `Loss = loss`) и `:116` (усреднение `Loss = lossSum / count`). Это сигнал
«учится ли вообще», а не приёмочная точность (приёмка = held-out, Гейт 1). Живой пример v196
(рекон `codex-recon-mvp-experiments.md:82-87`): `FinalLoss≈0.6882 ⇒ WEAK_NEAR_RANDOM`.
**Синхрон с `claude-plan-11` Гейт 1a (строка 696) и `claude-guide-01` §6.4** (`loss<0.685`,
`>=0.693=random=брак`) — три артефакта симметричны.

### Гейт измерения 1b — стабильность хвоста прогона (Final vs Average)

Если `FinalRewardMargin < AverageRewardMargin` ИЛИ `FinalLoss > AverageLoss` → вердикт
`UNSTABLE_RUN`: брать **best-checkpoint по held-out**, а не последний батч; запрет
`PASS_AS_TRAINED_FEATURE` на деградирующем хвосте. Живой пример v196: `margin
0.01013<0.01217` И `loss 0.6882>0.6873` ⇒ `UNSTABLE_RUN`. Тренд `LiveMarginTrend`
(rising/flat/degrading) читается из последовательности `DpoMetrics.RewardMargin`/`Accuracy`
по батчам (`DpoTrainer.cs:64,116-120`). **Синхрон с `claude-plan-11` Гейт 1b (строка 697) и
`claude-guide-01` §6.4** («`reward_margin ≥ 0` и растёт»): хвост не должен падать.

### Гейт измерения 2 — Runtime Evidence Firewall (анти-стаб)

Из `phase-completion-packager-SKILL.md:82-85`. Отвергать как доказательство обучения:
`validation_only: true`; `metric_type` со значением `static`/`contract`/`mixed`;
`cpu_ms`/`ram_mb`/`accuracy` = `UNKNOWN`. Build/contract/DNA проходят и на json-заглушке —
поэтому их НЕДОСТАТОЧНО для trained-узла. Нужны воспроизводимые числовые runtime-метрики,
и узел обязан **менять решение после обучения на новых парах**, а не реплеить точную
память.

### Гейт измерения 3 — Two-PASS (OP-57), 3 гейта для trained

Из `phase-completion-packager-SKILL.md:96-114`:

- `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — разрешён, когда скелет/контракт честно
  `not_ready`, нет семантик-claim, нет скрытого детерминизма, нет фейковых
  DPO/checkpoint/metrics (тесты-честности `PromptToTaskMachineSkeletonTests.cs`). **Это
  веха, не рабочая фича.**
- `PASS_AS_TRAINED_FEATURE` — разрешён ТОЛЬКО при полном комплекте: **(1)** held-out
  числовые метрики + **(2)** ablation (без обученного состояния → not_ready;
  `no_trained_state_baseline`) + **(3)** lifecycle: fresh-load probe + continue-training
  probe + e2e через реальную точку входа.

Смешение (trained на скаффолд-баре) =
`two_pass_conflation_untrained_reported_as_trained`, `-1000`.

### Гейт измерения 4 — Анти-leakage / permutation-invariance (per-edge)

На каждом ребре графа запретить решающие сигналы: `candidate_position`, `source_id`,
`pair_id`, `file_path`, `exact_prompt_replay`. Контроль `SelectedFromSourceIdOnly`
обязан быть `false`; реверс порядка кандидатов не меняет выбор
(`EvaluateCandidateOrderLeakage`, рекон `codex-recon-core-tests.md:23`). Набор
именованных контролей-эталон: `exact_prompt_replay`, `path_title_source_id_leakage`,
`nearest_neighbor_memorization`, `no_trained_state_baseline`, `shuffled_chosen_rejected`
(`Learn06HeldOutAblationRunnerTests.cs:42` + рекон `codex-recon-core-tests.md:25`).
**Anti-masquerade:** `no_replay_proven=true` требует **разных** наборов идентичного
типа (новые `row_id`/`source_id`, не старый `batch_id`). Если replay/exact-memory нельзя
исключить — гейт открыт, `no_replay_proven=false`, и это FAIL, не PASS
(`plan-review-SKILL.md`, Deterministic evidence masquerade firewall).

### Гейт измерения 5 — Запрет скрытого детерминизма (что вообще можно мерить)

`trainable-architecture-decision-SKILL.md`: KNN/BM25/scorer/selector/router/counter/
regex — НЕ архитектурный уровень; допустимы только как помеченная internal-деталь/
baseline/eval-only внутри allowed-уровня (KNN как memory-компонент разрешён, но не как
немаркированный финал). Если такой механизм — немаркированный финальный авторитет ребра
→ `FORBIDDEN_RUNTIME_SHORTCUT` (мерить «обученность» нельзя, сначала чинить;
A-LM-09/A-LM-13). Прямой code-эталон отсутствия скрытого детерминизма — рефлексия в
`PromptToTaskMachineSkeletonTests.cs:79-85`. Принцип E51B: «KNN can choose memory.
Arbiters decide acceptance» (рекон `codex-recon-core-docs-exp.md:104-107`) — память не
есть авторитет.

### Гейт измерения 6 — Легальный teacher / provenance (источник chosen/rejected)

`trainable-architecture-decision-SKILL.md` (Legal Teacher Gate) + триггер
`HOW-TO-USE-SKILLS.md:330`. Если `chosen`/`rejected` сгенерированы внешней моделью,
обязательны поля: `teacher_access_mode`, `hosting_provider`, `model_id`,
`model_license`, `provider_terms_checked`, `teacher_output_used_for_training`,
`closed_proprietary_model` (плюс `request_id`/`model_version` при наличии). **CANON:
closed-proprietary модель (Claude/OpenAI/Gemini) допустима только как judge/eval-критик,
НЕ как teacher для дистилляции.** Если `closed_proprietary_model=true` и
`teacher_output_used_for_training=true` → всегда `DATASET_REBUILD_REQUIRED`. Текущее
состояние данных — пробел: в teaching-ledger и supplement этих полей нет (рекон
`codex-recon-mvp-teaching-ledger.md:63`, `codex-recon-mvp-train-ready.md:84-88`); строки
помечены `accepted_model_led_*` — надо установить, какая модель-учитель, до общего
обучения.

### Гейт измерения 7 — Контракт результата эксперимента (delta-порог 5%)

`docs/EXPERIMENT-RESULTS-MAP.md` + схема
`docs/plans/test hypothesys/small experiment/experiment-result-schema-v1.json` (рекон
`codex-recon-core-docs-exp.md:22-25,43-48`): один эксперимент = baseline vs improved, 5
групп метрик (`prediction_error`/`time_ms`/`memory_peak_mb`/`cpu_avg_pct`/
`disk_write_bytes`), `verdict.passed=true` только при улучшении **>=5%**; `next_action`
enum: `PROMOTE_TO_NEXT_TIER`/`TUNE_AND_RETRY`/`ABANDON_APPROACH`/`NEEDS_HUMAN_REVIEW`.

### Гейт измерения 8 — near-dup блокирует accuracy

`cross_split_near_duplicate_count > 0` → любые accuracy-claim заблокированы до
адъюдикации. Held-out, который на самом деле почти-дубликат train (simhash/minhash выше
порога `SplitProof.NearDuplicateThreshold`), не является честным экзаменом. Это
усиление source-disjoint правила: «разные id» недостаточно, нужно «не почти-копия».

### Гейт измерения 9 — SCALE_UP, а не «провал» (анти-ложный-вывод)

`experiment-design-SKILL.md` + MAP: `SCALE_UP` = «нужно больше данных, алгоритм
корректен» (E14/E20: данные слишком тривиальны/разделимы на Tier 3;
`EXPERIMENT-RESULTS-MAP.md:44-45`). FAILED на Tier 2 ≠ гипотеза неверна → эскалация на
Tier 3 до финального вердикта. Критические находки измерения: `StratifiedSplit`
обязателен при фиксированном seed (`E40`, `EXPERIMENT-RESULTS-MAP.md:72`); забывание при
неверной `max_weight`-константе (`E44`, `:71`).

### Гейт измерения 10 — Capability score только с runtime-командой

`capability-measurement-SKILL.md`: шкала 0/25/50/75/100%; «Do not report a score without
the runtime command that proves it». Любой score >=75% обязан быть спарен с командой
раннера/теста, которая его воспроизводит.

### Правило читаемости (Human gate)

Любой статус измерения для Luba — на человеческом русском: что меряли, какое число
вышло, прошёл ли порог, что это доказывает и что НЕТ
(`phase-completion-packager-SKILL.md`, секция Human-Readable). Технические метки
(`PASS_NAMED_CONTROLS_NOT_LEAKAGE`, `SKIP_SHORTFALL`, `DATASET_REBUILD_REQUIRED`)
переводить тут же.

---

## §6b. Тест-стратегия измерения (9 слоёв; FC-26) + две PASS-точки

Тесты — **проект** (план), не запуск. По каждой ветке §0/§4 — слой (совпадает с
`claude-plan-11 §9`, чтобы гайд и план не расходились).

| Слой | Что проверяет в измерении |
|---|---|
| unit | поля `MeasurementReport` заполнены без UNKNOWN; `AccuracyAxis` раздельно несёт live и held-out |
| integration | замер батча 50 пар: `SpeedAxis` реальный, `AccuracyDelta` вычислен; held-out через `EvaluateAsync` |
| e2e/host | проход через L4-оркестратор (блок 03) на обученном состоянии даёт held-out число задачи |
| QA automation | split-overlap срабатывает ДО accuracy; near-dup>0 действительно блокирует accuracy |
| UX automation | человекочитаемый статус `scaffold`/`trained`/`rebuild_required`, без жаргона; техметки переведены |
| negative | closed-teacher в train → `DATASET_REBUILD_REQUIRED`; live margin выдан за точность → тест ловит как ошибку |
| untrained-behavior | пустой узел честно `not_ready`; рефлексией нет `Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt` |
| trained-acceptance | held-out lift на source-disjoint > порога + ablation роняет accuracy + fresh-load даёт ту же accuracy |
| regression | per-edge: выключение авторитета ребра роняет held-out (вклад обучения доказан); baseline-набор не падает |

Две легитимные PASS-точки (никогда не путать): `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
(веха) и `PASS_AS_TRAINED_FEATURE` (единственное «обучено»).

---

## 7. Пробелы / что нужно построить в плане

1. **Единый контракт измерения узла графа.** Свести разрозненные поля (`DpoTrainer`
   метрики; `TranslationProductHeldOutRunner` held-out числа; teaching-ledger ресурсы) в
   один типизированный `MeasurementReport` узла/ребра с тремя осями (`SpeedAxis`/
   `AccuracyAxis`/`QualityAxis`) и обязательным разделением live-accuracy vs held-out.
   Канон-владелец — блок `claude-plan-11`.
2. **Per-edge ablation/leakage.** Сейчас контроли (`no_trained_state_baseline`,
   permutation, source-id) живут на уровне машины. План обязан тиражировать их на КАЖДОЕ
   ребро (Graph-Style Reconstruction Gate:
   `deterministic_vs_trainable_authority_on_edges`,
   `hidden_deterministic_shortcut_check_tied_to_each_edge`).
3. **Реальный held-out + ablation раннер для каждого уровня** (Unit/Edge/Graph), а не только
   для машины перевода/skill-selection. `Learn06...` сам признаёт
   `PARTIAL_NOT_FULL_MACHINE_EVAL` для PromptToTask и SkillSelection (рекон
   `codex-recon-core-tests.md:24`). **РЕШЕНО (RR8, владелец — `claude-plan-11 §7.1`):** контракт
   раннера `IGraphMeasurementRunner.Measure(MeasurementRequest)` инстанцирован — ОДНА точка входа,
   `req.Level` ∈ {Unit, Edge, Graph} выбирает per-level held-out прогон; владелец эталона —
   `ExpectedLabelSource` (кто/откуда даёт правильный ответ: `human_label` / `source_license_authority`
   / `judge_open_weight` / `golden_fixture`; `FromNodeUnderTest` обязан быть `false` — эталон НЕ из
   измеряемого узла); числа порога — `LevelThresholds`, **ЕДИНЫЕ для unit/edge/graph** (Q30 LOCKED:
   `0.95` / `+0.02` / `0.025`, отдельный профиль `DynamicGraphDomain` НЕ вводится). Это **закрывает**
   `PARTIAL_NOT_FULL_MACHINE_EVAL`: после полного per-level прогона уровень получает
   `PASS_AS_TRAINED_FEATURE` (3 числа + 3 гейта зелёные) или честный
   `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`; `PARTIAL` — переходное, а не конечное состояние.
4. **Вычисленный `RewardMargin` для не-трансформерных узлов.** Фиксированный
   `RewardMargin=0.03/0.0` в переводе — метка, не сигнал. Узел, заявленный «обученным»,
   должен давать реальный margin (как `11.DPO/DpoTrainer.cs`).
5. **Continue-training probe реально исполнить** (повторный `TrainBatchAsync` поверх
   загруженного чекпойнта на новых `row_id`), а не только флаг
   `ContinueTrainingFromCheckpointReady=true`.
6. **Анти-replay для exact-lookup узлов** (`_exactSourceMap`, `RuntimeExactAccuracy`):
   ablation, доказывающая, что точное совпадение — не запоминание источника.
7. **Teacher/source-provenance измерять и хранить** (поля Legal Teacher Gate,
   `TeacherProvenance`-20) до любого общего обучения; до этого `chosen/rejected`
   модельного происхождения — evidence/user-adaptive/judge-only, не common-training.
8. **Авто-генератор DPO-троек с встроенными гейтами измерения** — главного артефакта
   (генератора) сейчас нет (блок `claude-plan-07`); его выход обязан сразу проходить Set
   Quarantine, near-dup и leakage-контроли, плюс no-signal путь (accepted=0 → не учим).
9. **Воспроизводимость:** часть раннеров пишет в локальный пользовательский путь
   (`C:\Users\lubao\.xiigen\...`, см. `Learn06HeldOutAblationRunnerTests.cs:30-33`). План
   измерения должен задать переносимые пути/фикстуры.

---

## 8. Ссылки

### Как проверены цитаты (метод)

Опорные `file:line` ниже прочитаны напрямую в `C:/Projects/llm_mvp_core` на дату
ревизии. Где номер строки бенчмарка отличался от факта, цитата уточнена в этом гайде:
held-out core ratios — `:369-374` (а составные emoji/named-entity/Bleu/ChrF — `:375-387`);
честный not_ready skeleton — `:18-22` (evidence-need на `:22`). Цитаты `DpoTrainer.cs:64,120`,
overlap `:285-287`, gate `:211-220` и зеркало `:389-398`, ресурсы `:239-241,244-247`,
ablation `:42-44`, рефлексия `:79-85` подтверждены без изменений.

### Recon-сводки (evidence)

- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-tests.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-docs-exp.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-27-translation.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-teaching-ledger.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-experiments.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-mvp-train-ready.md`
- `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/codex-recon-core-docs-skills-catalog.md`

### Реальные файлы кода (с опорными строками, проверено)

- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:64` — `Accuracy = margin > 0 ? 1f : 0f` (live, один батч).
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:120` — `Accuracy = accuracySum / count` (live, усреднение).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:211-220` — промоушен-гейт (числовые пороги, RunDirection).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:389-398` — тот же гейт (RunAllDirections).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:285-287` — контаминация `TRAIN_HELDOUT_OVERLAP`.
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:369-374` — held-out core ratios (Exact/Product/Bootstrap/Script/Copied/WrongDirection).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:375-387` — составные held-out (Emoji/NamedEntity/Bleu1/ChrF1).
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:239-241,244-247` — CpuMs/PeakRamMb/ElapsedMs, PassesGate/GateNote.
- `C:/Projects/llm_mvp_core/27.TranslationDomain/TatoebaPilotLoopRunner.cs:777` — promotionDecision + novelty-gate (рекон `codex-recon-core-27-translation.md:27`).
- `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:18-22` — честный not_ready (NotTrainedStatus/NO_TRAINED_STATE_LOADED + evidence-need).
- `C:/Projects/llm_mvp_core/Tests/PromptToTaskMachineSkeletonTests.cs:79-85` — запрет скрытого детерминизма (рефлексия: нет Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt).
- `C:/Projects/llm_mvp_core/Tests/Learn06HeldOutAblationRunnerTests.cs:42-44` — вердикты `PASS_NAMED_CONTROLS_NOT_LEAKAGE` / `PARTIAL_NOT_FULL_MACHINE_EVAL`.
- `C:/Projects/llm_mvp_core/Tests/SkillSelectionPhase06TrainedSelectorEvidenceTests.cs` — эталон lifecycle обученной машины (train→eval baseline vs trained→export/import→fresh-load→leakage-ablation) (рекон `codex-recon-core-tests.md:23`).

### Скилы / реестры (правила)

- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md` — allowed-уровни (A-LM-12), Graph-Style Reconstruction Gate, Legal Teacher / Provenance Gate, вердикты.
- `C:/Projects/llm_mvp_core/docs/skills/phase-completion-packager-SKILL.md:82-85` — Runtime Evidence Firewall (анти-стаб).
- `C:/Projects/llm_mvp_core/docs/skills/phase-completion-packager-SKILL.md:96-114` — Two-PASS gate (scaffold vs trained, 3 гейта).
- `C:/Projects/llm_mvp_core/docs/skills/capability-measurement-SKILL.md` — 6 классов, шкала 0-100%, runtime-команда обязательна.
- `C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md` — Deterministic evidence masquerade firewall, DPO/distillation legal teacher review gate, тест-стратегия FC-26/FC-27/FC-28.
- `C:/Projects/llm_mvp_core/docs/EXPERIMENT-RESULTS-MAP.md` — результаты E01-E50, SCALE_UP, key findings (E40 StratifiedSplit, E44 max_weight).
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md:330` — триггер DPO/distillation teacher provenance.
- `C:/Projects/llm_mvp_core/docs/plans/test hypothesys/small experiment/experiment-result-schema-v1.json` — схема результата эксперимента, delta>=5%, next_action enum (рекон `codex-recon-core-docs-exp.md:22-25`).

### Соседние claude-* блоки (консистентность канона)

- `claude-plan-11-measurement.md` — полный план измерения (12/26/59 фаз, `MeasurementReport`/`SpeedAxis`/`AccuracyAxis`/`QualityAxis`/`SplitProof`/`IGraphMeasurementRunner`); этот гайд — его человеческое объяснение.
- `claude-plan-08-legal-teacher-gate.md` — канон-владелец `TeacherProvenance` (20 полей) и `GraphDpoPair`.
- `claude-plan-07-dpo-builder.md` — авто-генератор DPO-троек (главный отсутствующий артефакт §7.8).
- `claude-plan-02-edge.md` — per-edge ablation/leakage/permutation (§7.2).
- `claude-06-ilearningdynamicgraph-architecture-brief.md` — A-LM-09/12/13, `GraphEdgeKind` (5).
