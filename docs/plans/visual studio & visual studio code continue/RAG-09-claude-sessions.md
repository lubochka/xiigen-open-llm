# RAG-09 — «claude-sessions» (наши материалы оркестрации: экспорт чатов Claude)

> **RAG-индекс, папка #9 из 12.** Часть общего свода, который готовит план ПРОДОЛЖЕНИЯ
> расширений для Visual Studio и VS Code (офлайн локальная модель: документация кода +
> design-to-code). Роль папки (Luba): **«наши материалы оркестрации»** — выгруженные
> (экспортированные) сессии чатов Claude.
>
> **Честная оговорка о глубине (важно для доверия к этому индексу):** в папке **1209 файлов**,
> из них **968** — это экспортированные чаты Claude в `Uncategorized/<дата>/<uuid>/<Заголовок>.md`.
> За 10 проходов физически невозможно поштучно глубоко разобрать 968 экспортов чатов. Поэтому
> подход честный и без пренебрежения: **КАЖДЫЙ файл учтён и категоризирован** (по кластеру
> темы/дате), а **глубоко читается только оркестрационно-релевантный поднабор** (подпапки
> оркестраторов + протоколы + уже готовый синтез). Ниже прямо помечено, что «категоризировано»,
> а что «глубоко прочитано».

- **Имя папки:** `claude-sessions`
- **Физический путь:** `C:/Users/lubao/Documents/xiigen/claude-sessions`
- **Роль (по словам Luba):** наши материалы оркестрации — exported Claude chat sessions.
- **Проход:** **1/10** (MAP + КАТЕГОРИЗАЦИЯ ВСЕХ файлов + поиск оркестрационного поднабора).
- **Дата индексации:** 2026-07-13.
- **Кросс-линк к RAG-06 (`llm dynamic flow`):** по брифу родителя, «метод экзаменации» из RAG-06
  выводил свои **SIMULATION-INVARIANTS** (categories: contract / invariant / …) из ~20
  высокоприоритетных сессий этой папки (`claude-sessions/Uncategorized/`, май 2026) + из папки
  `claude-sessions/grok orchestrator` (файлы 27,33,37,38,39,41,42). Значит **исходный материал тех
  инвариантов лежит ИМЕННО ЗДЕСЬ** — см. раздел «Оркестрационно-релевантный поднабор».

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R09, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Свежестная отметка: папка-источник — статичный архив экспортированных чатов; индекс (снят 2026-07-13) живых git-фактов не содержит и по содержанию актуален, проверено 2026-07-28. Единственная поправка контекста: картина репозиториев с 13 июля перевернулась (24 июля `main` впитал линии design-to-code и DPO; волна GPL/AGPL 23–26 июля; три хост-репозитория `claude-/cursor-/codex-xiigen-smart-skills` отсутствуют на машине) — любые выводы «куда класть код сегодня» сверять с авторитетной таблицей `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке. Архитектурный инвариант файла «ядро в `llm_mvp_core`, хосты — тонкие адаптеры» — проектный замысел, он этим не затронут.

---

## 0. Что это за папка на самом деле (одна из главных находок прохода 1)

Это **не** тематическая коллекция про оркестрацию. Это **batch-выгрузка ВСЕХ чатов Claude Luba**
из личного аккаунта — по многим проектам сразу. Доказательство — собственный `README.md` папки:
исходник = `old/data-...-batch-0000.zip`, а в `projects.json` перечислены проекты аккаунта, среди
которых **`xiigen` = 503 документа** (доминирующий), а рядом — совершенно посторонние проекты:

| Проект (из `README.md`) | docs | Релевантность нашему плану |
|---|---|---|
| **xiigen** (`019c2e10-…`) | **503** | **ЯДРО** — оркестрация, flow-движок, design-to-code |
| How to use Claude | 1 | низкая |
| כתיבת תוכן (написание контента, иврит) | 4 | нет |
| Immigration | 1 | нет |
| networking system / content generation / easy-peasy / Project management / О себе / daily report / social statistics / seasoned product owner / studies / Developers Studio / Automation skills / Испанский / Французкий | 0 | нет |

**Почему всё свалено в `Uncategorized`:** экспорт Claude **не содержит** привязки сессии к проекту
(`conversations.json` без project-id), поэтому скрипт-выгрузчик не смог разложить чаты по проектам и
положил все 968 в `Uncategorized/<DD.MM.YYYY HH.mm>/<uuid>/<Заголовок>.md` (время Asia/Jerusalem;
`:` нельзя в имени папки Windows). **Имя `.md`-файла = заголовок чата** — этого достаточно, чтобы
категоризировать все 968 без вскрытия каждого.

**Вывод для честности:** бриф говорил «MOSTLY on UNRELATED topics». По факту **большинство (≈60%)
экспортов — XIIGen/оркестрация-релевантны**, потому что это дамп в т.ч. xiigen-проекта (503 docs).
Посторонний «шум» реален, но это **меньшинство** (≈30%), а не большинство. Точные числа — ниже.

---

## 1. Реальный размер и структура (проверено `find`, не по памяти)

**ВСЕГО файлов: 1209.** Верхнеуровневая раскладка:

| Узел | Файлов | Что это |
|---|---:|---|
| `Uncategorized/` | **968** `.md` | экспортированные чаты Claude (916 датированных папок, в каждой `<uuid>/<Заголовок>.md`) |
| **loose top-level** | **49** | протоколы извлечения оркестрации + артефакты прошлого анализа сессий (см. §3) |
| `grok  orchestrator/` | **65** | ⭐ разборы оркестрации (Grok/Claude) + flow-оркестраторы 00–42 + design-to-code адаптеры |
| `sonnet - analyzes/` | **56** | ⭐ пофайловый анализ Sonnet каждого FLOW-оркестратора и протоколов |
| `claude orchestrator/` | **53** | ⭐ FLOW-01…42 orchestration + scenario 10/20-pass (figma-to-code, C#-skill-lib, FLOW-26) |
| `sonnet-user-prompts/` | **7** | триаж пользовательских промптов (accepted/rejected orchestration) |
| `prompt-orchestration/` | **6** | 6 сессий FLOW-29/30/31/33/34 (по одному промпту-оркестрации на файл) |
| `_tools/` | **3** | python/js утилиты выгрузки (`export_project_conversation_map…`) |
| `user prompts/` | **2** | `accepted orchestration pattertns.md`, `user prompts.md` |

⭐ = оркестрационно-релевантный поднабор (глубокое чтение в проходах 2–9).

**Распределение `Uncategorized` по датам** (из имён папок, формат `DD.MM.YYYY`):

| Период | Папок | Период | Папок |
|---|---:|---|---:|
| 05.2025 | 1 | 12.2025 | 37 |
| 09.2025 | 53 | 01.2026 | 23 |
| 10.2025 | 97 | 02.2026 | **394** |
| 11.2025 | 48 | 03.2026 | **227** |
| 01.2025 | 1 | 04.2026 | 35 |

Пик активности — **февраль–март 2026** (621 из 916 датированных папок): это совпадает с активной
фазой проектирования XIIGen flow-движка.

---

## 2. Категоризация ВСЕХ 968 экспортов (без пренебрежения)

Работа уже была частично сделана прошлой сессией и её результат лежит **в самой папке**:
`SESSION-INDEX.md` (индекс всех 968 по 10 категориям) + `SESSION-ANALYZE.json` (машинная категория на
каждую сессию) + `SESSION-ANALYSIS.md` (глубокий синтез 50+ топ-сессий). Я **сверил** их числа своим
независимым `grep` по заголовкам — совпадают. Итоговая раскладка (суммы точно = 968):

### 2.1. Категории из `SESSION-INDEX.md` (проверено, сумма = 968)

| # | Категория | Сессий | Релевантность | Глубина в этом своде |
|---|---|---:|---|---|
| 1 | **XIIGen — Architecture & Flow Design** | **365** | ВЫСОКАЯ | категоризировано; топ-16 уже глубоко в `SESSION-ANALYSIS.md` |
| 2 | **XIIGen — Flow Simulation & Testing** | **41** | ВЫСОКАЯ (источник SIMULATION-INVARIANTS) | топ-12 глубоко в `SESSION-ANALYSIS.md` |
| 3 | **XIIGen — Migration & Phase Implementation** | **37** | СРЕДНЯЯ | топ-6 глубоко |
| 4 | **XIIGen — Figma/Design Adapters** | **28** | ВЫСОКАЯ (design-to-code!) | топ-4 глубоко |
| 5 | **XIIGen — Plugin & Marketplace** | **16** | ВЫСОКАЯ (расширения!) | 1 глубоко |
| 6 | **Orchestration / Planning Sessions** | **98** | ВЫСОКАЯ | 3 глубоко (пересечение с выше) |
| 7 | Infrastructure / n8n / Azure Workflows | 92 | НИЗКАЯ | 0 (нерелевантно плану) |
| 8 | Angular / NestJS / Technical Debugging | 66 | НИЗКАЯ (ранняя разработка) | 0 |
| 9 | **Personal / Other Topics** | **214** | НЕТ | 0 (только учтены) |
| 10 | Untitled Sessions | 11 | неизвестно | 0 |
| | **ИТОГО** | **968** | | |

**Свёртка по релевантности:**
- **Оркестрация/XIIGen-релевантно (кат. 1–6): ≈ 585 сессий (60%).** Это ядро — именно ради него
  папка попала в наш свод.
- **Пограничное (кат. 7, n8n/Azure/workflow-debug): 92.** Автоматизация процессов, но не про наш
  движок; помечаю LOW, поштучно не читаю.
- **Явно посторонее (кат. 8–10: Angular-debug + Personal + Untitled): 291 (30%).** Учтены как
  кластер, глубоко НЕ читаются (см. §2.3).

### 2.2. Машинная категория из `SESSION-ANALYZE.json` (кросс-проверка)
`xiigen` = 308 · `other` = 646 · `personal` = 14 (сумма = 968). Это более консервативная разметка
(строгий фильтр по имени «xiigen»); индексная разметка §2.1 шире (учитывает flow/RAG/fabric/figma
без слова «xiigen»). Обе согласуются: релевантное ядро — несколько сотен сессий, не единицы.

### 2.3. Что именно составляет «посторонний» кластер (чтобы ничего не выдавать за релевантное)
Прямая выборка заголовков подтвердила посторонние темы — их видно по заголовку, вскрывать не нужно:
- **Иврит (личное):** резюме/CV, страховые формы (הראל), опросы, платежи штрафов, анимация к
  презентации, сравнение ноутбуков ONYX BOOK — ~10 шт.
- **Русский личный/справочный:** «Самые цитируемые испанские/французские слова / слова на иврите»,
  «Стадии бешенства у кошек», «Восстановление озонового слоя», «Путешествие на пароходе», «Покупка
  недвижимости в Мексике», «LinkedIn для кризисного менеджера», «Приложения для диджея», «Квантовые
  вычисления: основы», «Канадская иммиграция», загадки, «Генетическое сходство человека и дельфина».
- **Виза/иммиграция:** «Исправления формы I-140», «Виза талантов в США».
- **Ранний технический дебаг (не наш движок):** Selenium/Angular date-picker, CORS на stage,
  «403 Forbidden на Azure Search», Webpack SizeOnlySource, «Windows Server 2019 image retirement»,
  n8n JSON validation, Telegram-форматирование, YouTube-описания, разбивки видео-сценариев.
- **`untitled-session` / `We need to support a system th` / `Please follow carefully _conso`** —
  обрезанные заголовки; 11 явных untitled.

Точный автоматический подсчёт по off-topic-ключам дал **≈75** заведомо личных/посторонних заголовков
внутри `Uncategorized`; вместе с Angular-дебагом и untitled это укладывается в кат. 8–10 (291).

### 2.4. Доминирующие релевантные заголовки-кластеры (top по частоте, все — XIIGen)
Прямая частотная выборка по 968 заголовкам (это фактические, не выдуманные, числа):

| Повторов | Заголовок-кластер (XIIGen flow-движок) |
|---:|---|
| 81 | AI-powered dynamic workflow generation architecture |
| 44 | AI-powered flow generation system architecture |
| 21 | Dynamic flow generation system architecture with AI and RAG integration |
| 19 | AI-powered dynamic flow generation architecture |
| 18 | Fabric-first engine extension for complex flow generation |
| 17 | Data validation and HTML format conversion (валидация выходных JSON→HTML отчётов) |
| 14 | Engine extension for flow creation support |
| 10 | Validating task report JSON data / Employee task summary report / Data validation… |

Т.е. крупнейшие повторяющиеся кластеры — это **сам flow-движок XIIGen** (генерация динамических
воркфлоу, fabric-first расширение движка, RAG-интеграция), плюс валидация JSON/HTML-отчётов задач.
Это прямо перекликается с целью нашего плана (движок, который сам себя строит + документация/дизайн).

---

## 3. Оркестрационно-релевантный поднабор (глубокий разбор — фактически прочитано в этом проходе)

Здесь лежит то, ради чего папка в своде. Всё ниже **вскрыто и прочитано** в проходе 1 (цитирую
реальное содержимое). Это и есть исходники SIMULATION-INVARIANTS из кросс-линка RAG-06.

### 3.1. Уже готовый синтез прошлого анализа (loose top-level) — читать в первую очередь
- **`SESSION-ANALYSIS.md` (94 KB, 24 части / ~56 архитектурных паттернов, Russian).** ⭐ГЛАВНЫЙ файл.
  Это дистиллят 50+ топ-сессий XIIGen. Ключевое, что уже извлечено:
  - **Часть 1 — 5-этапный pipeline разработки флоу:** `FLOW-PREP → SIMULATION → GAP-PREP →
    GAP-TRANSLATE → Claude Code` (строго последовательно). FLOW-PREP = 19 раундов R0–R19
    (graph seeding, NODE grades, named checks, arbiter promotion, genesis triples…). SIMULATION =
    35 раундов (группы E/A/B/C/D/Synthesis). **Это и есть «SIMULATION» из имени SIMULATION-INVARIANTS.**
  - **Gap-классы:** H (infra), J (graph/routing), K (interface), L (persistence), M (RAG/curriculum),
    N (named check), A/B/G (scenario). **H0 Cadence Rule:** одна сессия = один файл, после раунда
    STOP → ждать «continue».
  - **Часть 2 — критические gaps:** напр. FLOW-12 GAP E-11 «все DPO-тройки помечены INVALID, т.к.
    `determineDpoQuality` ждёт `chosen:{code}` а handler пишет `chosen:string`» — прямо про DPO,
    релевантно RAG-05. FLOW-11: «figmaNodes discarded at API boundary → Figma-to-Code value
    proposition structurally unfulfilled» — прямой урок для нашего design-to-code.
  - **Часть 5 — NODE-примитив + 4-cycle chain:** NODE поля structure/intent/constraints/quality,
    вердикты LEAF/EXPAND, max depth = 3, 3-model parallel convergence, blind judging.
  - **Часть 6 — Graph RAG (fabric-first), 4-role AI decision pipeline, arbiter panel.**
  - **Части 8, 11, 12** — Figma adapter quality gates; recursive design simulation (7-depth ladder,
    Design Co-Architect vs Code-Developer mode); stack coupling taxonomy (90/10 split).
- **`SESSION-INDEX.md` (68 KB):** индекс всех 968 по 10 категориям с датой/кол-вом сообщений/названием
  (источник §2.1). Это «карта» всего дампа.
- **`SESSION-ANALYZE.json` (490 KB)** + `SESSION-ANALYZE-RESULTS-batch-01..08.json`: машинная
  категоризация 968 сессий и результаты 8 воркеров-суб-агентов (какие паттерны нашли). `AGENT-INSTRUCTIONS.md`
  — инструкция тем воркерам (правило: пропускать non-xiigen и msg_count<6).
- **Протоколы извлечения оркестрации (4 версии + complete):** `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2`,
  `…-COMPLETE-v1`, `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1`. Главное правило (v2):
  «Извлечение оркестрации — это не *что входит в план*, а *как цель пользователя превращается в
  контролируемый, проверяемый путь к работающему результату*». Это методология описания оркестрации
  на человеческом языке — прямой шаблон для нашего плана расширений.
- **`FILE-ANALYSIS-PROTOCOL-v1.md` — 18 МБ (!).** Огромный склеенный дамп; в проходе 10 отметить как
  «крупный агрегат, не для поштучного чтения». Пока — только учтён.
- **`samples of orchestarion examinations.md` (151 KB):** примеры «экзаменации оркестрации» (чтение
  MASTER_EXECUTION_PLAN / SESSION_STATE / UNIFIED_SOURCE_INDEX) — образцы метода.
- **`_tmp_row_915..944.txt` (30 шт.):** временные выгрузки отдельных строк манифеста (по одной
  сессии на файл) — рабочие артефакты, низкая ценность, учтены.

### 3.2. `grok  orchestrator/` (65) — ⭐ разборы оркестрации + design-to-code
Это чат-экспорты, где Luba просила модель «по-человечески описать оркестрацию реализации проекта» из
входных файлов. Прочитаны образцы:
- **`00–42 flow orchestrator.md`** (примерно 33 файла с номерами): по одному FLOW. Напр. `33 flow
  orchestrator.md` — «Оркестрация реализации проекта (как система сама себя строит)»: мини-ядро
  (Kernel: MicroserviceBase, DataProcessResult, ParseDocument, Scope Isolation, DynamicController;
  Layer-0 fabrics IDatabaseService/IQueueService/IAiProvider/IRagService/IFlowOrchestrator; Flow
  Engine исполняет JSON-DAG из Elasticsearch), затем **bootstrap-flow `platform-bootstrap-v1`** —
  DAG первого старта. Это **ключевой «self-building engine» нарратив**.
- **`GROK-ANALYSIS-AND-LINKING-PROTOCOL-v1.md`** — протокол разбора *любого* документа проекта:
  6 типов файлов (высокоуровневые планировщики; flow-оркестраторы 00–42; implementation/preparation;
  gap-анализ/симуляции; протоколы/правила; адаптеры) + как строить единую картину оркестрации.
- **Design-to-code адаптеры (ПРЯМО наш VS-план):** `adapt figma to code to canva/mira/webflow(.md/.txt)`,
  **`DRAWIO_TO_PROJECT_PLAN_GUIDE.md`** (drawio-UML → план проекта; входные: possible umls.drawio.xml,
  FigmaToCode-with-skills.zip, DEFINITIVE_MODULE_ARCHITECTURE, genie-skill-library-v6). Тема — «Small
  Business Networking App на базе XIIGen V17 (Genie DNA)»: постепенное описание функционала → тех-дизайн
  → этапы реализации из Figma/drawio.
- **Планы/пакеты:** `FLOW-PREP-LIBRARY-MASTER-PLAN.md`, `FLOW-01-02 plan files.md`, `V17 plan package.(md/txt)`,
  `XIIGEN_NESTJS_REACT_TRANSLATION.md`, `learning-pipeline-test-plan-v2.md`, `ui-e2e-test-plan-v2.md`,
  `module-architecture.md`, `genie-plugin-skill-factory.md`, `architecture gap.md`, `xiigen docs.(md/txt)`,
  `GROK_PROJECT_FILES_STATE-v1.json`, `prompts.txt`.
- **Подпапка `grok agent analyze/`:** 00_VISION…, 01_FLOW_PREPARATION…, 02_FLOW_ORCHESTRATOR_SERIES…,
  03_GAP_AND_SIMULATION… — краткие аналитические срезы серии оркестраторов.
- **`flow-01-phase-a-fixtures.zip.d`** — распакованные фикстуры FLOW-01 фазы A.

### 3.3. `claude orchestrator/` (53) — ⭐ FLOW-01…42 + scenario-passes
- **`FLOW-01-orchestration.md` … `FLOW-42-orchestration.md`** (42 файла): построчное человеческое
  описание оркестрации каждого FLOW. Напр. **`FLOW-31-orchestration.md` = «Design Intelligence Engine
  (Функциональность через Дизайн)»** — как превратить Figma-файл в полное архитектурное понимание
  системы (тип продукта, модули, бизнес-потоки, пробелы) без ручного описания. **Это прямое ядро
  design-to-code** для наших расширений.
- **`ADAPTER-C5-canva / M1-miro / W1-webflow-orchestration.md`** — оркестрация адаптеров к дизайн-хостам.
- **`scenario-10pass-*` и `scenario-20pass-*`** (figma-to-code, csharp-skill-library, flow26-integration)
  + **`scenario-20pass-SYNTHESIS.md`** и `scenario-10pass-COMPARISON-ANALYSIS.md`. Это **метод
  экзаменации Claude-vs-Grok** по 3 сценариям × N проходов × 4 измерения (Planning / Functional /
  User journey / E2E). Главный вывод синтеза: **«DATA FLOW MAP систематически пропускается обеими
  моделями»** — точные типы данных на каждой границе интерфейса не описываются. **Это и есть тот
  «метод экзаменации», из которого RAG-06 вывел свои contract/invariant-категории.**

### 3.4. `sonnet - analyzes/` (56) — ⭐ пофайловый анализ
Sonnet-разбор каждого артефакта: `FLOW-01…33-orchestration-analysis.md`, анализы всех
`ORCHESTRATION-EXTRACTION-PROTOCOL`-версий, `SESSION-ANALYSIS-analysis.md`, `SESSION-INDEX-analysis.md`,
`_manifest-analysis.md`, `ALL-FILES-LIST.txt`, анализы python-утилит. Полезно как «второе мнение» по
каждому оркестратору.

### 3.5. `prompt-orchestration/` (6), `sonnet-user-prompts/` (7), `user prompts/` (2), `_tools/` (3)
- `prompt-orchestration/` — 6 отобранных сессий (FLOW-29 Adaptive RAG Deep Research, FLOW-30 PromptOps
  Self-Learning, FLOW-31 Figma Screen Understanding, FLOW-33 System Initiation / DB Fabric P2, FLOW-34
  Skill Multi-Target Translation) — «эталонные» промпты оркестрации.
- `sonnet-user-prompts/` — триаж: `sonnet - accepted/rejected user prompts.md`,
  `sonnet - approved/declined orchestration.md` + csv (`_toprocess/_triage/_workset`). Это **разметка,
  какие пользовательские промпты дают хорошую оркестрацию** — ценно для дизайна промптов расширения.
- `user prompts/` — `accepted orchestration pattertns.md`, `user prompts.md`.
- `_tools/` — `export_project_conversation_map_from_claude_console.js`, `apply_project_conversation_map.py`
  (скрипты выгрузки/маппинга — как этот дамп был получен).

---

## 4. Как это связано с RAG-06 (SIMULATION-INVARIANTS) и с нашим планом расширений

- **Источник инвариантов.** RAG-06 описывает «метод экзаменации», выделяющий категории
  **contract** (форма in/out на границе) / **invariant** (граф/маршрутизация) / … По брифу родителя
  этот метод питался ~20 сессиями отсюда (`Uncategorized/`, май 2026) + `grok orchestrator`
  (27,33,37,38,39,41,42). В этой папке лежит **сырьё и уже готовый дистиллят** тех инвариантов:
  `SESSION-ANALYSIS.md` Часть 1 (SIMULATION 35 раундов, gap-классы H/J/K/L/M/N), Часть 5 (NODE
  contract structure/intent/constraints/quality), а «DATA FLOW MAP пропускается» из
  `scenario-20pass-SYNTHESIS.md` — прямая мотивация для «contract»-категории.
- **Design-to-code (левая половина нашей цели).** `FLOW-31-orchestration.md` (Design Intelligence
  Engine), `grok orchestrator/DRAWIO_TO_PROJECT_PLAN_GUIDE.md`, `adapt figma to code to *`, кат. 4
  (Figma/Design Adapters, 28 сессий), урок FLOW-11 «figmaNodes discarded at API boundary» — всё это
  прямые материалы для design-to-code части расширений VS/VS Code.
- **Оркестрация «движок сам себя строит» (правая половина).** `33/… flow orchestrator.md` (Kernel +
  bootstrap-flow), 5-этапный pipeline `FLOW-PREP→SIMULATION→GAP-PREP→GAP-TRANSLATE→Claude Code`,
  протоколы извлечения оркестрации — методология описания «пути от цели пользователя к проверяемому
  результату», которую наш план должен воспроизвести человеко-читаемо.
- **Промпт-дизайн расширения.** `sonnet-user-prompts/` + `user prompts/` = размеченный корпус
  «какой промпт → хорошая оркестрация», полезен для API-слоя расширения (простые «задача→ответ»).

---

## 5. Рамка проходов (2–10) — что делают следующие 9 заходов

- **Проход 1 (этот):** ✅ MAP + категоризация всех 1209 файлов; найден оркестрационный поднабор;
  установлена честная граница «категоризировано vs глубоко прочитано».
- **Проходы 2–3:** глубоко разобрать `SESSION-ANALYSIS.md` (все 24 части / 56 паттернов) — это самый
  плотный дистиллят; вынести все паттерны, релевантные движку/design-to-code/DPO.
- **Проход 4:** `claude orchestrator/` FLOW-01…42 + FLOW-31 (Design Intelligence) детально.
- **Проход 5:** `grok  orchestrator/` — flow orchestrator 00–42 + DRAWIO/figma-адаптеры (design-to-code).
- **Проход 6:** `scenario-10/20-pass-*` + `SYNTHESIS` — метод экзаменации Claude-vs-Grok (мост к RAG-06).
- **Проход 7:** протоколы `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE` + `GROK-ANALYSIS-AND-LINKING`.
- **Проход 8:** `sonnet - analyzes/` (пофайловые анализы) + `prompt-orchestration/` (6 эталонов).
- **Проход 9:** `sonnet-user-prompts/` + `user prompts/` (корпус промптов) + `_tools/` (как выгружалось);
  выборочно вскрыть 15–25 самых крупных `Uncategorized`-сессий по `SESSION-INDEX.md` (по msg_count),
  которых нет в `SESSION-ANALYSIS.md`.
- **Проход 10:** completeness-sweep — подтвердить, что все 1209 файлов учтены по кластеру; executive
  summary; явно зафиксировать, что посторонний массив (кат. 7–10, ≈383) категоризирован, но НЕ
  читался поштучно, и это честно и достаточно.

**Явная честность о покрытии (проход 1):** глубоко прочитано в этом заходе — ≈15 файлов
(README, AGENT-INSTRUCTIONS, SESSION-ANALYSIS ч.1–5, SESSION-INDEX, образцы grok/claude orchestrator,
FLOW-31, scenario-20pass-SYNTHESIS, extraction-protocol-v2, GROK-ANALYSIS-protocol, DRAWIO-guide).
Категоризированы (по заголовку/кластеру/дате) — **все 1209**. Ни один файл не отброшен как «шум»:
посторонний массив честно назван и локализован (кат. 7–10), а не выдан за релевантный.

---

## Pass 2/10 — SESSION-ANALYSIS.md (мастер-дистиллят: 24 части / 56 паттернов оркестрации)

> **Проход 2/10 (этот).** Глубоко прочитан `SESSION-ANALYSIS.md`
> (`C:/Users/lubao/Documents/xiigen/claude-sessions/SESSION-ANALYSIS.md`, 94 246 байт, 1285 строк),
> плюс сверены индекс `SESSION-INDEX.md` (67 947 байт) и машинный каталог `SESSION-ANALYZE.json`
> (501 807 байт). Это самый плотный человеко-авторский дистиллят всей папки — дата дистилляции
> **2026-05-28**. Раздел добавлен, ранние разделы (проход 1) не тронуты.

### 2.0. ЧЕСТНАЯ ПОПРАВКА СЧЁТА: не «24 части / 56 паттернов», а **35 частей / 119 паттернов**

Заголовок этого прохода (и §3.1 прохода 1) говорили «24 части / ~56 паттернов». **Прямое чтение файла
показывает другое, и это важно для доверия к своду:**

| Метрика | Заявлено (бриф/проход 1) | Фактически в файле (проверено) | Доказательство |
|---|---:|---:|---|
| Частей `## ЧАСТЬ N` | 24 | **35** (ЧАСТЬ 1 … ЧАСТЬ 35) | `grep -n "^## ЧАСТЬ"` → 35 совпадений |
| Пронумерованных паттернов в `SUMMARY` | 56 | **119** (пункты 1…119) | последний пункт SUMMARY = `119.` |
| Глубоко обработанных сессий | «50+» | 50+ вручную **+ 278 машинно** | `SESSION-ANALYZE.json.metadata` |

**Откуда взялась цифра «56» — и почему обе цифры правдивы (реконсиляция):** `SESSION-ANALYZE.json`
в `metadata` хранит `"total_new_patterns": 63` и `"final_total_patterns": 119`. Арифметика сходится
точно: **56 (исходный ручной анализ, Части 1–24) + 63 (добавлено 8 батчами суб-агентов, Части 25–35)
= 119**. То есть «24 части / 56 паттернов» — это **исходное состояние** файла (ручной разбор топ-сессий),
а «35 частей / 119 паттернов» — **текущее** состояние после того, как 8 батчей воркеров-суб-агентов
(`batch-01…batch-07`, обработано `processed: 278` из 968) дописали Части 25–35. В SUMMARY это видно
буквально: пункты **1–56 = Части 1–24** (ручной блок, заканчивается «56. FLOW-01..09 Self-Reference»),
пункты **57–119 = Части 25–35** (батч-блок, начинается «57. V15 Glass-Box Recursive Design Factory»).
Дальше в этом разделе счёт ведётся по **фактическим 35 частям / 119 паттернам**.

### 2.1. Три файла и роль каждого (проверено)

| Файл | Размер | Роль | Ключевые поля |
|---|---:|---|---|
| `SESSION-ANALYSIS.md` | 94 KB | ⭐ мастер-дистиллят (35 частей, SUMMARY 119 паттернов) | человеко-читаемые паттерны + источник-uuid + msg_count |
| `SESSION-INDEX.md` | 68 KB | человеческий индекс всех 968 сессий по 10 категориям | `Date | Msgs | Session Name`, отсортировано по дате/размеру |
| `SESSION-ANALYZE.json` | 490 KB | машинный каталог 968 сессий + агрегат батчей | per-session: `category`(xiigen/other/personal), `priority`(skip/low/medium/high), `status`, `batch`, `patterns_summary` |

`SESSION-ANALYZE.json.metadata` (дословно): `total_sessions:968`, `processed:278`, `pending_high:4`,
`pending_medium:16`, `pending_low:360`, `skip:440`, `total_batches:8`, `batch_size:50`,
`all_batches_complete:true`, `total_new_patterns:63`, `final_total_patterns:119`. Указатель на протокол:
`"protocol_file": "llm mvp/orchestrator extension/ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md"`. То есть
каталог сам ссылается на протокол извлечения оркестрации (проход 7 нашего свода).

### 2.2. Часть 1 — 5-этапный pipeline разработки флоу (полностью, это и есть «SIMULATION» из инвариантов)

**Строго последовательный конвейер (нельзя параллелить):**
`FLOW-PREP → SIMULATION → GAP-PREP → GAP-TRANSLATE → Claude Code`.

**FLOW-PREP = 19 раундов R0–R19** (источник: сессии bd1cc96c/152 msg, 638643f1/84 msg): R0 pre-authoring
+ init `STATE.json`; R1 graph seeding (21 edge в `xiigen-decision-graph`); R2 NODE completeness grades;
R3 infra gates; R4 CloudEvent name verification; R5 named-check registry (+~20 checks с тестами);
R6 ANALYTICS archetype registration L1–L4; R7 Phase-B routing record; R8 Phase-C calibration triples;
R9 Phase-F edge confidence; R10 arbiter promotion; R11 genesis patch triples; R12 NODE quality
correlation edges; R13 cross-flow dependency edges; R14 learning handoff; R15 `OUTCOME_PENDING→VALIDATED`;
R16–R19 wave gate + final checks.

