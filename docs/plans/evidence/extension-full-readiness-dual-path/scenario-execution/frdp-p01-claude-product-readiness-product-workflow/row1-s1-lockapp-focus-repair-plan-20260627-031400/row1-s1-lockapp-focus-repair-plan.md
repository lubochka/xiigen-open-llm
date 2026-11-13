# Row1 S1 LockApp focus repair plan

claim_scope: planning/repair packet only for Row1 S1 Claude route-discovery after LockApp/focus failure; not product execution.
evidence_scope: read the previous Row1 S1 no-send route discovery packet, focused no-send route discovery packet, route-discovery prep, and prep review. I did not visually inspect PNG crops, run desktop automation, type, send, run product code, use web/external AI, commit, push, or edit existing files.
count_claimed: 1 new repair-plan packet folder; 2 new packet files; 0 desktop automation runs; 0 prompt typing; 0 prompt sends; 0 product-readiness claims.
independent_evidence_count: 5 source evidence groups: workflow folder inventory, baseline no-send discovery packet, focused no-send discovery packet, approved prep packet, and prep-review packet.
subagent_id_or_NONE: bounded-sub-agent-planner-current-session / platform_id_not_available.
downgrade_needed: no for the narrow repair-plan verdict. Evidence supports only a next no-send route-discovery repair plan; product_ready remains false.

## Вердикт

`PASS_REPAIR_PLAN`.

Правильный следующий шаг: **A) repair Claude route discovery with a stricter multi-monitor/window-activation script**.

`product_ready=false`. Этот packet не доказывает Claude host readiness, visible answer readiness, full product readiness, token savings, semantic quality, training, checkpoint promotion или locator update.

## Человеческий анализ сбоя

Предыдущий focused no-send discovery доказал узкую вещь: процесс и окно Claude существовали, но интерактивный маршрут к Claude UI не был подтвержден.

Что видно из evidence:

- В baseline no-send packet было найдено локальное окно Claude: процесс `claude`, pid `15384`, hwnd `0x103B8`, заголовок `Claude`, окно на primary `DISPLAY2`.
- В focused packet тот же маршрут попытались уточнить безопасно: `SetForegroundWindow` не подтвердил Claude foreground; `WScript.Shell.AppActivate(15384)` вернул `true`, но foreground тоже не подтвердился как Claude.
- UI Automation видела только root window `Claude` и не нашла compose/input controls.
- `CopyFromScreen` crops были черными/blank; `PrintWindow` crops были blank/chrome-only.
- После единственного focus-only click по предполагаемой chrome/titlebar точке `730,178` foreground стал `LockApp` / `Windows Default Lock Screen`, а не Claude.
- Prompt typed: `0`; prompt sent: `0`.

Это похоже не на отсутствие Claude как программы, а на провал интерактивной desktop/session/window-activation preflight. Возможные причины, которые evidence поддерживает только как гипотезы: Windows session был на lock/secure surface; stale или непригодный foreground state сделал click небезопасным; или multi-monitor/window geometry была недостаточно строго подтверждена перед click. Evidence не поддерживает claim, что Claude route невозможен.

## Почему не B и не C сейчас

`B` не выбран: парковать Row1 Claude route и переходить к другому сценарию можно как scheduling choice, но это не лучший следующий Row1 repair step. У нас есть конкретный no-send repair path, который может остановиться безопасно до prompt typing/send.

`C` не выбран сейчас: human manual window arrangement нужен только если строгая preflight-проверка снова докажет `LockApp`/`LogonUI`, locked desktop, отсутствующее окно Claude, login/credential requirement, или невозможность privacy-safe capture. Текущий evidence еще не доказывает, что без человека невозможно.

## Exact Next Sub-Agent Work Order

Роль: bounded Row1 S1 Claude LockApp-focus no-send route-discovery repair sub-agent.

Максимум активной работы: 10 минут.

Тип задачи: planning/route-discovery repair execution only; not product execution.

Allowed write scope:

- Create one new packet folder under:
  `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-claude-product-readiness-product-workflow\row1-s1-lockapp-focus-nosend-route-discovery-YYYYMMDD-HHMMSS\`
- Write only new evidence files inside that new folder.
- Do not edit existing files.

Read scope:

- Previous Row1 S1 no-send packets and this repair-plan packet.
- No broad repo search unless needed to locate those packet files.

Implementation authorization:

- Authorized now only for the bounded no-send discovery script/run described below.
- Not authorized for product execution, prompt typing, prompt send, tests/builds/eval/training, code/governance/skill/plan edits, commit/push, web/external AI, or nested subagents.

### Required script behavior

1. Treat stale pid/hwnd from prior packets as evidence only. Re-enumerate current windows at runtime.

2. Before any focus action or mouse action, sample foreground window 3 times, 250 ms apart, using `GetForegroundWindow`, `GetWindowThreadProcessId`, process name, and title.

3. If any foreground sample is `LockApp`, `LogonUI`, title contains `Windows Default Lock Screen`, or the script cannot access an interactive foreground safely, stop immediately with `BLOCKED_LOCKAPP_PRE_ACTION`. Do not click. Do not capture Claude UI. Save only preflight JSON/action log.

4. Enumerate monitors via `System.Windows.Forms.Screen.AllScreens`. Record device name, primary flag, bounds, working area, and virtual desktop bounds.

5. Enumerate Claude candidates using Win32 `EnumWindows`, `IsWindowVisible`, `GetWindowText`, `GetWindowRect`, `DwmGetWindowAttribute` extended frame bounds if available, `GetWindowThreadProcessId`, `Get-Process`, and process path if accessible.

6. Candidate filter:

- process name equals `claude`;
- visible top-level window;
- title safe preview equals or contains generic `Claude`;
- not minimized/iconic;
- width >= 600 and height >= 400;
- not a `LockApp`, `LogonUI`, shell, or desktop window;
- do not log private title text beyond safe generic classification.

7. Select the candidate with largest visible intersection with the primary monitor working area. If a Claude window is partly offscreen or on the secondary monitor, use `ShowWindowAsync(hwnd, SW_RESTORE)` and `SetWindowPos` to place it inside primary `DISPLAY2` working area with a safe margin, for example x=80, y=80, width=min(1280, workingWidth-160), height=min(800, workingHeight-160). This is a window-placement action only; no prompt interaction.

8. Activate without mouse click first:

- call `ShowWindowAsync(hwnd, SW_RESTORE)`;
- call `BringWindowToTop(hwnd)`;
- use `AttachThreadInput` between current thread and foreground thread only for the activation attempt;
- call `SetForegroundWindow(hwnd)`;
- optionally call `WScript.Shell.AppActivate(pid)` only after exact candidate selection;
- wait 500 ms and resample foreground 3 times.

9. If foreground becomes `LockApp`/`LogonUI` at any point, stop with `BLOCKED_LOCKAPP_AFTER_ACTIVATION`. Do not click. Do not type. Do not send.

10. If foreground is not the selected Claude pid/hwnd/title after activation, stop with `BLOCKED_FOREGROUND_NOT_CLAUDE`. Do not click unless a later plan explicitly authorizes a separate click-only retry. This repair work order forbids mouse clicks because the previous click was the failure boundary.

11. Only after foreground is confirmed as Claude, run UI Automation read-only discovery:

- query root window and bounded descendants;
- record control type, automation id if safe, bounding rectangle, enabled/offscreen status;
- record only non-secret labels for route controls such as input/compose/edit, send button presence, extension/plugin entrypoint, model/status selector, or login requirement;
- do not log private chat message text, account identifiers, credentials, tokens, or conversation titles.

12. Only after foreground is confirmed as Claude, capture privacy-safe crops:

- `claude_top_status_redacted.png`: top 160 px of selected Claude bounds; hard-redact the rightmost 360 px and any account/avatar/profile area before saving.
- `claude_bottom_compose_redacted.png`: bottom 260 px of selected Claude bounds; hard-redact any region above the composer/input surface if prior chat content is visible; keep only non-secret composer/input geometry and button outlines/labels if safe.
- `claude_route_control_redacted.png`: small crop around a candidate input/extension control only if UIA/geometry identifies it and it can be saved without private/chat/account/secret content.

13. Never save a raw full-window screenshot. If a full-window capture is needed to compute crops, keep it only in memory and discard it before writing artifacts.

14. For every saved crop, write `capture_quality.json` with crop rectangle, source method (`CopyFromScreen` or `PrintWindow`), foreground confirmation state, redaction rectangles, mean luminance/variance, blank/black detection, and `route_evidence=true/false`.

15. Stop before typing, paste, Enter, Send, submit, Run, credential entry, or any model-backed response.

### Required evidence files

- `lockapp-focus-nosend-action-log.md`
- `lockapp-focus-nosend-preflight.json`
- `lockapp-focus-nosend-window-candidates.json`
- `lockapp-focus-nosend-activation-log.json`
- `lockapp-focus-nosend-uia-summary.json`
- `capture_quality.json`
- optional safe crops named exactly as above, only if privacy-safe after redaction
- final `route_discovery_packet.md`
- final `route_discovery_packet.json`

### PASS criteria

Return `PASS_ROUTE_DISCOVERY_REPAIRED` only if all are true:

- foreground preflight proves the desktop is not `LockApp`/`LogonUI`;
- exact current Claude window target is selected by runtime enumeration, not stale pid/hwnd;
- selected Claude foreground is confirmed after activation;
- route candidate is documented as either visible input/compose control or visible extension/plugin entrypoint;
- evidence proves `prompts_typed=0`, `prompts_sent=0`, `model_backed_responses_invoked=0`;
- saved crops, if any, are privacy-safe/redacted and non-blank;
- `product_readiness_claim=false` and `claude_host_readiness_claim=false`.

### NEEDS_FIX criteria

Return `NEEDS_FIX_PACKET` if:

- JSON does not parse;
- action log omits a required preflight/activation step;
- candidate selection is ambiguous;
- a crop is black/blank but still claimed as route evidence;
- redaction rectangles are missing for saved crops;
- route control is described too vaguely for a later executor to reproduce;
- the packet claims product/host readiness.

### BLOCKED criteria

Return `BLOCKED_ROUTE_UNKNOWN` or a narrower blocked code if:

- `LockApp`/`LogonUI` is foreground before or after activation;
- no Claude window is found;
- Claude requires login/credentials;
- input/extension route is not visible;
- safe evidence capture would expose secrets, account identifiers, private chat history, credentials, or raw private content;
- any continuation would require prompt typing, prompt send, credential entry, or model-backed response.

If the result is blocked specifically by `LockApp`/`LogonUI`, the next scheduler choice may be either human unlock/window arrangement or moving to another remaining scenario. That human request should happen only after this stricter no-click repair attempt records the blocked evidence.

## Verification required for this repair-plan packet

- JSON parse for `row1-s1-lockapp-focus-repair-plan.json`.
- `git diff --check -- <this packet folder>`.
- Supplemental `git diff --no-index --check -- NUL <created-md>` and `git diff --no-index --check -- NUL <created-json>` because the folder is new/untracked.

## Evidence sources used

- `row1-s1-no-send-route-discovery-20260627-030112\row1-s1-no-send-route-discovery.md`
- `row1-s1-no-send-route-discovery-20260627-030112\row1-s1-no-send-route-discovery.json`
- `row1-s1-no-send-focused-route-discovery-20260627-030621\route_discovery_packet.md`
- `row1-s1-no-send-focused-route-discovery-20260627-030621\route_discovery_packet.json`
- `row1-s1-route-discovery-prep-20260627-021451\row1-s1-route-discovery-prep.md`
- `row1-s1-route-discovery-prep-20260627-021451\row1-s1-route-discovery-prep.json`
- `row1-s1-route-discovery-prep-review-20260627-025125\row1-s1-route-discovery-prep-review.md`
- `row1-s1-route-discovery-prep-review-20260627-025125\row1-s1-route-discovery-prep-review.json`
