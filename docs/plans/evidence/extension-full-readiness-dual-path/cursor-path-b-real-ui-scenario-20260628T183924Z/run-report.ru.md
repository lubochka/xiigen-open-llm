# CURSOR-PATH-B-REAL-UI-SCENARIO-RUN-09

claim_scope: Узкий Row12 Path B real local Cursor UI route: реальное окно Cursor Extension Development Host, видимый route `XIIGen: Select Smart Skill`, видимый Row12 marker/scenario/pair в XIIGen input, отмена без submit.
evidence_scope: Новая evidence folder `cursor-path-b-real-ui-scenario-20260628T183924Z`, 4 финальных PNG screenshots, `report.json`, `window-inventory.json`, evidence-local helper script, и визуальная проверка screenshots этим sub-agent.
count_claimed: 1 prep artifact, 1 evidence-local helper script, 4 helper invocations total, 4 final screenshots accepted for narrow meanings, 0 prompt submissions, 0 product code edits.
independent_evidence_count: 38 before this run-report write; includes Path A/report/source reads, window/process/display probes, generated artifact parse, and visual screenshot inspection.
subagent_id_or_NONE: CURSOR-PATH-B-REAL-UI-SCENARIO-PREP-AND-RUN-09
downgrade_needed: Да для product readiness, token savings, semantic quality, backend submit, full chat UI path, and joined Path A+B readiness. Нет для узкого утверждения, что финальный screenshot set показывает реальный Cursor UI route и Row12 marker input.

## PASS/NEEDS_FIX

PASS_NARROW_PATH_B_REAL_CURSOR_UI_ROUTE_EVIDENCE.

Это не full product readiness и не joined Path A+B proof.

## Что было подготовлено

Перед UI run создан `scenario-prep.ru.md` с:

- target window title/process;
- login/session preflight;
- exact UI steps;
- expected visible state;
- screenshots to capture;
- two-monitor/window-overlap fallback;
- no secrets captured and no submit boundary.

## Same-scenario anchors from Path A

- Path A command/API: `xiigenSmartSkills.runtimeProof.invoke`;
- user-facing Path B route: `XIIGen: Select Smart Skill`;
- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`;
- scenario_id: `frdp-p01-cursor-token-savings-baseline`;
- pair_id: `frdp-p01-row12-cursor-token-baseline-v1`;
- Path A prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`.

Raw prompt text was not copied into UI screenshots.

## UI actions actually performed

Final accepted run:

1. Activated the target Cursor Extension Development Host window by exact title/process.
2. Pressed `Esc` once.
3. Pressed `F1` to open the Cursor command palette.
4. Typed `xiigen select smart skill`.
5. Captured the command palette filtered to `XIIGen: Select Smart Skill`.
6. Pressed `Enter` to open XIIGen input.
7. Typed safe Row12 marker/scenario/pair text.
8. Pressed `Home` before screenshot so the marker prefix is visible.
9. Captured the marker/scenario/pair visible in XIIGen input.
10. Pressed `Esc` to cancel without submit.

No password, token, cookie, or credential was typed or captured. Clipboard was not read. No external AI was used by this slice.

## Attempts and rejected evidence

- Attempt 1 failed before UI interaction because the helper used a PowerShell variable name that collided with the read-only automatic `$PID` variable.
- Attempt 2 completed but screenshots did not show the command palette or marker; rejected by visual inspection.
- Attempt 3 completed and showed route/input, but the crop was too tall and included lower response/path content; rejected for redaction hygiene.
- Attempt 4 used `F1`, tight top crops, and `Home` before marker screenshot; accepted for the narrow UI route claim.

## Evidence files

- `scenario-prep.ru.md`
- `run-cursor-path-b-ui.ps1`
- `window-inventory.json`
- `report.json`
- `01-target-extension-development-host-top.png`
- `02-command-palette-xiigen-select-smart-skill-top.png`
- `03-row12-marker-in-xiigen-input-top.png`
- `04-after-cancel-no-submit-top.png`

## Screenshot meanings

- `01-target-extension-development-host-top.png`: target Cursor Extension Development Host / XIIGen surface.
- `02-command-palette-xiigen-select-smart-skill-top.png`: command palette route showing `XIIGen: Select Smart Skill`.
- `03-row12-marker-in-xiigen-input-top.png`: XIIGen input showing `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1` plus scenario/pair prefix.
- `04-after-cancel-no-submit-top.png`: cancel/no-submit state after input dismissal.

## Non-claims

I did not verify product readiness.
I did not verify token savings.
I did not verify semantic quality.
I did not run backend submit for Path B.
I did not claim full live chat UI path.
I did not claim joined Path A+B readiness.
marker-only != full local desktop pass.
boundary/API evidence != host readiness.

## Next smallest bounded action

Run an independent review/join worker that reads Path A packet plus this Path B packet, verifies screenshot meanings and JSON consistency, and decides whether the combined Row12 evidence can be joined narrowly without claiming product readiness/token savings/semantic quality.
