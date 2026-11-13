# R01–R10: конкретные подзадачи сбора и гипотез

**13.10.2026 · Поручения, не выполненные исследования.** Читать только свой Rxx вместе с полным одноимённым разделом [LEARNING-WORK-ORDERS-01-10.md](LEARNING-WORK-ORDERS-01-10.md), [MODEL-DISPATCH-CONTRACT.md](MODEL-DISPATCH-CONTRACT.md) и своей строкой [DOMAIN-EXPERIMENT-RECIPES.md](DOMAIN-EXPERIMENT-RECIPES.md). A/B — две части того же прохода, не новые запуски. Полные полномочия, запреты, схемы и H-профиль наследуются по dispatch-контракту и разворачиваются в каждом фактически отправленном prompt. Все примеры здесь — ILLUSTRATIVE / NOT_TRAINING_DATA.

## R01 — сначала установить всю область и нагрузку

**Контекст:** SOURCE-ROUTES, COVERAGE-AND-WORKLOAD-GUARD, previous rounds thinking results, реальные деревья D/core; WorkLeft MASTER-INDEX, usage-guide, INDEX-01 и REDATE-01. Прежний S04-результат — источник с ограниченной областью, не доказательство общего обзора. Не выбирать алгоритм.

**R01.A · Инвентаризация и сохранение прежнего результата. Skills S/P.**
Вход — закреплённые refs, деревья и реальные прежние артефакты. Рекурсивно найди общие промпты, все WorkLeft/LeftToWork, RAG/NDJSON и слайсы; пройди к корням, указанным в исходных заданиях. Для каждого файла запиши идентичность, размер, роль, текущую/архивную версию и глубину уже имеющегося чтения. Сопоставь прежние выводы с их фактическими источниками: сохранённое наблюдение о S04 оставить, общий unsupported вывод понизить. **Выход:** строки SOURCES и краткий reconciliation в FINDINGS. **Приёмка:** дерево и реестр сопоставимы; отсутствуют подмена найденного прочитанным и удаление полезного старого evidence. **Хорошо:** у общего большого WorkLeft названы несколько разных тематических разделов. **Плохо:** `WorkLeft — учтён` одной строкой без конкретных файлов. **Край:** архивная копия новее по имени, но старее по содержанию; различить по provenance. **Передать B:** конкретные источники, ещё не назначенные диапазоны и объёмы.

**R01.B · Распределить вопросы, а не раздать одинаковый архив. Skills S/P/E.**
Вход — A и области R02–R10. Назначь владельца каждому обязательному семейству и тематическому диапазону, включая все S00–S10, семь доменов и три направления. Разнеси representation в R04, update/LoRA в R05, routing/kNN/graph в R06; R07 оставь синтез стыков. Для каждого получателя укажи ожидаемый результат, стоимость чтения и резерв на анализ/приёмку. **Выход:** source→span→question→owner→capacity map. **Приёмка:** нет владельца «потом», бесхозного обязательства и скрытой очереди из 50 сессий. **Хорошо:** R03 получает конкретную ветку NDJSON и нужные первичные решения. **Плохо:** R07 должен прочитать всё остальное. **Край:** найденный внешний источник недоступен — зафиксировать адрес и блокируемое решение, не выдумывать содержание. **Передача:** R02 получает общие задачи, R03 маршруты, R04–R10 свои диапазоны.

**Функциональная связь:** по каждому будущему claim о домене, DPO, kNN, LoRA и пользовательском результате можно назвать исходную цель и владельца исследования. Наличие реестра само по себе не подтверждает эти способности.

## R02 — восстановить ожидания пользователя без потери исходных линий

**Контекст:** R01; `D/original task.md`, `D/task prompt.md`, `D/task prompt.txt`, `D/goal of the round.md`, `D/Алгоритм создания ДПО.md`, `D/Изменения в плане работы - цель.md`, `D/Claude session decisions.md`; назначенные WorkLeft решения и COVERAGE-01. Недоступный вариант файла не заменять другим молча.

