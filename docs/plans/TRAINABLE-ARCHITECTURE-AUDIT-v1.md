# Trainable Architecture Audit v1

Статус: архитектурный аудит и карта исправлений. Это не разрешение на
реализацию и не claim, что модели уже обучены до semantic quality.

## Current-turn target gate

Этот документ является текущим user-visible target artifact для этой задачи:

```text
required_user_visible_artifact = docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md
current_request = довести audit plan до состояния, где видно: было -> что вычисляет -> чем заменить -> какие сеты -> какие тесты -> как обучать
stale_artifact_pointers_quarantined = ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md is historical context only for this turn
broad_search_used_as_completion_evidence = false
governance_repair_substitutes_for_artifact = false
```

Приёмка этого документа невозможна по факту "правила обновлены где-то ещё".
Приёмка возможна только если именно этот файл содержит конкретный repair для
каждого подозрительного поля/метода.

Область проверки:

- `43.SmartSkillsDomain`:
  `SkillSelectionMachine`, `PromptToTaskMachine`,
  `OrchestrationReasoningMachine`.
- `42.ProjectWikiDomain`:
  `OrchestrationPlanMachine`, `PromptFlowGroundingModel`,
  `OrchestrationKnnStore`.
- Связанные модели и обучающие материалы:
  `models/skill-selection`, `models/prompt-to-task`,
  `models/orchestration-reasoning`, `training-data/smart-skills`,
  `training-data/orchestration-reasoning`.

## Главный вывод человеческим языком

В проекте разрешены только такие уровни обучаемой архитектуры:

- `ILearningMachine<TDomain>`;
- `ILogicalOrchestrator`;
- `ILogicalFlow<TInput>`;
- `ILogicalUnit<TInput,TOutput>`;
- `ILearningMachinePair<TPersonal,TGlobal>`.

Счётчик, словарь, scorer, selector, KNN, regex, private `TryDecide*` метод или
`ChooseBestPattern` не становятся новым design pattern сами по себе.

Но это не значит, что они запрещены.

Правильное правило:

- они могут быть внутренней частью модели;
- они могут быть baseline на ранней стадии;
- они могут быть memory/retrieval/feature layer;
- они не могут быть выданы за полноценный обученный semantic unit без честных
  gates.

KNN отдельно: KNN не плохой и не forbidden. KNN может быть частью архитектуры
модели. Проверять надо не факт наличия KNN, а его роль: это компонент модели
или он тихо заменяет всю обучаемую единицу.

## Как читать этот отчёт

Для каждого места ниже есть:

1. Что код делает сейчас.
2. Короткая точная цитата.
3. Почему это может быть проблемой.
4. Было -> должно быть.
5. Какие модели/сеты затронуты.
6. Какие тесты должны закрыть риск.
7. Конкретный repair: какой разрешённый уровень появится, какой алгоритм он
   будет выполнять, какие строки данных переписать, какие тесты добавить.

## 1. SkillSelectionMachine

Файл: `43.SmartSkillsDomain/SkillSelectionMachine.cs`

### Что видно в коде

Класс действительно находится внутри разрешённого уровня:

```csharp
public sealed class SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>, IMachineTraining<SkillSelectionDomain>
```

Это хорошо: верхний контейнер архитектурно допустим.

Но внутри него сейчас много обученных счётчиков и словарей:

```csharp
private readonly Dictionary<int, int> _candidatePositionWeights = new();
private readonly Dictionary<string, Dictionary<string, int>> _promptUnderstandingSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _taskUnderstandingSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _promptContextSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _intermediateChecksSignals = new(StringComparer.Ordinal);
```

### Блокирующий repair прямо под quote block: было -> должно стать

Этот раздел является частью качества плана, а не будущей заметкой для executor.
Если хоть одна строка ниже останется на уровне "создать schema / chosen /
rejected / quarantine" без заполненного класса, алгоритма, DPO-примера, теста и
lifecycle, этот audit сам себя считает недоведённым до нормального уровня.

Общее правило для всех пяти замен:

- `ILogicalUnit` остаётся pure inference: у него нет `Train*` методов;
- обучение идёт через `SkillSelectionMachine.TrainMachineBatchAsync(...)`;
- `TrainMachineBatchAsync(...)` вызывает
  `SkillSelectionTrainingPipeline.Update*State(...)`;
- typed state хранится в checkpoint/manifest и загружается перед inference;
- K-NN (K-Nearest Neighbours) внутри unit является support/memory layer, а не
  самостоятельным selector/scorer pattern;
- common обучение берёт только repo/common rows и внешние MIT/Apache-2.0 rows,
  прошедшие license gate;
- user-adaptive обучение берёт Luba/user/project rows из local cache и не
  сливается молча в common.

#### 1. Было `_candidatePositionWeights` -> должно стать `CandidateOrderLeakageGuardUnit`

**Было:** `Dictionary<int,int>` хранит `candidate_index -> chosen_count`.
Такой state учит позицию кандидата, а не смысл запроса.

**Должно стать, точная форма кода:**

```text
43.SmartSkillsDomain/SkillSelection/Units/CandidateOrderLeakageGuardUnit.cs
  sealed class CandidateOrderLeakageGuardUnit
    : ILogicalUnit<CandidateOrderLeakageInput, CandidateOrderLeakageReport>

43.SmartSkillsDomain/SkillSelection/States/CandidateOrderLeakageGuardState.cs
  sealed record CandidateOrderLeakageGuardState(
      string UnitId,
      string CheckpointHash,
      IReadOnlyList<CandidateOrderLeakageCase> OrderSensitiveCases,
      IReadOnlyList<CandidateOrderLeakageCase> OrderStableCases,
      IReadOnlyList<string> TrainingPairIds,
      bool SemanticQualityClaimed)

43.SmartSkillsDomain/SkillSelection/Training/SkillSelectionTrainingPipeline.cs
  CandidateOrderLeakageGuardState UpdateCandidateOrderLeakageState(
      CandidateOrderLeakageGuardState current,
      IReadOnlyList<SkillSelectionDpoPair> pairs,
      SkillSelectionLicenseGateResult licenseGate)
```

**Call chain:** `SkillSelectionMachine.RunSelectionAsync(request)` вызывает
основные semantic units, затем
`CandidateOrderLeakageGuardUnit.ExecuteAsync(LogicalUnitRequest<CandidateOrderLeakageInput>)`.
Если report возвращает `OrderSensitive=true`, `SkillSelectionEvidenceArbiter`
блокирует любые `candidate_position` признаки и пересчитывает решение без них.

**Алгоритм обучения:** `UpdateCandidateOrderLeakageState` строит минимум три
перестановки `AvailableSkillIds` для каждой DPO-строки, сравнивает selected
skill, добавляет row в `OrderStableCases`, если chosen skill не меняется, и в
`OrderSensitiveCases`, если меняется только из-за порядка. Позиция кандидата не
прибавляет вес skill; она может попасть только в guard/provenance.

**K-NN методы и роль:**

```csharp
IReadOnlyList<CandidateOrderLeakageCase> FindNearestOrderSensitiveCases(
    CandidatePermutationVector vector,
    int k);
double ComputeOrderLeakageRisk(
    CandidatePermutationVector vector,
    IReadOnlyList<CandidateOrderLeakageCase> neighbours);
```

K-NN объясняет похожие leakage cases и поднимает risk. Он не выбирает skill.

**DPO и license/source gate:**

- source roots:
  `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl`,
  `training-data/smart-skills/semantic-quality/skill-selection-training-v1/`;
- external common rows запрещены, если `license_allowed` не `MIT` или
  `Apache-2.0`;
- local user rows остаются в user-adaptive leg.

**Конкретные DPO rows:**

```json
{"row_id":"ss-order-positive-001","source_ref":"training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl#demo-order-001","license_or_private_leg":"repo_common","prompt":"проверь план на архитектурные дыры","available_a":["plan-review","code-review","documentation-sync"],"available_b":["documentation-sync","plan-review","code-review"],"chosen":"plan-review","rejected":"documentation-sync","state_fields_updated":["OrderStableCases += row_id","TrainingPairIds += row_id"],"knn_memory_entry_added":"CandidatePermutationVector(prompt_features=['plan','architecture_review'], stable_chosen='plan-review')","checkpoint_written":"models/skill-selection/common/CandidateOrderLeakageGuardState.json","label":"KEEP_FOR_TRAINING","why":"chosen стабилен при перестановке"}
{"row_id":"ss-order-rejected-001","source_ref":"training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl#demo-order-002","license_or_private_leg":"repo_common_quarantine","prompt":"проверь план на архитектурные дыры","available_a":["plan-review","code-review"],"available_b":["code-review","plan-review"],"chosen_a":"plan-review","chosen_b":"code-review","wrong_signal":"candidate_position","state_fields_updated":["OrderSensitiveCases += row_id","BlockedFeatures += candidate_position"],"knn_memory_entry_added":"CandidatePermutationVector(order_sensitive=true, changed_only_by_order=true)","checkpoint_written":"models/skill-selection/common/CandidateOrderLeakageGuardState.json as diagnostics only","label":"QUARANTINE_DO_NOT_TRAIN","why":"решение меняется только из-за candidate order"}
{"row_id":"ss-order-quarantine-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/*#order-leak-demo","license_or_private_leg":"repo_common_quarantine","prompt":"обнови документацию","available_a":["documentation-sync","plan-review"],"chosen":"documentation-sync","source_position_hint":"chosen всегда index 0","state_fields_updated":["quarantine_index += row_id only"],"knn_memory_entry_added":"none; leakage row is not nearest-memory training","checkpoint_written":"false for trainable state; row copied to training-data/smart-skills/quarantine/skill-selection-order-leakage.jsonl","label":"QUARANTINE_DO_NOT_TRAIN","why":"row учит позицию, а не смысл"}
```

**Affected sets and rebuild/quarantine:** все rows с
`candidate_order_invariant=false` переносятся в quarantine file
`training-data/smart-skills/quarantine/skill-selection-order-leakage.jsonl`.
Train-ready copy rebuild допускается только после shuffle audit.

**Тесты:**

- `CandidateOrderLeakageGuardUnit_DetectsOrderSensitiveDecision`: assertion
  `report.OrderSensitive == true` и `report.BlockedFeatures` содержит
  `candidate_position`;
- `SkillSelectionMachine_IgnoresCandidatePositionForSemanticDecision`: assertion
  одинаковый prompt и разные permutations дают один `SelectedSkillId`;
- `SkillSelectionMachine_DoesNotExportCandidatePositionAsFinalAuthority`:
  checkpoint содержит `candidate_position_role=diagnostics_only`.

**Lifecycle:** load order:
`.xiigen/smart-skills/user/CandidateOrderLeakageGuardState.json` first,
`models/skill-selection/common/CandidateOrderLeakageGuardState.json` second.
Если оба отсутствуют, unit возвращает `HonestUntrainedScaffold`. Backup/export
пишет user state в `.xiigen/backups/smart-skills/`; import/share требует явного
user action; continue-training начинает с загруженного checkpoint hash.

