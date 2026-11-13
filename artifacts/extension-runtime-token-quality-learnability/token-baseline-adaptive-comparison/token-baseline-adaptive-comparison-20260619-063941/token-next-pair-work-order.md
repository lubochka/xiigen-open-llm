# Next bounded work order

Work order: TOKEN-PAIR-CLAUDE-ORCHESTRATION-PLAN-BASELINE-ADAPTIVE-TRACE-CAPTURE-OR-DEFER-05

Goal: capture or defer same-contract baseline_common_no_adaptive and adaptive_user_local_target trace refs for TOKEN-PAIR-CLAUDE-ORCHESTRATION-PLAN-001 / TOKEN-SCENARIO-ORCHESTRATION-PLAN-001 after the adaptive target locator/fresh-load evidence packet is available.

Allowed only if explicitly authorized by the next parent work order:
- create trace evidence artifacts in a new timestamped folder;
- use no external AI/model call, no host UI launch, no training/DPO/quality scoring, no checkpoint/model/locator/target mutation;
- do not edit source/test/docs/skills/AGENTS/CLAUDE;
- no git add/commit/push.

Completion rule: create `token-pair-002.json` and `token-pair-002.md` only if both baseline and adaptive trace refs are under the same pair contract. Otherwise defer with exact missing evidence.

Parent will perform commit/push after review: true.
