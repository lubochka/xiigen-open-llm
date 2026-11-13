# 156. Покрытие адресных поправок после Q003

Основание — [154](codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), действия — [155](codex-155-postq003-addressed-repair-plan-2026-11-03.md), общий договор — [50](../experimets/codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md). Изменены документы и добавлены свидетельства аудита; нативная реализация и её приёмка остаются UNRUN. Исторические тела документов и исходные raw сохранены; новые блоки явно обозначены POSTQ003.

## Матрица изменений

В 20 экспериментальных карточках есть собственные факт/область, действие, positive/negative и обязательная часть передаваемого поручения. В 13 карточках с разделом «Промпт и будущие свидетельства» поправка вставлена также непосредственно в него; Markdown-каркас отчёта не изменялся. Для семи остальных отдельный передаваемый блок находится в начале карточки. Восемь договоров переноса получают конкретные случаи, четыре карточки планирования — соответствующие ограничения. Индекс, навигация,150/49 и handover синхронизированы отдельно.

| Вид | Изменённый адрес | Названная проверка |
|---|---|---|
| experiment-card | [P01-trainable-recipient.md](../experimets/cards/P01-trainable-recipient.md) | `Q002-RECOVERY-AND-IDENTITY` |
| experiment-card | [P02-warm-base.md](../experimets/cards/P02-warm-base.md) | `Q003-REPAIR-IS-NOT-LEARNING` |
| experiment-card | [P03-historical-score-lineage.md](../experimets/cards/P03-historical-score-lineage.md) | `P03-FINAL-STATE-AND-METRIC` |
| experiment-card | [P04-preference-contrast.md](../experimets/cards/P04-preference-contrast.md) | `P04-EXACT-STATE-BEFORE-CONTRAST` |
| experiment-card | [P05-response-length-objective.md](../experimets/cards/P05-response-length-objective.md) | `P05-OBJECTIVE-STATE-PAIRING` |
| experiment-card | [P06-teaching-vector-adamw.md](../experimets/cards/P06-teaching-vector-adamw.md) | `P06-STATE-BEFORE-OPTIMIZER-PROOF` |
| experiment-card | [P11-autograd-reachability.md](../experimets/cards/P11-autograd-reachability.md) | `P11-SHARED-S1-MANIFEST` |
| experiment-card | [P12-optimizer-boundaries.md](../experimets/cards/P12-optimizer-boundaries.md) | `P12-SHARED-S1-AND-COUNTER` |
| experiment-card | [P18-state-identity.md](../experimets/cards/P18-state-identity.md) | `LFS-CONTENT-IDENTITY` |
| experiment-card | [P25-cold-restore.md](../experimets/cards/P25-cold-restore.md) | `COLD-RESTORE-AFTER-MATERIALIZATION` |
| experiment-card | [SAVED-STATE-PANEL-0407-01.md](../experimets/cards/SAVED-STATE-PANEL-0407-01.md) | `P126-DONOR-AND-TERMINAL` |
| experiment-card | [PARENT-ADOPTION-01.md](../experimets/cards/PARENT-ADOPTION-01.md) | `ADOPTION-USES-THE-SAVED-BYTES` |
| experiment-card | [KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | `P127-EXTERNAL-HISTORICAL-AUTHORITY` |
| experiment-card | [P131-negation-training-contrast.md](../experimets/cards/P131-negation-training-contrast.md) | `P131-EXACT-EVIDENCE-ROOT` |
| experiment-card | [P68-pretrain-pipeline.md](../experimets/cards/P68-pretrain-pipeline.md) | `P6801-CONFLICT-CONTENT` |
| experiment-card | [P69-graphrag-context.md](../experimets/cards/P69-graphrag-context.md) | `P6901-CONFLICT-CONTENT` |
| experiment-card | [P75-sampler-residual.md](../experimets/cards/P75-sampler-residual.md) | `P7501-CONFLICT-CONTENT` |
| experiment-card | [P76-utf8-boundary.md](../experimets/cards/P76-utf8-boundary.md) | `P7601-CONFLICT-CONTENT` |
| experiment-card | [P78-loss-plateau.md](../experimets/cards/P78-loss-plateau.md) | `P7801-CONFLICT-CONTENT` |
| experiment-card | [P81-input-conditioning.md](../experimets/cards/P81-input-conditioning.md) | `P8101-CONFLICT-CONTENT` |
| transfer-contract | [02-learning-state-and-fixtures.md](02-learning-state-and-fixtures.md) | `DONOR-IS-THE-MATERIALIZED-ORIGINAL` |
| transfer-contract | [03-test-and-execution-contract.md](03-test-and-execution-contract.md) | `PUBLISHED-IDENTITY-IS-NOT-NATIVE-PASS` |
| transfer-contract | [04-evidence-and-novelty.md](04-evidence-and-novelty.md) | `EXTERNAL-AUTHORITY-AND-METRIC-PHASE` |
| transfer-contract | [101-persistent-state-bundle-contract-2026-10-30.md](101-persistent-state-bundle-contract-2026-10-30.md) | `SERIALIZED-COMPONENTS-BEFORE-RESTORE` |
| transfer-contract | [103-transfer-queue-commit-mapping-2026-10-30.md](103-transfer-queue-commit-mapping-2026-10-30.md) | `ACCEPTED-CODE-VERSUS-CURRENT-DONOR` |
| transfer-contract | [21-sft-optimizer-continuity.md](21-sft-optimizer-continuity.md) | `RESTORE-IS-NOT-A-REPEATED-UPDATE` |
| transfer-contract | [28-dpo-tokenizer-snapshot.md](28-dpo-tokenizer-snapshot.md) | `TOKENIZER-AND-STATE-ARE-ONE-ASSIGNMENT` |
| transfer-contract | [13-dpo-reference-cache.md](13-dpo-reference-cache.md) | `REFERENCE-PAYLOAD-BEFORE-CACHE-CLAIM` |
| active-repair-plan | [codex-150-post131-addressed-repair-plan-2026-11-03.md](codex-150-post131-addressed-repair-plan-2026-11-03.md) | `P131-INPUTROOT / P127-AUTHORITY` |
| executor-contract | [codex-49-post131-output-binding-and-test-coverage-2026-11-03.md](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) | `P131-INPUTROOT / P127-AUTHORITY` |
| live-index | [00-execution-index.md](../experimets/cards/00-execution-index.md) | `Q002-RECOVERY / Q004-REVIEW / ALL-SAVED-DONORS` |
| navigation | [README.md](README.md) | `EXISTING-QUEUE-AND-DONORS` |
| navigation | [01-registry.md](01-registry.md) | `EXISTING-QUEUE-AND-DONORS` |
| handover | [SESSION-HANDOVER-2026-11-03.md](../SESSION-HANDOVER-2026-11-03.md) | `CONCURRENT-WORK-AND-SCOPE` |
| planning-card | [00-TRAINING-STATE-AND-EXPERIMENT-AUDIT-2026-10-20.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/00-TRAINING-STATE-AND-EXPERIMENT-AUDIT-2026-10-20.md) | `AUDIT-LAYERS` |
| planning-card | [02-DPO-DATA-OBJECTIVE-GRADIENT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/02-DPO-DATA-OBJECTIVE-GRADIENT.md) | `DPO-EXACT-STATE-AND-METRIC` |
| planning-card | [03-LORA-TOKENIZATION-VECTOR-SPACE.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/03-LORA-TOKENIZATION-VECTOR-SPACE.md) | `COORDINATE-AND-PAYLOAD-IDENTITY` |
| planning-card | [04-BASE-LORA-CHECKPOINT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/04-BASE-LORA-CHECKPOINT.md) | `PUBLISHED-BYTES-AND-COLD-LOAD` |
| planning-navigation | [README.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/README.md) | `CANONICAL-CORE-LINKS` |

## Причина → различающий случай → ожидаемый результат

| Причина | Положительный свидетель | Отрицательный свидетель | Приёмка ещё нужна |
|---|---|---|---|
| Внешний LFS pointer скрывает конфликт | Подлинный materialized payload | Правильный pointer к 289-байтовому конфликтному тексту | Native Load и обычный consumer существующего владельца |
| Shared S1 расходится с manifest | Исходный S1 с собственным manifest | Пересоздание manifest под повреждённый OID | Восстановление bytes и проверка назначенного reuse |
| Квитанция подменяет final state | Фактические применимые компоненты | Hash/count-only receipt вместо компонента | Подлинный архив и фактический cold restore |
| Исторический пакет сам задаёт свой hash | Внешний manifest из закреплённого commit | Route+steps+summary+local hashes под прежним attempt | Native finalizer/outer P127; synthetic mode отдельно |
| P131 root расположен уровнем выше | Continuation/evidence | Continuation как InputRoot | Явный layout failure/nonzero без вызова модели |
| Устаревший partial или смешанные метрики | Тот же attempt/arm/phase и построчный gold | DPO0 после восьми receipts; pre-update loss как final | Документная сверка выполнена; новый опыт не нужен |

## Что намеренно не переобъявлено выполненным

Проверка и адресное восстановление оставшихся binaries, поиск подлинного final archive P03, нативный ремонт P127/P131 и продуктовые Q01/R128/R117 остаются задачами прежних владельцев. Свежо проверены offline: 49 original/current pointers, 19 content identities, 42 исходных файла и строки панелей P01/P02/P03. Историческое обучение и нативные опыты не повторялись. Наличие этого текста не выполняет приёмочные тесты.

## Контроль сохранения и независимое ревью

Все изменённые старые файлы построены поверх точных Git bytes baseline. Основной приём — вставка маркированного блока; целевые замены относятся к строкам Q002/Q004, InputRoot P131 и authority P127. Старые каркасы отчётов, scientific payload, бюджеты и gold не переписывались. Перечень изменений и before/after Git blob SHA хранится в evidence. Независимый reviewer проверяет смысловые границы, передаваемые задания и весь diff; его вывод фиксируется отдельно. Неисправленное замечание не скрывается словом PASS.

Параллельные153/P115/P127 и review579d Q004 включены в baseline; чужие изменения сохранены. Публикация выполняется поверх свежего HEAD с проверкой expected SHA. Фактический новый commit и readback сообщаются в итоговом ответе. Исторические raw-ссылки и исходный pin7ce не подменяются SHA публикации.

## Регистрация новых документов

Все наши новые публикуемые файлы имеют префикс `codex-` по `AGENTS.md`, OP-3D-1. Существующие и параллельные файлы не переименованы. Навигация обоих README обновлена. **RAG TODO:** отдельная локальная конвенция регистрации `documents/index` в выбранных каталогах не установлена; прямого поручения на такую запись нет. OP-3D-1 требует не выдумывать механизм и не выполнять RAG-запись без явного назначения. Такая индексация не заявляется выполненной; это не отменяет внесённых в действующие карточки изменений.
