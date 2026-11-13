# Восстановленный алгоритмический контракт: от входа до следующего урока

**13.10.2026 · CP2 · документный проект, не выполненное обучение.** Это предметное дополнение существующего HYBRID-ALGORITHMS-AND-RUNS, а не новая архитектура с нуля и не ещё одна очередь. Разделы A01–A09 используются адресно назначенным Rxx. Подробные поручения находятся в RECOVERED-DESIGN-PROMPTS.md; действующие исходные work orders сохраняются.

Главная задача пересмотра: найти минимальную недостающую связь в уже существующей системе, реализовать её в последующей разрешённой работе и начать получать различающие учебные наблюдения. Новые tests/coverage и Windows jobs не являются результатом этой программы. В то же время качество обучаемого поведения и корректность программного/математического пути — разные вопросы: отказ от coverage-first не разрешает неверные градиенты, потерю состояния или данных. Обученное вычисление может быть детерминированным; случайность сама по себе не означает обучение.

## A01. Первичные основания и что именно ими установлено

P = `lubochka/xiigen-planning@0e6d55213c9e1aa317fec7308ea37ef1216a0d60`; D = `llm mvp/design to code session`; G = `llm mvp/dpo legal build`; F = `llm mvp/llm dynamic flow`; C = `llm mvp/visual studio & visual studio code continue`.

K = `lubochka/llm_mvp_core@a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3`. Результаты R01–R03 используют также прежний K `731d2e55728caf601f05daaca7ab3fc370dd0240`. Сравнение этих refs показывает новый S04/PSEC-коммит; перечисленные ниже LoRA/DPO/core contracts и прочитанные skills не входят в его список изменённых путей. Это проверка области дельты, не аудит всего нового S04. Уже прочитанное не обнуляется новым HEAD.

