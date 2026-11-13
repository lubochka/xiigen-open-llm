# CURSOR-SAFE-LAUNCH-PREFLIGHT-OR-DEFER-02

## Решение

Cursor не запускался. Решение: `defer_no_safe_launch`.

Почему: локально найден `Cursor.exe` версии `3.8.11` и extension development path candidate `C:\Projects\cursor-xiigen-smart-skills`, но безопасный контур запуска не доказан. В этом work order нет активного process-bound network deny rule, нет same-run network monitor для точного Cursor process tree, нет доказанного cleanup после future run, и нет разрешенной интерпретации старых backend/model metadata attempts. При таких вводных запуск Cursor был бы overclaim и рискованным runtime proof.

## Что известно

- Ветка: `codex/chat-gpt-5.5`.
- Executable: `C:\Users\lubao\AppData\Local\Programs\Cursor\Cursor.exe`.
- Version: `3.8.11`.
- CLI wrapper: `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd`.
- Current Cursor processes: 0.
- Extension path candidate: `C:\Projects\cursor-xiigen-smart-skills`; exists = `true`, package.json exists = `true`.
- Prior Cursor evidence already contains required risk strings such as `availableModels`, `getDefaultModelNudgeData`, `availableDocs`, `backendUrl`, `repoBackendUrl`, `telemBackendUrl`, `agentBackendUrlPrivacy`, and `agentBackendUrlNonPrivacy`.

## Чего не хватает для запуска

1. Active network deny rule or sandbox bound to the exact future Cursor process tree before launch.
2. Same-run process tree monitor from launch through cleanup.
3. Same-run network monitor proving zero successful outbound connections.
4. Explicit rule for blocked backend/model metadata attempts; default is fail/defer.
5. Post-run stdout/stderr/log scan with zero required risk-string matches.
6. Verified cleanup that touches only the timestamped isolated user-data/extensions dirs.
7. Current proof that the extension development path and command surface are exact for this run.

## Не overclaim

Этот packet доказывает только preflight/defer readiness. Он не доказывает Cursor runtime readiness, full UI/chat proof, token savings, quality improvement, semantic quality, learnability, target promotion, locator update, final readiness or product readiness.

## Следующий work order

`CURSOR-SAFE-LAUNCH-NETWORK-PROCESS-ISOLATION-REPAIR-03`: без запуска Cursor доказать или подготовить проверяемый process/network isolation contract. Если parent plan выбирает независимый безопасный трек, альтернативный next action: `RUNTIME-CODEX-LIVE-EVIDENCE-01`.