**R02.A · Извлечь обязательства и решения. Skills S/P/A.**
Прочитай содержательные постановки, отделив исходную цель от старых команд достижения. Составь requirement ledger: точная формулировка/источник, пользовательский результат, домен, уровень, направление, ограничения и действующая власть. Восстанови ввод→результат→feedback→обучение→повторное применение. Сохрани DTC API/рендеры/разрешения/БД/публикацию, IDE-потребителей, цену разных пользователей, many-to-many/async и Builder. **Выход:** CHANGE-MAP исходных ожиданий, ещё без переписывания live requirements. **Приёмка:** обратная трасса от каждой строки к первичной постановке; слайсы не заменяют домены. **Хорошо:** «ответ после дообучения используется приложением» имеет потребителя и критерий. **Плохо:** «сохранён JSON — обучение выполнено». **Край:** требование находится только в архивном теле; установить наличие явной отмены, не выбрасывать из-за баннера. **Передать B:** ledger и спорные решения.

**R02.B · Развести цель, метрику и политику. Skills P/A/M.**
Сопоставь семь исходных доменов, три направления и роли обучения с A. Отдельно опиши 95% к фиксированному эталону, confidence router, one-pair/no-repeat, unit-level train-ready и роли teacher/judge/student. Для каждого конфликта укажи обе первичные формулировки, что сохраняется сейчас и кому принадлежит решение. **Выход:** список конфликтов и способность→будущий наблюдаемый исход→R-owner. **Приёмка:** новая learning-first последовательность не отменяет исходные конечные результаты и не разрешает replay задним числом. **Хорошо:** более дешёвый маршрут оценивается только среди допустимых решений. **Плохо:** confidence 0.95 назван 95% правильных ответов. **Край:** смешанный домен без однозначного ярлыка — сохранить необходимость многометочной оценки/отказа от уверенного решения. **Передача:** R03 ключи поиска; R04–R08 предметные требования; R16 карта миграции.

**Функциональная связь:** каждое ожидание имеет независимое от test count определение успеха и неуспеха; измерений в R02 нет.

## R03 — пройти все маршрутные семейства к первичным основаниям

**Контекст:** SOURCES R01 и требования R02; WorkLeft RAG-MASTER-INDEX, RAG-00-usage-guide, RAG-01-dpo-legal-build, RAG-02-design-to-code, RAG-03-merge-and-branches, RAG-04-vs-vscode-continue, их реальные NDJSON, RAG-DELTAS, TASK-PROMPTS-CHRONICLE/COVERAGE и обнаруженные вложенные карты.

**R03.A · Разрешить маршруты и версии. Skills S/P.**
Разбери все назначенные текущие индексные строки на source root, путь, раздел, тип и преемника. Разрешай относительный путь от правильной библиотеки. Используй visited-set по repo/ref/path; содержательные версии и aliases сохраняй, повторные циклы не читай бесконечно. **Выход:** маршрутные строки SOURCES со статусами discovered/read/traced/unavailable и конфликтами версий. **Приёмка:** каждая найденная текущая ветка имеет результат разрешения; parse-success не выдан за смысловую проверку. **Хорошо:** запись ведёт в `dpo legal build`, а не в одноимённый файл DTC. **Плохо:** одинаковое basename принято за один источник. **Край:** ссылка ведёт в перенесённый архив — проверить преемника и сохранить первоначальную identity. **Передать B:** критические цепочки и точные неподтверждённые адреса.

**R03.B · Подтвердить основание каждого критического перехода. Skills S/A/C.**
Открой первичные тексты всех маршрутов, на которых основаны решения о требованиях, владельцах кода, алгоритме и интеграциях. Сопоставь заявленную власть/статус с текущими refs. В каждую цепочку добавь исходное требование, первичное решение, владельца реализации или конкретный gap. **Выход:** question→RAG row→primary span→current decision→consumer/owner. **Приёмка:** ни один критический вывод не опирается только на RAG-пересказ; непроверенные некритические ветки имеют owner и ограничение. Один удачный переход не закрывает весь подграф. **Хорошо:** unit-level pair boundary подтверждён первичным brief. **Плохо:** архивное READY стало сегодняшним ProductReady. **Край:** кодовая ветка недоступна — нельзя объявить механизм missing только по отсутствию доступа. **Передача:** R04–R08 адресные первичные основания; R10 остаток с вопросом/объёмом.

