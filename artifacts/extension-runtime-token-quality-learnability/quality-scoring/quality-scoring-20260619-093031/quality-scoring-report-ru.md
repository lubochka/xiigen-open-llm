# QUALITY-03: отложенный quality scoring

Всего: 9 quality rows для 9 существующих baseline/adaptive token pairs.
Сделано: 0/9 quality rows; scoring не запускался.
Сейчас: проверены plan/state, 9 token-pair artifacts, локальный QUALITY-01 rubric и QUALITY-02 eval-only runner. Локальный scorer существует, но он валидирует и агрегирует уже утвержденные reviewer/adjudication score rows; он не должен сам заменять человеческую оценку или model judge.
Осталось: 9/9 quality rows после отдельного bounded work order, который подготовит approved eval-only input manifest, row-specific reviewer/adjudication scores, held-out/source-disjoint/ablation/no-leakage/no-memorization refs и затем запустит runner без external AI/model calls.

Что это доказывает:
- В репозитории есть 9 accounting-only token pairs и локальный non-external eval-only scoring helper.
- Для текущей фазы quality scoring безопасно отложен, потому что token accounting не равно quality.

Что это не доказывает:
- Нет quality improvement, quality non-regression, semantic quality, learnability, token savings, runtime readiness, target promotion, locator update или product readiness.

Точные недостающие механизмы:
- approved_eval_input_manifest_ref для каждой из 9 пар;
- row-specific reviewer/adjudication score inputs по слоям PromptToTask, OrchestrationReasoning, SkillSelection, FinalUsefulness;
- reviewer notes, inter-rater status или explicit single-reviewer limitation;
- held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization refs;
- runner-ready JSONL rows with approved shared refs and local_private_eval_only scope.

Evidence refs:
- Packet: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-093031/quality-scoring-defer-packet.json
- Matrix: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-093031/quality-pair-readiness-matrix.json
- State: docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json
- Rubric: artifacts/extension-runtime-token-quality-learnability/quality-01-eval-prompt-pack-rubric-prep/quality-01-eval-prompt-pack-rubric-prep-20260618-230544/quality-rubric.json
- Runner contract: artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-231757/eval-only-runner-contract.json
- Runner code: 43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs

Next bounded work order:
- QUALITY-03-APPROVED-SCORING-INPUT-MANIFEST-AND-CONTROLS-01: Create approved eval-only input JSONL for the 9 current pair_ids with row-specific reviewer/adjudication scores, reviewer notes, redacted baseline/adaptive refs, held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization refs, then run Quality02EvalOnlyRunner with --synthetic-fixture-only false and --actual-approved-eval-executed true.

Confirmations:
- No external AI/model calls.
- No training, DPO authoring, checkpoint/model/locator mutation.
- No code/test/doc/skill/AGENTS/CLAUDE/plan edits.
- No git stage/commit/push.
