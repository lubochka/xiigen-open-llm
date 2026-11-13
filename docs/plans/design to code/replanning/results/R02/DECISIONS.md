# R02 — решения по требованиям и готовая передача

**13 октября 2026. Review/repair R02, с сохранением параллельного исправления авторства.** Это документный результат, не утверждение новой production архитектуры. [CHANGE-MAP.csv](CHANGE-MAP.csv) сохраняет 52 исходных ID; [FINDINGS.md](FINDINGS.md) объясняет систему и RC1–RC6; [SOURCES.csv](SOURCES.csv) закрепляет опоры; [RUN-LOG.md](RUN-LOG.md) фиксирует выполнение. Предыдущие подробности доступны [на опоре до review](https://github.com/lubochka/xiigen-planning/blob/10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078/llm%20mvp/design%20to%20code%20session/replanning/results/R02/DECISIONS.md); новое поручение не обнуляет их.

**Дельта этого ремонта:** уточнены собственный bounded prompt R02 (§3.1), наблюдаемый вклад четырёх уровней (§7), доменные примеры (§8) и протокол сопоставимых bulk (§9). Исправлены неоднозначные указания «получи teaching packet» и «независимые одинаковые панели»: исследователь читает существующий путь, не вызывает генератор; сравниваемые состояния получают одни и те же оценочные задания, отделённые от обучения. Ранее исправленное Chinese authorship не объявляется новой находкой. Это дополнение существующих поручений, не новый набор проходов и не разрешение выполнять обучение сейчас.

D = `llm mvp/design to code session`. Все короткие C/P/W/N/RV IDs ниже — источники именно R02, не одноимённые номера других пакетов.

## 0. Решённое исправление: авторство обеих сторон

**Обе окончательные стороны chosen/rejected создаёт допущенная открытая китайская модель. CODEX с Astra 6 ultracode выбирает учебную задачу вместо человека и отдельно оценивает качество exact pair. Student обучается.** Его настоящий слабый ответ на отдельную разрешённую учебную задачу — контекст генератора, не автоматически окончательный rejected.

Основание: [RUN §2.2 от 8 октября](https://github.com/lubochka/xiigen-planning/blob/59499f459b8e5c9c714ddb475c34bf3a393b5d99/llm%20mvp/design%20to%20code%20session/Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md) и [RUN §§2.1–2.2 на опоре предыдущего review](https://github.com/lubochka/xiigen-planning/blob/277959a20f2e6742d8caa51d08106e061e16a086/llm%20mvp/design%20to%20code%20session/Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md), P15/P16; нынешнее назначение ролей пользователя. P16 повторно прочитан автором алгоритмического review; blob `1f008553956af825089757bfea6655f7bfeabc83`. Свежая проверка тех же строк45–80 на planning `c19ebca4d35bd008773afbb34efae20399a3059c`, blob `6e0e47a06a568cf7fef2e7c73239ac2e3d6a5fb3`, подтверждает обе роли и авторство. В контракте сокращение разрешено как `GPT-6-Astra` + reasoning `ultra`; не искать отдельный preset и не считать это настройкой всех development workers.

Прежний R02 ошибочно возвёл историческую P14 teacher-goal ветвь со Student-only negative в действующую policy. Это **решённая ошибка приоритета**, не новый вопрос R12. Китайское авторство обеих сторон не запрещено историческим teacher-vs-teacher ограничением. Схема, unit address, semantic order, objective и fan-out P14 рассматриваются независимо и сохраняются там, где не заменены. Свежий retrieval SHA или слово «канон» не возвращает старой policy полномочия.

Не переименовывать старые Student/teacher outputs в Chinese-authored заменой provenance. CODEX не переписывает стороны. Исправление не разрешает новые providers, дополнительные calls, replacement sealed pair или повторное обучение.

## 1. Решённые требования и точные открытые вопросы

| Вопрос | Вывод R02 | Дальнейшее действие |
|---|---|---|
| Выбор урока / качество пары | CODEX Astra 6 ultracode в обеих ролях, §0. | R16/R20 исправляют active Qwen/self-judge/manual carriers. |
| Генерация / обучение | Chinese open model создаёт chosen И rejected; Student учится; Builder — отдельная требуемая способность. | Side authorship отдельно от происхождения context; RC2 закрыт источниками. |
| Детерминированные части | Вычисления, constraints, память и learned authority сохраняются вместе. | Сравнивать полезность/цену; random output не доказывает learning. |
| Четыре роли и Train | C01 Train у ILearningMachine; P11 DynamicGraph — L5 specialization, не шестой уровень. | Отсутствующая обучаемая связь остаётся gap, не удалением Flow/Graph. |
| Допуск мутации | P10 Decision12=A: один server-owned issuer, 25.ModelServer. | R05 отличает issuer от machine/trainer; не вводит второго offline issuer. |
| Literal адрес пары | P14: Level=unit, NodeOrEdgeId=unit identity, 14-field canon. | Lower item не имеет UnitId; R05 показывает actual sidecar/recipient bridge. |
| Смысловой порядок | OrderedPatternIds в sides, structural envelope в InputContext. | Candidate-array permutation не означает сортировку шагов решения; не добавлять поле15. |
| Target обучения selector | ChosenUnitId/RejectedUnitId — альтернативы, не доказанный target selector. | R05 current trace, R07 unresolved credit, R12 design choice. |
| Judge score и DPO reward | P14 reward — policy/reference log-probabilities serialized sides. | Не подменять objective judge score/similarity; отсутствие такого пути требует именованной альтернативы R12. |
| Margin как admission | **RC4: positive model margin до обучения не обоснован; policy=reference даёт ноль.** | R05 actual consumer старого LOW_CONTRAST; R12 отделяет semantic admission от learned margin. |
| N-кандидаты | N total→N−1 attempts; L losers→L attempts, не updates/independent families. | Tie/both-bad/unknown не принудительное preference; §0 определяет producing policy. |
| Replay и bulk | One-pair/no-repeat сохраняется; P12 replay70/30 — конфликтующий исторический режим. | R12 sample/step/episode; только явная migration R16 меняет policy. |
| 95% | Владельческая конечная цель к зафиксированному эталону сохраняется. | R13 rubric/uncertainty; не confidence/pass rate и не prerequisite первого урока. |
| Private / canonical | Project-owned артефакты не разрешают публикацию personal corpus. | R12/R16/R23 scope/portability/compatibility; R02 не публикует datasets. |
| Windows / подгонка тестов | R02 не доказал, что все runs лишние или все проверки подгонялись. | R09 реальные requirement/code/assertion/cost chains; R17 обоснованные изменения зависимостей. |
| Полный R01/R03 как gate | Доказанный subset используется без blanket DONE siblings. | R01 inventory, R03 routes, R10 union; чужие carriers не менять. |

Новые k/rank/beta/PC budgets, provider/license approvals и schema migrations здесь не выдумываются. Исторические NEW/READY/threshold не доказывают current implementation или пригодность пары.

## 2. Договор полезного урока

Следующие действия описывают целевой продуктовый цикл, а не команды на запуск в R02–R13. Сначала получить ответ текущего Student на новый ввод и сохранить domain/intent/language, представление, версию, путь и ресурсы. Локализовать слабость: mapping, representation, retrieval, solver или связь. **CODEX Astra 6 ultracode выбирает отдельную разрешённую учебную задачу.** Полный teaching packet: задача, matching saved weak Student response именно на неё либо один уже разрешённый изолированный ответ, разрешённый reference и допустимые прошлые examples с context-only IDs. Закрытые задачи/ответы/экзаменационные payload не входят.

**Китайская модель создаёт обе финальные стороны.** Источники context и авторство final sides различаются. **CODEX оценивает exact final pair**, не переписывает её и не подменяет gradient objective. При проекции необходимо доказать соответствие реально обучаемых bytes/recipient оценённому content; служебный WRONG tag не становится подсказкой студенту, а реальное отрицание не удаляется. Связать исходные authored sides, версию преобразования и фактический training payload; hash без проверки смысла перехода недостаточен.

Перед мутацией разрешить pending/applied attempt, загрузить совместимое принятое состояние, сохранить допустимую пару и полный pre-pair snapshot. Structurally valid candidate может сохраняться до semantic acceptance; missing/rejected/stale exact-content assessment блокирует optimizer, но не разрешает regeneration/replacement/второй update. После разрешённого изменения сравнить **одинаковые оценочные задания для обоих состояний, независимые от teaching families**, новые и старые способности и всю цену; точный bulk-протокол — §9/R13. При регрессе восстановить прежнее согласованное состояние; при uncertainty оставить прежнюю принятую модель; при invalid evidence исправить измерение, не эталон под ответ. Ordinary consumer использует принятую версию.

**Хороший пример, не dataset:** Chinese model получила разрешённую новую задачу перевода и matching Student response, создала chosen с сохранённым отрицанием и plausible rejected с конкретной ошибкой. CODEX оценивает окончательные стороны в разрешённом контексте; R05 прослеживает сохранение их смысла в проекции; utility выясняется на новых source families с numbers/placeholders/retention.

**Плохой:** CODEX пишет закрытый target, teacher перефразирует; hash исходника объявляют достаточным admission, а ответ на той же задаче — transfer. Другой плохой: Chinese model действительно создала обе стороны, но пара отклонена только из-за отменённого Student-only правила. Обратный обман — переименовать автора сохранённого Student response.

**Графовый пример:** одинаковый набор действий, но разный порядок prerequisite. Semantic order должен дойти до learning target, случайная позиция кандидатов — нет. Два равноценных допустимых пути дают TIE; более дешёвый предпочитается лишь по заранее заданной quality-equivalence/cost rubric. Авторство не превращает BOTH_BAD в качественную пару.

## 3. Общий модельный пакет и разделение нагрузки

Это дополнения **существующих Rxx**, не новая очередь. Dispatcher собирает один prompt: действующее поручение своего Rxx из LEARNING-WORK-ORDERS + относящаяся HYBRID-специализация + этот общий контракт + один раздел ниже. Не загружать все 25 задач/все C01–C11/весь raw R02 заново. Common contract включается и в реально делегируемую подзадачу, не заменяется ссылкой «сделай как выше».

**Опоры:** RV01/RV02/RV03 на planning `10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078`; P16 на `277959a20f2e6742d8caa51d08106e061e16a086`; core delta проверена предыдущим review на `a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3`. Прежние C01–C11 чтения имеют pin `731d2e55728caf601f05daaca7ab3fc370dd0240`; тот changed-file интервал их не менял. Это наследуемая кодовая опора, не новое чтение core этим ремонтом. Старые P/W/N сохраняют свои refs. Свежие проверки P07, RV03 и P16 на c19ebca4 перечислены в RUN-LOG. Перед работой прочесть относящуюся дельту, не утверждать full reread.

**Разрешено исследователям R02–R13:** source/code/существующие evidence read/search, документная/алгебраическая проверка, запись своего `D/replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}` с fresh SHA/readback. **Не разрешено этим дополнением:** product code/tests/workflows/core skills/weights/datasets changes, build/Windows/Actions/trainer/teacher/judge campaigns, cleanup/force-push/чужие shards. R16–R25 получают собственные последующие полномочия; исторические команды не исполняются.

**Skills — необходимые sections и dependencies, не список вместо чтения:** `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `generated-code-review-SKILL.md`/`code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`/`output-readability-gate-SKILL.md` в core/docs/skills. Для experiment owners — актуальные `experiment-design-SKILL.md`/`improvement-measurement-SKILL.md`. Исторические 30/50 reviews не активировать.

**Return packet:** понятные вход→решение→результат; exact repo/ref/path/span/blob; source/code fact отдельно от inference/unknown; current→target; артефакт; good/bad/edges; проверка документа сейчас; будущий различающий опыт и owner; неподтверждённое; точная передача; actual/requested model и independent reviewer либо NONE. Learning gain без опыта NOT_MEASURED.

**A/B — внутренние части одного Rxx**, не обязательные отдельные calls/отчёты. Ребёнок получает конкретные sources/allowed files/output/examples/verification и return-only authority. Одно существенное звено/конфликт на часть. При превышении контекста — checkpoint с конкретным remaining span/question/одним owner, не весь проект в R07/R10. Из пяти исторических P14 форматов глубоко прослеживается назначенный первый путь, остальные остаются именованными obligations.

### 3.1. Готовое поручение продолжения R02 без повторного общего аудита

**CURRENT TASK:** восстановить только доказанно недостающую связь «исходное ожидание → алгоритмический механизм → наблюдение → следующий владелец» в собственных пяти носителях R02. **NOT THE TASK:** выполнить R01/R03, закрыть все runtime gaps, выбрать новые k/rank/threshold, реализовать продукт или проводить обучение. Применяются полномочия, skills и return schema §3; отдельной подзадаче они передаются полностью.

**Контекст/RAG:** свои пять файлов; R02 в LEARNING-WORK-ORDERS-01-10 и HYBRID-ALGORITHMS-AND-RUNS; PARALLEL-R01-R03-JOIN; P01–P10/W01–W11 из SOURCES по конкретному вопросу; текущие §§2.1–2.2 RUN, не только пересказ P14. Путь P07 — `D/planning of testing.md`; RV03 — `D/09.09.26WorkLeft-RESEARCH-02-owner-decisions-resolution.md`, решения17–24 и many→many. SOURCE-ROUTES и опубликованные checkpoints siblings — advisory navigation. RAG-запись ведёт к primary span; один заголовок не доказательство решения. Идентичность = repo/ref/path/span/blob.

**A — требования и утраченная связь.** Сохрани существующие 52 ID и их primary основания. Прочитай относящиеся substantive spans, а не весь архив. Для конкретной дельты заполни: вход и requested output type; domain/intent/language; deterministic часть; learned decision/state; сигнал и training owner; ordinary consumer; что пользователь должен заметить; что опровергнет гипотезу. Сопоставь Unit, Flow, Orchestrator, Graph, Builder и три продуктовые линии; сохрани source-backed требования вне S04. Не превращай functional alias в новый API. Выход A — только изменённые строки/объяснения с источником и точным пробелом.

**B — пригодная передача.** Используй A, не открывай те же источники заново. Для каждого изменённого требования выбери одного lead по §11; дай ему исходный span, текущий code anchor либо честный unknown, bounded действие, пример правильного и неверного результата, крайний случай, способ документной приёмки и один различающий будущий опыт. Внутренняя часть имеет один вопрос и один основной артефакт; split нужен по разным mechanisms/owners, а не ради счётчика. Готовая достаточная дельта передаётся R04–R09 без ожидания полного join. Непоместившийся остаток получает exact span/question/owner/capacity, но не статус прочитанного.

**Хорошо:** для Flow показано, какие связи и корректирующие переходы меняются при fixed Unit, и как увидеть их вклад на прежнем input. **Плохо:** написаны слова hybrid/LoRA/DPO, но не назван изменяемый параметр, memory state или consumer; «восстановлены 52 строки» вместо доказательства охвата. **Крайние случаи:** несколько одинаково допустимых выходов; mixed SQL+BUG; одинаковая shape при разных координатах; historical role contradiction; новый source в параллельной ветке; источник недоступен. Приоритет текущей роли CODEX/Chinese не возвращается на голосование.

**Приёмка A/B:** обратный документный walkthrough от пользовательского результата к source, состоянию и владельцу; четыре уровня и три продукта не исчезли; новое требование не утонуло в старых ID; sibling shards не изменены. Отсутствие измерений — NOT_MEASURED. Результат — закрытый documentary delta и адресный handoff, не запуск следующих Rxx. Объединение R01–R03 остаётся в R10/J03; новый обязательный R26 не создаётся.

## 4. R03 — уникальная маршрутная дельта

**Контекст:** собственные routes, P06/P10/P11/P14, Q01/Q03, RV01/RV02/P16. **Skills:** startup/intent/source/plan/human.

**Промпт:** сохрани visited routes. Q02 теперь разрешён по конкретному пути и PE-1.1–PE-1.7; не ищи каталог заново. Добавь source-bound дельту и distinction старой P14 producer policy / действующего P16. Для оставшегося — root→index→primary→successor; code closure принадлежит R05, semantic catalogue reuse R08. Не трать свой RAG-проход на массовый поиск одного GraphDpoPair.

**Выход/проверка:** новые edges/refs/read-depth/remaining owner, не новый inventory. **Хорошо:** literal schema и отдельно superseded producer policy. **Плохо:** найденное имя или NEW означает отсутствующий код. **Edges:** одинаковые basenames разных roots, цикл, пустой extractor response большого файла, только исторический источник. Передай R08 точный каталог и ещё не прочитанные тела; не объявляй их завершёнными.

## 5. R04 — representation и many→many

**Контекст:** R02-001/007/027/035, C02/C03/C06, P12 D07, RV01 PE-3, RV02 ordered-side carrier; S00–S03. **Skills:** §3 + architecture/code.

**A:** trace raw input→tokenizer/parser→domain/stack features→vector/projection: producer/method/shape/coordinate version; сохраняемый смысл negation/numbers/units/placeholders/language/dependencies. **B:** trace requested-type consumer, materialization, many→many в обе стороны, empty/duplicate/repeated enumeration. B использует A, не перечитывает всю библиотеку и не создаёт новый LINQ provider.

Различай потерю входа и законно совпавшие ответы. Many→many не требует разных строк для семантически эквивалентных входов; требуется сохранить идентичность/связь входов с допустимыми выходами и не потерять разные обязательства. Контраст: два метода с одинаковой сигнатурой, но разными effects не должны получить одно обобщение с потерей отличия; два эквивалентных запроса могут иметь одинаковый правильный ответ. На доменном token passport фиксируй также reconstruction/decoding consumer либо явно отсутствие такого компонента; ближайший вектор сам по себе не является правильным выходным токеном.

**Выход:** source-bound port map, потеря/сохранение конкретного смысла и compatible-version contract R05/R06. **Проверка сейчас:** обратная трасса к исходному requirement и контраст. **Хорошо:** одинаковая shape не скрывает несовместимые пространства. **Плохо:** cosine/размерность доказывают семантику. **Edges:** OOV/Hebrew, schema drift, many→zero, схлопывание inputs, semantic versus candidate order. **Future R13:** fixed representation против одного изменения; downstream quality/retention/reindex cost, не coverage.

## 6. R05 — projected pair → objective → правильный recipient → consumer

**Цель:** показать, как проверенная CODEX Chinese-authored пара меняет нужное состояние Student, и найти smallest missing seam, не создавать второй trainer.

**Контекст:** R02-009/011–020/035/037/042/043/050/051; P14/RV02 §3.1–§3.2a; **P16 §§2.1–2.2 имеет приоритет по producing/assessment**; RV01 PE-1.1/PE-1.2/PE-1.4; C01/C06–C11. Точные реальные пути — SOURCES, не выдуманные DTO/API. P16 §2.3 даёт targeted вход `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs`, ResolveOrAuthorSinglePairAsync/ValidateTeacherProvenance/ParseTeacherOutput; актуальность call sites проверить. **Skills:** architecture/code/source/plan/prompt/human.

**A — выбранный путь пары.** Прочитай существующее описание/доступный сохранённый артефакт complete permitted teaching packet: task, matching Student response, reference, history context IDs. Если артефакта нет, установи конкретный missing owner/input; не создавай пару или Student response в исследовательском R05. Проследи Chinese output обеих сторон→canonical projection/marker handling→exact CODEX assessment binding→admission→lower item/batch→recipient. Сохрани author/source/version/context/side meanings. Выбранный solver ID, selector target, NodeOrEdgeId, source identity и state version не приравниваются. Отсутствие поля в DTO не доказывает отсутствия sidecar. Не импортируй отменённый Student-only negative.

**Адресное дополнение RC4:** найди actual consumer `margin >0.05`/LOW_CONTRAST или эквивалента. Откуда margin: expert preference, overlap, structural score или policy/reference log-ratio? Покажи достижимый call site/формулу. При отсутствии trace запиши PLAN_CONFLICT_RUNTIME_UNKNOWN, не production bug. Рассмотри на бумаге policy=reference; передай R12 точную зависимость, не предложение перепроверить весь DPO.

**B — мутация/состояние.** Установи server issuer и machine/trainer; probability полных serialized sides, masks/reduction, beta/reference freeze, optimizer/параметры. LoRA: W/A/B, inputs/outputs и downstream consumer; index/schema compatibility. Затем snapshot/pending/update/evaluate/accept-or-rollback/persist→ordinary inference. Unknown-running требует terminal state; новый ID не разрешает второй эффект. Сохранённый candidate с missing/rejected/stale assessment остаётся optimizer-ineligible; отказ не создаёт retry allowance.

Отдельно назови, что именно меняется: parameter/adapter update, kNN-memory insertion, graph topology или learned routing state. Полезный retrieval после записи примера не выдаётся за доказательство optimizer update; изменение весов без изменившегося consumer не выдаётся за обучение полезной способности. LoRA projection должна иметь входную/выходную систему координат, frozen/trainable matrices и заданный objective. Переобучение projection требует согласованной версии её потребителей; LoRA не объявляется tokenizer/codebook по одному названию. Референс DPO и accepted checkpoint — разные роли состояния; не сбрасывать reference к новой policy только ради красивого margin без отдельного решения R12.

**Выход:** producer/method→projection bytes/recipient→assessment→admission→objective→mutated state→ordinary consumer, current/gap отдельно; один missing seam, точный R12 question, один future no-update/update recipe. **Проверка сейчас:** обратная трасса от consumer к источнику; нет implicit arrow. **Хорошо:** verdict связан с exact final pair/context/target; служебный tag снят без потери NOT. **Плохо:** judge увидел annotated source, trainer получает другой смысл; judged hash вместо semantic validity; model reward заменён CODEX score.

**Edges:** legacy admission=null; stale content/context; TIE/BOTH_BAD; same text/different recipients; correlated fan-out; обе Chinese-authored sides действительно допустимы; history mislabeled as new pair; order lost by serialization; probability path отсутствует; initial policy=reference; stale index; rollback только A/B. **Future R13/R22:** те же evaluation families у сравниваемых состояний и disjoint teaching families, один разрешённый update, semantic/recipient quality, retention и вся цена. R07 получает лишь unresolved credit; R12 — конфликт; R18 — конкретный missing code seam. Не добавлять fixture-suite.

## 7. R06 — доменная функция kNN и полезный путь

**Контекст:** S07–S10; C03/C04/C10/C11, P12/P13, RV03 решение24 как route hint, R04 representation subset; R02 domain/path/consumer rows. **Skills:** source/architecture/code/plan/human.

**A:** найди actual domain×intent mapper, допустимый pool, features/normalization, metric/k, positive/negative/warning state, learned selector, arbiter и actual target execution. Prior signature cascade/vector neighbors/skill filter — source routes для проверки, не доказанные runtime функции. Сохрани BM25+kNN reference; проверь exact/ANN assumptions, version compatibility/reindex cost.

Для каждой предложенной domain-specific функции задай аргументы и результат: domain/intent, coordinate version, query features, допустимая память, метрика/веса, правило k/radius/abstain, constraints и runtime budget. Отдели фиксированную формулу от параметров, обновляемых по сигналу, и от простой смены memory entries. Не называй меняющееся k обучением без указания правила/состояния; не называй cosine универсальным доменным законом. Source/version правила мира или кода обязателен; conflicting/unknown rule не исправляется ростом similarity.

**B:** составь только различающие contrasts: actual/oracle-domain, fixed/domain metric, fixed/dynamic k, history off/on. Один фактор; interaction лишь по конкретной причинной гипотезе. Flow/Orchestrator: relation semantics, order, budgets, late/cancelled ветви и их цена. External consumer contract найден отдельно от его current implementation proof.

**Четыре уровня: обязательная передача наблюдаемого вклада, а не четыре новых исследования.** Используй готовый subset ведущего владельца. Для каждой строки назови actual state/signal/recipient; если его нет в доступном trace, запиши точный gap. Functional roles не заменяются четырьмя новыми physical models.

| Уровень / lead | Что остаётся фиксированным в различающем опыте | Что предполагается обучать / наблюдать | Пример опровержения |
|---|---|---|---|
| ILearningUnit / R04, update trace R05 | Domain/ports, допустимость, тот же representation contract и panel | Mapping входов в typed many→many outputs; downstream семантика, не только близость вектора | Учёт нескольких inputs исчез после materialization; loss упал, но обязательный effect потерян |
| ILearningFlow / R06 | Те же Units и задачи; те же обязательные constraints | Выбор/коррекция связей, последовательность и остановка correction loop; вклад при fixed Units | После улучшения отдельных Units исправляющий цикл по-прежнему ходит кругами либо теряет input |
| ILearningOrchestrator / R06 | Те же допустимые операции, данные и лимиты | Обучаемый выбор асинхронной комбинации; качество, полная latency/compute, cancel/late cost | Победившая ветвь быстрая, но общая цена speculative ветвей выше либо prerequisite не исполнен |
| ILearningDynamicGraph / R06, unresolved credit R07 | Совместимые паттерны и их базовые способности | Relations между паттернами разных порядков; перенос на новую композицию; адресат сигнала не равен случайному winner | Unit отвечает лучше, но выбор графа не меняется; успех знакомого пути ошибочно объявлен переносом higher-order relation |

Допустимый лучший путь — лучший **среди исследованных альтернатив при заданной рубрике и бюджете**, не доказанный global optimum. Сначала сравнить качество/допустимость, затем полную цену в заранее определённой области эквивалентного качества; fastest wrong не выигрывает. Learned-only comparator не означает отключение неизменных обязательных ограничений или прав доступа.

**Выход/приёмка:** mechanism→deterministic part→learned state/signal→owner→intervention→falsifier. **Хорошо:** SQL effect/domain constraint важнее близких слов. **Плохо:** universal cosine/k или warning как positive; fastest wrong path. **Edges:** empty compatible pool, mixed/unknown domain, stale warning, nonmetric/index mismatch, cycle, multiple valid outputs. **Future R13:** end-task quality/time/state size, не один retrieval hit. R07 получает только seams, R08 — rule/features, R13 — нужное сравнение.

## 8. R07 и R08 — разные владельцы, без повторного полного чтения

### R07: соединение механизмов

**Контекст:** R04–R06 готовые maps; R02-005/010/015/046; P14 ordered carrier/C11 recipient distinction только по unresolved edge. **Skills:** architecture/source/review/human.

**Промпт:** собери две допустимые combinations одного input. Для спорного соединения укажи semantic ports, fixed part, learned authority/state и recipient сигнала. Один intervention; bounded 2×2 только для обоснованного interaction. Не перечитывай все C01–C11, catalogue и slices. **Выход:** человеческая схема и source-bound gap, локальная гипотеза, falsifier, R12 decision либо R18 seam. **Хорошо:** fixed units/changing Flow даёт различимый end-task effect. **Плохо:** один общий reward всем рёбрам объявлен обучением графа. **Edges:** equivalent paths, incompatible fan-in, cancellation, higher-order recursion, no probability path. **Проверка:** обратная трасса learning signal. **Future R13:** curve/cost именно этой ступени, не только Unit.

### R08: WorkLeft baseline + восемь task types + семь subjects

**Контекст:** R02-014/016/021/022/024–026/028/036/052; P07/P10/P12/P13; W06/W11/Q01; RV01 exact catalogue и **P16 current roles/authorship**. Каталог: PE-2 orchestration, PE-3 translation, PE-4 server code, PE-5 client code, PE-6 server docs, PE-7 client docs, PE-8 stored procedures, PE-9 HTML-CSS; PE-10 consolidation. Использовать R03 resolved routes без ожидания полного R03. **Skills:** sources/architecture/prompt/review/experiment/human.

**A — reuse.** Прочитай относящиеся substantive PE bodies и RESEARCH-01. Раздели owner requirement, historical design, authored seed, actual data и measured evidence. Каждая задача получает obligation→PE/source span→negative families→baseline subject/language→reuse/adapt/do-not-import. Исторические DpoTrainReady/threshold/code-status/provider notes не current admission. RC1–RC6 уже найдены, а RC2 уже решён P16: не повторять discovery и не возвращать авторство владельцу вопросом.

**B — содержание.** Domain×intent×language passports сохраняют семь subjects и дополнительные задачи/языки; не требуют восьми физических моделей. Для каждого: source/version domain law, hard constraint versus preference, features representation/kNN, допустимые outcomes, meaningful positive/plausible negative/TIE/BOTH_BAD/unknown, source-family split, transfer/retention observation. Builder: pair validity отдельно от recipient utility. Это specification; actual train data не генерируется в R08.

**Примеры для постановки рубрики, не готовые пары и не Chinese-authored данные.** R08 связывает каждый с реальным разрешённым заданием/версией правила; R04/R06 получают features как гипотезы, не утверждённую универсальную метрику.

| Subject | Что различают representation/kNN и рубрика | Содержательный negative для будущего китайского автора | Наблюдение на новых families |
|---|---|---|---|
| EN↔RU | Смысл, отрицание, числа/единицы, placeholders и направление перевода | Беглая фраза с потерянным отрицанием или неверно перенесённым числом | Сохранность обязательного смысла и честный отказ при недостаточном контексте |
| Оркестрация программирования | Prerequisites, доступные skills, effects и полный budget | Правдоподобный план с пропущенной зависимостью или неоправданной дорогой ветвью | Допустимость и качество результата плюс полная цена пути |
| Figma→HTML+CSS | Tree/layout constraints, assets и несколько заданных разрешений | Красивый один screenshot при нарушении исходного дерева/поведения на другом разрешении | Качество на новом layout family, адаптивность и стоимость |
| C# documentation | Реальные signatures, effects, async/error/cancellation semantics | Гладкая документация с выдуманным effect или потерянным исключением | Верность новому коду, не совпадение с одной эталонной фразой |
| Angular documentation | Компоненты, inputs/outputs, связи с API и version-specific lifecycle | Описание UI без существенного binding или с неверным действием API | Верность связи компонент→сервис→результат на другом source family |
| SQL documentation | Dialect/version, parameters, NULL/cardinality/effects/transaction context | Текст с правильными именами таблиц, но неверным effect или условием JOIN | Объяснение «что» и «зачем» с подтверждением из кода, abstain вместо выдумки |
| Builder для остальных шести | Recipient domain/level, реальная предпочтительность и происхождение sides | Пара с переставленными labels, двумя неверными или равноценными сторонами | Отдельно качество допущенной пары и полезность урока для Student |

Дополнительные server/client code, stored procedures, HE/другие найденные языки не поглощаются этой стартовой семёркой. Mixed-domain задача получает несколько координат/неопределённость, а не принудительный единственный label. Для evolving rules сохранить версию и проверить drift; «законы мира» не выводятся из частоты токенов корпуса.

**Хорошо:** новое SQL описание различает parameters/shape/effects; без source честный abstain; tiny NOT change — значимый negative даже при high overlap. **Плохо:** всё свели к C#; old synthetic SQL объявлен production corpus; количество строк заменило breadth. **Edges:** Hebrew/OOV, client без API wiring, conflicting feedback, меняющееся правило, минимальная ошибка, несколько valid paths, correlated fan-out, privacy scope. **Проверка сейчас:** все 8 task types и 7 subjects имеют применимый passport либо exact missing source + один owner; содержательный counterexample, no invented measurements. **Future R13/R21/R22:** одинаковые panels до/после, transfer/retention/цена. R04 features, R05 pair content constraints, R06 rules/metric meaning, R13 rubric, R21 reuse-ready curriculum.

## 9. R12/R13 — анализ, а не повторная разведка

### R12: admission/objective и действительные architecture gaps

**Вход:** exact maps R04–R08; RV01 PE-1.2 п.5 и RV02 §3.2a; P16; RC1–RC6. **Skills:** architecture/planning/review/experiment/human.

**A:** рассмотри три контрпримера: policy=reference; правильная пара, где Student ещё предпочитает rejected; сомнительная пара с positive model margin. Отдели semantic quality CODEX от log-ratio model state. Используй actual consumer от R05. Выбери и обоснуй admission и последующую роль margin без circular gate; не подмени DPO objective экспертным score. Кандидат решения: quality/lineage-based admission, margin как model diagnostic; иной contract требует явного обоснования и constraints. Запрещены «разберёмся при реализации» и порог, подобранный под текущую пару. Неизмеренные числа не изобретать.

**B:** по достаточным maps разреши recipient/ordered preference, standard DPO или именованную альтернативу для отношения, sample/step/episode против replay, personal/common и online/idle/campaign boundaries. **Не открывай снова RC2: Chinese author обеих сторон уже установлен.** Не создавай второй issuer/поле15 по одному слову. Для выбора/отклонения: rationale, invariant, cost, falsifier и exact R16/R18/R20 carriers. Уже делегированный CODEX выбор не возвращается пользователю.

**Проверка сейчас:** бумажная трасса success/counterexample для принятого маршрута; ни одного скрытого звена в «future gate». **Future:** один различающий recipe R13/R22 и smallest missing seam R18, не весь renderer/IDE/security backlog. Независимая Astra Ultra экспертиза candidate — R15, не самопроверка R12.

### R13: сопоставимый bulk и наблюдение улучшения, а не заранее положительный ответ

**CURRENT TASK:** подготовить исполнимое описание первого различающего опыта и расширения по доменам/уровням. **NOT THE TASK:** запускать его сейчас, создавать новые product tests или доказывать преимущество hybrid словами. **Вход:** concrete outputs R04–R08, R12 choices, WorkLeft donor/rubric evidence; P07 и RV03 решения17–24, особенно learning curve каждой ступени. **Skills:** experiment/improvement/source/review/human; полномочия/return schema §3. Выход остаётся в пяти носителях своего R13; R22 получает recipe, R18 — smallest missing implementation seam.

**A — паспорт опыта и управление вмешательством.** Укажи domain×intent×language×level, исходный вопрос, source/version предметных правил, допустимые результаты и baseline/challenger. Назови изменяемую функцию/параметры/память и всё удерживаемое постоянным; source family является единицей split и зависимости, а не каждый fan-out loser. Зафиксируй реальные hardware/runtime/model/coordinate/index versions либо точный unresolved input. До измерения определить rubric, sampling, budget, uncertainty/sufficiency, acceptable regression и decision rule; числовой порог требует обоснования R13, не придумывается R02. Неопределённость означает INCONCLUSIVE, а не перенос рубрики под ответ.

Сравнения выбирать по вопросу, не запускать весь декартов перебор до первого урока: deterministic reference / learned policy / hybrid при одинаковых обязательных constraints; actual-domain / допустимый expert-domain при том же solver; fixed/domain metric при том же k; fixed/dynamic k при той же metric; memory-only / parameter update при согласованном остальном состоянии; frozen/адаптированная LoRA projection с тем же downstream consumer; fixed Units / изменённая Flow либо Orchestrator/Graph. Если одновременно меняется несколько частей, вывод только о пакете изменений; для attribution нужен отдельный bounded contrast, а не выдуманный вклад каждого блока. Не обязан существовать корректный learned-only вариант любого механизма: неприменимость обосновывается, а не реализуется небезопасный comparator.

**B — последовательность наблюдений.** Bulk B_j — учётная группа **новых разрешённых уроков**, а не разрешение mini-batch/replay/повторного optimizer effect. Сохраняется one-pair/no-repeat; R12 уточняет допустимую update semantics. Для каждого j записать входное accepted state S_(j-1), разрешённые unique teaching families/пары, attempted/admitted/applied/rejected counts, trial и принятое состояние S_j. Ноль допустимых updates — результат admission, не доказательство «обучение не работает».

Сравни S_(j-1) и candidate S_j на **одних и тех же evaluation identities и рубрике**; teaching families отделены от evaluation families. Для cumulative gain сравни S_0 и S_j на общей сопоставимой панели. При новом panel version перепроведи доступные сравниваемые frozen states на новом panel либо оставь ряды несопоставимыми: качество на всё более лёгких задачах не learning curve. Отдельно показывай retention на старых навыках и transfer на новых families/композициях. Несколько runs/seeds назначаются только по оценённой шумности и цене; один запуск не обосновывает статистическую уверенность.

Online feedback оформляется отдельным prequential stream: сначала сохранить prediction/version/cost на новом вводе, затем рассматривать коррекцию и допустимое обучение, затем оценивать следующие невиденные inputs. Коррекция не автоматически истина. Уже использованный для обучения ввод не становится held-out экзаменом; adaptation/selection по operational panel не называется untouched certification. Закрытая certification panel не передаёт payload/targets в teacher, task selector или pair assessor. Собственный Student evaluator вправе использовать предназначенные ему закрытые references внутри разрешённой evaluation boundary; запрет утечки не означает запрет оценщику видеть свой эталон.

**Раздельные оценки:** (1) provenance/structural admission; (2) CODEX Astra 6 ultracode — корректность и реальная предпочтительность exact Chinese-authored pair; (3) Student utility на новых задачах. Pair judge и Student evaluator — разные роли и контексты, даже при одинаковом семействе модели. Expert review R15/R25 — ещё отдельная экспертиза метода/архитектуры. Не выдавать самопроверку автора за независимую Astra Ultra экспертизу. Для сравнения ответов по возможности скрывать variant identity и менять случайный порядок предъявления; это не разрешение менять смысловой порядок шагов внутри ответа.

**Одна содержательная строка результата на domain/level/bulk:** refs и state IDs; panel/family counts; rubric quality с неопределённостью, допустимость/abstain, domain mapping/ошибки по типам; pathway effect; incremental и cumulative изменения; retention; attempted/admitted/applied lessons; wall-time и compute всех ветвей, p50/p95 при достаточной выборке; peak RAM/VRAM, disk/model/adapters/optimizer/index/graph/cache/snapshots; author/judge/token/remote cost, failed/cancelled/reindex cost; verdict и основание. Отсутствующие значения — NOT_MEASURED/INSUFFICIENT_SAMPLE, не нули. Сравнение PC-эффективности отделяет локальный inference/update от внешней генерации и экспертизы; маленький adapter не доказывает, что весь цикл помещается на PC. Общая средняя не скрывает худший/редкий домен.

**Хорошо:** на тех же новых SQL families улучшается объяснение effect без потери NULL semantics; полный memory/cost budget учтён; знак добавки следующего уровня измерен, даже если он отрицательный. Качественная пара без достаточного evidence Student gain даёт pair-quality accepted и utility INCONCLUSIVE — два разных результата. **Плохо:** новый bulk легче, loss/hash/model margin вырос, а это объявлено ускорением обучения; победившая ветвь бесплатна, проигравшие не учтены; все критерии меняются одновременно; весь прирост memory retrieval назван LoRA learning.

**Edges:** TIE/BOTH_BAD/UNJUDGEABLE, conflicting feedback, rare/worst domain, equivalent correct outputs, correlated fan-out, repeated adaptive evaluation, unknown/cross-domain input, coordinate/index drift, late/cancelled branch, pending update, partial rollback, no novelty, invalid evidence. Verdicts: IMPROVE, REGRESS, INCONCLUSIVE, INVALID_EVIDENCE; pair admission имеет собственный статус и не сливается с ними.

**Проверка результата R13 сейчас:** документный dry walkthrough одного no-update/update случая и одного misleading-progress контрпримера; явные inputs/outputs/owners/versions; ни одного implied evaluation→training leak; следующий R22 может исполнить recipe без придумывания метода, а R18 знает конкретный недостающий seam. A/B — части одного R13, не дополнительные сессии. Более широкий curriculum сохраняется по donor dependencies, но полнота всей матрицы не новый предварительный запрет на первый отдельно разрешённый узкий урок. Вывод о преимуществе hybrid и PC пригодности остаётся гипотезой до измерений.

## 10. Места будущего применения, без правки чужих shards сейчас

| Носитель | Правка / owner |
|---|---|
| W03/W04 Qwen-as-judge; W02/PROMPTS paid orientir | R16/R20: CODEX обе роли, Chinese обе стороны, отдельные context origins; weakness/rubric не closed target. |
| P14 summaries/AGENTS/README/H02 с «GraphDpoPair с UnitId» | R16/R18/R20: literal NodeOrEdgeId/target и actual bridge; semantic order без выдуманных DTO. |
| RV01 PE-1.1/PE-1.2/PE-1.4/PE-1.5 и reused prompts | R16/R20: RC1–RC6 по актуальным решениям; historical original не выдавать за всегда правильный active contract. |
| LOW_CONTRAST/margin actual consumer | R05 trace→R12 решение→R16 requirement→R18 implementation packet; R13/R22 опыт. Исправить actual owner, не только prose рядом. |
| Source→projection→exact pair judge→trainer | R05 trace; R12 binding final pair/context/recipient с P16; R18 narrow seam; R20 prompt. Без автоматического расширения schema/permits. |
| DPO-last / blanket Windows prerequisite | R09/R11 evidence→R17 policy→R19 roadmap→R20 dispatcher. DTC/IDE obligations сохранить с trigger возвращения. |
| Replay70/30, bulk/fan-out, snapshot/private | R12 решение; R16/R18/R23 реальные owners/lifecycle. Не скрытый повтор под новым ID. |
| Bulk comparability / four-level effect / evaluator boundaries | R13/R22 используют §9, R20 встраивает в исполняемый prompt; R23 измеряет полный PC footprint. Не новая очередь и не prerequisite из всех доменов. |

R21 curriculum, R22 experiments, R23 PC/feedback/state/resources, R24 visible integrations, R15/R25 independent Astra Ultra exact-candidate review. Без дополнительных неназванных 30/50 проходов.

## 11. Один ведущий исследователь на требование

`research_owner` в CHANGE-MAP перечисляет участвующие Rxx, а не приказ всем перечитать одну область. **Для dispatch ведущего определяет только таблица ниже.** Остальные перечисленные Rxx — получатели конкретного subset; `apply_owner` — последующее применение. Это расшифровка существующих 52 строк, не новая очередь и не второй набор requirement IDs.

| Lead | R02 IDs, префикс R02- опущен | Граница |
|---|---|---|
| R04 | 001, 007, 027 | Representation/typed multiplicity; не весь trainer. |
| R05 | 009, 011, 012, 013, 017, 018, 019, 020, 035, 037, 042, 043, 050, 051 | Pair/update/state chain; rubric от R08, architecture decision от R12. |
| R06 | 002, 003, 004, 006, 008, 029, 030, 031, 033, 034, 038, 039, 040 | Domain/path/consumer owners; не semantic audit всего corpus. |
| R07 | 005, 010, 015, 046 | Только hybrid/credit/Builder seams на готовых maps. |
| R08 | 014, 016, 021, 022, 024, 025, 026, 028, 036, 052 | Domain/data содержание; runtime у R04–R06, statistical design у R13. |
| R09 | 023, 032, 041, 048 | Ресурсы/цена/зависимости; без новых Windows runs. |
| R01 | 044 | Source inventory; R03 — маршруты, R10 — union. |
| R10 | 045, 047 | Сведение gaps/reuse, не повторный code audit. |
| R02 | 049 | Собственный parallel-safe handoff, не sibling edits. |

Все 52 ID распределены по одному lead. R12/R13 анализируют результаты, R14 зависимости; не получают весь непрочитанный проект как новый discovery. Новый независимый источник меняет конкретные affected rows, не сбрасывает весь R02.

## 12. Приёмка ремонта и параллельная публикация

Документные walkthroughs: Chinese model — author обеих сторон, Student — context/learner; RC2 не открывается заново; policy=reference не доказывает плохую пару; WRONG metadata отделена от semantic NOT/order; two losers→two attempts, не auto updates; graph-level цель достигает правильного target unit; 8 task types не заменяют 7 subjects/3 products; каждый ID имеет одного lead; R01/R03 не переписаны и не объявлены DONE.

Дополнительно: одинаковые правильные ответы не запрещены many→many; frozen/current состояния сравниваются на одинаковых evaluation identities; новая panel не имитирует progress; memory/parameter/graph updates различены; full speculative/reindex/external cost не скрыт; pair assessor не получает closed targets, а Student evaluator сохраняет свою разрешённую reference boundary. Весь curriculum и полный факторный эксперимент не превращены в новый gate первого урока.

Публикация: actual commit/blob/readback, fresh SHA при write; на 409 прочитать чужую дельту и сохранить её, не force overwrite. Самопроверка — не независимая Astra Ultra экспертиза, source reading — не runtime test, plan quality — не measured learning gain. Исследовательские дополнения не утверждают, что последующие Rxx уже выполнены. R02 review заканчивается документным результатом с адресной передачей, без нового coverage/Windows gate.
