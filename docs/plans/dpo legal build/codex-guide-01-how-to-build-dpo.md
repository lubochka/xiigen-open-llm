# Гайд 01 — Как строить DPO-пары (для ILearningDynamicGraph)

**Версия:** 1.0
**Автор:** Codex (Opus 4.8 MAX), роль «автор гайда»
**Применимо к:** план ILearningDynamicGraph — авто-построение DPO для
`ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` / `ILearningDynamicGraph`
(маппинг на реальную иерархию репозитория `ILogicalUnit` / `ILogicalFlow` /
`ILogicalOrchestrator` / `ILearningMachine` / `ILearningMachinePair`).
**Язык:** русский (первичный); поля и код — английский.
**Статус:** PLAN-ONLY. Это документация-гайд внутри плана, НЕ разрешение на
реализацию, тренировку, билд или запуск раннеров.
**Скилы-основания:** `docs/skills/trainable-architecture-decision-SKILL.md`
(Legal Teacher / Provenance Gate, Graph-Style Reconstruction Gate),
`docs/skills/dpo-triple-format-SKILL.md`,
`docs/skills/planning-dpo-authoring-SKILL.md`,
`docs/skills/dcm-dpo-construction-GUIDE.md`.

---

## 1. Назначение гайда

Этот гайд отвечает на один практический вопрос: **как из узла графа и из ребра
графа машинно построить DPO-пару (chosen / rejected / `[WRONG:]`)**, которую
можно подать в обучение, и при этом не нарушить три класса правил:

1. **Анти-leakage** — пара не должна учить модель «угадывать по утечке»
   (target-label, путь к файлу, id источника, точный текст промпта, порядок
   кандидатов).
2. **Легальный teacher-гейт** — chosen/rejected как обучающая цель допустимы
   только от open-weight permissive teacher или из first-party/permissive
   источников; запрещены выводы Claude/OpenAI/Gemini как distillation-цели.
3. **Запрет скрытого детерминизма** — DPO-пара существует, чтобы обучаемый юнит
   ПЕРЕКРЫЛ детерминированный keyword/regex/lookup-baseline, а не закрепил его.

Гайд: (а) фиксирует, как DPO строится в коде репозитория СЕЙЧАС, с точными
ссылками `file:line`; (б) перечисляет РАЗНЫЕ форматы пар, реально найденные в
коде и на диске, и предлагает единый канон для графа; (в) даёт пошаговый рецепт
авто-построения пары на узле и на ребре; (г) даёт гейты и список пробелов,
которые план обязан закрыть.

Этот документ не разрешает менять код, данные, чекпойнты. Любая DPO-строка,
батч или claim о качестве модели проверяется против этого гайда, против
`trainable-architecture-decision-SKILL.md` и против гайдов измерения, и не может
попасть в `Train*` без source/license-гейта, teacher-provenance, анти-leakage и
held-out разделения.

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

**Два важных уточнения аналогии:**

- **«Почти-хорошо» важнее «совсем плохо».** Если rejected — это явный мусор
  («не знаю», пустой ответ, бред), стажёр выучит только «не будь мусором» — это
  бесполезно. Сильный сигнал даёт *near-miss*: rejected отличается от chosen
  ровно одной осмысленной ошибкой (выбрал семантический ретривер там, где нужен
  точный лексический; пропустил проверку версии; ответил без доказательств).

- **Граф = много таких экзаменов, соединённых трубами.** У нас не один узел, а
  поток узлов (splitter → translator → planner → processor → applier →
  result-translator → arbiter → formatter), где выход одного узла = вход
  следующего. Поэтому DPO-пары бывают двух видов:
  - **пара на узле** — «для входа X этого узла правильный выход chosen, а не
    rejected»;
  - **пара на ребре** — «при таком выходе предыдущего узла правильно
    передать/смаршрутизировать в узел A (chosen), а не в узел B (rejected)»;
    то есть тестируется само решение о маршруте/передаче между узлами.

**Что НЕ является DPO-парой.** Пара «вход → правильный выход» без rejected — это
supervised (teacher-forcing), а не DPO. Такие ряды (формат `A → узел → B` из
`learning-sets`) — это сырьё; чтобы стать DPO, к ним надо ДОстроить rejected по
правилам ниже.

