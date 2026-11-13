---
artifact_type: requirements_coverage_matrix
authority_source: latest unquoted Luba work order (build my claude-* ILearningDynamicGraph plan, using codex-* as benchmark, PLAN-ONLY)
status: plan_only_claude_owned
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
benchmark_source_read_only: codex-REQUIREMENTS-COVERAGE-MATRIX.md (+ codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md, codex-plan-04/07/08/12, codex-EXECUTION-CONTROL-BOARD.md)
---

# Матрица покрытия требований — ILearningDynamicGraph (версия Claude)

Артефакт: `claude-REQUIREMENTS-COVERAGE-MATRIX.md`. Статус: ТОЛЬКО ПЛАН, владелец — Claude.
Это улучшенная версия матрицы Codex в моём namespace `claude-*`. Все `codex-*` файлы и старый
каркас `claude-00..07` прочитаны как бенчмарк/evidence (только чтение) и НЕ редактировались.

> Зачем нужен этот документ (человеческим языком). Это «карта обещаний»: один список того,
> что Luba потребовала от системы `ILearningDynamicGraph`, и напротив каждого требования —
> где оно решается (какая фаза и какой план-блок), каким Gate B (точное проверяемое условие)
> оно закрывается, и насколько честно оно покрыто **на уровне плана** (не на уровне кода —
> код ещё не разрешён). Матрица не утверждает, что что-либо реализовано, обучено или
> проверено в рантайме. Она утверждает только полноту и согласованность планирования.

---

## 0. Якорь задачи (Current task anchor)

```text
CURRENT TASK:
Построить мою (claude-*) улучшенную матрицу покрытия требований ILearningDynamicGraph,
читая codex-* как бенчмарк. Только план. Редактируемый namespace — только claude-*.md
в папке "dpo legal build".

NOT THE TASK:
Не редактировать и не создавать ни один codex-* файл (план Codex, codex-recon-*,
codex-50-cycle-*, codex-skill-improvement-*). Не писать/менять код, билд, тесты. Не git.
Не использовать внешние AI/модели. Не продолжать и не «чинить» рабочие артефакты Codex —
они только бенчмарк. Не выдавать ledger-строки за реальные циклы ревью. Не заявлять
реализацию/обучение/рантайм как сделанные.

EVIDENCE-ONLY:
Все codex-*.md (включая codex-REQUIREMENTS-COVERAGE-MATRIX.md, codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md,
codex-plan-00..14, codex-EXECUTION-CONTROL-BOARD.md, codex-BENCHMARK-DELTA-REPORT.md,
codex-GPT55-50-CYCLE-REVIEW-LEDGER.md, codex-recon-*), старый каркас claude-00..07,
прикреплённые тексты, старые пакеты суб-агентов. Это материал для чтения; он не создаёт
полномочий на исполнение, git, тесты, билды, обучение или расширение объёма.
```

```text
CURRENT_UNQUOTED_LUBA_INSTRUCTION = построить claude-REQUIREMENTS-COVERAGE-MATRIX.md как улучшенную версию, читая codex-* как бенчмарк; PLAN-ONLY; редактировать только claude-*
QUOTE_DELIMITER_PARSE: detected_quote_marker=none (current instruction unquoted); quoted_span_status=evidence_text_only; may_execute_from_quote=false
ATTACHMENT_QUOTE_PARSE: detected_attachment_quote_marker=none; allowed_use=benchmark_only; may_execute_from_attachment=false
CURRENT_TASK_DERIVATION: current_task_source=current_unquoted_luba_message; quoted_context_used_as_task_source=false; evidence_text_created_subrequirement=false
implementation_authorized = false
```

---

## 1. CANON-инварианты (сохранены без изменений)

Эти инварианты — общая истина для всех `claude-*` блоков и совпадают с каноном Codex
(в этом и состоит требование консистентности). Я их не ослабляю и не переопределяю.

| Инвариант | Человеческое значение | Источник-привязка (file:line) |
|---|---|---|
| **A-LM-12** — закрытый набор из 5 уровней обучения | `ILearningDynamicGraph` = специализация L5 `ILearningMachine<DynamicGraphDomain>`, а **не** новый 6-й уровень. 5 разрешённых уровней: `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair`. | `docs/DECISIONS-LOCKED.md:35`; контракт L5 `00.Core/LearningMachineContracts.cs:208` |
| **A-LM-02** — `Train*` только на `ILearningMachine` | Узел/поток/оркестратор — это чистый inference; обучение живёт только на машине L5. | `docs/DECISIONS-LOCKED.md:25` (см. также CLAUDE.md «Separation rule A-LM-02») |
| **A-LM-09** — финал ребра принимает обученный узел | На каждом ребре финальную authority выбора принимает обученный `EdgeAuthorityUnit` (один вход `EdgeAuthorityInput` → один выход `EdgeAuthorityDecision`), а не детерминированная формула. | `docs/DECISIONS-LOCKED.md:32` |
| **A-LM-13** — нет скрытого детерминизма под видом «семантики» | Если код/доки называют механизм «семантическим юнитом», он обязан быть обученным с lifecycle-доказательствами, иначе claim понижается. | `docs/DECISIONS-LOCKED.md:36` |
| **Легальный teacher-гейт** | Teacher для дистилляции/обучения — только open-weight permissive. Closed (Claude/OpenAI/Gemini) — только **судья** при `used_for_training=false`. `closed_proprietary_model=true && used_for_training=true ⇒ DATASET_REBUILD_REQUIRED`. Контаминация (`sonnet-*`) ⇒ `DATASET_REBUILD`. | `codex-plan-08`; CLAUDE.md «DPO/distillation legal teacher provenance review gate» |

