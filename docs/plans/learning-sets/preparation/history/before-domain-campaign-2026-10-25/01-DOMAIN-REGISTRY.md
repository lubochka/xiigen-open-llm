# Каталог родительских тем и профилей

> Актуальная дорожка новых партий — [17: контракт](17-CURRENT-PREPARATION-CONTRACT.md) и [18: команды](18-BATCH-RUNBOOK.md), `tools/batches.py`, встроенный rag/. Обязательные hashes/profile receipts из прежних описаний не применяются к подготовке; содержательное качество и native-контракт не ослабляются. Ниже сохранена предметная методика и история.

Полный порядок запуска — [EXECUTION-QUEUE.md](EXECUTION-QUEUE.md). Здесь каталог родительских тем; его next_id не заменяет next_stream_id общей очереди. Прямой ID пользователя выше реестров. В покрытой теме используются узкие [objective-карточки](curriculum/INDEX.md); общий parent не запускается дополнительно.

**70 тем, 129 сочетаний topic/profile, общий план 129000 TRAIN-пар.** В полном пакете 119 карточек и 162 потоков. Все 37 исходных parent ID и последующие дополнения сохранены. Генерация и обучение: NOT_RUN для текущего этапа.

Перевод разделён по целевому языку: tr.en, tr.ru, tr.he; шесть направлений задаются профилями. Оркестрация разделена по программным задачам; чужая предметная область в запросе router не превращает его набор в смешанный языковой корпус. Документация и написание кода — отдельные задачи C#/.NET, SQL и Angular.

