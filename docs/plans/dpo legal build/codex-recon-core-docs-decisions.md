# Codex Recon — Core Docs: Locked Decisions (DECISIONS-LOCKED.md)

Рекон для плана **ILearningDynamicGraph** (авто-построение DPO для
ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph).
Это сбор evidence, не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

- Цель рекона: `C:/Projects/llm_mvp_core/docs/DECISIONS-LOCKED.md`
  (38 строк, прочитан целиком; таблица решений D-01..D-17 и A-LM-01..A-LM-13 +
  одна implementation note по формату корпуса skill-selection).
- Правила классификации (skim): `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md`
  (v27.5). Ключевые активационные триггеры для DynamicGraph/DPO:
  - строки ~327-330: trainable-architecture-decision-SKILL + human-readable
    architecture audit для graph/DynamicGraph; требуют roles/hierarchy,
    nodes/edges, input/output контракты, many-to-many, fan-out/fan-in,
    deterministic-vs-trainable authority **на каждое ребро**, проверку скрытых
    детерминистических shortcut, привязанных к рёбрам графа.
  - строка ~330: DPO/distillation teacher провенанс — обязательные поля
    `teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`,
    `provider_terms_checked`, `teacher_output_used_for_training`,
    `closed_proprietary_model`. Claude/OpenAI/Gemini как teacher для дистилляции
    **запрещены**; разрешены только размещённые open-weight/permissive после
    проверки лицензии/условий.

Замечание по именам: файл оперирует именами уровней `ILearningMachine`,
`ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair`.
Имена из задачи (`ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` /
`ILearningDynamicGraph`) в DECISIONS-LOCKED.md **не встречаются** — это либо
новые проектные имена плана, либо алиасы существующей иерархии. Нужно доисследовать
(см. Пробелы).

---

## 2. Как устроено / работает (кратко)

DECISIONS-LOCKED.md — реестр финальных архитектурных решений. Две группы:
- **D-01..D-17** — общие решения (error-handling, fabric, компрессия, домены,
  retrieval, anti-leakage по flow grounding).
- **A-LM-01..A-LM-13** — решения по иерархии обучающихся машин
  (ILearningMachine и ниже), доменам обучения, host/extension границе и запрету
  псевдо-trainable дрейфа.
Все решения объявлены неизменяемыми без явного согласия Luba (строка 3).

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

Высоко релевантные locked-решения (это жёсткие границы для будущего плана графа):

**Иерархия / контракты уровней:**
- **A-LM-01** — `ILearningMachine<TDomain>` стоит НАД `ILogicalOrchestrator`:
  оборачивает оркестратор и добавляет цикл обучения; оркестратор НЕ знает про
  обучение. Граф (DynamicGraph) при добавлении обучения должен сохранить это
  разделение: inference-путь независим от training-пути.
- **A-LM-02** — `Train*` (`TrainBatchAsync`, `EvaluateAsync`) живут ТОЛЬКО на
  `ILearningMachine`, никогда на `ILogicalUnit/Flow/Orchestrator`. Прямое
  следствие для графа: узлы/рёбра графа (если это Unit/Flow) — чистый inference;
  обучение графа должно быть на машинном уровне, не на узле.
- **A-LM-09** — `IMachineTraining<TDomain>` расширяет `ILearningMachine<TDomain>`
  и добавляет `TrainMachineBatchAsync` (реализован `SkillSelectionMachine`,
  43.SmartSkillsDomain). Все 11 семантических юнитов 43 обязаны иметь
  `DeterministicFinalAuthority = false`. Детерминистические алгоритмы (BM25,
  regex, if-else) допустимы ТОЛЬКО как bootstrap-генераторы кандидатов, не как
  финальное продакшн-решение. Это прямой прецедент для DynamicGraph: финальную
  семантику принимает обучаемый уровень, не helper.

**Запрет псевдо-trainable дрейфа (центральное для графа):**
- **A-LM-12** — разрешённые trainable-паттерны ТОЛЬКО:
  `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`,
  `ILearningMachinePair`. Scorers, selectors, classifiers, routers, retrievers,
  detectors, KNN/Jaccard/BM25, dictionary/counter, regex/keyword/pattern
  matchers и приватные семейства методов (`TryDecide*`, `ChooseBestPattern`,
  `SelectBestPattern`, `CountMatchingSignals`) — это деталь реализации, НЕ новый
  архитектурный паттерн. Helper допустим только внутри разрешённого уровня и
  должен честно помечаться как internal/baseline, если lifecycle-доказательства
  не подтверждают семантическую заявку.
