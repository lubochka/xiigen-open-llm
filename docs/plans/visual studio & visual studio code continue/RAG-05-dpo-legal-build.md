# RAG-05 — папка «dpo legal build» (легальное обучение модели через DPO + ILearningDynamicGraph)

**Что это за файл.** Человекочитаемый русский RAG-индекс («что где лежит») по папке **#5 из 12**
в подготовке плана «продолжить расширения Visual Studio и VS Code» (offline локальная модель:
документация кода + design-to-code). Этот файл — навигатор по **185 документам** одной папки:
сначала объяснение трёх параллельных «семей» файлов, затем полный манифест (каждая группа/ключевой
файл — строка), затем подробный разбор мастер-плана и ключевых блоков (легальный teacher-гейт,
DPO-строитель, training-lifecycle, гайд построения DPO), затем текущий статус и рамка проходов 2–10.

**Физический путь папки:** `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build`
**Роль папки (по Любе):** обучение модели **легальным способом** (legal DPO build). Это план, через
который тренируется вся система: как строить DPO-пары, как обучать легально (license-gated
дистилляция), как измерять качество. **Релевантность: ВЫСОКАЯ — это центр обучения всего проекта.**
**Файлов в папке:** **185** (проверено `Glob` + `ls`, 2026-07-12; каталогов внутри нет).
**Про что все документы:** новый обучаемый уровень **`ILearningDynamicGraph`** (≡
`ILearningMachine<DynamicGraphDomain>`, Level 5) поверх существующей 5-уровневой иерархии
`llm_mvp_core`, плюс легальный teacher-гейт («dpo legal build»), плюс реальный DPO-тренер (Route B:
`11.DPO` + `18.CapabilityTraining`).

**Пасс этого RAG-индекса:** **1/10** (MAP + манифест + разбор верхних документов). Модель: Claude
Opus 4.8, Ultracode. Проходы 2–9 углубляют тела блоков `plan-01..14` + гайды + реконы; проход 10 —
сплошная сверка полноты + executive summary.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C2-R05-A, Цикл 2 документного обновления; добавка — ничего ниже не удалено и не переписано). Этот файл — снимок плана на 12–13 июля; кодовая ветка DPO-контура с тех пор ушла далеко вперёд. Авторитетная таблица — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке (раздел A.1, событие S4).
> 1. **Ветка `codex/dpo-implementation` (репозиторий `llm_mvp_core`): +455 коммитов с 14 июля.** На момент рекогносцировки Цикла 2 (28 июля) вершина `61d25ab5c` «fix: classify generated KNN as retrieval» — на 14 коммитов дальше точки аудита оркестратора `0f221ac89`. Ветка движется ежедневно: перед использованием перепроверять `git ls-remote` (спросить сервер о вершине ветки, ничего не скачивая).
> 2. **Среди 14 коммитов после точки аудита:** `d9ecc8937` «add learned router authority cutover» (переключение авторитета маршрутизации под-фаз на обученный решатель); `8a8cad4a2` «connect LoRA adapters to live training path» (LoRA-адаптеры — компактные дообучаемые надстройки над весами — подключены к ТРЕНИРОВОЧНОМУ пути, НЕ к пути ответов); `7ffab7580` «persist orchestrator episode memory» (персист эпизодной памяти оркестратора); пять коммитов топологических маршрутов (`576f345cb`, `2843215e8`, `1d2745e28`, `f7b797acf`, `ca440dc86`); четыре коммита допуска/карантина источников (`3a2dc5cb3`, `5586d0846`, `e571ddce0`, `9d41f8c66`).
> 3. **На вершине ветки существует семейство `DynamicGraphEdgeAuthority*`** («полномочия рёбер графа»): 11 исходных файлов (10 в `21.AgentInfrastructure/Logical/` + словарь в `00.Core/`) + 6 тестовых. «Пустой каркас» из аудита оркестратора больше не пуст.
> 4. **НЕ-ОВЕРКЛЕЙМ (обязательное правило чтения).** Эти коммиты ПОЯВИЛИСЬ на ветке; УДОВЛЕТВОРЯЮТ ли они дизайну плана — отдельный содержательный аудит кода, вне рамок этого документного обновления. В частности: (а) `MultiSubPhaseRouterUnit` + семейство `DynamicGraphEdgeAuthority*` НЕ доказаны равными канону `EdgeAuthorityUnit` из plan-02 §5 / §A.4.2; (б) «LoRA подключены к тренировочному пути» — НЕ та вклейка в путь ответов (forward/answer path), которую требовал пункт аудита WL-02; (в) типизированная легальная DPO-цепочка, teacher-маршрут (локальный Qwen3.5-9B «TP1», hosted Qwen3 «TP2») и авто-DPO-петля на точке аудита `0f221ac89` значились НЕ НАЧАТЫМИ — их появление после этой точки не проверялось.
> 5. **Итог:** статусы этого файла («в коде нет», «СКЕЛЕТ», «DESIGN-COMPLETE / CODE-ABSENT») читать как снимок на 12–13 июля; сегодня они могут быть устаревшими В ОБЕ СТОРОНЫ — что-то из «pending» частично построено, а что-то, что легко принять за готовое, не доказано.

---

## Кросс-ссылки на соседние папки sweep (уже сделаны — RAG-01/02/03/04)

- **RAG-01 (compare-branches):** ветка-интеграция целится именно в ЭТОТ DPO-план. RAG-01 назвал
  «настоящие файлы плана» как `codex-plan-00..14` + мастер `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` +
  матрицу `codex-REQUIREMENTS-COVERAGE-MATRIX.md` + гайды `codex-guide-01..05` + `claude-01/02/04/05/06`.
  **Уточнение этого прохода (важно):** на диске на самом деле **ТРИ параллельные семьи** файлов
  (`claude-*` = baseline-авторитет, `codex-*` = codex-дельты/мастер-индекс, `active-codex-execution-*`
  = сведённая исполнительная копия «baseline + валидированные дельты»). Полные тела блоков живут в
  `claude-plan-*` и `active-codex-execution-plan-*`; часть `codex-plan-*` — компактные
  encoding-восстановленные канон-заглушки (см. раздел «Три семьи»). Легальный teacher-гейт
  (`TeacherProvenance`, теперь **18–20 полей**) живёт здесь, в блоке 08.
- **RAG-04 (design-to-code):** реальный DPO-тренер «Route B» (`11.DPO` + `18.CapabilityTraining`) —
  это то, что ЭТА папка планирует включить в бой. Скомпилированный-но-пустой мост design-conversion
  из RAG-04 — по сути регрессионный тест для этого центра. Починка этого центра (обучение) чинит и
  translate, и wiki тоже (общий `11.DPO`). Эталон lifecycle для всего графа — `language-translate`
  (реальный `RewardMargin` из движка `11.DPO/DpoTrainer.cs`, а не из литерала).
- **RAG-03 (translate):** легальный license-гейт ЭТОЙ папки — то, что обязано отбраковать
  синтетические переводные данные без locator/provenance. Находка рекона R-17: контаминированные
  `sonnet-*` строки в `training-data/orchestration-reasoning/` → вердикт `DATASET_REBUILD_REQUIRED`.

---

## Три семьи файлов (без этого манифест не читается)

Все 185 файлов — это **три параллельные копии одного плана ILearningDynamicGraph** плюс общий слой
рекона и один транскрипт. Понимать их различие критично:

| Семья | Кол-во | Что это | Авторитет |
|---|---|---|---|
| **`claude-*`** | 34 | **Baseline-авторитет** (Claude). Полные тела блоков `claude-plan-00..14` (85–235 КБ каждый — самые глубокие), 8 библиотечных `claude-00..07`, доска/матрица, `claude-open-questions-answers-v1`, `claude-fable-plan-check-v1/v2`. | **Read-only baseline evidence authority** (по CLAUDE.md «Claude Baseline Plan Authority Gate»). |
| **`codex-*`** | 95 | **Codex-дельты + мастер-индекс + рекон.** Мастер `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` (1029 строк, индексирует блоки), `codex-plan-00..14` (часть — полные 56–79 КБ, часть — компактные 2–5 КБ encoding-восстановленные заглушки), 5 гайдов, 2 библиотеки, **63 рекон-файла**, ledger'ы/доски. | Codex-канон + evidence. **2 codex-префиксных файла — авторства Claude** (evidence-only). |
| **`active-codex-execution-*`** | 55 | **Сведённая исполнительная копия** = «весь Claude baseline + только валидированные непротиворечивые codex-дельты». Полные тела `plan-00..15` (91–260 КБ), гайды, доска/матрица, source-inventory (161 КБ), библиотеки знаний (20-pass/40-pass), 50-pass readiness-ledger'ы, Fable-gap-repair. | **Execution-facing canonical current** — исполнение стартует ТОЛЬКО когда active-codex содержит весь baseline + валидированные дельты. |
| `dpo plan session codex.md` | 1 | Транскрипт плановой сессии codex (1.3 МБ). | Evidence-only. |

> **Практический вывод для плана «продолжить расширения»:** самый глубокий и текущий источник —
> семья **`active-codex-execution-*`** (в частности `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`
> 258 КБ, `active-codex-execution-EXECUTION-CONTROL-BOARD.md` 261 КБ). Мастер
> `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` (93 КБ) — лучший **человекочитаемый вход** (он не дублирует
> блоки, а связывает их и держит §E вопросы). Семья `claude-*` — baseline для сверки.

---

## Полный манифест / структура (все 185 файлов учтены)

### F.1. Управляющие артефакты — источники истины (по каждой семье)

| Файл | Семья | Размер | Роль |
|---|---|---|---|
| `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | codex | 93 КБ | **МАСТЕР-ПЛАН** (человекочит. вход): §A архитектура-резюме, §B governance, §C индекс Part A + блоки P-01..P-14, §D ссылка на доску+матрицу, §F индекс артефактов, §E финальная секция вопросов. Разобран ниже. |
| `codex-EXECUTION-CONTROL-BOARD.md` | codex | 154 КБ | Доска исполнения: нумерация `Ф<P>.<SP>.<SS>` (12 фаз Ф0..Ф11, 26 подфаз, 59 подшагов), строгая последовательность, exit-gates, DAG. Единственный источник истины по execution-control. |
| `codex-REQUIREMENTS-COVERAGE-MATRIX.md` | codex | 12 КБ | Матрица трассировки R1..R21 → подшаги доски → Gate B → артефакт → статус (covered/partial/unmapped). |
| `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | active | 258 КБ | **Сведённый мастер-план** (полное тело, не индекс) — исполнительная канон-копия. |
| `active-codex-execution-EXECUTION-CONTROL-BOARD.md` | active | 261 КБ | Сведённая доска исполнения (полная). |
| `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md` | active | 166 КБ | Сведённая матрица покрытия (полная). |
| `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | claude | 235 КБ | Baseline мастер-план (полное тело). |
| `claude-EXECUTION-CONTROL-BOARD.md` | claude | 181 КБ | Baseline доска. |
| `claude-REQUIREMENTS-COVERAGE-MATRIX.md` | claude | 51 КБ | Baseline матрица. |

### F.2. Блоки плана Part A + P-01..P-14 (три копии каждого блока)

15 логических блоков (00 = §0 UML + §4 статус, 05 = §5 иерархия, остальные P-01..P-14). Присутствуют
в трёх семьях. **Глубина тел:** `claude-plan-*` 85–235 КБ (baseline), `active-codex-execution-plan-*`
91–260 КБ (canon current), `codex-plan-*` — mixed (полные 56–79 КБ: 00/01/02/03/05/09; заглушки
2–5 КБ: 04/06/07/08/10/11/12/13/14).

| Блок | Тема | codex-plan (КБ) | claude-plan (КБ) | active (КБ) |
|---|---|---|---|---|
| 00 | §0 UML всей системы + §4 статус | 62 | 88 | 102 |
| 01 | P-01 узел графа `IDynamicGraphNode` (L2) | 76 | 113 | 127 |
| 02 | P-02 ребро `IDynamicGraphEdge` + per-edge authority (L3) | 59 | 138 | 119 |
| 03 | P-03 оркестратор L4 (async-обход графа) | 56 | 85 | 91 |
| 04 | P-04 L5-машина `ILearningMachine<DynamicGraphDomain>` | 2 (заглушка) | 138 | 114 |
| 05 | §5 иерархия уровней + цепочки исполнения | 62 | 121 | 106 |
| 06 | P-06 async мульти-комбинации (1 вход → N → лучший) | 2 (заглушка) | 136 | 165 |
| 07 | P-07 авто-строитель DPO + канон `GraphDpoPair` | 2 (заглушка) | 108 | 141 |
| 08 | P-08 **легальный teacher-гейт** + provenance | 2 (заглушка) | 160 | 159 |
| 09 | P-09 авто-DPO петля (сердце проекта) | 79 | 173 | 184 |
| 10 | P-10 обучение+lifecycle (LoRA+DPO, split R5) | 2 (заглушка) | 115 | 122 |
| 11 | P-11 измерение (held-out/ablation/two-PASS) | 3 (заглушка) | 116 | 115 |
| 12 | P-12 замена скрытого детерминизма | 5 (заглушка) | 117 | 148 |
| 13 | P-13 консолидированная тест-стратегия (FC-26, 9 слоёв) | 2 (заглушка) | 96 | 167 |
| 14 | P-14 карта референсов (Stage 2) | 3 (заглушка) | 129 | 260 |
| 15 | (только active) same-scenario dual-path proof design | — | — | 32 |

### F.3. Гайды (5×3 = 15 файлов «как делать»)

| # | Тема | codex (КБ) | claude (КБ) | active (КБ) |
|---|---|---|---|---|
| 01 | как строить DPO-пары (разобран ниже) | 41 | 75 | 90 |
| 02 | как обучать (LoRA+DPO, lifecycle) | 44 | 72 | 89 |
| 03 | как мерить скорость/точность/качество | 40 | 68 | 82 |
| 04 | паттерн `ILearningMachine` и граф | 53 | 83 | 99 |
| 05 | авто-DPO петля | 44 | 78 | 103 |

### F.4. Библиотечные / контекстные документы (по семьям)

| Файл | Семья | Размер | Что это |
|---|---|---|---|
| `claude-00..07-*.md` (8 файлов) | claude | 4–17 КБ | Библиотечный индекс + 7 контекст-доков: authority-ledger (01), source-inventory+RECON (02, 17 КБ), ilearningmachine-notes (03), dpo-legal-model-policy (04), dpo-data-training-measurement-guide (05), architecture-brief (06), execution-gates+open-questions (07). |
| `active-codex-execution-00..07-*.md` (8 файлов) | active | 17–161 КБ | Те же 8 позиций, сведённая копия. **02-source-inventory = 161 КБ** (карта источников+референсов), **04-dpo-legal-model-policy = 54 КБ**, **07-execution-gates = 61 КБ**. |
| `codex-knowledge-library-llm-mvp-core.md` | codex | 55 КБ | Библиотека знаний по `llm_mvp_core` — индекс по 40 рекон-отчётам. |
| `codex-knowledge-library-llm-mvp.md` | codex | 49 КБ | Библиотека знаний по `llm_mvp` — индекс по 23 рекон-файлам. |
| `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` | active | 748 КБ | **40-pass** библиотека знаний по коду (R-B; parent-accepted 40/40). |
| `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` | active | 192 КБ | **20-pass** библиотека знаний по `llm mvp` (R-B; parent-accepted 20/20). |
| `claude-open-questions-answers-v1.md` | claude | 193 КБ | Банк открытых вопросов + ответы. |
| `claude-fable-plan-check-v1.md` / `-v2.md` | claude | 183 / 252 КБ | **Fable-аудиты покрытия плана** (v1: 127 находок = 91 COVERED / 30 PARTIAL / 6 MISSING). Источник текущего вердикта PLAN_NOT_READY. |

### F.5. Реконы llm_mvp_core (40 файлов `codex-recon-core-*`, evidence-only, ~12–17 КБ)

Пофайловый рекон подпроектов `llm_mvp_core` (по модулям 00..43 + docs/tests/assets):
`-00-tensor`, `-00-contracts`, `-00-rest`, `-04-trainloop`, `-05-emb`, `-06-vsearch`, `-07-graph`,
`-08-graphrag`, `-09-inf`, `-10-lora`, **`-11-dpo`** (движок DPO), `-14-continuous`, `-14-ml`,
`-15-logical`, `-15-rest`, `-16-autonomous`, `-17-dynamic`, **`-18-capability`** (авто-DPO пайплайн),
`-21-infra`, `-22-algos`, `-24-compress`, `-25-modelserver`, **`-26-router`** (keyword-router как
анти-паттерн `FORBIDDEN_RUNTIME_SHORTCUT`), `-27-translation` (эталон/R-19), `-28-nlp`, `-29-sc`,
`-33-matrix`, `-42-wiki` (Bootstrap-юниты), `-43-smartskills`, `-43-localwiki`; docs: `-docs-arch`,
`-docs-decisions`, `-docs-interfaces`, `-docs-submap`, `-docs-exp`, `-docs-skills-catalog`,
`-docs-skills-dpo`, `-docs-plans`; прочее: `-tests`, `-assets`. **Итого 40.**

### F.6. Реконы llm_mvp (23 файла `codex-recon-mvp-*`, evidence-only, ~14–15 КБ)

Пофайловый рекон доменов `llm_mvp` (папка планов/обучения):
`-lang-translate`, `-doc-learning`, `-local-wiki`, `-small-model`, `-orch-impl`, `-orch-teaching`,
`-orch-exp`, `-orch-ext`, `-extensions`, `-dyn-flow`, `-first-ext`, `-skills-loading`, `-all-skills`,
`-internal-learn`, **`-internal-learn-2`** (R-18: расхождение указателей train-ready V192/V309),
`-external-sources` (лицензии/provenance внешних источников), `-learning-sets`, `-teaching-ledger`,
`-plan-set`, `-train-ready`, `-obuchenie`, `-novyy-domen`, `-experiments`. **Итого 23.**

### F.7. Ledger'ы, доски, benchmark, skill-improvement (evidence/аналитика)

| Файл | Семья | Размер | Что это |
|---|---|---|---|
| `codex-BENCHMARK-DELTA-REPORT.md` | codex | 49 КБ | Benchmark Delta: сравнение плана Claude с каноном codex, ledger accept/reject/defer дельт (codex-owned). |
| `codex-50-cycle-plan-review-supplement.md` | codex | 49 КБ | **Claude-authored, EVIDENCE-ONLY.** Дополнение к протоколу review/fix (R20); 100 строк-индексов = НЕ циклы (`subagent_cycles_completed=0`). |
| `codex-skill-improvement-ideas-planning-review-architecture.md` | codex | 18 КБ | **Claude-authored, EVIDENCE-ONLY.** Идеи улучшения скилов планирования/ревью. |
| `codex-skill-improvement-ideas-...-codex-owned-v1.md` | codex | 6 КБ | Codex-owned идеи усиления skills (редактируемый auxiliary). |
| `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md` | codex | 16 КБ | Ledger 50/50 returned packet records (текущий comparison-run). |
| `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` | codex | 12 КБ | Ledger 50-cycle review. |
| `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md` | codex | 14 КБ | Поправки первого sweep-интегрирования. |
| `active-codex-execution-16-full-task-coverage-simulation-gate.md` | active | 241 КБ | Симуляция полного покрытия задачи. |
| `active-codex-execution-19-30-pass-sequential-...-ledger.md` | active | 434 КБ | 30-pass sequential original-task coverage ledger. |
| `active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md` | active | 761 КБ | **Канонический текущий 50-pass readiness ledger** (по CLAUDE.md — canonical ledger для Pass31+). |
| `active-codex-execution-41-current-ledger-50-pass-acceptance-index.md` | active | 14 КБ | Индекс приёмки 50/50 (parent-accepted). |
| `active-codex-execution-42-fable-gap-repair-additions-20260705.md` | active | 23 КБ | **Текущая карта ремонта** R-B/R-G + partial-дельты по Fable. |
| `active-codex-execution-17 / 18 / 31..37 / 50-pass-authority / F0-decision-unblocker / NO-SAFE-WORK / opus-f0-reconciliation / original-task* / plan-readiness-repair / XIIGEN-...-NO-BRANCH-POLICY` | active | 5–142 КБ | ~20 readiness/acceptance/reconciliation ledger'ов исполнительной семьи (parent-acceptance по блокам B10/B16/B19, seed-corpus, F0-разблокировка решений, оригинал-задача аудит). |
| `dpo plan session codex.md` | — | 1305 КБ | Транскрипт плановой сессии (evidence-only). |

**Сверка счёта:** 8 (§F.1) + 46 (§F.2: 15 codex-plan + 15 claude-plan + 16 active-plan) + 15 (гайды)
+ библиотеки/контекст + 63 рекона + ledger'ы + транскрипт. По семьям: **active 55 + claude 34 +
codex 95 + 1 транскрипт = 185** (проверено `ls`).

---

## Подробный разбор ключевых документов

### 1) МАСТЕР-ПЛАН — `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` (93 КБ, 1029 строк)

**Что это.** Единая точка входа в план `ILearningDynamicGraph`. Не дублирует блоки, а **связывает**
их: показывает Любе-архитектору из чего состоит план, в каком порядке реализовывать, чем доказывается
каждое требование, какие решения остаются за ней (§E). PLAN-ONLY (не разрешение на реализацию).

**Что строим (§A.2, аналогия «завод/оркестр с перекладываемыми лентами»):** обучаемый динамический
граф — модель, которая учится не одному навыку, а **отношениям между навыками**: кто кому передаёт
задачу, в каком порядке, как сливать результаты. Это **верхний обучающий уровень L5** поверх
существующей 5-уровневой иерархии — НЕ новый 6-й уровень.

- **узел-станок** = `ILearningUnit` (≡ `ILogicalUnit`, L2) — в рантайме сам не учится;
- **лента между станками** = `IDynamicGraphEdge` (NEW) — fan-out / fan-in / many-to-many /
  multi-output / return-loop;
- **карта лент** = `DynamicGraphTopology` (NEW) — хранится как данные, перекладывается на лету;
- **начальник цеха** = `ILearningOrchestrator` (≡ `ILogicalOrchestrator`, L4);
- **директор с ночной сменой обучения** = `ILearningDynamicGraph` (≡
  `ILearningMachine<DynamicGraphDomain>`, L5) — единственный с правом перенастраивать станки.

**Три жёстких правила через весь план (§A.2):**
1. **Обучение только на L5** (A-LM-02): узлы/рёбра — чистый inference, `Train*` только на машине.
2. **Запрет скрытого детерминизма** (A-LM-09 `DeterministicFinalAuthority=false`, A-LM-13): на каждом
   ребре финал принимает **обученный** узел; детерминистический роутер — либо генератор кандидатов,
   либо честно `baseline/eval-only`. Главный кандидат на замену — `MultiSubPhaseRouterUnit`
   (фикс-веса 0.55/0.25/0.20/0.02, `MultiSubPhaseRouterUnit.cs:283`, `isLearned:false`).
3. **Легальный teacher-гейт** («dpo legal build»): дистилляция — только open-weight permissive
   (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 modified-MIT / GLM / Mistral) через нейтральных
   провайдеров. Claude/OpenAI/Gemini как teacher — **никогда** (только judge, и judge не цель).

**Честное состояние системы (§A.2):** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — контракты 5 уровней
работают, скрытого детерминизма «под видом trained» нет (роутер честно `isLearned:false`), но
обученного DynamicGraph (checkpoint/ablation/fresh-load) **ещё нет**. Это **НЕ**
`PASS_AS_TRAINED_FEATURE`.

**Маппинг ролей Luba → реальные контракты (§A.3, CANON):**
| Имя Luba | Реальный контракт | Уровень | file:line |
|---|---|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` | L2 | `00.Core/LogicalContracts.cs:97` |
| `ILearningFlow` | `ILogicalFlow<TInput>` | L3 | `00.Core/LogicalContracts.cs:166` |
| `ILearningOrchestrator` | `ILogicalOrchestrator` | L4 | `00.Core/LogicalContracts.cs:202` |
| `ILearningDynamicGraph` | `ILearningMachine<DynamicGraphDomain>` | L5 | `00.Core/LearningMachineContracts.cs:208` |

**Семь NEW-типов (§A.4, в коде их НЕТ, проектируются):** `DynamicGraphDomain`, `IDynamicGraphNode`,
`IDynamicGraphEdge` (+ канон enum `GraphEdgeKind` = ровно 5 членов: FanOut/FanIn/ManyToMany/
MultiOutput/ReturnLoop, §A.4.1), `DynamicGraphTopology`, `EdgeAuthorityUnit` (обучаемый финальный
решатель ребра, вход `EdgeAuthorityInput` без candidate-index/SourcePath → выход
`EdgeAuthorityDecision`, §A.4.2), `GraphDpoPair` (канон DPO-пары, объединяет 5 форматов),
`TeacherProvenance` (поля легального учителя).

**§B Governance:** Authority Requirements Ledger R-01..R-20 (+ R21 branch-authority), contradiction
sweep, роль-loop, branch-правило (рабочее имя новой ветки реализации — **`DPOLegalLearning`**;
участвующие репо: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`,
`FigmaToCode-design-gen`, `CanvaToCode`), anti-loophole register, score-правила.

**§C индекс 16 блоков + §D ссылка на доску (Ф<P>.<SP>.<SS>, 12 фаз / 26 подфаз / 59 подшагов) и
матрицу.** §E — финальная секция вопросов: **5 активных вопросов** (E.0) + исторический банк Q1..Q41.
Активные: (1) активировать ли все 6 R15-доменов сейчас; (2) HTML/CSS+FLOW-38 фидбек по умолчанию
`PersonalAdaptive`?; (3) где живёт personal/adaptive legal-манифест; (4) канонический хост R16
(`xiigen-platform` vs FLOW-31 в `xiigen-flows`); (5) какие именно open-weight hosted-провайдеры
разрешены как teacher-для-обучения vs eval/judge-only.

**Теги:** `#master-plan #ILearningDynamicGraph #L5 #7-NEW-types #legal-gate #anti-determinism #plan-only`
**Кросс-линки:** доска `codex-EXECUTION-CONTROL-BOARD.md`, матрица `codex-REQUIREMENTS-COVERAGE-MATRIX.md`,
блоки `codex-plan-00..14`, полное тело — `active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`.

### 2) ЛЕГАЛЬНЫЙ TEACHER-ГЕЙТ — `codex-plan-08-legal-teacher-gate.md` (2 КБ, encoding-восстановленный канон-стаб; полное тело — `active-...-plan-08` 159 КБ / `claude-plan-08` 160 КБ)

**Что это.** Сердце «dpo legal build»: правило, как обучать легально. Teacher для
дистилляции/обучения — только open-weight permissive или явно легальный. **Claude, OpenAI, Gemini —
никогда как teacher-для-обучения**; допустимы как judge только при `used_for_training=false`.
Закрытая модель для обучения → `DATASET_REBUILD_REQUIRED`.

**Канон `TeacherProvenance` (в стабе — 18 полей):** `TeacherId`, `ModelName`, `ModelVersion`,
`Provider`, `ProviderType`, `LicenseId`, `LicenseUrl`, `LicenseSnapshotRef`, `WeightsAvailability`,
`TrainingUseAllowed`, `UsedForTraining`, `UsedForJudgingOnly`, `ClosedProprietaryModel`,
`TermsSnapshotRef`, `GeographyExportConstraint`, `SourceCategory`, `RawPrivateTextCopied`,
`RedactionPolicyRef`. *(Мастер §A.4 говорит про «18 полей = 13 базовых + 5 NEW»; более поздние active-обзоры
упоминают «20-field TeacherProvenance» — точное число уточнить в проходах 2–9 по active-plan-08.)*

**Enum'ы:** `SourceCategory` = {local, human, open_weight, hosted, deterministic_baseline,
self_judge, client_unverified, third_party}; `LegScope` = {CommonGlobal, PersonalAdaptive, EvalOnly,
Quarantine}; вердикты = {ACCEPT, BLOCK_FOR_HOSTED, BLOCK_FOR_COMMON, DATASET_REBUILD_REQUIRED,
QUARANTINE, EVAL_ONLY}.

**Gate B:** `legal-provenance-manifest.json` содержит только ACCEPT-строки для train-ready данных;
`self_judge_train_ready_without_legal_gate==0`; `triad_field_presence_rate==1.0`;
`train_ready_without_triad_count==0`. Hosted raw/private без redaction-доказательства блокирует
common/global (`BLOCK_FOR_COMMON`).

**Разрешённые common-лицензии** (по коду `ResultGapDpoWriters.cs:192-196`): **MIT, Apache-2.0,
ProjectInternal, Synthetic**. CC BY 4.0 — только с атрибуцией. Запрещены: CC BY-SA, GPL/AGPL,
unknown/unlicensed.

**Теги:** `#legal-gate #TeacherProvenance #license-families #DATASET_REBUILD_REQUIRED #judge-vs-teacher`

### 3) DPO-СТРОИТЕЛЬ — `codex-plan-07-dpo-builder.md` (2 КБ стаб; полное тело `active-...-plan-07` 141 КБ / `claude-plan-07` 108 КБ)

**Что это.** Авто-строитель DPO-пар. `GraphDpoBuilder.BuildPairAsync` выдаёт `GraphDpoPair` **только
когда проходят 4 гейта**: legal + split + provenance + authority. Пустые батчи для обучения невалидны.

**Канон `GraphDpoPair` (владелец — plan-09 §5.1):** `pair_id`, `level`, `node_or_edge_id`,
`input_context`, `chosen`, `rejected`, `wrong_markers`, `reason`, `teacher_provenance`,
`chosen_authority`, `rejected_authority`, `chosen_origin`, `rejected_origin`, `source_category`,
`leg_scope`, `legal_verdict`, `overlap_score`, `teacher_provenance_ref`, `split_proof_ref`,
`row_rejection_reason`, `source_ids`, `held_out`, `candidate_order_invariant`,
`excluded_leakage_signals`, `dpo_train_ready`. Legacy-адаптеры только на границе (`chosen_output →
chosen` и т.д.); хранимая пара — один словарь, не оба.

**Gate B:** `dpo_train_ready=true` требует полную триаду (legal accept + split accept +
authority/provenance accept); `train_ready_without_triad_count=0`; 5 входных форматов адаптируются
явными адаптерами (ни один не обходит legal provenance); FLOW-38/UI-фидбек входит только через
`GeneratedUiOutputFeedbackEvent`; путь `NO_AUTO_DPO_SIGNAL=true` не создаёт пару и промоушен.

**Теги:** `#dpo-builder #GraphDpoPair #5-formats-to-1-canon #triad-gate #near-miss`

### 4) TRAINING-LIFECYCLE — `codex-plan-10-training-lifecycle.md` (2 КБ стаб; полное тело `active-...-plan-10` 122 КБ / `claude-plan-10` 115 КБ)

**Что это.** Как обучать реально (не заглушка). Последовательность LoRA/DPO: **`Load(base) →
Load(lora) → Apply → Step → Evaluate → PromoteOrRollback`**. Наивная `SetValue`-мутация **не**
принимается как доказательство обучаемости.

**Поля манифеста:** `manifest_id`, `schema_version`, `graph_id`, `node_ids`, `edge_ids`,
`edge_authority`, `policy_version`, `source_cutoff`, `late_file_queue`, `path_id_strategy`,
`checkpoint_hash`, `base_checkpoint_hash`, `lora_checkpoint_hash`, `rollback_ref`,
`teacher_provenance_ref`, `split_proof_ref`. **Lifecycle-proof:** `process_id`, `restart_id`,
`entrypoint_trace_id`, `no_replay_cache_proof`, `loaded_checkpoint_hash`, `loaded_manifest_hash`, и т.д.

**Gate B:** свежая чистая загрузка процесса перед claim'ом trained PASS; rollback загружает прошлый
checkpoint и воспроизводит прошлую метрику; версия схемы манифеста присутствует и совместима; teacher
provenance = 18-полевой канон из plan-08. **R-19:** захардкоженный `RewardMargin` (литерал 0.03) в
`TranslationMachine.cs:70/123-126/187` — эталон обучения сломан, margin должен браться из движка
`11.DPO/DpoTrainer.cs`.

**Теги:** `#training-lifecycle #LoRA+DPO #checkpoint #fresh-load #continue-training #split-R5 #R-19`

### 5) ГАЙД ПОСТРОЕНИЯ DPO — `codex-guide-01-how-to-build-dpo.md` (41 КБ, 596 строк) — САМЫЙ ПОЛЕЗНЫЙ практический документ

**Что это.** Практический ответ: **как из узла/ребра графа машинно построить DPO-пару** (chosen /
rejected / `[WRONG:]`), не нарушив 3 класса правил: анти-leakage, легальный teacher-гейт, запрет
скрытого детерминизма. Гайд фиксирует, как DPO реализован в коде **сейчас** (с `file:line`),
перечисляет 5 реальных форматов, даёт 8-шаговый рецепт построения пары и список пробелов плана.

**Как DPO реализован СЕЙЧАС (§3, всё проверено чтением кода):**
- **Движок одного шага РАБОТАЕТ** (`11.DPO`): `DpoPair{Prompt;Chosen;Rejected}`
  (`11.DPO/Models/DpoPair.cs:3-8`), `Load(path)` JSONL (`11.DPO/DpoDataset.cs:18-26`), реальный шаг
  `DpoTrainer.Step` (`:31-70`): `margin = chosenReward − rejectedReward`,
  `loss = −log(σ(margin)+1e-8)`, ручной backward, `AdamW.Step`; `Beta=0.1`, `lr=1e-5`. **Пробел:**
  нет checkpoint/manifest/locator, export/import, fresh-load, continue-training, held-out;
  из боевого рантайма (15/42/43) `DpoTrainer` **не вызывается**.
- **Построитель пары на узле РАБОТАЕТ** (детерминированный): `WikiContextRoutingDpoGenerator`
  (`42.ProjectWikiDomain/...:17-290`) — chosen берётся из реального выхода юнита, rejected — near-miss
  + `[WRONG:]` (типы `always_bm25`, `always_semantic`, `ignore_stale_version`, `no_pre_checks`...).
  В `42.ProjectWikiDomain` — **12 таких `*DpoGenerator.cs`** («узлы графа wiki»).
- **Построитель пары на ребре РАБОТАЕТ**: `OrchestrationCorpusBuilder` (`OrchestrationTrainingPair`,
  3 режима Mode1/2/3, анти-leakage `NoExactPromptOverlap`).
- **Самый зрелый legal+anti-leakage код**: `43.SmartSkillsDomain/ResultGapDpoWriters.cs` —
  `ResultGapSourcePrivacyRouter.Classify` (`:111-153`) → quarantine / adaptive_only /
  trainable_common; schema-валидатор `:241-266` ловит утечку (raw private/vendor, source-id как
  признак); near-miss таксономия `:199-235`.
- **КЛЮЧЕВОЙ ФАКТ (Grep):** поля `teacher_access_mode`, `closed_proprietary_model`,
  `hosting_provider`, `teacher_output_used_for_training` встречаются **ТОЛЬКО в скилах/гайдах**
  (`docs/skills/*`) и НЕ встречаются ни в одном `.cs`, ни в одной строке `training-data/*.jsonl`.
  То есть легальный teacher-гейт **существует как правило, но не как поле данных** — обязательный
  пробел плана. Особый риск: на диске есть строки `source_ref: sonnet-local-analyzes:*` →
  происхождение chosen/rejected надо доказать против Legal Teacher Gate (Claude/OpenAI/Gemini →
  rebuild).

**8-шаговый рецепт (§4):** 0 классифицировать уровень → 1 legal gate ПЕРЕД построением → 2 сформировать
вход A (без утечки порядка/пути/id) → 3 chosen из легального teacher/first-party → 4 rejected =
near-miss + `[WRONG:]` (ровно одна осмысленная ошибка) → 5 `overlap_ratio<0.60` + permutation-инвариантность
→ 6 schema-валидатор анти-leakage → 7 разложить в `(Prompt,Chosen,Rejected)`, `reward_margin≥0`,
`loss<0.685` (≥0.693 = random = брак), held-out 50+50 disjoint по источнику → 8 честный статус
(`dpo_train_ready=false` / `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` пока нет lifecycle-метрик).

**8 пробелов плана (§7):** (1) teacher-provenance не реализован в данных; (2) нет единого
`GraphDpoPair` (5 несовместимых форматов); (3) граф как уровень отсутствует в коде; (4) chosen —
self-label/детерминирован; (5) lifecycle обучаемого узла не закрыт; (6) permutation/ablation-гейты
на ребре не доказаны; (7) supervised-ряды ≠ DPO; (8) расхождение указателей V192/V309.

