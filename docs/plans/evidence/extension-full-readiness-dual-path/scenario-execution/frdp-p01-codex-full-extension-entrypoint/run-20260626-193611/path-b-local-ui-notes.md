# Path B local UI notes

Scenario: Сценарий 20 / frdp-p01-codex-full-extension-entrypoint
Run root: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint\run-20260626-193611

What was attempted:
- Located a real local Codex Desktop process/window with title `Codex`.
- Captured `path-b-codex-window-process.json`.
- UIAutomation was attempted. It exposed only outer window/chrome controls, not the internal Codex app controls needed to create a clean thread or send a prompt.
- Local app/process metadata was inspected for a supported shortcut or route, but no exact controllable new-thread/send path was established.
- Because a clean UI route could not be controlled, I did not send the Row20 prompt in Codex Desktop.

Blocker:
`CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`

Safety/non-claims:
- no secrets captured: no tokens/passwords were entered or requested; no login flow was attempted.
- Path B did not prove installed plugin route visibility or invocation in Codex UI.
- Path B did not prove product readiness or full Codex chat UI readiness.

Redaction note:
- The initially captured visible state showed the local Codex Desktop window and a Docker Desktop window. Because it included private local UI/chat/workspace details, the stored path-b-codex-ui-capture.png was redacted after inspection. The screenshot remains blocker evidence, not a route PASS.
