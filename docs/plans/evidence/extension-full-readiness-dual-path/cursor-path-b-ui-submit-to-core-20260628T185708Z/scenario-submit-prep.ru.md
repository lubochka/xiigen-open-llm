# CURSOR-PATH-B-UI-SUBMIT-TO-CORE-11: подготовка одного UI submit запуска

claim_scope: подготовка одного bounded Path B submit-to-core запуска Row12 в реальном локальном Cursor UI.
evidence_scope: прочитаны prior Path B route/input/cancel packet `cursor-path-b-real-ui-scenario-20260628T183924Z`, join review `cursor-path-a-b-join-review-20260628T185039Z`, route-proof blocker `scenario-execution/cursor-path-b-route-proof-20260628-020041/result.json`, и код расширения `src/extension.ts` для `xiigen.smartSkills.select`.
count_claimed: 1 prep artifact, 0 UI submit попыток, 0 новых screenshots.
independent_evidence_count: 7 локальных чтений/проверок до этого prep.
subagent_id_or_NONE: CURSOR-PATH-B-UI-SUBMIT-TO-CORE-11
downgrade_needed: да. Этот prep сам по себе не доказывает submit-to-core.

## Цель

Провести один реальный локальный Cursor Path B UI запуск для того же Row12 сценария и реально отправить ввод через пользовательский маршрут расширения в core, если UI это позволяет.

Same-scenario anchors:

- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- scenario_id: `frdp-p01-cursor-token-savings-baseline`
- pair_id: `frdp-p01-row12-cursor-token-baseline-v1`

## Что уже известно из prior evidence

Prior Path B доказал только route/input/cancel:

- реальное окно Cursor Extension Development Host найдено;
- command palette показала `XIIGen: Select Smart Skill`;
- input принял Row12 marker/scenario/pair;
- prompt был отменен, `submitted_to_core=false`.

Join review разрешил только узкий partial join: Path A API/core-boundary reached core, Path B real UI route accepted the same marker, но `Path B submit-to-core not proven`.

Код расширения для `xiigen.smartSkills.select` показывает, что UI input создается через `vscode.window.showInputBox`, а непустой ввод после Enter передается в `runCursorSmartSkillsRequest(context, prompt, "xiigen.smartSkills.select", "command", ...)`. Поэтому в этом slice submit-кнопка для данного маршрута - клавиша `Enter` в XIIGen input после ввода safe marker/scenario/pair text.

## Target app/window

Target app: Cursor.

Target window title:

`[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor`

Target process:

- process name: `Cursor`
- expected process path: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`

Работать только с этим окном. Не использовать браузер, Gmail, Claude, основной Codex UI или другое приложение.

## Login/session preflight

1. Активировать target Cursor Extension Development Host по exact title/process.
2. Сделать безопасный top-window screenshot до ввода.
3. Проверить, что видимое окно - Extension Development Host для `cursor-xiigen-smart-skills`.
4. Если виден login/anonymous/credentials-only экран, не вводить secrets, не снимать passwords/tokens, записать blocker.
5. Если command palette доступна, продолжить один submit attempt.

## Exact UI steps for one attempt

1. Bring target Cursor Extension Development Host window to foreground.
2. Press `Esc` один раз, чтобы закрыть transient UI.
3. Press `F1`, открыть Cursor command palette.
4. Type exactly: `xiigen select smart skill`.
5. Expected visible state: command palette фильтруется до `XIIGen: Select Smart Skill`.
6. Capture `02-command-palette-xiigen-select-smart-skill-top.png`.
7. Press `Enter`, открыть XIIGen input.
8. Type exactly this safe marker/scenario/pair text:
   `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1 | frdp-p01-cursor-token-savings-baseline | frdp-p01-row12-cursor-token-baseline-v1`
9. Expected visible state: XIIGen input содержит Row12 marker/scenario/pair.
10. Capture `03-row12-marker-in-xiigen-input-top.png`.
11. Press `Enter` while focus remains in the XIIGen input. This is the submit action for the `showInputBox` route.
12. Wait up to 20 seconds for visible result or stable post-submit state.
13. Capture `04-after-submit-result-top.png`.
14. Scan runtime evidence locations for new `xiigen-cursor-xiigen.smartSkills.select-*` request/host-trace/core-trace/response files created after the submit start time.
15. Correlate only files from this run that contain the same marker/scenario/pair. If no same-marker trace is found, record `submitted_to_core=false`.

## Expected visible result

PASS requires both:

- visible/UI evidence from the target Cursor Extension Development Host before/input/submit/result sequence;
- structured evidence from the same run proving the UI submit reached core for the same marker/scenario/pair.

Expected visible result may be a Cursor information message or updated command result text indicating the XIIGen command entrypoint invoked llm_mvp_core. Visible result alone is not enough without same-marker structured trace/core evidence.

## Screenshots to capture

1. `01-target-extension-development-host-top.png`: target Cursor Extension Development Host before scenario input.
2. `02-command-palette-xiigen-select-smart-skill-top.png`: command palette with `XIIGen: Select Smart Skill`.
3. `03-row12-marker-in-xiigen-input-top.png`: XIIGen input containing only safe Row12 marker/scenario/pair text.
4. `04-after-submit-result-top.png`: post-submit visible result or stable state.
5. Optional `05-final-state-top.png` if result changes after additional wait.

Use top-window crops when possible. Do not capture passwords, tokens, cookies, private browser content, unrelated chat content, or secrets.

## Two-monitor/window-overlap fallback

Observed prior monitor layout may include a secondary monitor at negative X and primary monitor at X=0. The helper must:

1. identify the exact target by title/process before screenshots;
2. use the target window rectangle, not whole desktop, whenever possible;
3. call foreground/show-window before each major screenshot;
4. if overlap remains, capture only safe top-window crop and record overlap in `window-inventory.json`;
5. not treat two monitors as a blocker by itself.

## Evidence files to write

All new files must stay inside:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-path-b-ui-submit-to-core-20260628T185708Z`

Required:

- `scenario-submit-prep.ru.md`
- `run-report.ru.md` or `blocker.ru.md`
- `report.json`
- screenshots/logs
- helper script only if needed
- `git-diff-check.log`

## Non-claims and safety

No product code edits.
No AGENTS/CLAUDE/skills/governance/plan edits.
No secrets captured.
No credentials typed.
No external AI used.
No product readiness claimed.
No full Cursor readiness claimed.
No token savings claimed.
No semantic quality claimed.

If submit cannot be performed or same-marker core trace cannot be found, return `NEEDS_FIX` with the exact blocker and screenshots/logs that are safe.
