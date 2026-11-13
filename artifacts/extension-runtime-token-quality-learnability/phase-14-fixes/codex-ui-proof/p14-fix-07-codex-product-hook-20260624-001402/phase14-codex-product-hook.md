# Phase 14.04 Codex Product Hook Packet

## Вердикт

**NEEDS_CODEX_APP_SUPPORT.** В этом ограниченном срезе нельзя честно доказать Phase 14.04 как видимый Codex UI proof. Плагин и локальный component QA подтверждают только тонкую границу `src/codex-entrypoint.js -> llm_mvp_core`, но не показывают, что сообщение было отправлено через видимый Codex chat/app/plugin UI и что ответ был захвачен из отрисованной Codex поверхности.

Финальная готовность не заявлена. Downscope не принят. `visible-answer.txt` из component QA не засчитан как rendered Codex UI evidence.

## Что проверено

- Source repo использован: `C:/Projects/codex-xiigen-smart-skills`.
- Installed cache не изменялся.
- Код и harness не изменялись.
- `HOST-08F-codexComponentQA.js` прошел в текущем run directory и подтвердил adapter/core correlation only.
- Локальный `codex.exe` найден, но help-пробы `codex --help`, `codex debug --help`, `codex plugin --help` в этом запуске завершились `Access is denied`; синтетический prompt в live Codex thread не отправлялся.

## Почему это не PASS

Для 14.04 нужен sanctioned Codex product/app API, который позволяет создать изолированную тестовую Codex UI session, отправить сообщение через видимую поверхность, экспортировать rendered response и связать этот UI event с тем же `llm_mvp_core` trace/correlation id. В plugin-only срезе такого API нет: плагин может сформировать request и redacted display text, но не может сам создать Codex UI session, не может доказать отправку через chat/app UI и не может экспортировать rendered UI response.

## Предлагаемый product API contract

- `create_isolated_codex_ui_session(plugin_id_or_path, workspace_root, session_label, redaction_policy) -> session_id, loaded_plugin_identity, session_artifact_root`.
- `submit_visible_ui_message(session_id, message_text, correlation_id) -> ui_message_id, submission_timestamp, accepted_by_visible_surface`.
- `export_rendered_response(session_id, ui_message_id) -> rendered_text_or_html_path, screenshot_or_render_artifact_path, render_timestamp`.
- `export_invocation_trace(session_id, ui_message_id, correlation_id) -> plugin_invocation_id, request_path, response_path, trace_path, core_correlation_id`.
- `close_or_archive_test_session(session_id, archive_policy) -> closed_or_archived, archive_ref`.

PASS for 14.04 should require same-run success across `submit_visible_ui_message`, `export_rendered_response`, and `export_invocation_trace` in one isolated session. Anything less repeats adapter smoke and would fake visible UI proof.

## Evidence

- Machine packet: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\p14-fix-07-codex-product-hook-20260624-001402\phase14-codex-product-hook.json`.
- Component QA report: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\p14-fix-07-codex-product-hook-20260624-001402\raw\component-qa-report.json`.
- Codex CLI surface probe: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\p14-fix-07-codex-product-hook-20260624-001402\raw\codex-cli-surface-probe.json`.
- Raw evidence root: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof\p14-fix-07-codex-product-hook-20260624-001402\raw`.

## No-overclaim flags

- `ui_proof_supported_now=false`.
- `visible_codex_ui_submission_proven=false`.
- `rendered_response_capture_proven=false`.
- `same_run_core_correlation_proven=true`, but only for local adapter/component QA, not for visible Codex UI submission.
- `final_readiness_claimed=false`.
- `git.no_git_commands_were_run=true`.
