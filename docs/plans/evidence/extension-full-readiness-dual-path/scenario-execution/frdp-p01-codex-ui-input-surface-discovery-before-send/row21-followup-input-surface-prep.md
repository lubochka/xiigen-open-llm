# Сценарий Row21.1: frdp-p01-codex-ui-input-surface-discovery-before-send

## Назначение после Row21

Этот prep создан после принятого Row21 `frdp-p01-codex-codex-chat-ui-marker`.
Row21 принят только как scoped blocker evidence: Путь A дал supporting
entrypoint/adaptor/core-boundary proof, а Путь B остановился до ввода с
`UI_INPUT_CONTROL_NOT_FOUND`. Prompt не был typed/pasted/sent, clipboard не
использовался, blind input не использовался.

Board/state не открывают переход к Row22 как product run, пока не закрыт
ближайший Codex gap: clean marker-only Codex Desktop/UI input surface или
confirmed composer/input locator before any paste/send. Поэтому следующий
сценарий подготовки: безопасно подтвердить input surface до любого ввода или
сохранить точный blocker.

## Что доказывает и что не доказывает

Сценарий доказывает только одно из двух:

- найден подтвержденный Codex composer/input locator или безопасный
  screenshot/focus rectangle до любого paste/send;
- либо сохранен точный blocker `UI_INPUT_CONTROL_NOT_FOUND` с evidence, почему
  безопасный target недоступен.

Сценарий не доказывает готовность продукта. Он также не доказывает:

- Codex UI route pass;
- visible marker in Codex UI;
- prompt sent through Codex UI;
- token savings;
- semantic quality;
- training/learnability;
- checkpoint promotion;
- locator update;
- full extension readiness.

## Done / current / remaining

Done: Rows 12-21 accepted only as scoped/narrow or scoped blocker evidence.
Total scenarios: 28. Scoped accepted: 10. Product-ready full pass: 0. Full
joined pass: 0. Remaining: 18.

Current: Row21 follow-up input surface discovery before any send. This is a
prep-only artifact. It does not execute product/runtime/UI/tests.

Remaining after this prep: independent prep review, then a separately
authorized executor may run the exact discovery steps. Row22 remains blocked
behind this Codex input-surface gap until a safe pre-send target or exact blocker
is established.

## Путь A: automated/supporting evidence

Путь A is supporting only. The next executor must not claim host readiness from
Path A. If authorized later, Path A should read back existing Row21 artifacts
instead of launching product/runtime/tests:

```powershell
$root = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-codex-chat-ui-marker'
$joined = Get-Content -LiteralPath "$root\row21-joined-comparison.json" -Raw | ConvertFrom-Json
$blocker = Get-Content -LiteralPath "$root\path-b-ui\row21-path-b-blocker.json" -Raw | ConvertFrom-Json
$route = Get-Content -LiteralPath "$root\row21-route-20260626-201314\row21-route-status.json" -Raw | ConvertFrom-Json
```

Expected Path A output filenames for the future execution packet:

- `path-a-existing-row21-readback.json`
- `path-a-existing-row21-readback.md`

Path A success means only: existing Row21 supporting boundary and blocker
metadata were parsed and carried forward without product/runtime execution.

## Путь B: local UI discovery steps

Target app/window: local Codex Desktop app window titled/process-named `Codex`.
The executor must use the real local Codex window only. It must not use helper
JSON, logs, board files, screenshots of artifacts, or handler-level proof as UI
proof.

Session/login preflight:

1. Bring the existing Codex window into view only if it can be identified by
   process/title/bounds.
2. Confirm the visible app state is usable without capturing secrets. If login,
   email, password, token, recovery code, account details, private chat content,
   or workspace secrets would be captured, redact or stop.
3. If signed-in state cannot be safely confirmed, write
   `AUTH_OR_SESSION_PRECHECK_BLOCKED` and stop.
4. If two monitors/windows overlap, capture monitor layout and window bounds,
   move no windows unless explicitly authorized, and prefer a cropped/redacted
   active-window screenshot. If overlap prevents a safe target, stop with exact
   blocker.

Strict rule: не использовать слепой ввод. Do not type, paste, send, press Enter,
use clipboard, or use keyboard shortcuts until a confirmed safe input target
exists. Clicking is allowed only on a confirmed locator or a screenshot-confirmed
safe rectangle.

Timeout and partial evidence contract for UI diagnostics:

1. Every UIA traversal, control tree query, raw tree query, window discovery,
   bounds/monitor read, screenshot capture, image crop/redaction, and focus
   readback must have an explicit per-operation timebox. Required literal:
   `таймаут на каждую UIA-операцию`.
2. After every successful sub-step, immediately write a small `partial evidence`
   artifact before starting the next sub-step. A ledger/checklist row is not
   enough; the artifact must contain the step name, timestamp, result summary,
   and file paths produced so far.
3. If any operation times out, stop with a partial report that names the last
   successful sub-step and the exact artifact paths already written. Do not keep
   probing after a UIA/screenshot timeout.
