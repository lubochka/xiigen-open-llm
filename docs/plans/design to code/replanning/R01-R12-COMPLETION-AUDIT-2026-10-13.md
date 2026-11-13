# R01–R12: что действительно необходимо дополнить

**13 октября 2026. Результат аудита опубликованных исследований, а не выполнение R01–R12.**

## Главный вывод

Не нужно запускать все двенадцать исследований заново. Требуется закончить конкретные незакрытые вопросы и не считать исправленный промпт выполненным исследованием.

- **Содержательное продолжение обязательно:** R01, R03, R05, R07, R08, R11, R12.
- **Адресное дополнение существующего полезного результата:** R04, R06, R09.
- **Полный повтор не обоснован:** R02 и R10. Для них нужны точные передачи решений и обновление только затронутых выводов.

Это классификация данного аудита, не перезапись статусов чужих workers. Отсутствие измеренного learning gain само по себе не делает исследовательский или проектировочный раунд неудачным. Но рецепт будущего опыта не доказывает, что он выполнен, а перечень полей паспорта не заменяет заполненный предметный паспорт.

**Промпты продолжения:** [R01–R12 — адресные completion packets](R01-R12-COMPLETION-PACKETS-2026-10-13.md). Они дополняют существующие назначения, не создают вторую очередь, R26 или обязательные дополнительные 30/50 reviews. Программа остаётся 10+5+5+5.

## Снимок, метод и ограничения

Основной снимок planning: `d849fadfec11f31043e9de2a5fec1c08dfa56d12` (P0). Перед публикацией прочитан HEAD `c1a428a9f753bd97bb46544b02e47705d7416128` (P1) и сравнение P0→P1. Из затронутых R01–R12 источников дополнительно прочитаны новая шапка R07 FINDINGS, R08 RUN-LOG/CP3 и дополнение JOIN. Остальные изменённые R13/R14 документы не становятся предметом этого аудита.

Core ref подтверждён: `3c7f5f8c7809936e25579603c96ea6df8598fc40`. В этом аудите напрямую читались применимые sections skills, **не повторялся весь code audit R04–R07**. Указанные ниже кодовые дефекты — атрибутированные результаты этих workers, а не независимое второе подтверждение исполнения.

Прочитаны относящиеся результаты всех двенадцати Rxx: FINDINGS и, по вопросу, SOURCES/DECISIONS/RUN-LOG/CHANGE-MAP. Это не утверждение полного чтения всех пяти носителей каждого Rxx. Дополнительно проверены исходные четыре роли в `original task.md`, начало действующего WorkLeft RESEARCH-03, SOURCE-ROUTES, JOIN, применимые правила replanning и R11/R12 repair work orders. В `previous rounds thinking results.md` непосредственно рассмотрены начальный S04-only фрагмент и поздние разделы 1170–1270; полное чтение исторического мегатекста этим аудитом не заявляется. Сообщение R10 о собственном полном чтении сохраняется как его receipt.

Применены sections skills `trainable-architecture-decision`, `phase-planning`, `plan-review`, `code-examination`, `context-package-design`, `prompt-patch-authoring`, `output-readability-gate`. Старые pass-count/coverage-heavy routines не переносятся в текущую задачу.

Новые product tests, build, Windows/Actions, trainer, генератор DPO и внешний Astra reviewer не запускались. Core, datasets, weights, workflows и результаты параллельных workers этим аудитом не изменяются. GitHub write доступен; публикация выполнена через connector, внешний apply не требуется. `[skip ci]` не означает успешность каких-либо checks.

## Карта всех двенадцати раундов

