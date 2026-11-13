# CURSOR-LIVE-EVIDENCE-03 fallback report

## Итог

Live Cursor не запускался. Isolation gate не прошел, потому что helper firewall probe вернул `DEFER_ADMIN_REQUIRED_NO_FIREWALL_CHANGE`: текущая сессия не admin, временное outbound firewall rule не создавалось, а без этого запуск Cursor запрещен work order-ом.

Вместо live UI/chat proof выполнен fallback `test_only_adapter_or_defer`: локальный test-only adapter отправил три required scenario request-а напрямую в `llm_mvp_core` через `tools/xiigen-extension-core-api.ps1`. Это не Cursor UI, не Cursor chat participant и не Copilot/model surface.

## Что выполнено

- Positive debug-off: PASS, локальный core response создан.
- Positive debug-on: PASS, debug trace/visible debug shape создан.
- Negative/error missing locator: PASS как controlled error `target_locator_required`.
- Cursor launched: false.
- AI/model call possible through Cursor: false, потому что Cursor не запускался и prompt не отправлялся в Cursor/Copilot/chat/model surface.
- Temporary firewall rule created: false.
- Temporary firewall rule removed: not applicable; remaining rule count after probe = 0.
- Monitor ran against live Cursor: false, потому что live launch gate failed before launch.

## Что не закрыто

Этот packet не доказывает Cursor live UI/chat proof, runtime readiness, token savings, quality improvement, learnability, DPO/lifecycle readiness, target promotion, locator update, final readiness или product readiness.

## Основные refs

- Matrix: `cursor-test-only-adapter-matrix.json`
- Process/network evidence: `cursor-process-network-evidence.json`
- Redaction ledger: `cursor-redaction-ledger.json`
- No-overclaim: `cursor-no-overclaim.json`
- Hidden shortcut check: `cursor-hidden-shortcut-check.json`
- Human board: `human-status-board.json`
- Checks: `checks-report.json`
