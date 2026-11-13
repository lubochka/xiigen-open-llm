# Phase 04.02R - Local Desktop Marker Probe Prep Review

CURRENT TASK:
Review Phase 04.02 marker-only/redaction-safe probe prep. Accept only if Cursor probe is safe to run next, Claude/Codex are honestly marked as needing manual UI route or blockers, and no local desktop pass/readiness is claimed.

active_task_sentence_source: Luba current unquoted message: "Review Phase 04.02 marker-only/redaction-safe probe prep."

NOT THE TASK:
Do not launch GUI apps or real marker probes. Do not send prompts/messages to host services. Do not mutate models/training/checkpoint/locator. Do not git add/commit/push/checkout/reset/clean. Do not edit prep artifacts except typo/whitespace-only fixes. Do not declare readiness.

EVIDENCE-ONLY:
Phase 04.02 prep artifacts, workorders, prep packets, marker file, and Phase 04.01 discovery/control artifacts are review evidence only. They do not prove Local desktop pass, joined pass, readiness, token savings, semantic quality, training, checkpoint promotion, or locator update.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.02R: review prep/fix. Смысл шага: проверить, что подготовка marker probes безопасна и не открывает readiness claim.

Статус: PASS. Подготовка безопасна для следующего bounded Cursor marker-probe work: Cursor открывает только локальный marker file через exact `cursor.cmd` route; Claude и Codex честно оставлены в `NEEDS_MANUAL_UI_ROUTE`; локальный desktop pass, joined pass и readiness не заявлены.

Сделано:
- Прочитаны `phase-04-local-desktop-probe-prep.md`, `phase-04-local-desktop-probe-workorders.md`, `phase-04-local-desktop-probe-prep-packets.json`, and `phase-04-local-desktop-evidence/markers/cursor-frdp-p04-marker.txt`.
- Использован Phase 04.01 discovery/control context only as evidence for route/blocker consistency.
- Parsed the prep packets and checked host statuses/counts.
- Ran targeted searches for required statuses, forbidden pass/readiness claims, send/message/external-service language, screenshot/log, and redaction requirements.
- Ran `git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path`.

В процессе:
- Nothing is running. No GUI app, real marker probe, prompt/message send, screenshot capture, product runtime, model/training/checkpoint/locator mutation, or git write/ref action was started.

Осталось:
- Run Cursor Workorder 1 only as a future bounded real marker probe, after this PASS is accepted as the review gate.
- Keep Claude and Codex blocked from real marker probes until a separate manual/UI marker-only route is established and reviewed, or record `BLOCKED_WITH_REASON`.
- Review any future screenshot/log evidence before any Phase 02 `PASS_LOCAL_DESKTOP` packet can be considered.

Review нашёл:
- PASS: Cursor is safe for the first real marker probe. The route uses the exact local command `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd`, opens the prepared local marker file with `--new-window --suppress-popups-on-startup`, and forbids Cursor AI chat/external services.
- PASS: The Cursor marker file contains synthetic marker text only, says it has no private data, and explicitly says not to send it to Cursor AI chat or any external service.
- PASS: Claude is not overclaimed. Prep and packet status are `NEEDS_MANUAL_UI_ROUTE`; the route is limited to a fresh marker-only Claude surface or unsent input field, with `BLOCKED_WITH_REASON` if that cannot be done safely.
- PASS: Codex is not overclaimed. Prep and packet status are `NEEDS_MANUAL_UI_ROUTE`; the CLI path remains access-denied and future UI capture must avoid the current conversation/workspace.
- PASS: Workorders are one-host-at-a-time: Cursor first, then Claude manual/UI route prep, then Codex manual/UI route prep. Each workorder includes screenshot/log/crop/redaction requirements and downgrade behavior.
- PASS: Prep packets parse and match expected statuses: Claude `NEEDS_MANUAL_UI_ROUTE`, Cursor `READY_FOR_MARKER_PROBE`, Codex `NEEDS_MANUAL_UI_ROUTE`.
- PASS: Search hits for `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, readiness, token/quality/training/checkpoint/locator terms in the Phase 04.02 prep artifacts are non-claims, downgrade instructions, counts set to zero, or candidate marker row names. I found no local desktop pass/readiness claim.
- PASS: Parent can launch Cursor real marker probe as the next bounded sub-agent work, but only Cursor Workorder 1 and only after this review gate. Claude/Codex are not launch-ready.

Что исправлено:
- No prep artifacts were edited. This review file is the only changed file created in this slice.
- No substantive issue required a prep fix, so the review result is PASS rather than NEEDS_FIX.

Следующее действие:
- Next bounded work may be Cursor Workorder 1: open the local marker file in a fresh Cursor window, capture only the editor-area marker evidence or safe command log, redact/check before acceptance, and do not claim `PASS_LOCAL_DESKTOP` until actual evidence is collected and reviewed.

## Verification Output

Packet parse:
- Claude: `prep_status=NEEDS_MANUAL_UI_ROUTE`, `marker_can_be_placed_locally_without_external_or_private_data=False`, eligible first real marker probe rows empty.
- Cursor: `prep_status=READY_FOR_MARKER_PROBE`, `marker_can_be_placed_locally_without_external_or_private_data=True`, eligible rows present for Cursor marker probes.
- Codex: `prep_status=NEEDS_MANUAL_UI_ROUTE`, `marker_can_be_placed_locally_without_external_or_private_data=False`, eligible first real marker probe rows empty.
- Counts: `real_marker_probes_run=0`, `gui_apps_launched=0`, `prompts_or_messages_sent=0`, `screenshots_captured=0`, `pass_local_desktop_claims=0`, `pass_joined_claims=0`.

Targeted status/forbidden-claim search:
- Required statuses found: `READY_FOR_MARKER_PROBE`, `NEEDS_MANUAL_UI_ROUTE`, `BLOCKED_WITH_REASON`.
- `PASS_LOCAL_DESKTOP` / `PASS_JOINED` hits are confined to forbidden-action lists, downgrade behavior, non-claim statements, and zero-count packet fields.
- Cursor workorder and marker file include no prompt/message send route; the marker file says: `Boundary: do not send this content to Cursor AI chat or any external service.`
- Screenshot/log and redaction requirements are present for Cursor, Claude, and Codex.

Diff check:
- `git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path` exited 0 with no output.

Changed files:
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-probe-prep-review.md`

Git boundary:
- No git add, commit, push, checkout, reset, clean, branch, tag, merge, or rebase was used.