| Раунд | Что уже можно использовать | Что требуется сейчас | Чего не требовать повторно |
|---|---|---|---|
| R01 | Корни программы, все семейства слайсов, отдельные первичные переходы, LearningMachine lineage | Закрыть инвентаризацию, владельцев и неразрешённые адреса | Читать самому все смысловые тела всего проекта |
| R02 | 52 требования, четыре роли, исправленное Chinese authorship, RC1–RC6 | Адресные authority/consumer receipts; проверить перенос уже решённых правил | Повторно выбирать автора rejected или переписывать все требования |
| R03 | CP08, содержательные RAG-переходы и решения о порядке/authority | Закончить обязательные primary/successor вопросы, обновить route ledger | Проверять все адреса прежде первого полезного primary read |
| R04 | Разделённые representation channels, typed kNN consumer, исправленный приоритет | G01/G02, паспорта координат/ассоциаций и адресные передачи | Делать новый encoder обязательным первым шагом |
| R05 | Existing trainer→update→trial→publish; два конкретных input/objective дефекта | Ordinary generation consumer, semantic recipient, trainable groups и точные admission seams | Строить второй DPO trainer |
| R06 | Реальный kNN/BM25/graph runtime, E1–E6 и предметная score-family гипотеза | Конкретизация выбранной domain policy/consumer, остаток S07–S10, authority receipt | Писать заново уже существующие E1–E6 |
| R07 | Полезные F01–F03 adoption/runtime traces | Выполнить repaired synthesis, direct sources и причинную связь всех четырёх ролей | Выдавать новый work order A–J за выполненную архитектуру |
| R08 | Framework качества/доменов; частичные primaries; CP3 выявил точные дефекты | Заполненные доменные паспорта, узкие semantic traces, пары/семейства/рубрики | Повторять весь code audit R04–R07 |
| R09 | Ограниченное CI-исследование, KEEP/DEFER и честный INCONCLUSIVE | Воспроизводимая старая выборка, точный episode/causal cost, адресные критерии | Доказывать расходы новыми Windows runs |
| R10 | Полезный nine-way join, 25 change rows, сохранённые незавершённые статусы | Только новые semantic receipts и затронутые downstream deltas | Заново выполнять все девять исследований |
| R11 | Полезный first-lesson cut-set и процессные решения | Применить current repair contract: два DAG, актуальный R10, полная адресация переноса | Превращать весь delivery backlog в gate первого урока |
| R12 | Полезный исторический DPO fast path; исправленный work order A–F | Выполнить архитектурный синтез A–F, не только переиздать prompt | Объявлять document-model DPO всем hybrid core |

## R01 — карта программы не завершена

**Основание:** [R01 FINDINGS, F03–F05, F12–F13 и адресный остаток][r01]. Опубликованное состояние — IN_PROGRESS.

**C01.1. Неполный recursive inventory.** Есть обнаруженные корни и пакеты, но нет закрытой карты всех относящихся WorkLeft/LeftToWork/RAG/local-RAG/primary/successor поверхностей двух репозиториев и доступных связанных корней. Нужны конкретный path/ref, роль документа, актуальность, прочитанный вопрос/диапазон, owner остатка. Наличие каталога не означает чтения его решений.

**C01.2. Не завершено сопоставление семейств LF01–LF12 и U01–U14.** Требуется показать, куда попало каждое исходное обязательство, включая extras/HE↔EN и три линии DPO/DynamicGraph, DTC, IDE. Размер файла и количество записей не заменяют полноту требований.

**C01.3. Неразрешённые locators.** Названные `LOGICAL-ARCHITECTURE-REPOSITORY-MAP.md` и `E50-four-level-learning-interface-architecture.md` нельзя считать отсутствующей архитектурой по 404 чтения. Проверить точный parent/path/ref или назвать successor и ограничение. Не повторять тот же неработающий запрос без нового основания.

**Приёмка продолжения:** у каждой обязательной поверхности есть содержательный владелец и проверяемый маршрут; открытые участки имеют точное влияние на потребителя. R01 не забирает у R03–R09 всю исследовательскую работу.

## R02 — сохраняем результат, заканчиваем адресные передачи

**Основание:** [FINDINGS][r02] и [DECISIONS][r02d]. Здесь уже есть 52 requirement IDs, подробные prompts, domain examples и bulk protocol. Говорить, что этих частей нет, неправильно.

**C02.1. Не переоткрывать RC2.** Chinese author создаёт обе final sides; CODEX/Astra выбирает урок и отдельно оценивает точную пару. Ошибка прежней Student-only final rejected policy уже исправлена. Остаток — перенос в действующие производные планы R16/R20, с конкретным адресом, а не новое решение R12.

**C02.2. Закрыть адресацию действительно открытых вопросов.** RC4: semantic contrast/admission не равны положительному pre-update model margin. R05 устанавливает реального consumer LOW_CONTRAST, R12 принимает objective/admission решение. RC1/RC5/RC6: recipient unit против evaluated graph level; N total и L losers; meaningful order; judged projected pair без служебных markers. Нельзя превращать исправленный текст R02 в утверждение, что runtime уже исправлен.

