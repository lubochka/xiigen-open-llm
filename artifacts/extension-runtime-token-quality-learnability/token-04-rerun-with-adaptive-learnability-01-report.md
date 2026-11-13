# TOKEN-04-RERUN-WITH-ADAPTIVE-LEARNABILITY-01

What this proves:
- All 108 captured output rows were scored against the TOKEN-01 dimensions.
- All 54 baseline/adaptive pairs were compared.
- Current quality is diagnostic only: the corpus does not pass quality gates and does not prove adaptive model effect.

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
| Failed rows | 96 |
| Needs-repair rows | 12 |
| Pairs with scored-row non-regression | 54 |
| Pairs with quality gate pass | 0 |
| Pairs with real adaptive effect proven | 0 |
| Pairs allowed to claim token savings | 0 |

## Host Summary

| Host | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Average score |
|---|---:|---:|---:|---:|---:|
| Claude | 36 | 0 | 32 | 4 | 2.53 |
| Codex | 36 | 0 | 32 | 4 | 2.53 |
| Cursor | 36 | 0 | 32 | 4 | 2.53 |

## Blocking Quality Gaps

- PromptToTask rows report loaded_state_no_matching_learned_pattern rather than scenario-specific universal task quality.
- OrchestrationReasoning rows do not pass independent scenario-quality scoring; loaded/no-match baseline status is not semantic orchestration quality.
- SkillSelection rows may select skills, but semantic skill-selection quality is still not proven by held-out/source-disjoint gates.
- Adaptive rows do not prove real user-local adaptive model effect; many outputs are identical to baseline.

## Next Independent Track

ADAPTIVE-EFFECT-GAP-02: staged user assets load in adaptive mode but do not change TOKEN-01 extension outputs; repair scenario-aligned user-local adaptive DPO coverage and rerun fresh capture/scoring before token savings, quality improvement, semantic quality, or full learnability claims.
