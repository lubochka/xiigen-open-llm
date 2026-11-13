# Симуляции для проверки переносов — подготовка, не обученный результат

Статус: **SIMULATION_NOT_PRODUCTION / DESIGN_ONLY**. Здесь пока нет новых учебных пар или checkpoint. Исходные партии в соседних каталогах сохраняются.

Владелец назначения — [план 158](../../../docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-learning-sets-transfer-coverage-2026-11-04.md); подготовка — [инструкция 31](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/31-TRANSFER-VALIDATION-SIMULATIONS.md).

Будущая структура: `<profile>/v1/{selection.json,train.jsonl,dev.jsonl,exam.jsonl,review.json,projection.json}`. Изменение существующей партии создаёт новую версию с происхождением, а не перезаписывает исходник. Генератор не объявляет свою самопроверку независимым review; наличие файлов не даёт права на production import. Учебный старт хранится отдельно по 158 и появляется только после реально выполненной квалификации.
