# Микро-шаг 04: prep для нижней полосы composer без отправки

## Назначение

Этот prep готовит следующий микро-сценарий Row21.1 после того, как micro-step 03 остановился на `ACCESSIBILITY_BOUNDED_QUERY_TIMEOUT`: широкий read-only accessibility query достиг `max_nodes=500` без подтвержденного input/composer locator. Цель будущего микро-шага - сузить поиск до зоны, где обычно находится composer, то есть нижняя полоса окна Codex, и сохранить частичные доказательства без ввода и без отправки.

Это только подготовка. В этом prep не запускались product/runtime/UI/tests, не выполнялись runtime probes, не было кликов, typing, paste, Send, Enter, clipboard, screenshot capture, attachment, browser automation, commit или push.

## Принятый контекст

- Target process/window: `Codex`
- PID: `13508`
- HWND: `0x1038E`
- Bounds: `left=-2071 top=204 width=1283 height=733`
- Derived window rectangle: `left=-2071 top=204 right=-788 bottom=937`
- Micro-step 02: shallow UIA ControlView found no composer/input.
- Micro-step 03: read-only accessibility query hit `max_nodes=500` without confirmed input/composer.
- Review recommendation: run a lower composer band bounded query before webview metadata feasibility.

## Derived lower-band region

Basis: use lower 35% of the accepted target window because there is no stronger evidence for a smaller composer band yet. This is a bounded search region, not a click target.

- Lower-band height: `ceil(733 * 0.35) = 257`
- Lower-band top: `204 + 733 - 257 = 680`
- Lower-band bottom: `937`
- Lower-band left: `-2071`
- Lower-band right: `-788`
- Lower-band width: `1283`
- Lower-band rectangle: `left=-2071 top=680 width=1283 height=257`

The future executor must query/filter elements by bounds intersecting this lower-band region. It must not rely on blind clicking, inferred coordinates, or a visual guess. Required safety literal: не использовать слепой ввод.

## Read-only method for the future executor

The future executor must run a bounded read-only accessibility query rooted at `AutomationElement.FromHandle(0x1038E)` and inspect only elements whose bounding rectangles intersect the lower-band region.

Allowed inspection:

- Win32 recheck of HWND/PID/visibility/foreground/bounds.
- UIAutomation root lookup from HWND.
- Bounded tree traversal with a strict node budget limited to lower-band candidates.
- Bounds intersection filtering before deeper property inspection where possible.
- Read-only UIA properties: process id, control type, class, automation id hash, name length/hash only, bounding rectangle, enabled, offscreen, keyboard focusable.
- Pattern availability only: `ValuePattern`, `TextPattern`, `LegacyIAccessiblePattern`, `InvokePattern` when relevant for safe identification.
- No reading actual message text or `ValuePattern.Value`.

Forbidden actions:

- не использовать слепой ввод
- не отправлять
- No typing, clicking, paste, Send, Enter, clipboard, hotkeys, focus setting, SetFocus, Invoke, ExpandCollapse action, Toggle action, Scroll action, or any UI mutation.
- No product/runtime/UI/tests.
- No Electron/CDP `Runtime.evaluate`, `Input.*`, DOM mutation, Page navigation, or script injection.
- No commit/push.

## Timing and evidence limits

- Total runtime for the future executor: stop before 8 minutes.
- Per-operation `timeout` is mandatory.
- Each sub-step must write `partial evidence` before the next sub-step starts.
- If any query approaches the time budget, stop and write blocker evidence instead of expanding scope.

Recommended future executor timeouts:

- Step 04.0 start marker: shell `timeout` 10 seconds.
- Step 04.1 target window recheck: shell `timeout` 15 seconds; Win32 read-only budget 5 seconds.
- Step 04.2 lower-band root and geometry setup: shell `timeout` 15 seconds; UIA root lookup 750 ms.
- Step 04.3 lower-band bounded candidate query: shell `timeout` 60 seconds; per property 350 ms; per navigation call 750 ms; traversal budget 35 seconds; suggested max nodes 250 after lower-band intersection filtering.
- Step 04.4 locator classification: shell `timeout` 15 seconds.
- Step 04.5 status/report write: shell `timeout` 10 seconds.

## Future executor steps

### Step 04.0 - create start marker

Write `micro-step-04-start-marker.json` with target window info, lower-band region, forbidden actions, `product_readiness_claim=false`, and the literal `не доказывает готовность продукта`.

Partial evidence: `micro-step-04-start-marker.json`.

### Step 04.1 - recheck target window