**Функциональная связь:** правильная навигация должна приводить к источнику реального требования, а не к замкнутому кругу отчётов, пересказывающих друг друга.

## R04 — S00–S03 и реальные представления данных

**Контекст:** R02/R03, текущие S00–S03 по SOURCES; WorkLeft 01-code-changes, 05-branch-reconciliation-codex-dpo-implementation, SOLUTIONS-01, VERIFY-01/02; найденные encoder/tokenizer/state/evaluation consumers. Не угадывать class/method по названию задачи.

**R04.A · Сопоставить четыре слайса с существующим кодом. Skills C/S/A.**
Для каждого S00, S01, S02, S03 отдельно восстанови цель, требования, producer/consumer и доказательства. Читай реализацию и интерфейс в контексте, затем вызывающих и потребителей. Различай reusable, partial, missing и unknown; старый статус сравни со свежим кодом. **Выход:** четыре slice записи и карта повторной/уже выполненной работы. **Приёмка:** каждое утверждение о реализации имеет code ref и границу наблюдения; нет выводов только из названия теста. **Хорошо:** обработка уже есть, отсутствует лишь подключение потребителя. **Плохо:** перепланировать весь компонент, потому что старый checklist открыт. **Край:** реализация в другой ветке — записать доступность переноса, не считать её автоматически присутствующей в target. **Передать B:** representation и evaluation seams.

**R04.B · Проверить, что представление сохраняет нужный смысл. Skills A/C/M.**
Разведи текстовые token IDs, структурные признаки домена, embedding, проекцию и индекс. Для каждого укажи форму, единицы, нормировку, версию, источник обучения и потребителя. Разбери сценарии одинаковых слов с разным отрицанием, единицами и структурой; опиши, где различие теряется. Установи границы train/development/certification и lineage представлений. **Выход:** representation contract map и диагностические гипотезы. **Приёмка:** одинаковая размерность не считается совместимым смысловым пространством; изменение encoder имеет последствия для datastore/rollback. **Хорошо:** условные 10 ms и 10 s сохраняют различимые единицы до решения. **Плохо:** оба превращены в одинаковый «10» и объявлены эквивалентными. **Край:** пустое/неизвестное поле — явный missing/abstain, не нулевой вектор с мнимой уверенностью. **Передача:** R05 state/version boundaries; R06 representation seams; R12 кандидат минимальной починки.

**Функциональная связь:** будущая оценка — сохранение предметных различий и downstream качество маршрутизации/решения, а не shape match.

## R05 — S04–S06, однократное DPO-обновление и LoRA

**Контекст:** R04, реальные S04–S06 и role contracts; WorkLeft 02-teaching-chinese-model, INTEGRATION-01-training-into-work-program, TASK-PROMPTS-TRAINING, текущие trainer/adapter/state consumers. Windows-дефект — только одна возможная зависимость.

**R05.A · Проследить один настоящий update. Skills A/C/M.**
Для S04, S05 и S06 отдельно сопоставь обязательства и реализацию. Пройди pair bytes/provenance→eligible identity→состояние ученика→training owner→мутация→оценка→принять/откатить→следующий потребитель. Отличай пополнение памяти от обновления параметров. Найди actual objective; стандартный DPO требует определённых policy/reference и chosen/rejected likelihood, собственный preference learner описывается своим правилом. **Выход:** mutation/consumer trace, минимальные прямые блокеры урока и evidence gaps. **Приёмка:** изменение loss/hash или создание checkpoint не названы улучшением поведения. **Хорошо:** applied-but-not-evaluated journal ведёт к оценке либо предусмотренному восстановлению, не повторному gradient. **Плохо:** retry запускает принятую пару заново под новым ID. **Край:** обе стороны пары неверны — карантин, не принудительный chosen. **Передать B:** mutable-state boundary и найденные adapter owners.

**R05.B · Установить место и смысл LoRA/проекции. Skills A/C/M.**
Укажи конкретные матрицы/модуль, формы входа/выхода, trainable parameters, frozen reference и downstream потребителя. Опиши lifecycle адаптера и совместимость encoder/index/router. Разбери принятие, отказ и полный restore, включая optimizer/counters и прочее реально изменяемое состояние. **Выход:** adapter/projection map и контракт будущего no-adapter versus adapted сравнения. **Приёмка:** LoRA не названа tokenizer; нужное embedding space не выводится из одного совпадения размерностей. **Хорошо:** смена encoder требует совместимого индекса и учёта цены переиндексации. **Плохо:** небольшой файл адаптера доказывает компактность всей системы. **Край:** старый индекс с новым encoder — явно несовместимое состояние, не научный регресс kNN. **Передача:** R07 update seams, R08 ограничения пар, R12/R13 варианты и оценка.