**Теги:** `#how-to-build-dpo #DpoTrainer #WikiContextRouting #ResultGap-legal-router #anti-leakage #8-step-recipe`

---

## Текущий статус плана (из active-codex overlays — важно для «продолжить расширения»)

- **`plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`** (последний overlay 2026-07-06,
  `active-codex-execution-00-library-index.md`). `implementation_authorization: NO` до свежего
  approve Luba после ревью.
- **Fable-аудит** (`claude-fable-plan-check-v1.md`): **127 находок = 91 COVERED / 30 PARTIAL / 6
  MISSING.** Текущий repair-queue: R-G source-addressability неполна; несколько shared-contract /
  readability / lifecycle carrier'ов требуют family-repair. Карта ремонта —
  `active-codex-execution-42-fable-gap-repair-additions-20260705.md`.
- **R-B знание-библиотеки закрыты** (evidence-only): `llm mvp` 20-pass = 20/20 parent-accepted,
  `llm_mvp_core` 40-pass = 40/40 parent-accepted.
- **Продуктовое решение Luba 2026-07-07 (свежее, важное):** маршрут **`qwen3-together-v1` APPROVED** —
  hosted Qwen3 через Together, `model_license=Apache-2.0`, `closed_proprietary_model=false`,
  `teacher_output_used_for_training=true`, выведен из карантина для train/common (Luba: «Выведи
  маршрут из карантина»). Scope: **только** этот маршрут; DeepSeek(MIT) на том же чистом основании,
  но не blanket-approved; Kimi/GLM/Mistral — per-model verify; закрытые модели остаются FORBIDDEN.
  (`active-codex-execution-04-dpo-legal-model-policy.md` §Luba Product Decision Overlay.)
- **Честное состояние кода:** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — контракты есть, обученного
  DynamicGraph нет; `DpoTrainer` в бою не вызывается; legal teacher-поля есть в правилах, но не в данных.

---

## Рамка проходов 2–10 (что покрывают следующие проходы)

- **Пасс 1/10 (этот):** MAP + манифест всех 185 файлов (3 семьи) + разбор мастер-плана, plan-08
  legal-гейт, plan-07 dpo-builder, plan-10 training-lifecycle, guide-01. Текущий статус.
- **Пасс 2–3:** тела блоков графа — `plan-00` (§0 UML/статус), `plan-01` (узел), `plan-02` (ребро +
  `EdgeAuthorityUnit`), `plan-03` (оркестратор L4), `plan-04` (L5-машина), `plan-05` (§5 иерархия) —
  по семье `active-codex-execution-plan-*` (полные тела).
- **Пасс 4–5:** DPO-центр — `plan-06` (async-комбинации), `plan-09` (авто-DPO петля — «сердце»),
  `plan-11` (измерение held-out/ablation/two-PASS), `plan-12` (замена скрытого детерминизма),
  `plan-13` (тест-стратегия 9 слоёв), `plan-14` (карта референсов), `plan-15` (dual-path proof).
- **Пасс 6:** гайды 02–05 (train / measure / ilearningmachine-pattern / auto-dpo-loop).
- **Пасс 7:** реконы ядра `codex-recon-core-*` (по модулям — что реально работает/заглушка в 11.DPO,
  18.CapabilityTraining, 25.ModelServer, 26-router, 27-translation, 42-wiki).
- **Пасс 8:** реконы `codex-recon-mvp-*` (домены обучения) + библиотеки знаний (20-pass/40-pass).
- **Пасс 9:** ledger'ы/доски/Fable-checks/benchmark/open-questions — статусный слой, sync claude↔active.
- **Пасс 10:** сплошная сверка полноты (все 185 учтены) + executive summary «что и в каком порядке
  делать, чтобы включить легальное обучение в бой».

---

*Конец RAG-05, пасс 1/10. Индекс построен чтением: мастер-план (1029 строк, полностью), стабы
plan-07/08/10, guide-01 (596 строк, полностью), интро active/claude library-index, active-04-legal-policy
overlay. Все 185 файлов учтены и категоризированы. Источник путей —
`C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build`. PLAN-ONLY контекст: реализация требует
свежего approve Luba; текущий вердикт плана — PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE.*

---

## Pass 2/10 — ядро DPO: plan-07 builder + plan-08 legal-gate + plan-10 lifecycle (active-codex canon)

**Источник этого прохода.** Прочитаны ПОЛНОСТЬЮ (full-depth `active-codex-execution-*`, а не 2КБ
стабы, что видел Пасс 1): `plan-07-dpo-builder` (1748 стр.), `plan-08-legal-teacher-gate` (1677 стр.),
`plan-10-training-lifecycle` (1166 стр.). **Эти тела уточняют/исправляют два числа из Пасс 1:**
(1) `TeacherProvenance` = **20 полей** (не 18); (2) `GraphDpoPair` = **14 полей** (Pass37-канон), а НЕ
~24, что перечислял стаб — «лишние» поля ушли в `TriadVerdict`/манифест. Все три блока — **PLAN-ONLY**
(`implementation_authorization: NO until fresh Luba approval`, verdict `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`).

### 2.A) plan-08 — легальный teacher-гейт (владелец канона `TeacherProvenance`)

**Роль.** «Таможня» перед обучением: класс **`TeacherProvenanceGateUnit`** (NEW, L2 `ILogicalUnit`),
метод **`Evaluate(TeacherProvenance prov, RowUsageContext ctx)` → `TeacherProvenanceGateResult`**
(`legal_verdict`, `reason`, `dpo_train_ready_eligible`, `triad_present`,
`closed_proprietary_model_derived`). Отвечает на один вопрос: «легально ли использовать строку как цель
обучения общей модели?». **Главное правило: отсутствие маркера ≠ «чисто»** — пустой провенанс = минимум
QUARANTINE; ACCEPT только при положительно доказанной легальности.

**РАЗРЕШЕНИЕ 18-vs-20 (канон-владелец — блок 08 §3.1):** `TeacherProvenance` = **20 полей = 13 базовых +
5 NEW + 2 RR6**. При любом расхождении числа полей побеждает блок 08.
- **13 базовых:** `teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`,
  `model_version`, `request_id`, `provider_terms_checked`, `teacher_output_used_for_training`,
  `closed_proprietary_model`, `source_ref`, `source_sha256`, `contamination_markers`, `judge_model`
  (**`judge_model` — ВНУТРИ provenance**, не отдельное поле пары).
- **5 NEW (бенчмарк-дельты T2/T3/T5):** `terms_snapshot_ref`, `geography_export_constraint`,
  `source_category`, `raw_private_text_copied`, `redaction_policy_ref`.
- **2 RR6 (service-mode/grounding ban — отдельная ось риска от brand-denylist):** `service_mode`
  (enum `plain|grounding|web_search|retrieval_tool|code_exec|unknown`), `grounding_used` (bool).
  `used_for_training && (service_mode != plain || grounding_used) → C17 BLOCK` даже для allowlist-модели
  (Gemini Grounding / Google Search подмешивают web-контент с чужой лицензией).
- `closed_proprietary_model` — **вычисляемый** флаг (выводится из denylist по `model_id`, шаг 2), входу
  не доверяют (правило R2 «идентичность важнее флага»).

**Вердикт-enum (канон 08 §3.3) — 6 значений:** `ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED |
BLOCK_FOR_COMMON | EVAL_ONLY`. `EVAL_ONLY` — **NEW, фикс R17**: строка легально чистая, но
train-ineligible (неполная триада C15/C15b или невоспроизводимый конверт C8). Раньше это выражалось как
`ACCEPT`+флаг, и batch-гейт (ключится на `legal_verdict==ACCEPT`) физически пропускал строку в
`TrainBatchAsync` — теперь отдельный отвергаемый вердикт закрывает дыру. (Бенчмарк-стаб перечислял 5 без
`EVAL_ONLY` и с `BLOCK_FOR_HOSTED`; active намеренно берёт `EVAL_ONLY`, но hosted-кейс покрывает связкой
`BLOCK`+`BLOCK_FOR_COMMON`+`QUARANTINE`.) `LegScope` = {CommonGlobal, PersonalAdaptive, EvalOnly, Quarantine}.

**Машина вердикта `Evaluate` — 8 шагов (§2.2):** 1) ScanContamination (`model_id`+`judge_model` на
`sonnet|claude|gpt|openai|gemini`); 2) ClosedBlockByDenylist (closed && train → BLOCK / уже-train-ready →
`DATASET_REBUILD_REQUIRED`); 3) LookupAllowedTeacher (не в allowlist && train → QUARANTINE); 4)
LicenseAndTermsGate (license ∉ permissive → BLOCK C5; terms pending → QUARANTINE C6; geography → QUARANTINE
C7); 4b) ServiceModeGroundingGate (RR6 → BLOCK C17); 5) ReproducibilityGate (стохастика без
request_id/seed → `EVAL_ONLY` C8); 6) PrivacyRetentionGate (hosted raw private без redaction →
`BLOCK_FOR_COMMON` C9, только adaptive leg R5); 7) FirstPartyAndTriad (нет триады
raw_comparison_event+legal_envelope+training_batch → `EVAL_ONLY` C15); 8) JudgeBranch+ClientOrigin+
финальный ACCEPT. Инвариант: `dpo_train_ready=true` ⇔ `ACCEPT && provenance_presence_rate==1.0 (20 полей) &&
triad_present`.

**Легальные семьи (family→canonical-license map, §Pass02R-P07, инстанцирован как ДАННЫЕ на
`AllowedTeacherRegistry.family_canonical_license`; C5 сверяет declared license с canonical, не доверяет
флагу):**
| Семья | canonical_license | В C5-permissive `{Apache-2.0, MIT, ProjectInternal, Synthetic}` | Диспозиция |
|---|---|---|---|
| **Qwen3** | Apache-2.0 | член | **основной разрешённый teacher**; единственный с активным v1-конфигом |
| **DeepSeek** | MIT | член | canonical resolved, но НЕ v1-инстанцирован (нет ProviderConfig/terms-proof); verify-before-use |
| **Kimi (Moonshot)** | modified-MIT | **НЕ член** (`modified-MIT ≠ MIT`) | BLOCK/EVAL_ONLY до per-model verify; `RESOLVED_FROM_LEGAL_PART` = задача сама решила «проверить до использования» |
| **GLM (Zhipu/Z.ai)** | per-model (гетерогенная) | resolved per-release | BLOCK по умолчанию; per-release canonical-license row + heterogeneity flag |
| **Mistral** | per-model (гетерогенная) | resolved per-release | BLOCK по умолчанию; per-release verify + heterogeneity flag |

Denylist (никогда как teacher-для-обучения): **Claude / OpenAI / Gemini** (допустимы ТОЛЬКО как judge при
`used_for_training=false`). Инвариант скила: `closed && used_for_training ⇒ всегда DATASET_REBUILD_REQUIRED`
(никакое разрешение Luba это не снимает). Нейтральные хосты (Together/Fireworks/OpenRouter/Novita/DashScope)
несут `HostRole` для дисамбигуации «семья модели vs хост» (DeepSeek и Z.ai — двойная роль first_party/reseller).

**Решение Luba `qwen3-together-v1` — `APPROVED_BY_LUBA_2026-07-07` (§3.5.1b, active NOW):** hosted
Qwen3-via-Together выведен из карантина для train/common. Профиль: `HostingProvider=Together`
(`HostRole=neutral_reseller`), `ModelId=Qwen/Qwen3-235B-A22B`, `ModelLicense=Apache-2.0`,
`ServiceMode=plain`, `GroundingUsed=false`, `ProviderTermsChecked=true`,
`TeacherOutputUsedForTraining=true`, `ClosedProprietaryModel=false`,
`TermsSnapshotRef="LUBA_DECISION_2026-07-07(Together ToS 7.2 + Qwen3 Apache-2.0)"`. Основание: (а)
легальная часть задачи = whitelist моделей, на которых МОЖНО обучать (Qwen3 Apache-2.0 = дистилляция
разрешена); (б) Together ToS 7.2 отдаёт клиенту все права на Output, no-distillation-клаузы не найдено, ZDR
доступен. C6 `terms_snapshot_missing` QUARANTINE **снят ТОЛЬКО для этого маршрута**. Операционные
предусловия (НЕ блокеры карантина): ZDR-first до первого вызова, provenance JSON на каждый вызов, pin
serving id при cutover, verbatim-архив ToS (recommended). Scope-fence: DeepSeek(MIT) — то же чистое
основание, но НЕ blanket-approved; Kimi/GLM/Mistral — per-model verify; закрытые модели остаются FORBIDDEN.

**Статус plan-08 = PLAN-ONLY.** Все типы (`TeacherProvenance`, `TeacherProvenanceGateUnit`,
`TeacherProvenanceGateResult`, `AllowedTeacherRegistry`, `GraphDpoProvenanceWriter`) — NEW «в коде их НЕТ,
проектируются». Единственная реальная точка расширения в коде — license-хелпер **`IsAllowedCommonLicense`**
(`43.SmartSkillsDomain/ResultGapDpoWriters.cs:192-196`, фильтрует MIT/Apache-2.0/ProjectInternal/Synthetic),
который становится первым под-чеком C5. Подтверждает находку guide-01: легальный teacher-гейт **существует
как правило, но не как поле данных** (`teacher_access_mode`/`closed_proprietary_model`/`hosting_provider`
есть только в скилах, ни в одном `.cs` и ни в одной `training-data/*.jsonl`).

### 2.B) plan-07 — авто-строитель DPO (владелец канона `GraphDpoPair`)

**Роль.** `GraphDpoBuilder.BuildPairAsync(GraphDpoSourceRow raw, BuildContext ctx, ct) →
OperationResult<GraphDpoPair>` (NEW-класс). Нормализует **5 реально существующих** форматов пар в один
канон и проецирует на `LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`).

**5 входных форматов (все существуют в коде, по одному явному адаптеру):** F1 `DpoPair`
(`11.DPO/Models/DpoPair.cs:3`, `Prompt;Chosen;Rejected`); F2 `RoutingDpoPair`
(`26.TrainableRouter/Models/RoutingModels.cs:75`, есть `ChosenUnitId/RejectedUnitId`); F3
`OrchestrationPromptDpoPair` (`26.TrainableRouter/Training/OrchestrationPromptDpoPairBuilder.cs:54`); F4
`CodeTraceBuilderDpoPair` (`42.ProjectWikiDomain/CodeTraceBuilderDpoGenerator.cs:35`, есть канон `[WRONG:]`
`:48`); F5 `CrossStackBusinessFlowGraphDpoPair` (`42.ProjectWikiDomain/ProjectWikiDomainModels.cs:918`,
Angular↔.NET↔SQL, есть `RejectedType/EvidenceIds`). Ключевое наблюдение: **ни один из 5 не несёт легального
provenance учителя и ни один — анти-leakage** — это и есть причина существования строителя.

**Канон `GraphDpoPair` = 14 полей (Pass37 latest-Claude, владелец блок 07):** `PairId`; `Level`
(train-ready ⇒ `"unit"`); `NodeOrEdgeId`; `InputContext`; `Chosen`; `Rejected`; `WrongMarkers[]`; `Reason`
(≥24 символов, Gate B); `TeacherProvenance` (**nested 20-польная запись**, владелец блок 08); `SourceIds[]`;
`HeldOut`; `CandidateOrderInvariant`; `ExcludedLeakageSignals[]` (candidate_index/source_path/source_id/
file_order/correlation_id/exact_prompt); `DpoTrainReady` (гейт, не флаг-по-умолчанию). **5
строительских/диагностических полей** (`RejectedOrigin`, `LegScope`, `OverlapScore`, `SplitProofRef`,
`RowRejectionReason`) живут в `TriadVerdict`/манифесте, **НЕ** на канон-строке. Порядок-фичи (learned graph
order) несёт не 15-е поле, а сам prompt-triple: `chosen` сериализует принятые `OrderedPatternIds`, `rejected`
— отклонённые, `input_context` — структурный конверт (`OrderLevel`, `ConsumesPatternIds`, `CapabilityTags`)
без position/path/source-id/exact-prompt.

**Reward-скаляризация для НЕ-скалярных сторон (§3.2a — ключевой архитектурный фикс, владелец блок 07).**
Для любой стороны (последовательность/ранкинг/путь/план/объект) скаляр считается над СЕРИАЛИЗОВАННОЙ
стороной: `SerializeCandidate(side)` (Text → как есть; `OrderedPatternIds` → join "|"; объект →
CanonicalJson инвариантной культуры; иначе reject `STRUCTURED_SIDE_SERIALIZER_MISSING`). Затем
`reward(side) = Beta * (policyLogP(SerializeCandidate | input_context) − referenceLogP(... | input_context))`;
`margin = chosenReward − rejectedReward`. **`IHeldOutScorer.Score`/judge/ranking выбирают/аудируют `Y_best`
ТОЛЬКО как `preference_score`-provenance — не заменяют `reward(side)` и НЕ авторствуют chosen/rejected.**
Нет сериализации/log-prob для целой последовательности → строка не train-ready
(`REWARD_EXTRACTOR_MISSING`). `AcceptanceSignalEnvelope` запрещает, чтобы арбитр приёмки был
`trained_unit_self_confidence` (циклическая само-разметка) или single producer self-score.

**N-кандидат fan-out (§3.2a):** producer с N>1 кандидатами → **winner-vs-each-loser**: `N` кандидатов дают
ровно `N-1` `GraphDpoSourceRow` (и `N-1` попыток пары); 1-vs-1 = частный случай N=2; одно поле `rejected`
НИКОГДА не содержит список проигравших. `candidate_order_hash` = set/permutation-proof из stable-id+content-
hash, НЕ position-value и не authority (`CandidateOrderLeakageGuard`).

**`BuildPairAsync` — 4 гейта + порог (§3.3):** Шаг 0 защита (пустая сторона → `EMPTY_SIDE`,
`dpo_train_ready=false`, не throw — Rule 1); Шаг 1 адаптер формата; **Гейт 1 legal** (блок 08: `closed &&
used_for_training → DATASET_REBUILD_REQUIRED` ранний выход); **Гейт 2 split** (`ISplitProver`, near-dup с
held-out → `SPLIT_LEAK`/quarantine); **Гейт 3 authority/provenance+anti-leakage** (presence_rate==1.0 по 20
полям; triad `legal_accept && split_accept && authority_accept`; permutation-invariance — если выбор меняется
от перестановки → `candidate_order_invariant=false`/quarantine, это анти-shortcut A-LM-09 на уровне данных);
**Порог** (`overlap_score = TokenOverlap(chosen,rejected)`; ≥0.60 → `LOW_CONTRAST`; `reason<24` →
`REASON_TOO_SHORT`); Шаг 6 штамп `dpo_train_ready = все true`; инвариант `train_ready_without_triad_count=0`.
Пустой батч (0 строк прошли триаду) → **невалиден для обучения** (нельзя учить на нуле).

**Таксономия `rejected_origin` (§3.4, как строить «худший»):** `random-rejected` (валидный ответ из чужого
контекста); `copied` (дубликат chosen + внесённая ошибка `[WRONG:...]`); `missing-source` (ответ без опоры
на источник); `teacher-unknown` (без провенанса → блок гейт-1); `split-leak` (→ гейт-2); `order-artifact`
(→ гейт-3); `baseline-loser` (проигравший выход baseline-генератора типа `MultiSubPhaseRouterUnit` — мост к
ablation блоков 03/12). Канон `[WRONG:]` живёт в коде: `CodeTraceBuilderDpoGenerator.cs:48`
(`wrongAnnotationRate` считает долю rejected с `[WRONG:`).

**Статус plan-07 = PLAN-ONLY по строителю, но фундамент в коде есть.** `GraphDpoBuilder`,
`GraphDpoPairCandidate`, `GraphDpoProvenanceWriter`, `TrainableGraphDpoBuilderUnit` — NEW (ПРОЕКТ). НО: 5
форматов, канон `[WRONG:]` (F4), `WikiContextRoutingDpoGenerator` и 12 `*DpoGenerator.cs` в
`42.ProjectWikiDomain`, `OrchestrationCorpusBuilder`, зрелый legal/anti-leakage
`43.SmartSkillsDomain/ResultGapDpoWriters.cs` — **всё реально работает** (детерминированные построители пар).
Пробел: единого `GraphDpoPair` + nested-provenance + триадного гейта в коде нет.

### 2.C) plan-10 — жизненный цикл обучения (Load→Apply→Step→Evaluate→Promote|Rollback)

**Роль.** Временна́я ось L5-машины за один такт: `Load(base) → Load(lora) → Apply → Step → Evaluate →
PromoteOrRollback` (+ доказательства честности). Владелец такта — `DynamicGraphMachine :
ILearningMachine<DynamicGraphDomain>` (блок 04, `LearningMachineContracts.cs:208`). Фазы LC1..LC12.

**Такт по коду (привязка к реальным методам):** LC5 Step = `TrainBatchAsync` (`:212`) →
`ILogicalCapabilityTrainer` (A-LM-02) → `margin = chosenReward − rejectedReward` (`DpoTrainer.cs:44`),
`loss = −log σ(margin)` (`:47`), батч-усреднение `RewardMargin = marginSum/count` (`:119`). LC7 Evaluate =
`EvaluateAsync` (`:217`) → `LearningEvaluationResult` (`:90`) на source-disjoint.

**Precedent персистентности — `27.TranslationDomain/TranslationMachine.cs` (ПОЛНОСТЬЮ РЕАЛИЗОВАН, эталон
формы; читается only).** Ключевая граница «настоящая загрузка vs cached/synthetic pass»: контрактные
`ExportAsync`(`:222`)/`ImportAsync`(`:220`) обученное состояние **НЕ** persist/restore — это только
версия+ссылки (`LearningMachineExport` `:112`). Реальные веса сохраняет/грузит **`SaveCheckpointAsync`
(`:388`) / `LoadCheckpointAsync` (`:447`, static, fresh restore)** + `ExportCheckpoint`(`:254`) /
`ImportCheckpoint`(`:301`). Load(base) обязан звать `LoadCheckpointAsync`, а НЕ `ImportAsync` (иначе
маскарад персистентности — запрещён masquerade firewall). Guard-ы схемы/идентичности: `SchemaVersion:1`
(`:277`), `SchemaVersion!=1 → CHECKPOINT_SCHEMA_UNSUPPORTED` (`:308-310`), `MACHINE_ID_MISMATCH`(`:313-315`),
`DOMAIN_TAG_MISMATCH`(`:317-320`).

**Publish/locator/promote/rollback:** LC8 Promote — публикует checkpoint ТОЛЬКО при росте held-out
(`ExportAsync:222 → SaveCheckpointAsync:388 → 25.ModelServer / IModelRepository`, discoverable через
**`IRemoteUnitLocator`**, `AccuracyAtExport` = реальная held-out `:123`); нет роста → `ACCURACY_NOT_IMPROVED`,
прошлый checkpoint остаётся (образец дельта-гейта `16.AutonomousLearning/AutonomousLearningPipeline.cs:137-140`).
LC9 Rollback — `LoadCheckpointAsync` прошлого `rollback_ref` + **измеримое воспроизведение метрики**:
`|Accuracy(loaded_prev) − AccuracyAtExport(prev)| ≤ epsilon` на ТОМ ЖЕ held-out; несовпадение → откат не
доверенный, дефект persistence (append-only rollback-ledger). Continue-training/anti-forgetting/replay
разрешены ТОЛЬКО после первого source-disjoint held-out lift против замороженного baseline.

**Две легитимные PASS-точки (LC12):** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (нет checkpoint/lift — веха, не
фича) vs `PASS_AS_TRAINED_FEATURE` (checkpoint + held-out lift + ablation + fresh-load в новом процессе +
continue-training + e2e через реальный оркестратор блока 03). `no_replay_cache_proof` разведён на два:
(а) рантайм не берёт ответ из кэша прошлого процесса; (б) continue-training идёт на новых source-disjoint
строках. `LifecycleProof`-поля (`process_id`, `restart_id`, `entrypoint_trace_id`, `loaded_checkpoint_hash`,
`loaded_manifest_hash`, schema version, `rollback_ref`): любой `UNKNOWN` → «обучено» запрещено.

**Статус plan-10 = PLAN-ONLY по DynamicGraph, но lifecycle-форма доказана precedent'ом.**
`DynamicGraphMachine`, `LifecycleProof`, `GraphCheckpointManifest` — NEW СКЕЛЕТ (план). Инфра
`25.ModelServer`/`IModelRepository`/`IRemoteUnitLocator` — существует (детерминированная, allowed).
`TranslationMachine` — полностью реализованный precedent, доказывающий, что форма
`SaveCheckpointAsync/LoadCheckpointAsync/ExportCheckpoint/ImportCheckpoint`+schema-guard **работает**.
**Это ровно тот lifecycle, который RAG-04 назвал отсутствующим у Route B:** план не изобретает его, а
копирует персистентную форму TranslationMachine на DynamicGraph. Пробел из guide-01/RAG-01 подтверждён:
`11.DPO/DpoTrainer` (движок Step РАБОТАЕТ) из боевого рантайма (15/42/43) **не вызывается**; checkpoint/
manifest/locator/fresh-load/continue-training вокруг него для графа — план.

### Сводка implemented-vs-plan (три блока)

| Блок | Что уже РАБОТАЕТ в коде | Что PLAN-ONLY (NEW, проектируется) |
|---|---|---|
| **07 builder** | 5 форматов пар (F1-F5), канон `[WRONG:]` (`CodeTraceBuilderDpoGenerator.cs:48`), 12 `*DpoGenerator.cs` в 42, `OrchestrationCorpusBuilder`, legal/anti-leakage `ResultGapDpoWriters.cs` | `GraphDpoBuilder.BuildPairAsync`, канон `GraphDpoPair`{14}, nested-provenance, триадный гейт, reward-скаляризация, N-fan-out, `GraphDpoProvenanceWriter` |
| **08 legal-gate** | license-хелпер `IsAllowedCommonLicense` (`ResultGapDpoWriters.cs:192-196`), privacy-router `ResultGapSourcePrivacyRouter.Classify` (`:111-153`), schema-валидатор утечек (`:241-266`) | `TeacherProvenance`{20}, `TeacherProvenanceGateUnit.Evaluate` (8 шагов), 6-вердикт-enum, `AllowedTeacherRegistry` + family→license map — **как поле ДАННЫХ провенанса нет ни в одном `.cs`/`.jsonl`** |
| **10 lifecycle** | precedent `TranslationMachine` (`SaveCheckpointAsync:388`/`LoadCheckpointAsync:447`/schema-guard) ПОЛНОСТЬЮ; движок Step `DpoTrainer.cs:31-70`; инфра `25.ModelServer`/`IModelRepository`/`IRemoteUnitLocator` | `DynamicGraphMachine` (СКЕЛЕТ), `LifecycleProof`, `GraphCheckpointManifest`, весь такт для графа + promote/rollback-с-метрикой + two-PASS; `DpoTrainer` в бою (15/42/43) НЕ вызывается |

### Что остаётся на проходы 3–9 (уточнение рамки после чтения ядра)

- **Пасс 3:** тела блоков графа `plan-01` (узел `IDynamicGraphNode` L2), `plan-02` (ребро + `EdgeAuthorityUnit`
  L3, per-edge authority), `plan-00` (§0 UML/§4 статус), `plan-05` (§5 иерархия).
- **Пасс 4:** `plan-03` (оркестратор L4 async-обход), `plan-04` (L5-машина `ILearningMachine<DynamicGraphDomain>`,
  владелец `DynamicGraphMachine`), `plan-06` (async N-комбинаций 1→N→лучший).
- **Пасс 5:** `plan-09` (авто-DPO петля — «сердце», 184КБ, потребитель канонов 07/08/10), `plan-11`
  (измерение held-out/ablation/two-PASS), `plan-12` (замена скрытого детерминизма — `MultiSubPhaseRouterUnit`),
  `plan-13` (тест-стратегия 9 слоёв FC-26), `plan-14` (карта референсов), `plan-15` (dual-path proof).
- **Пасс 6:** гайды 02–05 (train / measure / ilearningmachine-pattern / auto-dpo-loop).
- **Пасс 7:** реконы ядра `codex-recon-core-*` (что реально работает/заглушка в 11.DPO, 18.CapabilityTraining,
  25.ModelServer, 26-router, 27-translation, 42-wiki).
- **Пасс 8:** реконы `codex-recon-mvp-*` + библиотеки знаний (20-pass/40-pass).
- **Пасс 9:** ledger'ы/доски/Fable-checks/open-questions — статусный слой, sync claude↔active.

*Конец Pass 2/10. Прочитано ПОЛНОСТЬЮ: active-plan-07 (1748 стр.), active-plan-08 (1677 стр., включая
машину вердикта §2.2, 20-полевой канон §3.1, family→license map §Pass02R-P07, qwen3-together §3.5.1b),
active-plan-10 (1166 стр., LC1-LC12 + precedent TranslationMachine). Разрешено: `TeacherProvenance`=20
полей (13+5+2 RR6, владелец блок 08); `GraphDpoPair`=14 полей (Pass37, владелец блок 07); qwen3-together-v1
`APPROVED_BY_LUBA_2026-07-07`. Все три блока PLAN-ONLY, verdict `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`.*

---

## Pass 3/10 — граф-архитектура: node (01) + edge (02) + UML (00) + hierarchy (05)

**Источник этого прохода.** Прочитаны ПОЛНОСТЬЮ (full-depth `active-codex-execution-*`, а не 2КБ codex-стабы):
`plan-00-partA-uml-status` (1099 стр.), `plan-01-node` (1251 стр.), `plan-02-edge` (1405 стр.),
`plan-05-partA-hierarchy` (1096 стр.). Это **СТРУКТУРНАЯ (Part A) архитектура** графа `ILearningDynamicGraph`:
узел (L2), ребро (L3), общая UML-карта системы (§0/§4) и место графа в 5-уровневой иерархии (§5). Все четыре
блока — **PLAN-ONLY** (`implementation_authorization: NO until fresh Luba approval`, verdict
`PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`, overlay 2026-07-05). Автор всех четырёх — Claude Opus 4.8 MAX,
namespace `claude-*`, сведён в `active-*`.

### 3.A) plan-00 — §0 UML всей системы + §4 статус (владелец Part A §0/§4 + канон §A.4.1/§A.4.2)

**Роль.** Единая карта всей системы на одной Mermaid-`classDiagram` (аналогия «завод с перекладываемыми
лентами»): станок=узел, лента=ребро, карта лент=топология, начальник цеха=оркестратор, директор с ночной
сменой=L5-машина. §0 отвечает на 5 вопросов (где узлы/рёбра и их виды; кто финал на ребре; где живёт
обучение = только L5; как строятся DPO-пары + где легальный teacher-гейт; что уже в коде vs NEW). §4 —
честная статус-таблица (анти-masquerade: контракт ≠ доказательство обученности).

**6 смысловых рядов диаграммы (§3 шаг 1):** (1) иерархия 5 уровней L5→L4→L3→L2 + сбоку L1
`ILearningMachinePair`; (2) субстрат графа (NEW `IDynamicGraphNode`/`IDynamicGraphEdge`/`DynamicGraphTopology`/
`DynamicGraphDomain` + существующие `GraphNode`/`GraphEdge`/`DynamicDomainTransition`); (3) рантайм-авторитет
(`AdaptiveLogicalOrchestrator`, `MultiSubPhaseRouterUnit`, `KnnLogicalUnit`, `DomainGraphClosure` — всё
работает); (4) построение DPO (`GraphDpoPairBuilder`(NEW)→`GraphDpoPair`(NEW)→`LearningTrainingItem`→
`DpoTrainer`); (5) легальный teacher-гейт (`TeacherProvenance`(NEW)+`LegalTeacherGateUnit`(NEW)+образец
`ResultGapSourcePrivacyRouter`); (6) lifecycle+измерение (`IModelRepository`/`IRemoteUnitLocator` из
`25.ModelServer`, метрики, `LearningMachineExport`).

**КАНОН §A.4.1 (владелец вида ребра) — `enum GraphEdgeKind { FanOut, FanIn, ManyToMany, MultiOutput,
ReturnLoop }` (РОВНО 5 членов).** Решение по спорным «членам»: `ReturnLoop`(=`BackEdge`/петля возврата) — 5-й
канонический член; `Sequential` — НЕ член (порядок обхода зависимостей `DomainGraphClosure`); `OneToOne` — НЕ
член (вырожденный FanOut с 1 целью); `Bridge` — НЕ член (флаг `KnowledgeEdge`); `AsyncParallel` — НЕ член
(режим исполнения = булев флаг `ParallelEligible` ребра). Устаревшие псевдонимы (обязаны ссылаться на канон):
`EdgeKind`, `DynamicGraphEdgeKind`, `GraphEdgeKind{...Bridge}`, `EdgeKind{...BackEdge,AsyncParallel}`.

**КАНОН §A.4.2 (решатель ребра) — `EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>`
(одна пара I/O, носитель A-LM-09).** Вход leakage-free (без candidate-index/позиции, без SourcePath); выход
`EdgeAuthorityDecision` = 5 полей владельца plan-02 §5 (`OrderedTargets[(id,Confidence)]`, `FanOutCount`,
`KnowledgeEdge`, `AuthoritySource`, `AblationBaselineRank[]`). **PASS21-фикс:** отдельного поля
`PerTargetConfidence[]` НЕТ (уверенность внутри кортежа `OrderedTargets`); старая active-строка теряла 4 поля
владельца — исправлено. Поле-владелец на ребре — `string AuthorityUnitId` (ссылка в реестр, latest-wins
A-LM-03); устар. псевдонимы `object/ILogicalUnit AuthorityUnit`, `AuthorityUnitRef`, `IDynamicGraphEdgeAuthority`.

**§4 статус-таблица (25 строк, честно) — сводка по статусам:**
- **РАБОТАЕТ как контракт (5 уровней + данные):** `ILearningMachine`(L5, `LearningMachineContracts.cs:208`),
  `ILogicalOrchestrator`(L4, `:202`), `ILogicalFlow`(L3, `:166`), `ILogicalUnit`(L2, `:97`),
  `ILearningMachinePair`(L1, `:245`); `LearningTrainingItem`(`:30`), метрики(`:43/:90`), `LearningMachineExport`(`:112`).
- **РАБОТАЕТ, детерминистический субстрат:** `GraphNode`/`GraphEdge`(Weight init 1.0), `DynamicDomainTransition`+
  `DomainGraphClosure`, `AdaptiveLogicalOrchestrator`(fan-out `:316`/ветки `:445`/петля `:361`).
- **BASELINE (работает, обученного состояния нет):** `MultiSubPhaseRouterUnit`(формула `:283`+словарь `:23-56`,
  **КАНДИДАТ №1 на замену**; в `unitRegistry` с `isLearned` НЕ регистрируется — литерал `isLearned:false` на
  `LogicalOrchestratorAssistant.cs:162/173` относится к `promptCompiler`/`DomainClassifierUnit`, НЕ к роутеру);
  `KnnLogicalUnit`(model-component, не финал); `ResultGapSourcePrivacyRouter`(образец license-роутера);
  `DpoTrainer`(движок loss/reward/Adam, без checkpoint/export/locator).
- **NEW (ПРОЕКТ, в коде нет):** `IDynamicGraphNode`, `IDynamicGraphEdge`(+`GraphEdgeKind`), `EdgeAuthorityUnit`,
  `DynamicGraphTopology`, `DynamicGraphDomain`, `GraphDpoPair`, `GraphDpoPairBuilder`, `TeacherProvenance`(20 полей),
  `LegalTeacherGateUnit`.
