# ROW12_CORRECTED_LAUNCH_COMMAND_CHECK_PACKET

status: COMMAND_STILL_MISSING_WITH_EVIDENCE

run_folder: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z

files_created:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\launch-visible-cursor-row12-corrected.ps1
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\command-availability-after-corrected-launch.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\command-availability-after-corrected-launch.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\corrected-launch-record.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-after-corrected-launch.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-after-corrected-launch-f1.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\logs\command-palette-probe-after-corrected-launch-typed.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-xiigen-open-smart-skills-panel-corrected-launch.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-xiigen-open-smart-skills-panel-corrected-launch-f1.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-xiigen-open-smart-skills-panel-corrected-launch-typed.png

launch_fix_summary:
The corrected launcher uses a PowerShell argument array and native invocation:
`& $cursorExe @arguments`. The array contains `--extensionDevelopmentPath` and
`C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`
as separate entries.

Corrected host process `50888` shows the full path intact in the command line:
`--extensionDevelopmentPath "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"`.
The corrected host exthost log also uses the intended folder as the search cwd:
`c:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`.

command_availability_evidence:
The real local Cursor Extension Development Host was targeted by process/window:
`[Extension Development Host] cursor-xiigen-smart-skills - Cursor`, PID `50888`,
window handle `144442202`. The target-window probe opened the Command Palette
with `F1`, clicked the palette input, and typed the exact search text
`XIIGen: Open Smart Skills Panel`.

The saved screenshot shows the exact query and only similar commands:
`Cursor Settings: Rules, Skills, Subagents` and `View: Toggle Panel Visibility`.
The exact command title `XIIGen: Open Smart Skills Panel` is not visible.
UI Automation also did not expose the exact command title in accessibility
names.

screenshot_status:
Captured:
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\screenshots\command-palette-xiigen-open-smart-skills-panel-corrected-launch-typed.png`

No row12 submit was run. The panel was not opened.

human_explanation:
The real launch-argument blocker was repaired: the development path with the
space is now passed intact and visible intact in the corrected Cursor process.
After that corrected relaunch, the command-palette check still did not show the
XIIGen command. The supported result is therefore
`COMMAND_STILL_MISSING_WITH_EVIDENCE`, not readiness.

next_smallest_action:
Inspect why the corrected Extension Development Host does not register/load the
XIIGen extension contribution despite the intact `--extensionDevelopmentPath`.
Do not run row12 submit.
