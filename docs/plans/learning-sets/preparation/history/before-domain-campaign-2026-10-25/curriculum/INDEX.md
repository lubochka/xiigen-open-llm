# Индекс узких заданий для Kiro

**75 микроцели, 165 потоков, 557 публичных reference-строк**. Они входят в общий пакет 148 карточек / 192 потоков. История и старые опоры сохранены.

[Общий промпт](../prompts/MASTER-PROMPT-RU.md) · [Полная очередь](../EXECUTION-QUEUE.md) · [Точные команды](pilot-queue.json) · [Доли бюджета](scale-plan.json). Первый batch включён в TRAIN-квоту. Подготовка, генерация, предметный oracle и обучение имеют разные статусы.

## Карточки

| ID | Родитель | Узкий навык | Профилей | Reference |
|---|---|---|---:|---:|
| [TR07-OMISSION](cards/TR07-OMISSION.md) | TR07 | Ровно одна пропущенная буква известного слова | 6 | 6 |
| [DOC-CS-NULL](cards/DOC-CS-NULL.md) | DOC-CS-01 | Документировать подстановку только вместо null | 1 | 4 |
| [CODE-CS-NULL](cards/CODE-CS-NULL.md) | CODE-CS-01 | Реализовать подстановку только вместо null | 1 | 4 |
| [OR-ROUTE-INTENT](cards/OR-ROUTE-INTENT.md) | OR-ROUTE-01 | Маршрут по действию при явном закрытом каталоге | 1 | 25 |
| [TR07-TRANSPOSE](cards/TR07-TRANSPOSE.md) | TR07 | Перестановка двух соседних букв одного слова | 6 | 6 |
| [TR07-VALIDWORD](cards/TR07-VALIDWORD.md) | TR07 | Корректное похожее слово нельзя исправлять в другое | 6 | 6 |
| [TR08-LAYOUT](cards/TR08-LAYOUT.md) | TR08 | Одно слово в известной неверной раскладке | 6 | 6 |
| [TR10-CONDITIONAL](cards/TR10-CONDITIONAL.md) | TR10 | Уточнить только тогда, когда контекста действительно недостаточно | 6 | 12 |
| [DOC-SQL-COUNT](cards/DOC-SQL-COUNT.md) | DOC-SQL-01 | Документировать COUNT по nullable столбцу | 1 | 4 |
| [CODE-SQL-COUNT](cards/CODE-SQL-COUNT.md) | CODE-SQL-01 | Написать COUNT по nullable столбцу | 1 | 4 |
| [DOC-NG-DISABLED](cards/DOC-NG-DISABLED.md) | DOC-NG-01 | Документировать boolean disabled binding | 1 | 4 |
| [CODE-NG-DISABLED](cards/CODE-NG-DISABLED.md) | CODE-NG-01 | Написать boolean disabled binding | 1 | 4 |
| [OR-AUTO-TERMINAL](cards/OR-AUTO-TERMINAL.md) | OR-AUTO-01 | Следующее действие по состоянию одного длительного запуска | 1 | 4 |
| [OR-CODE-PROMPT-CONTEXT](cards/OR-CODE-PROMPT-CONTEXT.md) | OR-CODE-01 | Готовность передачи задачи исполнителю по фактически отправляемому контексту | 1 | 4 |
| [OR-ARCH-FIFO](cards/OR-ARCH-FIFO.md) | OR-ARCH-01 | Назначение гарантии порядка в компоненте очереди | 1 | 4 |
| [OR-CLIENT-RUN-STATE](cards/OR-CLIENT-RUN-STATE.md) | OR-CLIENT-01 | Переход UI по подтверждению своего запуска | 1 | 4 |
| [OR-DESIGN-ROLE-PANEL](cards/OR-DESIGN-ROLE-PANEL.md) | OR-DESIGN-01 | Одна панель по роли, объекту и области доступа | 1 | 4 |
| [OR-UX-RECOVERY](cards/OR-UX-RECOVERY.md) | OR-UX-01 | Одно действие восстановления по известной причине | 1 | 4 |
| [OR-QA-CAUSAL](cards/OR-QA-CAUSAL.md) | OR-QA-01 | Один совместимый parent для сравнения учебного изменения | 1 | 4 |
| [TR01-NEGATION-SCOPE](cards/TR01-NEGATION-SCOPE.md) | TR01 | Область действия отрицания в короткой фразе | 6 | 12 |
| [TR02-ROLE-ORDER](cards/TR02-ROLE-ORDER.md) | TR02 | Смысловые роли при изменении порядка слов | 6 | 12 |
| [TR03-COMPLETION](cards/TR03-COMPLETION.md) | TR03 | Завершённое действие и ещё продолжающийся процесс | 6 | 12 |
| [TR04-EXPLICIT-GENDER](cards/TR04-EXPLICIT-GENDER.md) | TR04 | Род участника, явно указанный в тексте | 6 | 12 |
| [TR05-CONTEXT-SENSE](cards/TR05-CONTEXT-SENSE.md) | TR05 | Значение многозначного слова при достаточном контексте | 6 | 12 |
| [TR06-PLACEHOLDER](cards/TR06-PLACEHOLDER.md) | TR06 | Точное сохранение имени заполнителя в переводе | 6 | 12 |
| [TR09-IDENTIFIER](cards/TR09-IDENTIFIER.md) | TR09 | Сохранение точного программного идентификатора | 6 | 12 |
| [DOC-CS-CANCEL](cards/DOC-CS-CANCEL.md) | DOC-CS-02 | Документировать точную передачу CancellationToken | 1 | 4 |
| [CODE-CS-CANCEL](cards/CODE-CS-CANCEL.md) | CODE-CS-02 | Реализовать точную передачу CancellationToken | 1 | 4 |
| [DOC-CS-AWAIT](cards/DOC-CS-AWAIT.md) | DOC-CS-02 | Документировать побочный эффект после await | 1 | 4 |
| [CODE-CS-AWAIT](cards/CODE-CS-AWAIT.md) | CODE-CS-02 | Реализовать побочный эффект после await | 1 | 4 |
| [DOC-SQL-LEFT](cards/DOC-SQL-LEFT.md) | DOC-SQL-02 | Документировать LEFT JOIN с фильтром правой стороны | 1 | 4 |
| [CODE-SQL-LEFT](cards/CODE-SQL-LEFT.md) | CODE-SQL-02 | Написать LEFT JOIN с фильтром правой стороны | 1 | 4 |
| [DOC-SQL-GUARD](cards/DOC-SQL-GUARD.md) | DOC-SQL-03 | Документировать точные границы защищённого UPDATE | 1 | 4 |
| [CODE-SQL-GUARD](cards/CODE-SQL-GUARD.md) | CODE-SQL-02 | Написать точные границы защищённого UPDATE | 1 | 4 |
| [DOC-NG-LATEST](cards/DOC-NG-LATEST.md) | DOC-NG-02 | Документировать актуальность последнего Observable | 1 | 4 |
| [CODE-NG-LATEST](cards/CODE-NG-LATEST.md) | CODE-NG-02 | Реализовать актуальность последнего Observable | 1 | 4 |
| [DOC-NG-RECOVERY](cards/DOC-NG-RECOVERY.md) | DOC-NG-02 | Документировать восстановление одного запроса без завершения потока | 1 | 4 |
| [CODE-NG-RECOVERY](cards/CODE-NG-RECOVERY.md) | CODE-NG-02 | Реализовать восстановление одного запроса без завершения потока | 1 | 4 |
| [OR-FUNC-ACCEPTANCE](cards/OR-FUNC-ACCEPTANCE.md) | OR-FUNC-01 | Наблюдаемый критерий без изменения требования | 1 | 6 |
| [OR-DB-ATOMIC-CLAIM](cards/OR-DB-ATOMIC-CLAIM.md) | OR-DB-01 | Следующий шаг по доказанному владению операцией | 1 | 6 |
| [OR-SERVER-OUTBOX](cards/OR-SERVER-OUTBOX.md) | OR-SERVER-01 | Серверный переход между сохранением и доставкой события | 1 | 6 |
| [OR-MOBILE-ACK](cards/OR-MOBILE-ACK.md) | OR-MOBILE-01 | Закрытие локальной операции только по подходящему подтверждению | 1 | 6 |
| [OR-AUTO-FANIN](cards/OR-AUTO-FANIN.md) | OR-AUTO-01 | Сбор группы только по её составу и политике завершения | 1 | 6 |
| [TR02-COREFERENCE](cards/TR02-COREFERENCE.md) | TR02 | Референт местоимения в соседнем предложении | 6 | 24 |
| [TR05-MODALITY](cards/TR05-MODALITY.md) | TR05 | Разрешение и физическая способность слова «может» | 6 | 24 |
| [OR-ARCH-DECOMPOSE](cards/OR-ARCH-DECOMPOSE.md) | OR-ARCH-01 | Полезная декомпозиция одного узла с сохранением обязательств | 1 | 8 |
| [OR-QA-REVIEWER-COVERAGE](cards/OR-QA-REVIEWER-COVERAGE.md) | OR-QA-03 | Выбор недостающей предметной проверки для одного учебного шага | 1 | 8 |
| [DOC-SQL-SAVEPOINT](cards/DOC-SQL-SAVEPOINT.md) | DOC-SQL-03 | Документировать границу savepoint и состояние транзакции | 1 | 6 |
| [CODE-SQL-SAVEPOINT](cards/CODE-SQL-SAVEPOINT.md) | CODE-SQL-02 | Исправить один оператор отката по заданной границе | 1 | 6 |
| [TR08-RESTORE-OR-PRESERVE](cards/TR08-RESTORE-OR-PRESERVE.md) | TR08 | Восстановить неверную раскладку или сохранить буквальный идентификатор по видимому контексту | 6 | 12 |
| [DOC-CS-SNAPSHOT](cards/DOC-CS-SNAPSHOT.md) | DOC-CS-04 | Документировать точную границу снимка и общих ссылок | 1 | 6 |
| [CODE-CS-SNAPSHOT](cards/CODE-CS-SNAPSHOT.md) | CODE-CS-01 | Реализовать заданную границу фиксации данных | 1 | 6 |
| [DOC-NG-PAYLOAD](cards/DOC-NG-PAYLOAD.md) | DOC-NG-04 | Документировать runtime-состав Angular form payload | 1 | 6 |
| [CODE-NG-PAYLOAD](cards/CODE-NG-PAYLOAD.md) | CODE-NG-04 | Построить точный runtime-payload формы | 1 | 6 |
| [DOC-CS-FINALLY](cards/DOC-CS-FINALLY.md) | DOC-CS-05 | Описать возврат и наблюдаемые эффекты finally | 1 | 8 |
| [CODE-CS-STABLE-KEY](cards/CODE-CS-STABLE-KEY.md) | CODE-CS-03 | Удалить повторы по полному ключу с заданным порядком | 1 | 6 |
| [DOC-SQL-FILTER-STAGE](cards/DOC-SQL-FILTER-STAGE.md) | DOC-SQL-05 | Описать состав строк до группировки и отбор готовых групп | 1 | 6 |
| [CODE-SQL-ANTI-KEY](cards/CODE-SQL-ANTI-KEY.md) | CODE-SQL-04 | Построить антисоединение с точной политикой ключа и NULL | 1 | 6 |
| [DOC-NG-ROUTE-BOUNDARY](cards/DOC-NG-ROUTE-BOUNDARY.md) | DOC-NG-05 | Описать путь, экспорт и границу route/component loader | 1 | 6 |
| [CODE-NG-IMMUTABLE](cards/CODE-NG-IMMUTABLE.md) | CODE-NG-03 | Обновить нужную ветвь состояния без мутации соседних объектов | 1 | 6 |
| [OR-AUTO-TIME-WINDOW](cards/OR-AUTO-TIME-WINDOW.md) | OR-AUTO-05 | Ограничить автоматическую агрегацию заданным временным окном | 1 | 8 |
| [OR-AUTO-RESULT-COVERAGE](cards/OR-AUTO-RESULT-COVERAGE.md) | OR-AUTO-06 | Не завершать агрегацию по усечённой выдаче | 1 | 8 |
| [TR07-REPEAT](cards/TR07-REPEAT.md) | TR07 | Ровно одна лишняя повторённая буква внутри слова | 6 | 12 |
| [TR06-NUMBER-UNIT](cards/TR06-NUMBER-UNIT.md) | TR06 | Сохранение точной пары «число — единица» при переводе | 6 | 12 |
| [TR03-EVENT-ORDER](cards/TR03-EVENT-ORDER.md) | TR03 | До и после: порядок двух завершённых событий | 6 | 12 |
| [OR-AUTO-EVENT-GATE](cards/OR-AUTO-EVENT-GATE.md) | OR-AUTO-02 | Следующий шаг по точному событийному условию | 1 | 8 |
| [OR-DB-CAPACITY-ATOMIC](cards/OR-DB-CAPACITY-ATOMIC.md) | OR-DB-01 | Выбор плана сохранения ограниченного ресурса | 1 | 8 |
| [OR-ARCH-EVENT-CONTRACT](cards/OR-ARCH-EVENT-CONTRACT.md) | OR-ARCH-02 | Передача канонического имени события через границу компонентов | 1 | 8 |
| [OR-QA-SCORE-PRESENCE](cards/OR-QA-SCORE-PRESENCE.md) | OR-QA-01 | Сохранение наблюдённой оценки без подмены отсутствия нулём | 1 | 8 |
| [DOC-CS-PIPELINE](cards/DOC-CS-PIPELINE.md) | DOC-CS-03 | Описать next, короткое завершение и порядок эффектов конвейера | 1 | 8 |
| [CODE-CS-ROUND-STAGE](cards/CODE-CS-ROUND-STAGE.md) | CODE-CS-04 | Реализовать точный момент округления decimal | 1 | 8 |
| [DOC-SQL-WINDOW-FRAME](cards/DOC-SQL-WINDOW-FRAME.md) | DOC-SQL-04 | Описать строки, partition и границы оконного агрегата | 1 | 8 |
| [CODE-SQL-RANK-TIE](cards/CODE-SQL-RANK-TIE.md) | CODE-SQL-03 | Выбрать точную строку группы и сохранить её payload | 1 | 8 |
| [DOC-NG-CONFIG-READY](cards/DOC-NG-CONFIG-READY.md) | DOC-NG-03 | Описать фактическую готовность runtime config | 1 | 8 |
| [CODE-NG-CONFIG-READY](cards/CODE-NG-CONFIG-READY.md) | CODE-NG-05 | Реализовать ожидание runtime config по видимому контракту | 1 | 8 |

