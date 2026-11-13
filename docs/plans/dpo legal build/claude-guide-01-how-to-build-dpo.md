# Гайд 01 — Как строить DPO-пары (для ILearningDynamicGraph)

**Версия:** 1.0 (claude namespace; улучшенная версия бенчмарка `codex-guide-01`)
**Автор-роль:** автор `claude-*` гайда (Opus 4.8 MAX). Бенчмарк
`codex-guide-01-how-to-build-dpo.md` использован как **только-чтение evidence**:
я не копировал его дословно, а усиливал слабые места (полнота Part A, точность
цитат `file:line`, консистентность канона с остальными `claude-*`, тест-стратегия).
**Применимо к:** план ILearningDynamicGraph — авто-построение DPO для
`ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` / `ILearningDynamicGraph`
(CANON-маппинг на реальную иерархию репозитория `ILogicalUnit` / `ILogicalFlow` /
`ILogicalOrchestrator` / `ILearningMachine` / `ILearningMachinePair`, мастер §A.3).
**Язык:** русский (первичный, человеческим языком); имена типов/полей и код —
английский.
**Статус:** PLAN-ONLY. Это документация-гайд внутри плана, **НЕ** разрешение на
реализацию продукта/кода, тренировку, билд, запуск раннеров, git или вызов
внешних AI/API/моделей. Исполнение — только после свежего явного approve Luba в
текущем ходе (R2/R3, CLAUDE.md «Continuous role loop»).
**Скилы-основания:** `docs/skills/trainable-architecture-decision-SKILL.md`
(Legal Teacher / Provenance Gate, Graph-Style Reconstruction Gate, Graph-DPO
Artifact Triad Gate), `docs/skills/plan-review-SKILL.md` (DPO/distillation legal
teacher provenance review gate, deterministic-masquerade firewall),
`docs/skills/implementation-doc-uml-SKILL.md` (§0/§1 формат),
`docs/skills/dpo-triple-format-SKILL.md`, `docs/skills/planning-dpo-authoring-SKILL.md`.

**Канонические инварианты (сохранены, синхронны со всеми `claude-*`):** 12 фаз /
26 подфаз / 59 подшагов; нумерация `Ф<P>.<SP>.<SS>`; `GraphDpoPair` = **14 полей**
с **вложенным** `TeacherProvenance` = **20 полей** (владелец канона — блок 08);
A-LM-12 (L5, без 6-го уровня); легальный teacher-гейт (closed = только judge при
`used_for_training=false`); запрет скрытого детерминизма (A-LM-09/A-LM-13).

---

## 0. Якорь задачи и где этот claude-гайд улучшает бенчмарк

```text
CURRENT TASK: написать МОЙ (claude-namespace) улучшенный гайд «как строить
              DPO-пары» — план-документ, человеческим русским, Part A-формат,
              с сохранением структуры бенчмарка (8 разделов: назначение → концепт
              → код сейчас → рецепт → примеры → гейты → пробелы → ссылки) и
              канонических счётчиков/инвариантов. Записать ровно в
              claude-guide-01-how-to-build-dpo.md.
NOT THE TASK: исполнять/тестировать/билдить код, запускать раннеры, тренировать,
              вызывать внешние AI/API/модели, git; править/создавать любые codex-*
              (только чтение), продолжать чужой codex-план или claude-00..07 каркас.
EVIDENCE-ONLY: codex-guide-01 (бенчмарк гайда), codex-recon-* (рекон-сводки),
              claude-04/05 (моя политика/гайд измерения), claude-plan-07/08 (канон
              GraphDpoPair/TeacherProvenance), claude-ILEARNINGDYNAMICGRAPH-PLAN-v1
              §A.4 (канон полей/счётчиков), реальный код llm_mvp_core (читается для
              цитат), скилы plan-review / trainable-architecture-decision /
              implementation-doc-uml.
```

> **Якорь фазы (доска `claude-EXECUTION-CONTROL-BOARD`, evidence-only).** Этот
> гайд — методическая опора фазы **Ф3 «DPO-строитель»** (рецепт §4 = алгоритм
> `GraphDpoBuilder.BuildPairAsync` блока 07, подшаги Ф3.1.1–Ф3.1.3, Ф3.2.1–Ф3.2.2),
> легального гейта **Ф2.1** (§6.2 = блок 08) и измерения **Ф11** (§6.4 = блок 11).
> Канонические счётчики: **12 фаз (Ф0…Ф11) / 26 подфаз / 59 подшагов**, нумерация
> `Ф<P>.<SP>.<SS>`. Доска и блоки-владельцы — evidence-only, здесь не правятся.

**Где этот claude-гайд осознанно лучше бенчмарка `codex-guide-01` (delta-нота, не
слепая копия).** Бенчмарк — добротный методический гайд (597 строк) с честными
цитатами кода и правильным рецептом, но как **самостоятельный Part A-артефакт** у
него есть конкретные дыры. Я их чиню:

| # | Слабое место бенчмарка `codex-guide-01` | Улучшение в этом claude-гайде |
|---|---|---|
| D1 | **Нет §0 UML и §1 флоучарта.** По `implementation-doc-uml-SKILL` и Part A Completeness Contract документация-решение обязана начинаться с §0 classDiagram (`note for` на каждый класс + таблица симуляции каждой связи) и §1 flowchart с симуляцией каждой ветки. У бенчмарка ни диаграммы, ни флоучарта. | Добавил **§9 §0 UML** (classDiagram с `note for` + per-connection симуляции) и **§9 §1 flowchart** рецепта построения пары с per-branch симуляциями. |
| D2 | **Предложенный канон `GraphDpoPair` (§6.5) противоречит фиксированному канону `claude-*`.** Бенчмарк предлагает «объединить» ~25 разнородных полей (`target_level`, `curriculum_tier`, плоские provenance-поля вперемешку). Но мой sibling-канон (мастер §A.4, блок 07/08) фиксирует **ровно 14 полей** с **вложенным** `TeacherProvenance` (20 полей). Плоские provenance-поля в строке создают класс ошибки «train-ready без полного провенанса». | Выровнял на **канон 14/20** (§6.5): `teacher_provenance` — **вложенная** запись, провенанс не размазан по верхнему уровню; лишние поля бенчмарка либо переехали внутрь provenance, либо помечены как не-канон. Это синхрон с блоком 07 §3.2 и блоком 08 §3.1. |
| D3 | **Вердикты сформулированы свободным текстом.** Бенчмарк говорит `QUARANTINE_DO_NOT_TRAIN`, `DATASET_REBUILD_REQUIRED`, `dpo_train_ready=false`, но не сводит их в **единый канон-enum** строки. | Зафиксировал канон-enum вердикта строки (§6.6): `ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY` (6 значений, владелец — блок 08) и `LegScope` (4 значения). Это убирает разнобой формулировок. |
| D4 | **Нет тест-стратегии (axis верификации).** Бенчмарк даёт гейты-исходы (§6), но не план тестов untrained/trained по слоям (`plan-review` FC-26 / OP-55). | Добавил **§8 тест-стратегию** по слоям (U/NEG/BL/INT/QA/REG + UH/TA раздельно) и явную двух-PASS ноту: строитель — honest scaffold, а `PASS_AS_TRAINED_FEATURE` достигает обучаемый юнит ребра после блоков 10/11. |
| D5 | **Teacher-семейства/лицензии не сведены с каноном блока 08.** Бенчмарк перечисляет Qwen3/DeepSeek/Kimi/GLM/Mistral и лицензии MIT/Apache, но без привязки к реальному license-хелперу и к allowlist/denylist-реестру блока 08. | Привязал к реальному `IsAllowedCommonLicense` (`ResultGapDpoWriters.cs:192-196`, проверено чтением) и к `AllowedTeacherRegistry` (allowlist/denylist, блок 08 §3.5); добавил правило R2 «идентичность важнее флага». |
| D6 | **Счётчики/фазы не привязаны.** Бенчмарк не связывает рецепт с `Ф<P>.<SP>.<SS>` (12/26/59). | Явный якорь фаз (Ф3/Ф2.1/Ф11), счётчики 12/26/59, связь шагов рецепта ↔ подшаги доски. |

CANON-инварианты сохранены: маппинг ролей Luba → реальные контракты (мастер §A.3);
5 закрытых уровней (A-LM-12); легальный teacher-гейт (closed = только judge при
`used_for_training=false`, контаминация → `DATASET_REBUILD_REQUIRED`); запрет
скрытого детерминизма (A-LM-09/A-LM-13). Все цитаты кода — реально существующие,
перепроверены прямым чтением главного чекаута `C:/Projects/llm_mvp_core`. NEW-типы
помечены `NEW`; существующие — с `file:line`.

