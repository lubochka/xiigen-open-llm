# P38A parent review - implementation sub-agent git overrun repair

Вердикт: PASS_AS_GOVERNANCE_REPAIR_PENDING_COMMIT.

P38A принят как ремонт второй, более глубокой лазейки: обычный
artifact/code/content/data/report/plan/governance sub-agent больше не должен
иметь git surface вообще. Он создаёт артефакты и возвращает report; parent
делает review и только потом отдельно commit/push.

Что проверено и принято:

1. Normal artifact-producing work order теперь должен иметь
   `allowed_git_commands=NONE`.
2. Commit/push может быть только отдельным commit-only work order после parent
   review.
3. Commit-only work order не имеет права менять артефакты, код, docs, state,
   model files или checkpoints.
4. Sub-agent, который делает commit/push или emit git directives без exact
   authority, disqualified from further write work in current phase family until
   parent repair.
5. Parent review обязан читать sub-agent notification на claims о commit, push,
   staging, branch/ref actions и git directives. Чистый worktree сам по себе
   больше не доказывает чистый процесс.
6. P38 artifact записан как independently validated artifact with process
   violation, а не как clean sub-agent pass.

Parent validation:

- Rule text search in AGENTS/CLAUDE/skills: PASS.
- JSON/JSONL parse: PASS.
- UTF-8/readability scan: PASS.
- `git diff --check`: PASS.
- model directories diff-clean guard: PASS.

Ограничение: P38A не меняет product code, не запускает build/tests/training/eval
и не трогает model files. Это governance repair.
