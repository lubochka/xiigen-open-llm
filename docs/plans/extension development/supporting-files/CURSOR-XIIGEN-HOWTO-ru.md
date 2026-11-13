# Cursor XIIGen HOWTO

Этот документ описывает intended Cursor/VS Code path. Это documentation coverage only. Current Cursor full UI proof remains blocked/deferred until safe launch/no-external-AI/no-backend contract and live Extension Host/UI evidence exist.

## Всего / Сделано / Сейчас / Осталось

| Поле | Статус |
|---|---|
| Всего | Нужен Cursor/VS Code Extension Host path with visible command/chat/panel answer, error/debug/correlation evidence and core boundary proof. |
| Сделано | Docs now describe the safe Cursor boundary and evidence checklist. |
| Сейчас | Cursor full UI proof is not closed; token pairs 0/9; quality rows 0/9; trainable DPO rows 0/9; lifecycle rows 0/9; final reviews 0/10. |
| Осталось | Safe launch contract, live Extension Host/UI packet and independent evidence tracks. |

## Intended Cursor/VS Code path

1. User invokes a Cursor command, chat command or panel action.
2. Cursor extension host loads the thin adapter without external AI and without unapproved backend calls.
3. Adapter builds `XiigenExtensionRequest` with host `Cursor`, method id, prompt/task text, debug flag, provenance and correlation id.
4. Adapter calls the core `XiigenExtensionApiService`.
5. Cursor UI displays answer, warnings, errors, debug/correlation details and safe raw/redacted evidence.
6. Extension Host logs and packet evidence record the same run.

## Safe launch boundary

Before a proof packet can count, the work order must define:

- no external AI/model calls;
- no unrelated backend startup;
- exact Cursor executable or Extension Host launch path;
- exact workspace/extension id and command/panel/chat path;
- where logs are captured;
- how debug/raw output is redacted.

If this contract is missing, the Cursor track is deferred, not closed.

## No-code evidence algorithm

1. Launch Cursor/Extension Host under the approved safe contract.
2. Run one positive command through the real UI.
3. Run one controlled error/negative path.
4. Capture visible UI output, logs, request/response refs and correlation id.
5. Prove the adapter called `llm_mvp_core` and did not implement hidden semantic logic.
6. Label the packet as Cursor live runtime only for the named scenario if evidence passes.

## Extension Host evidence checklist

- Visible Cursor answer/error in command/chat/panel.
- Extension Host log with correlation id.
- Redacted input/output refs.
- Debug trace or debug suppression reason.
- Same-run timestamp/freshness.
- Scope label that does not claim token, quality or learning.

## Current limitation

Helper/direct adapter/dashboard evidence cannot prove Cursor readiness. This document does not claim that Cursor UI is ready; it only states what future evidence must contain.
