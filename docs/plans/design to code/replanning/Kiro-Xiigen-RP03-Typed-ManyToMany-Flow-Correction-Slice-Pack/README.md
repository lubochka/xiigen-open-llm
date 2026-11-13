# RP03 — Typed many→many и обучаемая Flow-коррекция · V7

**Действующий вход:** [один dispatcher V7](RP03-V7-LEARNING-FIRST-DISPATCH-2026-10-15.md). Сохранены **15 parent tasks и 9 child packets** в [manifest](PACKAGE-MANIFEST.json); новая версия слайса или очередные R01–R25 не создаются.

**Опубликовано в llm_plan3:** [PR №15](https://github.com/lubochka/xiigen-planning/pull/15), merge `46d80f5f2f4d00f83683a5962fa53a2a7a4eed66`. [Фактическая публикация и exact-SHA readback](../RP01-RP05-REPAIR-ATLAS-2026-10-15/20-RP03-OCT16-PUBLICATION-AND-READBACK-RECEIPT.md) закрывают первоначальные пометки REPAIR IN PROGRESS; это документный ремонт, не runtime/learning acceptance.

**16.10.2026 — адресный ремонт первого урока:** [findings O16-01–04](../RP01-RP05-REPAIR-ATLAS-2026-10-15/17-RP03-FIRST-LESSON-READINESS-REPAIR-2026-10-16.md), [применение, O16-05–06 и границы](../RP01-RP05-REPAIR-ATLAS-2026-10-15/18-RP03-OCT16-APPLIED-REPAIR-AND-REVIEW-SCOPE.md), [O16-07: устаревший startup исправлен](../RP01-RP05-REPAIR-ATLAS-2026-10-15/19-RP03-STARTUP-DRIFT-REPAIR-2026-10-16.md). Семь parent cards и один child переписаны непосредственно, без стека взаимоисключающих инструкций. Остальные карточки сохранены, не объявлены заново выполненными или полностью переаудированными.

Начать **00B ∥ 02A ∥ 00C-A**. 00A и подготовка независимых экзаменов 00C-B идут параллельно, не задерживая первый update. 02B строит минимальный действительный scorer сразу после трассировки; применяет один шаг только по настоящему независимому lawful feedback. 02D затем загружает **экспериментальный** checkpoint в новом процессе. Ни первый update, ни этот cold trial не требуют фальшивого serving admission, полного typed JOIN или Windows Actions.

**Численное обучение не обязано немедленно менять top-1.** Отдельно фиксируются параметрический шаг, изменение score/margin, смена ranking, cold-read, предметная польза и пользовательское внедрение. Доменное улучшение определяет независимый экзамен, а не выбранный размер шага или число тестов.

Typed lane 01A→01B→01C продолжается независимо. 02C подготавливает JOIN в изолированной композиции и выполняет ordinary adoption только после необходимого scoped admission. 03B начинает доступные доменные экзамены на trial, не ожидая ordinary serving; 03C подтверждает именно обычный пользовательский путь и PC-стоимость. Нельзя называть trial ordinary Host.

Основные документы: [алгоритм и экзамены](RP03-V7-ALGORITHM-AND-EXAMS.md), [execution board](EXECUTION-BOARD.md), [функциональная карта всех 15 владельцев](RP03-V7-OCT16-FUNCTIONAL-CROSSWALK.md), [как начать](INSTALL-AND-START.md), [active tasks](tasks/v7/), [children](tasks/v7/children/).

**Ноль новых product tests для source/planning/exam tasks.** Для изменённого детерминированного риска — минимальная существующая или адресная проверка. Для обучения — независимые одинаковые held-out задания C0/T, доменные законы, accuracy, abstention, retention, время и полный PC footprint. Нет coverage-цели и blanket Windows gate. Реальные права, F0, native security и обязательства поставки сохраняются.

DPO handoff: **CODEX с Astra 6 UltraCode** выполняет роль человека при выборе; **внешняя открытая китайская модель** создаёт обе стороны; **отдельный CODEX с Astra 6 UltraCode** оценивает exact pair. Flow preference learner, Student DPO и RP18 semantic projection — разные механизмы. Исторические RP01 route/исключения не переносятся автоматически.

V5/V6, старые tasks/ и tasks/v6/, а также October15 amendment essays сохраняются как source/history, не как добавочные executable prompts поверх исправленной V7. Чтение документа, публикация плана и авторская сверка не доказывают обучение: **Core/weights не изменены этой публикацией; learning/PC/domain gain NOT_MEASURED; независимая Astra NOT_PERFORMED.**
