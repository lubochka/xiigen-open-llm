# Мастер-план ILearningDynamicGraph — v1 (сборка решённых блоков, dpo legal build)

> **Артефакт:** `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`
> **Автор-роль:** сборщик мастер-плана (Opus 4.8 MAX). Собирает 16 решённых блоков
> (Part A §0/§4/§5 + блоки P-01..P-14) в единый мастер-план.
> **Тип артефакта:** ПЛАН-ONLY (документация-решение). НЕ промпт исполнителю и НЕ
> разрешение на реализацию продукта/кода, тренировку, билд, запуск раннеров, git
> или вызов внешних AI/API/моделей. Реализация требует **свежего явного
> разрешения Luba** после approve этого плана (R2/CLAUDE.md «Continuous role loop»).
> **Язык:** русский (человеческим языком сначала); имена типов/полей — английский.
> **Цитаты кода:** только реально существующие, с `file:line`. NEW-типы помечены
> `NEW`; существующие — с `file:line`. Чужие `claude-*`/`dcm-*` артефакты —
> evidence-only, не редактируются и не продолжаются.

---

## CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY (anchor, по CLAUDE.md «Current task anchor»)

```text
CURRENT TASK:  собрать мастер-план ILearningDynamicGraph (§A заголовок+резюме,
               §B governance/orchestration, §C индекс Part A + блоков P-01..P-14
               как полностью решённых §1/§2/§3, §D ссылка на доску исполнения
               codex-EXECUTION-CONTROL-BOARD.md (единственный источник истины по
               execution-control Ф<P>.<SP>.<SS>) + ссылка на матрицу покрытия
               codex-REQUIREMENTS-COVERAGE-MATRIX.md, §F Индекс артефактов, §E
               финальная секция ВОПРОСОВ в самом конце) в
               codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md.
NOT THE TASK:  реализовывать/тестировать/билдить код, запускать раннеры, вызывать
               внешние AI/API/модели, git, продолжать/править чужой
               claude-ILEARNINGDYNAMICGRAPH-*/dcm-*/не-свои codex-файлы, дублировать
               содержимое блоков (мастер-план ИНДЕКСирует блоки, а не копирует их).
EVIDENCE-ONLY: codex-recon-*, codex-guide-*, codex-knowledge-library-*, чужой
               claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md, два codex-префиксных
               файла АВТОРСТВА CLAUDE (codex-50-cycle-plan-review-supplement.md,
               codex-skill-improvement-ideas-planning-review-architecture.md —
               benchmark/evidence, см. codex-BENCHMARK-DELTA-REPORT.md:26-35/56-57),
               концепт-доки llm dynamic flow/, транскрипты, STATE-файлы.
```

---

# §A. Заголовок, назначение, краткое русское резюме архитектуры

## A.1. Назначение мастер-плана

Этот документ — **единая точка входа** в план проекта `ILearningDynamicGraph`. Он
не повторяет содержимое решённых блоков, а **связывает** их: показывает Любе как
архитектору-человеку, из чего состоит план, в каком порядке его реализовывать, чем
доказывается каждое требование, и какие решения остаются за ней (финальная секция
вопросов §E).

Шестнадцать решённых блоков-документов (каждый — самостоятельный §1/§2/§3 со своей
UML, симуляциями, цитатами кода и Gate B) собраны здесь в одну карту:

- **Part A** (что Luba читает и одобряет): §0 UML всей системы + §4 статус
  (`codex-plan-00`), §5 иерархия уровней + цепочки исполнения (`codex-plan-05`).
- **P-01..P-14** (решённые блоки §1/§2/§3): узел, ребро, оркестратор, L5-машина,
  async-комбинации, DPO-строитель, легальный teacher-гейт, авто-DPO петля,
  обучение+lifecycle, измерение, замена скрытого детерминизма, тест-стратегия,
  карта референсов.

## A.2. Краткое русское резюме архитектуры (что мы строим)

Мы строим **обучаемый динамический граф** — модель, которая учится не одному
навыку, а **отношениям между навыками разного порядка**: какой узел кому передаёт
задачу, в каком порядке, и как сливать результаты. Архитектурно это **верхний
обучающий уровень (Level 5)** поверх уже существующей пятиуровневой иерархии
проекта — НЕ новый шестой уровень.

**Аналогия (общая для всех блоков) — «завод/оркестр с перекладываемыми лентами»:**

- **узел-станок** = `ILearningUnit` (≡ `ILogicalUnit`, L2): берёт деталь, отдаёт
  деталь, днём (в рантайме) сам не учится;
- **лента между станками** = `IDynamicGraphEdge` (NEW): бывает веером наружу
  (fan-out), веером внутрь (fan-in), много-ко-многим, много-выходов и петлёй
  возврата;
- **карта лент** = `DynamicGraphTopology` (NEW): хранится как данные в реестре,
  перекладывается на лету (это и есть «динамический» граф);
- **начальник цеха** = `ILearningOrchestrator` (≡ `ILogicalOrchestrator`, L4):
  читает карту, обходит станки и ленты, собирает итог;
- **директор с ночной сменой обучения** = `ILearningDynamicGraph`
  (≡ `ILearningMachine<DynamicGraphDomain>`, L5): единственный, кто имеет право
  перенастраивать станки по результатам приёмки (что приняли, что забраковали).

**Три жёстких правила, проходящих через весь план:**

1. **Обучение живёт только на L5** (A-LM-02): узлы/рёбра — чистый inference, Train
   только на машине.
2. **Запрет скрытого детерминизма** (A-LM-09 `DeterministicFinalAuthority=false`,
   A-LM-13): на каждом ребре финал принимает **обученный** узел, а
   детерминистический роутер/счётчик/словарь — либо генератор кандидатов, либо
   честно помеченный `baseline/eval-only`. Главный кандидат на замену —
   `MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02,
   `MultiSubPhaseRouterUnit.cs:283`, `isLearned:false`).
3. **Легальный teacher-гейт** («dpo legal build»): дистилляция — только open-weight
   permissive (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 modified-MIT / GLM /
   Mistral) через нейтральных провайдеров. Claude/OpenAI/Gemini как teacher для
   дистилляции — **никогда** (только judge, и judge не цель обучения).
   Контаминированные строки → `DATASET_REBUILD_REQUIRED`.

**Текущее честное состояние системы (две PASS-точки, CLAUDE.md):**
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — контракты пяти уровней работают, скрытого
детерминизма «под видом trained» нет (роутер честно `isLearned:false`), но
обученного DynamicGraph (checkpoint/ablation/fresh-load) ещё **нет**. Это
**НЕ** `PASS_AS_TRAINED_FEATURE`.

## A.3. Маппинг ролей Luba → реальные контракты (CANON)

| Имя Luba | Реальный контракт | Уровень | file:line |
|---|---|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` | L2 | `00.Core/LogicalContracts.cs:97` |
| `ILearningFlow` | `ILogicalFlow<TInput>` | L3 | `00.Core/LogicalContracts.cs:166` |
| `ILearningOrchestrator` | `ILogicalOrchestrator` | L4 | `00.Core/LogicalContracts.cs:202` |
| `ILearningDynamicGraph` | `ILearningMachine<DynamicGraphDomain>` поверх динамического графа | L5 | `00.Core/LearningMachineContracts.cs:208` |

A-LM-12 (`docs/DECISIONS-LOCKED.md:35`) — закрытый набор из 5 уровней; новый 6-й
уровень НЕ вводим. DynamicGraph = специализация L5 (домен `TDomain`). Ведущий
вариант; финальное подтверждение уровня — в §E (вопрос Q1).

## A.4. Семь NEW-типов (проектируются в плане; в коде их НЕТ)

| # | NEW-тип | Что это | Решён в блоках |
|---|---|---|---|
| 1 | `DynamicGraphDomain` | sealed-маркер `TDomain` для L5-машины графа | 00, 04, 05 |
| 2 | `IDynamicGraphNode` | обёртка над узлом-`ILogicalUnit` (без Train, A-LM-02) | 00, 01, 04, 05 |
| 3 | `IDynamicGraphEdge` (+`GraphEdgeKind` — КАНОН §A.4.1) | типизированное ребро fan-out/fan-in/many-to-many/multi-output/return-loop + per-edge обучаемая authority | 00, 02, 03, 04, 05 |
| 4 | `DynamicGraphTopology` | мутируемая на лету карта узлов/рёбер (проекция реестра топологии) | 00, 03, 04, 05 |
| 5 | `EdgeAuthorityUnit` (КАНОН §A.4.2; вход `EdgeAuthorityInput`→выход `EdgeAuthorityDecision`) | обучаемый финальный решатель ребра (заменяет baseline-роутер) | 02 (владелец канона), 03, 04, 12 |
| 6 | `GraphDpoPair` | канон DPO-пары графа (объединяет 5 существующих форматов) | все блоки; канон в 07 |
| 7 | `TeacherProvenance` | поля легального учителя (`closed_proprietary_model` должно быть false) | все блоки; гейт в 08 |

> Канон `GraphDpoPair` (предложение, утвердить в §E): `pair_id; level(unit|edge|graph);
> node_or_edge_id; input_context; chosen; rejected; wrong_markers([WRONG:...]);
> reason; teacher_provenance{...}; source_ids[]; held_out; candidate_order_invariant;
> excluded_leakage_signals[]; dpo_train_ready(gate)`.
>
> Поля `TeacherProvenance` (КАНОН — владелец блок 08, синхрон с `codex-plan-08`
> §5/§8.4/§10 п.1; утвердить в §E): **18 полей = 13 базовых + 5 NEW бенчмарк-дельт
> T2/T3/T5**. 13 базовых: `teacher_access_mode; hosting_provider; model_id;
> model_license; model_version; request_id; provider_terms_checked;
> teacher_output_used_for_training; closed_proprietary_model; source_ref;
> source_sha256; contamination_markers; judge_model` (поле `judge_model` —
> **ВНУТРИ** `TeacherProvenance`, **НЕ** отдельным верхнеуровневым полем; CANON
> блок 07 §5.1, блок 08 note). 5 NEW: `terms_snapshot_ref` (T2),
> `geography_export_constraint` (T2), `source_category` (T5),
> `raw_private_text_copied` (T3), `redaction_policy_ref` (T3). Вердикт-enum:
> `ACCEPT|QUARANTINE|BLOCK|DATASET_REBUILD_REQUIRED|BLOCK_FOR_COMMON` (NEW
> `BLOCK_FOR_COMMON` — T3 §8.5: приватный текст в hosted без редакции → только
> adaptive leg). **Предусловие `dpo_train_ready=true`:**
> `provenance_field_presence_rate == 1.0` по **всем 18 полям** (синхрон с
> `codex-plan-08` Gate B п.1). При любом расхождении числа полей или размещения
> `judge_model` между этим §A и блоком 08 — **владелец канона блок 08**.

