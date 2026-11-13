# Микро-шаг 05: prep webview metadata feasibility, read-only

## Назначение

Этот prep готовит следующий микро-сценарий Row21.1 после того, как UIA/accessibility методы не дали безопасного locator и micro-step 04 lower-band UIA execution timed out and was shut down. Цель будущего микро-шага: проверить, существует ли уже безопасный read-only Electron/Chromium/WebView metadata endpoint для Codex PID/window.

Это только подготовка. В этом prep не запускались product/runtime/UI/tests, не выполнялся runtime probe, не открывались порты, не выполнялись HTTP GET, не было UIA traversal, кликов, typing, paste, Send, Enter, clipboard, commit или push.

## Точный target context

- Target process/window: `Codex`
- PID: `13508`
- HWND: `0x1038E`
- Bounds: `left=-2071 top=204 width=1283 height=733`
- Derived rectangle: `left=-2071 top=204 right=-788 bottom=937`
- Previous blockers:
  - Micro-step 02 shallow UIA ControlView did not confirm input/composer.
  - Micro-step 03 accessibility query failed/timed out and did not confirm input/composer.
  - Micro-step 04 lower-band UIA execution timed out and was shut down.
  - Previous UIA/accessibility methods failed or timed out.
- Required boundary: do not repeat UIA traversal.

## Method summary

Future executor must run webview metadata feasibility only. The method is read-only and may inspect only process/command-line metadata, safe process relationship metadata, available environment hints through read-only OS APIs, and already-listening local TCP ports. It may use `Get-CimInstance Win32_Process`, `Get-Process`, `Get-NetTCPConnection`, or `netstat -ano` for read-only discovery.

Allowed endpoint probe is only HTTP GET to an endpoint that is already present, already listening, local-only, and attributable to Codex PID/window:

- `GET http://127.0.0.1:<port>/json/version`
- `GET http://127.0.0.1:<port>/json/list`
- `GET http://[::1]:<port>/json/version`
- `GET http://[::1]:<port>/json/list`

Do not enable remote debugging. Do not restart Codex. Do not attach a debugger that mutates state. Do not create or forward ports. Do not use WebSocket CDP attach. Do not use browser automation. Do not use `Runtime.evaluate`. Exact forbidden phrase: не использовать Runtime.evaluate. Do not use `Input.*`. Exact forbidden phrase: не использовать Input.*. Do not type, click, paste, Send, Enter, hotkey, clipboard, or mutate DOM/app state. Exact send boundary: не отправлять.

## Attribution rules

A candidate endpoint is usable only when all are true:

- The listening port already existed before the future executor started probing it.
- The listener is bound to loopback only: `127.0.0.1` or `::1`.
- The owning PID is `13508`, or an attributable Codex child process whose parent chain leads to PID `13508` and whose command line/path identifies the same Codex app family.
- `/json/version` or `/json/list` returns Chromium/Electron DevTools metadata by HTTP GET without WebSocket attach.
- `/json/list` contains a target that can be safely associated with the Codex window or Codex app context without reading private text. Titles and URLs must be redacted or hashed when stored.

If the endpoint exists but is not attributable to Codex PID/window, stop with blocker evidence.

## Timing and evidence limits

- Future executor total wall-clock: stop before 8 minutes.
- Every operation must have a per-operation timeout.
- Write partial evidence after every completed sub-step before starting the next one.
- HTTP request timeout: 2 seconds per GET.
- Process metadata read timeout: 15 seconds per command.
- Port enumeration timeout: 20 seconds per command.
- Classification/report write timeout: 10 seconds.
- If the 8-minute limit approaches, stop and preserve partial evidence.

## Future executor steps

### Step 05.0 - create start marker

Write `micro-step-05-start-marker.json` with target PID/HWND/bounds, previous blockers, forbidden actions, `prep_followed=true`, `product_readiness_claim=false`, and the literal `не доказывает готовность продукта`.

Partial evidence: `micro-step-05-start-marker.json`.

### Step 05.1 - read process and command-line metadata

Read only OS metadata for PID `13508` and relevant child processes if discoverable without injection:

- process name/path
- parent process id
- command line
- creation time if available
- remote-debugging flags if already present
- environment hints only when available through safe read-only OS APIs

Do not inspect process memory. Do not inject. Do not restart. Do not change environment.

Partial evidence: `micro-step-05-process-metadata.json`.

Stop with `CODEX_PROCESS_METADATA_UNAVAILABLE` if PID `13508` cannot be read.

### Step 05.2 - enumerate already-listening local ports

