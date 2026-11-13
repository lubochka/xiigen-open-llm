# R08 — предметные законы, гибридные алгоритмы и измерение обучения

**R08-CP4 · 13.10.2026 · полный действующий prompt.** Историческое имя PARALLEL9 сохранено для dispatcher. По последнему прямому поручению владельца **R01–R20 могут начинать одновременно**. Это правило старта, не разрешение считать неподтверждённые решения принятыми. R08 не ждёт общего DONE и не выполняет соседние исследования. Программа остаётся 10 сборов + 5 анализов + 5 изменений + 5 дополнений; A–E ниже — части одного R08, не пять уже вызванных моделей.

## 0. Задание, границы и единица результата

Подготовь **заполненную предметную программу исследования**: какую способность система должна приобрести, какое различие обязана сохранять, что именно изменяется при обучении, кто использует изменённое состояние и какой опыт отличит улучшение от случайного ответа. Результат — паспорта, предметные контрасты, конкретные механизмы и адресные рецепты/остатки. Не возвращай ещё один список «изучить DPO/kNN/LoRA».

P = `lubochka/xiigen-planning:llm_plan3`; D = `llm mvp/design to code session`; K = `lubochka/llm_mvp_core:codex/dpo-implementation`; S = `K/docs/skills`. Planning baseline этого ремонта: `ffad2d223370508b8917b1377a5fbf51b578d1e2`; проверенный core C0: `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Каждый исполнитель закрепляет свои actual refs и relevant blobs. Изменение чужого RUN-LOG не сбрасывает собственное исследование.

**Область:** три направления DPO/DynamicGraph, Design-to-Code и IDE; S00–S10 и обнаруженные extras; семь исходных предметов плюс дополнительные языки/задачи из primary. Полная программа не сводится к S04. Но R08 не повторяет весь аудит R04–R07: он проверяет предметные различия на тех механизмах, которыми обосновывает свои выводы. Общий код читается один раз с несколькими предметными привязками.

**Разрешено:** чтение/поиск, статический разбор primary и actual code, анализ существующих наблюдений, документная проверка; запись только собственных пяти `D/replanning/results/R08/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Child return-only. **Не разрешено:** изменение product code/tests/workflows/weights/datasets/core skills, чужих результатов и live plan без отдельного наряда; Windows/Actions/build/tests/coverage/trainer/teacher/платный judge; установка, cleanup, force-push. Existing safeguards не отключать. Документный контрпример не становится production test или training pair.

Dispatcher передаёт этот документ полностью, own checkpoint и только относящиеся source/skill excerpts. Общие условия уже включены; common не дублировать. CP1/CP2/CP3, старые A/B и summaries — baseline evidence, не дополнительные очереди. CP3 целиком сохранён в Git на planning baseline выше. Его полезные A–E, примеры и ограничения сохраняются здесь; **заменены устаревшее число workers и обязательная хронология «сначала E1/DPO, затем остальные механизмы»**. Уникальный неучтённый пункт сохраняется с source/owner, не теряется при сокращении текста.

Все приведённые ниже пары/сценарии имеют статус **DOCUMENT_EXAMPLE / NOT_FOR_TRAINING / NOT_MEASURED**. Они не являются Chinese-authored, admitted или закрытыми экзаменационными данными.

## 1. Контекст, RAG и навыки

### Первичные и действующие источники

- Исходное назначение: `D/original task.md`, `task prompt.md`, `task prompt.txt`, `goal of the round.md`, `Алгоритм создания ДПО.md`, `Обучение создания HTML CSS - 1st level.md`, `XIIGEN-human-slice-map-2026-09-24.md`. Восстановить пользовательский результат и роли; исторические команды не исполнять.
- База R08: `09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md` и `09.09.26WorkLeft-RESEARCH-01-REVIEW-20-passes.md`. Сохранить исходные гипотезы и review deltas; не назначать двадцать новых reviews. Относящиеся `RESEARCH-03-dpo-algorithm-conformance`, `MECH-01-mechanism-validation`, `02-teaching-chinese-model`, `INTEGRATION-01-training-into-work-program` читать по конкретному предмету/механизму, а не целиком каждому child.
- Current authority: `D/Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md` §§2.1–2.3; current WorkLeft-02 и S04/S06/S07 learning-delivery. S04 CodeDocumentation Student сохраняется для **его DPO-урока**. Это не запрет раньше исследовать другой готовый реальный механизм E2/E3/E4. S04 local lesson и S05 remote publication — разные фазы. 95% — исходная продуктовая цель по предметной rubric, не heuristic confidence, coverage или условие старта каждого опыта.
- Межраундовые поправки: `results/R14/FINDINGS.md` CURRENT REPAIR V2 (capability DAG, E1–E4 без E1-first); `results/R15/FINDINGS.md` F-R15R-02…07 (pair compaction, objective, first-hit consumer, отдельные role falsifiers); актуальные R12/R13 status carriers и R16 prompt. R12/R13 repaired prompt не completed research; R15 parent audit не independent Astra PASS. Проверить поздние corrections; не переносить статус из старого snapshot.
- Предметные книги: `DOMAIN-EXPERIMENT-PLAYBOOK.md`, `DOMAIN-EXPERIMENT-RECIPES.md`, `HYBRID-ALGORITHMS-AND-RUNS.md`, `HYBRID-ALGORITHM-WORKPACKETS.md`, `LEARNING-EVALUATION-CONTRACT.md`, `ALGORITHM-EXAMINATION-RESULTS6.md` — только относящиеся sections. `previous rounds thinking results.md` — история claims; current carriers проверяются отдельно.

**RAG-маршрут:** `replanning/SOURCE-ROUTES.md` → WorkLeft `RAG-MASTER-INDEX` / `RAG-00-usage-guide` → соответствующий MD/NDJSON → объявленный root → primary → current successor. DPO/Graph, DTC и IDE — разные ветви; для используемого вывода дойти до его primary. Relative link разрешать от source root, не всегда от D. Учитывать nested roots, WorkLeft/LeftToWork и aliases. R01/R03 отвечают за полную карту; R08 — за смысловые переходы своих выводов и адресный возврат пропущенных ветвей. Tree entry, parsed NDJSON и ссылка не CONTENT_READ.

