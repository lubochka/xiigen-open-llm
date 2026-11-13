# P28 - вердикт по авторизации SkillSelection training

## Короткий ответ

Training сейчас не авторизован. P28 не запускал training, eval, checkpoint mutation, promotion или замену common model.

Причина простая: текущий work order прямо говорит сначала проверить авторизацию и считать training запрещенным, если active plan/state/current instruction не разрешают его явно. Такого явного разрешения нет. State держит `training_authorized=false`, `training_allowed=false`, `eval_allowed=false`. P27 ledger говорит `trainingAllowed=false`, `candidateTrainingAllowed=false`, `commonTrainingAllowed=false`, `notUsedForTrainingYet=true`, а parent review принял P27 только как checked repair dataset candidate.

## Цепочка полномочий

| Источник | Что говорит | Вывод для training |
|---|---|---|
| Current P28 work order | Не commit/push, сначала authorization verification, no training unless explicitly permitted. | Training не начинать. |
| Quote `"Продолжай исполнение плана через суб агентов"` | Разрешает продолжать план через суб-агентов, но сам по себе не разрешает training при более строгих gates. | Дает право на P28 verification, не на training. |
| AGENTS.md | Model work требует manifest, checkpoint refs, export/import, fresh-load, continue-training; candidates are not pre-approved material. | Нужна отдельная gated training slice. |
| Active state | `training_authorized=false`, `training_allowed=false`, `eval_allowed=false`. | Training/eval заблокированы. |
| Active plan | Требует source-disjoint eval, ablations, checkpoint rewrite, fresh-load, continue-training и no-claim firewall. | План задает gates, но P28 training не разрешает. |
| P27 authorization ledger | `candidateTrainingAllowed=false`, `notUsedForTrainingYet=true`, `nextTrainingSliceMustBeSeparate=true`. | P27 rows пока только candidate. |
| P27 parent review | P27 не training set, не common training, не checkpoint mutation и не promotion. | Нужен отдельный будущий slice. |

## Что нужно, чтобы training можно было запускать позже

1. Свежий bounded work order от parent/architect, который явно разрешает candidate-only SkillSelection training/eval.
2. State должен разрешать named candidate training/eval slice.
3. До training нужен source-disjoint split/family ledger для repair train/eval rows.
4. Нужен training batch card: local-private, candidate-only, `commonTrainingAllowed=false`, no common replacement.
5. В training slice должны быть gates: fresh-load, continue-training from existing checkpoint, export/import, ablation, candidate-order permutation, source-id/path/filename/pair-id leakage.
6. Dashboard/runtime evidence может быть только evidence, не semantic quality proof.
7. Promotion и common model replacement остаются отдельными parent review gates.

## Следующий безопасный шаг

Следующий safe non-training slice: P29 SkillSelection repair source-disjoint training batch gate design. Он должен подготовить split design, authorization matrix, leakage controls и batch card для будущего candidate-only training/eval slice, но сам training не запускать и модели не менять.

## No-overclaim

P28 является только authorization/work-order slice. Он не обучает модель, не создает checkpoint, не продвигает candidate, не заменяет common model и не заявляет semantic quality или SkillSelection completion.
