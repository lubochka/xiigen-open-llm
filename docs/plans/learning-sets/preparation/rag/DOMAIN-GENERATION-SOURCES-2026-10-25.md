# Источники новой волны подготовки DPO

Дата: 25.10.2026. Это карта чтения ограниченной выборки проекта, не аудит текущего production-кода и не доказательство обучения. Действующие карточки и промпты — в llm_plan3. Копии новых RAG-материалов размещаются также в main по прямому поручению пользователя.

## Новые исходники main

| ID | Закреплённый источник | Нужный фрагмент | Применение |
|---|---|---|---|
| [RAG-OR-006](sources/RAG-OR-006.md) | [FLOW-04-ENGINE-GAP-LIST](https://github.com/lubochka/xiigen-planning/blob/62525ad363746c33880797d902f77e0741c872a8/Flow%20preparation/final%20results/FLOW-04-ENGINE-GAP-LIST.md) | Строки 22–69 и 73–116 | OR-AUTO-EVENT-GATE, OR-DB-CAPACITY-ATOMIC, OR-QA-SCORE-PRESENCE |
| [RAG-OR-007](sources/RAG-OR-007.md) | [FLOW02 R19 SIM GAP CATALOG](https://github.com/lubochka/xiigen-planning/blob/62525ad363746c33880797d902f77e0741c872a8/Flow%20preparation/final%20results/FLOW%2002-%20R19-SIM-GAP-CATALOG.md) | Строки 87–103, rc3 | OR-ARCH-EVENT-CONTRACT |

Полные выбранные файлы: [006](originals/RAG-OR-006.md), [007](originals/RAG-OR-007.md). Это адресный перенос двух файлов, без скачивания main целиком. [Manifest](manifest.json) хранит Git-происхождение, [transfer map](transfer-map.json) — назначение копий. Наличие исторических SHA не требует checksum-прохода от генератора.

Исторический код `score or fallback` сам способен подменить настоящий 0; он не является эталоном новых пар. Имя `decrementAndCreate` или regex-проверка имени не доказывают атомарность. Новые примеры явно задают семантику состояния, границ транзакции, rollback, источника оценки и event contract. Пути TypeScript/Nest, thresholds, команды и release-гейты исходных документов не объявляются существующими интерфейсами native C# и не выполняются.

## Повторно использованные источники

| Материал | Источники | Что они дают |
|---|---|---|
| Повтор букв, числа/единицы, порядок событий в EN/RU/HE | [RAG-023](sources/RAG-023.md), [RAG-024](sources/RAG-024.md) | Методика авторинга, представление переводческой задачи и границы ошибочного ответа; новые предложения — авторские симуляции |
| Документация C#-цепочки и код округления | [RAG-027](sources/RAG-027.md), [RAG-028](sources/RAG-028.md), [RAG-029](sources/RAG-029.md) | Полный контекст, версии, видимый вызов и отличие документации от написания кода |
| SQL window/frame/rank/ties | [RAG-040](sources/RAG-040.md), [RAG-027](sources/RAG-027.md), [RAG-028](sources/RAG-028.md) | В частности исходный вопрос window vs GROUP BY, строки 696/841 оригинала; конкретные таблицы и oracle новых пар не выполнялись |
| Готовность runtime config | [RAG-037](sources/RAG-037.md), [RAG-027](sources/RAG-027.md), [RAG-028](sources/RAG-028.md) | Проектный ready/whenReady/load-контракт; новая Promise-симуляция не является доказанным Angular bootstrap |
| Атомарное выделение capacity и канонический handoff | [RAG-OR-001](sources/RAG-OR-001.md), [RAG-006](sources/RAG-006.md), новые 006/007 | Дополнительный предметный контекст; новые задачи ограничены полностью видимым условием |

## Навигация без повторного поиска

1. Назначенный stream → карточка из [единой очереди](../EXECUTION-QUEUE.md).
2. `prepare` → готовый prompt с примерами выбранного профиля, схемой и назначенными выдержками.
3. При необходимости дополнительного исходного фрагмента → точный ID этой карты → полный оригинал или закреплённый source URL.
4. Если источник отсутствует, восстановить именно его из llm_plan3. Не создавать пустой manifest, не выдумывать текст и не искать случайную копию в другом checkout.

[Переводческий модуль](../prompts/TRANSLATION-SURFACE-QUANTITY-TIME-RU.md) · [Оркестрационный модуль](../prompts/ORCHESTRATION-EVENTS-AND-EVIDENCE-RU.md) · [Документация и код](../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md).

Внешняя китайская модель создаёт DPO. Роль человека при выборе и отдельную оценку качества выполняет CODEX с Astra 6 ultracode. Сейчас CREATE_ONLY: никакого review, обучения, production CI или обязательного checksum перед следующей партией.
