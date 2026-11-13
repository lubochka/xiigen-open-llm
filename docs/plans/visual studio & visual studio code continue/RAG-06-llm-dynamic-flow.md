# RAG-06 — «llm dynamic flow» (описание динамичной модели ЛЛМ)

> **RAG-индекс, папка #6 из 12.** Часть общего свода, который готовит план ПРОДОЛЖЕНИЯ
> расширений для Visual Studio и VS Code (офлайн локальная модель: документация кода +
> design-to-code). Эта папка — HIGH relevance: здесь лежит **исходное описание
> динамической LLM-модели** (архитектура ILearningDynamicGraph / dynamic flow),
> которую формализует план из RAG-05 (dpo legal build).

- **Имя папки:** `llm dynamic flow`
- **Физический путь:** `C:/Users/lubao/Documents/xiigen/llm mvp/llm dynamic flow`
- **Роль (по словам Luba):** описание динамичной модели ЛЛМ (the dynamic LLM model).
- **Проход:** 1/10 (MAP + STRUCTURE + верхнеуровневые дизайн-документы).
- **Дата индексации:** 2026-07-12.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C2-R06-A, Цикл 2 документного обновления; добавка — ничего ниже не удалено и не переписано). Формула этого файла «`ILearningDynamicGraph` — DESIGN-COMPLETE, но CODE-ABSENT» опиралась на состояние репозитория `llm_mvp_core` до 13 июля. С 14 июля ветка `codex/dpo-implementation` получила +455 коммитов; на её вершине (`61d25ab5c` на момент рекогносцировки 28 июля; движется ежедневно — проверять `git ls-remote`, то есть спрашивать сервер о вершине) существуют: семейство из 11 исходных + 6 тестовых файлов `DynamicGraphEdgeAuthority*` («полномочия рёбер графа»), коммит `d9ecc8937` «add learned router authority cutover» (переключение маршрутизации под-фаз на обученный решатель), маршруты топологии (чтение текущей топологии, эфемерные мутации, прогоны с фиксацией ревизии). **НЕ-ОВЕРКЛЕЙМ:** «CODE-ABSENT» больше не буквально верно — код с родственными именами ПОЯВИЛСЯ; но НЕ доказано, что появившееся РЕАЛИЗУЕТ дизайн этой папки (канон `EdgeAuthorityUnit`, authority-правила D-13/14/15, топологии из 9 и 30 узлов). До содержательного аудита кода (вне рамок этого документного обновления) строго разделять «спроектировано в папке/плане» и «появилось на ветке». Авторитетная таблица веток — `BRANCH-STATUS-CURRENT-2026-07-28.md` (событие S4).

## ⚠️ Реальный размер и форма папки

Ранее Glob по этой папке **упал по таймауту** — не зря. Аккуратный `find` дал:

| Метрика | Значение |
|---|---|
| **Всего файлов** | **156 971** |
| Всего каталогов | 13 211 |
| Файлов `.md` | 156 366 (99.6%) |
| Файлов `.cs` (код) | 401 |
| `.csproj` | 31 |
| `.json` | 71 · `.csv` 21 · `.zip` 29 · `.jsonl` 9 · `.mjs` 5 · `.ps1` 2 · `.mmd` 2 · `.pdf` 12 · `.docx` 2 · `.txt` 15 · прочее (`.xml/.svg/.py/.html/.gitignore`) по 1 |

**Форма папки = СМЕСЬ трёх слоёв:**

1. **Дизайн-документы (верхний уровень, ~57 файлов).** Это ядро для нашей задачи —
   тексты, которые *описывают* динамическую LLM-модель (архитектура, 7 принципов,
   каст ролей, 4 реестра, 3 пути обратной связи, топология из 9 и из 30 узлов,
   заблокированные решения D-01..D-15, эксперименты E51/E52/E53).
2. **Огромное дерево СГЕНЕРИРОВАННЫХ обучающих наборов** (`all-documents-method-run/`,
   156 129 файлов = 99.5% всех файлов). Это не «шум»: это выход «метода экзаменации»,
   который прогнал **430 025** источников (файлы + записи внутри zip + вложенные zip)
   и на каждый источник сгенерировал пакет из ~12 `.md` (по одному на каждый из 9 узлов
   пайплайна + индекс + UML-frame + simulation-check).
3. **Снимок исходного кода репозитория** (`source-examination-cache/llm_mvp_core-main/`,
   561 файл, 401 `.cs`, 31 `.csproj` — весь проектный дерево `00.TensorCore … 35.ExperimentsV2`).
   Это «кэш экзаменации источников»: код, который метод читал как один из корпусов.

Итог: **это НЕ просто папка документов и НЕ репозиторий кода — это «рабочая станция»
дизайна динамического потока + гигантский офлайн-прогон генерации обучающих наборов из
всего корпуса XIIGen + кэш-снимок кода.**

## Кросс-связи (важно для сводного плана)

- **↔ RAG-05 (dpo legal build):** там план `ILearningDynamicGraph` (L5
  `ILearningMachine<DynamicGraphDomain>`, 7 новых типов: node/edge/EdgeAuthorityUnit/
  topology/GraphDpoPair/TeacherProvenance) назван **DESIGN-COMPLETE, но CODE-ABSENT**.
  **Данная папка — источник этого дизайна.** Здесь лежит первоисточник: `DYNAMIC-FLOW-
  ARCHITECTURE-PLAN-v1_0-DRAFT.md` + амендменты. Reserved-план RAG-05 формализует именно
  эту «editable flow graph + registries + arbiters + provenance» архитектуру в контракты
  ILearningMachine. Заблокированные решения **D-13, D-14, D-15** (внизу `DECISIONS-LOCKED.md`)
  — это те самые authority-правила (KNN-как-память, arbiter-как-authority, external-AI-как-
  advisory), которые RAG-05 переносит в `EdgeAuthorityUnit`/GraphDPO.
- **↔ RAG-02 (orchestrator extension / 9-узловой поток):** RAG-02 нашёл поток
  `Domain Splitter → Translator → SchemePlanner → Processor → Applier → ResultTranslator →
  ArbiterSelector → Arbiter → Formatter`. **Подтверждено здесь напрямую и физически:**
  каждый из ~13 000 сгенерированных пакетов содержит ровно файлы
  `node-01-domain-splitter-set.md … node-09-formatter-set.md` — это тот же 9-узловой каст.
  Данная папка — «мастер-описание» этого потока (v1 = 9 узлов) + его расширение
  (v2 = 30 узлов N00–N29, файл `dynamic-flow-v2-node-topology.csv`).
- **↔ Расширения VS / VS Code (цель всего свода):** дизайн явно закладывает UI-осмотр
  графа в расширении. `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` §Open Items **O-4**:
  «Where does the UI live? My default assumption is the Visual Studio extension (subproject 13).»
  То есть «examine»-аффорданс (проследить любой ответ назад через все юниты/рёбра/версии) —
  это фича будущего VS-расширения. Это прямой мост к нашей задаче.

---

## Структура / манифест

### Верхний уровень (57 файлов + 4 каталога)

**A. Ядро дизайна динамического потока (`DYNAMIC-FLOW-*`):**
- `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` (29 KB) — **главный документ**: 7 принципов,
  каст ролей, 4 реестра, 3 пути feedback, 5-уровневая иерархия доверия, sync/async обучение.
- `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` (20 KB) — амендмент «сузить перед split»:
  12 изменений, добавляет intake-gate / topology-router / sub-subject-routers / evidence-pack /
  budget-gate / knowledge-edge / external-AI-цепочку; ревизованный поток из **29 стадий**.
- `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` (12 KB) — единый канонический
  end-to-end порядок (20 runtime-шагов + dataset-preparation порядок + repeated-failure rule
  + idle-trigger rule). Ссылается на валидацию E51B/E52.
- `DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md` (2 KB) — locked-правило D-13.
- `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` (24 KB) — план мини-E2E эксперимента.
- `dynamic-flow-high-level-topology-v2.mmd` (2 KB) — Mermaid-топология v2.
- `dynamic-flow-v2-node-topology.csv` (8 KB) — **таблица 30 узлов N00–N29** (id, имя,
  runtime-вопрос, input/output shape, «почему держит юнит маленьким»).
- `logical-structure-compiler-node-map-v1.csv` (7 KB) — карта узлов компилятора логической структуры.

**B. Заблокированные решения и авторитет:**
- `DECISIONS-LOCKED.md` (3 KB) — **D-01..D-15** (финальные архитектурные решения).
- `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` (8 KB) — двухуровневое правило: KNN-arbiter-память
  до генерации + arbiter-verdict как hard-gate после генерации.
- `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` (15 KB) — путь «knowledge edge → external AI»
  как advisory-цепочка (не authority).

**C. Метод экзаменации документов → обучающие наборы:**
- `EXAMINATION-METHOD-FINAL.md` (**69 KB** — самый большой документ) — финальный метод.
- `document-examination-method-v2.md` (21 KB) — метод v2.
- `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md` (11 KB) — план «документ → наборы».
- `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` (24 KB) — анализ адаптации документов.
- `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` (29 KB).
- `FLOW-01-TRAINING-DATASET-PLAN-v3.md` (26 KB) и `-v4.md` (**252 KB** — второй по размеру).
- `flow-01-domain-splitter-training-full.md` (38 KB).

**D. Эксперименты E51 / E52 / E53 (runtime-доказательства):**
- `E51-END-TO-END-PIPELINE-PLAN-v1.md` (35 KB), `E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md`,
  `E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md`.
- `E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN-v1.md` (25 KB), `E53-EXECUTOR-ROUND-CONTRACT.md`,
  `E53-HUMAN-RESULT-CHECKLIST.md`, `E53-LEARNING-SET-TRANSLATION-MATRIX.md`,
  `E53-ORCHESTRATION-EVIDENCE-SCHEMA.md`, `E53-SCENARIO-001-EXPECTED-TRACE.md` (42 KB),
  `E53-SCENARIO-001-SOURCE-PACKET.md`, `E53-ROUND-A/B/C-VALIDATION-RESULTS.md`.

**E. Управление раундами / прочее:**
- `ROUND-MANIFEST-*.md` (~18 файлов) — манифесты раундов работы (2026-05-04).
- `WHAT-TO-DO-WHAT-NOT-TO-DO-v1.md` / `-v2.md`, `PROJECTS-21-23-TESTING-GUIDANCE-v1.md`,
  `SKILLS-BINDING-PROOF-PLAN-v1.md`, `SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`,
  `previos session.txt` (34 KB, контекст прошлой сессии).
- `improve algo.drawio.xml`, `llm_pipeline_corrected.svg` — визуальные схемы.
- `dynamic flow docs.zip` (501 KB) и `latest dynami flow docs.zip` (220 KB) — zip-копии документов.

### Каталог 1 — `all-documents-method-run/` (156 129 файлов, 13 012 каталогов) — ГЛАВНАЯ МАССА

Выход офлайн-прогона «метода экзаменации всех документов». Управляющие файлы:
- `00-RUN-INDEX.md`, `run-summary.json` — итоги прогона (см. ниже).
- Генераторы (`.mjs`): `generate-full-active-source-packages.mjs`,
  `generate-full-corpus-unique-packages.mjs`, `generate-remaining-business-flow-packages.mjs`,
  `generate-all-files-compact-node-sets.mjs`, `classify-subject-expertise-groups.mjs`.
- Скрипты (`.ps1`): `build-all-files-and-zips-manifest.ps1`, `build-nested-zip-entries-manifest.ps1`.
- Крупные CSV/JSON-манифесты корпуса: `corpus-files.json` (11.9 MB), `corpus-files.csv` (7.8 MB),
  `active-source-unique-documents.csv` (1 MB), `duplicate-groups.csv` (4 MB),
  `full-corpus-unique-status.csv` (2.7 MB), `full-active-source-status.csv`,
  `validation-report-*.csv`, `queued-business-flow-headings.csv`, `batch-01-*.csv`.

**4 подкаталога сгенерированных пакетов (каждый пакет = 12 `.md`):**

| Подкаталог | Файлов | Каталогов | Что это |
|---|---:|---:|---|
| `full-corpus-unique-training-sets/` | **120 828** | 10 070 | ~10 069 пакетов `doc-NNNNN-<slug>`, по одному на каждый уникальный документ всего корпуса |
| `full-active-source-training-sets/` | **34 860** | 2 906 | ~2 905 пакетов активных «живых» источников (не история/дубликаты) |
| `business-flows/` | 396 | 34 | 33 фокус-пакета бизнес-флоу (регистрация, онбординг, marketplace, ERP, finance, DevOps, forms/flows, visual-editor …) |
| `all-files-including-zips-compact/` | 23 | 1 | компактные node-наборы «все файлы включая zip» (финальный path-based проход на 430 025 источников) |

**Структура ОДНОГО пакета `doc-NNNNN-<slug>/` (ровно 12 файлов):**
```
00-INDEX.md              — индекс пакета
01-UML-FRAME.md          — UML-каркас для этого документа
10-SIMULATION-CHECK.md   — проверка симуляции прохода через 9 узлов
node-01-domain-splitter-set.md    ┐
node-02-domain-translator-set.md  │
node-03-scheme-planner-set.md     │
node-04-domain-processor-set.md   │ по одному обучающему набору
node-05-applier-set.md            │ на КАЖДЫЙ из 9 узлов пайплайна
node-06-result-translator-set.md  │ (A = human prompt, B = массив
node-07-arbiter-selector-set.md   │  domain-меток; каждая пара + rephrased)
node-08-arbiter-units-set.md      │
node-09-formatter-set.md          ┘
```
Каждый `node-*-set.md` — набор пар «Runtime A (человеческий промпт) → Runtime B (метки/результат)»
плюс перефразированные варианты. Пример (node-01, domain-splitter):
`A: "Use this document to understand Infrastructure Catalogue…"` →
`B: ["user intent","creation evidence","decision trail","version change"]`.

### Каталог 2 — `source-examination-cache/` (736 файлов, 186 каталогов)

Кэш-снимок ИСХОДНИКОВ, которые метод читал как корпус:
- `llm_mvp_core-main/` — **561 файл, весь проектный дерево репозитория** `llm_mvp_core`
  (`00.TensorCore, 01.Tokenizer, 02.DatasetPipeline, 03.TransformerModel, 04.TrainingLoop,
  05.Embeddings, 06.VectorSearch, 07.GraphEngine, 08.GraphRag, 09.Inference, 10.LoRA, 11.DPO,
  12.SearchEngine, 12.SemanticCompiler, 13.CodeOptimizer, 14.ContinuousLearning,
  14.MachineLearning, 15.OrchestratorModel, 16.AutonomousLearning, 21.AgentInfrastructure(.Desktop),
  22.AgentAlgorithms(.Desktop), 23.OssEquivalents(.Desktop), 24.Compression, 28.SemanticNlp,
  29.SemanticCompiler, 33.MatrixAlgorithms, 34.Experiments, 35.ExperimentsV2` — 401 `.cs` + 31 `.csproj`).
  Здесь физически лежит код проектов **13.CodeOptimizer, 15.OrchestratorModel, 16.AutonomousLearning,
  08.GraphRag, 11.DPO, 14.ContinuousLearning**, которые план RAG-05 переиспользует.
- `docs/` — 142 файла документации.
- `session-package/` — 22 файла (упакованное состояние сессии).
- `sessions/` — 11 файлов истории сессий.

### Каталог 3 — `round-zips/` (37 файлов, 11 каталогов)

Архивы 15 «раундов» работы (2026-05-04): round-01 (projects-21-23-e52) … round-15
(e53-round-c-trained-splitter). Заметны: round-08..11 = обучающие пары узлов N00–N03
(training-pairs, domain-splitter, domain-translator, scheme-planner); round-12..15 = E53
(orchestration-proof, round A/B/C). Внутри — `.zip` + пара распакованных каталогов.

### Каталог 4 — `flow-01-training-sets-v1/` (12 файлов, 1 каталог)

**Эталонный ПЕРВЫЙ пакет обучающих наборов вручную** (тот же 12-файловый шаблон:
`00-INDEX`, `10-SIMULATION-CHECK` + `10-SIMULATION-CHECK.zip`, `node-01…node-09`).
По сути «золотой образец», по которому потом сгенерированы десятки тысяч пакетов в
`all-documents-method-run/`.

**Все 156 971 файл учтены:** 57 верхнеуровневых + 156 129 (`all-documents-method-run`) +
736 (`source-examination-cache`) + 37 (`round-zips`) + 12 (`flow-01-training-sets-v1`).

---

## Подробный разбор (главные дизайн-документы)

### 1. `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` — сердце описания модели

**Что это.** Драфт архитектуры LlmFromScratch как **самомодифицирующейся сети маленьких
специализированных внутренних LLM-юнитов поверх редактируемого графа-потока**. Задаёт
принципы, каст, реестры, feedback-пути и сдвиг тест-философии. Схемы/код намеренно вынесены
в последующие документы.

**7 принципов (ключевое для понимания «динамичной модели»):**
1. **Universal Continuous Learning** — каждый юнит учится из feedback. «Boundary-code» —
   только истинные определения (словарь предикатов, контракт интерфейса, iron-rules из CLAUDE.md,
   D-07/08/09/11). Всё остальное learnable с 1-го дня. Решения стартуют как Q4-AI-pipeline
   (низкая уверенность) и *автоматически* становятся Q3-graph-RAG (прямой lookup) по мере
   накопления уверенности — граф сам «выводит» стоимость AI-вызова.
2. **Dynamic Flow Topology** — **граф пайплайна = данные, не код.** Лежит в своей БД, оркестратор
   читает топологию в рантайме. Добавить юнит = зарегистрировать cell + добавить node-edge;
   заменить = двинуть указатель; откатить = восстановить прошлую версию топологии.
   Правки на уровне рёбер (вставка узла между двумя существующими — реальный кейс Luba
   «нужно вставить arbiter-selector между стадиями»).
3. **Self-Similar Units** — единый внешний интерфейс (predicate-in / predicate-out + метаданные).
   Domain-splitter и formatter снаружи выглядят одинаково → hot-swap, swappability-judge,
   marketplace, gap-fetch без перекомпиляции оркестратора. Зеркальный примитив: «маленький
   обученный gate над реестром кандидатов» (splitter спереди, arbiter-selector сзади,
   skill-retriever в середине, swappability-judge над cells) — **один параметризованный вид, не 4**.
4. **Versioning and Rollback by Construction** — у каждого cell / топологии / train-run /
   dataset-slice цепочка версий (append-only). Три РАЗНЫХ хранилища: **schema** (контракт в коде,
   read-only), **state** (живой граф в реестре, read-write с версиями), **trail** (append-only
   event-store). Их нельзя смешивать.
5. **Internal AI Throughout** — никаких Claude/GPT/Anthropic/OpenAI ни в hot-path, ни в фоне.
   Единственное исключение — офлайн-подготовка данных берёт *контент* извне (SO/GitHub/Wikipedia/
   MS Docs), но никогда *суждения*. **Квалификация 2026-05-04:** external AI доступен только через
   named knowledge-edge путь ПОСЛЕ попытки внутренних юнитов, как advisory-материал, с адаптацией
   назад в локальную структуру и арбитрами.
6. **Provenance End-to-End** — каждая установка юнита/правка графа/импорт данных/train-run/
   промоушен/откат/апдейт уверенности записывается (timestamp, source, parent/child-state,
   trust-score, причина). Именно этот trail включает **«examine»-аффорданс** (проследить ответ
   назад через все юниты) — без него он не реализуем.
7. **Testing Adapts to Self-Modifying System** — поведенческие тесты на фикс-последовательность =
   анти-паттерн. Три категории: **contract** (форма in/out на границе), **invariant** (граф без
   циклов, у каждого узла обученная версия, append-only цепочки, FILLS резолвится), **replay**
   (state@T + input → записанный output). ~877 текущих тестов частично переклассифицируются.

**Каст ролей (v1, оперативный порядок):** domain splitter → domain translator (=пятиуровневый
SemanticCompiler как юнит) → decomposer → orchestrator → applier (per (role,domain)) → aggregator →
result translator (обратный путь, в коде ЕЩЁ НЕ построен) → formatter → style adapter; сбоку —
**arbiter pool** (маленькие обученные судьи), **arbiter selector** (зеркало splitter’а),
**swappability judge**, **gap detector**, **dataset curator** (в 16.AutonomousLearning).
**Расширение каста 2026-05-04:** перед splitter’ом добавлены narrowing/evidence-роли (intake-gate,
system-topology-router, sub-subject-router, evidence-pack-selector, result-convergence-gate,
budget/confidence-gate) + logical-structure-compiler как pre-stage.

**4 реестра (в 08.GraphRAG, но раздельные коллекции):** flow-registry (контракты топологий,
версии), unit-registry (cells: веса/метаданные/версии/трест + рёбра FILLS/SUPERSEDES/
SWAPPABLE_WITH/TRAINED_FROM), decision-graph (routing: ARCHETYPE/ROLE/SCORE_BRACKET/CONDITION,
рёбра REQUIRES_MINIMUM/OPTIONAL_ARBITER/ROUTES_TO/AVOIDS — XIIGen-форма), dataset-registry
(слайсы по (unit-role, domain) + provenance + conformance-suites). Общий event-sourcing;
прямые записи в storage запрещены — только типизированные API.

**3 пути feedback:** Path 1 операционный (реальное время, arbiter-selector → K арбитров → accept/
reroute; в реестры не пишет, только в event-log); Path 2 обучающий (ретроспективный по расписанию/
IdleDetector: атрибуция исходов → апдейт confidence рёбер, trust cells, conformance, dataset-slices —
**граф учится ЗДЕСЬ, не в Path 1**); Path 3 gap-driven (детектор находит устойчивую низкую
уверенность → create-or-fetch loop: реестр → repo → marketplace → веб-корпус → train → install под
пробацией; асинхронно к запросу пользователя).

**5-уровневая иерархия доверия к внешним источникам:** Tier1 локально-обучен-активен (full) →
Tier2 локально-обучен-неактивен (high, для rollback/AB) → Tier3 org-shared реестр (moderate,
подписан) → Tier4 внешний публичный marketplace (low, пробация) → Tier5 обучить с веб-корпуса
(no trust до валидации). TrustScorer из 14.ContinuousLearning расширяется на per-cell trust.

**Sync vs Async обучение:** default = **async** (запрос завершается на ближайшем существующем cell
как fallback, новый cell тренируется в очереди, audit-trail объясняет пользователю). Sync — escape-
hatch по явному запросу.

**Open Items (важно для нашей задачи):** **O-4 — UI осмотра графа = по умолчанию Visual Studio
extension (subproject 13)**; O-6 — reverse SemanticCompiler (result-translator) в коде отсутствует;
O-1 предлагает новые сабпроекты **18.TopologyRuntime** и **19.UnitRegistry**.
**Relevance: критическая** — это первоисточник дизайна ILearningDynamicGraph и явная привязка
examine-UI к VS-расширению.
**Теги:** #архитектура #dynamic-flow #7-принципов #реестры #провенанс #vs-extension #reverse-compiler-missing.

### 2. `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` — «сузить перед split»

**Что это.** Ключевой сдвиг дизайна: **не заменять** динамический поток, а обернуть его
**слоем сужения**. Проблема исходного `user prompt → domain splitter`: один первый splitter,
знающий всю вселенную XIIGen, станет большим/медленным/размытым. Решение:
`user request → intake gate → system topology router → sub-subject routers → local domain
splitters → existing dynamic flow`.

**12 изменений:** (1) intake-gate «какой это запрос?»; (2) system-topology-router выбирает из
**14 высокоуровневых областей XIIGen** (source-provenance, constitution, **dynamic-LLM-flow**,
flow-runtime, knowledge-memory, business-capability, **design-intelligence/UI**, implementation-
factories, security/tenancy, quality/arbiters, self-learning, deployment, **platform-translation/
alt-targets**, history/sessions/repair); (3) sub-subject-routers под каждой областью; (4) domain-
splitter спускается на уровень ниже (локальные домены внутри sub-subject); (5) evidence-pack-selector
перед translation («какие срезы источников имеют право учить этот шаг»); (6) планирование в два
уровня (topology-planner + local-scheme-planner); (7) массивы явно (translators/planners/processors/
appliers/result-translators/arbiters — МАССИВЫ, не один большой юнит); (8) result-convergence-gate
перед форматированием; (9) budget-and-confidence-gate (контроллер скорости/честности, НЕ решатель);
(10) **arbiter-aware KNN + hard verdict boundary**; (11) unit-registry-lookup на каждой array-стадии
(`role + topology area + sub-subject + local domain → smallest trusted unit`); (12) **knowledge-edge
path для external AI** (detector → external-help-planner → context-pack → external-AI-chain →
result-adapter → affected station → arbiters).

**Locked Authority Decision (повторяется в D-13/14/15):** Priority решает *что пробовать*;
KNN использует историю арбитров как *память*; Arbiters решают *что принято*; External AI — advisory,
НЕ authority. При конфликте побеждает вердикт арбитра (если пользователь явно не поправил).

**Ревизованный high-level поток — 29 стадий** (1 user request … 6 local-splitter … 13 convergence …
15 arbiter-array … 16 verdict-boundary … 19 style-adapter … 22 knowledge-edge … 25 external-AI …
29 dataset-curator/confidence-updater). Исходный 9-узловой поток остаётся ВНУТРИ.
**Relevance: очень высокая** (это фактический v2-дизайн модели).
**Теги:** #narrowing #14-областей #arbiter-authority #knowledge-edge #external-ai-advisory #массивы-юнитов.

### 3. `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` — единый порядок

