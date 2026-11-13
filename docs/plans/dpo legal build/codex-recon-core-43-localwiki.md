# Рекон: core / 43.LearningLocalWikiData — локальное вики-обучение, DPO-данные, раннер

Назначение файла: evidence для ПЛАНА ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph). Это сбор фактов,
не разрешение на реализацию. Все пути ниже реально найдены в репозитории.

Подзадача (узкая): как устроены локальное вики-обучение, DPO-данные и раннер в `43.LearningLocalWikiData`.

---

## 1. Что это и где (точные ключевые пути)

`43.LearningLocalWikiData` — это слой ДАННЫХ/реестра/отчётов/раннера для project-local wiki.
Runtime-логика (обучаемые юниты, генераторы DPO-пар, training pipelines, оркестратор) живёт НЕ здесь,
а в `42.ProjectWikiDomain` (это прямо зафиксировано в README и плане registry).

- `C:/Projects/llm_mvp_core/43.LearningLocalWikiData/README.md` — роль подпроекта: "It is data, not runtime logic"; layout (registry/reports/manifests/artifacts); правило владения; правило "no metric without durable report"; continuation rule (блокировка обучения до валидации реестра).
- `C:/Projects/llm_mvp_core/43.LearningLocalWikiData/PLAN-PROJECT-WIKI-TRAINING-ASSET-REGISTRY-v1.md` — главный архитектурный план: цель (XIIGen сам строит набор обучаемых wiki-юнитов), разделение код/данные/веса/project-evidence, training loop, статусы батчей, canonical units, манифесты, Gate B.
- `C:/Projects/llm_mvp_core/43.LearningLocalWikiData/PLAN-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE-v1.md` — расширенный план: Angular+.NET+SQL как одна система, weak-unit selection через `WikiTraceAggregator`, цель `cross_stack_advanced_wiki`.
- Раннер (единственный реальный исполняемый код обучения в 43): `C:/Projects/llm_mvp_core/43.LearningLocalWikiData/tools/ProjectWikiTrainingRunner/Program.cs`.
- Реестр обучаемых ассетов:
  - `.../43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json` (~1.6 МБ; status_counts: accepted=19, rejected=182, experimental_no_authority=193, blocked=26).
  - `.../registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.schema.json` (схема entry).
  - `.../registry/TRAINING-CONTINUATION-BLOCK.json` (блок-флаг; сейчас `false` = "registry validation passed").
  - `.../registry/ARTIFACT-INTEGRITY-AUDIT.json`.
- DPO-данные (примеры формата): `.../artifacts/base-wiki-runs/session-01-batch-00X/base-wiki-train-50.jsonl` и `...-heldout-50.jsonl`; реальные DPO-прогоны юнитов: `.../artifacts/runner-metrics/<run>/dpo-runs/<unit>/<batch>/dpo-pairs.jsonl` (+ `heldout-dpo-pairs.jsonl`, `checkpoints/policy_final.bin`, `project-wiki-dpo-metric-summary.json`).
- Отчёты: `.../reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.(md|json)` (выбор следующего слабого юнита, planning-only), `.../reports/LIVE-END-TO-END-RUNTIME-GATE.json`, `.../reports/UPGRADE-ROLLBACK-RUNTIME-GATE.json`, `.../reports/PLUGIN-RUNTIME-INTEGRATION-GATE.json`.
- Состояние плана: `.../STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json` (большой; execution_score, hole-register HOLE-001..HOLE-061).
- Другие тулзы: `.../tools/WikiContextHook/Program.cs`, `.../tools/PluginHostInvocationHarness/Program.cs` (плагин/хост-проба, не обучение).

Note для скила trainable-architecture-decision: фактический исполняемый код в 43 — это раннер-CLI и
проба-хуки; обучаемые юниты как классы (ILogicalUnit и т.п.) находятся в 42, а не в 43.

## 2. Как устроено / работает (кратко)

