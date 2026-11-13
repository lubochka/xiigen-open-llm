# Benchmark Delta Report — план Claude ILearningDynamicGraph → мой канон (codex)

> **Артефакт:** `codex-BENCHMARK-DELTA-REPORT.md`
> **Автор-роль:** Benchmark Delta аналитик (Opus 4.8 MAX).
> **Тип артефакта:** ПЛАН-ONLY / EVIDENCE-аналитика. Это **НЕ** разрешение
> реализовывать продукт/код, тренировать, билдить, запускать раннеры, git или
> вызывать внешние AI/API/модели. Сам по себе этот отчёт **не правит** канон:
> каждая `accept`-строка закрывается только когда мой целевой `codex-*`-артефакт
> реально содержит изменение (read-back по строке/секции) — а это отдельная
> задача с отдельным approve Luba.
> **Язык:** русский (человеческим языком); имена типов/полей/ID — английский.

---

## ЯКОРЬ ЗАДАЧИ (CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY)

```text
CURRENT TASK:  сравнить план Claude (claude-* в "dpo legal build" + чужой
               claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md) с МОИМ каноном codex и
               записать Benchmark Delta Report ровно в
               codex-BENCHMARK-DELTA-REPORT.md: по каждому полезному элементу плана
               Claude — benchmark_item | source | current_target_status |
               architecture_compatible | accept/reject/defer | destination_file+секция |
               Gate_B | completion_evidence; плюс Top-N, таблица конфликт/не-конфликт,
               причины отклонения/отсрочки.
NOT THE TASK:  править любой claude-* файл; править два codex-файла авторства Claude
               (codex-50-cycle-plan-review-supplement.md,
               codex-skill-improvement-ideas-planning-review-architecture.md);
               править иные codex-файлы кроме этого отчёта; копировать слабые места
               Claude; ломать мой CANON; исполнять/билдить/тестить код; git; внешние
               AI/модели.
EVIDENCE-ONLY: все claude-*.md (00..07 + claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md),
               codex-50-cycle-plan-review-supplement.md и
               codex-skill-improvement-ideas-planning-review-architecture.md (их автор
               — Claude); мой канон codex-* читается как источник «что у меня уже есть».
```

> **CANON (мои инварианты, которые нельзя нарушить ни одной accept-строкой):**
> 1. `ILearningDynamicGraph` = `ILearningMachine<DynamicGraphDomain>` (L5,
>    `00.Core/LearningMachineContracts.cs:208`), **без** 6-го уровня (A-LM-12).
> 2. Легальный teacher-гейт: дистилляция — только open-weight permissive; Claude/
>    OpenAI/Gemini — никогда teacher (только judge, judge ≠ цель обучения);
>    `closed_proprietary_model=true && used_for_training=true ⇒ DATASET_REBUILD_REQUIRED`.
> 3. Запрет скрытого детерминизма (A-LM-09/A-LM-13): финал ребра — обученный узел;
>    детерминированный роутер/счётчик/словарь = генератор кандидатов или честный
>    `baseline/eval-only`.

---

## 0. Метод сравнения и условные обозначения

### 0.1. Что с чем сравниваю

- **Benchmark (Claude):** библиотека `claude-00..07` в этой папке + более глубокий
  чужой план `C:/Projects/llm_mvp_core/docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md`
  (921 строка, Part A + source map + gates). Плюс два codex-файла авторства Claude
  (`codex-50-cycle-plan-review-supplement.md`,
  `codex-skill-improvement-ideas-planning-review-architecture.md`) — это тоже benchmark/evidence.
- **Мой канон (codex), редактируемый только по отдельной задаче:** мастер-план
  `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`, доска `codex-EXECUTION-CONTROL-BOARD.md`
  (12 фаз/26 подфаз/59 базовых подшагов `Ф<P>.<SP>.<SS>`), матрица
  `codex-REQUIREMENTS-COVERAGE-MATRIX.md`, 16 решённых блоков `codex-plan-00..14`,
  5 гайдов `codex-guide-01..05`, 2 библиотеки `codex-knowledge-library-*`.

### 0.2. Главный вывод сравнения (одной фразой)

Мой канон **системно глубже** benchmark-библиотеки Claude почти по всем осям (Part A
§0–§5 с `note for`/симуляциями/`file:line`, доска с строгой последовательностью,
матрица покрытия, per-block решённые блоки, машина вердикта легального гейта, two-PASS,
ablation/leakage/permutation, async N-комбинаций). Это **подтверждает сам Claude** в
своём `codex-50-cycle-plan-review-supplement.md` (цикл 47: «my first supplement…
superseded by deeper plan layer»; Second-Pass Summary: «later plan layer is materially
deeper than my first fast scaffold»; `main_plan_quality = strong`). Поэтому брать у
Claude можно только **точечные совместимые добавки**, а не структуру и не «быстрый»
уровень его `claude-06` brief.

### 0.3. Колонки строки дельты (формат по `plan-review-SKILL.md:2146`)

`benchmark_item | source (файл:строка/секция) | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence`.

- **current_target_status** — есть ли это уже у меня (covered / partial / absent).
- **architecture_compatible** — совместимо ли с CANON (да/нет + почему).
- **completion_evidence** — что докажет, что accept-строка реально внесена в канон
  (read-back точной строки/секции целевого `codex-*`). После GPT-5.5 cycles 26-30
  этот отчёт больше не считает все accept rows `PENDING`: T2/T3/T4/T5 получили
  target body read-back, T1 остаётся partial/read-back-needed для полного manifest
  lifecycle closure.

---

## 1. Top-N самых ценных совместимых добавлений (приоритет)