## Переходы полной очереди для этого подмножества

| Общий порядок | Поток | Следующий поток |
|---:|---|---|
| 1 | `TR07-OMISSION/ru-en` | `DOC-CS-NULL/base` |
| 2 | `DOC-CS-NULL/base` | `CODE-CS-NULL/base` |
| 3 | `CODE-CS-NULL/base` | `OR-ROUTE-INTENT/base` |
| 4 | `OR-ROUTE-INTENT/base` | `TR07-OMISSION/en-ru` |
| 5 | `TR07-OMISSION/en-ru` | `TR07-OMISSION/ru-he` |
| 6 | `TR07-OMISSION/ru-he` | `TR07-OMISSION/he-ru` |
| 7 | `TR07-OMISSION/he-ru` | `TR07-OMISSION/en-he` |
| 8 | `TR07-OMISSION/en-he` | `TR07-OMISSION/he-en` |
| 9 | `TR07-OMISSION/he-en` | `TR07-TRANSPOSE/ru-en` |
| 10 | `TR07-TRANSPOSE/ru-en` | `TR07-TRANSPOSE/en-ru` |
| 11 | `TR07-TRANSPOSE/en-ru` | `TR07-TRANSPOSE/ru-he` |
| 12 | `TR07-TRANSPOSE/ru-he` | `TR07-TRANSPOSE/he-ru` |
| 13 | `TR07-TRANSPOSE/he-ru` | `TR07-TRANSPOSE/en-he` |
| 14 | `TR07-TRANSPOSE/en-he` | `TR07-TRANSPOSE/he-en` |
| 15 | `TR07-TRANSPOSE/he-en` | `TR07-VALIDWORD/ru-en` |
| 16 | `TR07-VALIDWORD/ru-en` | `TR07-VALIDWORD/en-ru` |
| 17 | `TR07-VALIDWORD/en-ru` | `TR07-VALIDWORD/ru-he` |
| 18 | `TR07-VALIDWORD/ru-he` | `TR07-VALIDWORD/he-ru` |
| 19 | `TR07-VALIDWORD/he-ru` | `TR07-VALIDWORD/en-he` |
| 20 | `TR07-VALIDWORD/en-he` | `TR07-VALIDWORD/he-en` |
| 21 | `TR07-VALIDWORD/he-en` | `TR08-LAYOUT/ru-en` |
| 22 | `TR08-LAYOUT/ru-en` | `TR08-LAYOUT/en-ru` |
| 23 | `TR08-LAYOUT/en-ru` | `TR08-LAYOUT/ru-he` |
| 24 | `TR08-LAYOUT/ru-he` | `TR08-LAYOUT/he-ru` |
| 25 | `TR08-LAYOUT/he-ru` | `TR08-LAYOUT/en-he` |
| 26 | `TR08-LAYOUT/en-he` | `TR08-LAYOUT/he-en` |
| 27 | `TR08-LAYOUT/he-en` | `TR10-CONDITIONAL/ru-en` |
| 28 | `TR10-CONDITIONAL/ru-en` | `TR10-CONDITIONAL/en-ru` |
| 29 | `TR10-CONDITIONAL/en-ru` | `TR10-CONDITIONAL/ru-he` |
| 30 | `TR10-CONDITIONAL/ru-he` | `TR10-CONDITIONAL/he-ru` |
| 31 | `TR10-CONDITIONAL/he-ru` | `TR10-CONDITIONAL/en-he` |
| 32 | `TR10-CONDITIONAL/en-he` | `TR10-CONDITIONAL/he-en` |
| 33 | `TR10-CONDITIONAL/he-en` | `DOC-SQL-COUNT/base` |
| 34 | `DOC-SQL-COUNT/base` | `CODE-SQL-COUNT/base` |
| 35 | `CODE-SQL-COUNT/base` | `DOC-NG-DISABLED/base` |
| 36 | `DOC-NG-DISABLED/base` | `CODE-NG-DISABLED/base` |
| 37 | `CODE-NG-DISABLED/base` | `OR-AUTO-TERMINAL/base` |
| 38 | `OR-AUTO-TERMINAL/base` | `OR-CODE-PROMPT-CONTEXT/base` |
| 39 | `OR-CODE-PROMPT-CONTEXT/base` | `OR-ARCH-FIFO/base` |
| 40 | `OR-ARCH-FIFO/base` | `OR-CLIENT-RUN-STATE/base` |
| 41 | `OR-CLIENT-RUN-STATE/base` | `OR-DESIGN-ROLE-PANEL/base` |
| 42 | `OR-DESIGN-ROLE-PANEL/base` | `OR-UX-RECOVERY/base` |
| 43 | `OR-UX-RECOVERY/base` | `OR-QA-CAUSAL/base` |
| 44 | `OR-QA-CAUSAL/base` | `TR01-NEGATION-SCOPE/ru-en` |
| 45 | `TR01-NEGATION-SCOPE/ru-en` | `TR01-NEGATION-SCOPE/en-ru` |
| 46 | `TR01-NEGATION-SCOPE/en-ru` | `TR01-NEGATION-SCOPE/ru-he` |
| 47 | `TR01-NEGATION-SCOPE/ru-he` | `TR01-NEGATION-SCOPE/he-ru` |
| 48 | `TR01-NEGATION-SCOPE/he-ru` | `TR01-NEGATION-SCOPE/en-he` |
| 49 | `TR01-NEGATION-SCOPE/en-he` | `TR01-NEGATION-SCOPE/he-en` |
| 50 | `TR01-NEGATION-SCOPE/he-en` | `TR02-ROLE-ORDER/ru-en` |
| 51 | `TR02-ROLE-ORDER/ru-en` | `TR02-ROLE-ORDER/en-ru` |
| 52 | `TR02-ROLE-ORDER/en-ru` | `TR02-ROLE-ORDER/ru-he` |
| 53 | `TR02-ROLE-ORDER/ru-he` | `TR02-ROLE-ORDER/he-ru` |
| 54 | `TR02-ROLE-ORDER/he-ru` | `TR02-ROLE-ORDER/en-he` |
| 55 | `TR02-ROLE-ORDER/en-he` | `TR02-ROLE-ORDER/he-en` |
| 56 | `TR02-ROLE-ORDER/he-en` | `TR03-COMPLETION/ru-en` |
| 57 | `TR03-COMPLETION/ru-en` | `TR03-COMPLETION/en-ru` |
| 58 | `TR03-COMPLETION/en-ru` | `TR03-COMPLETION/ru-he` |
| 59 | `TR03-COMPLETION/ru-he` | `TR03-COMPLETION/he-ru` |
| 60 | `TR03-COMPLETION/he-ru` | `TR03-COMPLETION/en-he` |
| 61 | `TR03-COMPLETION/en-he` | `TR03-COMPLETION/he-en` |
| 62 | `TR03-COMPLETION/he-en` | `TR04-EXPLICIT-GENDER/ru-en` |
| 63 | `TR04-EXPLICIT-GENDER/ru-en` | `TR04-EXPLICIT-GENDER/en-ru` |
| 64 | `TR04-EXPLICIT-GENDER/en-ru` | `TR04-EXPLICIT-GENDER/ru-he` |
| 65 | `TR04-EXPLICIT-GENDER/ru-he` | `TR04-EXPLICIT-GENDER/he-ru` |
| 66 | `TR04-EXPLICIT-GENDER/he-ru` | `TR04-EXPLICIT-GENDER/en-he` |
| 67 | `TR04-EXPLICIT-GENDER/en-he` | `TR04-EXPLICIT-GENDER/he-en` |
| 68 | `TR04-EXPLICIT-GENDER/he-en` | `TR05-CONTEXT-SENSE/ru-en` |
| 69 | `TR05-CONTEXT-SENSE/ru-en` | `TR05-CONTEXT-SENSE/en-ru` |
| 70 | `TR05-CONTEXT-SENSE/en-ru` | `TR05-CONTEXT-SENSE/ru-he` |
| 71 | `TR05-CONTEXT-SENSE/ru-he` | `TR05-CONTEXT-SENSE/he-ru` |
| 72 | `TR05-CONTEXT-SENSE/he-ru` | `TR05-CONTEXT-SENSE/en-he` |
| 73 | `TR05-CONTEXT-SENSE/en-he` | `TR05-CONTEXT-SENSE/he-en` |
| 74 | `TR05-CONTEXT-SENSE/he-en` | `TR06-PLACEHOLDER/ru-en` |
| 75 | `TR06-PLACEHOLDER/ru-en` | `TR06-PLACEHOLDER/en-ru` |
| 76 | `TR06-PLACEHOLDER/en-ru` | `TR06-PLACEHOLDER/ru-he` |
| 77 | `TR06-PLACEHOLDER/ru-he` | `TR06-PLACEHOLDER/he-ru` |
| 78 | `TR06-PLACEHOLDER/he-ru` | `TR06-PLACEHOLDER/en-he` |
| 79 | `TR06-PLACEHOLDER/en-he` | `TR06-PLACEHOLDER/he-en` |
| 80 | `TR06-PLACEHOLDER/he-en` | `TR09-IDENTIFIER/ru-en` |
| 81 | `TR09-IDENTIFIER/ru-en` | `TR09-IDENTIFIER/en-ru` |
| 82 | `TR09-IDENTIFIER/en-ru` | `TR09-IDENTIFIER/ru-he` |
| 83 | `TR09-IDENTIFIER/ru-he` | `TR09-IDENTIFIER/he-ru` |
| 84 | `TR09-IDENTIFIER/he-ru` | `TR09-IDENTIFIER/en-he` |
| 85 | `TR09-IDENTIFIER/en-he` | `TR09-IDENTIFIER/he-en` |
| 86 | `TR09-IDENTIFIER/he-en` | `DOC-CS-CANCEL/base` |
| 87 | `DOC-CS-CANCEL/base` | `CODE-CS-CANCEL/base` |
| 88 | `CODE-CS-CANCEL/base` | `DOC-CS-AWAIT/base` |
| 89 | `DOC-CS-AWAIT/base` | `CODE-CS-AWAIT/base` |
| 90 | `CODE-CS-AWAIT/base` | `DOC-SQL-LEFT/base` |
| 91 | `DOC-SQL-LEFT/base` | `CODE-SQL-LEFT/base` |
| 92 | `CODE-SQL-LEFT/base` | `DOC-SQL-GUARD/base` |
| 93 | `DOC-SQL-GUARD/base` | `CODE-SQL-GUARD/base` |
| 94 | `CODE-SQL-GUARD/base` | `DOC-NG-LATEST/base` |
| 95 | `DOC-NG-LATEST/base` | `CODE-NG-LATEST/base` |
| 96 | `CODE-NG-LATEST/base` | `DOC-NG-RECOVERY/base` |
| 97 | `DOC-NG-RECOVERY/base` | `CODE-NG-RECOVERY/base` |
| 98 | `CODE-NG-RECOVERY/base` | `OR-FUNC-ACCEPTANCE/base` |
| 99 | `OR-FUNC-ACCEPTANCE/base` | `OR-DB-ATOMIC-CLAIM/base` |
| 100 | `OR-DB-ATOMIC-CLAIM/base` | `OR-SERVER-OUTBOX/base` |
| 101 | `OR-SERVER-OUTBOX/base` | `OR-MOBILE-ACK/base` |
| 102 | `OR-MOBILE-ACK/base` | `OR-AUTO-FANIN/base` |
| 103 | `OR-AUTO-FANIN/base` | `TR02-COREFERENCE/ru-en` |
| 104 | `TR02-COREFERENCE/ru-en` | `TR02-COREFERENCE/en-ru` |
| 105 | `TR02-COREFERENCE/en-ru` | `TR02-COREFERENCE/ru-he` |
| 106 | `TR02-COREFERENCE/ru-he` | `TR02-COREFERENCE/he-ru` |
| 107 | `TR02-COREFERENCE/he-ru` | `TR02-COREFERENCE/en-he` |
| 108 | `TR02-COREFERENCE/en-he` | `TR02-COREFERENCE/he-en` |
| 109 | `TR02-COREFERENCE/he-en` | `TR05-MODALITY/ru-en` |
| 110 | `TR05-MODALITY/ru-en` | `TR05-MODALITY/en-ru` |
| 111 | `TR05-MODALITY/en-ru` | `TR05-MODALITY/ru-he` |
| 112 | `TR05-MODALITY/ru-he` | `TR05-MODALITY/he-ru` |
| 113 | `TR05-MODALITY/he-ru` | `TR05-MODALITY/en-he` |
| 114 | `TR05-MODALITY/en-he` | `TR05-MODALITY/he-en` |
| 115 | `TR05-MODALITY/he-en` | `OR-ARCH-DECOMPOSE/base` |
| 116 | `OR-ARCH-DECOMPOSE/base` | `OR-QA-REVIEWER-COVERAGE/base` |
| 117 | `OR-QA-REVIEWER-COVERAGE/base` | `DOC-SQL-SAVEPOINT/base` |
| 118 | `DOC-SQL-SAVEPOINT/base` | `CODE-SQL-SAVEPOINT/base` |
| 119 | `CODE-SQL-SAVEPOINT/base` | `TR08-RESTORE-OR-PRESERVE/ru-en` |
| 120 | `TR08-RESTORE-OR-PRESERVE/ru-en` | `TR08-RESTORE-OR-PRESERVE/en-ru` |
| 121 | `TR08-RESTORE-OR-PRESERVE/en-ru` | `TR08-RESTORE-OR-PRESERVE/ru-he` |
| 122 | `TR08-RESTORE-OR-PRESERVE/ru-he` | `TR08-RESTORE-OR-PRESERVE/he-ru` |
| 123 | `TR08-RESTORE-OR-PRESERVE/he-ru` | `TR08-RESTORE-OR-PRESERVE/en-he` |
| 124 | `TR08-RESTORE-OR-PRESERVE/en-he` | `TR08-RESTORE-OR-PRESERVE/he-en` |
| 125 | `TR08-RESTORE-OR-PRESERVE/he-en` | `DOC-CS-SNAPSHOT/base` |
| 126 | `DOC-CS-SNAPSHOT/base` | `CODE-CS-SNAPSHOT/base` |
| 127 | `CODE-CS-SNAPSHOT/base` | `DOC-NG-PAYLOAD/base` |
| 128 | `DOC-NG-PAYLOAD/base` | `CODE-NG-PAYLOAD/base` |
| 129 | `CODE-NG-PAYLOAD/base` | `DOC-CS-FINALLY/base` |
| 130 | `DOC-CS-FINALLY/base` | `CODE-CS-STABLE-KEY/base` |
| 131 | `CODE-CS-STABLE-KEY/base` | `DOC-SQL-FILTER-STAGE/base` |
| 132 | `DOC-SQL-FILTER-STAGE/base` | `CODE-SQL-ANTI-KEY/base` |
| 133 | `CODE-SQL-ANTI-KEY/base` | `DOC-NG-ROUTE-BOUNDARY/base` |
| 134 | `DOC-NG-ROUTE-BOUNDARY/base` | `CODE-NG-IMMUTABLE/base` |
| 135 | `CODE-NG-IMMUTABLE/base` | `OR-AUTO-TIME-WINDOW/base` |
| 136 | `OR-AUTO-TIME-WINDOW/base` | `OR-AUTO-RESULT-COVERAGE/base` |
| 137 | `OR-AUTO-RESULT-COVERAGE/base` | `TR07-REPEAT/ru-en` |
| 138 | `TR07-REPEAT/ru-en` | `TR07-REPEAT/he-en` |
| 139 | `TR07-REPEAT/he-en` | `TR07-REPEAT/en-ru` |
| 140 | `TR07-REPEAT/en-ru` | `TR07-REPEAT/he-ru` |
| 141 | `TR07-REPEAT/he-ru` | `TR07-REPEAT/en-he` |
| 142 | `TR07-REPEAT/en-he` | `TR07-REPEAT/ru-he` |
| 143 | `TR07-REPEAT/ru-he` | `TR06-NUMBER-UNIT/ru-en` |
| 144 | `TR06-NUMBER-UNIT/ru-en` | `TR06-NUMBER-UNIT/he-en` |
| 145 | `TR06-NUMBER-UNIT/he-en` | `TR06-NUMBER-UNIT/en-ru` |
| 146 | `TR06-NUMBER-UNIT/en-ru` | `TR06-NUMBER-UNIT/he-ru` |
| 147 | `TR06-NUMBER-UNIT/he-ru` | `TR06-NUMBER-UNIT/en-he` |
| 148 | `TR06-NUMBER-UNIT/en-he` | `TR06-NUMBER-UNIT/ru-he` |
| 149 | `TR06-NUMBER-UNIT/ru-he` | `TR03-EVENT-ORDER/ru-en` |
| 150 | `TR03-EVENT-ORDER/ru-en` | `TR03-EVENT-ORDER/he-en` |
| 151 | `TR03-EVENT-ORDER/he-en` | `TR03-EVENT-ORDER/en-ru` |
| 152 | `TR03-EVENT-ORDER/en-ru` | `TR03-EVENT-ORDER/he-ru` |
| 153 | `TR03-EVENT-ORDER/he-ru` | `TR03-EVENT-ORDER/en-he` |
| 154 | `TR03-EVENT-ORDER/en-he` | `TR03-EVENT-ORDER/ru-he` |
| 155 | `TR03-EVENT-ORDER/ru-he` | `OR-AUTO-EVENT-GATE/base` |
| 156 | `OR-AUTO-EVENT-GATE/base` | `OR-DB-CAPACITY-ATOMIC/base` |
| 157 | `OR-DB-CAPACITY-ATOMIC/base` | `OR-ARCH-EVENT-CONTRACT/base` |
| 158 | `OR-ARCH-EVENT-CONTRACT/base` | `OR-QA-SCORE-PRESENCE/base` |
| 159 | `OR-QA-SCORE-PRESENCE/base` | `DOC-CS-PIPELINE/base` |
| 160 | `DOC-CS-PIPELINE/base` | `CODE-CS-ROUND-STAGE/base` |
| 161 | `CODE-CS-ROUND-STAGE/base` | `DOC-SQL-WINDOW-FRAME/base` |
| 162 | `DOC-SQL-WINDOW-FRAME/base` | `CODE-SQL-RANK-TIE/base` |
| 163 | `CODE-SQL-RANK-TIE/base` | `DOC-NG-CONFIG-READY/base` |
| 164 | `DOC-NG-CONFIG-READY/base` | `CODE-NG-CONFIG-READY/base` |
| 165 | `CODE-NG-CONFIG-READY/base` | `TR-EN-IF-01/ru-en` |
