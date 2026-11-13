# Сверка параллельных дополнений — 25.10.2026

## Статус

Объединены изменения исходного пакета, входящего дополнения с curriculum и подготовленного расширения тем/ролей. Эта заметка фиксирует решения при слиянии документов. Исторические отчёты не удалены и не объявляются повторно выполненными.

Свежая проверка объединённого инструмента: [merged-regression-2026-10-25.json](merged-regression-2026-10-25.json), runner — [check-preparation-merged.py](check-preparation-merged.py). В этом отчёте **42 из 42** проверок прошли. SHA-256 инструмента: `a5fecb846faede76a958a1aa5663299536027403d1201e1bad79224c825c98ce`. Это новый отчёт, а не перенос прежних результатов. Он описывает искусственные данные и проверки admission/целостности: вызовов моделей — 0, нативных oracle — 0, обучения Student — 0.

Проверка общей итоговой поставки, ссылок и единой очереди — отдельная работа по текущим hashes. Эта заметка не выдаёт её за выполненную; итоговый package report связывается из [13-MERGED-PREPARATION.md](../13-MERGED-PREPARATION.md).

## Что сохранено и как разрешены конфликты

| Вопрос | Решение |
|---|---|
| Темы и узкие objectives | index.json сохраняет каталог тем; curriculum/index.json — микроцели. Пакет objective не захватывает соседние навыки родительской карточки. |
| Порядок запуска | execution-queue.json задаёт единый реальный next_stream_id. Локальные next каталогов его не заменяют. Покрытые parent/profile не запускаются автоматически вторым кругом; прямой ID пользователя имеет приоритет. |
| Роли моделей | CODEX с astra 6 ultracode выбирает материал и оценивает пары. Китайская модель, реально выбранная в Kiro, генерирует кандидатов. Заполненный шаблон не объявляется фактическим вызовом. |
| Приёмка | Наши selection/review receipts и blind/key bindings сохранены вместе с incoming language/oracle evidence. Оба needs_* — явные boolean. Статус строки training_allowed=false соблюдается. |
| Точный материал | Сохранены отклонение duplicate JSON keys/non-finite чисел, значимые пробелы/Unicode и защита известных public families/prompts от DEV/EXAM. |
| Состав микронабора | Objective ID, conditional ambiguous/resolved и routing classes сохраняются. Классы одной темы не ошибочно объявляются смешением предметных доменов. |
| Документное доказательство | code.* всегда требует execution; docs.* всегда требует evidence. Source-grounded доступен только неисполняемому факту pinned контракта, с source/ref/hash/строками. Behavior/API/SQL/Angular preference требует execution. |
| Учебное состояние | Сохранены objective namespace B1, qualification и reuse совместимого старта. Назначенный путь не называется существующим checkpoint; общая native adapter карта сохранена. |
| История и объёмы | Исторический09 и входящие verification snapshots явно ограничены своими версиями. Активные корневые инструкции ссылаются на канонические индексы, не на устаревшие totals. |

## Согласованные интерфейсы

`prepare --objective <ID>` остаётся необязательным уточнением topic/profile. Смысловой validate получает `--blind --review --key --selection --review-receipt`. Формат evidence и два режима проверки документации полностью описаны в [REVIEW-PROMPT-RU.md](../prompts/REVIEW-PROMPT-RU.md); native binding — в [10-NATIVE-CONTRACT-BOUNDARY.md](../10-NATIVE-CONTRACT-BOUNDARY.md).

Источники преподавания остаются только xiigen-planning main/llm_plan3. Core skills/native references задают правила и входы, не добавляют внешний корпус. RAG состоит из проверенных slices; объём текущей подборки определяется sources.lock и manifest.

## Границы проверки

Сверка документов охватила 00,02,03,04,05,06,07,08,09, MASTER и REVIEW. Дополнительно прочитаны входящая native карта, curriculum index, связанная часть объединённого evidence validator и свежий regression report.

Доказательство `declared_evidence_only` не аутентифицирует провайдера; `provider_identity_authenticated=false` сохраняется. Настоящее CODEX-review обязано проверить семантическую обоснованность режима/source evidence, скрытые дубликаты и качество контраста. Reference-примеры не объявлены принятыми учебными парами.

Нативное обучение, Chinese generation и production-изменения в документной сверке не выполнялись. Выполненная проверка утилиты не регистрируется как успешный learning experiment.
