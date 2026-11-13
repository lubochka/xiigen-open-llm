# Row12 Open/Focus Hook Repair Evidence Packet

status: OPEN_FOCUS_FIX_READY_FOR_REVIEW

changed files:
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts
- C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js

exact command changed:
- xiigenSmartSkills.panel

exact behavior:
- Opens the XIIGen Smart Skills webview panel when no panel exists.
- Reveals the existing XIIGen Smart Skills webview panel when it already exists.
- Requests focus for textarea#xiigen-smart-skills-prompt via xiigenSmartSkills.focusPrompt.
- Runs no core request, no submit, no DPO/training action, and writes no panel UI evidence artifact during open/reveal.

submit path preserved:
- button#xiigen-smart-skills-submit still posts xiigenSmartSkills.submitPrompt.
- handleTrustedPanelSubmitMessage still invokes runCursorSmartSkillsRequest only for the webview submit path.

how this fixes Darwin's blocker:
- The previous xiigenSmartSkills.panel command invoked runCursorSmartSkillsRequest before createWebviewPanel.
- The command now creates/reveals the webview first and never calls runCursorSmartSkillsRequest in the open/reveal path.

tests run/results:
- node --check C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js: PASS
- Node mocked-vscode command smoke: PASS
  - registeredPanelCommand=true
  - createdCount=1
  - revealedOnSecondOpen=true
  - spawnCalled=false
  - htmlHasTextareaId=true
  - htmlHasSubmitId=true
  - htmlHasFocusHandler=true
  - htmlSaysNoCoreOnOpen=true
  - postMessageTypes=["xiigenSmartSkills.focusPrompt","xiigenSmartSkills.focusPrompt"]
- Static panel/open guard: PASS
  - panelCommandFound=true
  - panelOpenCommandCallsCoreBeforeReturn=false
  - submitHandlerStillCallsCore=true

what remains for real UI proof:
- Launch Cursor/extension host locally.
- Run command xiigenSmartSkills.panel from the real command palette or automation path.
- Verify the real webview is visible.
- Verify textarea#xiigen-smart-skills-prompt is focused and can receive typed text from UI automation.
- Only then click button#xiigen-smart-skills-submit for the same-scenario UI proof and verify the shared trusted handler/core path.

no git action run: true
