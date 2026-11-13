# XIIGEN: карта усиления карточек — 11 ноября 2026

Изменены описания **11 существующих документов**: 4 core и 7 planning. Их прежние байты сохранены, добавлены датированные уточнения. Большие частично прочитанные core-карточки получили отдельные supplements, а не отметку о полном пересмотре.

## Изменения существующих документов

| Репозиторий | Документ | Вид изменения |
|---|---|---|
| planning | `llm mvp/design to code session/replanning/possibleDifs/33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md` | Датированная вставка после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/27-NEW-E48-MISREAD-WARM-START-AND-MISSING-CHECKPOINT-2026-10-16.md` | Датированная вставка после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/27-MICRO-LOW-BUDGET-TRANSFER-AND-GRAPH-CLOSURE-2026-10-16.md` | Датированная вставка после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md` | Датированная вставка после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md` | Датированная вставка после заголовка |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/33-wiki-training-feature-reuse.md` | Датированное дополнение с указателем после заголовка |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md` | Датированное дополнение с указателем после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/60-COMPOSITE-CAUSAL-HYPOTHESES-VERIFIER-PHRASE-SKILL-DRIFT-2026-10-19.md` | Датированное дополнение с указателем после заголовка |
| planning | `llm mvp/design to code session/replanning/possibleDifs/21-MICRO-PHRASE-CLAUSE-SURVIVAL-EXPERIMENT-2026-10-16.md` | Датированное дополнение с указателем после заголовка |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/15-prompt-flow-retention.md` | Датированная вставка после заголовка |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/106-feedback-and-transition-memory-plan-2026-10-30.md` | Датированная вставка после заголовка |

## Новые адресные документы

| Репозиторий | Документ |
|---|---|
| core | `docs/plans/replanning-possible-diffs/experimets/audit-supplements/2026-11-11/P48-measurement-addendum.md` |
| core | `docs/plans/replanning-possible-diffs/experimets/audit-supplements/2026-11-11/P49-measurement-addendum.md` |
| core | `docs/plans/replanning-possible-diffs/experimets/audit-supplements/2026-11-11/P50-measurement-addendum.md` |
| core | `docs/plans/replanning-possible-diffs/experimets/audit-supplements/2026-11-11/P52-measurement-addendum.md` |
| core | `docs/plans/replanning-possible-diffs/experimets/audit-supplements/2026-11-11/P58-measurement-addendum.md` |
| planning | `llm mvp/design to code session/replanning/possibleDifs/2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md` |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/reviews/2026-11-11-product-transfer-boundaries.md` |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/reviews/2026-11-11-wiki-epoch-evidence-correction.md` |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/acceptance-contract.md` |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/2026-11-11-experiment-transfer-audit.md` |
| core | `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/repair-plan.md` |

## Как ошибки превращены в проверяемые требования

| Причина | Конкретное усиление |
|---|---|
| Лучший score не гарантирует допуск | P54 и соседние композиционные карточки проверяют недопустимого winner и допустимого runner-up через обычного caller |
| Два отчёта повторяют одну неверную формулу | P52 получает независимый gold histogram; P49 — три disjoint группы overlap |
| Валидатор есть, но его отказ не доходит до файла | P48/P49/P50/P52/P58 разделяют measurements, научный исход и внешний terminal; negative path исполняется через host |
| История подменяет текущую цель | Карточка15 назначает typed active revision и совместный checkpoint; старый PairId не выбирает новое тело |
| Неравенство строк подменяет actual provenance | Карточка106 связывает issued answer, verification и conditional Train; duplicate lookup предшествует stale admission |
| Успешная семантика скрывает отменённую оптимизацию | Карточка33 разносит behavior, resource benefit и public gate; стоимость эпох считается поэпохово |
| Несопоставимые знаменатели дают ложный допуск | Карточка109 отменяет общий <5% вывод из mixed3,3%, сохраняя локальную пользу |
| Неполный журнал выдан за всю работу | Full-suite coverage определяется discovery/case identities; P58 main105/Unit56 не переименовывается whole-class/full-flow |

## Сохранение истории и область проверки

Изменения не переписывают product, tests, skills и опубликованные raw. Все новые названия API явно помечены предлагаемыми. Исходные документы, правки, base/new Git blob и SHA256 перечислены в `document-change-manifest.json`. Проверка patch выполняется на отдельных копиях исходных байтов. Это не native тест продукта.

В core большие P48/P49/P50/P52/P58 cards прочитаны адресно; пять новых addendum находятся в `experimets/audit-supplements/2026-11-11/`. P54 и benefit-filter имеют отдельные дополнения в `fixes-based-on-experiments/reviews/`. Planning crosswalk связывает источники с опытами. Общий `acceptance-contract.md` применяет правила к соседним семействам профилактически, без заявления, что в каждом из них найден дефект.

Промежуточные commit: core `0dd69c4a95ed97ac55afd47e24d5cbf012d94202`; planning `f7a05c7dc13a335006caa4cd8ef6695adce27bf0`. Финальная публикация подтверждается отдельной квитанцией после update refs.