---

## 1. Назначение гайда

Этот гайд отвечает на один практический вопрос: **как из узла графа и из ребра
графа машинно построить DPO-пару (chosen / rejected / `[WRONG:]`)**, которую можно
подать в обучение L5-машины, и при этом не нарушить три класса правил:

1. **Анти-leakage** — пара не должна учить модель «угадывать по утечке»
   (target-label, путь к файлу/source-path, id источника, точный текст промпта
   exact-prompt, порядок/индекс кандидатов).
2. **Легальный teacher-гейт** — chosen/rejected как обучающая **цель** допустимы
   только от open-weight permissive teacher или из first-party/permissive
   источников; запрещены выводы Claude/OpenAI/Gemini как distillation-цели
   (только judge при `used_for_training=false`).
3. **Запрет скрытого детерминизма** — DPO-пара существует, чтобы обучаемый юнит
   **ПЕРЕКРЫЛ** детерминированный keyword/regex/lookup-baseline, а не закрепил его.

Гайд: (а) фиксирует, как DPO строится в коде репозитория **СЕЙЧАС**, с точными
ссылками `file:line`; (б) перечисляет **РАЗНЫЕ** форматы пар, реально найденные в
коде и на диске, и сводит их к **единому канону `GraphDpoPair` (14 полей)** для
графа; (в) даёт пошаговый рецепт авто-построения пары на узле и на ребре; (г) даёт
гейты, тест-стратегию и список пробелов, которые план обязан закрыть.

Этот документ **не разрешает** менять код, данные, чекпойнты. Любая DPO-строка,
батч или claim о качестве модели проверяется против этого гайда, против
`trainable-architecture-decision-SKILL.md` и против гайдов измерения, и не может
попасть в `Train*` без source/license-гейта, teacher-provenance, анти-leakage и
held-out разделения.

**Граница ответственности (CANON R5/R6).** Гайд описывает логику, которая живёт в
ядре `llm_mvp_core`, а не в агентном слое: строитель DPO — это
детерминированный нормализатор/валидатор внутри ядра. Агент/расширение его не
содержит и не дублирует (R6 — тонкий агент: только вызовы API).

---

## 2. Концепт простыми словами (без кода)

**Аналогия: экзамен с двумя ответами.**

Представь, что мы учим стажёра-архитектора принимать одно решение (например:
«какой ретривер выбрать для вопроса по wiki-проекту» — это решение на одном
**узле** графа). Мы не объясняем стажёру правило словами. Вместо этого мы кладём
перед ним один и тот же вопрос и **два готовых ответа**:

- **chosen** — правильный ответ (что и почему правильно);
- **rejected** — правдоподобно-неправильный ответ (выглядит разумно, но содержит
  одну конкретную ошибку);
- **`[WRONG: ...]`** — короткая подпись на полях rejected: какая именно ошибка и
  чем она плоха.

Стажёр много раз видит такие тройки и постепенно начинает сам предпочитать
chosen. В этом вся суть DPO (Direct Preference Optimization, прямая оптимизация
предпочтений): модель учится не на «правильном тексте», а на **разнице** между
хорошим и почти-хорошим.

**Три важных уточнения аналогии:**

- **«Почти-хорошо» важнее «совсем плохо».** Если rejected — это явный мусор
  («не знаю», пустой ответ, бред), стажёр выучит только «не будь мусором» — это
  бесполезно. Сильный сигнал даёт *near-miss*: rejected отличается от chosen ровно
  одной осмысленной ошибкой (выбрал семантический ретривер там, где нужен точный
  лексический; пропустил проверку версии; ответил без доказательств).

- **Граф = много таких экзаменов, соединённых трубами.** У нас не один узел, а
  поток узлов (splitter → translator → planner → processor → applier →
  result-translator → arbiter → formatter), где выход одного узла = вход
  следующего. Поэтому DPO-пары бывают двух видов:
  - **пара на узле** — «для входа X этого узла правильный выход chosen, а не
    rejected»;
  - **пара на ребре** — «при таком выходе предыдущего узла правильно
    передать/смаршрутизировать в узел A (chosen), а не в узел B (rejected)»;
    то есть тестируется само решение о маршруте/передаче между узлами.

- **Учитель эталона и судья — разные роли.** Кто написал «правильный образец»
  (chosen) — это **teacher** (учитель эталона). Кто только сказал «этот образец
  лучше того» — это **judge** (судья). Закрытую модель (Claude/OpenAI/Gemini)
  можно звать **только судьёй** и **только** когда её вывод не идёт в обучение;
  как автора эталона — никогда. Это и есть легальный teacher-гейт (§6.2).

**Что НЕ является DPO-парой.** Пара «вход → правильный выход» без rejected — это
supervised (teacher-forcing), а не DPO. Такие ряды (формат `A → узел → B` из
`learning-sets`) — это сырьё; чтобы стать DPO, к ним надо ДОстроить rejected по
правилам ниже (§4 шаг 4, §7 пункт 7).

---

## 3. Как это реализовано в нашем коде СЕЙЧАС (точные ссылки)

Здесь — что **РАБОТАЕТ** и что **ЗАГЛУШКА**. Все ссылки перепроверены прямым
чтением файлов главного чекаута `C:/Projects/llm_mvp_core`.

### 3.1. Движок одного DPO-шага — РАБОТАЕТ (но это не построитель пар)

`11.DPO` — низкоуровневый тренировочный движок. Он **НЕ** строит пары; ему пары
подают извне.

- Контракт пары: `11.DPO/Models/DpoPair.cs:3-8` — `record DpoPair { Prompt;
  Chosen; Rejected; }` (проверено: ровно три строки `init`, по умолчанию пустые).
  **Нет** полей source / license / teacher / near-miss / domain. Это минимальный
  «голый» формат.
- Загрузка набора: `11.DPO/DpoDataset.cs:18-26` — `Load(path)` читает JSONL по
  строке на `DpoPair`, пустые строки пропускает, `null` → исключение;
  `Shuffle(seed)` (строки 28-38) — Fisher-Yates, исходник не мутируется.
- Лосс и шаг обучения — РЕАЛЬНЫЕ: `11.DPO/DpoTrainer.cs:31-70` (`Step`), проверено
  чтением:
  - `chosenReward = Beta*(polChosenLogP − refChosenLogP)`,
    `rejectedReward = Beta*(polRejectedLogP − refRejectedLogP)` (строки 42-43);
  - `margin = chosenReward − rejectedReward` (строка 44);
  - `loss = −log(σ(margin)+1e-8)` (строка 47);
  - ручной backward `gradMargin = σ(margin) − 1` (строка 50), затем
    `_optimizer.Step(..., lr: _learningRate)` (строка 54);
  - метрики `DpoMetrics { Loss, ChosenReward, RejectedReward, RewardMargin,
    Accuracy = margin>0 ? 1f : 0f }` (строки 57-65).
- ЗАГЛУШКА/ПРОБЕЛ движка: нет сохранения весов как checkpoint/manifest/locator,
  нет export/import, fresh-load, continue-training, held-out, нет CPU/RAM/hash в
  метриках. Прямых вызовов `DpoTrainer` из боевого рантайма (15/42/43) не найдено —
  пары пока строятся, но в этот движок в проде не подаются.

**Вывод по 3.1:** `DpoTrainer` — честный «кирпич» применения сигнала
(`OK_INTERNAL_DETAIL` / `OK_MODEL_COMPONENT` по скилу), НО он диктует, что
авто-построитель графа обязан выдавать именно `(Prompt, Chosen, Rejected)` в
токенизируемом виде. Поэтому **конечная точка** канона `GraphDpoPair` — проекция в
`LearningTrainingItem` → JSONL `DpoPair` → `DpoDataset.Load` (§4 шаг 7).

### 3.2. Построение пары на узле — РАБОТАЕТ (детерминированный построитель)

Канонический работающий пример — `WikiContextRoutingDpoGenerator`
(`42.ProjectWikiDomain/WikiContextRoutingDpoGenerator.cs`). Это узел
«контекст-роутинг wiki» (выбор Mode2 semantic / Mode3 BM25 / abstain).

