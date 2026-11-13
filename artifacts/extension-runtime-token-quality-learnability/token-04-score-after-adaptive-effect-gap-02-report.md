# TOKEN-04-SCORE-AFTER-ADAPTIVE-EFFECT-GAP-02

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
| Quality-passing rows | 0 |
| Failed rows | 54 |
| Needs-repair rows | 54 |
| Pairs with scored-row non-regression | 54 |
| Pairs with quality gate pass | 0 |
| Pairs with real adaptive effect proven | 54 |
| Pairs allowed to claim token savings | 0 |

## Host Summary

| Host | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Average score |
|---|---:|---:|---:|---:|---:|
| Claude | 36 | 0 | 18 | 18 | 2.75 |
| Codex | 36 | 0 | 18 | 18 | 2.75 |
| Cursor | 36 | 0 | 18 | 18 | 2.75 |

## Blocking Quality Gaps

- PromptToTask rows report loaded_state_no_matching_learned_pattern rather than scenario-specific universal task quality.
- OrchestrationReasoning rows do not pass independent scenario-quality scoring; loaded/no-match baseline status is not semantic orchestration quality.
- SkillSelection rows may select skills, but semantic skill-selection quality is still not proven by held-out/source-disjoint gates.
- Adaptive output effect is now visible in pair hashes, but quality gates and semantic-quality gates still do not pass.

## Next Independent Track

QUALITY-SEMANTIC-GATES-03: adaptive effect is now visible, but quality gates and semantic-quality gates must still be repaired/evaluated before token savings or semantic claims.
