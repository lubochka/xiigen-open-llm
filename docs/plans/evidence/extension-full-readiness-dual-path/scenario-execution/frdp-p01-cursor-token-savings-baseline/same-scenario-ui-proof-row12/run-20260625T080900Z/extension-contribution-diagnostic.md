CURSOR_EXTENSION_CONTRIBUTION_DIAGNOSTIC_PACKET
status: CONTRIBUTION_DIAGNOSTIC_NEEDS_MORE_SPLIT

changed_files:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-contribution-diagnostic.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-contribution-diagnostic.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-open-smart-skills-label-check.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-open-smart-skills-panel-label-check.png

commands_run:
- Get-Content package.json / src\extension.ts / dist\extension.js: PASS
- Get-Content command-availability-after-responsive-host.json/md: PASS
- Get-Content corrected-launch-record.json: PASS
- rg targeted xiigen/manifest/activation/error markers in fresh short-temp Cursor logs: PASS_WITH_DIAGNOSTIC_FINDINGS
- ConvertFrom-Json package.json and targeted package command/activation/main check: PASS
- node package JSON/main/contribution check: PASS
- view_image current and prior screenshots: PASS, with prior panel-open screenshot mismatch found
- PowerShell UI probe search_text='Open Smart Skills Panel': PASS_NEGATIVE_NO_ROW12_SUBMIT
- Get-Date -AsUTC -Format o: FAILED_LOCAL_POWERSHELL_VERSION_NO_PRODUCT_EFFECT
- [DateTime]::UtcNow.ToString('o'): PASS

diagnosis:
- Cursor 3.8.23 is based on VS Code 1.105.1, so package `engines.vscode: ^1.90.0` is compatible.
- The corrected launch record points to `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`.
- Fresh renderer logs say Cursor loaded the development extension path.
- `package.json` parses, `main` exists, `contributes.commands` contains `xiigenSmartSkills.panel`, and `activationEvents` contains `onCommand:xiigenSmartSkills.panel`.
- `src\extension.ts` and `dist\extension.js` both register `xiigenSmartSkills.panel`.
- The responsive-host command-palette check for `XIIGen: Open Smart Skills Panel` is negative.
- A second UI check for `Open Smart Skills Panel` is also negative, so this is not explained by category/title rendering.
- Fresh exthost logs do not show XIIGen activation; that is expected if the command contribution never appears or invokes.
- The older `xiigen-smart-skills-panel-opened.png` evidence does not show the XIIGen panel; it shows Cursor Settings / Rules, Skills, Subagents.

fix_summary:
No package/source/dist/launcher fix was applied. Changing activation to `*` or changing titles/categories would be speculative from this evidence.

verification_results:
- package_json_parse: PASS
- package_panel_contribution_present: PASS
- package_panel_activation_present: PASS
- package_main_exists: PASS
- source_registers_panel_command: PASS
- dist_registers_panel_command: PASS
- cursor_version_engine_compatible: PASS
- exact_title_palette_probe: NEGATIVE
- prefix_free_palette_probe: NEGATIVE
- row12_submit_run: false
- node_check_dist_extension_js: NOT_RUN_SOURCE_DIST_NOT_TOUCHED
- npm_run_install_smoke: NOT_RUN_SOURCE_DIST_PACKAGE_NOT_TOUCHED
- launcher_ast_parse: NOT_RUN_LAUNCHER_NOT_TOUCHED

evidence_files:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\extension-contribution-diagnostic.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\corrected-launch-record.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-after-short-temp-launch.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-open-smart-skills-label-check.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-xiigen-open-smart-skills-panel-after-short-temp-launch.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-open-smart-skills-panel-label-check.png
- C:\Users\lubao\AppData\Local\Temp\xiigen-row12-cursor-devhost-20260625115339\user-data\logs\20260625T115343\window1\renderer.log
- C:\Users\lubao\AppData\Local\Temp\xiigen-row12-cursor-devhost-20260625115339\user-data\logs\20260625T115343\window1\exthost\exthost.log

next_smallest_action:
Run a bounded extension-host API probe in the same Cursor dev-host shape using `--extensionTestsPath` or equivalent to query `vscode.extensions.getExtension('xiigen.cursor-xiigen-smart-skills')` and `vscode.commands.getCommands(true)` for `xiigenSmartSkills.panel`. That split can distinguish manifest-admission failure from command-palette filtering without row12 submit.
