# Phase 04.02 - Next Local Desktop Marker Probe Workorders

CURRENT TASK:
Write exact next work orders for real marker probes, one host at a time, with screenshot/log/redaction requirements.

active_task_sentence_source: Phase 04.01 review PASS says parent can launch Phase 04.02 prep/fix; Luba authorized execution through sub-agents.

NOT THE TASK:
Do not run these workorders in this slice. Do not launch GUI apps, send prompts/messages, capture screenshots, claim readiness, or use git write/ref actions now.

EVIDENCE-ONLY:
These workorders are future execution instructions. They are not proof that any host marker is visible.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.02: prep/fix для marker-only/redaction-safe probes. Смысл шага: подготовить безопасные способы показать marker в реальных программах, не отправляя приватные данные и не делая readiness claim.

Статус: DONE. Workorders are prepared but not executed. Phase 04.02 needs review before any real marker probe.

## Workorder 1 - Cursor First Real Marker Probe

Status before execution: `READY_FOR_MARKER_PROBE`.

Exact future command route, not run in Phase 04.02 prep:

```powershell
$markerFile = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\phase-04-local-desktop-evidence\markers\cursor-frdp-p04-marker.txt"
& "C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd" --new-window --suppress-popups-on-startup $markerFile
```

Allowed content: only the prepared marker file. Do not open project files, terminals, Cursor AI chat, recent workspaces, or external services.

Eligible first scenario row:

- `scenario_id`: `frdp-p01-cursor-full-extension-entrypoint`
- `host`: Cursor
- `claim_id`: `full-extension`
- `marker`: `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint`

Additional eligible Cursor rows after the first marker probe:

- `frdp-p01-cursor-token-savings-baseline`
- `frdp-p01-cursor-semantic-quality-heldout`
- `frdp-p01-cursor-checkpoint-promotion-reload`

Screenshot/log requirements:

- Capture only the editor area with the marker file visible.
- Exclude Explorer/sidebar/recent files/account UI/notifications/terminal.
- Save screenshot only under `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/`.
- If command logs are captured, include only command path, exit behavior, and marker file path; no prompt/private data.
- Mark screenshot `PENDING_REDACTION` until inspected, then `REDACTED` or `REDACTION_NOT_REQUIRED`.

Downgrade behavior:

- If Cursor opens recent/private UI that cannot be cropped safely, stop and record `BLOCKED_WITH_REASON`.
- If the marker file is not visible, record `NEEDS_FIX`.
- Do not claim `PASS_LOCAL_DESKTOP` until actual evidence is collected and reviewed under Phase 02 PASS evidence gate.

## Workorder 2 - Claude Manual/UI Route Probe Prep

Status before execution: `NEEDS_MANUAL_UI_ROUTE`.

Allowed future route:

- Use only a fresh Claude window/surface or unsent input field where no chat history, account info, sidebar, file tree, notification, prompt, or project content is visible.
- Do not press send.
- Do not contact external Claude service with marker text unless a later work order explicitly authorizes external message send.
- If a marker-only surface cannot be created, record `BLOCKED_WITH_REASON`.

Candidate rows only after manual route is established and reviewed:

- `frdp-p01-claude-full-extension-entrypoint`
- `frdp-p01-claude-token-savings-baseline`
- `frdp-p01-claude-semantic-quality-heldout`
- `frdp-p01-claude-checkpoint-promotion-reload`

Screenshot/log requirements:

- Capture only a cropped marker-only UI/input area.
- Screenshot must show the row marker and no private content.
- Save evidence under `phase-04-local-desktop-evidence/`.
- Redaction status starts as `PENDING_REDACTION`; accept only `REDACTED` or `REDACTION_NOT_REQUIRED`.

Downgrade behavior:

- If the route requires sending a prompt/message, use `BLOCKED_WITH_REASON`.
- If the only visible surface contains prior chats/account data/private content, use `BLOCKED_WITH_REASON`.
- Do not claim `PASS_LOCAL_DESKTOP` or `PASS_JOINED`.

## Workorder 3 - Codex Manual/UI Route Probe Prep

Status before execution: `NEEDS_MANUAL_UI_ROUTE`.

Allowed future route:

- Use only a new marker-only Codex thread/window or unsent input field where this current conversation/workspace is not visible.
- Do not press send.
- Do not use the WindowsApps CLI path while it returns `Access is denied` as a proof route.
- If no clean marker-only Codex UI surface is available, record `BLOCKED_WITH_REASON`.

Candidate rows only after manual route is established and reviewed:

- `frdp-p01-codex-full-extension-entrypoint`
- `frdp-p01-codex-token-savings-baseline`
- `frdp-p01-codex-semantic-quality-heldout`
- `frdp-p01-codex-checkpoint-promotion-reload`

Codex chat UI row note:

- `frdp-p01-codex-codex-chat-ui-marker` is not first-probe eligible in this prep slice. It needs a dedicated real Codex chat UI route that separates helper/app boundary proof from actual chat UI evidence.

Screenshot/log requirements:

- Capture only a cropped marker-only Codex UI/input area.
- Exclude current conversation, workspace path, thread title, sidebar, account data, and prompt/private content.
- Save evidence under `phase-04-local-desktop-evidence/`.
- Redaction status starts as `PENDING_REDACTION`; accept only `REDACTED` or `REDACTION_NOT_REQUIRED`.

Downgrade behavior:

- If Codex CLI remains access-denied and no clean UI surface exists, use `BLOCKED_WITH_REASON`.
- If marker placement would expose current conversation/workspace, use `BLOCKED_WITH_REASON`.
- Do not claim `PASS_LOCAL_DESKTOP` or `PASS_JOINED`.

## Review Gate Before Execution

Phase 04.02 needs review before any real marker probe. The first allowed execution after review is Cursor Workorder 1 only. Claude and Codex need separate approval after their manual/UI route is accepted.

No git write/ref actions are part of any marker probe workorder.
