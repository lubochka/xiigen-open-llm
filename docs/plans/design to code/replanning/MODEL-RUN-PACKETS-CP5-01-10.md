# Подробные задания R01–R10 — INTEGRATED6

**13.10.2026 · редакторский checkpoint 2.** Постоянное имя CP5 сохранено. Эта редакция заменяет CP6/CP6.3 в данном файле; нет действующего ограничения на четыре workers. **По PARALLEL20 R01–R20 могут начинать или продолжать независимо; R01–R09 дают девять исследовательских входов R10/J03.** Это десять подготовленных назначений, не десять выполненных исследований. Вся программа: 10 сбор + 5 анализ + 5 изменения + 5 дополнения.

## Общий блок — включать полностью в каждое выдаваемое поручение

**CURRENT TASK:** выполнить только назначенный Rxx, получить его предметный результат и адресную передачу. **NOT THE TASK:** не выполнить весь проект за один проход, не переписать весь план вместо исследования, не запускать product tests/coverage/Windows/Actions/build/trainer/paid generation. Писать по-русски: что система должна уметь, какой механизм это обеспечивает, что установлено, где разрыв, какое наблюдение отличит улучшение от ошибки. Различать FOUND / PRIMARY_READ / CODE_TRACED / OBSERVED. Без опыта efficacy=NOT_MEASURED. Подготовленные примеры не admitted training material.

**Контекст и namespaces:** P=`lubochka/xiigen-planning:llm_plan3`; D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. Skills=K/docs/skills. Own results=D/replanning/results/Rxx. Фиксировать repo/ref/path/blob/section/read span. Default-branch search не доказывает состояние нужной ветки; старые offsets разрешать по своему blob/именованному разделу.

**Область:** original/task/goal prompts, WorkLeft/LeftToWork, current/nested RAG MD/NDJSON, S00–S10 и все найденные дополнительные этапы/обязательства. Три delivery линии: DPO/DynamicGraph; Design-to-Code; IDE. Семь baseline предметов: EN↔RU, orchestration, Figma→HTML/CSS, C#/Angular/SQL documentation, Builder для этих шести. HE↔EN, code/intents/stored procedures и другие найденные требования не удалять. DTC сохраняет tree/HTML/CSS/images→API→processing→несколько разрешений→quality/user-tier cost→DB→пары→обучение→publication→ordinary consumer. Deferred requirement получает owner/trigger, не исчезает.

**Четыре роли:** ILearningUnit — domain/input-vector/output schemas, typed many→many с associations; ILearningFlow — отношения Units и коррекция input→output; ILearningOrchestrator — асинхронные комбинации/выбор путей; ILearningDynamicGraph — отношения паттернов разных порядков. Сопоставлять с ILogical*/ILearningMachine по поведению. Train на Machine; Graph=L5 specialization, не шестой уровень/четыре trainer. Сохранять TensorCore/shared base/project adapters, StackDetector-before-normalization, semantic compression, BM25+kNN, positive/negative/warning memory, final arbiter и ordinary consumer до адресно разрешённой миграции. Детерминированное вычисление обученной функции не опровергает learning; правильный deterministic output не negative по происхождению.

**DPO policy:** current S04 RUN-CONTRACT §§2.1–2.3: внешняя открытая китайская модель пишет ОБЕ окончательные стороны DPO и не выполняет человеческий выбор/сертификацию; **CODEX GPT-6 Astra UltraCode** выполняет роль человека — выбирает chosen/rejected (или TIE/BOTH_BAD/UNJUDGEABLE) и отдельным шагом оценивает semantic quality/admissibility exact pair. Weak Student output — разрешённый context, не обязательный final rejected. Exact bytes/context/recipient/provenance/assessment фиксируются; изменённая пара не наследует старую оценку. Sealed, semantic-quality-accepted, optimizer-eligible, updated и useful — разные состояния. TIE/BOTH_BAD/UNJUDGEABLE не превращать в фиктивный chosen. Авторство решено; recipient/objective/margin — отдельные вопросы. One-pair/no-repeat/full-state accept-or-rollback остаются. Bulk — окно разных новых уроков, не replay70/30/minibatch promotion. Закрытые/платные экзаменационные ответы не передавать author/selector через посредника. Исторические authors не relabel; rejection не создаёт новый retry permit.

**Skills — загрузить относящиеся sections, не только перечислить названия:** P=phase-planning-SKILL.md + plan-review-SKILL.md; S=reference-inventory-and-source-map-SKILL.md + authority-requirement-binding-SKILL.md при разрешении нормы; A=trainable-architecture-decision-SKILL.md + human-readable-architecture-audit-SKILL.md; C=code-examination-SKILL.md + generated-code-review-SKILL.md; E=context-package-design-SKILL.md + prompt-patch-authoring-SKILL.md; M=experiment-design-SKILL.md + improvement-measurement-SKILL.md. Добавить actual startup/architect-behavior/output-readability sections по задаче. Применять source trace, архитектурные альтернативы, complete work order, counterexamples, human explanation. Старые 30/50-pass/nonparallel процедуры не отменяют текущие 25 и параллельные R01–R06. Исторический skill для иных продуктов с требованием full tests не становится learning-quality gate этой задачи.

