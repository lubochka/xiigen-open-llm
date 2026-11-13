# После P128: исполнение переносов, точность опытов и усиление карточек

Дата: 3 ноября 2026. Planning source: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; последующий `c5e7113` добавляет только handover. Результат — адресная сверка, обновлённый план ремонта и действующие задания карточек; продуктовые исправления не объявляются выполненными.

## Поздняя дельта учтена до публикации

Core [b4af9eef](https://github.com/lubochka/llm_mvp_core/commit/b4af9eef70f52020e3b10558fe86287dfb054023) исправил BOM/hash единым Utf8BytesOf для sync/async; опубликованная запись сообщает Release26/29 до и 36/36 после. Это правка другой линии и сообщённая целевая приёмка; rawTRX не перечитан данным аудитором. CASwinner, lateEvaluate, ordinary-storecomposition и независимый childrestore ещё открыты. Новый каркас P129 не содержит результата. Planningmerge95f992c с QA-уточнениями сохранён. Основной sourcef878 остаётся основанием большого аудита, b4 — отдельно проверенным обновлением статуса.

## Основной вывод

Сохранённые P126–P128 содержат настоящее исполнение и полезные измерения. Повторный аудит подтвердил прежние product-остатки CAS provenance, late Evaluate cancellation, ordinary-store composition и owner identity; найденная на f878 BOM/hash-причина уже исправлена в b4. Новый путь owner-пробела — public admitted constructor без recipient. Новые дефекты: потеря SourceRowId при одинаковом содержании одной семьи, чрезмерный запрет selective Dev/Test policies, неполная сверка полного restore и отсутствие связи обязательных результатов с окончательным exit в P126/P128.

Основной пакет в core:

- [145 — аудит, подтверждения и границы](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-145-post128-transfer-and-experiment-audit-2026-11-03.md).
- [146 — адресный план ремонта](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-146-post128-addressed-repair-plan-2026-11-03.md).
- [147 — покрытие изменённых карточек](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-147-post128-card-coverage-2026-11-03.md).
- [Воспроизводимый пересчёт и независимые просмотры](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/experiments-v2/_audits/post128-2026-11-03).

Восемь ключевых продуктовых файлов совпали с прежним срезом 9d4d79d. Старые находки поэтому названы открытыми, не новыми регрессиями. Новое подтверждение всегда различает прочитанный source, пересчёт saved results и нативный запуск. В этой среде build/discovery завершились `dotnet: command not found`, PowerShell также отсутствует. **Свежая нативная приёмка не выполнена**; точные ошибки записаны, Release-проверка включена в 146. Это не запрет на тесты и не новая просьба разрешить уже назначенную работу.

## Сохранить полезные результаты и правильные знаменатели

| Опыт | Что подтверждается | Что этим не установлено |
|---|---|---|
| Q125 / P25 | Восемь уроков, 0→19/24, после свежей загрузки 19/24, буквальное совпадение 24/24 | Способность к неученным отрицаниям/вопросам. Наблюдённые неверные ответы с 0,99 не стираются |
| P126 | Два цикла,16 завершённых плеч,28 updates; равные конечные weights/RNG в A/B; margin вырос | Новый правильный полный ответ при исходном потолке 66/66 и 46/46; приёмка текущего продукта по старым HintPathDLL |
| P127 | Пары p1/p2 и h1/h2:1/2→2/2, C0/2; во второй попытке 9 реальных mandatorychecks | Общий языковой порядок и native save/load, для которого не было API |
| P128 | Старая копия cap256→2048:1/2→2/2; ordinary retention29/29 и exam2/2, сохранённый полный state совпал | Необходимость того же cap-ремонта в живом DP planner, который уже даёт 2/2; достаточность всех автоматических проверок |

Цена P127 за обе попытки с контролями: **10 fit,100 предъявлений,136 чтений**, отдельно 24 холодных наблюдения,24 graph/48 node и 6 receipt checks. P128: **1 batch,29 уроков,64 Flow,17 composer**, плюс 2 live reference calls. Это не расширение паспортов задним числом; полная стоимость расположена рядом с бюджетом каждой попытки. P119 имеет две попытки 8+8 updates/80+80 предъявлений при неизменной точности 2/4.

## Десять адресно усиленных planning-документов

| Документ | Новая обязательная проверка |
|---|---|
| [03 — DPO pair/gradient](03-DPO-PAIR-AND-GRADIENT.md) | Оба конструктора и actual owner/recipient при равных checkpoint hashes |
| [07 — continual replay](07-CONTINUAL-REPLAY.md) | Сохранение SourceRowId у законных повторов, роли и tuple provenance |
| [10 — translation/restore](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | Байты→полное состояние→обычный ответ→обязательный итог |
| [22 — KNN state identity](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | Same-count target/lineage/order и полное independent comparison |
| [27 — sequence protocol](27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md) | Выполненный P127 finalizer и фактическая стоимость обеих попыток |
| [27 — sequence/negation gaps](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | Порядок, личность одинаковых строк и предел вывода о способности |
| [39 — material/model identity](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) | Проверенные bytes не заменяют владение изменяемым объектом |
| [54 — causal microexperiments](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) | Working oracle, provenance и отказ всех уровней завершения |
| [59-02 — learning path integrity](59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md) | Потребитель каждого доказательства, свежая native QA и неизменная история |
| [59 — passports/prompts](59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md) | Сумма попыток/контролей и три отдельных вывода: процедура, гипотеза, перенос |

В core обновлены 18 действующих карточных блоков. Это 28 адресных карточных поправок данного этапа, не повторный подсчёт прежних 36 поправок аудита 142. Остальные документы получили навигацию, общие требования или датированные исправления; их число не включается в 28.

Паспорт активной попытки остаётся закреплённым на ранее выданном pin; новое описание не меняет уже исполняемый опыт и не требует его повтора. Данные, gold, первичные outputs и исторические бюджеты сохранены. Для каждой карточки передаётся конкретный honest пример, одна адресная порча, рабочий consumer и влияние на finalizer.

## Две очереди остаются раздельными

В основной таблице опытов 128/131 RESULT_RECORDED (97,7%), Q129/Q130/Q131 пока без результата в закреплённом срезе. Handover сообщает об активном Q129 и другом владельце Q131; аудит их не назначает повторно. В очереди переносов первой остаётся **135-R01-CONCURRENCY (+R17)**, далее Q02→Q03→Q06; Q04 приостановлен, Q05 зависит от него.

Исторический большой Debug-прогон сообщает 16338 обнаруженных,15615 pass,628 fail,95 skip. Его 14 новых классов по собственной таблице дают 12 полностью зелёных,124 теста,121 pass/3 fail. Сырой TRX недоступен этому аудиту: это проверенная арифметика опубликованной расписки, не новый прогон.621 прочее падение не атрибутируется без baseline. Новые QA-выходы должны иметь свой каталог, run ID и source/DLL/command связь; исторические artifacts не перезаписываются.
