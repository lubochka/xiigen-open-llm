# Phase 04.03 - Cursor Real Marker Probe

CURRENT TASK:
Execute Cursor Workorder 1 real marker probe from Phase 04.02 prep. Open only the local marker file in Cursor using the exact safe route, capture log/screenshot evidence if possible, and update local desktop packets.

active_task_sentence_source: Luba current unquoted instruction: "Execute Cursor Workorder 1 real marker probe from Phase 04.02 prep."

NOT THE TASK:
Do not run Claude or Codex marker probes. Do not send prompts/messages to Cursor chat or external services. Do not mutate models/training/checkpoint/locator. Do not claim Cursor full extension readiness, product readiness, token savings, semantic quality, learnability, DPO quality, checkpoint/locator status, formal `PASS_LOCAL_DESKTOP`, or `PASS_JOINED`. Do not git add/commit/push/checkout/reset/clean.

EVIDENCE-ONLY:
Phase 04.02 prep/review, the local marker file, command logs, process/window metadata, and screenshot crop prove only a Cursor marker-only local desktop probe. They do not prove readiness or a joined claim.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.03: Cursor real marker probe. Смысл шага: открыть локальный marker file в Cursor, собрать screenshot/log evidence, не отправляя prompt наружу и не заявляя readiness.

Статус: DONE. Cursor открыт exact local route, marker file виден в Cursor editor crop, screenshot прошёл redaction gate как marker-only, packet обновлён только для Cursor со статусом `PASS_LOCAL_DESKTOP_MARKER_ONLY`. Это marker-only proof, не формальный `PASS_LOCAL_DESKTOP`, не `PASS_JOINED`, и не readiness.

Сделано:
- Выполнена exact safe route из Phase 04.02 prep:
  ```powershell
  $markerFile = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\phase-04-local-desktop-evidence\markers\cursor-frdp-p04-marker.txt"
  & "C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd" --new-window --suppress-popups-on-startup $markerFile
  ```
- Сохранён command/open log: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/15-cursor-marker-open-command.txt`.
- Найден Cursor window title `cursor-frdp-p04-marker.txt - Cursor`.
- Первый screen-coordinate screenshot attempt отклонён и удалён, потому что захватил текущую Codex conversation surface, а не marker-only Cursor editor.
- Выполнен safer window-render capture через Cursor window handle, затем сохранён только editor-area crop.
- Принят screenshot: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-not-required.png`.
- Обновлён `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-packets.json`: Cursor packet теперь `PASS_LOCAL_DESKTOP_MARKER_ONLY`; Claude/Codex packet records не менялись содержательно.

В процессе:
- Cursor window может оставаться открытым с локальным marker file.
- Нет запущенных prompt/message sends, Cursor chat, external service route, training/model/checkpoint/locator work, git action, или joined/readiness probe.

Осталось:
- Phase 04.03 needs review before proceeding.
- Reviewer должен проверить screenshot/log evidence и packet wording.
- Formal Phase 02 `PASS_LOCAL_DESKTOP` gate остаётся закрытым до отдельного review/acceptance.
- Claude и Codex остаются вне этого шага; для них не запускались marker probes.

Review нашёл:
- PASS evidence for marker-only: accepted screenshot crop показывает `FRDP-P04-PREP-Cursor` и `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint` в Cursor editor.
- PASS safety gate: accepted crop не содержит Explorer/sidebar/recent files/account UI/notifications/terminal/private content.
- PASS command boundary: marker opened as a local file through `cursor.cmd`; Cursor AI chat and external services were not used.
- Safety finding: ordinary screen-coordinate capture was unsafe in this desktop context because it captured the current Codex conversation surface. That attempt was rejected and deleted, with the blocker recorded.

Что исправлено:
- Cursor moved from discovery-only `PASS_LOCAL_DESKTOP_DISCOVERY` to marker-only `PASS_LOCAL_DESKTOP_MARKER_ONLY`.
- Screenshot evidence was narrowed to a redaction-safe editor crop.
- Stale pending screenshot metadata was corrected to point only to accepted marker-only evidence or rejected/deleted evidence.
- Packet non-claims now explicitly distinguish marker-only status from formal `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and readiness.

Следующее действие:
- Send Phase 04.03 to review. Do not proceed to Claude/Codex, joined evidence, readiness, token/quality/training/checkpoint/locator, or formal Local desktop PASS until this marker-only packet is reviewed.

## Evidence Paths

| Evidence | Path | Status |
|---|---|---|
| Command/open log | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/15-cursor-marker-open-command.txt` | collected |
| Rejected screen-coordinate metadata | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/16-cursor-marker-window-capture-metadata.txt` | rejected/deleted private surface |
| Rejected screenshot safety note | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/16-cursor-marker-window-screenshot-rejected-private-surface.txt` | recorded blocker |
| Window-render metadata | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/17-cursor-marker-window-printwindow-metadata.txt` | source deleted after crop |
| Accepted crop note | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-note.txt` | accepted |
| Accepted screenshot crop | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-not-required.png` | `REDACTION_NOT_REQUIRED` |
| Packet update | `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-packets.json` | Cursor marker-only packet updated |

## Command Summary

Commands/actions used in this slice:

1. Exact Cursor route:
   ```powershell
   $markerFile = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\phase-04-local-desktop-evidence\markers\cursor-frdp-p04-marker.txt"
   & "C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd" --new-window --suppress-popups-on-startup $markerFile
   ```
2. `Get-Process` for Cursor process/window-title evidence.
3. PowerShell local screenshot helper snippets using Windows window APIs and `System.Drawing` for window capture/crop.
4. JSON update for `phase-04-local-desktop-packets.json`.

No git add, commit, push, checkout, reset, clean, branch, tag, merge, or rebase was used.

## Verification Results

- PASS: `phase-04-local-desktop-packets.json` parsed as JSON.
- PASS: Cursor packet status is `PASS_LOCAL_DESKTOP_MARKER_ONLY`; `marker_visible=true`; `marker_placed=true`; screenshot path points to the accepted editor crop.
- PASS: Claude and Codex packet statuses remain observed as `BLOCKED_WITH_REASON`.
- PASS: formal `pass_local_desktop_claims=0`, `pass_local_desktop_marker_only_claims=1`, and `pass_joined_claims=0`.
- PASS: targeted search found the required terms: `Cursor`, `PASS_LOCAL_DESKTOP_MARKER_ONLY`, `BLOCKED_WITH_REASON`, `NEEDS_MANUAL_UI_ROUTE`, `screenshot`, `redaction`, `marker`, `PASS_JOINED`, and `readiness`.
- PASS: `git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md tools/extension-full-readiness-dual-path` exited 0.

Verification output paths:

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/19-cursor-marker-packet-json-validation.txt`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/20-cursor-marker-targeted-required-term-search.txt`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/cursor/21-cursor-marker-git-diff-check.txt`

## Explicit Non-Claims

- I did not run Claude or Codex marker probes.
- I did not send prompts/messages to Cursor chat or any external service.
- I did not mutate models, training data, checkpoints, or locator state.
- I did not claim Cursor full extension readiness, product readiness, token savings, semantic quality, learnability, DPO quality, checkpoint/locator status, formal `PASS_LOCAL_DESKTOP`, or `PASS_JOINED`.
- This is marker-only local desktop evidence for Cursor Workorder 1.