**Параллельность и права:** каждый R01–R20 читает current code/direct primary сам; sibling checkpoint — полезная versioned delta, не prerequisite DONE. Сохранять собственный snapshot, visited set, source IDs и остаток; чужой commit журнала не повод перечитывать всё. Чужой непоявившийся carrier=RESULT_NOT_OBSERVED после проверки tree/alias, не NOT_STARTED/FAILED. Результат прочитанного checkpoint не повышать до полного завершения worker. R07–R09 используют достаточные subsets; J03 входит в R10 и соединяет шесть snapshots плюс R07–R09. Нет нового обязательного J06/R26.

**Публикация:** R01–R15 пишут только свои пять carriers по COMMON-CONTRACT: SOURCES.csv, FINDINGS.md, DECISIONS.md, CHANGE-MAP.csv, RUN-LOG.md. Не создавать пять файлов на каждую часть A/B. Не менять sibling/shared results/live plan из research worker. Fresh bytes/SHA перед write; при конфликте reconcile без потери; remote readback после commit; без force-push. GitHub documentary writes разрешены, это не изменение C:. Core/code/tests/workflows/weights/datasets/installed overlays/core skills не менять; не отключать существующие проверки, не выполнять cleanup/install.

**Нагрузка:** A и B — части одного назначения с полезным checkpoint A, не независимые reviewers. Общий механизм имеет одного primary owner, другие читают свои стыки. Большой WorkLeft делить по question/section/span; generated corpora не обходить построчно ради счётчика. Реальный child получает ПОЛНЫЙ common, собственный task/non-task, sources/actual skills, действия, output, good/bad/edge, examination, limits и return-only rights. Нельзя делегировать только фразу «проверь алгоритм». Дополнительный фактический call явно учитывается. При нехватке контекста опубликовать coherent subset и точный остаток с owner, не начинать общий аудит заново.

**Приёмка:** source→исходное expectation→actual mechanism/gap→различающее наблюдение→downstream owner. Передача содержит origin=(run,local ID,checkpoint), primary span, смысл/границу, recipient+action. Получатель записывает в своём carrier semantic receipt: куда применён смысл либо почему не принят. SRC-R01…SRC-R06, HYBRID:Hxx и RECOVERED-CONTRACT:Axx — разные namespaces. Self-check не независимая Astra review. Непроведённый опыт не мешает закончить исследовательскую карточку; неизвестный critical contract ограничивает зависимое решение. Число tests/пар/files и changed hash не learning gain.

**Контрольные наблюдения:** mapping отдельно от retrieval и solver; exact/ANN отдельно от consumer использования соседей; memory-only отдельно от parameter update; representation отдельно от LoRA signal weighting; Unit отдельно от Flow/async/Graph. Сравнения задают frozen factors, одну intervention, lawful near-miss, independent source families, semantic metric и full cost. Отрицательный или неопределённый результат опыта не исправляется подгонкой рубрики. Исторические 95%/20×/2MB/+8% не сегодняшние measurements.

## R01 — карта всей программы и нагрузка источников

**Роль/skills:** инвентаризатор/планировщик, S/P/E. **Прямой context/RAG:** own carriers; SOURCE-ROUTES; actual P/K trees; WorkLeft INDEX-01/REDATE-01/MASTER/usage; original/task/goal prompts; slice queue. R02–R06 — advisory. **Не задача:** изучить все алгоритмы по именам или свести проект к S04.

**Prompt A.** Продолжи рекурсивную карту от сохранённого visited set. Найди RAG/local-RAG/MD/NDJSON, WorkLeft/LeftToWork, nested roots, исходные prompts, S00–S10/extras и связанные библиотеки. Для source зафиксируй path/blob/role/current-or-history/primary entrypoint/actual reading depth. Различай navigation RAG, runtime vector index и skills. Проверяй aliases/переносы; пустой ответ reader не доказывает отсутствия файла. Сохрани source IDs. Выход: карта найденного и действительно прочитанного с явными хвостами.

**Prompt B.** Распредели конкретные вопросы: R02 требования, R03 primary lineage, R04 representation, R05 DPO/update/state, R06 domain/kNN/graph, R07 connections, R08 laws/data, R09 process. У смешанного файла один inventory owner и разные spans. Требования вне slices/семи предметов сохраняются. Новые routes от R04–R06 включай без повторения их code traces. Выход: source→question→owner→bounded input→next hop; capacity и источник каждого дополнения.

**Хорошо:** большой code-changes поделен по смысловым разделам; R05 начинает direct trace независимо. **Плохо:** filename=READ; весь unread хвост назначен R10. **Край:** разные roots с одинаковым basename, два пробела, cycles, архив как единственный носитель, дополнительный язык.

