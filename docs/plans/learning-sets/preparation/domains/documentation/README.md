# Документация C#/.NET, SQL, Angular: материал для DPO

Каталог: 15 родительских тем и 18 микроцелей. Полный текущий порядок и NEXT — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); прямое указание ID выше очереди. [Общий промпт Kiro](../../14-GENERATION-CAMPAIGN.md) и [контракт формата](../../02-DATA-CONTRACT.md). Подготовка нового дополнения не означает выполненную генерацию, native oracle или обучение.

Описание должно соответствовать показанному коду, включая его ошибочное поведение; исправленный желаемый код не подставляется вместо фактов. Версия .NET/Angular, SQL-диалект, callee/fixture и условия входят в learner-visible prompt.

## Родительские темы

| ID | Тема | Домены |
|---|---|---|
| [DOC-CS-01](DOC-CS-01.md) | Локальная проверка входа и побочные эффекты | docs.csharp |
| [DOC-CS-02](DOC-CS-02.md) | Асинхронный вызов и передача ошибок | docs.csharp |
| [DOC-CS-03](DOC-CS-03.md) | Версия проекта и порядок конвейера | docs.csharp |
| [DOC-SQL-01](DOC-SQL-01.md) | NULL, COUNT и пустые группы | docs.sql |
| [DOC-SQL-02](DOC-SQL-02.md) | JOIN, фильтр и число строк | docs.sql |
| [DOC-SQL-03](DOC-SQL-03.md) | Границы транзакции и защищённое обновление | docs.sql |
| [DOC-NG-01](DOC-NG-01.md) | Input, состояние и отображение | docs.angular |
| [DOC-NG-02](DOC-NG-02.md) | Observable и актуальность ответа | docs.angular |
| [DOC-NG-03](DOC-NG-03.md) | Версия, зависимости и RTL | docs.angular |
| [DOC-CS-04](DOC-CS-04.md) | Ленивое перечисление и границы снимка данных | docs.csharp |
| [DOC-CS-05](DOC-CS-05.md) | Возврат, finally и порядок освобождения ресурса | docs.csharp |
| [DOC-SQL-04](DOC-SQL-04.md) | Оконный агрегат, рамка и устойчивый порядок | docs.sql |
| [DOC-SQL-05](DOC-SQL-05.md) | WHERE и HAVING: отбор строк и отбор групп | docs.sql |
| [DOC-NG-04](DOC-NG-04.md) | Reactive Forms: disabled, reset и фактическое значение | docs.angular |
| [DOC-NG-05](DOC-NG-05.md) | Маршрут и граница ленивой загрузки | docs.angular |

## Узкие задания

| ID | Учебная граница | Reference |
|---|---|---:|
| [DOC-CS-NULL](../../curriculum/cards/DOC-CS-NULL.md) | Документировать подстановку только вместо null | 4 |
| [DOC-SQL-COUNT](../../curriculum/cards/DOC-SQL-COUNT.md) | Документировать COUNT по nullable столбцу | 4 |
| [DOC-NG-DISABLED](../../curriculum/cards/DOC-NG-DISABLED.md) | Документировать boolean disabled binding | 4 |
| [DOC-CS-CANCEL](../../curriculum/cards/DOC-CS-CANCEL.md) | Документировать точную передачу CancellationToken | 4 |
| [DOC-CS-AWAIT](../../curriculum/cards/DOC-CS-AWAIT.md) | Документировать побочный эффект после await | 4 |
| [DOC-SQL-LEFT](../../curriculum/cards/DOC-SQL-LEFT.md) | Документировать LEFT JOIN с фильтром правой стороны | 4 |
| [DOC-SQL-GUARD](../../curriculum/cards/DOC-SQL-GUARD.md) | Документировать точные границы защищённого UPDATE | 4 |
| [DOC-NG-LATEST](../../curriculum/cards/DOC-NG-LATEST.md) | Документировать актуальность последнего Observable | 4 |
| [DOC-NG-RECOVERY](../../curriculum/cards/DOC-NG-RECOVERY.md) | Документировать восстановление одного запроса без завершения потока | 4 |
| [DOC-SQL-SAVEPOINT](../../curriculum/cards/DOC-SQL-SAVEPOINT.md) | Документировать границу savepoint и состояние транзакции | 6 |
| [DOC-CS-SNAPSHOT](../../curriculum/cards/DOC-CS-SNAPSHOT.md) | Документировать точную границу снимка и общих ссылок | 6 |
| [DOC-NG-PAYLOAD](../../curriculum/cards/DOC-NG-PAYLOAD.md) | Документировать runtime-состав Angular form payload | 6 |
| [DOC-CS-FINALLY](../../curriculum/cards/DOC-CS-FINALLY.md) | Описать возврат и наблюдаемые эффекты finally | 8 |
| [DOC-SQL-FILTER-STAGE](../../curriculum/cards/DOC-SQL-FILTER-STAGE.md) | Описать состав строк до группировки и отбор готовых групп | 6 |
| [DOC-NG-ROUTE-BOUNDARY](../../curriculum/cards/DOC-NG-ROUTE-BOUNDARY.md) | Описать путь, экспорт и границу route/component loader | 6 |
| [DOC-CS-PIPELINE](../../curriculum/cards/DOC-CS-PIPELINE.md) | Описать next, короткое завершение и порядок эффектов конвейера | 8 |
| [DOC-SQL-WINDOW-FRAME](../../curriculum/cards/DOC-SQL-WINDOW-FRAME.md) | Описать строки, partition и границы оконного агрегата | 8 |
| [DOC-NG-CONFIG-READY](../../curriculum/cards/DOC-NG-CONFIG-READY.md) | Описать фактическую готовность runtime config | 8 |

Код, документация, перефразирование и перевод одного источника сохраняют общие origin/leakage до split. Публичные reference и их потомки не являются закрытым EXAM. Точные first-run/dataset/B1 пути задаёт [pilot-queue.json](../../curriculum/pilot-queue.json); метрики, восстановление и достаточный ненулевой старт — [05](../../05-LEARNING-AND-CHECKPOINTS.md). Выбор/оценка выполняются CODEX, генерация — китайской моделью Kiro по [10](../../10-CODEX-AND-CHINESE-ROLES.md). NEXT/STOP соблюдаются немедленно, старые версии сохраняются.

