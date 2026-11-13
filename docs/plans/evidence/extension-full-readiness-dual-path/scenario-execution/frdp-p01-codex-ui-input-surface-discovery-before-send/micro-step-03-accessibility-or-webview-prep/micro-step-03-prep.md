# Микро-шаг 03: prep для accessibility/webview discovery без отправки

## Назначение

Этот prep готовит следующий микро-сценарий Row21.1 после того, как micro-step 02 не подтвердил input/composer через shallow UIA ControlView. Цель следующего шага: найти composer/input другим безопасным методом до любого ввода.

Это подготовка, а не выполнение. В этом prep не запускались product/runtime/UI/tests, не выполнялись клики, ввод, paste, Send, Enter, clipboard, скриншоты или runtime probes.

## Target window from accepted previous evidence

- Process: `Codex`
- PID: `13508`
- Handle: `0x1038E`
- Title: `Codex`
- Foreground: `true`
- Visible: `true`
- Bounds: `left=-2071 top=204 width=1283 height=733`
- Previous result: micro-step 02 shallow bounded UIA ControlView visited 12 nodes, rejected 8 candidates, and found no confirmed input/composer.

## Selected method

Primary method: accessibility focus/query without sending.

Reason: it is the safer/lower-risk local method because it stays read-only, does not attach to Electron DevTools, does not evaluate webview code, and does not repeat the same shallow UIA ControlView blindly. It uses foreground/focus readback, focused accessibility element properties, bounded RawView/content predicates, and pattern metadata only. It must not read message text content.

Fallback method: bounded Electron/webview inspection without typing/sending, only if feasible and safe.

Fallback reason: Electron/webview inspection can expose DOM or accessibility metadata, but it is higher risk because it may require a debugging endpoint. It is allowed only as a read-only feasibility path when an existing local `127.0.0.1` DevTools endpoint is already advertised by the Codex process. If no safe endpoint is present, fallback stops with blocker evidence.

## Global limits

- Total wall-clock limit: stop before 8 minutes.
- Per-operation timeout must be explicit in each command.
- Required evidence contract: write `partial evidence` after every successful sub-step before starting the next one.
- Required safety rule: не использовать слепой ввод.
- Required send rule: не отправлять anything.
- Forbidden actions: typing, clicking, paste, Send, Enter, clipboard, blind input, hotkeys, `Input.*` DevTools commands, `Runtime.evaluate`, DOM mutation, JS injection, product/runtime/UI/tests, commit, push.
- Privacy rule: do not capture secrets or exact private chat/workspace/account text. Redact names/text hashes if metadata must be preserved.
- Claim boundary: this prep and the future locator discovery do not prove product readiness. Literal: не доказывает готовность продукта.

## Primary path: accessibility focus/query without sending

All commands below are for the future executor. This prep does not run them.

### Step 03.0 - create start marker

Timeout: shell command timeout 10 seconds.

Command:

```powershell
$out = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-ui-input-surface-discovery-before-send\micro-step-03-accessibility-or-webview-prep'
$target = @{
  pid = 13508
  hwnd_hex = '0x1038E'
  title = 'Codex'
  bounds = @{ left = -2071; top = 204; width = 1283; height = 733 }
}
[ordered]@{
  artifact_type = 'micro_step_03_start_marker'
  created_at = (Get-Date).ToString('o')
  method_primary = 'accessibility_focus_query_without_sending'
  target = $target
  forbidden = @('typing','clicking','paste','Send','Enter','clipboard','blind input')
  product_readiness_claim = $false
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath "$out\micro-step-03-start-marker.json" -Encoding utf8
```

Partial evidence: `micro-step-03-start-marker.json`.

### Step 03.1 - recheck target window without UI interaction

Timeout: shell command timeout 15 seconds; Win32 read operations timeout budget 5 seconds total.

Command:

```powershell
# Read-only Win32 recheck: hwnd exists, PID matches, visible, foreground, bounds.
# Write result immediately to micro-step-03-window-recheck.json.
```

