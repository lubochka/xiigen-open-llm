# Phase 04.04 - Claude/Codex Manual-Route Work Orders

CURRENT TASK:
Prepare safe marker-only UI-control work orders for Claude and Codex if possible; otherwise record exact blockers.

active_task_sentence_source: Luba current unquoted instruction: "Prepare safe marker-only UI-control work orders for Claude and Codex if possible; otherwise record exact blockers."

NOT THE TASK:
Do not execute these work orders now. Do not run real Claude/Codex marker probes. Do not send prompts/messages. Do not click/type into GUI. Do not capture screenshots now. Do not claim readiness, `PASS_LOCAL_DESKTOP`, or `PASS_JOINED`. Do not use git write/ref actions.

EVIDENCE-ONLY:
These work orders are future route-establishment/blocker instructions. They are not evidence that Claude or Codex marker text is visible.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.04: Claude/Codex manual-route prep. Смысл шага: подготовить безопасный marker-only путь для Claude/Codex GUI или зафиксировать точный blocker.

Статус: DONE. Safe real-probe work orders are not available yet. The only safe future work orders are blocker/route-establishment work orders that either confirm a private-data-free unsent marker surface or keep the host at `BLOCKED_WITH_REASON`.

## Workorder 1 - Claude Manual Route Establishment Or Blocker

Current status before execution: `BLOCKED_WITH_REASON`.

Requested artifact type: manual-route-establishment packet, not marker-probe evidence.

Allowed future action:
- Inspect only whether Claude has a fresh blank marker-only surface or unsent input field.
- Use marker source `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/markers/claude-frdp-p04-manual-route-marker.txt`.
- The marker may be staged only as an unsent draft after confirming the visible surface contains no chat history, account data, sidebar, file tree, notification, prompt text, or private project content.
- Strict no prompt/no send rule: do not press send, do not submit, do not call external Claude service with the marker.

Forbidden:
- No real marker probe.
- No sent Claude prompt/message.
- No screenshot if private content is visible.
- No reuse of an existing chat with private/project context.

Screenshot/log/redaction requirements:
- Capture is allowed only after a clean marker-only crop is available.
- Screenshot starts `PENDING_REDACTION`; accepted only as `REDACTED` or `REDACTION_NOT_REQUIRED`.
- Logs may record route decision and blocker only; no prompt, no send, no secret, no account data.

Downgrade behavior:
- If Claude cannot show a marker-only unsent surface, record `BLOCKED_WITH_REASON`.
- If the route requires sending marker text to Claude service, record `BLOCKED_WITH_REASON`.
- If the route is clean but still not probed, record `READY_FOR_MANUAL_MARKER_PROBE` only after review, not in this slice.

## Workorder 2 - Codex Manual Route Establishment Or Blocker

Current status before execution: `BLOCKED_WITH_REASON`.

Requested artifact type: manual-route-establishment packet, not marker-probe evidence.

Allowed future action:
- Inspect only whether Codex has a new marker-only thread/window or unsent input field where this current conversation/workspace is not visible.
- Use marker source `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence/markers/codex-frdp-p04-manual-route-marker.txt`.
- The marker may be staged only as an unsent draft after confirming no current conversation, workspace path, thread title, sidebar, account data, prompt text, or private project content is visible.
- Strict no prompt/no send rule: do not press send, do not submit, do not use the current Codex chat as proof.
- Do not use the WindowsApps CLI route as evidence while it returns `Access is denied`.

Forbidden:
- No real marker probe.
- No sent Codex prompt/message.
- No screenshot if current conversation/workspace/private content is visible.
- No CLI proof from the access-denied WindowsApps resource.

Screenshot/log/redaction requirements:
- Capture is allowed only after a clean marker-only crop is available.
- Screenshot starts `PENDING_REDACTION`; accepted only as `REDACTED` or `REDACTION_NOT_REQUIRED`.
- Logs may record route decision and blocker only; no prompt, no send, no secret, no account data.

Downgrade behavior:
- If Codex cannot show a clean marker-only unsent surface, record `BLOCKED_WITH_REASON`.
- If the route exposes this conversation/workspace, record `BLOCKED_WITH_REASON`.
- If the route is clean but still not probed, record `READY_FOR_MANUAL_MARKER_PROBE` only after review, not in this slice.

## Review Gate Before Any Real Probe

Phase 04.04 needs review before any real Claude/Codex marker probe. This work order does not authorize prompt entry, send, screenshot capture, PASS_LOCAL_DESKTOP, PASS_JOINED, joined evidence, readiness, model mutation, training, checkpoint promotion, locator update, or git write/ref action.

