# CURSOR_EXTENSION_LOAD_DIAGNOSTIC_PACKET

status: LOAD_FIX_ACCEPT_CANDIDATE

changed_files:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\launch-visible-cursor-row12-corrected.ps1
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-load-diagnostic.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-load-diagnostic.md

commands_run:
- Read package.json, src/extension.ts, dist/extension.js, corrected-launch evidence, renderer/main/exthost logs, and the existing command-availability packet.
- PowerShell AST parse of launch-visible-cursor-row12-corrected.ps1: PASS.
- package.json ConvertFrom-Json plus targeted main/command/activation checks: PASS.
- Select-String command registration check in src and dist: PASS.
- Path-length comparison of old deep user-data path versus new short temp path: PASS.

diagnosis:
The extension package itself is not the blocker found here. package.json has main `./dist/extension.js`, engines.vscode `^1.90.0`, activation event `onCommand:xiigenSmartSkills.panel`, and command contribution `XIIGen: Open Smart Skills Panel`. Source and dist both register `xiigenSmartSkills.panel`.

The corrected launch did pass the extensionDevelopmentPath intact and Cursor logged `Loading development extension at c:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`. But that same corrected launcher still used the very deep row12 evidence folder for Cursor `--user-data-dir` and `--extensions-dir`.

The host logs then showed repeated Cursor storage and extension-host failures: `SQLITE_CANTOPEN` for `state.vscdb`, extension host startup timeout/unresponsive status, and multiple extension-host output log creation failures. The old workspace state DB path length was 264 characters. A short temp user-data root reduces the comparable path to 153 characters.

fix_summary:
The evidence launcher now uses a fresh short temp root for Cursor volatile state and keeps the evidence record in the row12 run folder. It also passes the development extension path using equals-form: `--extensionDevelopmentPath=<full path>`.

verification_results:
- launcher_ast_parse: PASS
- package_json_parse: PASS
- panel_command_title_match_count: 1
- panel_activation_event_match_count: 1
- source_dist_registration_check: PASS
- node --check dist/extension.js: NOT_RUN_NOT_TOUCHED
- npm run install-smoke: NOT_RUN_PACKAGE_SOURCE_DIST_NOT_TOUCHED
- UI command availability after fix: NOT_RUN

evidence_files:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-load-diagnostic.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\command-availability-after-corrected-launch.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\user-data\logs\20260625T112610\main.log
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\user-data\logs\20260625T112610\window1\renderer.log
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\user-data\logs\20260625T112610\window1\exthost\exthost.log

next_smallest_action:
Run the repaired launcher once, then perform only the narrow command-palette availability check for `XIIGen: Open Smart Skills Panel` without row12 submit.
