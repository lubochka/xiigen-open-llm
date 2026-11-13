# Phase 09 Claude full extension live proof

Статус: PHASE_09_CLAUDE_PROOF_READY_FOR_REVIEW

## Что решает фаза
Фаза 09 доказывает handler-level путь Claude extension: Claude hook/entrypoint вызывает реальный Node handler, handler передает redacted request в llm_mvp_core через thin adapter, возвращает видимый success/error output, показывает debug off/on поведение и фиксирует same-run correlation id на границе core.

## Покрытие 09.02-09.06
- 09.02: package/plugin wiring проверены через package.json, .claude-plugin/plugin.json и src/claude-entrypoint.js.
- 09.03: live handler-level request path выполнен через реальный Node entrypoint и локальный core endpoint llm_mvp_core.
- 09.04: видимый success stdout и видимый adapter error stdout проверены.
- 09.05: debug off скрывает internals; debug on показывает exact `xiigen <method> :`, `input: ...`, `output: ...` shape с redacted payload.
- 09.06: request, host trace и core trace содержат один и тот же correlation id в пределах одного запуска.

## Найденный Claude extension path
- C:\Projects\claude-xiigen-smart-skills

## Evidence leaf path
- C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z

## Что доказано
- Claude package/plugin wiring points to the real Node handler src/claude-entrypoint.js.
- The handler packages a XiigenExtensionRequest and calls llm_mvp_core through tools/xiigen-extension-core-api.ps1.
- Success path returns human-readable stdout from core-owned response text.
- Error path returns a human-readable adapter error without claiming core success.
- Debug off omits xiigen method/input/output internals from visible stdout.
- Debug on shows exact xiigen <method>/input/output shape using core-provided redacted payload.
- Same-run request, host trace, and core trace correlation ids match for the success run.

## Что не заявляется
- No full Claude desktop/app UI runtime proof is claimed.
- No Claude/Anthropic API or external AI call is claimed or used.
- No Cursor, Codex, dashboard, or API-only substitute evidence is used for Claude proof.
- No token savings, semantic quality, local model readiness, all-host readiness, or final project completion is claimed.
- No training, DPO mutation, checkpoint mutation, staging, commit, push, or branch operation is claimed.

## Риски для reviewer
- This is handler-level automation through the real extension handler, not a full Claude app UI session.
- The core endpoint is local and deterministic for proof; reviewer should not promote it to all-host readiness.
- The evidence uses controlled redacted test prompt strings, so it does not prove private-prompt handling beyond redaction policy and absence from visible debug output.

## Tests / commands / results
- node src/claude-entrypoint.js success debug off: exit=0; stdout=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\success-debug-off\stdout.txt; stderr=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\success-debug-off\stderr.txt
- node src/claude-entrypoint.js success debug on: exit=0; stdout=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\success-debug-on\stdout.txt; stderr=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\success-debug-on\stderr.txt
- node src/claude-entrypoint.js visible error path: exit=1; stdout=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\error-core-endpoint-missing\stdout.txt; stderr=C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof\phase09-claude-proof-20260623T132324Z\error-core-endpoint-missing\stderr.txt

## Проверки
- PASS 09.02_package_manifest_points_to_real_node_entrypoint
- PASS 09.02_claude_plugin_hook_calls_node_entrypoint
- PASS 09.02_entrypoint_is_thin_adapter_without_external_ai_or_host_logic
- PASS 09.03_debug_off_request_reaches_core_boundary
- PASS 09.04_visible_success_output_is_human_readable
- PASS 09.05_debug_off_omits_debug_internals
- PASS 09.05_debug_on_has_exact_xiigen_method_input_output_shape
- PASS 09.06_same_run_correlation_reaches_core_and_visible_trace
- PASS 09.04_visible_error_output_is_human_readable
- PASS 09.06_error_trace_keeps_host_correlation_without_core_claim

git_used=false