| ID | Источник / реально прочитанный участок при подготовке | Значение и граница доказательства |
|---|---|---|
| S01 | [R02 FINDINGS](results/R02/FINDINGS.md), §§1–2,5–7; [R03 FINDINGS](results/R03/FINDINGS.md), F01–F08; [R01 FINDINGS](results/R01/FINDINGS.md), F01–F05 | Восстановленные требования и маршруты. Часть больших возвратов инструмента усекалась; весь архив результатов не объявлен прочитанным. Не доказательство обучения. |
| S02 | [G/plan-09-auto-dpo-loop](<../../dpo legal build/active-codex-execution-plan-09-auto-dpo-loop.md>), 1–90 | Исходное отображение четырёх ролей на существующие уровни; Graph как специализация L5; плановая цепь богатой пары. Июльские code-first/DPO-last, статусы и команды — история. |
| S03 | [F/DECISIONS-LOCKED](<../../llm dynamic flow/DECISIONS-LOCKED.md>), таблица D01–D15 целиком | StackDetector до normalization, TensorCore, shared base/project LoRA, BM25+kNN, compression tiers, память/арбитры. Старые проценты, 20× и 2 MB — заявления исторического документа, не измерения текущего core. Replay конфликтует с активным one-pair. |
| S04 | [WorkLeft RAG usage guide](<../09.09.26WorkLeft-RAG-00-usage-guide.md>), 1–58; RAG-01 NDJSON 1–70 частично; RAG-03/04 NDJSON полные возвращённые записи; RAG-02 первые записи | Проверены разные корни и входы; RAG-01 приводит к S02, RAG-04 содержит вложенные карты dynamic-flow/translation/IDE. Это не завершение рекурсивного обхода R03. |
| S05 | [предыдущие раунды](<previous rounds thinking results.md>), начало и 1204–1380 частично | Уже исправлялись same-panel оценка, отсутствие выигрыша, 25 сессий и реальный dispatcher. Не повторять тот же трактат и не реактивировать старое read-only или старое R03=CI. |
| K01 | [00.Core/DynamicGraphContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/731d2e55728caf601f05daaca7ab3fc370dd0240/00.Core/DynamicGraphContracts.cs#L21-L126), 1–185 | ExecuteAsync возвращает queryable кандидатов, у которых OutputRefs/OutputType; пять relation kinds; L2 AuthorityUnitId и отдельный baseline. Реальное materialization и качество ещё требуют трассировки. |
| K02 | [00.Core/LearningMachineContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3/00.Core/LearningMachineContracts.cs), 1–170 | Владелец Train — machine, не Unit/Flow; LearningTrainingItem содержит preference triple; evaluation carrier имеет ExpectedResponse; common/personal и portability. Сигнатура не доказывает update или буквальное сравнение строк. |
| K03 | [21.AgentInfrastructure/Logical/BoundedDpoTrainingContentProjector.cs](https://github.com/lubochka/llm_mvp_core/blob/a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3/21.AgentInfrastructure/Logical/BoundedDpoTrainingContentProjector.cs), 1–144 | Identity включает Prompt/Chosen/Rejected, TeachingVector и consumer Context. Ограничение числа строк — технический bound, не разрешение переобучать bulk. |
| K04 | [11.DPO/DpoDataset.cs](https://github.com/lubochka/llm_mvp_core/blob/a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3/11.DPO/DpoDataset.cs), 1–200 | Load→ParseRow, отдельная admission metadata; legacy row имеет admission=null; GetRow возвращает row, Shuffle/indexer — Pair. Нужно проверить достижимых consumers, а не объявить обход admission по одному интерфейсу. |
| K05 | [11.DPO/DpoTrainer.cs](https://github.com/lubochka/llm_mvp_core/blob/a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3/11.DPO/DpoTrainer.cs), 1–170 | Объявлены policy/reference/AdamW/mutation admission и DPO formula. Исполнение loss, masks, backward и обновление параметров этим чтением не доказаны. |
| K06 | [10.LoRA/LoraLinear.cs](https://github.com/lubochka/llm_mvp_core/blob/a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3/10.LoRA/LoraLinear.cs), полный возвращённый класс | Конкретный base + A/B forward, dimensions, adapter parameters и RNG restore. Не tokenizer, не доказанное доменное embedding-пространство. |

Для первого использования критического вывода исполнитель читает его первичный участок, фиксирует actual ref/blob/span и только изменённую дельту. Путь из R02 не выдаётся за собственный code trace. Другие исходные требования, все WorkLeft, S00–S10 и библиотеки остаются в SOURCE-ROUTES и распределении R01–R10. Таблица выше не заменяет whole-program source map.

## A02. Сохраняемая архитектура и адреса изменения поведения

**Unit → Flow → Orchestrator → DynamicGraph** — функциональная иерархия, не обязательный линейный маршрут каждого ответа и не четыре новых trainer. S02 сопоставляет Unit/Flow/Orchestrator с ILogicalUnit/Flow/Orchestrator, а DynamicGraph — с `ILearningMachine<DynamicGraphDomain>` поверх графа. Не вводить шестой уровень либо второй trainer лишь из-за исходного alias. Точную сегодняшнюю реализацию устанавливают R04–R06.

| Уровень | Детерминированная работа | Что может учиться | Обязательный наблюдаемый результат |
|---|---|---|---|
| Unit | Типы/shape, ограничения домена, materialization множества кандидатов | Преобразование, проекция, содержательное ранжирование | Многие входы не схлопываются; допустимые outputs запрошенного типа сохраняют смысл и полезное разнообразие. |
| Flow | Проверка совместимости портов, перенос/объединение значений, ограниченная correction loop | Выбор полезного отношения, correction/mapping и learned authority ребра | При фиксированных units изменение связи причинно улучшает результат или экономит вычисления без потери качества. |
| Orchestrator | Dependencies, scheduling, quotas, cancellation, учёт всех branches | Выбор комбинаций, stopping/selection policy в допустимой области | Одному входу доступны разные комбинации; скорость не покупается выбором раннего неверного ответа. |
| DynamicGraph | Хранение/версионирование топологии, перечисление допустимых подграфов | Выбор/композиция паттернов разных порядков, диагностирование слабого механизма | Перенос на новые композиции, а не просто рост edge count или воспроизведение знакомого пути. |

Для каждого изменяемого механизма требуется цепь **сигнал → owner параметров/памяти → update → checkpoint/version → обычный inference consumer → наблюдаемое отличие**. Оптимизатор может принадлежать machine, а обучаемые параметры — конкретному authority unit. Если текущий unit-level механизм недостаточно выразителен для нужного отношения, R12 оформляет точный gap и проект изменения; требование graph learning не вычёркивается, `Level=graph` не появляется молча.

Паспорт соединения: producer/method; target/port; domain+intent; schema и coordinate version; cardinality; OutputRefs→реальный payload; допустимая потеря информации; frame/units; authority unit; fixed constraints; fan-out/fan-in/return-loop; состояние и его owner; async completion/partial/cancel semantics; budget; способ оценки. Одинаковая длина вектора не означает одинаковое пространство.

**Способы управлять связями для исследования:** зафиксировать units и заменить одну edge policy; зафиксировать связи и изменить один Unit/LoRA; сохранить решатели, но сравнить fixed/sequential и adaptive/async execution; сравнить плоский набор паттернов с одной higher-order композицией. При доказанном взаимодействии — одно 2×2 comparison, не полный перебор всего графа. Не приписывать каждому ребру общий reward всего пути без локализации причины.

## A03. Доменное представление: что сжимать и что нельзя потерять

Порядок исследования: **raw input → признаки языка/стека → безопасная нормализация → domain+intent → typed symbols/features/relations → embedding/projection → optional codebook → Unit/Flow/kNN/Graph → requested output**. Порядок частей может уточняться по коду, но S03 D07 запрещает терять C#/TS/PY markers нормализацией до их извлечения.

Предметный домен, намерение CODE_Q/BUG/PLAN/ARCH/REFACTOR, язык/направление, версия стека, уровень композиции и physical Student — разные оси. Не переписывать Domain enum до анализа его consumers; составная metadata может жить в существующем envelope. Mixed/unknown не обязаны получать ложный уверенный singleton class.

Каждый доменный «закон» получает источник, версию и категорию: строгий invariant; приближённая модель с предпосылками; пользовательская policy; экспертное предпочтение; неизвестное. Например, сохранение отрицания и единиц — функциональный контраст; выбранный стиль текста не универсальный закон. В языковом домене нет одного уникального правильного предложения; набор семантически допустимых ответов оценивается рубрикой.

Для каждого этапа representation указать retained information, потерянное, dimension/bytes, trainable owner, recovery/decoder и downstream criterion. Пять compression tiers из S03 сохраняются как исследуемый дизайн; не переносить старое 20× как достигнутый коэффициент. Сжатие успешно только при достаточном качестве задачи/отношений и снижении **полной** цены, включая codebook/index/decoder. Round-trip обязателен лишь там, где нужна обратимость; для lossy semantic projection оценивать полезность downstream, а не требовать восстановить каждый исходный символ.

**Три контраста для паспорта:** разные формулировки одного смысла должны оставаться близкими/совместимыми; близкие формулировки с разными отрицаниями/типами/единицами должны различаться; новые сочетания известных понятий должны сохранять отношения. Эти иллюстрации не являются закрытым экзаменом и не должны автоматически попасть в обучение.

## A04. Dynamic kNN: конкретное семейство кандидатов, не обещание лучшей функции

Сначала определить разрешённый пул `P(domain, permissions, schema, checkpoint, provenance)`. Domain uncertainty обрабатывается ограниченным multi-domain routing либо abstention; кандидат из неверного пространства не становится пригодным из-за похожих координат. Negative/warning memory фильтруется по актуальному контексту и версии правила: отказ в старых условиях не универсальный запрет.

Проектируемый пример функции для сравнения, **не утверждённый универсальный алгоритм**:

`D_d(q,z) = Σ_j w_j(d,q) · δ_j(phi_j(q), phi_j(z))`, где `w_j >= 0`, `Σ_j w_j = 1`.

Группы j выбираются предметно: lexical identifiers; semantic content; AST/tree shape; numeric values и units; relation/dependency structure. Масштабы δ нормируются по данным обучения, не по закрытому экзамену. Query-dependent веса могут сделать функцию несимметричной/неметрической; нельзя автоматически применить индекс, требующий метрику. На первом ограниченном наборе точный поиск — reference; ANN добавляется только при измеренной необходимости и допустимой retrieval loss.

`k_d(q)`/radius/abstention выбираются по наблюдаемой плотности, ambiguity, margin и бюджету. Learned selector получает собственный сигнал качества соседей/решения; фиксированный radius или счётчик не выдаётся за обучаемую политику. Сначала сравнить существующее расстояние с одной доменной функцией при прежнем k; затем отдельным опытом adaptive k. Не оптимизировать всё одновременно до первого урока.

**BM25+kNN:** S03 D10 сохраняет lexical+semantic fusion как важного кандидата для переиспользования. Для версии с несовместимыми raw scales допустим, например, weighted reciprocal-rank fusion `s(z)=Σ_m a_m/(c+rank_m(z))`; выбор весов/c и пригодность rank fusion — предмет R12/R22, не уже реализованная функция. Не складывать необоснованно BM25-score с cosine. Сравнивать BM25, kNN и hybrid только там, где они имеют смысл, с тем же solver и бюджетом.

У соседа отдельно хранятся relevance и допустимое использование: positive exemplar; negative с причиной существенного дефекта; warning для ограничения/риска. Warning не обязан быть rejected в DPO. Similarity не отменяет post-generation arbiter. Арбитр также может ошибаться: разногласие направляется на предметный разбор, а не обходится высоким score.

Измерять routing отдельно от retrieval отдельно от solver. Диагностики: actual domain против экспертно допустимого domain при том же solver; actual neighbors против обоснованного reference pool; memory с/без history. Экспертные маршруты/соседи в диагностике не становятся доступной production-подсказкой. Качество соседей, доля вредного positive reuse, downstream utility, latency и reindex cost обязательны; hit@k сам по себе не доказывает обучение.

## A05. DPO: качественная пара, правильный получатель, полезный update

**Три разных решения:** можно ли использовать материал; предпочтительна ли exact chosen сторона; улучшает ли update Student на новых задачах. Правильное происхождение не доказывает качество, хорошая пара не гарантирует полезность.

На одной задаче x и одном доступном контексте рассматриваются два результата/пути. Chosen удовлетворяет обязательным условиям; rejected содержит правдоподобную существенную ошибку, различающую нужную способность. Стороны не различаются скрытым доступом к эталону, бюджетом или комплектом инструментов без явного выделения этого фактора. Оба корректны и равнозначны — TIE; оба непригодны — BOTH_BAD; недостаточно evidence — UNKNOWN. Эти случаи не принуждаются к обычному DPO. Допустимый preference между двумя корректными решениями возможен по заранее объявленному критерию стоимости/стиля, но не должен разрушать semantic correctness.

Паспорт кандидата содержит: source/family и разрешённое использование; task/domain/intent; общий контекст; chosen/rejected и separate authors; rubric и существенную причину; role/UnitId либо адрес authority; state/checkpoint/representation refs; candidate order; exact-content identity; requested/actual judge; verdict. Это **семантическая запись для расследования и будущего проектирования**, не объявление нового JSONL schema. Содержимое закрытого экзамена и judge rationale не копируется в обучающие targets через generator.

Восстановленный S02 путь: `GraphDpoPairCandidate → разрешённая projection/admission → GraphDpoPair(unit+UnitId) → LearningTrainingItem/JSONL → DpoDataset → machine/trainer`. K03 дополнительно связывает TeachingVector и Context. K04 не доказывает, что rich metadata дошла до optimizer: R05 прослеживает каждое поле или его реальный out-of-band owner. Отдельно проверяются consumers `GetRow`, indexer и `Shuffle`; чтение legacy admission=null не равнозначно разрешённому update. Не расширять удобный DTO вместо выяснения настоящего транспорта.

Математический эталон стандартного DPO [Rafailov et al.](https://arxiv.org/abs/2305.18290):

`L = -log sigmoid(β · [(log πθ(y+|x)-log πθ(y-|x)) - (log πref(y+|x)-log πref(y-|x))])`.

R05 проверяет реальные log-probabilities, token masks/reduction, common conditioning, frozen reference, backward, trainable parameters и owner допуска до мутации. Комментарий K05 с формулой не доказательство правильного исполнения. Для finite route policy π по допустимым действиям возможна отдельная постановка preference objective, но только если определены нормировка, reference и credit assignment. Pairwise metric/ranking loss для kNN не переименовывается в стандартный DPO. TensorCore остаётся базой; новый внешний ML framework не вводится молча.

**Причинная адресация:** исправление неверного домена обучает доступную domain authority; правильный domain, но неверное отношение — соответствующий Unit/edge authority; плохой renderer при хорошем route не обвиняет все рёбра одинаково. В первом опыте меняется один owner при фиксированных остальных. Раздельно фиксировать число branches, пар, попыток, optimizer steps, принятых состояний: это разные величины.

Сохраняются one-pair/no-repeat/full-state rollback. Bulk — группа новых уроков для отчёта; не replay 70/30, не несколько epochs старых пар, не новый ID того же урока. Семантику одного урока/шага уточняет R12 по текущему коду и owner decisions, не по максимальному batch-size класса. Идентичность содержимого, пользовательский event и применение к конкретному owner — разные ключи; дедупликация не должна ни разрешать скрытый replay, ни запрещать допустимые разные задачи без анализа. Неизвестно завершившийся update сначала разрешается по журналу состояния, а не повторяется.

CODEX/Astra Ultra в заданной пользователем роли выбирает учебную слабость и оценивает exact pair; допущенный open-weight Chinese generator/собственный Builder создаёт материал; Student учится. Actual invocation устанавливается по реальному выполнению, не по записи имени в prompt. Независимая R15/R25 экспертиза — отдельная роль, не самооценка автора пары или этого документа.

## A06. LoRA: реальная проекция и смысл её результата

В K06 выполнена конкретная форма `y = base(x) + scaling · (dropout(x) Aᵀ) Bᵀ`, `A: r×din`, `B: dout×r`. Число параметров добавки равно `r(din+dout)`; это арифметика формы, не фактический RAM всего обучения. [Hu et al.](https://arxiv.org/abs/2106.09685) — первичная методическая опора low-rank adaptation, не доказательство результата XIIGEN.

R05 обязан назвать конкретную матрицу, owner, dimensions/rank/scaling, trainable/frozen state, objective, сохранение/random state и **consumer выходного вектора**. S03 shared base + project adapters сохраняется как архитектурная опора, а не основание обещать 2 MB или запуск на любом ПК. Адаптация attention projection не становится автоматически output-token projector: нужное подключение подтверждается по code path.

Сравнения: frozen projection против одной адаптации; downstream semantic task до/после; различимость отрицания/отношения/единиц; междоменный перенос и forgetting; совместимость существующего индекса. Rotation/scale drift может изменить retrieval без улучшения решения. Для discrete vector token нужен отдельный codebook/decoder, если consumer его требует; создание новых token IDs и изменение tokenizer vocabulary не побочный эффект LoRA. Менять координаты без согласованного re-embedding/reindex нельзя выдавать за чистый эффект LoRA.

## A07. Предметные домены и содержательные контрпримеры

Это иллюстрации для проектирования curriculum, **не готовый допущенный корпус и не независимый экзамен**. У реальных примеров R08/R21 фиксируют права, primary reference, версию среды и family lineage. Семь baseline сохраняются, новые исходные задачи/языки добавляются отдельно.

| Домен | Сохраняемые признаки / разновидности законов | Хорошая и плохая сторона одного различающего случая | Край и наблюдение |
|---|---|---|---|
| EN↔RU | Отрицание, числа/единицы, роли участников, направление, терминология; semantic fidelity, не буквальная строка | Перевод сохраняет «не более 5»; near-miss меняет на «более 5». | Два равноценных перевода = TIE, mixed-language/OOV; ошибка смысла и время отдельно. |
| Оркестрация | Preconditions, dependency DAG, side effects, resource limits, partial outputs | Путь учитывает зависимость входа B от A; near-miss запускает B с неверным промежуточным состоянием. | Две допустимые комбинации разной цены; поздняя хорошая ветвь против ранней неверной, partial/cancelled result. |
| Figma→HTML+CSS | Дерево/relations, layout constraints, размеры/units, viewport, semantics; user-tier quality/cost | Одна реализация сохраняет заданные отношения на нескольких разрешениях; другая похожа только на одном. | Missing asset, длинный текст/RTL; оценка рендера и структуры, не pixel equality одного screenshot. |
| C# documentation | AST/symbols, types, nullability, exceptions/side effects, версия языка | Описание соответствует достижимому поведению; near-miss обещает результат/побочный эффект, которого нет. | Async/overloads/generics; grounding в exact code, допустимы разные формулировки. |
| Angular documentation | TS/template bindings, data/events/lifecycle, версия framework | Объяснение следует фактическим bindings; near-miss путает направление события и данных. | Динамический template/условный render; одинаковое имя в разных scopes. |
| SQL documentation | Dialect/schema, keys/cardinality, NULL, joins/filtering/aggregation | Описание учитывает фактическую семантику запроса; near-miss приписывает rows, которые удаляет условие. | NULL/duplicates/outer joins, разные допустимые планы; смысл результата важнее похожего текста. |
| Builder для шести | Domain/task, family diversity, constraint/rubric, authorship и target owner | Создаёт обе допустимо происходящие стороны с осмысленным near-miss и привязкой к слабости. | BOTH_BAD, cosmetic negative, ambiguous preference, leaked exam; pair quality отдельно от Student gain. |

На каждом уровне Unit/Flow/Orchestrator/Graph есть свои новые композиции. Нельзя умножить семь доменов на четыре уровня и потребовать полный curriculum до первого урока; требуется полная карта и постепенно исполняемая очередь. Дополнительные HE↔EN, server/client code, stored procedures, offline IDE, DTC APIs/БД/публикация не исчезают из конечной поставки.

## A08. Измерение полезного обучения и компактности

Минимальный опыт начинается с **существующего пригодного baseline, одного target owner/динамического изменения и одной заранее определённой панели новых семейства задач**. R13 выбирает малый операционный panel с обоснованным размером и первым доменом по actual baseline readiness, затем существенно отличающийся второй домен. Нельзя заранее назначить code-documentation единственным доменом только из-за S04 или требовать всю platform/Windows поставку до опыта, которому она причинно не нужна.

До наблюдений фиксируются snapshot состояния и данных, lineage split, rubric, unit of independence, case weights, доступные ресурсы, единственное изменение, stopping/budget, критерии improved/regressed/inconclusive/invalid, допуск качества для сравнения цены. Конкретные численные thresholds R13 выбирает с предметным обоснованием, не после результата. Уже установленная owner goal 95% к эталону сохраняется как конечная цель, не произвольный ранний PASS.

Сравнение одного bulk: `ΔQ_d = Q(state_after,E_d) - Q(state_before,E_d)`. Панель E_d одинакова для обоих состояний. Обе версии получают новую панель при её смене. Парафразы одного источника не считаются независимыми семействами. Прежний checkpoint выполняет no-update comparison в сопоставимом контексте/retrieval/cache состоянии; отдельное изменение memory/index не маскируется под gradient learning. Опыт не обязан дать положительный результат.

Отчёт по каждому домену: semantic acceptable/correct, severe-law violations, routing/mixed/unknown outcomes, abstention и ошибка среди отвеченных, retrieval usefulness, path quality/полная цена, pair validity, improvement/retention, число независимых families и uncertainty. Сводный average не скрывает худший домен или редкие классы. Статистический метод согласуется с выборкой: family-level paired comparison/resampling или подходящий exact interval; при малой мощности — INCONCLUSIVE, не выдуманная значимость. Многократный просмотр operational panel делает его development feedback; для итогового удостоверения нужен отдельный неиспользованный набор.

**Best path:** сначала обязательная допустимость и достаточное качество; затем сравнение измеренных альтернатив по времени/ресурсам. Заявлять лучший среди рассмотренных вариантов при объявленном бюджете, не глобальный optimum. Если один путь точнее, другой дешевле и нет заданного обменного курса — показать Pareto-различие, а не придумать предпочтение владельца.

Полная цена: wall/CPU time; inference/update/teacher/judge/retrieval/reindex; все speculative branches и неудачные попытки; snapshot/rollback; peak RAM/VRAM; disk модели/base/adapters/optimizer/activations/index/graph/cache/manifests. Quantiles указывать с размером выборки. Положительный latency выигрыш при удвоении GPU work не является безусловной эффективностью. Локальный Student и teacher-assisted quality оцениваются раздельно. Сжатие текста, fewer parameters и memory-only retrieval gain не называются одним и тем же learning gain; kNN может улучшать ответ без gradient update, что отдельно показывают и исследования [Khandelwal et al.](https://arxiv.org/abs/1911.00172).

Исходы: IMPROVE; REGRESS; INCONCLUSIVE; INVALID_EVIDENCE. OOM или time-budget exhaustion — наблюдаемый неуспех в соответствующем режиме; утечка эталона/неодинаковые условия — invalid comparison. Экспертная Astra Ultra рецензия оспаривает причинный вывод и доменную семантику, но не заменяет численные наблюдения.

## A09. Первый урок, feedback и границы состояния

Путь пользователя: новый input → ответ принятого snapshot → оценка до обучения на этом input → допустимая коррекция/сигнал → правильная новая пара → изолированное candidate update → сопоставимая оценка и retention → принятие/восстановление → следующий новый input. Вредоносный, приватный, противоречивый или неверный feedback не считается истиной по факту получения. Personal state не попадает в shared/global без разрешения.

Согласованный state включает параметры/LoRA, optimizer и счётчики, RNG, representations/codebook, memory/index, graph authority и lifecycle journal. Определить actual ownership/version dependency, не требовать всегда копировать весь corpus. Snapshot/reference может быть immutable с copy-on-write там, где уже есть пригодная реализация. Publish не смешивает веса одной версии с индексом другой. При pending/unknown operation выяснить terminal state до retry/rollback. Retention не активирует скрытый replay старых pair IDs.

IdleDetector и internal-first из S03 разделяются на foreground пользовательский запрос и отдельно разрешённую учебную кампанию. Первый ответ не должен постоянно требовать внешнего учителя. План кампании не должен ждать неопределённого idle без описанного режима, budget/backpressure и остановки. Доступ к GitHub уже разрешён текущим поручением и не является external-model permission вопросом.

R13 передаёт первый examination recipe; R18 сначала выдаёт минимальный implementation packet, затем связанные расширения. При последующей отдельной команде на реализацию достаточный предметный пакет может работать до окончания всех curriculum/integration дополнений; этот документ сам не запускает код или обучение. Реальные блокеры — неизвестный update owner/objective/schema/data admission/consumer; ещё не измеренный прирост — вопрос опыта, а не повод бесконечно перепланировать.

**Текущий статус всего A01–A09:** восстановлены и конкретизированы требования, источники и способы будущего различения. Полезность выбранных гипотез, скорость и соответствие конкретному ПК — NOT_MEASURED; отдельная Astra Ultra экспертиза — NOT_PERFORMED. Никакого нового product test/coverage/Windows/trainer запуска этим документом не заявлено.
