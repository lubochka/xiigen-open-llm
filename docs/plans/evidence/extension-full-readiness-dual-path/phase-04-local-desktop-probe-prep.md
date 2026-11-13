# Phase 04.02 - Local Desktop Marker Probe Prep

CURRENT TASK:
Execute Phase 04.02 prep/fix for marker-only/redaction-safe local desktop probes.

active_task_sentence_source: Phase 04.01 review PASS says parent can launch Phase 04.02 prep/fix; Luba authorized execution through sub-agents.

NOT THE TASK:
Do not run real marker probes, launch GUI apps, send prompts/messages to Claude/Cursor/Codex services, capture private screenshots, mutate models/training/checkpoint/locator, claim readiness, or use git write/ref actions.

EVIDENCE-ONLY:
Phase 01 inventory rows, Phase 02 schema/contract, Phase 03 automated boundary packets, and Phase 04.01 discovery/control artifacts are inputs only. They do not prove Local desktop pass or joined readiness.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.02: prep/fix для marker-only/redaction-safe probes. Смысл шага: подготовить безопасные способы показать marker в реальных программах, не отправляя приватные данные и не делая readiness claim.

Статус: DONE. Подготовлены безопасные route/workorder/packet artifacts для будущих marker-only probes. Cursor имеет prep-only route через локальный marker file и exact CLI path. Claude и Codex не получили автоматический probe route: для них нужен manual/UI-control route или exact blocker before any real marker probe.

Сделано:
- Прочитаны Phase 04.01 discovery report, control protocol, discovery packets, review, Phase 01 inventory, Phase 03 automated boundary packets, and Phase 02 contract/schema.
- Создан prep artifact для Claude, Cursor, Codex with exact app/command route, marker placement rule, screenshot/log method, redaction rule, blocker/downgrade behavior, and eligible first-probe rows.
- Создан Cursor-only static marker file at `phase-04-local-desktop-evidence/markers/cursor-frdp-p04-marker.txt`.
- Созданы prep packets using only `READY_FOR_MARKER_PROBE`, `NEEDS_MANUAL_UI_ROUTE`, and exact blocker downgrade rules.
- Создан next workorder artifact for one-host-at-a-time real marker probes.

В процессе:
- No GUI app launch, real marker probe, prompt/message send, screenshot capture, model/training/checkpoint/locator mutation, product runtime, or git write/ref action is running.

Осталось:
- Review Phase 04.02 prep before any real marker probe.
- Run Cursor first only after review approval and redaction gate.
- Establish manual/UI-control route for Claude and Codex or record `BLOCKED_WITH_REASON` per host/scenario.

Review нашёл:
- Cursor is the only host with a proven exact CLI path that can safely open local marker-only content without external/private data.
- Claude has an installed/running GUI app but no discovered PATH CLI; a real marker probe would need a fresh private-data-free GUI surface and must not send a message.
- Codex has an installed/running GUI app and discovered WindowsApps CLI resource, but safe CLI probes returned `Access is denied`; a real marker probe needs a new marker-only UI surface and must not expose the current conversation/workspace.

Что исправлено:
- The Phase 04.01 discovery output is converted into prep-only host workorders instead of a readiness or Local desktop pass claim.
- Cursor now has a static local marker file and exact future command route.
- Claude/Codex are downgraded to manual-route-required prep status instead of assumed controllability.

Следующее действие:
- Send this Phase 04.02 prep packet to review. If accepted, run the Cursor workorder first as a real marker probe, then separately decide Claude and Codex manual/UI routes.

Commit/continuation boundary: no git add, commit, push, checkout, reset, clean, branch, tag, merge, or rebase used. Continuation is only to review, then one-host-at-a-time real marker probes after approval.

## Host Prep Matrix

