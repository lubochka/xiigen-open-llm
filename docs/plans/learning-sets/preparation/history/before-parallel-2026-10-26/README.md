# DPO для XIIGEN: подготовка и создание по 1000 пар на домен

**Действующая кампания: 21 домен × 1000 = 21 000 кандидатных пар.** Её единственный владелец квот — [campaign/registry.json](campaign/registry.json). 192 микротемы/направления распределяют эти 21 000; каждый поток не получает дополнительные 1000. Режим **CREATE_ONLY: только создание, без review**.

**Исправление пользователя #7:** наборы пишутся в `C:\Projects\llm_mvp_core\learning-sets`; инструкции читаются из `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation`. Это два разных корня.

[Начать](00-START-HERE.md) · [готовый промпт Kiro](prompts/MASTER-PROMPT-RU.md) · [порядок и квоты](EXECUTION-QUEUE.md) · [навигатор библиотеки](prompts/LIBRARY-NAVIGATOR-RU.md) · [что исправлено и почему](campaign/DELIVERY-2026-10-25.md)

## Запуск

Если пользователь уже выбрал конкретный сохранённый request, продолжить именно его через `resume`; новый пакет не переносит и не перезаписывает старую попытку. Для новой кампании:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

Скрипт подготавливает назначение, но не вызывает китайскую модель. Kiro читает выданный `prompt.md`, создаёт настоящие `prompt/chosen/rejected` и отсутствующий материал семей, выполняет готовые `finish` и `next` из `commands.md`. Следующий вызов `next` возобновляет незаконченный request либо назначает следующую партию. **DRAFT_CREATED означает создание одной партии, а не достижение 1000.** Статус считает фактически записанные кандидаты по каждому домену.

| Назначение | Точный путь Windows |
|---|---|
| **Создаваемые наборы** | `C:/Projects/llm_mvp_core/learning-sets` |
| Инструкции, карточки, исходные материалы | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation` |
| Основной скрипт | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1` |
| Состояние новой кампании | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/state.json` |
| Сохранённые смысловые семейства | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/family-material.jsonl` |


## Объём и содержание

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
| 21 | [code.angular](campaign/cards/D21-code.angular.md) | Написание Angular / TypeScript | 1000 | CAMPAIGN_DRAFTS_CREATED |


В каждом переводческом домене — по 500 строк для двух входных языков. `or.routing` — существующая самостоятельная тема маршрутизатора: 20 целевых доменов и класс `clarify`. Документация и написание кода имеют отдельные наборы C#/.NET, SQL и Angular. Каждая партия содержит одну микротему и один профиль; общий доменный счёт не означает смешивание тем в одном файле.

## Что получает генератор

- [21 карточку доменов](campaign/cards/) с квотой, следующей карточкой, хорошими и плохими ответами и простой самопроверкой.
- [192 компактных предметных назначения](campaign/lessons.json): для каждого профиля не менее двух полных контрастных пар, ограничения задачи и точные RAG-выдержки. В рабочий prompt попадает только назначенная микротема.
- [63 разобранных примера по доменам](campaign/AUTHORING-EXAMPLES.md), [24 дополнительных примера опечаток и раскладки](campaign/translation-second-examples.json), [20 примеров из новых источников](rag/authoring-references-2026-10-25.json). Это публичные учебные опоры автора; они не входят в квоту и не являются результатом китайской генерации.
- [65 источников RAG](rag/manifest.json), включая 35 выборочно перенесённых полных файлов из `main`. Все назначенные материалы доступны в `llm_plan3`; скачивать весь `main` не требуется.

Текущая китайская модель наследуется из Kiro без подмены. Внутреннее рассуждение — на английском; прогресс и отчёт — по-русски; учебный текст — на языке профиля. Скрытое рассуждение не выводится и не объявляется проверенным. Checksum, отдельный reviewer, фиксированный alias модели, production CI, экспорт и обучение не входят в создание.

Исторические карточки, примеры, опыты и результаты сохранены. Старый расширенный план 132 000 и его варианты теперь явно помечены как история, а не текущая квота. [Материалы до изменения](history/before-domain-campaign-2026-10-25/) · [будущая проверка обучения и повторное использование checkpoint](05-LEARNING-AND-CHECKPOINTS.md) · [граница native-импорта](10-NATIVE-CONTRACT-BOUNDARY.md).

Статус этой поставки: подготовлены инструкции, источники и инструменты создания. Китайская генерация 21 000 пар и Student-обучение этой поставкой не выполнялись; качество обучения не измерялось.

**Параллельное поручение по 192 потокам:** [контракт другой кампании](27-CAMPAIGN-CONTINUATION-CONTRACT.md) сохранён отдельно. Его 192000/192008 не меняют квоту D1000. Если пользователь назначил именно старую кампанию или существующий request, продолжать их; новая кампания не заменяет их автоматически.