Раннер `ProjectWikiTrainingRunner/Program.cs` — тонкая CLI-обёртка с режимами (`--mode`):
- `training-flow` (по умолчанию) — строит `ProjectWikiTrainingFlowAdapter` (тип из `ProjectWikiDomain`, namespace `AgentInfrastructure.Logical`), запускает `LogicalFlowRequest<string>` и пишет `project-wiki-training-flow-result.json`. То есть обучение проходит через Logical-flow юнита из 42.
- `live-end-to-end-runtime-gate` — грузит реестр (`ProjectWikiTrainingAssetRegistry.LoadFromDataRoot`), валидирует (`ValidateTrainingMayContinue`), грузит canonical units через registry+`InMemoryRemoteUnitLocator` (открывает checkpoint, проверяет manifest, регистрирует/локатит remote unit), затем `ProjectWikiRealProjectValidationRunner.ValidateProjectsAsync` по реальным проектам; пишет numeric metrics (cpu_ms/ram_mb/elapsed_ms).
- `generate-project-wiki-domains` — `DocumentationOrchestrator.GenerateAsync` (граф документации: method/call-edge/value-flow/question-seed/negative-DPO counts).
- `upgrade-rollback-runtime-gate` — проба upgrade/rollback по манифестам/checkpoint/compatibility-ref.
- `plugin-runtime-integration-gate` — пробы VS / VS Code (загрузка сборки `42.ProjectWikiDomain`, vsix, node-хук `hookRunner.js`; флаги mockReadiness, modelWeightsEmbedded).

Canonical units (список из раннера, `CanonicalUnits()`), которые и есть целевые обучаемые сущности:
`ProjectWikiRequestRouterUnit, StackAndVersionClassifierUnit, SystemBoundaryMapperUnit, CodeTraceBuilderUnit,
EntityImpactMapperUnit, DocumentationGeneratorUnit, NegativeDpoGenerator, ProjectWikiQueryPlanningFlow,
WikiContextRoutingUnit, ProjectWikiAnswerComposer, ProjectAnswerVerifier, WikiTraceAggregator,
ProjectWikiTrainingOrchestrator`.

Реестр (schema + JSON): на каждый batch — entry с `unit_name`, `status`
(accepted/rejected/experimental_no_authority/blocked), `train_rows`/`heldout_rows`, пути train/heldout/runner_report,
`metrics{accuracy,reward_margin,loss,cpu_ms,ram_mb,elapsed_ms}` (отсутствующее = строка `"UNKNOWN"`),
`checkpoint_ref` (часто `...checkpoints/policy_final.bin`), `checkpoint_allowed_as_continuation`,
`allowed_next_action`, `manifest_ref`, `locator_key` (например `project-wiki.request-router`),
`remote_unit_id` (`remote-unit://project-wiki.request-router/v1`), `upgrade_policy`, `compatibility_report_ref`,
`rollback_ref`, `artifact_checksum` (sha256), `import_export_status`, `locator_status`. Жёсткое правило:
rejected checkpoint нельзя использовать как continuation source.

Формат DPO-строки (jsonl), пример из `base-wiki-train-50.jsonl`:
`{"id","source_id","source_license_state","source_authority_state","pair_kind","prompt","chosen","rejected","compiled_prompt_in_training","contains_project_local_facts"}`.
Конвенция: в `rejected` присутствует маркер `[WRONG: <reason> - <пояснение>]`; `prompt` содержит блок Evidence
(project_id, documentation_version, evidence_id, source=path:line, symbol) + Question. `chosen` обязывает
отвечать только из evidence и называть evidence_ids. Это и есть учительский сигнал "отвечай по evidence, не выдумывай".

Метрики раннера (пример `base-wiki-dpo-runner-report.md`): final_accuracy, average/final_reward_margin,
average/final_loss, cpu_percent, working_set_delta_mb, checkpoint_bytes_total; gate-порог
`final_accuracy >= 0.90 AND final_reward_margin >= 0.02`. Это реальный DPO-обучающий цикл (loss + reward margin),
не статическая валидация.

