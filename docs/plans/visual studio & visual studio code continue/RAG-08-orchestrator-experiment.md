# RAG-08 — «orchestrator experiment» (оркестрационные эксперименты)

> Часть свипа из 12 папок для плана **продолжения расширений Visual Studio и VS Code**
> (офлайн локальная модель: документация кода + design-to-code).
> Папка №8/12. Роль (Luba): **оркестрационные эксперименты**. Релевантность: **ВЫСОКАЯ**.

- **Физический путь:** `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment`
- **Размер:** **513 файлов**, **94 директории** (не ~353, как в оценке — папка крупнее).
- **Состав по типам:** `md` — 320, `zip` — 112, `cs` — 29, `json` — 25, `svg` — 9,
  `png` — 7, `csv` — 4, `mmd` — 3, `pdf` — 2, `html` — 2.
- **Дата содержимого:** основная волна 2026-05-04 … 2026-05-05.
- **Статус этого файла:** RAG pass **1/10** — карта + структура + глубокий разбор верхних
  планов. Проходы 2–9 разбирают раунды/фазы поштучно; проход 10 — сплошная зачистка
  и executive summary.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R08, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Экспериментальные записи папки (май 2026) статичны и точны. Устарело то, что этот индекс говорит о «текущем repo»; авторитетная таблица — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке.
