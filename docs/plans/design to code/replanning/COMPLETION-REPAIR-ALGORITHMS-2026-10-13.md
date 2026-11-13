# Содержательное исправление: гибридные алгоритмы, DPO и получатель обучения

**13.10.2026. Тип результата: адресное чтение первоисточников и кода + выбранные проектные решения. Это НЕ исполнение R01–R25, не реализация и не измеренное обучение.**

Этот документ дополняет `R01-R12-COMPLETION-AUDIT-2026-10-13.md` результатами, а не ещё одним описанием того, что надо исследовать. Собственные `results/Rxx/*` параллельных исполнителей не переписываются. Их незавершённые статусы не повышаются этим редакционным дополнением. Проектируемый элемент явно отличается от обнаруженного кода.

## 1. Результат человеческим языком

Существующий ученик должен получить содержательно правильный новый урок, изменить именно свои параметры и затем ответить обычным способом на другие задачи. Для этого не нужен ещё один DPO trainer. Нужны сохранение смысла пары, правильно определённая целевая функция и доказанная связь принятого состояния с обычным ответом.

Одновременно большая архитектура не сводится к этому первому уроку. Доменная память помогает найти сведения; Unit решает типизированную подзадачу; Flow согласует результаты разных Unit; Orchestrator выбирает допустимое асинхронное выполнение; DynamicGraph учится переносить сочетания паттернов. У каждого уровня должен быть собственный предметный эффект, но не обязательно отдельный trainer.

**Выбранное разделение:** детерминированные механизмы задают допустимость и сохраняют смысл; обучаемые механизмы выбирают, сопоставляют и корректируют внутри допустимого пространства. Арбитр сохраняет право отказать даже очень близкому соседу или дешёвому пути. Нельзя объявлять доказанным обучение всех уровней по улучшению одного документационного solver.

## 2. Зафиксированная база и прямое чтение

P0 = `lubochka/xiigen-planning@ce04fcbf0f3f03d04787a801bbc78dc3fc26ba4e`.
K0 = `lubochka/llm_mvp_core@3c7f5f8c7809936e25579603c96ea6df8598fc40`.
P1 = `lubochka/xiigen-planning@8d85a411d1350e933a653c0e80da8d06b051e739`: наблюдаемая параллельная правка R16, не работа этого документа.
D = `llm mvp/design to code session`; F = `llm mvp/llm dynamic flow`; V = `llm mvp/visual studio & visual studio code continue`.

| ID | Прямой источник / фактически прочитанный участок | Что он устанавливает |
|---|---|---|
| A01 | P0 `D/original task.md`, 1–145; `D/Алгоритм создания ДПО.md`, начало и исходные роли | many→many, четыре роли, асинхронные комбинации и собственный Builder; исторические команды/модельные shortlist не активированы |
| A02 | P0 `D/09.09.26WorkLeft-MECH-01-mechanism-validation.md`, активный раздел до исторического тела | существующие механизмы хранения, обучения, оценки и исторически незакрытые граф/ordinary-use; не текущий runtime результат |
| A03 | P0 `D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, 1–105 | lifecycle, различие pair validity/utility, one-pair/no-repeat, роли и полный снимок |
| A04 | P0 `V/RAG-06-llm-dynamic-flow.md`, 1–100 → `F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`, 1–145 → `F/DECISIONS-LOCKED.md`, весь возвращённый файл | содержательный RAG-переход к двум первичным решениям; positive/negative/warning memory; authority; D-04…D-15 |
| A05 | P1 `D/09.09.26WorkLeft-02-teaching-chinese-model.md`, весь текущий файл в 1–105 | первый S04 Student — существующий CodeDocumentation, не Qwen/таблица маршрутов; local S04 отдельно от remote S05; R16 уже перенёс роли и критерии |
| C01 | K0 `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, 1510–1675, 1810–1945, 2080–EOF | подготовка/снимки, parent identity, отдельное frozen reference state, BuildBatch/Compact |
| C02 | K0 `11.DPO/DpoTrainer.cs`, 440–815 | StepVector, реальный optimizer recipient, текущий mean scorer, backward и VectorSignal |
| C03 | K0 `10.LoRA/LoraLinear.cs`, 1–155 | две зарегистрированные матрицы A/B, base+low-rank forward, dropout state; не перечень всех активных параметров конкретного пилота |
| C04 | K0 `26.TrainableRouter/Training/DistributedModelOrchestratorFlow.cs`, весь файл | specialist route возвращает endpoint/идентификаторы, но этот метод не выполняет specialist generation |
| C05 | K0 `10.LoRA/LoraApplicator.cs`, 160–315 | отдельные модели/seeded pair/triple topology; прочитанный участок не закрывает все RequiresGrad/install paths |