## A.4.1. КАНОН ребра — `GraphEdgeKind` (enum видов ребра)

> Закрывает расхождение четырёх блоков. На диске один тип ребра объявлен под
> **четырьмя** разными именами/наборами: `DynamicGraphEdgeKind{FanOut,FanIn,
> ManyToMany,MultiOutput,Sequential}` (`codex-plan-04` §5.2),
> `EdgeKind{OneToOne,FanOut,FanIn,ManyToMany,MultiOutput}` (`codex-plan-02` §5),
> `GraphEdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,Sequential,Bridge}`
> (`codex-plan-03` §5), `EdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,BackEdge,
> AsyncParallel}` (`codex-plan-05` §5.2). Доска Ф1.1.2 требует Gate B «5 видов
> ребра в enum». Эта таблица **решает** расхождение, а не фиксирует его.

**КАНОН-имя:** `GraphEdgeKind`. **КАНОН-набор — ровно 5 видов** (совпадает с Gate B
доски Ф1.1.2 «5 видов ребра» и с аналогией §A.2 «веер наружу/внутрь/много-ко-многим/
много-выходов/петля возврата»):

```text
public enum GraphEdgeKind { FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop }
```

**Решение по спорным членам (какие — kind ребра, какие — деталь обхода/флаг, НЕ kind):**

| Спорный член | Где встречался | Решение | Куда переходит |
|---|---|---|---|
| `ReturnLoop` (=`BackEdge`/петля возврата) | 04(Sequential≠это), 05(`BackEdge`) | **kind ребра** (5-й канонический) | `GraphEdgeKind.ReturnLoop` |
| `Sequential` | 03, 04 | **НЕ kind**: порядок обхода зависимостей | деталь обхода `DomainGraphClosure`/`DependsOn`, не член enum |
| `OneToOne` | 02 | **НЕ kind**: вырожденный `FanOut` с одной целью | покрывается `FanOut` (1 цель) |
| `Bridge` | 03 | **НЕ kind**: knowledge-edge | флаг/контракт `KnowledgeEdge` на ребре, не член enum |
| `AsyncParallel` | 05 | **НЕ kind**: режим исполнения | булев флаг `ParallelEligible` ребра, не член enum |

**Обязательность ссылок:** блоки `codex-plan-02/03/04/05` §5 и Part A §0
(`codex-plan-00`) обязаны ссылаться на этот единый `GraphEdgeKind` (5 членов);
локальные имена `DynamicGraphEdgeKind`/`EdgeKind`/`GraphEdgeKind{...Bridge}`/
`EdgeKind{...BackEdge,AsyncParallel}` — **устаревшие псевдонимы**, расхождение
числа членов = дефект. Это приводит число членов в соответствие с Gate B доски
Ф1.1.2 (ровно 5) и делает exit-gate Ф1.1 достижимым.

## A.4.2. КАНОН решателя ребра — `EdgeAuthorityUnit` (per-edge authority)

> Закрывает расхождение трёх I/O-сигнатур и двух имён. На диске обучаемый финальный
> решатель ребра объявлен как: `EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput,
> EdgeAuthorityDecision>` **и одновременно** `IDynamicGraphEdgeAuthority :
> ILogicalUnit<EdgeRoutingContext,EdgeRoutingDecision[]>` (оба в `codex-plan-02`
> §5); `ILogicalUnit<EdgeContext,EdgeDecision[]>` (`codex-plan-03`/`codex-plan-05`
> §5). Для одного R4-арбитра ребра допустим **только один** вход/выход (A-LM-09).

**КАНОН-имя:** `EdgeAuthorityUnit` (владелец канона — блок 02 `codex-plan-02` §5).
**КАНОН I/O (одна пара):**

```text
EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
  вход  EdgeAuthorityInput   — leakage-free: БЕЗ candidate-index/позиции в списке,
                               БЕЗ SourcePath (НЕ SubPhaseRouterSourceDocument);
                               нормализация до вызова
  выход EdgeAuthorityDecision — упорядоченные цели many→many (OrderedTargets[] +
                               PerTargetConfidence[]); Weight ребра = eval-only,
                               НЕ авторитет
```

**Устаревшие псевдонимы (обязаны ссылаться на канон):** `IDynamicGraphEdgeAuthority`,
пара `EdgeContext`/`EdgeDecision[]`, пара `EdgeRoutingContext`/`EdgeRoutingDecision[]`
— **deprecated**; блоки `codex-plan-02/03/05/12` и Part A §0/§5 обязаны привести
сигнатуру к канону. Внутреннее противоречие в `codex-plan-02` §5 (там сейчас два
контракта) снимается пометкой: канонический — `EdgeAuthorityUnit :
ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>`; `IDynamicGraphEdgeAuthority`
— устаревший псевдоним.

**КАНОН поля-владельца авторитета на ребре.** На диске тип поля не согласован:
`string AuthorityUnitRef` (`codex-plan-03` §5), `string? AuthorityUnitId`
(`codex-plan-04` §5.2), `object AuthorityUnit`/`ILogicalUnit AuthorityUnit`
(`codex-plan-02`/`codex-plan-05` §5, Part A §0). **КАНОН-решение (закрывает Q13/Q15
одним значением):** на ребре хранится **`string AuthorityUnitId`** — ссылка в
реестр (`InMemoryLogicalUnitRegistry`), резолв обученного владельца через
`ResolveActiveUnit`/latest-wins (A-LM-03), а **не** прямой объект-юнит. Физическое
место хранения ссылки — **поле ребра в `DynamicGraphTopology`** (проекция реестра
топологии), не отдельный реестр владельцев; это и определяет, где живёт авторитет
при export/import графа (блок 04 §3.4/§10). Блоки `codex-plan-02/03/04/05` и Part A
§0 обязаны использовать `string AuthorityUnitId` единообразно; `object/ILogicalUnit
AuthorityUnit` и `AuthorityUnitRef` — устаревшие псевдонимы. Вопросы Q13/Q15 (§E)
закрыты этим каноном.

---

# §B. GOVERNANCE / оркестрация-блок плана

> По CLAUDE.md «Plan governance orchestration pattern». Этот блок делает мастер-план
> само-управляемым: фиксирует требования, их маппинг на блоки/Gate B/артефакты,
> роль-loop, branch-правило, разрешённую верификацию, матрицу завершения, anti-stop
> и score-правила. Без этого блока мастер-план неполон.

## B.1. Authority Requirements Ledger (требования из задания Luba)

Источник полномочий: текущее задание Luba на сборку мастер-плана ILearningDynamicGraph
(CANON-блок задания) + 3 находки рекона. Каждое требование — binding.

| ID | Требование (из задания Luba / CANON / рекона) | Блок-владелец | Gate B (краткий) | Артефакт-доказательство |
|---|---|---|---|---|
| R-01 | Роли узел/поток/оркестратор/динам-граф = L2/L3/L4/L5 (маппинг ролей Luba) | 00, 05, 01, 02, 03, 04 | счётчик новых уровней = 0; маппинг с file:line | §C индекс + блоки 00/04/05 |
| R-02 | Авто-построение DPO из НЕсуществующих данных (генератор пар, не ручной набор) | 06, 07, 09 | BuildPairAsync на фикстуре даёт валидный `GraphDpoPair`; 5 форматов → 1 канон | `codex-plan-07`, `codex-plan-09` |
| R-03 | Teacher как ЦЕЛЬ дистилляции = только легальный open-weight permissive | 08, 07, 10, 14 | `closed_proprietary_model=true && used_for_training=true ⇒ BLOCK/REBUILD` | `codex-plan-08` гейт |
| R-04 | Judges из лучших моделей (включая closed) — только как СУДЬИ, не цель обучения | 06, 07, 08, 09, 12 | `judge_model` пишется отдельно; 0 train-ready строк с teacher=closed | `codex-plan-08` (C6/C7), `codex-plan-06` |
| R-05 | Легальный teacher-гейт + provenance как обязательное поле строки | 08 (владелец), все | **18 полей `TeacherProvenance`** (13 базовых + 5 NEW бенчмарк-дельт T2/T3/T5: terms_snapshot/geography_export/source_category/raw_private_text/redaction_policy); `provenance_field_presence_rate==1.0` по всем 18 — предусловие `dpo_train_ready`; манифест содержит только `verdict==ACCEPT` (enum +`BLOCK_FOR_COMMON`) | `codex-plan-08` §5/§8.4/§10 п.1 |
| R-06 | Async мульти-комбинации (один вход → N комбинаций → лучший) | 06 (владелец), 03, 05 | для budget.Max=4 стартует ровно 4 async; победитель != немаркированная формула | `codex-plan-06` |
| R-07 | Запрет скрытого детерминизма на ребре (A-LM-09/A-LM-13) | 12 (владелец), 02, 03, 04 | classified_mechanism_count==9; 0 немаркированных финальных авторитетов | `codex-plan-12` |
| R-08 | Обучаемость + lifecycle (LoRA+DPO, checkpoint/export/import/continue) | 10 (владелец), 04, 11 | fresh-load==trained; continue-training Δ>0; export/import round-trip SHA-256 | `codex-plan-10` |
| R-09 | Измерение скорость/точность/качество (held-out, ablation, без UNKNOWN) | 11 (владелец), 13 | `PASS_AS_TRAINED_FEATURE` только при held-out+ablation+lifecycle; 0 UNKNOWN | `codex-plan-11` |
| R-10 | SPLIT R5: общая модель/логика/сеты в llm_mvp_core; адаптивная — у пользователя | 04, 10, 14, 05 | манифест несёт `LegScope`; `DynamicGraphMachinePair` разделяет common/personal | `codex-plan-10` (9), `codex-plan-04` |
| R-11 | Тонкий агент R6 (агентный слой — только вызовы API, ядро в llm_mvp_core) | 14, 05 | 0 строк карты переносят алгоритм-логику в агентный слой | `codex-plan-14`, `codex-plan-05` §9 |
| R-12 | codex-префикс на всех артефактах плана | мастер + все блоки | имя каждого собственного файла начинается с `codex-`; полнота индекса §F покрывает **ВСЕ** `codex-*` файлы базы (3 управляющих §F.1 + 15 `codex-plan-*` §F.2 + 5 гайдов §F.3 + 2 библиотеки §F.4 + **63** рекон-файла §F.5 + §F.6, где (a) собственный `codex-BENCHMARK-DELTA-REPORT.md` + (b) 2 codex-префиксных файла **авторства Claude** = evidence-only) | §F (полный индекс) |
| R-13 | План пишется через суб-агентов (Part A полнота §0..§5, branch-parity) | мастер (сборка) | §C индекс §0/§4/§5 + §1/§2/§3 присутствуют | §C |
| R-14 | «Вопросы в конец» (все открытые вопросы — в финальной секции) | мастер §E | секция вопросов в самом конце документа | §E |
| R-15 | Полный набор §0..§5 Part A на уровне глубины (Part A Completeness Contract) | 00, 05 + P-01..P-14 §1/§2/§3 | §0 note for на каждый класс + симуляции; §1 класс+метод+параметры+шаги | §C |
| R-16 | Тест-стратегия по 9 слоям FC-26, untrained-honesty + trained-acceptance раздельно | 13 (владелец), все блоки §9 | матрица 14 строк P-01..P-14, 0 пустых клеток, 3 IsTrainable c обеими колонками | `codex-plan-13` |
| **Находки рекона** | | | | |
| R-17 (рекон-1) | Контаминированные строки `sonnet-*` в `training-data/orchestration-reasoning/` (08: 19 файлов/72 вхождения; 07: 35 совпадений/10 файлов) → DATASET_REBUILD | 08, 07 | `contaminated_rows_marked_REBUILD == contaminated_rows_found` | `codex-plan-08` (2) |
| R-18 (рекон-2) | Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») — нужен канонический перед обучением | 07, 09 | назван канонический указатель | §E Q (блок 07) |
| R-19 (рекон-3) | Захардкоженный `RewardMargin` (литерал 0.03) в `TranslationMachine.cs:70/123-126/187` — эталон обучения сломан | 10, 11 | литерал 0.03 как RewardMargin устранён; margin из движка `DpoTrainer.cs` | `codex-plan-10` (2), `codex-plan-11` |
| R-20 (current-turn branch authority) | Во всех участвующих репозиториях source authority перед началом реализации = самый последний updated branch; рабочее имя новой ветки для будущей реализации = `DPOLegalLearning` | мастер, доска, матрица | перед implementation work-order executor фиксирует для каждого repo: repo path/url, selected latest-updated branch, timestamp/ref evidence, divergence note, proposed new branch `DPOLegalLearning`; без этой таблицы Ф1 не стартует | `codex-EXECUTION-CONTROL-BOARD.md` §0.3.1; `codex-REQUIREMENTS-COVERAGE-MATRIX.md` R21 |

