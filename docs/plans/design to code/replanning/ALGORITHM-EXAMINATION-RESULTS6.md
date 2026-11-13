# От найденных алгоритмов к первому доказательному обучению

**13.10.2026 · RESULTS6 · предметная поправка к действующим 25 назначениям.** Это проектирование и публикация, не исполненный R10, не независимый review и не проведённые опыты. Эффективность/PC-fit — **NOT_MEASURED**, independent review — **NOT_PERFORMED**. Документная поправка подготовлена для проверки R15/R25; не означает разрешения запуска trainer или принятия новых весов.

## 1. Что изменилось после появления результатов всех шести исследований

Общей формулировки «исследовать DPO/kNN/LoRA/graph» теперь недостаточно. Ниже — принятые как **атрибутированные результаты** конкретные находки. Они сокращают следующую работу, но не превращают незавершённые исследования в DONE.

| Вход | Наблюдаемый статус/охват | Смысл, который должен перейти в последующие задания |
|---|---|---|
| [R01](results/R01/FINDINGS.md), F01–F13 | IN_PROGRESS; recursive inventory не закрыт | Сохраняются исходные prompts, все slices, WorkLeft families и дополнительные roots. Полный обход не заменяется S04 или списком уже прочитанных файлов. |
| [R02](results/R02/FINDINGS.md), review и §1 | Восстановленные требования/52 existing IDs; не full audit | Chinese model пишет final chosen/rejected; CODEX выбирает урок и оценивает exact pair. Cold-start margin — отдельный конфликт, не новый вопрос авторства. |
| [R03](results/R03/FINDINGS.md), CP08 | SOURCE_TRAVERSAL_NOT_COMPLETE | Сохранить four roles, semantic order, unit recipient, BM25+kNN/arbiter, raw-word→TaskSignal/LoRA seam и конкретный helper-ban conflict. |
| [R04](results/R04/FINDINGS.md), F01–F10/G01 | Предметный результат с ограниченными передачами | Разные vector spaces; reusable typed kNN; OutputRefs dereference ещё не доказан; Zip/association и semantic evaluator требуют адресной проверки. |
| [R05](results/R05/FINDINGS.md), CP1/F01–F08 | IN_PROGRESS | Trainer/backward/AdamW уже есть. Остаток — exact recipient, freeze/masks/reference, full state и update→publication→ordinary consumer; не второй trainer. |
| [R06](results/R06/FINDINGS.md), checkpoint B/F01–F14 | RESEARCH_DELIVERED_WITH_BOUNDED_HANDOFF; не product acceptance | Есть search/registry/L2/L3 seams. Конкретный L5 lifecycle NOT_ACTIVATED; top-k возвращает first-hit output; final learned authority нельзя доказать именем класса. |

Статусы относятся к прочитанным carriers, а не к предполагаемому состоянию работающего агента. При новом запуске проверить относящиеся blobs/sections и принять позднюю дельту, не перечитывая весь проект. **Эта таблица не подписывает общий join и не изменяет results/R01–R06.**

Прямо перепроверены в этой редакции на core `codex/dpo-implementation`: `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:115–275`, blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, и `11.DPO/DpoTrainer.cs:380–465`, blob `9c358978075db8c22fe0125eaea9e89e831d5635`. Остальные кодовые выводы здесь атрибутированы R04–R06, не заявлены новым полным code audit.

## 2. Четыре роли: способности, изменение и потребитель

Первичный источник — [original task](../original%20task.md), раздел «Роли каждого блока»; совместить с actual `00.Core/LogicalContracts.cs`, `LearningMachineContracts.cs`, `DynamicGraphContracts.cs`. Имена пользователя обозначают роли; не создавать новый API из-за написания Ilearnyng/Iquearable.

