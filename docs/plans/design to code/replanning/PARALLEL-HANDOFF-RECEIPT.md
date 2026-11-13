# Передача решений R01–R03: найденное должно попасть в поручение получателя

**13.10.2026 · CP-D этой авторской поправки. Не новый research round, не исполненный J03/R10 и не изменение чужих results.**

Применяется вместе с [PARALLEL-R01-R03-JOIN](PARALLEL-R01-R03-JOIN.md). Работа по общему сведению остаётся внутри R10; R04–R09 могут брать достаточные проверенные тематические checkpoints раньше. Этот документ не требует всем перечитать три исследования или создать шестой carrier.

## 1. Конкретный дефект, который исправляет передача

При подготовке адресно прочитаны:

- `results/R03/FINDINGS.md`, исходные строки 1–40, blob `6da9283cf430453fc337c0008d72b63f043aa7d3`: CP07 сам помечает traversal как IN_PROGRESS / SOURCE_TRAVERSAL_NOT_COMPLETE. Ранее порядок паттернов, objective и fan-out существовали в SOURCES/CHANGE-MAP, но не были полностью перенесены в FINDINGS/DECISIONS и согласованный журнал. Следовательно, отсутствие в кратком отчёте не означает отсутствие ценного исследования.
- `results/R02/FINDINGS.md`, исходные строки 1–32, blob `fec560b784b44ac95f2ae4ba1d4c0523b96f1b71`: сохранены четыре роли, domain-scoped memory, semantic order и требования программы; отдельно исправлено авторство final DPO sides и указан риск cold-start margin. Это атрибутированные source findings, не повторная проверка всех их первоисточников этой поправкой.
- `START-NEXT-RUN.md`, полный возвращённый текст запросами 1–70 и 71–155, blob `7a988cc952582680a3964535d554c821c056892b`: действующая база dispatch — **MODEL-RUN-PACKETS-CP5**, а прежние LEARNING-WORK-ORDERS/HYBRID/RECOVERED являются подробными источниками уникальных требований.

Это выборочная проверка передачи и актуального dispatcher, не завершение R01/R02/R03, всей библиотеки или code audit. Новые исходные версии сверяются по относящемуся изменению, не по общему числу commits.

## 2. Один небольшой receipt в существующих носителях

Worker продолжает свой snapshot/visited-set и владеет только `results/Rxx/`. В своём очередном FINDINGS/RUN-LOG он указывает для **каждой передаваемой уникальной мысли**, а не для каждой строки архива:

`origin_run + local_finding_id; originating_checkpoint; planning/core refs; primary source repo/ref/path/blob/span; read_depth; exact decision or hypothesis; recipient_round; question/seam; remaining uncertainty; suggested experiment or plan clause; write boundary`.

Получатель в своём уже существующем CHANGE-MAP/FINDINGS добавляет **receipt**:

`origin tuple → incorporated into own section/work order → preserved meaning → still-unverified primary span or conflict → next action/owner`.

Не менять обязательную CSV-схему COMMON-CONTRACT вслепую: использовать существующие поля/ссылку на именованный раздел FINDINGS. Не создавать отдельные пять carriers для каждого A/B. Это содержательное подтверждение применения знания, а не ещё один gate с количеством галочек.

**Окончание передачи:** либо решение реально вошло в назначенное поручение с сохранённым смыслом, либо есть точная причина, что не принято/не проверено, и ответственный. «Прочитал R03» без consumer не закрывает передачу. «Источник недоступен» не доказывает отсутствия реализации. Повтор одного blob в трёх отчётах не три независимых подтверждения.

## 3. Что передать сейчас без повторного решения уже решённого

| Сохранённая мысль | Получатель | Что должно появиться у получателя | Чего не делать |
|---|---|---|---|
| Четыре роли, typed many→many, исправляющие отношения, async combinations, higher-order graph/Builder | R04–R07 → R12/R16/R18 | Actual producer/consumer, schema/coordinate/owner map; отдельное причинное вмешательство для нужной роли | Не сводить всё к одному Unit, не придумывать четыре trainer |
| Domain-first positive/negative/warning memory, BM25+kNN и final arbiter | R06 → R12/R22 | Domain pool/metric/k/policy/history/arbiter отдельно; fixed-memory versus learned-policy comparison | Не объявлять universal cosine лучшим и не считать retrieved memory обучением весов |
| Semantic OrderedPatternIds, fan-out, recipient/selector versus winning solver | R05/R07/R08 → R12/R18/R22 | Сохранённый смысл порядка, реальный teaching target и контролируемые alternative paths | Не сортировать semantic order ради удобства и не наказать все edges проигравшего пути |
| Final chosen/rejected authorship уже разрешено действующим R02/START | R05/R08/R12/R16/R18/R21 | Обе окончательные стороны пишет разрешённая китайская модель; CODEX Astra/ultra выбирает урок и отдельно оценивает exact pair; actual provenance проходит consumer | Не переоткрывать вопрос автора без новой прямой дельты; raw Student response не автоматически final rejected |
| Cold-start policy/reference margin — отдельный вопрос, не качество пары | R05 → R12/R13/R16/R22 | Trace настоящего admission consumer; отдельная оценка semantic/material quality и learned margin | Не считать алгебраический контрпример уже воспроизведённым trainer bug; не отвергать качественную пару только из-за policy=reference |
| LoRA A/B projection уже найдена R03 как reuse seed | R05 → R12/R18/R22 | Actual layer/callers/state mutation/ordinary consumer; semantic projection и memory comparison | Не переписывать LoRA по умолчанию, не считать наличие класса доказательством learning или PC compactness |
| Старый blanket ban deterministic helpers конфликтует с нынешним hybrid заданием | R06 → R12/R16/R20 | Exact scope/authority comparison и выбранная граница helper versus learned selection | Не отменять всю preprocessing и не называть fixed heuristic learned authority |

