# Codex Recon — Архитектура расширений (Claude/Cursor/Codex/VS/VSCode): как зовётся API ядра

Корпус: `mvp` · Slug: `mvp-extensions` · Релевантность: medium
Под-задача: как расширения вызывают ядро (имена API), для плана ILearningDynamicGraph.
Статус evidence: это планы расширений (REFERENCE / DO-NOT-EXECUTE), не исполняемый код.

---

## Что это и где (точные ключевые пути)

Целевая папка содержит в основном `.zip` (не читаемы рекон-инструментами) и набор планов `.md`.
Прочитаны 4 самых релевантных под вопрос об API ядра:

- `C:/Users/lubao/Documents/xiigen/llm mvp/extensions/extracted/PLAN-XIIGEN-WIKI-VSCODE-v1.md`
  — VS Code / Cursor / Windsurf расширение (один `.vsix`).
- `C:/Users/lubao/Documents/xiigen/llm mvp/extensions/extracted/PLAN-XIIGEN-WIKI-VISUAL-STUDIO-v1.md`
  — Visual Studio 2022 расширение (VisualStudio.Extensibility, out-of-process).
- `C:/Users/lubao/Documents/xiigen/llm mvp/extensions/PLAN-XIIGEN-SMART-CODE-WIKI-PHASE-1-v1.md`
  — Phase-1 для всех платформ (Claude Code / Cursor / Codex / VS); общий контракт и addendum 2026-05-24.
- `C:/Users/lubao/Documents/xiigen/llm mvp/extensions/XIIGEN-PRODUCT-DESCRIPTION-v1.md`
  — продуктовое описание (что за продукт, какие IDE, продуктовые линии).

Не прочитаны (нерелевантны вопросу или нечитаемы): zip-архивы
`plugin-plans-*.zip`, а также `PLAN-SMART-CODE-WIKI-PHASE1-ALL-18-PLATFORMS-v1.md`
и `PLAN-XIIGEN-SMART-CODE-WIKI-PHASE-1-v1.md`-дубликат логики «18/19 платформ»
(защита от перегруза — те же контракты, что уже извлечены).

---

## Как зовётся API ядра (главный ответ)

Есть ДВА транспорта к одному ядру `42.ProjectWikiDomain` в `llm_mvp_core`:

### 1) Subprocess-API (Claude Code / Cursor / Codex / VS Code) — `wiki-context-hook.exe`
Один и тот же бинарь для всех трёх AI-CLI. Источник: `41.CodeWiki/HookRunner/Program.cs`
(имя проекта `41.CodeWiki` в Phase-1) / `wiki-context-hook.exe`. Контракт вызова:
- Mode 2 (инъекция в промпт): без аргументов, вход через env `HOOK_PROMPT`,
  `XIIGEN_PROJECT_ROOT` / `XIIGEN_SOLUTION_PATH`; выход stdout JSON `{ "systemPromptAddition": "..." }`.
- Mode 3 (карта кода): `--map <query>` → JSON `WikiMapResult { entries[], dataFlows[] }`.
- Готовность: `--ensure-ready --json` → `WikiReadinessResult { isReady, status, documentationVersion, entryCount, nextAction }`.

Обёртка на стороне VS Code (`src/hookRunner.ts`) — три функции:
`queryWiki(prompt)`, `getMap(query)`, `checkReadiness()`. Точки входа хоста:
`vscode.chat.createChatParticipant('xiigen.wikiContext', ...)` (инъекция контекста),
`WebviewViewProvider` (панель), команды `xiigen.copyWikiContext` / `xiigen.showMap`,
status bar. Конфиги хуков: Claude `hooks.json` (`UserPromptSubmit`),
Cursor `.cursor/hooks.json` (`beforeSubmitPrompt`/`sessionStart`),
Codex `plugin.json` (`before_prompt`, ключ `exec`).