**Проверка результата:** сопоставить inventory с полученным tree, пометить truncation/pagination, каждому обязательству найти owner. Полный tree не полное чтение. **Передача:** R02–R10; runtime efficacy N/A/NOT_MEASURED.

## R02 — исходные ожидания и сохранённые решения

**Роль/skills:** requirements architect, S/P/A/E. **Context:** own existing requirement IDs; D/original task.md, task prompt.md/.txt, goal of the round.md, Алгоритм создания ДПО.md; WorkLeft owner decisions/RESEARCH-02; S04 RUN §§2.1–2.3. **Не задача:** новые дубли IDs или повторное решение Chinese authorship.

**Prompt A.** Сопоставь каждое незакрытое ожидание с первичными словами владельца, областью, текущим consumer и статусом: действует/явно заменено/историческое/конфликт/unknown. Сохрани четыре роли, Machine owner, many→many/async/Graph, shared/project representation, все delivery линии. Не выдавай ранее прочитанное за новое открытие. Выход: изменённые existing requirement rows с source provenance.

**Prompt B.** Закрепи final chosen/rejected by Chinese author, CODEX selection/exact assessment, Student context-only. Раздели реальные конфликты: pre-update margin, recipient versus winning solver, unit-level canon versus higher-order learning, replay versus no-repeat, representation transport. Дай обе exact source стороны и конкретного R12/R16 consumer. Рассмотри два равноценных lawful решения: preference не обязано существовать. Выход: resolved decisions и адресные unresolved questions.

**Хорошо:** старый ID сохранён; R12 получает margin conflict, не выбор автора. **Плохо:** historical pair relabel или correct fixed solution назначен rejected. **Край:** несовместимые версии нормы, цитата старой команды, subject/intent смешаны, semantic order потерян.

**Проверка:** requirement→первичный смысл→consumer→будущая предметная examination прослеживается; неустановленная authority не скрыта. **Передача:** R05/R08/R12/R16/R21; empirical results не заявлять.

## R03 — RAG до первичных решений, не ещё один список файлов

**Роль/skills:** исследователь первоисточников, S/A/P. **Context:** own CP08/actual next checkpoint и visited set; WorkLeft RAG-MASTER/usage, RAG01–04 MD/NDJSON, DELTAS, TASK-PROMPTS-CHRONICLE/COVERAGE; SOURCE-ROUTES. **Прямой маршрут:** D/WorkLeft-RAG04→V/RAG-06-llm-dynamic-flow.md→F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md и DECISIONS-LOCKED.md; DPO→G; DTC→original/task prompts. **Не задача:** сначала завершить весь адресный аудит и только затем читать смысл.

**Prompt A.** Продолжи непрочитанные primary transitions: роли/эволюция контрактов, semantic order, domain scoping, positive/negative/warning memory, final arbiter, TensorCore/LoRA/semantic compression, reasoning node/skill selection/query path. Сохраняй F01–F23/REQ IDs и историю CP07/CP08. Для каждого critical вывода прочти primary span; RAG summary не заменяет текст. Раздели source date, scope и actual code evidence; старое CODE-ABSENT не состояние сегодня. Выход: visited graph с содержательными решениями и reading depth.

**Prompt B.** Передай unique решения с точными consumers R04–R08/R12. Chinese authorship не переоткрывать. Historical helper-ban сопоставить с разрешённым hybrid; historical replay не активировать. Candidate presentation permutation не semantic step permutation. Определи чёткий остаток каждого маршрута, включая недоступный source, без фиктивной полноты. Выход: смысловые receipts и conflict pairs.

**Хорошо:** negative memory хранит причину отказа, но similarity не отменяет arbiter. **Плохо:** пять цитат одного primary=пять независимых доказательств; correct deterministic ответ negative по происхождению. **Край:** cycles, moved primary, nested generated corpus, outdated status overlay, два namespaces H03.

**Проверка:** каждый принятый critical вывод имеет primary transition; unread tail не скрыт и не блокирует все независимые работы. **Передача:** R04 coordinates, R05 objective/recipient, R06 authority, R08 laws, R12 conflicts.

## R04 — представление и typed many→many до настоящего consumer

**Роль/skills:** representation architect/code examiner, A/C/P/M. **Context:** own latest results; D/original task.md, S00–S03; K/00.Core/LogicalContracts.cs, LearningMachineContracts.cs; actual-tree representation/consumer; WorkLeft 01-code-changes, 05-branch-reconciliation, SOLUTIONS-01, VERIFY-01/02 — только свои spans. **Не задача:** ждать R01–R03 или переписать tokenizer с нуля.

**Prompt A.** Продолжи actual ordinary input→StackDetector→normalization/parser/tokenizer/features→projection→LogicalUnitRequest→typed result/OutputRefs→materialization→consumer. Запиши input/output meaning, cardinality, associations, one→zero, several lawful outputs, lazy/eager/query execution и cancellation. Слово IQueryable не доказательство готового LINQ provider. Raw text, canonical text, TaskSignal, TeachingVector, tensor и output refs не одно пространство. Укажи schema/tokenizer/codebook/projection versions и actual methods/callers.