Как строится пара (метод `BuildBatchAsync`, строки 17-94):
1. Из seed-вопроса собирается `ProjectWikiUserQuestion`, прогоняется через
   `ProjectWikiQueryPlanningFlow.PlanAsync` и `WikiContextRoutingUnit.Route`
   (строки 46-62) — то есть **chosen берётся из реального выхода юнита**, а не
   придумывается.
2. `BuildChosen(...)` (строки 96-212) — формирует chosen как строки `key: value`
   (например `route_kind`, `requires_semantic_retriever`, `prechecks_complete:
   true`, `answer_before_retrieval: false`).
3. `BuildRejected(rejectedType, ...)` (строки 214-290) — формирует
   правдоподобно-неверный ответ по типу ошибки и добавляет подпись
   `[WRONG: <type> <последствие>]`. Реальные типы rejected в коде: `always_bm25`,
   `always_semantic`, `answer_instead_of_abstain`, `ignore_wrong_project`,
   `ignore_stale_version`, `no_pre_checks`.
4. Гейты качества батча (строки 75-86): `wrongAnnotationRate` (доля пар с
   `[WRONG:`) должна быть = 1.0; иначе варнинг `wrong_annotation_rate_below_gate`;
   контроль `batch_row_count_not_50` и `developer_reasoning_rows_below_40_percent`.

В `42.ProjectWikiDomain` есть 12 таких `*DpoGenerator.cs` (по Glob):
`CodeTraceBuilderDpoGenerator`, `WikiContextRoutingDpoGenerator`,
`ProjectWikiAnswerComposerDpoGenerator`, `StackAndVersionDpoGenerator`,
`SystemBoundaryMapperDpoGenerator`, `ProjectWikiQueryPlanningDpoGenerator`,
`CrossStackBusinessFlowGraphDpoGenerator`, `WikiTraceAggregatorDpoGenerator`,
`DocumentationGeneratorDpoGenerator`, `ProjectAnswerVerifierDpoGenerator`,
`EntityImpactMapperDpoGenerator`, `ProjectWikiRequestRoutingDpoGenerator` — это по
сути «узлы графа wiki», каждый со своим построителем пар.

**Вывод по 3.2:** построение пары на узле уже работает как детерминированный
fixture-builder. Это `OK` как генератор датасета, НО chosen здесь — self-label
(выход того же детерминированного юнита), значит teacher-источник истинности
детерминирован — это требует ablation/no-shortcut и легального teacher при
переходе к семантическому claim (см. §6.3, §7 п.4).

### 3.3. Построение пары на ребре / grounding — РАБОТАЕТ (корпус оркестрации)

`OrchestrationCorpusBuilder` (`42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs`)
строит пары для маршрутно-grounding-решений (это ближе всего к «ребру графа»):

- Запись пары: `OrchestrationTrainingPair`
  (`42.ProjectWikiDomain/OrchestrationPlanContracts.cs:110-122`) — поля `PairId,
  Mode, Prompt, Chosen (OrchestrationPlanChosen), RejectedText, ExpectedRouteMode,
  ExpectedBoundarySource, ExpectedMlGenerated, ExpectedIntentKind, FlowRef,
  SourceBundleId, IsHeldOut`.
- Три режима (`PairMode`): `Mode1LearnedFlowGrounding`, `Mode2Generalization`,
  `Mode3HumanExplanation` (методы `AddMode1Pairs`/`AddMode2Pairs`/`AddMode3Pairs`,
  строки 128-241). Пример rejected (строка 144): `Rejected route uses
  deterministic lookup instead of learned grounding.`
- Анти-leakage в корпусе: `NoExactPromptOverlap` (строки 243-254) + нормализация
  промпта `NormalizePrompt` (строки 256-262) — train/eval не должны делить
  одинаковые промпты; дополнительно проверяется disjoint по `SourceBundleId` и
  `FlowRef` (строки 75-77).
- Гейт корпуса `MeetsCorpusGate` (строки 81-93): минимальные счётчики по режимам +
  все overlap-флаги.

**Вывод по 3.3:** edge/grounding-пары и их анти-overlap-гейты уже реализованы.
ПРОБЕЛ: `OrchestrationTrainingPair` не несёт teacher-provenance и license-полей.
Канон `GraphDpoPair` (§6.5) добавляет их именно поверх таких форматов.

### 3.4. KNN-память узла — РАБОТАЕТ как компонент (не финальная authority)

`OrchestrationKnnStore` (`42.ProjectWikiDomain/OrchestrationKnnStore.cs`):
`FindNearestWithScore` (строки 32-43) ищет ближайшую обучающую пару по **Jaccard**
над токенами промпта (`Tokenize` строки 45-50, `Jaccard` 52-62); есть
`ExportPairs`/`ImportPairs` (строки 19-27).

Классификация по скилу: KNN — audit-triggered, не запрещён. Это допустимый
**компонент памяти** обучаемого узла (`OK_MODEL_COMPONENT`), пока он не становится
**единственной финальной authority** вместо обученного юнита. Для графа:
nearest-память поддерживает решение, но не заменяет обучаемый сигнал (и не
переносит source-path как сигнал — §6.1).

### 3.5. Легальный гейт источника и анти-leakage схемы — РАБОТАЕТ (самый зрелый код)

Самая зрелая легальная реализация — `43.SmartSkillsDomain/ResultGapDpoWriters.cs`
(узел ResultGap «обнаружение недостающей способности после ответа»):

- Дескриптор источника: `ResultGapSourceDescriptor` (`ResultGapDpoWriters.cs:7-14`)
  — `SourceKind, LicenseExpression, PrivacyScope, SourceRef, SourceSha256,
  SourceVersionId, UserApprovalRecorded`. Это реальный прецедент провенанса в
  проекте; канон `TeacherProvenance` (20 полей, блок 08) его **расширяет**, а не
  дублирует.
- Роутер приватности/лицензии `ResultGapSourcePrivacyRouter.Classify` (строки
  111-153): пустой источник → quarantine `source_identity_missing`;
  vendor/external/unknown → quarantine `external_vendor_or_unapproved_source`
  (правило `IsExternalOrVendor` строки 182-186); `forbidden`/`raw_private` privacy
  → quarantine `privacy_scope_forbidden` (правило `IsForbiddenPrivacy` строки
  188-190); private/correction → `adaptive_only`; approved common + разрешённая
  лицензия → `trainable_common`.
- Разрешённые common-лицензии `IsAllowedCommonLicense` (строки 192-196, проверено
  чтением): **MIT, Apache-2.0, ProjectInternal, Synthetic** (через
  `OrdinalIgnoreCase`). Это **точка расширения** шага 1 рецепта (§4) и шага 4
  гейта блока 08.
- Схема-валидатор анти-leakage `ResultGapDpoSchemaValidator.Validate` (строки
  241-266, проверено чтением): требует `chosen_ref`/`rejected_ref`/`skill_ids`, и
  явно ловит утечку — `Require(!ChosenTargetContainsRawPrivateOrVendorOutput,...)`,
  `!ContainsRawPrivateText`, `!ContainsRawVendorOutput`,
  `!CommonCheckpointMutationClaimed`, `TechnicalExecutionRefsExcluded`,
  `!SourceIdUsedAsTrainingFeature`, near-miss-таксономия train-ready,
  `RejectedWhyPlausible.Length >= 24` (строки 250-257); `CheckedFieldCount: 14`
  (строка 262).
- Near-miss таксономия `ResultGapNearMissTaxonomy` (строки 199-235, проверено
  чтением): коды `missing_required_capability_after_output`,
  `false_done_with_gap_remaining`, `correction_overfit_to_raw_answer`
  (`TrainReady: true`) и `generic_bad_output` (`TrainReady: false`, только
  quarantine).

**Вывод по 3.5:** анти-leakage и license-роутер реализованы и являются **образцом**
для графа. ПРОБЕЛ: здесь нет полей teacher-провенанса (`teacher_access_mode`,
`closed_proprietary_model`, `judge_model` и т.д.) — см. §6.2 и §7 п.1.

### 3.6. Что на диске (реальные форматы строк)

Проверены первые строки файлов (рекон-сводки `codex-recon-*`, evidence):

- `training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl`:
  **ref-only** формат — `pair_id, source_row_id, approved_pair_ref, error_code,
  chosen_ref, rejected_ref, source_ref, source_sha256, model_leg,
  abstraction_layer, universal_reasoning_pattern, excluded_technical_execution_refs,
  raw_private_text_copied:false, technical_execution_leakage:false,
  dpo_binding_ready, dpo_materialized, dpo_train_ready:false, training_started:false`.
  Строка несёт SHA256 источника и `excluded_technical_execution_refs`, а
  `dpo_train_ready=false` — честное «ещё не обучаемо».