**Что это.** Один канонический end-to-end порядок, чтобы будущие планы не выбирали разные
интеграции. **Runtime-последовательность (20 шагов):** 1 input → 2 source-role-gate → 3 **logical-
structure-compiler** (8-звенная цепочка: language/pattern-translator → context-marker-detector →
structural-role-tagger → semantic-normalizer → family-collapser → logic-predicate-extractor →
meaning-graph-assembler → fidelity/coverage-checker; **marker-detector обязан идти ДО нормализации**)
→ 4 intake+topology-narrowing → 5 evidence-pack → 6 arbiter-aware-KNN → 7 budget-gate → 8 unit-
registry-lookup → 9 local-dynamic-flow (translator→topology-planner→scheme-planner→processor→
applier→convergence) → 10 knowledge-edge-detector → 11 external-AI-path → 12 return-to-affected-
station → 13 arbiter-selector → 14 arbiter-array → 15 **verdict-boundary** → 16 result-translation/
format/style → 17 event-trail → 18 learning-gap-packet → 19 idle-learning-queue → 20 confidence-updater.

**Валидация (важно — не только intent):** E51B прогнал закрытый мини-flow — **6/6 трасс прошли**,
CH-01 выдал named knowledge-edge context-pack вместо выдумки ответа, CH-02 удержал авторитет арбитра
над высокосхожим соседом. Первый implementation-срез для проектов **21-23** мапит эту последовательность
в **5 рантайм-компонентов: InternalLlmGateway, InternalResultExaminer, ExternalAiRequestPackage,
ExternalAiResultAdapter, LearningEventScheduler**. E52 сравнил before/after: **0% baseline vs 100%
enhanced** контракт-успех на V06/CH-01/CH-02/IDLE-01.
Также: dataset-preparation-порядок, **repeated-failure rule** (порог: immediate hard edge / 2 подряд
с той же причиной после 1 repair / 3 всего на станции), **idle-trigger rule** (IdleDetector).
**Relevance: очень высокая** (мост дизайн → код проектов 21-23).
**Теги:** #канон-порядок #logical-structure-compiler #E51B-валидация #5-runtime-компонентов #21-23.

### 4. `DECISIONS-LOCKED.md` — D-01..D-15 (финальные решения)

**Что это.** Таблица заблокированных архитектурных решений. Ключевые для динамической модели:
- **D-04:** канонический domain-split = `CODE_Q / BUG / PLAN / ARCH / REFACTOR` (E18, 95% точность).
- **D-10:** BM25 + K-NN fusion (E34V2, +8% hit@1).
- **D-11:** нет внешних ML-фреймворков (TensorCore — единственный тензорный примитив).
- **D-13:** промоушен tier требует проверки покрытия (плотность субъектов, distinct-предикаты,
  named-edge-risks), не только прохождения метрики (E51A).
- **D-14:** Internal-LLM всегда первый; external-AI только с явным single-use approval + named-
  missing-competence; все результаты осматриваются внутренней моделью и хранятся как training-
  сигналы; обучение — только в idle под IdleDetector; новый адаптер ставится только если
  after-accuracy > before-accuracy (E51A/E51B, граница уверенности 0.50, CH-01/CH-02).
- **D-15:** Priority решает что пробовать; KNN = память из истории арбитров; arbiters решают что
  принято; **никакой priority/KNN/budget/style/external-AI не перекрывает rejection арбитра**.
**Relevance: критическая** (D-13/14/15 — это те authority-правила, что RAG-05 кладёт в EdgeAuthorityUnit).
**Теги:** #locked-decisions #D-04 #D-13 #D-14 #D-15 #arbiter-authority.

### 5. `dynamic-flow-v2-node-topology.csv` — 30 узлов N00–N29

**Что это.** Полная табличная топология v2 (id, имя, runtime-вопрос, input-shape, output-shape,
«почему держит юнит маленьким»). **N00** logical-structure-compiler → **N01** intake-gate →
**N02** system-topology-router → **N03** sub-subject-router → **N03B** budget/confidence-gate →
**N04** evidence-pack-selector → **N05** local-domain-splitter → **N06** domain-translator →
**N07** topology-planner → **N08** local-scheme-planner → **N09** unit-registry-lookup →
**N10** domain-processor → **N11** applier → **N12** result-convergence-gate → **N13** arbiter-
selector → **N14** arbiter-unit-array → **N15** arbiter-verdict-boundary → **N16** result-translator →
**N17** formatter → **N18** style-adapter → **N19** event-trail → **N20** arbiter-aware-KNN-memory →
**N21** knowledge-edge-detector → **N22** external-help-planner → **N23** external-context-pack-builder →
**N24** external-AI-chain → **N25** external-result-adapter → **N26** learning-gap-packet →
**N27** idle-learning-queue → **N28** dataset-curator → **N29** confidence-updater.
Примечание о **двух нумерациях**: сгенерированные обучающие пакеты используют *исходный 9-узловой*
каст (`node-01…node-09` = splitter/translator/scheme-planner/processor/applier/result-translator/
arbiter-selector/arbiter-units/formatter) — это ядро; CSV v2 расширяет его до 30 узлов narrowing-
слоем. **Relevance: очень высокая** (машиночитаемая карта модели). **Теги:** #топология-v2 #30-узлов #N00-N29.

### 6. `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` — два уровня арбитров

**Что это.** Уточняет, что arbiter-знание используется ДВАЖДЫ и по-разному: (1) **до генерации** —
arbiter-сигналы улучшают KNN-retrieval, evidence-выбор, dataset-slicing (positive/negative/warning-
соседи, причины отказа, какое ребро/зависимость проверял арбитр); (2) **после генерации** — arbiter-
verdict = hard-gate (accept/flag/return/ask/learning-gap), и никакой priority не перекрывает rejection.
Три РАЗНЫХ применения arbiter-знания: compiler-check (вход), KNN-signal (выбор), runtime-verdict (выход).
Валидация E51B CH-02: инструкция «дубликат аккаунта» дошла до rejection несмотря на близких соседей.
**Relevance: высокая.** **Теги:** #arbiter-two-level #KNN-память #positive-negative-warning #E51B-CH02.

### 7. `all-documents-method-run/00-RUN-INDEX.md` + `run-summary.json` — итоги прогона

**Что это.** Итоги финального «all-files» прохода метода экзаменации по всему `C:\Users\lubao\
Documents\xiigen`. **Цифры:** физических файлов 69 614; открыто zip-архивов 1 869; top-level zip-
записей 242 630; вложенных zip-контейнеров 3 622; вложенных zip-записей 117 781; глубина вложенности 3;
**всего source-записей 430 025**; UML node-файлов 9; **node-записей 3 870 225** (9 × 430 025);
compact training-пар на источник 18 → **всего 7 740 450 пар**. Валидация: все 9 `node-*.jsonl`
430 025/430 025 → pass. `run-summary.json` добавляет: кандидатных документов 19 545, уникальных
хэшей 10 069, дубликатов 9 476, активных уникальных 2 905, history/session-файлов 14 738.
Ранние прогоны: business-flow 33 пакета, active-source 2 905, corpus-unique 10 069.
**Relevance: высокая** (объясняет, откуда 156K файлов и что это за корпус). **Теги:** #method-run #430025-источников #7.7M-пар #корпус.

---

## Что такое «динамическая LLM-модель» (сводка архитектуры)

**Не один большой LLM, а редактируемый ГРАФ маленьких обученных юнитов:**
- **Граф = данные** (в БД), оркестратор читает топологию в рантайме; правки на уровне рёбер;
  версии + rollback встроены; полный provenance каждого шага (это и есть основа «examine»-UI в VS).
- **Ядро (v1) = 9 узлов:** Domain Splitter → Domain Translator → Scheme Planner → Domain Processor →
  Applier → Result Translator → Arbiter Selector → Arbiter Units → Formatter. **Именно этот каст
  физически прошит в ~13 000 сгенерированных обучающих пакетов** (`node-01…node-09`) — прямое
  подтверждение находки RAG-02.
- **v2 = 30 узлов N00–N29:** тот же 9-узловой центр, обёрнутый слоем сужения (intake-gate →
  topology-router → sub-subject-routers → evidence-pack → local-splitter) впереди и слоем обучения/
  внешней помощи (event-trail → KNN → knowledge-edge → external-AI-advisory → learning-gap →
  idle-queue → dataset-curator → confidence-updater) сзади.
- **Authority-правила (D-13/14/15):** Priority = что пробовать; KNN = память; **Arbiter = что принято**;
  External-AI = advisory. Всё внутреннее (Internal-AI throughout); обучение — в idle.
- **Связь с кодом:** дизайн мапится в 5 рантайм-компонентов (InternalLlmGateway, InternalResultExaminer,
  ExternalAiRequestPackage, ExternalAiResultAdapter, LearningEventScheduler) для проектов 21-23;
  предложены новые сабпроекты 18.TopologyRuntime + 19.UnitRegistry. **В RAG-05 это формализовано в
  `ILearningDynamicGraph` — DESIGN-COMPLETE, но CODE-ABSENT.** То есть данная папка = «что задумано»,
  RAG-05 = «как это оформить в ILearningMachine-контракты», а кода реализации пока нет.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C2-R06-B, Цикл 2; добавка). Утверждение выше «а кода реализации пока нет» устарело как буквальный факт: на ветке `codex/dpo-implementation` появились семейство `DynamicGraphEdgeAuthority*` (11 исходных + 6 тестовых файлов) и топологические маршруты. Соответствие появившегося дизайну НЕ проверено — появление ≠ реализация задуманного; содержательный аудит кода вне рамок этого документного обновления. См. оверлей Цикла 2 в начале файла и `BRANCH-STATUS-CURRENT-2026-07-28.md`.

---

## Рамка проходов (frame)

- **Проход 1/10 (этот): MAP + STRUCTURE + верхнеуровневые дизайн-документы.** ✅ Сделано:
  реальный счёт файлов, дерево, форма (смесь: дизайн-доки + сгенерированные обучающие наборы +
  снимок кода), глубокий разбор 7 ключевых документов, кросс-связи с RAG-05/RAG-02, привязка к VS-extension.
- **Проходы 2–9: вглубь по поддеревьям.**
  - Проход 2: `EXAMINATION-METHOD-FINAL.md` (69 KB) + `document-examination-method-v2.md` +
    `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md` (метод превращения документа в наборы).
  - Проход 3: `FLOW-01-TRAINING-DATASET-PLAN-v4.md` (252 KB) + `flow-01-training-sets-v1/` (эталон)
    + `flow-01-domain-splitter-training-full.md`.
  - Проход 4: эксперименты **E51** (`E51-END-TO-END-PIPELINE-PLAN-v1`, dual-track, E51A-tier2).
  - Проход 5: эксперименты **E53** (proof-plan, scenario-001-trace 42 KB, round A/B/C results,
    executor-round-contract, evidence-schema).
  - Проход 6: амендменты `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1` + `DYNAMIC-FLOW-MINI-E2E-
    EXPERIMENT-PLAN-v1_0` + `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1` +
    `logical-structure-compiler-node-map-v1.csv`.
  - Проход 7: `all-documents-method-run/` генераторы (`.mjs`/`.ps1`) + CSV-манифесты корпуса +
    выборочный разбор пакетов `full-corpus-unique-training-sets` / `full-active-source-training-sets`.
  - Проход 8: `all-documents-method-run/business-flows/` (33 бизнес-флоу-пакета) + `all-files-
    including-zips-compact/`.
  - Проход 9: `source-examination-cache/` (снимок кода `llm_mvp_core-main`, docs, session-package,
    sessions) + `round-zips/` (15 раундов) + `WHAT-TO-DO/*`, `PROJECTS-21-23-TESTING-GUIDANCE`,
    `SKILLS-BINDING-PROOF-PLAN`, `previos session.txt`, `.drawio.xml`/`.svg`/`.mmd`.
- **Проход 10: completeness-sweep + executive summary** (сверка, что каждый из 156 971 файла учтён
  по паттерну; финальная выжимка для сводного плана расширений VS/VS Code).

---

## Честная строка покрытия (проход 1/10)

Прочитаны и разобраны 7 первичных документов ЦЕЛИКОМ (`DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT`,
`DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1`, `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1`,
`DECISIONS-LOCKED`, `dynamic-flow-v2-node-topology.csv`, `ARBITER-KNN-AUTHORITY-AMENDMENT-v1`,
`all-documents-method-run/00-RUN-INDEX.md` + `run-summary.json`) + один эталонный `node-01-domain-
splitter-set.md` как образец формата обучающих пар. Вся структура из 156 971 файла перечислена и
учтена по паттерну (57 верхнеуровневых + 4 поддерева). НЕ прочитаны построчно (осталось на проходы
2–9): большие методичные документы (`EXAMINATION-METHOD-FINAL` 69 KB, `FLOW-01-*-v4` 252 KB), пакеты
экспериментов E51/E53, тела ~13 000 сгенерированных пакетов (характеризованы по образцу и итогам
прогона), снимок кода `source-examination-cache/llm_mvp_core-main` (561 файл), 15 round-zips.
Это соответствует плану прохода 1 (карта + верхние доки); тела поддеревьев — в проходах 2–9.

---

## Pass 2/10 — examination method (как сгенерированы 156K пакетов) + run-index

Проход 2 отвечает на один вопрос: **откуда взялись 156 971 файл в папке и что это за корпус.**
Ответ: это выход «метода экзаменации документов» (document-examination method) — алгоритма,
который берёт ЛЮБОЙ документ (внутренний или внешний open-source) и превращает его в
пер-нодовые обучающие пары A→B для 9-нодового динамического потока. Прочитаны целиком два
методических документа и артефакты прогона.

### Источники, прочитанные в этом проходе (все read-only)

| Файл | Размер | Что это |
|---|---|---|
| `document-examination-method-v2.md` | ~20 KB | Метод v2 — задаёт схему пакета из **12 файлов / 9 нод**. Именно её реализовал автоматический прогон. |
| `EXAMINATION-METHOD-FINAL.md` | 69 KB | Финальный метод — заменяет v2. Добавляет пре-стадию **node-00 (logical-structure-compiler)** = 13-файловая схема, лицензионный гейт и реестр внешних источников, форматы пар по группам источников, coverage-ledger. |
| `all-documents-method-run/00-RUN-INDEX.md` | — | Итоги финального all-files прогона (430 025 записей). |
| `all-documents-method-run/run-summary.json` | — | Машинные счётчики прогона. |
| Образцы: `full-active-source-training-sets/doc-0001-.../*` (12 файлов), `all-files-including-zips-compact/node-01-*.jsonl`, генератор `generate-all-files-compact-node-sets.mjs` | — | Проверка реальной формы пакета и механики генерации. |

### Что такое метод экзаменации (алгоритм)

Один раунд экзаменации работает **строго с ОДНИМ исходным документом** (one-document rule). Порядок:

1. **Назвать документ** — полный путь, имя, число строк, размер в байтах, первая и последняя
   значимые строки, SHA256, и *почему* выбран именно он.
2. **Найти все версии** — искать копии в `old/Older/history/session_files/.claude\worktrees`,
   для каждой записать путь/размер/строки/mtime и точные отличия (дубликат или разошлись).
3. **Найти session/history-свидетельства** — где документ создавался/использовался; классификация:
   creation / candidate-history / later-use / unrelated. Если сессии создания нет — сказать прямо.
4. **Прочитать UML pipeline как рамку соответствия** — вынуть КАЖДУЮ ноду и КАЖДОЕ ребро.
   Ребро отвечает на 4 вопроса: что уходит из источника, что получает приёмник, что приёмник
   обязан произвести, что сломается без этого ребра. Всё это идёт в `01-UML-FRAME.md` — это
   **связующая рамка**: пара, которую нельзя привязать к ребру, в раунд не входит.
5. **Прочитать документ целиком** и назвать домены, которые он определяет, ДО написания пар.

Затем для каждой из 9 нод пишутся пары **A→B**:

- **A = точный ВХОД ноды в рантайме, выписанный полностью.** Не ярлык, не описание категории —
  реальный контент (промпт пользователя / фрагмент+домен / перевод / формальный результат…).
- **B = точный ВЫХОД ноды, выписанный полностью.** Не описание того, каким выход *должен быть*, —
  сам выход (сами доменные ярлыки / полный текст формального перевода / план с под-задачами…).

Это — центральная поправка над v1. **v1 давал построчный ledger, где B ОПИСЫВАЛ выход** («ясная
формулировка работы на языке домена»); нода, обученная на таком B, учится ОПИСЫВАТЬ свою работу,
а не ДЕЛАТЬ её. Финальный метод перечисляет 5 провалов старых Flow-01-сетов (A = строка документа,
а не рантайм-вход; B описывает выход; нет rephrasing-пар; у арбитра (нода 8) нет pass-кейсов;
симуляция проверяла не то — ledger-критерий вместо рантайм-корректности).

Дополнительные правила метода:
- **Rephrasing rule** — каждая пара встречается минимум дважды: то же A/B другими словами, тот же
  смысл под другим углом, **на той же глубине** (перефразировка короче оригинала = потеря контента).
- **Симуляция** — после написания пар прогнать поток «как если бы ноды обучились только на этих
  парах», ответить на 10 вопросов (что выдаст splitter, translator, planner… и «чего не хватает»).
  Секция missing-parts обязана быть честной — скрытый пробел = провал.
- **Внешние источники** — сначала лицензионный гейт НА УРОВНЕ ЮНИТА (MIT/BSD/Apache-2.0/public
  domain/CC-BY/CC-BY-SA — можно; NC/ND/proprietary — `license_blocked`, пар не писать), затем
  provenance (URL, дата фетча, ревизии), фетч ДО экзаменации. Реестр источников: Rust RFC,
  Python PEP, Go proposals, Kafka KIP, OWASP ASVS/WSTG, IETF RFC, AOSA и т.д., с указанием, какие
  ноды каждый источник кормит лучше всего (proposals → ноды 7/8; архитектура → 2/3/4; requirements → 8).
- **Coverage-ledger** (`COVERAGE-LEDGER.jsonl`) — одна запись на юнит: examined / pairs_written /
  simulation_complete / promoted / license_blocked / circular / duplicate…

### Схема пакета из 12 файлов (реальные пакеты используют именно её)

Проверено на живом пакете `doc-0001-infrastructure-catalogue` — ровно 12 файлов:

```
00-INDEX.md              — идентичность документа (путь, размер, строки, SHA256,
                           первая/последняя строка), найденные версии, session-evidence,
                           домены, число пар на ноду
01-UML-FRAME.md          — полный инвентарь рёбер (связующая рамка раунда)
node-01-domain-splitter-set.md     — A: промпт → B: массив доменов
node-02-domain-translator-set.md   — (массив) A: фрагмент+домен → B: формальный перевод
node-03-scheme-planner-set.md      — A: все переводы пути → B: план из под-задач + fb-промпты
node-04-domain-processor-set.md    — (массив) A: под-задача → B: все исходы (success/failure/wait)
node-05-applier-set.md             — (массив) A: под-задача+fb+результат-4 → B: конкретное действие/исход
node-06-result-translator-set.md   — (массив) A: формальный результат → B: текст для пользователя
node-07-arbiter-selector-set.md    — A: R1..Rn → B: список арбитров + почему каждый
node-08-arbiter-units-set.md       — A: результат+специализация → B: pass / concern / reprocess
node-09-formatter-set.md           — A: одобренные переводы → B: финальный ответ под аудиторию
10-SIMULATION-CHECK.md   — сквозная симуляция потока + честная секция «чего не хватает»
```

Финальный метод добавляет 13-й файл `node-00-logical-structure-compiler-set.md` (общий
logical-structure-compiler ПЕРЕД 9 нодами; валидированная E51 форма — 60-слотовая структура:
22 слота счётчиков предикатов, 5 слотов доменов, до 32 слотов нормализованных корней,
1 слот счётчика декомпозиции). **Но автоматический прогон писал 12-файловую (9-нодовую) схему v2,
без node-00.** 4 ноды-массива (2, 4, 5, 6): одна и та же функция параллельно к каждому элементу.

### Итоги прогона (run totals)

Корень скана: `C:\Users\lubao\Documents\xiigen` (сам выход прогона исключён из скана).

Инвентарь корпуса (`run-summary.json`):
- Файлов-кандидатов документов: 19 545; уникальных хэшей: 10 069; дубль-копий: 9 476.
- Активных исходных кандидатов: 4 090; активных уникальных документов: **2 905**.
- History/session-файлов: 14 738; внешних/tooling-копий: 698.

Прошло несколько прогонов (нарастающе):
- **Batch-01 business-flow**: 33 сфокусированных пакета.
- **Full active source**: **2 905** пакетов — полные 12-файловые `.md`-пакеты (в
  `full-active-source-training-sets/doc-0001…doc-2905`).
- **Corpus-unique**: 10 069 пакетов уникального контента.
- **Финальный ALL-FILES (включая zip)** — путь-покрытие всего диска:
  - физических файлов 69 614 + zip-архивов 1 869 + top-level zip-записей 242 630 +
    вложенных zip-записей 117 781 (глубина вложенности 3; 8 ошибок открытия) =
    **430 025 итоговых source-записей**.
  - 9 нод-записей на источник × 430 025 = **3 870 225 нод-записей**.
  - 18 компактных обучающих пар на источник (2 на ноду: оригинал + rephrased) =
    **7 740 450 компактных пар**. Хранятся как 9 JSONL-файлов
    (`all-files-including-zips-compact/node-0X-*.jsonl`), НЕ как пер-документные пакеты.
  - Валидация: каждый `node-0X-*.jsonl` = 430 025 / 430 025 → pass.

Именно эта масса (физические тела 156K + компактные JSONL) даёт ~157 тыс. файлов в папке.

### КРИТИЧНО: корпус сгенерирован ДЕТЕРМИНИРОВАННЫМ ШАБЛОНОМ, а не семантической экзаменацией

Проверка генератора `generate-all-files-compact-node-sets.mjs` показала: массовый корпус —
это **механический скелет покрытия**, а не качественные семантические пары, которые описывает метод:

- `classify(row)` относит файл к классу по МЕТАДАННЫМ (zip-entry / session-history / tooling /
  visual-asset / readable-source / general) — по regex над путём/расширением, без чтения смысла.
- `domains(row)` мапит класс → ФИКСИРОВАННЫЙ список доменных ярлыков (например session-history →
  `['session intent','decision evidence','version context','resume state','open work','validation note']`).
- A/B — шаблонные строки, собранные из имени файла / `TextSample` (первые 180 символов) / заголовка;
  `rephrase()` — механическая подмена первого слова (Use→Apply, Plan→Prepare, Translate→Convert…),
  поэтому в компактных записях `rephrasedA` часто РАВНО `A`, а `rephrasedB` = `B`.
- Никакой LLM/модели в генерации нет; содержание документа по-настоящему не читается.

Следствие для сводного плана (важно, не переоценивать данные):
- **430K-корпус = кандидатный материал / структурный скаффолд**, демонстрирующий 9-нодовую схему в
  масштабе. Это НЕ финальные DPO-сеты. Настоящие качественные пары — это малый, вручную
  экзаменованный набор (Flow-01, business flows) и то, что метод предписывает делать «по одному
  документу, медленно и полностью».
- 2 905 full-active-source пакетов богаче (реальные SHA, первая/последняя строка, список версий,
  session-evidence), но всё ещё шаблон-управляемы, а не глубокая экзаменация.
- Разрыв «метод (идеал: одна пара = осмысленный рантайм A→B, написанный внимательным экзаменатором)»
  vs «прогон (детерминированный шаблон на 430K)» — ключевой факт: локальная модель динамического
  потока пока НЕ обучена на этом; корпус — это сырьё/каркас, требующий настоящей экзаменации или
  замены на LLM-генерацию пар.

### Для чего этот корпус (назначение)

- Пер-нодовый обучающий материал для 9 нод динамического потока (node-01..node-09): каждой ноде —
  свои A→B-пары, чтобы её можно было DPO-обучить (Direct Preference Optimization).
- Цель метода: превратить любой документ в **рантайм-формные** пары, чтобы ноды учились ДЕЛАТЬ свою
  работу, а не описывать её. Внешние proposal-архивы (RFC/PEP/KIP) особенно кормят тонкие
  арбитр-ноды 7/8; архитектурные источники (AOSA/.NET microservices) — ноды 2/3/4; requirement-сеты
  (OWASP/IETF) — ноду 8.
- Материал также питает дизайн InternalResultExaminer в проектах 21-23 (arbiter-constraint
  паттерны) и arbiter-aware KNN-память (negative/warning-соседи из отклонённых proposals) — связка
  с `ARBITER-KNN-AUTHORITY-AMENDMENT-v1` (см. проход 1).

### Что осталось на проходы 3-9

- **Проход 3+**: FLOW-01 тренировочные наборы (`FLOW-01-TRAINING-DATASET-PLAN-v3/v4`, до 252 KB) —
  реальные, вручную написанные пары нод (domain-splitter и др.); сравнить их качество с 430K-скаффолдом.
- Пакеты экспериментов **E51 / E53** (dual-track стратегия, end-to-end pipeline, orchestration-proof,
  round A/B/C валидации, scenario-001 trace) — как метод превращается в исполняемый пайплайн.
- `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1` и `DOCUMENT-ADAPTATION-ANALYSIS-v1` — план и
  анализ адаптации документов (мостик между методом и прогоном).
- Тела бизнес-флоу пакетов (`business-flows/`, 33 шт.) и репрезентативные full-active-source пакеты —
  выборочно, чтобы подтвердить/опровергнуть вывод о шаблонности на «богатом» конце.
- Снимок кода `source-examination-cache/llm_mvp_core-main` (561 файл) — как эти ноды реально
  ложатся на `ILearningDynamicGraph` / `ILogicalUnit`-иерархию (проход про код).

### Честная строка покрытия (проход 2/10)