- **ОТСУТСТВУЕТ:** Trained DynamicGraph (checkpoint/ablation/fresh-load). Вывод two-PASS:
  `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, **НЕ** `PASS_AS_TRAINED_FEATURE`.

### 3.B) plan-01 — узел `IDynamicGraphNode` (L2 «упрощённая матрица», владелец канона узла §5.2)

**Роль.** Узел = наименьшая исполняемая клетка графа = `ILearningUnit ≡ ILogicalUnit<TInput,TOutput>` (L2,
псевдоним, НЕ новый уровень, A-LM-12). «Упрощённая матрица» **буквальна**: forward узла = `features ·
PreferenceWeights` (форма `MatMul`/`Linear.Forward`, `Linear.cs:37/41`, `[N,in]→[N,out]`) + top-N (argmax).
Узел — чистый inference, **сам не учится** (A-LM-02, `LearningMachineContracts.cs:8`); обучение — на L5.

**`IDynamicGraphNode` — ДВЕ формы одного owner-канона (§5.2, FC-44 anti-triple-live-canon):**
1. **не-generic база** `IDynamicGraphNode { string NodeId; DynamicGraphNodeDescriptor Descriptor }` — то, что
   кладётся в топологический контейнер `DynamicGraphTopology` как гетерогенный список узлов без параметров типа.
   **БЕЗ Train** (базовый контракт тоже без Train/Learn).
2. **generic** `IDynamicGraphNode<TInput,TOutput> : IDynamicGraphNode` — исполняемая форма; наследует базу;
   `ExecuteAsync(LogicalUnitRequest<TInput>) → OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>`
   (делегирует внутреннему `ILogicalUnit`). **БЕЗ Train.**

**`DynamicGraphNodeDescriptor` (паспорт узла) — owner-канон §5.2 = 10 полей:** `NodeId`, `DomainTag` (ДОМЕН-нога
паспорта: **статический** объявленный домашний домен узла, НЕ рантайм-домен вызова — рантайм-домен приходит
per-request через `LogicalUnitRequest.DomainTag`, EXISTING `:62`; инвариант: узел исполняется только когда
`request.DomainTag==Descriptor.DomainTag` или descriptor — wildcard, иначе честный `NoDecision`),
`TInputTypeName`, `TOutputTypeName`, `IsTrained`, `DeterministicFinalAuthority`(=false, A-LM-09),
`RuntimeSemanticQualityClaimed`(=false пока нет trained-evidence), `AuthorityRole`
(`trained_node|baseline_candidate_generator|diagnostics_only`), `CheckpointRef?`, `ManifestRef?`. Поля-флаги —
по образцу `SmartSkillsSemanticUnitCatalog.cs:10-12`. **DomainTag — это FABLE-PARTIAL P-03 fix** (раньше
ДОМЕН-нога триады «домен+вход+выход» не имела carrier-поля). В §0 диаграмме descriptor рендерится **подмножеством**
(7 полей) с явной abridgement-note на owner (10 полей).

**`GraphNodePreferenceState` — ОБУЧАЕМЫЙ trained-state узла (РЕШЕНИЕ блока, не открытый вопрос):** typed record
с `PreferenceWeights: Dictionary<string,double>` (**`double`, НЕ `int`-counter**), `FeatureVocabulary`,
`SkillVocabulary`, `GraphNodeCalibration`(TopNThreshold/ConfidenceTemperature), `GraphNodeKnnMemory`(K/metric/
ExampleIds — nearest как support evidence, `OK_MODEL_COMPONENT`), `GraphNodeStateProvenance`(CheckpointHash/
SchemaVersion/TrainingBatchId/LegalTeacherGatePassed), `TrainedNotCounterBaseline`. Веса двигает **DPO
reward-margin** (`margin=chosenReward−rejectedReward`, апдейт по `11.DPO/DpoTrainer.cs:42-44/47/50-52/54/64`,
AdamW-шаг), а НЕ `+1`-инкремент. Образец **формы** (не обученности) — `PromptUnderstandingSkillUnit.Evaluate`
(`43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:437-495`): честный counter-baseline
(`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`) с `SemanticQualityClaimed:false`+`BaselineCounterOnly:true` (`:475-476`),
`NoDecision()` при счёте ≤0 (`:462-463`). Counter-`Dictionary<string,int>`+`+1` остаётся **только eval-only
ablation** — называть его trained = masquerade A-LM-13.

**`GraphNodeCandidate` (один кандидат many→many выхода) — большой record (~24 поля):** `CandidateId`, `UnitId`,
`UnitIds[]`, `LogicalUnitRef`, `OutputRefs[]`, `OutputType`, `Confidence`, `Score?`, `ProvenanceRefs[]`,
`PathOrderRef`, `LegalStatus`, `Status`, `ErrorCodes[]`, `SourceIds[]`, `TraceRef`, `EvidenceIds[]`, ref-поля
блоков `B1TeacherJudgeRef/B7GraphOrderRef/B11AsyncBatchRef/B12WinnerSelectionRef/B6UnitDpoBuilderRef`,
`SemanticQualityClaimed`. **Публичный выход узла = строго `IQueryable<GraphNodeCandidate>`** (единый owner-shape;
старый вопрос «IQueryable vs `GraphNodeCandidate[]`» — исторический, снят как forbidden-hedge P02; B11
материализует ОДИН раз на границе node→runner в `IReadOnlyList<GraphNodeCandidate>` внутри `AsyncCandidateBatch`;
`NO_OBJECT_OUTPUT`: сырой `object Output`/untyped-массивы НЕ каноничны). **REQUESTED TYPE `T`** (attachment
line 11 `input→IQueryable<results of REQUESTED TYPE>`) несётся как payload: `OutputType==typeof(T).Name` +
`OutputRefs` (ref-адресация) ИЛИ производный `TypedGraphNodeCandidate<T> : GraphNodeCandidate` (строгий payload);
публичный shape не меняется. Выбор ref vs generic — единственный `LUBA_PRODUCT_DECISION_RESIDUAL` (форм-фактор
внутри уже решённого owner-правила).

**Миграция 3 классов существующих `ILogicalUnit` (§3.В, узлы НЕ переписываются — оборачиваются):** (1) уже
обучаемые `*SkillUnit`(43) — только обернуть, `ExecuteAsync` не трогаем; (2) детерминистический
`MultiSubPhaseRouterUnit` → `AuthorityRole=baseline_candidate_generator`, `IsTrained=false`, НЕ финальная
авторитетность (A-LM-09/R4), завести обученную замену; (3) доменные `27.TranslationDomain` (`SemanticTokenizerUnit`/
`ExecutorUnit`, учатся через `ITranslationUnitTrainer`, НЕ `ILogicalUnit`) — нужна явная `ILogicalUnit`-обёртка
инференса.

### 3.C) plan-02 — ребро `IDynamicGraphEdge` (L3) + `EdgeAuthorityUnit` (АНТИ-ДЕТЕРМИНИЗМ, ядро A-LM-09)

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C2-R05-B, Цикл 2; добавка). Этот раздел описывает `EdgeAuthorityUnit` (обучаемый решатель ребра) как замену детерминированной формулы `MultiSubPhaseRouterUnit` и констатирует «в коде нет». На 28 июля на ветке `codex/dpo-implementation`: коммит `d9ecc8937` объявляет «learned router authority cutover» (переключение авторитета на обученный решатель), и существует семейство из 11 исходных + 6 тестовых файлов `DynamicGraphEdgeAuthority*`. ПОЯВЛЕНИЕ ≠ СООТВЕТСТВИЕ канону §A.4.2 (вход без утечки SourcePath, 5-полевой `EdgeAuthorityDecision`, `Id="unit-edge-authority-v1"` и т.д.): равенство НЕ проверено, нужен содержательный аудит кода — вне рамок этого документного обновления. См. оверлей Цикла 2 в начале файла и `BRANCH-STATUS-CURRENT-2026-07-28.md`.

**Роль.** Ребро = управляемый переход «выход узла → вход узла(ов)» = L3-флоу `ILearningFlow ≡ ILogicalFlow`
(`:166`), у которого есть **собственная обучаемая авторитетность**. Ребро **исполняется** на L3, авторитет
**вычисляет** обучаемая L2-юнит, **учится** этот юнит на L5. Сегодня «ребро» в коде разнесено в 3 несовместимых
вида: структурный `GraphEdge{From;To;Weight=1.0}` (`07.GraphEngine/Models/GraphEdge.cs:3`, вес ручной,
неизменяем, не обучаем); топологический `DynamicDomainTransition(FromId,ToId)` (`17.DynamicDomains/
DynamicDomainGraph.cs:3`, голая пара без типа/авторитета); неявный `SubPhaseRoutingDecision[]` внутри
оркестратора. Ни один не выражает fan-out/fan-in/many-to-many/multi-output/return-loop и владельца финала.

**`IDynamicGraphEdge` — ЕДИНЫЙ КАНОН (источник истины plan-03 §5 + plan-00 §A.4.2):** `EdgeId`, `FromNodeId`,
**`ToNodeIds: IReadOnlyList<string>`** (МНОЖЕСТВО целей; Sequential/OneToOne ⇒ 1 элемент — закрывает
структурную невозможность выразить FanOut/MultiOutput/ManyToMany), `Kind: GraphEdgeKind`(5 членов),
`ParallelEligible: bool` (async = флаг, НЕ член enum), **`AuthorityUnitId: string`** (ССЫЛКА в реестр на
обученного владельца, резолв latest-wins A-LM-03, НЕ прямой объект), `AuthorityUnitLevel`,
`DeterministicFinalAuthority: bool` (ОБЯЗАТЕЛЬНО `false` в проде, машинно-проверяемый носитель A-LM-09),
`BaselineUnitId?`, `BaselineRole`, `ExcludedLeakageSignals[]`, `SourceProof`, `CutoverProof`, `AblationProofRef`,
`TraverseAsync(EdgeTraversalInput)`. **Устаревшие псевдонимы (НЕ использовать):** прямой объект `ILogicalUnit
Authority`, `AuthorityUnitRef` — расхождение полей между claude-блоками = ДЕФЕКТ, не «деталь именования».

**5 видов ребра `GraphEdgeKind` (канон §A.4.1, ровно 5):** `FanOut` (веер наружу, деталь нескольким узлам, cap
`MaxAdaptiveBranchCount=8` `AdaptiveLogicalOrchestrator.cs:19`); `FanIn` (точка сборки, `IEdgeFanInMerger.Merge`);
`ManyToMany` (много-ко-многим); `MultiOutput` (одно ребро → несколько типизированных выходов); `ReturnLoop`
(петля коррекции/обратное ребро, honest deterministic).

**`EdgeAuthorityUnit` — ОБУЧАЕМЫЙ финальный авторитет ребра (ГЛАВНАЯ анти-детерминизм замена):** заменяет
детерминистический финал `MultiSubPhaseRouterUnit.Score` (фикс-веса `0.55/0.25/0.20/0.02`,
`MultiSubPhaseRouterUnit.cs:283-284` + словарь якорей `SubPhaseAnchorTerms` `:23`). `EdgeAuthorityUnit :
ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>`, `Id="unit-edge-authority-v1"`, `Role="edge-authority"`,
БЕЗ Train (учится на L5). Внутри сортирует кандидатов **по выходу обученной модели** (K-NN-ретривер как
model-component, прецедент `KnnLogicalUnit.ExecuteAsync→_index.Search` `KnnLogicalUnit.cs:218` — допустимо ВНУТРИ
уровня), а не по фикс-формуле. Вход `EdgeAuthorityInput` **физически без leakage** (`SemanticTokens[]`,
`PredicateIds[]`, `ContextSignals`, `Candidates`(порядок несущественен, perm-invariant), `ExcludedLeakageSignals=
["candidate_index","source_path","exact_prompt"]`; прецедент исключения пути — `MultiSubPhaseRouterUnit.
BuildPathFreeScoringText:291`, `source_path_used_for_scoring=false`). Выход `EdgeAuthorityDecision`
(`OrderedTargets[(id,Confidence)]`, `FanOutCount`, `KnowledgeEdge` — честный «не знаю» при пробитии
confidence-floor, `AuthoritySource="trained_edge_unit"`, `AblationBaselineRank[]`).

**Граница deterministic vs trainable на ребре (§3.2):** генерация кандидатов = детерминированный baseline
(`IEdgeCandidateProvider` над `SubPhaseAnchorTerms`+`HybridRanker`+bridges, `BASELINE_MUST_BE_LABELLED`); финал/
порядок fan-out = **обучаемый** `EdgeAuthorityUnit` (`NEEDS_ILOGICALUNIT_WRAPPER`); fan-in = явный typed-контракт
`IEdgeFanInMerger.Merge→GraphFanInMergeResult`(`enum FanInMergeRole{DeterministicFanInBaseline}`,
`MergeOrderIsAuthority=false`, B-16/F1-merge-carrier witness — порядок веток НЕ становится авторитетом); порядок
по зависимостям = детерминированный `BuildPredecessorMap` (`OK_INTERNAL_DETAIL`); обучение = L5 `TrainBatchAsync`;
замена baseline→trained = реестр latest-wins (A-LM-03).

**Коррекция / return-loop (§1.1 — R5 роль flow=КОРРЕКЦИЯ; две РАЗНЫЕ роли на одном механизме):**
**L3-поток ИНИЦИИРУЕТ** возврат (метит `KnowledgeEdge=true` `SubPhaseRouterUnit.cs:29` / эмитит
`PhaseTransitionSignal`), **L4-оркестратор ИСПОЛНЯЕТ обход** обратного ребра `GraphEdgeKind.ReturnLoop` через
`FollowTransitionSignalsAsync` (тело `AdaptiveLogicalOrchestrator.cs:361`, вызов `:147`, чтение сигналов `:367`)
с лимитом `MaxTransitionDepth=3` (`:22`, проверка `:380`). Семантику починки (Encoder/Critic/Repair) держит УЗЕЛ
L2, поток коррекцию «не думает». **P02-FIX (seam узел→ребро):** ребро — прямой потребитель узла; выход узла
`IQueryable<GraphNodeCandidate>` заходит в ребро через явную запись-адаптер `NodeOutputToEdgeInputAdapter`
(`SourceNodeId`, `UpstreamCandidateRefs[]` — ref-снимок БЕЗ позиции, `UpstreamOutputHash` order-free,
`ProjectionRule`), а не сырой `IQueryable`.

### 3.D) plan-05 — §5 иерархия: место L5 в 5-уровневой ILearningMachine + 5 помеченных цепочек

**Роль.** §5 — скелет машины: какие 5 уровней участвуют, какие типы Input/Output ходят между ними, в каком
порядке. Главный вывод (РЕШЕНИЕ, не вопрос): `ILearningDynamicGraph = ILearningMachine<DynamicGraphDomain>` —
**специализация существующего Level 5 (A-LM-12), НЕ новый 6-й уровень**. Аналогия — «дирижёр и оркестр».

**Иерархия L5→L1 (Input/Output):**
- **L5** `ILearningMachine<DynamicGraphDomain>` (`LearningMachineContracts.cs:208`): вход
  `LearningBatch<DynamicGraphDomain>`(`:18`)/`LearningEvaluationSet`; выход `LearningBatchResult`(`:43`)/
  `LearningEvaluationResult`(`:90`, `PassesGate:100`)/`LearningMachineExport`(`:112`); `GetOrchestratorAsync()`
  (`:232`)→`ILogicalOrchestrator`. **Единственный владелец Train\*** (A-LM-02).
- **L4** `ILogicalOrchestrator` (`LogicalContracts.cs:202`, `RunAsync:204`): вход `LogicalOrchestratorRequest`
  (`:174`); выход `LogicalOrchestratorResult`(`:182`, `BranchResults:190` — агрегат fan-in ЖИВЁТ ЗДЕСЬ, НЕ на
  `LogicalFlowResult`). Прецедент — `AdaptiveLogicalOrchestrator.RunAsync:85`.
- **L3** `ILogicalFlow<TInput>` (`:166`, `ExecuteAsync:168`): вход `LogicalFlowRequest<TInput>`(`:105`); выход
  `LogicalFlowResult`(`:131`). Владелец ребра; роль R5 = КОРРЕКЦИЯ.
- **L2** `ILogicalUnit<TInput,TOutput>` (`:97`, `:99`): вход `LogicalUnitRequest<TInput>`(`:62`); выход
  `LogicalUnitResult<TOutput>`(`:72`). Чистый inference.
- **L1** `ILearningMachinePair<TP,TG>` (`:245`): Common(`llm_mvp_core`)+Personal(user side) ноги (R5 SPLIT),
  sync только анонимный (A-LM-08, `:253-255`). Для DynamicGraph применяется если нужна user-adaptive нога (§12 Q).

**5 помеченных цепочек исполнения (§3):** (a) **inference** L5→4→3→2 (обучения нет, A-LM-02); (b) **обучение**
узла/ребра — только L5 (`TrainBatchAsync:212`, `GraphDpoPair`→`LearningTrainingItem:30`); (c) **авто-DPO петля**
(прогон→тройка chosen/rejected+`TeacherProvenance`→обучение→лучший прогон); (d) **async мульти-комбинация**
(параллельный fan-out `:462` / последовательный `:481` / сборка fan-in); (e) **коррекция/return-loop** (L3
организует возврат, L4 исполняет обход `FollowTransitionSignalsAsync:361`, глубина ≤3).

**7 NEW-типов (§5.2, канон §A.4; в коде НЕТ):** #1 `DynamicGraphDomain`(sealed marker L5), #2
`IDynamicGraphNode`(обёртка узла), #3 `IDynamicGraphEdge`+`GraphEdgeKind`(enum, 5 членов), #4
`DynamicGraphTopology`(мутируемая карта `Nodes[]`/`Edges[]`, здесь физически живёт `AuthorityUnitId` при
export/import графа блока 04), #5 `EdgeAuthorityUnit`(обучаемый решатель ребра), #6 `GraphDpoPair`(14 полей,
владелец P-07), #7 `TeacherProvenance`(20 полей, владелец P-08, `judge_model` ВНУТРИ). (+ вспомогательный B-16
carrier `IEdgeFanInMerger`/`GraphFanInMergeResult`/`FanInMergeRole`.)

### Сводка implemented-vs-plan-vs-stub (структурная архитектура)

| Тип/компонент | Статус | Владелец канона | Факт |
|---|---|---|---|
| 5 уровней L2-L5 + L1 (контракты) | **РАБОТАЕТ как контракт** | 00.Core | `LogicalContracts.cs:97/166/202`, `LearningMachineContracts.cs:208/245` |
| `MultiSubPhaseRouterUnit` (детерм. финал ребра) | **РАБОТАЕТ, BASELINE** (не обучен) | 21.AgentInfrastructure | `:283` фикс-веса + `:23` словарь; кандидат №1 на замену |
| Субстрат `GraphEdge`/`DynamicDomainTransition`/`GraphNode` | **РАБОТАЕТ, детерминистический** | 07/17 | Weight ручной, не обучаем |
| `DpoTrainer` (движок reward-margin) | **РАБОТАЕТ как кирпич** | 11.DPO | `:42-44/47/50-54/64`; без checkpoint/locator |
| `IDynamicGraphNode`(+база/generic), `DynamicGraphNodeDescriptor`(10), `GraphNodeCandidate`, `GraphNodePreferenceState` | **NEW / СКЕЛЕТ (ПРОЕКТ)** | plan-01 §5.2 | в коде нет; форма доказана образцом 43 |
| `IDynamicGraphEdge`(единый канон), `GraphEdgeKind`(5), `EdgeAuthorityUnit`, `EdgeAuthorityInput/Decision`, `IEdgeCandidateProvider`, `IEdgeFanInMerger`, `GraphFanInMergeResult` | **NEW / СКЕЛЕТ (ПРОЕКТ→цель trained)** | plan-02 §5 / plan-00 §A.4.1-2 | в коде нет |
| `DynamicGraphTopology`, `DynamicGraphDomain` | **NEW (ПРОЕКТ)** | plan-00/05 | мутируемая карта / marker L5 |
| Trained DynamicGraph (checkpoint/ablation/fresh-load) | **ОТСУТСТВУЕТ** | — | `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, не trained feature |

### Single-owner / dual-canon флаги (plan-file-family consistency — что найти проверяющему пасса 9)

- **`IDynamicGraphNode` — живой triple-render owner-канона (главный флаг этого прохода).** Владелец =
  **plan-01 §5.2**: база `{ NodeId, DynamicGraphNodeDescriptor Descriptor }` + generic
  `IDynamicGraphNode<TInput,TOutput> : IDynamicGraphNode` (ExecuteAsync→`IQueryable<GraphNodeCandidate>`), и
  ЯВНО запрещает вводить типонезависимый `{ NodeId, object Unit }` как самостоятельный контракт. **Но consumer-
  рендеры расходятся:** (1) **plan-00 §A.4** рендерит `IDynamicGraphNode : ILogicalUnit<LogicalUnitRequest
  <object>, object> { NodeId; ILogicalUnit<?,?> Unit; Role }`; (2) **plan-05 §5.2** рендерит `IDynamicGraphNode
  { NodeId; object Unit }` (с abridgement-note на owner, но всё равно с `object Unit` и без `Descriptor`/generic-
  формы). Это ровно `dual-live-canon`/`dropped-owner-type` класс, который ловили Fable-аудиты. **Смягчение:**
  plan-01 §5.2 это ЗНАЕТ и заявляет единый owner-канон, помечая рендеры plan-03/plan-05 как «abridgements,
  правка вне scope этого файла (они secondary)». То есть дрейф **задокументирован в owner-файле, но НЕ починен в
  consumer-файлах** — открытый family-repair пункт.
- **`GraphDpoPair` field-count parity — держится:** 14 полей одинаково в plan-01 §5.2, plan-02 §5, plan-05 §5.2
  (владелец P-07). `TeacherProvenance` 20 полей (13+5, `judge_model` внутри) одинаково в plan-01/02/05 (владелец
  P-08). `GraphEdgeKind` 5 членов одинаково в plan-00/02/05. `EdgeAuthorityDecision` 5 полей — PASS21 уже починил
  active-строку (была потеря 4 полей владельца). Расхождений по этим трём — НЕ найдено.
- **`AuthorityUnitId`-by-reference vs by-object:** все три структурных блока (00/02/05) согласованно требуют
  ССЫЛКУ по ID (latest-wins A-LM-03) и явно помечают `object/ILogicalUnit Authority`+`AuthorityUnitRef` как
  устаревшие псевдонимы. Согласовано.

### Что остаётся на проходы 4-9 (уточнение после чтения структурной архитектуры)

- **Пасс 4:** `plan-03` (оркестратор L4 async-обход графа, владелец единого канона ребра + fan-in), `plan-04`
  (L5-машина `DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>`, владелец такта lifecycle), `plan-06`
  (async N-комбинаций 1→N→лучший, владелец `AsyncCandidateBatch`/B-11 материализации).
- **Пасс 5:** `plan-09` (авто-DPO петля — «сердце», 184КБ, потребитель канонов 07/08/10), `plan-11` (измерение
  held-out/ablation/two-PASS), `plan-12` (замена скрытого детерминизма — `MultiSubPhaseRouterUnit`), `plan-13`
  (тест-стратегия 9 слоёв FC-26), `plan-14` (карта референсов), `plan-15` (dual-path proof).
- **Пасс 6:** гайды 02-05 (train / measure / ilearningmachine-pattern / auto-dpo-loop).
- **Пасс 7:** реконы ядра `codex-recon-core-*` (что работает/заглушка в 11.DPO, 18.CapabilityTraining,
  25.ModelServer, 26-router, 27-translation, 42-wiki).
- **Пасс 8:** реконы `codex-recon-mvp-*` + библиотеки знаний (20-pass/40-pass).
- **Пасс 9:** ledger'ы/доски/Fable-checks/open-questions — статусный слой, sync claude↔active; **проверить
  family-repair статус `IDynamicGraphNode` dual-render (00/05 vs owner 01) в `active-...-42-fable-gap-repair`.**

*Конец Pass 3/10. Прочитано ПОЛНОСТЬЮ: active-plan-00 (1099 стр., §0 UML/6 рядов + §4 25-строчный статус +
канон §A.4.1 5-членов/§A.4.2 решатель), active-plan-01 (1251 стр., узел L2 «упрощённая матрица», owner-канон
`IDynamicGraphNode` двух форм + 10-полевой descriptor + `GraphNodePreferenceState` double-веса + миграция 3
классов), active-plan-02 (1405 стр., ребро L3 + единый канон `IDynamicGraphEdge` + 5 `GraphEdgeKind` +
`EdgeAuthorityUnit` анти-детерминизм замена `MultiSubPhaseRouterUnit` + seam узел→ребро + return-loop L3/L4),
active-plan-05 (1096 стр., иерархия L5→L1 Input/Output + 5 цепочек + 7 NEW-типов). Все PLAN-ONLY, verdict
`PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`. Главный флаг: `IDynamicGraphNode` live dual/triple-render (owner 01
`{NodeId,Descriptor}`+generic vs consumers 00/05 `{NodeId,object Unit}`) — задокументирован в owner, family-repair
не завершён.*

## Pass 4/10 — оркестратор (03) + DynamicGraphMachine L5 (04) + async-комбинации (06)

**Источник этого прохода.** Прочитаны ПОЛНОСТЬЮ (full-depth `active-codex-execution-*`, не codex-стабы):
`plan-03-orchestrator` (981 стр.), `plan-04-dynamicgraph-machine` (1111 стр.), `plan-06-async-combinations`
(1642 стр.). Это **РАНТАЙМ + ОБУЧЕНИЕ** графа: L4-оркестратор (обход топологии), L5-машина (единственный
владелец `Train*`), async-комбинации (fan-out/fan-in/many→many + «прогон входа X через N комбинаций → кандидаты DPO»).
Все три — **PLAN-ONLY** (`implementation_authorization: NO until fresh Luba approval`, verdict `PLAN_NOT_READY /
FABLE_GAP_REPAIR_QUEUE`, overlay 2026-07-05). Автор — Claude Opus 4.8 MAX (`claude-*`), сведён в `active-*`.
В конце каждого active-файла — одинаковый addendum (R21 source-authority, DPOLegalLearning workdir, pre-commit
merge-gate, mock-only debt-gate) — это execution-control boilerplate, не содержательный канон блока.

### 4.A) plan-03 — оркестратор L4 `ILearningOrchestrator ≡ ILogicalOrchestrator` (обход графа, async)

**Роль.** L4-«начальник цеха»: берёт один `LogicalOrchestratorRequest`, раскладывает веер узлов (fan-out),
гоняет их async, упорядочивает по зависимостям рёбер, собирает в один результат (fan-in `BranchResults`),
обрабатывает петли возврата (`ReturnLoop`). **Train-логики НЕТ** (A-LM-02, `:8`) — чистый inference-координатор.
Контракт `ILogicalOrchestrator` (`00.Core/LogicalContracts.cs:202`), рантайм `AdaptiveLogicalOrchestrator`
(`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:10`).

**Полный обход `RunAsync` (`:85`) — РЕШЁН в коде сегодня, 10 шагов:** (0) guards — отмена/`null`/пустой
`UserRequest`→`Fail(EMPTY_REQUEST)` (`:96-97`)/`ValidateConstruction()`(`:1236`); (1) флаги `request.Flags ?? new
LogicalExecutionFlags()` (`:105`) — рубильники `UseLearnedUnitWhenAvailable`(`:43`), `UseContextCarryForward`(`:45`),
`UseKnowledgeEdgeDetection`(`:51`), `UseConfidenceFloor`(`:42`), `UseE35DependencyDetection`(`:47`); (2) компиляция
промпта `CompilePromptAsync`(`:244`, резолв `ResolveActiveUnit`:269) → общий `SubPhaseRouterSourceDocument` в metadata;
(3) классификация домена `ClassifyDomainAsync`(`:207`); (4) развилка `IsAdaptiveScenario`(`:1263`): явный сценарий →
`RunExplicitScenarioAsync`(`:283`), иначе адаптивный обход; (5) **FAN-OUT** `RouteAsync`(`:316`): `_multiRouter`
`ExecuteAsync` c `MultiSubPhaseRouterUnit.Id`(`:328`) → `SubPhaseRoutingDecision[]` → `ExpandRoutedDecisions`(`:772`)
вставляет мосты `FindBridgeSubPhases`(`:778`)/`TOPOLOGY_BRIDGE`(`:799`) → `CapRoutedDecisions`(`:808`) лимит
`MaxAdaptiveBranchCount=8`(`:19`); (6) **порядок** `ExecuteBranchesAsync`(`:445`): `OrderByTransitions`(дисп.`:541`/impl`:544`),
`HasTransitionDependency`(дисп.`:522`/impl`:525`) над `DomainGraphClosure.HasAnyDependency`(`:531`, проект 17); нет
зависимостей → **параллель** `Task.WhenAll`(`:465`); есть → **очередь** `foreach` с carry-forward; (7) одно ребро
`ExecuteBranchAsync`(`:496`): низкая уверенность/нет flow → честный `BuildKnowledgeEdgeFlowResult`(`:1112`); иначе
`ExecuteBindingAsync`(`:967`); (8) carry-forward `BuildContextPacket`(`:1032`); (9) **FAN-IN + петля**:
`ScheduleKnowledgeEdges`(`:930`) → `FollowTransitionSignalsAsync`(`:361`) исполняет `ReturnLoop` рекурсивным `RunAsync`
только при `depth < MaxTransitionDepth=3`(`:22`, проверка `:380`) → финал `BuildAggregateResult`(`:841`) собирает
`BranchResults`(`LogicalContracts.cs:190`), худший вердикт `VerdictRank`(`:913`).

**Исправление дрейфа канона ребра (главная delta бенчмарка codex-plan-03):** бенчмарк объявлял `GraphEdgeKind` из
**6** членов `{FanOut,FanIn,ManyToMany,MultiOutput,Sequential,Bridge}` + `AuthorityUnitRef`. Claude выровнял на
**канон 5 членов** `{FanOut,FanIn,ManyToMany,MultiOutput,ReturnLoop}` + флаг `ParallelEligible` (async — режим, не член
enum) + `string AuthorityUnitId` (ссылка в реестр, latest-wins A-LM-03). `Sequential` → `ParallelEligible=false`;
`Bridge` → операция вставки топологии (`FindBridgeSubPhases`), не вид ребра.

**NEW-надстройка (в плане, минимальная правка):** типизированные рёбра `IDynamicGraphEdge`/`DynamicGraphTopology`
вместо голого `DynamicDomainTransition(FromId,ToId)` (`17/DynamicDomainGraph.cs:3`); адаптер топологии поверх
`BuildDynamicDomainTransitions`(`:825`). **Ключ Gate B G5:** подмена baseline→trained fan-out НЕ требует нового кода в
`RunAsync` — обученный multi-router-юнит регистрируется `isLearned:true` и резолвится существующим
`ResolveActiveUnit`(`:269`) + latest-wins (A-LM-03). Носитель A-LM-09 на уровне типа — `bool DeterministicFinalAuthority`
(в проде обязан `false`); детерминистический хелпер как финал ребра → вердикт `FORBIDDEN_RUNTIME_SHORTCUT` (симуляция
S13). **B-16 fan-in typed carrier:** `IEdgeFanInMerger.Merge(branchResults,runId) → GraphFanInMergeResult`
(`FanInMergeRole.DeterministicFanInBaseline`, `MergeOrderIsAuthority=false`) — владелец plan-02; plan-03 не владеет
алгоритмом слияния, его `BuildAggregateResult` — детерминированная N→1 агрегация, которую typed-carrier делает явной.

**Статус L4:** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — обход честный, финал fan-out детерминистический baseline
`MultiSubPhaseRouterUnit` (фикс-веса `0.55/0.25/0.20/0.02`, `:283`, `isLearned:false`). `PASS_AS_TRAINED_FEATURE`
наступит только после обученного multi-router-юнита + checkpoint/ablation/fresh-load (блоки 01/04, фазы Ф4/Ф5/Ф8).
Открытые вопросы §12 — historical/pre-current evidence (Q1 уровень L5, Q2 канон ребра, Q3 обучаемость порядка обхода,
Q4 хранение `AuthorityUnitId`).

### 4.B) plan-04 — L5-машина `DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>` (обучение + фабрика)

**Роль.** L5-«директор завода + ночной отдел обучения» — **единственный владелец `Train*`** (A-LM-02). NEW-класс
`DynamicGraphMachine` (в коде НЕТ, скелет) над NEW-доменом `DynamicGraphDomain`; форма по precedent
`27.TranslationDomain/TranslationMachine.cs:7` (`TrainBatchAsync:45`/`EvaluateAsync:147`/`ExportAsync:199`/
`ImportAsync:222`/`GetOrchestratorAsync:489`). Псевдоним L5 (A-LM-12), **НЕ 6-й уровень**. Локальная карта этапов —
собственное пространство `M1…M12` (crosswalk в глобальную доску Ф4), без коллизии с каноном доски 12/26/59.

**5 методов контракта L5 (`LearningMachineContracts.cs`):** `TrainBatchAsync`(`:212`) — один DPO-шаг, вход
`LearningBatch<DynamicGraphDomain>`(`:18`), выход `LearningBatchResult`(`:43`, before/after accuracy + `PromotedUnitIds`);
`EvaluateAsync`(`:217`) — held-out БЕЗ мутации весов → `LearningEvaluationResult`(`:90`, `PassesGate:100`);
`ExportAsync`(`:222`) → `LearningMachineExport`(`:112`, `CheckpointRef`/`ManifestRef`→`25.ModelServer`, `AccuracyAtExport:123`);
`ImportAsync`(`:227`) — влить checkpoint (latest-wins + continue-training); `GetOrchestratorAsync`(`:232`) — **единственная
точка входа рантайма**, отдаёт L4 (блок 03) с обученным состоянием.

**Обучение (`TrainBatchAsync`):** делегируется `ILogicalCapabilityTrainer.TrainAndRegisterAsync` (A-LM-02, паттерн
`:204-207`) → `DpoTrainer` reward-margin `margin = chosenReward − rejectedReward` (`11.DPO/DpoTrainer.cs:44`),
`loss=−logσ(margin)`(`:47`), `gradMargin=σ(margin)−1`(`:50`), батч-усреднение `RewardMargin=marginSum/count`(`:119`).
Обучаемые цели (machine-coordinated L2-юниты): (1) узловой `PreferenceState` (блок 01); (2) `EdgeAuthorityUnit` (блок 02);
(3) `TrainableGraphDpoBuilderUnit` (владелец-канон блок 07; отвечает на клаузу «DPO built BY ILearningDynamicGraph»;
детерминированный `GraphDpoBuilder` = baseline/fallback). Все трое учатся только через L5.

