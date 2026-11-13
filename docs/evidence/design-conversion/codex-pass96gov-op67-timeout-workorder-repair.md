# Codex Pass96GOV OP-67 Timeout Work-Order Repair

claim_scope: Pass96GOV governance/work-order repair for the OP-67 temp-proof timeout loophole in `C:\Projects\llm_mvp_core`.
evidence_scope: Current Luba Pass96GOV work order, targeted reads of required governance/skill files, full reads of the named Pass88/Pass89/Pass90/Pass91B/Pass93/Pass95R context packets, read-only git status, additive edits to allowed governance skill files, and this evidence file. No product files, tests, builds, OP-67 proof, git write action, external provider action, or `C:\Users\lubao\Documents\xiigen` write was performed.
count_claimed: 1 governance repair evidence file written; 3 allowed governance files edited; 0 OP-67 proof runs; 0 product/test edits; 0 product tests/builds; 0 git add/commit/push/switch/checkout/reset/merge/rebase/stash/clean actions.
independent_evidence_count: 0 OP-67 proof records claimed. The readbacks, marker scans, and git status are verification evidence groups, not passes/reviews/cycles.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: yes for OP-67. This repair closes the timeout/work-order loophole only; it does not prove clean mergeability.

## Skill Load Status

| file | status | read boundary |
| --- | --- | --- |
| `AGENTS.md` | found / targeted-read | Read current sub-agent recovery, micro-check, <=10 minute, OP-67, OP-70, and target-artifact identity contexts. |
| `docs/SESSION-START-PROMPT.md` | found / targeted-read | Read current work-order completeness, skill-load, continuation, no-stop, no-approval, architect/governance boundaries, and heartbeat contexts. |
| `docs/skills/claim-verification-SKILL.md` | found / targeted-read | Read skill-load, no-mojibake, sub-agent identity, sub-agent error recovery, micro-check, timeout, and evidence-scope contexts. |
| `docs/skills/audit-protocol-SKILL.md` | found / targeted-read | Read work-order completeness, skill-load, identity, no-safe-work, and downgrade/recovery contexts. |
| `docs/skills/implementation-integrity-SKILL.md` | found / targeted-read | Read bounded-worker, <=10 minute packet, evidence-file integrity, and Check 16 OP-67 contexts. |
| `docs/skills/verification-before-completion-SKILL.md` | found / targeted-read | Read evidence-before-claim, count/sub-agent evidence, completion coverage, and status-only-not-completion contexts. |
| `docs/skills/plan-review-SKILL.md` | found / targeted-read | Read bounded worker, Final Between Bounded Slices, OP-67 FC-43, and sub-agent error recovery contexts. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found / targeted-read | Read work-order completeness, evidence layers, target verification, and deliverable-exists contexts. |
| `docs/evidence/design-conversion/codex-pass95r-skill-stack-mojibake-identity-recovery.md` | found / read | Read Pass95R identity/mojibake recovery packet. |

## Previous Worker Failure Classification

previous_worker_failure_classification: GOVERNANCE_GAP_AND_WORK_ORDER_OVERSCOPE.

Parent evidence recorded in the current work order says Arendt worker `019f2263-4166-7801-bc35-da39fd3046fe` timed out on a 1h wait, timed out on a 15m interrupt/status request, was closed with `previous_status=running`, and shutdown notification was received. Parent read-only checks found no `.codex-pass96*` temp path and no `docs/evidence/design-conversion/codex-pass96-op67-temp-proof-or-blocker.md`. Current git status shows untracked `grep.exe.stackdump`.

This evidence supports only: the broad OP-67/temp-proof worker returned no packet, left no evidence file, and must be treated as zero OP-67 proof. It does not support OP-67 closure, clean mergeability, product readiness, or product implementation.

## Files Changed

- `AGENTS.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md`

Not changed: `docs/SESSION-START-PROMPT.md`, `docs/skills/verification-before-completion-SKILL.md`, product files, test files, `claude-*` files, git refs/config, `grep.exe.stackdump`, or any path under `C:\Users\lubao\Documents\xiigen`.

