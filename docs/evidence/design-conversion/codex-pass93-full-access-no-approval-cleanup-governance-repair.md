# Pass93 Full Access No-Approval Cleanup Governance Repair

## Scope

Current task: close the permission-prompt loophole that allowed an approval prompt before exact `Stop-Process` and temp cleanup under danger-full-access / approval policy never.

Not the task: product implementation, tests/builds, git actions, temp cleanup, process termination, or any work outside `C:\Projects\llm_mvp_core`.

## Evidence Basis

- Existing `FULL_ACCESS_NO_PERMISSION_PROMPT_GATE` was found in `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/claim-verification-SKILL.md`, `docs/skills/reconnaissance-gate-SKILL.md`, and `docs/skills/audit-protocol-SKILL.md`.
- Existing micro-check cleanup gates were found in `AGENTS.md` and `docs/skills/claim-verification-SKILL.md`.
- `docs/skills/verification-before-completion-SKILL.md` did not contain the full-access marker before this repair, so it received a narrow completion-verification gate.

## Repair Added

The repair adds a narrow additive subgate:

- Under full access or approval policy never, agents must not use `sandbox_permissions`, approval prompts, or user-permission questions for authorized filesystem/governance cleanup.
- `approval prompt under full access/approval never is a governance failure`.
- `exact stale-process/temp cleanup` is allowed and required when exact PIDs/process names/path proof ties the cleanup to the current work chain.
- `Stop-Process is not automatically an approval boundary under full access`.
- If process ownership cannot be proven, agents record the exact blocker and perform read-only evidence only, without opening an approval prompt under approval policy never.
- Parent/direct command cleanup is allowed only for exact stale temp/process cleanup after repeated subagent cleanup-path failure or when approval prompts are forbidden by the environment, with before/after proof and no broadened scope.

## Files Changed

- `AGENTS.md`
- `docs/SESSION-START-PROMPT.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/reconnaissance-gate-SKILL.md`
- `docs/skills/audit-protocol-SKILL.md`
- `docs/skills/verification-before-completion-SKILL.md`
- `docs/evidence/design-conversion/codex-pass93-full-access-no-approval-cleanup-governance-repair.md`

## Claim Guard

claim_scope: governance wording repair only
evidence_scope: requested files, marker searches, readbacks, diff/conflict checks
count_claimed: 1 bounded governance repair
independent_evidence_count: 13
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for governance wording repair; no product/runtime/test claim made

## Verification Summary

- Required marker search found `approval prompt under full access/approval never is a governance failure`.
- Required marker search found `Stop-Process is not automatically an approval boundary under full access`.
- Required marker search found `exact stale-process/temp cleanup`.
- Changed-section readback showed the new `FULL_ACCESS_EXACT_CLEANUP_NO_APPROVAL_SUBGATE` next to the existing full-access gate in `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/claim-verification-SKILL.md`, `docs/skills/reconnaissance-gate-SKILL.md`, and `docs/skills/audit-protocol-SKILL.md`.
- Changed-section readback showed the new `Full Access Exact Cleanup Verification Gate` in `docs/skills/verification-before-completion-SKILL.md`.
- `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- <changed files>` returned exit code 0 with no output for tracked changed files; the untracked evidence file was separately covered by readback, marker search, conflict scan, and trailing-whitespace scan.
- Conflict marker scan on changed files returned no matches.
- Evidence file readback completed.