---

## 3. Как это реализовано в нашем коде СЕЙЧАС (точные ссылки)

Здесь — что РАБОТАЕТ и что ЗАГЛУШКА. Все ссылки проверены чтением файлов.

### 3.1 Движок одного DPO-шага — РАБОТАЕТ (но это не построитель пар)

`11.DPO` — низкоуровневый тренировочный движок. Он НЕ строит пары; ему пары
подают извне.

- Контракт пары: `11.DPO/Models/DpoPair.cs:3-8` — `record DpoPair { Prompt;
  Chosen; Rejected; }`. Три строки, по умолчанию пустые. **Нет** полей source /
  license / teacher / near-miss / domain. Это минимальный «голый» формат.
- Загрузка набора: `11.DPO/DpoDataset.cs:18-26` — `Load(path)` читает JSONL по
  строке на `DpoPair`, пустые строки пропускает, `null` → исключение;
  `Shuffle(seed)` (строки 28-38) — Fisher-Yates, исходник не мутируется.
- Лосс и шаг обучения — РЕАЛЬНЫЕ: `11.DPO/DpoTrainer.cs:31-70` (`Step`):
  - `margin = chosenReward − rejectedReward`, где
    `chosenReward = Beta*(polChosen − refChosen)` (строки 42-44);
  - `loss = −log(σ(margin)+1e-8)` (строка 47);
  - ручной backward `gradMargin = σ(margin) − 1` (строка 50), затем
    `AdamW.Step` (строка 54);
  - метрики `DpoMetrics { Loss, ChosenReward, RejectedReward, RewardMargin,
    Accuracy = margin>0?1:0 }` (строки 57-65).
  - `Beta = 0.1f`, `lr = 1e-5f` — константы (по рекону
    `codex-recon-core-11-dpo.md`).
- ЗАГЛУШКА/ПРОБЕЛ движка: нет сохранения весов как checkpoint/manifest/locator,
  нет export/import, fresh-load, continue-training, held-out, нет CPU/RAM/hash в
  метриках. Прямых вызовов `DpoTrainer` из боевого рантайма (15/42/43) не
  найдено — пары пока строятся, но в этот движок в проде не подаются.

**Вывод по 3.1:** `DpoTrainer` — честный «кирпич» применения сигнала
(`OK_INTERNAL_DETAIL` / `OK_MODEL_COMPONENT` по скилу), НО он диктует, что
авто-построитель графа обязан выдавать именно `(Prompt, Chosen, Rejected)` в
токенизируемом виде.

### 3.2 Построение пары на узле — РАБОТАЕТ (детерминированный построитель)

Канонический работающий пример — `WikiContextRoutingDpoGenerator`
(`42.ProjectWikiDomain/WikiContextRoutingDpoGenerator.cs`). Это узел
«контекст-роутинг wiki» (выбор Mode2 semantic / Mode3 BM25 / abstain).

Как строится пара (метод `BuildBatchAsync`, строки 17-94):
1. Из seed-вопроса собирается `ProjectWikiUserQuestion`, прогоняется через
   `ProjectWikiQueryPlanningFlow.PlanAsync` и `WikiContextRoutingUnit.Route`
   (строки 46-62) — то есть **chosen берётся из реального выхода юнита**, а не
   придумывается.
2. `BuildChosen(...)` (строки 96-212) — формирует chosen как строки
   `key: value` (например `route_kind`, `requires_semantic_retriever`,
   `prechecks_complete: true`, `answer_before_retrieval: false`).
3. `BuildRejected(rejectedType, ...)` (строки 214-290) — формирует
   правдоподобно-неверный ответ по типу ошибки и добавляет подпись
   `[WRONG: <type> <последствие>]`. Типы rejected (реальные значения в коде):
   `always_bm25`, `always_semantic`, `answer_instead_of_abstain`,
   `ignore_wrong_project`, `ignore_stale_version`, `no_pre_checks`.
4. Гейты качества батча (строки 75-86): `wrongAnnotationRate` (доля пар с
   `[WRONG:`) должна быть = 1.0; иначе варнинг `wrong_annotation_rate_below_gate`;
   контроль `batch_row_count_not_50` и `developer_reasoning_rows_below_40_percent`.