Partial evidence: `micro-step-03-window-recheck.json`.

Stop if hwnd is gone, PID mismatches, window is not visible, window is not foreground, or bounds differ materially. Blocker: `TARGET_WINDOW_RECHECK_FAILED`.

### Step 03.2 - query current accessibility focus only

Timeout: shell command timeout 20 seconds; `AutomationElement.FocusedElement` timeout 750 ms; each property timeout 350 ms.

Command:

```powershell
# Use UIAutomation read-only focus query.
# Record focused element process id, control type, class, automation id hash,
# name length/hash only, bounds, IsKeyboardFocusable, IsEnabled, IsOffscreen,
# supported pattern names, and whether bounds are inside the target Codex window.
# Do not call SetFocus. Do not click. Do not read Value/Text content.
# Write result immediately to micro-step-03-accessibility-focus-baseline.json.
```

Partial evidence: `micro-step-03-accessibility-focus-baseline.json`.

Stop if the focus query times out. Blocker: `ACCESSIBILITY_FOCUS_QUERY_TIMEOUT`.

### Step 03.3 - bounded accessibility query for input-like descendants

Timeout: shell command timeout 60 seconds; root-from-hwnd timeout 750 ms; each UIA property timeout 350 ms; each child/navigation call timeout 750 ms; total traversal budget 35 seconds.

Command:

```powershell
# Starting only from AutomationElement.FromHandle(0x1038E), run a bounded RawView
# or ContentView query by predicates instead of repeating shallow ControlView:
#   ControlType in Edit, Document, Custom, Pane only when keyboard-focusable or text/value pattern exists
#   IsKeyboardFocusable = true
#   IsEnabled = true
#   IsOffscreen = false
#   BoundingRectangle inside target bounds
#   geometry in lower half or composer-sized band
# Capture pattern availability only: TextPattern, ValuePattern, LegacyIAccessiblePattern.
# Capture no message text and no ValuePattern.Value.
# Reject whole-window/root/browser/document containers unless a nested focusable text pattern confirms composer semantics.
# Write result immediately to micro-step-03-accessibility-candidates.json.
```

Partial evidence: `micro-step-03-accessibility-candidates.json`.

Stop if traversal reaches timeout or max nodes before a safe result. Blocker: `ACCESSIBILITY_BOUNDED_QUERY_TIMEOUT`.

### Step 03.4 - classify locator without interaction

Timeout: shell command timeout 15 seconds.

Command:

```powershell
# Read micro-step-03-accessibility-focus-baseline.json and
# micro-step-03-accessibility-candidates.json.
# Accept exactly one locator only if it has:
#   enabled=true
#   keyboard_focusable=true OR text/value/legacy editable pattern evidence
#   non-empty bounds fully inside hwnd 0x1038E
#   not whole-window sized
#   not title bar/caption button/sidebar/history-only
#   composer-sized geometry near expected input area
#   no need for blind click or blind typing to prove it
# Write micro-step-03-locator-classification.json.
```

Partial evidence: `micro-step-03-locator-classification.json`.

Success status is allowed only if a confirmed input/composer locator is identified with evidence and no sending occurred. Otherwise stop with `INPUT_COMPOSER_NOT_CONFIRMED_AFTER_ACCESSIBILITY_QUERY`.

## Fallback path: bounded Electron/webview inspection without sending

Use this fallback only if primary path does not confirm a locator and total runtime remains safely below 8 minutes. This fallback must not start Codex, relaunch Codex, enable remote debugging, mutate app state, type, send, click, paste, or use clipboard.

### Step 03.F1 - feasibility check for an existing local endpoint

Timeout: shell command timeout 20 seconds.

Command:

```powershell
# Read Win32_Process.CommandLine for PID 13508 and local listening TCP ports.
# Continue only if the existing process already advertises a local DevTools endpoint
# bound to 127.0.0.1 or ::1. Do not add flags, do not restart, do not attach if absent.
# Write micro-step-03-webview-feasibility.json.
```

