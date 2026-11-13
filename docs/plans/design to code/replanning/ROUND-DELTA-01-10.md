# R01–R10: адресные дополнения к исполняемым промптам

**13.10.2026 · CP-B + R02-ACTIVATION-REPAIR. Это поручения, не выполненные R01–R10.** Исправление R02 из опубликованного результата теперь включено в действующее дополнение, которое загружает START-NEXT-RUN. Это правка исполняемого плана, не новый исследовательский проход.

## Как выдать один полный пакет модели

Этот файл не создаёт вторую очередь. Согласно [START-NEXT-RUN §3](START-NEXT-RUN.md), для назначенного Rxx dispatcher соединяет общую часть и сам Rxx из [MODEL-RUN-PACKETS-CP5-01-10](MODEL-RUN-PACKETS-CP5-01-10.md) и только соответствующее дополнение ниже. [LEARNING-WORK-ORDERS-01-10](LEARNING-WORK-ORDERS-01-10.md), HYBRID и RECOVERED сохраняются как предметные источники: включаются только ещё не перенесённые уникальные обязательства, не вторая полная базовая карточка. Повторяющиеся требования сводятся; уникальные не удаляются. Пользователь получает готовый prompt, а не обязанность склеить документы. Уже запущенные R01–R03 принимают только относящуюся дельту на естественном checkpoint; подтверждённое чтение не повторяют.

Обозначения: D = `llm mvp/design to code session` в `lubochka/xiigen-planning:llm_plan3`; K = `docs/skills` в `lubochka/llm_mvp_core:codex/dpo-implementation`; E = [LEARNING-ACCEPTANCE-DELTA](LEARNING-ACCEPTANCE-DELTA.md). Все технические пути привязываются к actual commit/blob/span, не к воспоминанию о ветке. RAG является адресом перехода, не свидетельством реализации. Зафиксируй полный источник там, где изменяешь файл; чтение тематического excerpt не называй чтением всего файла.

**Общие границы каждого дополнения:** выполнять только назначенное исследование; не создавать/запускать product tests, coverage, Windows/Actions, trainer, установки, платные генерации; не менять code, weights, datasets, core skills или результаты соседей. Публиковать содержательные additions в своих существующих пяти носителях `results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Не создавать дубликаты carriers на каждый подпункт. Проверять актуальные bytes/SHA перед записью и фактический remote readback после. NOT_MEASURED не заменять оценкой из головы.

**Skills в каждом effective packet:** относящиеся разделы `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `output-readability-gate-SKILL.md`; по задаче добавляются названные ниже. Записать found/read/missing, actual ref и глубину, а не просто список имён. Старые 30/50-review и all-green правила других задач не расширяют текущую программу. Человеческая форма результата: «Что система должна уметь; что подтверждено; что пока неизвестно; какое одно следующее действие теперь возможно». Техническая трасса идёт после объяснения.

**Размер и настоящие подзадачи.** A/B ниже — две последовательные части одного Rxx, если нет реального делегирования. Делегированный child получает целиком: текущую общую часть, границы parent Rxx, собственный A/B prompt, перечисленный source subset с разрешёнными spans, один результат, good/bad/edge и приёмку своего подпункта. Выход child — return-only patch/findings; запись делает parent. Нельзя передать ребёнку только фразу «смотри R07». Если source subset не помещается, parent делит его по конкретным вопросам/связям, записывает объём и получателя; не скрывает новый вызов и не перекладывает всё на R10. Дополнительный необходимый round оформляется по PLAN-25-RUNS, а не возникает автоматически.

---

## R01 — обнаружить весь охват, не объявляя каталог прочитанным

**Контекст/refs:** собственные results/R01; реальные деревья planning/core; SOURCE-ROUTES; WorkLeft INDEX-01, REDATE-01, RAG-MASTER-INDEX/usage-guide; previous rounds thinking results.md; базовый R01. **Skills:** reference-inventory-and-source-map, phase-planning. **E:** §1–2. **Зависимости:** R02/R03 не prerequisites.