R04–R16 FINDINGS приняты адресно как результаты других исследований, а не повторно исполненные трассы. В том числе R10 существует; R13 требует repair; R15 содержит parent-review, но independent invocation NOT_PERFORMED. Первоначальный R16 FINDINGS устарел относительно P1; действующий перенос подтверждён прямым чтением A05. История `previous rounds thinking results.md` прочитана адресно, не целиком. Полный обход всех RAG и рекурсивная инвентаризация не заявляются.

Исходные skills из K0 `docs/skills`: `trainable-architecture-decision-SKILL.md` 1–110; `phase-planning-SKILL.md` 105–165; `plan-review-SKILL.md` 200–265; `code-examination-SKILL.md` 100–160; `context-package-design-SKILL.md` 1–110; `prompt-patch-authoring-SKILL.md` 1–100; `output-readability-gate-SKILL.md` 40–125. Применены причинная трасса, явные альтернативы, локальные пакеты и человеческое изложение; исторические 30/50 ревью и coverage-кампании не импортированы.

## 3. Новые адресные наблюдения кода

### CR-A01 — точная пара и фактический учебный материал расходятся

C01: `BuildBatch` сначала создаёт admission по raw Prompt/Chosen/Rejected, затем передаёт в `Encode` результат `Compact` каждого поля. `Compact` сворачивает whitespace и обрезает каждое поле до 480 символов. `ToMask` помечает единицами весь реальный prompt+answer, а не только ответ.

Это **подтверждённый преобразователь и риск**, не измерение частоты испорченных уроков. Контрпример: два ответа совпадают до 480-го символа, а дальше один содержит обязательное условие отмены. Учитель и judge различают ответы; полученные token sequences могут оказаться одинаковыми. Другой случай — пробелы/переносы внутри значимого литерала. Raw fingerprint сам по себе этого не предотвращает.

**Решение CR-D01:** сохранить raw и точное model-visible представление, не выполнять скрытый Compact. Превышение capacity — явный отказ от данного материала или новый отдельно созданный и оценённый короткий урок; не обрезка после оценки. Приложение связывает версию tokenizer, сериализацию prompt/answer, offsets, masks и hash с assessment. Подпись автора остаётся у действительно сгенерированных им байтов. Для неизвестного токена нельзя подставлять эквивалентную по длине пустоту.

### CR-A02 — текущая objective не является выбранной response-conditional DPO

C02: `ComputeLogProbs` усредняет log probability всех помеченных next-token positions. C01 включает prompt positions. Этот же helper используется публичным `EvaluateAverageNextTokenLogProbability`.

**Решение CR-D02:** для DPO вводится отдельный response-only sequence-log-probability путь. Публичный average inference scorer не меняется незаметно. Для chosen/rejected применяются один и тот же serialization contract, tokenizer и правило окончания ответа. Attention mask и loss mask — разные сущности: prompt видим модели, но не входит в response loss. Граница ответа определяется реально применённым кодированием; нельзя считать, что отдельное кодирование prompt и совместное кодирование prompt+answer имеют одинаковый BPE-префикс без проверки этого контракта.

Выбранный математический контракт:

`ell_theta(y|x) = sum_t log p_theta(y_t | x, y_<t)` по позициям ответа;

`m = beta * [(ell_theta(chosen|x)-ell_ref(chosen|x)) - (ell_theta(rejected|x)-ell_ref(rejected|x))]`;

