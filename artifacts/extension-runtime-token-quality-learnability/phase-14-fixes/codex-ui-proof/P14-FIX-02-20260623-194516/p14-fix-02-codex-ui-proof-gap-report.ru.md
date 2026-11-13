# P14-FIX-02 Codex UI proof-gap packet

## Вердикт

NEEDS_FIX for Phase 14 FR-03.

Свежий запуск доказал только установленный plugin/skill smoke: installed plugin adapter src/codex-entrypoint.js called llm_mvp_core through tools/xiigen-extension-core-api.ps1, and core returned positive plus controlled negative responses with correlation ids. Это не доказывает полноценный Codex chat/app UI runtime, потому что запрос был запущен напрямую через node из cache plugin root, а не из фактической Codex chat/plugin UI поверхности.

## Что доказано

- installed_skill_surface_loaded: True
- adapter_core_boundary_executed: True
- installed_plugin_runtime_invocation_proven: True
- positive correlation id: P14-FIX-02-20260623-194516-POSITIVE
- negative correlation id: P14-FIX-02-20260623-194516-NEGATIVE
- positive response status: core_boundary_partial_no_semantic_quality_claim
- negative response status: invalid_request
- controlled negative expected error present: True
- redacted/debug display present in positive response: True
- redacted/debug display present in negative response: True

## Что не доказано

- full_codex_chat_ui_proven: false
- token_savings_proven: false
- quality_improvement_proven: false
- learnability_proven: false
- training_started: false
- eval_started: false
- checkpoint_mutation_started: false

Exact missing boundary: no evidence path in this turn originated from the actual Codex chat/plugin UI surface and captured the rendered user-visible response/error/debug there. Current evidence stops at installed plugin adapter -> llm_mvp_core core boundary.

## Evidence paths

- Packet JSON: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\P14-FIX-02-20260623-194516\p14-fix-02-codex-ui-proof-gap-packet.json
- Summary JSON: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\P14-FIX-02-20260623-194516\run-summary.json
- Positive request: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\request.json
- Positive response: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\response.json
- Positive trace: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\trace.json
- Negative request: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\request.json
- Negative response: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\response.json
- Negative trace: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\trace.json

## Commands/checks run

1. Read installed skill instructions: Get-Content -Raw C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md.
2. Positive installed plugin adapter smoke from C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500: node "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js" --method prompt-to-task --user-prompt-or-task-text "<redacted synthetic P14-FIX-02 smoke task>" --host-facts-json "{\"surface\":\"installed-codex-skill\",\"scope\":\"P14-FIX-02\",\"proof_attempt\":\"installed-plugin-adapter-core-boundary\",\"full_codex_chat_ui_claimed\":\"false\"}" --requested-skills-json "[\"xiigen-smart-skills\"]" --available-skills-json "[\"xiigen-smart-skills\"]" --check-set-inputs-json "{\"runtime04e\":\"installed-skill-smoke\",\"phase14_fix\":\"P14-FIX-02\"}" --provenance-json "{\"source\":\"installed-plugin-skill\",\"plugin\":\"codex-xiigen-smart-skills\",\"phase\":\"14\",\"fix\":\"P14-FIX-02\"}" --correlation-id P14-FIX-02-20260623-194516-POSITIVE --debug-enabled --core-root "C:\Projects\llm_mvp_core" --extension-root . --request-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\request.json" --response-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\response.json" --trace-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\positive\trace.json".
3. Controlled negative adapter/core validation path from C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500: node "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js" --method prompt-to-task --user-prompt-or-task-text "<redacted synthetic P14-FIX-02 smoke task>" --host-facts-json "{\"surface\":\"installed-codex-skill\",\"scope\":\"P14-FIX-02\",\"proof_attempt\":\"controlled-negative-validation\",\"full_codex_chat_ui_claimed\":\"false\"}" --requested-skills-json "[\"xiigen-smart-skills\"]" --available-skills-json "[\"xiigen-smart-skills\"]" --check-set-inputs-json "{\"runtime04e\":\"installed-skill-smoke\",\"phase14_fix\":\"P14-FIX-02\",\"target_locator_required\":\"true\"}" --provenance-json "{\"source\":\"installed-plugin-skill\",\"plugin\":\"codex-xiigen-smart-skills\",\"phase\":\"14\",\"fix\":\"P14-FIX-02\",\"negative_path\":\"target_locator_required_without_target_locator\"}" --correlation-id P14-FIX-02-20260623-194516-NEGATIVE --debug-enabled --core-root "C:\Projects\llm_mvp_core" --extension-root . --request-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\request.json" --response-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\response.json" --trace-path "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\reports\P14-FIX-02-codex-ui-proof\P14-FIX-02-20260623-194516\negative\trace.json".
4. Packet synthesis: C:\Projects\llm_mvp_core\tools\phase14-codex-ui-proof.ps1.

## Safety confirmations

- No git commands were run.
- No governance, skills, AGENTS, or CLAUDE files were edited.
- No external AI call was used by this diagnostic path; evidence is local adapter/core invocation only.
- No training/eval/checkpoint mutation was started.
- No final readiness claim is made.

## Exact next action

Add a Codex host/chat/plugin UI harness or callable plugin command bridge that originates the request from the actual Codex chat/plugin surface, captures the user-visible answer/error/debug with correlation id, and reuses the same redaction and controlled-negative checks. Current evidence stops at installed plugin adapter -> llm_mvp_core core boundary.