> Почему это важно прямо в матрице. Матрица — это место, где легче всего «срезать угол»:
> назвать детерминированный роутер «обученным юнитом» и поставить `covered`. Инварианты
> выше — это запрет такого среза. Любая строка ниже, которая претендует на обученность,
> обязана опираться на A-LM-09/12/13 и легальный гейт, иначе её честный статус — `partial`.

---

## 2. Канонические контракты (общий словарь, синхронен с claude-06 и codex-каноном)

Эти имена типов одинаковы во всех моих `claude-*` блоках и в бенчмарке Codex. Я их фиксирую
здесь как единый словарь, чтобы строки требований ссылались на точные имена, а не на описания.

| # | Канон-тип | Что это (человеческим языком) | Уровень | Владелец-блок |
|---|---|---|---|---|
| 1 | `IDynamicGraphNode` | Обёртка над узлом-`ILogicalUnit`; **без** `Train*` (A-LM-02). | L2 | план-01 |
| 2 | `IDynamicGraphEdge` (+`GraphEdgeKind`) | Типизированное ребро с per-edge обучаемой authority и анти-leakage полями. | L3 | план-02 |
| 3 | `GraphEdgeKind` (enum, ровно 5) | Виды ребра: `FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop`. | — | план-02 (канон §A.4.1) |
| 4 | `EdgeAuthorityUnit` (`EdgeAuthorityInput`→`EdgeAuthorityDecision`) | Обучаемый финальный решатель ребра; заменяет baseline-роутер по cutover. | L2-юнит | план-02 |
| 5 | `DynamicGraphTopology` (+`BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)`) | Мутируемая на лету карта узлов/рёбер; строится обученной логикой, **не** матрицей смежности. | — | план-03/04 |
| 6 | `DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>` | Владеет обучением, оценкой, export/import и постройкой рантайм-оркестратора. | L5 | план-04 |
| 7 | `DynamicGraphMachinePair : ILearningMachinePair<TPersonal,TGlobal>` | Разделяет общую (llm_mvp_core) и адаптивную (у пользователя) ноги — SPLIT R5. | L1-пара | план-04/10 |
| 8 | `GraphDpoBuilder.BuildPairAsync` → `GraphDpoPair` | Авто-строитель: из сравнения прогонов графа делает легальную DPO-пара только при тройном гейте. | — | план-07 |
| 9 | `GraphDpoPair` (канон-поля) | `pair_id; level(unit\|edge\|graph); node_or_edge_id; input_context; chosen; rejected; wrong_markers([WRONG:...]); reason; teacher_provenance{...}; source_ids[]; held_out; candidate_order_invariant; excluded_leakage_signals[]; dpo_train_ready(gate)`. | — | план-07 (владелец канона) |
| 10 | `TeacherProvenance` (20 полей) | 13 базовых + 5 NEW + 2 RR6 (`service_mode`/`grounding_used`, обязательны). См. §3 ниже. Предусловие `dpo_train_ready` — `provenance_field_presence_rate==1.0` по всем 20 (пустые RR6 ⇒ не train-ready, R4). | — | план-08 (владелец канона) |
| 11 | `DynamicGraphLegalGate` | Проверяет источник, модель/судью и provider-terms до входа строк в train/held-out. | — | план-08 |
| 12 | `GeneratedUiOutputFeedbackEvent` | Единственный вход для FLOW-38 и UI-фидбэка в DPO. | — | план-07 |

> При любом расхождении числа полей или размещения `judge_model` между этой матрицей и
> блоком 08 — **владелец канона — блок 08** (план-08). Эта матрица не переопределяет канон,
> она его трассирует.

---

## 3. Канон `TeacherProvenance` — 20 полей (владелец: план-08)

13 базовых: `teacher_access_mode; hosting_provider; model_id; model_license; model_version;
request_id; provider_terms_checked; teacher_output_used_for_training; closed_proprietary_model;
source_ref; source_sha256; contamination_markers; judge_model` (поле `judge_model` —
**внутри** `TeacherProvenance`, не отдельным верхнеуровневым полем).
5 NEW (бенчмарк-дельты T2/T3/T5): `terms_snapshot_ref` (T2); `geography_export_constraint` (T2);
`source_category` (T5); `raw_private_text_copied` (T3); `redaction_policy_ref` (T3).
2 RR6 (service-mode ban / grounding — **обязательны**): `service_mode` (RR6: для train обязан быть
`"plain"`, C17); `grounding_used` (RR6: bool, для train обязан быть `false`, C17). Пустые RR6-поля ⇒
`provenance_field_presence_rate<1.0` ⇒ строка НЕ train-ready (закрывает loophole R4).

`SourceCategory` (8 значений): local, human, open_weight, hosted, deterministic_baseline,
self_judge, client_unverified, third_party.
`LegScope` (4 значения): CommonGlobal, PersonalAdaptive, EvalOnly, Quarantine.
Вердикт-enum — **ровно 6 значений** (выровнено с владельцем канона план-08 §3.3):
`ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY`.
`EVAL_ONLY` — **полноправный** член канона (фикс R17: легально-чистая, но train-ineligible
строка из-за неполной триады C15/C15b или невоспроизводимого конверта C8), **а не только
«операционное» значение гейта**. (`BLOCK_FOR_HOSTED` остаётся внутренним операционным
значением гейта план-08 и не входит в канон-enum строки `GraphDpoPair`.)

---

