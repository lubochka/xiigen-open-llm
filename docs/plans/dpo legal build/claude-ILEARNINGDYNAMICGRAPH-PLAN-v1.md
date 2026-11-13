# Мастер-план ILearningDynamicGraph — v1 (claude namespace, dpo legal build)

> **Артефакт:** `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`
> **Автор-роль:** автор claude-плана (Opus 4.8 MAX). Это **улучшенная версия** мастер-плана
> ILearningDynamicGraph в моём namespace `claude-*`. Бенчмарк
> (`codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` и весь набор `codex-*`) использован как
> **только-чтение evidence**: я не копировал его слепо, а усиливал слабые места
> (ясность формулировок, точность цитат `file:line`, честность характеристик кода,
> глубину тест-стратегии, консистентность счётчиков).
> **Тип артефакта:** ПЛАН-ONLY (документация-решение). НЕ промпт исполнителю и НЕ
> разрешение на реализацию продукта/кода, тренировку, билд, запуск раннеров, git
> или вызов внешних AI/API/моделей. Реализация требует **свежего явного разрешения
> Luba** после approve этого плана (R2/CLAUDE.md «Continuous role loop»).
> **Язык:** русский (человеческим языком сначала); имена типов/полей — английский.
> **Цитаты кода:** только реально существующие, с `file:line`, сверены прямым чтением
> главного чекаута `C:/Projects/llm_mvp_core` (не worktree-копий). NEW-типы помечены
> `NEW`; существующие — с `file:line`. Чужие `codex-*`/`dcm-*` и старый каркас
> `claude-00..07` — evidence-only, не редактируются и не продолжаются.
> **Канонические инварианты (сохранены из бенчмарка):** 12 фаз / 27 подфаз / 64
> подшага (включают активный пилот Ф10.3 `language-translate`, Q41=Вариант B RESOLVED,
> ровно 1 пилот; прежний baseline 12/26/59 — до активации пилота, RR4-pilot-activation);
> нумерация `Ф<P>.<SP>.<SS>`; 7 NEW-типов; `GraphEdgeKind` (5 членов);
> `EdgeAuthorityUnit` (одна I/O-пара); `GraphDpoPair`; `TeacherProvenance` (20 полей, RR6);
> A-LM-12 (L5, без 6-го уровня); легальный teacher-гейт; запрет скрытого детерминизма
> (A-LM-09/A-LM-13).

---

## CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY (anchor, по CLAUDE.md «Current task anchor»)

```text
CURRENT TASK:  написать УЛУЧШЕННЫЙ мастер-план ILearningDynamicGraph в моём
               namespace claude-* (файл claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md):
               §A заголовок+русское резюме архитектуры, §B governance/orchestration,
               §C индекс Part A (§0/§4/§5) + блоков P-01..P-14 как решённых §1/§2/§3,
               §D ссылка на доску исполнения + матрицу покрытия, §F индекс артефактов,
               §E финальная секция ВОПРОСОВ в самом конце. Сохранить структуру и
               канонические счётчики (12 фаз / 27 подфаз / 64 подшага — после активации
               пилота Ф10.3 language-translate; Ф<P>.<SP>.<SS>) и CANON-инварианты; улучшить
               ясность/глубину/цитаты.
NOT THE TASK:  реализовывать/тестировать/билдить код, запускать раннеры, вызывать
               внешние AI/API/модели, git; редактировать/создавать любые codex-*
               файлы (план Codex, recon, 50-cycle, skill-improvement); продолжать или
               править старый каркас claude-00..07 либо чужие dcm-*; копировать
               бенчмарк дословно (улучшать, а не копировать).
EVIDENCE-ONLY: весь codex-* (codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md как главный
               бенчмарк; codex-EXECUTION-CONTROL-BOARD.md; codex-REQUIREMENTS-COVERAGE-
               MATRIX.md; codex-BENCHMARK-DELTA-REPORT.md; codex-recon-*/codex-guide-*/
               codex-knowledge-library-*); старый каркас claude-00..07; контракты
               00.Core/*; docs/* RAG; транскрипты; STATE-файлы. Чтение — да; правка
               или продолжение — нет.
```

> **Что именно улучшено относительно бенчмарка (для прозрачности ревью).**
> 1. **Точность цитат.** Сверены прямым чтением главного чекаута: `SubPhaseRouterUnit.cs:6`
>    (определение `SubPhaseRouterSourceDocument` с полем `SourcePath`) и
>    `MultiSubPhaseRouterUnit.cs:293` (комментарий «deliberately excluding SourcePath») —
>    бенчмарк говорил о leakage `SourcePath` абстрактно, здесь даны точные file:line.
> 2. **Честность характеристики роутера.** Бенчмарк писал `isLearned:false` как будто это
>    поле кода. В коде такого литерала нет — `MultiSubPhaseRouterUnit` просто
>    детерминистический, без обученного состояния/checkpoint. Здесь формулировка
>    исправлена на «детерминистический, обученного состояния нет» (A-LM-13: claim не
>    сильнее механизма).
> 3. **Тест-стратегия.** §C.4 P-13 и §B.6 дополнены явной привязкой 9 слоёв FC-26 к
>    двум PASS-состояниям (UH/TA) на уровне колонок матрицы.
> 4. **Консистентность счётчиков.** Единый честный срез покрытия (cycle-37 honest, R20
>    понижен в partial: `covered=12/partial=8/unmapped=0`) и числа полей/членов канонов
>    сведены в одну таблицу §B.12; этот же срез дословно несут доска §C и матрица §6/§7
>    (старые `covered=13/partial=7` и `covered=15/partial=5` — quarantined как false).

---

# §A. Заголовок, назначение, краткое русское резюме архитектуры

## A.1. Назначение мастер-плана

Этот документ — **единая точка входа** в план проекта `ILearningDynamicGraph`. Он не
повторяет содержимое решённых блоков, а **связывает** их: показывает Любе как
архитектору-человеку, из чего состоит план, в каком порядке его реализовывать, чем
доказывается каждое требование, и какие решения остаются за ней (финальная секция
вопросов §E).

Шестнадцать логических блоков-документов (каждый — самостоятельный §1/§2/§3 со своей
UML, симуляциями, цитатами кода и Gate B) собраны здесь в одну карту:

- **Part A** (что Luba читает и одобряет): §0 UML всей системы + §4 статус
  (блок 00), §5 иерархия уровней + цепочки исполнения (блок 05).
- **P-01..P-14** (решённые блоки §1/§2/§3): узел, ребро, оркестратор, L5-машина,
  async-комбинации, DPO-строитель, легальный teacher-гейт, авто-DPO петля,
  обучение+lifecycle, измерение, замена скрытого детерминизма, тест-стратегия,
  карта референсов.

Мастер-план — **индекс**, а не дубликат: порядок работ, статусы и счётчики живут в
доске исполнения (§D), трассировка требований — в матрице покрытия (§D), сами решения
§1/§2/§3 — в блоках `codex-plan-*` (evidence-источник). Этот claude-документ —
улучшенная **верхнеуровневая навигация и governance** над тем же набором решений.

## A.2. Краткое русское резюме архитектуры (что мы строим)

Мы строим **обучаемый динамический граф** — модель, которая учится не одному навыку,
а **отношениям между навыками разного порядка**: какой узел кому передаёт задачу, в
каком порядке, и как сливать результаты. Архитектурно это **верхний обучающий уровень
(Level 5)** поверх уже существующей пятиуровневой иерархии проекта — НЕ новый шестой
уровень.

**Аналогия (общая для всех блоков) — «завод/оркестр с перекладываемыми лентами»:**

- **узел-станок** = `ILearningUnit` (≡ `ILogicalUnit`, L2): берёт деталь, отдаёт
  деталь, днём (в рантайме) сам не учится;
- **лента между станками** = `IDynamicGraphEdge` (NEW): бывает веером наружу (fan-out),
  веером внутрь (fan-in), много-ко-многим, много-выходов и петлёй возврата;
- **карта лент** = `DynamicGraphTopology` (NEW): хранится как данные в реестре,
  перекладывается на лету (это и есть «динамический» граф);
- **начальник цеха** = `ILearningOrchestrator` (≡ `ILogicalOrchestrator`, L4): читает
  карту, обходит станки и ленты, собирает итог;
- **директор с ночной сменой обучения** = `ILearningDynamicGraph`
  (≡ `ILearningMachine<DynamicGraphDomain>`, L5): единственный, кто имеет право
  перенастраивать станки по результатам приёмки (что приняли, что забраковали).

**Три жёстких правила, проходящих через весь план:**

1. **Обучение живёт только на L5** (A-LM-02, `docs/DECISIONS-LOCKED.md:25`): узлы/рёбра
   — чистый inference, `Train*` только на машине.
2. **Запрет скрытого детерминизма** (A-LM-09 «DeterministicFinalAuthority = false»,
   `docs/DECISIONS-LOCKED.md:32`; A-LM-13, `:36`): на каждом ребре финал принимает
   **обученный** узел, а детерминистический роутер/счётчик/словарь — либо генератор
   кандидатов, либо честно помеченный `baseline/eval-only`. Главный кандидат на замену
   — `MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02,
   `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283`; обученного состояния
   нет — деталь честно классифицируется как baseline, см. §A.2.1).
3. **Легальный teacher-гейт** («dpo legal build»): дистилляция — только open-weight
   permissive (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 modified-MIT / GLM / Mistral)
   через нейтральных провайдеров. Claude/OpenAI/Gemini как teacher для дистилляции —
   **никогда** (только judge, и judge не цель обучения). Контаминированные строки →
   `DATASET_REBUILD_REQUIRED`. **RR6 (2026-06-30) — отдельная ось от brand-denylist:**
   даже allowlist-модель в режиме grounding/web-search/retrieval (как Gemini Grounding /
   Google Search) подмешивает чужой веб-контент → `service_mode≠plain ∨ grounding_used`
   при обучении ⇒ BLOCK (C17); closed-judge без пройденной terms-проверки ⇒ QUARANTINE
   (C11-blocked); `terms_snapshot_ref` для Together/Qwen3 = обязательный гейт со значением
   = pending Luba/внешнее (владелец `claude-plan-08 §11.1`).

## A.2.1. Честная характеристика baseline-роутера (улучшение точности vs бенчмарк)

> **Уточнение (A-LM-13, claim не сильнее механизма).** Бенчмарк характеризовал
> `MultiSubPhaseRouterUnit` как `isLearned:false`. В коде такого поля/литерала **нет**.
> Реальность точнее: класс `MultiSubPhaseRouterUnit` (`MultiSubPhaseRouterUnit.cs:8`,
> `: ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>`) считает
> взвешенную сумму с **захардкоженными** весами (`:283`
> `(predicateScore*0.55)+(exactSubPhaseMention*0.25)+(tokenScore*0.20)+(hybridScore*0.02)`)
> и **не имеет** обученного состояния, checkpoint или DPO-обновления. Поэтому корректная
> классификация (skill `trainable-architecture-decision`): это
> `BASELINE_MUST_BE_LABELLED` — детерминистический генератор кандидатов/baseline для
> ablation, а не «trained unit, помеченный isLearned=false». Замена финальной authority
> — обучаемый `EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW); сам роутер
> **понижается** до baseline, а не удаляется (удаление сломало бы ablation; «удаление ≠
> покрытие замены»).

## A.3. Маппинг ролей Luba → реальные контракты (CANON)

| Имя Luba | Реальный контракт | Уровень | file:line (сверено) |
|---|---|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` | L2 | `00.Core/LogicalContracts.cs:97` |
| `ILearningFlow` | `ILogicalFlow<TInput>` | L3 | `00.Core/LogicalContracts.cs:166` |
| `ILearningOrchestrator` | `ILogicalOrchestrator` | L4 | `00.Core/LogicalContracts.cs:202` |
| `ILearningDynamicGraph` | `ILearningMachine<DynamicGraphDomain>` поверх динамического графа | L5 | `00.Core/LearningMachineContracts.cs:208` |
| (two-leg, опционально) | `ILearningMachinePair<TPersonal,TGlobal>` | L1-связка | `00.Core/LearningMachineContracts.cs:245` |

A-LM-12 (`docs/DECISIONS-LOCKED.md:35`) — закрытый набор из 5 уровней; новый 6-й уровень
НЕ вводим. DynamicGraph = специализация L5 (домен `TDomain`). Ведущий вариант;
финальное подтверждение уровня — в §E (вопрос Q1).

## A.4. Семь NEW-типов (проектируются в плане; в коде их НЕТ)

| # | NEW-тип | Что это | Решён в блоках |
|---|---|---|---|
| 1 | `DynamicGraphDomain` | sealed-маркер `TDomain` для L5-машины графа | 00, 04, 05 |
| 2 | `IDynamicGraphNode` | обёртка над узлом-`ILogicalUnit` (без `Train*`, A-LM-02) | 00, 01, 04, 05 |
| 3 | `IDynamicGraphEdge` (+`GraphEdgeKind` — КАНОН §A.4.1) | типизированное ребро fan-out/fan-in/many-to-many/multi-output/return-loop + per-edge обучаемая authority | 00, 02, 03, 04, 05 |
| 4 | `DynamicGraphTopology` | мутируемая на лету карта узлов/рёбер (проекция реестра топологии) | 00, 03, 04, 05 |
| 5 | `EdgeAuthorityUnit` (КАНОН §A.4.2; вход `EdgeAuthorityInput` → выход `EdgeAuthorityDecision`) | обучаемый финальный решатель ребра (заменяет baseline-роутер) | 02 (владелец канона), 03, 04, 12 |
| 6 | `GraphDpoPair` | канон DPO-пары графа (объединяет 5 существующих форматов) | все блоки; канон в 07 |
| 7 | `TeacherProvenance` | поля легального учителя (`closed_proprietary_model` должно быть false) | все блоки; гейт в 08 |

> **B-10 — machine-internal обучаемый узел (НЕ меняет счётчик «7»).** Шаг починки B-10 ввёл
> `GraphOrderResolverUnit` (+ typed-state `GraphOrderState`) — обучаемый решатель порядка паттернов
> РАЗНОГО ПОРЯДКА (order-0..N), владелец канона — блок 04 (`claude-plan-04-dynamicgraph-machine.md`
> §3.3-bis). Он классифицирован как **внутренний L2-компонент L5-машины** (обучаемый шаг внутри
> `BuildDynamicTopology`), а НЕ как 8-й верхнеуровневый NEW-тип Part A, поэтому канон «семь NEW-типов
> иерархии графа» сохранён без рассинхронизации `claude-*`. Полное закрытие блокера — §E.2
> Q14-graph-order; вопрос «поднимать ли до 8-го типа» — RESOLVED(Luba 2026-06-30) там же: НЕ
> поднимать, остаётся machine-internal L2-компонентом (K=5), счётчик «7 NEW-типов» не тронут.

> **Канон `GraphDpoPair`** (КАНОН-СОСТАВ ПОЛЕЙ ЗАКРЫТ, B-13 / §E Q2 RESOLVED — **владелец блок 07**,
> ровно 14 полей; этот §A.4 — синхронная копия владельца):
> `pair_id; level(unit-only); node_or_edge_id(unit_id/ref); input_context; chosen; rejected;
> wrong_markers([WRONG:...]); reason; teacher_provenance{...}; source_ids[]; held_out;
> candidate_order_invariant; excluded_leakage_signals[]; dpo_train_ready(gate)`.
> `GraphDpoPair` chosen/rejected material is for one ILearningUnit/domain item only;
> graph/edge/combo/path/order ids stay provenance/context in `source_ids[]`, sidecar, or trace refs.
> Хранение без правки контрактов 00.Core: проекция `chosen/rejected/input_context` на
> существующий `LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`,
> поля `ItemId/Prompt/ChosenResponse/RejectedResponse`) + боковой DPO-манифест для
> остальных 10 полей, связанный через `SourceIds`
> (`00.Core/LearningMachineContracts.cs:26`). `teacher_provenance{...}` — **вложенная** запись
> `TeacherProvenance` (20 полей, RR6, владелец блок 08 §3.1), не плоский ref.
>
> **B-13 Single-Owner Reconciliation (один record — один владелец — один список полей).** Каждый из
> двух общих record-типов имеет **ровно один** владелец-файл и **ровно один** список полей:
> `GraphDpoPair` (14 полей) — владелец `claude-plan-07-dpo-builder.md`; `TeacherProvenance` (**20
> полей**, RR6) — владелец `claude-plan-08-legal-teacher-gate.md` §3.1. Этот §A.4 несёт **синхронную копию**
> обоих списков, не второй канон. Историческое расхождение закрыто: блок 04 §7 ранее нёс ВТОРОЙ,
> другой 18-польный список `TeacherProvenance` (имена `teacher_provider`/`teacher_model_id`/
> `judge_provider`/`judge_model_id`/… вместо канонических `model_id`/`provider_terms_checked`/
> `judge_model`/…) при «два словаря, оба заявляют 18 полей, оба ссылаются на 08» — этот список удалён
> и заменён цитатой канона 08 §3.1 (правка 2026-06-30, шаг B-13). Состав полей Q2 (`GraphDpoPair`) и
> Q3 (`TeacherProvenance`) — **RESOLVED**: расположение хранения провенанса было закрыто решением Luba
> 2026-06-29 (§E Q3 / §E.0); состав полей закреплён здесь как единый канон по владельцу.
>
> **Поля `TeacherProvenance`** (КАНОН — владелец **`claude-plan-08` §3.1** (мой namespace, не
> `codex-*`); §E Q3 storage RESOLVED, состав полей — B-13 RESOLVED): **20 полей = 13 базовых + 5 NEW
> бенчмарк-дельт T2/T3/T5 + 2 RR6 (service-mode ban)**. 13 базовых: `teacher_access_mode; hosting_provider; model_id;
> model_license; model_version; request_id; provider_terms_checked;
> teacher_output_used_for_training; closed_proprietary_model; source_ref; source_sha256;
> contamination_markers; judge_model` (поле `judge_model` — **ВНУТРИ**
> `TeacherProvenance`, **НЕ** отдельным верхнеуровневым полем). 5 NEW:
> `terms_snapshot_ref` (T2), `geography_export_constraint` (T2), `source_category` (T5),
> `raw_private_text_copied` (T3), `redaction_policy_ref` (T3). **2 RR6 (2026-06-30):**
> `service_mode` (plain|grounding|web_search|retrieval_tool|code_exec|unknown — для train/judge
> обязан быть `plain`) и `grounding_used` (bool) — **отдельная ось от brand-denylist**: allowlist-модель
> в режиме grounding/search подмешивает чужой веб-контент → C17 BLOCK (владелец `claude-plan-08`
> §2.2 шаг 4b / §4; см. §E Q5 RR6-указатель). **Вердикт-enum — ровно 6
> значений** (выровнено с владельцем канона `claude-plan-08` §3.3):
> `ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY`.
> `BLOCK_FOR_COMMON` (NEW, T3 §8.5: приватный текст в hosted без редакции → только adaptive
> leg) и `EVAL_ONLY` (NEW, фикс R17, `claude-plan-08` §3.3/§8.6: легально-чистая, но
> train-ineligible строка из-за неполной триады C15/C15b или невоспроизводимого конверта
> C8 — **полноправный** член канона, не просто «operational»: без него такая строка несёт
> `ACCEPT` и буквально проходит предикат batch-гейта `legal_verdict==ACCEPT`). **Предусловие
> `dpo_train_ready=true`:** `provenance_field_presence_rate == 1.0` по **всем 20 полям**
> (RR6: включая `service_mode`/`grounding_used`).
> При любом расхождении числа полей, числа/состава вердикт-enum или размещения `judge_model`
> между §A и блоком 08 — **владелец канона блок 08** (`claude-plan-08` §3.3 для enum, §3.1 для 20 полей).

## A.4.1. КАНОН ребра — `GraphEdgeKind` (enum видов ребра)

> Закрывает расхождение четырёх блоков. На диске один тип ребра объявлен под **четырьмя**
> разными именами/наборами: `DynamicGraphEdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> Sequential}` (`codex-plan-04` §5.2), `EdgeKind{OneToOne,FanOut,FanIn,ManyToMany,
> MultiOutput}` (`codex-plan-02` §5), `GraphEdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> Sequential,Bridge}` (`codex-plan-03` §5), `EdgeKind{FanOut,FanIn,ManyToMany,MultiOutput,
> BackEdge,AsyncParallel}` (`codex-plan-05` §5.2). Доска Ф1.1.2 требует Gate B «5 видов
> ребра в enum». Эта таблица **решает** расхождение, а не фиксирует его.

**КАНОН-имя:** `GraphEdgeKind`. **КАНОН-набор — ровно 5 видов** (совпадает с Gate B доски
Ф1.1.2 «5 видов ребра» и с аналогией §A.2 «веер наружу / внутрь / много-ко-многим /
много-выходов / петля возврата»):

```text
public enum GraphEdgeKind { FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop }
```

**Решение по спорным членам (какие — kind ребра, какие — деталь обхода/флаг, НЕ kind):**

| Спорный член | Где встречался | Решение | Куда переходит |
|---|---|---|---|
| `ReturnLoop` (=`BackEdge`/петля возврата) | 04 (Sequential≠это), 05 (`BackEdge`) | **kind ребра** (5-й канонический) | `GraphEdgeKind.ReturnLoop` |
| `Sequential` | 03, 04 | **НЕ kind**: порядок обхода зависимостей | деталь обхода `DomainGraphClosure`/`DependsOn`, не член enum |
| `OneToOne` | 02 | **НЕ kind**: вырожденный `FanOut` с одной целью | покрывается `FanOut` (1 цель) |
| `Bridge` | 03 | **НЕ kind**: knowledge-edge | флаг/контракт `KnowledgeEdge` на ребре, не член enum |
| `AsyncParallel` | 05 | **НЕ kind**: режим исполнения | булев флаг `ParallelEligible` ребра, не член enum |

**Обязательность ссылок:** блоки `codex-plan-02/03/04/05` §5 и Part A §0 (блок 00)
обязаны ссылаться на этот единый `GraphEdgeKind` (5 членов); локальные имена
`DynamicGraphEdgeKind` / `EdgeKind` / `GraphEdgeKind{...Bridge}` /
`EdgeKind{...BackEdge,AsyncParallel}` — **устаревшие псевдонимы**, расхождение числа
членов = дефект. Это приводит число членов в соответствие с Gate B доски Ф1.1.2 (ровно
5) и делает exit-gate Ф1.1 достижимым.

## A.4.2. КАНОН решателя ребра — `EdgeAuthorityUnit` (per-edge authority)

> Закрывает расхождение трёх I/O-сигнатур и двух имён. На диске обучаемый финальный
> решатель ребра объявлен как: `EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput,
> EdgeAuthorityDecision>` **и одновременно** `IDynamicGraphEdgeAuthority :
> ILogicalUnit<EdgeRoutingContext,EdgeRoutingDecision[]>` (оба в `codex-plan-02` §5);
> `ILogicalUnit<EdgeContext,EdgeDecision[]>` (`codex-plan-03`/`codex-plan-05` §5). Для
> одного R4-арбитра ребра допустим **только один** вход/выход (A-LM-09).

**КАНОН-имя:** `EdgeAuthorityUnit` (владелец канона — блок 02 `codex-plan-02` §5).
**КАНОН I/O (одна пара):**

```text
EdgeAuthorityUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
  вход  EdgeAuthorityInput   — leakage-free: БЕЗ candidate-index/позиции в списке,
                               БЕЗ SourcePath (НЕ переиспользует SubPhaseRouterSourceDocument,
                               у которого SourcePath есть — SubPhaseRouterUnit.cs:6-8);
                               нормализация входа до вызова
  выход EdgeAuthorityDecision — упорядоченные цели many→many (OrderedTargets[] +
                               PerTargetConfidence[]); Weight ребра = eval-only, НЕ авторитет