Если большой источник не возвращает текст, проверить metadata/blob и альтернативный доступ; пустой API content не означает пустой файл. В CP4 journal REVIEW-20 blob `3def99dd83bc6c9728d0c5e97dede47aa3ae6a82` снова не прочитан через contents/raw. Содержательные inline O*-H поправки внутри RESEARCH-01 можно извлекать как **отдельно прочитанный источник**, но нельзя называть это полным чтением journal. Записать точный неполученный scope и зависимый вывод; продолжать независимые паспорта. Ни blanket READY, ни бесконечный повтор одного failed fetch.

### Адресная загрузка skills

| Фаза | Реальные файлы в S |
|---|---|
| INTENT/HUMAN | `architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md` |
| SOURCE/PLAN | `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md` |
| ARCH/CODE | `trainable-architecture-decision-SKILL.md`, `code-examination-SKILL.md`; для implementation proposal также `generated-code-review-SKILL.md` |
| PROMPT/EXPERIMENT | `prompt-patch-authoring-SKILL.md`, `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md` |

Для загруженного skill записать repo/ref/path/blob/sections, found/read/missing. Название не load. Применять смысл, не механически наследовать старые 30/50 sequential passes, 70/30 replay, five intent labels, mandatory test-count layer, auto-Tier-3 или численный prompt-score как новый допуск. Нынешние scope/no-new-tests/no-repeat/budget/parallel rules имеют приоритет. SCALE_UP не доказывает правильность алгоритма. Непротиворечащие решения, включая StackDetector-before-normalization, сохраняются; core skills не переписываются этим нарядом.