## 4. Нумерация фаз (канонические счётчики сохранены)

```text
schema = Ф<P>.<SP>.<SS>   (Фаза.ПодФаза.ПодШаг)
baseline_phases    = 12   (Ф0…Ф11)
baseline_subphases = 26
baseline_substeps  = 59
phase4_3_numbering = Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split   (чисто 1..3, без .0)
reserve_rows = Ф10.3.1..Ф10.3.6  (вне baseline-59, заблокированы до приёмки Q41 Variant B)
```

| Фаза | Человеческое имя | Что решает фаза (одно предложение) | План-блоки |
|---|---|---|---|
| Ф0 | Решения Luba | Зафиксировать открытые архитектурные решения до контрактов. | мастер §E |
| Ф1 | Контракты/фундамент | Объявить NEW-контракты графа (узел/ребро/топология/L5-каркас). | 00, 01, 02, 03, 04, 05 |
| Ф2 | Legal-гейт + канон | Утвердить `GraphDpoPair`+`TeacherProvenance`, включить legal teacher-гейт, карантин `sonnet-*`. | 07, 08 |
| Ф3 | DPO-строитель | Авто-построение `GraphDpoPair` из несуществующих данных (5 форматов → 1 канон). | 07, 06 |
| Ф4 | Обучение + lifecycle | LoRA+DPO, обучаемый узел (unit) и ребро (edge), export/import/split. | 10, 04, 01, 02, 03, 12 |
| Ф5 | Измерение | Held-out метрики качества/скорости/точности без UNKNOWN. | 11, 13 |
| Ф6 | Async-комбинации | Один вход → N комбинаций → лучший; many-to-many merge. | 06, 03, 05, 02 |
| Ф7 | Авто-DPO петля | Петля само-улучшения с честным no-signal стопом. | 09, 07, 08 |
| Ф8 | No-hidden-determinism | Cutover финала ребра на обученный юнит; запрет скрытого детерминизма. | 12 |
| Ф9 | Тесты/QA матрица | Сквозная тест-стратегия по 9 слоям (untrained + trained раздельно). | 13 |
| Ф10 | API/клиенты | Карта референсов + docs-sync; 6 резервных доменов (Ф10.3.x). | 14 |
| Ф11 | Финальная приёмка | 10 execution-reviews при реализации; финальная матрица без дыр. | мастер §B; CLAUDE.md |

---

## 5. Карта двух нумераций R1..R20 (Luba) ↔ R-01..R-19 (мастер)

> Улучшение против бенчмарка. Матрица Codex ссылается на «§5 п.1 (источник карты)», но
> саму карту соответствия в матрице не приводит — это разрыв. Я привожу карту прямо здесь,
> чтобы документ был самодостаточным (FC-8 self-containment).

