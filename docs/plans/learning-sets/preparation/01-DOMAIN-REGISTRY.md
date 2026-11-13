# Действующий реестр доменов

**24 домена, каждый по 1000 кандидатных TRAIN-пар: 24 000 всего.** Машинная квота и порядок — [campaign/registry.json](campaign/registry.json); [217 назначений](EXECUTION-QUEUE.md) делят её между микротемами и профилями. Начальная 21-доменная часть сохранена с прежними IDs и объёмом. Исторический каталог тем `index.json` не задаёт текущие квоты.

| № | Домен / карточка | Содержание | Пул | Квота |
|---:|---|---|---|---:|
| 1 | [tr.en](campaign/cards/D01-tr.en.md) | Перевод на английский | `translations` | 1000 |
| 2 | [tr.ru](campaign/cards/D02-tr.ru.md) | Перевод на русский | `translations` | 1000 |
| 3 | [tr.he](campaign/cards/D03-tr.he.md) | Перевод на иврит | `translations` | 1000 |
| 4 | [or.routing](campaign/cards/D04-or.routing.md) | Маршрутизация по намерению | `orchestration` | 1000 |
| 5 | [or.functional](campaign/cards/D05-or.functional.md) | Функциональное описание | `orchestration` | 1000 |
| 6 | [or.architecture](campaign/cards/D06-or.architecture.md) | Архитектура | `orchestration` | 1000 |
| 7 | [or.code](campaign/cards/D07-or.code.md) | Оркестрация реализации кода | `orchestration` | 1000 |
| 8 | [or.database](campaign/cards/D08-or.database.md) | Оркестрация изменений базы данных | `orchestration` | 1000 |
| 9 | [or.client](campaign/cards/D09-or.client.md) | Клиент | `orchestration` | 1000 |
| 10 | [or.server](campaign/cards/D10-or.server.md) | Сервер | `orchestration` | 1000 |
| 11 | [or.mobile](campaign/cards/D11-or.mobile.md) | Мобильное приложение | `orchestration` | 1000 |
| 12 | [or.design](campaign/cards/D12-or.design.md) | Дизайн | `orchestration` | 1000 |
| 13 | [or.ux](campaign/cards/D13-or.ux.md) | UX | `orchestration` | 1000 |
| 14 | [or.qa](campaign/cards/D14-or.qa.md) | QA | `orchestration` | 1000 |
| 15 | [or.automation](campaign/cards/D15-or.automation.md) | Автоматизация | `orchestration` | 1000 |
| 16 | [docs.csharp](campaign/cards/D16-docs.csharp.md) | Документация C# / .NET | `software` | 1000 |
| 17 | [docs.sql](campaign/cards/D17-docs.sql.md) | Документация SQL | `software` | 1000 |
| 18 | [docs.angular](campaign/cards/D18-docs.angular.md) | Документация Angular | `software` | 1000 |
| 19 | [code.csharp](campaign/cards/D19-code.csharp.md) | Написание C# / .NET | `software` | 1000 |
| 20 | [code.sql](campaign/cards/D20-code.sql.md) | Написание SQL | `software` | 1000 |
| 21 | [code.angular](campaign/cards/D21-code.angular.md) | Написание Angular / TypeScript | `software` | 1000 |
| 22 | [prompt.span-routing](campaign/cards/D22-prompt.span-routing.md) | Разметка частей пользовательского промпта по доменам | `orchestration` | 1000 |
| 23 | [prompt.task-compilation](campaign/cards/D23-prompt.task-compilation.md) | Короткое точное задание для назначенного домена | `orchestration` | 1000 |
| 24 | [text.typo-normalization](campaign/cards/D24-text.typo-normalization.md) | Восстановление опечаток на исходном языке | `translations` | 1000 |

## Различия задач

`or.routing` классифицирует одно атомарное намерение по прежнему каталогу 20 предметных получателей либо `clarify`. D22 `prompt.span-routing` выделяет все самостоятельные части более длинного пользовательского текста, их предметные домены и явные зависимости. D23 `prompt.task-compilation` получает полный оригинал, выбранный сегмент, назначенный домен и реальные результаты предшественников; возвращает краткую точную задачу либо конкретный недостающий вход. Он не исполняет предметную задачу. D24 `text.typo-normalization` возвращает восстановленный текст на исходном языке, сохраняет явно буквальный текст или просит уточнение при неоднозначности.

Домен определяется обучаемым поведением, профиль — языком/направлением или вариантом контракта, микротема — узким содержательным навыком. «Отрицательный домен» не используется как название rejected-класса. Одна семья означает общий предметный сюжет; похожее написание не объединяет независимые слова.

## Профили новых задач

| Домен | Распределение | Микротемы |
|---|---|---|
| `prompt.span-routing` | `base`: 1000 | Покрытие частей: 600; зависимости: 400 |
| `prompt.task-compilation` | `base`: 1000 | Точность задачи: 700; недостающий вход: 300 |
| `text.typo-normalization` | EN 334 / RU 333 / HE 333 | DELETE, SUBSTITUTE, TRANSPOSE, INSERT, LAYOUT, PRESERVE, AMBIGUITY |

Профили `base` D22/D23 содержат короткие запросы RU/EN/HE по явно видимому каталогу и языковому условию. Новые задания и предметные ответы согласуются с [схемами и reference-пайплайном](29-ADDITIONAL-DOMAINS-AND-LEARNING.md). Дополнительный метадомен «создавать DPO» не вводится: DPO-пары — выход нынешней подготовки каждого домена.