**SIMULATION = 35 раундов**, группами: **GROUP E** (R0–R8, planning-layer correctness, 9 gaps),
**GROUP A** (R9–R13, спроектированные сценарии, 5 gaps), **GROUP B** (R14–R21, продуктовые сценарии,
9 gaps), **GROUP C/D** (advanced, pending), **Synthesis** (финальная консолидация gaps). Это буквальный
источник слова «SIMULATION» в имени `SIMULATION-INVARIANTS` из RAG-06.

**Gap-классы в SIMULATION** (таксономия дефектов): `H` infra (missing fixtures) · `J` graph/routing ·
`K` interface · `L` persistence · `M` RAG/curriculum · `N` named-check · `A/B/G` scenario-specific.

**H0 Cadence Rule** (MACHINE): одна сессия = один файл; после каждого раунда `present → ⛔ STOP →
ждать "continue" от Luba`; нарушение = немедленная коррекция. Имя файла раунда — канон
`FLOW-{XX}-{STAGE}-R{N}.md`; НИКОГДА префикс `SESSION-` и копирование имён из шаблонов (регрессия
после compaction). **Прямой урок для нашего плана:** человеко-читаемый план должен описывать именно
такой контролируемый путь «цель → проверяемые раунды → STOP-контроль», а не монолит.

### 2.3. Части 5+20+21 — NODE-примитив, cycle-chain, Pipeline Data Contracts, «XIIGen = компилятор»

**NODE — универсальный примитив домена** (Часть 5, Часть 21). Поля: `structure / intent / constraints /
quality`. Вердикты: `LEAF` (терминальный) vs `EXPAND` (рекурсивный, max depth = 3). Инвариант
`CONVERGENCE_MISSING_CONSTRAINTS`: пустой `constraints[]` → авто-fail. Центральный тезис Части 21:
**NODE — это «missing primitive»**, верифицированное представление, построенное через multi-model
convergence **ДО** генерации кода. Это прямой аналог того, что наш design-to-code должен строить из
Figma/дизайна ПЕРЕД кодом.

**Cycle-chain pipeline** (Часть 5): Cycle 1 Intent→Plan (PlannerHandler + Plan Reviewer); Cycle 2 Plan
Step→NODE (3-model parallel convergence, blind judging, challenger isolation); Cycle 3 Depth Decision
(LEAF/EXPAND, max depth 3); Cycle 4 Leaf NODE→Executor (AF pipeline); Cycle 5 (Meta) Meta-Arbiter при
`grade < 0.85`. **3 канала обучения:** `CODE_REASONING | DESIGN_REASONING | CONVERGENCE_REASONING`.

**Pipeline Data Contracts + «XIIGen is a Compiler»** (Часть 20, сессия c356c556/305 msg — крупнейшая
проработанная): ментальная модель — *«каждая стадия принимает форму входа, обязана произвести форму
выхода; стадия с неправильной формой молча портит весь downstream»*. Перед изменением любого файла
надо явно назвать: что стадия Receives / Must Produce / Who consumes / What consumer needs. Это **та же
идея, что в RAG-06 стала категорией `contract`** (форма in/out на границе). Конкретная таблица контрактов
(SESSION-LOAD-PLAN v16): Cycle-1 Planner получает `userIntent + constraints[]` → обязан выдать plan-steps
(абстрактные, доменные, одна ответственность, без tech-имён, с dependency chain) — **иначе generic-шаги
→ немедленная convergence на round 1 → бесполезная DPO-тройка**. Cycle-2 Convergence получает один шаг +
доменный контекст → NODE-spec со step-specific constraints — **иначе generic DNA-rules = мгновенная
convergence**.

**Конфигурация 3 моделей** (важно как реальный оркестрационный факт): Node 1 = Gemini
(`AI_GEMINI_PROVIDER`, gemini-2.5-flash-lite); Node 2 = GPT-4.1 (`AI_OPENAI_PROVIDER`); Node 3 = Claude
judge (`AI_JUDGE_PROVIDER`, claude-haiku-4-5); Node 4 = **Claude Code (external)** читает
`CYCLE-4 PENDING_IMPLEMENTATION`. **Живые баги движка при первом запуске:** (1) `AI_PROVIDER` env =
MOCK MODE → всегда grade 1.00 → реальные провайдеры не запускаются; (2) Cycle-1→Cycle-2 handoff сломан
(10 plan-steps, 0 Cycle-2 раундов); (3) `convergence.handler CONTEXT_INSUFFICIENT` — должен применить
протокол к себе до генерации. **Ментальная модель Flows-as-Nodes:** flows = nodes, skills = nodes,
arbiters = evaluator-nodes, judge = top-level orchestrator node (composed node graph, не иерархия).

### 2.4. Полная карта 35 частей (что где лежит + релевантность плану расширений VS/VS Code)

Легенда релевантности: **D2C** design-to-code (левая половина цели) · **SBE** self-building engine
(правая половина) · **ORCH** методология оркестрации · **GOV** session-governance/скиллы · **FAB**
fabric-first архитектура · **DPO** обучение/тройки · **TEST** тестирование · **SEC** безопасность.

| Часть | Тема (одной строкой) | Тэг | Для нашего плана |
|---:|---|---|---|
| 1 | 5-этапный pipeline; FLOW-PREP 19R; SIMULATION 35R; gap-классы H/J/K/L/M/N; H0 Cadence | ORCH | ⭐ шаблон контролируемого пути «цель→раунды→STOP» |
| 2 | Критические gaps: FLOW-12 billing (BootstrapCycleRouter не инжектится), FLOW-11 moderation, DPO E-11, **figmaNodes discard** | DPO/D2C | ⭐ уроки для DPO-контрактов и Figma-границы |
| 3 | Типичные структурные баги планов FLOW-18..34 (missing arbiter coupling, machineFreedom, WHAT CHANGED…) | GOV | чек-лист качества плана |
| 4 | 8 entry types; 4 MACHINE pattern types; archetype registry 20→33; pattern transfer rates; infra-gap growth | FAB | таксономии |
| 5 | ⭐ NODE-примитив; 4/5-cycle chain; 3 teaching channels; FLOW_SCOPED multitenant (CF-POLICY-01) | ORCH | ⭐ ядро «понимание до кода» |
| 6 | Graph RAG: `IGraphRagService` multi-provider; edge mgmt (promote/decay/rollback); 4-role AI pipeline; arbiter panel (scope_isolation ≥ 4) | FAB/RAG | архитектура RAG-слоя |
| 7 | Planning Skills Library v2.8 — 12 обязательных skills перед simulation (86 skills / 4 layers) | GOV | как грузить скиллы |
| 8 | ⭐ Figma adapter — Gate A(FC-1..15)/Gate B(AI cross-review)/Gate C(human); commit blast radius; test scale (~2710 tests, <$0.09) | D2C | ⭐ quality-gates для design-to-code |
| 9 | Product variants (saas/oss/enterprise/lean); solution bundles как shareable templates; D-34-1 thin-adapter | product | модель поставки |
| 10 | Flow reference plan формат; критические bug-паттерны (`taskTypeCycleBudgets:{}`…); Phase naming Z/O/P; порог 0.85 | GOV | формат плана |
| 11 | ⭐ Recursive Design Simulation: 7-depth ladder (request→records→events→subflow→merge→impl→tests); **Design Co-Architect vs Code-Developer mode** | D2C/ORCH | ⭐ рекурсивный разбор дизайна |
| 12 | Stack Coupling Taxonomy: `stackCategory`(closed enum 22) + `stackType`(open string); StackKey; **90/10 split**; StackCapabilityDeclaration; 65+ plugins/14 SDK | FAB/plugin | как расширять на новые хосты (VS/VS Code) |
| 13 | Skills v2.8.0: SK-510..519 Layer-5 «Dynamic Decision Architecture»; GENERATED vs REINFORCEMENT; 3-way outcome | GOV/DPO | скилл-слой обучения |
| 14 | ⭐ 7 канонических merged-файлов (SKILLS/UNIFIED_SOURCE_INDEX/BFA/TASK_TYPES/ENGINE_ARCH/SESSION_STATE/MASTER_EXECUTION_PLAN); gapless numbering; полный Task-Type Contract | GOV/core | как аккумулируется знание движка |
| 15 | Freedom Machine Philosophy: «если бизнес может захотеть менять → динамическое»; 5 поколений эволюции; AI-gen prompts как meta-skills | core | философия MACHINE/FREEDOM |
| 16 | Pre-Flow Infra Amendment Protocol: **amendments, не rewrites**; `ironRules:string[]` неизменен → parallel `ironRulesStructured?`; LIFO compensation | GOV | как править планы без слома |
| 17 | Skills v8 + FLOW-00.3 cost pipeline; **Genesis prompts = AF-1 INPUT** (не инструкции Claude Code); implementationMode:"af-pipeline" | GOV/SBE | ключ: кто пишет код |
| 18 | ⭐ SESSION-START GATE: читать все скиллы реально; H0 conflict table; two-phase PLANNING→approval→SIMULATION; Layer1(exec) vs Layer2(teaching) | GOV | ⭐ протокол старта сессии |
| 19 | Knowledge Governance (CF-POLICY-01, default PRIVATE, Class A/B/C portability); i18n FLOW-48; **Gate 0i «DIG IN DOCS FIRST»**; review-stage hierarchy | GOV | anti-ложный-BLOCK |
| 20 | ⭐ Pipeline Data Contracts; «XIIGen = компилятор»; 3-model config; mock-mode bug; Flows-as-Nodes | ORCH/core | ⭐ мост к RAG-06 `contract` |
| 21 | ⭐ Рефрейминг — 5 gaps to designed state (NODE absent; RAG-as-DB; uniform arbiters; no planning-DPO; no self-context); Business Arbiter cross-system; branch-merge renumbering | ORCH/core | ⭐ целевое состояние |
| 22 | Governance remediation GR-001 «zero tech debt, full stop»; ARCHITECT rules A-1..A-4; SK-554 NDJSON arbiter types | GOV | правила архитектора |
| 23 | Flow Planning Library (10 файлов вместо 5 мастер-планов, FLOW-00..47); ошибки генерации библиотеки; 5 audit skills; issue-rejection (верифицировать reviewer через код) | GOV | как строить библиотеку guidance |
| 24 | Branching/merge audit; renumbering (T577+/F1502+); **fabricated-name anti-pattern** (grep перед документированием); authoritative event names; process self-verification gap | GOV | честность процесса |
| 25 | ⭐ Engine patterns: V15 Glass-Box (наблюдаемое AI-мышление, продукт = сам resolution-loop); 5 resolution patterns; 3-sub-engine; 11-station factory; three-loop; Judge Panel V25U; **fabric-first 5-layer**; version-drift metric | FAB/core | ⭐ архитектура движка |
| 26 | Skills & graduation: tool maturity L0-L4; 8-file skill (SKILL.md+.NET+5 языков+implement.md); COPY/ADAPT/REWRITE; dynamic skill genesis; graduation 3 unseen domains; skill quality 6-dim 0.70; plugin-skill-factory 6-phase | GOV/SBE | ⭐ как рождаются/зреют скиллы |
| 27 | Fabric extensions: RagFabric 6 стратегий; **Secrets Fabric (7-й, $secret/$env)**; DNA-7 W3C trace; IPromptLibrary (versioned); Queue Main→Consumed→Archive→DLQ; Operational Fabric F39-F65; native-SDK rule | FAB | инфраструктурные фабрики |
| 28 | Flow dev patterns: **two-document deliverable** (SESSION-MASTER + MISSING-INFRASTRUCTURE); infra-convergence; gate-script curl/jq only; foundation flow; InjectionContract 6-stage; wait-states | GOV/SBE | формат deliverable на флоу |
| 29 | ⭐ Self-building engine: **T58 self-extension** (task-type генерит task-types); FLOW-26 meta-flow; bidirectional code-ingestion (V42); **two-mode framing** (planning vs implementation); AWAITING_DECISION + 5 design questions | SBE | ⭐ движок расширяет себя |
| 30 | Testing & governance: negative-bypass tests; FC-1..12 taxonomy (FC-10 propagation critical); scene-replay; 3 task archetypes + PhaseValidation + HumanApprovalGate; ANALYSIS+MIGRATION modes; executable gates | TEST | тест-стратегия |
| 31 | Advanced data: universal factory 12 services (V39); Phase-0B mini-RAG warm-up; prepaid wallet IR23-25; INJECTABLE vs PLATFORM-ONLY; counters; ERP value streams; flow mutation/extraction | FAB | данные/провайдеры |
| 32 | Security & moderation: Security Arbiter (isolated personas + SAST + adversarial); Moderation GATE 3-outcome; BFA API contracts registration | SEC | безопасность генерации |
| 33 | ⭐ Platform & Figma: **two-context isolation** (sandbox vs iframe, postMessage, clientStorage); QUIRK-11 + **SceneNode postMessage-getter bug** (y→sceneSettings); platform-adapter figma-types.d.ts shim | D2C | ⭐ реальные Figma-плагин-грабли |
| 34 | V28D: CrossReferenceExecutor (код+дизайн одновременно); HumanDialogNode; AmendmentArtifact; AF-14/AF-15 Judge Panel; FLOW-47 marketplace designBundleRefs gate | SBE/D2C | сравнение гетерогенных артефактов |
| 35 | Прочее high-value: **UML→JSON DAG (D13)**; System Absorption Archetype 4 (brownfield); GraphRAG golden-path; ComplexityOracle (детерминизм архетипа); transactional outbox; **context-compression phase-snapshots**; config-driven provider; FLOW-33 Python; Figma-plugin monetization + ML skill builder; Session-Load-Plan v16 | MIXED | ⭐ отдельные золотые паттерны |

### 2.5. Паттерны, наиболее релевантные плану расширений (сгруппировано)

**A. Design-to-code (левая половина цели — наш VS/VS Code «дизайн → код»):**
- **Три quality-gate** (Ч.8): Gate A = FC-1..15 автопроверки; Gate B = AI cross-review (audit-protocol
  10-point, теги 🔴 BLOCKING / ⚠️ WRITE-TIME / ℹ️ INFO); Gate C = human (Luba). Commit-blast-radius:
  GOVERNANCE/SOURCE/TEST-ONLY/PRODUCTION-CI — **атомарные раздельные коммиты**, нельзя смешивать.
- **Recursive Design Simulation** (Ч.11): 7-ступенчатая лестница request→records→events→subflow-paths→
  merges→implementation→tests; правило «раунды производят ФЛОУ, не analysis-секции»; «что может пойти
  не так» → явный arbiter ИЛИ subflow, **НИКОГДА заметки**. **Design Co-Architect vs Code-Developer
  mode** — Luba останавливает сессии, когда они сваливаются в code-developer; фикс — грузить
  `XIIGEN-DESIGN-ARCHITECT-SESSION-GUIDE-v1.md` в начале.
- **Платформенный адаптер Figma** (Ч.33): плагин живёт в 2 изолированных JS-контекстах (sandbox с
  Figma API без DOM ↔ iframe с React/DOM без Figma API), связь только `postMessage`; хранить ключи в
  `figma.clientStorage`, не `localStorage`. **Критический баг сериализации:** getter-свойства SceneNode
  (в т.ч. `y`) НЕ переживают `postMessage` → сортировка по `source.y` даёт всем y=0; фикс — читать из
  `element.style.sceneSettings`. Pipeline-файлы (styles.ts, element-code.ts, models/) **всегда копируются
  без изменений**, платформенный код — в ОДНОМ adapter-файле, совместимость через `figma-types.d.ts`
  shim (масштаб: Framer ~100 строк/2д → Webflow ~250/3.5д → Miro ~460/5д).
- **figmaNodes discard** (Ч.2, FLOW-11): `figmaNodes` отбрасываются на API-границе → value-proposition
  «Figma-to-Code» структурно не выполняется. **Прямое предупреждение для нашего плана:** проверить,
  что дизайн-узлы не теряются на границе адаптер→ядро.
- **UML→JSON DAG (D13, Ч.35):** канонический способ превращать UML-sequence-диаграммы в исполняемые
  JSON-DAG флоу в Elasticsearch (D13 дал 197 узлов / 222 ребра в 8 флоу) — прямой мост «диаграмма → флоу»
  для нашей design-to-code половины (перекликается с `DRAWIO_TO_PROJECT_PLAN_GUIDE.md` из прохода 1).

**B. Методология оркестрации / session-governance (шаблон для человеко-читаемого плана):**
- **SESSION-START GATE** (Ч.18): реально прочитать все скиллы (не назвать), подтвердить каждый SK-номером
  + одной фразой; вынести все H0-конфликты явно; подтвердить scope. Обязательная H0 Conflict Resolution
  Table (default/override/authority). Two-Phase: PLANNING → явное одобрение → SIMULATION. **Layer 1
  (execution scripts) vs Layer 2 (teaching content): Layer 2 — цель, Layer 1 — инструмент, не путать.**
- **7 канонических merged-файлов** (Ч.14) с **gapless sequential numbering** по сериям F/T/CF/ST/DD/SK/
  Template/Family; полный формат Task-Type Contract (one-line stubs = BUILD FAILURE).
- **Amendment Protocol** (Ч.16): не переписывать утверждённые планы, а добавлять нумерованные amendments
  (A1, A2…) с rationale; типы, ломающие 140 контрактов, — только parallel-field.
- **Gate 0i «DIG IN DOCS FIRST»** (Ч.19): нельзя поднимать BLOCK без поиска в project-knowledge —
  предотвращает ложные блокировки стандартных паттернов. **Fabricated-name anti-pattern** (Ч.24):
  проверять имена task-type/factory через grep ПЕРЕД документированием.
- **Process self-verification gap** (Ч.24): тот же rigour, что к генерируемому коду, не применялся к
  session-management; фикс — обязательный post-fix verification step.
- **two-document deliverable на флоу** (Ч.28): `SESSION-FLOW-XX-MASTER.md` (исполнимый план,
  curl/jq/node, `$T_START/$T_END` из STATE.json) + `FLOW-XX-MISSING-INFRASTRUCTURE.md` (gap-анализ,
  Tier A известные / Tier B новые). Gate-скрипты — **только curl/jq, без псевдокода**.

**C. Self-building engine (правая половина — «движок сам себя строит»):**
- **T58 self-extension contract** (Ч.29): task-type, генерящий новые task-types (META-архетип) —
  замыкает цикл self-building. **FLOW-26 meta-flow:** detect gap → generate contracts/code → validation
  loops → sandbox → governance ladder (CORE требует human approval). **Two-mode framing** (обязательный
  mode-gate в начале сессии): Mode A (specifying engine output) vs Mode B (writing service code) — иначе
  дрейф. **Genesis prompts = AF-1 INPUT** (Ч.17), а не инструкции для Claude Code.
- **Bidirectional / Code Ingestion** (Ч.29, V42) + **System Absorption Archetype 4** (Ч.35) —
  brownfield-приём существующего кода в типизированные skill-интерфейсы (релевантно нашей половине
  «документация существующего кода»).

**D. DPO / learning loop (мост к RAG-05):**
- **DPO E-11** (Ч.2): все тройки permanently INVALID, т.к. `determineDpoQuality` ждёт `chosen:{code}`,
  а handler пишет `chosen:string` — системный класс type-mismatch багов.
- **Generic steps → useless DPO** (Ч.20): plan-шаги обязаны заставлять 3 модели делать РАЗНЫЙ выбор;
  generic-шаг → convergence на round 1 → бесполезная тройка. **GENERATED vs REINFORCEMENT** (Ч.13):
  разные training-категории, разный `countsTowardThreshold`. **Graduation** (Ч.26): plan_completeness
  ≥0.90 и code_quality ≥0.85 на 3 подряд невиданных доменах; ≥83 DPO-троек для локального fine-tuning.
- **Learning feedback loop 4-way break** (Ч.2, FLOW-11): PersistentFeedbackStore in-memory / human
  feedback не доходит до prompt / ratings не связаны с passed / evolved prompts не применяются.

**E. Fabric-first архитектура (общий каркас):**
- **Fabric-First Five-Layer Hierarchy** (Ч.25, обязательный prompt-anchor): L0 Fabric Interfaces
  (Database/AI/Arbiter/Queue/RAG/Core) → L1 Factory (CreateAsync) → L2 Engine Contracts → L3 AF-станции
  1..11 → L4 Guardrails (BFA+DNA) → L5 Flow as Output. **Version-drift metric:** считать engine-refs;
  V71=0 refs = потеря концепции движка. **11-station AI factory:** Task Line (1-5) + Judge Line (6-10)
  + Decision Gate (11). **Judge Panel V25U:** специалист-судьи (architecture/security/data/perf/
  compliance/UX) параллельно, 4 стратегии агрегации (ALL_PASS/WEIGHTED/VETO_ONLY/MAJORITY), AF-14/AF-15.
- **90/10 split** (Ч.12): 90% общий core (Layer 2 неизменен) + 10% платформенный адаптер (только Layer 3)
  — **это прямая модель для наших VS/VS Code хостов: тонкий адаптер, ядро в llm_mvp_core** (совпадает с
  правилами R5/R6 из CLAUDE.md).
- **Secrets Fabric** (Ч.27, 7-я фабрика, `$secret:`/`$env:`), **DNA-7 W3C trace context**, **IPromptLibrary**
  (промпты как версионируемые ассеты), **Queue Main→Consumed→Archive→DLQ**, **ComplexityOracle** (Ч.35,
  детерминированный выбор архетипа), **Context-Compression phase-snapshots** (Ч.35, cold re-entry без
  полной истории — прямой паттерн для наших многосессионных RAG-проходов).

### 2.6. Связь с RAG-06 (SIMULATION-INVARIANTS): что уже использовано и что ещё НЕ вошло

- **Уже стало инвариантами RAG-06:** категория `contract` ← Часть 20 «Pipeline Data Contracts / XIIGen =
  компилятор» + `scenario-20pass-SYNTHESIS` («DATA FLOW MAP пропускается обеими моделями»); категория
  `invariant` (граф/маршрутизация) ← Часть 5 NODE + gap-класс `J` + arbiter-panel `scope_isolation ≥ 4`;
  «SIMULATION» ← Часть 1 (35 раундов). NODE-контракт `structure/intent/constraints/quality` — прямой
  первоисточник «contract»-примитива.
- **Ценное, чего (по брифу) НЕТ в RAG-06 и стоит поднять в план:** (1) **Design Co-Architect vs
  Code-Developer mode** (Ч.11) — режимный якорь, критичный для design-to-code; (2) **Two-mode framing /
  Genesis=AF-1-input** (Ч.17/29) — «кто пишет код» (совпадает с R2/R6 нашего CLAUDE.md); (3) **SceneNode
  postMessage-getter bug + two-context isolation** (Ч.33) — конкретные Figma-плагин-грабли для реального
  хоста; (4) **90/10 split + Stack Coupling Taxonomy** (Ч.12) — как масштабировать на новый хост
  (VS/VS Code) тонким адаптером; (5) **fabric-first 5-layer + version-drift metric** (Ч.25) — anchor
  против архитектурного дрейфа промпта; (6) **Context-Compression phase-snapshots** (Ч.35) — метод
  устойчивости длинных многосессионных планов (как наш 10-проходный RAG).

### 2.7. Что остаётся для проходов 3–9 (уточнено после глубокого чтения)

- **Проход 3:** дочитать «хвостовые» части, не раскрытые здесь дословно, на уровне цитат кода/файлов —
  Части 25–35 (батч-блок, 63 паттерна): каждая ссылается на конкретные session-uuid (`batch-01..07`),
  вынести источники и точные числа-счётчики (F/T/CF/ST/SK/DD/DR) для трассируемости.
- **Проход 4:** `claude orchestrator/` FLOW-01…42 + **FLOW-31 Design Intelligence** детально (левая
  половина).
- **Проход 5:** `grok  orchestrator/` flow-оркестраторы 00–42 + `DRAWIO_TO_PROJECT_PLAN_GUIDE` / figma-
  адаптеры (мост к Части 35 D13 UML→DAG).
- **Проход 6:** `scenario-10/20-pass-*` + `SYNTHESIS` — метод экзаменации Claude-vs-Grok (мост к RAG-06;
  подтвердить «DATA FLOW MAP пропускается» первоисточником).
- **Проход 7:** протоколы `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE` + `GROK-ANALYSIS-AND-LINKING`
  (на них ссылается `SESSION-ANALYZE.json.metadata.protocol_file`).
- **Проход 8:** `sonnet - analyzes/` пофайловые анализы + `prompt-orchestration/` 6 эталонов.
- **Проход 9:** `sonnet-user-prompts/` + `user prompts/` (корпус «промпт → хорошая оркестрация», для
  API-слоя расширения) + `_tools/`; выборочно 15–25 крупнейших `Uncategorized`-сессий по `SESSION-INDEX.md`,
  которых нет в Частях 1–35 (напр. по msg_count: 462/305/279/213/152/145/128 — часть уже процитирована).

**Честность о покрытии прохода 2:** дословно прочитан весь `SESSION-ANALYSIS.md` (1285 строк, все 35
частей + SUMMARY 1–119), сверены `metadata` в `SESSION-ANALYZE.json` и шапка `SESSION-INDEX.md`.
Исправлен счёт 24/56 → 35/119 с арифметическим доказательством (56+63=119). НЕ читались поштучно 968
экспортов чатов и 490 KB тела `SESSION-ANALYZE.json` (только `metadata` + структура записи) — это задача
выборочного прохода 9, не прохода 2.

---

## Pass 3/10 — claude orchestrator FLOW-01..42 + FLOW-31 Design Intelligence + scenario-synthesis

> **Проход:** **3/10.** **Дата:** 2026-07-13. **Что глубоко прочитано дословно:** весь
> `FLOW-31-orchestration.md` (340 строк, полностью), `scenario-20pass-SYNTHESIS.md` (309 строк),
> `scenario-10pass-COMPARISON-ANALYSIS.md` (207 строк). **Что прочитано на уровне шапки+ЗАЧЕМ:**
> все 42 файла `FLOW-01..42-orchestration.md` (H1-заголовок + секция «ЗАЧЕМ»/«Что мы строим») + 3
> адаптера (`ADAPTER-C5/M1/W1`). **Физический путь подпапки:**
> `C:/Users/lubao/Documents/xiigen/claude-sessions/claude orchestrator/` (53 файла).
> **Примечание по нумерации:** в шапке прохода 2 (§2.7) было сказано «FLOW-каталог = проход 4»; родитель
> перенёс эту работу в **проход 3**. Данный раздел закрывает бывший «проход 4»; ниже §3.6 переуточняет
> очередь 4–9.

### 3.1. Что такое серия FLOW-01..42 (каталог оркестрационных возможностей)

**Главная находка:** `claude orchestrator/` — это **каталог из 42 «оркестраций»**, где каждый файл
FLOW-NN — это разбор одной готовой сессии-оркестрации XIIGen по единому шаблону: **ЗАЧЕМ → КАК ВЫГЛЯДИТ
→ КАК УСТРОЕНО (семьи/фабрики F-XXXX, типы задач T-XXX, BFA-правила CF, шаблоны) → В КАКОМ ПОРЯДКЕ
(фазы) → ИЗВЕСТНЫЕ БАГИ**. Это **эталонный корпус «промпт-сессия → орке­страционный отчёт»** — ровно тот
few-shot-материал, из которого RAG-06 выводил свои SIMULATION-INVARIANTS. Каждый FLOW = одна
оркестрационная capability. По содержанию делятся на 3 класса:

**Класс 1 — прикладные домены (что движок УМЕЕТ построить как продукт), FLOW-01..17, 19–24, 28:**

| FLOW | Название | Возможность (одной строкой) |
|---|---|---|
| 01 | User Registration & Onboarding | регистрация/онбординг пользователя |
| 02 | Business Onboarding & Personalization | онбординг бизнеса + персонализация |
| 03 | Event Creation & Promotion | создание/промо событий |
| 04 | Post Publishing & Feed Distribution | публикация постов + раздача в ленту |
| 05 | Lesson Completion & Gamification | завершение уроков + геймификация |
| 06 | Marketplace Publishing & Distribution | публикация в маркетплейс (Outbox-паттерн) |
| 07 | Friend Request & Feed Integration | заявки в друзья + интеграция ленты |
| 08 | Multi-Tenant Foundation Research | ресёрч мультиарендного фундамента |
| 09 | Event Participation & Social Integration | участие в событиях + соц-интеграция |
| 10 | Shops & Commerce Modules | модули магазинов/коммерции |
| 11 | Social Network Modules | модули соцсети |
| 12 | Multi-Tenant Engine (ERP Integration) | мультиарендный движок + ERP + Stack Coupling Taxonomy (90/10 split) |
| 13 | Enterprise Finance Engine | корпоративный финансовый движок |
| 14 | Data Warehouse & Integration Engine | DWH + интеграции |
| 15 | Multi-Tenancy MVP Builder Platform | платформа-конструктор MVP |
| 16 | Giant Shop Marketplace | крупный маркетплейс-магазин |
| 17 | Freelancer Marketplace Platform | биржа фрилансеров |
| 19 | CI/CD & DevOps Control Plane | панель управления CI/CD/DevOps |
| 20 | Sponsored Content + Graph API + Ads (LARGEST) | рекламная платформа — **самый крупный флоу** |
| 21 | Forms & Flow Automation Builder | конструктор форм и автоматизаций |
| 24 | Learning Calendar / Personal AI Tutor | учебный календарь + персональный AI-тьютор |
| 28 | Blog/CMS Modules Platform | модули блога/CMS |

**Класс 2 — визуальные редакторы / design-to-code (ПРЯМО РЕЛЕВАНТНО расширению), FLOW-18, 22, 23, 31, 34, 41, 42 + адаптеры:** см. §3.2.

**Класс 3 — мета-флоу и движок само­построения/само­обучения (ядро оркестратора), FLOW-25, 26, 27, 29, 30, 32, 33, 36, 37, 38, 39, 40:**