**C02.3. Свести requirement→решение→конкретный downstream packet.** У 52 требований не должно быть безымянного «позже». При новой находке R01/R03/R08 проверить только затронутые строки: тип задачи, предметный домен, направление перевода и product line — разные оси.

**Приёмка:** изменённые строки имеют точное основание и адрес принятия. Полный rerun R02 не требуется; отсутствие готовых R12/R18 не делает уже восстановленные требования неверными.

## R03 — требуется закончить содержательные RAG-переходы

**Основание:** [RUN-LOG CP08][r03]. Статус traversal остаётся IN_PROGRESS.

**C03.1. Не путать счётчики.** Упомянутые 404 индексные записи — количество записей, не 404 прочитанных primaries и не HTTP-ошибки. 63 прежних address checks и 341 оставшийся locator не являются числом выполненных исследований.

**C03.2. Закрыть обязательный смысловой остаток по ветвям.** Продолжить опубликованные T/H/REQ IDs, например T11/v20 или T06/T08 transition/order, через актуальный primary/successor. DPO, DTC, branches/merge, IDE и найденные nested roots должны иметь покрытые consequential decisions, а не только существующие ссылки.

**C03.3. Передавать решение, не цитату.** Source→что сохраняется/заменено→какой algorithm/requirement затронут→конкретный owner. Прежний массовый Q1-before-Q2 барьер уже исправлен; не возвращать его. CP08 исправления Chinese authorship и запрета «deterministic значит плохой» также сохраняются.

**Приёмка:** каждый открытый маршрут закрыт по вопросу либо имеет точного владельца/ограничение; для критических выводов прочитан primary. Это продолжение source traversal, не пересборка всех старых receipts.

## R04 — не потерять смысл представлений и many→many

**Основание:** [FINDINGS][r04] и [RUN-LOG][r04l]. R04-REPAIR-1 — полезный результат с адресными пробелами.

**C04.1. G01/G02 остаются открытыми.** Нужны OutputRefs→реальный payload consumer и upstream segment→input/output association. Материализованный IQueryable не доказывает полную many→many связь, resolver или streaming; одиночный envelope также не доказывает её отсутствия.

**C04.2. Для реально используемых каналов заполнить representation passport.** Raw/canonical/semantic features/TeachingVector/saved-BPE kNN — не одно пространство. Указать координаты, словарь, domain/intent/stack, смысл input/output, version и конкретного consumer. Проверить, доходит ли отрицание/число/порядок/язык через альтернативный raw или typed канал, а не объявлять весь запрос потерянным по одному normalizer.

**C04.3. Замкнуть адресную передачу R05/R06/R07.** TeachingVector weighting не является learned output projection. E01 со сменой encoder больше не prerequisite E02; эта исправленная последовательность не должна откатиться в downstream tasks.

**Приёмка:** один многовходовый предметный пример прослежен до результатов с сохранением ассоциаций; механизм потери локализован либо гипотеза опровергнута. Оптимальность encoder и измеренный gain сейчас не требуются.

## R05 — самый близкий кодовый остаток до интерпретируемого урока

**Основание:** [FINDINGS CP2, §§3–8][r05]. Общий R05 явно OPEN.

**Уже найденные реальные проблемы, не новые задачи на изобретение trainer:** worker проследил `LogicalDpoTrainingUnit.BuildBatch/Compact`, где whitespace сворачивается, а каждая часть prompt/chosen/rejected обрезается до 480 символов перед Encode; и `DpoTrainer.ComputeLogProbs`, где mean по prompt+answer отличается от заявленного response-conditional objective. Это source/code observations, не измеренная частота ошибок и не выполненный patch.

**C05.1. Ordinary consumer.** Найти actual generation caller, выбирающий принятый checkpoint/adapter. Trial evaluator reload и generic stream loader недостаточны. `CodeDocumentationRoutingFlow` с routing profiles не доказывает использование document LlmModel.

**C05.2. Recipient bridge.** Связать intent урока→обучаемый selector/solver→PolicyModel/state→objective→обычный output. Для GraphDpoPair нужен реальный Machine/session/Context bridge. Winning solver ID нельзя автоматически считать обучаемым selector ID.

**C05.3. Input/objective packet.** Сохранить exact assessed distinction до tensors, установить response boundary, masks, reduction, reference freeze и trainable groups. Публичный average inference scorer не меняется молча вместе с DPO loss. Исправление кода выполняется по последующему разрешённому implementation packet, не этим аудитом.