Training loop (план registry §9 / cross-stack): цель -> читать реестр -> читать слабые места из
`WikiTraceAggregator` -> выбрать weak unit -> построить 50 строк -> обучить -> проверить held-out ->
runner report -> обновить реестр -> опубликовать manifest -> зарегистрировать через locator -> выбрать следующий unit.
Управляет `ProjectWikiTrainingOrchestrator` (класс в 42).

## 3. Релевантность к ILearningDynamicGraph

Очень высокая. 43 — это готовый референс "как уже сделан DPO-цикл для графа wiki-юнитов":
- DPO/обучение: подтверждённый формат DPO-пар (chosen/rejected + `[WRONG:]`), реальный раннер с loss/reward_margin/held-out gate, checkpoints (`policy_final.bin`), manifest+locator lifecycle, реестр статусов. Это прямой прототип для авто-построения DPO под Learning*-уровни.
- Граф/оркестрация: 13 canonical units образуют конвейер (router -> classifier -> boundary mapper -> code trace -> entity impact -> doc generator / negative DPO -> query planning flow -> context routing -> answer composer -> verifier), управляемый `ProjectWikiTrainingOrchestrator`. Это de-facto узлы/рёбра графа обучаемых решений — релевантно для DynamicGraph nodes/edges.
- Измерение/выбор следующего: `WikiTraceAggregator` + `PROJECT-WIKI-NEXT-TRAINING-ACTION.json` — механизм выбора слабого узла по сигналам ошибок. Прямой аналог метрик/feedback-петли для ILearningDynamicGraph.
- Teacher/provenance: в DPO-строках есть `source_id`, `source_license_state`, `source_authority_state`
  (например `SYNTHETIC_SEED` / `ACCEPT_INTERNAL_SYNTHETIC`) — заготовка под provenance/legal-teacher gate скила
  (teacher_access_mode, model_license, closed_proprietary_model). В найденных данных teacher = синтетический seed,
  закрытых проприетарных учителей в просмотренных строках не видно (полный реестр не вычитан построчно).
- Lifecycle (export/import/checkpoint/locator/upgrade/rollback) уже реализован в раннере и схеме реестра — это шаблон для требований жизненного цикла обучаемого юнита из CLAUDE.md.

## 4. Классификация (по trainable-architecture-decision-SKILL)

Важно: сам каталог 43 — это ДАННЫЕ; классифицируются сущности, на которые он ссылается (код в 42), и его собственный код-раннер.

- `ProjectWikiTrainingFlowAdapter` (используется раннером; класс в 42): уровень **ILogicalFlow** (flow-adapter для обучения юнита). Allowed level.
- 13 canonical units — заявлены как обучаемые. По именам и роли:
  - `ProjectWikiQueryPlanningFlow` — **ILogicalFlow**.
  - `ProjectWikiTrainingOrchestrator` — **ILogicalOrchestrator** (выбор/запуск/последовательность обучения).
  - Остальные (`*RequestRouterUnit`, `StackAndVersionClassifierUnit`, `SystemBoundaryMapperUnit`, `CodeTraceBuilderUnit`, `EntityImpactMapperUnit`, `DocumentationGeneratorUnit`, `WikiContextRoutingUnit`, `ProjectWikiAnswerComposer`, `ProjectAnswerVerifier`, `NegativeDpoGenerator`, `WikiTraceAggregator`) — заявлены как **ILogicalUnit**. КЛАССИФИКАЦИЯ ТРЕБУЕТ ПРОВЕРКИ В 42: имена `router/classifier/selector/aggregator` — это ровно те шаблоны, которые скил помечает как audit-triggered. Они допустимы как ILogicalUnit ТОЛЬКО при наличии trained state + lifecycle (training data, checkpoint, manifest, locator, fresh-load, continue-training, ablation). Реестр показывает, что для большинства из них статус rejected/experimental, то есть обученность не доказана (две PASS-категории CLAUDE.md: honest-untrained-scaffold vs trained-feature).
  - `WikiContextRoutingUnit` и `ProjectWikiRequestRouterUnit` (router) — выше риск скрытого детерминизма (regex/keyword/lookup). Требуют проверки на `FORBIDDEN_RUNTIME_SHORTCUT` в коде 42.
