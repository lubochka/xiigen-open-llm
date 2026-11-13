# Доска исполнения ILearningDynamicGraph — строго последовательная (codex-EXECUTION-CONTROL-BOARD)

> **Артефакт:** `codex-EXECUTION-CONTROL-BOARD.md`
> **Автор-роль:** архитектор доски исполнения (Opus 4.8 MAX).
> **Тип артефакта:** ПЛАН-ONLY (документация-решение, перестройка плана в строгую
> исполнительную доску). Это **НЕ** разрешение реализовывать продукт/код, тренировать,
> билдить, запускать раннеры, git или вызывать внешние AI/API/модели. Реализация
> требует свежего явного approve Luba после ревью.
> **Источник:** мастер-план `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` + 16 решённых
> блоков (`codex-plan-00..14`). Чужие `claude-*`/`dcm-*` — evidence-only, не правятся.
> **Язык:** русский (человеческим языком); имена типов/полей — английский.

---

## ЯКОРЬ ЗАДАЧИ (CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY)

```text
CURRENT TASK:  перестроить план ILearningDynamicGraph в строго последовательную
               исполнительную доску с трёхуровневой нумерацией Ф<P>.<SP>.<SS>
               (Фаза.ПодФаза.ПодШаг), exit-gate на каждую фазу, статусами
               execution-control, EXECUTION-VISIBILITY, сводными счётчиками и
               картой зависимостей (DAG). Записать ровно в
               codex-EXECUTION-CONTROL-BOARD.md.
NOT THE TASK:  исполнять/тестировать/билдить код, запускать раннеры, тренировать,
               вызывать внешние AI/API/модели, git, продолжать/править чужие
               claude-*/dcm-*/не-свои codex-файлы, дублировать содержимое блоков
               (доска ИНДЕКСирует блоки и упорядочивает их, а не копирует).
EVIDENCE-ONLY: 16 блоков codex-plan-*, мастер-план codex-ILEARNINGDYNAMICGRAPH-*,
               codex-recon-*, codex-guide-*, чужой claude-ILEARNINGDYNAMICGRAPH-*,
               концепт-доки llm dynamic flow/, транскрипты, STATE-файлы.
```

---

## 0. Как читать эту доску (легенда нумерации и статусов)

### 0.1. Нумерация — `Ф<P>.<SP>.<SS>`

- **Ф`<P>`** — Фаза `P` (верхний уровень исполнения). Всего фаз: **Ф0…Ф11** (12 фаз).
- **`.<SP>`** — ПодФаза `SP` внутри фазы `P` (средний уровень, группа работ).
- **`.<SS>`** — ПодШаг `SS` внутри подфазы (нижний уровень, атомарная единица работы
  с собственным Gate B).

Каждый ID **уникален**, **без дублей**, **без дыр**: подфазы фазы идут `1,2,3,…`
подряд; подшаги подфазы идут `1,2,3,…` подряд. Пример: `Ф1.2.3` = Фаза 1, ПодФаза 2,
ПодШаг 3.

### 0.2. Статусы (execution-control)

| Статус | Значение |
|---|---|
| `done` | Подшаг/подфаза/фаза завершены, Gate B пройден на 100%, evidence записан. |
| `in-progress` | Сейчас исполняется (ровно один активный подшаг во всём плане — единственная глобальная дорожка исполнения; см. §0.5). |
| `remaining` | Ещё не начато (нельзя начать, пока не выполнены зависимости и не открыта фаза). |
| `review-block` | Исполнение сделано, но REVIEW_INTERNAL нашёл gap; вернулось в ремонт. |
| `blocked` | Заблокировано внешней причиной: недоступный ассет, отсутствие authority/approve, внешняя зависимость или ожидание конкретного решения Luba (напр. Ф10.3.* ждут Luba Q41=Вариант B). НЕ применяется к Ф0: Ф0 — нормальная первая фаза, на момент плана = `remaining`, открывается как `in-progress` при старте. |

> **На момент плана ВСЁ = `remaining`** (это план-only; ни один реализационный
> подшаг не начат и начнётся только после approve Luba).

### 0.3. Правило строгой последовательности (жёсткое, для всех уровней)

```text
ПРАВИЛО СТРОГОЙ ПОСЛЕДОВАТЕЛЬНОСТИ (SEQUENTIAL GATING):

ЕДИНАЯ СЕМАНТИКА ПОРЯДКА (разрешение конфликта индекс vs DAG): закон исполнения —
строгий тотальный порядок по индексу Ф<P>.<SP>.<SS>, И этот индекс-порядок СОГЛАСОВАН
с частичным порядком «Зависит от» (DAG): для baseline-доски «Зависит от» никогда не
ссылается вперёд по индексу, поэтому индекс и DAG дают ОДИН ответ на «какой ровно один
подшаг сейчас». Индекс — это и порядок исполнения, и метка; «Зависит от» — это
дополнительные кросс-фазовые предпосылки, которые не противоречат индексу, а уточняют
готовность (если ID из «Зависит от» не `done`, подшаг ждёт, даже когда индекс-предшественник
уже закрыт).

1. Фаза N+1 НЕ начинается, пока exit-gate Фазы N не пройден на 100%.
2. ПодФаза SP+1 внутри фазы НЕ начинается, пока exit-gate ПодФазы SP не пройден.
3. ПодШаг SS+1 внутри подфазы НЕ начинается, пока Gate B ПодШага SS == PASS.
4. Дополнительно: если у подшага есть запись «Зависит от» с ID из ДРУГОЙ фазы/подфазы —
   этот подшаг не стартует, пока ВСЕ перечисленные ID не в статусе `done` (это
   предпосылка СВЕРХ индекс-порядка, а не альтернатива ему; она никогда не открывает
   подшаг раньше его индекс-предшественника).
5. Любой проваленный Gate B → статус `review-block`, исполнение возвращается в
   ремонт ЭТОГО же ID; следующий ID не открывается.
6. Единственное исключение порядка — Ф0: её exit-gate (ответы Luba) обязателен
   перед стартом Ф1; внутри Ф0 подшаги могут собираться параллельно (это сбор
   вопросов/решений Luba, а не реализация на дорожке исполнения), но фаза
   закрывается только когда ВСЕ ответы есть.
```

Это правило — закон доски. Доска спроектирована так, что в каждый момент времени
видно ровно один `in-progress` ID **во всём плане** (единственная глобальная дорожка
исполнения, §0.5), а весь «хвост» — `remaining`.

### 0.3.1. Repository branch authority pre-flight (binding amendment)

Перед стартом реализации во **всех участвующих репозиториях** authoritative source
ref выбирается как **самый последний updated branch** по inspectable branch metadata.
Рабочее имя новой ветки будущей реализации: `DPOLegalLearning`.

Candidate participating repo identifiers to verify/update during this pre-flight:
`llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`,
`FigmaToCode-design-gen`, `CanvaToCode`. These identifiers are a planning
candidate list, not proof that branch metadata has already been inspected.

Gate B до открытия Ф1:

| Поле evidence | Требование |
|---|---|
| `repo_path_or_url` | Каждый участвующий repo перечислен явно. |
| `branch_list_source` | Команда/API/снимок, из которого взят список branch metadata. |
| `selected_authority_branch` | Самый последний updated branch для этого repo. |
| `selected_branch_updated_at` | Timestamp/ref evidence, по которому branch выбран. |
| `divergence_note` | Кратко: есть ли расхождение с default/current branch и нужен ли merge/rebase decision. |
| `future_working_branch_name` | Ровно `DPOLegalLearning`. |

Это **не** разрешает создавать или переключать ветку сейчас. Создание/checkout/push
разрешаются только отдельным execution work-order после approve Luba.

### 0.4. Поля строки подшага

Каждый подшаг `Ф<P>.<SP>.<SS>` описан строкой-таблицей с полями:

`Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от (ID) | Статус | Тест-слои | Блок-источник`.

- **Owner** — три класса владельцев (не «все суб-агенты»; легенда обязана совпадать
  с реальными ячейками таблицы). **Ячейка Owner несёт РОЛЬ-КЛАСС** (`sa-contracts`,
  `sa-node`, …, `sa-api`, `Luba (решение)`, `архитектор`), а конкретный НИК суб-агента
  назначается в bounded work order на этапе исполнения и записывается в snapshot §B.1
  (`OWNER ТЕКУЩЕГО ПОДШАГА`), когда подшаг переходит в `in-progress`. Безусловного «ник
  дан в строке» нет, пока подшаг не стартовал:
  - **(a) реализационные подшаги Ф1…Ф10 (54 шт., вкл. Ф4.3.1)** — исполнители суб-агенты
    `gpt-5.5 / xhigh` (CLAUDE.md Architect/Subagent firewall); в ячейке — роль-класс
    `sa-*`, ник — в work order/snapshot при старте;
  - **(b) подшаги Ф0.1.* (3 шт.) — канон-решения** — owner=`Luba (решение)`
    (это РЕШЕНИЯ Luba, а не реализация и не суб-агент-работа);
  - **(c) подшаги Ф11 (2 шт.) — финальная приёмка/ревью** — owner=архитектор
    (Opus 4.8 MAX): Ф11.1.1 — doc/governance-ревью архитектора (completion coverage
    matrix); **Ф11.2.1 — SPLIT: review-gate/финальный вердикт владеет архитектор, а сами
    10 независимых execution-ревью ИСПОЛНЯЮТ суб-агенты `gpt-5.5 / xhigh` (10 отдельных
    пакетов)**; архитектор владеет только финальным вердиктом
    `ARCHITECT_VERDICT_ALLOW_FINAL`.
  - Разбиение owner-классов: **54 суб-агент-исполнителя (Ф1–Ф10; +1 = Ф4.3.1) + 3
    Luba-решения (Ф0) + 2 архитектор-owned (Ф11, из них Ф11.2.1 — split: gate=архитектор,
    10 ревью=суб-агенты) = 59**.
  - **Reconciliation R9 (две грани):** (1) «через суб-агентов» в смысле РЕАЛИЗАЦИИ = 54
    исполнителя Ф1–Ф10 (после добавления Ф4.3.1; ранее было 53); (2) Ф0.1.1-3 (3) — это
    РЕШЕНИЯ Luba, НЕ суб-агент-работа и НЕ реализация — законное исключение из R9;
    (3) Ф11.1.1 + Ф11.2.1-gate (2) — архитектор-ревью/вердикт, причём 10 execution-ревью
    Ф11.2.1 исполняют суб-агенты (архитектор владеет только токеном
    `ARCHITECT_VERDICT_ALLOW_FINAL`). **Каноническое число суб-агент-исполнителей
    реализации = 54; число 56 НЕ каноническое** (возникает только при ошибочном счёте Ф0
    как суб-агент-работы; даже с Ф4.3.1 реализационных исполнителей 54, а не 56). Авторский facet R9 (написание
    самого плана суб-агентами) трассируется на мастер R-13 + R20-протокол (реальные
    пакеты), а НЕ на execution-owner-колонку доски.
  - Архитектор (Opus 4.8 MAX) дополнительно владеет Ф0-gate, ревью и оркестрацией
    всего исполнения.
- **Тест-слои** — слои FC-26 для подшага (U=unit, BL=business-logic, INT=integration,
  E2E=e2e-host, QA=QA-automation, NEG=negative, UH=untrained-honesty,
  TA=trained-acceptance, REG=regression, UX=ux-automation).
- **Блок-источник + нота R11 (референс на каждый этап, исполняемый слой):** колонка
  «Блок-источник» каждого подшага = источник канона (`NN §...`) + код `file:line`. Чтобы
  R11 «референсы на каждый этап» закрывался на исполняемом слое (а не только для Ф10),
  каждый подшаг `Ф<P>.<SP>.<SS>` резолвится к ПРОЕКТАМ+ДОКУМЕНТАЦИИ затрагиваемых
  компонентов через crosswalk `codex-plan-14 §11.<SR>`: исполнитель берёт «Блок-источник»
  подшага → находит соответствующую SR-строку (project + README/docs/CONTRACT) в plan-14
  §11. Зависимость: plan-14 §11.0 обязан нести явную карту «каждый Ф<P>.<SP>.<SS> → SR-NN»
  для ВСЕХ 59 подшагов (не только Ф10.*); plan-14 в этом задании не редактируется —
  пункт зафиксирован как зависимость доски.

### 0.5. Дорожка исполнения (определение — устраняет неоднозначность «дорожки»)

**Дорожка исполнения ровно ОДНА и глобальная.** На всю доску в каждый момент времени
существует не более одного `in-progress` подшага (`in-progress P ∈ {0,1}` относится ко
ВСЕЙ доске, а не к группе). Параллельных дорожек реализации НЕТ. Это делает формулы
счётчиков §B.2 (`done + in-progress + remaining + review-block + blocked = 59`)
арифметически корректными без суммирования по нескольким дорожкам и сохраняет инвариант
строгой последовательности «каждая фаза/подфаза 100% до следующей».

Единственное допускаемое «параллельно» — сбор решений Luba в Ф0 (§0.3 правило 6): это
сбор ответов/решений, а НЕ работа на дорожке исполнения, поэтому он не создаёт второго
`in-progress` подшага реализации и не нарушает «ровно один in-progress во всём плане».

---

## 1. Сводные счётчики

| Метрика | Значение |
|---|---|
| Всего фаз (Ф0…Ф11) | **12** |
| Всего подфаз | **26** |
| Всего подшагов | **59** (включает Ф4.3.1 — обучаемый узел `level=unit`, прямой субъект R1) |
| `done` | **0** |
| `in-progress` | **0** |
| `remaining` | **59** |
| `review-block` | **0** |
| `blocked` | **0** (на момент плана Ф0 = `remaining` и откроется как `in-progress` при старте исполнения; `blocked` зарезервирован только для внешних причин; реализация всех фаз ждёт approve Luba) |
| Решённых блоков-документов (источник) | **16** (Part A §0/§4/§5 + P-01..P-14) |

> Счётчики подфаз/подшагов по фазам (для самоконтроля без дыр):
> Ф0=1 подфаза/3 подшага · Ф1=3/9 · Ф2=2/5 · Ф3=2/5 · Ф4=3/8 · Ф5=2/5 ·
> Ф6=2/4 · Ф7=2/4 · Ф8=2/4 · Ф9=3/6 · Ф10=2/4 · Ф11=2/2.
> Σ подфаз = 1+3+2+2+3+2+2+2+2+3+2+2 = **26**.
> Σ подшагов = 3+9+5+5+8+5+4+4+4+6+4+2 = **59** (Ф4 = 8: Ф4.1.1-2, Ф4.2.1-3, Ф4.3.1/2/3).

---

## 2. Карта зависимостей фаз (DAG) с обоснованием порядка

```text
Ф0  Решения Luba (§E как exit-gate)
      │  (канон уровня L5, схема GraphDpoPair, teacher-список, judge-политика,
      │   размещение common/adaptive, судьба baseline-роутеров — без них нельзя
      │   фиксировать контракты и легальный гейт)
      ▼
Ф1  Контракты и фундамент графа (узел/ребро/оркестратор/L5-машина/домен/топология)
      │  (NEW-типы и маппинг уровней — скелет, на который ложится всё остальное)
      ▼
Ф2  Легальный teacher-гейт + provenance + канон GraphDpoPair
      │  (ни одна строка не обучает без легального провенанса; формат пары — общий)
      ▼
Ф3  DPO-строитель (GraphDpoBuilder: chosen/rejected/[WRONG:], 5 форматов → 1 канон)
      │  (производит обучающие данные; нужен legal-гейт Ф2 как встроенный шаг)
      ▼
Ф4  Обучение + lifecycle (LoRA wiring, margin из движка, checkpoint/export/import/locator, split R5)
      │  (реальный сдвиг весов; нужны пары Ф3 и легальные данные Ф2)
      ▼
Ф5  Измерение (held-out / ablation / permutation / two-PASS)
      │  (доказывает обучаемость числом; нужен обученный узел Ф4)
      ▼
Ф6  Async мульти-комбинации (1 вход → N комбинаций → лучший)
      │  (генератор тренировочного сигнала; опирается на оркестратор Ф1, измерение Ф5)
      ▼
Ф7  Авто-DPO петля (интеграция Ф2-Ф6 в замкнутый цикл само-улучшения)
      │  (прогон → пара → обучение → измерение → промоушен → повтор)
      ▼
Ф8  Замена скрытого детерминизма (cutover baseline-роутеров → обучаемые юниты)
      │  (после того как обучаемый путь Ф4-Ф7 доказан — переключаем финал ребра)
      ▼
Ф9  Тесты/QA полная матрица (FC-26 по 14 строкам P-01..P-14, per-edge контроли)
      │  (сквозная верификация всего, что построено в Ф1-Ф8)
      ▼
Ф10 API/клиенты (figma→html+css; better-output→DPO; тонкий агент R-11)
      │  (прикладные швы наружу; ядро в llm_mvp_core)
      ▼
Ф11 Финальная приёмка (full completion gate + 10 execution reviews)
```

**Обоснование порядка (почему именно так):**

1. **Ф0 первой** — канон (уровень L5, поля `GraphDpoPair`/`TeacherProvenance`,
   teacher-список, размещение ассетов) повторяется как открытый вопрос почти в каждом
   блоке (Q1 во всех 16). Без ответов нельзя зафиксировать ни контракты, ни легальный
   гейт — иначе переделка всего скелета.
2. **Ф1 раньше Ф2-Ф4** — NEW-типы (`IDynamicGraphNode/Edge`, `DynamicGraphDomain`,
   `DynamicGraphTopology`, `DynamicGraphMachine`) — это скелет; на него ссылаются и
   `GraphDpoPair`, и legal-гейт, и обучение (§C.5 мастер-плана).