**Prompt B.** Прими R04-F01–F04 и receipt R05-F03 без переоткрытия всего исследования. В normalized channel могут теряться negation/numbers/RU/HE; raw сохранён и потому end-to-end failure не доказан. Локальный root vocab TeachingVector не приписывай retained BPE kNN. Проверь existing sharedRootVocab overload before new design. Подготовь E1 из ALGORITHM-EXPERIMENT-HANDOFF: одна representation change, frozen solver/pool/memory, compatible stored vectors, semantic outcome и reindex cost. Выход: channel/coordinate/association map и один smallest seam.

**Хорошо:** ≤ и > доходят как разные relations к своим output IDs. **Плохо:** same shape=semantic compatibility; hash change=understanding. **Край:** mixed language/stack, SQL NULL, schema unknown, many→many ambiguity, stale coordinates, missing predicates metadata.

**Проверка:** trace показывает реальный consumer каждого канала; E1 ловит конкретную потерю смысла, не только shape. **Передача:** R05 state/projection, R06 geometry/index, R07 associations, R12 choice, R13/E1. Уже опубликованный R04 не reset; учёба не выполняется.

## R05 — правильная DPO-пара, адресат, существующий update и обычный ответ

**Роль/skills:** learning architect/code examiner, A/C/P/M/E. **Прямой context:** own CP1/next checkpoint; S04–S06; G/active-codex-execution-plan-07-dpo-builder.md §§3.1–3.2a; current S04 RUN §§2.1–2.3; WorkLeft 02-teaching-chinese-model, INTEGRATION-01, TASK-PROMPTS-TRAINING, RESEARCH-03; actual K LearningMachineContracts, DpoPair/DpoDataset/DpoTrainerPairAdmission, BoundedDpoTrainingContentProjector, DpoTrainer, 10.LoRA/LoraLinear. R04/R06 helpful deltas, не prerequisites. Подробный старый run-prompts/R05-PARALLEL6 сохраняет unique source details, но второй общий блок не добавляется.

**Prompt A.** Продолжи только remaining trace: selection→Chinese final pair→exact CODEX assessment→14-field GraphDpoPair→lower DTO/envelope→recipient→admission→policy/reference objective→optimizer/LoRA parameter group→full state→publication/load→ordinary consumer. Четыре поля lower DTO не доказывают потерю адреса без caller/envelope. Training recipient не winning solver. Semantic OrderedPatternIds не сортировать ради hash. Для каждого перехода actual method/source либо precise gap.

**Prompt B.** Сохрани R05-F01–F08: существующий DpoTrainer уже делает backward/AdamW; pre-step Accuracy не after-quality; StepVector weighting не target-vector training; LoraLinear не tokenizer. Найди actual LOW_CONTRAST/margin consumer либо явно plan-only status. Для standard DPO зафиксируй action/response space, common context, policy/reference identity, masks, serialization/length, beta и reference refresh. При policy=reference m=0 не semantic rejection; zero Jacobian/masks проверяются отдельно. Иную pairwise/vector objective назвать её именем. Дай E3 и один полностью заданный first-missing-seam packet, а не новый trainer.

**Хорошо:** routing pair адресует selector и его actual state. **Плохо:** saved adapter не потребляется, но объявлено learning; изменение всех LoRA matrices обязательно на каждом шаге; author policy решается снова. **Край:** TIE/BOTH_BAD, stale exact assessment, identical sides, frozen/all-masked path, interrupted step/publication, incompatible reference/tokenizer, duplicate family under new ID.

**Проверка:** pair→gradient/state→ordinary consumer или точный недостающий стык; не переименовывать plan contradiction в доказанный runtime bug. **Передача:** R04/R06 changed-state versions; R07 connections; R12 objective/owner; R13/E3; R18 minimal repair. No-update control получает ту же память/контекст; efficacy NOT_MEASURED.

## R06 — domain mapping, dynamic kNN, память и learned path authority

**Роль/skills:** retrieval/routing architect, A/C/P/M. **Прямой context:** own checkpoint B/actual successor; S07–S10 и related earlier scopes; WorkLeft DESIGN-ACTIONS/P0–P6/PT; V/RAG06→F/authority amendment/DECISIONS; actual K/06.VectorSearch, 08.GraphRag, 15.OrchestratorModel, 17.DynamicDomains, 26.TrainableRouter, DynamicGraph/edge-authority, KnnLogicalUnit. R04/R05 не барьер. Old R06-PARALLEL6 — источник уникальных деталей, не второй общий packet.

**Prompt A.** Продолжи остаток R06-F01–F14, не перепиши аудит. Раздели subject/intent/stack/project→eligible pool→features→metric/score→k/radius→positive/negative/warning→reranker/aggregator→arbiter→async composition→output. Для каждого решения fixed/data-derived/learned authority, state, signal и owner. Keyword formula или имя Hybrid не доказывает learned policy/BM25+kNN. Confidence rule не calibration. Unknown/mixed domain не прятать в один fixed enum.