**Функциональная связь:** future evidence показывает улучшение новых решений, retention и полную стоимость; R05 сам не запускает обучение.

## R06 — S07–S10, доменная маршрутизация, kNN и композиция

**Контекст:** R02–R05, S07–S10 и S05-S10-learning-role-audit-2026-10-08; WorkLeft DESIGN-ACTIONS-01 и TASK-PROMPTS-P0-P1/P2-P3/P4-P6-PT; DPO/DTC/IDE primary routes. Domain recipes — кандидаты, не уже выбранный дизайн.

**R06.A · Сохранить обязательства четырёх слайсов. Skills S/A/C.**
Для S07, S08, S09, S10 отдельно укажи пользовательский сценарий, state/contracts, роль в графе и конечного DTC/IDE потребителя. Проследи many-to-many, fan-in/fan-out, async и передачу результата. Различи измерение качества графа и разрешённый training payload: текущие unit-level/UnitId/training-owner ограничения не расширяются словом «graph». **Выход:** четыре slice карты и role/authority map. **Приёмка:** результат не ограничен удобным graph JSON и не теряет интеграции. **Хорошо:** найден путь к реальному потребителю результата. **Плохо:** «все S07–S10 покрыты» по одному интерфейсу. **Край:** внешний repo недоступен — точный интерфейсный gap без догадки о внутреннем коде. **Передать B:** actual routing/retrieval/path seams.

**R06.B · Разобрать, что действительно динамическое. Skills A/C/M.**
Зафиксируй router, domain registry, допустимых соседей, distance features/units, нормировку, k/weights, uncertainty/fallback и graph selector. Для каждого решения назови hard rule, конфигурацию, memory update или learned policy. Свяжи представление R04 и обновление R05; установи ограничение кандидатов, индексную совместимость и цену fallback. **Выход:** mechanism table и различающие static/dynamic hypotheses. **Приёмка:** manual domain switch не объявлен научившимся выбирать k; минимальная цена считается только среди допустимых решений. **Хорошо:** неизвестный домен ведёт к обоснованному abstention/контролируемому пути. **Плохо:** нулевое число вызовов при неверном ответе объявлено лучшим маршрутом. **Край:** mixed domain, пустой neighbourhood, outlier и таймаут ветки имеют явный исход. **Передача:** R07 общие стыки, R12 выбор кандидата, R14 прямые зависимости.

**Функциональная связь:** per-domain mapping, valid-path rate, полезность соседства и end-to-end качество/цена, не число topology rows.

## R07 — repair as a model-ready multi-subtask work order

**Authority:** `R07-REPAIR-WORK-ORDER.md`. Прежний bounded R07-A/B больше не считается достаточным R07.

**R07-A — coverage repair.** Direct-read assigned WorkLeft, original DPO algorithm, original four-role task and relevant RAG primaries; reconcile with current core tree. Output direct-vs-handoff source matrix.

**R07-B — four-role graph.** Reconstruct Unit/Flow/async Orchestrator/DynamicGraph nodes, edges, many→many associations, deterministic-vs-learned authority and training owner. Include known/new/mixed domain walkthroughs.

**R07-C — domain laws / token-vector passports.** For every active domain family define correctness laws, must-preserve tokens/features, vector semantics/version, oracle/predicted domain, OOD behavior and hard negatives.

**R07-D — dynamic kNN.** Decide where kNN applies, candidate metric/features, dynamic k/radius/weights inputs, exact/ANN/index lifecycle, BM25/selector/arbiter interaction and falsifier. No universal magic k.

**R07-E — DPO recipient.** Trace weakness→CODEX/Astra lesson selection→open Chinese chosen/rejected authoring→CODEX/Astra exact-pair verdict→lossless tokens→objective→state→accept/rollback→ordinary adoption. Preserve TIE/BOTH_BAD/UNJUDGEABLE and one-pair/no-repeat.