Пример реального rejected из кода (строка 277):
`Always route to Mode3 BM25 because lexical lookup is simpler than semantic
retrieval.` + `[WRONG: always_bm25 misses multi-hop developer reasoning where
semantic expansion is required ...]`.

В `42.ProjectWikiDomain` есть 12 таких `*DpoGenerator.cs` (по Glob):
`CodeTraceBuilderDpoGenerator`, `WikiContextRoutingDpoGenerator`,
`ProjectWikiAnswerComposerDpoGenerator`, `StackAndVersionDpoGenerator`,
`SystemBoundaryMapperDpoGenerator`, `ProjectWikiQueryPlanningDpoGenerator`,
`CrossStackBusinessFlowGraphDpoGenerator`, `WikiTraceAggregatorDpoGenerator`,
`DocumentationGeneratorDpoGenerator`, `ProjectAnswerVerifierDpoGenerator`,
`EntityImpactMapperDpoGenerator`, `ProjectWikiRequestRoutingDpoGenerator` — это
по сути «узлы графа wiki», каждый со своим построителем пар.

**Вывод по 3.2:** построение пары на узле уже работает как детерминированный
fixture-builder. Это `OK` как генератор датасета, НО chosen здесь —
self-label (выход того же детерминированного юнита), значит teacher-источник
истинности детерминирован — это требует ablation/no-shortcut и легального
teacher при переходе к семантическому claim (см. §6).

### 3.3 Построение пары на ребре / grounding — РАБОТАЕТ (корпус оркестрации)

`OrchestrationCorpusBuilder` (`42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs`)
строит пары для маршрутно-grounding-решений (это ближе всего к «ребру графа»):

- Запись пары: `OrchestrationTrainingPair`
  (`42.ProjectWikiDomain/OrchestrationPlanContracts.cs:110-122`) — поля
  `PairId, Mode, Prompt, Chosen (OrchestrationPlanChosen), RejectedText,
  ExpectedRouteMode, ExpectedBoundarySource, ExpectedMlGenerated,
  ExpectedIntentKind, FlowRef, SourceBundleId, IsHeldOut`.
- Три режима (`PairMode`): `Mode1LearnedFlowGrounding`, `Mode2Generalization`,
  `Mode3HumanExplanation` (строки `AddMode1Pairs`/`AddMode2Pairs`/`AddMode3Pairs`,
  128-241). Пример rejected (строка 144):
  `Rejected route uses deterministic lookup instead of learned grounding.`
- Анти-leakage в корпусе: `NoExactPromptOverlap` (строки 243-254) + нормализация
  промпта `NormalizePrompt` (строки 256-262) — train/eval не должны делить
  одинаковые промпты; дополнительно проверяется disjoint по `SourceBundleId` и
  `FlowRef` (строки 75-77).
- Гейт корпуса `MeetsCorpusGate` (строки 81-93): минимальные счётчики по режимам
  + все overlap-флаги.

**Вывод по 3.3:** edge/grounding-пары и их анти-overlap-гейты уже реализованы.
ПРОБЕЛ: `OrchestrationTrainingPair` не несёт teacher-provenance и license-полей.

### 3.4 KNN-память узла — РАБОТАЕТ как компонент (не финальная authority)

`OrchestrationKnnStore` (`42.ProjectWikiDomain/OrchestrationKnnStore.cs`):
`FindNearestWithScore` (строки 32-43) ищет ближайшую обучающую пару по
**Jaccard** над токенами промпта (`Tokenize` строки 45-50, `Jaccard` 52-62);
есть `ExportPairs`/`ImportPairs` (строки 19-27).

Классификация по скилу: KNN — audit-triggered, не запрещён. Это допустимый
**компонент памяти** обучаемого узла (`OK_MODEL_COMPONENT`), пока он не становится
единственной финальной authority вместо обученного юнита. Для графа: nearest-память
поддерживает решение, но не заменяет обучаемый сигнал.

### 3.5 Легальный гейт источника и анти-leakage схемы — РАБОТАЕТ (самый зрелый код)