### 2) In-process .NET-API (Visual Studio) — `IWikiOrchestrator` + `WikiOrchestratorFactory`
VS-расширение ссылается напрямую на `42.ProjectWikiDomain.dll` (без subprocess).
Ядро зовётся так:
- Фабрика: `WikiOrchestratorFactory.Create(solutionPath)` → возвращает `IWikiOrchestrator`
  (`MockWikiOrchestrator` пока `model-manifest.json` не `PROMOTED`, иначе
  `ProjectLocalWikiOrchestrator` — «zero code change» между стадиями A/B).
- Интерфейс `IWikiOrchestrator`: `QueryAsync(question, projectRoot, WikiQueryOptions(TopN), ct)` → `WikiAnswer`;
  `GetReadinessStatus(projectRoot)` → `WikiReadinessStatus`;
  `GetRelevanceMap(query, projectRoot, topN)` → `WikiRelevanceMap`.
- Единственный класс-мост: `WikiService` (тул-вокно/команды зовут только его).
  Точки входа хоста: `ICopilotContextProvider` (VS 17.12+, MEF `[Export]`),
  `CodeWikiToolWindow` (Browse/Ask/Map), команды `ShowWikiPanelCommand` /
  `CopyWikiContextCommand` (Alt+W), `IVsStatusbar`.

### 3) Общий контракт (планируемый, ещё не сам код ядра)
Namespace из addendum: `XIIGen.SmartCodeWiki.Client` с типами
`WikiRequest`, `WikiAnswer`, `WikiReadinessStatus`, `WikiRelevanceMap`,
`WikiContextSlice`, `ProjectSystemMap`, `ProjectWikiUnitManifest`,
`ProjectWikiImportExportStatus`. Файлы-протокол в проекте пользователя:
`.xiigen/wiki-domain/{evidence/evidence-index.jsonl, model-manifest.json, system-map.json, wiki-readiness.json}`.

Вывод по имени API: каноническое имя ядрового API — **`IWikiOrchestrator`**
(фабрика `WikiOrchestratorFactory.Create`) внутри `42.ProjectWikiDomain`; для
не-.NET хостов то же ядро доступно через бинарь **`wiki-context-hook.exe`**
с тремя режимами (query / `--map` / `--ensure-ready`).

---

## Как устроено/работает (кратко)
Поток (addendum 2026-05-24): developer prompt → маршрут к project-wiki способности
→ граница системы проекта → план wiki-запросов → сбор evidence (Angular client /
.NET server / SQL stored procedures) → evidence-bound ответ → честный отчёт о
нехватке evidence вместо догадок. Состояния готовности расширены за пределы
`MOCK→PROMOTED`: `not_indexed … basic_wiki_ready … advanced_wiki_ready …
blocked_with_gaps`. Phase-1 само по себе — статический Roslyn-индекс + BM25, без AI.

---

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)
- **Граница хост↔ядро для плана.** ILearningDynamicGraph должен жить за тем же
  API-швом: хост-расширения остаются «тонкими» (R6 — простой набор вызовов
  «задача→ответ»), а граф/оркестрация/обучение — внутри `llm_mvp_core` через
  `IWikiOrchestrator` (или его будущий `ILearning*`-эквивалент). Эти планы дают
  готовую точку подключения: `WikiOrchestratorFactory.Create` — естественное
  место, где обученный граф заменит mock (паттерн A-LM-03 «Bootstrap → trained»).
- **Оркестрация.** `IWikiOrchestrator.QueryAsync` — кандидат на роль входа
  `ILearningOrchestrator`/`ILearningDynamicGraph` (план→fan-out по источникам→
  fan-in→evidence-bound ответ). Это совпадает с DynamicGraph-требованиями скила
  (узлы/рёбра, many-to-many, fan-out/fan-in, async).
- **Измерение / teacher-provenance.** Поля `confidence`, `EvidenceIds`,
  `documentationVersion`, состояния готовности — естественные места для метрик и
  provenance. НО: в планах нет полей teacher-provenance
  (`teacher_access_mode/model_id/model_license/...`), нет export/import/checkpoint
  обученной модели — это надо добавлять в план ILearningDynamicGraph.
- **Осторожно (анти-маскарад R4).** Текущее ядро в планах — BM25 + exact-name
  boost + Roslyn (детерминизм). Для ILearningDynamicGraph это НЕ покрытие
  обучаемого требования: удаление/обёртка BM25 ≠ trained unit.