Read-only Win32 recheck:

- HWND `0x1038E` exists.
- PID is still `13508`.
- Window is visible.
- Window is foreground or still the accepted target for this bounded query.
- Bounds materially match `left=-2071 top=204 width=1283 height=733`.

Partial evidence: `micro-step-04-window-recheck.json`.

Stop with `TARGET_WINDOW_RECHECK_FAILED` if the recheck fails.

### Step 04.2 - record lower-band geometry

Compute and record the explicit lower-band rectangle:

- `left=-2071`
- `top=680`
- `right=-788`
- `bottom=937`
- `width=1283`
- `height=257`
- `basis=lower_35_percent_of_target_window`

Partial evidence: `micro-step-04-lower-band-region.json`.

### Step 04.3 - bounded lower-band accessibility candidate query

Starting only from `AutomationElement.FromHandle(0x1038E)`, query descendants and keep only elements whose bounds intersect the lower-band region. Inspect patterns/properties only. Prefer early rejection of elements outside the lower band before reading deeper metadata.

Candidate acceptance hints:

- Control type is `Edit`, `Document`, `Custom`, `Pane`, or another type only if supported patterns make it relevant.
- Bounds intersect lower band and are not whole-window sized.
- Element is enabled and not offscreen.
- Element is keyboard focusable, or supports `ValuePattern`, `TextPattern`, `LegacyIAccessiblePattern`, or safe composer-relevant metadata.
- Evidence suggests editable/input/composer semantics without reading private text.

Partial evidence: `micro-step-04-lower-band-candidates.json`.

Stop with `LOWER_BAND_QUERY_TIMEOUT` if traversal hits `timeout`, max node budget, or the 8-minute wall-clock limit before a confirmed candidate.

### Step 04.4 - classify locator without interaction

Read only the lower-band evidence files and classify. A success classification requires a confirmed input/composer locator with:

- Bounds inside or intersecting the lower-band region.
- Editable, value, text, or invoke-related evidence sufficient to identify the input/composer surface.
- Not a broad container, whole-window element, title bar, sidebar-only region, history-only pane, button-only toolbar, or send action.
- No need for blind click or blind typing to prove it.
- No send action performed.

Partial evidence: `micro-step-04-locator-classification.json`.

### Step 04.5 - write status/report

Write final micro-step evidence for the future executor:

- `micro-step-04-status.json`
- `micro-step-04-report.md`
- `micro-step-04-blocker.json` only if blocked.

The report must state whether a locator was confirmed and must include `не доказывает готовность продукта`.

## Success criteria

Success status: `INPUT_COMPOSER_CONFIRMED_IN_LOWER_BAND_PRE_SEND`.

All required:

- Target window rechecked as PID `13508`, HWND `0x1038E`, bounds `left=-2071 top=204 width=1283 height=733`.
- Lower-band region explicitly recorded as `left=-2071 top=680 width=1283 height=257`.
- Confirmed input/composer locator with editable/value/text/invoke evidence and bounds in or intersecting the lower band.
- No send, no typing, no clicking, no paste, no clipboard, no hotkeys, no UI mutation.
- `partial evidence` exists for every completed sub-step.

## Blocker criteria

Blocked status is required when any of these occurs:

- `TARGET_WINDOW_RECHECK_FAILED`
- `LOWER_BAND_ROOT_LOOKUP_FAILED`
- `LOWER_BAND_QUERY_TIMEOUT`
- `LOWER_BAND_LOCATOR_NOT_CONFIRMED`
- `ONLY_BROAD_CONTAINERS_FOUND`
- `RUNTIME_APPROACHING_8_MINUTES_WITH_PARTIAL_EVIDENCE`

Blocker evidence must name the last successful sub-step and exact evidence filenames already written.

## Exact evidence filenames for the future executor

- `micro-step-04-start-marker.json`
- `micro-step-04-window-recheck.json`
- `micro-step-04-lower-band-region.json`
- `micro-step-04-lower-band-candidates.json`
- `micro-step-04-locator-classification.json`
- `micro-step-04-blocker.json`
- `micro-step-04-status.json`
- `micro-step-04-report.md`

## Non-claims

This prep does not prove a Codex UI route pass. It does not prove input availability. It does not prove send readiness. It does not prove product readiness: не доказывает готовность продукта. It does not prove token savings, semantic quality, training, learnability, checkpoint promotion, locator update, or full extension readiness.

## Independent prep review

Independent prep review is required before execution because this prep defines the next executor work order and the safety boundary for a host UI discovery step.
