# Opus P43 — Sweep пропущенных сценариев/требований (missing scenarios/requirements)

Пасс 43 из 50. Оценщик: Opus 4.8 (REAL-STATE ASSESSOR).
Дата: 2026-07-08. Папка: `C:\Users\lubao\Documents\xiigen\llm mvp\extension development`.

## 0. Задача этого прохода

Пере-прочитать планы + `REQUIREMENTS-COVERAGE-MATRIX` и найти **требования/сценарии,
которые ещё НЕ представлены как дыры в мастере** (`opus-PROJECT-CLOSURE-PLAN.md`,
P01…P42). Не переоценивать заново уже покрытое (28-сценарный dual-path board уже разобран
P31–P34; def-of-done — P02; конвейеры/DPO/lifecycle — P21–P30; git/tests — P39–P40). Ищу
именно **целые классы требований/сценариев, отсутствующие в инвентаре дыр**.

Рамка честности (P01): COPIED_DOC_ONLY ≠ BUILT_NOT_PROVEN ≠ PROVEN. Каждый real-status —
с file:line из реального репо/артефакта или явным «not found (searched X)».

## 1. Что прочитано в этом проходе

Пакет:
- `REQUIREMENTS-COVERAGE-MATRIX.md` (10 строк — все про упаковочную команду), `EXTENSIONS-IMPLEMENTATION-STATUS-BOARD.md` (15 фаз), `PLANS-AND-SUPPORTING-FILES-INVENTORY.md`.
- `supporting-files/.../product-readiness-scenario-board.md` (канон 28 сценариев — все single-project, Claude/Cursor/Codex).
- `copied-plans/SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md` (Q6/Q7 cross-project) + `...UPLIFT-STATE.json` (статусы Q6-03/Q7-01/Q7-02/Q7-03/Q7-04).
- `copied-plans/API-DASHBOARD-PLAN-v1.md` (AR-01..AR-22).

Реальный репо (bounded grep/read, `C:\Projects\llm_mvp_core`):
- `43.LearningLocalWikiData\validation\ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json`, `ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json`.
- `42.ProjectWikiDomain\ProjectWikiDomainModels.cs` (+ DPO-генераторы question-классов).
- `CLAUDE.md` (Completion coverage matrix pattern: `:1375`, `:1388`).

Дедуп-скан мастера: grep `cross-project|second project|xiigen-platform|orchestrator prompts|prompt-suite|wiki question|advanced wiki|multi-project|target project|repository scope` →
попадания только P24 (счёт cross-project-fixtures = 8 строк) и P04 (`xiigen-platform` как ПОТРЕБИТЕЛЬ границы = обратное направление); ни одного из 4 требований ниже в реестре нет.

## 2. Ключевой вывод

Реестр P01–P42 глубоко разобрал **три хоста × single-project** и по-гейтам def-of-done.
Но вне поля зрения остались **целые измерения требований**, названные планами/CLAUDE.md:

1. **Мультипроектность** — расширение должно работать на ВТОРОМ target-project
   (`xiigen-platform-dot-net-angular`), cross-project transfer (Q6-03/Q7/QC-07). Доказано лишь
   как **report-only contract-eval** (не runtime), и полностью **отсутствует** в 28-сценарном
   dual-path board (там все ряды — один проект).
2. **Plugin-orchestration хост-семья (VS Code + Visual Studio project-wiki plugin)** — отдельная
   поверхность с prompt-suite и scenario-классами (`figma_to_full_system_orchestration` и др.);
   реестр/board трактуют «3 расширения = Claude/Cursor/Codex» и эту семью игнорируют.
3. **Advanced wiki question classes (14 подклассов)** — `42.ProjectWikiDomain`; CLAUDE.md
   completion-matrix требует покрытия каждого подкласса, не 7 верхних бакетов. Не инвентаризовано.
4. **Замкнутый local learning-loop как named-сценарий (Q6-04)** — наблюдение вывода AI → детект
   gap → DPO-строка → continue-training → held-out улучшается над pre-adaptive baseline. Нет
   ряда в board и нет runtime-доказательства.