**Prompt B.** Используй E2/E4: существующий KnnLogicalUnit берёт Output первого hit; увеличение k не доказывает агрегацию. Сначала exact/ANN при той же geometry, затем fixed candidates/changed consumer, затем adaptive k как отдельный фактор. Existing exact index — in-memory diagnostic, не semantic oracle/persistence bundle. Query-dependent score может нарушать assumptions ANN. Сохрани реальные L2/L3 seams и NOT_ACTIVATED конкретного lifecycle без обобщения «весь graph отсутствует». Дай concrete law-sensitive example и cheapest next contrast, а не Cartesian sweep.

**Хорошо:** high-similarity wrong-dialect neighbor не вытесняет lawful candidate; предупреждение не положительный ответ. **Плохо:** top-k evidence IDs=better reasoning; memory gain назван LoRA gain; весь runtime заказан заново. **Край:** empty/tiny pool, k>count, unknown/mixed domain, stale index, zero norm, conflicting memories, skill outside candidates, expensive failed branch.

**Проверка:** отдельно измеряются mapping, retrieval relevance, consumer quality и full cost; dynamic k имеет signal/owner, не обещание optimum. **Передача:** R07 trained-state→preload и L2→L3 association; R12 helper-ban/margin/consumer choices; R13/E2; R08 domain contrasts; R22 expansion.

## R07 — связи четырёх ролей и credit assignment

**Роль/skills:** integration architect, A/C/P/M. **Context:** достаточные checkpoints R04/R05/R06, R03 semantic-order/recipient, original roles, Logical/DynamicGraph/composition contracts, WorkLeft MECH-01 и RESEARCH-03; ALGORITHM-EXPERIMENT-HANDOFF §1/§2/E4. **Не задача:** повторить весь DPO/kNN/RAG audit или ждать formal DONE всех siblings.

**Prompt A.** Начни с двух unresolved connections R06-F08: trained state→trusted edge-authority preload и L2 decision→L3 association. Проверь их current methods/callers/consumers, не создавай framework. Для одного input покажи producer→semantic port/version→consumer→decision→learning recipient→state owner. Отрази many→many associations, Flow correction, async fan-out/fan-in, semantic order и higher-order pattern. Сопоставь actual TeachingVector channel R04 с actual weighting/consumer R05, не с воображаемым kNN input.

**Prompt B.** Подготовь E4: frozen Units/changed Flow и frozen relations/changed Unit. Укажи fixed representation/pool/memory, changed state, lawful preference/recipient, outcome, all-branch cost и falsifier. Для Graph — unseen composition familiar primitives; для Orchestrator — selection quality отдельно от scheduler arrival. Global reward не копируется всем edges без credit objective. Если canon не выражает нужную способность, exact migration question в R12, не удаление роли. Выход: connection/credit contract, minimal seam и два bounded recipes.

**Хорошо:** Units правильны, но association ошибочна — меняется relation. **Плохо:** все блоки изменены и выигрыш приписан Flow; hash-sort уничтожает dependency order. **Край:** одинаковые types/разный смысл, cycle/timeout, partial fan-in, several lawful paths, same solver from different selectors.

**Проверка:** walkthrough локализует fault; intervention различает как минимум Unit failure и relation failure. **Передача:** R12 selected contracts; R13/E4; R18 reuse seam; R22 higher-order/async. Возможность исследования не зависит от полноты чужого unrelated tail.

## R08 — реальные законы доменов, meaningful negatives и Builder

**Роль/skills:** domain/curriculum researcher, S/A/M/E. **Context:** WorkLeft RESEARCH-01-training-quality-speed-accuracy-program и RESEARCH-01-REVIEW-20-passes; original prompts, DOMAIN-EXPERIMENT-PLAYBOOK/RECIPES, R02 PE catalogue, R04/R06 feature consumers; extensions  - orchestration based on our data/ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md по actual tree. Старые20 passes не повторять. **Не задача:** семь одинаковых text fixtures или admitted dataset.

**Prompt A.** Для семи предметов и extras дай law/source→representation feature→lawful output→semantic metric/oracle→ambiguity/abstention. Перевод: negation/scope/numbers/terms/context; orchestration: prerequisites/typed ports/resources; Figma: hierarchy/geometry/responsiveness; C#: actual code/API/types/exceptions; Angular: bindings/state/lifecycle/version; SQL: dialect/joins/NULL/grouping; Builder: correct recipient и utility урока. Временные/медицинские/юридические нормы требуют current primary. Subject и intent разделить.

