# RUNTIME-04F Codex App/Chat UI Surface Proof

Verdict: `PARTIAL_PASS_APP_SERVER_CHAT_MESSAGE_AND_INSTALLED_SKILL_LOAD_PROVEN_GAPS_RECORDED`.

## What This Proves

- `codex debug app-server send-message-v2` is a supported local CLI path in this Codex build.
- The path initialized a Codex Desktop app-server session, started a thread, submitted a turn, and returned a final answer.
- During that turn, Codex loaded the installed `xiigen-smart-skills` skill instructions from the installed plugin cache.
- The app-server smoke returned `files_edited: false`.

## What This Does Not Prove

- Direct `codex plugin invoke`, `codex plugin run`, or `codex plugin list` is not available in this local `codex-cli 0.130.0-alpha.5` binary.
- `codex app` only launches the desktop app/open-workspace flow; it did not produce an automatable plugin/chat result.
- Full desktop Codex chat UI and desktop app UI automation remain unproven.
- The app-server smoke did not execute the XIIGen adapter/core boundary; `XiigenExtensionApiService` was not called in 04F.
- Token savings, quality improvement, learnability, semantic quality, training, eval, checkpoint mutation, and full readiness remain unproven.

## Evidence

- Proof JSON: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-04f.json`
- Command evidence index: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\runtime04f-command-evidence-index.json`
- Summary: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\runtime04f-app-server-chat-surface-summary.json`
- App-server answer-only smoke: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\codex-debug-app-server-send-message-v2-answer-only-smoke.stdout.txt`
- App launch probe: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\codex-app-launch-timeout.stderr.txt`
- Plugin CLI negative probes: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\codex-plugin-invoke-help.stderr.txt`, `codex-plugin-run-help.stderr.txt`, `codex-plugin-list-json.stderr.txt`
- App-server schema: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04F-codex\20260616-233400\app-server-json-schema\`

## Required Next Runtime Gap Pass

Parent review found that the app-server smoke proves a real experimental message surface and installed skill instruction load, but it does not prove the XIIGen adapter/core boundary through the app-server/chat surface. Next work order is `RUNTIME-04G-CODEX-APP-SERVER-ADAPTER-CORE-BOUNDARY-PROOF` before moving to `TOKEN-01`. Do not start token runners, quality eval, training, checkpoint mutation, or readiness claims from this packet.