**Y_best-selection / граница детерминизма (§3.3 таблица владельцев) — ПРЯМОЙ ответ на acceptance/hidden-authority:**
JSON/manifest/checkpoint load-save = infra (allowed deterministic); обход closure/bridge/**Kahn**-levels = baseline/eval-only;
генератор кандидатов `Score`+словарь (`MultiSubPhaseRouterUnit`) = baseline candidate generator; **выбор лучшего пути/ребра/узла
по обученным весам** = обучаемый авторитет `EdgeAuthorityUnit`/узловой `PreferenceState` (**это и обучает машина**);
reward-margin = обучаемый; held-out accuracy = измерение (доказывает, но не выбирает). Правило: всё «нет» помечено
`eval-only/baseline/infra` и НЕ финальная семантическая власть; финал — обучаемый (`DeterministicFinalAuthority=false`).
**Order-канал (B7/B10):** `GraphOrderResolverUnit` — обучаемый L2-резолвер порядка; Kahn/topology/closure = только
перечисление кандидатов + safety/fallback/ablation baseline. Единый `GraphOrderDecision` несёт **точные** carrier-поля
`OrderedPatternIds[]` (accepted) / `RejectedPatternIds[]` (rejected near-miss) — уровни `order-0-direct-unit …
order-4-async-many-to-many-aggregation` (P01/P02 repair снял дрейф двух живых enumerations и «or equivalent»-хедж).
`candidate_order_hash` = set/permutation-доказательство, **НЕ** confidence-вход; `graph_path_ids` = evidence-only,
не learned feature; поля order живут на `GraphOrderDecision`, НЕ на `GraphDpoPair` (lock-14/блок-07).

**Acceptance-signal independence (арбитр ≠ self-confidence обученного юнита):** plan-04 закрывает это КОСВЕННО —
через `DeterministicFinalAuthority=false`, leakage-free вход, `candidate_order_hash` не как confidence, и запрет
`closed_proprietary_model && teacher_output_used_for_training → DATASET_REBUILD_REQUIRED`. **Явный circular-self-labelling
firewall (chosen из self-confidence того же юнита) — НЕ в plan-04**, он живёт в plan-06 (self_best/judge-origin, см. 4.C)
и, по границе ответственности, окончательно в plan-09 (auto-DPO арбитр). **Флаг для пасса 5:** проверить, что блок 09
называет РОВНО ОДИН авторитет над `chosen` (детерминированный Confidence-argmax vs judge/quality-axis) — иначе hidden-authority
ambiguity по правилу CLAUDE.md `Acceptance-signal independence`.

**Lifecycle (Export/Import + load-order + continue-training):** load user/cache checkpoint первым → repo/common вторым →
честный `not_ready`/baseline (M12.1). Latest-wins A-LM-03, discoverable через `IRemoteUnitLocator`. Continue-training =
`ImportAsync(checkpoint) → следующий TrainBatchAsync(новые source-disjoint строки)`, `parent_checkpoint_id` записывается,
rollback сохраняет предыдущий accepted checkpoint locator; **анти-replay**: новые `row_id`/`source_id`, не повтор старого
`batch_id` (masquerade-firewall). `BuildDynamicTopology(signal,budget) → BuildDynamicTopologyResult`
(`Topology`+`BudgetProof`+`FailureReason`+`MutationEvidence`) объявлен **машинным NEW-контрактом (владелец блок 04)**,
т.к. `claude-plan-03:265/:482` атрибутируют этот вход блоку 04; оркестратор его читает, но не строит.

**Gate B (численные locks 2026-06-29, planning lock ≠ runtime proof):** `ablation_threshold_locked=0.02`, held-out
`Accuracy>=0.95`, `AccuracyAfter−AccuracyBefore>=+0.02`, `RewardMargin>=0.025`; fresh-load даёт ту же accuracy (±допуск);
continue-training не деградирует (не replay); ablation (выкл. обучаемый авторитет) роняет held-out ≥0.02; ни одной
train-строки с `closed_proprietary_model && teacher_output_used_for_training`. **Legal teacher-гейт:** closed=judge-only,
`provenance_field_presence_rate==1.0` (20-полевой `TeacherProvenance` = 13 base + 5 NEW + 2 RR6 `service_mode`/`grounding_used`,
canon-владелец блок 08); `GraphDpoPair` = exact 14-field unit-level (canon-владелец блок 07) — блок 04 **синхронизирует, не
переопределяет**. Две PASS-точки: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` vs `PASS_AS_TRAINED_FEATURE`.

### 4.C) plan-06 — асинхронные комбинации (fan-out/fan-in/many→many/return-loop + «X через N комбинаций → кандидаты DPO»)

**Две роли async:** (1) **рантайм-обход** графа (тот же движок, что §3.А plan-03); (2) **поставщик кандидатов DPO** —
прогнать один вход `X` через `N` РАЗНЫХ комбинаций юнитов/маршрутов → `N` кандидатов `Y_i` → `chosen`/`rejected`.
Async — **режим обхода L4/L3, не новый уровень** (A-LM-12), Train-логики в обходе нет (A-LM-02). Локальный каркас
`L1…L12` (детализация одной фазы доски **Ф6**), счётчики локальные, не «канон всех claude-*».

**Рантайм-обход (§3.А, реальные строки):** fan-out `RouteAsync:316`+cap `MaxAdaptiveBranchCount=8`(`:19`); нет зависимостей
→ `var parallelResults = await Task.WhenAll(tasks)` (`:462-465`), первая упавшая → `Fail`(`:468-470`), значения в
**детерминированном порядке маршрута** (`:473`, НЕ кто финишировал первым); есть зависимости → `foreach`(`:478-492`) в
порядке `OrderByTransitions:544` c carry-forward `sequentialResults:481/488`; fan-in `BranchResults`(`:190`, сбор
`:135/:147`) → агрегат `BuildAggregateResult:840`, вердикт **worst-of** `OrderByDescending(VerdictRank):851` (best-of merge
ЗАПРЕЩЁН — прячет брак); return-loop `FollowTransitionSignalsAsync:361` (сбор `TransitionSignals:367`, cap
`MaxTransitionDepth=3`:22, проверка `:380`, повтор `RunAsync:433` depth+1 `:388`).

**Прогон X через N комбинаций (canon-владелец ЭТОТ блок 06; блок 09 — потребитель):** точка входа
`IGraphCandidateRunner.RunCandidatesAsync(GraphCandidateRunRequest request) → AsyncCandidateBatch`. `N = request.CombinationCount`
(единственный источник N), **default 4, потолок 8** (`LOCKED_BY_LUBA_2026_06_30`, потолок = `MaxAdaptiveBranchCount=8`);
валидация отвергает `<2`, `> Budget.MaxCombinations`, потолок `>8` без явного override. Перечисление —
`IGraphCombinationStrategy.Enumerate(input,combinationCount) → CombinationPlan[]`, конкретный класс
`AxisSweepCombinationStrategy` (FABLE-REPAIR; **детерминированный генератор ПУЛА, НЕ обучаемый, НЕ авторитет**). Три оси:
(a) `NodeSetIds[]` (топологически допустимые подмножества из `request.InputGraphRefs`/`InputUnitRefs`, фильтр зависимостей
проекта 17); (b) `EdgeOrderKey` (перестановка порядка рёбер); (c) `Flags` (`LogicalExecutionFlags`). Обход декартова
произведения во вложенном порядке `nodeSubset→edgeOrderKey→flagSet` с возрастающим `Index`; `combination_id =
Hash(NodeSetIds_sorted, EdgeOrderKey, Flags_sorted)`, дедуп по id, различие = ≥1 ось, **без добивки дубликатами**
(`requested_vs_actual`). Чистая функция (нет RNG) → воспроизводимость. **Порядок результата детерминирован по
`CombinationPlan.Index`, не по финишу** (`AsyncCandidateBatch.Candidates` сортирован по `Index`).

**B-11 материализация (PASS09/PASS10 overlay):** публичный выход узла/many→many = `IQueryable<GraphNodeCandidate>`;
`RunCandidatesAsync` материализует queryable **РОВНО один раз** в `IReadOnlyList<GraphNodeCandidate>` для
`GraphCandidateOutput.OutputRefs`/`AsyncCandidateBatch`. `NO_OBJECT_OUTPUT` (сырой `object Output`, untyped-массивы,
финальный `GraphNodeCandidate[]` — НЕ канон). Confidence-прообраз = `TransformerEmbeddingKnnLogicalUnit`:
`Math.Round(Math.Clamp(0.50 + selected.FusedScore*0.49, 0.0, 0.99), 2)` (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs:156`).
Словарь статусов: `OK`/`PARTIAL_FAILURE`/`CANCELLED`/`TIMEOUT`/`EMPTY_CANDIDATES`/`DUPLICATE_CANDIDATES`/`LOW_CONTRAST`/
`INVALID_PROVENANCE`. Бюджет `AsyncCombinationBudget(MaxCombinations, Timeout?, MaxParallelBranches, MaxParallelCombinationRuns=1)`:
три РАЗНЫХ счётчика — `MaxCombinations` (сколько планов строится), `MaxParallelCombinationRuns` (сколько план-прогонов
конкурентно, default 1, `LUBA_PRODUCT_DECISION_RESIDUAL` для >1), `MaxParallelBranches` (ветвевая конкуренция ВНУТРИ обхода).

**Финал ребра = обучаемая `EdgeAuthorityUnit` (canon 02):** async-ветки производят кандидатов/доказательства/замеры, а
выбор edge принимает обучаемый юнит (`OrderedTargets[]` из обученных весов, вход leakage-free). Фикс-формула
`MultiSubPhaseRouterUnit.Score:283` → `baseline_candidate_generator`, `DeterministicFinalAuthority=false`. Блок 06 **только
производит кандидатов**; выбор `Y_best`/winner-selection (B12 `WinnerSelectionDecision` над `CandidateSelectionInput`) —
downstream-потребитель блок 09 (§5.2 явно: метрика Y_best = Confidence кандидата `0.50+fused*0.49:156`; Position/Index —
диагностика, НЕ сигнал выбора).

**Acceptance-signal independence / self_best judge-origin (§3.Г шаг 2 + §8.1) — ЯДРО анти-circular в plan-06:** если teacher
не нужен, `chosen` обязан **побитово совпадать** с одной из реально прогнанных async-комбинаций (§3.Б), иначе closed-судья
мог его «дописать» → `DATASET_REBUILD_REQUIRED`. Под-гейт judge-origin: `judge_did_not_author_chosen==true` И
`chosen_equals_runtime_candidate_or_legal_goal==true`. Teacher для дистилляции = только open-weight permissive (Qwen3
Apache-2.0/DeepSeek MIT/Kimi K2 modified-MIT/GLM/Mistral) через нейтральных провайдеров; Claude/OpenAI/Gemini — **никогда**
teacher, только `judge_model`/eval.

### Сводка implemented-vs-plan-vs-stub (рантайм + обучение)

| Тип/компонент | Статус | Факт |
|---|---|---|
| `ILogicalOrchestrator` L4 + `AdaptiveLogicalOrchestrator` (весь async-обход) | **РАБОТАЕТ** | `LogicalContracts.cs:202`; `AdaptiveLogicalOrchestrator.cs:10` (fan-out `:316`, `Task.WhenAll:462-465`, worst-of `:851`, return-loop `:361` cap `:22`) |
| `ILearningMachine<TDomain>` L5 (контракт, 5 методов) | **РАБОТАЕТ как контракт** | `LearningMachineContracts.cs:208` (`:212/:217/:222/:227/:232`) |
| `DpoTrainer` (reward-margin/loss/grad) | **РАБОТАЕТ как кирпич** | `11.DPO/DpoTrainer.cs:44/47/50/119`; без checkpoint/locator |
| `TranslationMachine` (precedent формы L5) | **РАБОТАЕТ** | `27.TranslationDomain/TranslationMachine.cs:7` (`:45/:147/:199/:222/:489`) |
| `MultiSubPhaseRouterUnit` (детерм. финал fan-out) | **РАБОТАЕТ, BASELINE** (не обучен) | `:283` фикс-веса `isLearned:false`; кандидат №1 на замену |
| `DynamicGraphMachine`, `DynamicGraphDomain`, `IDynamicGraphLegalGate` | **NEW / СКЕЛЕТ (ПРОЕКТ)** | в коде нет; форма по precedent 27 |
| `IDynamicGraphEdge`/`DynamicGraphTopology`, `IEdgeFanInMerger`/`GraphFanInMergeResult` (B-16) | **NEW / СКЕЛЕТ** | в коде нет |
| `IGraphCandidateRunner`/`GraphCandidateRunRequest`/`AsyncCandidateBatch`/`GraphCandidateOutput`/`IGraphCombinationStrategy`+`AxisSweepCombinationStrategy`/`CombinationPlan` | **NEW / СКЕЛЕТ (владелец блок 06)** | в коде нет; `combination_id`=hash осей |
| `EdgeAuthorityUnit`, `GraphOrderResolverUnit`, `TrainableGraphDpoBuilderUnit` | **NEW / СКЕЛЕТ (обучаемые цели L5)** | в коде нет |
| `BuildDynamicTopology(signal,budget)` (владелец блок 04/машина) | **NEW / СКЕЛЕТ** | в коде нет |
| Обученный DynamicGraph (checkpoint/ablation/fresh-load) | **ОТСУТСТВУЕТ** | вывод two-PASS: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` во всех трёх блоках, НЕ trained feature |

### Hidden-authority / circular-self-labelling / dual-canon флаги (что проверить в пассах 5/9)

- **Y_best-selection determinism (главный флаг прохода).** Фикс-формула confidence `0.50 + fused*0.49`
  (`LlmIngredientLogicalUnits.cs:156`) — это ровно паттерн CLAUDE.md `Y_best-selection determinism classification rule`
  («fixed confidence formula silently becomes final authority»). plan-06 §5.2 **классифицирует** его явно: перечислитель =
  baseline pool generator (`DeterministicFinalAuthority=false`), а winner-selection (`Y_best`) вынесен в downstream блок 09
  (B12 `WinnerSelectionDecision`). **НО** финальный авторитет над `chosen` реально решается в блоке 09 — пасс 5 обязан
  проверить, что блок 09 называет РОВНО ОДИН авторитет (детерминированный Confidence-argmax **vs** judge/quality-axis), иначе
  hidden-authority ambiguity (`Acceptance-signal independence rule`).
- **Acceptance-signal independence.** plan-04 закрывает косвенно (leakage-free, `DeterministicFinalAuthority=false`,
  `candidate_order_hash` не confidence); plan-06 даёт self_best/judge-origin firewall (`chosen` побитово = реальная
  async-комбинация; `judge_did_not_author_chosen==true`). **Полный арбитр-независимости (accepted/rejected НЕ из
  self-confidence обучаемого юнита) — концерн plan-09**, пасс 5.
- **Dual-canon order-0..order-4 в plan-04 §8.4 — ПОЧИНЕН.** Раньше были две живые enumerations (Pass01 короткий список
  `unit pattern…graph-policy pattern` + Pass05 детальный `order-0-direct-unit…order-4-async-many-to-many-aggregation`);
  P01/P02 repair notes пометили Pass01 historical, объявили Pass05 single-owner, и заменили «or equivalent»-хедж точными
  carrier-полями `OrderedPatternIds[]`/`RejectedPatternIds[]`. Family-consistency пункт для пасса 9.
- **`AuthorityUnitId` by-reference vs by-object (тот же флаг, что Pass 3).** plan-03/04 требуют `string AuthorityUnitId`
  (ссылка, latest-wins); plan-04 Gate B явно называет прямой объект `ILogicalUnit Authority` в `claude-plan-02:625/:423`
  устаревшим псевдонимом (`claude-plan-00 §A.4.2`). Cross-file family-repair (00/02 vs owner) не завершён — для пасса 9.
- **Ownership `BuildDynamicTopology`** — согласовано: plan-03 читает вход «от блока 04», plan-04 объявляет его машинным
  NEW-контрактом. Расхождения нет.

### Что остаётся на проходы 5-9 (уточнение после чтения рантайм+обучение слоя)

- **Пасс 5:** `plan-09` (авто-DPO петля — «сердце», ПОТРЕБИТЕЛЬ кандидатов блока 06 через `IGraphCandidateRunner`; здесь
  живёт **арбитр winner-selection B12** и полная acceptance-signal independence — приоритетная проверка hidden-authority);
  `plan-11` (измерение held-out/ablation/two-PASS); `plan-12` (замена скрытого детерминизма `MultiSubPhaseRouterUnit`);
  `plan-13` (тест-стратегия 9 слоёв FC-26); `plan-14` (карта референсов); `plan-15` (dual-path proof).
- **Пасс 6:** гайды `02-05` (how-to-train / measure-quality / ilearningmachine-pattern-and-graph / auto-dpo-loop).
- **Пасс 7:** реконы ядра (11.DPO, 18.CapabilityTraining, 25.ModelServer, 26-router, 27-translation, 42-wiki) — что
  работает/заглушка; сверить цитаты `file:line` планов 03/04/06 против реального кода.
- **Пасс 8:** реконы mvp + библиотеки знаний (`knowledge-library-llm-mvp-20-pass`, `-llm-mvp-core-40-pass`).
- **Пасс 9:** ledger'ы/доски/Fable-checks/open-questions (статусный слой, sync claude↔active); проверить family-repair
  статусы (`IDynamicGraphNode` dual-render, `AuthorityUnitId` by-object в 02, order-0..4 canon) в
  `active-...-42-fable-gap-repair`.

*Конец Pass 4/10. Прочитано ПОЛНОСТЬЮ: active-plan-03 (981 стр., L4-оркестратор `RunAsync` обход графа fan-out/параллель/
fan-in/return-loop + исправление дрейфа канона ребра 6→5 членов + NEW-адаптер топологии + Gate B G1-G8), active-plan-04
(1111 стр., L5-машина `DynamicGraphMachine` 5 методов + reward-margin delegation + Y_best determinism §3.3 +
`GraphOrderResolverUnit`/`GraphOrderDecision` carriers + Export/Import lifecycle + Gate B numeric locks + two-PASS),
active-plan-06 (1642 стр., async fan-out/fan-in/many→many/return-loop + `IGraphCandidateRunner` прогон X через N комбинаций +
`AxisSweepCombinationStrategy`/`combination_id` + B-11 материализация `IQueryable→IReadOnlyList` + self_best/judge-origin
acceptance-firewall). Все PLAN-ONLY, verdict `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`. Главный флаг: Y_best-selection
authority над `chosen` вынесен в блок 09 (пасс 5) — проверить, что арбитр РОВНО ОДИН и не self-confidence обучаемого юнита.*

---

## Pass 5/10 — auto-DPO петля (09) + measurement (11) + no-determinism (12) + test/stage (13/14/15)

**Источник этого прохода.** Прочитаны full-depth `active-codex-execution-*` (не codex-стабы): `plan-09-auto-dpo-loop`
(1610+ стр., ПРИОРИТЕТ — «сердце»), `plan-11-measurement` (~900 стр.), `plan-12-no-hidden-determinism` (~900 стр.),
`plan-13-test-strategy` (матрица 14×9), `plan-14-stage-references` (карта референсов), `plan-15-same-scenario-dual-path`
(dual-path proof design). Все шесть — **PLAN-ONLY**: у каждого сверху `Fable Coverage Repair Overlay (2026-07-05)`,
текущий вердикт `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE` (кроме 15: `CLOSED_IN_PLAN_EXECUTION_EVIDENCE_PENDING`),
`implementation_authorization = NO until fresh Luba approval`. Автор — Claude Opus 4.8 (`claude-*`), сведён в `active-*`;
codex-версии — evidence/benchmark only. Этот проход ЗАКРЫВАЕТ дизайн DPO-движка и снимает главный флаг пасса 4
(арбитр winner-selection + acceptance-independence).

### 5.A) plan-09 — авто-DPO петля `AutoDpoLoopOrchestrator` (сердце ILearningDynamicGraph)

**Роль.** Управляющий цикл верхнего уровня графа: превращает обычный прогон проекта на реальной задаче в обучающий
сигнал и непрерывно дообучает узлы/рёбра, пока held-out качество не достигнет порога. NEW-класс `AutoDpoLoopOrchestrator`
реализует L5 `ILearningMachine<DynamicGraphDomain>` (A-LM-02: `Train*` только на машине, `:8`) и владеет ОДНИМ состоянием
графа + одним `_inputQueue`. Человек НЕ пишет пары руками — петля строит их сама из своих ошибок и эталона legal-наставника.

**Оборот петли — 6 шагов (Ф7), полностью решённый алгоритм:**
- **Шаг 0 (Ф7.1.1, `PrepareAsync`):** зафиксировать обучаемую цель (узел/ребро, напр. `orchestrator→unit`), параметры
  (`qualityThreshold`, `maxIterations=20` locked, `plateauWindow`, `minImprovement=+0.02`, `teacher_mode`,
  `teacherGapEpsilon=0.02`); проверить legal teacher (заполнить `TeacherProvenance` 20 полей, §8 G1); загрузить чекпойнт
  user/cache→repo/common→честный untrained scaffold (через `ImportAsync`, `LearningMachineContracts.cs:227`; на контракте
  `LoadCheckpointAsync` НЕТ — это NEW внутренний loader реализации по образцу `TranslationMachine.LoadCheckpointAsync`).
- **Шаг 1 (Ф7.1.2, ОБЯЗАТЕЛЬНО async):** взять вход `X` из `_inputQueue`, прогнать через `N` комбинаций — делегируется
  P-06: петля строит `GraphCandidateRunRequest` и зовёт owner-контракт `IGraphCandidateRunner.RunCandidatesAsync` →
  `AsyncCandidateBatch`. `N = request.CombinationCount` (default 4, потолок 8 = `MaxAdaptiveBranchCount`, `LOCKED_BY_LUBA`).
  Кандидат `Y_i` несёт typed `OutputRefs: IReadOnlyList<GraphNodeCandidate>` (PASS10 материализация `IQueryable→IReadOnlyList`
  РОВНО один раз; сырой `object Output` = `NO_OBJECT_OUTPUT`). Прообраз confidence = `0.50 + fused*0.49`
  (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs:156`).
- **Шаг 2 (Ф7.1.2):** `IGraphWeaknessDetectorUnit` (NEW `ILogicalUnit`) выбирает `Y_best` и помечает слабых кандидатов в
  `rejected`. Confidence здесь — НЕ acceptance-authority, сам строку train-ready сделать не может. Closed-судья разрешён,
  но ТОЛЬКО ranking/scalar/`wrong_reason`. B-15 residue: детектор — v1 diagnostic/ranking scope only; edge/graph
  train-ready отложен, `GraphDpoPair.Level` остаётся `unit` для v1.
- **Шаг 3 (Ф7.1.2):** если `Y_best` хуже наставника — взять эталон `Y_goal` у legal teacher через `ITeacherGoalProviderUnit`
  (NEW внутренний unit, вызывает canonical seam `ITeacherGoalClient`) + `LegalTeacherGate` (владелец блок 08). Если teacher
  не настроен → режим `self_best` (`chosen = Y_best`, `teacher_output_used_for_training=false`), причём `chosen` обязан
  **побитово** совпасть с реальной async-комбинацией (judge-origin firewall).
- **Шаг 4 (Ф7.1.2):** собрать `GraphDpoPair` через `IGraphDpoPairBuilderUnit` (NEW; канон-владелец блок 07, exact 14-field
  с nested `TeacherProvenance`; `GraphDpoProvenanceWriter` пишет manifest/projection proof ДО `DpoTrainReady`) → анти-leakage
  гейты §8 G2 (`overlap<0.60`, нет позиц./path/prompt-утечки, `[WRONG:]` = последствие) → обучить одним батч-шагом через
  `ILearningMachine.TrainBatchAsync` (`:212`) → `LearningTrainingItem`→`LearningBatch`→ реальный `DpoTrainer.StepVector`
  (`11.DPO/DpoTrainer.cs:169-224`). Continue-training из parent-чекпойнта (не с нуля); anti-forgetting replay 70/30
  (образец `16.AutonomousLearning/Training/ReplayBuffer.cs`).
- **Шаг 5 (Ф7.2.1):** измерить рост на **held-out** ДВУМЯ вызовами `EvaluateAsync` (`:217`, у него одиночная `Accuracy`,
  полей before/after НЕТ — они на `LearningBatchResult` от `TrainBatchAsync`): `Accuracy(before)` до train, `Accuracy(after)`
  после на ТОМ ЖЕ source-disjoint наборе. `ACCEPT_ROW` = `verdict=PASS` И `reward_margin≥0` И `loss<0.693` (ln2=random) И
  overlap/ablation/permutation пройдены. **R-19:** `reward_margin` берётся из движка `DpoTrainer.cs`, НЕ из литерала `0.03`
  (`27.TranslationDomain/TranslationMachine.cs:70/125/187` — дефект эталона, чинится в блоке 10). Носитель stop-порога `0.95`
  = `IHeldOutScorer.Score → RuntimeProductAccuracy` (Pass26 mirror, дуал-канон снят), локальная `EvaluateAsync.Accuracy` =
  диагностика батча.
- **Шаг 6 (Ф7.2.2):** промоушен чекпойнта (через `25.ModelServer`, гейт дельты `AutonomousLearningPipeline.cs:137-140`)
  ТОЛЬКО если held-out вырос И принята ≥1 авто-пара; **no-signal ветка** (первоклассная): 0 строк пережили гейты →
  `NO_AUTO_DPO_SIGNAL=true`, `stop_reason=no_signal`, `Promote=false`, локатор остаётся на `previous_checkpoint_ref` +
  `rollback_ref`. `ShouldStop` (условия): порог качества (ВЕТВИТСЯ по `teacher_mode`), плато, бюджет, no_signal.

**Источник входа `X` (RESOLVED_FROM_TASK_PROCESS 2026-07-07):** первичный = **runtime-observations** (наблюдать реальные
прогоны НАШИХ проектов, enqueue'ить именно низко-результатные входы `LowResultObservationRecord`; goal берёт легальная
внешняя модель). Grounding — дословная цитата процесса задачи (строка 20 оригинала, аудит `...-original-task-authority-audit`).
Seed-corpus (`SeedCorpus-LanguageTranslate-v1`) = ТОЛЬКО cold-start bootstrap; FLOW-38 better-output = вспомогательный поток.
Порог `enqueue_threshold = qualityThreshold − enqueueHysteresis = 0.95 − 0.05 = 0.90` (**DERIVED forced**, привязан к locked
held-out gate 0.95); `enqueueHysteresis=0.05` и `observationWindow W=100` = `PROPOSED_DEFAULT` (empirically-tunable).
**enqueue — ТОЛЬКО INTAKE-гейт, никогда финальный runtime-authority** (`learned_authority_remains_final=true`): решает лишь
попадёт ли вход в очередь, НЕ выбирает `Y_best`/`chosen`, НЕ принимает строку, НЕ повышает чекпойнт.

**★ АРБИТР WINNER-SELECTION — РОВНО ОДИН авторитет (снятие главного флага пасса 4; §«Pass 11 B12»).** Канон-владелец
winner-selection = **существующий `IGraphWeaknessDetectorUnit`** (единственный; вводить конкурирующий `CandidateWinnerSelectionUnit`/
`IWinnerSelectionUnit` ЗАПРЕЩЕНО без явного Luba-renaming). Контракт: `CandidateSelectionInput(AsyncCandidateBatch,
GraphOrderDecision?, ConsumerUnitId, LegalProvenanceRefs[], TraceRefs[]) → WinnerSelectionDecision` (поля: `chosen_candidate_ref`,
`chosen_source_kind: runtime_candidate|legal_teacher_goal`, `rejected_candidate_refs[]`, `weak_markers[]`, `confidence`
(диагностика), `judge_origin_status`, `judge_did_not_author_chosen`, `chosen_equals_runtime_candidate_or_legal_goal`,
`failure_code?`). Авторитет над `chosen` по состояниям — РОВНО ОДИН на каждое:
- **cold-start (нет scorer):** `SelectSelfBestColdStart(candidates)` — детерминированный **eval-only baseline** (argmax по
  `score(c)=0.60·verdict_pass + 0.30·schema_valid − 0.10·contamination_hits`, детерминированный total-order tie-break,
  `LOW_CONTRAST` guard `contrast_epsilon=0.05`). Ставит `chosen=Y_best` ТОЛЬКО, НЕ принимает train-ready/промоушен сам,
  и **перестаёт быть авторитетом, как только появляется scorer**;
- **scorer доступен:** `IHeldOutScorer` **вытесняет** cold-start baseline для `Y_best` И является **единственным**
  acceptance-authority;
- **teacher_on:** `chosen` = legal `Y_goal` только после block-08 proof.
Итог: hidden-authority ambiguity НЕТ — по каждому состоянию ровно один именованный авторитет; детерминированный
Confidence-argmax (`0.50+fused*0.49`) как ФИНАЛ **исключён** (см. ниже). **Вердикт: PASS на plan/design-уровне.**

**★ ACCEPTANCE-SIGNAL INDEPENDENCE (OP-66) — НЕТ circular self-labelling / reward-hacking.** Жёсткая граница
(`acceptance_signal_source != trained_unit_self_confidence`; `candidate_producer_self_score != acceptance_signal_source`;
`promotion_signal_source ∈ {held_out_scorer, ablation_delta, no_regression, fresh_load, continue_training}`). Полная
6-строчная классификационная таблица §3-Шаг3:

| Сигнал | Класс | Может выбрать `Y_best`/`chosen`? | Может принять train-ready/промоушен? |
|---|---|---|---|
| `trained_unit_self_confidence` / `0.50+fused*0.49` / self-rank | `diagnostic_only` → `forbidden_self_label` если арбитр | Нет как единственный авторитет | **Нет** (иначе `DATASET_REBUILD_REQUIRED`) |
| Self-best детерм. baseline над runtime-кандидатами | `deterministic_eval_only` | Да, под guard (stable-id + judge-origin/provenance) | Нет сам по себе |
| `IHeldOutScorer` (source-disjoint gold) | `independent_acceptance_signal` | Да (scorer/ranker) | **Да — ПЕРВИЧНЫЙ** (+legal/ablation/no-regression/fresh-load) |
| Legal open-weight teacher goal (после block-08) | `independent_acceptance_signal` | Да (`Y_goal`/chosen для scope) | Да (после provenance/terms/split/quarantine) |
| User better-output feedback (FLOW-38) | `independent_acceptance_signal` | Да (existing better/rejected + provenance) | Только personal/common после legal-гейтов |
| Closed/best судья (score/rank/verdict) | `independent` только для verdict-metadata | Может ranking; **НЕ авторить/редактировать `chosen`/`rejected`** | Только metadata при C11/C17/legal PASS |

Judge-origin под-гейт (измеримый, не декларация): `judge_did_not_author_chosen==true` И
`chosen_equals_runtime_candidate_or_legal_goal==true`; любой false → `DATASET_REBUILD_REQUIRED`. В `self_best` `chosen`
обязан ПОБИТОВО = реальная async-комбинация. **Вердикт: PASS — приёмка независима от self-confidence обучаемого юнита;
closed-судья = eval/ranking-only, авторить `chosen` не может; contamination (sonnet-*/openai/gpt-/claude-/gemini) → REBUILD.**

**§7 честный статус (implemented-vs-plan):** РАБОТАЕТ — DPO-математика (`DpoTrainer.cs:42-65,169-224`), контракт пары
(`11.DPO/Models/DpoPair.cs:3-8`), L5-контракт+DPO-records (`LearningMachineContracts.cs:18-39,208-234`), гейт дельты и
replay (образцы 16), DPO-канал (но вызывается ПУСТЫМ `[]` — `AutonomousLearningPipeline.cs:120`, «розетка без вилки»).
ЗАГЛУШКА — chosen/rejected шаблон-отрицание (`DefaultLogicalCapabilityTrainer.cs:173-174`, overlap~0.70, анти-пример).
ДЕФЕКТ — margin-литерал 0.03 (R-19), checkpoint-root в `GetTempPath()` (`:40-41`, нарушает R5). ОТСУТСТВУЕТ (NEW) —
сам `AutoDpoLoopOrchestrator`, авто-генерация пар из ошибок, `TeacherProvenance`, held-out/ablation per-edge, все
граф-контракты. **Итоговая метка: даже `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` недостижим до реализации NEW; `PASS_AS_TRAINED_FEATURE`
тем более.** Есть рабочие кирпичи, «сердце» (генератор пар + legal teacher + held-out per-edge) отсутствует.

### 5.B) plan-11 — измерение (три оси + две честности + numeric-locks)

**Три оси измерения:** СКОРОСТЬ (`CpuMs`/`PeakRamMb`/`ElapsedMs`/`train_rows`); ТОЧНОСТЬ (**live margin>0** vs
**held-out source-disjoint**); КАЧЕСТВО (novelty / ablation / permutation). **Главный подвох (назван с кодом):** live
reward-margin accuracy (`DpoTrainer.cs:64,120` — доля `margin>0` на train-парах) **≠** held-out точность
(`27.TranslationDomain/TranslationProductHeldOutRunner.cs:369-387`). «Точность» = ТОЛЬКО held-out source-disjoint ПОСЛЕ
проверки контаминации; live `margin>0` доказывает лишь «учится ли вообще».

**Две легальные PASS-метки:** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (контракт есть, честный `not_ready`, нет скрытого
детерминизма, честность доказана тестами) vs `PASS_AS_TRAINED_FEATURE` (три гейта: held-out числа **+** ablation **+**
lifecycle export/import/fresh-load/continue-training через реальный entrypoint).

**Numeric-locks (`LOCKED_FROM_LUBA_OR_PLAN`, PASS08 canonical, plan-11 стр.701-704):** `heldout_delta_min ≥ +0.02`;
`reward_margin_min ≥ 0.025`; `heldout_accuracy_min ≥ 0.95`; `ablation_delta_min = 0.02`. Промоушен-условие (стр.696-697):
`AccuracyAfter ≥ AccuracyBefore + 0.02 && RewardMargin ≥ 0.025 && RuntimeProductAccuracy ≥ 0.95 && RuntimeBootstrapRate ≤ 0.15`.
**5 ablation-контролей** (`Tests/Learn06HeldOutAblationRunnerTests.cs:42`, вердикт `PASS_NAMED_CONTROLS_NOT_LEAKAGE`):
`exact_prompt_replay`, `path_title_source_id_leakage`, `nearest_neighbor_memorization`, `no_trained_state_baseline`,
`shuffled_chosen_rejected`. **Novelty-gate** (`TatoebaPilotLoopRunner.cs:777`): `MinimumNewPatternRate = 1 − maxAccuracy`,
`MaximumSeenTokenRate = maxAccuracy`. **SPEED (Fable-repair):** `SpeedRegressionPolicy` (`Mode=Warn`, `WarnRatio=1.30`,
`MinRunsForRunCost=5`) — advisory в v1; выбор advisory(a) vs blocking(b) = `LUBA_PRODUCT_DECISION_RESIDUAL`, гейт не удаляется
молча. Проекты-образцы приёмки для B6/B7/B15: `TrainableGraphDpoBuilderUnit` (builder_ablation_delta ≥+0.02 vs deterministic
`GraphDpoBuilder`), `GraphOrderResolverUnit` (order-0..order-4, `resolver_off_ablation` роняет held-out), `DynamicGraphMachinePair`
(personal≠global hash, `common_contamination_count==0`).

### 5.C) plan-12 — запрет скрытого детерминизма (9 механизмов + блюпринт замены)

**Канон `classified_mechanism_count == 9` (R-07)**, каждый вердикт ∈ {`OK_ALLOWED_LEVEL`, `OK_INTERNAL_DETAIL`,
`OK_MODEL_COMPONENT`, `BASELINE_MUST_BE_LABELLED`, `FORBIDDEN_RUNTIME_SHORTCUT`}, каждый с `file:line`:

| # | Механизм | file:line | Вердикт → что должно стать |
|---|---|---|---|
| 1 | `MultiSubPhaseRouterUnit.Score` (фикс 0.55/0.25/0.20/0.02) | `21.../MultiSubPhaseRouterUnit.cs:283-284` | **FORBIDDEN** (если финал) → понизить до `IEdgeCandidateProvider` baseline; финал → `SemanticEdgeRouterUnit` |
| 2 | `KeywordUnitScorer.Score` | `26.../KeywordUnitScorer.cs:38,49` | **FORBIDDEN**→`BASELINE_MUST_BE_LABELLED` (baseline-генератор, `isLearned:false`) |
| 3 | `SubPhaseRouterUnit` / `KnowledgeEdge` | `21.../SubPhaseRouterUnit.cs:21,:29` | `BASELINE`(роутинг) + `OK_INTERNAL_DETAIL`(honest-degraded `KnowledgeEdge`) |
| 4 | `OrchestrationScenarioPhasePlanner.PlanPhases` (`text.Contains`) + порядок фаз | `26.../OrchestrationScenarioPhasePlanner.cs:5,:21,:30`; `OrchestrationTaskDecompositionFlow.cs:18-30` | **`BASELINE_MUST_BE_LABELLED`** для v1 (`RESOLVED_FROM_DOCUMENTED_AUTHORITY`, Luba 2026-06-30; ранее FORBIDDEN); обучаемый `SemanticPhasePlanUnit` = будущая опция. L4-путь, НЕ в счёте «9» |
| 5 | `RoutingProfileTrainer.Train` | `26.../RoutingProfileTrainer.cs:5,22` | `OK_INTERNAL_DETAIL` (offline builder) → обучение авторитета переориентировать на L5 `TrainBatchAsync` |
| 6 | `KnnLogicalUnit._index.Search` | `21.../KnnLogicalUnit.cs:218` | `OK_MODEL_COMPONENT` (внутри `SemanticEdgeRouterUnit`, НЕ exact-prompt replay) |
| 7 | `InMemoryLogicalUnitRegistry.ResolveLatest` | `21.../InMemoryLogicalUnitRegistry.cs:107,:118` | `OK_ALLOWED_LEVEL` (latest-wins A-LM-03, cutover после two-PASS) |
| 8 | `MultiSubPhaseRouterUnit.BuildPathFreeScoringText` | `21.../MultiSubPhaseRouterUnit.cs:291,:373` | `OK_INTERNAL_DETAIL` (анти-leakage образец; расширить запретом candidate-index) |
| 9 | Confidence-floor (`decision.KnowledgeEdge`) | `21.../AdaptiveLogicalOrchestrator.cs:504` | `OK_INTERNAL_DETAIL` (honest-degraded антимаскарад) |

Сводка: FORBIDDEN(финал) — 1,2 (до понижения); **немаркированных финальных авторитетов на РЕБРЕ после плана — 0** (Gate B R-07).
**Блюпринт замены:** NEW `SemanticEdgeRouterUnit` (`IsLearned=true`, `DeterministicFinalAuthority=false`) = обучаемый ФИНАЛ
ребра; baseline-роутеры **понижены, НЕ удалены** (нужны как генераторы кандидатов + ablation-контроль); `KnnLogicalUnit` =
model-component внутри; `CandidateOrderLeakageGuardUnit` (ADAPTED-FROM `SkillSelectionLogicalHierarchy.cs:271`) портирован
на ребро; обучение авторитета — через L5 `ILearningMachine.TrainBatchAsync` (`:212`), НЕ детерм. `RoutingProfileTrainer`.
Ablation-проход §3.D: A(обучен)−B(baseline) ≥ `ablation_threshold_locked` иначе `FORBIDDEN_RUNTIME_SHORTCUT`, cutover откат.
**Negative-sweep (evidence-bound):** `TryDecide*|ChooseBestPattern|SelectBestPattern|CountMatchingSignals` = **0 hits** в
`21.AgentInfrastructure`/`26.TrainableRouter`/`16.AutonomousLearning` — скрытого 10-го механизма этих семейств нет.