**Prompt B.** Каждый passport содержит concrete input, preferred response, правдоподобный near-miss с указанным дефектом, TIE/BOTH_BAD/UNJUDGEABLE пример и unseen-family transfer. Это DOCUMENT_EXAMPLE, не материал с выдуманным Chinese authorship. One-defect contrast отделить от ORCH interaction; PE catalogue mapped, N−1 variants остаются одной family. Отдельно material rights, source split, label leakage, rater disagreement и diagnostics allowed to teacher versus closed exam. Выход: distinct passports и minimum first/second-domain subset.

**Хорошо:** SQL negative неверно описывает NULL, а не просто короче; Builder lesson учит нужного owner. **Плохо:** negative потому что fixed/model label; семь domains отличаются только строкой domain. **Край:** correct alternatives, source law changes, mixed domains, training/eval duplicate family, hidden answer in ID.

**Проверка:** law/feature/consumer/contrast/metric связаны; proposed rubric различает предметную ошибку и не требует exact wording. **Передача:** R13 первый recipe; R21 progression; R22 efficiency; R23 feedback. Полный curriculum не blocker первого урока.

## R09 — реальная цена процесса, learning-first acceptance и гипотеза подгонки проверок

**Роль/skills:** process/code/learning-evaluation analyst, S/P/C/M/E/A. Обязательно загрузить относящиеся sections `reference-inventory-and-source-map-SKILL.md`, `authority-requirement-binding-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md`, `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md`. **Context:** WorkLeft `INTEGRATION-02-test-batching`, `TASK-PROMPTS-TEST-BATCHES`, `DEC-01-smart-app-control-runtime-path`, `DEC-02-architect-adjudication`, `DEC-04-runtime-blocked-executor-playbook`, `MATRIX-01-progress-by-dates`, `REPORT-01-progress`; actual workflow definitions; уже существующие Actions/job/run records; relevant git diffs; current first-lesson path из R04–R08 checkpoints; `previous rounds thinking results.md` и `PARALLEL-R01-R03-JOIN.md` только как versioned evidence. **Не задача:** запускать Windows/CI/coverage, доказывать learning зелёным workflow, заранее подтвердить обвинение в test-fitting, повторить R07/R08 целиком или ждать их DONE.

**Жёсткая role boundary.** Внешняя открытая китайская модель используется **только как author/generator обеих финальных DPO сторон** для допущенного урока. Она не выбирает winner, не выполняет человеческое предпочтение и не сертифицирует качество. **CODEX GPT-6 Astra UltraCode** выполняет роль человека: (1) выбирает, какая сторона chosen/rejected либо ставит TIE/BOTH_BAD/UNJUDGEABLE; (2) отдельно оценивает качество exact pair и admissibility урока; (3) при bulk comparison оценивает semantic quality до/после по заранее frozen rubric. Эти три действия не смешивать. Student output может быть context, но не обязан становиться final rejected. Reference policy не judge.

### Prompt A — восстановить реальную цену процесса и отделить delivery checks от learning evidence

1. Выбери bounded период и правило выборки **до** просмотра желаемого вывода. Для каждого наблюдения фиксируй source/ref/run/commit/временную границу и UNKNOWN там, где данных нет.
2. Раздели как минимум: queue time; job wall time; native/Windows environment failure; Smart App Control/runtime-path failure; повторный workflow без нового learning evidence; governance/review regeneration; implementation algorithm work; DPO authoring; CODEX Astra UltraCode selection; CODEX Astra UltraCode assessment; actual training/update; bulk evaluation; publication/host qualification.
3. Не выводи часы человека из количества commits, не суммируй overlapping jobs как wall-clock, не называй billing известным без source. Parent-SHA, cache-hit, unrelated commit и rerun должны быть отдельными edge cases.
4. Для каждого first-lesson prerequisite дай: functional expectation → actual mechanism/input/output → existing reusable evidence → что ещё действительно блокирует **первый learning experiment** → что относится только к host/release milestone.
5. Построй causal KEEP / REUSE / DEFER / REMOVE-FROM-LEARNING-GATE карту. `DEFER` не означает удалить обязательство: укажи downstream consumer, owner и trigger.
6. Отдельно посчитай **cost-to-learning-signal**: сколько wall time/compute/model calls/PC footprint требуется, чтобы получить один новый admitted DPO lesson, один actual update и один сопоставимый before/after result. Если это не измерено — NOT_MEASURED, а не оценка на глаз.

**Обязательный выход A:** source-bound cost ledger + causal DAG `first useful lesson` versus `full delivery/Windows qualification`; список повторных действий, не создающих нового learning evidence; earliest path к admitted pair→update→ordinary consumer→evaluation без unrelated Windows prerequisite.

### Prompt B — заменить coverage-first приёмку learning-system на functional learning examination

Не используй line/test coverage, количество зелёных тестов или Windows run как доказательство качества обучаемого поведения. Обычные детерминированные tests могут остаться только там, где они защищают механическую invariant (serialization, rights, one-update/no-repeat, schema compatibility, rollback, native integration); **они не являются learning-quality gate**.