Внешние методические anchors: [DPO, §4, Eq.7](https://arxiv.org/html/2305.18290v3), [LoRA](https://arxiv.org/html/2106.09685v2), [Adaptive kNN-MT](https://aclanthology.org/2021.acl-short.47/). Это обоснование исследуемых механизмов, не доказательство эффективности XIIGEN или разрешение заменить собственный runtime внешним framework. Предметные ссылки ниже требуют actual version/scope; публичная спецификация не разрешает автоматически копировать закрытый case в corpus.

## A. Восстановить предметные обязательства, не повторяя инвентаризацию

**Роль:** source/requirements researcher. **Контекст:** §1 primary, WorkLeft RESEARCH-01/REVIEW-20, RAG routes, existing R08 receipts. **Skills:** INTENT/HUMAN + SOURCE/PLAN. **Вход:** exact source spans и attributed sibling deltas. **Не задача:** весь repo inventory R01/R03 или выполнение исторических reviews.

**Промпт.** По каждому из семи предметов восстанови пользовательскую способность, исходную гипотезу, поправку review, её действующее значение и опровержение. Различай subject/domain, intent, язык, stack/version, project, slice, архитектурную роль и physical learner. Покажи `primary → successor → current expectation → passport → mechanism → examination → receiver`. Отдельно учти original-only extras, HE↔EN/прочие языки и IDE/DTC obligations: отсутствие в семи baseline строках не удаляет их из проекта. По ссылкам RAG проверь первичный смысл, а не только существование файла. Отметь конфликт E1-first с R14 V2 и нынешнее уточнение WorkLeft-02; не выдумывай нового Student.

**Выход:** заполненная карта в FINDINGS, retained-hypothesis register, точные SOURCES, CHANGE-MAP с affected expectation/owner. Для пропуска — конкретный source span, что уже известно, какой вывод ограничен, кто дочитывает. Внешний пример не заменяет недоступный private primary.

**Хорошо:** «SQL+BUG, ответ по-русски: SQL — предмет, BUG — intent, русский — output language; потеря ON/WHERE — конкретная гипотеза представления». **Плохо:** «в коде пять development labels, значит иных предметов нет». **Края:** разные roots с одинаковым basename; циклический RAG; superseded authority; недоступный большой journal; новый source-backed sibling delta.

**Проверка документа:** все семь предметов и обнаруженные extras имеют trace/остаток; claims соответствуют read depth; старые reviews не посчитаны новыми. **Будущее examination:** R13 связывает retained hypotheses с независимыми families. **Return:** заполненные строки, сохранённая и оспоренная гипотезы, exact refs/limits/receiver; не перечень названий файлов.

## B. Семь предметных паспортов: законы, tokens, соседи, пары и смысл

**Роль:** domain/experiment researcher. **Контекст:** A, primary task/source и B1–B7. **Skills:** SOURCE/PLAN + PROMPT/EXPERIMENT; ARCH/CODE для используемого consumer. **Не задача:** production corpus, новый универсальный encoder или runtime generation.

**Промпт.** Доведи каждый seed до заполненного паспорта: задача/input/output; закон или contract с scope/version/assumptions; существенные признаки; good answer, правдоподобный hard negative, второй корректный вариант и unknown case; source family; предложенные representation/kNN и falsifier; metric; реальный consumer либо точный UNTRACED seam. Назови тип основания: математическое/физическое ограничение, software specification, пользовательский contract или предпочтение. Не выдавай локальную конвенцию за закон природы. Для новых физических/предметных задач сохраняй единицы, диапазон применимости и неопределённость источника. Семь passport names — не закрытая онтология проекта.

### B1. Перевод EN↔RU и исходные дополнительные языки

Input: `Do not send 10 files before approval.` Good: «Не отправляйте 10 файлов до получения одобрения». Hard negative: «Отправьте 10 файлов до получения одобрения». Второй input меняет 10 на 20; корректный перефраз сохраняет смысл, а не точную строку. `bank` без контекста допускает неоднозначность, не forced preference. Основание: task/context; [W3C ITS 2.0 §8.16](https://www.w3.org/TR/its/) — категории localization errors.

Сохранить Unicode, отрицание/scope, числа/единицы, временные связи, термины и язык. Гипотеза: multilingual features плюс явные quantity/negation relations и learned reranking; lexical overlap — baseline. Измерять сохранение смысла и отдельно числовые/negation ошибки, затем blind pairwise preference допустимых вариантов. Независимая family — другой источник, не перевод или парафраз того же предложения. Consumer обязан вернуть текст на требуемом языке, а не только близкий embedding.

### B2. Оркестрация

Task: подтвердить адрес перед отправкой сообщения двум независимым разрешённым получателям. Good: `verify(A)→send(A)` параллельно с `verify(B)→send(B)`, associations сохранены. Hard negative: `send(A)→verify(A)`, пусть он быстрее. Два допустимых одинаково полезных порядка — TIE; неизвестная precondition требует разрешения/отказа. Основание: explicit task contract, original task и actual typed-port rules, не правило «всё последовательно».

Признаки: partial order, ports/types, side effects, identity, deadline/cancellation, budget. Гипотеза: predicate/subgraph/compatible-port similarity, learned ranking допустимых combinations. Метрики: достигнутая цель, admissible-path rate, полнота input→output associations, wall time и сумма compute всех ветвей. Отдельно считать branch attempts, cancellations, fallback. Лучший путь — среди исследованных допустимых альтернатив при зафиксированном objective, не доказанный global optimum.

### B3. Figma→HTML/CSS

Task: сохранить hierarchy, текст, доступное действие и layout на заданных 360 и 1280 CSS px. Good layout удовлетворяет обоим требованиям. Hard negative: fixed width 1200px выглядит верно только на большом viewport. Размеры — условие примера, не универсальный threshold. Основание: разрешённый design, `Обучение создания HTML CSS - 1st level.md`, [W3C Media Queries Level 4](https://www.w3.org/TR/mediaqueries-4/).

Признаки: tree, geometry, units/relative dimensions, constraints, text/image relations, viewport, interaction semantics. Гипотеза: hierarchical retrieval + learned ranking визуально/функционально допустимых вариантов. Метрики: responsive violations, fidelity rubric и functional outcome по каждому viewport, цена user tier и всей цепи API→model→render/evaluation→DB→ordinary consumer. Screenshot score один недостаточен. Все renders/HTML variants одного design — одна family. Missing assets/fonts и несколько корректных layouts не дают искусственный negative.

### B4. Документация C#

Input: `string? s = null; var n = s?.Length;` — объяснить n. Good: nullable результат null; hard negative: «n равен нулю». Для non-null receiver другая ошибка — утверждать, что `?.` подавляет любое исключение getter. Основание: exact source/version и [Microsoft C# null-conditional operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators).

Признаки: AST/operator, nullable type, control flow, actual caller/callee effects. Гипотеза: typed syntax/flow + semantic text; hard filters только для реально несовместимых APIs/versions, не всех строк с `null`. Метрики: factual correctness, существенные omissions/hallucinations, перенос на другие методы/проекты. Cancellation объясняется реальным token/API, не именем метода. Consumer DPO-ветви — existing CodeDocumentation Student и его разрешённый curriculum; этот seed не подменяет зарегистрированный материал новым toy lesson.

### B5. Документация Angular

Input: `computed(() => flag() ? count() : 0)`, flag сейчас false. Good: в этом вычислении count не читался и не стал отслеживаемой зависимостью. Hard negative: любое изменение count обязательно инвалидирует computed независимо от ветви. Не смешивать invalidation с eager recomputation. Основание: actual project version и [Angular Signals: dynamic dependencies](https://angular.dev/guide/signals).

Признаки: conditional dependency graph, signal reads/writes, scope, component/template relation, version. Гипотеза: dependency-pattern similarity вместо имён переменных. Метрики: точность state/lifecycle/dependency explanation и перенос на новый компонент. Family объединяет component/template/версии одного source. Несколько верных объяснений — не ложное preference; неизвестная версия/lifecycle context — qualified answer или UNJUDGEABLE.

### B6. Документация SQL

Task: каждый клиент и его оплаченные заказы, включая клиентов без оплаченных заказов. Дано customers {1,2}, единственный paid order у 1. Good: `customers c LEFT JOIN orders o ON o.customer_id=c.id AND o.status='paid'`. Hard negative: LEFT JOIN только по customer_id и затем `WHERE o.status='paid'` — клиент 2 исчезает. Основание: schema/dialect/version и [PostgreSQL 18 table expressions](https://www.postgresql.org/docs/18/queries-table-expressions.html).

Признаки: ON/WHERE, NULL/three-valued semantics, key relations, cardinality/bag semantics; порядок — лишь когда contract/ORDER BY задаёт его. Гипотеза: query AST + schema/relations/nullability, не один cosine слова JOIN. Метрики: semantic correctness, NULL/cardinality errors, объяснение контрпримера. Hard negative похож синтаксически, но меняет смысл. Независимая family — другая задача/схема с отдельным происхождением, не переименование таблиц. Перевод объяснения на русский не превращает SQL в translation-only domain.

### B7. DPO Builder

Task: по разрешённому weakness «путает ON/WHERE при outer join» выбрать новый разрешённый урок и построить содержательную пару. Good builder output лечит конкретное различие при одном task/context/output contract. Hard negative: два одинаково правильных ответа названы positive/negative лишь за стиль; другой failure — chosen скопирован из закрытого reference. Основание: original DPO algorithm, current RUN, WorkLeft-02/RESEARCH-03; §D ниже.

Признаки: weakness/law, lawful source family, общий input, обе полные стороны, preference rationale, provenance, recipient/objective, context-only versus train IDs. Гипотеза retrieval: поиск разрешённых teaching examples по weakness/law/recipient; negative/warning memory не выдаётся за chosen. Метрики: доля полезных правильно ориентированных пар, admission/disjointness, цена до допустимого урока, затем Student gain на независимых families. Число JSON rows не полезность. TIE/BOTH_BAD/UNJUDGEABLE допускают отказ без update.

**Выход B:** семь полных passport records плюс explicit extras; `law_scope, primary_ref, family/lineage, preserved_features, proposed_metric/k_policy, mutable_recipient, ordinary_consumer, example_status, read_depth, next_owner`. Для документных примеров нет выдуманных admitted family IDs; реальный manifest либо установлен, либо конкретно NOT_BOUND.

**Проверка сейчас:** для каждого паспорта пройти good/hard-negative/два valid alternatives/unknown; различие связано с законом и наблюдаемым результатом. **Будущее examination:** R13 — rubrics/семейства, R21 — curriculum, R22 — разрешённые опыты. **Плохой результат:** семь одинаковых шаблонов с разными labels. **Return:** заполненные паспорта и конкретные gaps, не новый prompt вместо результата.

## C. Связать предметы с четырьмя ролями, представлениями и dynamic kNN

**Роль:** architect/code and experiment researcher. **Контекст:** B, exact core paths, R04/R06/R07 receipts, R14 V2, R15 F-R15R-04/06. **Skills:** ARCH/CODE + PROMPT/EXPERIMENT. **Не задача:** повторный полный аудит всех trainers/graph consumers или создание четырёх новых Train API.

**Промпт.** По каждому используемому механизму заполнить: `deterministic law/admissibility → learned signal/objective → mutable state/owner → ordinary consumer → input/contrast → controlled intervention → observable outcome`. Для нового proposal показать current versus target; наличие кода и доказанный эффект не смешивать. Если общий seam подтверждён один раз, привязать все подходящие passports; отдельный domain-specific consumer всё равно требует собственного evidence.

### Четыре разные функции, не четыре названия классификатора

| Роль владельца | Детерминированная часть | Предмет обучаемого изменения | Различающий опыт / опровержение |
|---|---|---|---|
| ILearningUnit | domain/input/vector/output schemas, typed many→many и associations | собственная mapping/representation/action policy по допустимому сигналу | те же inputs/types, меняется только state выбранного Unit; output/associations и semantics измеряются. Изменился hash, ordinary output policy не использует state — не доказанное обучение |
| ILearningFlow | совместимость ports, допустимые переходы и ограничение циклов | отношения Units и коррекция input→output | Units/их outputs фиксированы; изменяется relation/correction state Flow. Улучшение от нового solver не засчитывается Flow |
| ILearningOrchestrator | async lifecycle, dependency order, cancellation, budget | выбор комбинаций/ветвей/путей и распределение ресурса | те же solvers/inputs/resources; меняется branch/path policy. Первая завершившаяся неправильная ветвь и неучтённая цена проигравших ветвей опровергают claim |
| ILearningDynamicGraph | допустимые higher-order relations/compositions и semantic order | relation state паттернов разных порядков | lower-level policies фиксированы; unseen lawful compositions отделены от memorized paths. Нет transfer либо ordinary graph consumer — claim не подтверждён |

Это поведенческие роли; actual ILogical*/ILearningMachine/ILearningMachinePair mapping устанавливается по коду. Graph=L5 specialization, не шестой уровень. Сохранить TensorCore/shared base/project LoRA, ORM-43 без hollow second machine, StackDetector-before-normalization, semantic compression, BM25+kNN, positive/negative/warning memory и final semantic arbiter. Сохранить typed queryable many→many intent, FanOut/FanIn/ManyToMany/MultiOutput/ReturnLoop, bounded cycles и OrderedPatternIds; parallel eligibility не новый вид ребра. Не переименовывать интерфейсы по опечатке.

Hard constraints защищают допустимость, но не присваивают себе learned final authority. Детерминированное вычисление с learned weights остаётся обучаемым механизмом; случайность не доказывает обучение. Корректный deterministic ответ нельзя назначать negative только из-за происхождения.

### Domain mapping и представления

Проследить input → subject/intent/language/stack → representation → retrieval → authority → typed output. Предусмотреть known-single, mixed, rare, unknown/OOD; correct-domain/wrong-solver и wrong-domain/accidentally-correct-answer. Oracle domain — **только диагностическая** ветвь. Не использовать правильный answer/eval label в deployed eligibility или retrieval features.

Развести raw/canonical text, retrieval embedding, loss-visible tokens и output projection. Для каждого — schema/version, vocab/codebook, Unicode/numbers/units/negation/order, truncation/masks/positions, dimensions, owner/caller/consumer. Между bulk одни координаты должны сохранять значение либо нужна явная совместимая migration/reindex. Lossy retrieval допустим лишь в пределах сохранности смысла на actual decision path; его collision не автоматически collision DPO objective. Одинаковая размерность не semantic compatibility.

### Domain-sensitive kNN: что именно проектировать

Для каждого B-passport зафиксировать: разрешённый corpus и eligibility; domain features; distance/score и нормировку; index assumptions; выбор k/radius/budget; веса/использование нескольких соседей; positive/negative/warning semantics; learned reranker/gate/final arbiter и fallback/abstention. Нельзя свести adaptive kNN к одному числу k.

Конкретная **гипотеза**, не утверждённая реализация: `p_theta(k | domain, query features, neighbour distances, disagreement, remaining budget)` по заранее ограниченному набору допустимых k, с отдельной abstain/fallback ветвью. Альтернатива — calibrated radius/confidence policy без градиентного обучения; назвать её именно такой. Для текстовых/структурных outputs не усреднять произвольно vectors/ответы: определить typed selection/reranking/aggregation, который действительно использует выбранных соседей. Правило фиксированного k и trained policy — разные сравниваемые механизмы.

Минимальные contrasts: fixed versus dynamic k при одинаковом pool/features/metric/solver; затем metric-only либо representation-only при остальных фиксированных условиях. Exact versus ANN на bounded diagnostic set отделяет качество representation от потери кандидатов индексом. Query-dependent/несимметричный score может требовать reranking или иного backend; не предполагать поддержку HNSW автоматически. Калибровка и обучение только на permitted development/train families. Полезность — изменение lawful neighbour/decision quality при полной цене, не рост evidence IDs.

### Конкретные probes из уже найденного кода

C0 = `3c7f5f8c7809936e25579603c96ea6df8598fc40`; это source-bounded evidence, не измеренная частота дефекта.

- CP3: `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`, raw Build/NormalizeRoots, `[^a-z]+`, before/after, per-prompt vocab/Take(32). Различающие inputs: `send 10`/`send 20`, `send before verify`/`send after verify`, `да`/`нет`. Supplied-vocab/compiled overloads и actual learner applicability проверить отдельно.
- CP3: `23.SemanticCompilerAdapter/DomainClassifierUnit.cs`, development intent labels и heuristic confidence. .90/.95 не per-domain measured accuracy; найти actual subject coordinate/caller.
- R15 F-R15R-04: `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, train-fixed k, `_index.Search(query,_k)`, first-hit output; остальные hits могут быть лишь evidence. Warning+answer не abstention. Сначала подтвердить текущий multi-hit consumer, затем приписывать эффект adaptive k.
- CP4 direct read: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` 2269–2343, BuildBatch/ToMask/Compact; raw admission затем whitespace collapse и 480-character cut каждой части. `11.DPO/DpoTrainer.cs` 665–771: ComputeLogProbs и BackwardLogProbs нормируют на count. Предметные loss-visible различия и objective рассматриваются в D; применимость к обычному Student не объявлена traced.

**Выход C:** source/method/caller/consumer map, четыре независимых role contrasts, domain-specific representation/kNN hypotheses и smallest distinguishing observation. **Хорошо:** «k вырос, но только первый hit влияет на Output; multi-neighbour effect не доказан; адресный consumer question передан R06/R12». **Плохо:** «новый k=7 — готовое обучение» или «один raw helper означает, что вся модель не понимает русский». **Края:** empty pool, cross-domain near miss, index/version mismatch, all branches cancelled, fan-in потерял identity, unbounded ReturnLoop.

**Проверка сейчас:** каждый consequential claim имеет read-depth и causal boundary; method-only finding не стал whole-system claim. **Будущее examination:** macro/per-domain mapping, confusion/abstention, relevance/recall, associations/path validity, semantic quality и полная стоимость. **Return:** R04 representation; R05 tokens/LoRA; R06 retrieval; R07 connections; R12 выбранный design seam. Не ждать их общего DONE.

## D. DPO, LoRA и независимость данных: пара должна дойти до правильного обучения

**Роль:** preference/learning-integrity researcher. **Контекст:** B/C, current RUN §§2.1–2.3, WorkLeft-02/RESEARCH-03, R05 receipts, R15 F-R15R-02/03. **Skills:** PROMPT/EXPERIMENT + ARCH/CODE + SOURCE/PLAN. **Не задача:** генерировать настоящие Teacher pairs, проводить Astra judge или менять trainer.

**Промпт.** Заполни common pair-review packet и семь документных walkthroughs. Поля: frozen task/context/output contract; law/assumptions; обе полные стороны; targeted weakness; preference rationale; lawful source family/lineage; author/selector/assessor; exact bytes/context identity; material transformations и связь с tokens/masks; objective/recipient; separate outcome evaluator. Pair — два ответа одному input, не независимые positive/negative rows. Negative должен быть правдоподобным и предметно хуже, а не заведомо мусорным или коротким ради удобного score. Стиль не становится ошибкой без соответствующего task preference.

**Пять диагностических verdict:** CHOSEN_BETTER, REJECTED_BETTER, TIE, BOTH_BAD, UNJUDGEABLE. Это исследовательские labels, не утверждение о runtime enum. Только обоснованная допустимая ориентация проходит действующий admission. При REJECTED_BETTER не переставлять стороны молча под прежним authoring/assessment; сохранить observation и следовать разрешённому процессу новой authoring/assessment попытки без автоматического retry. TIE/BOTH_BAD/UNJUDGEABLE не forced preference. Фиксировать доли всех verdict, а не только accepted pairs.

**Роли:** CODEX GPT-6 Astra UltraCode (S04 GPT-6-Astra/ultra) выбирает разрешённый урок и отдельно оценивает exact finished pair; допущенная внешняя открытая китайская модель пишет обе final sides; Student получает update; post-update Evaluator отдельный и фиксированный. Weak Student response — разрешённый weakness context, не обязательный final rejected. Reference policy — математическая опора, не judge. Actual invocation identity подтверждается runtime evidence, не алиасом. Документный self-check не independent Astra examination. Ни CODEX, ни editor не переписывает стороны под Chinese provenance.

### Что должно попасть в objective

Для стандартного LM-DPO reference §4 Eq.7 задаёт сравнение условных log probabilities двух completions:

```text
m = [log pi_theta(y+ | x) - log pi_ref(y+ | x)]
  - [log pi_theta(y- | x) - log pi_ref(y- | x)]
L_pair = -log sigmoid(beta * m)
```

Связать x/y+/y- с exact admitted material, response boundaries, EOS/padding/masks, frozen reference и правильной trainable parameter group. Суммарный conditional response score не то же самое, что средний score всей prompt+response sequence. Иной length-normalized objective допускается как **явно названная гипотеза**, а не молчаливая замена стандартного DPO; public `EvaluateAverageNextTokenLogProbability` не менять этим нарядом. Forward/backward обязаны соответствовать заявленному objective. Нулевой initial margin при policy=reference не означает плохую пару.

Для vector/action outputs определить space и actual likelihood либо точный pairwise surrogate/learning rule; kNN similarity или сохранение negative-memory само по себе не LM-DPO. Selection recipient не автоматически winning solver или ID training wrapper. Positive/negative смысл должен дойти до владельца именно той policy, которую хотят улучшить.

Различающий пример к actual Compact: chosen и rejected имеют общий длинный prefix и отличаются существенным условием после 480-го символа; raw assessor различает их, optimizer может получить одинаковые/изменённые стороны. Другой пример — значимые whitespace/formatting. Проследить transformations до tokens и обычного consumer; если pair не помещается, требуется явный admission outcome/новый допущенный материал, а не скрытая обрезка. Не объявлять этот trace измерением Student gain.

### LoRA и смысловой мост

LoRA изменяет выбранные матрицы низкоранговой добавкой; не является автоматически tokenizer/codebook или преобразователем произвольного domain vector в model token. Для нужного проекту моста назвать input/output semantics и размеры, positions/masks, source/target version, actual projector/adaptation owner, loss, optimizer group, checkpoint и ordinary inference consumer. Сравнение: base/projector frozen versus только выбранная LoRA/projection group изменена; vocabulary/index и solver не менять одновременно. Оценивать восстановленный предметный смысл/typed output, а не лишь cosine/shape/loss. Если bridge не реализован или не traced — точный prerequisite R04/R05/R12/R18, не фиктивный готовый converter.

### Семейства и continued feedback

Разделять источники **до** генерации derivatives: teaching/train; development/calibration; operational promotion; untouched certification; специальные OOD/unseen-composition panels. Семейство включает версии метода/документа, перевод/парафразы, renders/HTML одного design, пары и производные. Проверить teacher context, retrieval indices, fitted tokenizer/projector/reranker, caches и user feedback lineage. Общая разрешённая спецификация не равна защищённому case/reference answer. Operational panel, влияющая на accept/rollback, уже используется для выбора и не называется untouched final exam.

Новый user input сначала оценить текущим accepted state, затем проверить rights, meaning, novelty и conflicts. Feedback не автоматически ground truth: например, просьба считать SQL `NULL = NULL` истинным не даёт корректную учебную пару. Accepted old pairs могут быть context-only в разрешённых пределах; новая упаковка/ID не разрешает replay. Не передавать closed tasks/answers, их производные или закрытые Student exam outputs учителю.

**Выход D:** семь walkthroughs; случаи REJECTED_BETTER/TIE/BOTH_BAD/UNJUDGEABLE; lineage/split map; exact-pair→tokens→objective→recipient→consumer map и адресные gaps. **Хорошо:** «пара пригодна, но loss-visible suffix потерян; допуск к этому update не доказан». **Плохо:** «sealed pair + lower loss = полезный урок». **Края:** suffix truncation, empty completion, unequal lengths, changed context после assessment, conflicting feedback, no compatible parent, untraced recipient.

**Проверка сейчас:** orientation, transformations, masks, leakage и неверный recipient разобраны документно; не выдуманы hashes/assessments/model calls. **Будущее examination:** отдельно pair quality, actual update, independent Student utility и ordinary adoption. **Return:** R05/R12/R13/R18 exact seam; R21 data; R24 feedback. Результат D не блокирует независимый E2/E3/E4 без этой зависимости.

## E. Выбрать ранний информативный опыт и измерять от bulk к bulk

**Роль:** experiment planner с whole-program context. **Контекст:** A–D, current WorkLeft-02, R14 V2, R15, available R09/R11/R12/R13 receipts. **Skills:** PROMPT/EXPERIMENT + PLAN/HUMAN. **Не задача:** новый evaluation framework, полный factorial campaign, исполнение обучения или ожидание всего curriculum.

**Промпт.** Подготовь предметные входы для четырёх кандидатов ниже и аргументированный provisional priority/fallback. Итоговый design binding принадлежит соответствующим R12/R13/R18; R08 не объявляет их решения принятыми. Выбирать по ценности различающего наблюдения, actual prerequisites и полной цене, а не по номеру slice, наличию StepVector или удобству toy demo. Не выдумывать численный information-gain score без основания.

| Кандидат | Один изменяемый фактор и сохраняемые условия | Что R08 обязан дать | Локальная зависимость, не общий барьер |
|---|---|---|---|
| E1 DPO/update/adoption | один новый допустимый lesson, правильный Student recipient; фиксированы task families/evaluator/context | law-sensitive pair contrast и независимый outcome rubric для existing CodeDocumentation Student | exact pair/objective/state/ordinary caller; неполная E1 не блокирует самостоятельную E2 |
| E2 domain/representation/kNN | выбрать только representation, metric, k-policy или reranker; остальные factors фиксированы | domain-law features, lawful near misses, predicted/oracle diagnostic, retrieval/output metrics | compatible index и реальный decision consumer; fixed-rule gain не называется learned-policy gain |
| E3 Flow/Orchestrator | relation/path/budget state, Units и solvers фиксированы | typed associations, допустимые/недопустимые пути, endpoint quality и all-branch cost | actual relation/policy update owner и consumer, не готовность всей E1 |
| E4 DynamicGraph | одна higher-order relation policy, lower levels фиксированы | independent lawful compositions и отличие от memorized path | actual graph update/adopter и bounded transfer panel, не весь curriculum |

Для E1 не подменять Student новой Qwen-моделью, таблицей маршрутизации или TinyTrainingFixture. Для E2/E3/E4 также использовать actual existing seam, не создавать convenience learner. Diagnostic no-update сравнение может дать полезную информацию до первого update, но не считается состоявшимся обучением. Второй структурно иной предмет нужен для широкого transfer claim, не для старта первого bounded исследования.

### Заполненный handoff, а не пустой recipe

R08 сам заполняет: subject/task/law, hypothesis, good/bad/unknown, independent-family критерий и доступные source identities, primary behavioral metric с numerator/denominator/unit, controlled factor, frozen factors, falsifier, retained capability, constraints и адресные code observations. Не оставлять всю предметную конкретику «на R13».

По каждому candidate указать actual ordinary entry, state owner, permitted material/config и resource limits **если установлены**, иначе конкретный unresolved field, source already examined, вопрос и owner. R13 связывает actual manifests, evaluator invocation/rubric, sample plan/noise, minimum meaningful effect, tolerable regression и stopping rule до outcomes; R12 — спорный state/design; R18 — smallest implementation seam. `NOT_BOUND` не runtime-ready. Unknown effect — цель опыта; missing permission/schema/recipient — prerequisite. Не скрывать design decisions внутри будущего execution gate и не придумывать runtime command.

### Минимальная causal programme

Сначала зафиксировать parent и условия no-update baseline; **не требовать полного списка следующих contrasts до первого урока**. Для выбранного E1–E4 провести один separately authorized intervention, сравнить на той же независимой operational panel, принять или восстановить по policy, проверить ordinary reopen. Затем только контрасты, нужные для следующего открытого вопроса: predicted/oracle mapping; exact/ANN; fixed/dynamic k; representation-only; LoRA/projector-only; path/graph policy. Взаимодействия factors исследуются после мотивированного сигнала, не декартовым перебором.

Для claim «гибрид компактнее и эффективнее» сопоставить actual deterministic-only, learned-only и hybrid там, где ветви действительно реализованы и сравнимы. Доступные knowledge/memory/context, safety constraints и declared budgets сопоставимы; нельзя подыгрывать гибриду большим скрытым compute. Невозможная counterfactual ветвь — limitation, не fabricated score и не новый prerequisite всему проекту. Правильный deterministic baseline может выиграть; это полезный результат, не причина переопределять rubric.

### Bulk и полная стоимость

Bulk = заранее объявленное окно **новых последовательных уроков**, не replay всех накопленных pair IDs. Для окна хранить initial accepted parent, все attempts/rejections, accepted-state trajectory и end-state. Сравнивать parent/challenger на одинаковых заданиях; при новой panel прогнать обе версии на ней. Иначе изменение сложности bulk будет выдано за improvement. Между окнами сохранять anchor/retention panels; их адаптивное использование отделено от untouched certification.

Материалы/память могут сами улучшать ответы без weight update. Нужен comparable no-update/memory-only diagnostic control с разрешённым знанием, а не преимущество от дополнительных examples только у challenger. Отдельно измерять pre-update response на свежем user input, затем post-update effect на независимой family. Последовательные trials не выдавать за независимые seeds с replay одной пары; дополнительные repetitions подчиняются реальному no-repeat contract.

Минимальный отчёт по каждому предмету/окну:

- semantic correctness/quality по frozen rubric, отдельно law violations; domain mapping errors, conditional solver quality, mixed/OOD/abstention; macro и worst-domain, не один скрывающий провал aggregate;
- useful correctly oriented pair rate со всеми verdict в знаменателе; valid-path/association outcomes и лучшая наблюдённая допустимая альтернатива;
- time-to-first-valid-lesson **отдельно** от time-to-first-accepted-improvement; teacher/selection/assessment/update/evaluation/reopen, все неудачные/отменённые ветви; ordinary p50/p95 лишь при достаточной выборке;
- peak whole-process/system RAM/VRAM, storage/state growth, index/rebuild, encoder, base model, activations, adapters/projector, optimizer, graph, cache и snapshots; не размер одного LoRA;
- retention прежних доменов и применимость к ordinary consumer после restart.

Зафиксировать actual machine/config/caps до campaign: исторические 8GB VRAM/16GB RAM не доказательство нынешнего fit. Между обучением, внешним Teacher/assessor и ordinary inference показать раздельную и полную цену. Parallel research не означает 16 одновременно работающих GPU trainers.

Считать uncertainty по независимым source families, не по тысячам correlated paraphrases. Group-disjoint split и rare-domain stratification согласовать, не ломая family boundaries. Freeze evaluator/model/rubric/order policy, effect/regression bounds, budget, sampling/stopping и comparisons до outcomes. При blinded judge скрыть candidate label/order, сохранить raw verdict/rationale; disagreement и неоднозначность не исправлять постфактум под победителя. Exhausted budget или noise больше эффекта дают INCONCLUSIVE, а не обязательный auto-scale.

**Вердикты:** IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE; без измерений NOT_MEASURED. Missing не ноль. Валидный отрицательный опыт завершает проверку гипотезы, но не доказывает прирост. NO_TRAINING_NEEDED/identical weights нельзя скрывать или считать наученным улучшением.

### State и восстановление

Снимок охватывает реально mutable части: model/LoRA/projector, optimizer/scheduler/RNG/counters/identity/pair journal; изменённые tokenizer/index/metric/graph — тоже. Reader-visible bundle должен быть совместим. Unknown update outcome/awaiting-evaluation не разрешает второй optimizer update; resume/restore по actual policy. Full-state restore и ordinary adoption — разные наблюдения. S04 one-update scope не вечный запрет S06 campaigns новых уроков. Новые расходы, модели и разрешения не возникают из recipe.

**Признаки неверного направления:** gain только на знакомом template; oracle mapping помогает, deployed mapping нет; confidence вырос без точности; стороны слились до loss; projection change не дошёл до consumer; graph выигрывает лишь ресурсами; LoRA снизила loss и ухудшила semantics/retention; удачный bulk выбран после скрытых попыток. Для каждого назвать ближайший controlled contrast и owner, не назначать ещё десятки общих reviews.

**Выход E:** четыре bounded candidate records, provisional priority/fallback с причинами, готовый предметный пакет для лучшего доступного seam и точные unresolved prerequisites; R13/R18/R21–R24 handoffs. **Хорошо:** «E1 пока имеет exact-pair gap; E2 может первым ответить на domain-vector вопрос при фиксированном solver; обе гипотезы остаются в программе». **Плохо:** «сначала все семь доменов, все Windows runs и все ablations» или «всё отключить ради первого зелёного урока». **Края:** OOM, state mismatch, noise, no improvement, changed sibling consumer, cross-domain forgetting.

**Проверка сейчас:** expectation→metric/factor/falsifier связаны; числа/команды/готовность не выдуманы; зависимость у каждого seam локальна. **Будущее examination:** separately authorized runtime campaign и actual independent Astra/ultra review exact candidate по R15/R25, не авторская симуляция.

## 2. Работа при одновременном R01–R20

A/B начинают по direct sources; C/D используют собственные passports и available exact receipts; E можно выдавать по достаточному bounded subset. Не создавать цикл «R07 ждёт R08, R08 ждёт R07». Числа nine/twelve в older JOIN/common/AGENTS не ограничивают прямое поручение R01–R20; их source/integrity/write-scope правила сохраняются.

| Получатель | Что R08 передаёт, не ожидая whole DONE |
|---|---|
| R01–R03 | новые roots/primary requirements и точные ownership/lineage gaps |
| R04–R07 | domain-law distinguishing cases, actual source-bounded seams и конкретный вопрос к владельцу; не перепоручение всей R08 |
| R10 | versioned evidence subset, полнота/ограничения и pending receipts; R10/J03 по-прежнему сводит исследовательские входы, а не ждёт зависимый от него R16 |
| R11/R12 | причинное различие, competing hypothesis, deterministic/learned boundary, точный state/recipient вопрос |
| R13/R14 | предметная rubric/families и E1–E4 candidate prerequisites; отсутствие E1-first, не объявление final winner |
| R15 | exact candidate/ref, claims/counterexamples/limits для отдельного review; late changes не наследуют старый approval |
| R16 | source-backed expectation и конкретный active-requirement patch question; hypothesis остаётся provisional до относящегося design/review binding |

Каждый receipt: `origin_run/local_id/checkpoint; repo/ref/path/blob/span; meaning; proven_subset/read_depth; unresolved; affected_domain/role; receiving_owner; exact_action; evidence_needed; status`. Receiver должен показать, что изменилось в его task/decision/consumer, либо объяснить непринятый остаток. Простая ссылка «прочитал R08» не semantic receipt. Namespace IDs других программ не перенумеровывать.

Одновременный старт R11–R16 разрешает чтение/анализ/подготовку. Он не позволяет R16 превратить OPEN mechanism в принятое требование или перезаписать чужой result. R16 может применять независимые подтверждённые нормы в своём scope, а зависимый пункт оставляет явно provisional с exact owner. Не требовать ALL_R01_R16_DONE: это создаёт цикл. Late source delta перечитывает только affected spans и приводит к versioned correction; unrelated passports не reset. Conflicting applied requirement адресно возвращается владельцу, не тихо отменяется R08.

## 3. Деление объёма и model-ready child packets

Один parent/writer R08 ведёт five carriers; children ничего общего не пишут. A, C, D, E делятся только по конкретному вопросу/стыку, не полным повтором цепочки каждым ребёнком. Для B допустимы bounded группы: Translation; C#/Angular/SQL; DTC; Orchestration/Builder. Каждая получает только свои primary/consumer и полный относящийся B-seed.

**Dispatcher каждого child обязан развернуть содержанием:** текущий task/not-task, inherited §0 rights/§1 authority, actual role и нужные skills, exact source spans/ref/own checkpoint, goal/current→target, подробный prompt выбранной части, заполненный good/bad и edge из её seed, output fields, documentary acceptance сейчас, future examination отдельно, unresolved/receiver и return-only. Отправить «см. родительский документ, изучи SQL» недостаточно. Child без нужного контекста не считается исполнителем готовой подзадачи.

Пример SQL child: восстановить ON/WHERE law по exact source/dialect; связать B6 с одним actual representation/retrieval consumer; вернуть паспорт, один meaningful pair contrast, controlled E2 factor и source-bound gap. Он **не** исследует весь DPO/Graph, не создаёт corpus и не вызывает judge. Good — сохраняет unmatched customer и различает family/template. Bad — меняет таблицы в старом примере и называет это независимым holdout. Edge — неизвестный dialect, два корректных SQL, NULL/duplicates. Проверка — существенное различие и actual read-depth, не test count.

Использовать visited `(repo,ref,path,blob,span)` и единый общий source receipt вместо повторной загрузки мегабайтов. При перегрузе передать конкретный remaining question с полным packet; не уменьшать исходную область и не добавлять автоматически R26/30/50. Реальные extra calls учитываются как extra calls с причиной; A–E, четыре B-группы и E1–E4 сами по себе не вызовы моделей.

## 4. Приёмка и честный return

**Исследовательская приёмка R08:** семь source-bound passports плюс accounted extras; retained hypotheses/review deltas с точным unread остатком; actual method-bounded observations и четыре role contrasts; DPO/representation/LoRA/split walkthroughs; E1–E4 candidate records и предметный handoff; owners/receipts. Пустые шаблоны, опубликованный prompt или чужие summaries не закрывают исследование. Обязательный unread источник удерживает его completeness claim OPEN, но не отменяет подтверждённый subset.

**Документная проверка:** requirement→source→domain law→mechanism→factor→metric/falsifier→receiver; мысленно пройти good/bad/unknown/negative-outcome и late-delta cases; проверить path/ref, Russian readability и отсутствие противоречивого E1-first. Это не product test/coverage и не measured model gain.

Перед write — fresh file SHA, non-destructive reconcile; после — remote content/blob readback. Не менять чужие результаты, не force-push и не утверждать GitHub read-only из-за одного failed read. Использовать существующий COMMON-CONTRACT CSV schema и сохранять old rows/IDs как исторические evidence.

**Return сначала по-русски:** что система должна уметь; конкретный input/good/bad; что установлено/неизвестно; какие файлы изменены; какие functional examinations подготовлены; ближайший адресный seam и его owner; commit/readback. Затем machine fields. Отдельные статусы: PROMPT_REPAIR, RESEARCH_COMPLETENESS, RUNTIME_MECHANISM, LEARNING_GAIN, INDEPENDENT_REVIEW. Без runtime — NOT_MEASURED; без отдельного reviewer invocation — NOT_PERFORMED. Авторский self-check не independent Astra review; requested model/effort не actual invocation.