| FLOW | Название | Возможность движка (одной строкой) |
|---|---|---|
| 25 | Business Flow Arbiter / Cross-Flow Governance | «радиус взрыва» изменения + 4-пунктное арбитраж-меню (rework/reject/wrap/force); жёсткий предикат для FLOW-26; 37 своих BFA-правил CF-473..509 |
| 26 | Self-Developing Meta-Flow | **флоу, который строит флоу**: на `CapabilityGapDetected` → REUSE_BEFORE_GENERATE (RAG-скан → COPY/ADAPT/REWRITE, пропуск = балл 0) → генерация фабрики → 11 станций AF → sandbox → тест → GitOps PR → ассимиляция. 24 типа задач (максимум). Машина повышения SelfBuildRun: DRAFT→WIRED→VALIDATED→INJECTED→MINIMAL→**CORE** (CORE = ручное одобрение, обойти нельзя) |
| 27 | Human Interaction Gate + Dependency Scheduler | примитив EP-6: устойчивые возобновляемые человеческие шлюзы; ставит на паузу только затронутую ветку DAG, переживает рестарт; Redis SETNX atomic claim; FLOW-26 использует его для COREHumanApprovalGate (взаимная зависимость) |
| 29 | Adaptive RAG + Contextual Bandit | система сама выбирает стратегию извлечения знаний под запрос и улучшает выбор со временем (bandit) |
| 30 | PromptOps: Self-Learning Prompt Engineering | авто-улучшение промптов на данных: версионирование, A/B, защита от регрессий, мультиарендная изоляция |
| 32 | Template Marketplace / Sharable Flows | маркетплейс потоков (npm для AI-пайплайнов; торгуемый актив = ЛОГИКА/граф) + первая инициализация RAG |
| 33 | System Initiation / Bootstrap Kernel | **как платформа запускается впервые** без фабрик/RAG/потоков: читает планы → компилирует реестры → засевает двухслойный GraphRAG (Catalog Graph + Impact Graph) |
| 36 | Feature Registry / Self-Implementing Workload | движок = свой первый заказчик: сам генерит benchmark-инструментарий, судит качество, копит DPO-трипли; **решает «local LLM vs paid API»** data-driven матрицей |
| 37 | Engine Self-Awareness & Skill Infrastructure | pre-phase capability gate (rag-retrieve→validate→route→ai-generate→feedback): перед каждой фазой движок проверяет, хватает ли ему возможностей, иначе блок + remediation |
| 38 | Learning Loop Completion | **память между флоу**: 6+1 петель обучения (Loop 1 DPO→RAG promotion при teachingPoint в 3+ трипли из 2+ флоу; Loop 2 RAG quality evolution; и т.д.) — каждый следующий флоу умнее |
| 39 | Flow Design Teaching & Local Model Curriculum | см. §3.2 — **локальная модель + curriculum** |
| 40 | Client Push Infrastructure | инфраструктура клиентских push-уведомлений |

### 3.2. Design-to-code подмножество (то, ради чего этот проход) — ПРЯМАЯ РЕЛЕВАНТНОСТЬ VS/VSCode

Расширение VS/VSCode должно уметь **дизайн → код** и **офлайн локальную модель**. В корпусе это покрыто
плотнее всего:

- **FLOW-18 — Visual Flow Creation & Code Injection (META-FLOW).** «Водительское сиденье» платформы:
  пользователь рисует DAG-намерение на визуальном холсте → AI классифицирует намерение каждого узла →
  генерирует код на нескольких языках параллельно → **AST-валидация** → **горячая инъекция** в живой
  движок. 4 новых архетипа: `VISUAL_CREATION / CODE_INJECTION / SANDBOX_TEST / COLLABORATION`.
  Семьи 84–93 (F631–F696, 66 фабрик).
- **FLOW-22 / FLOW-23 — Visual Editor & Site Builder / Visual Editor Extended (Canvas/Layout).**
  Визуальный редактор сайтов и расширенная канва/раскладка — прямой аналог «дизайн-поверхности».
- **FLOW-31 — Design Intelligence Engine.** Figma → архитектура. Полный разбор — §3.3.
- **FLOW-34 — Skill Multi-Target Translation.** Одна каноническая spec → множество адаптеров: 5
  серверных SDK (Node/Go/Java/Rust/PHP) + 3 клиента (React/Vue/Angular) + WordPress Plugin/Theme.
  Это фундамент «экосистемы адаптеров» — «маркетплейс плагинов», где **каждый адаптер = отдельный npm-пакет**.
- **FLOW-41 / ADAPTER-C5 — Canva Text Elements.** AI-улучшение CSS-стилей текста в Canva; **90% кода
  переиспользуется** из Figma-плагина (`readSelection()` → `CanvaTextElement[]` из `@canva/design`).
- **FLOW-42 / ADAPTER-M1 — Miro Shape Heuristics.** 10 confidence-scored эвристик + пространственный
  анализ (что внутри чего) → вывод структуры layout из «бесконечного холста» без flex/grid.
- **ADAPTER-W1 — Webflow Designer Extension.** Перенос раскладки+текста Figma→Webflow (у Webflow есть
  настоящая CSS-модель: flex/padding/border/shadow/typography).
- **FLOW-39 — Flow Design Teaching & Local Model Curriculum (КЛЮЧЕВОЕ для офлайн-модели).** Решает две
  проблемы: (1) 6 flow-design skills лежат `.md`-файлами и **не видны skill-injection-станции** →
  засевает их в RAG (bootstrap-boundary, flow-vs-service-gate, topology-structure, self-questioning,
  learning-signal-capture, flow-design-cycle); (2) **curriculum-упорядочивание снижает порог fine-tuning
  локальной модели вдвое** → модель переключается раньше, экономит ~$40/проект. Prerequisite: FLOW-38.

**Вывод-мост:** связка FLOW-31 (понять дизайн) → FLOW-18/22/23 (визуально собрать/инжектить код) →
FLOW-34/41/42/адаптеры (перенести в конкретный целевой стек) → FLOW-36/37/38/39 (локальная модель учится
и заменяет платный API) — это ровно тот конвейер, который расширение VS/VSCode должно предоставить как
тонкий хост-адаптер поверх ядра `llm_mvp_core`.

### 3.3. FLOW-31 Design Intelligence Engine — детально (Figma → архитектура)

**Задача:** превратить Figma-файл в полное **архитектурное понимание** системы без ручного описания
модулей/потоков/зависимостей. Итог = **SystemModelIR**: тип системы + карта модулей + бизнес-потоки +
пробелы, с confidence ≥ 0.7 для «definitive», иначе обязательная метка «ambiguous».

**3-слойный IR-конвейер:**
- **Слой 1 — DesignIR (структурный, 0 LLM):** `screens[]`, `components[]` (+instanceCount), `designTokens`
  (цвета/шрифты/spacing из Figma Variables), `screenMap` (adjacency), `prototypeLinks` (trigger/action).
- **Слой 2 — ScreenSemanticsIR (функциональный, per-screen):** `screen_archetype` (один из 8:
  list|detail|form|feed|profile|checkout|report|settings), `ui_controls[]`, `user_actions[]`
  ("AddToCart"/"SubmitOrder"), `data_entities[]` ("Product"/"Order"), **`evidence[]` =
  [{nodeId,text,componentName}] — ОБЯЗАТЕЛЕН для каждого claim**. Строит Family 193 (Claude Vision +
  GPT-4o параллельно).
- **Слой 3 — SystemModelIR (глобальный):** `system_type_candidates` [{type,confidence,evidence}],
  `module_map`, `flows[]`, `gaps[]`. Требует прохождения всех 4 quality gates.

**Масштаб:** 9 семей (Families 191–199), 68 фабрик (F1271–F1338), 27 типов задач (T489–T515), 8 шаблонов
(Templates 83–90), 63 новых BFA-правила + 26 стресс-тестов. Семьи: 191 Figma Ingestion, 192 DesignIR
Processing (**НОЛЬ LLM — BUILD FAILURE если LLM вызван**), 193 AI Semantic Extraction, 194 Graph Index,
195 Vector RAG, 196 Module Mapping (матрица 20×15), 197 System Type Inference, 198 Gap Completion (все
заглушки = GENERATED tier + DynamicController), 199 Learning Loop (IJudgeOrchestrator31Service).

**Формула scoring модуля-кандидата (сумма весов = 1.0, нарушение = BUILD FAILURE):**
`score = evidence_weight×0.5 + vector_sim×0.3 + graph_hop_score×0.2`.

**4 quality gate (последовательно):** (1) **Evidence Coverage** — каждый claim ≥1 evidence (IRON RULE,
не warning); (2) **Cross-Screen Consistency** — одна сущность ("Product") описана идентично на всех
экранах; (3) **Module Dependency Constraint** — если есть Cart, то Checkout обязан присутствовать или
явно `cart-only`; (4) **Genie DNA Gate** — запрещены хардкод site-type значений (тип выводится из данных).

**7 фаз исполнения:** Ф1 Ingestion (T489–493, rate-limit guard) → Ф2 DesignIR (T494–497,
**детерминированно, 0 LLM**) → Ф3 AI Semantic (T498–500, Claude Vision‖GPT-4o, затем Gate 1) → Ф4
Graph+Vector (T501–507) → Ф5 Module Mapping + System Type (T508–511, Gate 2 и Gate 3) → Ф6 Gap Completion
(T512–513, стабы) → Ф7 Learning Loop (T514–515: негативные exemplars→RAG-корпус, позитивные→label
benchmark). **Правило:** AI стартует только с Ф3; при падении точности в Learning Loop > 5% → обязательное
ЧЕЛОВЕЧЕСКОЕ РЕВЬЮ.

**Известные баги (важны для реального хоста):** (1) файлы «31 …REPORT.md» относятся к FLOW-30 (31 = номер
прохода, не FLOW); настоящие = `FLOW31_VALIDATION_REPORT.md`, `FLOW31_MERGE_MASTER_PLAN.md`; (2) фантомный
архетип `MAPPING` (дублировал MODULE) удалён; (3) session-isolation смещение +196 (5 пропущенных FLOW
26–30), источник истины = `RENUMBER_MAP.json`; (4) конфликт тестовых baseline 3513→3696; (5) коллизия
CF-range FLOW-26/27. Итог: ALL CHECKS PASSED, 109 547 строк.

### 3.4. scenario-synthesis: метод экзаменации Claude-vs-Grok (первоисточник инварианта «DATA FLOW MAP»)

**Устройство эксперимента:** 3 сценария (**Figma-to-Code / CSharp Skill Library / FLOW-26 Integration**)
× 20 итерационных проходов × 4 измерения (planning / functional / user-journey / e2e). Проходы 1–10 —
`scenario-10pass-*`, 11–20 — `scenario-20pass-*`, свод — `scenario-20pass-SYNTHESIS.md`. Grok-базлайны
(~90–140 строк каждый) сравниваются с Claude (10-pass ~280–320 строк; 20-pass ~1060–1460 строк, +970%).

**ПАТТЕРН A (главная находка, подтверждает RAG-06): «DATA FLOW MAP систематически пропускается ОБЕИМИ
моделями».** Точные типы данных на входе/выходе каждого интерфейса Grok не даёт **ни разу** за 3
сценария; Claude 1–10 — частично (таблица «что делает» без типов); появилось только в Pass 11. Правильная
форма (пример): `IDatabaseService: IN string index/prefix/id + CancellationToken → OUT
DataProcessResult<Dictionary<string,object>> → FAILURE (error code, never throws)`. **Почему критично:**
FLOW-26 при генерации новой фабрики обязан знать точную форму данных, иначе
ContractGenerationOrchestrator произведёт неверный контракт — **data flow map = machine-readable spec для
самособирающегося движка**. Именно этот вывод стал примитивом `contract` в SIMULATION-INVARIANTS RAG-06.

**Остальные 5 паттернов пробелов (все отсутствуют у Grok):** B — **Failure Cascade** (что происходит при
отказе компонента и как ошибка каскадит; ключевое: `DataProcessResult<T>` содержит ошибку, но **не
транзакционную границу** → нужен Outbox/Saga); C — **Performance Envelope** (latency/throughput; напр.
5 AI-стадий ×10s = 50s; context-economics: Copilot ~8K токенов не тянет Phase 4+); D — **Sprint
Planning** (реалистичная разбивка по спринтам + MVP-веха); E — **Risk Register** (Top-5 рисков P×I;
найден **blocking**-риск: FLOW-26 5/7 → numbering collision для FLOW-27, если не обновить SESSION_STATE);
F — **Roads Not Taken** (отклонённые альтернативы + почему — обучающие данные «что НЕ делать»).

**11 новых протокольных правил** добавлены в `GROK-ORCHESTRATION-EXTRACTION-PROTOCOL-v1` (4 новых секции
6b–6g: User Journey, E2E Validation Frame, Data Flow Map, Failure Containment Boundaries, Risk Register,
Roads Not Taken; + 4 negative habits: 20 Journey Invisibility, 21 E2E Elision, 22 **Data Shape Blindness**,
23 Risk Invisibility; + 3 классификатора типов сессий).

**3 типа сессий с разным E2E-критерием (важно для «как тестировать»):** TYPE A Foundation/Spec (Figma-to-
Code): E2E = contract completeness, тестов 0 (реализаций ещё нет), маркер = DNA baseline; TYPE B Meta-
Orchestration (Skill Library): E2E = structural completeness (все форматы parseable), тесты отложены;
TYPE C Integration/Merge (FLOW-26): E2E = cross-file consistency, runtime verification (FlowOrchestrator
резолвит новые фабрики?), маркер = **SESSION_STATE_MERGE обновлён** (без этого = incomplete).

**Итоговый вывод синтеза:** Grok = human-first narrative (entry point, читается за 3 мин); Claude 20-pass
= practitioner + machine-training-data (типы данных, failure paths, счётчики тестов, risk-скоринг, sprint-
карты, roads-not-taken). Идеальный orchestration-отчёт = **narrative-качество Grok + глубина Claude** —
именно это должен авто-генерировать FLOW-26.

### 3.5. Выводы для плана расширения VS / VS Code

1. **FLOW-31 — прямой blueprint design-to-code intelligence** для расширения: 3-слойный IR
   (структура→семантика→системная модель), обязательный evidence-per-claim (nodeId+текст+компонент),
   scoring 0.5/0.3/0.2, 4 гейта, confidence-порог 0.7 «definitive vs ambiguous». Детерминированный слой
   (0 LLM) отделён от AI-слоя — совпадает с нашим R4 (no hidden determinism) и R6 (тонкий агент/ядро).
2. **Экосистема адаптеров (FLOW-34/41/42 + C5/M1/W1)** = модель «одна каноническая spec → тонкие
   per-host npm-адаптеры, 90% кода переиспользуется». Прямо ложится на «тонкий хост-адаптер поверх
   `llm_mvp_core`»: VS/VSCode = ещё один адаптер, а не форк ядра.
3. **FLOW-39 (local-model curriculum) + FLOW-36/37/38** = готовая методика офлайн-модели:
   curriculum снижает порог fine-tuning вдвое; skill-injection обязан видеть flow-design skills;
   петли DPO→RAG-promotion дают «память между сессиями» — то, что нужно офлайн-режиму расширения.
4. **DATA FLOW MAP + Failure Cascade — обязательные секции любой оркестрации** (иначе self-building
   генерит неверные контракты). Для документации-кода расширения это значит: интерфейсы описывать в
   форме `IN(тип)→[трансформация]→OUT(тип)→FAILURE(код)`, а не «что делает».
5. **Human gate неустраним** (FLOW-26 CORE-стадия, FLOW-27 EP-6): любой авто-инжект кода в живой движок
   проходит человеческое одобрение — прямой аналог требования Luba про подтверждения перед мутацией.

### 3.6. Что остаётся для проходов 4–9 (переуточнено после прохода 3)

- **Проход 4:** `grok  orchestrator/` — flow-оркестраторы 00–42 (grok-версии тех же флоу) +
  `DRAWIO_TO_PROJECT_PLAN_GUIDE` / figma-адаптеры (мост UML→DAG; первоисточник инвариантов из грок-файлов
  26/27/33/37/38/39/41/42, на которые ссылался RAG-06).
- **Проход 5:** `scenario-10/20-pass-*` полные тела (`figma-to-code` 84 KB, `csharp-skill-library` 68 KB,
  `flow26-integration` 78 KB) — построчные data-flow maps / failure cascades / sprint-карты по каждому
  из 3 сценариев (в проходе 3 прочитан только SYNTHESIS-свод, не тела).
- **Проход 6:** протоколы `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE` +
  `GROK-ORCHESTRATION-EXTRACTION-PROTOCOL-v1` + `GROK-ANALYSIS-AND-LINKING` +
  `EXTRACTION-METHOD.md` (метод экзаменации целиком; куда легли 11 новых правил и 4 negative habits).
- **Проход 7:** `sonnet - analyzes/` пофайловые анализы + `prompt-orchestration/` 6 эталонов.
- **Проход 8:** `sonnet-user-prompts/` + `user prompts/` (корпус «промпт → хорошая оркестрация» для
  API-слоя расширения) + `_tools/`.
- **Проход 9:** выборочно 15–25 крупнейших `Uncategorized`-сессий по `SESSION-INDEX.md` (msg_count
  462/305/279/213/152/145/128), которых нет в Частях 1–35 SESSION-ANALYSIS.

**Честность о покрытии прохода 3:** дословно прочитаны FLOW-31 (340 стр.) и оба scenario-synthesis
(516 стр. суммарно). Остальные 41 FLOW + 3 адаптера прочитаны на уровне H1+ЗАЧЕМ/«Что мы строим» —
достаточно для каталога capability, но НЕ построчно (тела фаз/фабрик FLOW-01..30,32..42 — материал
последующих выборочных проходов). Тела `scenario-*pass-*` (kb-размеры выше) НЕ читались построчно —
это проход 5.

---

## Pass 4/10 — grok orchestrator (Kernel + flows 00-42) + DRAWIO→PLAN + Figma→Canva/Miro/Webflow адаптеры

Прочитано в проходе 4 (папка `claude-sessions/grok  orchestrator/`, двойной пробел в имени, ~58 md + 4 файла
подпапки `grok agent analyze/`): дословно — `00`, `01`, `33`(частично), `20`(частично) `flow orchestrator.md`;
`Architecture plan.md`, `Flow design skills.md`, `module-architecture.md`, `genie-plugin-skill-factory.md`,
`DRAWIO_TO_PROJECT_PLAN_GUIDE.md`, три адаптера `adapt figma to code to canva/mira/webflow*.md`,
`FLOW-PREP-LIBRARY-MASTER-PLAN.md`(частично), `CSS-DEBUG-FIX-PLAN-v1.md`(частично),
`1st round phas 1files.md`(частично), `GROK-ANALYSIS-AND-LINKING-PROTOCOL-v1.md`,
`GROK_PROJECT_FILES_STATE-v1.json`, `prompts.txt`, все 4 файла `grok agent analyze/`.

### 4.1. Что такое «grok orchestrator»: тот же корпус FLOW, другой автор (Grok, не Claude)

**Жанр папки.** Это НЕ отдельный движок «Grok-оркестратора». Это серия сессий модели **Grok**, которой Луба
давала те же самые XIIGen-reference-планы (FLOW-00…FLOW-42, Architecture plan, module-architecture, skill-
factory, figma-адаптеры), что и Claude в проходе 3, и просила одно и то же (файл `prompts.txt`, дословно два
промпта, повторяющиеся во ВСЕХ файлах):

> «Мне нужно чтобы из этих файлов было по человечески описана оркестрация реализации проекта. Запрос который
> решается — есть в инпут файлах. Постепенное описание функционала. Решение тех дизайна. И сами этапы
> реализации» → затем «опиши поэтапно как ты анализировал файлы…».

Поэтому каждый `NN flow orchestrator.md` = **человеческий пересказ Grok-ом** reference-плана этого FLOW +
саморефлексия «как я читал файлы». Это прямое подтверждение вывода прохода 3 (scenario-synthesis, §3.4):
**Grok = human-first narrative, Claude = practitioner depth.** Здесь видно вживую: Grok-описания — это
аналогии (завод-конвейер, дирижёр оркестра, Lego-магазин), таблицы «решение→почему→что даёт», без единой
data-flow-таблицы с типами данных, без счётчиков тестов, без risk-register, без failure-cascade — ровно те
6 «паттернов пробелов» (A–F), которые проход 3 нашёл у Grok. Итог для нас: **эти файлы ценны как «нарративный
слой» того же корпуса** (быстро вводят в смысл каждого FLOW), но НЕ как источник машинных спецификаций —
для типов данных/контрактов надо идти в claude-серию (проход 3) и в scenario-*pass-* (проход 5).

**Важно: нумерация НЕполная.** В grok-папке есть только `00`, `01`, затем `15…42`. **Файлов `02–14` НЕТ**
(в claude-серии прохода 3 были все 42). Подпапка `grok agent analyze/` (4 файла, 2 июня 2026) — это Grok-
классификация всей папки на 6 типов файлов (vision → flow-orchestrator → preparation/implementation → gap/
simulation → протоколы → адаптеры) по протоколу `GROK-ANALYSIS-AND-LINKING-PROTOCOL-v1.md`. Протокол задаёт
«красную нить»: **Vision → Flow Orchestrator → Gap Analysis → Implementation → Learning Loop** и правило: на
каждый файл ответить на 4 вопроса (задача, этапы, связи, как именно связан). `GROK_PROJECT_FILES_STATE-v1.json`
— инвентарь 248 записей (68 dirs, 180 files) исходного xiigen-planning-проекта, из которого папка выросла.

### 4.2. Kernel + bootstrap = «самособирающийся движок» (flow 33 — самое ясное описание)

Файл `33 flow orchestrator.md` (33-system initiation) — **самый чёткий грок-текст про self-building engine**.
Аналогия Grok: «есть огромный план строительства города (7 merged-документов); раньше его вручную переводили
в код, теперь сам город (XIIGen) читает план и строит себя автоматически». Три слоя:

**1) Мини-Kernel (пишется руками ОДИН раз, работает с первого запуска):**
- Core Fabric + DNA-примитивы: `MicroserviceBase`, `DataProcessResult<T>`, `ParseDocument` (Dictionary),
  `BuildSearchFilter`, Scope Isolation (`tenantId`), `DynamicController`.
- Layer-0 Fabrics (тонкие адаптеры к внешнему миру): `IDatabaseService`, `IQueueService`, `IAiProvider`,
  `IRagService`, `IFlowOrchestrator`.
- **Flow Engine** — умеет выполнять JSON-DAG'и, лежащие в Elasticsearch, шаг за шагом.
- Инвариант: как только Kernel запущен, дальше **всё остальное = данные + flows, а не код руками**.

**2) Bootstrap-flow (`platform-bootstrap-v1`) — первый запуск (idempotent + resumable):** проверяет sentinel-
документ «platform bootstrapped»; если его нет — (a) регистрирует базовые event-схемы; (b) грузит **PlanBundle**
(7 merged-файлов: ENGINE_ARCHITECTURE, TASK_TYPES_CATALOG…) → превращает в машинные **реестры** в ES; (c) сеет
**GraphRAG двухслойный: Catalog Graph + Impact Graph** (связи семей↔фабрик↔задач↔навыков); (d) coverage-проверка
(что есть / чего нет) → при gaps запускает **self-extend**; (e) компилирует и публикует основные flows;
(f) smoke-тест простейшего e2e-flow; (g) ставит sentinel → «нормальный режим».

**3) Implement-Family meta-flow (рабочий конвейер само-развития, семьи фабрик параллельно):** один цикл на
семью = загрузка Family Spec + Skill Pack (SK-FAM-xx) → сборка **ContextPack через Hybrid RAG** (сначала
GraphRAG=точные связи, потом VectorRAG=похожий код/тесты/ошибки) → **multi-model генерация** (Claude+GPT+Gemini
параллельно, станции AF-1/AF-5) → **цикл из 5 арбитров** (Coverage, Security, DNA-Compliance=9 правил, Testing,
BFA-Dependencies) — пока все не согласятся → локальный deploy + smoke + regression-impact (GraphRAG говорит,
что перетестировать) → публикация в Implementation Registry + обновление GraphRAG + сохранение feedback. При
провале арбитра feedback → retry (3–10 итераций). Прогресс — в **Progress Ledger** (зеркалится в Jira/Azure
DevOps через F64): WorkSpec, ExecutionRecipe (prompt+модель+RAG), ArbiterJudgment (вердикты+доказательства),
CodeChangeSet, TestReport, GraphChangeSet, SkillDelta, NoCodeReport. **Human Interaction Gate** — нода
`WAITING_FOR_USER` ждёт мнения человека и шлёт уведомление (прямой аналог требования Луба про подтверждения).

**Пример «тяжёлого» FLOW (файл `20`, Sponsored Content + Graph API + рекламная платформа):** Master Execution
Plan → 118 фабрик (F734–F851), 20 task types (T287–T306), 5 flow-шаблонов, 24 BFA-правила, 16 стресс-тестов.
10 фаз P0–P7. Три суб-движка генерации: **INVENTORY** (собирает контекст/паттерны/прошлые FLOW) → **SYNTHESIS**
(AF-1 Genesis + AF-5 multi-model) → **JUDGMENT** (AF-6…AF-9: код-ревью, DNA-паттерны, iron rules, quality
gates). **11 станций AF-1…AF-11** как конвейер. Принципы: Fabric-first, 9 DNA-правил (никаких типизированных
моделей — только Dictionary; всё возвращает `DataProcessResult`), Freedom Machine (квоты/глубина графа/окна
атрибуции — в конфиги), backward-compat (FLOW 1–19 не ломаются), Iron Rules + Quality Gates (нарушил — сборка
падает сразу).

### 4.3. FLOW-00 серия — фундамент Kernel (файл `00`): Naming → Stack Coupling → Cost → Bundle

`00 flow orchestrator.md` описывает 4 подготовительных (infrastructure) потока, идущих ДО пользовательских
FLOW-01…24. Боль: «построили мощный self-developing engine (FLOW-25…35), но он нечитабелен, не multi-stack и
неуправляем по стоимости». Порядок и смысл (это и есть эволюция фундамента):
- **FLOW-00.1 Naming** — доменные имена вместо `flow35-contracts.ts`; `flowId`+`flowName` в контрактах; линтер
  `npm run lint:naming` на CI. → код самодокументируется.
- **FLOW-00.2 Stack Coupling** (самый архитектурный) — 4 уровня связанности `CONCEPT_NEUTRAL / IMPL_VARIES /
  STACK_COUPLED / INCOMPATIBLE`, 10 измерений стека, аннотация `stackCoupling` в каждом контракте+`topology.json`,
  **HybridGenesisPrompt** (5 секций, 1-я всегда нейтральная — один план работает и для NestJS, и для FastAPI),
  навыки SK-431/432/433, Principle 11, аудит T1–T566. → честная поддержка 7×7 стеков.
- **FLOW-00.3 Cost Pipeline** — цепочка `Provider → Dispatcher → CostTracker → AfPipeline → SpendGovernor →
  AF-11(feedback) → ExecutionLog`; каждый запуск пишет `cost_usd/tokens_in/out/model_id`, проверяет бюджет
  перед каждой AI-станцией; SK-425 выбирает модель по `value_ratio` (качество/цена). → AI-пайплайн перестаёт
  быть «чёрным ящиком» по деньгам.
- **FLOW-00 Bundle Activation** — Solution Bundle (B-001 «B2B Marketplace», B-002 «Community Platform»…); при
  активации BundleValidator → для каждого requiredFlow BootstrapOrchestrator (DRY_RUN→FULL) → предзаполняет
  FREEDOM config → атомарная фиксация в flow-lifecycle. → tenant получает готовый продукт «из коробки».

Жёсткие gate после каждой фазы: `npm run build + npm test + npm run lint:naming` + SK-418 checklist (31 пункт)
+ **⛔ STOP + явное "yes" от Луба** (тот же human-gate). `01 flow orchestrator.md` (FLOW-01 регистрация
разработчиков T47/T48/T49) — первый **production-flow, которым движок строит сам себя (dogfooding)**: mechanism-
first, fabric-интерфейсы (`IScopedMemoryService`, `ISchedulerService`, `ITenantRegistry`), 7 фаз (Gate→INJECT→
GENERATE→ITERATE→INTEGRATE→PROMOTE→CROSS-FLOW), а каждый task type по завершении автоматически даёт **DPO-тройки
+ distilled rules + calibration data** (self-reinforcing loop: движок строит продукт → тут же учится).

### 4.4. Само-строящийся слой: FLOW-DESIGN SKILLS + genie-dna composition + Skill Factory

Три файла раскрывают **как движок заставляют строить себя ПРАВИЛЬНО** (а не сваливаться в ручной код):

**`Flow design skills.md` (FLOW-DESIGN SKILLS v1.0, 6 навыков).** Боль: в FLOW-37/38 (v1 и v2) все capabilities
превращались в статические `.ts`-сервисы, которые обходили AF-9, не копили DPO, не учили движок → смысл self-
improving терялся. Защитный слой из 3 идей: (1) **Bootstrap Boundary** — исчерпывающий список того, что ещё
можно писать руками (остальное — только flow topology через AF-пайплайн); (2) **Flow Design Cycle** (5 стадий:
Gap Identified → Topology Designed → Generated & Judged → Runs & Records OUTCOME → Signals Promote); (3) **Self-
Questioning + Learning Signals** — любая AI-генерация дизайна ОБЯЗАНА задать себе 3+ вопроса об ошибках
(`selfQuestions[]/answers[]`), а каждый topology ОБЯЗАН заканчиваться `feedback.handler` с ≥1 `learning_signals[]`
(OUTCOME обязателен). Ключевой новый гейт `flow-vs-service-gate` (load_order 0.5) — 4-вопросный тест на каждую
capability, пишет `capabilityRouting[]` в STATE.json. После 10–15 потоков движок сам проектирует новые flows
лучше; FLOW-39 = meta-flow-design engine, который уже сам проектирует flows.

**`module-architecture.md` (XIIGen V18, genie-dna composition layer).** Одна фраза-стержень (The One Rule):
**«Ты строишь не 15 систем, а 1 универсальную машину + 15 наборов конфигов (JSON в Elasticsearch)».** Аналогия:
завод-конвейер один, под каждую модель машины меняешь инструкцию. 20 модулей → 5 супер-блоков (Display /
Transaction / Creator / Communication / Financial). Один модуль (напр. `Availability`) ведёт себя по-разному
через **Strategy Factory** (`config["strategy"]` = `date_range_overlap` для отелей / `time_slot` для курсов /
`quantity` для краудфандинга) — **никаких `if(siteType=="hotels")`**. 3-слойка Editor→Storage→Machine (Layer-3
машина НИКОГДА не знает тип сайта), 4 закона Genie DNA, MACHINE vs FREEDOM. Прямо релевантно расширению: этап
E4 создаёт `.cursorrules` + `.github/copilot-instructions.md` + `AGENT_QUICKSTART.md`, **чтобы Claude Code /
Cursor / Copilot автоматически читали DNA-правила перед генерацией кода** — то есть multi-host расширение — это
тонкий агент, читающий общий свод правил ядра.

**`genie-plugin-skill-factory.md` (Skill Factory).** Из одного работающего WordPress-плагина извлекли **10
универсальных reusable паттернов** в виде самодостаточных `SKILL.md`, чтобы любой AI (Claude/Cursor/Grok/Gemini)
генерировал похожие плагины для любой CMS без переписывания boilerplate. Ключевые решения, **прямо ложащиеся на
тонкий хост-адаптер**: (1) **CMSAdapter — единственная точка касания CMS**: весь WordPress-специфичный код
(`wpdb`, `wp_mail`, `get_option`) в одном классе за интерфейсом `ICMSAdapter`; сменить WordPress→Laravel/Joomla
= заменить один файл; (2) **MACHINE vs FREEDOM**; (3) **Regeneration Prompt в каждом SKILL.md** — любой AI в
любой момент восстанавливает скилл с нуля даже при потере контекста; (4) строгий Dependency Graph 01→10 (каждый
скилл зависит только от предыдущих). Мета-вывод: генерацию Phase 6 надо делать **через саму фабрику** — само-
оркестрирующийся процесс.

### 4.5. DRAWIO → PROJECT PLAN: 5-этапный мост «UML-диаграмма → план кода»

`DRAWIO_TO_PROJECT_PLAN_GUIDE.md` — методология превращения draw.io/UML в исполняемый план (на примере Small
Business Networking App из 8 sequence-диаграмм). **5-stage pipeline:**
1. **PARSE** — из каждой диаграммы вытаскиваем lifelines (сервисы), messages (вызовы), events (Publish/Consume),
   conditions (alt/loop/par).
