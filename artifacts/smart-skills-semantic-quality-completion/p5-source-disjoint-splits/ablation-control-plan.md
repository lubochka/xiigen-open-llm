# P5 Ablation Control Plan

Статус: `ablation_executable_now=false`.

Причина: сейчас нет ни trained model, ни source-disjoint train/eval split. Поэтому P5 только фиксирует обязательные controls для будущей фазы; запуск ablation/eval сейчас запрещён.

## Controls

| Control | Machine | Что должен сломать |
|---|---|---|
| exact prompt lookup | PromptToTask | Запоминание точной prompt-формулировки вместо извлечения task contract. |
| filename/path lookup | PromptToTask, OrchestrationReasoning | Решение по local path или filename вместо переносимой структуры. |
| keyword-only classifier | PromptToTask | Поверхностные prompt keywords вместо permission/validation/artifact/no-claim boundaries. |
| source-title shortcut | PromptToTask | Решение по title/provenance вместо task contract. |
| WordPress/plugin title shortcut | OrchestrationReasoning | Решение по WordPress/plugin provenance вместо phase-boundary reasoning. |
| static phase keyword shortcut | OrchestrationReasoning | Реакция на слова phase/review/commit без проверки порядка gate. |
| exact protocol text lookup | OrchestrationReasoning | Запоминание точного protocol text вместо переносимого phase-boundary поведения. |

## Когда можно запускать

Ablation controls можно исполнять только после появления:

- минимум двух независимых accepted source families для каждой target machine;
- явного train/eval split по source family;
- авторизованного trained/candidate model artifact в будущей фазе;
- held-out fixtures, source family которых отсутствует в training rows.

## No-Claim Boundary

P5 не запускал ablation, eval, training, code edits, tests, build, checkpoint/model rewrite, commit, push или branch switch. Semantic quality не заявлена.