3. **Ф2 раньше Ф3** — legal-гейт встроен в `GraphDpoBuilder` шагом 1 (блок 07 §3
   Шаг 1; блок 08 — владелец гейта). Строить пары без гейта = риск контаминации.
4. **Ф3 раньше Ф4** — обучение (`TrainBatchAsync`) кормится парами `GraphDpoPair`
   (блок 10 §3 Шаг 1; блок 09 Шаг 4). Нет пар — нечем обучать.
5. **Ф4 раньше Ф5** — измерение меряет обученный узел; пустую заглушку мерить
   нечем (блок 11 §3 вход).
6. **Ф6 после Ф5** — async-арбитр выбирает «лучший» и доказывает это измерением
   (блок 06 §8 G4 — метрики на стороне L5/Ф5).
7. **Ф7 после Ф2-Ф6** — петля по определению интегрирует все предыдущие (блок 09
   §1: связывает P-06/P-07/P-08/P-10/P-11 в один цикл).
8. **Ф8 после Ф7** — cutover (замена baseline на обученный финал ребра) безопасен
   только когда обучаемый путь доказан; иначе сломаем рабочий baseline (блок 12
   §E Q5: удаление ≠ покрытие; baseline остаётся для ablation).
9. **Ф9 после Ф8** — полная тест-матрица верифицирует уже построенное (блок 13 —
   поперечный, но финальная матрица собирается после cutover).
10. **Ф10 после Ф9** — прикладные швы (figma→html, better-output→DPO) подключают
    проверенное ядро; ядро остаётся в llm_mvp_core (блок 14, R5/R6).
11. **Ф11 последней** — full completion gate + 10 execution reviews только когда
    всё построено и проверено.

> Внутри Ф2..Ф6 есть «горизонтальные» зависимости между блоками (08↔07, 07↔06,
> 11↔09/10): они отражены в поле «Зависит от» каждого подшага, а не нарушают
> вертикальный порядок фаз.

---

# §A. ФАЗЫ — детально (название, что решает, exit-gate, подфазы, подшаги)

---

## Ф0 — Решения Luba (канон как exit-gate)

**Что решает (человеческим языком):** фиксирует все канонические решения, без которых
нельзя проектировать контракты и легальный гейт. Это семь решающих вопросов §E
мастер-плана плюс per-block остаток. На момент плана — все = `remaining`/`blocked`:
ответы даёт Luba.

**Exit-gate Ф0 (измеримо — фаза завершена, когда):**
- получены ответы Luba на **все 7 решающих вопросов** (Q1 уровень L5; Q2 канон
  `GraphDpoPair`; Q3 место `TeacherProvenance`; Q4 teacher-список + судьба `sonnet-*`;
  Q5 judge-политика; Q6 размещение common/adaptive; Q7 судьба `MultiSubPhaseRouterUnit`);
- закрыт **конечный (не «минимум»)** список блокирующих per-block вопросов с lock-флагом
  и явной картой «вопрос → первый зависимый подшаг» (так `decisions_open_count == 0`
  вычислим, а DAG-рёбра Ф0→{Ф1.x, Ф4.3.2, Ф5.x, Ф10.1.1, Ф10.2.1, Ф10.2.2} явные, а не
  подразумеваемые):
  - **Q8** строение выхода `IQueryable` vs массив → `output_shape_locked==true`
    (владелец Ф0.1.2; первый зависимый подшаг Ф1.2.1);
  - **Q11** порог просадки ablation → `ablation_threshold_locked==true`
    (владелец Ф0.1.3; первые зависимые подшаги Ф4.3.2, Ф5.2.1);
  - **Q14** обучаемость порядка обхода → `traversal_order_trainability_locked==true`
    (владелец Ф0.1.2; первый зависимый подшаг Ф1.3.2);
  - **Q30** порог held-out accuracy приёмки → `heldout_accuracy_threshold_locked==true`
    (владелец Ф0.1.3; первые зависимые подшаги Ф5.1.x/Ф5.2.1, exit-gate Ф5);
  - **Q37** origin better-output клиентов (FLOW-38/Smart*) — open-weight permissive ИЛИ
    следы закрытых → `better_output_origin_policy_locked==true`
    (владелец Ф0.1.3; первый зависимый подшаг Ф10.2.1; матрица §3.5 трактует Q37 как
    блокер R17);
  - **Q38** политика лицензий выходов клиентов (common vs user-adaptive, R5) →
    `output_license_policy_locked==true` (владелец Ф0.1.3; первый зависимый подшаг
    Ф10.2.2);
  - **Q-host (R16)** место шва дизайн→код: A=`xiigen-flows/FLOW-31` каноническое место
    шва (xiigen-platform — хост marketplace-клиентов); B=новый design→code endpoint в
    `xiigen-platform` → `figma_host_project_locked==true`
    (владелец Ф0.1.3; первый зависимый подшаг Ф10.1.1);
- закрытие Ф0 формализовано как конечный чек-лист:
  **Ф0 закрыта ⇔ Q1..Q7 отвечены И {Q8,Q11,Q14,Q30,Q37,Q38, Q-host} locked**;
- `decisions_open_count == 0` по этому конечному набору (Q1..Q7 + 7 lock-флагов);
  никаких открытых «минимум …»-остатков.

**Подфазы:** Ф0.1 (сбор и закрытие решающих вопросов).

### Ф0.1 — Решающие вопросы и канон

**Что решает:** собрать 7 решающих + блокирующий per-block остаток в один список и
получить ответы Luba.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф0.1.1 | Канон уровня: `ILearningDynamicGraph ≡ ILearningMachine<DynamicGraphDomain>` (L5), без 6-го уровня (A-LM-12) | §E Q1 мастер-плана; `DECISIONS-LOCKED.md:35` | Ответ Luba «да/нет/новый уровень»; запись в STATE `canon_level` | `canon_level` зафиксирован одним значением; если «новый уровень» → отдельная задача правки `DECISIONS-LOCKED.md` создана | Luba (решение) | — | remaining | — (decision) | мастер §E.1 Q1; 00/04/05 §12 |
| Ф0.1.2 | Канон формата: поля `GraphDpoPair` (14 обязательных) + `TeacherProvenance` (где хранить: строка/батч/манифест) **+ владелец Q8 (строение выхода `IQueryable` vs массив) + владелец Q14 (обучаемость порядка обхода)** | §E Q2/Q3; per-block Q8/Q14; блоки 06/07/08/09 §5 | Утверждённый набор полей; решение «боковой DPO-манифест + проекция на `LearningTrainingItem`»; зафиксированные значения Q8 и Q14 | `graphdpopair_fields_locked == true`; `teacherprovenance_storage` = одно из {row,batch,manifest}; **`output_shape_locked == true` (Q8; зависимый Ф1.2.1)**; **`traversal_order_trainability_locked == true` (Q14; зависимый Ф1.3.2)** | Luba (решение) | — | remaining | — (decision) | мастер §E.1 Q2/Q3; per-block Q8/Q14 |
| Ф0.1.3 | Канон легальности и политик: teacher-список (Qwen3/DeepSeek/Kimi/GLM/Mistral + провайдеры), judge-политика (closed только judge), размещение common/adaptive (R5), судьба baseline-роутеров (понизить, не удалять), судьба `sonnet-*` (REBUILD) **+ владелец Q11 (порог просадки ablation) + владелец Q30 (порог held-out accuracy) + владелец Q37 (origin better-output клиентов: open-weight permissive vs следы закрытых, R17) + владелец Q38 (политика лицензий выходов клиентов, R5) + владелец Q-host (место шва дизайн→код xiigen-flows/FLOW-31 vs новый endpoint в xiigen-platform, R16)** | §E Q4/Q5/Q6/Q7; per-block Q11/Q30/Q37/Q38/Q-host; R16/R17; блок 08 §12 | Замороженный allow-list teacher/провайдеров; judge allow-list; директория common+adaptive; решение по `MultiSubPhaseRouterUnit`=baseline; решение `sonnet-*`=DATASET_REBUILD_REQUIRED; зафиксированные пороги Q11/Q30, политика лицензий Q38, политика origin Q37, место хоста Q-host | `teacher_allowlist_frozen == true`; `judge_policy_set == true`; `asset_split_dirs_set == true`; `baseline_router_fate == "demote_keep_for_ablation"`; `sonnet_rows_fate == "DATASET_REBUILD_REQUIRED"`; **`ablation_threshold_locked == true` (Q11; зависимые Ф4.3.2, Ф5.2.1)**; **`heldout_accuracy_threshold_locked == true` (Q30; зависимые Ф5.1.x/Ф5.2.1, exit-gate Ф5)**; **`better_output_origin_policy_locked == true` (Q37; зависимый Ф10.2.1)**; **`output_license_policy_locked == true` (Q38; зависимый Ф10.2.2)**; **`figma_host_project_locked == true` (Q-host; зависимый Ф10.1.1)** | Luba (решение) | — | remaining | — (decision) | мастер §E.1 Q4-Q7; per-block Q11/Q30/Q37/Q38/Q-host; R16/R17 |

---

## Ф1 — Контракты и фундамент графа

**Что решает:** вводит типизированный скелет графа как контракты (узлы, рёбра,
топология, домен, L5-машина) и фиксирует маппинг ролей Luby на реальные уровни.
Превращает «поведение оркестратора» в явные типы fan-out/fan-in/many-to-many/return-loop.

**Exit-gate Ф1 (измеримо):**
- все NEW-контракты определены с сигнатурами и помечены NEW vs существующее с
  `file:line` (блоки 00/05/01/02/03/04 §5);
- число введённых новых архитектурных уровней == **0** (A-LM-12: DynamicGraph =
  домен L5);
- маппинг ролей с `file:line` (00.Core/LogicalContracts.cs:97/166/202,
  LearningMachineContracts.cs:208) зафиксирован;
- 0 правок сигнатур существующих контрактов L1..L5 (совместимость);
- узел/ребро БЕЗ Train-метода (A-LM-02): счётчик Train-методов в обёртках == 0;
- регрессия: baseline ≥ 1488 тестов не падает от введения NEW-контрактов.

**Подфазы:** Ф1.1 (NEW-контракты графа), Ф1.2 (узел L2), Ф1.3 (ребро L3 + L4/L5 связка).

### Ф1.1 — NEW-контракты графа (скелет типов)

**Что решает:** ввести `DynamicGraphDomain`, `IDynamicGraphNode`, `IDynamicGraphEdge`
(+канон-enum `GraphEdgeKind` + `string AuthorityUnitId`), `DynamicGraphTopology` как
сигнатуры (0 правок 00.Core). **КАНОН ребра (мастер §A.4.1):** имя enum — `GraphEdgeKind`
(РОВНО 5 членов `{FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop}`); устаревшие
псевдонимы `EdgeKind`/`AuthorityKind`/поле `AuthorityUnit`/член `AsyncParallel` НЕ
используются; async-параллельность ребра — булев флаг `ParallelEligible`, а не член enum
(мастер §A.4.1 стр.178; §E Q13 стр.758).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф1.1.1 | `DynamicGraphDomain` (sealed marker TDomain) + маппинг ролей L2/L3/L4/L5 | Ф0.1.1; `LearningMachineContracts.cs:208` | NEW `DynamicGraphDomain`; таблица маппинга с file:line | тип компилируется как marker; маппинг 4 ролей с file:line; 0 новых уровней | sa-contracts | Ф0.1.1 | remaining | U, REG | 00 §5.2; 05 §5.2; 04 §5.2 |
| Ф1.1.2 | `IDynamicGraphEdge` (+канон-enum `GraphEdgeKind` + `string AuthorityUnitId` + `IReadOnlyList<string> ToNodeIds` для multi-target) — типизированное ребро fan-out/fan-in/many-to-many/multi-output/return-loop + булев флаг `ParallelEligible` | Ф0.1.1; `DynamicDomainGraph.cs:3`; **канон — мастер §A.4.1** (блоки 02/03/04/05 §5 обязаны выровняться на него) | NEW `IDynamicGraphEdge`, канон-enum `GraphEdgeKind {FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop}`, поле `string AuthorityUnitId`, поле `IReadOnlyList<string> ToNodeIds` (multi-target, как plan-04/plan-00), флаг `ParallelEligible` | **РОВНО 5 видов ребра в enum `GraphEdgeKind`** (источник канона — мастер §A.4.1; блоки 02/03/04/05 выравниваются: блок-03 одиночный `ToNodeId` расширяется до `ToNodeIds`, блок-04 5-членный набор дополняется `ReturnLoop`); `AuthorityUnitId` (string) ссылается на обучаемый L2; `ParallelEligible` (bool) выражает async-режим без члена enum; `DeterministicFinalAuthority` поле present (=false в проде) | sa-contracts | Ф1.1.1 | remaining | U, BL, REG | мастер §A.4.1 (канон); 02 §5; 03 §5; 04 §5.2 |
| Ф1.1.3 | `DynamicGraphTopology` (мутируемая на лету проекция реестра) + `IDynamicGraphNode` (обёртка узла) | Ф0.1.1; `LogicalTopologyRegistry.cs:69/105`; блок 04 §3.5/§5.2 | NEW `DynamicGraphTopology` (Add/Mutate), `IDynamicGraphNode`, `BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)` | топология читается через адаптер `BuildDynamicDomainTransitions` **и** строится/мутируется через `BuildDynamicTopology(signal,budget)`; Gate B: `source_kind` allowed, `budget.MaxNodes/MaxEdges/MaxDepth/MaxParallelBranches` checked, `budget_exceeded` blocks mutation, mutation evidence recorded; обёртка БЕЗ Train | sa-contracts | Ф1.1.2 | remaining | U, BL, INT, REG | 00 §5.2; 04 §3.5/§5.2; 05 §5.2 |

### Ф1.2 — Узел графа (L2, «упрощённая матрица»)

**Что решает:** обёртка `IDynamicGraphNode` поверх `ILogicalUnit` без Train; алгоритм
features·weights→top-N + honest `NoDecision`; миграция существующих юнитов.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф1.2.1 | Контракт узла: `IDynamicGraphNode<TInput,TOutput>` + `DynamicGraphNodeDescriptor` + `GraphNodeCandidate` (many→many) | Ф1.1.3; `LogicalContracts.cs:97/62/72` | NEW узловые типы; решение `IQueryable` vs массив (Ф0 Q8) | Train-методов в обёртке == 0 (A-LM-02); descriptor несёт `IsTrained/AuthorityRole/CheckpointRef`; выход = коллекция (many→many) | sa-node | Ф1.1.3, Ф0.1.2 | remaining | U, REG | 01 §5 |
| Ф1.2.2 | Алгоритм узла: features·weights→top-N + honest `NoDecision` (счёт ≤0 → не выдумывать); анти-leakage входа (без candidate-index/source-path) | Ф1.2.1; образец `SkillSelectionLogicalHierarchy.cs:455-463`, `MultiSubPhaseRouterUnit.cs:291` | Описанный inference-путь узла; список `excluded_leakage_signals` | при счёте ≤0 → `HasDecision=false`, `SemanticQualityClaimed=false`; вход не содержит candidate-index/source-path как фич | sa-node | Ф1.2.1 | remaining | U, BL, UH, NEG | 01 §3.А; 01 §6 |
| Ф1.2.3 | Миграция существующих `ILogicalUnit` под узел: 3 класса случаев (обучаемый / детерминистический baseline / доменный trainable-интерфейс) | Ф1.2.2; `SkillSelectionLogicalHierarchy.cs:429+`, `MultiSubPhaseRouterUnit.cs:8`, `27.TranslationDomain` | План обёртки для 3 классов с file:line | для каждого из 3 классов named обёртка с file:line; baseline-узел несёт `IsTrained=false`, `AuthorityRole=baseline_candidate_generator` | sa-node | Ф1.2.2 | remaining | INT, REG | 01 §3.В |

### Ф1.3 — Ребро L3 + связка оркестратор L4 + машина L5

**Что решает:** ребро как L3-флоу с местом под обучаемую authority; оркестратор L4
читает топологию как контракт (без правки RunAsync); L5-машина-каркас на существующих
реестрах.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф1.3.1 | Флоу ребра `DynamicGraphEdgeFlow : ILogicalFlow` + `IEdgeCandidateProvider` (baseline) + `IEdgeFanInMerger` (контракт слияния) | Ф1.1.2; `LogicalContracts.cs:166`, `AdaptiveLogicalOrchestrator.cs:311/316` | NEW флоу ребра + контракты кандидатов/слияния (сигнатуры) | флоу реализует L3 без Train; merger — явный контракт fan-in (сегодня только массив `BranchResults`); candidate-provider помечен baseline | sa-edge | Ф1.1.2, Ф1.2.1 | remaining | U, BL, INT, REG | 02 §3.1/§5 |
| Ф1.3.2 | Оркестратор L4 читает топологию как контракт: адаптер `DynamicGraphTopology` поверх `BuildDynamicDomainTransitions` + вход от `BuildDynamicTopology(signal,budget)`; per-edge authority через `ResolveActiveUnit` (без нового кода в RunAsync); порядок обхода — по канону Q14 (Ф0.1.2) | Ф1.3.1, Ф0.1.2 (Q14); `AdaptiveLogicalOrchestrator.cs:269/316/825`; блок 04 §5.2 | Решение «adapter + BuildDynamicTopology + ResolveActiveUnit»; 0 новых решающих веток в RunAsync | подмена baseline→trained не требует нового кода в RunAsync (latest-wins, A-LM-03); `BuildDynamicTopology` output accepted only with budget proof and mutation evidence; G1..G8 блока 03 = 8/8; 0 скрытых финалов на L4; обучаемость порядка обхода соответствует `traversal_order_trainability_locked` (Q14) | sa-orch | Ф1.3.1, Ф0.1.2 | remaining | U, BL, INT, E2E, REG | 03 §3/§10 (G1..G8); 04 §5.2 |
| Ф1.3.3 | L5-машина-каркас `DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>`: 5 методов (Train/Eval/Export/Import/GetOrchestrator) поверх существующих реестров + lifecycle-загрузка (user→repo→honest scaffold) | Ф1.3.2; `LearningMachineContracts.cs:208-232`, `LogicalCapabilityTrainingContracts.cs:68` | NEW `DynamicGraphMachine` (сигнатуры 5 методов) + порядок загрузки чекпойнта | 5 методов реализуют контракт L5; число рёбер-с-authority == числу типов рёбер; lifecycle: user/cache→repo/common→`TRAINING_RUNNER_MISSING` | sa-machine | Ф1.3.2 | remaining | U, BL, INT, UH, REG | 04 §3/§10 |

