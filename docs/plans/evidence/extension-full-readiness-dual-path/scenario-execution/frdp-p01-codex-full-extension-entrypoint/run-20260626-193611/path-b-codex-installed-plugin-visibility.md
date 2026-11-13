# Path B Codex installed-plugin visibility

Result: `CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`

The local Codex Desktop window was present, but UIAutomation exposed only the outer window/chrome controls. I did not have a reliable controlled way to create a clean thread, paste the exact Row20 prompt, press Send once, and observe the response without risking blind input into the active chat. Therefore plugin/skill invocation was not proven in Codex UI.

Evidence available:
- `path-b-codex-window-process.json` shows a local Codex process/window.
- `path-b-codex-ui-capture.png` captures the visible local UI state at the blocker point.
- `path-b-codex-ui-readback.json` records the available UIA tree.
- `path-b-local-ui-notes.md` records the blocker and non-claims.

This is a blocker, not a PASS.