| Роль | Что она обязана делать | Что остаётся детерминированным | Что действительно может учиться | Как отличить пользу от декорации |
|---|---|---|---|---|
| **ILearningUnit** | Принять domain, строение input vectors/output и дать допустимые typed many→many results с associations | Types, schema/version, допустимость преобразования, сохранение input/output identity | Представление/проекция, ранжирование или преобразование содержимого — только у названного owner | На новых families меняется качество требуемого output; одинаковый shape и иной hash недостаточны |
| **ILearningFlow** | Корректировать отношения и преобразования между Units | Совместимость портов, обязательные dependencies, проверяемые ограничения | Выбор связи/коррекции/композиции при неизменных возможностях Units | Frozen Units + changed Flow; отдельно frozen Flow + changed Unit; наблюдать конечный результат, не только route score |
| **ILearningOrchestrator** | Асинхронно исследовать допустимые комбинации одного input и получить output | Cancellation, limits, publication/state identity, обязательный partial order | Выбор ветвей, их продолжения/остановки и объединения по качеству/цене | Одинаковый budget и задания; считать все ветви, а не только winning branch; fastest не автоматически best |
| **ILearningDynamicGraph** | Учить отношения паттернов разных порядков и применять их к новым compositions | Registry, ID/version consistency, допустимые graph operations и явные законы | Higher-order relationships и authority выбора при изменяемом состоянии | Holdout по структуре композиции, а не только по новым словам; graph size и наличие BFS не learning gain |

Роли не требуют линейного прохождения всех четырёх уровней каждым запросом. Train остаётся у **ILearningMachine**, Graph — L5 specialization, не шестой уровень и не четыре trainer. Сохраняются TensorCore, shared base/project adapters, Fabric/provider boundary, StackDetector-before-normalization, semantic compression и память positive/negative/warning. Изменение сохранённого решения требует предметной migration decision, а не молчаливого удаления.

### 2.1. Обязательная карточка каждого изменяемого алгоритма

Карточка не новый ledger: включить в FINDINGS/DECISIONS соответствующего Rxx.

`requirement/source → current producer → input semantics/version → deterministic operation → learned owner/state → objective/signal → update boundary → actual consumer → output semantics/version → comparator → controlled intervention → useful observation/falsifier → full cost → remaining uncertainty`.

Для connection дополнительно: source/target port, association key, cardinality, semantic order, failure/empty/abstention semantics, кому адресуется feedback и кто владеет параметрами. **ID выигравшего solver не является автоматически ID обучаемого selector.**

## 3. Ближайшие разрывы: кому и что именно дочитать

Это ограниченные вопросы по найденным seams, не утверждения об отсутствии реализации.

| Вопрос | Установленный вход | Один основной владелец | Необходимый выход |
|---|---|---|---|
| Pair→recipient→parameter group | R05-F05: GraphDpoPair, lower item, TeachingVector/Context, admission passport | R05; R07 получает только unresolved inter-role seam | Показать envelope/caller до конкретного Machine-owned состояния; MISSING_IMPLEMENTATION отдельно от TRACE_NOT_FINISHED |
| Accepted checkpoint→trusted preload→обычный запрос | R06-F08: runtime/resolver/pipeline/L2 call; R05 endpoint | R05 — checkpoint/publication; R07 — host handoff | Совпадающие state identity/version у update и ordinary consumer либо первый точный разрыв |
| L2 decision→L3 flow | R06-F08: `DynamicGraphRegistryL3SelectedTargetExecutor` | R07 | Фактическая передача выбранного target и параметров, не наличие двух отдельных ExecuteAsync |
| Projection→OutputRefs→typed payload | R04-G01/F06–F08 | R07 | Dereference, type/version, association и законное 1→0; не дублировать Graph API |
| Числа оценки→смысл задачи | R04-F09/F10 | R13, исходные domain laws от R08 | Источник labels/rubric/consumer; character score и numeric meter не назначать общей semantic accuracy |
| Dynamic k→решение | R06-F07, напрямую подтверждённый KnnLogicalUnit | R06→R12 | Что меняет k: evidence, ANN recall, aggregation или итоговый выбор; отдельно стоимость |
| Helper ban→hybrid authority | R03-F20/R06-F14 и DECISIONS-LOCKED D10/D15 | R12→R16/R20 | Область каждой нормы, допустимый helper, learned final owner, exact active consumers для миграции |

R07 не получает «закрыть все остатки R01–R06». Его максимум по первому маршруту — representation/payload association и L2/L3/ordinary-consumer handoff; deep trainer принадлежит R05, широкий source traversal — R01/R03, corpus/законы — R08. Если первый выбранный маршрут не использует некоторый seam, этот seam остаётся отдельным обязательством с owner, а не блокирует ранний урок.