---

## Ф2 — Легальный teacher-гейт + provenance + канон GraphDpoPair

**Что решает:** «dpo legal build» — ни одна строка не обучает без легального
провенанса. Вводит `TeacherProvenance` как обязательное поле строки, машину вердикта
ACCEPT/QUARANTINE/BLOCK/DATASET_REBUILD_REQUIRED, реестр разрешённых teacher, и канон
`GraphDpoPair` (объединяет 5 форматов в 1).

**Exit-gate Ф2 (измеримо):**
- `TeacherProvenance` определён со всеми **18 полями (13 + 5 NEW:** `TermsSnapshotRef`,
  `GeographyExportConstraint`, `SourceCategory`, `raw_private_text_copied`,
  `redaction_policy_ref`**)**; домены значений заданы;
- машина вердикта покрывает все **ветки C2-C15b (включая C15/C15b — Graph-DPO Artifact
  Triad Gate, фикс #3 plan-08 §8.6; точный перечень строк по таблице §4 plan-08:
  C2,C3,C3-identity,C3-scan,C4,C5,C6,C6a,C6b,C7,C8,C9,C10,C10-neg,C11,C12,C13,C14 + C15/C15b)**,
  для каждой фиксированной строки возвращает точный `verdict`+`reason`;
- **Graph-DPO Artifact Triad Gate (CANON-предусловие `dpo_train_ready`,
  trainable-architecture-decision-SKILL §Graph-DPO Artifact Triad Gate):**
  `triad_field_presence_rate == 1.0` для всех строк с `dpo_train_ready=true` (триада
  `raw_comparison_event` + `legal_provenance_dpo_envelope` + `training_batch` с
  `candidate_order_hash`/`split_id`/`seed`/`sampling_params`); `train_ready_without_triad_count == 0`;
  `legal_verdict==ACCEPT` без полной триады ⇒ строка остаётся eval-only
  (`dpo_train_ready=false`) — plan-08 §10 п.11;
- `accept_without_provenance == 0`; правило **по идентичности (R2)**
  `IsClosedByDenylist(model_id) && used_for_training=true ⇒ BLOCK` в 100%
  (`closed_proprietary_model` выводится из denylist, не доверяется как вход);
- `judge_model=closed` при `used_for_training=false ⇒ ACCEPT && judge_is_closed=true`;
- `provenance_field_presence_rate == 1.0` по всем 18 полям;
  `self_judge_train_ready_without_legal_gate == 0`;
- канон `GraphDpoPair` (14 обязательных полей) утверждён; адаптеры из 5 форматов → 1
  канон (integration-тест «5 форматов → 1 канон»);
- `AllowedTeacherRegistry` создан (Qwen3/DeepSeek/Kimi/GLM/Mistral; denylist
  Claude/OpenAI/Gemini; denylist-вердикт по `model_id` протестирован NEG);
- манифест `legal-provenance-manifest.json` содержит только `legal_verdict==ACCEPT`;
  гейт батча допускает в `TrainBatchAsync` **ТОЛЬКО `legal_verdict==ACCEPT`** и
  отвергает `LearningBatch` со строкой `legal_verdict ∈ {BLOCK,
  DATASET_REBUILD_REQUIRED, BLOCK_FOR_COMMON, QUARANTINE}` ИЛИ `legal_verdict==PENDING`
  (`QUARANTINE` = легальность teacher НЕ доказана положительно: `teacher_not_in_allowlist`,
  `license_or_terms_unverified`, `terms_snapshot_missing`, `geography_export_not_allowed`,
  `provenance_not_reproducible` — plan-08 §3.3/§8.1; измеримо:
  `batch_admits_only_legal_verdict_ACCEPT == true`);
  **зависимость:** plan-08 §3.3/Gate B п.7 привести к той же формулировке (только
  `ACCEPT` идёт в общий корпус);
- **связка plan-08 §3.3 шаг-7:** `dpo_train_ready == true` требует
  `legal_verdict == ACCEPT` (`legal_gate_passed == true`) как необходимое
  предусловие — без легального гейта строка не может стать train-ready.

**Подфазы:** Ф2.1 (TeacherProvenance + реестр + машина вердикта), Ф2.2 (канон GraphDpoPair + адаптеры + гейт батча).

### Ф2.1 — TeacherProvenance + реестр + машина вердикта