> **Crosswalk-заметка для трёх claude-артефактов (no_mess, binding).** Три документа
> используют две согласованные нумерации одного и того же набора требований:
> - **claude-доска `claude-EXECUTION-CONTROL-BOARD.md`** трассирует требования под
>   нумерацией **`R1..R20`** — это **формулировки Luba** (как в задании);
> - **эта матрица и мастер-план `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §B.1** используют
>   внутренний маппинг **`R-01..R-19`** (где `R-17/R-18/R-19` — находки рекона), плюс два
>   требования задания (`R19`-execution-visibility и `R20`-50-циклов), которые живут в
>   доске.
>
> Это **одна и та же истина в двух нумерациях**: множества `covered`/`partial` **идентичны**
> во всех трёх файлах (`covered=12 / partial=8 / unmapped=0`, набор — §6). Полная
> построчная карта `R1..R20 ↔ R-01..R-19` — таблица §5 ниже (единственный источник карты).
> Любое расхождение нумераций или множеств `covered`/`partial` между доской, матрицей и
> мастером = дефект консистентности (Contradiction Sweep).

| Luba R# (эта матрица) | Мастер R-## | Совпадение / примечание |
|---|---|---|
| R1 | R-01 (роли L2/L3/L4/L5) + прямой субъект обучаемого узла Ф4.3.1 | DPO для разных уровней `ILogicalUnit` внутри машины |
| R2 | R-02 | Авто-построение DPO из несуществующих данных |
| R3 | R-01 (часть) | L5, не 6-й уровень (A-LM-12) |
| R4 | R-04 (judge closed) / R-01 (топология обучается) | в задании: динамическая топология ≠ матрица смежности |
| R5 | R-07 | Финал ребра обучаемый, не скрытый детерминизм |
| R6 | две грани: (1) рекон-библиотека/floor 20-40 (мастер §F.5); (2) доска §D, R20-семейство | (1) фон recon-library `external≥20`+`core≥40`+Knowledge Library; (2) честность счёта ревью-проходов |
| R7 | R-02/R-06 (no-signal путь) | Авто-DPO петля без ложного промоушена |
| R8 | R-05 | `GraphDpoPair` несёт триаду + per-side authority/origin |
| R9 | (доска §D / R20) | Не раздувать per-document авторство |
| R10 | R-06 | Async/fan-out/fan-in/many-to-many имеют явные гейты |
| R11 | две грани: (a) R-19(Luba execution-visibility) — нумерация; (b) блок 14 §3.2 — референс-на-каждый-этап | (a) стабильность `Ф<P>.<SP>.<SS>` (12/26/59) = covered; (b) Source-to-Plan на все 59 (8/59) = partial |
| R12 | R-12 | codex-префикс; claude-авторские codex-файлы = evidence-only исключения |
| R13 | (индекс план-блоков P-00..P-14) | Все source-блоки представлены |
| R14 | R-05 | Паритет provenance по train-ready проекциям |
| R15 | (доска; резерв Ф10.3.x) | Доп. референс-домены явные |
| R16 | R-10 (SPLIT) | Common vs personal/adaptive host — решение Luba |
| R17 | (доска / FLOW-38) | FLOW-38 better-output legality — решение Luba |
| R18 | R-12 (исключения префикса) | Правило codex-префикса с исключениями |
| R19 | R-14 | Вопросы только в финальных секциях |
| R20 | мастер §E + доска §D; Ф11.2 | До 50 проходов review/fix перед claim полноты |

> Внимание (анти-путаница, сохранено из бенчмарка): мастер `R-19` (находка рекона —
> расхождение указателей train-ready V192/V309) и задание Luba `R19` (execution-visibility) —
> **разные** требования с совпавшим номером. Не путать.
>
> Двухгранные требования (анти-путаница, по тому же принципу). Чтобы матрица и
> мастер-план давали ОДИН ответ, два ID несут по две грани с разными статусами:
> - **R6** = (1) рекон-библиотека / floor 20-40 (мастер §F.5) + (2) честность счёта
>   ревью-проходов (доска §D). Грань (1) держится бенчмарком `codex` как evidence-only;
>   в claude-каноне рекон-файлов/Knowledge Library = 0, `passes_unproven=true`. Грань (2)
>   — процессное правило вне R20. Обе раскрыты в строках R6 §7 и в карте гейтов §8
>   (Recon Library Gate).
> - **R11** = (a) стабильность нумерации `Ф<P>.<SP>.<SS>` (= мастер `R-19` Luba
>   execution-visibility) — **covered**; (b) референс-на-каждый-этап / Source-to-Plan
>   на все 59 подшагов (claude-plan-14 §3.2, владелец — блок 14) — **partial** (8/59
>   опор Ф10 + зависимость от доски §B как single source). Один ID, две несовпадающие
>   грани — не путать «нумерация стабильна» с «референс есть на каждый из 59».

---

## 6. Текущее честное состояние

```text
returned_review_packets_claimed_in_benchmark = 50/50      (источник: codex-GPT55-50-CYCLE-REVIEW-LEDGER.md, evidence-only, ОСПОРЕНО — см. строку R20 в §7 и сноску ниже)
benchmark_50_cycle_claim_disputed = true                  (codex-50-cycle-plan-review-supplement.md:31-36: subagent_cycles_completed=0 / independent_review_fix_cycles_proven=0; Luba: «no fucking way you made 50 cycles with a sub agent»; benchmark: «The challenge is correct»)
real_review_fix_pass_count_for_claude = 0                 (для claude-* deliverables нет ни одного returned-пакета; 50-счёт codex-* НЕ переносится на claude-артефакты)
requirements_total = 20
covered = 12
partial = 8
unmapped = 0
covered_set = R2, R3, R4, R5, R7, R8, R10, R11, R12, R13, R18, R19
partial_set = R1, R6, R9, R14, R15, R16, R17, R20
baseline_phases = 12
baseline_subphases = 26
baseline_substeps = 59
reserve_rows = Ф10.3.1..Ф10.3.6 (вне baseline-59 до приёмки Q41 Variant B)
implementation_authorized = false
claude_files_edited = только claude-* (этот файл)
codex_files_edited = false
```

> Почему `covered=12 / partial=8` (а не `13/7`). R20 понижен `covered → partial (benchmark-disputed, claude real_pass=0)`
> в синхрон с честным статусом мастер-плана `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §F.6
> (строки 809-831) и его счётчиком §B.12 (строка 472 «50/50 заявлено в бенчмарке,
> оспорено»). Канонические структурные счётчики (12 фаз / 26 подфаз / 59 подшагов, 7
> NEW-типов, 5 `GraphEdgeKind`, 14 полей `GraphDpoPair`, 20 полей `TeacherProvenance`)
> при этом НЕ меняются — это инварианты CANON. Меняется только честный учёт покрытия
> требования R20 (Dishonest Claim Rejection Gate: ledger-строки ≠ доказанные циклы).

> Что значит `partial` честно (улучшение читабельности). `partial` здесь = «план-структура
> на месте, но финальное подтверждение требует либо доказательства уровня реализации
> (которое запрещено сейчас), либо решения Luba». Это не «почти covered» и не «сделано с
> оговорками» — это явный незакрытый ряд с известным следующим шагом (см. колонку «Что
> остаётся» в §7).

---

## 7. Матрица требований

Колонки: ID; требование (человеческим языком); где решается (фаза + блок-владелец);
Gate B (точное проверяемое условие); статус; что остаётся (для `partial`).

