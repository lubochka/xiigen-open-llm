# Продолжение аудита XIIGEN — 11 ноября 2026

Основной документ: [проверка исполнения, уточнённый план и рабочие карточки](02-audit-and-repair-addendum.md).

## Что входит в пакет

| Материал | Назначение |
|---|---|
| [Аудит продолжения](02-audit-and-repair-addendum.md) | Сверка F01–F13 и актуальной доски B01–B16, актуальные границы и порядок ремонта |
| [План ремонта](../repair-plan.md) | 14 шагов, дополненные atomic claim, trusted receipts, PairId/import и finalizer |
| [Карта карточек](03-card-acceptance-matrix.md) | 18 изменённых существующих документов: 14 core карточек/договоров, план, 3 planning постановки |
| [Manifest вставок](document-change-manifest.json) | Base/new blobs, SHA256 и доказательство сохранения старых байтов |
| [Product calculations](product-recomputation.json) | P54 oracle/DP, WC01 replay, P49/P52, scalar loss |
| [Evidence review](evidence-review.md) | Sink/finalizer, raw guard TRX, full-suite console и ROW124 |
| [Evidence calculations](evidence-review-portable-recomputed.json) | Пересчёты с 50 pinned inputs |
| [Поздняя delta B01/F02](05-late-b01-delta.md) | Helper уже есть без wiring; новый resource denominator и absolute caps проверены |
| [Независимый review](final-review.md) | Найденные дефекты самого плана и проверка их устранения |
| [Проверка пакета](04-validation.md) | Ссылки, байты, scope и границы публикации |

Продуктовые код/тесты и исторические raw не изменялись. Native tests не запускались: .NET SDK и PowerShell отсутствуют. Все новые численные расчёты отдельно названы Python/аналитическими; новые Train/Predict/Flow/Load не выполнялись.

## Как воспроизвести расчёты

Нужны Python со стандартной библиотекой и файлы core, совпадающие с pin 55cfd9d00bbebdeae04da7c505b84d0b1cb9b747. Скрипты находятся в этом каталоге; core checkout для входов может быть отдельным. Оба output должны быть новыми путями, вне опубликованных raw.

    python <audit-dir>/recompute_product_measurements.py --core <pinned-core-root> --output <new-product-result.json>
    python <audit-dir>/recompute_evidence_followup_portable.py --core <pinned-core-root> --output <new-evidence-result.json>

Первый скрипт читает [source-manifest.json](source-manifest.json), второй содержит expected blobs внутри. Несовпадение source SHA, отключённые assertions Python или существующий output не дают молчаливого нового PASS. Оба повторно проверяют неизменность входов. Они не скачивают отсутствующие raw и не запускают модели/C#/PowerShell.

P54 проверяется только как ограниченная синтетическая интервальная модель. WC01 повторяет source-equivalent метод прежнего replay. ROW124 пересчитывает текстовые receipts, а не двоичные преобразования. Main full-suite TRX не получен; его XML parse не заявляется.

Поздняя проверка движущейся ветки имеет отдельный pin05caa и два новых receipts:

    python <audit-dir>/recompute_delta_allocations.py --core <core-with-05caa-inputs> --output <new-delta-result.json>

Этот третий скрипт использует явные runtime проверки и не зависит от Python assert. Он пересчитывает сохранённые planner allocations; whole-product/native проверка им не выполняется.

## Промежуточные сохранения

- [8322da46](https://github.com/lubochka/llm_mvp_core/commit/8322da463bf3b40bb07f0cb6f4cfc025abbdde11) — область и закреплённые источники продолжения.
- [0b2cca56](https://github.com/lubochka/llm_mvp_core/commit/0b2cca56c69d524f6057d82a1cc2828e05e9377e) — воспроизводимые product calculations.
- [405e7e5b](https://github.com/lubochka/xiigen-planning/commit/405e7e5bcfc245c4f68980175e57912dfd108b96) — реальные усиления N28/N29/N60-01 и planning checkpoint.

Параллельные публикации предыдущей сессии d9fc526b/3fb0519e сохранены. Финальная публикация core и closing note planning проверяются чтением по commit SHA; отсутствие собственного commit ID в файле до создания commit не заменяется вымышленной квитанцией.