**C05.4. Подтверждённые неизвестные.** Actual saved selection, authoring/assessment, разрешённый lesson/evaluator/budget; global LOW_CONTRAST consumer; итоговые base/shared/project parameter groups, dtype/rank и durable rollback всего затронутого состояния. До фактического вызова нельзя подставлять invented IDs.

**Приёмка исследования:** полный выбранный source→mutation→ordinary consumer trace или точный missing adapter с владельцем; решение, что именно требуется до live lesson. S06 campaigns уже существуют; один S04 update не становится вечным ограничением всех bulks.

## R06 — гипотезы есть; нужна конкретная политика и её потребитель

**Основание:** [FINDINGS checkpoint C][r06] и [DECISIONS §3, E1–E6][r06d]. Уже есть score-family `sum_j w_dj(q)*phi_dj(q,z)`, bounded k/radius и разбор exact/ANN/reranker. Повторно поручать придумать это с нуля не нужно.

**C06.1. Превратить выбранную family в предметный кандидат.** Назвать eligible pool, law-sensitive features/normalization, fixed/adaptive/learned параметры, update signal/owner, k/radius bounds и actual consumer. Различать ручную настройку, адаптивное правило и выученную политику.

**C06.2. Учитывать first-hit baseline.** В прочитанном KnnLogicalUnit остальные hits дают evidence IDs, ответ берётся из `bestExample.Output`. Для нового k нужна понятная польза дополнительных соседей. Exact retrieval, ANN effort и learned ranking не меняются одновременно под одним словом «dynamic kNN».

**C06.3. Замкнуть доменный/графовый контур.** Выбранный classifier training owner, отказ/abstention у caller, положительная/отрицательная/warning память и final arbiter; координаты и совместимость индекса после адаптации. Query-conditioned score не объявляется автоматически пригодной HNSW metric.

**C06.4. Сохранить полный S07–S10 объём.** R06 прямо не заявляет complete task-catalog/external-client audit. Нужна task→capability→actual/proposed→owner/trigger карта оставшихся S07 сравнений/Builder, S08 DTC, S09 IDE/private index, S10 acceptance. Это не prerequisites первого document lesson.

**Приёмка:** конкретный baseline/challenger и future falsifier для выбранного домена; остальные обязательства адресно переданы. Graph Train/Evaluate NOT_ACTIVATED не равно отсутствию всего graph runtime, а learned flag не равно обучению.

## R07 — выполнить repaired synthesis, а не очередной ремонт задания

**Основание:** [актуальная шапка FINDINGS на P1][r07] и [current JOIN override][join]. Новая revision 2 исправляет work-order fields/skills, но прямо оставляет R07_REPAIR_REQUIRED до исполнения A–J.

**C07.1. Direct primaries.** MECH-01, RESEARCH-03, исходный DPO algorithm и relevant RAG lineage не были исходной базой узкого R07. Их назначенные смысловые разделы должны быть реально использованы, с сохранением useful F01–F03.

**C07.2. Четыре причинные роли.** Не просто четыре строки названий: Unit many→many; Flow correction/relations при fixed Units; Orchestrator async combinations/partial results/cost; Graph higher-order composition. На каждой стрелке нужны тип/семантика, mutable state, сигнал, recipient, version/storage/rollback и ordinary consumer.

**C07.3. Два конкретных adoption остатка.** Document-model child→обычная generation; accepted semantic graph-selector state→trusted preload→L2 decision→actual L3 flow. Существующий L2→L3 runtime сохранить. Отсутствие loader на одном пути не обобщать на весь repo.

**C07.4. Семантическое объединение, не ожидание флагов.** Current JOIN имеет R07-specific правило: preflight может идти сразу; synthesis/J должен принять latest observable R01–R06 handoff packets. Требуется receipt по каждому входу и явная граница неполного evidence, а не ожидание полного DONE всех шести. R08/R09 не отменяются.

**Приёмка:** выполнить соответствующие части уже существующего `R07-REPAIR-WORK-ORDER.md` A–J; один сквозной предметный случай и контрпример на каждую различаемую связь. Никакой прирост качества не утверждается без опыта.

## R08 — заполнить доменное содержание, исправить verdict, не дублировать весь code audit

**Основание:** [FINDINGS][r08], [SOURCES][r08s] и свежий [RUN-LOG CP3 на P1][r08l]. Последний — review/work-order/research IN_PROGRESS.