```

> **Точная привязка leakage-инварианта (улучшение vs бенчмарк).** Запрет `SourcePath` во
> входе `EdgeAuthorityInput` подкреплён реальным кодом: `SubPhaseRouterSourceDocument`
> объявлен с полем `SourcePath` (`21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs:6`),
> а действующий роутер уже **намеренно исключает** этот путь из скоринга
> (`MultiSubPhaseRouterUnit.cs:293` «Join semantic document fields while deliberately
> excluding SourcePath»; провенанс пути отдельно — `:424`/`:431`). То есть инвариант «не
> учить по пути источника» уже частично реализован в проекте и `EdgeAuthorityInput`
> обязан его наследовать как контракт, а не как комментарий.

**КАНОН поля-владельца авторитета на ребре.** На диске тип поля не согласован:
`string AuthorityUnitRef` (`codex-plan-03` §5), `string? AuthorityUnitId`
(`codex-plan-04` §5.2), `object AuthorityUnit`/`ILogicalUnit AuthorityUnit`
(`codex-plan-02`/`codex-plan-05` §5, Part A §0). **КАНОН-решение (закрывает Q13/Q15
одним значением):** на ребре хранится **`string AuthorityUnitId`** — ссылка в реестр
(`InMemoryLogicalUnitRegistry`), резолв обученного владельца через `ResolveActiveUnit`/
latest-wins (A-LM-03; реальный метод — `AdaptiveLogicalOrchestrator.cs:269`
`private ILogicalUnit<TInput,TOutput> ResolveActiveUnit<TInput,TOutput>(...)`), а **не**
прямой объект-юнит. Физическое место хранения ссылки — **поле ребра в
`DynamicGraphTopology`** (проекция реестра топологии), не отдельный реестр владельцев;
это и определяет, где живёт авторитет при export/import графа (блок 04 §3.4/§10). Блоки
`codex-plan-02/03/04/05` и Part A §0 обязаны использовать `string AuthorityUnitId`
единообразно; `object/ILogicalUnit AuthorityUnit` и `AuthorityUnitRef` — устаревшие
псевдонимы. Вопросы Q13/Q15 (§E) закрыты этим каноном.

---

# §B. GOVERNANCE / оркестрация-блок плана

> По CLAUDE.md «Plan governance orchestration pattern». Этот блок делает мастер-план
> само-управляемым: фиксирует требования, их маппинг на блоки/Gate B/артефакты,
> роль-loop, branch-правило, разрешённую верификацию, матрицу завершения, anti-stop и
> score-правила. Без этого блока мастер-план неполон.

## B.1. Authority Requirements Ledger (требования из задания Luba)

Источник полномочий: текущее задание Luba на сборку мастер-плана ILearningDynamicGraph
(CANON-блок задания) + 3 находки рекона. Каждое требование — binding.

| ID | Требование (из задания Luba / CANON / рекона) | Блок-владелец | Gate B (краткий) | Артефакт-доказательство |
|---|---|---|---|---|
| R-01 | Роли узел/поток/оркестратор/динам-граф = L2/L3/L4/L5 (маппинг ролей Luba) | 00, 05, 01, 02, 03, 04 | счётчик новых уровней = 0; маппинг с file:line | §C индекс + блоки 00/04/05 |
| R-02 | Авто-построение DPO из НЕсуществующих данных (генератор пар, не ручной набор) | 06, 07, 09 | `BuildPairAsync` на фикстуре даёт валидный `GraphDpoPair`; 5 форматов → 1 канон | `codex-plan-07`, `codex-plan-09` |
| R-03 | Teacher как ЦЕЛЬ дистилляции = только легальный open-weight permissive **в режиме plain (RR6)** | 08, 07, 10, 14 | `closed_proprietary_model=true && used_for_training=true ⇒ BLOCK/REBUILD`; **RR6: `service_mode≠plain ∨ grounding_used` && used_for_training ⇒ BLOCK (C17) даже для allowlist-модели** | `claude-plan-08` §4 (C2/C3/C17), §11.1 |
| R-04 | Judges из лучших моделей (включая closed) — только как СУДЬИ, не цель обучения **и только с пройденной terms-проверкой + plain (RR6)** | 06, 07, 08, 09, 12 | `judge_model` пишется (внутри provenance); 0 train-ready строк с teacher=closed; **RR6: closed-judge ⇒ ACCEPT (C11) только при `provider_terms_checked && terms_snapshot_ref≠PENDING && service_mode=plain`, иначе C11-blocked QUARANTINE / C17 BLOCK** | `claude-plan-08` §4 (C11/C11-blocked/C17), §3.6.2, §11.1 |
| R-05 | Легальный teacher-гейт + provenance как обязательное поле строки | 08 (владелец), все | **20 полей `TeacherProvenance`** (RR6: +`service_mode`/`grounding_used`); `provenance_field_presence_rate==1.0` по всем 20 — предусловие `dpo_train_ready`; service-mode-запрет C17 (grounding/search ⇒ BLOCK для train) отдельно от brand-denylist; манифест содержит только `verdict==ACCEPT` (enum 6 значений, +`BLOCK_FOR_COMMON` +`EVAL_ONLY`; канон §A.4 / `claude-plan-08` §3.3) | `claude-plan-08` §5/§8.4/§10 п.1/§11.1 |
| R-06 | Async мульти-комбинации (один вход → N комбинаций → лучший) | 06 (владелец), 03, 05 | для `budget.Max=4` стартует ровно 4 async; победитель != немаркированная формула | `codex-plan-06` |
| R-07 | Запрет скрытого детерминизма на ребре (A-LM-09/A-LM-13) | 12 (владелец), 02, 03, 04 | `classified_mechanism_count==9`; 0 немаркированных финальных авторитетов | `codex-plan-12` |
| R-08 | Обучаемость + lifecycle (LoRA+DPO, checkpoint/export/import/continue) | 10 (владелец), 04, 11 | fresh-load==trained; continue-training Δ>0; export/import round-trip SHA-256 | `codex-plan-10` |
| R-09 | Измерение скорость/точность/качество (held-out, ablation, без UNKNOWN) | 11 (владелец), 13 | `PASS_AS_TRAINED_FEATURE` только при held-out+ablation+lifecycle; 0 UNKNOWN | `codex-plan-11` |
| R-10 | SPLIT R5: общая модель/логика/сеты в llm_mvp_core; адаптивная — у пользователя | 04, 10, 14, 05 | манифест несёт `LegScope`; `DynamicGraphMachinePair` разделяет common/personal | `codex-plan-10` (9), `codex-plan-04` |
| R-11 | Тонкий агент R6 (агентный слой — только вызовы API, ядро в llm_mvp_core) | 14, 05 | 0 строк карты переносят алгоритм-логику в агентный слой | `codex-plan-14`, `codex-plan-05` §9 |
| R-12 | codex-префикс на всех артефактах плана-бенчмарка / claude-префикс на моих | мастер + все блоки | имя каждого собственного файла начинается с своего namespace; полнота индекса §F покрывает **все файлы СВОЕГО namespace (claude-*) пофайлово** (31 файл, §F.1) **И весь codex-* базы-бенчмарка по бакетам как evidence-only** (94 файла = 15 plan + 40 recon-core + 23 recon-mvp + 5 guide + 2 library + 9 other, §F.2-§F.6) | §F (полный индекс) |
| R-13 | План пишется через суб-агентов (Part A полнота §0..§5, branch-parity) | мастер (сборка) | §C индекс §0/§4/§5 + §1/§2/§3 присутствуют | §C |
| R-14 | «Вопросы в конец» (все открытые вопросы — в финальной секции) | мастер §E | секция вопросов в самом конце документа | §E |
| R-15 | Полный набор §0..§5 Part A на уровне глубины (Part A Completeness Contract) | 00, 05 + P-01..P-14 §1/§2/§3 | §0 `note for` на каждый класс + симуляции; §1 класс+метод+параметры+шаги | §C |
| R-16 | Тест-стратегия по 9 слоям FC-26, untrained-honesty + trained-acceptance раздельно | 13 (владелец), все блоки §9 | матрица 14 строк P-01..P-14, 0 пустых клеток, 3 IsTrainable c обеими колонками | `codex-plan-13` |
| **Находки рекона** | | | | |
| R-17 (рекон-1) | Контаминированные строки `sonnet-*` в `training-data/orchestration-reasoning/` (08: 19 файлов/72 вхождения; 07: 35 совпадений/10 файлов) → DATASET_REBUILD | 08, 07 | `contaminated_rows_marked_REBUILD == contaminated_rows_found` | `codex-plan-08` (2) |
| R-18 (рекон-2) | Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») — нужен канонический перед обучением | 07, 09 | назван канонический указатель | §E Q21 (блок 07) |
| R-19 (рекон-3) | Захардкоженный `RewardMargin` (литерал 0.03) в `27.TranslationDomain/TranslationMachine.cs:70/125/187` — эталон обучения сломан | 10, 11 | литерал 0.03 как RewardMargin устранён; margin из движка `11.DPO/DpoTrainer.cs` | `codex-plan-10` (2), `codex-plan-11` |

> **Точность R-19 (улучшение vs бенчмарк).** Бенчмарк цитировал `TranslationMachine.cs:70/
> 123-126/187`. Прямое чтение главного чекаута даёт точные строки литерала `0.03`:
> `:70` (`RewardMargin: 0.03,`), `:125` (тернар `? 0.03`), `:187`
> (`RewardMargin: _trainingHistory.Count > 0 ? 0.03 : 0.0,`). Диапазон `123-126`
> заменён точным `:125`.

> **Сверка нумераций (binding, crosswalk-заметка для no_mess): мастер `R-01..R-19` ↔
> задание Luba `R1..R20`.** Этот ledger §B.1 — внутренний срез мастер-плана
> (`R-01..R-19`, где `R-17/R-18/R-19` — находки рекона). **Claude-доска
> `claude-EXECUTION-CONTROL-BOARD.md` §C использует нумерацию `R1..R20` — формулировки
> Luba** (как в задании); эта матрица/мастер используют маппинг `R-01..R-19`. Это **одна
> истина в двух нумерациях**: множества `covered`/`partial` **идентичны** во всех трёх
> claude-файлах (`covered=12 / partial=8 / unmapped=0`). Полная построчная карта
> соответствия — в **claude-матрице §5** (единственный источник карты). Два требования
> задания живут в доске, а **НЕ** в таблице выше, и оба **binding (не optional)**:
>
> | Требование задания | Где живёт (владелец) | Gate B | Статус |
> |---|---|---|---|
> | **R20** — до 50 проходов review/fix | мастер §E входные замечания + доска §D Review-Fix протокол; Ф11.2 (10 execution reviews при реализации) | реальные ревью-пакеты (reviewer id, before/after ref, findings, fix, recheck); ledger-строки ≠ циклы | **partial** (benchmark-disputed; claude `real_pass=0`). Luba дословно оспорила заявку о 50 циклах: «no fucking way you made 50 cycles with a sub agent»; бенчмарк признал челлендж верным («The challenge is correct»). `codex-50-cycle-plan-review-supplement.md` несёт `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0` (100 ledger-строк ≠ доказанные циклы), тогда как `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` отдельно повторяет 50/50 — два evidence-файла **конфликтуют**. Для claude-* `real_review_fix_pass_count = 0` (claude-EXECUTION-CONTROL-BOARD.md:1433; прозаическая формулировка — :1384). Подробности — §F.6 |
> | **R19 (Luba, execution-visibility)** — строгая последовательность + полная execution-видимость + нумерация `Ф<P>.<SP>.<SS>` | доска §0.1 нумерация, §0.3 sequential gating, §A exit-gates, §B EXECUTION-VISIBILITY | каждый ID уникален без дыр; в любой момент виден текущий `Ф<P>.<SP>.<SS>` + счётчики done/total | covered доской |
>
> Внимание: `R19` мастера (находка рекона train-ready V192/V309) и `R19` задания Luba
> (execution-visibility) — **разные** требования с совпавшим номером; не путать.

## B.2. Contradiction sweep (противоречий нет)

Сквозная проверка терминами ledger по всем блокам: ни одно предложение мастер-плана или
блока не противоречит binding-требованию.

- «обучаемый граф» vs «без нового уровня» — не противоречие: DynamicGraph =
  специализация L5 (R-01, A-LM-12), не 6-й уровень.
- «teacher» vs «judge closed» — не противоречие: teacher только open-weight (R-03),
  judge может быть closed но не цель обучения (R-04).
- «детерминистический обход порядка рёбер ОК» vs «запрет детерминизма» — не
  противоречие: запрет относится к **финальной authority выбора ребра** (R-07), а
  порядок зависимостей (`DomainGraphClosure`, используется в
  `AdaptiveLogicalOrchestrator.cs:531/552`) — honest helper без claim.
- «matrix R5 = no-hidden-determinism `covered`» vs «`claude-plan-12` #4 = DEFERRED» —
  контрадикция **СНЯТА B-22** (§E.0 Q-mech4): механизм #4 (план фаз сценария
  `OrchestrationScenarioPhasePlanner.PlanPhases`) — отдельный L4-авторитет пути
  оркестрации, по прецеденту Q14 помечен `BASELINE_MUST_BE_LABELLED` (honest baseline без
  claim) + дан инстанцированный обучаемый блюпринт `SemanticPhasePlanUnit`; matrix R5 и
  `claude-plan-12` (§8.2/§7/Gate B п.6) приведены в согласие. Немаркированных финалов —
  0 и на ребре, и на L4-пути #4.
- «удалить baseline-роутер» — отсутствует и запрещён: удаление ≠ покрытие замены;
  `MultiSubPhaseRouterUnit` понижается до baseline/ablation, не удаляется
  (`codex-plan-12` Q5; см. §A.2.1).

## B.3. Main focus goal и проверка шагов

**Main focus goal:** обучаемый динамический граф (L5 над узлами/рёбрами) с авто-DPO
петлёй на **легальных** teacher-данных, с доказанной обучаемостью (held-out + ablation +
lifecycle) и SPLIT R5 / тонким агентом R6 — как **план-решение** (не реализация). Каждый
подшаг `Ф<P>.<SP>.<SS>` доски исполнения (`claude-EXECUTION-CONTROL-BOARD.md`, см. §D)
сверяется с этим goal колонкой «что решает».

## B.4. Branch-правило, allowed verification, forbidden tests

- **Branch (общее правило репозитория llm_mvp_core):** `git checkout` как «фикс»
  запрещён (CLAUDE.md branch convention). Прежняя формулировка «текущая ветка
  (`codex/chat-gpt-5.5`), не переключать» сохраняется как общий запрет на самовольное
  переключение, **но для этого проекта (DPO legal build) действует Политика веток
  (NEW) ниже**: authority-веткой считается самый последний обновлённый branch каждого
  участвующего репозитория, а рабочая ветка проекта называется `DPOLegalLearning`.
- **Политика веток (NEW) — единая для всех участвующих репозиториев.** В **каждом**
  участвующем репозитории authority = самый последний **ОБНОВЛЁННЫЙ** branch (по дате
  последнего коммита) на момент старта работ; рабочая ветка проекта **создаётся ОТ
  authority** и называется **`DPOLegalLearning`**. Участвующие репозитории (5):
  - `llm_mvp_core` (ядро: общая модель/логика/обучающие сеты, R5);
  - `xiigen-platform-dot-net-angular` (хост-платформа маркетплейс-клиентов, R16/R17);
  - `xiigen-flows-dot-net-angular` (шов дизайн→анализ `FLOW-31`, R16; better-output
    `FLOW-38`, R17);
  - `FigmaToCode-design-gen` (клиент генерации html+css, R16);
  - `CanvaToCode` (клиент генерации html+css, R16).

  > **План-only оговорка.** Это **политика плана**, а не разрешение действовать:
  > определение authority-ветки и создание `DPOLegalLearning` от неё — это **реализация**
  > (git-запись), которая в текущей задаче запрещена (см. ниже Forbidden) и выполняется
  > только после свежего явного approve Luba. Фактическая дата «последнего обновлённого
  > branch» фиксируется на момент старта работ, не на момент написания плана.
- **Allowed verification для плана:** чтение артефактов (Read), счётные проверки Gate B
  по тексту блоков (`note for` на каждый класс, число связей в таблице симуляции, число
  NEW-типов, число открытых вопросов), сверка цитат `file:line` прямым чтением кода.
- **Forbidden:** `dotnet test`/`dotnet build`/`dotnet run`, раннеры, тренировка,
  git-запись, внешние AI/API/модели — это план-only, не реализация (Rule 4: full test
  run не ритуал для документации-фазы).

## B.5. Роль-loop (внутренние состояния, не user-facing паузы)

```text
ARCHITECT_INTERNAL -> EXECUTOR_INTERNAL -> REVIEW_INTERNAL
   -> ARCHITECT_INTERNAL_REPAIR -> EXECUTOR_INTERNAL_CONTINUE
```

Для этого мастер-плана (документ-сборка в namespace claude-*) активная роль —
автор-сборщик блока плана. Реализация продукта по строго последовательной доске
исполнения (`claude-EXECUTION-CONTROL-BOARD.md`, см. §D) начинается только после
**свежего approve Luba** этого плана (R2) и идёт через bounded суб-агентов под
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
| Трейнабл-решения | `trainable-architecture-decision-SKILL.md` (Graph-Style Reconstruction Gate + Graph-DPO Artifact Triad Gate + Legal Teacher/Provenance Gate), `dpo-triple-format-SKILL.md` |

> **Тест-стратегия (улучшение vs бенчмарк, по `plan-review` FC-26).** Для каждой
> ветки Part A/Part B блок 13 обязан дать стратегию по 9 слоям: unit, integration,
> e2e/host, QA-automation, UX-automation, negative, untrained-behavior,
> trained-acceptance, regression. Два PASS-состояния держатся раздельно: untrained →
> `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (доказать честный `not_ready`, отсутствие скрытого
> детерминизма), trained → `PASS_AS_TRAINED_FEATURE` (checkpoint + held-out + ablation +
> fresh-load + continue-training + e2e). Ветка без стратегии = admission-blocking hole.

## B.7. Document RAG lookup order (для незакрытых вопросов)

1. Активный план (этот мастер + 16 блоков бенчмарка как evidence).
2. Document RAG плана: `docs/ARCHITECTURE.md`, `docs/COMPONENT-INTERFACES.md`,
   `docs/DECISIONS-LOCKED.md`, `docs/EXPERIMENT-RESULTS-MAP.md`, `docs/SUBPROJECT-MAP.md`.
3. Локальные контракты `00.Core/*` и реестр/репорт-артефакты (25.ModelServer,
   18.CapabilityTraining).
4. Реконы/гайды `codex-recon-*`/`codex-guide-*` (evidence-only).
5. Если ответа нет — вопрос идёт в §E (не пауза, если безопасный default выводим).

## B.8. State-поля и точки обновления

`STATE.json` (при реализации): `main_focus_goal`,
`current_phase_substep (Ф<P>.<SP>.<SS>)`, `next_action`,
`goal_reached(=false до полного завершения)`, `stop_allowed(=false)`,
`plan_completion_hole_register[]`, `reviewed_commit`, `two_pass_status`
(scaffold/trained per узел). Источник `phase_queue` и статусов исполнения — claude-доска
`claude-EXECUTION-CONTROL-BOARD.md` (§D); обновляется на каждом переходе подшага
`Ф<P>.<SP>.<SS>` по протоколу видимости доски §B.

## B.9. Anti-loophole register и repair path

- False-completion при открытом плане → `-1000`, запись в hole register, ремонт
  governance, продолжение от первого hole (CLAUDE.md «Plan-incomplete finish recovery»).
- Soft-stop при известном next_action → `-1000` (Partial-progress stop firewall).
- Скаффолд, заявленный как trained → FAIL `two_pass_conflation` (OP-57/FC-28).
- Детерминистический финал под claim «обучено» → `FORBIDDEN_RUNTIME_SHORTCUT` (A-LM-09).
- Claim сильнее механизма (например `isLearned:false` как несуществующее поле) →
  `REPORT_CLAIM_TOO_STRONG`/A-LM-13: понизить формулировку до фактического механизма
  (см. §A.2.1).

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
groups»), верификация-перед-коммитом — хард-гейт. Для этого мастер-плана git — **вне
scope** (NOT THE TASK).

## B.12. Сводка канонических чисел (single source of truth внутри плана)

> Эта таблица сведена явно (улучшение vs бенчмарк), чтобы любое расхождение чисел между
> §A, §C, §D, §F ловилось одним взглядом.

| Канон | Число | Где владелец |
|---|---|---|
| Фазы доски (Ф0..Ф11) | **12** | доска §0.1 / §1 |
| Подфазы доски | **26** | доска §0.1 / §1 |
| Базовые подшаги `Ф<P>.<SP>.<SS>` | **59** | доска §0.1 / §1 |
| NEW-типы | **7** | §A.4 |
| Члены `GraphEdgeKind` | **5** | §A.4.1 |
| Поля `GraphDpoPair` | **14** | §A.4 / блок 07 |
| Поля `TeacherProvenance` (13 базовых + 5 NEW + 2 RR6) | **20** | §A.4 / блок 08 §3.1 |
| Классифицированные deterministic-механизмы (P-12) | **9** | блок 12 / R-07 |
| Логические блоки Part A (в 15 файлах `codex-plan-*`) | **16** | §F.2 |
| Покрытие требований (cycle-37 honest; R20 понижен в partial) | covered **12** / partial **8** / unmapped **0** | матрица §D.2 |
| Returned review packets (R20, benchmark, disputed; claude `real_pass=0`) | **50/50 заявлено в бенчмарке, оспорено** | §F.6 |

---

# §C. Part A — индекс (§0/§4/§5 + блоки P-01..P-14 как решённые §1/§2/§3)

> Мастер-план не дублирует блоки. Каждая строка ниже — указатель на полностью решённый
> блок-документ с его собственными UML, симуляциями, цитатами кода и измеримым Gate B.
> Все пути — в базе `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.
>
> **Источник решения = `claude-plan-*` (синхрон с §F.1).** Полностью решённые блоки
> Part A (§0/§4/§5 + §1/§2/§3 для P-00..P-14) живут в **моих** файлах `claude-plan-*`
> (перечень — §F.1, 15 файлов). Именно на них указывают строки §C.1-§C.4 как на источник
> решения. Бенчмарк-аналоги `codex-plan-*` (§F.2) — **benchmark-evidence, read-only**: они
> читались как образец и местами являются стабами (не полными блоками), поэтому **не**
> являются источником решения и в §C не цитируются как «решённый блок-источник».

## C.1. §0 — UML всей системы + §4 — статус → `claude-plan-00-partA-uml-status.md`

- **Содержит:** полная Mermaid `classDiagram` всей системы (25 классов в 6 группах,
  `note for` на каждом), таблица симуляции E1..E31 (вход→результат, три ребра авторитета
  E10/E15/E18 различают `authority_now` baseline vs `authority_target` trained), §4
  статус-таблица всех 25 компонентов (honest: ОТСУТСТВУЕТ trained DynamicGraph, BASELINE
  `MultiSubPhaseRouterUnit` — без обученного состояния, §A.2.1).
- **Gate B (измеримо):** 25/25 классов с `note for`; 31/31 связей в таблице симуляции; 0
  NEW без сигнатуры; 0 существующих ссылок без `file:line`; импорт Mermaid в draw.io без
  правок.

## C.2. §5 — Иерархия уровней + помеченные цепочки → `claude-plan-05-partA-hierarchy.md`

- **Содержит:** ровно 5 уровней с типами Input/Output (`file:line` для существующих, NEW
  для новых); 4 помеченные цепочки расписаны пошагово — (a) inference L5→4→3→2, (b)
  обучение только на L5, (c) авто-DPO петля, (d) async мульти-комбинация; таблица
  симуляции S1..S12; решение «где хранить `GraphDpoPair`/`TeacherProvenance`» (проекция
  на `LearningTrainingItem` `:30` + боковой DPO-манифест, 0 правок контрактов 00.Core).
- **Gate B (измеримо):** 5/5 уровней с Input/Output; 4/4 цепочки пошагово; S1..S12
  симуляций; 0 немаркированных детерминистических финалов; `TeacherProvenance` present;
  0 правок сигнатур L1..L5.

## C.3. §4 — статус (источник) → `claude-plan-00-partA-uml-status.md` §7

Статус-таблица всех компонентов (работает / BASELINE / NEW / ОТСУТСТВУЕТ) живёт в блоке
00 §7. Двух-PASS вывод системы: `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (контракты пяти
уровней работают, скрытого детерминизма «под видом trained» нет, но обученного
DynamicGraph — checkpoint/ablation/fresh-load — ещё нет). Это **НЕ**
`PASS_AS_TRAINED_FEATURE`.

## C.4. Блоки P-01..P-14 (полностью решённые §1 алгоритмы / §2 описания / §3 цитаты кода)

> Каждый блок несёт §1 (алгоритм: класс + метод + параметры + шаги), §2 (описание:
> аналогия + структуры данных + «что НЕ делает» + сигнатуры), §3 (цитаты кода `file:line`
> + сейчас/почему/должно стать/как менять), §4 статус, §5 контракты, §8 гейты, §9
> тест-стратегия FC-26 (9 слоёв; UH/TA раздельно для трейнабл-строк), §10 Gate B, §12
> вопросы.