#### 2. Было `_promptUnderstandingSignals` -> должно стать `PromptUnderstandingSkillUnit`

**Было:** `Dictionary<string, Dictionary<string,int>>` хранит
`skill_id -> prompt_signal -> count`. Это keyword counter baseline.

**Должно стать, точная форма кода:**

```text
43.SmartSkillsDomain/SkillSelection/Units/PromptUnderstandingSkillUnit.cs
  sealed class PromptUnderstandingSkillUnit
    : ILogicalUnit<PromptUnderstandingSkillInput, PromptUnderstandingSkillEvidence>

43.SmartSkillsDomain/SkillSelection/States/PromptUnderstandingSkillUnitState.cs
  sealed record PromptUnderstandingSkillUnitState(
      string UnitId,
      string CheckpointHash,
      IReadOnlyList<string> PromptFeatureVocabulary,
      IReadOnlyList<string> SkillVocabulary,
      float[][] PromptToSkillWeights,
      float[] SkillBias,
      PromptUnderstandingCalibration Calibration,
      IReadOnlyList<PromptUnderstandingKnnExample> KnnMemory,
      IReadOnlyList<string> TrainingPairIds,
      bool SemanticQualityClaimed)

43.SmartSkillsDomain/SkillSelection/Training/SkillSelectionTrainingPipeline.cs
  PromptUnderstandingSkillUnitState UpdatePromptUnderstandingState(
      PromptUnderstandingSkillUnitState current,
      IReadOnlyList<SkillSelectionDpoPair> pairs,
      SkillSelectionLicenseGateResult licenseGate)
```

**Call chain:** `SkillSelectionMachine.RunSelectionAsync(request)` builds
`PromptUnderstandingSkillInput` from normalized prompt, available skills and
skill descriptions, then calls
`PromptUnderstandingSkillUnit.ExecuteAsync(...)`; result contributes evidence,
not final authority by itself.

**Алгоритм обучения:** для каждой DPO-пары берутся prompt features из chosen и
rejected explanations. Для features, которые поддержали chosen skill,
`PromptToSkillWeights[feature][chosenSkill] += learningRate`; для features,
которые поддержали rejected keyword-only match,
`PromptToSkillWeights[feature][rejectedSkill] -= learningRate`. Затем
calibration обновляет confidence по held-out paraphrases. Exact prompt id,
source file, pair id и candidate order не попадают в feature vocabulary.

**K-NN методы и роль:**

```csharp
IReadOnlyList<PromptUnderstandingKnnExample> FindNearestPromptExamples(
    PromptFeatureVector promptVector,
    int k);
PromptUnderstandingSupport ComputePromptKnnSupport(
    PromptFeatureVector promptVector,
    string skillId,
    int k);
```

K-NN показывает похожие paraphrase examples и confidence support. Exact prompt
replay запрещён: если nearest distance равен exact/source match, example
помечается `ExactMemoryOnly` и не участвует в semantic claim.

**DPO и license/source gate:**

- common roots: repo skill descriptions, approved prompt-to-task examples,
  universalized MIT/Apache-2.0 instruction/task examples;
- user roots: `C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts`,
  `sonnet-user-prompts`, user corrections;
- `SkillSelectionLicenseGate.VerifyForCommonLearning(source)` должен вернуть
  `Allowed=true` и `License in {MIT, Apache-2.0}` перед попаданием во common.

**Конкретные DPO rows:**

```json
{"row_id":"ss-prompt-positive-001","source_ref":"C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts#redacted-plan-review-001","license_or_private_leg":"user_adaptive_private","prompt":"проверь план на архитектурные дыры и дырки в gate","available":["plan-review","generated-code-review","documentation-sync"],"chosen":"plan-review","rejected":"generated-code-review","state_fields_updated":["PromptToSkillWeights['plan_quality_gap']['plan-review'] += lr","PromptToSkillWeights['generated_code']['generated-code-review'] -= lr","KnnMemory += paraphrase example"],"knn_memory_entry_added":"PromptUnderstandingKnnExample(prompt_features=['plan','gate','architecture_hole'], chosen='plan-review')","checkpoint_written":".xiigen/smart-skills/user/PromptUnderstandingSkillUnitState.json","label":"KEEP_FOR_TRAINING","why":"prompt просит проверить план, а не уже сгенерированный код"}
{"row_id":"ss-prompt-rejected-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/prompt-review-nearmiss-demo","license_or_private_leg":"repo_common","prompt":"review generated class for silent failure","available":["generated-code-review","plan-review"],"chosen":"generated-code-review","rejected":"plan-review","wrong_signal":"keyword review -> plan-review","state_fields_updated":["PromptToSkillWeights['generated_class']['generated-code-review'] += lr","PromptToSkillWeights['review_keyword']['plan-review'] -= lr"],"knn_memory_entry_added":"PromptUnderstandingKnnExample(prompt_features=['generated_class','silent_failure'], chosen='generated-code-review')","checkpoint_written":"models/skill-selection/common/PromptUnderstandingSkillUnitState.json","label":"KEEP_FOR_TRAINING","why":"слово review не должно автоматически выбирать plan-review"}
{"row_id":"ss-prompt-quarantine-001","source_ref":"C:/Projects/llm_mvp_core/docs/plans/X.md#local-path-demo","license_or_private_leg":"user_adaptive_or_evidence_only","prompt":"как в файле C:/Projects/llm_mvp_core/docs/plans/X.md","chosen":"plan-review","rejected":"documentation-sync","state_fields_updated":["quarantine_index += row_id only"],"knn_memory_entry_added":"none; exact local path is forbidden prompt feature","checkpoint_written":"false for common; optional user-adaptive quarantine log only","label":"QUARANTINE_DO_NOT_TRAIN","why":"ответ можно угадать по local path/source id"}
```

**Affected sets and rebuild/quarantine:** rows из user prompts с private paths
идут только в user-adaptive checkpoint. Common rebuild удаляет local paths,
source ids, pair ids, exact prompt hashes и сохраняет только universal prompt
meaning.

**Тесты:**

- `PromptUnderstandingSkillUnit_SelectsPlanReviewForArchitecturePlanPrompt`:
  assertion chosen evidence top skill is `plan-review`;
- `PromptUnderstandingSkillUnit_DoesNotUseExactPromptIdAsFeature`: assertion
  exported vocabulary does not contain `source_id`, `prompt_hash`, local path;
- `PromptUnderstandingSkillUnit_ParaphraseKeepsSkillStable`: paraphrases of
  same plan-review request keep same top skill.

**Lifecycle:** user checkpoint
`.xiigen/smart-skills/user/PromptUnderstandingSkillUnitState.json` loads first;
common checkpoint `models/skill-selection/common/PromptUnderstandingSkillUnitState.json`
loads second; backup/export/import/share preserve `TrainingPairIds` and
license provenance; continue-training starts from `CheckpointHash`, not empty
weights.

#### 3. Было `_taskUnderstandingSignals` -> должно стать `TaskUnderstandingSkillUnit`

**Было:** `Dictionary<string, Dictionary<string,int>>` хранит
`skill_id -> task_signal -> count`. Это счётчик task labels, а не понимание
границы задачи.

**Должно стать, точная форма кода:**

```text
43.SmartSkillsDomain/SkillSelection/Units/TaskUnderstandingSkillUnit.cs
  sealed class TaskUnderstandingSkillUnit
    : ILogicalUnit<TaskUnderstandingSkillInput, TaskUnderstandingSkillEvidence>

43.SmartSkillsDomain/SkillSelection/States/TaskUnderstandingSkillUnitState.cs
  sealed record TaskUnderstandingSkillUnitState(
      string UnitId,
      string CheckpointHash,
      IReadOnlyList<string> TaskKindVocabulary,
      IReadOnlyList<string> DeliverableVocabulary,
      IReadOnlyList<string> RiskFlagVocabulary,
      IReadOnlyList<string> SkillVocabulary,
      float[][] TaskToSkillWeights,
      TaskUnderstandingCalibration Calibration,
      IReadOnlyList<TaskUnderstandingKnnExample> KnnMemory,
      IReadOnlyList<string> TrainingPairIds,
      bool SemanticQualityClaimed)

43.SmartSkillsDomain/SkillSelection/Training/SkillSelectionTrainingPipeline.cs
  TaskUnderstandingSkillUnitState UpdateTaskUnderstandingState(
      TaskUnderstandingSkillUnitState current,
      IReadOnlyList<SkillSelectionDpoPair> pairs,
      SkillSelectionLicenseGateResult licenseGate)
```

**Call chain:** `PromptToTaskMachine` produces `StructuredTaskContract`;
`SkillSelectionMachine` passes it to
`TaskUnderstandingSkillUnit.ExecuteAsync(...)`; evidence says which skill fits
task kind, deliverable and risk flags.

**Алгоритм обучения:** для each DPO pair parse `task_kind`,
`requested_artifact_type`, `risk_flags`, `deliverable_scope`. Chosen skill
получает positive update for `(task_kind, deliverable, risk)` features; rejected
skill получает negative update when it matches соседнюю, но неверную task
boundary. Project file paths and execution recipes are stripped before common.

**K-NN методы и роль:**

```csharp
IReadOnlyList<TaskUnderstandingKnnExample> FindNearestTaskExamples(
    TaskFeatureVector taskVector,
    int k);
double ComputeTaskBoundarySimilarity(
    TaskFeatureVector left,
    TaskFeatureVector right);
```

K-NN даёт near-miss examples: похожая task boundary, но другой правильный skill.
Он не подменяет classifier.

**DPO и license/source gate:**

- source roots: approved orchestration plans, prompt-to-task DPO pairs,
  `C:/Users/lubao/Documents/xiigen/sonnet local - analyzes/`,
  skill library descriptions;
- external common rows только MIT/Apache-2.0 and universalized;
- rows with local repo path stay adaptive-only unless transformed to universal
  task shape.

**Конкретные DPO rows:**

