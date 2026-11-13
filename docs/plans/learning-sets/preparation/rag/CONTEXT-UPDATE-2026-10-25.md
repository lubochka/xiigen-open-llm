# RAG: контекст перевода, декомпозиция и предметная проверка

25.10.2026. К сохранённым 55 источникам добавлены два; теперь **57 закреплённых записей**. Это курируемые выдержки, не полный индекс всего репозитория и не train-ready корпус.

| ID | Материал | Что использовать | Что не переносить |
|---|---|---|---|
| [RAG-OR-004](sources/RAG-OR-004.md) | GUIDE-B26-STEP-6-CYCLE3-CONTEXT | Полезное разделение ответственности и сохранение обязательств | Несогласованные S2/S4/S5, произвольный threshold/depth, эвристику по слову and |
| [RAG-OR-005](sources/RAG-OR-005.md) | GUIDE-B24-STEP-4-CYCLE2-TEMPLATE | Предметные проверяющие для конкретного шага | Фиксированные панели, оценки и модели как универсальное правило |

Источник обоих — `main@f16e68bc662bc629bc45bd0e6a46011c12865199`. Пути, line spans, Git blob SHA и SHA-256 выдержок находятся в [manifest.json](manifest.json). Все прежние записи и байты источников сохраняются. [coverage.json](coverage.json) отражает ограниченное покрытие; [search.py](search.py) проверяет целостность и ищет локально без вызовов модели.

В planning-ветке добавлены 6 микроцелей и 76 публичных reference-строк. Учебные источники и ответы симуляций различаются: описанные новые пары не приписаны исходникам как настоящий runtime gold. Контрфактуалы и переводы одних сцен имеют общую lineage; четыре SQL-GUARD условия уточнены с сохранением старых полных строк. [Состав дополнения](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/learning-sets/preparation/16-CONTEXT-AND-TRANSACTION-CASES.md).

Общий пакет: 119 карточек, 162 потоков, 728 активных reference-строк. [Стартовый промпт](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/learning-sets/preparation/14-GENERATION-CAMPAIGN.md), [полная очередь](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/learning-sets/preparation/EXECUTION-QUEUE.md). Generated/accepted TRAIN, native oracle и обучение этим обновлением не заявляются.
