# После P117: покрытие и прямое усиление карточек

**Дата:** 3 ноября 2026. Основной source core879a07b / planning66c53bb. [Аудит138](codex-138-post117-transfer-and-experiment-audit-2026-11-03.md) и [план139](codex-139-post117-addressed-repair-plan-2026-11-03.md) объясняют находки. Этот файл показывает **что действительно изменено**, а не сертификат всех опытов.

## 1. Глубина проверки

| Область | Выполненная работа | Что не заявляется |
|---|---|---|
| P114–P117 | Program/runner/verifier/raw; независимый пересчёт сохранённых чисел и текстовых состояний; причинные и смысловые ограничения | Новый Train, native C#/PowerShell execution, новая byte-проверка backing LFS weights |
| Изменения product после POST113 | CAS/интерфейс/semantic key/ValueKind/zero-weight/material guard/lexical snapshot/UTF-8; реальные owners и тестовые ветви | Успешная сборка текущего product и полный end-to-end PASS |
| P108–P113 и прежние S/R | Адресная повторная проверка открытых ветвей runner/numeric/owner/material/channel/admission/import | Повторный полный аудит всех ранних130 результатов |
| P104–P125 |22 карточки: собственный positive/corrupt/limit и действующий copied prompt |22 заново исполненных опыта или доказанное закрытие всех подслучаев |
| possibleDifs |16 существующих документов усилены,1 новый audit; связанные owners и work orders согласованы | Полная актуальная аттестация всех исторических source claims планирования |

Публикационная дельта c257523 проверена отдельно:5 файлов, actual text binding признан реализованным; PUB01/PUB02 имеют pure witness, не native PASS. R04/135/registry/P112/planning обновлены по фактическому остатку.

Дополнительная дельта918:6точных blobs (2product,1test,docs,status/outcome); S03/S06 source fixes признаны, конкретная приёмка открыта. P118 переведён в recorded-external-unreviewed, текущий prompt saved-only, budget/gold таблицы сохранены. Его новый Program/raw не аттестованы. Исторический независимый review P118 был выполнен до этой поздней правки; её статус/очередь и таблицы проверены основным автором отдельно.

Научный pin не плавает вслед за HEAD. Новые commits между аудитом и публикацией описываются отдельной дельтой; их файлы не подменяют исходник основного saved-data пересчёта.

## 2. Core:22 карточки с конкретным исправлением договора

В каждой карточке добавлен блок POST117 и изменён **текст действующего передаваемого задания**. Полный договор44 присутствует внутри copied prompts. Индивидуальные условия применяются по механизму: например, project+file+position нужен переходам, а не любому произвольному материалу.