- **A-LM-13** — если код/доки называют "семантические юниты", реализованные лишь
  как секции чекпойнта, словари, счётчики или scoring-методы, план/ревью обязаны
  классифицировать каждый элемент как: настоящий `ILogicalUnit` | секцию
  чекпойнта | baseline-деталь | запрещённый runtime-shortcut | слишком сильную
  заявку отчёта. Это обязательная проверка для каждого узла/ребра графа.

**Авторитет/решения (для рёбер графа: кто принимает финал):**
- **D-15** — приоритет решает что пробовать; KNN использует историю арбитра как
  память; арбитры решают что принято; ни priority/KNN/budget/style/external-AI
  не могут переопределить отклонение арбитра. Важно для many-to-many рёбер:
  authority separation на ребре.
- **D-14** — внутренняя LLM всегда первой; внешний AI — только разовое
  одобрение с named missing-competence; все результаты проверяются внутренней
  моделью; все проверенные результаты сохраняются как training signals;
  обучение только в idle под IdleDetector; новый адаптер ставится только если
  after-accuracy > before-accuracy. Прямо задаёт правило измерения и
  promotion-gate для обучаемого графа.

**DPO / anti-leakage / измерение (для авто-построения DPO графа):**
- **A-LM-03** — Bootstrap `ILogicalUnit` сохраняют точную статическую логику
  оригиналов (без изменения поведения); это СТАРТОВАЯ ТОЧКА для DPO-обучения, не
  замена. Обученные юниты заменяют Bootstrap только после публикации
  валидированных весов раннером. Каркас для генерации DPO графа должен исходить
  из этого паттерна.
- **D-16** — `PromptFlowGroundingModel` (42.ProjectWikiDomain) — единственный
  разрешённый продакшн-путь flow grounding. `FlowReferenceDetector` — только
  build-time corpus/audit, недостижим из продакшн-роута. Удалённые контракты
  (PromptFlowIndex, GenerationRouteMode.KnownFlowRetrieval, RetrievalSource,
  _flowIndex) нельзя возвращать. Для графа: детерминистический flow-lookup не
  должен маскироваться под обучаемый grounding на рёбрах.
- **D-17** — `exactPromptLeakageRate` обязан быть 0.0 во всех eval-гейтах
  OrchestrationPlanMachine; любой eval-промпт, точно совпадающий с train-промптом
  → PassesGate=false; exclusion-without-failure запрещён. Обязательный
  leakage-гейт для любого DPO/eval графа (ablation/leakage из CLAUDE.md).
- **A-LM-08** — `SyncPersonalToGlobalAsync` шлёт только анонимизированные
  learning signals (не сырые промпты/userID/пути). Privacy-граница для
  обмена обученным состоянием графа.

**Host/extension граница (если граф будет торчать в расширения):**
- **A-LM-11** — claude/codex/cursor-xiigen-smart-skills — тонкие API-клиенты
  llm_mvp_core; без training-логики, корпуса, источника библиотеки навыков,
  user-state, semantic ranking/selection/completion. Вся обучаемая логика —
  в 43.SmartSkillsDomain. Граница на XiigenExtensionRequest/Response.