`L = -log sigmoid(m)`.

Это контракт исходной DPO objective (Rafailov et al., arXiv:2305.18290v3, §4, Eq.7: https://arxiv.org/html/2305.18290v3). Усреднённая/нормированная по длине версия возможна только как отдельно названная экспериментальная objective, не как незаметная замена. При `policy=reference` значение m=0 ожидаемо; производная по m равна -1/2. Поэтому нулевая текущая margin не делает содержательно валидный урок недопустимым. Нулевой итоговый gradient/identical weights возможен по другим причинам и фиксируется честно.

### CR-A03 — TeachingVector здесь управляет силой шага, не смыслом выходного пространства

C02: `StepVector` умножает вклад на `VectorSignal`; последний считает число активных координат и ограничивает multiplier диапазоном 1…2. В этом потребителе перестановка ненулевых координат не изменяет multiplier. Поэтому его нельзя объявлять semantic domain projection, обучаемой tokenization или доказательством качества представления. Другие потребители не исключены.

**Решение CR-D03:** в первом сравнении зафиксировать текущую weighting policy и её стоимость; изменение weighting — самостоятельная ablation. Если сравниваются objective и semantic projection, они не меняются одновременно. Отдельно проверить соответствие reported loss фактически оптимизируемому weighted loss: текущий отчёт использует vectorWeight, тогда как gradient дополнительно использует VectorSignal. Метрики нельзя сопоставлять как один и тот же objective без описания этого различия.

### CR-A04 — обнаружен конкретный routing result, который не равен генерации

C04: при выборе specialist `OrchestrateAsync` возвращает ChosenUnitId, endpoint, TrainingRunId и пустые phases. Внутри этой ветви нет вызова specialist endpoint. Для orchestration candidate вызывается decomposition flow. Это подтверждает необходимость трассировать следующего caller/consumer; не доказывает отсутствие generation во всём репозитории.

**Решение CR-D04:** для первого S04 урока обучаемый получатель — существующий CodeDocumentation `LlmModel` согласно A05. Routing profiles и graph edge authority не подменяют его. Остаток — точный ordinary generation caller, выбор accepted model version и вызов модели. Повторное чтение assessment endpoint недостаточно. В R18 запрещён выдуманный готовый CLI/endpoint, если найден только routing receipt.

## 4. Разрешение архитектурных противоречий — результат R12-A в пределах прочитанного

| Решение | Сохранённый смысл | Разрешение конфликта / применение |
|---|---|---|
| F/D-04 CODE_Q, BUG, PLAN, ARCH, REFACTOR | канонические development intent labels | не переименовывать эти labels; добавить отдельные оси subject, language/direction, stack, project, intent. SQL+BUG допустимо; одно не заменяет другое |
| F/D-05 shared base + project LoRA | локальная специализация без отдельного полного base на каждый проект | сохранить isolation и version manifest; «2 MB» — историческое утверждение, не бюджет текущего ПК. Shared promotion требует отдельной междоменной/межпроектной оценки |
| F/D-06 replay70/30 | историческая гипотеза сохранения навыков | в текущей программе новых уроков действует one-pair/no-repeat из A03/A05. Историю можно использовать в разрешённом context; нельзя повторно обновлять веса по старой pair под новым ID. Новый replay-режим требует отдельного явного решения, не вводится этим ремонтом |
| F/D-07 detector before normalization | сохранить stack markers | принято без изменения; raw, semantic/order features и index space всё равно паспортизируются отдельно |
| F/D-08 stratified split | избегать случайного исчезновения классов | группировать сначала по независимой source/semantic family, затем стратифицировать; строковая стратификация не даёт независимости |
| F/D-10 BM25+kNN | сохранить уже предложенную fusion как baseline-кандидат | старый +8% не новое измерение; веса/шкалы/consumer исследуются раздельно, не назначаются оптимальными по историческому числу |
| F/D-11 TensorCore | использовать существующие tensor primitives | не добавлять новый внешний ML framework в первый patch |
| F/D-13 coverage | предметная плотность, разнообразие predicate profiles, named edge risks | это охват доменного материала, не требование branch/line coverage и не повод добавлять Windows перед каждым уроком |
| F/D-14 internal-first, idle update, guarded external help | локальность, стоимость, feedback и проверка перед установкой | сохранить runtime разрешения и idle gate; A05 отдельно определяет author/judge для учебного цикла. Наблюдаемый ввод не становится автоматически лицензированным DPO. 0.50 не универсальная вероятность и не новый порог |
| F/D-15 arbiter authority | priority выбирает попытку; arbiter решает допуск результата | действует и до retrieval как history features, и после generation как veto. Высокая similarity не отменяет нарушение доменного закона |

A04 разрешает правильный locator `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`. Ошибка адреса без `-v1` была исправлена; документ не объявлен отсутствующим. Это адресное закрытие locator, не закрытие всего R01/R03.

## 5. Причинные контракты четырёх ролей — результат R12-B

Ниже выбран **целевой контракт**, не утверждение, что все названные состояния уже реализованы. Существующие ILogical* envelopes/Graph runtime из R04/R06/R07 переиспользуются после точной сверки. Владение training остаётся у ILearningMachine.

| Роль | Вход → выход и детерминированная часть | Обучаемое состояние, сигнал, сохранение | Ordinary consumer и различающий опыт |
|---|---|---|---|
| Unit | `(request_id, input_id, domain, input_schema, output_schema, raw, representation_id)` → набор `(output_id, source_input_ids, typed_payload, evidence, confidence)`. Типы, scope, hard laws и границы данных проверяются до допуска | Для первого CodeDocumentation: параметры policy/установленного adapter конкретной model family. Сигнал — exact admitted DPO response preference. Для memory-unit: corpus/tokenizer/index state; это другой update, не gradient DPO. Snapshot/version включает соответствующее состояние | Ordinary document generation или actual typed output consumer. Freeze routing/Flow/Graph; изменить только Unit state и сравнить новые source families |
| Flow | набор outputs предыдущих Units + port schemas + association graph → согласованные inputs/outputs следующих Units. Детерминированные type/shape conversions допустимы, semantic order и input/output связь сохраняются | Проектируемый relation/correction selector `theta_flow`: выбор допустимого mapping/коррекции. Сигнал — предпочтение двух lawful correction plans, оценённых по последующему результату. Training command адресуется Machine и этому state family; не Unit solver | Runtime mapping consumer получает mapping+version и исполняет выбранное преобразование. Freeze Units; сравнить ошибочную/корректную ассоциацию и перенос на новые типосочетания |
| Orchestrator | goal, dependencies, contexts, candidate flows, budgets → async lawful execution plan + observed outcomes. Hard dependency, cancellation, deadlines, resource limits не обучаются как необязательные рекомендации | Проектируемая policy `theta_route` ранжирует допустимые планы/остановку; reward/preference учитывает качество, стоимость всех ветвей, failure/cancel. State version привязана к candidate/port contract | Реальный flow executor, не только адрес endpoint. Freeze Units и Flow mapping; менять policy, сравнивать partial-order validity и time-to-lawful-answer, включая wasted work |
| DynamicGraph | graph snapshot, pattern orders, admissible rewrites/compositions → typed composition/edge-authority decisions. Integrity, isolation и final arbiter остаются hard boundaries | Проектируемые learned relation/composition parameters или memory с явным update. GraphDpoPair обучает selector соответствующей ассоциации, не автоматически выбранный solver. Model/version/provenance → loader → trusted preload — ещё не доказанный code seam | Существующий L2 authority→L3 flow путь по R07; отсутствующее доказательство — происхождение accepted selector preload. Freeze primitives; оценить unseen lawful compositions, не только знакомые paths |

### Many→many: конкретное сохраняемое отношение

Иллюстрация: два входных фрагмента `A` и `B`; для A допустимы A1/A2, для B — B1; итоговая сводка AB зависит от A1 и B1. Transport должен сохранить `A→{A1,A2}`, `B→{B1}`, `{A1,B1}→AB`. Список из четырёх payload без этих рёбер не выполняет исходное требование. Простое `Zip(inputs,outputs)` неверно при разном числе результатов/частичном отказе/асинхронном завершении.

**Выбранная форма целевого transport:** stable input/output IDs + `source_input_ids` + typed payload либо точный versioned payload reference + explicit ordering/dependency edges. OrderIndex не заменяет идентичность; порядок завершения async не становится смысловым порядком. Пустой результат и many-valid-results — разные исходы. `IQueryable` не считается реализованным контрактом только потому, что вернулся IEnumerable/массив; требования к deferred query проверяются отдельно и не решаются переименованием.

**Честный незакрытый факт R04:** конкретный current `OutputRefs` resolver и все segment→payload callers ещё не traced здесь. Форма выше решает проектный вопрос, но не подтверждает существующую проводку.

## 6. Полный DPO lifecycle — результат R12-C

1. Разрешённый диагностический input выявляет конкретную слабость; закрытый confirmatory материал не передаётся teacher.
2. CODEX/Astra выбирает учебный case в human-role. Проверяется действующий profile/capacity/privacy admission; этот документ не разрешает новый model call.
3. Допущенная открытая китайская модель создаёт обе финальные стороны. Сохраняются actual provider/model/profile/artifact provenance, raw bytes, family_id и связь с задачей.
4. Judge видит обезличенные A/B без авторской подсказки chosen. Его ответ разделён на `winner=A|B|TIE|UNJUDGEABLE`, `quality_A`, `quality_B`, `pair_usable`, `violation_reason`. BOTH_BAD не тождественен tie; две хорошие, но равноценные стороны не дают полезной preference. Если лучше B, нельзя молча подменить стороны после assessment: application оформляет новый assessed ordered candidate по действующему admission.
5. CR-D01 связывает оценённую пару с actual optimizer serialization и масками. Pair validity не определяется текущей DPO margin. `LOW_CONTRAST` должен означать явно указанный semantic/token/probability диагноз; неизвестный downstream consumer этого флага остаётся адресным остатком R05.
6. Before mutation фиксируются parent bytes, trainable parameter key set, reference identity, optimizer/RNG/counters, accepted/pending pair IDs и изменяемые representation/index state. Всё неизменяемое указывается как frozen, а не копируется бесконечно ради отчёта.
7. Одно разрешённое применение новой пары обновляет правильный recipient. DPO loss не обучает graph selector, если gradients фактически идут только в document LlmModel.
8. Challenger оценивается изолированно, без ранней установки как accepted. Parent/no-update получают тот же допустимый retrieval context и compute policy; новые teaching answers не добавляются одному control как скрытая память.
9. IMPROVE при заранее зафиксированной practically-useful разнице/retention/cost допускает promotion. REGRESS/INCONCLUSIVE сохраняют parent. INVALID_EVIDENCE требует исправить опыт и не называется отсутствием эффекта. Никакой target/threshold не снижается после результатов.
10. После accept — publication по разрешённому этапу и cold ordinary consumer readback выбранной версии. Для local S04 не добавляется автоматически remote S05 gate; requirements S05 сохраняются отдельно. Fault recovery не повторяет уже выполненный optimizer step ради отсутствующего отчёта.
11. Следующий bulk — новые последовательные допустимые уроки. Повторы/attempts, unique pairs, independent families, optimizer entries, accepted updates и accepted improvements считаются отдельно.

### Preference над путями не сводится к similarity

Для selector DPO нужны normalized policy probabilities над одинаковым допустимым candidate set либо conditional action probabilities всего path; `sum log p(action|history)` сохраняет порядок. Arbitrary cosine/BM25 scores сами по себе не `log pi`. Для обучаемого reranker допустима отдельная pairwise ranking loss, но её нельзя подписывать «standard DPO», пока не задана policy/reference probability model. Correct deterministic answer не становится negative из-за способа получения.

## 7. Заполненный кандидат dynamic kNN — результат R12-D

**Выбранный предметный кандидат:** C# CodeDocumentation, различение null-conditional semantics. Это проектируемая hypothesis family для E2, не новая обязательная зависимость E1.

**Допустимый пул:** только разрешённые project/scope entries с compatible C# language/representation version, нужной задачей explanation и проверенными источниками. Mixed SQL/C# input сначала разделяется по subjects и ports; не маскируется высоким CODE_Q confidence. Positive, negative и warning находятся в разных typed roles; rejected fragment не копируется как правильный ответ.

**Признаки пары query/neighbor:** (1) exact AST operator relation `?.` / `??` / ordinary `.`, (2) nullable input/return type match, (3) control-flow/guard scope, (4) preserved literal/quantity match, (5) lexical/code identifier relevance, (6) same arbiter-risk pattern, (7) semantic embedding similarity при compatible space. Binary law/type features не заменяются token-count; BM25 и cosine сначала калибруются на development families в сопоставимые bounded ranks, не складываются как одинаковые единицы.

**Score family:** `s_d(q,n)=sum_j softmax(g_theta(d,q))_j * phi_j(q,n) - risk_penalty(d,q,n)`. Hard eligibility применяется раньше score. `risk_penalty` не отменяет hard veto. Small bounded `g_theta` — проектируемый domain/query reranker; Machine хранит его параметры и update history. Учебный сигнал — оценённое предпочтение candidate evidence/plan по качеству конечной документации, а не авторский chosen label.

**k/radius:** первое сравнение удерживает pool, encoder, k и consumer постоянными и меняет только reranker. Следующий отдельный опыт разрешает policy выбрать из `k∈{1,2,4,8}` в пределах фактического eligible count и capacity. Это исследовательская ограниченная сетка, не объявленный оптимум. Policy наблюдает top-gap, противоречия, количество независимых полезных evidence families и residual budget. Radius нормируется в versioned score-space; при insufficient evidence результат — abstain/knowledge edge, а не принудительный best wrong neighbor.

**Кому нужны дополнительные соседи:** проектируемый typed evidence aggregator сопоставляет подтверждённые semantic claims с source IDs; conflicting/negative examples дают предупреждение, а не голос за ошибку. Solver получает evidence set и сохраняет claim→source связь. В старом first-hit `KnnLogicalUnit` дополнительные IDs сами по себе ещё не такой aggregator. Если output остается `bestExample.Output`, нельзя приписывать новый gain «объединению соседей».

**Index strategy:** малый lawful pool — existing exact search diagnostic. HNSW остаётся candidate generator в собственном fixed vector space; query-dependent score используется для rerank. Nonmetric score нельзя незаметно передать индексу с cosine assumptions. Отдельно измерять ANN recall against exact candidate ranking. Смена представления — новый index identity, rebuild/migration и атомарная совместимая установка; одинаковая размерность не доказательство совместимости.

**Falsifier:** если при том же pool/encoder/consumer на новых semantic families нет выигрыша либо растут ошибки null semantics/cost, hypothesis не поддержана. Если выигрывает только oracle-domain, слабое место mapper, а не solver. Если exact помогает, ANN нет — расследовать candidate recall, не отвергать сразу learned score.

## 8. LoRA и векторный результат — результат R12-E

**Обнаружено C03:** `LoraLinear` возвращает base(x) + scaled low-rank transformation через A/B; класс не делает tokenization и сам не устанавливает policy freeze всех base parameters. Две матрицы зарегистрированы как `lora_a/lora_b`. C02 optimizer берёт фактические RequiresGrad parameters. Полный key set конкретного пилота всё ещё требует traced installation и model manifest, а не вывода из имени папки.

**Выбранный первый путь:** сохранить compatible persisted tokenizer и base/adapter topology CodeDocumentation. Не добавлять semantic output projector как условие первого DPO урока. Для LoRA efficacy сравнивать одинаковый base/inputs/policy и конкретную адаптацию; ordinary consumer обязан загрузить эти adapters, а не только новый файл метрик.

**Выбранный следующий projection design:** если Flow требуется перевести solver output в domain vector/token, завести отдельное versioned преобразование `P_d: source_space -> target_space` с named owner. Вход — typed semantic result/его frozen encoder representation; выход — координаты документированного target codebook либо logits над target tokens. Supervised/contrastive alignment обучает вектор; conditional token objective — вероятностный decoder. Low-rank A/B могут параметризовать P_d, но это не делает его inverse tokenizer. Смысл координат, output decoder и consumer фиксируются до обучения. Без target identity задача проекции недоопределена.

Shared/project isolation: сохраняется shared base; project adapter не повышается в shared автоматически. Снимок включает base hash, target modules, ranks, alpha, dropout/RNG и project/domain scope; применимость новой проекции к старому kNN index проверяется как migration, не угадывается по форме tensor.

**Полный бюджет ПК:** peak resident memory учитывает одновременно живые policy/reference/assessment copies, frozen base, adapters, gradients, optimizer moments, activations/logits, tokenizer, index/graph/cache, rollback snapshots и process overhead. Disk включает parents/candidates/manifests/индексы/сохранённые пары. C01 реально хранит отдельный frozen reference и полные parameter snapshots; следовательно, размер одного adapter не верхняя оценка всего процесса.

Для A/B размер trainable coefficients равен `r*(d_in+d_out)` на projection; даже маленькое значение не ограничивает activations или полный base. Бюджет выражается через конкретные размеры/precision/concurrency и измеренный high-water mark выбранного ПК. Этот документ не подтверждает 2 MB adapter, 8 GB fit, CPU speedup или ускорение всей системы. Внешний teacher/evaluator тоже входят в стоимость опыта, даже если не занимают локальную VRAM.

## 9. Выбор первого вмешательства — результат R12-F, не готовая команда запуска

| Кандидат | Информативный вопрос | Переиспользование | Остаток / риск | Решение сейчас |
|---|---|---|---|---|
| E1 CodeDocumentation exact DPO | умеет ли существующий Student перенести допустимый новый урок в обычный ответ? | existing trainer, checkpoint/assessment lifecycle | exact material/objective; active parameter set; ordinary generation bridge; current capacity/profile | **выбран основной путь первого обучения**: прямо соответствует A05 и проверяет центральный разрыв, не создавая новую архитектуру |
| E2 C# domain reranker | помогает ли law-sensitive retrieval при fixed solver? | existing BPE/kNN, exact search и предметный пример §7 | learned score/aggregator ещё проектируются; нельзя выдать memory enrichment за neural learning | **параллельный исследовательский путь**, не запасная подмена E1. Может дать раннее полезное observation без ожидания общего Graph |
| E3 Flow/Orchestrator | обучаются ли mapping/выбор пути при frozen Units? | runtime contracts и graph execution seams | нет доказанного accepted relation-policy loader и полной many→many проводки | следующий собственный опыт, не prerequisite document E1 |
| E4 higher-order Graph | переносит ли система сочетания на unseen lawful compositions? | topology/edge-authority/L3 runtime | lifecycle и semantic selector recipient ещё не привязаны | сохраняется обязательством полной программы; не объявляется выполненным E1–E3 |

Выбор E1 — проектное решение по **явному текущему объекту обучения A05 и локализованному causal bottleneck**, а не утверждение о численно максимальном information gain. Implementation cost категориально меньше нового полного Graph пути за счёт переиспользования; сроки/стоимость не измерены. Если допустимый existing checkpoint/ordinary caller не найден, остаётся конкретный readiness gap, не бесконечная смена winner. Можно продолжить E2 research, но нельзя назвать его выполненным S04 DPO.

**Граница передачи R18:** выбран предмет/recipient family и решение input/objective; не привязаны фактический checkpoint, approved lesson identity, complete trainable key set, LOW_CONTRAST downstream и ordinary generation entry. Поэтому `ARCHITECTURE_DELTA_DESIGNED`, но `FIRST_LESSON_RUNTIME_BINDING_OPEN`. Эти поля заполняются текущими bytes/callers, не выдуманными путями. Никаких новых Windows runs, покрытия, обучения или правок core этим документом не выполнено.
