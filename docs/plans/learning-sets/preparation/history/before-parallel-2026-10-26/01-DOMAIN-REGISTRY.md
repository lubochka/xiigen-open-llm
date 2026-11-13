# Домены и текущая квота

Авторитетная машиночитаемая версия: [campaign/registry.json](campaign/registry.json). **21 домен, 21 000 кандидатных пар, ровно 1000 на domain_id.** У существующих 73 родительских тем и 148 предметных карточек 192 активных topic/profile-потока. Их доли перечислены в [очереди](EXECUTION-QUEUE.md); они не являются дополнительными доменами.

| Порядок | Домен | Что учим | Кандидатных пар | Следующий домен |
|---:|---|---|---:|---|
| 1 | [tr.en](campaign/cards/D01-tr.en.md) | Перевод на английский | 1000 | tr.ru |
| 2 | [tr.ru](campaign/cards/D02-tr.ru.md) | Перевод на русский | 1000 | tr.he |
| 3 | [tr.he](campaign/cards/D03-tr.he.md) | Перевод на иврит | 1000 | or.routing |
| 4 | [or.routing](campaign/cards/D04-or.routing.md) | Маршрутизация по намерению | 1000 | or.functional |
| 5 | [or.functional](campaign/cards/D05-or.functional.md) | Функциональное описание | 1000 | or.architecture |
| 6 | [or.architecture](campaign/cards/D06-or.architecture.md) | Архитектура | 1000 | or.code |
| 7 | [or.code](campaign/cards/D07-or.code.md) | Оркестрация реализации кода | 1000 | or.database |
| 8 | [or.database](campaign/cards/D08-or.database.md) | Оркестрация изменений базы данных | 1000 | or.client |
| 9 | [or.client](campaign/cards/D09-or.client.md) | Клиент | 1000 | or.server |
| 10 | [or.server](campaign/cards/D10-or.server.md) | Сервер | 1000 | or.mobile |
| 11 | [or.mobile](campaign/cards/D11-or.mobile.md) | Мобильное приложение | 1000 | or.design |
| 12 | [or.design](campaign/cards/D12-or.design.md) | Дизайн | 1000 | or.ux |
| 13 | [or.ux](campaign/cards/D13-or.ux.md) | UX | 1000 | or.qa |
| 14 | [or.qa](campaign/cards/D14-or.qa.md) | QA | 1000 | or.automation |
| 15 | [or.automation](campaign/cards/D15-or.automation.md) | Автоматизация | 1000 | docs.csharp |
| 16 | [docs.csharp](campaign/cards/D16-docs.csharp.md) | Документация C# / .NET | 1000 | docs.sql |
| 17 | [docs.sql](campaign/cards/D17-docs.sql.md) | Документация SQL | 1000 | docs.angular |
| 18 | [docs.angular](campaign/cards/D18-docs.angular.md) | Документация Angular | 1000 | code.csharp |
| 19 | [code.csharp](campaign/cards/D19-code.csharp.md) | Написание C# / .NET | 1000 | code.sql |
| 20 | [code.sql](campaign/cards/D20-code.sql.md) | Написание SQL | 1000 | code.angular |
| 21 | [code.angular](campaign/cards/D21-code.angular.md) | Написание Angular / TypeScript | 1000 | CAMPAIGN_COMPLETE |


## Как считается перевод

| Домен результата | Профили | Всего |
|---|---|---:|
| tr.en | ru-en: 500; he-en: 500 | 1000 |
| tr.ru | en-ru: 500; he-ru: 500 | 1000 |
| tr.he | en-he: 500; ru-he: 500 | 1000 |

Опечатки, неверная раскладка и правильная форма одной лексемы связаны через lexical family. Общий сюжет связан через scenario/origin family. Две самостоятельные лексемы не объединяются только по расстоянию между строками. Для shared source в коде и документации связь устанавливается явным конкретным сценарием, не совпадением порядкового номера.

Разные предметные темы остаются в отдельных shards. Классы и отрицательные контроли внутри одной темы задаются request. В or.routing запрос о переводе является входом для решения о маршруте; он не превращает этот shard в переводческий. Конечный router-набор имеет 21 класс с 47–48 строками на класс; полный состав нужен будущему учебному пилоту, первый узкий shard не объявляется достаточным обучающим набором.

Старое поле `target_pairs_per_profile=1000` сохранено в историческом каталоге для совместимости. В текущей кампании оно **не используется как бюджет**. Старые расширенные квоты и очереди сохранены в history и помечены как справочные.
