# Cursor Command Registration Repair Packet

packet_type: CURSOR_COMMAND_REGISTRATION_REPAIR_PACKET

status: COMMAND_ALREADY_REGISTERED_NEEDS_HOST_RELOAD

## Scope

- active_task_sentence_source: `CURRENT TASK: Diagnose and repair why the Cursor command XIIGen: Open Smart Skills Panel / command id xiigenSmartSkills.panel is not visible in the active Cursor Extension Development Host command palette.`
- git_authority: forbidden
- commit_push_authorized: false
- requested_model: gpt-5.5
- requested_reasoning_effort: xhigh
- mode_verified_from_work_order: true
- external_ai_used: false
- full_row12_ui_proof_rerun: false
- readiness_claimed: false

## Diagnosis

The Cursor extension package already contributes and registers the command:

- `package.json` has `activationEvents` entry `onCommand:xiigenSmartSkills.panel`.
- `package.json` contributes command id `xiigenSmartSkills.panel` with title `XIIGen: Open Smart Skills Panel` and category `XIIGen`.
- `package.json` main entry is `./dist/extension.js`.
- `src/extension.ts` registers `vscode.commands.registerCommand("xiigenSmartSkills.panel", ...)`.
- `dist/extension.js` registers `vscode.commands.registerCommand("xiigenSmartSkills.panel", ...)`.

No command id/title mismatch was found in package, source, or dist.

The real blocker is the active dev-host launch path. Row12 captured this command line:

```text
--extensionDevelopmentPath C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills
```

The path contains a space in `extensions reasearch`. Cursor's renderer log then reported:

```text
Error scanning extensions at /C:/Users/lubao/Documents/xiigen/extensions: Unable to resolve nonexistent file 'c:\Users\lubao\Documents\xiigen\extensions'
```

That is the extension path split at the first space, so Cursor scanned `C:\Users\lubao\Documents\xiigen\extensions` instead of the actual extension folder. Because the development extension was not loaded from the intended folder, its contributed commands were absent from the command palette. Pressing Enter after the XIIGen search therefore opened Cursor's built-in `Rules, Skills, Subagents` settings command.

## Repair

No package/source/dist edit is needed for command registration. The required repair is to relaunch/reload Cursor Extension Development Host with `--extensionDevelopmentPath` passed as one intact argument, for example through a PowerShell argument array or explicit quoting:

```powershell
$cursorArgs = @(
  "--new-window",
  "--user-data-dir", $UserDataDir,
  "--extensions-dir", $ExtensionsDir,
  "--extensionDevelopmentPath", "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills",
  "--disable-workspace-trust",
  "--skip-welcome",
  "--skip-release-notes",
  "--skip-onboarding",
  "--disable-telemetry",
  "--disable-updates",
  "--disable-gpu",
  "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
)
Start-Process -FilePath $CursorExePath -ArgumentList $cursorArgs
```

The next host launch evidence must show Cursor no longer logs `Error scanning extensions at /C:/Users/lubao/Documents/xiigen/extensions`, and the command palette should then expose `XIIGen: Open Smart Skills Panel`.

## Verification Results

- `node --check C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`: PASS.
- `npm run install-smoke`: PASS, exit code 0.
- `package.json` JSON parse: PASS.
- Row12 evidence JSON parse for `row12-cursor-ui-proof.json`, `panel-open-attempt.json`, `cursor-process-command-line.json`, and `safe-host-log-search-summary.json`: PASS.
- Targeted literal command-id check in package/source/dist: PASS.
- Initial `rg` regex check failed due local shell quoting of the pattern; it was rerun with `Select-String -SimpleMatch` and passed. The failed grep is not product evidence.

## Changed Files

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\command-registration-repair.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\command-registration-repair.json`

## Verification Outputs Written By Install Smoke

- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\install-smoke-report.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\install-smoke-trace.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-off\xiigen-cursor-core-response.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-off\xiigen-cursor-core-trace.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-off\xiigen-cursor-entrypoint-response.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-off\xiigen-cursor-host-trace.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-off\xiigen-cursor-request.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-on\xiigen-cursor-core-response.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-on\xiigen-cursor-core-trace.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-on\xiigen-cursor-entrypoint-response.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-on\xiigen-cursor-host-trace.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\reports\HOST-IMPL-CURSOR-01\install-smoke\debug-on\xiigen-cursor-request.json`

## Remaining Gaps

- I did not rerun full row12 UI proof.
- I did not run browser login or auth recovery.
- I did not prove the active host command palette after relaunch.
- I did not run `git diff --check` because the work order sets `allowed_git_commands = NONE`.
- No readiness, token savings, semantic quality, training, checkpoint promotion, or full host UI proof is claimed.

## Next Smallest Action

Relaunch or reload Cursor Extension Development Host with the quoted/intact `--extensionDevelopmentPath` argument, then run only the narrow command-palette availability check for `XIIGen: Open Smart Skills Panel`. If visible, row12 can resume from panel open/input/submit in its own authorized UI proof task.
