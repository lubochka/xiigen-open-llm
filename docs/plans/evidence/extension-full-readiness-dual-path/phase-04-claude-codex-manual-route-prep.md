# Phase 04.04 - Claude/Codex Manual-Route Prep

CURRENT TASK:
Execute Phase 04.04 Claude/Codex manual-route prep or exact blockers. Prepare safe marker-only UI-control work orders for Claude and Codex if possible; otherwise record exact blockers. Do not run real probes yet.

active_task_sentence_source: Luba current unquoted instruction: "Execute Phase 04.04 Claude/Codex manual-route prep or exact blockers."

NOT THE TASK:
Do not run real Claude/Codex marker probes. Do not send prompts/messages to Claude/Codex services. Do not click/type into GUI except process/window metadata inspection or work-order prep, and this slice used no GUI interaction. Do not capture screenshots. Do not mutate models/training/checkpoint/locator. Do not claim readiness, `PASS_LOCAL_DESKTOP`, or `PASS_JOINED`. Do not use git add/commit/push/checkout/reset/clean.

EVIDENCE-ONLY:
Phase 04 discovery/control artifacts, Phase 04.02 prep/workorders/prep packets/review, and the Phase 04.03 Cursor marker probe review are evidence only. They do not authorize Claude/Codex probes and do not prove Claude/Codex local desktop pass.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.04: Claude/Codex manual-route prep. Смысл шага: подготовить безопасный marker-only путь для Claude/Codex GUI или зафиксировать точный blocker.

Статус: DONE. Prep-only slice is complete, but both Claude and Codex are blocked for real marker probes right now. I did not find evidence of a safe marker-only unsent local draft or private-data-free local UI surface for either host. Both hosts therefore use `BLOCKED_WITH_REASON` in the current prep packet, with a possible next safe route that must be reviewed before any real marker probe.

Сделано:
- Read Phase 04 discovery report, control protocol, discovery/review packets, Phase 04.02 prep/workorders/prep packets/review, and the Cursor marker probe review for the marker-only pattern.
- Created Claude/Codex manual-route prep decisions with exact blockers instead of assuming safe UI control.
- Created static marker text files for future manual route establishment only; these files are not probes and were not sent anywhere.
- Created blocker work orders for future route-establishment attempts, including no prompt/no send constraints, screenshot/redaction requirements, and downgrade behavior.
- Updated `phase-04-local-desktop-packets.json` with current Phase 04.04 prep statuses and preserved prior Cursor marker evidence only as evidence-only history.

В процессе:
- Nothing is running for Claude/Codex GUI, CLI, screenshot capture, prompts/messages, product runtime, training, checkpoint/locator, or git.

Осталось:
- Review Phase 04.04 before any real Claude/Codex marker probe.
- If review accepts the blocker packet, the next safe action is route establishment only: a human/manual UI step must confirm a fresh marker-only unsent input field or clean local surface.
- If no clean surface exists, keep the host/scenario at `BLOCKED_WITH_REASON`.

Review нашёл:
- Claude Appx/GUI is installed/running from Phase 04.01 evidence, but no safe CLI route was discovered and no marker-only unsent draft surface has been proven. A probe would risk private chat/account exposure or external-service interaction.
- Codex Appx/GUI is installed/running and a WindowsApps CLI resource is discovered, but safe CLI probes returned `Access is denied`. The current Codex GUI cannot be used as evidence because it may expose this conversation/workspace.
- Cursor's reviewed marker-only local file path is a useful safety pattern, but it does not transfer to Claude/Codex because those hosts do not have a proven local-file-open route.

Что исправлено:
- Claude/Codex are no longer left as ambiguous "manual later" items. Each now has an explicit prep packet with blocker, possible next safe route, and no-prompt/no-send constraints.
- The future work order is downgraded from "run marker probe" to "establish or reject a safe manual UI route."
- The packet vocabulary for this step uses `READY_FOR_MANUAL_MARKER_PROBE`, `NEEDS_MANUAL_UI_ROUTE`, and `BLOCKED_WITH_REASON`; no current Claude/Codex prep status is PASS.

Следующее действие:
- Send Phase 04.04 to review. If accepted, run only a manual-route-establishment work order for Claude or Codex, not a real marker probe. A real marker probe becomes eligible only after a clean unsent marker-only surface is confirmed and reviewed.

Commit/continuation boundary: no git add, commit, push, checkout, reset, clean, branch, tag, merge, or rebase was used.

## Host Prep Decisions

| Host | Current prep status | Safe marker-only unsent local draft exists? | Safe local UI-control route exists? | Exact blocker | Possible next safe route |
|---|---|---:|---:|---|---|
| Claude | `BLOCKED_WITH_REASON` | Not verified | Not verified | No safe CLI; no verified marker-only unsent Claude surface; GUI may expose chat/account/private data or contact external Claude service if marker is sent. | Manual operator opens or identifies a fresh blank Claude surface/input field, confirms no private content is visible, places marker only as an unsent draft, and records route metadata. If any send/service call is needed, remain blocked. |
| Codex | `BLOCKED_WITH_REASON` | Not verified | Not verified | WindowsApps CLI route is access denied; current Codex GUI may expose this conversation/workspace; no safe marker-only UI surface is proven. | Manual operator creates or identifies a new marker-only Codex thread/window or unsent input field that hides current conversation/workspace. If unavailable, remain blocked. |

## Static Marker Files

These files are safe marker sources only. They are not proof that a GUI route exists.

- Claude marker source: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/markers/claude-frdp-p04-manual-route-marker.txt`
- Codex marker source: `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/markers/codex-frdp-p04-manual-route-marker.txt`

Both marker files state: no prompt, no send, no external service, no screenshot until a clean redaction-safe surface is confirmed.

## Screenshot, Redaction, And Log Rules

- No screenshot was captured in Phase 04.04.
- Future screenshot evidence is allowed only after the visible area is marker-only and has no chat history, account data, thread title, sidebar, workspace path, file tree, notification, prompt text, or private content.
- Future screenshot status starts as `PENDING_REDACTION`; accepted values are only `REDACTED` or `REDACTION_NOT_REQUIRED`.
- Future logs may include route metadata and app/command paths already discovered, but must not include prompt text, secrets, account data, private project content, or sent messages.
- The exact no prompt/no send constraint is binding: do not press send, do not submit, do not call an external AI service, and do not treat an unsent draft as a completed marker probe.

## Explicit Non-Claims

- I did not run a Claude marker probe.
- I did not run a Codex marker probe.
- I did not send a prompt/message to Claude, Codex, Cursor, or any external service.
- I did not click/type into Claude or Codex GUI.
- I did not capture screenshots or videos.
- I did not claim `PASS_LOCAL_DESKTOP`.
- I did not claim `PASS_JOINED`.
- I did not claim product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality, learnability/training, DPO quality, checkpoint promotion, locator update, or host runtime pass.