Прочитаны ЦЕЛИКОМ: `document-examination-method-v2.md`, `EXAMINATION-METHOD-FINAL.md` (69 KB),
`all-documents-method-run/00-RUN-INDEX.md`, `run-summary.json`. Проверены на живых артефактах:
12-файловый пакет `doc-0001-infrastructure-catalogue` (весь список файлов + head `00-INDEX`/`node-01`),
одна компактная JSONL-запись `node-01-*.jsonl`, тело генератора
`generate-all-files-compact-node-sets.mjs` (функции `classify/domains/topic/rephrase/record/nodeRecords`).
НЕ читались построчно (осталось на 3-9): тела 2 905 + 10 069 + 33 пакетов (охарактеризованы по
образцу и итогам прогона), FLOW-01 v3/v4, E51/E53, снимок кода. Вывод о детерминированной
шаблонности корпуса подтверждён кодом генератора (`Доказательство:` `domains()` = фиксированный
map класс→ярлыки, `rephrase()` = подмена первого слова), а не только по образцу.

---

## Pass 3/10 — FLOW-01 training-dataset план + рукописные node-наборы (real vs scaffold)

Проход 3 отвечает на один вопрос: **есть ли в папке ХОТЬ КАКИЕ-ТО настоящие обучаемые
данные динамического потока — или всё это скаффолд?** Для этого прочитан план обучающего
набора FLOW-01 (v3/v4), «эталонный» рукописный пакет `flow-01-training-sets-v1/` (все 9 узлов),
отдельный полный набор splitter’а и два мостовых документа «экзаменация → обучающие наборы».
Вывод короткий: **настоящие обучаемые пары ЕСТЬ, но их крайне мало (один документ, ~10 реальных
промптов splitter’а, 7 арбитр-негативов, 12 teaching-записей E51B); бо́льшая часть «рукописных»
наборов всё ещё использует старый провальный формат «B описывает выход», который сам проект
официально отверг.** Массовый 430K-корпус (проход 2) остаётся детерминированным скаффолдом.

### Источники, прочитанные в этом проходе (все read-only)

| Файл | Размер | Что это |
|---|---|---|
| `FLOW-01-TRAINING-DATASET-PLAN-v4.md` | **252 KB** | Главный план — «source-line ledger»: все 357 строк исходника + 93 строки истории, каждая привязана к узлу/ребру/A/B. **Сам себя дисквалифицирует** (см. ниже). |
| `FLOW-01-TRAINING-DATASET-PLAN-v3.md` | 26 KB | Предыдущая версия (A→B + массивы, но без сохранения строк/версий/истории, arbiter-selector склеен с result-translator). |
| `flow-01-training-sets-v1/` (12 файлов) | — | «Эталонный» рукописный пакет по 9 узлам (`node-01…node-09` + `00-INDEX` + `10-SIMULATION-CHECK`). Прочитаны все 9 node-наборов. |
| `flow-01-domain-splitter-training-full.md` | 38 KB | **Отдельный, качественно другой** полный набор splitter’а — настоящие runtime-пары. |
| `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md` | 11 KB | Мост «документ → наборы» (7-фазный раунд). **Сам помечен SUPERSEDED**. |
| `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` | 24 KB | Карта техобслуживания всех 10 доков — прямо подтверждает диагноз descriptive-B в v4. |

### Что задаёт план FLOW-01 (v3 → v4)

- **v3 → v4.** Сам v4 в разделе `Verdict On v3`: v3 «имел правильное направление» (A→B пары,
  распознал array-стадии), но «недостаточно»: не сохранял каждую строку источника, не сохранял
  версии документа, не включал строки истории-кандидата и **склеил arbiter-selector в
  result-translator**. v4 это чинит, делая **source-line ledger единственной истиной**: ничего
  в наборе не заменяет и не сокращает строку источника; интерпретация стоит РЯДОМ со строкой.

- **Структура v4 (провенанс-леджер).** (1) Source Contract — ОДИН исходный документ
  `business flows\01-user-registration.md` (357 строк) + опорные UML `improve algo.drawio.xml` и
  `llm_pipeline_corrected.svg` + фрагмент истории `umls.md` (строки 1-93). (2) Version Rows — две
  версии (357 строк, 21 094 vs 20 950 байт, хэш-префиксы `B050498D…` vs `1D17EF1C…`) + точные
  изменённые строки (например `GDPR`→`data protection law`, `API Gateway`→`the programming interface
  Gateway`, `PII`→`personal data`, `FLOW-01`→`the user registration flow`). Точный транскрипт
  создания НЕ найден — честно записано. (3) Corrected UML Rules (6 правил): **processor/applier/
  result-translator — это МАССИВЫ юнитов, а не один юнит; arbiter-selector отдельный от
  result-translator; арбитры до финального форматтера; форматтер получает только одобренные
  переводы**. (4) Exact UML Node Ledger **N001-N027** + Exact UML Edge Ledger **E001-E023** (каждое
  ребро сохранено «не сливать»). (5) Full Source Line Ledger **D001-D357** — по одной записи на
  строку: `original | node taught | edge preserved | A taught | B expected | why`. (6) Candidate
  History Line Ledger **H001-H093** из `umls.md` — помечены как «candidate history evidence, not
  proof of creation».

- **Целевые счётчики (quality bar плана).** Все 357 строк распределены по 9 наборам ровно один раз
  (из `00-INDEX`): splitter 11, translator 82, scheme-planner 68, processor 21, applier 24,
  result-translator 21, arbiter-selector 32, **arbiter-units 0** (все derived), formatter 98.
  Pass-критерий симуляции: все 357 строк представлены; массивы остаются массивами; arbiter-selector
  отдельно; форматтер только одобренные; ни одна строка не заменена коротким summary.

- **🔴 КРИТИЧНО: v4 сам себя дисквалифицирует как обучающий набор.** Раздел `Maintenance Note
  After E51` прямо говорит: *«the ledger's `B expected` entries are **not final training outputs**.
  They are **interpretation hints** beside preserved source lines. Per-node training pair files must
  be written under `EXAMINATION-METHOD-FINAL.md`, where B is always the actual output written in
  full, not a label, not a category, and not a description of what the node should produce.»*
  То есть **v4 — это провенанс-леджер (что учить и откуда), а НЕ набор runtime-пар A→B**. Его
  `B expected` — это ярлыки/описания (D001: `B expected: the subject: user registration and sign-up
  onboarding`; строки-разделители: `a preserved section boundary in the final training memory`).
  `DOCUMENT-ADAPTATION-ANALYSIS-v1` §10 повторяет диагноз дословно: D001 B — «это ярлык, а не сам
  текст, который должен произвести Node 1».

### Что реально содержат рукописные наборы `flow-01-training-sets-v1/` (по узлам)

Пакет построен под СТАРЫМ, ныне **superseded** планом (эра descriptive-B), а НЕ под финальным
методом. Прочитаны все 9 node-файлов. Формат строки: `Original source text | A received | B this
node must produce | Edge | Why`. Качество РЕЗКО разное по узлам:

| Узел | Строк | Формат B | Вердикт |
|---|---:|---|---|
| node-01 splitter | 145 | ярлыки доменов, **меняются по содержанию строки** | **полу-реально** |
| node-02 translator | 811 | фикс. шаблон «A clear work statement in the language of X» | descriptive/scaffold |
| node-03 scheme-planner | 1270 | ЕДИНЫЙ фикс. шаблон, дословно повторён на каждой строке | descriptive/scaffold |
| node-04 processor | 199 | фикс. шаблон «A processed requirement that preserves the source line…» | descriptive/scaffold |
| node-05 applier | 226 | фикс. шаблон «A domain result preserving publisher, consumer, event…» | descriptive/scaffold |
| node-06 result-translator | 199 | фикс. шаблон «A human-readable result fragment that still preserves…» | descriptive/scaffold |
| node-07 arbiter-selector | 379 | фикс. шаблон-список «Reviewer choice such as safety, event order…» (симуляция сама зовёт его «thin») | descriptive/scaffold |
| **node-08 arbiter-units** | 66 | **настоящие derived-негативы: неверный результат → Flag + причина + reprocess** | **РЕАЛЬНО (лучшее)** |
| node-09 formatter | 901 | фикс. шаблон «A section, sentence, or boundary…»; много строк = пустые разделители | descriptive/scaffold |

**Реальный сигнал только у node-08** (и частично node-01). Пример node-08 (единственный по-настоящему
обучаемый DPO-материал в пакете):
> `A: The result stores the sign-in token in visible browser storage.`
> `B: Flag it. The source says tokens are stored in httpOnly, Secure, SameSite cookies. Send the
> affected fragment back for reprocessing.` (anchor D105)

— 7 рукописных отклонённых примеров, каждый привязан к конкретному правилу источника (D105 httpOnly;
D115/D257 «5 попыток/15 мин»; D117/D268 256-битный токен; D241/D074 идемпотентность; D277 сохранить
старый userId; D071/D255 dead-letter-queue; D121-D125 consent/audit). Все честно помечены `DERIVED …
not a direct source quote`. **arbiter-units — 0 source-строк** (это и должно быть: арбитр учится не
на строках документа, а на неверных результатах).

Пример **descriptive-провала** (node-02, то, что метод прямо запретил — B ОПИСЫВАЕТ выход, а не ДЕЛАЕТ
его): для строки `«  - POST /auth/sso/{provider}…»` → `B: A clear work statement in the language of:
identity and access`. Узел, обученный на таком B, научится ГОВОРИТЬ «ясная формулировка на языке
identity and access», а не реально ПЕРЕВОДИТЬ фрагмент. node-03 ещё хуже: один и тот же шаблон
`«A planned piece with its place in the provider path, email path…»` дословно на КАЖДОЙ из 68 строк —
B вообще не зависит от входа.

### Отдельный НАСТОЯЩИЙ набор: `flow-01-domain-splitter-training-full.md`

Это **качественно другой артефакт** — «успешный retry» splitter’а в ПРАВИЛЬНОМ формате финального
метода: **A = реальный runtime-промпт пользователя, B = сам выход, выписанный полностью** (список
функциональных территорий на intent-сохраняющем языке). Пример:
> `Input: I want to sign up for the platform using my Google account.`
> `Output: - Who handles authenticating users through external providers like Google… - Who creates
> the user's profile once authentication succeeds… - Who tracks where the user is in the registration
> process… - Who delivers the initial questionnaire via chat…`

~8-12 таких примеров (Google-SSO, email/password, questionnaire, merge-дубликат, rate-limit, funnel-
метрики, отложенная доставка, просроченный verification-токен). **Это единственный по-настоящему
корректный runtime-набор A→B в папке — но покрывает лишь 1 из 9 узлов (splitter).** Он НЕ лежит внутри
`flow-01-training-sets-v1/` (там splitter — это descriptive-полу-реальный node-01), а отдельным файлом
верхнего уровня.

### Мост «экзаменация → обучающие наборы»

- `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md` — 7-фазный раунд на ОДИН документ:
  (1) назвать один источник; (2) найти версии; (3) найти session/history-свидетельства (creation/
  candidate/later-use/unrelated); (4) прочитать UML как рамку соответствия (все узлы + все рёбра,
  ребро = 4 вопроса); (5) построить source-line ledger (строка на каждую строку источника);
  (6) построить пер-нодовые наборы (шаблон файла `ROW ID / Source / Original / A / B / Edge / Why`);
  (7) симулировать поток «как если бы обучили только на этих наборах» + обязательная честная секция
  missing-parts. **Сам помечен `Superseded Pair-Writing Rule`**: где он допускает descriptive/label/
  category B — верх берёт `EXAMINATION-METHOD-FINAL.md` (B = точный выход полностью); плюс требует
  добавить пре-стадию `node-00-logical-structure-compiler-set.md`.
- `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` — карта техобслуживания всех 10 доков (что верно / что устарело /
  что менять). Именно она зафиксировала применённые правки 2026-05-04: **D-14/D-15** в
  `DECISIONS-LOCKED`, пре-стадию **node-00** (60-слотовый feature-вектор), N00/N03B в topology-CSV, и
  предупреждение о descriptive-B в v4. Приоритет изменений: сначала D-14/D-15, затем метод + v4
  («проблема формата B заразит любые пары, написанные из него, пока не добавлена оговорка»).

### 🎯 Вердикт по реальным обучаемым данным (real vs scaffold)

Три слоя «данных динамического потока» в порядке убывания качества:

1. **`flow-01-domain-splitter-training-full.md`** — НАСТОЯЩИЕ runtime-пары A→B (правильный формат), но
   ~10 примеров и только splitter.
2. **node-08 arbiter-units** (7 derived-негативов) — настоящий DPO-негатив-материал, привязан к
   правилам источника. Единственный реальный набор внутри `flow-01-training-sets-v1/`.
3. **E51B (проход 4 разберёт детально)** — единственная замкнутая ВАЛИДАЦИЯ: 12 teaching-записей + 6
   held-out/challenge-трасс, выведенных из того же Flow-01 registration, дали **6/6 trace success**
   (CH-01 knowledge-edge, CH-02 arbiter-authority при KNN-схожести 0.857). Это доказывает, что
   материал МОЖЕТ учить — на крошечном масштабе.

Всё остальное — **не обучаемый рантайм-сигнал**:
- **430K-корпус** (проход 2) = механический детерминированный скаффолд (regex-классификатор +
  фиксированный map доменов).
- **v4-леджер D001-D357/H001-H093** = провенанс (что учить и откуда), B — ярлыки; **сам план говорит,
  что это НЕ финальные обучающие выходы**.
- **7 из 9 node-наборов** в `flow-01-training-sets-v1/` (translator/planner/processor/applier/
  result-translator/arbiter-selector/formatter) = descriptive-шаблоны в старом провальном формате «B
  описывает выход».

**Итог для сводного плана расширений:** реальной, готовой к DPO обучающей выборки динамического потока
в этой папке фактически НЕТ — есть один правильный splitter-набор (~10 пар), 7 арбитр-негативов и
12 E51B teaching-записей на ОДИН документ (user-registration). Провенанс-инфраструктура (леджер,
версии, UML-рамка, метод) — зрелая и качественная, но пер-нодовые пары для 8 из 9 узлов ещё нужно
ПЕРЕПИСАТЬ под `EXAMINATION-METHOD-FINAL` (B = реальный выход), либо генерировать LLM’ом. Это прямо
подтверждает вывод RAG-05: `ILearningDynamicGraph` = DESIGN-COMPLETE, но и данные, и код — CODE-ABSENT/
DATA-ABSENT на уровне обучаемой массы. `Доказательство:` v4 `Maintenance Note After E51` («B expected
… are not final training outputs»); node-02..07/09 B = фиксированные шаблоны (grep подтвердил
идентичный B на каждой строке node-04/05/06); node-08 = 7 derived-строк, 0 source-строк;
`flow-01-domain-splitter-training-full.md` = реальные Input→Output.

### Что осталось на проходы 4-9

- **Проход 4: эксперименты E51** (`E51-END-TO-END-PIPELINE-PLAN`, dual-track, E51A-tier2) — как
  Flow-01 материал превратился в замкнутый мини-flow; 12 teaching + 6 challenge, 60-слотовый вектор,
  0%→100% контракт (E52), пороги уверенности 0.50.
- **Проход 5: эксперименты E53** (proof-plan, scenario-001-trace 42 KB, round A/B/C, executor-round-
  contract, evidence-schema) — оркестрация-доказательство и обученный splitter (round-15).
- **Проход 6:** `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT` + `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN` +
  `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION` + `logical-structure-compiler-node-map-v1.csv`.
- **Проход 7:** генераторы/CSV-манифесты `all-documents-method-run/` + выборка пакетов
  `full-corpus-unique` / `full-active-source` (подтвердить шаблонность на «богатом» конце).
- **Проход 8:** `all-documents-method-run/business-flows/` (33 бизнес-флоу) + `all-files-including-
  zips-compact/`.
- **Проход 9:** `source-examination-cache/llm_mvp_core-main` (561 файл кода — как узлы ложатся на
  `ILearningDynamicGraph`/`ILogicalUnit`) + `round-zips/` (15 раундов, в т.ч. round-08..11 обучающие
  пары узлов N00-N03) + `WHAT-TO-DO/*`, `previos session.txt`, схемы.

### Честная строка покрытия (проход 3/10)

Прочитаны ЦЕЛИКОМ: `FLOW-01-TRAINING-DATASET-PLAN-v4.md` (голова + структура разделов + весь ledger-
формат D-строк и H-строк по grep), `flow-01-training-sets-v1/00-INDEX.md`, `10-SIMULATION-CHECK.md`,
`node-01`, `node-08` целиком; `node-02`/`node-03`/`node-07`/`node-09` — головы + подтверждение
descriptive-B; `node-04`/`node-05`/`node-06` — B-строки по grep (подтверждён идентичный фикс-шаблон);
`flow-01-domain-splitter-training-full.md` (первые ~90 строк, 8 примеров); `DOCUMENT-EXAMINATION-TO-
TRAINING-SETS-PLAN-v1.md` целиком; `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` целиком. НЕ читались построчно
(осталось на 4-9): полные тела v4-леджера (357 D + 93 H строк — охарактеризованы по формату и §10
adaptation-analysis), `FLOW-01-...-v3.md` (охарактеризован по `Verdict On v3` в v4), пакеты E51/E53,
снимок кода, round-zips. Вывод «настоящих обучаемых данных мало, бо́льшая часть — descriptive-скаффолд»
подтверждён: (а) самопризнанием v4, (б) grep-идентичностью B в node-04/05/06, (в) §10 adaptation-
analysis, (г) прямым сравнением node-08 (реально) и `*-splitter-training-full` (реально) против
descriptive-узлов.


---

## Pass 4/10 — эксперименты E51/E52 (closed-loop training proof)

Задача прохода: разобрать «пакет E51» — самопровозглашённый closed-loop / self-learning
эксперимент (12 teaching + 6 held-out, 60-слотовый вектор, 0%→100% в E52) и вынести честный
вердикт: это настоящее эмпирическое доказательство обучения динамического flow — или
спроектированная/симулированная трасса? Каждое утверждение привязано к файлу:строке.

### 0. Что физически лежит в папке (граница честности)

В папке `llm dynamic flow` от «пакета E51/E52» присутствует НЕ реализация экспериментов E51A/E51B,
а только: (а) планы, (б) тест-файл, ссылающийся на E51A/E51B, (в) исполняемый код E52 внутри
round-zip. Проверено:
- Планы (loose): `E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md` (203 строки),
  `E51-END-TO-END-PIPELINE-PLAN-v1.md` (877 строк), `E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md` (234).
- `ROUND-MANIFEST-2026-05-04-PROJECTS-21-23-E52.md` (loose, 58 строк) — манифест раунда E52.
- Единственный round-zip с исполняемым кодом эксперимента: `round-zips/round-01-projects-21-23-e52-2026-05-04.zip`
  (24 файла, 51.9 KB). Внутри — E52 + классы проектов 21-23 + общий тест `Tests/DynamicFlowE51Tests.cs`.
- `grep -rl "class E51BDynamicFlowMiniE2E|class E51ACodeContextPipeline|class E51EndToEndPipeline"`
  по ВСЕЙ папке (включая `source-examination-cache/` и остальные round-zip 02/04/06/07) →
  **0 совпадений**. Реализация E51A/E51B (`E51ACodeContextPipeline.cs`, `E51BDynamicFlowMiniE2E.cs`,
  `E51EndToEndPipeline.cs`) в эту папку не попала. Числа E51A/E51B видны здесь ТОЛЬКО как:
  (1) проза планов и (2) hardcoded ожидаемые значения в `DynamicFlowE51Tests.cs`.
- Результатных JSON для E51A/E51B в папке НЕТ; присутствует только E52-результат
  (`experiments-v2/E52-.../results/run-2026-05-04-085736.json/.md` внутри round-01 zip).

Вывод раздела 0: «E51A: enhanced overlap 0.967 / domain 100% / trace 100%» — это цитата из
плана (`E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md:29-33`), а не воспроизводимый в этой папке артефакт.
Только E52 здесь исполняемо-воспроизводим по коду.

### 1. Двухтрековая стратегия E51 (что задумано)

`E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md` разделяет один «E51» на два независимых эксперимента,
чтобы успех одного не прятал провал другого (:166-170):
- **E51A `E51A-CODE-CONTEXT-PIPELINE`** — «component-chain proof» (:37). Доказывает техническую
  цепочку: stack detection → семантическая токенизация → извлечение логических предикатов →
  domain routing → K-NN retrieval → template beautification → запись метрик (:27-33). Гейт:
  `enhanced_predicate_overlap >= 0.60 AND domain_accuracy >= 0.70` (:121).
- **E51B `E51B-DYNAMIC-FLOW-MINI-E2E`** — «real dynamic-flow proof» (:51). Доказывает архитектурный
  вопрос: маленький teaching-набор и test-набор проходят один и тот же логический путь, выбирают
  верную локальную станцию, применяют арбитров как жёсткий гейт, детектируют «край знаний»
  (knowledge edge) и эмитят learning-пакеты (:49). Гейт E51B: 18 записей компилируются в
  логические фичи, 4 held-out registration-промпта маршрутизируются в ожидаемый subject, арбитр
  отклоняет duplicate-account, biometric-челлендж создаёт knowledge-edge context-pack + learning
  packet; итоговый числовой гейт `e2e_trace_success_rate = 6/6` (:127-139).

Важно про терминологию «12 teaching + 6 challenge»: строгий счёт из стратегии — **18 записей =
12 teaching + 6 held-out** (:127), где 6 held-out = 4 registration-теста (TE-01..TE-04) + 2 челленджа
(CH-01 knowledge-edge, CH-02 arbiter). «6/6 trace success» (:139) — это все 6 held-out трасс,
а не 6 отдельных «challenge records».

Общие правила обоих экспериментов (:91-105): детерминированные данные; JSON+markdown evidence;
held-out тесты; **запрет вызывать внешний AI в первой версии**; нельзя менять locked compiler
domain enum и 22-предикатный словарь. То есть уже по дизайну это оффлайн-детерминированные
пробы, а не онлайн-обучение модели.

### 2. E51A — code-context pipeline: что реально делает (детерминированный K-NN, не обученная сеть)

Что такое «enhanced» в E51A по плану `E51-END-TO-END-PIPELINE-PLAN-v1.md`:
- Baseline = случайный шаблон без K-NN; ожидаемый overlap 0.15-0.20, domain accuracy ~0.20
  (случайно на 5 классов) (:32-37, :450-458).
- Enhanced = пять стадий: `StackDetector.Detect` → `SemanticTokenizerPipeline.Level3Tokens`
  (Level1→Level2→Level3 предикаты) → `DomainClassifier` **K-NN** (k=3/5/7 по тиру) →
  Jaccard-дистанция по множествам предикатов → `TemplateBeautifier` (:308-316, :348-354, :389-397).
- «Обучение» = индексация train-примеров для K-NN (`classifier.Train(trainPredicates)`, :577)
  и голосование ближайших соседей. Никакого градиента, checkpoint, DPO — это **retrieval/память
  по разметке**, а не обученная модель. Ground-truth (`RequiredPredicates`/`ForbiddenPredicates`)
  задан руками для каждого из 20 inline-примеров (:82-89, :96-232).
- «Knowledge edge» в E51A = чисто пороговое: `neighborDistance > 0.70` ⇒ `LowConfidence=true`
  (:404). Это метка «нет хорошего соседа», а не сигнал обучения.

Тестовые подтверждения (round-01 zip, `Tests/DynamicFlowE51Tests.cs`) — E51A тесты гоняют реальный
код на inline-данных: `LogicExtractor.Extract` даёт непустые предикаты (:22-30), V06 compound-промпт
маршрутизируется в `BUG_FIX` (:57-67), V07 имеет ≥2 сильных соседа ≥0.60 (:90-100), «wrong-domain»
retrieval выглядит неуверенно (score<0.50, :130-157). Числа тира-1 (из плана E51A-TIER2, :29-33):
baseline overlap **0.283**, enhanced **0.967**, domain accuracy **100%**, trace success **100%**,
15/15 focused, 892/892 full (:19-23). Это валидный component-proof детерминированного K-NN-конвейера,
но НЕ доказательство, что «модель научилась»: улучшение = «есть индекс соседей» против «случайный
шаблон».

### 3. E51B — mini-e2e: откуда 12 teaching / 60-слотовый вектор / 6-of-6 (это designed ground-truth)

Все ключевые числа E51B в этой папке существуют как **жёстко зашитые Assert-ожидания** в
`Tests/DynamicFlowE51Tests.cs` (round-01 zip) — то есть как проверки соответствия заранее
прописанной эталонной трассе, а не как измеренный результат обучения:
- 12 teaching records: `Assert.Equal(12, E51BDynamicFlowMiniE2E.TeachingRecordsPublic().Count)` (:206-209).
- 4 held-out с зафиксированными арбитрами: `Assert.Equal(4, ...ExpectedHeldOutTracesPublic().Count)`;
  ожидаемые арбитры прописаны руками — TE-01 `[identity_consistency, token_safety]`,
  TE-02 `[verification_order, token_safety]`, TE-03 `[identity_consistency]`, TE-04 `[token_safety]`
  (:211-220).
- **60-слотовый вектор**: `Assert.Equal(22, ...PredicateVocabularySize)` и
  `Assert.Equal(60, Teaching.VectorWidth)` = `Runtime.VectorWidth` (:229-231); в evidence —
  `FeatureVectorContract` содержит строку `"60 slots"` (:286). Смысл — см. раздел 5.
- Предсказанные локальные subject-ы захардкожены как эталон: TE-01→`AUTH_PROVIDER_ENTRY`,
  TE-02→`EMAIL_VERIFICATION`, TE-03→`IDENTITY_DUPLICATE_PREVENTION`, TE-04→`TOKEN_SESSION_SAFETY`
  (:247-259).
- CH-01 (knowledge edge): `KnowledgeEdgeDetected` + `ArbiterVerdict == "learning_gap_packet_created"`;
  соседи присутствуют, но все с score `0 < s < 0.50` (:237-293). Context-pack содержит фиксированные
  фразы «provider sign-in», «email verification», «duplicate prevention», «token safety» и явное
  «does not yet know how biometric passport verification changes the registration flow» (:261-275).