2. **MAP** — каждую lifeline → существующий V17-skill ИЛИ «нужен новый» (напр. Feed Service → Skill 46).
3. **LAYER** — раскладка по слоям зависимости (нельзя строить Feed, пока нет Connection и Post; Layer 0–5).
4. **PLAN** — фазы с шагами и **save points** (точки сохранения состояния для resume).
5. **GENERATE** — по каждому шагу генерим код через промпты (читаем `SKILL.md → implement.md` → кастомизируем
   под конкретные события диаграммы).

Пример оркестрации одного flow (Friend Request): диаграмма → **Flow Definition (JSON-DAG)**: nodes
`CreateFriendRequest → Publish FriendRequestSent → parallel(GroupWeight, EventWeight, QuestionnaireWeight) →
WeightCalculation → FeedIntegration`, edges+conditions, event-wiring (Connection публикует `FriendRequestAccepted`;
Matching/Feed/Notification/Analytics подписываются), а данные хранятся как **динамические документы**
(`Dictionary<string,object>`, `weights:{group:0.6,event:0.3,questionnaire:0.8}`) без изменения схемы БД.
Вывод методологии: **архитектуру не изобретаем заново — Genie DNA уже спроектирована под event-driven + doc-
based + score-calculated**; надо лишь добавить доменные skills в правильном порядке. Приоритет строим по частоте
появления сервиса в диаграммах (Feed/Matching/Analytics — в 7 из 8 → Phase 1). Это прямой парный аналог связки
FLOW-31 (design→IR→system-model) из прохода 3, но со стороны UML-входа, а не Figma-входа.

### 4.6. Figma → код → Canva / Miro / Webflow / Framer: **паттерн адаптера поверх общего ядра**

Это самый важный для нашего расширения материал: семья из 4 адаптеров (**C5 Canva, F1 Framer, W1 Webflow,
M1 Miro**), которые **переиспользуют ОДНО и то же ядро** — CSS-движок из плагина **FigmaToCode** (`styles.ts`
= 18-этапный пайплайн, `element-code.ts`), **скопированный без единого изменения** (жёсткое правило: файлы
пайплайна трогать нельзя). Канонический промежуточный формат — **«фигмообразный узел» (`SceneNode`)**. Задача
любого адаптера сводится к 5 частям: (a) `docs/FIGMA-<HOST>-PROPERTY-MAP.md` — таблица соответствия свойств;
(b) адаптер, строящий `SceneNode` из формата хоста; (c) storage (SDK хоста вместо `localStorage`); (d) writer
(write-back в хост); (e) React-UI с табами. Ядро (18-шаговый пайплайн → CSS) общее.

**Таблица сложности (что читаем из хоста → сложность адаптера → новизна):**

| Порт | Что читаем из хоста | Ядро адаптера | Сложность | Сессий |
|------|---------------------|---------------|-----------|--------|
| **C5 Canva** | только текст (runs с разными стилями) | `normalizeTextRuns()` → доминирующий стиль; `font-weight-map`; `buildFigmaLikeNode()` | **самый лёгкий** (тренировочный порт) | 5 |
| **F1 Framer** | текст + CSS-строки | парсинг CSS-строк | средняя | — |
| **W1 Webflow** | полный layout + текст (у Webflow **уже настоящий CSS**) | `css-parser.ts` (шортхэнды→свойства), `webflow-adapter.ts` (рекурсивный обход, 22 свойства) + **Diff-view: наш CSS vs нативный CSS Webflow** (уникальная валидация точности пайплайна) | средняя | 5 |
| **M1 Miro** | фигуры на бесконечном холсте (x/y, размер, цвет, форма) | **новый «мозг»**: `shape-classifier.ts` (10 правил R1–R10 + confidence 0–1: прямоугольник с рамкой+текст→`<div class=card>`, синий с «Купить»→`<button>`), `spatial-analyzer.ts` (кто внутри кого, ряд/колонка/абсолют, отступы, gap → flex-инференс), `border-radius-map`, `html-content-parser` | **самая сложная** (Miro не знает про «карточку»/«кнопку»/«флекс» — структуру надо угадывать по косвенным признакам) | 6 |

Каждый план — Session 0 (12 проверок FC-1…FC-12) + 5–6 рабочих сессий, **после каждой ⛔ STOP + одобрение
человека**; финал — ZIP `<host>-adapter-FINAL.zip` + `STATE.json → COMPLETE`. Все четыре на момент архива были
`current_session: 0` (план готов, ждёт старта Session 1).

**Реальное общее ядро (FigmaToCode) — файлы `1st round phas 1files.md` + `CSS-DEBUG-FIX-PLAN-v1.md`.** Плагин
работает в **двух изолированных контекстах Figma Plugin API**: (1) **Plugin Sandbox** (`src/plugin/`) — есть
`figma.*`, НЕТ DOM/fetch/React; тут `scanElements()` (рекурсивный обход дерева) → `getStyleForElement()` в
`styles.ts` (~1150 строк, **18-этапный пайплайн извлечения стилей**) → `prepareCodeBaseForElement()`/
`prepareCssForElement()` в `element-code.ts` (генерация HTML/CSS-строк); (2) **UI Iframe** (`src/ui/`) — React
17, есть DOM/fetch/Canvas, **5 табов: Preview / Code / HTML Preview / Tree / AI Enhance**. Реальные баги
пайплайна (важны как «грабли» для нашего design-to-code): текст → `<div>` вместо `<p>`, картинки → невалидный
`<img></img>`, дублирование CSS-свойств, `margin` не превращался в `padding` у инпутов, наслоение `!important`,
неверные цвета drop-shadow. Отдельная драма — **K-Means классификатор layout'ов в `BuildFlex`**: он слишком
агрессивно ставил `display:flex/grid` на root-экраны (`parent.id==='0:1'`) и 2-child контейнеры (r²=0), bypass-
guards слепо отключали позиционирование даже у explicit ABSOLUTE детей; порядок починки строго: сначала
`AddSetting` (убрать дубли position → чистый debug) → root guard → confidence thresholds (сделать классификатор
«тише») → умные bypass → только потом `flex:true` для FLEX_COL. Это конкретный пример «детерминированный слой
отделён от AI-слоя» и того, что **обучаемый классификатор нельзя делать финальной властью без guard-калибровки**
(перекликается с R4 no-hidden-determinism и с order-leakage-guard из наших правил).

### 4.7. Выводы прохода 4 для плана расширения VS / VS Code

1. **Паттерн адаптера Figma→Canva/Miro/Webflow/Framer — готовый blueprint нашего «тонкого хост-адаптера поверх
   `llm_mvp_core`».** Одно каноническое ядро (`styles.ts` 18-шагов) копируется без изменений; каждый хост даёт
   лишь property-map + adapter(`→SceneNode`) + storage + writer + UI. Для VS/VSCode это значит: **общий design-
   to-code core в `llm_mvp_core`, а VS и VSCode — ещё два адаптера, а не форк ядра** (совпадает с R6 «тонкий
   агент/ядро» и с выводом прохода 3 про экосистему адаптеров FLOW-34/41/42).
2. **`SceneNode` = канонический промежуточный IR.** Любой источник дизайна (Figma, Canva-runs, Miro-фигуры,
   Webflow-CSS, draw.io-UML) нормализуется в один IR, а дальше работает общий движок. Это тот же 3-слойный IR,
   что FLOW-31 в проходе 3 (структура→семантика→системная модель).
3. **M1 Miro `shape-classifier` (R1–R10 + confidence) — эталон «угадывания структуры из голых координат»** для
   случаев, когда вход беден (скриншот/эскиз без CSS). Confidence-score + human-review при низкой уверенности =
   наш обязательный human-gate. K-Means-история из CSS-DEBUG — предупреждение: **обучаемый layout-классификатор
   должен быть guarded (root guard, confidence thresholds), а не финальной властью.**
4. **DRAWIO→PLAN 5-stage (Parse→Map→Layer→Plan→Generate) + save points** — методология для UML/диаграммного
   входа расширения; напрямую даёт «Flow Definition (JSON-DAG)» из диаграммы. Пара к Figma-входу.
5. **Kernel/bootstrap self-building (flow 33)** — если расширение должно уметь «дорастать» (self-extend при
   gaps), берём модель: мини-Kernel руками, дальше всё = данные+flows; Hybrid RAG (GraphRAG+VectorRAG) в
   ContextPack; 5 арбитров; Progress Ledger; `WAITING_FOR_USER` human-gate.
6. **Multi-host авто-чтение правил (module-architecture E4):** `.cursorrules` + `copilot-instructions.md` +
   `AGENT_QUICKSTART.md` — механизм, которым Claude Code/Cursor/Copilot тянут DNA-правила ядра. Для нашего
   расширения — способ держать ядро тонким, а правила — в одном своде.
7. **Skill-Factory + Regeneration Prompt (genie-plugin-skill-factory):** CMSAdapter-паттерн (одна точка касания
   хоста за интерфейсом) — прямой аналог «одного host-adapter файла» на VS/VSCode; regeneration-prompt в каждом
   skill = устойчивость к потере контекста.

### 4.8. Честность о покрытии прохода 4 + что остаётся для проходов 5–9

**Честность о покрытии.** Дословно прочитаны `00`/`01` flow (эволюция FLOW-00 серии + FLOW-01), три figma-
адаптера целиком, `DRAWIO_TO_PROJECT_PLAN_GUIDE`, `Architecture plan`, `Flow design skills`,
`module-architecture`, `genie-plugin-skill-factory`, все 4 `grok agent analyze/`, протокол+state+prompts.
Файлы `33`/`20` flow, `FLOW-PREP-LIBRARY-MASTER-PLAN`, `CSS-DEBUG-FIX-PLAN`, `1st round phas 1files` прочитаны
**частично** (первые 45–80 строк) — достаточно для сути Kernel/bootstrap, self-building и общего FigmaToCode-
ядра, но НЕ построчно. **Grok-flow `02–14` в папке отсутствуют** (не пропуск чтения — их физически нет).
НЕ читались построчно (материал будущих проходов): grok-flow `15–19, 21–32, 34–42` (каждый ~13–92 KB — грок-
нарративы тех же FLOW, ценность = нарративный слой, не спецификация), а также `FLOW-01-02 plan files.md`,
`Flow implementation.md`, `Flow preparation.md`, `architecture gap.md`, `documentation files changes plan.md`,
`learning-pipeline-test-plan-v2.md`, `ui-e2e-test-plan-v2.md`, `flow-mobility-auth-package-v2.md`,
`XIIGEN_NESTJS_REACT_TRANSLATION.md`, `V17 plan package.md/.txt`, `xiigen docs.md/.txt`,
`agent-output-format-skill for webse.md`.

**Что остаётся для проходов 5–9 (без изменений относительно §3.6, уточнено):**
- **Проход 5:** тела `scenario-10/20-pass-*` (`figma-to-code` 84 KB, `csharp-skill-library` 68 KB,
  `flow26-integration` 78 KB) — построчные data-flow maps / failure cascades / sprint-карты по 3 сценариям
  (в проходе 3 читался только SYNTHESIS-свод; тела адаптеров и claude-flow тел — там же).
- **Проход 6:** протоколы `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE` +
  `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1` + `AGENT-INSTRUCTIONS.md` + метод экзаменации
  (куда легли 11 правил / 4 negative habits из прохода 3).
- **Проход 7:** `sonnet - analyzes/` пофайловые анализы + `prompt-orchestration/` эталоны +
  `samples of orchestarion examinations.md`.
- **Проход 8:** `sonnet-user-prompts/` + `user prompts/` (корпус «промпт → хорошая оркестрация» для API-слоя
  расширения) + `_tools/` + `build_session_json.py`.
- **Проход 9:** выборочно крупнейшие `Uncategorized`-сессии по `SESSION-INDEX.md` (msg_count 462/305/279/…),
  которых нет в Частях 1–35 `SESSION-ANALYSIS`; при остатке бюджета — построчное чтение самых важных grok-flow
  (`33`/`20`) и `FLOW-PREP-LIBRARY-MASTER-PLAN` целиком.

---

## Pass 5/10 — scenario multi-pass examinations (figma-to-code / csharp-skill-library / flow-26)

**Что прочитано в этом проходе (построчно, целиком):** три больших тела 20-проходной экзаменации в
`claude orchestrator/`:
- `scenario-20pass-figma-to-code.md` — 1462 строки, ~86 KB;
- `scenario-20pass-csharp-skill-library.md` — 1108 строк, ~70 KB;
- `scenario-20pass-flow26-integration.md` — 1130 строк, ~80 KB.

В проходе 3 читался только `scenario-20pass-SYNTHESIS.md` (свод метода Claude-vs-Grok). Здесь прочитаны сами
ТЕЛА — то есть глубочайший рабочий пример метода экзаменации, применённого к трём точным доменам расширения:
**Figma→код** (design-to-code), **skill-library для нескольких AI-хостов**, и **само-строящийся движок**.
Рядом лежат укороченные `scenario-10pass-*` (проходы 1–10) и `scenario-10pass-COMPARISON-ANALYSIS.md`; тела
20-проходов инкорпорируют 10-проходные внутрь (Раздел I) и добавляют Раздел II (проходы 11–20).

### 5.0. Что такое «20 проходов»: структура метода (одинаковая во всех трёх файлах)

Метод — это НЕ 20 пересказов одного и того же, а **20 разных ИЗМЕРЕНИЙ (dimensions)** одной сессии. Каждый
проход берёт готовый разбор Grok (компактное «что было») как baseline и добавляет один слой инженерной
глубины. Файл делится надвое:

- **Раздел I — проходы 1–10 (базовая аналитика):** 1 Gap Audit (что Grok упустил, 10–13 категорий) · 2 Planning
  Layer (что вызвало сессию, планировочные ворота) · 3 Functional (что делает каждый компонент) · 4 User Journey
  (пошагово, что видел пользователь) · 5 E2E Coverage (что тестировалось / почему тестов нет) · 6 Before/After
  (фундаментальный сдвиг) · 7 Save Points (STATE JSON + resumability) · 8 Design Rationale (глубокое «почему») ·
  9 Cross-Session Deps (зависимости между сессиями) · 10 Lasting Impact (5 установленных паттернов).
- **Раздел II — проходы 11–20 (углублённая аналитика):** Data Flow Map · Failure Mode · Performance/Scale ·
  Security · Testing Hierarchy · Sprint Plan · Risk Register · Roads Not Taken · Protocol Contribution ·
  Narrative Position — ИЛИ доменно-специфичные аналоги (для flow-26: State Machine · Evidence Bundle · AF-11
  Stations · Tenant Isolation · Recovery · Operational Runbook · Self-Build vs Manual · Engine Limits).

Каждый проход заканчивается ЛОГОМ и обновлённым сравнением с Grok. **Ключевой инвариант метода:** ценность
растёт от «что произошло» (Grok, ~140 строк) → «как связано» (10 проходов, ~290 строк) → «что именно
реализовать и как эксплуатировать» (20 проходов, ~640 строк). Для нашего плана это и есть готовый шаблон
«как разворачивать один сценарий расширения в исполнимую спецификацию».

### 5.1. Сценарий A — figma-to-code (design-to-code Foundation) — что экзаменует + deliverable

**Что за сессия:** 05.02.2026, транскрипт `AI-powered Figma to code generation platform architecture.md`
(~385 KB, 6 сообщений). Пользователь сказал `"I would like to create a skills set for my new project"` —
первый переход «проектировать → реализовывать». **Deliverable сессии:** первый живой skill
`generic-core-interfaces` (реальный C# код MicroserviceBase на все 19 компонентов Architecture.docx) +
28-skill implementation plan (6 слоёв) + стратегическое решение **polyglot**.

**Что экзаменует многопроходный разбор (pass-by-pass, самое ценное для расширения):**
- **P3 Functional / DNA-основа:** `generic-core-interfaces` = ДНК всей платформы. Каждый из 19 компонентов =
  C# интерфейс: `IDatabaseService` (generic CRUD по любой БД), `IQueueService`, `IAiProvider` (generic
  prompt→completion), `IRagService`, `IFlowOrchestrator`, `IPermissionsService`, `ISecurityService`,
  `INotificationService`, `IReportService`; `MicroserviceBase<T>` инкапсулирует все 19 (mandatory DI);
  `DataProcessResult<T>` — унифицированный Success/Error/NotFound, **никаких exceptions** (нарушение = балл 0).
  Динамические документы = `object`/`Dictionary<string,object>`; `BuildSearchFilter()` = запрос только по
  непустым полям.
- **P2 Planning-gate «три пути»:** Specifications-first (рекоменд.) / Complete single-stack / Full multi-stack —
  формальная точка ветвления, сессия закончилась open question (это правильный исход стратегической сессии).
- **Polyglot spec (P3/P8):** каждый skill получает структуру `spec.json` (JSON Schema, языконезависимый
  контракт) + `openapi.yaml` + `implementations/{dotnet,nodejs,rust}/` + `adapters/{elasticsearch,redis,kafka,
  aws-sqs}/`. Это превращает XIIGen из .NET-инструмента в **платформу для любого стека** — и это тот же
  «тонкий адаптер поверх общего ядра», что и Figma→Canva/Miro адаптеры из прохода 4.
- **P11 Data Flow Map:** точные IN/OUT/промежуточные формы для каждого интерфейса. Инвариант: данные нигде не
  «текут» сырыми через слои — каждая граница обёрнута `DataProcessResult<T>`; `object` остаётся `object`
  внутри DB/Queue и десериализуется только на краях (executor/handler).
- **P12 Failure Cascade:** для каждого сервиса — каскад БЕЗ `DataProcessResult` (throw → worker crash →
  потеря задачи → flow навсегда «Running») ПРОТИВ каскада С ним (Error/RateLimited статус → StepFailed →
  ретрай с того же шага). Вывод: `DataProcessResult<T>` — необходимое, но НЕ достаточное; полная надёжность =
  + Outbox + Idempotency + Saga + Circuit Breaker (Layer 2-3). `RateLimited` — отдельный статус (поэтому
  Status = enum, не bool): защищает от денежных дублей AI-вызовов.
- **P13 Performance:** главный bottleneck — AI-вызовы (2–30 c); весь остальной стек +50–200 мс. Оптимизация
  DB/Queue даёт <1% E2E; ключевая оптимизация — параллелизация AI (fan-out в `IAiProvider`).
- **P14 Security:** tenant-изоляция ДЕКЛАРИРОВАНА, но не enforced на уровне `IDatabaseService`/`IQueueService`
  (`index`/`queueName` — строки → cross-tenant риск). 3 атаки: Index Enumeration, Dynamic Document Injection,
  Non-Empty Filter Bypass. Error-message leakage: `ex.Message` может утечь путь/SQL/ES-query → нужен generic
  message+code.
- **P15 Testing pyramid:** Unit (DataProcessResult/BuildSearchFilter, ~50) → Contract (абстрактный
  `IDatabaseServiceContractTests<TImpl>`, ~200, принудительный Liskov) → Integration (Flow+ES+Redis, ~100) →
  E2E (полный flow с AI, ~20, $5-10/run) → Polyglot contract (spec.json валидирует output каждого языка, ~50).
  Итого ~420 тестов, ~$10-15/CI, ~40 мин.
- **P16 Sprint Plan:** 9 спринтов (18 недель), критический путь Layer 1→2→Auth→Gateway→Orchestrator→Executors;
  MVP (первый рабочий flow) — конец спринта 5 (~10 недель).
- **P17 Risk Register:** Polyglot drift (56/100), DataProcessResult evolution breaking-change (50), AI Cost
  Explosion (48), Foundation lock-in (30), Session handoff потеря polyglot-решения (40).
- **P18 Roads Not Taken:** .NET-only (TAM ~15-20%), Exceptions/Result<T>/OneOf вместо DataProcessResult,
  GraphQL вместо REST, Event Sourcing, Orleans actor model — почему каждый отклонён.
- **P19 Protocol Contribution:** новые правила FOUNDATION-SESSIONS (Pass/Fail критерии), POLYGLOT-GATE,
  DATA-FLOW-CONTRACT, FAILURE-CASCADE-ANALYSIS, TENANT-ISOLATION-CHECKPOINT.

**Итоговый deliverable разбора A:** полная инженерная спецификация design-to-code фундамента — DNA-контракты +
polyglot-структура + data-flow + failure-модель + пирамида тестов + 18-недельный план. Именно этот слой
питает FLOW-26 (REUSE-BEFORE-GENERATE сканирует эти skills).

### 5.2. Сценарий B — csharp-skill-library (multi-host skill library) — ⭐ самый прямой для расширения

**Что за сессия:** 05.02.2026, транскрипт `XIIGen C# skills implementation.md` (1.43 MB, 22 сообщения).
**Deliverable сессии — это буквально образец «расширение для нескольких AI-хостов»:**
- **4 файла Skill Library, по одному на AI-хост** (каждый оптимизирован под парсинг конкретного инструмента):
  `CLAUDE.md` (Claude Code, Markdown+XML-теги `<project>/<rules>/<skills>/<forbidden>`) · `.cursorrules`
  (Cursor, простой текст, правила в первых 100 строках) · `copilot-instructions.md` (GitHub Copilot,
  Markdown+комментарии) · `.clinerules` (Cline, YAML-like system prompt). **Один DNA-паттерн описан в 4
  форматах, потому что каждый хост парсит конфиг по-своему — один файл на всех = потеря части правил.**
  → Это прямой аналог того, что наш план расширения VS/VS Code должен сделать: тонкие host-адаптеры
  (Claude/Cursor/Copilot/Cline/VS Code) поверх ОДНОГО общего ядра-контекста.
- **`PROMPTS.md` — 10-фазная self-contained цепочка промптов** (самый ценный долгосрочный актив). Логика 10 фаз
  = 6 слоёв × «каждая фаза = завершённый deployable increment». Структура каждого промпта: `CONTEXT (load
  before starting)` → `WHAT TO BUILD` → `DNA COMPLIANCE CHECKLIST` → `TESTS` → `DONE WHEN`. Каждая фаза
  исполнима БЕЗ знания истории проекта.
- **`context/interfaces.cs` — «world model» для AI** (все интерфейсы + base classes + DTO + enum в одном файле;
  загрузка = полная замена «объяснять проект с нуля»).
- **React Demo (3 вкладки Chat/Flows/Skills)** — одноразовый UX proof для stakeholder buy-in.
- **v14 package = 112 файлов.**

**Что экзаменует многопроходный разбор (pass-by-pass, ядро для расширения):**
- **P11 Prompt Quality — формальная 7-компонентная система оценки промпта:** Load context · Scope · DNA
  checklist · Interface contracts · Dependencies · Done-when criteria · Negative examples. Скоринг: A
  (исполнимость без истории, 0–3) · B (DNA enforcement, 0–2) · C (Testability, 0–2). Минимально приемлемо
  A≥2,B≥1,C≥1. → Это готовый рубрик-скоринг для нашего «промпт→оркестрация» API-слоя (пригодится проходу 8).
- **P12 Drift Risk — governance для `interfaces.cs`:** каскад при stale-контексте (AI генерит по старому
  интерфейсу → CS0117 → confusion → дорогой откат). 3 категории расхождений: Breaking (компиляция ловит) /
  **Silent** (семантика изменилась, самое опасное — компилируется, тесты зелёные, поведение неверное) /
  Additive (не ловится, но безвредно). 4-уровневый governance: PR-checklist → CI Roslyn-diff → AI-assisted
  «compare before proceeding» → версионирование (`// Version: v14.2`).
- **P13 Context Window Economics — количественное обоснование tool-routing:** Phase 4 ≈ 6-8K токенов промпт +
  8K код ≈ 16-20K/сессия. Помещается: Claude/GPT-4o — да; Cursor (16-32K) — риск; Copilot inline (~8K) — нет.
  Митигации M1 Phase splitting · M2 Context summary (только релевантная секция interfaces.cs) · M3 Tool routing
  (сложные фазы 6-7 → Claude; шаблонные 2-3 → Copilot) · M4 Progressive loading. → **Прямое правило для
  нашего расширения:** какой хост какие задачи тянет по размеру контекста.
- **P14 Multi-AI Coordination:** PROMPTS.md молча предполагает ПОСЛЕДОВАТЕЛЬНОЕ выполнение; при параллельной
  работе (Claude Phase 2 + Cursor Phase 3) — merge-конфликты в `interfaces.cs`/`Program.cs`/`appsettings.json`.
  Нужны PARALLELISM RULES + git flow (одна активная Phase на разработчика).
- **P15 React Demo per-tab:** для каждой вкладки — данные / взаимодействие / что нужно от реального backend /
  что валидирует. Вывод: demo = отличный stakeholder-tool и UX-гипотеза, но НЕ E2E-тест (mock data, нулевая
  production-готовность).
- **P16 v14 Package — реконструкция 112 файлов по 7 категориям:** A Skill Library (4) · B Canonical Context
  (2) · C PROMPTS Chain (11) · D Architecture Refs (~6) · E React Demo (~30) · F Starter Scaffolds (~45) ·
  G Test Templates (~14). Ядро = ~17 файлов (A+B+C), остальное вспомогательное.
- **P17 Executable Spec — эволюция PROMPTS.md → FLOW-SPEC.yaml:** YAML-заголовки (`phase/depends_on/produces/
  context_required/ai_recommended/estimated_tokens`) + machine-readable DNA checks (JSON) + верифицируемые
  `done_when` (compilation/unit_tests/interface_check команды). Тогда FLOW-26 сможет авто-парсить и
  авто-генерировать skill-stubs.
- **P18 Skill Library vs RAG (7 параметров):** Skill Library = высокая предсказуемость/низкий setup/высокая
  cross-tool поддержка, но высокий drift-риск и грубая гранулярность. Переходный порог на RAG: interfaces.cs
  > 5000 строк ИЛИ реализован codebase-RAG. Для состояния проекта 05.02.2026 Skill Library — однозначно верный
  выбор. → **Наш вывод:** расширение стартует со Skill-Library-слоя, эволюционирует в hybrid (статические
  правила + динамический RAG-контекст, что у нас = RAG-06 SIMULATION-INVARIANTS + локальная модель).
- **P19 META-ORCHESTRATION session type:** сессия производит ИНФРАСТРУКТУРУ для реализации, а не feature;
  её ценность = «multiplier factor» (сколько будущих сессий ускоряет). Deferred E2E (валидация в downstream-
  сессии). Governance-gaps как обязательный раздел.
- **P20 Prompt Evolution:** V14→V20→V35→V50, семантическое версионирование Phase-промптов
  (`Since/Deprecated/Reason/Migration`); долгосрочно PROMPTS.md → авто-генерируемый FLOW-26.

**Итоговый deliverable разбора B:** структура skill-library для мульти-хостового расширения + рубрика качества
промпта + экономика контекста и tool-routing + governance против дрейфа контекста. **Это самый прямой
input в план расширения VS/VS Code** — хосты (Claude/Cursor/Copilot/Cline) и слой конфигов совпадают
буквально.

### 5.3. Сценарий C — flow-26 self-building (само-строящийся движок) — что экзаменует + deliverable

**Что за сессия:** 01.03.2026, интеграционная — вливание определений FLOW-26 в 7 канонических merged-документов.
**Deliverable сессии:** 28 новых фабрик (F1075–F1102) в 6 семействах (154–159), обновлены 5 из 7 канонических
файлов (2 — `MASTER_EXECUTION_PLAN` и `SESSION_STATE` — НЕ завершены = 5/7 partial state). Ключевая
планировочная задача — **проблема офсета +93 фабрики** (standalone-файлы писались от baseline F982
post-FLOW-22, канон достиг F1074 post-FLOW-25) — была АНТИЦИПИРОВАНА в `26-FLOW26_MERGE_PLAN.md`, а не
обнаружена в процессе.

**Что экзаменует многопроходный разбор — это полная спецификация «цикла само-построения»:**
- **6 семейств / 28 фабрик (P3):** 154 Gap Detection (F1075–1081: детект+классификация gap, reuse-scan) ·
  155 Contract & Artifact Generator (F1082–1087: фабрика-спека, task-type контракты, BFA draft ДО кода) ·
  156 Genesis & Validation (F1088–1092: 11 AF-станций, DNA-чек, security, bounded fix-loops) · 157 Sandbox &
  E2E (F1093–1097: изолированный деплой, E2E с ОРИГИНАЛЬНЫМ запросом, Evidence Bundle) · 158 GitOps
  Assimilation (F1098–1101: branch+PR+CI, дважды зовёт FLOW-25, обновление registry) · 159 Promotion Ladder &
  Human Gate (F1102: 6-стадийная state-machine DRAFT→CORE; CORE требует человека).
- **P12 SelfBuildRun State Machine (6 стадий):** `DRAFT → WIRED → VALIDATED → INJECTED → MINIMAL → CORE`. Для
  каждой стадии: триггер входа, условие выхода, failure-route, ответственный компонент, оценка времени (DRAFT
  3-8мин … VALIDATED 15-45мин с fix-loop max 5 … CORE — ожидание человека НЕограничено). Всё персистится в ES,
  идемпотентно (рестарт resume с последнего состояния).
- **P13 Evidence Bundle — 12 секций (иммутабельный «единый источник правды для промоушена»):** factorySpec ·
  taskTypeContract · eventSchemas · BFA draft · codeBundle · unitTests · e2eTests · DNA matrix · securityScan ·
  sandboxLog · flowTemplate · promotionRecommendation. Каждая секция: что содержит / кто производит / когда
  становится иммутабельной / авто-валидация. При seal() — SHA-256 хэш, любая правка = BUNDLE_INTEGRITY_
  VIOLATION + SECURITY_ALERT.
- **P14 AF Pipeline — 11 станций (строгий порядок, ни одну нельзя пропустить):** AF-1 Intent Decomposition ·
  AF-2 Context Assembly (гибридный GraphRAG+VectorRAG, **REUSE_BEFORE_GENERATE = machine rule**, reuse-решение
  COPY/ADAPT/REWRITE обязательно записано) · AF-3 Multi-Model Generation (все AI через `IAiProvider`, никаких
  прямых импортов провайдера) · AF-4 Structural Validation (нет `.Result/.Wait()`, namespace, полная реализация
  интерфейса) · AF-5 Test Generation (mock fabric, 100% перехват) · AF-6 DNA Full Scan (9 паттернов, binary) ·
  AF-7 Security Scan (0 HIGH/CRITICAL) · AF-8 BFA Alignment (детект code-drift событий) · AF-9 Integration Test
  (E2E из ОРИГИНАЛЬНОГО запроса, не синтетика) · AF-10 Flow Template Validation · AF-11 Feedback Loop Closure
  (финальный вердикт PROCEED/HALT).
- **P15 Tenant Isolation:** что изолировано per-tenant (sandbox namespace `self-build/{tenantId}/{runId}`,
  Evidence Bundle, SelfBuildRun record, GitOps branch, Redis-каналы, RAG-контекст) vs что общее (7 канонов,
  AF-логика, shared skill-паттерны, CORE-registry). Дедупликация одновременных CapabilityGapDetected по
  `(capabilitySignature, phase)`.
- **P11 Canonical Governance:** каноны = «конституция, не вики» (нельзя half-amend); каждый апдейт — designated
  owner + декларация scope до открытия файлов; атомарность на уровне 7 файлов; `SESSION_STATE_MERGE` = commit-
  gate (обновляется последним). 5-check валидация (offset/count/BFA/skill-coverage/DR-uniqueness). FLOW-26
  провалил checks 1,2,5 → отсюда governance-протокол.
