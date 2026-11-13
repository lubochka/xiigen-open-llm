# Codex Recon — core/43.SmartSkillsDomain (юниты выбора скилов)

Вопрос задачи: какие юниты/контракты отвечают за **выбор скилов**, и нет ли
детерминистического словаря/счётчика, выступающего **финальной авторитетностью**.

Короткий ответ: финальной детерминистической авторитетности нет. Выбор скила
делают типизированные обучаемые `ILogicalUnit` (11 штук) под Level-5
`ILearningMachine<SkillSelectionDomain>`. Счётчик позиции кандидата
`_candidatePositionWeights` намеренно выведен из решения и обёрнут в guard-юнит
как диагностика/provenance. Есть отдельный устаревший
`SkillSelectionOrchestrator` (contract-only заглушка), который ничего не решает.

---

## 1. Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SkillSelectionMachine.cs`
  — главный класс `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>,
  IMachineTraining<SkillSelectionDomain>` (Level 5). Содержит маршрутизацию
  решения, тренировку, checkpoint/manifest/export/import, eval-gate.
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SkillSelectionContracts.cs`
  — все record-контракты: `SkillSelectionRequest`, `SkillSelectionDecision`,
  типизированные `*SkillUnitState` (trained state), `*SkillProfile`,
  `*KnnExample`, `SkillSelectionCheckpoint`, манифест, DPO-пары
  (`SkillSelectionMachinePair`), feedback/adaptive-сигналы. Также здесь живут
  bootstrap-классы `ResultGapMachine` / `TaskCompletionMachine` и
  user-local адаптивные writer/trainer.
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs`
  — Level 4/3/2: `SkillSelectionLogicalOrchestrator`, `SkillSelectionLogicalFlow`,
  4 адаптерных flow-юнита + `CandidateOrderLeakageGuardUnit` + 11 семантических
  `*SkillUnit` (PromptUnderstanding, TaskUnderstanding, TaskType, Routing,
  SkillCandidates, SkillSelection, PromptContext, ResultGap, OutputValidation,
  TaskCompletion, IntermediateChecks).
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SmartSkillsSemanticUnitCatalog.cs`
  — реестр 11 доменов; у каждого `Trainable: true`,
  `DeterministicFinalAuthority: false`, `RuntimeSemanticQualityClaimed: false`.
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SkillSelectionDomain.cs`
  — пустой маркер-тип `SkillSelectionDomain` (TDomain).
- `C:/Projects/llm_mvp_core/43.SmartSkillsDomain/SkillSelectionOrchestrator.cs`
  — отдельный устаревший `ILogicalOrchestrator`, всегда возвращает пустой выбор
  (`S1_CONTRACT_ONLY`, `NO_TRAINED_SEMANTIC_SELECTION`). Не путать с рабочим
  `SkillSelectionLogicalOrchestrator` из Hierarchy-файла.

## 2. Как устроено / работает (кратко)

Цепочка уровней (соответствует ILearningMachine-иерархии проекта):

`SkillSelectionLogicalOrchestrator` (L4) -> `SkillSelectionLogicalFlow` (L3) ->
4 адаптерных `ILogicalUnit` (инвентарь/checkpoint-boundary/decision-path/
authority-boundary) + `CandidateOrderLeakageGuardUnit` -> делегирует в
`SkillSelectionMachine.DecideFromTrainedState` (L5), который вызывает
`TryDecideWithSemanticUnitsOnly` и перебирает 11 типизированных `*SkillUnit` (L2).

Решение в `DecideFromTrainedState` (SkillSelectionMachine.cs:107-143):
1. сначала `TryDecideWithSemanticUnitsOnly` — приоритет у финального
   `SkillSelectionSkillUnit`, затем prompt_context, result_gap,
   output_validation, task_completion, intermediate_checks, skill_candidates,
   routing, task_type, task_understanding, prompt_understanding (порядок задан
   явно, строки 173-216);
2. если ни один семантический юнит не дал решение — возвращается ПУСТОЙ выбор
   с `RequiresSecondPass=true`, и приклеивается отчёт guard-юнита. Никакого
   выбора по счётчику позиции не происходит.

Каждый `*SkillUnit.Evaluate` (например PromptUnderstandingSkillUnit:437-495):
- берёт типизированный trained state (`*SkillUnitState` с `SkillProfiles`,
  `*Vocabulary`, у части — `KnnMemory`);
- извлекает признаки промпта через `ExtractPromptUnderstandingSignals`;
- по каждому доступному скилу суммирует **обученные веса**
  `profile.*FeatureWeights.GetValueOrDefault(signal)` и берёт argmax;
