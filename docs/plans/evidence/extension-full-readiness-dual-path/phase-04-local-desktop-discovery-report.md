# Phase 04.01 - Local Desktop Discovery/Control Report

CURRENT TASK:
Execute Phase 04.01 Local desktop proof discovery/control slice. Determine which real local host programs are installed/controllable for Claude, Cursor, and Codex on this computer, and create a safe evidence protocol for screenshots/logs/markers. Do not claim readiness.

active_task_sentence_source: Luba authorized execution through sub-agents and explicitly required local testing with active programs on the computer when automation cannot fully prove host UI.

NOT THE TASK:
Do not run all 28 host scenarios. Do not send private/secrets/user data to external services. Do not mutate models, train, promote checkpoints, update locators, or claim readiness. Do not use git write/ref actions.

EVIDENCE-ONLY:
Phase 01 inventory, Phase 02 schema/contract, and Phase 03 automated boundary packets are inputs only. Phase 03 boundary evidence is preserved for later join work and is not desktop proof in this slice.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker, не останавливая весь проект.

Шаг 04.01: discovery/control протокол для Claude, Cursor, Codex. Смысл шага: найти доступные приложения, способ запуска/контроля, visible marker и screenshot/log evidence path.

Статус: DONE. Срез discovery/control выполнен: для Claude, Cursor и Codex собраны локальные evidence-файлы по командам, известным путям, установленным пакетам, ярлыкам, процессам и безопасным CLI-probe. Это не readiness и не scenario pass; для неподконтрольных GUI-маркеров записаны точные blockers.

Сделано:
- Прочитаны Phase 04 section плана, Phase 01 inventory, Phase 02 schema/contract/sample Local desktop packet, Phase 03 packets и rereview.
- Создан и выполнен helper `tools/extension-full-readiness-dual-path/discover_phase04_local_desktop.ps1`.
- Сохранены discovery outputs в `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/`.
- Созданы discovery/control rows для Claude, Cursor, Codex.
- Создан safe evidence protocol для будущих screenshots/logs/markers.
- Создан `phase-04-local-desktop-packets.json` только для discovery rows, не для 28 scenario passes.

В процессе:
- Ничего не запущено для scenario probes.
- Нет активного screenshot capture, host prompt smoke, training, checkpoint/locator mutation, product runtime или git action.

Осталось:
- Phase 04.02 может идти только после выбора безопасного UI-control пути: marker-only fresh window/profile, manual redaction gate, or exact blocker per scenario.
- Claude and Codex need a controlled GUI/manual evidence route before real scenario probes.
- Cursor has a runnable local CLI path, but screenshot/marker smoke still needs a redaction-safe fresh-window protocol before scenario evidence can be accepted.

Review нашёл:
- Claude is installed and running as an MSIX/Appx app, but `Get-Command claude`/`claude-code` did not find a PATH CLI. GUI marker/screenshot is blocked by private chat/account risk unless a controlled fresh surface is prepared.
- Cursor is installed as a user app and has exact CLI paths that return version/help evidence. Generic PATH invocation from this workspace failed because the wrapper resolved `Cursor.exe` relative to the workspace; exact command path works.
- Codex is installed and running as an MSIX/Appx app, and `Get-Command codex` sees a WindowsApps resource path, but direct `--version`/`--help` probes return `Access is denied`. GUI marker/screenshot is blocked by this live chat/workspace privacy risk unless a controlled fresh surface is prepared.

Что исправлено:
- The discovery helper was corrected after an initial shortcut-root argument binding bug and a probe-target sort syntax bug.
- Superseded evidence files from the first helper run were removed so the final evidence folder contains only the clean numbered bundle.
- Cursor controllability is recorded using exact CLI path probes instead of the broken generic wrapper invocation.

Следующее действие:
- Phase 04.02 should start as a real local scenario-probe preparation/fix slice, not readiness work. It should first create a marker-only launch surface and redaction checklist for each host, then run at most the authorized scenario probes with exact blockers where GUI control remains unsafe.

Commit/continuation boundary:
- No git add, commit, push, checkout, reset, clean, branch, tag, merge, or rebase was used.
- Continuation is allowed only to the next bounded Phase 04.02 local desktop probe/fix slice. It is not allowed to jump to readiness, joined evidence, checkpoint, locator, training, DPO, or all 28 host scenarios.

Opaque codes after explanation: `PASS_LOCAL_DESKTOP_DISCOVERY`, `BLOCKED_WITH_REASON`, `NOT_DISCOVERED_YET`, `NO_READINESS_CLAIM`, `DISCOVERY_ONLY`.

## Host Discovery Summary

