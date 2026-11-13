# TOKEN-04 Quality Scoring Full Hosts

What this proves:
- All 108 captured real host output rows were scored against the TOKEN-01 dimensions.
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
| Failed rows | 108 |
| Pairs with scored-row non-regression | 54 |
| Pairs with quality gate pass | 0 |
| Pairs with real adaptive effect proven | 0 |
| Pairs allowed to claim token savings | 0 |

## Host Summary

| Host | Rows | PASS rows | FAIL rows | Average score |
|---|---:|---:|---:|---:|
| Claude | 36 | 0 | 36 | 2 |
| Codex | 36 | 0 | 36 | 2 |
| Cursor | 36 | 0 | 36 | 2 |

## Blocking Quality Gaps

- PromptToTask rows report loaded_state_no_matching_learned_pattern rather than scenario-specific universal task quality.
- OrchestrationReasoning rows report not_trained_no_learned_guide_state or manifest/checkpoint mismatch.
- SkillSelection rows return empty or second-pass decisions rather than final semantic skill selection.
- Adaptive rows do not prove real user-local adaptive model effect; many outputs are identical to baseline.

## Next Independent Track

QUALITY-GAP-REPAIR-01: repair the quality blockers before token savings or semantic-quality claims. Start with OrchestrationReasoning manifest/checkpoint mismatch, PromptToTask no-matching-pattern coverage, SkillSelection empty/second-pass output, and real user-local adaptive model effect; then rerun TOKEN-04 scoring.
