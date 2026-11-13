# DPO для XIIGEN: 24 домена и три параллельные группы

**4 ноября: наборы для проверки переноса.** [Инструкция 31](31-TRANSFER-VALIDATION-SIMULATIONS.md) связывает реальные партии с [планом 158](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-learning-sets-transfer-coverage-2026-11-04.md). Указаны отдельное место симуляций, независимый экзамен, конфликтующие ID и различающие проверки C#/SQL/Angular/перевода/router. Исходные данные и квоты кампании сохраняются; новые пары и обучение этой документной правкой не выполнены.

**Текущий этап — подготовка и создание по 1000 кандидатных пар на домен, всего 24 000.** [Реестр](campaign/registry.json) содержит 217 небольших назначений и является единственным владельцем текущих квот. 21 исходный домен и их квоты сохранены; добавлены три отдельно запрошенные способности. Генерация в Kiro и обучение XIIGEN этой поставкой не выполнялись.

[Начать](00-START-HERE.md) · [главный промпт](prompts/MASTER-PROMPT-RU.md) · [общая очередь](EXECUTION-QUEUE.md) · [навигатор источников](prompts/LIBRARY-NAVIGATOR-RU.md) · [результат поставки](campaign/DELIVERY-2026-10-26.md).

Новый [аудит локальных кандидатов 26 октября](30-GENERATOR-DELTA-AUDIT-2026-10-26.md): 30 конфликтующих ID, повторные незаданные сообщения в A128 и 22 новые ошибки буквенных операций. Охват и положительные примеры указаны отдельно; весь корпус и обученная модель не аттестованы. Предметные уточнения включены в короткие стартовые подсказки, разбор аудита не добавляется к каждой партии.

## Три готовых запуска

| Независимое окно Kiro | Пул | Домены | Квота |
|---|---|---|---:|
| [Переводы и опечатки](prompts/PARALLEL-TRANSLATIONS-RU.md) | `translations` | `tr.en`, `tr.ru`, `tr.he`, `text.typo-normalization` | 4000 |
| [Оркестрация и части prompt](prompts/PARALLEL-ORCHESTRATION-RU.md) | `orchestration` | 12 `or.*` + `prompt.span-routing`, `prompt.task-compilation` | 14000 |
| [Код и документация](prompts/PARALLEL-SOFTWARE-RU.md) | `software` | 3 `docs.*` + 3 `code.*` | 6000 |

Каждое окно создаёт свой agent ID. Координатор назначает свободный домен и резервирует нужные общие семейства. Работа с моделью проходит вне короткой файловой блокировки. Статусы показывают владельца, тему, реальные сохранённые строки, зачтённые кандидаты, остаток и замечания к качеству. Исходные request/draft сохраняются; текущие команды находятся в возвращённом `coordination-commands.md`.

[Локальный аудит кандидатов от 26 октября](28-GENERATOR-OUTPUT-AUDIT-2026-10-26.md) · [хорошие и плохие примеры по его находкам](prompts/AUTHORING-SELF-CHECKS-2026-10-26.md). Аудит относится к указанному в нём срезу; прежнее описание runner не подменяет текущий координатор. Генератор читает только относящийся к назначению раздел памятки. Это не отдельный review-шаг и не изменение квот. **DRAFT_CREATED означает одну созданную партию, а не достигнутую квоту или установленное качество.** Копии и retry той же пары не становятся новыми уникальными парами.

| Назначение | Точный путь Windows |
|---|---|
| Создаваемые наборы | `C:/Projects/llm_mvp_core/learning-sets` |
| Инструкции и назначенные источники | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation` |
| Общая матрица доменов, тем, владельцев и качества | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json` |
| Авторитетное состояние координатора | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/state.json` |
| Общий материал смысловых семейств | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/family-material.jsonl` |
| Новая партия | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/Axxx/Bxxxx/draft.jsonl` |

## Домены и объём

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

В переводческих доменах по 500 строк каждого из двух входных языков. У нормализатора EN=334, RU=333, HE=333. `or.routing` сохраняет прежний атомарный route/clarify-контракт с 20 предметными получателями; новые D22/D23 обучают отдельные действия над составным prompt, не расширяя старый контракт задним числом. «Создание DPO для локального обучения каждого домена» — требуемый результат этих 24 наборов, а не рекурсивный домен генерации DPO.

## Что входит в пакет

- 24 карточки доменов, 217 предметных назначений, точные квоты, следующая карточка внутри каждого пула и микротематические prompt для генератора.
- **203 новых открытых образца:** 126 перевод/нормализация и 77 оркестрация/документация/код/разбор prompt. Они дополняют прежние примеры; не входят в квоту и не становятся независимым EXAM. В рабочее задание попадают только подходящие к его objective/profile примеры.
- Две инструкции простой самопроверки: [переводы и опечатки](campaign/TRANSLATION-SELF-CHECKS-2026-10-26.md), [оркестрация и программные задачи](campaign/SOFTWARE-SELF-CHECKS-2026-10-26.md).
- [Четыре сквозных reference-сценария](campaign/PROMPT-PIPELINE-REFERENCE-2026-10-26.md): полный исходный prompt → части/домены → точная задача → ожидаемый предметный результат. Это примеры контрактов, не утверждение о реализованном native pipeline.
- [RAG-индекс](rag/INDEX.md): 69 записей, включая 39 выборочно перенесённых полных originals из `main`. Новые четыре источника описаны с ограничениями в [навигации prompt](rag/PARALLEL-PROMPT-SOURCES-2026-10-26.md). Весь `main` не копируется.
- [Оценка существующих коммитов](verification/COMMITTED-CORPUS-ASSESSMENT-2026-10-26.md) и [исходные замечания](campaign/quality-status-seed.json), автоматически добавляемые в общую runtime-матрицу без обнуления её прогресса.

Reason internally in English. Do not output private reasoning or a chain-of-thought transcript. Краткие статусы и итог — по-русски; учебный текст — на языке назначенного профиля. Используй текущую выбранную китайскую модель Kiro без override. Не угадывай имя или версию модели; неизвестное происхождение обозначай `unknown`.

CREATE_ONLY: реальные кандидатные DPO-пары и материал семейств. Самопроверка смысла каждой составляемой строки входит в создание. Отдельный reviewer, checksum-гейты, acceptance, export/import, обучение и production CI в эту сессию не входят. `finish` проверяет структуру и назначенный объём, но не доказывает смысловую правильность или обучение.

Исторические опыты, записи, IDs и альтернативная кампания 192 потоков сохранены. Их отдельные квоты не прибавляются к текущим 24 000. [Прежние входные документы](history/before-parallel-2026-10-26/) · [граница будущего обучения](29-ADDITIONAL-DOMAINS-AND-LEARNING.md) · [правила сохранения checkpoint](05-LEARNING-AND-CHECKPOINTS.md).