Самая зрелая легальная реализация — `43.SmartSkillsDomain/ResultGapDpoWriters.cs`
(узел ResultGap «обнаружение недостающей способности после ответа»):

- Дескриптор источника: `ResultGapSourceDescriptor`
  (`ResultGapDpoWriters.cs:7-14`) — `SourceKind, LicenseExpression, PrivacyScope,
  SourceRef, SourceSha256, SourceVersionId, UserApprovalRecorded`.
- Роутер приватности/лицензии `ResultGapSourcePrivacyRouter.Classify`
  (строки 111-153):
  - пустой источник → quarantine `source_identity_missing` (113-114);
  - vendor/external/unknown → quarantine `external_vendor_or_unapproved_source`
    (116-117, правило `IsExternalOrVendor` строки 182-186);
  - `forbidden`/`raw_private` privacy → quarantine `privacy_scope_forbidden`
    (119-120);
  - private/correction → `adaptive_only` (122-134);
  - approved common + разрешённая лицензия → `trainable_common` (137-150).
  - Разрешённые common-лицензии (`IsAllowedCommonLicense` строки 192-196):
    **MIT, Apache-2.0, ProjectInternal, Synthetic**.
- Схема-валидатор анти-leakage `ResultGapDpoSchemaValidator.Validate`
  (строки 241-266): требует chosen_ref/rejected_ref/skill_ids, и явно ловит
  утечку — `Require(!ChosenTargetContainsRawPrivateOrVendorOutput, ...)`,
  `!ContainsRawPrivateText`, `!ContainsRawVendorOutput`,
  `!CommonCheckpointMutationClaimed`, `TechnicalExecutionRefsExcluded`,
  `!SourceIdUsedAsTrainingFeature`, near-miss-таксономия train-ready,
  `RejectedWhyPlausible.Length >= 24` (строки 250-257).
- Near-miss таксономия `ResultGapNearMissTaxonomy` (строки 199-235): коды
  `missing_required_capability_after_output`, `false_done_with_gap_remaining`,
  `correction_overfit_to_raw_answer` (train-ready=true) и
  `generic_bad_output` (train-ready=**false**, только quarantine).

**Вывод по 3.5:** анти-leakage и license-роутер реализованы и являются образцом
для графа. ПРОБЕЛ: здесь нет полей teacher-провенанса (`teacher_access_mode`,
`closed_proprietary_model` и т.д.) — см. ниже.

### 3.6 Что на диске (реальные форматы строк)

Проверены первые строки файлов:

- `training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl`:
  **ref-only** формат — `pair_id, source_row_id, approved_pair_ref, error_code,
  chosen_ref, rejected_ref, source_ref, source_sha256, model_leg,
  abstraction_layer, universal_reasoning_pattern, excluded_technical_execution_refs,
  raw_private_text_copied:false, technical_execution_leakage:false,
  dpo_binding_ready, dpo_materialized, dpo_train_ready:false, training_started:false`.
  То есть строка несёт SHA256 источника и `excluded_technical_execution_refs`, а
  `dpo_train_ready=false` — честное «ещё не обучаемо».
- `training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed.jsonl`:
  формат с `prompt_input_task_context` + `chosen_ref/chosen_summary/rejected...`,
  `source_quality:"gold"`, `machine_target:"PromptToTaskMachine"`,
  `private_or_project_specific_text_copied:false`.
- `training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl`:
  payload-формат с `chosen_scenario_plan_features` (intent_signals,
  reasoning_steps, evidence_expectations, decision_guards) — то есть пара,
  разложенная в признаки, а не в сырой текст.

**КЛЮЧЕВОЙ ФАКТ (проверено Grep):** поля `teacher_access_mode`,
`closed_proprietary_model`, `hosting_provider`, `teacher_output_used_for_training`
встречаются ТОЛЬКО в файлах скилов/гайдов (`docs/skills/*`), и НЕ встречаются ни
в одном `.cs` и ни в одной строке `training-data/*.jsonl`. То есть легальный
teacher-гейт пока существует как ПРАВИЛО, но не как поле данных — это
обязательный пробел плана (§7).

---

## 4. Пошаговый рецепт авто-построения DPO-пары