- **P16 Recovery Plan** (пошагово чинит 2 незавершённых файла, 140-180 мин), **P17 Operational Runbook**
  (5 monitoring points + alert thresholds + restart + rollback bad CORE promotion), **P18 Self-Build vs Manual**
  (матрица 9 измерений + hybrid: генерить FLOW-26, стоп на INJECTED, человек ревьюит, ручной GitOps),
  **P20 Engine Limits** (7 ограничений: не может менять существующие фабрики / добавлять fabric-интерфейсы /
  менять DNA / cross-cutting / учиться из прошлых прогонов / параллелить один capability / делегировать human
  gate; + roadmap FLOW-26→26b→26c→FLOW-35→полная автономия).

**Итоговый deliverable разбора C:** полный «self-building loop» на уровне эксплуатации — state-machine +
Evidence Bundle + AF-11 + governance + runbook + пределы. Human-gate at CORE = IRON RULE (не bypassable
конфигом), потому что FLOW-26 меняет сам движок. **Для нашего расширения** это референс, как локальная модель
может САМА достраивать недостающие навыки/адаптеры (gap-detect → contract-first → sandbox E2E с оригинальным
запросом → human-gate), не превращаясь в неконтролируемую генерацию.

### 5.4. Метод многопроходной экзаменации «в действии» (обобщение по трём файлам)

1. **Baseline-first:** каждый разбор стартует от готового Grok-резюме и работает как «gap-audit + углубление»,
   а не с нуля. Проход 1 всегда = что baseline упустил (10-13 категорий).
2. **Одно измерение на проход:** проходы НЕ повторяются — planning / functional / journey / e2e / data-flow /
   failure / perf / security / testing / sprint / risk / alternatives / protocol / narrative (или доменные
   аналоги: state-machine / evidence-bundle / stations / isolation / runbook / limits).
3. **Каждый проход завершается артефактом:** таблица IN/OUT, каскад отказа, матрица рисков, per-station чек —
   не «мысли», а инспектируемая структура. Это тот же принцип, что наш `deliverable-slice`.
4. **Три уровня ценности:** Grok (что) → 10 проходов (как связано) → 20 проходов (что реализовать + как
   эксплуатировать/автоматизировать). Метод явно нацелен на «training data для авто-extraction pipeline».
5. **Честность о пробелах встроена:** flow-26 сам маркирует свой 5/7 partial-state как риск, а не «частичную
   победу»; каждый проход держит явный список «что НЕ проверено» (E2E не run, cross-validation skipped).
6. **Протокольный выход:** каждый файл в проходе 19 предлагает конкретные правила в
   `GROK-ORCHESTRATION-EXTRACTION-PROTOCOL` (Foundation-sessions, Polyglot-gate, Meta-orchestration type,
   Partial-completion, Baseline-offset, Canonical-merge). Это прямо стыкуется с проходом 6 (протоколы).

### 5.5. Прямые спецификации для плана расширения VS / VS Code (что переносим)

- **Мульти-хост через один общий контекст (из B):** ядро логики = `llm_mvp_core`; на каждый хост — тонкий
  адаптер-конфиг (`CLAUDE.md` / `.cursorrules` / `copilot-instructions.md` / `.clinerules` / VS Code
  settings). Один и тот же контракт, разный формат парсинга. Это подтверждает «thin host-adapter over shared
  core» из прохода 4 на уровне конфиг-слоя, а не только рендер-адаптеров.
- **Self-contained промпт-цепочка (из B):** формат `CONTEXT → WHAT TO BUILD → DNA CHECKLIST → TESTS → DONE
  WHEN`, каждая фаза исполнима без истории; 7-компонентная рубрика качества промпта (A/B/C-скоринг) — как
  готовый gate для нашего API-слоя «промпт→оркестрация».
- **Tool-routing по размеру контекста (из B, P13):** сложные задачи → Claude (200K), шаблонные → Copilot/Cursor;
  митигации phase-splitting / context-summary / progressive-loading. Расширение должно знать «какой хост что
  тянет».
- **Governance против drift (из B, P12):** `context/interfaces.cs`-как-world-model требует PR-checklist + CI
  diff + версионирование; Silent-расхождение — самое опасное. Для нас = синхронизация общего ядра-контракта с
  тем, что видят хосты.
- **DNA-контракты ядра (из A):** `DataProcessResult<T>` (enum-статусы, включая RateLimited) как lingua franca
  границ; generic-интерфейсы; non-empty filter; async+CancellationToken везде. Data-flow map и failure-cascade
  как обязательные проходы разбора любого нового сценария расширения.
- **Self-building loop как референс автономии (из C):** gap-detect → contract-first (спека ДО кода) →
  sandbox E2E с ОРИГИНАЛЬНЫМ запросом → Evidence Bundle (иммутабельный) → human-gate на самом рискованном
  шаге. Локальная модель расширения может достраивать недостающие адаптеры/навыки по этому шаблону, с
  human-gate вместо неконтролируемой генерации.
- **Метод разбора как процесс расширения (из 5.4):** любой новый сценарий расширения разворачивать теми же
  измерениями (functional/data-flow/failure/testing/protocol) до состояния «исполнимая спецификация», а не
  «идея».

### 5.6. Честность о покрытии прохода 5 + что остаётся для проходов 6–9

**Честность о покрытии.** Дословно прочитаны ЦЕЛИКОМ три тела: `scenario-20pass-figma-to-code.md` (1462 стр.),
`scenario-20pass-csharp-skill-library.md` (1108 стр.), `scenario-20pass-flow26-integration.md` (1130 стр.).
Укороченные `scenario-10pass-*` НЕ перечитывались построчно — их проходы 1–10 инкорпорированы внутрь
20-проходных тел (Раздел I), проверено сверкой заголовков и лог-таблиц. `scenario-20pass-SYNTHESIS.md` и
`scenario-10pass-COMPARISON-ANALYSIS.md` прочитаны в проходе 3 (свод), здесь не дублировались. Реальные входные
транскрипты сценариев (`AI-powered Figma to code...` 385 KB, `XIIGen C# skills implementation` 1.43 MB,
26-*merged документы) НЕ читались — это первичные источники, а разбор уже дистиллировал из них спецификацию;
при необходимости первичной верификации они лежат вне папки `claude-sessions` (в worktrees/Architecture plan).

**Что остаётся для проходов 6–9 (без изменений по составу, уточнено):**
- **Проход 6:** протоколы `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE` +
  `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1` + `AGENT-INSTRUCTIONS.md` — сюда стыкуются
  проход-19 вклады из всех трёх сценариев (Foundation-sessions, Polyglot-gate, Meta-orchestration type,
  Partial-completion, Baseline-offset, Canonical-merge) + 11 правил / 4 negative habits из прохода 3.
- **Проход 7:** `sonnet - analyzes/` пофайловые анализы + `prompt-orchestration/` эталоны +
  `samples of orchestarion examinations.md` (эталонные образцы того же метода экзаменации).
- **Проход 8:** `sonnet-user-prompts/` + `user prompts/` — корпус «промпт → хорошая оркестрация» для API-слоя
  расширения; сюда прикладывается 7-компонентная рубрика качества промпта (A/B/C) из прохода 5 (§5.2 P11);
  + `_tools/` + `build_session_json.py`.
- **Проход 9:** выборочно крупнейшие `Uncategorized`-сессии по `SESSION-INDEX.md` (msg_count 462/305/279/…),
  которых нет в Частях 1–35 `SESSION-ANALYSIS`; при остатке бюджета — построчное чтение самых важных grok-flow
  (`33`/`20`) и `FLOW-PREP-LIBRARY-MASTER-PLAN` целиком.

## Pass 6/10 — extraction/examination протоколы + prompt-orchestration (метод + prompt-слой)

Это проход о **методе** (как чат-сессия/пакет документов превращается в паттерн оркестрации — движок за
`SESSION-ANALYSIS.md` и за методом экзаменации RAG-06) и о **prompt-слое** (как пользовательский промпт
отображается в оркестрацию — прямой аналог prompt-input будущего расширения VS/VSCode). Проход 5 предварительно
относил `prompt-orchestration/` + `samples` к проходу 7; текущее задание parent-агента (авторитет) явно кладёт
их в проход 6 вместе с протоколами — расхождение разрешено в пользу задания, §6.6 фиксирует стык, чтобы проход 7
не дублировал.

### 6.0. Что прочитано в этом проходе (evidence)

Верхний уровень папки `claude-sessions/` содержит не только 1156 транскриптов, но и **набор
метод-документов** — именно они цель прохода 6:

| Файл | Байт | Как прочитан |
|---|---|---|
| `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md` | 56 092 | целиком (10 шагов + типы флоу + формат вывода + чеклисты + анти-паттерны) |
| `ORCHESTRATION-EXTRACTION-PROTOCOL-COMPLETE-v1.md` | 32 709 | целиком (объединение v1 + FROM-INPUTS; 20 разделов) |
| `ORCHESTRATION-EXTRACTION-PROTOCOL-v1.md` | 48 563 | как источник COMPLETE (объединён в COMPLETE §«Источник объединения»), построчно не перечитан |
| `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1.md` | 65 202 | заголовки + алгоритм Step 1–13 (§5) + output-формат; тело кейсов 4.1–4.12 инкорпорировано в COMPLETE §16 |
| `AGENT-INSTRUCTIONS.md` | 2 664 | целиком (инструкция суб-агента-воркера анализа сессий) |
| `README.md` | 1 836 | целиком (происхождение экспорта, 17 Claude-проектов, `xiigen` = 503 doc) |
| `samples of orchestarion examinations.md` | 151 127 | структура + первые ~70 стр. (сырой дамп готовых экзаменаций — рабочий выход того же протокола на FLOW-29 и др.) |
| `prompt-orchestration/` (6 файлов) | 10–45 KB каждый | у всех 6: `session details` + `prompt` (дословно) + `input files` + `result files` + `orchestration`-нарратив |
| `FILE-ANALYSIS-PROTOCOL-v1.md` | **18 401 976** | НЕ читан — это 18-МБ дамп данных (агрегированный per-file анализ), не метод-документ; помечен как источник прохода 9 при необходимости |

Проверка чтения (Rule-5 side-effect): последняя строка v2 — «Не применяется для: ... Ответов на конкретные
технические вопросы без документов»; последняя строка COMPLETE-v1 — раздел «20. One-Line Rule».

### 6.1. Метод извлечения оркестрации — «протокол экзаменации» (движок за SESSION-ANALYSIS + RAG-06)

**Главное правило (дословно, v2/COMPLETE):** «Извлечение оркестрации — это не "что входит в план". Это "как
цель пользователя превращается в контролируемый, проверяемый путь к работающему результату"». Плохой выход =
реестр (`Добавлены F982-F1027, T367-T374, CF-458-CF-472`). Хороший выход отвечает на 5 вопросов: что человек
хотел построить → как это выглядит как функциональность → почему система делится именно на такие зоны/семейства
→ какие тех-решения превращают функциональность в архитектуру → в каком порядке реализуется, проверяется,
сливается и считается готовым.

**Конвейер v2 (10 шагов) — это и есть метод экзаменации сессии/пакета:**

1. **Приём и классификация материала.** Составить карту входа (что получено, тип+роль). Типы источников:
   `USER_PROMPT` (цель) · `INPUT_FILES` (домен/ограничения) · `PRODUCED_FILES` · `STATE_FILE` (завершено ли) ·
   `CANONICAL_MERGED_DOC` (текущая истина, приоритет над input) · `PLAN_FILE` · `VALIDATION_FILE` ·
   `RAW_SESSION` (только для понимания как родился план) · `GENERATED_OUTPUT` (свидетельство, проверять).
   **Порядок доверия (убыв.):** текущая инструкция пользователя → указанный протокол/скилл → state/canonical
   merged → произведённые артефакты → input-файлы → история сессии → прежние сводки. **Обязательный пакет:**
   никогда не извлекать оркестрацию из одного файла, если есть input/context.
2. **Сканирование структуры.** Из главного файла выписать ровно 4 вещи: Задача · Что уже есть (backward
   compat, цифры/версии) · Что добавляется (конкретно F631-F640, не «10 фабрик») · Фазы. Тест готовности: можно
   ли написать одно предложение «Мы строим [X] который позволяет [кому] делать [что]»? Нет → оркестрация не
   понята, стоп.
3. **Синхронизация с актуальным состоянием.** `RENUMBER_MAP` (все ссылки только по НОВЫМ id). **SESSION_STATE
   — двойная роль:** (1) граница последнего завершённого артефакта (ниже — не трогать, выше — новое); (2) Zone
   Decomposition Table (зоны→семейства, это Layer 2, фиксировать сразу). **Типы input.zip:** A — оригинальные
   бизнес-требования (Layer 1); B — старые delta прошлого FLOW (только как стиль/шаблон); C — детальные delta
   того же FLOW (deep-dive, но числа только из files.zip). При двух ZIP: финальный ZIP = текущая истина, input
   первым читать нельзя (риск устаревшей нумерации). Validation: без «N/N PASS» не писать «завершено».
   **Кросс-проверка зон** (баг FLOW-25): имя семейства в SESSION_STATE vs ENGINE_ARCHITECTURE — рассинхрон =
   документационный баг, в секцию «Что поймал протокол».
4. **Глубокое чтение по типам файлов.** P0_PLAN читать по частям (PART1 no-code→Layer1, PART2 renumber, PART3
   phases→Layer3, PART4 validation, PART5 examples, PART6-7 counters). ENGINE_ARCHITECTURE: на компонент —
   зачем/что делает/куда смотрит/через какой Fabric/от чего зависит (без сигнатур/DNA-номеров). TASK_TYPES:
   `ENTRY`+`PURPOSE`→пользовательский сценарий; `MACHINE` (неизменяемое: consent-gate, server-grading,
   append-only ledger, tenantId) / `FREEDOM` (настраиваемое через ES-конфиг). BFA: только CRITICAL/HIGH.
5. **Красная нить + повторяющийся паттерн.** Позвоночник от первого триггера до результата (убрать retry/edge
   cases). **Три обязательных слоя:** Layer 1 (что хочет пользователь, без архитектуры) → Layer 2 (мост:
   таблица зон + принцип деления одним предложением + зона→фабрики + Design Decisions как «бизнес-требование →
   DD-NNN: решение») → Layer 3 (task types/фабрики/BFA). Прыжок через слой = неверный вывод. **BUILD TIME vs
   RUNTIME** разделять. Внутренние движки генерации: `INVENTORY (сбор контекста) → SYNTHESIS (генерация) →
   JUDGMENT (проверка: AF-станции, BFA, DNA, quality gates, стресс-тесты)` — нельзя сводить к «AI генерирует
   код».
6. **Распознавание типа флоу** (таблица из 13 типов, каждому — доп-правила): Длинная сессия · Адаптер/порт (3
   вещи: даёт хост / умеет движок / ново для платформы; ≥2 адаптера → таблица сравнения) · Мульти-фича
   (definition-time vs runtime) · Системный FLOW (release train) · Marketplace/финансовый (runtime lifecycle +
   idempotency/ledger/KYC) · Миграция+новый FLOW (два уровня, не сливать) · Регулируемая платформа
   (consent→targeting→auction→attribution→quotas→billing) · No-code/формы (зоны = data lifecycle) · Визуальный
   редактор/CMS · Расширение прошлого флоу (before/after, P0=истина) · Обучение/тьютор (замкнутый цикл) ·
   Governance/Cross-Flow Gate · Meta-Flow/Self-Extension.
7. **Границы задачи:** что просит / что НЕ входит / что подтвердить до начала.
8. **Синтез.** Нарративный порядок `ЗАЧЕМ → КАК ВЫГЛЯДИТ → КАК УСТРОЕНО → В КАКОМ ПОРЯДКЕ ДЕЛАЛИ`. Правило
   упрощения (тест «если убрать слово — изменится ли понимание?»). **Запрещённые формулировки** (описывают
   инструмент, не действие): «Применить planning-skill G0-G7», «Валидировать план». **Структура шага:**
   `N. [глагол] [конкретный объект] → [измеримый результат]`; работающие глаголы: открыть/прочитать/выписать/
   проверить/сравнить/запустить/заблокировать; нерабочие: проанализировать/обработать/применить.
9. **Формат вывода** (шаблон markdown): Что мы строим · Что это даёт · Главная новая возможность · Аналогия ·
   Архитектурный мост (принцип деления + зоны + DD) · Как выглядит для пользователя · BUILD TIME (фазы P0-P7) ·
   RUNTIME · Основной бизнес-процесс · Железные гарантии (MACHINE→позитивные обещания) · Новые паттерны ·
   Итоговое состояние · **«Что поймал протокол»** (обязательная секция: рассинхроны/редакционные артефакты/
   арх-наблюдения; если чисто — писать явно) · Что дальше · Оффер.
10. **Проверка перед отправкой.** Главный вопрос: «Оркестрация по этапам правильно построена и ничего не
    упускает?». Чеклист: цепочка замкнута (первый триггер? каждый этап производит вход следующего? нет слепых
    пятен?) · каждый этап честен (этап делает то что написано — не «контекст» описывает arbitration, баг
    FLOW-25) · гарантии настоящие (MACHINE реально не обходится; escape hatch описан полностью) · **симуляция**
    — мысленно проиграть цепочку от триггера до результата, любой непонятный переход = не описан.

**Алгоритм из FROM-INPUTS-AND-PLAN-v1 (Step 1–13)** — то же ядро в форме процедуры: (1) найти человеческий
продукт → (1.1) сначала функциональное описание → (1.2) построить functional-to-architecture bridge → (2)
новая возможность → (3) красная нить → (3.1) повторяющийся FLOW-паттерн → (4) отделить оркестрацию от
инвентаря → (5) фазы как human work chunks → (6) бизнес-процесс до техники → (6.1) сохранить story of
realization → (7) renumbering/version drift → (8) guardrails/safety → (8.1) DD как мост → (9) source vs final
версия → (9.1) отделить глобальную миграцию от возможности флоу → (9.2) источник истины когда input
исторический → (10) verification/completion state → (11) объяснить процесс анализа по запросу → (12) человеческий
синтез, не копирование файлов → (13) финальная симуляция. **Rejection checklist (18 пунктов)** и **anti-patterns**
— отбраковка выхода, который начинается с id/файлов, копируется в другой проект без изменений, перечисляет
артефакты без lifecycle, говорит «complete» без validation, смешивает build-time и runtime, использует «AI
generates X» без safety/validation/feedback.

**Связь с RAG-06 и SESSION-ANALYSIS:** этот протокол — тот самый «метод экзаменации», которым дистиллированы
56 паттернов `SESSION-ANALYSIS.md` (проход 2) и которым RAG-06 разбирал сценарии по измерениям
functional/data-flow/failure. `samples of orchestarion examinations.md` (151 KB) — сырой выход этого метода:
первые строки — готовая экзаменация «FLOW-29 Adaptive RAG» ровно по шаблону (ШАГ 6 Классификация → ЗАЧЕМ → КАК
ВЫГЛЯДИТ → КАК УСТРОЕНО с мостами A/B → зоны/семейства/фабрики). То есть protocol = метод, samples = эталонные
прогоны метода, SESSION-ANALYSIS = дистиллят.

### 6.2. Agent instructions — как суб-агентов направляли экзаменовать сессии (`AGENT-INSTRUCTIONS.md`)

Инструкция **суб-агента-воркера пакетного анализа** (не «как писать оркестрацию», а «как обрабатывать батч
сессий»):

- **Задача воркера:** обработать все сессии своего `batch_id`; на каждую: прочитать первые **60 строк** (взять
  Summary) → определить есть ли НОВЫЙ архитектурный паттерн, которого ещё нет в `SESSION-ANALYSIS.md` →
  записать находки в свой output-файл → обновить `SESSION-ANALYZE.json` (status="processed").
- **Файлы:** `SESSION-ANALYZE.json` (фильтр по batch_id) · `SESSION-ANALYSIS.md` (читать ПЕРВЫМ чтобы не
  дублировать — «Parts 1-24, 56 patterns») · base path `C:\Users\lubao\Documents\xiigen`.
- **Output-схема JSON:** `{batch_id, processed_count, new_patterns[{session_name, session_id, pattern_title,
  pattern_description, source_quote}], sessions_processed[{session_id, status, patterns_found,
  patterns_summary}]}`. Пишется в `SESSION-ANALYZE-RESULTS-{batch_id}.json` (на диске есть batch-01..08 — это
  и есть возвраты воркеров).
- **Что считать новым паттерном:** арх-решение не из 56 существующих; НОВЫЙ skill-номер/governance-правило;
  НОВЫЙ тип бага/анти-паттерн; НОВЫЙ инфраструктурный паттерн/протокол.
- **Что скипать** (mark processed, patterns_found=0): не-XIIGen темы (personal/Angular-debug/n8n/Azure);
  чистые execution-логи без арх-инсайтов; уже покрытое в Parts 1-24; `message_count <= 4` (слишком коротко).

Это **fan-out/reduce паттерн дистилляции корпуса**: N воркеров × батч → JSON-возвраты → мастер
`SESSION-ANALYSIS.md`. Прямой прообраз того, как parent распараллеливает проходы 6-10 RAG по папкам, и того, как
расширение может дистиллировать локальный корпус сессий пользователя.

### 6.3. `prompt-orchestration/` — эталоны «промпт → задача → оркестрация» (prompt-слой)

6 файлов, каждый — реальная чат-сессия, приведённая к единой карточке из **7 блоков**: `session details`
(id/name/msg_count/source-путь/**flow meaning**/**extraction status**) → `prompt` (дословный промпт
пользователя) → `session file placement` → `input files` (вложения/ZIP) → `result files` (произведённые
артефакты) → `orchestration` (извлечённый нарратив со ссылками `<!-- source lines N-M -->`). Это буквально
«вход-промпт → выход-оркестрация», карта prompt-слоя.

**Повторяемый шаблон промпта (дословно совпадает у FLOW-29, FLOW-30, FLOW-31, а FLOW-34 — вариант):**
> «We need to support a system that is capable of generating complex flows by user demand, and to generate
> nodes, inject code, this code needs to stand our architecture requirements being easy to test and later be a
> part of our core architecture… On the UI please take approach **fabric first** no matter which platform… We
> need to extend the engine… to support a flow creation described in **NN-\*** documents attached. Always check
> the basic prompt and the sources you have in the project. I need the files *including the flow NN only*: NN-…
> **SKILLS_FACTORY_RAG, UNIFIED_SOURCE_INDEX, V62_BFA_STRESS_TEST, TASK_TYPES_CATALOG, ENGINE_ARCHITECTURE,
> SESSION_STATE, MASTER_EXECUTION_PLAN**».

**Отображение промпт → задача → оркестрация (стабильное):**
- **промпт** = «расширь движок под флоу NN» + «fabric-first UI» + «проверь basic_prompt и project sources».
- **задача (deliverable)** = произвести **7 канонических merged-документов** одного флоу (ровно эта семёрка) +
  сохранить `SESSION_STATE` как границу/счётчики.
- **вход** = `basic_prompt.txt` + `RAG_INDEX.md` + 7 текущих `*_MERGED.md` (предыдущее состояние) + `NN-*.md`
  (спека нового флоу, часто с `deep research`) + пакет скилл-ZIP (`xiigen-architect-skill`, `vibecoder`,
  `skillbuilder`, `code-to-skills`, `claude-useful-skills`, `xiigen_v18_complete`, `module-architecture`).
- **выход** = те же 7 файлов, переписанные под флоу NN (в `result files` видно `/home/claude/flowNN/…` и
  `FLOWNN_complete.zip`).

**Per-flow (flow meaning + суть оркестрации):**
- **FLOW-29 — Adaptive RAG Deep Research Engine** (msg 2): адаптивная маршрутизация запроса → выбор стратегии
  поиска (vector/GraphRAG/hybrid), бандитная оптимизация `(modelId, promptVersionId, flowVariantId)`, ворота
  качества перед промоушеном (мультимодельная проверка галлюцинаций), версионирование всего с откатом
  pointer-swap, OTel-спаны через Queue Fabric. Для тенанта невидим — делает существующие флоу дешевле/точнее;
  всё настраивается через FREEDOM-конфиг. Зоны 175-184, фабрики F1176-F1247.
- **FLOW-30 — PromptOps Self-Learning Prompts** (msg 1): промпты как **versioned assets + policies**, не строки
  (`PromptTemplate/PromptVersion/PromptPatch/PromptPolicy/EvalSuite`). Runtime-цикл узла `Execute → Judge →
  Learn`; sub-flow улучшения промпта — DAG `CollectContext → RetrieveEvidencePack(RAG) → Critique(multi-model)
  → GenerateCandidate → EvaluateCandidate → PromotionDecision(candidate→canary→active) → Persist+Explain`.
  Мульти-тенант без утечки (global baseline + tenant deltas), MACHINE/FREEDOM-граница, node-card UX. Против
  «prompt sprawl»: hard-cap вариантов, auto-prune, drift-protection, context-efficiency gate.
- **FLOW-31 — Figma Screen Understanding Engine / «Functionality by Design»** (msg 4): анализ Figma-экранов →
  функциональный смысл → UI-графы → маппинг на модули → completion пропусков для кодогенерации. Прямая стыковка
  с Design-to-Code / Figma-адаптером прохода 4. Fabric-first UI, продолжение прошлой сессии (часть 7 файлов уже
  была готова).
- **FLOW-33 — System Initiation / Self-Building Bootstrap** (msg 18): **иной промпт** — «начать РАЗРАБОТКУ с
  флоу 33 и дать системе достроить остальное; планируй маленькими фазами, после каждой дай скачать код и жди
  approve; сохраняй state чтобы продолжить с места остановки; 34 — позже». Оркестрация: перевод плана .NET9/C#13
  → Python3.12/FastAPI + React Native/Expo; красная нить `PlanBundle → mini kernel → bootstrap-flow → registries
  + GraphRAG → implement-family loop → multi-model generation → 5 арбитров + retry → local deploy + smoke →
  implementation registry/ledger/UI`; 18 фаз (P1 DNA-примитивы done, P2 Database Fabric, … P13 implement-family
  meta-loop, P14-15 RN UI, P16 AWS/CI/CD, P17-18 E2E + первая семья).
- **FLOW-33 Database Fabric P2** (msg 13): продолжение — «раздели фазу 2 на 5 меньших фаз, покажи план и не
  продолжай пока не одобрю». Оркестрация P2 = 5 approval-gated под-фаз (base → ES-провайдер → PG-провайдер →
  registry/resolver → cross-provider), каждая с тестами и `STATE.json`, ZIP после каждой под-фазы.
- **FLOW-34 — Skill Multi-Target Translation Ancestor** (msg 19): «единый skillset из v4, для каждого скилла —
  when/why + альтернатива на server (.NET7/8/9, PHP, Node, Rust, Python) и client (Angular, React+Redux, RN);
  сначала no-code объяснение по фазам, потом валидация плана против промпта, потом positive/negative примеры;
  фаза короткая для save-state; ZIP после каждой фазы, не продолжать без feedback; проверить что уже сделано и
  продолжить». Предок мульти-таргет-трансляции навыков (кросс-стек библиотека — тот же принцип, что XIIGen
  skills-mapping из MEMORY).

**Управляющий контракт промптов (то, что задаёт форму работе, независимо от домена):** fabric-first UI ·
семёрка канонических документов как единица deliverable · «short phase → save state → ZIP → жди approve» ·
«проверь что уже создано, не сбрасывай, продолжи» · basic_prompt + project sources как обязательный контекст ·
явное «только флоу NN» (границы задачи, ШАГ 7 протокола).

### 6.4. Как это связано с планом «Continue VS & VSCode extensions» (prompt-слой)

- **Prompt-input расширения = этот prompt-слой.** Пользователь в хосте (VS/VSCode) пишет промпт →
  расширение-адаптер (тонкий, R6) отправляет задачу в ядро `llm_mvp_core` → ядро прогоняет оркестрацию → ответ
  возвращается в UI хоста. `prompt-orchestration/` — готовый корпус «как реальный промпт превращается в
  контролируемую оркестрацию»; он же — эталон для тренировки/оценки prompt→task-маршрутизатора расширения
  (какой промпт → какие артефакты/фазы/гейты).
- **Метод экзаменации = ридер сессии для локальной модели.** Протокол v2/COMPLETE — это то, как локальная
  модель расширения должна *читать* чат-сессию/пакет и извлекать исполнимую оркестрацию (а не пересказ):
  порядок доверия к источникам, source-of-truth detection, renumber-sync, 3-слойный мост, симуляция цепочки,
  секция «что поймал протокол» (детект рассинхронов). Это прямой донор для «prompt→task→orchestration» слоя,
  который в CLAUDE.md значится требованием (`42.ProjectWikiDomain`: user request → code trace → doc graph →
  wiki answer).
- **Fan-out/reduce дистилляция (`AGENT-INSTRUCTIONS.md`)** = шаблон, как расширение может дистиллировать
  локальный корпус сессий пользователя в адаптивную модель (батч-воркеры → JSON-возвраты → мастер-дистиллят),
  с dedupe против уже известных паттернов и skip-правилами.
- **Governance-совпадения** с нашими правилами: `MACHINE/FREEDOM` = «железные гарантии» vs настраиваемое;
  «не писать complete без validation» = completion-evidence gate; «save state / phase / approve» = bounded
  work-order + STATE.json + human gate; «оркестрация ≠ реестр артефактов» = запрет на реестр-как-оркестрацию.
- **Запрещённые формулировки протокола** (описание инструмента вместо действия: «применить planning-skill
  G0-G7») — тот же класс, что наш `deliverable-slice` / anti-ceremony: шаг обязан иметь глагол+объект+результат.

### 6.5. Что поймал протокол в самих метод-документах (наблюдения)

- **Дубли протокола:** три живых версии (v1, v2, COMPLETE-v1) + четвёртый источник (FROM-INPUTS-AND-PLAN-v1).
  v2 (RU, 10 шагов) и COMPLETE-v1 (RU/EN, 20 разделов) — параллельные каноны одного метода; для расширения
  брать **v2 как процедуру + COMPLETE §16 special cases + FROM-INPUTS Step 1–13 как алгоритм**. Это `dual-live`
  на уровне метода — при переносе выбрать один owner, остальные пометить историческими.
- **README-факт:** экспорт из `conversations.json` без project-mapping → все 1156 сессий в `Uncategorized`;
  Claude-проект `xiigen` имел **503 project docs** (не транскрипты) — они НЕ в этой папке. То есть корпус
  сессий здесь = разговоры, а не canonical docs проекта.
- **`result files` во многих карточках** содержат дубли с висячей кавычкой (`…SESSION_STATE.md"`) — редакционный
  артефакт экспортёра, не два разных файла; FLOW-30 result files = «REQUIRES CHECK: не извлечены автоматически».

### 6.6. Честность о покрытии прохода 6 + что остаётся для проходов 7–9

