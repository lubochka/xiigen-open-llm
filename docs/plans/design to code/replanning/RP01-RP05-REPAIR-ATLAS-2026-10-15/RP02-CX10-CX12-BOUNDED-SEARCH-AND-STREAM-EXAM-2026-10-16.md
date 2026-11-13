# RP02 CX10–CX12 · Ограниченный поиск, совместимость векторов и честный bulk

**16.10.2026 · IN_PLACE_DOCUMENT_REPAIR_PUBLISHED / AUTHOR_AUDIT_ONLY.** Это дополнение Atlas и адресный ремонт действующего RP02, не новый dispatcher, V8, три обязательных раунда или три наблюдавшиеся production-аварии. Пакет сохраняет **18 основных карточек + 3 дочерних**. Промежуточный статус IN_PLACE_REPAIR_PENDING в коммите `0cb0467b7f09d0bcbb9e40c5d8a270c4b1d0d116` относился к первой публикации; ниже зафиксирована последующая интеграция в сами задания. Core, данные и веса не изменялись.

## 1. Что опубликовано, а не только предложено

[Действующий RP02 и маршрут](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/README.md) содержит ссылки на этот аудит. [PR №16](https://github.com/lubochka/xiigen-planning/pull/16) успешно объединён в `llm_plan3`: **merge `0150cef44594920216265a109802f448eac582fb`**, проверенный PR head `dd927c3aeb9ec6702e1221f5fcc13431bce1a24f`. Ранее непосредственно опубликована 01C: **`2f884cf17f85c767b8e7e18b0485a051ac298ffe`**. README с адресными ссылками: **`cb39658b8988f87b05f78e98eab1328f3eaa64cb`**.

| Изменённая физическая карточка | Применённая конкретика |
|---|---|
| [RP02.01C](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.01C.md) | CX11: actual projection/admission/load/query contract; semantic identity, width, finite/zero policy; отказ до неверного distance, без обязательной общей миграции. |
| [RP02.02B2](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.02B2.md) | CX10: actual work budget, equal-score traversal, request-local probe/expansion budget, cooperative stopping и residual work. |
| [RP02.02B2/LEARN](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/children/RP02.02B2-LEARN.md) | Точные признаки context × action сохранены; завершённые, ограниченно остановленные, незавершённые и неисполненные outcomes не смешиваются при обучении политики. |
| [RP02.02C](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.02C.md) | Same lawful semantic space; отдельные geometric/law-scorer references; неполный brute-force не exact full-pool oracle; полная стоимость. |
| [RP02.03B](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.03B.md) | CX12: literal feedback timeline, уникальные lessons/steps/families, все dispositions и раздельный учёт затрат. |
| [RP02.03A](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.03A.md) | Read-only экзамен использует новые различия только для назначенного вопроса, сохраняет независимые семьи, blind grading и первый C03 без общего gate. |

Это **шесть изменённых packet files: пять основных и один child**. Каждая карточка содержит входы, контекст, actual references, skills, назначенную фазу/права, пошаговый prompt, GOOD/BAD/EDGE с причинами, falsifier, recovery и формат результата. Остальные 15 packet прочитаны, но не переписываются ради нового счётчика. Весь Atlas не является обязательным контекстом перед каждым заданием.

## 2. Основа проверки и ранее исправленные замечания

Исходный P: `5f11281d50463535050bef641a46cb87c978ff3c`; K: `58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`. [Предыдущие применённые findings](08-RP02-APPLIED-REPAIR-AND-ADDITIONAL-FINDINGS.md) и фактические карточки уже разделяют θ-update, learned-k, качество, стоимость, cold consumption и promotion. CX01–CX09, семантические признаки и исходные владельцы сохранены. Наличие планового исправления не переводит code/science claims в DONE.

Прямо подтверждено в прочитанных методах K: KnnLogicalUnit.ExecuteAsync использует `hits[0]`; несколько EvidenceIds сами по себе не доказывают решение по нескольким соседям. Search(topK) использует max(efSearch,topK). **Shared SetEfSearch уже был явно разобран в прежней RP02.02B2: это НЕ новая находка.** BpeTokenizer.Train/Encode использует 256 byte tokens и UTF-8: гипотеза «новая кириллица обязательно превращается в неизвестный токен» этим источником НЕ подтверждается. Byte coverage не заменяет POS, референты и предметные связи.

## 3. CX10 · efSearch не является строгим бюджетом работы и отмены

**Класс:** новый конкретный source discriminator ранее требовавшегося PC budget, не измеренный performance incident.

Источники: [HnswIndex.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/06.VectorSearch/HnswIndex.cs), blob `03287c05bcd6c4422d679a3b2e37a03f418918d6`, Search/GreedySearch/SearchLayer; [KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs), blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, ExecuteAsync.

SearchLayer ограничивает results через ef, но добавляет каждый обнаруженный сосед в candidates. Остановка по качеству использует строгое `best.score < worstResult`. При одинаковых scores оно ложно: на связном графе могут быть посещены все достижимые nodes даже при ef=1. Это контрпример по исходному алгоритму, **не выполненный .NET benchmark**. Upper-level descent также расходует work. Search не принимает cancellation token; ExecuteAsync проверяет отмену перед синхронным Search. Task.WhenAny/внешний timeout не доказывает прекращение CPU работы.

**Применённый ремонт:** 02B2 отличает настройки, измерение и реальное enforcement. Probe/expansion тратят один request-local budget; указываются фактические distance/visit/frontier/stop counters, либо их недоступность. Определены cooperative stop, крупнейшая неотменяемая операция/overshoot, incomplete result и действительная граница best-so-far/ABSTAIN. 02B2/LEARN не использует ненаблюдавшийся final answer или придуманную стоимость как preference label. 03A учитывает residual work и не запускает загрязняющий следующий arm.

**GOOD:** SQL aliases/C# дубликаты/RU aliases имеют одинаковый coarse vector, но policy ограничивает реальную работу и сообщает incomplete outcome; предметные признаки и final arbiter не отключаются. **BAD:** ef=8 назван «восемь сравнений»; caller вернулся по timeout, а продолжающаяся работа объявлена бесплатной. **EDGE:** ties, zero coarse vectors, отмена во время descent, дорогой parser, budget закончился до первого допустимого кандидата.

**Falsifier/recovery:** реальный достигнутый backend уже имеет доказанную cooperative границу → REUSE_NO_CHANGE. Наличие только timeout не подтверждает остановку. Исправляется один actual seam, не весь scheduler/Graph. Испорченный cost exam переоценивается read-only без replay применённого урока. При новом hard boundary нужна только ближайшая непокрытая проверка изменённого риска. Малый feasible fixed-k θ pilot может стартовать с честно измеренной, а не гарантированно ограниченной стоимостью.

## 4. CX11 · Несовместимая ширина векторов может дать ложную cosine

**Класс:** конкретизация существовавшего требования representation/loader compatibility; не новая категория и не доказанная обычная авария KnnLogicalUnit.

Источник: [Distances.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/06.VectorSearch/Distances.cs), blob `3bef456735d33aa23228c6aee72dde308a09bcac`, CosineSimilarity, вместе с HnswIndex Add/Load/Search. Цикл проходит a.Length без equal-length guard. Для a=[1], b=[1,100] получается 1 при игнорировании хвоста; обратный порядок обращается за пределами короткого массива. Это **аналитический source counterexample**, не runtime receipt. В прочитанном HnswIndex Add/Load общей ширины не устанавливают.

**Важная граница:** KnnLogicalUnit сам строит training/query vectors одной `_vectorDimensions`; его текущий обычный вызов не объявляется дефектным. Требуется достигнутый projection/load caller, иначе source-backed reuse. Equal width необходима, но не означает одинаковый смысл координат.

**Применённый ремонт:** 01C описывает actual transform и semantic-space/version/width/finite/zero admission до distance. Нельзя padding/truncation, смешивать coordinate orders или считать LoRA A/B parameter bytes токенами. 02C/03A сохраняют валидную общую геометрию и полноту reference. Прерванный brute-force допускает только честно обозначенный завершённый subset или NOT_IDENTIFIED, а не exact full-pool oracle.

**GOOD:** compatible parent/trial/index и реальная meaningful projection; несовместимый loaded vector отклоняется до сравнения. **BAD:** короткий SQL vector получает cosine=1 с длинным C#/AST vector, а «ремонт» сводится к padding. **EDGE:** та же длина при перестановке ролей, quantization/deprojection, NaN/Inf, empty/zero/no-information, старый persisted index. Нулевой вектор следует явной domain policy, не универсальному ложному запрету.

**Falsifier/recovery:** фактический внешний guard уже защищает seam → REUSE_NO_CHANGE. Иначе owner 01C/01B исправляет только admission/transform, сохраняет compatible parent, затем read-only exam. Проверка этого отказа не требует нового обучения. Защищённый fixed-space первый урок не ждёт общей миграции.

## 5. CX12 · Bulk должен отличать запоминание случая от будущей способности

**Класс:** методическая конкретизация WorkLeft R16 и существующих 03B/03A, не найденный runtime bug.

[WorkLeft RESEARCH-03, current R16](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) уже требует unique lessons/no-repeat и независимые семьи. Новая конкретика предотвращает ошибку слабого исполнителя: после добавления feedback-case в память его повторный ответ нельзя назвать unseen generalization.

**Применённый ремонт:** 03B задаёт prediction-before-correction → законный feedback → selection/Chinese author/separate judge → отдельный UPDATE → immutable trial → independent exam. Три результата отделены: фактическое предсказание до feedback, TRAIN_DIAGNOSTIC уже изученного случая и unseen-family оценка. При отсутствии сохранённого предсказания: PREQUENTIAL_NOT_AVAILABLE, но допустимый independent C0/T продолжается. Новая telemetry-платформа не обязательна перед первым уроком.

Bulk содержит реальные counts/denominators: offered, rights-eligible, novel, author-generated, judge-admitted, unique applied lessons, optimizer steps; tied/both-bad/unjudgeable/duplicate/revoked/failed не исчезают. Stored reuse/ветвления не подгоняются под выдуманную линейную воронку. Несколько losers одного input не независимые семьи. Кривая способности привязана к уникальным applied lessons и независимым examined families, не только номеру bulk.

Serving/training/author/judge/exam costs разделены, полный total сохранён. Если вместе с θ изменили answer-bearing memory, фиксировать её для θ contrast или явно объявлять compound intervention. SEALED ответы не попадают в retrieval/teacher/features. Первый полезный урок не ждёт фиксированного большого bulk.

**GOOD:** после одного C# урока отдельно показаны training diagnostic, новая C# family и RU retention; SQL расширяется своим паспортом. **BAD:** запомнили case, повторили его, назвали unseen accuracy; сорок failed/tied кандидатов исчезли из стоимости десяти применённых. **EDGE:** delayed/conflicting feedback, смена контекста, нет pre-feedback prediction, нет novel material, fan-out losers, несовместимый checkpoint.

**Falsifier/recovery:** отсутствует подтверждённая независимость семьи/предварительное предсказание, grader применяет lesson или скрыты costs. Исправить соответствующее evidence, сохранить parent и не повторять уже применённую пару. Реальный существующий ledger переиспользуется.

## 6. Функциональность остаётся обязательной без coverage-first процесса

Domain axes → 01A/C01; semantic coordinates и domain laws → 01B/01D/C02; настоящий θ ranker → 02B1/C03; contextual/learned search effort → 02B2/C04; exact/ANN → 02C/C05; допустимая typed fusion → 02A/C06; реальное next-role/token/AST/action решение → 02D/C07; Student×θ learning interaction → 03C/C08; LoRA projection → 01C с RP18; feedback/retention/bulk → 03B/03A; независимый exact-candidate review → 04A.

C#/EN→RU/SQL исследуются своими паспортами: операторы/nullability/AST; отрицание/условия/POS/один референт; JOIN/NULL/три значения/multiplicity/dialect. Один удачный домен не доказывает остальные. RP03 Unit many→many/Flow и RP04 asynchronous Orchestrator/higher-order Graph не сводятся к retrieval и не реализуются заново внутри RP02. RP01 Student DPO и RP05 continual ownership сохранены.

**Ближайшее исполнение:** 00A + один ready 00B + minimum 01D + 02B1/S + 03A/S → reuse/CODE → один новый разрешённый UPDATE → немедленный 03A/E. Scorer с первого опыта использует релевантные domain laws, а не только соседскую близость. Learned-k, all-domain passports, LoRA interaction, exact full-pool reference и новая telemetry не универсальные prerequisites этого урока; их собственные claims требуют собственных результатов.

Source/plan/E/review/no-change = **0 новых product tests**. Изменённые hard invariants — ближайшая existing check, новая только под distinct uncovered changed risk. Windows/coverage не доказывают обучение и не добавляются как gate. Компилятор/SQL engine могут быть предметными oracle, не кампанией покрытия реализации.

## 7. Роли, действительная глубина проверки и receipt

**CODEX, requested `astra 6 ultracode`, выполняет делегированную человеческую роль выбора. Допущенная внешняя открытая китайская модель создаёт ОБЕ final chosen/rejected стороны. Отдельный CODEX, requested `astra 6 ultracode`, оценивает неизменённую exact pair.** Actual runtime ID и invocation receipt фиксируются отдельно. Student-output examiner и независимый code/science reviewer — другие реальные вызовы. Автор этой правки не выдаёт себя за эти выполненные роли. RP01-specific synthetic exception не переносится в RP02 молча.

**Прочитано:** все 18 действующих основных RP02 cards и все три child packets; effective context/README/manifest и релевантные прежние Atlas passages. Исходные предметные маршруты прочитаны выборочно: D/original task.md; WorkLeft MASTER current entry и RESEARCH-03 R16; V/RAG-06 → F/ARBITER-KNN-AUTHORITY-AMENDMENT Decision/roles/priority; начало previous rounds и current JOIN. Это содержательный RAG→primary переход, но НЕ полный обход всей библиотеки, всех WorkLeft/NDJSON или повтор R01–R25.

**Core:** KnnLogicalUnit Train/ExecuteAsync, HnswIndex search/helpers/persistence, Distances и начало BpeTokenizer Train/Encode/Decode. Полный runtime trace всех ordinary callers, .NET reproduction и измерение PC не выполнены. Обобщать эти конкретные методы на все encoder/consumer запрещено.

**Шесть запрошенных skills:** прочитаны relevant sections `trainable-architecture-decision-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md` в K/docs/skills. Исторические модельные/coverage routines не отменяют действующее пользовательское назначение. Авторская смысловая проверка не является отдельным independent invocation.

**Публикация проверена:** PR diff содержит ровно пять намеченных файлов; 01C опубликован отдельным commit. Compare PR head→merge показывает только сохранённую параллельную RP04 INSTALL-AND-START правку, без дополнительного RP02 изменения. После merge повторно открыты предметные участки 01C/02B2/03A; оба manifest открыты на exact merge SHA и имеют одинаковый blob `863797be89d62900709b8c6f7e6110fe593eb59d`, 18 IDs и 3 children. Это проверка публикации/контракта, не исполнение алгоритмов и не полный независимый semantic review. GitHub update_ref дважды вернул GraphQL error; применён обычный PR merge, без force-push и без потери соседских правок.

**Итог:** `RP02_DOCUMENT_REPAIR_PUBLISHED; AUTHOR_AUDIT_ONLY; PRODUCT_TESTS_ADDED=0; CORE_UNCHANGED; DATA_WEIGHTS_UNCHANGED; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`.
