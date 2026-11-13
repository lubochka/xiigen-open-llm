# RUNTIME_BROWSER_SCENARIO_PREP - 2026-06-29

claim_scope:
- Создан doc-only packet: human-readable Russian scenario prep для будущего runtime/browser proof.
- Это подготовка сценария, а не запуск продукта.
- Никакие runtime/browser/tests/builds/server/browser/git/PR/cleanup/secret действия не выполнялись.

evidence_scope:
- Прочитан `codex-proof-task-scenario-queue-2026-06-29.md`.
- Прочитан `codex-fresh-human-full-chain-audit-2026-06-29.md`.
- Прочитан `codex-runtime-design-proof-resolver-2026-06-28.md`.
- Прочитан `codex-designtocode-worktree-realignment-2026-06-29.md`.
- Выполнен read-only targeted search по этим четырем allowed handoff docs для runtime/browser command, URL/port, click/typing, screenshot, health, и DesignToCode selectors.
- Проверено, что этот target file не существовал перед созданием.

count_claimed:
- scenario prep packets created: 1
- source document reads: 4
- targeted read-only searches: 1
- runtime/browser executions: 0
- tests/builds/server starts: 0
- screenshots captured: 0
- secrets captured: 0
- git/PR/cleanup mutations: 0

independent_evidence_count:
- 6 current-turn evidence records: 4 source document reads, 1 targeted search, 1 target nonexistence check before creation.

subagent_id_or_NONE:
- delegated-runtime-browser-scenario-prep-current-session

downgrade_needed:
- Да. Evidence supports only this doc-only prep packet and the fact that exact runtime/browser execution selectors were not discovered in the allowed docs. It does not prove runtime readiness, browser readiness, host UI readiness, visual fidelity, clean repo, merge readiness, PR readiness, or product completion.

## Required literals

- `RUNTIME_BROWSER_SCENARIO_PREP`
- `human-readable Russian scenario prep`
- `exact clicks/typing/expected visible state`
- `no secrets captured`
- `Proof readiness items are tasks, not stop blockers`
- `Work happens on DesignToCode in a separate folder or worktree`

## Простое русское резюме

`Proof readiness items are tasks, not stop blockers`.

По-человечески: runtime/browser proof сейчас не является финальным стопом. Это конкретная задача подготовки и затем отдельная задача выполнения. Этот packet описывает, какие точные данные нужны, куда будущий worker должен смотреть, какие действия ему можно будет выполнять только после отдельного разрешения, какие screenshots/evidence нужно собрать, и где нельзя делать claims шире фактов.

`Work happens on DesignToCode in a separate folder or worktree`.

Рабочая папка для будущего proof: `C:\Projects\llm_mvp_core_DesignToCode_worktree`.

Рабочая ветка для будущего proof: `DesignToCode`.

Свежий audit сообщает, что `origin/DesignToCode` указывает на `8f20ceb8a9bfbadab2ad309226c47dd76e714548`. Этот факт доказывает только состояние ref, не runtime/browser readiness.

## Что известно из allowed docs

Из allowed docs известно:

- Runtime/browser proof должен быть отдельным bounded work order после prep PASS и explicit run authority.
- Future proof должен использовать точные команды, URL/port, login/session preflight, clicks/typing/expected visible state, evidence paths, screenshot policy, thresholds, и failure policy.
- Existing saved-output proof находится под `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof`, но allowed docs explicitly downgrade it to saved HTML/CSS browser render only. Это не live runtime/browser proof.
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` остается активным для runtime/browser proof, пока exact selectors и execution permissions не supplied.
- No runtime/browser execution, no screenshot proof, no external visual AI, no tests/builds были доказаны этими docs.

Из allowed docs не обнаружено:

- точная build/runtime/server command line;
- точная restore/build policy;
- точный URL, port, route, health endpoint;
- точный app/window/browser target для запуска;
- точный login/session preflight или explicit no-login scenario;
- точные clicks/typing/waits;
- точный expected visible state;
- точный screenshot filename set;
- точный evidence output directory для нового runtime proof;
- точные pass/fail thresholds.

Поэтому этот packet не изобретает команды. Если exact commands cannot be discovered from allowed docs, discovery worker is the next bounded task.

## Цель будущего proof

Цель будущего proof: показать, что approved DesignConversion UI или service реально запускается из `C:\Projects\llm_mvp_core_DesignToCode_worktree` и дает ожидаемое видимое состояние в browser/runtime сценарии.

Нельзя claims:

- host readiness from boundary/API evidence;
- full local desktop pass from marker-only/saved-output evidence;
- runtime/browser pass without actually running approved commands and scenario;
- screenshot proof without approved screenshot capture;
- product readiness from this prep packet.

## App/window target

Точный app/window target не найден в allowed docs, поэтому future execution пока не готов.

Future exact target placeholder, to be replaced by discovery worker before execution:

- App/service target: `<APP_OR_SERVICE_NAME_FROM_DISCOVERY>`
- Browser/window target: `<BROWSER_CHANNEL_FROM_DISCOVERY>` window opened to `<APP_URL_FROM_DISCOVERY>`
- URL/route target: `<PROTOCOL_HOST_PORT_ROUTE_FROM_DISCOVERY>`
- Health endpoint: `<HEALTH_ENDPOINT_FROM_DISCOVERY_OR_EXPLICIT_NONE>`
- Viewport/device profile: `<VIEWPORT_OR_DEVICE_PROFILE_FROM_DISCOVERY>`
- Login/session policy: `<EXPLICIT_NO_LOGIN_OR_SESSION_PREFLIGHT_FROM_DISCOVERY>`

До замены этих placeholders на exact values future proof worker must not open browser, start server, capture screenshot, or claim runtime/browser proof.

## Repo/worktree target

Exact repo/worktree path for future discovery and execution:

- Worktree root: `C:\Projects\llm_mvp_core_DesignToCode_worktree`
- Work branch: `DesignToCode`
- Source authority boundary: source branch remains read-only authority, not work branch.
- Forbidden by default: git mutation, PR mutation, branch mutation, cleanup/delete/move/stash/restore, secrets, external AI.

## Commands for future worker

Exact runtime/browser commands were not discovered from allowed docs.

Future command placeholders, to be replaced by `bounded-runtime-browser-command-discovery-worker` before any execution:

1. Change directory:
   - `<cd C:\Projects\llm_mvp_core_DesignToCode_worktree>`

2. Dependency/restore command:
   - `<RESTORE_OR_INSTALL_COMMAND_FROM_DISCOVERY_OR_EXPLICIT_NONE>`

3. Build command:
   - `<BUILD_COMMAND_FROM_DISCOVERY_OR_EXPLICIT_NONE>`

4. Runtime/server start command:
   - `<SERVER_START_COMMAND_FROM_DISCOVERY>`

5. Health check command:
   - `<HEALTH_CHECK_COMMAND_FROM_DISCOVERY>`

6. Browser automation command or manual browser instruction:
   - `<BROWSER_AUTOMATION_COMMAND_OR_MANUAL_BROWSER_STEPS_FROM_DISCOVERY>`

7. Server stop/cleanup command:
   - `<SERVER_STOP_COMMAND_FROM_DISCOVERY>`

These are placeholders, not approved commands. They must not be run until a discovery worker replaces them with exact commands and Luba authorizes runtime/browser execution.

## Exact clicks/typing/expected visible state

`exact clicks/typing/expected visible state` is required before runtime/browser proof execution.

Current state from allowed docs:

- Exact clicks: not discovered.
- Exact typing: not discovered.
- Exact waits: not discovered.
- Exact expected visible state: not discovered.

Future scenario placeholders, to be replaced before execution:

| Step | Future exact action | Future exact expected visible state | Evidence |
| ---: | --- | --- | --- |
| 1 | Open `<APP_URL_FROM_DISCOVERY>` in `<BROWSER_CHANNEL_FROM_DISCOVERY>` | `<LANDING_OR_APP_READY_TEXT_FROM_DISCOVERY>` visible; no crash/error page | screenshot `<01-app-ready.png>` if approved, plus DOM/text assertion |
| 2 | `<CLICK_OR_NAVIGATION_STEP_1_FROM_DISCOVERY>` | `<EXPECTED_STATE_AFTER_STEP_1_FROM_DISCOVERY>` | screenshot `<02-after-step-1.png>` if approved, plus assertion |
| 3 | Type `<SAFE_NON_SECRET_INPUT_FROM_DISCOVERY>` into `<FIELD_SELECTOR_OR_LABEL_FROM_DISCOVERY>` | `<EXPECTED_STATE_AFTER_TYPING_FROM_DISCOVERY>` | screenshot `<03-after-typing.png>` if approved, plus assertion |
| 4 | `<CLICK_OR_SUBMIT_STEP_FROM_DISCOVERY>` | `<EXPECTED_FINAL_STATE_FROM_DISCOVERY>` | screenshot `<04-final-state.png>` if approved, plus assertion |

Typing policy:

- Use only non-secret fixture text approved in the future scenario.
- Never type passwords, tokens, API keys, private account data, or credentials.
- `no secrets captured`.

## Screenshots and evidence to capture

Screenshots are not approved or generated by this prep packet.

Future screenshot/evidence placeholders:

- Evidence directory: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>`
- Command transcript: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\runtime-command-transcript.txt`
- Service health result: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\service-health-result.txt`
- Browser transcript: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\browser-scenario-transcript.txt`
- Screenshot 1 if approved: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\01-app-ready.png`
- Screenshot 2 if approved: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\02-after-step-1.png`
- Screenshot 3 if approved: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\03-after-typing.png`
- Screenshot 4 if approved: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\04-final-state.png`
- Visible-state assertions: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\visible-state-assertions.json`
- Final pass/fail report: `<RUNTIME_BROWSER_EVIDENCE_DIR_FROM_DISCOVERY>\runtime-browser-proof-report.md`