- CH-02 (arbiter overrides K-NN): ближайший сосед `>= 0.70` (высокое сходство!), но
  `ArbiterVerdict == "rejected"`, `Passed == true`, итог содержит «must not create duplicate
  accounts» (:295-307). То есть арбитр-правило намеренно бьёт высокое K-NN-сходство.
- E51B общий гейт: `EnhancedPrimary == 1.0`, `ShouldScaleUp == false` (:194-203).

Вывод раздела 3: E51B — это **спроектированная мини-трасса с ручным ground-truth**. Тесты проверяют,
что детерминированный конвейер воспроизводит заранее известные ответы (subject-ы, арбитры, verdict-ы),
и что арбитры перекрывают K-NN. «6-of-6» = 6 held-out трасс совпали с эталоном. Это доказывает
«форма flow работает и арбитр — жёсткий гейт», но НЕ «локальная модель обучилась»: нет ни обучаемого
веса, ни checkpoint-а, ни измеренной пост-обучающей точности.

### 4. E52 — «0%→100%» cycle comparison: разбор кода и результата (главный «пруф»)

E52 (`round-01 zip: 35.ExperimentsV2/Experiments/E52Projects2123CycleComparison.cs`) — это перенос
находок E51A/E51B в «проекты 21-23» (internal-first ассистент) и before/after сравнение из **4 трасс**:
V06 (routing), CH-01 (approval package), CH-02 (arbiter authority), IDLE-01 (idle learning) (:43-49).

Как считается «0%→100%» (:50-51):
```
double baseline = traces.Count(t => t.BaselinePassed) / (double)traces.Length;
double enhanced = traces.Count(t => t.EnhancedPassed) / (double)traces.Length;
```
Критично: **3 из 4 baseline захардкожены `false`** прямо в конструкции трасс:
- CH-01 `BaselinePassed` = литерал `false` (:172, поле в конструкторе `CycleTrace(... , false, ...)`).
- CH-02 `BaselinePassed` = литерал `false` (:197).
- IDLE-01 `BaselinePassed` = литерал `false` (:224).
- Только V06 baseline вычисляется: `BaselineRoute(prompt)` для промпта «The tokenizer crashes on
  emoji input why and how to fix it?» (:116) считает keyword-ничью → `codeQ >= bug` → возвращает
  `CODE_Q`, а гейт требует `== "BUG"` ⇒ `false` (:230-236). Итог baseline = 0/4.

А «enhanced» — реальный, но **детерминированный** код (не модель):
- V06 enhanced: `InternalLlmGateway.InspectRouting` → `Classify` — это **подсчёт ключевых слов**
  по словарям доменов (`InternalLlmGateway.cs:93-117`), а «confidence=0.50» = `scores[Bug]/total`
  (доля keyword-ов), не выход модели. Плюс `gateway.ExternalCallCount == 0` — но
  `ExternalCallCount => 0` **захардкожен константой** (`InternalLlmGateway.cs:27`), т.е. «внешних
  вызовов 0» не измеряется, а гарантировано возвращается (E52 :125, :132).
- CH-01 enhanced: `InternalResultExaminer.Examine` — «confidence» = `(present+satisfied)/totalChecks`,
  где present/satisfied = **подстрочные совпадения** required-предикатов/арбитров в тексте ответа
  (`InternalFirstAiSupport.cs:47-63`); `needsExternal = confidence < 0.50 && missing>0` (:63).
  Затем `PermissionGate.Check` → `RequiresApproval` (E52 :137-164).
- CH-02 enhanced: `ExternalAiResultAdapter.Adapt` → `ContradictsLockedDuplicateRule` — **захардкоженное
  keyword-правило**: `combined.Contains("callback") && ("twice"|"repeated"|"same provider") &&
  ("second account"|"duplicate account") && ArbiterConstraints.Contains("identity_consistency")`
  (`InternalFirstAiSupport.cs:134-146`) ⇒ `SignalType == "external_answer_rejected_by_arbiter"`
  (E52 :186-199).
- IDLE-01 enhanced: `LearningEventScheduler` — `QueueIfReady("BUG", 5, "five edited DPO pairs...")`
  ставит в очередь `LearningEvent`; `DrainIfIdle` пока не idle не отдаёт, а при idle — отдаёт события
  (E52 :203-227). Но `LearningEventScheduler.cs:28-49` показывает: это **чистая очередь**
  (`Queue<LearningEvent>`), никакого обучения не происходит — `DrainIfIdle` просто возвращает
  накопленные события. «Five DPO pairs» = целое число `5`, а не реальные обученные пары.

Результат E52 (round-01 zip `run-2026-05-04-085736.json`): `baseline_cycle_success_rate: 0` (:10),
`enhanced_cycle_success_rate: 1` (:11), `improvement_pct: 100`, `passed: true`,
`next_action: PROMOTE_TO_NEXT_TIER`, `notes: "baseline=0%, enhanced=100%, improved=4/4, failures=[]"`
(:50-66); markdown-зеркало — «Baseline 0% / Enhanced 100%» (`run-...085736.md:5-6`). Манифест раунда:
E51/E52 + проекты 21-23 focused **144/144**, полный прогон **916/916**, «baseline 0% / enhanced 100%
across V06, CH-01, CH-02, IDLE-01» (`ROUND-MANIFEST-...E52.md:48-51`).

Вывод раздела 4: «0%→100%» — это **before/after контракт-демонстрация**, а не измерение обучения.
Baseline занижен конструкцией (3/4 = литерал `false`, V06 — keyword-ничья), enhanced = детерминированные
правила (keyword-роутинг, substring-examiner, захардкоженный duplicate-арбитр, очередь). Один и тот же
«enhanced»-код давал бы 100% и до, и после любого «обучения»; ни в одной из 4 трасс нет измерения
модели «до тренировки» против «после тренировки».

### 5. 60-слотовый вектор — что это значит

60-слотовый feature-вектор — это **фиксированный контракт кодирования** логической структуры, общий
для teaching и runtime (в этом весь смысл E51B: «одна и та же compiler-форма для обучения и рантайма»,
тест `E51B_TeachingAndRuntimeRecords_Use_TheSameCompilerShape`, :222-235). Рядом жёстко зафиксирован
словарь из **22 предикатов** (`PredicateVocabularySize == 22`, :229). Слоты — это позиции признаков
над этим предикатным словарём (22 предиката раскладываются в 60 слотов признаков: наличие/счёт
предикатов + производные позиции); это **инженерная ширина вектора для сравнения teaching↔runtime
K-NN-профилей**, а не «60 нейронов обученной сети». Тест проверяет только равенство ширин
(`Teaching.VectorWidth == Runtime.VectorWidth == 60`) и наличие строки `"60 slots"` в
`FeatureVectorContract` (:230-231, :286) — т.е. симметрию кодирования, а не обученные значения.

### 6. Вердикт: настоящее эмпирическое обучение или designed/simulated trace?

**Designed / детерминированная симуляция, а не эмпирическое доказательство обучения.** Основания:
1. Нет обучаемой единицы: ни градиента, ни весов, ни checkpoint-а, ни DPO-апдейта, ни export/import.
   «Enhanced» везде = детерминированный код: K-NN-retrieval по ручной разметке (E51A/E51B),
   keyword-классификатор (`InternalLlmGateway.Classify`), substring-examiner
   (`InternalResultExaminer`), захардкоженное арбитр-правило (`ContradictsLockedDuplicateRule`),
   очередь событий (`LearningEventScheduler`).
2. Замкнутый цикл «run → detect gap → teach → re-run → improve» **спроектирован, но не выполнен
   эмпирически**: «detect gap» = порог дистанции/сравнение confidence; «teach» = постановка
   `LearningEvent` в очередь (без тренировки); шаг «re-run → improve» с измерением одной и той же
   модели до/после отсутствует. E52 сравнивает не «модель до обучения vs после», а «baseline-код
   (частью литерал false) vs enhanced-код».
3. «0%→100%» и «6-of-6» — совпадение детерминированного конвейера с **ручным ground-truth**
   (Assert-ожидания в `DynamicFlowE51Tests.cs`; захардкоженные baseline-false в E52). Baseline
   занижен конструкцией.
4. `ExternalCallCount => 0` и «five DPO pairs = int 5» — это контрактные константы, а не измерения.

Что это ВСЁ ЖЕ доказывает (не обесценивать): (а) техническая цепочка компилятор→предикаты→routing→
K-NN→beautify исполнима на реальных inline-данных и бьёт случайный baseline (E51A: 0.283→0.967);
(б) арбитры реализованы как жёсткий гейт, перекрывающий даже высокое K-NN-сходство (E51B CH-02:
сосед ≥0.70, но verdict `rejected`); (в) internal-first + approval-gate + idle-очередь склеены в
исполнимый контракт (E52 144/144, 916/916). Это добротный **scaffold/контракт-пруф формы flow**,
согласующийся с выводами проходов 2-3 («430K = scaffold», «design+data оба фактически отсутствуют»):
E51/E52 показывают, что каркас закрытого цикла собран и детерминированно проходит, но **эмпирического
обучения динамической модели тут нет**. Это самое близкое к «доказательству, что цикл работает», но
по силе это designed trace, а не learned proof.

### 7. Что остаётся на проходы 5-9

- **Проход 5 (E53):** round-zip 12-15 (`e53-high-level-orchestration-proof-plan`, round A/B/C,
  round-15 `trained-splitter`) — там заявлен «обученный splitter», это ближайший кандидат на
  реальное обучение (проверить, есть ли checkpoint/веса или снова детерминизм).
- **Проход 6:** `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT`, `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN`,
  `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION`, `logical-structure-compiler-node-map-v1.csv`.
- **Проход 9:** найти реализацию E51A/E51B (`E51ACodeContextPipeline.cs`/`E51BDynamicFlowMiniE2E.cs`)
  — в этой папке её НЕТ; вероятно в снимке `source-examination-cache/llm_mvp_core-main` или в раунде,
  не попавшем сюда. Подтвердить, что «22 предиката / 60 слотов / K-NN» действительно детерминированы
  в коде, а не только в тестах.

### Честная строка покрытия (проход 4/10)

Прочитаны ЦЕЛИКОМ: `E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md`, `E51-END-TO-END-PIPELINE-PLAN-v1.md`,
`E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md`, `ROUND-MANIFEST-2026-05-04-PROJECTS-21-23-E52.md`; из round-01
zip прочитаны ЦЕЛИКОМ `E52Projects2123CycleComparison.cs`, `run-2026-05-04-085736.json/.md`,
`DynamicFlowE51Tests.cs`, `InternalLlmGateway.cs`, `LearningEventScheduler.cs`,
`InternalFirstAiSupport.cs`. Проверено grep-ом отсутствие реализации E51A/E51B в папке и в round-zip
02/04/06/07 и в `source-examination-cache/`. НЕ читались построчно (осталось на 5-9): `PermissionGate.cs`,
`ExternalAiStateGuards.cs`, `ResultWriter.cs`, `ExperimentRunner.cs`, `DECISIONS-LOCKED.md` (D-13/14/15
упомянуты в манифесте/плане), реализация E51A/E51B и E53-раунды. Вердикт прохода: E51/E52 — designed
closed-loop scaffold с ручным ground-truth и детерминированным «enhanced»; настоящего обучения
(веса/checkpoint/DPO-апдейт/пост-тренировочная метрика) в пакете нет.

---

## Pass 5/10 — E53 orchestration proof + scenario-001 trace + trained-splitter claim

Проход 5 отвечает на четыре вопроса: (1) что доказывает E53; (2) как выглядит трасса
сценария-001 по шагам; (3) round-15 «trained splitter» — это реально обученный юнит с весами
или снова детерминизм; (4) что означают числа 12/12 и 997/997. Все источники read-only; для
проверки кода распакован `round-zips/round-15-e53-round-c-trained-splitter-2026-05-04.zip`.

### Источники, прочитанные ЦЕЛИКОМ в этом проходе

| Файл | Что это |
|---|---|
| `E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN-v1.md` (25 KB) | План эксперимента: 12 доказуемых способностей, 8 abilities-под-тестом, pass-gates, 3 раунда A/B/C. |
| `E53-SCENARIO-001-EXPECTED-TRACE.md` (42 KB) | Оракул: ожидаемый выход КАЖДОЙ станции для одного тяжёлого сценария. Не обучающий набор. |
| `E53-ROUND-A-VALIDATION-RESULTS.md` | Валидация раунда A (документы + оракул; код не менялся). |
| `E53-ROUND-B-VALIDATION-RESULTS.md` | Валидация раунда B (детерминированный эксперимент; 985→997, focused 12/12). |
| `E53-ROUND-C-VALIDATION-RESULTS.md` + `ROUND-MANIFEST-...E53-ROUND-C-TRAINED-SPLITTER.md` | Раунд C — замена node-01 на «trained splitter». |
| `E53HighLevelOrchestrationProof.cs` (863 стр., из round-15 zip) | **Фактический код**, включая класс `Flow01Node01TrainedDomainSplitter`. |
| `run-2026-05-04-133842.json` (из round-15 zip) | Evidence-триплет раунда C (JSON). |

### 1. Что доказывает E53 (по замыслу и по факту)

**Замысел (plan :11-15, :36-52).** E53 — это **orchestration/management proof**, а не proof
качества модели. Центральный вопрос (plan :13): «может ли динамический поток управлять сложной
человеческой задачей, сохраняя структуру, зависимости, параллельные пути, граничные условия,
knowledge-edges, авторитет арбитра и человекочитаемый вывод, не привязываясь ни к какому
стеку/рантайму?». План перечисляет **12 способностей** (plan :38-52): промпт→логическая
структура без потери намерения; охват всех доменных территорий; array-узлы fan-out/fan-in без
потери пакетов; планировщик сохраняет порядок зависимостей/wait/parallel/rejection/boundary;
processor+applier по одному пункту; convergence ловит missing/conflict/dup/blocked;
knowledge-edge срабатывает; **0 живых внешних AI-вызовов** (только approval-ready context-pack);
arbiter-selector выбирает по формальным результатам, а не по финальному тексту; **вердикт
арбитра обязателен и не перекрывается** priority/KNN/budget/style; финальный вывод человекочитаем
(proceed/blocked/rejected/unknown/next-actions); evidence читается без перезапуска.

**Что E53 НЕ доказывает (plan :53-63, прямым текстом):** не доказывает качество локальной LLM
на новых данных; не доказывает живую внешнюю интеграцию; не порождает код/схемы БД/облачные
манифесты. «E53 is a management and orchestration proof».

**По факту (round B/C validation).** E53 доказывает, что **детерминированный оркестрационный
каркас** проходит все 12 pass-gates на одном захардкоженном сценарии: `lost_edge_count=0`,
массивы 11-in/11-out на node-02/04/05 (node-06 сворачивает 11→7 сохраняя классы статусов, Round B
:101-107), `dependency_violation_count=0`, `invalid_parallel_run_count=0`,
`invented_biometric_plan_items=0`, `duplicate_creation_accepted=false`,
`forbidden_stack_term_count=0`, `human_result_sections_present=5/5`, `live_external_calls=0`,
`learning_gap_packet_count=4`, `e2e_orchestration_success=true` (Round B :135-151; подтверждено в
JSON round-15 `"e2e_orchestration_success": true`). То есть E53 доказывает **форму управления**
(рёбра сохраняются, массивы держат контракт, зависимости соблюдены, арбитр режет дубликат-аккаунт,
biometric уходит в context-pack, финал stack-independent) — но НЕ обучение.

### 2. Трасса сценария-001 (что поток делает по шагам)

Сценарий `E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION` (plan :71-86): миграция онбординга —
часть юзеров начали с email+пароль, часть хотят Google-вход, часть бросили анкету на середине и
хотят продолжить позже; повторные provider-callback НЕ должны плодить дубликаты аккаунтов; нужно
принять terms/privacy до активации; если токен/сессия протухли — остановиться и запросить свежее
доверенное действие; стейкхолдер спрашивает про биометрическую паспорт-верификацию для high-risk;
другой стейкхолдер просит СОЗДАВАТЬ второй аккаунт при повторном callback ради аудита. Нельзя
выбирать язык/БД/фреймворк/облако. Файл трассы — **оракул** (trace :11-15: «It is not a training
set. It is the expected end-to-end orchestration behavior»).

Пошагово (trace, по узлам):
1. **node-00 logical-structure-compiler** (:41-89): человеческая задача → структура с
   `predicate_counts_22` (22-слотовый вектор предикатов), `domain_slots_5`
   (`planning/architecture/bug_fix/code_question/orchestration`), `normalized_roots`, meaning-graph
   (9 units + 6 dependency-рёбер), `conflicts` (audit-запрос дубля vs duplicate_prevention),
   `boundaries` (resume-rules-missing, policy-rules-need-evidence), `knowledge_edges`
   (missing biometric passport), `no_stack_constraint: true`, `confidence: 0.86`.
2. **topology-route** (:95-113): область `customer_onboarding_orchestration`, 5 sub-subjects,
   `route_status: complex_multidomain_orchestration`.
3. **node-01 domain-splitter** (:119-213): **11 доменных пакетов** d01…d11 (provider-identity,
   email-state, duplicate-prevention, token-session, questionnaire, resume-boundary,
   policy-boundary, biometric-knowledge-edge, audit-traceability, arbiter-review-required,
   human-readable-decision). Каждый пакет несёт `coverage_status`
   (covered/boundary/knowledge_edge/conflict_supporting_context/required). `split_confidence: 0.86`.
4. **node-02 translator-array** (:215-299): 11 пакетов → 11 формальных statement’ов, маркеры
   конфликта/границы/knowledge-edge сохранены (11-in/11-out).
5. **node-03 scheme-planner** (:301-433) — **ядро трассы**: 11 задач b01…b11 с `depends_on`,
   `allowed_parallel_with`, `stop_or_wait_condition`, `review_marker`. Явные:
   `dependency_rules` (b03←b01,b02; b04←b03 режет дубль; b09←b03,b04,b05,b08),
   `parallel_groups` (2 группы), `wait_states` (5), `recovery_or_rejection_rules` (reject-duplicate,
   request-fresh-trust, boundary-resume, block-activation, learning-gap-biometric).
6. **node-04 processor-array** (:439-529): по каждой задаче — success/failure/wait/review/reprocess
   (11-in/11-out).
7. **node-05 applier-array** (:531-599): 11 applied-results со статусами
   accepted/rejected_branch/blocked/boundary/knowledge_edge/pending.
8. **convergence-gate** (:601-639): раскладка accepted(4)/blocked(4)/knowledge_edge(1)/
   rejected(1)/pending(1), конфликты и сохранённые зависимости названы отдельно.
9. **knowledge-edge context-pack** (:641-683): `cp01_biometric_passport_verification`,
   `live_external_call: false`, `approval_required_before_live_call: true` — pack готовится, но
   **живой вызов не делается**.
10. **node-07 arbiter-selector** (:685-722): 7 обязательных арбитров (identity_consistency,
    token_safety, policy_evidence, resume_state_coverage, knowledge_edge_non_invention,
    audit_traceability, stack_independence).
11. **node-08 arbiter-units** (:728-783): вердикты. Ключевое — `identity_consistency:
    rejected_fragment_found` на дубликат-аккаунт; `audit_traceability:
    accepted_with_rejection_boundary` (аудит не воскрешает дубль).
12. **node-06 result-translator** (:785-833): 11 формальных → 7 человеческих фрагментов, статусы
    (can_proceed/rejected/blocked/boundary/knowledge_edge/next_actions) сохранены.
13. **node-09 formatter** (:835-869): финальный ответ с 5 секциями **Can proceed / Blocked /
    Rejected / Unknown / Next actions**, без единого стек-термина.
14. **learning-gap-packets** (:871-904): 4 пакета (biometric knowledge-edge, resume-rules,
    policy-rules, duplicate-rejection-negative), каждый нацелен на конкретную станцию/роль.

Итог трассы: `orchestration_completed_with_rejections_boundaries_and_learning_gaps`,
`live_external_calls: 0`, `stack binding: none` (trace :29-39).

### 3. Round-15 «trained splitter» — вердикт: ДЕТЕРМИНИЗМ, не обученный юнит

Round C (manifest + validation) заявляет, что node-01 теперь идёт **через «trained Flow-01
splitter adapter» вместо прежнего direct-fixture** (Round-C validation :13). Проверка кода
`E53HighLevelOrchestrationProof.cs` (round-15 zip) показывает, что это **захардкоженная таблица
правил с фильтром по точному совпадению строк**, а не модель:

- Класс `Flow01Node01TrainedDomainSplitter` (:368-435). Внутри — статический массив
  `LearnedRules` из **11 захардкоженных записей** `LearnedDomainPacketRule` (:371-384). Каждая
  запись = `(PacketId, DomainLabel, CoverageStatus, PromptFragment, ReasonForInclusion,
  RequiredSignals[])`, где `PromptFragment`/`ReasonForInclusion` — **дословно те же строки, что в
  оракуле** trace node-01.
- `Split()` (:387-397) — вся «инференция»: `LearnedRules.Where(rule =>
  rule.RequiredSignals.All(signals.Contains)).Select(...)`. Это **чистая проверка вхождения строк
  в `HashSet<string>`** (ordinal, точное совпадение).
- `BuildSignalSet()` (:400-434) складывает в `HashSet<string>` units/conflicts/boundaries/
  knowledge-edges/required-outputs из node-00 плюс агрегатные флаги (`has_conflict`,
  `has_boundary`, `has_knowledge_edge`, `no_stack_constraint`).
- Вход node-00 (`CompiledLogicalStructure`) — **сам захардкоженный литерал** (:325-361: списки
  units/dependencies/conflicts/boundaries/knowledge-edges/required-outputs + `true`). Поэтому все
  требуемые сигналы всегда присутствуют, и splitter **всегда отдаёт ровно те же 11 пакетов** —
  фиксированное отображение 11→11, не зависящее от содержания по-настоящему.
- **grep по всему файлу (863 стр.): 0 совпадений** на `double[]`, `float`, `matrix`, `tensor`,
  `gradient`, `backprop`, `checkpoint`, `.Save(`, `.Load(`, `Adam`, `loss`, `epoch`, `argmax`,
  `logit`. Нет весов, градиента, checkpoint-а, DPO-апдейта, export/import. «Learned»/«trained» —
  только в именах и комментариях (`LearnedRules`, `Flow01Node01TrainedDomainSplitter`, «learned
  packet shapes»).

Само же валидационное признание закрывает вопрос (Round-C validation :92-95): **«No shared
production Flow-01 splitter class exists in the codebase yet. Round C therefore introduced an
experiment-local trained adapter … backed by the accepted Flow-01 node-01 training rules … a
truthful implementation of Round C inside the current experiment architecture.»** Манифест
(:38-40): «The trained splitter contains **learned packet rules** … Each rule has **required
structure signals**. The splitter emits a packet only when those required signals are present» —
это описание **правило-фильтра**, а не обучения.

Дополнительно: замена fixture→«trained» дала **байт-в-байт тот же выход** (те же 11 packet-id,
Round-C validation :49-61) и **не изменила ни одного числа** (baseline 997/997 до, 997/997 после;
manifest :85-91). «Обучение», не меняющее ни выхода, ни метрики, — это подмена имени, а не
обученная фича. **Это тот же designed/deterministic паттерн, что E51/E52 (вывод прохода 4):**
детерминированный код в наряде «trained». По терминам CLAUDE.md это ровно класс
`hidden determinism / masquerade` (R4, Deterministic-Boundary review gate): нет ни одного из
обязательных признаков обученного юнита (trained state, checkpoint/manifest, export/import,
locator, fresh-load, continue-training, ablation).

### 4. Что означают 12/12 и 997/997

- **12/12** — это **12 сфокусированных поведенческих E53-тестов** (`dotnet test … --filter "E53"`,
  Round B :33-44). Их имена (Round B :66-77): `E53_Run_WritesJsonMarkdownAndMermaidEvidence`,
  `E53_EvidenceJson_Uses_RequiredMetricKeys`, `E53_Run_PreservesAllHardEdges`,
  `E53_Run_PreservesArrayFanOutAndFanInCounts`, `E53_Run_EnforcesDependencyOrder`,
  `E53_Run_AllowsOnlyValidParallelGroups`, `E53_Run_RoutesBiometricPassportToKnowledgeEdge`,
  `E53_Run_RejectsDuplicateAccountCreation`, `E53_Run_KeepsFinalResultStackIndependent`,
  `E53_Run_FinalHumanResultPassesChecklist`, `E53_Run_ProducesLearningGapPacketsForMissingCoverage`,
  `E53_Run_ModelsPolicyGapAsEarlyStartAndResolvedDependency`. Они проверяют **структуру
  оркестрации** (сохранение рёбер, счётчики fan-out/fan-in, порядок зависимостей, режет ли арбитр
  дубликат, stack-independence, запись evidence-файлов) — т.е. что детерминированная трасса
  совпадает с оракулом. Это **НЕ тесты обучения**. (План изначально называл 10 тестов E53_Run_*
  (plan :697-711 / Round A :97); Round B добавил 12.)
- **997/997** — это **весь тест-сьют LlmFromScratch** (`dotnet test … Tests\LlmFromScratch.Tests.csproj`).
  Baseline до E53 = **985/985**; +12 E53-тестов в раунде B → **997/997** (Round B :20-27); раунд C
  число не изменил (997→997, manifest :85-88). То есть 997/997 = «полный сьют зелёный, регрессий
  нет», а не измерение модели.

Важно: тела 12 E53-тестов физически **в этой папке отсутствуют** — они в репозитории
(`C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj`), как и сам продакшн-код станций;
в round-15 zip лежит только `E53HighLevelOrchestrationProof.cs` + evidence-триплет + манифест.
Это согласуется с находкой прохода 4 (реализация E51A/E51B тоже не в папке).

