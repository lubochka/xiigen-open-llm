# Phase 04.01 - Local Desktop Evidence Control Protocol

CURRENT TASK:
Create a safe evidence protocol for local desktop screenshots/logs/markers for Claude, Cursor, and Codex.

active_task_sentence_source: Luba required real local testing with active programs on this computer when automation cannot fully prove host UI.

NOT THE TASK:
Do not run all scenarios, do not claim readiness, do not expose secrets/private data, and do not use git write/ref actions.

EVIDENCE-ONLY:
This protocol governs future Phase 04 local evidence capture. It does not itself prove any host scenario.

## Required Marker Strings

| Host | Marker |
|---|---|
| Claude | `FRDP-P04-DISCOVERY-Claude` |
| Cursor | `FRDP-P04-DISCOVERY-Cursor` |
| Codex | `FRDP-P04-DISCOVERY-Codex` |

## Capture Rules

1. Prefer local command logs over screenshots when logs prove only install/control discovery and contain no prompts, secrets, account data, or private file content.
2. Capture screenshots only when the visible area is marker-only or has been checked for redaction risk.
3. Do not capture full desktop screenshots if unrelated windows, chats, account names, file trees, tokens, or private project details are visible.
4. If a host app would contact an external AI service to display the marker, use only a redacted marker-only smoke after explicit Phase 04.02 authorization, and record `external_service_dependency=true`.
5. If marker placement would expose private content, record `BLOCKED_WITH_REASON` and continue other hosts/scenarios.
6. Store every screenshot/log under `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/`.
7. Name screenshots with host, marker, and timestamp, for example `screenshot-claude-FRDP-P04-DISCOVERY-Claude-YYYYMMDD-HHMMSS.png`.
8. Every evidence record must include `redaction_status`: `REDACTED`, `REDACTION_NOT_REQUIRED`, or `PENDING_REDACTION`.
9. Do not convert discovery/control proof into `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, or readiness.

## Host-Specific Control Paths

### Claude

- Discovery evidence: Appx package `Claude_1.15200.0.0_x64__pzs8sxrjxfjjc`; running process path `C:\Program Files\WindowsApps\Claude_1.15200.0.0_x64__pzs8sxrjxfjjc\app\Claude.exe`.
- Command path: `NOT_FOUND` for `claude` and `claude-code` via `Get-Command`.
- Control method for Phase 04.02: manual GUI control or a separately discovered local automation route.
- Marker policy: do not type `FRDP-P04-DISCOVERY-Claude` into Claude until the window is known to be marker-only or fully redacted.
- Screenshot policy: screenshot only after visible private chats/account state are absent or redacted.
- Current blocker: GUI prompt/screenshot may expose private chat/account state or contact external AI.

### Cursor

- Discovery evidence: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`; exact CLI path `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd`.
- Control method for Phase 04.02: use exact CLI path, not generic PATH wrapper from this workspace.
- Suggested marker smoke after authorization: create/open a local marker-only file in a fresh Cursor window with `--new-window --suppress-popups-on-startup <marker-file>`.
- Screenshot policy: capture only the editor area with the marker file visible, after checking that explorer, recent files, account UI, terminal, and notifications do not expose private content.
- Current blocker: GUI screenshot was not captured in Phase 04.01 because the window may reveal workspace/recent-file/account UI.

### Codex

- Discovery evidence: Appx package `OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0`; running process path `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\Codex.exe`.
- Command path: `Get-Command codex` finds a WindowsApps resource path, but `--version` and `--help` return `Access is denied`.
- Control method for Phase 04.02: manual GUI control, in-app approved marker-only thread/window, or a Codex app-supported command route if one is discovered.
- Marker policy: do not place `FRDP-P04-DISCOVERY-Codex` in the current chat if it would expose this conversation or workspace details.
- Screenshot policy: capture only after a marker-only surface is available and redaction check passes.
- Current blocker: CLI access denied and current app UI may expose private conversation/workspace content.

## Packet Status Rules

Allowed Phase 04.01 discovery/control statuses:

- `PASS_LOCAL_DESKTOP_DISCOVERY`: installed/control path found for discovery only.
- `BLOCKED_WITH_REASON`: installed or partially discovered, but marker/control/screenshot is blocked with exact reason.
- `NOT_DISCOVERED_YET`: no local app/command/process evidence found yet.

These statuses must not be mapped to Phase 02 `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, or `joined_claim_proven=true`.

## Minimum Evidence Per Future Scenario

For each future local desktop scenario:

- `host`
- `scenario_id`
- `claim_id`
- `marker`
- `app_path`
- `command_path`
- `controllability`
- `marker_visible=true/false`
- `screenshot_or_log_path`
- `redaction_status`
- `external_service_dependency`
- `blocker_or_NONE`
- explicit non-claims

If any field cannot be collected safely, use `BLOCKED_WITH_REASON` and preserve the exact blocker.
