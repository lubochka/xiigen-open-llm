# XIIGen Webview Control Path Diagnostic

status: CONTROL_PATH_NOT_FOUND_NEEDS_CODE_HOOK

No row12 prompt typed. No submit clicked. No harmless x typed. No git action run.

## Target Window Facts

- Exact row12 run-data host: [Extension Development Host] cursor-xiigen-smart-skills - Cursor, PID 50888, handle 0x89C035A, monitor \\.\DISPLAY1, bounds left -1920 top 0 right -640 bottom 720. Screenshot: screenshot-pid50888-h89C035A-Extension_Development_Host_cursor-xiigen-smart-skills_-_Cursor.png.
- Later temp row12 devhost: PID 60424, handle 0x3430720, monitor \\.\DISPLAY2, bounds left 60 top 60 right 1460 bottom 960. Screenshot shows Cursor Error dialog obstruction and no confirmed XIIGen panel.
- Later temp row12 devhost: PID 69924, handle 0x2D15CC, monitor \\.\DISPLAY2, bounds left 320 top 140 right 1600 bottom 940. Screenshot shows Cursor Error dialog obstruction and no confirmed XIIGen panel.
- Obstructing dialog: title Error, PID 54684, handle 0x2FC0F48, from separate headless extensionTestsPath process. It was recorded but not closed.

## Routes Checked

1. UIA: Not available for textarea. Control and Raw views expose only WebView/RootWebArea; no 	extarea, Edit, or submit button control surfaced. See uia-raw-tree-summary.json and per-window UIA tree files.
2. Keyboard/click: Not proven. The XIIGen panel input was not visible, so no deterministic coordinates or Tab sequence were established. No click/typing mutation was performed.
3. CDP/DevTools: Not available. No Cursor-owned listening port and no /json/list or /json/version response on common DevTools ports. See cursor-related-listening-ports-rerun.json and devtools-json-probe-rerun.json.
4. VS Code/Cursor command/API: Command xiigenSmartSkills.panel exists, but current implementation runs
unCursorSmartSkillsRequest before creating the webview panel, so it is not a pure safe reveal/focus route under the do-not-submit constraint. No non-TEST_ONLY focus command was found. See extension-command-surface.json and extension-panel-command-excerpt-lines-115-140.txt.

## Proven / Not Proven

Proven: host windows were identified; row12 host PID 50888 is visible on DISPLAY1; UIA does not expose the textarea; CDP is not reachable; extension code defines 	extarea#xiigen-smart-skills-prompt but no live route to it was proven.

Not proven: XIIGen panel visibility in a logged-in host; deterministic focus into textarea; row12 prompt typing; any readiness claim.

## Next Subagent Work Order Recommendation

Use a smaller prerequisite step before typing: either attach to a fresh host launched with usable webview DevTools/CDP and prove DOM selector 	extarea#xiigen-smart-skills-prompt, or get explicit approval for a tiny non-submit focus/reveal hook. Do not invoke xiigenSmartSkills.panel as a pure reveal route unless Luba accepts its current side effect:
unCursorSmartSkillsRequest occurs before createWebviewPanel.
