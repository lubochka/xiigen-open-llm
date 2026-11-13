# Row20 Path A Post-Repair Evidence Packet

claim_scope: Row20 Path A installed-plugin JS entrypoint boundary metadata acceptance only.
evidence_scope: Named precondition files read, installed skill read, installed entrypoint patch, post-repair command attempt with recorded failure, one targeted post-fix node .\src\codex-entrypoint.js command from installed plugin root, generated JSON parsing, and targeted run-folder verification.
count_claimed: 1 source patch; 2 command attempts total; 1 fresh repair folder; 2 fresh run folders; 0 host UI; 0 joined passes; 0 product-ready extensions.
independent_evidence_count: 11.
subagent_id_or_NONE: Row20PathAEntryRepair-Codex-20260626.
downgrade_needed: yes. The evidence supports only Path A boundary metadata preservation if the gate passed.

## Files Changed

- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-entrypoint-repair-20260626-194721\entrypoint-repair-notes.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-entrypoint-repair-20260626-194721\entrypoint-repair-status.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\request.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\response.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\trace.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\command-result.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\row20-path-a-post-repair-evidence-packet.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-195005\row20-path-a-post-repair-status.json

## Command

Ran post-fix installed-plugin boundary command from:

- C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500

Entrypoint:

- node .\src\codex-entrypoint.js

Exit code: 0
Response success: True

A prior command attempt in C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\path-a-run-20260626-194721 failed after the first patch hunk because recovered numeric-looking metadata was not yet string-valued for the core dictionary contract. That failure is recorded as evidence and not counted as acceptance.

## Metadata Gate

- HostFacts.surface: True
- HostFacts.row: True
- HostFacts.scenario_id: True
- CheckSetInputs.scenario_id: True
- CheckSetInputs.marker: True
- Provenance.source: True
- Provenance.plugin: True
- Provenance.scenario_id: True

path_a_result: PATH_A_BOUNDARY_METADATA_PRESERVED
path_a_acceptance_gate_passed: True

## Non-Claims

I did not verify product readiness.
I did not run host UI.
I did not run a joined Path A/Path B scenario.
I did not run training, eval, DPO, checkpoint promotion, or locator mutation.
I did not mutate checkpoints or locators.
I did not use hidden external AI.
This packet is not evidence that 3 extensions are product ready.

## Return Packet

subagent_id_or_nickname: Row20PathAEntryRepair-Codex-20260626
files_read: named precondition evidence files, installed src\codex-entrypoint.js, installed skills\xiigen-smart-skills\SKILL.md
files_changed: listed above
patch_summary: Added CLI object metadata parser for PowerShell-stripped JSON object arguments, kept recovered metadata values string-valued for core dictionary contracts, and fail-closed diagnostics for invalid provided metadata flags.
command_run_or_blocker: two command attempts total; first failed on string-contract mismatch; post-fix command exit_code=0; response_success=True
verification_commands_run: generated JSON parse checks for required metadata; targeted evidence file writes
verification_results: metadata_preserved=True; path_a_acceptance_gate_passed=True; path_a_result=PATH_A_BOUNDARY_METADATA_PRESERVED
path_a_result: PATH_A_BOUNDARY_METADATA_PRESERVED
exact_next_action: Parent review of this evidence packet; do not claim product readiness from Path A boundary metadata alone.
mutation_boundaries_kept: no host UI/screenshots; no training/eval/DPO/checkpoint/locator mutation; no product code edits; no board/counter/status updates outside fresh evidence folders; no git stage/commit/push; no hidden external AI.
non_claims: no product readiness; no host UI; no joined pass; no token savings; no semantic quality; no learnability; no checkpoint promotion.
parent_will_perform_commit_push_after_review=true

## Targeted Verification Commands Run

- Select-String against installed src\codex-entrypoint.js for parsePowerShellStrippedObject, parseCliJsonObjectOrDefault, host-facts-json, check-set-inputs-json, and provenance-json.
- Parsed post-fix request.json and verified HostFacts.surface, HostFacts.row, HostFacts.scenario_id, CheckSetInputs.scenario_id, CheckSetInputs.marker, Provenance.source, Provenance.plugin, and Provenance.scenario_id.
- Parsed row20-path-a-post-repair-status.json and verified path_a_result=PATH_A_BOUNDARY_METADATA_PRESERVED, path_a_acceptance_gate_passed=true, and host/product/training/mutation claims false.
- Ran git diff -- src/codex-entrypoint.js in the installed plugin worktree. The diff includes earlier target-locator changes already present before this worker patched; this worker-authored source repair is the CLI metadata object parser and the three metadata flag callsite changes.
- Ran git status --short -- against the fresh Row20 repair/run folders in C:\Projects\llm_mvp_core.

## Git Visibility

The installed plugin source is inside its own git worktree. The project evidence folders are untracked in C:\Projects\llm_mvp_core. No git stage, commit, or push was run.
