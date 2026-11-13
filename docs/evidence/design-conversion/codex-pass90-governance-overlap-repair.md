# Codex Pass90 Governance Overlap Repair

claim_scope: Pass90 inspected the branch-governance overlap named by Pass89, made a minimal additive governance repair in two allowed files, and wrote this evidence report. This is not product implementation and not a clean-mergeability proof.
evidence_scope: Current-turn targeted reads of required governance/skill files, current and origin/main marker searches/readbacks for `AGENTS.md`, `CLAUDE.md`, `docs/skills/claim-verification-SKILL.md`, and `docs/skills/plan-review-SKILL.md`, same-base diff scans against merge-base `54399063198d984944ad09ab56433e6a4e86af6e`, additive edits to `CLAUDE.md` and `docs/skills/plan-review-SKILL.md`, and post-edit readbacks/marker searches before this report write.
count_claimed: 1 Pass90 evidence report written; 2 allowed governance files additively edited; 0 product edits; 0 test edits; 0 dotnet runs; 0 git stage/commit/push/checkout/switch/reset/merge/rebase/stash/clean actions; 0 clean-mergeability claims.
independent_evidence_count: 0 independent pass/review records claimed. Command and readback outputs are listed as evidence groups, not counted as cycles, reviews, or passes.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: yes. The evidence supports only a governance-overlap reduction and candidate-proof preparation. OP-67 remains open.

## Skill Load Status

| file | status | read boundary |
| --- | --- | --- |
| `AGENTS.md` | found / targeted-read | OP-67 current lines 6456-6503; current sub-agent recovery and micro-check lines 781-830; origin/main OP-67 and sub-agent recovery marker searches/readbacks. |
| `CLAUDE.md` | found / targeted-read | Clean-mergeability current section around lines 2399-2434; current and origin/main sub-agent recovery/visibility area around lines 646-690; post-edit readback lines 684-703. |
| `docs/SESSION-START-PROMPT.md` | found / targeted-read | Claim ledger lines 221-250 and skill-load/identity gates lines 285-326. |
| `docs/skills/claim-verification-SKILL.md` | found / targeted-read | Current sub-agent recovery and micro-check lines 419-460; origin/main recovery section around lines 396-413. |
| `docs/skills/plan-review-SKILL.md` | found / targeted-read | OP-67 mergeability check lines 3240-3273; current/origin-main final-between-slices gate around lines 3429-3524; post-edit readback lines 3455-3520. |
| `docs/skills/implementation-integrity-SKILL.md` | found / targeted-read | Check 16 clean-mergeability lines 738-768. |
| `docs/skills/audit-protocol-SKILL.md` | found / targeted-read | Skill-load/identity gates lines 133-144 and pass-evidence packet proof lines 247-262. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found / targeted-read | Evidence thresholds/layers lines 140-180 and Tier 0 governance anchors lines 275-288. |
| `docs/skills/verification-before-completion-SKILL.md` | found / targeted-read | Evidence-before-claim lines 51-60, verification sequence lines 161-177, and status-only-not-completion lines 332-334. |

## What Was Inspected

- Pass89 routing plan: `docs/evidence/design-conversion/codex-pass89-op67-closure-plan.md`.
- Current worktree and origin/main marker presence for the four allowed governance files.
- Same-base hunk scans from merge-base `54399063198d984944ad09ab56433e6a4e86af6e` to current worktree and to `origin/main`.
- Dirty overlap checks against `origin/main` and `origin/DesignToCode-clean`.
- A read-only committed-HEAD-only `git merge-tree` scan. This ignores uncommitted Pass90 edits and cannot prove the all-dirty or candidate tree.

## Inspected Overlap