**R07-F — LoRA / compact state.** Separate representation vector, retrieval vector, LoRA delta and graph state; identify adapter topology, parameter group, tokens/masks, checkpoint compatibility and ordinary consumer.

**R07-G — hybrid interaction.** Classify components HARD DETERMINISTIC / LEARNED AUTHORITY / LEARNED SCORE+DETERMINISTIC RESOLVER / EVAL-ONLY / DUPLICATE / MISSING CONNECTION.

**R07-H — PC/continual viability.** Define exact peak RAM/VRAM, persistent bytes, update/eval/inference/reindex/rollback cost and safe user-feedback continuation measurements.

**R07-I — R13 experiment matrix.** Define causal bulk-to-bulk comparisons with one primary factor, protected holdouts, uncertainty/falsifier and IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE.

Each subtask uses the full skill/context/reference/example/edge/verification contract from the repair work order. They are one R07 execution packet, not nine independent review-count claims.

## R08 — семь доменов, качество пар и пригодность имеющихся измерений

**Контекст:** WorkLeft RESEARCH-01-training-quality-speed-accuracy-program и RESEARCH-01-REVIEW-20-passes как baseline; R02/R05–R07; доступные manifests и прежние outcomes. Закрытые certification answers не открывать для разработки.

**R08.A · Разобрать данные и отрицательные примеры. Skills S/M/A.**
Для EN↔RU, orchestration, Figma→HTML+CSS, C#, Angular, SQL и DPO Builder опиши независимые исходные семейства, поддомены/сложность, права/provenance и доступные задачи. Разбери chosen/rejected на один контекст, корректность preferred, содержательный hard negative, причину предпочтения, ничьи и обе плохие стороны. **Выход:** семь data passports и pair-quality inventory с конкретными пробелами. **Приёмка:** много перефразировок одного источника не названо разнообразием; запрещённые/неясные пары не admitted. **Хорошо:** negative меняет важное ограничение, оставаясь правдоподобным. **Плохо:** chosen грамотный, rejected бессмысленный — это названо лечением ошибки маршрутизации. **Край:** спорная человеческая оценка — карантин/арбитраж, не насильственная метка. **Передать B:** lineage и доступные старые измерения.

**R08.B · Проверить, о чём говорят старые scores. Skills M/A/P.**
Для каждого заявления о gain запроси сравниваемые версии, общий набор, rubric/judge, raw outcomes, цену и ограничения. Раздели training, operational development/monitoring и sealed certification. Найди leakage через исходные семьи, retrieval и диагностику; выдели селекцию удачных попыток. **Выход:** reused evidence versus unsupported claims, набор confounders и требований к R13. **Приёмка:** ни отсутствие raw outcomes, ни training loss не подменяются ростом полезности; старые 20 reviews не засчитаны Rxx. **Хорошо:** обе версии оценены на новой общей панели. **Плохо:** вчера сложный bulk дал 70%, сегодня лёгкий 90%, значит +20 пунктов обучения. **Край:** нет training replicates из-за no-repeat — ограничить вывод о variance, не повторять принятые пары. **Передача:** R13 protocol, R21 curriculum gaps.

**Функциональная связь:** качество pair author и полезность пары для будущего поведения ученика учитываются отдельно; retention не вводит запрещённый replay.

## R09 — измерить цену процесса и проверить подозрение о подгонке

**Контекст:** существующие commit diffs/run/job evidence; WorkLeft INTEGRATION-02-test-batching, TASK-PROMPTS-TEST-BATCHES, DEC-01/02/04, MATRIX-01-progress-by-dates, REPORT-01-progress. Новые jobs не запускать.

**R09.A · Построить наблюдаемую карту затрат. Skills S/M/C.**
Выбери ограниченное обоснованное окно истории; раздели queue/build/execution/environment/repair и документное обслуживание. Свяжи действия с новым механизмом, прямым blocker или реально принятым улучшением. Не выводи человеко-часы из CI duration и не подменяй неизвестное нулём. **Выход:** cost rows с timestamps, источником, единицами, полнотой и causal link. **Приёмка:** выводы ограничены выборкой; стоимость не нормируется только на число commits. **Хорошо:** отдельны стоимость неуспешных попыток и полезного результата. **Плохо:** время создания пары исключено из цены обучения. **Край:** задача в очереди была отменена — cancellation не равен нулевой длительности/успеху. **Передать B:** самые дорогие повторяющиеся зависимости и адреса изменений.

