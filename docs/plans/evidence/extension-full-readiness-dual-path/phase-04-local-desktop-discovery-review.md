# Phase 04.01R - Local Desktop Discovery/Control Review

CURRENT TASK:
Review Phase 04.01 Local desktop discovery/control packet. Accept only if discovery is honest, safe, and sufficient to plan Phase 04.02 marker/redaction-safe probes without overclaim.

active_task_sentence_source: Luba current unquoted message: "Review Phase 04.01 Local desktop discovery/control packet."

NOT THE TASK:
Do not launch GUI apps or take screenshots. Do not run host scenario probes. Do not mutate models/training/checkpoint/locator. Do not use git add/commit/push/checkout/reset/clean. Do not declare readiness.

EVIDENCE-ONLY:
Phase 04.01 report, control protocol, packets JSON, local evidence files, and discovery helper output are review evidence only. Phase 03 boundary evidence remains join input only.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить те же сценарии в реальных локальных программах на компьютере или записать точный blocker.

Шаг 04.01R: review discovery/control. Смысл шага: проверить, что discovery не выдаёт installed за controllable, и что следующий шаг безопасно ведёт к marker/screenshot evidence.

Статус: PASS. Discovery/control packet is honest, safe, and sufficient for Phase 04.02 preparation/fix planning. It does not authorize real marker probes yet.

Сделано:
- Read `phase-04-local-desktop-discovery-report.md`.
- Read `phase-04-local-desktop-control-protocol.md`.
- Parsed `phase-04-local-desktop-packets.json`.
- Checked the `phase-04-local-desktop-evidence/` directory.
- Read `discover_phase04_local_desktop.ps1`.
- Ran targeted searches for required host/control/evidence terms and forbidden readiness status terms.
- Ran `git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path`.

В процессе:
- No GUI launch, screenshot capture, local host scenario probe, training/checkpoint/locator mutation, or git write/ref action is running.

Осталось:
- Phase 04.02 may launch only a prep/fix slice that creates marker-only/redaction-safe surfaces and exact blocker handling.
- Real marker probes must wait until the marker-only/redaction gate is prepared and authorized for each host.

Review нашёл:
- PASS: Installed/running evidence is not treated as controllable UI proof. Claude and Codex are marked `BLOCKED_WITH_REASON`; Cursor is `PASS_LOCAL_DESKTOP_DISCOVERY` only for discovery/control path, not scenario pass.
- PASS: Claude, Cursor, and Codex each have an app path, command path or exact `NOT_FOUND`/blocker, plus marker, screenshot/log path, blocker, and redaction treatment.
- PASS: The packet records zero screenshots, zero GUI markers placed, and zero scenario rows marked passed.
- PASS: `PASS_LOCAL_DESKTOP` and `PASS_JOINED` appear only in explicit non-claim/prohibition contexts, not as accepted status claims.
- PASS: Phase 03 is preserved as later join input only; the protocol forbids mapping discovery statuses to `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, or `joined_claim_proven=true`.
- PASS: External-service/private-data risk is handled by blocking Claude/Codex GUI marker/screenshot routes and requiring Cursor redaction before screenshot evidence.
- NOTE: The evidence folder contains three additional verification logs, `09-packet-json-validation.txt`, `10-targeted-required-term-search.txt`, and `11-git-diff-check.txt`, not listed in `phase-04-local-desktop-packets.json`. This is not a blocker because all packet-listed evidence files exist and the extra files are review/verification outputs, not discovery claims.

Что исправлено:
- No discovery artifact was edited. This review file was added as the only changed file.

Следующее действие:
- Parent can launch Phase 04.02 prep/fix for marker-only/redaction-safe surfaces and exact blockers.
- Parent must not launch real marker probes yet.
- Parent must not claim readiness, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, joined proof, token/quality/training/checkpoint/locator results, or full extension readiness.

## Verification Output

- JSON parse: `EXIT_CODE: 0`; `packet_count: 3`; statuses are `BLOCKED_WITH_REASON` and `PASS_LOCAL_DESKTOP_DISCOVERY`; `screenshots_captured: 0`; `scenario_rows_marked_passed: 0`.
- Packets summary: hosts `Claude, Cursor, Codex`; statuses `Claude=BLOCKED_WITH_REASON; Cursor=PASS_LOCAL_DESKTOP_DISCOVERY; Codex=BLOCKED_WITH_REASON`; markers placed `False/False` for all hosts; screenshot paths `NOT_CAPTURED` for all hosts.
- Evidence existence: packet lists 10 evidence files; all listed files exist; actual evidence directory has 13 files because of 3 extra verification logs.
- Targeted search: required terms found across report/protocol/packet/evidence/helper: `Claude`, `Cursor`, `Codex`, `app_path`, `controllability`, `marker`, `screenshot`, `blocker`, `redaction`, `PASS_LOCAL_DESKTOP_DISCOVERY`, `BLOCKED_WITH_REASON`, `NOT_DISCOVERED_YET`.
- Forbidden status search: `PASS_LOCAL_DESKTOP` and `PASS_JOINED` found only in non-claim/prohibition contexts and not as packet statuses.
- Phase 03/join boundary search: report line states Phase 03 automated boundary packets are inputs only and not desktop proof; protocol line states discovery statuses must not map to Phase 02 `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, or `joined_claim_proven=true`.
- `git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path`: `EXIT_CODE: 0`.