**Честность.** Целиком прочитаны: `ORCHESTRATION-EXTRACTION-PROTOCOL-v2.md`,
`ORCHESTRATION-EXTRACTION-PROTOCOL-COMPLETE-v1.md`, `AGENT-INSTRUCTIONS.md`, `README.md`, и по всем 6
`prompt-orchestration/`-файлам — блоки `session details/prompt/input files/result files` + начало
`orchestration`-нарратива. Прочитаны заголовки + алгоритм §5 (Step 1–13) + output-формат
`PROTOCOL-…-FROM-INPUTS-AND-PLAN-v1.md`; его тело кейсов 4.1–4.12 не перечитано построчно (инкорпорировано в
COMPLETE §16, сверено по заголовкам). `ORCHESTRATION-EXTRACTION-PROTOCOL-v1.md` не перечитан построчно (источник
COMPLETE). `samples of orchestarion examinations.md` — структура + первые ~70 стр. (подтверждён как сырой выход
метода на FLOW-29; полный построчный разбор всех экзаменаций — не делался, объём 151 KB, это эталонные прогоны,
не новый метод). `FILE-ANALYSIS-PROTOCOL-v1.md` (18 МБ) не читан — дамп данных, не метод.

**Стык с прежней рамкой:** проход 5 относил `prompt-orchestration/` + `samples` к проходу 7 — задание parent
перенесло их в проход 6 (выполнено здесь). Поэтому **проход 7 переопределяется** на непрочитанное: пофайловые
`sonnet - analyzes/` + `claude orchestrator`/`grok orchestrator` остатки, не покрытые проходами 3–4, и
полный разбор `samples of orchestarion examinations.md` (все экзаменации, не только FLOW-29).

**Остаётся для проходов 7–9:**
- **Проход 7:** `sonnet - analyzes/` (пофайловые sonnet-разборы) + полный `samples of orchestarion
  examinations.md` (эталонные прогоны метода §6.1 на всех флоу) + любые остатки `claude/grok orchestrator`,
  не вошедшие в проходы 3–4.
- **Проход 8:** `sonnet-user-prompts/` + `user prompts/` — корпус «промпт → хорошая оркестрация» для API-слоя
  расширения (к нему — 7-компонентная рубрика качества промпта A/B/C из прохода 5 §5.2 P11 + шаблон промпта
  §6.3); + `_tools/` + `build_session_json.py` (как строился `SESSION-ANALYZE.json` для воркеров §6.2).
- **Проход 9:** выборочно крупнейшие `Uncategorized`-сессии по `SESSION-INDEX.md` (msg_count 462/305/279/…),
  которых нет в Частях 1–35 `SESSION-ANALYSIS`; при бюджете — `FILE-ANALYSIS-PROTOCOL-v1.md` точечно (18 МБ,
  только если нужен агрегированный per-file слой).

**RAG-файл:** `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-09-claude-sessions.md`
(проход 6 добавлен как отдельная секция, разделы 1–5 не тронуты).

---

## Pass 7/10 — sonnet analyzes (56) + orchestration-examination samples (модельно-аналитический слой)

> Область прохода: `claude-sessions/sonnet - analyzes/` (56 файлов) + `claude-sessions/samples of
> orchestarion examinations.md` (151 KB, 1564 стр.). Цель — закрыть **модельно-аналитический слой**:
> третий, форензик-угол на тот же корпус, плюс сверка трёх модельных взглядов (Claude / Grok / Sonnet)
> на оркестрацию. Проход 6 перенёс сюда `sonnet - analyzes/` и полный `samples`-файл; проходы 3–4 уже
> покрыли `claude orchestrator` (синтез-нарративы) и `grok orchestrator` (сырой драфт).

### 7.1. Что такое папка `sonnet - analyzes/` (56 файлов) — форензик-слой поверх всего корпуса

Это **не** ещё один набор оркестрационных нарративов. Это **пофайловый критический разбор всей папки
`claude-sessions/`**, выполненный моделью Sonnet 2026-06-02. Sonnet берёт каждый артефакт корпуса —
инструкции агента, README, мастер-дистиллят, пайплайн-скрипты, протоколы, batch-результаты, индексы,
и сами `claude orchestrator/FLOW-XX` файлы — и прогоняет через жёсткий шаблон вопросов. Это слой **на
уровень выше** Claude/Grok: Sonnet анализирует сами аналитические артефакты (включая выход Claude) и
пайплайн, который их произвёл.

**56 файлов делятся на классы:**
- `FLOW-01…FLOW-33-orchestration-analysis.md` (33 файла) — разбор каждого `claude orchestrator/FLOW-XX`
  файла (тот же корпус, что проход 3, но форензик-углом).
- `SESSION-ANALYSIS-analysis.md`, `SESSION-ANALYZE-analysis.md`, `SESSION-INDEX-analysis.md` — разбор
  мастер-документов знаний и навигации.
- `SESSION-ANALYZE-RESULTS-batch-01…08-analysis.md` (8 файлов) — разбор выхода 8 агентов-воркеров
  (тут Sonnet вытаскивает конкретные извлечённые паттерны P1–P67).
- `build_session_json-analysis.md`, `export_project_conversation_map-analysis.md`,
  `apply_project_conversation_map-analysis.md`, `_manifest-analysis.md` — разбор **пайплайн-скриптов**.
- `AGENT-INSTRUCTIONS-analysis.md`, `README-analysis.md` — инфраструктура.
- `ORCHESTRATION-EXTRACTION-PROTOCOL-v1/v2/COMPLETE-analysis.md`,
  `PROTOCOL-FROM-INPUTS-AND-PLAN-v1-analysis.md` — разбор самих протоколов извлечения.
- `samples-orchestarion-examinations-analysis.md` — мета-разбор файла с эталонными прогонами.

**Жёсткий шаблон Sonnet (6–9 вопросов на файл):** (1) Кому адресован — агенту или человеку;
(2) Что теряется без файла; (3) Что читает / что производит (входной/выходной); (4) Этапы/структура;
(5) Связи с другими файлами (таблица); (6) Ключевые термины/поля; (7) **Что остаётся непонятным**
(противоречия и пробелы); (8) Место в пайплайне (ASCII-диаграмма); (9) для скриптов — **Риски и слабые
места**. Завершает каждый файл секция **«Что поймал анализ»** — то, что не лежит на поверхности.

### 7.2. Чем угол Sonnet отличается от Claude и Grok (что Sonnet нашёл/добавил)

Ключевой вклад прохода: Sonnet — это **аудитор-форензик**, а не автор-синтезатор. Он даёт то, чего
нет ни у Claude (нарратив оркестрации), ни у Grok (сырой драфт):

1. **Секция «Что остаётся непонятным» = явный реестр пробелов и противоречий.** Пример
   (`AGENT-INSTRUCTIONS`): порог длины сессии противоречит сам себе — «SKIP» говорит
   `message_count <= 4`, «Process» говорит `< 6`; нет защиты от race condition при параллельной записи
   в один `SESSION-ANALYZE.json`; «56 паттернов / 24 части» — живые числа, написаны один раз, не
   обновляются.
2. **Кросс-файловые противоречия с канонической документацией.** `SESSION-ANALYSIS-analysis` ловит:
   `CLAUDE.md` перечисляет 8 Elasticsearch-индексов, а живые сессии упоминают ещё 5
   (`xiigen-translation-refs`, `xiigen-lifecycle`, `xiigen-marketplace-packages`,
   `EVENT_PROPAGATION_INDEX`, `FLOW_DEPENDENCY_GRAPH`) — вывод: при работе с ES сверяться с
   SESSION-ANALYSIS, не с CLAUDE.md. Ещё: `FILE-ANALYSIS-PROTOCOL-v1` описывает SESSION-INDEX как «8
   категорий», а по факту 10.
3. **Аудит провенанса и деградации трассируемости.** `batch-04-analysis` ловит **регрессию**: в
   batch-01/03 у каждого паттерна было поле `source_quote`, в batch-04 оно **исчезло полностью** —
   паттерны P57–P67 теряют прямое доказательство происхождения (остаются только UUID сессий). Плюс
   важное открытие: `source_quote` вообще **не дословные цитаты, а аналитические рефераты агента от
   третьего лица** («Claude identified…», «Luba advocated…») — при верификации надо читать оригинал.
4. **«Риски и слабые места» для скриптов** — то, что синтез-нарратив пропускает. `build_session_json`:
   сравнение имён `p.lower() in nl OR nl in p.lower()` даёт ложные срабатывания (новую сессию помечает
   как обработанную); скрипт **полностью перезаписывает** `SESSION-ANALYZE.json`, стирая заполненные
   агентами `patterns_summary`; захардкоженные пути `C:\Users\lubao\...`; нет идемпотентности по
   `session_id` (сравнение по имени). `export_project_conversation_map`: `findUuidInObject` берёт первый
   попавшийся UUID → при чужом UUID в localStorage все запросы вернут 403.
5. **Диагностика ошибок классификатора.** `SESSION-INDEX-analysis` ловит: две **иммиграционные** сессии
   («Provincial immigration programs…», «Israeli degree recognition…») попали в технический раздел
   «Migration & Phase Implementation» из-за слова «immigration». Классификация по ключевым словам в
   названии, не по содержанию.
6. **Философские системные разрывы.** `SESSION-ANALYSIS-analysis` формулирует **Process
   Self-Verification Gap**: тот же rigour (проверки, тесты, BFA-валидация), что применяется к
   сгенерированному коду, **не** применяется к сессионным файлам, которые этот код производят. Система
   умеет верифицировать то, что генерирует для пользователей, но не то, что генерирует для себя.
7. **Числовая сверка как метод.** Sonnet проверяет счётчики арифметически: SESSION-INDEX
   365+41+37+28+16+98+92+66+214+11 = 968 (совпадает с заголовком); batch-01: 14 сессий с
   `patterns_found>=1` − 2 слияния = 12 объектов в `new_patterns` («расхождение объяснено, не баг»).

**Итог сравнения угла.** Grok пишет живой, но неточный черновик оркестрации (числа могут быть
устаревшими). Claude синтезирует из Grok+canonical проверенный нарратив и держит секцию «Что поймал
протокол». Sonnet поднимается ещё выше: аудирует **весь корпус вместе с выходом Claude и пайплайном** —
и находит пробелы, противоречия, регрессии и риски, невидимые изнутри одного флоу.

### 7.3. Конкретное содержимое, извлечённое Sonnet (полезное для плана расширений)

**Полный пайплайн обработки истории сессий (две параллельные ветки), восстановленный Sonnet:**

```
Ветка A (для агентов — извлечение паттернов):
  _manifest.csv (969 строк, 10 колонок, автогенератор НЕ найден в репо — внешний инструмент)
    -> build_session_json.py (categorize: xiigen/personal/other; priority high/med/low/skip по
       порогам 40/15/6 сообщений; батчи по 50; сортировка high->med->low)
    -> SESSION-ANALYZE.json (машинный реестр + batch_id)
    -> 8 агентов-воркеров x 50 сессий (по AGENT-INSTRUCTIONS.md: читать только первые 60 строк,
       сравнить с известными паттернами, не дублировать)
    -> SESSION-ANALYZE-RESULTS-batch-01..08.json (кэш паттернов + журнал обработки)
    -> SESSION-ANALYSIS.md (мастер: 35 частей + SUMMARY из 119 паттернов) -> контекст для новых сессий

Ветка B (правильная раскладка по проектам — обход неполного экспорта Anthropic):
  export_project_conversation_map_from_claude_console.js (в браузере, DevTools, живое приватное API
     /conversations_v2, credentials:"include", 17 захардкоженных проектов)
    -> claude-project-conversation-map.json/.csv
    -> apply_project_conversation_map.py (JOIN conversation_uuid = session_id, shutil.copy2 — копирует,
       не перемещает, оригиналы целы)
    -> claude-sessions-mapped/<project>/<date>/<id>/<name>.md

Параллельно (для человека): _manifest.csv -> SESSION-INDEX.md (968 сессий, 10 разделов, снимок 2026-05-28)
```

Почему ветка B вообще есть: экспорт аккаунта Anthropic (`conversations.json`) **не содержит поля
привязки к проекту** — все 968 сессий валятся в «Uncategorized». Живое веб-приложение claude.ai эту
связь знает через недокументированный эндпоинт `conversations_v2`. Скрипт вытаскивает карту, пока
пользователь авторизован. Это прямой техответ на проблему из `README`.

**Конкретные извлечённые паттерны (Sonnet распарсил batch-файлы):**
- batch-01 -> P1–P12 (24% yield из 50 сессий): COPY/ADAPT/REWRITE + 7-Step Code-to-Skills; Tool Maturity
  Lifecycle L0–L4 + 4 критичных бага V7; Experience-First Architecture + Glass-Box Design Resolution;
  Three Task Archetypes + JudgmentMode.PhaseValidation + HumanApprovalGate; **AWAITING_DECISION**
  lifecycle + 5 обязательных design-вопросов (anti-regression: блокирует разработку до ответа); V28D
  extensions (Judge Panel); **FLOW-33 Python-стек** (Python 3.12 + FastAPI + React Native — сюрприз в
  .NET-системе); Figma Plugin гибридная монетизация + ML skill builder; **QUIRK-11** (React vs HTML
  export, SceneNode postMessage serialization bug); Gate Script Executability + SK-419 8-полей;
  Session Load Plan v16 + SK-528 + systemic `isolated:false` bug; FLOW-47 Marketplace designBundleRefs.
- batch-04 -> P57–P67 (11 паттернов): Three-Loop Engine, Judge Panel V25U, Skill Genesis Loop, Tool
  Graduation Pipeline (sandbox->key vault->injection), ANALYSIS+MIGRATION Execution Modes, Flow
  Mutation/Extraction, W3C Trace Context как DNA-7, Self-Developing Meta-Flow (FLOW-26: +28 фабрик,
  +27 task types, +35 BFA-правил в одном расширении — рост на порядок), Master Plan v4 Executable
  Gates, Fabric-First UI Mandate, **P67 Completed-But-Unmerged Extension Gap** (первый
  ПРОЦЕССНЫЙ паттерн — человеческая ошибка «доделал расширение, но не влил в реестр», с алгоритмом
  исправления: «сначала влить всё незавершённое, потом продолжать»).
- «Два типа нуля» (важно для дедупликации): `Skipped: category=other` (агент НЕ читал контент) !=
  `Deep read: no new patterns` (агент прочитал, но покрыто/pre-XIIGen эпоха). Смешивать нельзя.

### 7.4. Файл `samples of orchestarion examinations.md` — эталонные прогоны метода (1564 стр.)

Это **транскрипт метода в бою**: агент применяет `ORCHESTRATION-EXTRACTION-PROTOCOL-v2` к серии
**FLOW-29 -> FLOW-36** (8 экзаменаций). Не учебные примеры — боевые прогоны на реальных zip-пакетах.
Каждый экзамен — жёсткий 8-шаговый паттерн: (1) команда `продолжай тест`; (2) лог чтения
(`Ran N commands`, `Read N files`); (3) ЗАЧЕМ; (4) КАК ВЫГЛЯДИТ; (5) КАК УСТРОЕНО (зоны/фабрики/
MACHINE vs FREEDOM); (6) В КАКОМ ПОРЯДКЕ ДЕЛАЛИ (фазы с оценками); (7) **«Что поймал протокол»**
(обязательная); (8) расширение протокола (`Edited …PROTOCOL-v2.md +N -0`). Файл — **feedback-слой петли
самообучения**: сессия N расширяет протокол -> сессия N+1 работает более умным протоколом. Рост
протокола: ~700 -> 1138 строк после FLOW-33 (+60%), 1138 -> 1219 после FLOW-34.

**Доказательная база ценности протокола: ни один из 8 флоу не прошёл без хотя бы одного документного
бага.** Конкретные worked examples (это и есть «концентрат» самплов):

- **FLOW-30** (PromptOps, Generative Learning): Cascading Stale Header глубины 4 — заголовок
  SESSION_STATE застрял на «Post the self-developing meta-flow» (FLOW-26), в теле FLOW-27/28/29/30.
  Введён **алгоритм измерения глубины стейла: N_last − N_header = 30−26 = 4**. Отличие Selection
  (FLOW-29) от Generative (FLOW-30): выбрать лучшую стратегию vs написать лучший промпт через
  Critic->Editor->Guard.
- **FLOW-31** (Design Intelligence): три бага сразу — VALIDATION_REPORT/EXECUTION_PLAN для «31»
  ИДЕНТИЧНЫ файлам FLOW-30 (naming confusion: «31» = номер сессионного прохода, не FLOW); PHANTOM
  archetype `ContractArchetype.MAPPING` (в плане, в enum никогда не добавлен, дублировал MODULE);
  **Session-Isolation offset +196 фабрик** (input baseline F1075/post-FLOW-25, canonical F1271/
  post-FLOW-30, пропущено 5 флоу). Плюс архитектурно: **Evidence-first IRON RULE** (каждый AI-вывод
  обязан иметь >=1 evidence item nodeId+текст+компонент, нарушение = полный отказ); **ZERO LLM
  constraint** (T495 — BUILD FAILURE если LLM вызван в детерминированной задаче); **Dual-Session
  Output** (domain merge + meta-system upgrade V31* в одном zip — нельзя смешивать); матрица 20x15
  (20 типов систем x 15 типов модулей) жёстко ограничивает пространство поиска.
- **FLOW-32** (Sharable Flows Marketplace): Session-Isolation offset = 0 (renumbering не нужен, F1339 ->
  F1339). Новый documentation-паттерн **TRIPLE_INPUT_TYPE**: пакет содержит 3 независимых вектора
  (meta-system upgrade + go-to-market research про Salesforce/HubSpot + сам domain merge). Эволюция:
  Dual-Session (FLOW-31) -> Multi-Vector/Triple (FLOW-32). Шаблон = App Store, Logic/Data Separation.
- **FLOW-33** (System Initiation / Bootstrap): **пик тяжести — 3 серьёзных бага**. (1) **Merge Plan
  State Label Mislabel**: лейбл «Post FLOW-32» правильный, но числа post-FLOW-31 (F1338 вместо F1418) ->
  renumber table неверна (shift +708 вместо +788). (2) **Максимальная Session-Isolation Gap = 15 флоу**
  (input baseline post-FLOW-17/F630, canonical post-FLOW-32/F1418, пропущены 18–32). Ground truth —
  `FLOW33_RENUMBER_MAP.json`. (3) Interface Naming Inconsistency. Строит bootstrap kernel + implement-
  family meta-engine (AI-цикл с 5-арбитровым консенсусом). Протокол расширен 7 блоками.
- **FLOW-34** (Skill Multi-Target Translation): Session-Isolation 4-й раз подряд (16 флоу пропущено).
  Отличие от FLOW-33: числа merge plan правильные (без Label Mislabel). Новый баг **Cross-Flow
  Compatibility Gap** (Family 89 конфликт). Оксюморон в индексе: «pre-F630 F641» бессмысленно, т.к.
  F641 > F630. Архитектура: **Canonical Skill Spec** (OpenAPI + CloudEvents + JSON Schema + golden
  tests -> адаптер на каждый из 10 стеков, один скил -> N реализаций, все против одних golden tests).
- **FLOW-35** (Secrets Fabric + ConfigBuilder + Grok Provider): **переломный момент — ПЕРВЫЙ флоу без
  Session-Isolation Bug** (план стартовал с правильного F1484, нет RENUMBER_MAP). Единственный чисто
  инфраструктурный bug-fix флоу (чинит 2 SDK-нарушения из кода FLOW-33). Новые находки: Test Baseline
  Mismatch (план «~1197 total» vs факт 842); **SDK Hygiene Gap** (добавили CF-789/790 про secrets, но
  нет BFA-правила «FORBIDDEN: import EOL package» / «cross-provider SDK import» — нарушение повторится
  без gate). Вывод Sonnet: исчезновение Session-Isolation Bug в FLOW-35 — **не случайность, команда
  ужесточила подготовку входных файлов**; файл-самплы = зеркало здоровья процесса.
- **FLOW-36** (Self-Implementing Engine Benchmark): **два новых класса бага**. (1) **Partial CRITICAL
  CORRECTION** (главный): REQUIREMENTS-SUMMARY объявляет «CRITICAL CORRECTION», чинит коллизии task
  types и BFA, но пропускает design records (DR-240 start при canonical next DR-269 -> коллизия
  DR-240..268) и stress tests (ST-432 при занятом ST-499) — **частичное исправление создаёт ложную
  безопасность, опаснее забывчивости**. (2) **Document Alias Substitution**: точные ID заменены
  псевдонимами («the retrieval system», «a task type», «the meta-arbitration flow») — псевдонимы
  скрывают числа и делают точную верификацию невозможной. (3) Phase Numbering Inconsistency. Плюс
  **тройной стек-конфликт**: ENGINE_ARCHITECTURE = Python 3.12+FastAPI, планы FLOW-36 = NestJS
  10+TS 5, актуальный CLAUDE.md = .NET 9/C# — тест-baseline несравнимы (842 Python vs 2342 NestJS).

**Эволюция Session-Isolation Bug (сквозная линия):** появился FLOW-31 (gap 5) -> FLOW-33 (gap 15, пик)
-> FLOW-34 (gap 16) -> **исчез FLOW-35** -> FLOW-36 (только DR/ST ошибочны, фабрики корректны).
Системная причина: input-файлы генерировались в изолированных сессиях без загрузки canonical state;
протокол научился ловить это через RENUMBER_MAP.

### 7.5. Сверка трёх модельных взглядов (Claude / Grok / Sonnet) — закрытие модельно-аналитического слоя

Один и тот же корпус оркестрации прошёл через три модели, каждая на своём уровне абстракции:

| Модель | Роль | Артефакт | Уровень | Сильная сторона | Слабость |
|--------|------|----------|---------|-----------------|----------|
| **Grok** | Живой черновик | `grok orchestrator/NN flow.md` | внутри флоу, сырой | быстрый «живой» язык, продуктовое чутьё | числа могут быть устаревшими/неточными (Claude ловит overcounting: 14 vs 13 checks) |
| **Claude** | Синтезатор-нарратив | `claude orchestrator/FLOW-XX-orchestration.md` | внутри флоу, проверенный | синтез Grok+canonical, обязательная секция «Что поймал протокол», BUILD/RUNTIME, ЗАЧЕМ->КАК->ПОРЯДОК | смотрит внутрь одного флоу; не аудирует пайплайн и кросс-файловые противоречия |
| **Sonnet** | Форензик-аудитор | `sonnet - analyzes/*-analysis.md` | над корпусом | реестр пробелов/противоречий, риски скриптов, аудит провенанса, числовая сверка, place-in-pipeline | не автор нарратива; описывает, а не строит; статус «без замечаний» местами оптимистичен |

**Ключевой вывод сверки:** три взгляда **иерархичны, а не конкурентны**. Grok = исходное сырьё
(low authority по «Порядку доверия» протокола: 7-й уровень). Claude = проверенный синтез (проверяет
Grok против canonical, держит «Что поймал протокол»). Sonnet = мета-аудит, работающий на уровень выше —
он берёт в том числе **выход Claude** (те самые `FLOW-XX-orchestration.md`) и пайплайн, который всё
произвёл, и находит то, что не видно изнутри одного флоу: противоречие CLAUDE.md по ES-индексам,
регрессию `source_quote` в batch-04, ошибки классификатора в SESSION-INDEX, риски перезаписи в
`build_session_json`, Process Self-Verification Gap. Практически: **Sonnet «Что остаётся непонятным»
+ «Риски» — это готовый backlog верификации** для любого, кто продолжает работу; Claude-нарративы —
готовое «что строим»; Grok — быстрый первый проход. Три протокола (v1 объясняющий 583 стр., v2
операционный 828 стр., FROM-INPUTS-AND-PLAN параллельная ветка 2247 стр. с 12 спецкейсами и Source
Classification / Authority Order из 7 уровней) — общий метод, которым все три модели пользовались.

**Связь с расширениями VS / VS Code (зачем это в RAG):** метод оркестрации + трёхуровневая проверка
(Grok-черновик / Claude-синтез / Sonnet-аудит) — это готовый шаблон для orchestrator-слоя расширения:
как из пакета разнородных документов извлечь «красную нить» (намерение -> контролируемый проверяемый
путь), как классифицировать входы по Authority Order, и как встроить обязательный self-audit
(«Что поймал протокол» + Sonnet-стиль «Что остаётся непонятным»). Bug-каталог самплов (Stale Header /
Session-Isolation / Label Mislabel / Partial Critical Correction / Alias Substitution) — это готовые
проверки согласованности нумерации/состояния, применимые к любому многосессионному генеративному
пайплайну.

### 7.6. Честность о покрытии прохода 7 + что остаётся для проходов 8–9

**Честность (что реально прочитано построчно).** Полностью: `AGENT-INSTRUCTIONS-analysis`,
`README-analysis`, `SESSION-ANALYSIS-analysis`, `samples-orchestarion-examinations-analysis`,
`FLOW-01-` и `FLOW-31-orchestration-analysis`, `build_session_json-analysis`,
`ORCHESTRATION-EXTRACTION-PROTOCOL-v2-analysis`, `PROTOCOL-FROM-INPUTS-AND-PLAN-v1-analysis`,
`SESSION-ANALYZE-RESULTS-batch-01-analysis`, `SESSION-INDEX-analysis`,
`SESSION-ANALYZE-RESULTS-batch-04-analysis` (12 файлов целиком); `export_project_conversation_map-`,
`apply_project_conversation_map-`, `_manifest-analysis` (начала + ключевые секции). Это **15 из 56**
файлов, покрывающие **все классы** папки (инструкции, README, мастер-анализ, пер-флоу, пайплайн-скрипты,
протоколы, batch-результаты, индексы). Шаблон Sonnet восстановлен из FLOW-01 (там же реконструирован
стандартный шаблон FLOW-серии). Оставшийся 41 файл — это `FLOW-02..33-analysis` (тот же шаблон,
характеризован через FLOW-01/FLOW-31) и `batch-02/03/05/06/07/08-analysis` (та же структура, что 01/04).
`samples of orchestarion examinations.md` — структура (1564 стр.) + все 8 экзаменаций по секциям
«Что поймал протокол» + FLOW-30..36 конкретика прочитаны через grep-срезы; построчно все 151 KB не
читались (эталонные прогоны метода, не новый метод — суть инкорпорирована в §7.4).

**НЕ покрыто пофайлово (честно):** `FLOW-02..30, FLOW-32, FLOW-33-analysis` (28 пер-флоу sonnet-файлов),
`batch-02/03/05/06/07/08-analysis` (6 файлов), `SESSION-ANALYZE-analysis`,
`ORCHESTRATION-EXTRACTION-PROTOCOL-v1/COMPLETE-analysis`. Все — того же шаблона; уникальная ценность в
них — конкретные пер-флоу баги и паттерны P13–P56/P68+, при точечной надобности берутся адресно.

**Остаётся для проходов 8–9 (по рамке проходов 6–7):**
- **Проход 8:** `sonnet-user-prompts/` + `user prompts/` — корпус «промпт -> хорошая оркестрация» для
  API-слоя расширения (к нему — 7-компонентная рубрика качества промпта из прохода 5 §5.2 + шаблон
  промпта §6.3). Плюс адресный сбор паттернов P13–P56/P68+ из непрочитанных batch-файлов, если нужен
  полный реестр 119 паттернов.
- **Проход 9:** выборочно крупнейшие `Uncategorized`-сессии по `SESSION-INDEX.md` (msg_count
  462/305/279/…), которых нет в Частях 1–35 `SESSION-ANALYSIS`; при бюджете — `FILE-ANALYSIS-PROTOCOL-v1.md`
  точечно (18 МБ, только если нужен агрегированный per-file слой). Проход 10 — сборка/сверка RAG.

**RAG-файл (проход 7):** `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-09-claude-sessions.md`
(проход 7 добавлен как отдельная секция; разделы 1–6 не тронуты).


## Pass 8/10 — user-prompt корпус + pipeline-скрипты (prompt-input API-слой расширения)

Цель прохода: закрыть **prompt-input слой** — то есть точно описать, **какой формы бывает реальный
вход пользователя** (сырые промпты), **как этот вход отображается в оркестрацию** (уже разобранную в
проходах 5–6), **что Sonnet нашёл при разборе промптов** (что принимается/отклоняется и почему), и
**как корпус механически собирается** (pipeline-скрипты — это модель того, как расширение будет
строить свои собственные данные «промпт→оркестрация»). Это прямой аналог API-слоя расширения:
`host prompt → тонкий адаптер (нормализация) → core-оркестрация`.

Прочитано построчно в этом проходе (папки `user prompts/`, `sonnet-user-prompts/`, `_tools/`,
корневой `build_session_json.py`):
`user prompts/user prompts.md` (105 KB, 103 записи промптов — сырой корпус), `user prompts/accepted
orchestration pattertns.md` (эталон-разбор одного паттерна целиком), `sonnet-user-prompts/sonnet -
accepted user prompts.md` (78 записей), `sonnet-user-prompts/sonnet - rejected user prompts.md` (94
записи с причинами), `sonnet-user-prompts/sonnet - approved orchestration.md` (15 записей, полный
разбор), `sonnet-user-prompts/sonnet - declined orchestration.md` (63 записи, полный разбор), три
triage-CSV (`_triage.csv` 916 строк, `_workset.csv` 191, `_toprocess.csv` 172),
`_tools/apply_project_conversation_map.py`, `_tools/export_project_conversation_map_from_claude_console.js`,
корневой `build_session_json.py`.

---

### 8.1. Что физически лежит в prompt-корпусе (карта файлов)

| Файл | Роль в pipeline | Объём | Формат записи |
|---|---|---|---|
| `user prompts/user prompts.md` | **Сырой корпус** — извлечённые первые (task-origin) сообщения пользователя | 103 записи | `Проект / Название / Уникальный номер / Дата / Message index` + текст промпта, разделитель `---` |
| `user prompts/accepted orchestration pattertns.md` | **Эталон** одного полного разбора «промпт→оркестрация» (Flow 33) с метаданными сессии, списком приложенных файлов, «Accepted Orchestration», «Why Accepted», «Next Extraction Layer» | 1 паттерн | развёрнутый markdown-шаблон |
| `sonnet-user-prompts/sonnet - accepted user prompts.md` | **Sonnet-фильтр: ПРИНЯТО** — промпты, прошедшие критерий task-origin (годятся как обучающие входы) | 78 | те же 5 полей + текст |
| `sonnet-user-prompts/sonnet - rejected user prompts.md` | **Sonnet-фильтр: ОТКЛОНЕНО** — с полем `Rejection reason:` | 94 | 5 полей + `Rejection reason` |
| `sonnet-user-prompts/sonnet - approved orchestration.md` | **Позитив prompt→orchestration**: `Промпт + Оркестрация(5 шагов) + Файл плана + Файлы контекста + Результат ОДОБРЕНО ✓` | 15 | `## Название` + структурированные блоки |
| `sonnet-user-prompts/sonnet - declined orchestration.md` | **Негатив prompt→orchestration**: `Промпт + Оркестрация(попытка) + Причина отклонения + Повторная попытка(Standalone версия)` | 63 | `## Название` + блоки |
| `_triage.csv` / `_workset.csv` / `_toprocess.csv` | Механическая триаж-очередь (см. §8.5) | 916 / 191 / 172 | `Folder,UUID,Name,MessageCount,FilePath` |

**Ключевой вывод для расширения:** это не «логи», а **готовый размеченный датасет «промпт →
принято/отклонено → оркестрация → повторная попытка»**. Именно этой размеченной парой обучается
prompt-input адаптер: сначала **gate** (годен ли вход), потом **map** (в какую оркестрацию), потом
при провале — **rewrite** (нормализовать вход в самодостаточный).

---

### 8.2. Реальные формы входа — что промпт-API расширения ОБЯЗАН принимать