Evidence must include:

- exact command lines actually run;
- timestamps;
- cwd;
- service start/stop result;
- health check output;
- browser URL/viewport/channel;
- exact clicks/typing performed;
- expected vs actual visible state;
- screenshot paths only if screenshots were actually approved and captured;
- downgrade line for any missing/skipped path.

## Fallbacks

If server command unavailable:

- Do not invent a command.
- Return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.
- Delegate `bounded-runtime-browser-command-discovery-worker`.

If browser unavailable:

- Do not claim browser proof.
- Capture only the failed browser availability evidence if that capture is inside the future worker write scope.
- Return with exact missing browser/channel/runtime reason.
- Proposed next task: browser/tooling discovery or local offline scaffold decision packet.

If login/session unavailable:

- Do not ask for or capture secrets.
- Do not print credentials.
- Do not attempt anonymous login recovery unless future scenario explicitly authorizes it.
- Return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` with missing session policy.
- `no secrets captured`.

If app opens anonymously:

- Record that the app opened anonymously.
- Do not enter credentials.
- Follow only the future approved no-login path if one exists.
- Otherwise stop future execution and request explicit login/session preflight policy.

If screenshots are not approved:

- Do not capture screenshots.
- Use only approved text/DOM/state assertions if future execution authorizes browser access.
- Downgrade claim: browser proof without screenshot proof, if and only if browser run actually occurred.

If saved-output-only fallback is selected:

- Use existing saved-output path only as saved HTML/CSS boundary evidence.
- Do not claim live runtime/browser proof, source-design fidelity, screenshot proof, or product readiness.

## Future discovery worker

Because exact commands and scenario selectors were not discovered from allowed docs, the next bounded task is:

`bounded-runtime-browser-command-discovery-worker`

Allowed future read scope:

- `C:\Projects\llm_mvp_core_DesignToCode_worktree`
- relevant package/project/config files only;
- existing handoff docs needed to connect the scenario goal;
- no secrets;
- no runtime/browser/tests/builds unless separately authorized.

Allowed future output:

- one doc-only selector packet with exact commands, app/window target, URL/port, login/session policy, clicks/typing/expected visible state, evidence paths, screenshot policy, thresholds, and stop rules.

Forbidden future discovery actions unless separately authorized:

- no server start;
- no browser launch;
- no tests/builds;
- no dependency install;
- no git/PR mutation;
- no cleanup;
- no secret value read or print.

## Future execution worker gate

Only after discovery packet PASS and explicit runtime/browser execution authority:

`bounded-runtime-browser-proof-worker`

Future worker may run only the exact approved commands and exact scenario. It must stop on selector drift, missing URL, missing command, missing expected state, login/session failure without approved recovery, unapproved write path, browser failure, server failure, or any secret exposure risk.

## Files changed

- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-runtime-browser-scenario-prep-2026-06-29.md`

## Verification

This packet should be verified by readback/literal check only.

Required verification literals:

- `RUNTIME_BROWSER_SCENARIO_PREP`
- `human-readable Russian scenario prep`
- `exact clicks/typing/expected visible state`
- `no secrets captured`
- `Proof readiness items are tasks, not stop blockers`
- `Work happens on DesignToCode in a separate folder or worktree`

No tests/builds/runtime/browser/server/screenshots/git/PR/cleanup/secret verification is implied.

## Next safe followups

1. Run readback/literal verification for this file only.
2. Delegate `bounded-runtime-browser-command-discovery-worker` because exact runtime/browser commands and exact UI scenario selectors were not discovered from allowed docs.
3. After discovery PASS and Luba explicit execution authority, delegate `bounded-runtime-browser-proof-worker`.

## Class C yes-no

Class C yes-no: no.

Reason: this packet is not branch creation from absent branch/source authority. The DesignToCode branch/worktree already exists in the docs. This work is runtime/browser scenario prep only.

## Final boundary

This is a task, not a stop blocker.

This is doc-only scenario prep. It did not run runtime, browser, tests, builds, servers, screenshots, external AI, git, PR actions, cleanup, or secret checks.

The evidence supports only:

- `RUNTIME_BROWSER_SCENARIO_PREP` file created;
- allowed docs were read;
- exact runtime/browser execution selectors were not found in those docs;
- future command/scenario placeholders and discovery worker were recorded;
- `no secrets captured`.
