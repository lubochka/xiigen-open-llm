# P36 parent review - eval fixture rows

Вердикт: PASS_AS_NON_TRAINING_FIXTURE_ROWS_PENDING_COMMIT.

P36 принят как набор проверочных строк для будущего runner. Это не запуск eval,
не обучение, не изменение checkpoint и не доказательство semantic quality.

Что принято:

1. 36 fixture rows: 12 для PromptToTask, 12 для OrchestrationReasoning, 12 для
   SkillSelection.
2. 36 dashboard/API scenario rows, по одному сценарию на каждую fixture row.
3. Все fixture rows имеют `split_role=eval_holdout`, `universal_only=true`,
   `training_allowed=false`, `eval_execution_allowed=false`,
   `not_executed_yet=true`.
4. Источники разведены по machine/source family внутри artifact set; P36 не
   создаёт train split и не смешивает train/eval.
5. Remaining gaps явно сохранены: нужны runner input wiring, real eval
   execution, metrics, ablation, fresh-load, continue-training и checkpoint
   manifest instance перед любым quality claim.

Parent validation:

- JSON/JSONL parse: PASS.
- UTF-8/readability scan: PASS.
- Fixture/schema/count scan: PASS.
- No-overclaim/boundary scan: PASS.
- `git diff --check`: PASS.
- model directories diff-clean guard: PASS.

Ограничение: P36 материализует fixture rows как артефакты, но ещё не подключает
их к runner/dashboard production path и не запускает модель.