Рецепт един для узла и ребра; различие — в шаге 2 (что является входом A) и
шаге 4 (что варьируется в rejected).

**Шаг 0. Классифицировать цель по разрешённому уровню.**
Назвать узкую способность и сопоставить её ровно одному разрешённому уровню:
`ILogicalUnit` (узел), `ILogicalFlow` / `ILogicalOrchestrator` (последовательность/
маршрут), `ILearningMachine` / `ILearningMachinePair` (владелец обучения).
scorer/selector/router/KNN/regex — НЕ уровень, только деталь внутри уровня
(`trainable-architecture-decision-SKILL.md`, раздел allowed levels).

**Шаг 1. Зафиксировать и проверить источник (legal gate ПЕРЕД построением).**
Заполнить дескриптор источника (как `ResultGapSourceDescriptor`): `source_ref`,
`source_sha256`, `license`, `privacy_scope`, плюс teacher-provenance (§6).
Прогнать через роутер: `trainable_common` / `adaptive_only` / `quarantine`.
Если quarantine — пара НЕ строится для общего обучения.

**Шаг 2. Сформировать A (вход).**
- Узел: реальный рантайм-вход узла (для роутинга — пользовательский вопрос; для
  array-узла — один элемент). Правило `learning-sets`: НЕ класть строку
  документа, если человек так бы не написал.
- Ребро: выход предыдущего узла + контекст маршрутизации (`candidateUnitIds`,
  `requiredDirection`, флаги вроде `UseKnowledgeEdgeDetection`).
- Анти-leakage: A не должен содержать порядок кандидатов как сигнал, id
  источника, точный путь файла.

**Шаг 3. Получить chosen.**
Предпочтительно — из легального teacher или из first-party канона (выход
проверенного юнита, как в `WikiContextRoutingDpoGenerator`). chosen конкретен:
реальное решение/маршрут/выход, не «составь план».

**Шаг 4. Построить rejected как near-miss + `[WRONG:]`.**
Выбрать один тип ошибки из таксономии узла (например `always_semantic`,
`ignore_stale_version`, `no_pre_checks`). rejected отличается от chosen РОВНО
одной осмысленной ошибкой и остаётся правдоподобным. Добавить
`[WRONG: <код> <короткое функциональное последствие>]`.
Запрет: rejected-мусор, отрицание-шаблон («Do not generate a capability for
sub-phase X» — именно он дал провал калибровки 2026-05-06, overlap ~0.70,
loss=ln2; см. `codex-recon-mvp-internal-learn.md`).

**Шаг 5. Проверить overlap и инвариантность.**
- `overlap_ratio(chosen, rejected) < 0.60` (правило из внешнего реестра пар).
- train/eval не делят промпт (`NoExactPromptOverlap`), а для ребра-выбора —
  permutation-инвариантность: chosen стабилен при перестановке кандидатов
  (`candidate_order_invariant=true`), иначе `QUARANTINE_DO_NOT_TRAIN`
  (`dpo-triple-format-SKILL.md`).

**Шаг 6. Прогнать схема-валидатор анти-leakage.**
Как `ResultGapDpoSchemaValidator`: запретить raw private/vendor текст в
chosen-цели, `source_id` как признак, technical execution refs; near-miss код
обязан быть train-ready; `RejectedWhyPlausible` ≥ 24 символов.

**Шаг 7. Разложить в обучаемую форму и измерить.**
Свести к токенизируемой паре для `DpoTrainer` (Prompt/Chosen/Rejected), считать
`reward_margin = logP(chosen) − logP(rejected)` ≥ 0 и `loss` < 0.685
(≥0.693 = random-guess = брак, `TEACHING-MEASUREMENT-GUIDANCE-v6`). Held-out 50+50
disjoint по источнику (`dcm-dpo-construction-GUIDE.md` §7).

**Шаг 8. Статус честно.**
Пока нет held-out метрик + ablation + fresh-load + continue-training через
checkpoint/manifest/locator — статус строки `dpo_train_ready=false` /
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, НЕ `PASS_AS_TRAINED_FEATURE`.

---

## 5. Конкретные примеры с реальными значениями

