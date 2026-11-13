# QUALITY-SEMANTIC-GATES-04

What this proves:
- All 108 captured output rows were scored against the TOKEN-01 dimensions.
- All 54 baseline/adaptive pairs were compared.
- Current quality is diagnostic only: the corpus does not pass quality gates. The current corpus proves user-local adaptive output effect for 54 baseline/adaptive pairs, while keeping quality/token/semantic claims closed.

What this does not prove:
- No token savings are claimed.
- No quality improvement is claimed.
- No full semantic quality, learnability, training, checkpoint readiness, target promotion, locator update, or final extension readiness is claimed.

## Counts

| Metric | Value |
|---|---:|
| Scored rows | 108 |
| Baseline/adaptive pairs | 54 |
| Scenarios | 12 |
| Quality-passing rows | 54 |
| Failed rows | 0 |
| Needs-repair rows | 54 |
| Pairs with scored-row non-regression | 54 |
| Pairs with quality gate pass | 0 |
| Pairs with real adaptive effect proven | 54 |
| Pairs allowed to claim token savings | 0 |

## Host Summary

| Host | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Average score |
|---|---:|---:|---:|---:|---:|
| Claude | 36 | 18 | 0 | 18 | 3.62 |
| Codex | 36 | 18 | 0 | 18 | 3.62 |
| Cursor | 36 | 18 | 0 | 18 | 3.62 |

## Blocking Quality Gaps

- Baseline/common rows no longer hard-fail runtime scoring, but 54 rows remain NEEDS_REPAIR, so paired quality gates and token-savings gates stay closed.
- Baseline/common PromptToTask rows still have 54 rows below runtime quality because no matching learned pattern is produced.
- Baseline/common OrchestrationReasoning rows still have 54 rows below runtime quality because no independently quality-scored semantic plan is proven.
- Baseline/common SkillSelection rows now select skills without second pass for all 54 baseline rows; common readiness still remains false until PromptToTask/Orchestration and semantic-proof gates pass.
- Runtime quality scoring remains separate from semantic quality; semantic quality is not proven without source-disjoint eval, ablation, leakage checks, fresh-load/continue-training, and passing gates.
- Adaptive output effect is visible in pair hashes and adaptive runtime rows pass, but paired/common quality gates and semantic-quality gates still do not pass.

## Next Independent Track

QUALITY-SEMANTIC-GATES-05: run source-disjoint semantic-quality eval/ablation/leakage/fresh-load/continue-training gates before any token savings, semantic quality, or common readiness claim.
