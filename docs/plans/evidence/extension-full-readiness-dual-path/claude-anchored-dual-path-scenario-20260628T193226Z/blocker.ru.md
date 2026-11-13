# Blocker packet: Claude anchored dual-path scenario

## Honesty scope

- claim_scope: Path A anchored Claude extension adapter/core invocation completed; Path B local Claude UI/app was found but same-prompt submit was blocked before mutation because a safe blank/new-chat control could not be invoked.
- evidence_scope: files in this evidence folder only; no product code edits; no AGENTS/CLAUDE/skills/governance/plan edits.
- count_claimed: 1 Path A attempt completed; 1 Path B guarded attempt blocked before submit.
- independent_evidence_count: 5 executed evidence actions: Path A helper, Path B process preflight, Path B accessibility preflight, failed pre-UI helper launch, guarded Path B helper safety-block.
- subagent_id_or_NONE: CLAUDE-ANCHORED-DUAL-PATH-SCENARIO-15
- downgrade_needed: yes. The evidence supports only Path A anchored adapter/core proof plus Path B blocker, not full dual-path same-scenario proof.

## Anchors

- marker: `FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1`
- scenario_id: `frdp-p01-claude-anchored-dual-path`
- pair_id: `frdp-p01-row12-claude-anchored-v1`
- prompt_sha256: `f1c7299220fbf3c8027dc700a0980e0732761d9af028e9e3e6c8c28dc34feafa`

## Path A result

Path A ran through the local Claude extension adapter route:

`C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js` -> `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`

Evidence:

- `path-a-command.json`
- `path-a-request.json`
- `path-a-core-trace.json`
- `path-a-host-trace.json`
- `path-a-response.json`
- `path-a-output.log`
- `path-a-stderr.log`
- `path-a-summary.json`

Narrow findings:

- `path-a-summary.json` reports exit code `0`.
- `path-a-request.json` contains the marker, scenario_id, and pair_id in the request prompt.
- `path-a-core-trace.json` has the matching prompt hash.
- `external_ai_used=false` in core/host trace.
- `path-a-response.json` reports success with status `core_boundary_partial_no_semantic_quality_claim`.
- The response did not echo all anchors; anchor strength for Path A comes from request + prompt hash + trace, not response text.

## Path B blocker

Path B did not submit the prompt.

What ran:

- `path-b-preflight.json`: found a visible local Claude app window.
- `path-b-accessibility-preflight.json`: inspected UI accessibility without dumping private text.
- `path-b-local-claude-ui-probe.ps1`: attempted a guarded UI flow.
- `path-b-blocker.json`: recorded the safety blocker.

Exact blocker:

`Could not invoke a visible New Chat control before submitting; avoiding capture or mutation of an existing conversation.`

Safety result:

- same_prompt_submitted=false
- screenshots_captured=0
- no_secrets_typed=true
- no_secrets_captured=true

## Join result

Join review was not run because Path B did not submit the same prompt and did not produce UI screenshots/result text. Full Claude anchored same-scenario dual-path proof is not proven.

The evidence supports only:

1. Path A anchored Claude extension adapter/core invocation.
2. Path B local Claude UI/app availability.
3. Path B blocker before same-prompt submission.

## Not claimed

- product readiness
- token savings
- semantic quality
- training
- checkpoint promotion
- locator update
- full live Claude UI same-prompt completion

## Next smallest bounded action

Manually open a blank authenticated Claude chat with no private content visible, or expose a reliable local New Chat UI control/shortcut, then rerun the same Path B probe with the same marker/scenario_id/pair_id/prompt hash.
