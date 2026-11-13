# Codex-Рекон: MVP «documentation learning» (DPO/обучение для документации legacy .NET)

Назначение: evidence для ПЛАНА `ILearningDynamicGraph`. Это сбор фактов, не разрешение на реализацию.
Дата: 2026-06-23. Роль: рекон-аналитик (Opus 4.8 MAX).

---

## 1. Что это и где (точные ключевые пути)

Область-цель существует и непустая. Это методология обучения маленькой локальной модели документировать legacy .NET/C# код «снизу вверх» (D1 метод → D2 класс → D3 цепочка вызовов → D4 workflow), партиями по 50 DPO-пар, по образцу translation/orchestration обучения.

Каталог гайдов: `C:/Users/lubao/Documents/xiigen/llm mvp/documentation learning/`
- `README.md` — карта и порядок чтения 13 документов; ядро-правило «50 пар = размер партии, не весь курс».
- `CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md` — 4 уровня D1–D4, состав пар на 50, gate-пороги.
- `CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md` — рантайм-контракт `CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>`, 9 логических нод (01 Source Inventory … 08 Documentation Arbiter … 09 Report Formatter), правило propagation, transit-gate.
- `CODE-DOC-DPO-REGISTRY-GUIDE-v1.md` — форма пары `Pair[ID]/A/B/Reason/Phrasing-B/Coverage`, 8 типов пар (CD-A … CD-H), правило аннотации `[WRONG: ...]`.
- `CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` — требования к chosen/rejected, overlap 0.25–0.55 (<0.60), A-side evidence.
- `CODE-DOC-EXAMINATION-METHOD-v1.md` — 13 вопросов экзамена, 10 per-row метрик, hard-fail флаги, пороги приёмки партии.
- Прочее: `CODE-DOC-50-PAIR-ABLATION-RUNBOOK-v1.md`, `CODE-DOC-EXECUTOR-HARD-RESET-v1.md`, `CODE-DOC-SMALL-MODEL-RATIONALE-v1.md`, `CODE-DOC-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md`, `EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md` (+`...-SESSION-v1.md`), `documentation learning.zip`.
- Ссылки на документы, которых НЕ нашёл в каталоге (упоминаются в README/headers, но Glob не показал): `CODE-DOC-SOURCE-REGISTRY-v1.md`, `CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md`, `CODE-DOC-DPO-REJECTED-PROMPTS-v1.md` — пробел, см. §6.

Фактические прогоны (evidence-only, выходы раннера): `documentation learning/documentation-runs/`
- `dotnet-csharp-runner-adapter/batch-001..005/` и `local-legacy-bottom-up-transit/batch-001/`.
- На партию: `TRAIN-PAIRS.jsonl` (50), `HELDOUT-ROWS.jsonl` (50), `SOURCE-LEDGER.jsonl`, `PAIR-VALIDATION-LEDGER.jsonl`, `TRAINING-METRICS.json`, `HELDOUT-EVALUATION.json`, `STREAM-STATE.json`, `NEXT-BATCH-DECISION.md`, `LOOK-LIKE-RUN-REPORT.md`, `runner-results/runner-NNN/` (`code-doc-batch-run-summary.json`, `code-doc-routing-dpo-pairs.jsonl`, `code-doc-heldout-cases.jsonl`).

Реализация рантайма в core-репозитории (НЕ в области-цели; найдено через Grep):
- `C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRoutingFlow.cs`
- `.../26.TrainableRouter/Training/CodeDocumentationTrainingRunner.cs`
- `.../26.TrainableRouter/Training/CodeDocumentationBatchFileRunner.cs`
- `.../26.TrainableRouter/Training/RoutingDpoPairBuilder.cs`, `RoutingTrainingModels.cs`, `OrchestrationRoutingCurriculum.cs`
- Тесты: `C:/Projects/llm_mvp_core/Tests/CodeDocumentationRoutingFlowTests.cs`, `.../Tests/CodeDocumentationTrainingRunnerTests.cs`
- Проектная заметка: `C:/Projects/llm_mvp_core/docs/projects/26-trainable-router.md`

---

## 2. Как устроено/работает (кратко)

ВАЖНО: в материалах есть ДВА разных DPO-уровня — их легко спутать.