Это НЕ переоценка кода — это дыры **инвентаря требований/сценариев**: соответствующие
требования нельзя «не выполнить», пока их вообще нет строкой в матрице/бордe.

## 3. Новые дыры P43 (детали)

### G43-1 — Мультипроектность / cross-project transfer (второй target-project)
- **Требование:** план `SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md:664` (Q6-03 «Same request
  contract works on second project; local preferences separated»), `:668-676` (Q7 cross-project
  transfer + end-to-end semantic proof), `:831` (QC-07 «Cross-project validation works … reports
  for `xiigen mvp` and `xiigen-platform-dot-net-angular`»).
- **Реально:** `...UPLIFT-STATE.json` Q7-01 `:300` status PASS, но `:302`
  `artifact_kind = report_only_common_assets_contract_evaluation_not_runtime_model_inference`,
  `:328` `model_decision_path_invoked:false`, `:329` `report_scope=...not_host_runtime_not_dashboard_api_not_full_product`;
  Q7-02 `:334` PASS, `:336` `report_only_common_plus_adaptive_..._not_runtime_model_inference`,
  `:372` `model_decision_path_invoked:false`; оценено всего **4 строки/режим (2 на проект)** (`:313`,`:349`);
  «adaptive improves local fit» = контракт-скор `2→4` (`:359-360`), не runtime-метрика.
- **Board:** `product-readiness-scenario-board.md:23` — 28 рядов, все single-project (Claude 9/Cursor 9/Codex 10); **0** рядов для второго проекта.
- **Дедуп:** P04 gap (`:79`) = платформа как ПОТРЕБИТЕЛЬ границы (0 кода вызова) — обратное направление; P24 (`:328`) только считает 8 fixture-строк. Само требование «расширение работает на N target-проектах» как измерение покрытия — новое.
- **Статус:** PARTIAL (report-only contract eval; not runtime; absent from board).
- **Closure:** ввести cross-project ряды в dual-path board (хост × второй проект); прогнать реальный runtime cross-project inference (Path A boundary + Path B host) на `xiigen-platform-dot-net-angular`; common-only vs common+adaptive доказать held-out метриками, не совпадением контрактов; зафиксировать разделение personal-ноги по project-root-hash.

### G43-2 — Plugin-facing orchestrator prompt-suite + scenario-классы (VS Code / Visual Studio wiki plugin)
- **Требование:** `CLAUDE.md` Post-completion challenge pattern — «plugin-facing prompt-suite gate
  executes prompts from orchestrator prompts.txt through the plugin entrypoint against the required
  repository scopes»; Completion coverage matrix — «every orchestrator prompt scenario class named
  by the plan is covered».
- **Реально (реальный репо):** `43.LearningLocalWikiData\validation\ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json:41-51`
  ожидает `xiigen.openWikiPanel`/`ShowWikiPanelCommand`/`CopyWikiContextCommand`/Copilot-chat-participant→`queryWiki`;
  `:99` «Visual Studio or VS Code user-facing entrypoint evidence»; `:164`
  `if_plugin_entrypoint_missing_block_plugin_orchestration_claim`. `ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json:6`
  `scenario_classes` = `figma_to_full_system_orchestration`, `figma_into_existing_system`,
  `system_from_user_explanation_without_complete_design`, `multi_user_diet_application_with_daily_feedback` (и др.).
  Host-точки входа отсутствуют (реестр P01/P09 — entrypoints MISSING) ⇒ по собственному правилу
  suite plugin-orchestration claim заблокирован.
- **Дедуп:** мастер grep `orchestrator prompts|prompt-suite|scenario_class` → 0. P09-P20 разбирают Claude/Cursor/Codex, не VS Code/Visual Studio wiki-plugin и не эти scenario-классы.
- **Статус:** SUITE EXISTS AS EXPECTED-RESULTS JSON / no plugin entrypoint, not run through host, absent from register+board.
- **Closure:** решить scope у Luba: если VS Code/Visual Studio project-wiki plugin в поставке — построить plugin-точки входа и прогнать suite через host command/panel/chat против названных repository scopes, покрыть каждый scenario_class; иначе явно вынести из scope с решением Luba. В любом случае добавить эту хост-семью и scenario-классы в инвентарь покрытия.