- `training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed.jsonl`:
  формат с `prompt_input_task_context` + `chosen_ref/chosen_summary/rejected...`,
  `source_quality:"gold"`, `machine_target:"PromptToTaskMachine"`,
  `private_or_project_specific_text_copied:false`.
- `training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl`:
  payload-формат с `chosen_scenario_plan_features` (intent_signals, reasoning_steps,
  evidence_expectations, decision_guards) — то есть пара, разложенная в признаки, а
  не в сырой текст.

> **КЛЮЧЕВОЙ ФАКТ (рекон, evidence-only).** Поля `teacher_access_mode`,
> `closed_proprietary_model`, `hosting_provider`, `teacher_output_used_for_training`
> встречаются в файлах скилов/гайдов (`docs/skills/*`) и **отсутствуют** в `.cs` и
> в строках `training-data/*.jsonl`. То есть легальный teacher-гейт пока существует
> как ПРАВИЛО, но не как поле данных — это обязательный пробел плана (§7 п.1).
> **Честность по доказательствам (CLAUDE.md «No Dishonest Answers»):** я не запускал
> live-grep в этой сессии для пере-подсчёта вхождений; число «19 файлов/72» из
> рекона держу как benchmark-sourced ориентир, а не как текущий факт диска —
> Ф2.1.3 обязан пере-сканировать на момент исполнения (синхрон с блоком 08 §6.3).

---

## 4. Пошаговый рецепт авто-построения DPO-пары

Рецепт един для узла и ребра; различие — в шаге 2 (что является входом A) и шаге 4
(что варьируется в rejected). Это **методическая** проекция алгоритма
`GraphDpoBuilder.BuildPairAsync` (блок 07 §3.3, NEW). Соответствие подшагам доски:
шаги 1-6 ↔ Ф3.1.1–Ф3.1.3 (адаптеры+гейты), шаг 7 ↔ Ф3.2.1–Ф3.2.2 (триада+
материализация).

**Шаг 0. Классифицировать цель по разрешённому уровню.**
Назвать узкую способность и сопоставить её ровно одному разрешённому уровню:
`ILogicalUnit` (узел), `ILogicalFlow` / `ILogicalOrchestrator`
(последовательность/маршрут), `ILearningMachine` / `ILearningMachinePair`
(владелец обучения). scorer/selector/router/KNN/regex — **НЕ** уровень, только
деталь внутри уровня (`trainable-architecture-decision-SKILL.md`, allowed levels;
A-LM-12 — закрытый список из 5 уровней).

**Шаг 1. Зафиксировать и проверить источник (legal gate ПЕРЕД построением).**
Заполнить дескриптор источника (как `ResultGapSourceDescriptor`,
`ResultGapDpoWriters.cs:7-14`): `source_ref`, `source_sha256`, `license`,
`privacy_scope`, плюс teacher-provenance (§6.2, 20 полей). Прогнать через
`TeacherProvenanceGateUnit.Evaluate` (блок 08) → вердикт `ACCEPT / QUARANTINE /
BLOCK / DATASET_REBUILD_REQUIRED / BLOCK_FOR_COMMON / EVAL_ONLY`. Если не ACCEPT — пара **не
строится для общего обучения** (но честно помечается, не выбрасывается молча).

**Шаг 2. Сформировать A (вход).**
- Узел: реальный рантайм-вход узла (для роутинга — пользовательский вопрос; для
  array-узла — один элемент). Правило `learning-sets`: НЕ класть строку документа,
  если человек так бы не написал.
- Ребро: выход предыдущего узла + контекст маршрутизации (`candidateUnitIds`,
  `requiredDirection`, флаги вроде `UseKnowledgeEdgeDetection`).
- Анти-leakage: A **не должен** содержать порядок/индекс кандидатов как сигнал, id
  источника, точный путь файла (source-path).

**Шаг 3. Получить chosen.**
Предпочтительно — из легального teacher (open-weight permissive) или из first-party
канона (выход проверенного юнита, как в `WikiContextRoutingDpoGenerator`). chosen
конкретен: реальное решение/маршрут/выход, не «составь план».

**Шаг 4. Построить rejected как near-miss + `[WRONG:]`.**
Выбрать один тип ошибки из таксономии узла (например `always_semantic`,
`ignore_stale_version`, `no_pre_checks`). rejected отличается от chosen **РОВНО
одной** осмысленной ошибкой и остаётся правдоподобным. Добавить `[WRONG: <код>
<короткое функциональное последствие>]`.
Запрет: rejected-мусор; отрицание-шаблон («Do not generate a capability for
sub-phase X» — именно он дал провал калибровки 2026-05-06, overlap ~0.70,
loss=ln2; см. `codex-recon-mvp-internal-learn.md`).

**Шаг 5. Проверить overlap и инвариантность.**
- `overlap_ratio(chosen, rejected) < 0.60` (порог из внешнего реестра пар; синхрон
  с блоком 07 §3.3 шаг 5).
- train/eval не делят промпт (`NoExactPromptOverlap`,
  `OrchestrationCorpusBuilder.cs:243-254`), а для ребра-выбора —
  permutation-инвариантность: chosen стабилен при перестановке кандидатов
  (`candidate_order_invariant=true`), иначе `QUARANTINE`
  (`rejected_origin="order-artifact"`, `dpo-triple-format-SKILL.md`).

**Шаг 6. Прогнать схема-валидатор анти-leakage.**
Как `ResultGapDpoSchemaValidator` (`ResultGapDpoWriters.cs:241-266`): запретить raw
private/vendor текст в chosen-цели, `source_id` как признак, technical execution
refs; near-miss код обязан быть train-ready; `RejectedWhyPlausible.Length >= 24`.

**Шаг 7. Разложить в обучаемую форму и измерить.**
Свести к токенизируемой паре: проекция канона `GraphDpoPair` →
`LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`,
`ItemId←pair_id`, `Prompt←input_context`, `ChosenResponse←chosen`,
`RejectedResponse←rejected`) → JSONL `DpoPair` → `DpoDataset.Load`
(`11.DPO/DpoDataset.cs:18`). Считать `reward_margin = logP(chosen) − logP(rejected)`
≥ 0 и `loss` < 0.685 (≥0.693 = random-guess = брак,
`TEACHING-MEASUREMENT-GUIDANCE-v6`). Held-out 50+50 disjoint по источнику
(`dcm-dpo-construction-GUIDE.md` §7).

**Шаг 8. Статус честно (двух-PASS).**
Пока нет held-out метрик + ablation + fresh-load + continue-training через
checkpoint/manifest/locator — статус строки `dpo_train_ready=false` /
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, **НЕ** `PASS_AS_TRAINED_FEATURE` (CLAUDE.md
«Untrained honesty vs trained readiness pattern»).

---

## 5. Конкретные примеры с реальными значениями

### 5.1. Пара на УЗЛЕ (контекст-роутинг wiki) — реальный шаблон из кода

Вход A (узел `WikiContextRoutingUnit`, вопрос разработчика):
> "I want to add DiscountCode to CampaignCreateDto. Where does it travel and what
> can break?"

chosen (compact-форма из `BuildChosen`, строки 161-170):
```text
context_route_verdict: correct_project_wiki_context_route
route_kind: Mode2SemanticHybrid
intent: EntityImpactTrace
requires_semantic_retriever: True
allows_lexical_fallback: True
prechecks_complete: true
answer_before_retrieval: false
```

rejected + `[WRONG:]` (тип `always_bm25`, строка 233):
```text
context_route_verdict: wrong_project_wiki_context_route
route_kind: Mode3LexicalEvidence
requires_semantic_retriever: false
[WRONG: always_bm25 misses multi-hop developer reasoning where semantic
expansion is required after the query plan identifies many evidence families.]
```

Чему учит пара: для multi-hop entity-impact вопроса нужен семантический ретривер;
выбор «всегда BM25» — правдоподобная, но неверная экономия. Анти-leakage: chosen и
rejected отличаются режимом ретривера, а не id вопроса/порядком кандидатов.

### 5.2. Пара на РЕБРЕ (grounding-маршрут оркестрации) — из OrchestrationCorpusBuilder

