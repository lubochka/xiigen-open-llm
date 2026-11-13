# Action log — Claude Path B route navigation recovery

## Scope

Bounded local UI route recovery only. No prompt submission. No repo tests/builds. No git stage/commit/push.

## Actions

1. Created evidence folder:
   `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-navigation-recovery-20260628-141558\`

2. Enumerated visible windows and Claude-like processes.
   - Evidence file: `initial-window-inventory.json`
   - Result: found Claude Desktop process/window.
   - Target window: title `Claude`, PID `15384`, HWND `0x103B8`.
   - Bounds: `Left=689 Top=160 Right=1905 Bottom=837 Width=1216 Height=677`.
   - Overlap noted: Cursor window `[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor`, HWND `0x13706DE`, overlapped target bounds.

3. Attempted to foreground target Claude window and captured screen region.
   - Screenshot: `screenshot-001-claude-foreground.png`
   - Result: capture included overlap, so it was not accepted as sufficient target evidence.

4. Ran second activation attempt and captured both screen and exact HWND capture.
   - Screen screenshot: `screenshot-002-claude-activate-screen.png`
   - Exact HWND screenshot: `screenshot-003-claude-printwindow.png`
   - Result: `PrintWindow` succeeded and showed Claude Desktop with modal `Claude is still working`.

5. Dumped shallow Windows UIAutomation tree for target Claude window.
   - Evidence file: `uia-dump-before-modal-cancel.json`
   - Result: Electron/Chromium accessibility tree exposed only coarse window/frame elements, not route DOM text.

6. Minimized overlapping Cursor development host window to expose target Claude window.
   - Window minimized: HWND `0x13706DE`
   - Screenshot: `screenshot-004-claude-after-minimize-cursor.png`
   - Result: target Claude window visible with modal.

7. Clicked `Cancel` on Claude modal `Claude is still working`.
   - Click: global coordinates `X=1266 Y=536`
   - Screenshot after click: `screenshot-005-claude-after-cancel-modal.png`
   - Result: modal closed. No prompt was submitted. The visible Claude sidebar showed `Recents > XIIGen Smart Skills UI proof run`.

8. Clicked recent route `XIIGen Smart Skills UI proof run`.
   - Click: global coordinates `X=821 Y=700`
   - Screenshot after click: `screenshot-006-claude-xiigen-route-opened.png`
   - Result: route opened in Claude Code view. Input field remained empty; no prompt was submitted.

## Exact result

`CLAUDE_ROUTE_RECOVERED`: Claude Desktop route/entrypoint `Code > Recents > XIIGen Smart Skills UI proof run` is visible and opens.