| Host | Discovery status | app_path | command path | controllability | marker | screenshot/log evidence path | blocker | redaction |
|---|---|---|---|---|---|---|---|---|
| Claude | `BLOCKED_WITH_REASON` for control; installed/running discovered | `C:\Program Files\WindowsApps\Claude_1.15200.0.0_x64__pzs8sxrjxfjjc\app\Claude.exe` from process list; Appx package `Claude_1.15200.0.0_x64__pzs8sxrjxfjjc` | `NOT_FOUND` for `claude` and `claude-code` via `Get-Command`; Start Menu target listed as `C:\Users\lubao\AppData\Local\AnthropicClaude\claude.exe` but known-path check did not find that file | GUI process exists; CLI not discovered; UI automation/screenshot is manual blocker until a marker-only/private-data-free surface exists | `FRDP-P04-DISCOVERY-Claude` not placed | Logs: `01-get-command-discovery.json`, `04-appx-package-discovery.json`, `05-shortcuts-discovery.json`, `06-process-discovery.json`, `08-marker-smoke-policy.json`; screenshot: `NOT_CAPTURED` | Running GUI may expose account/chat history or external AI prompt surface; no safe marker smoke was run | `PENDING_REDACTION` for any future screenshot; no screenshot captured now |
| Cursor | `PASS_LOCAL_DESKTOP_DISCOVERY` for discovery/control path | `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`; registry says Cursor (User) 3.8.23 | `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd` and sibling `cursor`; exact-path `--version` returned Cursor 3.8.11 | CLI launch/control is available by exact path; generic PATH wrapper failed from this workspace and should not be used as the control command | `FRDP-P04-DISCOVERY-Cursor` not placed in GUI; future marker can use a fresh marker file/new window after redaction gate | Logs: `01-get-command-discovery.json`, `02-known-path-discovery.json`, `03-installed-apps-discovery.json`, `05-shortcuts-discovery.json`, `07-cli-version-help-probes.json`, `08-marker-smoke-policy.json`; screenshot: `NOT_CAPTURED` | Screenshot/GUI marker not captured because a Cursor window may reveal workspace/recent-file/account UI | `PENDING_REDACTION` for future screenshot; CLI logs contain no prompt/private data beyond local paths/version |
| Codex | `BLOCKED_WITH_REASON` for CLI/control; installed/running discovered | `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\Codex.exe`; Appx package `OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0` | `Get-Command codex` found `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\resources\codex.exe`, but safe probes returned `Access is denied` | GUI app is active; CLI command is discovered but not runnable from this shell; UI marker/screenshot blocked by live chat/workspace privacy | `FRDP-P04-DISCOVERY-Codex` not placed | Logs: `01-get-command-discovery.json`, `04-appx-package-discovery.json`, `06-process-discovery.json`, `07-cli-version-help-probes.json`, `08-marker-smoke-policy.json`; screenshot: `NOT_CAPTURED` | WindowsApps CLI access denied; GUI capture may reveal this conversation/workspace | `PENDING_REDACTION` for any future screenshot; no screenshot captured now |

## Evidence Files

All command output for this slice is under `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/`:

- `00-discovery-summary.json`
- `00-discovery-summary.txt`
- `01-get-command-discovery.json`
- `02-known-path-discovery.json`
- `03-installed-apps-discovery.json`
- `04-appx-package-discovery.json`
- `05-shortcuts-discovery.json`
- `06-process-discovery.json`
- `07-cli-version-help-probes.json`
- `08-marker-smoke-policy.json`

Screenshots captured: none.

Screenshot evidence path for future captures: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/`.

## Discovery Counts

| Count | Value |
|---|---:|
| Hosts examined | 3 |
| Installed/running hosts discovered | 3 |
| Hosts with exact CLI/version evidence | 1 |
| Hosts with GUI/app process evidence | 2 |
| Hosts with command alias discovered but blocked | 1 |
| Hosts with marker actually placed | 0 |
| Screenshots captured | 0 |
| Local desktop scenario passes claimed | 0 |
| 28 scenario rows marked passed | 0 |

## Phase 04.02 Readiness For Real Scenario Probes

Phase 04.02 can run real scenario probes only after a fix/prep step:

- Cursor can likely proceed first with exact CLI path plus a marker-only file/new-window protocol, after a screenshot redaction gate is approved.
- Claude needs a GUI/manual control route because no local CLI was discovered and the installed app may expose private chat/account state.
- Codex needs a GUI/manual or app-approved control route because the command alias exists but safe CLI probes return `Access is denied`, and the current app UI may expose this conversation.

Therefore the next phase status is `NEEDS_FIX` for full real local scenario probes, even though this discovery/control slice is `DONE`.

## Explicit Non-Claims

- I did not run all 28 host scenarios.
- I did not place a marker into Claude, Cursor, or Codex UI.
- I did not capture screenshots.
- I did not send private, secret, user, prompt, or project content to external AI services.
- I did not run product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality, learnability/training, DPO, checkpoint promotion, locator update, or joined evidence probes.
- I did not claim `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, host runtime pass, product readiness, or extension readiness.
- This report is discovery/control evidence only.