## New Governance Rules

1. OP-67/temp-proof workers must be atomic and evidence-first.
2. Before any temp clone/copy/candidate/materialization/temp-index/merge-tree proof operation expected to exceed 10 minutes, the worker must write/read back a codex-prefixed evidence/status artifact or return a heartbeat/status packet.
3. If the worker cannot write/read back that early evidence file before long operations, it must return `NEEDS_FIX` immediately.
4. A broad OP-67 worker that times out or is closed with no packet and no evidence file counts as zero OP-67 proof and zero completed work for that scope.
5. Timeout/incomplete/no-result remains a recovery queue, not final status.
6. After a no-result broad OP-67 worker, the next OP-67 attempt must split into: `inventory-only -> candidate-materialization-only -> merge-tree-original-only -> merge-tree-main-only -> cleanup-only`.
7. Each slice must write/read back its own codex-prefixed evidence file before claiming completion.
8. No all-in-one long OP-67 proof worker is allowed again until the atomic slices pass.

## Context Read Summary

- Pass88 recorded OP-67 as the nearest safe continuation and product work as blocked by branch-governance risk.
- Pass89 recorded OP-67 still open and recommended governance-overlap repair/candidate-proof preparation.
- Pass90 reduced governance overlap but explicitly did not prove clean mergeability.
- Pass91B recorded the candidate-tree proof blocker and cleanup closure; OP-67 remained not proven.
- Pass93 repaired no-approval cleanup governance but did not authorize product/test/git write work here.
- Pass95R preserved the spawned-worker identity rule: this Pass96GOV packet uses `UNKNOWN_IN_SUBAGENT`, not `NONE`.

## Post-Write Verification

| check | result |
| --- | --- |
| Changed governance line readback | PASS. `Select-String` read back the new OP-67/temp-proof rule contexts in `AGENTS.md`, `docs/skills/claim-verification-SKILL.md`, and `docs/skills/implementation-integrity-SKILL.md`. |
| Evidence file readback | PASS. `Get-Content -Raw` read back this file after creation. |
| Conflict marker scan | PASS. `rg -n "^(<<<<<<<\|=======\|>>>>>>>)" AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/implementation-integrity-SKILL.md docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md` returned no matches. |
| Mojibake marker scan | PASS. `rg -n "\x{00D0}\|\x{00D1}\|\x{00C2}\|\x{00E2}\|\x{FFFD}" AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/implementation-integrity-SKILL.md docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md` returned no matches. |
| Tracked changed-file whitespace check | PASS. `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/implementation-integrity-SKILL.md` exited 0 with no output. |
| Evidence file trailing-whitespace scan | PASS. `rg -n "[ \t]+$" docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md` returned no matches. |
| Current status check | PASS as inventory only. Current status still shows the pre-existing dirty worktree plus this new untracked evidence file; `grep.exe.stackdump` remains untracked and was not deleted. |

## Next Recommended Worker

next_recommended_worker: Pass96OP67A inventory-only worker.

The next OP-67 worker should not run temp clone/copy/proof. It should read status/inventory only, write/read back `docs/evidence/design-conversion/codex-pass96-op67-inventory-only.md` or another codex-prefixed inventory evidence file named in its work order, and return a packet. Only after that succeeds should a separate candidate-materialization-only worker run.

## Final Verdict

final_verdict: PASS96GOV_GOVERNANCE_REPAIR_WRITTEN_AND_VERIFIED_OP67_PROOF_STILL_NOT_PROVEN.

I did not run OP-67 proof. I did not run product tests/builds. I did not stage, commit, push, switch, checkout, reset, merge, rebase, stash, clean, or delete `grep.exe.stackdump`. I did not write to `C:\Users\lubao\Documents\xiigen`. The evidence supports only this governance/work-order repair and the required next atomic OP-67 retry shape.