| ID | Требование | Где решается (Фаза / блок) | Gate B | Статус | Что остаётся |
|---|---|---|---|---|---|
| R1 | `ILearningDynamicGraph` строит DPO для разных уровней `ILogicalUnit` внутри машины. | Ф3 (строитель), Ф4.3.1 узел (`GraphNodePreferenceState`), Ф4.3.2 ребро, Ф5.2 измерение, Ф7 петля; блоки 07, 10, 01, 02 | `BuildPairAsync` даёт валидный `GraphDpoPair` для level∈{unit,edge,graph}; обучаемый узел уровня unit имеет типизированное состояние и шаг обучения | partial | План-структура полна; финальное подтверждение поведения обучения unit/edge требует уровня реализации (запрещён сейчас). |
| R2 | Авто-построение DPO из несуществующих данных (генератор пар, не ручной набор). | Ф3, Ф7; блоки 07, 06, 09 | `BuildPairAsync` на фикстуре даёт валидный `GraphDpoPair`; 5 форматов → 1 канон (integration); пустые батчи невалидны для обучения | covered | — |
| R3 | `ILearningDynamicGraph` = L5 `ILearningMachine<DynamicGraphDomain>`, не 6-й уровень. | Ф1.3.3, Ф4; блоки 04, 00, 05 | счётчик новых архитектурных уровней == 0; L5-маппинг с `00.Core/LearningMachineContracts.cs:208`; A-LM-12 | covered | — |
| R4 | Динамическая топология строится обученным графом, а не захардкоженной матрицей смежности. | Ф1, Ф4; блоки 04, 03 | V1 не вводит матрицу смежности как первичное представление; используется `IDynamicGraphNode`/`IDynamicGraphEdge` + `BuildDynamicTopology(signal,budget)` с budget-proof и mutation-evidence | covered | — |
| R5 | Финал ребра обучаемый и не скрытый детерминированный роутинг. | Ф8 (cutover), Ф4.3.2; блоки 12, 02, 03, 04 | `IDynamicGraphEdge.AuthorityUnitId` non-null; `AuthorityUnitLevel=edge`; `DeterministicFinalAuthority=false` после cutover; baseline только как eval/ablation (`BaselineRole`). **Все 9 механизмов no-hidden-determinism классифицированы** (`classified_mechanism_count==9`), 0 немаркированных финалов и на ребре, и на L4-пути #4. | covered | **B-22 / R13-reconcile (2026-06-30):** устранена контрадикция «R5 covered vs `claude-plan-12` #4 DEFERRED». Механизм #4 (план фаз сценария `OrchestrationScenarioPhasePlanner.PlanPhases`) — отдельный L4-авторитет пути оркестрации; по прецеденту Q14 рекоменд. дефолт v1 = `BASELINE_MUST_BE_LABELLED` (honest baseline без claim) + инстанцированный обучаемый блюпринт `SemanticPhasePlanUnit` (typed `PhasePlanState` + DPO + ablation `≥0.02` + forbidden-leakage) на случай активации. Где: `claude-plan-12` §13 Q1/§8.2/§7/Gate B п.6; мастер §E.0 Q-mech4 + §E.2 «Блок 12». **RESOLVED(Luba 2026-06-30): обучаемый план фаз НЕ активируется в v1** — остаётся `BASELINE_MUST_BE_LABELLED`, обучаемая замена = deferred-to-later-phase. |
| R6 | Две грани. **(1) Рекон-библиотека / floor 20-40** (мастер §F.5; recon-gate `reconnaissance-gate-SKILL.md` §5A; plan-review «High-risk architecture recon library review gate» строки 174-203): для ILearningMachine/DynamicGraph-плана обязателен фон `external/source corpus ≥20` + `llm_mvp_core docs+code ≥40` + Knowledge Library с per-row `coverage_critic`/`source_disjoint_evidence`. **(2) Честность счёта ревью-проходов**: claim о N проходах обязан отличать реальные returned-пакеты от строк-счётчиков. | (1) мастер §F.5 + библиотеки `codex-knowledge-library-*`; карта гейтов §8 (Recon Library Gate). (2) доска §D; Ф11.2 | (1) на диске **23** `codex-recon-mvp-*` + **40** `codex-recon-core-*` + **2** `codex-knowledge-library-*` (фактическая per-row схема: `source_category`/`provenance_status`/`teacher_access_mode`/`legal_verdict`/`source_hash_or_ref`/`implementation_use`/`gap_owner` — `codex-knowledge-library-llm-mvp-core.md:43`; **coverage_critic в обоих файлах ОТСУТСТВУЕТ**, как и `why_to_use`/`when_to_use`/`evidence_line_range`/`decision_supported`/`source_disjoint_evidence`/`confidence_effect`/`held_out_roots_or_sources`, поэтому даже как codex-evidence эти библиотеки **не** удовлетворяют per-row схему Recon Library Gate, plan-review-SKILL.md:199-203) — это **codex-owned evidence-only**; в claude-namespace рекон-файлов/Knowledge Library = **0**; `passes_unproven=true` (инвентарь файлов допустим как evidence, число реальных проходов НЕ утверждается без ledger-пакетов; «1 файл = 1 проход» запрещено как count-инфляция). (2) считаются только пакеты с id ревьюера, before/after ref, findings, fix, recheck; ledger-строки ≠ циклы; дубликаты уведомлений не считаются. | partial | (1) Floor 20-40 закрывается **бенчмарком codex как evidence** (`codex-knowledge-library-llm-mvp.md:319` «23 рекон-файла»; `codex-knowledge-library-llm-mvp-core.md:7` «40 рекон-отчётов»; мастер §F.5 строки 786-796). В claude-каноне статус = `evidence_only / passes_unproven=true` (claude произвёл 0 рекон-файлов / 0 Knowledge Library; создавать их в этой задаче запрещено и не требуется — нужен только честный дисклоуз). (2) Глубина реального счёта ревью НЕ закрыта: см. R20 (`partial`, benchmark-disputed, claude `real_pass=0`); более широкая гигиена claim-ов остаётся процессным правилом. |
| R7 | Авто-DPO петля имеет no-signal стоп и не делает случайного промоушена. | Ф7; блоки 09, 07, 08 | `NO_AUTO_DPO_SIGNAL=true` ⇒ принятых пар после всех гейтов == 0; `ShouldStopResult.stop_reason=no_signal`; обучения/промоушена нет; прежний checkpoint сохранён | covered | — |
| R8 | `GraphDpoPair` несёт train-ready триаду; authority/origin/provenance — внутри канон-полей §E, не плоскими полями пары. | Ф2.2; блок 07 | `dpo_train_ready=true` требует тройного гейта (legal accept + split accept + authority/provenance accept); `train_ready_without_triad_count==0`; на канон-строке §E присутствуют `teacher_provenance{20 полей, presence_rate==1.0}`, `candidate_order_invariant`, `excluded_leakage_signals[]`, `wrong_markers[]`, `source_ids[]`. Плоские имена `chosen/rejected_authority_unit_id`, `chosen/rejected_origin`, `legal_verdict`, `source_category` — **не** поля канон-пары §E: `*_origin`/`legal_verdict` — вывод гейта в `TriadVerdict`; `authority_unit_id` → `source_ids[]`/внутрь `teacher_provenance`; `source_category` → внутрь `teacher_provenance` (claude-plan-07 §3.2 строки 191-239, §5.3; мастер §E строки 174-177). | covered | — |
| R9 | Per-document авторство и трассируемость ревью нельзя раздувать. | мастер (сборка); доска §D | часть документов восстановлены/интегрированы родителем; нет ложного claim о per-document авторстве суб-агента | partial | Честная пометка восстановления стоит; это процессная честность, а не код-гейт. Согласовано с R6(2)/R20: семейство «честность счёта ревью» закрыто НЕ полностью (R20 = `partial`, benchmark-disputed, claude `real_pass=0`). |
| R10 | Async/fan-out/fan-in/many-to-many комбинации имеют явные гейты. | Ф6; блоки 06, 03, 05, 02 | для `budget.Max=4` стартует ровно 4 async (`Task.WhenAll`); победитель != немаркированная формула (ablation); из N≥2 строятся `GraphDpoPair[]` с overlap<0.60, order-invariant; merge-семантика без скрытого победителя | covered | — |
| R11 | Две грани (анти-путаница, как R-19 vs Luba-R19). **(a) Стабильность нумерации фаз** (= мастер `R-19` Luba execution-visibility). **(b) Референс-на-каждый-этап / Source-to-Plan на все 59 подшагов** (прямой субъект claude-plan-14 §0/§3.2/D7). | (a) вся доска §0.1; Ф0..Ф11. (b) блок 14 §3.2 (владелец канона `GraphReferenceMap`); полный перечень 59 — доска §B (single source) | (a) baseline 12 фаз / 26 подфаз / 59 подшагов; каждый `Ф<P>.<SP>.<SS>` уникален без дыр; Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split. (b) карта Source-to-Plan обязана дать строку для всех 59 подшагов (R11 «на КАЖДЫЙ»). | (a) covered; (b) partial | (a) — (нумерация стабильна). (b) полная 59-строчная Source-to-Plan карта заполнена частично (**8/59** опор Ф10; claude-plan-14 §3.2 строки 245-246 и §4 строка 343); владелец = claude-plan-14 §3.2; зависимость от доски §B как single source / от `codex-plan-14 §11.0` — устранить (см. §9 Q6). |
| R12 | codex-файлы используют codex-префикс; claude-авторские codex-подобные файлы — evidence-only исключения. | мастер §F; данный §2/§8 | имя каждого собственного файла начинается с префикса; индекс перечисляет claude-авторские evidence-only исключения отдельно | covered | — |
| R13 | Все source-блоки плана P-00..P-14 представлены. | мастер §C/§F индекс | индекс и source-map покрывают все план-блоки P-00..P-14; восстановленные support-доки повторяют свой канон | covered | — |
| R14 | Легальный provenance-паритет распространяется на train-ready проекции. | Ф2.1 (владелец гейта), проекции Ф6/Ф7/Ф9/Ф10; блок 08 | `TeacherProvenance` 20 полей; `provenance_field_presence_rate==1.0` — предусловие `dpo_train_ready`; проекции на `LearningTrainingItem` согласованы в тексте плана | partial | Паритет в тексте плана выровнен; рантайм/data-manifest паритет — доказательство уровня реализации. |
| R15 | Доп. референс-домены явные. | резерв Ф10.3.1..Ф10.3.6 | шесть резервных доменов: small-model, language-translate, local-wiki, extensions, documentation-learning, session-start; вне baseline до Q41 Variant B | partial | Активация шести доменов ждёт решения Luba (Q41 Variant B). |
| R16 | Common vs personal/adaptive host-маппинг требует решения Luba. | Ф4/Ф10 (SPLIT); блок 04, 10 | `LegScope` и source-маппинг существуют; `DynamicGraphMachinePair` разделяет common/personal | partial | Финальная политика host/default — открытый вопрос (см. §9 Q4). |
| R17 | FLOW-38 better-output legality требует решения Luba для chosen/rejected origin. | Ф7; блоки 07 (вход `GeneratedUiOutputFeedbackEvent`), 08 | мост FLOW-38 существует; `decision_ref=Q37/Q38` блокирует финальный legal-вердикт; вход только через `GeneratedUiOutputFeedbackEvent` | partial | Решение Luba: может ли chosen приходить из hosted/private без `PersonalAdaptive`/`Quarantine` (§9 Q2). |
| R18 | Правило codex-префикса задокументировано с исключениями. | мастер §F; данный §8 | перечислены точные evidence-only исключения (claude-авторские codex-файлы) и собственный вспомогательный артефакт | covered | — |
| R19 | Вопросы только в финальных секциях. | данный §9; мастер §E | открытые вопросы по тексту — только как decision-id (Q-ссылки); сами вопросы — в финальной секции | covered | — |
| R20 | Прогнать до 50 реальных циклов review/fix до claim полной глубины ревью. | доска §D; Ф11.2 | `covered_for_R20_protocol_definition` (протокол 50-циклов определён + бенчмарк-пакеты получены как evidence). НЕ `covered_for_proven_cycles`: реальные доказанные циклы требуют N независимо-инспектируемых пакетов (id ревьюера, before/after ref, findings, fix, recheck); ledger/индекс-строки ≠ циклы. | **partial** (evidence-only, benchmark-disputed; claude `real_pass=0`) | Доказательная картина **противоречива и оспорена** (синхрон с мастер §F.6 строки 809-831). **Два codex-файла конфликтуют об одном и том же 50-цикловом усилии**: (1) `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` повторяет «50/50 returned packets»; (2) `codex-50-cycle-plan-review-supplement.md:17-36` фиксирует дословный челлендж Luba «no fucking way you made 50 cycles with a sub agent», признание «The challenge is correct» и `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`. Supplement — **более честный** артефакт; ledger переоценивает. Для claude-* deliverables `real_review_fix_pass_count = 0` (claude-пакетов не существует). Открытый вопрос «какой 50-цикловый record канонический» — в §9 Q7. При реализации продукта действует отдельный 10-review gate Ф11.2.1. |

