# True Cursor host-visible no-send row12: packet

## Статус

`BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED`

Что проверено: в `cursor-xiigen-smart-skills` есть host-visible command surface `xiigenSmartSkills.runtimeProof.invoke`, и она принимает `prompt`, `evidenceRoot`, `method`, `requestedSkills`, `availableSkills`, `scenario`, `correlationId`.

Что не получилось: evidence-only runner `row12-host-visible-command-runner.test.js` был создан в этой папке и дважды передан Cursor через `--extensionTestsPath`:

- `Cursor.exe`
- `cursor.cmd`

Обе команды вернули exit code 0. После второй попытки появились только partial refs:

- runtimeProof request JSON
- core-response JSON

Но полный packet не появился: `row12-host-visible-runner-report.json`, `xiigen-cursor-host-runtime-latest.json`, host trace, core trace, entrypoint response и redacted trace отсутствуют. Поэтому это не `CURSOR_HOST_VISIBLE_ROW12_TOKEN_TRACE_READY`.

## Узкое утверждение

Поддержано только это: текущая попытка не доказала complete true Cursor host-visible row12 trace. Она доказала partial Extension Host/core boundary progress: request с тем же marker/prompt hash был записан, core-response был записан, но harness lifecycle не завершил runner report и обязательные trace refs.

Я не заявляю token savings. Я не заявляю semantic quality. Я не заявляю readiness.

## Следующее bounded действие

Нужен отдельный bounded worker с правом править только Cursor extension test/harness files:

1. Добавить repo-internal runner, например `C:/Projects/cursor-xiigen-smart-skills/test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js`.
2. Добавить узкий launch wrapper/script или package script, например `scripts/run-frdp-row12-host-visible.ps1`.
3. Runner должен вызвать `vscode.commands.executeCommand("xiigenSmartSkills.runtimeProof.invoke", row12Input)` с тем же marker и prompt hash `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`.
4. Runner должен писать report и trace refs в evidence root, заданный параметром/env var.
5. После PASS rerun paired row12 measurement against the host-visible trace refs.

## Verification

Verification commands/results are recorded in `result.json` after the final checks.
