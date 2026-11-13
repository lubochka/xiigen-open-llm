# TEST_ONLY Handler Ref Boundary Fix

status: TESTONLY_HANDLER_REF_BOUNDARY_FIX_READY_FOR_REVIEW

changed files:
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\WEBVIEW-SUBMIT-nodeHandlerSmoke.js
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\WEBVIEW-SUBMIT-testOnlyHandlerQA.js

TEST_ONLY evidence fields/names:
- response field: hostHandlerOnlyRef
- summary refs field: host_handler_only_ref
- artifact filename pattern: *-handler-only-*.json
- artifact kind: TEST_ONLY_HANDLER_ONLY_BOUNDARY
- absent from TEST_ONLY response, summary, and corrected run evidence: the legacy UI-named host-panel ref fields and UI-named panel artifact stem

real UI evidence fields/names preserved:
- existing real UI response ref field and panel artifact filename stem are unchanged in source/dist
- write path: writePanelUiArtifact(...)

tests run/results:
- node --check .\dist\extension.js
  result: PASS, exit code 0
- node --check .\test\host-qa\WEBVIEW-SUBMIT-nodeHandlerSmoke.js
  result: PASS, exit code 0
- node --check .\test\host-qa\WEBVIEW-SUBMIT-testOnlyHandlerQA.js
  result: PASS, exit code 0
- $stamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'); $env:WEBVIEW_SUBMIT_TESTABILITY_DIR = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\testonly-handler-ref-boundary-fix-$stamp"; node .\test\host-qa\WEBVIEW-SUBMIT-nodeHandlerSmoke.js
  result: PASS, exit code 0, node_handler_smoke_passed=true, core_result_faked=false, core_path_invoked=true
- negative token scan over corrected run directory for the legacy UI-named ref/file tokens
  result: PASS, exit code 0, no matches
- Select-String -Path 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\testonly-handler-ref-boundary-fix-20260625T115030Z\*' -Pattern 'hostHandlerOnlyRef','host_handler_only_ref','handler-only' -SimpleMatch
  result: PASS, exit code 0, expected matches present

corrected evidence refs:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\testonly-handler-ref-boundary-fix-20260625T115030Z\webview-submit-testability.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\testonly-handler-ref-boundary-fix-20260625T115030Z\xiigen-cursor-xiigenSmartSkills.panel-webview-submit-node-handler-1782388232044-positive_request-handler-only-2026-06-25T11-50-32-047Z.json

cleanup note:
- An initial smoke command used unsupported Get-Date -AsUTC on this PowerShell version and wrote to testonly-handler-ref-boundary-fix- with an empty suffix. That generated empty-suffix evidence directory was removed after resolving the exact path under the run evidence parent.

what remains:
- No remaining code/test work for this P1 ref boundary fix.
- The actual VS Code Extension Host QA harness was syntax-checked only; it was not run as an Extension Host session in this packet.

git:
- No git action run.