> **Сверка нумераций (binding): мастер `R-01..R-20` ↔ задание Luba `R1..R21`.**
> Этот ledger §B.1 — внутренний срез мастер-плана (`R-01..R-20`, где
> `R-17/R-18/R-19` — находки рекона; `R-20` — current-turn branch authority).
> Доска `codex-EXECUTION-CONTROL-BOARD.md` §C и матрица
> `codex-REQUIREMENTS-COVERAGE-MATRIX.md` трассируют **задание Luba `R1..R21`**.
> Полная карта соответствия двух нумераций — в матрице §5 п.1
> (источник карты). Два требования задания живут в доске, а **НЕ** в таблице выше,
> и оба **binding (не optional)**:
>
> | Требование задания | Где живёт (владелец) | Gate B | Статус |
> |---|---|---|---|
> | **R20** — до 50 проходов review/fix | мастер §E входные замечания + доска §D Review-Fix протокол; Ф11.2 (10 execution reviews при реализации) | реальные ревью-пакеты по доске §D.1 (`real_pass_count`: reviewer id, before/after ref, findings, fix, recheck); ledger-строки ≠ циклы | **covered/current-run complete**: текущий ledger `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md` имеет 50/50 returned packet records после C49-C50 integration; старые 50/50 ledgers — evidence-only для этого текущего сравнения |
> | **R19 (Luba, execution-visibility)** — строгая последовательность + полная execution-видимость + нумерация `Ф<P>.<SP>.<SS>` | доска §0.1 нумерация, §0.3 sequential gating, §A exit-gates, §B EXECUTION-VISIBILITY | каждый ID уникален без дыр; в любой момент виден текущий `Ф<P>.<SP>.<SS>` + счётчики done/total | covered доской |
>
> Внимание: `R19` мастера (находка рекона train-ready V192/V309) и `R19` задания
> Luba (execution-visibility) — **разные** требования с совпавшим номером; не
> путать (см. также §F intro про `R-18` vs codex-префикс).

## B.2. Contradiction sweep (противоречий нет)

Сквозная проверка терминами ledger по всем блокам: ни одно предложение мастер-плана
или блока не противоречит binding-требованию.

- «обучаемый граф» vs «без нового уровня» — не противоречие: DynamicGraph =
  специализация L5 (R-01, A-LM-12), не 6-й уровень.
- «teacher» vs «judge closed» — не противоречие: teacher только open-weight (R-03),
  judge может быть closed но не цель обучения (R-04).
- «детерминистический обход порядка рёбер ОК» vs «запрет детерминизма» — не
  противоречие: запрет относится к **финальной authority выбора ребра** (R-07),
  а порядок зависимостей (`DomainGraphClosure`) — honest helper без claim.
- «удалить baseline-роутер» — отсутствует и запрещён: удаление ≠ покрытие замены;
  `MultiSubPhaseRouterUnit` понижается до baseline/ablation, не удаляется
  (`codex-plan-12` Q5).

## B.3. Main focus goal и проверка шагов

**Main focus goal:** обучаемый динамический граф (L5 над узлами/рёбрами) с авто-DPO
петлёй на **легальных** teacher-данных, с доказанной обучаемостью (held-out +
ablation + lifecycle) и SPLIT R5/тонким агентом R6 — как **план-решение** (не
реализация). Каждый подшаг `Ф<P>.<SP>.<SS>` доски исполнения
(`codex-EXECUTION-CONTROL-BOARD.md`, см. §D) сверяется с этим goal колонкой «что
решает».

## B.4. Branch-правило, allowed verification, forbidden tests

- **Current parent-session branch note:** текущая ветка плановой сессии
  (`codex/chat-gpt-5.5`) не является source authority для реализации. НЕ
  переключать; `git checkout` как «фикс» запрещён (CLAUDE.md branch convention).
- **Implementation branch authority amendment (2026-06-24):** для всех участвующих
  репозиториев перед стартом реализации authoritative source ref выбирается как
  **самый последний updated branch** по inspectable branch metadata. Рабочее имя
  новой ветки реализации: `DPOLegalLearning`. Это плановое правило не создаёт ветку
  сейчас и не разрешает `git checkout`; оно добавляет pre-flight Gate B: repo-by-repo
  таблица `repo -> latest updated branch -> evidence timestamp/ref -> divergence note`.
- **Candidate participating repo identifiers for that pre-flight:** `llm_mvp_core`,
  `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`,
  `FigmaToCode-design-gen`, `CanvaToCode`. Executor must verify/update this list
  from actual repo paths/URLs before selecting authority branches.
- **Allowed verification для плана:** чтение артефактов (Read), счётные проверки
  Gate B по тексту блоков (note for на каждый класс, число связей в таблице
  симуляции, число NEW-типов, число открытых вопросов).
- **Forbidden:** `dotnet test`/`dotnet build`/`dotnet run`, раннеры, тренировка,
  git-запись, внешние AI/API/модели — это план-only, не реализация (Rule 4: full
  test run не ритуал для документации-фазы).

## B.5. Роль-loop (внутренние состояния, не user-facing паузы)

```text
ARCHITECT_INTERNAL -> EXECUTOR_INTERNAL -> REVIEW_INTERNAL
   -> ARCHITECT_INTERNAL_REPAIR -> EXECUTOR_INTERNAL_CONTINUE
```

Для этого мастер-плана (документ-сборка) активная роль — автор-сборщик блока плана.
Реализация продукта по строго последовательной доске исполнения
(`codex-EXECUTION-CONTROL-BOARD.md`, см. §D) начинается только после **свежего
approve Luba** этого плана (R2) и идёт через bounded суб-агентов под
ARCHITECT_INTERNAL (CLAUDE.md «Architect-owned session»).

## B.6. Требуемые скилы (по фазам жизненного цикла плана)

