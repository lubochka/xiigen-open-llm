# RUNTIME-CLAUDE-CURSOR-CODEX-LIVE-EVIDENCE-PLAN-OR-REPAIR-01

Дата: 2026-06-19T03:39:10+03:00
Ветка: codex/chat-gpt-5.5
Режим: plan/repair only. В этой фазе не запускались Claude, Cursor, Codex, runtime probes, eval, training, DPO, checkpoint/model/locator/target mutation, source edits или external AI/model calls.

## Всего / Сделано / Сейчас / Осталось

| Поле | Статус |
|---|---|
| Всего | Нужно получить packet-backed runtime evidence для Claude, Cursor и Codex, затем отдельно собрать token before/after 9/9, quality scoring 9/9, trainable DPO 9/9, lifecycle 9/9 и 10 fresh final readiness reviews. |
| Сделано | Docs/QA has docs-edit + docs-review pass as documentation coverage only. Предыдущие runtime refs классифицированы: Claude narrow hook only, Cursor blocked/deferred, Codex plugin-cache adapter smoke only. |
| Сейчас | Full UI/chat readiness не закрыта. Token complete baseline/adaptive pairs: 0/9. Quality rows ready for actual scoring: 0/9. Trainable DPO rows: 0/9. Actual lifecycle rows: 0/9. Final readiness reviews counted: 0/10. Claim locks false. |
| Осталось | Запустить отдельные bounded runtime evidence work orders по host-границам, начиная с `RUNTIME-CLAUDE-LIVE-EVIDENCE-01`. Каждый packet обязан сказать, что он доказывает и чего не доказывает. |

## Классы evidence

| Evidence class | Что считается доказательством | Что не закрывает |
|---|---|---|
| Live UI/chat proof | Реальная named host surface принимает prompt через Claude/Cursor/Codex UI/chat/app/command, вызывает thin adapter и `XiigenExtensionApiService`, показывает visible answer/error/debug в host surface, пишет same-run correlation id, timestamp/freshness, raw/redacted input/output refs и negative/error path. | Token savings, quality improvement, learnability, target promotion, locator update, common model readiness, final readiness или другой host. |
| Extension host proof | Реальный extension host/app process загружает thin adapter, выполняет command/panel/plugin path и пишет host logs с correlation id. Это может быть near-live evidence, если visible chat UI не задействована. | Full UI/chat proof, если пользовательская chat/app surface не показала ответ/ошибку. |
| CLI/helper/adapter proof | Локальный helper или adapter command форматирует request и достигает core boundary. | Live host UI/chat/app readiness. |
| Dashboard/API proof | Dashboard/API request/response показывает core behavior. | Claude/Cursor/Codex host runtime. |
| Test-only proof | Test adapter, harness, static JSON или package smoke проверяет shape/entrypoint. | Production runtime, token, quality или learning. |

## Host-by-host matrix

