# Проверка постановок: где обучение измеряется, а где рецепт ещё неполон

**Базовый полный текстовый аудит — core669477: 154 карточки, 133 inline-промпта, 125 P-семейств, 470 подслучаев. Перед публикацией учтены core827a1 и core6e: 157 карточек и 36 очередных ID. Два соседних planning-проекта и два новых проекта этого пакета увеличивают объединённую очередь до 40; protected папка этим аудитом не изменялась.**

Следующий — **№08 N63-INGRESS-LENGTH**. E-OPT-STEP и LF-03 уже завершены. Устаревшие NEXT на оба опыта отозваны; сохранённые primary результаты имеют приоритет перед заголовком index. [Последняя сверка](10-final-reconciliation.md), [реестр](04-execution-registry.md).

## 1. Что проверено во всех карточках

На 669477 каталог содержит 155 Markdown: 154 карточки плюс index. Проверены Git blob bytes, наличие default-модели, русского отчёта, немедленного STOP/NEXT, ограничение isolated experiment и сохранение старых результатов. Из 125 P-файлов извлечены 133 отдельных inline-промпта: P86 содержит 9, остальные по 1. Для всех 133 найдены правила any-useful-metric success, exact original source/learning connection, запрет runtime в документной фазе и запрет выдумывать отсутствующий host. [154 строки и 470 подслучаев](evidence/all-154-card-readiness.md), [структурные проверки карточек](evidence/card-rule-presence.json), [133 промпта с source lines](evidence/p-prompt-rule-presence.json).

Это проверка формулировок и адресная смысловая проверка выбранных цепочек. Она **не подтверждает семантически все 470 подслучаев, не проверяет компиляцию и не утверждает наличие 470 готовых hosts**. Механический поиск точных слов имеет false negatives: например,  «без удаления» эквивалентно «не удалять». Такие несовпадения вручную проверены; они не заявлены как новые нарушения. Также не выдаётся роль REFERENCE_FAMILY за READY_TO_RUN.

В core827 добавлены только 3 карточки ROPE/AST/BPE-REALIZED, ранее присутствовавшие в planning; удалений 0. Значение 157 относится к свежей инвентаризации, а 154/133 — к базовому полному текстовому проходу. Изменённые 33 existingcards и 3 newcards проверены адресно в свежем delta; повторный полный смысловой аудит всего каталога не заявляется. [Delta](09-core-827-delta.md).

На core6e изменены 27 файлов карточек, добавлено 0 и удалено 0; 29 остающихся inherited output roots/entry совпали с 827headers. Это ограниченный delta, не новый полный смысловой аудит 470 подслучаев. [Квитанция](evidence/core6e-delta.json).

## 2. Остались ли все найденные детали в protected experiments

| Группа | Покрытие | Что делать исполнителю |
|---|---|---|
| Существующие 33 core669ID и 3 planning-only | На 827 все 36 уже есть в coreindex; повторно добавлять NG/ROPE/AST/BPE не нужно | Использовать единственный effective registry и current output mapping |
| Исторические neural/kNN/loader/restore темы | Имеют P-семейства и сохранённые evidence; P125/ingress/cold действительно содержат learned-state цепочку | Не повторять успешный опыт ради нового отчёта; различать memory и neural |
| Новая потеря target апострофа VT | Отдельного такого фактора в 832 planningMD, 154 baselinecards и 3 добавленных corecards не найдено; точный source дефект достижим | Новый малый blueprint только в possibleDifs; 2 native Train/arm, 8 EXAM; NOT_RUN |
| Новые координатные копии last-axis Mean RMS | Ранее RMS-COORD менял MultiplyBroadcast и исключал Mean; новая reduction не покрыта прежним фактором | Отдельный blueprint только в possibleDifs; 4 nativeupdates/arm; NOT_RUN |
| P54.1–P54.7 | Учебная релевантность есть, но конкретныеобученный артефакт/supplier/hostcontracts неполны | Не назначать generic executor, не искать другую программу; автор закрывает перечисленные ниже входы |

