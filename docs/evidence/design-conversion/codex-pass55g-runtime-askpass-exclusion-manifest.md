# Codex Pass55G Runtime Askpass Exclusion Manifest

## Scope

claim_scope: Persist the Pass55F runtime askpass exclusion manifest for narrow DesignConversion product-scope proof only.
evidence_scope: User-provided Pass55F classification plus current path-scoped git status for the exact askpass prefix.
count_claimed: 5 tracked deleted runtime askpass files in this manifest.
independent_evidence_count: 1 current path-scoped git status record.
subagent_id_or_NONE: NONE
downgrade_needed: No for the exact five-file askpass exclusion manifest; yes for any broader clean-tree, full-repo mergeability, artifact integrity, or export completeness claim.

## Skill Load Status

AGENTS.md: found/read relevant proof, claim, status, sub-agent identity, and evidence-scope gates.
docs/skills/claim-verification-SKILL.md: found/read relevant skill-load, count-claim, sub-agent, untracked-artifact, completion-claim, and evidence-boundary gates.
docs/skills/audit-protocol-SKILL.md: found/read relevant evidence-bound audit/readback gate excerpts.
docs/skills/plan-review-SKILL.md: found/read relevant claim/evidence and mergeability/future-gate excerpts.

## Exclusion Manifest

id: codex-pass55g-runtime-askpass-exclusion-manifest
applies_to: DesignConversion product-scope proof only.
excluded_common_prefix: artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb
excluded_status: tracked deleted runtime askpass helper files, git status short code `D`.

excluded_files:
- askpass-empty.sh
- askpass-main.js
- askpass.sh
- ssh-askpass-empty.sh
- ssh-askpass.sh

reason: Pass55F classified these five tracked deleted runtime askpass helper files as excludable only from the narrow DesignConversion product proof because they are runtime askpass artifacts under the extension live-proof user-data capture, not DesignConversion product source files.

allowed_claim: These exact five tracked deleted files under the excluded_common_prefix may be excluded from the narrow DesignConversion product-scope proof.

forbidden_claims:
- This does NOT prove a clean worktree.
- This does NOT prove full repo mergeability.
- This does NOT prove artifact integrity.
- This does NOT prove export completeness.
- This does NOT hide or excuse any other tracked deletion, untracked file, modified file, source-code change, product artifact issue, or repo dirt outside the exact five basenames above.

non_hide_guard: The exclusion is valid only when current scoped status shows exactly the five `D` rows listed above under the excluded_common_prefix. Any additional status row, basename mismatch, prefix mismatch, or status-code mismatch is outside this manifest and must be reported separately.

## Current Validation Command

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core status --short -- artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb
```

Current validation output shape:

```text
 D artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-empty.sh
 D artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-main.js
 D artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass.sh
 D artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass-empty.sh
 D artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass.sh
```

boundary: This manifest does NOT prove clean worktree, full repo mergeability, artifact integrity, export completeness, source-code readiness, test readiness, build readiness, or any product proof beyond the exact five askpass deletions named here.

## Validation Packet

changed_files:
- docs/evidence/design-conversion/codex-pass55g-runtime-askpass-exclusion-manifest.md

validation_commands:
- `git -c safe.directory=C:/Projects/llm_mvp_core status --short -- artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb`
- `Get-Content docs/evidence/design-conversion/codex-pass55g-runtime-askpass-exclusion-manifest.md`
- `rg -n "askpass-empty.sh|askpass-main.js|askpass.sh|ssh-askpass-empty.sh|ssh-askpass.sh|clean worktree|full repo mergeability|artifact integrity|export completeness|does NOT hide" docs/evidence/design-conversion/codex-pass55g-runtime-askpass-exclusion-manifest.md`
- `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- docs/evidence/design-conversion/codex-pass55g-runtime-askpass-exclusion-manifest.md`

remaining_blockers:
- This artifact is evidence-only and does not resolve repo dirt outside the five exact askpass deletions.
- No export, clone, tests/builds, source edits, restore/delete askpass, stage/commit/push, checkout/reset/merge/rebase/stash/clean, full clean-tree proof, external repo action, or broad cleanup was performed.

next_single_subagent_work_order: NONE. Pass55G requested only this persisted manifest and scoped validation.