Запись `OrchestrationTrainingPair` (Mode1, строки 134-146):
```text
PairId:   mode1-train-flow-33
Mode:     Mode1LearnedFlowGrounding
Prompt:   "learned grounding plan FLOW-33 flow 33 33-star canonical orchestration"
Chosen:   <OrchestrationPlanChosen для FLOW-33>
Rejected: "Rejected route uses deterministic lookup instead of learned grounding."
FlowRef:  FLOW-33   IsHeldOut: false
```
Чему учит: на этом ребре правильно выбрать обученный grounding, а не
детерминированный lookup. Это прямой анти-shortcut-сигнал (R4 / запрет скрытого
детерминизма / A-LM-09). В каноне `GraphDpoPair` такой rejected получает
`rejected_origin="baseline-loser"` (§6.5, блок 07 §3.4).

### 5.3. Ref-only строка на диске (orchestration-reasoning) — реальная первая строка
```text
pair_id: or-dpo-pair:or01u-sonnet-clinerules-001
chosen_ref:  or01u:chosen:governance-intent-risk-linkage
rejected_ref: or01u:rejected:surface-summary-without-evidence
source_ref:  sonnet-local-analyzes:.clinerules-analysis.md
source_sha256: af174a555ca9...fb05
model_leg: common
excluded_technical_execution_refs: [exact_repo_paths, patch_steps, host_specific_commands, code_bound_file_edits]
raw_private_text_copied: false   technical_execution_leakage: false
dpo_train_ready: false   training_started: false
```
Замечание: имя источника содержит `sonnet` — это сигнал, что происхождение
chosen/rejected надо проверить teacher-гейтом (§6.2) ДО `dpo_train_ready=true`.
Если `sonnet` использован как teacher (`used_for_training=true`) → вердикт
**`DATASET_REBUILD_REQUIRED`** (блок 08 §6.2, ветка C3).

### 5.4. Negative-пример (что НЕ принимать)
```text
chosen:   route_kind: Mode2SemanticHybrid ... (правильно)
rejected: "I don't know."                 <-- МУСОР, не near-miss
```
Вердикт: `QUARANTINE` (`rejected_origin="generic_bad_output"`, train-ready=false по
`ResultGapNearMissTaxonomy`, `ResultGapDpoWriters.cs:221-226`). rejected не
правдоподобен, overlap почти 0, ничему не учит, кроме «не будь пустым».

---

## 6. Гейты и правила

### 6.1. Анти-leakage (обязательные запреты в признаках/целях)

Запрещены как обучающие признаки: target-label напрямую, **source-path / id
источника**, **точный текст промпта (exact-prompt replay)**, **порядок/индекс
кандидата**, имена пользователей/проектов/сессий, raw private/vendor текст в
chosen-цели. Реализованные проверки-образцы: `ResultGapDpoWriters.cs:250-257`
(privacy/raw/source-id), `OrchestrationCorpusBuilder.cs:243-262` (exact-prompt
overlap), permutation-инвариантность из `dpo-triple-format-SKILL.md`.

> **Образец «как надо» (проверено в мастере §A.4.2).** `SubPhaseRouterSourceDocument`
> объявлен с полем `SourcePath` (`21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs:6`),
> а действующий роутер уже **намеренно исключает** этот путь из скоринга
> (`MultiSubPhaseRouterUnit.cs:293` «Join semantic document fields while
> deliberately excluding SourcePath»). То есть инвариант «не учить по пути
> источника» уже частично реализован; вход обучаемого юнита ребра обязан наследовать
> его как **контракт**, а не как комментарий.

Допустимы: нормализованный вход, абстрагированные дефекты/признаки,
nearest-память без утечки пути (§3.4).

### 6.2. Легальный teacher-гейт (provenance) — обязателен, в коде ОТСУТСТВУЕТ

chosen/rejected как обучающая **цель** допустимы только когда teacher — разрешённая
open-weight permissive модель ИЛИ first-party/permissive источник, и проверены
provider terms + лицензия.

- **Разрешённые teacher-семейства (allowlist, open-weight, нейтральные провайдеры):**
  Qwen3 (Apache-2.0), DeepSeek (MIT), Kimi K2 (modified-MIT), GLM, Mistral. Канон
  allowlist/denylist хранится как **данные** в `AllowedTeacherRegistry` (блок 08
  §3.5), решение по идентичности `IsClosedByDenylist(model_id)` читает denylist, не
  хардкодит бренды в логике.
- **ЗАПРЕЩЕНЫ как distillation-teacher (denylist):** Claude, OpenAI/GPT, Gemini,
  Gemini Grounding. Если `closed_proprietary_model=true` (выведен из denylist) И
  `teacher_output_used_for_training=true` → вердикт **`DATASET_REBUILD_REQUIRED`**,
  **без** легальных исключений (`trainable-architecture-decision-SKILL.md` Legal
  Teacher Gate; `plan-review-SKILL.md` DPO/distillation review gate; никакое
  «разрешение Luby» это не снимает). Закрытые модели допустимы только как
  **judge/eval/критика** при `teacher_output_used_for_training=false`, не как
  chosen/rejected-цель.
- **Правило R2 (идентичность важнее флага):** «закрытость» выводится из denylist по
  `model_id`, а **не** из входного флага `closed_proprietary_model`. Отсутствие
  маркера ≠ open_weight; пустой провенанс ≠ ACCEPT (минимум QUARANTINE).
- **Обязательные provenance-поля строки (канон 20 полей, владелец — блок 08; сейчас
  их нет ни в коде, ни в jsonl):**
  ```text
  13 базовых: teacher_access_mode; hosting_provider; model_id; model_license;
              model_version; request_id; provider_terms_checked;
              teacher_output_used_for_training; closed_proprietary_model;
              source_ref; source_sha256; contamination_markers; judge_model
  5 NEW:      terms_snapshot_ref; geography_export_constraint; source_category;
              raw_private_text_copied; redaction_policy_ref
  2 RR6:      service_mode; grounding_used   (обязательны; пустые ⇒ presence_rate<1.0 ⇒
              строка НЕ train-ready, R4; для train service_mode=="plain" И grounding_used==false)
  ```
  `judge_model` — поле **ВНУТРИ** `TeacherProvenance`, не отдельное верхнеуровневое
  поле пары (канон блока 08 §3.1).
- Лицензии общего корпуса (совпадают с кодом `IsAllowedCommonLicense`,
  `ResultGapDpoWriters.cs:192-196`): MIT, Apache-2.0, ProjectInternal, Synthetic.
  CC BY 4.0 — только с атрибуцией в метаданных. Запрещены для общего корпуса: CC
  BY-SA, GPL/AGPL, unknown/unlicensed. Instruction-tuning с model-output риском
  (ShareGPT/WizardLM/Alpaca/Dolly/UltraChat/FLAN-raw) — evidence-only/quarantine
  (`codex-recon-mvp-external-sources.md`).
- Правило first-party: ассистент-ответ внешнего чат-корпуса даёт только
  A-side-форму запроса (`a_side_style_only`); chosen ВСЕГДА строится из
  XIIGen-контрактов/кода/одобренных стандартов, не копируется из чужой модели.
- Источники приватных правок Любы/проектов = user-adaptive (personal leg), не
  common (R5). Hosted + raw private text без редакции → `BLOCK_FOR_COMMON` (только
  adaptive leg, блок 08 §8.5).

### 6.3. Запрет скрытого детерминизма (R4 / A-LM-09 / A-LM-13)

DPO-пара обязана учить обучаемый юнит **ПЕРЕКРЫТЬ** детерминированный baseline, а
не закрепить его. Системный риск зафиксирован реконом: `26.TrainableRouter`
(`KeywordUnitScorer.Score`) — это keyword-counter под именем «Trainable router»
(`codex-recon-core-26-router.md`, вердикт `FORBIDDEN_RUNTIME_SHORTCUT`).
Дополнительно `MultiSubPhaseRouterUnit` (`MultiSubPhaseRouterUnit.cs:283`,
фикс-веса 0.55/0.25/0.20/0.02, обученного состояния нет) — корректная
классификация `BASELINE_MUST_BE_LABELLED` (мастер §A.2.1).

Правило: keyword/regex/lookup допустимы **только** помеченными eval-only/baseline;
на каждом ребре графа назвать владельца финальной authority (`EdgeAuthorityUnit`,
NEW) и приложить ablation/no-shortcut тест. Строки данных не должны учить тому, что
запрещено в рантайме: пара с `order-artifact` → карантин.

### 6.4. Гейты измерения (что = «обучилось»)

