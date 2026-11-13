# Карта адресных усилений и будущей приёмки

Дата: 11 ноября 2026. Здесь перечислены реально изменённые существующие документы. Все проверки в последнем столбце — требования будущего ремонта, если отдельно не названы уже выполненным аналитическим пересчётом. Нового native PASS нет.

14 core карточек/договоров + 1 core repair-plan + 3 planning постановки = 18 существующих документов этого продолжения. В объединении с прежними 11 и четырьмя пересечениями — 24 уникальные карточки/договора и отдельно план. Исходные байты всех 18 файлов сохранены.

| Документ | Основание | Что усилено | Различающая приёмка |
|---|---|---|---|
| [core: P48-weighting-curriculum.md](../../../../experimets/cards/P48-weighting-curriculum.md) | F04/B06; F10 | Finite verdict отдельно от whole-run/CE | Late assert, соседний failed case, missing TRX; backward-order сохранён |
| [core: P49-probe-warm-start.md](../../../../experimets/cards/P49-probe-warm-start.md) | F07 и F04 | Input novelty не равно pair novelty | Фактические e-t1/e-t2, три группы 2+1+1; finalizer после late asserts |
| [core: P50-graph-dependencies.md](../../../../experimets/cards/P50-graph-dependencies.md) | F05 | Guard управляет validity/hypothesis/actual exit | Lessons8→9, unknown step, missing stage через тот же host |
| [core: P52-semantic-annotation.md](../../../../experimets/cards/P52-semantic-annotation.md) | F06/B03 и B05/B06 | Ceiling 2/4 отдельно от cold0/4 и от иной попытки | Gold oracle, path/hash/JSON pointer, owner до RunArm |
| [core: P54-phrase-composition.md](../../../../experimets/cards/P54-phrase-composition.md) | F01/B01, F02 | Лучший допустимый план и matched resource window | Оба interval witnesses, ordinary reader, reverse mutation, boundaries |
| [core: P58-feedback-learning.md](../../../../experimets/cards/P58-feedback-learning.md) | F04/F13, B05/B06 | Достижимый invalid/no-contrast и stage ledger | TRAIN acquisition, equal-start/instrument failure; healthy main105/56 |
| [core: 03-test-and-execution-contract.md](../../../03-test-and-execution-contract.md) | F03/F04, B05–B09 | Owner до preparation/Train; terminal принадлежит runner | Untracked collision, concurrency, late failure, missing raw, bounded incomplete |
| [core: 04-evidence-and-novelty.md](../../../04-evidence-and-novelty.md) | F06, B07, ROW124 | Уровень доказательства и immutable correction | Независимый oracle; raw parse отдельно от recount; method/computed hashes |
| [core: 106-feedback-and-transition-memory-plan-2026-10-30.md](../../../106-feedback-and-transition-memory-plan-2026-10-30.md) | F09/B10; review R2/R3 | Server-owned canonical receipts и in-flight claim | True echo, forged text, 2 deliveries/1 dose, failure-after-Train, trusted restore |
| [core: 15-prompt-flow-retention.md](../../../15-prompt-flow-retention.md) | F08/B11; review R4 | PairId immutable; целый candidate import | Changed duplicate до mutation; incompatible member; intact old state |
| [core: 20-prompt-domain-e2e.md](../../../20-prompt-domain-e2e.md) | F08/B11 и B12 | FlowRef/revision/PairId/body/recipient — одна цепь | Stale/empty/foreign body; exact/reordered/no-overlap; actual fallback |
| [core: 32-wiki-feature-key-isolation.md](../../../32-wiki-feature-key-isolation.md) | Профилактика F02/F11 | Собственный ресурсный допуск и отдельные оси | Не переносить FIX-WC допуск на FIX-WK; epoch ledger; resource mutation |
| [core: 48-learned-route-body-identity.md](../../../48-learned-route-body-identity.md) | F08/B11, B12 | Body из active revision, не по одному FlowRef | A→B1→B2, ID order, conflicting payload/checkpoint |
| [core: repair-plan.md](../repair-plan.md) | Review R1–R5 и B06 | Конструкция ремонта доведена до исполнимого договора | Atomic claim, authoritative records restore, PairId integrity, actual P49 row |
| [planning: 28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/405e7e5bcfc245c4f68980175e57912dfd108b96/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md) | F08/B11, B12, temporal correction | Сохранён current FIX-P118 и уточнена active revision | Exact vs paraphrase; duplicate identity; import и empty neighbor |
| [planning: 29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/405e7e5bcfc245c4f68980175e57912dfd108b96/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | F06/F09; профилактика leakage | Адрес/перевод/cold/ceiling и runtime-доступность label | Reached reader vs masking; canonical provenance; source-disjoint scope |
| [planning: 60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/405e7e5bcfc245c4f68980175e57912dfd108b96/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md) | F04/F13 | Физический расход и отрицательный executor path | Partial/no-contrast ledger; artificial child; no journal-only PASS |
| [core: 109-p53-p55-transfer-plan-2026-10-30.md](../../../109-p53-p55-transfer-plan-2026-10-30.md) | Поздний DRAFT B01 и повтор F02 | Признан exact-count helper без wiring; разделены component cost, product A/B и новый regression cap | 9-token scope; mixed denominator; strict< caps допускают некоторые >5%; actual path после wiring |

## Граница автоматического усиления

Общий договор 03/04 распространяет измерительные и исполнительные требования на следующие слайсы, но не заявляет, что дефект обнаружен в каждом соседе. Профилактические требования сохраняют собственные data/learner/consumer/threshold/дозу карточки. Новый научный повтор не возникает из-за вставки документа.

Полные base/new Git blob, SHA256, offset/размер вставки, сохранение прежних bytes и историческое объединение перечислены в [document-change-manifest.json](document-change-manifest.json). Источник выводов — [согласованный audit addendum](02-audit-and-repair-addendum.md); последовательность реализации — [план ремонта](../repair-plan.md).