Сырой корпус (103 записи) распадается на устойчивые кластеры форм входа. Это буквально те формы, что
пользователь набирает в чат хоста (Claude Code / Cursor / VS / VS Code), и которые адаптер расширения
должен уметь распознать и превратить в task. Важно: почти каждый вход — это **естественный язык + список
требований + «рамка процесса»**, а не структурированная команда.

**Кластер A — «bootstrap/начать разработку с flow N»** (самый частый ядровой):
> «We would like to start the development from flow 33 - system initiation and then let the system
> complete all the rest work. Please plan the implementation in small phases - and after each phase let
> me download the code, and wait until I approve the next one. Please save the states to be able to
> continue from where you stopped.»
Часто с усилением: `Please read 33* files carefully!!! 34 will be at later stages, not now!!!` (явная
граница scope) и с указанием под-разбиения: `I need the phase 2 to be divided to 5 smaller phases`.

**Кластер B — «миграция/адаптация документации на стек»** (десятки почти-дубликатов):
> «I would like to translate the current xiigen implementation (flow 33) from python + react native to
> node js - nest.js and react.js. Please make a plan divided to 10 phases how to do that.»
Вариант с явным списком целевых артефактов: `...adapt the current documentation to the correct syntax:
SKILLS_FACTORY_RAG_MERGED, UNIFIED_SOURCE_INDEX_MERGED, V62_BFA_STRESS_TEST_MERGED,
TASK_TYPES_CATALOG_MERGED, ENGINE_ARCHITECTURE_MERGED, SESSION_STATE_MERGE, MASTER_EXECUTION_PLAN_MERGED,
RAG_INDEX, README, XIIGEN_MODULE_ARCHITECTURE_PROPOSAL`. Плюс continuation-хвост: `phase 1-8.2 are done
in "done so far" zip. Please proceed 8.3 phase only`.

**Кластер C — «единая skill-library для мульти-хостов»** (прямой прообраз нашего расширения):
> «I need a full documentation and a skillset ... organized in a library that different ai can use:
> copilot, claude ai, claude code, cursor... Please create for each skill an alternative in: For server:
> .Net 7/8/9, Php, Node.js, Rust, Python; For client: Angular, React (+redux), React Native.»

**Кластер D — «дизайн-фичи / архитектурная консультация»** (event-driven, i18n, feature-parity):
> «I need your help in designing new features. I need your help to design the multi language support of
> the site ...» — далее нумерованный технический контракт (React i18n → server IStorageContainer →
> marketplace delta → tenant/master AI-ключи → fallback EN). Это самый «богатый» вход: сам по себе несёт
> готовую под-оркестрацию потоков данных.

**Кластер E — «figma/css/plugin debug»** (продуктовая ветка расширений):
> «I would like to be able to debug the css that is generated from figma design, for that I need a full
> log of changes for each node where and why we established its css style ...» / «HTML styling
> discrepancies in plugin implementations» / «Node settings to CSS mapping matrix».

**Кластер F — «freemium-плагины / трафик / marketplace»** (продуктовая стратегия):
> «The goal is to deliver 20 freemium plugins to start driving traffic to our xiigen platform ... order
> these by their potential of gathering free traffic.»

#### 8.2.1. Две повторяющиеся «рамки-обёртки» (boilerplate-контракты входа)

Поверх содержательной части пользователь почти всегда прикладывает один из двух стандартных «контрактов
процесса». Расширение должно уметь их **распознать как метаданные задачи, а не как содержание** и
пронести в оркестрацию:

**(1) «basic prompt.txt / consolidation prompt.txt» — рамка фазового исполнения** (дословный
повторяющийся блок):
- «Please first make a **no code explanation** of what you are going to do - divide to phases»
- «Then **validate the plan** - it actually covers all the requirements in the prompt»
- «Then make **positive and negative examples** of the expected results»
- «For each phase - **short enough to save state** and recover with minimal time lost»
- «If needed, make a **small rag** of documents you have - or a **small skills factory**»
- «After each phase **make a zip / let me download / do not proceed until I approve**»
- «**Do not reset files** - first check what is already created and continue from there»
- «Please **save states** to be able to continue from where you stopped»

**(2) «8 delivery items» — рамка качества/инженерной дисциплины** (дословно в figma-adaptation,
plugin-theme, multi-tenant-validation промптах):
1. Project must compile, all tests must pass.
2. Автотесты: (a) каждый логический юнит отдельно; (b) симуляция всех потоков юнита; (c) e2e UI-тесты
   зелёные; (d) локальная Docker-альтернатива для e2e каждого интерфейса.
3. Планирование учитывает это **с самого начала**.
4. **No "other responsibility"** — если не работает, чиним логику, **не подгоняем тест** (retroactive
   development).
5. Все изменения отражены в локальных документах: doc, readme, flows, skills, agents.
6. Каждый баг = **новый набор тестов**: unit, simulation, e2e.
7. Сначала план (no code, ai prompts, positive/negative examples), **и только после одобрения Luba —
   разработка**.
8. Документируем всё в git и, если есть, в jira/Trello.

Эти две рамки — **инвариант product-owner требований Luba**. В расширении они становятся «стоячими
настройками задачи» (phase-gated исполнение + quality-gate), которые адаптер прикрепляет к каждому task
независимо от содержательного кластера.

---

### 8.3. Prompt → Orchestration mapping: канонический 5-шаговый скелет

Главная находка `sonnet - approved orchestration.md` (15 полностью разобранных ОДОБРЕННЫХ примеров):
**любой валидный промпт отображается в один и тот же 5-шаговый скелет оркестрации**, заполняемый
содержимым конкретного промпта. Это и есть контракт API-слоя `prompt → core-orchestration`:

```
ШАГ 1  agent-output-format-skill
       → Consumer = Claude Code; «три-файловое правило»:
         REFERENCE-PLAN.md + STATE.json + SESSION-N.md
ШАГ 2  infrastructure-discovery-skill
       → обнаружить существующий код/интерфейсы/тесты под задачу
         (context/interfaces.cs, skills/INDEX.md, XIIGen.* namespaces,
          FlowOrchestrator, Elasticsearch-индексы flows-definitions/state/results)
ШАГ 3  planning-skill Gates G0–G7
       G0 инвентаризация · G1 DNA safety (не сломать DataProcessResult<T>,
       CancellationToken, AddXIIGen{Feature}(), multi-tenant isolation)
       · G2 fabric-карта модулей/потоков · G3 шаблон плана под задачу
       · G4 лог технических решений · G5 тест-матрица (unit+simulation+e2e+docker)
       · G6 синхронизация doc/readme/flows/skills/agents · G7 принципы P1–P8
ШАГ 4  plan-review-skill SESSION-0
       → FC-1..FC-15 + два независимых AI-обзора + одобрение Luba
ШАГ 5  Выход = разбиение на SESSION-1 .. SESSION-N
       → одна SESSION = одна проверяемая фаза со state + download-checkpoint
Файл плана: REFERENCE-PLAN-<ТЕМА>.md
Файлы контекста: <явный список источников из промпта>
Результат: ОДОБРЕНО ✓
```

Примеры заполнения скелета (из ОДОБРЕННЫХ):
- **figma-skills-adaptation** → выход SESSION-1 (инвентаризация skills), SESSION-2..4 (адаптация
  planning/testing/documenting skills), SESSION-5 (Docker e2e).
- **multi-language i18n** → G2 fabric-карта `tenant IStorageContainer → marketplace IStorageContainer →
  AI translation → React i18n`; выход SESSION-1..5 (ITranslationService дизайн → server delta → React
  i18n → AI-провайдер → тесты/доки). Заметьте: богатый вход из §8.2 кластера D порождает подробную
  fabric-карту почти без «домысливания».
- **python→nodejs миграция 20 фаз** → выход 5 SESSION-ов, каждый содержит группу из 4 фаз.

**Спаривание с проходами 5–6:** ШАГ 4 использует **7-компонентную рубрику качества промпта A/B/C**
(проход 5 §5.2 P11: Load context · Scope · DNA · … со скорингом A≥2, B≥1, C≥1) как формальный вход-gate;
ШАГ 5 повторяет **повторяемый шаблон промпта** из прохода 6 §6.3 (FLOW-29/30/31/34) — там оркестрация
записана как исполняемые фазы. То есть: **проход 5 = чем мерить качество входа, проход 6 = как выглядит
эталонная развёрнутая оркестрация одного потока, проход 8 = какой сырой вход в эту оркестрацию попадает
и как он нормализуется.**

---

### 8.4. Что Sonnet нашёл при разборе промптов (accept/reject таксономия)

Это самая ценная часть для **gate-логики адаптера**. Sonnet разметил два независимых слоя фильтрации.

#### 8.4.1. Слой 1 — годен ли промпт как task-origin (`accepted`/`rejected user prompts`)

Из 94 отклонённых промптов таксономия причин (точный подсчёт):

| Причина | Кол-во | Смысл |
|---|---|---|
| `OFF_TOPIC` | 66 | не про xiigen вообще (иммиграция, визы, испанский, квантовые вычисления, LinkedIn-CV, D&B supplier risk, n8n Azure DevOps, чужие Angular-проекты) |
| `NO_TASK_ORIGIN_FOUND` | 10 | нет ни одного сообщения, где формулируется конкретный новый deliverable; всё — load/prep, review, коррекции, «continue» |
| `REVIEW_ONLY` | 9 | все содержательные сообщения — «please review …» уже готового артефакта, а не запрос на создание |
| `CONTINUATION_COMMAND` | 9 | операнд — «proceed phase 8.3 only» / «continue» при уже готовом плане в STATE.json (паттерн N-06) |

**Вывод:** валидный вход = **самодостаточное сообщение-источник задачи, называющее конкретный новый
deliverable**. Off-topic, «только ревью», «только продолжай» и «нет источника задачи» — это НЕ входы для
оркестрации. Для расширения это фильтр первого уровня: адаптер не запускает планировщик на review-only /
continuation / off-topic вход.

#### 8.4.2. Слой 2 — можно ли построить корректную оркестрацию (`declined orchestration`, 63 записи)

Даже on-topic промпт может не дать корректную оркестрацию. Причины отклонения (кластеры):
- **Continuation hook** — `Message index: 3/5/7`, «Following the current documentation», «based on
  history documents and gap closure plan», «We have executed phases a-C», «Now - please analyze your
  inputs», «Ok let's continue to check the design». Промпт не самодостаточен — крючок на предыдущий
  контекст сессии.
- **Unreferenceable prior artifact** — «Please load the session», «Please load this plan», «Please load
  the session documents following session start prompt», «Please load the skills». Ссылка на артефакт
  без пути в репозитории.
- **Unresolvable attachment** — «attached text», «xml attached», «follow carefully basic_prompt.txt»
  (файла нет в репо), «28-* documents attached», «See attached marketplace-publishing files»,
  «Attaching a plugin that we already have». Вложение не разрешимо по конкретному имени/пути.
- **Question / consultation** — «how can I give you access?», «Is that an interesting challenge? What do
  you think? How we would approach this?». Первичное действие — вопрос/обсуждение, а не запрос плана.
- **Too vague** — «make documentation» / «load to session and suggest the best version» без конкретики
  что документировать, какие файлы, какая структура.
- **«Given the current code state» hook** — «Given the current state of the code 'xiigen mvp'» как
  первичный контекстный крючок (подразумевает незафиксированное состояние).

#### 8.4.3. Механизм «Standalone версия» — контракт нормализации входа (ГЛАВНОЕ для адаптера)

Каждая declined-запись содержит поле **«Повторная попытка: Standalone версия»** — как переписать
некорректный вход в самодостаточный исполнимый. Это **буквально то, что должен делать prompt-адаптер
расширения**: взять расплывчатый/контекстно-зависимый промпт хоста и нормализовать его в standalone-task,
который принимает core-оркестрация. Схема нормализации (выведена из всех 63 переписываний):

1. **Разрешить все ссылки в конкретные пути** — «current documentation» → `docs/self-learning-cycles/`
   + `flows-definitions` index; «the session» → `.claude/worktrees/vigorous-margulis/docs/sessions/
   historyRag/`; «task 17» → `XIIGen.Tests` / `docs/tasks/task-17/`.
2. **Назвать явные deliverables** — вместо «make documentation» → «(1) CLAUDE.md-overview + arch diagram,
   (2) skills/INDEX.md со списком skills purpose/class/deps/DI, (3) session startup instructions —
   три конкретных файла, готовых к git commit».
3. **Развернуть вопрос/консультацию в план** — «what do you think?» → «Create a feature parity
   management plan ... Deliverables: (1) audit … (2) matrix template … (3) workflow … (4)
   REFERENCE-PLAN-*.md … (5) SESSION-1 steps».
4. **Пришить исполняемую структуру** — каждый standalone заканчивается `REFERENCE-PLAN-<ТЕМА>.md` +
   `SESSION-1..N` с gate-командами и STOP-маркерами (тот же скелет §8.3 ШАГ 5).
5. **Сохранить содержательный контракт из входа** — нумерованные требования пользователя (напр. i18n
   delta-логика, 8 delivery items) переносятся в план дословно, а не выбрасываются.

Формула: **`vague/contextual host prompt → (resolve refs · name deliverables · unfold to plan · attach
SESSION structure · preserve user contract) → standalone task → 5-шаговая оркестрация §8.3`**. Провал
любого пункта 1–3 = адаптер должен либо дозапросить у пользователя недостающий путь/deliverable, либо
переписать сам (если контекст доступен), но **не запускать оркестрацию на нерешаемом входе**.

---

### 8.5. Pipeline-скрипты — как корпус собирается механически (модель ingest для расширения)

Три скрипта образуют полный конвейер сборки датасета «промпт→оркестрация». Это прообраз того, как
расширение будет **само** ингестить и строить свои обучающие данные из истории сессий хоста.

**(1) `_tools/export_project_conversation_map_from_claude_console.js`** — браузерный DevTools-скрипт.
Причина существования (дословно в шапке): ZIP-экспорт аккаунта Anthropic содержит `conversations.json`
и `projects.json`, **но не содержит надёжного маппинга conversation→project**. Скрипт логинится через
cookie, определяет `organizationId` (cookie `lastActiveOrg` → local/sessionStorage → ручной ввод),
и по каждому проекту из хардкод-списка `PROJECTS_FROM_EXPORT` (там есть `xiigen`, `Immigration`,
`Испанский`, `Французский`, `content generation` и т.д.) тянет `/api/organizations/{orgId}/projects/
{uuid}/conversations_v2?limit=1000&offset=…` c пагинацией. На выходе скачивает
`claude-project-conversation-map.json` + `.csv` (поля: project_uuid, project_name, conversation_uuid,
conversation_name, created_at, updated_at, model).

**(2) `_tools/apply_project_conversation_map.py`** — применяет этот маппинг к разбитым markdown-сессиям.
Читает `_manifest.csv`, для каждой сессии подставляет `project_name`/`project_uuid` (если нет — попадает
в `Uncategorized`), и **копирует** файл в `claude-sessions-mapped/<project>/<date_folder>/<session_id>/
<session_name>.md`, переписывая манифест с колонкой `project`. Жёсткое **правило безопасности**: скрипт
отказывается писать внутрь `claude-sessions/` (сравнивает `output_root` с `sessions_root`, `relative_to`
→ refuse). Санитизация имён (`clean_segment`: убирает `<>:"/\|?*`, схлопывает пробелы, обрезает до 120
символов). То есть: **исходный корпус read-only, производный — в отдельную папку**.

**(3) `build_session_json.py` (корень папки)** — строит рабочую очередь `SESSION-ANALYZE.json`
(тот самый вход для воркеров extraction-протокола, разобранного в проходах 6 §6.2 и 7). Логика:
- читает `_manifest.csv` (session_id, session_name, message_count, file_path, created_at_local);
- `categorize(name, msgs)`: сначала `personal_kw` (immigration/spanish/french/hebrew/career/visa/resume/
  linkedin…) → `personal, skip`; `msgs<=2` → `other, skip`; иначе keyword-матч `xiigen_kw` (xiigen/flow/
  simulation/pipeline/architect/skill/orchestrat/figma/plugin/rag/executor/dpo/arbiter/fabric/freedom…)
  ∪ `figma_kw` ∪ `migration_kw`;
- если xiigen/figma/migration: `msgs>=40 → high`, `>=15 → medium`, `>=6 → low`, иначе `skip`;
- помечает уже обработанные (хардкод-список `processed_names` из ~65 названий) как `processed`;
- сортирует pending по (priority desc, message_count desc) и **режет на батчи по 50** (`batch-NN`);
- пишет `SESSION-ANALYZE.json` с metadata (total/processed/pending_high/medium/low/skip/total_batches).

**Полный конвейер целиком:**
```
ZIP-экспорт аккаунта → split в per-session .md + _manifest.csv
   → (браузер) export_project_conversation_map.js  [conversation→project mapping]
   → apply_project_conversation_map.py             [фильтр в xiigen-проект, копия в *-mapped/]
   → build_session_json.py                          [категоризация+приоритет+батчи-по-50]
   → SESSION-ANALYZE.json                           [рабочая очередь воркеров]
   → ORCHESTRATION-EXTRACTION-PROTOCOL (проход 6)   [per-batch извлечение промпт→оркестрация]
   → triage CSV (_triage 916 → _workset 191 → _toprocess 172)  [ручной отбор relevant]
   → sonnet accept/reject prompts + approved/declined orchestration (§8.3–8.4)
```
Triage-CSV — это «воронка»: `_triage.csv` 916 (весь список Folder/UUID/Name/MessageCount/FilePath) →
`_workset.csv` 191 (xiigen-relevant, отсортированы по MessageCount: топ — «Xiigen flow 01 testing …»
462 сообщения, «Loading xiigen session …» 305) → `_toprocess.csv` 172 (финальная очередь на разбор).

**Смысл для расширения:** тот же конвейер применим внутри хоста — история сессий пользователя (Claude
Code / Cursor / VS) фильтруется по проекту, категоризируется по keyword+объёму, режется на батчи, из
каждой сессии извлекается пара «первый промпт → построенная оркестрация», размечается accept/reject, и
это становится **локальными адаптивными обучающими данными** prompt-input адаптера. Гейт безопасности из
скрипта (2) — прямой прообраз «не писать в исходный корпус» / изоляции пользовательских данных.

---

### 8.6. Итог prompt-input слоя: контракт адаптера расширения

Собирая проходы 5, 6, 8, prompt-input API-слой расширения выглядит так:

```
host user prompt (естественный язык + требования + рамка-обёртка §8.2.1)
        │
        ▼  [ADAPTER ГЕЙТ 1: task-origin]           ← таксономия §8.4.1
   off-topic / review-only / continuation / no-task-origin  → НЕ запускать оркестрацию
        │ (валидный источник задачи)
        ▼  [ADAPTER ГЕЙТ 2: самодостаточность]      ← таксономия §8.4.2
   continuation-hook / unresolvable-attachment / unreferenceable-artifact /
   question / too-vague  → нормализовать в «Standalone версию» §8.4.3
        │ (standalone task)
        ▼  [ADAPTER ОЦЕНКА]  7-компонентная рубрика A/B/C (проход 5 §5.2 P11)
        │
        ▼  [CORE ОРКЕСТРАЦИЯ]  5-шаговый скелет §8.3
   agent-output-format → infrastructure-discovery → planning G0–G7 →
   plan-review FC-1..15 + 2 AI + Luba → SESSION-1..N
        │  (+ рамки-обёртки §8.2.1 как стоячие настройки: phase-gate + 8 delivery-items quality-gate)
        ▼
   REFERENCE-PLAN-<ТЕМА>.md + STATE.json + SESSION-N.md   (три-файловый выход)
```

Тонкость границы (соответствует R6 «Simple Agent Boundary» из CLAUDE.md): **адаптер делает только
gate + resolve + normalize + прикрепление контракта**; вся содержательная логика (discovery, planning
gates, review FC-checks, разбиение на SESSION) живёт в core (`llm_mvp_core`). Промпт-корпус и sonnet-
разметка из этого прохода — это **обучающий набор именно для адаптерного gate/normalize слоя**, не для
core.

---

### 8.7. Честность о покрытии прохода 8 + что остаётся для проходов 9–10

**Прочитано построчно (полностью):** `user prompts/accepted orchestration pattertns.md` (целиком),
`sonnet - approved orchestration.md` (все 15 записей, целиком), `sonnet - rejected user prompts.md`
(первые ~94 записи по причинам + полная таксономия-подсчёт по всему файлу), `sonnet - declined
orchestration.md` (первые ~24 записи целиком + все 63 причины отклонения через grep-срез),
`build_session_json.py` (целиком), `apply_project_conversation_map.py` (целиком),
`export_project_conversation_map_from_claude_console.js` (целиком), заголовки+сэмплы всех трёх triage-CSV.
`user prompts/user prompts.md` — прочитаны записи 1–400 и 700–940 построчно (охват всех кластеров форм
A–F §8.2), плюс подсчёт 103 записей; средние ~300 строк не читались построчно (это near-дубликаты
continuation-вариантов кластеров A/B — те же формы с иным номером фазы, ценность инкорпорирована в §8.2).

**НЕ покрыто пофайлово (честно):** `sonnet - accepted user prompts.md` — прочитаны первые 78→~330 строк
(все классы форм видны; полный список 78 принятых промптов — это те же кластеры A–F, адресно берётся при
надобности). Средние continuation-дубликаты `user prompts.md` (≈строки 400–700, часть 940+). Полные тела
declined-записей 25–63 (причины отклонения все извлечены; развёрнутые «Standalone версии» этих 39 записей
— тот же механизм нормализации §8.4.3, читаются адресно).

**Остаётся для проходов 9–10 (по рамке §5 / проходов 6–7):**
- **Проход 9:** выборочно крупнейшие `Uncategorized`-сессии по `SESSION-INDEX.md` (msg_count
  462 «Xiigen flow 01 testing», 305 «Loading xiigen session», 279 «Self-learning cycles»), которых нет
  в Частях 1–35 `SESSION-ANALYSIS.md` — чтобы поднять реальные многошаговые диалоги «промпт → коррекции
  → финальная оркестрация» (динамика, которой нет в статичном корпусе первых сообщений). При бюджете —
  `FILE-ANALYSIS-PROTOCOL-v1.md` (18 МБ) точечно, если нужен агрегированный per-file слой.
- **Проход 10:** сборка/сверка итогового RAG-навигатора по всем 9 файлам RAG-01..09, финальная карта
  «prompt-input (проход 8) → orchestration-catalog (проходы 3–4,6) → invariants/QA (RAG-06, проход 5) →
  extension continue-plan», и явный чеклист что из claude-sessions переносится в план продолжения VS/VS
  Code расширений.

**RAG-файл (проход 8):** `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code
continue/RAG-09-claude-sessions.md` (проход 8 добавлен как отдельная секция «## Pass 8/10 …»; разделы
1–7 не тронуты).


---

## Pass 9/10 — no-neglect sweep крупнейших непокрытых сессий + честное покрытие 1209 файлов

> Проход 9 закрывает правило Любы «no neglect»: до этого глубоко читались
> оркестраторные подпапки (`claude/grok orchestrator`), протоколы извлечения,
> prompt-корпус и sonnet-разметка. Оставались непрочитанными **самые крупные
> сырые тела сессий** в `Uncategorized/` (462 / 305 / 279 / 204 / 190 / 184 …
> сообщений) и не было доказательной проверки, что кластеры «OLD UNRELATED»
> (Angular/NestJS/Hebrew/иммиграция) действительно нерелевантны. Этот проход
> открыл крупнейшие тела, извлёк их содержание и **скорректировал прежнее
> заявление о покрытии** там, где оно оказалось неточным.

### 9.0. Метод прохода (почему это не «categorized-only», а реальное чтение)

Каждый файл-сессия имеет жёсткую структуру (проверено на `Xiigen flow 01
testing…md`, 108 044 строк / 14 МБ):

```
--- YAML frontmatter (session_id, message_count, source_archive) ---
# <Название сессии>
## Summary        ← 3–5 плотных абзацев человеческого дистиллята всей сессии
## Messages
## 1. Human   `created … message_id …`
## 2. Assistant …
… (462 сообщения)
```

Блок `## Summary` — это **не оглавление, а полноценная выжимка** всей многошаговой
беседы: фазы, принятые решения, номера T/F/CF/SK, доменные термины, паттерн
поведения Любы. Для тел 6–14 МБ построчное чтение 40–108 тыс. строк невозможно и
бессмысленно (это 90 % near-дубликатов итераций «версия v3→v4→v5»); **правильная
единица чтения — Summary + адресный grep по телу**. Ниже извлечены Summary всех
крупнейших непокрытых сессий (границы: от `## Summary` до первого `## N. Human`).

### 9.1. Крупнейшие непокрытые XIIGen-сессии — что реально внутри (с доказательствами)

**462 — «Xiigen flow 01 testing and architecture review»**
(`Uncategorized/05.04.2026 18.36/cf930197-…`, 462 сообщения, 14 МБ, 05–12.04.2026).
Ветка `claude/vigorous-margulis` репо `xiigen-mvp-claude-vigorous-margulis`.
Содержание: 7 итераций (v1→v7) плана реализации FLOW-36…40 + FLOW-45; удаление
всех human-stop-gate в пользу **AUTOMATED GATE PROTOCOL** (tsc → full server jest
→ full client jest → Playwright → commit → push); scope-таксономия (каждый
документ обязан нести `connectionType` + `knowledgeScope`); три scope: GLOBAL
(`FLOW_SCOPED`/`GLOBAL`/`MASTER_TENANT_ID`), PRIVATE (`knowledgeScope=PRIVATE`,
tenantId из ALS), MODULE_SCOPED (transient, не персистится). Границы артефактов:
`Next T=T605, F=F1519, CF=CF-809, SK=SK-529`. Вторая фаза — корень бага «Cycle 2
даёт ноль NODE» = пустой `constraints[]` → `CONVERGENCE_MISSING_CONSTRAINTS` (не
баг проводки), фикс = новый `flow-constraints.ts` с iron-rules на поток. Затем
**рекурсивная top-down design-simulation методология** FLOW-01 (документы R1–R4):
раунды **порождают потоки, а не разделы анализа**; узлы EXPAND становятся
subflow в следующем раунде; каждый «что может пойти не так» кладётся в arbiter
или subflow (никогда в заметку); семиуровневая лестница user-request → records →
events → subflow-paths → path-merges → implementation → tests → training-corpus.
Концепт **flow library**: все потоки, спроектированные XIIGen на claude-sonnet-4-6,
публикуются как GLOBAL-шаблоны, любой tenant копирует и адаптирует (MACHINE-правила
залочены, FREEDOM-ключи настраиваемы). Финал — `XIIGEN-DESIGN-ARCHITECT-SESSION-
GUIDE-v1.md` (гайд, порождающий «design co-architect», а не «code developer»).
Термины: NODE (structure/intent/constraints/quality), LEAF vs EXPAND, arbiter-панели
(IronRules, Security, Domain, Business, Completeness, Prompts, Skills,
scope_isolation), MACHINE vs FREEDOM, DNA-3/5/8/9, CF-POLICY-01 (отсутствие
knowledgeScope → PRIVATE), три обучающих канала CODE_REASONING / DESIGN_REASONING /
CONVERGENCE_REASONING, H0-override (прямая инструкция Любы бьёт governance-дефолт).

**305 — «Loading xiigen session and architecture skills»**
(`05.04.2026 09.16/c356c556-…`, 305 сообщений, 8.4 МБ). Реализация FLOW-06/07/08
на NestJS 10 + TS 5 / React 18 + Vite 5. Ключевое: **два конфликтующих T-номерных
пространства** (PLAN-STATE-файлы vs engine-контракты) → single-source-of-truth =
engine-контракты + `TaskTypeRegistry`; чистка удаления FLOW-01 (осиротевшие T47–T49
в 4 файлах); порядок реализации FLOW-08 → 07 → 06 → повторный FLOW-01 через
`cycle-chain/run`; скрытая зависимость: `ListingDiscoveryEngine` (FLOW-08) слушает
событие ровно `"OnboardingCompleted"` из FLOW-01 T49; FLOW-01 удалён, потому что
построен на неверном пайплайне (`flow/execute` вместо `cycle-chain/run`).
Артефакты: `PLAN-FLOW-06-07-08-IMPLEMENTATION-v3`, `XIIGEN-SESSION-LOAD-PLAN-v12…v15`,
**SK-526 (scope isolation arbiter, обязателен на каждом узле)**, HOW-TO-USE-SKILLS
v3.1.0, LIVE-ENGINE-RUN-PROTOCOL, GAP-SCOPE-01…05. Живой прогон FLOW-01 вскрыл две
проблемы: mock-режим даёт идеальный grade 1.00 (реальные провайдеры не запускались)
и разрыв Cycle 1 → Cycle 2 (10 plan-steps, 0 Cycle 2 rounds). Мета-вывод:
`convergence.handler` должен применять собственный протокол движка
`CONTEXT_INSUFFICIENT` к самому себе перед генерацией. Родились планы FLOW-36…40,
recursive-orchestration-plan v2, **SK-528 (pipeline-position check + Q0 gate)**,
апдейты SK-452 и agent-constitution. Подтверждён ментальный модель: потоки-как-узлы,
навыки-как-узлы, арбитры-как-evaluator-узлы, **судья (judge) — top-level
orchestrator-узел**.

**279 — «Self-learning cycles development plan validation»**
(`19.03.2026 19.28/8b6eabd1-…`, 279 сообщений, 6.8 МБ). Продуктовые варианты XIIGen:
**xiigen-saas (MODE-A hosted portal), xiigen-oss (MODE-B-full OSS), xiigen-enterprise
(MODE-B-full .NET desktop), xiigen-lean (MODE-B-full React Native + Python)**. Решение
D-34-1: FLOW-34 делает только MODE-B-thin адаптеры; enterprise получает FlowBundle +
NestJS reference как guidance, не runtime. **Feature Registry**: поле `productScope`
(«client-capability» vs «xiigen-capability»), расширенный `platformId`, сигнал
`XIIGEN_VARIANT`. Опция B для capabilities-манифеста (все возможности как stubs со
статусом NOT_STARTED). Массово произведены flow-reference-планы FLOW-03/05/07/09/11/13
(Waves 2–3, T-диапазоны). Новые навыки: **SK-434 ParallelWaveCoordinator, SK-435
ProductVariantRouter, SK-436 BundleVersionGuard**; SK-418 → v1.1 (23-item checklist,
V16–V23 visibility-gates) → v1.2 (V24–V28 client-side). Клиентская архитектура:
`CLIENT-SIDE-ARCHITECTURE-SPEC.md` (offline queue, draft/abandoned session state,
background-step → client-signal, полный `useFlow` SDK-контракт), `CLIENT-TESTING-
PLAN.md`. Паттерн Любы: параллельные Claude Code сессии, точечные заказы («делай
только Flow 10»), правка «ленивых» ответов (репак старого без нового контента),
устранение всех плейсхолдеров «see v5».