- `reward_margin ≥ 0` и растёт; `loss < 0.685` (≥0.693 = random = брак).
- `overlap_ratio(chosen, rejected) < 0.60`; train/eval disjoint по промпту и
  источнику (50+50, disjoint by source).
- Для семантического claim обязательны: held-out accuracy, ablation без checkpoint,
  permutation/leakage-проверки, fresh-load воспроизведение, continue-training из
  существующего checkpoint. Иначе статус — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` /
  `OPEN`, не `PASS_AS_TRAINED_FEATURE` (CLAUDE.md «Untrained honesty vs trained
  readiness pattern»; детали — `claude-05` и блок 11).
- Anti-masquerade: недоступные числа = `UNKNOWN`, не выдуманные (образец —
  `ProjectWikiTrainingFlowAdapter`, рекон core-42). Захардкоженный `RewardMargin`
  (литерал 0.03 в `27.TranslationDomain/TranslationMachine.cs:70/125/187`) —
  сломанный эталон, устраняется (R-19, блок 10/11).

### 6.5. Канон формата DPO для графа (РАЗНЫЕ форматы → один `GraphDpoPair`, 14 полей)

Найдены **5 разных** форматов: (1) голый `DpoPair{Prompt,Chosen,Rejected}`
(`11.DPO/Models/DpoPair.cs:3`); (2) `RoutingDpoPair`
(`26.TrainableRouter/Models/RoutingModels.cs:75`); (3) `OrchestrationPromptDpoPair`
(`26.TrainableRouter/Training/OrchestrationPromptDpoPairBuilder.cs:54`); (4)
`CodeTraceBuilderDpoPair` с `[WRONG:]` (`42.../CodeTraceBuilderDpoGenerator.cs:35`);
(5) `CrossStackBusinessFlowGraphDpoPair` с `RejectedType`/`EvidenceIds`
(`42.../ProjectWikiDomainModels.cs:918`).

> **Исправление D2 (консистентность канона).** Бенчмарк `codex-guide-01` §6.5
> предлагал «объединить» ~25 разнородных полей с **плоским** `teacher_provenance_ref`
> и полями вроде `target_level`/`curriculum_tier`. Это **противоречит** фиксированному
> канону всех `claude-*` (мастер §A.4, блок 07 §3.2). Я выравниваю на **канон 14
> полей** с **вложенным** `TeacherProvenance` (20 полей) — это убирает класс ошибки
> «строка train-ready, а провенанс не полон».

**КАНОН `GraphDpoPair` — ровно 14 полей (владелец — блок 07; провенанс вложен):**
```text
1  pair_id
2  level                       // unit | edge | graph
3  node_or_edge_id
4  input_context               // вход A (узел: рантайм-вход; ребро: выход предыдущего узла)
5  chosen
6  rejected
7  wrong_markers[]             // [WRONG: code consequence] (канон dpo-triple)
8  reason                      // почему chosen лучше (≥24 символа)
9  teacher_provenance          // ВЛОЖЕННЫЙ TeacherProvenance (20 полей; владелец — блок 08)
10 source_ids[]                // провенанс источников (id юнитов/evidence переезжают сюда)
11 held_out                    // строка попала в экзаменационный сплит?
12 candidate_order_invariant   // выбор устойчив к перестановке кандидатов?
13 excluded_leakage_signals[]  // [candidate_index, source_path, exact_prompt]
14 dpo_train_ready             // GATE: true только при пройденной триаде (§6.6)
```
Сопутствующие рабочие поля строителя (`rejected_origin`, `leg_scope`,
`overlap_score`, `row_rejection_reason`, `split_proof_ref`) — это поля черновика/
вердикта строителя (блок 07 §3.2), не часть 14-польного канона строки; в финальной
строке они отражены через `dpo_train_ready` + `teacher_provenance` + журнал
строителя.

**Хранение без правки контрактов 00.Core:** проекция `chosen/rejected/input_context`
на `LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`) + боковой
DPO-манифест для остальных полей, связанный через `SourceIds`
(`00.Core/LearningMachineContracts.cs:26`).

**Таксономия `rejected_origin` (откуда берётся «худший» вариант; блок 07 §3.4):**
`random-rejected` (случайный валидный из другого контекста), `copied` (дубликат
chosen с внесённой ошибкой `[WRONG:]`, маркер обязателен), `missing-source` (ответ
без опоры на evidence), `teacher-unknown` (без провенанса → блокируется гейтом 1),
`split-leak` (near-duplicate held-out → карантин), `order-artifact` (зависит от
позиции → карантин), `baseline-loser` (проигравший выход baseline-генератора —
ablation-данные, мост к §5.2 и блоку 06).

### 6.6. Канон-enum вердикта строки и предусловие train-ready (исправление D3)

**Вердикт-enum (владелец — блок 08 §3.3) — 6 значений:**
```text
ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY
```
`EVAL_ONLY` (NEW, фикс R17, блок 08 §3.3/§8.6) — легально-чистая, но train-ineligible
строка (неполная триада C15/C15b или невоспроизводимый конверт C8): отдельный отвергаемый
вердикт, **не** `ACCEPT`.
**`LegScope` (4 значения):** `CommonGlobal | PersonalAdaptive | EvalOnly | Quarantine`.

**Предусловие `dpo_train_ready=true`** (триада, блок 07 §3.3 / блок 08 §2.2):
```text
dpo_train_ready =
     legal_verdict == ACCEPT
  && provenance_field_presence_rate == 1.0   (все 20 полей TeacherProvenance)
  && triad_present == true                    (raw_comparison_event +
                                               legal_provenance_dpo_envelope +
                                               training_batch — Graph-DPO Artifact
                                               Triad Gate)
  && overlap_score < 0.60
  && len(reason) >= 24
  && candidate_order_invariant == true