**Промпт A — инвентарь и идентичность.**
> Восстанови уже сделанную часть своего R01 по точным source refs. Обойди дерево всей относящейся программы, включая найденные связанные корни, все варианты WorkLeft/LeftToWork, RAG/local-RAG/MD/NDJSON, общие prompts, S00–S10 и области вне слайсов. Сохрани repo/ref/path/blob и различай discovered, structure-read, content-read, primary-traced. Укажи, было ли дерево полным или ответ truncated; при truncation продолжи по поддеревьям, а не объявляй отсутствие. Отметь ссылки на другие root и одинаковые имена разных файлов. Выход A — содержательный раздел SOURCES с границами обхода и адресным остатком.

**Промпт B — нагрузка и сохранение обязательств.**
> По результату A назначь каждому обязательному источнику/разделу одного основного владельца. Свяжи источники четырёх ролей, исходных доменов, DPO, vectorization, kNN, LoRA, compactness и feedback с R02–R09. Для смешанного документа выдели конкретные смысловые диапазоны; не назначай всем весь файл. В результате B перечисли необслуженные обязательства и capacity: кто, какой вопрос, какие spans, что уже известно, что осталось. Новая ссылка не означает новый review round. Источник после cutoff добавляется адресной дельтой.

**Хорошо:** одинаковые `decisions.md` в двух корнях имеют разные source IDs; representation читает R04, update — R05, graph — R06. **Плохо:** «S04 и skills просмотрены, значит R01 завершён»; «остальное исследует R10». **Края A:** truncated tree, циклическая ссылка, удалённый/moved файл. **Края B:** новый домен вне слайсов, большой единственный первоисточник, источник нескольких тем.

**Приёмка A:** заявленный охват сверяем с деревом/разрешёнными страницами; UNREAD не исчезает. **Приёмка B:** каждый обязательный остаток имеет конкретного владельца и посильную часть; ни один перечень не выдаётся за чтение. **Функциональная передача:** карта ведёт к механизмам и будущим доменным опытам; никаких выводов об accuracy. R02/R03 могут немедленно использовать проверенные части без общего барьера.

## R02 — сохранить исходные решения и не смешать роли с текущими классами

