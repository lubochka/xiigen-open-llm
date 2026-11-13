# Parent Review RUNTIME-04E

Verdict: PASS_ACCEPTED_PENDING_PARENT_COMMIT.

Reviewed at: 2026-06-16T23:28:04+03:00

Review role: bounded parent-review subagent. Review only; no product implementation and no commit/push.

## Decision

RUNTIME-04E is accepted for the phase commit boundary. The evidence proves a real installed Codex skill surface invocation through nested `codex exec`, loading the installed skill from the cache and running the existing thin Codex adapter from the installed plugin cache into `llm_mvp_core`.

The accepted scope is narrow: installed skill surface plus adapter/core boundary. Direct plugin CLI invocation, Codex chat UI, Codex app UI, token savings, quality improvement, learnability, semantic quality, training, eval, checkpoint mutation, and full readiness remain unproven.

## Accepted Evidence

- Installed plugin manifest exists in source and installed cache with `skills: "./skills/"`.
- Installed skill file exists in source and installed cache at `skills/xiigen-smart-skills/SKILL.md`.
- Positive nested `codex exec` JSONL parses and reports use of `codex-xiigen-smart-skills:xiigen-smart-skills`.
- Positive last message reports `installed_skill_surface_loaded=true`, `adapter_core_boundary_executed=true`, and `installed_plugin_runtime_invocation_proven=true`.
- Positive request, response, and trace JSON parse successfully.
- Positive trace records `call_target` as `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Positive response status is `core_boundary_partial_no_semantic_quality_claim`.
- Negative nested `codex exec` JSONL parses and shows the installed skill instructions were read before the negative smoke.
- Negative last message reports adapter exit code `1`, request exists, response and trace do not exist, and the missing core script error is bounded.
- `src/codex-entrypoint.js` remains a thin adapter that writes `XiigenExtensionRequest`, invokes `tools/xiigen-extension-core-api.ps1`, and keeps host-side training/ranking/DPO/checkpoint/external-AI flags false.
- `SKILL.md` is a thin invocation surface with explicit no-overclaim and no-hidden-logic instructions.

## Rejected Or Unproven Claims

- Direct `codex plugin invoke` or equivalent CLI invocation is not proven.
- Codex chat UI invocation is not proven.
- Codex app UI invocation is not proven.
- Token savings, quality improvement, learnability, semantic quality, training, eval, checkpoint mutation, and full product readiness are not proven.
- Custom `HostFacts`, `CheckSetInputs`, and custom `Provenance` argument preservation is not proven; the evidence correctly records this as a Windows PowerShell quoting hardening gap.

## Authorship Gate

The accepted implementation/proof changes are attributed to subagent `RUNTIME-04E-CODEX-INSTALLED-PLUGIN-INVOCATION-SURFACE-PROOF` as recorded in `codex-runtime-proof-04e.json`. This review subagent only created the parent review artifacts and adjusted execution-state review status/next action. No product code implementation was authored in this review pass.

## Next Work Order

First: parent-owned commit boundary for the accepted RUNTIME-04E file set. Do not start further runtime/app/chat work before that boundary unless Luba explicitly changes the order.

Exact accepted commit scope:

- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-04e.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-04e.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\parent-review-RUNTIME-04E.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\parent-review-RUNTIME-04E.json`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- `C:\Projects\codex-xiigen-smart-skills\skills\xiigen-smart-skills\SKILL.md`
- `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\**`

After commit: `RUNTIME-04F-CODEX-APP-OR-CHAT-UI-SURFACE-PROOF`, only if direct Codex app/chat UI invocation is still required. Argument-passing hardening should be included in RUNTIME-04F if the app/chat work needs the same smoke harness, or split into a separate tiny repair if the parent wants a smaller phase before app/chat exploration.

## Verification

- JSON parse proof/review/state/key Codex summary: PASS.
- Test-Path plugin manifest, skill file, installed manifest, installed skill, positive evidence, and negative evidence: PASS.
- `rg` no-overclaim/core-boundary/hidden-logic checks: PASS.
- `git diff --check` in `C:\Projects\llm_mvp_core`: PASS.
- `git diff --check` in `C:\Projects\codex-xiigen-smart-skills`: PASS.
- Product tests: not run; not authorized by this review work order.