## 4. DPO: полезная пара, правильный сигнал, полезное изменение

### 4.1. Пара строится по смысловому различию

Сохраняется текущий [S04 RUN-CONTRACT](../Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md) §§2.1–2.3: китайский автор обеих окончательных сторон; CODEX/Astra в запрошенной конфигурации выбирает разрешённую задачу и отдельно оценивает exact pair; Student обучается. Raw Student answer может помогать автору, но не обязан быть final rejected. Actual provider/model/version/call и права проверяются перед реальным использованием; исторический shortlist не допуск сегодня.

Для одной пары фиксировать **одну задачу и одинаковый разрешённый context**; subject/intent/language/stack/schema; final chosen/rejected; применимый закон/ограничение; в чём содержательный контраст; recipient/learning objective; provenance; source-family identity; assessment identity и связь с точными bytes. Пара с изменёнными сторонами не наследует старый assessment.

Предпочтительны близкие допустимые по форме варианты, различающиеся существенным свойством. Не создавать negative из бессмыслицы, другой задачи, запрещённого материала или просто «ответа детерминированного алгоритма». Когда оба решения корректны, стоимость может определять preference только при сопоставимом качестве и заранее заданной задаче оптимизации. TIE/BOTH_BAD/UNJUDGEABLE не превращаются в ложный chosen/rejected.

**Иллюстрация SQL, не training data:** запрос требует сохранить все записи левой таблицы, включая записи без соответствия. Chosen объясняет сохранение таких строк; rejected выглядит правдоподобно, но утверждает, что остаются только совпавшие. Необходимы actual dialect/query/schema и независимая проверка применимости закона; сама иллюстрация не публикуется как допущенная пара.

**Иллюстрация Flow:** при одинаковых Units chosen сохраняет dependency «проверить условие до необратимого действия», rejected переставляет операции. Recipient — выбирающий эту связь компонент, а не автоматически исполнитель последней операции. Семантический порядок нельзя сортировать как набор ID.

### 4.2. Objective не равен оценке эксперта

Для probability-based DPO:

`m = beta * [(log pi(chosen|x) - log ref(chosen|x)) - (log pi(rejected|x) - log ref(rejected|x))]`

`L = -log sigmoid(m)`.

При `pi=ref` имеем `m=0`, но `dL/dm=-1/2`. Это контрпример универсальному правилу «нулевой margin запрещает первый урок», а не гарантия ненулевого изменения каждого параметра. Masks, одинаковые стороны, Jacobian, frozen parameters и неверная адресация могут дать no-op. В прочитанном `DpoTrainer.Step` есть backward и optimizer; запрет по positive pre-update margin в этом фрагменте не найден. Верхний admission остаётся адресным вопросом R05, не объявленным runtime bug.

