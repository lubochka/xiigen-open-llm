# Рекон: 27.TranslationDomain — образец обученного юнита (DPO / метрики / чекпойнт / promotionDecision)

Назначение: evidence для ПЛАНА ILearningDynamicGraph. Это сбор фактов, не разрешение на реализацию.
Цель существует и непустая. Прочитаны 5 ключевых файлов (из ~30 в области): `TranslationMachine.cs`, `TranslationTrainedUnits.cs`, `TranslationProductHeldOutRunner.cs`, `TranslationUnitContracts.cs`, `ru-en-product-prompt-b001-train-ready.jsonl`; точечно подтверждён `TatoebaPilotLoopRunner.cs` (promotionDecision).

## Что это и где (точные ключевые пути)

- Машина (уровень 5): `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs`
  - `TranslationMachine : ILearningMachine<TranslationDomain>` (стр. 7). Полный жизненный цикл: `TrainBatchAsync`, `TrainStructuredBatchAsync`, `EvaluateAsync`, `ExportAsync`/`ImportAsync` (только метаданные), `ExportCheckpoint`/`ImportCheckpoint`, `SaveCheckpointAsync`/`LoadCheckpointAsync` (реальное состояние на диск), `GetOrchestratorAsync`.
- Обучаемые юниты: `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs`
  - `SemanticTokenizerUnit`, `ExecutorUnit` (по направлению), `QualityVerifierUnit` — все реализуют `ITranslationUnitTrainer` (`TrainOnExample`, `MeasureAccuracy`).
- Контракты юнитов / DPO-строка / KNN-хранилище: `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationUnitContracts.cs`
  - `TranslationLearningExample` (поля `ChosenOutputJson`/`RejectedOutputJson`/`SemanticSignature`/`SourceId`/`HeldOut`, стр. 124), `ITranslationUnitTrainer` (стр. 161), `TranslationKnnStore` (мост train↔inference, стр. 178).
- Чекпойнт-контракты: `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationCheckpointContracts.cs`
- Раннер метрик product/exact/bootstrap + гейт промоушена: `C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs`
- Цикл Tatoeba + promotionDecision/novelty-gate: `C:/Projects/llm_mvp_core/27.TranslationDomain/TatoebaPilotLoopRunner.cs`
- Примеры DPO-данных: `C:/Projects/llm_mvp_core/27.TranslationDomain/TrainingData/*.jsonl`; held-out: `tr-ho-product-heldout-v*.jsonl`; источники с лицензией: `SourceData/tatoeba-pilot-attribution-manifest.json`.

## Как устроено / работает (кратко)

- DPO-пара (JSONL train-ready): поля `id, source, direction, pair_family, prompt, a_side, chosen, rejected, validation_outcome, validation_errors`. `chosen` — правильный выход; `rejected` несёт маркер `[WRONG: <weakness - gap - consequence>]`. Пример (`ru-en-product-b001-001`): a_side=`Фразовая карта: Привет; Я новый тут; чем занимаешься.`, chosen=`Phrase map: Hey!; ... 😊`, rejected=`... [WRONG: product_phrase_boundary_lost - drops casual chat tone or emoji ...]`.
- Обучение: `TrainStructuredBatchAsync` маршрутизирует каждый `example` по `UnitId` к `ITranslationUnitTrainer.TrainOnExample`. Юниты пишут выученное состояние в словари и в общий `TranslationKnnStore.AddPair(signature, source, target)`. `RewardMargin=0.03` только если все строки содержат `[WRONG:` (иначе 0.0) — это формальная метка наличия отрицательного примера, не вычисленная награда модели.
- Inference: `GetOrchestratorAsync` → `InlineTranslationOrchestrator` → `TranslationRoutingFlow` → юниты. Executor: exact-source → phrase-map → word-map → KNN-каскад (`FindNearestCascade`: full→prefix→structure signature) с guard-ами направления/скрипта.
- Метрики (раннер, на held-out): `RuntimeExactAccuracy` (точное совпадение строк), `RuntimeProductAccuracy` (составной: не bootstrap-fallback, все required-маркеры, нет forbidden, правильный скрипт, не копия источника, эмодзи и именованные сущности сохранены), `RuntimeBootstrapRate` (доля ответов с `:BOOTSTRAP]`), плюс TargetScript/CopiedSource/WrongDirection/Emoji/NamedEntity, BLEU-1, chrF1.
- Чекпойнт: `SaveCheckpointAsync` пишет один JSON атомарно (tmp→move) со всем состоянием (KnnStore, tokenizer, executors по направлениям, verifier, история, identity-строки, курсор Tatoeba). `LoadCheckpointAsync` — единственный валидный путь после рестарта, без replay/переобучения; `ImportCheckpoint` валидирует schema/machineId/domainTag и полноту направлений (`EXECUTOR_DIRECTIONS_INCOMPLETE`).
- Контаминация: раннер блокирует пересечение train↔held-out (`TRAIN_HELDOUT_OVERLAP`) до объявления accuracy.
- promotionDecision (`TatoebaPilotLoopRunner.cs:777`): значения `SKIP_SHORTFALL` | `PROMOTE_NEW_PATTERN_GT_{X}_AND_SEEN_TOKENS_LT_{Y}` | `SKIP_REQUIRES_...`. Пороги от `maxAccuracy`: `MinimumNewPatternRate = 1 - maxAccuracy`, `MaximumSeenTokenRate = maxAccuracy` (строгий novelty-gate из CLAUDE.md).
- Промоушен-гейт раннера (`TranslationProductHeldOutRunner.cs:211/389`): `AccuracyAfter >= AccuracyBefore + 0.02 && RewardMargin >= 0.025 && ProductAccuracy >= 0.95 && BootstrapRate <= 0.15 && TargetScript == 1.0 && Copied == 0 && WrongDirection == 0 && Emoji == 1.0 && NamedEntity == 1.0` → `PassesGate` + `GateNote` ("...gate passed/failed; do not promote...").