- если суммарный счёт `<= 0` — `NoDecision()` (нет авторитетности «по умолчанию»);
- TaskType/Routing (и др. с KNN) дополнительно считают KNN-поддержку
  `FindNearest*Examples` строго как `knn_role=support_evidence_only` —
  только в evidence, в выбор argmax она не входит.

Тренировка: `TrainBatchAsync` / `TrainMachineBatchAsync` -> `LearnPair` ->
по `TargetTrainingAdapter` направляет пару в нужный `Learn*Pair`, который
обновляет веса в соответствующем training-state. `_candidatePositionWeights`
тоже инкрементится (строка 767: `+ 1`), но только для guard/диагностики.

Признаки (feature extractor) — детерминированы:
`ExtractPromptUnderstandingSignals` (SkillSelectionMachine.cs:2726) — токенизация:
буквенно-цифровые токены длиной >=4, lower, distinct, sort. Контекст-сигналы
(`ExtractPromptContextSignals`:2738) добавляют префиксы `prompt:` / `host_key:` /
`host_value:` и редактируют path-like/PII (`IsSafeContextToken` фильтрует
`users`/`projects`/цифры). Это извлечение фич, не решение.

Lifecycle присутствует: `ExportCheckpoint`/`LoadCheckpoint`,
`SaveCheckpointAsync` (atomic + отдельный manifest),
`ImportAsync` (валидирует machine id/domain/manifest), статический
`LoadCheckpointAsync` (fresh-load), `EvaluateAsync` (held-out, gate 0.8).
Все checkpoint/manifest помечены `SemanticQualityClaimed: false`.

## 3. Релевантность к ILearningDynamicGraph

Высокая — это самый зрелый в репозитории пример «много обучаемых юнитов под
одной машиной», прямой прототип для DPO-графа:

- **DPO**: `SkillSelectionMachinePair(Chosen/Rejected, RejectedType,
  SourcePromptEntryId, TargetTrainingAdapter)` — готовая форма пары. DPO-строка
  строится из `LearningTrainingItem.ChosenResponse/RejectedResponse`
  (`BuildMachinePair`, :543). Адаптивные user-local сигналы:
  `ResultGapAdaptiveDpoSignal`, `SkillSelectionCorrectionAdaptiveDpoSignal`
  (chosen/rejected refs, provenance, `StorageScope`, `RouteMode`).
- **Обучение/state**: каждый узел = типизированный `*SkillUnitState` +
  `*SkillProfile` (веса) + опц. `*KnnExample` (память). Это шаблон узла графа
  с собственным trained-state и update-алгоритмом.
- **Граф/оркестрация**: L3 flow последовательно гоняет узлы и собирает
  evidence/warnings; L5 `DecideFromTrainedState` — fan-in с явным приоритетом.
  Для DynamicGraph здесь не хватает рёбер/many-to-many/async — см. «Пробелы».
- **Измерение**: `LearningBatchResult` (Accuracy before/after, RewardMargin,
  CpuMs, RamMb, PromotedUnitIds), `LearningEvaluationResult` (held-out gate),
  per-unit checkpoint section count.
- **teacher/provenance**: пары несут `SourcePromptLedgerId`/`ProvenanceRefs`,
  user-local сигналы запрещают raw private text (`ContainsRawPrivateText=false`)
  и мутацию common-checkpoint (`CommonCheckpointMutationClaimed=false`).
  Явного teacher-license поля (по Legal Teacher Gate скила) в контрактах нет —
  это нужно добавлять в DynamicGraph-плане.

## 4. Классификация (по trainable-architecture-decision-SKILL.md)