**C08.1. Различать прочитанное и найденное.** CP2 C01–C11 имеют TREE_DISCOVERED, не code trace. CP3 дополнительно сообщает прямое чтение `task prompt.txt`, хотя SOURCES CP2 ещё PENDING. Это уже **несинхронный receipt**, а не повод снова объявить файл непрочитанным и перечитать без вопроса.

**C08.2. Заполнить реальные паспорта.** Семь baseline предметов и подтверждённые extras: law/source/version→feature/token distinction→plausible negative→mapping/representation/kNN hypothesis→mutable owner→ordinary result→evaluation families. Восьмитиповой task catalog R02 не подменяет предметные домены. Нужны реальные доступные source classes и честные NONE/UNKNOWN там, где corpus не найден.

**C08.3. Узкие semantic traces.** CP3 верно замечает, что R08-B не должен снова выполнять весь R04–R07. Принять точные sibling traces, самостоятельно проверить только критический доменный стык: сохраняется ли нужное различие после действующей projection и попадает ли оно к нужному consumer.

**C08.4. Verdict пары.** CP3 сообщает пробел R08-D: надо различать победу второго кандидата и пригодность пары как урока. Presentation-order preference (FIRST/SECOND/TIE/BOTH_BAD/UNJUDGEABLE либо эквивалент) и admission — разные поля. Не менять запечатанные стороны под старым assessment; авторский label chosen ещё не гарантирует правильного предпочтения.

**C08.5. Bulk examination.** Наполнить protocol конкретными family units, lawful/mixed/OOD cases, closed-vs-operational split, same-memory control, quality/mapping/path/cost/retention и заранее установленным uncertainty/stopping rule. Общий список metrics уже есть; теперь нужны task-specific recipes, не новые обязательные проценты.

**Приёмка:** предметные паспорта, источник контраста, precise recipient/consumer и различающий опыт. Не требовать измерений, пока раунду поручено проектирование; не считать placeholders законченным проектированием.

## R09 — восстановить проверяемую цену и причинную цепочку

**Основание:** [FINDINGS][r09] и [SOURCES][r09s]. Полезное исследование следует сохранить.

**C09.1. Воспроизводимость 30-run sample.** SOURCE row хранит агрегаты, но не исходные 30 строк с run IDs/timestamps/status/attempt и вычислением duration. Нужен embedded table или ссылка на сохранённый raw snapshot. Показать cancelled/in-progress handling; различать workflow elapsed, сумму job durations, queued time и runner occupation. Если job-level данные не получены, не называть workflow durations фактическими runner-minutes.

**C09.2. Не подменять старое окно новым.** Если исходную first-30 выборку нельзя восстановить, исходные цифры остаются historical reported/unreproduced; новая выборка маркируется отдельно. Не запускать CI для этого.

**C09.3. Test-fitting episode.** По существующему requirement→failed assertion→exact diff→authority→следствию классифицировать конкретный случай либо оставить UNRESOLVED. RED→GREEN не доказывает подгонку; один native defect не оправдывает любой Windows gate.

**C09.4. Цена до полезного learning signal.** Связать затраты с причинным путём урока и exact clauses KEEP/REUSE/DEFER/REPLACE-AS-EVIDENCE, owner и return trigger. Время человека/счёт/PC training footprint нельзя выводить из CI длительностей. Отсутствующие измерения честно передать R23.

**Приёмка:** пересчитываемая bounded sample либо явное понижение количественного вывода; предметная dependency/episode classification. Никакая кампания покрытия не добавляется.

## R10 — точечный re-join, не новый общий аудит

**Основание:** [FINDINGS][r10] и [CHANGE-MAP C01–C25][r10c]. Nine-way integration действительно существует. Общая исследовательская неполнота явно сохранена.

**C10.1. Добавить новые версии по смыслу.** R07 revision 2 — work-order-only, R08 CP3 — новые прочтения/диагноз, не законченная domain research. Обновить только затронутые status/claim receipts. Старое R10 нельзя использовать как подтверждение последующих результатов.

**C10.2. Принятие downstream.** Change map уже содержит 25 строк — не создавать такую же заново. Для критических правил добавить exact source finding/checkpoint→решение→точный destination packet/section→remaining condition. Статус LOCKED описывает правило, не фактическое исполнение потребителем.