```json
{"row_id":"ss-task-positive-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/task-review-demo","license_or_private_leg":"repo_common","task_kind":"plan_review","requested_artifact":"findings_only","prompt":"review governance changes, no edits","chosen":"plan-review","rejected":"implementation-integrity","state_fields_updated":["TaskToSkillWeights['plan_review']['plan-review'] += lr","TaskToSkillWeights['code_integrity']['implementation-integrity'] -= lr"],"knn_memory_entry_added":"TaskUnderstandingKnnExample(task_kind='plan_review', artifact='findings_only', chosen='plan-review')","checkpoint_written":"models/skill-selection/common/TaskUnderstandingSkillUnitState.json","label":"KEEP_FOR_TRAINING","why":"artifact is review findings, not code integrity execution"}
{"row_id":"ss-task-rejected-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/task-code-repair-demo","license_or_private_leg":"repo_common","task_kind":"code_repair","requested_artifact":"production_code_change","prompt":"fix failing parser and add test","chosen":"implementation-integrity","rejected":"documentation-sync","wrong_signal":"task mentions fix but docs-only selected","state_fields_updated":["TaskToSkillWeights['code_repair']['implementation-integrity'] += lr","TaskToSkillWeights['documentation_only']['documentation-sync'] -= lr"],"knn_memory_entry_added":"TaskUnderstandingKnnExample(task_kind='code_repair', chosen='implementation-integrity')","checkpoint_written":"models/skill-selection/common/TaskUnderstandingSkillUnitState.json","label":"KEEP_FOR_TRAINING","why":"task requires code repair, docs alone are wrong"}
{"row_id":"ss-task-quarantine-001","source_ref":"C:/Projects/llm_mvp_core/private-plan.md#path-leak-demo","license_or_private_leg":"evidence_only","task_kind":"unknown","source_file":"C:/Projects/llm_mvp_core/private-plan.md","chosen":"plan-review","state_fields_updated":["quarantine_index += row_id only"],"knn_memory_entry_added":"none; source_file is forbidden task feature","checkpoint_written":"false","label":"QUARANTINE_DO_NOT_TRAIN","why":"chosen can be inferred from file/path source instead of task semantics"}
```

**Affected sets and rebuild/quarantine:** `prompt-to-task` rows feed this unit
only after `StructuredTaskContract` is present. Any row missing task kind,
deliverable and risk flags goes to
`training-data/smart-skills/quarantine/task-understanding-missing-contract.jsonl`.

**Тесты:**

- `TaskUnderstandingSkillUnit_DistinguishesReviewFromRepair`: same domain,
  different artifact type selects different skill;
- `TaskUnderstandingSkillUnit_RejectsPathBasedCommonFeature`: common vocabulary
  lacks local paths;
- `TaskUnderstandingSkillUnit_NearMissDoesNotCollapseToKeyword`: near-miss
  plan-review vs generated-code-review keeps correct boundary.

**Lifecycle:** user task state loads from
`.xiigen/smart-skills/user/TaskUnderstandingSkillUnitState.json`; common state
loads from `models/skill-selection/common/TaskUnderstandingSkillUnitState.json`;
backup/export/import/share keep `TaskKindVocabulary` and source license refs;
continue-training uses previous calibration and appends new pair ids.

#### 4. Было `_promptContextSignals` -> должно стать `PromptContextSkillSelectionUnit`

**Было:** `Dictionary<string, Dictionary<string,int>>` хранит
`skill_id -> prompt/context_signal -> count`, а private scorer суммирует prompt
score и context score.

**Должно стать, точная форма кода:**

```text
43.SmartSkillsDomain/SkillSelection/Units/PromptContextSkillSelectionUnit.cs
  sealed class PromptContextSkillSelectionUnit
    : ILogicalUnit<PromptContextSkillInput, PromptContextSkillEvidence>

43.SmartSkillsDomain/SkillSelection/States/PromptContextSkillSelectionUnitState.cs
  sealed record PromptContextSkillSelectionUnitState(
      string UnitId,
      string CheckpointHash,
      IReadOnlyList<string> PromptFeatureVocabulary,
      IReadOnlyList<string> ContextFeatureVocabulary,
      IReadOnlyList<string> SkillVocabulary,
      float[][] PromptWeights,
      float[][] ContextWeights,
      float[] BiasVector,
      PromptContextCalibration Calibration,
      IReadOnlyList<PromptContextKnnExample> KnnMemory,
      IReadOnlyList<string> TrainingPairIds,
      bool SemanticQualityClaimed)

43.SmartSkillsDomain/SkillSelection/Training/SkillSelectionTrainingPipeline.cs
  PromptContextSkillSelectionUnitState UpdatePromptContextState(
      PromptContextSkillSelectionUnitState current,
      IReadOnlyList<SkillSelectionDpoPair> pairs,
      SkillSelectionLicenseGateResult licenseGate)
```

**Call chain:** `SkillSelectionMachine.RunSelectionAsync(request)` builds
`PromptContextSkillInput` from prompt, current repo/task context, active mode,
artifact type and available skills. `PromptContextSkillSelectionUnit` returns
evidence with prompt contribution, context contribution and explanation.

**Алгоритм обучения:** chosen/rejected pair must contain same or similar prompt
with context delta. Update prompt weights only for prompt meaning; update
context weights only for context features that actually changed correct skill.
If context is missing, unit lowers confidence instead of inventing certainty.

**K-NN методы и роль:**

```csharp
IReadOnlyList<PromptContextKnnExample> FindNearestPromptContextExamples(
    PromptFeatureVector promptVector,
    ContextFeatureVector contextVector,
    int k);
PromptContextSupport ComputePromptContextSupport(
    PromptFeatureVector promptVector,
    ContextFeatureVector contextVector,
    string skillId,
    int k);
```

K-NN supports context-sensitive cases. It cannot use candidate index, source id
or exact prompt hash as a similarity shortcut.

**DPO и license/source gate:**

- source roots: plan-review/code-review/documentation context examples,
  current session context ledgers, user adaptive corrections;
- external common rows only MIT/Apache-2.0 workflow/planning examples after
  `SkillSelectionLicenseGate.VerifyForCommonLearning`;
- private session context stays user-adaptive.

**Конкретные DPO rows:**

```json
{"row_id":"ss-context-positive-001","source_ref":"C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts#redacted-context-review-001","license_or_private_leg":"user_adaptive_private","prompt":"проверь это","context":"artifact=plan; mode=review-only; no edits","chosen":"plan-review","rejected":"implementation-integrity","state_fields_updated":["ContextWeights['artifact_plan']['plan-review'] += lr","ContextWeights['changed_code']['implementation-integrity'] -= lr","Calibration.context_required += 1"],"knn_memory_entry_added":"PromptContextKnnExample(prompt='проверь это', context_features=['artifact_plan','review_only'], chosen='plan-review')","checkpoint_written":".xiigen/smart-skills/user/PromptContextSkillSelectionUnitState.json","label":"KEEP_FOR_TRAINING","why":"контекст говорит review-only plan"}
{"row_id":"ss-context-rejected-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/context-code-review-demo","license_or_private_leg":"repo_common","prompt":"проверь это","context":"artifact=changed C# files; generated code review required","chosen":"generated-code-review","rejected":"plan-review","wrong_signal":"prompt-only short text ignored context","state_fields_updated":["ContextWeights['changed_code']['generated-code-review'] += lr","PromptWeights['check_this']['plan-review'] -= lr when context disagrees"],"knn_memory_entry_added":"PromptContextKnnExample(context_features=['changed_code','generated_code_review_required'], chosen='generated-code-review')","checkpoint_written":"models/skill-selection/common/PromptContextSkillSelectionUnitState.json","label":"KEEP_FOR_TRAINING","why":"тот же короткий prompt меняет skill из-за context"}
{"row_id":"ss-context-quarantine-001","source_ref":"training-data/smart-skills/semantic-quality/skill-selection-training-v1/context-shortcut-demo","license_or_private_leg":"repo_common_quarantine","prompt":"проверь это","context":"source_row_id=SS-42; chosen_skill_index=0","chosen":"plan-review","state_fields_updated":["quarantine_index += row_id only"],"knn_memory_entry_added":"none; source_row_id/chosen_skill_index forbidden","checkpoint_written":"false for trainable state","label":"QUARANTINE_DO_NOT_TRAIN","why":"context содержит shortcut, а не смысл"}
```

**Affected sets and rebuild/quarantine:** rows без явного `context_features`
нельзя тренировать для этого unit. Они идут в
`training-data/smart-skills/quarantine/prompt-context-missing-context.jsonl`
или используются только как prompt-understanding material.

**Тесты:**

- `PromptContextSkillSelectionUnit_ContextSwitchChangesSkill`: same prompt,
  different context changes top evidence as expected;
- `PromptContextSkillSelectionUnit_PromptOnlyReturnsLowConfidenceWhenAmbiguous`;
- `PromptContextSkillSelectionUnit_DoesNotUseCandidatePositionOrSourceId`.

**Lifecycle:** load
`.xiigen/smart-skills/user/PromptContextSkillSelectionUnitState.json` first,
`models/skill-selection/common/PromptContextSkillSelectionUnitState.json`
second; export/import includes context vocabulary, calibration and license
refs; share requires user action; continue-training appends new context rows
without erasing old calibration.

#### 5. Было `_intermediateChecksSignals` -> должно стать `IntermediateChecksSkillUnit`

**Было:** `Dictionary<string, Dictionary<string,int>>` хранит
`skill_id -> intermediate_check_signal -> count`. Это не объясняет, какой
review/check finding требует какой skill.

**Должно стать, точная форма кода:**

```text
43.SmartSkillsDomain/SkillSelection/Units/IntermediateChecksSkillUnit.cs
  sealed class IntermediateChecksSkillUnit
    : ILogicalUnit<IntermediateChecksSkillInput, IntermediateChecksEvidence>

43.SmartSkillsDomain/SkillSelection/States/IntermediateChecksUnitState.cs
  sealed record IntermediateChecksUnitState(
      string UnitId,
      string CheckpointHash,
      IReadOnlyList<string> CheckFeatureVocabulary,
      IReadOnlyList<string> SkillVocabulary,
      float[][] CheckToSkillWeights,
      IntermediateChecksCalibration Calibration,
      IReadOnlyList<IntermediateChecksKnnExample> KnnMemory,
      IReadOnlyList<string> TrainingPairIds,
      bool SemanticQualityClaimed)

43.SmartSkillsDomain/SkillSelection/Training/SkillSelectionTrainingPipeline.cs
  IntermediateChecksUnitState UpdateIntermediateChecksState(
      IntermediateChecksUnitState current,
      IReadOnlyList<SkillSelectionDpoPair> pairs,
      SkillSelectionLicenseGateResult licenseGate)
```

**Call chain:** after first-pass prompt/task/context evidence,
`SkillSelectionMachine` passes current findings to
`IntermediateChecksSkillUnit.ExecuteAsync(...)`; unit can add required skills
or raise confidence for a skill, but `SkillSelectionEvidenceArbiter` still
checks no source/path shortcut is used.

**Алгоритм обучения:** each DPO row includes `intermediate_findings`.
For finding features that require a skill, update `CheckToSkillWeights` toward
chosen. For rejected rows that ignore required finding or add unrelated skill,
apply negative update. Findings are normalized to universal labels:
`documentation_drift`, `test_scope_gap`, `plan_part_a_missing`,
`hidden_determinism`, not exact file paths.

**K-NN методы и роль:**

```csharp
IReadOnlyList<IntermediateChecksKnnExample> FindNearestCheckExamples(
    CheckFeatureVector checkVector,
    int k);
IntermediateChecksSupport ComputeCheckSupport(
    CheckFeatureVector checkVector,
    string skillId,
    int k);
```