Use `Get-NetTCPConnection` or `netstat -ano` to list existing listeners. Keep only loopback listeners on `127.0.0.1` or `::1` whose owning PID is PID `13508` or an attributable Codex child process.

Partial evidence: `micro-step-05-listening-ports.json`.

Stop with `WEBVIEW_METADATA_ENDPOINT_NOT_FOUND` if no safe local listener is found.

### Step 05.3 - HTTP GET metadata from existing endpoint only

For each safe candidate endpoint, run only bounded HTTP GET:

- `/json/version`
- `/json/list`

Use 2 second timeout per request. Store redacted metadata only. Redact or hash private titles, URLs, workspace paths, user/account strings, and message text if present. Do not call `/json/protocol`, WebSocket URLs, `Runtime.evaluate`, `Input.*`, `Page.*`, DOM mutation, or any CDP command.

Partial evidence:

- `micro-step-05-endpoint-version-redacted.json`
- `micro-step-05-endpoint-list-redacted.json`

Stop with `WEBVIEW_METADATA_HTTP_GET_FAILED` if the endpoint is unreachable or does not return metadata by HTTP GET.

### Step 05.4 - classify feasibility and attribution

Read only the partial evidence files. A success classification requires:

- safe endpoint found
- endpoint is local-only and already listening
- endpoint is attributable to Codex PID/window
- returned metadata proves only webview metadata feasibility
- no input, no send, no runtime evaluation, no UI mutation

Partial evidence: `micro-step-05-attribution-classification.json`.

### Step 05.5 - write status/report

Write final future-execution evidence:

- `micro-step-05-status.json`
- `micro-step-05-report.md`
- `micro-step-05-blocker.json` only if blocked

The report must explicitly say: `не доказывает готовность продукта`.

## Success criteria

Success status: `WEBVIEW_METADATA_ENDPOINT_FEASIBLE_READONLY`.

All required:

- Target PID `13508`, HWND `0x1038E`, bounds `left=-2071 top=204 width=1283 height=733` are recorded from accepted context.
- Safe already-listening local metadata endpoint is found.
- Endpoint attribution to Codex PID/window is proven through process metadata and endpoint metadata.
- Only `/json/version` and/or `/json/list` were read by HTTP GET.
- No UIA traversal was repeated.
- No `Runtime.evaluate`, no `Input.*`, no WebSocket attach, no typing/clicking/paste/send/clipboard, no DOM/app mutation.
- Partial evidence exists for every completed sub-step.

This success proves only the ability to inspect webview metadata. It does not prove prompt sending. It does not prove input/composer locator. It does not prove product readiness: не доказывает готовность продукта.

## Blocker criteria

Blocked status is required when any of these occurs:

- `CODEX_PROCESS_METADATA_UNAVAILABLE`
- `WEBVIEW_METADATA_ENDPOINT_NOT_FOUND`
- `WEBVIEW_METADATA_ENDPOINT_NOT_LOCAL_ONLY`
- `WEBVIEW_METADATA_ENDPOINT_NOT_ATTRIBUTABLE_TO_CODEX_PID_WINDOW`
- `WEBVIEW_METADATA_HTTP_GET_FAILED`
- `WEBVIEW_METADATA_UNSAFE_OR_PRIVATE_TEXT_RISK`
- `RUNTIME_APPROACHING_8_MINUTES_WITH_PARTIAL_EVIDENCE`

Blocker evidence must name the last successful sub-step and exact evidence filenames already written.

## Exact evidence filenames for the future executor

- `micro-step-05-start-marker.json`
- `micro-step-05-process-metadata.json`
- `micro-step-05-listening-ports.json`
- `micro-step-05-endpoint-version-redacted.json`
- `micro-step-05-endpoint-list-redacted.json`
- `micro-step-05-attribution-classification.json`
- `micro-step-05-blocker.json`
- `micro-step-05-status.json`
- `micro-step-05-report.md`

## Required validation phrases

- Микро-шаг 05
- webview metadata
- read-only
- не использовать Runtime.evaluate
- не использовать Input.*
- не отправлять
- не доказывает готовность продукта

## Non-claims

This prep does not execute the scenario. It does not verify any endpoint exists. It does not inspect live Codex runtime. It does not prove a UI route pass. It does not prove prompt sending. It does not prove input availability. It does not prove product readiness: не доказывает готовность продукта. It does not prove token savings, semantic quality, training, learnability, checkpoint promotion, locator update, or full extension readiness.

## Independent prep review

Independent prep review is required before execution because this prep defines the next executor work order and the safety boundary for webview metadata feasibility.
