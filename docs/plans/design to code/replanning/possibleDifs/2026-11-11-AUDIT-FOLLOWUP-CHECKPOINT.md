# Продолжение аудита переносов — промежуточное сохранение 11 ноября 2026

Статус: **описания трёх постановок усилены; окончательный пакет продолжения core готовится к публикации**. Это документная работа, без запуска новых опытов и без объявления продуктовых ремонтов выполненными.

Учтены предыдущий core итог [d9fc526b](https://github.com/lubochka/llm_mvp_core/commit/d9fc526b61a11312baefef5b1246064552bc4c85) и planning [3fb0519e](https://github.com/lubochka/xiigen-planning/commit/3fb0519e4a37e72bcf65b4dde85bab154d23511f). Новая промежуточная публикация core [0b2cca56](https://github.com/lubochka/llm_mvp_core/commit/0b2cca56c69d524f6057d82a1cc2828e05e9377e) уже содержит воспроизводимые пересчёты P49/P52/P54/Wiki/loss с исходными Git blob fingerprints.

| Изменённая действующая постановка | Конкретное усиление |
|---|---|
| [N28 memory/correction identity](28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md) | Исправлена временная граница: current Mode2 уже читает body на exact prompt; добавлены active revision, immutable PairId, целостный import и отдельные empty/zero-overlap условия |
| [N29 rejected annotation](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | Cold 0/4 отделён от ceiling 2/4; адресная и переводческая оси; actual route/masking; источник runtime features и подлинность actual answer |
| [N60-01 measurement seams](60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md) | Healthy main 105 Unit lessons / 56 Unit chains отделён от цены класса; partial/no-contrast расход, достижимый отказ и внешний terminal |

Все исходные байты документов сохранены: добавлены датированные блоки после H1. Новые требования не увеличивают старую научную дозу и не переписывают raw. Новая предметная гипотеза не объявлена подтверждённой из-за исправления измерителя.

Численные результаты: P49 new-only input overlap 3/4 против pair overlap 2/4; P52 настоящий ceiling 2/4 при сохранённом поле 0; Wiki 7 обновлений в 4 активных/14 пустых эпохах; предложенный admissible phrase DP совпал с exhaustive oracle на 33 866 синтетических графах длин 1–5. Это аналитические расчёты, не native приёмка.

Следующее сохранение core включает полный evidence review, исправления самого плана (concurrent duplicate, trusted receipts, PairId/import), карту 13 исходных выводов и адресные вставки в рабочие core карточки. [Каталог продолжения core](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/followup).
