# Аудит покрытия и исполнительских контрактов — fb9175cc

> **Граница источников:** основной текст ниже относится к core `fb9175cc`. Перед публикацией отдельно проверен новый commit `d962f958`: [позднее уточнение](10-late-core-delta-d962f958.md), [актуальный реестр](04-registry.md). Исторические номера/статусы и счётчики 163 cards / 8 prompts не являются повторной аттестацией нового среза. NEXT сейчас — №10 PARENT-ADOPTION-01; старые raw и попытки сохраняются.

**Независимая проверка документов и исполнительских контрактов.** Защищённые документы core не изменены; обучение, inference, сборка, публикация checkpoint и продуктовые проверки не запускались. Уровень проверки ниже указан отдельно для реестра, текстовых признаков и содержательно прочитанных карточек. Главные действия в новом add-only пакете: обновить NEXT на №10, сохранить полезные результаты №09 и его отдельной пробы, дать конкретное облегчённое назначение обучения для новых факторов и устранить смешение private raw restore с production parent adoption. Текущий N63C остаётся назначенным acquisition recipe на canonical B0: отсутствие отдельного pretrained parent не добавляет к нему gate, warmup или updates.

## Срезы и границы доказательства

- Core: `fb9175cc752d41e1cb525e0609b7c96140940217`.
- Planning: `fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c`.
- Полностью прочитаны skills `phase-planning-SKILL.md` (1232 строки) и `plan-review-SKILL.md` (4274 строки). Из AGENTS прочитаны применимые разделы, **не весь файл**.
- Полностью прочитаны текущий execution-index, протокол 29, N62 saved passport, N63I, N63R; адресно — N63C, P02.PARENT и заголовки/данные frozen-transpose; полностью — ATTN executor prompt. Протокол 24 прочитан с формулами/счётчиками. У N63C/P02 в машинной таблице сохранены конкретные диапазоны, без утверждения независимого ревью всех source/API.
- Все 171 файла карточек/prompts структурно просканированы, каждому назначена роль в текущем реестре и exact line references. Это **не 171 семантический PASS**. В этом независимом потоке численные строки карточек проверены на смысл и ограничения. Основной автор отдельно сверил сохранённые raw; результат доступен в `data/saved-metric-reconciliation.json`, без повторного runtime.

Политики skills применяются с приоритетом текущего поручения: модель исполнителя — default текущей сессии, никакого нового override; экспериментальные копии и минимальный host допустимы; production ceremony не должна препятствовать маленькому учебному A/B. STOP-транскрипт в задании — материал для запрета повторения поведения, а не текущая команда закончить этот аудит. Live STOP/NEXT всегда немедленны.

## 1. Фактическая очередь продвинулась; planning должен догнать core

**CONFIRMED_STALE_DISPATCH.** В текущем core следующий неиспользованный ID — №10 `N63-CORRECTION-COLLISION`; №01–09 consumed, 42 разных core ID, 33 будущих. Это явно зафиксировано в [индексе:5–13](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5-L13) и [текущем паспорте №09:3–5](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L3-L5).