Для цепочек requirement→code→failed assertion→diff классифицируй:
- **UNJUSTIFIED_ORACLE_FITTING** — expectation подогнали под текущий ответ без изменения исходного функционального смысла;
- **FIXTURE_OR_TEST_BUG** — проверка не соответствовала действительному контракту;
- **LEGITIMATE_REQUIREMENT_CHANGE** — owner/source действительно изменил функциональный контракт;
- **MECHANISM_INVARIANT_FIX** — детерминированный механизм был неверен;
- **NOT_LEARNING_EVIDENCE** — green/red result ничего не говорит об улучшении learned behavior;
- **INCONCLUSIVE** — evidence недостаточно.

Для каждого learning-related capability построить examination contract, который дальше потребляют R11/R13/R17/R22/R23:

**Domain mapping.** Реальные предметные законы → oracle domain/intent и predicted domain/intent → confusion/error taxonomy → downstream solver/path. Измерять mapping accuracy отдельно от solver quality. Unknown/mixed domain не forced label.

**DPO quality.** Chinese author генерирует обе final sides. CODEX GPT-6 Astra UltraCode до обучения делает exact semantic preference/admissibility review. Сохранять positive, negative/near-miss, TIE/BOTH_BAD/UNJUDGEABLE; correct deterministic answer не становится negative только из-за происхождения. Pair validity отдельно от optimizer utility.

**Dynamic kNN.** Для каждого домена определить lawful feature meanings, normalization, candidate pool, metric/score family, dynamic `k/radius` function и actual neighbor consumer. Сравнивать fixed-neighbor/changed-consumer и changed-metric/fixed-consumer, чтобы не приписывать retrieval gain не тому механизму. Не объявлять одну universal kNN function лучшей заранее.

**Vector tokenization / representation.** Проверять, что domain-law-relevant meaning доходит до vector/token space без silent loss; сравнивать representation versions на одном downstream task/panel. LoRA/TeachingVector weighting не называть tokenizer или semantic projection без actual mapping.

**LoRA / learned state.** Установить adapted matrices/parameter group, update recipient, frozen base, before/after checkpoint identity и ordinary consumer. Проверить, что полученный learned state реально используется inference path. Отдельно memory-only effect versus parameter update.

**Hybrid deterministic + learned path.** Для ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph фиксировать deterministic admissibility/helper, learned authority/state, update signal/recipient и ordinary effect. Измерять не “случайность”, а способность выбрать/скорректировать более точный и компактный путь при одинаковых constraints. Frozen Units/changed Flow и обратный contrast; same candidates/budget для async choice; unseen higher-order composition для Graph.

**Bulk-to-bulk measurement.** Перед bulk frozen: panel, rubric, source-family split, resource budget, mapping metric, semantic accuracy/quality metric, retention set, p50/p95/total latency, RAM/VRAM/disk/compute footprint, stopping rule и tolerated regression. Bulk = последовательность **новых** admitted lessons, не replay. Сравнить incumbent/challenger на одинаковой panel; no-update control получает сопоставимую memory/context/compute. Verdict только IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE.

**Compact-learning question.** R09 обязан передать в R22/R23 способ измерить, даёт ли сочетание deterministic + learned mechanisms более компактное обучение на PC: quality per admitted lesson, quality per update, quality per wall-clock, quality per peak memory/disk и retention after user-feedback continuation. “Меньше кода/тестов” не является compact learning.

### Prompt C — решение по Windows/CI и раннему learning path

1. Найди **конкретную native boundary**, для которой Windows обязателен функционально. Если first lesson её не пересекает, Windows qualification переносится к соответствующему host/release milestone.
2. Не отключай workflow и не объявляй его бесполезным глобально; убери только его статус как prerequisite/evidence для learning gain там, где нет причинной зависимости.
3. Для каждого KEEP/DEFER/REMOVE-FROM-LEARNING-GATE зафиксируй functional expectation, downstream consumer и falsifier.
4. Сформируй минимальный путь, по которому можно как можно раньше начать реальное обучение: admitted Chinese-authored pair → CODEX Astra UltraCode human-choice + assessment → vector/token/objective → one update → ordinary consumer → frozen-panel comparison → accept/rollback/inconclusive.
5. Если path упирается в неизвестный contract из R04–R08, запиши точный missing input и продолжай все независимые части; не превращай sibling DONE в глобальный gate.

**Хороший пример:** Windows workflow нужен для native host packaging, но не участвует в pair→token→update→ordinary inference path. R09 переносит его в host milestone, сохраняет owner/trigger, а learning acceptance заменяет frozen before/after panel с mapping, semantic quality, latency и PC footprint.

**Плохой пример:** “все tests удалить”; “1000 green tests = модель научилась”; “queue duration = CPU”; “Astra сказал хорошо” без frozen rubric/baseline/challenger; Chinese model сама выбирает preferred side; dynamic kNN объявлен лучшим без domain-law experiment; один улучшенный fixture считается доказательством whole-program gain.