Таблица задаёт **передачу исследовательских выводов**, не утверждает, что все primary/code traces уже выполнены автором этой поправки. Полные маршруты: START §§3.2–3.4, R02/R03 SOURCES и исходный PARALLEL join.

## 4. Разные snapshots, late findings и отсутствие взаимных блокировок

R01 инвентаризирует; R02 читает исходные задачи/решения непосредственно; R03 идёт от RAG к первичному тексту. Им не нужны готовые siblings. Совпавшие F01/H03 из разных файлов не один namespace: source aliases SRC-R01–SRC-R03, HYBRID:Hxx и RECOVERED-CONTRACT:Axx остаются различимыми.

При новом checkpoint получатель сравнивает относящиеся source blobs/spans и изменённую трактовку. Только новый RUN-LOG commit не обнуляет прежнее чтение. Если меняется существенная policy/схема, отмечается конкретный затронутый вывод/recipe; исправляются его зависимые потребители, не весь план.

R10 фиксирует входные версии и объединяет уникальные решения с их receipts. Поздний материал после cutoff получает точный owner/вопрос. Уже фактически выполненный отдельный J03 переиспользуется с честным дополнительным call count. Недостающий обязательный источник не прячется: сначала адресное перераспределение внутри имеющихся раундов, затем обоснованная extension-карточка по PLAN-25-RUNS при реальном уникальном остатке. Дополнительный round не возникает из переименования checkpoint.

## 5. Что именно dispatcher берёт из новой поправки

Действующий полный base остаётся **общий блок MODEL-RUN-PACKETS-CP5-01-10 + собственная CP5 Rxx карточка**. Новые [R01–R10 additions](ROUND-DELTA-01-10.md), [R11–R25 additions](ROUND-DELTA-11-25.md) и [learning examination](LEARNING-ACCEPTANCE-DELTA.md) являются адресными подробными источниками для уточнения того же Rxx, не второй обязательной очередью.

Указания на LEARNING-WORK-ORDERS в заголовках новых additions относятся к предметным источникам, а не отменяют актуальную CP5 базу. При сборке использовать установленную в START иерархию: **CP5 base → уникальное относящееся содержание прежних/новых источников → один deduplicated effective prompt**. Не передавать одновременно две полные конкурирующие common sections. Формальные обязанности и source routes сохранить; повторы не увеличивают context.

Новая карточка уточняет causal control, meaningful counterexample, scope, subtask acceptance и owner. Она не возвращает уже решённое final-pair authorship в состояние UNRESOLVED. DPO objective и current one-pair/no-repeat policy не меняются научной формулой или словом bulk. Requested model label не доказывает actual invocation; независимость рецензента устанавливается по реальному запуску.

Результат для модели — **полный один prompt**, содержащий раскрытые prompt/context/skills/refs/examples/edges/outputs/examination, а не список ссылок. Родитель наполняет его доказанными source spans; при реально делегированной A/B задаче выдаёт ребёнку полный её пакет и return-only полномочия. Человек не склеивает документы вручную.

## 6. Проверка receipts без нового review treadmill

Один документный walkthrough внутри соответствующего Rxx проверяет:

- R03 нашёл semantic order — у R07/R22 есть именно semantic-order contrast, а не только слово graph.
- R02 исправил автора пары — R05/R08 проверяют actual provenance consumer, а R12 не выбирает автора заново.
- R03 показал LoRA class — R05 ищет реальный update/consumer, не пишет второй LoraLinear и не заявляет measured gain.
- R01 ещё IN_PROGRESS — R04 работает с достаточным проверенным representation subset, не ждёт unrelated inventory.
- Новый empirical result отсутствует — сохраняется NOT_MEASURED и готовый различающий опыт, а не 30/50 новых reviews.

Принятый receipt подтверждает пригодность передачи знания. Он не подтверждает runtime, полезность DPO, независимую Astra экспертизу, компактность или улучшение модели. Первый достаточный recipe R13 и implementation packet R18 после отдельной авторизации не должны ждать завершения всей программы R25.

## 7. Пять checkpoint подготовки этой поправки

CP-A — контракт причинной оценки (`ab85cfaac803ec978d095b63371b48f9c9b160cf`). CP-B — десять research additions (`39ee66cda4d50c7b8cbf3b38b02318c625271481`). CP-C — пятнадцать analysis/change/supplement additions (`c6a9c0643d8f9f7f33caebd99887263bfdd998ac`). CP-D — настоящий receipt и сверка актуального CP5 dispatch в этом документе. CP-E — подключение к действующему START и remote readback; до фактической записи это оставшийся шаг, не выполненный статус.

Это пять этапов авторской работы над планом, не пять независимых модельных исследований и не выполнение 25 будущих Rxx. Публикация первых трёх commits подтверждена ответами write API; выборочный immutable readback R01–R10 additions уже сделан. Финальная запись START и её проверка должны подтверждаться фактическим отдельным результатом, а не этой строкой.