### 5.1 Пара на УЗЛЕ (контекст-роутинг wiki) — реальный шаблон из кода

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
выбор «всегда BM25» — правдоподобная, но неверная экономия. Анти-leakage: chosen
и rejected отличаются режимом ретривера, а не id вопроса/порядком.

### 5.2 Пара на РЕБРЕ (grounding-маршрут оркестрации) — из OrchestrationCorpusBuilder

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
детерминизма).

### 5.3 Ref-only строка на диске (orchestration-reasoning) — реальная первая строка
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
chosen/rejected надо проверить teacher-гейтом (§6) ДО `dpo_train_ready=true`.

### 5.4 Negative-пример (что НЕ принимать)
```text
chosen:   route_kind: Mode2SemanticHybrid ... (правильно)
rejected: "I don't know."                 <-- МУСОР, не near-miss
```
Вердикт: `QUARANTINE_DO_NOT_TRAIN`. rejected не правдоподобен, overlap почти 0,
ничему не учит, кроме «не будь пустым».

---

## 6. Гейты и правила

### 6.1 Анти-leakage (обязательные запреты в признаках/целях)
Запрещены как обучающие признаки: target-label напрямую, **source-path / id
источника**, **точный текст промпта (exact-prompt replay)**, **порядок/индекс
кандидата**, имена пользователей/проектов/сессий, raw private/vendor текст в
chosen-цели. Реализованные проверки-образцы: `ResultGapDpoWriters.cs:250-257`
(privacy/raw/source-id), `OrchestrationCorpusBuilder.cs:243-262` (exact-prompt
overlap), permutation-инвариантность из `dpo-triple-format-SKILL.md`.
Допустимы: нормализованный вход, абстрагированные дефекты/признаки,
nearest-память без утечки пути.

### 6.2 Легальный teacher-гейт (provenance) — обязателен, в коде ОТСУТСТВУЕТ
chosen/rejected как обучающая цель допустимы только когда teacher — разрешённая
open-weight permissive модель ИЛИ first-party/permissive источник, и проверены
provider terms + лицензия.
- **Разрешённые teacher-семейства (open-weight, нейтральные провайдеры):** Qwen3
  (Apache-2.0), DeepSeek, Kimi K2, GLM, Mistral.
- **ЗАПРЕЩЕНЫ как distillation-teacher:** Claude, OpenAI/GPT, Gemini, Gemini
  Grounding. Если `closed_proprietary_model=true` и
  `teacher_output_used_for_training=true` → вердикт **`DATASET_REBUILD_REQUIRED`**,
  без легальных исключений (`trainable-architecture-decision-SKILL.md`, Legal
  Teacher Gate). Закрытые модели допустимы только как **judge/eval/критика**, не
  как chosen/rejected-цель.
- **Обязательные provenance-поля строки (добавить — сейчас их нет ни в коде, ни в
  jsonl):**
  ```text
  teacher_access_mode:
  hosting_provider:
  model_id:
  model_license:
  provider_terms_checked:
  teacher_output_used_for_training:
  closed_proprietary_model:
  request_id/model_version (когда доступно):
  ```
- Лицензии общего корпуса (совпадают с кодом `IsAllowedCommonLicense`,
  `ResultGapDpoWriters.cs:192-196`): MIT, Apache-2.0, ProjectInternal, Synthetic.
  CC BY 4.0 — только с атрибуцией в метаданных. Запрещены для общего корпуса:
  CC BY-SA, GPL/AGPL, unknown/unlicensed. Instruction-tuning с model-output
  риском (ShareGPT/WizardLM/Alpaca/Dolly/UltraChat/FLAN-raw) — evidence-only/
  quarantine (`codex-recon-mvp-external-sources.md`).
- Правило first-party: ассистент-ответ внешнего чат-корпуса даёт только
  A-side-форму запроса (`a_side_style_only`); chosen ВСЕГДА строится из
  XIIGen-контрактов/кода/одобренных стандартов, не копируется из чужой модели.
- Источники приватных правок Любы/проектов = user-adaptive (personal leg), не
  common (R5).