| Карточка | Уточнённый контракт | Минимальное различающее повреждение/проверка | Глубина |
|---|---|---|---|
| [P104](../experimets/cards/P104-law-eligibility.md) | Eligibility и обязательный итог | При прежних числе кандидатов и score заменить только law/task/schema identity; рабочий admission должен отказать. Missing lawful-positive либо failed mandatory law check запрещает общий VALID. | Адресный старый остаток и профилактика |
| [P105](../experimets/cards/P105-frozen-reference.md) | Замороженный reference и actual state | Подменить один reference key/байты при прежнем count; проверяющий обязан отказать и изменить общий verdict. Frozen-changed не остаётся warning. | Адресный старый остаток и профилактика |
| [P106](../experimets/cards/P106-corrected-semantic-target.md) | Исправленная цель, ключ и версия | При том же количестве строк изменить source/key/schema; до mutation получить typed отказ. Floor-negative применять только после положительного доказательства существующего кандидата. | Адресный старый остаток и профилактика |
| [P107](../experimets/cards/P107-lawful-neighbor-aggregation.md) | Полная панель соседей и причинная граница | При том же selected ID заменить output или убрать одну строку панели; рабочий comparator обязан отказать. Shuffled outputs/first-hit controls не заменяются counts. | Адресный старый остаток и профилактика |
| [P108](../experimets/cards/P108-metadata-carrier.md) | Carrier, receipt и стадия | JSON null/object/wrong element у metadata даёт контрактный отказ; null result, build-only receipt, unknown observation и живой dummy descendant не дают successful terminal. | Адресный старый остаток и профилактика |
| [P109](../experimets/cards/P109-gold-validity.md) | Gold и независимость всех стадий | Чистая KNN таблица при exam-family в tokenizer fit должна проваливать independence; missing adjudication и non-finite confidence не превращаются в VALID. | Адресный старый остаток и профилактика |
| [P110](../experimets/cards/P110-numeric-matrix-cost.md) | Численная мера с конечным допуском | NaN actual, NaN/+Infinity tolerance, лишняя/пропущенная координата при прежнем заявленном N должны отказать через NumericOutputVerifier. Honest/zeros/identity должны проходить. | Адресный старый остаток и профилактика |
| [P111](../experimets/cards/P111-reward-credit.md) | Адрес feedback и полный After | Добавить только лишний After owner либо изменить незарегистрированный key при прежнем selected owner; AddressingVerifier обязан отказать. Selected-U1/U2 positive остаётся. | Адресный старый остаток и профилактика |
| [P112](../experimets/cards/P112-family-disjoint-evaluation.md) | Семья фактически потреблённого материала | При прежних claimed hash/N заменить consumed row; отдельно поместить TEST-family только в tokenizer fit. Оба случая должны отказать; честный семейный split проходит. | Адресный старый остаток и профилактика |
| [P113](../experimets/cards/P113-feature-energy.md) | Вектор и энергия полностью | Поменять одну координату или распределение каналов при прежнем total energy/claimed fraction; ChannelReceiptVerifier должен различить honest и corrupt. Missing channel недопустим. | Адресный старый остаток и профилактика |
| [P114](../experimets/cards/P114-zero-weight-pair.md) | Нулевой вес, all-zero и полный расход | All-zero у copy не должен вызывать optimizer; NaN receipt и false verifierOk должны менять итог. Honest B/C parity сохранить. Сравнивать Loss только с той же UnscaledLoss-осью. | Source + saved-data audit |
| [P115](../experimets/cards/P115-bm25-statistics-cache.md) | Статистика, независимость и полный паритет | Trusted revision отдельно; NaN average/tolerance, ghost doc, same-count changed text, другой output того же ID и обрезанная query panel должны отказать. Корпус с U+001F/U+001E имеет однозначную identity. | Source + saved-data audit |
| [P116](../experimets/cards/P116-e43-file-boundary.md) | Файл, проект, replay и две физические попытки | Project swap при прежнем file label/tokens, duplicate key, malformed hash и JSON null результата должны отвергаться. Honest six boundary controls сохраняются; новый Train для JSON-ремонта запрещён. | Source + saved-data audit |
| [P117](../experimets/cards/P117-translation-direction-memory.md) | Достигнутая память, actual export и полный смысл | Same-count target/signature mutation должна ломать actual-export/oracle сверку; каждый false copy/reload/control ломает итог. Будущий guard требует кандидата в той же реальной ячейке и отдельного FIFO-контроля. | Source + saved-data audit |
| [P118](../experimets/cards/P118-projectwiki-content.md) | Потребление lesson payload обычным планом | При прежних ID/count/style заменить validate→save на validate→notify: reader обязан показать изменение содержания, evaluator задачи — отказ неверному плану. Missing trace не даёт VALID; meaningful overlap positive сохраняется. | Поздняя recorded external попытка918; научный аудит не выполнен |
| [P119](../experimets/cards/P119-learning-dynamics.md) | Учебная динамика, общая доза и корректная loss | NaN/Infinity loss или изменённый frozen reference должны отвергаться финализатором; same-stage/scaling loss сравнивать отдельно от уже домноженной. No-update positive сохраняет состояние. | Профилактика будущего назначения |
| [P120](../experimets/cards/P120-semantic-compression-laws.md) | Смысл после сжатия и фактические токены | Неизменный honest ASCII/control проходит; not/no и distinct operation payload проверяются собственным gold. Подмена payload при том же op/ID и пропуск операторного различия не дают семантический PASS. | Профилактика будущего назначения |
| [P121](../experimets/cards/P121-project-adapter-selection.md) | Проект, адрес адаптера и реальное восстановление | При прежних count/hash-labels заменить adapter owner/project или один чужой tensor; отказ isolation обязателен. Lawful explicit sharing проходит, unknown project получает typed unbound. | Профилактика будущего назначения |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | Unicode, координаты и действующий consumer | Unicode marks/digits/operators и неизвестный token ID имеют явный контракт; одинаковое число активных slots при разных roots не считается semantic improvement. Honest ASCII контроль сохраняется. | Профилактика будущего назначения |
| [P123](../experimets/cards/P123-design-conversion-critic-evidence.md) | Геометрия действительно доходит до critic | Изменить геометрию при прежних flags: если consumer её не читает, learned-claim не проходит. Missing geometry/control и constant verdict не могут дать VALID. Honest заранее заданный случай сохраняется. | Профилактика будущего назначения |
| [P124](../experimets/cards/P124-p3-fixed-discriminator-contract.md) | Контрактное правило и изменение learned state | Тот же verdict при изменённых contract/feedback/actual state должен проверяться против fixed-rule baseline; missing feedback trace не даёт learning PASS. Честное заранее данное правило может законно не учиться. | Профилактика будущего назначения |
| [P125](../experimets/cards/P125-executor-memory-learning.md) | Записанный alias, CAS и восстановленный executor | Два staged writer захватывают один base; поздний проигрыш CAS сохраняет доступный candidate и не меняет победившее состояние. Early expected-version refusal — отдельный positive. | Записанный alias; продолжение приёмки |

