# ROW12_PANEL_INPUT_SUBMIT_DISCOVERY_PACKET

status: `PANEL_NO_INPUT_SUBMIT_PATH_IN_SOURCE`

run_folder:
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305`

## Files Created

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305\panel-input-submit-discovery.json`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305\panel-input-submit-discovery.md`

## Source Files Read

- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\package.json`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-instructions\row12-cursor-auth-preflight-same-scenario-ui-script.md`

## Source Inspection Result

The panel source has no visible row12 input field, textarea, contenteditable region, submit button, form, keyboard shortcut, webview message command, or submit handler.

Evidence:

- `package.json` lines 29-32 contribute `xiigenSmartSkills.panel` with title `XIIGen: Open Smart Skills Panel`.
- `src\extension.ts` lines 199-206 show the panel command uses optional programmatic input or a default prompt, then immediately calls `runCursorSmartSkillsRequest`.
- `src\extension.ts` lines 211-217 create a webview panel titled `XIIGen Smart Skills` with `{ enableScripts: false }`.
- `src\extension.ts` lines 1022-1027 show `buildPanelHtml` returns only output elements: `h1`, `p`, `pre`, `p`, `p`.
- `dist\extension.js` lines 89-107 and 874-879 match the compiled source behavior.
- `src\extension.ts` lines 117-128 contain a `vscode.window.showInputBox`, but it belongs to the separate `xiigen.smartSkills.select` command, not the panel webview.

Search terms were run across local `src` and `dist`: `createWebviewPanel`, `buildPanelHtml`, `enableScripts`, `<input`, `<textarea`, `<button`, `form`, `submit`, `acquireVsCodeApi`, `postMessage`, `onDidReceiveMessage`, `addEventListener`, `showInputBox`, `xiigenSmartSkills.panel`, `xiigen.smartSkills.select`, and `registerCommand`.

Only `src\extension.ts` and `dist\extension.js` exist under the inspected `src`/`dist` roots; no separate webview/html/js assets were found.

## Input Path

`input_path`: none found for the panel.

The only prompt entry UI found is:

- command: `xiigen.smartSkills.select`
- UI: `vscode.window.showInputBox`
- prompt: `Task for XIIGen smart-skill selection`
- placeholder: `Describe the concrete task to send to llm_mvp_core`
- source: `src\extension.ts` lines 117-128

That is not the `xiigenSmartSkills.panel` input path.

The panel command accepts optional `RuntimeProofCommandInput` when invoked programmatically, but that is not a visible panel input/submit path.

## Submit Path

`submit_path`: none found for the panel.

Reason: the webview has scripts disabled and static HTML only. There is no panel-side button, form, message bridge, event listener, or submit handler.

## Exact UI Action Sequence If Known

No exact row12 prompt entry and submit sequence is known, because no panel input/submit path exists in source. Do not invent a click.

Safe panel-open-only sequence supported by source/scenario:

1. Open Cursor Command Palette with `Ctrl+Shift+P`.
2. Type exactly: `XIIGen: Open Smart Skills Panel`.
3. Select/confirm the command titled exactly: `XIIGen: Open Smart Skills Panel`.
4. Press `Enter`.
5. Expected opened panel title/header: `XIIGen Smart Skills`.
6. Expected panel body is output-only status/text/`Response`/`Trace`; no row12 input control is expected from source.

## What Is Not Proven

- I did not run Cursor UI, browser, login, or the full row12 prompt scenario.
- This does not prove visible Cursor panel availability on the host session.
- This does not prove a Path B row12 UI result.
- This does not prove token savings, semantic quality, readiness, training, checkpoint promotion, or locator update.
- This does not prove that a programmatic command argument path is acceptable as visible UI proof.

## Human Explanation

Source inspection found the XIIGen Smart Skills panel is output-only. The panel can be opened from the Command Palette, and the command can receive prompt input only as a programmatic command argument. The webview itself has no visible field where a UI agent can type the row12 prompt and no visible control to submit it.

## Next Smallest Action

Plan/code repair, not UI run: either change the scenario to use a different approved visible UI path with exact controls, or implement/review a real panel input and submit control before any row12 prompt execution.