- Код самого 43 (раннер `Program.cs`): **внутренний детерминистический хелпер / harness**, НЕ обучаемый уровень.
  Причина: `CanonicalUnits()`, `SelectPublishableEntry()` (LINQ-фильтр по status=="accepted"), `ResolveRepoPath`,
  `TryReadOneByte`, пробы плагинов — это детерминированная оркестрация запуска/проверки и чтение файлов.
  По скилу это eval/harness-логика (orchestration of training + gates), а не trainable authority. Это корректно:
  раннер не должен принимать обучаемых решений (план registry §2.3: runner "не решает, что архитектурно делать дальше").
- DPO-датасеты в 43 (jsonl): не код. По Required Dataset Table их вердикт зависит от строки; синтетический seed
  `base-wiki-*` = `KEEP_FOR_BASELINE_ONLY`/`KEEP_FOR_TRAINING` (порог в base-wiki batch-001 НЕ пройден:
  final_accuracy=0, reward_margin<0 — то есть baseline FAILED, использовать как negative/diagnostic). Юнит-DPO-прогоны
  со статусом rejected/experimental в реестре = `QUARANTINE_DO_NOT_TRAIN` / `KEEP_FOR_EVAL_ONLY` до адъюдикации.

Ссылка на правило: trainable-architecture-decision-SKILL "Decision Procedure" (шаги 3-8: определить механизм,
сопоставить ровно одному allowed level, проверить runtime authority и lifecycle) и "Verdicts"
(`OK_ALLOWED_LEVEL` только при наличии lifecycle-evidence; иначе `NEEDS_ILOGICALUNIT_WRAPPER` /
`FORBIDDEN_RUNTIME_SHORTCUT`). KNN-note: KNN/retrieval допустимы как компонент внутри allowed level.

## 5. Пробелы / что доисследовать

- Реальная реализация классов в `42.ProjectWikiDomain` НЕ прочитана (вне границ этой подзадачи). Чтобы классифицировать каждый из 13 юнитов окончательно (allowed level vs скрытый детерминизм), нужен рекон по 42: файлы `ProjectWikiRequestRouterUnit.cs`, `WikiContextRoutingUnit*`, `ProjectWikiQueryPlanningDpoGenerator.cs`, `ProjectWikiTrainingOrchestrator.cs`, `WikiTraceAggregator*`, `*TrainingPipeline.cs` (имена видны в STATE produced_artifacts).
- Полный реестр (1.6 МБ) вычитан только в голове (первые entry + status_counts). Построчная проверка teacher/provenance, наличие закрытых проприетарных учителей и распределение `pair_kind` не сделаны.
- `ProjectWikiTrainingFlowAdapter` / `ProjectWikiTrainingAssetRegistry` / `ProjectWikiRealProjectValidationRunner` — определения в 42; не подтверждены сигнатуры и логика валидации (ValidateTrainingMayContinue, held-out gate).
- Как именно строятся DPO-пары для каждого юнита (генераторы `*DpoGenerator.cs`) — ключ для авто-построения DPO в ILearningDynamicGraph; находится в 42, требует отдельного рекона.
- Связь имён интерфейсов: в коде используется `ILogical*` (ILogicalUnit/Flow/Orchestrator) и `ILearningMachine`; новый план говорит про `ILearning*` (ILearningUnit/Flow/Orchestrator/DynamicGraph). Нужно уточнить у плана/в 00.Core, это переименование/новый слой поверх существующей иерархии `LearningMachineContracts.cs` (см. CLAUDE.md §ILearningMachine) или отдельная ветка.
- `WikiContextHook/Program.cs` и `PluginHostInvocationHarness/Program.cs` просмотрены только по имени/Glob, не по содержимому (для текущего вопроса о DPO/раннере они вторичны).