| Фаза | Скилы/гайды |
|---|---|
| Создание плана | `planning-session-startup-SKILL.md`, `implementation-doc-uml/algorithms/code-refs/status/hierarchy-SKILL.md` |
| Ревью плана | `plan-review-SKILL.md` (FC-14/FC-20..FC-28/FC-33), `architect-behavior-classifier-SKILL.md` (FAIL-25..36/FAIL-49) |
| Исполнение | `skill-injection-SKILL.md` (SK-VERIFY/SK-QA), `generated-code-review-SKILL.md` Layers 1-5, `implementation-integrity-SKILL.md` |
| Код-ревью | `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, Deterministic-Boundary Code Review Gate |
| Docs-sync | «Documentation follows code» (CLAUDE.md) |
| Завершение | `phase-completion-packager-SKILL.md` (two-PASS gate), Completion coverage matrix |
| Трейнабл-решения | `trainable-architecture-decision-SKILL.md`, `dpo-triple-format-SKILL.md` |

## B.7. Document RAG lookup order (для незакрытых вопросов)

1. Активный план (этот мастер + 16 блоков).
2. Document RAG плана: `docs/ARCHITECTURE.md`, `docs/COMPONENT-INTERFACES.md`,
   `docs/DECISIONS-LOCKED.md`, `docs/EXPERIMENT-RESULTS-MAP.md`,
   `docs/SUBPROJECT-MAP.md`.
3. Локальные контракты `00.Core/*` и реестр/репорт-артефакты (25.ModelServer,
   18.CapabilityTraining).
4. Реконы/гайды `codex-recon-*`/`codex-guide-*` (evidence-only).
5. Если ответа нет — вопрос идёт в §E (не пауза, если безопасный default выводим).

## B.8. State-поля и точки обновления

`STATE.json` (при реализации): `main_focus_goal`,
`current_phase_substep (Ф<P>.<SP>.<SS>)`, `next_action`,
`goal_reached(=false до полного завершения)`, `stop_allowed(=false)`,
`plan_completion_hole_register[]`, `reviewed_commit`, `two_pass_status`
(scaffold/trained per узел). Источник `phase_queue` и статусов исполнения —
доска `codex-EXECUTION-CONTROL-BOARD.md` (§D); обновляется на каждом переходе
подшага `Ф<P>.<SP>.<SS>` по протоколу видимости доски §B.5.

## B.9. Anti-loophole register и repair path

- False-completion при открытом плане → `-1000`, запись в hole register, ремонт
  governance, продолжение от первого hole (CLAUDE.md «Plan-incomplete finish
  recovery»).
- Soft-stop при известном next_action → `-1000` (Partial-progress stop firewall).
- Скаффолд, заявленный как trained → FAIL `two_pass_conflation` (OP-57/FC-28).
- Детерминистический финал под claim «обучено» → `FORBIDDEN_RUNTIME_SHORTCUT`
  (A-LM-09).

## B.10. Score-правила (краткий свод)

| Нарушение | score_delta |
|---|---|
| False/soft stop при известном next_action | -1000 |
| Прямой write архитектором под sub-agent-only | -1000 |
| Ответ инструментом до settings-audit на challenge Luba (R8) | -1000 |
| Repair артефакта до идентификации skill-gap | -500 |
| Part A core != Part B core (OP-49) | -500 |
| План-задача → выдан продукт-код вместо плана (R2) | -500 |

## B.11. Commit/push — чекпойнт, не user-facing стоп

Один commit на фазу (Rule 6), точный commit-group из ledger (CLAUDE.md «Commit
groups»), верификация-перед-коммитом — хард-гейт. Для этого мастер-плана git —
**вне scope** (NOT THE TASK).

---

# §C. Part A — индекс (§0/§4/§5 + блоки P-01..P-14 как решённые §1/§2/§3)

> Мастер-план не дублирует блоки. Каждая строка ниже — указатель на полностью
> решённый блок-документ с его собственными UML, симуляциями, цитатами кода и
> измеримым Gate B. Все пути — в базе `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.

## C.1. §0 — UML всей системы + §4 — статус → `codex-plan-00-partA-uml-status.md`

- **Содержит:** полная Mermaid classDiagram всей системы (25 классов в 6 группах,
  `note for` на каждом), таблица симуляции E1..E31 (вход→результат, три ребра
  авторитета E10/E15/E18 различают `authority_now` baseline vs `authority_target`
  trained), §4 статус-таблица всех 25 компонентов (honest: ОТСУТСТВУЕТ trained
  DynamicGraph, BASELINE `MultiSubPhaseRouterUnit`).
- **Gate B (измеримо):** 25/25 классов с `note for`; 31/31 связей в таблице; 0
  NEW без сигнатуры; 0 существующих ссылок без file:line; импорт Mermaid в draw.io
  без правок.

## C.2. §5 — Иерархия уровней + помеченные цепочки → `codex-plan-05-partA-hierarchy.md`

- **Содержит:** ровно 5 уровней с типами Input/Output (file:line для существующих,
  NEW для новых); 4 помеченные цепочки расписаны пошагово — (a) inference
  L5→4→3→2, (b) обучение только на L5, (c) авто-DPO петля, (d) async
  мульти-комбинация; таблица симуляции S1..S12; решение «где хранить
  GraphDpoPair/TeacherProvenance» (проекция на `LearningTrainingItem` + боковой
  DPO-манифест, 0 правок контрактов 00.Core).
- **Gate B (измеримо):** 5/5 уровней с Input/Output; 4/4 цепочки пошагово; S1..S12
  симуляций; 0 немаркированных детерминистических финалов; `TeacherProvenance`
  present; 0 правок сигнатур L1..L5.

## C.3. §4 — статус (источник) → `codex-plan-00-partA-uml-status.md` §7

Статус-таблица всех компонентов (работает / BASELINE / NEW / ОТСУТСТВУЕТ) живёт в
блоке 00 §7. Двух-PASS вывод системы: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

## C.4. Блоки P-01..P-14 (полностью решённые §1 алгоритмы / §2 описания / §3 цитаты кода)

> Каждый блок несёт §1 (алгоритм: класс + метод + параметры + шаги), §2 (описание:
> аналогия + структуры данных + «что НЕ делает» + сигнатуры), §3 (цитаты кода
> file:line + сейчас/почему/должно стать/как менять), §4 статус, §5 контракты,
> §8 гейты, §9 тест-стратегия FC-26, §10 Gate B, §12 вопросы.

| Блок | Файл | Уровень/тема | Ключевой Gate B (измеримо) |
|---|---|---|---|
| **P-01** | `codex-plan-01-node.md` | Узел графа: `IDynamicGraphNode` поверх L2 `ILogicalUnit`, «упрощённая матрица» features·weights→top-N + honest NoDecision | Train-методов на узле = 0 (A-LM-02); 3 гейта (legal/no-shortcut/anti-leakage); миграция 3 классов случаев с file:line; 9 слоёв FC-26 c UH+TA |
| **P-02** | `codex-plan-02-edge.md` | Ребро графа: L3 `ILogicalFlow` + обучаемая per-edge authority | `OrderedTargets` инвариантен к 5 перестановкам (PASS); held-out `PassesGate`, RewardMargin>0, After>Before; ablation просадка ≥ порог; `exactPromptLeakageRate==0.0`; lifecycle fresh-load/continue/export-import |
| **P-03** | `codex-plan-03-orchestrator.md` | Оркестратор L4 (async): обход графа fan-out/fan-in/петля/knowledge-edge | G1..G8 = 8/8; обход с file:line (fan-out :316, петля :433, fan-in :872 и т.д.); подмена baseline→trained без нового кода в RunAsync (`ResolveActiveUnit`:269, latest-wins, A-LM-03); 0 скрытых финалов на L4 |
| **P-04** | `codex-plan-04-dynamicgraph-machine.md` | L5-машина `ILearningMachine<DynamicGraphDomain>` | маппинг с file:line (`:208`); каждое ребро с обучаемым владельцем + baseline-роль; 5 методов пошагово; 7 NEW-типов; число рёбер-с-authority == числу типов рёбер |
| **P-06** | `codex-plan-06-async-combinations.md` | Async мульти-комбинации (1 вход → N → лучший) | budget.Max=4 → ровно 4 async (`Task.WhenAll`); победитель != немаркированная формула (ablation); из N≥2 строятся `GraphDpoPair[]` overlap<0.60, order-invariant; ветки B/C honest |
| **P-07** | `codex-plan-07-dpo-builder.md` | Авто-строитель DPO: канон `GraphDpoPair` | 5 форматов → 1 канон (integration-тест); `BuildPairAsync` overlap<0.60, reason≥24; `TeacherProvenanceGate` closed→`DATASET_REBUILD_REQUIRED`; permutation order-invariant; QA-скан 0 train-ready с closed-маркерами; материализация в `DpoDataset.Load` (`11.DPO/DpoDataset.cs:18-26`) |
| **P-08** | `codex-plan-08-legal-teacher-gate.md` | Легальный teacher-гейт + provenance | **18 полей `TeacherProvenance`** (13 базовых + 5 NEW T2/T3/T5: terms_snapshot/geography_export/source_category/raw_private_text/redaction_policy); `provenance_field_presence_rate==1.0` по всем 18; `contaminated_rows_marked_REBUILD == found`; **14 веток C2-C14** вердикта (incl. C6a terms_snapshot, C6b geography, C13 privacy/redaction ⇒ `BLOCK_FOR_COMMON`, C14 source_category QA); accept с пустым provenance == 0; closed-judge при `used_for_training=false` ⇒ ACCEPT; манифест только `verdict==ACCEPT` |
| **P-09** | `codex-plan-09-auto-dpo-loop.md` | Авто-DPO петля (сердце проекта) | в `TrainBatchAsync` ≥1 авто-пара (не []); legal+anti-leakage PASS; held-out reward≥0, loss<0.693, After≥Before+0.02; промоушен только при росте; `ShouldStop` из {threshold,plateau,budget}; honest two-PASS метка |
| **P-10** | `codex-plan-10-training-lifecycle.md` | Обучение+lifecycle: LoRA+DPO, починка заглушек, split R5 | 10 строк: LoRA реально вклеена (ablation Δ≠0); литерал 0.03 устранён; continue-training Δ>0 ≥2 шага; export/import SHA-256; fresh-load без replay; closed→REBUILD; манифест `LegScope`+pair common/personal; 0 UNKNOWN |
| **P-11** | `codex-plan-11-measurement.md` | Измерение: скорость/точность/качество | `GraphMeasurementReport` PASS_AS_TRAINED только при: held-out source-disjoint + Δ≥+0.02 + reward≥0.025 + ablation + 5 leakage/permutation контролей + lifecycle; 0 UNKNOWN; live margin никогда как приёмочная точность |
| **P-12** | `codex-plan-12-no-hidden-determinism.md` | Классификация и замена скрытого детерминизма | classified_mechanism_count==9; каждый forbidden/baseline имеет blueprint с trainable-классом (`SemanticEdgeRouterUnit` NEW), typed state, training-algo, excluded-leakage, `CandidateOrderLeakageGuardUnit` NEW, ablation; every quote has file:line; 0 новых уровней |
| **P-13** | `codex-plan-13-test-strategy.md` | Консолидированная тест-стратегия (FC-26) | матрица 14 строк P-01..P-14, 0 пустых клеток (9 колонок слоёв); каждая ✔ с assertion/Gate; 3 IsTrainable c обеими колонками UH+TA; P-12 continue-training прогоном; 6 гейтов §8 сопоставлены |
| **P-14** | `codex-plan-14-stage-references.md` | Карта референсов (Stage 2) | count(distinct StageId)==13 (P-01..P-13); 6 осей-проектов ≥1 строкой; 34/34 путей существуют; teacher/router строки помечены legal/baseline; 0 строк на claude-*/dcm-* как target |

> **Branch-parity (Part A core == Part B core):** мастер-план держит Part A (§0/§4/§5
> + §1/§2/§3 в блоках) как единый источник истины. Part B (work-orders/раннеры/gate-
> команды) появляется при реализации и не должен вводить компоненты, невидимые в
> Part A, или иной алгоритм (OP-49). На стадии плана Part B ещё не написан —
> реализация требует approve Luba.

## C.5. Зависимости между блоками (что на что опирается)

```text
00 (UML/статус) ── задаёт карту ──> все блоки
05 (иерархия)   ── задаёт уровни ──> 01,02,03,04
01 (узел L2) ─┐
02 (ребро L3)─┼─> 04 (L5-машина) ──> 03 (оркестратор L4 как inference-вход)
03 (L4)      ─┘                         │
04 ── GetOrchestratorAsync ────────────┘
07 (DPO-строитель) ──> 09 (авто-DPO петля) ──> 10 (обучение+lifecycle)
08 (legal teacher) ── гейт ──> 06,07,09,10,11
06 (async N-комбинаций) ──> 07 (строит пары из кандидатов)
11 (измерение) ── приёмка ──> 09,10
12 (no-determinism) ── классификация/замена ──> 01,02,03,04
13 (тест-стратегия) ── поперечно ──> P-01..P-14
14 (референсы) ── навигация ──> P-01..P-13
```

