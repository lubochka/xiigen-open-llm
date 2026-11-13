# RUNTIME-04E Codex Installed Skill Invocation Surface Proof

Verdict: PASS_INSTALLED_CODEX_SKILL_SURFACE_INVOCATION_PROVEN_CHAT_APP_NOT_PROVEN.

This slice added the smallest supported installed Codex surface found locally: a plugin `skills` entry and one `xiigen-smart-skills` skill. After cachebusting and reinstalling `codex-xiigen-smart-skills@xiigen-local`, a fresh `codex exec` session loaded the installed skill, read its `SKILL.md`, and ran the existing thin adapter from the installed plugin cache.

## Proven

- `codex plugin add codex-xiigen-smart-skills@xiigen-local --json` installed version `0.1.0+codex.runtime04e-20260616-231500`.
- `codex plugin list --json` shows the plugin installed and enabled.
- The installed cache contains `.codex-plugin/plugin.json` with `skills: "./skills/"`.
- The installed cache contains `skills/xiigen-smart-skills/SKILL.md`.
- Nested Codex exec reported using `codex-xiigen-smart-skills:xiigen-smart-skills`.
- Positive smoke command exited `0`.
- Positive response status is `core_boundary_partial_no_semantic_quality_claim`.
- Positive trace call target is `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Redaction flags show no raw private prompt persisted and no secrets.
- Controlled negative path passed: missing core script produced a bounded adapter error with request written and no response/trace.

## Not Proven

- Direct `codex plugin invoke` or equivalent CLI surface was not found.
- `codex app` exposes app launch only, not a noninteractive installed-plugin call.
- Full Codex chat UI or app UI invocation remains unproven.
- Token savings, quality improvement, learnability, semantic quality, training, eval, checkpoint mutation, and full product readiness remain unproven.

## Caveat

The nested PowerShell command did not preserve custom `HostFacts`, `CheckSetInputs`, or custom `Provenance` JSON; the request retained the prompt, requested/available skill, correlation id, debug flag, response, and trace. This is an argument-quoting hardening gap, not a hidden model shortcut.

## Evidence

- Proof JSON: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-04e.json`
- Plugin summary: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\runtime04e-installed-skill-surface-summary.json`
- Plugin add: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\rerun-codex-plugin-add-xiigen-local-cachebuster.stdout.txt`
- Plugin list: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\rerun-codex-plugin-list-json-after-cachebuster.stdout.txt`
- Positive Codex exec: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\codex-exec-installed-skill-smoke.stdout.jsonl`
- Positive last message: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\codex-exec-installed-skill-smoke.last-message.txt`
- Positive request/response/trace: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\RUNTIME-04E-codex\20260616-231500\installed-skill-codex-exec-smoke\positive\`
- Negative Codex exec: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\codex-exec-installed-skill-negative.stdout.jsonl`
- Negative last message: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04E-codex\20260616-231500\codex-exec-installed-skill-negative.last-message.txt`

## Next Work Order

RUNTIME-04F-CODEX-APP-OR-CHAT-UI-SURFACE-PROOF: if required, identify a documented Codex app/chat UI invocation surface or `.app.json` schema and prove direct app/chat UI invocation. Also harden the installed-skill smoke with a tiny argument wrapper or response schema so custom `HostFacts`, `CheckSetInputs`, and `Provenance` survive Windows PowerShell quoting.
