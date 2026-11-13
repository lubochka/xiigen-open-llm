# TOKEN-PAIR-CLAUDE-ORCHESTRATION-PLAN-ADAPTIVE-MECHANISM-OR-DEFER-02

## Goal

Repair or defer the missing adaptive mechanism for TOKEN-PAIR-CLAUDE-ORCHESTRATION-PLAN-001 / TOKEN-SCENARIO-ORCHESTRATION-PLAN-001 without external AI/model calls, host UI launch, training, DPO, quality scoring, checkpoint/model/locator mutation, source edits, or git writes.

## Current input

- Complete token pairs remain 1/9.
- Pair 002 is deferred in artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-060208/token-pair-002-defer.json.
- Missing mechanism: no proven adaptive_user_local_target row for Claude orchestration-plan with exact target locator/model manifest/user-local checkpoint/fresh-load refs tied to this pair id.
- Claims remain closed: token_savings_claim_allowed=false and quality_non_regression_status=NOT_RUN.

## Allowed write scope

- A new timestamped folder under artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/
- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json only

## Allowed work

1. Read this defer packet, TOKEN-01/TOKEN-02/TOKEN-03 refs, pair 001 artifacts and current state.
2. Prove whether local core/adapter can force both baseline_common_no_adaptive and adaptive_user_local_target for Claude orchestration-plan with the same pair id, same prompt/method/debug/evidence/accounting/redaction contract, and no mutations.
3. If both sides are proven, create token-pair-002.json and .md plus trace/count refs.
4. If adaptive_user_local_target cannot be proven, return a refreshed defer packet naming the exact missing target locator/model/fresh-load mechanism.
5. Keep all token/product savings, quality, learnability, runtime readiness, target promotion, locator update, final readiness and product readiness claims false.

## Forbidden

No external AI/model calls. No host UI launch. No training/DPO/quality scoring/checkpoint/model/locator/target mutation. No source/test/docs/skills/AGENTS/CLAUDE edits. No git add/commit/push.

## Verification

Parse all created/updated JSON/JSONL plus state JSON; run git diff --check on the new folder and state; scan for question-mark placeholder; run no-overclaim and allowed-path guards; run textual trailing whitespace/BOM scan count 0 and no-index whitespace simulation if possible.