---

# §D. Последовательность реализации → доска исполнения + матрица покрытия

> **Изменение v1.1 (дедуп).** Раньше §D держал собственную таблицу
> `ordered_phase_queue` (PH-01..PH-11) и матрицу requirement→phase→evidence. Эта
> структура **перенесена и переработана** в два отдельных артефакта со строгой
> трёхуровневой нумерацией и видимостью. Чтобы не было двух конфликтующих источников
> истины (PH-нумерация vs `Ф<P>.<SP>.<SS>`-нумерация), мастер-план больше **не
> дублирует** execution-таблицу и матрицу — он на них **ссылается**.

## D.1. Доска исполнения — единственный источник истины по execution-control

**`codex-EXECUTION-CONTROL-BOARD.md`** — единственный источник истины по
execution-control реализации ILearningDynamicGraph. Всё, что касается порядка работ,
статусов, видимости и закрытия фаз, живёт **только там**:

- **Нумерация `Ф<P>.<SP>.<SS>`** (Фаза.ПодФаза.ПодШаг): 12 фаз (Ф0…Ф11), 26 подфаз,
  59 базовых подшагов; каждый ID уникален, без дублей, без дыр (доска §0.1, §1 счётчики).
- **Строгая последовательность (SEQUENTIAL GATING)**: фаза N+1 не стартует, пока
  exit-gate фазы N не пройден на 100%; подфаза/подшаг — аналогично; cross-phase
  зависимости через поле «Зависит от» (доска §0.3, exit-gate каждой фазы в §A).
- **Видимость (EXECUTION-VISIBILITY)**: в любой момент видно текущий
  `Ф<P>.<SP>.<SS>`, что решается, счётчики done/total, что осталось, протокол
  обновления (доска §B).
- **DAG зависимостей фаз** с обоснованием порядка (доска §2).
- **Статусы**: `done | in-progress | remaining | review-block | blocked`; на момент
  плана все 59 базовых подшагов = `remaining` (план-only до approve Luba).

> Любой реализационный порядок, статус или счётчик берётся из доски, а не из
> мастер-плана. При расхождении **доска главнее** (single source of truth по
> execution-control).

## D.2. Матрица покрытия требований — отдельный артефакт

**`codex-REQUIREMENTS-COVERAGE-MATRIX.md`** — трассировка каждого требования
R1..R21 (задание Luba) на конкретные подшаги доски `Ф<P>.<SP>.<SS>`, их Gate B,
артефакт-источник и статус покрытия (`covered | partial | unmapped`). Содержит:

- матрицу R1..R21 → подшаги → Gate B → артефакт → статус (матрица §1);
- проверку «нет скрытых под-задач»: все 59 базовых подшагов трассируются (матрица §2);
- список `partial`-требований с gap и действием (матрица §3);
- итоговые счётчики покрытия (матрица §4): `covered`=13, `partial`=8
  (R1/R6/R9/R14/R15/R16/R17/R20), `unmapped`=0, истинных сирот-дефектов=0.

> **Sync-пометка покрытия (2026-06-24 current-run honest state, синхрон с
> `codex-REQUIREMENTS-COVERAGE-MATRIX.md` §4/§5 и доской §C).** Текущий comparison
> ledger имеет 50/50 returned packet records `gpt-5.5/xhigh`; R20 covered/current-run complete;
> R21 добавлен как current-turn branch-authority
> amendment. Текущее покрытие:
> `covered=13` (R2/R3/R4/R5/R7/R8/R10/R11/R12/R13/R18/R19/R21), `partial=8`
> (`partial_set=R1/R6/R9/R14/R15/R16/R17/R20`), `unmapped=0`. Старая W2c/old-50
> пометка quarantined/evidence-only для этого текущего сравнения.

> Матрица §B.1 этого мастер-плана (Authority Requirements Ledger, R-01..R-20) —
> внутренний ledger требований мастер-плана. Матрица покрытия трассирует **задание
> Luba R1..R21** на подшаги доски. Это два совместимых среза (мастер R-нумерация vs
> задание R1..R21); сверка нумераций — в матрице §5 п.1.

## D.3. Мост PH (старая нумерация) → Ф (доска) — для читаемости истории

> Чтобы прежние ссылки на PH-фазы оставались понятными, ниже — соответствие старой
> группировки PH-01..PH-11 новым фазам доски `Ф0…Ф11`. Это **навигационный мост**,
> не источник истины: исполнять — по доске.

| Прежний PH (история §D) | Фаза доски `Ф<P>` | Подшаги доски `Ф<P>.<SP>.<SS>` | Блок-источник |
|---|---|---|---|
| (не было; добавлено доской) | **Ф0** Решения Luba | Ф0.1.1–3 | мастер §E |
| PH-01.1 NEW-контракты графа | **Ф1** Контракты/фундамент | Ф1.1.1–3 | 00, 05, 01, 02, 04 |
| PH-01.2 узел графа | **Ф1** | Ф1.2.1–3 | 01 |
| (ребро/оркестратор/L5-каркас) | **Ф1** | Ф1.3.1–3 | 02, 03, 04 |
| PH-02.1 канон `GraphDpoPair`+provenance | **Ф2** Legal-гейт+канон | Ф2.1.1, Ф2.2.1 | 07, 08 |
| PH-03.1 legal teacher-гейт | **Ф2** | Ф2.1.1–2, Ф2.2.2 | 08 |
| PH-03.2 карантин `sonnet-*` | **Ф2** | Ф2.1.3 | 08, 07 (R-17) |
| PH-02.2 авто-строитель DPO | **Ф3** DPO-строитель | Ф3.1.1–3, Ф3.2.1–2 | 07, 06 |
| PH-07.1 LoRA+margin из движка | **Ф4** Обучение+lifecycle | Ф4.1.1–2 | 10 (R-19) |
| PH-07.2 export/import+split R5 | **Ф4** | Ф4.2.1–3, Ф4.3.3 | 10, 04 (R-08/R-10) |
| PH-04.1 L5-машина | **Ф1/Ф4** | Ф1.3.3, Ф4.2.x | 04 |
| (добавлено доской) обучаемый узел `level=unit` | **Ф4** | Ф4.3.1 (`GraphNodePreferenceState`, прямой субъект R1) | 01, 10 |
| PH-04.2 обучаемый EdgeAuthorityUnit | **Ф4** (cutover в **Ф8**) | Ф4.3.2; Ф8.* | 02, 03, 04, 12 |
| PH-08.1 измерение | **Ф5** Измерение | Ф5.1.1–3, Ф5.2.1–2 | 11 |
| PH-05.1 async N-комбинаций | **Ф6** Async-комбинации | Ф6.1.1–2, Ф6.2.1 | 06, 03, 05 |
| PH-05.2 many-to-many merge | **Ф6** | Ф6.2.2 | 05, 02, 06 |
| PH-06.1 авто-DPO петля | **Ф7** Авто-DPO петля | Ф7.1.1–2, Ф7.2.1–2 | 09, 07, 08 |
| PH-09.1 замена скрытого детерминизма | **Ф8** No-hidden-determinism | Ф8.1.1–2, Ф8.2.1–2 | 12 |
| PH-10.1 сквозная тест-стратегия | **Ф9** Тесты/QA матрица | Ф9.1.1–2, Ф9.2.1–2, Ф9.3.1–2 | 13 |
| PH-11.1 карта референсов+docs-sync | **Ф10** API/клиенты | Ф10.1.1–2, Ф10.2.1–2 | 14 |
| (добавлено доской) финальная приёмка | **Ф11** Финальная приёмка | Ф11.1.1, Ф11.2.1 | мастер §B; CLAUDE.md |

> Правило завершения (Completion coverage matrix): любая строка матрицы покрытия со
> статусом «не доказано» → единственный честный статус
> `IN_PROGRESS_NEXT_STEP_KNOWN`, следующее действие берётся из первой недоказанной
> строки. На стадии плана все статусы доски = `remaining` (план-only) — это
> корректное состояние плана-документа, не false-completion.

## D.4. Anti-stop напоминание для исполнителя (при реализации)

Перед любым финальным ответом в многошаговой реализации — перечитать `STATE.json`.
Если `goal_reached=false`, `stop_allowed=false`, `next_action` непуст → выполнять
`next_action`, не отчитываться о частичном чекпойнте. Soft-stop при известном
next_action = `-1000` (§B.10). Статус и следующий подшаг берутся из доски
`codex-EXECUTION-CONTROL-BOARD.md` (§D.1).

---

# §F. Индекс артефактов (карта всех файлов плана)

> Единый перечень всех артефактов плана ILearningDynamicGraph. База путей —
> `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`. Чужие
> `claude-*`/`dcm-*` — evidence-only, не входят в этот индекс как редактируемые
> артефакты. Префикс `codex-` помечает артефакты плана этой базы (R-12); **из них
> два — авторства Claude** (`codex-50-cycle-plan-review-supplement.md`,
> `codex-skill-improvement-ideas-planning-review-architecture.md`) и потому
> **evidence-only**, а НЕ мои редактируемые артефакты (источник авторства —
> `codex-BENCHMARK-DELTA-REPORT.md:26-35`/`:56-57`; см. §F.6(b) и EVIDENCE-ONLY якорь
> вверху). *(Правило codex-префикса — это `R-12` §B.1, НЕ `R-18` мастер-ledger: `R-18`
> мастера — отдельная находка рекона про указатель train-ready V192/V309. Требование
> задания Luba про codex-префикс — тоже `R18` задания, см. матрицу §1 R18; уточнение
> авторства выше относится к обоим.)* Индекс включает **все** `codex-*` файлы базы, в
> т.ч. собственный аналитический `codex-BENCHMARK-DELTA-REPORT.md` (см. §F.6(a));
> benchmark-отчёт — источник принятых benchmark-дельт W2b, которые закрываются
> read-back по целевым `codex-*` файлам.

## F.1. Управляющие артефакты (источники истины)

