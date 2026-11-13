# QSG-21 live runtime target probes

## Итог по-человечески

QSG-21 собрал свежие target-bound evidence для Claude, Cursor и Codex на более близких к host runtime поверхностях, чем QSG-20.
Это не открывает full live UI/chat readiness, token savings, quality improvement, learnability или production promotion.

## Host evidence

### Claude

- Evidence class: `LIVE_PLUGIN_HOOK`
- Surface: Claude Code SessionStart hook command invoked by claude.exe --plugin-dir
- Passed: `true`
- Live UI/chat status: `NOT_FULL_CHAT_UI_CLAUDE_LIVE_PLUGIN_HOOK_ONLY`
- Positive: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Real Claude Code plugin hook runtime target-bound evidence; not full Claude chat UI readiness.

### Cursor

- Evidence class: `EXTENSION_HOST`
- Surface: Cursor/VS Code Extension Host command
- Passed: `true`
- Live UI/chat status: `NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY`
- Positive: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Cursor Extension Host command target-bound evidence; not full Cursor chat UI/panel readiness.

### Codex

- Evidence class: `CODEX_APP_SERVER_PLUGIN`
- Surface: Codex debug app-server send-message-v2 installed-skill/plugin turn
- Passed: `true`
- Live UI/chat status: `NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY`
- Positive: `PASS`
- Missing locator negative: `PASS`
- Invalid locator negative: `PASS`
- Rollback locator: `PASS`
- Scope: Codex app-server/plugin target-bound evidence; not desktop UI or final readiness.

## What this proves

- TargetLocator identity, target mode, missing-locator negative behavior, invalid-locator negative behavior, rollback mode, visible output, and core trace refs were recorded for each passing host surface.
- Claude evidence is live Claude Code plugin hook evidence.
- Cursor evidence is Extension Host command evidence.
- Codex evidence is app-server/plugin evidence only when the app-server child report exists; otherwise it is labelled test-only fallback.

## What this does not prove

- `full_live_ui_chat_readiness`
- `token_savings_claim_allowed`
- `quality_improvement_claim_allowed`
- `semantic_quality_claimed`
- `learnability_claimed`
- `common_readiness_claim_allowed`
- `target_promotion_done`
- `locator_update_done`
- `production_rollback_closure_done`
- `final_readiness_claimed`

## Promotion and rollback

- Promotion eligible after this packet: `true`
- Production-like locator load closure done: `true`
- Production-like rollback load closure done: `true`
- Rollback/load closure status: `PASS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_NO_PRODUCTION_MUTATION`
- Production locator promotion done: `false`
- Production rollback closure done: `false`
- Promotion review ref: `artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-21-production-like-locator-promotion-review.json`

## Deterministic boundary

- Verdict: `PASS_NO_HIDDEN_TRAINABLE_DECISION_IN_QSG21_AUTOMATION`
- No host-side trainable scorer/router/lookup/counter or model mutation was added.

## Next

QUALITY-SEMANTIC-GATES-22: parent review of QSG-21 host evidence and production-like locator load/rollback closure. Actual production locator/checkpoint mutation still needs an explicit promotion work order or approval because QSG-21 kept locator_update_done, target_promotion_done, production_rollback_closure_done, token savings, quality, learnability, common readiness, and final readiness closed.