Новые model/native controls не прибавлены молча к прежним caps. Чистые проверки идут по сохранённым/синтетическим receipt без модели; необходимый дополнительный native срез получает отдельное конечное назначение. Его отсутствие фиксируется как UNRUN/NOT_OBSERVED.

## 3. Действующие входы и противоречия статусов

| Файл | Что исправлено |
|---|---|
| fixes README и01-registry | Совместимый EvaluateAsync3b4d2b8 больше не первый отсутствующий fix. Q117 индекс закрыт879a07b; настоящее продолжение — приёмка. Общий порядок внешнего реестраQ01–Q66 сохранён; R117-01/02 задают подготовку научной приёмки внутри своих владельцев |
| 135, текущий R01 | CAS сохранён; прежнее предложение общего writer gate помечено историческим. Поздний конфликт и checkpoint candidate описаны139; уже исправленные signature/schema не назначаются заново |
| 03-test-and-execution-contract,40-effective-executor-context,43-audit-executor-repairs | Договор44 включён в обязательный вход: actual state, finite/keysets, finalizer, receipt/ledger и saved-only |
| Новый44 | Общие требования с областью применимости, собственным positive/corrupt, раздельными научным/процессным/product статусами |
| P116/P117 и поздний P118 паспорта | RESULT_RECORDED вместо активного DESIGNED_NOT_RUN; историческая постановка сохранена, действующее задание — ремонт saved evidence |
| P116/P117 и поздний P118 copied prompt | Первоначальный общий запускной текст заменён current saved-only prompt; точная историческая версия доступна по pinned GitHub ссылке |
| 00-execution-index |На879a117/13; после918 текущие118recorded/12designed из130; порядок/ID не изменены. В Q114–Q117 прямо записаны ограничения и общий расход, прежнее «перерасход0» уточнено |
| Новый138/139/140 | Факты, выбранные адресные решения и охват раздельны; исходные raw не переписываются |

P125 — alias уже выполненного опыта, не Q125. Ни этот аудит, ни новая ссылка44 не превращают его в новый PRIMARY.

## 4. Planning: точный перечень17 документов