**Крайние случаи:** cache; parent SHA run; unrelated commit; missing billing; required serialization/native defect; оба DPO ответа корректны; оба плохи; exact deterministic answer; unknown/mixed domain; ANN меняет neighbors; consumer игнорирует дополнительные neighbors; representation collision/truncation; LoRA update есть, ordinary consumer использует старый checkpoint; user feedback конфликтует с law/source; bulk улучшает среднее, но ломает protected family; PC fit достигается только ценой неприемлемой latency.

**Документная проверка результата сейчас:** каждое число имеет source/boundary; каждый classified test-fitting case имеет requirement/code/assertion/diff/owner; ни один green/red test не назван learning gain; role boundary Chinese-author versus CODEX GPT-6 Astra UltraCode selector/assessor видна явно; каждый перенос сохраняет functional consumer; first-lesson causal path не содержит unrelated Windows/coverage gate; все proposed learning metrics имеют owner R13/R17/R22/R23 и помечены NOT_MEASURED до опыта.

**Future empirical examination — не выполнять внутри R09:** R13/R22 создают frozen recipes для domain mapping, DPO quality, representation, dynamic kNN, LoRA/adoption, Unit↔Flow↔Orchestrator↔Graph contrasts и bulk-to-bulk quality/time/PC. **CODEX GPT-6 Astra UltraCode** выполняет human preference/semantic assessment по frozen rubric; external Chinese open model только authors DPO sides. R23 добавляет PC/feedback/continuation-learning footprint. R17 заменяет coverage-first wording в live acceptance, сохраняя необходимые deterministic mechanism/host checks.

**Передача:** R11 causal DAG; R13 measurement recipe; R17 learning-first acceptance policy; R22 algorithm/connection efficiency experiments; R23 PC/feedback budget. Новых Windows/CI/product tests/training runs внутри R09 нет.

## R10 — join девяти независимых research snapshots, гипотез и остатка

**Роль/skills:** integrator/analyst, S/P/A/M. **Context:** R01–R09 actual checkpoints; PARALLEL-R01-R03-JOIN (историческое имя, девять R01–R09 research inputs; PARALLEL20 разрешает двадцать стартов); WorkLeft DOCDEBT/VERIFY-03..05 только по своему вопросу; ALGORITHM-EXPERIMENT-HANDOFF §1. **Не задача:** full re-audit или подмена чужого IN_PROGRESS своим DONE.

**Prompt A.** Выполни J03 внутри R10: отдельно R01/R02/R03/R04/R05/R06 snapshot/status/ref/read-depth, затем R07–R09. Сохрани previous join, добавь только source/meaning delta. Объедини union уникальных consequential findings, не только пересечение. Обязательные receipts: R04 channel/coordinate→R05/R06/R07/R13; R05 existing trainer/weighting/consumer→R12/R18; R06 first-hit/existing L2-L3/margin/helper-ban→R07/R12/R13/R16. Во всех решениях сохраняются all slices/WorkLeft/3products/7+subjects/four roles. Выход: evidence/decision/consumer map с явными limits.

**Prompt B.** Для bottleneck дай competing explanations/evidence/falsifier: mapping versus solver; approximate retrieval versus consumer; local TeachingVector versus retained BPE; no update versus ineffective pair; memory versus weights; weak Unit versus wrong relation. Дешевейшее различающее наблюдение передай владельцу. First-lesson sufficient subset отделить от whole-program completeness. Непрочитанный хвост имеет точный вопрос/span/owner/capacity; не прячется в R15/R25. Расширение только с full packet и учётом дополнительного call.

**Хорошо:** m=0 — mathematical counterexample, current trainer defect не доказан автоматически. **Плохо:** прежний four-input join применён к шести; один source процитирован шесть раз как независимое подтверждение; новый R26 обязателен по умолчанию. **Край:** carrier отсутствует/alias, concurrent source changes, поздний checkpoint, namespaces clash, source inaccessible, owner overload.

**Проверка:** каждая consequential находка принята конкретным downstream prompt либо имеет объяснённый отказ/остаток; partial join не full verdict. **Передача:** R11–R15 достаточный analysis subset, R18 nearest seam, R21–R24 сохранённая полная область. Join не learning evidence.

## Сохранённые источники и граница этой редакции

Предметные WORK-ORDERS/HYBRID/RECOVERED/ROUND-DELTA/domain books, прежние R05/R06-PARALLEL6 и история доступны; уникальный source detail не отменён сокращением повторяющегося common. Dispatcher переносит только относящуюся уникальную норму в одно поручение. На противоречиях parallel-count/ownership/teacher policy действует текущая редакция и live user authority; не склеивать несовместимые commons.

Изменён только плановый packet book. R01–R06 results, code, tests, datasets и weights не менялись. Действующие findings R04/R05/R06 приняты как атрибутированные исследования, не повторный code audit или измерение. Текущие results/R01–R25 содержат опубликованные документные срезы с неодинаковыми scope/status. Их наличие не доказывает полное исполнение каждого назначения, независимую Astra-экспертизу или измеренное обучение.