Следовательно, ответ на вопрос «все ли детали уже оформлены корректными экспериментами в защищённой папке» — **нет**. Отсутствующие два фактора оформлены здесь без нарушения запрета на запись в protected; остальные конкретные пробелы постановки не скрыты общим PASS.

## 3. Оставшиеся конфликты после нового результата

1. `00-execution-index.md` на 827 назначает LF-03, а неизменённый `24-neural-learning-signal-protocol.md` и остаточные абзацы N62 ещё назначают E-OPT-STEP. После позднего core6e LF-03 тоже completed: действуют NEXT08 N63-INGRESS-LENGTH и 12 будущих ID inherited протокола. Новые E-OPT/LF-03 wrappers/readouts не выполнять.
2. поля factor/measured_path результата E-OPT ошибочно наследуют R1. Bridge source показывает hardcoded подписи, исходник B-trainer — actual optimizer-step factor. Исправление этого документа не меняет raw, не заменяет measured gain выдуманным и не назначает новый run.
3. R2/EOPT source-manifests имеют placeholder B hash. Поздний text diff не доказывает привязку исторического бинарника. Это ограничение рядом с кандидатом, а не оправдание удалить его или переучить.
4. Current index предлагает писать полезный результат в protected `required-changes.md`, но пользователь прямо запретил эту папку в настоящей работе. Effective место — один candidate в назначенном разрешённом R; production и protected не меняются.
5. Старый planning картой выходных путей расходится с новым core для AST/NG/BPE-REALIZED. [05](05-output-contract.md) выбирает current827 назначение и явно снимает прежнее future назначение; старые файлы не перемещаются/не удаляются.
6. Старые замечания KH-COST о MethodKnn/FileActionKnn и переходе 23→24 уже исправлены текущим FileActionKnn recipe. Их не выдаём повторно за новую ошибку; выполненность host от этого не появляется.

## 4. Семь конкретных пробелов P54

Ниже сохранены exact строки защищённого исходного файла базового среза. Имя файла — `P54-phrase-composition.md`; core827 не менял его, core6e поменял только строку 114 NEXT06→NEXT07. Все 7 recipe gaps остались; [проверка latest bytes](evidence/current-p54-delta.md). Общий раздел на строке 21 сам говорит, что host/project/commands ещё не назначены. Это допустимые будущие зависимые читатели состояния, но не готовые новые задания на запуск.

| Подслучай | Exact protected source lines | Чего не хватает до поручения executor | Additive исправление автора в possibleDifs |
|---|---|---|---|
| P54.1: выбор сегментации | `P54-phrase-composition.md:37–45` | Подтверждённый learned checkpoint с пересекающимися close-user/user-session maps, контрольными word mappings и запросами без exact entry | Назвать сохранённый artifact и его учебное происхождение; закрепить literal ограниченную панель до 8 tokens, один bounded composer factor, native host/command/число readouts. Не выдавать придуманный dictionary за historical learned state. |
| P54.2: original token index | `:47–55` | Реальные token sequences этого tokenizer, где удаление empty-normalized token сдвигает индекс; learned fallback entries и отсутствие exact interception | Автор должен показать входные bytes и ожидаемый native trace originalIndex→lookup до обучения/после чтения состояния; изменить только перенос originalIndex, не punctuation/thresholds. |
| P54.3: обязательное слово в phrase composer | `:57–65` | Источник mandatory mask, независимый от TEST gold, и learned partial phrase coverage | Указать конкретный runtime supplier обязательности; если mask controlled-input, так и назвать. Проверить конечный Execute/fallback, а не считать промежуточный false полноценным отказом. |
| P54.4: исчезновение границы | `:67–75` | Learned phrase и actual tokenizer output данного разделителя, плюс документированный смысл boundary | Закрепить domain contract для arrow/другого separator и две literal contrasting строки; сохранить mapping/index behaviour, не объединять с P54.2. |
| P54.5: неполное word-map покрытие | `:77–85` | Learned word entries, отсутствие exact/phrase interception, independent mandatory mask и downstream fallback expectation | Назначить конкретный trained state, native query/reader trace; изменить только word-map return при omission. Показать wrong-accepted и answered/all вместе. |
| P54.6: semantic rerank | `:87–95` | Одна закреплённая общая composer version/candidate list, state и разрешённый action-object supplier | Freeze общий composer для A/B заранее, один rerank factor; source-defined relation отдельно от gold. Не соединять генерацию альтернатив из P54.1 с выбором P54.6 в одном опыте. |
| P54.7: word-map закрывает kNN | `:97–105` | Один checkpoint/query с достижимыми word-map и kNN кандидатами, checker relation/polarity contract, eligibility trace | Зафиксировать обе learned записи и ordinary Execute path; только CONTRADICTION пропускает на прежний kNN. UNKNOWN/COMPATIBLE сохраняют старый return; не менять k или stored pairs ради выигрыша. |