> Только то, что (а) реально усиливает мой канон и (б) совместимо с CANON. Каждое —
> детализировано в §3. «Сила» = насколько это закрывает реальный пробел.

| # | Добавление | Почему ценно (пробел в моём каноне) | Куда (мой файл) | Сила |
|---|---|---|---|---|
| **T1** | **Lifecycle-полнота манифеста: `rollback` + `schema_version` + `source_cutoff` + `late_file_queue` + `path_id_strategy`** | Скил `trainable-architecture-decision-SKILL.md` (Graph manifest lifecycle, стр. 96-99) ТРЕБУЕТ их явно; у меня в `codex-plan-*` и в управляющих артефактах **их НЕТ** (Grep `rollback\|schema_version\|cutoff\|late-file` по `codex-plan-*` = 0; по доске/матрице/мастеру = 0). Claude видит `25.ModelServer` upgrade/rollback и checkpoint-manifest-conversion. | `codex-plan-10` §lifecycle + `codex-plan-04` §5; доска **Ф4.2.3** | **высокая** |
| **T2** | **`terms_snapshot_ref` (со снимком/датой) + geography/export-constraint как ХРАНИМЫЕ поля провенанса** | У меня в `TeacherProvenance` есть `provider_terms_checked` (bool) и `model_version/request_id`, но **нет** ссылки-снимка условий с датой и нет поля юрисдикции/экспорта. Claude фиксирует `terms_snapshot_ref` (`claude-04:60`) и geography/export gate (`claude-02:61`, `claude-07`). | `codex-plan-08` §3.1/§5 (`TeacherProvenance`); доска **Ф2.1.1** | **средняя** |
| **T3** | **Явный privacy/retention + redaction gate для hosted-провайдеров** как ИМЕНОВАННЫЙ гейт | У меня приватность закрыта через R5 (adaptive leg) и анти-leakage, но **отдельного** «raw project text → hosted provider retention/redaction» гейта как строки нет. Claude: `privacy_gate` (`claude-07:43`), retention/redaction (`claude-04:26,79,90`). | `codex-plan-08` §8 (новый под-гейт) + `codex-plan-10` split R5; доска **Ф2.1.2** | **средняя** |
| **T4** | **Конкретный иллюстративный набор async-комбинаций (candidate A/B/C/D как путь-шаблоны)** для читаемости §2 | Механизм N-комбинаций у меня есть (`codex-plan-06`, доска Ф6), но **наглядного** набора «A: Encoder→Direct; B: Encoder→Critic→Repair; C: parallel Evidence+Risk→Merge; D: Bridge→Critic→Final» нет. Claude: `claude-05:92-99`. | `codex-plan-06` §2 (аналогия) | **низкая (читаемость)** |
| **T5** | **Source-registry-категория на КАЖДУЮ строку (local/human/open-weight/hosted/deterministic-baseline/self-judge) как единая enum рядом с вердиктом** | У меня вердикт строки есть (ACCEPT/QUARANTINE/BLOCK/REBUILD, блок 08), но единой **категории происхождения** строки как отдельного перечисляемого поля нет; Claude вводит `rejected_origin`/source-категории (`claude-04:48-49`, `claude-05` judge-strategy). Усиливает аудит контаминации. | `codex-plan-08` §3.1 (`TeacherProvenance` доп. поле `source_category`) | **низкая** |

> **Итого Top-5.** T1 — единственное по-настоящему «дырное» (скил требует, канон молчит).
> T2/T3 — полезные легальные уточнения. T4/T5 — читаемость/аудит, не архитектура.

---

## 2. Полная Benchmark Delta таблица (по каждому полезному элементу плана Claude)

> Сгруппировано по источникам. `acc`=accept, `rej`=reject, `def`=defer.

### 2.1. Из `claude-06-ilearningdynamicgraph-architecture-brief.md`

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Иерархия `ILearningMachine` над orchestrator; train только на машине | `claude-06:118,134` | covered (мой §A.3, A-LM-02) | да | **rej** (дубль) | — | — | у меня уже §A.3 + блок 04/05; добавление нечего |
| `DynamicGraphMachine`/`Orchestrator`/`Flow`/`Unit` компонентный вид | `claude-06:17-31` | covered (мой §0 UML, блоки 00/03/04) | да, но мельче | **rej** (мой §0 глубже: 25 классов, `note for`, симуляции E1..E31) | — | — | мой `codex-plan-00` строго полнее |
| Graph state как список полей (node/edge registry, dpo history, metric snapshots, checkpoint manifest) | `claude-06:59-70` | covered (мой `DynamicGraphTopology` + typed state блоки 01/02/04) | да | **rej** (мой typed-record сильнее, чем плоский список) | — | — | `codex-plan-04` §5 typed state |
| Routing model input/output контракты (selected flow ids, parallel levels, bridge subphases, reason codes) | `claude-06:76-98` | covered (мой `EdgeKind`/`AuthorityKind`, блок 02/03) | да | **rej** (дубль, у меня типизировано) | — | — | `codex-plan-02/03` §5 |
| Async: независимые ветки параллельно, зависимые ждут; сравнивать комбинации, не юниты | `claude-06:100-114` | covered (мой блок 06 + доска Ф6) | да | **rej** (дубль) | — | — | `codex-plan-06`; доска Ф6.1 |
| Разделение deterministic infra vs learned policy (closure/bridge = helper, DPO path = learned) | `claude-06:72` | covered (мой блок 12 + A-LM-09/13) | да | **rej** (мой блок 12 = классификация 9 механизмов с blueprint) | — | — | `codex-plan-12` |
| Solved-subtasks таблица (hierarchy/runtime entry/DPO row/legal/branch/transition/helpers/learned-claim) | `claude-06:130-142` | covered (распределено по моим 16 блокам) | да | **rej** (мой охват шире и с Gate B) | — | — | мастер §C |

