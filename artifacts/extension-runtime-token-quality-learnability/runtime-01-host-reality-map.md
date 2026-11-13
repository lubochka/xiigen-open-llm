# RUNTIME-01 Host Reality Map

Phase: `RUNTIME-01`

Sub-agent: `RUNTIME-01`

Created: `2026-06-16T20:35:47.6586009+03:00`

Scope: read-only local inspection and artifact/state update only. No Claude, Cursor, or Codex host runtime was launched in this slice. No product code, training, eval, checkpoint, commit, or push action was performed.

## What This Proves

- The Claude, Cursor, and Codex extension repositories exist under `C:\Projects`.
- Each host has a concrete thin-adapter/source-entrypoint candidate and package/config evidence.
- Each host has a concrete runtime path candidate that can be handed to the next bounded runtime phase.
- Supporting evidence is separated from live-readiness evidence.

## What This Does Not Prove

- This map does not prove live Claude, Cursor, or Codex extension readiness.
- This map does not prove token savings, quality improvement, DPO, learnability, training, checkpoint mutation, fresh-load, rollback, target promotion, or final product readiness.
- Prior runtime reports are evidence to inspect; this RUNTIME-01 slice did not rerun or freshen them.

## Evidence Separation

| Evidence type | Status in this map | Boundary |
|---|---|---|
| Dashboard/API/helper evidence | Supporting only | Does not prove live host runtime. |
| Package smoke/install smoke | Supporting only | Does not prove full extension readiness. |
| Test-only adapter/handler/component QA | Supporting only | Does not prove production chat/UI/plugin runtime. |
| Direct handler/core call | Supporting only | Does not prove host adapter load or visible host output. |
| Prior runtime proof packets | Historical evidence for next phase planning | Not fresh evidence from this RUNTIME-01 slice. |

## Claude

Repo/path: exists at `C:\Projects\claude-xiigen-smart-skills`.

Real host runtime/chat/UI/CLI entrypoint candidate:

- Executable exists: `C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe`.
- Candidate command shape from prior packet: `claude.exe --plugin-dir <phase-scoped plugin dir> --include-hook-events --output-format stream-json --debug hooks --debug-file <debug.log> --permission-mode dontAsk --tools "" --no-session-persistence --max-budget-usd 0.05 -p <redacted prompt>`.
- Plugin manifest candidate: `C:\Projects\claude-xiigen-smart-skills\.claude-plugin\plugin.json`, with a `SessionStart` command invoking `src\claude-entrypoint.js`.

Thin adapter path candidate:

- `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js`
- `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1`

Command/package/test path candidate:

- `C:\Projects\claude-xiigen-smart-skills\package.json`
- `C:\Projects\claude-xiigen-smart-skills\.claude-plugin\plugin.json`
- `C:\Projects\claude-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\claude-xiigen-smart-skills\reports\HOST-IMPL-01-probe\...`
- `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\...`

Required fresh evidence files for next runtime phase:

- `claude-runtime-proof.md/json` or refreshed `claude-runtime-qa-report.md/json`.
- Fresh Claude command record, stream log, debug log, hook/core request, hook/core response, host trace, core trace, visible output transcript, redacted/raw input/output refs, same-run artifact freshness check, and controlled error-path artifacts.

Exact missing field / execution condition:

- `fresh_runtime_launch_in_this_slice=false`; RUNTIME-01 did not launch Claude.
- `persistent_or_global_claude_installation_proof` remains absent; prior Claude proof is session-scoped through `--plugin-dir`.
- `hooks\hooks.json` is empty, so active hook registration must come from the `.claude-plugin\plugin.json` path or a fresh runtime-specific plugin dir, not from `hooks\hooks.json`.

Next phase:

- `RUNTIME-02` if host runtime launch is approved for a fresh Claude Code session-scoped proof.
- `HOST-IMPL-01` only if parent requires persistent/global Claude hook installation rather than the current phase-scoped plugin-dir runtime path.

Boundary notes:

- The adapter builds `XiigenExtensionRequest`, invokes the core endpoint, and marks host semantic ranking/training/corpus parsing/external AI as false.
- No hidden model/training logic was observed in the inspected adapter path.

## Cursor

Repo/path: exists at `C:\Projects\cursor-xiigen-smart-skills`.

Real host runtime/chat/UI/CLI entrypoint candidate:

- Executable exists: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`.
- Candidate command shape from prior packet: `Cursor.exe --extensionDevelopmentPath C:\Projects\cursor-xiigen-smart-skills --extensionTestsPath C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\runtime03-extensionHostQA.js --user-data-dir <run>\cursor-user-data --extensions-dir <run>\cursor-extensions-dir --disable-workspace-trust --headless --new-window C:\Projects\cursor-xiigen-smart-skills`.
- User-facing entrypoint candidates in package metadata: `command:xiigen.smartSkills.select`, `command:xiigenSmartSkills.panel`, `chat:xiigen-smart-skills`.

Thin adapter path candidate:

- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Projects\cursor-xiigen-smart-skills\dist\extension.js`

Command/package/test path candidate:

- `C:\Projects\cursor-xiigen-smart-skills\package.json`
- `C:\Projects\cursor-xiigen-smart-skills\test\host-qa\HOST-08B-extensionHostQA.js`
- `C:\Projects\cursor-xiigen-smart-skills\test\host-qa\HOST-08C-chatParticipantDiscovery.js`
- `C:\Projects\cursor-xiigen-smart-skills\test\host-qa\HOST-08D-chatHandlerLevelQA.js`
- `C:\Projects\cursor-xiigen-smart-skills\test\host-qa\HOST-08H-cursorPlanSkillScenarioQA.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\install-smoke.ps1`