| Блок | Файл (источник решения, `claude-plan-*`) | Уровень/тема | Ключевой Gate B (измеримо) |
|---|---|---|---|
| **P-01** | `claude-plan-01-node.md` | Узел графа: `IDynamicGraphNode` поверх L2 `ILogicalUnit`, «упрощённая матрица» features·weights→top-N + honest NoDecision | `Train`-методов на узле = 0 (A-LM-02); 3 гейта (legal/no-shortcut/anti-leakage); миграция 3 классов случаев с `file:line`; 9 слоёв FC-26 c UH+TA |
| **P-02** | `claude-plan-02-edge.md` | Ребро графа: L3 `ILogicalFlow` + обучаемая per-edge authority `EdgeAuthorityUnit` | `OrderedTargets` инвариантен к 5 перестановкам (PASS); held-out `PassesGate`, RewardMargin>0, After>Before; ablation просадка ≥ порог; `exactPromptLeakageRate==0.0`; lifecycle fresh-load/continue/export-import |
| **P-03** | `claude-plan-03-orchestrator.md` | Оркестратор L4 (async): обход графа fan-out/fan-in/петля/knowledge-edge | G1..G8 = 8/8; обход с `file:line`; подмена baseline→trained без нового кода в `RunAsync` через `ResolveActiveUnit` (`AdaptiveLogicalOrchestrator.cs:269`, latest-wins, A-LM-03); 0 скрытых финалов на L4 |
| **P-04** | `claude-plan-04-dynamicgraph-machine.md` | L5-машина `ILearningMachine<DynamicGraphDomain>` | маппинг с `file:line` (`:208`); каждое ребро с обучаемым владельцем + baseline-роль; 5 методов пошагово; 7 NEW-типов; число рёбер-с-authority == числу типов рёбер |
| **P-06** | `claude-plan-06-async-combinations.md` | Async мульти-комбинации (1 вход → N → лучший) | `budget.Max=4` → ровно 4 async (`Task.WhenAll`); победитель != немаркированная формула (ablation); из N≥2 строятся `GraphDpoPair[]` overlap<0.60, order-invariant; ветки B/C honest |
| **P-07** | `claude-plan-07-dpo-builder.md` | Авто-строитель DPO: канон `GraphDpoPair` (14 полей) | 5 форматов → 1 канон (integration-тест); `BuildPairAsync` overlap<0.60, reason≥24; `TeacherProvenanceGate` closed→`DATASET_REBUILD_REQUIRED`; permutation order-invariant; QA-скан 0 train-ready с closed-маркерами; материализация в `DpoDataset.Load` (`11.DPO/DpoDataset.cs:18`) |
| **P-08** | `claude-plan-08-legal-teacher-gate.md` | Легальный teacher-гейт + provenance (20 полей, RR6) | `provenance_field_presence_rate==1.0` по всем 20 (RR6: +`service_mode`/`grounding_used`); `contaminated_rows_marked_REBUILD == found`; **18 веток C2-C17 (+C11-blocked)** вердикта (C9 privacy/raw-no-redaction ⇒ `BLOCK_FOR_COMMON`; C12 judge_became_teacher ⇒ `DATASET_REBUILD_REQUIRED`; C13 accept_full_provenance ⇒ ACCEPT при 20/20 + triad; C14 self_judge_train_ready_without_legal_gate ⇒ запрет; C15/C15b Graph-DPO Artifact Triad ⇒ eval-only; C16 source_category ∈ {client_unverified,third_party} && used_for_training && НЕ allowlisted ⇒ QUARANTINE [R17, marker-absence≠open_weight]; **C17 RR6 service_mode≠plain ∨ grounding_used ⇒ BLOCK [отдельно от brand-denylist]; C11-blocked RR6 closed-judge без terms-проверки ⇒ QUARANTINE**); accept с пустым provenance == 0; closed-judge при `used_for_training=false` + terms-проверен + service_mode=plain ⇒ ACCEPT; манифест только `verdict==ACCEPT`. **Владелец таблицы веток — claude-plan-08 §4/§11.1; при расхождении числа/семантики веток главенствует блок 08 §4** (мастер индексирует, не переопределяет) |
| **P-09** | `claude-plan-09-auto-dpo-loop.md` | Авто-DPO петля (сердце проекта) | в `TrainBatchAsync` ≥1 авто-пара (не []); legal+anti-leakage PASS; held-out reward≥0, loss<0.693, After≥Before+0.02; промоушен только при росте; `ShouldStop` из {threshold,plateau,budget}; honest two-PASS метка |
| **P-10** | `claude-plan-10-training-lifecycle.md` | Обучение+lifecycle: LoRA+DPO, починка заглушек, split R5 | 10 строк: LoRA реально вклеена (ablation Δ≠0); литерал 0.03 устранён (R-19); continue-training Δ>0 ≥2 шага; export/import SHA-256; fresh-load без replay; closed→REBUILD; манифест `LegScope`+pair common/personal; 0 UNKNOWN |
| **P-11** | `claude-plan-11-measurement.md` | Измерение: скорость/точность/качество | `GraphMeasurementReport` PASS_AS_TRAINED только при: held-out source-disjoint + Δ≥+0.02 + reward≥0.025 + ablation + 5 leakage/permutation контролей + lifecycle; 0 UNKNOWN; live margin никогда как приёмочная точность |
| **P-12** | `claude-plan-12-no-hidden-determinism.md` | Классификация и замена скрытого детерминизма | `classified_mechanism_count==9`; каждый forbidden/baseline имеет blueprint с trainable-классом (`SemanticEdgeRouterUnit` NEW), typed state, training-algo, excluded-leakage, `CandidateOrderLeakageGuardUnit` NEW, ablation; every quote has `file:line`; 0 новых уровней |
| **P-13** | `claude-plan-13-test-strategy.md` | Консолидированная тест-стратегия (FC-26) | матрица 14 строк P-01..P-14, 0 пустых клеток (9 колонок слоёв); каждая ✔ с assertion/Gate; 3 IsTrainable c обеими колонками UH+TA; P-12 continue-training прогоном; 6 гейтов §8 сопоставлены |
| **P-14** | `claude-plan-14-stage-references.md` | Карта референсов (Stage 2) | `count(distinct StageId)==13` (P-01..P-13); 6 осей-проектов ≥1 строкой; 34/34 путей существуют; teacher/router строки помечены legal/baseline; 0 строк на claude-*/dcm-* как target |

> **Branch-parity (Part A core == Part B core):** мастер-план держит Part A (§0/§4/§5 +
> §1/§2/§3 в блоках) как единый источник истины. Part B (work-orders/раннеры/gate-команды)
> появляется при реализации и не должен вводить компоненты, невидимые в Part A, или иной
> алгоритм (OP-49). На стадии плана Part B ещё не написан — реализация требует approve
> Luba.

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

> Мастер-план **не дублирует** execution-таблицу и матрицу — он на них **ссылается**.
> Источник истины по порядку/статусам — доска (`Ф<P>.<SP>.<SS>`); источник трассировки
> требований — матрица. Это устраняет конфликт «двух источников истины» (старая
> PH-нумерация полностью переведена в `Ф<P>.<SP>.<SS>`).

## D.1. Доска исполнения — единственный источник истины по execution-control

**`claude-EXECUTION-CONTROL-BOARD.md`** (мой namespace claude-*, см. §F.1) —
единственный источник истины по execution-control реализации ILearningDynamicGraph.
Бенчмарк `codex-EXECUTION-CONTROL-BOARD.md` — **evidence-only** (см. §F.3): он читается
как образец, но governance моего мастер-плана указывает на claude-доску, а не на
codex-evidence. Всё, что касается порядка работ, статусов, видимости и закрытия фаз,
живёт **только в claude-доске**:

- **Нумерация `Ф<P>.<SP>.<SS>`** (Фаза.ПодФаза.ПодШаг): 12 фаз (Ф0…Ф11), 27 подфаз, 64
  подшага (включают активный пилот Ф10.3 `language-translate`, 5 подшагов; Q41 RESOLVED);
  каждый ID уникален, без дублей, без дыр (доска §0.1, §1 счётчики).
- **Строгая последовательность (SEQUENTIAL GATING)**: фаза N+1 не стартует, пока
  exit-gate фазы N не пройден на 100%; подфаза/подшаг — аналогично; cross-phase
  зависимости через поле «Зависит от» (доска §0.3, exit-gate каждой фазы в §A).
- **Видимость (EXECUTION-VISIBILITY)**: в любой момент видно текущий `Ф<P>.<SP>.<SS>`,
  что решается, счётчики done/total, что осталось, протокол обновления (доска §B).
- **DAG зависимостей фаз** с обоснованием порядка (доска §2).
- **Статусы**: `done | in-progress | remaining | review-block | blocked`; на момент
  плана все 59 базовых подшагов = `remaining` (план-only до approve Luba).

> Любой реализационный порядок, статус или счётчик берётся из claude-доски
> (`claude-EXECUTION-CONTROL-BOARD.md`), а не из мастер-плана. При расхождении
> **claude-доска главнее** (single source of truth по execution-control). Бенчмарк
> `codex-EXECUTION-CONTROL-BOARD.md` в расхождении не участвует — он evidence-only.

## D.2. Матрица покрытия требований — отдельный артефакт

**`claude-REQUIREMENTS-COVERAGE-MATRIX.md`** (мой namespace claude-*, `status:
plan_only_claude_owned`, см. §F.1) — трассировка каждого требования R1..R20 (задание
Luba) на конкретные подшаги доски `Ф<P>.<SP>.<SS>`, их Gate B, артефакт-источник и статус
покрытия (`covered | partial | unmapped`). Бенчмарк
`codex-REQUIREMENTS-COVERAGE-MATRIX.md` — **evidence-only** (см. §F.3). Claude-матрица
содержит:

- матрицу R1..R20 → подшаги → Gate B → артефакт → статус (матрица §1);
- проверку «нет скрытых под-задач»: все 59 базовых подшагов трассируются (матрица §2);
- список `partial`-требований с gap и действием (матрица §3);
- итоговые счётчики покрытия (матрица §4).

