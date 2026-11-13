# Codex Pass109 - Subagent Identity Governance Repair

claim_scope: Governance-only repair for the spawned-worker identity loophole that allowed packets to return `subagent_id_or_NONE: NONE`.
evidence_scope: AGENTS/session/skill governance files plus Pass105-Pass108 final evidence files. No product behavior, DesignConversion code/test edits, training, provider calls, git write operations, or files under C:\Users\lubao\Documents\xiigen.
count_claimed: 1 governance repair artifact, 0 product passes.
independent_evidence_count: 4 context evidence files inspected plus direct changed-rule readback and governance verification commands.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for the narrow governance repair; yes for any product, training, or pass-count claim.

## Skill Load Status

- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- docs/skills/planning-session-startup-SKILL.md: found/read.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read.
- docs/skills/claim-verification-SKILL.md: found/read.
- docs/skills/audit-protocol-SKILL.md: found/read.
- docs/skills/output-skepticism-SKILL.md: found/read.
- docs/skills/plan-review-SKILL.md: found/read.
- docs/skills/generated-code-review-SKILL.md: found/read.
- docs/skills/implementation-integrity-SKILL.md: found/read.
- docs/skills/verification-before-completion-SKILL.md: found/read.

## Files Read

- AGENTS.md
- docs/SESSION-START-PROMPT.md
- docs/skills/planning-session-startup-SKILL.md
- docs/skills/architect-behavior-classifier-SKILL.md
- docs/skills/claim-verification-SKILL.md
- docs/skills/audit-protocol-SKILL.md
- docs/skills/output-skepticism-SKILL.md
- docs/skills/plan-review-SKILL.md
- docs/skills/generated-code-review-SKILL.md
- docs/skills/implementation-integrity-SKILL.md
- docs/skills/verification-before-completion-SKILL.md
- docs/evidence/design-conversion/codex-pass105-runtime-dpo-review-approval-boundary.md
- docs/evidence/design-conversion/codex-pass106-local-dpo-candidate-approval-transition.md
- docs/evidence/design-conversion/codex-pass107-approved-dpo-handoff-export.md
- docs/evidence/design-conversion/codex-pass108-training-intake-boundary.md

## Files Written

- AGENTS.md
- docs/SESSION-START-PROMPT.md
- docs/skills/planning-session-startup-SKILL.md
- docs/skills/claim-verification-SKILL.md
- docs/skills/audit-protocol-SKILL.md
- docs/skills/plan-review-SKILL.md
- docs/evidence/design-conversion/codex-pass109-subagent-identity-governance-repair.md

## Implementation Summary

Added `pass109_spawned_worker_identity_pre_return_self_check` to the parent
agent file, session prompt, startup skill, claim-verification skill, audit
protocol, and plan-review skill. The new subgate requires every spawned-worker
work order to include this return-schema instruction:

```text
subagent_id_or_NONE: <platform id if visible, otherwise UNKNOWN_IN_SUBAGENT>
subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
```

Before return, the spawned worker must search/read back its own packet and any
evidence file it wrote for `subagent_id_or_NONE`. If the packet contains
`subagent_id_or_NONE: NONE`, `subagent_id_or_NONE=NONE`, `NONE_PARENT`,
`current-thread worker`, or `id not visible`, the worker must correct the packet
before return. Parent/reviewer acceptance gates must reject an uncorrected
spawned-worker packet as identity-defective evidence-only and count it as 0
passes/reviews/cycles.

## Changed Rule Readback

- AGENTS.md: pass109 self-check visible at lines 376-389.
- docs/SESSION-START-PROMPT.md: pass109 self-check visible at lines 322-335.
- docs/skills/planning-session-startup-SKILL.md: pass109 self-check visible at lines 171-184.
- docs/skills/claim-verification-SKILL.md: pass109 self-check visible at lines 189-203.
- docs/skills/audit-protocol-SKILL.md: pass109 self-check visible at line 145.
- docs/skills/plan-review-SKILL.md: pass109 self-check visible at lines 308-317.

## Verification Results

- `rg "subagent_id_or_NONE|UNKNOWN_IN_SUBAGENT|spawned worker|identity" docs/skills AGENTS.md docs/SESSION-START-PROMPT.md` found the existing identity gates and the directly relevant local skill/guide surfaces.
- Pass105-Pass108 final evidence search found `subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT` in all four current evidence files and found no `subagent_id_or_NONE: NONE` or `subagent_id_or_NONE=NONE` in those final files. This supports only that the inspectable final packets use the corrected fallback; it does not prove the raw earlier defective packets.
- Changed-rule readback found the new pass109 self-check in all six patched governance files.
- Search over the six patched governance files found `UNKNOWN_IN_SUBAGENT` and `subagent_identity_self_check` in every patched rule surface.
- Search for `subagent_id_or_NONE: NONE` / `subagent_id_or_NONE=NONE` in patched governance files found only forbidden/rejection examples, not a permissive spawned-worker example.
- Conflict-marker scan over patched governance files and this Pass109 evidence file returned no matches.
- Mojibake scan over the new pass109 changed sections and this Pass109 evidence file returned no matches. A broader file scan still sees pre-existing literal marker-list examples in AGENTS.md and claim-verification-SKILL.md; those are policy examples, not changed Pass109 text.
- `git diff --check -- AGENTS.md docs\SESSION-START-PROMPT.md docs\skills\planning-session-startup-SKILL.md docs\skills\claim-verification-SKILL.md docs\skills\audit-protocol-SKILL.md docs\skills\plan-review-SKILL.md` returned no output; this untracked evidence file had a separate trailing-whitespace scan with no matches.
- Path-scoped status over allowed governance/evidence paths plus `grep.exe.stackdump` as a status path only listed the six modified governance files, this new Pass109 evidence file, and pre-existing untracked `grep.exe.stackdump`. It did not read, delete, or modify `grep.exe.stackdump`.

## Closed Claim

Future spawned-worker work orders and packets now have an explicit mandatory
fallback to `UNKNOWN_IN_SUBAGENT`, a pre-return self-check, and parent/reviewer
rejection/count-zero rules for uncorrected spawned-worker `NONE` identity
packets.

## Not Closed Claims

- No product behavior was implemented or verified.
- No DesignConversion production/test file was edited in Pass109.
- No raw Pass105-Pass108 defective packet was proven from the final evidence files; those final files show the corrected fallback.
- No training, checkpoint mutation, weights, model upload, public host/API, provider call, production DB, broad tests, mergeability, git add/commit/push/branch/merge/rebase/checkout, or product readiness was run or claimed.

## Forbidden Scope Check

- No `claude-*` file was edited.
- Nothing under `C:\Users\lubao\Documents\xiigen` was written.
- `grep.exe.stackdump` was used only as a path-scoped git status argument and was not read, deleted, or modified.
- No DesignConversion product/test file was edited for this governance pass.
- No permission prompt or sandbox/escalation setting was used.

## Next Recommended Slice

For the next spawned-worker work order, include the new return-schema snippet
verbatim and require the worker to report `subagent_identity_self_check` before
the parent accepts any pass/review/count evidence.