Required fresh evidence files for next runtime phase:

- `cursor-runtime-proof.md/json` or refreshed `cursor-runtime-qa-report.md/json`.
- Fresh Cursor launch summary, stdout/stderr logs, extension-host evidence directory, request/response/core trace/host trace/entrypoint response refs, visible command/chat/panel output, raw/redacted input/output refs, same-run artifact freshness check, and controlled error-path artifacts.

Exact missing field / execution condition:

- `fresh_runtime_launch_in_this_slice=false`; RUNTIME-01 did not launch Cursor.
- `full_chat_participant_ui_invocation` remains distinct from handler-level and discovery evidence; current executable candidate is strongest for command/extension-host path.
- `runtime_path_selection` must be explicit in the next work order: command, panel, or chat participant.

Next phase:

- `RUNTIME-03` if the command/Extension Host path is accepted as the next runtime target.
- `HOST-IMPL-02` only if parent requires a missing/incomplete chat participant UI path rather than command/panel runtime evidence.

Boundary notes:

- The Cursor adapter registers command, panel, and chat participant paths and delegates through `runCursorSmartSkillsRequest`.
- Handler-level/test-only evidence is correctly labelled and must not be used as full chat UI proof.

## Codex

Repo/path: exists at `C:\Projects\codex-xiigen-smart-skills`.

Real host runtime/chat/UI/CLI entrypoint candidate:

- Executable exists: `C:\Users\lubao\AppData\Local\OpenAI\Codex\bin\f1c7ee7a13db5fed\codex.exe`.
- Candidate command shape from prior packet: `codex exec --json --dangerously-bypass-approvals-and-sandbox -C C:\Projects\codex-xiigen-smart-skills --add-dir C:\Projects\llm_mvp_core -o <last-message> -`.
- Plugin metadata candidate: `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`.

Thin adapter path candidate:

- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1`

Command/package/test path candidate:

- `C:\Projects\codex-xiigen-smart-skills\package.json`
- `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- `C:\Projects\codex-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-08F-codexComponentQA.js`
- `C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js`

Required fresh evidence files for next runtime phase:

- Refreshed `codex-runtime-proof.md/json` or `codex-runtime-qa-report.md/json`.
- Fresh Codex command transcript/stdout JSONL/stderr log/last-message, source-entrypoint request/response/core trace refs, visible answer/debug/error text, raw/redacted input/output refs, same-run artifact freshness check, and controlled error-path artifacts.

Exact missing field / execution condition:

- `fresh_runtime_launch_in_this_slice=false`; RUNTIME-01 did not launch Codex.
- `full_codex_plugin_chat_ui_invoke_path` remains missing; current executable candidate is Codex CLI `exec`, not a full plugin chat UI proof.
- `available_skills_parser_rerun_after_repair` is required for any SkillSelection runtime claim. Current source maps `--available-skills-json`, and parser repair evidence passed, but that is supporting evidence until RUNTIME-04 is rerun.

Next phase:

- `RUNTIME-04` if Codex CLI exec source-entrypoint runtime proof is the accepted target and the next work order explicitly reruns after the parser repair.
- `HOST-IMPL-03` only if parent requires full Codex plugin/app chat UI invocation rather than CLI exec plus source-entrypoint evidence.

Boundary notes:

- The inspected Codex adapter constructs `XiigenExtensionRequest`, invokes the core endpoint, and records no host semantic ranking, model training, corpus parsing, DPO generation, or learned model mutation.
- Component QA and parser repair remain supporting evidence only, not full Codex runtime readiness.

## Recommended Next Bounded Work Orders

1. `RUNTIME-02` - Claude runtime QA, if the phase-scoped Claude Code `--plugin-dir` path is accepted.
2. `RUNTIME-03` - Cursor runtime QA, if the command/Extension Host path is accepted and the next work order names command/panel/chat target explicitly.
3. `RUNTIME-04` - Codex runtime QA rerun after parser repair, if Codex CLI exec source-entrypoint runtime proof is accepted.

If parent requires persistent host installation or full chat UI/plugin invocation for any host before runtime proof, use `HOST-IMPL-01`, `HOST-IMPL-02`, or `HOST-IMPL-03` for that host instead.

## Code/Architecture Review Notes

- Boundary risk: old runtime and adapter reports can tempt overclaim. They must be used as planning evidence only unless the next runtime phase refreshes them.
- Boundary risk: Cursor handler-level tests and Codex component QA are valuable, but they are not live UI/plugin proof.
- Boundary risk: Claude has both `.claude-plugin\plugin.json` and empty `hooks\hooks.json`; next runtime work must name the exact hook registration source.
- Boundary risk: Codex parser repair supports future SkillSelection evidence, but RUNTIME-04 must be rerun before any SkillSelection runtime claim.

## Verification Commands For This Slice

- `rg --files C:\Projects\claude-xiigen-smart-skills C:\Projects\cursor-xiigen-smart-skills C:\Projects\codex-xiigen-smart-skills`
- `rg -n "RUNTIME-01|HOST-IMPL|RUNTIME-02|RUNTIME-03|RUNTIME-04|entrypoint" ...`
- PowerShell `Test-Path` checks for the three host executables and inspected config/report files.
- JSON parse checks for `runtime-01-host-reality-map.json` and execution state.
- `git diff --check`

Verification result: PASS. `runtime-01-host-reality-map.json` parsed, execution state parsed, and `git diff --check` returned cleanly.