> **Честное состояние покрытия (cycle-37):** `covered=12`
> (R2/R3/R4/R5/R7/R8/R10/R11/R12/R13/R18/R19), `partial=8`
> (R1/R6/R9/R14/R15/R16/R17/**R20**), `unmapped=0`, истинных сирот-дефектов=0. Старая
> W2c-пометка `covered=15/partial=5` quarantined как false. **R20 понижен с `covered` до
> `partial` (benchmark-disputed, claude `real_pass=0`):** заявка о 50 циклах оспорена Luba
> дословно и признана бенчмарком верной; `codex-50-cycle-plan-review-supplement.md` несёт
> `subagent_cycles_completed=0`, тогда как `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`
> повторяет 50/50 — два evidence-файла конфликтуют; для claude-* доказанных проходов нет
> (claude-EXECUTION-CONTROL-BOARD.md:1433; прозаическая формулировка — :1384). Полное обоснование — §F.6 (Dishonest Claim
> Rejection Gate; ledger-строки ≠ циклы).
>
> **Doc-sync note (binding):** инлайн-счёт выше — единая честная истина для всех трёх
> claude-артефактов. Пересинхрон ВЫПОЛНЕН: claude-матрица `claude-REQUIREMENTS-COVERAGE-MATRIX.md`
> (§6 счётчики + §7 строка R20) и claude-доска `claude-EXECUTION-CONTROL-BOARD.md` (§C шапка
> covered/partial + §C строка R20 + CYCLE-PLAN-ONLY HONEST BOARD STATUS) несут тот же срез
> `covered=12/partial=8/unmapped=0` с R20 в `partial` — расхождения числа между тремя файлами
> нет. Любое будущее появление `covered=13/partial=7` или `covered=15/partial=5` = регрессия
> консистентности (источник истины по R20 — данная честная пометка).
>
> Матрица §B.1 этого мастер-плана (Authority Requirements Ledger, R-01..R-19) —
> внутренний ledger требований мастер-плана. Claude-матрица покрытия
> (`claude-REQUIREMENTS-COVERAGE-MATRIX.md`) трассирует **задание Luba R1..R20** на
> подшаги доски. Это два совместимых среза; сверка нумераций — в claude-матрице §5 п.1.

## D.3. Мост PH (старая нумерация) → Ф (доска) — для читаемости истории

> Навигационный мост (не источник истины): прежние ссылки на PH-фазы остаются понятными;
> исполнять — по доске.

| Прежний PH (история) | Фаза доски `Ф<P>` | Подшаги доски `Ф<P>.<SP>.<SS>` | Блок-источник |
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
| PH-04.2 обучаемый `EdgeAuthorityUnit` | **Ф4** (cutover в **Ф8**) | Ф4.3.2; Ф8.* | 02, 03, 04, 12 |
| PH-08.1 измерение | **Ф5** Измерение | Ф5.1.1–3, Ф5.2.1–2 | 11 |
| PH-05.1 async N-комбинаций | **Ф6** Async-комбинации | Ф6.1.1–2, Ф6.2.1 | 06, 03, 05 |
| PH-05.2 many-to-many merge | **Ф6** | Ф6.2.2 | 05, 02, 06 |
| PH-06.1 авто-DPO петля | **Ф7** Авто-DPO петля | Ф7.1.1–2, Ф7.2.1–2 | 09, 07, 08 |
| PH-09.1 замена скрытого детерминизма | **Ф8** No-hidden-determinism | Ф8.1.1–2, Ф8.2.1–2 | 12 |
| PH-10.1 сквозная тест-стратегия | **Ф9** Тесты/QA матрица | Ф9.1.1–2, Ф9.2.1–2, Ф9.3.1–2 | 13 |
| PH-11.1 карта референсов+docs-sync | **Ф10** API/клиенты | Ф10.1.1–2, Ф10.2.1–2 | 14 |
| (добавлено доской) финальная приёмка | **Ф11** Финальная приёмка | Ф11.1.1, Ф11.2.1 | мастер §B; CLAUDE.md |

> Правило завершения (Completion coverage matrix): любая строка матрицы покрытия со
> статусом «не доказано» → единственный честный статус `IN_PROGRESS_NEXT_STEP_KNOWN`,
> следующее действие берётся из первой недоказанной строки. На стадии плана все статусы
> доски = `remaining` (план-only) — это корректное состояние плана-документа, не
> false-completion.

## D.4. Anti-stop напоминание для исполнителя (при реализации)

Перед любым финальным ответом в многошаговой реализации — перечитать `STATE.json`. Если
`goal_reached=false`, `stop_allowed=false`, `next_action` непуст → выполнять
`next_action`, не отчитываться о частичном чекпойнте. Soft-stop при известном next_action
= `-1000` (§B.10). Статус и следующий подшаг берутся из claude-доски
`claude-EXECUTION-CONTROL-BOARD.md` (§D.1).

---

# §F. Индекс артефактов (карта всех файлов плана)

> Единый перечень всех артефактов плана ILearningDynamicGraph. База путей —
> `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`. Этот claude-документ —
> **мой** редактируемый артефакт (namespace claude-*). Все `codex-*` (план Codex, recon,
> control-таблицы, benchmark-report) и старый каркас `claude-00..07` — **evidence-only**:
> я их читаю как бенчмарк/доказательство, но не редактирую и не продолжаю.

## F.1. Мои редактируемые артефакты (namespace claude-*) — полный пофайловый перечень

> **Полнота индекса (закрывает Gate B R-12 «полнота индекса §F покрывает все файлы
> СВОЕГО namespace (claude-*) пофайлово И весь codex-* базы-бенчмарка по бакетам как
> evidence-only», §B.1 строка 307).** Эта секция §F.1 закрывает первую половину формулы
> (claude-* пофайлово, 31 файл); вторая половина (codex-* по бакетам, 94 файла с честным
> disk-счётом) — в §F.2-§F.6, итоговая сверка disk-счёта codex-базы — в §F.6.
> Ниже перечислен **весь** мой редактируемый namespace
> `claude-*` пофайлово, разбитый как у бенчмарка (codex §F): управляющие → `claude-plan-*`
> → `claude-guide-*` → старый каркас. Счётчик равен реальному disk-счёту: **31
> `claude-*.md`** (3 управляющих + 15 `claude-plan-*` + 5 `claude-guide-*` + 8
> `claude-00..07`). База путей — `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.

**(1) Управляющие артефакты (мои источники истины — 3 файла):**

| # | Артефакт | Роль |
|---|---|---|
| 1 | `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | **Этот файл** — улучшенный мастер-план (моя единая точка входа, индекс блоков, governance §B, вопросы §E) |
| 2 | `claude-EXECUTION-CONTROL-BOARD.md` | Доска исполнения — **мой единственный источник истины по execution-control** (`Ф<P>.<SP>.<SS>`, 12 фаз/27 подфаз/64 подшага с активным пилотом Ф10.3 `language-translate`, exit-gates, EXECUTION-VISIBILITY, DAG); см. §D.1 |
| 3 | `claude-REQUIREMENTS-COVERAGE-MATRIX.md` | **Моя матрица покрытия** требований R1..R20 → подшаги → Gate B → артефакт → статус (`status: plan_only_claude_owned`); см. §D.2 |

**(2) Решённые блоки Part A (§0/§4/§5) + P-01..P-14 — 15 файлов `claude-plan-*`:**

| # | Файл `claude-plan-*` | Логический блок / тема |
|---|---|---|
| 1 | `claude-plan-00-partA-uml-status.md` | §0 UML всей системы + §4 статус |
| 2 | `claude-plan-01-node.md` | P-01 узел графа (L2 `IDynamicGraphNode`) |
| 3 | `claude-plan-02-edge.md` | P-02 ребро графа (L3 + per-edge `EdgeAuthorityUnit`; владелец канона §A.4.2) |
| 4 | `claude-plan-03-orchestrator.md` | P-03 оркестратор (L4, async-обход) |
| 5 | `claude-plan-04-dynamicgraph-machine.md` | P-04 L5-машина `ILearningMachine<DynamicGraphDomain>` |
| 6 | `claude-plan-05-partA-hierarchy.md` | §5 иерархия уровней + цепочки исполнения |
| 7 | `claude-plan-06-async-combinations.md` | P-06 async мульти-комбинации (1 вход → N → лучший) |
| 8 | `claude-plan-07-dpo-builder.md` | P-07 авто-строитель DPO + канон `GraphDpoPair` |
| 9 | `claude-plan-08-legal-teacher-gate.md` | P-08 легальный teacher-гейт + provenance (**владелец канона веток C2-C17 +C11-blocked §4; RR6 service-mode/terms §11.1**) |
| 10 | `claude-plan-09-auto-dpo-loop.md` | P-09 авто-DPO петля |
| 11 | `claude-plan-10-training-lifecycle.md` | P-10 обучение+lifecycle (LoRA+DPO, split R5) |
| 12 | `claude-plan-11-measurement.md` | P-11 измерение (held-out/ablation/two-PASS) |
| 13 | `claude-plan-12-no-hidden-determinism.md` | P-12 замена скрытого детерминизма |
| 14 | `claude-plan-13-test-strategy.md` | P-13 консолидированная тест-стратегия (FC-26) |
| 15 | `claude-plan-14-stage-references.md` | P-14 карта референсов (Stage 2) |

**(3) Гайды — 5 файлов `claude-guide-*`:**

| # | Файл `claude-guide-*` | Тема |
|---|---|---|
| 1 | `claude-guide-01-how-to-build-dpo.md` | как строить DPO-пары |
| 2 | `claude-guide-02-how-to-train.md` | как обучать (LoRA+DPO, lifecycle) |
| 3 | `claude-guide-03-how-to-measure-quality.md` | как мерить скорость/точность/качество |
| 4 | `claude-guide-04-ilearningmachine-pattern-and-graph.md` | паттерн `ILearningMachine` и граф |
| 5 | `claude-guide-05-auto-dpo-loop.md` | авто-DPO петля |

**(4) Старый каркас — 8 файлов `claude-00..07` (мой namespace, но evidence-only в этой задаче):**

| # | Файл | Тема |
|---|---|---|
| 1 | `claude-00-library-index.md` | индекс ранней planning-library |
| 2 | `claude-01-authority-requirements-ledger.md` | ранний ledger требований |
| 3 | `claude-02-source-inventory-and-reference-map.md` | инвентарь источников и карта референсов |
| 4 | `claude-03-ilearningmachine-pattern-notes.md` | заметки по паттерну `ILearningMachine` |
| 5 | `claude-04-dpo-legal-model-policy.md` | политика легальных моделей DPO |
| 6 | `claude-05-dpo-data-training-measurement-guide.md` | данные/обучение/измерение DPO |
| 7 | `claude-06-ilearningdynamicgraph-architecture-brief.md` | ранний бриф архитектуры графа |
| 8 | `claude-07-execution-gates-open-questions.md` | exit-gates и открытые вопросы |

> **Disk-счёт (Gate B R-12, claude-namespace):** 3 + 15 + 5 + 8 = **31 `claude-*.md`** —
> совпадает с реальным перечнем на диске; первая половина формулы Gate B R-12 (полнота
> §F по claude-* пофайлово) достигнута. Вторая половина (весь codex-* базы по бакетам как
> evidence-only) закрывается в §F.2-§F.6 с честным disk-счётом codex-базы в §F.6.
>
> **Пометка по старому каркасу `claude-00..07`:** это ранняя planning-library; **в этой
> задаче он evidence-only** (не редактируется, не продолжается). Канон GraphDpoPair/
> TeacherProvenance/счётчики держатся согласованными с ним и с бенчмарком (тот же
> ILearningMachine-паттерн, тот же legal-гейт, та же иерархия L1..L5).

## F.2. Бенчмарк-блоки Part A (§0/§4/§5) + P-01..P-14 — 15 файлов `codex-plan-*` (evidence-only)

> 15 физических файлов `codex-plan-*` несут **16 логических блоков** Part A: блок 00
> содержит §0 UML + §4 статус (два логических блока в одном файле), блок 05 — §5 иерархию;
> остальные — P-01..P-14 (без отдельного P-05, т.к. §5 живёт в `codex-plan-05`).

| # | Файл `codex-plan-*` (evidence) | Логический блок / тема | Фаза(ы) доски |
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

## F.3. Управляющие артефакты бенчмарка (evidence-only)

| # | Артефакт | Роль |
|---|---|---|
| 1 | `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` | Мастер-план Codex (бенчмарк, по которому построена эта улучшенная версия) |
| 2 | `codex-EXECUTION-CONTROL-BOARD.md` | **Бенчмарк** доски исполнения (`Ф<P>.<SP>.<SS>`, exit-gates, видимость, DAG); evidence-only — НЕ источник истины для моего плана (мой источник — `claude-EXECUTION-CONTROL-BOARD.md`, §F.1/§D.1) |
| 3 | `codex-REQUIREMENTS-COVERAGE-MATRIX.md` | **Бенчмарк** матрицы покрытия R1..R20 → подшаги → Gate B → артефакт; evidence-only — мой источник трассировки — `claude-REQUIREMENTS-COVERAGE-MATRIX.md` (§F.1/§D.2) |
| 4 | `codex-BENCHMARK-DELTA-REPORT.md` | Benchmark Delta Report Codex (сравнение/ledger дельт) |

## F.4. Гайды (5 файлов `codex-guide-*`, evidence-only)

| # | Файл | Тема |
|---|---|---|
| 1 | `codex-guide-01-how-to-build-dpo.md` | как строить DPO-пары |
| 2 | `codex-guide-02-how-to-train.md` | как обучать (LoRA+DPO, lifecycle) |
| 3 | `codex-guide-03-how-to-measure-quality.md` | как мерить скорость/точность/качество |
| 4 | `codex-guide-04-ilearningmachine-pattern-and-graph.md` | паттерн `ILearningMachine` и граф |
| 5 | `codex-guide-05-auto-dpo-loop.md` | авто-DPO петля |

## F.5. Библиотеки знаний + реконы (evidence-only)

> **Честный счёт (Dishonest Claim Rejection Gate).** Библиотеки индексируют рекон-**файлы/
> отчёты**, а НЕ «проходы/passes»: на диске ровно 23 `codex-recon-mvp-*.md` и 40
> `codex-recon-core-*.md` (`codex-knowledge-library-llm-mvp.md:319` «23 рекон-файла»;
> `codex-knowledge-library-llm-mvp-core.md:7` «40 рекон-отчётов»). Приравнивание «1 файл
> = 1 проход» запрещено (count-инфляция).

| Группа | Префикс | Охват |
|---|---|---|
| Библиотеки | `codex-knowledge-library-*` | 2 файла: `-llm-mvp` (индекс 23 рекон-файлов) и `-llm-mvp-core` (индекс 40 рекон-отчётов) |
| Рекон ядра | `codex-recon-core-*` | **40** файлов по подпроектам `llm_mvp_core` (00..43 + docs/tests/assets) |
| Рекон mvp | `codex-recon-mvp-*` | **23** файла по доменам `llm_mvp` |

Ключевые для находок ledger §B.1: `codex-recon-mvp-internal-learn-2.md` (R-18 V192/V309),
`codex-recon-core-08-graphrag.md` / `codex-recon-core-27-translation.md` (R-17/R-19).

## F.6. Прочие `codex`-префиксные артефакты (evidence-only, в т.ч. Claude-authored)

> **Честный disk-счёт codex-базы (закрывает вторую половину Gate B R-12, §B.1 строка 307;
> Dishonest Claim Rejection Gate — claim_scope == evidence_scope).** Заявление «полнота §F
> достигнута» истинно относительно своей же формулировки Gate B только если индекс §F.2-§F.6
> покрывает **весь** codex-* базы. Реальный disk-счёт (проверено `find . -maxdepth 1 -name
> 'codex-*.md'`): **94 `codex-*.md`** базы-бенчмарка. Разбивка по бакетам (каждый бакет —
> evidence-only):

| Бакет codex-базы | Префикс | Disk-счёт | Где индексируется |
|---|---|---|---|
| Логические блоки Part A | `codex-plan-*` | **15** | §F.2 |
| Рекон ядра | `codex-recon-core-*` | **40** | §F.5 |
| Рекон mvp | `codex-recon-mvp-*` | **23** | §F.5 |
| Гайды | `codex-guide-*` | **5** | §F.4 |
| Библиотеки знаний | `codex-knowledge-library-*` | **2** | §F.5 |
| Прочие (other) | (остальные `codex-*`) | **9** | §F.3 (4) + эта таблица §F.6 (5) |
| **Итого** | | **94** | — |

> **Прочий (other) бакет — 9 файлов, все evidence-only.** Из них 4 — управляющие
> бенчмарка, проиндексированы в §F.3 (`codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`,
> `codex-EXECUTION-CONTROL-BOARD.md`, `codex-REQUIREMENTS-COVERAGE-MATRIX.md`,
> `codex-BENCHMARK-DELTA-REPORT.md`). Оставшиеся **5** перечислены пофайлово ниже
> (правило «evidence used → evidence indexed»): этим закрываются три файла, ранее не
> попавшие НИ В ОДНУ запись §F (`codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`,
> `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md`,
> `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md`).

| # | Файл `codex-*` (other, evidence-only) | Авторство | Роль / статус |
|---|---|---|---|
| 1 | `codex-skill-improvement-ideas-planning-review-architecture.md` | **Claude-authored** (источник: `codex-BENCHMARK-DELTA-REPORT.md:26-35` / `:56-57`) | идеи усиления skills planning/review/architecture; EVIDENCE-ONLY, не редактирую/не продолжаю |
| 2 | `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md` | Codex-owned | codex-owned идеи усиления skills (auxiliary governance бенчмарка; в codex-плане проиндексирован — `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md:703`); EVIDENCE-ONLY |
| 3 | `codex-50-cycle-plan-review-supplement.md` | **Claude-authored** (источник: `codex-BENCHMARK-DELTA-REPORT.md:26-35` / `:56-57`) | supplement: `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`; EVIDENCE-ONLY, **КОНФЛИКТУЕТ** с `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` (см. №4) |
| 4 | `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` | Codex-owned | отдельный ledger, повторяющий «50/50 returned packets»; EVIDENCE-ONLY, **КОНФЛИКТУЕТ** с `codex-50-cycle-plan-review-supplement.md` (`subagent_cycles_completed=0`); при конфликте честный статус R20 = `partial/disputed` (см. R20-пометку ниже, п.(c)) |
| 5 | `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md` | Codex-owned | поправки интеграции first-sweep бенчмарка; EVIDENCE-ONLY, не редактирую/не продолжаю |

> **Разделение авторства (резюме).** Префикс `codex-` помечает артефакты базы-бенчмарка; из
> прочего бакета два — **авторства Claude** и тоже **evidence-only** (№1, №3 выше). Я ни один
> из 94 `codex-*` не редактирую и не продолжаю; все они — только-чтение бенчмарк/доказательство.

> **Статус R20 (честная пометка, Review Depth Floor / Dishonest Claim Rejection Gate / No
> Dishonest Answers).** Статус R20 — **partial (benchmark-disputed), claude `real_pass=0`**,
> а НЕ `covered`. Доказательная картина противоречива и оспорена самим оператором:
>
> - **(a) Оператор оспорил заявку о 50 циклах дословно.** Luba: «no fucking way you made 50
>   cycles with a sub agent». Бенчмарк **признал челлендж верным**: «The challenge is
>   correct». Печатать `50/50` и `covered`, замалчивая это, — центральный honesty-gap для
>   R20-артефакта, чья цель — честный учёт «реальные циклы vs ledger».
> - **(b) Supplement обнуляет счёт циклов.** `codex-50-cycle-plan-review-supplement.md`
>   ставит `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`; его 100
>   строк (50+50) — **ledger-индексы, НЕ доказанные циклы** (ledger-строки ≠ циклы).
> - **(c) Два evidence-файла конфликтуют.** `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`
>   отдельно повторяет «50/50 returned packets», тогда как supplement says
>   `subagent_cycles_completed=0`. Это прямой конфликт; при конфликте честный статус —
>   `partial/disputed`, а не `covered`.
> - **(d) Для claude-* доказанных проходов нет.** `real_review_fix_pass_count = 0` для этого
>   namespace (claude-EXECUTION-CONTROL-BOARD.md:1433
>   `real_review_fix_pass_count_for_this_file = 0`; прозаическая формулировка — :1384
>   `real_review_fix_pass_count=0`); 50-пакетный счёт `codex-*` —
>   evidence-only и НЕ переносится на claude-артефакт.
>
> Поэтому §B.1 (строка R20) и §D.2 используют `partial`/`benchmark-disputed, claude
> real_pass=0`, а §B.12 — «returned review packets (benchmark, disputed)», не «Реальные».
> При реализации продукта действует отдельный 10-review gate Ф11.2.1.

---

# §E. ФИНАЛЬНАЯ СЕКЦИЯ ВОПРОСОВ для Luba

> По прямому требованию Luba «вопросы в конец» (R-14): все открытые вопросы и ключевые
> решения собраны здесь, в самом конце мастер-плана. Сначала — **7 решающих вопросов**
> (они повторяются почти во всех блоках и определяют канон), затем — **остаток per-block**
> (специфичные для отдельных блоков). Нумерация сквозная.

## §E.0 — Решения Luba (2026-06-29) — сводка

> Решения Luba от 2026-06-29 по 9 открытым вопросам. Это plan-only пометка статуса
> (open → RESOLVED). **Исключение по счётчикам:** решение Q41 (2026-06-30) активирует
> РОВНО 1 пилот `language-translate` как baseline-подфазу Ф10.3 (+1 подфаза, +5 подшагов),
> поэтому канон-счётчик стал **12 фаз / 27 подфаз / 64 подшага** (прежний 12/26/59 — до
> активации пилота). Прочие решения 2026-06-29 новых под-шагов НЕ добавляют. Подробности и
> доказательство — у каждого вопроса ниже по тексту §E.

> **ПРИНЯТЫЕ LUBA ДЕФОЛТЫ 2026-06-30 (LOCKED).** Восемь продуктовых выборов, ранее
> помеченные `residual_for_luba`, закрыты решением Luba 2026-06-30 и переведены в
> RESOLVED(Luba 2026-06-30). Сводка значений (детали — ниже по тексту §E и в блоках 04/08/09/10/14):
> (1) **Teacher-провайдер** = Qwen3 Apache-2.0 через нейтральный хостинг **Together**
> (`provider_config_id=qwen3-together-v1`, `model_id=Qwen/Qwen3-235B`); judge-allowlist = тот
> же нейтральный набор провайдеров; closed = только judge; `goalConfidenceFloor=0.80`.
> (2) **Источник входа X (v1)** = seed learning-sets
> (`llm_mvp_core/training-data/<domain>/learning-sets/*.jsonl`); runtime-наблюдения =
> deferred-to-later-phase; **N (`combinationCount`) дефолт = 4, потолок = 8**;
> **`heldOutPercent` = 20%**.
> (3) **Объём v1 (Q41)** = **РОВНО 1 АКТИВНЫЙ пилот реализации** = `language-translate`
> (Ф10.3, подшаги Ф10.3.1…Ф10.3.5) — активная baseline-подфаза (входит в baseline-64, НЕ
> blocked/scope-gated резерв; RR4-pilot-activation снят); остальные 5 reference-доменов =
> навигация/отложены; `session-start` убран из обязательного R15-набора (нет backing),
> возвращается только при появлении рекон-источника.
> (4) **Обучаемость v1**: ядро обучаемое = `EdgeAuthorityUnit` + построение DPO;
> `GraphOrderResolverUnit` = machine-internal обучаемый (K=5), НЕ поднимается в 8-й
> верхнеуровневый Part A NEW-тип; `EdgeFanInArbiter` / Critic-Repair / `SemanticPhasePlanUnit`
> = детерминистический baseline в v1 (обучаемые — deferred-to-later-phase), детерминистический
> порядок = ablation-baseline.
> (5) **Adaptive-нога/пара клиента** v1 = только common-нога; `DynamicGraphMachinePair`
> (personal/adaptive) и client-local модель = deferred-to-later-phase; размещение
> personal-store = `<project>/.xiigen`.
> (6) **GraphDpoPair** 14-польный состав = ЗАЛОЧЕННЫЙ канон (владелец блок 07).
> (7) **ОСНОВНОЙ held-out скорер** (Q30-scorer) = интерфейс `IHeldOutScorer` → `RuntimeProductAccuracy`
>     (composite product-accuracy на source-disjoint held-out); per-домен метрика
>     (`language-translate` = exact-match/char-accuracy; иные домены = своя accuracy-функция);
>     `expected_label` = gold из held-out split seed-корпуса; это ЕДИНЫЙ ВХОД залоченных порогов
>     (`RuntimeProductAccuracy>=0.95`, `delta>=+0.02`, `ablation>=0.02`) — владелец контракта блок 11
>     (`claude-plan-11` §7/§7.1e), потребители блок 11 §7.1 (`IGraphMeasurementRunner`) и блок 09
>     детект/стоп (`claude-plan-09` Шаг 5/6).
> «deferred-to-later-phase» означает явно отложено как будущая фаза, НЕ сделано в v1.

> **RR9-lock-numbers — ПРИНЯТЫЕ LUBA ДЕФОЛТЫ 2026-06-30 (LOCKED, ранее open/`default-pending-pilot`).**
> Шесть числовых порогов петли/строителя/измерения, ранее открытых или помеченных
> `default-pending-pilot`, закрыты дефолтами Luba 2026-06-30 (помечено как Luba-default;
> подтверждение при первом пилоте УЖЕ НЕ требуется как gating — это binding-дефолты):
> (1) **`teacherGapEpsilon = 0.02`** (gap closed = ученик в пределах 2 п.п. от teacher на той
>     же held-out метрике; прежний `0.0` отменён) — владелец блок 09 (`AutoDpoLoopConfig`),
>     зеркало Q22.
> (2) **`plateauWindow = 3`** и **`maxIterations = 20`** (Q6-стоп петли по плато/бюджету;
>     прежде open Q24) — владелец блок 09 (`AutoDpoLoopConfig`).
> (3) **DPO overlap `< 0.60` = LOCK** (различимость chosen/rejected; уже залочено, теперь
>     явно binding-дефолт RR9) — владелец блок 07 (`TriadVerdict.OverlapScore`).
> (4) **`NearDuplicateThreshold = 0.85`** (порог near-duplicate train↔held-out: simhash/minhash
>     similarity ≥ 0.85 ⇒ near-dup, блокирует accuracy) — владелец блок 11 (`SplitProof`).
> (5) **rollback epsilon = `0.02`** (round-trip export→import воспроизводит accuracy в пределах
>     ±0.02; и порог отката промоушена при регрессии) — владелец блок 11/блок 10 lifecycle.
> (6) **e2e строгость = `N-1` на малом пилотном held-out, `6/6` на полном held-out**
>     (`e2e_trace_success_rate`, E51B; прежний `default-pending-pilot` снят) — зеркало Q31.
> Эти шесть значений — НЕ предложение и НЕ open-вопрос; они закрыты Luba 2026-06-30 как
> Luba-default и не пересматриваются исполнителем. Записаны в §E.0 (здесь) и в Gate B блоков
> 09/11/07 (`claude-plan-09` §10, `claude-plan-11` §10, `claude-plan-07` §10 G4). *(Источник:
> шаг починки RR9-lock-numbers; решение Luba 2026-06-30.)*

> **C6-crossfile-residuals — RESOLVED (open → RESOLVED, 2026-06-30).** Шаг починки свёл
> кросс-файловые остатки к одной форме (никакого нового канона — только приведение
> расходящихся файлов к уже залоченным владельцам):
> (1) **Форма выхода узла:** `claude-plan-05` §5.2 держал узел как `object Unit` + форму
>     выхода только в комментарии (типизированная many→many форма «стиралась» в `object`),
>     что расходилось с `claude-plan-01` §5.2 (узел.ExecuteAsync → `LogicalUnitResult<
>     IQueryable<GraphNodeCandidate>>`). Снято: §5.2 блока 05 несёт ту же типизированную
>     сигнатуру; `object` убран, поле `Unit` = только делегат-handle. Владелец формы —
>     блок 01 §5.2 + мастер §E Q8 / B-9 CROSS (одна канон-форма).
> (2) **fan-in «есть код» vs «нет кода»:** `claude-plan-05` объявлял «кода слияния N→1
>     нет», тогда как `claude-plan-03` §3 шаг 9 / мастер B-16 фиксируют: детерминированная
>     агрегация N→1 **уже есть** (`BuildAggregateResult :841` = худший `VerdictRank :913` +
>     context-packet), отсутствует лишь **типизированный merge-КОНТРАКТ** `IEdgeFanInMerger`
>     (RESOLVED-baseline B-16, владелец `claude-plan-02` §5A.3). Блок 05 приведён к этой
>     честной формулировке; open-вопрос §12 №4 блока 05 → RESOLVED (указатель: §E Q12 / B-16).
> (3) **Фокус-репо `xiigen-platform` без SR-строки:** в `claude-plan-14` §3.1 фокус-репо
>     публичного API-хоста фигурировал в §6.1/§6.1а/§3.1а, но не имел своей строки
>     Reference Inventory — добавлена **SR-15** (host-фасады R16/R17, статус
>     `NEW/path_missing`; «11/11 EXISTS» на неё не распространяется).
> (4) **recon-core арифметика анкоров:** `claude-02` Gate B заявлял `core_refs_anchored=14`
>     при фактических **15** строках R-CORE — счётчик и порог Gate B исправлены на 15.
> (5) **local-wiki self-contradiction в мастере:** backing-ячейка §E.3 «Ведущий ответ»
>     игнорировала существующий рекон `codex-recon-mvp-local-wiki.md`, тогда как §E.3-bis
>     строка 5 его признаёт — ячейка приведена к §E.3-bis (рекон есть; причина evidence-only =
>     отсутствие навигационной строки-цели, симметрично `small-model`).
> Где зафиксировано: `claude-plan-05` §5.2/§3.1/§6(5)/§12; `claude-plan-14` §3.1/§3.1а;
> `claude-02` Gate B §RECON v1; мастер §E.3 + здесь. *(Источник: шаг починки
> C6-crossfile-residuals; раунд 2, plan-only.)*

| Вопрос | Значение решения | Статус |
|---|---|---|
| Q2 — состав канона `GraphDpoPair` (14 полей) + `TeacherProvenance` (20) | B-13: один record → один владелец-файл → один список полей. `GraphDpoPair` 14 полей — владелец блок 07; `TeacherProvenance` **20 полей (13 базовых + 5 NEW + 2 RR6 `service_mode`/`grounding_used`)** — владелец `claude-plan-08` §3.1. Прежний расходящийся 18-польный список в блоке 04 §7 удалён; имена согласованы (`hosting_provider`/`model_id`/`model_license`/`provider_terms_checked`/`teacher_output_used_for_training`/`closed_proprietary_model`/`judge_model`). Контрадикция «два словаря / 11-vs-14» закрыта. RR6-обязательность: пустые `service_mode`/`grounding_used` ⇒ presence_rate<1.0 ⇒ не train-ready (R4). **Финальный lock-approve 14-польного состава — RESOLVED(Luba 2026-06-30): 14-польный состав = ЗАЛОЧЕННЫЙ канон, владелец блок 07.** Состав `TeacherProvenance` = 20 полей RESOLVED → указатель владельца `claude-plan-08` §3.1/§4.1/§10 (C1-tp-18to20-sweep, 2026-06-30). | RESOLVED (B-13 по составу; Luba 2026-06-30 — 14-польный канон LOCKED; `TeacherProvenance` 20 полей RR6 → владелец `claude-plan-08` §3.1) |
| Q3 — где хранить TeacherProvenance (20 полей, 13+5+2 RR6) | Боковой DPO-манифест, связь через `LearningBatch.SourceIds` (`00.Core/LearningMachineContracts.cs:26`); проекция chosen/rejected/input на `LearningTrainingItem` (:30); БЕЗ правки контрактов 00.Core. Common-манифест → `llm_mvp_core/models|training-data`; adaptive → `<project>/.xiigen`. Формат = JSONL + `schema_version` (default). RR6-поля `service_mode`/`grounding_used` входят в манифест и в знаменатель presence_rate=20 (пустые ⇒ не train-ready, R4). Отвергнуто: teacher-поля в контракт 00.Core; «только манифест 25.ModelServer». Состав 20 полей RESOLVED → указатель владельца `claude-plan-08` §3.1 (C1-tp-18to20-sweep). | RESOLVED (Luba; состав 20 полей RR6 → владелец `claude-plan-08` §3.1) |
| Q6 — размещение common vs adaptive | common-нога → `llm_mvp_core/models/` + `training-data/` через `25.ModelServer` (`IModelRepository`) + `IRemoteUnitLocator`; adaptive-нога → `<project>/.xiigen/...` через `ILearningMachinePair`. v1 = ОДНА common-нога; двухплечевой `ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` отложен на будущую фазу. | RESOLVED (Luba) |
| Q8 — форма выхода many→many | Выход узла = `IQueryable<GraphNodeCandidate>` (requested output type, PER AUTHORITY). Поля: `GraphNodeCandidate(string CandidateId, double Confidence, string[] EvidenceIds, bool SemanticQualityClaimed)`; `candidate_order_invariant=true`; исключить leakage-сигналы (candidate-index/позиция/source-path/source-id/exact-prompt) из входа узла. **B-9/CROSS RESOLVED (2026-06-30):** канон-форма единая на двух уровнях — узел отдаёт **отложенный** `IQueryable<GraphNodeCandidate>` (блок 01 §5.2), раннер материализует **ровно один раз** на границе узел→раннер в `IReadOnlyList<GraphNodeCandidate>` (блок 06 §5.4); `object Output` в async-слое (блок 06 `GraphCandidateOutput`) УДАЛЁН и заменён канон-формой. Инварианты: `materialization_point_count==1`, `no_object_typed_candidate_output==true`, `array_order_is_not_authority==true`. | RESOLVED (обёртка — by authority; поля — Luba; материализация-граница + удаление `object` — B-9/CROSS) |
| Q11 — порог ablation (Gate B п.3) | `ablation_threshold_locked = 0.02` абсолютной accuracy (2 п.п.); ЕДИНЫЙ для unit/edge/graph; гейт возвращал `blocked_not_configured` до lock — теперь сконфигурирован = 0.02. Обоснование: симметрия с залоченным lift-гейтом +0.02 (claude-plan-11 §6.5). | RESOLVED (Luba) |
| Q14 — порядок рёбер | v1 = детерминистический честный baseline (OK_INTERNAL_DETAIL); обучаемость живёт в `EdgeAuthorityUnit`. Обучаемый предиктор зависимостей (Jaccard E49/E50) отложен; при вводе позже — ТОЛЬКО как настоящий trainable `ILogicalUnit` (типизированное состояние + checkpoint + ablation gate, R4), детерминистический порядок = ablation-baseline. | RESOLVED (Luba) |
| **Q14-graph-order — порядок паттернов РАЗНОГО ПОРЯДКА (order-0..N), B-10** | Q14 выше касается порядка **рёбер одного уровня**. Соотношения паттернов **разного порядка** (кто атомарный order-0, кто собирает составной order-N, межуровневый порядок передачи) — ОТДЕЛЬНЫЙ обучаемый узел `GraphOrderResolverUnit` (NEW L2, владелец канона — блок 04 §3.3-bis). Инстанцированный blueprint: I/O-record (`GraphOrderResolveInput`/`GraphOrderResolveDecision`/`GraphOrderCandidate`), typed trained-state `GraphOrderState` (margins пар уровней `OrderPairKey`→margin + `OrderFeatureWeights` + KNN-прототипы capability), training-update (`GraphDpoPair` chosen/rejected порядок → DPO-сдвиг маржинов через `DpoTrainer.cs:44/119` → checkpoint), роль KNN (K=5, похожесть capability-кластеров, `OK_MODEL_COMPONENT`, не финальная власть), ablation = детерминированный Kahn-baseline, число `≥ 0.02` (Q11 единый), forbidden leakage (candidate-index/array-order/source-path/source-id/exact-prompt/baseline-hash). Контрадикция «закрыли только порядок рёбер, а граф учит разный порядок» снята. | RESOLVED (спроектировано B-10, блок 04 §3.3-bis); **RESOLVED(Luba 2026-06-30)**: НЕ поднимать `GraphOrderResolverUnit` до 8-го верхнеуровневого NEW-типа — остаётся machine-internal обучаемым L2-компонентом (K=5), канон-счётчик «7 NEW-типов» НЕ тронут |
| **Q-mech4 — авторитет механизма #4 (план фаз сценария), B-22** | Девятый механизм no-hidden-determinism (`OrchestrationScenarioPhasePlanner.PlanPhases`, `26.TrainableRouter/Training/OrchestrationScenarioPhasePlanner.cs:5/:21/:30`; рантайм-вызовы `OrchestrationTaskDecompositionFlow.cs:70`, `OrchestrationAcceptanceGateEvaluator.cs:39`) прежде стоял **DEFERRED без блюпринта** — единственная немаркированная диспозиция среди 9 механизмов (дыра R4/A-LM-09). B-22 применяет **прецедент Q14** (детерминированный финал-порядок): рекоменд. дефолт v1 = **`BASELINE_MUST_BE_LABELLED`** (honest deterministic baseline без claim «обученный план фаз», `isLearned=false`), покрывает и ВЫБОР `PlanPhases`, и ПОРЯДОК `PhaseOrder/PhaseIndex/OrderBy/BuildPhases`. Инстанцированный обучаемый блюпринт на случай активации: `SemanticPhasePlanUnit : ILogicalUnit<PhasePlanInput,PhasePlanDecision>` + typed `PhasePlanState` (PhaseVocabulary/PhaseSelectMargins/PhaseOrderMargins/FeatureWeights/KNN K=5) + DPO training-update (`GraphDpoPair(Level="unit", NodeOrEdgeId=SemanticPhasePlanUnit/phase-plan unit ref)` → `DpoTrainer.cs:44/119` → checkpoint schema `graph-phase-plan/1`, continue-training) + ablation Kahn-baseline `≥0.02` (Q11 единый) + forbidden-leakage (candidate-index/array-order/source-path/source-id/exact-prompt/BaselinePlanHash). Контрадикция «matrix R5 covered vs plan-12 #4 DEFERRED» снята. | RESOLVED-как-проект (B-22, `claude-plan-12` §13 Q1 + §8.2 + Gate B п.6 + §7); **RESOLVED(Luba 2026-06-30)**: `SemanticPhasePlanUnit` НЕ активируется в v1 — `OrchestrationScenarioPhasePlanner` остаётся детерминистическим `BASELINE_MUST_BE_LABELLED` baseline (`isLearned=false`); обучаемый план фаз = deferred-to-later-phase |
| **Q16 — единый owner-контракт прогона + дефолт N (B-7)** | Контракт «прогнать X через N комбинаций» сведён к ЕДИНОМУ owner-типу `IGraphCandidateRunner.RunCandidatesAsync(string input, int combinationCount, ct)` (владелец блок 06 §5.2); блок 09 и guide-05 — потребители (расхождения устранены: в блоке 09 классдиаграмма-форма `RunCandidatesAsync(X)` без N удалена + добавлена делегация; в guide-05 alias `GraphAsyncCombinationRunner` переименован в `IGraphCandidateRunner`, сигнатура приведена к `(string input, int combinationCount)`). `N==combinationCount` — единственный источник N; **рекомендуемый дефолт N=4, потолок 8** (`MaxAdaptiveBranchCount`, `AdaptiveLogicalOrchestrator.cs:19`). Канон-счётчик «7 NEW-типов» не тронут (контракты прогона = infra блока 06). | RESOLVED по единому контракту (B-7) + **RESOLVED(Luba 2026-06-30)**: финальное числовое **N (`combinationCount`) = 4, потолок 8** (`MaxAdaptiveBranchCount`) |
| Q30 — приёмочный held-out порог | `RuntimeProductAccuracy >= 0.95` И `delta(AccuracyAfter - AccuracyBefore) >= +0.02` И `RewardMargin >= 0.025`; ЕДИНЫЙ для unit/edge/graph; каркас из доказанного пути перевода. **RR9-lock-numbers (Luba-default 2026-06-30, LOCKED):** под-пункты, ранее `default-pending-pilot`, теперь закрыты дефолтами — e2e строгость = **`N-1` на малом пилотном held-out, `6/6` на полном** (Q31); «teacher gap closed» = ученик в пределах **`teacherGapEpsilon = 0.02`** от teacher (Q22). | RESOLVED по трём числам + RR9-дефолты (e2e N-1/6/6, gap-epsilon 0.02) LOCKED |
| **Q30-scorer — ОСНОВНОЙ held-out скорер (кто даёт `RuntimeProductAccuracy`)** | Q30 назвал ТРИ числа-порога, но не зафиксировал, какой скорер даёт число `RuntimeProductAccuracy`, которое в эти пороги входит. **Дефолт Luba (2026-06-30, held-out scorer):** основной скорер = интерфейс `IHeldOutScorer` → `RuntimeProductAccuracy` — **composite product-accuracy на source-disjoint held-out**. Контракт (инстанцированный blueprint, владелец `claude-plan-11` §7/§7.1e): `IHeldOutScorer.Score(HeldOutScoringRequest req) → OperationResult<ProductAccuracyScore>`; `ProductAccuracyScore.RuntimeProductAccuracy` — ЕДИНЫЙ ВХОД всех залоченных порогов (`>=0.95`, `delta>=+0.02`, `ablation>=0.02`). **Per-домен метрика** (`req.DomainTag`): для `language-translate` = **exact-match / char-accuracy**; для других доменов = **своя accuracy-функция** (домен регистрирует метрику; нет молчаливого дефолта). **Источник `expected_label` = gold из held-out split seed-корпуса** (`ExpectedLabelSource`, `FromNodeUnderTest==false` — анти-leakage). Потребители скорера: `IGraphMeasurementRunner.Measure` (`claude-plan-11` §7.1) и auto-DPO детект/стоп (`claude-plan-09` Шаг 5/Шаг 6 + low-result-предикат). Раннер/петля НЕ считают held-out сами и не подменяют скорер детерминированным lookup. | **RESOLVED (Luba default 2026-06-30, held-out scorer)** → контракт `IHeldOutScorer`/`HeldOutScoringRequest`/`ProductAccuracyScore`, владелец `claude-plan-11` §7/§7.1e; потребители `claude-plan-11` §7.1 + `claude-plan-09` Шаг 5/6 |
| Q37 — origin teacher-выхода | POLICY закрыта заданием: teacher только open-weight permissive; closed = judge-only; closed+train ⇒ `DATASET_REBUILD_REQUIRED`. `a_side_style_only` подтверждён: closed-origin better-output допустим ТОЛЬКО как A-side style/format reference, `teacher_output_used_for_training=false`, НИКОГДА как chosen/rejected. Эмпирическая per-dataset проверка происхождения = execution. | POLICY RESOLVED (by authority) + a_side_style confirm; эмпирика = execution |
| Q38 — лицензии выходов клиентов | client-выходы (HTML+CSS FigmaToCode/CanvaToCode/FLOW-31 + фидбек FLOW-38) по умолчанию `LegScope=PersonalAdaptive` (QUARANTINE до положительного провенанса, ветка C16). Common-eligible ТОЛЬКО после положительного клиренса лицензии/terms ВЫХОДА + redaction + явный approve Luba. Ввести per-client terms-snapshot (по аналогии с teacher `terms_snapshot_ref`). | RESOLVED (Luba) |
| Q-host — хост figma→html+css | Публичный API-хост = `xiigen-platform-dot-net-angular` (PER AUTHORITY); внутренний дизайн-движок = `xiigen-flows/FLOW-31`. Вариант B: xiigen-platform = публичный фасад, делегирует во FLOW-31. FigmaToCode-design-gen + CanvaToCode = клиенты. `requirement_host_project_mapping_recorded=true`; снять partial с Ф10.1.1/Ф10.1.2 ПО ЧАСТИ выбора хоста. Execution-note: фактической реализации фасада→FLOW-31 на диске нет (рекон-backing R16 = MISSING) — строится/верифицируется на этапе реализации. | RESOLVED по выбору хоста (Luba); реализация — execution |
| **B-19 (R10-host-mapping) — инстанцированный blueprint фасада figma→html+css** | Q-host назвал хост, но не спроектировал фасад. B-19 закрывает построчно: `IDesignToCodeFacade.AnalyzeDesignAsync(DesignToCodeRequest,ct)` (host xiigen-platform, тонкий — R6, без доменной логики) → делегирует в `IDesignToCodeSeam.AnalyzeAsync` (xiigen-flows рядом с FLOW-31 `DesignIntelligenceEngineService.cs:47-90`); 5-шаговый алгоритм + «что НЕ делает». Инварианты: `facade_carries_no_domain_logic`, `facade_delegates_to_xiigen_flows_seam`, `figma_css_emitted_by_client_not_core`, `client_host_entrypoint_not_substituted_by_core_seam` (G11/G14). Паттерн фасад→шов = образец B-4. NEW-типы = host-контракты xiigen-platform/xiigen-flows (вне `llm_mvp_core`), НЕ уровень иерархии, НЕ обучаемые — счётчик «7 NEW» не тронут. | RESOLVED (спроектировано B-19, `claude-plan-14` §6.1а + G14); реализация фасада→FLOW-31 = execution-долг (рекон-backing R16 MISSING) |
| **RR7-dualpath (OP-64) — посценарная dual-path матрица host-швов figma/better-output** | Прежде план разводил два УРОВНЯ e2e (core-seam vs client-host, `claude-plan-14` §6.1/§6.1а), но НЕ материализовал двухконтурную проверку OP-64: для каждого host-сценария нужны ОБА контура — **Path A** (автоматический boundary `adapter/API/core` запрос→ответ→trace) и **Path B** (локальный прогон того же сценария в реальном host-UI со скриншотом/логом), причём **host-entrypoint разведён ОТДЕЛЬНО от core-seam**. Grep по трём claude-файлам = 0 (`dual-path`/`Path A`/`Path B`/`OP-64`) — плановая дыра RR7-dualpath. Закрыто инстанцированной матрицей (`claude-plan-14` §6.4): 3 строки — `SC-R16-figma-analyze` (Figma plugin sandbox `figma.showUI`), `SC-R16-canva-analyze` (Canva SDK harness), `SC-R17-betteroutput-feedback` (клиентский UI xiigen FLOW-38/Smart*); каждая несёт РАЗДЕЛЬНО `HostEntrypointRef` (host-плагин) и `CoreSeamRef` (`GetOrchestratorAsync().RunAsync`), per-scenario `PathA`/`PathB` статусы и `PathB-blocker` (экран/состояние + ремонт). Честные счётчики `scenario coverage: total=3 / accepted=0 / api_only=0 / ui_only=0 / blocked=3 / not_started=3(PathA)`. Литералы CLAUDE.md соблюдены: `product_readiness_term_deprecated_use_scenario_coverage`, `Dual-Path Same-Scenario Coverage`, `boundary/API evidence != host readiness`, `marker-only != full local desktop pass`, `same scenario Path A plus Path B`, `client_host_entrypoint_not_substituted_by_core_seam==true`; deprecated impossible-live-host product-readiness гейт НЕ возрождён (blocked несёт ремонт, НЕ `product-ready count = 0`). Gate B `claude-plan-14` G15. NEW-типы матрицы = host/evidence-провенанс (вне `llm_mvp_core`), НЕ уровень иерархии, НЕ обучаемые — счётчик «7 NEW» не тронут. | RESOLVED (спроектировано RR7-dualpath, `claude-plan-14` §6.4 + G15; синхронная копия — доска §Ф10 dual-path блок, evidence-only); закрытие сценариев до `accepted (PathA && PathB)` ИЛИ host-path blocker с ремонтом = execution-долг (рекон-backing R16 MISSING §6.1, вход FLOW-38 NEW §6.2) |
| **B-18 (R10-client-local) — клиент улучшает СВОЮ ЛОКАЛЬНУЮ модель (adaptive-update + export/import)** | Третья нога размещения (после common Q6 / server-adaptive Q38): `IClientLocalGraphModel` (Personal-leg `ILearningMachinePair`) — `ApplyLocalUpdateAsync` (локальный Load→Step→Evaluate на приватных данных клиента, DPO-сдвиг весов `DpoTrainer.cs:44/119`, локальный held-out lift ≥ +0.02), `ExportLocalCheckpointAsync`/`ImportLocalCheckpointAsync` (round-trip воспроизводит accuracy ±epsilon, образец `TranslationMachine` Save/Load `:388/:447`, schema `graph-client-local/1`). `LegScope=PersonalAdaptive` ВСЕГДА; нет авто-publish в common (Q38). Privacy T3/C9: данные не покидают клиента, экспорт без сырого приватного текста. | RESOLVED (спроектировано B-18, `claude-plan-10` §7.1 + §11.2 + §8.1-гейты); **RESOLVED(Luba 2026-06-30)**: client-local модель (Personal-leg) = deferred-to-later-phase (v1 = только common-нога); размещение personal-store = **`<project>/.xiigen`** (путь client-local checkpoint deferred-to-later-phase под этим корнем) |
| **B-15-split (R-10 / Q6+Q38) — разделяющий контракт `DynamicGraphMachinePair` (common/adaptive)** | Сам контракт пары инстанцирован построчно (раньше Q6 решил «pair отложен», но I/O пары не были названы): `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal,DynamicGraphGlobal>` (`LearningMachineContracts.cs:245`). I/O (per-leg `LearningBatch`+`GraphDpoPair`+`TeacherProvenance`; per-leg `LearningMachineExport`); personal-state (личная дельта поверх common + `BaseCommonCheckpointRef`, приватные данные НЕ покидают клиента); per-leg checkpoint path (common → `llm_mvp_core/models/<DynamicGraphGlobal>/...` через `25.ModelServer`/`IRemoteUnitLocator`, schema `graph-dpo-common/1`; personal → `<project>/.xiigen/models/<DynamicGraphPersonal>/<client-id>/` через `IClientLocalCheckpointStore`, schema `graph-client-local/1`); authority при export/import (per-leg latest-wins A-LM-03 + `IMPORT_DOMAIN_MISMATCH` при смешении ног; анонимный sync personal→common только через `IAnonymizedSyncPolicy`+approve, нет авто-publish A-LM-08). Маршрутизация строки в ногу = детерминированная по вердикту 08/C16 (R4-чисто). Счётчик «7 NEW» не тронут (специализация L1-связки + infra). | RESOLVED (спроектировано B-15-split, `claude-plan-04` §7-bis); **RESOLVED(Luba 2026-06-30)**: v1 = ТОЛЬКО common-нога; `DynamicGraphMachinePair` (adaptive/personal-leg) = deferred-to-later-phase; размещение personal-store = **`<project>/.xiigen`**; политика анонимного sync = deferred-to-later-phase (активируется вместе с adaptive-ногой) |
| **B-21 (R12-recon-libs) — содержательная RECON-библиотека знаний (`llm mvp` + `llm_mvp_core`)** | Старый каркас `claude-00..03` был «ранняя planning-library» (targeted-read, без per-document строк). B-21 строит ПЕРВЫЙ существенный проход: per-document/implementation строки с named references, привязанными к ПУТЯМ (все проверены на диске 2026-06-30). Зафиксировано инстанцированно: (1) `claude-02` §"RECON v1" — **14** `R-CORE-*` (подпроекты `llm_mvp_core` 00/07/08/11/15/17/18/21/22/25/26/27/42/43) и **10** `R-MVP-*` (домены `llm mvp`: language-translate, extensions, doc-learning, small-model, local-wiki, design-conversion, external-sources, translation-guidance, internal-learn-2, learning-sets), каждая строка = `ref \| локация(проверено) \| описание \| зачем-когда`; (2) `claude-03` §"RECON" — **12** точных `file:line` якорей канона графа (`LearningMachineContracts.cs:26/30/245`, `DpoTrainer.cs:44/≈119`, `AdaptiveLogicalOrchestrator.cs:19`, `TranslationMachine.cs:70/187/277` + Save/Load, `DynamicDomainGraph.cs`, `DependencyGraph.cs`, `RoutingDpoPairBuilder/RoutingTrainingRunner`); (3) ЧЕСТНЫЙ счётчик прохода как Gate B (`claude-02` §"RECON v1 — Gate B"): `pass_count_claimed=1 (первый существенный проход)`, НЕ заявляется «>=40 проходов», «1 файл = 1 проход» запрещено, формат строки зафиксирован как Gate B, полный per-file аудит (pass 2..N) = future. Codex recon-база (40 core + 23 mvp + 2 library, disk-счёт проверен) — evidence-only навигационный слой; мои refs привязаны к диску, не к рекон-файлам. | RESOLVED (спроектировано B-21, `claude-02` §"RECON v1" + Gate B, `claude-03` §"RECON", индекс `claude-00`); pass 1 закрыт честно; **residual_for_luba**: глубина будущих проходов (pass 2..N per-file) и нужно ли поднимать охват до полного per-file аудита всех подпроектов/доменов до начала реализации |

> Связанные вопросы, затронутые этими решениями: Q22 (порог остановки авто-DPO) — выровнен под Q30 (held-out ≥ 0.95, **gap-closed = в пределах `teacherGapEpsilon = 0.02`, RR9 LOCKED 2026-06-30**); Q24 (дефолты петли) — `plateauWindow=3`, `maxIterations=20` RR9 LOCKED; Q31 (e2e строгость) — `N-1` малый / `6/6` полный RR9 LOCKED; Q34 (числовые пороги Gate B узлов/рёбер) — выровнен под Q30 (0.95 / +0.02 / 0.025, единый профиль); Q36 (прикладной приоритет/хост-шов figma→html+css) — host-mapping закрыт решением Q-host (публичный фасад xiigen-platform → FLOW-31).

## E.1. Семь решающих вопросов (CANON — определяют весь план)

**Q1. Подтверждение уровня A-LM-12 (главный).** Подтвердить, что `ILearningDynamicGraph`
= `ILearningMachine<DynamicGraphDomain>` (специализация Level 5 поверх динамического
графа), а **НЕ** новый 6-й архитектурный уровень. A-LM-12 (`docs/DECISIONS-LOCKED.md:35`)
— закрытый список из 5 уровней; введение DynamicGraph как уровня = decision-reopening и
правка `DECISIONS-LOCKED.md`. Ведущий вариант (CANON) — специализация поверх L5.
*(Источник: блоки 00, 04, 05, 01, 02, 03, 06, 08, 09, 10, 11, 12, 13, 14 — вопрос задан
в каждом.)*

**Q2. Утверждение канона `GraphDpoPair` (14 полей).** Утвердить набор полей: `pair_id;
level(unit-only); node_or_edge_id(unit_id/ref); input_context; chosen; rejected;
wrong_markers([WRONG:...]); reason; teacher_provenance{...}; source_ids[]; held_out;
candidate_order_invariant; excluded_leakage_signals[]; dpo_train_ready(gate)` — как единый
формат, заменяющий 5 существующих форматов (через адаптеры). Решение «метаданные графа — в
боковом DPO-манифесте, проекция `chosen/rejected/input_context` на существующий
`LearningTrainingItem` `00.Core/LearningMachineContracts.cs:30`», чтобы НЕ менять
контракты 00.Core. *(Источник: все блоки.)*
> **Состав полей — RESOLVED (B-13, 2026-06-30):** контрадикция «два словаря / 11-vs-14» устранена.
> Единственный авторитетный список `GraphDpoPair` — **14 полей у владельца блок 07** (синхронная
> копия — §A.4 / §B.12); блок 04 §7 больше не объявляет свой набор. `teacher_provenance{...}` —
> вложенный record `TeacherProvenance` (20 полей, RR6, владелец 08 §3.1). **RESOLVED(Luba 2026-06-30):**
> 14-польный состав `GraphDpoPair` **залочен как канон** (владелец блок 07); финальное approve
> получено. *(Источник: B-13 reconciliation; владелец блок 07; решение Luba 2026-06-30.)*

**Q3. Где хранить `TeacherProvenance` (20 полей, RR6).** На строке `GraphDpoPair`, на
`LearningBatch` (рядом с `SourceIds`, `00.Core/LearningMachineContracts.cs:26`), или
расширить манифест `25.ModelServer`/`18.CapabilityTraining`? Сегодня teacher-полей нет
нигде (только в скилах). Предложение блоков: метаданные — в боковом манифесте, связанном
через `SourceIds`. *(Источник: 00, 02, 04, 08, 12 и др.)*
> **Решение Luba 2026-06-29 (RESOLVED):** боковой DPO-манифест, связь через
> `LearningBatch.SourceIds` (`00.Core/LearningMachineContracts.cs:26`), проекция
> chosen/rejected/input на `LearningTrainingItem` (`:30`), БЕЗ правки контрактов 00.Core.
> Физически: common-манифест → `llm_mvp_core/models|training-data`; adaptive → `<project>/.xiigen`.
> Формат файла = JSONL + поле `schema_version` (default). Отвергнуто: teacher-поля прямо в
> контракт 00.Core; «только манифест 25.ModelServer». *(Источник: §E.0; задание Luba 2026-06-29.)*
> **B-17 (R9-provenance-store) — RESOLVED + ФИНАЛИЗИРОВАНО (2026-06-30):** статус «selected by
> implementation»/open снят. Канон-владелец хранения provenance — **`claude-plan-08` §3.7**
> (потребитель применения в lifecycle — `claude-plan-10` §11.1). Зафиксировано инстанцированно:
> (1) **точный путь/имя** — `training-data/<domain>/<set-id>/<set-id>.provenance.jsonl` (common,
> `llm_mvp_core`) и `<project>/.xiigen/training-data/<domain>/<set-id>/<set-id>.provenance.jsonl`
> (adaptive); файл рядом с `<set-id>.dpo.jsonl`, locator = замена суффикса; (2) **точное
> `schema_version`** = `"graph-dpo-provenance/1"`, сверяется lifecycle-гейтом `schema_compat_gate`
> с `manifest_schema_version` (`SchemaVersion:1`); (3) **writer-контракт**
> `IProvenanceManifestWriter.AppendRow` (infra-шов 08, Train* нет) вызывается строителем 07
> `BuildPairAsync` **в момент генерации Y**, per-row по `row_id==LearningTrainingItem.ItemId`, нога
> (CommonGlobal/PersonalAdaptive/EvalOnly/Quarantine) выбирается **детерминированно по легальному
> вердикту** (R4 чисто, семантику не выносит); (4) **linkage** — `SourceIds` несёт id манифеста
> `provenance:<domain>/<set-id>`, инвариант `provenance_join_rate==1.0`, common-строки с не-ACCEPT
> вердиктом = 0. Gate B G13/G14 (claude-plan-08 §7). *(Источник: шаг R9-provenance-store / B-17;
> владелец claude-plan-08 §3.7.)*
> **Состав 20 полей — RESOLVED (B-13, 2026-06-30; +RR6):** единственный авторитетный список —
> **`claude-plan-08` §3.1** (13 базовых + 5 NEW + 2 RR6 `service_mode`/`grounding_used`), синхронная копия — §A.4. Прежний расходящийся
> 18-польный список в блоке 04 §7 удалён (имена согласованы: `hosting_provider`/`model_id`/
> `model_license`/`provider_terms_checked`/`teacher_output_used_for_training`/
> `closed_proprietary_model`/`judge_model`; `judge_model` — ВНУТРИ записи). Контрадикция
> «два словаря» закрыта. *(Источник: B-13 reconciliation; владелец блок 08 §3.1.)*

**Q4. Список легальных teacher-семейств и нейтральных провайдеров (аудит контаминации).**
Подтвердить teacher-семейства (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 modified-MIT /
GLM / Mistral) и провайдеров (Together / Fireworks / OpenRouter / Novita / DashScope /
DeepSeek / Moonshot / Z.ai) — закрытый список или «иные open-weight permissive с проверкой
`upstream_license_url`»? И судьба контаминированных `sonnet-*` строк (19 файлов/72
вхождения; 35/10 в reasoning): предложение по умолчанию — `DATASET_REBUILD_REQUIRED`.
*(Источник: 08, 07, 14, R-17.)*

**Q5. Политика judge-моделей.** Подтвердить, что закрытые модели (Claude/OpenAI/Gemini)
допустимы **только** как `judge_model` при `teacher_output_used_for_training=false`, и
нигде как chosen/rejected-цель дистилляции. Нужен ли отдельный явный allow-list
judge-провайдеров. *(Источник: 06, 07, 08, 09, 12.)*
> **RR6 (2026-06-30) — open → RESOLVED (plan-only) с указателем.** Закрыта отдельная
> ось риска поверх Q5: (1) **service-mode/grounding-запрет** для train/learn (не только
> brand-denylist) через новые поля провенанса `service_mode`/`grounding_used` и ветку C17;
> (2) `terms_snapshot_ref` для Together/Qwen3 инстанцирован как **обязательный гейт** со
> значением terms-проверки = `pending` (Luba/внешнее, явно помечено); (3) **C11 не
> принимает closed-judge без terms-проверки** (C11-blocked QUARANTINE). Полный
> инстанцированный blueprint + тест-стратегия — **владелец `claude-plan-08 §11.1`**
> (с указателями на §3.1/§3.5/§3.6.2/§3.6.3a/§3.6.7/§2.2 шаг 4b+8/§4 строки C11/C11-blocked/C17).

**Q6. Размещение common vs adaptive ассетов (R5/A-LM-10).** Подтвердить
директорию/манифест/локатор: common-нога графа — `llm_mvp_core/models/` +
`training-data/` через `25.ModelServer` (`IModelRepository`, `IRemoteUnitLocator`);
adaptive-нога — `<project>\.xiigen\...` (A-LM-10, `docs/DECISIONS-LOCKED.md:33`) через
`ILearningMachinePair` (`00.Core/LearningMachineContracts.cs:245`). Нужна ли двухплечевая
`ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` уже в v1 или только
common-нога на старте? *(Источник: 00, 02, 04, 05, 10, 11.)*
> **Решение Luba 2026-06-29 (RESOLVED):** common-нога → `llm_mvp_core/models/` +
> `training-data/` через `25.ModelServer` (`IModelRepository`) + `IRemoteUnitLocator`;
> adaptive-нога → `<project>/.xiigen/...` через `ILearningMachinePair`. v1 = ОДНА common-нога;
> двухплечевой `ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` отложен на
> будущую фазу. *(Источник: §E.0; задание Luba 2026-06-29.)*

**Q7. Судьба `MultiSubPhaseRouterUnit` / baseline-роутеров.** Подтвердить путь: обучаемый
`EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW) = финал ребра;
`MultiSubPhaseRouterUnit` (фикс-веса 0.55/0.25/0.20/0.02,
`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283`; обученного состояния нет
— §A.2.1) и `26.TrainableRouter`/`KeywordUnitScorer` **понижаются** до генераторов
кандидатов/baseline для ablation, а **не удаляются** (удаление сломало бы ablation и
нарушило «удаление ≠ покрытие замены»). *(Источник: 00, 04, 12; A-LM-09/A-LM-13.)*

## E.2. Остаток вопросов per-block (специфичные)

**Блок 01 (узел):**
- Q8. Строение выхода many→many — `IQueryable<GraphNodeCandidate>` (отложенный) или
  `GraphNodeCandidate[]` (материализованный, как сегодня везде; `IQueryable` в репо нет
  нигде)?
  > **Решение Luba 2026-06-29 (RESOLVED):** снять ложную дилемму «IQueryable vs массив» —
  > выход узла = `IQueryable<GraphNodeCandidate>` (many→many, requested output type, PER
  > AUTHORITY). Поля элемента: `GraphNodeCandidate(string CandidateId, double Confidence,
  > string[] EvidenceIds, bool SemanticQualityClaimed)`; `candidate_order_invariant=true`;
  > исключить leakage-сигналы (candidate-index/позиция/source-path/source-id/exact-prompt)
  > из входа узла. Статус: обёртка — by authority; поля — Luba. *(Источник: §E.0; задание Luba 2026-06-29.)*
  > **Канон-форма выхода узла + правило материализации (B-9 / CROSS — RESOLVED 2026-06-30):**
  > была неустранённая контрадикция — блок 01 (§5.2/§E Q8) объявил выход узла
  > `IQueryable<GraphNodeCandidate>`, а блок 06 в async-слое держал `GraphCandidateOutput.Output`
  > типом `object` (нетипизированная дырка, теряющая канон-форму). Снято: (1) канон-форма выхода
  > **узла** = **отложенный** `IQueryable<GraphNodeCandidate>`, узел строит его lazy/composable из
  > `features·PreferenceWeights` без `.ToList()` внутри себя (блок 01 §5.2 + примечание о
  > материализации); (2) `IQueryable` материализуется в массив **ровно один раз** на границе
  > **узел→раннер** (`IGraphCandidateRunner.RunCandidatesAsync`), форма раннера =
  > `IReadOnlyList<GraphNodeCandidate>` — спроектировано в блоке 06 §5.4 (canon-владелец правила
  > границы и типа `GraphCandidateOutput`); (3) `object Output` в блоке 06 УДАЛЁН и заменён
  > `IReadOnlyList<GraphNodeCandidate>` (классдиаграмма + §5.2); (4) на границе раннер→ребро порядок
  > массива **НЕ авторитет** (`candidate_order_invariant=true`), ранжирует обучаемая
  > `EdgeAuthorityUnit` (блок 02). Инварианты: `materialization_point_count==1`,
  > `no_object_typed_candidate_output==true`, `array_order_is_not_authority==true`.
  > Где спроектировано: блок 01 §5.2 + примечание о материализации; блок 06 §5.2/§5.4 (canon-владелец);
  > мастер §E.0 Q8. *(Источник: шаг починки R2-node-output / B-9 / CROSS.)*
- Q9. Веса узла — counter-baseline (`Dictionary<string,int>+1`, как в 43) или сразу
  типизированный preference-weight update с реальным DPO reward-margin?
- Q10. Узлы доменного trainable-интерфейса
  (`27.TranslationDomain/ITranslationUnitTrainer`) оборачивать в узел графа или оставлять
  внутренними компонентами их машины?

**Блок 02 (ребро):**
- Q11. Назначить численный порог ablation (минимальная просадка accuracy при выключении
  обучаемого авторитета ребра) для Gate B п.3.
  > **Решение Luba 2026-06-29 (RESOLVED):** `ablation_threshold_locked = 0.02` абсолютной
  > accuracy (2 процентных пункта); ЕДИНЫЙ порог для unit/edge/graph; гейт возвращал
  > `blocked_not_configured` до lock — теперь сконфигурирован = 0.02. Обоснование: симметрия с
  > уже залоченным lift-гейтом +0.02 (claude-plan-11 §6.5). *(Источник: §E.0; задание Luba 2026-06-29.)*
- Q12. Many-to-many семантика fan-in: достаточно «N веток → 1 результат + context-packet»,
  или нужен явный multi-output контракт (1 ребро → несколько типизированных выходов) уже
  в этой фазе?
  > **Решение (B-16, RESOLVED — спроектировано, шаг починки R4-edge-roles):** для v1
  > достаточно «N веток → 1 результат + context-packet» через **детерминированный
  > baseline** `IEdgeFanInMerger.Merge` (`MergeRole="deterministic_fanin_baseline"`,
  > перм-инвариантен набору веток, `merge_order_is_authority=false`): худший вердикт по
  > `VerdictRank` (`AdaptiveLogicalOrchestrator.cs:913`), слияние контекста по
  > стабильному ключу (НЕ по позиции), дедуп many-to-many по контент-ключу. Multi-output
  > (1 ребро → несколько типизированных выходов) уже выражен каноном
  > `IDynamicGraphEdge.ToNodeIds[]` + `GraphEdgeKind.MultiOutput`; отдельный обучаемый
  > merge в этой фазе НЕ вводится. Решение fan-in = baseline консистентно с Q14
  > (порядок/сборка = инфраструктура; обучаемость = выбор авторитета в `EdgeAuthorityUnit`).
  > Где спроектировано: `claude-plan-02-edge.md` **§5A.3** (алгоритм N→1 merge + контракт)
  > + §5A.4 (симуляция перм-инвариантности) + §13 п.6; статус §5A.5.
  > **RESOLVED(Luba 2026-06-30):** обучаемый арбитр конфликтов слияния
  > (`EdgeFanInArbiterUnit`, настоящий trainable `ILogicalUnit` по R4: typed state +
  > checkpoint + ablation `>= 0.02`) **НЕ вводится в v1** — v1 остаётся на детерминистическом
  > baseline `IEdgeFanInMerger.Merge`; обучаемый fan-in арбитр = deferred-to-later-phase.
  > Единственное обучаемое ядро ребра в v1 = `EdgeAuthorityUnit`. *(Источник: шаг починки
  > R4-edge-roles / B-16; решение Luba 2026-06-30.)*
- Q12b. Downstream-арбитр приёмки (источник `DownstreamAccepted`) и узел коррекции
  Critic/Repair (B-15) — подтверждение дефолта.
  > **Решение (B-15, RESOLVED — спроектировано):** источник `DownstreamAccepted` =
  > детерминированный baseline `EdgeDownstreamAcceptanceUnit` (L2, `acceptance_baseline`,
  > signal/eval-only — НЕ обучаемый, чтобы исключить reward-hacking «модель сама себе
  > ставит оценку»); критерий приёмки = `VerdictRank` + confidence-floor + явный
  > REPROCESS. Коррекция вход→выход = L3 `EdgeCorrectionFlow` (Critic-узел → Repair-узел,
  > лимит глубины `MaxTransitionDepth=3`, `AdaptiveLogicalOrchestrator.cs:22`). Это
  > закрывает «голый булев» `DownstreamAccepted`: он — типизированный вердикт названного
  > L2-арбитра, питающий `Chosen`/`Rejected` DPO (блок 07/09). Семантический финал ребра
  > остаётся единственным обучаемым — `EdgeAuthorityUnit`. Где спроектировано:
  > `claude-plan-02-edge.md` **§5A.1** (арбитр приёмки) + **§5A.2** (узел коррекции) +
  > §13 п.7; статус §5A.5.
  > **RESOLVED(Luba 2026-06-30):** Critic/Repair-узлы **остаются честным детерминистическим
  > baseline в v1** (НЕ обучаемые L2); обучаемая Critic/Repair-коррекция = deferred-to-later-phase.
  > Семантический финал ребра остаётся единственным обучаемым в v1 = `EdgeAuthorityUnit`.
  > *(Источник: шаг починки R4-edge-roles / B-15; решение Luba 2026-06-30.)*

**Блок 03 (оркестратор):**
- Q13. **Закрыт каноном §A.4.1/§A.4.2** (`GraphEdgeKind` 5 членов / `string
  AuthorityUnitId` / `ParallelEligible`). Требуется лишь финальное подтверждение Luba:
  утвердить NEW-сигнатуры `IDynamicGraphEdge` и `DynamicGraphTopology` как адаптер поверх
  `BuildDynamicDomainTransitions` (`AdaptiveLogicalOrchestrator.cs:825`) и как результат
  `BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)` с budget-proof /
  `budget_exceeded` / mutation-evidence Gate B из блока 04.
- Q14. Порядок рёбер (параллель/очередь) — оставить детерминистическим baseline
  (`DomainGraphClosure` + E35; вызовы `AdaptiveLogicalOrchestrator.cs:531/552`) или сделать
  обучаемый предиктор зависимостей (Jaccard E49/E50 → baseline для ablation)? По умолчанию
  — детерминистический.
  > **Решение Luba 2026-06-29 (RESOLVED):** v1 = детерминистический честный baseline для
  > порядка обхода/зависимостей рёбер (OK_INTERNAL_DETAIL); обучаемость живёт в
  > `EdgeAuthorityUnit`. Обучаемый предиктор зависимостей (Jaccard E49/E50) отложен; если
  > вводить позже — ТОЛЬКО как настоящий trainable `ILogicalUnit` (типизированное состояние +
  > checkpoint + ablation gate, R4), с детерминистическим порядком как ablation-baseline.
  > *(Источник: §E.0; задание Luba 2026-06-29.)*
  > **Q14-graph-order (B-10, RESOLVED — спроектировано):** контрадикция «Q14 закрыл только порядок
  > **рёбер одного уровня**, а `ILearningDynamicGraph` по определению учит соотношения паттернов
  > **РАЗНОГО ПОРЯДКА**» снята. Введён отдельный обучаемый узел **`GraphOrderResolverUnit`** (NEW L2;
  > владелец канона — блок 04, `claude-plan-04-dynamicgraph-machine.md` **§3.3-bis**): резолвит
  > межуровневый порядок паттернов order-0..N (кто атомарный, кто собирает составной, в каком порядке
  > передавать). Инстанцированный blueprint (не промпт исполнителю): контракт I/O
  > (`GraphOrderResolveInput`/`GraphOrderResolveDecision` + узловой `GraphOrderCandidate`,
  > канон-форма B-9 — отложенный `IQueryable`); typed trained-state **`GraphOrderState`**
  > (`OrderPreferenceMargins: OrderPairKey→double` = соотношение пар уровней; `OrderFeatureWeights`;
  > KNN-`CapabilityPrototypes`, K=5); training-update (`GraphDpoPair` chosen/rejected порядок →
  > DPO-сдвиг маржинов через `DpoTrainer.cs:44/119` → checkpoint машины Export/Import,
  > continue-training не replay); роль **KNN** = только признак похожести capability-кластеров
  > (`OK_MODEL_COMPONENT`, не финальная власть, сырые prompt не хранятся → нет exact-prompt leakage);
  > **ablation** = детерминированный Kahn-топосорт baseline, число **`≥ 0.02`** (Q11
  > `ablation_threshold_locked`, единый); forbidden leakage = candidate-index/позиция/array-order/
  > source-path/source-id/exact-prompt/`BaselineOrderHash`-как-признак. Вызывается машиной внутри
  > `BuildDynamicTopology`; оркестратор 03 только читает результат (см. также
  > `claude-plan-03-orchestrator.md §3` нота B-10 и `claude-plan-05-partA-hierarchy.md §3.4` шаг 4).
  > **RESOLVED(Luba 2026-06-30):** `GraphOrderResolverUnit` **НЕ поднимается** до 8-го
  > верхнеуровневого NEW-типа Part A — остаётся machine-internal обучаемым L2-компонентом блока 04
  > (KNN K=5, `OK_MODEL_COMPONENT`); канон-счётчик «ровно 7 NEW-типов» НЕ изменён. Обучаемость
  > порядка паттернов разного порядка живёт ВНУТРИ машины графа, без отдельного Part A-типа.
  > *(Источник: шаг починки B-10 / R3-graph-order; решение Luba 2026-06-30.)*
- Q15. **Закрыт каноном §A.4.2:** per-edge авторитет хранится как `string
  AuthorityUnitId` (ссылка в реестр, резолв `ResolveActiveUnit`/latest-wins,
  `AdaptiveLogicalOrchestrator.cs:269`) в **поле ребра `DynamicGraphTopology`** (не
  отдельный реестр владельцев); это определяет, где живёт авторитет при export/import
  графа (блок 04 §3.4/§10). Требуется лишь финальное подтверждение Luba.

**Блок 06 (async-комбинации):**
- Q16. Утвердить `budget.MaxCombinations` по умолчанию = 4 (потолок 8 =
  `MaxAdaptiveBranchCount`, `AdaptiveLogicalOrchestrator.cs:19`) или иной потолок для
  широких входов.
  > **Единый owner-контракт прогона + дефолт N (B-7, RESOLVED — спроектировано, шаг починки
  > R8-async-contract):** до этого шага был незакрытый рассинхрон — контракт «прогнать X через N
  > комбинаций» был объявлен в трёх файлах ТРЕМЯ разными формами: (1) блок 06 §5.2 — канон
  > `IGraphCandidateRunner.RunCandidatesAsync(string input, int combinationCount)`; (2) блок 09 —
  > классдиаграмма держала `RunCandidatesAsync(X)` БЕЗ N как метод НА самом
  > `AutoDpoLoopOrchestrator` (расходилась с собственной прозой §5.6); (3) guide-05 — alias
  > `GraphAsyncCombinationRunner.RunCandidatesAsync(X, N)` (имя owner-типа НЕ совпадало с каноном).
  > Сведено к ЕДИНОМУ owner-контракту: **`IGraphCandidateRunner.RunCandidatesAsync(string input,
  > int combinationCount, CancellationToken ct = default) → OperationResult<IReadOnlyList<GraphCandidateOutput>>`**,
  > canon-владелец — **блок 06 §5.2**; блок 09 и guide-05 — ПОТРЕБИТЕЛИ (инжектируют
  > `IGraphCandidateRunner`, зовут owner-форму С N, не переобъявляют). Конкретно: (a) в блоке 09
  > классдиаграмма исправлена — `RunCandidatesAsync(X)` на оркестраторе УДАЛЁН, добавлен класс
  > `IGraphCandidateRunner` + ребро делегации `шаг1`; контракт петли = `RunLoopAsync(GraphAutoDpoCycleRequest, ct)`,
  > N берётся из `GraphAutoDpoCycleRequest.combinationCount` и пробрасывается в owner-форму; (b) в
  > guide-05 alias `GraphAsyncCombinationRunner` переименован в `IGraphCandidateRunner` во всех точках
  > (classDiagram, легенда, симуляция S1/S2, шаг 1 прозы, delta-нота D2), сигнатура приведена к
  > `(string input, int combinationCount)`. **Дефолт N зафиксирован: рекомендуемый `N=combinationCount=4`,
  > потолок 8** (`MaxAdaptiveBranchCount`, `AdaptiveLogicalOrchestrator.cs:19`); `N==combinationCount` —
  > ЕДИНСТВЕННЫЙ источник N (формы без N запрещены). Источник числа = ЭТОТ Q16 / блок 06 §12 Q4;
  > один владелец числа — блок 06, потребители только пробрасывают. Канон-счётчик «7 NEW-типов Part A»
  > НЕ тронут (`IGraphCandidateRunner`/`IGraphCombinationStrategy`/`CombinationPlan`/`GraphCandidateOutput` =
  > infra-контракты прогона блока 06, не верхнеуровневые Part A-типы). Где спроектировано: блок 06
  > §5.2 (владелец), блок 09 §5.5/§5.6 + классдиаграмма (потребитель, conformance-долг закрыт),
  > guide-05 §0/§3 шаг 1 (потребитель). **RESOLVED(Luba 2026-06-30):** финальное числовое значение
  > **`N (combinationCount)` по умолчанию = 4, потолок = 8** (`MaxAdaptiveBranchCount`,
  > `AdaptiveLogicalOrchestrator.cs:19`); `N==combinationCount` — единственный источник N, формы
  > без N запрещены. *(Источник: шаг починки R8-async-contract / B-7; решение Luba 2026-06-30.)*
- Q17. Утвердить `goalConfidenceFloor` (предложено 0.80) для зова легального teacher за
  `Y_goal`.
  > **Связь с реальным вызовом (B-2/B-3, спроектировано):** `goalConfidenceFloor` теперь —
  > поле `TeacherCallRequest.GoalConfidenceFloor` реального egress-порта `ITeacherClient`
  > (`claude-plan-08 §3.6.1`); default 0.80 передаётся обёрткой `ITeacherGoalProviderUnit`
  > (блок 09 Шаг 3). **RESOLVED(Luba 2026-06-30): `goalConfidenceFloor = 0.80` залочено** как
  > дефолт v1; место его применения в реальном пути вызова определено. *(Источник: шаг починки
  > R5-teacher-call; решение Luba 2026-06-30.)*
- Q18. Куда приземлять обучение пар: новая L5-машина графа или временный мост через
  `AutonomousTrainer.TrainAsync(...dpoPairs)`?

**Блок 12 (no-hidden-determinism / cutover):**
- Q-mech4. Авторитет механизма #4 (план фаз сценария: ВЫБОР `PlanPhases` + ПОРЯДОК
  `PhaseOrder/PhaseIndex/BuildPhases`) — оставить детерминированным baseline или поднять в
  обучаемый L4/L5-юнит?
  > **B-22 / R13-reconcile (RESOLVED-как-проект, 2026-06-30):** до этого шага #4 был
  > единственной диспозицией среди 9 механизмов со статусом **DEFERRED без блюпринта** — это
  > была реальная контрадикция с двух сторон: (1) matrix R5 печатал no-hidden-determinism
  > как `covered` «после cutover», а `claude-plan-12` §8.2 заявлял #4 **не закрытым**;
  > (2) канон R4/A-LM-09 требует, чтобы **каждый** из 9 механизмов имел вердикт + (для
  > forbidden/baseline) блюпринт. Снято применением **прецедента Q14** (детерминированный
  > финал-ПОРЯДОК рёбер уже решён как honest baseline + обучаемый путь по требованию):
  > порядок/выбор ФАЗ — тот же класс задачи на L4-пути оркестрации. Диспозиция #4 теперь
  > **спроектирована**: рекоменд. дефолт v1 = **`BASELINE_MUST_BE_LABELLED`** (honest
  > deterministic baseline, `isLearned=false`, `phase_plan_semantic_quality_claimed=false`),
  > плюс **инстанцированный** (не промпт) обучаемый блюпринт замены `SemanticPhasePlanUnit`
  > (typed `PhasePlanState`: PhaseVocabulary/PhaseSelectMargins/PhaseOrderMargins/
> FeatureWeights/KNN K=5; training-update `GraphDpoPair(Level="unit", NodeOrEdgeId=SemanticPhasePlanUnit/phase-plan unit ref)` chosen/rejected
  > → DPO-сдвиг маржинов через `DpoTrainer.cs:44/119` → checkpoint schema `graph-phase-plan/1`,
  > continue-training не replay; ablation Kahn-baseline `≥0.02` единый Q11; forbidden-leakage
  > candidate-index/array-order/source-path/source-id/exact-prompt/BaselinePlanHash). Где
  > спроектировано: `claude-plan-12` §13 Q1 (полный блюпринт) + §8.2 + §3.0 + §3.B + Gate B
  > п.6 + строка #4 в §7 (все приведены в согласие); matrix R5 синхронизирован (примечание
  > о #4). После B-22: «немаркированных финальных авторитетов — 0» верно и для ребра, и для
  > L4-пути #4; `classified_mechanism_count == 9` сохранён; baseline НЕ удаляется (нужен для
  > ablation). **RESOLVED(Luba 2026-06-30):** обучаемый `SemanticPhasePlanUnit` **НЕ
  > активируется в v1** — план фаз сценария остаётся детерминистическим
  > `BASELINE_MUST_BE_LABELLED` baseline (`isLearned=false`,
  > `phase_plan_semantic_quality_claimed=false`); инстанцированный обучаемый блюпринт
  > сохраняется как deferred-to-later-phase (исполняется только при будущей активации). baseline
  > НЕ удаляется (нужен для ablation). *(Источник: шаг починки B-22 / R13-reconcile; прецедент
  > §E.0 Q14; образец блюпринта §5.3 + B-10; решение Luba 2026-06-30.)*

**Блок 07 (DPO-строитель):**
- Q19. `chosen_origin=first_party_deterministic` — оставить как baseline-режим (honest
  scaffold, `semantic_quality_claimed=false`) или запретить и требовать teacher для каждой
  train-ready пары?
- Q20. Пороги `overlap_ratio < 0.60` и `loss < 0.685` — оставить как канонические или
  задать иные для графа?
  > **RR9-lock-numbers (RESOLVED, Luba-default 2026-06-30):** DPO `overlap_ratio < 0.60` = LOCK
  > (различимость chosen/rejected, владелец блок 07 `TriadVerdict.OverlapScore`/Gate B G4);
  > `loss` приёмки = `< 0.693` (ln2 = random-guess, единый с блоком 09/11). Записано в
  > `claude-plan-07` §3/§10 G4 и `claude-plan-09` §10. *(Источник: шаг RR9-lock-numbers.)*
- Q21. Расхождение указателей train-ready (V192 vs V309 в «internal learnin 2») — какой
  канонический перед обучением графа? *(R-18.)*

**Блок 09 (авто-DPO петля):**
- Q22. Порог остановки `qualityThreshold`: предложено held-out `AccuracyAfter >= 0.95` +
  разрыв с teacher закрыт; утвердить или задать per-уровень (unit/edge/graph)?
  > **Решение Luba 2026-06-29 (RESOLVED, выровнен под Q30):** held-out `AccuracyAfter >= 0.95`,
  > ЕДИНЫЙ порог для unit/edge/graph (не per-уровень). **RR9-lock-numbers (Luba-default
  > 2026-06-30, LOCKED):** «Разрыв с teacher закрыт» теперь ИЗМЕРИМ и залочен —
  > `teacher_gap_closed := HeldOutAccuracy(Y_best) >= HeldOutAccuracy(Y_goal) − teacherGapEpsilon`
  > с **`teacherGapEpsilon = 0.02`** (ученик в пределах 2 п.п. от teacher на той же held-out
  > метрике; прежний `0.0`/`default-pending-pilot` снят). Записано в `claude-plan-09`
  > `AutoDpoLoopConfig` (§5.3), §3 Шаг 6 и Gate B §10 п.3/п.6. *(Источник: §E.0 / Q30;
  > задание Luba 2026-06-29 + RR9-lock-numbers 2026-06-30.)*
- Q23. Разрешён ли режим `self-best` (chosen=Y_best без внешнего teacher) как легальный
  дефолт, с включением teacher только при настроенном legal-провайдере?
  > **Реальный путь вызова teacher/judge + провайдер (B-2, B-3 / шаг R5-teacher-call, RESOLVED —
  > спроектировано):** до этого шага был незакрытый блокер — план описывал «таможню»
  > провенанса (`TeacherProvenanceGateUnit`, блок 08) и обёртку `ITeacherGoalProviderUnit`
  > (блок 09 Шаг 3), но **за обёрткой не стоял реальный клиент**: не было интерфейса вызова,
  > контракта запрос/ответ, выбора провайдера, шва авторизации, предвызовного terms/license-гейта.
  > Спроектированы два egress-порта (владелец канона — `claude-plan-08 §3.6`): **`ITeacherClient`**
  > (`.RequestGoalAsync(TeacherCallRequest, ct) → TeacherCallResult{GoalOutput, TeacherProvenance 20/20 (RR6),
  > Status}`, §3.6.1) и **`IJudgeClient`** (`.JudgeAsync → JudgeCallResult{WinnerIndex, Score, WrongReason,
  > Status}`, §3.6.2). Реальный путь сделан продуктовым: **mock — только test-only** (инвариант
  > `product_path_uses_mock==false`, §3.6.6; продуктовый DI даёт `HttpTeacherClient`/`HttpJudgeClient`).
  > **Provider-selection** (§3.6.3): `ProviderConfig` — данные в `allowed-teacher-registry.json`, НЕ
  > бренд в логике; для **v1** привязан дефолт-профиль `provider_config_id="qwen3-together-v1"`
  > (`model_id="Qwen/Qwen3-235B"`, `model_license="Apache-2.0"`, нейтральный провайдер из
  > {Together/Fireworks/OpenRouter/DashScope}, `role="teacher"`). **Auth-seam** (§3.6.4):
  > `IProviderCredentialResolver` читает секрет по имени `CredentialKeyRef` (значение ключа НЕ в
  > плане/коде/реестре/провенансе). **Связь с legal-гейтом (главное правило R5/B-2):** каждый
  > teacher-вызов проходит `TeacherProvenanceGateUnit.Evaluate` **ДО сетевого байта** — denylist
  > `model_id` (Claude/OpenAI/Gemini) → `Status=LEGAL_BLOCKED`, сеть НЕ трогается (NEG-тест: счётчик
  > сетевых вызовов = 0). **Judge allow-list** (§3.6.2/3.6.3): closed-модель допустима ТОЛЬКО
  > `role="judge"` при `teacher_output_used_for_training=false` (ветка гейта C11); anti-authoring
  > guard блокирует судью, вернувшего новый текст (`AUTHORED_NEW_TEXT_BLOCKED` → не может стать chosen,
  > иначе C12 `DATASET_REBUILD_REQUIRED`). Если профиль недоступен → честный `provider_unavailable`,
  > петля легально уходит в `self_best` (это и есть ответ Q23: `self_best` — легальный дефолт, teacher
  > включается только при настроенном legal-профиле). Где спроектировано: `claude-plan-08 §3.6`
  > (.1–.8, владелец), потребители — `claude-plan-09 §3 Шаг 2/Шаг 3`, политика —
  > `claude-04-dpo-legal-model-policy.md` «Real teacher/judge call path». Канон-счётчик «7 NEW-типов
  > Part A» НЕ тронут (egress-порты = infra-шов блока 08, не верхнеуровневые Part A-типы).
  > **RESOLVED(Luba 2026-06-30):** (a) финальный нейтральный `hosting_provider` для `qwen3-*` =
  > **Together** (`provider_config_id=qwen3-together-v1`, `model_id=Qwen/Qwen3-235B`,
  > `model_license=Apache-2.0`, `role=teacher`); (b) расширение teacher-allowlist «иными open-weight
  > permissive» = deferred-to-later-phase (v1 = только профиль `qwen3-together-v1`); (c) judge-allowlist
  > = **тот же нейтральный набор провайдеров**, closed-модель допустима ТОЛЬКО `role=judge` при
  > `teacher_output_used_for_training=false`. *(Источник: шаг починки R5-teacher-call / B-2,B-3;
  > решение Luba 2026-06-30.)*
- Q24. Параметры по умолчанию: `maxIterations`, `plateauWindow`, `regressionWindow`,
  `minImprovement=0.02`, replay 70/30 — принять предложенные или задать свои?
  > **RR9-lock-numbers (RESOLVED, Luba-default 2026-06-30):** `plateauWindow = 3`,
  > `maxIterations = 20` ЗАЛОЧЕНЫ как Luba-дефолты (Q6-стоп петли по плато/бюджету);
  > `minImprovement = +0.02` (уже Q30), replay = `70/30` (образец `ReplayBuffer.cs`),
  > `regressionWindow` = `3` (тот же tail как plateau). Записано в `claude-plan-09`
  > `AutoDpoLoopConfig` (§5.3) и Gate B §10 п.6. *(Источник: шаг RR9-lock-numbers.)*
  > **Вход/cold-start/триггер/стоп петли (B-1, B-5, B-6, B-11, B-12, RESOLVED —
  > спроектировано, шаг починки R7-loop-entrypoint):** до этого шага был незакрытый
  > блокер — петля (блок 09) описывала шаги 1→6, но НЕ имела (1) конкретного источника
  > входа `X` и runtime-контракта запуска; (2) построения cold-start первого
  > source-disjoint held-out (а «данных пока нет», §E.0 — значит экзамен нечем мерить
  > до первого обучения); (3) ЧИСЛОВОГО триггера низкого результата, привязанного к
  > детектору; и не залочила в Gate B пороги Q30 и cold-start-дефолт `self_best`.
  > Спроектировано в `claude-plan-09 §3 Шаг 0/1/2, §5.3/§5.5, §10 Gate B п.3/4/6/7`:
  > **(B-1) entrypoint** = `AutoDpoLoopOrchestrator.RunLoopAsync(GraphAutoDpoCycleRequest,
  > ct)` дренирует `_inputQueue`, заполненную `ISeedCorpusReader.LoadInputs(SeedCorpusRef)`;
  > **источник `X` дефолт v1** = first-party `learning-sets`
  > (`llm_mvp_core/training-data/<domain>/learning-sets/*.jsonl`, common-нога, R5/Q6);
  > прогон N комбинаций делегируется в owner-контракт блока 06
  > `IGraphCandidateRunner.RunCandidatesAsync(string input, int combinationCount)` —
  > это ТАКЖЕ закрывает conformance-долг, зафиксированный блоком 06 §5.2 (форма без N на
  > оркестраторе устранена). **(B-5) cold-start held-out** = `BuildColdStartHeldOutAsync`:
  > детерминированный сплит по `stable_hash(SourcePromptEntryId) mod 100 < heldOutPercent`
  > (дефолт 20%), source-disjoint, метка из самого корпуса, валидация `SplitProof` (блок 11,
  > `near-dup>0`→отклонить); `seed_corpus_missing` → `stop_reason=no_seed_corpus`,
  > честный `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. **(B-6) числовой триггер** =
  > `WeaknessTriggerConfig`: `low_result := Confidence<confidenceFloor(0.80) OR
  > Verdict∈{REJECT,KNOWLEDGE_EDGE} OR HeldOutAccuracy(Y_best)<0.95 OR teacher-gap` —
  > детерминированный честный предикат, ВКЛЮЧАЕТ обучение (не выбирает chosen, R4-чисто).
  > **(B-11) стоп-порог залочен в Gate B** = `qualityThreshold=0.95` (Q30/Q22), плюс
  > `delta≥+0.02` и `RewardMargin≥0.025` как приёмочный профиль промоушена. **(B-12)
  > `self_best` залочен как cold-start дефолт** (Q23): legal-провайдер не настроен ⇒
  > `teacher_mode=self_best` без паузы; teacher_on только при настроенном legal-профиле +
  > `ITeacherClient.Status==OK`. Где спроектировано: `claude-plan-09` (владелец петли),
  > потребляет блок 06 (runner), блок 08 (teacher/judge call path), блок 11 (SplitProof/
  > held-out measurement). **RESOLVED(Luba 2026-06-30):** (a) источник `X` (v1) = **ТОЛЬКО
  > seed `learning-sets`** (`llm_mvp_core/training-data/<domain>/learning-sets/*.jsonl`);
  > runtime-наблюдения = deferred-to-later-phase; (b) числовое **`N (combinationCount)` = 4,
  > потолок 8** (владелец числа — блок 06 §12 Q4); (c) `confidenceFloor` = **0.80** единый
  > (per-домен ужесточение для product-узлов = deferred-to-later-phase); (d) **`heldOutPercent`
  > = 20%**. *(Источник: шаг починки R7-loop-entrypoint / B-1,B-5,B-6,B-11,B-12; решение
  > Luba 2026-06-30.)*
  > **T1-bootstrap-input (open → RESOLVED-with-bootstrap-blueprint, 2026-07-01).** Прежний
  > блок называл «источник входа X = дефолт seed learning-sets», смешивая два РАЗНЫХ
  > объекта cold-start; находка «seed отсутствует / данных пока нет» не была закрыта как
  > явная зависимость. **Разведено (владелец — `claude-plan-09 §ADD-09.3-bis`):**
  > **(a) DPO-пары ГЕНЕРИРУЮТСЯ петлёй** — на входе их НЕТ и не требуется (это ЦЕЛЬ
  > задания: запуск проекта → низкий результат → teacher-goal/`self_best` → DPO →
  > обучение до held-out 0.95); **(b) input-prompt seed = REQUIRED-precondition** — это
  > источник ВХОДНЫХ ЗАДАЧ X, без которого `_inputQueue` пуст и петле нечего запускать.
  > (b) закрыт новым **Seed-Corpus Existence Proof Gate** (`ProveSeedCorpusExistsAsync` →
  > `SeedCorpusExistenceProof`, вызывается в `PrepareAsync` ДО `BuildColdStartHeldOutAsync`):
  > REQUIRED-precondition, НЕ молчаливый дефолт; **`required_min_entry_ids = 50` LOCKED**
  > (>=50 РАЗНЫХ `SourcePromptEntryId`, first-party/allowed-license) + **блюпринт авторинга**
  > `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl`
  > (`SeedCorpus-LanguageTranslate-v1`; старый `seed-input-prompts.jsonl` = historical/pre-Pass10 alias). При
  > `seed_existence_proven=false` петля честно `cold_start_blocked=true /
  > stop_reason=no_seed_corpus / PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (не ошибка, не стоп-проекта).
  > PLAN-ONLY: файла на диске нет («данных пока нет»), авторинг = реализация после approve Luba.
  > *(Указатель: `claude-plan-09 §ADD-09.3-bis` + §12 п.7 T1-нота; зеркало карты — `claude-plan-14 §3.2` строка seed-precondition.)*

**Блок 10 (обучение+lifecycle):**
- Q25. `IncrementalTrainer` (14): ведущий вариант B2 — вывести из пути обучения графа
  (канон = `LogicalDpoTrainingUnit` из 18) с пометкой `NOT_ON_GRAPH_TRAINING_PATH`; или B1
  — реальное делегирование в LoRA+DpoTrainer?
- Q26. `IModelServer` scope: достаточно ли `LocalFileModelServer` (локальный файловый
  транспорт весов), а сетевой HTTP — отдельный work-order вне блока 10?
- Q27. Чинить захардкоженный `RewardMargin` (литерал 0.03,
  `27.TranslationDomain/TranslationMachine.cs:70/125/187`) прямо в эталоне в этом блоке,
  или вынести в отдельный блок «эталон обучения»? *(R-19.)*
- Q28. Tatoeba (CC-BY) допустим как источник common-training (не MIT/Apache), или только
  evidence/user-adaptive?
- Q29. Унифицировать ранг LoRA (сейчас 2 vs 8: `DefaultLogicalCapabilityTrainer.cs:858` vs
  `LoraConfig.cs:5`) для узлов графа, или per-tier с обоснованием?

**Блок 11 (измерение):**
- Q30. Доменный приёмочный порог held-out accuracy для узлов/рёбер: каркас предложен
  (Δ≥+0.02, reward-margin≥0.025, held-out≥0.95 как в переводе) — подтвердить число 0.95
  или иное.
  > **Решение Luba 2026-06-29 (RESOLVED):** `RuntimeProductAccuracy >= 0.95` И
  > `delta(AccuracyAfter - AccuracyBefore) >= +0.02` И `RewardMargin >= 0.025`; ЕДИНЫЙ порог
  > для unit/edge/graph; каркас заимствован из доказанного пути перевода. Под-пункты, которые
  > Luba явно НЕ выбирала числом, остаются помеченным default «подтвердить при первом пилоте»,
  > НЕ binding: e2e строгость (предлагается N-1 на малом пилотном held-out, строгое 6/6 на
  > полном) и «разрыв с teacher закрыт» = judge-паритет. Статус: RESOLVED по трём числам
  > (0.95 / +0.02 / 0.025). **RR9-lock-numbers (Luba-default 2026-06-30, LOCKED):** под-пункты,
  > ранее `default-pending-pilot`, закрыты дефолтами — e2e строгость = `N-1` на малом пилотном
  > held-out, `6/6` на полном (Q31); «teacher gap closed» = в пределах `teacherGapEpsilon = 0.02`
  > (Q22). *(Источник: §E.0; задание Luba 2026-06-29 + RR9-lock-numbers 2026-06-30.)*
- Q31. Строгость e2e-порога графа: E51B требует 6/6 (`e2e_trace_success_rate`); для
  DynamicGraph принять 100% на малом held-out или допустить N-1?
  > **RR9-lock-numbers (RESOLVED, Luba-default 2026-06-30):** e2e строгость ЗАЛОЧЕНА —
  > **`N-1` на малом пилотном held-out** (один промах допустим, чтобы малый набор не давал
  > ложный 0/N), **строгое `6/6` (100%) на полном held-out**. Прежний `default-pending-pilot`
  > снят. Записано в `claude-plan-11` §10 п.* и `claude-plan-09` Gate B §10. *(Источник: шаг
  > RR9-lock-numbers.)*
- Q32. Существующий `CrossStackBusinessFlowGraphDpoPair`
  (`ProjectWikiDomainModels.cs:918`) — адаптируется под канон `GraphDpoPair` или канон
  строится поверх доменных пар?
- Q33. Воспроизводимые пути артефактов измерения: часть раннеров пишет в
  `C:/Users/lubao/.xiigen/...` (`Learn06HeldOutAblationRunnerTests.cs:30-33`) — задать
  переносимые фикстуры/repo-пути по R5?

**Блок 13 (тест-стратегия):**
- Q34. Числовые пороги Gate B для узлов/рёбер: переиспользовать пороги перевода
  (`RuntimeProductAccuracy>=0.95`, `RewardMargin>=0.025`,
  `TranslationProductHeldOutRunner.cs:211-220`) или задать отдельный профиль для
  `DynamicGraphDomain`?
  > **Решение Luba 2026-06-29 (RESOLVED, выровнен под Q30):** переиспользовать пороги перевода
  > как ЕДИНЫЙ профиль для unit/edge/graph: `RuntimeProductAccuracy >= 0.95`,
  > `delta >= +0.02`, `RewardMargin >= 0.025` (отдельный профиль для `DynamicGraphDomain` НЕ
  > вводится). *(Источник: §E.0 / Q30; задание Luba 2026-06-29.)*
- Q35. `RewardMargin` для не-трансформерных рёбер: требовать вычисленный margin (как
  `11.DPO/DpoTrainer.cs`) для каждого «обученного» ребра, или допускать рёбра-обёртки с
  честной пометкой baseline/eval-only (влияет на разделение колонок UH/TA для P-03)?
  > **C2-measure-coherence — open → RESOLVED (2026-06-30):** прежде open-вопрос о числе `RewardMargin`
  > для рёбер downgraded **open → RESOLVED** с **указателем на владельца** `claude-plan-11`
  > §6.5a (согласование числа) + §7.2 (graph-level held-out + per-level epsilon). Решение:
  > **(1)** ребро, заявленное как «обученное» (колонка TA, `PASS_AS_TRAINED_FEATURE`), ОБЯЗАНО нести
  > **вычисленный** held-out `RewardMargin` из движка `11.DPO/DpoTrainer.cs` и проходить **target
  > `RewardMargin >= 0.025`** (Q30 LOCKED); литерал `0.03` (`TranslationMachine.cs:70/125/187`) и
  > любой `[WRONG:]`-плейсхолдер как margin ЗАПРЕЩЕНЫ (R-19, чинит блок 10).
  > **(2)** ребро-обёртка без вычисленного margin допустимо ТОЛЬКО как честно помеченное
  > `baseline`/`eval-only` (колонка UH, `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`), НЕ как trained-ребро.
  > **(3) Честная нота о текущем evidence (не скрывать):** реально наблюдаемый margin сегодня — **live
  > ~0.01** (v196: `AverageRewardMargin≈0.01217`, `FinalRewardMargin≈0.01013`), что **ниже** target
  > 0.025; held-out margin рёбер графа ещё НЕ измерен. Значит 0.025 — это **target-to-prove**, и
  > пока он не доказан held-out'ом на source-disjoint, ребро остаётся UH (scaffold), а не TA. Live
  > ~0.01 и target 0.025 — разные оси (live training-stream vs held-out промоушен-гейт), не
  > сравниваются как «провал порога» напрямую (Гейт 1).
  > **(4)** epsilon lifecycle ребра (fresh-load/rollback) = `0.02` per-level (`LevelEpsilons`,
  > `claude-plan-11` §7.2). *(Источник: шаг C2-measure-coherence; владелец чисел — `claude-plan-11`
  > §6.5a/§7.2; синхрон — `claude-guide-03` §5.3; R-19 / Q27 / Q30.)*

**Блок 14 (карта референсов):**
- Q36. Прикладной приоритет v1: какой шов первичен — figma→html+css (FLOW-31 + FigmaToCode
  + CanvaToCode) или better-output→DPO (FLOW-38 + SmartCodeAssistant/SmartWiki)?
  > **Решение Luba 2026-06-29 (RESOLVED по host-mapping шва figma→html+css, см. Q-host):**
  > для шва figma→html+css публичный API-хост = `xiigen-platform-dot-net-angular` (PER
  > AUTHORITY), внутренний дизайн-движок = `xiigen-flows/FLOW-31` (Вариант B: xiigen-platform
  > = публичный фасад, делегирует во FLOW-31; FigmaToCode-design-gen + CanvaToCode = клиенты).
  > `requirement_host_project_mapping_recorded=true`; снять partial с Ф10.1.1/Ф10.1.2 ПО
  > ЧАСТИ выбора хоста. Execution-note: фактической реализации фасада→FLOW-31 на диске нет
  > (рекон-backing R16 = MISSING) — строится/верифицируется на этапе реализации.
  > *(Источник: §E.0 / Q-host; задание Luba 2026-06-29.)*
  > **B-19 (R10-host-mapping) — RESOLVED + ИНСТАНЦИРОВАНО (2026-06-30):** статус
  > «host назван, фасад не спроектирован» снят. Фасад figma→html+css спроектирован
  > построчно (Part A): `IDesignToCodeFacade.AnalyzeDesignAsync(DesignToCodeRequest request,
  > CancellationToken ct)` в host-проекте `xiigen-platform` (тонкий — R6, без доменной
  > логики) → делегирует в `IDesignToCodeSeam.AnalyzeAsync` (`xiigen-flows` рядом с FLOW-31
  > `DesignIntelligenceEngineService.cs:47-90`); 5-шаговый алгоритм (валидация → без
  > доменной логики в фасаде → делегация → граница core≠host-клиента G11 → возврат) +
  > «что НЕ делает». Инварианты `facade_carries_no_domain_logic`/`facade_delegates_to_xiigen_flows_seam`/
  > `figma_css_emitted_by_client_not_core`/`client_host_entrypoint_not_substituted_by_core_seam`
  > = Gate B G14. Паттерн фасад→шов = образец B-4 (`IBetterOutputFeedbackFacade`→
  > `Flow38BetterOutputSeam`); NEW-типы = host-контракты (вне `llm_mvp_core`), счётчик «7
  > NEW» не тронут. Остаётся execution: реализация фасада→FLOW-31 на диске (рекон-backing
  > R16 MISSING). *(Источник: шаг R10-host-client / B-19; владелец `claude-plan-14` §6.1а.)*
  > **B-18 (R10-client-local) — RESOLVED + СПРОЕКТИРОВАНО (2026-06-30):** недостающий
  > поток «клиент улучшает СВОЮ локальную модель» спроектирован как ТРЕТЬЯ нога размещения
  > (после common Q6 / server-adaptive Q38): `IClientLocalGraphModel` (конкретизация
  > Personal-плеча `ILearningMachinePair<DynamicGraphPersonal,DynamicGraphGlobal>`) —
  > `ApplyLocalUpdateAsync` (локальный такт Load→Step→Evaluate на приватных данных клиента,
  > реальный DPO-сдвиг весов `DpoTrainer.cs:44/119`, локальный held-out lift ≥ +0.02 Q11,
  > НЕ `SetValue`-маскарад), `ExportLocalCheckpointAsync`/`ImportLocalCheckpointAsync`
  > (export/import round-trip воспроизводит accuracy ±epsilon в новом процессе, образец
  > `TranslationMachine` `SaveCheckpointAsync :388`/`LoadCheckpointAsync :447`, schema
  > `"graph-client-local/1"`). `LegScope=PersonalAdaptive` ВСЕГДА, нет авто-publish в common
  > (Q38, ветка C16 блока 08); privacy T3/C9: приватные данные не покидают клиента, экспорт
  > без сырого приватного текста (`RawPrivateTextCopied==false`). Гейты §8.1: client_local_
  > privacy/roundtrip/no_autopublish/real_training. **RESOLVED(Luba 2026-06-30):** (1) размещение
  > personal/client-local store = под корнем **`<project>/.xiigen`** (точный client-local checkpoint-путь
  > под этим корнем = deferred-to-later-phase вместе с активацией ноги); (2) двухплечевой pair в v1 =
  > **НЕ активируется** — v1 = только common-нога; client-local модель (Personal-leg) =
  > deferred-to-later-phase. *(Источник: шаг R10-host-client / B-18; владелец `claude-plan-10`
  > §7.1 + §11.2; решение Luba 2026-06-30.)*
  > **RR7-dualpath (OP-64, dual-path host-сценарии) — open → RESOLVED (2026-06-30):**
  > прежде open-вопрос «как материализовать двухконтурную проверку host-сценариев (Path A
  > boundary adapter/API/core + Path B локальный host-UI) с разведением host-entrypoint
  > отдельно от core-seam» (grep по трём claude-файлам = 0) downgraded **open → RESOLVED**.
  > Закрыто инстанцированной посценарной dual-path матрицей с явным **указателем на владельца**:
  > `claude-plan-14` §6.4 (3 строки SC-R16-figma-analyze / SC-R16-canva-analyze /
  > SC-R17-betteroutput-feedback; раздельные `HostEntrypointRef`/`CoreSeamRef`; per-scenario
  > `PathA`/`PathB`; счётчики `total=3/accepted=0/api_only=0/ui_only=0/blocked=3/not_started=3(PathA)`),
  > Gate B `claude-plan-14` G15, синхронная копия — доска `claude-EXECUTION-CONTROL-BOARD.md`
  > §Ф10 (dual-path блок, evidence-only), мастер RESOLVED-таблица B-row RR7-dualpath (выше).
  > Литералы соблюдены: `Dual-Path Same-Scenario Coverage`, `same scenario Path A plus Path B`,
  > `boundary/API evidence != host readiness`, `client_host_entrypoint_not_substituted_by_core_seam==true`;
  > deprecated product-readiness гейт НЕ возрождён. Закрытие сценариев = execution-долг.
  > *(Источник: дефект RR7-dualpath, 2026-06-30; владелец матрицы `claude-plan-14` §6.4.)*
  > **T2-figma-scope (figma→html+css / CanvaToCode = client-of-API, НЕ v1 training-домен) — open → RESOLVED (2026-07-01).**
  > Прежде `figma-html-css` был помечен `NOT_COVERED / вне R15-scope` и одновременно «E2E
  > требуется» → читалось как **coverage-дыра** плана auto-DPO, для закрытия которой якобы
  > нужен «up-front блокер-ряд активации домена» (seed figma + подшаги Ф10.<k> + снятие
  > legal-QUARANTINE). Это **мис-классификация**: она молча превращала клиентский сценарий во
  > второй training-домен вопреки RESOLVED «v1 training-пилот = ровно 1 = `language-translate`»
  > (§E.3-bis). **Решение (SCOPE, не coverage-дыра):** `figma→html+css` (FigmaToCode) и
  > `CanvaToCode` — это **КЛИЕНТСКИЕ сценарии-потребители API** (`client-of-API`): вызывают
  > публичный API (`IDesignToCodeFacade.AnalyzeDesignAsync` для design +
  > `IBetterOutputFeedbackFacade.SubmitFeedbackAsync` для better-output) и получают ответ; они
  > **НЕ** v1 auto-DPO training-домен и **НЕ** несут собственной петли обучения графа в v1
  > (v1 обучается ТОЛЬКО пилот `language-translate`). Значит «figma E2E» =
  > **out-of-v1-training-scope client-integration** (навигация/отложено), а НЕ NOT_COVERED-дыра;
  > «coverage» figma/canva доказывается через **API-контракт better-output/design** (Path A
  > boundary: request→response→trace, §6.1а/§6.2), а НЕ через собственную петлю обучения figma.
  > seed-строка `figma-html-css` (§ADD-MASTER.1) БОЛЬШЕ не «BLOCKING dep для v1» — у client-of-API
  > нет v1 seed-training корпуса, т.к. он не тренируется в v1; seed релевантен только при будущей
  > явной активации figma как training-домена (deferred, отдельный approve → полный
  > `DomainActivationBlueprint(D)` §ADD-MASTER.2). Соблюдён R6 (тонкий клиент, ядро в
  > `llm_mvp_core`). **Указатель:** мастер §ADD-MASTER.3-bis (владелец scope-решения) +
  > `claude-plan-14` §6.3 (client-of-API нота в R15-разделе) + `claude-plan-14` §6.1/§6.4
  > (figma-сценарии = client-host, доказываются API-контрактом, не training-петлёй).
  > *(Источник: шаг починки T2-figma-scope, 2026-07-01.)*
- Q37. Better-output как teacher (P-04/P-12): выход FLOW-38/Smart* получен от open-weight
  permissive или есть следы закрытых? Закрытые → `DATASET_REBUILD_REQUIRED` /
  a_side_style_only.
  > **Решение Luba 2026-06-29 (POLICY RESOLVED by authority + a_side_style confirm):** решения
  > Luba НЕ требуется — политика закрыта заданием (teacher только open-weight permissive;
  > closed = judge-only; closed+train ⇒ `DATASET_REBUILD_REQUIRED`). Подтверждена семантика
  > `a_side_style_only`: closed-origin better-output допустим ТОЛЬКО как A-side style/format
  > reference, `teacher_output_used_for_training=false`, НИКОГДА как chosen/rejected.
  > **Обновление (B-4 / R6-betteroutput — RESOLVED по input-контракту):** input-контракт
  > FLOW-38/Smart* больше **не** `NEW/path_missing` — он спроектирован построчно: NEW-record
  > `GeneratedUiOutputFeedbackEvent` (14 полей) + точка эмиссии (хост xiigen-platform,
  > `emission_creates_dpo_pair=false`) + declaration-entrypoint Вариант B (фасад
  > `IBetterOutputFeedbackFacade.SubmitFeedbackAsync` → `xiigen-flows` `Flow38BetterOutputSeam`
  > рядом с FLOW-31) + адаптер `Flow38FeedbackAdapter` (6-й входной формат →
  > `GraphDpoSourceRow` → `GraphDpoBuilder.BuildPairAsync` → гейт C16). Спроектировано в
  > `claude-plan-07 §8.3a` (владелец типа/адаптера) + `claude-plan-08 §8.7` (вердикт-
  > маршрутизация) + `claude-plan-14 §6.2/G13` (existence-status `NEW`-спроектирован).
  > Остаётся **execution**: эмпирическая per-dataset проверка фактического происхождения
  > реальных выходов (allowlist vs closed-следы) и прогон через гейт C16.
  > *(Источник: §E.0; задание Luba 2026-06-29; B-4.)*
- Q38. Лицензии XIIGen-клиентов и их выходов (R5): сгенерённый HTML+CSS
  (FigmaToCode/CanvaToCode/FLOW-31) и фидбек FLOW-38 — common-training или
  user-adaptive/private (возможно `ILearningMachinePair`)?
  > **Решение Luba 2026-06-29 (RESOLVED):** client-выходы (сгенерённый HTML+CSS
  > FigmaToCode/CanvaToCode/FLOW-31 + фидбек FLOW-38) по умолчанию `LegScope=PersonalAdaptive`
  > (QUARANTINE до положительного провенанса, ветка C16). Common-eligible ТОЛЬКО после
  > положительного клиренса лицензии/terms ВЫХОДА + redaction + явный approve Luba. Ввести
  > per-client terms-snapshot (по аналогии с teacher `terms_snapshot_ref`).
  > **Механика маршрутизации (B-4):** реализуется веткой C16 блока 08 над событием
  > `GeneratedUiOutputFeedbackEvent` (поле `SourceCategory=client_unverified` дефолт,
  > per-client `ClientTermsSnapshotRef`); адаптер `Flow38FeedbackAdapter` заполняет
  > provenance, вердикт-leg выносит гейт 08 (дефолт `PersonalAdaptive`/QUARANTINE;
  > `CommonGlobal` только после клиренса+approve). Спроектировано в `claude-plan-07 §8.3a`
  > + `claude-plan-08 §8.7`.
  > *(Источник: §E.0; задание Luba 2026-06-29; B-4.)*
- Q39. P-13: распаковывать ли отдельной задачей `extensions/plugin-plans-*.zip` ради
  Claude/Cursor/Codex-специфичных деталей хуков, или рекон `codex-recon-mvp-extensions.md`
  достаточен?
- Q40. CanvaToCode: держать как полноценный второй шов design→code наравне с FigmaToCode
  или как опциональный порт (тогда строки P-11/P-13 помечаются optional)?
- Q42 (T3-targetoutcome-flow38). Target-outcome и FLOW-38: план даёт «design complete» —
  считать ли это доказанной named-целью (реальное обучение до held-out 0.95)? И контур
  FLOW-38 better-output→обучение — он в дизайне «разорван» (severed circuit) или спроектирован?
  > **open → RESOLVED (T3-targetoutcome-flow38, 2026-07-01) — DESIGN, без git.** Оба под-вопроса
  > закрыты как **осознанная граница дизайна**, а не скрытый overclaim и не дефект.
  > **(a) Два exit-условия разведены ЯВНО:** план даёт РОВНО
  > `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (design-complete + честный not-run; реальный milestone,
  > но НЕ рабочая фича); `PASS_AS_TRAINED_FEATURE` (реальный прогон до held-out 0.95 + ablation
  > + fresh-load + continue-training + e2e) = **реализация**, вне plan-only (seed MISSING
  > §ADD-MASTER.1). Подача первого как второго = masquerade-FAIL (-1000, two_pass_conflation,
  > §B.9). **(b) FLOW-38 контур = DESIGN-COMPLETE, «severed circuit» СНЯТ:** common-path intake
  > инстанцирован построчно (`claude-plan-07 §8.3a`: 6-й формат `GeneratedUiOutputFeedbackEvent`
  > → `Flow38FeedbackAdapter` D1..D5 → `GraphDpoSourceRow` → `GraphDpoBuilder.BuildPairAsync` →
  > гейт 08 → leg); adaptive (Personal) нога честно **deferred** с ИНСТАНЦИРОВАННЫМ
  > `DeferredAdaptiveLegActivationBlueprint` (`ILearningMachinePair`, типы/update/lifecycle/
  > триггер). В v1 (common-only) обучающий эффект better-output = 0 — осознанная граница scope,
  > а не разорванная/недоспроектированная проводка. **Указатель:** мастер §ADD-MASTER.4
  > (владелец T3-решения) + `claude-plan-09 §ADD-09.9` (design-complete + deferred-блюпринт) +
  > `claude-plan-09 §ADD-09.10` (два exit-условия, Ф10.3.4 = 6-я исполнительная вещь) +
  > `claude-plan-14 §6.2/G13` (`boundary/API evidence != host readiness`). Соблюдены R4/R5/R6 и
  > RESOLVED «ровно 1 training-пилот language-translate». *(Источник: шаг починки
  > T3-targetoutcome-flow38, 2026-07-01.)*

## E.3. Сквозной вопрос охвата референсов (R15-scope)

**Q41 (R15-scope). Доп. референсы — навигационные ссылки или домены реализации?**

**КАНОН-набор R15 (единый закрытый список — устраняет расхождение трёх источников).** До
cycle-37 sync repair три управляющих артефакта давали **три разных** перечня R15-доменов
(мастер §E — 4, матрица §3.3 — 6, доска — 5). После RESOLVED(Luba 2026-06-30) канон-набор
**сведён к ОДНОМУ счётчику: 1 пилот + навигация**. `session-start` **убран** из обязательного
R15-набора (нет backing, см. RESOLVED-блок в конце §E.3-bis), поэтому обязательный
R15-доп.референс-набор v1 = **5 доменов** (1 пилот реализации + 4 навигация); `session-start`
держится отдельно как **conditional re-add** (возвращается только при появлении рекон-источника):

```text
R15-набор (обязательный v1) = { language-translate (пилот №1, Вариант B),
                                extensions, documentation-learning,
                                small-model, local-wiki }            // 5 доменов: 1 пилот + 4 навигация
R15-conditional (вне обязательного счётчика) = { session-start }     // re-add только при появлении рекон-источника
```

**Единый счётчик (anti-contradiction):** обязательных R15-доменов = **5** (1 пилот +
навигация), НЕ 6. Прежнее число «6» (с `session-start`) — stale до RESOLVED(Luba 2026-06-30)
и здесь снято; §E.3-bis «остальные 5 — навигация» и итог «1 пилот + навигация» — тот же
счётчик.

**Sync-заметка (binding):** claude-матрица `claude-REQUIREMENTS-COVERAGE-MATRIX.md`
(§3.3/§4.1) и claude-доска `claude-EXECUTION-CONTROL-BOARD.md` (§C строка R15) обязаны
использовать **этот же список дословно** (5 обязательных + 1 conditional `session-start`);
любое расхождение перечней по R15 = дефект (Contradiction Sweep). Бенчмарк-аналоги
`codex-*` — evidence-only, в binding-синхрон не входят.
Подтвердить статус этих доменов в плане v1 по двум вариантам:

- **Вариант A (ведущий, CANON-предложение):** это **навигационные ссылки** —
  evidence/доказательный слой (как реконы `codex-recon-mvp-*` и карта референсов блока
  14). Домен R-15 **covered ссылкой/навигацией** только тогда, когда у него есть
  конкретная навигационная привязка (точный `codex-recon-*` / блок P-NN / контракт
  `00.Core`). Домен без такой привязки — **НЕ covered**, а честно `evidence-only,
  навигационная цель отсутствует` (правило «удаление/ссылка ≠ покрытие замены»).
- **Вариант B:** это **домены реализации** (каждый несёт собственную обучаемую логику
  графа). Тогда требование R-15 по ним **partial** и нужны **новые подшаги в Ф10** доски
  (по подшагу на домен: контракт → DPO-строитель → legal-гейт → измерение →
  тест-стратегия FC-26 с раздельными колонками UH/TA), а матрица помечает их `partial` с
  gap-действием до закрытия.

**Ведущий ответ (CANON) — честный per-domain статус backing (согласовано с матрицей
§3.3):**

| Домен R15 | Backing-артефакт | Статус навигационной цели |
|---|---|---|
| `extensions` | `codex-recon-mvp-extensions.md`; блок 14 (Ф10) | **covered** ссылкой (рекон есть) |
| `language-translate` | `codex-recon-mvp-lang-translate.md`; блоки 10/11 (эталон) | **covered** ссылкой + **эталон** margin/lifecycle |
| `documentation-learning` | `codex-recon-mvp-doc-learning.md` | **covered** ссылкой (рекон есть) |
| `small-model` | `codex-recon-mvp-small-model.md` есть, но отдельной строки в карте блока 14 нет | **evidence-only**: рекон есть, навигационная цель в карте отсутствует |
| `local-wiki` | рекон `codex-recon-mvp-local-wiki.md` есть (read-only сверено 2026-06-23; рядом `codex-recon-core-42-wiki.md`/`codex-recon-core-43-localwiki.md`), но отдельной навигационной строки-цели в карте блока 14 нет — присутствует там только как graph-proto evidence под P-09 | **evidence-only**: рекон есть, навигационная строка-цель в карте блока 14 отсутствует (причина — отсутствие строки-цели, НЕ отсутствие рекона; симметрично `small-model`) |
| `session-start` | `codex-recon-*` НЕТ; строки в библиотеках/карте НЕТ | **evidence-only, навигационная цель отсутствует**; предусловие закрытия: назвать недостающий рекон-источник **или** убрать из R15-набора по решению Luba |

**Эталон (отдельно от covered-статуса):** `language-translate` — эталон margin/lifecycle
для всего графа: реальный `RewardMargin` берётся из движка (`11.DPO/DpoTrainer.cs`), а не
из литерала (R-19, блоки 10/11), lifecycle (checkpoint/export/import/continue-training,
held-out+ablation+fresh-load) калибруется по уже доказанному пути перевода. Если позже
Luba выберет Вариант B хотя бы для одного домена, он перестаёт быть covered/evidence-
ссылкой и получает собственные подшаги Ф10 + строку `partial` в матрице. *(Источник:
R-15, блок 14; эталон — R-19, блоки 10/11; согласовано с матрицей §D.2 и доской §D.1.)*

### E.3-bis. РЕКОМЕНДУЕМЫЙ ДЕФОЛТ по каждому reference-домену (6 строк = 5 обязательных v1 + 1 conditional `session-start`; R11-domains-scope, B-8 / B-20, RESOLVED-как-проект)

> **Счётчик (anti-contradiction):** таблица ниже описывает **6 строк диспозиции**, но
> обязательных R15-доменов v1 — **5** (1 пилот + 4 навигация, §E.3 head); 6-я строка
> `session-start` = conditional re-add (вне обязательного счётчика). Это тот же единый
> счётчик «1 пилот + навигация», не второе число.

> **Человеческим языком (RU).** До этого шага Q41 честно давал ДВЕ опции (A = навигация,
> B = домен реализации), но **не называл рекомендуемый дефолт для каждого домена** —
> это и была дыра R11-domains-scope (B-8/B-20). Закрываю её: для **каждой строки** диспозиции
> даю конкретную рекомендацию — «навигация» (Вариант A) или «домен реализации» (Вариант B
> с подшагами Ф10) — с обоснованием. **Это РЕКОМЕНДАЦИЯ плана, а не факт:** объём доменов
> v1 — продуктовый выбор Luba (бюджет/приоритет), поэтому весь набор изначально был помечен
> `residual_for_luba`. **СТАТУС ОБНОВЛЁН — RESOLVED(Luba 2026-06-30):** объём v1 залочен =
> РОВНО 1 АКТИВНЫЙ пилот `language-translate` (активная baseline-подфаза Ф10.3, входит в
> baseline-64, НЕ blocked резерв; RR4-pilot-activation снят); остальные 5 = навигация/отложены;
> `session-start` убран из R15-набора (см. RESOLVED-блок в конце §E.3-bis).

**Критерий выбора дефолта (плоско, без жаргона).** Домен рекомендуется как **домен
реализации (Вариант B)** только если у него есть всё одновременно: (1) свой рекон-источник
на диске (навигационная цель достижима), (2) уже доказанный lifecycle-эталон в проекте
(переиспользуемый margin/checkpoint/held-out), (3) ясная графовая семантика «разного
порядка» для DPO-строителя. Иначе домен рекомендуется как **навигация (Вариант A)** —
честная ссылка/evidence, без раздувания baseline-доски (в v1 в baseline входит только
1 активный пилот Ф10.3 `language-translate`). Это правило «удаление/ссылка ≠
покрытие замены»: домен без backing не объявляется covered.

| # | Домен R15 | Backing (есть/нет) | **Рекомендуемый дефолт v1** | Обоснование (1 фраза) |
|---|---|---|---|---|
| 1 | `language-translate` | рекон + **доказанный эталон** margin/lifecycle (блоки 10/11) | **B — домен реализации (пилот №1)** | единственный с реальным lifecycle-эталоном → им калибруется весь граф; идеальный первый пилот Ф10.3 |
| 2 | `extensions` | рекон `codex-recon-mvp-extensions.md` есть | **A — навигация в v1** (B — кандидат №2 при approve) | рекон есть, но lifecycle-эталона нет; графовая семантика хуков сложна → отложить реализацию до пилота №1 |
| 3 | `documentation-learning` | рекон `codex-recon-mvp-doc-learning.md` есть | **A — навигация в v1** (B — кандидат №3 при approve) | рекон есть, но нет доказанного margin/checkpoint → covered ссылкой, реализация позже |
| 4 | `small-model` | рекон есть, **навигационной строки-цели в карте блока 14 НЕТ** | **A — навигация**, предусловие: добавить строку-цель в карту блока 14 | backing-файл есть, но навигационная цель не дотянута → сначала дотянуть ссылку, потом думать о B |
| 5 | `local-wiki` | рекон `codex-recon-mvp-local-wiki.md` есть, но в карте только graph-proto evidence под P-09 | **A — навигация**, предусловие: выделить строку-цель отдельного домена | как small-model: файл есть, отдельная навигационная цель отсутствует |
| 6 | `session-start` | **рекон НЕТ; строк в картах/библиотеках НЕТ** | **A — навигация, но cold**: либо назвать недостающий рекон-источник, либо **убрать из R15-набора** | нет ни backing-файла, ни навигационной цели → честный `evidence-only, цель отсутствует`; реализация невозможна без источника |

**Итог (RESOLVED 2026-06-30 — продуктовый выбор Luba залочен):** **РОВНО 1 АКТИВНЫЙ пилот
реализации** = `language-translate` (Ф10.3, подшаги Ф10.3.1…Ф10.3.5, Вариант B). Пилот —
**активная baseline-подфаза** в доске исполнения (статус `remaining`, план-only; откроется
после approve, как любая baseline-фаза), а **НЕ** `blocked`/scope-gated резерв: решение Luba
Q41=Вариант B уже принято, поэтому держать пилот `blocked` было бы прямым противоречием
RESOLVED-состоянию (дефект RR4-pilot-activation, снят). **Остальные 5 — навигация (Вариант A,
reference-only, НЕ подшаги доски):** extensions/documentation-learning covered ссылкой;
small-model/local-wiki evidence-only (предусловие — дотянуть строку-цель в карту блока 14);
session-start убран из обязательного R15-набора (conditional re-add). Активация единственного
RESOLVED-пилота пересчитывает канон-счётчики с 12/26/59 на **12 фаз / 27 подфаз / 64 подшага**
(Ф10 → 3 подфазы / 9 подшагов; согласовано с доской `claude-EXECUTION-CONTROL-BOARD.md` §1 и
`claude-plan-14` §6.3/G10). Прежняя формула «0 доменов реализации в baseline-59 + 6
blocked-слотов вне 59» относилась к снятой схеме (до RESOLVED) и здесь устранена.

**Каноническая 5-шаговая структура домена реализации Ф10.3 (подшаг на домен, B-20).**
Эта структура УЖЕ применена к активному пилоту `language-translate` (Ф10.3.1…Ф10.3.5, входит
в baseline-64). Для БУДУЩИХ дополнительных кандидатов (Вариант B для ещё одного домена D, напр.
`extensions` как пилот №2) активация добавляет ОДНУ новую подфазу `Ф10.<k>` со **строго пятью**
подшагами по тому же шаблону (это deferred-to-later-phase, требует отдельного approve Luba; до
такого approve кандидат остаётся навигацией, не подшагом доски):

```text
Ф10.3.k.1  контракт домена D       → назвать DomainTag="<D>Domain", I/O узла/ребра графа D
Ф10.3.k.2  DPO-строитель домена D   → GraphDpoBuilder.BuildPairAsync на корпусе D (chosen/rejected пути D)
Ф10.3.k.3  legal-гейт домена D      → TeacherProvenanceGateUnit + ветка C16 (LegScope) над строками D
Ф10.3.k.4  измерение домена D       → held-out source-disjoint + ablation ≥0.02 (Q11) + Q30-триада (0.95/+0.02/0.025)
Ф10.3.k.5  тест-стратегия FC-26 D   → 9 слоёв + РАЗДЕЛЬНЫЕ колонки UH (PASS_AS_HONEST_UNTRAINED_SCAFFOLD) / TA (PASS_AS_TRAINED_FEATURE)
```

Активный пилот `language-translate` → R-15 помечена `partial` (scope RESOLVED, но pilot
execution evidence ещё не реализован) до закрытия его подшагов Ф10.3.1…Ф10.3.5; каждый будущий
активированный домен-кандидат → матрица помечает его R-15-строку `partial` с gap-действием до
закрытия его 5 подшагов; пока approve кандидата нет — он остаётся covered-ссылкой или
evidence-only (статус-таблица выше).

> **RESOLVED(Luba 2026-06-30) (продуктовый выбор Luba, залочен):**
> (1) **Объём v1** = **РОВНО 1 активный пилот реализации** = `language-translate` (Вариант B,
>     подшаги Ф10.3.1…Ф10.3.5) — **активная baseline-подфаза**, входит в baseline-64 (НЕ
>     blocked/scope-gated резерв; RR4-pilot-activation снят); остальные 5 доменов = навигация
>     (Вариант A, reference-only) / отложены.
> (2) **Порядок пилотов** при будущем Варианте B = `language-translate` (пилот №1) → extensions →
>     documentation-learning; small-model/local-wiki = только после дотягивания строки-цели
>     (deferred-to-later-phase); `session-start` — только после появления рекон-источника.
> (3) **session-start** = **убран из обязательного R15-набора** (нет backing); возвращается
>     только если соберётся рекон-источник (deferred-to-later-phase).
> *(Источник: шаг починки R11-domains-scope / B-8 / B-20; владелец канона R15 — мастер §E.3
> + `claude-plan-14` §6.3; согласовано с матрицей §D.2 и доской §D.1; решение Luba 2026-06-30.)*

---

---

## §ADD-MASTER — Additive: consolidated E2E precondition gates для reference-доменов

> **Additive**: собирает разрозненные предусловия (мастер §E.3-bis + шаблон Ф10.\<k\> +
> plan-08 legal) в единые up-front блокер-ряды per-domain; прежние §E-решения не меняются.

### §ADD-MASTER.1 — Seed-Corpus Existence Proof Gate (закрывает BLOCKING/HIGH: seed «рекомендуемый дефолт» без EXISTS)

**Правило (LOCKED-add):** любой домен, чей E2E-исход зависит от seed-корпуса learning-sets,
несёт пометку EXISTS/MISSING на диске (как SR-00..SR-10). Рекон-backing
(`codex-recon-mvp-<domain>.md`) `!=` data-backing. MISSING seed-корпус = явный
design/dependency-блокер-ряд (авторинг датасета), НЕ молчаливое допущение, НЕ «0 .cs», НЕ terms-review.

```text
Seed-статус на 2026-07-01 (мастер §E.0 «данных пока нет» → конкретизирован):
  language-translate : 27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl → PLAN-SHAPE RESOLVED / FILE MISSING → execution-proof-pending (авторинг датасета)
  small-model        : training-data/small-model/learning-sets/*.jsonl        → MISSING → BLOCKING dep
  extensions         : training-data/extensions/learning-sets/*.jsonl         → MISSING → BLOCKING dep
  figma-html-css     : training-data/figma-html-css/learning-sets/*.jsonl     → MISSING → BLOCKING dep + вне R15-scope
Следствие: E2E-исход каждого = недостижим в v1 до авторинга датасета. Петля честно → no_seed_corpus.
```

Pass10/active-codex reconciliation: the `language-translate` file is still not
created, so execution proof remains pending, but the v1 source-data contract is
not open. Current active pilot seed contract is `SeedCorpus-LanguageTranslate-v1`,
self-authored synthetic local JSONL at future path
`llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl`.
Minimum row fields: `seed_id`, `source_language`, `target_language`, `input_text`,
`chosen_translation`, `rejected_translation`, `wrong_marker` or `rejected_reason`,
`split=train|held_out`, `source_category=synthetic_self_authored`,
`license=project_internal_self_authored`, `TeacherProvenance` or provenance refs,
and `semantic_quality_claimed=false`. Train rows use only `split=train`;
measurement uses only `split=held_out`; missing legal/provenance/source fields
reject or quarantine the row before train-ready. Unit-level DPO only; all
non-`language-translate` domains remain navigation/reference-only unless Luba
explicitly approves a later active pilot.

### §ADD-MASTER.2 — Reference-domain E2E precondition consolidation gate (закрывает HIGH: предусловия разрознены §E.3-bis:1758 + шаблон:1785)

**Правило (LOCKED-add):** когда фокус-исход = «E2E цикл домена D» для ЛЮБОГО домена R15-набора,
план либо содержит инстанцированный per-домен blueprint, либо помечает единым up-front блокер-рядом.
Единый bundle per-domain (собран в ОДНОМ месте, не разрозненно):

```text
DomainActivationBlueprint(D) := {
  (1) seed-корпус путь + EXISTS/MISSING        (§ADD-MASTER.1 / §ADD-09.3)
  (2) per-домен accuracy-функция + gold        (§ADD-09.4 / §ADD-11.4)
  (3) активация Варианта B: строка-цель блок 14 (§ADD-14) + подшаги Ф10.<k>.1-5 (шаблон :1785)
  (4) lifecycle-эталон (user/cache→repo/common→backup/export/import→continue-training)
  (5) графовая семантика (узел/ребро/порядок для D)
  (6) для client-output доменов (figma-html-css): снятие legal-QUARANTINE (approve Luba, plan-08:1396)
}
Статус bundle:
  language-translate : (1) shape/path resolved as SeedCorpus-LanguageTranslate-v1 but file authoring/proof pending; (2)(3)(5) инстанцированы (§ADD-09.4/§ADD-09.10); (4) plan-10.
  small-model/extensions : bundle ПУСТ по (1)(2)(3) → «E2E D недостижим в v1»; предусловия = (1)+(2)+(3).
  figma-html-css : вне R15-scope + bundle ПУСТ + (6) QUARANTINE → «E2E D недостижим в v1»; предусловия = (1)+(2)+(3)+(6)+добавить домен в scope.
```

### §ADD-MASTER.3 — Domain-Scope-vs-Focus Reconciliation (закрывает HIGH: figma-html-css вне R15-scope, но E2E требуется)

**Правило (LOCKED-add):** когда фокус требует E2E для домена D, ПЕРВЫЙ шаг — сверить D со
scope-таблицей §E.3 (6 R15-доменов). Если D отсутствует (как `figma-html-css` — он только host-шов
R16 SR-16/SR-11, НЕ в списке 6 R15-доменов), явно записать «D вне domain-scope: E2E auto-DPO НЕ
обещан; требуется up-front блокер-ряд активации (§ADD-MASTER.2 предусловия)» ДО симуляции шагов.

> **F2-sr16-disambig (RESOLVED 2026-07-01) — разведение id `SR-16` в `claude-plan-14`.** «SR-16»
> здесь и в §ADD-MASTER.2 (:1910 «строка-цель блок 14 §ADD-14») означает **in-repo trainable/lifecycle
> backing R16 `44.DesignConversionDomain`** (`claude-plan-14 §ADD-14.1`, EXISTS in-repo), НЕ seed-корпус.
> В `claude-plan-14` id `SR-16` раньше ошибочно нёс ДВА объекта; коллизия устранена: **SR-16 =
> DesignConversion backing**, **SR-17 = seed input-prompt корпус пилота `language-translate`** (владелец
> блюпринта — `claude-plan-09 §ADD-09.3-bis`). Ссылки §E на seed-корпус остаются по имени
> §ADD-09.3-bis / пути `language-translate-seed-v1.jsonl` (SR-17; старый `seed-input-prompts.jsonl` = historical/pre-Pass10 alias), а не по id `SR-16`. Инвариант «один
> SR-NN = один объект» восстановлен по всем трём файлам (plan-14, мастер §E, plan-09).

### §ADD-MASTER.3-bis — T2-figma-scope: figma→html+css / CanvaToCode = КЛИЕНТ-of-API, НЕ v1 training-домен (SCOPE-решение, не coverage-дыра) — RESOLVED (2026-07-01)

**Человеческим языком (RU).** Прежняя формулировка §ADD-MASTER.3 оставляла `figma-html-css`
двусмысленным: он был помечен `NOT_COVERED / вне R15-scope`, но одновременно «E2E требуется» →
это читалось как **coverage-дыра**, для закрытия которой якобы нужен «up-front блокер-ряд
активации домена» (seed-корпус figma + подшаги Ф10.<k> + снятие legal-QUARANTINE). Это была
**мис-классификация**: она молча превращала клиентский сценарий в кандидат-training-домен v1,
что прямо противоречит RESOLVED(Luba 2026-06-30) «v1 training-пилот = **РОВНО 1** =
`language-translate`» (§E.3-bis). T2-figma-scope это исправляет **решением о scope**, а не
заполнением дыры.

**Решение (SCOPE, LOCKED-add).** `figma→html+css` (FigmaToCode `controller.ts`) и `CanvaToCode`
(`canva-adapter.ts`) — это **КЛИЕНТСКИЕ сценарии-потребители API** (`client-of-API`): клиент
берёт дизайн, зовёт публичный API-хост (`IDesignToCodeFacade.AnalyzeDesignAsync` для design +
`IBetterOutputFeedbackFacade.SubmitFeedbackAsync` для better-output-фидбэка) и получает
конкретный ответ. Они **НЕ** являются **v1 auto-DPO training-доменом** и **НЕ** несут
собственной петли обучения графа в v1. Значит:

```text
Классификация (LOCKED-add, T2-figma-scope):
  figma-html-css / CanvaToCode  = client-of-API scenario (вызывает better-output/design API)
                                != v1 auto-DPO training-domain
  v1 auto-DPO training-pilot    = ТОЛЬКО language-translate (Ф10.3, §E.3-bis, RESOLVED 2026-06-30)

Следствие 1 (scope, НЕ coverage-дыра):
  «figma E2E» = out-of-v1-training-scope client-integration (навигация/отложено).
  Это НЕ NOT_COVERED-дыра плана auto-DPO: домен намеренно ВНЕ v1 training-scope.
  Прежнее «требуется up-front блокер-ряд активации домена (seed figma + Ф10.<k> + снятие
  QUARANTINE)» СНЯТО как мис-классификация — это активировало бы 2-й training-домен вопреки
  RESOLVED «ровно 1 пилот».

Следствие 2 (как доказывается «coverage» клиента):
  «coverage» figma/canva доказывается через API-КОНТРАКТ better-output/design
  (Path A boundary: request→response→trace по §6.1а/§6.2 фасадам), а НЕ через собственную
  петлю обучения figma. Клиент — потребитель ответа API; ядро (llm_mvp_core) владеет логикой,
  клиент тонкий (R6). Обучается (auto-DPO) в v1 ТОЛЬКО пилот language-translate.

Следствие 3 (§ADD-MASTER.1 seed-строка figma-html-css переклассифицирована):
  строка `figma-html-css : training-data/.../learning-sets/*.jsonl → MISSING → BLOCKING dep`
  БОЛЬШЕ НЕ «BLOCKING dep для v1»: у client-of-API сценария НЕТ собственного v1 seed-training
  корпуса, потому что он не тренируется в v1. seed остаётся MISSING и релевантен ТОЛЬКО если
  Luba позже явно активирует figma как training-домен (deferred, отдельный approve).

Будущее (deferred, требует явного approve Luba):
  если Luba позже захочет, чтобы САМ figma-шов обучался как отдельный training-домен D,
  тогда — и только тогда — применяется полный DomainActivationBlueprint(D) (§ADD-MASTER.2):
  seed-корпус figma + подшаги Ф10.<k>.1-5 + legal-QUARANTINE снятие. До этого approve
  figma/canva остаются client-of-API (навигация), а НЕ training-домен.
```

**Где зафиксировано:** этот §ADD-MASTER.3-bis (владелец scope-решения) + Q36 §E-леджер (ниже,
open→RESOLVED T2-figma-scope) + `claude-plan-14` §6.3 (R15-раздел, client-of-API нота) +
`claude-plan-14` §6.1/§6.4 (dual-path figma-сценарии = client-host, доказываются API-контрактом,
а не training-петлёй). Соблюдён R6 (тонкий клиент, ядро в `llm_mvp_core`) и RESOLVED «ровно 1
training-пилот language-translate». *(Источник: шаг починки T2-figma-scope, 2026-07-01.)*

### §ADD-MASTER.4 — T3-targetoutcome-flow38: два exit-условия как ОСОЗНАННАЯ граница дизайна + FLOW-38 контур DESIGN-COMPLETE (не «severed circuit») — RESOLVED (2026-07-01)

**Человеческим языком (RU).** Два прежних чтения давали ложный сигнал. (1) Target-outcome:
план мог прочитаться так, будто «design complete» = достигнутая named-цель (реальное обучение
до held-out 0.95) — это был бы **скрытый overclaim**. (2) FLOW-38: §ADD-09.9 читался как
**«severed circuit»** — будто контур better-output→обучение в дизайне разорван. T3-репарация
закрывает оба как **DESIGN** (не execution) — явно, без overclaim.

**Решение (a) — два exit-условия разведены явно (LOCKED-add, T3).** Это осознанная граница, а
не скрытый пробел (CLAUDE.md «Untrained honesty vs trained readiness», OP-56/OP-57):

```text
EXIT-1 = PASS_AS_HONEST_UNTRAINED_SCAFFOLD  → это ДАЁТ план (design-complete, честный not-run).
         Реальный milestone: контракты+DPO-строитель+legal-гейт+intake+тест-стратегия
         спроектированы; узел честно not_ready; нет скрытого детерминизма (R4/A-LM-09);
         честность доказуема UH-тестами. НЕ рабочая фича, обучения НЕ было.
EXIT-2 = PASS_AS_TRAINED_FEATURE            → это = РЕАЛИЗАЦИЯ (execution, вне плана).
         Требует реального прогона до held-out 0.95 НА ДИСКЕ + ablation + fresh-load +
         continue-training + e2e. Сейчас недостижимо plan-only (seed MISSING §ADD-MASTER.1).
Граница : план даёт РОВНО EXIT-1; EXIT-2 = execution. Подача EXIT-1 как EXIT-2 =
         masquerade-FAIL (-1000, two_pass_conflation, §B.9). Владелец детали — claude-plan-09
         §ADD-09.10 (Ф10.3.4 = 6-я исполнительная вещь: реальный прогон, не «написана стратегия»).
```

**Решение (b) — FLOW-38 контур DESIGN-COMPLETE, «severed circuit» снят (LOCKED-add, T3).**
Разведены полнота ДИЗАЙНА контура и факт ИСПОЛНЕНИЯ прогона:

```text
Полнота дизайна (Объект 1): common-path intake ИНСТАНЦИРОВАН построчно —
  claude-plan-07 §8.3a: 6-й формат GeneratedUiOutputFeedbackEvent (14 полей) →
  Flow38FeedbackAdapter (D1..D5: гард/редакция/маппинг/provenance/маршрутизация) →
  GraphDpoSourceRow → GraphDpoBuilder.BuildPairAsync → гейт 08 (C2/C3/C9/C16) → leg-вердикт.
  Ни одно звено common-пути НЕ «path_missing по дизайну» ⇒ контур DESIGN-COMPLETE.
Adaptive-нога (Personal): DEFERRED, но с ИНСТАНЦИРОВАННЫМ блюпринтом активации
  (claude-plan-09 §ADD-09.9 DeferredAdaptiveLegActivationBlueprint: ILearningMachinePair,
  типы Personal/Global-checkpoint, update-алго, lifecycle, триггер = approve Luba).
Факт исполнения (Объект 2): v1 = common-only (мастер:1607); дефолт FLOW-38 = C16
  PersonalAdaptive (мастер:1601) ⇒ в v1 нога НЕ активна; обучающий эффект = 0 (осознанно).
  Предусловия ненулевого эффекта (execution-долг): (a) активация Personal-adaptive ноги ИЛИ
  клиренс+approve каждой строки в CommonGlobal; (b) `better_output` в GraphLoopInput.source;
  (c) per-домен scorer design→code (§ADD-09.4). До этого FLOW-38 v1 = Path A boundary-evidence.
Вывод: контур НЕ разорван и НЕ недоспроектирован — это осознанная граница v1-scope (common-only).
```

**Где зафиксировано:** этот §ADD-MASTER.4 (владелец T3-решения) + `claude-plan-09 §ADD-09.9`
(FLOW-38 design-complete + DeferredAdaptiveLegActivationBlueprint) + `claude-plan-09 §ADD-09.10`
(два exit-условия, Ф10.3.4 6-я исполнительная вещь) + `claude-plan-07 §8.3a` (intake инстанцирован)
+ `claude-plan-14 §6.2/G13` (existence-status `NEW`-спроектирован, `boundary/API evidence != host
readiness`) + §E Q37/Q42 (open→RESOLVED). Соблюдены R4 (no-hidden-determinism), R5/R6, RESOLVED
«ровно 1 training-пилот language-translate». *(Источник: шаг починки T3-targetoutcome-flow38, 2026-07-01.)*

---

*Конец улучшенного мастер-плана ILearningDynamicGraph v1 (namespace claude-*). PLAN-ONLY:
документация-решение, индексирующая 16 логических блоков (15 файлов `codex-plan-*` как
evidence-источник; §F индекс артефактов). Источник истины по execution-control —
claude-доска `claude-EXECUTION-CONTROL-BOARD.md` (`Ф<P>.<SP>.<SS>`, §D); трассировка
требований — claude-матрица `claude-REQUIREMENTS-COVERAGE-MATRIX.md` (§D.2). Бенчмарк
`codex-*` — evidence-only. Не разрешает реализацию
продукта/кода, тренировку, билд, запуск раннеров, git или вызов внешних моделей.
Реализация по доске исполнения требует свежего явного approve Luba после ревью этого
плана (R2). Цитаты кода — только реально существующие, с `file:line`, сверены прямым
чтением `C:/Projects/llm_mvp_core`. NEW-типы помечены; существующие — с `file:line`. Чужие
`codex-*`/`dcm-*` и старый каркас `claude-00..07` — evidence-only.*