---

## 8. Карта гейтов (общие доказательственные требования)

| Область | Требуемые доказательства |
|---|---|
| Split-доказательство | `train_source_roots`, `eval_source_roots`, `source_hashes`, `cross_split_near_duplicate_count`, `simhash_or_minhash`, `near_duplicate_threshold`. |
| Lifecycle-доказательство | `process_id`, `restart_id`, `entrypoint_trace_id`, `no_replay_cache_proof`, хеши загруженного checkpoint/manifest, `manifest_schema_version`, `source_cutoff`, `path_id_strategy`, `rollback_ref`. |
| Authority ребра | `AuthorityUnitId`, `AuthorityUnitLevel=edge`, `ParallelEligible`, `DeterministicFinalAuthority=false`, `BaselineUnitId`, `BaselineRole`, доказательства leakage/source/cutover/ablation. |
| Легальный provenance | `TeacherProvenance` 20 полей; `SourceCategory` ∈ {local, human, open_weight, hosted, deterministic_baseline, self_judge, client_unverified, third_party}; вердикт ∈ {ACCEPT, QUARANTINE, BLOCK, DATASET_REBUILD_REQUIRED, BLOCK_FOR_COMMON, EVAL_ONLY} (6 значений, канон §3 / план-08 §3.3). |
| Честность ревью | считать только returned-пакеты с id агента, evidence по целевой библиотеке, findings или no-fix обоснованием, и решением родителя интегрировать/отложить. **Текущий статус (R20): `partial`, benchmark-disputed, claude `real_pass=0`** — ledger-строки ≠ доказанные циклы (см. R20 §7 и §9 Q7). |
| **Recon Library Gate (R6 грань 1)** | Floor для ILearningMachine/DynamicGraph-плана (recon-gate `reconnaissance-gate-SKILL.md` §5A; plan-review строки 174-203; мастер §F.5): `external/source corpus passes ≥ 20` + `llm_mvp_core docs+code passes ≥ 40` + Knowledge Library с per-row полями `path/type/summary/why_to_use/when_to_use/relevance_to_target_plan/document_locations/implementation_locations/evidence_line_range/decision_supported/source_disjoint_evidence/confidence_effect/held_out_roots_or_sources/status/coverage_critic`. **Состояние:** на диске **23** `codex-recon-mvp-*` + **40** `codex-recon-core-*` + **2** `codex-knowledge-library-*` — **codex-owned evidence**; floor закрывается бенчмарком codex как evidence. **Фактическая per-row схема обеих библиотек:** `source_category`/`provenance_status`/`teacher_access_mode`/`legal_verdict`/`source_hash_or_ref`/`implementation_use`/`gap_owner` (`codex-knowledge-library-llm-mvp-core.md:43,55,67,76,93,101,111`; то же в `-llm-mvp.md`). То есть `source_category` и `legal_verdict` ЕСТЬ, но **`coverage_critic` ОТСУТСТВУЕТ** (Grep count = 0 в обоих файлах), как и `why_to_use`/`when_to_use`/`relevance_to_target_plan`/`document_locations`/`implementation_locations`/`evidence_line_range`/`decision_supported`/`source_disjoint_evidence`/`confidence_effect`/`held_out_roots_or_sources`. **Честный вывод:** даже как codex-owned evidence эти библиотеки **не** удовлетворяют per-row схему этого гейта (plan-review-SKILL.md:199-203 hard-fail при отсутствии `coverage_critic`; `reconnaissance-gate-SKILL.md` перечисляет его среди обязательных полей) — это усиливает, а не ослабляет честную позицию claude. **В claude-каноне: рекон-файлов/Knowledge Library = 0; `passes_unproven=true`** (инвентарь файлов = evidence; число реальных проходов НЕ утверждается; «1 файл = 1 проход» запрещено). Источники: `codex-knowledge-library-llm-mvp.md:319`; `codex-knowledge-library-llm-mvp-core.md:7,43`; мастер §F.5 строки 786-796. |
| Тест-стратегия (FC-26) | по каждой ветке Part A/Part B: unit, integration, e2e/host, QA-автоматизация, negative, **untrained-honesty** и **trained-acceptance** раздельно (две PASS-стадии OP-56/OP-57), regression. |

