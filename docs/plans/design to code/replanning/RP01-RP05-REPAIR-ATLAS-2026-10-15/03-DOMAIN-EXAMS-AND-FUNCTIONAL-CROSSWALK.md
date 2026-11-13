# 03 · Предметные экзамены вместо покрытия недетерминированного поведения

**DESIGN/ILLUSTRATIONS ONLY, 15.10.2026.** Ниже нет исполненных опытов, Chinese-authored training material или закрытых экзаменационных примеров. Все примеры публичны: они показывают тип различия и **не должны копироваться в EXAM_SEALED**. Финальные учебные chosen/rejected создаёт допущенная внешняя открытая китайская модель после CODEX выбора case; отдельный CODEX astra 6 ultracode оценивает exact pair.

Основание: [RP02 V7.2 runbook](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7.2-LEARN-FIRST-RUNBOOK.md), [RP03 V6.3](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/RP03-V6-LEAN-START.md), [RP05 V4](../Kiro-Xiigen-RP05-Continual-Learning-PC-Feedback-Slice-Pack/RP05-V4-CODE-PATH-AND-SCIENCE-REPAIR-2026-10-14.md), [WorkLeft RESEARCH-03](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), [52-requirement crosswalk](../R02-52-REQUIREMENTS-32-SLICE-CROSSWALK-2026-10-14.md). Протоколы ниже — предлагаемая конкретизация для проверки карточек; они не заменяют согласованный профиль действующего экспериментального владельца и не добавляют обязательную quota.

## 1. Для каждого алгоритма — собственный эффект и опровержение

| Механизм / владелец | Что фиксировать и что менять | Что наблюдать у пользователя | Что НЕ закрывает задачу |
|---|---|---|---|
| Domain mapping / RP02 01A | Solver, корпус и budget fixed; predicted mapping vs source-qualified oracle mapping | Верный subject/intent/direction/stack, MIXED/UNKNOWN, последствия ошибочного маршрута | Верный domain label при неверном решении |
| Semantic representation / RP02 01B/01D, RP18 | Тот же legal corpus и task; один encoder/feature change, совместимый index | Сохранены отрицание, числа, типы, роли, referent, порядок; downstream law correctness | Одинаковая dimension, красивый embedding plot, loss без consumer |
| Law-conditioned ranker / RP02 02B1 | Same-q legal candidate pool, φ/decoder/corpus fixed; θ0→θ1 | Другой допустимый candidate/path или осмысленное abstention, качество на новой семье | Только hits[0] заменён votes; поменялись constants чужого ReplayFusionRanker |
| Adaptive k / RP02 02B2 | Representation, scorer и eligible pool fixed; fixed-k vs contextual/learned-k policy | Law quality, abstention, useful neighbours, time/RAM при разной density/uncertainty | k всегда 8; heuristic if назван learned θk |
| Exact vs ANN / RP02 02C | Same vector space, legal corpus, scoring and task | Recall against exact reference плюс end-task quality и полный retrieval cost | Высокий recall без правильного ответа; incompatible metric без review |
| Student response-DPO / RP01 | Same C0, reference, kNN, data/decoder; только допустимые Student LoRA parameters | Independent semantic answer quality, retention, cold served checkpoint | Judge score вместо objective; smoke/profile/веса B0 названы learning |
| Unit typed many→many / RP03 01A–C | Source/input identity и schemas | Несколько связанных допустимых TOutput и реальное чтение следующим Unit/Host | Один output размножен N раз, associations потеряны |
| Flow correction / RP03 02B–D | Unit/kNN/graph fixed; реальный lawful preference изменяет W | Корректная association/port/path на новой композиции, cold W used | Делегирование call или Student LoRA названо Flow update |
| E3 route/order/stop / RP04 01B | Legal async pool, Unit/Flow/kNN/E4 fixed; policy parameter changes | Лучший из реально оценённых допустимых путей при сравнимом бюджете | Невыполненная ветвь автоматически rejected; скрытые расходы fan-out |
| E4 higher-order patterns / RP04 02B | Order/type/law preserved; relation policy state changes | Перенос на невиденные order-2/3 compositions, не только знакомые edges | JSON graph/hash изменился; TrainBatch still NOT_ACTIVATED |
| Feedback/continual / RP05 | Accepted parent, новый novel signal/lesson, frozen exam policy | B1/B2 новые навыки без недопустимой потери старых, accepted cold state | Новый ID старой пары; private state отправлен наружу |
| Hybrid / RP08/RP11/RP13 | Один фактор, затем ограниченное взаимодействие реально работающих факторов | Law-correct end task и quality/cost/retention tradeoff | Все параметры/данные изменены одновременно, «гибрид компактнее» по размеру LoRA |