A) DPO документации-контента (то, что описывают гайды как «обучение писать документацию»).
- Формат пары (`CODE-DOC-DPO-REGISTRY-GUIDE-v1.md`): `Pair[ID]`, A (code/call_context/known_callee_summaries/requested_documentation_level + полный version-aware stack: app_side, runtime_family, target_framework, csharp_language_version, language_version_confidence, project_style, web_framework(+version), frontend_framework(+version), stack_label), B (`chosen`/`rejected`), `Reason` (why_chosen/why_rejected), `Phrasing-B` (альт. корректная док-ция), `Coverage` (source_id, license_state, …, rejected_type, overlap_ratio).
- chosen = полезная док-ция (intent/design/edge-case, цитирует только видимый код и принятые callee-summaries, разделяет local vs delegated, фиксирует side-effects/error-paths, уважает framework lifecycle, без «модернизации»).
- rejected = правдоподобная ошибка с ОДНИМ дефектом + обязательная аннотация `[WRONG: <слабость> - <что не выучить> - <последствие>]`. 8 типов CD-A..CD-H (local purpose, IO/errors, side-effects, framework lifecycle, callee propagation, component/workflow, version-aware routing, bottom-up call transit).
- Метод обучения: партиями 50 пар + 50 disjoint held-out. Приёмка `ACCEPT_BATCH` ТОЛЬКО если: существует рантайм-flow и использован, реально прогнан DPO, реально прогнан held-out inference, DPO reward margin > 0, factual_accuracy ≥ порог уровня (D1/D2 ≥0.80, D3 ≥0.78, D4 ≥0.75), invented_behavior в пределах. Иначе статус `PAIRSET_READY_TRAINING_PENDING` / `..._RUNNER_MISSING`. Projected-метрики — только в look-like отчётах, не для приёмки.

B) DPO маршрутизации (routing) — то, что РЕАЛЬНО генерит раннер сейчас.
- В фактических прогонах (`runner-results/.../code-doc-routing-dpo-pairs.jsonl`, `TRAIN-PAIRS.jsonl`) chosen/rejected — это «Route to unit-dotnet-legacy-doc-l1» vs «Route to unit-orchestration-plan-v309 [WRONG: ...]». То есть тренируется ВЫБОР специалиста-юнита (документация vs оркестрация vs перевод), а не текст документации.
- Код: `CodeDocumentationTrainingRunner` строит `RoutingDpoPair` через `RoutingDpoPairBuilder` (`CodeDocumentationTrainingRunner.cs:99`, `:310`). Кандидаты в `CodeDocumentationRoutingFlow.cs:11` = {OrchestrationV309, TranslationRuEn, DocumentationDotNet}.
- Метрики раннера (`code-doc-batch-run-summary.json`, `TRAINING-METRICS.json`): routing_accuracy, version_gate_accuracy, transit_gate_accuracy, bottom_up_order_accuracy, callee_summary_transit_accuracy, local_vs_delegated_accuracy, cpu_ms, peak_ram_mb, elapsed_ms, dpo_pairs. Замечено расхождение: в batch-001 `version_gate_accuracy=0` у runner-001, но `=1` у runner-002 (`metric_mode: runner_verified`) — статус/версионный гейт нестабилен между прогонами.

Рантайм-flow (`CodeDocumentationRoutingFlow.RouteDocumentationAsync`): строит `RoutingRequest` → `DistributedRoutingBootstrap.RouteAndLocateAsync` → затем детерминированные source/version/transit гейты → вердикт `DOCUMENTATION_ROUTE_READY` либо `DOCUMENTATION_ROUTE_REVIEW_REQUIRED`. Сеты: общая модель/логика и наборы лежат в репозитории (источники — MIT/Apache, напр. `dotnet/docs` в `TRAIN-PAIRS.jsonl`); приёмка требует license proof (Node 01 reject без proof).

---

## 3. Релевантность к ILearningDynamicGraph

Высокая. Это готовый, отработанный на практике шаблон «авто-построение DPO + обучение по уровням + измерение + provenance», прямо переносимый на узлы/рёбра графа:
- DPO/обучение: две модели DPO (контент-док и routing) с явным chosen/rejected и обязательной аннотацией ошибки — прямой образец пар для рёбер графа (выбор следующего узла) и для узлов-исполнителей.
- Граф/оркестрация: 9-нодовая bottom-up цепочка (01→09) с propagation callee→caller — это и есть DynamicGraph-подобный поток с fan-in evidence; рантайм уже выражен как `ILogicalFlow` (Level 3), внутренние ноды могут быть `ILogicalUnit` (Level 2). Соответствует Graph-Style Reconstruction Gate скила (узлы/рёбра/контракты/кто владеет authority на ребре).
- Измерение: набор per-row метрик и hard-fail флагов (`CODE-DOC-EXAMINATION-METHOD-v1.md`) + батч-гейты — готовая модель «held-out + reward margin > 0 + пороги», нужная для приёмки рёбер/узлов графа.
- Teacher-provenance/legal: source-registry + license_state + Node 01 reject — совпадает с Legal Teacher/Provenance Gate скила (для целевого каталога `dpo legal build` это центральная тема). В материалах НЕ виден явный teacher (chosen/rejected, похоже, авторские/из MIT-доков, не дистилляция закрытой модели) — это плюс, но нужно подтвердить (см. §6).
- Граница detached: документ-flow требует, чтобы рантайм был именно flow/orchestrator, а не «filename switch»/`static switch` — совпадает с запретом скрытых детерминированных shortcut на рёбрах.

---

## 4. Классификация кода (уровни ILearningMachine vs детерминированные хелперы)

Правила: `trainable-architecture-decision-SKILL.md` (только 5 разрешённых уровней: ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair; всё прочее — internal detail, если не обёрнуто и без lifecycle-evidence).