### 2.2. Из `claude-04-dpo-legal-model-policy.md`

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Core rule: closed/hosted API output нельзя в common-training/distill/DPO без явных terms+approval | `claude-04:16-17` | covered (мой блок 08 §3.3 шаг 2 BLOCK) | да | **rej** (дубль, у меня жёстче — машина вердикта) | — | — | `codex-plan-08` §3.3 |
| Closed API prohibitions таблица (train/hidden teacher/judge/raw-logs/access≠rights) | `claude-04:20-28` | covered (мой блок 08 §3.2 denylist + §8.1) | да | **rej** (дубль) | — | — | `codex-plan-08` §3.2 |
| Candidate conditions (self-hosted open-weight / hosted open / closed / human / local-deterministic) | `claude-04:31-37` | covered (мой allowlist Qwen3/DeepSeek/Kimi/GLM/Mistral + first-party/human/baseline) | да | **rej** (дубль) | — | — | `codex-plan-08` §3.1/§3.2 |
| **Provenance field `terms_snapshot_ref`** (снимок условий с датой) | `claude-04:60` | integrated in target body | **да** (доп. поле, не ломает канон) | **acc** (T2) | `codex-plan-08` §3.1/§5: добавить поле `TermsSnapshotRef` в `TeacherProvenance` | поле `TermsSnapshotRef` присутствует в record + домен значений; gate: `open_weight_hosted && used_for_training=true && TermsSnapshotRef==UNKNOWN ⇒ QUARANTINE` | READBACK: `codex-plan-08-legal-teacher-gate.md` contains `TermsSnapshotRef` and `GeographyExportConstraint` in `TeacherProvenance` |
| **Privacy retention / redaction** (raw project text в hosted-логах → запрет/редакция) | `claude-04:26,79,90` | integrated in target body, with remaining product policy questions in final section | **да** | **acc** (T3) | `codex-plan-08` §8.5 privacy/retention; ссылка из `codex-plan-10` split R5 | под-гейт §8.5 описывает: hosted-провайдер + raw private text ⇒ требуется redaction_policy_ref или local-only; Gate B: `hosted && raw_private_text_copied=true && redaction_policy_ref==none ⇒ BLOCK_FOR_COMMON`; retention absence ⇒ `BLOCK_FOR_HOSTED` | READBACK: `codex-plan-08-legal-teacher-gate.md` contains `BLOCK_FOR_COMMON` and `BLOCK_FOR_HOSTED`; `codex-plan-10-training-lifecycle.md` contains `LegScope` |
| **`source category` / `rejected_origin` per row** как enum | `claude-04:48-49` | integrated in target body | да | **acc** (T5, низкий приоритет) | `codex-plan-08` §3.1 + `codex-plan-07` §5: `SourceCategory` enum, per-side origin/authority fields | поле `SourceCategory` с доменом; `rejected_origin`/`RejectedOrigin` present; QA-скан: 0 train-ready с `SourceCategory=self_judge` без legal-гейта | READBACK: `codex-plan-08-legal-teacher-gate.md` has `public enum SourceCategory`; `codex-plan-07-dpo-builder.md` has `RejectedOrigin` |
| Provenance поля row_id/task_id/domain_tag/prompt_hash/chosen_hash/rejected_hash | `claude-04:43-70` | covered (мой `pair_id/level/node_or_edge_id/source_ids/source_sha256`, блок 08 §5) | да | **rej** (эквивалент уже есть) | — | — | `codex-plan-08` §5 |
| Allowed planning defaults (prefer self-hosted; human gold; closed blocked; separate eval/train legs) | `claude-04:83-90` | covered (мой §3.1/§8.1 + R5 legs) | да | **rej** (дубль) | — | — | `codex-plan-08` §8.1 |

### 2.3. Из `claude-05-dpo-data-training-measurement-guide.md`

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Pair shape prompt/chosen/rejected + graph metadata (graph_task_id, chosen/rejected_path, evidence_ids, split) | `claude-05:19-40` | covered (мой канон `GraphDpoPair` 14 полей, блок 07/08 §5) | да | **rej** (мой канон богаче) | — | — | `codex-plan-07` §5.1 |
| Pair generation strategy (low-quality local = rejected; approved = chosen; reject near-identical/nonsense; split first) | `claude-05:44-51` | covered (мой блок 07 §3 Шаг3-6, overlap<0.60, near-miss) | да | **rej** (дубль) | — | — | `codex-plan-07` §3 |
| Judge strategy таблица (human gold / open-weight future / graph self-judge secondary / closed blocked / deterministic helper) | `claude-05:54-61` | covered (мой блок 08 §3.3.8 judge-ветка + блок 06 judge) | да | **rej** (дубль) | — | — | `codex-plan-08` §3.3 |
| Measurement families (DPO/graph routing/branch quality/lifecycle/leakage/resource) | `claude-05:64-72` | covered (мой блок 11 §3 три оси + per-edge контроли) | да | **rej** (мой §11 строже: live≠held-out, 5 leakage) | — | — | `codex-plan-11` |
| Anti-deterministic warning (learned-claim требует train/held-out/margin/lift/fresh-load/ablation/split) | `claude-05:74-86` | covered (мой two-PASS блок 11 §10) | да | **rej** (дубль, у меня two-PASS жёстче) | — | — | `codex-plan-11` §10 |
| **Async unit combination comparison: конкретный набор candidate A/B/C/D** | `claude-05:92-99` | integrated in target body | да | **acc** (T4, читаемость) | `codex-plan-06` §2 (аналогия): иллюстративный набор A/B/C/D candidate paths | в §2 присутствует ≥4 именованных candidate-пути; помечено «иллюстрация, не финальный список» | READBACK: `codex-plan-06-async-combinations.md` §2 contains the illustration marker and A/B/C/D candidate paths |
| Quality gates (row_validity/plausibility/legal/split/judge/metric/lifecycle/ablation/async/promotion) | `claude-05:102-115` | covered (распределено: блок 07 §10, 08 §10, 11 §10, доска exit-gates) | да | **rej** (дубль) | — | — | `codex-plan-07/08/11` §10 |