- **A-LM-10** — adaptive user-state для 43 в `<project root>\.xiigen\smart-skills\`,
  не перезаписывается молча апгрейдами серверной библиотеки; конфликты →
  patch-proposal / DPO-row, не silent overwrite. Common-ассеты (baseline) —
  в `llm_mvp_core/models/` и `training-data/`. Это прецедент для размещения
  common-vs-adaptive состояния графа (соответствует R5 из CLAUDE.md).

**Контекст домена обучения (косвенно):**
- **A-LM-04** — `27.TranslationDomain` = `ILearningMachine<TranslationDomain>`,
  4 direction-адаптера (RU_EN/EN_RU/HE_EN/EN_HE), дисциплина 50 train + 50
  held-out на батч. Пример доменной learning-машины и батч-дисциплины.
- **A-LM-05** / **A-LM-06** / **A-LM-07** — номера/границы подпроектов
  (30.DesignConversionDomain; BOOTSTRAP_FALLBACK в
  ProjectWikiPluginOrchestrationAdapter логируется как warning; affiliate global
  server = 25.ModelServer + endpoint в 15.OrchestratorModel). Релевантно для
  размещения/публикации обученного графа и наблюдаемости fallback.
- Implementation note (строка 37) — формат корпуса skill-selection:
  `corpus-snapshot.jsonl` + `corpus-pointer.json` (НЕ `corpus.jsonl.gz`).
  Orchestration/translation продолжают `corpus.jsonl.gz`. Если граф пишет/читает
  корпус — формат per-domain фиксирован.

Низко/косвенно релевантные: D-03 (компрессия), D-04/D-12 (имена доменов
CODE_Q/BUG/PLAN/ARCH/REFACTOR — канонические), D-07/D-08/D-09/D-13 (рецепты
экспериментов), D-10/D-11 (BM25+KNN fusion как retrieval-деталь; запрет внешних
ML-фреймворков).

---

## 4. Классификация (уровень vs детерминистический хелпер)

DECISIONS-LOCKED.md — **документ-реестр**, не исполняемый код, поэтому сам файл
уровню иерархии не принадлежит. Но решения внутри задают классификацию для
будущего кода графа (по правилу A-LM-12 / A-LM-13 и активационным триггерам
HOW-TO-USE-SKILLS):

- Разрешённые архитектурные уровни (для узлов/рёбер/обёрток графа):
  `ILearningMachine` (training-уровень, A-LM-01/02/09), `ILogicalOrchestrator`,
  `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair` (common+user две ноги).
- Детерминистические хелперы (НЕ архитектура, только внутри уровня, честно
  помеченные internal/baseline): BM25/KNN/Jaccard, scorer/selector/classifier/
  router/retriever/detector, dictionary/counter, regex/keyword/pattern matcher,
  `TryDecide*`/`ChooseBestPattern`/`SelectBestPattern`/`CountMatchingSignals`.
  Почему: явно по правилу скила A-LM-12 (строка 35) и активационным триггерам
  HOW-TO-USE-SKILLS строки 327, 346-347 (любой словарь/счётчик/scorer требует
  blueprint-замены на `ILogicalUnit`, иначе это shortcut).
- Финальную семантику/решение на ребре графа обязан принимать обучаемый уровень
  (`DeterministicFinalAuthority=false`, A-LM-09; авторитет арбитра, D-15);
  детерминистика — только bootstrap-генератор кандидатов (A-LM-03/A-LM-09) или
  build-time corpus/audit (D-16).

---

## 5. Пробелы / что доисследовать

1. **Имена иерархии.** `ILearningUnit/ILearningFlow/ILearningOrchestrator/
   ILearningDynamicGraph` из задачи отсутствуют в DECISIONS-LOCKED.md. Нужно
   подтвердить по `00.Core/LearningMachineContracts.cs` и
   `docs/COMPONENT-INTERFACES.md` / `docs/ARCHITECTURE.md §9`, являются ли это
   новыми именами или алиасами `ILogicalUnit/Flow/Orchestrator`. DynamicGraph
   как locked-уровень в этом файле НЕ зафиксирован — статус нужно уточнить
   (новый паттерн потребует решения Luba, т.к. A-LM-12 перечисляет закрытый
   список разрешённых паттернов; добавление DynamicGraph как нового
   архитектурного уровня — потенциально decision-reopening).
2. **DPO-формат.** Конкретный формат DPO-троек (Pair/A/B/Reason) в этом файле не
   описан — см. `docs/skills/dpo-triple-format-SKILL.md` и
   `docs/EXPERIMENT-RESULTS-MAP.md`.
3. **Teacher-provenance поля.** Полный список и реализация провенанс-полей —
   в HOW-TO-USE-SKILLS (строка 330) и `trainable-architecture-decision-SKILL.md`;
   нужно сверить с реальными схемами DPO-строк.
4. **Граф-специфика (nodes/edges/fan-out, async, per-edge authority).** В
   DECISIONS-LOCKED.md прямых решений нет; требования идут из активационных
   триггеров HOW-TO-USE-SKILLS (строка 329). Нужен отдельный рекон по коду
   оркестрации (15.OrchestratorModel/Logical) и существующим flow-адаптерам.
5. **Размещение common vs adaptive графа.** A-LM-10/A-LM-11 дают прецедент
   (43.SmartSkillsDomain), но конкретная директория/манифест для DynamicGraph-
   ассетов не зафиксирована — доисследовать `models/`, `training-data/`,
   `25.ModelServer`, `IModelRepository`, `IRemoteUnitLocator`.