Partial evidence: `micro-step-03-webview-feasibility.json`.

If no safe endpoint exists, stop fallback with `WEBVIEW_READONLY_ENDPOINT_NOT_AVAILABLE`.

### Step 03.F2 - read-only target metadata

Timeout: shell command timeout 20 seconds; HTTP request timeout 2 seconds each.

Command:

```powershell
# GET /json/version and /json/list from the existing local endpoint only.
# Record target titles/types/urls redacted or hashed where private.
# Do not call Runtime.evaluate, Input.*, Page.*, DOM mutation, or JavaScript.
# Write micro-step-03-webview-targets-redacted.json.
```

Partial evidence: `micro-step-03-webview-targets-redacted.json`.

### Step 03.F3 - read-only accessibility snapshot if safe

Timeout: shell command timeout 45 seconds; CDP call timeout 5 seconds.

Command:

```powershell
# If and only if an existing page/webview target is safely identified, call a read-only
# accessibility/DOM metadata method such as Accessibility.getFullAXTree.
# Do not request text values beyond role/name length/hash, editable/focusable states,
# bounds if available, and node role metadata.
# Do not call Runtime.evaluate, Input.*, DOM.set*, Page.navigate, or any mutating method.
# Write micro-step-03-webview-ax-snapshot-redacted.json.
```

Partial evidence: `micro-step-03-webview-ax-snapshot-redacted.json`.

Then classify with the same criteria as Step 03.4 and write `micro-step-03-webview-locator-classification.json`.

## Success criteria

Success status: `INPUT_SURFACE_CONFIRMED_PRE_SEND_ACCESSIBILITY_OR_WEBVIEW`.

Required evidence:

- Target window rechecked as PID `13508`, handle `0x1038E`, visible, foreground, bounds `left=-2071 top=204 width=1283 height=733`.
- Confirmed input/composer locator with exact bounds and accessibility/webview metadata.
- Locator is not a whole-window/root container, not a title bar/caption button, not sidebar/history-only.
- No typing, clicking, paste, Send, Enter, clipboard, blind input, hotkeys, product/runtime/UI/tests, commit, or push.
- Report explicitly says the evidence не доказывает готовность продукта.

## Blocker criteria

Blocked status is required when safe methods do not produce a confirmed locator:

- `TARGET_WINDOW_RECHECK_FAILED`
- `ACCESSIBILITY_FOCUS_QUERY_TIMEOUT`
- `ACCESSIBILITY_BOUNDED_QUERY_TIMEOUT`
- `INPUT_COMPOSER_NOT_CONFIRMED_AFTER_ACCESSIBILITY_QUERY`
- `WEBVIEW_READONLY_ENDPOINT_NOT_AVAILABLE`
- `WEBVIEW_READONLY_METADATA_UNSAFE`
- `INPUT_COMPOSER_NOT_CONFIRMED_AFTER_WEBVIEW_QUERY`
- `RUNTIME_APPROACHING_8_MINUTES_WITH_PARTIAL_EVIDENCE`

Each blocker must include exact evidence filenames already written and the last successful sub-step.

## Exact evidence filenames for the future executor

- `micro-step-03-start-marker.json`
- `micro-step-03-window-recheck.json`
- `micro-step-03-accessibility-focus-baseline.json`
- `micro-step-03-accessibility-candidates.json`
- `micro-step-03-locator-classification.json`
- `micro-step-03-webview-feasibility.json`
- `micro-step-03-webview-targets-redacted.json`
- `micro-step-03-webview-ax-snapshot-redacted.json`
- `micro-step-03-webview-locator-classification.json`
- `micro-step-03-blocker.json`
- `micro-step-03-status.json`
- `micro-step-03-report.md`

## Non-claims

This prep makes no Codex UI route pass claim. It makes no product readiness claim. It does not prove token savings, semantic quality, training, learnability, checkpoint promotion, locator update, or full extension readiness. It only prepares a safe next micro-scenario to identify a pre-send input/composer locator or preserve a precise blocker.