**Обязательная граница:** обучаемое вычисление может исполняться детерминированно. Случайность сама по себе не является обучением. Память kNN может обеспечивать непараметрическую адаптацию без gradient; это иной механизм, чем trainable θ и иной causal contrast. [Исходная kNN-LM работа](https://arxiv.org/abs/1911.00172) полезна для этого различия, но не доказывает эффективность XIIGEN.

## 2. Domain-law passport: минимально нужная структура

Для каждого экзаменируемого правила записать:

```text
rule_id; domain subject/intent; language/direction; stack/dialect/version
primary_source_path/blob/span; source_family; permitted_use/tenant
rule_kind = HARD_ESTABLISHED | SOFT_CONTEXTUAL | UNKNOWN
applicability_preconditions; counterexample_to_overbroad_rule
input/output types; actual user consumer; semantic features + units
unknown/missing masks; encoder/codebook/index version
positive behaviour; plausible near-miss; ambiguity/OOD; warranted abstention
oracle/evaluator; rubric frozen_before_T; measured endpoint; falsifier
```

Это не требование реализовать новый универсальный DTO. Сначала достаточно evidence packet с уже существующими compatible fields. Новые публичные API добавляются только по доказанному type/consumer gap. Первому RP02 θ pilot нужны минимальные реально вычислимые доменные отличия, а не полный универсальный token standard.

### C# — синтаксическая роль и nullable смысл

**Публичный illustrative input:** объяснить `s?.Length ?? 0` при `string? s`, явно задав nullability/версию языка.

**GOOD:** различить conditional member access `?.` и null-coalescing `??`; объяснить, почему внешнее `?? 0` даёт fallback при null и почему результат `s?.Length` отдельно имеет nullable смысл. Формулировка может быть другой, если сохраняет значение.

**Plausible BAD:** «оператор ?. сам возвращает ноль, когда строка null». Это предметная ошибка, а не плохой стиль. Нельзя учить лишь находить символ `?` или предпочитать более длинный ответ.

**EDGE:** non-null пустая строка; non-null непустая строка; неверно заданный тип receiver; dynamic; недостающая версия/контекст. Не распространять закон на непроверенную overload/receiver semantics.

**Vector/kNN features:** AST operator role/order, receiver/result type, nullability assumptions, semantic claim IDs, source version, unknown masks; lexical similarity отдельно от закона. Слова/скобки без структуры недостаточны для syntax claim.

**Exam:** независимые project/source families со сменой имён, структуры выражения, control-flow и контекста; не косметические клоны одного snippet. На каждой семье оценить critical semantic claims, unjustified certainty, valid path и resource cost. Compiler/реальное выполнение разрешённого snippet может быть independent law oracle; это не внутренний coverage target. Не запускать неизвестный код с внешними эффектами в unrestricted среде.

**Falsifier:** learned ranker чаще выбирает fluent неверное объяснение; T выиграл только на training snippet; поменялась feature extraction одновременно с θ и не выделен C0. Ответ: REGRESS/INVALID_EVIDENCE, а не правка ожидаемого закона.

### EN→RU / RU — референт, роль слова и отрицание

**Публичный пример:** «Книга лежала на столе; он был тяжёлым». Мужской pronoun может относиться к «стол», а не к «книга». Наличие женского существительного не запрещает все последующие мужские формы. В неоднозначном контексте нужно сохранить альтернативные referent hypotheses, а не выдумывать единственный объект.

**GOOD:** agreement проверяется относительно того же объекта и синтаксической роли; отрицание/условие перевода `Do not send until approved` сохраняются. `No more than five` не превращается в `не менее пяти`.

**Plausible BAD:** грамматически гладкая фраза с ошибочным antecedent; affirmative вместо prohibition; мужская форма отклонена только из-за ранее встреченного женского noun. Такие false hard veto измеряются отдельно от missed violations.

**EDGE:** несколько подходящих referents; quoted speech; смешанные языки; неизвестный gender; ё/е и Unicode normalization; изменение направления перевода. Для ambiguous family несколько верных интерпретаций или clarification допустимы по заранее определённой rubric.

**Vector/kNN features:** candidate POS posterior, dependency role, referent identity/probability, gender/number/case когда применимо, polarity/negation scope, direction, confidence/missing masks. Не записывать ground-truth exam labels как runtime features. Joint role/referent compatibility важнее одного cosine score.

**Exam:** семьи с тем же lexical content и разными syntactic/referent relations, плюс paraphrases с сохранённым смыслом; predicted-domain и oracle-domain arms отдельно. Метрики: semantic preservation, agreement conditioned on correct referent, negation/quantifier errors, false-hard-veto, warranted vs excessive abstention, end-to-end time. RP02 recipient не автоматически Student RP01.

**Falsifier:** выигрыш зависит от угадывания пола по surface word; T не сохраняет отрицание; RU representation нормализована так, что кириллица пропала. Regex `[^a-z]+` из прежних leads нужно подтвердить в реальном reached caller, не объявлять актуальным дефектом только по manifest.

### SQL — закон зависит от задачи, схемы и диалекта

**Публичное illustrative задание:** вернуть каждого клиента и его открытые заказы, сохранив клиентов без таких заказов. Dialect/schema зафиксировать перед экспериментом.

```sql
-- Candidate A: предикат открытого заказа ограничивает matching правой стороны.
SELECT c.id, o.id AS order_id
FROM customers AS c
LEFT JOIN orders AS o
  ON o.customer_id = c.id AND o.status = 'open';

-- Candidate B: дополнительный WHERE может удалить unmatched строки.
SELECT c.id, o.id AS order_id
FROM customers AS c
LEFT JOIN orders AS o ON o.customer_id = c.id
WHERE o.status = 'open';
```

**GOOD:** A соответствует поставленной цели сохранения клиентов. **Plausible BAD:** B объявлен эквивалентным A для всех данных. При семье с unmatched клиентом это различие становится наблюдаемым. Здесь описано ожидаемое по семантике поведение, **не выполненный SQL run**.

**EDGE:** пустая orders; только closed orders; NULL status; несколько matching orders; duplicates; неизвестный dialect; tenant permissions. Отдельный family про NOT IN/NULL и NOT EXISTS не считать тем же самым правилом.

**Vector/kNN features:** relation/column role, join type, ON/WHERE predicate location, NULL/three-valued logic, cardinality goal, dialect/schema/version, rights scope. High similarity не делает запрос правильным или разрешённым.

**Exam:** независимые schemas и data-generation families, source-qualified result sets, не raw output snapshots от собственной новой реализации. SQL execution для oracle — sandbox/read-only synthetic data и разрешённый budget. Ground-truth cardinality и preserved left IDs оценивать по цели. Не исполнять запрещённый query ради получения «плохого примера».

**Falsifier:** T выбирает B по более короткому text; law-aware kNN допускает чужой tenant; рост средней accuracy скрывает потерю unmatched cases. Ответ: critical-law failure/REGRESS, не усреднение с лёгкими C# ответами.

## 3. DPO: четыре разных проверки вместо одного «валидный JSON»

### 3.1. Происхождение и exact pair

Один immutable record связывает case/context/rights, фактический Chinese author request и обе exact final sides, source-family TRAIN, actual separate CODEX judge и judged digest. CODEX astra 6 ultracode выполняет selection и assessment раздельно. Feedback user edit/model response может быть разрешённым observation context, но не переатрибутируется Chinese output. Source material, final-side authorship и judgement — разные поля.

**Не форсировать preference:** TIE/BOTH_BAD/UNJUDGEABLE/no usable contrast не закрываются случайным выбором. Содержательная ошибка rejected должна быть правдоподобной; слово WRONG, бессмыслица, длина или стиль не должны стать shortcut. Два правильных решения допускают preference по заранее объявленному критерию стоимости/полноты, а не по вкусу после ответа.

### 3.2. Материал доходит до того же recipient без потери смысла

Проверить exact prompt/chosen/rejected и response-only mask после normalization/segmentation/projection, recipient identity и неизменность judged bytes. Если material изменился после judgement, нужна новая оценка этого material. Различать `artifact_integrity=SEALED_IMMUTABLE` и `data_partition=TRAIN`; EXAM_SEALED не используется в optimizer.

### 3.3. Objective соответствует заявленному обучению

Для стандартного response DPO принципиален policy/reference contrast, а не scalar judge score:

```text
m = [log πθ(y+|x) − log πref(y+|x)]
  − [log πθ(y−|x) − log πref(y−|x)]
L_DPO = −log sigmoid(β m)
```

Это математический reference из [DPO](https://arxiv.org/abs/2305.18290), **не заявление о текущем точном коде XIIGEN**. Реальные masks, sequence aggregation, β, reference lifecycle и trainable parameter keys читать в reached implementation. Judge отвечает за качество пары, optimizer — за свою objective. Нулевой начальный margin не равен плохой паре.

RP02 candidate θ и RP03 Flow W могут использовать отдельный bounded pairwise rank loss; не объявлять эту классификацию Student DPO. [LoRA](https://arxiv.org/abs/2106.09685) описывает low-rank weight adaptation; семантическое соответствие выходного token/vector требует отдельного encoder/projector/decoder и собственного опыта, а не следует из названия LoRA.

### 3.4. Польза после обучения

Валидная pair может не улучшить Student. После actual update нужны независимые C0/T outputs, retention, full-PC и cold adopted state. При NO_GAIN pair quality и provenance не переписываются задним числом. При REGRESS принятый parent сохраняется/восстанавливается по целостному lifecycle.

**Три отрицательных сущности нельзя смешивать:** (1) семантически ошибочная, но законно полученная Student-DPO rejected side; (2) lawful inferior same-query candidate для ранжирования среди допустимых вариантов; (3) forbidden tenant/type/action или warning memory, запрещённый для positive serving. У каждой свой путь хранения, использования и veto.

## 4. Контролируемое соединение детерминированного и обучаемого

Базовая цепочка для проверки, не требование нового API:

```text
request + rights + requested output type
 → domain/intent/direction + uncertainty
 → typed, versioned semantic representation
 → lawful recall pool + known-law checks
 → learned candidate / Flow / route preference under fixed context
 → bounded execution and final arbiter
 → actual typed answer / qualified abstention
 → consented weakness → approved exact lesson → once-only state update
 → independent examination → accepted cold ordinary consumer
```

**Hard constraints сохраняются во всех arms.** D=deterministic lawful baseline; L=learned decision with обязательными safety laws; H=hybrid. «L без tenant/privacy/arbiter guards» не является допустимым control ради красивого выигрыша H. Сравнить равный budget, candidate access и actual available paths.

Сначала один фактор: RP02 θ при fixed representation/k; Student LoRA при fixed kNN; Flow W при fixed Unit. Когда оба компонента реализованы, возможен ограниченный 2×2: Student update off/on × law-conditioned kNN adaptation off/on. Проверить, что четыре arms реально executable, права и законные ограничения одинаковы, корпус/decoder/candidates совместимы. Разность разностей характеризует взаимодействие в этом конкретном контексте; она не доказывает глобальную оптимальность гибрида.

«Лучший путь» означает лучший **среди реально наблюдавшихся или source-oracle обоснованных допустимых альтернатив при данном бюджете**. Неисполненные branch outcomes — UNKNOWN, не отрицательные labels. Для внешних side effects контрфактическое выполнение может быть недопустимо: тогда claim scope ограничивается safe simulator/read-only trace или наблюдаемыми альтернативами.

Для Unit→Flow→Orchestrator→Graph проверить каждый переход: OutputRef/association, law/schema/version, state owner и actual reader. Изменение одного owner не приписывать всем уровням. Успешная LoRA генерация не доказывает learned graph. Наличие compile-ready graph contracts не доказывает активные Train/Import/Export.

## 5. Минимальный наблюдаемый lesson receipt и bulk protocol

```yaml
experiment_id: actual_unique_id
purpose: one_causal_question
recipient: actual_method_and_state_owner
source: primary_path_blob_span_and_law_version
partition: TRAIN_or_OP_DEV_or_EXAM_SEALED
integrity: immutable_material_digest
actors: selection_receipt_author_receipt_exact_pair_judge_receipt_or_NOT_INVOKED
parent: code_state_encoder_index_candidate_pool_digests
control: same_corrected_code_with_no_learning_update
treatment: same_control_except_one_named_learned_state
update: actual_parameter_delta_once_journal_or_NOT_RUN
cold: actual_new_process_reader_selected_digest_output_or_NOT_RUN
exam: independent_family_raw_outputs_and_rubric_or_NOT_MEASURED
resources: whole_PC_and_all_external_failed_work_or_NOT_MEASURED
verdict: PILOT_ONLY_or_IMPROVE_or_NO_GAIN_or_REGRESS_or_INCONCLUSIVE_or_INVALID_EVIDENCE
review: actual_independent_invocation_or_NOT_PERFORMED
```

Поля являются схемой evidence, не объявлением существующего класса. До эффекта отсутствующие actual hashes отмечаются NOT_RUN, не заполняются вымышленными значениями.

**Статистическая единица — независимая семья источника/задачи.** Для семей f считать paired difference `score(T,f)−score(C0,f)`, отдельно `score(C0,f)−score(P,f)`. Средний family gain сопровождать raw wins/ties/losses, размером выборки и заранее выбранной оценкой неопределённости, например family-level bootstrap при достаточной выборке. Для очень маленькой панели честнее exploratory counts, чем мнимая точность. Не считать несколько outputs одной семьи независимыми.

**Новые bulks:** parent и treatment отвечают на одинаковую свежую панель; одновременно проверяются старые source families для retention. Отдельно worst-domain, false-hard-veto, critical-law errors, OOD/abstention и overconfidence. Уже использованный для подбора гиперпараметров/уроков holdout превращается в development; confirmatory set требует отдельной процедуры ограниченного доступа. Не раскрывать его автору учебных пар.

**95% ultimate quality target** сохраняется как owner requirement с pinned reference/rubric и необходимой неопределённостью. Он не становится условием запуска первого урока. Pilot-P12/другой prospective panel из R22 — предложение, не универсальный обязательный объём и не выполненная аттестация.

**Full-PC accounting:** разовые и amortized costs отдельно; wall/CPU, warm/cold p50/p95 с оговоркой малого N, peak RAM/VRAM, base/adapter/optimizer/checkpoint/index/graph/cache/rollback bytes, rebuild/reindex/load, teacher/judge tokens/time/failed calls, все started/losing/cancelled async branches. Не выдавать работу малого θ scorer на CPU за доказанную возможность всего Student training на PC.

## 6. Что проверять программно и чего не превращать в test coverage

| Изменение | Минимальный достаточный engineering evidence | Scientific evidence |
|---|---|---|
| Источники, prompt, схема экзамена, review, REUSE_NO_CHANGE | 0 новых product tests; content/source/dispatch check | Честная гипотеза, owner, falsifier; runtime NOT_RUN |
| Новый/изменённый type, tenant, provenance, split boundary | Ближайший существующий allow/reject check; добавить только если изменённый риск иначе не проверяется | Lawful corpus/output, false veto, ordinary consumer |
| Изменённые masks/objective/finite parameter update | Адресная проверка фактического знака/маски/finite и разрешённых parameter keys | Реальный C0/T gain/retention, не только loss |
| Changed journal/checkpoint/accepted pointer | Применение once, corrupt/incompatible refusal и полный recover/cold на затронутом seam | Тот же checkpoint реально служит обычному запросу |
| Async cancel/side effects/budget | Узкая проверка достигнутой новой границы | Качество и стоимость всех фактически запущенных ветвей |
| Stochastic/model outputs, adaptive-k/POS/semantic correctness | Нет строки expected-output на каждое возможное поведение, нет % coverage | Различающие независимые domain families и заранее frozen scoring |
| Реальная Windows/native/host поставка | Необходимые применимые release/security проверки на этой стадии | Не являются доказательством learning gain |

Минимум означает достаточность для **изменённого риска**, а не произвольный запрет на нужную проверку и не ровно 1–3 tests на любую задачу. Не удалять существующие safety guards ради красивого нулевого счётчика. Не запускать широкие Windows workflows ради source/prompt-only ревизии.

## 7. Функциональная сохранность за пределами первого C# урока

Это краткий маршрут переноса **всех групп исходных R02 IDs**. Для точной формулировки каждого ID остаётся первичным [существующий crosswalk](../R02-52-REQUIREMENTS-32-SLICE-CROSSWALK-2026-10-14.md) и results/R02/CHANGE-MAP.csv; ниже не подтверждается их реализация.

| R02 IDs | Что нельзя потерять | Owner / триггер расширения | Нужное наблюдение |
|---|---|---|---|
| 001–004 | Unit many→many, Flow correction, async Orch, higher-order Graph | RP03/RP04; собственный первый recipient/feedback | Associated typed output, actual policy change, unseen composition |
| 005–010 | Hybrid, multi-axis domain, semantic vectors, law kNN, LoRA projection, causal credit | RP02/RP18/RP11/RP13; адресные arms по мере готовности | Same-budget law utility, correct semantic recipient, factor isolation |
| 011–017 | CODEX выбор, Chinese BOTH sides, отдельный judge, meaningful preference, Builder utility, rights, no replay | RP01/RP07/RP09; один допущенный новый lesson | Exact bytes/roles/objective, новая независимая utility |
| 018–024 | Полный rollback/cold, repo artifacts, 95%-goal, comparable bulks, whole-PC, feedback | RP05/RP10, RP09, соответствующий S owner | Accepted state/новый процесс, retention, приватность и cost |
| 025–027 | Все исходные домены, восемь task types, дополнительно Hebrew→English | RP02/RP06/RP07/RP09 и S21; по готовности конкретного adapter | Отдельный corpus/task/direction и обычный consumer; C# не закрывает остальные |
| 028 | Hard law vs uncertain premise/user preference | RP02 и domain owners | Rule passport, known applicability, false veto/OOD |
| 029–038 | DTC input tree/assets/images; API/machine; multiple viewports; Free/Paid/Pro budgets; improvement loop; DB/lesson destination; видимая utility; accepted publish; все названные consumers | RP06 и реальные host owners; после доступности их lawful input/adapter | End-to-end Figma/Canva→responsive useful output→teaching/adoption, не один screenshot |
| 039–043 | IDE repo index, plan/execution, local-first/offline, private/common state, cross-device restore | RP06/RP05/S23; собственный integration trigger | Cold installed/local user path, source privacy, state/encoder compatibility |
| 044–049 | Полнота исходной задачи, bounded prompts, human clarity, reuse, fast lesson, concurrent ownership | Все owners; применяется при выдаче/ремонте выбранной задачи | Source→requirement→task→observable result, no stale dispatch/overwrite |
| 050–052 | Правильный Machine/Unit training target, causal effect, retention/OOD honesty | RP03/RP04/RP07/RP09/RP11/RP05 | Один фактический state owner, same-arm contrast, worst-domain retention |

Для DTC сохранить named external consumers из исходной задачи: xiigen-platform-dot-net-angular, xiigen-flows-dot-net-angular, FigmaToCode-design-gen и CanvaToCode. Для каждого нужны реальный API/host owner и собственное доказательство доставки; один core endpoint не закрывает четыре продукта. В данной ревизии эти репозитории и их runtime не проверялись.

**Acceptance этого документа:** у каждого выбранного requirement есть адресный consumer/exam или честный missing-owner/gap; ни одно требование не исчезло ради ускорения; ранний pilot не ждёт всей таблицы. **Не acceptance продукта:** наличие этой матрицы не означает trained Student, active graph, полный multi-domain охват или независимую Astra-приёмку.