| file | current-vs-origin/main overlap | Pass90 action |
| --- | --- | --- |
| `AGENTS.md` | Both sides carry the sub-agent recovery region. Shared markers include `Sub-agent error is recovery queue, not stop`, `usage limit/capacity exhaustion/timeout/tool error`, `errored agent without evidence counts as zero cycles/reviews/passes`, `record exact failing agent id/error`, `spawn smaller bounded recovery agent if tools allow`, `switch to next independent safe bounded work item if exact path externally blocked`, `no sub-agent capacity is external blocker, not product completion`, `preserve all uncommitted evidence`, and `human-readable phase counts and remaining work`. Current additionally carries `Sub-agent micro-check incomplete packet recovery gate`, `0 completed checks is not a pass`, `atomic command/result before composite script`, `unknown cleanup routes to cleanup worker`, and `after two 0-check incomplete packets repair the work-order shape`. | Not edited. Current already semantically preserves the main-side sub-agent recovery gate while retaining the branch-side micro-check gate. Rewriting this same region would add conflict risk without a narrower proof gain. |
| `CLAUDE.md` | Origin/main carries a `Sub-agent error is recovery queue, not stop` block after `Sub-Agent Visibility Ledger`. Current had the visibility ledger and extension/readiness gates but lacked the exact main-side recovery block in that location. | Edited additively. Inserted the origin/main-compatible sub-agent recovery block after `Sub-Agent Visibility Ledger`, before `False Completion / Stop Repair Rule`. |
| `docs/skills/claim-verification-SKILL.md` | Current preserves the main-side `Sub-agent error is recovery queue, not stop` gate and adds the branch-side `Sub-agent micro-check incomplete packet recovery gate`. Origin/main lacks the micro-check section. | Not edited. Current preserves both the recovery gate and the stricter micro-check gate; deleting either would remove a Luba rule. |
| `docs/skills/plan-review-SKILL.md` | Origin/main adds sub-agent recovery hard-fail literals and reject bullets in the Final Between Bounded Slices Review Gate. Current lacked those exact main-side literals while retaining adjacent continuation/status gates. | Edited additively. Added the missing sub-agent recovery literals and reject bullets. |

## Changes Made Or Not

- `CLAUDE.md`: edited. Added only the sub-agent recovery block read back at current lines 684-703.
- `docs/skills/plan-review-SKILL.md`: edited. Added only sub-agent recovery hard-fail literals read back at current lines 3461-3468 and reject bullets read back at current lines 3512-3520.
- `AGENTS.md`: not edited. Current already contains OP-67 plus sub-agent recovery and micro-check material; no minimal additive change was necessary for the Pass90 overlap goal.
- `docs/skills/claim-verification-SKILL.md`: not edited. Current already contains both the main-side sub-agent recovery gate and the branch-side micro-check gate.
- Production and test code: not edited.

## Verification Performed Before This Report Write

- Read back `CLAUDE.md` changed lines 684-703. The changed lines contain no visible mojibake markers.
- Read back `docs/skills/plan-review-SKILL.md` changed lines 3461-3468 and 3512-3520. The changed lines contain no visible mojibake markers.
- Marker search found OP-67/mergeability literals in `AGENTS.md`, `CLAUDE.md`, and `docs/skills/plan-review-SKILL.md`.
- Marker search found micro-check literals in `AGENTS.md` and `docs/skills/claim-verification-SKILL.md`.
- Marker search found sub-agent recovery literals in `AGENTS.md`, `CLAUDE.md`, `docs/skills/claim-verification-SKILL.md`, and `docs/skills/plan-review-SKILL.md`.

## What Remains For OP-67

OP-67 remains open. The current worktree still overlaps `origin/main` in all four allowed governance files:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/plan-review-SKILL.md`

The current worktree still overlaps `origin/DesignToCode-clean` in:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/plan-review-SKILL.md`

The committed-HEAD-only merge-tree scan still reported `changed in both` for shared governance content, including `AGENTS.md` and `docs/skills/claim-verification-SKILL.md`. That command does not include this uncommitted Pass90 worktree repair, so it is only blocker evidence, not proof of the repaired candidate.

Product implementation cannot safely continue under OP-67 yet. The branch-governance prerequisite still requires an authorized candidate/staged tree or committed-tip proof that the exact candidate merges cleanly into both the original/base branch and `main`.

## Next Bounded Worker Recommendation

Recommended next worker: Pass91 candidate-tree or staged-tree OP-67 proof, only after Luba grants explicit authority for the required candidate/stage operation.

Scope:

- Build an exact candidate tree from the already-authorized dirty governance/evidence files, or stage exactly the authorized closure files if Luba grants staging authority.
- Run `git diff --check` on the exact candidate.
- Run no-write mergeability checks for that exact candidate into `origin/DesignToCode-clean` and `origin/main`.
- If any `changed in both`, add/add, conflict marker, or same-region governance drift remains, repair only the explicitly authorized governance files and rerun.
- Do not claim `mergeability: clean into <original> AND main (git merge-tree, 0 conflicts)` until the exact candidate tree proves it.

Forbidden next-worker assumptions:

- Do not treat this semantic repair as clean mergeability.
- Do not stage, commit, push, PR, switch, reset, merge, rebase, stash, clean, run dotnet, or touch product/test code unless Luba explicitly authorizes that later.

## Pass Verdict

PASS90_GOVERNANCE_OVERLAP_REDUCED_OP67_STILL_OPEN.

This report is evidence-only. I did not verify clean mergeability. I did not run dotnet. I did not stage, commit, push, PR, switch, reset, merge, rebase, stash, or clean. The evidence supports only: two minimal additive governance repairs were made and OP-67 still needs candidate/staged-tree proof.