### G43-3 — Advanced wiki question classes (42.ProjectWikiDomain): 14 подклассов, не 7 бакетов
- **Требование:** `CLAUDE.md:1375` «every required advanced wiki question class and subtype is
  present, not only the top-level buckets»; `:1388` «seven category buckets when the plan requires
  fourteen underlying classes».
- **Реально (реальный репо):** `42.ProjectWikiDomain\ProjectWikiDomainModels.cs:95` `ProjectWikiQueryIntent`,
  `:459` `ProjectWikiRequestRouteKind` (`:462` `WikiQuestionAnswer` и др.); DPO-генераторы классов
  `ProjectWikiRequestRoutingDpoGenerator.cs`, `CrossStackBusinessFlowGraphDpoGenerator.cs`,
  `SystemBoundaryMapperDpoGenerator.cs`. `ProjectWikiOrchestrationFlow` (TD2) — часть deliverable
  (CLAUDE.md «Project wiki domain»).
- **Дедуп:** мастер grep `wiki question|advanced wiki` → 0. P21-P23 разбирают PromptToTask/OrchestrationReasoning/SkillSelection машины, не 42-доменные question-классы; ни один пасс не инвентаризовал wiki question classes/subtypes и не проверил ловушку «7 бакетов вместо 14 подклассов».
- **Статус:** CODE PRESENT (enums/DPO generators) / нет class-by-class coverage-матрицы.
- **Closure:** перечислить канонический список advanced wiki question классов/подклассов (проверить = 14); построить матрицу «класс → runtime-invoked ответ + evidence»; поймать «7 vs 14»; внести в def-of-done как отдельные строки покрытия.

### G43-4 — Замкнутый local learning-loop как named-сценарий (Q6-04)
- **Требование:** `SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md:665` (Q6-04): «AI output observed →
  gap/correction detected → DPO row generated with loaded guides → adaptive model continue-training →
  local held-out scenario improves over pre-adaptive baseline». Это ядро adaptive-value-prop.
- **Реально:** во всём репо adaptive-принятых DPO-строк для skill-selection ≈ 0 (P24: 4 файла
  `adaptive-dpo-signals.jsonl` по 1 синтетической строке, для skill-selection — 0); ingestion
  наблюдённого вывода AI (feedback) не реализован (P11/P14: grep `feedback`→0); ни одного ряда
  замкнутого learning-loop в 28-board.
- **Дедуп:** P24 (строки), P25 (continue-training = счётчик), P11/P14 (feedback unhandled) —
  фрагменты цепочки на code-уровне. НОВОЕ здесь: сам **named end-to-end сценарий** «наблюдение→
  улучшение над pre-adaptive baseline» отсутствует как строка инвентаря покрытия/board — то есть
  даже не отслеживается, доказан ли центральный adaptive-цикл.
- **Статус:** PLAN-NAMED SCENARIO / нет ряда в board, нет runtime-доказательства замкнутого цикла.
- **Closure:** добавить Q6-04 замкнутый learning-loop как отдельный dual-path сценарий (per host); реализовать ingestion наблюдённого вывода AI → детект gap → DPO → continue-training; доказать held-out ПОСЛЕ > pre-adaptive baseline (числом), Path A + Path B.

## 4. Вывод P43

Найдены 4 НОВЫХ дыры — не переоценка кода, а **пропущенные измерения инвентаря
требований/сценариев**: (1) мультипроектность/cross-project transfer (report-only, вне board),
(2) plugin-orchestration хост-семья VS Code/Visual Studio + orchestrator scenario-классы (вне
реестра/board), (3) advanced wiki question classes 14-vs-7 (не инвентаризовано), (4) замкнутый
Q6-04 learning-loop как named-сценарий (нет ряда/ runtime). Все — с реальными file:line из
репо/артефактов и явным дедупом против P02/P04/P11/P14/P21-P25/P31-P34. Проект НЕ завершён.
