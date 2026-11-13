# QUALITY-PRODUCT-CORE-NO-DETERMINISTIC-SHORTCUT-DIMENSION-REPAIR-01

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
| Quality-passing rows | 108 |
| Failed rows | 0 |
| Needs-repair rows | 0 |
| Rows passing all required dimension scores | 108 |
| Rows with required dimension below pass threshold | 0 |
| Rows with no_deterministic_shortcut below pass threshold | 0 |
| Rows with zero selected skills | 0 |
| Rows with zero requested skills | 0 |
| Rows with required skill selection absent | 0 |
| Pairs with scored-row non-regression | 54 |
| Pairs with quality gate pass | 54 |
| Pairs with real adaptive effect proven | 54 |
| Pairs allowed to claim token savings | 0 |

## Host Summary

| Host | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Zero selected | Zero requested | Required absent | Average score |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Claude | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |
| Codex | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |
| Cursor | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |

## Method Summary

| Method | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Zero selected | Zero requested | Required absent | Average score |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| orchestration-plan | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |
| prompt-to-task | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |
| skill-selection | 36 | 36 | 0 | 0 | 0 | 0 | 0 | 4 |

## Blocking Quality Gaps

- All runtime quality rows pass, and 54 paired runtime quality gates pass; semantic quality, product readiness, and token savings remain separate claims.
- Baseline/common PromptToTask rows pass runtime-row quality, but semantic quality remains unproven.
- Baseline/common OrchestrationReasoning rows pass runtime-row quality, but semantic quality remains unproven.
- Baseline/common SkillSelection rows now select skills without second pass for all 54 baseline rows; common readiness still remains false until PromptToTask/Orchestration and semantic-proof gates pass.
- All required dimension scores meet the runtime pass threshold; semantic/product gates still remain separate.
- Runtime quality scoring remains separate from semantic quality; semantic quality is not proven without source-disjoint eval, ablation, leakage checks, fresh-load/continue-training, and passing gates.
- Adaptive output effect is visible in pair hashes, and 54 paired runtime quality gates pass; token-savings and semantic/product gates remain separate and closed.

## Next Independent Track

QUALITY-PRODUCT-SEMANTIC-PRODUCT-READINESS-EVIDENCE-REPAIR-01: runtime quality rows now pass after no-deterministic-shortcut repair; keep product_ready=false and token_savings_claim_allowed=false until source-disjoint semantic eval, ablation/leakage checks, fresh-load/continue-training probes, live Claude/Cursor/Codex host readiness evidence, and token-savings gates all pass.
