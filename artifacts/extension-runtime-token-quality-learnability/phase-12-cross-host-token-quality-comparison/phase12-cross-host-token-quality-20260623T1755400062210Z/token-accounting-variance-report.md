# Phase 12 token accounting and variance

Метод оценки токенов: xiigen-phase12-heuristic-word-punctuation-v1. Это эвристика, а не tokenizer конкретного провайдера.

| Scenario | Host | Baseline total | Adaptive total | Delta | Reduction | Quality block |
|---|---|---:|---:|---:|---|---|
| phase12-heldout-prompt-to-task-01 | Claude | 308 | 276 | -32 | True | False |
| phase12-heldout-prompt-to-task-01 | Cursor | 308 | 276 | -32 | True | False |
| phase12-heldout-prompt-to-task-01 | Codex | 308 | 276 | -32 | True | False |
| phase12-heldout-orchestration-plan-01 | Claude | 303 | 277 | -26 | True | False |
| phase12-heldout-orchestration-plan-01 | Cursor | 303 | 277 | -26 | True | False |
| phase12-heldout-orchestration-plan-01 | Codex | 303 | 277 | -26 | True | False |
| phase12-heldout-skill-selection-01 | Claude | 308 | 281 | -27 | True | False |
| phase12-heldout-skill-selection-01 | Cursor | 308 | 281 | -27 | True | False |
| phase12-heldout-skill-selection-01 | Codex | 308 | 281 | -27 | True | False |

Итог: token reduction в harness-оценке наблюдается у 9 из 9 пар. Принятый token success: True.
