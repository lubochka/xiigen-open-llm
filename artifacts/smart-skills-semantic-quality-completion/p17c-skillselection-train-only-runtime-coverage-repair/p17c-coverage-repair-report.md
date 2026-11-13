# P17C: train-only runtime coverage repair material

## Краткий вывод

P17B доказал реальный runtime path для SkillSelection, но четыре source-disjoint positive rows были выбраны как `claim_verification` вместо ожидаемых навыков. P17C не обучает модель и не меняет checkpoint. Этот пакет готовит безопасный train-side repair material, чтобы родитель мог решить, можно ли открыть следующий rebuild/rerun этап.

## Что провалилось в P17B

| Failed row | Unit | Expected | Selected | Repair material |
|---|---|---|---|---|
| `P16B-SS-PROMPT-003` | `prompt_understanding` | `planning` | `claim_verification` | `P17C-SS-PROMPT-PLAN-001`, `P17C-SS-PROMPT-PLAN-002` |
| `P16B-SS-TASK-002` | `task_understanding` | `planning_dpo_authoring` | `claim_verification` | `P17C-SS-TASK-DPO-001` |
| `P16B-SS-TASK-003` | `task_understanding` | `trainable_architecture_decision` | `claim_verification` | `P17C-SS-TASK-ARCH-001`, `P17C-SS-TASK-ARCH-002` |
| `P16B-SS-CHAIN-003` | `runtime_chain` | `trainable_architecture_decision` | `claim_verification` | `P17C-SS-CHAIN-ARCH-001` |

## Почему это не утечка eval/holdout

Failed rows перечислены только в mapping/quarantine artifacts. Они не являются source rows для candidate material. Candidate rows используют локальные train-side/adaptive источники: phase planning, planning session startup, DPO triple format, trainable architecture decision and implementation integrity. Для trainable architecture есть риск broad source-family overlap, поэтому эти строки остаются `candidate_training_ready=false` и `common_training_ready=false`; их можно использовать только после parent review как adaptive candidate material.

## Что остается запрещено

- Нельзя обучать на `P16B-SS-PROMPT-003`, `P16B-SS-TASK-002`, `P16B-SS-TASK-003`, `P16B-SS-CHAIN-003`.
- Нельзя использовать P17B runtime result rows как training rows.
- Candidate position, source id, path/title, exact prompt and nearest-only signals remain shortcut controls, not selector authority.
- No semantic quality, SkillSelection readiness, full API readiness, trained-model readiness or final-model readiness is claimed.

## Следующий ограниченный этап

Если родитель принимает P17C material, следующий bounded phase: rebuild a SkillSelection candidate checkpoint from accepted adaptive train-side rows, then rerun runtime selector evidence.
