# R08 — CP4: исправленное поручение и проверенная публикация

**13.10.2026. Текущий запрос: review and repair R8. PROMPT_REPAIR = REPAIRED / PUBLISHED.** Это новый адресный ремонт после CP3, а не повторное предъявление прежних commits. Полное исследование R08, обучение и независимая Astra-экспертиза этим ремонтом не выполнялись.

Основной вход: [run-prompts/R08-PARALLEL9.md](run-prompts/R08-PARALLEL9.md). [R08.md](run-prompts/R08.md) — короткий alias; [START-NEXT-RUN.md §1](START-NEXT-RUN.md#1-собрать-пакет-до-вызова-не-поручать-сборку-пользователю) передаёт полный CP4 один раз, own checkpoint и адресные поздние результаты. Пользователь не собирает книги вручную.

## 1. Что было исправлено именно сейчас

| ID | Новый review finding | Применённое исправление |
|---|---|---|
| CP4-F01 | CP3 уже содержал полезные A–E, семь предметных примеров, DPO verdicts и четыре роли. Их отсутствие нельзя было объявить заново. | Полезное содержание сохранено в CP4; прежний полный текст доступен на baseline commit. Новая правка адресует конкретные зависимости и недостаточную причинную конкретность. |
| CP4-F02 | R08 использовал R01–R12, initial shared dispatcher/JOIN — R01–R09, тогда как прямое поручение разрешает R01–R16. | R08/alias принимают шестнадцать параллельных стартов. R10 остаётся partial research join, не ждёт зависимый R16. R11–R16 могут читать/готовить работу, но unresolved design не становится approved, shared writes сериализуются. |
| CP4-F03 | CP3 E закреплял первый DPO recipe перед остальными контрастами, а R14 CURRENT REPAIR V2 прямо заменял E1-first на capability DAG. | Четыре candidate families E1–E4 имеют отдельные вопросы, fixed/changed factors и локальные prerequisites. Existing CodeDocumentation Student сохраняется для E1; нет автоматического нового learner и нет общего E1 gate для независимых E2/E3/E4. |
| CP4-F04 | Формат recipe перечислялся, но значительная предметная привязка могла снова целиком перейти R13. | R08 обязан сам заполнить law/task/hypothesis/examples, доступные source identities, family criterion, metric с единицей и знаменателем, factor/falsifier/retention. R13 связывает реальные manifests/evaluator/численные criteria; R12 — спорный state/design; R18 — точную реализационную связь. NOT_BOUND не runtime-ready. |
| CP4-F05 | Четыре роли и перечень алгоритмов не обеспечивали однозначного causal attribution каждому компоненту. | Добавлена таблица: Unit state; Flow при frozen Units; Orchestrator при тех же solvers/resources; Graph при frozen lower levels на unseen compositions. У каждой роли state/signal/consumer и собственное опровержение. |
| CP4-F06 | Для dynamic kNN мало назвать k, distance и confidence. | Разделены pool/eligibility/features/metric/index/k/radius/budget/neighbor consumer/arbiter/fallback. Приведён bounded query/domain-dependent k-policy как гипотеза, а не принятая лучшая функция; fixed-rule adaptation не называется обученной policy. |
| CP4-F07 | Свежий code read подтвердил конкретный разрыв между raw DPO admission и фактическим training input. | В C/D включены BuildBatch/Compact/ToMask и mean-score contrasts: различие после 480 символов, смысл whitespace, response boundaries, conditional objective, правильный recipient. Это source-bounded риск, не измеренная частота и не whole-system diagnosis. |
| CP4-F08 | Общий START был дополнен другой работой уже после нашего initial read: появились COMPLETION-REPAIR результаты и R01–R16. Откат к initial START потерял бы ценные решения. | Перед edit повторно прочитан новый START. Его completion §0, sixteen-start semantics и чужие детали сохранены. Изменён только маршрут R08: полный CP4 без повторного COMMON, DOMAINS §§2–9 как заполненный baseline, не новая обязанность составить те же паспорта. |

**Уточнение CP4-F03 после позднего completion delta.** Current ALGORITHMS §9/START сохраняют CodeDocumentation как основной first-learning recipient по WorkLeft; R12 принимает или предметно оспаривает этот выбор. Это не измеренный глобальный оптимум. CP4 не отменяет выбранного Student и не объявляет другого победителя. Исправлено превращение этой E1-ветви в универсальный барьер независимому исследованию E2/E3/E4. В START это различие записано явно.

## 2. Какая работа теперь получается из R08

A восстанавливает первичные ожидания, WorkLeft/review hypotheses и RAG→primary→successor. B принимает семь существующих паспортов и добавляет только доказанные предметные различия/остатки. C связывает их с четырьмя ролями, domain mapping, representations и real kNN consumers. D проводит документные walkthroughs exact pair→tokens/masks→objective→recipient→ordinary use, отдельно LoRA/projection и split lineage. E готовит конкретные E1–E4 candidates, достаточный первый предметный handoff и программу bulk-to-bulk observations.

Каждая часть содержит роль, контекст, skills, подробный prompt, sources, результат, good/bad/edge, проверку документа и отдельную будущую проверку обучения. B можно разделить на четыре предметные группы; child получает полный необходимый текст и return-only права, не абстрактное «изучи домен». A–E и candidate labels не считаются фактически вызванными независимыми моделями.

От bulk к bulk сравниваются parent/challenger на одинаковых заданиях; новая panel оценивает обе версии. Bulk состоит из новых уроков, не replay старых IDs. Раздельно проверяются pair validity, реальное изменение state, использование обычным потребителем и полезность. Измерения охватывают mapping/semantic accuracy, lawful path/associations, все attempts/branches, время до допустимого урока и до принятого улучшения, whole-PC RAM/VRAM/storage/index/rebuild, retention и feedback. Operational promotion panel не объявляется untouched certification.

Сохранены: семь предметов плюс original extras/languages; DPO/DynamicGraph, DTC, IDE; S00–S10; typed many→many/associations, пять видов связей и semantic order; actual ILearningMachine ownership, Graph=L5; TensorCore/shared base/project LoRA; StackDetector-before-normalization; BM25+kNN и positive/negative/warning memory/final arbiter; one-new-pair/full-state restore; текущие author/selector/pair-assessor/outcome-evaluator полномочия; S04 local/S05 remote/S06 campaigns. Корректный deterministic ответ не negative по происхождению.

## 3. Источники нового review и границы чтения

Initial planning pin P0: `ffad2d223370508b8917b1377a5fbf51b578d1e2`. Core C0: `3c7f5f8c7809936e25579603c96ea6df8598fc40`, повторно проверен по branch API. Совпадение C0 с CP3 не считается новым чтением всего кода.

| Прямо прочитанный источник | Scope / использование |
|---|---|
| replanning/AGENTS.md; old R08-PARALLEL9.md; R08.md | Governing repair/write scope; old A–E и examples прочитаны по диапазонам; установлены точные противоречия и reusable содержание. Old main blob `723bcd324b576ea3550c14be3146da346458f162`. |
| results/R08/FINDINGS.md и RUN-LOG.md | Выбранные current CP3 sections, research/measurement boundaries, прежние code observations и unread остатки. Не новые собственные measurements. |
| PARALLEL-R01-R03-JOIN.md, SOURCE-ROUTES.md | Прямые/обрезанные выдачей разделы ownership, первичных маршрутов, WorkLeft и handoffs; не полный recursive обход. |
| previous rounds thinking results.md | Запрошенные диапазоны 1–70 и 800–885, часть выдачи ограничена; не полный исторический архив. Blob `875ed0ec632e93ddb87769330bcca7ad28c1866c`. |
| results/R14/FINDINGS.md | 1–100, CURRENT REPAIR V2; blob `40fa4cf3d5d69de5a1b358ecffe5d2b227a146cb`; явная отмена обязательного E1-first. |
| results/R15/FINDINGS.md | 1–90; blob `677c00614c0ef0652891e14b02f857bdfde03ec4`; code risks, four-role contrasts и ограничения R12/R13/independent review. |
| run-prompts/R16.md | Полный возвращённый packet 1–140; blob `0859f1f7ef6031b4cbf437511706f5c43db23044`; source-bound requirements и provisional boundaries. |
| WorkLeft-RAG-MASTER-INDEX → WorkLeft-02-teaching-chinese-model | MASTER initial relevant section → actual current WorkLeft-02 1–74; WorkLeft-02 blob `6495f5be2e89720bad011c5ae23fb84b2e983ea0`. Реальный primary transition, не весь RAG. |
| WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program | Начальные active sections и часть archive introduction, requested 1–100; семь предметов, one-pair programme и наличие inline O*-H corrections. Полный архив не прочитан. |
| K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs | 2260–2347, BuildBatch/Compact/ToMask; blob `cb8744fcbbcff99821852e789dc92163976c0ad7`. Admission raw bytes; затем whitespace compaction и cut480; mask для всех non-padding positions. |
| K/11.DPO/DpoTrainer.cs | 665–771, ComputeLogProbs/BackwardLogProbs; blob `9c358978075db8c22fe0125eaea9e89e831d5635`. Mean score/count и соответствующий binary-mask backward; не полный ordinary Student chain. |
| Late COMPLETION-REPAIR-INTEGRATION | Прочитаны 105–215, включая §§6–7 и точную строку R08; blob `589744ecaedaaf77f7a4955baa227bd997da0c29`. R08 принимает filled DOMAINS, закрывает consumers/review gaps, не повторяет все R04–R07. |
| Late COMPLETION-REPAIR-DOMAINS | Прочитана начальная часть requested 1–90, выдача ограничена около C# passport. Это attributed reusable domain content, не свежая полная семидоменная проверка; §§2–9/10 передаются R08/R13 как exact pending inputs. |
| START-NEXT-RUN | Initial версия прочитана; перед edit свежая concurrent версия прочитана дополнительными диапазонами. Применённый preimage blob `21ab56ff80ec1665c3cfa17fded252ff66555562`; поздний completion текст сохранён. |

**Skills фактически прочитаны из C0:** architect-behavior-classifier 1–125; phase-planning 1–115; plan-review 80–185; code-examination 1–125; prompt-patch-authoring 95–210; human-readable-architecture-audit 1–135; trainable-architecture-decision 1–130; experiment-design 1–130; improvement-measurement 1–120. Использованы repair scope, полный work-order, causal state/consumer, буквальные good/bad и человеческая читаемость. Другие skills в будущем packet — требования будущей загрузки, не заявление об их чтении здесь. Старые 30/50-pass, auto-tier/replay/test-count routines не запускались.

Внешняя методика проверена по первичным HTML DPO §4 Eq.7, LoRA и Adaptive kNN-MT (ACL 2021 short.47). Это основание формул/гипотез, не доказательство эффективности XIIGEN. Семантические examples CP3 сохранены с первичными spec references и actual-version caveat; они не превращены в admitted corpus.

### Неполученный источник

`09.09.26WorkLeft-RESEARCH-01-REVIEW-20-passes.md`: новый contents request 1–55 снова вернул пустой content при существующем blob `3def99dd83bc6c9728d0c5e97dede47aa3ae6a82`; отдельный raw fetch сообщил too large/unsupported. Полное чтение и извлечение всех unique review deltas **не выполнены**. Inline corrections RESEARCH-01 — самостоятельный возможный источник, не подмена полного journal. Этот адресный остаток остаётся у R08-A; нет выдуманного review или двадцати новых раундов.

## 4. Применённые файлы и readback

В этой сессии изменены **четыре документа** в planning/replanning: основной prompt, alias, START и этот журнал. Пять `results/R08` carriers не переписывались; чужие результаты, live WorkLeft, core, skills, datasets, weights и workflows не менялись. Не присваивались чужие DONE/PASS.

| Файл / публикация | Commit | Возвращённый и проверенный remote blob |
|---|---|---|
| R08-REVIEW-CP4.md, промежуточный checkpoint | `aaa4c6e07b4188b0bd047de04638140259d0e551` | `cee7856518edcf6dd3e48c04b80840fc1b8e2282`; прочитан перед этой финальной заменой |
| run-prompts/R08-PARALLEL9.md | `8af890bd73f26391edba8ce6b60f8fea5118e90f` | `eb2bacab68926a14f6a6bf4caf802d19a37a17a7` |
| run-prompts/R08.md | `fbd702d11e28e1ddcd3bdf9af871bbab72b6631b` | `b28d0f1eda9ae40311aa4c941146b090073a1ed8` |
| START-NEXT-RUN.md, адресная интеграция позднего результата | `a85cfa2283084aa72eba45e9c538ab488ed3848b` | `4308ce099b76d4f769dbf86b357437b2be588195` |

До финальной записи журнала выполнен GitHub content/blob readback трёх исполняемых файлов: основной prompt — header/source scope, D/E и ending/handoffs; alias — полный возвращённый текст; START — специальный маршрут/таблица R08. Проверены readable Russian, один full packet, отсутствие повторного COMMON, сохранение completion baseline, различие E1 recipient/универсального E1 gate, write scope и независимые статусы. Остальные поля доступны в committed bytes; remote checksum не называется semantic proof всей программы. Финальный journal commit/его readback фиксируются внешним tool receipt, чтобы не создавать бесконечную self-hash запись.

Содержательная самопроверка: конкретный SQL child получает law/input/negative/consumer question; kNN first-hit не принимается за learned aggregation; suffix-loss не принимается за exact-pair training; неизвестный recipient остаётся local prerequisite; готовый E2 не ждёт unrelated E1; отрицательный/неясный опыт не объявляется улучшением; поздний R16 delta не заставляет reset всего R08. Это документная проверка автора, не независимый модельный review.

## 5. Итоговые границы

| Вид результата | Статус |
|---|---|
| Новый review и ремонт действующего R08 / его dispatcher | REPAIRED / PUBLISHED |
| Полное выполнение исследования R08 | НЕ ВЫПОЛНЯЛОСЬ ЭТИМ РЕМОНТОМ; prior carriers сохраняют свои статусы |
| Полный обход RAG/WorkLeft/code/исторического архива | НЕ ЗАЯВЛЯЕТСЯ |
| Обучение, измеренный прирост, PC fit | NOT_MEASURED |
| Отдельный Astra/ultra reviewer invocation | NOT_PERFORMED |
| Авторская документная проверка и remote readback | PERFORMED в указанных границах |

Новых product tests/coverage/Windows/Actions/build/trainer/teacher/judge запусков здесь не было. `[skip ci]` в doc commit messages не является гарантией поведения всех настроенных workflows; их результаты не использовались для заявления о полезном обучении.

**Ближайшая работа назначенного R08:** принять уже заполненные DOMAINS и own prior findings, дочитать адресные primary/review/consumer gaps, связать их с конкретным controlled observation и передать versioned subset владельцам. Это продолжение исследования, а не повторный ремонт этого prompt и не перезапуск всей программы.
