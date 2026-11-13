# Claude dual-path discovery — 20260628T191742Z

## Honesty block

claim_scope: discovery/review only for existing Claude dual-path evidence.
evidence_scope: inspected local filesystem paths, targeted text hits, and parsed JSON metadata; did not run product probes, did not inspect screenshot pixels, did not send prompts, did not edit product/governance files.
count_claimed: 1 bounded discovery packet.
independent_evidence_count: 7 command result sets plus parsed files named below.
subagent_id_or_NONE: CLAUDE-DUAL-PATH-EVIDENCE-DISCOVERY-13.
downgrade_needed: yes; this packet supports only narrow evidence discovery, not full Claude readiness.

## Scope

Work order: `CLAUDE-DUAL-PATH-EVIDENCE-DISCOVERY-13`.

This slice was discovery/planning only. I did not implement product code. I did not edit AGENTS, CLAUDE, skills, governance, plans, or product files. I did not use web or external AI. I did not capture secrets. I did not claim product readiness or full Claude readiness.

## Roots found

| Root | Status | Meaning |
| --- | --- | --- |
| `C:\Projects\llm_mvp_core_codex_sync_20260628` | exists | Core sync root inspected. |
| `C:\Projects\claude-xiigen-smart-skills` | exists | Claude extension repo/path found. |
| `C:\Projects\cursor-xiigen-smart-skills` | exists | Cursor root exists, not the main target. |
| `C:\Projects\codex-xiigen-smart-skills` | exists | Codex root exists, not the main target. |

The likely Claude extension root was present, so broad sibling-directory hunting was not needed beyond the initial confirmation.

## What existing core evidence says

Older Phase 04 rollups still record Claude as blocked for safe local marker route:

- `phase-04-local-desktop-discovery-report.md`: Claude installed/running was discovered, but GUI marker/screenshot was blocked by private chat/account risk.
- `phase-04-claude-codex-route-establishment.md`: Claude was downgraded to `BLOCKED_WITH_REASON`; the file says `claude --name` help proves display-name/positional-prompt separation only, not future local-only/no-send safety.
- `phase-04-local-desktop-packets.json`: Claude row is `BLOCKED_WITH_REASON`, `formal_local_desktop_pass=false`, `joined_claim_proven=false`.
- `phase-04-local-desktop-status-board.md`: Phase 04 formal local desktop pass rows = 0 and formal joined pass rows = 0.

Later `scenario-execution` folders contain narrower Claude same-scenario evidence packets that should be reviewed separately from those older Phase 04 blocker rollups.

## Claude Path A evidence found

Path:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-same-scenario-path-a-20260628-144612\`

Parsed `result.json` status:

`CLAUDE_SAME_SCENARIO_PATH_A_PASS`

Automated path recorded in that packet:

- host adapter: `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js`
- core endpoint: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- core target: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`
- method: `prompt-to-task`
- command result: exit code `0`, stderr empty
- selected skills in response summary: `xiigen-smart-skills`, `api-documentation`, `planning`
- explicit non-claims: no full Claude readiness, no host UI readiness, no token savings, no semantic quality, no learning/training/DPO/checkpoint/locator claim

Supported narrow claim:

Claude thin adapter/API/core Path A exists for this exact prompt only.

## Claude Path B evidence found

