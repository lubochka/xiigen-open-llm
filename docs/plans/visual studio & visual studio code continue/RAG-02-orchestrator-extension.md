# RAG-02 — Папка «orchestrator extension»

- **Папка №:** 2 из 12 (sweep для плана «Visual Studio & VS Code continue»)
- **Физический путь:** `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension`
- **Роль папки (по Luba):** project-local wiki-модель + промпты и протоколы оркестратора. Ключевой контур: обучение документации/вики, orchestration prompts, и plugin/host-entrypoint, через который расширение реально дёргает ядро. **Релевантность: HIGH.**
- **Всего файлов:** **51** (проверено `find -type f`): ~42 markdown-документа + 3 JSON + 3 XML/drawio + 4 ZIP + 1 TXT + 1 PS1. Подпапки: `tools/` (1 файл), `reports/` (2 файла).
- **Статус этого RAG:** pass 1/10 — карта + полный манифест + первые глубокие разборы. Passes 2–9 добирают остальные файлы кумулятивно; pass 10 — sweep полноты + executive summary.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R02, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Содержимое папки «orchestrator extension» статично, но два живых факта, на которые опирался этот индекс, изменились; авторитетная таблица — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке.
> 1. **Картина слияния ветки исполнения перевернулась.** Ветка `orchestrator-extensions` (вершина `b293045bd`, не двигалась с 10 июля) — та, на которой план продолжения расширений собирался вести всю реализацию, — по пере-прогону слияния «на бумаге» (`git merge-tree --write-tree`, файлы не трогаются) теперь даёт в `main` **28 конфликтующих файлов** (было 0), а в latest-базу `codex/chat-gpt-5.5` — **0** (был ровно 1). `main` больше НЕ строгий предок ветки: общий предок `6624161be`, ветка +41 коммит / main +688. Правило мержабельности OP-67 в плане обновлено аддендумом-оверлеем 2026-07-28 в §1.1 файла `PLAN-VS-VSCODE-CONTINUE-v1.md` (пункт «(г)» OP-67-блока) — сверяться с ним, а не со старыми литералами.
> 2. **Код-сверки проходов 7 и 9 — срез рабочей копии `C:/Projects/llm_mvp_core` на ~13 июля.** С тех пор: 24 июля `main` (вершина `df379c624`) впитал ветки `DesignToCode-clean` и `codex/dpo-implementation` (заявки на слияние PR #38/#39); DPO-ветка получила +455 коммитов; сама рабочая копия стоит на `codex/dpo-implementation` (`0f221ac89`) с 486 несохранёнными записями. Выводы о структуре wiki-кода остаются верным историческим срезом; перед любой новой сверкой — сначала `git ls-remote` (спросить сервер о вершинах веток).
> 3. Ветки офлайн-документации кода `local_wiki_planning` (`219d7ba89`) и `claude/sonnet-local-wiki` (`93a463817`) с мая не двигались — переподтверждено `git ls-remote` 2026-07-28; содержательные описания wiki-контура в этом файле по-прежнему актуальны.

---

## ГЛАВНЫЙ ВЫВОД PASS 1 (для плана расширений)

Эта папка — это **проект «project-local wiki model» = внутренняя локальная модель документации кода** + весь её обучающий/оркестрационный контур. Она напрямую отвечает шагам целевого плана: (1) подключение к кодовой базе, (2) индексация локальной моделью офлайн, (4) прогон задачи через все стадии оркестрации, (6) DPO-дообучение внутренней модели.

**Найден реальный plugin/host-entrypoint контракт** (не порт, а именно контракт хост-интеграции):

- **`PLAN-PLUGIN-PACKAGING-v1.md`** — определяет бинарь **`wiki-context-hook.exe`** (проект `42.WikiHookRunner` / `42.WikiHookRunner.Mode3`), который **VS Code `.vsix` и Visual Studio `.vsix` реально запускают** как хук на каждый промпт. Это исполняемая точка входа расширения в ядро `llm_mvp_core`. Внутри — `WikiOrchestratorFactory.Create(projectRoot)` → `orchestrator.QueryAsync(...)`. Аргументы CLI: `--map`, `--ensure-ready`, `--json`, `--version`, hook-режим по умолчанию.
- **`PLAN-XIIGEN-LICENSING-v1.md`** — как этот бинарь монетизируется/дистрибутится: `LicenseManager.CheckDomain(...)` + license-сервер (Stripe/PayPal) + приватный репо доменных DLL. Бесплатный tier = `.NET` домен (`42.ProjectWikiDomain`), платные = Angular/AngularJS/Java/SQL.

**Cross-link с RAG-01 подтверждён:** RAG-01 нашёл, что local-wiki полностью поглощён активными `42.ProjectWikiDomain` + `43.LearningLocalWikiData`, а `ProjectWikiOrchestrationFlow`/`SubmitUserTaskApi` — вероятная host-facing точка. Эта папка показывает, что **весь дизайн этой модели, её промпты/протоколы и packaging-контракт живут здесь**. Ядро логики (`WikiContextRetriever`, `ProjectLocalWikiOrchestrator`, `ProjectWikiQueryPlanningFlow`, Mode 2/Mode 3) — в `42.ProjectWikiDomain`; тонкий host-адаптер (`wiki-context-hook.exe` + `hookRunner.ts`) — здесь описан.

**Текущее состояние модели (важно для честности плана):** базовая модель `xiigen-small-doc-wiki-base` **НЕ принята** — held-out Q/A завален (`generic_documentation_qa_accuracy = 0.286` при пороге 0.90; `hallucination_rate = 0.68` при пороге 0.00). Диагноз во всех отчётах один: hash-token DPO-раннер недостаточен, и **отсутствует обученный `ProjectWikiQueryPlanningFlow`**. Часть оркестрационного контура позднее (Phase 37) реализована в `ORCHESTRATION-MACHINE-TRAINING-PLAN-v20` (`PromptFlowGroundingModel`, `OrchestrationPlanMachine`) — несколько документов здесь помечены SUPERSEDED/IMPLEMENTED и указывают на актуальную `docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` и `docs/ARCHITECTURE.md`.

---

## Полный манифест файлов (денумератор покрытия — 51 файл, без пропусков)

Легенда типа: PLAN=план; PROTOCOL=протокол/правила; SESSION=исполнительная сессия; STATE=состояние; CORPUS=корпус данных; REPORT=отчёт; MANIFEST/JSON=машинные артефакты; DIAGRAM=drawio; BIN=бинарный архив; PROMPTS=исходные промпты; TOOL=скрипт.

| # | Файл (относительно папки) | Тип | Что это (по факту чтения) |
|---:|---|---|---|
| 1 | `orchestrator prompts.txt` | PROMPTS | **Фундамент требований.** Исходный founding-промпт Luba: 7 пользовательских сценариев (figma→система, figma в существующую систему, генерация системы с нуля, client-app, генерация контента, скиллы паттернов, UML-валидация), n8n-подобный flow-движок, GraphRAG-альтернативы, DevOps/K8s, «скиллы как часть кода», библиотека скиллов для copilot/claude/cursor. Deep-covered. |
| 2 | `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md` | PLAN | **МАСТЕР-архитектура** локальной wiki-модели. Phase 1 (документация→negative DTO→train) и Phase 2 (question bank→Q/A). 4 уровня обучения (L0 router … L4 feature/system), Mode 2/Mode 3 retrieval, промоушен-гейты, интеграция с 25/26/42. Status: REFERENCE-DO-NOT-EXECUTE. Deep-covered. |
| 3 | `DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1.md` | PLAN | Распределённая модель: `IOrchestrator→ILogicalFlow→ILogicalUnit`, `TrainableRoutingFlow`, `25.ModelServer`, `26.TrainableRouter`, `RemoteUnitManifest`, обучаемый роутер на DPO. **SUPERSEDED** (Phase 37 → v20). Deep-covered. |
| 4 | `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` | PROTOCOL | Контракт `ProjectWikiQueryPlanningFlow : ILogicalFlow<ProjectWikiUserQuestion, ProjectWikiQueryPlan>` — превращает вопрос разработчика в многошаговый план поиска доказательств до ответа. Интенты, типы query-step, sufficiency rules, DPO-пары, метрики. Deep-covered. |
| 5 | `PLAN-PLUGIN-PACKAGING-v1.md` | PLAN | **HOST-ENTRYPOINT.** Упаковка `llm_mvp_core` в бинари `wiki-context-hook.exe` (Mode 2+3) и `wiki-context-hook-mode3.exe` (BM25), IL-trim/single-file, publish-профили win/linux/osx, `.vsix` для VS Code и VS. `Program.cs`, `hookRunner.ts`. Deep-covered. |
| 6 | `PLAN-PLUGIN-PACKAGING-v1 to test with visual studio.md` | PLAN | Вариант packaging-плана с фокусом «тест в Visual Studio». Заголовок/архитектура совпадают с #5 (тот же `wiki-context-hook.exe`, Mode 2/3). Точный diff против #5 отложен на pass 2. |
| 7 | `PLAN-XIIGEN-LICENSING-v1.md` | PLAN | Монетизация/дистрибуция: license-сервер (Stripe+PayPal), приватный репо `xiigen_smart_domains`, `LicenseManager.CheckDomain` + `DomainLoader` + CDN signed URL. Free=.NET домен, Paid=Angular/AngularJS/Java/SQL. Head-covered. |
| 8 | `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` | PLAN | Закрывает разрыв «документирует код» → «помогает безопасно менять legacy». Все wiki-юниты — обучаемые; обучение генерическое ДО валидации на проектном коде. Head-covered. |
| 9 | `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` | PLAN | Для каждого gap: конкретная validation-задача + обязательные вариации ответа + лицензионные источники для pre-training. «Gap не закрыт, пока не прошли все вариации». Head-covered. |
| 10 | `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` | PROTOCOL | Реальный code-trace + обучение: одна оркестрационная заявка → project-local wiki, отвечающая с точными code-evidence. Ссылается на весь стек executor-протоколов. Head-covered. |
| 11 | `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` | PROTOCOL | Гейты «готово»: продукт = оркестрационная заявка, а не папка JSONL. Финальный отчёт только после report-eligibility gate. Head-covered. |
| 12 | `EXECUTOR-DISCIPLINE-RULES-v1.md` | PROTOCOL | Правила executor: сырые числа без ASCII-графики; не останавливаться на тривиальном шаге; не заявлять «done/promoted» до реального порога обучения. Head-covered. |
| 13 | `EXECUTOR-STAGE-PROMPT-v2.md` | PROTOCOL | Текущий короткий stage-prompt: перед каждой под-стадией восстановить цель. Целевая задача: «добавить `DiscountCode` в `CampaignCreateDto`». Head-covered. |
| 14 | `EXECUTOR-STAGE-PROMPT-v1.md` | PROTOCOL | **SUPERSEDED** v2. Историч. контекст; в файле мойибейк (битая кодировка русского). Не использовать. Head-covered. |
| 15 | `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` | PROTOCOL | **IMPLEMENTED Phase 37** (→ v20). Реальный цикл обучения orchestration/planning DPO-модели. Baseline `RUN-479/V309` (1404 rows, weighted acc 0.838). Head-covered. |
| 16 | `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` | PROTOCOL | ~80KB. **IMPLEMENTED Phase 37.** Живой гайд: библиотека проверенных решений юнитов + расширение на другие языки после каждого цикла. Head-covered. |
| 17 | `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` | PROTOCOL | Формат runner-отчёта (Section 22): eligibility gate, сырые CPU/RAM/time метрики, поведение при accuracy < 0.75. Head-covered. |
| 18 | `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` | PROTOCOL | ~280KB (самый большой текст). Русский протокол извлечения оркестрации из пакета документов («как цель → контролируемый проверяемый путь»). Источники: REAL-CHAT-CHAINS-v2, EPISODES-1000-v8. Head-covered; глубокий разбор — pass 2. |
| 19 | `ORCHESTRATION-PROBLEM-LIST-v1.md` | PROTOCOL | Короткий (978 B). P-001: статичный «мозг» (regex/if в `BuildIntent`/`EvidencePlanner`/`UnitSelector`/`HumanRouteComposer`). **RESOLVED Phase 37** через `PromptFlowGroundingModel` (D-16). Deep-covered. |
| 20 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-01-BASE-MODEL.md` | SESSION | Сессия 1: определить/обучить/валидировать базовую модель `xiigen-small-doc-wiki-base` до любых project-адаптеров. Head-covered. |
| 21 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-02-SCAFFOLD-AND-INGEST.md` | SESSION | Сессия 2: scaffold `.xiigen/wiki-domain/` + ingest принятой документации (реком. `askapediaweb`). Head-covered. |
| 22 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-03-RELATION-GRAPH-AND-NEGATIVE-DTO.md` | SESSION | Сессия 3 (Phase 1): relation-rows (call/value-flow edges) + верифицированные negative documentation DTO. Head-covered. |
| 23 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-04-TRAIN-DOCUMENTATION-WIKI.md` | SESSION | Сессия 4 (Phase 1): обучить локальный doc-wiki адаптер; только Mode 3 lexical, без GraphRag/Embedding. Head-covered. |
| 24 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-05-QUESTION-BANK-AND-QA-ROWS.md` | SESSION | Сессия 5 (Phase 2): trainable question bank + `batch-001` (50 train / 50 held-out Q/A). Head-covered. |
| 25 | `PROJECT-LOCAL-WIKI-MODEL-SESSION-06-QA-EVALUATE-REGISTER.md` | SESSION | Сессия 6 (Phase 2): eval held-out Q/A, verify citations, publish manifest через model-server, зарегистрировать routing-кандидата. Head-covered. |
| 26 | `PROJECT-LOCAL-WIKI-MODEL-STATE.json` | STATE | ~71KB. State-машина сессий. Текущий gate: **SESSION-01, query-planning batch-030 REJECTED → перейти к `WikiTraceAggregator`**. Указатели на 25/26/12/08 + DPO-runner. Head-covered (55/… строк). |
| 27 | `base-wiki-model-manifest.json` | MANIFEST | Манифест базовой модели. Status: **FAILED held-out QA, remediation required**. Контракты → 25.ModelServer/26.TrainableRouter; runner `InternalTrainReadyDpoRunner`; acceptance-gate + текущие метрики. Deep-covered. |
| 28 | `base-wiki-acceptance-report.md` | REPORT | Отчёт приёмки SESSION-01: DPO-gate PASS, но checkpoint held-out Q/A **NOT PASSED** (acc 0.286, halluc 0.68). Ремедиация = `ProjectWikiQueryPlanningFlow`. Deep-covered. |
| 29 | `base-wiki-heldout-evaluation.json` | JSON | Машинные метрики held-out eval: FAILED; `query_plan_accuracy=null`, `hallucination_rate=0.68`. Deep-covered (head). |
| 30 | `base-wiki-heldout-evaluator-gap.md` | REPORT | Gap: checkpoint-backed evaluator существует (`ProjectWikiBaseHeldoutEvaluator`), но `ProjectWikiQueryPlanningFlow` всё ещё отсутствует; 4 опции ремедиации. Deep-covered. |
| 31 | `base-wiki-training-plan.md` | PLAN | Как обучать `xiigen-small-doc-wiki-base` (генерически, без фактов проектов). Approved source families, split, taxonomy, gate. Head-covered. |
| 32 | `orchestrator wiki.drawio.xml` | DIAGRAM | **Главная архитектурная диаграмма.** Doc-flow (Make doc→Main IOrchestrator→Code→DocumentationOrchestrator→…→ProjectLocalWikiOrchestrator) и Q-flow (Question→L0 routing→ProjectWikiRoutingFlow→ProjectWikiQueryPlanningFlow→WikiContextRetriever→Mode2/Mode3). Deep-covered. |
| 33 | `orchestrator wiki.drawio Negative DTO.xml` | DIAGRAM | Вариант диаграммы с акцентом на «Teach local wiki llm» / Negative DTO рёбра. Head-covered. |
| 34 | `orchestrator wiki.drawio.zip` | BIN | ~108KB бинарный drawio-архив/бэкап. Не распакован (отложено). |
| 35 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` | CORPUS | ~1.8MB. **Актуальный** корпус 1000 кейсов оркестрации (русский): промпт → ожидаемая оркестрация. «Заменяет v1–v7». Формат: область/подтверждение/результат/риск/фокус. Head-covered (кейсы 0001–0003). |
| 36 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v1.md` | CORPUS | ~1.97MB. Прежняя версия корпуса 1000 кейсов (superseded v8). Spot-check в поздних passes. |
| 37 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v2.md` | CORPUS | ~2.22MB. Прежняя версия (superseded v8). |
| 38 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v3.md` | CORPUS | ~1.58MB. Прежняя версия (superseded v8). |
| 39 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v4.md` | CORPUS | ~2.46MB. Прежняя версия (superseded v8). |
| 40 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v5.md` | CORPUS | ~2.63MB. Прежняя версия (superseded v8). |
| 41 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v6.md` | CORPUS | ~2.60MB. Прежняя версия (superseded v8). |
| 42 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v7.md` | CORPUS | ~2.70MB. Прямой предок v8 (v8 «пересобран из v7»). |
| 43 | `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-v1.md` | CORPUS | ~22KB. Ранний малый корпус эпизодов (промпт→оркестрация первого слоя). Источник: `translated_files_FINAL.zip`. Head-covered. |
| 44 | `REAL-CHAT-ORCHESTRATION-CHAINS-v2.md` | CORPUS | Реальные цепочки оркестрации из чатов XIIGen (русский): Точный фрагмент/Граница задачи/Не смешивать с/Правильная оркестрация/Результат для человека. Head-covered. |
| 45 | `REAL-CHAT-ORCHESTRATION-CHAINS-v1.md` | CORPUS | **SUPERSEDED** v2 (v1 неправильно разрезал длинный message). Head-covered через заголовок v2. |
| 46 | `reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md` | REPORT | ~124KB. Точные промпты из `orchestrator prompts.txt` → **фактические** оркестрированные ответы XIIGen. Показывает реальные юниты: `TaskOrchestrationUnit`, `ProjectUnderstandingUnit`, `EvidencePlanner`, `UnitSelector`, `HumanRouteComposer`, `VerificationAbstentionUnit`, `CrossStackBusinessFlowGraphUnit`. Head-covered. |
| 47 | `reports/ORCHESTRATOR-PROMPT-ACTUAL-OUTPUTS-v1.md` | REPORT | ~100KB. Фактические выходы оркестрации промптов (похожий формат, «ORCHESTRATED», разбор на рабочие части, вопросы к project wiki/RAG). Head-covered. |
| 48 | `tools/Invoke-ProjectWikiPhaseReport.ps1` | TOOL | PowerShell-раннер: гоняет `ProjectWikiDomainTests`, пишет phase-метрики (pass rate, false-positive risk, CPU/RAM/elapsed) в `phase-metrics/`. Head-covered. |
| 49 | `wiki-sql-phase2-plans-v1.zip` | BIN | ~12KB. Архив планов SQL Phase-2 (stored-procedure impact). Не распакован (отложено на pass sql). |
| 50 | `wiki-sql-phase2-plans-v2.zip` | BIN | ~17KB. Архив планов SQL Phase-2 v2. Не распакован. |
| 51 | `wiki-training-language-plans-v1.zip` | BIN | ~18KB. Архив планов обучения по языкам (multi-language coverage). Не распакован. |

---

## Подробный разбор (deep-covered в pass 1)

### [HOST-ENTRYPOINT] `PLAN-PLUGIN-PACKAGING-v1.md` — реальная точка входа расширения
- **Что это:** архитектурно-сборочный план, как превратить `llm_mvp_core` (35+ проектов) в лёгкий бинарь для `.vsix`. Status: REFERENCE, 2026-05-15.
- **Ключевое (прочитано):** два бинаря с разными dependency-цепочками:
  - `wiki-context-hook-mode3.exe` (~3 MB) — только BM25 (`42.ProjectWikiDomain` → `12.SearchEngine`), офлайн < 200ms, не требует весов. Шипится Stage A.
  - `wiki-context-hook.exe` (~25–40 MB) — Mode 2 (семантика) + Mode 3; цепочка `42.ProjectWikiDomain → 08.GraphRag → 05.Embeddings → 09.Inference → 03.TransformerModel → 00.TensorCore/01.Tokenizer`, `06.VectorSearch`, `25.ModelServer`. Требует `model-manifest.json` status=PROMOTED, иначе fallback в Mode 3.
- **Точка входа (`Program.cs`):** `WikiOrchestratorFactory.Create(projectRoot)` → `GetReadinessStatus` → `QueryAsync(prompt, projectRoot, WikiQueryOptions(topN), ct)` → `WikiContextFormatter.Format(answer)` → JSON `{ systemPromptAddition }`. Хук **никогда не блокирует** промпт (всегда exit 0). ENV: `HOOK_PROMPT`, `XIIGEN_PROJECT_ROOT`, `WIKI_TOP_N`, `WIKI_CONFIDENCE_MIN` (0.65).
- **Дистрибуция:** VS Code `.vsix` кладёт бинари в `bin/{rid}/`, `hookRunner.ts::getHookBinaryPath()` выбирает по platform/arch; VS `.vsix` ссылается на `42.ProjectWikiDomain.dll` напрямую (`<Reference HintPath>`).
- **Запреты:** в бинарь **нельзя** тянуть тренировочные проекты (`04.TrainingLoop`, `10.LoRA`, `11.DPO`, `02.DatasetPipeline`, `14/16.*Learning`, `InternalTrainReadyDpoRunner`, любые `*Tests*`).
- **Релевантность:** **HIGH — это host-integration контракт расширений.** Теги: host-entrypoint, vsix, packaging, mode2/mode3, wiki-context-hook.
- **Cross-link:** пара к `42.ProjectWikiDomain` (ядро) и к RAG-01 `ProjectWikiOrchestrationFlow`/`SubmitUserTaskApi`; монетизация — `PLAN-XIIGEN-LICENSING-v1.md`.

### [ТРЕБОВАНИЯ] `orchestrator prompts.txt` — фундамент всей системы
- **Что это:** оригинальный founding-промпт Luba (англ.) — описание всего продукта XIIGen. Это исходные требования, из которых выросли все корпуса и отчёты этой папки.
- **Ключевое:** сервер = core .NET на Kubernetes, клиент = React Native; n8n-подобный flow (trace id + polling); первый flow = figma nodes tree (raw css/html) → несколько AI-моделей с positive/negative feedback → review другой моделью → результат клиенту; промежуточные результаты в Elastic; feedback (good/neutral/negative + текст) сохраняется для обучения. **7 сценариев использования** (figma→система; figma в существующую систему; генерация с нуля; client-app с формами/диетой; генерация контента; скиллы паттернов; UML-валидация). Требования: generic-интерфейсы для БД/очередей/AI, generic-репозиторий (Database Fabric), очередь джобов, GraphRAG/RAG для контекста AI-стадий, библиотека скиллов для copilot/claude/cursor, chat для взаимодействия.
- **Релевантность:** **HIGH (CONTEXT-фундамент).** Это тот самый вход, который разбирают `reports/ORCHESTRATOR-*` и `USER-ORCHESTRATION-SCENARIOS-*`. Теги: requirements, figma-to-code, design-to-code, orchestration, skills-library, RAG.

### [МАСТЕР-АРХИТЕКТУРА] `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
- **Что это:** главный reference-план локальной wiki-модели (REFERENCE-DO-NOT-EXECUTE; выполнять через SESSION-файлы из STATE.json).
- **Ключевое:** каждый проект получает свой `.xiigen/wiki-domain/` (documentation, evidence, relations, question-bank, training, heldout, weights, manifests). **Bottom-up compiler-like propagation:** callee meaning+params+return → caller получает значение → трансформирует/ветвит/персистит → caller meaning → next caller. Четыре ответственности (нельзя смешивать): `DocumentationOrchestrator` (док из кода), `ProjectLocalWikiOrchestrator` (ответы), `ProjectWikiRoutingFlow` (L0 роутер проект-да/нет), `ProjectWikiQueryPlanningFlow` (план поиска). 4 уровня обучения L0–L4. Два retrieval-режима: **Mode 3** = BM25 (`SearchEngineFacade`/`12.SearchEngine`) для детерминированного lookup/citation; **Mode 2** = `GraphRag.HybridRetriever` (`08.GraphRag`) + `EmbeddingModel` (07/09) — если embedding нет, **не** делать вид, что семантика, а сообщать `semantic_retrieval_unavailable`. Negative DTO taxonomy (13 типов) + verifier. Question bank (10 seed-классов) — Phase 2. Промоушен-гейт: ≥500 train/500 held-out, citation ≥0.98, halluc=0, cross-project isolation=1.0. Интеграция: `model-manifest.json` = project-view `RemoteUnitManifest`; публикация через `IModelRepository`; локатор `IRemoteUnitLocator`; слот `42.ProjectWikiDomain`.
- **Релевантность:** **HIGH.** Теги: local-model, offline-index, DPO, bottom-up-docs, mode2/mode3, learning-levels, question-bank.
- **Cross-link:** реализует диаграмму `orchestrator wiki.drawio.xml`; сессии 01–06; протоколы done/code-trace/gap-closure; base-model manifest.

### [РАСПРЕДЕЛЁННАЯ МОДЕЛЬ / ROUTER] `DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1.md` (SUPERSEDED)
- **Что это:** план «модель как design pattern», не монолит. **SUPERSEDED Phase 37** (интегрировано в `ORCHESTRATION-MACHINE-TRAINING-PLAN-v20`; `25.ModelServer`/`26.TrainableRouter` пока не собраны как отдельные подпроекты).
- **Ключевое:** одна способность = один независимо обучаемый unit с манифестом, грузится с model-server по unit id. Роутинг — не статическая таблица, а обучаемый `TrainableRoutingFlow : ILogicalFlow<RoutingRequest>` (DPO-пары chosen/rejected с `[WRONG:]`). Контракты: `IModelRepository` (LoadWeights/Upload), `IRemoteUnitLocator` (Locate/Register), `RemoteUnitManifest` (unit id/role/domain/branch/run/dataset-sha/model+lora config/tokenizer). Разные домены обучаются на своих branch-семействах; промоушен мёржит только проверенные манифесты, не сырые веса.
- **Релевантность:** **MEDIUM-HIGH** (архитектура шага 6 «внешняя модель дообучает внутреннюю» + маршрутизация задач). Теги: distributed-model, trainable-router, model-server, DPO-routing, manifest. Важно: помечен superseded — сверять с v20/ARCHITECTURE.md.

### [QUERY PLANNING] `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`
- **Что это:** REQUIRED-протокол недостающего юнита `ProjectWikiQueryPlanningFlow`. Локальная wiki **не имеет права** отвечать одним прямым lookup; сначала — план расследования.
- **Ключевое:** позиция в pipeline: `question → ProjectWikiRequestRouter → ProjectWikiQueryPlanningFlow → WikiContextRetriever → WikiAnswerComposer/ProjectLocalWikiModelUnit → ProjectAnswerVerifier → WikiTraceAggregator`. 13 интентов (field_addition, workflow_trace, stored_procedure_impact, missing_evidence_abstention, wrong_project_rejection …), ~30 типов query-step (включая полный SQL-набор для stored procedures). Sufficiency rules (для `entity_impact`/`field_addition`/`workflow_trace`/`change_location`/`stored_procedure_impact`). DPO: chosen = многошаговый план (не ответ), rejected = single lookup с `[WRONG:]`. Метрики: `query_plan_accuracy≥0.90`, `required_touchpoint_recall≥0.95`, `wrong_project_rejection≥0.98`. Gate: если `query_planning_flow_invoked=false` — run валит wiki-gate.
- **Релевантность:** **HIGH.** Это шаг 4 плана («задача идёт через ВСЕ стадии оркестрации»). Теги: query-planning, investigation-plan, sql-impact, DPO, sufficiency.
- **Cross-link:** его отсутствие — корневая причина провала base-model (см. ниже).

### [СТАТУС МОДЕЛИ] `base-wiki-model-manifest.json` + `base-wiki-acceptance-report.md` + `base-wiki-heldout-evaluator-gap.md` + `base-wiki-heldout-evaluation.json`
- **Что это:** приёмочный контур базовой модели `xiigen-small-doc-wiki-base`.
- **Ключевое (сходится во всех 4):** DPO-side gate **PASS** (final_accuracy=1, reward_margin≈0.024), но **checkpoint-backed held-out Q/A FAILED**: `generic_documentation_qa_accuracy=0.286` (порог 0.90), `hallucination_rate=0.68` (порог 0.00); `citation_format`=1.0, `abstention`=1.0, `answer_contract`=1.0 — проходят. Manifest status: `BASE_MODEL_HELDOUT_QA_FAILED_RUNNER_ARCHITECTURE_REMEDIATION_REQUIRED`. Диагноз: hash-token DPO-раннер ранжирует знакомые паттерны, но не является достаточной wiki-answer моделью; **не хватает обученного `ProjectWikiQueryPlanningFlow`**. Evaluator существует (`ProjectWikiBaseHeldoutEvaluator`), грузит `policy_step_*.bin`. `BASE_MODEL_ACCEPTED` не достигнут → SESSION-02 заблокирована.
- **Релевантность:** **HIGH (честность плана).** Показывает, что «локальная модель учится» пока НЕ доказана. Теги: base-model, heldout-fail, remediation, query-planning-missing, DPO-runner-limit.

### [ДИАГРАММА] `orchestrator wiki.drawio.xml`
- **Что это:** главная draw.io-схема, которой следует мастер-план. Читается напрямую (XML).
- **Ключевое:** два потока. Doc-flow: `Make a documentation → Main IOrchestrator → Code → DocumentationOrchestrator (reads code) → Documentation → Question Seeds → Wiki DPO Pairs + Held-out QA → (Teach local wiki LLM) → ProjectLocalWikiOrchestrator`. Q-flow: `Question about code → Main IOrchestrator (question entry) → [L0 project wiki routing] → ProjectWikiRoutingFlow → ProjectLocalWikiOrchestrator → ProjectWikiQueryPlanningFlow (plan wiki queries) → WikiContextRetriever (Mode2/Mode3) → ProjectEvidenceIndex (Mode3 BM25) / GraphRag.HybridRetriever (Mode2 semantic) → EmbeddingModel (07/09)`.
- **Релевантность:** **HIGH (единый визуальный контракт).** Теги: architecture-diagram, doc-flow, question-flow.

### [КОРПУСА и ОТЧЁТЫ] scenario-corpus + `reports/ORCHESTRATOR-*`
- **`USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md`** — актуальный корпус 1000 обучающих кейсов оркестрации (промпт → ожидаемая человеческая оркестрация: область/подтверждение/результат/риск/контрольный фокус). Это обучающий/эталонный материал для orchestration-модели. **HIGH как training-corpus.**
- **`reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md` / `...PROMPT-ACTUAL-OUTPUTS-v1.md`** — фактические оркестрированные ответы XIIGen на промпты из `orchestrator prompts.txt`. Раскрывают реальные внутренние юниты оркестратора: `TaskOrchestrationUnit`, `ProjectUnderstandingUnit`, `EvidencePlanner`, `UnitSelector`, `HumanRouteComposer`, `VerificationAbstentionUnit`, `CrossStackBusinessFlowGraphUnit`. **HIGH — показывают, что оркестратор реально возвращает.** Теги: actual-outputs, orchestration-units, evidence-first.

### [СОСТОЯНИЕ] `PROJECT-LOCAL-WIKI-MODEL-STATE.json`
- **Что это:** машина состояний исполнения (source of truth для порядка сессий).
- **Ключевое (head):** `status = SESSION_01_QUERY_PLANNING_BATCH_030_REJECTED_MOVE_TO_TRACE_AGGREGATOR`; SESSION-01 активна, 02–06 = NOT_STARTED. Последний gate: held-out accuracy 0.78 / answer-without-analysis 0.6 (<0.75) → wiki done=false. `next_action`: перейти к `WikiTraceAggregator` (обучить cross-unit trace-строки, доказывающие порядок QueryPlanningFlow → WikiContextRoutingUnit → WikiAnswerComposer и блокирующие answer-before-evidence). Инфра: `25.ModelServer`, `26.TrainableRouter`, `12.SearchEngine`, `08.GraphRag`, embedding 07/09, `InternalTrainReadyDpoRunner`.
- **Релевантность:** **HIGH.** Теги: state, session-order, current-gate, trace-aggregator.

### [МОНЕТИЗАЦИЯ] `PLAN-XIIGEN-LICENSING-v1.md`
- **Что это:** план лицензирования/дистрибуции доменов расширения. Head-covered.
- **Ключевое:** на машине разработчика `wiki-context-hook.exe` → `LicenseManager.CheckDomain("angular")` → читает `~/.xiigen/license.json` → если валидна, скачивает `DomainDll` с CDN по signed URL → `DomainLoader` → кэш в `~/.xiigen/domains/{domain}/{version}/`. License-сервер `xiigen.com/api/license` (Stripe/PayPal checkout+webhook, validate, download-url). Приватный репо `github.com/lubochka/xiigen_smart_domains` = source of truth для доменных DLL; плагин никогда не ходит в GitHub напрямую. Free = `.NET` домен (`42.ProjectWikiDomain`) в публичном репо; Paid = Angular/AngularJS/Java/SQL.
- **Релевантность:** **MEDIUM-HIGH** (как расширение получает доп-домены и деньги). Теги: licensing, domain-dll, stripe, paypal, cdn, domain-loader.

---

## Флаги plugin/host-entrypoint (для сборки плана)

1. **`PLAN-PLUGIN-PACKAGING-v1.md`** — ⭐ основной host-entrypoint контракт: `wiki-context-hook.exe` (`42.WikiHookRunner`), `hookRunner.ts`, `.vsix` для VS Code и Visual Studio. Ядро — в `42.ProjectWikiDomain` (`WikiOrchestratorFactory`, `WikiContextRetriever`, `ProjectLocalWikiOrchestrator`).
2. **`PLAN-PLUGIN-PACKAGING-v1 to test with visual studio.md`** — вариант того же контракта под VS-тестирование (diff — pass 2).
3. **`PLAN-XIIGEN-LICENSING-v1.md`** — host-side loader доменов (`LicenseManager`/`DomainLoader`) внутри того же бинаря.
4. Cross-link RAG-01: `ProjectWikiOrchestrationFlow`/`SubmitUserTaskApi` в `15.OrchestratorModel`/`42.ProjectWikiDomain` — это то, что бинарь дёргает внутри ядра.

---

## План покрытия по passes

- **Pass 1 (этот):** карта + полный манифест (51/51) + глубокие разборы host-entrypoint/master-plan/distributed/query-planning/base-model-status/диаграмма/state/licensing + характеристика корпусов и отчётов.
- **Passes 2–9 (кумулятивно, ещё не глубоко разобрано):**
  - Диф `PLAN-PLUGIN-PACKAGING-v1` vs `...to test with visual studio`.
  - Полные разборы executor-протоколов (#8–#17), sessions 01–06 (тела, не только цели).
  - `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` (280KB) — глубоко.
  - `ORCHESTRATION-TRAINING-PROTOCOL-*` (v1/living/reporting) — уточнить связь с Phase 37 / v20.
  - Тела отчётов `reports/*` (100–124KB) — какие ещё юниты и сценарии.
  - Sampling корпусов v1–v7 (уникальный контент против v8) + `REAL-CHAT-CHAINS-v2` тело + EPISODES-v1.
  - **Распаковка 3 zip** (`wiki-sql-phase2-plans-v1/v2`, `wiki-training-language-plans-v1`) и `orchestrator wiki.drawio.zip`.
  - `tools/Invoke-ProjectWikiPhaseReport.ps1` — полный разбор gate-логики.
- **Pass 10:** sweep полноты (каждый файл ≥ 1 evidence-строка) + executive summary для плана расширений.

---

## Честная строка покрытия

**9 из 51 файлов прочитаны полностью и глубоко разобраны** (`PLAN-PLUGIN-PACKAGING-v1`, `orchestrator prompts.txt`, `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1`, `DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1`, `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1`, `base-wiki-model-manifest.json`, `base-wiki-acceptance-report.md`, `base-wiki-heldout-evaluator-gap.md`, `orchestrator wiki.drawio.xml`). **Ещё ~4 прочитаны существенно (head/большой фрагмент)** (`PROJECT-LOCAL-WIKI-MODEL-STATE.json`, `reports/…EXACT-PROMPTS…`, `EPISODES-1000-v8`, `PLAN-XIIGEN-LICENSING-v1`). **Остальные text-файлы прочитаны как минимум по заголовку/началу** (evidence-based манифест-строки). **НЕ открыты бинарно:** 4 zip (характеризованы по имени+контексту) и корпуса v1–v7/chains-v1 (охарактеризованы как прежние версии файлов, которые открыты). Все 51 присутствуют в манифесте без пропусков.

---

## Pass 2/10 — executor/training протоколы (#8–#17) + packaging VS-variant diff

- **Статус этого pass:** глубоко разобраны 12 файлов: оба packaging-плана (точный diff), executor/training-протоколы #8–#17 и problem-list #19. Все прочитаны полностью (living-guide ~80KB прочитан целиком по секциям). Ниже — тела, ключевые правила/гейты и статус source-of-truth vs superseded.

### ГЛАВНЫЙ ВЫВОД PASS 2 (для плана расширений)

1. **Packaging «to test with visual studio» — это НЕ отдельный VS-специфичный план.** Он **байт-в-байт идентичен** `PLAN-PLUGIN-PACKAGING-v1.md`, КРОМЕ двух правок, и обе убирают `ProjectWikiQueryPlanningFlow` из Mode 2. То есть это «упакуй то, что реально собирается, для теста в VS», без зависимости от ещё-недостроенного query-planning юнита. Никакой новой VS-специфики (VS `.vsix` секция, `42.ProjectWikiDomain.dll` через `HintPath`) — она уже присутствует ОДИНАКОВО в обоих файлах.
2. **Разрыв «честности» pass 1 разрешён/уточнён.** Pass 1 нашёл, что base-модель `xiigen-small-doc-wiki-base` провалила held-out QA (0.286). Living-guide (#16) показывает, что это **другой** артефакт: 11 wiki-юнитов обучались отдельно через DPO (`InternalTrainReadyDpoRunner`) и достигли **per-unit PROMOTED_MILESTONE** (2026-05-19…22, реальные checkpoint/manifest/metrics), а `ProjectWikiQueryPlanningFlow`+`WikiTraceAggregator` прошли real-project validation (5 проектов, 6393 вопроса, exact_evidence 1.0). **НО у КАЖДОГО юнита стоит `full wiki done: false`.** Значит: отдельные DPO-юниты натренированы до милстоунов, но целостная Mode 2 wiki-модель как продукт НЕ принята. Для плана расширений это = ship Mode 3 (BM25) сначала, Mode 2 только после реального «wiki done».

### 1. Packaging diff — `PLAN-PLUGIN-PACKAGING-v1.md` vs `...to test with visual studio.md`

Точный `diff` (проверено командой `diff`, 2 хунка, файл-VS на 2 строки короче):

```diff
# Хунк 1 — карта зависимостей Mode 2 (строки 37–38 базового → 37 VS):
< → 42.ProjectWikiDomain (ProjectLocalWikiOrchestrator,
<     ProjectWikiQueryPlanningFlow, WikiContextRetriever)
---
> → 42.ProjectWikiDomain (ProjectLocalWikiOrchestrator, WikiContextRetriever)

# Хунк 2 — секция "Two Binaries", Deliverable 2 (строка 69 базового удалена):
< Runs: user query -> ProjectWikiQueryPlanningFlow -> retrieval -> answer
```

- **Headline diff:** VS-вариант **вычёркивает обе ссылки на `ProjectWikiQueryPlanningFlow`** из Mode 2 (из карты зависимостей 42.ProjectWikiDomain и из описания run-цепочки Deliverable 2). Больше НИЧЕГО не отличается — те же два бинаря (`wiki-context-hook-mode3.exe` ~3MB / `wiki-context-hook.exe` ~25–40MB), тот же `Program.cs`, те же publish-профили (win/linux/osx), те же size/startup-гейты (Mode3 < 8MB / Full < 50MB; cold start < 50ms), тот же CI, тот же список запрещённых training-проектов, та же VS `.vsix` секция с прямой ссылкой на `42.ProjectWikiDomain.dll`.
- **Смысл для VS-теста:** query-planning flow на момент 2026-05-15 ещё не был готов как рантайм-зависимость Mode 2, поэтому «тестовый» вариант описывает Mode 2 без него — то, что реально скомпилируется и запустится в VS. **Релевантность: HIGH** (это прямой ответ на «как тестировать в Visual Studio»: паковать без несобранного юнита). Теги: packaging-diff, vs-test, query-planning-dropped, mode2-simplified.

### 2. Executor/training-протоколы — тела и статус

#### [SOURCE OF TRUTH · единый список юнитов] `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` (#8) — deep
- **Что это:** каноничный источник **11 обучаемых wiki-юнитов**, их порогов и remediation. Закрывает разрыв «документирует код» → «безопасно менять legacy». Все wiki-юниты — trainable; generic-обучение ДО валидации на проектном коде; проектный код = ТОЛЬКО validation/evidence, не generic training source.
- **11 юнитов + пороги (дословно):** `ProjectWikiRequestRouter` (routing≥0.92, wrong_project==1.0, stale≥0.98) · `StackAndVersionClassifier` (stack≥0.95, unknown_false==0) · `CodeTraceBuilder` (method_recall≥0.90, call_edge_prec≥0.90) · `EntityImpactMapper` (impact_path≥0.85, touch_point_recall≥0.85, calc≥0.80) · `DocumentationGenerator` (doc_quality≥0.95, source_evidence==1.0, bottom_up==1.0) · `NegativeDpoGenerator` (chosen≠rejected==1.0, shallow_negation==0) · `ProjectWikiQueryPlanningFlow` (query_plan≥0.90, touchpoint_recall≥0.95, multi_hop≥0.90, missing_evidence≥0.95) · `WikiContextRoutingUnit` (route≥0.90, wrong_project==0) · `WikiAnswerComposer` (completeness≥0.88, exact_evidence≥0.98, halluc==0) · `ProjectAnswerVerifier` (halluc_catch==1.0, wrong_project_catch==1.0) · `WikiTraceAggregator` (trace_coverage==1.0, failed_unit_has_remediation==1.0).
- **Ключевые правила:** batch = **ровно 50 строк** (не 54, не 47). Developer-reasoning rows ≥40% широкого batch. Таксономия вопросов Level 0–5 (routing/version → symbol → local dependency → developer change reasoning → architecture → evidence discipline), включая `stored_procedure_impact/trace`. Default 50-row распределение задано (entity_impact 10, flow_trace 8, change_location 6, impact_analysis 6, caller/callee 6, arch 5, missing_evidence 4, wrong_project 5). Entity Impact Answer Contract (10 пунктов: entity→validator→mapper→service→repo→serializer→tests→unresolved→next). `WikiQueryTrace`/`WikiUnitTrace` схемы. 5 validation-проектов (wizGameserver, GenieDynamicServer, geniesoftGameMVPServer, genietasks, askapediaweb). Метрики валидны только если persisted в JSON, иначе `REPORT_ARTIFACT_WRITE_FAILED`.
- **Статус:** REQUIRED REFERENCE, **активный source-of-truth для имён/порогов юнитов** (living-guide и runner-reporting ссылаются на него как на канон). Не помечен superseded. **Релевантность: HIGH.**

#### [VALIDATION 45/53] `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` (#9) — deep
- **Что это:** для каждого gap — конкретная validation-задача + обязательные вариации + open-source pre-training источники с лицензионным гейтом. **Правило: gap не закрыт, пока не прошли ВСЕ вариации; одна прошедшая не считается.**
- **8 gaps → 53 обязательных вариаций** (проверено по summary-таблице): Entity impact (9 вар, ≥0.85), Multi-step flow trace (8, ≥0.85), Change location (7, ≥0.88), Impact/regression (5, ≥0.85), Architecture boundary (5, ≥0.80), Missing evidence/abstention (3, 100%), Query plan orchestration (8, query_plan≥0.90), Stored-procedure logic (8, sp_query_plan≥0.90). **База = 45, со stored-procedure = 53.** Промпты выведены из `orchestrator prompts.txt` сценариев (diet app, Figma, content pipeline). Целевой пример-якорь: «add DiscountCode to CampaignCreateDto».
- **Лицензионный гейт (важно для R5/common-training):** ACCEPT: MIT/Apache-2.0/BSD/CC-BY(attr). QUARANTINE: ShareAlike/unclear (напр. FEVER CC-BY-SA). BLOCK: NC/GPL/AGPL/книги/блоги/Stack Overflow/n8n(ACCEPT_SUPPORT_ONLY)/Wikipedia/MIT OCW. Конкретные ACCEPT-репо: `dotnet-architecture/eShopOnWeb`, `ardalis/CleanArchitecture`, `JasonGT/NorthwindTraders`, `Netflix/conductor`, `temporalio/samples-dotnet`, `MassTransit`, `TruthfulQA`. Validation-only (не training): DNN/DotNetNuke, old nopCommerce, SmartStoreNET(AGPL).
- **Статус:** REQUIRED REFERENCE, активный. **Релевантность: HIGH** (это тест-матрица extension-сценария; Gate 6 done-протокола требует все 45/53).

#### [ИСПОЛНИТЕЛЬНАЯ ТРАССА · Stages 0–13] `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` (#10) — deep
- **Что это:** реальный goal + трасса из кода + ожидаемый выход и метрики на каждой стадии. Объект-трасса `ProjectCodeTrace` (project_id, frameworks, symbols, methods, call_edges, value_flow_edges, unresolved_calls, documentation_units, negative_dpo_rows, wiki_query_plans, training_batches, coverage_snapshots) — обновляется после КАЖДОГО batch.
- **14 стадий (0–13):** Intake → Source Inventory/Stack detect → Symbol/Method trace → Callgraph/Value-flow → Bottom-up doc generation → Doc acceptance gate (0.95/1.00) → Negative DPO (Phase 1, не вопросы) → 50-row batch → Runner+weight publication → Query-planning+coverage verification → Ten-batch review → Entity impact map → Developer reasoning rows → WikiQueryTrace/WikiUnitTrace.
- **Current Implementation Status (дословно, важно для честности):** Реализовано частично: main-orchestrator entrypoint `ProjectDocumentationPipelineFlowAdapter` (через `LogicalOrchestratorAssistant.CreateDefault`), code trace в `ProjectDocumentationKnowledgeGraph.CodeTrace`. **НЕ done (12 пунктов):** batch-to-batch trace persistence, robust legacy-parser, реальная doc-LLM генерация, trainable negative DPO, обучающий раннер wired to domain, weight publication через `IModelRepository`, five-project coverage gate, Phase 2 question DB, routing через обученный `ProjectWikiRoutingFlow`, planning через обученный `ProjectWikiQueryPlanningFlow`, stored-procedure tracing.
- **Статус:** REQUIRED EXECUTION TRACE, активный. **Релевантность: HIGH** (карта того, что офлайн-индексация реально делает по стадиям).

#### [ГЕЙТЫ ГОТОВНОСТИ · Gate 0–7] `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` (#11) — deep
- **Что это:** граница «готово». **Продукт = оркестрационная заявка, а не папка JSONL.** JSONL (`evidence-index`, `documentation-units`, `call-edges`, `value-flow-edges`, `wiki-query-plans`…) — только snapshot/cache, не public API. In-memory source of truth = `ProjectDocumentationKnowledgeGraph`.
- **8 гейтов:** Gate 0 Report-Eligibility (report-scope + persisted artifact) · Gate 1 Orchestrated-Request (вход через `ProjectDocumentationPipelineFlowAdapter`/main orchestrator, не скрипт) · Gate 2 Documentation-Quality (doc≥0.95, source==1.0, bottom_up==1.0) · Gate 3 Relation-Graph consistency · Gate 4 Negative-DPO (Phase 1, `[WRONG:]`, chosen≠rejected) · **Gate 5 Training** (если runner нет → `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`, НЕ «trained») · **Gate 5.5 Unit-Training** (все 11 юнитов trained либо явно blocked; no silent deterministic placeholder) · **Gate 6 Answer** (все 45 base / 53 sp вариаций, query-plan гейт, exact_evidence==1.0, cross_project_leakage==0, halluc==0) · Gate 7 Five-Project Coverage.
- **Что НЕ считать done:** JSONL присутствуют; markdown присутствует; BM25 вернул строку; structural-тесты без обучения; 1–2 sample-вопроса; synthetic accuracy как real; deterministic placeholder назван trained-моделью.
- **Статус:** REQUIRED VERIFICATION PROTOCOL, активный. **Релевантность: HIGH** (определение «extension готово» = эти гейты).

#### [ДИСЦИПЛИНА ИСПОЛНИТЕЛЯ · Rule 1–11] `EXECUTOR-DISCIPLINE-RULES-v1.md` (#12) — deep
- **Что это:** правила против двух провалов — (а) стоп на тривиальном шаге ради approval, (б) «done/promoted» до реального порога. Overrides session-default early-stopping.
- **11 правил:** R1 сырые числа (без ASCII-баров) · R2 не стоп между пройденными гейтами (source→validate→standalone→combo→promote→next без вопросов) · **R3 минимальные пороги до «done»** (accuracy≥target И reward_margin≥0.02 два combo подряд И ≥200 rows И проверка на ≥1 validation-проекте с exact_evidence≥0.95 И query-plan прошёл И WikiUnitTrace=PASS) · R4 <0.75 — source+continue, не репортить тренд · R5 50 строк = минимум работы, не отдых · R6 «нужно больше данных» = действие, не стоп · R7 репорт vs вопрос · R8 запрещённые фразы («training complete», «unit ready», «waiting for instructions»…) · R9 report-gate до статуса (нельзя green-test выдавать за runner-report; verdict-слова PROMOTED/HELD/REJECTED/DATA_NEEDED/TRAINING_IN_PROGRESS зарезервированы для runner) · R10 время не стоп-гейт · R11 метрики сохранить в артефакт ДО репорта.
- **Статус:** REQUIRED, активный (governance-слой, на который ссылаются все протоколы). **Релевантность: HIGH.**

#### [STAGE-PROMPT текущий] `EXECUTOR-STAGE-PROMPT-v2.md` (#13) — deep
- **Что это:** короткий промпт, читаемый перед КАЖДОЙ суб-стадией и перед «done». Goal = natural request → trained units → evidence retrieval → exact answer с evidence_ids → WikiQueryTrace/WikiUnitTrace. Целевая задача-якорь: «add DiscountCode to CampaignCreateDto». Содержит: Goal-Check (действие полезно только если двигает к обученным юнитам/coverage/trace/blocker-fix/living-guide-update), Report-Gate self-check schema, **Placement Rule** (project-specific → `<project>/.xiigen/wiki-domain/` через `IRemoteUnitLocator`; general algorithm → `llm_mvp_core/42.ProjectWikiDomain`; static-код только для механики: hashing/counting/JSONL-validation/parsing), **Genie DNA Check** (generic interfaces / fabric-first / dynamic document / reusable reasoning), каноничные 11 юнитов, Sub-Stage Map (Stage 0–13 с «что читать» + дилеммами), «Before Claiming Done» чеклист (15 пунктов).
- **Статус:** REQUIRED SHORT PROMPT, **актуальный** (source of truth для стадийного исполнения; v1 → superseded этим файлом). **Релевантность: HIGH.**

#### [STAGE-PROMPT v1 · SUPERSEDED + МОЙИБЕЙК] `EXECUTOR-STAGE-PROMPT-v1.md` (#14) — deep
- **Статус:** явно **SUPERSEDED by v2** (в шапке: «STOP: do not execute from this v1 file… open v2»). Историч. контекст.
- **МОЙИБЕЙК подтверждён (НЕ исправлять):** тело написано битой кодировкой русского — UTF-8, прочитанный как Latin-1: напр. `## Ð¦Ð•Ð›Ð¬ Ð­Ð¢ÐÐŸÐ` = «ЦЕЛЬ ЭТАПА», `Ð¡ÑƒÐ±-ÑÑ‚Ð°Ð¿ 0` = «Суб-этап 0». Контент по смыслу = ранняя версия стадийного промпта (5 условий Rule 3 в конце: accuracy≥target, margin≥0.02 ×2, rows≥200, exact_evidence≥0.95, WikiUnitTrace=PASS). Не использовать для отчётности/решений. **Релевантность: LOW** (только историч.).

#### [ORCH-TRAINING · IMPLEMENTED Phase 37] `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` (#15) — deep
- **Статус-баннер (дословно):** `STATUS: IMPLEMENTED — Phase 37 (2026-06)`. **Актуальные gate-команды/метрики/checkpoint переехали в** `C:\Projects\llm_mvp_core\docs\plans\ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md`; checkpoint `C:\Projects\llm_mvp_core\models\orchestration\orchestration-v1\`. Т.е. это **описание реализованного цикла, source-of-truth = v20 в llm_mvp_core/docs.**
- **Что важно сохранить:** baseline `RUN-479/V309` (1404 rows, 88 batches; orchestration 501/plans 829/skills 74; weighted_acc 0.838068, final_acc 1.0, weighted_margin 0.028928). Цикл: Gate A (standalone candidate) → Gate B (full combo, только combo становится baseline). 50-row batch (стабильный shape) vs **16-row diagnostic probe** (для рискованного tail к сильному baseline). Ablation head/tail/single-row. DPO-row поля (id/origin/a_side/chosen/rejected/target_behavior/domain_focus…), hard-гейты (chosen≠rejected, `[WRONG:]`, no mojibake, no `sub_phase=3.6`). Runner: `InternalTrainReadyDpoRunner.csproj` в `experiments-v2\INTERNAL-TRAIN-READY-V191`. Known debt: `sub_phase=3.6` blocked (нет registered 3.6 в топологии), 21-module schema-shaped 3.2 rows blocked. **Релевантность: MEDIUM-HIGH** (это machine-training контур, реализованный; для extensions — фон, актуальный source = v20).

#### [ORCH-TRAINING LIVING GUIDE · IMPLEMENTED Phase 37 · ~80KB] `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` (#16) — deep (прочитан целиком)
- **Статус-баннер:** тот же `IMPLEMENTED — Phase 37`, source → v20 + checkpoint `models/orchestration/orchestration-v1/`.
- **Section 18 — Unit Solution Library (ГЛАВНОЕ):** по каждому из 11 wiki-юнитов записаны РЕАЛЬНЫЕ обученные checkpoint'ы + DPO-метрики + manifest'ы + failure-modes (даты 2026-05-19…22). Каждый юнит помечен **`full wiki done: false`**. Сводка статусов:
  - `ProjectWikiRequestRouter`: route target PROMOTED (`...trainable-route-promote-044`, structural_route 1.0), но DPO watch-метрика 0.88 < 0.92 — watch, не блокер.
  - `StackAndVersionClassifier`: PROMOTED (batch-001, все accuracy 1.0, conf 0.94).
  - `CodeTraceBuilder`: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST (batch-019, dpo_heldout 0.98). Watch: invented_callee 0.9.
  - `EntityImpactMapper`: PROMOTED_MILESTONE (confirmation-018, heldout 0.90, margin 0.039). Watch: wrong_project_evidence/missing_persistence 0.8.
  - `DocumentationGenerator`: PROMOTED_MILESTONE для stored-procedure documentation (batch-011, heldout 0.94; real-project = askapediaweb).
  - `NegativeDpoGenerator`: PROMOTED (promotion-051, heldout 0.98; 250/250 rows batches 047–051).
  - `ProjectWikiQueryPlanningFlow`: несколько PROMOTED milestones (query-series-028 heldout 0.92 + stored-procedure-011 heldout 1.0) + **REAL_PROJECT_VALIDATION_GATE_PASSED 2026-05-22** (5 проектов, 616 evidence, 6393 вопроса, exact_evidence 1.0, query_planning_invocation 1.0, entity_impact_trace 1.0, 13 sp-вопросов на askapediaweb).
  - `WikiContextRoutingUnit`: PROMOTED_TARGET_SIGNAL с known cross-unit `no_pre_checks` debt (heldout 0.92) — precheck-долг принадлежит QueryPlanningFlow+TraceAggregator.
  - `WikiAnswerComposer`: general PROMOTED (heldout 1.0); stored-procedure answer-composition до batch-036 (heldout 0.98) — очень длинный batch-лог 004–040, **остаточный `procedure_name_only` residual ~0.9 не закрыт**.
  - `ProjectAnswerVerifier`: PROMOTED_MILESTONE (batch-019, heldout 0.98). Watch: rejects_valid_answer 0.9.
  - `WikiTraceAggregator`: PROMOTED_TARGET_SIGNAL + context-before-query-plan sequence confirm (batch-022, heldout 1.0) + **REAL_PROJECT_TRACE_VALIDATION_GATE_PASSED 2026-05-22** (5 проектов, entity_impact_trace 1.0).
- **Section 19** — процедура обновления протокола (Trigger 1 promoted / 2 rejected / 3 new source / 4 ten-batch). **Section 20** — Multi-Language (принцип: reasoning-паттерн language-independent, меняются только domain-примеры). **Language Coverage Table: English = promoted (per-unit milestones), Python/TypeScript/Java/Rust = PENDING везде.** Python/TS adaptation notes есть (FastAPI/Pydantic/SQLAlchemy; NestJS/React Native). **Section 21** — authority (Section 18 = единственное место для proven-паттернов; версия+дата+baseline в шапке).
- **Мойибейк (мелкий, НЕ исправлять):** в Section 20 стрелки «English â†’ Python» тоже битые (UTF-8→Latin-1), но локально, не всё тело как в v1.
- **Релевантность: HIGH** (это единственный документ, доказывающий, что wiki-юниты РЕАЛЬНО обучались через DPO с метриками; критично для честности «локальная модель учится»). Актуальный source-of-truth для orchestration-machine — v20, но per-unit wiki training-ledger живёт здесь.

#### [RUNNER REPORTING · Section 22–25] `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` (#17) — deep
- **Что это:** формат обязательного runner-отчёта + resource-репортинг + поведение <0.75.
- **Section 22:** 22.0 Report-Eligibility Gate (runner report разрешён только когда report_scope==training_runner_report И runner_output_available; иначе — implementation/blocker status без verdict-слов) · **22.0A Persisted Report Artifact Gate** (CPU/RAM/accuracy НЕЛЬЗЯ из памяти — только из сохранённого JSON; пути: `runner-reports/<run-id>-<unit>-section22.json`, `phase-metrics/<run-id>/PHASE-RESOURCE-REPORT.json`; иначе `REPORT_ARTIFACT_WRITE_FAILED`) · 22.1 сырые метрики (weighted_accuracy, reward_margin, loss, heldout, cpu/ram/elapsed, checkpoint_size, verdict) · 22.2 формат (plain numbers) · 22.3 resource-пороги (cpu<30 light/30–70 normal/>70 heavy; ram_delta<50 normal/>200 investigate; elapsed_per_50rows<300k fast/>1.8M problem). Verdict ∈ {PROMOTED, HELD, REJECTED, DATA_NEEDED, TRAINING_IN_PROGRESS}.
- **Section 23:** <0.75 = data-sourcing phase (не репортить тренд, continue). Source Authority Gate (ACCEPT/QUARANTINE/BLOCK) + порядок (XIIGen sessions → XIIGen docs → permissive external → CC-BY → quarantine). Safe external families: dotnet/eShop, ardalis/CleanArchitecture, dotnet/efcore, temporalio/samples-dotnet, conductor-oss, MassTransit, dapr, TruthfulQA, AOSA(CC-BY). 200 rows = первый сигнал, не promotion.
- **Section 24:** 50-row loop + **24.0A Continuation/Replay Gate** (cumulative DPO должен грузить previous policy checkpoint + frozen reference; fresh runs: `policy_initialization: fresh_random_policy`; continued: previous checkpoint path + max_abs_delta>0; promotion требует ≥2 mixed confirmation runs после focused remediation; только `promotion-confirmation` публикует веса). Пример 2026-05-19 NegativeDpoGenerator (heldout 0.98). **Section 25:** per-unit dashboard (11 юнитов) + overall после 10 batches (units_at_target/above_0.75/below_0.75, coverage_delta).
- **Статус:** REQUIRED, активный (reporting-governance, не помечен Phase-37-implemented; на него ссылаются v1/living-guide/discipline-rules). **Релевантность: HIGH.**

#### [PROBLEM LIST] `ORCHESTRATION-PROBLEM-LIST-v1.md` (#19) — deep (короткий, был в pass 1)
- **P-001** (написан по-русски: «Это хуево»): статичный «мозг» в pipeline — `BuildIntent` (regex), `EvidencePlanner` (одинаковые вопросы), `UnitSelector` (`if`), `HumanRouteComposer` (шаблон), `RequiresExternalInput` (слова figma/audio/platform). **STATUS: RESOLVED — Phase 37** (v20): `PromptFlowGroundingModel` заменяет весь if/regex/deterministic flow-routing в `42.ProjectWikiDomain`; `FlowReferenceDetector` → build-time corpus/audit only (никогда runtime); добавлен `exactPromptLeakageRate` гейт (==0.0). См. D-16 в `DECISIONS-LOCKED.md`. **Релевантность: HIGH** (прямое подтверждение R4 «no hidden determinism» — статику убрали в обучаемый юнит).

### 3. Active source-of-truth vs superseded — сводка (для плана)

| Файл | Роль | Статус | Source of truth |
|---|---|---|---|
| `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1` (#8) | канон 11 юнитов+пороги | **АКТИВЕН** | этот файл |
| `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1` (#9) | 45/53 validation-матрица | **АКТИВЕН** | этот файл |
| `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1` (#10) | Stages 0–13 | **АКТИВЕН** | этот файл |
| `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1` (#11) | Gate 0–7 | **АКТИВЕН** | этот файл |
| `EXECUTOR-DISCIPLINE-RULES-v1` (#12) | Rule 1–11 governance | **АКТИВЕН** | этот файл |
| `EXECUTOR-STAGE-PROMPT-v2` (#13) | стадийный промпт | **АКТИВЕН** | этот файл (v1 superseded) |
| `EXECUTOR-STAGE-PROMPT-v1` (#14) | ранний промпт | **SUPERSEDED** (+мойибейк) | v2 |
| `ORCHESTRATION-TRAINING-PROTOCOL-v1` (#15) | machine-training цикл | **IMPLEMENTED Phase 37** | `docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` + `models/orchestration/orchestration-v1/` |
| `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1` (#16) | per-unit training-ledger | **IMPLEMENTED Phase 37** | v20 (но per-unit checkpoint-леджер живёт здесь) |
| `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1` (#17) | reporting-governance | **АКТИВЕН** (referenced) | этот файл |
| `ORCHESTRATION-PROBLEM-LIST-v1` (#19) | P-001 static brain | **RESOLVED Phase 37** (D-16) | v20 / DECISIONS-LOCKED.md |

**Нюанс для честности:** wiki-стек (#8–#13, #17) НЕ несёт баннер «Phase 37 implemented» — это дизайн/governance проектной wiki-модели; НО living-guide (#16, implemented) показывает, что эти 11 юнитов реально прошли DPO-обучение до per-unit милстоунов. Итог: юниты обучены до milestone, `full wiki done: false` везде → целостная Mode 2 wiki как продукт НЕ принята. Orchestration-machine контур (#15/#16/#19) реализован и переехал в `llm_mvp_core/docs/plans` v20.

### 4. Что остаётся на passes 3–9

- `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` (#18, ~280KB) — глубоко (самый большой текст, ещё не разобран телом).
- Тела sessions 01–06 (#20–#25) — не только цели, но входы/выходы/stop-gates.
- Тела `reports/*` (#46/#47, 100–124KB) — какие ещё юниты/сценарии оркестратор реально возвращает.
- Sampling корпусов v1–v7 (#36–#42) на уникальный контент против v8; тело `REAL-CHAT-CHAINS-v2` (#44); EPISODES-v1 (#43).
- **Распаковка 4 zip** (#34, #49–#51: drawio.zip, wiki-sql-phase2 v1/v2, wiki-training-language v1).
- `tools/Invoke-ProjectWikiPhaseReport.ps1` (#48) — gate-логика; `orchestrator wiki.drawio Negative DTO.xml` (#33).
- `PROJECT-LOCAL-WIKI-MODEL-STATE.json` (#26, ~71KB) — полный разбор state-машины (в pass 1 head).
- **Pass 10:** sweep полноты (каждый файл ≥1 evidence-строка) + executive summary для плана расширений.

### Честная строка покрытия pass 2 (кумулятивно с pass 1)

**Глубоко разобрано на pass 2: 12 файлов** — оба packaging-плана (точный `diff`), #8–#13, #14 (мойибейк), #15, #16 (весь ~80KB), #17, #19. С учётом pass 1 (9 deep + ~4 substantial) кумулятивно **≈21/51 файлов прочитаны полностью и глубоко разобраны**. НЕ трогалось на pass 2: #18 (280KB), sessions #20–#25 (тела), reports #46/#47 (тела), корпуса #35–#45 (кроме v8-head из pass 1), STATE.json #26 (полное тело), 4 zip #34/#49–#51, tools #48, diagram #33. Все 51 файла остаются в манифесте pass 1 без пропусков. НИ ОДИН файл не изменён (read-only соблюдён); мойибейк в #14 и #16-Section20 зафиксирован, НЕ исправлен.


---

## Pass 3/10 — execution sessions #20–#25 + STATE.json body + unit-library tail (#16)

Прочитано глубоко и целиком на pass 3: 6 session-файлов #20–#25 (тела, не только one-line goals из pass 1), `PROJECT-LOCAL-WIKI-MODEL-STATE.json` #26 (все 574 строки), и хвост Unit Solution Library `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` #16 Section 18 юниты 6–11 (строки 390–1342). Ни один файл не изменён (read-only соблюдён). Append-only в RAG.

### A. 6 execution-сессий #20–#25 — тела (цепочка сборки локальной wiki-модели)

Общий каркас: `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md` — reference-план, но **исполнять его напрямую запрещено**; исполняется по одному session-файлу за раз, с остановкой на каждом stop-gate (`execution_rule` в STATE.json). Каждая сессия имеет статус `EXECUTION SESSION` и жёсткое «не начинать пока предыдущая не прошла stop gate».

- **SESSION-01 — Base Wiki Model (#20).** Цель: определить/собрать/валидировать `xiigen-small-doc-wiki-base` ДО любого проектного адаптера. Outputs: `base-wiki-model-manifest.json`, `base-wiki-training-plan.md`, `base-wiki-heldout-evaluation.json`, `base-wiki-acceptance-report.md`. Acceptance gate: generic-doc Q/A accuracy ≥0.90, query-plan accuracy ≥0.90, touchpoint recall ≥0.95, citation-format ≥0.98, missing-evidence abstention ≥0.95, **hallucination == 0.00**; answer-contract обязан нести project id + documentation version + evidence ids + confidence; **никаких project-specific фактов** в базовой модели. Runner: `InternalTrainReadyDpoRunner.csproj -- --context 96 --batch-size 16 --save-every 25`. Stop: после acceptance report; **`.xiigen/wiki-domain/` в этой сессии не создавать**.
- **SESSION-02 — Scaffold + Accepted Documentation Ingest (#21).** Phase 1. Обязательная точка входа (иначе Stage 0 не пройден): `LogicalOrchestratorAssistant.RunAsync("Document this .NET project, train its local wiki, and register it for project questions.", ProjectDocumentationPipelineFlowAdapter.ScenarioId, metadata{project_root, wiki_domain_root, documentation_version, persist_artifacts, verify_coverage})`. Рекомендованный первый проект — `C:\Projects\askapediaweb`. Прямое создание `.xiigen/wiki-domain/` без этой точки входа **не проходит Stage 0**. Primary output = in-memory `ProjectCodeTrace` + `ProjectDocumentationKnowledgeGraph`; persisted-снапшоты — `.xiigen/wiki-domain/{domain-manifest.json, model-manifest.json, documentation/, evidence/evidence-index.jsonl, relations/{documentation-units,call-edges,value-flow-edges}.jsonl, eval/rejected-documentation-ledger.jsonl}`. **Documentation Acceptance Gate** (жёсткий): documentation_quality ≥0.95, stack_version ==1.00, source_evidence ==1.00, bottom_up_propagation ==1.00, workflow_use_case ==1.00 (workflow-доки), feature_boundary ==1.00 (feature-доки), system_boundary ==1.00 (system-доки), hard_failure_count ==0. Отклонённые доки → `eval/rejected-documentation-ledger.jsonl`, в тренинг не идут. **Mode 3 evidence-index = BM25** через `SearchEngineFacade` (12.SearchEngine) — «не описывать как vector search, EmbeddingModel не требуется»; строки индекса несут evidence_id/project_id/documentation_version/doc_path/source_path/source_line/symbol_id/relation_symbol_id/target_framework/csharp_language_version. **Source Consent Gate**: `source_authority` ∈ {first_party_internal, customer_approved, local_validation_only, blocked_no_consent}; `blocked_no_consent` → stop. Phase-boundary: **никаких question-bank/тренинг-строк** здесь.
- **SESSION-03 — Relation Graph + Negative Documentation DTO (#22).** Phase 1. Accepted docs → machine-readable relation rows + verified plausible-negative documentation DTOs. **Documentation Relation Contract**: symbol / meaning / source location / accepted parameters+meaning / callers / return type+meaning / evidence ids; для caller/workflow дополнительно called functions / callee meaning / arguments / origin каждого значения / return values / что caller делает с каждым return. **Entity Impact Map Contract**: кто accepts/validates/maps/stores/serializes/reads каждое поле, какие calcs/processes зависят, какие тесты покрывают, что unresolved (partial maps только с меткой). Negatives — только `NegativeDtoDocumentationOrchestrator` для DOC-негативов (wrong meaning / wrong caller / missing callee / wrong arg mapping / wrong return usage / wrong framework-version / invented side effect / stale) — **вопросы генерировать запрещено**. Developer-reasoning DPO-строки (entity_impact/flow_trace/change_location/impact_analysis/regression_risk) — это Phase-1 DPO, **НЕ Phase-2 question DB**; A-side = задача разработчика, chosen несёт grounded path, rejected правдоподобно-неполный с `[WRONG: ...]`. Row Gate: ровно 50 train + 50 heldout (disjoint), dev-reasoning ≥40% широких батчей, каждый negative DTO provably wrong против accepted docs, каждый caller-row ≥1 call-edge (или explicit no-callee reason), каждый call-edge с return → value-flow edge, **никаких вопросов**. Stop: после прохождения relation-graph + negative-DTO валидации (не просто после записи файлов). Адаптер здесь не тренируется.
- **SESSION-04 — Train Documentation Wiki (#23).** Phase 1. Тренировать/оценить локальный doc-wiki адаптер, опубликовать как кандидат `ProjectLocalWikiOrchestrator`. **Phase 1 НЕ требует семантики** — только Mode 3 lexical (12.SearchEngine); `GraphRag.HybridRetriever`/`EmbeddingModel` **запрещены до Phase 2**. Runner тот же. Heldout-метрики: documentation_unit/meaning/parameter/caller/return/callee/argument_mapping/return_usage/relation_edge/negative_dto_rejection/citation accuracy, hallucination_rate, cpu_ms/elapsed_ms per row, peak_ram, adapter_size + code-trace deltas (methods_documented/call_edges_covered/value_flow_edges_covered/missing_transit_edges_remaining/missing_workflow_methods_remaining). Publication: `LocalModelRepository.UploadAsync(GeneratedCapabilityCheckpointManifest manifest, string serverBaseUrl, ...)`, ссылка+checksum в `model-manifest.json`, register/locate через `IRemoteUnitLocator`, **веса не коммитить в git**. **Stop-gate (ключевой honesty-механизм):** либо accepted checkpoint, либо явный статус `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`; только потом писать `eval/documentation-heldout-evaluation.json`, `eval/documentation-training-run-report.json`, `model-manifest.json`.
- **SESSION-05 — Question Bank + Wiki Q/A Rows (#24).** Phase 2. Сгенерировать/обновить **trainable question bank**, экспортировать seeds, собрать batch-001 = 50 Q/A train + 50 heldout из accepted docs. **Не выбирать вопросы руками** — только flow: `accepted docs + evidence index + previous wiki Q/A failures → TrainableQuestionBankFlow → question candidates → ProjectWikiQueryPlanningFlow expected plans → utility scoring → 50 train + 50 heldout seed export`. Каждая bank-строка: question_id/question_status/utility_score/selection_policy_version/times_used_for_training/times_used_for_heldout/last_eval_result. Canonical taxonomy Levels 0–5 (L0 wrong_project/stale_version; L1 symbol/location/parameter/return; L2 caller/callee/dependency/value_flow; L3 entity_impact/flow_trace/change_location/impact_analysis/regression_risk/stored_procedure_impact/stored_procedure_trace; L4 architectural_decision/system_boundary/feature/integration_boundary; L5 missing_evidence/evidence/wrong_level/partial_trace). batch-001 distribution (сумма ровно 50): 10 entity_impact, 8 flow_trace, 6 change_location, 6 impact_analysis, 6 caller/callee/dependency/value_flow, 5 architectural/system_boundary/feature, 4 missing_evidence/evidence/partial_trace, 5 wrong_project/stale_version/wrong_level. **Вопросы ≠ documentation negatives**: `NegativeDtoDocumentationOrchestrator` и его verifier/ledgers здесь **запрещены**; question-строки не содержат rejected-ответов — только expected answer/abstention + expected `ProjectWikiQueryPlan`. DNN/старый nopCommerce/SmartStoreNET — validation-only. Stop: после structural validation; адаптер не тренируется.
- **SESSION-06 — Q/A Evaluate, Verify, Register (#25).** Phase 2. Тренировать wiki-адаптер, оценить heldout Q/A, верифицировать цитаты, опубликовать manifest, зарегистрировать unit как routing-кандидат. **`ProjectAnswerVerifier` проверяет**: у каждого ответа есть предшествующий `ProjectWikiQueryPlan`; план содержит нужные evidence-категории под тип вопроса; каждый cited evidence_id есть в `evidence-index.jsonl` и принадлежит тому же project_id; documentation version ответа == `domain-manifest.documentation_version`; текст не использует source-path другого проекта; missing-evidence → abstain. **20-real-question pass gate**: 20/20 внутри проекта, 20/20 с валидными evidence-id или валидным abstention, ≥19/20 совпал answer-type, ≥19/20 совпал plan-shape, 0 hallucinated citations, 0 stale-document acceptances, utility write-back для каждого heldout-вопроса. Каждый вопрос → `WikiQueryTrace`, каждый батч → `WikiUnitTrace`, ни один unit не падает без remediation. **Implementation Artifacts в `42.ProjectWikiDomain`** (создать/обновить .csproj + файлы): `ProjectLocalWikiOrchestrator` (владеет последовательностью `ProjectWikiQueryPlanningFlow → WikiContextRetriever → ProjectLocalWikiModelUnit → ProjectAnswerVerifier`), `ProjectWikiQueryPlanningFlow`, `WikiContextRetriever`, `ProjectLocalWikiModelUnit`, `ProjectEvidenceIndex`, `ProjectAnswerVerifier`, `ProjectDocumentationPipelineFlowAdapter`, `ProjectWikiCoverageVerifier`, `WikiTraceAggregator`, `ProjectWikiDomainServiceCollectionExtensions`. `ProjectEvidenceIndex` — внутренняя зависимость `WikiContextRetriever` для Mode 3; никто вне оркестратора не должен звать его как pre-orchestrator сервис. **Retrieval Mode Contract**: Mode 3 = lexical BM25 (`SearchEngineFacade` из 12.SearchEngine над evidence-index, без embeddings); Mode 2 = semantic (`GraphRag.HybridRetriever` из 08.GraphRag + активный `EmbeddingModel` из проекта 07 или 09 + relation graph + BM25 hits). **Hard rule**: 12.SearchEngine — только BM25, не vector; если `EmbeddingModel` отсутствует, Mode 2 возвращает `semantic_retrieval_unavailable` и либо использует Mode-3-only контекст, либо abstain. Stop: после `eval/{batch-001-heldout-evaluation,batch-001-query-plan-evaluation,real-question-verification}.json`, `traces/batch-001/{wiki-query-traces.jsonl,wiki-unit-trace.json}`, `question-bank/question-utility-updates-batch-001.jsonl`, обновления `model-manifest.json`. batch-002 не начинать до review.

### B. `PROJECT-LOCAL-WIKI-MODEL-STATE.json` (#26, 574 строки) — полное тело state-машины

- **Верхний статус:** `status = "SESSION_01_QUERY_PLANNING_BATCH_030_REJECTED_MOVE_TO_TRACE_AGGREGATOR"`; `current_session = SESSION-01`; `target_project_slot = 42.ProjectWikiDomain`. `existing_infrastructure`: model_server 25.ModelServer, trainable_router 26.TrainableRouter, lexical 12.SearchEngine, semantic 08.GraphRag, embedding 07/09, runner `experiments-v2/INTERNAL-TRAIN-READY-V191/.../InternalTrainReadyDpoRunner.csproj`.
- **Sessions-массив (ЧЕСТНАЯ ПОЗИЦИЯ ЦЕПОЧКИ):** SESSION-01 status = `QUERY_PLANNING_BATCH_030_REJECTED_MOVE_TO_TRACE_AGGREGATOR`; **SESSION-02, 03, 04, 05, 06 = `NOT_STARTED` (все)**. То есть 6-сессионный конвейер реальной сборки проектной wiki (scaffold реального проекта → ingest → relation graph → train → question bank → QA/register) **не начинался за пределами SESSION-01**; вся зафиксированная работа — это per-unit DPO-ремедиация 11 юнитов, идущая под «зонтиком» SESSION-01. Это прямо согласуется с «full wiki done: false» повсюду: интегрированный Mode-2 wiki-продукт end-to-end не принят.
- **`last_gate` (дословно):** ProjectWikiQueryPlanningFlow batch-030 answer-analysis mixed **rejected**. Deterministic verdict = `PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED`, но DPO heldout accuracy 0.7799999714, heldout margin 0.0055291420, final accuracy 0.8400000036, cpu_ms 163734, avg_cpu 12.59%, peak_ram 90.10MB, elapsed 216964ms; критично — **`answer_without_intermediate_analysis` heldout accuracy = 0.6 < 0.75**. «Full wiki done false.» То есть детерминированный структурный gate прошёл, а DPO-watch-метрика (ответ без промежуточного анализа доказательств) провалилась → это и есть блокер.
- **`next_action` (дословно):** не продолжать с query-planning batch-030; перенести remediation на **`WikiTraceAggregator`** — обучить cross-unit trace-строки, доказывающие порядок `ProjectWikiQueryPlanningFlow → WikiContextRoutingUnit → WikiAnswerComposer` и блокирующие «answer-before-evidence-analysis»; перед user-facing выводом персистить CPU/RAM/accuracy отчёт. (`latest_query_planning_answer_analysis_mixed_probe.next_valid_unit = "WikiTraceAggregator"`.) **Связка с Unit 11:** это ровно ответственность WikiTraceAggregator (cross-unit sequence check) — STATE.json замыкает петлю на юнит из Section 18.
- **`latest_real_project_validation` = `VALIDATION_GATE_PASSED`:** project_count 5, evidence_record_count 616, total_question_count 6393, exact_evidence_accuracy 1.0, query_planning_invocation_rate 1.0, entity_impact_trace_pass_rate 1.0, stored_procedure_questions 13 (проект `C:\Projects\askapediaweb`), cpu_ms 23750, peak_ram 16.29MB, elapsed 22874ms. Summary/report в `phase-metrics/2026-05-21-real-project-wiki-validation/`.
- **`latest_base_wiki_run`:** batch `session-01-batch-003`, run_id `base-wiki-session-01-batch-003-20260515-1005`, status `DPO_RUNNER_COMPLETE_GATE_PASSED`, final_accuracy 1, final_reward_margin 0.02105, elapsed 13260ms, avg_cpu 12.70%, max_cpu 13.70%, working_set_delta 22.85MB, **`project_wiki_domain_tests`: 12/12 passed**. → Базовая модель (session-01) свой DPO-gate уже прошла; блокирует не она, а per-unit query-planning/trace-aggregator ремедиация.
- **Тело STATE.json ~на 85% — журнал WikiAnswerComposer stored-procedure батчей 024–040 + query-planning 029–030** (каждый как отдельный ключ `latest_answer_composer_*` / `latest_query_planning_*` с train_rows/heldout_rows/heldout_accuracy/margin/loss/final_*/cpu/ram/elapsed/per-type accuracy/next_baseline_policy+reference/manifest+sha256/phase_report). Ключевые milestone/провалы: batch-024 PROMOTED (heldout 0.92), 025 residual REJECTED (proc_name_only 0.8, neg margin), 026 lowdose REJECTED (proc_name_only 0.733), 027 candidate → **028 PROMOTED_MILESTONE** (heldout 0.98, proc_name_only 0.9, missing_sql_parameters/result_shape/table_side_effects/dotnet_callers все 1.0, но один heldout row с negative margin), 029/030/031 REJECTED, 032 candidate → **033 PROMOTED_MILESTONE** (heldout 0.98), 034 REJECTED, 035 candidate → **036 PROMOTED_MILESTONE** (heldout 0.98, margin 0.0465), 037–040 REJECTED (040 simplified-name обвалил proc_name_only до 0.0, margin −0.10). Сквозной остаточный gap во всех промоушенах: **`procedure_name_only` держит ровно один held-out negative-margin row → «continue remediation without full wiki done claim»**. Промоушен-паттерн строго candidate→confirmation (2 mixed run) и только confirmation-батч публикует веса (совпадает с #17 Section 24.0A).

### C. Unit Solution Library — хвост, юниты 6–11 (#16 Section 18, реальные checkpoint/метрики/status)

- **Unit 6 — `NegativeDpoGenerator`.** Отв.: плаузибельные, но provably-wrong rejected-примеры из accepted docs+trace. Min-метрики: chosen≠rejected 1.0, wrong_annotation_rate 1.0, provably_wrong_against_trace 1.0, valid_negative_diversity ≥0.90. **Status: PROMOTED** (batch `negative-dpo-generator-promotion-confirmation-051`, heldout 0.980000019, margin 0.003840186, 250 train / 250 heldout по батчам 047–051). Failure (2026-05-19): изолированный focus на `missing_parameters` не улучшил (batch048 застрял 0.70) — вылечил mixed parameter-variety батч, а не больше одинаковых focus-строк.
- **Unit 7 — `ProjectWikiQueryPlanningFlow`.** Отв.: developer-вопрос → многошаговый evidence-investigation план ДО retrieval/answer. Min: query_plan_accuracy ≥0.90, touchpoint_recall ≥0.95, multi_hop_sequence ≥0.90, missing_evidence_detection ≥0.95, irrelevant_query ≤0.05, answer_readiness ≥0.90. Милстоуны: (1) `promotion-confirmation-025` PROMOTED heldout 0.98/margin 0.00236; (2) `stored-procedure-promotion-confirmation-011` PROMOTED (stored_procedure_impact) heldout 1.0; required rejected types = procedure_name_only/missing_sql_parameters/missing_result_shape/missing_table_side_effects/missing_dotnet_callers; validation-only = DNN/nopCommerce/SmartStoreNET; (3) `query-series-promotion-confirmation-028` **PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST** heldout 0.92, query_plan_accuracy 1.0, multi_hop 1.0; (4) **REAL_PROJECT_VALIDATION_GATE_PASSED 2026-05-22** (5 проектов / 616 evidence / 6393 вопроса / exact_evidence 1.0 / invocation 1.0 / entity_impact_trace 1.0 / 13 SP-вопросов на askapediaweb). Важные failure-уроки: не использовать generic-токен «execute» как SP-сигнал (мисроутит методы с именем Execute); marker-heavy rejected для missing_sql_parameters ухудшает (probe-020 heldout 0.6); слишком грубый/короткий rejected обваливает answer-analysis (probe-024 heldout 0.0); global required_difference портит уже стабильные типы (probe-021 heldout 0.58); «avoid stale» wording триггерит StaleDocumentationRejection и режет multi-hop до 0.8 (probe-027). **full wiki done: false.**
- **Unit 8 — `WikiContextRoutingUnit`.** Отв.: выбрать evidence-retrieval путь и scope. Min: context_route_accuracy ≥0.90, wrong_project_context_rate ==0, entity_impact_route_accuracy ≥0.85. Route-виды: Mode3LexicalEvidence (прямые symbol/location/param/return), Mode2SemanticHybrid (multi-hop entity/workflow/SP), MissingEvidenceAbstention, WrongProjectRejection, StaleDocumentationRejection. **Status: `PROMOTED_TARGET_SIGNAL_WITH_KNOWN_CROSS_UNIT_PRECHECK_DEBT`** (batch `...balanced-combo-014`, heldout 0.9200000167, margin 0.0257, final 0.812). **Ключевой архитектурный вывод:** долг `no_pre_checks` — это **cross-unit trace-требование**, принадлежащее `ProjectWikiQueryPlanningFlow` + `WikiTraceAggregator`, а НЕ этому юниту (label-only precheck-строки не чинили: батчи 008–012 ≤0.6, 012 обвал до 0.0; diverse/binary дошли до 0.7 в 018 но регрессировали в 019; near-correct query-plan-proof в 020 = 0.52). WikiContextRoutingUnit должен *потреблять* верифицированный query-план, а не промоутиться повтором no-precheck строк. **full wiki done: false.**
- **Unit 9 — `WikiAnswerComposer`.** Отв.: собрать проектно-локальный ответ с точными цитатами, uncertainty, missing-evidence. Min: answer_completeness ≥0.88, exact_evidence_accuracy ≥0.98, hallucination ==0, partial_trace_disclosure ≥0.95. Паттерн — **compact signal-token** chosen-строка (`GOOD_PROJECT_WIKI_ANSWER` + PROJECT_ID/EVIDENCE_ID(token-safe)/LOCATION/PARAMETERS/RETURNS/CALLERS/CALLEES/PROJECT_BOUNDARY_SAME_PROJECT/VALUE_FLOW/IMPACT_PATH/ANSWER_PERMISSION_ALLOWED); SP-расширение добавляет PROCEDURE_IDENTITY/PROCEDURE_NAME/STORED_PROCEDURE/SQL_PARAMETERS/SQL_INPUT_CONTRACT/RESULT_SHAPE/OUTPUT_CONTRACT/TABLE_SIDE_EFFECTS/PERSISTENCE_IMPACT/DOTNET_CALLERS/CALLER_VALUE_FLOW/MISSING_EVIDENCE_CHECK. Grounding-gate принимает token-safe evidence id == raw id (позволяет компактность). Rejected-каталог: missing_evidence_id, missing_callers_and_callees, missing_parameters_and_returns, single_line_summary_only, hallucinated_behavior, wrong_project_evidence, procedure_name_only, missing_sql_parameters, missing_result_shape, missing_table_side_effects, missing_dotnet_callers (каждый с `[WRONG: ...]`). **Status: general PROMOTED (heldout 1.0); SP answer-composition PROMOTED_MILESTONE до batch-036 (heldout 0.98)**; остаточный **`procedure_name_only ~0.9` с одним negative-margin held-out row — НЕ закрыт**. Failures: raw evidence-id в chosen ослабляет DPO-разделение (batch-004 heldout 0.74); weak-type focus на повреждённом checkpoint (batch-006 heldout 0.52); verbose full-text SP-ответы (batch-011 heldout 0.66) → чинит compact signal-token. **full wiki done: false.**
- **Unit 10 — `ProjectAnswerVerifier`.** Отв.: верифицировать корректность ответа, цитаты, project boundary, abstention. Min: hallucination_catch 1.0, wrong_project_catch 1.0, stale_version_catch ≥0.98, unsupported_answer_catch ≥0.98. **Status: `PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST`** (batch `project-answer-verifier-signal-promotion-confirmation-batch-019`, dpo_heldout 0.9800000191, margin 0.01749, final 1.0, cpu 132875ms, ram 75.27MB, manifest emitted). Remediation-путь: восстанавливать из batch-012 (не из обвалившегося 016) → 017 (0.96) → 018 mixed (0.98) → 019 promotion (0.98). Failures: plain «passed/warnings» строки слишком слабы, прячут false-positive (batch-001 heldout 0.30); focused weak-mix обвалил wrong-project/abstention дискриминацию (batch-009 heldout 0.24). Residual watch: `rejects_valid_answer` 0.9 с одним negative per-row margin — проходит per-type gate, но остаётся в real-project QA. **full wiki done: false.**
- **Unit 11 — `WikiTraceAggregator`.** Отв.: писать `WikiQueryTrace`/`WikiUnitTrace`, вскрывать слабые юниты, форсировать remediation вместо слепого продолжения; **владеет cross-unit sequence check** (query-planning верифицирует project/version/evidence prechecks ДО context retrieval; evidence analysis ДО answer composition). Min: unit_trace_coverage 1.0, failed_unit_has_remediation 1.0, ten_batch_gap_analysis 1.0, cross_unit_sequence_accuracy ≥0.95. Chosen несёт failed_units[]/remediation[]/stop_blind_training/query_pass_rate и сохраняет порядок юнитов; для `no_prechecks` не сваливает вину на WikiContextRoutingUnit, а называет owner’ов = QueryPlanningFlow + TraceAggregator. Rejected: ignores_failed_unit / continues_blind_training / missing_remediation / context_before_query_plan_verified / wrong_owner_for_no_prechecks. **Status: `PROMOTED_TARGET_SIGNAL_WITH_CONTEXT_SEQUENCE_CONFIRMATION`** (batch `wiki-trace-aggregator-context-before-confirmation-022`, heldout 1.0, margin 0.0165, final 1.0) + **`REAL_PROJECT_TRACE_VALIDATION_GATE_PASSED 2026-05-22`** (5 проектов / 616 evidence / 6393 вопроса / entity_impact_trace 1.0). Failures: короткие target-signal labels реверсировали failed-trace строки; broad all-unit vocabulary → recency/type collapse; batch-021 прошёл численно но через fallback `incomplete_trace_summary` (не accepted), только batch-022 с точным `[WRONG: context_before_query_plan_verified ...]` подтверждает. **full wiki done: false.** → Это тот самый юнит, на который STATE.json `next_action` переносит текущую query-planning-030 ремедиацию.

### D. Что это меняет для плана расширений (host = `wiki-context-hook.exe`) + что осталось

- **Честная позиция сборки:** 11 юнитов обучены до per-unit PROMOTED/PROMOTED_MILESTONE (с 2 real-project validation gate’ами passed), НО 6-сессионный build-конвейер стоит на SESSION-01, SESSION-02..06 = NOT_STARTED, и интегрированный Mode-2 wiki-продукт **`full wiki done: false` во всех 11 юнитах и во всех state-ключах**. Текущий единственный живой блокер: query-planning `answer_without_intermediate_analysis` = 0.6 (<0.75) → remediation перенесён на WikiTraceAggregator (cross-unit порядок QueryPlanning→ContextRouting→AnswerComposer, блок «answer-before-evidence-analysis»).
- **Для плана «продолжить extensions»:** финальный host-путь (`wiki-context-hook.exe` → 42.ProjectWikiDomain оркестратор → ответ в VS/VS Code) опирается на `ProjectLocalWikiOrchestrator` с фиксированной последовательностью `ProjectWikiQueryPlanningFlow → WikiContextRetriever → ProjectLocalWikiModelUnit → ProjectAnswerVerifier` (SESSION-06). Пока эта последовательность не пройдёт integrated-gate (а не только per-unit), extension отдаёт per-unit-обученную, но не end-to-end-принятую wiki. Base model (session-01-batch-003) + 12/12 ProjectWikiDomain тестов — уже зелёные, это надёжная стартовая точка.
- **Осталось на passes 4–9** (без изменений к плану pass 2 + уточнения): `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` (#18, ~280KB, тело); тела `reports/*` (#46/#47, 100–124KB — какие ещё юниты/сценарии оркестратор реально возвращает); корпуса v1–v7 (#36–#42) на уникальность против v8, `REAL-CHAT-CHAINS-v2` (#44), `EPISODES-v1` (#43); распаковка 4 zip (#34, #49–#51: drawio, wiki-sql-phase2 v1/v2, wiki-training-language v1); `tools/Invoke-ProjectWikiPhaseReport.ps1` (#48) gate-логика; диаграмма `orchestrator wiki.drawio Negative DTO.xml` (#33). Section 19 (Trigger 1–4 update-процедура), Section 20 (Multi-Language: English promoted, Python/TS/Java/Rust PENDING), Section 21 (authority) из #16 — прочитаны на pass 2/3, тело update-триггеров можно углубить на pass 4.

**Честная строка покрытия pass 3 (кумулятивно):** глубоко разобрано на pass 3 — 8 файлов (#20, #21, #22, #23, #24, #25 тела; #26 STATE.json все 574 строки; #16 Section 18 юниты 6–11). С учётом pass 1–2 кумулятивно **≈29/51 файла** прочитаны полностью. НЕ трогалось: #18 (280KB), reports #46/#47 (тела), корпуса #35–#45 (кроме v8-head + подсчётов pass 1), 4 zip #34/#49–#51, tools #48, diagram #33. Ни один файл не изменён (read-only соблюдён); мойибейк нигде не правился.

---

## Pass 4/10 — ORCHESTRATION-EXTRACTION-PROTOCOL-v2 (#18, ~280KB)

**Покрытие pass 4 (честно):** прочитано глубоко всё тело файла (2600 строк / 280 806 байт): строки 1–251 (ГЛАВНОЕ ПРАВИЛО + ШАГ 1–3 механика SESSION_STATE + начало bug-каталога), 676–2600 (ШАГ 4–10, все layer-паттерны, SIMULATION-INVARIANTS, Forensic Mode, форматы вывода, анти-паттерны, все pattern-библиотеки). Средний блок ШАГ 3 (строки 126–676) — это **per-FLOW таблица документных багов** FLOW-01…FLOW-42 + ADAPTER-W1 (~40 строк) плюс ~20 определений новых типов багов: тело прочитано на строках 147–251, репрезентативно + через grep покрыты строки 167–215 (все ключевые FLOW-ряды) и все ~20 заголовков bug-типов (строки 229–585). Единственное, что НЕ прочитано дословно построчно — полный текст каждого отдельного FLOW-ряда в диапазоне 251–676 (это повторяющийся форензик-каталог, не таксономия/метод/output-shape). Файл не изменялся (read-only), мойибейк не правился, append-only в RAG соблюдён.

### A. Назначение протокола (PURPOSE) и его роль как «моста»

Протокол отвечает на один вопрос: **«как цель пользователя превращается в контролируемый, проверяемый путь к работающему результату»** — а НЕ «что входит в план» (строки 5–6). Это ключ для шага (4) целевого флоу расширений (task → все стадии оркестрации → work-plan): протокол — это **мост между сырыми чат/эпизод-корпусами и обученной моделью оркестрации**. Он берёт пакет документов (ZIP, md-файлы, сессии) и извлекает оркестрацию как человекочитаемый маршрут решения задачи.

**Первичный выход (PRIMARY) — оркестрация в форме `ЗАЧЕМ → КАК ВЫГЛЯДИТ → КАК УСТРОЕНО → В КАКОМ ПОРЯДКЕ ДЕЛАЛИ`** (строки 20–22). Всё остальное (расширение протокола, каталоги багов) — SECONDARY, пишется ПОСЛЕ оркестрации, не вместо. Зафиксированная ошибка FLOW-36/FLOW-38: написали расширение протокола без оркестрации → пользователь получил «анализ без результата» (строки 33–34). Для Тип E (training-data-пакет) допускается частичная оркестрация с явной пометкой недоступного.

### B. Входы (INPUTS) — что протокол цитирует

Строка 10 (`**Источники:**`) прямо называет корпуса, которые протокол превращает в обучающий материал:
- `REAL-CHAT-ORCHESTRATION-CHAINS-v2.md` (#44 в этой же папке) — реальные цепочки чата;
- `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` (#41, 1.8 МБ, финальный v8 из серии v1–v8) — сценарии из 1000 эпизодов;
- `ORCHESTRATION-EXTRACTION-PROTOCOL-v1.md` (предок) + `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1.md`;
- разборы FLOW-16 → FLOW-24.

Дополнительные источники, встроенные в тело: SIMULATION-INVARIANTS выведены из **20 высокоприоритетных сессий `claude-sessions/Uncategorized/` (май 2026)** (строка 1542); блок «НОВЫЕ ПАТТЕРНЫ ИЗ КОРПУСА (батчи 01-07)» — из `SESSION-ANALYSIS.md` части 25-35, **63 новых паттерна к 56 существующим, добавлено 2026-05-28** (строка 2552); Best Practices — из папки `claude-sessions/grok orchestrator` (файлы 27,33,37,38,39,41,42).

### C. Метод / стадии — 10 ШАГов (это и есть «все стадии оркестрации»)

| ШАГ | Что делает | Ключевые правила |
|-----|-----------|-----------------|
| **1. Приём + классификация** | 9 типов источников (USER_PROMPT, INPUT_FILES, PRODUCED_FILES, STATE_FILE, CANONICAL_MERGED_DOC, PLAN_FILE, VALIDATION_FILE, RAW_SESSION, GENERATED_OUTPUT) | Порядок доверия убыв.: текущая инструкция → указанный протокол → state/canonical → produced → input → сессия → прежние сводки. Найти «единственный источник правды» (`FLOW{N}_P0_PLAN.md`) и прочесть целиком до остального |
| **2. Сканирование структуры** | Выписать ровно 4: Задача / Что уже есть (цифры, диапазоны) / Что добавляется (конкретно F631–F640, не «10 фабрик») / Фазы | Тест готовности: сформулировать «Мы строим X который позволяет Y делать Z». Не выходит → оркестрация не понята, стоп |
| **3. Синхронизация с состоянием** | RENUMBER_MAP; **SESSION_STATE многоблочный — читать ПОСЛЕДНИЙ блок «Post FLOW-N», не заголовок файла** (append-only накапливает drift) | Алгоритм глубины стейла: N_last − N_header; ≥2 = cascading stale header → consolidation pass по всем 7 файлам. Здесь же весь per-FLOW bug-каталог |
| **4. Глубокое чтение по типам** | P0_PLAN (6 частей: NO-CODE / RENUMBER MAP / PHASE BREAKDOWN / VALIDATION / EXAMPLES / RAG), ENGINE_ARCHITECTURE (зачем+что+куда+FABRIC), SESSION_STATE, TASK_TYPES (ENTRY+PURPOSE → user-сценарий; MACHINE/FREEDOM → гарантии), BFA (CRITICAL/HIGH) | Multi-Vector Input: domain-delta vs meta-system-upgrade vs go-to-market — описывать раздельно, для оркестрации только domain-delta. Кол-во input-файлов ≠ кол-во векторов |
| **5. Красная нить + 3 слоя** | Слой1 (что хочет юзер) → Слой2 МОСТ (A: таблица зон + принцип деления; B: зона→F-диапазон; C: DD «бизнес-требование»→архитектурное правило) → Слой3 (task types, фабрики, BFA). Прыжок через слой = неверно | + 12 layer-паттернов (см. §E) + BUILD TIME vs RUNTIME (INVENTORY→SYNTHESIS→JUDGMENT, не «AI генерирует код») |
| **6. Распознавание типа флоу** | Большая таблица ~30 типов флоу, у каждого «Признаки» + «Дополнительно» (обязательные доп-проверки) | См. §E |
| **7. Границы задачи** | Что просит задача / что НЕ входит / что подтвердить до старта | — |
| **8. Синтез** | Нарративный порядок `ЗАЧЕМ→КАК ВЫГЛЯДИТ→КАК УСТРОЕНО→В КАКОМ ПОРЯДКЕ`. Убрать жаргон, оставить принципы (idempotent/resumable/fabric-first/human gate) | Рабочие глаголы: открыть/прочитать/выписать/проверить/сравнить/заблокировать/вернуть. Запрещены: «проанализировать», «применить skill», «валидировать план» без объекта/результата |
| **9. Формат вывода** | Markdown-шаблон: Что строим / Что даёт / Главная новая возможность / Аналогия / Архитектурный мост (зоны+DD) / Как выглядит для юзера / BUILD TIME (фазы P0–P7) / RUNTIME / Основной бизнес-процесс / **Железные гарантии** (MACHINE от позитива) / Новые архетипы / Итоговое состояние (F/T/CF/SK) / **«Что поймал протокол»** (обязательная) / Что дальше | «Железные гарантии» = переформулировка MACHINE-запретов в обещания системы («деньги не спишутся дважды — за счёт idempotency key на уровне БД») |
| **10. Проверка перед отправкой** | Главный вопрос: «Оркестрация по этапам построена и ничего не упускает?» Чеклист полноты цепочки (замкнута? каждый этап честен? гарантии настоящие? картина полная?) + **симуляция** (мысленно проиграть триггер→результат) + чеклист отклонения | Отклонить если: начинается с номеров/файлов; нельзя написать «Мы строим X»; happy-path без сбоя/retry/timeout; гарантия молча обходится |

### D. Runtime-инварианты (SIMULATION PATTERNS) — «невидимы в статических flow-документах»

Это стадии, которые проявляются только при SIMULATION/Phase A execution (строки 1540–1786). Прямо описывают build-time конвейер оркестрации:

- **5-этапный pipeline разработки флоу (MACHINE, строго sequential):** `FLOW-PREP (19 раундов R0–R19) → SIMULATION (35 раундов, gap-классы H/J/K/L/M/N/A/B/G) → GAP-PREP → GAP-TRANSLATE → Claude Code`. Нельзя параллелить/пропускать. FLOW-PREP раунды включают R1=Graph seeding, R2=NODE grades, **R11=Genesis triples**, R14=Learning handoff.
- **H0 CADENCE (MACHINE):** одна сессия = один файл; после каждого раунда `present → ⛔ STOP → ждать "continue"`. Имена `FLOW-{XX}-{STAGE}-R{N}.md`.
- **PATTERN TRANSFER RATES:** structural паттерны (SETNX, DNA-8, MACHINE ordering) ~100% cross-flow; FLOW-12 subscriptions 85%; средний domain 50–70%; meta-flow FLOW-34 33%; domain-specific ~0% (правильно). → При планировании нового флоу сначала брать structural (100%), потом близкие домены.
- **Архетип-реестр эволюция:** 20 (через FLOW-13) → 21 → 22 → 23 → **33** (после FLOW-20). Новый архетип = обновить `ContractArchetype` enum + `tierMap` в `feedback.handler.ts` + SIMULATION re-run.

### E. Таксономия — точные правила и перечисления (это то, что модель оркестрации должна воспроизводить)

**8 ENTRY TYPES (полная таксономия, ранее было 4–5):** `HTTP` (sync REST) | `EVENT` (async consumer) | `DUAL` (HTTP+Event) | `INLINE` | `INLINE-PURE` (без side-effects) | `SCHEDULED` (cron/timer) | `REQUEST_RESPONSE` (sync, не REST) | `HYBRID_SYNC_ASYNC` (sync accept + async processing).

**4 MACHINE PATTERN TYPES:** `constant` (FREEDOM config не меняет) | `formula` (детерминированно из inputs) | `ordering` (порядок шагов фиксирован) | `state_machine` (конечный автомат с MACHINE-переходами).

**NODE PRIMITIVE — основной примитив цикла** (ядро runtime-оркестрации, «The Missing Primitive», строки 1626–1644, 2432–2441): поля `structure` (object, I/O+events+deps) / `intent` (string, цель узла) / `constraints` (array, **НИКОГДА пустой** → `CONVERGENCE_MISSING_CONSTRAINTS`, step-specific, НЕ generic DNA rules) / `quality` (number, ≥0.85 = MACHINE). Вердикты: `LEAF` (терминальный → Cycle 4 Executor) | `EXPAND` (рекурсивный, порождает sub-flow, max depth = 3 MACHINE). **NODE создаётся через multi-model convergence ДО генерации кода.**

**4-Cycle Chain Pipeline (build-time «task → work-plan»):** Cycle 1 Planner (Intent→Plan, PlannerHandler+Plan Reviewer) → Cycle 2 Convergence (Plan Step→NODE, 3-model parallel, blind judging, challenger isolation) → Cycle 3 Depth Decision (LEAF/EXPAND ≤3) → Cycle 4 Executor (Leaf NODE→AF pipeline) → Cycle 5 Meta (Meta-Arbiter на grade < 0.85). **3 teaching channels:** `CODE_REASONING` | `DESIGN_REASONING` | `CONVERGENCE_REASONING`.

**12 layer-паттернов (ШАГ 5) — каждый со своим набором обязательных проверок:** Engine Primitive/Cross-Cutting (backward-compat матрица + zero-migration) · Governance/Cross-Flow Gate (cross-flow protection matrix, static-before-AI ordering — AI только повышает severity, escape-hatch constraints, blast radius chain, durable arbitration) · Self-Learning Infrastructure (замкнутость feedback loop, arm-structure invariant, promotion ladder GENERATED→INJECTED→MINIMAL→CORE, human gate) · Design Intelligence (mandatory IR pipeline DesignIR→ScreenSemanticsIR→SystemModelIR, evidence-anchored confidence = evidence×0.4+quality×0.4+llm×0.2, «don't know» > wrong) · Meta-Flow/Self-Extension (idempotent bootstrap state machine, two-layer GraphRAG seeding, **5-Arbiter Consensus Coverage→Security→DNA→Testing→BFA** — все 5, порядок MACHINE, ContextPack TTL, backward-compat fence) · Multi-Language/Canonical Spec (canonical-before-variant, P0→P1∥P2→P3→P4, cross-language DNA matrix из 5 invariants) · Infra Bug Fix/SDK Hygiene (Phase 0 = 0 артефактов, bootstrap order Secrets→Config→DB→Queue→AI→RAG→Flow) · Self-Implementing Engine (Two-Track A человек / B движок, gradual autonomy 0→30→70→90%, meta-loop human gate) · **LLM Dynamic Flow / 9-node (см. §F)**.

**Таблица типов флоу (ШАГ 6, ~30 типов):** Длинная сессия, Адаптер/порт, Multi-Language, Мульти-фича, Системный, Marketplace/финансовый, Template/App-Store Marketplace, Миграция+новый FLOW, Регулируемая платформа, No-code/формы, Визуальный редактор/CMS, Расширение флоу, Обучение/тьютор, Governance Gate, Meta-Flow, Proto-Meta-Flow (FLOW-26), Engine Primitive, Self-Learning, Design Intelligence, Infra Bug Fix, Engine Self-Awareness (FLOW-37), Self-Implementing (FLOW-36), Learning Loop Completion (FLOW-38), Flow Design Teaching/Local Model Curriculum (FLOW-39), Domain Business Flow post-learning (FLOW-01-new), Pre-Canonical Domain Flow (FLOW-02..09), Wave -1 Infrastructure/Client Push (FLOW-40), Platform Adaptation+Heuristic (FLOW-42), Multi-Tenant App Factory (FLOW-15), Visual Flow Authoring (FLOW-18), DevOps Control Plane (FLOW-19), Platform Adaptation/Ecosystem Adapter (FLOW-41). У каждого — точные «Признаки» (например `implementationMode: hybrid` + `bootstrapBoundary: Phase A only`) и обязательные доп-проверки.

**MACHINE / FREEDOM split** (сквозной): MACHINE = что нельзя менять (consent-гейт, server-side grading, append-only ledger, tenantId, bootstrap order); FREEDOM = ES-конфиги (длительность, пороги, модель AI, quiet hours). MACHINE-инвариант, защищённый только текстом плана (без CF-правила REJECT) = «text-only constraint», архитектурное наблюдение.

**Каталог документных багов (ШАГ 3, ~20 типов):** Session-Isolation Renumbering (+ Reverse + Bidirectional + Differential по артефактам), Document/Total Alias Substitution, Incomplete Merge Packaging, Phase Ordering Deviation, Stale/Cascading Stale Header, Premature Manual Implementation, Merge Plan State Label Mislabel, Interface Naming Inconsistency, Partial CRITICAL CORRECTION, Phase Numbering Inconsistency, PHANTOM Archetype, File Naming Cross-Session Confusion, Fabricated Names. Каждый — с алгоритмом обнаружения + примером FLOW. В Forensic Mode (F2) проверка всего каталога обязательна, даже если «не найдено».

### F. Как протокол производит обучающий материал для модели оркестрации (связь со step 4 плана)

**Главная связка — 9-Node LLM Dynamic Flow (строки 1282–1297):** это буквально runtime-пайплайн модели оркестрации, «task → все стадии → work-plan»:
`Node1 Domain Splitter (user prompt → массив domain labels) → Node2 Domain Translator (fragment+label → formal domain statement) → Node3 Scheme Planner (все statements → ordered/parallel work pieces) → Node4 Domain Processor → Node5 Applier → Node6 Result Translator → Node7 Arbiter Selector → Node8 Arbiter Units (pass/concern/reprocess) → Node9 Formatter (approved results → один связный ответ)`. Свойства-инварианты: только approved-результаты доходят до Formatter; арбитры возвращают ТОЛЬКО проваленный фрагмент; параллельные ветки до merge; wait state перед merge; recovery route. Именно этот граф — цель обучения step 4 (пользовательский промпт → все стадии оркестрации → work-plan).

**Формы обучающих строк (output-row / DPO shapes), которые протокол фиксирует:**
- **Pipeline Data Contracts (строки 2400–2406):** Cycle 1 Planner получает `userIntent + constraints[]`, обязан произвести plan-steps (abstract, domain-specific, one responsibility, no tech names) — критично: шаги должны вынуждать 3 модели делать РАЗНЫЕ выборы, иначе convergence на Round 1 → **useless DPO**. Cycle 2 получает один plan-step, производит NODE `{structure, intent, step-specific constraints, quality target}` — constraints STEP-SPECIFIC, generic = немедленная convergence.
- **«Каждое planning decision = DPO triple»** — G4 Planning-DPO gap (строка 2449): в текущем поведении planning-решения не captured; целевое — каждое решение становится DPO-тройкой.
- **DPO Triple Type Mismatch (E-11, критический баг, FLOW-12, строка 1585):** `determineDpoQuality` ждёт `chosen: {code: string}`, а `feedback.handler` пишет `chosen: string` → все тройки INVALID навсегда. То есть корректная форма chosen-строки = объект `{code}`, не голая строка. (Согласуется с pass-3 находкой Unit 8/K1: `modelComparison.chosen.model` правильно, `chosen.code` — stale.)
- **Edge Integrity Rule (строки 1299–1309):** «No package may train a node as if it receives source-document lines at runtime» — каждая A/B-пара должна иметь runtime-shape (как реальный запрос юзера), НЕ document-shape (строки плана). Generic B-ответ вида «The user intent responsibility preserves…» = training-data defect (обучит pipeline принимать markdown как user input).
- **Genesis triples** (R11 FLOW-PREP) и **LEARNING-HANDOFF.json** (distilledRules + dpoTriples + shadowRunSummary + calibrationRecords + forNextFlowPlanning) — переносят обученный сигнал между флоу.

**Build-time дисциплина, которую протокол закрепляет для качества сигнала:** 3 обязательных прохода Forensic Mode (Normal → Bug-hunting → Grounding на реальных файлах на диске, не транскрипт); реальные документы > транскрипт; «XIIGen is a Compiler» — стадия с неправильной формой output молча corrupts всё downstream (перед изменением: input shape? output shape? кто потребляет? что потребителю нужно? неизвестно → halt).

### G. Конкретные unit/flow/gate имена, встреченные в #18 (сверх pass 1-3)

- **7 canonical merged файлов** (все флоу аккумулируются, gapless numbering = BUILD FAILURE): `SKILLS_FACTORY_RAG_MERGED` / `UNIFIED_SOURCE_INDEX_MERGED` / `V62_BFA_STRESS_TEST_MERGED` / `TASK_TYPES_CATALOG_MERGED` / `ENGINE_ARCHITECTURE_MERGED` / `SESSION_STATE_MERGE` / `MASTER_EXECUTION_PLAN_MERGED`.
- **Полный формат Task Type Contract (12 полей, обязателен):** `TASK TYPE / ARCHETYPE / ENTRY / PURPOSE / DISTINCT FROM / FACTORY DEPENDENCIES (F-N via CreateAsync) / FABRIC RESOLUTION (DATABASE/QUEUE/AI/RAG FABRIC) / AF CONFIGURATION (AF-1 генерирует, AF-6 review, AF-9 judge) / BFA VALIDATION (CF-N) / MACHINE-FREEDOM / IRON RULES / QUALITY GATES`. One-line stub = немедленное отклонение плана.
- **Gates/правила:** V9 gate (teaching health, BUILD_FAILURE), FC-1..FC-31 plan-review gates (FC валидируются в dedicated round, не inline), Gate 0i «DIG IN DOCS FIRST», Moderation GATE (approved/rejected/human-review), Negative Bypass Tests (каждое MACHINE rule обязано иметь bypass-test), CF-POLICY-01 (knowledgeScope default = PRIVATE), GR-001 (Zero Tech Debt), ARCHITECT Rules A-1..A-4.
- **T58 Self-Extension Contract** (META archetype): принимает capability description → производит factory interfaces + AF configs + BFA rules + iron rules + quality gates (замыкает петлю self-building engine). **AWAITING_DECISION** lifecycle-state + 5 mandatory design questions.
- **Живые баги AF-run:** Mock Mode = 1.00 (`AI_PROVIDER=MOCK` вместо `AI_GEMINI/OPENAI/JUDGE_PROVIDER`); Cycle 1→2 handoff не инжектирован; CONTEXT_INSUFFICIENT loop.

### H. Что осталось в #18 для последующих passes

- Полный дословный текст каждого отдельного FLOW-ряда в per-FLOW bug-каталоге ШАГ 3 (строки ~251–585 подряд): FLOW-31…FLOW-42 определения новых bug-типов прочитаны через заголовки+grep, но если понадобится точная форензик-цитата по конкретному FLOW — дочитать построчно (это повтор одного форензик-шаблона, для метода/таксономии/output-shape не нужен).
- Перекрёстная сверка таксономии #18 против реальных корпусов `REAL-CHAT-ORCHESTRATION-CHAINS-v2.md` (#44), `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` (#41) и v1-v7 (#36-#42) — планировалось на passes 5-9 (проверить, что 9-node/4-cycle/8-entry-типы совпадают с тем, что реально размечено в корпусах и в `reports/*` #46/#47).

**RAG-файл (обновлён append-only):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`

---

## Pass 5/10 — reports: actual orchestrator outputs (#46/#47)

**Что прочитано в этом проходе (deep-read целиком):**
- `reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md` (#46, 1429 строк, ~124KB) — 19 секций.
- `reports/ORCHESTRATOR-PROMPT-ACTUAL-OUTPUTS-v1.md` (#47, 1264 строки, ~100KB) — 19 секций.

Оба файла — это **дословный дамп реальных ответов оркестратора** на founding-промпты из `orchestrator prompts.txt` (те же промпты, что перечислены в pass-1/2). Это самое близкое к «что расширение реально возвращает пользователю в чат». Никакой мета-аналитики/легенды/summary в конце ни у одного файла нет — только `prompt -> фактический ответ`, 19 раз. Каждый из 19 промптов вернул `Status: ORCHESTRATED` (0 падений, 0 отказов на входе).

### A. Что это за файлы и в чём разница между #46 и #47

Это **два прогона одного и того же оркестратора** по одному и тому же списку из 19 founding-промптов, различающиеся конфигом входа/evidence:

| | #46 EXACT-PROMPTS-ACTUAL-RESULTS | #47 PROMPT-ACTUAL-OUTPUTS |
|---|---|---|
| Заголовки секций | сырые (`01. Original prompt line 1`, `Later usage 1`) | человеческие (`01. Create skill set for new project`, `03. Figma to whole system`) |
| Текст промптов | **как в исходном файле**: с опечатками (`neutrile`, `callories`), с mojibake em-dash в промптах 17/18, многострочные | **очищенные**: опечатки исправлены (`neutral`), mojibake убран, многострочные схлопнуты в один параграф |
| Блок «Что я вижу в проекте» | **8 строк** (5 базовых + 3 строки `wiki evidence says` для GenieDynamicApp) | **5 строк** (те же 5 базовых, **без** трёх `wiki evidence says`) |
| Финальный ответ (форма) | идентичная 7-секционная (см. B) | идентичная 7-секционная |

**Ключевой вывод из разницы блока evidence:** блок «Что я вижу в проекте» **не зависит от промпта** — он одинаков во всех 19 секциях внутри каждого файла. В #46 он содержит 3 дополнительные строки локальной wiki-evidence (`# GenieDynamicApp local wiki evidence`, `UI surface: content generation flow screen`, `Client integration: Angular app should call backend job endpoints`), в #47 их нет. То есть **project-evidence — это фиксированный скан фиксированного корпуса (GenieDynamicApp + GenieDynamicServer), а не пер-промптный RAG**; #46 = прогон с загруженной локальной wiki, #47 = прогон без неё. Это прямое подтверждение (согласуется с pass-1/3): `ProjectUnderstandingUnit` отдаёт статический project-scan, а не свежую пер-запросную выборку.

### B. Форма реального оркестрированного ответа (7 секций) — то, что расширение реально возвращает

Каждый ответ жёстко шаблонный, ровно 7 секций (русские литералы — дословно из вывода):

1. **`Status: ORCHESTRATED`** — верхний статус (всегда ORCHESTRATED на всех 19).
2. **`Я понял задачу так:`** — эхо промпта, обрезанное до ~180 символов + `...` (для длинных). Это НЕ переформулировка — буквальный echo/truncate входа.
3. **`Что я вижу в проекте:`** — фиксированный project-evidence блок (8 строк в #46 / 5 в #47), одинаковый для всех промптов. Продукт `ProjectUnderstandingUnit`.
4. **`Как вести работу:`** — нумерованный маршрут: шаг 1 «подтвердить текущую систему (active repo, linked repos, UI/backend/storage/flow surfaces)», шаг 2 «разложить задачу на рабочие части + reuse before rewrite», затем **по одному шагу на каждый фрагмент промпта** вида `Для части "<фрагмент>" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence`, затем `Собрать общий маршрут ... что можно отдать executor-у`, затем `Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом`. Продукт `HumanRouteComposer`.
5. **`Какие вопросы XIIGen должен задать project wiki/RAG:`** — 3 фиксированных вопроса (existing screens/routes/services/flows/jobs/storage; which items grounded in code vs need external input; where reuse before new implementation) + по одному `What project evidence is needed before planning: <фрагмент>?` на каждый фрагмент. Продукт `EvidencePlanner` (+ `WikiContextRoutingUnit`, когда добавлен).
6. **`Что нельзя замазывать:`** — либо `Критичных missing evidence на этом уровне не найдено.`, либо список `External input required before final implementation detail: <фрагмент>.` Продукт `VerificationAbstentionUnit` / `MissingEvidenceAbstentionUnit`.
7. **`Внутри XIIGen для этого используются:`** — список реально вызванных внутренних units (см. C).

**Как строятся «части» (fragment splitter):** входной промпт режется на фрагменты **наивным разбиением по пунктуации** (запятые/точки/стрелки). Результат часто бессмысленно-мелкий: отдельными «частями» становятся `queues`, `prisma`, `redis`, `music`, `debug`, `audio`, `CloudWatch`, `security`, `optimizations`, `gremlin)`. То есть это **не** семантическое доменное разбиение — это текстовый split. Это критично для понимания «что реально работает» (см. E).

### C. Реестр реальных внутренних units (кто что производит)

**Базовые 7 units — присутствуют во ВСЕХ 19 ответах в обоих файлах:**

| # | Unit | Что производит (по позиции в ответе) |
|---|---|---|
| 1 | `TaskOrchestrationUnit` | верхний оркестратор-entrypoint (даёт `Status: ORCHESTRATED`, fan-out к остальным) |
| 2 | `ProjectUnderstandingUnit` | секция «Что я вижу в проекте» (статический project/wiki scan) |
| 3 | `EvidencePlanner` | секция «Какие вопросы задать project wiki/RAG» + пер-фрагментные evidence-вопросы |
| 4 | `UnitSelector` | мета-выбор, какие units включить в маршрут |
| 5 | `HumanRouteComposer` | секция «Как вести работу» (нумерованный маршрут) |
| 6 | `VerificationAbstentionUnit` | секция «Что нельзя замазывать» (verify + abstain) |
| 7 | `CrossStackBusinessFlowGraphUnit` | кросс-стек (client+server) бизнес-флоу-граф |

**Условные units — добавляются по содержанию промпта:**

| Unit | Триггер | На каких промптах (#47) | На каких (#46) |
|---|---|---|---|
| `MissingEvidenceAbstentionUnit` | когда в секции 6 сработал флаг `External input required` (attached files / внешние сервисы / генерация медиа) | 02, 07, 16, 18 | 02, 03, 04, 05, 07, 16, 17, 18 |
| `WikiContextRoutingUnit` | когда промпт про RAG / graph / wiki / skills-library / context-routing | 15, 16, 17, 19 | 15, 16, 17, 19 |

Разница набора `MissingEvidenceAbstentionUnit` между #46 и #47 объясняется чувствительностью флага `External input required` к точной фрагментации: очищенные промпты 03/04/05 в #47 больше не дают срабатывания на ведущем фрагменте (в #46 они цеплялись за `1. User enters figma` / `figma` / `to the user ... platform`). То есть **флаг external-input — эвристика, зависящая от разбиения и ключевых слов (figma, attached, media, audio)**, а не от смысла.

Первое появление `WikiContextRoutingUnit`: #46 строка 1067 (промпт 15 «graph and rag capabilities»).

### D. Прямое соответствие Bootstrap-units из llm_mvp_core (важно для плана)

Названные в отчётах units — это тот же **Bootstrap ILogicalUnit family (TD1)** из `42.ProjectWikiDomain`, который CLAUDE.md описывает как «6 Bootstrap ILogicalUnit implementations, оборачивающих исходную статическую orchestration-логику ... DPO training starting point». Соответствие почти 1:1:

| Unit в отчётах (#46/#47) | Bootstrap unit в llm_mvp_core (42.ProjectWikiDomain) |
|---|---|
| `TaskOrchestrationUnit` | `BootstrapIntentBuilderUnit` + orchestration-flow entrypoint |
| `ProjectUnderstandingUnit` | `BootstrapProjectScannerUnit` |
| `EvidencePlanner` | `BootstrapEvidencePlannerUnit` |
| `UnitSelector` | `BootstrapUnitSelectorUnit` |
| `HumanRouteComposer` | `BootstrapRouteComposerUnit` |
| `VerificationAbstentionUnit` | `BootstrapOrchestrationVerifierUnit` |

Вывод: **то, что реально возвращает расширение на founding-промпты — это выход Bootstrap-слоя (статическая, до-обучающая логика), а не выход обученной модели.** Именно поэтому ответы детерминированно-шаблонны. Согласуется с CLAUDE.md/pass-3: пока runner не опубликовал веса, флоу обязан отдавать `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` / `BOOTSTRAP_FALLBACK`, а не «обученную локальную модель».

### E. Связь с 9-node LLM Dynamic Flow (#18) и что реально работает vs scaffolded

**9-node flow (#18, pass-4) — это ДИЗАЙН runtime-пайплайна:** `Node1 Domain Splitter -> Node2 Domain Translator -> Node3 Scheme Planner -> Node4 Domain Processor -> Node5 Applier -> Node6 Result Translator -> Node7 Arbiter Selector -> Node8 Arbiter Units -> Node9 Formatter`.

**Что реально исполняется в отчётах — это сильно вырожденная (scaffold) реализация этого дизайна:**

| 9-node дизайн (#18) | Что реально видно в выводе (#46/#47) | Оценка |
|---|---|---|
| Node1 Domain Splitter (prompt -> массив domain labels) | наивный **fragment-splitter по пунктуации** -> фрагменты сырого текста (`queues`, `redis`, `music`) | вырожденный Node1: делит, но НЕ на доменные метки |
| Node2 Domain Translator (fragment+label -> формальный statement) | **отсутствует** — фрагмент используется дословно | НЕ реализован |
| Node3 Scheme Planner (ordered/parallel work pieces) | **отсутствует** — вместо плана единый линейный список «Для части ... найти evidence» | НЕ реализован |
| Node4 Processor / Node5 Applier (multi-model apply) | **отсутствует** — никаких вызовов AI-моделей в выводе | НЕ реализован |
| Node6 Result Translator | секция «Я понял задачу так» = просто echo-truncate | вырожденный |
| Node7/Node8 Arbiter Selector + Arbiter Units (pass/concern/reprocess) | **отсутствует** — арбитража нет; вместо этого эвристика `VerificationAbstentionUnit` (Критичных evidence нет / External input required) | НЕ реализован (только abstain-эвристика) |
| Node9 Formatter (approved results -> один связный ответ) | реализован как **фиксированный 7-секционный шаблон** | реализован как шаблон, но собирает evidence-план, а НЕ результат |

**Что РЕАЛЬНО работает (детерминированно, но работает):**
- Entrypoint принимает произвольный промпт и всегда возвращает структурированный 7-секционный ответ (`ORCHESTRATED`), 19/19 без падений.
- Статический project-evidence scan (фиксированный корпус GenieDynamicApp + GenieDynamicServer).
- Fragment-splitter + генерация пер-фрагментных evidence-вопросов.
- Эвристика external-input/abstention («что нельзя замазывать»).
- Unit-selection + именование вызванных units (7 базовых + 2 условных).

**Что scaffolded / НЕ реально (не выдавать за «фичу»):**
- НЕТ пер-промптного RAG: блок evidence фиксирован (5/8 строк) вне зависимости от промпта.
- НЕТ семантического domain-splitting (только текстовый split по пунктуации).
- НЕТ Node2-Node8: нет доменной трансляции, scheme-планирования, multi-model apply, арбитража, convergence, reprocess-петли.
- НЕТ вызова AI-моделей, DPO, reward/margin в самом выводе — «ответ» это **план сбора evidence**, а не реализованный результат/код.
- Согласуется с pass-1/3: runtime = Bootstrap/BOOTSTRAP_FALLBACK, обученной модели ещё нет.

Связь с **11 wiki-units (#16)**: единицы отчётов — это orchestrator-extension-именование того же Bootstrap-семейства (см. D). 11 wiki-units из #16 — более широкий контракт (`ILogicalUnit`/`ILogicalFlow` уровни), а 7-9 названных здесь units — конкретное подмножество, реально дергаемое founding-промптами через `ProjectWikiOrchestrationFlow` / `TaskOrchestrationUnit`. Полную сверку «11 wiki-units vs units отчётов» имя-в-имя отложить на pass 6-7 (нужен код `42.ProjectWikiDomain` + `15.OrchestratorModel`).

### F. Выводы для плана продолжения VS / VSCode расширений

1. **Контракт ответа известен и стабилен.** Расширение уже умеет: промпт -> `Status: ORCHESTRATED` + 7 секций (понял-так / вижу-в-проекте / как-вести-работу / вопросы-к-RAG / что-нельзя-замазывать / список-units). Это готовый машинно-стабильный формат для отрисовки в панели VS/VSCode (секции/аккордеон; список units — как «trace/why»).
2. **Главный gap для «настоящего» ответа — Node2-Node8.** Чтобы ответ перестал быть evidence-планом и стал результатом, нужно подключить обученный слой поверх Bootstrap (DPO-обученные `IntentBuilder/EvidencePlanner/RouteComposer`) — то, что CLAUDE.md называет заменой Bootstrap-default после публикации весов runner-ом (A-LM-03).
3. **project-evidence должен стать пер-промптным.** Сейчас `ProjectUnderstandingUnit` возвращает фиксированный скан (GenieDynamicApp/Server) — для реального расширения нужен пер-запросный RAG по активному репозиторию пользователя (это же и есть роль `WikiContextRoutingUnit`, пока подключаемого лишь на RAG-промптах).
4. **fragment-splitter — первый кандидат на замену** обучаемым доменным сплиттером (Node1), иначе `queues`/`redis`/`music` как «части» ломают качество downstream.
5. **19 founding-промптов = готовый прогонный набор** для Path A (automated boundary) сценарного покрытия: у каждого известен фактический ответ (baseline), можно проверять регрессии «до/после» подключения обученного слоя. #46 (с wiki) и #47 (без wiki) дают две baseline-конфигурации.

### G. Что осталось на passes 6-9

- **pass 6-7:** сверка кода — `42.ProjectWikiDomain` (Bootstrap units) + `15.OrchestratorModel` (`ProjectWikiOrchestrationFlow`, `ProjectDocumentationPipelineFlowAdapter`) против имён units из отчётов; подтвердить 1:1 маппинг D и точную точку, где `BOOTSTRAP_FALLBACK`/`runner_missing` возвращается.
- **pass 6-9:** сверка таксономии #18 (9-node/4-cycle/8-entry) с реально размеченными корпусами `REAL-CHAT-ORCHESTRATION-CHAINS-v2.md` (#44), `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` (#41) и v1-v7 (#36-#42) — совпадает ли размеченное с реально исполняемым в этих отчётах (гипотеза: корпуса содержат ДИЗАЙН 9-node, отчёты показывают вырожденную Bootstrap-реализацию).
- **pass 8-9:** оставшиеся непрочитанные файлы (из 51) + финальная сборка «continue-extensions» карты: contract ответа (готов) -> gaps (Node2-8, пер-промптный RAG, доменный splitter, обученный слой) -> prompt-baseline (19 промптов x 2 конфига).

**RAG-файл (обновлён append-only):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`


## Pass 6/10 — orchestration training corpora (#35 EPISODES-1000-v8, #44 REAL-CHAT-CHAINS-v2, #43 EPISODES-v1)

Цель прохода: RAG по трём корпусам, которые являются ОБУЧАЮЩИМ МАТЕРИАЛОМ для orchestration-модели (промпт пользователя -> ожидаемая оркестрация первого слоя). Это тот вход, на который ссылается extraction-протокол #18 (pass 4) и который должен заменить вырожденный Bootstrap-вывод, разобранный в pass 5 (#46/#47).

### Честная строка покрытия pass 6
- `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-v1.md` (#43, 22KB, 421 строка) — **прочитан ПОЛНОСТЬЮ** (20 сценариев).
- `REAL-CHAT-ORCHESTRATION-CHAINS-v2.md` (#44, 29KB, 476 строк) — **прочитан ПОЛНОСТЬЮ** (12 реальных цепочек + секция «Что v2 исправляет»).
- `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` (#35, 1.8MB, 20005 строк, 1000 кейсов) — **ВЫБОРОЧНО (sampled, НЕ все 1000)**: полностью прочитан формат/заголовок; прочитаны кейсы 0001–0009, 0500–0503, 0950–0952 (≈15 кейсов); извлечён полный список из 30 доменов через grep по всем 1000 кейсам с подсчётом. Тело оркестрации по доменам подтверждено идентичным на кейсах 0500 vs 0950 (см. каверзу ниже). Я НЕ читал все 1000 кейсов дословно.

### A. Точный ROW/CASE-формат каждого корпуса

**#43 EPISODES-v1 (20 сценариев, seed-корпус, ручной).**
Заголовок файла явно фиксирует контракт: `промпт пользователя -> ожидаемая оркестрация первого слоя`; «Здесь нет кода. Здесь нет implementation documentation. Здесь нет плана исправления XIIGen.» Источник: `translated_files_FINAL.zip` + `translated orchestrator learning sets.zip`.
Формат одного кейса:
```
## N. <название класса задачи>
### Промпт пользователя
```text
<сырой промпт пользователя, 1-3 предложения>
```
### Ожидаемая оркестрация
1. <шаг>
2. <шаг>
... (6–8 нумерованных шагов оркестрации первого слоя)
```
Поля: только два — сырой промпт и нумерованный список ожидаемых шагов. Нет подтверждения/риска/фокуса (это добавлено позже в v8). Шаги — человеческие («сначала не резать код, а построить карту зависимостей»), не код.

**#35 EPISODES-1000-v8 (1000 кейсов, машинно-развёрнутый, АКТИВНЫЙ корпус).**
Заголовок: «Этот файл заменяет v1-v7. Он пересобран из v7 так, чтобы каждый кейс звучал человечески: тип работы, подтверждение, ожидаемый результат, риск и контрольный фокус. Формат каждого кейса: промпт пользователя -> ожидаемая оркестрация.»
Формат одного кейса (ТОЧНАЯ структура, поля — литералы):
```
## Кейс NNNN   (0001..1000)
### Промпт пользователя
```text
У меня ситуация из области «<ДОМЕН>»: уже есть или нужно найти «<ПОДТВЕРЖДЕНИЕ>»,
ожидаемый результат — «<РЕЗУЛЬТАТ>», главный риск — «<РИСК>».
Особый фокус: <ФОКУС>. Построй ожидаемую оркестрацию человеческим языком.
```
### Ожидаемая оркестрация
Оркестратор должен начать с человеческой цели: это кейс из области «<ДОМЕН>», где нельзя
переходить к исполнению без подтверждений и понятного маршрута.
Главное подтверждение: <ПОДТВЕРЖДЕНИЕ>.
Ожидаемый первый результат: <РЕЗУЛЬТАТ>.
Главный риск: <РИСК>.
1. <шаг маршрута>            <-- 3 шага, ФИКСИРОВАНЫ per-домен
2. <шаг маршрута>
3. <шаг маршрута>
Контрольный фокус: <ФОКУС>.
Завершать кейс можно только после проверки, что маршрут понятен человеку, связан с
подтверждениями и имеет следующий безопасный шаг.
```
5 переменных слотов на кейс: `область/домен`, `подтверждение` (что найти/дано), `результат` (первый ожидаемый артефакт), `риск` (главная ошибка = зерно негатива), `фокус` (контрольная точка). Тело из 3 нумерованных шагов — не свободное, а привязано к домену.

**#44 REAL-CHAT-CHAINS-v2 (12 реальных цепочек из настоящих чатов XIIGen).**
Статус: исправление v1 — «Один длинный message больше не притворяется одним prompt; atomic tasks separated». Каждая цепочка = разбор одного реального фрагмента чата на атомарную задачу.
Формат одного кейса (поля — литералы):
```
## N. <название задачи>
Источник чата: `<uuid чата>`.
### Точный фрагмент
```text
<дословный сырой текст из реального чата пользователя, на англ.>
```
### Граница задачи      — что именно этот фрагмент просит сделать
### Не смешивать с       — соседняя задача из того же message, которую нельзя подтягивать (НЕГАТИВ/дистрактор)
### Правильная оркестрация
1..N нумерованные шаги «сверху вниз до рабочих подуровней»
### Результат для человека — что пользователь должен увидеть при правильной оркестрации
```
Ключевое отличие от v1/v8: есть провенанс (uuid чата), дословный сырой текст, и явное поле `Не смешивать с` — это единственный корпус, где негатив назван эксплицитно как соседняя задача.

### B. Счётчики кейсов и домены

| Корпус | Кейсов | Тип | Формат тела | Негатив-сигнал |
|---|---|---|---|---|
| #43 v1 | 20 | ручной seed | 6–8 шагов, свободный | нет (только постановка проблемы) |
| #35 v8 | 1000 | машинно-развёрнут из v7 | 3 шага, ФИКСИРОВАН per-домен | поле «Главный риск» (описание, не rejected-маршрут) |
| #44 chains-v2 | 12 | реальные чаты | N шагов, свободный | поле «Не смешивать с» (соседняя задача) |

**#35 v8 — 30 доменов (`из области «…»`), ~33–34 кейса на домен (проверено grep по 1000 кейсам):**
Первые ~20 доменов = развёртка 20 сценариев v1; последние ~10 — новые «мета»-домены о самом процессе обучения/RAG.
1. архитектура из идеи и диаграмм · 2. Figma в целую систему · 3. Figma в существующую систему · 4. визуальное исправление design-to-code · 5. разворот из-за privacy/security риска · 6. каскад после исправлений в неправильном порядке · 7. разделение модулей и tenant reuse · 8. недостающая инфраструктура до исполнения · 9. жизненный цикл feature flow · 10. перенос на другой стек · 11. интеграция с WordPress или host-платформой · 12. расширение на платформы и marketplace · 13. извлечение знаний из legacy · 14. управление агентом и исправление ролей · 15. UML, automations и связи сущностей · 16. content generation pipeline · 17. RAG и GraphRAG вокруг AI-шагов · 18. AI microservices и generated code · 19. альтернативные стеки, hosting и DevOps · 20. guidance как шаблон оркестрации · 21. план как episode · 22. транскрипт сессии как episode · 23. разбор документа для базы знаний · 24. полный проход по корпусу · 25. проверка RAG grounding · 26. маршрутизация моделей и timeout · 27. продуктовая проверка orchestration advisor · 28. проверка покрытия и gate · 29. честное состояние и resume · 30. калибровка глубины.

**#43 v1 — 20 сценариев (заголовки):** 1 Бизнес-идея->архитектура · 2 Дизайн/diagram->buildable plan · 3 Инструмент даёт неправильный визуал · 4 Несколько fix-ов сразу стало хуже · 5 Нашли privacy/security проблему · 6 Слишком связанные модули · 7 Исследовать много modules без прыжков к выводам · 8 Нет foundation под feature · 9 Подход неправильный, смена направления · 10 Платформа внутри другой платформы (VS/VSCode) · 11 Guidelines сломались в исполнении · 12 План сам себя проверяет (anti-loophole) · 13 Guidelines слишком тяжёлые, парализация · 14 Много flows по одному протоколу · 15 Визуальное качество на массе экранов · 16 Измеряли не то (метрики) · 17 Design review по ролям · 18 Извлечь знания из старого кода · 19 Marketplace/расширение на много платформ · 20 Guidance -> повторяемая оркестрация.

**#44 chains-v2 — 12 реальных чатов (заголовки, 2 uuid-источника):** 1 Skill set для нового проекта · 2 n8n-like flow management · 3 Figma->whole system · 4 Figma design->existing system · 5 System from explanation when design incomplete · 6 AI-assisted diet app flow · 7 Content generation flow · 8 AI microservices, UI flow visibility, debugging, code injection · 9 Generic interfaces, database fabric, queue jobs, microservice base · 10 Alternative stacks and RAG/GraphRAG context planning · 11 CSS generation debug and matrix correction · 12 Planning/review workflow with uploaded skills. (Источники чатов: `c02db858-8191-46c7-adc4-14f7c1b41ed1` для семейства #1–#11, `2d19ed25-93dc-438d-8c02-dda9a5fc7c9e` для #12.)

### C. Репрезентативные примеры (дословные цитаты)

**#43 v1 — сценарий 10 (прямо про VS/VSCode-расширение, это наш продукт):**
Промпт: «Я хочу, чтобы возможности моей платформы были доступны внутри Visual Studio, VS Code или другой среды, где пользователь уже работает.»
Ожидаемая оркестрация (сокр.): 1. Определить host platform и entrypoint. 2. Разделить обязанности: host plugin только вход/отображение, XIIGen core принимает смысловые решения. 3. Какие данные host передаёт: active project, linked projects, user prompt, selected files. … 7. Запретить перенос product brain в plugin. (= прямой источник правила R6 «simple agent boundary».)

**#35 v8 — Кейс 0001:**
Промпт: «У меня ситуация из области «архитектура из идеи и диаграмм»: уже есть или нужно найти «описание продукта», ожидаемый результат — «карта ролей, событий и границ системы», главный риск — «выдумать модули без подтверждений». Особый фокус: сначала проверить недостающие подтверждения и не замазывать неизвестное…»
Оркестрация: Главное подтверждение: описание продукта. Ожидаемый первый результат: карта ролей, событий и границ системы. Главный риск: выдумать модули без подтверждений. 1. сначала понять бизнес-цель и границы системы. 2. связать документы, диаграммы и реальные gaps. 3. не переходить к реализации до архитектурного правила. Контрольный фокус: сначала проверить недостающие подтверждения и не замазывать неизвестное.

**#35 v8 — Кейс 0500 (домен «проверка RAG grounding»):** тело = 1. доказать, что RAG вернул содержание, а не только ID. 2. сравнить релевантный и нерелевантный запрос. 3. проверить, что ответ использует найденные факты. (Риск: «довериться ID без содержания».)

**#44 chains-v2 — цепочка 2 (n8n-like flow engine, дословный фрагмент):**
«I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html code, the flow will apply to several ai models … then provide to additional review to another model and return a result to the client. On each step middle results with prompts will be stored on elastic database…»
`Не смешивать с`: «Не смешивать с skill library как deliverable. Skill library может быть supporting artifact, но сама задача здесь — execution system.»
Оркестрация: lifecycle submit->trace id->store body->enqueue->execute nodes->store outputs->review->return; polling every X sec; node-модель (input/prompt/provider/output/status/retry); fan-out к нескольким AI + review-модель; feedback good/neutral/negative+text привязан к trace/node/prompt. (Это дословный источник дизайна многомодельного pipeline = Node4/5/7 из #18.)

### D. КАВЕРЗА (критичный caveat качества для обучения): v8 — это НЕ 1000 уникальных reasoning-примеров

Проверено дословно: Кейс 0500 и Кейс 0950 — оба домен «проверка RAG grounding» — имеют **побайтово идентичные 3 шага** оркестрации; различаются только `результат` («список ошибок поиск» vs «grounding verdict») и `фокус`. То же для «маршрутизации моделей» (0501 vs 0951 — идентичные шаги). Кейсы 0002 и 0003 (соседние Figma-домены) тоже делят один шаблон шагов.
Вывод: **тело оркестрации в v8 ФИКСИРОВАНО per-домен — ~30 шаблонов маршрута всего**, а «1000 кейсов» получены комбинаторной перестановкой 4 полей-слотов (подтверждение/результат/риск/фокус) поверх этих 30 шаблонов.
- Разнообразие на уровне ПРОМПТА (поверхности) — высокое (1000 разных формулировок).
- Разнообразие на уровне РАССУЖДЕНИЯ (тело маршрута) — низкое (~30 шаблонов).
Практический смысл: модель, обученная на v8 как есть, выучит по сути **классификатор из 30 доменов + копирование слотов подтверждение/результат/риск/фокус в фиксированный шаблон**, а не подлинное многошаговое планирование. Это НЕЛЬЗЯ выдавать за «обученную семантическую оркестрацию». Для настоящего reasoning нужны либо chains-v2-подобные реальные цепочки (разнообразное свободное тело), либо расширение пула шаблонов, либо DPO поверх (см. E).

### E. Как это кормит orchestration-обучение (сигнал обучения — честно)

**Тип сигнала: СУПЕРВАЙЗ A->B (промпт -> целевая/gold оркестрация), НЕ готовые DPO chosen/rejected пары.**
- Все три корпуса дают только «правильную»/положительную сторону (target). В них НЕТ парного «rejected»-маршрута.
- Негатив присутствует лишь как ОПИСАНИЕ, не как полный отвергнутый ответ: v8 поле `Главный риск` (что НЕ делать), chains-v2 поле `Не смешивать с` (соседняя задача-дистрактор), v1 — сама постановка проблемы. Это зёрна негатива, но не rejected-оркестрации.
- Ни один корпус НЕ в требуемом DPO-формате `Pair[ID]/A/B/Reason/Phrasing-B` (см. MEMORY project_xiigen_learning_sets: XIIGEN батчи = «candidate material only»). Значит эти три файла — **кандидатный/supervised материал**, а не готовые обучающие DPO-триплеты.
- Чтобы получить DPO-пары из этого материала: `chosen` = «Ожидаемая/Правильная оркестрация», `rejected` = сгенерированный слабый кандидат (напр. вырожденный Bootstrap-вывод из pass 5 #46/#47, или маршрут, нарушающий `Главный риск`/`Не смешивать с`), `Reason` = поле риска/границы. Этот шаг генерации rejected — ещё НЕ сделан в этих файлах.

### F. Маппинг на 9-node flow (#18, pass 4) и на 7-секционный вывод расширения (#46/#47, pass 5)

Структура полей v8 почти 1:1 ложится на выход, который расширение УЖЕ отдаёт (как Bootstrap-scaffold), и на дизайн 9-node:

| Поле корпуса (v8/chains) | Узел 9-node (#18) | Секция вывода расширения (#46/#47, pass 5) |
|---|---|---|
| `область/домен` (v8) / `Граница задачи` (chains) | Node1 Domain Splitter (prompt->domain label) | «Я понял задачу так» |
| `Не смешивать с` (chains) | Node1 (разделение мульти-доменного промпта на атомы) | — (в Bootstrap отсутствует) |
| нумерованные шаги маршрута | Node3 Scheme Planner (ordered work pieces) | «Как вести работу» |
| `Главное подтверждение` | вход для Node2/Node4 + RAG | «Вопросы к RAG» |
| `Ожидаемый первый результат` | Node9 Formatter (форма ответа) | форма 7-секционного шаблона |
| `Главный риск` + `Контрольный фокус` | Node7/Node8 Arbiter (abstain / external-input-required) | «Что нельзя замазывать» |
| многомодельный pipeline (chains #2) | Node4 Processor / Node5 Applier / Node7 review | — (в Bootstrap отсутствует) |

**Важный вывод для плана:** поля v8 (подтверждение/результат/риск/фокус/шаги) — это ровно СУПЕРВАЙЗ-ТАРГЕТЫ для тех самых секций, которые расширение уже эмитит детерминированно-шаблонно (pass 5). То есть v8/chains — это готовый обучающий вход, чтобы заменить вырожденный Bootstrap-вывод обученным, при этом контракт выхода (7 секций / форма ответа) НЕ меняется. Но:
- v8 даёт единичную `область` на кейс => учит ОДНО-лейбловой классификации, а НЕ мульти-доменному сплиту, который Node1 обязан делать. Мульти-доменный сплит учится только на chains-v2 (`Не смешивать с`, 12 примеров) — этого мало.
- Node2 (доменная трансляция), Node4/5 (multi-model apply), convergence/reprocess-петли (Node7/8) в v8 НЕ покрыты телом маршрута; частично покрыты только в chains-v2 (цепочки 2, 3, 8, 10).

### G. Что осталось на passes 7–9
- **pass 7 (по заданию parent):** сверка кода `42.ProjectWikiDomain` (Bootstrap units) + `15.OrchestratorModel` против имён units из отчётов #46/#47; точка возврата `BOOTSTRAP_FALLBACK`/`runner_missing`.
- **pass 8:** уникальность v1–v7 (#36–#42, файлы 1.5–2.7MB) относительно v8. Гипотеза из заголовка v8 («заменяет v1-v7», «пересобран из v7»): v1–v7 — это устаревшие/промежуточные версии того же 1000-корпуса (v7 — прямой предок v8), а не новый материал; нужно подтвердить diff доменов/формата (менялись ли поля риск/фокус, число доменов, шаблоны шагов). Также сверить `REAL-CHAT-ORCHESTRATION-CHAINS-v1.md` (36KB, тоже 12 секций) — что именно v2 исправил дословно.
- **pass 9:** итог «continue-extensions»: contract выхода (готов, pass 5) -> обучающий вход (v8/chains, pass 6) -> сигнал (supervised A->B + зёрна негатива, нужен шаг генерации rejected для DPO) -> gaps (мульти-доменный сплит недо-покрыт; тело маршрута v8 шаблонно; Node2/4/5/7 недо-покрыты).

**RAG-файл (обновлён append-only):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`

---

## Pass 7/10 — code cross-check (42.ProjectWikiDomain + 15.OrchestratorModel + WikiHookRunner)

- **Статус этого pass:** ГРАУНДИНГ доков против РЕАЛЬНОГО кода в `C:/Projects/llm_mvp_core` (READ-ONLY: Glob/Grep/Read, git не трогался). Проверены 4 задания parent: host-entrypoint, orchestrator-units, точка `BOOTSTRAP_FALLBACK`, 11 wiki-units + query-planning. Все выводы ниже — с `file:line`.

### ГЛАВНЫЙ ВЫВОД PASS 7 (для плана расширений)

1. **Гипотеза pass 5 ПОДТВЕРЖДЕНА кодом: вывод расширения = Bootstrap / `BOOTSTRAP_FALLBACK`.** Реальный runtime-путь хука заканчивается на ШЕСТИ детерминированных Bootstrap-юнитах, обученные чекпойнты НЕ грузятся ни на одном runtime-пути.
2. **Доковый host-entrypoint назван неверно.** Доки (pass 1, `PLAN-PLUGIN-PACKAGING-v1`) обещают бинарь `42.WikiHookRunner` + `WikiOrchestratorFactory.Create(projectRoot)`. В коде **такого проекта и такого класса НЕТ** (repo-wide grep по `.cs` = 0 совпадений на `WikiOrchestratorFactory`; нет top-level каталога `42.WikiHookRunner`). Реальный, СОБРАННЫЙ хук лежит под другим именем: `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs`. То есть packaging-план — это ДИЗАЙН, а materialised хук — это уже другой, более простой файл. Для плана «continue extensions» это критично: строить надо от реального `WikiContextHook/Program.cs` + `ProjectWikiPluginOrchestrationAdapter`, а не от несуществующего `WikiOrchestratorFactory`.
3. **Обучающий контур КОДОМ существует, но НЕ подключён к runtime (training-vs-runtime wiring gap).** У каждого wiki-юнита есть полный DPO-lifecycle в коде (`*DpoGenerator` / `*TrainingPipeline` / `*TrainingRunner` / `*ManifestPublisher`), но фабрика `CreateDefault` инстанцирует юниты через `new ...()` без загрузки чекпойнта/манифеста. Это ровно тот «центральный» разрыв, что описан в аудитах (обучение не подключено в проде; веса — локальная агрегация; default-цепочка запирает запрос ДО модели).

### 1. Host-entrypoint: `42.WikiHookRunner` / `wiki-context-hook.exe` / `WikiOrchestratorFactory.Create` — ПЛАН-ONLY, но есть РЕАЛЬНЫЙ хук под другим именем

- **`WikiOrchestratorFactory` — отсутствует в коде.** Repo-wide grep по `*.cs` (паттерн включал `WikiOrchestratorFactory`) вернул 0 совпадений на это имя. Top-level каталоги проверены (`Get-ChildItem -Directory`) — есть `42.ProjectWikiDomain`, **нет** `42.WikiHookRunner`. Вывод: `WikiOrchestratorFactory.Create` + `WikiContextFormatter.Format` живут ТОЛЬКО в тексте `PLAN-PLUGIN-PACKAGING-v1.md` (pass 1). **PLAN-ONLY.**
- **Реальный хук:** `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs` (console app). Ключевое:
  - `Program.cs:12` — `var adapter = new ProjectWikiPluginOrchestrationAdapter();` — **дефолтный конструктор без оркестратора** (см. §3 — это и активирует `BOOTSTRAP_FALLBACK`).
  - `Program.cs:9-13` — режим `--orchestrate` → `adapter.OrchestrateAsync(request)`.
  - `Program.cs:24-35` — режим `--ensure-ready` → возвращает **захардкоженный** `isReady=true`, `status="PLUGIN_ORCHESTRATION_HOOK_AVAILABLE"`, `documentationVersion="plugin-runtime"`, `entryCount=1`. Это НЕ проверка готовности модели (никакого чтения манифеста/чекпойнта), а всегда-true заглушка.
  - `Program.cs:45-71` — вход через ENV: `XIIGEN_ORCHESTRATION_REQUEST` (JSON целиком), иначе `HOOK_PROMPT`, `XIIGEN_PROJECT_ROOT` (default `CurrentDirectory`), `XIIGEN_LINKED_REPO_PATHS`, `XIIGEN_PLUGIN_ENTRYPOINT` (default `wiki-context-hook`). Совпадает по СМЫСЛУ с ENV из плана (`HOOK_PROMPT`/`XIIGEN_PROJECT_ROOT`), но без `WIKI_CONFIDENCE_MIN`/`WIKI_TOP_N` из плана.
- **Итог задания №1:** entrypoint-контракт по духу реален и собирается, но под именем `WikiContextHook` (проект `43.LearningLocalWikiData/tools`), а НЕ `42.WikiHookRunner`; фабрика `WikiOrchestratorFactory` не существует — вместо неё прямой `new ProjectWikiPluginOrchestrationAdapter()`.

### 2. Orchestrator-units из отчётов #46/#47 — какие реальны, где, Bootstrap-vs-trained

| Имя юнита из отчёта (pass 5) | Есть как класс? | Файл:line | Природа на runtime |
|---|---|---|---|
| `TaskOrchestrationUnit` | ДА | `42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs:60` | детерминированный (композиция пайплайна; интент делегирован в `BootstrapIntentBuilderUnit`, комм. `:176-177`) |
| `ProjectUnderstandingUnit` | ДА | `ProjectTaskOrchestrationPipeline.cs:180` | детерминированный (FS-скан/эвристики) |
| `EvidencePlanner` | ДА | `ProjectTaskOrchestrationPipeline.cs:308` | детерминированный |
| `UnitSelector` | ДА | `ProjectTaskOrchestrationPipeline.cs:343` | детерминированный (if-chain) |
| `HumanRouteComposer` | ДА | `ProjectTaskOrchestrationPipeline.cs:371` | детерминированный (шаблон строки) |
| `VerificationAbstentionUnit` | НЕТ (концепт/имя из отчёта) | — | роль закрыта `ProjectAnswerVerifier.cs` + `BootstrapOrchestrationVerifierUnit` (`IOrchestrationVerifierUnit`, `BootstrapOrchestrationUnits.cs:269`) |
| `MissingEvidenceAbstentionUnit` | НЕТ (концепт/имя из отчёта) | — | роль закрыта `ProjectWikiPluginOrchestrationAdapter.FindMissingEvidence` → статус `BLOCKED_WITH_GAPS` (adapter `:168-173`) + verifier |
| `CrossStackBusinessFlowGraphUnit` | ДА | `42.ProjectWikiDomain/CrossStackBusinessFlowGraphUnit.cs` | trainable-семейство есть (см. §4), но на runtime не грузится |
| `WikiContextRoutingUnit` | ДА | `42.ProjectWikiDomain/WikiContextRoutingUnit.cs` | детерминированный роутер Mode2/Mode3 |

- **Bootstrap-набор (6 юнитов) — реальный источник вывода расширения.** `BootstrapOrchestrationUnits.cs`: `BootstrapIntentBuilderUnit` (`:19`), `BootstrapProjectScannerUnit` (`:103`), `BootstrapEvidencePlannerUnit` (`:144`), `BootstrapUnitSelectorUnit` (`:183`), `BootstrapRouteComposerUnit` (`:225`), `BootstrapOrchestrationVerifierUnit` (`:269`). Комментарии прямо признают детерминизм: `:17` «parses a user prompt using the same **Regex.Split**», `:181` «selects execution units using the same **if-chain** logic». Контракты — `OrchestrationUnitContracts.cs` (`IIntentBuilderUnit:76`, `IRouteComposerUnit:100`).
- **Вывод задания №2:** пять «report-имён» — это РЕАЛЬНЫЕ детерминированные классы старого пайплайна; два (`Verification/MissingEvidence Abstention`) — только концепт-имена из отчёта, роль закрыта verifier+adapter. Все — детерминированные, НЕ trained.

### 3. Точная точка `BOOTSTRAP_FALLBACK` (задание №3)

Вопреки формулировке из CLAUDE.md/RAG-01, ветвь `ILogicalOrchestrator` vs `SubmitUserTaskApi` живёт **НЕ** в `15.OrchestratorModel/ProjectWikiOrchestrationFlow.cs`, а в доменном адаптере:

- **`42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs`:**
  - `:52-55` поля: `_submitUserTaskApi` (комм. «is the **BOOTSTRAP_FALLBACK** path when no orchestrator is provided»), `_orchestrator` (`ILogicalOrchestrator?`, optional).
  - `:59-70` ctor: `_orchestrator = orchestrator;` комм. `:68` «null means **BOOTSTRAP_FALLBACK** active». Дефолт всех параметров = null.
  - `:111` **развилка** `if (_orchestrator is not null)` → orchestrator-путь: `_orchestrator.RunAsync(...)` со сценарием `project-wiki-orchestration-flow` (`:116-139`).
  - `:141-161` **`else` = BOOTSTRAP_FALLBACK**: комм. `:143` «flow not registered — falling back to direct SubmitUserTaskApi», `:145` «**BOOTSTRAP_FALLBACK** active for project-wiki-orchestration-flow»; исполняется `await _submitUserTaskApi.SubmitAsync(...)` (`:147`).
- **`SubmitUserTaskApi`** = `42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs:45`; в ctor `:87-92` дефолтит РОВНО шесть Bootstrap-юнитов (`?? new BootstrapIntentBuilderUnit()` … `?? new BootstrapOrchestrationVerifierUnit()`).
- **`15.OrchestratorModel/ProjectWikiOrchestrationFlow.cs`** (orchestrator-«proper» путь) сам по себе тоже **терминируется на Bootstrap**: `:25/:32` держит `SubmitUserTaskApi`, `:79` `await _submitUserTaskApi.SubmitAsync(...)`; `passed = task.Status == "ORCHESTRATED"` (`:87`). То есть даже «правильный» orchestrator-путь сейчас = те же шесть Bootstrap-юнитов, а не обученная модель.
- **Регистрация flow:** `15.OrchestratorModel/LogicalOrchestratorAssistant.cs:130-136` — `project-wiki-orchestration-flow` регистрируется в default-ассистенте через `new ProjectWikiOrchestrationFlow()`. НО реальный хук (`WikiContextHook/Program.cs:12`) конструирует адаптер **без** передачи этого оркестратора → на практике активен именно `else`-путь `BOOTSTRAP_FALLBACK`. Тесты это фиксируют: `Tests/ProjectWikiOrchestrationFlowTests.cs:88` и `Tests/ProjectTaskOrchestrationPipelineTests.cs:46` строят `new ProjectWikiPluginOrchestrationAdapter()` (дефолт → fallback).
- **Вывод задания №3:** дефолтная конструкция адаптера (и в проде-хуке, и в тестах) = `_orchestrator==null` = `BOOTSTRAP_FALLBACK` = детерминированный `SubmitUserTaskApi`. Warning в коде помечен комментарием, но это ТЕКУЩИЙ основной путь, а не редкий деградационный.

### 4. 11 wiki-units + query-planning в `42.ProjectWikiDomain` (задание №4): существуют, trainable-семейства есть, но runtime — детерминирован

- **Файлы юнитов присутствуют** (все в `42.ProjectWikiDomain/`): `ProjectWikiQueryPlanningFlow.cs`, `WikiContextRetriever.cs`, `ProjectLocalWikiOrchestrator.cs`, `ProjectWikiRequestRouterUnit.cs`, плюс `CodeTraceBuilderUnit`, `EntityImpactMapperUnit`, `DocumentationGeneratorUnit`, `StackAndVersionClassifierUnit`, `SystemBoundaryMapperUnit`, `CrossStackBusinessFlowGraphUnit`, `WikiContextRoutingUnit`, `WikiTraceAggregatorUnit`, `ProjectAnswerVerifier`, `ProjectLocalWikiModelUnit`, `NegativeDtoDocumentationOrchestrator`, `PromptFlowGroundingModel`, `OrchestrationPlanMachine` (Phase 37/v20). У КАЖДОГО ключевого юнита рядом лежит полный DPO-lifecycle: `*DpoGenerator.cs`, `*TrainingPipeline.cs`, `*TrainingRunner.cs`, `*ManifestPublisher.cs`. То есть обучающая инфраструктура — реальный код, а не заглушки.
- **НО runtime-конструкция детерминирована и без загрузки весов** — `ProjectWikiDomainServiceCollectionExtensions.cs`:
  - `CreateDefault(...)` `:11-33`: `new WikiContextRetriever(evidenceIndex, hybridRetriever)` (`:20`), `new ProjectWikiQueryPlanningFlow()` (`:21`), `new ProjectLocalWikiModelUnit()` (`:22`), `new ProjectAnswerVerifier()` (`:23`) — всё через `new`, БЕЗ `IModelRepository.LoadWeights` / манифеста / чекпойнта. `evidenceIndex` берётся из JSONL на диске (`ProjectEvidenceIndex.LoadFromWikiDomain`, `:16`).
- **`ProjectWikiRequestRouterUnit.Route()` (`:10-70`) — детерминированный keyword-роутер**: цепочка `ContainsAny(text, "...")` по десяткам ключевых фраз → фиксированное `Decision(...)` с захардкоженной `confidence` (0.92–0.95). Это R4-«hidden determinism»: несмотря на наличие соседних `ProjectWikiRequestRoutingTrainableModel.cs` / `...TrainingRunner.cs`, на runtime маршрутизация = regex/keyword if-chain, обученная модель не вызывается.
- **`ProjectWikiQueryPlanningFlow.PlanAsync` (`:17-60`) — тоже детерминирован**: `DetectIntent` + `ExtractSymbols` (compiled `Regex`, `:13-15`) + `BuildSteps` + `CalculateConfidence`; никакого чтения чекпойнта. Это тот самый юнит, чьё «отсутствие обученной версии» доки (pass 1) назвали корневой причиной провала base-model — код показывает, что RUNTIME-версия существует, но она rule-based, а не trained.
- **Вывод задания №4:** все 4 названных юнита + ещё ~11 обучаемых семейств физически есть в `42.ProjectWikiDomain`; классово это правильная иерархия. Но на исполнении они — детерминированные скелеты; обученные чекпойнты из training-раннеров в конструкторы не подключены.

### Сводка «доки vs код» (закрытие разрыва pass 1–6)

| Доковое утверждение (pass 1–6) | Код-реальность (pass 7) | Вердикт |
|---|---|---|
| `wiki-context-hook.exe` = проект `42.WikiHookRunner` | Такого проекта нет | **PLAN-ONLY (имя неверно)** |
| Вход через `WikiOrchestratorFactory.Create(projectRoot)` | 0 совпадений в `.cs`; вместо этого `new ProjectWikiPluginOrchestrationAdapter()` в `WikiContextHook/Program.cs:12` | **PLAN-ONLY, заменён адаптером** |
| Реальный собранный хук | `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs` | **CODE (под другим именем)** |
| Вывод расширения = Bootstrap/fallback (гипотеза pass 5) | дефолт-адаптер → `_orchestrator==null` → `BOOTSTRAP_FALLBACK` → 6 Bootstrap-юнитов | **ПОДТВЕРЖДЕНО** |
| Юниты #46/#47 (`TaskOrchestrationUnit` и др.) | реальные детерминированные классы в `ProjectTaskOrchestrationPipeline.cs` | **CODE, детерминированы** |
| `ProjectWikiQueryPlanningFlow` обучен | класс есть, но `PlanAsync` = regex/rule-based; trained-раннер не вызывается | **CODE, но deterministic** |
| «локальная модель учится / грузит веса» | `CreateDefault` всё `new`-ит, без `LoadWeights`/манифеста | **training-vs-runtime wiring gap** |

### Что осталось коду на passes 8–9 (дополнение к плану passes 8/9 выше — не заменяет его)

- **Код-остаток pass 8:** прочитать тела `WikiContextRetriever.cs` (Mode3 BM25 через `12.SearchEngine` vs Mode2 через `08.GraphRag.HybridRetriever`) и `ProjectLocalWikiOrchestrator.cs` — подтвердить, где именно эмитится `semantic_retrieval_unavailable` и `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`/`runner_missing`; проверить, есть ли ХОТЬ ОДНА runtime-точка, где `IModelRepository.LoadWeights`/манифест реально грузится (искать `LoadWeights`, `IRemoteUnitLocator`, `IModelRepository` в `25.ModelServer`/`26.TrainableRouter`).
- **Код-остаток pass 9:** сформулировать для плана «continue extensions» минимальный product-spine: (1) строить от реального `WikiContextHook/Program.cs` + `ProjectWikiPluginOrchestrationAdapter`, НЕ от несуществующего `WikiOrchestratorFactory`; (2) закрыть wiring-gap — подать в `CreateDefault`/адаптер обученные чекпойнты (иначе расширение навсегда отдаёт Bootstrap-вывод); (3) передать зарегистрированный `ILogicalOrchestrator` в адаптер, чтобы выйти из `BOOTSTRAP_FALLBACK`.

**Честная строка покрытия pass 7:** прочитаны целиком/существенно 7 код-файлов (`ProjectWikiOrchestrationFlow.cs`, `ProjectWikiPluginOrchestrationAdapter.cs` тело развилки, `ProjectWikiRequestRouterUnit.cs:1-70`, `ProjectWikiQueryPlanningFlow.cs:1-75`, `ProjectWikiDomainServiceCollectionExtensions.cs` целиком, `WikiContextHook/Program.cs` целиком, + grep-подтверждения по `BootstrapOrchestrationUnits.cs`/`ProjectTaskOrchestrationPipeline.cs`). Файловый инвентарь `42.ProjectWikiDomain` снят полностью (`Get-ChildItem`, ~100 `.cs`). НЕ читались целиком: `WikiContextRetriever.cs`, `ProjectLocalWikiOrchestrator.cs`, тела training-runner'ов (отложено на код-остаток pass 8, см. выше). Всё — READ-ONLY, git не трогался.

**RAG-файл (обновлён append-only pass 7):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`


---

## Pass 8/10 — remaining files: corpora v1–v7, chains-v1, 4 archives, negative-DTO diagram, tools

Пасс закрывает файлы, которых passes 1–7 не касались, чтобы ни один файл папки не остался «в стороне». Всё READ-ONLY, git не трогался; правился только этот RAG-файл (append). Архивы распаковывались во временную read-папку вне исходной директории.

**Файлы, покрытые в этом проходе (9 корпусных + 1 seed + 2 chains + 4 zip + диаграмма + PS1):**
`USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-v1.md` (seed), `...-1000-v1..v8.md`, `REAL-CHAT-ORCHESTRATION-CHAINS-v1.md` (+ сверка с v2), `orchestrator wiki.drawio.zip`, `wiki-sql-phase2-plans-v1.zip`, `wiki-sql-phase2-plans-v2.zip`, `wiki-training-language-plans-v1.zip`, `orchestrator wiki.drawio Negative DTO.xml`, `tools/Invoke-ProjectWikiPhaseReport.ps1`.

### 1. Корпус USER-ORCHESTRATION-SCENARIOS: уникальность v1–v7 против v8 (задание №1)

Есть ДВА разных корпуса, не путать:
- **Seed (мелкий): `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-v1.md`** — 420 строк, вручную написанный первичный набор. 20 доменных разделов человеческим языком (`## 1. Бизнес-идея -> архитектура системы` … `## 20. Нужно превратить guidance в повторяемую оркестрацию`). Формат простой: `промпт -> ожидаемая оркестрация первого слоя`, без кода. Это **концептуальный источник**, из которого потом раздули 1000-корпус. Уникален как компактная человеко-читаемая таксономия из 20 ситуаций; не заменяется 1000-версиями (там та же смысловая карта, но абстрагированная в 30 доменов).
- **1000-корпус: `...-1000-v1..v8.md`** — 8 версий по РОВНО 1000 кейсов каждая (`grep -c '^## (Пример|Кейс)' = 1000` во всех восьми).

Сводка версий 1000-корпуса (заголовок каждого файла сам декларирует, что он заменяет):

| Версия | строк | кейсов | доменов | что это | вердикт уникальности |
|---|---|---|---|---|---|
| v1 | 20013 | 1000 | — (1 шаблон) | **механическое размножение**: шаги 1–4,6,7 идентичны во всех 1000 кейсах, меняется только строка 5 «Использовать тему эпизода: `<English source title>`» (напр. `Document type classification`) | superseded; product-ценности нет |
| v2 | 19015 | 1000 | source-driven | строится от отдельных источников (episode map/guidance/review), но **всё ещё тянет английские source-заголовки** в промпты | superseded |
| v3 | 18007 | 1000 | **24** | промпты по-русски, человеческий смысл; заголовок: «заменяет v1 и v2» | superseded (24⊂30) |
| v4 | 22005 | 1000 | **30** | добавлены поля цель/подтверждение/ожидаемый результат/риск/первый проверяемый результат; «заменяет v1,v2,v3» | superseded |
| v5 | 22005 | 1000 | 30 | микроправка v4: `missing evidence`→`missing подтверждения` (заголовок по ошибке всё ещё «v4») | superseded |
| v6 | 22005 | 1000 | 30 | пошаговые списки переведены на русский; «заменяет v1-v5» | superseded |
| v7 | 22005 | 1000 | 30 | почти = v6 (заголовок-опечатка «v6»); **прямой предшественник v8** | superseded |
| **v8** | 20005 | 1000 | **30** | «заменяет v1-v7, пересобран из v7»; каждый кейс переписан человечнее (тип работы / подтверждение / ожидаемый результат / риск / контрольный фокус) | **актуальный, канонический** |

**Проверка перекрытия доменов (уникального материала в v1–v7 НЕТ):** все 30 доменов v4–v7 присутствуют в v8. `grep 'из области «…»'` в v8 даёт ровно 30 различных доменов (первый счёт «13» был артефактом обрезки многобайтных `« »` в UTF-8, при расширенном захвате — 30). Точечно сверено: `Figma`(149), `marketplace`(146), `tenant`(185), `privacy`(82), `design-to-code`(68), `GraphRAG`(78), `WordPress`(72), `legacy`(78), `каскад`(68), `транскрипт`(66), `глубины`(72) — все в v8. Единственный домен, который по английскому названию (`model routing`/`timeout`) в v8 не находится дословно, присутствует под русским именем: `маршрутизация модел…`(85 hits) + `timeout`(89 hits) — то есть **не потерян, только переформулирован**.

**Вердикт задания №1:** v8 — единственная версия, которую нужно грузить в план как корпус первого-слоя orchestration-advisor. v1–v7 — исторические промежуточные ступени (v1 механический → v2 англо-загрязнён → v3 24 домена → v4-v7 30 доменов → v8 гуманизирован из v7). **Ни одна из v1–v7 не держит уникального продуктового материала, которого нет в v8.** Единственная «уникальность» v1/v2 — сырые английские названия эпизодов-источников как темы (провенанс, а не тренировочная ценность). Отдельно сохраняется мелкий seed-файл (20 ручных доменов) как компактная человеческая карта.

### 2. chains-v1 vs chains-v2 (задание №2): что именно починил v2

Оба файла — по **13 цепочек** (`## ` = 13 в обоих). `REAL-CHAT-ORCHESTRATION-CHAINS-v1.md` (#45) — черновик; `...-v2.md` (#44, покрыт в pass 6) — исправление.

- **Дефект v1 (сам заявлен в шапке v2):** «один длинный пользовательский message был разрезан и пересказан так, будто это отдельные готовые prompt-ы. Это смешало разные задачи и исказило оркестрацию». Пример: цепочка №1 в v1 берёт весь длинный prompt (skill set + n8n-like flow engine + Figma-вход + multi-model AI + промежуточное хранилище в Elastic + feedback + future learning) как ОДНУ слепленную задачу.
- **Что добавил v2 (структура полей на каждую цепочку):**
  - `Точный фрагмент` — дословный непрерывный кусок из чата (не пересказ);
  - `Граница задачи` — что ИМЕННО просит этот фрагмент;
  - `Не смешивать с` — соседняя задача из того же message, которую нельзя подтягивать в эту оркестрацию;
  - `Правильная оркестрация` — ведение сверху вниз до рабочих подуровней;
  - `Результат для человека` — что пользователь должен увидеть.
- **Конкретный fix:** та же цепочка №1 в v2 сужается до «Skill set для нового проекта» — только фрагмент про .NET/Kubernetes/React-Native skill set; часть про flow engine выносится в `Не смешивать с`. v1 также имел лишние отсылки к нескольким chat-id и «Смысловой разбор»; v2 привязывает к одному chat-id и режет по границам задач.
- **Вердикт:** v2 строго лучше; v1 — предшественник (superseded). Для плана как reference-набор реальных цепочек использовать v2.

### 3. Четыре ZIP-архива (задание №3): содержимое и релевантность

| Архив | файлов | содержит loose в папке? | что это | релевантность |
|---|---|---|---|---|
| `orchestrator wiki.drawio.zip` | 26 (289 KB) | да, почти все | **ранний снимок-бандл всей папки** от 2026-05-15: drawio.xml + обе диаграммы + PROJECT-LOCAL-WIKI-MODEL-* сессии + планы/протоколы + `orchestrator prompts.txt`. Внутренний `STATE.json` = 2739 байт против loose 71497 байт → снимок СТАРЕЕ loose-файлов | **избыточен**: уникального нет, всё есть loose в более новых версиях; исторический delivery-снимок |
| `wiki-sql-phase2-plans-v1.zip` | 2 | **НЕТ** (уникально) | `SESSION-07-SQL-STORED-PROCEDURE-WIKI.md` (17 KB) + `PLAN-SQL-CONNECTION-EXTRACTOR-v1.md` (21 KB) | **уникальный материал**: Phase-2 SQL-расширение wiki |
| `wiki-sql-phase2-plans-v2.zip` | 2 | **НЕТ** (уникально) | те же 2 файла, расширенные (28 KB + 27 KB) | **уникально, новее v1** — брать v2 |
| `wiki-training-language-plans-v1.zip` | 4 | **НЕТ** (уникально) | `PLAN-WIKI-TRAINING-{ANGULAR, ANGULARJS, JAVA, SQL}-v1.md` | **уникальный материал**: per-stack планы обучения wiki |

**Уникальное содержимое (важно для «continue extensions» — расширение wiki за пределы .NET-кода):**
- **SESSION-07 SQL** (EXECUTION SESSION, Phase 2, НЕ заменяет Phase 1 code-wiki): (1) найти connection strings в проекте; (2) спросить явное разрешение per-database (consent gate); (3) для одобренных — извлечь stored procedures/views/triggers из ЖИВОЙ БД; (4) построить SQL-wiki-домен с полным impact-графом на объект; (5) интегрировать в `ProjectLocalWikiOrchestrator`, чтобы вопросы про SP отвечали с теми же evidence-id цитатами, что и вопросы про код. Стоп-гейт: не стартовать до прохождения SESSION-04 и наличия ≥1 connection string.
- **PLAN-SQL-CONNECTION-EXTRACTOR-v1** — архитектура компонентов под `42.ProjectWikiDomain/Sql/`: `ISqlConnectionExtractor`/`SqlConnectionExtractor` (скан connection strings), `ISqlConsentGate`/`SqlConsentGate` (согласие per-connection + хранение), `ISqlObjectExtractor`/`SqlObjectExtractor` (извлечение SP/views/triggers из БД), `ISqlDocumentationOrchestrator`/`SqlWikiDocumentationOrchestrator`. Статус: REFERENCE.
- **Language-plans (v1)** — правило у всех одинаковое: «pre-train на reasoning-паттернах из открытых источников, потом валидировать на коде проекта» (two-layer). ANGULAR (2+, signals/components), **ANGULARJS (1.x — намеренно ОТДЕЛЬНЫЙ план**, чтобы модель не отвечала на AngularJS языком Angular 2+), JAVA (Spring Boot — доминирующий стек в XIIGen-validation), SQL (SP/DW/Financial/ERP/CRM/Integration; ключевое отличие — документируемый объект не класс, а SP/view/trigger в БД). Все REFERENCE, расширяют `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`.

**Вывод задания №3:** 3 из 4 архивов держат УНИКАЛЬНЫЙ материал (SQL Phase-2 ×2 + language-plans), которого нет loose и который passes 1–7 не видели; это готовый product-spine для расширения wiki на SQL и на не-.NET стеки. `orchestrator wiki.drawio.zip` — избыточный старый снимок.

### 4. Диаграмма Negative-DTO + `tools/Invoke-ProjectWikiPhaseReport.ps1` (задание №4)

**`orchestrator wiki.drawio Negative DTO.xml`** (8393 байт, 2026-05-14 13:27) — прочитан целиком (drawio/mxGraphModel). Узлы: `Orchestrator domain`(контейнер), `Main IOrchestrator`, `Make a documentation`→`Main IOrchestrator`, `Code`→`Documentation`/`DocumentationOrchestrator`(«reads code»), `Documentation`→`ProjectLocalWikiOrchestrator`(«Teach local wiki llm»)/→`NegativeDtoDocumentationOrchestrator`, `DocumentationOrchestrator`→`ProjectLocalWikiOrchestrator`, `NegativeDtoDocumentationOrchestrator (generates candidates + validates)`→`ProjectLocalWikiOrchestrator`/→`ProjectEvidenceIndex (Mode 3 BM25)`(«validate evidence ids»); нижняя ветка вопроса: `Question about code`→`Main IOrchestrator (question entry)`→`ProjectLocalWikiOrchestrator`(«Get answers from local logical orchestrator»).

- **Diff с базовой `orchestrator wiki.drawio.xml`** (10756 байт, 2026-05-15 10:40 — новее): вопреки названию, Negative-DTO вариант — это РАНЕЕ и ПРОЩЕ. Уникально в нём только: узел `NegativeDtoDocumentationOrchestrator (generates candidates + validates)` + ребро `validate evidence ids`→`ProjectEvidenceIndex(BM25)`, плюс метка вопроса «Get answers from local logical orchestrator» и строчная «Teach local wiki llm». Базовая диаграмма БОГАЧЕ и заменяет её: добавляет `EmbeddingModel (07/09)`, `GraphRag.HybridRetriever (Mode 2 semantic)`, `WikiContextRetriever (Mode 2/Mode 3)`, `ProjectWikiQueryPlanningFlow (query plan)`, `ProjectWikiRoutingFlow`, `L0 project wiki routing`, `Question Seeds`, `Wiki DPO Pairs + Held-out QA`, `plan/execute planned queries`, `requires active embedding` — но при этом ЯВНОГО узла NegativeDto в базовой нет.
- **Ценность Negative-DTO-диаграммы:** документирует роль реального класса `NegativeDtoDocumentationOrchestrator` (pass 7 подтвердил, что класс существует в `42.ProjectWikiDomain`): генерировать документационные КАНДИДАТЫ и валидировать их evidence-id против `ProjectEvidenceIndex` (BM25, Mode 3). Это негативно-DPO ветка (кандидат → проверка id). Обе диаграммы — architecture reference, не код.

**`tools/Invoke-ProjectWikiPhaseReport.ps1`** (197 строк) — прочитан целиком. PowerShell-раннер тест-гейтов ProjectWikiDomain с явной анти-masquerade дисциплиной:
- **Params:** `-SolutionPath` = `C:\Projects\llm_mvp_core\LlmFromScratch.sln`, `-WorkDir` = repo root, `-ReportRoot` = `…\orchestrator extension\phase-metrics`, `-IncludeFullSolution` (switch).
- **Фазы (dotnet test --filter):** (1) `phase-01-query-planning-unit-gate` → `FullyQualifiedName~ProjectWikiQueryPlanning`; (2) `phase-02-project-wiki-domain-integration` → `FullyQualifiedName~ProjectWikiDomainTests`; (3) `phase-03-real-project-validation-five-dotnet-wikis` → `…ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`; (4, только с `-IncludeFullSolution`) `phase-04-full-solution-regression` → пустой фильтр (вся solution).
- **Метрики на фазу:** regex по stdout `Passed! - Failed: X, Passed: Y, Skipped: Z, Total: N`; `pass_rate = passed/total`; сэмплирование процессов `dotnet/testhost/vstest.console` каждые 250 мс → peak working-set (MB) и CPU (ms). Сэмплинг best-effort (ошибка выборки не должна скрыть провал теста).
- **Гейт-логика честности (главное):** `false_positive_risk = "none"` ТОЛЬКО если `exit_code==0 AND total>0 AND failed==0`, иначе `"partial_gate"`. Жёсткие scope-поля: `section_22_applicable=false` (Section 22 = training/DPO — не применимо), `runner_output_available=false`, `missing_runner_metrics=[reward_margin, loss, heldout_accuracy]`, `allowed_claim="test_result_only_not_training_promotion"` (summary: `"phase_resource_report_generated_not_training_promotion"`). То есть зелёный `dotnet test` НИКОГДА не может быть выдан за promotion обученной модели.
- **Артефакты:** пишет per-phase JSON + `PHASE-RESOURCE-REPORT.json` в `phase-metrics/<yyyy-MM-dd-HHmmss>/`, ставит `artifact_written_before_message=true` (артефакт до сообщения — тот же конвеншн, что в reports pass 5/6).
- **Связка с планом:** это ровно тот тул, что меряет гейты ProjectWikiDomain (query-planning unit, domain integration, валидация на 5 реальных .NET-wiki), и он САМ в коде/тулинге декларирует отсутствие runner-метрик (`reward_margin/loss/heldout_accuracy`) — независимое подтверждение вывода pass 1/pass 7 про `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`/`runner_missing` (training-vs-runtime wiring gap). Папка `phase-metrics/` в директории отсутствует → тул тут ни разу не запускался; это спецификация раннера, не набор готовых метрик.

### Что осталось (для passes 9–10)

- Файлы папки покрыты полностью (top-level = 48 файлов + `reports/` + `tools/`). Passes 1–7 + этот pass 8 = ни одного неохваченного файла: манифест/host, протоколы/packaging, sessions/STATE/units, #18 extraction protocol, #46/#47 reports, #35/#44/#43 корпуса, код-сверка (pass 7), и здесь — корпуса v1–v7 + seed, chains-v1, 4 zip, обе диаграммы, PS1.
- **Код-остаток (перенесён из pass 7, НЕ закрыт здесь — этот pass про файлы папки, не про repo-код):** тела `WikiContextRetriever.cs` / `ProjectLocalWikiOrchestrator.cs` (где эмитятся `semantic_retrieval_unavailable` и `runner_missing`) и поиск реальной runtime-точки `IModelRepository.LoadWeights`/манифеста в `25.ModelServer`/`26.TrainableRouter` — задача pass 9.
- **Для плана «continue extensions» этот pass добавляет:** (a) канон корпуса = 1000-**v8** (+ seed как человеческая карта), chains = **v2**; (b) уникальные product-ветки из zip: **SQL Phase-2** (consent-gated извлечение SP/views/triggers → SQL-wiki-домен в `ProjectLocalWikiOrchestrator` с evidence-id цитатами) и **per-stack language training** (Angular/AngularJS-отдельно/Java/SQL, two-layer pre-train→validate); (c) тест-гейт-раннер `Invoke-ProjectWikiPhaseReport.ps1` как готовый Path-A измеритель гейтов ProjectWikiDomain, честно ограниченный `test_result_only_not_training_promotion`.

**RAG-файл (обновлён append-only pass 8):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`

---

## Pass 9/10 — синтез: host-контракт, дизайн-vs-реальность, обучающий конвейер, cross-folder мосты, открытые вопросы

**Статус pass 9 (честно):** закрыт код-остаток pass 7/8 — прочитаны ЦЕЛИКОМ `42.ProjectWikiDomain/WikiContextRetriever.cs` (101 стр.), `ProjectLocalWikiOrchestrator.cs` (317 стр.), `ProjectLocalWikiModelUnit.cs` (188 стр.) + `ProjectWikiQueryPlanningManifestPublisher.cs` как образец publisher'а + repo-wide grep по load-точкам (`LoadWeightsAsync`/`LoadCheckpoint`/`PromotedLocalModelLoader`/`IRemoteUnitLocator`). READ-ONLY соблюдён (git не трогался; изменён только этот RAG-файл, append-only). Это СИНТЕЗ-проход: сводит passes 1–8 + закрытый код-остаток в одну исполняемую картину для плана «continue VS/VSCode extensions».

### 1. Код-остаток ЗАКРЫТ: training-vs-runtime wiring gap подтверждён на теле кода

**Где эмитятся статусы (file:line):**
- `semantic_retrieval_unavailable_fallback_to_bm25` — `WikiContextRetriever.cs:88`, ровно в ветке `if (_hybridRetriever is null)` (`:81-89`): когда `HybridRetriever` (08.GraphRag, Mode 2 semantic) не передан в ctor (`:12` — параметр опционален, default `null`), retriever молча падает в BM25 (`Mode3LexicalEvidence`) и добавляет этот warning. Далее он всплывает как метрика `semantic_unavailable` в `ProjectLocalWikiOrchestrator.cs:131`. Это НЕ баг — заявленный degrade-контракт (Mode 2 нет → Mode 3), совпадает с SESSION-06 Retrieval Mode Contract (pass 3). Реальная Mode-3 выборка = `_evidenceIndex.Search(retrievalQuestion, request.TopK)` (`:41`, BM25 через `ProjectEvidenceIndex`), Mode-2 = `_hybridRetriever.Retrieve(...)` (`:91`).
- `runner_missing` / `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` — эмитятся НЕ на answer-пути, а на doc-generation пути: `ProjectDocumentationPipelineFlowAdapter.cs:47,68` и `ProjectDocumentationPipelineOrchestrator.cs:74,117,136`. Честный статус «граф документации готов, обучающего раннера нет» (Gate 5, pass 2).

**Грузит ли ХОТЬ ОДНА runtime-точка answer-пути веса/чекпойнт/манифест? — НЕТ (подтверждено телом + grep):**
- `ProjectLocalWikiOrchestrator.AskWithTraceAsync` (`:50-100`) = фиксированная цепочка `_queryPlanningFlow.PlanAsync → _retriever.RetrieveAsync → _modelUnit.AnswerAsync → _verifier.Verify → BuildTrace`. НИ ОДНОГО `LoadWeightsAsync`/`LoadCheckpoint`/`Locate`. Собственный self-trace юнитов ДОСЛОВНО помечает их детерминированными: планировщик `"deterministic-pretrain-surface"` (`:208`), retriever `"bm25-plus-optional-graphrag"` (`:218`), verifier `"deterministic-verifier"` (`:227`). Метрика `query_plan_invoked` жёстко захардкожена `"1"` (`:132`) — т.е. «query-plan вызван» всегда true независимо от качества.
- `ProjectLocalWikiModelUnit.AnswerAsync` (`ProjectLocalWikiModelUnit.cs:7-55`) = чистый строковый ШАБЛОНИЗАТОР над retrieved evidence: берёт top-hit (`:35`), собирает текст `BuildAnswerText` (`:57-77`); developer-reasoning и cross-stack секции — это `string.Contains` по ключевым словам (`:79-150`, напр. «stored procedure»/«sql»/«add»/«field»/«trace»). Confidence — ФИКСИРОВАННЫЕ КОНСТАНТЫ: `0.88` (Mode2) / `0.82` (Mode3) (`:43`), `0.35` при отсутствии evidence (`:29`). Ни весов, ни чекпойнта, ни обучаемого сигнала — R4 hidden determinism.
- Repo-wide: точное имя метода = `LoadWeightsAsync` (не `LoadWeights` — поэтому grep по `LoadWeights(` даёт 0). Объявлен `25.ModelServer/Contracts/IModelRepository.cs:8`, реализован `LocalModelRepository.cs:10`. Но ЕДИНСТВЕННАЯ runtime-точка, где промо-модель реально ГРУЗИТСЯ на исполнении — `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:304` (`DesignConversionPromotedLocalModelLoader.LoadAsync`), и она целиком в домене **design-conversion** (`44.DesignConversionDomain`), НЕ wiki. В `42.ProjectWikiDomain` load-точек promoted-модели = **0**.
- В `42.ProjectWikiDomain` все совпадения load-подобных имён (31 по 17 файлам) = либо 12 `*ManifestPublisher.cs` (только `UploadAsync`/`RegisterAsync` через `IModelRepository`/`IRemoteUnitLocator` — PUBLISH/EXPORT сторона; образец `ProjectWikiQueryPlanningManifestPublisher.cs:107` upload + `:111` register, причём публикация разрешена ТОЛЬКО после `PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED`, `:42-43`), либо тренировочные раннеры (`LoadCheckpoint` только в `ProjectWikiDpoMetricRunner.cs:137,146`, `OrchestrationPlanMachine.cs:415`, `OrchestrationPilotLoopRunner.cs:44`). Ни один — не на answer-пути.

**Итог остатка (не просто confirmed — refined):** wiring-gap = не «код неполный», а АРХИТЕКТУРНО РАЗОРВАННАЯ ПЕТЛЯ. Publish-сторона (12 publishers) кладёт манифесты в model-server; тренировочные раннеры сохраняют/грузят чекпойнты; но answer-сторона (`ProjectWikiDomainServiceCollectionExtensions.CreateDefault` → `new WikiContextRetriever/ProjectWikiQueryPlanningFlow/ProjectLocalWikiModelUnit/ProjectAnswerVerifier`, pass 7 `:20-23`) НИКОГДА не читает опубликованный манифест обратно. Доказательство, что это РЕШАЕМО (а не «фича невозможна»): соседний домен design-conversion уже имеет рабочую runtime-load-точку (`DesignConversionModelServerApi.cs:304`) — тот же `IModelRepository`, тот же паттерн promoted-local-model. Wiki-домену нужно ровно это ребро: `manifest → LoadWeightsAsync → construct unit from checkpoint` вместо `new`.

**Бонус-уточнение к pass 1:** `26.TrainableRouter` — НЕ «пока не собран отдельным подпроектом», а РЕАЛЬНЫЙ собранный подпроект: `Contracts/{IUnitScorer,ITrainableRoutingFlow,IRoutingDecisionLog}.cs`, `Training/{TrainableRoutingFlow,RoutingTrainingRunner,RoutingProfileTrainer,DistributedRoutingBootstrap,OrchestrationTaskDecompositionFlow,CodeDocumentationTrainingRunner}.cs`, `Models/RoutingModels.cs`. Конкретный scorer = `KeywordUnitScorer.cs` (keyword-based) — т.е. и routing-инфраструктура есть, но её боевой scorer детерминирован и в wiki-runtime не подключён. Усиливает R4-вывод pass 7. (`25.ModelServer` тоже реально собран: `Contracts/IModelRepository.cs`, `Local/LocalModelRepository.cs`, `DesignConversion/DesignConversionModelServerApi.cs`, `Remote/InMemoryRemoteUnitLocator`.)

### 2. Синтез папки #2 — одна исполняемая картина для плана «continue extensions»

Это НЕ папка одних планов — это полу-построенный продукт «project-local wiki» с РЕАЛЬНЫМ собранным хуком, РЕАЛЬНОЙ обучающей инфраструктурой и ОДНОЙ ясной точкой разрыва. Пять слоёв:

**(a) Реальный host-entrypoint (строить ОТ него, не от плана):**
- Собранный бинарь = `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs` (console app), НЕ план-имя `42.WikiHookRunner`/`wiki-context-hook.exe`; фабрики `WikiOrchestratorFactory` в коде НЕТ (pass 7, 0 совпадений).
- Вход: `new ProjectWikiPluginOrchestrationAdapter()` (`Program.cs:12`, дефолт-ctor БЕЗ оркестратора → это и активирует fallback) → `adapter.OrchestrateAsync(request)`.
- Контракт I/O: ENV `XIIGEN_ORCHESTRATION_REQUEST`(JSON) / `HOOK_PROMPT` + `XIIGEN_PROJECT_ROOT` + `XIIGEN_LINKED_REPO_PATHS`; выход = машинно-стабильный 7-секционный `Status: ORCHESTRATED` (pass 5 §B: понял-так / вижу-в-проекте / как-вести-работу / вопросы-к-RAG / что-нельзя-замазывать / список-units) — готов к отрисовке в панели VS/VSCode (аккордеон + trace).
- `--ensure-ready` = ЗАГЛУШКА (всегда `isReady=true`, `PLUGIN_ORCHESTRATION_HOOK_AVAILABLE`, `Program.cs:24-35`) — манифест НЕ читает. Первый honest-gap «настоящего» readiness.
- Extension→core (R6 simple-agent boundary СОБЛЮДЁН): тонкий адаптер (`hookRunner.ts`/`Program.cs`) → ENV/JSON → `ProjectWikiPluginOrchestrationAdapter` в `llm_mvp_core`. Вся логика в ядре, агент = тонкий транспорт.

**(b) Дизайн vs реальность оркестрации:**
- ДИЗАЙН (корпуса #18 + диаграмма): 9-node LLM Dynamic Flow (Domain Splitter → Translator → Scheme Planner → Processor → Applier → Result Translator → Arbiter Selector → Arbiter Units → Formatter) + 4-cycle chain (Planner→Convergence→Depth→Executor→Meta).
- РЕАЛЬНОСТЬ (pass 5/7): вырожден до 6 детерминированных Bootstrap-юнитов через `BOOTSTRAP_FALLBACK` (`ProjectWikiPluginOrchestrationAdapter.cs:141-161`; активен, потому что `Program.cs:12` конструирует адаптер без оркестратора). Node2–8 ОТСУТСТВУЮТ: нет доменной трансляции, scheme-планирования, multi-model apply, арбитража. Работает (детерминированно): приём промпта, статический project-scan, наивный fragment-splitter по пунктуации (`queues`/`redis`/`music` как «части»), evidence-вопросы, abstention-эвристика, 7-секционный шаблон. НЕ работает: пер-промптный RAG (evidence фиксирован на GenieDynamicApp/Server), семантический domain-split, обученный слой.

**(c) Обучающий конвейер (11 юнитов + сессии + корпуса) и ЕДИНСТВЕННЫЙ блокер:**
- 11 trainable wiki-юнитов (pass 2/3): реальный DPO-lifecycle в коде (`*DpoGenerator`/`*TrainingPipeline`/`*TrainingRunner`/`*ManifestPublisher`), каждый достиг per-unit PROMOTED/PROMOTED_MILESTONE (реальные checkpoint/manifest/metrics 2026-05-19…22) + 2 real-project validation gate PASSED (5 проектов, 616 evidence, 6393 вопроса, exact_evidence 1.0).
- НО 6-сессионный build-конвейер стоит на SESSION-01; SESSION-02..06 = NOT_STARTED (STATE.json); у ВСЕХ 11 юнитов `full wiki done: false` — интегрированный Mode-2 wiki-продукт end-to-end НЕ принят.
- ЕДИНСТВЕННЫЙ живой блокер (STATE.json `last_gate`/`next_action`): query-planning batch-030 метрика `answer_without_intermediate_analysis = 0.6 < 0.75` (структурный deterministic gate ПРОШЁЛ, DPO-watch-метрика — нет) → remediation перенесён на `WikiTraceAggregator` (обучить cross-unit trace-порядок QueryPlanning→ContextRouting→AnswerComposer, блок «answer-before-evidence»). Base-модель (session-01-batch-003) свой DPO-gate прошла; 12/12 ProjectWikiDomain тестов зелёные — надёжная стартовая точка.
- Корпуса-канон: 1000-**v8** (30 доменов) как orchestration-advisor корпус + seed (20 ручных доменов) как человеческая карта; реальные цепочки = chains-**v2**; тест-гейт-раннер = `Invoke-ProjectWikiPhaseReport.ps1` (Path-A измеритель, честно ограничен `test_result_only_not_training_promotion`).

**(d) SQL/multi-language расширение (из zip, pass 8 — уникальный материал):**
- SQL Phase-2 (`wiki-sql-phase2-plans-v2.zip`): consent-gated извлечение stored procedures/views/triggers из ЖИВОЙ БД → SQL-wiki-домен в `ProjectLocalWikiOrchestrator` с теми же evidence-id цитатами, что и код-вопросы. Компоненты под `42.ProjectWikiDomain/Sql/` (`ISqlConnectionExtractor`/`ISqlConsentGate`/`ISqlObjectExtractor`/`ISqlDocumentationOrchestrator`). **Код УЖЕ частично готов:** `ProjectLocalWikiModelUnit` уже РАЗЛИЧАЕТ SQL/stored-procedure вопросы (`ProjectLocalWikiModelUnit.cs:79-92` developer-reasoning для SP, `:146-170` `MentionsDatabase`/`IsDatabaseEvidence` через `StoredProcedureName`/`SqlParameters`/`SqlTablesRead`/`SqlTablesWritten`) — значит evidence-модель уже несёт SQL-поля, останется extractor + consent-gate.
- Per-stack language-plans (`wiki-training-language-plans-v1.zip`): Angular / AngularJS-ОТДЕЛЬНО (чтобы не отвечать на AngularJS языком Angular 2+) / Java(Spring Boot) / SQL; two-layer (pre-train на open-source reasoning → validate на коде проекта). Section 20 (#16): English promoted, Python/TS/Java/Rust PENDING.
- Монетизация (`PLAN-XIIGEN-LICENSING-v1`): Free = .NET домен (`42.ProjectWikiDomain`), Paid = Angular/AngularJS/Java/SQL через `LicenseManager.CheckDomain` + `DomainLoader` + CDN signed URL.

**(e) Единственная самая крупная вещь к починке = training-vs-runtime wiring gap (§1).** Пока `CreateDefault`/адаптер не читают опубликованный манифест и не передают зарегистрированный `ILogicalOrchestrator`, extension НАВСЕГДА отдаёт Bootstrap-вывод — сколько бы юнитов ни обучилось (весь per-unit training невидим на runtime). Минимальный product-spine (3 ребра): **(1)** строить от реального `WikiContextHook/Program.cs` + `ProjectWikiPluginOrchestrationAdapter`, НЕ от несуществующего `WikiOrchestratorFactory`; **(2)** закрыть wiring-gap `manifest → LoadWeightsAsync → construct-unit-from-checkpoint` (образец рядом: DesignConversion `DesignConversionModelServerApi.cs:304`); **(3)** передать зарегистрированный `ILogicalOrchestrator` (`LogicalOrchestratorAssistant.cs:130-136` уже регистрирует `ProjectWikiOrchestrationFlow`) в адаптер, выйдя из `BOOTSTRAP_FALLBACK` — но с учётом, что даже «proper» путь сам сейчас терминируется на Bootstrap (`ProjectWikiOrchestrationFlow.cs:79`), поэтому ребро (3) полезно ТОЛЬКО вместе с (2).

### 3. Cross-folder мосты (папка #2 из 12)

- **`orchestrator experiment` (9-node E53 training rounds).** #2 ДАЁТ: дизайн (9-node LLM Dynamic Flow #18, 4-cycle chain, корпус v8, baseline-отчёты #46/#47 с 19 промптами ×2 конфига) + приёмный код-слот (`OrchestrationPlanMachine`/`PromptFlowGroundingModel`, Phase 37/v20, с `LoadCheckpointAsync` `:415`). #2 БЕРЁТ: собственно E53 обучающие раунды, которые произведут чекпойнты для орк-модели (Node2–8) — то, что заменит Bootstrap. Мост: результат тех раундов должен приземлиться через publisher→manifest→(новое ребро load)→адаптер.
- **`dpo legal build` (DPO/legal план).** #2 ДАЁТ: 11-юнит DPO-паттерн (chosen = многошаговый evidence-план, rejected = single-lookup с `[WRONG:]`), лицензионный гейт (ACCEPT MIT/Apache/BSD; QUARANTINE ShareAlike; BLOCK GPL/AGPL/книги/SO/Wikipedia) и source-consent gate. #2 БЕРЁТ: конвертацию supervised A→B корпусов (v8) в DPO — base-модель провалилась (0.286) отчасти из-за недостаточности hash-token DPO-раннера; `dpo legal build` — план этой конвертации/легального сбора.
- **`language translate learning`.** #2 ДАЁТ: per-stack language-plan паттерн (two-layer generic-then-validate, правило AngularJS-отдельно) + требование multi-language (Section 20 PENDING). #2 БЕРЁТ: реальную translation/multi-language обучающую способность. Общий блокер обоих — checkpoint-persistence (см. CLAUDE.md «Translation checkpoint writer guard») + одинаковая two-layer дисциплина.
- **`design to code session` — САМЫЙ сильный мост для wiring-gap.** #2 БЕРЁТ: уже РАБОТАЮЩИЙ у design-conversion runtime-load паттерн (`DesignConversionPromotedLocalModelLoader.LoadAsync`, `DesignConversionModelServerApi.cs:304`; `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:78`) — это reference-реализация РОВНО того ребра, которого #2 не хватает (тот же `IModelRepository`/25.ModelServer). #2 ДАЁТ design-to-code: wiki/orchestration-контекст (figma→система = сценарий #1 в `orchestrator prompts.txt`, cross-stack evidence-модель).
- **`compare branches` (RAG-01).** RAG-01 установил: local-wiki поглощён активными `42.ProjectWikiDomain` + `43.LearningLocalWikiData`, host-facing точка = `ProjectWikiOrchestrationFlow`/`SubmitUserTaskApi`. #2 ПОДТВЕРДИЛ и УТОЧНИЛ кодом: развилка `BOOTSTRAP_FALLBACK` живёт в доменном адаптере (`ProjectWikiPluginOrchestrationAdapter.cs:141-161`), а НЕ в flow; реальный хук = `WikiContextHook`, не `42.WikiHookRunner`. #2 ДАЁТ RAG-01 точные `file:line`; RAG-01 ДАЁТ #2 карту веток/раскладки проектов.
- **`external-sources`.** #2 ДАЁТ: лицензионно-consent гейт + конкретные ACCEPT-репо для generic pre-training (`dotnet-architecture/eShopOnWeb`, `ardalis/CleanArchitecture`, `JasonGT/NorthwindTraders`, `Netflix/conductor`, `temporalio/samples-dotnet`, `MassTransit`, `TruthfulQA`) и validation-only список (DNN/старый nopCommerce/SmartStoreNET). #2 БЕРЁТ: собственно внешний обучающий материал generic doc-reasoning ДО валидации на коде проекта — база провалила held-out QA частично из-за отсутствия такого generic-корпуса.

_(остальные из 12 папок — не в фокусе этого задания parent; мосты выше покрывают все, названные parent как релевантные folder #2.)_

### 4. Открытые вопросы для Luba (с рекомендованными дефолтами)

1. **Host-entrypoint reconciliation.** План говорит `42.WikiHookRunner` + `WikiOrchestratorFactory.Create`; код = `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs` + `new ProjectWikiPluginOrchestrationAdapter()`. Какое имя каноническое для плана «continue»? **Дефолт:** строить от РЕАЛЬНОГО `WikiContextHook` + адаптера; план-имена `42.WikiHookRunner`/`WikiOrchestratorFactory` пометить SUPERSEDED (иначе executor будет искать несуществующий проект).
2. **Ship Mode 3 (BM25) сначала или ждать Mode 2 (semantic)?** Mode 3 (`wiki-context-hook-mode3.exe` ~3MB) собирается и работает офлайн БЕЗ весов; Mode 2 требует непринятой модели + активного embedding. **Дефолт:** ship Mode 3 первым (Stage A) как честный evidence-lookup extension (confidence 0.82, честные цитаты), Mode 2 — после integrated «wiki done».
3. **Закрыть wiring-gap СЕЙЧАС или доучить сначала?** Gap = `CreateDefault` не грузит манифест, поэтому даже уже-обученные per-unit чекпойнты невидимы (extension отдаёт Bootstrap). **Дефолт:** закрыть wiring-gap ПЕРВЫМ (разблокирует ВЕСЬ уже проделанный per-unit training за один шаг), взяв паттерн design-conversion `:304`; иначе дальнейшее обучение не меняет вывод расширения.
4. **Выйти из BOOTSTRAP_FALLBACK — передать `ILogicalOrchestrator` в адаптер?** `LogicalOrchestratorAssistant.cs:130-136` уже регистрирует `ProjectWikiOrchestrationFlow`, но `Program.cs:12` его не передаёт. **Дефолт:** да, передать — но учесть, что даже «proper» путь сейчас тоже терминируется на Bootstrap (`ProjectWikiOrchestrationFlow.cs:79`), поэтому это ребро полезно ТОЛЬКО вместе с вопросом 3 (иначе смена пути ничего не меняет по существу).
5. **Multi-language приоритет после .NET?** Section 20: English promoted; Python/TS/Java/Rust PENDING; zip даёт Angular/AngularJS/Java/SQL. **Дефолт:** SQL первым (evidence-модель уже несёт SQL-поля; Phase-2 план v2 готов; Paid-домен #1 по ценности для legacy-impact вопросов) → затем Angular (frontend-половина cross-stack вопросов, `IsClientEvidence` уже есть в коде).
6. **SQL domain scope — читать ЖИВУЮ БД?** SESSION-07 требует consent-gated извлечение из живой БД (connection strings → per-DB разрешение → extract SP/views/triggers) — чувствительно (доступ к БД пользователя). **Дефолт:** строить `42.ProjectWikiDomain/Sql/` с жёстким `ISqlConsentGate` (default DENY); начать со static `.sql`-file parsing в репозитории; live-DB подключение — только по отдельному явному разрешению Luba per-connection.
7. **query-planning batch-030 блокер — путь?** STATE.json: `answer_without_intermediate_analysis=0.6<0.75`, `next_action=WikiTraceAggregator`. Продолжать remediation этого юнита или пометить «per-unit достаточно, integrated-gate позже»? **Дефолт:** это НЕ блокер для ship Mode 3 (BM25 не зависит от trained query-planning); для Mode 2 — продолжить WikiTraceAggregator remediation как в STATE.json, но НЕ держать весь extension-план заложником этой одной DPO-watch-метрики.

**Честная строка покрытия pass 9 (кумулятивно):** глубоко закрыт код-остаток — прочитаны целиком `WikiContextRetriever.cs`, `ProjectLocalWikiOrchestrator.cs`, `ProjectLocalWikiModelUnit.cs`, `ProjectWikiQueryPlanningManifestPublisher.cs` + repo-wide grep load-точек (подтверждено: 0 runtime-load в 42.ProjectWikiDomain answer-пути; единственная load-точка репо = DesignConversion `:304`). С учётом passes 1–8 — все 51 файла папки покрыты + код `42.ProjectWikiDomain`/`15.OrchestratorModel`/`25.ModelServer`/`26.TrainableRouter` сверен read-only. Ни один файл не изменён, кроме этого RAG (append-only); git не трогался; мойибейк не правился.

**RAG-файл (обновлён append-only pass 9):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`

---

## Pass 10/10 — completeness sweep + executive summary (folder #2/12 orchestrator-extension)

**Статус pass 10 (FINAL):** это завершающий проход. Сделано: (1) перечитан весь RAG-файл (960 строк, passes 1–9) и сверен против манифеста pass 1; (2) `find -type f` подтвердил РОВНО 51 файл в папке — совпадает с манифестом 1:1, ни одного лишнего/пропущенного; (3) закрыт последний «head-covered» файл — `base-wiki-training-plan.md` (#31) и `base-wiki-heldout-evaluation.json` (#29) прочитаны ПОЛНОСТЬЮ в этом проходе, чтобы ни один файл не остался только манифест-строкой. READ-ONLY соблюдён (git не трогался; изменён только этот RAG-файл, append-only; мойибейк в #14/#16 не правился).

### 1. Completeness sweep — все 51 файла, покрытие по группам

Сверка: манифест pass 1 (строки 30–82) нумерует файлы 1–51; `find` дал ровно те же 51 (48 top-level + `reports/` ×2 + `tools/` ×1). Ниже — покрытие по группам с честным остатком.

| Группа | Файлы (манифест #) | Где разобрано | Статус |
|---|---|---|---|
| **A. Host-entrypoint + packaging + licensing** | #5 PLAN-PLUGIN-PACKAGING-v1, #6 …to-test-with-VS, #7 PLAN-XIIGEN-LICENSING-v1 | pass 1 (deep) + pass 2 (точный `diff`) + pass 7/9 (код-граундинг: реальный хук = `WikiContextHook`, НЕ `42.WikiHookRunner`) | **COMPLETE** |
| **B. Мастер-архитектура + распределённая модель** | #2 PROJECT-LOCAL-WIKI-MODEL-PLAN-v1, #3 DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1 | pass 1 (deep) | **COMPLETE** (#3 помечен SUPERSEDED→v20) |
| **C. Executor/training-протоколы** | #4 QUERY-PLANNING-FLOW, #8 GAP-CLOSURE, #9 GAP-VALIDATION-EXAMPLES, #10 CODE-TRACE, #11 DONE-PROTOCOL, #12 DISCIPLINE-RULES, #13 STAGE-PROMPT-v2, #14 STAGE-PROMPT-v1, #15 ORCH-TRAINING-v1, #16 LIVING-GUIDE (~80KB), #17 RUNNER-REPORTING, #19 PROBLEM-LIST | pass 2 (все тела; living-guide целиком) + pass 3 (#16 Section 18 юниты 6–11) | **COMPLETE** (#14 superseded+мойибейк; #15/#16/#19 = IMPLEMENTED Phase 37) |
| **D. Execution sessions + STATE** | #20–#25 SESSION-01…06, #26 STATE.json | pass 3 (тела целиком; STATE все 574 строки) | **COMPLETE** |
| **E. Base-model артефакты** | #27 manifest, #28 acceptance-report, #29 heldout-evaluation.json, #30 evaluator-gap, #31 training-plan | pass 1 (deep #27/#28/#30) + **pass 10 (#29 и #31 прочитаны ПОЛНОСТЬЮ)** | **COMPLETE** |
| **F. Extraction protocol (280KB)** | #18 ORCHESTRATION-EXTRACTION-PROTOCOL-v2 | pass 4 (deep) | **COMPLETE с честным остатком**: тело 1–251 + 676–2600 дословно; per-FLOW форензик-каталог (251–585) через header+grep (повторяющийся шаблон; таксономия/метод/output-shape взяты полностью) |
| **G. Reports (реальные выходы)** | #46 EXACT-PROMPTS-ACTUAL-RESULTS (1429 стр.), #47 PROMPT-ACTUAL-OUTPUTS (1264 стр.) | pass 5 (оба целиком) | **COMPLETE** |
| **H. Корпуса оркестрации** | #35 EPISODES-1000-v8, #36–#42 v1–v7, #43 EPISODES-v1 seed, #44 chains-v2, #45 chains-v1 | pass 6 (#43/#44 целиком, #35 sampled) + pass 8 (v1–v7 diff, chains-v1 diff) | **COMPLETE с честным остатком**: #35 v8 — прочитано ~15 из 1000 кейсов + полный формат + grep 30 доменов (тело маршрута = ~30 шаблонов, не 1000 уникальных reasoning; полное построчное чтение продукт-ценности не добавит); v1–v7 — sampled, подтверждено «уникального материала против v8 нет» |
| **I. Диаграммы** | #32 drawio.xml, #33 Negative-DTO.xml, #34 drawio.zip | pass 1 (#32 deep) + pass 8 (#33 целиком, #34 распакован) | **COMPLETE** (#34 = старый избыточный снимок папки) |
| **J. Prompts + tools + zip-архивы** | #1 orchestrator prompts.txt, #48 Invoke-ProjectWikiPhaseReport.ps1 (197 стр.), #49 sql-phase2-v1.zip, #50 sql-phase2-v2.zip, #51 language-plans.zip | pass 1 (#1 deep) + pass 8 (#48 целиком, 3 zip распакованы+прочитаны) | **COMPLETE** (#50/#51/#49 держат УНИКАЛЬНЫЙ материал: SQL Phase-2 + per-stack language-планы) |

**Итог sweep: 51/51 файлов покрыты. НИ ОДНОГО неохваченного файла.** Каждый имеет манифест-строку (pass 1) + глубокий или sampled разбор (passes 1–10). Единственные «честные остатки» — тело #18 per-FLOW каталога (повтор форензик-шаблона) и полное построчное чтение 1000 кейсов #35 v8 и всех 7000 кейсов v1–v7 (доказано: reasoning-тело = ~30 доменных шаблонов, дубли подтверждены дословно на кейсах 0500=0950). Оба остатка НЕ содержат непокрытого метода/таксономии/контракта/product-материала.

**Новое из pass-10 дочтения (#31 base-wiki-training-plan.md + #29 heldout-evaluation.json) — усиливает честность плана, не меняет выводов:**
- #31 даёт полную **12-видовую pair-таксономию базовой модели** (`generic_symbol_answer`/`generic_caller_answer`/`generic_workflow_answer`/`generic_query_plan`/`generic_entity_impact_plan`/`generic_feature_answer`/`citation_required`/`missing_evidence_abstain`/`wrong_project_reject`/`stale_documentation_reject`/`stack_version_preserve`/`wrong_level_reject`), каждая с chosen/rejected + обязательным `[WRONG: ...]`. Правило: базовая модель обучается ГЕНЕРИЧЕСКИ, без фактов `askapediaweb`/`GenieDynamicServer`/`wizGameserver`/`genietasks`. Runner тот же (`InternalTrainReadyDpoRunner --context 96 --batch-size 16 --save-every 25`), и файл САМ признаёт ключевое ограничение: «runner тренирует DPO-строки, но НЕ делает held-out NL Q/A generation, citation verification, abstention evaluation — нужен отдельный evaluator». Это дословный источник training-vs-runtime разрыва на уровне base-model.
- #29 подтверждает дословными числами: `status=HELDOUT_QA_FAILED_QUERY_PLANNING_NOT_VALIDATED`; `generic_documentation_qa_accuracy=0.2857`, `hallucination_rate=0.68`, `query_plan_accuracy=null`; три hard-failure кода: `HELDOUT_QA_BELOW_THRESHOLD`, `QUERY_PLANNING_FLOW_NOT_VALIDATED`, **`HASH_TOKEN_RUNNER_NOT_GENERALIZING`** («runner улучшает in-batch метрики, но не улучшает held-out Q/A preference после повторного показа»). При этом `citation_format=1.0`, `missing_evidence_abstention=1.0`, `answer_contract=1.0`, DPO-gate `final_accuracy=1`, `ProjectWikiDomainTests 12/12 passed`. Т.е. база честно «определена, но не принята», блокер — недостающий валидированный `ProjectWikiQueryPlanningFlow` + недостаточность hash-token DPO-раннера. Полностью сходится с pass 1/7/9.

### 2. ИСПОЛНИТЕЛЬНОЕ РЕЗЮМЕ ПАПКИ #2 (можно читать ТОЛЬКО это)

**Что это за папка и что она даёт плану «продолжить VS / VS Code расширения».** «orchestrator extension» — это не пачка планов, а **полу-построенный продукт «project-local wiki model»**: локальная модель документации кода + весь её обучающий/оркестрационный контур + реальный host-контракт, через который расширение дёргает ядро `llm_mvp_core`. Ключевая находка для плана: **реальная точка входа расширения — это собранный консольный хук `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs`**, который конструирует `new ProjectWikiPluginOrchestrationAdapter()` и зовёт `adapter.OrchestrateAsync(request)` (вход через ENV `XIIGEN_ORCHESTRATION_REQUEST`/`HOOK_PROMPT` + `XIIGEN_PROJECT_ROOT`). Плановые имена `42.WikiHookRunner` / `wiki-context-hook.exe` / `WikiOrchestratorFactory.Create` (из `PLAN-PLUGIN-PACKAGING-v1.md`) в коде **НЕ существуют** (repo-wide grep = 0) — это ДИЗАЙН, а не собранный артефакт. Строить план «continue» надо от реального `WikiContextHook` + адаптера, иначе executor будет искать несуществующий проект. Граница R6 «simple agent» соблюдена: агент = тонкий транспорт (ENV/JSON), вся логика в ядре.

**Дизайн оркестрации vs реальность рантайма.** ДИЗАЙН (в корпусах #18 + диаграмме `orchestrator wiki.drawio.xml`) — это богатый **9-node LLM Dynamic Flow** (Domain Splitter → Translator → Scheme Planner → Processor → Applier → Result Translator → Arbiter Selector → Arbiter Units → Formatter) + 4-cycle chain. РЕАЛЬНОСТЬ (доказано телом отчётов #46/#47 pass 5 и кодом pass 7): рантайм вырожден до **6 детерминированных Bootstrap-юнитов через `BOOTSTRAP_FALLBACK`** (`ProjectWikiPluginOrchestrationAdapter.cs:141-161`, активен потому что `Program.cs:12` строит адаптер без оркестратора). Расширение УЖЕ стабильно отдаёт машинно-читаемый **7-секционный ответ** (`Status: ORCHESTRATED` + понял-так / вижу-в-проекте / как-вести-работу / вопросы-к-RAG / что-нельзя-замазывать / список-units) — 19/19 founding-промптов без падений, готов к отрисовке в панели VS/VSCode. НО Node2–8 отсутствуют: нет доменной трансляции, scheme-планирования, multi-model apply, арбитража; project-evidence фиксирован (скан GenieDynamicApp/Server, не пер-промптный RAG); domain-split — наивный по пунктуации (`queues`/`redis`/`music` как «части»). Это Bootstrap-scaffold, а не обученная модель, и это НЕЛЬЗЯ выдавать за «фичу».

**Обучающий конвейер (11 юнитов) и единственный живой блокер.** 11 обучаемых wiki-юнитов имеют РЕАЛЬНЫЙ DPO-lifecycle в коде (`*DpoGenerator`/`*TrainingPipeline`/`*TrainingRunner`/`*ManifestPublisher`) и каждый достиг **per-unit PROMOTED / PROMOTED_MILESTONE** с настоящими checkpoint/manifest/metrics (2026-05-19…22) + ДВА real-project validation-gate PASSED (5 проектов, 616 evidence, 6393 вопроса, exact_evidence 1.0) — это задокументировано в `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` Section 18. НО у ВСЕХ 11 юнитов стоит **`full wiki done: false`**, 6-сессионный build-конвейер стоит на SESSION-01, а **SESSION-02..06 = NOT_STARTED** (`PROJECT-LOCAL-WIKI-MODEL-STATE.json`). Интегрированный Mode-2 wiki-продукт end-to-end НЕ принят. Единственный живой блокер (STATE.json `last_gate`/`next_action`): query-planning batch-030 метрика `answer_without_intermediate_analysis = 0.6 < 0.75` (структурный детерминированный gate ПРОШЁЛ, DPO-watch-метрика — нет) → remediation перенесён на `WikiTraceAggregator`. Базовая модель (`base-wiki-*`) свой DPO-gate прошла, но held-out Q/A провалила (0.286 при 0.90; halluc 0.68) — корневая причина = недостающий валидированный `ProjectWikiQueryPlanningFlow` + недостаточность hash-token DPO-раннера.

**Самая крупная и самая важная вещь к починке = training-vs-runtime wiring gap.** Обучающая инфраструктура (publish-сторона: 12 `*ManifestPublisher` кладут манифесты в `25.ModelServer`; тренировочные раннеры сохраняют чекпойнты) реальна, но **answer-сторона НИКОГДА не читает опубликованный манифест обратно**: `ProjectWikiDomainServiceCollectionExtensions.CreateDefault` инстанцирует юниты через `new ...()` без `IModelRepository.LoadWeightsAsync`/чекпойнта. Собственный self-trace юнитов дословно помечает их `deterministic-pretrain-surface`/`bm25-plus-optional-graphrag`/`deterministic-verifier`; confidence — фиксированные константы (0.88/0.82). Значит: **сколько бы юнитов ни обучилось, extension навсегда отдаёт Bootstrap-вывод, пока разрыв не закрыт.** Доказательство, что это РЕШАЕМО (а не «фича невозможна»): соседний домен design-conversion УЖЕ имеет рабочую runtime-load-точку `DesignConversionModelServerApi.cs:304` (`DesignConversionPromotedLocalModelLoader.LoadAsync`) — тот же `IModelRepository`, тот же паттерн. Wiki-домену нужно ровно это ребро: `manifest → LoadWeightsAsync → construct-unit-from-checkpoint` вместо `new`.

**Стратегия «Mode 3 first» + расширение на SQL и другие языки.** `wiki-context-hook-mode3.exe` (~3MB, только BM25 через `12.SearchEngine`) собирается и работает офлайн БЕЗ весов — его надо шипить ПЕРВЫМ как честный evidence-lookup extension (confidence 0.82, настоящие цитаты); Mode 2 (semantic, `08.GraphRag.HybridRetriever` + embedding) — только после integrated «wiki done». Уникальный product-материал из zip-архивов (pass 8): **SQL Phase-2** (`wiki-sql-phase2-plans-v2.zip`) — consent-gated извлечение stored procedures/views/triggers из ЖИВОЙ БД → SQL-wiki-домен с теми же evidence-id цитатами (компоненты под `42.ProjectWikiDomain/Sql/`; код УЖЕ частично несёт SQL-поля в `ProjectLocalWikiModelUnit`); и **per-stack language-планы** (Angular / AngularJS-ОТДЕЛЬНО / Java / SQL, two-layer generic-then-validate). Монетизация (`PLAN-XIIGEN-LICENSING-v1`): Free = .NET-домен, Paid = Angular/AngularJS/Java/SQL через `LicenseManager.CheckDomain` + `DomainLoader` + CDN.

**7 открытых вопросов для Luba (headline; полностью — в pass 9 §4 с рекомендованными дефолтами):** (1) host-entrypoint reconciliation — строить от реального `WikiContextHook`, план-имена `42.WikiHookRunner`/`WikiOrchestratorFactory` пометить SUPERSEDED; (2) ship Mode 3 первым или ждать Mode 2 → **дефолт Mode 3 первым**; (3) закрыть wiring-gap СЕЙЧАС или доучивать → **дефолт: закрыть gap первым** (разблокирует весь уже проделанный per-unit training одним шагом); (4) передавать ли `ILogicalOrchestrator` в адаптер (полезно ТОЛЬКО вместе с #3, т.к. даже «proper» путь сам терминируется на Bootstrap); (5) multi-language приоритет → **дефолт SQL первым, затем Angular**; (6) SQL scope — читать ли ЖИВУЮ БД → **дефолт: жёсткий `ISqlConsentGate` default-DENY, начать со static .sql**; (7) query-planning batch-030 блокер — **дефолт: НЕ держать весь extension-план заложником одной DPO-watch-метрики** (BM25 Mode 3 от неё не зависит).

**Must-read файлы (если читать не всё):** `PLAN-PLUGIN-PACKAGING-v1.md` (host-контракт, но сверять с кодом), `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md` (мастер-архитектура), `orchestrator wiki.drawio.xml` (визуальный контракт двух потоков), `PROJECT-LOCAL-WIKI-MODEL-STATE.json` (текущий gate + `next_action`), `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` Section 18 (per-unit training-ledger — единственное доказательство реального DPO-обучения), `reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md` (baseline фактических выходов, 19 промптов), `USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md` (канон корпуса) + `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` (дизайн 9-node), `wiki-sql-phase2-plans-v2.zip` + `wiki-training-language-plans-v1.zip` (расширение за .NET). Код-якоря (repo `C:/Projects/llm_mvp_core`): `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs:12`, `42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs:141-161`, `.../ProjectWikiDomainServiceCollectionExtensions.cs:20-23`, референс-ребро `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:304`.

### 3. Честный отчёт покрытия (folder #2/12)

- **Всего файлов: 51** (`find -type f` = 51; манифест pass 1 = 51; совпадение 1:1, 0 пропущено, 0 лишних).
- **Прочитано полностью (тело целиком):** ~40 файлов — все протоколы/планы/сессии (#2–#26 кроме sampled-корпусов), оба reports (#46/#47), STATE.json (574 стр.), living-guide (~80KB), #18 (существенно: 1–251 + 676–2600 дословно, середина header+grep), #43/#44 корпуса, #33 диаграмма, #48 PS1, содержимое 3 уникальных zip, и в этом проходе — #29 + #31.
- **Sampled (частично, с честным остатком):** #35 EPISODES-1000-v8 (~15 из 1000 кейсов + полный формат + grep 30 доменов; reasoning-тело = ~30 шаблонов, дубли подтверждены дословно); #36–#42 v1–v7 (header+формат+domain-diff — доказано «уникального против v8 нет»); #34 drawio.zip (распакован, сверен как старый снимок); #18 per-FLOW форензик-каталог (повтор шаблона).
- **Что НЕ запускалось (вне scope RAG):** НЕ было `dotnet build` / `dotnet test` / runtime-исполнения / запуска хука / запуска PowerShell-раннера (`phase-metrics/` в папке отсутствует → тул ни разу не гонялся; это спецификация, не готовые метрики). Код-сверка passes 7/9 — строго READ-ONLY (Glob/Grep/Read), git не трогался.
- **RAG-файл:** всего строк после pass 10 = **1017** (было 960 после pass 9; pass 10 добавил ~57 строк append-only). Путь: `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`.

**folder #2/12 RAG COMPLETE** — все 51 файла покрыты (манифест + deep/sampled), host-entrypoint заземлён в реальном коде, дизайн-vs-рантайм разрыв подтверждён телом кода, обучающий конвейер и единственный блокер зафиксированы, cross-folder мосты и 7 открытых вопросов с дефолтами готовы для плана «continue VS/VSCode extensions». READ-ONLY соблюдён; изменён только этот RAG-файл (append-only); мойибейк не правился; git не трогался.

**RAG-файл (обновлён append-only pass 10 — FINAL):** `C:\Users\lubao\Documents\xiigen\llm mvp\visual studio & visual studio code continue\RAG-02-orchestrator-extension.md`