### 6.3 Запрет скрытого детерминизма (R4)
DPO-пара обязана учить обучаемый юнит ПЕРЕКРЫТЬ детерминированный baseline, а не
закрепить его. Системный риск зафиксирован реконом: `26.TrainableRouter`
(`KeywordUnitScorer.Score`) — это keyword-counter под именем «Trainable router»
(`codex-recon-core-26-router.md`, вердикт `FORBIDDEN_RUNTIME_SHORTCUT`).
Правило: keyword/regex/lookup допустимы только помеченными eval-only/baseline; на
каждом ребре графа назвать владельца финальной authority и приложить
ablation/no-shortcut тест.

### 6.4 Гейты измерения (что = «обучилось»)
- `reward_margin ≥ 0` и растёт; `loss < 0.685` (≥0.693 = random = брак).
- `overlap_ratio(chosen, rejected) < 0.60`; train/eval disjoint по промпту и
  источнику (50+50, disjoint by source).
- Для семантического claim обязательны: held-out accuracy, ablation без
  checkpoint, permutation/leakage-проверки, fresh-load воспроизведение,
  continue-training из существующего checkpoint. Иначе статус —
  `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`/`OPEN`, не `PASS_AS_TRAINED_FEATURE`
  (CLAUDE.md «Untrained honesty vs trained readiness pattern»).
- Anti-masquerade: недоступные числа = `UNKNOWN`, не выдуманные (образец —
  `ProjectWikiTrainingFlowAdapter`, рекон core-42).

### 6.5 Канон формата DPO для графа (предложение — РАЗНЫЕ форматы → один)
Найдены 5 разных форматов: (1) голый `DpoPair{Prompt,Chosen,Rejected}`
(`11.DPO/Models/DpoPair.cs`); (2) узловой текст с `[WRONG:]`
(`WikiContextRoutingDpoGenerator`); (3) запись `OrchestrationTrainingPair` с
Mode/FlowRef/RejectedText (`OrchestrationPlanContracts.cs:110`); (4) ref-only с
SHA256 на диске (`orchestration-reasoning-dpo-pairs.jsonl`); (5) ru-текстовый
`P5HDpoRow` с `ChosenRu/RejectedRu/RejectedErrorTypeRu/RemovedLeakageRu`
(`PromptToTaskAblationHarness.cs:535`) и ref-only
`PromptToTaskTrainingPairContract` (`PromptToTaskContracts.cs:44`).

**Предлагаемый канон `GraphDpoPair` (план обязан утвердить):** объединить сильные
стороны — ref-based (без копирования сырого текста) + узловые поля + provenance +
анти-leakage флаги:
```text
pair_id
node_or_edge: node | edge
target_level: ILogicalUnit | ILogicalFlow | ILogicalOrchestrator | ILearningMachine
graph_node_id / graph_edge_id (from->to)
a_input_ref            # вход (узел: рантайм-вход; ребро: выход предыдущего узла)
chosen_ref / rejected_ref
wrong_annotation        # текст [WRONG: code consequence]
near_miss_taxonomy_code
pair_family
curriculum_tier (1..5)
# provenance (Legal Teacher Gate)
source_ref / source_sha256 / model_leg(common|adaptive)
license / privacy_scope
teacher_access_mode / hosting_provider / model_id / model_license
provider_terms_checked / teacher_output_used_for_training / closed_proprietary_model
# anti-leakage
excluded_technical_execution_refs[]
raw_private_text_copied=false / source_id_used_as_feature=false
candidate_order_invariant / overlap_ratio
# lifecycle/status
split_role(train|heldout) / held_out_set_id
dpo_train_ready / training_started / validation_outcome(accepted|quarantine|rejected)
```

---

## 7. Пробелы / что нужно построить в плане

1. **Teacher-provenance не реализован в данных.** Поля `teacher_access_mode /
   closed_proprietary_model / hosting_provider / ...` есть только в скилах, нет ни
   в `.cs`, ни в `training-data/*.jsonl` (проверено Grep). План обязан добавить их
   как обязательные поля строки и как гейт перед `dpo_train_ready=true`.
   Особый риск: на диске есть строки с `source_ref: sonnet-local-analyzes:*`
   (`orchestration-reasoning-dpo-pairs.jsonl`) — происхождение chosen/rejected
   надо доказать против Legal Teacher Gate (Claude/OpenAI/Gemini → rebuild).