K-NN explains which previous findings were similar. It cannot decide from
reviewer name, comment id, file path or source row id.

**DPO и license/source gate:**

- source roots: plan-review findings, code-review findings, documentation drift
  findings, dashboard QA findings, DPO-quality review findings;
- common rows require universalized finding labels and MIT/Apache-2.0 license
  for external examples;
- project/user review packets stay adaptive-only unless sanitized.

**Конкретные DPO rows:**

```json
{"row_id":"ss-check-positive-001","source_ref":"docs/plans/API-DASHBOARD-STATE.json#documentation-drift-demo","license_or_private_leg":"repo_common","intermediate_findings":["documentation_drift"],"current_task":"code slice accepted but public docs stale","chosen":"documentation-sync","rejected":"generated-code-review","state_fields_updated":["CheckToSkillWeights['documentation_drift']['documentation-sync'] += lr","CheckToSkillWeights['generated_code_review']['generated-code-review'] -= lr"],"knn_memory_entry_added":"IntermediateChecksKnnExample(check_features=['documentation_drift'], chosen='documentation-sync')","checkpoint_written":"models/skill-selection/common/IntermediateChecksUnitState.json","label":"KEEP_FOR_TRAINING","why":"finding requires docs sync skill"}
{"row_id":"ss-check-rejected-001","source_ref":"docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md#hidden-determinism-demo","license_or_private_leg":"repo_common","intermediate_findings":["hidden_determinism"],"current_task":"semantic claim uses dictionary counter","chosen":"trainable-architecture-decision","rejected":"test-integrity","wrong_signal":"test focus before architecture classification","state_fields_updated":["CheckToSkillWeights['hidden_determinism']['trainable-architecture-decision'] += lr","CheckToSkillWeights['test_gap']['test-integrity'] -= lr"],"knn_memory_entry_added":"IntermediateChecksKnnExample(check_features=['hidden_determinism'], chosen='trainable-architecture-decision')","checkpoint_written":"models/skill-selection/common/IntermediateChecksUnitState.json","label":"KEEP_FOR_TRAINING","why":"finding requires architecture classification before tests"}
{"row_id":"ss-check-quarantine-001","source_ref":"private-review-comment#123456","license_or_private_leg":"evidence_only","intermediate_findings":["comment_id=123456","file=C:/Projects/private/foo.cs"],"chosen":"code-examination","state_fields_updated":["quarantine_index += row_id only"],"knn_memory_entry_added":"none; comment id/file path are forbidden check features","checkpoint_written":"false","label":"QUARANTINE_DO_NOT_TRAIN","why":"finding signal is source/comment identity, not universal check semantics"}
```

**Affected sets and rebuild/quarantine:** review-derived rows must be rebuilt
with normalized finding labels. Any row where chosen skill can be inferred from
review file path, reviewer id, comment id or source row id goes to
`training-data/smart-skills/quarantine/intermediate-checks-source-leakage.jsonl`.

**Тесты:**

- `IntermediateChecksSkillUnit_DocumentationDriftSelectsDocumentationSync`;
- `IntermediateChecksSkillUnit_HiddenDeterminismSelectsTrainableArchitectureDecision`;
- `IntermediateChecksSkillUnit_DoesNotLearnCommentIdOrFilePath`;
- `IntermediateChecksSkillUnit_ContinueTrainingPreservesExistingFindings`.

**Lifecycle:** load
`.xiigen/smart-skills/user/IntermediateChecksUnitState.json` first,
`models/skill-selection/common/IntermediateChecksUnitState.json` second.
Backup/export/import/share preserve normalized labels, source-license refs and
pair ids. Continue-training starts from loaded `CheckToSkillWeights` and
calibration; it must not rebuild from zero or overwrite user corrections with
repo/common updates.

#### Общий lifecycle acceptance для пяти rows

`SkillSelectionMachine` is acceptable only when manifest/report can show:

```text
load_order=user_cache_then_repo_common
user_cache_path=.xiigen/smart-skills/user/<unit-state>.json
repo_common_path=models/skill-selection/common/<unit-state>.json
backup_path=.xiigen/backups/smart-skills/<timestamp>/<unit-state>.json
export_method=SkillSelectionModelExporter.ExportUnitStateAsync
import_method=SkillSelectionModelImporter.ImportUnitStateAsync
share_method=SkillSelectionUserStateSharePackage.CreateAsync
continue_training_method=SkillSelectionTrainingPipeline.Update<StateName>
checkpoint_continuity_assertion=new_checkpoint.ParentCheckpointHash == loaded.CheckpointHash
common_license_gate=MIT_or_Apache_2_0_only
```

#### Почему это не просто prompt/spec

Этот section fail, если review найдёт любое из следующего:

- row говорит только "создать schema" без class/file/record/method names;
- row говорит только "chosen/rejected/quarantine" без заполненного JSON example;
- row не называет `Update*State` algorithm and what weights/cases change;
- row не называет K-NN method signature and says only "use K-NN";
- row не разделяет common MIT/Apache-2.0 material and user-adaptive material;
- row не называет exact test names and assertions;
- row не описывает load user/cache first, repo/common second, backup, export,
  import, share and continue-training from checkpoint.

Полный участок: `43.SmartSkillsDomain/SkillSelectionMachine.cs:64-85`.

Затем runtime идёт по цепочке private decision methods:

```csharp
var promptContextDecision = TryDecideWithPromptContext(request);
...
var promptUnderstandingDecision = TryDecideWithPromptUnderstanding(request);
...
if (_candidatePositionWeights.Count == 0 || request.AvailableSkillIds.Count == 0)
```

Участок: `43.SmartSkillsDomain/SkillSelectionMachine.cs:116-162`.

Candidate-position обучение выглядит так:

```csharp
_candidatePositionWeights[index.Value] = _candidatePositionWeights.GetValueOrDefault(index.Value) + 1;
```

Участок: `43.SmartSkillsDomain/SkillSelectionMachine.cs:574-581`.

### Конкретный repair для `_candidatePositionWeights`

#### Как сейчас

Строка:

```csharp
private readonly Dictionary<int, int> _candidatePositionWeights = new();
```

создаёт память вида:

```text
позиция кандидата -> сколько раз skill на этой позиции был chosen
```

Например, если в обучающих строках правильный skill часто стоял первым, модель
может выучить не смысл задачи, а правило:

```text
если не уверен — выбирай skill на позиции 0
```

Это не semantic skill selection. Это order leakage: сигнал про порядок списка
кандидатов, а не про задачу пользователя.

#### Как должно быть

`_candidatePositionWeights` не должен участвовать в final skill selection.

Есть два допустимых целевых состояния:

1. **Минимальный repair:** удалить candidate-position из final decision path и
   оставить старые значения только как diagnostics/provenance в checkpoint.
   Тогда отчёт честно пишет:

   ```text
   candidate_position_final_authority=false
   candidate_position_role=diagnostics_only
   semantic_quality_claimed=false
   ```

2. **Лучший repair первого допустимого уровня:** сделать отдельный
   `ILogicalUnit`:

   ```text
   CandidateOrderLeakageGuardUnit
     input: SkillSelectionRequest + candidate permutations
     output: CandidateOrderLeakageReport
   ```

   Этот unit не выбирает skills. Он учится и проверяет, не зависит ли выбор от
   порядка кандидатов.

#### Алгоритм нового `CandidateOrderLeakageGuardUnit`

1. Взять `SkillSelectionRequest`:

   ```text
   prompt="нужно проверить план на архитектурные дыры"
   AvailableSkillIds=[
     "plan-review",
     "code-review",
     "documentation-sync"
   ]
   ```

2. Построить минимум две перестановки того же списка:

   ```text
   A=[plan-review, code-review, documentation-sync]
   B=[documentation-sync, plan-review, code-review]
   ```

3. Запустить основной semantic/context selector на A и B.

4. Сравнить результат:

   ```text
   A -> plan-review
   B -> plan-review
   ```

   Это хорошо: выбор стабилен, значит порядок не управляет решением.

5. Если результат меняется только из-за порядка:

   ```text
   A -> plan-review
   B -> documentation-sync
   ```

   unit возвращает:

   ```text
   order_sensitive=true
   must_suppress_position_features=true
   unsafe_features=["candidate_position"]
   ```

6. `SkillSelectionMachine` в таком случае обязан пересчитать решение без
   candidate-position features или вернуть честный degraded result, но не
   выдавать позиционный выбор как semantic answer.

#### Как это должно выглядеть кодом на уровне архитектуры

Не как новый “scorer pattern”, а как разрешённая единица:

```csharp
public sealed class CandidateOrderLeakageGuardUnit
    : ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport>
```

И основной путь должен быть таким:

```text
SkillSelectionMachine
  -> PromptContextSkillSelectionUnit выбирает по смысловым/context signals
  -> CandidateOrderLeakageGuardUnit проверяет перестановки кандидатов
  -> если order_sensitive=false, решение можно вернуть
  -> если order_sensitive=true, позиционные признаки блокируются и решение пересчитывается
```

То есть candidate-position больше не “учитель выбора skill”. Он становится
guard/debug сигналом, который защищает модель от неправильного обучения.

#### Какие сеты задеты и как чинить

| Set / asset | Как сейчас может ломать обучение | Как чинить |
|---|---|---|
| `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` | Может хранить `candidate_position_weights` рядом с learned state, и это выглядит как обученная модель. | Перенести candidate-position в diagnostics/provenance section или удалить из importable final-authority state. Manifest должен писать `candidate_position_role=diagnostics_only`. |
| `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl` | Если chosen skill всегда стоит на одной позиции, row учит порядок, а не смысл. | Добавить permutation audit. Row train-ready только если chosen skill остаётся тем же при 2-3 перестановках кандидатов. |
| `training-data/smart-skills/semantic-quality/skill-selection-training-v1/skill-selection-q5-04r-semantic-training.jsonl` | Нельзя повышать в train-ready, если rejected/chosen различаются candidate order или source position. | Для каждой row создать проверку: `candidate_order_invariant=true`. Если false -> `QUARANTINE_DO_NOT_TRAIN` или переписать row с stable candidate permutations. |
| `training-data/smart-skills/semantic-quality/skill-selection-inventory-v1/skill-inventory.jsonl` | Inventory может задавать порядок skills как побочный сигнал. | Использовать только как reference source; при создании DPO shuffle order и проверять, что label не зависит от позиции. |
| `artifacts/metric-honesty/SS-*.json` | Уже честно фиксируют `candidate_position_final_authority=false`; это надо сохранить и расширить. | Добавить поля `candidate_order_permutation_checked`, `order_sensitive_case_count`, `position_features_blocked`. |

#### Какие тесты задеты и как чинить

