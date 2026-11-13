# Row20 execution report

## Что проверял сценарий

Сценарий 20 `frdp-p01-codex-full-extension-entrypoint` проверял узкую готовность входной точки Codex extension: установленный plugin `codex-xiigen-smart-skills` должен провести один безопасный non-mutating smoke request через текущий `src/codex-entrypoint.js` до `llm_mvp_core` adapter/core boundary.

## Path A result

Status: `PASS_NARROW_BOUNDARY`.

Evidence files:
- `path-a-entrypoint-contract-review.json`
- `path-a-request.json`
- `path-a-response.json`
- `path-a-trace.json`
- `path-a-command-result.json`
- `path-a-stdout.txt`
- `path-a-stderr.txt`

Observed evidence:
- package name: `codex-xiigen-smart-skills`
- package main: `src/codex-entrypoint.js`
- JS entrypoint exists: `True`
- command exit code: `0`
- request method: `prompt-to-task`
- request correlation id matches Row20: `true`
- request prompt matches Row20: `true`
- selected skill: `xiigen-smart-skills`
- response status: `core_boundary_partial_no_semantic_quality_claim`
- trace external_ai_used: `False`
- trace training_started: `False`
- trace semantic_quality_claimed: `false`
- trace runtime_extension_claimed: `false`
- trace host_runtime_claimed: `False`
- trace claim_scope: `core-owned endpoint invocation only; no host runtime claim`

Path A proves only the installed-plugin JS entrypoint adapter/core boundary for this one Row20 smoke. Path A is not UI proof and not product readiness.

## Path B result

Status: `BLOCKED_CLEAN_SURFACE_NOT_PROVEN`.

Evidence files:
- `path-b-codex-window-process.json`
- `path-b-codex-ui-capture.png`
- `path-b-codex-ui-readback.json`
- `path-b-local-ui-notes.md`
- `path-b-codex-installed-plugin-visibility.md`

What happened:
- A real local Codex Desktop process/window was found.
- UIAutomation exposed only outer window/chrome controls, not the inner controls needed for a clean new-thread/send operation.
- I did not send the Row20 prompt through Codex UI because blind input into the active chat would not be clean evidence.
- A local visible state screenshot was captured, inspected, then redacted in the stored PNG to avoid preserving private local UI/chat/workspace details.

Blocker: `CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`.

Path B proves only that the local Codex Desktop window existed and that the UI route was blocked for this execution packet. It does not prove plugin invocation in Codex UI.

## Joined comparison

Same Row20 prompt/correlation was used for Path A. Path B did not reach prompt send, so same-scenario UI result remains not proven.

Narrow supported claim:
Path A supports only installed Codex plugin JS entrypoint adapter/core boundary execution for one non-mutating Row20 smoke request. Path B supports only a local UI blocker/visible-state capture.

## Non-claims

- `product_readiness_claimed=false`
- `full_joined_pass=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`
- `learnability_claimed=false`
- `training_started=false`
- `eval_started=false`
- `checkpoint_mutation_started=false`
- `checkpoint_promotion_claimed=false`
- `locator_update_claimed=false`
- `external_ai_used=false`
- `code_mutation_claimed=false`
- `git_action_claimed=false`

## Review state

Accepted execution review result: `PASS_SCOPED` only.

`row20_accepted=true` only for scoped installed Codex plugin JS entrypoint adapter/core boundary evidence. `formal_joined_pass=false`; Codex UI route, prompt sent through Codex UI, visible Codex UI response, full Codex chat UI readiness, product readiness, token savings, semantic quality, learnability/training, checkpoint promotion, and locator update remain not proven.

Independent execution review is no longer required for this Row20 scoped package. Independent packaging review is required next.
