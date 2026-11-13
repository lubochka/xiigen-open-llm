# CURSOR-PATH-B-REAL-UI-SCENARIO-PREP-09

claim_scope: Подготовка одного bounded Path B запуска Row12 в реальном локальном Cursor UI.
evidence_scope: Подготовка основана на Path A review/report `cursor-host-visible-runtime-review-20260628T183240Z` и `reference-preservation-row12-cursor-fix-20260628T153936Z`, на `package.json`, `src/extension.ts`, `test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js`, и на локальной инвентаризации Cursor окон/мониторов.
count_claimed: 1 prep artifact, 0 Path B запусков, 0 новых screenshots.
independent_evidence_count: 13 команд/файлов до подготовки.
subagent_id_or_NONE: CURSOR-PATH-B-REAL-UI-SCENARIO-PREP-AND-RUN-09
downgrade_needed: Да. Этот файл сам по себе не является UI proof.

## Цель

Подготовить один реальный локальный Cursor Path B UI запуск для того же Row12 сценария, что и Path A:

- command/API Path A: `xiigenSmartSkills.runtimeProof.invoke`;
- user-facing Path B route: `XIIGen: Select Smart Skill`;
- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`;
- scenario_id: `frdp-p01-cursor-token-savings-baseline`;
- pair_id: `frdp-p01-row12-cursor-token-baseline-v1`;
- Path A prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`.

Raw prompt text is intentionally not copied into this prep or screenshots. UI input uses only the safe marker/scenario references.

## Target app/window

Target app: Cursor.

Target window title:
`[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor`

Observed target process:

- process name: `Cursor`;
- process path: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`;
- observed PID during prep: `11592`.

Monitor layout observed during prep:

- `\\.\DISPLAY1`: non-primary, bounds `X=-1920 Y=0 W=1280 H=720`;
- `\\.\DISPLAY2`: primary, bounds `X=0 Y=0 W=1920 H=1080`.

## Login/session preflight

1. Activate the target Cursor Extension Development Host window by exact title/process.
2. Capture a top-window screenshot before typing.
3. Verify the visible window is Cursor Extension Development Host for `cursor-xiigen-smart-skills`.
4. If Cursor opens anonymous/login-only UI or asks for credentials, do not type secrets.
5. If a login prompt blocks the XIIGen command palette or command input, record `NEEDS_PARENT_TOOLING_OR_USER_LOGIN` with the exact visible state and next user action. No passwords, tokens, or cookie values may be captured.
6. If the existing session allows command palette access, continue to the scenario steps.

## Exact UI steps for one attempt

1. Bring the target Cursor Extension Development Host window to foreground.
2. Press `Esc` once to clear transient UI state.
3. Press `Ctrl+Shift+P` to open the command palette.
4. Type exactly: `xiigen select smart skill`.
5. Expected visible state: the command palette filters to `XIIGen: Select Smart Skill`.
6. Capture screenshot `02-command-palette-xiigen-select-smart-skill-top.png` as a top-window crop.
7. Press `Enter` to open the XIIGen task input.
8. Type exactly this safe marker-only text:
   `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1 | frdp-p01-cursor-token-savings-baseline | frdp-p01-row12-cursor-token-baseline-v1`
9. Expected visible state: a real Cursor UI input for XIIGen contains the Row12 marker/scenario/pair references.
10. Capture screenshot `03-row12-marker-in-xiigen-input-top.png` as a top-window crop.
11. Press `Esc` to cancel without submitting the prompt to core or external AI.
12. Capture screenshot `04-after-cancel-no-submit-top.png` if the window remains stable after cancellation.

## Screenshots to capture

1. `01-target-extension-development-host-top.png`: top crop of the target Cursor window title/menu before scenario typing.
2. `02-command-palette-xiigen-select-smart-skill-top.png`: command palette showing `XIIGen: Select Smart Skill`.
3. `03-row12-marker-in-xiigen-input-top.png`: XIIGen input containing only safe Row12 marker/scenario/pair text.
4. `04-after-cancel-no-submit-top.png`: top crop after cancel, if available.

Screenshots must avoid passwords, tokens, browser cookies, private messages, and unrelated lower-panel content. Prefer top-window crops over full desktop screenshots.

## Fallback for two monitors/window overlap

1. Identify the target by exact title and process before each screenshot.
2. Use the target window rectangle, not whole desktop capture, when possible.
3. If windows overlap, attempt `SetForegroundWindow`/`ShowWindow` for the target.
4. If overlap remains, record the visible overlap in `window-inventory.json` and capture only target-window top crop if it is safe.
5. Do not treat two monitors or overlap as an automatic blocker.

## Expected result

PASS is allowed only for a new run that captures fresh screenshots showing:

- the real Cursor Extension Development Host window;
- the user-facing `XIIGen: Select Smart Skill` route;
- the Row12 marker/scenario/pair visible in XIIGen input;
- cancellation before submit.

If screenshots or UI control are unavailable from this sub-agent session, write a blocker instead of claiming Path B pass.

## Non-claims

This prep and any marker-only UI run do not claim product readiness, token savings, semantic quality, training, checkpoint promotion, locator update, full chat UI path, external AI behavior, or joined Path A+B readiness.

No secrets captured. No prompt submitted. No product code edits.