2. **Нет единого `GraphDpoPair`.** 5 несовместимых форматов (см. §6.5). План
   должен утвердить канон и адаптеры из каждого старого формата.
3. **Граф как уровень отсутствует.** В коде нет `ILearningDynamicGraph`,
   many-to-many/fan-out/fan-in/multi-output edge contracts. Сейчас — линейные
   sub-phase + flow-фабрики (рекон core-18/core-42). План обязан реконструировать
   рёбра по Graph-Style Reconstruction Gate: на каждом ребре назвать владельца
   authority и hidden-shortcut-проверку.
4. **chosen — self-label / детерминирован** в узловых генераторах
   (`WikiContextRoutingDpoGenerator`, `26.TrainableRouter`). Нужен легальный
   teacher как источник истинности + ablation, иначе DPO закрепит детерминизм.
5. **Lifecycle обучаемого узла не закрыт.** `DpoTrainer` без checkpoint/export/
   import/locator/fresh-load/continue-training; `26.TrainableRouter` без реального
   DPO-цикла (поля `RewardMargin/DpoAccuracy/Promoted` — плейсхолдеры). План
   обязан добавить полный asset-lifecycle на каждый обучаемый узел/ребро
   (CLAUDE.md «Trainable Unit pattern»).
6. **Permutation/ablation-гейты на ребре не доказаны.** Есть exact-prompt overlap,
   но нет permutation-инвариантности выбора маршрута и no-shortcut-теста в коде.
7. **Supervised-ряды ≠ DPO.** Корпус `learning-sets` (формат `A→узел→B`) —
   supervised; материал для rejected (failure/feedback/arbiter-flag) есть, но не
   сведён в тройки (`codex-recon-mvp-learning-sets.md`). План должен задать
   правило сборки тройки из этих блоков.
8. **Расхождение указателей версий** train-ready (V192 vs V309) в
   `internal learnin 2` — определить канонический train-ready перед обучением.

---

## 8. Ссылки

**Рекон-сводки (evidence):**
- `codex-recon-core-18-capability.md` — пайплайн авто-DPO 18.CapabilityTraining.
- `codex-recon-core-11-dpo.md` — движок 11.DPO.
- `codex-recon-core-42-wiki.md` — Bootstrap-юниты и flow-адаптеры.
- `codex-recon-core-26-router.md` — keyword-router как анти-паттерн.
- `codex-recon-mvp-internal-learn.md`, `...-internal-learn-2.md` — метод
  авто-построения пар, провал калибровки 2026-05-06, формат `[WRONG:]`.
- `codex-recon-mvp-external-sources.md` — лицензии/provenance внешних источников.
- `codex-recon-mvp-learning-sets.md`, `codex-recon-mvp-plan-set.md` — формат
  `A→узел→B`, 9-узловой граф, источники негативов.

**Реальные файлы кода (проверены, file:line):**
- `C:/Projects/llm_mvp_core/11.DPO/Models/DpoPair.cs:3-8`
- `C:/Projects/llm_mvp_core/11.DPO/DpoDataset.cs:18-38`
- `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:31-70`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiContextRoutingDpoGenerator.cs:17-94, 96-212, 214-290`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs:96-262`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanContracts.cs:110-122`
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationKnnStore.cs:32-62`
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/ResultGapDpoWriters.cs:7-14, 111-197, 199-235, 241-266`
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/PromptToTaskContracts.cs:44-59`
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/PromptToTaskAblationHarness.cs:535-549`

**Реальные файлы данных (проверены первые строки):**
- `C:/Projects/llm_mvp_core/training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl`
- `C:/Projects/llm_mvp_core/training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl`
- `C:/Projects/llm_mvp_core/training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed.jsonl`

**Скилы/гайды (правила):**
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/dpo-triple-format-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/planning-dpo-authoring-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/dcm-dpo-construction-GUIDE.md`
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md`

---

*Конец гайда 01. PLAN-ONLY: документация внутри плана ILearningDynamicGraph. Не
разрешает реализацию, тренировку, билд, запуск раннеров, git или вызов внешних
моделей. Цитаты кода — только реально существующие, с file:line.*
