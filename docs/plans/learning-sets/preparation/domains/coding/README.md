# Написание кода: материал для DPO

Каталог: 13 родительских тем и 18 микроцелей. Полный текущий порядок и NEXT — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); прямое указание ID выше очереди. [Общий промпт Kiro](../../14-GENERATION-CAMPAIGN.md) и [контракт формата](../../02-DATA-CONTRACT.md). Подготовка нового дополнения не означает выполненную генерацию, native oracle или обучение.

Требуемое поведение полностью задаётся в prompt: входы, изменения состояния, граничные случаи, версия и формат ответа. Chosen и rejected исполняются на одинаковых входах поздним native oracle; компиляция или форма JSON не доказывают контракт.

## Родительские темы

| ID | Тема | Домены |
|---|---|---|
| [CODE-CS-01](CODE-CS-01.md) | Реализация простого граничного контракта | code.csharp |
| [CODE-CS-02](CODE-CS-02.md) | Async и точная передача отмены | code.csharp |
| [CODE-SQL-01](CODE-SQL-01.md) | Агрегат, NULL и дубликаты | code.sql |
| [CODE-SQL-02](CODE-SQL-02.md) | Защищённое изменение и точное соединение | code.sql |
| [CODE-NG-01](CODE-NG-01.md) | Binding и вывод состояния | code.angular |
| [CODE-NG-02](CODE-NG-02.md) | Смена запроса и сохранение значения | code.angular |
| [CODE-CS-03](CODE-CS-03.md) | Устойчивое удаление дублей по заданному ключу | code.csharp |
| [CODE-CS-04](CODE-CS-04.md) | Decimal: разрядность, midpoint и момент округления | code.csharp |
| [CODE-SQL-03](CODE-SQL-03.md) | Выбор строки в группе с явным разрешением равенств | code.sql |
| [CODE-SQL-04](CODE-SQL-04.md) | Антисоединение: NULL, составной ключ и дубликаты | code.sql |
| [CODE-NG-03](CODE-NG-03.md) | Неизменяемое обновление состояния для OnPush | code.angular |
| [CODE-NG-04](CODE-NG-04.md) | Точная проекция typed Reactive Forms в запрос | code.angular |

## Узкие задания

| ID | Учебная граница | Reference |
|---|---|---:|
| [CODE-CS-NULL](../../curriculum/cards/CODE-CS-NULL.md) | Реализовать подстановку только вместо null | 4 |
| [CODE-SQL-COUNT](../../curriculum/cards/CODE-SQL-COUNT.md) | Написать COUNT по nullable столбцу | 4 |
| [CODE-NG-DISABLED](../../curriculum/cards/CODE-NG-DISABLED.md) | Написать boolean disabled binding | 4 |
| [CODE-CS-CANCEL](../../curriculum/cards/CODE-CS-CANCEL.md) | Реализовать точную передачу CancellationToken | 4 |
| [CODE-CS-AWAIT](../../curriculum/cards/CODE-CS-AWAIT.md) | Реализовать побочный эффект после await | 4 |
| [CODE-SQL-LEFT](../../curriculum/cards/CODE-SQL-LEFT.md) | Написать LEFT JOIN с фильтром правой стороны | 4 |
| [CODE-SQL-GUARD](../../curriculum/cards/CODE-SQL-GUARD.md) | Написать точные границы защищённого UPDATE | 4 |
| [CODE-NG-LATEST](../../curriculum/cards/CODE-NG-LATEST.md) | Реализовать актуальность последнего Observable | 4 |
| [CODE-NG-RECOVERY](../../curriculum/cards/CODE-NG-RECOVERY.md) | Реализовать восстановление одного запроса без завершения потока | 4 |
| [CODE-SQL-SAVEPOINT](../../curriculum/cards/CODE-SQL-SAVEPOINT.md) | Исправить один оператор отката по заданной границе | 6 |
| [CODE-CS-SNAPSHOT](../../curriculum/cards/CODE-CS-SNAPSHOT.md) | Реализовать заданную границу фиксации данных | 6 |
| [CODE-NG-PAYLOAD](../../curriculum/cards/CODE-NG-PAYLOAD.md) | Построить точный runtime-payload формы | 6 |
| [CODE-CS-STABLE-KEY](../../curriculum/cards/CODE-CS-STABLE-KEY.md) | Удалить повторы по полному ключу с заданным порядком | 6 |
| [CODE-SQL-ANTI-KEY](../../curriculum/cards/CODE-SQL-ANTI-KEY.md) | Построить антисоединение с точной политикой ключа и NULL | 6 |
| [CODE-NG-IMMUTABLE](../../curriculum/cards/CODE-NG-IMMUTABLE.md) | Обновить нужную ветвь состояния без мутации соседних объектов | 6 |
| [CODE-CS-ROUND-STAGE](../../curriculum/cards/CODE-CS-ROUND-STAGE.md) | Реализовать точный момент округления decimal | 8 |
| [CODE-SQL-RANK-TIE](../../curriculum/cards/CODE-SQL-RANK-TIE.md) | Выбрать точную строку группы и сохранить её payload | 8 |
| [CODE-NG-CONFIG-READY](../../curriculum/cards/CODE-NG-CONFIG-READY.md) | Реализовать ожидание runtime config по видимому контракту | 8 |

Код, документация, перефразирование и перевод одного источника сохраняют общие origin/leakage до split. Публичные reference и их потомки не являются закрытым EXAM. Точные first-run/dataset/B1 пути задаёт [pilot-queue.json](../../curriculum/pilot-queue.json); метрики, восстановление и достаточный ненулевой старт — [05](../../05-LEARNING-AND-CHECKPOINTS.md). Выбор/оценка выполняются CODEX, генерация — китайской моделью Kiro по [10](../../10-CODEX-AND-CHINESE-ROLES.md). NEXT/STOP соблюдаются немедленно, старые версии сохраняются.