---

## Классификация (уровень ILearning* vs детерминистический хелпер)
По правилам `HOW-TO-USE-SKILLS.md` (триггеры «scorer/selector/router/KNN/BM25/
dictionary» → детерминизм; уровни `ILearningMachine/ILogicalOrchestrator/
ILogicalFlow/ILogicalUnit/ILearningMachinePair`) и CLAUDE.md «allowed trainable
architecture pattern gate»:

- `IWikiOrchestrator` / `WikiOrchestratorFactory.Create` (`QueryAsync`):
  по форме (план→retrieve→compose) — кандидат на уровень **ILogicalOrchestrator**
  (Level 4). В текущих планах это интерфейс-оркестратор с mock-реализацией, НЕ
  обучаемая модель: фабрика лишь переключает Mock↔ProjectLocal по
  `model-manifest.json`. Без training-data/learning-signal/eval/export-import это
  **honest untrained scaffold**, не trained feature (OP-56).
- `wiki-context-hook.exe` (Program.cs, режимы query/`--map`/`--ensure-ready`):
  транспорт + обёртка над оркестратором. Сам бинарь — **не архитектурный уровень**,
  а host-adapter/transport (тонкая граница, R6/Full Extension Definition Gate:
  entrypoint + target-locator/core API path).
- `WikiBm25Index` / `WikiContextRetriever` (BM25 + exact-name boost, hop=1):
  **внутренний детерминистический хелпер** (scorer/retriever) — по правилу скила
  это implementation detail, допустим только как eval/baseline/labeling, НЕ как
  финальный авторитет обучаемого графа (R4 / Deterministic-Boundary Gate).
- `RoslynFileScanner` / `DataFlowTracer` / `SolutionWikiBuilder` / `WikiEntry` /
  `DataFlowEdge` / `WikiSnapshot`: **детерминистическая статическая экстракция**
  (Roslyn syntax-only). Это corpus/feature-источник для DPO/обучения графа, но не
  обучаемый уровень сам по себе.
- `WikiService` (VS): **host-adapter/мост** (тонкий слой), не уровень иерархии.
- `MockWikiOrchestrator`: явный **scaffold/cold-start** (BOOTSTRAP/mock), должен
  репортить `MOCK`/`runner_missing`, не «trained» (совпадает с паттерном
  `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`).

Итого: реальная обучаемость в этих файлах отсутствует; всё — детерминированный
scaffold + транспорт. Для ILearningDynamicGraph планы дают только границу API и
точку замены (`WikiOrchestratorFactory.Create`).

---

## Пробелы / что доисследовать
1. **Сам код ядра не здесь.** Это планы расширений. Реальные сигнатуры
   `IWikiOrchestrator`, `WikiOrchestratorFactory`, `WikiQueryOptions`, `WikiAnswer`
   надо проверить в `llm_mvp_core/42.ProjectWikiDomain` (есть ли они уже или это
   план). Зафиксировать file:line из кода, а не из плана.
2. **Расхождение имён проекта.** Phase-1 называет новый проект `41.CodeWiki`, а
   addendum/VS-план ссылаются на `42.ProjectWikiDomain`. Уточнить, какой проект —
   реальный владелец ядра (по CLAUDE.md это `42.ProjectWikiDomain`).
3. **Нет lifecycle обученной модели.** В планах нет export/import/checkpoint/
   manifest-ref/locator_key/remote_unit_id для обучаемого графа (addendum
   упоминает manifest/locator только как readiness-критерий, не как тренируемый
   ассет). Для ILearningDynamicGraph это добавлять в план.
4. **Нет teacher-provenance полей** для DPO/distillation — отсутствуют целиком.
5. **Zip-архивы** `plugin-plans-*.zip` не вскрыты (вне прав/нечитаемы) — там могут
   быть Claude/Cursor/Codex-специфичные детали хуков; при необходимости распаковать
   отдельной задачей.
6. **«ILearningDynamicGraph» как термин** в этих файлах не встречается — связь
   только архитектурная (граница API + точка замены оркестратора).