| Test | Что должен проверять |
|---|---|
| `SkillSelectionMachine_IgnoresCandidatePositionForSemanticDecision` | Один prompt, один набор skills, разные порядки. SelectedSkillIds одинаковые. |
| `CandidateOrderLeakageGuardUnit_DetectsOrderSensitiveDecision` | Специальный synthetic baseline меняет выбор при перестановке; guard возвращает `order_sensitive=true`. |
| `SkillSelectionMachine_DoesNotExportCandidatePositionAsFinalAuthority` | Export/checkpoint/manifest не дают candidate-position секции claim final authority. |
| `SkillSelectionMachine_FreshLoadDoesNotUseCandidatePositionAuthority` | После import checkpoint позиционные веса не начинают выбирать skill. |
| `SkillSelectionDpoRows_RequireCandidateOrderInvariant` | DPO/material-quality gate отклоняет rows, где chosen меняется из-за порядка кандидатов. |

#### Было -> должно быть именно для этой строки

| Было | Должно быть |
|---|---|
| `Dictionary<int,int>` учит позицию кандидата и потенциально может влиять на выбор. | Позиция не участвует в semantic decision. Она либо diagnostics-only, либо вход в `CandidateOrderLeakageGuardUnit`. |
| “Часто правильный skill на позиции 0” может стать decision signal. | “Правильный skill стабилен при перестановке кандидатов” становится обязательным gate для train-ready row. |
| Repair звучит как “provenance-only”. | Repair описывает конкретный алгоритм: permutation check -> leakage report -> suppress position features -> semantic selector без candidate index. |

### Конкретная замена всех словарей SkillSelectionMachine

Вопрос “что должно быть вместо `Dictionary<string, Dictionary<string,int>>`” решается не одним новым словом “unit”.
Нужна одна настоящая обучаемая `ILogicalUnit` на каждый смысловой словарь. Каждый unit получает
типизированное trained state, свой источник обучения, свои DPO-пары, свою K-NN (K-Nearest Neighbours)
роль и свои тесты.

#### 1. `_candidatePositionWeights`

**Сейчас:**

```csharp
private readonly Dictionary<int, int> _candidatePositionWeights = new();
```

**Что это вычисляет:** позиция кандидата в списке -> сколько раз skill на этой позиции был chosen.

**Проблема:** позиция в списке не является смыслом задачи. Если это влияет на выбор, модель учит порядок
массива, а не то, какой skill нужен.

**Должно быть:**

```text
CandidateOrderLeakageGuardUnit
input: SkillSelectionRequest + candidate permutations
output: CandidateOrderLeakageReport
typed state: CandidateOrderLeakageGuardState
```

**Алгоритм:** unit строит 2-3 перестановки одного и того же списка skills, запускает semantic selector
на каждой перестановке и проверяет, изменился ли выбранный skill только из-за порядка. Если изменился,
unit возвращает `order_sensitive=true`, блокирует feature `candidate_position` и не даёт такому решению
быть semantic answer.

**K-NN роль:** только diagnostics/support. Можно хранить ближайшие order-sensitive случаи для объяснения,
но нельзя выбирать skill через nearest candidate position.

**DPO-пары:** один и тот же prompt + один и тот же набор skills в разных порядках. Chosen должен быть
стабилен. Если chosen меняется от перестановки, строка идёт в quarantine.

**Тесты:** permutation invariance; no-position-authority after fresh-load; export/import не сохраняет
candidate-position как final authority.

#### 2. `_promptUnderstandingSignals`

**Сейчас:**

```csharp
private readonly Dictionary<string, Dictionary<string, int>> _promptUnderstandingSignals = new(StringComparer.Ordinal);
```

**Что это вычисляет:** skill -> prompt-signal -> integer weight.

**Проблема:** это baseline-счётчик. Он может быть полезной памятью, но сам по себе не является
типизированной обучаемой единицей и легко превращается в keyword-to-skill lookup.

**Должно быть:**

```text
PromptUnderstandingSkillUnit
input: normalized user prompt + available skills
output: PromptUnderstandingSkillEvidence
typed state: PromptUnderstandingSkillUnitState
```

**Typed state вместо словаря:** `FeatureVocabulary`, `SkillVocabulary`, `WeightMatrix`, `BiasVector`,
`Calibration`, `KnnMemory`, `TrainingProvenance`. Внутри `WeightMatrix` может физически храниться
матрица/массив, но архитектурно это named trained state, а не свободный nested dictionary.

**Алгоритм обучения:** из DPO-пар prompt paraphrase -> chosen skill / rejected skill. Unit обновляет
веса признаков, которые отличают правильное понимание prompt от похожего неправильного понимания.

**K-NN методы:** `FindNearestPromptExamples(vector, k)` и `ComputeKnnSupport(...)`. K-NN объясняет,
какие похожие prompt-примеры поддерживают решение, но не должен быть exact prompt replay.

**Источники обучения:** реальные user prompts из `C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts`,
`sonnet-user-prompts`, skill descriptions из repo, approved prompt-to-task examples. В common часть можно
добавлять внешние MIT/Apache-2.0 источники с задачами/инструкциями/issue-примерами; private user prompts
идут в user-adaptive leg.

**DPO-пары:** chosen выбирает skill по смыслу prompt; rejected выбирает правдоподобный, но неверный skill
из-за поверхностного keyword match.

**Тесты:** paraphrase stability; no exact prompt/source id leakage; ablation по prompt-understanding unit;
fresh-load сохраняет решение без retrain-from-zero.

#### 3. `_taskUnderstandingSignals`

**Сейчас:**

```csharp
private readonly Dictionary<string, Dictionary<string, int>> _taskUnderstandingSignals = new(StringComparer.Ordinal);
```

**Что это вычисляет:** skill -> task-signal -> integer weight.

**Проблема:** task boundary подменяется подсчётом токенов. Это не показывает, что модель поняла тип задачи.

**Должно быть:**

```text
TaskUnderstandingSkillUnit
input: StructuredTaskContract + available skills
output: TaskUnderstandingSkillEvidence
typed state: TaskUnderstandingSkillUnitState
```

**Typed state:** `TaskFeatureVocabulary`, `TaskKindVocabulary`, `SkillVocabulary`, `TaskToSkillWeights`,
`Calibration`, `KnnMemory`, `TrainingProvenance`.

**Алгоритм обучения:** берёт structured task из PromptToTask и учит, какой skill нужен для task kind,
constraints, deliverables, risk flags. Обновление происходит от DPO-пар, где rejected ошибается именно
в границе задачи.

**K-NN методы:** `FindNearestTaskExamples(taskVector, k)`, `ComputeTaskSimilarity(...)`.

**Источники обучения:** approved orchestration plans, prompt-to-task DPO strategy, новые разборы из
`sonnet local - analyzes`, skill library. Внешние источники common допускаются только с MIT/Apache-2.0
license и после universalization, чтобы не учить проектные file paths/code execution recipes.

**DPO-пары:** same/similar prompt, но chosen правильно определяет задачу; rejected выбирает skill под
соседнюю задачу, например code-review вместо plan-review.

**Тесты:** near-miss task boundary; task paraphrase; no project path/source leakage; ablation.

#### 4. `_promptContextSignals`

**Сейчас:**

```csharp
private readonly Dictionary<string, Dictionary<string, int>> _promptContextSignals = new(StringComparer.Ordinal);
```

**Что это вычисляет:** skill -> prompt/context-signal -> integer weight; затем `TryDecideWithPromptContext`
суммирует веса.

**Проблема:** это learned count/linear baseline, а не доказательство semantic context selection.

**Должно быть:**

```text
PromptContextSkillSelectionUnit
input: prompt features + task/context features + available skills
output: PromptContextSkillEvidence
typed state: PromptContextSkillSelectionUnitState
```

**Typed state:** `PromptFeatureVocabulary`, `ContextFeatureVocabulary`, `SkillVocabulary`,
`PromptWeights`, `ContextWeights`, `BiasVector`, `Calibration`, `KnnMemory`, `TrainingProvenance`.

**Алгоритм обучения:** учит случаи, где один prompt без контекста неоднозначен, а context меняет правильный
skill. Например “проверь план” при context “архитектурная полнота” -> plan-review; при context “код уже
изменён” -> code-review + implementation-integrity.

**K-NN методы:** `FindNearestPromptContextExamples(promptVector, contextVector, k)` и
`ComputePromptContextSupport(...)`.

**DPO-пары:** same prompt with different context. Chosen меняется по контексту, rejected игнорирует контекст
и выбирает общий skill.

**Тесты:** context-switch меняет решение; prompt-only недостаточен; no candidate position; no source id;
fresh-load + continue-training.

#### 5. `_intermediateChecksSignals`

**Сейчас:**

```csharp
private readonly Dictionary<string, Dictionary<string, int>> _intermediateChecksSignals = new(StringComparer.Ordinal);
```

**Что это вычисляет:** skill -> signal from intermediate checks -> integer weight.

**Проблема:** review/check finding может требовать новый skill, но словарь не объясняет, какой именно
check привёл к какому решению и как это проверять.

**Должно быть:**

```text
IntermediateChecksSkillUnit
input: intermediate review/check findings + current task + available skills
output: IntermediateChecksEvidence
typed state: IntermediateChecksUnitState
```

**Typed state:** `CheckFeatureVocabulary`, `SkillVocabulary`, `CheckToSkillWeights`, `KnnMemory`,
`Calibration`, `TrainingProvenance`.

**Алгоритм обучения:** учит пары “найденный промежуточный gap -> какой дополнительный skill нужен”.
Например, если review нашёл documentation drift, chosen добавляет documentation-sync; rejected не добавляет.

**K-NN методы:** `FindNearestCheckExamples(checkVector, k)` и `ComputeCheckSupport(...)`.

**Источники обучения:** plan-review findings, code-review findings, dashboard QA findings, DPO quality reviews,
problem-with-authority examples. Private project/user cases идут в adaptive leg; common leg получает только
универсализированные rows.

**DPO-пары:** same task with intermediate finding present vs absent. Chosen включает skill, required by finding;
rejected пропускает его.

**Тесты:** intermediate finding selects required skill; no source id leakage; ablation; dashboard/API scenario
показывает, какой check добавил какой skill.

#### Общий lifecycle для всех пяти unit

Каждый replacement unit обязан иметь один и тот же жизненный цикл:

```text
1. Load user/cache checkpoint first.
2. If user state is absent, load repo/common checkpoint.
3. If both are absent, return honest untrained/scaffold status.
4. Export/import/backup/share user state without mixing it into common state.
5. Continue training from the loaded checkpoint, not from zero.
6. Save manifest/checkpoint/locator/report with semantic_quality_claimed=false until held-out + ablation pass.
```

Это значит: “создать `ILogicalUnit`” недостаточно. Unit должен уметь жить как модель: загрузиться, сохраниться,
перенестись, продолжить обучение и честно сказать, если он ещё baseline.

#### Implementation-ready план замены пяти полей `SkillSelectionMachine.cs:64-85`

