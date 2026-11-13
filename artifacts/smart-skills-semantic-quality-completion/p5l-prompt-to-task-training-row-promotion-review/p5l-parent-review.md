# P5L parent review: PromptToTask row promotion/readiness

## Вердикт

`PASS_DATASET_READY_SPLIT_NOT_TRAINING`

P5L принят как promotion/readiness slice. Он не обучает модель и не разрешает semantic-quality claim. Он делает одну нужную вещь перед P6: отделяет строки, которые можно использовать как dataset/split candidates, от строк, которые можно реально подать в training.

## Review cycles

1. **Semantic precision review: FAIL -> fixed.** Первичная версия ставила `training_ready=true` всем шести строкам, включая eval/holdout. Это было overclaim. Субагент исправил split semantics: 6 dataset-ready, 2 P6 training inputs, 2 P7 eval candidates, 2 holdout candidates.
2. **Row/count review: PASS.** `prompt-to-task-training-ready-candidates.jsonl` содержит 6 rows; `dataset_ready_for_p6_p7=6`, `used_for_training_in_p6=2`, `used_for_eval_in_p7=2`, `used_for_holdout_eval=2`, eval/holdout with `training_ready=true=0`.
3. **Safety review: PASS.** `p6_training_allowed_by_this_artifact_alone=true=0`, `semantic_quality_claimed=true=0`, `training_allowed=true=0`, `eval_allowed=true=0`. P5L does not run training/eval/checkpoint/export/import.
4. **Readability/state review: PASS.** Artifacts are readable Russian, JSON/JSONL parse, mojibake scan passed, state records parent review requirement and split semantics.

## Accepted scope

- Accepted: P5L promotion/readiness artifacts, split semantics, state update.
- Accepted for next phase: P6 may read only `used_for_training_in_p6=true` rows as training input candidates after this parent acceptance.
- Reserved: eval and holdout rows must stay out of training and be used by later eval/fresh-load gates.
- Not accepted as done: training, checkpoint rewrite, source-disjoint eval PASS, ablation PASS, fresh-load, continue-training, semantic quality.

## Next action

Commit and push P5L. After that, issue P6 bounded subagent work order for PromptToTask training/checkpoint candidate creation using only the 2 training rows and preserving the 2 eval + 2 holdout rows for later evaluation. P6 still cannot claim semantic quality.