**C10.3. Сохранить два verdict.** Nine inputs observed / integration usable отдельно от research corpus complete / first lesson implementation-ready. Поздний R07/R08 результат может изменить гипотезу, не обнуляя все девять исследований.

**Приёмка:** только затронутые deltas и точные адреса следующего анализа. Полный rerun R10 не требуется; неопубликованные будущие results не являются сегодняшним дефектом join.

## R11 — выполнить текущий двухуровневый dependency contract

**Основание:** [RUN-LOG][r11l], [SOURCES][r11s], [DECISIONS][r11d] и [R11-REPAIR-WORK-ORDER][r11w]. Новый governing prompt прямо не выполняет R11 сам по себе.

**C11.1. Свежий R10 receipt.** Историческое «R10 не наблюдался на старте» было честным. Сейчас R10 есть: принять его explicit gaps/decisions, обновить текущую передачу, не удаляя историю.

**C11.2. Два явных DAG.** FIRST-LESSON и FULL-DELIVERY должны иметь actual task/capability IDs и edges. Нужна полная адресация перенесённых обязательств трёх линий: owner, trigger, retained safeguard. Список общих «defer Windows/encoder» недостаточен как карта всей программы.

**C11.3. Minimal cut-set условен выбранным recipient.** Document learner, routing policy и graph-selector используют разные state families. Для каждого retained prerequisite показать, какое нарушение вызывает его удаление. Минимальность первого пути не означает глобально оптимальную архитектуру.

**C11.4. Не создавать циклы.** R11 задаёт causal alternatives; R12 выбирает owner/design; R13 уточняет recipe. Каждый может исследовать независимо; финальный зависимый вывод принимает нужный конкретный receipt, не глобальный DONE всего corpus.

**Приёмка:** два согласованных DAG и конкретная cut-set таблица с falsifiers/owners; свежая версия R10 учтена. Не объявлять историю old bounded R11 доказательством исполнения нового repair contract.

## R12 — повторно выполнить архитектурный синтез A–F

**Основание:** [RUN-LOG][r12l], [SOURCES][r12s] и полный [R12-REPAIR-WORK-ORDER][r12w]. Публичный статус: REPAIR_PUBLISHED / REEXECUTION_REQUIRED.

**C12.A. Authority reconciliation.** Original task/DPO→WorkLeft→RAG successors→current code. Разрешить scoped helper bans, semantic compression, BM25+kNN, positive/negative/warning memory, shared/project LoRA, historical70/30 vs new-lessons/no-repeat. Chinese authorship не открывать заново.

**C12.B. Четыре solved causal contracts.** Input/domain→deterministic admissibility→learned authority/state→objective/signal→mutation→storage/version/rollback→ordinary output. Actual/proposed/unknown отдельно; Train не размножается на четыре trainers.

**C12.C. Exact DPO lifecycle.** Зафиксировать semantic recipient, response objective/masks/reference, quality/admission/preference separation, preserved distinction, applied-once/recovery и ordinary adoption. LearningTrainingItem/GraphDpoPair/sidecar согласовать по фактическому пути, не добавлением UnitId наугад.

**C12.D. Domain-parameterized family.** Заполнить law-sensitive features/normalization, candidate eligibility, score/k/radius/state, memory/arbiter, exact reference, ANN assumptions, many→many output и mixed/OOD behavior для целевых families. Это экспериментально выбираемые кандидаты, не заявление «найдена лучшая функция».

**C12.E. LoRA/projection/PC.** Указать source/target representation, parameter group, frozen/shared/project state, actual loss и downstream projection consumer. LoRA сама по себе не tokenization. Посчитать символический полный бюджет и указать требуемые измерения/лимиты среды, не выводить PC-fit из малого adapter.

**C12.F. Выбор первого опыта и связи с полной архитектурой.** E1 DPO/adoption; E2 domain/vector/kNN; E3 Flow/Orchestrator; E4 higher-order Graph. Выбрать первый по causal bottleneck и ожидаемой информативности/стоимости, а не только близости удобного кода. Остальные не все prerequisites первого.

**Дополнительный receipt:** R12 SOURCES текущего ремонта не фиксирует актуальный R10/R11. Перед окончательным выбором добавить их точные применимые выводы, не принимать старый CP1 за новое согласование.

**Приёмка:** решения A–F реально заполнены и согласованы; одна первая implementation/examination пара готова к следующему владельцу, остальные связи имеют конкретные кандидаты. Непроведённые эксперименты остаются NOT_MEASURED, но неопределённые owner/objective/consumer не прячутся под словом «проверим позднее».

