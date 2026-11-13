# Cursor login/session recovery preflight

Дата пакета: 2026-06-28T00:43:20+03:00.

Область: только login/session recovery preflight для одного Cursor scenario. Scenario prompt не вводился и не отправлялся. Секреты не вводились и не снимались.

## Счетчики

- Cursor scenario: 1.
- Path A: accepted по API/core boundary evidence `cursor-path-a-api-boundary-20260627-235243`.
- Path B: blocked by login/session gate по clean-host evidence `cursor-clean-host-preflight-20260628T002347+0300`; текущий live Cursor сейчас закрыт.
- Осталось: перезапустить clean Extension Development Host, пройти login/session recovery с участием пользователя, заново сделать preflight, и только после видимой безопасной сессии запускать Path B prompt-run отдельным bounded work.

## Что прочитано

- `cursor-path-a-api-boundary-20260627-235243/result.json`: Path A status `accepted`, `accepted=true`, core exit code `0`, prompt hash matched.
- `cursor-path-a-api-boundary-20260627-235243/verification.json`: JSON parse failures `0`, Path A accepted gate true.
- `cursor-clean-host-preflight-20260628T002347+0300/result.json`: clean host launched, Storage Error absent, login/signup gate present, prompt not entered, prompt not submitted.
- `cursor-clean-host-preflight-20260628T002347+0300/ui-summary.md`: visible UI was Cursor login/signup gate, no XIIGen Smart Skills input route visible.
- `cursor-clean-host-preflight-20260628T002347+0300/launch-command.json`: relaunch route is new Cursor window with test-only user-data/extensions dirs, extension development path, `--command xiigenSmartSkills.panel`, and the extension development folder.

## Текущее Cursor состояние

Live probe found no running `Cursor` process and no target Cursor window. Поэтому current screenshot не сделан: нечего было безопасно снимать. Это не blocker для этого preflight; это означает, что следующий шаг должен начать с clean-host relaunch.

Предыдущий clean-host packet уже содержит screenshot `target-window.png`, где виден Cursor login/signup gate. Этот пакет только ссылается на него и не копирует screenshot, чтобы не смешивать live evidence с предыдущим снимком.

## Safe recovery route

1. Relaunch Cursor clean Extension Development Host using the same route summarized in `cursor-clean-host-preflight-20260628T002347+0300/launch-command.json`: new window, test-only storage dirs, extension development path, `--command xiigenSmartSkills.panel`, and extension development folder.
2. If Cursor shows the login/signup gate again, click `Log In` only with the user present.
3. If the browser opens and already has a valid session, capture only non-secret state such as "browser opened" or "return-to-Cursor pending".
4. If the browser asks for password, PIN, 2FA, token, or another secret, stop automation and let the user complete credentials manually. Do not type, read, log, or screenshot secrets.
5. After the user completes login, return to Cursor and run a fresh login/session preflight. Capture only a redacted non-secret Cursor state.
6. Do not enter the scenario prompt until a separate authorized Path B prompt-run starts after session preflight passes.

## Evidence files

- `result.json`: structured finding packet.
- `window-inventory.json`: current live Cursor process/window inventory.
- `ui-summary.md`: human-readable UI state summary.
- `screenshot-manifest.json`: screenshot status for this packet.
- `action-log.md`: actions performed.
- `redaction-log.md`: secret handling and screenshot safety notes.
- `verification.json`: checks after writing.

## Parent recommendation

Parent can review and commit this new evidence folder if accepted. This sub-agent did not commit or push. Push should happen only under the parent workflow after review and authorization.
