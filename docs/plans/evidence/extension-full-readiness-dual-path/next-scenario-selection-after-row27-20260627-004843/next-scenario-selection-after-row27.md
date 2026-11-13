# Next scenario selection after Row27

claim_scope: next safe planning/orchestration selection after Row27 scoped commit
evidence_scope: read `product-readiness-scenario-board.md/json`, `product-readiness-action-plan.md/json`, and `phase-14-user-visible-status-dashboard.md`
count_claimed: total 28; scoped accepted 13; product-ready full pass 0; remaining 15
independent_evidence_count: 3 planning/status artifact groups read in this turn
subagent_id_or_NONE: Curie the 2nd / 019f05e6-8f45-75c1-9c17-86b49596c371
downgrade_needed: yes - selection supports only scenario repair/prep, not execution or readiness

## Decision

Status: `NEEDS_SCENARIO_REPAIR`.

Next selected scenario/subphase: Row28 `frdp-p01-codex-locator-update-reload` scenario-prep repair only.

This is not runtime execution. It is the smallest independent planning repair that can proceed now without fresh Luba approval, because the current instruction forbids product/runtime/UI/tests/eval/heldout/training and Row23 remains blocked by missing approved input path.

## Counts

Done: 13/28 scoped evidence accepted.

Current: Row28 scenario-prep repair selected as the next safe bounded work item.

Remaining: 15 scenario rows still open.

Product-ready full pass: 0/28.

Full joined pass: 0.

Readiness claims opened: 0.

## Why Row28 Is Safe

Row23 `frdp-p01-codex-semantic-quality-heldout` is not safe to execute because the latest accepted status is `BLOCKED_NO_APPROVED_INPUT_PATH`. The missing fields are exactly one approved safe Row23 input/path with provenance, approval quote, category, evidence refs, forbidden actions, and claim locks.

Row24 `frdp-p01-codex-learnability-training-continue` is not selected because the current instruction says Row24 should not be used if dependent, and the current turn forbids training/eval/runtime work.

Row28 is safe only as scenario repair/prep because it is independent of Row23 semantic-quality input, does not require training/eval/heldout, and can be reduced to a written dry-run locator scenario script with no locator mutation, no production update, no host/UI execution, no tests, and no readiness claim.

## What This Proves

This selection proves only that a bounded next work item exists: Row28 scenario-prep repair for a future non-mutating locator dry-run proof.

It can produce a human-readable work order and scenario-prep artifact for later review.

## What This Does Not Prove

It does not prove product readiness, Codex readiness, host/UI readiness, locator update, production locator mutation, checkpoint promotion, semantic quality, token savings, training, lifecycle readiness, or a full joined pass.

It does not execute Row28. It does not change board counts. It does not authorize commit or push.

## Exact Next Sub-Agent Work Order

Role: bounded Row28 scenario-prep repair sub-agent, max 10 minutes.

Current authority quote: "Ты bounded next-scenario selector after Row27 commit в C:\Projects\llm_mvp_core. Planning/orchestration only, max 10 minutes."

Task: create a human-readable Row28 scenario-prep packet for `frdp-p01-codex-locator-update-reload` that defines a future non-mutating dry-run locator diff/fresh-load/rollback evidence path without executing it.

Allowed reads:
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-14-user-visible-status-dashboard.md`
- Row27 evidence folder only for adjacent checkpoint/rollback context: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332/`

Allowed write scope:
- Create `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-locator-update-reload/row28-scenario-prep-<timestamp>/`
- Write only `row28-scenario-prep.md` and `row28-scenario-prep.json` there.

Required content:
- exact Row28 purpose: locator source of truth, dry-run locator diff, fresh-load plan, rollback locator plan, and expected blocker statuses;
- explicit non-execution boundary: no runtime, no UI, no tests, no eval, no heldout, no training, no production locator mutation, no checkpoint promotion;
- exact evidence filenames that a later executor would produce if separately authorized;
- exact pass/fail/blocked statuses for future execution;
- claim locks: `product_readiness_claim=false`, `formal_joined_pass=false`, `host_ui_readiness_claim=false`, `locator_update_claim=false`;
- statement that Row27 proves only temp/staging checkpoint reload and rollback, not locator update or product readiness;
- statement that Row23 remains blocked and Row24 is not selected.

Forbidden:
- no code edits;
- no AGENTS/CLAUDE/skills edits;
- no board/action-plan/dashboard edits;
- no product/runtime/UI/tests/eval/heldout/training;
- no locator/checkpoint mutation;
- no commit/push/stage.

Expected decision status from that sub-agent: `NEEDS_SCENARIO_REPAIR` if prep is written; `TRUE_HUMAN_DECISION_REQUIRED` only if the scenario-prep cannot identify locator source-of-truth fields without Luba-provided material.

Commit/push boundary owner: parent only after later explicit authorization; no git action in this work order.

parent_will_perform_commit_push_after_review=true.

## Human Decision Check

A true human decision is required for Row23 execution, because approved Row23 input/path is missing.

A true human decision is not required for the selected Row28 scenario-prep repair, because it is planning-only and preserves all non-execution and non-readiness boundaries.