```
ACCEPT **без** полной триады ⇒ строка остаётся **eval-only**
(`dpo_train_ready=false`). Инвариант: `train_ready_without_triad_count == 0`.

---

## 7. Пробелы / что нужно построить в плане

1. **Teacher-provenance не реализован в данных.** Поля `teacher_access_mode /
   closed_proprietary_model / hosting_provider / judge_model / ...` есть только в
   скилах, нет ни в `.cs`, ни в `training-data/*.jsonl`. План обязан добавить их как
   обязательные поля строки (20 полей, блок 08) и как гейт перед
   `dpo_train_ready=true`. Особый риск: на диске есть строки с `source_ref:
   sonnet-local-analyzes:*` — происхождение chosen/rejected надо доказать против
   Legal Teacher Gate (Claude/OpenAI/Gemini как teacher → rebuild).
2. **Нет единого `GraphDpoPair`.** 5 несовместимых форматов (§6.5). План должен
   утвердить канон 14 полей и адаптеры из каждого старого формата (блок 07 Ф3.1.1-3).
3. **Граф как уровень отсутствует.** В коде нет `ILearningDynamicGraph`,
   many-to-many/fan-out/fan-in/multi-output edge contracts. Сейчас — линейные
   sub-phase + flow-фабрики (рекон core-18/core-42). План обязан реконструировать
   рёбра по Graph-Style Reconstruction Gate: на каждом ребре назвать владельца
   authority (`EdgeAuthorityUnit`, NEW) и hidden-shortcut-проверку (блок 02/03/12).
4. **chosen — self-label / детерминирован** в узловых генераторах
   (`WikiContextRoutingDpoGenerator`, `26.TrainableRouter`). Нужен легальный teacher
   как источник истинности + ablation, иначе DPO закрепит детерминизм (§6.3).
5. **Lifecycle обучаемого узла не закрыт.** `DpoTrainer` без
   checkpoint/export/import/locator/fresh-load/continue-training;
   `26.TrainableRouter` без реального DPO-цикла (поля
   `RewardMargin/DpoAccuracy/Promoted` — плейсхолдеры). План обязан добавить полный
   asset-lifecycle на каждый обучаемый узел/ребро (CLAUDE.md «Trainable Unit
   pattern»; блок 10).
6. **Permutation/ablation-гейты на ребре не доказаны.** Есть exact-prompt overlap,
   но нет permutation-инвариантности выбора маршрута и no-shortcut-теста в коде
   (блок 02/12).
7. **Supervised-ряды ≠ DPO.** Корпус `learning-sets` (формат `A→узел→B`) —
   supervised; материал для rejected (failure/feedback/arbiter-flag) есть, но не
   сведён в тройки (`codex-recon-mvp-learning-sets.md`). План должен задать правило
   сборки тройки из этих блоков (§4 шаг 4, блок 06).
8. **Расхождение указателей версий** train-ready (V192 vs V309) в «internal learnin
   2» — определить канонический train-ready перед обучением (R-18, блок 07/09).

---

## 8. Тест-стратегия гайда/рецепта (axis верификации, OP-55 / FC-26) — улучшение D4

Рецепт описывает **детерминированный** строитель данных, а не обученную модель.
Поэтому у самого строителя нет `PASS_AS_TRAINED_FEATURE`; правильный приёмочный
класс — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (строитель честно помечен как
нормализатор/валидатор, скрытого детерминизма «под видом обучения» в нём нет).
`PASS_AS_TRAINED_FEATURE` достигает **обучаемый юнит ребра** (`EdgeAuthorityUnit`)
на построенных строках — после блоков 10/11 (фазы Ф4-Ф5).

| Слой | Что проверяем (для рецепта/строителя) | Привязка |
|---|---|---|
| **U** (unit) | каждый шаг рецепта даёт ожидаемый исход; `overlap<0.60`, `reason≥24`, 18/18 presence; enum-домены вердикта/LegScope | §4, §6.6 |
| **NEG** (negative) | мусор-rejected ⇒ QUARANTINE (`generic_bad_output`); denylist-`model_id` ⇒ BLOCK/REBUILD (R2); `order-artifact` ⇒ карантин; пустой провенанс ≠ ACCEPT; exact-prompt overlap ⇒ карантин (не молчаливый дроп) | §5.4, §6.1-6.3 |
| **BL** (baseline) | `baseline-loser` / first-party self-label помечен `semantic_quality_claimed=false`, не выдаётся за semantic | §5.2, §6.3 |
| **INT** (integration) | 5 форматов → 1 канон `GraphDpoPair`; проекция → `LearningTrainingItem` → `DpoDataset.Load(train).Count == N(train-ready)`; не-ACCEPT не доходит до `TrainBatchAsync` | §3.1, §4 шаг 7, §6.6 |
| **QA** | скан-отчёт: 0 train-ready строк с `sonnet/openai/gpt/claude/gemini` в `source_ref`/`judge_model`; `train_ready_without_triad_count == 0` | §6.2, §6.6 |
| **REG** (regression) | расширение provenance/`SourceCategory` не ломает существующий путь `ResultGapSourceDescriptor`/`IsAllowedCommonLicense` | §3.5 |
| **UH** (untrained-honesty) | строитель честно репортит «данные ещё не обучают» (`dpo_train_ready=false`), не маскирует детерминизм под обучение | §4 шаг 8, §6.4 |
| **TA** (trained-acceptance) | **не для строителя**, а для юнита ребра на этих строках: held-out Δ≥+0.02, reward≥0, ablation, fresh-load, continue-training (блок 11) | §6.4, блок 11 |

Untrained-vs-trained честность: любой отчёт, называющий вердикты строителя или его
выход «обученным качеством», — masquerade (запрещено `plan-review-SKILL.md`
deterministic-masquerade firewall).

---

## 9. Part A: §0 UML + §1 флоучарт (improvement D1, `implementation-doc-uml-SKILL`)

### §0. Класс-диаграмма строителя DPO + per-connection симуляции

```mermaid
classDiagram
    class GraphDpoBuilder {
        +BuildPairAsync(GraphDpoSourceRow, BuildContext, ct) Task~OperationResult~GraphDpoPair~~
        +BuildBatchAsync(GraphDpoSourceRow[], BuildContext, ct) Task~OperationResult~GraphDpoBatch~~
    }
    class GraphDpoPair {
        +pair_id
        +chosen
        +rejected
        +wrong_markers
        +teacher_provenance
        +candidate_order_invariant
        +14 полей всего
        +dpo_train_ready
    }
    class TeacherProvenanceGateUnit {
        +Evaluate(prov, ctx) TeacherProvenanceGateResult
    }
    class TeacherProvenance {
        +model_id
        +judge_model
        +closed_proprietary_model
        +20 полей всего
    }
    class LearningTrainingItem {
        +ItemId
        +Prompt
        +ChosenResponse
        +RejectedResponse
    }
    class DpoDataset {
        +Load(path) DpoDataset
    }

    GraphDpoBuilder ..> GraphDpoPair : строит (output)
    GraphDpoBuilder ..> TeacherProvenanceGateUnit : шаг 1 legal-гейт
    GraphDpoPair *-- TeacherProvenance : содержит (вложенный)
    TeacherProvenanceGateUnit --> TeacherProvenance : читает 20 полей
    GraphDpoPair ..> LearningTrainingItem : проекция (только train-ready)
    LearningTrainingItem ..> DpoDataset : материализуется как JSONL

    note for GraphDpoBuilder "NEW (в коде НЕТ, владелец блок 07). Детерминированный сборочный цех DPO: 5 форматов→канон→триада гейтов→train-ready. Это helper-нормализатор/валидатор, НЕ обучаемый уровень. Статус: ПРОЕКТ."
    note for GraphDpoPair "NEW (в коде НЕТ). Канон DPO-пары графа — 14 полей. teacher_provenance ВЛОЖЕН (20 полей), НЕ плоский ref. dpo_train_ready — gate. Статус: ПРОЕКТ."
    note for TeacherProvenanceGateUnit "NEW (владелец блок 08). L2 ILogicalUnit — легальный фильтр строки. closed && used_for_training => DATASET_REBUILD_REQUIRED. Статус: ПРОЕКТ."
    note for TeacherProvenance "NEW (владелец блок 08). 20 полей легальности; judge_model ВНУТРИ; closed_proprietary_model — вычисляемый из denylist. Статус: ПРОЕКТ."
    note for LearningTrainingItem "СУЩ. 00.Core/LearningMachineContracts.cs:30. Цель ПРОЕКЦИИ train-ready пары для L5-обучения. Статус: РАБОТАЕТ."
    note for DpoDataset "СУЩ. 11.DPO/DpoDataset.cs:18. Материализация JSONL (prompt/chosen/rejected). Конечная точка строителя. Статус: РАБОТАЕТ."
```

**Легенда:**

| Класс | Что делает | Статус |
|---|---|---|
| `GraphDpoBuilder` | Детерминированный строитель: 5 форматов→канон→триада→train-ready | NEW (ПРОЕКТ, блок 07) |
| `GraphDpoPair` | Канон DPO-пары графа (14 полей), provenance вложен | NEW (ПРОЕКТ, блок 07) |
| `TeacherProvenanceGateUnit` | Легальный гейт строки (L2), 8-шаговая машина вердикта | NEW (ПРОЕКТ, блок 08) |
| `TeacherProvenance` | 20 полей легальности учителя | NEW (ПРОЕКТ, блок 08) |
| `LearningTrainingItem` | Цель проекции train-ready пары для L5 | РАБОТАЕТ (`LearningMachineContracts.cs:30`) |
| `DpoDataset` | Материализация JSONL для DPO-обучения | РАБОТАЕТ (`11.DPO/DpoDataset.cs:18`) |

**Per-connection симуляции (для КАЖДОЙ связи — вход + ожидаемый результат):**

| Связь (A → B) | Пример входа (симуляция) | Ожидаемый результат по этой ветке |
|---|---|---|
| `GraphDpoBuilder ..> GraphDpoPair` | `raw` с пустым `chosen` | `Ok(pair)` с `dpo_train_ready=false`, `row_rejection_reason="EMPTY_SIDE"` — не throw (Rule 1) |
| `GraphDpoBuilder ..> TeacherProvenanceGateUnit` | provenance `model_id="claude-3.5-sonnet"`, `used_for_training=true` | ранний выход: `DATASET_REBUILD_REQUIRED`, `leg_scope=Quarantine`, `dpo_train_ready=false` |
| `GraphDpoPair *-- TeacherProvenance` | provenance 19/20 полей | `provenance_field_presence_rate<1.0` → `dpo_train_ready=false` |
| `TeacherProvenanceGateUnit --> TeacherProvenance` | `judge_model="gpt-4o"`, `used_for_training=false`, chosen от open-weight | `ACCEPT` (judge-only легален) |
| `GraphDpoPair ..> LearningTrainingItem` | train-ready пара (вся триада true) | проекция `ItemId←pair_id, Prompt←input_context, ChosenResponse←chosen, RejectedResponse←rejected` |
| `LearningTrainingItem ..> DpoDataset` | N train-ready строк → JSONL | `DpoDataset.Load(path).Count == N` (`11.DPO/DpoDataset.cs:18`) |

### §1. Флоучарт рецепта построения пары + per-branch симуляции

```mermaid
flowchart TD
    A["📥 Сырьё: вход A + chosen + rejected\n(узел или ребро, 1 из 5 форматов)"] --> B{"Обе стороны\nнепустые?"}
    B -->|"НЕТ (пустой chosen/rejected)"| BR["❌ EMPTY_SIDE\ndpo_train_ready=false"]
    B -->|"ДА"| C{"Учитель легальный?\n(не closed-for-training)"}
    C -->|"НЕТ (Claude/OpenAI/Gemini как teacher)"| CR["🚫 DATASET_REBUILD_REQUIRED\nleg_scope=Quarantine"]
    C -->|"ДА (open-weight ИЛИ judge-only)"| D{"Не near-duplicate\nс held-out?"}
    D -->|"НЕТ"| DR["🗃️ split-leak → QUARANTINE"]
    D -->|"ДА"| E{"Provenance полон (18) И\nнет leakage И order-invariant?"}
    E -->|"НЕТ"| ER["🗃️ order-artifact / неполный provenance\n→ QUARANTINE / eval-only"]
    E -->|"ДА"| F{"overlap<0.60 И reason≥24\nИ триада есть?"}
    F -->|"НЕТ"| FR["❌ LOW_CONTRAST / REASON_TOO_SHORT /\nтриада неполна → eval-only"]
    F -->|"ДА"| G["✅ dpo_train_ready=true\nпроекция → LearningTrainingItem → DpoDataset"]
```

**Per-branch симуляции (для КАЖДОЙ ветки — вход + ожидаемый исход):**

| Узел/ветка | Вход (симуляция) | Ожидаемый исход |
|---|---|---|
| B → НЕТ | `chosen=""` | `EMPTY_SIDE`, `dpo_train_ready=false` |
| B → ДА | `chosen`/`rejected` оба заполнены | перейти к legal-гейту |
| C → НЕТ | provenance `model_id="claude-sonnet"`, `used_for_training=true` | `DATASET_REBUILD_REQUIRED` (denylist, R2) |
| C → ДА (judge-only) | `judge_model="gemini"`, `used_for_training=false`, chosen от `qwen3` | пройдено, перейти к split-гейту |
| D → НЕТ | (input+chosen) near-duplicate held-out строки | `split-leak` → QUARANTINE |
| D → ДА | (input+chosen) уникален относительно held-out | перейти к provenance/leakage-гейту |
| E → НЕТ (order) | chosen меняется при перестановке кандидатов | `order-artifact` → QUARANTINE (A-LM-09) |
| E → НЕТ (provenance) | 19/20 полей | `dpo_train_ready=false` (eval-only) |
| E → ДА | 18/18, leakage вырезан, order-invariant | перейти к overlap/триада-гейту |
| F → НЕТ (overlap) | `chosen`≈`rejected`, `overlap=0.74` | `LOW_CONTRAST`, `dpo_train_ready=false` |
| F → НЕТ (триада) | ACCEPT по легальности, но нет `raw_comparison_event` | eval-only (`triad_incomplete`) |
| F → ДА | open-weight teacher, overlap=0.31, reason=40 симв., order-invariant, триада есть | `dpo_train_ready=true`, проекция → `LearningTrainingItem` (S10) |

> **Валидация UML (implementation-doc-uml-SKILL §«Validation gate»):** `note for` на
> каждом классе ✓; легенда после диаграммы ✓; решающие узлы §1 — человеческие
> вопросы ✓; каждая связь §0 имеет ≥1 симуляцию + ожидаемый результат ✓; каждая
> ветка §1 имеет вход + ожидаемый исход ✓.

---

## 10. Сводный чек-лист соответствия (для ревью)

```text
[x] Структура бенчмарка сохранена: 8 разделов (назначение/концепт/код-сейчас/рецепт/
    примеры/гейты/пробелы/ссылки) + добавлены §8 тест-стратегия и §9 Part A UML.
[x] Канон-счётчики: 12 фаз / 26 подфаз / 59 подшагов, Ф<P>.<SP>.<SS>; якорь Ф3/Ф2.1/Ф11.
[x] CANON-инварианты: A-LM-12 (5 уровней, без 6-го); legal teacher-гейт (closed=judge
    при used_for_training=false; контаминация→DATASET_REBUILD_REQUIRED);
    A-LM-09/A-LM-13 (нет скрытого детерминизма — строитель помечен helper).
[x] GraphDpoPair = 14 полей (D2 исправлен), TeacherProvenance вложен = 20 полей —
    синхрон с мастер §A.4, блок 07 §3.2, блок 08 §3.1.
[x] Вердикт-enum (D3): ACCEPT|QUARANTINE|BLOCK|DATASET_REBUILD_REQUIRED|BLOCK_FOR_COMMON|EVAL_ONLY;
    LegScope = 4 значения.
[x] Цитаты file:line перепроверены чтением главного чекаута: DpoPair.cs:3-8;
    DpoTrainer.cs:31-70 (42-44/47/50/54/57-65); ResultGapDpoWriters.cs:192-196/241-266/
    199-235/7-14; DpoDataset.cs:18-26; LearningTrainingItem :30; SourceIds :26.
[x] Тест-стратегия (D4): U/NEG/BL/INT/QA/REG/UH/TA; строитель = honest scaffold,
    TA — для юнита ребра, не строителя.
[x] Honesty по доказательствам: число «19/72» помечено benchmark-sourced; live-scan
    не запускался в этой сессии (PLAN-ONLY).
[x] codex-* не редактировались (только чтение); claude-00..07 каркас — evidence.
```

---

## 11. Ссылки

**Рекон-сводки (evidence, только чтение):**
- `codex-recon-core-18-capability.md` — пайплайн авто-DPO 18.CapabilityTraining.
- `codex-recon-core-11-dpo.md` — движок 11.DPO.
- `codex-recon-core-42-wiki.md` — Bootstrap-юниты и flow-адаптеры.
- `codex-recon-core-26-router.md` — keyword-router как анти-паттерн.
- `codex-recon-mvp-internal-learn.md`, `...-internal-learn-2.md` — метод
  авто-построения пар, провал калибровки 2026-05-06, формат `[WRONG:]`.
- `codex-recon-mvp-external-sources.md` — лицензии/provenance внешних источников.
- `codex-recon-mvp-learning-sets.md`, `codex-recon-mvp-plan-set.md` — формат
  `A→узел→B`, 9-узловой граф, источники негативов.

**Реальные файлы кода (перепроверены чтением, file:line):**
- `C:/Projects/llm_mvp_core/11.DPO/Models/DpoPair.cs:3-8`
- `C:/Projects/llm_mvp_core/11.DPO/DpoDataset.cs:18-38`
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:31-70`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiContextRoutingDpoGenerator.cs:17-94, 96-212, 214-290`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs:96-262`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanContracts.cs:110-122`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationKnnStore.cs:32-62`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDomainModels.cs:918`
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/ResultGapDpoWriters.cs:7-14, 111-153, 182-196, 199-235, 241-266`
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs:26, 30`
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs:6` (поле `SourcePath`)
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283, 293` (фикс-веса; исключение SourcePath)

**Sibling claude-* (мой namespace, канон-синхрон, evidence):**
- `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §A.4 — канон `GraphDpoPair` (14) /
  `TeacherProvenance` (20) / счётчики.
- `claude-plan-07-dpo-builder.md` — алгоритм `BuildPairAsync`, таксономия `rejected_origin`.
- `claude-plan-08-legal-teacher-gate.md` — владелец канона `TeacherProvenance`,
  ветки вердикта C2-C16, реестр allowlist/denylist.
- `claude-04-dpo-legal-model-policy.md`, `claude-05-dpo-data-training-measurement-guide.md`.

**Скилы/гайды (правила):**
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/implementation-doc-uml-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/dpo-triple-format-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/planning-dpo-authoring-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/dcm-dpo-construction-GUIDE.md`

---

*Конец гайда 01 (claude namespace). PLAN-ONLY: документация-решение внутри плана
ILearningDynamicGraph. Не разрешает реализацию, тренировку, билд, запуск раннеров,
git или вызов внешних моделей. Цитаты кода — только реально существующие, с
file:line, сверены прямым чтением главного чекаута. Бенчмарк `codex-guide-01` и
прочие codex-* — evidence-only, не редактировались.*