### 5. Критический вывод прохода 5

E53 = **самый полный designed orchestration proof** в папке, и он честно так и заявлен планом
(«deterministic first», plan :687-691; Round C = замена ОДНОЙ станции). Он доказывает, что
**оркестрационный каркас 9-узлового потока исполним и детерминированно проходит 12 gate’ов на
одном тяжёлом сценарии**, с реальными полезными свойствами: рёбра не теряются, массивы держат
контракт, планировщик хранит зависимости/параллелизм/wait, арбитр жёстко режет дубликат-аккаунт,
biometric не выдумывается (уходит в context-pack без живого вызова), финал человекочитаем и
stack-independent. Но **эмпирического обучения динамической модели в E53 нет**: node-00 — фикстура,
node-01 «trained splitter» — правило-фильтр по 11 захардкоженным правилам без весов, node-02…09 —
детерминированные фикстуры (Round C validation :43). «trained» дало нулевую наблюдаемую разницу.
Это подтверждает и усиливает сводный вывод RAG-06 (проходы 2-4): **дизайн богатый, каркас
исполним, но и данные (430K = scaffold), и «обученные» юниты (E51/E52/E53) фактически
детерминированы; настоящей обученной динамической модели в папке пока нет.** Для плана
ПРОДОЛЖЕНИЯ расширений VS/VS Code это значит: E53-трасса — отличный **готовый оракул/контракт**
для UI-осмотра потока (examine-аффорданс, O-4), но пункт «заменить фикстуры обученными юнитами по
одному» (plan :737, Round D — заменить node-00) остаётся невыполненным.

### Что остаётся на проходы 6-9

- **Проход 6:** амендменты `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` (15 KB) +
  `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` (24 KB) +
  `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` (29 KB) +
  `logical-structure-compiler-node-map-v1.csv`. Проверить: knowledge-edge/external-AI как advisory,
  форма 60-слотового компилятора, mini-E2E-план.
- **Проход 7:** `all-documents-method-run/` генераторы (`.mjs`/`.ps1`) + CSV-манифесты корпуса +
  выборочный разбор пакетов `full-corpus-unique` / `full-active-source` (подтвердить шаблонность на
  большем числе примеров).
- **Проход 8:** `business-flows/` (33 бизнес-флоу-пакета) + `all-files-including-zips-compact/`.
- **Проход 9:** `source-examination-cache/llm_mvp_core-main` (снимок кода — искать реальную
  реализацию станций/E51A/E51B, если есть) + остальные `round-zips/` (round-08..14: обучающие пары
  N00-N03, E52, E53 round A/B) + `WHAT-TO-DO/*`, `SKILLS-BINDING-PROOF-PLAN`, `previos session.txt`,
  `.drawio.xml`/`.svg`/`.mmd`.

### Честная строка покрытия (проход 5/10)

Прочитаны ЦЕЛИКОМ: `E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN-v1.md`,
`E53-SCENARIO-001-EXPECTED-TRACE.md` (42 KB, все узлы), `E53-ROUND-A/B/C-VALIDATION-RESULTS.md`,
`ROUND-MANIFEST-...E53-ROUND-C-TRAINED-SPLITTER.md`; из `round-15` zip прочитан ЦЕЛИКОМ ключевой
код `E53HighLevelOrchestrationProof.cs` (класс `Flow01Node01TrainedDomainSplitter` :368-435,
node-00 фикстура :325-361, `BuildDomainPackets` :364-365) + сверка `run-2026-05-04-133842.json`.
Grep по всему .cs подтвердил отсутствие весов/тензоров/градиента/checkpoint/DPO/loss (0 совпадений).
НЕ читались построчно (осталось на 6-9): `E53-EXECUTOR-ROUND-CONTRACT.md`,
`E53-LEARNING-SET-TRANSLATION-MATRIX.md`, `E53-ORCHESTRATION-EVIDENCE-SCHEMA.md`,
`E53-HUMAN-RESULT-CHECKLIST.md`, `E53-SCENARIO-001-SOURCE-PACKET.md` (характеризованы по плану и
трассе); тела 12 E53-тестов (в репозитории, не в папке). Вердикт прохода: E53 — designed
deterministic orchestration proof; round-15 «trained splitter» = правило-фильтр без весов (тот же
masquerade-класс, что E51/E52); 12/12 = структурные E53-тесты, 997/997 = полный сьют без регрессий;
настоящей обученной динамической модели в E53 нет.

---

## Pass 6/10 — v2 слой: node-00 компилятор + intake/topology + external-AI advisory + knowledge-edge/learning-gap/idle-queue

Проход 6 разбирает **управляющий / само-обучающий слой v2** — ту обвязку, которая
превращает 9-узловое ядро в *самомодифицирующуюся* систему. Это самый важный проход для
целевого потока «внешняя модель обучает внутреннюю через DPO» (шаг 6 целевого флоу): именно
здесь описан механизм, как система (а) сжимает любой промпт в стабильную логическую структуру
ДО узлов, (б) сужает вход перед split, (в) зовёт внешний ИИ ТОЛЬКО как advisory на «краю знания»,
и (г) складывает этот внешний ответ в отложенную обучающую очередь. Прочитаны ЦЕЛИКОМ 4 документа.

### Источники, прочитанные в этом проходе (все read-only)

| Файл | Размер | Что это |
|---|---|---|
| `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` | 29 KB | **Спека node-00 (logical-structure compiler)** — 10 шагов компилятора + провенанс каждого шага из экспериментов E01/E08/E15/E16/E17/E20/E23/E25-27; 10 архитектурных правил; влияние на все 9 узлов; полная Mermaid-топология. |
| `logical-structure-compiler-node-map-v1.csv` | 7 KB | **Карта узлов компилятора** — LSC-01…LSC-10 (компилятор) + DF-01…DF-10 (динамический слой), для каждого: main_input, main_output, **experiment_or_session_basis**, design_note. |
| `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` | 24 KB | **План мини-E2E (E51)** — 18 записей на одной семье (регистрация), 60-слотовый feature-вектор, 6/6 gate, CH-01 knowledge-edge / CH-02 arbiter-rejection, места кода в репозитории. |
| `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` | 15 KB | **Амендмент внешнего ИИ** — 8 станций knowledge-edge пути, can/cannot внешнего ИИ, порядок авторитета, реестр поставщиков, привязка к проектам 21-23. |

### 1. node-00 = logical-structure compiler (raw prompt → стабильная логическая структура)

**Главная находка документа.** Отсутствующий верхнеуровневый элемент дизайна — **общий компилятор
логической структуры**, который стоит ПЕРЕД domain-splitter’ами, ПЕРЕД dataset-slicer’ами и перед
большинством малых юнитов. Одна работа: `сырой человеческий/документный текст → стабильная
логическая структура`. **Ключевое правило (Rule 1): один и тот же компилятор используется и для
runtime-промпта, и для подготовки обучающих наборов** — иначе юнит в проде получит вход не той
формы, на какой учился. Это прямо закрывает вопрос из прохода 2 («на каком языке учатся узлы?»):
не на сыром тексте, а на логической структуре, а сырой текст хранится рядом для fidelity-проверки и
reverse-перевода.

**10 шагов компилятора (LSC-01…LSC-10 из CSV) — каждый мапится на конкретный ПРОЙДЕННЫЙ эксперимент:**

| # | Узел | Вход → выход | Основа (эксперимент) |
|---|---|---|---|
| LSC-01 | **Source role gate** | сырой текст → роль источника (runtime-запрос / требование / коррекция / версия / правило / failed / approved) + evidence-статус | product sessions 6,7 |
| LSC-02 | Language & pattern translator | текст+роль → каноничный порядок слов, ясная формулировка (тот же смысл) | **E25/E26/E27** (passed strongly) |
| LSC-03 | **Context marker detector** | текст ДО нормализации → важные маркеры, которые нельзя потерять | **E20 StackDetector** |
| LSC-04 | Structural role tagger | текст с маркерами → роли (action/target/condition/value/list/rule/section/result) | **E01 AstTokenizer, E08 SyntaxRoles, E09** |
| LSC-05 | Semantic normalizer | текст+роли → чистые концепт-термины (меньше декора/филлера) | **E15 SemanticNormalizer** |
| LSC-06 | Family collapser | нормализованные термины → малые семьи действий/смыслов (get/fetch/find/load = одно) | **E16 FamilyCollapser** |
| LSC-07 | **Logic predicate extractor** | семьи+роли → компактная последовательность логических предикатов | **E17 LogicExtractor** (passed **очень сильно** — главный прирост) |
| LSC-08 | **Meaning graph assembler** | предикаты+роли+source-role+evidence → связный **meaning-graph** (кто/действие/объект/условие/результат/зависимости/источник/пробелы) | semantic-NLP plan; sessions 6,7 |
| LSC-09 | **Fidelity & coverage checker** | оригинал+структура → missing/added/ambiguous + pass/rework | reverse-fidelity session notes |
| LSC-10 | **Reverse logical translator** | одобренная структура → читаемый домен/пользовательский язык | **E23 TemplateBeautifier, E25-27** |

**Критично про порядок: LSC-03 (marker-detector) ОБЯЗАН идти ДО LSC-05 (нормализации)** — правило из
E20 (нормализация уничтожает маркеры контекста). Это же правило было в каноническом порядке прохода 1.

**Warning-эксперименты (что НЕ делать) — тоже в спеке:** E07 (broad context token) — **провалился**
(шум); E22 DatasetRouter — **слабо валидирован** (нужны богаче данные); E24 AttributedRouter —
**провалился** (ранняя атрибуция добавляет шум > пользы); E20 — ordering-правило выше. Вывод:
компилятор не должен стать «шумным мешком тегов» — только чистая структура с явными полями,
evidence и confidence.

**⚠️ ГЛАВНОЕ ОТЛИЧИЕ ОТ ПРОХОДОВ 4-5: node-00 компилятор — ЕДИНСТВЕННЫЙ узел, у которого есть РЕАЛЬНЫЙ
КОД в репозитории (не фикстура).** Mini-E2E-план (`DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN` :27-42)
прямо фиксирует: в `29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs` УЖЕ реализованы 8 шагов
конвейера — (1) токенизация, (2) **stack-detection ДО нормализации**, (3) route-classification,
(4) semantic-normalization, (5) family-collapsing, (6) logic-predicate-extraction, (7) опц.
decomposition, (8) опц. beautification. `LogicExtractor` имеет **фиксированный словарь из 22
предикатов** — «маленький внутренний язык». Это ровно шаги LSC-02…LSC-10. То есть **node-00 из
дизайна ≈ существующий SemanticCompiler-конвейер**. Это самая «настоящая» часть всей папки. Важная
честная оговорка: это **детерминированный NLP-конвейер** (правила/словари/классификаторы из E15-E27),
а не обученная нейромодель — но, в отличие от E51/E52/E53-«trained» узлов (проходы 4-5), он реально
исполним и оттестирован.

**60-слотовый feature-вектор (валидирован E51B):** `22 слота счётчиков предикатов + 5 слотов one-hot
залоченного домена + до 32 слотов нормализованных корней (из SemanticNormalizer) + 1 слот счётчика
декомпозиции = максимум 60`. Правило: **вектор строится НЕ из сырого текста напрямую** — только из
выходов того же компилятора (нормализованные корни разрешены, т.к. приходят из compiler-path, а не
как отдельный shortcut). **Relevance: критическая** (это база «шага 6» — то, на чём реально можно
обучать/дообучать малый узел).
**Теги:** #node-00 #logical-structure-compiler #22-предиката #60-слотов #29.SemanticCompiler #E17 #E20-ordering.

### 2. intake / topology narrowing — сужение перед split (доказано «в малом» на mini-E2E)

Дизайн сужения (intake-gate → system-topology-router из **14 областей** → sub-subject-routers →
local-domain-splitters) был описан в проходе 1. Mini-E2E доказывает его **в минимальной серьёзной
форме** и разрешает ключевой архитектурный вопрос — *зачем* нужно сужение после логической компиляции:

- **Локальные субъекты ≠ залоченные домены.** Компилятор-домены залочены как `CODE_Q / BUG / PLAN /
  ARCH / REFACTOR` (D-04) и эксперимент их **не меняет**. Вместо этого вводятся **4 локальных
  субъект-метки** уровня эксперимента (ниже широкого домена «регистрация»): `AUTH_PROVIDER_ENTRY`,
  `EMAIL_VERIFICATION`, `IDENTITY_DUPLICATE_PREVENTION`, `TOKEN_SESSION_SAFETY`. Локальный субъект —
  это **результат routing’а (через K-NN по обучающим записям)**, а не новый домен компилятора.
- **Baseline vs Enhanced (почему нужна топология-сужение):** baseline = только залоченный
  compiler-домен → «слишком широко» (много регистрационных запросов падают в один домен, напр. `PLAN`);
  enhanced = полный feature-packet (predicate-counts + normalized-roots + decomposition + K-NN по
  12 обучающим записям) → **различает 4 локальных субъекта**. Это и есть эмпирическое обоснование
  narrowing-слоя v2.
- **K-NN = память, не авторитет.** Ключевое правило, тестируемое на CH-02: K-NN может выбрать
  ближайшие примеры (даже с высоким сходством), но **принимает решение арбитр**, не сходство.

**Relevance: очень высокая.** **Теги:** #narrowing #локальные-субъекты #залоченные-домены #KNN-память #baseline-too-broad.

### 3. external-AI advisory rule — внешний ИИ как «канал помощи», НЕ авторитет

Амендмент `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1` формализует единственный разрешённый путь наружу.
**Правило (Final Rule):** локальные юниты делают работу, когда компетентны; внешний ИИ зовётся, только
когда локальная система может **объяснить, почему она недостаточно компетентна**; ответ адаптируется
обратно в поток и проверяется арбитрами до принятия. Ответ внешнего ИИ **никогда не входит в финальный
вывод напрямую** — возвращается через динамический поток.

**8 станций knowledge-edge пути (= DF-06…DF-10 в CSV):**
`knowledge-edge-detector → external-help-planner → external-context-pack-builder → external-AI-chain →
external-result-adapter → affected local station → arbiters → learning-gap-packet → idle-learning-queue`.

- **Station 2 External Help Planner** решает: один поставщик или несколько; broad/narrow; split на
  несколько внешних вопросов; cross-check; что можно выпустить наружу; какую форму ответа требовать.
  Он **не решает задачу — только проектирует цепочку помощи**.
- **Station 3 Context Pack Builder:** внешний ИИ получает НЕ весь корпус, а компактный структурный
  пакет (оригинальный запрос + логическая структура + маршрут + упавшая станция + local-result-so-far
  + arbiter-warnings + разрешённый evidence + точный вопрос + требуемая форма ответа + **границы: что
  НЕ предполагать / НЕ менять / что сохранить**). Чувствительный материал сокращается/маскируется/блокируется.
- **Station 5 Result Adapter:** переводит внешний ответ обратно в язык станций (исправленная логическая
  структура / новый перевод домена / починенный кусок плана / недостающая зависимость / warning /
  фрагмент результата / кандидат-обучающий-пример / «спросить пользователя» / «ответ непригоден»).
  Сохраняет: какой внешний ответ породил результат, что изменено/добавлено/не доверено, какая станция
  получает результат. **Эта станция не даёт внешнему ИИ писать в финальный ответ.**

**Что внешний ИИ МОЖЕТ:** объяснить недостающий концепт, предложить починку, сравнить альтернативы,
оспорить локальное допущение, найти пропущенный edge, предложить формулировку, конвертировать
неизвестную структуру в известную, дать внешнее знание (когда разрешено).
**Что внешний ИИ НЕ МОЖЕТ:** перекрыть rejection арбитра; стать обучающей истиной без валидации;
обойти source-authority; писать прямо в финал; заменить локальную топологию; тихо изменить запрос
пользователя; стереть упавшие локальные попытки; спрятать неопределённость.
**Порядок авторитета:** external ИИ vs арбитры → **побеждают арбитры**; external ИИ vs source-authority
→ **побеждает source-authority** (если сам источник не исследуется как возможно неверный); поставщики
спорят между собой → расхождение записывается и уходит арбитрам/пользователю.

**Реестр поставщиков** (что судит хорошо / какой материал может получать / cost / privacy-level /
reliability / known-failures / output-formats / runtime-vs-idle-only). **user-clarification** — параллельный,
структурно похожий канал: если недостающее — это preference/intent/permission/private/business, спрашивают
ПОЛЬЗОВАТЕЛЯ, а не внешний ИИ.

**Привязка к коду (проекты 21-23) — уже названы рантайм-компоненты** (амендмент :10-22): `PermissionGate`
(single-use approval на `call_external_ai`), `ExternalAiRequestPackage` (конкретный context-pack),
`ExternalAiResultAdapter` (путь возврата в локальную структуру), `InternalResultExaminer` (внутренняя
проверка качества), `LearningEventScheduler` (передача в idle-обучение). **Round 07** усилил
`ExternalAiResultAdapter`: он проверяет внешние ответы через **именованную таблицу arbiter-правил** по
`ArbiterConstraints` пакета; первые правила — `identity_consistency` (реджект дубликат-аккаунта) и
`token_safety`; неизвестные имена правил НЕ реджектят по умолчанию (нужна явная запись + focused-тесты).

**Locked D-14 (из DECISIONS-LOCKED, проход 1):** Internal-LLM всегда первый; external-AI только с явным
single-use approval + названной missing-competence; все результаты осматриваются внутренней моделью и
хранятся как training-сигналы; обучение — только в idle под IdleDetector; новый адаптер ставится только
если `after-accuracy > before-accuracy`; граница уверенности 0.50 (E51A/E51B, CH-01/CH-02).
**Relevance: критическая** (это и есть «внешняя модель как учитель»). **Теги:** #external-ai-advisory #8-станций #арбитр-побеждает #D-14 #проекты-21-23 #round07-arbiter-table.

### 4. knowledge-edge + learning-gap + idle-queue = механизм ОТЛОЖЕННОГО обучения

Три станции складываются в главную само-обучающую петлю (DF-06 и DF-10 в CSV):

**(a) Knowledge Edge Detector** отвечает «достигли ли границы локальной компетентности?». Вход: проверенная
логическая структура, маршрут, выбранные cells, качество KNN-соседей, missing-evidence, arbiter-warnings,
failed-attempts, confidence, source-authority, user-constraints. **6 типов края:** `no edge` (продолжать
локально) / `soft edge` (локально + усиленные арбитры) / `hard edge` (спросить внешний ИИ) / `user edge`
(спросить пользователя — недостающее личное/preference) / `corpus edge` (искать в локальных документах
снова) / `learning edge` (записать gap для idle-обучения). **Жёсткое правило: детектор обязан НАЗВАТЬ,
чего не хватает — никогда только «low confidence».**
**Failed-attempt rule + пороги (v1):** failed attempt = один полный проход через станцию, кончающийся
блокирующим результатом (тот же арбитр реджектит по той же причине / та же зависимость отсутствует / тот
же convergence-конфликт / то же evidence недоступно…). Порог: **немедленный hard edge** (нет trusted-юнита
/ нет allowed-evidence / явно нужно внешнее знание); **2 подряд** с той же причиной после 1 repair;
**3 всего** на станции (даже если причины связаны, но не идентичны).

**(b) Learning Gap Packet** — создаётся на КАЖДОЕ external-help событие; это **мост между runtime-помощью
и будущим обучением малого юнита**. Поля: оригинальный запрос/текст, логическая структура ДО помощи,
станция-край, причина края, локальные попытки/провалы, использованные KNN-соседи, arbiter-warnings/verdicts,
external context-pack, внешние вопросы, внешние ответы, адаптированный результат, финальный вердикт арбитра,
**класс материала: positive / negative / warning**, и **какая роль-юнита + домен должны из него учиться позже**.

**(c) Idle Learning Queue** — «система НЕ переобучается на hot-path». Триггерит **IdleDetector из
16.AutonomousLearning**, когда фоновая работа безопасна. Триггер проверяет: нет активного запроса за те же
ресурсы; compute-budget позволяет; thermal/power позволяют; в очереди достаточно пакетов для полезного
батча; source-evidence доступно; rollback+conformance-сравнение выполнимы. При срабатывании: **группирует
learning-gap пакеты по роли+домену; accepted → positive; rejected → negative/warning; создаёт малые
обучающие наборы; гоняет conformance-suite; сравнивает новый юнит с предыдущим; ставит ТОЛЬКО если новый
проходит; хранит rollback-инфо.** Цель: «тот же knowledge-edge должен случаться реже после того, как система
на нём научилась» → превращение внешней помощи в локальную компетентность со временем.
**Relevance: критическая.** **Теги:** #knowledge-edge #6-типов-края #learning-gap-packet #idle-queue #positive-negative-warning #IdleDetector #conformance-install-if-better.

### 5. Mini-E2E план (E51) — минимальная замкнутая проверка всей петли

**Что проверяет один вопрос:** может ли крошечный обучающий + крошечный тестовый набор пройти один и тот
же logical-structure-путь и дать трассируемые корректные решения станций, при этом арбитры остаются жёстким
гейтом, а неизвестные кейсы становятся learning-пакетами.

- **Датасет = 18 записей на ОДНОЙ семье `Flow-01 user registration`:** 12 обучающих (TR-01…TR-12, по 3 на
  каждый из 4 локальных субъектов) + 4 held-out (TE-01…TE-04) + 1 knowledge-edge challenge (CH-01) + 1
  arbiter-hard-gate challenge (CH-02). «Если не работает здесь — не прояснится добавлением сотен файлов».
- **CH-01 (biometric passport verification):** ожидается **knowledge-edge** — система видит, что это рядом с
  регистрацией, но НЕТ trusted-локального пакета для биометрии → **не выдумывает ответ, строит context-pack +
  learning-gap packet**. Missing-competence формулируется человекочитаемо: «локальный набор знает
  provider-signin/email-verification/duplicate-prevention/token-safety, но НЕ знает, как биометрия меняет флоу».
- **CH-02 (создать второй аккаунт на повторный callback):** ожидается **arbiter rejection** — K-NN найдёт
  дубликат-примеры с высоким сходством, но арбитр `identity_consistency` **реджектит**, т.к. это противоречит
  duplicate-prevention. Это тест правила «K-NN выбирает память, арбитры решают приёмку».
- **Gate:** первичная метрика `e2e_trace_success_rate = 6/6` (4 held-out + 2 challenge), **5/6 не принимается**;
  полный сьют `≥ 877/877`; 18/18 покрытие компилятором; teaching/runtime-симметрия (один feature-метод).
- **3 крошечных арбитра** (детерминированные судьи, «пока не обязаны быть LLM»): `identity_consistency`,
  `token_safety`, `verification_order`.
- **Место кода:** новое — `35.ExperimentsV2/Experiments/E51DynamicFlowMiniE2E.cs` + `Tests/DynamicFlowMiniE2ETests.cs`;
  НЕ менять — domain-enum `29.SemanticCompiler/Core/Core.cs`, словарь предикатов `LogicExtractor.cs`. Внешний
  вызов в первом эксперименте **симулированный** (фиксированный ответ), живой поставщик — «позже».
**Relevance: очень высокая** (мост дизайн→код, конкретный оракул трасс). **Теги:** #mini-e2e #E51 #18-записей #6-из-6 #CH-01-knowledge-edge #CH-02-arbiter-reject #симулированный-внешний.

### 6. КАК ЭТО МАПИТСЯ НА «внешняя модель обучает внутреннюю через DPO» (шаг 6 целевого флоу)

Это центральный вывод прохода 6. Самообучающая машинерия v2 — это **буквально дизайн «external teaches
internal via DPO»**, но реализованный как *отложенный* контур, не hot-path:

1. **Сигнал учителя берётся только на named knowledge-edge.** Внешний ИИ (учитель) зовётся не всегда, а
   когда детектор может назвать недостающую компетенцию (advisory, D-14 single-use approval).
2. **Внешний ответ становится DPO-материалом через Learning Gap Packet.** Пакет уже несёт готовую
   разметку пар предпочтений: `positive` (принятый/адаптированный результат = *chosen*), `negative` (упавшие
   локальные попытки / реджекнутый внешний ответ = *rejected*), `warning` (соседи-предупреждения). Плюс поле
   «какая роль-юнита + домен учится» = адресация конкретного малого узла (dataset-grid = **unit × domain**,
   не только domain).
3. **Тренировка — в idle, по одному узлу, с install-if-better + rollback.** Idle-queue группирует пакеты по
   роли+домену → малый обучающий набор → conformance-suite → сравнение new-vs-previous → **install только если
   after > before** (D-14) → rollback сохранён (versioning-by-construction, принцип 4). Это ровно DPO-петля
   `11.DPO/DpoTrainer` из плана RAG-05, но с внешним ИИ как источником сигнала предпочтения.
4. **Внутренняя модель остаётся авторитетом.** Арбитры > внешний ИИ; source-authority > внешний ИИ; внешний
   ответ не пишет в финал. То есть учитель даёт *сигнал*, но *приёмку* всегда решает внутренняя система — это
   защищает DPO от reward-hacking’а внешним ИИ (соответствует R4/Acceptance-signal-independence из CLAUDE.md).

**⚠️ ЧЕСТНЫЙ СТАТУС DESIGN vs REAL (не переоценивать — согласуется с проходами 4-5 и RAG-05):**
- **node-00 компилятор** — **частично РЕАЛЕН** (`29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs`,
  22-предикатный `LogicExtractor`), но это детерминированный NLP-конвейер, не обученная сеть. Это самая
  сильная «настоящая» опора для шага 6.
- **narrowing / K-NN local-subject routing** — DESIGN + заявленная E51B-валидация (6/6), но по вердикту
  прохода 4 сами E51/E52 = **симулированы, не обучены**; K-NN-код (`StructuralKnnPredictor`,
  `BruteForceIndex`, `E22DatasetRouter`) существует, но 6/6 — это designed/детерминированная трасса.