Эта секция фиксирует не "направление", а конкретное решение. Исполнитель не
должен сам придумывать, чем заменить словари. Он должен реализовать или
спланировать ровно такие unit/state/test rows, либо явно вернуть архитектурный
review, если в коде найдено лучшее имя при той же структуре.

##### Было: один класс держит пять разных смыслов в raw dictionaries

```csharp
private readonly Dictionary<int, int> _candidatePositionWeights = new();
private readonly Dictionary<string, Dictionary<string, int>> _promptUnderstandingSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _taskUnderstandingSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _promptContextSignals = new(StringComparer.Ordinal);
private readonly Dictionary<string, Dictionary<string, int>> _intermediateChecksSignals = new(StringComparer.Ordinal);
```

`Dictionary` как структура хранения не запрещён внутри реализации. Проблема в
том, что сейчас эти словари выглядят как самостоятельные "learned units", хотя
у них нет отдельного typed state, input/output contract, lifecycle, DPO-gate и
тестов качества. Поэтому audit repair обязан разнести их по разрешённым
`ILogicalUnit`.

##### Должно быть: пять unit/state rows

| Старое поле | Что оно вычисляет сейчас | Новый разрешённый уровень | Новый class/state | Что меняется в коде | Какие сеты трогает | Какие тесты обязательны |
|---|---|---|---|---|---|---|
| `_candidatePositionWeights` | `candidate index -> count`, то есть "какая позиция часто была chosen". | `ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport>` | `CandidateOrderLeakageGuardUnit` + `CandidateOrderLeakageGuardState` | Убрать из final selection authority. Новый unit делает permutation check и возвращает guard report, а не selected skills. | `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl`, `training-data/smart-skills/semantic-quality/skill-selection-training-v1/*`, `models/skill-selection/*checkpoint*` | permutation invariance; no-position-authority after import; export/import marks position as diagnostics-only; DPO row has `candidate_order_invariant=true`. |
| `_promptUnderstandingSignals` | `skill -> prompt token/signal -> int`, то есть prompt-keyword support for skill. | `ILogicalUnit<PromptUnderstandingSkillInput, PromptUnderstandingSkillEvidence>` | `PromptUnderstandingSkillUnit` + `PromptUnderstandingSkillUnitState` | Заменить raw nested dictionary на typed state: vocabulary + weight matrix + bias + calibration + K-NN memory + provenance. Old `TryDecideWithPromptUnderstanding` becomes unit call/result merge. | user prompts, `sonnet-user-prompts`, skill descriptions, prompt-to-task examples, MIT/Apache-2.0 universal instruction/task examples after license gate | paraphrase stability; exact-prompt/source-id leakage; ablation; fresh-load; continue-training from checkpoint. |
| `_taskUnderstandingSignals` | `skill -> task signal -> int`, то есть task-boundary support for skill. | `ILogicalUnit<TaskUnderstandingSkillInput, TaskUnderstandingSkillEvidence>` | `TaskUnderstandingSkillUnit` + `TaskUnderstandingSkillUnitState` | Structured task contract becomes input. State stores task kind vocabulary + task-to-skill weights. Old `TryDecideWithTaskUnderstanding` becomes unit call/result merge. | approved orchestration plans, prompt-to-task DPO, `sonnet local - analyzes`, skill library, universalized external task decomposition material | near-miss task boundary; paraphrase; no path/source leakage; ablation; dashboard/API example showing task-kind evidence. |
| `_promptContextSignals` | `skill -> prompt/context signal -> int`, then sums prompt score + context score. | `ILogicalUnit<PromptContextSkillInput, PromptContextSkillEvidence>` | `PromptContextSkillSelectionUnit` + `PromptContextSkillSelectionUnitState` | Split prompt weights and context weights in typed state. Old `TryDecideWithPromptContext` becomes unit call. Candidate index/source ids are forbidden inputs. | same prompt/different context DPO rows; plan-review/code-review/documentation context examples; user adaptive corrections | context-switch changes skill; prompt-only is insufficient; no candidate-position; no source-id; fresh-load; continue-training. |
| `_intermediateChecksSignals` | `skill -> intermediate finding/check signal -> int`, то есть check-finding support for skill. | `ILogicalUnit<IntermediateChecksSkillInput, IntermediateChecksEvidence>` | `IntermediateChecksSkillUnit` + `IntermediateChecksUnitState` | Intermediate review/check findings become typed input. Old private scorer becomes unit call that explains which check added which skill. | plan-review findings, code-review findings, dashboard QA findings, DPO-quality findings, problem-with-authority examples (adaptive unless universalized) | finding selects required skill; no source-id leakage; ablation; dashboard/API scenario shows finding -> added skill. |

##### Конкретная форма typed state вместо raw dictionary

Пример для `_promptContextSignals`; остальные четыре поля получают такой же
typed-state стиль, но со своими vocabulary и evidence names:

```csharp
public sealed record PromptContextSkillSelectionUnitState(
    string UnitId,
    string CheckpointHash,
    IReadOnlyList<string> PromptFeatureVocabulary,
    IReadOnlyList<string> ContextFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    float[][] PromptWeights,
    float[][] ContextWeights,
    float[] BiasVector,
    PromptContextCalibration Calibration,
    IReadOnlyList<PromptContextKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);
```

Почему это лучше: executor видит не "какой-то Dictionary", а модельный state с
явным смыслом каждого массива. Внутри `PromptWeights` физически может быть
матрица чисел, но архитектурно это state конкретного `ILogicalUnit`, а не новый
неразрешённый "dictionary scorer pattern".

##### Конкретная форма unit call вместо private `TryDecideWith*`

Сейчас runtime chain выглядит как набор приватных методов:

```csharp
var promptContextDecision = TryDecideWithPromptContext(request);
var promptUnderstandingDecision = TryDecideWithPromptUnderstanding(request);
...
```

Должно стать chain of units внутри `SkillSelectionMachine`:

```text
SkillSelectionMachine.RunSelectionAsync(request)
  -> PromptUnderstandingSkillUnit.ExecuteAsync(prompt input)
  -> TaskUnderstandingSkillUnit.ExecuteAsync(task input)
  -> PromptContextSkillSelectionUnit.ExecuteAsync(prompt + context input)
  -> IntermediateChecksSkillUnit.ExecuteAsync(check findings input)
  -> CandidateOrderLeakageGuardUnit.ExecuteAsync(permutation input)
  -> SkillSelectionEvidenceArbiter merges evidence and rejects leaky results
```

`SkillSelectionEvidenceArbiter` не является новым trainable pattern. Это
детерминированный safety/merge слой внутри `SkillSelectionMachine`: он только
проверяет, что unit outputs не используют запрещённые сигналы, и собирает
итоговый ответ. Он не учится и не заявляет semantic quality.

##### Как каждый unit обучается

| Unit | Training update rule | DPO chosen | DPO rejected | Запрещённые сигналы |
|---|---|---|---|---|
| `CandidateOrderLeakageGuardUnit` | сохраняет случаи, где permutations меняют/не меняют output; учит leakage detector. | same selected skill across permutations. | selected skill changes only because candidate order changed. | candidate index as positive skill evidence. |
| `PromptUnderstandingSkillUnit` | увеличивает веса prompt features, которые отличают chosen skill from rejected skill; калибрует confidence. | skill выбран по смыслу prompt. | plausible skill выбран по keyword-only match. | exact prompt id, source file, candidate order. |
| `TaskUnderstandingSkillUnit` | обновляет task-kind -> skill weights из structured task. | skill соответствует типу задачи и deliverable. | skill соответствует соседней, но неверной задаче. | file path/project-specific execution recipe in common model. |
| `PromptContextSkillSelectionUnit` | отдельно обновляет prompt and context weights; context может менять skill only when justified. | same prompt + context выбирает правильный skill. | ignores context or overfits prompt-only. | source id, pair id, candidate index. |
| `IntermediateChecksSkillUnit` | учит mapping from review/check finding to required extra skill. | finding добавляет нужный skill. | finding игнорируется или добавляет unrelated skill. | exact review file path, reviewer name, source row id. |

##### K-NN (K-Nearest Neighbours) role per unit

K-NN здесь разрешён как memory/support component внутри unit, не как вся модель:

| Unit | K-NN method | Роль |
|---|---|---|
| `CandidateOrderLeakageGuardUnit` | `FindNearestOrderSensitiveCase(vector, k)` | объяснить похожие leakage cases; не выбирать skill. |
| `PromptUnderstandingSkillUnit` | `FindNearestPromptExamples(promptVector, k)` | дать support examples для prompt meaning; exact prompt replay запрещён. |
| `TaskUnderstandingSkillUnit` | `FindNearestTaskExamples(taskVector, k)` | показать похожие task-boundary examples; не заменяет task classifier. |
| `PromptContextSkillSelectionUnit` | `FindNearestPromptContextExamples(promptVector, contextVector, k)` | поддержать context-sensitive decision. |
| `IntermediateChecksSkillUnit` | `FindNearestCheckExamples(checkVector, k)` | показать похожие review/check findings. |

##### Common vs user-adaptive training

Common leg в `llm_mvp_core`:

- universal skill descriptions;
- universalized prompt/task examples;
- universalized orchestration analyses;
- MIT/Apache-2.0 external planning/task/issue examples after license gate;
- no private user chat, no project-specific execution recipes, no exact local
  paths as learning signals.

User-adaptive leg on user machine:

- Luba/user prompts and AI corrections;
- local project-specific examples;
- private chats;
- result-gap rows;
- "AI answered, user corrected" DPO rows;
- backups/share/import/export are allowed as user-owned assets, not silently
  merged into common.

##### External source intake для common DPO

Внешний материал нельзя класть в common training "потому что он похож".
Каждый source family получает отдельную строку intake:

```text
source_name:
source_url_or_local_archive:
license_file_ref:
license_allowed: MIT | Apache-2.0 only unless Luba approves otherwise
allowed_fields:
universal_skill_taught:
project_specific_fields_removed:
prompt_to_task_rows:
orchestration_reasoning_rows:
skill_selection_rows:
held_out_split_rule:
reviewer:
```

Хорошие внешние источники должны быть не абстрактной категорией, а candidate
source row с проверкой license file. Ни один пример ниже не считается
pre-approved: перед common training нужно записать exact URL/local archive,
license file/ref, allowed fields, и что именно этот источник учит.