> 1. **Рекомендация «заменить веса-константы `SubPhaseRouterUnit` на выученные» — уже выполнена на живой ветке.** С 14 июля ветка `codex/dpo-implementation` получила +455 коммитов; её вершина на момент рекогносцировки 28 июля — `d9ecc8937` «learned router authority cutover» (переключение маршрутизации под-фаз с детерминированной формулы на обученный решатель): `MultiSubPhaseRouterUnit.cs` (+692 строки, +1361 тестов). Юнит `MultiSubPhaseRouterUnit`, который проход 5 зафиксировал как «след, не вывод», стал центральным. Знак равенства между ним и задуманным в плане «EdgeAuthorityUnit» без содержательного аудита не ставить (событие S4 в оверлей-файле).
> 2. **«КВАЛИФИЦИРОВАННОЕ ДА» прохода 9 — срез на ~13 июля; сегодня ответ сильнее.** После среза: LoRA-адаптеры подключены к живому ТРЕНИРОВОЧНОМУ пути (`8a8cad4a2`), появилось семейство «edge authority» из 11 исходных файлов, а 24 июля `main` впитал DPO-линию (заявка на слияние PR #39).
> 3. Рабочая копия `C:\Projects\llm_mvp_core`, в которой велись сверки, теперь стоит на `codex/dpo-implementation` (`0f221ac89`) с 486 несохранёнными записями; DPO-ветка движется ежедневно — перед любой новой сверкой сначала `git ls-remote` (спросить сервер о вершине, ничего не скачивая).

## Кросс-ссылки на соседние RAG (уже готовы в этой же целевой папке)

- **RAG-06 (llm dynamic flow)** — там впервые встретились эксперименты E51/E52/**E53**.
  RAG-06 нашёл: **E53 = доказательство детерминированной оркестрации**, «обученный
  splitter» = жёстко закодированные правила, реальных весов нет. **Эта папка держит
  ПОЛНЫЕ пакеты E53** (манифесты раундов D–K, результаты, «trained-unit» раунды) —
  и **подтверждает вывод RAG-06** (см. «Главный вывод» ниже).
- **RAG-02 (orchestrator-extension)** — сама оркестрационная модель (9-узловой flow,
  «обученные» логические юниты, проекты 15/21/22/23). **Эта папка — лаборатория** для
  тех узлов; здесь их «прото-код» (`35.ExperimentsV2/LogicalFlow`).
- **RAG-05 (dpo legal build)** — иерархия ILearningDynamicGraph / logical-unit-flow-
  orchestrator. **Здесь — фазовые эксперименты (Phase 25–70+), строящие эту иерархию**
  снизу вверх: контракт юнита → flow → оркестратор → реестры → топология → фазы/переходы.

---

## ГЛАВНЫЙ ВЫВОД pass 1 — что доказывают эти эксперименты (обучено vs детерминировано)

**Вопрос миссии:** есть ли здесь ГЕНУИННО обученный юнит, или всё детерминированное/
симулированное?

**Ответ (подтверждён чтением реального C#-кода, не только манифестов):**
**всё детерминированное.** «Trained/pair-backed» — это ярлык на адаптерах, а не машинное
обучение. Доказательства (файлы прочитаны целиком):

1. **`T34TransitionDomainSplitterUnit.cs`** (раунд Phase-26, «первая реальная замена
   логического юнита»). Метод `BuildPackets(phaseContext)` **игнорирует вход** —
   буквально `_ = phaseContext;` — и возвращает **жёстко зашитый массив из 10 пакетов**
   (`td01…td10`) с заранее прописанным текстом и статусами `covered/concern/boundary`.
   `public const double Confidence = 0.92;` — **константа времени компиляции**, не
   выученное значение. Комментарий в коде честно называет вывод «deterministic output».
2. **`SecurityArbiterUnit.cs`** (раунд Phase-85, «CB4 pair-backed node-08»). Метод
   `Evaluate()` делает **сопоставление ключевых слов**: `normalized.Contains("pkce")`,
   `Contains("code_challenge_method=plain")`, `Contains("ess-session-001")` — и по ним
   выбирает один из **трёх жёстко зашитых вердиктов** (PASS/REPROCESS/CONCERN), привязанных
   к id пар CB4-3.4-001/005/009. `public const double Confidence = 0.94;` — снова константа.
   Это regex/keyword-detector, а не обученная модель.
3. **Контракт замены (T35 из implementation-phase-plan)** сам по себе доказывает суть:
   критерий приёмки — вывод юнита должен **точно совпасть** с детерминированным оракулом
   T34 (`packet_id_mismatch_count = 0`, `domain_label_mismatch_count = 0`, …), а гейт
   уверенности — `unit_min_confidence >= 0.70` (проходит зашитой 0.92/0.94). То есть
   «замена логического юнита» — это **рефакторинг интерфейса** (тот же результат через
   `ILogicalUnit`), а не доказательство обучения.
4. **Batch-1/Batch-2 findings** прямо пишут: у Flow-01 **нет корпусных pair-файлов**
   для node-08 и node-09 (Finding F1/G1). Значит «trained arbiter/formatter» доказаны
   против ответственностей E53, а **не против живого LLM-корпуса**. «Обучение» = адаптер-
   контракт поверх зашитых ответов.

**Нюанс эволюции (важно для плана продолжения):** более поздние планы (2026-05-05, v4–v7)
**сами признают** этот пробел и меняют направление: `WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-
PLAN-v4` — «locks the transition layer as **non-deterministic and orchestrator-based**»;
`ORCHESTRATION-TEACHING-CYCLE-PLAN-v3` — «trains and measures the transition **orchestrator
pattern**, not a single deterministic normalizer»; `LEARNING-CYCLE-IMPLEMENTATION-PLAN-v3`
— «Version 3 makes it an **orchestrator**». То есть проектная мысль двигалась от
«детерминированный interface-swap» → к «адаптивный обучаемый transition-orchestrator»,
но **реализованный код на дату папки остаётся детерминированным**. Это ровно тот разрыв
(scaffold честный, но не обученная фича), который важен для плана продолжения расширений.

**Согласование с RAG-06:** полностью совпадает. RAG-06 видел это со стороны `llm dynamic
flow`; здесь виден полный пакет E53 и исходники — вывод тот же: **E53 доказывает, что
9-узловая архитектура сохраняет консистентную оркестрацию при поузловой замене фикстур на
адаптеры; она НЕ доказывает наличие обученной модели с весами/чекпоинтами.**

---

## Структура / манифест (каждый файл учтён)

Папка — это **журнал раундов**: почти каждый артефакт принадлежит одному из «раундов»
(round/phase), а раунд = {манифест + результаты/evidence + zip-снимок}. Ниже — по семьям.

### A. Верхнеуровневые планы и модели (индивидуально — это «мозг» папки)

| Файл | Что это |
|---|---|
| `MASTER-EXPERIMENT-PLAN-v1.md` | Карта **14 экспериментов в 4 категориях** (см. разбор ниже). |
| `E53-ORCHESTRATION-PROOF-PLAN-v1.md` | Дизайн доказательства E53: 3 сценария (конфликт требований, knowledge edge, цепочка зависимостей), ручной прогон с human-proxy. |
| `E53-ROUND-D-PLAN-AND-BATCH-PROTOCOL-v1.md` | Протокол батчей D–G / H–K для поузловой замены. |
| `MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | Дизайн многофазной оркестрации: **Phase Context Packet**, **Phase Transition Gate**, фаза→фаза перенос решений/ограничений/knowledge-edge. |
| `LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | **Ключевой** план кода: `ILogicalUnit → ILogicalFlow → ILogicalOrchestrator`, фазы 25–31, флаги, гейты T35. |
| `LOGICAL-TRANSITION-ORCHESTRATION-MODEL-v1/v2.md` | Модель перехода: `human prompt → A/B/X transition object → expert split`; v2 supersedes v1. |
| `WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-PLAN-v1..v4.md` (+ `-EXPERIMENT-SET-v1`) | Как учить/тестировать flow как оркестратор **всей разработки**; v4 фиксирует transition-слой недетерминированным. |
| `ORCHESTRATION-TEACHING-CYCLE-PLAN-v1..v3.md` | Цикл обучения оркестрации; v3 — первый «настоящий» цикл после коррекции transition-слоя. |
| `LEARNING-CYCLE-IMPLEMENTATION-PLAN-v1..v3.md` | Реализация цикла обучения; v3 = transition-orchestrator версия. |
| `CORPUS-EXPANSION-PLAN-v1..v7.md` (7 шт.) | План расширения обучающего корпуса; v7 = transition-orchestrator teaching corpus. |
| `ORCHESTRATOR-INSPECTOR-DESIGN-v1..v4.md` (v3-final, v4-performance-first) | Дизайн **Windows-desktop инспектора** рантайма оркестратора (проекты 21–23): станции, топология; v4 меняет 3D Helix на лёгкий WPF Canvas. |
| `ORCHESTRATION-RESOURCE-METRICS-SPEC-v1.md` | Спека метрик ресурсов (CPU/RAM/время) оркестрации. |
| `EXAMINATION-METHOD-ORCHESTRATION-v2..v6.md` (5 шт.) | Метод «examination» артефактов раунда (структурный/контрактный/суждение). |
| `ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | Мета-эксперимент M1: проверка самого протокола экзаменации. |
| `HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT-v1.md`, `HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1.md` | Методички: как из плана/скилла/гайда сделать эксперимент и learning-set. |
| `DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md`, `DEVELOPMENT-PHASES-GUIDE-EXTENSION-v1.md` | Карта жизненного цикла разработки → фазы/суб-фазы оркестрации. |
| `GUIDANCE-DOMAIN-MAPPING-…`, `SKILL-DOMAIN-MAPPING-…`, `PLANNING-DOCUMENT-BINDING-…`, `PLANNING-DOCUMENT-DOMAIN-MAPPING-…`, `SKILL-BINDING-…`, `LEARNING-SET-TRANSLATION-BINDING-EXPERIMENT-PLAN-v1.md` | Планы экспериментов Категории 2 (G1–G6): маппинг доменов guidance/skill/planning документов. |
| `EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md` | Источник экспериментов Категории 3 (T1–T5): экзамен обучающих пар по узлам. |
| `EXTERNAL-SOURCE-REGISTRY-v2..v5.md`, `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v2..v5.md` (8 шт.), `SOURCE-AUTHORITY-COVERAGE-AUDIT-v1.md` | Реестр внешних источников и пайплайн их прогона через оркестрацию. |

### B. Раунды E53 (замена фикстур на «trained» адаптеры) — 8 подпапок + батчи

- **`E53-ROUND-D` … `E53-ROUND-K`** (8 подпапок). В каждой: `ROUND-MANIFEST-…-E53-ROUND-X-TRAINED-*.md`,
  `E53-ROUND-X-VALIDATION-RESULTS.md`, `BATCH-ROUND-SUMMARY.md`. Что «обучается» по раундам:
  D = node-00 compiler, E = node-03 scheme planner, F = node-08 arbiters, G = node-09
  formatter, H = node-02 domain translator, I = node-04/05 processor+applier,
  J = node-06 result translator, K = node-07 arbiter selector.
- **`E53-BATCH-1/BATCH-REVIEW-FINDINGS.md`** (раунды D–G), **`E53-BATCH-2/BATCH-REVIEW-FINDINGS.md`**
  (раунды H–K) — архитекторский обзор батча: таблица стабильности 12 метрик, Findings F1/G1
  (нет node-08/node-09 корпуса).

### C. Раунды фазовых переходов и «обучения на неудачах»

- **`T34-PHASE-TRANSITION-ROUND-A-2026-05-04/`** — исходное связывание, доменная карта,
  контракт исполнителя, ожидаемый trace, таксономия сбоев, human-result стандарты перехода.
- **`P3-LEARNING-FROM-FAILURE-ROUND-A-2026-05-04/`** — цикл «learning from failure»:
  дискриминатор сбоев, таксономия цикла обучения, source-binding, ожидаемый trace.
- **`ROUND-MANIFEST-2026-05-04-T34-PHASE-TRANSITION-ROUND-B.md`** — раунд B перехода.

### D. Раунды WDA (Whole-Development-Area) — реальный router

- **`WDA-00-ROUND-A`**, **`WDA-01-ROUND-A`**, **`WDA-01-ROUND-B`**, **`WDA-02-THROUGH-06-FINAL`**
  (4 подпапки) + `WDA-01-REAL-SOURCE-LEDGER-v1.md`, `WDA-01-ROUTER-REPLACEMENT-EVIDENCE-2026-05-05.json`,
  `WDA-01-ROUTER-REPLACEMENT-EXPECTED-TRACE-v1.md`, `WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md`.
  Контракты/evidence/confusion-таблицы для «реального» суб-фазного роутера.

### E. ROUND-MANIFEST-*.md верхнего уровня — **96 файлов** (журнал фаз 25→118)

Это скелет папки. Разбиение по смыслу:
- **Phase 25–31** — построение иерархии: 25 logical-unit-contract, 26 T35 logical-unit-
  replacement, 27 logical-flow-adapter, 28 logical-orchestrator, 29 unit-registry,
  30 flow-registry, 31 orchestrator-assistant.
- **Phase 32–52** — топология/фазы/суб-фазы/переходы как **мутируемые команды**:
  contract-relocation, topology-registry, topology-commands, add-domain/phase/sub-phase/
  transition mutation-unit, topology-flow-*, assistant-*-route, *-registry.
- **Phase 53–70** — WDA-набор, corpus-expansion, WDA-раунды, **инспектор** (60–70:
  inspector-design/shell/data-layer, trace-context, tracing-decorators, station-trace-tree,
  trace-tree-panel, parallel-wave-rendering, station-inspector, helix-topology, phase-subphase-nodes).
- **Phase 71–81** — инспектор рантайма (pending-buffering, command-bar, project21-23 traces,
  resource-guard, lightweight-topology, v4-plan-alignment) + corpus-completion-ledger + WDA-01 real-router.
- **Phase 82–99** — Wave-1 quota/heldout, **CB4** intake и поузловые CB4-замены (node-01…09
  как arbiter-unit/selector/formatter/translator/applier/processor/planner/splitter),
  ext31-broadening, cross-source, ext24-closure, ext25-pipeline-chain, Wave-2-center.
- **Phase 100–118** — Wave-2-center узлы 02/03/04-05-06/07-08-09, batch-001 index,
  corpus-v4-audit, corpus-plan-v5-continuous, Wave-2 батчи 002–008 (UIUX / functional-spec /
  technical-remainder / implementation-remainder / testing-qa / qa-automation / transitions),
  corpus-build-ready, orchestration-learning-plan-rewrite, logical-transition-correction,
  transition-orchestrator-pattern, canonical-external-learning-set-method.
- Плюс `ROUND-MANIFEST-…-PHASE-DOCUMENTATION-COMBINATION-AUDIT.md`.

### F. Wave-корпуса (обучающие пары/леджеры) — верхний уровень

- **WAVE-1** (5 файлов): conformance-suite, held-out-split, source-gap-analysis,
  quota-ledger v1/v2.
- **WAVE-2-BATCH-002…008** (**35 файлов**): по каждому батчу — INDEX / SOURCE-LEDGER /
  COVERAGE-LEDGER / CONFORMANCE-NOTES / FULL-PATH-PAIR-SET (или TRANSITION-PAIR-SET).
- **WAVE-2-CENTER-PHASE-BATCH-001** (**13 файлов**): conformance-suite v1/v2, coverage-ledger
  v1/v2, full-path-summary, heldout-manifest + `…HELDOUT-CONFORMANCE-EVIDENCE-2026-05-05.json`,
  index, source-ledger, sub-phase-router-set, node-02/03/04-05-06/07-08-09 pair-sets.
- **CENTER-PHASE-QUOTA-BATCH-001-CB4-INTAKE-LEDGER.md**.

### G. CB4 (Corpus-Build-4) — security-verification flow

- **`CB4-STAGE-5*` evidence JSON (14 шт.)**: STAGE-5 security-flow-registration + 5A…5M
  (node-08 arbiter-replacement, node-07 arbiter-selector, node-09 formatter, node-06
  result-translator, node-05 applier, node-04 processor, node-03 planner, node-02 translator,
  node-01 splitter, ext31-broadening, cross-source, ext24-closure, ext25-pipeline-chain).
- **`CB4-STAGE-5-SECURITY-FLOW-REGISTRATION-PLAN-v1.md`** + `CORPUS-BUILD-READY-COMPLETION-SUMMARY`.

### H. Леджеры и аудиты завершённости корпуса

- **CORPUS-COMPLETION-LEDGER-v1..v30** (**30 файлов**) — инкрементальный журнал готовности корпуса.
- `CORPUS-V4-FINAL-COMPLETION-AUDIT-2026-05-05.md`, `CORPUS-COMPLETION-LEDGER-v*`.

### I. Аудит документации фаз — подпапка

- **`phase-documentation-audit-2026-05-04/`**: `PHASE-DOCUMENTATION-AUDIT-REPORT-v1.md`,
  `PHASE-REGISTRY-AND-COVERAGE-MATRIX-v1.md`, `PHASE-TRANSITION-EXPERIMENT-MATRIX-v1.md`,
  `NEXT-ACTION-ITEMS-FOR-MULTI-PHASE-ORCHESTRATION-v1.md`, манифест, **5 CSV**
  (phase-document-registry, phase-orchestration-readiness, phase-subphase-coverage,
  phase-transition-tests) + `phase-documentation-audit-summary.json`.

### J. Отчёты, визуализации, evidence

- `EXPERIMENT-REPORT-T34-AND-P3-2026-05-04` в 5 форматах (md / html / pdf / VISUAL.html / VISUAL.pdf).
- **9 SVG**: continuous_learning_integration, dynamic_flow_data_layer_uml, dynamic_flow_runtime_uml,
  missing_tools_for_context_completion, orchestrator_inspector_layout и др.
- **7 PNG**: `phase-73-inspector-visual-preview*` (v2…v6 + open-active-preview) — превью инспектора.
- **3 MMD** (Mermaid), в т.ч. `WDA-02-06-DEPENDENCY-GRAPH.mmd`.

### K. Код-снимки (реальный C#) — где смотреть реализацию

- **`round-zips/`** (58 подпапок, **87 файлов**: 51 вложенных zip + 11 md + **25 .cs**):
  извлечённые снимки стадий Phase 25–30 и T34-round-B. Здесь живут контракты и юниты:
  `LogicalContracts.cs`, `LogicalFlowContracts.cs`, `LogicalOrchestratorContracts.cs`,
  `InMemoryLogicalOrchestrator.cs`, `InMemoryLogicalUnitRegistry.cs`, `InMemoryLogicalFlowRegistry.cs`,
  `T34LogicalFlowAdapter.cs`, **`T34TransitionDomainSplitterUnit.cs`**, `T35LogicalUnitReplacementProof.cs`,
  `T34PhaseTransitionProof.cs`, + тесты.
- **`round-phase-84…87-…-staging/`** (4 подпапки, code/docs): CB4 security-flow — **`LogicalOrchestratorAssistant.cs`**
  (проект 15.OrchestratorModel), `SecurityVerification34FlowAdapter.cs`, **`SecurityArbiterUnit.cs`**,
  `SecurityArbiterSelectorUnit.cs`, `SecurityFormatterUnit.cs`, тесты.

### L. Zip-снимки — **112 zip всего**

- **61 zip верхнего уровня** = 57 × `round-phase-NN-*.zip` (пофазные снимки Phase 61…117)
  + 4 агрегатных: `70-81 runs results.zip`, `82-93 rounds results.zip`, `run 51-70 results.zip`,
  `orchestrator docs.zip`.
- **51 zip внутри `round-zips/`** (вложенные пофазные снимки Phase 25–30 и T34-round-B).

---

## Подробный разбор (глубоко — верхние документы)

### 1. `MASTER-EXPERIMENT-PLAN-v1.md` (496 строк)

**Что это:** главная карта — **14 экспериментов в 4 категориях**, все стартуют с «Round A»
(дизайн-документы), затем «Round B+» (исполнение). Базовая линия тестов: **997/997** после
Round C (планы, где написано 985, устарели — исполнитель мысленно подставляет 997).

- **Категория 1 — E53 Fixture Replacement (Rounds D–K).** Поузлово заменять
  детерминированные фикстуры «обученными» юнитами, **оракул E53 и 12 тестов неизменны**.
  Батч-1 = D–G, Батч-2 = H–K. Обзор батча следит за **12 метриками**, которые должны
  оставаться стабильными: 11 domain-packets, 0 lost-edges, 0 dependency-violations,
  0 invented-biometric-items, duplicate-creation=false, 0 forbidden-stack-terms,
  5/5 human-result-sections, 0 live-external-calls, 4 learning-gap-packets,
  e2e-orchestration-success=true. **Любое изменение метрики между раундами = finding.**
- **Категория 2 — G1–G6 (Domain/Governance document examination):** проверяет, что
  guidance/skill/planning-документы корректно маппятся на домены и привязываются к
  арбитрам. Использует саму оркестрационную архитектуру как **движок экзаменации**.
  Каждый эксперимент = 5 оракул-документов (domain-map, failure-taxonomy, expected-trace,
  human-result-standards, executor-contract). G1 источник — `SESSION-START-PROMPT-v2_1`,
  G3 — `CONTINUOUS-LEARNING-SESSION-PLAN`, и т.д. G2 и G6 частично пересекаются (skill routing).
- **Категория 3 — T1–T5 (Training-pair examination):** пять экспериментов из
  `EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1`. Ловят распространённый сбой обучающих пар:
  **B описывает вывод, а не является выводом**; кроссоверы между узлами (applier-логика в
  planner-паре и т.п.).
- **Категория 4 — M1 (мета-эксперимент):** проверяет сам трёхслойный протокол экзаменации
  (structural / contract / judgment). Зависит от наличия хотя бы одного реального раунда
  (B/C/D), т.к. сценарий E3 воспроизводит паттерн b08-two-state из Round B.

**Что доказывает завершённый набор:** Кат.1 — архитектура даёт **консистентную оркестрацию**
при поузловой замене фикстур; Кат.2 — архитектура обобщается на **экзаменацию документов**;
Кат.3 — умеет проверять **свои же обучающие пары**; Кат.4 — сам протокол экзаменации корректен.
**Релевантность плану продолжения:** это доказательство «архитектура держит контракт», а не
«модель обучена». Теги: `#e53` `#orchestration` `#examination-engine` `#deterministic-proof`.

### 2. `MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` (637 строк)

**Что это:** дизайн расширения 9-узлового (одно-фазного) flow до **многофазного
жизненного цикла разработки** (UI/UX → functional spec → technical design → implementation → QA).
Добавляет 3 вещи **над и рядом** с пайплайном (не заменяя его):

1. **Phase Context Packet** — структурированный артефакт, который завершённая фаза
   производит вместе с human-result: `Decisions made` (принятые решения + rationale + источник),
   `Constraints inherited` (неразрешённые ограничения — переносятся без изменений),
   `Knowledge edges` (названная нехватка компетенции), `Boundary markers` (присутствует, но
   не доспецифицировано), `Rejected items` (что арбитры отклонили + причина). **Механизм
   честности переходов:** следующая фаза стартует не с чистого листа, а с полной записи
   принятого/отклонённого/отложенного/неизвестного.
2. **Phase Transition Gate** — между двумя фазами проверяет 4 условия: `Constraint
   preservation`, `Decision acknowledgment`, `Knowledge edge continuity`,
   `Rejection preservation`. Вердикт: **PASS / PASS WITH GAPS / BLOCK**. Гейт **не запускает**
   следующую фазу — только решает, разрешён ли старт.
3. (Часть 1 продолжается фазовым реестром/командами — реализовано в манифестах Phase 32–52.)

**Связь:** это концептуальная основа под T34 (phase-3→phase-4 transition) и WDA (whole-
development-area). Именно отсюда растут «phase context packet», которые `T34TransitionDomainSplitterUnit`
принимает на вход (и, как показано выше, игнорирует). Теги: `#multi-phase` `#transition-gate`
`#context-packet` `#lifecycle`. Кросс: RAG-05 (иерархия), RAG-02 (runtime-модель).

### 3. Один манифест раунда E53 — `E53-ROUND-D` (TRAINED-COMPILER) + VALIDATION-RESULTS

**Что это:** раунд 16, заменяет **только node-00** (logical structure compiler) на «trained
Flow-01 node-00 compiler». Node-01 остаётся «trained splitter» из Round C; узлы 02–09
остаются детерминированными. **Что реально изменилось:** `BuildCompiledLogicalStructure()`
теперь строит сырую human-проблему E53 и передаёт в `Flow01Node00TrainedLogicalCompiler.Compile(...)`,
который «applies **learned signal rules** derived from the accepted Flow-01 node-00 pairs»
(детектирует subject/units/dependencies/conflicts/boundaries/knowledge-edges/required-outputs/
no-stack). В evidence-JSON добавлено поле `compiled_logical_structure` (чтобы grep-гейт видел
`local_subject`).

**Verdict:** PASS. Метрики после замены: domain-packets **11**, node02-in/out **11/11**,
learning-gap-packets **4**, live-external-calls **0**, e2e-success **true**, lost-edges **0**,
dependency-violations **0**. Тесты: E53 focused **12/12**, полный сет **997/997** — без дрейфа.
**Судейская проверка:** «trained compiler introduced **no metric variance** relative to Round C» —
т.е. «обучение» дало **побайтово тот же результат**, что детерминированная фикстура. Это и есть
подпись рефакторинга-адаптера, а не обучения. Теги: `#e53-round-d` `#node-00` `#no-metric-variance`
`#adapter-not-model`.

### 4. `LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` (398 строк) — КЛЮЧЕВОЙ

**Что это:** самый важный код-план: как перейти от «экспериментов-классов» к
**функциональному расширяемому оркестратору** без большого небезопасного переписывания.
Три абстракции по порядку (в проекте `35.ExperimentsV2/LogicalFlow`, **не новый проект**):

- **`ILogicalUnit<TInput,TOutput>`** — наименьшая исполнимая ячейка; async с самого начала;
  детерминированные юниты возвращают `Task.FromResult(...)`. Явно сказано: юнит **может быть**
  «deterministic, internally LLM-backed, external-approval-backed, learned, or hybrid» —
  все через один интерфейс. (Это и есть «дверь», через которую детерминированное выдаётся
  за «trained».)
- **`ILogicalFlow`** — топология юнитов с рёбрами/гейтами; E53 и T34 сейчас = flow'ы,
  написанные как классы-эксперименты.
- **`ILogicalOrchestrator`** — координатор уровня ассистента: выбирает flow, применяет флаги,
  запускает, хранит evidence, планирует обучение, возвращает human-result. Это будущая
  «orchestrator assistant surface».

**Флаги** (records `LogicalExecutionFlags`): Optimization (default true — `UseCompiledPredicate
Representation`, `UseDomainScopedRetrieval`, `UseConfidenceFloor`, `UseLearnedUnitWhenAvailable`,
`UseTraceEvidence`, `UseContextCarryForward`, `UseIdleLearningScheduling`); Safety (`UseArbiter
Authority`, `UseKnowledgeEdgeDetection`, `UseInternalFirstExecution`, `RequireExternalAiApproval`,
`RejectArbiterContradictions` — обязательны в продукте); Experiment-control (`UseDeterministic
FixtureBaseline`=false, `WriteEvidenceArtifacts`, `CompareAgainstOracle`, `FailOnMetricDrift`).

**Фазовый план 25–31:** 25 контракт `ILogicalUnit` (+ тесты флагов), **26 T35** — первая
реальная замена (node-01 splitter юнит), 27 `ILogicalFlow` + `T34LogicalFlowAdapter`,
28 `ILogicalOrchestrator` + `InMemoryLogicalOrchestrator`, 29 unit-registry, 30 flow-registry,
31 production-adjacent `LogicalOrchestratorAssistant` (в 15.OrchestratorModel, возвращает
`PASS_WITH_GAPS` для T34). **Гейты T35:** `logical_unit_replacement_success=true`, packet/domain/
coverage/source-mismatch = 0, `unit_min_confidence >= 0.70`. **Итоговая цель:** маленькие
юниты (детерм./learned/LLM) + flow-по-топологии + оркестратор + флаги + safety-гейты +
«путь от evidence эксперимента к продуктовому поведению ассистента». Теги: `#implementation-plan`
`#ILogicalUnit` `#ILogicalFlow` `#ILogicalOrchestrator` `#phases-25-31` `#interface-swap`.
Кросс: RAG-02 (проекты 15/21/22/23), RAG-05 (иерархия ILearningDynamicGraph).

---

## Как это связано со свипом (для будущего плана продолжения расширений)

- **Что здесь реально есть:** зрелая, поэтапно построенная **иерархия оркестрации** (юнит→flow→
  оркестратор→реестры→топология→фазы/переходы) с чистыми контрактами, evidence-дисциплиной,
  инспектором рантайма и большим корпусным аппаратом (Wave-1/2, CB4). Архитектура —
  переносимая (stack-independent) и подходит как «мозг» для расширений VS/VS Code.
- **Чего здесь НЕТ:** реальной обученной модели. «Trained» юниты = детерминированные адаптеры
  (жёсткие массивы + keyword-detectors + `const` confidence). Нет весов, чекпоинтов, градиентного
  обучения, held-out-метрик обученной фичи. Для плана продолжения это **PASS_AS_HONEST_UNTRAINED_
  SCAFFOLD**, а не `PASS_AS_TRAINED_FEATURE`.
- **Куда двигалась мысль:** поздние планы (v4–v7, 2026-05-05) переориентируют transition-слой
  на недетерминированный обучаемый **transition-orchestrator** — это готовый «следующий шаг»,
  но не реализованный в коде на дату папки.

---

## Рамка для проходов 2–10 (что осталось; этот файл — pass 1/10)

**Покрыто в pass 1:** полная карта (513 файлов / 94 папки), учёт всех семей, глубокий разбор
4 верхних документов + 2 реальных .cs (подтверждение детерминизма) + batch-findings + E53-proof-plan.

**Осталось разобрать (≈505 файлов), план проходов:**
- **Pass 2** — E53-ROUND-E…K: манифесты + validation-results (что «обучается» на каждом узле,
  метрики стабильности) + Batch-2 findings.
- **Pass 3** — T34 (Round A/B) и P3 (learning-from-failure): контракты, таксономии сбоев,
  ожидаемые trace, human-result стандарты.
- **Pass 4** — WDA-00/01/02-06: реальный суб-фазный router, confusion-таблицы, evidence.
- **Pass 5** — ROUND-MANIFEST Phase 25–52 (иерархия + топология/фазы/переходы как команды).
- **Pass 6** — ROUND-MANIFEST Phase 53–81 (WDA-набор + инспектор рантайма 60–81, проекты 21–23).
- **Pass 7** — ROUND-MANIFEST Phase 82–118 (CB4 security-flow, Wave-2-center узлы, батчи 002–008).
- **Pass 8** — Wave-1/Wave-2 корпуса + CB4-STAGE evidence (14 json) + CORPUS-COMPLETION-LEDGER v1–v30.
- **Pass 9** — методички/модели/инспектор-дизайны/EXAMINATION-METHOD/EXTERNAL-SOURCE registry +
  реальный код в `round-zips/` (25 .cs: контракты, реестры, оркестратор) и staging (CB4 .cs).
- **Pass 10** — сплошная зачистка (ни один файл не пропущен) + executive summary + финальный
  вердикт trained-vs-deterministic по всей папке.

**Честная строка покрытия pass 1:** прочитаны целиком 6 документов
(`MASTER-EXPERIMENT-PLAN`, `E53-ORCHESTRATION-PROOF-PLAN`, `E53-ROUND-D` манифест +
validation-results, `E53-BATCH-1` findings, `LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN`,
частично `MULTI-PHASE-ORCHESTRATION-DESIGN` — 120 строк) и **2 реальных C#-файла целиком**
(`T34TransitionDomainSplitterUnit.cs`, `SecurityArbiterUnit.cs`) + 8 документов по 14 строк
(head-выборка для категоризации) + `LogicalOrchestratorAssistant.cs` (60 строк). Остальные
505 файлов учтены структурно по имени/семье/подпапке (без построчного чтения — это работа
проходов 2–10). Заявляю **только** карту + структуру + вердикт по прочитанному коду; полный
построчный разбор раундов НЕ заявляется как выполненный в pass 1.

---

## Pass 2/10 — E53-раунды A-K (по-раундовая проверка: обученный юнит или детерминированный оракул?)

> Прочитано целиком в этом проходе: **все 24 файла** раундов D–K (по 3 на раунд:
> ROUND-MANIFEST + VALIDATION-RESULTS + BATCH-ROUND-SUMMARY) + **оба** BATCH-REVIEW-FINDINGS
> (Batch-1 D–G, Batch-2 H–K) + **реальный C#-исходник** `E53HighLevelOrchestrationProof.cs`
> (1390 строк, `C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\`) — прочитаны тела
> `Flow01Node00TrainedLogicalCompiler` (compile + все Detect*-методы), `Flow01Node01TrainedDomainSplitter`
> (LearnedRules[] + Split + BuildSignalSet), `Flow01Node08TrainedArbiterUnits` (LearnedRules[] + Review)
> и заголовки всех 10 «trained»-классов. Grep-подтверждения по всему файлу (см. ниже).

### Уточнение по составу «A-K» (задание сказало A..K, физически есть D..K)

В папке **нет** подпапок E53-ROUND-A/B/C. По `MASTER-EXPERIMENT-PLAN` и манифесту раунда D:
Round A = дизайн-документы, Round B = первый прогон, **Round C = baseline `997/997` + первый
«trained» node-01 splitter** (это исходная точка перед D). Реальные пакеты замены фикстур —
это **раунды D–K (round 16→23)**. Round D манифест прямо пишет: «Round C's trained node-01
splitter stays active». Поэтому «trained node-01 splitter» — это Round C, а D–K добавляют
остальные 9 узлов поверх него.

### ГЛАВНЫЙ вывод pass 2 (подтверждён исходником, не только манифестами)

**Все 10 «обученных» юнитов E53 (node-00…node-09) — детерминированные жёстко-зашитые
оракулы, а не машинно-обученные модели.** Механизм идентичен у всех узлов:

1. Каждый «trained» юнит — это `private static class Flow01NodeNNTrained*` **внутри одного
   файла** `E53HighLevelOrchestrationProof.cs`. Отдельных .cs-юнитов, сборок, ассетов — нет.
2. Внутри — массив **`LearnedRules[]` из record-ов**, где **вручную прописаны точные
   ожидаемые выводы E53** (id пакетов `d01…d11`, тексты, статусы, вердикты `r01…r10`).
   «Learned» — это **ярлык в комментарии**, а не результат обучения. Пример (node-01):
   `new("d01_provider_identity_entry", "provider_identity_entry", "covered", "...", ["provider_sign_in"])`.
3. Выбор правил — это **фильтр совпадения сигналов**:
   `LearnedRules.Where(rule => rule.RequiredSignals.All(signals.Contains)).Select(...)` —
   не инференс, а «если сигнал присутствует → верни зашитую строку».
4. Сигналы у node-00 компилятора строятся **keyword-матчингом `Contains()`** по нормализованному
   (ToLowerInvariant) тексту **единственного фиксированного** E53-сценария:
   `if (Contains(normalized, "google") && ContainsAny(normalized, "sign-in","provider")) units.Add("provider_sign_in")`;
   `if (Contains(normalized, "biometric") && Contains(normalized, "passport")) units.Add("biometric_passport_knowledge_edge")`;
   `DetectLocalSubject` возвращает зашитую строку `complex_registration_and_onboarding_orchestration`
   при наличии слов onboarding+flow+account, иначе `unknown_orchestration_subject`.
5. **Grep по всему файлу (1390 строк):** `checkpoint`=0, `gradient`=0, `epoch`=0, `weight`=0,
   `Tensor`=0, `softmax`=0, `Confidence`=**0** (в E53-юнитах даже нет `const Confidence`, в
   отличие от T34/Security-юнитов из pass 1), `dpo/backprop/neural/embedding`=0. `Train`=63 —
   **все** это имена классов/комментарии `Trained`; `learned`=84 — все это ярлык на зашитых
   правилах. Ни одного признака обучения, весов, чекпоинтов или DPO.

Иначе говоря: «обучение derived from accepted Flow-01 pairs» на деле означает, что **человек
прочитал пары и захардкодил правила** — а для узлов 04–09 даже пар не было (см. Findings).

### Таблица по раундам (unit → method → deterministic-or-trained → validation)

| Раунд (round#) | Заменённый узел → «trained»-класс | Метод (что реально делает код) | Det/Trained | Validation (E53 / full / ключевые метрики) |
|---|---|---|---|---|
| **D** (16) | node-00 logical compiler → `Flow01Node00TrainedLogicalCompiler` | keyword `Contains()`-матчинг по норм.тексту → зашитые units/deps/conflicts/boundaries/edges; `DetectLocalSubject` возвращает `const` строку | **ДЕТЕРМ.** | 12/12 · 997/997 · 11 domain-packets · `local_subject` виден · **no metric variance vs Round C** |
| **E** (17) | node-03 scheme planner → `Flow01Node03TrainedSchemePlanner` | `LearnedRules[]` (зашитые plan-item records) + `.Where(RequiredSignals.All(signals.Contains))` | **ДЕТЕРМ.** | 12/12 · 997/997 · 11 plan-items · b08 two-state сохранён · no variance |
| **F** (18) | node-08 arbiter units → `Flow01Node08TrainedArbiterUnits` | `LearnedArbiterVerdictRule[]` (7 зашитых вердиктов r01…r10) + фильтр совпадения сигналов | **ДЕТЕРМ.** (oracle-backed) | 12/12 · 997/997 · 7 verdicts · **Finding F1: нет node-08 corpus pair-файла** |
| **G** (19) | node-09 formatter → `Flow01Node09TrainedFormatter` | зашитые section-rules (5 секций human-result checklist) | **ДЕТЕРМ.** (oracle-backed) | 12/12 · 997/997 · 5/5 sections · **Finding G1: нет node-09 corpus pair-файла** |
| **H** (20) | node-02 domain translator → `Flow01Node02TrainedDomainTranslator` | `LearnedTranslatedStatementRule[]` + фильтр сигналов; 11 packets→11 statements | **ДЕТЕРМ.** | 12/12 · 997/997 · 11→11 · handoff/conflict/boundary/edge пути сохранены |
| **I** (21) | node-04 processor + node-05 applier → `Flow01Node04TrainedProcessor` + `Flow01Node05TrainedApplier` | два массива зашитых outcome/result-правил + фильтр сигналов | **ДЕТЕРМ.** | 12/12 · 997/997 · 11 processed / 11 applied · нет node-04/05 corpus |
| **J** (22) | node-06 result translator → `Flow01Node06TrainedResultTranslator` | зашитые fragment-fold правила; 11 formal→7 human fragments | **ДЕТЕРМ.** | 12/12 · 997/997 · 11→7 fold сохранён · нет node-06 corpus |
| **K** (23) | node-07 arbiter selector → `Flow01Node07TrainedArbiterSelector` | зашитые selector-responsibility правила + фильтр риск-сигналов | **ДЕТЕРМ.** | 12/12 · 997/997 · 7 selections · нет node-07 corpus |

*(Round C baseline, для контекста: node-01 splitter → `Flow01Node01TrainedDomainSplitter` —
тоже `LearnedRules[]` из 11 зашитых DomainPacket-record → **ДЕТЕРМ.**)*

### Что означают «12/12» и «997/997» (почему они НЕ доказывают обучение)

- **12** = число focused-тестов E53 (перечислены в BATCH-ROUND-SUMMARY раунда D:
  `E53_Run_PreservesAllHardEdges`, `..._RejectsDuplicateAccountCreation`,
  `..._RoutesBiometricPassportToKnowledgeEdge`, `..._KeepsFinalResultStackIndependent` и т.д.).
- **997** = полный тест-сет проекта. Обе цифры **никогда не двигаются** во всех 8 раундах.
- Причина неподвижности прямо названа в каждой VALIDATION: «**trained X introduced no metric
  variance**», «no orchestration metric changed». Это и есть подпись: **оракул E53 и 12 тестов
  НЕ меняются между раундами** (манифест: «expected trace oracle did not change», «twelve E53
  tests did not change»), а адаптеры настроены **воспроизводить фикстуру побайтово**. Тест,
  который сравнивает вывод с неизменным оракулом, всегда зелёный, если адаптер = тот же оракул
  другим интерфейсом. Это доказывает **консистентность контракта при interface-swap
  (`ILogicalUnit`)**, а не наличие обученной модели.

### Cross-round стабильность (из BATCH-REVIEW-FINDINGS — все метрики «yes/stable»)

Batch-1 (D–G) и Batch-2 (H–K) сводные таблицы: `domain packet count`=11,
`plan item count`=11, `arbiter verdict count`=7, `human fragment count`=7,
`arbiter selection count`=7, `human result sections`=5/5, `e2e_orchestration_success`=true,
`lost_edge_count`=0, `dependency_violation_count`=0, `forbidden_stack_term_count`=0,
`learning_gap_packet_count`=4 — **все стабильны во всех раундах**. Вывод обзоров дословно:
«No architecture-contract drift». То есть замена всех 10 узлов не сдвинула ни одной метрики.

### Корпусный разрыв — прямое признание в исходниках (важно для плана продолжения)

Манифесты F и G и оба Findings **честно пишут**, что для узлов **04, 05, 06, 07, 08, 09 в
`flow-01-training-sets-v1` НЕТ registration-specific pair-файлов**. Цитата (Round F):
«No Flow-01 registration-specific node-08 pair file exists … Round F therefore uses the
accepted E53 arbiter responsibilities … as an **oracle-backed trained adapter**. This … remains
a **corpus gap before live LLM replacement**». Batch-2 классифицирует это как **corpus gap,
не architecture gap**. То есть даже формально «обучаемых» пар у половины узлов нет — их
«обучение» = зашитый под E53-ответственности оракул.

### Общий вердикт E53 (pass 2)

**E53 = доказательство детерминированной оркестрации через поузловую замену фикстур на
адаптеры-оракулы. НЕ доказательство обученной модели.** Все 10 узлов node-00…node-09 после
раундов D–K работают, но каждый — жёстко-зашитый `LearnedRules[]` + keyword-`Contains`-матчинг
по единственному фиксированному сценарию E53. Ни весов, ни чекпоинтов, ни градиента, ни DPO,
ни даже `const Confidence`. Это **полностью подтверждает** вывод RAG-06 (E53) и вердикт
pass-1: для плана продолжения расширений — **`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`**, а не
`PASS_AS_TRAINED_FEATURE`. Ценность E53 для плана: доказана переносимая, стабильная
`ILogicalUnit → ILogicalFlow → ILogicalOrchestrator` архитектура-«скелет», в которую позже
можно вставить реальные обученные/LLM-юниты **на месте тех же оракулов** (замена — уже
проверенный контрактом путь). Теги: `#e53-rounds-D-K` `#deterministic-oracle`
`#interface-swap` `#no-training-lifecycle` `#corpus-gap-node-04..09` `#confirms-RAG-06`.

### Что осталось для проходов 3–9 (без изменений к плану pass-1)

- **Pass 3** — T34 (Round A/B) + P3 (learning-from-failure): контракты, таксономии сбоев,
  ожидаемые trace, human-result стандарты; сверить `T34TransitionDomainSplitterUnit.cs`
  (pass 1 уже показал `_ = phaseContext;` + зашитый массив td01…td10 + `const Confidence=0.92`).
- **Pass 4** — WDA-00/01/02-06: «реальный» суб-фазный router, confusion-таблицы, evidence
  (проверить, детерминирован ли WDA-router так же, как E53-узлы).
- **Pass 5** — ROUND-MANIFEST Phase 25–52 (иерархия юнит/flow/оркестратор/реестры + топология/
  фазы/переходы как мутируемые команды).
- **Pass 6** — ROUND-MANIFEST Phase 53–81 (WDA-набор + инспектор рантайма 60–81, проекты 21–23).
- **Pass 7** — ROUND-MANIFEST Phase 82–118 (CB4 security-flow, Wave-2-center узлы, батчи 002–008);
  сверить `SecurityArbiterUnit.cs` (pass 1: keyword `Contains("pkce")` + `const Confidence=0.94`).
- **Pass 8** — Wave-1/Wave-2 корпуса + CB4-STAGE evidence (14 json) + CORPUS-COMPLETION-LEDGER v1–v30.
- **Pass 9** — методички/модели/инспектор-дизайны/EXAMINATION-METHOD/EXTERNAL-SOURCE registry +
  реальный код в `round-zips/` (25 .cs: контракты, реестры, оркестратор) и staging (CB4 .cs).
- **Pass 10** — сплошная зачистка + executive summary + финальный trained-vs-deterministic вердикт.

**Честная строка покрытия pass 2:** прочитаны **целиком** 24 файла раундов D–K + 2
BATCH-REVIEW-FINDINGS + ключевые тела реального `E53HighLevelOrchestrationProof.cs` (compile
node-00, splitter node-01, arbiter node-08) + заголовки всех 10 «trained»-классов + grep-аудит
lifecycle-терминов по всему файлу (1390 строк). Round-zip снимки (round-16…23) **не
распаковывались** — живой repo-файл является накопленным итогом раундов D–K и содержит все
классы, названные в манифестах; этого достаточно для cross-check кода. Заявляю **только**
по-раундовый вердикт trained-vs-deterministic для E53 D–K; проходы 3–9 остаются как выше.

---

## Pass 3/10 — T34 phase-transition + P3 learning-from-failure (self-improvement эксперименты)

> Прочитано целиком в этом проходе: **все 7 файлов** `T34-PHASE-TRANSITION-ROUND-A`
> (source-binding, transition-domain-map, transition-failure-taxonomy, expected-trace,
> human-result-standards, executor-contract, round-manifest) + `ROUND-MANIFEST-…-T34-ROUND-B`
> + **все 7 файлов** `P3-LEARNING-FROM-FAILURE-ROUND-A` (source-binding, failure-discriminator,
> learning-cycle-taxonomy, expected-trace, human-result-standards, executor-contract,
> round-manifest) + сводный `EXPERIMENT-REPORT-T34-AND-P3-2026-05-04.md` (911 строк) +
> **реальный C#-исходник** `P3LearningFromFailureProof.cs` (`C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\`):
> прочитаны тела `BuildLearningCycle`, `BuildVerificationRun`, `MentionsBaseService`,
> `BuildCorruptedScenarios`, `BuildCorruptedHumanResult` + grep всего файла на
> `LearnedDiscriminator/discriminatorActivated/Generalization/OverGeneralizationRisk`.
> Файл `T34PhaseTransitionProof.cs` подтверждён существующим (glob), тело кросс-проверено
> по отчёту; pass 1 уже прочитал родственный `T34TransitionDomainSplitterUnit.cs`
> (`_ = phaseContext;` + зашитый массив td01…td10 + `const Confidence=0.92`).

### Место T34/P3 в общей карте (это ДРУГАЯ ветка, чем E53)

E53 (pass 1–2) доказывал **одно-фазную** оркестрацию (9 узлов обрабатывают один source-packet).
T34 и P3 строят слой **НАД** 9-узловым пайплайном — **много-фазный жизненный цикл разработки**
(UI/UX → functional spec → technical design → implementation → testing → QA → QA-automation).
Это ровно те «self-improvement» эксперименты, которые релевантны петле плана «внешняя модель
учит внутреннюю»: **T34 = детекция перехода фаза→фаза**, **P3 = обучение на упавшем переходе**.
Концептуальная основа обоих — `MULTI-PHASE-ORCHESTRATION-DESIGN` (pass 1, §2): Phase Context
Packet + Phase Transition Gate. Кодовые фазы: T34 = code-phase 24, P3 = code-phase 25.

### ЧАСТЬ 1 — T34: как модель детектирует переход Phase 3 → Phase 4 (transition gate)

**Что проектирует T34.** Не «хорош ли downstream-план вообще», а узкий вопрос: **можно ли
СТАРТОВАТЬ downstream-фазу, если дан upstream Phase Context Packet?** Конкретная пара привязки
(`T34-SOURCE-BINDING`): upstream = Phase 3 technical-design (`DYNAMIC-FLOW-ARCHITECTURE-PLAN`,
`…AMENDMENT`, `DECISIONS-LOCKED`), downstream = Phase 4 implementation
(`AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0`). Upstream-пакет содержит **6 decisions (DCTX-01..06),
7 constraints (CSTR-01..07), 3 knowledge-edges (KEDGE-01..03), 2 boundary-markers (BND-01..02),
4 rejected-items (REJ-01..04)**.

**4 домена перехода** (`TRANSITION-DOMAIN-MAP`), по которым узел-02 переводит пакеты в формальные
утверждения — это и есть «датчик перехода»:
1. **Constraint Preservation** — сохранился ли upstream-constraint downstream с равной/большей
   силой (статусы: `preserved | weakened | absent`; enforcement: `enforced | stated_not_enforced | absent`).
2. **Decision Acknowledgment** — признано ли upstream-решение или явно помечено out-of-scope
   (`incorporated | out_of_scope | absent`).
3. **Knowledge Edge Continuity** — остаётся ли upstream knowledge-edge виден downstream, пока
   его не разрешит новый источник (`continued | resolved_by_source | absent`).
4. **Rejection Preservation** — остаётся ли отклонённый item отклонённым (ключ: `semantic_match`
   + `artifact_type_match` — отличить runtime-топологию от build-time-последовательности).

**Таксономия сбоев перехода F1–F6** (`TRANSITION-FAILURE-TAXONOMY` — это и есть «failure taxonomy
of transitions», о которой спрашивало задание):

| Тип | Сбой | Сигнал | Вердикт |
|---|---|---|---|
| **F1** | Dropped Constraint (upstream-constraint отсутствует downstream, хотя работа от него зависит) | ни упомянут, ни enforced | **BLOCK** |
| **F2** | Stated-But-Unenforced (constraint назван, но нет gate/test, который упал бы при нарушении) | правило видно, Gate B его не проверяет | **CONCERN** |
| **F3** | Absent Knowledge-Edge Acknowledgment (edge реально вне scope, но downstream не помечает out-of-scope) | edge не упомянут, компонент может идти дальше только т.к. edge вне его зоны | **CONCERN** |
| **F4** | Silent Knowledge-Edge Disappearance (edge исчез, downstream ведёт себя как будто решён) | нет источника, разрешающего edge, но работа зависит от него как от решённого | **BLOCK** |
| **F5** | Reinstated Rejected Item (отклонённый item вернулся с той же семантикой и типом артефакта) | upstream отклонил X, downstream снова использует X для той же цели | **BLOCK** |
| **F6** | Superficial Reinstatement (похоже на отклонённый item, но другой тип артефакта) | rejected = runtime-топология, downstream = build-time-последовательность | **PASS** |

**Смысл вердиктов:** `PASS` = сохранил/признал/корректно ограничил; `CONCERN` = стартовать можно,
но названный пробел надо закрыть до приёмки вывода фазы; `BLOCK` = upstream-авторитет отсутствует
или противоречит так, что фазу небезопасно стартовать. Самая тонкая граница — **F6/T34-3**:
механический чекер видит фиксированную последовательность и флагует как отклонённый статический
рантайм-пайплайн; корректный арбитр проверяет **тип артефакта** и ставит PASS.

**3 сценария-оракула** (ожидаемый trace `EXPECTED-TRACE`, вердикт `PASS_WITH_GAPS`, block-count 0):
- **T34-1** — BaseService inheritance назван upstream, но downstream-план не имеет Gate B/теста →
  **CONCERN** (F2).
- **T34-2** — cross-phase context packet edge вне scope AutonomousLearning, но не помечен
  out-of-scope → **CONCERN** (F3).
- **T34-3** — фиксированная последовательность Phase 1..5 = build-time порядок, НЕ рантайм-
  топология → **PASS** (F6).

**Как это течёт по 9 узлам** (тот же пайплайн, что E53, только вход = transition source-packet):
node-00 компилирует source-binding в `phase_transition_source_packet` (6/7/3/2/4 категории),
**node-01 → 10 domain-packets** (t34-p01…p10), node-02 → 10 формальных переводов в
transition-vocabulary, node-03 → план экзаменации s01…s07 (с параллельными группами и
критическим суждением «сравни тип артефакта, а не слова» для T34-3), node-04..06 обрабатывают
детерминированно, node-07 выбирает арбитры (constraint/knowledge-edge/rejection/severity-boundary),
node-08 выдаёт вердикты (2 CONCERN + 1 PASS, block=0), **node-09 → человеческий результат
`PASS WITH GAPS`** с 5 обязательными секциями (verdict / ≥5 сохранённых constraints / gap-1
BaseService / gap-2 cross-phase / accepted-pass build-time).

**T34 Round B (реализовано в коде, детерминированно).** `T34PhaseTransitionProof.cs`
(12 record-типов, 14 методов). Ключ — метод **`Mentions()`**: маппит upstream-constraint на
packet **keyword-матчингом** (`sourceItem.Contains("BaseService")` → true только для td06).
Пайплайн `BuildEvidence()` строит 10 зашитых domain-packets (td01…td10, td06/td07 = concern),
10 переводов, 3 сценария → вердикты. Результат: `PASS_WITH_GAPS`, 10/10 packets, 3/3 сценария,
2 concern / 0 block / 1 pass, 5/5 секций. Тесты **8/8**, baseline **997/997 → 1005/1005**,
commit `e6622a8` → main. Round B также перерисовал 4 SVG (runtime-uml, data-layer-uml,
continuous-learning, missing-tools) — вводит сущности `PhaseTopology`, `ContextPacketStore`,
`TransitionEvidence`, `PhaseLearningGap` (см. ниже — это персистентный слой для реальной петли).

### ЧАСТЬ 2 — P3: как модель «учится на упавшем переходе» (failure-discriminator + learning-cycle)

**Что проектирует P3.** P3 расширяет T34 и отвечает: **что происходит, когда gate реально
БЛОКИРУЕТ, и учится ли система правильному уроку — БЕЗ over-generalization?** Три под-эксперимента
строго по порядку (`P3-SOURCE-BINDING`):
- **P3-C (corrupted)** — тот же upstream-пакет, но downstream domain-packet-set **испорчен**:
  из 10 пакетов **убран td06 (BaseService)** → 9 пакетов, ни один не упоминает BaseService.
  Gate обязан выдать **BLOCK** с `LostConstraintCount=1`, атрибутированным именно BaseService.
- **P3-L (learning cycle)** — BLOCK-находка идёт в idle-learning cycle, который производит
  **1 FeedbackRecord** (назвать упавший предикат) + **1 LearnedDiscriminator** (закодировать
  границу) + **LearningCycleVerdict**.
- **P3-V (verification)** — оригинальный T34-набор (td06 present, concern) прогоняется снова;
  gate **консультируется** с реестром дискриминаторов, находит его **неактивным** (BaseService
  присутствует) и выдаёт **PASS_WITH_GAPS** — как в T34. Дискриминатор НЕ over-generalized.

**Failure-discriminator** (`P3-FAILURE-DISCRIMINATOR` — сердце P3) — различает BLOCK vs
PASS_WITH_GAPS **на уровне предиката**, 3 состояния одного и того же constraint:

| Состояние constraint | Evidence | LostConstraintCount | Вердикт |
|---|---|---|---|
| **covered** — td06 в наборе, status covered, Gate B есть | `Mentions()=true`, covered | 0 | **PASS** |
| **present-as-concern** — td06 в наборе, status concern | `Mentions()=true`, concern | 0 | **PASS_WITH_GAPS** |
| **entirely-absent** — ни один packet не упоминает BaseService | `Mentions()=false` для всех | ≥1 | **BLOCK** |

Ключевое различие, которое P3 доказывает: «entirely absent» и «present-but-unenforced» выглядят
похоже (constraint не enforced), но семантика РАЗНАЯ — absent = downstream вообще не знает о
constraint (не может enforce то, о чём не знает); concern = знает и явно пометил пробел.

**Learning-cycle** (`P3-LEARNING-CYCLE-TAXONOMY` — «learning-cycle design» из задания). Idle-cycle
после BLOCK производит 3 типизированных артефакта:
- **FeedbackRecord** {FeedbackId, SourceExperimentId, SubExperimentId, BlockedScenarioId,
  **DroppedConstraint**, UpstreamAuthority, DownstreamEvidence, BlockVerdict, LostConstraintCount,
  RejectedPattern, LearningNote} — фиксирует ОДНУ BLOCK-находку, **ключ = конкретный упавший
  предикат**, не общая категория.
- **LearnedDiscriminator** {DiscriminatorId, SourceFeedbackId, **PredicateName**, AbsentPattern,
  PresentButUnenforced, AbsentVerdict=`BLOCK`, PresentButUnenforced=`PASS_WITH_GAPS`,
  **Specificity=«BaseService predicate only»**, **Generalization=«none»**, ActiveFrom}.
- **LearningCycleVerdict** {CycleId, FeedbackRecordsAdded=1, DiscriminatorsAdded=1,
  DiscriminatorSummary, **OverGeneralizationRisk=«none»**}.

**Чего цикл НЕ должен выучить** (жёстко в оракуле): правило, блокирующее любой не-enforced
constraint (это превратило бы каждый T34-concern в BLOCK); правило на cross-phase-context concern;
правило на build-time-sequence сценарий; общее constraint-drop правило на любой предикат.
Дискриминатор **BaseService-специфичен по дизайну** — новые предикаты требуют новых FeedbackRecord.
Триггер цикла: BLOCK сохранён + `LostConstraintCount>0` + в реестре ещё нет дискриминатора для
этого предиката. Специфичность доказывается в P3-V: `Mentions()=true` для td06 → дискриминатор
**consulted но inactive** → PASS_WITH_GAPS, concern-count=2.

**P3 pass-gates (8 шт):** P3-C BLOCK + LostConstraint≥1(BaseService) + cross-phase остаётся concern;
P3-L FeedbackRecordsAdded=1 + DiscriminatorsAdded=1(BaseService-only, no-gen); P3-V PASS_WITH_GAPS +
concern=2/block=0 + DiscriminatorActivated=false.

### ЧАСТЬ 3 — Trained vs deterministic (КОД-ВЕРИФИЦИРОВАНО)

**Вердикт: полностью ДЕТЕРМИНИРОВАННО.** «Learning from failure» — это **скриптованная пьеса в
трёх актах**, а не машинное обучение. Доказательства из живого `P3LearningFromFailureProof.cs`:

1. **`BuildLearningCycle()` (строки 325–361)** — «обучение» = **вызов конструктора с зашитыми
   строковыми литералами**. `LearnedDiscriminator` создаётся как
   `new LearnedDiscriminator("P3-L-DISC-001","P3-L-FB-001","BaseService", …, "BLOCK","PASS_WITH_GAPS","BaseService predicate only","none")`.
   Возвращает **всегда ровно 1 feedback + 1 discriminator**. Ни один вход не влияет на содержимое —
   ответ впечатан в код. `OverGeneralizationRisk="none"` — **литерал**, не измеренная величина.
2. **`MentionsBaseService()` (строки 444–452)** — «предикатный `Mentions()`» на деле =
   `packet.PacketId.Contains("base_service")` / `PromptFragment.Contains("BaseService")` /
   `DomainLabel.Contains("base_service")`. Чистый **keyword-`Contains`**, несмотря на риторику
   отчёта про «22-slot predicate vector».
3. **`discriminatorActivated` (строка 375)** —
   `lostConstraints > 0 && learningCycle.LearnedDiscriminators.Any(d => d.PredicateName == "BaseService")`
   — детерминированный boolean. `lostConstraints` = LINQ `.Count()` по зашитым constraint-строкам
   vs зашитым packets через `.Contains("BaseService")`.
4. **Наборы пакетов зашиты:** `BuildCorruptedDomainPackets()` = 9 хардкод-пакетов (td06 просто
   опущен), `BuildOriginalDomainPackets()` = 10. «Порча» = код, который не добавляет td06.
   `BuildCorruptedScenarios()` возвращает 3 хардкод-сценария (P3-C-1 BLOCK / P3-C-2 concern /
   P3-C-3 pass). Человеческие результаты — `string.Join` зашитых предложений.
5. **Нет реестра, нет персистентности, нет весов/чекпоинтов/градиента/DPO/training-data.**
   «Реестр дискриминаторов», который «консультируется» в P3-V, — это просто `learningCycle`,
   переданный аргументом в том же вызове; между запусками ничего не сохраняется.

**Итог:** P3-C возвращает зашитый BLOCK, P3-L конструирует зашитый discriminator-record, P3-V
возвращает зашитый PASS_WITH_GAPS. Тесты **9/9**, baseline **1005/1005 → 1014/1014**, commit
`8e4134b` → main. Это **демонстрация КОНЦЕПЦИИ дискриминатора и учебного цикла**, а не обученный
дискриминатор. Полностью совпадает с pass 1/2: **`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`**, НЕ
`PASS_AS_TRAINED_FEATURE`.

### Значение для плана продолжения расширений (self-improvement петля)

- **Что здесь реально есть — ДИЗАЙН петли, а не сама петля.** T34+P3 дают **чистую форму** петли
  «learning-from-failure», которая нужна плану «внешняя модель учит внутреннюю»:
  `упавший переход (BLOCK) → FeedbackRecord(dropped-predicate) → LearnedDiscriminator(specific) →
  реестр → консультация на следующем прогоне → неактивен, если предикат присутствует`. Это
  правильный скелет: предикат-специфичные дискриминаторы, явная защита от over-generalization,
  типизированные записи обратной связи.
- **Чего НЕТ — обучения, персистентности, обобщения.** Дискриминатор захардкожен под ОДИН
  предикат (BaseService), реестр не переживает процесс, «обобщения нет» декларативно (`"none"`).
  Реальная петля должна: (а) строить FeedbackRecord/Discriminator ИЗ данных упавшего прогона, а
  не из литералов; (б) персистить их (SVG Round B уже называет нужные сущности:
  `ContextPacketStore`, `TransitionEvidence`, `PhaseLearningGap`, `EventLog` — они спроектированы,
  но не построены — «designed but not fully built»); (в) обобщать на новые предикаты через
  обучение, а не через ручное добавление новых record-ов.
- **Failure taxonomy F1–F6 — переиспользуемый актив.** Для расширений VS/VS Code это готовая
  рубрикация «безопасно ли стартовать следующий шаг» (dropped/unenforced/absent-edge/silent-edge/
  reinstated/superficial) с чёткой границей CONCERN vs BLOCK — ценна как ЛОГИКА арбитра
  качества-перехода, даже если под ней позже встанет обученный юнит вместо `Contains()`.
- **«Next» из отчёта:** P4 (full-lifecycle phase topology, Phase 1→3→4), Gate B closure для
  BaseService (перевести concern → covered), training-pairs для BLOCK-пути (node-07/node-08) —
  всё это ещё не сделано на дату папки.

Теги: `#t34-phase-transition` `#p3-learning-from-failure` `#transition-gate` `#failure-taxonomy-F1-F6`
`#learned-discriminator` `#deterministic-scaffold` `#no-training-lifecycle` `#self-improvement-design`
`#confirms-pass1-pass2` `#PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

### Что осталось для проходов 4–9 (без изменений к плану pass-1/2)

- **Pass 4** — WDA-00/01/02-06: «реальный» суб-фазный router, confusion-таблицы, evidence
  (проверить, детерминирован ли WDA-router так же, как E53-узлы и T34/P3).
- **Pass 5** — ROUND-MANIFEST Phase 25–52 (иерархия юнит/flow/оркестратор/реестры + топология/
  фазы/переходы как мутируемые команды).
- **Pass 6** — ROUND-MANIFEST Phase 53–81 (WDA-набор + инспектор рантайма 60–81, проекты 21–23).
- **Pass 7** — ROUND-MANIFEST Phase 82–118 (CB4 security-flow, Wave-2-center узлы, батчи 002–008).
- **Pass 8** — Wave-1/Wave-2 корпуса + CB4-STAGE evidence (14 json) + CORPUS-COMPLETION-LEDGER v1–v30.
- **Pass 9** — методички/модели/инспектор-дизайны/EXAMINATION-METHOD/EXTERNAL-SOURCE registry +
  реальный код в `round-zips/` (25 .cs) и staging (CB4 .cs).
- **Pass 10** — сплошная зачистка + executive summary + финальный trained-vs-deterministic вердикт.

**Честная строка покрытия pass 3:** прочитаны **целиком** 7 файлов T34-Round-A + T34-Round-B
манифест + 7 файлов P3-Round-A + сводный отчёт T34-AND-P3 (911 строк) + ключевые тела реального
`P3LearningFromFailureProof.cs` (`BuildLearningCycle`, `BuildVerificationRun`, `MentionsBaseService`,
`BuildCorruptedScenarios/HumanResult`) + grep lifecycle/discriminator-терминов по файлу. P3-Round-B
отдельного манифеста в папке НЕТ (glob `*P3*ROUND-B*` = 0), но P3-Round-B КОД существует и
задокументирован в отчёте (`8e4134b`); `T34PhaseTransitionProof.cs` подтверждён существующим,
тело кросс-проверено по отчёту (построчно не читалось — pass 1 уже читал родственный
`T34TransitionDomainSplitterUnit.cs`). Заявляю **только** дизайн T34/P3 + failure-taxonomy +
learning-cycle + код-верифицированный вердикт deterministic для P3; проходы 4–9 остаются как выше.

---

## Pass 4/10 — Phase 25-52: построение иерархии ILogicalUnit→Flow→Orchestrator + editable topology

> **Сверка нумерации прохода.** Внутренний план pass-1/2/3 (строки 726-733 этого файла)
> ставил «Phase 25-52» на pass 5, а WDA — на pass 4. Родитель (parent work order) в ТЕКУЩЕЙ
> инструкции пересобрал очередь и назначил ЭТОТ pass 4/10 именно на **ROUND-MANIFEST Phase 25-52**
> (иерархия юнит/flow/оркестратор + мутируемая топология). Следую текущей инструкции родителя;
> WDA и Phase 53-118 пере-секвенированы в блок «Что осталось» ниже. Это не смена вывода, а
> смена порядка проходов.

### ГЛАВНЫЙ вывод pass 4 (КОД-ВЕРИФИЦИРОВАНО из round-zips)

- **Это САМАЯ реальная, реально собранная часть всей папки.** В отличие от E53/T34/P3 (проходы
  1-3: дизайн + детерминированный оракул-скаффолд), фазы 25-52 — это **28 последовательных
  раундов настоящего C#-кода**: каждый раунд называет конкретные `.cs`-файлы, конкретные тесты,
  фокус-прогон + полный прогон suite, и кладёт `round-zips/round-phase-NN-*.zip` со снимком кода.
  Проверено физически: `round-phase-48-*.zip` содержит `LogicalTopologyRegistry.cs` (24 916 байт)
  и `LogicalTopologyRegistryTests.cs` (27 873 байт); zip'ы 32-52 все на месте.
- **Цепочка тестов монотонна и непрерывна — сильнейшее доказательство «реального кода», а не
  бумаги:** `1014 → 1020 → 1028 → 1033 → 1039 → 1045 → 1051 → 1057 → 1058 → 1065 → 1068 → 1074
  → 1081 → 1085 → 1093 → 1095 → 1101 → 1104 → 1105 → 1114 → 1116 → 1122 → 1125 → 1126 → 1134
  → 1136 → 1142 → 1145 → 1146`. Ни одного разрыва: выход каждого раунда = вход следующего.
- **НО архитектурно это по-прежнему ДЕТЕРМИНИРОВАННЫЙ скаффолд — подтверждает вердикт
  проходов 1-3.** Код-верификация из zip'ов: mutation-units возвращают ЖЁСТКО ЗАШИТУЮ
  `public const double Confidence = 1.0;` с комментарием «deterministic registry mutation as
  fully trusted after validation» — ни обучения, ни `Random`, ни trained-состояния; реестр —
  четыре **in-memory** `Dictionary<string, LogicalXEntry>` (не переживают процесс). То есть:
  **реальный работающий каркас редактируемой топологии, но без обученного интеллекта и без
  персистентности.** Классификация: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (честный необученный
  каркас, а не рабочая обученная фича).
- **Это — тот самый ДИЗАЙН-ИСТОЧНИК `ILearningDynamicGraph` из RAG-05.** «Динамический граф» =
  **editable topology**: во время работы через типизированные команды можно добавлять/править
  4 сущности графа (domain / phase / sub-phase / transition), каждая версионируется в реестре.
  Фазы 25-52 — это phase-by-phase КАК этот редактируемый граф собран.

### Блок A. Фазы 25-31 — базовая иерархия (contract → unit → flow → orchestrator → реестры → runtime-seam)

Строгий порядок зависимостей, «одна фаза = одно новое присутствие», оракулы (T34/E53) не
трогаются, проекты 21-23 в этих фазах не меняются:

| Ф | Что строит (файл) | Суть | Тесты |
|---|---|---|---|
| 25 | `35.ExperimentsV2/LogicalFlow/LogicalContracts.cs` | Контракт `ILogicalUnit<TIn,TOut>` (async с самого начала, `Task.FromResult` для детерминированных) + `LogicalExecutionFlags` (16 флагов: 7 optimization=on, 5 safety=on, 4 experiment-control) + records request/result | 1014→1020 |
| 26 | `T34TransitionDomainSplitterUnit.cs` + `T35LogicalUnitReplacementProof.cs` | **T35 = первая реальная замена**: один `ILogicalUnit` заменяет T34 node-01, точный паритет 10 пакетов (id/domain/coverage/source = 0 расхождений), confidence 0.92 ≥ 0.70; T34 остаётся оракулом | 1020→1028 |
| 27 | `LogicalFlowContracts.cs` + `T34LogicalFlowAdapter.cs` | Контракт `ILogicalFlow<TIn>`; адаптер оборачивает НЕизменный T34 как flow, тот же вердикт/метрики/human-result | 1028→1033 |
| 28 | `LogicalOrchestratorContracts.cs` + `InMemoryLogicalOrchestrator.cs` | Контракт `ILogicalOrchestrator`; выбирает flow по scenario-id из in-memory binding-map, применяет дефолт-флаги, `Fail` (не exception) на неизвестный сценарий | 1033→1039 |
| 29 | `InMemoryLogicalUnitRegistry.cs` | Реестр юнитов: resolve по id/role/domain/version/trust-state; «latest trusted», rejected пропускаются | 1039→1045 |
| 30 | `InMemoryLogicalFlowRegistry.cs` | Реестр flow: resolve по flow-id и scenario-id (убрана хардкод-карта) | 1045→1051 |
| 31 | `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` (+ ref на проект 35) | **Первый runtime-ready шов в продуктовом проекте 15**: вызывает `ILogicalOrchestrator`, возвращает `AgentInfrastructure.OperationResult<T>`, T34-request → `PASS_WITH_GAPS`. Всё ещё experiment-backed/T34-scoped | 1051→1057 |

### Блок B. Фазы 32-52 — editable topology (мутируемый динамический граф)

**Фаза 32 — RELOCATION (ключевая для плана расширений):** контракты
`ILogicalUnit/ILogicalFlow/ILogicalOrchestrator` + все records **перенесены из эксперимент-проекта
35 в рантайм-инфраструктуру `21.AgentInfrastructure/Logical/LogicalContracts.cs`** (namespace
`AgentInfrastructure.Logical`), а эксперимент-owned контракт-файлы **удалены**. Причина — разорвать
цикл зависимостей и дать проектам 21-23 использовать контракты. Тест
`LogicalContracts_LiveInAgentInfrastructureAssembly` это доказывает. → **Для расширений VS/VS Code
контракты логической иерархии живут в проекте 21, а не в экспериментах.**

**Фазы 33-52 — повторяющийся 5-шаговый ПАТТЕРН на каждую из 4 сущностей графа.** Порядок внутри
каждой сущности строго один и тот же (contract-first, потом registry-storage, потом typed-command,
потом mutation-unit, потом flow-branch, потом assistant-route):

```
(1) REGISTRY  → добавить хранение сущности в ILogicalTopologyRegistry (add/update + resolve + list, ревизии)
(2) COMMAND   → добавить typed AddXCommand : LogicalTopologyCommand (дискриминация ТИПОМ C#, НЕ строкой)
(3) UNIT      → добавить AddXTopologyMutationUnit : ILogicalUnit<AddXCommand, LogicalXEntry>
(4) FLOW      → TopologyMutationFlow маршрутизирует AddXCommand → unit (код X_MUTATION_APPLIED)
(5) ASSISTANT → LogicalOrchestratorAssistant.CreateDefault подключает unit; вход RunTopologyCommandAsync
```

Развёртка паттерна по 4 сущностям:

| Сущность | REGISTRY | COMMAND | UNIT | FLOW | ASSISTANT | код успеха |
|---|---|---|---|---|---|---|
| **Domain** | 33 (`LogicalDomain`) | 34 (`AddDomainCommand`) | 35 (`AddDomainTopologyMutationUnit`) | 36 (`TopologyMutationFlow`) | 37 (`RunTopologyCommandAsync`) | `DOMAIN…APPLIED` |
| **Phase** | 38 (`LogicalPhase`) | 39 (`AddPhaseCommand`) | 40 (`AddPhaseTopologyMutationUnit`) | 41 (+ветка) | 42 (wire) | `PHASE_MUTATION_APPLIED` |
| **Sub-Phase** | 43 (`LogicalSubPhase`) | 44 (`AddSubPhaseCommand`) | 45 (`AddSubPhaseTopologyMutationUnit`) | 46 (+ветка) | 47 (wire) | `SUB_PHASE_MUTATION_APPLIED` |
| **Transition** | 48 (`LogicalTransition`) | 49 (`AddTransitionCommand`) | 50 (`AddTransitionTopologyMutationUnit`) | 51 (+ветка) | 52 (wire) | `TRANSITION_MUTATION_APPLIED` |

Тест-дельты этого блока (все зелёные): 32:+1, 33:+7, 34:+3, 35:+6, 36:+7, 37:+4, 38:+8, 39:+2,
40:+6, 41:+3, 42:+1, 43:+9, 44:+2, 45:+6, 46:+3, 47:+1, 48:+8, 49:+2, 50:+6, 51:+3, 52:+1.

### Механизм editable-topology (что именно «динамический граф» умеет к фазе 52)

- **Хранилище графа = `ILogicalTopologyRegistry` (21.AgentInfrastructure.Logical).** Держит 4 типа
  версионированных записей: `LogicalDomainEntry`, `LogicalPhaseEntry`, `LogicalSubPhaseEntry`,
  `LogicalTransitionEntry` — каждый в своём `Dictionary<string, …>` (in-memory). У каждого:
  add-or-update (инкремент revision), resolve, list-по-порядку-ревизий; валидация id/имени/
  назначения/владения-доменом; нормализация predicate-id / source-id / constraint-id.
- **Transition связывает узлы:** `LogicalTransition` = source-kind/source-id → target-kind/target-id
  + purpose. Это и есть рёбра графа поверх узлов domain/phase/sub-phase.
- **Мутация — ТОЛЬКО типизированной командой.** Дискриминация через систему типов C#
  (`AddDomainCommand`/`AddPhaseCommand`/`AddSubPhaseCommand`/`AddTransitionCommand` :
  `LogicalTopologyCommand`), а не через строковое поле-операцию. Матч по runtime-типу, не по
  string-switch.
- **Единый рантайм-вход:** `LogicalOrchestratorAssistant.RunTopologyCommandAsync(LogicalTopologyCommand)`
  → `TopologyMutationFlow` → нужный mutation-unit → registry. Возвращает стабильные метрики
  (stored-id, parent-id, revision, command-type, unit-confidence) в `LogicalFlowResult`.
- **Два раздельных пути ассистента:** старый строковый/NL `RunAsync` (T34-flow) НЕ трогается;
  типизированный `RunTopologyCommandAsync` — отдельно. **NL→команда классификация НЕ построена** и
  явно отложена: манифесты 36/37/52 называют будущий `TopologyIntentClassifierUnit` как отдельный
  логический юнит. То есть человек-текст «добавь фазу X» пока не превращается в команду автоматически.

### Real-code-vs-design (итог pass 4)

- **Real code:** ДА — 28 раундов, конкретные `.cs`, растущие зелёные тесты, физические zip-снимки
  (проверен phase-48). Это не план, а собранный каркас.
- **Trained/learned:** НЕТ — `Confidence = 1.0` константа, детерминированная валидация, никакого
  обучения/DPO/checkpoint.
- **Персистентность:** НЕТ — всё in-memory `Dictionary`, реестры не переживают процесс.
- **Продуктовость:** ЧАСТИЧНАЯ — шов есть в проекте 15, но flow всё ещё T34-experiment-backed
  (`PASS_WITH_GAPS`), не corpus-backed прод; проекты 22/23 не подключены.

### Значение для плана продолжения расширений VS / VS Code

- **Это скелет «редактируемого оркестратора», который расширение и должно показывать/дёргать.**
  Готовые рантайм-контракты в проекте 21 (`AgentInfrastructure.Logical`) + типизированная
  mutation-поверхность (`RunTopologyCommandAsync`) — это ровно тот API, к которому тонкий host-адаптер
  (Claude/Cursor/Codex/VS/VSCode) может слать «добавь domain/phase/sub-phase/transition» и получать
  типизированный результат. Соответствует R6 (простая граница агента: API-вызов → конкретный ответ).
- **Три явные дыры, которые план продолжения обязан закрыть (иначе «динамический граф» не
  продуктовый):** (1) **персистентность** — in-memory `Dictionary` заменить на сохраняемый стор
  (иначе топология исчезает между сессиями расширения); (2) **NL-вход** — построить
  `TopologyIntentClassifierUnit` (сейчас только typed-команды, человек-текст не маршрутизируется);
  (3) **обучаемость** — `Confidence=1.0`-константа и детерминированные mutation-units должны стать
  местом, куда встаёт обученный юнит (это и есть переход от `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` к
  `PASS_AS_TRAINED_FEATURE`).
- **Паттерн «5 шагов на сущность» = готовый рецепт расширения графа.** Любая новая сущность графа
  (напр. «constraint», «gate», «artifact») добавляется тем же контуром registry→command→unit→flow→
  route — это переиспользуемый blueprint для плана.

Теги: `#logical-unit-flow-orchestrator` `#editable-topology` `#topology-mutation` `#ILogicalTopologyRegistry`
`#typed-command-discrimination` `#in-memory-not-persistent` `#deterministic-confidence-1.0`
`#contract-relocation-proj21` `#RunTopologyCommandAsync` `#no-NL-classifier-yet`
`#design-source-of-ILearningDynamicGraph` `#real-code-not-design` `#PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
`#confirms-pass1-pass2-pass3`.

### Что осталось для проходов 5-9 (пере-секвенировано после переназначения pass 4 родителем)

- **Pass 5** — WDA-00/01/02-06 (реальный суб-фазный router, confusion-таблицы, evidence): детерминирован
  ли WDA-router так же, как узлы E53/T34/P3 (перенесено сюда со старого pass-4-слота).
- **Pass 6** — ROUND-MANIFEST Phase 53-81 (WDA-набор развития + инспектор рантайма 60-81, трейсы
  проектов 21-23).
- **Pass 7** — ROUND-MANIFEST Phase 82-118 (CB4 security-flow, Wave-2-center узлы 100-104, батчи 002-008,
  transition-orchestrator-pattern 116-118).
- **Pass 8** — Wave-1/Wave-2 корпуса + CB4-STAGE evidence (14 json) + CORPUS-COMPLETION-LEDGER v1-v30.
- **Pass 9** — методички/модели/инспектор-дизайны/EXAMINATION-METHOD/EXTERNAL-SOURCE registry + реальный
  код в `round-zips/` (25 .cs) и staging (CB4 .cs).
- **Pass 10** — сплошная зачистка + executive summary + финальный trained-vs-deterministic вердикт.

**Честная строка покрытия pass 4:** прочитаны **целиком** `LOGICAL-UNIT-FLOW-ORCHESTRATOR-
IMPLEMENTATION-PHASE-PLAN-v1.md` (398 строк) + **все 28 ROUND-MANIFEST фаз 25-52** (каждый файл
целиком). Код-верификация из round-zips (не только манифесты): подтверждено наличие zip'ов 32-52;
извлечён и проверен листинг `phase-48` (LogicalTopologyRegistry.cs + тесты по размеру); grep по
`phase-50` mutation-unit дал `public const double Confidence = 1.0;` + комментарий «deterministic
registry mutation»; grep по `phase-48` registry дал 4 in-memory `Dictionary<string, LogicalXEntry>`.
Тест-цепочка 1014→1146 сверена по «Baseline/Full suite after edits» в каждом манифесте (непрерывна).
Заявляю **только**: фазы 25-52 = реальный, тестируемый, но детерминированный+in-memory каркас
editable-topology (дизайн-источник ILearningDynamicGraph); НЕ читал построчно тела всех 25 .cs
(проверены точечно 2 файла из zip) и НЕ запускал тесты сам — опираюсь на манифест-цифры + точечную
код-выборку. Проходы 5-9 — как выше.

---

## Pass 5/10 — WDA router + Phase 53-81 (runtime inspector + 21-23 интеграция)

> **Сверка нумерации прохода.** Родитель в ТЕКУЩЕЙ инструкции объединил в ЭТОТ pass 5/10 ДВА блока
> (внутренний план pass-1/2 держал их как pass-5 «WDA» + pass-6 «Phase 53-81»): (1) **WDA-набор
> WDA-00…06 + реальный суб-фазный router** и (2) **ROUND-MANIFEST Phase 53-81** (WDA-фазы + рантайм-
> инспектор 60-79 + интеграция проектов 21/22/23). Следую текущей инструкции родителя; Phase 82-118
> и корпуса пере-секвенированы в «Что осталось» ниже. Это смена порядка, не смена вывода.
>
> Прочитано целиком в этом проходе: **все 4 core-контракта реального router**
> (`WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1`, `…ROUTER-REPLACEMENT-EXPECTED-TRACE-v1`,
> `…ROUTER-REPLACEMENT-EVIDENCE-2026-05-05.json`, `WDA-01-REAL-SOURCE-LEDGER-v1`) +
> `WDA-01-CONFIDENCE-AND-WRONG-DOMAIN-GUARD-CHECKLIST` + `WDA-01-ROUND-B-CONFUSION-TABLE` +
> `WDA-00-SOURCE-PACKET` + `WDA-02-06-HUMAN-RESULT` + **реальный C#-исходник**
> `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\SubPhaseRouterUnit.cs` (381 строка, прочитан
> ЦЕЛИКОМ) + **9 ROUND-MANIFEST** фаз 53/60B/62/64/73/74/75/78/79/81 (целиком). Остальные ~21
> манифест фаз 53-81 охарактеризованы по имени + структурному паттерну (см. таблицу).

### ГЛАВНЫЙ вывод pass 5 (КОД-ВЕРИФИЦИРОВАНО из живого репозитория)

- **WDA («Whole-Development-Area») = набор доказательств оркестрации ВСЕГО жизненного цикла
  разработки** (7 фаз → 31 суб-фаза → 6 переходов), поверх 9-узлового E53-пайплайна. WDA-00
  доказывает топологию, **WDA-01 — суб-фазный роутер** (классификация документа → одна из 31
  суб-фазы), WDA-02…06 — покрытие фаз/переходов/обучения/методологии/полного цикла.
- **Роутер WDA-01 — ДЕТЕРМИНИРОВАННЫЙ scorer совпадений, НЕ обученная модель**, но — важное
  отличие от E53/T34/P3 — это **честный, зависимый-от-входа** детерминизм, а НЕ жёстко-зашитый
  ответ. `SubPhaseRouterUnit` реально читает каталог суб-фаз из `ILogicalTopologyRegistry`,
  токенизирует реальный текст входа и считает overlap-score против реального каталога. Веса
  смеси — константы `0.40/0.40/0.20`; ни обучения, ни весов, ни чекпоинта, ни DPO.
- **Гейты, которые «проходит» роутер, тавтологичны (гарантированы арифметикой, а не обучением)** —
  это ключевая находка pass 5, см. разбор кода ниже: `confidence = 0.70 + score*0.27` ⇒ confidence
  **всегда ≥ 0.70** (порог уверенности не может провалиться); `wrongDomainScore = Math.Min(0.49, …)`
  ⇒ **всегда < 0.50** (wrong-domain-гейт не может сработать); `knowledgeEdge = confidence < 0.70`
  ⇒ **всегда false** (router-knowledge-edge-count конструктивно = 0). То есть evidence-метрики
  «min_confidence ≥ 0.70 / max_wrong_domain < 0.50 / router_knowledge_edge_count = 0» доказывают
  не качество классификации, а **математические кэпы формул**.
- **Рантайм-инспектор (Phase 60-79) — САМЫЙ продуктовый десктоп-артефакт всей папки**: реальное
  WPF-приложение `LlmInspector` + библиотека `LlmInspector.Core` (новые проекты в
  `C:\Projects\llm_mvp_core`), реальный код, реальные тесты (1153→1171), реальные скриншоты, **живая
  интеграция с рантайм-проектами 21/22/23 через tracing-декораторы** (не моки). Наблюдает
  *сгенерированное* поведение станций, не пред-читает топологию.
- **Паттерн детерминированного скаффолда ПРОДОЛЖАЕТСЯ и подтверждён** для всего блока WDA + инспектор.
  Классификация — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, а не `PASS_AS_TRAINED_FEATURE` (совпадает с
  проходами 1-4). **НОВЫЙ след для проходов 6-9:** grep по репозиторию нашёл более поздние реальные
  юниты — `AdaptiveLogicalOrchestrator.cs`, `MultiSubPhaseRouterUnit.cs`, `PromptRoutingInputCompilerUnit.cs`,
  `DefaultLogicalCapabilityTrainer` (+ тесты) — которых нет в манифестах этой папки; ЕСЛИ где-то есть
  настоящее обучение, искать надо там (пометка ниже).

### ЧАСТЬ 1 — WDA: что проектирует и что доказывает

**WDA-00 (topology bootstrap, Phase 54-55).** Представляет весь жизненный цикл как **мутируемую
логическую топологию** (ровно тот editable-graph из pass 4): 7 фаз, 31 суб-фаза, 6 смежных переходов.
Разрешает конфликт «27 vs 31 суб-фаза» в пользу **31 как канонической** (X01/A01). Явно ВНЕ scope:
NL-мутация топологии, классификация документов, оркестрация суб-фаз, валидация переходов, обучение —
всё это U01…U05 «не доказано в WDA-00». Round B — детерминированный тест через типизированную
topology-command-поверхность (из фаз 33-52).

**WDA-01 (sub-phase router, Phase 57-58 деттерм. → Phase 81 реальный юнит).** Задача узкая и точная:
**один скомпилированный документ → ровно одна из 31 суб-фазы**. 31 probe = 31 суб-фаза, идеальное
1:1 (5 суб-фаз в phase-1-ui-ux, 4 в phase-2-functional-spec, 5 в phase-3-technical-design, 5 в
phase-4-implementation, 4 в phase-5-testing, 4 в phase-6-qa, 4 в phase-7-qa-automation).
- **Round B (Phase 58)** — `WDA-01-ROUND-B-CONFUSION-TABLE` дословно: «used by the **deterministic**
  WDA-01 proof», все 31 строк pass, expected == produced. Это детерминированная фикстура.
- **Guard-checklist** несёт вперёд урок E51A (wrong-domain retrieval) и E51B (knowledge-edge
  confidence): каждый probe обязан дать `confidence ≥ 0.70` И `nearest_wrong_sub_phase_score < 0.50`,
  иначе суб-фаза «тонкая» и требует «1 original + 1 phrasing B пары».
- **WDA-02…06 human-result** честно фиксирует разрыв: «The **trained `sub-phase-router` unit is still
  missing**. WDA-01 proved the classification oracle and deterministic shape, but all 31 probes are
  still source knowledge edges». Финальное next-action: «**Implement or train the real
  `sub-phase-router` unit and rerun WDA-01 without the deterministic fixture**».

**Реальный router (Phase 81) — КОД-ВЕРИФИКАЦИЯ `SubPhaseRouterUnit.cs` (прочитан целиком):**
- Контракт `ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>`; `Id =
  "unit-sub-phase-router-v1"`, читает суб-фазы из `ILogicalTopologyRegistry.ListSubPhases()` (не
  хардкодит 31).
- **Механизм = взвешенный overlap-score** (метод `ScoreCandidate`):
  `score = predicateScore*0.40 + identityScore*0.40 + textScore*0.20`, где
  `predicateScore` = доля предикатов кандидата, покрытых входом (Jaccard-подобное пересечение
  нормализованных predicate-id); `identityScore` = доля токенов `SubPhaseId+Name`, покрытых входом;
  `textScore` = доля токенов `SubPhaseId+Name+Purpose`. **Веса 0.40/0.40/0.20 — руками заданные
  константы, не выученные.** Токенизация: split по разделителям, `ToLowerInvariant`, отброс токенов
  ≤2 симв. и стоп-слов (`and/the/phase/sub/source/document/development/owns`).
- **Уверенность и wrong-domain — фиксированная арифметика (тавтологичные гейты):**
  `CalculateConfidence(score) = Round(Min(0.97, 0.70 + score*0.27), 2)` → диапазон **[0.70; 0.97]**,
  порог 0.70 никогда не проваливается; `CalculateWrongDomainScore(s) = Round(Min(0.49, s*0.49), 2)` →
  **всегда < 0.50**; `knowledgeEdge = confidence < 0.70` → **всегда false**. Значит evidence-JSON
  (`min_confidence_gate ">= 0.70"`, `max_wrong_domain_score_gate "< 0.50"`, `router_knowledge_edge_count 0`)
  доказывает КЭПЫ ФОРМУЛ, а не силу классификатора.
- **Один настоящий инженерный плюс (path-firewall):** `SourcePath` НЕ участвует в скоринге
  (`BuildPathFreeScoringText` намеренно исключает путь), но сохраняется как provenance; отдельно
  считается path-only-скор и выдаётся **warning `source_path_semantic_conflict`**, если путь тянет в
  другую суб-фазу — защита от утечки «имя файла → метка» (аналог candidate-order-leakage-guard из CLAUDE.md).
- **Evidence (Phase 81):** `deterministic_fixture_used: false`, 31/31 correct phase, 31/31 correct
  sub-phase, 0 knowledge-edge, тесты focused WDA01 1/1, WDA 7/7, full **1171/1171**. То есть «реальный
  юнит» = детерминированный scorer заменил детерминированную фикстуру; `broad_wave1_seed_ready: true`,
  но `corpus_complete: false` (квоты 3100 train / 620 sub-phase held-out / 60 transition held-out открыты).

**Вывод по router:** это **детерминированный keyword+predicate-overlap классификатор** (BM25/Jaccard-
класс), а НЕ обученная модель. По CLAUDE.md-гейту «Allowed trainable architecture pattern» и R4 (no
hidden determinism) — это допустимый *implementation detail внутри* `ILogicalUnit`, но не trained-фича;
флаг `UseLearnedUnitWhenAvailable=true` выставлен, но обученного юнита нет — работает детерминированный
scorer. Это ровно тот «слот», куда позже должен встать обученный юнит.

### ЧАСТЬ 2 — Phase 53-81: карта пофазного строительства

**Блок 53-59 — WDA-набор (документные раунды + детерминированные прогоны):**

| Ф | Имя | Что строит | Det/Trained |
|---|---|---|---|
| 53 | WHOLE-DEVELOPMENT-EXPERIMENT-SET | doc-раунд: определяет стартовый набор WDA-00…06, фиксирует 31 (не 27) суб-фазу, арифметику 3100/620 | doc |
| 54 | WDA-00-ROUND-A | оракул-артефакты topology bootstrap (source-packet/expected-trace/checklist/executor-contract) | doc-оракул |
| 55 | WDA-00-ROUND-B | детерм. тест topology bootstrap через typed-command-поверхность | **ДЕТЕРМ.** |
| 56 | CORPUS-EXPANSION-PLAN-V4 | план корпуса v4 (следующий за v3; переориентация на суб-фазный seed) | doc |
| 57 | WDA-01-ROUND-A | оракул классификации (selection-ledger, classification-trace, guard-checklist, contract) | doc-оракул |
| 58 | WDA-01-ROUND-B | детерм. proof классификации (confusion-table 31/31 pass) | **ДЕТЕРМ.** |
| 59 | WDA-02-THROUGH-06 | детерм. proof: фазы/переходы/обучение/методология/полный цикл (+dependency-graph.mmd) | **ДЕТЕРМ.** |

**Блок 60-72 — построение рантайм-инспектора (реальный WPF-код, монотонные тесты):**

| Ф | Имя | Что строит (реальные файлы `LlmInspector[.Core]`) | Тесты |
|---|---|---|---|
| 60/60B | INSPECTOR-DESIGN-V3 / V3-FINAL | дизайн: инспектор наблюдает *сгенерированное*, не пред-читает топологию; топология-реестр как вложенный `ILogicalOrchestrator`; станционный I/O — центр UX | 1153 |
| 61 | INSPECTOR-SHELL-SCAFFOLD | только WPF-каркас: окно, resource-dictionaries, command-bar placeholder, пустые панели | — |
| 62 | INSPECTOR-DATA-LAYER | новая либа `LlmInspector.Core`: `PropertyNode`, `StationKind`, `StationTrace`, `PropertyNodeSerialiser`; start/completion делят `TraceId` | 1153→1157 |
| 63 | TRACE-CONTEXT-SCOPE | `TraceContextScope` с async-контекст-изоляцией | +тесты |
| 64 | TRACING-DECORATORS | `TracingLogicalUnit/FlowDecorator/StorageDecorator` + `InspectorSession`; рекурсивный трейс из runtime-событий (orchestrator→flow→unit→storage, 4 id / 8 raw events) | 1160 |
| 65 | STATION-TRACE-TREE | сборка parent/child дерева трейсов (`StationTraceTree`) | — |
| 66 | TRACE-TREE-PANEL | WPF-панель биндинга дерева трейсов | — |
| 67 | PARALLEL-WAVE-RENDERING | `WaveIndex`: параллельные станции на одном уровне, sequential child-waves ниже | — |
| 68 | STATION-INSPECTOR | панель точечного input/output инспектора станции | — |
| 69 | HELIX-TOPOLOGY-DOMAIN | 3D domain-ring сцена (Helix Toolkit), rebuild geometry+edges | — |
| 70 | PHASE-SUBPHASE-NODES | узлы фаз/суб-фаз в сцене | — |
| 71 | PENDING-BUFFERING | буфер для out-of-order прихода phase/sub-phase/transition трейсов | — |
| 72 | COMMAND-BAR | command-bar с реальной поверхностью `LogicalExecutionFlags` | — |

**Блок 73-81 — интеграция проектов 21/22/23 + performance-pivot + корпус/router:**

| Ф | Имя | Что строит | Ключевое |
|---|---|---|---|
| 73 | PROJECT21-TRACE-AND-INSPECTOR-UI | оборачивает РЕАЛЬНЫЕ проект-21 `PermissionGate`→`TracingPermissionGate`, `LearningEventScheduler`→`TracingLearningEventScheduler`; карты станций показывают тип (orchestrator/flow/node/arbiter/storage) + CPU/RAM/memory-pressure; 3D overview | 1165; UI-smoke через UI Automation + скриншоты |
| 74 | INSPECTOR-RESOURCE-GUARD | **стоп-раунд:** 3D-превью нагрузило машину → откат demo-heavy startup, 1 add-domain на старте, сокращение тесселяции/сетки | тесты не пере-гоняли (prev 1165) |
| 75 | INSPECTOR-LIGHTWEIGHT-TOPOLOGY | **разворот на perf-first:** выкинут Helix 3D → лёгкий **WPF Canvas 2D**; STOP-кнопка + time/trace-бюджеты; топология всё ещё из completed `StationTrace`, не из прямого чтения реестра | 1165; smoke 117 MB |
| 76 | INSPECTOR-V4-PLAN-ALIGNMENT | выравнивание на `ORCHESTRATOR-INSPECTOR-DESIGN-v4-performance-first` | — |
| 77 | LIGHTWEIGHT-TOPOLOGY-AUTOMATED-COVERAGE | автоматические тесты лёгкой топологии (замена ручного smoke) | — |
| 78 | PROJECT22-TRACES | детерм. `RunProject22AlgorithmsSampleAsync`: трейсы РЕАЛЬНОГО internal-first пути 22 — `InternalLlmGateway.InspectRouting`, `RouteClassifier.Classify`, `KnnSkillFilter.FilterAsync`, `InternalResultExaminer.Examine`, `ExternalAiRequestPackage/ResultAdapter`; НЕ зовёт внешний AI | 1170; **находка:** строковый duplicate-account-детектор в адаптере считает фразу «without creating a duplicate account» доказательством дубля (хрупкость keyword-матчинга) |
| 79 | PROJECT23-TRACES | детерм. `RunProject23StateMachineSampleAsync`: трейсы проект-23 `AgentStateMachine.RunAsync`, `AgentStateNode`, `ExternalAiStateGuards.ShouldRequestExternalAiApproval`, approval-node; реальный `StateCheckpointer` (SQLite temp) | 1171 |
| 80 | CORPUS-COMPLETION-LEDGER | инкрементальный ледж-журнал готовности корпуса (v-серия) | doc |
| 81 | WDA-01-REAL-ROUTER | `SubPhaseRouterUnit` заменяет детерм.фикстуру (см. Часть 1) | **ДЕТЕРМ.** scorer; 1171/1171 |

### ЧАСТЬ 3 — deterministic-vs-trained (подтверждено)

- **WDA-00…06:** детерминированные proof'ы (сами документы говорят «deterministic … proof»,
  «trained sub-phase-router unit is still missing»). Обучения нет.
- **WDA-01 real router (Phase 81):** реальный, зависимый-от-входа, но полностью **ДЕТЕРМИНИРОВАННЫЙ**
  scorer с тавтологичными гейтами; веса-константы; ни весов/чекпоинта/градиента/DPO. Более «настоящий»,
  чем оракулы E53/T34 (считает по реальному реестру+входу), но не trained.
- **Инспектор 60-79:** реальный WPF-код + реальные тесты (1153→1171) + реальная интеграция с проектами
  21/22/23 через декораторы; **сценарии-семплы детерминированные** (`RunProject2X…Sample` = «deterministic
  local search / deterministic gateway»). Оборачиваемые компоненты (RouteClassifier, KnnSkillFilter,
  duplicate-detector) — те самые детерминированные keyword/KNN-скореры, о хрупкости которых прямо
  зафиксирована находка в Phase 78. Наблюдает сгенерированное, не пред-читает топологию.
- **Итог:** `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` для всего блока WDA+инспектор — паттерн проходов 1-4
  продолжается без разрыва.

### Значение для плана продолжения расширений VS / VS Code

- **`LlmInspector` — готовая «рантайм-observability поверхность», которую расширение и должно
  показывать.** Это десктоп-окно, которое через tracing-декораторы наблюдает живое исполнение
  оркестратора и проектов 21/22/23 (permission-gate, learning-scheduler, internal LLM gateway, route
  classifier, KNN skill filter, state-machine, external-AI approval guard, checkpointer), рисует
  станции с типом + CPU/RAM/memory-pressure. Для VS/VSCode-расширения это ровно «панель трейсов
  оркестрации/скиллов». Ключевой дизайн-инвариант («наблюдать сгенерированное, не пред-читать
  топологию»; топология из completed-трейсов, не из прямого чтения реестра) — переносим в webview/tree-view.
- **Perf-урок 74-75 — прямой сигнал для host-расширения:** 3D Helix «нагрузил машину» → форсированный
  разворот на лёгкий 2D Canvas + STOP-кнопка + time/trace-бюджеты. Расширение в VS/VSCode обязано
  стартовать в bounded-режиме (никаких demo-heavy автозапусков), иначе повторит тот же OOM/CPU-стресс
  (совпадает с CLAUDE.md-гейтом «PNG/OOM/host-safety»).
- **Роутер = слот под обучаемый юнит, а не сам обучаемый юнит.** Для «динамического графа» расширения
  `SubPhaseRouterUnit` даёт правильный контракт `ILogicalUnit<Doc, RoutingDecision>` с confidence/
  wrong-domain/knowledge-edge полями, но их значения — арифметические кэпы. План продолжения должен:
  (1) заменить веса-константы 0.40/0.40/0.20 и формулы confidence на выученные (checkpoint/held-out);
  (2) убрать тавтологичность гейтов (сейчас порог 0.70 и потолок 0.50 гарантированы кодом); (3) построить
  корпус (3100 train / 620+60 held-out — открыт). До этого — честный необученный scorer.
- **NL-вход по-прежнему не построен** (подтверждает pass 4): WDA-00 явно «NL topology mutation вне
  scope», роутер получает *скомпилированный* документ с predicate-id, а не сырой текст пользователя.
  `PromptRoutingInputCompilerUnit` (найден в репо) — вероятный кандидат на этот компилятор; проверить в
  проходах 6-9.
- **СЛЕД для настоящего обучения (проверить в проходах 6-9):** репо-grep вне манифестов этой папки нашёл
  `AdaptiveLogicalOrchestrator.cs`, `MultiSubPhaseRouterUnit.cs`, `PromptRoutingInputCompilerUnit.cs`,
  `DefaultLogicalCapabilityTrainer` (+ `DefaultLogicalCapabilityTrainerTests`, `AdaptiveLogicalOrchestratorTests`).
  Имена намекают на эволюцию к адаптивному/обучаемому оркестратору ПОСЛЕ WDA-01. Если где-то в проекте
  есть реальный training-lifecycle, искать надо ИМЕННО там — это единственный ненулевой шанс найти
  `PASS_AS_TRAINED_FEATURE` вместо scaffold.

Теги: `#wda-whole-development-area` `#sub-phase-router` `#SubPhaseRouterUnit` `#deterministic-scorer`
`#jaccard-predicate-overlap` `#tautological-confidence-gate` `#path-firewall-guard` `#llm-inspector-wpf`
`#tracing-decorators` `#project-21-22-23-integration` `#helix-to-canvas-perf-pivot` `#observe-generated-not-preread`
`#no-training-lifecycle` `#PASS_AS_HONEST_UNTRAINED_SCAFFOLD` `#confirms-pass1-4`
`#lead-AdaptiveLogicalOrchestrator-DefaultLogicalCapabilityTrainer`.

### Что осталось для проходов 6-9 (пере-секвенировано)

- **Pass 6** — ROUND-MANIFEST Phase 82-118 (CB4 security-flow node-01…09, Wave-2-center узлы 100-104,
  батчи 002-008, ext-broadening/cross-source/closure, transition-orchestrator-pattern 116-118); сверить
  `SecurityArbiterUnit.cs` (pass 1: keyword `Contains("pkce")` + `const Confidence=0.94`).
- **Pass 7** — Wave-1/Wave-2 корпуса (pair-sets/ledgers) + CB4-STAGE evidence (14 json) +
  CORPUS-COMPLETION-LEDGER v1-v30 + WAVE-2-CENTER pair-sets.
- **Pass 8** — методички (HOW-TO-TRANSLATE / HOW-TO-STRUCTURE) + модели (LOGICAL-TRANSITION,
  WHOLE-DEV-AREA v1-v4, TEACHING-CYCLE, LEARNING-CYCLE) + инспектор-дизайны v1-v4 + EXAMINATION-METHOD
  v2-v6 + EXTERNAL-SOURCE registry v2-v5 + phase-documentation-audit (5 CSV).
- **Pass 9** — реальный код: `round-zips/` (25 .cs) + staging CB4 .cs **+ более поздние репо-юниты**
  `AdaptiveLogicalOrchestrator` / `MultiSubPhaseRouterUnit` / `PromptRoutingInputCompilerUnit` /
  `DefaultLogicalCapabilityTrainer` (проверить: есть ли ТАМ настоящий training-lifecycle, или тот же
  детерминизм).
- **Pass 10** — сплошная зачистка (ни один файл не пропущен) + executive summary + финальный
  trained-vs-deterministic вердикт по всей папке.

**Честная строка покрытия pass 5:** прочитаны **целиком** 8 WDA-документов (4 core-контракта router +
guard-checklist + confusion-table + WDA-00-source-packet + WDA-02-06-human-result) + **реальный
`SubPhaseRouterUnit.cs` (381 строка) целиком** (арифметика confidence/wrong-domain проверена построчно) +
**9 ROUND-MANIFEST** фаз 53/60B/62/64/73/74/75/78/79/81 целиком. Тест-цепочка инспектора 1153→1157→1160→
1165→1170→1171 сверена по «Baseline/Full» в манифестах (не запускал сам). Остальные ~21 манифест фаз
53-81 (61/63/65-72/76/77/80 и WDA-00/01-round-манифесты 54/55/56/57/58/59) охарактеризованы по имени +
структурному паттерну, БЕЗ построчного чтения (это добор проходов 6/10). Репо-юниты
`AdaptiveLogicalOrchestrator`/`DefaultLogicalCapabilityTrainer`/`MultiSubPhaseRouterUnit`/
`PromptRoutingInputCompilerUnit` найдены grep'ом, но НЕ прочитаны — заявлены как СЛЕД, не как вывод.
Заявляю **только**: WDA-router = детерминированный overlap-scorer с тавтологичными гейтами (код-верифиц.);
инспектор 60-79 = реальный WPF + живая интеграция 21/22/23, детерминированные семплы; паттерн
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` продолжается. Проходы 6-10 — как выше.

---

## Pass 6/10 — Phase 82-118: CB4 security-flow + Wave-2-center + transition-orchestrator (эндпойнт дуги)

> Прочитано **целиком** в этом проходе: **17 ROUND-MANIFEST** фаз 82/83/84/85/93/94/98/99/102/
> 105/114/115/116/117/118 (+ структурно-точечно 86-92/95-97/100-104/106-113 по grep-сверке
> тест-счётчиков и Files-Changed) + `CB4-STAGE-5-SECURITY-FLOW-REGISTRATION-PLAN-v1.md` +
> `CORPUS-BUILD-READY-COMPLETION-SUMMARY-2026-05-05.md` + **два реальных staging-исходника C#
> целиком**: `SecurityArbiterUnit.cs` (155 строк, node-08) и `SecurityFormatterUnit.cs`
> (193 строки, node-09) из `round-phase-85/87-…-staging/code/35.ExperimentsV2/LogicalFlow/`.
> Grep по всем 37 манифестам 82-118 подтвердил прогрессию тест-счётчиков и границу «последний
> код = Phase 99».

### Место фаз 82-118 в дуге (это ФИНАЛЬНЫЙ блок эксперимента 25→118)

Три параллельные линии сходятся в конце:
1. **CB4 (Phase 82-97)** — построить и «обучить» реальный **security-verification-3.4** flow
   (9 узлов) на лицензионно-чистом корпусе OWASP-ASVS.
2. **Wave-2-center + батчи (Phase 98-114)** — довести обучающий корпус до **build-ready** по
   всем 31 суб-фазам жизненного цикла разработки + 6 переходам.
3. **Эндпойнт-разворот (Phase 115-118)** — переписать план обучения: с «поузлового обучения»
   на **prompt-level transition-orchestrator**; это тот же самый v4/v7-пивот, что pass-1 и
   RAG-06 уже зафиксировали как «мысль двинулась от детерминизма к обучаемому оркестратору».

### A. Phase 82-83 — подготовка корпуса CB4 (документ-раунды, кода нет)

- **Phase 82 (Wave-1 quota/held-out/conformance):** определяет **quota-ledger на 31 суб-фазу**,
  форму held-out (**620 суб-фазных + 60 transition** документов) и conformance-проверки, чтобы
  «seed-строки не приняли за готовый корпус». Кода нет; тест-база остаётся **1171/1171** (с Phase 81).
- **Phase 83 (CB4 corpus intake):** принимает лицензионно-чистый CB4-материал OWASP-ASVS:
  **892 pair-instances / 446 base-pairs**, 9 node-ролей, 4 source-family, target суб-фаза **3.4**.
  Явно: «corpus всё ещё не полон; CB4 = материал для ОДНОЙ суб-фазы, не весь v4». WDA 7/7.

### B. Phase 84-97 — CB4 security-verification-3.4 flow (**детерминированный**, поузловая замена + расширение сценариев)

**Ядро этого прохода.** CB4 повторяет ровно тот же паттерн, что E53 (pass 1-2), но на новом
домене (security) и с настоящей регистрацией flow в ассистенте:

- **Phase 84 (Stage 5 registration):** `SecurityVerification34FlowAdapter` регистрируется в
  `LogicalOrchestratorAssistant.CreateDefault` как **второй** string-input flow рядом с T34:
  ```text
  T34-PHASE-3-TO-4-TRANSITION-GATE -> technical-design transition proof
  security-verification-3.4        -> CB4 security-verification proof
  ```
  Дефолтный сценарий ассистента остаётся T34; CB4 запускается только при выборе
  `cb4-security-verification`. **3 оракул-сценария** (источник — OWASP-ASVS-EXECUTOR-CONTRACT):
  **S1→PASS**, **S2→REPROCESS** (PKCE `code_challenge_method=plain` при поддерживаемом S256),
  **S3→CONCERN** (ASVS против внутреннего 30-мин стандарта). Обязательные проверки: атрибуция
  OWASP ASVS 5.0.0 видна; S2 называет `ASVS 10.4.6`/`S256` и **не содержит** запрещённых слов
  `compliant`/`satisfies`; external-AI-calls = **0**. Тест-база **997→1172** (за фазы 25-84
  накопилось ~175 новых тестов инспектора/WDA/CB4). План 84 честно пишет: «proves runtime route
  registration, **not full learning**».
- **Phase 85-93 (поузловая замена):** каждый из 9 узлов заменяется отдельным
  `ILogicalUnit`, **по одному за фазу**, при неизменных оракул-вердиктах S1/S2/S3:

  | Фаза | Узел → класс-юнит | Anchor-пары | Confidence |
  |---|---|---|---|
  | 85 | node-08 arbiter → `SecurityArbiterUnit` | CB4-3.4-001/005/009 | `const 0.94` |
  | 86 | node-07 arbiter-selector → `SecurityArbiterSelectorUnit` | — | const |
  | 87 | node-09 formatter → `SecurityFormatterUnit` | — | `const 0.92` |
  | 88 | node-06 result-translator → `SecurityResultTranslatorUnit` | — | const |
  | 89 | node-05 applier → `SecurityApplierUnit` | — | const |
  | 90 | node-04 processor → `SecurityProcessorUnit` | — | const |
  | 91 | node-03 planner → `SecurityPlannerUnit` | — | const |
  | 92 | node-02 translator → `SecurityTranslatorUnit` | — | const |
  | 93 | node-01 splitter → `SecuritySplitterUnit` | — | const |

  Порядок замены **обратный** (node-08 первым, node-01 последним) — обоснование Phase 85:
  «правильный arbiter-юнит бесполезен, если не выбран правильный arbiter» → сперва selector-цепочка.
  К Phase 93 весь 9-узловой chain = `ILogicalUnit`, S1/S2/S3 стабильны, тест-база **1172/1172**.
- **Phase 94-97 (расширение сценариев, НЕ замена):** flow расширяется с 3 до **6 сценариев**
  через accepted ext-probes, chain из 9 юнитов не трогается:
  - 94 (ext31): **S4→REPROCESS** (RFC 9700 public-client PKCE), **S5→CONCERN** (IP-only
    throttling), **S6→PASS** (per-client-id throttling) → **1173**;
  - 95 (cross-source): комбинированные правила (`node-cross-source-set-ext24`) → **1174**;
  - 96 (ext24 closure) → **1175**; 97 (ext25 pipeline-chain) → **1176**.

**Код-верификация детерминизма (staging .cs прочитаны целиком — паттерн держится до КОНЦА CB4):**
- **`SecurityArbiterUnit.cs`**: `public const double Confidence = 0.94;` (константа компиляции,
  не выученное значение). `Evaluate()` = **keyword-детектор**: `normalized.Contains("code_challenge_method=plain")`
  → REPROCESS(CB4-3.4-005); `Contains("ess-session-001") || Contains("exactly 30 minutes")`
  → CONCERN(CB4-3.4-009); иначе PASS(CB4-3.4-001). Три `Build*Verdict`-метода возвращают
  **жёстко-зашитый** record с готовым текстом/RuleId/сигналами. Единственная «логика» помимо
  keyword-матчинга — flag-гейты (`UseArbiterAuthority`, `RejectArbiterContradictions` обязаны быть true).
- **`SecurityFormatterUnit.cs`**: `const Confidence = 0.92;` + `Format()` = `switch (input.ScenarioId)`
  → `"S2"`/`"S3"`/`_` → три метода `FormatS1/S2/S3`, каждый строит **зашитый StringBuilder-текст**
  с фиксированными строками ASVS. Чистый детерминированный форматтер.

**Вывод по CB4:** это **E53-паттерн на новом домене** — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.
«Pair-backed / trained» = адаптер, чей вывод точно совпадает с оракулом; веса/чекпоинты/
градиент/DPO — отсутствуют, «обучение» = `const Confidence` + keyword-`Contains`. Новизна против
E53: (1) flow **реально зарегистрирован** в продуктовом `LogicalOrchestratorAssistant` (не просто
класс-эксперимент); (2) есть safety-gate на flag'ах (arbiter-authority); (3) сохранена атрибуция
лицензионного источника (OWASP ASVS 5.0.0). Это ценный **скелет security-flow** для расширения,
но не обученная модель.

### C. Phase 98-104 — Wave-2-center (корпус лайфсайкла + единственный *исполняемый* held-out через реальный роутер)

- **Phase 98 (batch-001):** стартует корпус жизненного цикла для **5 центральных суб-фаз**
  (3.1 архитектура, 3.3 API-контракт, 3.4 security, 4.1 backend, 4.3 интеграция). 20 training-
  пакетов, 20 sub-phase-router A/B-строк, 5 held-out. Каждая суб-фаза — 4 строки по стратам
  (A-breadth / B-team-context / C-depth / D-failure-edge). Кода нет; **1176/1176**. Честно:
  «не доказывает held-out через runtime».
- **Phase 99 (held-out conformance — ЕДИНСТВЕННАЯ фаза 82-118 с продуктовым кодом-изменением
  роутера):** пять held-out строк прогоняются через **реальный `SubPhaseRouterUnit`**.
  Тест сразу вскрыл **два уверенно-неверных маршрута** (W2C-HLD-002 → 2.1 вместо 3.3;
  W2C-HLD-004 → 7.4 вместо 4.1). Фикс: развести скоринг на **identity 0.40 / predicate 0.40 /
  general-text 0.20** и добавить `DocumentType` в токен-сет. Гейты: 5/5 phase-id, 5/5 sub-phase-id,
  confidence ≥ 0.70, wrong-domain < 0.50. **1177/1177**, WDA **7→8**. *(Это ровно тот
  детерминированный overlap-scorer с тавтологичными гейтами, что pass 5 разобрал по коду —
  здесь он лишь до-настроен весами-константами; всё ещё не обучение.)*
- **Phase 100-104 (остальные узлы корпуса):** 100 = node-02 translator-пары, 101 = node-03
  planner-пары, 102 = node-04/05/06 chain (по 5 original + 15 Phrasing-B), 103 = node-07/08/09
  review/formatter-пары, 104 = index+summary. **Все — корпус-раунды, кода НЕТ** (создаются только
  `*-PAIR-SET`/`*-LEDGER` .md + zip). Это построение обучающих ПАР, а не обучение.

### D. Phase 105-114 — Wave-2 батчи 002-008 + declared «build-ready complete»

- **105-106:** corpus-v4 completion-audit + `CORPUS-PLAN-V5-CONTINUOUS` (план перехода к
  непрерывному корпусу). Кода нет, **1177/1177**.
- **107-113 (батчи 002-008):** чистые корпус-раунды (кода нет): 002 UI/UX, 003 functional-spec,
  004 technical-remainder, 005 implementation-remainder, 006 testing-QA, 007 QA-automation,
  008 transitions.
- **Phase 114 — `CORPUS-BUILD-READY-COMPLETION-SUMMARY`:** объявляет **build-ready корпус
  завершённым**:
  ```text
  31/31 суб-фаз · 6/6 переходов · 144 training-пакета · 36+6 held-out ·
  558 original pair-rows · 558 Phrasing-B · node-01..09 покрытие полное по всем 31 суб-фазам
  Full suite 1177/1177 · WDA 8/8 · latest code commit 41eaf1f (phase 99)
  ```
  **Ключевой честный дисклеймер (важен для плана продолжения):** `build-ready ≠ production-mature`.
  Production-mature quota = **3 100 / 620 / 60** (train/heldout/transition) — build-ready 558 пар
  = **~18 %** от production-цели. «Runtime learned-unit conformance сильнее всего на node-01;
  узлы 02-09 имеют build-ready пары и остаются КАНДИДАТАМИ на замену обучаемым юнитом» — т.е.
  даже сам summary признаёт: настоящего обучения нет, есть пары под будущее обучение.

### E. Phase 115-118 — ЭНДПОЙНТ: тройной разворот на transition-orchestrator (**план, НЕ код**)

Последние 4 фазы — это переписывание НАПРАВЛЕНИЯ обучения (все «planning artifact change only,
no production code changed»):

- **Phase 115 (learning-plan rewrite):** первый обучаемый юнит = `orchestration-prompt-splitter`;
  первый measured-cycle: `deterministic reference split → learned splitter v1 → held-out →
  one corrective row → learned splitter v2 → resource/quality comparison`. Node-08 security-
  arbiter обучение **понижено** (downstream от orchestration-level). Wave-1 переклассифицирован
  как «deep security-authority corpus», не доказывающий maturity сам по себе.
- **Phase 116 (logical-transition correction):** первый обучаемый юнит переставлен на
  `logical-transition-normalizer`; splitter стал вторым и получает A/B/X transition-объект.
  Модель: `human prompt → A→B с трансформацией X → экспертные ветки A1→B1+X1… → логические
  юниты решают только свой компактный принцип → финал переводит обратно в человеческий язык`.
- **Phase 117 (transition-orchestrator-pattern — ГЛАВНАЯ коррекция):** transition-слой
  перестаёт быть одним детерминированным `ILogicalUnit`-нормализатором и становится
  **недетерминированным `LogicalTransitionOrchestrator : ILogicalOrchestrator`**, который:
  ```text
  выбирает среди transition-flows; генерирует несколько A/B/X кандидатов;
  держит отклонённых кандидатов видимыми; запускает transition-арбитров;
  возвращает выбранный переход ИЛИ запрос уточнения; затем передаёт его в expert-split.
  ```
  Порождает v2/v4/v3/v7 версии моделей (LOGICAL-TRANSITION-MODEL-v2, WHOLE-DEV-AREA-v4,
  TEACHING-CYCLE-v3, LEARNING-CYCLE-v3, CORPUS-EXPANSION-v7). **Next action = создать O1
  teaching-артефакты, ЗАТЕМ «implement or execute the first measured transition-orchestrator
  teaching cycle»** — то есть первый реальный обучающий цикл ещё НЕ выполнен.
- **Phase 118 (canonical external-learning-set-method — ФИНАЛЬНЫЙ файл дуги):** приводит
  external-learning-set документы к каноническому dynamic-flow паттерну; создаёт
  `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` (совпадает с реальным `21.AgentInfrastructure\
  Logical\LogicalContracts.cs`), восстанавливает 9-node pair-структуры, маппит внешние источники
  на **сигнатуры контрактов**, обновляет `SESSION-START-PROMPT-v3`. **Next action всей дуги
  (последняя строка Phase 118): «Produce O1 teaching artifacts using EXAMINATION-METHOD-v6»**
  — transition-orchestrator rows + Node-01..09 rows + teaching-unit rows + held-out/corruption rows.

### Эндпойнт всей дуги 25→118 (ответ на вопрос миссии)

- **Финальный тест-счётчик:** **1177/1177** (WDA 8/8). Последнее изменение **продуктового кода —
  Phase 99** (commit `41eaf1f`, до-настройка `SubPhaseRouterUnit`). **Фазы 100-118 (19 фаз) не
  меняли ни строки продуктового кода** — это построение обучающих пар (Wave-2 батчи) и
  переписывание планов обучения.
- **Всё ещё детерминировано? — ДА, до самого конца.** CB4 security-юниты (85-97) = keyword-
  `Contains` + `const Confidence` (код прочитан). WDA-роутер (99) = overlap-scorer с весами-
  константами. Transition-orchestrator (117) существует **только как документированное
  направление**, кода/чекпоинта/обученного цикла нет. Дуга **заканчивается на ПЛАНЕ произвести
  O1 teaching-строки** для первого measured cycle — который в этой папке так и не исполнен.
- **Итог для плана продолжения расширений VS/VSCode:** **`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`**
  подтверждён по всей дуге 25-118 (согласуется с pass 1-5 и RAG-06/RAG-05). Что готово к переносу
  в «мозг» расширения: (1) **зарегистрированный в ассистенте security-verification-3.4 flow** —
  готовый образец, как оркестратор выбирает и гоняет доменный flow с safety-гейтами и атрибуцией
  источника; (2) **build-ready корпус** 558 пар на 31 суб-фазу × 9 узлов + 6 переходов — готовое
  сырьё под обучение; (3) **финальный целевой контракт** transition-orchestrator (`ILogicalOrchestrator`
  с A/B/X-кандидатами, видимыми rejected, transition-арбитрами). Что ОБЯЗАН добавить план
  продолжения, чего здесь нет: заменить `const Confidence`/keyword-`Contains`/веса-константы
  роутера на **реальный обучаемый юнит** (checkpoint + held-out метрики + ablation + continue-
  training), закрыть **корпусный разрыв 558 → 3 100/620/60**, и **фактически исполнить** O1-цикл,
  который дуга лишь запланировала (Phase 115-118).

Теги: `#phase-82-118-endpoint` `#cb4-security-verification-3.4` `#SecurityArbiterUnit`
`#SecurityFormatterUnit` `#const-confidence-0.94-0.92` `#keyword-contains-detector`
`#owasp-asvs-attribution` `#s1-s6-oracle-scenarios` `#wave-2-center-corpus`
`#SubPhaseRouterUnit-identity-0.40` `#build-ready-not-production-mature` `#558-of-3100-corpus-gap`
`#transition-orchestrator-pattern-plan-only` `#LogicalTransitionOrchestrator` `#O1-teaching-cycle-unexecuted`
`#last-code-phase-99-1177` `#no-code-phase-100-118` `#PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
`#confirms-RAG-06` `#confirms-pass1-5`.

### Что осталось для проходов 7-9 (без изменений к пере-секвенированному плану pass 5)

- **Pass 7** — Wave-1/Wave-2 корпуса (pair-sets/ledgers содержимое) + CB4-STAGE evidence (14 json,
  проверить, что evidence-числа = зашитым оракул-вердиктам) + CORPUS-COMPLETION-LEDGER v1-v30
  (инкрементальный журнал) + WAVE-2-CENTER pair-sets (node-02..09 содержимое пар).
- **Pass 8** — методички (HOW-TO-TRANSLATE / HOW-TO-STRUCTURE) + модели (LOGICAL-TRANSITION v1/v2,
  WHOLE-DEV-AREA v1-v4, TEACHING-CYCLE v1-v3, LEARNING-CYCLE v1-v3, CORPUS-EXPANSION v1-v7) +
  инспектор-дизайны v1-v4 + EXAMINATION-METHOD v2-v6 + EXTERNAL-SOURCE registry v2-v5 +
  phase-documentation-audit (5 CSV).
- **Pass 9** — реальный код: `round-zips/` (25 .cs контракты/реестры/оркестратор) + оставшиеся
  CB4 staging .cs (84 adapter, 86 selector) **+ более поздние репо-юниты**
  `AdaptiveLogicalOrchestrator` / `MultiSubPhaseRouterUnit` / `PromptRoutingInputCompilerUnit` /
  `DefaultLogicalCapabilityTrainer` — единственный ненулевой шанс найти настоящий training-lifecycle.
- **Pass 10** — сплошная зачистка + executive summary + финальный trained-vs-deterministic вердикт.

**Честная строка покрытия pass 6:** прочитаны **целиком** 17 ROUND-MANIFEST фаз (82/83/84/85/93/
94/98/99/102/105/114/115/116/117/118 + CB4-STAGE-5-registration-plan + CORPUS-BUILD-READY-summary) +
**2 реальных staging C#-файла целиком** (`SecurityArbiterUnit.cs`, `SecurityFormatterUnit.cs`) +
grep-сверка тест-счётчиков и Files-Changed по остальным ~20 манифестам 82-118 (86-92/95-97/100-101/
103-104/106-113 — охарактеризованы по имени + структурному паттерну + подтверждённой прогрессии
теста, БЕЗ построчного чтения тел). Staging-исходники узлов 01-07 (splitter/translator/planner/
processor/applier/result-translator/selector) НЕ прочитаны построчно — их детерминизм выведен из
идентичного паттерна двух прочитанных юнитов + манифестов «only node-N replaced, S1/S2/S3 stable,
confidence ≥ 0.70»; это добор pass 9. Заявляю **только**: CB4 = детерминированный security-flow
(код-верифиц. по node-08/node-09), реально зарегистрирован в ассистенте; Wave-2 = build-ready
корпус 558 пар (18 % от production-quota); эндпойнт Phase 118 = ПЛАН transition-orchestrator +
O1-цикл, не исполненный; финал кода = 1177/1177 на Phase 99; паттерн
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` держится до Phase 118.


---

## Pass 7/10 — Wave корпуса + CB4-STAGE evidence + CORPUS-COMPLETION-LEDGER (реальный обучающий корпус?)

**Роль прохода:** это КОРПУСНЫЙ + evidence-слой (no-neglect). Проходы 1-6 разобрали код/эксперименты
(E53/T34/P3/WDA/CB4/inspector) и вынесли вердикт «детерминированный scaffold, обучение не запускалось».
Pass 7 отвечает на один вопрос: **существует ли здесь реальный обучаемый корпус оркестрации — то есть
настоящие пары node A→B (вход юнита → типизированный `LogicalUnitResult`), как требует план, — или это
descriptive-template scaffold, который видели в RAG-06?**

**Что прочитано целиком в этом проходе (честная строка покрытия pass 7):**
- Пары-файлы (build-ready корпус): `WAVE-2-BATCH-002-UIUX-FULL-PATH-PAIR-SET-v1.md` (целиком, 152 стр.),
  `WAVE-2-BATCH-003-FUNCTIONAL-SPEC-...` (секция node-02), `WAVE-2-BATCH-008-TRANSITIONS-TRANSITION-PAIR-SET-v1.md`
  (целиком, 47 стр.).
- Центр-фаза (реальные пары): `WAVE-2-CENTER-PHASE-BATCH-001-NODE-02-DOMAIN-TRANSLATOR-SET-v1.md` (целиком,
  119 стр.), `...-NODE-03-SCHEME-PLANNER-SET-v1.md` (pair index целиком).
- Accounting: `CORPUS-BUILD-READY-COMPLETION-SUMMARY-2026-05-05.md` (целиком), `WAVE-1-QUOTA-LEDGER-v2.md`
  (целиком, 31-строчная per-sub-phase таблица), `CORPUS-V4-FINAL-COMPLETION-AUDIT-2026-05-05.md` (целиком),
  `CORPUS-COMPLETION-LEDGER-v30.md` (целиком, последний из 30).
- CB4 evidence: `CB4-STAGE-5-...-EVIDENCE.json` и `CB4-STAGE-5A-...-EVIDENCE.json` целиком + grep-сводка по
  всем 14 CB4-STAGE json (replaced_station / confidence / external_ai / full_suite).
- Runtime: `WAVE-2-CENTER-PHASE-BATCH-001-HELDOUT-CONFORMANCE-EVIDENCE-2026-05-05.json` (целиком, Phase 99).
- Grep-сверка: target-quota в `CORPUS-EXPANSION-PLAN-v5.md`; поиск teaching/training-run следов (epoch/loss/
  reward_margin/weights-published/checkpoint) по всей папке (0 попаданий).
- НЕ прочитаны построчно тела остальных ~4 batch pair-set (004/005/006/007) и центр-node-04-05-06 / 07-08-09
  set — их характер выведен из идентичного структурного паттерна двух прочитанных представителей каждого класса
  (batch = template, center = hand-crafted) + accounting-таблиц; это добор pass 9.

---

### ГЛАВНЫЙ ВЫВОД pass 7 — корпус ДВУХСЛОЙНЫЙ, и это критично

В папке НЕ один корпус, а **три разных источника пар с разным качеством**, которые accounting-документы
местами суммируют вместе и этим создают ложную картину «build-ready complete»:

| Слой | Что это | Объём | Реальность пар | Источник |
|---|---|---|---|---|
| **A. Center Phase Batch 001** | ручные пары node A→B | 75 original-строк (+75 Phrasing B) | **РЕАЛЬНЫЕ** — настоящий `LogicalUnitResult` с доменным знанием | написаны вручную |
| **B. Wave-2 Batch 002-008** | descriptive-template scaffold | 483 original-строки (+483 Phrasing B) | **SCAFFOLD** — заполнение шаблона, утёкший placeholder | автогенерация |
| **C. CB4 (security-3.4)** | source-backed детерминированные пары | 892 instance / 446 base pair | **РЕАЛЬНЫЙ источник, но детерминированный** — не обучаемый | OWASP/NIST/CWE/IETF |

Ключ: build-ready «558 пар» = слой A (75) + слой B (483). То есть **~87 % «готового» корпуса — это
template-scaffold**, и лишь ~13 % (центр-фаза) — настоящие пары. CB4 (892/446) считается ОТДЕЛЬНО и
покрывает только 1 из 31 суб-фазы (3.4).

---

### Слой B — Wave-2 Batch 002-008: descriptive-template scaffold (подтверждает RAG-06)

Пары batch-файлов — это НЕ пары «вход юнита → выход юнита», а заполнение одного шаблона по каждой суб-фазе.
Все строки одной суб-фазы идентичны (меняются только суффикс A/B/C/D и список предикатов), confidence
захардкожен `0.82` во всех строках, coverage_status у всех `source_gap`. Смоки-ган — **утёкший
незарезолвленный placeholder** `$(System.Collections.Hashtable.Gap)` (это `ToString()` .NET-Hashtable,
попавший прямо в «обучающий» текст).

Цитата 1 (`WAVE-2-BATCH-002-UIUX-FULL-PATH-PAIR-SET-v1.md`, node-02, строка `N02-W2B002-1.1-A-001`):
> "The Design system and tokens packet activates DECLARE, VALIDATE_OP, RETURN. The required behavior is to
> preserve the Design system and tokens responsibility and keep the source condition
> $(System.Collections.Hashtable.Gap) visible."

В `WAVE-2-BATCH-003-FUNCTIONAL-SPEC` тот же шаблон дословно, подставлено только имя суб-фазы:
> "The User stories and acceptance criteria packet activates DECLARE, VALIDATE_OP, RETURN. The required
> behavior is to preserve the User stories and acceptance criteria responsibility and keep the source
> condition $(System.Collections.Hashtable.Gap) visible."

Node-08 arbiter в тех же batch-файлах: `"Pass when covered evidence is preserved. Concern when
$(System.Collections.Hashtable.Gap) is present."` — тот же утёкший placeholder в роли «вердикта арбитра».
Внизу каждого файла — правило `Phrasing B Rule`: companion получается «заменой заголовка суб-фазы на
эквивалентную формулировку при сохранении того же B-выхода» → это подтверждает, что B-выход — шаблон, а не
доменный ответ.

**Вердикт слоя B:** это ровно тот descriptive-template scaffold, о котором предупреждал RAG-06. Как
обучающие пары A→B они непригодны (B не является конкретным `LogicalUnitResult`, placeholder не
зарезолвлен, нет дифференциации).

---

### Слой A — Center Phase Batch 001: РЕАЛЬНЫЕ пары (и корпус сам это осознаёт)

Центр-фаза устроена принципиально иначе. Заголовок файла прямо задаёт контракт:
> "Each A below is one domain packet. Each B is the actual translated statement, not a description of what a
> translated statement should contain."

B здесь — конкретный `LogicalUnitResult<DomainTranslation>` с дифференцированным доменным содержанием,
дифференцированным confidence (0.86-0.94) и дифференцированным coverage_status (covered/boundary/conflict).

Цитата 2 (`WAVE-2-CENTER-PHASE-BATCH-001-NODE-02-DOMAIN-TRANSLATOR-SET-v1.md`, `W2C-N02-009`, coverage=conflict):
> "Public clients require PKCE. A missing code_challenge causes reprocess, and backward compatibility is not
> a valid security exception for the current requirement."

Цитата 3 — самая показательная (`W2C-N02-008`, coverage=conflict). Корпус САМ, в реальном ряду, определяет
scaffold-форму слоя B как невалидную:
> "A valid training pair must contain the exact unit input and exact logical result output. A descriptive B
> output is malformed and must be rejected before it enters training."

Node-03 scheme-planner центра — тоже реальный: настоящие упорядоченные шаги, dependency_edges и
blocked_paths (например `plan-3.4-security-authorization`: "s01 enforce public-client PKCE as reprocess path;
… s04 reject future-release exception for current PKCE MUST"). Внизу — `Gate Result: 20 node-02 original
pairs PASS / 20 Phrasing B companions PASS / Actual formal B statements PASS`.

**Внутреннее противоречие корпуса (важно для плана):** «реальный» центр-ряд `W2C-N02-008` объявляет форму
batch-пар (слой B) «malformed and must be rejected before it enters training», но accounting суммирует и
центр, и batch в одну цифру «558 build-ready complete». То есть корпус официально засчитывает как готовые те
самые строки, которые его же эталон запрещает пускать в обучение.

Слой транзиций (Batch 008, 18 строк) — промежуточный: шаблонный, но осмысленный (реальная phase→phase
семантика), пример `PC-T03`: "Phase context summary carries architecture decisions, API contracts, security
constraints from phase-3-technical-design to phase-4-implementation, with rejected and missing items
preserved as predicate-level context." Placeholder-утечки нет; но это по-прежнему заполнение шаблона по 6
переходам, не реальные документные пары.

---

### Слой C — CB4 (security-verification-3.4): реальный источник, но детерминированный движок

CB4 — самый ценный по ИСТОЧНИКУ слой, но по механике это детерминированный flow, не обучение.

Факты из `CB4-STAGE-5-...-EVIDENCE.json` (регистрация flow, Phase 84):
- Источники: `OWASP ASVS 5.0.0, NIST SSDF, CWE Top 25, IETF RFCs` — реальные внешние authority-стандарты
  (не 446 отдельных документов, а 4 семейства стандартов, транслированные в пары).
- Объём: `pair_instances_available: 892`, `base_pairs_available: 446`, `100 original training + 20 held-out
  + 784 candidate`.
- Режим: `stage_5_mode: "deterministic flow adapter first; learned station replacement later"`.
- Зарегистрирован реально: `registration_surface: LogicalOrchestratorAssistant.CreateDefault`, `status:
  registered`.
- Оракул фиксированный: 3 сценария S1 PASS / S2 REPROCESS (PKCE plain при поддержке S256) / S3 CONCERN
  (ASVS 7.3.1 vs внутренний стандарт). `external_ai_call_count: 0` во всех сценариях.

Прогрессия 14 CB4-STAGE evidence json (grep-сводка) — каждая стадия заменяет ОДНУ станцию на `ILogicalUnit`,
сохраняя тот же S1/S2/S3 оракул, с **захардкоженным confidence-константой (не обученным распределением)**:

| Stage | replaced_station | confidence | external AI | full_suite |
|---|---|---|---|---|
| 5A | node-08 arbiter | 0.94 | 0 | 1172/1172 |
| 5B | node-07 arbiter selector | 0.93 | 0 | 1172/1172 |
| 5C | node-09 formatter | 0.92 | 0 | 1172/1172 |
| 5D | node-06 result translator | 0.92 | 0 | 1172/1172 |
| 5E | node-05 applier | 0.91 | 0 | 1172/1172 |
| 5F | node-04 processor | 0.90 | 0 | 1172/1172 |
| 5G | node-03 planner | 0.89 | 0 | 1172/1172 |
| 5H | node-02 domain translator | 0.88 | 0 | 1172/1172 |
| 5I | node-01 domain splitter | 0.87 | 0 | 1172/1172 |
| 5J-5M | EXT31 broadening / cross-source / EXT24 closure / EXT25 pipeline-chain | — | 0 | 1173→1176 |

Confidence монотонно убывает 0.94→0.87 по ходу замены узлов — это **ручные константы, назначенные по узлам,
а не выученные веса**. Все 9 узлов заменены детерминированными pair-backed юнитами; оракул неизменен; внешних
AI-вызовов ноль; тесты растут 1172→1176.

**Вердикт слоя C:** CB4 — единственный source-backed корпус с реальным доменным содержанием (OWASP ASVS),
и он реально прогнан через 9-узловой pipeline, зарегистрированный в ассистенте. Но это `PASS_AS_HONEST_
UNTRAINED_SCAFFOLD`: детерминированные юниты с ручным confidence-полом ≥ 0.70, фиксированный оракул, ноль
обучения. Совпадает с вердиктом CB4 из pass 6 (код-верификация node-08/node-09).

---

### Corpus completion accounting — две несводимые системы учёта

**Система 1 — build-ready (`CORPUS-BUILD-READY-COMPLETION-SUMMARY`)** считает template-строки по узлам 01-09
через 31 суб-фазу:
```
development sub-phases represented: 31 / 31
adjacent lifecycle transitions represented: 6 / 6
training document packets: 144
sub-phase held-out packets: 36 ; transition held-out packets: 6
original learning pair rows: 558 ; Phrasing B companions: 558
Build-Ready orchestration corpus complete: YES
Production-mature corpus complete: NO
```
Разбивка 558 по батчам: Batch 001 (center) = 75; 002 UIUX = 75; 003 func-spec = 60; 004 tech = 75; 005 impl
= 75; 006 testing/QA = 120; 007 QA-auto = 60; 008 transitions = 18. → реальных (center) 75, scaffold 483.

**Система 2 — source-backed (`WAVE-1-QUOTA-LEDGER-v2`)** считает только принятые пары из реальных источников,
и картина совсем другая. Из 31 суб-фазы **только 3.4-security имеет ненулевые принятые пары**:
```
3.4-security-and-authorization-model: accepted_pair_instances 892 ; base_pairs 446 ; held_out 20
все остальные 30 суб-фаз: accepted_pair_instances 0, base_pairs 0 ("not started" / "planned center-phase batch")
promoted source docs: 0  (для всех 31)
```

**Целевая квота (production-mature), `CORPUS-EXPANSION-PLAN-v5`:**
```
31 sub-phases × 100 training doc packets = 3,100 training document packets
31 sub-phases × 20 held-out = 620 held-out document packets
+ 60 transition held-out
```
→ 558 / 3,100 ≈ **18 %** от target (совпадает с числом из pass 1-6). Но это 18 % ПО template-строкам; по
source-backed парам реально закрыта 1 суб-фаза из 31 (только CB4).

---

### CORPUS-V4-FINAL-COMPLETION-AUDIT — честный контр-вердикт самого корпуса

`CORPUS-V4-FINAL-COMPLETION-AUDIT` прогоняет 10 условий финального объявления из v4 и выносит:
> "Full corpus v4 complete: NO"

Из 10 условий: **1 PASS** (WDA 8/8), **8 FAIL**, **1 PARTIAL**. Блокирующие материальные разрывы (цитата):
```
3,080 training document packets still missing
615 sub-phase held-out document packets still missing
60 transition held-out document packets still missing
node-02 through node-09 runtime learned-unit conformance still missing
31 active sub-phase flows not registered
phase-context compiler / transition-gate / topology-intent-classifier not complete
probationary regression suspension not proven
```
И «completion boundary» — прямое признание, что расширение слоя B (те самые 483 scaffold-строки) синтетично:
> "Further automatic file generation without additional reviewed source packets would create synthetic or
> repetitive rows that do not satisfy the intent of the v4 source authority rule. … cannot honestly be
> finished in the current state by generating more files from the same small Batch 001 source set."

То есть audit самого корпуса прямо называет автогенерацию слоя B «synthetic or repetitive rows». Это
внутреннее подтверждение scaffold-вывода.

---

### Runtime-конформанс — только node-01, и это ручной детерминированный скорер

Единственный runtime-evidence по центр-корпусу — `WAVE-2-CENTER-...-HELDOUT-CONFORMANCE-EVIDENCE` (Phase 99):
- `runtime_unit: AgentInfrastructure.Logical.SubPhaseRouterUnit` — это ТОЛЬКО node-01 (router). Узлы 02-09
  runtime-конформанса не имеют (подтверждает V4-audit FAIL #5).
- 5 held-out строк, 5/5 совпадений фазы и суб-фазы; confidence_floor ≥ 0.70; wrong_domain_ceiling < 0.50.
- **Смоки-ган «не обучение, а ручной тюнинг формулы»:** после двух промахов (`W2C-HLD-002` ушёл в 2.1,
  `W2C-HLD-004` ушёл в 7.4) «фикс» — переписать веса детерминированной формулы скоринга:
  `old_scoring: "predicate 0.55 + text 0.45"` → `new_scoring: "predicate 0.40 + identity 0.40 + general
  text 0.20"` + добавить входной токен `DocumentType`. Это ручная правка коэффициентов, а не тренировка.
- Итог: `full_suite 1177/1177` (это и есть эндпойнт-цифра из pass 1-6), verdict PASS.

---

### Teaching / training run — НЕ ЗАПУСКАЛСЯ (подтверждено)

Grep по всей папке (epoch / loss / reward_margin / weights published / checkpoint saved / accuracy_before/
after) — **0 попаданий**. Последний из 30 леджеров, `CORPUS-COMPLETION-LEDGER-v30.md`, сам говорит, что
обучение — всё ещё будущее действие:
> "The next action is O1 teaching artifact production using v6: 1. transition orchestrator rows for O1 …
> 4. teaching-unit rows for the first measured learning run."

«first measured learning run» — в будущем времени. Подтверждает вердикт pass 1-6: teaching-цикл ни разу не
исполнялся; веса не публиковались; чекпойнтов обучения нет.

---

### РЕАЛЬНО-ОБУЧАЕМЫЙ-КОРПУС — вердикт pass 7

**Существует ли здесь реальный трейнабл-корпус оркестрации? — Частично как СЫРЬЁ, нет как обученный актив.**

1. **Реальные пары A→B есть, но их мало и они не обучены:** только Center Phase Batch 001 (75 строк) —
   настоящие `LogicalUnitResult`-пары с доменным знанием; корпус сам объявляет их эталоном и отвергает
   scaffold-форму. Это качественный seed для DPO, но 75 строк на 5 суб-фаз, не прогнанные через обучение.
2. **Основная масса build-ready (483/558 ≈ 87 %) — descriptive-template scaffold** с утёкшим placeholder
   `$(System.Collections.Hashtable.Gap)`, уравненным confidence 0.82; как обучающие пары непригодны без
   переписывания в реальный `LogicalUnitResult`.
3. **Единственный source-backed корпус (CB4, 892/446, OWASP/NIST/CWE/IETF) реален по источнику, но
   детерминирован** — ручной confidence, фиксированный оракул, 0 обучения; покрывает 1 суб-фазу из 31.
4. **Обучение не запускалось** — ни teaching-цикла, ни весов, ни чекпойнтов; последний леджер держит
   «first measured learning run» как будущее.
5. **Собственный audit корпуса ставит «v4 complete: NO»** и называет автогенерацию слоя B «synthetic or
   repetitive rows» — то есть «558 build-ready complete» — это учёт scaffold-строк, а не доказанного
   трейнабл-корпуса.

Итог для плана продолжения расширений: **реального обученного корпуса оркестрации здесь НЕТ**; есть (а)
маленький качественный seed реальных пар (центр-фаза, 75), (б) один реальный source-backed детерминированный
корпус (CB4, 3.4), и (в) большой scaffold-балласт (483), который нельзя засчитывать как обучающие данные. Всё
это держит паттерн `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` из проходов 1-6 — на уровне корпуса он подтверждён, не
опровергнут.

---

### Что осталось для проходов 8-9 (по этой папке)

- **Pass 8** — оставшиеся planы/методология: `CORPUS-EXPANSION-PLAN v1-v7` эволюция (как менялась квота и
  правило source-authority), `EXAMINATION-METHOD-ORCHESTRATION v2-v6`, `LEARNING-CYCLE-IMPLEMENTATION-PLAN
  v1-v3`, `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN v2-v5`, `EXTERNAL-SOURCE-REGISTRY v2-v5`,
  `HOW-TO-STRUCTURE-A-LEARNING-SET-*` / `HOW-TO-TRANSLATE-A-DOCUMENT-*` — как задумано строить настоящий
  трейнабл-набор (метод, ещё не исполненный).
- **Pass 8/9** — построчная сверка непрочитанных pair-set (batch 004/005/006/007 tails, center node-04-05-06
  / 07-08-09 sets, sub-phase-router set) — подтвердить, что паттерн «batch=template, center=hand-crafted»
  держится на всех, без исключений (сейчас выведено по 2 представителям класса).
- **Pass 9** — CORPUS-COMPLETION-LEDGER v1-v29 (эволюция учёта, где именно scaffold начал засчитываться как
  build-ready), WAVE-1 gap-analysis / source-authority-coverage-audit / conformance-suite, WDA-01 real-router
  contract, и `orchestrator docs.zip` / round-zips (если нужно распаковать).
- **Pass 10** — сплошная зачистка + executive summary + финальный trained-vs-deterministic вердикт для плана
  продолжения расширений VS / VS Code.

**Честная строка покрытия pass 7:** прочитаны целиком 2 представителя pair-класса (UIUX-batch = template,
center-node-02 = hand-crafted) + transitions-set + node-03-center pair-index + 4 accounting-документа
(build-ready summary, WAVE-1 quota ledger, V4 final audit, ledger v30) + 2 CB4 json целиком + 14 CB4 json
через grep-сводку + Phase-99 held-out json целиком + grep-сверка target-quota и отсутствия teaching-следов.
НЕ прочитаны построчно тела batch 004/005/006/007 и center node-04..09 set — их характер выведен из
идентичного класс-паттерна (добор pass 9). Заявляю ТОЛЬКО: корпус двухслойный — 75 реальных center-пар + 483
template-scaffold (build-ready 558, 18 % от 3,100) + отдельный source-backed детерминированный CB4 (892/446,
только 3.4); обучение не запускалось; собственный audit = «v4 complete: NO»; реального обученного
трейнабл-корпуса оркестрации НЕТ.

---

## Pass 8/10 — методология: corpus-expansion + examination-method + domain-mapping + phase-doc-audit

> Прочитано целиком в этом проходе: `HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT-v1` (441 стр.),
> `HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1` (421 стр.),
> `DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1` (808 стр.),
> `EXAMINATION-METHOD-ORCHESTRATION-v6` (574 стр.),
> `CORPUS-EXPANSION-PLAN-v1` (621 стр.) целиком + v2/v3/v4/v5/v6 (шапки+core-correction) + v7 (149 стр.) целиком,
> `SKILL-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1.0`, `GUIDANCE-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1.0`,
> `PLANNING-DOCUMENT-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1.0`, `ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0`
> (4 domain-mapping плана целиком), `PHASE-DOCUMENTATION-AUDIT-REPORT-v1`, `PHASE-REGISTRY-AND-COVERAGE-MATRIX-v1`,
> `PHASE-TRANSITION-EXPERIMENT-MATRIX-v1`, `NEXT-ACTION-ITEMS-FOR-MULTI-PHASE-ORCHESTRATION-v1` (аудит целиком),
> шапки `EXTERNAL-SOURCE-REGISTRY-v5` + `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v5`.

### Место pass 8 в дуге (это СЛОЙ МЕТОДА)

Проходы 1–7 разобрали, что **реально построено и исполнено** (E53 D–K, T34/P3, WDA, CB4, Wave-корпуса) —
и доказали, что всё это **детерминированный scaffold** (обучение ни разу не запускалось). Pass 8 разбирает
**слой замысла**: методички «как документ становится обучающим набором → парами → обученным юнитом», квоты
корпуса и их source-mapping, аудит покрытия документации по фазам, и эксперименты доменного маппинга
(guidance/skill/planning/artifact-examination). **Ключевой вывод pass 8: методология зрелая, полная и
связная — но она не исполнена. Разрыв не в методе, а в исполнении.** Планировалось 3 100 обучающих
документов; построено ~144 пакета. Спроектированы 5 экспериментов доменного маппинга; исполнены 0 (только
Round A дизайн-документы). Спроектирован teaching-цикл (parse→features→train→heldout→correct); «первый
измеренный прогон обучения» стоит в будущем времени во всех леджерах.

---

### ЧАСТЬ 1 — Задуманный тренировочный пайплайн (документ → эксперимент → learning-set → обученный юнит)

Метод — это **связка из 4 методичек** (все 2026-05-04/05), образующих один конвейер:

**1. `DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1` — что вообще учим.**
Определяет **7 фаз × 31 суб-фазу** жизненного цикла разработки (Design 1.1–1.5, Functional Spec 2.1–2.4,
Technical Design 3.1–3.5, Implementation 4.1–4.5, Testing 5.1–5.4, QA 6.1–6.4, QA-Automation 7.1–7.4). Для
каждой суб-фазы: вопрос, который она отвечает, + «primary signals» (словарь-детектор). Метод классификации
документа — **2 вопроса** (какую проблему решает / какой словарь использует) + **4 правила** (специфичность
= глубина; один документ = одна фаза; implementation-план принадлежит Phase 4, а не фазе, которую он цитирует;
отсутствие документа — само по себе сигнал = knowledge edge). **Experiment Readiness Check (3 условия):**
однозначная фаза + ≥3 проверяемых claim + ≥1 known failure mode. **Transition Readiness Check:** обе фазы имеют
≥1 документ и upstream делает решения, которые downstream обязан признать; upstream context packet (≥3 записи
по 5 категориям) строится вручную ДО эксперимента.

**2. `HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT-v1` — документ → эксперимент (6 шагов).**
Central Question обязана иметь позитивную И негативную половину (что документ claims делать корректно / как
он может молча упасть). Шаги: (1) прочитать документ как набор **claim** 5 типов — enforcement / binding /
coverage / sequencing / honesty; (2) для каждого claim найти **failure mode** («что должно быть истинным,
чтобы claim выглядел выполненным, но по сути упал»); (3) выбрать **ровно 3 сценария** (по одному на самый
«тихий» failure mode, с известным правильным ответом ЗАРАНЕЕ); (4) написать **5 оракул-документов В ПОРЯДКЕ**:
① Examination Domain Map (словарь формальных доменных утверждений), ② Binding Failure Taxonomy (типы сбоев +
вердикт арбитра), ③ Expected Orchestration Trace (оракул: ожидаемый выход на каждой node-границе; самая
критичная секция — scheme planner / граф зависимостей), ④ Human Result Standards (что formatter обязан
выдать), ⑤ Executor Contract (что именно делает Round B); (5) Training Gap Map (по узлу: existing pair / new
pair / human proxy); (6) Pass Gates + Failure Classification. **Единственное нерушимое правило: оракул пишется
ДО прогона** («если оракул легко написать — эксперимент недостаточно труден»).

**3. `HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1` — результаты эксперимента → обучающие пары (3
действия строго по порядку).** ① **Correction pass** — применить все CONCERN-находки к существующим парам
(текст меняется, id/A/node/лицензия неизменны; REPROCESS-пары НЕ правятся — удаляются и переписываются в ②).
② **Gap closure pass** — писать новые пары ТОЛЬКО для узлов «new pair needed»; **vocabulary constraint** —
каждый B обязан использовать словарь из оракул-документа ① (иначе узел не генерализует); формы пар по узлам
(node-02 = 1 packet; node-03 = все statements сразу + граф зависимостей; node-07 обязателен pass-case;
node-08 = pass|concern|reprocess, «pass» = тишина); **минимум пар** = 1 original + 1 phrasing-B на сценарий на
узел (3 сценария × 4 узла = 8 пар, не 20). ③ **Coverage extension pass** — узлы, что были human-proxy,
превращаются в пары (единственный случай, где B берётся из evidence-трейса, а не пишется с нуля).
**Наивысшая ценность — узлы 04/05/06** (у них 0 покрытия во внешнем корпусе; без них любой сценарий, дошедший
до processing, навсегда падает в human proxy). Цепочка: node-03 B → node-04 A; node-04 B → node-05 A; node-05
B → node-06 A. Index обновляется после КАЖДОГО действия («пара, которой нет в индексе, не существует для
обучения»).

**4. `EXAMINATION-METHOD-ORCHESTRATION-v6` — форма пары и что именно теперь учат.** Каждая пара: `Contract:`
(сигнатура интерфейса) → `A:` (реальный runtime-вход) → `B:` (реальный runtime-выход, **написанный целиком**;
«B не должен говорить, что output должен содержать — он должен ЕГО содержать»). **3 слоя пар:** ① transition-
orchestrator (prompt → `LogicalOrchestratorResult` с selected/rejected кандидатами), ② 9-node flow (node-01
DomainSplitter … node-09 Formatter, точные контракты `ILogicalUnit<...>`), ③ teaching-units (`PairParserUnit`
→ `FeatureExtractionUnit` → `ModelTrainingUnit` → `HeldOutEvaluationUnit` → `CorrectionLearningUnit`).
Teaching-пары ОБЯЗАНЫ нести измерения: `teaching_time_ms`, `cpu_time_ms`, `working_set_peak_bytes`,
`training_accuracy`, `held_out_accuracy` и т.д. **Failure modes EM-1..EM-5:** контракт не назван; B описывает
вместо показа; transition заменил flow-узлы; downstream-форма выдумана до upstream-выхода; корпус учит факт,
а не оркестрацию. — **Это ровно тот стандарт, которому реальный построенный корпус (pass 7) НЕ соответствует:**
87 % scaffold-строк нарушают EM-2 (B описывает/шаблон) и EM-5 (учат факт, не оркестрацию).

**Вывод части 1:** задуманный пайплайн полный и честный — `документ → 5 оракулов → эксперимент → CONCERN/
REPROCESS-находки → 3-этапный learning-set (correction/gap-closure/coverage-extension) → обученный узел через
teaching-flow`. Он написан так, что человек-архитектор пишет оракул руками, а обучение приходит потом. **Ни
один узел этого конвейера от «эксперимент прошёл» до «узел обучен» не был пройден до конца в данных папки** —
все реальные раунды (E53/T34/P3/WDA/CB4) остановились на детерминированном оракуле-адаптере, обучение (шаг 4
методички + Layer-3 teaching-пары) не запускалось.

---

### ЧАСТЬ 2 — Квоты corpus-expansion и source-mapping (эволюция v1 → v7)

**`CORPUS-EXPANSION-PLAN-v1`** — исходная квота: **100 train + 20 test на суб-фазу**. При 27 суб-фазах =
**2 700 train / 540 test** (+60 cross-phase = 3 160 / 680). Механика:
- **Стратифицированный сплит (D-08):** 4 страты на суб-фазу — A domain-breadth (вертикали: e-commerce/
  healthcare/fintech/dev-tools/social), B team-context (стартап/mid/enterprise/OSS), C depth-level (набросок/
  спека/референс), D failure/edge-case; минимум 25/25/25/25 в train, 5/5/5/5 в test — гарантирует, что test не
  из «чистых» документов.
- **3 source-tier:** Tier 1 real-project (после анонимизации; наивысшая ценность), Tier 2 open-source (текущий
  primary; Apache-2.0/MIT = training OK, CC-BY = attribution, PSF-2.0 = OK), Tier 3 synthetic (только gap-filler
  против реального regulatory/technical референса, после domain-expert review). 4 обяз. аннотации на документ:
  source-id `[BATCH]-[SUB-PHASE]-[SEQ]`, license class, attribution y/n, anonymization y/n.
- **Held-out = 5 категорий × 4 документа:** ① correct domain assignment (conf>0.70), ② knowledge-edge detection
  (learning-gap packet, без выдуманного ответа), ③ cross-sub-phase constraint, ④ conflict detection (rejected
  item), ⑤ stratum-D edge cases. **Порог прохождения суб-фазы = 17/20**; cross-phase = 60 документов (10 на 6
  переходов), все с верным вердиктом gate.
- **Build waves 1–5** (по overlap с существующим корпусом, не по номеру фазы): Wave-1 = 3.1/3.3/3.4/4.1/4.3
  (совпадают с OTel/EF Core/Dapper парами), … Wave-4 = design 1.1–1.5 (самый далёкий словарь строится
  ПРЕДПОСЛЕДНИМ), Wave-5 = QA-automation + transition-документы. **Eval cadence — после каждой волны**, с
  regression-проверкой предыдущих волн.

**Эволюция квоты (важно — здесь квота тихо ужимается):**
| Версия | Дата | Ядро изменения | Квота |
|---|---|---|---|
| **v1** | 05-04 | исходный план 27 суб-фаз, страты/тиры/волны/eval-17/20 | **2 700 / 540** (+60) |
| **v2** | 05-04 | ретрансляция node-01..09 → роли `ILogicalUnit`; контент не менялся | 2 700 / 540 (+60) |
| **v3** | 05-04 | assumed-state: контракты в `21.AgentInfrastructure/Logical`; карта «существующий класс → `ILogicalUnit`-адаптер» (InternalLlmGateway, RouteClassifier, …) | тот же |
| **v4** | 05-04 | **исправлен счёт 27 → 31 суб-фаза**; WDA-набор (WDA-00..06) = гейт ПЕРЕД масштабом корпуса | **3 100 / 620** (+60 = 680 held-out) |
| **v5** | 05-05 | убраны stop/report-гейты (continuous execution); **2 уровня завершения: Build-Ready vs Production-mature** — production-квота вынесена в backlog, чтобы не блокировать первый полезный build | build-ready = минимум full-path покрытия |
| **v6** | 05-05 | build-ready готов (**144 training-packet, 36 held-out, 31/31 суб-фаз, 6/6 переходов**); новая цель — orchestration teaching corpus (prompt → topology), а не изолированное поведение узла | +orchestration-слой поверх |
| **v7** | 05-05 | первая граница обучения = `prompt → LogicalTransitionOrchestrator → A/B/X transition`; transition-слой недетерминированный; сценарии **O1–O5** | transition-orchestrator корпус |

**КРИТИЧЕСКИЙ разрыв методология↔реальность:** план требовал **3 100 обучающих документов** (v4). Реально к v6
существует **144 training-packet** (≈4,6 % от 3 100; из них по pass 7 лишь ~75 строк — настоящие пары, остальное
scaffold). То есть **числовой floor 100/суб-фазу был молча понижен** через введение «build-ready ≠ production-
mature» (v5) до «минимум full-path покрытия», а затем цель вообще сместилась с «наполнить квоту» на «научить
transition-оркестрацию» (v6/v7) — при том что базовая квота так и не набрана. Для плана продолжения: **это
незакрытый floor**, а не выполненный план (см. CLAUDE.md `background-library-as-artifact gate`: числовой floor
связывающий, пока Luba явно не одобрит меньший scope).

**Source-mapping (`EXTERNAL-SOURCE-REGISTRY v2–v5` + `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN v2–v5`):**
каждый источник обязан заявить: какие transition-типы / какие фазы·суб-фазы·task-типы / какие runtime-контракты
(`ILogicalUnit/Flow/Orchestrator`) он может учить + какие использования исключены. **Если источник нельзя
привязать хотя бы к одному runtime-контракту — он остаётся вне learning-set.** License-состояния: `first_party`,
`external_license_checked`, `attribution_required`, `source_gap`, `corruption_case`, `verify_before_use`
(последнее НЕ может войти в обучающие строки). Пайплайн: `source registry → license/authority check → prompt/
evidence extraction → transition-orchestrator rows → Node 01…09 rows → teaching-unit rows → held-out/corruption
rows → measured learning-cycle evidence → batch zip`. Правило: **ни одна downstream-строка не пишется из
воображения** — каждый A берётся из принятого B предыдущей станции (тот же EM-4 анти-выдумывание).

---

### ЧАСТЬ 3 — Эксперименты доменного маппинга (Категория 2 / «E48–E52» — маппинг-пруфы)

Это **отдельная ветка** от E53: E53 проверяет оркестрацию на сложной человеческой задаче; маппинг-эксперименты
проверяют, что **сама skills/guidance/planning-инфраструктура правильно роутит и байндит СВОИ инструменты до
начала оркестрации**. Здесь оркестрационная архитектура используется как **движок экзаменации governance-
документов**. Все планы датированы 2026-05-04, `Status: Awaiting approval`, baseline **985/985**, единый шаблон:
Round A = 5 оракул-документов → Round B = детерминированный эксперимент (**не зовёт live-LLM**) → Round C =
второй домен. (Номера E48–E52/G1–G6 присваиваются ПОСЛЕ одобрения; внутри планы ссылаются на «E51B expected
trace» как эталон конкретности.)

| План | Тестируемый субъект | Что доказывает (ключевые claim) | Известный failure mode-источник |
|---|---|---|---|
| **SKILL-DOMAIN-MAPPING** | skills-factory (129 скиллов/15 слоёв); первый домен — plan-review set из 6 скиллов | правильный роут скилла к задаче/домену; байнд арбитров (goal-delivery first, two-input isolation, scope-isolation, business-logic, principles, NDJSON); active-prerequisite (скилл, названный в flow-документе, загружен, а не inert label); scope-gap; authority-preservation при подмене FC-батареи кастом-арбитрами | Behavioral Correction **8** (inert citation + кастом-арбитры пропускают DNA-violations) и **10** (FC scope-gap на remediation-планах) |
| **GUIDANCE-DOMAIN-MAPPING** | session-start-prompt (первый guidance-документ) | absorption directive fires first (paraphrase до governance); session-type classification только по keyword (не по контенту документа); governance-first load-order (шаги 1–2 до 3–4); verbatim goal-anchor; Hard Rule B (correction = весь next-response); scope Hard Rule A | Behavioral Correction **7** (governance-wall до paraphrase) и **6** (реклассификация из контента) |
| **PLANNING-DOCUMENT-DOMAIN-MAPPING** | semantic-compiler session-plan | 4 critical rules верно кодируют источники (Rule A→E20, B→E23, C→E44, D→locked D-04); Gate B detection accuracy (0 false-neg / 0 false-pos); loading-sequence completeness; test-name coverage; arbiter-binding | 5 найденных gaps (Rule C без файла; Phase-3 gate стреляет поздно; SUCCESS-regression; loading-gap; test-coverage) |
| **ARTIFACT-EXAMINATION-PROTOCOL** (мета-M1) | 3-слойный протокол экзаменации артефактов раунда (structural / contract / judgment) | structural fires на inventory-mismatch ДО чтения контента; contract читает реальный контент, не manifest-claim; judgment на node-03 читает ГРАФ, не список; node-08 verdict-class (reprocess для structural defect, не concern); node-09 читает human-result против checklist; gap classification | синтетический артефакт с **5 внедрёнными сбоями** (структурный+контрактный+3 judgment); 6 арбитров |

Плюс **binding-варианты** (`SKILL-BINDING`, `PLANNING-DOCUMENT-BINDING`, `LEARNING-SET-TRANSLATION-BINDING`)
и `SESSION-*` — тот же метод для проверки, что «скилл/план/learning-set привязан к правильному домену/арбитру».
**Дерева зависимостей:** guidance (стартует раньше всего) → skills (роут скиллов) → planning (исполнение плана)
→ artifact-examination (проверка того, что произвели сессии). Все Round-A можно писать параллельно; Round-B
независимы.

**REAL vs PLANNED по этой ветке:** это **PLANNED, не PROVEN**. В папке есть Round-A дизайн-планы (эти 4+3
файла), но **НЕТ** ни одного round-пакета/validation-results/round-zip для домен-маппинга (в отличие от E53
D–K, T34, P3, WDA, CB4, у которых есть исполненные раунды). То есть маппинг-эксперименты **спроектированы, но
не исполнены**; их Round B (детерминированная проверка) не запускался в данных папки. Даже если бы исполнились
— по собственному дизайну Round B **детерминированный** (применяет задокументированные правила, не зовёт LLM),
то есть доказал бы «правила роутинга/байндинга консистентны», а не «обучен маппинг-юнит».

---

### ЧАСТЬ 4 — Аудит документации фаз (какие фазы имеют источники для обучения)

**`phase-documentation-audit-2026-05-04`** (report + coverage-matrix + transition-matrix + next-actions + 5 CSV
+ summary.json). Вопрос аудита: хватает ли текущей документации XIIGen, чтобы стартовать эксперименты по всему
жизненному циклу, или важные фазы — ещё knowledge edges, где learning-set преждевременен.

- **Инвентарь:** просканировано **5 667** doc-like файлов → **2 030** experiment-ready кандидатов. Проверено 7
  фаз, **31 суб-фаза**, 6 phase-переходов, 24 sub-phase-перехода.
- **Матрица покрытия (31 суб-фаза):** **26 covered_for_experiment**, **1 thin_boundary** (6.4 Accessibility
  Audit — 1 кандидат, мало), **4 knowledge_edge_or_missing_source** — **5.2 Integration Tests, 6.3 Security &
  Compliance Review, 7.1 Test Scenario Automation, 7.2 Regression Suite Maintenance**. Реестр фаз: Phase 1–4 =
  Ready; Phase 5 = Ready-с-одной-границей; Phase 6 = Ready-с-gap-и-thin; Phase 7 = Partial-boundary.
- **Переходы:** все 6 adjacent phase-переходов = transition_experiment_candidate; из 24 sub-phase-переходов 18
  candidate, **6 knowledge-edge** (5.1→5.2, 5.2→5.3, 6.2→6.3, 6.3→6.4, 7.1→7.2, 7.2→7.3 — все из-за 4 missing-
  source суб-фаз).
- **Вердикт аудита:** архитектура ГОТОВА для multi-phase экспериментов, но **корпус НЕ равномерно готов для
  learning-set**. Правильный следующий шаг — transition-эксперименты там, где source-authority уже есть
  (**Phase 3 → Phase 4 первым** — это и есть T34), а 4 missing-суб-фазы **явно помечать knowledge edges**
  (та же дисциплина, что E53: не выдумывать отсутствующую компетенцию). Next-actions: T34 Round A = oracle-
  writing (5 T34-документов), НЕ строить broad learning-sets для 4 missing-областей, произвести 5 source-
  authority документов (INTEGRATION-TEST / SECURITY-COMPLIANCE / QA-AUTOMATION-SCENARIO / REGRESSION-SUITE /
  ACCESSIBILITY-AUDIT source-authority).
- **Связь с планом продолжения:** аудит — это **карта «где вообще есть материал, чтобы учить»**. Для расширений
  VS/VS Code (документация кода + design-to-code) сильнее всего покрыты Technical Design (3.x), Implementation
  (4.x), Design/Layout (1.1–1.2) и CI/CD/reporting (7.3–7.4); слабее — интеграционные тесты, security-review и
  QA-automation-scenario. Это прямой список «где корпус придётся достраивать перед обучением».

---

### ГЛАВНЫЙ вывод pass 8

**Метод есть, исполнения нет.** Слой методологии (4 методички + corpus-plan v1–v7 + 4+ домен-маппинг-плана +
phase-doc-audit + external-source-registry/pipeline) — **зрелый, самосогласованный и честный**: он точно
описывает, как документ становится обучающим набором и как узел обучается, с анти-выдумыванием (EM-1..5),
vocabulary-constraint, held-out 17/20, teaching-измерениями и license/source-gate. Но:

1. **Квота не набрана:** план 3 100 train-документов → реально ~144 packet (~75 реальных строк); floor 100/
   суб-фазу молча понижен через «build-ready ≠ production-mature».
2. **Домен-маппинг-эксперименты (E48–E52/G-series) — только Round A дизайн:** 0 исполненных раундов; и по
   дизайну Round B детерминированный.
3. **Teaching-цикл не запускался:** Layer-3 teaching-пары (`ModelTrainingUnit`/`HeldOutEvaluationUnit`) описаны,
   но «первый измеренный прогон обучения» — в будущем времени (подтверждает pass 1–7).
4. **Аудит сам признаёт:** корпус НЕ равномерно готов; 4 суб-фазы — knowledge edges; строить broad learning-set
   там преждевременно.

Для плана продолжения расширений VS/VS Code: **это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` и на уровне метода** —
берём готовую методологию как **чертёж будущего трейнабл-конвейера** (она реально хороша и переносима), но
относимся к «144 build-ready / домен-маппинг / teaching-цикл» как к **незакрытым floor-ам**, а не к
выполненной работе. Самый дешёвый честный первый продуктовый срез из этой папки — исполнить ОДИН полный
проход методички (документ → 5 оракулов → эксперимент → 3-этапный learning-set → **реальный teaching-прогон с
held-out метриками**) на одной сильно-покрытой суб-фазе (напр. 3.1/4.1), чтобы впервые пройти конвейер от
конца до конца и получить первую обученную единицу с чекпойнтом.

**Теги:** `#method-layer` `#corpus-quota-3100-vs-144` `#doc-to-experiment-to-learningset` `#5-oracle-docs`
`#3-action-learningset` `#examination-method-v6` `#domain-mapping-E48-E52-round-A-only` `#phase-doc-audit-26-of-31`
`#4-knowledge-edges` `#teaching-cycle-never-run` `#planned-not-proven` `#confirms-pass-1-7`.

### Что осталось для проходов 9–10

- **Pass 9** — `CORPUS-COMPLETION-LEDGER v1–v29` (где именно scaffold начал засчитываться как build-ready),
  WAVE-1 gap-analysis / source-authority-coverage-audit / conformance-suite, WDA-01 real-router contract,
  `LEARNING-CYCLE-IMPLEMENTATION-PLAN v1–v3` + `ORCHESTRATION-TEACHING-CYCLE-PLAN v1–v3` (детали teaching-цикла),
  ORCHESTRATOR-INSPECTOR-DESIGN v1–v4, реальный код `round-zips/` (25 .cs) и CB4 staging (.cs) — построчная
  сверка непрочитанных pair-set tails (batch 004–007, center node-04..09).
- **Pass 10** — сплошная зачистка (ни один из 513 файлов не пропущен) + executive summary + финальный
  trained-vs-deterministic вердикт по всей папке для плана продолжения расширений VS / VS Code.

**Честная строка покрытия pass 8:** прочитаны ЦЕЛИКОМ 4 методички (translate/structure/dev-phases/examination-v6),
`CORPUS-EXPANSION-PLAN-v1` и `v7`, 4 домен-маппинг-плана (skill/guidance/planning/artifact-examination), 4 файла
phase-doc-audit (report/coverage-matrix/transition-matrix/next-actions); прочитаны ШАПКИ+core-correction
`CORPUS-EXPANSION-PLAN v2–v6` и `EXTERNAL-SOURCE-REGISTRY-v5`+`PIPELINE-PLAN-v5`. НЕ читались построчно: тела
`SKILL-BINDING`/`PLANNING-DOCUMENT-BINDING`/`LEARNING-SET-TRANSLATION-BINDING` (характер выведен из
идентичного шаблона 4 прочитанных домен-маппинг-планов), 5 CSV аудита (их содержание отражено в report/matrix
нарративно), `LEARNING-CYCLE-IMPLEMENTATION-PLAN`/`ORCHESTRATION-TEACHING-CYCLE-PLAN` (добор pass 9). Заявляю
ТОЛЬКО: методология полная и честная, но не исполнена — квота 3 100→~144, домен-маппинг = Round-A-only (0
исполнено), teaching-цикл не запускался, аудит сам ставит 26/31 covered + 4 knowledge-edges; это `PASS_AS_
HONEST_UNTRAINED_SCAFFOLD` на уровне метода, подтверждает проходы 1–7.

---

## Pass 9/10 — real-training lead (репо-код) + синтез + cross-folder + открытые вопросы

> Это SYNTHESIS-проход по папке №8. Задание перенаправило pass 9 с «file-by-file tail»
> (ledger v1–v30, inspector-дизайны, round-zips) на **закрытие real-training-лида в живом
> repo-коде** + сборку одной actionable-картины для плана «продолжить расширения VS / VS Code».
> Остаток пофайловой зачистки (CORPUS-COMPLETION-LEDGER v9–v30, ORCHESTRATOR-INSPECTOR-DESIGN
> v1–v4, LEARNING-CYCLE-IMPLEMENTATION v1–v3, round-zips 25 .cs) переходит в pass 10.
>
> **Прочитано ЦЕЛИКОМ в этом проходе (живой repo-код, read-only):**
> `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` (1271 стр.),
> `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs` (495 стр.),
> `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs` (167 стр.),
> `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` (953 стр.),
> `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` (467 стр., содержит `LogicalDpoTrainingUnit`
> + `TransformerEmbeddingKnnLogicalUnit`) + grep-сверка тела `11.DPO/DpoTrainer.cs` `StepVector`.

### ГЛАВНЫЙ вывод pass 9 — «есть ли реально обученный юнит в текущем repo?» → **КВАЛИФИЦИРОВАННОЕ ДА**

Это **разворот** относительно всей папки №8. В `orchestrator experiment` (проходы 1–8) вердикт был
плоское **НЕТ**: все «trained» юниты E53/T34/CB4 = зашитые `LearnedRules[]` + keyword-`Contains`,
без весов/чекпоинтов/DPO. Но **живой repo-код на дату этого прохода содержит настоящий трейнабл-
конвейер**, которого в папке №8 не было. Он подтверждён построчно и по цепочке вызовов:

**Реальная цепочка обучения (Route B из RAG-05 — «DpoTrainingHandoff → 11.DPO/DpoTrainer» — ВЖИВУЮ подключена):**

`AdaptiveLogicalOrchestrator.RunWithCapabilityGenerationAsync`
(`AdaptiveLogicalOrchestrator.cs:158`) → `DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync`
(`DefaultLogicalCapabilityTrainer.cs:94`) → `ExecuteDpoTeachingAsync`
(`:208/:423`) → `new LogicalDpoTrainingUnit().ExecuteAsync`
(`LlmIngredientLogicalUnits.cs:308/:320`) → `new DpoTrainer(...).StepVector(batches, step)`
(`LlmIngredientLogicalUnits.cs:355/:372` → `11.DPO/DpoTrainer.cs:173`) → **`BackwardLogProbs` +
`AdamW.Step(_policy.Parameters(), lr)`** (`DpoTrainer.cs:203-213`, backprop `logits.Backward` `:317`).

| Файл (живой repo) | Что реально делает код | Det/Trained | Доказательство (file:line) |
|---|---|---|---|
| **`DpoTrainer.cs`** (`11.DPO`) | `StepVector` считает DPO-margin, делает `BackwardLogProbs(chosen,+β)` / `(rejected,−β)`, потом `AdamW.Step(policy.Parameters())` — настоящий градиентный шаг по весам transformer'а | **TRAINED (градиент)** | `:19/:27` AdamW; `:173` StepVector; `:203-213` backward+step; `:317` logits.Backward |
| **`LogicalDpoTrainingUnit`** (`LlmIngredientLogicalUnits.cs`) | Оборачивает DpoTrainer в `ILogicalUnit`: применяет `LoraApplicator.Apply` (10.LoRA), грузит parent-checkpoint (`LoadCheckpoint`), кодирует prompt/chosen/rejected токенайзером в тензоры `[1,T]`, крутит до `MaxStepCount` с `ConvergenceMonitor`, пишет `dpo_step_*.json` метрики (Loss/Accuracy) | **TRAINED** | `:308` класс; `:339` LoRA; `:344` LoadCheckpoint; `:355` DpoTrainer; `:372` StepVector; `:393-400` ingredient-ids `11.DPO:DpoTrainer.StepVector` |
| **`DefaultLogicalCapabilityTrainer`** (`18.CapabilityTraining`) | Полный lifecycle: tiny-transformer policy+reference (`LlmModel`), DPO-шаг, `SaveCheckpoint(policy.bin)` + `tokenizer.Save`, манифест с checkpoint-ссылками, **continue-training от parent-манифеста** (`FindActiveParentManifest` → version++, TrainingStepCount+1), регистрация в `InMemoryLogicalUnitRegistry` на trust=Probationary, замер CPU/время | **TRAINED (полный цикл)** | `:31` referenceModel; `:208` DPO; `:276` SaveCheckpoint; `:284` KNN-wrap; `:499-514` continue-from-parent; `:329` Register |
| **`TransformerEmbeddingKnnLogicalUnit`** (`LlmIngredientLogicalUnits.cs`) | Инференс-юнит: реальные `EmbeddingModel(model,tokenizer)`-эмбеддинги + `HnswIndex` ANN-поиск; финальный выбор — E34V2-фьюжн (vector 0.45 + BM25 0.35 + pattern 0.20) | **TRAINED-эмбеддинги + ДЕТЕРМ. фьюжн-выбор** | `:31` класс; `:79` EmbeddingModel; `:82` HnswIndex; `:177-204` ScoreCandidates |
| **`AdaptiveLogicalOrchestrator`** (default `RunAsync`) | Маршрутизация/классификация по умолчанию — детерминированные юниты; learned-юнит подставляется ТОЛЬКО при `flags.UseLearnedUnitWhenAvailable` + зарегистрированном юните | **ДЕТЕРМ. по умолчанию** | `:46` trainer опционален; `:158` train-путь отдельный; `:171` нет trainer→нет обучения; `:269-280` ResolveActiveUnit; `:275` fallback |
| **`MultiSubPhaseRouterUnit`** | Роутер = зашитый словарь `SubPhaseAnchorTerms` (30 суб-фаз × keyword-якоря) + predicate/token overlap + HybridRanker (BM25+KNN) фьюжн; `E34V2` как tie-breaker `×0.02` | **ДЕТЕРМ.** | `:23-56` anchor-словарь; `:249-288` Score; `:282-284` веса |
| **`PromptRoutingInputCompilerUnit`** | Компиляция промпта = `SemanticTeachingPromptCompiler` (детерминированный семантик-компайлер 29), confidence — константы 0.70/0.92 | **ДЕТЕРМ.** | `:19/:52` compiler; `:94` const conf |

### Честные оговорки (чтобы ДА не превратилось в overclaim `PASS_AS_TRAINED_FEATURE`)

Плюмбинг настоящий, но **это ещё не «обученная продуктовая фича»** — по 4 причинам:

1. **Opt-in, не default.** Обучение запускается только через `RunWithCapabilityGenerationAsync`
   (не `RunAsync`), только когда возник knowledge-edge-бранч, **и** в конструктор передан
   `_capabilityTrainer` (он `ILogicalCapabilityTrainer?` — необязателен, `:46`), **и** взведён
   `UseLearnedUnitWhenAvailable`. Без trainer'а честный ответ — видимый knowledge edge (`:171`).
   Дефолтный рантайм (роутинг + классификация домена + компиляция) — **детерминированный**.
2. **Учится на СИНТЕТИЧЕСКИХ строках, НЕ на корпусе папки №8.** `chosen`-сторона —
   авто-шаблон `"Generated capability for sub-phase X: <Name>. Execute ... task type ..."`
   (`BuildChosenCapabilityText` `:680-683`); `rejected` — фикс-строка
   `"Do not generate a capability for sub-phase X."` (`:174`). То есть DPO-предпочтение = «выдай
   шаблон способности для этой суб-фазы» vs «откажись». Это **self-labelling** из дескриптора
   суб-фазы (её же name/purpose/predicates), а НЕ из 144 build-ready packet / 75 реальных center-
   пар / CB4-корпуса. Настоящая ML-механика кормится синтетическим скэффолдом.
3. **Tiny-масштаб.** `BuildTinyConfig` (`:882-894`): VocabSize 512, ContextLen 64, HiddenDim 32,
   4 слоя, LoRA Rank 2 (`:856-859`). Это доказательство-жизнеспособности-цикла, а не
   продуктовая модель.
4. **Финальный выбор learned-юнита остаётся детерминированным фьюжном.** `TransformerEmbeddingKnn`
   даёт transformer-эмбеддинг, но итоговый pick — BM25+KNN+pattern-фьюжн (`ScoreCandidates`).
   Обучены веса policy.bin (реально), но authority-выбор на инференсе — детерминированное
   ранжирование (перекликается с `Acceptance-signal independence` / `Y_best-determinism`
   гейтами CLAUDE.md — это то, что аудит должен пометить при промоушене).

**Итог по real-training-лиду:** «настоящее обучение существует?» → **ДА, конвейер (DPO+LoRA+
transformer+checkpoint+continue-training+manifest+registry) собран и работает в живом repo**, но
он `opt-in`, `tiny`, и **накормлен синтетическими one-line-строками, а не корпусом папки №8**.
Для плана продолжения это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` **с готовым трейнабл-двигателем**:
двигатель есть, топлива (реального корпуса, прогнанного через него) — ещё нет.

### СИНТЕЗ папки №8 — одна actionable-картина для плана «продолжить расширения»

**(а) Реально переиспользуемые активы (то, что можно НЕ строить заново):**

- **Готовая иерархия `ILogicalUnit → ILogicalFlow → ILogicalOrchestrator`** в живом repo
  (`21.AgentInfrastructure/Logical` + `15.OrchestratorModel`): реестры юнитов/флоу/топологии,
  adaptive-роутинг по топологии, knowledge-edge как первоклассный результат, transition-follow-up
  с bounded-глубиной, context-carry-forward пакеты. Это **«мозг» для обоих расширений** — VS/VS Code
  шлют промпт, получают human-result + trace.
- **Готовый трейнабл-двигатель** (см. выше): `DefaultLogicalCapabilityTrainer` +
  `LogicalDpoTrainingUnit` + `11.DPO/DpoTrainer` + `10.LoRA` + checkpoint/continue-training/manifest/
  probationary-trust-registry. **Это и есть «shared center» из RAG-05, уже подключённый.**
- **Editable-topology 5-step-паттерн** (Phase 32–52): add/relocate domain/phase/sub-phase/transition
  как мутируемые команды + реестры — расширяемость без переписывания.
- **LlmInspector / ORCHESTRATOR-INSPECTOR** (Phase 60–81, WPF Canvas после v4): готовый рантайм-
  трейс-инструмент (станции, топология, parallel-wave, resource-guard) — **готовый UI под trace-view
  расширения VS**.
- **Зрелый, честный МЕТОД корпуса** (pass 8): документ → 5 оракулов → эксперимент → 3-этапный
  learning-set → held-out/teaching — с анти-выдумыванием EM-1..5 и license/source-gate.
- **CB4 security-verification корпус** + **75 реальных center-пар** + phase-doc-audit (26/31
  суб-фаз имеют источники) — первый реальный материал под обучение.

**(б) Реальность (что НЕ следует называть готовым):**

- Вся арка папки №8 (E53/T34/P3/WDA/CB4/inspector) — **честный НЕобученный скэффолд**: «trained»
  юниты там = детерминированные оракулы. Обучение **никогда не запускалось на этом корпусе**.
- **Корпус = 18 % квоты** (144 build-ready из плановых 3 100), из них **87 % — шаблон, ~13 %
  (~75 строк) — реальные пары**; floor 100/суб-фазу молча понижен через «build-ready ≠ production».
- Домен-маппинг-эксперименты (E48–E52/G-серия) — **только Round-A дизайн, 0 исполнено**.
- Двигатель из (а) обучен только на синтетике — «реальная обученная оркестрационная единица»
  ещё не существует.

**(в) Кратчайший путь до РЕАЛЬНО обученной оркестрационной единицы:**

Соединить (а)-двигатель с (а)-корпусом: **прогнать реальные пары папки №8** (75 center-пар +
CB4 + build-ready packet-и одной хорошо-покрытой суб-фазы, напр. 3.1 Architecture или 4.1 Backend)
**через уже существующий `DefaultLogicalCapabilityTrainer`/`LogicalDpoTrainingUnit`** — заменив
синтетический `chosen/rejected` (`:680-683`,`:174`) на реальные A/B-пары из корпуса. Это даёт первый
end-to-end проход: реальный корпус → DPO/LoRA-шаг → policy.bin checkpoint → held-out-замер →
promoted probationary-юнит в реестре → retry через `RunWithCapabilityGeneration`. Двигатель, реестр,
checkpoint, continue-training, KNN-wrap уже написаны — не хватает **топлива и одного held-out-замера**.

### Cross-folder мосты (сшивка свипа)

- **RAG-06 (llm dynamic flow):** папка №8 — это **полная лаборатория** E51/E53, которую RAG-06 видел
  сбоку. Вывод совпадает по E53 (детерминированная оркестрация). НО pass 9 **добавляет то, чего
  RAG-06 не видел**: в живом repo уже собран реальный DPO-двигатель — «trained vs deterministic»
  теперь двухуровневый: **папка №8 = детерм.; runtime-движок 18/21/11 = реальный DPO (opt-in)**.
- **RAG-05 (dpo legal build):** папка №8 строит ту же иерархию `ILearningDynamicGraph` снизу вверх;
  **«shared center» Route B (`DpoTrainingHandoff → 11.DPO/DpoTrainer`), который RAG-05 держал как
  требование, — в этом проходе подтверждён ПОДКЛЮЧЁННЫМ вживую** (`DpoTrainer.StepVector`). Это
  прямой ответ на `REAL_TRAINER_ROUTE_B_GATE`: маршрут существует; чего нет — реального корпуса и
  held-out-эвиденса на нём.
- **RAG-02 (orchestrator-extension):** `LogicalOrchestratorAssistant` + logical-юниты для проектов
  15/21–23 — папка №8 их «прото-код»; живой `AdaptiveLogicalOrchestrator` — их зрелая версия.
  Тонкий host-адаптер (R6: агент = набор API-вызовов) сидит поверх этого оркестратора.
- **Инспектор:** `LlmInspector`/`ORCHESTRATOR-INSPECTOR` (Phase 60–81) = **готовый UI под trace-view
  расширения VS** (станции/топология/wave/resource-guard уже спроектированы и превью-скриншоты есть).

### Открытые вопросы для Luba (с дефолтами)

1. **Переиспользовать готовую иерархию + инспектор или строить заново?**
   *Дефолт: переиспользовать* `21.AgentInfrastructure/Logical` + `15.OrchestratorModel` + LlmInspector
   как «мозг» и trace-UI; расширения = тонкие host-адаптеры (R6). Rebuild не оправдан — контракт
   уже проверен interface-swap'ом (E53).
2. **Реальный-корпус-сначала или доводка метода?**
   *Дефолт: корпус-сначала* — исполнить ОДИН end-to-end проход на реальных 75 center-парах/CB4 через
   существующий двигатель, чтобы впервые получить обученный юнит с checkpoint+held-out. Метод уже
   зрелый (pass 8); ещё круг методологии — это process-about-process, а не продукт.
3. **Квота 3 100 vs меньший реальный сет?**
   *Дефолт: явно принять меньший scope* (напр. 100–150 реальных пар на 1–2 суб-фазы) как первый
   build — но зафиксировать это как **осознанное понижение floor'а по решению Luba**, а не молчаливое
   (сейчас floor 100/суб-фазу понижен без явного согласия — `background-library-as-artifact gate`).
4. **Кормить `DefaultLogicalCapabilityTrainer` реальными парами вместо синтетики?**
   *Дефолт: да* — заменить `BuildChosenCapabilityText`/фикс-rejected (`:680-683`,`:174`) на реальные
   A/B-строки корпуса; это точка, где двигатель начинает учить настоящую способность, а не шаблон.
   Требует решения Luba: какие суб-фазы первыми и какой held-out-split.
5. **Детерминированные оракулы E53/CB4 — оставить как ablation-базлайн?**
   *Дефолт: да, пометить eval-only baseline* — они дают точку сравнения «обученный юнит vs
   детерминированный оракул» (ablation-гейт CLAUDE.md), но не могут быть финальной authority на
   продуктовом пути. Полезны именно как база для held-out-дельты.

**Теги:** `#real-training-lead-CLOSED` `#route-B-DpoTrainer-wired` `#qualified-YES` `#opt-in-not-default`
`#synthetic-rows-not-folder8-corpus` `#tiny-config` `#DPO+LoRA+checkpoint+continue-training`
`#reusable-hierarchy+inspector` `#shortest-path=real-corpus-through-existing-engine`
`#bridges-RAG-06/05/02` `#5-open-questions`.

### Честная строка покрытия pass 9

Прочитаны ЦЕЛИКОМ 5 живых repo-файлов (`AdaptiveLogicalOrchestrator` 1271, `MultiSubPhaseRouterUnit`
495, `PromptRoutingInputCompilerUnit` 167, `DefaultLogicalCapabilityTrainer` 953,
`LlmIngredientLogicalUnits` 467) + grep-сверка тела `11.DPO/DpoTrainer.cs` `StepVector`/backward/AdamW.
Заявляю ТОЛЬКО: (1) real-training-лид ЗАКРЫТ — в живом repo есть настоящий DPO/LoRA/checkpoint-
конвейер (Route B подключена), но opt-in, tiny и на синтетике, не на корпусе папки №8 →
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` с готовым двигателем; (2) синтез активов/реальности/кратчайшего
пути; (3) мосты к RAG-06/05/02 + inspector; (4) 5 открытых вопросов с дефолтами. НЕ читались
построчно в этом проходе: `DpoTrainer.cs` целиком (сверен грепом ключевых строк), тела
`10.LoRA/LoraApplicator`, `05.Embeddings/EmbeddingModel`, `06.VectorSearch/HnswIndex` (их роль
выведена из вызовов и ingredient-манифеста), CORPUS-COMPLETION-LEDGER v9–v30, INSPECTOR-DESIGN v1–v4,
round-zips 25 .cs — это добор pass 10.

### Что осталось для pass 10

- Сплошная зачистка: CORPUS-COMPLETION-LEDGER v9–v30 (где именно scaffold стал считаться build-ready),
  ORCHESTRATOR-INSPECTOR-DESIGN v1–v4 (детали trace-UI под VS), LEARNING-CYCLE-IMPLEMENTATION v1–v3 +
  ORCHESTRATION-TEACHING-CYCLE v1–v3 (teaching-цикл), round-zips 25 .cs (контракты/реестры/оркестратор),
  непрочитанные pair-set tails (batch 004–007, center node-04..09), 9 SVG / 7 PNG / 3 MMD.
- Executive summary по всей папке (513 файлов, ни один не пропущен) + финальный trained-vs-deterministic
  вердикт с учётом развязки pass 9 (папка = детерм.; runtime-движок = реальный DPO opt-in) для плана
  продолжения расширений VS / VS Code.

---

## Pass 10/10 — completeness sweep + executive summary (folder #8/12 orchestrator experiment)

> **Роль прохода (ФИНАЛ папки №8):** сплошная зачистка + executive summary. Не вводит нового вердикта —
> сшивает проходы 1–9 в одну картину, проверяет, что **ни одна из 12 семей (§A–§L) и ни один из 513
> файлов не пропущен**, честно помечает, что читалось построчно, а что закрыто по-паттерну/сэмплом, и
> даёт Luba один читаемый раздел, из которого можно взять всё нужное для плана продолжения расширений.
>
> **Что дочитано/до-сэмплено именно в этом проходе (закрытие хвостов pass 9):**
> `CORPUS-COMPLETION-LEDGER-v9.md` целиком (представитель серии v1–v29),
> `LEARNING-CYCLE-IMPLEMENTATION-PLAN-v3.md` целиком + `ORCHESTRATION-TEACHING-CYCLE-PLAN-v1.md` (шапка+unit),
> `WAVE-2-CENTER-PHASE-BATCH-001-NODE-04-05-06-CHAIN-SET-v1.md` (ранее непрочитанный center-хвост),
> **физический инвентарь `round-zips/`** (7 stage-подпапок, извлечённые .cs) и **staging CB4 .cs**
> (phase-84/85/86/87), пересчёт типов файлов (513) и групп (96 round-manifest / 30 ledger / 57 top-zip /
> 9 svg / 7 png / 3 mmd). Остальное — сверка карты pass 1 против реального дерева.

### 1. Completeness sweep — покрытие по 12 семьям (ни одна не пропущена)

Пересчёт типов (весь корень, 513): **md 320 · zip 112 · cs 29 · json 25 · svg 9 · png 7 · csv 4 · mmd 3 ·
pdf 2 · html 2** = 513 ✓. Директорий **94**. Ниже — статус каждой семьи из §A–§L карты (pass 1), с честным
остатком.

| Семья (что это) | Объём | Где разобрано | Статус | Честный остаток |
|---|---|---|---|---|
| **A. Верхнеуровневые планы/модели** («мозг» папки: MASTER / E53-PROOF / MULTI-PHASE / LOGICAL-UNIT-FLOW / LOGICAL-TRANSITION v1-2 / WHOLE-DEV-AREA v1-4 / TEACHING-CYCLE v1-3 / LEARNING-CYCLE v1-3 / RESOURCE-METRICS) | ~35 md | pass 1 (4 ключевых целиком), pass 6/8 (модели), pass 10 (LEARNING-v3, TEACHING-v1) | **COMPLETE** | v2/v6-промежутки моделей читаны шапкой+core-correction; вывод (план, не код) идентичен |
| **B. E53-раунды D–K** (замена 9 узлов на «trained» адаптеры) | 8 подпапок × 3 + 2 batch-findings = 26 | **pass 2 — все 24 + 2 findings целиком** + реальный `E53HighLevelOrchestrationProof.cs` | **COMPLETE** | — (полное построчное покрытие) |
| **C. T34 / P3 раунды** (phase-transition + learning-from-failure) | T34-A 7 + P3-A 7 + T34-B манифест + report(5 форматов) | **pass 3 — все 14 + манифест + report.md + `P3LearningFromFailureProof.cs` целиком** | **COMPLETE** | report .html/.pdf/.VISUAL = те же данные в др. форматах (не открывались как бинарь) |
| **D. WDA-00…06** (реальный суб-фазный router) | 4 подпапки (15 файлов) + 4 top-level WDA | **pass 5 — 8 core-док целиком + `SubPhaseRouterUnit.cs` (381 стр.) целиком** | **COMPLETE** | часть WDA-subdir оракул-файлов охарактеризована по представителю (тот же паттерн) |
| **E. ROUND-MANIFEST top-level** (журнал фаз 25→118) | **96 md** | pass 4 (28 фаз 25-52 целиком), pass 5 (9 из 53-81 + rest), pass 6 (17 из 82-118 + grep-rest) | **COMPLETE** | ~40 манифестов закрыты по имени+тест-счётчику+Files-Changed (не построчно), вывод непрерывен |
| **F. Wave-корпуса** (обучающие пары/леджеры) | WAVE-1 (5) + WAVE-2-BATCH-002…008 (35) + WAVE-2-CENTER (13) + intake-ledger = 53 | pass 7 (представители каждого класса целиком) + **pass 10 (center node-04-05-06 хвост)** | **COMPLETE / by-pattern** | batch 004/005/006/007 tails не построчно — паттерн «batch=template, center=real» подтверждён на 3 center-хвостах, исключений нет |
| **G. CB4** (security-verification-3.4 flow) | 14 STAGE-json + registration-plan + build-ready-summary + intake-ledger | pass 6 (registration-plan + 2 staging .cs), **pass 7 (2 json целиком + 14 через grep)** | **COMPLETE** | node-01…07 staging .cs выведены из идентичного паттерна node-08/09 (прочитаны) |
| **H. Corpus-completion ledgers/audits** | **30 ledger** + V4-final-audit + build-ready + intake | pass 7 (v30 + V4-audit + build-ready целиком), **pass 10 (v9 целиком)** | **COMPLETE / by-pattern** | v1-v8,v10-v29 — инкрементальный журнал, каждый повторяет «v4 not complete»; сэмпл v9+v30 репрезентативен |
| **I. phase-documentation-audit** (подпапка) | 4 report-md + 5 CSV + манифест + summary.json = 10 | **pass 8 — 4 report-md целиком** | **COMPLETE / PARTIAL** | 5 CSV не читаны построчно (их содержимое отражено нарративно в report/matrix) |
| **J. Отчёты / визуализация / evidence** | EXPERIMENT-REPORT (5 форматов) + 9 SVG + 7 PNG + 3 MMD | pass 3 (report.md), pass 1/5 (viz по имени+контексту) | **PARTIAL (осознанно)** | 9 SVG / 7 PNG (inspector-preview) / 3 MMD НЕ рендерились как изображения — охарактеризованы по имени+сопутствующему манифесту (UML/inspector-layout/dependency-graph); содержательно дублируют текст дизайнов |
| **K. Код-снимки (реальный C#)** | round-zips (17 извлечённых .cs в 7 stage-дир) + CB4 staging (.cs) | passes 1/2/4/6 (ключевые .cs целиком), **pass 10 (инвентарь round-zips подтверждён)** | **COMPLETE** | round-zips .cs = ТЕ ЖЕ контракты/реестры/оркестратор/T34/T35, уже верифицированные (LogicalContracts, InMemoryLogicalOrchestrator/Registry, T34PhaseTransitionProof) — нового det-vs-trained нет |
| **L. Zip-снимки** | **112 zip** (57 top-level round-phase + 51 nested в round-zips + 4 агрегатных) | pass 4 (phase-48 распакован+проверен) | **PARTIAL-by-design** | zip'ы = пофазные снимки уже-манифестированного .cs/.md; распакован 1 репрезентативный (phase-48) + 7 round-zips stage-дир; массовая распаковка не нужна (содержимое = прочитанные манифесты) |
| **+ Слой МЕТОДА** (методички + corpus-expansion v1-7 + domain-mapping + external-source registry/pipeline) | ~25 md | **pass 8 — 4 методички + corpus-v1/v7 + 4 domain-mapping-плана целиком** | **COMPLETE** | binding-варианты выведены из шаблона 4 прочитанных |
| **+ Живой repo-код** (pass-9 lead, вне папки) | 5 файлов (18/21/11 проекты) | **pass 9 — все 5 целиком + grep DpoTrainer** | **COMPLETE** | `DpoTrainer.cs`/LoRA/Embeddings целиком не читаны — роль по вызовам |

**Вывод sweep: ни одна семья не пропущена.** 8 семей — COMPLETE (построчно/репрезентативно), 2 — COMPLETE-by-pattern
(F/H: паттерн подтверждён многократно, хвосты по-паттерну), 2 — PARTIAL осознанно (**J** — визуальные бинарники
не рендерились; **L** — zip'ы не распаковывались массово, т.к. дублируют прочитанные манифесты). Ни один из этих
двух PARTIAL не может изменить вердикт trained-vs-deterministic (это картинки дизайнов и снимки уже-прочитанного кода).

### 2. EXECUTIVE SUMMARY папки №8 (для Luba — можно читать ТОЛЬКО этот раздел)

**Что это за папка и что она строит.** «orchestrator experiment» — это полный лабораторный журнал одной большой
дуги из **~94 экспериментальных раундов (Phase 25 → Phase 118)**, которая снизу вверх собирает иерархию
`ILogicalUnit → ILogicalFlow → ILogicalOrchestrator`: сначала контракт наименьшей исполнимой ячейки (25),
первую реальную замену узла (26/T35), flow-адаптер (27), оркестратор (28), реестры юнитов/флоу (29-30),
рантайм-шов в продуктовом проекте 15 (31), затем **редактируемую топологию** — динамический граф из 4 сущностей
(domain/phase/sub-phase/transition), каждую можно добавлять типизированной командой (32-52), затем рантайм-
**инспектор** (WPF-десктоп `LlmInspector`, наблюдает живое исполнение через tracing-декораторы, 60-81), и, наконец,
доменные доказательства оркестрации: **E53** (9-узловой flow на сложной человеческой задаче регистрации/онбординга),
**CB4** (security-verification-3.4 flow на лицензионно-чистом OWASP-ASVS-корпусе, реально зарегистрирован в
ассистенте, 84-97) и **WDA** (оркестрация ВСЕГО жизненного цикла разработки: 7 фаз × 31 суб-фаза × 6 переходов).
Дуга заканчивается тройным разворотом плана (115-118) на **transition-orchestrator** и заданием произвести
«O1 teaching-строки» для первого измеренного цикла обучения.

**Реальность дуги (главное, что нельзя приукрасить).** Вся арка Phase 25-118 — это **честный НЕобученный скаффолд**
(`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`), а не рабочая обученная фича. Каждый «trained» юнит внутри папки — это
**жёстко-зашитый детерминированный оракул**: E53-узлы = массивы `LearnedRules[]` с руками прописанными ответами +
keyword-`Contains`-матчинг по единственному фиксированному сценарию (проверено по коду `E53HighLevelOrchestrationProof.cs`);
CB4-узлы = `switch`/`Contains("code_challenge_method=plain")` + `public const double Confidence = 0.94/0.92`
(проверено по `SecurityArbiterUnit.cs`/`SecurityFormatterUnit.cs`); topology-mutation-юниты = `const Confidence = 1.0`;
WDA-router = взвешенный overlap-scorer с **тавтологичными гейтами** (`confidence = 0.70 + score*0.27` ⇒ всегда ≥ 0.70;
wrong-domain всегда < 0.50) и весами-константами 0.40/0.40/0.20 (проверено по `SubPhaseRouterUnit.cs`). Тест-счётчики
«12/12» и «1177/1177» доказывают лишь **консистентность контракта при interface-swap** (оракул неизменен → адаптер,
воспроизводящий оракул, всегда зелёный), а не наличие модели. Обучающий **корпус двухслойный и на 18 % от квоты**:
из «558 build-ready пар» ~87 % (483) — descriptive-template scaffold с утёкшим placeholder
`$(System.Collections.Hashtable.Gap)` и уравненным confidence 0.82, и лишь ~13 % (75 center-пар) — реальные
`LogicalUnitResult`-пары; плановая production-квота 3 100/620/60 закрыта на ~4,6 %, а числовой floor 100/суб-фазу
**молча понижен** через введение «build-ready ≠ production-mature». Собственный аудит корпуса выносит «v4 complete:
**NO**» и называет автогенерацию слоя B «synthetic or repetitive rows». **Teaching/training-цикл ни разу не запускался**
(grep по epoch/loss/reward_margin/checkpoint = 0; последний леджер держит «first measured learning run» в будущем
времени). Домен-маппинг-эксперименты (E48-E52/G-серия) — только Round-A дизайн, 0 исполнено.

**КЛЮЧЕВОЙ ПОВОРОТ (это меняет план).** Папка №8 говорит плоское «обучения нет» — но **живой репозиторий на дату
свипа содержит настоящий трейнабл-DPO-конвейер**, которого в этой папке не было (pass 9, прочитано построчно 5
repo-файлов). Реальная цепочка подключена вживую: `AdaptiveLogicalOrchestrator.RunWithCapabilityGenerationAsync`
→ `DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync` → `LogicalDpoTrainingUnit.ExecuteAsync` →
`DpoTrainer.StepVector` → **`BackwardLogProbs` + `AdamW.Step(policy.Parameters())`** — то есть настоящий градиентный
шаг по весам tiny-transformer'а, с LoRA, checkpoint-save/load, continue-training от parent-манифеста, манифестом и
регистрацией юнита в реестре на trust=Probationary. **Это ровно «shared center» Route B из RAG-05
(`DpoTrainingHandoff → 11.DPO/DpoTrainer`) — и здесь он ПОДТВЕРЖДЁН ПОДКЛЮЧЁННЫМ.** Честные оговорки, чтобы не
перехвалить: он **opt-in** (не дефолтный `RunAsync`, а отдельная ветка + необязательный trainer в конструкторе +
флаг `UseLearnedUnitWhenAvailable`), **tiny** (VocabSize 512 / HiddenDim 32 / 4 слоя / LoRA rank 2), накормлен
**синтетическими one-line-строками** (`chosen` = авто-шаблон «выдай способность для суб-фазы X», `rejected` = «не
выдавай»), а **не** корпусом папки №8, и финальный выбор learned-юнита на инференсе остаётся детерминированным
BM25+KNN-фьюжном. Итого двухуровневый вердикт: **папка №8 = детерминированный scaffold; runtime-движок 18/21/11 =
реальный DPO, но opt-in и на синтетике.** Двигатель есть — топлива (реального корпуса, прогнанного через него) ещё нет.

**Реально переиспользуемые активы (то, что план продолжения может НЕ строить заново).** (1) **Готовая иерархия
`ILogicalUnit → Flow → Orchestrator`** в живом repo `21.AgentInfrastructure/Logical` (+ шов в `15.OrchestratorModel`):
реестры, adaptive-роутинг по топологии, knowledge-edge как первоклассный результат, context-carry-forward — это
готовый «мозг», к которому тонкий host-адаптер (VS/VSCode/Claude/Cursor/Codex) шлёт промпт и получает human-result +
trace (соответствует R6: агент = набор API-вызовов). (2) **Готовый трейнабл-двигатель** DPO+LoRA+transformer+
checkpoint+continue-training+probationary-registry (см. поворот выше). (3) **Editable-topology 5-шаговый паттерн**
(registry→command→unit→flow→route на каждую сущность графа) — переиспользуемый рецепт расширения графа без
переписывания. (4) **`LlmInspector` / ORCHESTRATOR-INSPECTOR** (WPF Canvas после perf-разворота 74-75) — готовый
рантайм-trace-UI (станции, топология, parallel-wave, resource-guard, живая интеграция с проектами 21/22/23) — прямой
кандидат под панель трейсов расширения VS с уже усвоенным perf-уроком (bounded-старт, никаких demo-heavy автозапусков).
(5) **Зрелый честный МЕТОД корпуса** (документ → 5 оракулов → эксперимент → 3-этапный learning-set → held-out/teaching,
с анти-выдумыванием EM-1..5 и license/source-gate). (6) **CB4 security-корпус (OWASP/NIST/CWE/IETF) + 75 реальных
center-пар + phase-doc-audit** (26/31 суб-фаз имеют источники) — первый реальный материал под обучение.

**Кратчайший путь до реально обученной оркестрационной единицы (одна рекомендация).** Соединить двигатель с
корпусом: **прогнать реальные пары папки №8** (75 center-пар + CB4 + build-ready пакеты ОДНОЙ хорошо-покрытой
суб-фазы, напр. 3.1 Architecture или 4.1 Backend) **через уже существующий `DefaultLogicalCapabilityTrainer` /
`LogicalDpoTrainingUnit`** — заменив синтетический `chosen/rejected` (`BuildChosenCapabilityText` `:680-683` /
фикс-rejected `:174`) на реальные A/B-строки корпуса. Это даёт первый end-to-end проход: реальный корпус → DPO/LoRA-
шаг → `policy.bin` checkpoint → held-out-замер → promoted probationary-юнит в реестре → retry через
`RunWithCapabilityGeneration`. Двигатель, реестр, checkpoint, continue-training, KNN-wrap уже написаны — не хватает
**топлива и одного held-out-замера**. Ещё один круг методологии — это process-about-process, а не продукт.

**Роль папки в свипе (cross-folder) и 5 открытых вопросов.** Папка №8 — это **полная лаборатория E51/E53**, которую
RAG-06 видел сбоку (вывод по E53 совпадает: детерминированная оркестрация), но pass 9 добавил то, чего RAG-06 не
видел — вживую собранный DPO-движок. Для **RAG-05** это прямой ответ на `REAL_TRAINER_ROUTE_B_GATE`: маршрут
`DpoTrainingHandoff → 11.DPO/DpoTrainer` существует и подключён (не хватает корпуса и held-out на нём). Для **RAG-02**
`LogicalOrchestratorAssistant` + logical-юниты (проекты 15/21-23) — это «прото-код», а живой `AdaptiveLogicalOrchestrator`
— их зрелая версия под тонкий host-адаптер. **5 открытых вопросов для Luba (с дефолтами):** (1) переиспользовать
готовую иерархию+инспектор или строить заново → *дефолт: переиспользовать*; (2) реальный-корпус-сначала или ещё
доводка метода → *дефолт: корпус-сначала (один end-to-end проход)*; (3) квота 3 100 vs меньший реальный сет → *дефолт:
явно принять меньший scope как осознанное понижение floor'а (сейчас понижен молча)*; (4) кормить трейнер реальными
парами вместо синтетики → *дефолт: да, заменить синтетику на A/B-строки корпуса*; (5) детерминированные оракулы
E53/CB4 — оставить как ablation-базлайн → *дефолт: да, пометить eval-only baseline, не финальная authority*.

**MUST-READ файлы (минимальный набор, если открывать не всё).** Для архитектуры расширения: `LOGICAL-UNIT-FLOW-
ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` (иерархия+фазы 25-31) и живые `21.AgentInfrastructure/Logical/
AdaptiveLogicalOrchestrator.cs` + `SubPhaseRouterUnit.cs`. Для трейнабл-двигателя (поворот): живые
`18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` + `LlmIngredientLogicalUnits.cs` (`LogicalDpoTrainingUnit`)
+ `11.DPO/DpoTrainer.cs`. Для корпуса/метода: `CORPUS-BUILD-READY-COMPLETION-SUMMARY-2026-05-05.md` +
`CORPUS-V4-FINAL-COMPLETION-AUDIT-2026-05-05.md` (честный «NO») + `WAVE-2-CENTER-PHASE-BATCH-001-NODE-02-DOMAIN-
TRANSLATOR-SET-v1.md` (эталон реальной пары) + `EXAMINATION-METHOD-ORCHESTRATION-v6.md` (форма пары/teaching).
Для доменных доказательств: `MASTER-EXPERIMENT-PLAN-v1.md` + `E53-ORCHESTRATION-PROOF-PLAN-v1.md` +
`CB4-STAGE-5-SECURITY-FLOW-REGISTRATION-PLAN-v1.md`. Для UI: `ORCHESTRATOR-INSPECTOR-DESIGN-v4-performance-first.md`.
Для целевого направления: `WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-PLAN-v4.md` + `LEARNING-CYCLE-IMPLEMENTATION-PLAN-v3.md`.

### 3. Честный отчёт о покрытии (финал папки №8)

- **Всего файлов: 513** (md 320 · zip 112 · cs 29 · json 25 · svg 9 · png 7 · csv 4 · mmd 3 · pdf 2 · html 2),
  94 директории. Все 12 семей §A–§L + слой метода + живой repo-код — **учтены, ни одна не пропущена** (таблица §1).
- **Прочитано ПОСТРОЧНО целиком за 10 проходов:** ~50+ md-документов (все ключевые планы/модели/методички/аудиты),
  **24 файла E53 D-K + 2 batch-findings**, **14 файлов T34-A/P3-A + report.md**, **8 WDA core-док**, **28
  ROUND-MANIFEST фаз 25-52**, **26+ ROUND-MANIFEST фаз 53-118** (deep), представители всех pair-классов Wave-1/2/center,
  2 CB4-json целиком + 14 через grep, 30-й и 9-й ledger, V4-audit; **и 9 реальных C#-файлов целиком** (E53-proof,
  P3-proof, T34-splitter, SubPhaseRouter, SecurityArbiter, SecurityFormatter, + 5 живых repo-файлов трейнабл-движка) +
  grep-верификация `DpoTrainer.StepVector`.
- **Закрыто по-паттерну / сэмплом (честно, не построчно):** ~40 ROUND-MANIFEST (по имени+тест-счётчику+Files-Changed),
  batch-pair-set tails 004-007, ledgers v1-v8/v10-v29, CB4 node-01…07 staging .cs, binding-варианты domain-mapping.
  Во всех случаях паттерн подтверждён ≥2 прочитанными представителями класса — исключений не найдено.
- **Сознательно НЕ открыто (не влияет на вердикт):** 9 SVG / 7 PNG (inspector-preview) / 3 MMD как изображения —
  охарактеризованы по имени+манифесту; 5 CSV аудита (отражены нарративно в report/matrix); массовая распаковка 112 zip
  (распакованы репрезентативно phase-48 + 7 round-zips stage-дир — содержимое = уже-прочитанные манифесты/.cs);
  report .html/.pdf/.VISUAL (те же данные, что .md).
- **Что НЕ запускалось (архитектор-only, read-only свип):** ни `dotnet build`, ни `dotnet test`, ни training-runner,
  ни распаковка всех zip. Тест-счётчики (997→1177) взяты из манифестов, не перегонялись. Вердикт trained-vs-deterministic
  опирается на **прочитанный исходный код** (9 .cs целиком) + манифесты + grep-аудит lifecycle-терминов.
- **Финальный вердикт папки №8 (двухуровневый):** внутри папки — **`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`** по всей дуге
  25-118 (все «trained» = детерминированные оракулы; обучение не запускалось; корпус 18 % с 87 % балласта); в живом
  repo — **реальный DPO/LoRA/checkpoint-двигатель (Route B подключён), но opt-in, tiny и на синтетике**. Ценность для
  плана продолжения расширений VS/VSCode: готовые переносимые активы (иерархия + движок + editable-topology + inspector +
  метод + первый реальный материал), кратчайший шаг — прогнать реальный корпус через существующий трейнер.
- **folder #8/12 RAG COMPLETE.**
