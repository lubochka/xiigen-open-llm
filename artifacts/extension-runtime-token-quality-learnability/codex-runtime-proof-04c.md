# RUNTIME-04C Codex Installed Plugin / Chat UI Proof

## Verdict

Classification: `GAP_SUPPORTED_INSTALLED_PLUGIN_OR_CHAT_UI_PROOF_NOT_COMPLETED`

I could not prove a supported installed Codex plugin/app/chat UI invocation path for `codex-xiigen-smart-skills`. The local proof remains bounded: RUNTIME-04B already proved the direct `codex.exe exec` source-entrypoint final answer artifact, while this 04C pass proves that the local CLI/plugin marketplace surface does not yet expose `codex-xiigen-smart-skills` as an installed or available plugin.

## Commands Attempted

- `codex --version`
  - result: failed from the Windows shim with `Access is denied`
- `C:\Users\lubao\AppData\Local\OpenAI\Codex\bin\f1c7ee7a13db5fed\codex.exe --version`
  - result: `codex-cli 0.140.0-alpha.2`
- `...\codex.exe plugin --help`
  - result: plugin surface exposes marketplace/list/add/remove management
- `...\codex.exe plugin list --json`
  - result: installed plugins listed; `codex-xiigen-smart-skills` absent
- `...\codex.exe plugin list --available --json`
  - result: available marketplace plugins listed; `codex-xiigen-smart-skills` absent
- `...\codex.exe plugin marketplace list`
  - result: configured marketplaces are `openai-primary-runtime`, `openai-bundled`, and `openai-curated`
- `...\codex.exe app --help`
  - result: launches Codex Desktop for a workspace path; no noninteractive plugin/chat invocation evidence
- `...\codex.exe chat --help`
  - result: no distinct chat subcommand; output fell back to root CLI help

## Evidence

- Run root: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04C-codex\20260616-223116`
- Command summary: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04C-codex\20260616-223116\command-surface-summary.json`
- Installed plugin JSON: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04C-codex\20260616-223116\direct-plugin-list-json.stdout.txt`
- Available plugin JSON: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04C-codex\20260616-223116\direct-plugin-list-available-json.stdout.txt`
- Marketplace list: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04C-codex\20260616-223116\direct-plugin-marketplace-list.stdout.txt`
- Extension packaging: `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- Adapter source reviewed: `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`

## What Is Proven

- The direct Codex binary works and reports `codex-cli 0.140.0-alpha.2`.
- Codex plugin management is marketplace based: plugin install requires `PLUGIN@MARKETPLACE` or `PLUGIN --marketplace MARKETPLACE`.
- `codex-xiigen-smart-skills` is absent from installed and available plugin JSON output for the configured marketplaces.
- The repository has a plugin manifest and thin source-entrypoint package metadata, but no configured marketplace snapshot exposes it to Codex.
- The adapter keeps the core boundary: it builds request JSON, invokes `llm_mvp_core`, and keeps host semantic/model/training flags false.

## What Is Not Proven

- Supported installed Codex plugin invocation.
- Codex chat UI invocation.
- Full Codex plugin/app readiness.
- Token savings, quality improvement, semantic quality, learnability, training, eval, checkpoint mutation, fresh-load, rollback, target promotion, or full product readiness.

## Gap

Local Codex can list configured plugin marketplaces and install plugins from them, but `codex-xiigen-smart-skills` is not present in those marketplaces. `codex app` can launch the desktop app, but it does not provide a noninteractive proof that this plugin was invoked in chat/app UI. No supported CLI `chat` invocation path was exposed.

## Next Work Order

`RUNTIME-04D-CODEX-LOCAL-MARKETPLACE-PACKAGING-PROOF`: create a supported local Codex marketplace snapshot for `codex-xiigen-smart-skills`, with `marketplace.json` and plugin package layout matching `codex plugin marketplace add` and `codex plugin add` requirements. Then run `codex plugin marketplace add <local-snapshot>`, `codex plugin add codex-xiigen-smart-skills@<marketplace>`, `codex plugin list --json`, and only after installed/enabled evidence exists attempt a supported plugin/app/chat invocation path. If Codex exposes no invocation API after install, produce a second gap packet naming the missing Codex app/chat invocation surface. Keep implementation limited to packaging metadata; do not edit core logic or claim token/quality/learnability/readiness.

## Verification

- JSON parse: passed for proof, state, command summary, gap summary, and key plugin list JSON evidence.
- `rg` marker checks: passed for gap verdict, installed-plugin/chat UI proof fields, no-overclaim fields, adapter boundary, and next work order.
- `git diff --check`: passed in both repositories.