| Candidate source example | Exact URL / archive candidate | License evidence before use | Почему подходит | Как учиться |
|---|---|---|---|---|
| Kubernetes enhancement proposal examples | `https://github.com/kubernetes/enhancements` or local archive mirror of that repo | Verify upstream `LICENSE` is Apache-2.0-compatible and save `license_file_ref=https://github.com/kubernetes/enhancements/blob/master/LICENSE`; if unavailable -> evidence-only. | Учит формулировать problem statement, goals, non-goals, risks, graduation criteria. | Для `PromptUnderstandingSkillUnit` и `TaskUnderstandingSkillUnit`: chosen = universal task boundary; rejected = path/command-specific or wrong-scope decomposition. Удалять имена файлов/команд проекта. |
| Apache Airflow docs/tutorials / operator how-tos | `https://github.com/apache/airflow/tree/main/docs` or local archive mirror | Verify upstream `LICENSE` is Apache-2.0 and save `license_file_ref=https://github.com/apache/airflow/blob/main/LICENSE`; if docs license differs or unclear -> evidence-only. | Учит orchestration order: gather evidence -> choose operation -> verify -> document. | Для `TaskUnderstandingSkillUnit` и OrchestrationReasoning: chosen = правильный порядок шагов; rejected = skipped evidence/gate. Исключать concrete shell/code recipes from universal common. |
| Apache Beam programming guide / patterns | `https://github.com/apache/beam/tree/master/website/www/site/content/en/documentation` or local archive mirror | Verify upstream `LICENSE` is Apache-2.0 and save `license_file_ref=https://github.com/apache/beam/blob/master/LICENSE`; if selected docs have separate terms -> evidence-only. | Учит pipeline thinking, input/output boundaries, transforms, validation. | Для OrchestrationReasoning: chosen = correct data-flow decomposition; rejected = mixed responsibilities or missing verification. |
| VS Code extension samples | `https://github.com/microsoft/vscode-extension-samples` or local archive mirror | Verify upstream `LICENSE` is MIT and save `license_file_ref=https://github.com/microsoft/vscode-extension-samples/blob/main/LICENSE`; if absent -> evidence-only. | Учит host-extension boundary: thin client command/panel -> API request -> response/debug. | Для SkillSelection/PromptToTask: chosen = host-boundary skill or API-test skill; rejected = core logic moved into extension. |

Плохие источники:

- unknown license;
- GPL/AGPL unless Luba separately approves legal/product path;
- private chats or project files for common model;
- exact code execution recipes tied to one repository;
- examples where answer can be guessed from filename/path/source id/order.

Это правило касается и skills: skill library rows can train common only after
universalization. Project-specific code execution details stay adaptive-only or
evidence-only until the local wiki/model layer is ready to learn technical
execution deliberately.

##### Acceptance gate for this audit row

This audit section is complete only if `rg` finds all five replacement units in
this exact file:

```powershell
rg -n "_candidatePositionWeights|PromptUnderstandingSkillUnit|TaskUnderstandingSkillUnit|PromptContextSkillSelectionUnit|IntermediateChecksSkillUnit|CandidateOrderLeakageGuardUnit|CandidateOrderLeakageGuardState|PromptUnderstandingSkillUnitState|TaskUnderstandingSkillUnitState|PromptContextSkillSelectionUnitState|IntermediateChecksUnitState|FindNearestPromptExamples|FindNearestTaskExamples|FindNearestPromptContextExamples|FindNearestCheckExamples|candidate_order_invariant" docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md
```

And review must answer in human language:

```text
Было: raw dictionary/counter state controls or supports selection.
Должно быть: each semantic capability has one named ILogicalUnit with typed state,
training update rule, K-NN support role, DPO pair rule, lifecycle, affected sets,
and tests. Candidate position is guard/diagnostics only, never final semantic
authority.
```

#### Внешние источники для common обучения

Для common модели можно использовать только источники с понятной лицензией. Предпочтительные лицензии:
MIT и Apache-2.0. Подходящие семейства источников:

- permissive task decomposition / workflow examples;
- permissive issue and pull-request discussions where license allows reuse;
- permissive documentation examples and troubleshooting guides;
- permissive instruction-following / planning traces after filtering to universal behavior.

Что нельзя класть в common без отдельного решения:

- unknown-license data;
- GPL-family data;
- private user chats;
- локальные project-specific execution recipes;
- строки, где правильный ответ можно угадать по filename/path/source id/candidate order.

Prompt-context scorer считает сумму весов сигналов:

```csharp
PromptScore = _promptContextSignals.TryGetValue(skillId, out var profile)
    ? requestSignals.Where(IsPromptContextPromptSignal).Sum(signal => profile.GetValueOrDefault(signal))
    : 0,
ContextScore = _promptContextSignals.TryGetValue(skillId, out var contextProfile)
    ? requestSignals.Where(IsPromptContextContextSignal).Sum(signal => contextProfile.GetValueOrDefault(signal))
    : 0
```

Участок: `43.SmartSkillsDomain/SkillSelectionMachine.cs:1073-1089`.

### Что это значит

Сейчас это не “полноценная semantic skill-selection model”.

Это разрешённая оболочка `ILearningMachine`, внутри которой есть несколько
learned counter/scorer baselines. Они могут быть полезны, но их нельзя называть
самостоятельными обучаемыми design patterns.

### Что в коде сделано правильно

Код сам не даёт импортировать checkpoint/manifest, который заявляет semantic
quality:

```csharp
if (checkpoint.PromptUnderstanding.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("Prompt understanding unit must not claim full semantic quality.", ...);
...
if (manifest.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("SkillSelection persistence import must not claim full semantic quality.", ...);
```

Участок: `43.SmartSkillsDomain/SkillSelectionMachine.cs:1751-1823`.

Это хорошая защита от ложного “готово”.

### Было -> должно быть

| Было сейчас | Должно быть перед сильным claim |
|---|---|
| Есть `ILearningMachine`, но внутренние unit-like sections реализованы как словари, счётчики и `TryDecideWith*`. | Либо честно писать “baseline / learned counter model”, либо оформить каждую реально заявленную semantic capability как настоящий `ILogicalUnit` / `ILogicalFlow` / `ILearningMachine` state. |
| `_candidatePositionWeights` хранится в checkpoint. | Это должно быть scaffold/provenance или fallback only. Оно не должно определять semantic selection. |
| `source_pair_ids` хранятся рядом с профилями. | Это provenance. Они не должны быть сигналом выбора. |
| PromptContext выбирает skill через суммы integer weights. | Это допустимый learned linear/count baseline, но не доказательство semantic skill selection. |
| Reports могут выглядеть как “skill selection trained”. | Reports должны явно говорить: `semantic_quality_claimed=false`, route/baseline type, gates not yet passed. |

### Какие сеты и модели затронуты

| Asset / set | Как влияет |
|---|---|
| `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` | Содержит `candidate_position_weights` и `source_pair_ids`. Его нельзя считать semantic-quality моделью. |
| `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl` | Надо проверить, нет ли строк, где правильный skill фактически задан порядком кандидатов. |
| `training-data/smart-skills/semantic-quality/skill-selection-training-v1/skill-selection-q5-04r-semantic-training.jsonl` | Сейчас строки помечены `training_eligible=false` / `excluded_from_training_payload=true`; их нельзя тренировать напрямую. |
| `training-data/smart-skills/semantic-quality/skill-selection-inventory-v1/skill-inventory.jsonl` | Хорош как inventory/material source, но не DPO train-ready сам по себе. |
| `artifacts/metric-honesty/SS-*.json` | Эти артефакты важны, потому что они уже фиксируют `candidate_position_final_authority=false` и `semantic_quality_claimed=false`; их надо сохранить как honesty evidence. |

### Какие тесты нужны

- Перестановка кандидатов: тот же prompt, тот же набор skills, но другой
  порядок. Выбор не должен зависеть от позиции.
- Source/pair leakage: `source_pair_ids`, `pair_id`, `source_row_id` не должны
  менять выбор.
- Exact prompt memory: near-miss prompt должен проверять смысл, а не точную
  строку.
- Ablation: если убрать конкретный learned section, качество по его сценарию
  должно упасть.
- Export/import/fresh-load: после загрузки checkpoint поведение и честные flags
  должны сохраняться.

## 2. PromptToTaskMachine

Файл: `43.SmartSkillsDomain/PromptToTaskMachine.cs`

### Что видно в коде

Модель выбирает learned pattern:

```csharp
var selectedPattern = SelectBestPattern(request, learnedState);
...
var matchedSignalCount = CountMatchingSignals(request, selectedPattern);
```

Участок: `43.SmartSkillsDomain/PromptToTaskMachine.cs:120-155`.

Сам выбор устроен как подсчёт совпавших сигналов:

```csharp
private static PromptToTaskLearnedPattern? SelectBestPattern(...)
...
var score = CountMatchingSignals(request, pattern);
```

Участок: `43.SmartSkillsDomain/PromptToTaskMachine.cs:419-428`.

Checkpoint/manifest не имеют права заявлять semantic quality:

```csharp
if (checkpoint.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("Checkpoint must not claim semantic quality in PTT-04.", ...);
```

Участок: `43.SmartSkillsDomain/PromptToTaskMachine.cs:526-532`.

```csharp
if (manifest.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("Manifest must not claim semantic quality in PTT-04.", ...);
```

Участок: `43.SmartSkillsDomain/PromptToTaskMachine.cs:606-612`.

### Что это значит

PromptToTask сейчас умеет брать prompt и подбирать learned pattern по сигналам.
Это лучше, чем hardcoded exact prompt lookup, но это всё ещё pattern/signal
baseline.

Это не доказывает, что модель действительно “понимает prompt пользователя” на
новых формулировках.

### Было -> должно быть

| Было сейчас | Должно быть перед сильным claim |
|---|---|
| PromptToTask выбирает pattern по пересечению сигналов. | Нужно доказать на held-out prompts, что выбирается универсальная задача, а не похожий source row. |
| В checkpoint есть learned patterns. | Нужны train-ready DPO rows, split train/held-out, leakage checks. |
| `semantic_quality_claimed=false` стоит честно. | Держать false, пока нет held-out + ablation + fresh-load + continue-training. |
| Технические refs исключаются в DPO material. | Это надо продолжать: common PromptToTask должен учить универсальное “prompt -> task”, а не repo-specific execution. |

### Какие сеты и модели затронуты

| Asset / set | Как влияет |
|---|---|
| `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json` | Это baseline learned-pattern asset, не final semantic model. |
| `training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-dpo.jsonl` | Сейчас rows имеют `training_allowed=false`; это материал для подготовки train-ready DPO, не готовый train set. |
| `training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed.jsonl` | Может быть source для примеров, но нужно проверить, что в train payload не попадают пути, названия файлов, exact prompt ids. |
| `artifacts/dpo-quality/*` и `artifacts/dpo-material-quality/*` | Должны стать gate evidence: какие rows можно переводить из materialized в train-ready. |

### Какие тесты нужны

- Prompt paraphrase test: та же задача, другая формулировка.
- Negative near-miss: похожий prompt, но другая граница задачи.
- No path/title leakage: путь файла или название документа не должны быть
  ключом решения.
- DPO quality gate: chosen/rejected должны отличаться смыслом задачи, а не
  технической деталью.

## 3. OrchestrationReasoningMachine