### 5.D) plan-13/14/15 — тест-стратегия + карта референсов + dual-path

**plan-13 (владелец канона ТЕСТ-СТРАТЕГИИ, Ф9):** матрица **14×9 FC-26** — строки = блоки P-01..P-14, колонки = 9 слоёв
(`unit / integration / e2e-host / QA-automation / UX-automation / negative / untrained-behavior / trained-acceptance /
regression`). **Пустая обязательная клетка = непринятый экзамен (дефект).** Для IsTrainable-строк слой trained-acceptance
разворачивается в ДВЕ под-клетки: **UH** (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`) и **TA** (`PASS_AS_TRAINED_FEATURE`).
`IsTrainable=true` = P-01/P-02/P-04 (3 канон); P-03/P-06/P-09/P-10/P-11 = трейнабл-СМЕЖНЫЕ (наследуют UH/TA от машины P-04,
обязательны e2e/regression); P-05/P-07/P-08/P-12/P-13/P-14 = инфра/гейты/доки (`IsTrainable=false`, TA=N/A с причиной).
Машина `TestStrategyMatrix.Build/Verify` (planning-артефакт → в реализации L2 `ILogicalUnit` для docs-sync), поля вердикта:
`cells_total`, `empty_required_cells`, `trainable_rows_with_uh_and_ta`, `masquerade_open_count`, `pass_ledger_index_only`.
AntiMasqueradeGate на TA: `no_replay_proven=true`. Ledger-строка ≠ принятый экзамен (только с протоколом; FC-27/FC-28).

**plan-14 (владелец канона `GraphReferenceMap`):** навигационный реестр (НЕ обучаемое состояние) — источник канона
(`NN §...` + `file:line`) + проверяемый рекон на КАЖДЫЙ этап; `mapped_target=64` (R11: источник на каждый baseline-подшаг).
Субъект R11 (референсы на этап), R12 (Stage3-читаемость + code-refs), R15 (доп.референс-домены), R18 (авторство по префиксу —
`codex-recon-*` = evidence-разведка, НЕ authority). Source-addressability (все `PATH_EXISTS_VERIFIED` + navigation/library rows):
`Small model racional`, `local wiki learning`, `extensions`, `documentation learning` (все под `C:\Users\lubao\Documents\xiigen\llm mvp\`).
Классы доказательности: `PATH_EXISTS_ONLY`/`LINE_REF_VERIFIED`/`QUOTE_VERIFIED`/`NEEDS_CODE_REF_INVENTORY`/`HISTORICAL_UNVERIFIED`.
Рабочая ветка/workdir = `DPOLegalLearning` под `C:\Projects\DPOLegalLearning\<repo>` (Luba live authority 2026-07-09).

**plan-15 (dual-path proof design, blocker 3 = `CLOSED_IN_PLAN_EXECUTION_EVIDENCE_PENDING`, НЕ execution proof):**
Global Same-Scenario Rule — Path A (automated/API/core) + Path B (local host UI) доказывают ОДИН сценарий только когда
совпадают: `scenarioId` + input fixture + route contract + expected result + visible host state + legal/provenance verdict
+ joinable evidence filename, и join-файл даёт `sameScenarioEquality.matched=true`. `correlationId`/marker сам по себе
НЕДОСТАТОЧЕН. Path A без B = `api_only`; B без A = `ui_only`; любой один = partial/evidence-only, НЕ считается readiness.
Сценарии: `SCN-R16-DESIGN-TO-HTMLCSS`, `SCN-R17` (клиент-surfaces `FigmaToCode`/`CanvaToCode` — будущие Path B цели, НЕ
platform-host authority). Repo-authority: `xiigen-platform-dot-net-angular` + `xiigen-flows-dot-net-angular` + `llm_mvp_core`.

### Сводка Pass 5 (implemented-vs-plan-vs-stub, DPO-движок)

| Компонент | Статус | Факт |
|---|---|---|
| DPO-математика (margin/loss/acc/StepVector) | **РАБОТАЕТ** | `11.DPO/DpoTrainer.cs:42-65,169-224` |
| DPO-канал в обучение | **РАБОТАЕТ, но вызывается `[]`** | `16.AutonomousLearning/.../AutonomousLearningPipeline.cs:120` |
| Гейт дельты промоушена + replay 70/30 | **РАБОТАЕТ (образцы)** | `AutonomousLearningPipeline.cs:137-140`; `ReplayBuffer.cs` |
| held-out runner (пилот) + ablation-контроли | **РАБОТАЕТ (пилот language-translate)** | `TranslationProductHeldOutRunner.cs:369-387`; `Learn06HeldOutAblationRunnerTests.cs:42` |
| chosen/rejected шаблон-отрицание | **ЗАГЛУШКА/анти-пример** | `DefaultLogicalCapabilityTrainer.cs:173-174` (overlap~0.70) |
| margin-литерал 0.03 (R-19), checkpoint temp-root | **ДЕФЕКТ** | `TranslationMachine.cs:70/125/187`; `DefaultLogicalCapabilityTrainer.cs:40-41` |
| `AutoDpoLoopOrchestrator`, `IGraphWeaknessDetectorUnit`(B12), `ITeacherGoalProviderUnit`, `IGraphDpoPairBuilderUnit`, `SemanticEdgeRouterUnit`, `GraphOrderResolverUnit` | **NEW / СКЕЛЕТ (кода нет)** | все проектируются в 09/12; petля отсутствует |
| Legal teacher + `TeacherProvenance`(20), held-out per-edge, граф-контракты | **ОТСУТСТВУЕТ (NEW)** | полей нет ни в .cs, ни в jsonl |
| Обученный DynamicGraph (checkpoint/ablation/fresh-load) | **ОТСУТСТВУЕТ** | вердикт two-PASS: даже honest-scaffold недостижим до реализации NEW |

### Что остаётся на проходы 6-9

- **Пасс 6:** гайды `guide-02..05` (how-to-train / measure-quality / ilearningmachine-pattern-and-graph / auto-dpo-loop) —
  сверить методический слой против планов 09/11/12.
- **Пасс 7:** реконы ядра — открыть реальный код и проверить цитаты `file:line` планов 09/11/12 (11.DPO, 18.CapabilityTraining,
  21.AgentInfrastructure, 26.TrainableRouter, 27.TranslationDomain, 16.AutonomousLearning, 25.ModelServer): что работает/заглушка;
  особенно `MultiSubPhaseRouterUnit.cs:283`, `KeywordUnitScorer.cs:38`, `DpoTrainer.cs:42-65/169-224`, `AutonomousLearningPipeline.cs:120/137-140`.
- **Пасс 8:** реконы mvp + библиотеки знаний (`knowledge-library-llm-mvp-20-pass`, `-llm-mvp-core-40-pass`).
- **Пасс 9:** статусный слой (ledger'ы/доски/Fable-checks/open-questions, sync claude↔active); family-repair статусы
  (`IDynamicGraphNode` dual-render, `AuthorityUnitId` by-object в 02, order-0..4 canon) в `...-42-fable-gap-repair`;
  проверить, что Fable overlay 2026-07-05 не оставил открытых MISSING/PARTIAL по DPO-движку.

*Конец Pass 5/10. Прочитано full-depth: active-plan-09 (петля 6 шагов Ф7 + B12 winner-selection единственный владелец
`IGraphWeaknessDetectorUnit` + OP-66 acceptance-independence 6-строчная таблица + judge-origin firewall + no-signal/ShouldStop
+ input-source runtime-observations + §7 честный статус), active-plan-11 (три оси + две PASS-метки + numeric-locks
0.02/0.025/0.95/ablation-0.02 + 5 ablation-контролей + novelty-gate + SpeedRegressionPolicy), active-plan-12 (9 механизмов
классифицированы + блюпринт `SemanticEdgeRouterUnit` + negative-sweep 0 hits), active-plan-13 (матрица 14×9 FC-26 UH/TA),
active-plan-14 (`GraphReferenceMap` + source-addressability), active-plan-15 (dual-path Same-Scenario Rule). ★ ГЛАВНЫЙ ФЛАГ
ПАССА 4 СНЯТ: арбитр winner-selection = РОВНО ОДИН владелец (`IGraphWeaknessDetectorUnit`); авторитет над `chosen` по
каждому состоянию единственный (cold-start deterministic-baseline-under-guard → вытесняется `IHeldOutScorer`; teacher_on →
legal `Y_goal`); self-confidence `0.50+fused*0.49` исключён как финал (`diagnostic_only`/`forbidden_self_label`); приёмка
НЕЗАВИСИМА от self-confidence обучаемого юнита (`acceptance_signal_source != trained_unit_self_confidence`) — circular
self-labelling/reward-hacking закрыт. Вердикт design-уровня: PASS; все шесть файлов PLAN-ONLY, `PLAN_NOT_READY`,
implementation_authorization NO до свежего approve Luba.*


---

## Pass 6/10 — гайды: how-to build-DPO/train/measure/pattern/auto-loop (codex-guide-01..05 + claude-01/03/05/06)

**Что покрывает этот проход.** Методический слой папки — «как построить DPO / как обучить /
как измерить / паттерн ILearningMachine / замкнутая авто-DPO петля». Это и есть требование
**M1 «гайды»**, ради которого весь проект: научить исполнителя строить пары, обучать узел/ребро
и мерить, что он именно обучился, а не схитрил. Полнодепный разбор пяти зрелых мануалов
`active-codex-execution-guide-01..05` (в тексте они называют себя «claude-гайд», это уже
слитые/канонические версии, `populated_now`, Fable-overlay 2026-07-05) + четырёх тонких ранних
компаньонов `claude-01/03/05/06` (2026-06-23, фаза «library creation only»).

### 6.0. Два поколения гайдов — не путать

| Слой | Файлы | Дата/статус | Роль |
|---|---|---|---|
| **Зрелые мануалы (канон)** | `active-codex-execution-guide-01..05-*.md` (по ~85–105 КБ) | `populated_now`, file:line-verified, Fable-overlay 2026-07-05 | Полный how-to: §2 концепт / §3 код-сейчас (file:line) / §4 рецепт / §5 примеры / §6 гейты / §7 пробелы / §8 тест-стратегия FC-26 / §9 Part A UML. **Это M1.** |
| **Ранние компаньоны (тонкие)** | `claude-05` (4.7 КБ), `claude-01` (5.2 КБ), `claude-03` (9.3 КБ), `claude-06` (5.6 КБ) | `populated_now_no_training_run`, 2026-06-23 | Скелет фазы наполнения библиотеки; без file:line (кроме `claude-03`, дополнен B-21-реконом 2026-06-30). Эволюционировали в зрелые мануалы. |

Зрелые мануалы **надстраивают** тонкие: тот же скелет, но с проверенными строками кода, каноном
14-польного `GraphDpoPair` / 20-польного `TeacherProvenance`, дельта-нотами против бенчмарка и
тест-стратегией по слоям. При конфликте канон = зрелые guide-* + мастер `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1`.

### 6.1. Guide-01 «Как строить DPO-пары» → план **07** (builder) + **08** (legal-gate)

Чему учит (методический костяк):
- **Концепт (§2):** DPO = учить на РАЗНИЦЕ chosen vs rejected, где `[WRONG: код последствие]` —
  подпись ошибки. Ключ: rejected = **near-miss** (одна осмысленная ошибка), а не мусор («I don't
  know» ⇒ `QUARANTINE generic_bad_output`). Две разновидности пар: **на узле** (для входа X узла
  правильный выход chosen) и **на ребре** (правильно смаршрутизировать в узел A, а не B). Роли
  разделены: **teacher** (автор эталона) ≠ **judge** (только «этот лучше того»); закрытые модели —
  только judge при `used_for_training=false`.
- **Код-сейчас (§3, всё file:line-проверено):** движок шага `11.DPO/DpoTrainer.cs` РАБОТАЕТ (но пар
  не строит); плоский `DpoPair.cs:3-8` (3 поля); построители пар узлов РАБОТАЮТ как детерминированные
  fixture-builders (`WikiContextRoutingDpoGenerator.cs:17-94`, 12 `*DpoGenerator.cs` в 42), но chosen =
  self-label; edge/grounding-пары `OrchestrationCorpusBuilder.cs:96-262` с анти-overlap-гейтом
  `NoExactPromptOverlap` (:243-254); самый зрелый легальный образец — `43.SmartSkillsDomain/ResultGapDpoWriters.cs`
  (source-descriptor :7-14, privacy-router :111-153, лицензии MIT/Apache-2.0/ProjectInternal/Synthetic :192-196,
  schema-validator анти-leakage :241-266, near-miss-таксономия :199-235).
- **Рецепт (§4, 8 шагов):** 0 классифицировать уровень → 1 legal gate ДО построения → 2 сформировать A
  (без candidate-index/source-path/exact-prompt) → 3 chosen от легального teacher/first-party → 4 rejected
  как near-miss + `[WRONG:]` → 5 overlap<0.60 + permutation-инвариантность → 6 schema-валидатор →
  7 проекция в `LearningTrainingItem` → JSONL `DpoPair` → 8 честный статус (двух-PASS).
- **Канон `GraphDpoPair` = ровно 14 полей** (§6.5, владелец 07; `PairId, Level=unit, NodeOrEdgeId,
  InputContext, Chosen, Rejected, WrongMarkers[], Reason≥24, TeacherProvenance(nested 20), SourceIds[],
  HeldOut, CandidateOrderInvariant, ExcludedLeakageSignals[], DpoTrainReady`). **PASS02 override:** train-ready
  пара — **unit-level** сравнение, не whole-graph/path; graph path/order = конверт, не поля пары.
  Вердикт-enum (§6.6, владелец 08): `ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY`;
  предусловие `dpo_train_ready=true` = триада (ACCEPT + provenance 20/20 + triad_present + overlap<0.60 + reason≥24 + order-invariant).
- **Таксономия `rejected_origin`:** `random-rejected / copied / missing-source / teacher-unknown / split-leak /
  order-artifact / baseline-loser` (последний — мост к async-комбинациям блока 06).
- Worked-примеры (§5): узловая пара wiki-роутинга (chosen `route_kind:Mode2SemanticHybrid` vs rejected
  `always_bm25` с `[WRONG:]`); рёберная пара FLOW-33 grounding; ref-only строка диска с `source_ref:sonnet-*`
  (⇒ проверить teacher-гейтом, если `used_for_training=true` → `DATASET_REBUILD_REQUIRED`).

### 6.2. Guide-02 «Как обучать один юнит» → план **10** (lifecycle) + движок **11.DPO**

Чему учит:
- **Аналогии (§2):** два экзаменатора (reference frozen vs policy trainable); LoRA = «стикеры поверх
  книги»; reward margin = «насколько уверенно лучше»; continue-training ≠ обучение с нуля; lifecycle =
  где модель живёт после обучения.
- **Код-сейчас (§3):** `DpoTrainer` РАБОТАЕТ — формула в шапке `:9`, `Beta=0.1` `:13`, margin `:44`,
  `StepMany` `:72-125`, **`EvaluateMany` `:127-167` = held-out-оценщик движка**, `StepVector` `:169-224`,
  `VectorSignal=clamp(1+slots/60,1..2)`. **LoRA (`10.LoRA`) РАБОТАЕТ как математика, но НЕ вклеена в
  Forward** (`LoraApplicator.Apply` возвращает адаптеры «сбоку», `SetValue` нет — пробел). Единственный
  реальный цикл pair→LoRA→DPO→метрики — `LogicalDpoTrainingUnit` (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs:308-415`).
  **`IncrementalTrainer` (14) — ЗАГЛУШКА** (`:38-53` строит путь, `Task.CompletedTask`, весов не пишет —
  Layer-5 FAIL). ModelServer (25) ЧАСТИЧНО (нет сетевого export весов). Эталон lifecycle — `27.TranslationDomain`
  (Save `:384-388`, Load `:444-480`, Import `:298-360`), НО с ловушкой **R-19: захардкоженный `RewardMargin:0.03`**
  на `:70/:125/:187`.
- **Рецепт (§4, 12 шагов):** уровень → пары → 2 копии модели (policy+reference) → LoRA к policy →
  continue-training (нюанс §3.4: чекпойнт грузится ПОСЛЕ заморозки) → токенизация → `StepVector` цикл с
  `ConvergenceMonitor` → held-out через `EvaluateMany` → промоушен по дельте (`+0.02`/reward`≥0.025`/product`≥0.95`) →
  save checkpoint+manifest → publish+locator → fresh-load+continue проба → split R5 (v1 common-only).
- **Solved Architecture Block (§6.6):** единственный обучаемый суспект гайда — **кто авторитет числа
  reward margin**; решение: только `DpoTrainer` (`:44/63`), literal `0.03` удаляется. **11 пробелов (§7)**,
  каждый привязан к R-ID/блоку: R-08 (LoRA wiring, IncrementalTrainer, IModelServer, ранг 2vs8), R-19/R-09
  (margin из движка + held-out), R-05/R-17 (teacher-provenance), R-10 (split lifecycle, дубль реестров).

### 6.3. Guide-03 «Как мерить скорость/точность/качество» → план **11** (measurement)

Чему учит (три оси):
- **Концепт (§2):** нанимаешь переводчика. СКОРОСТЬ (cpu/ram/elapsed/rows) — бюджет, не качество.
  ТОЧНОСТЬ = НЕ путать live-accuracy (доля margin>0 на train, «учится ли вообще») и **held-out** (экзамен
  на новых строках — это и есть приёмочная точность). КАЧЕСТВО = доказать «научился, а не схитрил» три
  ловушки: зубрёжка (novelty-gate), тайная шпаргалка (ablation `no_trained_state_baseline`), подсматривание
  разметки (permutation/leakage). Две честности: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (веха) ≠ `PASS_AS_TRAINED_FEATURE`.
- **Код-сейчас (§3):** скорость РАБОТАЕТ (`TranslationProductHeldOutRunner.cs:239-241`); точность = ДВА числа
  (`DpoTrainer.cs:64/120` live vs held-out `:369-387` с блоком контаминации `TRAIN_HELDOUT_OVERLAP` `:285-287`);
  качество РАБОТАЕТ — промоушен-гейт `:211-220` (9 порогов), novelty-gate `TatoebaPilotLoopRunner.cs:777`
  (`newPattern>(1-maxAcc) AND seenTokens<maxAcc`), ablation `Learn06HeldOutAblationRunnerTests.cs:42` (5
  именованных контролей), честный not_ready `PromptToTaskMachineSkeletonTests.cs:18-22` + рефлексия-запрет
  `Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt` `:79-85`.
- **10 гейтов измерения (§6):** Гейт 1 (не путать live/held-out), 1a (loss<0.685 сигнал; ≥0.693≈ln2 =
  random = брак; 0.685..0.693 = `WEAK_NEAR_RANDOM`), 1b (Final<Average ⇒ `UNSTABLE_RUN`, брать best-checkpoint),
  2 (Runtime Evidence Firewall, UNKNOWN запрещён для done), 3 (Two-PASS OP-57), 4 (permutation per-edge),
  5 (нет скрытого детерминизма), 6 (legal teacher), 7 (delta-порог 5%), 8 (near-dup блокирует accuracy),
  9 (SCALE_UP ≠ провал), 10 (capability score только с runtime-командой).
