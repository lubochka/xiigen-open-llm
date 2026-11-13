# P7F evidence card

## Scope
P7F создает отдельный PromptToTask checkpoint-кандидат из P7D плюс двух новых train-only строк. Финальные model/checkpoint assets не изменялись.

## Safety
- Training on P7 eval/holdout: false.
- Exact reserved signal mapping: false.
- Semantic quality claimed: false.
- Final model overwrite: false.

## Review boundary
Родитель должен проверить строки, source ledger, candidate checkpoint и focused test evidence перед любым commit/push или продолжением P7 rerun.