### 2.4. Из `claude-07-execution-gates-open-questions.md`

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Stage queue S0–S9 (library→review→Luba accept→20-pass→40-pass→arch→DPO→training→promotion) | `claude-07:17-28` | covered (моя доска Ф0–Ф11 строго последовательная) | да, но мельче | **rej** (моя доска богаче: 59 базовых подшагов, exit-gate каждой фазы, DAG) | — | — | `codex-EXECUTION-CONTROL-BOARD.md` §A |
| Hard gates before implementation (library/authority/legal/source/hierarchy/graph-helper/async/metric/lifecycle/privacy) | `claude-07:32-43` | covered (мои exit-gates Ф0–Ф11 + matrix) | да | **rej** (дубль; privacy/retention — см. T3 acc) | — | — | доска exit-gates |
| **`geography / export constraints` как отдельный legal-gate** | `claude-07:43`-смежно; `claude-02:61` | **absent** (у меня нет поля юрисдикции/экспорта в провенансе) | да (доп. поле) | **acc** (часть T2) | `codex-plan-08` §3.1: доп. поле `GeographyExportConstraint` в `TeacherProvenance` (или в `AllowedTeacherRegistry`) | поле present; реестр teacher несёт `geography_allowed[]`; QA: hosted teacher вне allowed-geography ⇒ QUARANTINE | PENDING |
| Acceptance criteria checklist (required files / prefix / non-empty / header / future-honesty / questions-last / no-git) | `claude-07:47-57` | covered (мои якоря + §E вопросы-в-конце + codex-префикс) | да | **rej** (дубль; процессное) | — | — | мастер §E; якоря всех артефактов |
| 10 open questions для Luba (target project / single-machine vs pair / first DPO target / closed-as-eval / open-weight candidate / pilot threshold / legal sub-agent / personal-private / product UI / library growth) | `claude-07:60-70` | covered (мои §E.1 Q1–Q7 + §E.2 Q8–Q40 — шире и конкретнее) | да | **rej** (мои вопросы конкретнее и привязаны к file:line) | — | — | мастер §E |
| Closed API eval prohibition benchmark item | `claude-07:64` | covered (мой §E.1 Q5 + блок 08 §3.3.8: closed как judge при `!used`) | да | **rej** (у меня уже решено политикой + вынесено в Q5) | — | — | мастер §E.1 Q5 |

### 2.5. Из `claude-00..03` (index / authority ledger / source inventory / pattern notes)

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Authority requirements ledger R-001..R-020 + artifact coverage | `claude-01:19-57` | covered (мой мастер §B.1 R-01..R-19 + матрица R1..R20) | да | **rej** (мой ledger + отдельная матрица покрытия глубже) | — | — | мастер §B.1; матрица §1 |
| Source inventory exact paths (LearningMachineContracts/LogicalContracts/AdaptiveOrchestrator/DynamicDomainGraph/DependencyGraph/DpoPair/DpoTrainer/RoutingDpoPairBuilder/TranslationMachine) | `claude-02:17-30` | covered (мой блок 14 карта 34/34 путей + S1..S12 в чужом claude-плане совпадают) | да | **rej** (дубль; мой блок 14 шире — 6 осей-проектов) | — | — | `codex-plan-14` |
| Future collection phases (20-pass external / 40-pass core / provider terms / model candidate / impl audit) | `claude-02:65-71` | covered (мой R6 + библиотеки + Ф0.1.3 teacher) | да, но процессно | **rej** (у меня покрыто как R6 partial — это процесс, не код) | — | — | матрица §3.1 (R6) |
| ILearningMachine pattern notes (roles table, reuse rules, boundary warning) | `claude-03:38-77` | covered (мой блок 05 + guide-04) | да | **rej** (дубль) | — | — | `codex-plan-05`; `codex-guide-04` |
| Reuse `LearningTrainingItem`/`LearningBatchResult`/context packets/transition signals/dependency levels | `claude-03:66-72` | covered (мой блок 02/05 проекция на `LearningTrainingItem`, блок 03 transition) | да | **rej** (дубль) | — | — | `codex-plan-05` §5 |

### 2.6. Из чужого `claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` (более глубокий план Claude)