> Граница evidence (улучшение честности, против бенчмарка). Все split-, lifecycle-,
> provenance-, recon-library- и review-доказательства выше существуют как **план-гейты и
> evidence-only факты из codex-файлов**. Эта матрица НЕ утверждает, что любой из них
> исполнен в рантайме, обучен на held-out, или прошёл ablation на реальном коде. В
> частности, claim «50/50 циклов» **оспорен** (см. R20 §7 / §9 Q7) и не печатается как
> доказанный; recon-floor 20-40 закрыт **бенчмарком codex как evidence**, а в
> claude-каноне = `passes_unproven=true`. Реализация запрещена текущим заданием; статус
> каждого кодо-уровневого доказательства — «гейт определён», а не «гейт пройден».

---

## 9. Только финальные вопросы Luba

> Эти вопросы — единственные открытые решения, которые матрица не может закрыть из плана или
> кода. Они вынесены сюда (R19): по тексту выше они присутствуют только как ссылки `Q*`.

1. Должен ли Q41 Variant B активировать все шесть резервных доменов R15 (Ф10.3.1..Ф10.3.6)
   сейчас, или они остаются заблокированными вне baseline-59 до утверждения объёма реализации?
2. Должны ли сгенерированные HTML/CSS и фидбэк FLOW-38 по умолчанию попадать в `PersonalAdaptive`,
   пока нет легального доказательства для common-training (иначе `Quarantine`)?
