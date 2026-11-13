# P5L evidence card

## Что проверено

Проверены шесть P5H PromptToTask DPO rows против P5I ablation plan и P5K runtime evidence. После ремонта semantics P5L различает dataset readiness и training readiness.

## Итог после ремонта

- Dataset-ready rows: 6
- P6 training input rows: 2
- P7 eval candidate rows: 2
- Holdout candidate rows: 2
- Deferred/rejected rows: 0
- Semantic quality claimed: false
- Training allowed: false
- Eval allowed: false

## Важная граница

Только `train_candidate` строки имеют `training_ready=true` и `used_for_training_in_p6=true`. Eval и holdout строки остаются dataset-ready, но не training-ready, чтобы не разрушить source-disjoint оценку. P5K `not_ready` не считается PASS или FAIL ablation.

## Следующий шаг

Parent review P5L repair. Если parent принимает ремонт, P6 может использовать только P5L train split rows как training inputs; eval/holdout должны остаться для последующей оценки.
