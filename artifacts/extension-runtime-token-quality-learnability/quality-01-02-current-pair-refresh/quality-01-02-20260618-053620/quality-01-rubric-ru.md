# QUALITY-01 rubric for current TOKEN-03/TOKEN-04 pairs

Run: `quality-01-02-20260618-053620`

Область: свежая human-readable rubric для 54 handler-level pair rows из TOKEN-03 `token-03-20260618-040021` и TOKEN-04 `token-04-20260618-044550`. Это evidence для следующего QUALITY-03 rerun, а не заявление о token savings, quality improvement, semantic quality, learnability, live-host savings или final readiness.

## Слои оценки

| Слой | Что оценивается | PASS-порог | Важное ограничение |
|---|---|---:|---|
| PromptToTask | Насколько запрос превращён в bounded task с deliverables, constraints и evidence boundary | 4/5 | Generic status без сценарных требований не проходит как полный слой |
| OrchestrationReasoning | Насколько задача превращена в универсальный план с gates и evidence refs | 4/5 | Handler-level trace не заменяет live host runtime |
| SkillSelection | Насколько выбранные skills покрывают смысл задачи, а не candidate order/hash/path | 4/5 | Confidence является supporting signal, не semantic proof |
| Final usefulness | Насколько ответ полезен reviewer-у и не overclaim-ит | 4/5 | Краткий generic answer может быть readable, но остается limited |

## Общие hard rules

- Scorer разрешён только как deterministic evaluation helper.
- Любой overclaim, external AI task solving, training, model/checkpoint/locator mutation или runtime substitution закрывает PASS.
- Token delta не является quality evidence.
- QSG28 staged/test-only target остается explicit limitation.
- OrchestrationReasoning common fallback остается explicit limitation.
- Source-disjoint/no-leakage/no-memorization PASS невозможен без source_ref/doc_id/license/train-vs-held-out split для текущего pack.

## Что это доказывает

- Есть прозрачная rubric для PromptToTask, OrchestrationReasoning, SkillSelection и final usefulness на текущих 54 парах.

## Что это не доказывает

- Не доказывает token savings, quality improvement, semantic quality, learnability, live Claude/Cursor/Codex runtime или final readiness.

## Следующий независимый track

- Rerun QUALITY-03 scoring over current pair set using these QUALITY-01/QUALITY-02 artifacts, with claims still closed unless QUALITY-03 gates pass honestly.