Файл: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs`

### Что видно в коде

Модель выбирает learned guide pattern:

```csharp
var chosenPattern = ChooseBestPattern(request, learnedGuideState);
...
var matchedSignalCount = CountMatchingSignals(request, chosenPattern);
```

Участок: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:139-158`.

Pattern проходит через required signals:

```csharp
if (!SatisfiesRequiredSignals(request, pattern))
    continue;

var score = CountMatchingSignals(request, pattern);
```

Участок: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:469-474`.

Checkpoint/manifest тоже не имеют права заявлять semantic quality:

```csharp
if (checkpoint.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("Checkpoint must not claim semantic quality in OR-04A.", ...);
```

Участок: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:608-614`.

```csharp
if (manifest.SemanticQualityClaimed)
    return OperationResult<bool>.Fail("Manifest must not claim semantic quality in OR-04A.", ...);
```

Участок: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:693-699`.

### Что это значит

OrchestrationReasoning сейчас не “думает как архитектор” в полном смысле. Он
выбирает подготовленный reasoning pattern по required signals. Это может быть
хорошим первым общим слоем, особенно если DPO сделан качественно, но это пока
не доказанная reasoning model.

### Было -> должно быть

| Было сейчас | Должно быть перед сильным claim |
|---|---|
| Выбор сценария через `ChooseBestPattern` и `CountMatchingSignals`. | Нужно доказать, что модель выбирает правильный orchestration plan на новых анализах, а не на знакомых refs. |
| Learned patterns лежат в checkpoint. | Нужны train-ready DPO rows + held-out rows из новых `sonnet local - analyzes`. |
| Material rows часто имеют `training_allowed=false`. | Нельзя тренировать из них напрямую. Сначала quality gate -> train-ready copy. |
| В отчётах может звучать “orchestration reasoning”. | Пока писать “universal orchestration baseline / learned pattern selection”, если gates не пройдены. |

### Какие сеты и модели затронуты

| Asset / set | Как влияет |
|---|---|
| `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json` | Baseline learned-pattern model; держать `semantic_quality_claimed=false`. |
| `training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl` | Materialized DPO material, но `training_allowed=false`; нужен train-ready promotion. |
| `training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl` | Binding/source rows есть, но `dpo_train_ready=false`; это registry, не разрешение тренировать. |
| `training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl` | Очень важный held-out слой: должен проверять новые universal scenarios без source leakage. |
| `C:/Users/lubao/Documents/xiigen/sonnet local - analyzes/xiigen-file-analysis-state.json` | Источник новых анализов; нужен повторяемый importer, который превращает только universal reasoning часть в DPO material. |

### Какие тесты нужны

- Source-disjoint held-out: анализы, которых не было в train split.
- Near-miss orchestration: похожий контекст, но другая правильная стратегия.
- No source-ref route: `source_row_id`, filename, title, path, line ref не
  должны быть ключом ответа.
- DPO quality: rejected должен быть правдоподобным, но ошибочным по одной
  конкретной причине.
- Dashboard/API scenario: показать, что API возвращает честный статус baseline
  и не пишет `semantic_quality_claimed=true`.

## 4. ProjectWiki KNN / PromptFlowGrounding

Файлы:

- `42.ProjectWikiDomain/OrchestrationKnnStore.cs`
- `42.ProjectWikiDomain/PromptFlowGroundingModel.cs`
- `42.ProjectWikiDomain/OrchestrationPlanMachine.cs`

### Что видно в коде

KNN store хранит training pairs и ищет ближайший pair по Jaccard:

```csharp
public OrchestrationTrainingPair? FindNearest(string prompt, PairMode? mode = null)
    => FindNearestWithScore(prompt, mode)?.Pair;
```

```csharp
Jaccard(promptTokens, Tokenize(pair.Prompt).ToHashSet(StringComparer.OrdinalIgnoreCase))
```

Участок: `42.ProjectWikiDomain/OrchestrationKnnStore.cs:29-42`.

В `PromptFlowGroundingModel` есть не только KNN, но и trained weights:

```csharp
private readonly OrchestrationKnnStore _knn = new();
private readonly Dictionary<string, OrchestrationFlowGroundingWeights> _weights = new(StringComparer.Ordinal);
```

```csharp
_knn.Add(pair);
_weights[pair.FlowRef] = BuildWeights(pair);
```

Участок: `42.ProjectWikiDomain/PromptFlowGroundingModel.cs:5-18`.

Runtime сначала считает weighted score:

```csharp
var best = _weights
    .Select(pair => new
    {
        FlowRef = pair.Key,
        Score = ScoreTokens(tokens, pair.Value.FlowRefWeights),
        Weights = pair.Value
    })
```

Участок: `42.ProjectWikiDomain/PromptFlowGroundingModel.cs:28-37`.

Потом использует KNN как дополнительный nearest evidence:

```csharp
var nearest = _knn.FindNearestWithScore(queryText, PairMode.Mode1LearnedFlowGrounding);
```

Участок: `42.ProjectWikiDomain/PromptFlowGroundingModel.cs:42`.

`OrchestrationPlanMachine` использует grounding и потом nearest learned pair:

```csharp
var grounding = _promptFlowGroundingModel.TryGround(prompt, contract);
...
var learned = _promptFlowGroundingModel.FindLearnedPair(prompt, contract, grounding);
```

Участок: `42.ProjectWikiDomain/OrchestrationPlanMachine.cs:86-94`.

Mode 2 fallback прямо использует nearest plan:

```csharp
var nearestPlan = _mode2Knn.FindNearest(prompt, PairMode.Mode2Generalization);
var planText = _routeComposer.RenderMode2Plan(nearestPlan, contract, prompt);
```

Участок: `42.ProjectWikiDomain/OrchestrationPlanMachine.cs:109-110`.

### Что это значит

KNN здесь не надо считать ошибкой автоматически.

В `PromptFlowGroundingModel` он выглядит как часть архитектуры модели:

- есть trained pairs;
- есть `_weights`;
- есть weighted scoring;
- есть KNN nearest evidence.

То есть правильный вопрос не “почему тут KNN?”, а:

> KNN является честно описанным component/memory layer внутри learned grounding
> model, или отчёт говорит так, будто один nearest-neighbor lookup сам является
> всей обученной моделью?

### Было -> должно быть

| Было сейчас | Должно быть |
|---|---|
| Audit раньше звучал так, будто KNN сам по себе подозрителен. | KNN должен быть audit-triggered, not forbidden. |
| Mode 1 использует weights + KNN. | Документация должна сказать: KNN является частью learned grounding architecture, если final decision опирается на trained weights/state и проходит ablation. |
| Mode 2 использует KNN fallback. | Документация должна честно назвать это fallback/baseline, если Mode 2 не заявлен как full learned semantic plan. |
| Test должен “disable KNN”. | Тест должен проверять claim scope: KNN component можно оставить; запрещено только выдавать fallback KNN за всю обученную модель. |

### Какие сеты и модели затронуты

| Asset / set | Как влияет |
|---|---|
| `training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl` | Если rows используются для KNN/weights, нужно разделить train payload и provenance refs. |
| `training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl` | Должен проверять, что nearest evidence не просто узнаёт source row. |
| `models/orchestration-reasoning/orchestration-reasoning-v1/*` | Если модель заявляет learned grounding/reasoning, manifest/report должен описывать роль KNN: component, fallback или baseline. |
| `docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` и state/evidence | Если там KNN был принят как forbidden, wording надо уточнить: forbidden только unlabelled final shortcut. |

### Какие тесты нужны

- KNN role test: report явно пишет `knn_role=model_component` или
  `knn_role=fallback_baseline`, а не просто “learned”.
- Weighted-state ablation: если убрать trained weights, Mode 1 должен потерять
  качество или отказаться от claim.
- Source-disjoint nearest test: nearest pair не должен быть exact/source-id
  memory.
- Fallback honesty test: Mode 2 KNN fallback не должен включать
  `semantic_quality_claimed=true`.

## 5. Сводная таблица affected sets

| Сет / модель | Текущий статус | Что делать |
|---|---|---|
| `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` | Baseline checkpoint; есть candidate-position и source-pair provenance; semantic quality false. | Не выбрасывать. Не считать final semantic model. Проверить candidate-order/source-id leakage. |
| `training-data/smart-skills/semantic-quality/skill-selection-training-v1/skill-selection-q5-04r-semantic-training.jsonl` | Material/held-out-like rows; многие rows явно не train eligible. | Не тренировать напрямую. Делать train-ready copy после DPO quality gate. |
| `training-data/smart-skills/semantic-quality/skill-selection-inventory-v1/skill-inventory.jsonl` | Inventory material from skills. | Использовать как source/reference, не как готовый DPO. |
| `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json` | Learned-pattern baseline. | Держать false claim, пока нет held-out/ablation/continue-training. |
| `training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-dpo.jsonl` | Universalized material, но `training_allowed=false`. | Перевести часть rows в train-ready только через quality/leakage review. |
| `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json` | Learned-pattern baseline. | Не заявлять full reasoning quality. |
| `training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl` | Хорошее universal material, но не training approval. | Сделать повторяемый pipeline из новых `sonnet local - analyzes`. |
| `training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl` | Binding/source registry; `dpo_train_ready=false`. | Использовать как registry; train only после gate. |
| `training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl` | Held-out fixtures. | Расширять новыми анализами и near-miss cases. |

## 6. Что исправлять не надо

Не надо удалять все словари, KNN и pattern matching.

Они могут остаться, если:

- находятся внутри разрешённого уровня;
- честно названы baseline/component/internal detail;
- не становятся hidden final authority;
- не учатся на source ids, pair ids, candidate order, filenames;
- не дают `semantic_quality_claimed=true` без gates.

## 7. Что исправлять надо

1. Отчёты и документация должны перестать звучать так, будто baseline уже
   является semantic model.
2. Сеты с `training_allowed=false` или `training_eligible=false` нельзя подавать
   в обучение напрямую.
3. Candidate-position и source-pair fields должны быть provenance-only.
4. KNN должен быть описан по роли:
   - model component;
   - memory/retrieval layer;
   - candidate retriever;
   - fallback baseline.
5. Для каждой роли нужен свой test:
   - component quality;
   - fallback honesty;
   - source-disjoint behavior;
   - no exact prompt/source id leakage.

## 8. Итог

Начинать с нуля не нужно.

Но текущие модели надо считать не финальными semantic models, а смесью:

- разрешённых архитектурных оболочек;
- baseline/internal mechanisms;
- KNN/model components;
- quarantined DPO material;
- честных `semantic_quality_claimed=false` assets.

Правильный следующий repair не “всё удалить”, а:

1. Честно разметить роли механизмов.
2. Перестроить только те train sets, где обучающий сигнал загрязнён
   candidate order/source ids/technical execution refs.
3. Добавить tests, которые не дадут baseline пройти как semantic model.
4. Только после этого повышать claim модели.