Последний planning overlay всё ещё назначает №09: [12-current-queue-and-n63-evidence:1–3,50,60](https://github.com/lubochka/xiigen-planning/blob/fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/12-current-queue-and-n63-evidence.md#L1-L3) и `data/resolved-dispatch.json` с `current_next.id=N63-AR-CAP`, `completed_id_count=8`, `remaining_id_count=37`. JSON получен отдельно целиком; его Git blob `3e84f8d7c5307fecf006d2404143a1f16801907c`.

**Добавочное исправление:** новый registry/dispatch задаёт NEXT10 и 9 consumed. Три прежних предложения A1/A2/A3 сохраняются, не перенумеровываются как новые находки. В опубликованном добавочном реестре после двух новых гипотез:42 core+5 proposals=47 строк,9 consumed,38 остаются. Completed не означает доказанный рост качества: №09 остаётся consumed даже при недействительном 0→0 вердикте исходного протокола.

Есть остаточная навигационная неоднозначность внутри исторических карточек: [N62:36](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L36) печатает исторический NEXT08, а current index уже NEXT10. Запрет повторов в N62:3–7 сохраняет безопасность данных; новый dispatcher должен прямо разделить historical successor и current next unconsumed, чтобы не отправлять исполнителя снова оформлять завершённые опыты. Это не новый learning bug и не основание повторять №08.

## 2. Полное количество карточек не равно полному покрытию готовыми опытами

| Объект | Точное количество | Что проверено |
|---|---:|---|
| P-карточки | 125 | Все перечислены; каждый файл содержит справочную/неисполнительскую роль |
| Специальные карточки | 38 | Все однозначно привязаны к 42 ID текущего индекса |
| Из них consumed files | 6 | Они представляют 9 ID: N62 объединяет четыре |
| Из них future files | 32 | Они представляют 33 ID: N61 содержит retention и identity |
| Итого карточек без index | 163 | Полный файловый и реестровый inventory |
| Отдельные executor prompts | 8 | Все привязаны к owner ID |
| Итого проверенных текстовых файлов | 171 | Структурная проверка; смысловые границы отдельно по строкам |

Сам index прямо отказывается считать все 470 подслучаев готовыми hosts: [78–82](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L78-L82). Например, [P02:99–117](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/P02-warm-base.md#L99-L117) оставляет author fields — subcase/domain, exact initial state, data, caller/command, mutation, RunId, counts — не заполненными. Наличие строки в P-family означает описанную гипотезу, но не полностью поставленный малый обучающий эксперимент.

**Предел покрытия:** данный аудит устанавливает 125 сохранённых семейств и 42 назначенных ID. Он не устанавливает, что все 470 подслучаев имеют отдельное сопоставимое A/B, законченный host или проверенную чувствительность метрик. В новом пакете уровни надо писать раздельно: `SOURCE_HYPOTHESIS_MAPPED`, `EXPERIMENT_RECIPE_ASSIGNED`, `LEARNING_PATH_REVIEWED`, `MEASURED`. Исполнитель не должен сам выбирать вариант из справочника.

Файлы результатов инвентаризации:

- `data/card-review-inventory.json`: 171 строка, queue mapping, formal marker line hits, source permalinks, отдельный semantic scope.
- `data/card-review-inventory.md`: читаемая таблица всех 171 файла.
- Генерация inventory использовала только текстовые файлы и текущий index; полные patterns и метод записаны в `data/card-review-inventory.json`.

В 171/171 найдено правило default model и STOP/NEXT; в 171/171 — текстовые указания ухудшений и до/после. Эти числа относятся к опубликованным regex. Старый первичный regex не узнавал правильное немедленное правило в N62/N63I/ATTN; ручное чтение показало ложные пропуски. Следовательно, отсутствие слова «бюджет» или команды у retired N63R не записывается как ошибка: запуск там намеренно отозван. Наличие слова `Train` тоже не доказывает правильную связь измерителя с learner.

## 3. Положительный learning signal: точное разблокирование вместо общего запрета

**UNQUALIFIED_DESIGN, а не доказанный runtime failure.** Ровно 15 future neural cards и 4 их prompts содержат `DESIGNED_UNQUALIFIED_LEARNING_SIGNAL` в строке 3. Это все 15 ID: N63-CORRECTION-COLLISION, LC-REF-CACHE, E-SFT-RESPONSE-MASK, E-SFT-MOMENT-CONTINUITY, E-DPO-VALID-LENGTH, E-DPO-SFT-ORDER, GRADB-LOCALITY-01, RMS-COORD-ALLOC-01, DPO-TOKENIZER-SNAPSHOT-01, N-HEAD-01, ROPE-TRAIN-TABLE-01, GRAD-ACCUM-COPY-01, FROZEN-BROADCAST-MAP-01, RMS-ROW-REDUCE-01, ATTN-TRANSPOSE-STRIDES-01. Exact per-card anchors находятся в JSON.

Протокол 29 уже корректно разделяет приобретение навыка/resource measurement и retention/restore: [7–14](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md#L7-L14). Из отсутствия pretrained donor не следует общий запрет настоящего обучения. Числа 2/4/9 — прежний авторский дизайн, не лимит пользователя. Одновременно честный заголовок `UNQUALIFIED` не доказывает достаточность выбранного budget для ненулевого результата.

Для новой SILU-карточки конкретное разблокирование, которое готовит основной автор, обоснованно должно использовать полностью назначенный H64 sign native recipe с **фиксированными 400 updates**, взятый из уже сохранённой LEARN-SANITY-PROBE. Это новый factor ID, собственный конечный бюджет A/B и source copy; не повтор NEURAL-FROZEN-TRANSPOSE-01 и не бесплатное использование чужих 400 steps. Текущая frozen-карточка описывает этот фактический сигнал в [32–34](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md#L32-L34). Полный checkpoint этой пробы не аттестован, поэтому нельзя выдать несуществующий `S0.bin` за уже готовый donor.

Предел известного сигнала: TRAIN 0/2→1/2 и EXAM 0/2→1/2 полезны по своим осям, но `+,+` и `−,−` равны constant baseline1/2; усвоение условного арифметического правила не доказано. Протокол 24 уже требует этого пояснения: [52–56](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md#L52-L56). Ресурсный эффект настоящих 400 updates имеет самостоятельный смысл при сохранённой траектории; ему не нужно приписывать рост conditional accuracy.

Не переносить 400 steps/H64 или H32/H16 donor на остальные 15 рецептов без назначения конкретной совместимой config/панели. Для каждого незавершённого дизайна автор уточняет learning mechanism, states и конечное расписание; исполнитель не ищет checkpoint, не повышает budget после B и не делает loop-until-PASS. Нынешний аудит ничего не запускает и не требует ждать native production parent перед всеми опытами.

## 4. N63C: вмешательство участвует в обучении; шаблон отчёта теряет обязательные поля

**REPORT_SCHEMA_CONFLICT.** N63C адресно задаёт изменение SFT.Response после настоящего DPO на той же policy. DPO-response mask и SFT full-sequence objective сознательно сохраняются разными; это часть исходного пути, не дополнительный fix. [Source chain и граница simulation:50–79](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L50-L79) не утверждают прохождение полного production IncrementalTrainer с внешним teacher. [Native update bridge:122–128](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L122-L128) содержит настоящие authority/material checks и реальные DPO/SFT calls. Отсюда гипотеза обучения допустима, несмотря на isolated experiment exceptions.

В [N63C:167](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L167) обязательны C=correct tokens, T=denominator, E=whole teacher-forced response exact, N=NLL, M=preference margin в каждом S0/S1/S2. Однако готовая таблица [185–189](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L185-L189) содержит только margin/NLL. Она не противоречит математике scorer, но её буквальное заполнение снова не даст пользователю обязательных ненулевых/нулевых ответов рядом с gain.

**Конкретное add-only уточнение без новых forwards:** дать в новом пакете этот единственный report table и включить его в локальный prompt:

| Split / row / показатель | A S0 | A S1 после DPO | A S2 после SFT | B S0 | B S1 после DPO | B S2 после SFT |
|---|---|---|---|---|---|---|
| TRAIN T1: C/T, E, N, M | — | — | — | — | — | — |
| EXAM Q1: C/T, E, N, M | — | — | — | — | — | — |
| EXAM Q2: C/T, E, N, M | — | — | — | — | — | — |

Все поля извлекаются из уже назначенных 36 side-score calls; дополнительных updates/readouts не назначать. M gain — усиление предпочтения, C gain — правильные отдельные токены, E gain — teacher-forced целый ответ. Ни одно из них само не является свободной генерацией. Для forgetting нужен соответствующий положительный S1 до SFT; без него `RETENTION_NOT_ESTABLISHED`. Если итог 0→0, это не допустимое доказательство освоения, хотя measured NLL и resource observations сохраняются отдельно.

## 5. Raw SFT restore не требует production adoption; P02.PARENT не блокирует его

Два разных будущих work orders должны быть написаны различно:

| Проверяемый вопрос | Минимальный честный experiment contract | Чего он не доказывает |
|---|---|---|
| Сохраняется ли выученное состояние в private A/B после загрузки? | Один назначенный compatible raw donor; config/tokenizer/LoRA topology; actual native load и optimizer import в свежие объекты; общая frozen panel; одинаковый A/B start; реальная учебная continuation по конечному recipe | Что штатный START примет этот donor как repository-owned parent |
| Примет ли штатный следующий START ранее обученный SFT parent? | P02.PARENT, private repo-shaped A/B, truthful SFT provenance/schema path, native manifests/authority/source continuation bundle, точный owner/entry | Что два arbitrary raw binary files уже являются таким bundle |

Первый путь уже представлен сохранёнными RESTORE-MINIMAL-SET и N63R-PROBE; импорт raw в экспериментальную модель не надо превращать в неподдельный DPO publication. Наличие m/v в бинарном файле и `TryImport=true` подтверждает факт импорта, но без отдельного post-import export нельзя приписывать ему уже выполненное побитное сравнение. Это предел существующего evidence, **не разрешение тратить новые tokens на hash-audit или повтор старой пробы**.

У второго пути настоящий узкий gap: [протокол 29:31–36,42–51](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md#L31-L51) требует truthful SFT provenance там, где current native manifest ждёт DPO applied identity. H32 WA назначен, PC01 paths назначены, entry ещё отсутствует. В isolated copy можно разработать экспериментальную адаптацию, но нельзя обозначить чужой DPO dataset как настоящий или назвать trained model `InitialBaselineNoUpdate`. Отсутствие этого контракта не блокирует rawLoadS0, Unit/KNN/BPE training или P25 cold consumer.

Вывод о «любом Step1/parent=null как необученном» также неверен: native первый обученный root может иметь такую пару полей; значение имеет отдельная verified InitialBaselineNoUpdate роль. Переносить этот metadata shortcut в список причин всех 0 ответов нельзя.

## 6. Как не потерять полезный №09 и не завысить его результат

Current N63R уже даёт правильно ограниченное описание, которое новая публикация должна сохранить. [Исходный №09:28–30](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L28-L30): 9 updates/32 readouts, все correct responses/tokens0. Реальное удержание A07 и его попадание в учебный sample — механический gain, но это не retention приобретённого навыка.

Отдельная [N63R-PROBE:38–60](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L38-L60) имеет 4600 updates/40 readouts. WA45/92old TRAINtokens,0/2whole exact. B лучше A сохраняет A07 и EO02, хуже усваивает N01/N02 и не защищает A01. C уравнивает updates B/C, но не число входных токенов, состав и порядок учебных строк. Это кандидатообразующий полезный retention эффект **отдельных токенов** с рядом показанными регрессиями и нагрузкой. Суммировать budgets двух протоколов как один A/B, называть probe результатом старых 9 steps либо назначать rerun запрещено.

Source table уже исправляет стоимость: updates+50%, input tokens+86.79%, allocations+106.77%, wall+131.86% у B против A. Основной автор сверил это с actual raw; результаты сохранены в `data/saved-metric-reconciliation.json`. утверждение «всё+50%» или «двойные updates» возвращать нельзя. Одно A→B→C измерение wall/CPU/process peak не устанавливает причинную устойчивость, но наблюдённую полезную дельту по своей оси скрывать также нельзя.

## 7. Минимальное правило для каждого нового prompt/card

Каждая новая карточка и каждый новый executor prompt должны включить правило самостоятельно, а не ссылаться только на общий README:

> Модель исполнителя — default текущей сессии, Student и native learner заданы отдельно. Отчёт — русский Markdown по локальной таблице, skills и их точные пути уже перечислены. Внешний DPO teacher не выдумывать; если calls0/fixture simulation, явно N/A для live-teacher роли. Измерять механизм, который действительно участвует в Train либо в адресно указанном получении/загрузке выученного состояния. Любое измеренное полезное улучшение — успех этой метрики и домена; accuracy может не вырасти, регрессии рядом. Не выдавать 0→0 или единичный constant-correct ответ за освоение общего правила. Работать только в назначенной экспериментальной копии, не чинить production/tests/CI до отдельного решения пользователя. Старые опыты/raw не удалять и не переигрывать. По «следующий» немедленно прекратить прошлый report/review/hash/Git/retry и перейти только к назначенному NEXT; по «остановить всё» немедленно остановиться. Недописанное оформление не разрешает продолжать прежнее задание. Одиночный ID заканчивается END.

Существующие DPO роли надо различать по фактическому пути, а не назначать Chinese live teacher каждому SFT/BPE/Unit/resource опыту. ATTN prompt уже делает это явно в [16](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/prompts/ATTN-TRANSPOSE-STRIDES-01-executor.md#L16). Распределение CODEX/Astra для author/judge не заменяет пользовательский default исполнителя. Имена старых models или `$DEFAULT_MODEL` без конкретной семантики не заполняют native Student config.

## Выход аудита

Защищённые оригиналы остаются read-only. Все исправления относятся только к новым possibleDifs docs: current dispatch, report schema и scope diagnostics/restore; новые научные гипотезы source audit готовят другие агенты. Результат этого прохода не утверждает новой успешной training run и не служит разрешением автоматического product transfer.


## N63C report table

Это назначенный добавочный шаблон следующего №10. Он разрешает только документный конфликт таблицы, не аттестует ещё не выполненный learning recipe и не увеличивает 36 уже назначенных side-score calls. Использовать его при будущем разрешённом опыте вместо укороченной таблицы margin/NLL; защищённый оригинал не редактировать.

| Split / row / показатель | A S0 | A S1 после DPO | A S2 после SFT | B S0 | B S1 после DPO | B S2 после SFT |
|---|---|---|---|---|---|---|
| TRAIN T1: C/T, E, N, M | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED |
| EXAM Q1: C/T, E, N, M | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED |
| EXAM Q2: C/T, E, N, M | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED | NOT_MEASURED |

C — correct response tokens; T — число оцениваемых response targets; E — whole teacher-forced response exact; N — chosen response NLL; M — preference margin. Во всех состояниях оценивать одну и ту же frozen TRAIN/EXAM panel с тем же reducer. Показать также actual updates/targets/cost и все ухудшения. `NOT_MEASURED` не заменять нулём. Это таблица ожидаемого отчёта, не новые измеренные данные.

### Поправка к локальной полноте правила успеха

Формальный полный текст useful-axis rule найден в 169/171 исходных files. N63I/N63R содержат конкретные gains, но не полный самостоятельный текст; global index22 правило сохраняет. Два адресных блока приведены в [06](06-consumed-card-rule-addendum.md). Этот overlay не меняет исходный счётчик и не даёт другим 169 файлам semantic PASS.
