# Codex Recon — Ранние заметки по расширениям (MVP "first llm extensions")

Задача: собрать evidence для ПЛАНА ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph).
Это рекон-сбор фактов, НЕ разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Целевая область `C:/Users/lubao/Documents/xiigen/llm mvp/first llm extensions/`
содержит РОВНО два файла (Glob по всей области, подпапок нет):

- `C:/Users/lubao/Documents/xiigen/llm mvp/first llm extensions/PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1 claude code.md`
  (527 строк, v1.0, 2026-05-07, session type ARCHITECT)
- `C:/Users/lubao/Documents/xiigen/llm mvp/first llm extensions/PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v1.md`
  (359 строк, v1.0, 2026-05-07, companion-порт для Codex)

Это РАННИЕ архитектурные заметки/планы первого поколения расширений ("плагин
токен-оптимизатор"), а НЕ код. Оба — `.md` планы. Кода в области нет.

Цитируемые ими источники РЕАЛЬНО существуют в репозитории (подтверждено):
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs`
  (Glob подтвердил; также присутствует во множестве worktrees `.claude/worktrees/...`).
- Прочие цитаты (`29.SemanticCompiler/Routing/RouteClassifier.cs`,
  `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`,
  `22.AgentAlgorithms/Context/AstRelevanceRanker.cs`,
  `22.AgentAlgorithms/Context/AstPackedContextBuilder.cs`,
  `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`,
  `LogicalContracts.cs` / `LogicalContextPacket`) — в рамках узкой под-задачи НЕ
  проверены пофайлово (защита от перегрузки); указаны как "to-do" в Пробелах.

---

## 2. Как устроено / работает (кратко)

Замысел: плагин для Claude Code (и порт на Codex), снижающий потребление токенов
на ~50-60% за счёт ДЕТЕРМИНИРОВАННЫХ хуков-exe (Native AOT, .NET), без участия
модели. Новый проект `40.HookRunners` в `llm_mvp_core` поставляет 5 exe:

1. Orchestration Router (`UserPromptSubmit`): классифицирует домен/субфазы через
   `RouteClassifier.ClassifyByKeywords` + `MultiSubPhaseRouterUnit`, инжектирует
   подсказку оркестрации. Экономия ~15-25%.
2. Skill Selector (`UserPromptSubmit`, depends_on router): BM25-ранжирование
   frontmatter скилов через `AstRelevanceRanker`, отдаёт топ-5. Экономия ~30-40%.
3. Context Injector (`SessionStart`): читает `STATE.json` вверх по дереву, лепит
   снэпшот ≤500 токенов. Экономия ~20%.
4. Output Validator (`Stop`): проверка ответа по DNA-правилам / наличию Gate B,
   инжект флага в следующий промпт. Экономия непрямая (анти-retry).
5. Task Completion Guard (`UserPromptSubmit` CAPTURE + `Stop` VERIFY): извлекает
   требования задачи в `.claude/task-state.json`, на Stop считает
   coverage=addressed/total; при <1.0 даёт exit code 1 + `injectionForNextPrompt`,
   чтобы Claude не завершался преждевременно. Анти-дрейф фокуса.

Порт на Codex: события хуков идентичны; exe платформонезависимы; различия только в
манифесте (`.codex-plugin/`), путях через env (`SKILLS_DIR`, `TASK_STATE_PATH`),
флаге `--mode codex` у OutputValidator и marketplace. Заявлено ~9 строк правок C#.

Принцип границы (совпадает с R6 проекта): вся "тяжёлая" логика — в `llm_mvp_core`;
хост-слой (плагин/exe) тонкий, читает env и пишет JSON в stdout.

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

Прямой архитектурной общности с ILearningDynamicGraph МАЛО: эти планы намеренно
ДЕТЕРМИНИРОВАНЫ ("без участия модели", "детерминированные хуки") и НЕ описывают ни
обучения, ни DPO, ни графа узлов/рёбер, ни teacher-provenance, ни измерения
качества модели. Полезны как контекст/анти-пример, а не как образец графа.

Косвенная польза для плана ILearningDynamicGraph:
- Оркестрация/маршрутизация: `MultiSubPhaseRouterUnit` (Level 2 ILogicalUnit,
  реально существует) — кандидат как УЗЕЛ графа маршрутизации; но его текущая логика
  keyword/router детерминирована — это вход для DPO, а не уже обученный узел.
- Точки измерения: "coverage = addressed/total" и token-savings метрики — пример
  того, КАК область считает результат; для ILearningDynamicGraph нужны held-out
  numeric metrics/ablation, а не token% — поэтому это образец того, что НЕ годится
  как доказательство обучения (substitution-ловушка по CLAUDE.md).
- Граница host↔core (R6): подтверждает паттерн "тонкий хост + ядро в llm_mvp_core",
  который ILearningDynamicGraph должен соблюдать (общие модели/логика в репо — R5).
- Извлечение требований/предикатов (`ExtractPredicates`,
  `PromptRoutingInputCompilerUnit`) — потенциальный источник СЫРЫХ сигналов для
  построения DPO-пар (chosen/rejected), но в текущем виде это детерминированный
  парсер, а не teacher.
- Teacher-provenance: НЕ затронут вообще. Эти exe детерминированы и не вызывают
  внешних моделей — значит teacher/distillation-источников тут нет (нечего
  лицензировать), что согласуется с запретом Claude/OpenAI/Gemini как teacher.

Вывод по релевантности: medium-low. Ценно как (а) карта реально существующих
router/selector/extractor компонентов-кандидатов в узлы графа и (б) анти-пример
"детерминированное ≠ обучаемое".

---

## 4. Классификация (уровень ILearningMachine vs детерминистический хелпер)

Правило (HOW-TO-USE-SKILLS.md, триггеры строк 327 и 342; CLAUDE.md "Allowed
trainable architecture pattern gate"): scorer/selector/classifier/router/
retriever/detector, KNN/Jaccard/BM25, dictionary/counter, regex/keyword matching и
private `TryDecide*`/`Choose*`/`Count*` — НЕ являются архитектурными уровнями. Они
допустимы только как ДЕТЕРМИНИСТИЧЕСКИЕ хелперы ВНУТРИ разрешённого уровня
(ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit /
ILearningMachinePair) и только с lifecycle-доказательствами для обучаемости.

Файлы области (оба `.md` плана) → НЕ код, классификации уровня не подлежат
(артефакты-планы). Классифицирую компоненты, КОТОРЫЕ они вводят/цитируют:

- Хук-exe (orchestration-router, skill-selector, context-injector,
  output-validator, task-completion-guard) и проект `40.HookRunners`:
  **внутренний детерминистический хелпер / тонкий host-runner.** Причина: явно
  "детерминированные хуки, без участия модели"; нет trained state, checkpoint,
  export/import, locator, обучения. Это host-адаптеры/CLI, а не уровни иерархии.
- `RouteClassifier.ClassifyByKeywords` (keyword classify): **детерминистический
  хелпер** (router/keyword). Не уровень.
- `AstRelevanceRanker` (BM25 ранжирование): **детерминистический хелпер**
  (BM25/scorer). Явно подпадает под запрет строки 342/327 быть уровнем.
- `MultiSubPhaseRouterUnit`: **уже Level 2 ILogicalUnit** по имени/контракту
  (ExecuteAsync), НО его внутренняя логика sub-phase routing детерминирована
  (router). Как ILogicalUnit это допустимая ОБОЛОЧКА; "обучаемым" он станет лишь
  после DPO-весов и lifecycle-доказательств (A-LM-03 bootstrap-паттерн).
- `PromptDomainClassifier.Classify`, `PromptRoutingInputCompilerUnit.ExtractPredicates`,
  `CoverageChecker` (coverage=addressed/total), `RequirementExtractor`,
  `DnaRuleChecker`, `StateJsonReader`: **детерминистические хелперы**
  (classifier/extractor/counter/rule-match/IO). Не уровни.
- `LogicalContextPacket` / `LogicalContracts.cs`: тип-контракт (DTO), не уровень.

Итого: вся область — детерминированная host-инфраструктура. Единственный объект
уровня иерархии среди цитат — `MultiSubPhaseRouterUnit` (Level 2 ILogicalUnit-
оболочка с детерминированным нутром). Ни одного ILearningMachine/Orchestrator/
Flow/Pair и ни одного доказанно-обученного юнита в области нет.

---

## 5. Пробелы / что доисследовать

- Пофайлово НЕ верифицированы (защита от перегруза): `RouteClassifier.cs`,
  `PromptDomainClassifier.cs`, `AstRelevanceRanker.cs`, `AstPackedContextBuilder.cs`,
  `PromptRoutingInputCompilerUnit.cs`, `LogicalContracts.cs`. Подтверждён только
  `MultiSubPhaseRouterUnit.cs`. Перед использованием в плане — прочитать реальные
  сигнатуры/контракты (входы/выходы, OperationResult<T>), т.к. план 2026-05-07 мог
  устареть (например `40.HookRunners` мог так и не появиться).
- Существование проекта `40.HookRunners` в текущем дереве НЕ проверено (это план,
  не факт кода). Нужно подтвердить, реализован ли он вообще.
- Расхождение в плане: TaskCompletionGuard зависит то от "12.SemanticCompiler",
  то от "29.SemanticCompiler" (ExtractPredicates) — какой проект реально владеет
  `PromptRoutingInputCompilerUnit`/ExtractPredicates, требует проверки.
- Нет связи с DPO-конвейером ILearningDynamicGraph: если router/selector/extractor
  будут узлами графа, нужно отдельно спроектировать chosen/rejected/quarantine,
  trained state, checkpoint/manifest, locator, fresh-load и continue-training —
  здесь этого нет ВООБЩЕ.
- Метрики области (token% , coverage) недостаточны как доказательство обучения;
  для ILearningDynamicGraph нужны held-out numeric + ablation — доисследовать,
  какие наборы/раннеры это дадут.
- Не проверены другие, более поздние, "extensions"-артефакты вне этой узкой папки
  (возможны более актуальные планы расширений в `dpo legal build/` или в репо) —
  вне границ текущей под-задачи.