Общий P54: 21 явно говорит, что host/project/commands пока не назначены. Поэтому эти семь **учебно релевантны**, но не входят в текущую runnable queue. Нельзя устранять пробел запуском посторонней программы или обучением нового набора «для галочки». Такие reader эксперименты допустимы на одном доказанно обученном checkpoint без новых Train, когда автор закроет названные contract/artifact dependencies. Утверждение «все детали уже описаны как полностью готовые эксперименты» по нынешнему corpus было бы неверным.


## 5. Что делает before/after валидным

Измерение должно замыкать **input → реальная учебная операция → наблюдаемое обучаемое состояние до/после → обычный потребитель результата**. Для loader/restore допустим уже обученный неизменяемый checkpoint, когда показаны его writer и реальный потребитель; произвольный DTO round-trip не заменяет этот путь.

A0/A1 и B0/B1 используют один и тот же заранее закреплённый TRAIN/EXAM и независимый gold. Для новых neural рецептов одинаковые TRAIN prompts читаются до/после, NLL/rank/teacher-forced answer считаются отдельно от свободного ответа; diagnostic forwards явно включены в бюджет и исключены из стоимости TRAIN. Два online losses разных уроков не заменяют такую панель. Для обучаемого Unit памяти до/после показываются реальные записи и обычные ответы; отсутствие optimizer не делает их неучебными.

Точность, false accept, false reject, answered/all, retention и ресурсы имеют свои знаменатели. Allocated bytes, retained heap, process peak и time — разные величины. Граница стоимости должна охватывать настоящий Train; например, таймер E25 начинается после Train, поэтому он не доказывает дешевизну обучения. Все эти ограничения применены в [кандидатах](02-measured-candidates.md) и [исторических результатах](08-historical-learning.md).

Успех — любое измеренное полезное улучшение своей оси и темы, даже при неизменной accuracy. Рядом остаются ухудшения. Неизмеренное — NOT_MEASURED, failure до Train — техническая ошибка, а не accuracy0. Нельзя дописывать отсутствующие счётчики из ожидаемого бюджета или проводить новый run ради отчёта.

## 6. Новые карточки прошли документную проверку

VT и RMS имеют точно закреплённые версии исходников, единственный patch, перечисленные native files, буквально заданные inputs/gold, фиксированные параметры модели и модель текущей сессии по умолчанию, budgets, host/output/entry, русские report/candidate и skills. Guard существующего attempt расположен до Train, автоматический выбор другого attempt запрещён. Простой изолированный host/DTO/throw допустим; подмена learner и изменения продукта не разрешены. Независимая проверка проверяет рецепт чтением, **не объявляет сборку и runtime готовыми**. [Материалы проверки](07-evidence-and-review.md).

**STOP/NEXT в каждой новой карточке и каждом промпте:** прекратить прежнюю работу немедленно, включая report/hash/review/retry/Git. Не продолжать прошлый опыт под предлогом оформления. После полного STOP прекратить всё; допустимо только необходимое прекращение собственного ещё работающего процесса. Старые эксперименты и результаты сохраняются. Очередь не является разрешением выполнить её целиком в этой документной сессии.