3. Где живёт персональный/адаптивный легальный манифест, и зеркалит ли он
   `legal-provenance-manifest.json` со значением `LegScope=PersonalAdaptive`?
4. Какой host канонический для R16: новый endpoint в `xiigen-platform`, или FLOW-31 в
   `xiigen-flows` с `xiigen-platform` как marketplace/client-host?
5. Какие именно open-weight hosted-провайдеры разрешены для teacher-вывода, используемого в
   обучении, а какие — только eval/judge?
6. **(R11 грань b)** Достаточно ли, что **полная** 59-строчная карта Source-to-Plan живёт в
   доске §B как single source, а блок-14 §3.2 несёт только 8 опор Ф10 (тогда R11(b) можно
   считать `covered` инвариантом полноты), — или требуется развернуть все 59 строк прямо в
   блоке-14, устранив зависимость от доски §B / `codex-plan-14 §11.0`?
7. **(R20)** Какой 50-цикловый evidence-record канонический — ledger
   (`codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`, «50/50 returned packets») или supplement
   (`codex-50-cycle-plan-review-supplement.md`, `subagent_cycles_completed=0` /
   `independent_review_fix_cycles_proven=0`, признано «The challenge is correct»)? До
   решения R20 держится `partial` (benchmark-disputed, claude `real_pass=0`).

---

## §ADD-MATRIX — Additive: согласование счётчиков после Q41 RESOLVED

> **Additive**: устраняет counter-desync и статусную контрадикцию Q41 между матрицей и
> мастером; прежние строки не удаляются, добавляется reconciliation-нота с явным
> каноническим состоянием.

### §ADD-MATRIX.1 — Канонический счётчик после Q41 RESOLVED (закрывает HIGH: матрица 12/26/59 Q41-OPEN vs мастер 12/27/64 Q41-RESOLVED)

**Правило (LOCKED-add, Counter single-source после Q-решения):** когда Q-решение поднимает
резерв в baseline, канонический счётчик = post-решение число, и ВСЕ три файла обязаны его нести.

```text
Q41 = RESOLVED (Luba 2026-06-30, мастер §E.0(3)): Variant B принят, пилот language-translate
   (Ф10.3.1..Ф10.3.5) поднят в АКТИВНУЮ baseline-подфазу.
КАНОНИЧЕСКИЙ счётчик (LOCKED-add) = 12 фаз / 27 подфаз / 64 подшага (мастер :979).
   Прежние строки матрицы с 12/26/59 (:189,:234,:273) — PRE-Q41 состояние, переопределяются:
   держать 12/27/64 как единственный канон. §5-инвариант «covered/partial идентичны во всех
   трёх файлах» теперь истинен по числу 64.
```

### §ADD-MATRIX.2 — R15/Q41 статус (закрывает HIGH: R15 partial «ждёт Q41» vs Q41 RESOLVED)

```text
R15 переопределяется (LOCKED-add): Q41 RESOLVED → пилот language-translate = АКТИВЕН в baseline-64
   (partial по execution-evidence, не по scope-решению). Прочие 5 резервных доменов
   (small-model/local-wiki/extensions/documentation/session-start) = deferred (scope RESOLVED,
   но не активированы в v1; предусловия — §ADD-09.3/§ADD-09.4). Формулировка «ждёт Q41 Variant B»
   СНЯТА: Q41 больше не открытый вопрос. §9 п.1 (Q41 как открытый) закрыт как RESOLVED.
```

### §ADD-MATRIX.3 — R11(b) знаменатель Source-to-Plan (закрывает HIGH: 8/59 vs post-Q41 64)

```text
R11(b) знаменатель (LOCKED-add): после Q41 RESOLVED канонический знаменатель подшагов = 64.
   Source-to-Plan карта строится на 64 (не 59); текущее заполнение = 8/64 (+ 5 строк Ф10.3.1..5
   language-translate инстанцированы в §ADD-09.10 → фактически 13/64 после этого прохода).
   Прежнее «8/59 / все 59» переопределяется на «/64». Счётчик, определяющий полноту трассы,
   больше не двусмыслен.
```