Primary theory: [Rafailov et al., DPO](https://arxiv.org/abs/2305.18290). Формула здесь также сопоставлена с actual Step. Vector distance, judge score или ranking loss не называть standard DPO без соответствующей вероятностной постановки. Для non-differentiable route назвать отдельный selector/objective и механизм credit assignment; выбор пары не доказывает достижимость градиента до него.

### 4.3. Реальный lesson и bulk

Последовательность: допустимый новый материал → exact assessed pair → корректный recipient/admission → persisted baseline/full-state boundary → один разрешённый update → сохранённый кандидат → тот же ordinary consumer → независимое сопоставление → accept/rollback/inconclusive по принятому контракту.

One-pair/no-repeat сохраняется. Bulk — окно наблюдений по **разным новым урокам**, а не mini-batch/replay. У пары сохраняется semantic family, не только новый UUID. Нельзя обучить несколько ablation branches повторно на той же паре и назвать это исследованием без отдельного разрешённого изменения no-repeat policy. Parent/candidate можно сравнивать inference-only; matching memory/context у no-update контроля не требует повторного градиента.

Сначала установить нет ли pending/awaiting-evaluation уже применённого урока. При неизвестных effects сначала recovery, не ещё один optimizer step. Rollback включает применимые weights/adapters/optimizer/scheduler/RNG/counters, accepted/pending identities и representation/index/graph compatibility; раскрытые данные и потраченные деньги rollback не отменяет.

## 5. Domain mapping, токены и dynamic kNN

### 5.1. Карта домена — не один enum

Разделять subject domain(s), intent, source/target language, stack/dialect/version, project/tenant scope, input/output schema и composition level. SQL×BUG — не две взаимоисключающие метки. Mixed/unknown/empty/shifted domain не исключать из examination.

Domain mapping измеряется отдельно от solver. Предложенные метрики: ошибки каждой оси, macro quality по subjects, multi-label precision/recall где уместно, unknown-detection и calibration/abstention. **Это проект метрик, не измеренные значения.** Oracle-domain нужен только как диагностический контроль на оценке: не передавать эталонные labels в обучение или обычный router.

### 5.2. Представление проверяется по сохранённому смыслу

R04 различил raw/canonical/TeachingVector и persisted BPE/kNN. Нельзя объявить все эти каналы одним пространством. Локальные root coordinates TeachingVector не доказывают дефект другого persisted BPE. TeachingVector в R05-StepVector масштабирует сигнал; это не автоматически target output embedding.

У каждого представления нужны version и meaning of coordinates: tokenizer/codebook, vocabulary, parser/features, normalization, projection и output decoder/typed consumer. Одинаковая длина не совместимость. Если меняется геометрия, назвать migration/reindex/cost и способ обращения со старыми snapshots до внедрения.

Контрасты: отрицание/числа/единицы/референты для языков; binding/types/nullability для C#; relations/lifecycle для Angular; cardinality/NULL/dialect для SQL; hierarchy/layout/units/breakpoints для дизайна; prerequisites/ports/semantic order для orchestration. Эквивалентное перефразирование должно сохранять смысл; minimal near-miss должен менять именно значимое свойство. Зависимость от конкретной официальной версии закона/спецификации устанавливает R08, не угадывается по примеру.

### 5.3. Семь отдельных решений вместо «добавить умный kNN»

1. Определить допустимый pool по scope/правам/schema; domain hint boost не фильтр доступа.
2. Получить law-sensitive features из объявленного представления.
3. Выбрать distance/score и нормировки.
4. Найти кандидатов; exact reference и ANN approximation различаются.
5. Выбрать k/radius/abstention с учётом density/uncertainty/budget.
6. Определить **consumer соседей**: top-1, aggregation, reranker, memory context или proposals.
7. Применить final authority/arbiter; positive/negative/warning память помогает, не отменяет отказ.

Исследовательское семейство, **не выбранный optimum**: `d(q,z|domain)=sum_j w_j(domain,q)*delta_j(q,z)` при явно заданных масштабах/ограничениях. Query-dependent weights не обязаны задавать глобальную metric, пригодную для прежнего HNSW. Проверить approximation на том же допустимом pool. Существующий BruteForceIndex — cosine reference, не готовый exact backend произвольного custom score; custom-score scan при необходимости становится именованным узким seam, а не скрытой предпосылкой.

Текущий KnnLogicalUnit использует `hits[0]` и возвращает `bestExample.Output`; остальные hits дают evidence IDs. Поэтому для frozen ordered hits изменение k не меняет top-1 rule. При ANN k может косвенно изменить первый hit; это эффект поиска, не доказанное обучение агрегации. Не обещать gain от dynamic k до указания механизма потребления.

## 6. LoRA: что именно меняется и к чему подключено

LoRA — low-rank изменение конкретной матрицы, не tokenizer, codebook или автоматический decoder смыслового vector token. Первичная опора: [Hu et al., LoRA](https://arxiv.org/abs/2106.09685); actual XIIGEN `10.LoRA/LoraLinear.cs` исследует R05 с callers/freeze/parameter groups. В исходном методе база frozen, но наличие класса в проекте само по себе не доказывает, что runtime действительно её замораживает.

Для выбранной функции указать W/A/B/scale/rank, shapes, input coordinate meaning, изменяемые параметры, freeze/masks, objective, output coordinate meaning, actual consumer и полный state lifecycle. Если цель — semantic projection, выбрать соответствующее supervised/contrastive/ranking objective либо обоснованную probability-based постановку; DPO поверх текстов не доказывает обучение всех vector spaces.

Examination: fixed encoder/codebook/pool + parent/no-adapter/static/adapted state где состояния реально доступны; сравнить смысл декодированного/typed результата, retrieval/route utility, retention и стоимость. Смена encoder одновременно с LoRA не позволяет приписать эффект адаптеру. No-adapter — диагностическая ветвь, не разрешение выключить нужные protections.

## 7. Минимальная лестница опытов, а не полный Cartesian sweep

Это design recipes для R13/R18/R22. Они не запускаются этим документом и не становятся новыми 25+7 сессиями. До исполнения каждый заполняется actual inputs/versions/entrypoint/budget/permissions.

| Опыт | Один вопрос и контроль | Основное наблюдение | Что опровергает интерпретацию |
|---|---|---|---|
| **E0: existing ordinary baseline** | Принятый compatible state против его же повторного обычного вызова; никаких новых weights | Реальный путь input→state→typed/semantic output; variablity/cost | Ответ пришёл из demo/fixture, другой checkpoint или скрытый внешний solver |
| **E1: первый настоящий урок** | Parent и candidate после одного нового разрешённого урока; same memory/context/pool/rubric | Domain-specific utility, parent/candidate paired difference, полный cost | Изменился только hash/loss; consumer не читает candidate; gain объясняется памятью |
| **E2: representation и domain** | Сначала frozen solver и law-sensitive representation; отдельно predicted/oracle domain | Meaning preservation, mapping и downstream correctness | Проекция не используется; leakage oracle label; улучшилась лишь длина/shape |
| **E3: retrieval/k/aggregation** | Same representation/pool; exact vs ANN; затем fixed vs proposed score/k с явным consumer | Neighbor relevance отдельно от answer/path utility; recall/cost | k меняет только evidence; новый score несовместим с индексом; pool изменён незаметно |
| **E4: Flow и async Orchestrator** | Frozen Units/changed relation; отдельный обратный контроль; same budgets | Associations, lawful completion, answer quality; цена всех branches | Путь красивее, output хуже; потерян результат; fastest ошибочно объявлен best |
| **E5: higher-order Graph** | Известные primitives, held-out combinations/structures; control без learned higher-order relation | Перенос и полезный path при ограничениях | Запомнены task IDs/композиции; эффект объясняется большим search budget |
| **E6: Builder/feedback/retention/PC** | Новые содержательные пары и feedback после независимой проверки; before-update observation | Pair validity отдельно от Student benefit, retention, incremental latency/footprint | User correction принята за truth; обучение повторяет family; adapter мал, full runtime не помещается |

**Порядок выбора:** E0/E1 по кратчайшему доказанному ordinary route; E2–E6 по обнаруженному ограничению. Не делать E2/E3 обязательными до E1 только из-за их исследовательской привлекательности. Если именно representation/consumer делает E1 недостоверным, исправляется эта конкретная причина. CodeDocumentation и graph route — сравниваемые кандидаты R12, не объявленные здесь реализованными end-to-end.

## 8. Как сопоставлять bulk-to-bulk без подгонки

До каждого опыта: task population/family selection, baseline/reference/candidate identities, representation и pool versions, rubric, primary quality measure, hard failures, practical threshold/retention tolerance, budget и правило решения. Отсутствующие thresholds помечаются **PROPOSED / EXPLORATORY**, а не подбираются после ответов и не разрешают automatic promotion.

Для семейства f на bulk b: `Delta_b,f = Q(candidate_b, same_cases_f) - Q(parent_b, same_cases_f)`. Сначала агрегировать варианты внутри family, затем families; N−1 вариантов одной задачи не N−1 независимых свидетельств. Сохранять per-domain distribution и failures, не только общий average.

Общая diagnostic panel позволяет сравнивать стадии. Дополнительная fresh family panel показывает перенос. Поскольку operational panel используется для решения о модели, она **не untouched certification**. Итоговая закрытая проверка отделена; никакие её targets, объяснения или производные пары не возвращаются в teaching context. Прямую разницу средних разных по сложности bulk не называть improvement.

Uncertainty: R13 выбирает family-level paired interval/resampling или иной подход с обоснованными assumptions. При зависимых sequential lessons это uncertainty по evaluation population, не независимость всех training updates. Заранее выбрать fixed analysis points либо корректную sequential процедуру; многократное подсматривание и остановка на первом красивом результате не confirmatory evidence. При малой выборке результат может быть только exploratory/INCONCLUSIVE.

Отдельные оси: domain mapping; neighbor relevance; semantic answer/task completion; path law violations; pair validity; actual update/ordinary state consumption; retention; p50/p95 latency и wall time; RAM/VRAM/disk. DpoTrainer.Accuracy по pre-update margin и character match перевода не заменяют эти значения.

Вердикты: **IMPROVE**, **REGRESS**, **INCONCLUSIVE**, **INVALID_EVIDENCE**. Без запуска — **NOT_MEASURED**. Улучшение качества ценой недопустимого нарушения нельзя усреднить в PASS. Не обнаруженная разница не доказанное равенство. Отдельный Astra Ultra reviewer оценивает blind/presentation-balanced ответы и научную интерпретацию; его название не заменяет actual invocation и evidence.

## 9. Компактность, лучший путь и непрерывное обучение

«Лучший» означает лучший **среди рассмотренных допустимых alternatives** по заранее определённому quality/cost, а не доказанный глобальный optimum. Предпочтительна quality-constrained cost comparison или Pareto frontier; нельзя незаметно обменять корректность на скорость одним средним score.

Считать: base/runtime, encoder/codebook, adapters, gradients/optimizer/activations, index/graph/memory/cache, snapshots, загрузку/переключение, reindex, failed/cancelled branches, generation/assessment/evaluation. Current-thread allocations не peak process memory. Исторические «2 MB adapter», «20× compression», «+8%» не measurement нынешнего PC. R23 устанавливает фактическое целевое железо и режим; не обещать fit по размеру весов.

Learning cost: время/ресурсы всех допущенных попыток и оценок до содержательного выигрыша, число новых valid families, retained capability и цена следующего ordinary запроса. Если gain не подтверждён, деление cost/gain не выдаётся за полезную эффективность. Retrieval/memory gain полезен, но не переименовывается в weight learning.

Feedback: сначала ответ frozen current version и pre-update observation; затем source/rights/meaning check, ambiguity/disagreement handling, duplicate/family check; затем отдельный разрешённый новый урок в подходящем idle режиме, full-state recovery и retention examination. Не учиться из любого пользовательского утверждения автоматически. Pending lesson не запускается второй раз. Personal/global scope, leakage и version compatibility сохраняются.

## 10. Применение к 25 поручениям и граница чтения

Полные существующие Rxx сохраняются. [ROUND-DELTA-RESULTS6](ROUND-DELTA-RESULTS6.md) даёт **только свою адресную добавку** каждому Rxx: новые входы, точное поручение, examples/edge, документную приёмку и будущую examination. Dispatcher вставляет её смысл в одно effective поручение по START-NEXT-RUN, без загрузки всех25 и обеих книг целиком. Никакой второй очереди не создаётся.

Прямо прочитаны при этой подготовке: original task начало; whole conditional slice queue; WorkLeft RAG usage начало; SOURCE-ROUTES и assigned families; Continue RAG-06 начало и его primary DECISIONS-LOCKED; previous rounds начало; относящиеся published R01–R06 sections; действующие entrypoints; два названных core excerpts. Это **не полный обход всей библиотеки**. Непрочитанные roots/spans сохраняют владельцев R01/R03/R08 и не объявляются исчезнувшими.

Применены фактически прочитанные skill sections core/docs/skills: trainable-architecture-decision (1–110), phase-planning (110–200), plan-review (1–100), code-examination (1–105), prompt-patch-authoring (1–115), human-readable-architecture-audit (1–110). Из них сохранены source/consumer trace, полный model packet, named wrong/correct contrast, candidate-bound review и понятное объяснение. Исторические 30/50 sequential routines/coverage mandates не отменяют текущие 25, R01–R06 parallel и документный scope; сами core skills здесь не изменены.

Подтверждение документа: source→сохраняемое решение→actual/proposed mechanism→различающий опыт→owner. Это не продуктовые tests, не покрытие, не Windows run и не независимое scientific acceptance. Результаты отдельных workers и действующие code/tests/workflows/weights/datasets не изменяются.