- **external-AI advisory + knowledge-edge + learning-gap + idle-queue** — DESIGN + **названы** рантайм-куски
  проектов 21-23 (`PermissionGate`, `ExternalAiRequestPackage`, `ExternalAiResultAdapter`,
  `InternalResultExaminer`, `LearningEventScheduler`) + Round-07 arbiter-table. Но **живой вызов внешнего
  поставщика и реальное idle-DPO-дообучение НЕ реализованы** — mini-E2E явно использует *симулированный*
  внешний ответ и НЕ переобучает (idle-replay + install-gate перечислены как FUTURE-раунд: «replace simulated
  external help with supplier adapters, add idle-time replay and installation gates»).

**Итог для сводного плана расширений VS/VS Code:** дизайн «внешний учит внутреннего через DPO» **полон и
конкретен** (детектор края → learning-gap с positive/negative/warning → idle-DPO по unit×domain →
install-if-better+rollback), провенанс компилятора привязан к пройденным экспериментам, а node-00 имеет
реальный SemanticCompiler-код. Но **сам контур внешний-ИИ→DPO-дообучение пока не подключён** (симуляция +
названные, но не доказанные рантайм-компоненты) — это ровно та дыра, что RAG-05 называет
`DESIGN-COMPLETE, CODE-ABSENT`. Для расширений это значит: knowledge-edge/learning-gap-пакет — отличный
готовый **контракт для UI** (показать пользователю «край знания», context-pack, почему спросили внешний ИИ,
что ушло в idle-обучение — прямая привязка к examine-аффордансу O-4), но пункт «реально дообучить малый узел
из external-help пакета» остаётся невыполненным.

### Что остаётся на проходы 7-9 (без изменений, подтверждено)

- **Проход 7:** `all-documents-method-run/` генераторы (`.mjs`/`.ps1`) + CSV-манифесты корпуса + выборочный
  разбор пакетов `full-corpus-unique` / `full-active-source` (подтвердить шаблонность на большем числе примеров).
- **Проход 8:** `business-flows/` (33 бизнес-флоу-пакета) + `all-files-including-zips-compact/`.
- **Проход 9:** `source-examination-cache/llm_mvp_core-main` (снимок кода — **искать реальную реализацию
  SemanticCompilerPipeline.cs / StructuralKnnPredictor / E22DatasetRouter / E41IdleDetector / E46ConfidenceGate**,
  подтвердить, что node-00 и K-NN действительно есть в коде) + остальные `round-zips/` + `WHAT-TO-DO/*`,
  `SKILLS-BINDING-PROOF-PLAN`, `previos session.txt`, `.drawio.xml`/`.svg`/`.mmd`.
- **Проход 10:** completeness-sweep + executive summary.

### Честная строка покрытия (проход 6/10)

Прочитаны ЦЕЛИКОМ 4 документа: `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` (все 10 шагов
компилятора, 10 правил, влияние на 9 узлов, Mermaid-топология), `logical-structure-compiler-node-map-v1.csv`
(все 20 строк LSC-01…LSC-10 + DF-01…DF-10 с experiment_basis), `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md`
(round-contract, датасет 18, все 4 held-out трассы, оба challenge, gate-таблица, места кода, expected-evidence
JSON), `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` (8 станций, can/cannot, authority-order, реестр, привязка
21-23, Round-07). Извлечён механизм v2-слоя: node-00 компилятор (реальный SemanticCompiler-конвейер,
60-слотовый вектор), narrowing (4 локальных субъекта vs залоченные домены, baseline-too-broad), external-AI
advisory (арбитр/источник > внешний), knowledge-edge/learning-gap/idle-queue (отложенное обучение),
и явный маппинг на «external→internal DPO» с ЧЕСТНЫМ вердиктом DESIGN-полон / контур не подключён.
НЕ читались построчно (не в scope прохода 6): реальные тела `SemanticCompilerPipeline.cs` /
`StructuralKnnPredictor.cs` / `E41IdleDetector.cs` в `source-examination-cache` (осталось на проход 9 —
подтвердить, что node-00/K-NN/idle-код физически присутствует); генераторы корпуса (проход 7). Вердикт
прохода: v2-слой — это описанная петля само-обучения «внешний учит внутреннего через DPO»; node-00 компилятор
частично реален (детерминированный), но сам external→DPO-контур симулирован/не подключён (согласуется с
проходами 4-5 и RAG-05 `CODE-ABSENT`).


## Pass 7/10 — генераторы + all-documents-method-run (учёт 156K, подтверждение scaffold)

Этот проход закрывает «учёт без пропусков» для 99% папки: 156 129 файлов внутри
`all-documents-method-run/`. Проверено на уровне **кода генераторов** и на уровне
**данных** (сэмплы тел пакетов + строки JSONL). Всё только для чтения; изменён
лишь этот RAG-файл.

### 0. Точный учёт 156 129 файлов (сходится до штуки)

`find all-documents-method-run -type f` = **156 129**. Раскладка по структуре
(ничего не осталось «неучтённым»):

| Часть | Пакетов | Файлов на пакет | Итого файлов |
|---|---|---|---|
| `business-flows/` (курируемые бизнес-потоки) | 33 | 12 | 396 |
| `full-active-source-training-sets/` (уникальные активные документы) | 2 905 | 12 | 34 860 |
| `full-corpus-unique-training-sets/` (уникальный контент всего корпуса) | 10 069 | 12 | 120 828 |
| `all-files-including-zips-compact/` (compact-слой: 9 JSONL + манифесты/сводки) | — | — | 23 |
| корень `all-documents-method-run/` (оркестрация: CSV/JSON/mjs/ps1/md) | — | — | 22 |
| **Итого** | | | **156 129** |

Сумма: 396 + 34 860 + 120 828 + 23 + 22 = **156 129** — совпадает точно.
Итог: **156 084 файла** (99,97%) — это тела doc-пакетов (12-файловые package),
и только **45 файлов** — инфраструктура (генераторы, манифесты, сводки).

**12-файловый пакет** (одинаков для всех трёх наборов — подтверждено `ls` на
`doc-0001-infrastructure-catalogue` и на `01-user-registration…`):
`00-INDEX.md`, `01-UML-FRAME.md`, `node-01…node-09-*-set.md` (9 узловых файлов),
`10-SIMULATION-CHECK.md`.

### 1. Полный toolchain генераторов (кто что делает)

В корне `all-documents-method-run/` — 7 скриптов, из них 2 PowerShell-инвентаризатора
и 4 mjs-генератора пакетов + 1 mjs-классификатор:

| Скрипт | Тип | Что делает | Вход → выход |
|---|---|---|---|
| `build-all-files-and-zips-manifest.ps1` (176 стр.) | инвентаризатор | `Get-ChildItem -Recurse -Force` по всему `C:\Users\lubao\Documents\xiigen`, открывает **каждый .zip**, пишет по строке на файл/zip-entry. Классификация файла = `KindFor` (по расширению) + `CategoryFor` (regex по пути). `TextSample` = первая непустая строка первых 32 КБ. | дерево диска → `all-files-and-zip-entries-manifest.csv` (196 МБ) |
| `build-nested-zip-entries-manifest.ps1` (177 стр.) | инвентаризатор | Догоняет **вложенные zip-в-zip** (zip внутри zip внутри zip, глубина до 3). | манифест → `…-manifest-with-nested.csv` (342 МБ, +117 781 nested-entry) |
| `classify-subject-expertise-groups.mjs` (188 стр.) | классификатор | Читает nested-манифест построчно; каждую из 430 025 записей раскладывает по **19 предметным группам G01–G19** первым сработавшим regex (`session|history…`, `flow|dag…`, `auth|tenant…` и т.д.). | манифест → `subject-expertise-groups.csv`/`.json` |
| `generate-remaining-business-flow-packages.mjs` (1171 стр.) | генератор | **33 захардкоженных бизнес-спеки** (объекты с `title:` — ERP/SAP, Finance, Data warehouse/ClickUp, Amazon/AliExpress marketplace, Upwork/Freelancer и т.п.) прогоняются через тот же 12-файловый шаблон. Это **курируемый** набор. | 33 спеки → `business-flows/` (396 файлов) |
| `generate-full-active-source-packages.mjs` (731 стр.) | генератор | По `active-source-unique-documents.csv` (2 905 уникальных активных документов). Читает исходник, `classify`/`topicFor`/`domainsFor`/`headings` — детерминированная выжимка заголовков; строит 12 файлов из шаблонных функций. `PairCount:77` на пакет. Есть reuse-логика: если документ уже был в batch01 business-flow — пакет переиспользуется. | 2 905 док → `full-active-source-training-sets/` (34 860 файлов) |
| `generate-full-corpus-unique-packages.mjs` (~730 стр.) | генератор | **Тот же шаблон-семейство** (проверено: содержит `rephrase`/`scenarios`/`umlFrame`/`node01` на стр. 163/181/201/276), но по `corpus-files.csv` — 10 069 уникальных по SHA-контенту документов (дубликаты схлопнуты). | 10 069 док → `full-corpus-unique-training-sets/` (120 828 файлов) |
| `generate-all-files-compact-node-sets.mjs` (264 стр.) | генератор (compact) | Не пишет doc-пакеты, а стримит по all-files-манифесту (430 025 записей) сразу в **9 больших JSONL** — по одной node-записи каждого из 9 узлов на источник. | манифест → 9 × `node-0N-*-set.jsonl` |

Классификация «pipeline» из этих скриптов: **inventory (ps1)** → **classify/subject
(mjs)** → **pairs/rephrase/compact (mjs)**. Семантического анализа (LLM) нигде нет —
только `Get-ChildItem`, regex, извлечение заголовков и интерполяция строк-шаблонов.

### 2. Compact-слой: 430 025 источников → 3 870 225 node-records → 7 740 450 «пар»

Из `00-RUN-INDEX.md` и `run-summary.json` (сходятся):

- Физических файлов: **69 614**; zip-архивов открыто: **1 869**; top-level
  zip-entry: **242 630**; вложенных zip-контейнеров: **3 622**; nested zip-entry:
  **117 781**; глубина вложенности: **3**; ошибок открытия: 0 диск / 8 nested.
- **Всего source records: 430 025** (69 614 physical + 242 630 zip-entry + 117 781 nested-zip-entry).
- **9 UML-node JSONL**, по **9 node-records** на источник → **3 870 225** node-records.
- **18 compact-пар** на источник (у каждой node-записи original A/B **и** rephrased A/B)
  → **7 740 450** пар. Валидация: все 9 файлов `430025 / 430025 -> pass`.

Девять JSONL (по узлу конвейера, суммарно ~6,1 ГБ):
`node-01-domain-splitter` (587 МБ), `node-02-domain-translator` (540 МБ),
`node-03-scheme-planner` (788 МБ), `node-04-domain-processor` (649 МБ),
`node-05-applier` (610 МБ), `node-06-result-translator` (655 МБ),
`node-07-arbiter-selector` (488 МБ), `node-08-arbiter-units` (654 МБ),
`node-09-formatter` (729 МБ).

**Что лежит в каждой строке JSONL** (из `record()` в compact-генераторе): `sourceId`,
`sourceKind`, `relativePath`, `archiveRelativePath`, `zipEntryPath`, `category`,
`readableKind`, `node`, `pairName`, `A`, `B`, `rephrasedA`, `rephrasedB`, `labels`.
То есть zip-границы (archive path + entry path) сохраняются в каждой записи — «edge
rule» в индексе гарантирует, что у каждого источника есть все 9 узловых записей.

### 3. Предметный учёт корпуса — 19 групп (G01–G19)

`subject-expertise-groups.csv` раскладывает все 430 025 записей по темам первым
сработавшим regex. Ключевой факт для сводного плана: **корпус доминирован историей
сессий, а не свежим доменным контентом.**

| Группа | Записей | Доля | Примечание |
|---|---|---|---|
| G01 Session/History/Version | **280 727** | **65,3%** | Claude-code-session*.txt, STATE.json, worktree, .claude — «мусорная масса» истории |
| G07 Design/Figma/Visual/UI | 72 712 | 16,9% | сильно раздуто `.git\objects\*` бинарями |
| G02 Flow Planning/Orchestration | 29 936 | 7,0% | |
| G03 Architecture/Fabric/Skill | 25 256 | 5,9% | |
| G04 Learning/Teaching/Prompt/Model | 8 860 | 2,1% | |
| G06 Business Domains | 5 250 | 1,2% | |
| G09 Backend/API | 3 228 | | |
| G05 Retrieval/Graph | 2 337 | | |
| G08 Frontend/Mobile/Web | 843 | | |
| G12 Testing/Validation | 351 | | |
| G14 WordPress/Plugin/CMS | 262 | | |
| G18 Archive/Zip boundary | 117 | | |
| G19 General/Uncategorized | 71 | | .clinerules, AGENTS.md, CLAUDE.md |
| G10/G11/G13/G15 | 42/17/12/4 | | почти пусто |

Вывод по теме: только ~2% источников относятся к «Learning/Model» (G04) и ~1% к
бизнес-доменам (G06). Основная масса — это **побочный продукт истории разработки**
(сессии, git-объекты), пропущенный через тот же 9-узловой шаблон, а не курируемые
доменные знания. Инвентаризация честная и исчерпывающая, но обучающая ценность
массы низкая.

### 4. Пере-подтверждение scaffold на уровне ДАННЫХ (с цитатами)

Пасс-2 назвал массу «детерминированным scaffold». Проход 7 подтверждает это и в
коде, и в данных — двумя независимыми доказательствами.

**Доказательство A — код генераторов чисто шаблонный.** В
`generate-full-active-source-packages.mjs` все узловые функции (`node01…node09`,
`umlFrame`, `sim`, `indexFile`) — сборка строк через интерполяцию `${topic}`/`${d}`,
без единого вызова модели. «Перефразирование» — цепочка `.replace()`:
`rephrase()` меняет `Use→Apply`, `Explain→Show`, `Plan→Prepare`, `Find→Identify`,
`Review→Check`, `Turn→Translate`, `Create→Build`, `Compare→Contrast`. Сценарии — 10
фиксированных предложений-шаблонов. `domainsFor()` — словарь `baseByClass` (6
доменов на класс) + дописанные regex-заголовки.

**Доказательство B (данные, doc-пакет).** Цитата из
`full-active-source-training-sets/doc-0001-…/node-01-domain-splitter-set.md`:

> `A: "Use this document to understand Infrastructure Catalogue — XIIGen E2E Test Suite and keep its main responsibilities visible."`
> `B: ["test objective","scenario setup","expected behavior","failure evidence"]`
> `Pair 1.1 - Rephrased`
> `A: "Apply this document to understand Infrastructure Catalogue — XIIGen E2E Test Suite…"`

Здесь `B` — просто заголовки, вытащенные из исходного документа (`headings()`),
`topic` — первый заголовок, а «rephrased» отличается лишь заменой первого глагола
`Use→Apply`. В той же пачке **Pair 1.7** имеет `A` и rephrased-`A` **побайтово
одинаковыми** (ни один глагол не совпал с цепочкой `rephrase()`) — прямой признак
механического `.replace()`, а не смыслового парафраза.

**Доказательство C (данные, compact JSONL).** Первая строка
`all-files-including-zips-compact/node-01-domain-splitter-set.jsonl`:

> `{"sourceId":null,"sourceKind":"physical-file","relativePath":".clinerules",…,`
> `"A":"Use file .clinerules to understand .clinerules and keep its relevant responsibilities visible.",`
> `"B":["file identity","folder context","file type","source meaning"],`
> `"rephrasedA":"Use file .clinerules to understand .clinerules and keep its relevant responsibilities visible.",`
> `"rephrasedB":["file identity","folder context","file type","source meaning"]}`

Тут «topic» = само имя файла `.clinerules` (текст не читался — не в whitelist
расширений), `B` = фиксированный список из словаря `domains()` для класса «general
file», а `rephrasedA` **побайтово равна** `A`. Причина показательна: в compact-
генераторе цепочка `rephrase()` содержит и `Use→Apply`, и обратно `Apply→Use`, так
что строка на `"Use "` совершает круг и возвращается в `"Use "`. То есть
«перефразирование» дало **нулевую вариацию** — окончательное подтверждение, что это
не examination, а template scaffold. (Ранее `sim`/`node`-тексты пасса-2 подтверждали
то же на бизнес-пакетах.)

**Итог scaffold:** 156 084 файла тел пакетов и 7,74 млн compact-пар — это
детерминированный шаблонный «обвес» вокруг файлового инвентаря (path + заголовки +
regex-класс), а **не** семантический разбор содержимого. Это добросовестный
inventory-масштаб, но как обучающие данные он почти целиком синтетический boilerplate.
Настоящие/полу-настоящие данные — из ранних проходов: рукописные node-наборы
(FLOW-01, пасс-3) и частично реальный node-00 компилятор (v2-слой, пасс-6).

### 5. Что осталось для проходов 8–9

- **Проход 8** — оставшиеся не-generated файлы папки (вне `all-documents-method-run`):
  корневые дизайн-доки, ещё не разобранные детально (`DYNAMIC-FLOW-*`,
  `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT`, `SKILLS-BINDING-PROOF`, `WHAT-TO-DO-*`),
  `round-zips/`, `source-examination-cache/`, `flow-01-training-sets-v1/`,
  два zip (`dynamic flow docs.zip`, `latest dynami flow docs.zip`), `previos session.txt`.
- **Проход 9** — сшивка: как этот слой соотносится с extension-планом (VS / VS Code),
  что из «динамической модели» реально пригодно для продолжения расширений vs что
  остаётся scaffold; финальная честная строка покрытия по всей папке.

### Мета