- `CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>` (`CodeDocumentationRoutingFlow.cs:6`) → уровень ILogicalFlow (Level 3). Допустимый trainable-уровень. Контракт в гайде (`...ORCHESTRATION-PLAN-v1.md:61`) — `ICodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>`. ВЕРДИКТ: OK_ALLOWED_LEVEL (для routing-решения), при условии lifecycle-evidence (checkpoint/manifest/locator/fresh-load/continue-training) — её наличие в этом рекон-срезе НЕ проверено.
- Source/Version/Transit гейты внутри flow (`EvaluateSourceGate/EvaluateVersionGate/EvaluateTransitGate`, `...Flow.cs:51-53`) → детерминированные хелперы. Допустимо как internal detail/eval-gate ВНУТРИ flow; не являются самостоятельным trainable-уровнем и не должны быть финальной «семантической» authority. ВЕРДИКТ: OK_INTERNAL_DETAIL (как гейты), но если кто-то назовёт version/transit «обученной» проверкой — REPORT_CLAIM_TOO_STRONG.
- `CodeDocumentationTrainingRunner` / `CodeDocumentationBatchFileRunner` / `RoutingDpoPairBuilder` (`Training/*.cs`) → инфраструктура обучения/раннер (строит `RoutingDpoPair`, метрики, чекпойнт-артефакты). Не архитектурный уровень — это support/eval-only механика. ВЕРДИКТ: OK_INTERNAL_DETAIL (раннер), корректно лежит в core-репо (R5).
- 9 нод гайда (Node 01..09) → описаны как логические ноды; внутри допустимы `ILogicalUnit` (Level 2), «trainable behavior is the full flow/orchestrator» (`...ORCHESTRATION-PLAN-v1.md:76`). Это согласуется с правилом «фан-аут/фан-ин — на уровне flow/orchestrator».
- Сами DPO-пары/сеты (TRAIN-PAIRS/HELDOUT/registry) → данные, не код. По Dataset-таблице скила: контент-пары = REWRITE_TO_DPO/KEEP_FOR_TRAINING при наличии license proof; routing-пары = KEEP_FOR_TRAINING для routing-машины. Риск утечки: prompt-slice в routing-парах содержит stack/version-строку — нужно проверить, что выбор юнита не предсказывается тривиально (анти-leakage), иначе DATASET_REBUILD_REQUIRED. (Гипотеза для плана, не приговор.)

Скрытых детерминированных shortcut, выдаваемых за trainable, в просмотренном срезе НЕ обнаружено: routing идёт через `DistributedRoutingBootstrap.RouteAndLocateAsync`, а детерминированные гейты честно отделены и дают «REVIEW_REQUIRED».

---

## 5. Соответствие правилам проекта (кратко)

- R4 (нет скрытого детерминизма): соблюдено в коде — flow/runner, а не switch; гейты честно помечены.
- R5 (модели/сеты в репо, split): общая логика и раннер в `26.TrainableRouter`; источники MIT/Apache; OK.
- Provenance/Legal Teacher Gate: source-registry/license_state присутствуют как требование; явный закрытый teacher не виден (нужно подтвердить).
- Граф-gate скила (узлы/рёбра/authority/anti-shortcut на ребре): материалы дают почти готовую реконструкцию (9 нод + propagation), но без явной таблицы «кто владеет authority на каждом ребре» в терминах графа — это работа плана.

---

## 6. Пробелы / что доисследовать (для плана)

1. Отсутствуют в каталоге (но требуются README/промптом): `CODE-DOC-SOURCE-REGISTRY-v1.md`, `CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md`, `CODE-DOC-DPO-REJECTED-PROMPTS-v1.md`. Подтвердить наличие (возможно внутри `documentation learning.zip`) — без них source/version/transit гейты не доопределены.
2. Две DPO-цели (контент-док vs routing) НЕ разведены явно в гайдах: гайды учат контент-парам, а раннер тренирует routing. Для `ILearningDynamicGraph` нужно решить, какая из них = «ребро графа» (routing) и какая = «узел-исполнитель» (контент), и как они сочленяются.
3. Lifecycle-evidence (checkpoint/manifest/locator/fresh-load/continue-training) для `CodeDocumentationRoutingFlow` НЕ проверена в этом срезе — нужно для вердикта «trained feature», иначе только `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.
4. version_gate нестабилен между прогонами (batch-001: 0 vs 1) — выяснить причину (детерминированный гейт даёт разный результат → возможен дефект гейта или входных данных).
5. Teacher/provenance: подтвердить, что chosen-стороны контент-пар — авторские/из MIT-доков, а НЕ выход закрытой модели (Claude/OpenAI/Gemini), иначе по скилу `DATASET_REBUILD_REQUIRED`.
6. Anti-leakage routing-пар: проверить, что юнит не предсказывается из stack/version-подстроки prompt-slice (permutation/ablation) — иначе rebuild.
7. Связь с реальным графом: в core нет (в просмотренном) `ILearningDynamicGraph`-обёртки над этим flow; план должен определить, оборачивается ли `CodeDocumentationRoutingFlow` в orchestrator/граф или служит одним узлом.
