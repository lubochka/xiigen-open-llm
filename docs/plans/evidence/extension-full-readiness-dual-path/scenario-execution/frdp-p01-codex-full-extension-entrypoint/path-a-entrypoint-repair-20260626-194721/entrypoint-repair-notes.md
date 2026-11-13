# Row20 Path A Entrypoint Repair Notes

claim_scope: Minimal installed-plugin entrypoint metadata repair only.
evidence_scope: Named precondition evidence read, installed entrypoint and skill read, source patch to installed plugin entrypoint, one failed post-repair command attempt exposing string-contract mismatch, one targeted post-fix installed-plugin boundary command, generated JSON parse checks.
count_claimed: 1 source patch with 2 hunks; 2 command attempts total; 1 fresh repair folder; 2 fresh run folders created by this worker.
independent_evidence_count: 11 evidence records.
subagent_id_or_NONE: Row20PathAEntryRepair-Codex-20260626.
downgrade_needed: yes. This proves only Path A boundary metadata preservation when acceptance is true; not host UI, joined pass, or product readiness.

## Root Cause

The installed entrypoint parsed metadata flags with JSON.parse through parseJsonOrDefault. Under Windows PowerShell native invocation, JSON object quotes can arrive stripped before Node receives argv. The previous code had a stripped-JSON fallback for array skill flags only, so object metadata flags fell back to empty objects without diagnostics.

The first post-repair command attempt exposed a second narrow issue in the repair: stripped numeric-looking values were recovered as numbers, but the core HostFacts/CheckSetInputs/Provenance dictionaries require string values. The patch was narrowed so recovered stripped metadata values remain strings.

## Patch

Patched only:

- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js

Patch summary: Added CLI object metadata parser for PowerShell-stripped JSON object arguments, kept recovered metadata values string-valued for core dictionary contracts, and fail-closed diagnostics for invalid provided metadata flags.

The patch preserves existing request field names: HostFacts, CheckSetInputs, Provenance.

## Command Attempts

- Failed patch-validation attempt recorded in: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-194721
- Post-fix proof run recorded in: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005

## Mutation Boundaries

No host UI, screenshots, training, eval, DPO, checkpoint mutation, locator mutation, product code edits, board/status updates outside fresh evidence folders, git stage, commit, or push were performed.
