# Фаза 04: PromptToTask universal task contract

Решение: PHASE_04_SLICE_READY_FOR_REVIEW

Статус: Фаза 04/14, шаг 04.06/04.08, статус READY_FOR_REVIEW

## Счетчики

- Всего подшагов 04.02-04.06: 5
- Сделано: 5
- Осталось: 0
- Провалено: 0
- Положительные сценарии: 5
- Негативные authority-сценарии: 5
- Проваленные eval rows: 0

## Что доказано

- PromptToTask строит структурированный task contract из текущего raw prompt на source-disjoint положительных сценариях.
- QUOTE/attachment/prior assistant/state next_action/old approval остаются evidence-only без текущей unquoted реактивации.
- Source id, family id, pair id и prompt id не меняют решение в leakage ablation.
- Общий checkpoint загружался и не мутировал.

## Что не доказано

- Full semantic quality не заявлялась.
- Пользовательская adaptive модель в этом срезе не обучалась.
- Claude/Cursor/Codex host UI/runtime readiness не проверялась.

## Граница baseline

Использован прозрачный nearest-signal baseline внутри PromptToTaskTaskPatternUnit. Он помечен как baseline/internal и не является semantic quality claim.