4. If total sub-agent runtime approaches eight minutes, the executor must
   `остановиться до 8 минут` with partial evidence instead of continuing toward
   the ten-minute tool timeout.
5. Diagnostic order is narrow and mandatory: enumerate candidate windows first,
   then capture bounds/monitor layout, then run `shallow UIA` only, then attempt
   raw UIA only if shallow UIA completed and wrote partial evidence. Full-tree
   unbounded traversal is forbidden.
6. These timeout rules do not weaken the safety rules: не использовать слепой
   ввод, capture no secrets, and make no product readiness claim.

Safe strategy 1: UIA-first locator discovery.

1. Enumerate top-level windows by process/title with a per-operation timeout and
   write immediate partial evidence.
2. Capture bounds/monitor layout for the candidate Codex window with a
   per-operation timeout and write immediate partial evidence.
3. Capture only redacted `shallow UIA` control data first; do not run an
   unbounded full-tree traversal.
4. Capture redacted UIA raw view tree only if shallow UIA completed, wrote
   partial evidence, and the remaining runtime is safely below eight minutes.
5. Search for a confirmed input candidate with ControlType `Edit`, `TextBox`,
   or composer-sized `Document`/text input semantics.
6. Reject any broad RootWebArea or whole-window target.
7. Require enabled/focusable state, non-zero bounds, bounds inside the visible
   Codex window, and composer-sized geometry near the expected input area.
8. Only after that, click the center of the confirmed candidate and capture
   focus readback. Do not type.

Safe strategy 2: screenshot/focus-rectangle discovery.

1. Capture a redacted screenshot that preserves only UI layout needed to locate
   the composer/input area.
2. If private content cannot be safely redacted while preserving the candidate
   target, stop with exact blocker.
3. Mark the candidate focus rectangle in a separate evidence image.
4. Confirm the rectangle is not whole-window sized, not sidebar/file-tree/chat
   history, and not a private-content area.
5. Click only the confirmed rectangle and capture focus readback or caret/focus
   visual evidence. Do not type.

Safe strategy 3: accessibility-name cross-check, only if available.

1. If UIA exposes accessible names or automation ids like composer, prompt,
   chat input, message input, or edit field, record the exact property values.
2. Cross-check the accessibility candidate against screenshot bounds.
3. Reject the locator if the property is absent, ambiguous, duplicated, or
   resolves to whole-window/broad document.

## Success criteria

Success status is `INPUT_SURFACE_CONFIRMED_PRE_SEND` only when all are true:

- Codex window is confirmed and safely visible.
- A composer/input locator or screenshot/focus rectangle is confirmed before
  paste/send.
- Evidence contains exact bounds and focus readback.
- No prompt was typed, pasted, or sent.
- Clipboard was not used.
- No secrets captured.
- The report explicitly says this does not prove product readiness.

Blocked status is valid when it preserves a точный blocker:

- `UI_INPUT_CONTROL_NOT_FOUND`
- `AUTH_OR_SESSION_PRECHECK_BLOCKED`
- `PRIVATE_CONTENT_REDACTION_UNSAFE`
- `WINDOW_OVERLAP_OR_TARGET_AMBIGUOUS`
- `FOCUS_CONFIRMATION_FAILED`

## Rejection criteria

Reject the run if any of these happen:

- blind input is used;
- prompt is typed, pasted, or sent;
- clipboard is used;
- broad RootWebArea or whole-window rectangle is accepted as input;
- screenshot keeps private chat/account/workspace content unredacted;
- locator lacks bounds, enabled/focusable state, or reproducible readback;
- evidence claims Codex UI route pass or product readiness from discovery only;
- board/state/code/governance files are edited during this execution.

## Exact evidence filenames for future execution

The future executor must create evidence under this same scenario folder using
these exact filenames:

- `path-a-existing-row21-readback.json`
- `path-a-existing-row21-readback.md`
- `path-b-input-surface-discovery-monitor-layout.json`
- `path-b-input-surface-discovery-window-focus-candidate.json`
- `path-b-input-surface-discovery-uia-control-tree-redacted.json`
- `path-b-input-surface-discovery-uia-raw-tree-redacted.json`
- `path-b-input-surface-discovery-safe-screenshot-redacted.png`
- `path-b-input-surface-discovery-focus-rectangle.png`
- `path-b-input-surface-discovery-focus-readback.json`
- `path-b-input-surface-discovery-no-blind-input-attestation.txt`
- `path-b-input-surface-discovery-video.mp4`
- `path-b-input-surface-discovery-blocker.json`
- `path-b-input-surface-discovery-report.md`
- `path-b-input-surface-discovery-status.json`

If video capture is unavailable or unsafe, the status JSON must set
`video_captured=false` and record the exact reason. Screenshot/UIA/focus
evidence remains required for success.

## Non-claims

No product execution was performed by this prep. No runtime probe, UI action,
test, build, commit, push, training, eval, checkpoint mutation, locator update,
or board/state update is claimed. This prep is not a completed cycle and not an
independent prep review.