| Host | Prep status | exact app/command route from discovery | Can marker be placed locally without external/private data? | Screenshot/log capture method | Redaction rule | Eligible first real marker probe rows |
|---|---|---|---|---|---|---|
| Claude | `NEEDS_MANUAL_UI_ROUTE` | App: `C:\Program Files\WindowsApps\Claude_1.15200.0.0_x64__pzs8sxrjxfjjc\app\Claude.exe`; CLI: `NOT_FOUND` for `claude` and `claude-code`; Start Menu target was listed but known-path check did not find it. | Not proven. Future route must use a fresh marker-only Claude surface or an unsent input field. If the marker must be sent to an external Claude service, use `BLOCKED_WITH_REASON`. | Manual/UI agent may capture only a cropped marker-only input/surface after confirming no chat history, account identity, sidebar, file tree, notification, prompt, or project content is visible. Logs are allowed only for local app/open evidence and must contain no prompts or private paths beyond already-discovered app paths. | `PENDING_REDACTION` until the crop is inspected; accept only `REDACTED` or `REDACTION_NOT_REQUIRED`. | None now. After manual route is established, candidate order: `frdp-p01-claude-full-extension-entrypoint`, `frdp-p01-claude-token-savings-baseline`, `frdp-p01-claude-semantic-quality-heldout`, `frdp-p01-claude-checkpoint-promotion-reload`. |
| Cursor | `READY_FOR_MARKER_PROBE` | App: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`; exact command: `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd`; use exact path, not generic PATH wrapper. | Yes, for a static local marker file. Prepared file: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/markers/cursor-frdp-p04-marker.txt`. Do not use Cursor AI chat for this probe. | Future command route only after review: open the marker file in a fresh Cursor window with `--new-window --suppress-popups-on-startup`. Capture editor-area crop or command log; do not capture explorer/recent files/terminal/account/notifications. | The marker file itself is `REDACTION_NOT_REQUIRED`; screenshot remains `PENDING_REDACTION` until cropped and checked. | `frdp-p01-cursor-full-extension-entrypoint`, `frdp-p01-cursor-token-savings-baseline`, `frdp-p01-cursor-semantic-quality-heldout`, `frdp-p01-cursor-checkpoint-promotion-reload`. |
| Codex | `NEEDS_MANUAL_UI_ROUTE` | App: `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\Codex.exe`; command resource: `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\resources\codex.exe`; safe CLI probes returned `Access is denied`. | Not proven. Future route must use a new marker-only Codex thread/window or an unsent input field. It must not expose this conversation/workspace and must not send the marker unless separately authorized. If no clean surface is available, use `BLOCKED_WITH_REASON`. | Manual/UI agent may capture only a cropped marker-only Codex UI surface after confirming no current conversation, workspace path, thread title, sidebar, prompt text, or account data is visible. CLI logs remain blocked by `Access is denied`. | `PENDING_REDACTION` until the crop is inspected; accept only `REDACTED` or `REDACTION_NOT_REQUIRED`. | None now. After manual route is established, candidate order: `frdp-p01-codex-full-extension-entrypoint`, `frdp-p01-codex-token-savings-baseline`, `frdp-p01-codex-semantic-quality-heldout`, `frdp-p01-codex-checkpoint-promotion-reload`. The Codex chat UI row is not first-probe eligible until a real Codex chat UI route is proven. |

## Scenario Eligibility Rules

- A row is first-probe eligible only when the host has a marker-only route that avoids external/private data and the marker can be captured with a redaction-safe screenshot/log.
- Cursor first-probe eligible rows are the four Cursor rows with Phase 03 `PASS_AUTOMATED_BOUNDARY`, because those rows have the most immediate future join value once Local desktop evidence exists.
- Claude and Codex rows are not first-probe eligible until a manual/UI-control route is established and reviewed.
- Rows with Phase 03 `NEEDS_VERIFICATION` can still receive a future Local desktop blocker or marker record, but they are not prioritized for first real marker probe because the Automated boundary side is not accepted yet.

## Explicit Non-Claims

- I did not run a real marker probe.
- I did not launch Claude, Cursor, or Codex GUI apps.
- I did not send any prompt/message to Claude, Cursor, Codex, or external AI services.
- I did not capture screenshots or videos.
- I did not mark any scenario as `PASS_LOCAL_DESKTOP`.
- I did not mark any scenario as `PASS_JOINED`.
- I did not claim product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality, learnability/training, DPO quality, checkpoint promotion, locator update, host runtime pass, or full extension readiness.