## Релевантность к ILearningDynamicGraph

- Готовый референс «обученного юнита» с полным lifecycle: train → eval на held-out → numeric-метрики → export/import → checkpoint на диск → fresh-load без replay. Это шаблон для узла графа (`ILearningUnit`) и для измерения качества по рёбрам.
- DPO-провенанс: уже есть формат пары с `chosen/rejected/[WRONG:]`, `SourceId`, `validation_outcome=train_ready`, контроль контаминации train↔held-out и манифест лицензии (`tatoeba-pilot-attribution-manifest.json`). Прямо переносится в требования teacher-provenance/source-license gate для авто-построения DPO.
- Оркестрация: `ILearningMachine → ILogicalOrchestrator → ILogicalFlow → юниты` — рабочий пример многоуровневой цепочки и KNN как компонента модели внутри уровня (не как самостоятельная архитектура).
- promotionDecision + novelty-gate — готовый пример «измеримого решения о продвижении» для узлов/рёбер DynamicGraph.

## Классификация уровней (по trainable-architecture-decision-SKILL.md)

- `TranslationMachine` → УРОВЕНЬ `ILearningMachine<TDomain>` (allowed). Полное lifecycle-доказательство присутствует. Verdict: `OK_ALLOWED_LEVEL`.
- `InlineTranslationOrchestrator` → `ILogicalOrchestrator` (allowed); `TranslationRoutingFlow` → `ILogicalFlow` (роутинг направлений).
- `SemanticTokenizerUnit` / `ExecutorUnit` / `QualityVerifierUnit` → обучаемые юниты, но через **доменный интерфейс `ITranslationUnitTrainer`, а НЕ `ILogicalUnit<TInput,TOutput>`**. По правилу скила (стр. 95–110) единственные allowed trainable-уровни — `ILearningMachine/ILogicalOrchestrator/ILogicalFlow/ILogicalUnit/ILearningMachinePair`. `ITranslationUnitTrainer` — доменный контракт обучения, не один из allowed-уровней. Класс-обёртки `ILogicalUnit` для инференса этих юнитов в прочитанных файлах НЕ видны (пробел — доисследовать `TranslationRoutingFlow.cs`/`TranslationDirectionAdapters.cs`). Verdict для самих юнитов как «уровня графа»: `OK_MODEL_COMPONENT` внутри `ILearningMachine`, но при переносе в DynamicGraph нужен явный `ILogicalUnit`-врапер.
- `TranslationKnnStore.FindNearest/FindNearestCascade` → внутренний компонент модели (KNN — audit-triggered, не запрещён). В коде честно помечен: «decides how to search, not what translation means; stored targets come only from training data» (`TranslationUnitContracts.cs:240`). Verdict: `OK_MODEL_COMPONENT` (поисковый компонент внутри ExecutorUnit), не финальная самостоятельная архитектура.
- Детерминистические хелперы внутри юнитов (ВНУТРЕННЯЯ ДЕТАЛЬ, не trainable-уровень): `ExecutorUnit._exactSourceMap` (точный source→target lookup, conf 0.93), `_phraseMap`/`_surfaceWordMap`/`_wordMap` (E26), `ApplyE27Reorder` (правило перестановки VALIDATE_CHECK+NULL_LITERAL), guard-листы `EnRu*` (function words). `QualityVerifierUnit` — правила `concept→required word` через парсинг строки issue (`TryParseIssue`), `Contains`-проверки. `SemanticTokenizerUnit` — rule-based токенайзер + словарь поправок (`_patternCorrections`/`_signatureCorrections`). Это `OK_INTERNAL_DETAIL`, но `_exactSourceMap`/точный lookup и порог `RuntimeExactAccuracy` — кандидаты на проверку leakage/replay (точное запоминание источника), если такой узел в графе будет заявлен как «обученный».

## Пробелы / что доисследовать

- НЕ прочитаны: `TranslationRoutingFlow.cs`, `TranslationDirectionAdapters.cs` (`TrainedTranslationAdapter`) — там может быть `ILogicalUnit`-обёртка инференса и точная привязка к allowed-уровню; `TranslationCheckpointContracts.cs` (полная схема записей чекпойнта); `TranslationStructuredBatchBuilder.cs` (как JSONL → `TranslationLearningExample`, режим `Structural` signature); `TranslationModelManifestContract.cs` (manifest/locator для `25.ModelServer`/`IRemoteUnitLocator`).
- Не подтверждено наличие реального опубликованного чекпойнта/манифеста через `25.ModelServer`/`IModelRepository` и `IRemoteUnitLocator` (требование Trainable Unit pattern из CLAUDE.md) — проверить отдельно.
- `RewardMargin` фиксированный (0.03/0.0) — не вычисленная reward-margin модели; для DynamicGraph это «метка наличия отрицательного примера», а не обучающий сигнал DPO. Уточнить, нужен ли реальный margin.
- Точное запоминание (`_exactSourceMap`, exact-accuracy) — провести ablation/no-shortcut проверку на leakage перед заявлением «обученный» для узла графа.
- Лицензионный provenance: проверить `SourceData/tatoeba-pilot-attribution-manifest.json` против teacher/source-license gate (Tatoeba = CC-BY; не MIT/Apache) для common-training пригодности.
