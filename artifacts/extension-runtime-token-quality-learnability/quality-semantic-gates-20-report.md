# QSG-20 live host target-bound probes

## Итог по-человечески

QSG-20 собрал target-bound evidence через QSG-19 TargetLocator, но не открыл финальные product/runtime claims.
Claude и Codex представлены handler CLI evidence, Cursor представлен Extension Host command evidence.
Это полезная runtime/adapter проверка, но это не полный live UI/chat PASS для всех трех hosts.

## Источники

- План: `docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- State: `docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- QSG-19 report: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-19-report.json`
- QSG-19 host rows: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-19-host-target-locator-plumbing-rows.jsonl`
- Target locator: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-17-test-only-active-locator.json`

## Evidence per host

### Claude

- Evidence class: `HANDLER_CLI`
- Live UI/chat status: `NOT_LIVE_UI_CHAT_HANDLER_CLI_ONLY`
- Positive path: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Handler CLI target-bound evidence; narrower than live UI/chat.

### Cursor

- Evidence class: `EXTENSION_HOST`
- Live UI/chat status: `NOT_LIVE_UI_CHAT_EXTENSION_HOST_COMMAND_ONLY`
- Positive path: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Extension host target-bound evidence; narrower than live UI/chat.

### Codex

- Evidence class: `HANDLER_CLI`
- Live UI/chat status: `NOT_LIVE_UI_CHAT_HANDLER_CLI_ONLY`
- Positive path: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Handler CLI target-bound evidence; narrower than live UI/chat.

## Rollback/load closure

- Status: `NOT_RUN_INSUFFICIENT_LIVE_TARGET_BOUND_REPRESENTATION`
- Reason: QSG-20 evidence is target-bound but not full live UI/chat for all Claude/Cursor/Codex hosts, so production-like rollback/load closure and locator promotion remain closed.
- Production locator promotion: `false`
- Production rollback closure: `false`

## Claims

Opened narrow claims:
- `claude_handler_cli_target_locator_positive_negative_rollback_paths`
- `cursor_extension_host_target_locator_positive_negative_rollback_paths`
- `codex_handler_cli_target_locator_positive_negative_rollback_paths`

Kept closed:
- `live_ui_chat_claimed`
- `runtime_extension_claimed`
- `token_savings_claim_allowed`
- `quality_improvement_claim_allowed`
- `semantic_quality_claimed`
- `learnability_claimed`
- `production_locator_promotion_done`
- `production_rollback_closure_done`
- `final_readiness_claimed`

## Deterministic boundary

- Verdict: `PASS_NO_HIDDEN_TRAINABLE_DECISION_IN_HOST_CHANGES`
- Host-side changes are thin target-locator plumbing only; no scorer/router/lookup/counter/trainable decision moved into host repos.

## QSG-21

QUALITY-SEMANTIC-GATES-21: repair/run true Claude live plugin/hook and Codex app-server/plugin target-bound probes with the QSG-19 TargetLocator, rerun Cursor UI/chat if required, then run production-like locator promotion plus rollback/load closure only after all three hosts have live target-bound evidence.

## Validation

- Validation verdict: `PASS_LIMITED_EVIDENCE_RECORDED_QSG21_REQUIRED`
- Target-bound path sufficiently represented: `false`
- Live UI/chat pass count: `0`