> Это самый сильный benchmark-документ Claude (Part A §0–§5, source map, gates). По
> большинству осей он **сопоставим** с моим каноном, но мой канон разнесён на доску +
> матрицу + 16 блоков и потому полнее на execution-контроле. Беру только точечное.

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| Part A §0–§5 (UML+note for, алгоритмы A1–A5, code refs, status, hierarchy) | `claude-…DPO-PLAN-v1.md:213-450` | covered (мой §0 блок 00 + §1/§2/§3 в P-01..P-14 + §5 блок 05) | да | **rej** (паритет; мой Part A разнесён по решённым блокам с Gate B на каждый) | — | — | мастер §C; `codex-plan-00/05` |
| `ordered_phase_queue` статусы done/in-progress/remaining/review-block/blocked | `claude-…:516-539` | covered (моя доска те же статусы + строгий SEQUENTIAL GATING) | да | **rej** (моя доска строже: exit-gate каждой фазы + DAG) | — | — | доска §0.2/§0.3 |
| 20-pass / 40-pass план таблицами (EXT-01..20, CORE-01..40) | `claude-…:541-569` | partial у меня (R6 partial; библиотеки есть, счётчика проходов нет) | да, процессно | **def** (см. §4 D1 — это часть R6 partial; решение Luba) | `codex-knowledge-library-*` (индекс проходов) | счётчик `recon_passes_llm_mvp>=20 / core>=40` зафиксирован как evidence-строка | DEFERRED (нужно решение Luba по трактовке R6) |
| Chosen/Rejected authority таблица (human/legal-teacher/self-judge/deterministic/registry) | `claude-…:454-462` | covered (мой блок 08 §3.3 + блок 06 judge + блок 07 chosen-origin) | да | **rej** (дубль; у меня машина вердикта) | — | — | `codex-plan-08` §3.3 |
| Source state vocabulary ACCEPT/SUPPORT_ONLY/QUARANTINE/REVIEW_REQUIRED/METHOD_ONLY/BLOCK | `claude-…:470-478` | partial (у меня ACCEPT/QUARANTINE/BLOCK/REBUILD; нет `ACCEPT_SUPPORT_ONLY`/`METHOD_ONLY`/`REVIEW_REQUIRED`) | да (расширение enum вердикта, не ломает) | **def** (полезно, но пересекается с моим `KEEP_FOR_*`/`REWRITE_TO_DPO` из скила — нужно решить, какой словарь канон) | `codex-plan-08` §3.3 (расширить домен вердикта) | домен вердикта согласован со скилом `trainable-architecture-decision-SKILL.md:438-445` (KEEP_FOR_BASELINE_ONLY/EVAL_ONLY и т.д.) | DEFERRED (избежать двух конфликтующих словарей вердиктов) |
| Held-out split + leakage controls (50/50, candidate-order permutation, source-disjoint) | `claude-…:482-490` | covered (мой блок 11 + блок 07 permutation + блок 06) | да | **rej** (дубль) | — | — | `codex-plan-11`; `codex-plan-07` §3 |
| Measurement guide families + precedent file:line | `claude-…:505-514` | covered (мой блок 11 §3 с теми же прецедентами + live≠held-out) | да | **rej** (дубль) | — | — | `codex-plan-11` |

### 2.7. Из codex-файлов авторства Claude (evidence)

> `codex-50-cycle-plan-review-supplement.md` и
> `codex-skill-improvement-ideas-planning-review-architecture.md` — автор Claude,
> это **evidence**, не правлю. Большинство их «accepted deltas» **уже есть** в моём
> каноне (Claude сам это фиксирует во Second-Pass: SP-01..SP-50). Беру только то, что
> ещё не закрыто.

| benchmark_item | source | current_target_status | architecture_compatible | accept/reject/defer | destination_file + секция | Gate_B | completion_evidence |
|---|---|---|---|---|---|---|---|
| `25.ModelServer` несёт upgrade/rollback + checkpoint-manifest-conversion + continue-training metadata → lifecycle-endpoint графа | `codex-50-cycle…:233,240` | **absent** (rollback/upgrade/manifest-schema у меня в lifecycle НЕ выписаны) | **да** | **acc** (T1, ядро) | `codex-plan-10` §lifecycle + `codex-plan-04` §5; доска **Ф4.2.3** | манифест графа несёт: `manifest_id`, `schema_version`, `source_cutoff`, `late_file_queue`, `path_id_strategy`, `rollback_ref`; Gate B: `Rollback(prev)` загружает прошлый checkpoint и даёт прошлую метрику; `schema_version` присутствует | PENDING (read-back lifecycle-секции блока 10) |
| Graph manifest должен нести graph_id/node_ids/edge_ids/edge_authority/policy_version/fan-in-out schema/pair-split | `codex-50-cycle…:242` | partial (typed state есть; манифест графа как отдельная схема — частично) | да | **def** (пересекается с T1; вносить вместе) | `codex-plan-04` §5 (graph manifest schema) | манифест графа перечисляет graph_id/node_ids/edge_ids/edge_authority/policy_version | DEFERRED (внести единым пакетом с T1) |
| Реестр-категория источника per row + row rejection ledger (валидные типы rejected) | `codex-50-cycle…:96-98,31` | partial (вердикт per row есть; агрегированного row-rejection-ledger артефакта нет) | да | **def** (пересекается с T5; низкий приоритет) | `codex-plan-08` §8.4 (метрика) + §3.1 (T5) | row-rejection ledger перечисляет: random-rejected/copied/missing-source/teacher-unknown/split-leak/order-artifact с count | DEFERRED |
| Reproducibility gate (seed/sampling params для stochastic candidate/judge) | `codex-50-cycle…:330-334` (`09.Inference`) | **covered** (Grep `seed\|reproducib` по `codex-plan-*` = 8 файлов: 03/04/06/07/08/11/13/14) | да | **rej** (уже есть у меня) | — | — | `codex-plan-08`/`07`/`06` (seed/sampling уже учтены) |
| Strict allowed-level decision matrix (5 уровней, всё остальное internal detail) | `codex-50-cycle…:38` | covered (мой §A.3 + блок 12 + A-LM-12) | да | **rej** (дубль) | — | — | мастер §A.3; `codex-plan-12` |
| `18.CapabilityTraining` как сильнейший LoRA+DPO прецедент | `codex-50-cycle…:198` | covered (мой блок 10 §6 блюпринты A–H ссылаются на capability training) | да | **rej** (уже использую) | — | — | `codex-plan-10` §6 |
| Skill-improvement идеи Claude (named_library_population_gate, review-count ledger, dynamic_graph_trainable_claim_gate) | `codex-skill-improvement-ideas-planning-review-architecture.md:108-153` | вне scope (это про скилы, не про мой план) | n/a | **rej** (НЕ цель этого отчёта; новый `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md` — отдельный Codex-owned auxiliary artifact, не этот Claude benchmark row) | — | — | вне scope этого отчёта |