RAG-файл: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`
Проход 7/10 — только чтение генераторов и данных + append этой секции. Предыдущие
секции не тронуты.

---

## Pass 8/10 — код-snapshot (что реально в коде) + остаток не-генерированных файлов

Проход 8/10. Единственный агент (Opus 4.8), только чтение + append этой секции.
Цель: приземлить «динамическую модель» к фактическому коду — что физически лежит
кодом в snapshot-копии репозитория, а что остаётся дизайном/симуляцией; и закрыть
no-neglect по оставшимся не-генерированным файлам папки (вне `all-documents-method-run/`).

### 8.1 Что это за snapshot

`source-examination-cache/llm_mvp_core-main/` — это снимок-копия репозитория
`llm_mvp_core` (внутри двойная вложенность: `llm_mvp_core-main/llm_mvp_core-main/`).
Всего 561 файл, из них 401 `.cs`. Это НЕ живой репозиторий, а точечная копия,
которую динамический флоу использовал как корпус для examination-метода (плюс
рядом `docs/`, `session-package/` со скиллами и `sessions/` с 11 транскриптами
сессий — контекст, не имплементация динамического флоу).

Признаки, что snapshot старее/уже текущего репо:
- в snapshot ЕСТЬ дубли-номера проектов: `12.SearchEngine` и `12.SemanticCompiler`;
  `14.ContinuousLearning` и `14.MachineLearning`; две копии компилятора —
  `12.SemanticCompiler` (18 `.cs`) и `29.SemanticCompiler` (17 `.cs`), причём их
  `LogicExtractor.cs` РАЗЛИЧАЮТСЯ (diff -> DIFFERENT), т.е. это параллельные версии;
- в snapshot НЕТ проектов, которые есть в живом репо по CLAUDE.md:
  `42.ProjectWikiDomain`, `25.ModelServer`, `44.DesignConversionDomain`;
- архплан упоминает базлайн «877 tests», а живой CLAUDE.md — «>=1488».
Вывод: snapshot — это evidence-корпус на момент ~2026-05, а не текущий исходник.

### 8.2 Реальный код (file:line) — «строительные блоки» есть

1. node-00 компилятор — реальный, но wired 6 из 8 стадий.
   `29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs` (84 строки).
   `Compile()` (стр. 50–83) реально прогоняет 6 стадий: (1) `AstTokenizer` стр. 56,
   (2) `StackDetector` на СЫРОМ тексте стр. 61 (D-07: до нормализации),
   (3) `RouteClassifier` стр. 65, (4) `SemanticNormalizer` стр. 68,
   (5) `FamilyCollapser` стр. 71, (6) `LogicExtractor` стр. 74; затем compression =
   words/predicates (стр. 77–79). Заголовок (стр. 11–12) называет ещё две
   «опциональные» стадии — `Decomposer (E21)` и `TemplateBeautifier (E23)` — но они
   НЕ инжектятся и НЕ вызываются в `Compile()`. То есть «8/10 steps» из пасса-6 =
   6 стадий реально исполняются, 2 названы, но не подключены. Файлы
   `Planning/Decomposer.cs`, `Beautification/TemplateBeautifier.cs` в проекте лежат.

2. 22-предикатный LogicExtractor — реальный, но детерминированный (R4-релевантно).
   `29.SemanticCompiler/Compression/LogicExtractor.cs` (88 строк). `const int
   VocabSize = 22` (стр. 11); `PredicateKeys` из 22 ключей (стр. 18–42:
   NULL_CHECK…AGGREGATE_OP); `TriggerMap` — хардкод substring-триггеров (стр. 45–69);
   `Extract()` (стр. 73–87) — substring-матч без обучения, fallback `CALL_OP`. Это
   regex/keyword-класс (детерминированная свёртка), НЕ обученная единица — тот самый
   «hidden-determinism», который дизайн (D-14/D-15) обещает заменить обучаемыми юнитами.

3. K-NN — реальный brute-force. `35.ExperimentsV2/Core/StructuralKnnPredictor.cs`
   (50 строк): `Train()` (стр. 14–18); `Predict()` — O(n) расстояние, топ-K, мода
   (стр. 21–36); `AverageNearestNeighborDistance()` (стр. 41–49). Рядом реальные
   `MethodKnnPredictor.cs`, `FileActionKnnPredictor.cs`,
   `22.AgentAlgorithms/Context/KnnSkillFilter.cs`.

4. Структурный вектор для K-NN — реальный, на Roslyn.
   `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs` (98 строк): вектор
   AST-узла через 3 уровня предков + позиция/сиблинги/глубина/scope + `SyntacticRole`
   (стр. 12–66, `Microsoft.CodeAnalysis.CSharp`); `record struct StructuralVector` с
   взвешенным `DistanceTo` (стр. 89–97: SyntacticRole вес 4.0, Parent1 3.0…).
   Реальный feature-engineering, но детерминированный (не нейросеть).

5. 16.AutonomousLearning IdleDetector — реальный (E41).
   `16.AutonomousLearning/IdleDetector/IdleDetector.cs` (63 строки): 4 условия —
   CPU<20% (стр. 44), input>10 мин (стр. 48), батарея>=50% или зарядка (стр. 53),
   диск>=2 ГБ (стр. 57); все 4 -> `SafeToTrain` (стр. 61). Рядом реальные
   `AutonomousTrainer.cs`, `ReplayBuffer.cs`, `TrustScorer.cs`, `DatasetWeighter.cs`,
   `AutonomousLearningPipeline.cs` (9 `.cs`). `LearningEventScheduler` (имя из
   canonical-sequence) в проекте 16 НЕ найден — см. 8.3.

6. DPO-тренер — реальный from-scratch. `11.DPO/DpoTrainer.cs`: настоящая
   DPO-математика — `LlmModel` policy/reference, `AdamW`, `Beta=0.1`, лог-вероятности
   chosen/rejected, `margin = chosenReward - rejectedReward`, `-log sigma(margin)`,
   checkpointDir (стр. 1–45+). Плюс `DpoDataset.cs`, `Models/{DpoBatch,DpoMetrics,
   DpoPair}.cs`. `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs` — реальная ручная сеть
   (Embedding->Linear->ReLU->Linear, CrossEntropy, вручную).

Масштаб реального кода по проектам (число `.cs`): 35.ExperimentsV2 = 106 (E01–E50 +
Core/Infrastructure), 24.Compression 25, 22.AgentAlgorithms 25, 33.MatrixAlgorithms 24,
23.OssEquivalents 20, 34.Experiments 19, 12.SemanticCompiler 18, 29.SemanticCompiler 17,
21.AgentInfrastructure 14, 15.OrchestratorModel 12, 14.MachineLearning 11, 28.SemanticNlp
10, 13.CodeOptimizer 10, 00.TensorCore 10, 16.AutonomousLearning 9, 11.DPO 5 … То есть
фундамент («блоки Лего») реально закодирован.

### 8.3 Что DESIGN-ONLY / физически ОТСУТСТВУЕТ в snapshot

Проверено grep-ом по `--include="*.cs"` во всём snapshot — 0 совпадений:

- Станции E51/E52/E53 — не в коде. `35.ExperimentsV2` идёт ровно E01…E50
  (E01AstTokenizer…E50DomainGraphClosure). `E51/E52/E53` в `.cs`-контенте — 0.
  Подтверждает пассы 4/5: E51/E52 симулированы, E53 детерминирован — они в
  дизайн-доках и round-zips, не в коде snapshot.
- 5 названных рантайм-компонентов E51/E52 для 21-23 — не в коде. Canonical-sequence
  называет «первый срез 21-23» как 5 частей: `InternalLlmGateway`,
  `InternalResultExaminer`, `ExternalAiRequestPackage`, `ExternalAiResultAdapter`,
  `LearningEventScheduler` (+ `KnowledgeEdge`). Grep по snapshot — 0 совпадений.
  Дизайн/симуляция, не подключённый код.
- Собственные узлы флоу — не в коде. `DomainSplitter`, `SchemePlanner`,
  `DomainTranslator`, `DynamicFlow`, `node-00..03` в `.cs` — 0 совпадений.
- Проекты 21-23 в snapshot = обычная агент-инфраструктура, НЕ рантайм флоу.
  `21.AgentInfrastructure`: `AgentLoop`, `AgentState`, `ToolRegistry`,
  `PermissionGate`, `CodebaseIndexer`, `ContextAssembler`, `SessionStateStore`.
  `22.AgentAlgorithms`: `ReactAgentLoop`, `AgentOrchestrator`, `LlmJudge`,
  `RouteClassifier`, декомпозеры. `23.OssEquivalents`: MCP (`McpToolDispatcher/
  Transport/Definition`), Roslyn (`RoslynCodeAnalyzer`, `CallGraphBuilder`,
  `RoslynSyntaxValidator`), `AgentStateMachine`, `StructuredLlmClient`. Фундамент под
  агента/расширения, но БЕЗ приделанного E51/E52 knowledge-edge/arbiter-рантайма.

Итог реальности: в snapshot реально закодированы блоки (компилятор-свёртка, K-NN,
структурные векторы на Roslyn, DPO-тренер, IdleDetector, агент-инфра, MCP,
эксперименты E01–E50). Собственно «динамическая модель» — редактируемая топология
узлов, 14 обучаемых ролей, арбитры, knowledge-edge, 5 рантайм-компонентов 21-23,
станции E51/E52/E53 — существует как ДИЗАЙН + симуляция/оракул, а не как подключённый
рантайм в этом snapshot.

### 8.4 Остаток не-генерированных файлов (вне all-documents-method-run/)

A. Корневой мастер-дизайн (design-only).
- `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` (29 КБ) — мастер. Статус прямым
  текстом: «No implementation has started under this plan». 7 принципов: (1) всё
  обучается с 1-го дня, (2) топология = данные в БД, редактируется на уровне ребра,
  (3) само-подобные юниты (единый predicate-in/out контракт -> hot-swap/marketplace),
  (4) версионирование/rollback по построению (append-only), (5) только внутренний ИИ,
  (6) сквозной provenance, (7) тесты -> contract/invariant/replay. Цаст ~14 ролей:
  domain splitter, domain translator (= 5-уровневый SemanticCompiler как юнит),
  decomposer, orchestrator, applier (per-domain), aggregator, result translator
  (обратный путь «неполный в коде»), formatter, style adapter, arbiter pool, arbiter
  selector, swappability judge, gap detector, dataset curator.
- `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` (20 КБ) — «сужающий слой»:
  `intake gate -> system topology router -> sub-subject routers -> local domain
  splitters -> существующий флоу`; компилятор логической структуры как pre-stage;
  budget/confidence-gate; типизированные пути возврата арбитра по источнику ошибки.
- `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` (12 КБ) — один канонический
  E2E-порядок. Заявляет валидацию: E51B «6/6 traces passed», CH-01 сделал
  knowledge-edge context pack вместо выдумки, CH-02 удержал власть арбитра;
  «первый срез 21-23» = 5 рантайм-компонентов (их нет в коде, см. 8.3); E52
  before/after: 0% -> 100% contract success по V06/CH-01/CH-02/IDLE-01 (симуляция).
- `DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md` (1.8 КБ) + `DECISIONS-LOCKED.md`
  (D-01…D-15). Ключевое: D-04 = 5 доменов CODE_Q/BUG/PLAN/ARCH/REFACTOR; D-11 нет
  внешних ML-фреймворков (TensorCore); D-13/D-14/D-15 — разделение власти: priority
  что пробовать, KNN = память по истории арбитров, АРБИТР решает что принято, внешний
  ИИ — только совет (single-use approval, идл-обучение, адаптер ставится только если
  after-accuracy > before-accuracy). Ни один сигнал не перебивает отказ арбитра.
- `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` (8 КБ) — двухуровнево: сигналы арбитра
  улучшают KNN-ретрив ДО генерации; вердикты арбитра — жёсткий гейт ПОСЛЕ.
- `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` (15 КБ) — контролируемый escape:
  `PermissionGate` single-use `call_external_ai`; `ExternalAiRequestPackage`;
  `ExternalAiResultAdapter` через таблицу правил арбитра (`identity_consistency`,
  `token_safety`); Round 07 hardening.
- `SKILLS-BINDING-PROOF-PLAN-v1.md` (31 КБ) — «фабрика скиллов как оркестрационная
  подсистема» на кластере design-for-ai (5 скиллов); тест-кейс — конфликт бюджета
  инъекции max-3.
- `WHAT-TO-DO-WHAT-NOT-TO-DO-v1.md` / `v2.md` (24/27 КБ) — правила поведения сессии
  из session-файлов; v2 — корректировки после раунда 21-23/E52. ВНИМАНИЕ: мохибейк
  (UTF-8, прочитанный как CP1252), это artefact-текст.

B. Семейство E51 (планы, design-only): `E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md`,
`E51-END-TO-END-PIPELINE-PLAN-v1.md` (35 КБ), `E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md`.

C. Семейство E53 (детерминированный orchestration-proof): `E53-EXECUTOR-ROUND-CONTRACT`,
`E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN` (25 КБ), `E53-HUMAN-RESULT-CHECKLIST`,
`E53-LEARNING-SET-TRANSLATION-MATRIX`, `E53-ORCHESTRATION-EVIDENCE-SCHEMA`,
`E53-ROUND-A/B/C-VALIDATION-RESULTS`, `E53-SCENARIO-001-EXPECTED-TRACE` (42 КБ),
`E53-SCENARIO-001-SOURCE-PACKET`. -> E53 = детерминированное доказательство прохода по
цепочке узлов без потери рёбер (round-C — «trained splitter»).

D. Прочие корневые доки/диаграммы: `EXAMINATION-METHOD-FINAL.md` (68 КБ, метод — пасс 2),
`document-examination-method-v2.md`, `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md`
(29 КБ), `DOCUMENT-ADAPTATION-ANALYSIS-v1.md`, `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md`,
`PROJECTS-21-23-TESTING-GUIDANCE-v1.md` (25 КБ), `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md`
(24 КБ); диаграммы: `dynamic-flow-high-level-topology-v2.mmd`,
`dynamic-flow-v2-node-topology.csv`, `logical-structure-compiler-node-map-v1.csv`,
`improve algo.drawio.xml`, `llm_pipeline_corrected.svg`.

E. `FLOW-01-TRAINING-DATASET-PLAN-v3.md` (26 КБ) / `v4.md` (252 КБ) +
`flow-01-domain-splitter-training-full.md` (37 КБ) — планы/полный набор данных
splitter-а (v4 — самый крупный текстовый файл папки).

F. `flow-01-training-sets-v1/` — рукописные тренировочные наборы (реальные, но крошечные).
9 наборов по узлам + `00-INDEX.md` + `10-SIMULATION-CHECK.md`(.zip). Формат — ручные
A/B-строки на каждую строку исходного документа (пример: `ROW DOMAIN_SPLITTER-001`,
Source `01-user-registration.md:L001`, «A received»/«B this node must produce»). Строки:
node-01 splitter 145, node-02 translator 811, node-03 scheme-planner 1270, node-04
processor 199, node-05 applier 226, node-06 result-translator 199, node-07
arbiter-selector 379, node-08 arbiter-units 66, node-09 formatter 901. Это v1 семейства
FLOW-01 (пасс-3 разбирал flow-01 «tiny real data») — человеко-написанные, НЕ scaffold,
но малого объёма. Мохибейк BOM в заголовках присутствует.

G. `round-zips/` — 15 раундов 2026-05-04 (упаковка-артефакты сессий). Каждому
соответствует `ROUND-MANIFEST-*.md` в корне: r01 projects-21-23-e52 · r02
document-adaptation · r03 what-to-do-v2 · r04 first-run-gap-closure · r05
topology-visual-update · r06 rollback-gate · r07 arbiter-constraint-adapter
(+corrected) · r08 node-00 training-pairs · r09 node-01 domain-splitter · r10 node-02
domain-translator · r11 node-03 scheme-planner · r12 E53 high-level-orchestration-proof-plan
· r13 E53 round-A doc+oracle · r14 E53 round-B deterministic-proof · r15 E53 round-C
trained-splitter. (r08 и r09 также распакованы в одноимённые папки.) Раунды 08-11 =
«thread-3» узлы node-00..03; 12-15 = E53. Упакованная история раундов, evidence-only.

H. Zip-бандлы дизайна: `dynamic flow docs.zip` (501 КБ), `latest dynami flow docs.zip`
(220 КБ) — заархивированные пачки тех же дизайн-доков (round-zip packaging).
`previos session.txt` (33 КБ) — транскрипт прошлой Claude web-сессии, загружавшей
`llm-session-package.zip` + `llm-from-scratch-skills`; это лог/контекст, evidence-only.

### 8.5 Таблица реальности (real code vs design-only)

| Компонент динамического флоу | В snapshot | Статус |
|---|---|---|
| node-00 SemanticCompilerPipeline | `29.SemanticCompiler/Pipeline/*.cs:50-83` | РЕАЛЬНЫЙ, wired 6/8 стадий |
| 22-предикатный LogicExtractor | `29.SemanticCompiler/Compression/*.cs:11-87` | РЕАЛЬНЫЙ, детерминированный |
| StructuralKnnPredictor / K-NN | `35.ExperimentsV2/Core/*.cs:6-49` | РЕАЛЬНЫЙ brute-force |
| StructuralVectorExtractor (Roslyn) | `35.ExperimentsV2/Infrastructure/*.cs:9-98` | РЕАЛЬНЫЙ, детерминированный |
| IdleDetector (E41) | `16.AutonomousLearning/IdleDetector/*.cs:20-62` | РЕАЛЬНЫЙ |
| DpoTrainer / ProbeNeuralNetwork | `11.DPO/DpoTrainer.cs`, `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs` | РЕАЛЬНЫЕ |
| Эксперименты E01–E50 | `35.ExperimentsV2/Experiments/*` | РЕАЛЬНЫЕ харнессы |
| Станции E51/E52/E53 | — (0 в `.cs`) | DESIGN/симуляция/оракул |
| 5 рантайм-компонентов 21-23 (InternalLlmGateway…LearningEventScheduler) | — (0 в `.cs`) | DESIGN-ONLY |
| Узлы флоу (DomainSplitter/SchemePlanner/DynamicFlow/node-00..03 как классы) | — (0 в `.cs`) | DESIGN-ONLY |
| Топология-граф, 14 ролей, arbiter pool, knowledge-edge | — | DESIGN-ONLY |
| flow-01 наборы (node-01..09) | `flow-01-training-sets-v1/` (в корне папки) | РУКОПИСНЫЕ, крошечные |

### 8.6 Что осталось для проходов 9–10

- Проход 9 — сшивка со стороной расширений (VS / VS Code): что из snapshot реально
  пригодно как основа продолжения расширений (агент-инфра 21-23, MCP в 23,
  Roslyn-анализ, DPO-тренер, K-NN/компилятор-свёртка) vs что остаётся дизайном (весь
  рантайм динамического флоу). Расширению нужен тонкий host-адаптер (R6), ядро — из
  `llm_mvp_core`; но «динамическая модель» пока дизайн, значит план опирается на
  реальные блоки + честный ярлык «flow-рантайм не подключён».
- Проход 10 — финальная честная строка покрытия по всей папке (генерированный scaffold
  + реальные блоки + дизайн/симуляция), единый вердикт «real vs scaffold vs design».

### Мета

RAG-файл: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`
Проход 8/10 — только чтение snapshot-кода и остатка не-генерированных файлов + append
этой секции. Предыдущие секции (пассы 1–7) не тронуты.

---

## Pass 9/10 — синтез: архитектура vs реальность, реальные блоки, cross-folder, открытые вопросы

Проход 9/10. Единственный агент (Claude Opus 4.8, Ultracode), только чтение + append этой
секции. Это **синтез-проход**: он не открывает новых поддеревьев, а сводит находки проходов 1–8
(эта папка) и RAG-01…05 (соседние папки) в **одну действенную картину** для плана «продолжить
расширения Visual Studio и VS Code». Ничего из проходов 1–8 не переписано; ниже — только связывание
уже доказанного. Где утверждение опирается на находку прошлого прохода — дана ссылка на проход и
file:line, установленный там.

### 9.1 Синтез папки-6 — что эта папка даёт плану «продолжить расширения»

Одним предложением: **эта папка — полное, зрелое ОПИСАНИЕ «динамической LLM-модели» (редактируемый
граф маленьких обученных юнитов) плюс богатая детерминированная СИМУЛЯЦИЯ того, что она работает, —
но сам обучающий/рантайм-контур ещё НЕ подключён к коду, а 99% корпуса — это механический scaffold,
а не обучающие данные.** Дизайн — actionable; «обученная динамическая модель» — пока нет.

Разложение по шести осям (a–f), которые нужны сводному плану:

**(a) Архитектура (что задумано).** Не один большой LLM, а **самомодифицирующаяся сеть маленьких
специализированных внутренних LLM-юнитов поверх редактируемого графа-потока** (проход 1, §1
`DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`):
- **Граф = данные, не код** (принцип 2): топология лежит в своей БД, оркестратор читает её в
  рантайме; добавить юнит = зарегистрировать cell + ребро; заменить = двинуть указатель; откатить =
  восстановить версию топологии. Правки на уровне рёбер (вставить arbiter-selector между стадиями).
- **v1 = 9 узлов** (splitter→translator→scheme-planner→processor→applier→result-translator→
  arbiter-selector→arbiter-units→formatter) — **физически прошиты** в ~13 000 сгенерированных пакетах
  `node-01…node-09` (проход 1, подтверждает RAG-02). **v2 = 30 узлов N00–N29**
  (`dynamic-flow-v2-node-topology.csv`): тот же 9-узловой центр + слой сужения впереди (N00 компилятор
  → intake → topology-router по 14 областям → sub-subject → evidence-pack → local-splitter) + слой
  обучения/внешней помощи сзади (event-trail → KNN → knowledge-edge → external-AI-advisory →
  learning-gap → idle-queue → dataset-curator → confidence-updater).
- **Разделение власти (D-13/14/15, проход 1 §4 + проход 8):** **Priority** решает *что пробовать*;
  **KNN = память** из истории арбитров; **Arbiter = что принято** (hard-gate); **External-AI =
  advisory**, никогда не authority; никакой priority/KNN/budget/style/external не перекрывает
  rejection арбитра. Всё внутреннее (Internal-AI throughout); обучение — только в idle под IdleDetector.
- Встроенные версионирование/rollback (принцип 4, три раздельных хранилища: schema/state/trail) +
  сквозной provenance (принцип 6) — **это и есть техническая основа examine-UI**, см. (f).

**(b) Реальность (что реально есть).** Проходы 2–8 дали строгий, привязанный к коду вердикт:
- **Дизайн — DESIGN-COMPLETE и богатый** (7 принципов, 30-узловая топология, авторитет, external-AI
  advisory, idle-DPO — всё описано конкретно).
- **Рантайм-контур динамического потока НЕ подключён.** Grep по snapshot-коду (проход 8.3): станции
  E51/E52/E53, 5 рантайм-компонентов 21-23 (`InternalLlmGateway`, `InternalResultExaminer`,
  `ExternalAiRequestPackage`, `ExternalAiResultAdapter`, `LearningEventScheduler`), собственные узлы
  флоу (`DomainSplitter`/`SchemePlanner`/`DynamicFlow`/node-00..03 как классы) — **0 совпадений в
  `.cs`**. Это дизайн + симуляция/оракул, не подключённый код.
- **156 971-файловый корпус = детерминированный scaffold** (проходы 2 и 7, доказано кодом
  генераторов): `classify()` по regex над путём, `domains()` = фиксированный map класс→ярлыки,
  `rephrase()` = подмена первого слова (часто `rephrasedA == A` побайтово). ~65% корпуса — вообще
  история сессий/git-объекты (G01), а не доменные знания. Как обучающие данные — синтетический
  boilerplate.
- **Настоящих обучаемых данных динамического потока — крошечно** (проход 3): один правильный
  splitter-набор `flow-01-domain-splitter-training-full.md` (~10 runtime-пар A→B), 7 арбитр-негативов
  node-08, 12 teaching-записей E51B — всё на ОДНОМ документе (user-registration). 7 из 9 рукописных
  node-наборов — в старом провальном формате «B описывает выход».
- **«Обученные» юниты E51/E52/E53 — детерминированы, не обучены** (проходы 4–5): «enhanced» = K-NN по
  ручной разметке + keyword-классификатор + substring-examiner + захардкоженное duplicate-правило +
  очередь; «0%→100%» = 3/4 baseline захардкожены `false`; round-15 «trained splitter» =
  правило-фильтр по 11 захардкоженным правилам, **0 весов/градиента/checkpoint** (grep). Это класс
  `hidden determinism / masquerade` (R4). Ценность реальная — но это **contract-proof формы**, не
  learned-proof.

**(c) Реальные переиспользуемые блоки («Лего», которые ЕСТЬ в коде — проход 8.2/8.5).** Это самое
ценное для плана: фундамент реально закодирован и оттестирован, даже если рантайм флоу поверх него —
дизайн.

| Блок | Где (в snapshot; путь в живом репо аналогичен) | Статус | Роль в дин. потоке |
|---|---|---|---|
| **node-00 SemanticCompilerPipeline** | `29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs:50-83` | РЕАЛЬНЫЙ, wired 6/8 стадий, детерминированный | сырой промпт → стабильная логическая структура; **самая сильная реальная опора** |
| **22-предикатный LogicExtractor** | `29.SemanticCompiler/Compression/LogicExtractor.cs:11-87` | РЕАЛЬНЫЙ, детерминированный (substring-триггеры) | «маленький внутренний язык» = вход для 60-слотового вектора |
| **StructuralKnnPredictor (K-NN)** | `35.ExperimentsV2/Core/StructuralKnnPredictor.cs:6-49` | РЕАЛЬНЫЙ brute-force | «память» (D-15): local-subject routing, positive/negative соседи |
| **StructuralVectorExtractor (Roslyn)** | `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs:9-98` | РЕАЛЬНЫЙ, детерминированный | feature-инжиниринг AST-узлов для K-NN |
| **IdleDetector (E41)** | `16.AutonomousLearning/IdleDetector/IdleDetector.cs:20-62` | РЕАЛЬНЫЙ (4 условия → SafeToTrain) | триггер отложенного (idle) обучения — **сердце «обучение не на hot-path»** |
| **DpoTrainer (from-scratch)** | `11.DPO/DpoTrainer.cs` (+ `DpoDataset`, `DpoBatch/Metrics/Pair`) | РЕАЛЬНЫЙ: AdamW, Beta=0.1, `margin=chosen−rejected`, `-log σ(margin)`, checkpointDir | **Route B тренер = центр обучения всей системы** (общий с RAG-05) |
| **ProbeNeuralNetwork** | `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs` | РЕАЛЬНАЯ ручная сеть (Embedding→Linear→ReLU→Linear, CrossEntropy) | доказывает, что from-scratch обучаемый юнит возможен |
| **Агент-инфра 21-23 + MCP + Roslyn** | `21.AgentInfrastructure` (AgentLoop/ToolRegistry/PermissionGate/CodebaseIndexer), `22.AgentAlgorithms` (ReactAgentLoop/LlmJudge/RouteClassifier), `23.OssEquivalents` (`McpToolDispatcher/Transport`, `RoslynCodeAnalyzer/CallGraphBuilder`) | РЕАЛЬНЫЕ | **готовый фундамент под host-адаптер расширения** (R6: тонкий агент, ядро в core) |
| **Эксперименты E01–E50** | `35.ExperimentsV2/Experiments/*` (106 `.cs`) | РЕАЛЬНЫЕ харнессы | провенанс каждого шага компилятора (E15/E16/E17/E20/E23/E25-27) |

**(d) Кратчайший путь от дизайна к реальному динамическому потоку** (синтез — минимальный безопасный
спайн, не полный v2). Порядок выведен из зависимостей блоков (c) и честного статуса (b):
1. **Опереться на node-00 (реальный SemanticCompiler) как единственную живую точку входа** — он уже
   даёт стабильную логическую структуру + 60-слотовый вектор (проход 6). Ничего дообучать не надо для
   старта.
2. **Материализовать ОДИН реальный обучаемый узел end-to-end** через уже существующий `11.DPO/
   DpoTrainer` + `IdleDetector`, а не 14 ролей сразу. Кандидат № 1 — **domain-splitter**: единственный
   узел, у которого есть корректный runtime-набор A→B (`flow-01-domain-splitter-training-full.md`,
   ~10 пар). Обучить его как `ILogicalUnit` с trained state + checkpoint + fresh-load — это превращает
   round-15 «trained splitter» (правило-фильтр) в НАСТОЯЩИЙ обученный юнит и закрывает masquerade.
3. **Подключить 5 рантайм-компонентов 21-23 как тонкую обвязку** (`InternalLlmGateway` →
   `InternalResultExaminer` → `PermissionGate`/`ExternalAiRequestPackage`/`ExternalAiResultAdapter` →
   `LearningEventScheduler`) — они СПРОЕКТИРОВАНЫ (external-AI amendment, проекты 21-23) и имеют
   arbiter-table (Round-07), но их нет в коде; это самый прямой мост «дизайн → рантайм».
4. **Топология-в-БД + оркестратор-читает-в-рантайме** (принцип 2) — предложенные сабпроекты
   `18.TopologyRuntime` + `19.UnitRegistry` (Open Item O-1). Это шаг, после которого граф становится
   редактируемым «данными». Он больше остальных, поэтому идёт последним в минимальном спайне.
5. **Заменять фикстуры обученными юнитами по одному** (plan Round D: начать с node-00), каждый — с
   install-if-better + rollback (D-14: after-accuracy > before-accuracy). Это делает E53-оракул
   постепенно «настоящим».

**(e) Петля «внешняя модель обучает внутреннюю через DPO» (шаг 6 целевого флоу)** — главный вывод
прохода 6, синтезированный: v2-слой **буквально описывает** «external teaches internal via DPO» как
*отложенный* контур (не hot-path):
- Сигнал учителя берётся **только на named knowledge-edge** (детектор обязан назвать недостающую
  компетенцию; external-AI по D-14 single-use approval).
- Внешний ответ становится DPO-материалом через **Learning Gap Packet**, который уже несёт готовую
  разметку: `positive` (принятый результат = *chosen*), `negative` (упавшие локальные попытки /
  реджекнутый внешний ответ = *rejected*), `warning`, + адрес «какая роль-юнит + домен учится»
  (dataset-grid = **unit × domain**).
- Тренировка — **в idle, по одному узлу**, group-by role+domain → conformance-suite → install только
  если after > before → rollback сохранён. Это ровно `11.DPO/DpoTrainer`-петля RAG-05, но с внешним
  ИИ как источником сигнала предпочтения.
- **Внутренняя модель остаётся авторитетом** (арбитры > external, source-authority > external, внешний
  ответ не пишет в финал) — это встроенная защита DPO от reward-hacking внешним ИИ (совпадает с
  R4/Acceptance-signal-independence из CLAUDE.md).
- ⚠️ **Честный статус:** контур СПРОЕКТИРОВАН полно и конкретно, но **не подключён** — mini-E2E
  использует *симулированный* внешний ответ и НЕ переобучает (idle-replay + install-gate помечены как
  FUTURE-раунд). Это ровно дыра RAG-05 `DESIGN-COMPLETE, CODE-ABSENT`.

**(f) O-4: дом examine-UI = Visual Studio extension.** `DYNAMIC-FLOW-ARCHITECTURE-PLAN` §Open Items
**O-4** прямым текстом: «Where does the UI live? My default assumption is the Visual Studio extension
(subproject 13)». Это прямой мост к цели сводного плана: **examine-аффорданс** (проследить любой ответ
назад через все юниты/рёбра/версии/knowledge-edges/learning-пакеты) реализуем **потому что principle-6
provenance-trail пишет каждый шаг**. Для расширения это значит: даже пока флоу-рантайм — дизайн,
**E53-трасса + knowledge-edge/learning-gap-контракты уже готовы как оракул/контракт для UI** (показать
пользователю: маршрут через узлы, вердикты арбитров, «край знания», context-pack, что ушло в
idle-обучение). Это самый дешёвый первый видимый продукт расширения над этой папкой.

### 9.2 Cross-folder мосты (что папка-6 берёт у соседей / даёт соседям)

**↔ RAG-05 (dpo legal build) — ЦЕНТРАЛЬНАЯ пара, взаимная.** Это главный мост всего свода:
- **Папка-6 = ИСТОЧНИК дизайна; RAG-05 = его ФОРМАЛИЗАЦИЯ в контракты.** RAG-05 берёт «editable flow
  graph + registries + arbiters + provenance» из ЭТОЙ папки и оформляет в
  **`ILearningDynamicGraph ≡ ILearningMachine<DynamicGraphDomain>` (Level 5)** с типами
  `IDynamicGraphNode` (L2, block P-01), `IDynamicGraphEdge`+per-edge authority (L3, P-02),
  оркестратор L4 (P-03), `GraphDpoPair`/auto-DPO (P-07/P-09), **легальный teacher-гейт
  `TeacherProvenance` 18–20 полей** (P-08). Заблокированные D-13/14/15 из ЭТОЙ папки → это ровно
  `EdgeAuthorityUnit`/GraphDPO в RAG-05.
- **Статусы совпадают:** RAG-05 называет `ILearningDynamicGraph` **DESIGN-COMPLETE, CODE-ABSENT** —
  это тот же вердикт, что проходы 2–8 здесь (дизайн богат, рантайм/данные отсутствуют).
- **Что папка-6 ДАЁТ RAG-05:** человекочитаемое «зачем» (7 принципов, 29-стадийный поток, external-AI
  8 станций, knowledge-edge/learning-gap/idle механика) + реальные блоки (node-00, K-NN, IdleDetector,
  DpoTrainer) + оракул E53. **Что папка-6 БЕРЁТ у RAG-05:** строгие ILearningMachine-контракты,
  execution-control-board (12 фаз), requirements-matrix R1..R21, легальный license-gated teacher-гейт
  (чего в дизайне ЭТОЙ папки нет в виде контракта — только принцип 5 «контент извне, суждения нет»).
- **Общий центр — `11.DPO/DpoTrainer` (Route B: `11.DPO` + `18.CapabilityTraining`).** Он реален в
  обеих папках; починка этого центра чинит и dynamic-flow idle-DPO, и translate, и wiki (все три —
  домены поверх одного тренера).

**↔ RAG-02 (orchestrator extension) — 9-узловой поток + host-entrypoint.** RAG-02 нашёл поток
`Domain Splitter → Translator → SchemePlanner → Processor → Applier → ResultTranslator →
ArbiterSelector → Arbiter → Formatter` — **папка-6 подтверждает его физически** (каждый из ~13 000
пакетов = `node-01…node-09`). Что папка-6 БЕРЁТ у RAG-02: **реальный host-entrypoint контракт** —
бинарь **`wiki-context-hook.exe`** (`42.WikiHookRunner`), который VS Code `.vsix` и Visual Studio
`.vsix` запускают на каждый промпт (`WikiOrchestratorFactory.Create → orchestrator.QueryAsync`). Это
и есть исполняемая точка входа расширения в ядро — то, через что examine-UI (O-4) и весь флоу реально
дёргается из хоста. Что папка-6 ДАЁТ RAG-02: 30-узловую топологию и external-AI/idle-DPO слой, которых
у wiki-контура нет (у RAG-02 своя проблема — `xiigen-small-doc-wiki-base` НЕ принят: acc 0.286,
halluc 0.68; та же болезнь «тренер недостаточен + обучаемый flow отсутствует»).

**↔ RAG-01/03/04 — динамический поток ОБОРАЧИВАЕТ эти домены.** 9/30-узловой граф — это универсальный
оркестрационный каркас; конкретные способности приходят как домены-юниты внутри него:
- **RAG-03 (language-translate):** «domain-translator» узел (node-02) = translation-способность.
  Активный `27.TranslationDomain` — реальный `ILearningMachine<TranslationDomain>`, но **недообучен**
  (acc 0.3333 на HEAD; реальный DPO-прогон 2026-05-11 дал 0.58→0.29 при росте объёма, reward margin
  около нуля/отрицательный). Это конкретное доказательство, что общий `DpoTrainer` РЕАЛЬНО крутится, но
  данные/масштаб слабы — тот же диагноз, что у dynamic-flow scaffold.
- **RAG-04 (design-to-code):** целевой `44.DesignConversionDomain` — это ещё один домен для флоу
  (`design → html+css → server → model → visual-QA → DPO`). Важно: RAG-04 содержит **единственную во
  всём репо работающую точку рантайм-загрузки локальной модели**
  (`25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:304` →
  `DesignConversionPromotedLocalModelLoader.LoadAsync`) — эталон загрузчика, который папке-6 нужен, но
  модель пуста и загрузчик по умолчанию никем не вызывается (0 production-callers). Статус
  `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — тот же класс, что здесь.
- **RAG-01 (compare-branches):** governance-контекст (ceremony-vs-delivery) + карта «три питающих
  контура» (local_wiki / language_translate / external_domains). Все три — домены-кандидаты для
  дин.потока; все три недообучены. `claude-BRANCH-GAP-INTEGRATION-PLAN` — инженерный план, как их
  переносить в DPO-план (RAG-05).

**Общая нить всех шести папок:** один и тот же паттерн — **зрелый дизайн + реальные from-scratch блоки
+ общий `11.DPO/DpoTrainer`, но обученные/подключённые фичи отсутствуют** (dynamic-flow рантайм не
собран; translate 0.3333; wiki 0.286; design-to-code 0/10). Центр (обучение) один, и он пуст. План
«продолжить расширения» должен опираться на реальные блоки + честный ярлык «флоу-рантайм и обученные
юниты не подключены», а не на scaffold-объём и не на «trained»-названия.

### 9.3 Открытые вопросы для Luba (нумерованные, с дефолтами)

Это продуктовые/бизнес-решения, которые нельзя безопасно выбрать локально; для каждого дан безопасный
дефолт (по правилам «technical route ambiguity is not a stop» / «choose smallest safe product spine»),
чтобы план не вставал.

1. **430K-корпус — демо или пересобрать?** Держать 156K-scaffold как inventory-демонстрацию 9-узловой
   схемы в масштабе, или пересобрать реальные обучающие пакеты (LLM-авторские A→B по
   `EXAMINATION-METHOD-FINAL`)? **Дефолт:** оставить scaffold как демо (не удалять — provenance-ценно),
   но НЕ считать обучающими данными; реальные пары генерировать по одному домену через тренер.
2. **Подключить флоу-рантайм или остаться дизайном?** Материализовать топологию-в-БД + обучаемые роли
   (`18.TopologyRuntime`/`19.UnitRegistry` + 5 компонентов 21-23), или пока держать флоу как
   дизайн/оракул под UI? **Дефолт:** минимальный спайн 9.1(d) — node-00 (реален) + ОДИН обученный
   splitter + тонкая обвязка 21-23; полную 30-узловую топологию отложить.
3. **Приоритет O-4 examine-UI в VS-расширении.** Делать examine-UI сейчас (поверх готовых E53-трасс +
   knowledge-edge/learning-gap контрактов) как первый видимый продукт, или после подключения рантайма?
   **Дефолт:** делать сейчас на оракуле/контрактах (самый дешёвый видимый результат), помечая данные
   «оракул, не живой рантайм».
4. **Какие реальные блоки переиспользовать первыми?** Подтвердить порядок: node-00 SemanticCompiler →
   DpoTrainer → IdleDetector → K-NN → агент-инфра 21-23/MCP. **Дефолт:** этот порядок (по зависимостям
   и по «уже в коде и оттестировано»).
5. **External-AI advisory: какой поставщик и когда включать?** Реестр поставщиков спроектирован
   (cost/privacy/reliability/known-failures), но конкретный поставщик и «runtime vs idle-only» не
   выбраны; плюс принцип 5 запрещает Claude/GPT/Anthropic/OpenAI в hot-path. **Дефолт:** оставить
   external-вызов *симулированным* (как в mini-E2E) до явного решения Luba; включать только через
   `PermissionGate` single-use approval, idle-only.
6. **Кто первым становится настоящим обученным юнитом?** domain-splitter (есть ~10 корректных пар) или
   node-00-компилятор (Round D)? **Дефолт:** splitter первым (данные готовы, закрывает round-15
   masquerade), затем node-00.
7. **Легальный teacher-гейт для dynamic-flow.** Взять `TeacherProvenance` (18–20 полей) и license-gate
   из RAG-05/RAG-03 как обязательный фильтр обучающих источников этой папки (принцип 5 сейчас — только
   текст, не контракт)? **Дефолт:** да — переиспользовать контракт RAG-05, не изобретать свой.

**Итого открытых вопросов: 7** (все с безопасными дефолтами; ни один не блокирует старт минимального
спайна 9.1(d)).

### Честная строка покрытия (проход 9/10)

Это синтез-проход: новых поддеревьев не открывал, новых файлов построчно не читал. Синтезированы уже
доказанные находки проходов 1–8 ЭТОЙ папки (каждая с file:line в своём проходе: node-00
`29.SemanticCompiler/Pipeline/*.cs:50-83`; LogicExtractor `*.cs:11-87`; K-NN
`35.ExperimentsV2/Core/*.cs:6-49`; IdleDetector `16.AutonomousLearning/IdleDetector/*.cs:20-62`;
DpoTrainer `11.DPO/DpoTrainer.cs`; grep-подтверждённое отсутствие E51/E52/E53/5-компонентов/узлов в
`.cs`) + cross-folder evidence из RAG-01 (headline + манифест), RAG-02 (headline + host-entrypoint
`wiki-context-hook.exe`), RAG-03 (headline: translate 0.3333/DPO-прогон 2026-05-11), RAG-04 (headline:
загрузчик `DesignConversionModelServerApi.cs:304`, 0/10), RAG-05 (headline + три семьи + F.2 блоки
P-01..P-14, `ILearningDynamicGraph` DESIGN-COMPLETE/CODE-ABSENT). Прочитаны для этого прохода: весь
RAG-06 (1–8), заголовки/манифесты RAG-01/02/03/04/05 (read-only). Не заявляю нового кода/тестов/
рантайма — их нет и в предыдущих проходах. Вердикт синтеза: **дизайн actionable, реальные блоки есть,
рантайм и обученные юниты не подключены; кратчайший спайн и 7 открытых вопросов с дефолтами
сформулированы.** Остаётся проход 10 — completeness-sweep всех 156 971 файла + executive summary.

### Мета

RAG-файл: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`
Проход 9/10 — только чтение (RAG-06 целиком + заголовки RAG-01..05) + append этой секции.
Предыдущие секции (пассы 1–8) не тронуты.

---

## Pass 10/10 — completeness sweep + executive summary (folder #6/12 llm dynamic flow)

Проход 10/10 — **ФИНАЛЬНЫЙ** для папки #6. Единственный агент (Claude Opus 4.8, Ultracode),
только чтение всего RAG-06 (пассы 1–9) + **независимая перепроверка переписи файлов на диске**
(`find -type f`, read-only) + append этой секции. Предыдущие секции не тронуты. Три задачи:
(1) доказать, что ни одна группа из 156 971 файла не пропущена (no-neglect sweep);
(2) дать одну **executive-выжимку**, которую Luba может прочитать ВМЕСТО всего RAG-06;
(3) честный отчёт покрытия (что прочитано вглубь / что покрыто по паттерну / что НЕ запускалось).

### 10.1 Completeness sweep — перепись сверена с диском в этом проходе (0 расхождений)

В пассе 10 я заново пересчитал файлы прямо на диске и сверил с пассами 1 и 7. **Всё сошлось до
одной штуки** (`Доказательство:` `find -type f | wc -l` по каждому поддереву):

| Группа | Файлов (find, пасс 10) | Пасс(ы) покрытия | Глубина | Статус no-neglect |
|---|---:|---|---|---|
| **Верхний уровень** (`maxdepth 1`) = 49 `.md` + 2 `.zip` + 2 `.csv` + 1 `.xml` + 1 `.txt` + 1 `.svg` + 1 `.mmd` | **57** | 1, 3, 4, 5, 6, 8 | DEEP-READ | **COMPLETE** — все мастер-дизайн-доки, амендменты, эксперименты E51/E53, метод, топология прочитаны целиком или охарактеризованы (пасс 8.4). 2 `.zip` = дубль-архивы тех же доков; `previos session.txt` = evidence-only. |
| `all-documents-method-run/` | **156 129** | 2, 7, 8 | PATTERN + SAMPLED | **COMPLETE-BY-PATTERN** — см. ниже (разбивка сошлась: 396+34 860+120 828+23+22). |
| — `business-flows/` | 396 | 7, 8 | pattern + sample | 33 курируемых пакета; генератор + тела сэмплированы |
| — `full-active-source-training-sets/` | 34 860 | 2, 7 | pattern + sample | 2 905 пакетов; `doc-0001` тело + generator-код прочитаны |
| — `full-corpus-unique-training-sets/` | 120 828 | 7 | pattern + sample | 10 069 пакетов; тот же шаблон-семейство (grep-подтверждён) |
| — `all-files-including-zips-compact/` | 23 | 2, 7 | JSONL sample | 9 JSONL (~6,1 ГБ) + манифесты; первая строка node-01 прочитана |
| — корень (оркестрация) | 22 | 7 | DEEP-READ | 7 скриптов (`.mjs`/`.ps1`) + CSV/JSON-манифесты корпуса разобраны |
| `source-examination-cache/` (снимок кода 561 = 401 `.cs`; + docs 142 + session-package 22 + sessions 11) | **736** | 8, 9 | KEY-CODE DEEP-READ + grep-sweep | **COMPLETE** — 6 ключевых `.cs` прочитаны построчно (пасс 8.2); grep по всему snapshot подтвердил ОТСУТСТВИЕ E51/E52/E53/5-компонентов/узлов-флоу (0 в `.cs`); docs/sessions = контекст, охарактеризованы |
| `round-zips/` (15 раундов 2026-05-04) | **37** | 4, 5, 8 | 2 code-zip DEEP-READ + 13 manifest | **COMPLETE** — r01 (код E52) и r15 (код E53 trained-splitter) распакованы и прочитаны; r02–r14 охарактеризованы по `ROUND-MANIFEST-*` (пасс 8.4G) |
| `flow-01-training-sets-v1/` (эталон 12) | **12** | 3 | DEEP-READ | **COMPLETE** — все 9 node-наборов + `00-INDEX` + `10-SIMULATION-CHECK` прочитаны (пасс 3) |
| **ИТОГО** | **156 971** | 1–9 | — | **57 + 156 129 + 736 + 37 + 12 = 156 971 ✓** |

**Честно про 156 129 сгенерированных файлов:** они **покрыты по паттерну + выборочно
просэмплированы, а не прочитаны построчно каждый** — и это НЕ пропуск, а единственно правильный
метод. Все 156 084 тела пакетов произведены **пятью детерминированными скриптами** (`.mjs`/`.ps1`),
чьи функции `classify()`/`domains()`/`rephrase()`/`record()` прочитаны построчно (пассы 2, 7), а
выход подтверждён живыми сэмплами (тело `doc-0001`, первая строка compact-JSONL, node-08) — где
даже видно, что `rephrasedA == A` побайтово. Прочитать 156 084 механически идентичных шаблонных
файла по одному **физически невозможно и логически бессмысленно**: генератор-код + сэмплы дают
100%-ную характеризацию. Оставшиеся 45 инфраструктурных файлов (генераторы + манифесты) прочитаны
вглубь. **Ни одна группа не оставлена без внимания.**

### 10.2 EXECUTIVE SUMMARY (можно читать ТОЛЬКО это — папка #6 «llm dynamic flow»)

**1. Что это и главный вывод.** Папка `llm dynamic flow` (156 971 файл) — это **первоисточник
описания «динамической LLM-модели»** проекта: не один большой LLM, а **самомодифицирующаяся сеть
маленьких обученных юнитов поверх редактируемого графа-потока**. Для плана «продолжить расширения
Visual Studio и VS Code» она даёт три вещи: (а) полное, зрелое **описание архитектуры** динамической
модели; (б) богатую **детерминированную симуляцию** того, что она работает (эксперименты E51/E52/E53,
оракул-трасса); (в) набор реально закодированных **from-scratch блоков**, на которые план может
опереться. Главный честный вывод одним предложением: **дизайн — actionable и полный, но сам
обучающий/рантайм-контур ещё НЕ подключён к коду, «обученные» юниты фактически детерминированы, а
99% корпуса (156K файлов) — механический scaffold, а не обучающие данные.**

**2. Архитектура (что задумано).** Модель описана как **редактируемый граф**: топология = данные в
БД, оркестратор читает её в рантайме; добавить юнит = зарегистрировать cell + ребро, заменить =
двинуть указатель, откатить = вернуть версию (правки на уровне рёбер). **Ядро v1 = 9 узлов**
(Domain Splitter → Translator → Scheme Planner → Processor → Applier → Result Translator →
Arbiter Selector → Arbiter Units → Formatter) — этот каст **физически прошит** в ~13 000
сгенерированных пакетах `node-01…node-09` (подтверждает находку RAG-02). **v2 = 30 узлов N00–N29**:
тот же 9-узловой центр, обёрнутый спереди **слоем сужения** (N00 компилятор логической структуры →
intake-gate → topology-router по 14 областям XIIGen → sub-subject → evidence-pack → local-splitter)
и сзади **слоем обучения/внешней помощи** (event-trail → KNN-память → knowledge-edge → external-AI-
advisory → learning-gap → idle-queue → dataset-curator → confidence-updater). Власть жёстко разделена
заблокированными решениями **D-13/D-14/D-15**: *Priority* решает что пробовать, *KNN = память*,
**Arbiter = что принято (hard-gate)**, *External-AI = только совет, никогда не authority*; ни один
сигнал не перекрывает отказ арбитра; всё внутреннее (Internal-AI throughout), обучение — только в
idle. Встроенные версионирование/rollback + сквозной provenance — техническая основа **examine-UI**.

**3. Реальность (что реально есть — не переоценивать).** Пассы 2–8 дали строгий, привязанный к
коду вердикт: **(а)** дизайн DESIGN-COMPLETE и богат; **(б)** рантайм-контур динамического потока
**НЕ подключён** — grep по snapshot-коду: станции E51/E52/E53, 5 рантайм-компонентов проектов 21-23
(`InternalLlmGateway`, `InternalResultExaminer`, `ExternalAiRequestPackage`, `ExternalAiResultAdapter`,
`LearningEventScheduler`), собственные узлы флоу (`DomainSplitter`/`SchemePlanner`/`DynamicFlow`) —
**0 совпадений в `.cs`**; **(в)** 156K-корпус = **детерминированный scaffold** (генератор-код:
regex-классификатор + фиксированный map доменов + подмена первого слова; ~65% корпуса — вообще
история сессий/git-объекты, а не знания); **(г)** настоящих обучаемых данных **крошечно** — один
корректный splitter-набор (~10 пар A→B), 7 арбитр-негативов node-08, 12 teaching-записей E51B, всё
на ОДНОМ документе (user-registration); **(д)** «обученные» юниты E51/E52/E53 — **детерминированы,
не обучены**: «0%→100%» держится на том, что 3 из 4 baseline захардкожены `false`, а round-15
«trained splitter» = правило-фильтр по 11 захардкоженным правилам с **0 весов/градиента/checkpoint**
(это класс `hidden determinism / masquerade`, R4). Ценность E53 реальна, но это **contract-proof
формы оркестрации**, а не доказательство обучения.

**4. Реальные переиспользуемые блоки + петля «внешняя модель учит внутреннюю через DPO».** Самое
ценное для плана: фундамент реально закодирован и оттестирован (в snapshot; в живом репо аналогично):
**node-00 SemanticCompilerPipeline** (`29.SemanticCompiler/Pipeline/…:50-83`, реальный, wired 6/8
стадий — **самая сильная живая опора**), **22-предикатный LogicExtractor**, **StructuralKnnPredictor**
(brute-force K-NN = «память» D-15), **StructuralVectorExtractor** (Roslyn-фичи), **IdleDetector**
(`16.AutonomousLearning`, 4 условия → SafeToTrain — сердце «обучение не на hot-path»), **DpoTrainer**
(`11.DPO/DpoTrainer.cs`: AdamW, Beta=0.1, `margin=chosen−rejected`, `-log σ(margin)`, checkpointDir —
**центр обучения всей системы**), агент-инфра 21-23 + MCP + Roslyn (**готовый фундамент под host-
адаптер расширения**, R6). Поверх этих блоков v2-слой **буквально описывает целевой «шаг 6» — external
teaches internal via DPO** как *отложенный* контур: сигнал учителя берётся только на named knowledge-
edge (external-AI по D-14 single-use approval); внешний ответ становится DPO-материалом через
**Learning Gap Packet** с готовой разметкой `positive`(chosen)/`negative`(rejected)/`warning` + адрес
«какая роль-юнит × домен учится»; тренировка — в idle, по одному узлу, install-if-better + rollback;
внутренняя модель остаётся авторитетом (защита DPO от reward-hacking внешним ИИ). ⚠️ Контур
СПРОЕКТИРОВАН полно, но **не подключён** — mini-E2E использует *симулированный* внешний ответ и НЕ
переобучает.

**5. Дом examine-UI (O-4) + центральная пара с RAG-05.** `DYNAMIC-FLOW-ARCHITECTURE-PLAN` §Open Items
**O-4** прямым текстом: «Where does the UI live? My default assumption is the **Visual Studio extension**
(subproject 13)». Это прямой мост к цели свода: **examine-аффорданс** (проследить любой ответ назад
через все юниты/рёбра/версии/knowledge-edges) реализуем, потому что principle-6 provenance-trail пишет
каждый шаг. Практический вывод для расширения: **даже пока флоу-рантайм — дизайн, E53-трасса +
knowledge-edge/learning-gap-контракты УЖЕ готовы как оракул/контракт для UI** — это самый дешёвый
первый видимый продукт расширения (показать маршрут через узлы, вердикты арбитров, «край знания»,
context-pack, что ушло в idle-обучение; пометить «оракул, не живой рантайм»). Cross-folder:
**эта папка = ИСТОЧНИК дизайна, RAG-05 (dpo legal build) = его ФОРМАЛИЗАЦИЯ** в контракты
`ILearningDynamicGraph ≡ ILearningMachine<DynamicGraphDomain>` (D-13/14/15 → `EdgeAuthorityUnit`/
GraphDPO). Оба статуса совпадают: **DESIGN-COMPLETE, CODE-ABSENT**. Общий центр обеих — **`11.DPO/
DpoTrainer`**; починка этого одного тренера чинит и dynamic-flow idle-DPO, и translate (RAG-03), и
wiki (RAG-02), и design-to-code (RAG-04) — все они домены поверх одного пустого центра.

**6. Кратчайший спайн, 7 открытых вопросов, must-read.** Минимальный безопасный путь дизайн→рантайм
(не полный v2): **node-00 (реален) как живой вход → материализовать ОДИН обученный узел (domain-
splitter, данные готовы) через `DpoTrainer`+`IdleDetector` → тонкая обвязка 5 компонентов 21-23 →
топология-в-БД (`18.TopologyRuntime`/`19.UnitRegistry`) → заменять фикстуры обученными юнитами по
одному (install-if-better+rollback).** **7 открытых вопросов для Luba** (все с безопасными дефолтами,
ни один не блокирует старт спайна): 1) 430K-корпус — держать как демо или пересобрать реальные пары
(деф.: держать как демо, не считать данными); 2) подключать флоу-рантайм или остаться дизайном (деф.:
минимальный спайн); 3) делать O-4 examine-UI сейчас на оракуле (деф.: да); 4) порядок переиспользования
блоков (деф.: node-00→DpoTrainer→IdleDetector→K-NN→21-23); 5) какой external-AI-поставщик и когда
(деф.: симуляция + `PermissionGate` idle-only до решения Luba); 6) кто первым станет настоящим
обученным юнитом (деф.: splitter); 7) легальный teacher-гейт `TeacherProvenance` из RAG-05 (деф.: да,
переиспользовать). **Must-read файлы** (в порядке важности): `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-
DRAFT.md` (мастер, 7 принципов, O-4), `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` (v2, 14 областей),
`dynamic-flow-v2-node-topology.csv` (карта 30 узлов), `DECISIONS-LOCKED.md` (D-13/14/15),
`PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` (node-00 компилятор), `EXTERNAL-AI-KNOWLEDGE-
EDGE-AMENDMENT-v1.md` (external→internal путь), `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` (E51),
`E53-SCENARIO-001-EXPECTED-TRACE.md` (оракул для examine-UI), `DYNAMIC-FLOW-CANONICAL-INTEGRATION-
SEQUENCE-v1.md` (канон-порядок → 5 рантайм-компонентов); реальный код: `29.SemanticCompiler/Pipeline/
SemanticCompilerPipeline.cs`, `11.DPO/DpoTrainer.cs`, `16.AutonomousLearning/IdleDetector/IdleDetector.cs`,
`35.ExperimentsV2/Core/StructuralKnnPredictor.cs`.

### 10.3 Честный отчёт покрытия (проход 10/10 — вся папка #6)

- **Всего файлов: 156 971** (перепроверено `find -type f` в этом проходе; сошлось с пассами 1 и 7 до
  штуки; каталогов 13 211).
- **Прочитано ВГЛУБЬ (построчно / целиком)** — «настоящий» слой папки: ~49 верхнеуровневых дизайн/
  эксперимент/метод `.md` + 2 топология-`.csv` (пассы 1, 3, 4, 5, 6, 8); 6 ключевых `.cs` снимка кода
  (`SemanticCompilerPipeline`, `LogicExtractor`, `StructuralKnnPredictor`, `StructuralVectorExtractor`,
  `IdleDetector`, `DpoTrainer` — пасс 8.2); код E52 (round-01 zip) и E53 (round-15 zip) — пассы 4, 5;
  все 12 файлов `flow-01-training-sets-v1/` — пасс 3; 7 генераторов `.mjs`/`.ps1` + манифесты
  `all-documents-method-run/` — пасс 7.
- **Покрыто ПО ПАТТЕРНУ + выборочно просэмплировано** (не построчно каждый — детерминированный
  scaffold): **156 084** тела сгенерированных пакетов + 9 compact-JSONL (~6,1 ГБ) в
  `all-documents-method-run/`. Характеризация 100%-ная через прочитанный генератор-код + живые сэмплы
  (тело `doc-0001`, compact-JSONL, node-08); докозано, что это механический шаблон (`rephrasedA==A`).
  Плюс docs/sessions в `source-examination-cache/` и r02–r14 в `round-zips/` — охарактеризованы по
  манифестам/структуре (evidence/context, не имплементация динамического флоу).
- **Что НЕ запускалось** (по правилам прохода — только чтение): не выполнялись `dotnet build`/`dotnet
  test`/`dotnet run`, обучение (`DpoTrainer`/idle-DPO), генерация пакетов (`.mjs`/`.ps1`), распаковка
  всех zip кроме round-01/round-15, живой вызов external-AI. Все выводы — из чтения кода/данных/манифестов,
  не из исполнения. Реального обученного динамического потока в папке нет (это установлено, а не пропущено).
- **Ни одна из 5 групп файлов не пропущена** (57 + 156 129 + 736 + 37 + 12 = 156 971, сверено с диском).

**folder #6/12 RAG COMPLETE.**

### Мета

RAG-файл: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`
Проход 10/10 (ФИНАЛЬНЫЙ) — только чтение (весь RAG-06 пассы 1–9 + независимая перепись `find` на диске)
+ append этой секции. Предыдущие секции (пассы 1–9) не тронуты. Папка #6 «llm dynamic flow» закрыта:
156 971 файл учтён, executive summary написан, честный отчёт покрытия дан.