Path:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-prompt-submission-20260628-142748\`

Parsed `result.json` status:

`CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`

The packet says existing screenshots show prompt entry/submission/progress and a later final visible response in the real Claude Desktop / Codex UI route. The same packet records:

- `no_new_prompt_submitted=true`
- `secrets_captured=false`
- evidence files include `before-prompt.png`, `after-prompt-entered*.png`, `after-send-immediate.png`, `final-after-30s.png`, `final-after-120s.png`, and `current-claude-window-20260628-144001.png`

I did not independently inspect those screenshot pixels in this discovery slice, to avoid unnecessary secret exposure. The evidence supports only that the existing packet claims and file references are present and parseable.

Supported narrow claim:

Existing Path B packet says the already-submitted Claude prompt reached a final visible response in the real local UI route.

## Same-scenario join evidence found

Path:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-same-scenario-dual-path-join-20260628-145508\`

Parsed `result.json` status:

`CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED`

Parsed `result.json` and `evidence-map.json` show:

- Path A expected/observed status: `CLAUDE_SAME_SCENARIO_PATH_A_PASS`
- Path B expected/observed status: `CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`
- both referenced statuses were marked `verified=true` in the join packet
- prompt hash: `305fb4c060179282a469164e3042ad4098ac87866bc1557418450f4308038c72`
- the join proves only that the same prompt is mapped to the Path A and Path B packets and both referenced statuses matched expected statuses
- the join explicitly does not prove full Claude readiness, token savings, semantic quality, learnability, training, DPO, checkpoint mutation/promotion, or locator update

## Same-scenario anchors

Affirmative anchors found:

- same prompt text recorded in Path A and join packet
- SHA-256 prompt hash: `305fb4c060179282a469164e3042ad4098ac87866bc1557418450f4308038c72`
- Path A status: `CLAUDE_SAME_SCENARIO_PATH_A_PASS`
- Path B status: `CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`
- join status: `CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED`

Explicit anchors not found as positive fields:

- `marker`: no positive marker anchor found for this same-scenario join; related Path A response text indicates marker was not provided.
- `scenario_id`: no positive scenario_id field found in parsed result/join metadata.
- `pair_id`: no positive pair_id field found in parsed result/join metadata.

Therefore the same-scenario linkage currently rests on exact prompt/prompt-hash plus referenced result statuses, not on marker/scenario_id/pair_id.

## Claude extension repo evidence found

Key files in `C:\Projects\claude-xiigen-smart-skills`:

- `package.json`: host package name `claude-xiigen-smart-skills`; host kind `Claude`; entrypoint `src/claude-entrypoint.js`; PowerShell launcher `src/claude-entrypoint.ps1`.
- `reports\CL-04-smoke-trace-report.json`: `readiness_level=real_claude_host_runtime_smoke`; `host_runtime_claimed=true`; scope is Claude Code `--print` with session-local plugin loaded through `--plugin-dir`, SessionStart hook executed entrypoint and returned core Xiigen response; explicitly not full extension-suite completion.
- `reports\RUNTIME-02-claude-live-runtime\runtime-02-20260618-010211\runtime-02-claude-live-runtime-mirror.json`: verdict `PASS_NARROW_CLAUDE_CODE_PLUGIN_LOAD_AND_HOOK_CORE_BOUNDARY_RUNTIME_QA`; scope says Claude Code plugin load plus local SessionStart hook/core-boundary path only, full Claude chat UI/Appx proof remains unproven.
- `reports\HOST-08E-claude-hook-qa.json`: `runtime_extension_claimed=false`; remaining distinction says HOST-08E is Claude hook-command/source QA and not full Claude chat UI evidence.
- `phase40-claude-core-response.json`: records `runtimeEvidence=true` but `fullHostRuntimeEvidence=false` and evidence scope `core_api_check_set_runtime_not_full_host`.

These files support narrow Claude Code plugin/hook/core-boundary evidence, not full Claude Desktop/UI readiness.

## What is already proven

Narrowly proven by parsed packet metadata:

- Claude extension repo exists at the expected root.
- Existing Claude adapter/API/core Path A packet exists and reports `CLAUDE_SAME_SCENARIO_PATH_A_PASS`.
- Existing Claude Path B local UI packet exists and reports `CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`.
- Existing join packet exists and reports `CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED`.
- The join packet links Path A and Path B by exact same prompt and prompt hash.

Not independently proven by this discovery slice:

- screenshot visual content, because I did not inspect pixels;
- full Claude readiness;
- product readiness;
- formal full local desktop pass for all Claude scenarios;
- token savings, semantic quality, learnability, training, DPO, checkpoint mutation/promotion, locator update.

## What is missing

- Independent review/acceptance of the later Claude same-scenario join against the source packets.
- Positive `marker`, `scenario_id`, or `pair_id` anchors for the same-scenario join.
- Updated matrix/dashboard reconciliation: older Phase 04/06 files still show Claude blocked, while later scenario-execution evidence records a narrow same-prompt join.
- Full Claude readiness evidence across all required rows remains missing.
- If visual proof is required for acceptance, a redaction-safe screenshot review or re-capture packet is still needed; this discovery did not do it.

## Next smallest bounded action

1. Assign an independent review sub-agent to verify `claude-same-scenario-dual-path-join-20260628-145508` against:
   - `claude-same-scenario-path-a-20260628-144612\result.json`
   - `claude-path-b-prompt-submission-20260628-142748\result.json`
   - the referenced redaction notes and command logs
2. If the reviewer accepts the join, update the product/readiness status artifacts to record only the narrow one-scenario Claude same-prompt dual-path coverage.
3. If the reviewer rejects visual sufficiency, run a bounded Path B visual-redaction verification of existing screenshots or a safe no-secret recapture. Do not send new prompts unless separately authorized.

## Commands run

- Root existence check for core sync and extension roots.
- Sibling Claude repo name discovery under `C:\Projects`.
- Timestamp command using `(Get-Date).ToUniversalTime().ToString(...)`.
- `rg --files` over core evidence and Claude repo.
- Targeted `rg` searches for Claude, Path A/Path B, same-scenario, markers, statuses, and readiness/non-claim language.
- Parsed JSON metadata from Path A, Path B, join, Phase 04 packets, and Claude extension repo reports.
- Created bounded evidence folder with `New-Item -ItemType Directory -Force`.
- `git diff --check` is recorded in `git-diff-check.log`.