---

## 3. Детализация accept-строк (что именно внести, без правки канона сейчас)

> Каждая accept — **совместима с CANON** и **не ломает** мои инварианты. Внесение —
> отдельная задача (PLAN-ONLY → нужен approve Luba). Здесь — точная цель и Gate B.

### 3.1. T1 (acc) — Lifecycle-полнота манифеста графа: rollback / schema_version / source_cutoff / late_file_queue / path_id_strategy

- **Источник benchmark:** `codex-50-cycle-plan-review-supplement.md:233,240,242` (Claude:
  `25.ModelServer` upgrade/rollback + checkpoint-manifest-conversion + continue-training
  metadata; graph manifest fields). Подкреплено требованием скила
  `trainable-architecture-decision-SKILL.md:96-99` (Graph manifest lifecycle: «manifest_id,
  source cutoff, late-file queue, schema version, … export/import, backup, fresh-load,
  continue-training, and rollback»).
- **Пробел в каноне (доказан Grep):** по `codex-plan-*` `rollback|schema_version|cutoff|late-file`
  = **0 совпадений**; по доске/матрице/мастеру = **0**. Мой lifecycle (блок 10 Ф4.2,
  блок 04) покрывает export/import/fresh-load/continue-training/SHA-256, но **не**
  rollback, schema-версионирование манифеста, source-cutoff и очередь поздних файлов.
- **Совместимость:** да. Это расширение манифеста (данные/метаданные), не новый
  уровень, не новый финальный авторитет — CANON цел.
- **Куда:** `codex-plan-10` §lifecycle (блюпринт D/G) + `codex-plan-04` §5 (graph manifest
  schema); доска **Ф4.2.3** (Gate B манифеста дополнить).
- **Gate B (измеримо):** манифест графа несёт поля `manifest_id`, `schema_version`,
  `source_cutoff`, `late_file_queue`, `path_id_strategy`, `rollback_ref`;
  `Rollback(prev_checkpoint)` загружает прошлый чекпойнт и воспроизводит прошлую
  held-out метрику (тест отката); `schema_version` обязателен и проверяется при import.
- **completion_evidence:** PENDING — read-back lifecycle-секции `codex-plan-10` и §5
  `codex-plan-04` после внесения.

### 3.2. T2 (acc) — `terms_snapshot_ref` + geography/export-constraint как хранимые поля провенанса

- **Источник benchmark:** `claude-04:60` (`terms_snapshot_ref`); `claude-02:61` и
  `claude-07` (geography / export constraints как legal-gate).
- **Пробел:** мой `TeacherProvenance` (блок 08 §3.1/§5) имеет `provider_terms_checked`
  (bool), `model_version`, `request_id`, но **не** ссылку-снимок условий с датой и
  **не** поле юрисдикции/экспорта. Grep `terms_snapshot|geography` по `codex-plan-*` = 0.
- **Совместимость:** да. Доп. поля record + ветка QUARANTINE — это усиление легального
  гейта, прямо в духе CANON-инварианта 2.
- **Куда:** `codex-plan-08` §3.1 и §5 (добавить `TermsSnapshotRef`,
  `GeographyExportConstraint` в `TeacherProvenance`; `geography_allowed[]` в
  `AllowedTeacherRegistry`); доска **Ф2.1.1** (Gate B провенанса дополнить).
- **Gate B:** `open_weight_hosted && teacher_output_used_for_training=true &&
  TermsSnapshotRef==UNKNOWN ⇒ QUARANTINE (reason `terms_snapshot_missing`)`; hosted
  teacher вне `geography_allowed[]` ⇒ QUARANTINE.
- **completion_evidence:** READBACK — `codex-plan-08-legal-teacher-gate.md`
  contains `TermsSnapshotRef` and `GeographyExportConstraint` in `TeacherProvenance`.

### 3.3. T3 (acc) — Явный privacy/retention + redaction gate для hosted-провайдеров

- **Источник benchmark:** `claude-07:43` (`privacy_gate`); `claude-04:26,79,90`
  (raw project text retention, redaction, privacy boundary).
- **Пробел:** у меня приватность закрыта косвенно (R5 adaptive-leg, анти-leakage,
  `RI-03` local/offline boundary), но **отдельного именованного** гейта «raw private
  text → hosted retention/redaction» как строки в блоке нет.
- **Совместимость:** да. Это governance/eval-only гейт данных (как мой блок 08), не
  семантический авторитет — CANON цел; усиливает R5/privacy.
- **Куда:** `codex-plan-08` §8 — новый под-гейт **§8.5 privacy/retention**; ссылка из
  `codex-plan-10` split R5 (Ф4.3.2); доска **Ф2.1.2**.
- **Gate B:** `hosted_provider && raw_private_text_copied=true && redaction_policy_ref==none
  ⇒ BLOCK_FOR_COMMON` (личное — только adaptive leg); поле `raw_private_text_copied`
  и `redaction_policy_ref` присутствуют в провенансе/манифесте.
- **completion_evidence:** READBACK — `codex-plan-08-legal-teacher-gate.md`
  contains privacy/redaction §8.5 with `BLOCK_FOR_COMMON` and `BLOCK_FOR_HOSTED`;
  `codex-plan-10-training-lifecycle.md` contains `LegScope`.