| Механизм (file) | Уровень / роль | Вердикт |
|---|---|---|
| `SkillSelectionMachine` (Machine.cs:10) | `ILearningMachine` L5 + `IMachineTraining` | `OK_ALLOWED_LEVEL` — есть train/eval/export/import/checkpoint/manifest/fresh-load |
| 11 `*SkillUnit` (Hierarchy.cs:429+) | `ILogicalUnit` L2, learned weight-profile argmax | `OK_ALLOWED_LEVEL` — решение по обученным весам, не по фикс. словарю |
| KNN в TaskType/Routing/etc (`FindNearest*`) | компонент модели внутри L2, `knn_role=support_evidence_only` | `OK_MODEL_COMPONENT` — в выбор не входит, только evidence |
| `_candidatePositionWeights` (Machine.cs:65, инкремент :767) | внутреннее состояние, НЕ уровень | НЕ финальная авторитетность: задемпфировано (`RouteMode=candidate_position_scaffold_not_final_semantic_authority`), выбор идёт раньше через семантические юниты |
| `CandidateOrderLeakageGuardUnit` (Hierarchy.cs:271) | `ILogicalUnit` L2, `Role=..._diagnostics_provenance_only`, `CanSelectSkill=false` | `OK_INTERNAL_DETAIL` — это guard от order-leakage (permutation-проба current/reversed), а не селектор |
| `ExtractPromptUnderstandingSignals` (Machine.cs:2726) | детерминированный токенайзер-фич | `OK_INTERNAL_DETAIL` — допустимый источник признаков внутри L2; не авторитетность |
| `SkillSelectionLogicalFlow` / `...Orchestrator` (Hierarchy.cs:6,47) | `ILogicalOrchestrator`/`ILogicalFlow` L4/L3 адаптеры | `OK_ALLOWED_LEVEL` — делегируют в machine, не добавляют ранжирование |
| `ResultGapMachine`, `TaskCompletionMachine` (Contracts.cs:927,984) | bootstrap-эвристики, помечены `bootstrap_test_harness_not_semantic_authority` | `BASELINE_MUST_BE_LABELLED` — честно помечены; финальный путь идёт через `ResultGapSkillUnit`/`TaskCompletionSkillUnit` |
| `SkillSelectionOrchestrator` (Orchestrator.cs:6) | устаревшая contract-only заглушка | `OK_INTERNAL_DETAIL` — ничего не решает (`NO_TRAINED_SEMANTIC_SELECTION`); кандидат на удаление/доисследование |

Главный вывод по вопросу: **детерминистического словаря или счётчика как
финальной авторитетности выбора скила НЕТ.** Единственный счётчик
(`_candidatePositionWeights`) явно классифицирован как diagnostics/provenance и
заблокирован guard-юнитом; решение принимают обученные `ILogicalUnit` под
`ILearningMachine`. Все юниты декларируют `SemanticQualityClaimed=false`
(честный untrained/baseline-scaffold статус, не ложное «обучено»).

## 5. Пробелы / что доисследовать

- **Веса узлов — счётчики (`Dictionary<string,int>` + `+1`)**, а не нормированные
  DPO-веса/margin. Для ILearningDynamicGraph по скилу это «learned counter
  baseline»: допустимо как baseline, но `RewardMargin` сейчас = разница accuracy,
  а не DPO reward-margin на уровне пары. Стоит решить, остаётся ли counter или
  заменяется на типизированный preference-weight update.
- **Нет рёбер/контрактов графа**: текущая композиция — последовательный перебор
  с приоритетом (fan-in), без явных edge-контрактов, many-to-many, fan-out,
  async-исполнения, и без per-edge владельца авторитетности. Graph-Style Gate
  скила требует это реконструировать в плане.
- **Legal Teacher / provenance поля**: в DPO-контрактах нет
  `teacher_access_mode/hosting_provider/model_id/model_license/...`. Для
  DPO-графа их надо ввести (особенно если источник пар — внешняя модель).
- **`SkillSelectionOrchestrator.cs` vs `SkillSelectionLogicalOrchestrator`**:
  две сущности с похожими именами; неясно, кто зарегистрирован в
  `15.OrchestratorModel`. Доисследовать регистрацию/locator (вне этой области).
- **Реальные `PromptUnderstandingMachine`/`TaskTypeMachine` и т.д. из каталога
  (MachineName) не найдены файлами в области 43** — в коде вместо отдельных
  машин используются `*SkillUnit` внутри `SkillSelectionMachine`. Каталог
  называет машины, которых как отдельных классов, похоже, нет. Проверить
  расхождение каталог↔код при планировании.
- **Не дочитаны полностью** (объём): хвост `SkillSelectionLogicalHierarchy.cs`
  (после ~1162: SkillCandidates/SkillSelection/ResultGap/OutputValidation/
  TaskCompletion/IntermediateChecks units + training-state классы) и середина
  `SkillSelectionMachine.cs` (1075-2700: остальные `Learn*`/`Export*`,
  training-state классы, валидация checkpoint/manifest). По прочитанным юнитам
  паттерн идентичен (argmax по обученным весам, KNN как support-only), но при
  написании плана это нужно подтвердить построчно.
- Не читались: `OrchestrationReasoning*`, `PromptToTask*`, `ResultGapDpoWriters.cs`,
  `ExtensionEvalOnlyQualityRunner*`, `Quality03*` — вне узкого вопроса о выборе
  скилов, но релевантны для DPO-раннеров/измерения ILearningDynamicGraph.
