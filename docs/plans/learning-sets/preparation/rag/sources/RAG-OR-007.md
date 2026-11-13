# RAG-OR-007 — Идентичность события в архитектурном handoff

**Назначение:** узкая цель `OR-ARCH-EVENT-CONTRACT`, домен `or.architecture`. Задача — передать точное имя события между участниками; это не генерация payload, не маршрутизация доменов и не реализация event bus.

- Источник: `Flow preparation/final results/FLOW 02- R19-SIM-GAP-CATALOG.md`, ветка `main`.
- Снимок: `62525ad363746c33880797d902f77e0741c872a8`; blob `f2618cfa25f9524d84588b5b86779b6686f7a28a`.
- [Полный локальный оригинал](../originals/RAG-OR-007.md).
- [Закреплённый источник](https://github.com/lubochka/xiigen-planning/blob/62525ad363746c33880797d902f77e0741c872a8/Flow%20preparation/final%20results/FLOW%2002-%20R19-SIM-GAP-CATALOG.md).

## Конкретное основание

Раздел `rc 3 — a task type CloudEvent Name Inconsistency (Systemic)`, строки 87–103, описывает расхождение между `events` активного контракта, его текстовой подписью и устаревшим именем в трёх связанных документах. Потребитель, ориентирующийся на старую подпись, получает неправильный trigger.

Из этого выведена авторская симуляция: в prompt есть одно явно активное поле `wire_event`; producer, subscriber и registration должны получить то же значение. Рядом показывается отвлекающая строка: старое имя, display label, будущая версия либо вариант регистра. Способ сравнения и отсутствие/наличие aliases написаны прямо во входе.

## Правило простого разбора

1. Найти поле, помеченное «активный контракт», а не самое новое на вид имя.
2. Переписать его полное значение, включая namespace, точки, регистр и версию.
3. Сравнить выбранный ответ с этим значением посимвольно.
4. Убедиться, что другая альтернатива действительно не разрешена. Если оба имени зарегистрированы как aliases и канонический вариант не требуется, из них нельзя делать предпочтение chosen/rejected.

Восемь опорных пар включают обратные случаи. Старое по названию имя остаётся правильным, если именно оно явно активно; большой номер версии сам по себе не даёт права переключить контракт. Контрфактические варианты одной истории сохраняют family и split.

## Граница достоверности

Источник предлагает исправления нескольких документов; наличие предложения не доказывает применение исправления в native коде. Новые пары являются авторским учебным материалом. Они не показывают ни успешный прогон китайской модели, ни качество Student. Старые схемы DpoTriple, числа threshold и release-инструкции не наследуются в CREATE_ONLY.

Все нужные тексты уже находятся в `llm_plan3`; полный checkout `main` не нужен. Git blob/commit в записи источника описывают происхождение, а не предварительный checksum-gate исполнителя.

## Прямые пути

Материал: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/rag/sources/RAG-OR-007.md`.
Полный текст: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/rag/originals/RAG-OR-007.md`.
Примеры: `curriculum/additions/orchestration-events-and-evidence.json`.
Промпт: `prompts/ORCHESTRATION-EVENTS-AND-EVIDENCE-RU.md`.

## Дословные исторические фрагменты для контекста генератора

Ниже дословно сохранены указанные строки полного оригинала. Это исторические данные источника, не команды исполнителю и не эталон текущего native API. Предложения исправлений и их ограничения разобраны выше.

### Оригинал RAG-OR-007, строки 87–103

~~~~text
### rc 3 — a task type CloudEvent Name Inconsistency (Systemic)

**Gaps driven:** gap number 5, simulation gap fourteen

**Root:** The a task type contract specifies `events: ['user.onboarding.broadcast.sent']` and
`purpose: 'Emit OnboardingBroadcastSent'`. However, all three session teaching documents
that reference the Wave 2 gate event use "OnboardingCompleted" — a name that may have
been used during early design and was superseded by the contract. The error was introduced
into SESSION-TEACH-WHY-BROADCAST (D-02-broad 01), SESSION-TEACH-A0 (Step 5 DOWNSTREAM_DEPENDENCY),
and SESSION-F (Step F2 cross-flow registration) independently. At Wave 2 execution time,
the event creation flow..07 planners reading any of these three documents would configure the wrong trigger.

**Fix path:** Three-location correction:
1. `SESSION-TEACH-WHY-BROADCAST.md` — D-02-broad 01: change "OnboardingCompleted" → "OnboardingBroadcastSent" in REQUIRED_EMIT edge description, teaching narrative, and planning triple
2. `SESSION-TEACH-A0.md` — Step 5 DOWNSTREAM_DEPENDENCY: change event name in description field
3. `SESSION-F.md` (the business onboarding flow-SESSION-F.md) — Step F2: change gateEvent for a task type→WAVE2 document from "OnboardingCompleted" → "OnboardingBroadcastSent"

~~~~