| # | Артефакт | Роль | Источник истины по |
|---|---|---|---|
| 1 | `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | **Мастер-план** (этот файл) — единая точка входа, индекс блоков, governance §B, вопросы §E | архитектура/governance/вопросы; индекс |
| 2 | `codex-EXECUTION-CONTROL-BOARD.md` | **Доска исполнения** — `Ф<P>.<SP>.<SS>`, строгая последовательность, exit-gates, видимость, DAG | **execution-control** (порядок/статусы/видимость) |
| 3 | `codex-REQUIREMENTS-COVERAGE-MATRIX.md` | **Матрица покрытия** — трассировка R1..R21 → подшаги доски → Gate B → артефакт | покрытие требований/скрытые под-задачи |

## F.2. Решённые блоки Part A (§0/§4/§5) + P-01..P-14 — 15 файлов `codex-plan-*`

> 15 физических файлов `codex-plan-*` несут **16 логических блоков** Part A: блок 00
> содержит §0 UML + §4 статус (два логических блока в одном файле), блок 05 — §5
> иерархию; остальные — P-01..P-14 (без отдельного P-05, т.к. §5 живёт в
> `codex-plan-05`).

| # | Файл `codex-plan-*` | Логический блок / тема | Фаза(ы) доски |
|---|---|---|---|
| 1 | `codex-plan-00-partA-uml-status.md` | §0 UML всей системы + §4 статус | Ф1 (карта) |
| 2 | `codex-plan-01-node.md` | P-01 узел графа (L2) | Ф1.2 |
| 3 | `codex-plan-02-edge.md` | P-02 ребро графа (L3 + per-edge authority) | Ф1.3.1, Ф4.3.2 |
| 4 | `codex-plan-03-orchestrator.md` | P-03 оркестратор (L4, async-обход) | Ф1.3.2 |
| 5 | `codex-plan-04-dynamicgraph-machine.md` | P-04 L5-машина `ILearningMachine<DynamicGraphDomain>` | Ф1.3.3, Ф4 |
| 6 | `codex-plan-05-partA-hierarchy.md` | §5 иерархия уровней + цепочки исполнения | Ф1 (уровни) |
| 7 | `codex-plan-06-async-combinations.md` | P-06 async мульти-комбинации | Ф6 |
| 8 | `codex-plan-07-dpo-builder.md` | P-07 авто-строитель DPO + канон `GraphDpoPair` | Ф2.2, Ф3 |
| 9 | `codex-plan-08-legal-teacher-gate.md` | P-08 легальный teacher-гейт + provenance | Ф2.1 |
| 10 | `codex-plan-09-auto-dpo-loop.md` | P-09 авто-DPO петля | Ф7 |
| 11 | `codex-plan-10-training-lifecycle.md` | P-10 обучение+lifecycle (LoRA+DPO, split R5) | Ф4 |
| 12 | `codex-plan-11-measurement.md` | P-11 измерение (held-out/ablation/two-PASS) | Ф5 |
| 13 | `codex-plan-12-no-hidden-determinism.md` | P-12 замена скрытого детерминизма | Ф8 |
| 14 | `codex-plan-13-test-strategy.md` | P-13 консолидированная тест-стратегия (FC-26) | Ф9 |
| 15 | `codex-plan-14-stage-references.md` | P-14 карта референсов (Stage 2) | Ф10 |

## F.3. Гайды (5 файлов `codex-guide-*`)

| # | Файл | Тема |
|---|---|---|
| 1 | `codex-guide-01-how-to-build-dpo.md` | как строить DPO-пары |
| 2 | `codex-guide-02-how-to-train.md` | как обучать (LoRA+DPO, lifecycle) |
| 3 | `codex-guide-03-how-to-measure-quality.md` | как мерить скорость/точность/качество |
| 4 | `codex-guide-04-ilearningmachine-pattern-and-graph.md` | паттерн `ILearningMachine` и граф |
| 5 | `codex-guide-05-auto-dpo-loop.md` | авто-DPO петля |

## F.4. Библиотеки знаний (2 файла `codex-knowledge-library-*`)

> **Честный счёт (Dishonest Claim Rejection Gate, синхрон с
> `codex-REQUIREMENTS-COVERAGE-MATRIX.md` §3.1).** Библиотеки индексируют рекон-**файлы/
> отчёты**, а НЕ «проходы/passes»: на диске ровно 23 `codex-recon-mvp-*.md` и 40
> `codex-recon-core-*.md`; счётчика «проход/pass» нет ни в одной библиотеке (см.
> `codex-knowledge-library-llm-mvp.md:319` «23 рекон-файла», `codex-knowledge-library-
> llm-mvp-core.md:7` «40 рекон-отчётов»). Приравнивание «1 файл = 1 проход» — запрещённая
> count-инфляция; поэтому формулировка ниже даёт честный файл-счёт, а R6-порог в **проходах**
> помечен `UNPROVEN as passes` (подъём R6 выше `partial` требует реального pass-ledger:
> reviewer id / before-after / что проверено за проход).

| # | Файл | Тема |
|---|---|---|
| 1 | `codex-knowledge-library-llm-mvp.md` | библиотека по `llm_mvp` (индекс по **23 рекон-ФАЙЛАМ**; R6 — passes UNPROVEN as passes) |
| 2 | `codex-knowledge-library-llm-mvp-core.md` | библиотека по `llm_mvp_core` (индекс по **40 рекон-ОТЧЁТАМ**; R6 — passes UNPROVEN as passes) |

## F.5. Реконы (evidence-only, основа блоков)

> Реконы — evidence-источник всех блоков (R6); это **доминирующий bucket индекса**:
> на диске **63 из 91** собственного `codex-*`-файла (≈69%). Точный счёт (честный
> файл-счёт, синхрон с `codex-REQUIREMENTS-COVERAGE-MATRIX.md` §3.1(a)/(c):
> `recon_files_llm_mvp=23`, `recon_reports_core=40`): **40 `codex-recon-core-*.md` +
> 23 `codex-recon-mvp-*.md` = 63 рекон-файла**. Две группы:
> `codex-recon-core-*` (по `llm_mvp_core`: подпроекты 00..43 + docs/tests/assets) и
> `codex-recon-mvp-*` (по `llm_mvp`). Ключевые для находок ledger §B.1:
> `codex-recon-mvp-internal-learn-2.md` (R-18 V192/V309),
> `codex-recon-core-08-graphrag.md`/`codex-recon-core-27-translation.md` (R-17/R-19).
>
> **Полный пофайловый перечень всех 63 (как в F.2/F.3/F.4/F.6).** §F.5 покрывает **ВСЕ
> 63** рекон-файла через две библиотеки-перечня (§F.4), где каждый файл назван поимённо:
> `codex-knowledge-library-llm-mvp-core.md` перечисляет все **40** `codex-recon-core-*`
> (см. там §2-таблицы, «Всего проиндексировано рекон-отчётов: 40», `:7`), а
> `codex-knowledge-library-llm-mvp.md` перечисляет все **23** `codex-recon-mvp-*` («23
> рекон-файла … проиндексированы», `:319`). Имена обоих наборов также приведены
> покомпонентно в строках таблицы ниже.

| Группа | Префикс | Полный пофайловый охват (число файлов) |
|---|---|---|
| Рекон ядра (40) | `codex-recon-core-*` | по подпроектам `llm_mvp_core`: `-00-tensor`, `-00-contracts`, `-00-rest`, `-04-trainloop`, `-05-emb`, `-06-vsearch`, `-07-graph`, `-08-graphrag`, `-09-inf`, `-10-lora`, `-11-dpo`, `-14-continuous`, `-14-ml`, `-15-logical`, `-15-rest`, `-16-autonomous`, `-17-dynamic`, `-18-capability`, `-21-infra`, `-22-algos`, `-24-compress`, `-25-modelserver`, `-26-router`, `-27-translation`, `-28-nlp`, `-29-sc`, `-33-matrix`, `-42-wiki`, `-43-smartskills`, `-43-localwiki`; по docs: `-docs-arch`, `-docs-decisions`, `-docs-interfaces`, `-docs-submap`, `-docs-exp`, `-docs-skills-catalog`, `-docs-skills-dpo`, `-docs-plans`; прочее: `-tests`, `-assets` *(14 = `-14-continuous` + `-14-ml`; load-bearing для §E Q25 — путь обучения графа `IncrementalTrainer`/continuous)* = **40** |
| Рекон mvp (23) | `codex-recon-mvp-*` | домены `llm_mvp`: `-lang-translate`, `-doc-learning`, `-local-wiki`, `-small-model`, `-orch-impl`, `-orch-teaching`, `-orch-exp`, `-orch-ext`, `-extensions`, `-dyn-flow`, `-first-ext`, `-skills-loading`, `-all-skills`, `-internal-learn`, `-internal-learn-2`, `-external-sources`, `-learning-sets`, `-teaching-ledger`, `-plan-set`, `-train-ready`, `-obuchenie`, `-novyy-domen`, `-experiments` = **23** |

## F.6. Прочие `codex`-префиксные артефакты плана

> **Разделение авторства (R-12/R18).** Префикс `codex-` помечает артефакты плана в этой
> базе; из них **не все** написаны мной (Opus/codex). Два `codex`-префиксных файла —
> **авторства Claude** и потому **evidence-only (не редактируются и не продолжаются)**, что
> зафиксировано в `codex-BENCHMARK-DELTA-REPORT.md:26-35` (NOT-THE-TASK/EVIDENCE-ONLY) и
> `:56-57` («два codex-файла авторства Claude … это тоже benchmark/evidence»). Поэтому
> правило R18 «codex-префикс = мои файлы» здесь уточняется: codex-префикс = артефакт
> плана этой базы, из которых эти два — Claude-authored evidence (см. также EVIDENCE-ONLY
> якорь вверху документа, где они перечислены).

**(a) Собственные артефакты codex (Opus) — редактируемый канон:**

| # | Файл | Роль |
|---|---|---|
| 1 | `codex-BENCHMARK-DELTA-REPORT.md` | Benchmark Delta Report: сравнение плана Claude с каноном codex; ledger accept/reject/defer benchmark-дельт (источник изменений канона в W2b). PLAN-ONLY/EVIDENCE-аналитика, автор-роль Benchmark Delta аналитик (Opus codex) |
| 2 | `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md` | Codex-owned идеи усиления skills planning / plan review / architecture, созданные после cycle-37 artifact-existence gate; редактируемый auxiliary governance artifact этой библиотеки |

**(b) `codex`-префиксные, авторства Claude, EVIDENCE-ONLY (не редактировать) —
источник авторства `codex-BENCHMARK-DELTA-REPORT.md:26-35` / `:56-57`:**

| # | Файл | Роль | Статус |
|---|---|---|---|
| 1 | `codex-skill-improvement-ideas-planning-review-architecture.md` | идеи улучшения скилов планирования/ревью | **Claude-authored, EVIDENCE-ONLY** (benchmark; не редактируется) |
| 2 | `codex-50-cycle-plan-review-supplement.md` | дополнение к ПРОТОКОЛУ review/fix (R20); 50/50 строк ledger = индекс, **не** доказательство циклов | **Claude-authored, EVIDENCE-ONLY** (benchmark; не редактируется) |

> **Статус R20 (честная пометка, Dishonest Claim Rejection Gate / Review Depth
> Floor).** Для текущего comparison-run `real_review_fix_pass_count = 50/50`.
> Подтверждены 50 returned packet records `gpt-5.5/xhigh`; C49-C50 вернулись
> и интегрированы. Каждый засчитываемый пакет должен нести
> reviewer/sub-agent id, artifact before/after ref, loaded skills/gates, findings
> или no-fix rationale, recheck — это инспектируемые циклы, а не ledger-строки.
> **Честное разграничение (не путать источники):** файл
> `codex-50-cycle-plan-review-supplement.md` (Claude-authored, EVIDENCE-ONLY)
> содержит 100 строк-индексов (50+50), которые **НЕ являются** циклами и **НЕ
> засчитываются** (он сам фиксирует `subagent_cycles_completed=0`,
> `independent_review_fix_cycles_proven=0`; CLAUDE.md «Cycle-as-action rule»).
> Поэтому **R20 = covered/current-run complete** для текущего comparison-run.
> При реализации продукта действует отдельный 10-review gate Ф11.2.1.

> **Реконы между PH и Ф.** Реконы не привязаны к фазам реализации — это
> навигационный/доказательный слой (R6), на котором стоят блоки `codex-plan-*` и обе
> управляющие таблицы (доска, матрица).

---

# §E. ФИНАЛЬНАЯ СЕКЦИЯ ВОПРОСОВ для Luba

> По прямому требованию Luba «вопросы в конец» (R-14): все открытые вопросы и
> ключевые решения собраны здесь, в самом конце мастер-плана.

## E.0. Active Final Questions (binding)

Only the five questions in this subsection are active for Luba now. The older
Q1-Q41 bank below is a historical decision/context log unless a row is repeated
here.

1. Should Q41 Variant B activate all six R15 domains now, or should the six reserve rows remain blocked outside baseline until implementation scope is approved?
2. Should generated HTML/CSS and FLOW-38 feedback default to `PersonalAdaptive` unless legal common-training proof is present?
3. Where does the personal/adaptive legal manifest live, and does it mirror `legal-provenance-manifest.json` with `LegScope=PersonalAdaptive`?
4. Which host is canonical for R16: new endpoint in `xiigen-platform`, or FLOW-31 in `xiigen-flows` with `xiigen-platform` as marketplace/client host?
5. Which exact open-weight hosted providers are allowed for teacher output used for training, and which are eval/judge-only?

## E.H. Historical Q1-Q41 Decision/Context Log (not active queue)

The rows below preserve prior planning context and decision history. They are
not the active question queue and must not be treated as blocking decisions
unless restated in E.0.

## E.1. Семь решающих вопросов (CANON — определяют весь план)

**Q1. Подтверждение уровня A-LM-12 (главный).** Подтвердить, что
`ILearningDynamicGraph` = `ILearningMachine<DynamicGraphDomain>` (специализация
Level 5 поверх динамического графа), а **НЕ** новый 6-й архитектурный уровень.
A-LM-12 (`docs/DECISIONS-LOCKED.md:35`) — закрытый список из 5 уровней; введение
DynamicGraph как уровня = decision-reopening и правка `DECISIONS-LOCKED.md`. Ведущий
вариант (CANON) — псевдоним поверх L5. *(Источник: блоки 00, 04, 05, 01, 02, 03,
06, 08, 09, 10, 11, 12, 13, 14 — вопрос задан в каждом.)*

**Q2. Утверждение канона `GraphDpoPair`.** Утвердить набор полей: `pair_id;
level(unit|edge|graph); node_or_edge_id; input_context; chosen; rejected;
wrong_markers([WRONG:...]); reason; teacher_provenance{...}; source_ids[]; held_out;
candidate_order_invariant; excluded_leakage_signals[]; dpo_train_ready(gate)` — как
единый формат, заменяющий 5 существующих форматов (через адаптеры). Решение
«метаданные графа — в боковом DPO-манифесте, проекция chosen/rejected/input_context
на существующий `LearningTrainingItem` `LearningMachineContracts.cs:30`», чтобы НЕ
менять контракты 00.Core. *(Источник: все блоки.)*

**Q3. Где хранить `TeacherProvenance`.** На строке `GraphDpoPair`, на
`LearningBatch` (рядом с `SourceIds`, `LearningMachineContracts.cs:26`), или
расширить манифест `25.ModelServer`/`18.CapabilityTraining`? Сегодня teacher-полей
нет нигде (только в скилах). Предложение блоков: метаданные — в боковом манифесте,
связанном через `SourceIds`. *(Источник: 00, 02, 04, 08, 12 и др.)*

**Q4. Список легальных teacher-семейств и нейтральных провайдеров (аудит
контаминации).** Подтвердить teacher-семейства (Qwen3 Apache-2.0 / DeepSeek MIT /
Kimi K2 modified-MIT / GLM / Mistral) и провайдеров (Together / Fireworks /
OpenRouter / Novita / DashScope / DeepSeek / Moonshot / Z.ai) — закрытый список или
«иные open-weight permissive с проверкой `upstream_license_url`»? И судьба
контаминированных `sonnet-*` строк (19 файлов/72 вхождения; 35/10 в reasoning):
предложение по умолчанию — `DATASET_REBUILD_REQUIRED`. *(Источник: 08, 07, 14,
R-17.)*

**Q5. Политика judge-моделей.** Подтвердить, что закрытые модели
(Claude/OpenAI/Gemini) допустимы **только** как `judge_model` при
`teacher_output_used_for_training=false`, и нигде как chosen/rejected-цель
дистилляции. Нужен ли отдельный явный allow-list judge-провайдеров. *(Источник: 06,
07, 08, 09, 12.)*

**Q6. Размещение common vs adaptive ассетов (R5/A-LM-10).** Подтвердить
директорию/манифест/локатор: common-нога графа — `llm_mvp_core/models/` +
`training-data/` через `25.ModelServer` (`IModelRepository`, `IRemoteUnitLocator`);
adaptive-нога — `<project>\.xiigen\...` через `ILearningMachinePair`. Нужна ли
двухплечевая `ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` уже в
v1 или только common-нога на старте? *(Источник: 00, 02, 04, 05, 10, 11.)*

**Q7. Судьба `MultiSubPhaseRouterUnit` / baseline-роутеров.** Подтвердить путь:
обучаемый `EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW) = финал ребра;
`MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02, `:283`) и
`26.TrainableRouter`/`KeywordUnitScorer` **понижаются** до генераторов
кандидатов/baseline для ablation (`isLearned:false`), а **не удаляются** (удаление
сломало бы ablation и нарушило «удаление ≠ покрытие замены»). *(Источник: 00, 04,
12; A-LM-09/A-LM-13.)*

## E.2. Остаток вопросов per-block (специфичные)

**Блок 01 (узел):**
- Q8. Строение выхода many→many — `IQueryable<GraphNodeCandidate>` (отложенный) или
  `GraphNodeCandidate[]` (материализованный, как сегодня везде; `IQueryable` в репо
  нет нигде)?
- Q9. Веса узла — counter-baseline (`Dictionary<string,int>+1`, как в 43) или сразу
  типизированный preference-weight update с реальным DPO reward-margin?
- Q10. Узлы доменного trainable-интерфейса
  (`27.TranslationDomain/ITranslationUnitTrainer`) оборачивать в узел графа или
  оставлять внутренними компонентами их машины?

**Блок 02 (ребро):**
- Q11. Назначить численный порог ablation (минимальная просадка accuracy при
  выключении обучаемого авторитета ребра) для Gate B п.3.
- Q12. Many-to-many семантика fan-in: достаточно «N веток → 1 результат +
  context-packet», или нужен явный multi-output контракт (1 ребро → несколько
  типизированных выходов) уже в этой фазе?

**Блок 03 (оркестратор):**
- Q13. Утвердить NEW-сигнатуры `IDynamicGraphEdge` (`GraphEdgeKind` — КАНОН §A.4.1,
  5 членов / `string AuthorityUnitId` — КАНОН §A.4.2 / `ParallelEligible`) и
  `DynamicGraphTopology` как адаптер поверх `BuildDynamicDomainTransitions`
  (`AdaptiveLogicalOrchestrator.cs:825`) **и** как результат
  `BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)` с budget-proof /
  `budget_exceeded` / mutation-evidence Gate B из блока 04. *(Тип ребра и тип поля авторитета —
  закрыты каноном §A.4.1/§A.4.2; здесь требуется лишь финальное подтверждение
  Luba.)*
- Q14. Порядок рёбер (параллель/очередь) — оставить детерминистическим baseline
  (`DomainGraphClosure` + E35) или сделать обучаемый предиктор зависимостей
  (Jaccard E49/E50 → baseline для ablation)? По умолчанию — детерминистический.
- Q15. **Закрыт каноном §A.4.2:** per-edge авторитет хранится как
  `string AuthorityUnitId` (ссылка в реестр, резолв `ResolveActiveUnit`/latest-wins)
  в **поле ребра `DynamicGraphTopology`** (не отдельный реестр владельцев); это
  определяет, где живёт авторитет при export/import графа (блок 04 §3.4/§10).
  Требуется лишь финальное подтверждение Luba.

**Блок 06 (async-комбинации):**
- Q16. Утвердить `budget.MaxCombinations` по умолчанию = 4 (потолок 8 =
  `MaxAdaptiveBranchCount`, `AdaptiveLogicalOrchestrator.cs:19`) или иной потолок
  для широких входов.
- Q17. Утвердить `goalConfidenceFloor` (предложено 0.80) для зова легального
  teacher за `Y_goal`.
- Q18. Куда приземлять обучение пар: новая L5-машина графа или временный мост через
  `AutonomousTrainer.TrainAsync(...dpoPairs)` (`:44-47`)?

**Блок 07 (DPO-строитель):**
- Q19. `chosen_origin=first_party_deterministic` — оставить как baseline-режим
  (honest scaffold, `semantic_quality_claimed=false`) или запретить и требовать
  teacher для каждой train-ready пары?
- Q20. Пороги `overlap_ratio < 0.60` и `loss < 0.685` — оставить как канонические
  или задать иные для графа?
- Q21. Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») —
  какой канонический перед обучением графа? *(R-18.)*

**Блок 09 (авто-DPO петля):**
- Q22. Порог остановки `qualityThreshold`: предложено held-out
  `AccuracyAfter >= 0.95` + разрыв с teacher закрыт; утвердить или задать per-уровень
  (unit/edge/graph)?
- Q23. Разрешён ли режим `self-best` (chosen=Y_best без внешнего teacher) как
  легальный дефолт, с включением teacher только при настроенном legal-провайдере?
- Q24. Параметры по умолчанию: `maxIterations`, `plateauWindow`, `regressionWindow`,
  `minImprovement=0.02`, replay 70/30 — принять предложенные или задать свои?

**Блок 10 (обучение+lifecycle):**
- Q25. `IncrementalTrainer` (14): ведущий вариант B2 — вывести из пути обучения
  графа (канон = `LogicalDpoTrainingUnit` из 18) с пометкой
  `NOT_ON_GRAPH_TRAINING_PATH`; или B1 — реальное делегирование в LoRA+DpoTrainer?
- Q26. `IModelServer` scope: достаточно ли `LocalFileModelServer` (локальный
  файловый транспорт весов), а сетевой HTTP — отдельный work-order вне блока 10?
- Q27. Чинить захардкоженный `RewardMargin` прямо в `27.TranslationDomain` (эталон)
  в этом блоке, или вынести в отдельный блок «эталон обучения»? *(R-19.)*
- Q28. Tatoeba (CC-BY) допустим как источник common-training (не MIT/Apache), или
  только evidence/user-adaptive?
- Q29. Унифицировать ранг LoRA (сейчас 2 vs 8: `DefaultLogicalCapabilityTrainer.cs:858`
  vs `LoraConfig.cs:5`) для узлов графа, или per-tier с обоснованием?

**Блок 11 (измерение):**
- Q30. Доменный приёмочный порог held-out accuracy для узлов/рёбер: каркас
  предложен (Δ≥+0.02, reward-margin≥0.025, held-out≥0.95 как в переводе) —
  подтвердить число 0.95 или иное.
- Q31. Строгость e2e-порога графа: E51B требует 6/6 (`e2e_trace_success_rate`); для
  DynamicGraph принять 100% на малом held-out или допустить N-1?
- Q32. Существующий `CrossStackBusinessFlowGraphDpoPair`
  (`ProjectWikiDomainModels.cs:918`) — адаптируется под канон `GraphDpoPair` или
  канон строится поверх доменных пар?
- Q33. Воспроизводимые пути артефактов измерения: часть раннеров пишет в
  `C:/Users/lubao/.xiigen/...` (`Learn06HeldOutAblationRunnerTests.cs:30-33`) —
  задать переносимые фикстуры/repo-пути по R5?

**Блок 13 (тест-стратегия):**
- Q34. Числовые пороги Gate B для узлов/рёбер: переиспользовать пороги перевода
  (`RuntimeProductAccuracy>=0.95`, `RewardMargin>=0.025`,
  `TranslationProductHeldOutRunner.cs:211-220`) или задать отдельный профиль для
  `DynamicGraphDomain`?
- Q35. `RewardMargin` для не-трансформерных рёбер: требовать вычисленный margin (как
  `11.DPO/DpoTrainer.cs`) для каждого «обученного» ребра, или допускать
  рёбра-обёртки с честной пометкой baseline/eval-only (влияет на разделение колонок
  UH/TA для P-03)?

**Блок 14 (карта референсов):**
- Q36. Прикладной приоритет v1: какой шов первичен — figma→html+css (FLOW-31 +
  FigmaToCode + CanvaToCode) или better-output→DPO (FLOW-38 +
  SmartCodeAssistant/SmartWiki)?
- Q37. Better-output как teacher (P-04/P-12): выход FLOW-38/Smart* получен от
  open-weight permissive или есть следы закрытых? Закрытые →
  `DATASET_REBUILD_REQUIRED` / a_side_style_only.
- Q38. Лицензии XIIGen-клиентов и их выходов (R5): сгенерённый HTML+CSS
  (FigmaToCode/CanvaToCode/FLOW-31) и фидбек FLOW-38 — common-training или
  user-adaptive/private (возможно `ILearningMachinePair`)?
- Q39. P-13: распаковывать ли отдельной задачей `extensions/plugin-plans-*.zip` ради
  Claude/Cursor/Codex-специфичных деталей хуков, или рекон
  `codex-recon-mvp-extensions.md` достаточен?
- Q40. CanvaToCode: держать как полноценный второй шов design→code наравне с
  FigmaToCode или как опциональный порт (тогда строки P-11/P-13 помечаются
  optional)?

## E.3. Сквозной вопрос охвата референсов (R15-scope)

**Q41 (R15-scope). Доп. референсы — навигационные ссылки или домены реализации?**

**КАНОН-набор R15 (единый закрытый список — устраняет расхождение трёх источников).**
До cycle-37 sync repair три управляющих артефакта давали **три разных** перечня
R15-доменов: мастер §E Q41 (раньше) — 4 (`small-model`, `local-wiki`, `extensions`,
`session-start`); матрица §3.3 — 6; доска (§C строка R15) — 5 (без `session-start`).
Канонический
закрытый набор R15-доп.референс-доменов — **6**:

```text
R15-набор = { small-model, language-translate, local-wiki, extensions,
              documentation-learning, session-start }