### 3.4. T2-доп. (acc) — geography/export как отдельная ветка реестра teacher

- Сведено в §3.2 (одно поле + `geography_allowed[]` в `AllowedTeacherRegistry`).
  Отдельной задачи не требует; Gate B — там же.

### 3.5. T4 (acc, читаемость) — иллюстративный набор async-комбинаций A/B/C/D

- **Источник benchmark:** `claude-05:92-99`.
- **Пробел:** механизм N-комбинаций есть (`codex-plan-06`, доска Ф6), но **наглядного**
  набора путей-шаблонов в §2 (аналогия) нет — это только улучшение читаемости Part A.
- **Совместимость:** да (пример, не контракт).
- **Куда:** `codex-plan-06` §2.
- **Gate B:** в §2 присутствует ≥4 именованных candidate-пути (A=Encoder→Direct;
  B=Encoder→Critic→Repair; C=parallel Evidence+Risk→Merge; D=Bridge→Critic→Final),
  помеченных «иллюстрация, не финальный список комбинаций».
- **completion_evidence:** READBACK — `codex-plan-06-async-combinations.md` §2
  contains the A/B/C/D candidate-path illustration and marks it as an illustration,
  not the final combination list.

### 3.6. T5 (acc, низкий) — `SourceCategory` как enum-поле провенанса

- **Источник benchmark:** `claude-04:48-49` (`rejected_origin`/source-категории);
  `claude-05:54-61` (judge/source strategy).
- **Пробел:** вердикт строки есть; единой enum-категории происхождения нет.
- **Совместимость:** да (доп. метаданное поле).
- **Куда:** `codex-plan-08` §3.1 (`SourceCategory`:
  local|human|open_weight|hosted|deterministic_baseline|self_judge).
- **Gate B:** поле present с доменом; QA-скан: 0 train-ready строк с
  `SourceCategory=self_judge` без пройденного legal-гейта.
- **completion_evidence:** READBACK — `codex-plan-08-legal-teacher-gate.md`
  contains `public enum SourceCategory`; `codex-plan-07-dpo-builder.md` contains
  `RejectedOrigin`/`rejected_origin` and per-side authority fields.

---

## 4. Отклонённые (rej) и отложенные (def) — со сводными причинами

### 4.1. Главные причины REJECT (кратко)

| Причина reject | Кого касается | Пояснение |
|---|---|---|
| **R-DUP — уже есть в каноне глубже** | большинство строк §2.1–§2.6 | мой Part A/блоки/доска покрывают это с бóльшей глубиной и Gate B; добавлять нечего |
| **R-WEAKER — benchmark мельче, копировать = деградация** | `claude-06` brief; «fast plan» уровень; плоские списки graph-state/routing | Claude сам помечает `claude-*` как «fast scaffold … superseded» (`codex-50-cycle` цикл 47, Second-Pass Summary); беру структуру у себя |
| **R-PROCESS — процессное, не план-артефакт** | acceptance-criteria checklist, future-collection фразы, 10 generic questions | у меня это уже как якоря/§E/матрица; дублировать процессное в план не нужно |
| **R-OUT-OF-SCOPE — не цель этого отчёта** | skill-improvement идеи Claude из `codex-skill-improvement-ideas-planning-review-architecture.md` | правка скилов — отдельная задача с отдельным approve Luba; новый Codex-owned skill-ideas файл имеет отдельный ownership status |
| **R-ALREADY — Claude сам отметил accept, и у меня это есть** | SP-01..SP-50 из `codex-50-cycle`; reproducibility/seed; allowed-level matrix; capability precedent | проверено Grep/чтением: уже в каноне |

### 4.2. DEFER — с причиной и условием закрытия

| benchmark_item | source | причина defer | условие закрытия (решение Luba) |
|---|---|---|---|
| 20-pass/40-pass как таблицы EXT/CORE с Gate B | `claude-…DPO-PLAN-v1.md:541-569` | это часть моего **R6 partial** (мета-процесс рекона, не реализационный подшаг) | Luba решает трактовку R6: принять как выполненный мета-фон, либо добавить evidence-строку счётчика проходов в `codex-knowledge-library-*` (матрица §3.1) |
| Source-state словарь `ACCEPT_SUPPORT_ONLY/REVIEW_REQUIRED/METHOD_ONLY` | `claude-…:470-478` | **конфликт двух словарей вердиктов**: у меня вердикт строки = ACCEPT/QUARANTINE/BLOCK/REBUILD + dataset-вердикты скила `KEEP_FOR_BASELINE_ONLY/EVAL_ONLY/REWRITE_TO_DPO/QUARANTINE_DO_NOT_TRAIN` (`trainable-architecture-decision-SKILL.md:438-445`). Введение третьего словаря = бардак | Luba/архитектор утверждает ЕДИНЫЙ канон вердиктов; тогда либо мапнуть Claude-словарь на скиловый, либо расширить домен блока 08 без дублей |
| Graph manifest полная схема (graph_id/node_ids/edge_ids/edge_authority/policy_version/fan-in-out) | `codex-50-cycle…:242` | пересекается с **T1**; вносить единым пакетом, чтобы не дробить манифест | вносится вместе с T1 (§3.1) одной задачей |
| Row rejection ledger (агрегированный артефакт валидных типов rejected) | `codex-50-cycle…:31,98` | пересекается с **T5**; низкий приоритет; per-row вердикт уже даёт блок 08 | после T5; решить, нужен ли отдельный агрегированный ledger или достаточно метрики §8.4 блока 08 |

