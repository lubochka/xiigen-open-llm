# Next Runtime Work Orders

These are bounded execution packets for the next runtime evidence slices. They are not permission for this phase to launch hosts.

## 1. RUNTIME-CLAUDE-LIVE-EVIDENCE-01

Requested artifact type: runtime evidence proof-or-defer packet for Claude.

Allowed files/artifacts:
- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/runtime-claude-live-evidence/`.
- Update only `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- No source, test, docs, skills, AGENTS.md, CLAUDE.md, checkpoint, model, locator or target edits.

Allowed commands/surfaces:
- Inspect local Claude docs/config and existing Claude adapter paths.
- Use the exact Claude host/plugin/chat/command surface only if the work order records whether that path would invoke external AI/model behavior and keeps to the approved boundary.
- If a no-external-AI live UI/chat path cannot be proven, produce a defer packet instead of launching.
- Near-live extension host or local hook proof may be captured only with explicit evidence class that says it is not full UI/chat proof.

Exact scenario prompts:
- Positive debug-off: `XIIGen runtime evidence smoke: convert this into a task using prompt-to-task. Return only the XIIGen visible response.`
- Positive debug-on: same prompt with debug enabled and redacted debug trace.
- Negative/error: same host surface with missing or invalid target locator, expecting a controlled visible error.

Required trace fields:
- host_name, host_surface, evidence_class, scenario_id, method_id, debug_enabled;
- prompt_sha256, redacted_prompt_ref, response_sha256, redacted_response_ref, raw_private_ref_if_allowed;
- correlation_id, timestamp_start, timestamp_end, adapter_entrypoint_ref, core_trace_ref, visible_output_ref, error_ref;
- tokenizer_name/version/counting method and token_count_input/output/total if counted;
- quality_row_ref=null unless a separate quality work order authorizes scoring;
- learning_ref=null unless a separate learnability work order authorizes DPO/lifecycle evidence;
- no_hidden_deterministic_shortcut_check.

No hidden deterministic shortcut boundary:
- The host adapter may route and display. It must not implement semantic prompt-to-task/orchestration/skill-selection logic, fixture replay, transcript replay, path/title lookup or keyword rules as final authority.

No-overclaim checks:
- The packet must include "What this proves / What this does not prove / Next independent track".
- It must not claim token savings, quality improvement, semantic quality, learnability, target promotion, locator update, final readiness or product readiness.

Parent commit/push:
- `git_authority=forbidden` for the sub-agent.
- `parent_will_perform_commit_push_after_review=true`.

## 2. CURSOR-SAFE-LAUNCH-PREFLIGHT-OR-DEFER-02

Requested artifact type: Cursor preflight/defer packet before any Cursor relaunch.

Allowed files/artifacts:
- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/cursor-safe-launch-preflight-or-defer/`.
- Update only `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- No source, tests, docs, skills, model/checkpoint/locator/target edits.

Allowed commands/surfaces:
- Read local docs/config and existing Cursor evidence.
- Build a preflight contract with exact executable path, version, command line, isolated user-data dir, isolated extensions dir, extension development path and cleanup boundary.
- Do not launch Cursor unless the same bounded work order has a verified network/process isolation contract active before launch.

Required trace fields:
- preflight_id, executable_path, version, command_line, process_tree_ref;
- network_deny_rule_ref, network_monitor_ref, stdout_ref, stderr_ref;
- scan result for `availableModels`, `getDefaultModelNudgeData`, `pricing`, `availableDocs`, `user privacy mode`, `backendUrl`, `repoBackendUrl`, `telemBackendUrl`, `cmdkBackendUrl`, `geoCppBackendUrl`, `cppConfigBackendUrl`, `agentBackendUrlPrivacy`, `agentBackendUrlNonPrivacy`;
- cleanup_ref and redaction_status.

No-overclaim checks:
- A blocked/deferred result is valid progress, not runtime proof.
- Clean Cursor live proof may be attempted only after preflight PASS.

Parent commit/push:
- `git_authority=forbidden` for the sub-agent.
- `parent_will_perform_commit_push_after_review=true`.

## 3. RUNTIME-CODEX-LIVE-EVIDENCE-01

Requested artifact type: runtime evidence proof-or-defer packet for Codex.

Allowed files/artifacts:
- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/runtime-codex-live-evidence/`.
- Update only `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- No source, tests, docs, skills, model/checkpoint/locator/target edits.

Allowed commands/surfaces:
- Inspect installed Codex plugin/app surface refs and existing plugin-cache adapter evidence.
- Use Codex desktop/app/chat/app-server/plugin surface only if the work order has exact invocation scope and no-overclaim rules.
- If only plugin-cache adapter smoke is possible, record it as adapter/helper support evidence and leave full Codex UI/app proof open.

Exact scenario prompts:
- Positive debug-off: same as Claude scenario.
- Positive debug-on: same scenario with debug enabled.
- Negative/error: missing or invalid target locator.

Required trace fields:
- host_name, codex_surface, plugin_id, plugin_version_or_ref, scenario_id, method_id, debug_enabled;
- prompt_sha256, redacted_prompt_ref, response_sha256, redacted_response_ref;
- correlation_id, timestamp_start, timestamp_end, adapter_entrypoint_ref, core_trace_ref, visible_output_ref, error_ref;
- tokenizer_name/version/counts if counted;
- no_external_ai_or_model_call_status and no_hidden_deterministic_shortcut_check.

No-overclaim checks:
- Plugin-cache adapter smoke cannot close Codex desktop/app/chat proof.
- Packet must not claim token savings, quality improvement, learnability, target promotion, locator update, final readiness or product readiness.

Parent commit/push:
- `git_authority=forbidden` for the sub-agent.
- `parent_will_perform_commit_push_after_review=true`.