## Порядок продолжения без нового затора

**Техническая полоса первого урока:** R05 уточняет faithful input/objective/ordinary document consumer; R07 независимо исследует связи/recipient и затем принимает R01–R06 subset; R08 готовит один наполненный domain dossier и параллельно остальные; R12 выполняет выбор архитектуры; R11 обновляет причинные зависимости, R10 интегрирует только дельты. Live experiment проектируется следующим R13; implementation work order — R18. Этот аудит не выдаёт разрешение на запуск обучения.

**Полоса полноты программы:** R01/R03 продолжают inventory/primary traversal; R02 принимает requirement deltas; R04/R06 закрывают свой representation/domain/slice остаток; R09 восстанавливает bounded costs. Она не блокирует каждый локально доказанный шаг, но её обязательства нельзя вычеркнуть из FULL-DELIVERY.

**Рабочая единица продолжения:** один unresolved question и его consumer. Внутренние части не объявляются новыми Rxx или независимыми model calls. При настоящем делегировании child получает полный prompt/context/skills/sources/good-bad-edge/examination/rights/return schema. Незавершённый checkpoint сохраняет остаток, не переписывает очередной раз всё задание.

## Какая функциональная приёмка нужна вместо coverage

Нужны разные наблюдения: качество mapping при predicted/oracle/mixed/OOD domain; сохранение law-sensitive различий в actual tokens/vectors; полезность дополнительных соседей при fixed pool; DPO pair quality отдельно от update usefulness; semantic output после адаптации LoRA; Flow correction при frozen Units; total branch cost Orchestrator; transfer новых Graph compositions; ordinary-consumer adoption и retention после новых bulks.

План опыта заранее задаёт baseline/challenger, источник независимости families, fixed/changed factors, acceptable quality и cost, negative/edge cases, uncertainty и stop rule. Допустимы IMPROVE, REGRESS, INCONCLUSIVE, INVALID_EVIDENCE. Не обязательно улучшение каждого bulk. Equal-quality deterministic solution может победить; shorter unlawful path не выигрывает по цене. Hard invariants/provenance/rollback проверяемы адресно, но их исправность не является доказательством обученной способности.

## Источники аудита

Ссылки ниже закреплены на прочитанных commit, а не на будущем плавающем HEAD. Полные source IDs/primary paths предыдущих workers разрешаются внутри их SOURCES и source appendices; это атрибутированная база.

[r01]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R01/FINDINGS.md
[r02]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R02/FINDINGS.md
[r02d]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R02/DECISIONS.md
[r03]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R03/RUN-LOG.md
[r04]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R04/FINDINGS.md
[r04l]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R04/RUN-LOG.md
[r05]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R05/FINDINGS.md
[r06]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R06/FINDINGS.md
[r06d]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R06/DECISIONS.md
[r07]: https://github.com/lubochka/xiigen-planning/blob/c1a428a9f753bd97bb46544b02e47705d7416128/llm%20mvp/design%20to%20code%20session/replanning/results/R07/FINDINGS.md
[r08]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R08/FINDINGS.md
[r08s]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R08/SOURCES.csv
[r08l]: https://github.com/lubochka/xiigen-planning/blob/c1a428a9f753bd97bb46544b02e47705d7416128/llm%20mvp/design%20to%20code%20session/replanning/results/R08/RUN-LOG.md
[r09]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R09/FINDINGS.md
[r09s]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R09/SOURCES.csv
[r10]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R10/FINDINGS.md
[r10c]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R10/CHANGE-MAP.csv
[r11l]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R11/RUN-LOG.md
[r11s]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R11/SOURCES.csv
[r11d]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R11/DECISIONS.md
[r11w]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/R11-REPAIR-WORK-ORDER.md
[r12l]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R12/RUN-LOG.md
[r12s]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/results/R12/SOURCES.csv
[r12w]: https://github.com/lubochka/xiigen-planning/blob/d849fadfec11f31043e9de2a5fec1c08dfa56d12/llm%20mvp/design%20to%20code%20session/replanning/R12-REPAIR-WORK-ORDER.md
[join]: https://github.com/lubochka/xiigen-planning/blob/c1a428a9f753bd97bb46544b02e47705d7416128/llm%20mvp/design%20to%20code%20session/replanning/PARALLEL-R01-R03-JOIN.md
