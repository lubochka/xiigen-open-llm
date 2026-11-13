# CURSOR-SAFE-LAUNCH-CONFIG-OR-DEFER-01

## Доска

| Поле | Значение |
| --- | --- |
| Всего | План должен доказать Claude, Cursor и Codex UI/chat/runtime paths, token before/after, quality, learnability, backup/export/import/share, continue-training, DPO, rollback, docs and final readiness. |
| Сделано | Прочитаны релевантные runtime/no-stop/source boundary sections плана, execution state, оба предыдущих Cursor proof/repair artifact дерева, предыдущий harness, reports, packets, checks и redacted backend metadata evidence. Cursor не запускался. |
| Сейчас | Решение: безопасно перезапускать Cursor Extension Host сейчас нельзя. В локальных config/docs/artifacts нет проверенного launch/network-isolation contract, который предотвращает или доказывает отсутствие встроенных Cursor backend/model metadata calls до host relaunch. |
| Осталось | Cursor full UI proof остается deferred/blocked; следующий независимый безопасный work order: `RUNTIME-CODEX-FULL-CHAT-UI-NO-EVAL-PROOF-01`. Token savings, quality improvement, semantic quality, learnability, target promotion, final readiness and product readiness остаются false. |

## Решение

Ответ на главный вопрос: нет, сейчас нельзя безопасно перезапускать Cursor Extension Host.

Причина простая и неприятно конкретная: предыдущий Cursor host уже писал backend/model metadata attempts в stderr. Repair packet подтвердил 46 совпадений по `ConnectError`, `availableModels`, `getDefaultModelNudgeData`, `pricing`, `availableDocs`, `user privacy mode` и backend URL keys. Это не было XIIGen task-solving и не было внешним AI вызовом из XIIGen harness, но work order запрещает external AI/model boundary в целом. Локально найдено требование на safe launch/network-isolation contract, но не найден сам проверенный contract.

Поэтому этот packet не открывает Cursor readiness, не запускает Cursor и не выдает relaunch work order. Он переводит Cursor track в честный deferred/blocked status и сохраняет no-stop поведение плана: независимый Codex proof может продолжаться.

## Что нужно для будущего Cursor relaunch

Чтобы в будущем ответ стал "да", до запуска Cursor должен существовать отдельный preflight artifact со всеми пунктами ниже:

1. Точный launch contract: executable path, Cursor version, command line, isolated user-data dir, isolated extensions dir, extension development path, extension tests path, environment variables, debug setting, target locator setting and cleanup boundary.
2. Локальная docs/config evidence: конкретная локальная документация или конфигурация Cursor, которая доказывает offline/no-backend/no-model-metadata behavior для встроенных Cursor services, а не только `external_ai=false` у XIIGen.
3. Network isolation evidence before launch: active process-bound or sandbox-bound deny rule for the exact Cursor process tree, with proof that it is active before Cursor starts and removed after cleanup. Generic machine-wide hope is not enough.
4. No metadata-attempt evidence: post-run stdout/stderr and host logs must have zero matches for `availableModels`, `getDefaultModelNudgeData`, `pricing`, `availableDocs`, `user privacy mode`, `backendUrl`, `repoBackendUrl`, `telemBackendUrl`, `cmdkBackendUrl`, `geoCppBackendUrl`, `cppConfigBackendUrl`, `agentBackendUrlPrivacy`, and `agentBackendUrlNonPrivacy`.
5. Network monitor evidence: same-run monitor packet must show zero successful outbound connections from the Cursor proof process tree. If it shows blocked attempts to model/backend metadata endpoints, that is not clean absence unless a later human-approved work order explicitly accepts blocked-attempt semantics.
6. Host proof evidence: only after preflight passes, command/debug-off, command/debug-on, panel or explicit API gap, chat participant or explicit safe-invoke gap, negative/error path, same-run request/response/core trace/host trace, correlation id, freshness, redaction and cleanup evidence may be captured.
7. Claim locks: token savings, quality improvement, semantic quality, learnability, live/full host readiness, common readiness, training readiness, target promotion, locator update, final readiness and product readiness must all remain false unless later phases prove them.

## Следующее действие

`RUNTIME-CODEX-FULL-CHAT-UI-NO-EVAL-PROOF-01`

Codex proof can continue independently because the plan says an unavailable evidence type must be marked deferred or blocked without stopping safe independent tracks. Cursor remains a named gap, not a reason to stop the whole execution flow.

## Что этот packet доказывает

- Предыдущие Cursor artifacts достаточно показывают risk: built-in Cursor backend/model metadata attempts were observed.
- Сейчас нет локально доказанного safe relaunch/network-isolation contract.
- Cursor relaunch was not performed in this phase.
- Cursor full UI proof is deferred/blocked, while Codex proof remains the next independent safe action.

## Что этот packet не доказывает

- Clean no-external-AI/model-call Cursor host proof.
- Full Cursor chat participant UI/app proof.
- Safe Cursor relaunch readiness.
- Token savings, quality improvement, semantic quality, learnability, target promotion, final readiness or product readiness.