**204 — «Preparing flow 01-24 for development with Claude code»**
(`10.04.2026 10.12/f3f0753b-…`, 204 сообщения, 12 МБ). Co-architect mode, guide v1.8.
FLOW-15/16/17 (SaaS Multi-Tenancy T221–224, Marketplace Payments T225–228, Freelancer
Marketplace T229–232). Цикл: **prepare → ⛔STOP → review → fix** (каждый как отдельная
инструкция). Термины реализации: `arbiterConfig`-блоки, scope_isolation arbiters
(FC-32), DPO-триплы (V9-002/003), DNA-8 (audit before enqueue), SETNX-идемпотентность,
OCC через `storeDocumentWithOCC`, LIFO saga-компенсация, FREEDOM-config TTL,
machineComponents vs freedomComponents, `appliesTo` future-flow валидация,
`curriculumTier`/`qualityScore` поля корпуса, PLATFORM_ONLY vs INJECTABLE фабрики,
анти-паттерн `isolated:false`. Устойчивые правила-коррекции (перенесены во все
следующие документы): скобки в bash-fallback-цепочках; flownum boundary-checks
(`>=16/17/18`) в `appliesTo`; scope_isolation count `>=4` (по одному на узел, не `>=1`);
позитивные machineComponents-проверки вместо негативных assert; source-path
`analytics-warehouse` не `data-warehouse-analytics`.

**190 + 156 — «Xiigen architect session setup»** (два, `12.04.2026 18.44/18.45`).
Summary 190-сессии — это **чистые QA-таблицы разрывов** (пример реального содержания,
а не абстракция): FLOW-07-TEACH-QA и FLOW-08-TEACH-QA перечисляют HIGH/MEDIUM gaps —
отсутствие 8-й `scope_isolation` NDJSON-записи при 7 domain-арбитрах; `grep … | head -5`
как silent no-op (guide #16); **negative-assertion traps** («does NOT contain
'requestId'», хотя корректное правило именно содержит слово `requestId`); хардкод
SEED-21..25 в трёх местах вместо чтения файла; PLAN_EXEMPLAR-фантом. Это доказывает,
что «architect session setup» = сессии ревью качества TEACH-QA/Implementation-планов,
а не пустой setup.

**184 — «Co-architect collaboration and planning»** (`16.04.2026 09.20/ff6038c4-…`).
Пример co-architect-вердикта: план v1 «95 % confirming, 5 % correcting» — две мелкие
правки: (1) prereq-фильтр в `seedGlobalTopologies()` (одна строка: пропускать
`prereq-*` файлы, чтобы engine-internal потоки не попадали в `xiigen-flow-templates`);
(2) Turn 6 — `TopologyViewer` node-click drill-through в AF-pipeline. Демонстрирует
**поведение «архитектор-ревьюер», а не «кодер»** — подтвердить форму плана, точечно
поправить, `Proceed`.

**Прочие крупные (152/150/145), охваченные адресно:** «Master plan 04 update» (152),
«Simulation planning session with skill file requirements» (152/108/87 — три версии),
«Session prompt instructions» (150), «Loading xiigen architect and planning skills»
(145/126) — все того же класса: загрузка architect/planning-навыков, симуляционное
планирование потоков, обновление master-plan. Новых **архитектурных примитивов сверх**
уже зафиксированных в Частях 1–35 SESSION-ANALYSIS и в проходах 3–8 они не вводят
(тот же NODE/arbiter/MACHINE-FREEDOM/cycle-chain словарь), поэтому засчитаны как
categorized+summary-verified, без построчного дубля.

### 9.2. Что из этих тел — НОВЫЙ orchestration/extension-материал (ещё не в RAG)

Дельта поверх проходов 1–8 (то, что даёт крупные тела и чего не было в статичном
корпусе первых сообщений):

1. **AUTOMATED GATE PROTOCOL** дословно: `tsc → server jest → client jest →
   Playwright → commit → push` — готовый gate-конвейер для VS/VS Code executor-плана
   (соответствует «Gate B per placement» из CLAUDE.md).
2. **Трёхscope data-classification** (`connectionType` + `knowledgeScope`:
   GLOBAL/PRIVATE/MODULE_SCOPED, CF-POLICY-01 «absent → PRIVATE») — прямой прообраз
   разделения common-модель-в-репо vs adaptive-user-модель (R5 CLAUDE.md) для расширения.
3. **judge = top-level orchestrator-узел; flows/skills/arbiters = узлы** — единая
   node-абстракция всей оркестрации (305-сессия), это тот самый orchestrator/judge
   слой, что нужен плану расширений.
4. **`CONTEXT_INSUFFICIENT` рекурсивно к самому convergence.handler** — движок обязан
   применить свой же протокол недостатка контекста к себе до генерации (антипаттерн
   «отвечать на неверный вопрос»).
5. **flow-constraints.ts / пустой constraints[] → CONVERGENCE_MISSING_CONSTRAINTS** —
   конкретный корень «нулевого NODE», полезно как negative-test для рантайма.
6. **SK-526/528/452, SK-434/435/436** — навыки координации волн, product-variant
   роутинга, bundle-version guard, scope-isolation arbiter, pipeline-position/Q0 gate.
7. **useFlow SDK-контракт + offline queue + draft state + background→client signal** —
   клиентская сторона расширения (что именно возвращает host-адаптер пользователю).

### 9.3. КОРРЕКЦИЯ прежнего заявления о покрытии: кластеры «OLD UNRELATED» — что реально (доказательно)

Прежние проходы отнесли «Angular/NestJS/Technical Debugging» и «Personal/Other» к
нерелевантным. **Проверка это частично опровергает** — категоризация SESSION-INDEX
грубая (по названию), и релевантность надо судить по содержанию, а не по бакету.

**(A) Genuinely UNRELATED — подтверждено с доказательствами (можно смело пропускать):**
- **135 «Канадская иммиграция: требования к образованию»** — личная иммиграция Любы
  Karpenko (NOC 21231, PNP, Nova Scotia NSNP, Saskatchewan SINP OID, APEGS, Manitoba
  MPNP, Quebec PSTQ). Ноль orchestration. Evidence: Summary — «continued an active
  Canadian immigration application process… provincial nominee program (PNP) pathways».
- **118 «Express Entry заявка в Канаду»** — личная иммиграция (FSWP 67-point grid,
  IELTS→CLB, ICT via Genie Soft branch, EB-2 NIW). Ноль orchestration. (Побочно:
  упомянут «a Figma plugin with 177,000+ downloads» — биография Любы, не задача.)
- Прочие в Personal/Other по названию явно личные/непрофильные: **79 «Виза талантов
  в США», 56 «Самые цитируемые испанские слова», 50 «Bion's Network Theory», 48/45
  ивритские (анимация к презентации; проверка рисков поставщиков через Dun&Bradstreet
  MCP), 32 «Квантовые вычисления: основы», 30 «Исправления формы I-140», 26 «Способы
  восстановления озонового слоя», 24 …**. Это разнотемье личных консультаций.
- В «Angular/NestJS»: **17 «Подготовка к интервью по .NET Core и React Native»** —
  личная подготовка к собеседованию, нерелевантно.

**(B) МИС-категоризовано как «unrelated», но реально РЕЛЕВАНТНО (это и есть коррекция):**
- **108 «Debugging and improving CSS code»** (лежит в Angular/NestJS) — на деле
  **Figma-to-code CSS-движок**: ML-классификаторы (K-Means, Linear Regression, Pattern
  Matcher, Naive Bayes) в CSS-пайплайне; **мульти-провайдер AI enhancement
  (Claude/OpenAI/Gemini) с orchestrator + judge паттерном (Phase 7)** поверх существующего
  `IDesignAiProvider`; правило приватности `clientStorage` vs `setPluginData`; артефакты
  `AIEnhanceOrchestrator.ts`, `AIEnhanceJudge.ts`, `CombineResults.ts`, `classifiers.ts`,
  `scene-memory.ts`; `UNIFIED-PLAN-FINAL.md` (46 микро-фаз). Это **прямой design-to-code +
  orchestrator/judge материал**, релевантный расширениям.
- **101 «Improving design validation and CSS testing», 34 «Node settings to CSS mapping
  matrix», 29 «CSS creation algorithm improvement», 27/26/15/12 CSS-сессии** — тот же
  Figma-плагин design-engine кластер (~несколько сотен сообщений суммарно), не «Angular
  debugging».
- **152 «Documentation and skills for Claude Code»** (лежит в Personal/Other) — на деле
  **Figma-плагин Design Generator debugging**: `IDesignAiProvider.ts`,
  `ClaudeComparisonProvider.ts`, `OpenAIComparisonProvider.ts`, независимый `scoreOnly()`
  scoring с explicit deduction-rubrics + Scoring Mode toggle (Independent vs Self-reported),
  `isCanvasBlank()`/`wouldOverlapSibling()`/`cropToContent()` guards. Мульти-провайдер
  оценочный (judge) слой — релевантно.
- **90 «Managing feature parity across multiple platform implementations»** (Personal/Other)
  — на деле XIIGen flow-reference-планы (FLOW-10/12), SK-416…436, `useFlow` hook-контракт,
  offline queue, C1–C5 mock-тесты, и **правило human-readable-companion**: машинные
  идентификаторы (`flowId:"FLOW-33"`, `taskTypeId:"T521"`) всегда парны человекочитаемым
  (`flowName`, `taskTypeName`) во всех выходах (логи, Jira, DPO-триплы) — прямой аналог
  правила CLAUDE.md «машинные метки только после человеческого объяснения».
- Прочее в Personal/Other, реально XIIGen: **98 «Debug 2 Opus», 96 «Testing task 17»,
  66/64 «Testing strategy for Claude code MVP», 57 «Node matrix design fixes», 42 «Login
  HTML preview gap», 30 «Local RAG and open source LLM benchmarking», 28 «Event-driven user
  registration design», 26 «Structured session planning and codebase simulation», 26
  «Continuing design session», 25 «Phased project execution with state management»**.

**Вывод коррекции:** прежнее «OLD UNRELATED = Angular/NestJS/Hebrew/immigration» верно
**только** для immigration + иврит-анимация + личные разнотемные консультации (испанский,
квантовая физика, озон, Bion, I-140, виза талантов, интервью-подготовка). Кластер
**«CSS/design/Angular-debugging» и часть «Personal/Other» содержат крупный design-to-code
+ orchestrator/judge + flow-plan слой, который РЕЛЕВАНТЕН** и который проходы 1–8 уже
покрыли **по существу** через: RAG-04 (design-to-code сессия), проход 5 (figma-to-code /
flow-26 сценарии), проходы 3–4 (Figma→Canva/Miro/Webflow адаптеры, DRAWIO→PLAN). То есть
контент не потерян — он был захвачен через тематические подпапки, просто SESSION-INDEX
раскладывает те же темы по грубым бакетам с вводящими в заблуждение именами.

### 9.4. Слой FILE-ANALYSIS-PROTOCOL-v1 (18 МБ) + `_tmp_row_*` (28 файлов) — что это и почему не orchestration-material

- **`FILE-ANALYSIS-PROTOCOL-v1.md`** (18 МБ) — не сессия и не оркестрация, а
  **агрегированный per-file протокол разбора**, составленный на анализе 10 ключевых
  файлов. Его ценность — он явно фиксирует **3-слойную структуру всей папки**: Слой 1 =
  сырые 968 сессий Claude о разработке XIIGen (`Uncategorized/дата/session_id/*.md`,
  экспорт из claude.ai, потому все в «общей куче»); Слой 2 = инструменты/протоколы; Слой 3
  = индексы/аналитика (119 паттернов, SESSION-ANALYSIS.md). И главный пайплайн:
  `_manifest.csv → build_session_json.py → SESSION-ANALYZE.json (реестр 968) → 8 агентов
  по AGENT-INSTRUCTIONS.md (по 50 сессий) → SESSION-ANALYZE-RESULTS-batch-01..08.json →
  SESSION-ANALYSIS.md (119 паттернов)`. Это метод-слой (уже отражён в проходах 1–2, 6–7).
- **`_tmp_row_915…944.txt`** (28 файлов в корне) — **не claude-sessions диалоги**, а
  per-file forensic-строки ДРУГОГО корпуса: `platform development\extracted_v18\
  V17-skills\…\*.cs` (например `AnalyticsService.cs` навыка 48: 57 642 байта, MD5,
  интерфейс `IAnalyticsService` ровно 20 методов/6 групп, DNA-маркеры, DI Scoped, НЕ
  наследует MicroserviceBase, заглушка `CheckAlertsAsync`). Это тот же **forensic
  «размер+MD5+манифест vs код» метод**, что зафиксирован в проходе 7 (sonnet-forensic-audit),
  применённый к .NET-коду библиотеки навыков — метод-доказательство, не новый
  orchestration-примитив. Засчитано categorized+method-known.

### 9.5. Финальное честное заявление о покрытии всех 1209 файлов

Реальный состав папки (проверено `find`): **1209 файлов = 968 (`Uncategorized/*.md`
сессии) + 65 grok-orchestrator + 56 sonnet-analyzes + 53 claude-orchestrator + 6
prompt-orchestration + 7 sonnet-user-prompts + 2 user-prompts + 3 _tools + 49 root**
(root = 28 `_tmp_row` + SESSION-ANALYSIS.md + SESSION-INDEX.md + 4 протокола + 8 batch-JSON
+ SESSION-ANALYZE.json + _manifest.csv + build_session_json.py + README/AGENT-INSTRUCTIONS +
samples of orchestartion examinations.md).

| Слой (файлов) | Уровень покрытия | Где в RAG | Почему это полно, не пренебрежительно |
|---|---|---|---|
| claude orchestrator (53) | ГЛУБОКО построчно | Проход 3 | FLOW-01..42 + FLOW-31 Design Intelligence + scenario-synthesis — весь каталог потоков |
| grok orchestrator (65) | ГЛУБОКО построчно | Проход 4 | Kernel + flows 00-42 + DRAWIO→PLAN + Figma→Canva/Miro/Webflow адаптеры |
| scenario examinations (samples) | ГЛУБОКО | Проход 5 | figma-to-code / csharp-skill-library / flow-26 мульти-pass |
| extraction protocols (4) + prompt-orchestration (6) | ГЛУБОКО | Проход 6 | метод + prompt-слой извлечения оркестрации |
| sonnet - analyzes (56) | ГЛУБОКО | Проход 7 | модельно-аналитический forensic-слой (метод «манифест vs код») |
| sonnet-user-prompts (7) + user prompts (2) | ГЛУБОКО | Проход 8 | prompt-input API-слой расширения (103 промпта, accept/reject таксономия) |
| SESSION-ANALYSIS.md (1) | ГЛУБОКО | Проход 2 | мастер-дистиллят: 24 части / 56 (в файле 119) паттернов оркестрации |
| Uncategorized 968 сессий | КАТЕГОРИЗАЦИЯ ВСЕХ 968 + Summary-verified крупнейших | Проходы 1, 9 | 100 % расклассифицированы (проход 1); крупнейшие тела (462/305/279/204/190/184/152/90/108…) прочитаны через Summary+grep (проход 9) |
| FILE-ANALYSIS-PROTOCOL (1, 18 МБ) + `_tmp_row` (28) | ХАРАКТЕРИЗОВАНО | Проход 9 | 3-слойная карта папки + пайплайн; forensic-строки V17-skills (метод известен) |
| tools/JSON/manifest (root) | ХАРАКТЕРИЗОВАНО | Проходы 1, 6 | build_session_json.py, batch-JSON, manifest — инфраструктура пайплайна |

**Честная граница «глубоко vs категоризация»:** из 968 сырых сессий построчно (Summary +
адресный grep) прочитаны **все крупнейшие тела** (топ по message_count: 462, 305, 279, 204,
190, 184, 156, 152×3, 150, 145, 135, 126, 118, 108, 101…) плюс кластерные представители
CSS/design и immigration для доказательной проверки релевантности. **Остальные ~900 малых
сессий (2–90 сообщений) не читались построчно** — и это **не пренебрежение**, потому что:
(1) все 968 расклассифицированы в проходе 1; (2) массив 1–8 сообщений — это оборванные/
дублирующие рестарты одной темы (в SESSION-INDEX видно по 10–40 повторов «AI-powered flow
generation…» с msg_count 1–3); (3) содержательные паттерны этих сессий уже дистиллированы
в SESSION-ANALYSIS.md (119 паттернов) и в тематических подпапках, прочитанных глубоко в
проходах 3–8; (4) новых архитектурных примитивов сверх зафиксированного словаря
(NODE/arbiter/MACHINE-FREEDOM/cycle-chain/judge-orchestrator/scope-isolation) они не вводят.
Единственная содержательная **правка честности** — кластеры «Angular/CSS» и часть
«Personal/Other» НЕ полностью нерелевантны: там крупный design-to-code + orchestrator/judge
слой, но он уже покрыт по существу (RAG-04, проходы 3–5). Пренебрежения по теме расширений
нет: весь orchestration/design-to-code/prompt/method/scenario материал захвачен.

### 9.6. Что остаётся для прохода 10 (сборка)

- Собрать итоговый RAG-навигатор по всем 9 файлам (RAG-01..09) с единой картой:
  **prompt-input (проход 8) → orchestration-catalog (проходы 3–4,6) → invariants/QA
  (RAG-06, проход 5) → design-to-code + judge/orchestrator слой (проход 9 §9.3, RAG-04) →
  extension continue-plan (VS / VS Code)**.
- Явный чеклист переноса из claude-sessions в план продолжения расширений: AUTOMATED GATE
  PROTOCOL, трёхscope classification (GLOBAL/PRIVATE/MODULE), judge=top-orchestrator node,
  useFlow SDK-контракт + offline queue, SK-набор (526/528/434-436), human-readable-companion
  правило, `IDesignAiProvider` + orchestrator/judge из Figma-плагина как прообраз
  host-адаптера.
- Финальная сверка «что уходит в core (`llm_mvp_core`) vs что остаётся тонким host-адаптером»
  (границы R5/R6 CLAUDE.md).

**RAG-файл (проход 9):** `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual
studio code continue/RAG-09-claude-sessions.md` (проход 9 добавлен отдельной секцией
«## Pass 9/10 …»; разделы 1–8 не тронуты).

---

## Pass 10/10 — навигатор RAG-09 + transfer-checklist для плана VS/VSCode + финальный вердикт папки 9

> Проход 10 — **сборка, не новое чтение корпуса**. Он не открывает сессии заново, а
> перечитывает сам RAG-09 (проходы 0–9, ~2537 строк) и превращает его в одну навигационную
> карту + чеклист переноса + честный вердикт папки 9. Цель — чтобы автор плана «Continue VS &
> VS Code extensions» нашёл нужный слой за один экран и знал, что именно из этой папки он
> переносит в план и что искать в других RAG-папках.

### 10.1. НАВИГАТОР RAG-09 — карта собственных секций (где что лежит для автора плана)

RAG-09 устроен как **6 смысловых зон**. Ниже — что автор плана находит в каждой и куда идти.

| Зона (что нужно автору плана) | Секции RAG-09 | Что там лежит (одной фразой) |
|---|---|---|
| **A. Идентичность папки + честное покрытие** | §0–§1 (стр. 29–88), §9.5 (стр. 2483) | Папка = экспорт чатов claude.ai о разработке XIIGen. Реально 1209 файлов = 968 сессий + 65 grok-orch + 56 sonnet-analyzes + 53 claude-orch + prompt-orch/user-prompts/tools/root. Таблица «глубоко vs категоризация». |
| **B. PROMPT-INPUT слой (что расширение принимает на вход)** | Проход 8 §8.1–§8.7 (стр. 1870–2229) | 103 промпта, 6 кластеров форм A–F, две рамки-обёртки; accept/reject таксономия (Гейт 1 task-origin, Гейт 2 самодостаточность); **Standalone-нормализация**; pipeline-скрипты ingest; §8.6 — итоговый контракт адаптера входа. |
| **C. ORCHESTRATION-каталог (что расширение исполняет)** | Проход 2 §2.x (стр. 310–577), Проход 3 §3.1 (стр. 590), Проход 4 §4.1–4.5 (стр. 805–967), Проход 6 §6.1–6.3 (стр. 1339–1562) | SESSION-ANALYSIS = 35 частей / 119 паттернов; FLOW-01..42 каталог; grok Kernel/bootstrap self-building; DRAWIO→PLAN 5-stage; extraction-протокол v1/v2/COMPLETE; **канонический 5-шаговый скелет `prompt→оркестрация`** (§8.3). |
| **D. INVARIANTS + QA + gate-конвейер** | Проход 5 §5.0–5.6 (стр. 1065–1338), Проход 9 §9.1–9.2 (стр. 2266–2399) | Метод многопроходной экзаменации; **7-компонентная рубрика качества промпта A/B/C** (P11); **AUTOMATED GATE PROTOCOL** (`tsc→jest→jest→Playwright→commit→push`); **три-scope classification** (GLOBAL/PRIVATE/MODULE_SCOPED); SK-набор (526/528/434–436). |
| **E. DESIGN-TO-CODE + JUDGE/ORCHESTRATOR слой** | Проход 3 §3.2–3.4 (стр. 645–761), Проход 4 §4.6–4.7 (стр. 968–1032), Проход 9 §9.3 (стр. 2400) | FLOW-31 Design Intelligence (3-слойный IR Figma→SystemModelIR, judge=IJudgeOrchestrator31); **`SceneNode`-IR + Figma→Canva/Miro/Webflow/Framer адаптеры** поверх одного ядра; **`IDesignAiProvider` мульти-провайдер orchestrator+judge** (из Figma-плагина) = прообраз host-адаптера. |
| **F. МОДЕЛЬНО-АНАЛИТИЧЕСКИЙ (forensic) слой** | Проход 7 §7.1–7.6 (стр. 1608–1868) | Sonnet-форензик поверх корпуса; **сверка трёх моделей Grok(черновик)/Claude(синтез)/Sonnet(аудит)** — иерархичны, не конкурентны; готовый шаблон orchestrator-слоя с обязательным self-audit («Что поймал протокол» + «Что остаётся непонятным»). |

**Красная нить (как зоны стыкуются в один конвейер расширения):**
```
[B prompt-input §8]  →  [D gate/rubric §5,§9]  →  [C orchestration-catalog §2,3,4,6]  →  [E design-to-code+judge §3,4,9.3]
 host prompt/дизайн      Гейт1/Гейт2 + A/B/C        5-шаговый скелет §8.3               SceneNode-IR + judge=top-node
 → Standalone-норм.      + AUTOMATED GATE           agent-output→discovery→G0-G7        → генерация/адаптер под стек
                                                    →review→SESSION-1..N
        └────────────────── [F forensic §7] обвязывает всё обязательным self-audit ──────────────────┘
```

### 10.2. TRANSFER-CHECKLIST — активы папки 9 → как каждый ложится в план VS/VSCode

Каждый актив **доказан этой папкой** (не гипотеза), с точкой в RAG-09 и одной строкой «как маппится на расширение».

| # | Актив (где в RAG-09) | Как маппится на VS/VSCode-расширение |
|---|---|---|
| **T1. AUTOMATED GATE PROTOCOL** (§9.1 462-сессия, §9.2 п.1) | `tsc → server jest → client jest → Playwright → commit → push` — готовый **Gate B executor-конвейер** плана; человеческие stop-gate удалены в пользу авто-цепочки (совпадает с «Gate B per placement» CLAUDE.md). |
| **T2. Три-scope data-classification** (§9.1, §9.2 п.2) | `connectionType`+`knowledgeScope`: **GLOBAL** (общая модель/шаблоны в репо) / **PRIVATE** (адаптивная user-модель, tenantId из ALS) / **MODULE_SCOPED** (transient). CF-POLICY-01 «absent→PRIVATE». Прямой прообраз R5-раздела «common-в-репо vs adaptive-user» для локальной модели расширения. |
| **T3. judge = top-level orchestrator-узел** (§9.1 305-сессия, §9.2 п.3) | Единая node-абстракция: flows/skills/arbiters — узлы, **судья = верхний orchestrator-узел**. Это тот самый orchestrator/judge-слой, который план расширения кладёт в core; хост лишь шлёт задачу и получает вердикт. |
| **T4. useFlow SDK + offline-queue** (§9.1 279-сессия, §9.2 п.7, §9.3 90-сессия) | `useFlow` hook-контракт + offline queue + draft/abandoned session state + background-step→client-signal. Это **клиентская сторона host-адаптера**: что именно расширение возвращает пользователю в VS/VSCode UI (панель/статус). |
| **T5. Мульти-AI-host 4-config паттерн** (Проход 5 §5.2) | Один DNA-паттерн в 4 форматах под парсер каждого хоста: `CLAUDE.md` (MD+XML-теги) · `.cursorrules` · `copilot-instructions.md` · `.clinerules` (YAML-like). → VS/VSCode = **ещё N тонких host-конфигов поверх ОДНОГО ядра-контекста**, а не форк. |
| **T6. Self-contained prompt-chain + A/B/C quality-gate** (§5.2 P11, §8.3) | `PROMPTS.md` 10 фаз, каждая исполнима без истории (`CONTEXT→WHAT TO BUILD→DNA CHECKLIST→TESTS→DONE WHEN`); рубрика **A (исполнимость 0-3) · B (DNA 0-2) · C (testability 0-2), мин. A≥2,B≥1,C≥1**. → формальный вход-gate качества промпта в prompt-API расширения. |
| **T7. Standalone-нормализация (контракт prompt-адаптера)** (§8.4.3, §8.6) | `vague/contextual host prompt → (resolve refs · name deliverables · unfold to plan · attach SESSION structure · preserve user contract) → standalone task`. Плюс два адаптер-гейта (task-origin; самодостаточность). → **точная спецификация нормализующего слоя адаптера** перед core-оркестрацией. |
| **T8. `IDesignAiProvider` мульти-провайдер orchestrator+judge** (§9.3 108- и 152-сессии) | Figma-плагин: `AIEnhanceOrchestrator.ts`+`AIEnhanceJudge.ts` поверх `IDesignAiProvider` (Claude/OpenAI/Gemini), независимый `scoreOnly()` с deduction-rubrics + Scoring Mode toggle. → **работающий прообраз host-адаптера с судьёй**: провайдеры за интерфейсом, orchestrator+judge над ними. |
| **T9. `SceneNode`-IR + Figma-адаптер blueprint** (Проход 4 §4.6–4.7, §3.3) | Каноническое ядро (`styles.ts` 18-шагов FigmaToCode) копируется без изменений; каждый хост = property-map + adapter(`→SceneNode`) + storage + writer + UI. `SceneNode` — **единый промежуточный IR** для Figma/Canva/Miro/Webflow/draw.io. → blueprint «тонкий host-адаптер поверх общего design-to-code ядра `llm_mvp_core`». |
| **T10. Extraction/examination протокол = метод само-документации модели** (Проход 6 §6.1–6.2, Проход 7 §7.4–7.5, §8.5) | Полный ingest-конвейер (`ZIP→split→map→categorize→батчи-50→extraction-протокол→accept/reject`) + 3-модельная проверка (Grok/Claude/Sonnet). → **как локальная модель расширения сама строит обучающие данные** из истории сессий хоста; гейт «не писать в исходный корпус» = изоляция user-данных. |
| **T11. Core-vs-thin-adapter граница (R5/R6)** (§4.7 п.1, §5.5, §8.6, §9.5) | Инвариант всей папки: **вся содержательная логика (discovery, planning-gates, review FC, judge, design-to-code движок) — в `llm_mvp_core`; host-слой (VS/VSCode/Cursor/Claude) — только gate+resolve+normalize+writer+UI**. Adapter не запускает оркестрацию на нерешаемом входе. |
| *(бонус)* **T12. human-readable-companion правило** (§9.3 90-сессия) | Машинные id (`flowId:"FLOW-33"`, `taskTypeId:"T521"`) всегда парны человекочитаемым (`flowName`,`taskTypeName`) во всех выходах (логи, Jira, DPO) — прямой аналог правила CLAUDE.md «машинные метки только после человеческого объяснения». → формат вывода UI/логов расширения. |

**Гейт мержабельности (для плана, не для этой RAG):** переносимые активы T1–T12 — это документные требования; при их внесении в план продолжения расширений держать `clean-mergeable-into-original-and-main` (evidence-строку `mergeability: clean into <original> AND main`), т.к. общие файлы (`CLAUDE.md`/skills) правятся additive.

### 10.3. ФИНАЛЬНЫЙ честный вердикт папки 9 (что даёт и чего НЕ даёт плану расширений)

**Что папка 9 ДАЁТ плану (её роль в общем RAG):** это **источник ORCHESTRATION + PROMPT-слоя + DESIGN-TO-CODE/JUDGE** для расширений. Конкретно и доказательно:
- **Prompt-input контракт** (T6–T7): чем мерить вход и как нормализовать сырой host-промпт в исполнимую standalone-задачу — готовая спецификация адаптерного gate/normalize слоя (Проход 8).
- **Orchestration-каталог + скелет** (T3, §2/§3/§4/§6): 119 паттернов, FLOW-01..42, 5-шаговый скелет `prompt→оркестрация`, judge=top-node, Kernel self-building — «что именно исполняет core».
- **Design-to-code + judge прообраз** (T8–T9): `SceneNode`-IR, Figma→N-хостов адаптеры поверх одного ядра, `IDesignAiProvider`+orchestrator+judge — рабочий образец «тонкий host-адаптер над общим движком».
- **Gate/QA/scope дисциплина** (T1–T2, T10, T12): AUTOMATED GATE PROTOCOL, три-scope classification, ingest-конвейер само-документации, human-readable-companion.
- **Граница core-vs-thin** (T11): подтверждённый всей папкой инвариант R5/R6.

**Чего папка 9 НЕ ДАЁТ (честная граница — искать в других RAG-папках):**
- **Нет рантайм-проводки кода** расширения (реальные C#/TS классы, регистрация в `15.OrchestratorModel`, `IModelRepository`/`25.ModelServer`, `IRemoteUnitLocator`, checkpoint/manifest lifecycle) — это живёт в RAG-папках **`llm_mvp_core` (2/4/6/8)**, не здесь. Папка 9 — это **чаты об архитектуре XIIGen/Figma-плагина**, а не сам `llm_mvp_core`-код.
- **Нет готового VS/VSCode host-entrypoint** (registered command, panel, chat participant, Extension Host wiring) — этот слой план берёт из VS/VSCode-специфичных RAG-папок; папка 9 даёт только **паттерн** тонкого адаптера (T5/T9/T11) и клиентский контракт (T4).
- **Нет обученной локальной модели / DPO-checkpoint** — папка даёт *метод* сбора данных (T10) и *scope-разделение* (T2), но не веса и не runner. Trained-feature readiness доказывается в core-RAG, не здесь.
- **Мисс-категоризация SESSION-INDEX** (§9.3): часть «Angular/CSS/Personal» реально = design-to-code/judge материал; он **не потерян** (покрыт через RAG-04 и проходы 3–5), но SESSION-INDEX как источник истины по релевантности **не годится** — судить по содержанию, не по бакету.

**Итоговая формула роли папки 9:** `claude-sessions = ЧЕМ кормить core (prompt-input) + ЧТО исполняет core (orchestration-каталог) + КАК судить design-to-code (judge/SceneNode) + КАКОЙ дисциплиной (gate/scope/self-audit)` — но **проводку этого в живой код и host-UI расширения план берёт из core-RAG (2/4/6/8) и VS/VSCode-папок**. Папка 9 закрыта: все 1209 файлов расклассифицированы (проход 1), все тематические подпапки прочитаны глубоко (проходы 2–8), крупнейшие тела Uncategorized прочитаны через Summary+grep (проход 9), пренебрежения по теме расширений нет.

**RAG-файл (проход 10, ФИНАЛ):** `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual
studio code continue/RAG-09-claude-sessions.md` (проход 10 добавлен отдельной секцией
«## Pass 10/10 …»; разделы 0–9 не тронуты). **Папка 9 (#9/12) закрыта.**