Пути ниже принадлежат ветке llm_plan3. Основной новый отчёт содержит source permalinks, научный пересчёт и порядок ремонта; соседние карточки усилены по соответствующему механизму.

| Документ | Изменение |
|---|---|
| [codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [30-MICRO-SIX-DIRECTION-KNN-LEARNING-QUALITY-EXPERIMENT-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/30-MICRO-SIX-DIRECTION-KNN-LEARNING-QUALITY-EXPERIMENT-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [2026-11-02-P108-POSITIVE-TRANSFER-DELTA.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-11-02-P108-POSITIVE-TRANSFER-DELTA.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [2026-11-02-P110-P113-POSITIVE-TRANSFER-DELTA.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-11-02-P110-P113-POSITIVE-TRANSFER-DELTA.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [README.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/README.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [20-MICRO-FUSION-CAUSAL-EXPERIMENT-AND-CODEX-WORK-ORDER.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/20-MICRO-FUSION-CAUSAL-EXPERIMENT-AND-CODEX-WORK-ORDER.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [2026-11-02-EXECUTOR-CONTEXT-FOLLOWUP.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-11-02-EXECUTOR-CONTEXT-FOLLOWUP.md) | Уточнение текущего статуса, адресных контролей и границы доказательства; первоначальные данные сохранены |
| [codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) | Новый самостоятельный аудит и маршрутизация138–140/44 |

В N20/N30 исправлены сами copyable work orders; в N32/N34 ограничение saved-evidence/назначенного этапа стоит непосредственно у последовательности запуска. N10/N41 уточняют actual restore и позднюю публикацию; N16 не приказывает заново писать уже существующий zero-filter. N20 cache не закрывает отдельную проблему нормировки candidate pool.

## 5. Проверка автором и независимая проверка

Второй аудитор независимо сверил138/139/44 с P115–P117 и выявил плановые ловушки, исправленные до публикации:

- Boolean terminal нельзя описывать как enum; enum относится к state.
- В P115 четыре строки, а не пять; duplicate QueryId имеет собственный отказ.
- Namespace не добавляется незаметно в существующий KNN checkpoint и RU→EN factor; это отдельный N30-B/P117.2.
- Обычный SaveCheckpointAsync после CAS читает CurrentVersion победителя. Кандидат строится из explicit candidateVersion/stagedRuntime; ExportedAtUtc не входит в сравнение семантического состояния после нового экспорта.
- Общий provenance договор учитывает schema своего владельца и не требует project/file у любого DPO/cache.
- Recount P114 поддерживает как LFS pointers, так и materialized binary checkout; в реально проверенном срезе были pointers и это отражено в результате.

Проверены Python recount, ссылки новых документов, точные130 Q и неизменность IDs/очереди/статусов по отношению к научному смыслу, отсутствие product/raw edits. Это документная QA и saved-data проверка, не выполненные нативные regression tests.

## 6. Как пользоваться покрытием

При поручении конкретного ремонта читать его owner в139, собственную карточку и44. Текст139 задаёт вход/выход/negative/positive/budget и границу приёмки;140 только помогает найти адрес. Завершать можно только проверенный контракт. Не закрывать соседние направления, весь native E43 или полную архитектуру по одному успешному helper.

## Итоговое согласование с параллельной публикацией84f

Публикационное согласование84f: внешний реестр переписан владельцем в114самостоятельных работ с единой очередьюQ01–Q66; эта структура сохранена, R117 нанесены как уточнения существующих владельцев. Внешняя строка результатаP118 сохранена; report уже опубликован. Из84f проверены diff путей, пересекающиеся документы и наличие report, не новый ClaimCorrectionReader и не научная реализацияP118. Сохранены все114основных work-ID внешнего реестра; новых дублей в этот счёт не добавляли. P118 data/budget tables не изменены, текущий prompt проверяет уже существующий report. Прежний независимый readback не приписывается этим последним согласовательным строкам; их проверил основной автор.