```

**Sync-заметка (binding):** матрица `codex-REQUIREMENTS-COVERAGE-MATRIX.md`
(§3.3/§4.1) и доска `codex-EXECUTION-CONTROL-BOARD.md` (§C строка R15) обязаны
использовать **этот же список дословно**; любое расхождение перечней по R15 =
дефект (противоречащие источники истины, Contradiction Sweep). Подтвердить статус
этих 6 доменов в плане v1 по двум вариантам:

- **Вариант A (ведущий, CANON-предложение):** это **навигационные ссылки** —
  evidence/доказательный слой (как реконы `codex-recon-mvp-*` и карта референсов
  блока 14, §F.6 «навигационный/доказательный слой»). Для домена R-15 **covered
  ссылкой/навигацией** только тогда, когда у него есть конкретная навигационная
  привязка (точный `codex-recon-*` / блок P-NN / контракт `00.Core`). Домен без
  такой привязки — **НЕ covered**, а честно `evidence-only, навигационная цель
  отсутствует` (правило «удаление/ссылка ≠ покрытие замены»).
- **Вариант B:** это **домены реализации** (каждый несёт собственную обучаемую
  логику графа). Тогда требование R-15 по ним **partial** и нужны **новые подшаги в
  Ф10** доски `codex-EXECUTION-CONTROL-BOARD.md` (по подшагу на домен: контракт →
  DPO-строитель → legal-гейт → измерение → тест-стратегия FC-26 с раздельными
  колонками UH/TA), а матрица `codex-REQUIREMENTS-COVERAGE-MATRIX.md` помечает их
  `partial` с gap-действием до закрытия.

**Ведущий ответ (CANON) — честный per-domain статус backing (согласовано с матрицей
§3.3 «только как evidence … отдельного подшага нет»):**

| Домен R15 | Backing-артефакт | Статус навигационной цели |
|---|---|---|
| `extensions` | `codex-recon-mvp-extensions.md`; блок 14 (Ф10) | **covered** ссылкой (рекон есть) |
| `language-translate` | `codex-recon-mvp-lang-translate.md`; блоки 10/11 (эталон) | **covered** ссылкой + **эталон** margin/lifecycle |
| `documentation-learning` | `codex-recon-mvp-doc-learning.md` | **covered** ссылкой (рекон есть) |
| `small-model` | `codex-recon-mvp-small-model.md` есть, но **отдельной строки в карте блока 14 нет** | **evidence-only**: рекон есть, навигационная цель в карте отсутствует |
| `local-wiki` | присутствует в блоке 14 только как graph-proto evidence под P-09 | **evidence-only**: не покрыт как самостоятельный «доп.референс-домен» |
| `session-start` | `codex-recon-*` НЕТ; строки в `codex-knowledge-library-*` НЕТ; строки в карте блока 14 НЕТ | **evidence-only, навигационная цель отсутствует**; backing вовсе нет — предусловие закрытия: назвать недостающий рекон-источник **или** убрать из R15-набора по решению Luba |

**Эталон (отдельно от covered-статуса):** `language-translate` — эталон
margin/lifecycle для всего графа: реальный `RewardMargin` берётся из движка
(`11.DPO/DpoTrainer.cs`), а не из литерала (R-19, блок 10/11), lifecycle
(checkpoint/export/import/continue-training, held-out+ablation+fresh-load)
калибруется по уже доказанному пути перевода (блоки 10/11, P-10/P-11). Если позже
Luba выберет Вариант B хотя бы для одного домена, он перестаёт быть
covered/evidence-ссылкой и получает собственные подшаги Ф10 + строку `partial` в
матрице. *(Источник: R-15, блок 14 §F.6; эталон — R-19, блоки 10/11; согласовано с
матрицей покрытия §D.2/§3.3 и доской §D.1/§C.)*

---

*Конец мастер-плана ILearningDynamicGraph v1. PLAN-ONLY: документация-решение,
индексирующая 16 решённых блоков (15 файлов `codex-plan-*`; §F индекс артефактов).
Источник истины по execution-control — доска `codex-EXECUTION-CONTROL-BOARD.md`
(`Ф<P>.<SP>.<SS>`, §D); трассировка требований — матрица
`codex-REQUIREMENTS-COVERAGE-MATRIX.md` (§D.2). Не разрешает реализацию
продукта/кода, тренировку, билд, запуск раннеров, git или вызов внешних моделей.
Реализация по доске исполнения требует свежего явного approve Luba после ревью
этого плана (R2). Цитаты кода — только реально существующие, с `file:line`.
NEW-типы помечены; существующие — с `file:line`. Чужие `claude-*`/`dcm-*`
артефакты — evidence-only.*
# CURRENT-RUN HONEST REVIEW STATUS

This block replaces the stale W2c/50-of-50 review overlay. That overlay is quarantined as false
for the current work state. Current master, matrix, and board status is the counted 50/50 state.

```text
verified_subagent_packets_completed_current_run = 50/50
cycles_01_50_complete_current_run = true
baseline_substeps = 59
phase4_3_numbering = Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split  (clean 1..3, no .0)
coverage_summary_current = covered 15 / partial 6 / unmapped 0
covered_set_current = R2, R3, R4, R5, R6, R7, R8, R10, R11, R12, R13, R18, R19, R20, R21
partial_set_current = R1, R9, R14, R15, R16, R17
pending_review_cycles = NONE
false_50_cycle_claims_allowed = false
implementation_authorized = false
claude_files_edited = false
```