---

## 5. Таблица КОНФЛИКТ / НЕ-КОНФЛИКТ (проверка, что accept не ломает CANON)

> Сквозная проверка: каждое accept/defer против трёх инвариантов CANON и против
> «не копировать слабые места Claude».

| Пункт | Затрагивает CANON-инвариант | Конфликт | Обоснование |
|---|---|---|---|
| T1 rollback/schema/cutoff/late-file/path-id | #1 уровень L5 / #3 детерминизм | **НЕ конфликт** | манифест/метаданные lifecycle; не новый уровень, не финальный авторитет ребра |
| T2 terms_snapshot_ref / geography | #2 легальный гейт | **НЕ конфликт** (усиливает) | доп. поля провенанса + ветка QUARANTINE; строго в духе teacher-гейта |
| T3 privacy/retention/redaction | #2 легальный гейт / R5 | **НЕ конфликт** (усиливает) | governance/eval-only гейт данных; личное → adaptive leg (R5) |
| T4 async A/B/C/D набор | #3 детерминизм | **НЕ конфликт** | иллюстрация комбинаций; победитель всё равно через ablation/измерение (блок 06/11) |
| T5 SourceCategory enum | #2 легальный гейт | **НЕ конфликт** | метаданное поле; не влияет на выбор маршрута |
| DEFER: source-state словарь Claude | #2 легальный гейт | **ПОТЕНЦИАЛЬНЫЙ конфликт словарей** | два параллельных словаря вердиктов = бардак; поэтому **defer** до единого канона |
| REJECT: `claude-06` как архитектура | #1/#3 | **конфликт глубины** | его brief мельче; принятие деградировало бы мой §0/блоки — поэтому reject |
| REJECT: minimal `DpoPair` без 14-полей канона | #2 | **конфликт** | мой `GraphDpoPair` (14 полей + provenance) строже; нельзя понижать |
| REJECT: closed-API как teacher «после terms» (мягкая формулировка Claude `claude-04:35`) | #2 | **прямой конфликт** | скил и CANON: closed teacher для distill = **никогда** (`DATASET_REBUILD_REQUIRED`), даже с terms; closed только judge. Принять нельзя |

> **Особо (защита CANON):** строка `claude-04:35` «Closed API provider … possible future
> allowed use: Limited judge or evidence route only after explicit terms» — у меня это
> уже жёстче (closed = только judge при `used_for_training=false`; teacher-distill
> закрытой модели = BLOCK/REBUILD без исключений, блок 08 §3.3 шаг 2). Любая мягкая
> «teacher after terms» формулировка Claude **отклонена**, чтобы не ослабить легальный
> гейт.

---

## 6. Сводка для StructuredOutput / следующего шага

```text
benchmark_compared        = claude-00..07 + claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md
                            + codex-50-cycle-plan-review-supplement.md
                            + codex-skill-improvement-ideas-planning-review-architecture.md (evidence, Claude)
codex_owned_auxiliary     = codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md
my_canon_compared         = codex-ILEARNINGDYNAMICGRAPH-PLAN-v1 + EXECUTION-CONTROL-BOARD
                            + REQUIREMENTS-COVERAGE-MATRIX + codex-plan-00..14 + guides
overall                   = мой канон системно глубже; брать только точечные совместимые добавки
accepted_count            = 6 (T1, T2-terms_snapshot, T2-geography, T3, T4, T5)
deferred_count            = 4 (20/40-pass как R6; source-state словарь; graph-manifest-схема с T1; row-rejection ledger)
rejected_class            = дубли / мельче / процессное / out-of-scope / уже-есть / ослабляющие легальный гейт
canon_broken              = false (см. §5 — конфликтов с инвариантами нет; ослабляющие отклонены)
all_accept_status         = PARTIAL_AFTER_CYCLE_30_REPAIR (T2/T3/T4/T5 integrated in target bodies; T1 still needs full manifest read-back)
claude_files_edited       = false
codex_claude_files_edited = false
only_this_report_written  = true_for_original_BDR_pass; false_after_later_cycle_integrations
```

*Конец Benchmark Delta Report. PLAN-ONLY / EVIDENCE-аналитика. Не разрешает реализацию
продукта/кода, тренировку, билд, запуск раннеров, git или вызов внешних моделей.
Внесение accept-строк в мой канон `codex-*` — отдельная задача с отдельным approve
Luba; до этого все accept = PENDING. Чужие `claude-*` и codex-файлы авторства Claude —
evidence-only, не редактировались.*
# FIRST-SWEEP GPT-5.5 REVIEW AMENDMENT POINTER

This Benchmark Delta Report is amended by `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md`, based on 25 verified GPT-5.5/xhigh review packets recorded in `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`.

Historical benchmark-delta overlay from the earlier sweep; not current-run
completion proof for the 2026-06-24 Claude-comparison ledger:

```text
historical_first_sweep_packet_rows = 50/50
current_run_completion_proof_from_this_file = false
claude_attachment_status = evidence_only
claude_files_edited = false
T2_terms_snapshot_geography = integrated_in_target_body_plan08
T3_privacy_redaction_retention = integrated_in_target_body_plan08_plan10
T4_async_examples_fanin_many_to_many = integrated_in_target_body_plan06
T5_SourceCategory_rejected_origin = integrated_in_target_body_plan07_plan08
all_accept_status = PARTIAL_T1_REMAINING_READBACK
```

If older BDR rows say T2/T3/T4/T5 are `PENDING`, read them as pre-cycle-30 state. Current cycle-50 repair status is integrated for T2/T3/T4/T5 and partial for T1 until implementation-time manifest lifecycle read-back is completed.
