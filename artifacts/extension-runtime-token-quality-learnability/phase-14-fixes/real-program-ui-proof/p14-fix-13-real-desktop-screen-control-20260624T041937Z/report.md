# Phase 14.13 real desktop screen-control proof

Evidence root:

`C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z`

## Общий статус

Попытаны все 3 host: Claude, Cursor, Codex.

Итог по evidence:

| Host | Real app найден | Status |
|---|---:|---|
| Cursor | yes | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED |
| Claude | yes | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED |
| Codex | yes | NEEDS_FIX_EVIDENCE_INSUFFICIENT |

Full readiness не заявляется ни для одного host. Cursor и Claude получили свежие real app marker screenshots, но extension/runtime path из этих host не доказан. Codex real app/plugin surface найден и local core-boundary trace записан, но fresh marker не появился ни на одном Codex screenshot, поэтому Codex не закрыт.

## Per-host evidence

| Host | Real app found | Как запущен/активирован | Marker | Screenshot path | Extension/runtime trace path | Status | Limitation |
|---|---:|---|---|---|---|---|---|
| Cursor | yes | Запущен/активирован `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe` с marker file из evidence folder | `P14-REAL-CURSOR-20260624T041937Z` | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\raw\cursor\window-cursor-real-marker.png` | none | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED | Screenshot доказывает real Cursor window + marker, но не XIIGen extension/runtime invocation from inside Cursor. |
| Claude | yes | Активировано реальное Claude app window; marker typed into visible input; Enter не нажимался | `P14-REAL-CLAUDE-20260624T041937Z` | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\raw\claude\window-claude-real-marker-direct-keys.png` | none | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED | Screenshot доказывает visible Claude app control, но no safe local XIIGen extension/runtime surface was available; external Claude prompt was not submitted. |
| Codex | yes | Активировано реальное Codex Desktop window; пробовал conversation scroll, prompt/input paste, Plugins surface, plugin search, direct keys | `P14-REAL-CODEX-20260624T041937Z` | primary blocker screenshot: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\raw\codex\window-codex-real-marker-plugin-search-direct-keys.png` | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\traces\codex\trace.json` | NEEDS_FIX_EVIDENCE_INSUFFICIENT | Real Codex app/plugin surface visible, but marker absent. Trace is local core-boundary support only and says `host_runtime_claimed=false`. |

## Что происходило на компьютере

На desktop были активированы реальные окна приложений. Cursor был выведен на foreground и открыл local marker file. Claude был выведен на foreground; после нескольких попыток marker удалось ввести прямыми keystrokes в нижний input без отправки prompt. Codex был выведен на foreground; открывались conversation area и Plugins surface, но screen-control не смог сфокусировать поле так, чтобы fresh marker стал видимым на screenshot.

## Exact blockers and next fix

Cursor blocker: XIIGen extension/runtime path не был вызван из Cursor UI в этом run. В `.cursor\extensions` targeted discovery не нашел matching `xiigen|llm|mvp|smart` extension directory. Next fix: открыть установленную XIIGen Cursor extension/panel или явно установить/enable extension command, затем rerun с visible marker/output + trace.

Claude blocker: real Claude app marker screenshot есть, но no safe local-only XIIGen extension/runtime control surfaced in Claude UI. Я не отправлял prompt во внешний Claude. Next fix: подготовить Claude Desktop extension/MCP surface with local XIIGen command, then rerun with visible marker/output and local trace.

Codex blocker: Codex app found/activated, Codex Plugins surface visible, local installed-plugin core trace succeeded, но marker не появился в Codex UI screenshots after scroll, prompt/input paste, plugin search field, and direct-key attempts. Next fix: use a reliable Codex desktop UI automation target or in-app tool/search field that accepts text, then capture marker; optionally rerun installed plugin trace with same marker. Do not mark Codex full-ready from current markerless screenshots.

## Codex trace summary

Codex local installed-plugin smoke wrote:

- `traces\codex\request.json`
- `traces\codex\response.json`
- `traces\codex\trace.json`

Inspected result:

- `response_success=true`
- `response_status=core_boundary_partial_no_semantic_quality_claim`
- `core_boundary_executed=true`
- `runtime_extension_claimed=false`
- `host_runtime_claimed=false`
- `external_ai_used=false`
- `training_started=false`
- `semantic_quality_claimed=false`

This trace is supporting only. It does not repair the missing Codex marker screenshot.

## Plan update summary

Updated:

`C:\Projects\llm_mvp_core\docs\plans\XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md`

The plan now records p14-fix-13 as partial: Cursor/Claude real app marker screenshots present; Codex remains `CODEX_NEEDS_FIX_EVIDENCE_INSUFFICIENT`; 14.13 remains open.

## Review-safe commit evidence

Raw screenshots remain local/raw source evidence and were not overwritten. For commit/review evidence, use the review-safe copies below:

| Host | Commit-safe review-safe screenshot | Raw/local source screenshot | Redaction/crop summary |
|---|---|---|---|
| Cursor | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\review-safe\cursor-window-real-marker-review-safe.png` | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\raw\cursor\window-cursor-real-marker.png` | Cropped to Cursor app chrome/editor marker area; unrelated inactive tab masked. Cursor window identity and marker `P14-REAL-CURSOR-20260624T041937Z` remain visible. |
| Claude | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\review-safe\claude-window-real-marker-review-safe.png` | `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-20260624T041937Z\raw\claude\window-claude-real-marker-direct-keys.png` | Sidebar private items, usage/dashboard details, project/workspace chips, usage banner, and account/model footer details masked. Claude app chrome/Code tab and marker `P14-REAL-CLAUDE-20260624T041937Z` remain visible. |

Commit-safe evidence status: Cursor and Claude review-safe screenshots are the preferred review/commit references. Raw paths are retained only as local/raw evidence.

## Verification commands/results

Executed:

- Visual inspection of `review-safe\cursor-window-real-marker-review-safe.png`: PASS, marker readable and Cursor app identity remains visible.
- Visual inspection of `review-safe\claude-window-real-marker-review-safe.png`: PASS, marker readable and Claude app chrome/Code surface remains visible.
- `Get-Content report.json -Raw | ConvertFrom-Json`: PASS, `report.json valid JSON`.
- `git diff --check -- docs/plans/XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-14-codex-visible-marker-20260624T044217Z`: PASS.
- Scoped `git status --short -- ...`: plan is modified and both evidence folders are currently untracked; no staging was used to make Git diff include untracked evidence files.
- Exact text trailing-whitespace check over the edited plan/report/json files: PASS.
- Repo-wide diff check was not run for this review-safe evidence task.

## Git boundary

No `git stage`, `git add`, `git commit`, `git push`, `checkout`, `reset`, `clean`, or delete operation was used.
