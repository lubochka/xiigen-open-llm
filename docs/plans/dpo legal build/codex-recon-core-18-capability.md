# РЕКОН: 18.CapabilityTraining — тренировка способностей (пайплайн, данные, метрики)

Подзадача рекона: «Тренировка способностей: пайплайн, данные, метрики» для плана
ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph).
Источник правил классификации: `docs/skills/trainable-architecture-decision-SKILL.md`.
Это сбор evidence для плана, не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Проект `C:\Projects\llm_mvp_core\18.CapabilityTraining` (assembly `CapabilityTraining`,
namespace `AgentInfrastructure.Logical`). Это уже существующий конвейер
«наблюдённое ребро знаний (knowledge edge) -> DPO-обучение крошечной модели ->
регистрация пробационного (probationary) обучаемого юнита и retry-flow».

Исходники (7 .cs):
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\DefaultLogicalCapabilityTrainer.cs` (1019 строк) — главный оркестратор обучения; класс `DefaultLogicalCapabilityTrainer : ILogicalCapabilityTrainer`, метод `TrainAndRegisterAsync`. Содержит также `GeneratedCapabilityFlowFactory : ILogicalFlowFactory<string>` и `GeneratedCapabilityFlow : ILogicalFlow<string>`.
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\LlmIngredientLogicalUnits.cs` (408) — реальные юниты: `TransformerEmbeddingKnnLogicalUnit<TOutput> : ILogicalUnit<string,TOutput>` (инференс через embedding+HNSW) и `LogicalDpoTrainingUnit : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>` (обучающий юнит DPO).
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\TeachingRegistrySnapshot.cs` (88) — durable-контракты: `GeneratedCapabilityCheckpointManifest`, `TeachingRegistrySnapshot`, `TeachingRegistrySnapshotKinds`.
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\TeachingRegistryPersistence.cs` (333) — capture/restore реестра, save/load манифестов, имя `generated_capability_manifest.json`.
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\ReplayFusionRanker.cs` (74) — ранжирование replay-манифестов BM25+overlap fusion (E34V2).
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\LogicalExperimentOptimizations.cs` (254) — idle-gate D-14 (`ILogicalTeachingIdleGate`, `AutonomousLearningIdleGate` через reflection в проект 16) и (по ссылкам из trainer) `LogicalTeachingDatasetWeighter`, `AutonomousLearningReplayMixer`, `AutonomousLearningTrustPromotionScorer`.
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\CapabilityResultTranslator.cs` (30) — перевод KNN-вывода в человекочитаемый текст.

Зависимости (csproj): 00.Core, 00.TensorCore, 01.Tokenizer, 03.TransformerModel,
05.Embeddings, 06.VectorSearch, 10.LoRA, 11.DPO, 12.SemanticCompiler,
21.AgentInfrastructure, 23.SemanticCompilerAdapter. Внешних ML-фреймворков нет
(Rule 10 соблюдён). Проекта НЕТ ссылок на 15.OrchestratorModel и 25.ModelServer
(см. Пробелы — манифест содержит `ServerBaseUrl`/`ServerUnitEndpoint`, но публикации
в ModelServer здесь нет).

---

## 2. Как устроено / работает (кратко)

ПАЙПЛАЙН (`TrainAndRegisterAsync`, файл DefaultLogicalCapabilityTrainer.cs):
1. Валидация request (`KnowledgeEdgeBranches`, `Topology/Flows/Units`). Если флаг
   `UseIdleLearningScheduling` — спросить idle-gate (D-14); если не idle — вернуть
   `LogicalCapabilityTrainingResult` с пометкой «Teaching deferred by D-14».
2. Для каждой ветки: извлечь sub-phase (`ExtractSubPhaseId`: ключи `missing_sub_phase`/
   `routed_sub_phase_id`/`sub_phase_id` из `branch.Metrics`), проверить в топологии,
   собрать `TrainingBranchPlan` с A-стороной (`BuildTrainingTextCore`: compiled prompt +
   branch result + sub-phase + предикаты + 60-мерный teaching-вектор), B-chosen
   (`BuildChosenCapabilityText`) и B-rejected («Do not generate a capability…»).
3. Один батч DPO на весь same-domain ряд: `LogicalTeachingDatasetWeighter.ComputeCurrentRepeatCount`
   (D-09 веса по rare-классам), `AutonomousLearningReplayMixer.BuildPreferences`
   (текущие пары + replay-манифесты × repeat), затем `ExecuteDpoTeachingAsync` ->
   `LogicalDpoTrainingUnit.ExecuteAsync`.
4. `LogicalDpoTrainingUnit` (LlmIngredientLogicalUnits.cs): применяет LoRA
   (`LoraApplicator.Apply`, rank=2/alpha=4), при наличии — грузит родительский
   checkpoint (continue-training), кодирует пары токенайзером в `DpoBatch`/тензоры,
   крутит цикл `DpoTrainer.StepVector` до `MaxStepCount` (default 5) с ранней
   остановкой `ConvergenceMonitor` (порог 0.002, окно).
5. Регистрация: обучить `DomainClassifierUnit` (отдельный обучаемый юнит, persist в
   `domain-classifier-training.json`); затем на каждый sub-phase сохранить
   tokenizer.json + policy.bin, обернуть в `TransformerEmbeddingKnnLogicalUnit.Train`
   (embedding-модель + HNSW над примерами), сохранить манифест, зарегистрировать
   юнит (`Units.Register`, trust=Probationary, isLearned:true) и flow-фабрику
   (`Flows.RegisterFactory`).
6. Вернуть `LogicalCapabilityTrainingResult` (зарегистрированные flow/unit/sub-phase
   ids, evidenceIds = пути checkpoint+метрик, тайминги веток, нотки).

ИНФЕРЕНС (retry-путь): `GeneratedCapabilityFlowFactory.Create` -> если
`UseLearnedUnitWhenAvailable` и есть Probationary юнит -> `GeneratedCapabilityFlow`,
который зовёт `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync`: embed запроса ->
HNSW поиск -> fusion-скоринг (vector 0.45 + lexical/BM25 0.35 + pattern 0.20) ->
выбор примера -> confidence = 0.50 + fused*0.49.

ДАННЫЕ: вход — `LogicalFlowResult[]` ветки + compiled prompt; обучающий ряд — DPO
preference (Prompt/Chosen/Rejected + teaching-вектор). Персист: `generated_capability_manifest.json`
(гиперпараметры модели/LoRA, пути checkpoint, predicate/domain ids, родитель,
`TrainingStepCount`), `TeachingRegistrySnapshot` (дамп реестра), метрики DPO
`dpo_step_*.json` в checkpoint-каталоге. Checkpoint-root по умолчанию ВНЕ репозитория
(`Path.GetTempPath()/llm-mvp-capability-training`).

МЕТРИКИ: `LogicalDpoTrainingResult` (PreferenceCount, DpoStepCount, LoraAdapterCount,
LastLoss, LastAccuracy, TrainingTimeMs, MetricFilePaths, IngredientIds);
`DpoMetrics` от `DpoTrainer.StepVector` (Loss/Accuracy); тайминги по веткам
(`LogicalCapabilityTrainingBranchTiming`). Reward margin как отдельное поле здесь не
выводится (Loss/Accuracy есть; см. Пробелы).

---

## 3. Релевантность к ILearningDynamicGraph

ОЧЕНЬ ВЫСОКАЯ — это фактически готовый референс-пайплайн авто-DPO, который план
ILearningDynamicGraph должен либо переиспользовать, либо расширить:
- DPO/обучение: полный цикл «pair -> LoRA -> DpoTrainer -> checkpoint -> манифест ->
  регистрация юнита» уже реализован (`LogicalDpoTrainingUnit`, `DefaultLogicalCapabilityTrainer`).
- Граф/оркестрация: оперирует уровнями `ILogicalUnit`/`ILogicalFlow`/`ILogicalFlowFactory`
  и реестрами `InMemoryLogicalUnitRegistry`/`InMemoryLogicalFlowRegistry`,
  `ILogicalTopologyRegistry` (sub-phase узлы) — это рёбра/узлы, на которые ляжет
  DynamicGraph. Авто-генерация юнита+flow по «недостающему sub-phase» — это и есть
  динамическое доращивание графа.
- Авто-построение DPO: `BuildTrainingTextCore`/`BuildChosenCapabilityText`/Rejected
  показывают, как из наблюдённой ветки автоматически строится preference-ряд — прямой
  прообраз авто-DPO для узлов графа.
- Измерение: метрики/тайминги/манифесты/snapshot дают готовые поля для evidence-пакета
  (но без held-out accuracy, ablation, fresh-load — см. Пробелы; по
  CLAUDE.md «Completion evidence pattern» текущая полнота недостаточна для
  PASS_AS_TRAINED_FEATURE).
- teacher-provenance: chosen/rejected строятся ДЕТЕРМИНИРОВАННО внутри проекта (не от
  внешней LLM-teacher), значит Legal Teacher Gate скила здесь формально не нарушен
  (нет closed-proprietary teacher). НО манифест НЕ содержит полей teacher_access_mode/
  hosting_provider/model_id/model_license/provider_terms_checked. Для плана это пробел
  provenance: если DynamicGraph начнёт тянуть teacher-выводы, эти поля надо добавить.

---

## 4. Классификация (по правилам trainable-architecture-decision-SKILL.md)

| Механизм (file/class) | Заявленный паттерн | Фактический механизм | Уровень / классификация | Authority | Вердикт по скилу |
|---|---|---|---|---|---|
| `LogicalDpoTrainingUnit` (LlmIngredientLogicalUnits.cs) | обучающий юнит | LoRA+DpoTrainer.StepVector, тензоры, checkpoint | `ILogicalUnit` (обучающий) | runtime training | OK_ALLOWED_LEVEL — реальный цикл обучения с метриками |
| `TransformerEmbeddingKnnLogicalUnit<T>` (LlmIngredientLogicalUnits.cs) | learned KNN unit | EmbeddingModel(LlmModel)+HnswIndex поиск, fusion-скоринг | `ILogicalUnit` (инференс), KNN = компонент модели | runtime selection | OK_MODEL_COMPONENT — KNN/HNSW как memory-слой обученной embedding-модели, роль названа честно (см. KNN-note скила) |
| fusion-скоринг внутри `ScoreCandidates` (vector .45 + BM25 .35 + pattern .20) | — | детерминированная фьюжн-формула + `PatternClassifier` | внутренний детерминированный хелпер ВНУТРИ allowed-юнита | reranker над KNN-кандидатами | OK_INTERNAL_DETAIL пока он re-ranker внутри обученного юнита; ВНИМАНИЕ: для строгого semantic-claim нужен ablation/no-shortcut тест (детерминированный re-ranker не должен быть единственной authority) |
| `ReplayFusionRanker` (ReplayFusionRanker.cs) | E34V2 BM25+KNN fusion | детерминированный BM25-like + token-overlap | внутренний детерминированный хелпер | выбор replay-материала для обучения (НЕ runtime-ответ) | OK_INTERNAL_DETAIL — это data-selection/replay, не финальная authority ответа |
| `DefaultLogicalCapabilityTrainer` (TrainAndRegisterAsync) | тренер способностей | оркестрация обучения+регистрации | по сути уровень оркестрации обучения (через `ILogicalCapabilityTrainer`) | training-orchestration | OK — но это тренер, не runtime-оркестратор `ILogicalOrchestrator` |
| `GeneratedCapabilityFlow` / `GeneratedCapabilityFlowFactory` | retry-flow | оборачивает learned unit в flow | `ILogicalFlow` / `ILogicalFlowFactory` | runtime flow | OK_ALLOWED_LEVEL |
| `DomainClassifierUnit` (обучается в RegisterTrainedDomainClassifier) | классификатор домена | `.Train(...)`, persist в json, wrap `CompiledDomainClassifierUnit` | `ILogicalUnit` (обучаемый); внутри возможна counter/predicate-логика | runtime classify | OK_ALLOWED_LEVEL по обёртке; внутренний механизм классификатора НЕ читался здесь — доисследовать (определён вне проекта 18) |
| `LogicalTeachingDatasetWeighter` (D-09), `AutonomousLearningReplayMixer`, `AutonomousLearningTrustPromotionScorer` | — | детерминированные веса/микс/скоринг доверия | внутренние детерминированные хелперы (data weighting / trust) | НЕ runtime-ответ | OK_INTERNAL_DETAIL — корректно как baseline/механика наборов |
| `ConvergenceMonitor` | ранняя остановка | дельта loss по окну | внутренний хелпер обучения | training control | OK_INTERNAL_DETAIL |
| `AutonomousLearningIdleGate` | D-14 idle-gate | reflection в проект 16 IdleDetector | внутренний хелпер-гейт | gate расписания | OK_INTERNAL_DETAIL |

Скрытых «детерминированный скоринг, выдающий себя за trainable-юнит» НЕ обнаружено:
обучаемые юниты реально обучаются (DPO/LoRA/embedding+HNSW), а детерминированные
формулы честно играют роль re-ranker/data-selection/механики наборов внутри
allowed-уровней либо вне runtime-ответа. Главный риск по скилу — re-ranker внутри
`TransformerEmbeddingKnnLogicalUnit` без ablation-доказательства, что выбор не сводится
к детерминированной формуле (Minimum Breadth Challenge: confidence-floor и
fallback-to-first-entry стоит проверить отдельно — `confidence = 0.50 + fused*0.49`
даёт мягкий пол 0.50 даже при нулевом сигнале).

---

## 5. Пробелы / что доисследовать

- ABLATION / NO-SHORTCUT: нет видимого теста, что `TransformerEmbeddingKnnLogicalUnit`
  не редуцируется к детерминированному re-ranker (BM25/pattern). По скилу для
  semantic-claim это обязательное доказательство. Тесты этого проекта не в
  18.CapabilityTraining — искать в тестовом проекте (доисследовать).
- HELD-OUT МЕТРИКИ: выводятся LastLoss/LastAccuracy на обучающих парах; нет held-out
  accuracy / reward-margin / fresh-load / continue-training PROBE как отдельной
  evidence. Для PASS_AS_TRAINED_FEATURE (CLAUDE.md) этого мало.
- LIFECYCLE export/import/share: continue-training через родительский checkpoint есть
  (`ParentPolicyCheckpointPath`, `BuildTeachingStack`), НО export/import/share и
  публикация через 25.ModelServer/`IModelRepository` и locator (`IRemoteUnitLocator`)
  в проекте 18 НЕ реализованы (поля `ServerBaseUrl/ServerUnitEndpoint` в манифесте
  пустые). По CLAUDE.md «Trainable Unit pattern» — это требует доработки/проверки в 15/25.
- TEACHER-PROVENANCE поля: в манифесте нет teacher_access_mode/hosting_provider/
  model_id/model_license/provider_terms_checked. Сейчас chosen/rejected
  детерминированы (teacher-нарушения нет), но план DynamicGraph должен решить, нужны ли
  эти поля при будущем distillation.
- НЕ ЧИТАЛИСЬ полностью (защита от перегрузки): `TeachingRegistryPersistence.cs`
  строки 60-333 (restore/SHA256/`ManifestSha256`/`SourceDatasetSha256` детали),
  `LogicalExperimentOptimizations.cs` строки 70-254 (точные формулы
  `LogicalTeachingDatasetWeighter` D-09 max_weight, `AutonomousLearningReplayMixer`,
  `AutonomousLearningTrustPromotionScorer`). Definition `DomainClassifierUnit`,
  `LogicalCapabilityTrainingRequest/Result`, `InMemoryLogicalUnitRegistry`,
  `ILogicalTopologyRegistry` — вне проекта 18 (00.Core / 21.AgentInfrastructure /
  23.SemanticCompilerAdapter), доисследовать при детальном плане.
- DynamicGraph как явный уровень: в проекте 18 НЕТ типа `ILearningDynamicGraph` /
  графовых рёбер с many-to-many/fan-out/fan-in/multi-output edge contracts. Текущая
  модель — линейный sub-phase + flow-фабрики. По Graph-Style Reconstruction Gate скила
  план обязан надстроить графовые контракты рёбер поверх этого тренера.
