# QSG-21 live runtime target probes

## Итог для человека

QSG-21 собрал свежие target-bound evidence для трех host-поверхностей: Claude, Cursor и Codex. Все 12 host evidence rows прошли: для каждого host прошли positive target locator, missing locator negative, invalid locator negative и rollback locator.

Это доказывает только более близкую к runtime привязку target locator и rollback/load путей. Отчет не открывает claim на full live UI/chat readiness, token savings, quality improvement, semantic quality, learnability, common readiness, real target promotion, real locator update, production rollback closure или final readiness.

## Что доказано

- Claude прошел через `LIVE_PLUGIN_HOOK`: Claude Code `SessionStart` hook command, вызванный через `claude.exe --plugin-dir`.
- Cursor прошел через `EXTENSION_HOST`: команда Cursor/VS Code Extension Host.
- Codex прошел через `CODEX_APP_SERVER_PLUGIN`: Codex debug app-server `send-message-v2` installed-skill/plugin turn.
- Для каждой из трех поверхностей записаны target locator identity, target mode, missing-locator negative behavior, invalid-locator negative behavior, rollback mode, visible output и refs на core traces.
- `all_host_case_rows_passed = true`: всего прошло 12 строк evidence.
- Production-like locator load closure и production-like rollback load closure записаны без мутации production locator/checkpoint.
- Rollback/load closure status: `PASS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_NO_PRODUCTION_MUTATION`.

## Host evidence

### Claude

- Evidence class: `LIVE_PLUGIN_HOOK`
- Surface: Claude Code `SessionStart` hook command invoked by `claude.exe --plugin-dir`
- Passed: `true`
- Live UI/chat status: `NOT_FULL_CHAT_UI_CLAUDE_LIVE_PLUGIN_HOOK_ONLY`
- Positive target locator: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: real Claude Code plugin hook runtime target-bound evidence; not full Claude chat UI readiness.

### Cursor

- Evidence class: `EXTENSION_HOST`
- Surface: Cursor/VS Code Extension Host command
- Passed: `true`
- Live UI/chat status: `NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY`
- Positive target locator: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Cursor Extension Host command target-bound evidence; not full Cursor chat UI/panel readiness.

### Codex

- Evidence class: `CODEX_APP_SERVER_PLUGIN`
- Surface: Codex debug app-server `send-message-v2` installed-skill/plugin turn
- Passed: `true`
- Live UI/chat status: `NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY`
- Positive target locator: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Codex app-server/plugin target-bound evidence; not desktop UI or final readiness.

## Что не доказано и остается закрытым

- Full live UI/chat readiness не доказан.
- Token savings не разрешены к claim.
- Quality improvement не разрешен к claim.
- Semantic quality не claimed.
- Learnability не claimed.
- Common readiness не разрешен к claim.
- Real target promotion не выполнен.
- Real locator update не выполнен.
- Production rollback closure не выполнен.
- Final readiness не claimed.

## Promotion и rollback

- Promotion review eligibility после этого packet: `true`.
- Production-like locator load closure done: `true`.
- Production-like rollback load closure done: `true`.
- Candidate refs verified: `true`.
- Actual production locator promotion done: `false`.
- Actual production locator update done: `false`.
- Actual production rollback closure done: `false`.
- Locator/checkpoint mutated: `false`.
- Promotion review ref: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-production-like-locator-promotion-review.json`.

Практический смысл: QSG-21 годится как evidence packet для следующего review шага, но не является реальным promotion шагом и не меняет production locator/checkpoint.

## Deterministic boundary

- Verdict: `PASS_NO_HIDDEN_TRAINABLE_DECISION_IN_QSG21_AUTOMATION`.
- Host-side trainable scorer/router/lookup/counter не добавлялся.
- Host-side semantic logic, model training, DPO generation и checkpoint mutation не добавлялись.
- Core logic остается в `llm_mvp_core`.

## Источники evidence

- QSG-21 JSON report: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-report.json`.
- Promotion review JSON: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-production-like-locator-promotion-review.json`.
- Host evidence JSONL: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-host-evidence-rows.jsonl`.
- Rollback/load closure JSON: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-rollback-load-closure.json`.

## Следующий шаг

QUALITY-SEMANTIC-GATES-22 должен выполнить parent review QSG-21 host evidence и production-like locator load/rollback closure. Любая настоящая production locator/checkpoint mutation требует отдельного явного promotion work order или approval.