| Host | Current evidence | Missing evidence | Next executable work order | Exact scenario prompt(s) | Required redaction/debug/token log fields | Expected evidence class | No-overclaim boundary |
|---|---|---|---|---|---|---|---|
| Claude | `runtime-claude-full-chat-ui-no-eval-proof-20260618-190216` produced safe local hook/core-boundary evidence through node entrypoint with debug-off, debug-on and negative missing-locator. Classification remains `narrow_live_host_surface`; full Claude Desktop/chat UI was not launched. | Real Claude Desktop/chat/plugin visible surface proof, or an explicit no-launch defer packet if the live surface would require unapproved external AI/model calls. Need visible answer/error/debug in Claude-visible surface, correlation id, freshness and core boundary proof. | `RUNTIME-CLAUDE-LIVE-EVIDENCE-01` | Positive debug-off: `XIIGen runtime evidence smoke: convert this into a task using prompt-to-task. Return only the XIIGen visible response.` Positive debug-on: same prompt with debug enabled. Negative: same host surface with missing/invalid target locator to produce controlled error. | host_name, host_surface, evidence_class, scenario_id, method_id, debug_enabled, prompt_sha256, redacted_prompt_ref, response_sha256, redacted_response_ref, raw_private_ref_if_allowed, correlation_id, timestamp_start/end, adapter_entrypoint_ref, core_trace_ref, visible_output_ref, error_ref, redaction_status, tokenizer_name/version, token_count_input/output/total if counted, quality_row_ref=null, learning_ref=null. | Preferred: live UI/chat proof. Fallback: extension host proof or defer packet, explicitly not full UI/chat proof. | This packet may prove only the named Claude runtime path and scenario. It must not claim token savings, quality, semantic quality, learnability, target promotion, locator update, final readiness or product readiness. |
| Cursor | Previous Cursor proof/repair found backend/model metadata attempts; `cursor-safe-launch-config-or-defer-20260618-213149` says safe relaunch is not allowed until a verified launch/network-isolation contract exists. | Preflight launch/network-isolation contract, exact executable/version/process tree, isolated user-data/extensions dirs, zero backend/model metadata attempts, same-run monitor/log evidence, then command/panel/chat visible proof. | `CURSOR-SAFE-LAUNCH-PREFLIGHT-OR-DEFER-02` before any Cursor relaunch. | Preflight has no user prompt. If preflight later passes, runtime positive prompt should match Claude scenario text for cross-host comparison; negative prompt must produce controlled error without external AI/model calls. | preflight_id, executable_path, version, command_line, process_tree_ref, isolated_user_data_dir, isolated_extensions_dir, extension_development_path, network_deny_rule_ref, monitor_ref, stdout/stderr refs, forbidden_backend_terms_scan, correlation_id plan, redaction_status. Runtime logs later add prompt/output/token fields. | Preflight/defer packet first. Runtime proof only after preflight passes. | A blocked/deferred Cursor packet is not failure of the whole plan and not readiness proof. It must not relaunch Cursor without preflight PASS. |
| Codex | `runtime-codex-full-chat-ui-no-eval-proof-20260618-213952` used installed skill/plugin-cache adapter smoke through cache entrypoint and core boundary. Report itself has mojibake in human text; evidence remains adapter/plugin-cache smoke only. | Real Codex desktop/app/chat/app-server/plugin runtime invocation inside a Codex surface, visible answer/error/debug, correlation/freshness, or explicit defer if surface cannot be safely invoked. | `RUNTIME-CODEX-LIVE-EVIDENCE-01` | Positive debug-off and debug-on prompts should match Claude scenario text. Negative path should force missing/invalid target locator. | host_name, codex_surface, plugin_id/version/ref, app/chat/app-server path, scenario_id, method_id, debug_enabled, prompt_sha256, redacted_prompt_ref, redacted_response_ref, correlation_id, timestamp/freshness, adapter_entrypoint_ref, core_trace_ref, visible_output_ref, error_ref, tokenizer/token counts if counted, no_external_ai_or_model_call_status, redaction_status. | Preferred: live Codex UI/app proof. Fallback: app-server/plugin host proof or defer packet; existing plugin-cache adapter smoke remains support only. | Codex adapter smoke does not prove Codex desktop chat UI/app readiness. Future packet proves only its named surface and scenario. |

## Что нужно логировать для будущего token before/after и quality

Runtime packet сам не обязан закрывать token/quality, но должен сохранить поля, чтобы позже можно было сравнить same-pair baseline/adaptive:

- `host_name`, `host_surface`, `evidence_class`, `scenario_id`, `pair_id`;
- `mode`: `baseline_common_model_only` или `adaptive_common_plus_user_model`, если token phase explicitly authorizes counting;
- `method_id`: `prompt-to-task`, `orchestration-plan` или `skill-selection`;
- `debug_enabled`, `target_locator_mode`, `adaptive_model_ref` or `adaptive_disabled_ref`;
- `prompt_sha256`, `visible_answer_sha256`, tokenizer name/version/accounting type;
- input/output/total token counts and counting source;
- raw private refs and redacted refs kept separate;
- `quality_row_ref`, `quality_score`, `quality_regression` left null until quality work order runs;
- `correlation_id`, timestamp/freshness, command/surface refs and host logs;
- no hidden deterministic shortcut check result.

Token comparison later is valid only when the same host, prompt, method, evidence class, target locator mode except baseline/adaptive variable, tokenizer/accounting version and redaction policy match, and when quality non-regression passes for the same pair.

## Как learning evidence отделяется от runtime proof

Runtime proof answers only: did the named host surface invoke the thin adapter and core boundary and show visible output/error/debug for this scenario. Local model learning remains a separate LEARN/DPO/lifecycle track. Raw Claude/Cursor/Codex output can be diagnostic/evaluation evidence only; it must not become DPO chosen/rejected training text, common training data, target promotion input or locator update input unless a later bounded source/license/privacy/material-quality work order explicitly approves transformed rows.

This phase itself does not launch hosts. It creates the next exact execution packets and repairs the readable Russian status.