- **Живые числа v196 (§5.2, реконом):** `AvgAcc≈0.6228, FinalAcc≈0.5833, AvgLoss≈0.6873, FinalLoss≈0.6882,
  AvgMargin≈0.01217, FinalMargin≈0.01013` ⇒ гайд честно читает это как `WEAK_NEAR_RANDOM` + `UNSTABLE_RUN`
  (сам прогон НЕ дотягивает до trained-сигнала). **9 пробелов (§7):** единый `MeasurementReport`
  (3 оси), per-edge ablation/leakage, реальный held-out-раннер на каждый уровень (`IGraphMeasurementRunner`),
  вычисленный margin, continue-training проба, анти-replay для `_exactSourceMap`, teacher-provenance,
  авто-генератор троек с встроенными гейтами, переносимые пути (не `C:\Users\lubao\.xiigen\`).

### 6.4. Guide-04 «Паттерн ILearningMachine + граф» → план **01/02/03/04** + **05** (hierarchy)

Чему учит:
- **Аналогия завода (§2):** станок=`ILogicalUnit`(L2), линия=`ILogicalFlow`(L3), стрелочник развилки=
  **`EdgeAuthorityUnit`**(NEW, обучаемый), начальник цеха=`ILogicalOrchestrator`(L4), директор с отделом
  обучения=`ILearningMachine`(L5). Правило A-LM-02: перенастраивать станки может ТОЛЬКО директор (Train
  только на машине). Граф = ленты перекладываются на лету (fan-out/fan-in/return-loop); топология = данные
  в реестре. Опаснейшее место — развилка: тупой счётчик/таблица слов = детерминистический shortcut,
  допустим только как baseline/генератор кандидатов, финал решает **обученный** `EdgeAuthorityUnit`.
- **Код-сейчас (§3, сводка §3.9):** 5 уровней РАБОТАЮТ как контракты (`LogicalContracts.cs:97/166/202`,
  `LearningMachineContracts.cs:208/245`, A-LM-02 в комментарии `:8`); DPO-формат/метрики РАБОТАЮТ
  (`LearningTrainingItem :30`, `LearningBatchResult :43-62` с обязательными числами); граф-субстрат РАБОТАЕТ,
  но **детерминистический** (`07.GraphEngine/GraphNode.cs:3` — мутируемо только `PageRankScore`; `GraphEdge.cs:3`
  — вес init=1.0, НЕ обучаем); динамика топологии РАБОТАЕТ детерминистично (`17.DynamicDomains`, mutation-юниты в 21);
  **главный кандидат на shortcut** — `MultiSubPhaseRouterUnit.cs:8/249-288` (словарь якорных слов `:23-56`,
  фикс-веса `:283/284`), честно `isLearned:false` = кандидат №1 на замену `EdgeAuthorityUnit`; `KnnLogicalUnit.cs:33/98`
  — обучаемый KNN-компонент (construction-time `Train`, не нарушает A-LM-02); 6 Bootstrap-узлов = honest scaffold.
- **Главный факт-вывод:** иерархия 5 уровней есть как контракты, граф-субстрат есть как детерминистический
  код, но **обученного DynamicGraph НЕТ**. `ILearningDynamicGraph` = **псевдоним** `ILearningMachine<DynamicGraphDomain>`
  (не 6-й уровень, A-LM-12). **9 пробелов (§7):** рёберные контракты как типы (multi-output/fan-in/per-edge-authority/
  async), обучаемая замена роутера 26, teacher-provenance-типы, lifecycle trained-замены 6 Bootstrap-узлов,
  Jaccard E49/E50 как baseline, размещение common vs adaptive, тесты untrained-честности, статус домена в DECISIONS-LOCKED.

### 6.5. Guide-05 «Замкнутая авто-DPO петля» (сердце проекта) → план **09** + **06** (async) + **12** (no-determinism)

Чему учит (это уже разбиралось со стороны plan-09 в Pass 5 — здесь метод-слой):
- **Контур (§3):** DPO-движок РАБОТАЕТ (`DpoTrainer.cs:42-65`); авто-построение «из наблюдённой ветки»
  РАБОТАЕТ ЧАСТИЧНО (`DefaultLogicalCapabilityTrainer.cs:167-175`), но **chosen/rejected — детерминированный
  шаблон** (`BuildChosenCapabilityText :680-683`; rejected `"Do not generate a capability..." :174` — это
  анти-пример, overlap~0.70, провал калибровки 2026-05-06); контур «обучить→измерить→повысить» РАБОТАЕТ как
  образец (`16.AutonomousLearning`), НО **DPO-канал ПУСТОЙ**: `AutonomousLearningPipeline.cs:120` зовёт
  `TrainAsync(..., [])` — «розетка без вилки»; сигнатура принимает DPO-пары (`AutonomousTrainer.cs:44-47`), но
  генератора «слабое место→пара» нет. `26.TrainableRouter` — детерминированный keyword-scorer (`FORBIDDEN_RUNTIME_SHORTCUT`).
- **Рецепт петли (§5, 6 шагов, Ф7):** 0 подготовка (subject + legal teacher + чекпойнт) → 1 прогон входа X
  через N разных комбинаций (`IGraphCandidateRunner.RunCandidatesAsync`, **N LOCKED=4, потолок 8**) → 2 обнаружить
  слабый выход, выбрать `Y_best` → 3 взять `Y_goal` у legal teacher (или `chosen=Y_best`) → **4 обучить L5
  СТРОИТЬ DPO**: ядро R-C — обучаемый `TrainableGraphDpoBuilderUnit` (реализует `IGraphDpoPairBuilderUnit`,
  learned pair-construction policy), детерминированный `GraphDpoBuilder.BuildPairAsync` — только baseline/validator/
  fallback (если шаг 4 ТОЛЬКО через него — R-C остаётся скрытым детерминизмом!) → 5 измерить на held-out → 6
  промоушен по дельте + ledger + regression-окно 3.
- **6 гейтов петли (§6):** G1 legal teacher (closed → `DATASET_REBUILD_REQUIRED`), G2 анти-leakage пары
  (overlap<0.60, order-invariant), G3 no-hidden-determinism (per-edge authority), G4 held-out измерение,
  G5 lifecycle (checkpoint-root ≠ `GetTempPath`), G6 две PASS-метки. **Gate B (§7)** = 8 exit-условий Ф7.
  **9 пробелов (§9):** координатор `GraphAutoDpoLoop` + генератор из ошибок, legal teacher (08), held-out+ablation
  (11), граф-контракты+субъекты, замена роутинга 26 (Ф8), lifecycle публикации, provenance chosen/rejected в
  корпусах (блокер legal — `sonnet/GPT-5/Codex` rollout запрещён как teacher), расхождение версий train-ready V192/V309,
  связка «одна строка за раз» + regression.

### 6.6. Тонкие компаньоны claude-01/03/05/06 — что несут, чем короче

- **`claude-05` (DPO data training measurement guide, 4.7 КБ):** ранняя объединённая версия build+train+measure.
  Даёт: минимальную форму пары (`prompt/chosen/rejected` + graph-metadata `chosen_path/rejected_path/
  source_disjoint_group/...`), 8-шаговую pair-generation-стратегию, **judge-таблицу** (Human=gold; open-model=
  future conditional; closed API=blocked now; deterministic checker=allowed helper), 6 measurement-families
  (DPO/routing/branch/lifecycle/leakage/resource), anti-deterministic warning (learned claim требует 7 evidence),
  async-комбинации (candidate A/B/C/D), 10 quality-gates. Без file:line. → зерно зрелых guide-01/02/03.
- **`claude-01` (authority requirements ledger, 5.2 КБ):** НЕ методический — фиксирует, что фаза = «создать
  библиотеку планов в папке `dpo legal build`», 20 требований R-001..R-020 (write-scope только `claude-*`
  basenames, no execution/git/train, questions только в claude-07). Важно для прохода 9: объясняет, почему
  ранние claude-* тонкие (это была фаза наполнения, не реализации).
- **`claude-03` (ILearningMachine pattern notes, 9.3 КБ):** самый содержательный тонкий — иерархия, роли-таблица,
  fit `ILearningDynamicGraph=ILearningMachine<DynamicGraphDomain>`, 7 reuse-rules, Boundary Warning (closure/
  bridge/PageRank ≠ доказательство обучения). **Дополнен B-21-реконом (2026-06-30)** с 12 проверенными
  строками-якорями в 10 файлах (`SourceIds :26`, `LearningTrainingItem :30`, `DpoTrainer.cs:44` margin,
  `AdaptiveLogicalOrchestrator.cs:19` MaxBranch=8, `TranslationMachine.cs:70/187` литерал 0.03). Честный boundary:
  «12 строк, не 40 проходов, не весь репозиторий».
- **`claude-06` (architecture brief, 5.6 КБ):** component-view/data-flow/graph-state диаграммы (Mermaid),
  routing-model I/O, async-experimentation, интерфейсы в прозе (`DynamicGraphMachine/DpoPairBuilder/LegalGate/
  TrainingRunner/MeasurementRunner/Orchestrator`), 9 solved-subtasks, evidence-to-architecture mapping. → зерно guide-04.

### 6.7. Guide↔code drift — НЕЗАВИСИМАЯ проверка (моя, этой сессии, `C:/Projects/llm_mvp_core`)

Проверял не по реконам, а прямым чтением боевого чекаута. **Вывод: гайды цитируют РЕАЛЬНЫЕ file:line,
контент точен построчно на самых нагруженных якорях; дрейф — только ±2-3 строки в одном файле.**

| Якорь гайда | Прямое чтение (эта сессия) | Вердикт |
|---|---|---|
| `DpoPair.cs:3-8` (3 поля `Prompt/Chosen/Rejected`, init="") | Точно так, namespace `DPO.Models` | ТОЧНО |
| `DpoTrainer.cs:42-44` margin / `:47` loss / `:57-65` metrics / `:64` Accuracy | Факт: chosenReward **:44**, rejectedReward :45, margin **:46**, loss **:49**, metrics **:59-67**, Accuracy **:66** | дрейф **+2..+3 строки** (контент идентичен; гайд «раньше» факта) |
| `IncrementalTrainer.cs:38-53` заглушка `TrainAsync` (path + `Task.CompletedTask`, честный комментарий) | Дословно: комментарий «LoraApplicator.Apply and DpoTrainer.Step are wired up in 16.AutonomousLearning ... Here we record the intended adapter path» на :48-49 | ТОЧНО |
| `TranslationMachine.cs:70/125/187` литерал `0.03` (R-19) | `70: RewardMargin: 0.03,` / `125: ? 0.03` / `187: RewardMargin: _trainingHistory.Count>0 ? 0.03 : 0.0,` | ТОЧНО построчно |
| `AutonomousLearningPipeline.cs:120` пустой `[]` DPO-канал | `120: var trainResult = await _trainer.TrainAsync(domain, weightedExamples, []);` | ТОЧНО |
| `MultiSubPhaseRouterUnit.cs:283/284` фикс-веса; `:293` исключение SourcePath | `283: (predicateScore*0.55)+(exactSubPhaseMention*0.25)+(tokenScore*0.20)+(hybridScore*0.02)`; `284: ...*0.50...+(contextScore*0.10)`; `293: // Join semantic document fields while deliberately excluding SourcePath.` | ТОЧНО |

Дополнительно: гайды **сами честно фиксируют и правят дрейф** — guide-02 §3.7 уточняет R-19 (бенчмарк
цитировал `:123-126`, литерал `0.03` на `:70/:125/:187`); guide-03 §8 правит held-out `:369-374` (составные `:375-387`)
и not_ready skeleton `:18-22`. Guide-01 §3.6 честно помечает число «19 файлов/72» (teacher-provenance) как
**benchmark-sourced ориентир, а не live-факт диска** — Ф2.1.3 обязан пере-сканировать при исполнении.
**Итог drift-проверки:** это НЕ выдуманные ссылки; мануалы file-anchored и самокорректирующиеся. Единственное
систематическое расхождение — `DpoTrainer.cs` уехал на +2-3 строки от версии гайда (эволюция файла), контент совпадает.

### 6.8. M1-guides coverage — сводка

| M1-требование гайда | Файл-владелец | Карта в план | Глубина | Статус |
|---|---|---|---|---|
| **как строить DPO** | guide-01 | блок 07 (+08) | full (§2-§9 + UML) | покрыто |
| **как обучать** | guide-02 | блок 10 (+11.DPO) | full (12-шаг рецепт, 11 пробелов) | покрыто |
| **как мерить (speed/accuracy/quality)** | guide-03 | блок 11 | full (3 оси, 10 гейтов, живые числа v196) | покрыто |
| паттерн ILearningMachine + граф | guide-04 | блоки 01/02/03/04/05 | full (5 уровней, §3.9 сводка) | покрыто |
| авто-DPO петля (сердце) | guide-05 | блоки 09/06/12 | full (6-шаг Ф7, 6 гейтов, Gate B) | покрыто |
| ранние тонкие компаньоны | claude-05/01/03/06 | — | thin (зерно + ledger + pattern + arch-brief) | учтены, эволюционировали в guide-* |

**Вердикт Pass 6:** методический слой M1 (build/train/measure + pattern + auto-loop) присутствует **полностью
и на полной глубине**, file:line-verified, самокорректирующийся по дрейфу, синхронизирован с каноном
`GraphDpoPair`(14)/`TeacherProvenance`(20)/`GraphEdgeKind`(5)/`EdgeAuthorityUnit` и с блоками плана 07/08/09/10/11/12.
Все гайды PLAN-ONLY (`PLAN_NOT_READY`, implementation_authorization NO до свежего approve Luba). Единый сквозной
**центральный дефект**, который все 5 гайдов называют одинаково: **обучение не подключено в проде** — DPO-канал
пуст (`AutonomousLearningPipeline.cs:120` `[]`), reward margin захардкожен (`TranslationMachine.cs:70/125/187` `0.03`),
финальный авторитет ребра детерминирован (`MultiSubPhaseRouterUnit.cs:283`), генератора «слабое место→пара» нет.

### Что остаётся на проходы 7-9

- **Пасс 7:** реконы ядра — открыть реальный код и проверить цитаты file:line планов 07/09/10/11/12
  (`11.DPO`, `18.CapabilityTraining`, `10.LoRA`, `16.AutonomousLearning`, `25.ModelServer`, `26.TrainableRouter`,
  `27.TranslationDomain`). Часть якорей уже независимо подтверждена в §6.7 (DpoPair, DpoTrainer ±2-3 стр,
  IncrementalTrainer-заглушка, R-19, пустой DPO-канал, роутер-фикс-веса) — пассу 7 остаётся `LogicalDpoTrainingUnit.cs:308-415`,
  `LocalModelRepository.cs`, `ResultGapDpoWriters.cs`, Bootstrap-узлы 42.
- **Пасс 8:** реконы mvp + библиотеки знаний (`knowledge-library-llm-mvp-20-pass`, `-llm-mvp-core-40-pass`);
  реконы `codex-recon-*` (evidence, на которые ссылаются гайды: `mvp-internal-learn`, `mvp-external-sources`,
  `mvp-teaching-ledger`, `mvp-experiments` v196, `core-tests`).
- **Пасс 9:** статусный слой (ledger'ы/доски/Fable-checks/open-questions, sync claude↔active); проверить, что
  Fable-overlay 2026-07-05 в шапках guide-01..05 не оставил открытых MISSING/PARTIAL по методическому слою;
  расхождение версий train-ready V192/V309 (guide-01 §7 п.8 / guide-05 §9 п.8) — определить канонический.

*Конец Pass 6/10. Прочитано full-depth: guide-01 (§2-§11: концепт, код-сейчас 3.1-3.6, рецепт 8-шаг, примеры,
гейты 6.1-6.6, канон 14/20/enum, пробелы, тест-стратегия U/NEG/BL/INT/QA/REG/UH/TA, Part A UML), guide-02
(§3.1-3.8 file:line, рецепт 12-шаг, примеры включая числовой прогон margin, гейты 6.1-6.6, Solved Block, 11
пробелов R-08/R-19/R-05/R-10), guide-03 (§2-§7: три оси, код 3.1-3.3, рецепт 7-шаг, живые числа v196, 10 гейтов,
9 пробелов), guide-04 (§2-§7: завод-аналогия, 5 уровней 3.1-3.9, карта фаз, 9 пробелов), guide-05 (§3.1-3.6 контур,
рецепт 6-шаг Ф7, 6 гейтов + Gate B, тест 9 слоёв, 9 пробелов), claude-05/01/03/06 (тонкие компаньоны целиком).
Независимо перепроверено прямым чтением боевого чекаута 6 якорей (§6.7): дрейф контента = 0, дрейф строк = только
+2-3 в DpoTrainer.cs. ★ ГЛАВНЫЙ ВЫВОД ПАССА 6: слой M1-гайдов (build/train/measure + pattern + auto-loop) полон,
file-anchored, самокорректирующийся; единый центральный дефект во всех пяти — «обучение не подключено в проде»
(empty DPO-канал + hardcoded margin + deterministic edge-authority + отсутствие генератора пар из ошибок). Все
гайды PLAN-ONLY, `PLAN_NOT_READY`.*

---

## Pass 7/10 — codex-recon-core-* (реконы модулей llm_mvp_core: что реально в коде)

**Что покрывает этот проход.** 40 файлов `codex-recon-core-*` — пофайловый рекон боевого чекаута
`C:/Projects/llm_mvp_core` (не план, а «что реально в коде»). Это фундамент-сверка: планы 07/08/09/10/11/12
и гайды описывают, что ДОЛЖНО быть; реконы фиксируют, что ЕСТЬ (реальный класс / file:line / заглушка).
Прочитаны full-depth 26 самых нагруженных для DPO-центра (00-contracts/tensor/rest, 04, 05, 06, 07, 08, 09,
10, **11**, 14-continuous, 14-ml, 15-logical/rest, 16, 17, **18**, 21, 22, **25**, **26**, **27**, 42, 43-smart,
43-localwiki, assets, tests, docs-submap, docs-decisions); остальные 14 характеризованы по заголовку+relevance
(§7.0). **Все 40 учтены.** Совпадает с Pass 6 §6.7 drift-проверкой (DpoTrainer, IncrementalTrainer-заглушка,
R-19, пустой DPO-канал, роутер-фикс-веса) — реконы file-anchored, не выдуманы. PLAN-ONLY контекст сохраняется:
это evidence для плана, не разрешение на реализацию.

### 7.0. Полный список 40 core-recon файлов (все учтены)

Слаг = `codex-recon-core-<X>.md`. ★ = full-depth в этом проходе; ○ = характеризован по заголовку/relevance.

| # | Слаг | Модуль / тема | Реальность (одна строка) | Rele­vance к DPO-графу |
|---|---|---|---|---|
| 1 | ★ `00-contracts` | 00.Core — 5 контрактов ILearningMachine | 5 уровней есть как контракты (L2/3/4 inference, L5/L1 train); teacher-полей нет | ФУНДАМЕНТ |
| 2 | ★ `00-tensor` | 00.TensorCore | чистая C#-арифметика (MatMul/Softmax/autograd); субстрат many→many, не уровень | subst­rate |
| 3 | ★ `00-rest` | 00.Core — OperationResult/BaseService/реестры | `OperationResult<T>`; BaseService/ISessionStorage живут в 21, не в 00 | ВЫСОКАЯ |
| 4 | ★ `04-trainloop` | 04.TrainingLoop | pre-train LLM (next-token CE); НЕ DPO, метрик/held-out нет | низкая (шаблон) |
| 5 | ★ `05-emb` | 05.Embeddings | feature-extractor (encoder→mean-pool→L2); OK_MODEL_COMPONENT | прямая (input-вектор) |
| 6 | ★ `06-vsearch` | 06.VectorSearch | BruteForce+HNSW KNN; НЕ IQueryable, НЕ many→many; retriever-компонент | низкая |
| 7 | ★ `07-graph` | 07.GraphEngine | детерм. граф+PageRank; `GraphEdge.Weight` init=1.0 НЕ обучаем | средняя (скелет) |
| 8 | ★ `08-graphrag` | 08.GraphRag | Local+Global(PPR)+Hybrid retriever, веса 0.6/0.4 фикс | низкая (донор паттерна) |
| 9 | ★ `09-inf` | 09.Inference | авторегресс. декодинг (Sampler без seed); НЕ trainable-уровень | низкая-косвенная |
| 10 | ★ `10-lora` | 10.LoRA | LoRA-математика РАБОТАЕТ, но `Apply` НЕ вклеивает в Forward (нет SetValue) | ВЫСОКАЯ (механизм) |
| 11 | ★ **`11-dpo`** | 11.DPO | **движок шага DPO РАБОТАЕТ**; в бою не вызывается; пар не строит | ЯДРО |
| 12 | ★ `14-continuous` | 14.ContinuousLearning | `DpoPair`+FeedbackRouter есть; `IncrementalTrainer` — ЗАГЛУШКА | ВЫСОКАЯ (дыра) |
| 13 | ★ `14-ml` | 14.MachineLearning | классич. ML «с нуля» (MLP/QLearning); изолирован, метрик нет | низкая (референс) |
| 14 | ★ `15-logical` | 15.OrchestratorModel/Logical | регистрация flow/unit, latest-wins подмена (A-LM-03) | ВЫСОКАЯ (интеграция) |
| 15 | ★ `15-rest` | 15.OrchestratorModel — RunAsync | два слоя: A (ILogical, тонкий) + B (legacy facade); мозги в 21 | высокая |
| 16 | ★ `16-autonomous` | 16.AutonomousLearning | 8-шаг self-improve контур; DPO-канал ПУСТ (`[]` на :120) | средняя (прецедент) |
| 17 | ★ `17-dynamic` | 17.DynamicDomains | детерм. граф-калькулятор (closure+bridge); Jaccard E49/E50 = baseline | средняя (прецедент) |
| 18 | ★ **`18-capability`** | 18.CapabilityTraining | **Route B: рабочий референс авто-DPO** (pair→LoRA→StepVector→ckpt→manifest→register) | ОЧЕНЬ ВЫСОКАЯ |
| 19 | ★ `21-infra` | 21.AgentInfrastructure | BaseService/fabric + реестры юнитов/flow/топологии; `ResolveLatest` tie-break | ВЫСОКАЯ (фундамент) |
| 20 | ★ `22-algos` | 22.AgentAlgorithms | KNN/Jaccard/BM25/keyword-router — детерм. хелперы/baseline | средняя (кандидаты-ретриверы) |
| 21 | ○ `24-compress` | 24.Compression | сжатие как часть lifecycle ассетов (corpus.jsonl.gz) | низкая (lifecycle-деталь) |
| 22 | ★ **`25-modelserver`** | 25.ModelServer | lifecycle-инфра ЧАСТИЧНА: `IModelServer` без реализации, нет сетевого export | ВЫСОКАЯ (публикация) |
| 23 | ★ **`26-router`** | 26.TrainableRouter | `KeywordUnitScorer`=`FORBIDDEN_RUNTIME_SHORTCUT`; DPO-поля пустые плейсхолдеры | высокая (анти-паттерн) |
| 24 | ★ **`27-translation`** | 27.TranslationDomain | **единственный зрелый full-lifecycle `ILearningMachine`**; R-19 литерал 0.03 | ЭТАЛОН |
| 25 | ○ `28-nlp` | 28.SemanticNlp | семантические NLP-юниты (Partial по submap) | средняя |
| 26 | ○ `29-sc` | 29.SemanticCompiler | детерм. pipeline (AstTokenizer→RouteClassifier→…); OperationResult SC29 | низкая (детерм.) |
| 27 | ○ `33-matrix` | 33.MatrixAlgorithms | матричные алгоритмы для «упрощённой матрицы» ILearningUnit | низкая |
| 28 | ★ `42-wiki` | 42.ProjectWikiDomain | 6 Bootstrap `ILogicalUnit` = старт DPO (A-LM-03); честный runner_missing | ВЫСОКАЯ (seed) |
| 29 | ★ `43-localwiki` | 43.LearningLocalWikiData | ДАННЫЕ/реестр/раннер; реальный DPO-цикл (loss+reward+held-out gate) | ОЧЕНЬ ВЫСОКАЯ |
| 30 | ★ `43-smartskills` | 43.SmartSkillsDomain | самый зрелый «11 юнитов под 1 машиной»; `DeterministicFinalAuthority=false` | ВЫСОКАЯ (прототип) |
| 31 | ★ `assets` | models/training-data/artifacts | реальные ckpt/manifest/index.json/corpus.jsonl.gz; teacher-провенанса нет | ВЫСОКАЯ (lifecycle) |
| 32 | ★ `tests` | Tests/ | шаблоны честности untrained + приёмки trained (held-out/ablation/lifecycle) | ОЧЕНЬ ВЫСОКАЯ |
| 33 | ○ `docs-arch` | docs/ARCHITECTURE.md | §9 иерархия ILearningMachine + диаграмма компонентов | справка |
| 34 | ★ `docs-decisions` | docs/DECISIONS-LOCKED.md | D-01..D-17 + A-LM-01..A-LM-13 (жёсткие границы графа) | ВЫСОКАЯ (locked) |
| 35 | ○ `docs-exp` | docs/EXPERIMENT-RESULTS-MAP | E01-E50: метрики, SCALE_UP, DPO/flow evidence | справка |
| 36 | ○ `docs-interfaces` | docs/COMPONENT-INTERFACES.md | справочник интерфейсов, особ. Bootstrap-юниты 42 | справка |
| 37 | ○ `docs-plans` | docs/plans/ | индекс планов (в т.ч. `claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1`, evidence-only) | справка |
| 38 | ○ `docs-skills-catalog` | docs/skills (каталог) | каталог скилов для графа/DPO | справка |
| 39 | ★ `docs-skills-dpo` | docs/skills (DPO/trainable/graph) | `trainable-architecture-decision-SKILL` + правила уровней | правила классиф. |
| 40 | ★ `docs-submap` | docs/SUBPROJECT-MAP.md | статусы Done/stub всех сабпроектов (сверено в коде) | ВЫСОКАЯ (карта статусов) |

### 7.1. ЯДРО — 11.DPO: движок шага РАБОТАЕТ, но пар не строит и в бою не вызван

`11.DPO` — низкоуровневая **тренировочная инфраструктура DPO**, «движок одного шага», НЕ продуктовая модель,
НЕ `ILearningMachine`, НЕ построитель пар. Что реально в коде:
- `DpoPair{Prompt;Chosen;Rejected}` — 3 строки, **без** source/metadata/near-miss/domain/teacher-полей.
- `DpoDataset.Load(path)` — JSONL по строке, `Shuffle(seed)` Fisher-Yates (исходник не мутируется).
- `DpoTrainer.Step` (`:31-70`) РАБОТАЕТ по-настоящему: `LogProbs` (`:227-261`, log-softmax по next-token с
  маской, отдельно policy vs frozen reference); `chosenReward=Beta*(polChosen−refChosen)`, `Beta=0.1f`;
  `margin=chosenReward−rejectedReward`; `loss=−log(σ(margin)+1e-8)`; **ручной backward** `gradMargin=σ(margin)−1`
  (chosen `+`, rejected `−`, только в policy); `AdamW.Step(policy.Parameters(), lr=1e-5)`; reference НЕ трогается.
- `DpoMetrics{Step,Loss,ChosenReward,RejectedReward,RewardMargin,Accuracy=(margin>0?1:0)}`; `SaveMetrics` —
  атомарный JSON `dpo_step_NNNNNN.json` (`.tmp`+`File.Move`, DNA rule 8).
- Варианты: `StepMany` (усреднение по батчам, grad-scale 1/N, 1 optimizer step), **`EvaluateMany`** (те же метрики
  БЕЗ backward/update — held-out-оценщик движка), **`StepVector`** (`DpoVectorBatch{Batch,TeachingVector,Weight}`,
  `VectorSignal=clamp(1+active_slots/60,1..2)`).

**Доказано тестами** (`DpoTrainerTests`): формула, positive/non-NaN loss, бинарная accuracy, `margin=chosen−rejected`,
атомарная запись, и что при копии reference→policy reward margin **растёт за 40 шагов** (`trained>initial+1e-5`,
initial=0). **Пробелы (для графа):** (1) нет построения пары (в 11 нет логики «из узла/ребра → chosen/rejected»);
(2) нет provenance/teacher-полей в `DpoPair`; (3) нет checkpoint/manifest/locator/export-import/fresh-load/
continue-training/held-out-качества; (4) **прямого вызова `DpoTrainer`/`new DpoBatch` из боевого рантайма
(14/15/16/25/42/43) НЕ найдено — только тесты** (боевого пайплайна обучения через 11.DPO ещё нет).
Классификация: `OK_INTERNAL_DETAIL` — честная training-infrastructure, не скрытый shortcut (реальный градиент есть).

### 7.2. ROUTE B — 18.CapabilityTraining: рабочий референс авто-DPO (то, что план должен переиспользовать)

**ОЧЕНЬ ВЫСОКАЯ релевантность** — это фактически готовый пайплайн авто-DPO «наблюдённое ребро знаний → DPO-обучение
крошечной модели → регистрация probationary-юнита + retry-flow». 7 `.cs`; зависит от 10.LoRA+11.DPO (НЕ от 15/25).

Пайплайн `DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync` (1019 стр.):
1. валидация + idle-gate D-14 (иначе «Teaching deferred»);
2. по каждой ветке: `ExtractSubPhaseId` → `TrainingBranchPlan` с A-стороной (`BuildTrainingTextCore`: compiled
   prompt + branch result + sub-phase + предикаты + **60-мерный teaching-вектор**), B-chosen
   (`BuildChosenCapabilityText`) и B-rejected (`"Do not generate a capability…"`);
3. один батч на same-domain ряд: `LogicalTeachingDatasetWeighter` (D-09 rare-веса) + `AutonomousLearningReplayMixer`
   → `ExecuteDpoTeachingAsync` → **`LogicalDpoTrainingUnit.ExecuteAsync`** (`LlmIngredientLogicalUnits.cs:308-415`):
   `LoraApplicator.Apply(rank=2/alpha=4)` (`:339`) → при наличии грузит родительский checkpoint (continue-training)
   → токенизация в `DpoBatch` → цикл `DpoTrainer.StepVector` до `MaxStepCount`(=5) с ранней остановкой
   `ConvergenceMonitor`(порог 0.002);
4. регистрация: обучить `DomainClassifierUnit` (persist json) → на каждый sub-phase сохранить tokenizer.json+policy.bin,
   обернуть в `TransformerEmbeddingKnnLogicalUnit.Train` (embedding+HNSW), сохранить `generated_capability_manifest.json`,
   `Units.Register(trust=Probationary, isLearned:true)`, `Flows.RegisterFactory`.

**Инференс (retry):** `GeneratedCapabilityFlow` → `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync`: embed →
HNSW → fusion (vector 0.45 + BM25 0.35 + pattern 0.20) → `confidence=0.50+fused*0.49`.
**Метрики:** `LogicalDpoTrainingResult`(PreferenceCount, DpoStepCount, LoraAdapterCount, LastLoss, LastAccuracy,
TrainingTimeMs, MetricFilePaths). **Checkpoint-root по умолчанию ВНЕ репо** (`Path.GetTempPath()/llm-mvp-capability-training`).
**Пробелы:** нет held-out accuracy/reward-margin как отдельной evidence (только LastLoss/LastAccuracy на train);
export/import/share + публикация через 25.ModelServer/locator НЕ реализованы (`ServerBaseUrl/ServerUnitEndpoint` пусты);
нет teacher-провенанс-полей в манифесте; риск — fusion-re-ranker `confidence=0.50+fused*0.49` даёт мягкий пол 0.50
даже при нулевом сигнале (нужен ablation). **DynamicGraph как уровень отсутствует** — модель линейная (sub-phase+flow),
без many-to-many/fan-in/fan-out/multi-output edge-контрактов.

### 7.3. 04.TrainingLoop + 10.LoRA — низ стека обучения, LoRA-wiring разрыв

- **04.TrainingLoop** — pre-train LLM (next-token CE): `Trainer.TrainAsync` (grad-accum → Forward → CE-loss →
  ручной backward → `ClipGradNorm` → `CosineWithWarmup` → `AdamW.Step` → `SaveCheckpoint`). Grep по `DPO|ILearning*`
  = **0**. Метрика — только `loss`/`lr` в Console, файлов нет; нет resume/manifest/held-out. Для графа — только
  **референс шаблона шага** (steps→loss→clip→optimizer→ckpt), не источник DPO-логики.
- **10.LoRA** — parameter-efficient FT поверх TensorCore. `LoraLinear.Forward = base(x)+scaling*((dropout(x)@Aᵀ)@Bᵀ)`
  (A=Kaiming, B=нули → начальная поправка 0; base заморожен). **КРИТИЧНЫЙ разрыв (подтверждён кодом + 10-lora.md
  §54-63):** `LoraApplicator.Apply` замораживает параметры, через reflection находит `q_proj/v_proj`, создаёт
  `LoraLinear` и **возвращает их «сбоку» — НЕ делает `SetValue`, НЕ встраивает в `LlmModel.Forward`, НЕ сохраняет**.
  Их обязан подцепить вызывающий training-loop. Единственный реальный потребитель — `LogicalDpoTrainingUnit`
  (`:339`). Классификация: `OK_INTERNAL_DETAIL`/`OK_MODEL_COMPONENT`, честно (док прямо запрещает выдавать за
  ILearningMachine). Пробел: КТО реально применяет градиенты к `lora_a/lora_b` — делегировано `IIncrementalTrainer`
  (14), а тот — заглушка (§7.4).

### 7.4. 25.ModelServer + 14.ContinuousLearning + 16.AutonomousLearning — lifecycle частичен, каналы пусты

- **25.ModelServer** — тонкий слой публикации/локации (не тренирует, не инференсит). Контракты `IModelRepository`
  (`LoadWeightsAsync`/`UploadAsync`), `IRemoteUnitLocator`, `IModelServer`. **Реализована только Local-ветка:**
  `LocalModelRepository.UploadAsync` считает SHA-256 чекпойнта и возвращает `RemoteUnitManifest` — **реальной
  сетевой загрузки НЕТ**; `InMemoryRemoteUnitLocator` = `Dictionary`. **`IModelServer.PublishAsync(Stream weights)`/
  `OpenWeightsAsync` — БЕЗ реализации;** `serverBaseUrl` в `LoadWeightsAsync` игнорируется (import весов только из
  локального файла). Гейты ДО публикации есть (verdict `_GATE_PASSED`, `GatePassed`, `PublishAllowed`).
  `GeneratedCapabilityCheckpointManifest` (определён в 18) несёт continue-training поля (`ParentUnitId`,
  `ParentPolicyCheckpointPath`, `TrainingStepCount`) + provenance ДАТАСЕТА (`SourceDatasetSha256`, `TrainingBranch`),
  но **НЕ teacher-провенанс** (нет `teacher_access_mode/hosting_provider/model_id/model_license/closed_proprietary_model`).
  split common-vs-adaptive (R5) на уровне 25 не выражен.
- **14.ContinuousLearning** — `DpoPair{Prompt,Chosen,Rejected,Domain,CapturedAt}` + `FeedbackRouter.Route`
  (EDITED→`DpoPair(Chosen=UserCorrection,Rejected=ModelResponse)`; REJECTED→`Chosen="[no preferred response]"`).
  Но **`IncrementalTrainer.TrainAsync` (`:38-53`) — ЗАГЛУШКА**: строит путь `{domain}.lora`, `Task.CompletedTask`,
  `Ok(path)` — НИ `LoraApplicator.Apply`, НИ `DpoTrainer.Step` не зовёт (делегирует в 16). Реестр — только in-memory,
  persist/fresh-load DPO-пар нет.
- **16.AutonomousLearning** — 8-шаг ночной self-improve контур (idle-gate → baseline → сбор → взвешивание → train →
  enhanced → promote-если-дельта>0). **DPO-канал ФИЗИЧЕСКИ проложен** (`AutonomousTrainer.TrainAsync(..., IReadOnlyList<DpoPair>)`),
  но pipeline зовёт его **пустым `[]`** (`AutonomousLearningPipeline.cs:120`) — «розетка без вилки»: авто-генерации
  пар «слабое место→chosen/rejected» НЕТ. `TrustScorer` — фикс-веса 0.30/0.20/0.15/0.35/0.10 (не обучается).

Итог для lifecycle: continue-training-механика есть (parent-checkpoint), но **export/import/share + сетевая
публикация + fresh-load через 25 — не закрыты**; реального боевого DPO-прохода вне 18 нет.

### 7.5. ЭТАЛОН — 27.TranslationDomain: единственный зрелый full-lifecycle ILearningMachine

`TranslationMachine : ILearningMachine<TranslationDomain>` (`:7`) — **полный жизненный цикл на диске:** `TrainBatchAsync`/
`TrainStructuredBatchAsync`, `EvaluateAsync`, `Export/ImportCheckpoint`, `SaveCheckpointAsync` (атомарно tmp→move,
всё состояние: KnnStore+tokenizer+executors+verifier+история+курсор Tatoeba), `LoadCheckpointAsync` (единственный
валидный путь после рестарта, **без replay**), `ImportCheckpoint` (валидирует schema/machineId/полноту направлений
`EXECUTOR_DIRECTIONS_INCOMPLETE`), `GetOrchestratorAsync`→`InlineTranslationOrchestrator`→`TranslationRoutingFlow`→юниты.
DPO-строка train-ready: `id,source,direction,pair_family,prompt,a_side,chosen,rejected,validation_outcome`
(rejected несёт `[WRONG: weakness-gap-consequence]`). Метрики held-out: `RuntimeExactAccuracy`, `RuntimeProductAccuracy`
(составной), `RuntimeBootstrapRate`, BLEU-1, chrF1; контаминация `TRAIN_HELDOUT_OVERLAP` блокируется до accuracy.
promotion-гейт (`:211/389`): `Acc≥+0.02 && RewardMargin≥0.025 && ProductAcc≥0.95 && Bootstrap≤0.15 && Script==1.0 &&
Copied==0 && WrongDir==0 && Emoji==1.0 && NamedEntity==1.0`; novelty-gate `TatoebaPilotLoopRunner.cs:777`
(`MinNewPattern=1−maxAcc`, `MaxSeenToken=maxAcc`).
**ЛОВУШКА R-19:** `RewardMargin` захардкожен `0.03/0.0` (`:70/:125/:187`) — формальная метка наличия `[WRONG:`,
а НЕ вычисленная награда; для графа margin должен браться из движка `11.DPO/DpoTrainer.cs`. Другие суспекты для
ablation: `_exactSourceMap` (точный source→target lookup, conf 0.93) — проверить leakage/replay. Лицензия источника —
Tatoeba **CC-BY** (не MIT/Apache) → для common-training нужна проверка teacher/source-license gate.

### 7.6. Прототипы графа обучаемых юнитов — 43.SmartSkills / 42.Wiki / 43.LocalWiki

- **43.SmartSkillsDomain** — самый зрелый «много юнитов под одной машиной». `SkillSelectionMachine :
  ILearningMachine<SkillSelectionDomain>, IMachineTraining` (L5, полный ckpt/manifest/export/import/fresh-load/eval-gate 0.8);
  11 семантических `*SkillUnit` (L2) решают **argmax по ОБУЧЕННЫМ весам** `profile.*FeatureWeights` (при `≤0` → `NoDecision()`,
  нет authority «по умолчанию»); KNN только `knn_role=support_evidence_only`. **Детерминистического финального авторитета
  выбора скила НЕТ:** единственный счётчик `_candidatePositionWeights` (`:65`, `+1` на `:767`) явно задемпфирован
  (`RouteMode=candidate_position_scaffold_not_final_semantic_authority`) и заблокирован `CandidateOrderLeakageGuardUnit`
  (permutation current/reversed, `CanSelectSkill=false`). Каталог: у всех `Trainable:true`, `DeterministicFinalAuthority:false`,
  `SemanticQualityClaimed:false` (честный scaffold). Пробел: веса — счётчики `Dictionary<string,int>`, а не DPO-margin;
  `OrchestrationReasoningMachine`/`PromptToTaskMachine` = СКЕЛЕТЫ (`NotTrainedStatus`).
- **42.ProjectWikiDomain** — **6 Bootstrap `ILogicalUnit` = канонический старт DPO** (A-LM-03): `TaskOrchestrationUnit`
  гоняет intent→scan→evidence→select→compose→verify строго последовательно (линейный DAG 1→6, аккумулирующий fan-in,
  БЕЗ fan-out). Все — детерм. хелперы (regex/ФС-обход/keyword/if-цепочка/шаблон/булев чек), честно `Confidence:1.0`,
  untrained. Слот подмены на trained-веса из 25 (A-LM-03). Flow-адаптеры честны: `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`
  / метрики `UNKNOWN` (антимаскарад). 12 `*DpoGenerator.cs` для wiki-юнитов есть; для 6 оркестрационных — grep не нашёл.
- **43.LearningLocalWikiData** — **ДАННЫЕ/реестр/раннер** (runtime-логика в 42). Реальный DPO-цикл: `dpo-pairs.jsonl` +
  `heldout-dpo-pairs.jsonl` + `checkpoints/policy_final.bin` + метрики (loss+reward_margin+held-out), gate
  `final_accuracy≥0.90 && final_reward_margin≥0.02`. Реестр `PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json`
  (accepted=19/rejected=182/experimental=193/blocked=26; rejected-checkpoint нельзя как continuation). 13 canonical units
  (router→classifier→…→verifier) под `ProjectWikiTrainingOrchestrator`. Провенанс в строке: `source_id`,
  `source_license_state`, `source_authority_state` (`SYNTHETIC_SEED`/`ACCEPT_INTERNAL_SYNTHETIC`) — заготовка под
  legal-teacher gate. base-wiki batch-001 FAILED (final_accuracy=0, reward_margin<0 → negative/diagnostic).

### 7.7. Субстрат ЕСТЬ vs 7 NEW-типов графа ОТСУТСТВУЮТ (сквозной вывод реконов)

**Что реально ЕСТЬ (сверено по коду в ≥5 реконах):**
- 5-уровневая иерархия как **контракты** (`00.Core`: `ILogicalUnit :97`, `ILogicalFlow :166`, `ILogicalOrchestrator :202`,
  `ILearningMachine<T> :208`, `ILearningMachinePair :245`; A-LM-02 в шапке — Train* только на машине);
- реальные `ILearningMachine<T>`: `27.TranslationMachine`, `42.OrchestrationPlanMachine`, `43.SkillSelectionMachine`
  (+ адаптеры ResultGap/PromptToTask/OrchestrationReasoning);
- рабочий движок DPO-шага (11), LoRA-математика (10), auto-DPO-пайплайн (18), lifecycle-инфра публикации (25 частично);
- детерминистический граф-субстрат: `07.GraphEngine` (узлы/рёбра/PageRank), `08.GraphRag` (retriever), `17.DynamicDomains`
  (closure+bridge, mutation-юниты правят реестр топологии на лету), `LogicalTopologyRegistry`+`AdaptiveLogicalOrchestrator`
  (21, реальный движок RunAsync :85), `PhaseTransitionSignal`/`LogicalContextPacket` (зачатки fan-in/рёбер);
- реальные ассеты: `models/index.json` (5 моделей), `training-data/*/corpus.jsonl.gz`, checkpoints/manifests/runner-reports;
- тест-шаблоны обеих PASS-категорий: честность untrained (`PromptToTaskMachineSkeletonTests` — `NotTrainedStatus` +
  рефлексия «нет Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt») и приёмка trained (`SkillSelectionPhase06…` —
  train→eval-held-out→export/import→**fresh-load**→continue + leakage-ablation 5 контролей + order-invariance).

**Чего НЕТ (проектируется планом; grep = «No files found» подтверждён в 00-contracts/docs-submap/docs-decisions):**
- имён `ILearningDynamicGraph`/`ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator` в коде НЕТ — это алиасы/новый слой
  над существующей иерархией (A-LM-12 перечисляет ЗАКРЫТЫЙ список 5 паттернов; добавление DynamicGraph как нового уровня —
  потенциальный decision-reopening за Любой);
- 7 NEW-типов из мастер-плана §A.4 (`DynamicGraphDomain`, `IDynamicGraphNode`, `IDynamicGraphEdge`+`GraphEdgeKind`×5,
  `DynamicGraphTopology`, `EdgeAuthorityUnit`, `GraphDpoPair`, `TeacherProvenance`) — нет ни одного;
- edge-контракты many-to-many/fan-out/multi-output/async-graph-execution и **владелец финального авторитета НА РЕБРЕ**;
- **teacher-провенанс отсутствует ВЕЗДЕ в данных и коде** — подтверждают ВСЕ реконы (00/18/25/26/27/42/43/assets): поля
  `teacher_access_mode/hosting_provider/model_id/model_license/closed_proprietary_model` есть только в скилах/гайдах, но
  НЕ в `.cs` и НЕ в `.jsonl` (совпадает с Pass 1 §guide-01 «KEY FACT»/Pass 2). Особый риск: на диске `source_ref: sonnet-*` →
  доказать teacher-гейтом или `DATASET_REBUILD_REQUIRED`.

### 7.8. Инвентарь скрытого/детерминистического авторитета (кандидаты замены на EdgeAuthorityUnit)

| Механизм (file:line) | Что делает сейчас | Вердикт по скилу | Роль для графа |
|---|---|---|---|
| `26.KeywordUnitScorer.Score` | `text.Contains(keyword)` подсчёт + boost, рантайм-выбор юнита | **`FORBIDDEN_RUNTIME_SHORTCUT`** (детерминизм как финал под именем «Trainable») | заменить обучаемым `IUnitScorer`/EdgeAuthorityUnit |
| `26.RoutingProfileTrainer.Train` | сбор множества keyword'ов, нет весов/loss/ckpt | `REPORT_CLAIM_TOO_STRONG` (keyword-harvest, не learning) | не выдавать за learning |
| `26.RewardMargin/DpoAccuracy/Promoted` | контрактные плейсхолдеры, нигде не вычисляются | `REPORT_CLAIM_TOO_STRONG` | заполнить реальным DPO-циклом |
| `MultiSubPhaseRouterUnit.cs:283` (в 21/эксп.) | фикс-веса 0.55/0.25/0.20/0.02, `isLearned:false` | честный baseline (Pass 1 кандидат №1) | замена на trained edge-authority |
| `16.AutonomousLearningPipeline.cs:120` | DPO-канал зовётся пустым `[]` | «розетка без вилки» | подключить генератор пар из ошибок |
| `27.TranslationMachine.cs:70/125/187` | `RewardMargin` литерал 0.03 (R-19) | сломанный сигнал | margin из `DpoTrainer` |
| `14.IncrementalTrainer.cs:38-53` | `Task.CompletedTask`, весов не пишет | `FAIL Layer 5` / stub | реальная привязка LoRA+DPO |
| `21.InMemoryLogicalUnitRegistry.ResolveLatest` | tie-break «latest by RegistrationOrder» | `OK` как выбор версии; РИСК если станет финалом на ребре | authority на ребре — обученному юниту |
| `07.PageRank` / `08.Hybrid(0.6/0.4)` / `17.Jaccard(0.35)` / `22.BM25/KNN` | детерм. ранжирование/retrieval | `OK_INTERNAL_DETAIL`/`BASELINE_MUST_BE_LABELLED` | candidate-generator/eval-baseline ВНУТРИ уровня, не финал |

Правило (A-LM-09/A-LM-12/A-LM-13, `docs-decisions`): scorer/selector/router/retriever/KNN/Jaccard/BM25/dictionary/counter/
`TryDecide*` — **деталь реализации внутри разрешённого уровня, НЕ архитектурный паттерн**; финальную семантику на ребре
принимает обучаемый уровень с `DeterministicFinalAuthority=false`; детерминистика — только bootstrap-генератор кандидатов
или build-time audit (D-16). D-17: `exactPromptLeakageRate==0.0` обязателен во всех eval-гейтах.

### Что остаётся на проходы 8-9

- **Пасс 8:** реконы `codex-recon-mvp-*` (23 файла — домены обучения в `llm mvp`: internal-learn V192/V309, external-sources
  лицензии, teaching-ledger, experiments v196) + библиотеки знаний (`knowledge-library-llm-mvp-20-pass` 192 КБ /
  `-llm-mvp-core-40-pass` 748 КБ, parent-accepted) — evidence, на которые ссылаются гайды; связать реконы ядра (этот проход)
  с реконами доменов обучения.
- **Пасс 9:** статусный слой — ledger'ы/доски/Fable-checks (127 находок = 91/30/6)/open-questions/benchmark-delta; sync
  claude↔active; определить канонический train-ready V192/V309; проверить, что `qwen3-together-v1 APPROVED` overlay
  (2026-07-07) согласован во всех семьях. Учесть §7.7: 5 активных open-questions (E.0) + отсутствие teacher-полей в коде —
  это то, что план обязан закрыть до `implementation_authorization`.
- **Пасс 10:** сплошная сверка полноты (185+ файлов) + executive summary «что и в каком порядке включать в бой»:
  Route B (18) как ядро → margin из движка (R-19 fix) → LoRA-wiring (10→Forward) → teacher-провенанс-поля (везде) →
  сетевой export (25 `IModelServer`) → генератор пар из ошибок (16 `[]`) → edge-authority вместо роутера-26.

*Конец Pass 7/10. Прочитано full-depth 26 реконов ядра (00×3, 04, 05, 06, 07, 08, 09, 10, 11, 14×2, 15×2, 16, 17, 18, 21,
22, 25, 26, 27, 42, 43×2, assets, tests, docs-submap, docs-decisions), характеризованы по заголовку 14 (24/28/29/33 +
docs-arch/exp/interfaces/plans/skills-catalog + skills-dpo частично). **Все 40 `codex-recon-core-*` учтены (§7.0).**
★ ГЛАВНЫЙ ВЫВОД ПАССА 7: субстрат обучения РЕАЛЕН и file-anchored — движок DPO-шага (11.DPO) работает, есть рабочий
референс авто-DPO (18.CapabilityTraining, Route B), зрелый full-lifecycle эталон (27.TranslationDomain), 5-уровневая
иерархия как контракты, детерминистический граф-субстрат (07/08/17/21) и честные тест-шаблоны обеих PASS-категорий.
НО: (1) обучение НЕ подключено в бою (DpoTrainer из рантайма не зовётся; DPO-канал 16 пуст `[]`; IncrementalTrainer 14
заглушка; LoRA-Apply не вклеена в Forward); (2) reward margin захардкожен (R-19 `0.03`); (3) **teacher-провенанс
отсутствует ВЕЗДЕ в данных и коде** (только в правилах); (4) **7 NEW-типов графа и имена `ILearning*Graph` в коде
ОТСУТСТВУЮТ** — проектируются планом поверх существующей иерархии; (5) lifecycle частичен (25 `IModelServer` без
реализации, нет сетевого export/import). Всё PLAN-ONLY, `PLAN_NOT_READY` — реконы это evidence, не разрешение.*


## Pass 8/10 — codex-recon-mvp-* + knowledge-libraries (реконы папок mvp + база знаний)

**Что покрывает этот проход.** 23 файла `codex-recon-mvp-*` (пофайловый рекон 23 sweep-папок
корпуса `C:/Users/lubao/Documents/xiigen/llm mvp/` — методология/данные обучения, НЕ код) + 2
`codex-knowledge-library-*` (индекс-библиотеки поверх реконов). Если Pass 7 фиксировал «что реально
в КОДЕ» (`llm_mvp_core`), то этот проход фиксирует «что реально в МЕТОДОЛОГИИ и ДАННЫХ» вокруг кода:
как строят DPO-пары, какие форматы пар, какие лицензии, где реальные обученные корпуса и числа.
Full-depth прочитаны 8 самых нагруженных для training-центра (`lang-translate`, `local-wiki`,
`external-sources`, `train-ready`, `small-model`, `learning-sets`, `obuchenie`, `internal-learn` +
`internal-learn-2`); остальные 15 прочитаны и характеризованы (§8.0). **Все 23 recon + 2 library
учтены (25 файлов).** Ключевой сквозной факт совпадает с Pass 7: `Accuracy = доля(margin>0)`
подтверждена ЖИВЫМ кодом `11.DPO/DpoTrainer.cs:64,106,120` (train-ready + experiments) — реконы
file-anchored, не выдуманы. PLAN-ONLY: это evidence для плана, не разрешение на реализацию.

### 8.0. Полный список 25 файлов (все учтены)

Слаг = `codex-recon-mvp-<X>.md`. ★ = full-depth в этом проходе; ○ = характеризован по содержимому/relevance.
Каждый recon = рекон одной sweep-папки `llm mvp` под углом DPO-плана ILearningDynamicGraph.

| # | Слаг | Sweep-папка `llm mvp` | Реальность (одна строка) | Rele­vance к DPO-центру |
|---|---|---|---|---|
| 1 | ★ `lang-translate` | `language translate learning` | 2 плоскости: 4 LoRA-переводчика (RU_EN/EN_RU/HE_EN/EN_HE) + `TranslationRoutingFlow:ILogicalFlow` (R1→R4); on-disk translator-JSONL `source/chosen_translation/rejected_translation/defect_type`, `SYNTHETIC_SEED`, без `[WRONG:]` | ВЫСОКАЯ (эталон DPO/LoRA) |
| 2 | ★ `local-wiki` | `local wiki learning` | Протокол 3 сессий: документ+сессия → per-node пары A→B по 9-узловому UML-графу оркестрации; анти-leakage правила (B=полный вывод, ledger≠training, f(x)=rephrase); в области сырой лог Codex/GPT-5 (closed teacher) | ВЫСОКАЯ (прообраз графа) |
| 3 | ★ `external-sources` | `external sources learning 2` | Легальный гейт: реестр 14 семейств, MIT/Apache-2.0 preferred, BLOCK CC-BY-SA/AGPL/GPL + model-output (ShareGPT/Alpaca/Dolly/FLAN); анти-distillation `external_role=a_side_style_only` | ОЧЕНЬ ВЫСОКАЯ (legal-ядро) |
| 4 | ★ `train-ready` | `train-ready-v2` (+глубокий корпус) | DPO-JSONL 12–15 полей (`a_side/chosen/rejected/target_behavior`); двусмысленность каталога (supplement 12 пар vs глубокий V246/672); Accuracy=margin>0 подтверждён по коду | ВЫСОКАЯ (формат+числа) |
| 5 | ★ `small-model` | `Small model racional` | Рациональ «малой модели достаточно» (6 слоёв, батчи 50, held-out); KNN accuracy 1.000 на held-out = меморизация (TR-HO-17..32 контаминация, запрещено) | ВЫСОКАЯ (прецедент раскладки) |
| 6 | ★ `learning-sets` | `learning-sets` | Supervised-пары `Runtime A/Expected B/Alternate/Source evidence` для 9 узлов; B узла N = A узла N+1; негативы через failure/feedback/arbiter-flag (нет явных rejected-веток) | ВЫСОКАЯ (I/O-контракты узлов) |
| 7 | ★ `obuchenie` | `обучение языку` | `build_dataset.py` флэттенит 38 батчей → `game-pairs.json` (1849 пар: 1000 RU→EN + 849 EN→RU, 7 типов дефектов); схема `{id,dir,src,ok,bad,type,why}`; детерм. фронтенд-игра | ВЫСОКАЯ (данные/ETL) |
| 8 | ★ `internal-learn` | `intelrnal learn` | XIIGen-first DPO-конвейер: исполняемый `prepare_internal_learning_sets.py` (1099 стр., инвентарь→классиф→авто-извлечение пар→tier/eval/repair); гайды v8/v15/v6/v11/v2; цель = OrchestrationRoutingFlow/AdaptiveLogicalOrchestrator | ВЫСОКАЯ (авто-DPO метод) |
| 9 | ★ `internal-learn-2` | `internal learnin 2` | Model-led DPO-корпус: канон `train-ready-v2/*trainable.jsonl` (V309: 1404 стр., avg acc 0.838, margin 0.0289); поля `a_side/chosen/rejected/[WRONG:]/target_behavior/domain_focus`; учит Layer-0 prompt-compiler→PredicateIds+Confidence | ОЧЕНЬ ВЫСОКАЯ (прогнанный корпус) |
| 10 | ★ `orch-impl` | `orchestrator implementation` | Дизайн `AdaptiveLogicalOrchestrator`: домен→компилятор→MultiSubPhaseRouter→E49(мосты)→E50(closure)→ветви→PhaseTransitionSignal→агрегация; 60-слот teaching-вектор→DPO/LoRA→Probationary; решающие механизмы детерминированы | ВЫСОКАЯ (движок-предшественник) |
| 11 | ○ `orch-teaching` | `orchestrator teaching plan` | «Одна пара за раз»: LoRA(Rank2/4)+`DpoTrainer.Step`, кумулятивное обучение + continue-training из parent-чекпойнта, trust-машина (Probationary/Trusted/Rejected) через `ResolveLatest`; 5 обучаемых юнитов; источник=XIIGen-сессии | ВЫСОКАЯ (lifecycle-прецедент) |
| 12 | ○ `orch-exp` | `orchestrator experiment` | E53 «Dynamic Flow Validation»: 9-узловой конвейер node-00..09; 12 стабильных метрик (lost_edge=0, dependency_violation=0, live_external_calls=0); тесты 12/12 и 997/997 без дрейфа; numeric learned-метрик НЕТ | ВЫСОКАЯ (эталон цели+измерение) |
| 13 | ○ `orch-ext` | `orchestrator extension` | Референс-граф обучаемых юнитов: `IOrchestrator→ILogicalFlow→ILogicalUnit`; `ProjectWikiQueryPlanningFlow` (chosen=план, rejected=single lookup+[WRONG:]); 11 wiki-юнитов; Source Authority Gate (ACCEPT/QUARANTINE/BLOCK) | ОЧЕНЬ ВЫСОКАЯ (референс-реализация) |
| 14 | ○ `dyn-flow` | `llm dynamic flow` | Концепт-предшественник графа: самомодифицирующаяся сеть юнитов поверх редактируемого графа-данных; узлы=роли, рёбра=маршруты (fan-out/fan-in/петли); D-13 locked-авторитет (арбитр решает, KNN/priority=guidance) | ВЫСОКАЯ (концепт-предок) |
| 15 | ○ `plan-set` | `plan the learning set` | `EXAMINATION-METHOD-FINAL` (метод «документ→per-node сеты»): пара A→B (точный вход/выход, не описание), rephrasing ≥2×, pass-кейс на Node 8, array-узлы=пара на элемент; 7 стадий внешних источников + license-gate | ВЫСОКАЯ (метод наполнения) |
| 16 | ○ `doc-learning` | `documentation learning` | 2 DPO-цели: контент-документация (D1→D4) и routing юнитов (`RoutingDpoPair`); рантайм `CodeDocumentationRoutingFlow:ILogicalFlow` в **26.TrainableRouter**; партии 50+50 held-out, обязательный `[WRONG:]`, source-license reject на Node 01 | ВЫСОКАЯ (2-цель DPO, живой код) |
| 17 | ○ `skills-loading` | `skills loading as an ai extension` | Двухпроходный skill-loading: детерм. retriever (BM25/registry) даёт кандидатов → `SkillSelectionMachine:ILearningMachine` решает; проход 2 ResultGapInspector→DPO-эмиссия; точка интеграции passthrough `OrchestrationPlanMachine.cs:65` | ВЫСОКАЯ (паттерн «helper готовит, юнит решает») |
| 18 | ○ `teaching-ledger` | `teaching-ledger` | Выход teaching-раннера: 3 оси измерения (точность=verdict KNOWLEDGE_EDGE→PASS; скорость=elapsed/cpu/ram/disk/checkpoint_bytes; качество=`dpo_step_*.json` Loss/RewardMargin/Accuracy); везде только Step 1 (нет траектории) | ВЫСОКАЯ (шаблон измерения) |
| 19 | ○ `novyy-domen` | `новый домейн` | Транскрипт: как заводится домен «с нуля» = копия весов base-модели + fine-tune на доменном DPO-шарде (НЕ retrain-from-zero); export/import/upgrade/rollback через `25.ModelServer`+`ILearningMachineRepository`; honest scaffold | ВЫСОКАЯ (рецепт lifecycle узла) |
| 20 | ○ `experiments` | `experiments-v2` (пуста!) | Целевая xiigen-папка ПУСТА; реальный корпус метрик в `llm_mvp_core/experiments-v2/INTERNAL-TRAIN-READY-V191` (Program.cs+DPO.dll); V196: 3916 пар, avg acc 0.6228, margin 0.01217; Accuracy=margin>0 | ВЫСОКАЯ (числа+раннер) |
| 21 | ○ `all-skills` | `all-skills` | >100 translated-скилов; DPO как «эксперимент-пара» (input/baseline=rejected/enhanced=chosen/delta); 6 типов learning-signal; реальное обучение помечено FUTURE/38.ContinuousLearning; анти-masquerade гейт измерения | средняя (формат+гейты) |
| 22 | ○ `extensions` | `extensions` | Граница host↔ядро: 2 транспорта к `42.ProjectWikiDomain` — in-proc `IWikiOrchestrator` (`WikiOrchestratorFactory.Create`) для VS + бинарь `wiki-context-hook.exe` (query/`--map`/`--ensure-ready`) для Claude/Cursor/Codex/VSCode; ядро детерм. scaffold | средняя (API-шов расширений) |
| 23 | ○ `first-ext` | `first llm extensions` | Ранний Token-Optimizer-плагин: 5 детерм. хуков-exe (−50-60% токенов БЕЗ модели, `40.HookRunners`); ни DPO/обучения/графа; единственный уровень-объект = `MultiSubPhaseRouterUnit` (L2-оболочка с детерм. нутром); анти-пример «детерм.≠обучаемое» | низкая (контекст границ) |
| — | ★ `knowledge-library-llm-mvp` | (индекс) | Индекс-библиотека по 23 recon корпуса `llm mvp`: §1 резюме + §2 10 тем-таблиц + §3 top-10 + §4 11 сводных пробелов (319 стр.) | НАВИГАЦИЯ |
| — | ★ `knowledge-library-llm-mvp-core` | (индекс) | Индекс-библиотека по 40 recon корпуса `llm_mvp_core` (это Pass 7): §1 резюме + §2 10 тем + §3 приоритет + §4 сводные пробелы BLOCKING/HIGH/MED/LOW (226 стр.) | НАВИГАЦИЯ |

### 8.1. Две knowledge-library: что содержат, сколько проходов/записей

Обе библиотеки — это **индекс-библиотекари** (навигаторы по рекон-отчётам), а НЕ пофайловые
`>=20/40-pass` reading-ledger'ы. Каждая строка таблиц несёт единый provenance-схема-набор колонок:
`source_category=source_inventory`, `provenance_status=teacher_provenance_missing`,
`teacher_access_mode=UNKNOWN`, `legal_verdict=evidence_only_not_train_ready`,
`implementation_use=planning_reference`, `gap_owner=legal_gate_plan`. Т.е. честный вердикт по ВСЕМ
входам обеих библиотек: **evidence-only, не train-ready** — ничего не объявлено готовым к обучению.

- **`codex-knowledge-library-llm-mvp-core.md`** (226 стр.) — индекс по **40** core-реконам (= материал
  Pass 7). §1: главный факт «имён `ILearningUnit/Flow/Orchestrator/DynamicGraph` в коде НЕТ; есть только
  5-уровневая `ILearningMachine`-иерархия; A-LM-12 = закрытый набор уровней». Прообразы обучаемых узлов:
  27.TranslationDomain (зрелый), 43.SmartSkillsDomain (11 юнитов), 42 (6 Bootstrap), 18.CapabilityTraining
  (авто-DPO). §2: 10 тем-таблиц. §3: приоритетный short-list. §4: сводные пробелы по риску
  (4.1 BLOCKING архитектура; 4.2 BLOCKING teacher/provenance; 4.3 HIGH скрытый детерминизм; 4.4 HIGH
  незавершённый lifecycle; 4.5 MEDIUM недочитанный код; 4.6 LOW правила/воспроизводимость).
  **§4.7 явно фиксирует: `codex-recon-mvp-*` ВНЕ этого индекса** (отдельный проход) — т.е. две библиотеки
  НЕ дублируют друг друга, а покрывают разные корпуса.
- **`codex-knowledge-library-llm-mvp.md`** (319 стр.) — индекс по **23** mvp-реконам (= материал ЭТОГО
  прохода). §1: 5 вещей, что корпус даёт плану (готовый прообраз 9-узлового графа; рабочий авто-DPO
  ETL; понимание Accuracy=margin>0; легальный teacher-гейт; прецеденты малой модели/нового домена) +
  2 сквозных красных флага (teacher-провенанс не заполнен почти нигде; graph-контракты по Graph-Style
  Gate отсутствуют везде). §2: 10 тем-таблиц (A контракты … J ассеты). §3: приоритетный top-10 файлов
  (top-1 = `local-wiki`, top-2 = internal-learn(+2), top-3 = orch-exp). §4: **11 сводных across-corpus
  пробелов** (legal teacher gate не заполнен; graph-контрактов нет; lifecycle не доказан end-to-end;
  метрика подменена live margin; анти-leakage не проверен на рёбрах; формат пар не унифицирован;
  расхождения версий/код↔доки; целевой нейминг не сопоставлен; common vs user-adaptive leg не закреплён;
  тяжёлые zip не открыты; несколько файлов-источников не найдены).

**Число «проходов/записей»:** библиотеки НЕ ведут счётчик проходов чтения — они индексируют
рекон-*отчёты* (40 и 23 соответственно). Для plan-family-гейта `N-pass library is a required built
artifact` это важно: построенный артефакт = индекс реконов, а не журнал `>=20/40` независимых
проходов над корпусом. Пробел зафиксировать как честный (см. §8.6).

### 8.2. Что реконы говорят о папках, уже покрытых RAG-01..04 (согласие/конфликт)

Ключевой мета-факт (из library §1): корпус `llm mvp` — это **методология и данные**, а боевой код — в
`C:/Projects/llm_mvp_core` (= Pass 7 / RAG-05 core). Т.е. mvp-реконы дают «метод/данные ПОЗАДИ» тех же
доменов, а Pass 7 и RAG-02/03 давали «код». Совпадения (agreement) сильные, конфликты — только
версионные/сталые ссылки.

- **RAG-03 (language_translate learning).** Реконы `lang-translate`+`obuchenie`+`small-model` реконят ровно
  этот домен. **СОГЛАСИЕ:** 4 LoRA-адаптера по направлениям, `TranslationRoutingFlow:ILogicalFlow` R1→R4,
  батчи 50 train+50 held-out, метрики reward-margin/BLEU/chrF, «DPO учит границы предпочтений, не язык с
  нуля» — совпадает с RAG-03 (перевод = самый зрелый обучаемый домен) и с Pass 7 §эталон 27.TranslationDomain.
  **ДОБАВЛЯЮТ:** on-disk формат translator-пар (`source/chosen_translation/rejected_translation/defect_type`,
  `SYNTHETIC_SEED`, без `[WRONG:]`) расходится со спекой T01 (registry требует `a_side/chosen/rejected`+инлайн
  `[WRONG:]`) — format-drift, привести перед переносом в legal build. `obuchenie` даёт готовые 1849 игровых
  пар; `small-model` даёт leakage-урок (KNN acc 1.000 = меморизация, TR-HO-17..32).
- **RAG-02 (orchestrator extension).** Реконы `orch-ext`+`orch-impl`+`orch-teaching`+`orch-exp` реконят ровно
  оркестратор. **СОГЛАСИЕ:** иерархия `IOrchestrator→ILogicalFlow→ILogicalUnit`, `ProjectWikiQueryPlanningFlow`,
  11 обучаемых wiki-юнитов, DPO 50-строчные батчи с обязательным `[WRONG:]`, публикация через
  `IModelRepository`/`IRemoteUnitLocator` (25.ModelServer/26.TrainableRouter), continue-training из
  parent-чекпойнта, trust-машина `ResolveLatest` — точно совпадает с RAG-02 и Pass 7 §15/§25/§26.
  **ДОБАВЛЯЮТ:** движок E49(мосты)/E50(closure)/пороги — детерминированный авторитет на рёбрах (потенц.
  `FORBIDDEN_RUNTIME_SHORTCUT`); 60-слот teaching-вектор + E51B teaching-runtime symmetry; E53 12/12+997/997
  без дрейфа.
- **RAG-01 (compare branches) / RAG-04 (design-to-code).** Прямого пересечения меньше; но `local-wiki`+
  `learning-sets`+`plan-set` дают 9-узловой UML-граф (splitter→…→formatter с reprocessing-петлёй) — тот же
  DynamicGraph-прообраз, что фигурирует в ветках `local_wiki_planning`. `extensions` даёт host↔core границу
  (`IWikiOrchestrator`/`wiki-context-hook.exe`), релевантную теме VS/VSCode-расширений (цель всей RAG-серии).

**Совпадение с Pass 7 (core, тот же RAG-05):** `Accuracy=доля(margin>0)` подтверждён по живому
`11.DPO/DpoTrainer.cs:64,106,120` (train-ready §, experiments §3) — точно как Pass 7 §7.1. Teacher-провенанс
отсутствует и в данных (этот проход), и в коде/манифестах (Pass 7 §7 сводные пробелы). `IncrementalTrainer`
заглушка, DPO-канал 16 пуст, R-19 хардкод — единый диагноз обоих корпусов. **Конфликтов НЕТ; есть версионные
дрейфы** (см. §8.3) и одна код↔доки-нестыковка (`orch-impl`: транскрипт «проект 21 = остров»,
`PromptDomainClassifier` не звался в рантайме — vs v7-доки описывают подключённым; сверять по живому коду).

### 8.3. Форматы DPO-пар (4 сосуществуют) + реальные обученные корпуса и числа

**4 несовместимых формата пар в корпусе (library §4 п.6):**
1. **Каноническая тройка** (`internal-learn-2`, `train-ready`, `experiments`):
   `a_side / chosen / rejected / [WRONG: ...] / target_behavior / domain_focus` (+`id/origin/normalized_family/
   bucket/tier/source_status/source/source_license_state`; глубокий корпус +`notes/repaired_from/repair_reason`).
2. **Плоский translator-формат** (`lang-translate`, `obuchenie`): `source/chosen_translation/
   rejected_translation/defect_type/defect_description`, `source_license_state:SYNTHETIC_SEED`, **без** `[WRONG:]`.
3. **Supervised A→B** (`learning-sets`, `plan-set`): `Runtime A / Expected B / Alternate A/B / Source evidence` —
   НЕ тройка, rejected нет явно (материал для rejected — в failure/feedback/arbiter-flag блоках).
4. **Игровой** (`obuchenie` game): `{id,dir,src,ok,bad,type,why}` (1849 пар).
   → План обязан задать ЕДИНУЮ каноническую тройку и правило конверсии failure/feedback/arbiter-flag → rejected.

**Реальные обученные корпуса на диске (числа, не выдумка):**
- `internal learnin 2/model-led-run-20260508-v1` — актуальный **model-led** прогон. RUN-STATUS: **V309**,
  1404 строки, avg accuracy **0.838**, avg reward margin **0.0289**, final accuracy 1.0; domain_focus
  orchestration/plans/skills = 501/829/74. Checkpoint V309 ~4.77 МБ.
- `train-ready-v2` глубокий корпус: pointer **V246**, `trainable_rows=672`, sha256 `B851169C…`; историческая
  база `v196-stratified` = 3916 строк. Отдельно `train-ready-v2/` (мелкая указанная папка) = supplement
  `v197-dpo-quality` (12 пар, строгое подмножество основного).
- `experiments-v2/INTERNAL-TRAIN-READY-V191` (в `llm_mvp_core`): **V196** = 3916 пар, BatchSize 16, 245 батчей,
  ContextLen 96, VocabSize 2048; AverageLoss≈0.6873, **AverageAccuracy≈0.6228**, FinalAccuracy≈0.5833,
  **AverageRewardMargin≈0.01217**; шаг 1 `{Loss 0.6837, RewardMargin 0.01934, Accuracy 0.75}`.
- baseline **RUN-416/V246** (из internal-learn): 672 строки, weighted accuracy **0.7068**, reward margin **0.016892**.

**Расхождение версий-указателей (library §4 п.7, BLOCKING для выбора canonical):**
`CURRENT-TRAIN-READY-POINTER` = V192/3884 строк **vs** другой pointer V246/672 **vs** RUN-STATUS V309/1404.
Неясен canonical train-ready для плана — решить. Плюс сталая ссылка раннера на несуществующий путь
`experiments-v2\INTERNAL-TRAIN-READY-V191\runner\...\11.DPO\DpoTrainer.cs` (реальный: `llm_mvp_core/11.DPO/
DpoTrainer.cs`; в раннере `DpoTrainer` собран в `DPO.dll`, .cs в поддереве нет).

**Авто-DPO ETL (рабочий референс метода):** `prepare_internal_learning_sets.py` (1099 стр.:
inventory→`classify_source`→`extract_*` построители пар→normalize/validate→tier1-5/eval/repair/manifest),
`augment_internal_learning_sets_tier3.py`, `build_dataset.py` (флэттенинг 38 батчей→1849 game-пар). Метод
6-шаговый (scan→eligibility→compiler-sim→pair→batch→measurement-gate); квоты TypeA≥40/TypeB≥20/TypeC≥30
+3 corruption; gate `reward_margin_mean≥0`, `loss<0.685` (≥0.693=ln2=брак), overlap-scan (`>0.60`=брак).
Провал калибровки 2026-05-06 (29/40 батчей margin<0) — из-за шаблона-отрицания «Do not generate a capability
for sub-phase X» (overlap~0.70), запрещён.

### 8.4. 9-узловой граф-прообраз + легальный гейт (ядро для DPO-центра)

**Один и тот же 9-узловой UML-конвейер оркестрации** повторяется в 7 папках (`local-wiki`, `learning-sets`,
`plan-set`, `orch-exp`/E53 node-00..09, `orch-impl`, `dyn-flow`, `doc-learning`):
`Domain Splitter(01) → Domain Translator[array](02) → Scheme Planner(03) → Domain Processor[array](04) →
Applier[array,self-loop](05) → Result Translator[array](06) → Arbiter Selector(07) → Arbiter Units(08) →
Formatter(09)`, с обратным ребром `Arbiter → Splitter` (reprocessing). Это прямой прообраз DynamicGraph:
узлы=роли-юниты, рёбра=маршруты, array-узлы(2,4,5,6)=fan-out/fan-in, arbiter=fan-in+петля. Совпадает с
Graph-Style/DynamicGraph Reconstruction Gate скила. **ЖЁСТКОЕ ТРЕБОВАНИЕ (local-wiki §, plan-set §):**
arbiter-selector ОБЯЗАН быть обучаемым `ILogicalUnit` на ребре — если станет regex/dictionary/counter, это
`FORBIDDEN_RUNTIME_SHORTCUT`. Расхождение нумерации (MAPPING: 07=Formatter/08=Arbiter-Selector/09=Arbiter-Units
vs v2/INDEX: 07=Arbiter-Selector/09=Formatter) — зафиксировать каноническую нумерацию в плане.

**Легальный teacher/provenance-гейт (`external-sources` = центральный для «dpo legal build»):**
- ПРИНИМАЮТСЯ: MIT, Apache-2.0 (семейства 9–14 строго MIT/Apache-2.0); CC BY 4.0 только с `attribution_text`
  в метаданных; RFC/IETF, MITRE CWE, NIST SP = public-domain (`external_license_checked`, attribution:false).
- ЗАПРЕЩЕНЫ: CC BY-SA (share-alike, исключены OWASP/StackOverflow/Dolly), AGPL, GPL, unknown/unlicensed;
  `verify_before_use` не может попасть в train-строки.
- Model-output-provenance risk (instruction-tuning): ShareGPT/WizardLM = BLOCK; UltraChat = REVIEW; Alpaca
  (CC-BY-NC) = BLOCK; Dolly (CC-BY-SA) = QUARANTINE; FLAN = METHOD-ONLY. Пилот берёт только OASST1/OASST2
  (Apache-2.0), все строки `model_review_required`.
- **Анти-distillation барьер:** внешний chat-корпус даёт ТОЛЬКО A-side форму (`external_role:a_side_style_only`);
  **chosen ВСЕГДА из XIIGen-контрактов/кода/одобренных стандартов**, ассистент-ответ источника НИКОГДА не
  копируется как chosen/authority. Совпадает с Legal Teacher Gate скила (Claude/OpenAI/Gemini запрещены как
  distillation-teacher).

### 8.5. Сквозной красный флаг: teacher-провенанс (BLOCKING для «dpo legal build»)

Поля Legal Teacher Gate (`teacher_access_mode / hosting_provider / model_id / model_license /
provider_terms_checked / closed_proprietary_model / teacher_output_used_for_training`) **отсутствуют почти
во ВСЕХ данных корпуса** — есть только провенанс входной подсказки (`user_request`, `SourceTier:runtime_prompt`),
но НЕ провенанс teacher-стороны (chosen). Конкретные зоны риска (при closed proprietary teacher → скил даёт
`DATASET_REBUILD_REQUIRED`):
- `local-wiki`: сырой `rollout-*.jsonl` = Codex/GPT-5 (provider openai) + `excutor session.txt` — closed
  teacher в области; проверить, откуда P-01..P-16/N-01..N-07 (человеческие промпты Любы = можно; сгенерировано
  моделью = нельзя).
- `orch-impl`/`orch-teaching`: XIIGen-сессии помечены `first_party`, НО если «architect response», используемый
  как `Chosen`/DPO-target, был выводом закрытой модели — под запрет distillation.
- `train-ready`/`internal-learn-2`: все строки `source_status=accepted_model_led_*` («model-led», не дословно
  извлечено), teacher не идентифицирован.
- `teaching-ledger`/`obuchenie`/`novyy-domen`: chosen/rejected, вероятно, синтезированы раннером
  (`result_text`=шаблон) или из собственных translator-прогонов — но это НЕ доказано, надо подтвердить по коду
  раннера, не по артефактам.
Дополнительно (leakage-риск): подозрительно ровные `Confidence` 0.85/0.86 и `learned_unit_confidence` 0.94–0.99
(`teaching-ledger`) — проверить на confidence-floor/target-label leakage/детерминизм; routing-пары
(`doc-learning`) — проверить, что выбор юнита не предсказуем из stack/version-подстроки prompt-slice.

### 8.6. Что остаётся на Pass 9-10

- **Не открыты тяжёлые/zip/бинарные артефакты** (защита от перегруза, но релевантны): `35.ExperimentsV2.zip`
  (E01-E50/progress.json) и `test hypothesys.zip` (RESEARCH-04/progressive curriculum X04) в `small-model`;
  `FLOW-01-TRAINING-DATASET-PLAN-v4.md` (252 КБ), `dynamic-flow-v2-node-topology.csv`, E51/E52/E53-схемы в
  `dyn-flow`; `01-UML-FRAME.md` (binding frame рёбер) в `learning-sets`; `REGISTRY-DECISION-LEDGER.md` (~435 КБ),
  `ACCEPTED-MANIFEST.md`, `deep-extracted/*flow01..48*` в `internal-learn-2`; тела гайдов
  examination-v15/measurement-v6/registry-v11 в `internal-learn`; per-row чистота .jsonl рядов в `external-sources`.
- **Файлы-источники не найдены в областях** (доисследовать в zip/репо): `improve algo.drawio.xml` (UML графа,
  local-wiki/learning-sets); `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` (skills-loading, лежит в
  `extensions reasearch`); `CODE-DOC-SOURCE-REGISTRY/STACK-VERSION/DPO-REJECTED-PROMPTS-v1.md` (doc-learning);
  `XIIGEN-DPO-REGISTRY-GUIDE-v1.md` (имя не найдено в internal-learn-2).
- **Библиотеки — индекс, не N-pass-журнал:** зафиксирован честный пробел — если plan-family-гейт требует
  `>=20/40-pass knowledge library` как *журнал проходов*, построенные `codex-knowledge-library-*` это
  индексы реконов, а не журналы; план должен либо принять индекс как artifact, либо добрать пассы.
- **Для Pass 9-10 (по индексу core-library и заданию parent):** сверить mvp-методологию с боевым кодом
  (`15.OrchestratorModel`, `42.ProjectWikiDomain`, `21.AgentInfrastructure/Logical`, `26.TrainableRouter`) —
  DPO-генераторы троек (`*DpoGenerator.cs`, `RoutingDpoPairBuilder`), реальная топология 6 Bootstrap-юнитов,
  движок рёбер `AdaptiveLogicalOrchestrator` (fan-out/fan-in/authority), lifecycle checkpoint→manifest→locator→
  fresh-load→continue-training; выбрать canonical train-ready версию (V192/V246/V309); закрыть 4-форматный
  дрейф пар и teacher-provenance-гейт. Всё PLAN-ONLY.

*Итог Pass 8: 23 mvp-recon + 2 library учтены (25 файлов). Корпус `llm mvp` = МЕТОДОЛОГИЯ+ДАННЫЕ обучения
(9-узловой граф-прообраз, авто-DPO ETL, легальный license-гейт, реальные корпуса V196/V246/V309 с числами),
боевой код — в `llm_mvp_core` (Pass 7). Согласие с RAG-02/03 и Pass 7 полное (перевод и оркестратор — те же
эталоны; Accuracy=margin>0 подтверждён по живому коду); конфликтов нет, есть версионные дрейфы (V192/V246/V309)
и teacher-провенанс-дыра ВЕЗДЕ. Реконы — evidence для плана, не разрешение; `PLAN_NOT_READY`.*

---

## Pass 9/10 — статус/готовность + синтез + cross-folder инвариант + открытые вопросы

**Источник этого прохода (что реально прочитано полностью/целенаправленно).** Статусный/ledger-слой папки:
`active-codex-execution-42-fable-gap-repair-additions-20260705.md` (полностью), `codex-REQUIREMENTS-COVERAGE-MATRIX.md`
(полностью), `active-codex-execution-41-current-ledger-50-pass-acceptance-index.md` (полностью — 4 наложенных
overlay), `active-codex-execution-37-pass37-claude-baseline-sync.md` (полностью — sync claude↔active + Pass39
B-inventory-указатель), `active-codex-execution-16-full-task-coverage-simulation-gate.md` (таблица B-1..B-22 +
Pass06-backlog, grep), `active-codex-execution-F0-decision-unblocker.md` (F0-триаж Q1..Q38/Q-host + Fable-v2 две
переоткрытые строки + «Last Section» + B-21-floor), `codex-BENCHMARK-DELTA-REPORT.md` (§0 метод/вывод),
`codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` (правила счёта + первый sweep), `codex-50-cycle-plan-review-supplement.md`
(Second-Pass verdict, grep), `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md` (§0-1 honesty-lock/board-canon),
`codex-skill-improvement-ideas-*` (§0-1), snapshot статус-таблицы `plan-00-partA-uml-status.md` (grep). Это
**PLAN-ONLY** пасс: ничего не исполнялось, только чтение + синтез в RAG-файл.

### 9.1) ⚠️ ГЛАВНАЯ НАХОДКА СТАТУСА: на диске ДВА живых вердикта готовности (version-drift)

Passes 1-8 этого RAG писали текущий статус как **`PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`** (по overlay 2026-07-05).
**Проход 9 нашёл более свежий overlay, который это перекрывает.** Честная сверка по датам:

| Дата | Файл-носитель | Вердикт | Статус носителя |
|---|---|---|---|
| 2026-07-05 | `-42-fable-gap-repair` (header), `-37-pass37-sync` (header), `-40/-41` нижние блоки | **PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE** | **HISTORICAL** (по собственному `historical_classification_rule` этих файлов) |
| 2026-07-07 | `-41-...-acceptance-index.md` блок **S17** | PLAN_READY_FOR_LUBA_REVIEW_EXCEPT_2_LUBA_DECISIONS | HISTORICAL (перекрыт S17-B04) |
| 2026-07-07 | `-41-...-acceptance-index.md` блок **S17-B04** (самый верхний) | **`PLAN_READY_FOR_LUBA_REVIEW` (design complete)** | **CURRENT TOP VERDICT** |

**Самый свежий канонический вердикт (S17-B04, 2026-07-07):** `plan_readiness = PLAN_READY_FOR_LUBA_REVIEW (design
complete)`; `implementation_authorization = NO until Luba says «реализуй»`. Все **plan-level design-решения**
объявлены `RESOLVED_FROM_DOCUMENTED_AUTHORITY` — т.е. авторы плана пошли в task/legal/master документы и
**извлекли** уже данные Любой ответы, а не выбирали за неё (это ровно правило из MEMORY.md: «ответ обычно уже в
документах — извлеки, не спрашивай»). Канонический S17/S17-B04 блок живёт в `-40-current-50-pass-readiness-
simulation-20260701.md`; `-41` его зеркалит.

**Честная оговорка (stale-status-carrier / dual-live-canon).** Хедеры `-42` и `-37` НЕ пересведены — они по-прежнему
печатают `PLAN_NOT_READY`, хотя `-41` их объявил историческими. По правилам самих файлов побеждает новейший
S17-B04, НО дрейф носителей реален и это **тот самый `stale-status-carrier`**, который CLAUDE.md называет
admission-blocking. **Практический вывод не меняется**: в ОБОИХ вердиктах `implementation_authorization = NO` —
кода нет, пока Люба не скажет «реализуй». Разница только в формулировке ЧТО осталось: «доремонт плана» (старый) vs
«дизайн готов, ждём approve + execution-proof» (новый, канонический).

### 9.2) Fable-аудит покрытия и как его закрыли (repair-траектория)

- **Источник вердикта:** `claude-fable-plan-check-v1.md` (Fable 5.0, max-effort, 11 последовательных проходов):
  **127 находок = 91 COVERED / 30 PARTIAL / 6 MISSING.** Это уронило прежний `PLAN_READY` до `PLAN_NOT_READY /
  FABLE_GAP_REPAIR_QUEUE` (2026-07-05). Есть и `claude-fable-plan-check-v2.md` (252 КБ) — v2-проход (нашёл две
  переоткрытые Luba-only строки + B-21 floor-binding, см. ниже).
- **Карта ремонта — `-42-fable-gap-repair-additions-20260705.md`:** 4 блокера + 10 partial + 6 governance-правок.
  - **FABLE-BLOCKER-01 (R-B 20-pass `llm mvp` docs library):** ЗАКРЫТ — `active-...-knowledge-library-llm-mvp-
    20-pass.md`, parent-verified **20/20** (20 sub-агентов Aristotle…Faraday, readback: 1 heading/pass, нет
    placeholder-id, нет conflict/mojibake-маркеров).
  - **FABLE-BLOCKER-02 (R-B 40-pass `llm_mvp_core` docs+code library):** ЗАКРЫТ — `active-...-knowledge-library-
    llm-mvp-core-40-pass.md`, parent-verified **40/40** (Sagan…Lorentz).
  - **FABLE-BLOCKER-03 (floor-downgrade без Luba):** восстановлен binding-floor; текущий итог — `Q-B21-FLOOR =
    RESOLVED_FROM_TASK_MINIMUM` (floor 20/40 = **task minimum**, МET at 20/20+40/40, остаётся binding; сузить
    можно только явной current-цитатой Любы).
  - **FABLE-BLOCKER-04 (R-G source-addressability):** все 10 именованных источников теперь `PATH_EXISTS_VERIFIED`
    (исправлены 2 неверных исторических пути: `llm mvp\Planning sessions\...` → `xiigen\Planning sessions\session-
    start-extracted`; `C:\Projects\xiigen-platform` → `-dot-net-angular`). Остаточно: navigation/library-строки.
  - **10 FABLE-PARTIAL** (все propagated/repaired по S17): order-taxonomy single-owner; `IQueryable<GraphNodeCandidate>`
    propagation (важно: `plan-02-edge` добавлен в очередь 2026-07-07 как ПРЯМОЙ потребитель node→edge-шва);
    `DomainTag`-carrier; four-role parity (добавлена строка `ILearningFlow`); mojibake-repair 7 файлов; trainable
    DPO-builder update-алгоритм + lifecycle (`TrainableGraphDpoBuilderUnit`/`GraphDpoBuilderUnitState`);
    `AllowedTeacherRegistry.canonical_license` per-family; async-scheduling semantics; threshold/routing floors;
    guide→plan traceability.
- **Итог S17 (2026-07-07):** все PLAN-FIXABLE Fable-items **CLOSED на plan-level**. Остаток до полной готовности —
  НЕ дизайн, а: (2 Luba-decision → позже сведены к RESOLVED) + **EXECUTION_PROOF** (только после «реализуй») +
  **EXECUTION_TUNING** (числовые reward-веса + пороги, задаются при реализации) + **EXTERNAL** (Qwen3/Together terms).

### 9.3) Матрица покрытия требований (R1..R21) — codex-канон

`codex-REQUIREMENTS-COVERAGE-MATRIX.md`: **requirements_total=21; covered=15; partial=6; unmapped=0.**
- **covered (15):** R2,R3,R4,R5,R6,R7,R8,R10,R11,R12,R13,R18,R19,R20,R21.
- **partial (6):** **R1** (unit/edge training — нужно impl-доказательство, не только структура), **R9** (per-doc
  authorship traceability), **R14** (legal-provenance parity должна прорасти в data-manifest/runtime, не только в
  тексте), **R15** (6 reserve-доменов вне baseline до Q41 Variant B), **R16** (common-vs-personal host — Luba-решение),
  **R17** (FLOW-38 better-output legality — Luba-решение).
- **Baseline доски:** 12 фаз / 26 подфаз / **59 подшагов** (после добавления `Ф4.3.0`; старое `58` — stale);
  reserve-строки `Ф10.3.1..Ф10.3.6` вне baseline-59 до принятия Q41 Variant B.
- **Numbering-crosswalk** (важно — две системы нумерации): master `R-01..R-20` ↔ Luba `R1..R21`. Коллизии, где
  СМЫСЛ различается: master **R-18** (recon train-ready pointer mismatch) ≠ Luba **R18** (codex-prefix ownership);
  master **R-19** (hardcoded RewardMargin) ≠ Luba **R19** (questions-stay-final); master **R-20** (branch-authority)
  = Luba **R21**. При ссылке на «R-N» всегда указывать систему.
- **Дрейф числа полей в матрице:** матрица (R14/Legal-provenance gate) всё ещё пишет `TeacherProvenance = 18 fields`,
  тогда как канон-владелец block-08 = **20 полей** (13+5+2 RR6), а `GraphDpoPair` = **14 полей** (Pass37, владелец
  block-07). Это ровно то расхождение, что Pass 2 уже зафиксировал; матрица — ещё один stale-carrier.

### 9.4) Инвентарь блокеров B-1..B-22 (текущий владелец = Pass39-таблица в `-16-...-simulation-gate.md`)

Пост-0aa5730c (2026-07-03) + Pass43-оверлей (по `-37` §Pass39-addendum и `-16` §B-status):
- **B-1, B-3..B-17, B-19, B-20, B-22:** `RESOLVED_IN_PLAN_EXECUTION_PROOF_PENDING` — спроектировано в активном плане,
  ждут execution-proof (код/тест/runtime после «реализуй»).
- **B-2 (ЕДИНСТВЕННЫЙ живой дизайн/внешний остаток):** hosted-provider / Luba hosted-use proof для Qwen3/Together
  ПЕРЕД train/common использованием. Это не «дизайн-дыра», а внешний terms/approval-гейт.
- **B-8:** `RESOLVED_SCOPE` — v1-скоуп = **ровно один активный training-пилот, `language-translate`**; остальные
  домены = navigation/reference-only, пока Люба не расширит.
- **B-18:** `HISTORICAL_OR_TRACE_ONLY` — client-local/adaptive заменён v1-common-only-политикой.
- **B-21:** `RESOLVED_FROM_TASK_MINIMUM` (floor 20/40 met, binding; см. 9.2).
- **Общая формула:** «B-1..B-22 не имеют оставшегося РЕАЛИЗУЕМОГО design/dependency-блокера под Pass39/43/44; hosted-
  provider-proof требуется только перед hosted train/common». Это НЕ `PLAN_READY`, НЕ 50-pass, НЕ impl-auth, НЕ
  provider/legal/runtime-proof.
- **Pass06-backlog (`-16` §Pass06):** держит трейс-ремонт для сгруппированных/потерянных номеров (`B-5 =
  MISSING_NUMBER_NEEDS_TRACE_REPAIR` — в attachment 912 B-5..B-20 сгруппированы без индивидуального B-5-ряда).
  Правило: `Missing blocker numbers are trace defects, not permission to drop them`.

### 9.5) F0 decision-locks (Q1..Q38/Q-host) — всё «resolved from documented authority»

`-F0-decision-unblocker.md` разбивает каждый бывший decision-blocker на 3 полосы (attachment-backed locks / policy-
принципы / impl-проверки). Итог (все извлечены из документов, НЕ выбраны за Любу):
- **Q1** L5/нет 6-го уровня (A-LM-12). **Q2** `GraphDpoPair` = unit-level DPO (`level=unit`). **Q3** sidecar-манифест
  через `LearningBatch.SourceIds`, JSONL+`schema_version`; common под `llm_mvp_core/models|training-data`, adaptive
  под `<project>\.xiigen`. **Q4** open-weight permissive allowlist; `sonnet-*` → rebuild. **Q5** closed = judge/eval-
  only, никогда teacher. **Q6/B15** v1 = **common-only** в `llm_mvp_core` (`25.ModelServer` + `IRemoteUnitLocator`);
  `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal,DynamicGraphGlobal>` — отложенный adaptive-
  blueprint. **Q7** `MultiSubPhaseRouterUnit` = baseline/candidate-generator для ablation, снят с final-authority
  после trained-cutover. **Q8** `IQueryable<GraphNodeCandidate>` {`CandidateId`,`Confidence`,`EvidenceIds[]`,
  `SemanticQualityClaimed`}, `candidate_order_invariant=true`, leakage excluded. **Q11** `ablation_threshold=0.02`.
  **Q14** детерминированный honest traversal/topological baseline для v1; обучаемость — в `EdgeAuthorityUnit`;
  Jaccard-предиктор отложен как реальный trainable-unit. **Q30** held-out `>=0.95`, delta `>=+0.02`, reward `>=0.025`.
  **Q37** better-output origin = self-`ILearningDynamicGraph` ИЛИ legal hosted open-weight teacher; closed = judge/eval.
  **Q38** generated client outputs = adaptive/quarantine по умолчанию; common только после license-clearance +
  redaction + per-client terms + Luba-approve. **Q-host** figma-to-html+css API на `xiigen-platform` (facade →
  `xiigen-flows`/FLOW-31); FigmaToCode/CanvaToCode = callers; better-output-to-DPO API на обоих.
- **Fable-v2 переоткрыл 2 «осиротевшие» Luba-only строки → потом сведены к RESOLVED (2026-07-07):**
  **Q-P09-XSRC** (что кормит `AutoDpoLoopOrchestrator._inputQueue`) = `RESOLVED_FROM_TASK_PROCESS` — **runtime-
  наблюдения = ПЕРВИЧНЫЙ feed** (owner `plan-09:318-348`; original-task line 20). **Q-P12-M4** (судьба mechanism-#4
  `OrchestrationScenarioPhasePlanner.PlanPhases`) = `RESOLVED_FROM_DOCUMENTED_AUTHORITY` — детерминированный
  **BASELINE_MUST_BE_LABELLED** (Luba 2026-06-30 master §E.0 Q-mech4; owner `plan-12:1321-1384`).
- **⚠️ Workdir-drift (важно для реализации):** implementation-workdir = **копия** `C:\Projects\DPOLegalLearning\
  llm_mvp_core`, и её `docs\DECISIONS-LOCKED.md` обрывается на D-15 и **не содержит A-LM-09/A-LM-10/A-LM-12/A-LM-13**,
  от которых зависит DPO-план. Первый безопасный slice после «реализуй» — decision-lock-sync этого workdir с
  источником `C:\Projects\llm_mvp_core`. (Плюс в source-`SESSION-START-PROMPT.md` и `no-product-decisions-SKILL.md`
  найдены conflict-маркеры — их читали как evidence, не правили.)

### 9.6) Продуктовое решение Luba (свежее) + teacher-семьи

`qwen3-together-v1` = **APPROVED_BY_LUBA_2026-07-07** как model-license-evidence (Qwen3 Apache-2.0,
`closed_proprietary_model=false`, `teacher_output_used_for_training=true`) — Люба вывела маршрут из карантина.
**НО** hosted train/common использование ещё заблокировано (`B-2`): нужен `ProviderTermsProof` (exact model-id,
Together ToS snapshot, privacy/ZDR, retention/output-ownership, `service_mode=plain`, `grounding_used=false`,
`provider_terms_checked=true`, непустой `terms_snapshot_ref`, Luba hosted-approve). До этого — `BLOCK`/`QUARANTINE`.
**Family→canonical_license map:** Qwen3=Apache-2.0 (approved); DeepSeek=MIT (то же чистое основание, но НЕ blanket-
approved); Kimi=modified-MIT (verify-before-use); GLM=per-release proof; Mistral=heterogeneous_per_release. Закрытые
(Claude/OpenAI/Gemini) — FORBIDDEN как teacher, допустимы judge/eval при `used_for_training=false`.

### 9.7) Sync claude↔active + benchmark + 50-cycle (что это за ledger-слой)

- **claude↔active sync (`-37-pass37`):** active-codex = «весь Claude baseline + валидированные codex-дельты».
  Pass37 починил owner-дрейф: `GraphDpoPair` owner = block-07 (14 полей), `TeacherProvenance` owner = block-08 §3.1
  (20 полей = 13+5+2 RR6 `service_mode`/`grounding_used`); B-16 fan-in возвращает типизированный
  `GraphFanInMergeResult` (не голый `LogicalFlowResult`), с `MergeRole`/`MergeOrderIsAuthority=false`.
- **Benchmark Delta (`codex-BENCHMARK-DELTA-REPORT.md`):** вывод — codex-канон **системно глубже** Claude-библиотеки
  почти по всем осям (Part A §0-§5 с `note for`/симуляциями/`file:line`, доска, матрица, per-block, машина легального
  вердикта, two-PASS, ablation/leakage/permutation, async-N). **Это подтвердил сам Claude** в своём (Claude-
  authored, evidence-only) `codex-50-cycle-plan-review-supplement.md`: cycle 47 «superseded by deeper plan layer»,
  Second-Pass `main_plan_quality = strong`, `implementation_ready_without_luba_approval = false`. Из Claude берут
  только точечные совместимые добавки (T2/T3/T4/T5 delta-строки получили read-back после cycles 26-30).
- **50-cycle (`codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` + `-CLAUDE-COMPARISON-LEDGER-20260624.md`):** это **plan-
  comparison циклы (Codex-owned файлы)**, НЕ product-циклы. Первый sweep = 25 counted-пакетов (cycles 01-25,
  вердикты FIX_NEEDED/FAIL/PARTIAL по authority/legal/lifecycle/edge-authority/split-proof); второй sweep 26-50
  ревьюит исправленный план; controlling current-run count = comparison-ledger (50/50). Правило счёта:
  «cycle считается только при возвращённом packet; дубликат notification-wrapper не создаёт цикл».
- **50-pass readiness (`-40/-41`):** это ОТДЕЛЬНАЯ линия — **50 accepted planning-pass sections** (Pass01-50, 50/50
  после Pass37-50 repair). Явная граница во всех overlay: «НЕ 50 циклов, НЕ product-readiness, НЕ PLAN_READY, НЕ
  B-22 closure, НЕ implementation authorization».

### 9.8) СИНТЕЗ папки #5 — одна картина

**ILearningDynamicGraph legal-DPO план = DESIGN-COMPLETE, но CODE-ABSENT.**
- **Дизайн строгий и закрытый** (по новейшему S17-B04): arbiter-authority + acceptance-independence (арбитр приёмки
  пары НЕ = self-confidence обучаемого юнита), no-hidden-determinism (A-LM-09/13: финал ребра — обученный узел,
  роутер честно `isLearned:false`/baseline), легальный teacher-гейт (20-полевой `TeacherProvenance`, closed=rebuild),
  единый 14-полевой `GraphDpoPair` (5 форматов→1 канон), triad-гейт (legal+split+provenance/authority), held-out/
  ablation/two-PASS измерение. Все plan-level решения `RESOLVED_FROM_DOCUMENTED_AUTHORITY`.
- **Кода нет** (честно, `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`): все **7 NEW-типов** (`DynamicGraphDomain`,
  `IDynamicGraphNode`, `IDynamicGraphEdge`+`GraphEdgeKind`, `DynamicGraphTopology`, `EdgeAuthorityUnit`,
  `GraphDpoPair`, `TeacherProvenance`) в коде ОТСУТСТВУЮТ; `DpoTrainer` (`11.DPO`) реально работает как движок одного
  шага, но **из боевого рантайма (15/42/43) не вызывается**; легальный teacher-гейт существует как ПРАВИЛО в
  `docs/skills/*`, но НЕ как поле данных ни в одном `.cs` / `training-data/*.jsonl` (ключевой Grep из guide-01).
- **Кратчайший путь к реальному тренеру (Route B):** (1) поднять `18.CapabilityTraining` как авто-DPO-пайплайн +
  провод `11.DPO/DpoTrainer` в боевой рантайм; (2) добавить checkpoint/manifest/locator + held-out + provenance-
  поле в данные; (3) заменить hardcoded `RewardMargin` (литерал 0.03 в `TranslationMachine.cs:70/123-126/187`) на
  margin из движка `DpoTrainer.cs`; (4) v1-пилот = `language-translate` (B-8), common-only (Q6/B15). Эталон lifecycle
  — `language-translate` (RewardMargin из движка, а не из литерала).
- **Готовность:** `PLAN_READY_FOR_LUBA_REVIEW (design complete)` / `implementation_authorization = NO until «реализуй»`.

### 9.9) Cross-folder мост + ЦЕНТРАЛЬНЫЙ ИНВАРИАНТ (почему эта папка — центр)

**ЦЕНТРАЛЬНЫЙ ИНВАРИАНТ: `11.DPO/DpoTrainer` — ОДИН общий тренер, который нужен ВСЕМ доменам. Починка ЭТОГО центра
(провод DpoTrainer в рантайм + checkpoint/lifecycle + legal-поле в данных) чинит design-conversion, translate и wiki
ОДНОВРЕМЕННО.** Что каждая папка получает от этого центра:
- **RAG-04 (design-to-code):** скомпилированный-но-ПУСТОЙ мост design-conversion — это регрессионный тест центра;
  его «Route B» (`11.DPO`+`18.CapabilityTraining`) = ровно то, что эта папка планирует включить. Как только центр
  обучается — design-conversion получает реальный trained-selector вместо заглушки.
- **RAG-03 (translate):** `language-translate` — ЭТАЛОН и v1-пилот (B-8). Легальный license-гейт ЭТОЙ папки —
  то, что обязано отбраковать синтетические переводные данные без locator/provenance (находка R-17: контаминированные
  `sonnet-*` строки → `DATASET_REBUILD_REQUIRED`). Центр даёт translate реальный `RewardMargin` из движка.
- **RAG-02 (wiki):** 12 `*DpoGenerator.cs` в `42.ProjectWikiDomain` уже строят тройки chosen/rejected+`[WRONG:]`, но
  `DpoTrainer` их не тренирует в бою. Провод центра → wiki-Bootstrap-юниты становятся обучаемыми.
- **Легальный гейт обслуживает external-sources** (лицензированные корпуса: MIT/Apache-2.0 preferred, qwen3-together
  approved) **и RAG-03** (reject synthetic-no-locator). Один legal-гейт — на весь граф.
**Вывод для плана «продолжить расширения VS/VS Code»:** расширения (документация кода + design-to-code) не станут
«обучаемыми» offline, пока этот центр (folder #5) не переведён из DESIGN в CODE. Порядок: folder #5 (тренер) →
затем translate/wiki/design-conversion получают обучение «бесплатно» через общий `11.DPO`.

### 9.10) Открытые вопросы для Luba (4 группы; дефолты предложены планом)

План (S17-B04) считает, что **plan-level design-решений не осталось** — но для запуска реализации Любе всё равно
нужно ПОДТВЕРДИТЬ извлечённые-из-документов дефолты и снять внешние/approve-гейты. Пять master §E.0-вопросов всё ещё
физически напечатаны как «открытые» в master-плане и матрице (ещё один stale-carrier), хотя overlay их закрыл.

1. **Канонический источник плана.** active-codex («весь baseline + валидированные дельты») vs claude-baseline vs
   codex-master? — **Дефолт: `active-codex-execution-*` (execution-facing canon).** claude-* = read-only baseline-
   authority для сверки; codex-master = человекочитаемый вход. *Нужно: подтвердить.*
2. **Разрешение начать реализацию.** Сейчас `implementation_authorization = NO`. — **Дефолт: НЕ начинать** до явного
   «реализуй». *Нужно: явное current-слово «реализуй» (плюс подтвердить рабочую ветку `DPOLegalLearning` и repo-set:
   llm_mvp_core / xiigen-platform-dot-net-angular / xiigen-flows-dot-net-angular / FigmaToCode-design-gen /
   CanvaToCode).*
3. **Подтвердить 5 master §E.0 «resolved-from-authority» извлечений** (план решил их за-документами, не спрашивая):
   (a) активировать ли все **6 R15-доменов сейчас** (Q41 Variant B, reserve `Ф10.3.1..6`) — **дефолт: НЕТ, v1 =
   только `language-translate`** (B-8); (b) HTML/CSS+FLOW-38 фидбек по умолчанию **`PersonalAdaptive`** — **дефолт:
   да** (common только после license-clearance+approve, Q38); (c) где живёт personal/adaptive legal-манифест —
   **дефолт: `<project>\.xiigen`** (common — `llm_mvp_core/models|training-data`, Q3); (d) канонический хост R16 —
   **дефолт: `xiigen-platform` facade → `xiigen-flows`/FLOW-31** (Q-host); (e) какие open-weight hosted-провайдеры
   разрешены как teacher-для-обучения vs eval/judge-only — **дефолт: qwen3-together approved (после terms-proof),
   остальные per-model verify** (Q4/Q5). *Нужно: одобрить извлечения или переоткрыть.*
4. **Внешние/провайдерские гейты (B-2, не дизайн):** (a) `ProviderTermsProof` для hosted Qwen3/Together ПЕРЕД
   train/common (Together ToS snapshot + retention/output-ownership + Luba hosted-approve) — **дефолт: заблокировано
   до proof**; (b) teacher-семьи сверх qwen3: DeepSeek(MIT), Kimi(modified-MIT), GLM, Mistral — **дефолт: per-model
   license+terms verify, не blanket-approve.** *Нужно: решение по каждой семье при необходимости.*

**Счёт открытых вопросов для Luba: 4 группы (внутри: 2 подтверждения канона/старта + 5 §E.0-извлечений на
подтверждение + 2 внешних provider-гейта) = 9 пунктов решения.** Все с безопасными дефолтами; ни один не блокирует
чтение/планирование — блокируют только decision-dependent продуктовое поведение и hosted train/common.

*Конец RAG-05, пасс 9/10 (статус/ledger-слой + синтез + cross-folder инвариант + открытые вопросы). Прочитано
чтением: fable-gap-repair (полн.), requirements-matrix (полн.), acceptance-index-41 (полн., 4 overlay), pass37-sync
(полн.), B-1..B-22 таблица (grep), F0-unblocker (F0-триаж+Fable-v2+Last-Section+B-21), benchmark-delta §0,
50-cycle-ledger §правила+sweep1, 50-cycle-supplement (Second-Pass verdict), first-sweep-amendments §0-1, skill-ideas
§0-1, plan-00 status (grep). ГЛАВНОЕ: новейший вердикт `PLAN_READY_FOR_LUBA_REVIEW (design complete)` /
`impl_auth=NO until «реализуй»` (S17-B04 2026-07-07) перекрывает `PLAN_NOT_READY` из passes 1-8; есть stale-status-
carrier дрейф носителей. Центральный инвариант: `11.DPO/DpoTrainer` — общий тренер; починка folder #5 включает
обучение для design-conversion/translate/wiki сразу. Пасс 10 — сплошная сверка полноты 185 файлов + executive
summary.*

---

## Pass 10/10 — completeness sweep + executive summary (folder #5/12 dpo legal build)

**Что это за проход.** Финальный проход RAG-05. Ничего нового не «разбираем» — делаем **сплошную сверку
полноты**: каждый из **185 файлов** имеет ли дом в манифесте passes 1-9, честно ли помечено что прочитано
глубоко, а что характеризовано; плюс единый **executive summary** (только его достаточно Любе, чтобы понять
роль папки в плане «продолжить расширения VS/VS Code»). PLAN-ONLY: ничего не исполнялось, не собиралось, не
тренировалось; только чтение RAG-файла + сверка с `ls`/`grep` папки. Источник сверки —
`C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build` (пересчитано `ls`+`grep` 2026-07-12).

### 10.1) Сверка счёта — 185 файлов, три семьи, ни один не потерян

Пересчёт `grep` подтвердил ровно манифест passes 1-8:

| Семья | Кол-во (grep) | Манифест | Статус |
|---|---|---|---|
| `claude-*` | **34** | 34 | ✅ совпало |
| `codex-*` (без active) | **95** | 95 | ✅ совпало |
| `active-codex-execution-*` | **55** | 55 | ✅ совпало |
| `dpo plan session codex.md` (транскрипт) | **1** | 1 | ✅ совпало |
| **ИТОГО** | **185** | **185** | ✅ **сходится** |

Пере­счёт по группам: recon-core **40** ✓, recon-mvp **23** ✓, plan-блоки (3 семьи) **46** ✓, гайды (3 семьи)
**15** ✓ — всё совпало с §F манифеста.

> **Одна микро-поправка честности (не меняет 185).** Счётная строка §F (строка «8 (§F.1)») слегка занижает:
> контрольных артефактов (plan/board/matrix) на самом деле **9** = 3 семьи × 3, а не 8. Авторитетный
> пофамильный счёт (55+34+95+1=185) верен и сходится; расхождение только в промежуточной подсумме §F.1.
> Пересобранные подсуммы: F.1=9, F.2=46, F.3=15, F.4=23, F.5=40, F.6=23, F.7=29 → **9+46+15+23+40+23+29 = 185**.

### 10.2) Полнота по группам (COMPLETE / PARTIAL + честный остаток)

Легенда глубины: **★ deep** = прочитано полностью/рабоче в проходе-владельце; **◑ char** = характеризовано
(заголовок+relevance / сверено против более глубокой копии семьи / grep-/секция-scoped); **○ heavy-char** =
тяжёлый артефакт, прочитан только на уровне секции/overlay/grep, НЕ построчно (честно назван ниже).

| Группа (файлов) | Проход-владелец | Глубина | Вердикт полноты |
|---|---|---|---|
| **F.1 контрольные (9)**: master/board/matrix ×3 семьи | 1, 9 | codex-master ★, codex-matrix ★; active/claude master 258/235 КБ ○ (через блоки/сверку), board 261/181/154 КБ ◑, active-matrix 166 КБ ◑ | **COMPLETE** (по-паттерну; тяжёлые тела — через блоки/deeper-copy) |
| **F.2 plan-блоки (46)** = 16 active + 15 claude + 15 codex | 1-5 | active-plan-00..15 **★ все 16 deep**; codex-plan полные 00/01/02/03/05/09 ◑ + стабы 04/06/07/08/10..14 ★ (2-5 КБ, малы); claude-plan-00..14 ◑ (baseline-сверка) | **COMPLETE** — канон-current (active) прочитан глубоко; codex/claude = сверка |
| **F.3 гайды (15)** = 5 active + 5 claude + 5 codex | 1, 6 | codex-guide-01..05 **★ deep**; guide-01 ещё и Pass 1 ★; active-guide/claude-guide ◑ (супersets/дубли того же контента) | **COMPLETE** |
| **F.4 библиотеки/контекст (23)** | 1, 6, 8, 9 | codex-knowledge-library ×2 **★ deep** (226+319 стр.); claude-01/03/05/06 ★, claude-00 ★; active-00/04 ★ (overlay); active-02 161 КБ / active-07 61 КБ ◑; **40-pass (748 КБ) ○**, **20-pass (192 КБ) ○** (parent-verified 40/40, 20/20 — не построчно); **open-questions 193 КБ ○** (через F0-синтез); fable-v1 183 КБ ◑ (127 находок), **fable-v2 252 КБ ○** | **COMPLETE** — навигация/статус извлечены; heavy-тела честно ○ |
| **F.5 recon-core (40)** | 7 | **26 ★ deep** (00/04/05/06/07/08/09/10/**11**/14×2/15×2/16/17/**18**/21/22/**25**/**26**/**27**/42/43×2/assets/tests/docs-submap/docs-decisions) + **14 ◑** (по заголовку+relevance) | **COMPLETE** (все 40 учтены) |
| **F.6 recon-mvp (23)** | 8 | **8-9 ★ deep** (lang-translate/local-wiki/external-sources/train-ready/small-model/learning-sets/obuchenie/internal-learn/internal-learn-2) + **~14 ◑** | **COMPLETE** (все 23 учтены) |
| **F.7 ledger/board/benchmark/skill + транскрипт (29)** = 21 active + 7 codex + 1 транскрипт | 9 | **★ deep**: -41-acceptance-index (14 КБ), -42-fable-gap (23 КБ), -37-sync, -F0-unblocker; **◑ grep/секция**: -16 simulation-gate (241 КБ, B-1..B-22), benchmark §0, 50-cycle ×2 (правила+sweep1), first-sweep §0-1, skill-ideas ×2 §0-1, ~остальные ~15 active-ledger'ов (31-35/17/18/50-pass-authority/NO-SAFE-WORK/opus-f0/original-task*/plan-readiness-repair/NO-BRANCH); **○ heavy-char**: **-40 (761 КБ)** — читан через S17-B04 в -41-зеркале; **-19 30-pass (434 КБ)** ○; **транскрипт `dpo plan session codex.md` (1.3 МБ) ○ — evidence-only, НЕ читан построчно** | **COMPLETE по учёту / PARTIAL по глубине** (heavy-тела честно ○) |

**Итог сверки: НИ ОДНА группа не пропущена.** Все 185 файлов имеют дом в манифесте; каждая группа
**COMPLETE по учёту**. Единственные честные «PARTIAL по глубине» — тяжёлые артефакты (○), перечислены поимённо
в §10.4; их СМЫСЛ извлечён (через overlay/зеркало/parent-acceptance/индекс), но построчного чтения гигантских
тел не было — это осознанная защита от перегруза, а не пропуск.

### 10.3) EXECUTIVE SUMMARY папки #5 «dpo legal build» (только это можно читать)

**Что папка даёт плану «продолжить расширения VS/VS Code».** Это **центр легального обучения всего проекта** —
185-документный план нового обучаемого уровня **`ILearningDynamicGraph`** (≡ `ILearningMachine<DynamicGraphDomain>`,
**Level 5**) поверх уже существующей 5-уровневой иерархии `llm_mvp_core`. Модель учится не одному навыку, а
**отношениям между навыками**: кто кому передаёт задачу, в каком порядке, как сливать результаты
(аналогия Любы — «завод/оркестр с перекладываемыми лентами»). Проектируются **7 NEW-типов**, которых в коде
пока НЕТ: `DynamicGraphDomain`, `IDynamicGraphNode`, `IDynamicGraphEdge` (+ канон-enum `GraphEdgeKind` ровно из
5 членов), `DynamicGraphTopology`, **`EdgeAuthorityUnit`** (обучаемый финальный решатель ребра — вход без
candidate-index/SourcePath, чтобы не было leakage), единый **`GraphDpoPair`** (14 полей, объединяет 5 форматов
пар в один канон) и **`TeacherProvenance`** (20 полей — легальный паспорт учителя). Через весь план идут три
жёстких правила: **обучение только на L5** (A-LM-02), **запрет скрытого детерминизма** (A-LM-09/13: на каждом
ребре финал принимает ОБУЧЕННЫЙ узел, детерминистический роутер — только генератор кандидатов или честно
`baseline/eval-only`), и **легальный teacher-гейт** (дистилляция — только open-weight permissive через нейтральных
провайдеров; Claude/OpenAI/Gemini как учитель — НИКОГДА, только judge, и judge не цель). Плюс авто-DPO-петля
(сердце проекта) и измерение held-out/ablation/two-PASS.

**Реальность (честно).** План **DESIGN-COMPLETE, но CODE-ABSENT**. Новейший канонический вердикт (блок S17-B04
от 2026-07-07 в `-41-acceptance-index`): **`PLAN_READY_FOR_LUBA_REVIEW (design complete)`**,
`implementation_authorization = NO until Luba скажет «реализуй»`. Все plan-level дизайн-решения объявлены
`RESOLVED_FROM_DOCUMENTED_AUTHORITY` (авторы пошли в task/legal/master документы и ИЗВЛЕКЛИ уже данные Любой
ответы, а не выбирали за неё). Честное состояние системы — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`: контракты 5
уровней работают, скрытого детерминизма «под видом trained» нет (роутер честно `isLearned:false`), но обученного
DynamicGraph (checkpoint/ablation/fresh-load) **ещё нет**. В коде: все 7 NEW-типов ОТСУТСТВУЮТ; `DpoTrainer`
(`11.DPO`) реально работает как движок одного DPO-шага, но **из боевого рантайма (15/42/43) не вызывается**;
легальный teacher-гейт существует как ПРАВИЛО в `docs/skills/*`, но НЕ как поле данных ни в одном `.cs` /
`.jsonl`.

**Кратчайший путь к реальному тренеру (Route B).** После «реализуй»: (1) поднять `18.CapabilityTraining` как
авто-DPO-пайплайн + провести `11.DPO/DpoTrainer` в боевой рантайм; (2) добавить
checkpoint/manifest/locator + held-out-оценку + provenance-поле прямо в данные; (3) заменить hardcoded
`RewardMargin` (литерал 0.03 в `TranslationMachine.cs:70/123-126/187`) на margin из движка `DpoTrainer.cs`;
(4) v1-пилот = **`language-translate`** (B-8), common-only (Q6/B15). Эталон lifecycle для всего графа —
`language-translate` (реальный `RewardMargin` из движка, а не из литерала). Первый безопасный slice после
«реализуй» — **decision-lock-sync** implementation-workdir (`C:\Projects\DPOLegalLearning\llm_mvp_core`, чей
`DECISIONS-LOCKED.md` обрывается на D-15 и НЕ содержит A-LM-09/10/12/13) с источником `C:\Projects\llm_mvp_core`.

**ЦЕНТРАЛЬНЫЙ КРОСС-ПАПОЧНЫЙ ИНВАРИАНТ (почему эта папка — центр всех 12).** **`11.DPO/DpoTrainer` — это ОДИН
общий тренер, который нужен ВСЕМ доменам.** Починка ЭТОГО центра (провод DpoTrainer в рантайм + checkpoint/
lifecycle + legal-поле в данных) чинит **design-conversion + translate + wiki ОДНОВРЕМЕННО**: RAG-04
(design-to-code) — его «Route B» = ровно то, что эта папка планирует включить, скомпилированный-но-пустой мост
design-conversion = регрессионный тест центра; RAG-03 (translate) — `language-translate` = ЭТАЛОН и v1-пилот,
центр даёт ему реальный `RewardMargin`; RAG-02 (wiki) — 12 `*DpoGenerator.cs` в `42.ProjectWikiDomain` уже строят
тройки chosen/rejected+`[WRONG:]`, но `DpoTrainer` их в бою не тренирует. **Вывод для плана «продолжить
расширения»: расширения (документация кода + design-to-code) не станут «обучаемыми» offline, пока этот центр
(folder #5) не переведён из DESIGN в CODE.** Порядок: folder #5 (тренер) → затем translate/wiki/design-conversion
получают обучение «бесплатно» через общий `11.DPO`.

**Легальный гейт + оговорка готовности.** Продуктовое решение Любы (свежее): **`qwen3-together-v1` =
APPROVED_BY_LUBA_2026-07-07** (Qwen3 Apache-2.0, `teacher_output_used_for_training=true`) — маршрут выведен из
карантина. **НО** hosted train/common использование ещё заблокировано единственным живым остатком **B-2**: нужен
`ProviderTermsProof` (Together ToS snapshot, retention/output-ownership, `service_mode=plain`,
`grounding_used=false`, Luba hosted-approve) — это внешний terms/approval-гейт, не дизайн-дыра. Family→license:
Qwen3=Apache-2.0 (approved), DeepSeek=MIT, Kimi=modified-MIT (verify), GLM/Mistral=per-release. **Оговорка
готовности (stale-status-carrier):** на диске ДВА живых вердикта — новейший `PLAN_READY_FOR_LUBA_REVIEW`
(S17-B04) перекрывает старый `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`, но хедеры `-42`/`-37` и master-план/матрица
всё ещё физически печатают старую формулировку и 5 «открытых» §E.0-вопросов; по правилам самих файлов побеждает
новейший, но дрейф носителей реален. **В ОБОИХ вердиктах `implementation_authorization = NO` — вывод не
меняется.** **Must-read файлы (вход для плана):** мастер-план `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`
(человекочит. вход); канон-current тела `active-codex-execution-plan-07-dpo-builder` / `-08-legal-teacher-gate`
/ `-09-auto-dpo-loop` / `-10-training-lifecycle`; `active-codex-execution-guide-01-how-to-build-dpo`; текущий
вердикт `active-codex-execution-41-current-ledger-50-pass-acceptance-index.md` + карта ремонта
`active-codex-execution-42-fable-gap-repair-additions-20260705.md`; самый глубокий свод —
`active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` (258 КБ).

### 10.4) Честный отчёт покрытия

- **Всего файлов в папке:** **185** (перепроверено `ls`+`grep` 2026-07-12; подкаталогов нет).
- **Прочитано full-depth / рабоче (★):** **≈ 65-70 файлов** — мастер-план (полн., 1029 стр.), все 16
  `active-codex-execution-plan-00..15` (канон-current тела), `codex-guide-01..05`, обе `codex-knowledge-library-*`
  (226+319 стр.), 26 из 40 recon-core, 8-9 из 23 recon-mvp, `codex-REQUIREMENTS-COVERAGE-MATRIX`,
  `-41-acceptance-index`, `-42-fable-gap-repair`, `-37-sync`, `-F0-unblocker`, `claude-01/03/05/06`,
  `claude-fable-plan-check-v1` (127 находок), малые codex-стабы plan-04/06/07/08/10..14.
- **Характеризовано (◑, по заголовку+relevance / сверкой против глубокой копии семьи / grep-секция):**
  **≈ 100-115 файлов** — дубль-копии plan/guide/matrix/board семей `claude-*`/`codex-*` (сверка против
  глубокой `active-*`), 14 recon-core ○ + ~14 recon-mvp ○, `active-00..07` контекст-доки (через overlay),
  ~15 active-ledger'ов статус-слоя (grep/секция), benchmark/50-cycle/skill-ideas (§0-1), `active-02` 161 КБ,
  `active-07` 61 КБ, `claude-fable-plan-check-v1` тело.
- **НЕ читано построчно (○ heavy-char, честно названо):** транскрипт **`dpo plan session codex.md` (1.3 МБ,
  evidence-only)**; `-40-current-50-pass-readiness (761 КБ)` — читан через зеркало S17-B04 в `-41`;
  `knowledge-library-40-pass (748 КБ)` и `-20-pass (192 КБ)` — через parent-acceptance 40/40, 20/20;
  `-19 30-pass (434 КБ)`; `-16 simulation-gate (241 КБ)` — grep B-1..B-22; большие тела
  `active-plan-master (258 КБ)`/`active-board (261 КБ)`/`active-matrix (166 КБ)`/`claude-master (235 КБ)`/
  `claude-open-questions (193 КБ)`/`claude-fable-v2 (252 КБ)`/`claude-board (181 КБ)` — через блоки/deeper-copy/
  синтез. Смысл каждого извлечён; гигантские тела не разворачивались (защита от перегруза, не пропуск).
- **Внешние zip/UML/drawio, флагнутые в Pass 8** (`35.ExperimentsV2.zip`, `test hypothesys.zip`,
  `01-UML-FRAME.md`, `REGISTRY-DECISION-LEDGER.md ~435 КБ`, `improve algo.drawio.xml`, `FLOW-01-...-v4.md` и т.п.)
  **лежат ВНЕ этих 185 файлов** — в исходном корпусе, на который ссылаются реконы, а не в папке «dpo legal
  build». Их неоткрытие — глубинная заметка о нижележащем корпусе, а НЕ дыра в покрытии папки #5.
- **Что НЕ запускалось (PLAN-ONLY):** ни `dotnet build`, ни `dotnet test`, ни training-runner, ни git-write, ни
  выполнение кода. Отсутствие 7 NEW-типов в коде подтверждено реконами (Pass 7, file-anchored), а не компиляцией.
- **Ни один файл не пренебрёгнут:** каждый из 185 учтён в манифесте §F и в сводной таблице §10.2.

**folder #5/12 RAG COMPLETE.**

*Конец RAG-05, пасс 10/10 (FINAL). Сплошная сверка полноты: 185 файлов, 3 семьи (claude 34 / codex 95 / active
55 / +1 транскрипт), пересчёт `ls`+`grep` совпал с манифестом; ни одна группа не пропущена (F.1=9 микро-поправка,
итог 185 сходится). ≈65-70 файлов full-depth, ≈100-115 характеризовано, heavy-тела (транскрипт 1.3 МБ + активные
ledger'ы/библиотеки 761/748/434/241/192 КБ + канон-тела 258/261/235/193 КБ) честно ○ (смысл извлечён, не
построчно). Executive summary (§10.3): `ILearningDynamicGraph` legal-DPO = DESIGN-COMPLETE / CODE-ABSENT /
`PLAN_READY_FOR_LUBA_REVIEW` / impl NO until «реализуй»; Route B = провод `18.CapabilityTraining`+`11.DPO/
DpoTrainer` + checkpoint/held-out/provenance; ЦЕНТРАЛЬНЫЙ ИНВАРИАНТ — `11.DPO` = один общий тренер (чинит
design-conversion+translate+wiki сразу); legal-гейт — qwen3-together approved, остаток B-2 (Together terms).
PLAN-ONLY: ничего не исполнялось. Готово к переходу на folder #6 (llm dynamic flow).*