**Контекст/refs:** original task.md, task prompt.md/.txt, goal of the round.md, Алгоритм создания ДПО.md, Изменения в плане работы - цель.md, Claude session decisions.md; SOURCE-ROUTES §2/4; WorkLeft owner-decisions и исходные results/R02. **Обязательная уже опубликованная опора:** [R02 DECISIONS на 78ed05e5](https://github.com/lubochka/xiigen-planning/blob/78ed05e5cf8673aa803a25b12c743d0963d23335/llm%20mvp/design%20to%20code%20session/replanning/results/R02/DECISIONS.md), blob `a8bcaa83e78eef2f22db3febc3dc5f2e8ed07962`, именованные §§0,3.1,5–9. Это источник конкретной дельты, не приказ каждому читать весь R02 или повторять его исследование. Новую редакцию сверять по относящимся изменениям. **Skills:** authority-requirement-binding, trainable-architecture-decision, prompt-patch-authoring; source/planning/review/code-review/human skills — из общей CP5 части по фактически затронутым вопросам. **E:** §2–3,6,9. **Зависимости:** прямые источники; готовые R01/R03 не требуются.

**Промпт A — source-preserved expectations.**
> Из текущей постановки и первичных решений восстанови обязанность, а не удобную новую архитектуру. Используй существующие 52 requirement IDs; не создавай повторный реестр вместо устранения конкретного пробела. Для Unit зафиксируй domain/vector/output schemas и typed many→many; для Flow — отношения и correction; для Orchestrator — async альтернативные комбинации; для DynamicGraph — отношения паттернов разных порядков, Builder и teaching target. Добавь конечные DTC/IDE/consumer и все доменные обязательства. Для каждой изменённой строки дай exact source/authority, scope, действующее/историческое/конфликтное состояние и цепочку: input/requested output type → deterministic mechanism → learned decision/state → signal/training owner → actual consumer или точный NOT_TRACED gap → наблюдаемый результат → falsifier. Имена интерфейсов и число строк не доказывают реализацию. Выход A — дельта прежней карты требований с источниками и алгоритмическим смыслом, без придуманных C# signatures.

**Промпт B — различие намерения, owner и реализации.**
> По A отдели логическую роль от физической модели и владельца обучения. Применяй уже решённые роли: CODEX с Astra 6 ultracode выбирает учебную задачу вместо человека; открытая китайская модель создаёт ОБЕ окончательные стороны chosen/rejected; CODEX с Astra 6 ultracode отдельно оценивает качество exact pair; Student учится. Слабый Student response — разрешённый контекст, не обязательный final rejected; CODEX не переписывает стороны под китайским авторством. Сохрани deterministic helper versus learned choice, positive/negative/warning memory, final arbiter, one-pair/no-repeat и полный accept-or-rollback. Если требование оценивает graph, а пара адресует unit, различи literal `NodeOrEdgeId` в 14-field плановом каноне, выбранный solver и actual target обучаемого selector. Не утверждай наличие поля `UnitId` в lower LearningTrainingItem; реальный manifest/sidecar/recipient bridge прослеживает R05. Не удаляй graph learning и не добавляй Train на все интерфейсы. Передай только настоящий незакрытый конфликт R05/R12; авторство обеих сторон не переоткрывать. Выход B — bounded handoff: requirement/source span → механизм/пробел → одно различающее наблюдение → один ведущий owner, без выполнения его исследования.

**R02-ACTIVATION-REPAIR — содержание, которое dispatcher переносит в соответствующие задания.** Это применение опубликованного ремонта, а не новый набор проходов. Для делегируемой части включать её исходный контекст, этот относящийся пункт, конкретный результат, good/bad/edge и проверку; не передавать одну ссылку.

| Получатель / опора в R02 DECISIONS | Обязательное содержание effective prompt и результата |
|---|---|
| R04 / §5 | Различить semantic collapse и законно одинаковые ответы; сохранить связь inputs→typed outputs. Разделить tokenizer/encoding/projection/decoder, назвать coordinate version и downstream consumer. Контрпример: одинаковая shape при несовместимом смысле. |
| R05 / §6 | Читать и прослеживать существующий teaching packet, не генерировать его в исследовательском проходе. Связать authored sides→projection→exact CODEX assessment→admission→правильный recipient→update→ordinary consumer. Разделить memory insertion, parameter/LoRA update и graph/routing state. Model margin до обучения не подменяет semantic pair quality; RC4 требует actual consumer trace, а не нового выбора автора. |
| R06/R07 / §7 | Для kNN назвать domain/intent, features, допустимый pool, metric/weights, k/radius/abstain, learned state/signal и цену индекса. Для Unit/Flow/Orchestrator/Graph отдельно назвать fixed parts, изменяемое состояние, наблюдаемый вклад и опровержение. R07 исследует только неразрешённую связь; fixed Units/changing Flow не повторяет полный аудит Units. |
| R08 / §8 | Сохранить семь baseline subjects, восемь task types и extras. Паспорт: source/version доменного правила → признаки → meaningful positive/plausible negative/TIE/BOTH_BAD → новые families/retention. Примеры в документе не выдаются за Chinese-authored dataset. |
| R13/R22 / §9 | Передать сопоставимые bulk observations: те же evaluation identities и rubric для сравниваемых states, disjoint teaching families, incremental/cumulative gain, retention/transfer и uncertainty. Новая более лёгкая panel не progress. При смене panel сравнить доступные frozen states на общей панели либо признать ряды несопоставимыми. Это передача метода в существующие задания, не выполнение опыта в R02. |
| R09/R23 / §9 | Учитывать весь PC footprint, optimizer/index/cache/snapshots/reindex, неуспешные/отменённые ветви и внешние author/judge costs. Online input: prediction/version/cost до рассмотрения коррекции, затем наблюдение на следующих невиденных inputs; correction не автоматическая истина. |

**Хорошо:** «Flow обязан научиться полезной коррекции; его state прослеживает R06; сравнение меняет только Flow при fixed Units и тех же задачах». Также допустимы одинаковые правильные ответы на эквивалентные inputs, если не потеряна связь с ними. **Плохо:** «есть ILearningUnit, поэтому все четыре роли реализованы»; «старый файл исторический, значит решение отменено»; «repair опубликован только в results, значит effective prompt уже исправлен». **Края A:** SQL+BUG как разные оси, несколько верных выходов, неизвестные domain laws/coordinate versions. **Края B:** переименованный ILogical*; конфликт user decision и старого пересказа; stale pair assessment; параллельный source delta; непроверенный recipient.

**Приёмка A:** обратная трасса к прямой постановке не теряет обязательство вне S04; публикация старых результатов не называется новым исследованием. **Приёмка B:** конфликт имеет обе стороны, consequence и точный owner, а не голосование отчётов. Документный walkthrough различает good Chinese pair без доказанного Student gain, memory-only улучшение без parameter update и лучшую отдельную ветвь с худшей полной стоимостью. **Проверка включения ремонта:** effective prompt основан на CP5 и содержит относящиеся пункты таблицы, а не только ссылку на results; исходные sibling checkpoints сохранены. Это проверка текста/передачи, не новые product tests или Windows gate. **Функциональная передача:** R12/R16 — source-preservation/решения; R13 — различающий опыт; R18 — smallest missing implementation seam по тематическим traces. Полная доменная матрица не предварительное условие первого отдельно разрешённого узкого урока. Качество, скорость и компактность остаются NOT_MEASURED до фактического опыта; независимая Astra Ultra экспертиза не подменяется самопроверкой.

## R03 — закончить маршруты RAG к содержательным первоисточникам

**Контекст/refs:** все собственные опубликованные R03 checkpoints; WorkLeft RAG-MASTER/usage, RAG-01..04 MD+NDJSON, RAG-DELTAS, TASK-PROMPTS-CHRONICLE/COVERAGE и найденные вложенные индексы. Начальный восстановленный путь: `visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md` → `llm dynamic flow`; подтвердить его actual ref. **Skills:** reference-inventory-and-source-map, authority-requirement-binding. **E:** §1–3,5–6. **Зависимости:** независимый старт.

**Промпт A — адресуемость и первичный смысл.**
> Продолжи собственный visited-set, не перечитывая уже подтверждённые неизменившиеся blobs. Для каждой найденной текущей RAG-ветви проследи declared source root, target, successor и критические primary spans. Parse NDJSON доказывает адресуемость, не смысл. Выход A — маршруты question→RAG row→primary decision и точный unvisited frontier. Нельзя закрыть ветвь одной удобной записью, оставив относящиеся критические строки непрочитанными.

**Промпт B — восстановленные алгоритмы без повторной изобретательности.**
> Выдели в прослеженных источниках положительную/отрицательную/предупреждающую память, domain-first retrieval/BM25+kNN, final arbiter, TensorCore/shared base/project adapters, detector-before-normalization, TeachingVector/Context и адрес получателя (literal NodeOrEdgeId в 14-field плановом каноне; actual lower bridge устанавливается отдельно), graph-to-unit teaching и update/recovery policy, когда эти решения действительно содержатся в источнике. Для каждого дай quoted span, статус преемника, affected producer/consumer и owner R04–R08. Отдельно отрази реальный конфликт replay versus one-pair; не выбирай policy по частоте упоминаний. Выход B — source-backed recovered decisions плюс ограниченная очередь primary gaps.

**Хорошо:** обнаруженная в IDE RAG библиотека dynamic flow сохраняет её собственный root и первичное решение арбитра. **Плохо:** universal cosine и replay придуманы вместо найденных решений; отчёт R03 используется как доказательство работающих весов. **Края A:** устаревшая абсолютная Windows-ссылка, alias, цикл. **Края B:** одновременно существуют отменённая и действующая политики; требование найдено в старом приложении.

**Приёмка A:** все заявленные critical transitions можно повторить по source identity. **Приёмка B:** recovered ≠ implemented; одно решение не потеряно при дедупликации. **Передача:** R04–R09 получают готовые тематические spans сразу; R10 — версии и настоящие конфликты, не обязанность повторить весь traversal.

## R04 — representation и Unit: доказать сохранение смысла, а не размер вектора

**Контекст/refs:** базовый R04; S00–S03; WorkLeft 01-code-changes, 05-branch-reconciliation, SOLUTIONS-01, VERIFY-01/02; R02/R03 тематические checkpoints; actual core symbols через code RAG. **Skills:** code-examination, generated-code-review, trainable-architecture-decision, experiment-design. **E:** §2–5,7. **Зависимости:** достаточные тематические источники, не DONE всех R01–R03.

**Промпт A — реальная карта преобразований.**
> Проследи raw input→stack/domain/intent detection→normalization/tokenization→encoding/projection→typed Unit output→real consumer. Для каждого этапа укажи владельца, schema/coordinate version и какие сведения могут теряться. Сохрани исходный stage-order, если он подтверждён; одинаковая длина вектора не доказывает совместимость. Найди существующую реализацию/дубликат прежде нового механизма. Выход A — method map с exact file/symbol/span и отдельными NOT_TRACED разрывами, включая multi-input/multi-output.

**Промпт B — examination card для представлений.**
> На source-supported pilot domain создай исполнимый будущий recipe: meaning-preserving transformation; похожий текст с изменённым решающим смыслом; oracle/predicted domain; fixed Unit/consumer. Определи baseline representation и один challenger, разрешённые данные, независимые families, semantic output rubric, стоимость encoding/reindex и falsifier. Затем укажи, какую одну минимальную code seam надо реализовать, если сравнение ещё невозможно. Выход B — recipe, который различает routing failure, representation failure и solver failure без новых unit tests.

**Хорошо:** переименование bound variable сохраняет binding, а перенос переменной в другой scope меняет ожидаемый смысл. **Плохо:** shape и cosine объявлены семантической правильностью. **Края A:** неизвестная версия, nullable/type mismatch, non-text domain. **Края B:** обе версии одинаково плохи; несколько валидных выходов.

**Приёмка A:** есть потребитель полученного вектора, не только producer. **Приёмка B:** смена одного фактора и контрпример позволяют отклонить challenger; измерения не выдуманы. **Передача:** R05 получает actual projection/update touchpoints; R06 — index compatibility; R13 — первый representation recipe.

## R05 — DPO/update/LoRA: связать предпочтение с действительным изменением состояния

**Контекст/refs:** базовый R05; S04–S06; WorkLeft 02-teaching-chinese-model, INTEGRATION-01-training-into-work-program, TASK-PROMPTS-TRAINING; Алгоритм создания ДПО.md; R02 policy/R04 projection; actual trainer/consumer через core RAG. **Skills:** code-examination, trainable-architecture-decision, experiment-design. **E:** §6–8.

**Промпт A — preference-to-state trace.**
> Проследи китайского автора обеих sides→candidate pair→exact CODEX Astra 6 ultracode assessment/admission→проекцию и actual recipient bridge (NodeOrEdgeId в плановом каноне; TeachingVector/Context/lower payload проверить по коду)→training owner→actual parameter mutation→snapshot/accept-or-restore→consumer нового состояния. Проверь порядок и semantic binding реальных переходов, не выводи существование API или поля UnitId из этой смысловой схемы. Различай сохранённую пару, update, принятую версию и observed utility. Укажи objective: suitable probabilistic DPO с реальными log probabilities/reference либо явно другое pairwise learning, не similarity под названием DPO. Разбери confounded graph reward и unit-level target; поставь bounded migration question вместо молчаливого удаления graph learning. Выход A — state/credit/provenance map.

**Промпт B — LoRA/rollback comparison.**
> На полученной карте назови конкретные trainable/frozen matrices, dimensions, rank, scale, projection meaning, decoder и index compatibility. Подготовь no-adaptation/source-faithful-static/candidate comparison с same-panel semantic outcomes и полной памятью. Восстанови current one-pair/no-repeat admission/recovery; перечисли causally mutable state и поведение при crash/unknown completion. Bulk оставь группой наблюдений разных новых уроков. Выход B — smallest update+recovery+examination handoff с unresolved prerequisites, не code или training.

**Хорошо:** новая разрешённая пара меняет известный owner, его новая версия читается consumer; stale index отдельно учитывается. **Плохо:** hash другой — «обучение успешно»; транспонирование LoRA автоматически объявлено domain tokenizer. **Края A:** TIE/BOTH_BAD, несовпадающий context, restricted judge content. **Края B:** crash после update до commit, shared base mutation, несовместимый index, повтор старой пары с новым ID.

**Приёмка A:** objective и true update target прослежены либо явно блокируют только зависимый опыт. **Приёмка B:** известен допустимый recovery без второго градиента поверх unknown state; техническая сохранность не выдаётся за gain. **Передача:** R12 — разрешаемые architecture decisions; R13/R18 — конкретный первый урок и minimum missing seam.

## R06 — kNN, Flow, Orchestrator, Graph: изучить полезность связей

**Контекст/refs:** базовый R06; S07–S10; WorkLeft DESIGN-ACTIONS-01, TASK-PROMPTS-P0-P1/P2-P3/P4-P6-PT; R03 dynamic-flow/arbiter sources; R04 representation versions; R05 state/owner. **Skills:** trainable-architecture-decision, async-concurrent-architecture, code-examination, experiment-design. **E:** §3–5,8–9.

**Промпт A — механизм выбора и память.**
> На реальном коде раздели domain pool, features, distance/ranking, k policy, BM25 contribution, positive/negative/warning memory, graph relations, final arbiter и owners. Найди learned authority в Flow/Orchestrator/Graph; наличие if/score/случайности не доказывает обучение. Зафиксируй допустимые relation/port transforms, async fan-out/fan-in, отмену ветвей, консистентное чтение snapshot и полный бюджет. Выход A — source-to-symbol connection map и existing reuse candidates.

**Промпт B — причинные вмешательства.**
> Для каждого существующего или отсутствующего learned decision опиши intervention: frozen units + меняющаяся Flow; fixed candidate universe + routing policy; unchanged primitives + graph relation learning; fixed memory + learned kNN policy versus changed memory + frozen policy. Укажи что держится неизменным, candidate admissibility, наблюдаемую пользу, вред и стоимость всех ветвей/index maintenance. Дай один law-sensitive hard negative и один cross-domain/unknown case. Выход B — адресные mechanism experiments с owner и minimum missing seam, а не Cartesian product всех настроек.

**Хорошо:** graph учится новой полезной композиции при тех же primitives; adaptive kNN имеет downstream benefit в своём domain/budget. **Плохо:** больше edges/neighbors считается улучшением; fastest branch отменяет arbiter; fixed cosine объявлен лучшей функцией для всех доменов. **Края A:** incompatible ports, cycle, exhausted budget. **Края B:** поздний valid result, wrong-domain nearest neighbor, graph reward не локализует виновный unit.

**Приёмка A:** все четыре роли имеют source-backed mapping или precise gap. **Приёмка B:** механизм можно причинно отличить от памяти/другой политики; невозможность isolation отмечена как confounding. **Передача:** R07 получает только действительно неразрешённые связи; R12/R13 — shortlist и recipes.

## R07 — соединить доказанные части без повторного общего аудита

**Контекст/refs:** results R04–R06 с exact checkpoints; WorkLeft MECH-01-mechanism-validation и RESEARCH-03-dpo-algorithm-conformance; original role decision; конкретные primary spans открытых стыков. **Skills:** trainable-architecture-decision, code-examination, plan-review. **E:** §2–3,6–9.

**Промпт A — end-to-end trace одного полезного маршрута.**
> Используя принятые method maps, проследи input→domain→representation→lawful candidates→Flow/Orchestrator/Graph choice→output→feedback→pair→state owner→следующий consumer. В каждой стрелке назови schema/version/meaning, mutable state, policy and provenance. Читай новый source только для конкретного unresolved seam; не повторяй R04–R06. Выход A — complete bounded trace и отсутствующие стрелки.

**Промпт B — локализация препятствия первому уроку.**
> Раздели отсутствующий контракт/owner/objective/permission, отсутствующий небольшой код, неизмеренную эмпирическую пользу и необязательный platform polish. Для каждого true blocker дай минимальный implementation/examination packet: источник, существующий код, producer/consumer, изменение, lawful positive/counterexample, state/recovery, наблюдение и cost. Не замени required learned choice детерминированным shortcut ради красивого trace. Выход B — ordered bounded gap set с точным владельцем, не общий NOT_READY.

**Хорошо:** Unit state меняется, но inference читает старый adapter; ближайшее действие — подключить настоящий consumer и сравнить frozen/updated state. **Плохо:** «вся архитектура ещё не доказана, нужен новый общий обзор». **Края A:** route не проходит все четыре роли; версия меняется между async ветвями. **Края B:** спорный owner блокирует только свой урок, но другой законный маршрут уже пригоден.

**Приёмка A:** нет выдуманных interfaces и невидимых переходов. **Приёмка B:** каждое препятствие меняет ближайший исполнимый шаг; unknown gain с готовым recipe не назван design blocker. **Передача:** R12/R13/R18 могут использовать узкий достаточный пакет до окончания общего R25.

## R08 — данные и доменная широта: пары должны учить нужному различию

**Контекст/refs:** WorkLeft RESEARCH-01-training-quality-speed-accuracy-program и RESEARCH-01-REVIEW-20-passes как уже существующая база; baseline domains, all additional R02/R03 obligations; R04–R07 mechanisms; actual permissions/provenance policy. **Skills:** experiment-design, improvement-measurement, trainable-architecture-decision. **E:** §4–8.

**Промпт A — law-aware pair curriculum.**
> Сохрани каждый baseline domain/direction и дополнительно найденную область отдельно от intent/slice/model. Для каждого дай real-world task family, применимые hard/conditional/soft laws, representation discriminants, meaningful chosen/rejected construction, TIE/BOTH_BAD handling и provenance. Positive требует lawful полезного решения при том же input/context; negative отличается причинно значимым провалом. Раздели training examples, operational monitoring и untouched confirmation по source/problem families, а не случайным почти-дубликатам. Выход A — domain→law→pair→mechanism curriculum map.

**Промпт B — антиутечка и исследовательская информативность.**
> Подготовь для первого pilot и другого структурно отличающегося domain конкретные exam families: seen-domain unseen-family, new composition, boundary/OOD, law-sensitive near-match, retained old skill. Покажи, какая разница между кандидатами учит domain mapping, path ranking, local transformation или graph relation; не приписывай один confounded reward всем блокам. Укажи разрешённый путь получать данные и что остаётся UNKNOWN. Выход B — данные/разметка/split/credit handoff для R13/R21/R22, без платной генерации и создания live training corpus.

**Хорошо:** SQL chosen сохраняет NULL/aggregation semantics, rejected похож текстом, но нарушает именно заданное условие; оба относятся к одной задаче. **Плохо:** chosen от более дорогой модели; random junk negative; те же templates попали в confirmation. **Края A:** две правильные альтернативы, противоречивый user feedback, разные model terms. **Края B:** забывание старого домена, неизвестные ground truth/laws, cross-domain ambiguity.

**Приёмка A:** источник и критерий предпочтения объяснимы; каждый retained domain имеет owner. **Приёмка B:** независимость families и недопущенный teaching из evaluation проверяемы документально. **Передача:** существующий WorkLeft research не запускается заново как ещё 20 проходов.

## R09 — выяснить реальную цену процесса и локального обучения

**Контекст/refs:** WorkLeft INTEGRATION-02-test-batching, TASK-PROMPTS-TEST-BATCHES, DEC-01/02/04, MATRIX-01-progress-by-dates, REPORT-01-progress; существующие workflow metadata/logs/diffs и фактический hardware inventory при доступности. **Skills:** code-examination, plan-review, improvement-measurement. **E:** §8–9.

**Промпт A — задержки и проверка гипотезы о подгонке.**
> По имеющимся данным составь bounded episode timeline: запрос→реальный diff→ожидание/повтор CI→исправление→пользовательский результат. Раздели queue, execution, reviewer/agent context overhead, repeated planning, implementation и training. Гипотеза подгонки требует конкретного изменения rubric/assertion после результата; один failed run её не доказывает. Не запускай новые Windows/Actions для сбора доказательств. Выход A — verified episodes, ограничения измерения и предложения delete-from-critical-path/defer/retain с impact.

**Промпт B — PC feasibility envelope.**
> Раздели существующие measured и missing ресурсы: base/model runtime, adapters, optimizer/activations, index, embeddings, graph, cache, snapshots, reindex, data и failed attempts. Исторические PC specs не объявляй сегодняшним measured profile. Для missing данных дай короткий будущий measurement recipe и owner; не требуй full training перед оценкой самой feasibility. Выход B — cost/latency/memory accounting для pilot и shortlist наиболее дешёвых информативных шагов.

**Хорошо:** повтор CI имеет конкретный run/date/diff и не связан с первым learner seam; его перенос не отменяет функциональную обязанность. **Плохо:** «все тесты бесполезны» без анализа; FLOPs/adapter size приравнены wall-time/whole-PC footprint. **Края A:** неполные логи, отменённые ветви, GitHub недоступен. **Края B:** OOM, cold/warm cache, ноль принятых улучшений — стоимость не делится на ноль.

**Приёмка A:** оценки экономии отделены от измерений; existing checks не отключены. **Приёмка B:** budget учитывает весь путь, есть remaining measurement owner. **Передача:** R11 получает причины, R14 порядок, R13/R23 ограничение experiment budget.

## R10 — source-preserving join и адресный остаток

**Контекст/refs:** PARALLEL-R01-R03-JOIN; собственный R10/возможный already-executed J03; точные checkpoints R01–R09 и реальные primary spans конфликтов; WorkLeft DOCDEBT/VERIFY-03..05 по назначенным вопросам. **Skills:** reference-inventory-and-source-map, authority-requirement-binding, plan-review, output-readability-gate. **E:** §1–3,9.

**Промпт A — интеграция без потери и перезапуска.**
> Зафиксируй конечные входные checkpoint tuples R01–R03 и используемые тематические R04–R09. Для каждого source/decision сохрани identity, read depth, все первичные основания и owner. Дедуплицируй утверждения, не требования; конфликтующие версии остаются обе с lineage. Уже исполненный J03 зачти по действительному охвату и счёту calls. Выход A — union map с exact incoming refs, incorporated/deferred/conflicting/late-delta rows.

**Промпт B — только настоящее разрешение остатка.**
> Сверь сохранение четырёх ролей, baseline и дополнительных доменов, всех слайсов/внеслайсовых задач, всех обнаруженных RAG/WorkLeft families и specific algorithms. Для каждого непрочитанного critical span назначь ровно одного владельца и вопрос; R10 не выполняет девять чужих исследований. Передай R12 архитектурные конфликты, R13 измерительные, R14 стоимость/порядок. Выдай ближайший пакет, который уже достаточен для следующей работы, и отличи его от unresolved частей. Выход B — bounded handoff и точный remaining-capacity assessment.

**Хорошо:** новый commit поменял только RUN-LOG соседа: relevant source reading не повторяется. **Плохо:** самый свежий отчёт уничтожает остальные; поздний источник перезапускает весь join; «все headings есть, поэтому источники изучены». **Края A:** одинаковые F01, разные refs, worker ещё работает. **Края B:** moved source, превышение capacity, новый важный domain после cutoff.

**Приёмка A:** union сохраняет provenance и несогласие; чужие каталоги не изменены. **Приёмка B:** каждый retained expectation имеет owner/следующее наблюдение; prepared recipe не выдан за observed gain. **Передача:** R11–R15 получают пригодную основу, не условие снова обследовать весь проект. Общий R10 не является prerequisite для уже достаточных тематических работ R04–R09.

## Что закрывает этот checkpoint

Опубликованы десять дополнений с собственными A/B prompts, входами, навыками, source routes, примерами, краями, outputs, текущей приёмкой и будущим функциональным назначением. Ни один Rxx здесь не объявлен исполненным. Научные критерии находятся в E; это документная поправка, не product test suite и не доказательство learning quality.