| Каталоговый порядок | ID | Тема | Домены | Профили | План TRAIN на профиль |
|---:|---|---|---|---|---:|
| 1 | [TR01](domains/translation/TR01.md) | Сохранение отрицания | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 2 | [DOC-CS-01](domains/documentation/DOC-CS-01.md) | Локальная проверка входа и побочные эффекты | docs.csharp | base | 1000 |
| 3 | [CODE-CS-01](domains/coding/CODE-CS-01.md) | Реализация простого граничного контракта | code.csharp | base | 1000 |
| 4 | [OR-ROUTE-01](domains/orchestration/OR-ROUTE-01.md) | Выбор домена по намерению, а не по ключевому слову | or.routing | base | 1000 |
| 5 | [TR02](domains/translation/TR02.md) | Кто совершил действие над кем | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 6 | [TR03](domains/translation/TR03.md) | Время и завершённость действия | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 7 | [TR04](domains/translation/TR04.md) | Число и явно заданный род | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 8 | [TR05](domains/translation/TR05.md) | Значение слова по контексту | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 9 | [TR06](domains/translation/TR06.md) | Точные числа, метки и заполнители | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 10 | [TR07](domains/translation/TR07.md) | Опечатки внутри одной лексической семьи | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 11 | [TR08](domains/translation/TR08.md) | Слово набрано в неверной раскладке | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 12 | [TR09](domains/translation/TR09.md) | Перевод вокруг кода и технических имён | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 13 | [TR10](domains/translation/TR10.md) | Уточнение при неразрешённой неоднозначности | tr.en, tr.ru, tr.he | ru-en, he-en, en-ru, he-ru, en-he, ru-he | 1000 |
| 14 | [OR-FUNC-01](domains/orchestration/OR-FUNC-01.md) | Один проверяемый критерий функционального требования | or.functional | base | 1000 |
| 15 | [OR-ARCH-01](domains/orchestration/OR-ARCH-01.md) | Владелец решения и граница зависимости | or.architecture | base | 1000 |
| 16 | [OR-CODE-01](domains/orchestration/OR-CODE-01.md) | Следующий шаг реализации по готовым зависимостям | or.code | base | 1000 |
| 17 | [OR-DB-01](domains/orchestration/OR-DB-01.md) | Порядок изменения данных при заданных ограничениях | or.database | base | 1000 |
| 18 | [OR-CLIENT-01](domains/orchestration/OR-CLIENT-01.md) | Состояние клиента соответствует ответу сервера | or.client | base | 1000 |
| 19 | [OR-SERVER-01](domains/orchestration/OR-SERVER-01.md) | Обязательное условие перед серверным изменением | or.server | base | 1000 |
| 20 | [OR-MOBILE-01](domains/orchestration/OR-MOBILE-01.md) | Синхронизация офлайн-изменения без потери данных | or.mobile | base | 1000 |
| 21 | [OR-DESIGN-01](domains/orchestration/OR-DESIGN-01.md) | Выбор UI-поверхности по пользователю и состоянию | or.design | base | 1000 |
| 22 | [OR-UX-01](domains/orchestration/OR-UX-01.md) | Понятное следующее действие при затруднении | or.ux | base | 1000 |
| 23 | [OR-QA-01](domains/orchestration/OR-QA-01.md) | Проверка меняющегося учебного поведения | or.qa | base | 1000 |
| 24 | [OR-AUTO-01](domains/orchestration/OR-AUTO-01.md) | Ожидание и продолжение одного запуска | or.automation | base | 1000 |
| 25 | [DOC-CS-02](domains/documentation/DOC-CS-02.md) | Асинхронный вызов и передача ошибок | docs.csharp | base | 1000 |
| 26 | [DOC-CS-03](domains/documentation/DOC-CS-03.md) | Версия проекта и порядок конвейера | docs.csharp | base | 1000 |
| 27 | [DOC-SQL-01](domains/documentation/DOC-SQL-01.md) | NULL, COUNT и пустые группы | docs.sql | base | 1000 |
| 28 | [DOC-SQL-02](domains/documentation/DOC-SQL-02.md) | JOIN, фильтр и число строк | docs.sql | base | 1000 |
| 29 | [DOC-SQL-03](domains/documentation/DOC-SQL-03.md) | Границы транзакции и защищённое обновление | docs.sql | base | 1000 |
| 30 | [DOC-NG-01](domains/documentation/DOC-NG-01.md) | Input, состояние и отображение | docs.angular | base | 1000 |
| 31 | [DOC-NG-02](domains/documentation/DOC-NG-02.md) | Observable и актуальность ответа | docs.angular | base | 1000 |
| 32 | [DOC-NG-03](domains/documentation/DOC-NG-03.md) | Версия, зависимости и RTL | docs.angular | base | 1000 |
| 33 | [CODE-CS-02](domains/coding/CODE-CS-02.md) | Async и точная передача отмены | code.csharp | base | 1000 |
| 34 | [CODE-SQL-01](domains/coding/CODE-SQL-01.md) | Агрегат, NULL и дубликаты | code.sql | base | 1000 |
| 35 | [CODE-SQL-02](domains/coding/CODE-SQL-02.md) | Защищённое изменение и точное соединение | code.sql | base | 1000 |
| 36 | [CODE-NG-01](domains/coding/CODE-NG-01.md) | Binding и вывод состояния | code.angular | base | 1000 |
| 37 | [CODE-NG-02](domains/coding/CODE-NG-02.md) | Смена запроса и сохранение значения | code.angular | base | 1000 |
| 38 | [TR-EN-IF-01](domains/translation/TR-EN-IF-01.md) | Условие действия и направление условной связи — английский | tr.en | ru-en, he-en | 1000 |
| 39 | [TR-EN-SCOPE-01](domains/translation/TR-EN-SCOPE-01.md) | Область действия отрицания, «только» и количества — английский | tr.en | ru-en, he-en | 1000 |
| 40 | [TR-EN-ORDER-01](domains/translation/TR-EN-ORDER-01.md) | Временной порядок событий и одновременный интервал — английский | tr.en | ru-en, he-en | 1000 |
| 41 | [TR-RU-IF-01](domains/translation/TR-RU-IF-01.md) | Условие действия и направление условной связи — русский | tr.ru | en-ru, he-ru | 1000 |
| 42 | [TR-RU-SCOPE-01](domains/translation/TR-RU-SCOPE-01.md) | Область действия отрицания, «только» и количества — русский | tr.ru | en-ru, he-ru | 1000 |
| 43 | [TR-RU-ORDER-01](domains/translation/TR-RU-ORDER-01.md) | Временной порядок событий и одновременный интервал — русский | tr.ru | en-ru, he-ru | 1000 |
| 44 | [TR-HE-IF-01](domains/translation/TR-HE-IF-01.md) | Условие действия и направление условной связи — иврит | tr.he | en-he, ru-he | 1000 |
| 45 | [TR-HE-SCOPE-01](domains/translation/TR-HE-SCOPE-01.md) | Область действия отрицания, «только» и количества — иврит | tr.he | en-he, ru-he | 1000 |
| 46 | [TR-HE-ORDER-01](domains/translation/TR-HE-ORDER-01.md) | Временной порядок событий и одновременный интервал — иврит | tr.he | en-he, ru-he | 1000 |
| 47 | [OR-FUNC-02](domains/orchestration/OR-FUNC-02.md) | Настраиваемое значение и обязательный инвариант | or.functional | base | 1000 |
| 48 | [OR-ARCH-02](domains/orchestration/OR-ARCH-02.md) | Передача точного контракта между двумя узлами | or.architecture | base | 1000 |
| 49 | [OR-CODE-02](domains/orchestration/OR-CODE-02.md) | Ограниченное поручение на исправление одного дефекта | or.code | base | 1000 |
| 50 | [OR-DB-02](domains/orchestration/OR-DB-02.md) | Ключ идемпотентности отражает логическую операцию | or.database | base | 1000 |
| 51 | [OR-CLIENT-02](domains/orchestration/OR-CLIENT-02.md) | Запоздалый ответ не заменяет актуальное состояние | or.client | base | 1000 |
| 52 | [OR-SERVER-02](domains/orchestration/OR-SERVER-02.md) | Публикация опирается на подтверждённое сохранение | or.server | base | 1000 |
| 53 | [OR-MOBILE-02](domains/orchestration/OR-MOBILE-02.md) | Продолжение после возврата приложения из фона | or.mobile | base | 1000 |
| 54 | [OR-DESIGN-02](domains/orchestration/OR-DESIGN-02.md) | Видимость и редактирование по роли внутри одного экрана | or.design | base | 1000 |
| 55 | [OR-UX-02](domains/orchestration/OR-UX-02.md) | Ошибка одного поля без потери остального ввода | or.ux | base | 1000 |
| 56 | [OR-QA-02](domains/orchestration/OR-QA-02.md) | Сравнение, которое выделяет один учебный фактор | or.qa | base | 1000 |
| 57 | [OR-AUTO-02](domains/orchestration/OR-AUTO-02.md) | Следующий шаг графа после завершения всех зависимостей | or.automation | base | 1000 |
| 58 | [DOC-CS-04](domains/documentation/DOC-CS-04.md) | Ленивое перечисление и границы снимка данных | docs.csharp | base | 1000 |
| 59 | [DOC-CS-05](domains/documentation/DOC-CS-05.md) | Возврат, finally и порядок освобождения ресурса | docs.csharp | base | 1000 |
| 60 | [DOC-SQL-04](domains/documentation/DOC-SQL-04.md) | Оконный агрегат, рамка и устойчивый порядок | docs.sql | base | 1000 |
| 61 | [DOC-SQL-05](domains/documentation/DOC-SQL-05.md) | WHERE и HAVING: отбор строк и отбор групп | docs.sql | base | 1000 |
| 62 | [DOC-NG-04](domains/documentation/DOC-NG-04.md) | Reactive Forms: disabled, reset и фактическое значение | docs.angular | base | 1000 |
| 63 | [DOC-NG-05](domains/documentation/DOC-NG-05.md) | Маршрут и граница ленивой загрузки | docs.angular | base | 1000 |
| 64 | [CODE-CS-03](domains/coding/CODE-CS-03.md) | Устойчивое удаление дублей по заданному ключу | code.csharp | base | 1000 |
| 65 | [CODE-CS-04](domains/coding/CODE-CS-04.md) | Decimal: разрядность, midpoint и момент округления | code.csharp | base | 1000 |
| 66 | [CODE-SQL-03](domains/coding/CODE-SQL-03.md) | Выбор строки в группе с явным разрешением равенств | code.sql | base | 1000 |
| 67 | [CODE-SQL-04](domains/coding/CODE-SQL-04.md) | Антисоединение: NULL, составной ключ и дубликаты | code.sql | base | 1000 |
| 68 | [CODE-NG-03](domains/coding/CODE-NG-03.md) | Неизменяемое обновление состояния для OnPush | code.angular | base | 1000 |
| 69 | [CODE-NG-04](domains/coding/CODE-NG-04.md) | Точная проекция typed Reactive Forms в запрос | code.angular | base | 1000 |
| 70 | [OR-QA-03](domains/orchestration/OR-QA-03.md) | Предметный состав проверки одного учебного решения | or.qa | base | 1000 |

Выбор и review — [CODEX по контракту 10](10-CODEX-AND-CHINESE-ROLES.md); кандидаты — выбранная китайская модель Kiro. Отчёт на русском. NEXT прекращает предыдущую задачу, STOP прекращает действия; активный процесс ожидается по одному реальному handle. Успех обучения определяется измеренным полезным показателем с показом регрессий, не числом строк этого каталога.
