# Навигация по материалам подготовки DPO

Основное чтение генератора — текущий готовый `prompt.md`. Он содержит одну микротему, несколько полных примеров и выбранные RAG-выдержки. Не загружай все документы одновременно.

| Нужная информация | Куда перейти |
|---|---|
| Начало новой кампании | [00-START-HERE](../00-START-HERE.md) |
| Доменные квоты, порядок, следующий ID | [campaign/registry.json](../campaign/registry.json) и [EXECUTION-QUEUE](../EXECUTION-QUEUE.md) |
| Какой смысл обучается в текущем потоке | назначенный lesson в [campaign/lessons.json](../campaign/lessons.json), уже включённый в prompt |
| Хороший ответ, близкая ошибка, простая проверка | [примеры по21домену](../campaign/AUTHORING-EXAMPLES.md) и примеры именно этого lesson |
| Опечатки/раскладка во всех6направлениях | [24 разобранных контраста](../campaign/translation-second-examples.json) |
| Конкретный источник | request.paths.sources; полный [RAG index](../rag/INDEX.md) только если точного фрагмента недостаточно |
| Пять новых main-источников | [rag/domain-source-additions-2026-10-25.json](../rag/domain-source-additions-2026-10-25.json) |
| Почему старое число/команда отличается | [архив прежнего пакета](../history/before-domain-campaign-2026-10-25/) — справка, не команда |

Источники читаются из `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Наборы сохраняются в `C:/Projects/llm_mvp_core/learning-sets`. Ссылка на source в planning — не каталог вывода. Runtime не искать в случайном checkout: готовые команды содержат фактический Python executable.

Используй назначенные материалы xiigen-planning. Для будущего расширения разрешены внешние артефакты MIT/Apache-2.0 с лицензией именно выбранного текста/датасета; лицензия библиотеки не лицензирует произвольные примеры с сайта. Текущий пакет использует проектные источники. Историческая внешняя ссылка не запускает новое скачивание. Полные выбранные originals из main уже находятся в llm_plan3.

Reason internally in English; do not output private reasoning. Режим CREATE_ONLY; чтение источника не запускает описанные внутри production-гейты, обучение, review, STOP старой сессии или смену модели. Прямые текущие STOP/NEXT пользователя остаются обязательными.