**Что решает:** легальный шлагбаум данных как обязательное поле + детерминированный
гейт-юнит (governance/eval-only, не семантическая власть).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф2.1.1 | `TeacherProvenance` (18 полей: 13 + 5 NEW `TermsSnapshotRef`, `GeographyExportConstraint`, `SourceCategory`, `raw_private_text_copied`, `redaction_policy_ref`) + домены значений + `AllowedTeacherRegistry` (data: allowlist/denylist/провайдеры/`geography_allowed[]`) | Ф0.1.3; образец `ResultGapDpoWriters.cs:7-14/192-196`; benchmark T2 (`claude-04:60`; `claude-02:61`) | NEW `TeacherProvenance` (18 полей, включая `SourceCategory`); файл `allowed-teacher-registry.json` (+ `geography_allowed[]`) | 18 полей с доменами (13 + 5 NEW), `SourceCategory` присутствует в record; allowlist=Qwen3/DeepSeek/Kimi/GLM/Mistral; denylist=Claude/OpenAI/Gemini; **denylist-вердикт по идентичности (R2, обязателен):** `IsClosedByDenylist(model_id) ⇒ closed_proprietary_model=true` (флаг ВЫВОДИТСЯ из denylist по `model_id`, не принимается на веру), а ветка `IsClosedByDenylist(model_id) && teacher_output_used_for_training=true ⇒ BLOCK` присутствует и протестирована **NEG-тестом** (denylist-`model_id` как BLOCK-триггер); **доп. ветка-вердикт terms_snapshot:** `open_weight_hosted && teacher_output_used_for_training=true && TermsSnapshotRef==UNKNOWN ⇒ QUARANTINE (reason terms_snapshot_missing)`; **доп. ветка-вердикт geography:** hosted teacher вне `geography_allowed[]` ⇒ QUARANTINE; **R17 client/third_party (определяющее поле, не только лист Ф10.2.1):** `SourceCategory` несёт значения `client_unverified`/`third_party` сверх enum plan-08 §3.1 (`local`/`human`/`open_weight`/`hosted`/`deterministic_baseline`/`self_judge`) — нота «мой канон расширяет plan-08 T5»; все поля (включая client/third_party значения `SourceCategory`) present в record/реестре | sa-legal | Ф0.1.3 | remaining | U, NEG | 08 §3.1/§3.2/§5 (расширено client/third_party); benchmark T2 |
| Ф2.1.2 | `TeacherProvenanceGateUnit.Evaluate` — машина вердикта (8 шагов §3.3): scan contamination (по `model_id` И `judge_model`) → closed-block-by-denylist → lookup → license → reproducibility → first-party → ACCEPT → judge-ветка; + под-гейт §8.5 privacy/retention (поля `raw_private_text_copied`, `redaction_policy_ref`) | Ф2.1.1; `training-data/orchestration-reasoning/*.jsonl` (контаминация); benchmark T3 (`claude-07:43`; `claude-04:26,79,90`) | NEW гейт-юнит + `TeacherProvenanceGateResult` (+ поля privacy/retention) | ветки C2-C15b (включая C15/C15b — Graph-DPO Artifact Triad Gate, фикс #3; перечень по таблице §4 plan-08) → точный verdict+reason; **триада dpo_train_ready:** `triad_field_presence_rate==1.0` для `dpo_train_ready=true`, `train_ready_without_triad_count==0`, ACCEPT без полной триады ⇒ eval-only (plan-08 §8.6/§10 п.11); **идентичность важнее флага (R2):** `IsClosedByDenylist(model_id) && teacher_output_used_for_training=true ⇒ BLOCK` 100% (а `closed_proprietary_model` выводится из denylist, не доверяется как вход); `ScanContamination` покрывает `model_id` И `judge_model`; judge-closed при `!used ⇒ ACCEPT`; `accept_without_provenance==0`; `provenance_field_presence_rate==1.0` по всем 18 полям; `self_judge_train_ready_without_legal_gate==0`; **privacy/retention под-гейт:** `hosted_provider && raw_private_text_copied=true && redaction_policy_ref==none ⇒ BLOCK_FOR_COMMON` (личное → только adaptive leg R5); поля `raw_private_text_copied`/`redaction_policy_ref` present; **R17 НОВАЯ ветка C16 client-origin (определяющий юнит, не лист):** `source_category ∈ {client_unverified, third_party} && teacher_output_used_for_training=true && provenance NOT положительно-allowlisted ⇒ QUARANTINE (reason client_origin_unverified)`; протестирована NEG-тестом «marker-absence ≠ open_weight» (отсутствие маркера НЕ даёт молчаливый ACCEPT) | sa-legal | Ф2.1.1 | remaining | U, BL, NEG, UH | 08 §3.3/§8.5/§10 (+C16 client-origin); benchmark T3 |
| Ф2.1.3 | Аудит/карантин контаминированных `sonnet-*` строк → DATASET_REBUILD_REQUIRED (рекон-1: 19 файлов/72 вхождения; 35/10 в reasoning) | Ф2.1.2 (gate); R17 | Список строк с вердиктом REBUILD; решение rebuild/quarantine | `contaminated_rows_marked_REBUILD == contaminated_rows_found`; 0 train-ready строк с `sonnet/openai/gpt/claude/gemini` в source_ref | sa-legal | Ф2.1.2, Ф0.1.3 | remaining | INT, QA, NEG | 08 §6.3; 07 §6.5; R17 |

### Ф2.2 — Канон GraphDpoPair + адаптеры + гейт батча

**Что решает:** единый формат пары для всего графа; адаптеры из 5 существующих
форматов; гейт батча перед обучением.

Schema owner note: `codex-plan-09` §5.1 owns the canonical `GraphDpoPair`
record shape; `codex-plan-07` builds/adapts it; `codex-plan-08` owns
`TeacherProvenance` and `legal_verdict`. Board Owner cells below name execution
owners for implementation, not schema ownership.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф2.2.1 | `GraphDpoPair` (14 обязательных полей; `judge_model` внутри `TeacherProvenance`; `wrong_markers`=IReadOnlyList) + проекция на `LearningTrainingItem` | Ф0.1.2; `LearningMachineContracts.cs:30` | NEW `GraphDpoPair`; маппинг chosen/rejected/input→триплет | 14 полей утверждены; проекция на `LearningTrainingItem` без правки 00.Core; **`dpo_train_ready==true` требует `legal_verdict==ACCEPT` (`legal_gate_passed==true`)** как необходимое предусловие (plan-08 §3.3 шаг-7) | sa-legal | Ф2.1.1, Ф0.1.2 | remaining | U, REG | 07 §5.1; 06/08/09 §5 |
| Ф2.2.2 | Адаптеры из 5 форматов → 1 канон + гейт батча (в `TrainBatchAsync` допускается ТОЛЬКО `legal_verdict==ACCEPT`; строка `legal_verdict ∈ {BLOCK, DATASET_REBUILD_REQUIRED, BLOCK_FOR_COMMON, QUARANTINE}` не допускается; строка без проставленного `legal_verdict` = PENDING тоже не допускается — закрыт путь необработанной строки И путь недоказанной легальности) + `legal-provenance-manifest.json` | Ф2.2.1, Ф2.1.2 | Адаптеры (DpoPair/Wiki/Orchestration/jsonl/P5H → GraphDpoPair); гейт батча; манифест | integration «5 форматов → 1 канон» PASS; манифест содержит только `legal_verdict==ACCEPT`; **`batch_admits_only_legal_verdict_ACCEPT == true`**; батч отвергнут до `TrainBatchAsync`, если есть строка с `legal_verdict ∈ {BLOCK, DATASET_REBUILD_REQUIRED, BLOCK_FOR_COMMON, QUARANTINE}` ИЛИ строка с `legal_verdict==PENDING` (ни необработанной, ни недоказанно-легальной строки в `TrainBatchAsync`); зависимость: plan-08 §3.3/Gate B п.7 — та же формулировка | sa-legal | Ф2.2.1, Ф2.1.2 | remaining | INT, BL, NEG, QA | 07 §10; 08 §3.3/§10 |

---

## Ф3 — DPO-строитель (GraphDpoBuilder)

**Что решает:** сердце «авто-DPO из несуществующих данных» — машинно строит тройки
`(вход, chosen, rejected + [WRONG:])` из прогона графа, прогоняя через 3 гейта
(legal/leakage/near-miss-quality). Заменяет self-label `chosen` на легального teacher.

**Exit-gate Ф3 (измеримо):**
- `GraphDpoBuilder.BuildPairAsync` на фикстуре даёт валидный `GraphDpoPair`:
  `WrongMarkers` непуст, `overlap_ratio < 0.60`, `reason.Length >= 24`,
  `ValidationOutcome="accepted"`;
- `TeacherProvenanceGate` встроен шагом 1: closed → DATASET_REBUILD_REQUIRED;
- permutation-инвариантность для edge: при ≥3 перестановках `candidate_node_ids`
  `chosen` идентичен → `candidate_order_invariant=true`; иначе QUARANTINE;
- материализация `GraphDpoPair → DpoPair` читается `DpoDataset.Load`
  (`11.DPO/DpoDataset.cs:18-26`) без исключения;
- QA-скан: 0 train-ready строк с closed-маркерами.

**Подфазы:** Ф3.1 (builder + 3 гейта), Ф3.2 (rejected near-miss + адаптер старых генераторов).

### Ф3.1 — GraphDpoBuilder + три гейта построения

**Что решает:** оркестратор построения пары (классификация уровня → legal-гейт →
вход A → chosen → invariants → schema-validator).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф3.1.1 | `GraphDpoBuilder.BuildPairAsync`: Шаг 0 классификация уровня (unit/edge/graph) + Шаг 1 legal-гейт (вызов `TeacherProvenanceGate`) + Шаг 2 вход A (`LeakageScrubber`) | Ф2.2.1, Ф2.1.2; `WikiContextRoutingDpoGenerator.cs:40-45` | NEW `GraphDpoBuilder` (BuildPairAsync/BuildBatchAsync) | level из {unit,edge,graph} иначе Fail; legal-гейт встроен; вход прогнан через scrubber (пути/id/имена вычищены) | sa-builder | Ф2.2.1, Ф2.1.2 | remaining | U, BL, INT | 07 §3 Шаг0-2 |
| Ф3.1.2 | `ResolveChosenAsync`: teacher-путь (легальный) ИЛИ first-party baseline (`chosen_origin`, `semantic_quality_claimed=false`); **домен `chosen_origin` расширен значением `client_supplied_unverified` (R17, определяющий юнит)** | Ф3.1.1; `WikiContextRoutingDpoGenerator.cs:58` | chosen + `chosen_origin` (`legal_teacher`/`first_party_deterministic`/`human_correction`/**`client_supplied_unverified`**) + флаг semantic-claim | teacher-путь → `chosen_origin=legal_teacher`; first-party → `semantic_quality_claimed=false`; **`chosen_origin=client_supplied_unverified ⇒ QUARANTINE`** (значение определено здесь, лист Ф10.2.1 на него ссылается, а не вводит впервые) | sa-builder | Ф3.1.1 | remaining | U, BL, UH, NEG | 07 §3 Шаг3; 07 §6.2 (+client_supplied_unverified) |
| Ф3.1.3 | `CheckInvariants` + `GraphDpoSchemaValidator`: overlap<0.60, no-exact-prompt-overlap, permutation-инвариантность (edge), schema-валидатор анти-leakage | Ф3.1.2; `OrchestrationCorpusBuilder.cs:243-262`, `ResultGapDpoWriters.cs:241-266` | NEW `GraphDpoSchemaValidator`; вердикт invariants | overlap≥0.60 → quarantine; permutation меняет выбор → QUARANTINE_DO_NOT_TRAIN; schema-fail → `GRAPH_DPO_SCHEMA_INVALID` | sa-builder | Ф3.1.2 | remaining | U, BL, NEG, leakage | 07 §3 Шаг5-6/§10 |

### Ф3.2 — Rejected near-miss + адаптер старых генераторов

**Что решает:** построение rejected как near-miss + `[WRONG:]` (не мусор); адаптация
существующих генераторов в канон; материализация в движок.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф3.2.1 | `BuildRejected`: реальная ошибка прогона ИЛИ синтетический near-miss из таксономии + `[WRONG: код последствие]`; запрет rejected-мусора/отрицания-шаблона | Ф3.1.3; `WikiContextRoutingDpoGenerator.cs:230-267`, `ResultGapDpoWriters.cs:199-235` | rejected near-miss + wrong-annotation | rejected отличается от chosen РОВНО одной ошибкой; `[WRONG:]` непуст; мусор «I don't know» → QUARANTINE | sa-builder | Ф3.1.3 | remaining | U, BL, NEG | 07 §3 Шаг4; 07 §6 |
| Ф3.2.2 | Адаптер `WikiContextRoutingDpoGenerator`→`GraphDpoPair` (с `chosen_origin=first_party_deterministic`) + материализация `GraphDpoPair → DpoPair` (`DpoDataset.Load`) | Ф3.2.1; `WikiContextRoutingDpoGenerator.cs`, `11.DPO/DpoDataset.cs:18-26` | Адаптер 50-строчного батча; ref→текст материализация | адаптер даёт валидный `GraphDpoPair` на каждую строку; `DpoDataset.Load` парсит без исключения, `Count`=числу train-ready | sa-builder | Ф3.2.1, Ф2.2.2 | remaining | INT, QA, REG | 07 §3 Шаг7/§9/§10 |

---

## Ф4 — Обучение + lifecycle (LoRA+DPO, checkpoint/export/import/locator, split R5)

**Что решает:** делает обучение настоящим, а не заглушкой. Реальная вклейка LoRA в
Forward; reward margin из движка (устранить литерал 0.03); continue-training от
checkpoint; export/import через 25.ModelServer (SHA-256 round-trip), locator,
fresh-load; SPLIT common/personal (`DynamicGraphMachinePair`).

**Exit-gate Ф4 (измеримо, 10 строк блока 10 §10 + обучаемый узел level=unit):**
- **обучаемый узел `level=unit` (Ф4.3.1, прямой субъект R1):** `GraphNodePreferenceState`
  обучен DPO reward-margin'ом — `PreferenceWeights` сдвинуты, held-out `PassesGate`,
  `RewardMargin>0`, `After>Before`, `TrainedNotCounterBaseline==true` (counter-baseline
  только eval-only ablation);
- LoRA wired (структурно: `ForwardWired==true`, `AttachedCount>0`, градиент течёт в
  `lora_a/lora_b`) И trained (output-дельта≠0 после ≥1 DPO-шага) — раздельно;
- литерал 0.03 как RewardMargin устранён; margin из `DpoMetrics.RewardMargin`;
- continue-training замкнут: `Load(base)→Load(lora)→Apply→Step`, Δ>0 за ≥2 шага;
- `IModelServer` работает локально (`LocalFileModelServer`: SHA-256 совпадает);
- lifecycle полный: checkpoint + манифест с `TeacherProvenance` + locator + fresh-load
  (рестарт→LoadCheckpoint→та же метрика без replay);
- легальный гейт: closed → DATASET_REBUILD_REQUIRED;
- анти-leakage: `CandidateOrderInvariant` подтверждён перестановкой; ablation ≠
  детерминированный пол confidence;
- split R5: манифест несёт `LegScope`; `DynamicGraphMachinePair` разделяет
  common/personal; sync только обезличенный;
- числа без UNKNOWN (`AccuracyBefore/After`, `RewardMargin`, `CpuMs`, `PeakRamMb`,
  `ElapsedMs`).

**Подфазы:** Ф4.1 (LoRA wiring + margin из движка), Ф4.2 (continue-training + IModelServer + lifecycle), Ф4.3 (split R5 + обучаемый EdgeAuthorityUnit).

### Ф4.1 — LoRA wiring + reward margin из движка

**Что решает:** починка двух заглушек обучения — LoRA реально влияет на Forward, и
margin вычисляется, а не хардкодится.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф4.1.1 | LoRA wiring (блюпринт A, ведущий вариант A3): `Apply` возвращает `LoraApplyResult` + карта адаптеров в `CausalSelfAttention.Forward` (поля Linear/readonly целы); inference-режим dropout | Ф3.2.2; `LoraApplicator.cs:9/35`, `CausalSelfAttention.cs:12-15/48`, `LoraLinear.cs:52` | Wired LoRA (`AttachedCount/ForwardWired`); dropout-флаг | (wired) `ForwardWired==true`, `AttachedCount>0`, градиент в `lora_a/lora_b`; output-дельта НЕ требуется на init (B=Zeros) | sa-train | Ф3.2.2, Ф1.3.3 | remaining | U, INT, REG | 10 §6 блюпринт A; 10 §10 п.1 |
| Ф4.1.2 | Reward margin из движка (блюпринт C): устранить литерал 0.03 в `TranslationMachine.cs:70/123-126/187`; `RewardMargin` из `DpoMetrics.RewardMargin` (`EvaluateMany`/`StepVector`) | Ф4.1.1; `TranslationMachine.cs:70/123-126/187`, `DpoMetrics.cs:9`, `DpoTrainer.cs:127-167` | Margin из движка во всех 3 местах | grep по `TranslationMachine.cs` НЕ находит `0.03` как RewardMargin; `RewardMargin == DpoMetrics.RewardMargin` | sa-train | Ф4.1.1 | remaining | U, BL, REG | 10 §6 блюпринт C; 10 §10 п.2; R19 |

### Ф4.2 — Continue-training + IModelServer + lifecycle

**Что решает:** замкнуть жизненный цикл: дообучение от чекпойнта, локальный транспорт
весов, fresh-load, манифест с teacher-провенансом.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф4.2.1 | Continue-training (блюпринт E): `LoraCheckpointIo.Save/Load` + порядок `Load(base)→Load(lora)→Apply→Step`; `IncrementalTrainer` (блюпринт B, ведущий B2) → `NOT_ON_GRAPH_TRAINING_PATH` | Ф4.1.2; `LlmIngredientLogicalUnits.cs:339-350`, `IncrementalTrainer.cs:50-52` | NEW `LoraCheckpointIo`; решение IncrementalTrainer | порядок load подтверждён; margin после родителя Δ>0 за ≥2 шага; `IncrementalTrainer` не no-op (B2: REBUILD-маркер) | sa-train | Ф4.1.2 | remaining | U, INT, TA | 10 §6 блюпринт B/E; 10 §10 п.3-4 |
| Ф4.2.2 | `IModelServer` (блюпринт F): `LocalFileModelServer` (stream↔файл, атомарно) + публикация через `IModelRepository.UploadAsync` (SHA-256) + locator `IRemoteUnitLocator` | Ф4.2.1; `IModelServer.cs:5-15`, `LocalModelRepository.cs:32-77` | NEW `LocalFileModelServer`; путь публикации | `PublishAsync`→`OpenWeightsAsync` SHA-256 совпадает; HTTP явно вне scope; locator резолвит unitId | sa-train | Ф4.2.1 | remaining | INT, QA | 10 §6 блюпринт F; 10 §10 п.5 |
| Ф4.2.3 | Lifecycle полный (блюпринт D/G): манифест с `TeacherProvenance` (`GeneratedCapabilityCheckpointManifest`) + поля lifecycle `manifest_id`/`schema_version`/`source_cutoff`/`late_file_queue`/`path_id_strategy`/`rollback_ref` + held-out/ablation/fresh-load/rollback раннер; числа без UNKNOWN | Ф4.2.2; `TeachingRegistrySnapshot.cs:41-83`, `LlmIngredientLogicalUnits.cs:389-390`; benchmark T1 (`codex-50-cycle…:233,240,242`); скил `trainable-architecture-decision-SKILL.md:96-99` | Расширенный манифест (+ lifecycle/rollback-поля); held-out+rollback-раннер | манифест несёт teacher-поля **И** `manifest_id`/`schema_version`/`source_cutoff`/`late_file_queue`/`path_id_strategy`/`rollback_ref`; `schema_version` обязателен и проверяется при import; fresh-load: рестарт→LoadCheckpoint→та же метрика без replay; **тест отката:** `Rollback(prev_checkpoint)` загружает прошлый чекпойнт и воспроизводит прошлую held-out метрику; 0 полей UNKNOWN | sa-train | Ф4.2.2 | remaining | INT, QA, TA, NEG | 10 §6 блюпринт D/G; 10 §10 п.6-7/п.10; benchmark T1 |

### Ф4.3 — Split R5 + обучаемый EdgeAuthorityUnit

**Что решает:** разделение common/adaptive ноги, обучаемый узел `level=unit`
(GraphNodePreferenceState — прямой субъект R1) и обучаемый финальный решатель ребра
(заготовка под cutover Ф8; baseline пока не трогаем). **Покрытие R1:** `level=unit`
обучается ОТДЕЛЬНЫМ подшагом Ф4.3.1 (узел), `level=edge` — Ф4.3.2 (ребро); удаление/
перенос одного не покрывает другой (plan-review «removal/deferral != coverage»).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф4.3.1 | **Обучаемый узел `level=unit` (прямой субъект R1):** `GraphNodePreferenceState` (plan-01 §5.2) обучается через L5 на `GraphDpoPair(level=unit)` DPO reward-margin'ом — раздельно от ребра Ф4.3.2 | Ф4.2.3, Ф1.2.1, Ф4.1.2 (margin из движка); `GraphDpoPair(level=unit)` из Ф3.1.1; **plan-01 §3.Б/§5.2**, `11.DPO/DpoTrainer.cs:42-65` | trained `GraphNodePreferenceState` (`PreferenceWeights`) + checkpoint узла | для `level=unit` пары: `PreferenceWeights` сдвинуты DPO reward-margin (margin = chosenReward − rejectedReward, `DpoTrainer.cs:42-65`); held-out `PassesGate`; `RewardMargin>0`; `After>Before`; `TrainedNotCounterBaseline==true`; counter-baseline `Dictionary<string,int>`+(+1) ТОЛЬКО eval-only ablation (не финальная власть) | sa-train | Ф4.2.3, Ф1.2.1, Ф4.1.2 | remaining | U, BL, TA, leakage | 01 §3.Б/§5.2; 01 Gate B п.5; 10 §6 |
| Ф4.3.2 | Обучаемый `EdgeAuthorityUnit`/`SemanticEdgeRouterUnit` (NEW, `level=edge`): вход `EdgeAuthorityInput` (физически без candidate-index/source-path) → `EdgeAuthorityDecision`; обучается через L5 | Ф4.3.1, Ф1.3.1, Ф0.1.3 (Q11 порог ablation); `GraphDpoPair(level=edge)` из Ф3; `MultiSubPhaseRouterUnit.cs:291`, блок 02 §5 | NEW обучаемый узел-решатель ребра + typed state | вход НЕ `SubPhaseRouterSourceDocument` (нормализующий адаптер); для `level=edge` пары: held-out PassesGate, RewardMargin>0, After>Before; ablation просадка ≥ порог (Ф0 Q11, `ablation_threshold_locked`) | sa-train | Ф4.3.1, Ф1.3.1, Ф0.1.3 | remaining | U, BL, TA, leakage | 02 §3/§5/§10; 12 §6.2 |
| Ф4.3.3 | Split R5: `LegScope` (CommonGlobal/PersonalAdaptive) в манифесте + `DynamicGraphMachinePair : ILearningMachinePair` (common в llm_mvp_core, personal у пользователя, sync обезличенный) | Ф4.3.2; `LearningMachineContracts.cs:245-261` | NEW `DynamicGraphMachinePair`; `LegScope` в манифесте | манифест несёт `LegScope`; pair разделяет common/personal; `SyncPersonalToGlobalAsync` шлёт только обезличенный сигнал (инспекция payload) | sa-train | Ф4.3.2, Ф0.1.3 | remaining | INT, BL | 10 §6 блюпринт H; 10 §10 п.9; R10 |

---

## Ф5 — Измерение (held-out / ablation / permutation / two-PASS)

**Что решает:** экзаменатор графа — доказывает числом, что узел/ребро/граф обучился,
а не притворяется. Единый `GraphMeasurementReport` для трёх масштабов; разделяет
honest-scaffold vs trained-feature.

**Exit-gate Ф5 (измеримо, блок 11 §10):**
- `GraphMeasurementReport` покрывает 3 оси (СКОРОСТЬ/ТОЧНОСТЬ с разделением
  live-vs-held-out/КАЧЕСТВО) + `HonestyVerdict` + `ToHumanRu()`;
- разделение точности доказано тестом (`LiveMarginAccuracy != HeldOutAccuracy`,
  приёмка использует только held-out);
- two-PASS реализуем: `PASS_AS_TRAINED_FEATURE` только при held-out числа + ablation
  (`no_trained_state_baseline`) + lifecycle (fresh-load==trained И continue Δ>0);
- per-edge контроли: `SelectedFromSourceIdOnly==false`, permutation-invariance, 5
  именованных leakage-контролей на каждом ребре;
- пороги: novelty-gate `newPattern > (1-maxAccuracy) AND seenTokens < maxAccuracy`;
  held-out Δ≥+0.02; reward-margin≥0.025; held-out accuracy ≥ порог Q30
  (`heldout_accuracy_threshold_locked`, владелец Ф0.1.3);
- анти-стаб: 0 полей UNKNOWN/static/contract/mixed для trained-claim;
- teacher-provenance present; контаминация → DATASET_REBUILD_REQUIRED.

**Подфазы:** Ф5.1 (контракт отчёта + точность/скорость + честность untrained), Ф5.2 (качество per-edge + lifecycle-проба + two-PASS).

### Ф5.1 — Контракт отчёта + точность/скорость + untrained-honesty

**Что решает:** единый отчёт измерения, разделение live/held-out точности, ресурсы,
честность необученного.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф5.1.1 | `GraphMeasurementReport` (3 оси + honesty + ToHumanRu) + `GraphMeasurementCoordinator` (ClassifyLevel) + `GraphSpeedMeter` (CpuMs/RamMb/ElapsedMs/rows) | Ф4.2.3; `LearningMachineContracts.cs:43-62`, `CrossStackBusinessFlowGraphTrainingRunner.cs:35-39` | NEW отчёт + координатор + speed-meter | отчёт несёт 3 оси + honesty + ToHumanRu; ClassifyLevel отвергает scorer/KNN как финал (`FORBIDDEN_RUNTIME_SHORTCUT`); скорость без UNKNOWN | sa-measure | Ф4.2.3 | remaining | U, QA | 11 §3 Шаг0/5; 11 §10 п.1/6 |
| Ф5.1.2 | `GraphAccuracyMeter` (live vs held-out РАЗДЕЛЕНЫ) + `HeldOutSplitGuard` (контаминация до точности, source-disjoint); порог приёмки held-out — по канону Q30 (Ф0.1.3) | Ф5.1.1, Ф0.1.3 (Q30 порог held-out); `DpoTrainer.cs:64/120`, `LearningMachineContracts.cs:217`, `TranslationProductHeldOutRunner.cs:285-287`; блок 11 §split proof | NEW accuracy-meter + split-guard | `LiveMarginAccuracy != HeldOutAccuracy` доказано тестом; приёмка только held-out против порога Q30 (`heldout_accuracy_threshold_locked`); split proof содержит `train_source_roots`, `eval_source_roots`, `source_hashes`, `cross_split_near_duplicate_count`, `simhash_or_minhash`, `near_duplicate_threshold`; `cross_split_near_duplicate_count>0` или overlap → точность запрещена | sa-measure | Ф5.1.1, Ф0.1.3 | remaining | U, BL, NEG | 11 §3 Шаг2-3; 11 §10 п.2; 11 §split proof |
| Ф5.1.3 | `MeasureUntrainedHonesty`: пустой узел/ребро честно `not_ready` (`NO_TRAINED_STATE_LOADED`) + рефлексия без Regex/Dictionary/Classifier/Lookup/Bm25/ExactPrompt | Ф5.1.2; `PromptToTaskMachineSkeletonTests.cs:18-20/79-85` | NEW untrained-honesty-проба | пустой элемент → `not_ready` + evidence-токен; рефлексия не находит запрещённых членов → `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` | sa-measure | Ф5.1.2 | remaining | UH, NEG | 11 §3 Шаг1 |

### Ф5.2 — Качество per-edge + lifecycle-проба + two-PASS

**Что решает:** качество (novelty/ablation/permutation/leakage) на каждом ребре,
реальный continue-training, финальный выбор честности.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф5.2.1 | `GraphQualityMeter` (per-unit И per-edge): novelty-gate + ablation `no_trained_state_baseline` + 5 leakage-контролей + permutation-invariance | Ф5.1.3, Ф4.3.1, Ф4.3.2, Ф0.1.3 (Q11 порог ablation); `TatoebaPilotLoopRunner.cs:777`, `Learn06HeldOutAblationRunnerTests.cs:42` | NEW quality-meter (per-unit + per-edge) | **per-unit (`level=unit`, узел Ф4.3.1):** held-out обученного `GraphNodePreferenceState` против frozen-baseline даёт `RewardMargin>0`, `After>Before`, ablation просадка ≥ порог (Ф0 Q11) — измерение, что узел реально обучился, а не counter-baseline; **per-edge (`level=edge`, ребро Ф4.3.2):** novelty `newPattern>(1-maxAcc) AND seenTokens<maxAcc`; `SelectedFromSourceIdOnly==false`; permutation не меняет выбор; ablation просадка ≥ порог (Ф0 Q11, `ablation_threshold_locked`); 5 контролей на каждом ребре | sa-measure | Ф5.1.3, Ф4.3.1, Ф4.3.2, Ф0.1.3 | remaining | BL, NEG, TA, leakage | 11 §3 Шаг4; 11 §10 п.4-5 |
| Ф5.2.2 | `GraphLifecycleProbe` (export→import→fresh-load→continue-training реальным прогоном) + `Compose` (two-PASS: held-out+ablation+lifecycle → trained; масскарад → FAIL) | Ф5.2.1; `LearningMachineContracts.cs:222/227`, `OrchestrationReasoningMachineContinueTrainingTests.cs:13-19`; блок 11 §lifecycle proof | NEW lifecycle-проба + two-PASS логика | `freshEval==trainedEval`; continue Δ>0 прогоном; lifecycle proof содержит `process_id`, `restart_id`, `entrypoint_trace_id`, `no_replay_cache_proof`, loaded checkpoint/manifest hashes, `manifest_schema_version`, `source_cutoff`, `path_id_strategy`, `rollback_ref`; масскарад → `two_pass_conflation_untrained_reported_as_trained` | sa-measure | Ф5.2.1, Ф4.2.3 | remaining | TA, INT, NEG | 11 §3 Шаг6-7; 11 §10 п.3; 11 §lifecycle proof |

---

## Ф6 — Async мульти-комбинации (1 вход → N → лучший)

**Что решает:** «один вход → N комбинаций → лучший» параллельно, с честным выбором
победителя. Генератор тренировочного сигнала для авто-DPO петли: вместо шаблона
chosen приходит от лучшей реально прогнанной комбинации (или легального teacher).

**Exit-gate Ф6 (измеримо, блок 06 §10):**
- для `budget.Max=4` flow стартует ровно 4 async комбинации (`Task.WhenAll`) и
  собирает 4 кандидата (упавшая = error-evidence, не chosen);
- победитель != немаркированная детерминированная формула; ablation без baseline-judge
  сохраняет held-out accuracy (`delta_without_helper >= 0`); иначе honest baseline;
- из N≥2 валидных кандидатов построены `GraphDpoPair[]` с `overlap<0.60`,
  `candidate_order_invariant=true`, `[WRONG:]` непуст;
- legal: каждая train-ready пара `closed_proprietary_model=false` ИЛИ chosen=Y_best;
  judge_model отдельно; 0 строк с teacher=Claude/OpenAI/Gemini;
- **judge-origin под-гейт (измеримая защита CANON #2 «judge != teacher-цель», plan-09 §8
  G1/G2 + §10 п.2; обязателен и в self_best, и в teacher_on):** для каждой принятой
  строки `chosen_equals_runtime_candidate_or_legal_goal == true` (chosen побитово равен
  выходу одной из реально прогнанных async-комбинаций ученика ИЛИ легальному Y_goal при
  `teacher_on`) И `judge_did_not_author_chosen == true` (closed `judge_model` вернул ТОЛЬКО
  ranking/scalar/wrong_reason, без поставки/правки текста chosen); нарушение любой ⇒
  `DATASET_REBUILD_REQUIRED`, строка НЕ принимается;
- honest-ветки: всё плохо → KNOWLEDGE_EDGE без chosen; overlap≥0.60 →
  NO_PREFERENCE_SIGNAL, 0 train-ready;
- learning-delta (на стороне L5/Ф5): RewardMargin≥0, loss<0.693 на held-out.

**Подфазы:** Ф6.1 (планировщик + async-арбитр + сбор кандидатов), Ф6.2 (judge + teacher-goal-gate + сборщик пар + many-to-many merge).

### Ф6.1 — Планировщик комбинаций + async-арбитр

**Что решает:** какие комбинации пробовать (baseline) и параллельный прогон одного
входа через N комбинаций.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф6.1.1 | `CombinationPlanner.BuildCombinations` (baseline: узлы/порядок/флаги) + async-режим через канон-флаг ребра `ParallelEligible=true` (НЕ член enum; канон-enum — `GraphEdgeKind` 5 членов, мастер §A.4.1 стр.178; член `AsyncParallel` устарел) + budget.Max=4 (потолок 8) + детерминированный дедуп spec по `SpecSignature` | Ф5.2.2, Ф1.3.2; `AdaptiveLogicalOrchestrator.cs:19/544`, `LogicalContracts.cs:46-59` | NEW планировщик + `CombinationSpec[]` + `SpecSignature` на каждый spec | помечен baseline (не выбирает победителя); потолок 8 = `MaxAdaptiveBranchCount`; async-параллельность выражена флагом `ParallelEligible`, а не членом enum; **дедуп (G6, plan-06 §10 п.7, измеримо):** `|distinct SpecSignature| == |specs|` И `|specs| <= budget.Max`, детерминированно при `budget.Seed` (вместо неизмеримого «уникальных») | sa-async | Ф5.2.2, Ф1.3.2 | remaining | U, BL, unit-dedup | 06 §3 Шаг0; 06 §8 G6; 06 §10 п.1/п.7; мастер §A.4.1 |
| Ф6.1.2 | `AsyncCombinationArbiterFlow.RunCombinationsAsync` (`Task.WhenAll`, изоляция metadata-копий) + `CollectCandidates` (pre-filter, honest knowledge-edge, runtime-дедуп одинаковых выходов) | Ф6.1.1; `AdaptiveLogicalOrchestrator.cs:462-465/976` | NEW async-flow + `CombinationCandidate[]` | budget.Max=4 → ровно 4 async (integration-тест, не последовательно); упавшая = error-evidence; metadata изолирована; **runtime-дедуп (G7, plan-06 §8 G7, измеримо):** два разных spec с идентичным output на X ⇒ к судье доходит ОДИН кандидат, `duplicate_candidates_collapsed` учтён; вырожденная пара overlap≈1.0 НЕ строится | sa-async | Ф6.1.1 | remaining | INT, BL, NEG, unit-dedup | 06 §3 Шаг1-2; 06 §8 G7; 06 §10 п.1 |

### Ф6.2 — Judge + teacher-goal + сборщик пар + merge

**Что решает:** судейство лучшего, легальный teacher-эталон при слабом Y_best,
сборка пар, правило слияния fan-in.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф6.2.1 | `ICombinationJudgeUnit` (лучшая модель как judge, источники скрыты) + `DeterministicCombinationJudgeUnit` (fallback baseline, isLearned:false) + `TeacherGoalGate` (Y_best или легальный Y_goal) | Ф6.1.2, Ф2.1.2; `AdaptiveLogicalOrchestrator.cs:913-927` | NEW judge-юниты + teacher-goal-gate | judge не видит combination_id/позицию; fallback помечен baseline; Y_best<floor → легальный teacher; closed teacher-цель → DATASET_REBUILD_REQUIRED; **judge-origin под-гейт (plan-09 §8 G1/G2 + §10 п.2, измеримо):** `chosen_equals_runtime_candidate_or_legal_goal == true` (chosen побитово = выход реально прогнанной комбинации ученика ИЛИ легальный Y_goal) И `judge_did_not_author_chosen == true` (closed judge вернул ТОЛЬКО ranking/scalar/wrong_reason, без поставки/правки текста chosen); нарушение любой ⇒ `DATASET_REBUILD_REQUIRED` (строка не принята); проверка обязательна и в self_best, и в teacher_on | sa-async | Ф6.1.2, Ф2.1.2 | remaining | U, BL, NEG, leakage, judge-origin-NEG | 06 §3 Шаг3-4; 06 §10 п.2/4; 09 §8 G1/G2/§10 п.2 |
| Ф6.2.2 | `GraphDpoPairAssembler` (chosen/rejected/[WRONG:]/overlap<0.60) + `CombinationVerdict` (A/B/C) + many-to-many merge на fan-in (обучаемый ИЛИ honest baseline-merge с пометкой) | Ф6.2.1, Ф3.2.1, Ф1.3.1 | NEW сборщик пар + merge-правило | A=COMBINATIONS_RANKED→пары; B=NO_PREFERENCE_SIGNAL (overlap≥0.60, 0 train-ready); C=KNOWLEDGE_EDGE без chosen; merge N→1 `FinalHumanResult` | sa-async | Ф6.2.1, Ф3.2.1, Ф1.3.1 | remaining | BL, INT, NEG | 06 §3 Шаг5-6; 05 §3.4; 06 §10 п.3/5 |

---

## Ф7 — Авто-DPO петля (интеграция Ф2-Ф6)

**Что решает:** замыкает само-обучение графа в один цикл «прогон → пара →
TrainBatchAsync → измерение → промоушен → повтор». Сердце проекта: ученик сам себе
готовит карточки-уроки из ошибок и эталона наставника, непрерывно.

**Exit-gate Ф7 (измеримо, блок 09 §10):**
- канал не пуст: в `TrainBatchAsync` реально передаётся ≥1 авто-пара (не `[]`),
  построенная из шага 2 (слабый выход) и шага 3 (goal);
- гейты зелёные: `legal_verdict=ACCEPT` (или `used_for_training=false`),
  `anti_leakage_verdict=PASS` (overlap<0.60, candidate_order_invariant=true);
- **опорная (reference) политика DPO для петли — явно (plan-09 §6.4, `DpoTrainer.cs:42-65`,
  `margin = Beta*(polChosenLogP−refChosenLogP) − Beta*(polRejectedLogP−refRejectedLogP)`,
  `loss=−log(sigmoid(margin))`):** reference = ЗАМОРОЖЕННЫЙ предыдущий принятый чекпойнт
  (frozen baseline), относительно которого считаются margin/loss на held-out, а НЕ текущая
  обучаемая policy;
- обучение измеримо на held-out: reward_margin≥0, loss<0.693, After≥Before+0.02,
  где margin = policy_after против frozen reference_before;
- **обработка первой итерации/cold-start:** на итерации 0 reference == стартовый чекпойнт
  (policy==reference ⇒ margin≡0 ⇒ loss=ln2=0.693 по построению), поэтому ACCEPT_ROW по
  `loss<0.693` применяется к строке ПОСЛЕ ≥1 DPO-шага (margin меряется policy_after против
  frozen reference_before), а НЕ к нулевому шагу; зеркальная нота для plan-09 §3 Шаг0/Шаг5 +
  §10 п.3: reference — frozen baseline, не сама обучаемая policy;
- промоушен только при росте held-out; иначе `ACCURACY_NOT_IMPROVED`;
- `ShouldStop` возвращает stopReason ∈ {threshold, plateau, budget}; при этом
  threshold — режимная конъюнкция (teacher-on: `quality И teacher_gap_closed`
  epsilon; self-best: только `quality`) согласно блоку 09 §3 Шаг6;
- **judge-origin под-гейт (plan-09 §8 G1/G2 + §10 п.2, обязателен в self_best и teacher_on):**
  для каждой принятой строки `chosen_equals_runtime_candidate_or_legal_goal == true` И
  `judge_did_not_author_chosen == true`; нарушение любой ⇒ `DATASET_REBUILD_REQUIRED`,
  строка не принимается;
- честная метка two-PASS (scaffold vs trained).

**Подфазы:** Ф7.1 (детектор слабости + teacher-goal + сборка пары), Ф7.2 (оркестратор петли + ShouldStop + промоушен).

### Ф7.1 — Юниты петли (слабость → goal → пара)

**Что решает:** обнаружить слабый выход, взять легальный goal, собрать пару (всё —
inference-юниты, A-LM-02).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф7.1.1 | `IGraphWeaknessDetectorUnit` (выбор Y_best + пометка слабых как rejected-сырьё; judge_model отдельно) + `ITeacherGoalProviderUnit` (legal teacher как goal через `LegalTeacherGate`) | Ф6.2.2, Ф2.1.2; `AutonomousLearningPipeline.cs:137-140` | NEW детектор слабости + teacher-goal-провайдер | Y_best выбран; слабые помечены; teacher-goal через legal-гейт; режим self-best (без teacher) допустим; **judge-origin под-гейт (plan-09 §8 G1/G2 + §10 п.2, измеримо; обязателен в self_best и teacher_on):** в `self_best` closed `judge_model` РАЗРЕШЁН только если переранжирует/НЕ авторствует chosen — `chosen_equals_runtime_candidate_or_legal_goal == true` И `judge_did_not_author_chosen == true`; если closed-судья переписал/поставил chosen ⇒ `used_for_training=true` обход легального гейта ⇒ `DATASET_REBUILD_REQUIRED`, строка не принята | sa-loop | Ф6.2.2, Ф2.1.2 | remaining | U, BL, NEG, judge-origin-NEG | 09 §3 Шаг2-3; 09 §8 G1/G2/§10 п.2 |
| Ф7.1.2 | `IGraphDpoPairBuilderUnit` (= Ф3 builder в петле): собрать пару + анти-leakage + anti-forgetting replay (70/30) | Ф7.1.1, Ф3.1.1; `ReplayBuffer.cs` | Пара из петли + replay-микс | пара построена через builder Ф3; overlap<0.60; replay подмешан 70/30 | sa-loop | Ф7.1.1, Ф3.1.1 | remaining | U, BL, INT | 09 §3 Шаг4 |

### Ф7.2 — Оркестратор петли + ShouldStop + промоушен

**Что решает:** управляющий цикл L5, условие остановки, промоушен по дельте.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф7.2.1 | `AutoDpoLoopOrchestrator : ILearningMachine<DynamicGraphDomain>` (PrepareAsync/RunCandidatesAsync/TrainBatchAsync/EvaluateAsync) — один оборот петли шаги 1→6 | Ф7.1.2, Ф5.2.2, Ф4.2.3 | NEW оркестратор петли | `batch.Items.Count ≥ 1` (не `[]`); один оборот: пара→Train→Eval→промоушен; **reference = frozen baseline (предыдущий принятый чекпойнт), margin/loss считаются policy_after против frozen reference_before (plan-09 §6.4, `DpoTrainer.cs:42-65`); ACCEPT_ROW `loss<0.693` применяется ПОСЛЕ ≥1 DPO-шага, не к cold-start итерации 0 (где margin≡0 ⇒ loss=ln2)**; held-out reward_margin≥0, loss<0.693, After≥Before+0.02 | sa-loop | Ф7.1.2, Ф5.2.2, Ф4.2.3 | remaining | INT, E2E, TA | 09 §3 Шаг1-5; 09 §10 п.1-3 |
| Ф7.2.2 | `ShouldStop` (threshold/plateau/budget/no_signal; threshold — режимная КОНЪЮНКЦИЯ по блоку 09 §3 Шаг6) + промоушен по дельте (только рост held-out) + append-only ledger + кумулятивность (regression window) | Ф7.2.1; `AutonomousLearningPipeline.cs:137-140`; блок 09 §no-signal | Условие остановки + промоушен-гейт | stopReason ∈ {threshold,plateau,budget,no_signal}; **threshold в режиме teacher-on требует `quality И teacher_gap_closed` (epsilon); в режиме self-best — только `quality`**; no-signal ветка: `NO_AUTO_DPO_SIGNAL=true`, `accepted_auto_pair_count_after_all_gates=0`, `ShouldStopResult.stop_reason=no_signal`, no training, no promotion, previous checkpoint retained; промоушен только при росте (иначе `ACCURACY_NOT_IMPROVED`); regression-ретест прошлых входов | sa-loop | Ф7.2.1 | remaining | BL, REG, TA, NEG | 09 §3 Шаг6; 09 §10 п.4-6; 09 §no-signal |

---

## Ф8 — Замена скрытого детерминизма (cutover на обучаемые юниты)

**Что решает:** закрывает все скрытые детерминистические финалы (анти-masquerade).
Классифицирует 9 механизмов, заменяет финал ребра обученным `SemanticEdgeRouterUnit`,
понижает baseline-роутеры до генераторов кандидатов (не удаляет), вводит
`CandidateOrderLeakageGuardUnit`.

**Exit-gate Ф8 (измеримо, блок 12 §10):**
- `classified_mechanism_count == 9`; каждый forbidden/baseline имеет blueprint;
- blueprint называет trainable-класс (`SemanticEdgeRouterUnit`), typed state
  (`SemanticEdgeRoutingState`), training-algo, excluded-leakage;
- `CandidateOrderLeakageGuardUnit` специфицирован + negative-тест;
- legal-teacher поля специфицированы + правило DATASET_REBUILD_REQUIRED;
- ablation-gate специфицирован (прогон без обученного юнита);
- every code quote has file:line; 0 новых уровней;
- cutover через latest-wins: `ResolveLatest` отдаёт обученный поверх baseline;
  baseline остаётся с `IsLearned=false` для ablation.

**Подфазы:** Ф8.1 (классификация 9 механизмов + блюпринты), Ф8.2 (cutover latest-wins + guard + ablation-доказательство).

### Ф8.1 — Классификация механизмов + блюпринты замены

**Что решает:** дать каждому из 9 механизмов вердикт и блюпринт замены (per-method,
не «заверни в unit»).

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф8.1.1 | Инвентаризация + классификация 9 механизмов (KeywordUnitScorer, MultiSubPhaseRouterUnit, SubPhaseRouterUnit, ScenarioPhasePlanner, RoutingProfileTrainer, KnnLogicalUnit, ResolveLatest, BuildPathFreeScoringText, confidence-floor) по правилу вердикта §3C | Ф7.2.2; `KeywordUnitScorer.cs:38-52`, `MultiSubPhaseRouterUnit.cs:283`, `InMemoryLogicalUnitRegistry.cs:118` | Сводная таблица вердиктов (9 строк) | `classified_mechanism_count == 9`; каждый вердикт ∈ {ALLOWED/OK_INTERNAL/OK_MODEL/BASELINE/FORBIDDEN}; every quote has file:line | sa-determinism | Ф7.2.2 | remaining | BL, NEG | 12 §6.1/§10 |
| Ф8.1.2 | Блюпринты замены для FORBIDDEN/BASELINE: `SemanticEdgeRouterUnit` (NEW) + `SemanticEdgeRoutingState` (typed) + training-algo + excluded-leakage; DPO позитив/rejected/quarantine примеры | Ф8.1.1, Ф4.3.2; блок 12 §6.2 | Блюпринты (per-method) с named классами | каждый forbidden/baseline имеет blueprint; назван trainable-класс + typed state + training-algo + excluded-leakage список | sa-determinism | Ф8.1.1, Ф4.3.2 | remaining | U, BL | 12 §6.2/§10 |

### Ф8.2 — Cutover latest-wins + guard + ablation

**Что решает:** подключить обученный финал ребра через реестр (baseline остаётся),
guard от order-leakage, доказать ablation.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф8.2.1 | `CandidateOrderLeakageGuardUnit` (NEW): перестановка кандидатов не меняет выбор; diagnostics/guard, не финал | Ф8.1.2; образец `SkillSelectionLogicalHierarchy.cs:271-274` | NEW guard-юнит + negative-тест | guard PASS при инвариантности; FAIL при position-feature (блокирует); negative-кейс с искусственной позицией → FAIL | sa-determinism | Ф8.1.2 | remaining | U, NEG, leakage | 12 §6.3/§5.3 |
| Ф8.2.2 | Cutover: `ResolveLatest` отдаёт обученный `SemanticEdgeRouterUnit` вместо baseline (latest-wins, A-LM-03); baseline `MultiSubPhaseRouterUnit`/`KeywordUnitScorer` → генератор кандидатов (`isLearned:false`); ablation-доказательство | Ф8.2.1, Ф7.2.2; `InMemoryLogicalUnitRegistry.cs:107-118`; plan-11 two-PASS | Cutover-решение + ablation-прогон | обученный юнит выбран поверх baseline; baseline НЕ удалён (для ablation); **cutover запрещён, пока selected edge unit не имеет `PASS_AS_TRAINED_FEATURE` по plan-11 two-PASS** (held-out + ablation + lifecycle + no-UNKNOWN); ablation: без обученного accuracy просаживается ≥ порог | sa-determinism | Ф8.2.1, Ф7.2.2, plan-11 trained-acceptance | remaining | INT, E2E, TA, NEG, REG | 12 §3 Шаг E-F; 12 §10; 11 §8 |

---

## Ф9 — Тесты/QA полная матрица (FC-26)

**Что решает:** доказывает, что граф работает И что обученные узлы реально обучились.
Сквозная тест-стратегия: матрица 14 строк P-01..P-14, 9 слоёв, per-edge контроли,
обе колонки честности (UH+TA) для 3 обучаемых веток.

**Exit-gate Ф9 (измеримо, блок 13 §10):**
- матрица содержит ровно 14 строк P-01..P-14, 0 пустых клеток (9 колонок слоёв);
- каждая ✔ несёт конкретную assertion/Gate (file:line эталона или числовой порог);
- 3 IsTrainable-ветки (P-01/P-02/P-03) несут обе колонки UH+TA (3×2=6 обязательных);
- P-12 continue-training прогоном (не флагом);
- 6 гейтов §8 сопоставлены ≥1 строке;
- каждый NEW-тест-класс сопоставлен пробелу из §7;
- регрессия: baseline ≥ 1488 тестов зелёный.

**Подфазы:** Ф9.1 (структурные слои графа), Ф9.2 (генератор DPO + teacher-provenance), Ф9.3 (финальная матрица + честность + регрессия).

### Ф9.1 — Структурные тесты графа (fan-out/in/many-to-many/on-the-fly)

**Что решает:** unit/business-logic/integration слои для топологии и рёбер графа.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф9.1.1 | `DynamicGraphTopologyMutationTests` (NEW): fan-out/fan-in/many-to-many/multi-output + on-the-fly мутация (ребро между исполняющимися узлами, инвариант сохранён) | Ф8.2.2, Ф1.3.1; `TopologyMutationFlowTests.cs:7-64` | NEW структурный тест-класс | fan-out/in/many-to-many покрыты; on-the-fly мутация сохраняет инвариант; verdict + lookup PASS | sa-qa | Ф8.2.2, Ф1.3.1 | remaining | U, BL, INT, NEG | 13 §3.3/§9 P-04/P-05/P-06 |
| Ф9.1.2 | `GraphEdgeDeterminismGuardTests` + `GraphEdgeLeakageAblationTests` (NEW, per-edge): no-hidden-determinism на каждом ребре + ablation/permutation/source-id на каждом ребре | Ф9.1.1, Ф8.2.1; `PromptToTaskMachineSkeletonTests.cs:79-85`, `SkillSelectionPhase06...:269-305` | NEW per-edge тест-классы | каждое ребро: рефлексия no-determinism; `CandidateOrderInvariant=true`; `SelectedFromSourceIdOnly=false`; 5 ablation-контролей | sa-qa | Ф9.1.1, Ф8.2.1 | remaining | UH, TA, NEG, leakage | 13 §3.2/§9 P-03 |

### Ф9.2 — Тесты генератора DPO + teacher-provenance + continue-training

**Что решает:** слои генератора пар, легального гейта, реального continue-training.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф9.2.1 | `GraphDpoGeneratorTests` + `GraphDpoTeacherProvenanceGateTests` (NEW): train-ready записан / Set Quarantine bad-taxonomy / no-raw-text / leakage-exclusion; permissive→Pass, closed→DATASET_REBUILD_REQUIRED | Ф3.2.2, Ф2.2.2; `ResultGapPhase07DpoWriterTests.cs:6-236` | NEW тест-классы генератора + teacher-гейта | `Written=true` для train-ready; bad-taxonomy → Quarantined+файл НЕ создан; closed teacher → REBUILD; no-raw-text | sa-qa | Ф3.2.2, Ф2.2.2 | remaining | U, BL, NEG, QA | 13 §3.4/§3.5/§9 P-08/P-09/P-10/P-11 |
| Ф9.2.2 | `GraphNodeContinueTrainingTests` (NEW): continue-training ПРОГОНОМ (merge сохраняет старое + добавляет новое; дубликат отвергнут; нет состояния → отказ) | Ф4.2.1, Ф5.2.2; `OrchestrationReasoningMachineContinueTrainingTests.cs:13-19` | NEW continue-training тест | continue-training доказан прогоном (не флагом); merge корректен; дубликат/пустое состояние отвергнуты | sa-qa | Ф4.2.1, Ф5.2.2 | remaining | TA, INT, REG | 13 §3.2 Шаг6/§9 P-12 |

### Ф9.3 — Финальная матрица + честность + регрессия

**Что решает:** собрать матрицу 14×9 без дыр, проверить обе колонки честности,
прогнать регрессию.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф9.3.1 | Матрица «компонент → слои → assertion/Gate» (14 строк P-01..P-14, 9 колонок) + сопоставление 6 гейтов §8 + NEW-классы ↔ пробелы §7 | Ф9.1.2, Ф9.2.1, Ф9.2.2 | Заполненная матрица 14×9 | `строк==14` AND `пустых_клеток==0` AND `IsTrainable_с_обеими_колонками==3` AND `гейтов_сопоставлено==6`; каждая ✔ с assertion/Gate | sa-qa | Ф9.1.2, Ф9.2.1, Ф9.2.2 | remaining | все 9 слоёв | 13 §9/§10 |
| Ф9.3.2 | Регрессия + анти-loophole (FM-1/FM-3/FM-6/FM-7/FM-8): coverage-vs-execution, trainable-architecture-evidence, ui/ux scope, stub-fix masking | Ф9.3.1; `test-integrity-SKILL.md` | Регрессионный прогон + анти-loophole отчёт | baseline ≥ 1488 тестов зелёный; FM-3/FM-8 PASS для каждой строки; нет ослабления gate для «позеленить» | sa-qa | Ф9.3.1 | remaining | REG, QA | 13 §9 анти-loophole |

---

## Ф10 — API/клиенты (figma→html+css; better-output→DPO; тонкий агент R-11)

**Что решает:** прикладные швы наружу, ради которых граф обучается. Держит ядро в
llm_mvp_core (R5), агентный слой тонкий (R-11: только API-вызовы; не путать с
требованием R6 про рекон-библиотеку). Два шва: figma→html+css
(FLOW-31 + FigmaToCode + CanvaToCode) и better-output→DPO (FLOW-38 + Smart*).

**Exit-gate Ф10 (измеримо, блок 14 §10 адаптировано на реализацию):**
- шов figma→html+css: FLOW-31 `AnalyzeDesignAsync` → `GetOrchestratorAsync` →
  `RunAsync`; алгоритм генерации/выбора остаётся в llm_mvp_core; клиенты
  FigmaToCode/CanvaToCode тонкие (R-11: 0 строк алгоритм-логики в агенте);
- шов better-output→DPO: FLOW-38 outcome+score → `GraphDpoPair`; better-output
  проходит legal-гейт (closed → DATASET_REBUILD_REQUIRED);
- лицензии выходов клиентов классифицированы (common-training vs user-adaptive, R5/Ф0 Q38);
- e2e через реальный host-entrypoint клиента (или явно scoped handler-evidence);
- 0 строк переносят алгоритм-логику в агентный слой (R-11);
- **условный гейт Q41 (R15):** если Luba выбрала Вариант B (Q41, мастер стр.736) —
  до закрытия Ф10 активируется условная подфаза Ф10.3 и добавляется по одному
  подшагу на домен (small-model / language-translate / local-wiki / extensions /
  documentation-learning / session-start); пока
  Q41 не Вариант B, Ф10 закрывается без Ф10.3 (резерв остаётся неактивным).

**Подфазы:** Ф10.1 (шов figma→html+css), Ф10.2 (шов better-output→DPO + тонкий агент).

### Ф10.1 — Шов figma → html+css

**Что решает:** подключить FLOW-31 и клиенты FigmaToCode/CanvaToCode к ядру через
тонкий API-вызов.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф10.1.1 | Шов дизайн→код: **точная роль FLOW-31 — design-АНАЛИЗ, а НЕ эмиссия html+css.** `DesignIntelligenceEngineService.AnalyzeDesignAsync` (читано :47-90) валидирует BFA-rule-pack, считает componentCount/tokenCount, апсертит analysis-документ, пишет outbox-события и возвращает `analysis_ready`/`needs_review` — он не генерирует html+css. Сама генерация html+css живёт ТОЛЬКО в клиентах (FigmaToCode `prepareCssForElement`, controller.ts). Шов: тонкий клиент шлёт задачу «дизайн→код» в ядро через API (`GetOrchestratorAsync().RunAsync`); алгоритм генерации/выбора в llm_mvp_core; FLOW-31 остаётся analysis/governance-sidecar (ИЛИ вводится НОВЫЙ design→code endpoint как шов). **Binding-mapping (R16 vs код):** требование R16 называет хост `xiigen-platform`, а реальный код шва — `xiigen-flows-dot-net-angular/FLOW-31` (`DesignIntelligenceEngineService.cs:47-90`, проверено на диске); в `xiigen-platform-dot-net-angular` НЕТ `DesignIntelligence*`/figma→code сервиса (Glob = No files found), `xiigen-platform` отдаёт design→code только через marketplace-клиентов (`Smart*` в `Xiigen.Platform.Marketplace`). | Ф9.3.2; `DesignIntelligenceEngineService.cs:47-90` (xiigen-flows), `controller.ts:3` (FigmaToCode), `LearningMachineContracts.cs:232` | Точка вызова ядра из FLOW-31/клиента + контракт задачи + явная binding-mapping-строка (xiigen-platform↔xiigen-flows/FLOW-31) + лейбл роли FLOW-31 | клиент/FLOW-31 вызывает ядро через API-шов; 0 строк алгоритм-логики в FLOW-31; **`flow31_role_labelled = design_analysis_not_code_generation`**; **`html_css_generation_owner_named = client(FigmaToCode/CanvaToCode)`**; **`requirement_host_project_mapping_recorded == true`** (несоответствие `xiigen-platform`↔`xiigen-flows/FLOW-31` названо явно); **`figma_host_project_locked == true` (Ф0 Q-host) И принятая интерпретация R16 зафиксирована строкой «не сводить API xiigen-platform к xiigen-flows без пометки»** (дизъюнкция «ИЛИ вынесена открытым вопросом» УБРАНА — подшаг нельзя закрыть с открытым вопросом; реализация шва зависит от выбора места endpoint); **два уровня e2e разведены честно:** (a) **core-seam e2e** = задача «дизайн→код» доходит до `GetOrchestratorAsync().RunAsync` (canon plan-13 §3 стр.62-64) — это CORE-evidence, НЕ host-entrypoint клиента; (b) **client-host e2e** для FigmaToCode/CanvaToCode = либо explicitly-scoped handler-evidence (плагин Figma `figma.showUI` нельзя гонять обычным host-тестом — пометить явно), либо назван конкретный механизм плагинного e2e (Figma plugin sandbox / Canva SDK harness); ЗАПРЕЩЕНО выдавать core `GetOrchestratorAsync`-прогон за host-entrypoint клиента | sa-api | Ф9.3.2, Ф0.1.3 | remaining | E2E (core-seam + client-host scoped), INT, UX | 14 §6.1; 14 P-11; plan-13 §3 (e2e-host определение) |
| Ф10.1.2 | Клиенты FigmaToCode (`controller.ts`) + CanvaToCode (`canva-adapter.ts`): тонкие хосты (R6), отправляют задачу через FLOW-31→ядро; «ML classifier» плагина помечен baseline. **Провенанс-нота (R16↔R17, CANON-инвариант 2):** `controller.ts:14` (читано) показывает генерацию имён, назначенных Claude («Design Gen session state — maps Claude-assigned names → real Figma IDs»), т.е. выход клиента FigmaToCode может быть Claude-derived. Любой сгенерированный html+css FigmaToCode/CanvaToCode, попадающий в DPO, обязан нести `TeacherProvenance`; при `closed_proprietary_model=true` и обучении он BLOCK / `DATASET_REBUILD_REQUIRED` (допустим только как judge/eval или style-only a_side). | Ф10.1.1; `controller.ts:1/14/53-62`, `canva-adapter.ts` | Тонкие клиенты design→code + провенанс-нота закрытого teacher на выходе клиента | клиенты тонкие (R6: 0 алгоритм-логики); **client-host entrypoint существует и его e2e — explicitly-scoped handler-evidence (Figma `figma.showUI`/Canva SDK нельзя гонять обычным host-тестом) ИЛИ названный плагинный механизм (Figma plugin sandbox / Canva SDK harness); НЕ core `GetOrchestratorAsync` выдаётся за host клиента**; «ML classifier» помечен baseline/helper; **`client_generated_output_provenance_checked == true`**; **closed-derived figma→code выход ⇒ не chosen для обучения (`DATASET_REBUILD_REQUIRED`)** (кросс-линк на Ф10.2.1); **plan-13 P-14 теперь несёт явную assertion клиентского шва/R16 через proof-field gate client-host/R16 seam evidence, не косвенно через R16** | sa-api | Ф10.1.1 | remaining | E2E (client-host scoped), UX, NEG | 14 §6.5; 14 P-11/P-14; plan-13 P-14 |

### Ф10.2 — Шов better-output → DPO + тонкий агент

**Что решает:** петля улучшения локальной модели из фидбека клиентов; граница агента.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф10.2.1 | FLOW-38 `RecordFeedbackAsync` outcome+score → `GeneratedUiOutputFeedbackEvent` → `GraphDpoPair` (chosen=better-output, rejected=worse) через builder Ф3; better-output проходит legal-гейт. **R17-сценарий — клиенты xiigen (FLOW-38/Smart*) в проде:** better-output чаще всего приходит из собственной модельной интеграции клиента с НЕИЗВЕСТНЫМ/третьесторонним провенансом. **Закрыт путь «отмыва»:** marker-absence ≠ open-weight. Любой client/customer-origin better-output обязан нести явный `SourceCategory` и по умолчанию = QUARANTINE + `semantic_quality_claimed=false`, пока провенанс не установлен ПОЛОЖИТЕЛЬНО (allowlist-match), никогда не молчаливый ACCEPT при отсутствии маркера. Значения `chosen_origin=client_supplied_unverified ⇒ QUARANTINE` (определено в Ф3.1.2) и `SourceCategory ∈ {client_unverified, third_party}` (определено в Ф2.1.1; ветка-вердикт C16 client-origin определена в Ф2.1.2) — этот лист ССЫЛАЕТСЯ на уже-определённые в апстриме значения, а не вводит их впервые. **judge→teacher (self_judge):** если better/worse выбран любой judge-моделью, строка тегается `SourceCategory=self_judge` и обязана пройти legal-гейт (closed judge допустим ТОЛЬКО при `teacher_output_used_for_training=false`; если судейский «better» становится chosen, `used_for_training=true` и closed ⇒ `DATASET_REBUILD_REQUIRED`). | Ф10.1.2, Ф7.1.2, Ф2.1.2, Ф3.1.2; `RagQualityFeedbackService.cs:48`; `GeneratedUiOutputFeedbackEvent` contract | Конверсия фидбека → event → пара + правила client-origin/self_judge провенанса | outcome+score → `GeneratedUiOutputFeedbackEvent` с `output_provenance`; event may_build_graph_dpo_pair=true только при legal_verdict=ACCEPT; better-output от closed → `DATASET_REBUILD_REQUIRED`; chosen легален; **client/customer-origin без положительного провенанса ⇒ QUARANTINE (не ACCEPT по отсутствию маркера)**; **`chosen_origin` поддерживает `client_supplied_unverified ⇒ QUARANTINE`**; **`SourceCategory` имеет client/third_party значение**; **`self_judge_train_ready_without_legal_gate == 0`** (блок 08 §8.4 / C14); judge-выбранный «better», ставший chosen, при closed ⇒ `DATASET_REBUILD_REQUIRED` | sa-api | Ф10.1.2, Ф7.1.2, Ф2.1.2, Ф3.1.2 | remaining | INT, NEG, BL | 14 §6.2; 14 P-12 |
| Ф10.2.2 | Smart*-клиенты lifecycle (`SmartCodeAssistantService` export/import/locator; `SmartWikiService` abstention=honest negative) + лицензии выходов (common vs user-adaptive, R5; политика по канону Q38, Ф0.1.3) + docs-sync | Ф10.2.1, Ф4.3.3, Ф0.1.3 (Q38 политика лицензий); `SmartCodeAssistantService.cs:44/133`, `SmartWikiService.cs:217` | Стыковка клиентского lifecycle + классификация лицензий | Smart* export/import стыкуется с `IModelRepository`/locator; abstention=образец rejected; лицензии выходов классифицированы по политике Q38 (`output_license_policy_locked`, Ф0.1.3); docs обновлены | sa-api | Ф10.2.1, Ф4.3.3, Ф0.1.3 | remaining | INT, UX, REG | 14 §6.3/§6.4; 14 P-12/P-13; R10/R17 |

### Ф10.3 — R15 доп.домены (УСЛОВНАЯ подфаза — активируется ТОЛЬКО при Q41 = Вариант B)

**Что решает (условно):** Q41 «Вариант B» (мастер стр.736) требует «по подшагу на
домен в Ф10 доски». Чтобы выбор Варианта B НЕ был невидимой правкой единственного
источника истины (мастер §D.1), эти слоты зарезервированы здесь явно как
**scope-gated резерв (вне baseline-59)**. **Статус каждого слота = `blocked`** (из
разрешённого словаря §0.2, ровно 5 значений; причина — ожидание решения Luba Q41=Вариант B);
`scope-gated/вне-59` — это пометка ОХВАТА, а НЕ 6-й статус. При выборе Варианта B слоты
переходят в `remaining` и входят в счётчики; пока Q41 ≠ Вариант B — остаются `blocked`
резервом вне baseline-59, и Ф10 закрывается без них.

| ID (резерв) | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф10.3.1 | Шов домена **small-model** (слот, наполняется при Вариант B) | Ф10.2.2; Q41=Вариант B | Шов small-model→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59); при активации — тонкий клиент R-11 + ядро в llm_mvp_core | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |
| Ф10.3.2 | Шов домена **language-translate** (слот) | Ф10.2.2; Q41=Вариант B | Шов language-translate→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59) | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |
| Ф10.3.3 | Шов домена **local-wiki** (слот) | Ф10.2.2; Q41=Вариант B | Шов local-wiki→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59) | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |
| Ф10.3.4 | Шов домена **extensions** (слот) | Ф10.2.2; Q41=Вариант B | Шов extensions→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59) | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |
| Ф10.3.5 | Шов домена **documentation-learning** (слот) | Ф10.2.2; Q41=Вариант B | Шов documentation-learning→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59) | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |
| Ф10.3.6 | Шов домена **session-start** (слот) | Ф10.2.2; Q41=Вариант B | Шов session-start→ядро (R15) | активируется (→`remaining`) при `Q41==Вариант B`; иначе `blocked` (scope-gated резерв вне baseline-59) | sa-api | Ф10.2.2, Q41 (ждёт Luba Q41=Вариант B) | blocked | E2E, INT | мастер Q41 стр.736; R15 |

> **Счётчики:** Ф10.3.* — статус `blocked` (scope-gated резерв), НЕ учитываются в
> базовых 12 фаз / 26 подфаз / 59 подшагов (план-only baseline = Вариант ≠ B). При выборе
> Варианта B эти 6 слотов переходят в `remaining`, и счётчики Ф10/итог пересчитываются
> (Ф10 → 3 подфазы / 10 подшагов, итог → +6 подшагов); до этого baseline-счётчики §1
> неизменны. «scope-gated/вне-59» — пометка охвата, а НЕ отдельный статус-словарь.

---

## Ф11 — Финальная приёмка (full completion gate + 10 execution reviews)

**Что решает:** доказывает, что весь план реализован: три полных швa (если применимо),
обученный граф с lifecycle, легальные данные, тесты/QA, нет скрытого детерминизма,
ядро в llm_mvp_core. Запускает 10 реальных независимых execution reviews.

**Exit-gate Ф11 (измеримо):**
- completion coverage matrix: каждая строка requirement→phase→evidence (§B.1
  мастер-плана, R-01..R-20) = `доказано`; 0 строк `NOT_PROVEN`;
- two-PASS: каждый обучаемый узел/ребро = `PASS_AS_TRAINED_FEATURE` (held-out +
  ablation + fresh-load + continue-training + e2e) ИЛИ честно `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`;
- 10 реальных независимых execution reviews (gpt-5.5/xhigh), каждый non-PASS не
  считается; каждый review: agent id, scope, verdict, findings, recheck;
- `STATE.json`: `goal_reached=true`, `ARCHITECT_VERDICT_ALLOW_FINAL` записан, hole
  register пуст;
- live git/state согласованы (если коммит-граница пройдена).

**Подфазы:** Ф11.1 (completion coverage matrix), Ф11.2 (10 execution reviews + финальный вердикт).

### Ф11.1 — Completion coverage matrix

**Что решает:** свести requirement→phase→evidence и доказать каждую строку.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф11.1.1 | Матрица requirement→phase→evidence (R-01..R-20) — каждая строка с evidence-артефактом и статусом | Ф10.2.2; мастер §D.3 | Заполненная матрица завершения | каждая из R-01..R-20 = `доказано`; 0 строк `NOT_PROVEN`; два-PASS метка на каждый обучаемый элемент | архитектор (ревью) | Ф10.2.2 | remaining | — (матрица) | мастер §D.3; CLAUDE.md completion matrix |

### Ф11.2 — 10 execution reviews + финальный вердикт

**Что решает:** 10 реальных независимых ревью через суб-агентов; финальный архитект-вердикт.

| ID | Что решает | Вход | Выход | Gate B (измеримо) | Owner | Зависит от | Статус | Тест-слои | Блок-источник |
|---|---|---|---|---|---|---|---|---|---|
| Ф11.2.1 | 10 реальных независимых execution-ревью реализации (код/runtime/тесты): три швa, no-hidden-determinism, llm_mvp_core ownership, trainable lifecycle, DPO/source quality, тесты/e2e/QA, docs, live runtime; затем финальный архитект-вердикт | Ф11.1.1 | 10 review-пакетов (исполняют суб-агенты) + финальный вердикт (архитектор) | 10 PASS-ревью (non-PASS не считается); каждый: agent id/scope/verdict/findings/recheck; `goal_reached=true`; точный токен `ARCHITECT_VERDICT_ALLOW_FINAL`; visibility sub-ledger: §B.6 | review-gate owner = архитектор (Opus 4.8 MAX); исполнители 10 ревью = суб-агенты gpt-5.5/xhigh (10 независимых пакетов); архитектор владеет только финальным вердиктом `ARCHITECT_VERDICT_ALLOW_FINAL` | Ф11.1.1 | remaining | все слои (через ревью) | CLAUDE.md Final Execution Review Gate; мастер §B; доска §B.6 |

---

# §B. EXECUTION-VISIBILITY (как в любой момент понять, где мы)

> Этот раздел отвечает на вопрос Luby: «в любой момент — какая фаза/подфаза/подшаг
> сейчас, что решается, сколько done/осталось, и что остаётся». Это
> человеко-читаемая доска, не машинный лог.

## B.1. Где мы сейчас (snapshot — заполняется при исполнении)

> Этот snapshot ДОСЛОВНО несёт обязательный человеко-читаемый шаблон статуса
> (`plan-review-SKILL.md:640-651` + Sequential Execution-Control Gate): помимо
> указателей фаза/подфаза/подшаг — поля «Сейчас / Что найдено / Что исправлено /
> Что решает фаза / Что осталось» и встроенные счётчики. Технический label
> (`done|in-progress|remaining|review-block|blocked`) допустим ТОЛЬКО в скобках.

```text
CURRENT Ф<P>.<SP>.<SS>:  <id текущего in-progress подшага>   (на момент плана: НЕ НАЧАТО)
ЧТО РЕШАЕТСЯ СЕЙЧАС:     <поле «Что решает» текущего подшага>
ТЕКУЩАЯ ФАЗА:            Ф<P> — <название>
ЧТО РЕШАЕТ ФАЗА:         <одно простое русское предложение — что решает текущая фаза>
ТЕКУЩАЯ ПОДФАЗА:         Ф<P>.<SP> — <название>
OWNER ТЕКУЩЕГО ПОДШАГА:  <Luba (решение) | архитектор (ревью) | роль-класс sa-* + назначенный НИК суб-агента gpt-5.5/xhigh>
                          (роль-класс взят из ячейки Owner; конкретный ник назначается
                           в work order при старте подшага и пишется здесь;
                           Ф0.1.* → owner=Luba (решение); Ф11.1.1 → owner=архитектор;
                           Ф11.2.1 → архитектор владеет review-gate/вердиктом, а 10
                           execution-ревью исполняют суб-агенты gpt-5.5/xhigh;
                           Ф1–Ф10 → суб-агент (роль-класс sa-*); см. таксономию §0.4)
СЕЙЧАС:                  <человеческий перевод статуса; технический label
                          done|in-progress|remaining|review-block|blocked только в скобках>
ЧТО НАЙДЕНО:             <что REVIEW_INTERNAL / Gate B-проверка увидела, или «пока не проверялось»>
ЧТО ИСПРАВЛЕНО:          <что fix изменил, или «пока ничего не менялось»>
ЧТО ОСТАЛОСЬ:            <оставшаяся работа и boundary простыми словами>
EXIT-GATE ТЕКУЩЕЙ ФАЗЫ:  <измеримое условие закрытия фазы>
СЛЕДУЮЩИЙ ПОДШАГ:        <id следующего по порядку>  (откроется после Gate B текущего)
СЧЁТЧИКИ ФАЗЫ:           сделано <X>/<Y>, осталось <Y-X>/<Y>   (см. §B.2)
СЧЁТЧИКИ ВСЕГО ПЛАНА:    сделано <Z>/59, осталось <59-Z>/59    (см. §B.2)
```

## B.2. Счётчики прогресса (заполняются при исполнении)

> Разбиение **исчерпывающее, непересекающееся и суммируется в фиксированный итог**
> при любом состоянии. `remaining` = только ещё-не-начатое (review-block и blocked
> считаются ОТДЕЛЬНО, не входят в remaining). `in-progress P ∈ {0,1}` во всём плане
> (единственная глобальная дорожка, §0.5; не захардкожена 1: в ремонте/на финише P может
> быть 0).

```text
ФАЗЫ:     done X + in-progress P + remaining = 12            (P ∈ {0,1})
ПОДФАЗЫ:  done Y + in-progress P + remaining = 26            (P ∈ {0,1})
ПОДШАГИ:  done Z + in-progress P + remaining R + review-block W + blocked V = 59   (P ∈ {0,1})
```

На момент плана: `done 0`, `in-progress 0`, `remaining 59`, `review-block 0`,
`blocked 0` (0+0+59+0+0=59), всё в ожидании approve Luba и закрытия Ф0.
(Условные Ф10.3.* — статус `blocked`, scope-gated резерв вне этих 59, см. Ф10.3.)

## B.3. Доска фаз (плоский список с прогрессом)

| Фаза | Название | Подфазы | Подшаги | Exit-gate (краткий) | Статус фазы |
|---|---|---|---|---|---|
| Ф0 | Решения Luba | 1 | 3 | 7 решающих вопросов закрыты | remaining |
| Ф1 | Контракты и фундамент графа | 3 | 9 | NEW-контракты + 0 новых уровней + 0 правок L1..L5 | remaining |
| Ф2 | Легальный teacher-гейт + GraphDpoPair | 2 | 5 | 18 полей provenance + ветки вердикта C2-C15b (denylist-by-id; C15/C15b = triad gate) + триада (`triad_field_presence_rate==1.0`) + 5 форматов→1 канон + манифест только ACCEPT (QUARANTINE не в TrainBatchAsync) | remaining |
| Ф3 | DPO-строитель | 2 | 5 | BuildPairAsync валидная пара + permutation-инвариантность + материализация | remaining |
| Ф4 | Обучение + lifecycle | 3 | 8 | LoRA wired+trained + margin из движка + lifecycle полный + обучаемый узел level=unit (Ф4.3.1) + split R5 | remaining |
| Ф5 | Измерение | 2 | 5 | held-out≠live + two-PASS + per-edge контроли + 0 UNKNOWN | remaining |
| Ф6 | Async мульти-комбинации | 2 | 4 | 4 async + honest winner + пары overlap<0.60 + honest-ветки | remaining |
| Ф7 | Авто-DPO петля | 2 | 4 | канал не пуст + гейты PASS + held-out delta + ShouldStop | remaining |
| Ф8 | Замена скрытого детерминизма | 2 | 4 | 9 классиф. + блюпринты + guard + cutover latest-wins + ablation | remaining |
| Ф9 | Тесты/QA матрица | 3 | 6 | 14 строк / 0 пустых клеток / UH+TA×3 / baseline≥1488 | remaining |
| Ф10 | API/клиенты | 2 | 4 | 2 швa + ядро в llm_mvp_core + тонкий агент R-11 + legal-гейт | remaining |
| Ф11 | Финальная приёмка | 2 | 2 | coverage matrix 0 NOT_PROVEN + 10 reviews + ALLOW_FINAL | remaining |

## B.4. Список оставшегося (на момент плана — все 59 подшагов)

На момент плана оставшиеся подшаги = ВСЕ 59 (`remaining`):
Ф0.1.1-3 · Ф1.1.1-3, Ф1.2.1-3, Ф1.3.1-3 · Ф2.1.1-3, Ф2.2.1-2 · Ф3.1.1-3, Ф3.2.1-2 ·
Ф4.1.1-2, Ф4.2.1-3, Ф4.3.1-3 · Ф5.1.1-3, Ф5.2.1-2 · Ф6.1.1-2, Ф6.2.1-2 ·
Ф7.1.1-2, Ф7.2.1-2 · Ф8.1.1-2, Ф8.2.1-2 · Ф9.1.1-2, Ф9.2.1-2, Ф9.3.1-2 ·
Ф10.1.1-2, Ф10.2.1-2 · Ф11.1.1, Ф11.2.1.

При исполнении (одно определение, согласованное с §B.2 — review-block и blocked
считаются ОТДЕЛЬНО, не складываются в `remaining`):
- `remaining` = только ещё-не-начатые подшаги;
- `review-block` = исполнено, но Gate B провален (в ремонте) — отдельный счётчик;
- `blocked` = заблокировано внешней причиной — отдельный счётчик;
- `done` = завершённые; текущее = единственный `in-progress` во всём плане (одна глобальная дорожка, §0.5).
«Незакрытая работа» в человеческом смысле = `remaining + review-block + blocked`,
но в счётчиках §B.2 это ТРИ отдельных слагаемых, а не одно `remaining`.

## B.5. Как обновлять доску (протокол видимости)

1. При старте подшага: статус `remaining` → `in-progress`; заполнить B.1 snapshot.
2. При прохождении Gate B: `in-progress` → `done`; инкремент счётчиков B.2; открыть
   следующий по порядку подшаг (проверив поле «Зависит от»).
3. При провале Gate B: `in-progress` → `review-block`; не открывать следующий;
   вернуть в ремонт того же ID (ARCHITECT_INTERNAL_REPAIR).
4. При закрытии последнего подшага подфазы: проверить exit-gate подфазы; затем открыть
   первую подфазу следующей по порядку (только если exit-gate фазы достигается её
   последней подфазой).
5. При закрытии последней подфазы фазы: проверить exit-gate фазы; только при 100% —
   открыть Ф`<P+1>`.1.1.
6. Производная подфаза закрывается только после закрытия всех своих подшагов и своего
   exit-gate; нельзя открывать следующую производную подфазу по одному успешному
   подшагу внутри текущей.
7. Scope-gated reserve rows Ф10.3.1..Ф10.3.6 не участвуют в выборе "следующего"
   baseline-подшага, пока Luba не активирует Q41 Variant B; активный next-step ищется
   по baseline-59, пропуская inactive reserve rows.

## B.6. 10-review visibility sub-ledger for Ф11.2.1

This ledger is visibility for the future execution-review gate inside `Ф11.2.1`.
It does not add phases, subphases, or substeps; §B.2 remains 12 / 26 / 59. It is
not §D/R20 plan-review depth and must not be counted as a 50-cycle plan review.

| Review # | Reviewer id | Scope | Verdict | Findings | Fix/recheck evidence | counts_toward_10 |
|---|---|---|---|---|---|---|
| 1 | TBD | three seams | TBD | TBD | TBD | false |
| 2 | TBD | no-hidden-determinism | TBD | TBD | TBD | false |
| 3 | TBD | llm_mvp_core ownership | TBD | TBD | TBD | false |
| 4 | TBD | trainable lifecycle | TBD | TBD | TBD | false |
| 5 | TBD | DPO/source quality | TBD | TBD | TBD | false |
| 6 | TBD | tests/e2e/QA | TBD | TBD | TBD | false |
| 7 | TBD | docs/readback | TBD | TBD | TBD | false |
| 8 | TBD | live runtime | TBD | TBD | TBD | false |
| 9 | TBD | cross-repo branch authority | TBD | TBD | TBD | false |
| 10 | TBD | final integration risk | TBD | TBD | TBD | false |

`counts_toward_10=true` only for a real returned PASS packet with reviewer id,
scope, findings/no-fix rationale, and recheck evidence. Non-PASS, summaries, and
ledger rows without a returned packet stay `false`.

---

# §C. Трассировка требований R1-R21 → фазы/подшаги

> Каждое требование задания Luby покрыто фазами доски. Это доказывает, что
> перестройка в доску ничего не потеряла.

> **КАНОН covered/partial (2026-06-24 current-run honest state + branch-authority amendment — идентично матрице §4 и мастеру §D.2).**
> `requirements_total=21`; **`covered=15`** (R2, R3, R4, R5, R6, R7, R8, R10, R11, R12, R13,
> R18, R19, R20, R21); **`partial=6`** (R1, R9, R14, R15, R16, R17); `unmapped=0`. Из 6 partial:
> **R15/R16/R17 — решения Luba** (§E мастера: scope R15 / host-mapping R16 / origin-legality
> Q37/Q38 R17); **R6/R9 — честная мета** (passes-unproven; per-документный authoring);
> **R1/R14 — pending implementation/readback evidence** (R1 unit-подшаг есть, R14
> field-parity propagated, но runtime/data-manifest evidence ещё не является реализацией);
> **R20 covered/current-run complete**: `verified_subagent_packets_completed_current_run=50/50`.
> Старая W2c/old-50 пометка `covered=15/partial=5` quarantined/evidence-only. Структурный счёт
> подшагов: 12 фаз / 26 подфаз / 59 подшагов (§1).

| Требование | Суть | Покрыто фазами/подшагами |
|---|---|---|
| R1 | ILearningDynamicGraph строит DPO для разных ILearningUnit внутри машины | Ф3 (builder, классиф. level∈{unit,edge,graph}), **Ф4.3.1 (per-unit: `GraphNodePreferenceState`)**, Ф4.3.2 (per-edge: `EdgeAuthorityUnit`), Ф5.2.1 (измерение per-unit И per-edge), Ф7 (петля) |
| R2 | DPO строить только разрешёнными моделями (легальный список) | Ф0.1.3, Ф2.1 (гейт+реестр), Ф2.2.2 (гейт батча) |
| R3 | Все документы планирования в dpo legal build | Все артефакты `codex-*` в `dpo legal build/` (мета) |
| R4 | Динамический graph помогает создавать DPO | Ф1.1 (топология), Ф6 (комбинации), Ф7 (петля) |
| R5 | Роли: Unit=матрица; Flow=коррекция; Orchestrator=async; DynamicGraph=паттерны разного порядка | Ф1.2 (узел), Ф1.3.1 (ребро), Ф1.3.2 (оркестратор), Ф1.3.3 (L5) |
| R6 | Рекон-фон: проверенный library/evidence corpus по `llm_mvp` и `llm_mvp_core` → библиотека | `codex-recon-*` и knowledge-library evidence; exact file/report inventory допустим как evidence, но `passes_unproven=true`: количество реальных проходов не утверждается без ledger packet evidence |
| R7 | Авто-DPO из несуществующих данных (прогон→низкий результат→teacher goal→обучать) | Ф6 (N-комбинаций), Ф7.1 (слабость→goal→пара), Ф7.2 (цикл) |
| R8 | Async-прогон одного входа через несколько комбинаций | Ф6.1.2 (Task.WhenAll), Ф6.2 (judge/merge) |
| R9 | План строится через суб-агентов (Opus 4.8 MAX оркестрирует) | Owner-колонка по трём классам (§0.4): 54 подшага Ф1–Ф10 — суб-агенты gpt-5.5/xhigh (вкл. Ф4.3.1); 3 подшага Ф0 — Luba (решение, законное исключение из R9); 2 подшага Ф11 — архитектор (Ф11.1.1 doc/governance-ревью; Ф11.2.1 review-gate архитектора + 10 исполняющих суб-агентов). **Reconciliation: каноническое число суб-агент-исполнителей реализации = 54; «56» НЕ каноническое (ошибочный счёт Ф0 как суб-агент-работы).** Авторский facet R9 (написание плана суб-агентами) → мастер R-13 + R20-протокол (реальные пакеты), НЕ execution-owner-колонка; мета |
| R10 | Stage1 — паттерн ILearningMachine + скилы → гайды | Ф1 (контракты по гайдам), Ф2-Ф4 (DPO/обучение/измерение по гайдам); мета |
| R11 | Stage2 — разбить этап на под-задачи + референсы | Вся трёхуровневая структура (26 подфаз/59 подшагов). **Референс «на каждый этап» резолвится на исполняемом слое (см. §0.4 нота R11):** колонка «Блок-источник» каждого подшага → проекты+документация затрагиваемых компонентов через явный crosswalk codex-plan-14 §11.<SR>; для НЕ-Ф10 подшагов SR-строка резолвится правилом §0.4 (а не только прозой §11.0). Зависимость: codex-plan-14 §11.0 должен нести явную строку-сопоставление «каждый Ф<P>.<SP>.<SS> → SR-NN» для ВСЕХ 59 подшагов (plan-14 НЕ редактируется в этом задании — отмечено как зависимость доски) |
| R12 | Stage3 — человеческим языком + референсы кода + UML/решение/алгоритмы | §A фаз (что решает) + блок-источник каждого подшага (§1/§2/§3 в блоках) |
| R13 | Stage4 — все требования учтены, нет скрытых под-задач, обучение не прячет детерминизм | §C (эта таблица) + Ф8 (no-hidden-determinism) + Ф9 (матрица) |
| R14 | Легальный гейт: только open-weight permissive; никогда Claude/OpenAI/Gemini; provenance | Ф0.1.3, Ф2.1.1-2, Ф2.2.2, Ф4.2.3 (манифест), Ф10.2.1 (better-output) |
| R15 | Доп. референсы (small-model / language-translate / local-wiki / extensions / documentation-learning / session-start) | Ф10 (швы+клиенты), Ф10.3 (scope-gated резерв всех 6 доп.доменов при Q41=Вариант B: Ф10.3.1 small-model / Ф10.3.2 language-translate / Ф10.3.3 local-wiki / Ф10.3.4 extensions / Ф10.3.5 documentation-learning / Ф10.3.6 session-start), блок 14 карта (мета). **Q41 verbatim-sync: этот R15-список несёт ВСЕ 6 канонических доменов дословно (теми же дефисными токенами) — един с мастером §E.3 Q41 и матрицей §3.3; расхождение перечней по R15 = дефект Contradiction Sweep** |
| R16 | API figma→html+css на xiigen-platform; клиенты FigmaToCode/CanvaToCode | Ф0.1.3 (Q-host `figma_host_project_locked`), Ф10.1.1 (binding-mapping xiigen-platform↔xiigen-flows/FLOW-31; FLOW-31=design-анализ, html+css генерят клиенты; core-seam vs client-host e2e разведены), Ф10.1.2 (FigmaToCode/CanvaToCode + провенанс-нота закрытого teacher); **статус: partial до ответа Luba на Q-host (синхронно с матрицей §3.5)** |
| R17 | API better-output→learning-DPO на xiigen-platform + xiigen-flows | Ф0.1.3 (Q37 `better_output_origin_policy_locked`), Ф2.1.1/Ф2.1.2 (SourceCategory client/third_party + ветка C16), Ф3.1.2 (`chosen_origin=client_supplied_unverified`), Ф10.2.1 (FLOW-38), Ф10.2.2 (Smart*); **статус: partial до ответа Luba на Q37+Q38 (синхронно с матрицей §3.5)** |
| R18 | Префикс codex для моих файлов | Имя `codex-EXECUTION-CONTROL-BOARD.md` + все блоки `codex-*` (мета) |
| R19 (NEW) | Строгая последовательность + полная execution-видимость + нумерация Фаза.ПодФаза.ПодШаг | §0.1 нумерация, §0.3 sequential gating, §A exit-gates, §B EXECUTION-VISIBILITY |
| R20 (NEW) | До 50 проходов review/fix пока план корректно выстроен и покрывает требования | §D Review-Fix протокол + текущий comparison ledger `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md`: 50/50 returned packet records; C49-C50 returned and integrated; old 50/50 ledgers are evidence-only for this current run unless reconciled. Ф11.2 (10 execution reviews при реализации) остаётся отдельным gate. **covered/current-run complete** |
| R21 (NEW) | Во всех участвующих репозиториях source authority перед реализацией = самый последний updated branch; новая рабочая ветка = `DPOLegalLearning` | §0.3.1 branch authority pre-flight: repo-by-repo table `repo_path_or_url`, `branch_list_source`, `selected_authority_branch`, `selected_branch_updated_at`, `divergence_note`, `future_working_branch_name=DPOLegalLearning`; candidate repo identifiers: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`; plan-only, без checkout/create сейчас |

---

# §D. Протокол review/fix доски (R20: до 50 проходов)

> R20 требует прогнать review/fix, пока доска полностью корректно выстроена и
> покрывает все требования. Это протокол ревью самой ДОСКИ (план-артефакта), не
> реализации. При исполнении плана действует отдельный 10-review gate Ф11.2.

## D.1. Что проверяет каждый проход ревью доски

```text
REVIEW-FIX CHECK (для доски исполнения):
1. NUMBERING:  каждый ID Ф<P>.<SP>.<SS> уникален; нет дублей; нет дыр
               (подфазы 1..N подряд; подшаги 1..M подряд). Σ совпадает со счётчиками §1.
2. SEQUENTIAL: каждая фаза имеет exit-gate; правило строгой последовательности §0.3
               явно; «Зависит от» у каждого подшага не ссылается вперёд по фазам;
               ЕДИНСТВЕННАЯ семантика порядка (индекс Ф<P>.<SP>.<SS> не противоречит DAG
               «Зависит от», §0.3); число дорожек определено = 1 глобальная (§0.5).
3. GATE_B:     каждый подшаг имеет измеримый Gate B (сравнение/счёт/assertion),
               не «работает корректно».
4. COVERAGE:   каждое R1-R21 покрыто ≥1 фазой/подшагом (§C); нет потерянного блока
               P-01..P-14 (каждый блок-источник присутствует в колонке «Блок-источник»).
5. STATUS:     все статусы на момент плана = remaining (план-only); легенда статусов
               полная (5 значений).
6. VISIBILITY: §B отвечает «какая фаза/подфаза/подшаг сейчас, сколько done/total,
               что остаётся» без машинного лога.
7. DAG:        порядок фаз обоснован; нет циклов; зависимости блоков (§C.5 мастер-плана)
               отражены в «Зависит от».
```

## D.2. Ledger проходов ревью (заполняется при ревью; каждая строка — реальный пакет)

| Проход | Reviewer (id, model/effort) | Что проверено | Найдено | Фикс | Recheck |
|---|---|---|---|---|---|
| Old sweep rows 1..50 | old ledger `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` | Historical/evidence-only for the current Claude-comparison run | Prior table text included 50/50 completion wording that is not current-run proof | Quarantined for current-run arithmetic; do not use as current R20 completion evidence unless exact packet records are reattached/reconciled | evidence-only |
| Current run C01..C30 | `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md` | R20 honesty, R21 branch authority, board B.6, matrix/master sync, legal/architecture gaps | C01-C30 returned packet records before C31 batch integration | status/authority/evidence-first patch applied to master, board, matrix, ledger | historical interim state; superseded by C49-C50 final count |
| Current run C31 | `019ef79a-0441-7290-85db-fec59f405aac` / Kuhn | R20/R21 consistency after C01-C30 correction | pre-C31 finding: old-sweep rows had stale completion wording | this §D.2 quarantine row replaces stale old-sweep rows | readback passed |
| Current run C32 | `019ef79a-455c-7a52-bc10-887673b055d7` / Peirce | B.5/B.6/F11.2.1 gating | F11.2.1 did not point back to §B.6 | F11.2.1 Gate B and source now reference §B.6 | pending readback |
| Current run C33 | `019ef79a-7f26-7483-91b4-257f4aab63a8` / Aquinas | final questions and R-number crosswalk | matrix final queue drift + missing promised crosswalk | matrix final queue and crosswalk patched | pending readback |
| Current run C34 | `019ef79a-ba10-7e30-9fb3-39fb4660a811` / Goodall | plan-14 source/reference ownership | pre-C34 finding: plan-14 not yet canonical owner | plan-14 canonical ownership patched; C40 readback passed (`codex-plan-14-stage-references.md` §Canonical Ownership / branch ref) | readback passed |
| Current run C35 | `019ef79a-f696-7751-b580-791ddf7a0352` / Turing | legal/DPO canon drift | provenance fields, GraphDpoPair owner, verdict vocabulary, MeasurementReport axes drift | legal/DPO projection patches pending/applied in plan files | pending readback |
| Current run C36 | `019ef79b-32e8-79e0-b819-cd9c59df3b93` / Godel | plan-12 architecture depth | plan-12 lacked 9-mechanism inventory and cutover guard | plan-12 depth patch pending/applied | pending readback |
| Current run C37 | `019ef7a4-641d-77c0-9362-276c7abe43a9` / Anscombe | post-C31-C36 consistency | active R20 question/status mismatch remained | master/matrix status and final-question handling patched | readback passed |
| Current run C38 | `019ef7a4-a666-7460-9486-786a3dd41ed3` / McClintock | legal/DPO provenance and verdict namespace | unqualified verdict wording could collide across gates | namespaced verdict fields patched in plan/guide surfaces | readback passed |
| Current run C39 | `019ef7a4-e5ad-7c91-9247-ef6d121ff77f` / Confucius | plan-12 measurable Gate B | no-hidden-determinism proof rows needed measurable predicates | plan-12 proof predicates patched, later tightened by C45 | readback passed |
| Current run C40 | `019ef7a5-2203-7d73-8eaf-12945f44fd81` / Hypatia | plan-14 ownership readback | no new defect after ownership patch | no edit required | readback passed |
| Current run C41 | `019ef7a5-62cd-7253-a09e-c0003d673a85` / Euler | crosswalk/final-question sync | wording parity defect remained | matrix/master final-question wording patched | readback passed |
| Current run C42 | `019ef7a5-9b11-77b0-b5cb-7ddb0641d436` / Sartre | branch-authority amendment coverage | no branch metadata was inspected; plan-only wording needed to stay explicit | branch authority kept plan-only with repo-by-repo future evidence table | readback passed |
| Current run C43 | `019ef7ad-1fb8-7340-8909-6e94b01fd039` / Gibbs | post-C37-C42 consistency | no new actionable defect | no edit required | readback passed |
| Current run C44 | `019ef7ad-591a-7550-97f0-4a9f1ebdc000` / Hilbert | GraphDpoPair schema owner clarity | schema ownership vs execution-owner assignment could be confused | plan-09 and board clarify schema owner separately from execution owner | readback passed |
| Current run C45 | `019ef7ad-9c9b-7f90-b3ac-c43c66f5cca7` / Dirac | plan-12 measurable proof predicates | three proof rows were not directly assertable | plan-12 predicates patched to boolean/reference checks | readback passed |
| Current run C46 | `019ef7ad-e8c0-79f1-a228-76e710053615` / Banach | final-question wording parity | matrix Q wording used weaker phrasing | matrix wording patched to match master intent | readback passed |
| Current run C47 | `019ef7ae-2af7-7283-ade6-99485e1a37aa` / Parfit | branch-authority and `DPOLegalLearning` wording | no new actionable defect | no edit required | readback passed |
| Current run C48 | `019ef7ae-6f17-7231-9c4b-71153bb64441` / Pauli | stale support-plan status counts | support plans still had stale active count lines | support-plan status lines patched to honest interim wording, then superseded by C49-C50 final count integration | readback passed |
| Current run C49 | `019ef7b8-bfcf-7d13-bd4c-1251dea926af` / Pasteur | final status/count/honesty sweep | no active stale completion claim; old hits are historical/evidence-only or defect/search text | no edit required beyond final count integration | readback passed |
| Current run C50 | `019ef7b8-d414-7082-a1ac-58c863ed81e7` / Bacon | final branch-authority/no-Claude/no-implementation audit | no contradiction found | no edit required beyond final count integration | readback passed |

> Правило R20: проходы считаются только как реальные пакеты ревью (reviewer id,
> artifact before/after, findings или no-fix justification, recheck). N строк ledger
> после одного ревью — провал (CLAUDE.md «Cycle-as-action rule»). **На текущий
> честный момент comparison-run имеет 50/50 returned packet records**; циклы
> 01-50 complete for the current comparison run.

> **Честный учёт (синхронно с мастером §F.6 и матрицей R20):** для текущего
> comparison-run зачтено 50/50 returned packet records в
> `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md`; pending = NONE.
> Их **нельзя путать** со 100 строками (50+50)
> в `codex-50-cycle-plan-review-supplement.md` (Claude-authored, EVIDENCE-ONLY) —
> это `ledger_row_count`, НЕ циклы (supplement сам фиксирует
> `subagent_cycles_completed=0`, `independent_review_fix_cycles_proven=0`;
> CLAUDE.md «Cycle-as-action rule»). R20 = `covered/current-run complete`; при
> реализации продукта действует отдельный 10-review gate Ф11.2.1.

---

*Конец доски исполнения ILearningDynamicGraph. PLAN-ONLY: документация-решение,
перестраивающая план в строго последовательную исполнительную доску. Нумерация
Ф<P>.<SP>.<SS> (12 фаз / 26 подфаз / 59 подшагов), exit-gate на каждую фазу, статусы
execution-control, EXECUTION-VISIBILITY, сводные счётчики, DAG зависимостей.
Не разрешает реализацию продукта/кода, тренировку, билд, запуск раннеров, git или
вызов внешних моделей. Реализация требует свежего явного approve Luba. Чужие
`claude-*`/`dcm-*` артефакты — evidence-only.*
# CURRENT-RUN HONEST BOARD STATUS

This block replaces stale W2c/old-50 board overlays for the current Claude-comparison run.
Those overlays are evidence-only until exact returned packets are reconciled. Current board,
matrix, and master status is the counted 50/50 current-run state.

```text
verified_subagent_packets_completed_current_run = 50/50
cycles_01_50_complete_current_run = true
baseline_phases = 12
baseline_subphases = 26
baseline_substeps = 59
phase4_3_numbering = Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split  (clean 1..3, no .0)
reserve_rows = Ф10.3.1..Ф10.3.6 (по 1 на R15-домен; вне baseline-59)
reserve_rows_in_baseline_count = false
reserve_rows_status = blocked_scope_gated
coverage_summary_current = covered 15 / partial 6 / unmapped 0
covered_set_current = R2, R3, R4, R5, R6, R7, R8, R10, R11, R12, R13, R18, R19, R20, R21
partial_set_current = R1, R9, R14, R15, R16, R17
current_execution_pointer = NONE_PLAN_ONLY
ordered_phase_queue = [Ф0, Ф1, Ф2, Ф3, Ф4, Ф5, Ф6, Ф7, Ф8, Ф9, Ф10, Ф11]
implementation_authorized = false
false_50_cycle_claims_allowed = false
```