**R09.B · Отличить исправление fixture от ослабления цели. Skills C/P/M.**
Для выбранных цепочек восстанови независимую исходную спецификацию, реализацию, failing expectation и изменение. Классифицируй defect/fixture/spec correction/unsupported weakening/unknown. Для каждого Windows blocker проверь, находится ли он на пути первого локального урока или только на release/host пути. **Выход:** evidence за/против подгонки и конкретные stop/defer/reuse кандидаты с защищаемым свойством. **Приёмка:** изменение assertion само по себе не доказывает подгонку; зелёный rerun не оправдывает её. **Хорошо:** перенос release-only шага сохраняет owner и условие возврата. **Плохо:** удалить всё красное из репозитория. **Край:** отсутствует исходная спецификация — UNKNOWN, не обвинительный либо оправдательный вердикт. **Передача:** R11 причинные факты, R17 точные live clauses.

**Функциональная связь:** оценивать время до первого достоверного улучшения и стоимость попыток, не количество зелёных runs.

## R10 — закрыть только адресные пробелы и передать анализу полную картину

**Контекст:** сводки и SOURCES R01–R09; назначенные остатки; WorkLeft DOCDEBT-01, TASK-PROMPTS-DOCDEBT, VERIFY-03/04/05. Это не поручение заново прочитать всю библиотеку.

**R10.A · Сверить полноту по исходным ожиданиям. Skills S/P/A.**
Сопоставь три направления, S00–S10, семь доменов и обязательства вне слайсов с фактическими уровнями evidence. Дочитай только точно назначенные critical spans. Раздели недоступное, непрочитанное, спорное и отсутствующее в коде. **Выход:** coverage matrix и адресные gaps. **Приёмка:** нет строки «остальное аналогично» вместо обязательного домена/слайса; каждое ограничение привязано к блокируемому решению. **Хорошо:** неизвестный IDE consumer ограничивает release вывод, но не доказанный local baseline. **Плохо:** 10 отчётов автоматически означают завершённое исследование. **Край:** объём остатка превышает ёмкость — честная частичная область и owner внутри 25, без R26. **Передать B:** компактный evidence bundle и доступные независимые линии анализа.

**R10.B · Подготовить разные входы пяти аналитикам. Skills P/A/E.**
Сгруппируй конкурирующие гипотезы: недостаток алгоритма, плохой сигнал/оценка, несовместимое состояние, дублирование, лишняя последовательность и стоимость среды. Каждая получает evidence for/against, неизвестное и falsifier. Подготовь отдельные адресные входы R11 causes, R12 architecture, R13 measurement, R14 dependencies, R15 independent examination. **Выход:** пять bounded handoffs в существующих носителях, не пять новых исследований. **Приёмка:** каждому дан именно его вопрос и доступные источники; общий context сохранён, сырой архив не размножен. **Хорошо:** R12 получает проверенные seams плюс два спорных решения. **Плохо:** всем отправлен один мегабайтный dump и поручено «найти всё». **Край:** источники противоречат — оба сохранены, более удобный не назначен истинным без основания. **Передача:** R11–R15.

**Функциональная связь:** анализ получает материал для выбора первого измеримого урока и широкого curriculum; не получает выдуманного доказательства, что гибрид уже эффективен.


**R07-J — integration / first-teaching seam.** After the R01–R06 semantic join, integrate A–I, choose one semantic recipient, one earliest falsifiable lesson path, one R18 missing seam and one R13 causal examination. CODEX/Astra 6 ultracode performs the architecture-quality review; measured learning remains separate.

**Dependency correction for R07:** R07-PREFLIGHT may direct-read sources immediately, but R07-SYNTHESIS/J must consume the latest observable R01–R06 packets first. See R07-REPAIR-WORK-ORDER.md §§14–18. Each A–J packet has its own prompt/context/skills/RAG/references/good/bad/edge/verification/return requirements there; the bullets above are only an index.
