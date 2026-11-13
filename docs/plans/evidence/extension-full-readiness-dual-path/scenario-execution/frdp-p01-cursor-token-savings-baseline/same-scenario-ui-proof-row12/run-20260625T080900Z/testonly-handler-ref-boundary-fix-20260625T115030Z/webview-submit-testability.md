# Webview Submit Testability Harness

Status: TEST_HARNESS_ACCEPT_CANDIDATE

This is TEST_ONLY handler-path evidence. It proves the shared trusted panel submit handler routes a supplied prompt through `runCursorSmartSkillsRequest` and the core API path with `entrypointType = panel-webview-submit`.

It does not prove a visible webview was observed, does not prove desktop automation can focus/type into the webview textarea, does not claim panel UI creation, full UI proof, or product readiness, and does not remove the need for later manual/real UI screenshot proof.

The focused Cursor Extension Host launch was attempted, but did not execute the test because Cursor startup logged `Error mutex already exists`. The Node smoke did not fake the core result; it used the extension's registered command and the real core API script.

JSON evidence: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\testonly-handler-ref-boundary-fix-20260625T115030Z\webview-submit-testability.json
